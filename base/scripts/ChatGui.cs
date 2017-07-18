//------------------------------------------------------------------------------
//
// ChatGui.cs
//
//------------------------------------------------------------------------------

$CHANNEL_STATUS = "STATUS";
$VERSION_STRING = "Dynamix IRC Chat 1.2.0";
$ESCAPE_SEQ = "_-_";
$IRCClient::serverList = GetIRCServerList(0);
$IRCClient::serverCount = getRecordCount($IRCClient::serverList);
$IRCClient::retries = -1;
$IRCClient::serverIndex = -1;
$IRCClient::serverport = 6667;

$AWAY_TIMEOUT = 5 * 60 * 1000;
$VERSION_FLOOD_TIMEOUT = 5.0;
$PING_FLOOD_TIMEOUT = 5.0;

// Person flags
$PERSON_SPEAKER  = 1;
$PERSON_OPERATOR = 2;
$PERSON_IGNORE   = 4;
$PERSON_AWAY     = 8;

// Channel flags
$CHANNEL_PRIVATE       = 1;
$CHANNEL_MODERATED     = 2;
$CHANNEL_INVITE        = 4;
$CHANNEL_LIMITED       = 8;
$CHANNEL_NEWMESSAGE    = 16;
$CHANNEL_IGNORE_EXTERN = 32;
$CHANNEL_SECRET        = 64;
$CHANNEL_TOPIC_LIMITED = 128;
$CHANNEL_HAS_KEY       = 256;
$CHANNEL_NEW           = 512;

//------------------------------------------------------------------------------
function JoinChatDlg::onWake(%this)
{
   if ($IRCClient::state $= IDIRC_CONNECTED)
      IRCClient::requestChannelList();
   else
      MessageBoxYesNo("Connect IRC","Connect to IRC server?","IRCClient::connect();","Canvas.popDialog(JoinChatDlg);");
}
																 
//------------------------------------------------------------------------------
function JoinChatList::onSelect(%this,%id,%text)
{
   JoinChatName.setValue(%text);
}

//------------------------------------------------------------------------------
function JoinChatDlg::join(%this)
{
   IRCClient::join(IRCClient::channelName(JoinChatName.getValue()));
	Canvas.popDialog(JoinChatDlg);
   LaunchTabView.viewTab("CHAT",ChatGui,0);
}

//------------------------------------------------------------------------------
function ChatGui::onAdd(%this)
{
   // Add the Member popup menu:
   new GuiControl(ChatMemberActionDlg) {
      profile = "GuiModelessDialogProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";

      new ShellPopupMenu(ChatMemberPopup) {
         profile = "ShellPopupProfile";
         position = "0 0";
         extent = "0 0";
         minExtent = "0 0";
         visible = "1";
         maxPopupHeight = "200";
         noButtonStyle = "1";
      };
   };
}

//------------------------------------------------------------------------------
function ChatGui::onWake(%this)
{
   Canvas.pushDialog(LaunchToolbarDlg);

	ChatTabView.addSet(1,"gui/shll_horztabbuttonB","5 5 5","50 50 0","5 5 5");

   ChatGui.awake = true;
   ChatTabView.setSelected($IRCClient::currentChannel);
	ChatGuiScroll.scrollToBottom();
}

//------------------------------------------------------------------------------
function ChatGui::setKey(%this,%ignore)
{
}

//------------------------------------------------------------------------------
function ChatTabView::onAdd(%this)
{
   %this.addTab($IRCClient::channels.getObject(0),"STATUS");
}

//------------------------------------------------------------------------------
function ChatTabView::onSelect(%this,%obj,%name)
{
	ChatTabFrame.setAltColor(%obj.private);
	%i = %obj.findMember($IRCClient::people.getObject(0));
	ChatEditChannelBtn.setVisible(%obj.getFlags(%i) & $PERSON_OPERATOR);
	ChatChannelTopic.setValue(%obj.topic);
   if (ChatGui.awake)
   {
      if ($IRCClient::currentChannel == $IRCClient::attachedChannel)
         ChatGuiMessageVector.detach();
      ChatGuiMessageVector.attach(%obj);
		$IRCClient::attachedChannel = %obj;
   }
   $IRCClient::currentChannel = %obj;
   ChatRoomMemberList_refresh(%obj);
   ChatMessageEntry.schedule(1, makeFirstResponder, true);
}

//------------------------------------------------------------------------------
function ChatTabView::openNewPane(%this)
{
	Canvas.pushDialog(JoinChatDlg);
}

//------------------------------------------------------------------------------
function ChatTabView::closeCurrentPane(%this)
{
   if ($IRCClient::currentChannel == $IRCClient::channels.getObject(0))
      LaunchTabView.closeCurrentTab();
   else
      IRCClient::part($IRCClient::currentChannel.getName());
}

//------------------------------------------------------------------------------
function JoinPublicTribeChannel(%tribe)
{
	IRCClient::join(IRCClient::channelName(%tribe) @ "_Public");
	LaunchTabView.viewTab("CHAT",ChatGui,0);
}

//------------------------------------------------------------------------------
function JoinPrivateTribeChannel(%tribe)
{
	IRCClient::join(IRCClient::channelName(%tribe) @ "_Private");
	LaunchTabView.viewTab("CHAT",ChatGui,0);
}

//------------------------------------------------------------------------------
function KeyChannelJoin()
{
	Canvas.popDialog(ChannelKeyDlg);
	IRCClient::join($IRCClient::keyChannel SPC EditChannelKey.getValue());
}

//------------------------------------------------------------------------------
function ChatGuiMessageVector::urlClickCallback(%this,%type,%url,%content)
{
	switch$(%type)
	{
		case "http":
			gotoWebPage(%url);
		case "server":
			IRCClient::onJoinGame(%content,%url);

			%url = nextToken(%url,a," ");
			%url = nextToken(%url,map,"(");
			%url = nextToken(%url,type,")");

			// set the loading gui
//          LoadingGui.map = %map;
//    		LoadingGui.missionType = %type;
//    		Canvas.setContent(LoadingGui);
//    		Canvas.repaint();
			
			JoinGame(%content);
		case "warrior":
			LaunchBrowser(%url,"Warrior");
	}
}

//------------------------------------------------------------------------------
function ChatSendText()
{
   if ($IRCClient::people.getObject(0).flags & $PERSON_AWAY)
		IRCClient::away("");
	else
	{
		if ($IRCClient::awaytimeout)
			cancel($IRCClient::awaytimeout);
		$IRCClient::awaytimeout = schedule($AWAY_TIMEOUT,0,"ChatAway_Timeout");
	}
   if ($IRCClient::currentChannel.private)
		IRCClient::send2(ChatMessageEntry.getValue(),$IRCClient::currentChannel.getName());
	else
   	IRCClient::send2(ChatMessageEntry.getValue(),"");
   ChatMessageEntry.setValue("");
}

//------------------------------------------------------------------------------
function ChatAway_Timeout()
{
	$IRCClient::awaytimeout = 0;
	IRCClient::away($pref::IRCClient::awaymsg);
}

//------------------------------------------------------------------------------
function ChatRoomMemberList::onAdd(%this)			
{
	ChatRoomMemberList.addStyle($PERSON_OPERATOR,
										 "sys_op_eye.png",			"",
										 "","","");
	ChatRoomMemberList.addStyle($PERSON_IGNORE,
										 "",								"mute_speaker.png",
										 "","","");
	ChatRoomMemberList.addStyle($PERSON_IGNORE | $PERSON_OPERATOR,
										 "sys_op_eye.png",			"mute_speaker.png",
										 "","","");
	ChatRoomMemberList.addStyle($PERSON_AWAY,
										 "",								"",
										 "128 128 128","","");
	ChatRoomMemberList.addStyle($PERSON_OPERATOR | $PERSON_AWAY,
										 "sys_op_eye.png",			"",
										 "128 128 128","","");
	ChatRoomMemberList.addStyle($PERSON_IGNORE | $PERSON_AWAY,
										 "",								"mute_speaker.png",
										 "128 128 128","","");
	ChatRoomMemberList.addStyle($PERSON_IGNORE | $PERSON_OPERATOR | $PERSON_AWAY,
										 "sys_op_eye.png",			"mute_speaker.png",
										 "128 128 128","","");
}

//------------------------------------------------------------------------------
function ChatRoomMemberList_refresh(%c)
{
	ChatRoomMemberList.clear();
   for (%i = 0; %i < %c.numMembers(); %i++)
	{
		ChatRoomMemberList.addRow(%c.getMemberId(%i),%c.getMemberNick(%i));
		ChatRoomMemberList.setRowStyle(%i,%c.getMemberId(%i).flags | %c.getFlags(%i));
	}
}

//------------------------------------------------------------------------------
function ChannelBannedList_refresh()
{
	ChannelBanList.clear();
	%j = 0;
   for (%i = 0; %i < $IRCClient::numBanned; %i++)
	{
		%p = $IRCClient::banList[%i];
		if (!$IRCClient::removeBan[%p])
		{		
			ChannelBanList.addRow(%p,IRCClient::taggedNick(%p));
			ChannelBanList.setRowStyle(%j,%p.flags);
			%j++;
		}
	}
	ChannelBanList.sort(0);
}

//------------------------------------------------------------------------------
function ChannelRemoveBan()
{
	$IRCClient::removeBan[ChannelBanList.getSelectedId()] = true;
	ChannelBannedList_refresh();
}

//------------------------------------------------------------------------------
function ChatRoomMemberList::onRightMouseDown(%this,%column,%row,%mousePos)
{
   // Open the action menu:
   ChatMemberPopup.member = %this.getRowId(%row);
	ChatMemberPopup.position = %mousePos;
	Canvas.pushDialog(ChatMemberActionDlg);
	ChatMemberPopup.forceOnAction();
}

//------------------------------------------------------------------------------
function ChatMemberActionDlg::onWake(%this)
{
	ChatMemberPopup.clear();
	%nick = IRCClient::displayNick(ChatMemberPopup.member);
	%is = $IRCClient::currentChannel.findMember(ChatMemberPopup.member);
	%im = $IRCClient::currentChannel.findMember($IRCClient::people.getObject(0));

	// if ( !ChatMember.player.isBot )

	ChatMemberPopup.add(%nick,-1);
	for (%i = 0; %i < strlen(%nick) * 1.5; %i++)
		%line = %line @ "-";
	ChatMemberPopup.add(%line,-1);

	if (ChatMemberPopup.member == $IRCClient::people.getObject(0))
	{
		if (ChatMemberPopup.member.flags & $PERSON_AWAY)
			ChatMemberPopup.add("Set Present",0);
		else
			ChatMemberPopup.add("Set Away",1);
	}
	else
	{
		if (strcmp(ChatMemberPopup.member.getName(),$IRCClient::currentChannel.getName()))
			ChatMemberPopup.add("Chat",2);
	
		if ($IRCClient::currentChannel.getFlags(%im) & $PERSON_OPERATOR)
		{
			if (strlen(ChatMemberPopup.member.nick) &&
				 !($IRCClient::currentChannel.getFlags(%is) & $PERSON_OPERATOR))
				ChatMemberPopup.add("Admin",3);
			ChatMemberPopup.add("Kick",4);
			ChatMemberPopup.add("Ban",5);
		}
		if (ChatMemberPopup.member.flags & $PERSON_IGNORE)
			ChatMemberPopup.add("Unmute",6);
		else
			ChatMemberPopup.add("Mute",6);
		for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
		{
			%c = $IRCClient::channels.getObject(%i);
			if (%c.private)
				continue;
			%cis = %c.findMember(ChatMemberPopup.member);
			%cim = %c.findMember($IRCClient::people.getObject(0));
			if (%cis < 0 &&
				 (!(%c.flags & $CHANNEL_INVITE) ||
			 	  %c.getFlags(%cim) & $PERSON_OPERATOR))
				ChatMemberPopup.add("Invite to" SPC IRCClient::displayChannel(%c.getName()),%c);
   	}
	}
}

//------------------------------------------------------------------------------
function ChatPrivate()
{
	if (ChatRoomMemberList.getSelectedId() != $IRCClient::people.getObject(0))
	{
		%c = IRCClient::findChannel(ChatRoomMemberList.getSelectedId().getName(),true);
		ChatTabView.setSelected(%c);
	}
}

//------------------------------------------------------------------------------
function ChatMemberPopup::onSelect(%this,%id,%text)
{
   switch( %id )
   {
      case 0:	// Set Back
			IRCClient::away("");
		case 1:	// Set Away
			IRCClient::away($pref::IRCClient::awaymsg);
      case 2:	// Chat
			%c = IRCClient::findChannel(ChatMemberPopup.member.getName(),true);
			ChatTabView.setSelected(%c);
      case 3:  // Admin
         IRCClient::setOperator(ChatMemberPopup.member);
      case 4:  // Kick
         IRCClient::kick(ChatMemberPopup.member,$pref::IRCClient::kickmsg); 
      case 5:  // Ban
         IRCClient::ban(ChatMemberPopup.member,true); 
			IRCClient::kick(ChatMemberPopup.member,$pref::IRCClient::banmsg);
      case 6:  // Mute/Unmute
         IRCClient::ignore(ChatMemberPopup.member,!(ChatMemberPopup.member.flags & $PERSON_IGNORE));
		default:	// Invite
			IRCClient::invite(ChatMemberPopup.member,%id);
   }
}

//------------------------------------------------------------------------------
function ChannelBanList::onAdd(%this)
{
	ChannelBanList.addStyle($PERSON_IGNORE,
									"",								"mute_speaker.png",
									"","","");
	ChannelBanList.addStyle($PERSON_AWAY,
									"",								"",
									"128 128 128","","");
	ChannelBanList.addStyle($PERSON_IGNORE | $PERSON_AWAY,
									"",								"mute_speaker.png",
									"128 128 128","","");
}

//------------------------------------------------------------------------------
function ChatGui::onClose(%key)
{
	if ($IRCClient::people.getObject(0).flags & $PERSON_AWAY)
		IRCClient::away("");
   for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
      IRCClient::part($IRCClient::channels.getObject(%i).getName());
}

//------------------------------------------------------------------------------
function ChatGui::onSleep(%this)
{
   ChatGui.awake = false;
	$IRCClient::attachedChannel = 0;

	Canvas.popDialog(LaunchToolbarDlg);
}

//------------------------------------------------------------------------------
function EditChannelOptions()
{
	%c = $IRCClient::currentChannel;
	ChannelOptionsDlg.channel = %c;
	%im = $IRCClient::currentChannel.findMember($IRCClient::people.getObject(0)); 

	EditChannelName.setValue(IRCClient::displayChannel(%c.getName()));

	IRCClient::requestBanList($IRCClient::currentChannel);
	
	EditChannelTopic.setValue(%c.topic);

	$EditChannelInvite = %c.flags & $CHANNEL_INVITE;
	ButtonChannelInvite.setActive(!%c.tribe);

	$EditChannelModerate = %c.flags & $CHANNEL_MODERATED;
	
	$EditChannelLimit = %c.flags & $CHANNEL_LIMITED;
	ButtonChannelLimit.setActive(!%c.tribe && !$EditChannelInvite);
	if ($EditChannelLimit)
	{
		EditChannelMaxMembers.setValue(%c.personLimit);
		EditChannelMaxMembers.setActive(!%c.tribe && !$EditChannelInvite);
	}
	else
		EditChannelMaxMembers.setActive(false);

	$EditChannelKey = %c.flags & $CHANNEL_HAS_KEY;
	ButtonChannelKey.setActive(!%c.tribe);
	if ($EditChannelKey)
	{
		EditChannelPassword.setValue(%c.key);
		EditChannelPassword.setActive(!%c.tribe);
	}
	else
		EditChannelPassword.setActive(false);
	
	Canvas.pushDialog(ChannelOptionsDlg);
}

//------------------------------------------------------------------------------
function ToggleChannelInvite()
{
	if ($EditChannelInvite)
	{
		ButtonChannelLimit.setActive(false);
		EditChannelMaxMembers.setActive(false);
	}
	else
	{
		ButtonChannelLimit.setActive(true);
		ToggleChannelLimit();
	}
}

//------------------------------------------------------------------------------
function ToggleChannelLimit()
{
	EditChannelMaxMembers.setActive($EditChannelLimit);
}

//------------------------------------------------------------------------------
function ToggleChannelKey()
{
	EditChannelPassword.setActive($EditChannelKey);
}

//------------------------------------------------------------------------------
function CancelChannelOptions()
{
	Canvas.popDialog(ChannelOptionsDlg);
	// remove temporarily created people
	for (%i = 0; %i < $IRCClient::numBanned; %i++)
	{
		%p = $IRCClient::banList[%i];
		if (!%p.ref)
		{
			$IRCClient::people.remove(%p);
			%p.delete();
		}
	}
}

//------------------------------------------------------------------------------
function AcceptChannelOptions()
{
	Canvas.popDialog(ChannelOptionsDlg);

	%c = ChannelOptionsDlg.channel;

	// undo bans and remove temporarily created people
	for (%i = 0; %i < $IRCClient::numBanned; %i++)
	{
		%p = $IRCClient::banList[%i];
		if ($IRCClient::removeBan[%p])
			IRCClient::ban(%p,false);
		if (!%p.ref)
		{
			$IRCClient::people.remove(%p);
			%p.delete();
		}
	}

	%t = EditChannelTopic.getValue();
	if (strcmp(%t,%c.topic))
		IRCClient::topic(%c,%t);

	if ($EditChannelInvite != (%c.flags & $CHANNEL_INVITE))
		IRCClient::setInvite(%c,$EditChannelInvite);

	if ($EditChannelModerate != (%c.flags & $CHANNEL_MODERATED))
		IRCClient::setModerate(%c,$EditChannelModerate);
	
	if ($EditChannelLimit)
	{
		%l = EditChannelMaxMembers.getValue();
		if (!(%c.flags & $CHANNEL_LIMITED) || %l != %c.personLimit)
			IRCClient::setLimit(%c,true,%l);
	}
	else
		if (%c.flags @ $CHANNEL_LIMITED)
			IRCClient::setLimit(%c,false,0);
	
	if ($EditChannelKey)
	{
		%k = EditChannelPassword.getValue();
		if (!(%c.flags & $CHANNEL_HAS_KEY) || strcmp(%k,%c.key))
			IRCClient::setKey(%c,true,%k);
	}
	else
		if (%c.flags & $CHANNEL_HAS_KEY)
			IRCClient::setKey(%c,false,"");
}

//------------------------------------------------------------------------------
function IRCClient::init()
{
   $IRCClient::tcp = new TCPObject(IRCTCP);
   
   $IRCClient::people = new SimGroup(IRCPeople);
   $IRCClient::channels = new SimGroup(IRCChannels);
	$IRCClient::connectwait = 0;
   $IRCClient::room = "";
   $IRCClient::numCensorWords = 0;

   $IRCClient::people.add(new SimObject("IRCMe")
							  {
								 real = "";
								 identity = "";								 
								 nick = "";
								 flags = 0;
								 ping = 0;
								 ref = 1;
							  });
					  
   $IRCClient::channels.add(new ChannelVector($CHANNEL_STATUS)
								{
								   topic = "IRC Status";
								   key = "";
								   flags = 0;
								   personLimit = 0;
									private = false;
									tribe = false;
								});
}

//------------------------------------------------------------------------------
function IRCClient::notify(%event)
{
	switch$(%event)
	{
		case IDIRC_CONNECTING_SOCKET:
		case IDIRC_CONNECTED:
			IRCClient::requestChannelList();
		case IDIRC_ERR_HOSTNAME:
			MessageBoxOK("Bad Hostname","Could not resolve IRC server address " @ $IRCClient::server @ ".","");
		case IDIRC_ERR_TIMEOUT:
			MessageBoxOK("No Response","Connection failed. The IRC server did not respond.","");
		case IDIRC_ERR_DROPPED:
			MessageBoxOK("Connection Dropped","You have been disconnected from IRC server " @ $IRCClient::server @ ".","");
		case IDIRC_RECONNECT:
		case IDIRC_ERR_RECONNECT:
			MessageBoxOK("No Reconnect","Unable to reconnect to IRC server " @ $IRCClient::server @ ".","");
		case IDIRC_ERR_BADCHALLENGE:
		case IDIRC_ERR_BADCHALRESP_REPLY:
		case IDIRC_CHANNEL_LIST:
			JoinChatList.clear();
			for (%i = 0; %i < $IRCClient::numChannels; %i++)
				JoinChatList.addRow(%i,IRCClient::displayChannel($IRCClient::channelNames[%i]));
			JoinChatList.sort(0);
			if (strlen(JoinChatName.getValue()))
			{
				%i = JoinChatList.findTextIndex(JoinChatName.getValue());   
				JoinChatList.scrollVisible(%i);
				JoinChatList.setSelectedById(JoinChatList.getRowId(%i));
			}
		case IDIRC_CHANNEL_HAS_KEY:
			KeyChannelName.setValue(IRCClient::displayChannel($IRCClient::keyChannel));
			Canvas.pushDialog(ChannelKeyDlg);
		case IDIRC_ADDCHANNEL:
			if (ChatTabView.tabCount() < $IRCClient::channels.getCount())
				ChatTabView.addTab($IRCClient::nextChannel,
										 IRCClient::displayChannel($IRCClient::nextChannel.getName()),
										 $IRCClient::nextChannel.private);
			if ($IRCClient::nextChannel && !$IRCClient::nextChannel.private)
			{
				ChatTabView.setSelected($IRCClient::nextChannel);
				$IRCClient::nextChannel = 0;
			}
			alxPlay(sButtonDown,0,0,0);
		case IDIRC_JOIN:
			%i = $IRCClient::currentChannel.findMember($IRCClient::people.getObject(0));
			ChatEditChannelBtn.setVisible($IRCClient::currentChannel.getFlags(%i) & $PERSON_OPERATOR);
			ChatRoomMemberList_refresh($IRCClient::currentChannel);
		case IDIRC_SORT:
			%i = $IRCClient::currentChannel.findMember($IRCClient::people.getObject(0));
			ChatEditChannelBtn.setVisible($IRCClient::currentChannel.getFlags(%i) & $PERSON_OPERATOR);
			ChatRoomMemberList_refresh($IRCClient::currentChannel);
		case IDIRC_PART:
			ChatRoomMemberList_refresh($IRCClient::currentChannel);
		case IDIRC_KICK:
			if ($IRCClient::nextChannel)
			{
				$IRCClient::attachedChannel = 0;
				ChatTabView.setSelected($IRCClient::nextChannel);
				$IRCClient::nextChannel = 0;
			}
			ChatTabView.removeTab($IRCClient::deletedChannel);
		case IDIRC_INVITED:
			MessageBoxOKCancel("Invite",
									 "You have been invited to channel " @
									 IRCClient::displayChannel($IRCClient::invitechannel) @
									 " by " @ $IRCClient::inviteperson @ ".",
									 "IRCClient::join($IRCClient::invitechannel);");
		case IDIRC_BAN_LIST:
			ChannelBannedList_refresh();
		case IDIRC_TOPIC:
			ChatChannelTopic.setValue($IRCClient::currentChannel.topic);	
		case IDIRC_DELCHANNEL:
			if ($IRCClient::nextChannel)
			{
				$IRCClient::attachedChannel = 0;
				ChatTabView.setSelected($IRCClient::nextChannel);
				$IRCClient::nextChannel = 0;
			}
			ChatTabView.removeTab($IRCClient::deletedChannel);

		default:
			echo("IRCClient: [NOTIFY] " @ %event);
	}
}

//------------------------------------------------------------------------------
function IRCClient::statusMessage(%message)
{
   $IRCClient::channels.getObject(0).pushBackLine("[STATUS] " @ %message);
}

//------------------------------------------------------------------------------
function IRCClient::connecting()
{
	$IRCClient::connectwait++;
	if ($IRCClient::connectwait > 1)
		return;

	ChatChannelPane.setTitle("CONNECTING");
	JoinChatPane.setTitle("CONNECTING");
	ChannelBanPane.setTitle("CONNECTING");
}

//------------------------------------------------------------------------------
function IRCClient::connected()
{
	if (!$IRCClient::connectwait)
		return;

	$IRCClient::connectwait--;
	if ($IRCClient::connectwait)
		return;

	ChatChannelPane.setTitle("CHAT");
	JoinChatPane.setTitle("CHOOSE CHAT CHANNEL");
	ChannelBanPane.setTitle("EDIT BAN LIST");
}

//------------------------------------------------------------------------------
function IRCClient::newMessage(%channel,%message)
{
   if (%channel == $IRCClient::channels.getObject(0) ||
	   %channel $= "")
      $IRCClient::channels.getObject(0).pushBackLine(%message);
	else
		%channel.pushBackLine(%message);
}

//------------------------------------------------------------------------------
function IRCClient::findPerson(%nick)
{
   for (%i = 0; %i < $IRCClient::people.getCount(); %i++)
   {
      %person = $IRCClient::people.getObject(%i);
      if (%person.getName() $= %nick)
	     return %person;
   }
}

//------------------------------------------------------------------------------
function IRCClient::findPerson2(%prefix,%create)
{
	// typical name
	// Yakuza|!yakuzasama@pool032-max7.ds23-ca-us.dialup.earthlink.net
	%prefix = nextToken(%prefix,nick," !");
	nextToken(%prefix,ident," ");

	if (strlen(%nick))
	{
		if (getSubStr(%nick,0,1) $= "@" || getSubStr(%nick,0,1) $= "+")
			%nick = getSubStr(%nick,1,strlen(%nick)-1);
		if (strlen(%ident) && getSubStr(%ident,0,1) $= "~")
			%ident = getSubStr(%ident,1,strlen(%ident)-1);
      
		// look 'em up
		%p = IRCClient::findPerson(%nick);
		if (%p)
		{
			if (strlen(%ident) && strcmp(%ident,%p.identity))
			{
				IRCClient::setIdentity(%p,%ident);
				IRCClient::correctNick(%p);
			}

			return %p;
		}
	  
		if (%create)
		{
			%p = new SimObject(%nick)
				{
					real = "";
					identity = "";
					nick = "";
					flags = 0;
					ping = 0;
					ref = 0;
				};
			if (strlen(%ident))
				IRCClient::setIdentity(%p,%ident);
			$IRCClient::people.add(%p);

			%c = IRCClient::findChannel(%nick);
			if (%c && %c.numMembers != 2)
			{
				IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ " has reconnected.");
				%c.addMember(%p,IRCClient::taggedNick(%p));
				%p.ref++;
				if (%c == $IRCClient::currentChannel)
					IRCClient::notify(IDIRC_JOIN);
			}

			// initiate WHO do determine username
			if (!strlen(%ident) && %p != $IRCClient::people.getObject(1) &&
				 getSubStr(%nick,strlen(%nick)-1,1) $= "^")
				IRCClient::whois(%p);

			return %p;
		}
	}
}

//------------------------------------------------------------------------------
function IRCClient::doEscapes(%string)
{
   // escape spaces
   while ((%i = strpos(%string," ")) != -1)
      %string = getSubStr(%string,0,%i) @ $ESCAPE_SEQ @ "01" @
      			 getSubStr(%string,%i+1,strlen(%string)-(%i+1));
   
   return %string;
}

//------------------------------------------------------------------------------
function IRCClient::undoEscapes(%string)
{
   %esclen = strlen($ESCAPE_SEQ);
   %offset = 0;
   %search = %string;
   while ((%i = strpos(%search,$ESCAPE_SEQ)) != -1)
   {
		%code = getSubStr(%search,%i+%esclen,2);
		switch$(%code)
		{
			case "01":
				%replace = " ";
		}

      %string = getSubStr(%string,0,%offset+%i) @ %replace @
        		          	  getSubStr(%string,%offset+%i+%esclen+2,
      	   		                  strlen(%string)-(%offset+%i+%esclen+2));
		%search = getSubStr(%string,%offset,strlen(%string)-%offset);
	}

	return %string;
}

//------------------------------------------------------------------------------
function IRCClient::setIdentity(%p,%ident)
{
	%p.identity = %ident;

	if (getSubStr(%p.getName(),strlen(%p.getName())-1,1) $= "^")
	{	
		// identity is set and user is WON-authenticated
		%i = strpos(%ident,"@");
		while ((%j = strpos(%ident,"@",%i+1)) >= 0)
			%i = %j;
		%user = IRCToByte(getSubStr(%ident,0,%i));
	
		%delimeter = $ESCAPE_SEQ @ "00";
		%delimlen = strlen(%delimeter);
		%i = strpos(%user,%delimeter);
		%name = getSubStr(%user,0,%i);
		%user = getSubStr(%user,%i+%delimlen,
								strlen(%user)-(%i+%delimlen));
		%i = strpos(%user,%delimeter);
		%tag = getSubStr(%user,0,%i);
		%append = getSubStr(%user,%i+%delimlen,1);
   	  
		if (strlen(%tag))
			if (%append)
			{
				%p.nick = %name @ %tag;
				%p.tagged = "<tribe:0>" @ %name @ "</tribe><tribe:1>" @ %tag @ "</tribe>";
			}
			else
			{
				%p.nick = %tag @ %name;
				%p.tagged = "<tribe:1>" @ %tag @ "</tribe><tribe:0>" @ %name @ "</tribe>";
			}
		else
		{
			%p.nick = %name;
			%p.tagged = "<tribe:0>" @ %name @ "</tribe>";
		}
	}
}

//------------------------------------------------------------------------------
function IRCClient::displayNick(%person)
{
   // identity is set and user is WON-authenticated
   if (strlen(%person.nick))
		return %person.nick;   
   else
      return %person.getName();
}

//------------------------------------------------------------------------------
function IRCClient::taggedNick(%person)
{
   // identity is set and user is WON-authenticated
   if (strlen(%person.nick))
		return %person.tagged;   
   else
      return %person.getName();
}

//------------------------------------------------------------------------------
function IRCClient::correctNick(%p)
{
	for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
	{
		%c = $IRCClient::channels.getObject(%i);
		if (%c.getName() $= %p.getName())
		{
			ChatTabView.setTabText(%c,IRCClient::displayNick(%p));
			%c.topic = "Private chat with" SPC IRCClient::displayNick(%p);
			if (%c == $IRCClient::currentChannel)
				IRCClient::notify(IDIRC_TOPIC);
		}

		%mi = %c.findMember(%p);
		if (%mi >= 0)
		{
			%c.setMemberNick(%mi,IRCClient::taggedNick(%p));
			%c.sort();
			if (%c == $IRCClient::currentChannel)
				IRCClient::notify(IDIRC_SORT);
		}
		else
			if (%c == $IRCClient::banChannel)
				IRCClient::notify(IDIRC_BAN_LIST);	
	}
}

//------------------------------------------------------------------------------
function IRCClient::findChannel(%name,%create)
{
   if (%name $= "")
      return $IRCClient::currentChannel;

   for (%i = 0; %i < $IRCClient::channels.getCount(); %i++)
   {
	  %c = $IRCClient::channels.getObject(%i);
      if (%c.getName() $= %name)
         return %c;
   }
   if (%create)
   {
	  %topic = "";
	  %flags = $CHANNEL_NEW;
	  %private = false;
	  for (%i = 0; %i < $IRCClient::numChannels; %i++)
	     if (%name $= $IRCClient::channelNames[%i])
		 {
		    %topic = $IRCClient::channelTopics[%i];

			break;
		 }
      
		if (strcmp(getSubStr(%name,0,1),"#") && strcmp(getSubStr(%name,0,1),"&"))
      {
         %p = IRCClient::findPerson(%name);
         %topic = "Private chat with " @ IRCClient::displayNick(%p);
         %flags = %flags | $CHANNEL_PRIVATE;
			%private = true;
		}
		%l = strlen(%name);
		%tribe = (%l > 7 && strcmp(getSubStr(%name,%l-7,7),"_Public") == 0) ||
		 	 		(%l > 8 && strcmp(getSubStr(%name,%l-8,8),"_Private") == 0);
      %c = new ChannelVector(%name)
			   {
				  topic = %topic;
				  key = "";			      
				  flags = $CHANNEL_NEW;
				  personLimit = 0;
				  private = %private;
				  tribe = %tribe;
			   };
		$IRCClient::channels.add(%c);
      if (%c.private)
      {
			%me = $IRCClient::people.getObject(0);
			%c.addMember(%me,IRCClient::taggedNick(%me));
			%me.ref++;
			%c.addMember(%p,IRCClient::taggedNick(%p));
			%p.ref++;

			$IRCClient::nextChannel = %c;
			IRCClient::notify(IDIRC_ADDCHANNEL);
		}

      return %c;   
   }
}

//------------------------------------------------------------------------------
function IRCClient::displayChannel(%channel)
{
   if (getSubStr(%channel,0,1) $= "#" || getSubStr(%channel,0,1) $= "&")
		return IRCClient::undoEscapes(getSubStr(%channel,1,strlen(%channel)-1));
   else
   	return IRCClient::displayNick(IRCClient::findPerson(%channel));
}

//------------------------------------------------------------------------------
function IRCClient::channelName(%channel)
{
   %channel = IRCClient::doEscapes(%channel);		// escape spaces
   
   if (getSubStr(%channel,0,1) $= "#")
      return %channel;
   else
      return "#" @ %channel;
}

//------------------------------------------------------------------------------
function IRCClient::reset()
{
   $IRCClient::numBanned = 0;

   // Delete all the channels except status
   $IRCClient::numChannels = 0;
   while ($IRCClient::channels.getCount() > 1)
   {
	  %channel = $IRCClient::channels.getObject(1);
	  $IRCClient::channels.remove(%channel);
	  %channel.delete();
   }
   $IRCClient::currentChannel = $IRCClient::channels.getObject(0);

   // Delete everyone except myself
   while ($IRCClient::people.getCount() > 1)
   {
	  %person = $IRCClient::people.getObject(1);
	  $IRCClient::people.remove(%person);
	  %person.delete();
   }
}

//------------------------------------------------------------------------------
function IRCClient::connect()
{
   if (strcmp($IRCClient::state,IDIRC_CONNECTING_WAITING))
	{
		IRCClient::disconnect();
      if($IRCClient::retries == -1 || $IRCClient::retries > 5)
      {
         $IRCClient::serverIndex++;
         $IRCClient::retries = 0;
         if($IRCClient::serverIndex >= $IRCClient::serverCount)
         {
            IRCClient::newMessage("","Unable to connect to IRC servers."); 
            IRCClient::notify(IDIRC_ERR_RECONNECT);
            $IRCClient::state = IDIRC_DISCONNECTED;
            return;
         }
         $IRCClient::server = getField(getRecord($IRCClient::serverList, $IRCClient::serverIndex), 0);
      }
		IRCClient::newMessage("","Connecting to " @ $IRCClient::server);
		$IRCClient::state = IDIRC_CONNECTING_SOCKET;
		IRCClient::notify(IDIRC_CONNECTING_SOCKET);
      
      $IRCClient::tcp.connect($IRCClient::server);  
	}
}

//------------------------------------------------------------------------------
function IRCClient::connect2(%address,%port)
{
   $IRCClient::server = %address @ ":" @ %port;

   IRCClient::connect();
}

//------------------------------------------------------------------------------
function IRCTCP::onConnected(%this)
{
   IRCClient::newMessage("","Established TCP/IP connection");
   %me = $IRCClient::people.getObject(0);
   IRCClient::send("CERT " @ WONLoginIRC());
   $IRCClient::state = IDIRC_CONNECTING_WAITING;         
}

//------------------------------------------------------------------------------
function IRCTCP::onDNSFailed(%this)
{
   IRCClient::newMessage("","Could not resolve address " @ $IRCClient::server @ "."); 
   $IRCClient::state = IDIRC_DISCONNECTED;
   IRCClient::notify(IDIRC_ERR_HOSTNAME);
}

//------------------------------------------------------------------------------
function IRCTCP::onConnectFailed(%this)
{
   if ($IRCClient::state $= IDIRC_CONNECTED)
   {
      IRCClient::newMessage("","You have been disconnected from " @ $IRCClient::server @ "."); 
      if (!IRCClient::reconnect())
      {
         $IRCClient::state = IDIRC_DISCONNECTED;
         IRCClient::notify(IDIRC_ERR_DROPPED);
      }
   }
   else
   {
      IRCClient::newMessage("","Connection failed. The server did not respond."); 
      IRCClient::disconnect();
      IRCClient::notify(IDIRC_ERR_TIMEOUT);
   }
}

//------------------------------------------------------------------------------
function IRCClient::reconnect()
{
   if (!$pref::IRCClient::autoreconnect)
      return (false);

   $IRCClient::retries++;
   IRCClient::newMessage("","Try " @ $IRCClient::retries @ ".  Attempting to reconnect to " @ $IRCClient::server @ "."); 
   IRCClient::notify(IDIRC_RECONNECT);
   IRCClient::connect();
   return true;
}

//------------------------------------------------------------------------------
function IRCClient::disconnect()
{
	if ($IRCClient::awaytimeout)
	{
		cancel($IRCClient::awaytimeout);
		$IRCClient::awaytimeout = 0;
	}

   $IRCClient::state = IDIRC_DISCONNECTED;

   $IRCClient::tcp.disconnect();

   $IRCClient::silentList = 0;
   $IRCClient::silentBanList = 0;

   IRCClient::reset();
}

//------------------------------------------------------------------------------
function IRCClient::send(%message)
{
   $IRCClient::tcp.send(%message @ "\r\n");
}

//------------------------------------------------------------------------------
function IRCTCP::onLine(%this,%line)
{
   // HACK:  Windows 2000 bug.  We shouldn't need to do this!
   if ($IRCClient::state $= IDIRC_CONNECTING_SOCKET)
      IRCTCP::onConnected(%this);
   
   IRCClient::processLine(%line);
}

//------------------------------------------------------------------------------
function IRCClient::processLine(%line)
{
   // RFC_1459: Message Packet format
   //
   // <message>  ::= [':' <prefix> <SPACE> ] <command> <params> <crlf>
   // <prefix>   ::= <servername> | <nick> [ '!' <user> ] [ '@' <host> ]
   // <command>  ::= <letter> { <letter> } | <number> <number> <number>
   // <SPACE>    ::= ' ' { ' ' }
   // <params>   ::= <SPACE> [ ':' <trailing> | <middle> <params> ]
   //
   // <middle>   ::= <Any *non-empty* sequence of octets not including SPACE
   //                or NUL or CR or LF, the first of which may not be ':'>
   // <trailing> ::= <Any, possibly *empty*, sequence of octets not including
   //                   NUL or CR or LF>
   // <crlf>     ::= CR LF

   %src = %line;
   if($pref::enableBadWordFilter)
      %src = filterString(%src);

   if (strlen(%src))
   {
      // check for prefix
      if (getSubStr(%src,0,1) $= ":")
	     %src = nextToken(getSubStr(%src,1,strlen(%src)-1),prefix," ");

	  // this is the command
	  %src = nextToken(%src,command," :");

	  // followed by its params
	  %src = nextToken(%src,params,"");

	  if (!IRCClient::dispatch(%prefix,%command,%params))
     {
	     echo("IRCClient: " @ %command @ " not handled by dispatch!");
        echo("(cmd:) " @ %prefix @ " " @ %command @ " " @ %params);
     }
   }
}

//------------------------------------------------------------------------------
function IRCClient::dispatch(%prefix,%command,%params)
{
   switch$(%command)
   {
      case "PING":
	     IRCClient::onPing(%prefix,%params);
	  case "PONG":
		 IRCClient::onPong(%prefix,%params);
	  case "PRIVMSG":
		 IRCClient::onPrivMsg(%prefix,%params);
	  case "JOIN":
		 IRCClient::onJoin(%prefix,%params);
	  case "NICK":
		 IRCClient::onNick(%prefix,%params);
	  case "QUIT":
		 IRCClient::onQuit(%prefix,%params);
	  case "ERROR":
		 IRCClient::onError(%prefix,%params);
	  case "TOPIC":
		 IRCClient::onTopic(%prefix,%params);
	  case "PART":
		 IRCClient::onPart(%prefix,%params);
	  case "KICK":
		 IRCClient::onKick(%prefix,%params);
	  case "MODE":
		 IRCClient::onMode(%prefix,%params);
	  case "AWAY":
		 IRCClient::onAway(%prefix,%params);
	  case "NOTICE":
		 IRCClient::onNotice(%prefix,%params);
	  case "VERSION":
		 IRCClient::onVersion(%prefix,%params);
	  case "ACTION":
		 IRCClient::onAction(%prefix,%params);
	  case "INVITE":
		 IRCClient::onInvite(%prefix,%params);

	  case "301":
		 IRCClient::onAwayReply(%prefix,%params);
	  case "305":
		 IRCClient::onUnAwayReply(%prefix,%params);
	  case "306":
		 IRCClient::onNowAwayReply(%prefix,%params);
	  case "311":
		 IRCClient::onWhoisUserReply(%prefix,%params);
	  case "312":
		 IRCClient::onWhoisReply(%prefix,%params);
	  case "318":
		 IRCClient::onWhoisReply(%prefix,%params);
	  case "319":
		 IRCClient::onWhoisReply(%prefix,%params);
	  case "315":
		 IRCClient::onEndOfWho(%prefix,%params);
	  case "317":
		 IRCClient::onWhoisIdle(%prefix,%params);
	  
	  case "322":
		 IRCClient::onList(%prefix,%params);
	  case "323":
		 IRCClient::onListEnd(%prefix,%params);
	  case "324":
		 IRCClient::onModeReply(%prefix,%params);
	  case "331":
		 IRCClient::onNoTopic(%prefix,%params);
	  case "332":
		 IRCClient::onTopic(%prefix,%params);

	  case "341":
		 IRCClient::onInviteReply(%prefix,%params);

	  case "352":
		 IRCClient::onWhoReply(%prefix,%params);
	  case "353":
		 IRCClient::onNameReply(%prefix,%params);
	  case "367":
		 IRCClient::onBanList(%prefix,%params);
	  case "368":
		 IRCClient::onBanListEnd(%prefix,%params);
	  case "372":
		 IRCClient::onMOTD(%prefix,%params);
	  case "376":
		 IRCClient::onMOTDEnd(%prefix,%params);
	  case "401":
		 IRCClient::onNoSuchNick(%prefix,%params);
	  case "422":
		 IRCClient::onMOTDEnd(%prefix,%params);
	  case "433":
		 IRCClient::onBadNick(%prefix,%params);
	  case "444":
		 IRCClient::onNoLogin(%prefix,%params);
	  case "465":
		 IRCClient::onServerBanned(%prefix,%params);
	  case "468":
		 IRCClient::onInvalidNick(%prefix,%params);
	  case "471":
		 IRCClient::onChannelFull(%prefix,%params);
	  case "473":
		 IRCClient::onChannelInviteOnly(%prefix,%params);
	  case "474":
		 IRCClient::onChannelBanned(%prefix,%params);
	  case "475":
		 IRCClient::onBadChannelKey(%prefix,%params);

	  case "CHALLENGE":
		 IRCClient::onChallenge(%prefix,%params);
	  case "CHALRESP_REPLY":
		 IRCClient::onChalRespReply(%prefix,%params);
     case "DBMSG":
       HandleDatabaseProxyResponse(%prefix, %params);
	  default:
	     return false;
   }

   return true;
}

//------------------------------------------------------------------------------
function IRCClient::onPing(%prefix,%params)
{
   %time = getSimTime();

   if ((%time - $IRCClient::lastPinged) >= $PING_FLOOD_TIMEOUT)
   {
      $IRCClient::lastPinged = %time;

	  if (strlen(%prefix))
	  {
	     nextToken(%prefix,nick,"!");
		 %p = IRCClient::findPerson(%nick);

		 if (%p)
		    if (%p.ping)
			{
			   IRCClient::newMessage($IRCClient::currentChannel,
			   						 IRCClient::taggedNick(%p) @ " roundtrip delay: " @ (%time-%p.ping)/1000 @ " seconds.");
			   %p.ping = 0;
			}
			else
			{
			   %params = nextToken(%params,nick," :");
			   nextToken(%params,key," \x01");
			   IRCClient::send("NOTICE " @ %nick @ " :\x01PING " @ %key);
			}
	  }
	  else
	     IRCClient::send("PONG " @ %params);
   }
}

//------------------------------------------------------------------------------
function IRCClient::onPong(%prefix,%params)
{
   nextToken(%params,nick," ");
   %p = IRCClient::findPerson(%nick);
   if (%p && %p.ping)
   {
      IRCClient::newMessage($IRCClient::currentChannel,
      						IRCClient::taggedNick(%p) @ " roundtrip delay: " @ (getSimTime()-%p.ping)/1000 @ " seconds.");
	  %p.ping = 0;
   }
}

//------------------------------------------------------------------------------
function IRCClient::onJoin(%prefix,%params)
{
   %p = IRCClient::findPerson2(%prefix,true);
   %c = IRCClient::findChannel(%params,true);

   if (%c.addMember(%p,IRCClient::taggedNick(%p)))
   {
		%p.ref++;
		IRCClient::notify(IDIRC_JOIN);
   }
   if ($pref::IRCClient::showjoinleave && !(%p.flags & $PERSON_IGNORE))
      IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ " has joined the conversation.");

   // if this is me then set this as the current channel
   if (%p == $IRCClient::people.getObject(0))
   {
   	$IRCClient::nextChannel = %c;
		IRCClient::notify(IDIRC_ADDCHANNEL);      

		IRCClient::send("MODE " @ %c.getName());
   }
	IRCClient::connected();
}

//------------------------------------------------------------------------------
function IRCClient::onPrivMsg(%prefix,%params)
{
   %p = $IRCClient::people.getObject(0);
   %params = nextToken(%params,ch," ");
   %msg = %params;
   
   // messages always lead with a ':'
   if (getSubStr(%msg,0,1) $= ":")
      %msg = getSubStr(%msg,1,strlen(%msg)-1);

   %nick = %ch;
   if (getSubStr(%ch,0,1) $= "@" || getSubStr(%ch,0,1) $= "+")
      %nick = getSubStr(%nick,1,strlen(%nick)-1);
   if ( %nick $= %p.getName())
      nextToken(%prefix,ch," !");
   
   if (strcmp(getSubStr(%msg,0,1),"\x01"))
   {
		// are we IGNORING this person?
   	%p = IRCClient::findPerson2(%prefix,true);
		if (%p && (%p.flags & $PERSON_IGNORE))
      	return;
      
      %c = IRCClient::findChannel(%ch, true);
      if ( !%c )
         return;
      IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ ": " @ %msg);
   }
   else
   {
      // otherwise it's a command imbeded inside PRIVMSG (oh great!)
      %msg = nextToken(%msg,command," \x01");
      IRCClient::dispatch(%prefix,%command,%ch @ ": " @ %msg);
   }
}

//------------------------------------------------------------------------------
function IRCClient::onNick(%prefix,%params)
{
   %person = IRCClient::findPerson2(%prefix,false);
   
   if (%person)
   { 
      if (!(%person.flags & $PERSON_IGNORE))
         IRCClient::newMessage($IRCClient::currentChannel,%person.getName() @ " is now known as " @ %params @ ".");

      %channel = IRCClient::findChannel(%person.getName());
      
      if (%channel)
	     %channel.setName(%params); 

      %person.setName(%params);

      // If this is me, re-set the console variable
      if (%person == $IRCClient::people.getObject(0))
         $IRCClient::NickName = %person.getName();
   }
}

//------------------------------------------------------------------------------
function IRCClient::onQuit(%prefix,%params)
{
   %p = IRCClient::findPerson2(%prefix,false);

   if (%p)
   {
      // For every channel
      for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
		{
			%c = $IRCClient::channels.getObject(%i);
			if (%c.removeMember(%p))
			{
				if (!(%p.flags & $PERSON_IGNORE))
					IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ " has disconnected from IRC.");
				if (%c == $IRCClient::currentChannel)
					IRCClient::notify(IDIRC_PART);
			}
		}

		// clean up the Person
		$IRCClient::people.remove(%p);
		%p.delete();
   }
}

//------------------------------------------------------------------------------
function IRCClient::onError(%prefix,%params)
{
   IRCClient::newMessage($IRCClient::currentChannel,%params);
   IRCClient::notify(IDIRC_ERROR);
   IRCClient::disconnect();
}

//------------------------------------------------------------------------------
function IRCClient::onMOTD(%prefix,%params)
{  // command 372   
   // EXAMPLE :StLouis.MO.US.UnderNet.org 372 homer128 :- ==> Disclaimer/ Rules:
   %msg = nextToken(%params,prefix,":");
   IRCClient::newMessage($IRCClient::currentChannel,%msg);
}

//------------------------------------------------------------------------------
function IRCClient::onMOTDEnd(%prefix, %params)
{  // command 372   
   // EXAMPLE :StLouis.MO.US.UnderNet.org 372 homer128 :- ==> Disclaimer/ Rules:
   if ($IRCClient::state $= IDIRC_CONNECTING_WAITING)
   {
      IRCClient::newMessage("","Successfully connected to " @ $IRCClient::server @ ":" @ $IRCClient::serverport);
      $IRCClient::state = IDIRC_CONNECTED;
      IRCClient::notify(IDIRC_CONNECTED);

      // auto join a room if requested
      if (strlen($IRCClient::room))
         IRCClient::send("JOIN " @ $IRCClient::room);
   }
}

//------------------------------------------------------------------------------
function IRCClient::onNoSuchNick(%prefix,%params)
{
	%params = nextToken(%params,me," ");
	nextToken(%params,nick," :");

	%c = IRCClient::findChannel(%nick);
	if (%c && %c.private)
	{
		%p = IRCClient::findPerson(%nick);
		if (%p)
		{
			if (!(%p.flags & $PERSON_IGNORE))
				IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ " has disconnected from IRC.");
			%c.removeMember(%p);
			%p.delete();
			if (%c == $IRCClient::currentChannel)
				IRCClient::notify(IDIRC_PART);
		}
	}
}

//------------------------------------------------------------------------------
function IRCClient::onNameReply(%prefix,%params)
{  // command 353   
   // EXAMPLE homer128 = #Starsiege :homer128 Fusion^WP KM-UnDead Rick-wrk Lord-Star Apoc0410
   %params = nextToken(%params,nick," =@");
   %me = IRCClient::findPerson(%nick);
   if (!%me)
   {
      echo("IRCClient::onNameReply: My nick should have existed.");

	  return;
   }

   // find the end of channel name
   %params = nextToken(%params,ch," :");
   %c = IRCClient::findChannel(%ch);
   if (!%c)
   {
      echo("IRCClient::onNameReply: This channel should have existed.");

   	  return;
   }

   // loop through the nick and add them to the channel
   while (strlen(%params))
   {
      %params = nextToken(%params,nick," ");
      %p = IRCClient::findPerson2(%nick,true);

      %flags = 0;
      if (getSubStr(%nick,0,1) $= "@")
         %flags = %flags | $PERSON_OPERATOR;
      else if (getSubStr(%nick,0,1) $= "+")
         %flags = %flags | $PERSON_SPEAKER;

      // If it's not me, add them to the channel
      if (%p != %me)
	  	{
      	%c.addMember(%p,IRCClient::taggedNick(%p),%flags);
			%p.ref++;
	  	}
      // If it is me, just set the flags
      else
      {
         %i = %c.findMember(%p);
         if (%i != -1)
            %c.setFlags(%i,%flags,true);
      }
   }

   IRCClient::notify(IDIRC_JOIN);
}

//------------------------------------------------------------------------------
function IRCClient::onWhoReply(%prefix, %params)
{
   // command 352   
   // EXAMPLE: :StLouis.MO.US.UnderNet.org 352 homer128 #Starsiege ~DrAwkward 198.74.39.31 los-angeles.ca.us.undernet.org DrAwkward H :3 JR
   %params = nextToken(%params,me," ");
   %params = nextToken(%params,ch," ");
   %params = nextToken(%params,user," ");
   %params = nextToken(%params,at," ");
   %params = nextToken(%params,server," ");
   %params = nextToken(%params,nick," *+@");
   %params = nextToken(%params,HG," ");
   %params = nextToken(%params,hops," ");
   nextToken(%params,real," ");

   %p = IRCClient::findPerson(%nick);
   if (!%p) return;

	if (getSubStr(%user,0,1) $= "~")
		%user = getSubStr(%user,1,strlen(%user)-1);

   // update person in question
   if (strcmp(%p.identity,%user @ "@" @ %at))
	{
   	IRCClient::setIdentity(%p,%user @ "@" @ %at);
   	IRCClient::correctNick(%p);
   }
   %p.real = %real;

   // Send it to the status channel
   IRCClient::newMessage("","WHO " @ IRCClient::taggedNick(%p) @ ": " @ %user @ "@" @ %at @ " (" @ %real @ ") connected to server " @ %server @ ".");
	IRCClient::connected();
}

//------------------------------------------------------------------------------
function IRCClient::onEndOfWho(%prefix,%params)
{
   IRCClient::notify(IDIRC_END_OF_WHO);
   // IRCClient::newMessage("","End of WHO list.");
}

//------------------------------------------------------------------------------
function IRCClient::onPart(%prefix,%params)
{
   %p = IRCClient::findPerson2(%prefix,false);
   %c = IRCClient::findChannel(%params);

   if (%p && %c)
      if (%c.removeMember(%p))
      {
         if ($pref::IRCClient::showjoinleave && !(%p.flags & $PERSON_IGNORE))
		   	IRCClient::newMessage(%c,IRCClient::taggedNick(%p) @ " has left the conversation.");
	     	IRCClient::notify(IDIRC_PART);
		 	%p.ref--;
		 	if (%p.ref == 0)
		 	{
		    	$IRCClient::people.remove(%p);
				%p.delete();
		 	}
		}

	// if this was me parting, clean up the channel
   if (%p == $IRCClient::people.getObject(0) && %c)
   {
		$IRCClient::channels.remove(%c);
		for (%i = 0; %i < %c.numMembers(); %i++)
		{
			%m = %c.getMemberId(%i);
			%m.ref--;
			if (%m.ref == 0)
			{
				$IRCClient::people.remove(%m);
				%m.delete();
			}
		}
		%c.delete();
		$IRCClient::deletedChannel = %c;
		if ($IRCClient::currentChannel == %c)
			$IRCClient::nextChannel = $IRCClient::channels.getObject(0);
		IRCClient::notify(IDIRC_DELCHANNEL);
   }
}

//------------------------------------------------------------------------------
function IRCClient::onNoTopic(%prefix,%params)
{
   %params = nextToken(%params,channel," ");
   %params = nextToken(%params,channel," ");

   // Just a message
   IRCClient::newMessage($IRCClient::currentChannel,%channel @ ": No topic is set.");
}

//------------------------------------------------------------------------------
function IRCClient::onTopic(%prefix,%params)
{
   %params = nextToken(%params,ch," :");
   %c = IRCClient::findChannel(%ch);
   if (!%c)
   {
      %params = nextToken(%params,ch," :");
      %c = IRCCLient::findChannel(%ch);
   }

   if (!%c)
      return;
   %c.topic = %params;
	if (%c == $IRCClient::currentChannel)
		IRCClient::notify(IDIRC_TOPIC);
}

//------------------------------------------------------------------------------
function IRCClient::onKick(%prefix,%params)
{
   // EXAMPLE: BarbieGrl!Forgot@tsbrk3-204.gate.net KICK #HackPhreak RiseR :0,1 Shitlisted 
   nextToken(%prefix,host," !");
   %params = nextToken(%params,ch," ");
   %params = nextToken(%params,nick," :");

   %h = IRCClient::findPerson(%host);
   if (%h)
      %host = IRCClient::taggedNick(%h);
   %c = IRCClient::findChannel(%ch);
   %p = IRCClient::findPerson(%nick);
   
   if (%p && %c)
      if (%c.removeMember(%p))
	  {
         // was it me?
         if (%p == $IRCClient::people.getObject(0))
         {
            // Delete the channel
            for (%i = 0; %i < $IRCClient::channels.getCount(); %i++)
			   if ($IRCClient::channels.getObject(%i) == %c)
			   {
			      $IRCClient::channels.remove(%c);
				  %c.delete();
				  $IRCClient::deletedChannel = %c;
				  if ($IRCClient::currentChannel == %c)
					 $IRCClient::nextChannel = $IRCClient::channels.getObject(0);

				  break;
			   }

            IRCClient::statusMessage("Host " @ %host @ " kicks " @ IRCClient::taggedNick(%p) @ " out of the chat room, saying \"" @ %params @ "\"");
            IRCClient::notify(IDIRC_KICK);
         }
         else
         {
            IRCClient::newMessage($IRCClient::currentChannel,"Host " @ %host @ " kicks " @ IRCClient::taggedNick(%p) @ " out of the chat room, saying \"" @ %params @ "\"");
	  	 	IRCClient::notify(IDIRC_PART);
		 }
	  	 
	  	 %p.ref--;
		 if (%p.ref == 0)
		 {
		    $IRCClient::people.remove(%p);
			%p.delete();
		 }
	  }
}

//------------------------------------------------------------------------------
function IRCClient::onModeReply(%prefix,%params)
{
   // Strip the person name
   %params = nextToken(%params,person," ");

   // Send the rest on to the usual handler
   IRCClient::onMode(%prefix,%params);
}

//------------------------------------------------------------------------------
function IRCClient::onMode(%prefix,%params)
{
   // EXAMPLE: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +v homer128
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +m
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege -m
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +lo 50 nick
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +l 500
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege -l
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +s
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege -s
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege +p
   // EXAMPLE: PACKET: :RickO!ricko@rick-266.dynamix.com MODE #starsiege -p

   IRCClient::newMessage("",%params);
   %params = nextToken(%params,ch," ");
   %params = nextToken(%params,mode," ");

   %enable = (getSubStr(%mode,0,1) $= "+");
   %c = IRCClient::findChannel(%ch);
   if (!%c) return;

   for (%i = 1; %i < strlen(%mode); %i++)
      switch$(getSubStr(%mode,%i,1))
      {
         // PERSON Mode command
         case "o":   // Operator
            %params = nextToken(%params,arg," ");
            %i = (strlen(%arg) ? %c.findMember(IRCClient::findPerson(%arg)) : -1);
            if (%i == -1)
               break;
            %c.setFlags(%i,$PERSON_OPERATOR,%enable);
            %c.sort();
				if (%c == $IRCClient::currentChannel)
					IRCClient::notify(IDIRC_SORT);

            // only display the message if my privilages are modified.
            if (strcmp(%arg,$IRCClient::people.getObject(0).getName()))
               break;

            nextToken(%prefix,arg,"!");
            if (%c.getFlags(%i) & $PERSON_OPERATOR)
			   IRCClient::newMessage(%c,%arg @ " made you an operator.");
			else
			   if (%c.getFlags(%i) & $PERSON_SPEAKER)
			      IRCClient::newMessage(%c,%arg @ " made you a speaker.");
			   else
			      IRCClient::newMessage(%c,%arg @ " made you an spectator.");

         case "v":   // Speaker (voice)
            %params = nextToken(%params,arg," ");
            %i = (strlen(%arg) ? %c.findMember(IRCClient::findPerson(%arg)) : -1);
            if (%i == -1)
               break;
            %c.setFlags(%i,$PERSON_SPEAKER,%enable);
				%c.sort();
				if (%c == $IRCClient::currentChannel)
					IRCClient::notify(IDIRC_SORT);

            // only display the message if my privilages are modified.
            if (strcmp(%arg,$IRCClient::people.getObject(0).getName()))
               break;

            nextToken(%prefix,arg,"!");
            if (%c.getFlags(%i) & $PERSON_OPERATOR)
			   IRCClient::newMessage(%c,%arg @ " made you an operator.");
			else
			   if (%c.getFlags(%i) & $PERSON_SPEAKER)
			      IRCClient::newMessage(%c,%arg @ " made you a speaker.");
			   else
			      IRCClient::newMessage(%c,%arg @ " made you a spectator.");

         // CHANNEL Mode command
         case "b":   // Ban
            %params = nextToken(%params,arg," ");

         case "i":   // Channel is Invite only
            if (%enable)
			   %c.flags = %c.flags | $CHANNEL_INVITE;
			else
			   %c.flags = %c.flags & ~$CHANNEL_INVITE;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "l":   // Channel has limited members
            %params = nextToken(%params,arg," ");
            %c.personLimit = (%enable ? %arg : 0);
			if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_LIMITED;
			else
			   %c.flags = %c.flags & ~$CHANNEL_LIMITED;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "m":   // Channel is moderated
            if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_MODERATED;
			else
			   %c.flags = %c.flags & ~$CHANNEL_MODERATED;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "n":   // External messages are ignored
			if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_IGNORE_EXTERN;
			else
			   %c.flags = %c.flags & ~$CHANNEL_IGNORE_EXTERN;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "p":   // Channel is Private
            if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_PRIVATE;
			else
			   %c.flags = %c.flags & ~$CHANNEL_PRIVATE;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "s":   // Channel is Secret
            if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_SECRET;
			else
			   %c.flags = %c.flags & ~$CHANNEL_SECRET;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "t":   // Channel topic limited to Operators
            if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_TOPIC_LIMITED;
			else
			   %c.flags = %c.flags & ~$CHANNEL_TOPIC_LIMITED;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);

         case "k":   // Channel has secret key
            %params = nextToken(%params,arg," ");
            %c.key = (%enable ? %arg : "");
            if (%enable)            
			   %c.flags = %c.flags | $CHANNEL_HAS_KEY;
			else
			   %c.flags = %c.flags & ~$CHANNEL_HAS_KEY;
            IRCClient::notify(IDIRC_CHANNEL_FLAGS);
      }
}

//------------------------------------------------------------------------------
function IRCClient::onWhoisReply(%prefix,%params)
{
   // EXAMPLE :rick-266.dynamix.com 311 homer128 RickO ricko rick-266.dynamix.com 198.74.38.222 :Rick
   // EXAMPLE :rick-266.dynamix.com 319 homer128 RickO :@#starsiege 
   // EXAMPLE :rick-266.dynamix.com 312 homer128 RickO rick-266.dynamix.com :WIRCSRV Windows IRC Server
   // $params = nextToken($params,ch," ");
   // IRCClient::newMessage("",%params);
}

//------------------------------------------------------------------------------
function IRCClient::onWhoisUserReply(%prefix,%params)
{
   %params = nextToken(%params,chunk," ");
   %tmp = %params;
   %params = nextToken(%params,nick," ");
   %params = nextToken(%params,user," ");
   %params = nextToken(%params,at," ");
   %real = nextToken(%params,chunk,":");

   %p = IRCClient::findPerson(%nick);
	if (!%p) return;
	
	if (getSubStr(%user,0,1) $= "~")
		%user = getSubStr(%user,1,strlen(%user)-1);

   // update person in question
   if (strcmp(%p.identity,%user @ "@" @ %at))
	{
   	IRCClient::setIdentity(%p,%user @ "@" @ %at);
   	IRCClient::correctNick(%p);
   }
   %p.real = %real;

	// Send it to the status channel
   IRCClient::newMessage("","WHOIS " @ IRCClient::taggedNick(%p) @ ": " @ %user @ "@" @ %at @ " (" @ %real @ ").");
	IRCClient::connected();
}

//------------------------------------------------------------------------------
function IRCClient::onWhoisIdle(%prefix,%params)
{
   // EXAMPLE :rick-266.dynamix.com 317 homer128 RickO 5350 907453369 :seconds idle
   // the date is encoded in '907453369' but I have not figured out how to decode it :(
   %params = nextToken(%params,nick," "); // strip out caller's name
   %params = nextToken(%params,nick," ");
   %params = nextToken(%params,seconds," ");
   nextToken(%params,date," ");

   %p = IRCClient::findPerson(%nick);

   %iMinutes = %seconds/60;
   %iSeconds = %seconds%60;
   if (%iMinutes)
      IRCClient::newMessage("",IRCClient::taggedNick(%p) @ " has been idle for " @ %iMinutes @ " minute(s) and " @ %iSeconds @ " second(s).");
   else
      IRCCLient::newMessage("",IRCClient::taggedNick(%p) @ " has been idle for " @ %iSeconds @ "second(s).");
}

//------------------------------------------------------------------------------
function IRCClient::censor(%str)
{
   if (!strlen(%str))
      return false;
   if (!$IRCClient::numCensorWords)
      if ($IRCClient::censorChannel)
      {
         %buffer = %str;
         %p = strlwr(%buffer);
         while (strlen(%p))
         {
            %p = nextToken(%p,word," ,");
            $IRCClient::censorWords[$IRCClient::numCensorWords] = %word;
         	$IRCClient::numCensorWords++;
         }
      }

   %buffer = %str;
   strlwr(%buffer);

   for (%i = 0; %i < $IRCClient::numCensorWords; %i++)
      if (strstr(%buffer,$IRCClient::censorWords[%i]))
         return true;

   return false;
}

//------------------------------------------------------------------------------
function IRCClient::onList(%prefix,%params)
{
   //EXAMPLE: :StLouis.MO.US.UnderNet.org 322 homer128 #bmx 9 :BMX Rules!

   %params = nextToken(%params,nick," ");
   %params = nextToken(%params,ch," ");
   %topic  = nextToken(%params,users," :");

	%l = strlen(%ch);
	if ((%l > 7 && strcmp(getSubStr(%ch,%l-7,7),"_Public") == 0) ||
		 (%l > 8 && strcmp(getSubStr(%ch,%l-8,8),"_Private") == 0))
		return;   
   if (IRCClient::censor(%topic))
      return;

   if ($IRCClient::silentList)
   {
	  $IRCClient::channelNames[$IRCClient::numChannels] = %ch;
	  $IRCClient::channelTopics[$IRCClient::numChannels] = %topic;
	  $IRCClient::numChannels++;
   }
   else
      IRCClient::newMessage($IRCClient::currentChannel,%users @ " " @ %ch @ " -- " @ %topic);
}

//------------------------------------------------------------------------------
function IRCClient::onBanList(%prefix,%params)
{
   %params = nextToken(%params,banned," ");
   %params = nextToken(%params,banned," ");
   %params = nextToken(%params,banned," ");
	nextToken(%banned,banned,"!*@*");

   if ($IRCClient::silentBanList)
   {
		// temporarily create person
   	$IRCClient::banList[$IRCClient::numBanned] = IRCClient::findPerson2(%banned,true);
      $IRCClient::removeBan[$IRCClient::banList[$IRCClient::numBanned]] = false;
      $IRCClient::numBanned++;
   }
}

//------------------------------------------------------------------------------
function IRCClient::onListEnd(%prefix,%params)
{
   if ($IRCClient::silentList)
   {
      $IRCClient::silentList = false;
      IRCClient::notify(IDIRC_CHANNEL_LIST);
		IRCClient::connected();
   }
}

//------------------------------------------------------------------------------
function IRCClient::onBanListEnd(%prefix,%params)
{
   if ($IRCClient::silentBanList)
   {
      $IRCClient::silentBanList = false;
      IRCClient::notify(IDIRC_BAN_LIST);
		IRCClient::connected();
   }
}

//------------------------------------------------------------------------------
function IRCClient::onBadNick(%prefix,%params)
{
   IRCClient::newMessage("","NOTICE: Nickname (" @ $IRCClient::people.getObject(0).getName() @ ") is already in use.");
   IRCClient::notify(IDIRC_ERR_NICK_IN_USE);
}

//------------------------------------------------------------------------------
function IRCClient::onNoLogin(%prefix,%params)
{
	%msg = nextToken(%params,cmd," :");
	IRCClient::connected();
	IRCClient::newMessage("","Could not log in:" SPC %msg);	
}

//------------------------------------------------------------------------------
function IRCClient::onAway(%prefix,%params)
{
   %msg = nextToken(%params,nick," :");
   %p = IRCClient::findPerson(%nick);
   if (!%p)
      return;
	%c = IRCClient::findChannel(%nick);
	if (!%c)
		return;

   if (strlen(%msg))
   {
      %p.flags = %p.flags | $PERSON_AWAY;
      if (!(%p.flags & $PERSON_IGNORE))
      	IRCClient::newMessage($IRCClient::currentChannel,IRCClient::taggedNick(%p) @ " is away: " @ %msg);
   }
   else
   {
      %p.flags = %p.flags & ~$PERSON_AWAY;
      if (!(%p.flags & $PERSON_IGNORE))
      	IRCClient::newMessage($IRCClient::currentChannel,IRCClient::taggedNick(%p) @ " has returned.");
   }
	if ($IRCClient::currentChannel.findMember(%p) >= 0)
		IRCClient::notify(IDIRC_SORT);
}

//------------------------------------------------------------------------------
function IRCClient::onAction(%prefix,%params)
{  
   %msg = nextToken(%params,ch," :");
   %c = IRCClient::findChannel(%ch,true);

   IRCClient::newMessage(%c,%prefix @ " " @ %msg);
}

//------------------------------------------------------------------------------
function IRCClient::onAwayReply(%prefix,%params)
{
   //EXAMPLE :rick-266.dynamix.com 301 homer128 RickO :Gone fishing.   
   %params = nextToken(%params,me," ");
   %msg = nextToken(%params,nick," :");
	%p = IRCClient::findPerson(%nick);

	if (%p)
	{	
		%p.flags = %p.flags | $PERSON_AWAY;
		IRCClient::newMessage($IRCClient::currentChannel,IRCClient::taggedNick(%p) SPC "is away:" SPC %msg);
		IRCClient::notify(IDIRC_SORT);
	}
}

//------------------------------------------------------------------------------
function IRCClient::onUnAwayReply(%prefix,%params)
{
	if ($IRCClient::awaytimeout)
		cancel($IRCClient::awaytimeout);
	$IRCClient::awaytimeout = schedule($AWAY_TIMEOUT,0,"ChatAway_Timeout");
   IRCClient::newMessage("","You are no longer marked as being away.");
}

//------------------------------------------------------------------------------
function IRCClient::onNowAwayReply(%prefix,%params)
{
   IRCClient::newMessage("","You have been marked as being away.");
}

//------------------------------------------------------------------------------
function IRCClient::onNotice(%prefix,%params)
{
   //EXAMPLE NOTICE AUTH :*** Found your hostname

   %sender = IRCClient::findPerson2(%prefix,true);

   if (!%sender || !(%sender.flags & $PERSON_IGNORE))
   {
      %msg = strchr(%params,"\x01");
      if (!strlen(%msg))
      {
         // Skip past the target name
         %params = nextToken(%params,params,":");

         IRCClient::newMessage("","NOTICE FROM " @ IRCClient::taggedNick(%sender) @ ": " @ %params);
      }
      else
      {
         // otherwise it's a command imbeded inside NOTICE (oh great!)
         %msg = nextToken(%msg,command," \x01");
         IRCClient::dispatch(%prefix,%command,%msg);
      }
   }
}

//------------------------------------------------------------------------------
function IRCClient::onChannelFull(%prefix,%params)
{  
   %channel = nextToken(%params,channel," ");
   nextToken(%channel,channel," ");

   IRCClient::connected();
   IRCClient::statusMessage("Cannot join " @ %channel @ ": room is full.");
   IRCClient::notify(IDIRC_CHANNEL_FULL);
}

//------------------------------------------------------------------------------
function IRCClient::onChannelInviteOnly(%prefix,%params)
{  
   %channel = nextToken(%params,channel," ");
   nextToken(%channel,channel," ");

   IRCClient::connected();
   IRCClient::statusMessage("Cannot join " @ %channel @ ": room is invite only.");
   IRCClient::notify(IDIRC_INVITE_ONLY);
}

//------------------------------------------------------------------------------
function IRCClient::onInvite(%prefix,%params)
{
   // Find or create the person (should never be NULL)
   %p = IRCClient::findPerson2(%prefix,true);
   if (%p)
   {
      %params = nextToken(%params,channel,":");
      %channel = %params;

      // Only bother the user if they aren't ignoring this person
      if (!(%person.flags & $PERSON_IGNORE))
      {
         // Set vars and notify the responder
         $IRCClient::invitechannel = %channel;
         $IRCClient::inviteperson = IRCClient::displayNick(%p);
         IRCClient::notify(IDIRC_INVITED);
      }
   }
}

//------------------------------------------------------------------------------
function IRCClient::onInviteReply(%prefix,%params)
{
   %params = nextToken(%params,me," ");
	%params = nextToken(%params,person," ");
   %params = nextToken(%params,channel," ");
	%p = IRCClient::findPerson(%person);

   IRCClient::newMessage($IRCClient::currentChannel,"You have invited" SPC IRCClient::taggedNick(%p) SPC "to channel" SPC IRCClient::displayChannel(%channel) @ ".");
}

//------------------------------------------------------------------------------
function IRCClient::onChannelBanned(%prefix,%params)
{  
   %channel = nextToken(%params,channel," ");
   nextToken(%channel,channel," ");

   IRCClient::connected();
   IRCClient::statusMessage("Cannot join " @ %channel @ ": you have been banned.");
   IRCClient::notify(IDIRC_BANNED_CH);
}

//------------------------------------------------------------------------------
function IRCClient::onServerBanned(%prefix,%params)
{  
   $IRCClient::state = $IDIRC_DISCONNECTED;

   IRCClient::statusMessage("You have been banned from this server."); 
   // IRCClient::notify(IDIRC_BANNED_SERVER);
}

//------------------------------------------------------------------------------
function IRCClient::onInvalidNick(%prefix,%params)
{
   $IRCClient::state = IDIRC_DISCONNECTED;
   %params = nextToken(%params,name,":");
   IRCClient::newMessage("",%params);
}

//------------------------------------------------------------------------------
function IRCClient::onBadChannelKey(%prefix,%params)
{
   %channel = nextToken(%params,channel," ");
   nextToken(%channel,channel," ");

	IRCClient::connected();
   IRCClient::statusMessage("Cannot join " @ IRCClient::displayChannel(%channel) @ ": invalid password.");
	$IRCClient::keyChannel = %channel;
   IRCClient::notify(IDIRC_CHANNEL_HAS_KEY);
}

//------------------------------------------------------------------------------
function IRCClient::onVersion(%prefix,%params)
{  
   %time = getSimTime();

   if ((%time - $IRCClient::lastVersioned) >= $VERSION_FLOOD_TIMEOUT)
   {
      $IRCClient::lastVersioned = %time;
   
      nextToken(%prefix,nick,"!");
      %params = nextToken(%params,msg,": ");
      nextToken(%params,msg,"\x01");
      %p = IRCClient::findPerson(%nick);
      if (%p && strlen(%msg))
         IRCClient::newMessage($IRCClient::currentChannel,%nick @ "'s version: " @ %msg @ ".");
      else
         IRCClient::send("NOTICE " @ %prefix @ " :\x01VERSION " @ $VERSION_STRING @ "\x01");
   }
}

//------------------------------------------------------------------------------
function IRCClient::onChallenge(%prefix,%params)
{
   %params = nextToken(%params,challenge," ");
   %me = $IRCClient::people.getObject(0);   
   IRCClient::newMessage("","Received WON authentication challenge");
   %response = WONLoginIRC(%challenge);
   if (strcmp(%challenge,"ERROR"))
      IRCClient::send("CHALRESP " @ %response);
   else
      IRCClient::notify(IDIRC_ERR_BADCHALLENGE);
}

//------------------------------------------------------------------------------
function IRCClient::onChalRespReply(%prefix,%params)
{
   %params = nextToken(%params,nick," ");
   %params = nextToken(%params,ident," ");
   %params = nextToken(%params,reply," ");
   %me = $IRCClient::people.getObject(0);
	%me.setName(%nick);
	IRCClient::setIdentity(%me,%ident @ "@localhost");
   if (WONLoginIRC(%reply) $= "OK")
   {
      IRCClient::newMessage("","Successfully connected to " @ $IRCClient::server @ ":" @ $IRCClient::serverport);
      $IRCClient::state = IDIRC_CONNECTED;
      IRCClient::notify(IDIRC_CONNECTED);

		if ($IRCClient::awaytimeout)
			cancel($IRCClient::awaytimeout);
	  	$IRCClient::awaytimeout = schedule($AWAY_TIMEOUT,0,"ChatAway_Timeout");
	  	// auto join a room if requested
      if (strlen($IRCClient::room))
         IRCClient::send("JOIN " @ $IRCClient::room);
   }
   else
	{
		IRCClient::disconnect();
      IRCClient::notify(IDIRC_ERR_BADCHALRESP_REPLY);
	}
}

//------------------------------------------------------------------------------
function IRCClient::send2(%message,%to)
{
   if ((strcmp($IRCCLient::state,IDIRC_CONNECTING_IRC) && strcmp($IRCClient::state,IDIRC_CONNECTED)) ||
       !strlen(%message))
      return;

   if (getSubStr(%message,0,1) $= "/")   
   {
      %buffer = getSubStr(%message,1,strlen(%message)-1);
      %params = nextToken(%buffer,command," ");

      // dispatch the command to a handler -- if one exists
	  // we don't need to handle all outgoing commands just a few
	  switch$(%command)
	  {
	    case "PING":
		    IRCClient::ping(%params);
		 case "PART":
			IRCClient::part(%params);
		 case "AWAY":
			IRCClient::away(%params);
		 case "ME":
		 	IRCClient::sendAction(%params);
		 case "ACTION":
			IRCClient::sendAction(%params);
		 case "JOIN":
			IRCClient::join(%params);
		 case "QUIT":
			IRCClient::quit(%params);

		 default:
			// otherwise ship it to the server, RAW
		    IRCClient::send(getSubStr(%message,1,strlen(%message)-1));
	  }
   }
   else
      if (strlen(%to))
      {
         IRCClient::send("PRIVMSG " @ %to @ " :" @ %message);
         %c = IRCClient::findChannel(%to);
         if (%c)      
         {
            %me = $IRCClient::people.getObject(0);
            IRCClient::newMessage(%c,IRCClient::taggedNick(%me) @ ": " @ %message);
         }
      }
      else
         if (strcmp($IRCClient::currentChannel.getName(),$CHANNEL_STATUS))
         {
            IRCClient::send("PRIVMSG " @ $IRCClient::currentChannel.getName() @ " :" @ %message);
            %me = $IRCClient::people.getObject(0);
            IRCClient::newMessage($IRCClient::currentChannel,IRCClient::taggedNick(%me) @ ": " @ %message);
         }
}

//------------------------------------------------------------------------------
function IRCClient::sendAction(%message)
{
   if (($IRCClient::state != $IDIRC_CONNECTING_IRC && $IRCClient::state != $IDIRC_CONNECTED) || 
       !strlen(%message))
      return;

   if (getSubStr(%message,0,1) $= "/")   
      IRCClient::send2(%message,"");
   else
   {
      IRCClient::send("PRIVMSG " @ $IRCClient::currentChannel.getName() @ " :\x01ACTION " @ %message @  "\x01");
      %me = $IRCClient::people.getObject(0);
      IRCClient::newMessage($IRCClient::currentChannel,%me.getName() @ " " @ %message);
   }
}

//------------------------------------------------------------------------------
function IRCClient::join(%params)
{
   %tmp = nextToken(%params,channel," ");
   nextToken(%tmp,key," ");

   // Only allow one channel / key combination at a time - no
   // comma delimiting
   nextToken(%channel,channel,",");
   nextToken(%key,key,",");

   %c = IRCClient::findChannel(%channel);
   if (%c)
   {
      $IRCClient::nextChannel = %c;
      IRCClient::notify(IDIRC_ADDCHANNEL);
   }
   else
	{
      IRCClient::send("JOIN " @ %channel @ " " @ %key);
		IRCClient::connecting();
	}
}

//------------------------------------------------------------------------------
function IRCClient::quit(%params)
{
   IRCClient::send("QUIT");

   IRCClient::disconnect();
}

//------------------------------------------------------------------------------
function IRCClient::nick(%nick)
{
   if (($IRCClient::state $= IDIRC_CONNECTED || $IRCClient::state $= IDIRC_CONNECTING_IRC) &&
   	   strlen(nick))
   {
      if (stricmp(%nick, $IRCClient::people.getObject(0).getName()))
         IRCClient::send("NICK " @ %nick);
   }
   else
   {
	  $IRCClient::people.getObject(0).setName(%nick);
      $IRCClient::NickName = %nick;
   }
}

//------------------------------------------------------------------------------
function IRCClient::name(%name)
{
   $IRCClient::people.getObject(0).real = %name;
}

//------------------------------------------------------------------------------
function IRCClient::part(%params)
{
	nextToken(%params,ch," ");
	if (%ch $= $CHANNEL_STATUS)
		return;

	%c = IRCClient::findChannel(%ch);
	if (%c)
		for (%i = 0; %i < $IRCClient::channels.getCount(); %i++)
			if ($IRCClient::channels.getObject(%i) == %c)
			{
				if (getSubStr(%ch,0,1) $= "#" || getSubStr(%ch,0,1) $= "&")
					IRCClient::send("PART " @ %params);
				else
				{
					$IRCClient::channels.remove(%c);
					for (%i = 0; %i < %c.numMembers(); %i++)
					{
						%m = %c.getMemberId(%i);
						%m.ref--;
						if (%m.ref == 0)
						{
							$IRCClient::people.remove(%m);
							%m.delete();
						}
						else
							if (%m != $IRCClient::people.getObject(0) &&
								 (%m.flags & $PERSON_AWAY))
								%m.flags = %m.flags & ~$PERSON_AWAY;
					}
					%c.delete();
					$IRCClient::deletedChannel = %c;
					if ($IRCClient::currentChannel == %c)
						$IRCClient::nextChannel = $IRCClient::channels.getObject(0);
					
					IRCClient::notify(IDIRC_DELCHANNEL);
				}            

			break;
		}
}

//------------------------------------------------------------------------------
function IRCClient::away(%params)
{
   %me = $IRCClient::people.getObject(0);
   if (strlen(%params))
   {
      if ($IRCClient::awaytimeout)
		{
			cancel($IRCClient::awaytimeout);
			$IRCClient::awaytimeout = 0;
		}
      %me.flags = %me.flags | $PERSON_AWAY;         
      IRCClient::send("AWAY :" @ %params);
      for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
		{
			%c = $IRCClient::channels.getObject(%i);
			if (%c.private)
      		IRCClient::send("PRIVMSG " @ %c.getName() @ " :\x01AWAY :" @ %params @ "\x01");
		}
   }
   else
   {
      %me.flags = %me.flags & ~$PERSON_AWAY;         
      IRCClient::send("AWAY");
      for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
		{
			%c = $IRCClient::channels.getObject(%i);
			if (%c.private)
      		IRCClient::send("PRIVMSG " @ %c.getName() @ " :\x01AWAY \x01");
		}
      
   }
	IRCClient::notify(IDIRC_SORT);
}

//------------------------------------------------------------------------------
function IRCClient::ping(%params)
{
   %params = nextToken(%params,nick," ");
   while (strlen(%nick))
   {
      %p = IRCClient::findPerson(%nick);
      if (%p)
      {
         %p.ping = GetSimTime();
         IRCClient::send("PRIVMSG " @ %nick @ " :\x01PING 0\x01");
      }
      %params = nextToken(%params,nick," ");
   }
}

//------------------------------------------------------------------------------
function IRCClient::setOperator(%p)
{
   IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " +o " @ %p.getName());
}

//------------------------------------------------------------------------------
function IRCClient::setSpeaker(%nick)
{
   if (strcmp($IRCClient::currentChannel.getName(),$CHANNEL_STATUS))
   {
      %i = $IRCClient::currentChannel.findMember(%nick);

      if (%i != -1)
      {
         if ($IRCClient::currentChannel.getFlags(%i) & $PERSON_OPERATOR)
            IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " -o " @ %nick);

         IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " +v " @ %nick);
      }
   }
}

//------------------------------------------------------------------------------
function IRCClient::setSpectator(%nick)
{
   if (strcmp($IRCClient::currentChannel.getName(),$CHANNEL_STATUS))
   {
      %i = $IRCClient::currentChannel.findMember(%nick);

      if (%i != -1)
      {
         if ($IRCClient::currentChannel.getFlags(%i) & $PERSON_OPERATOR)
            IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " -o " @ %nick);
         if ($IRCClient::currentChannel.getFlags(%i) & $PERSON_SPEAKER)
            IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " -v " @ %nick);
      }
   }
}

//------------------------------------------------------------------------------
function IRCClient::kick(%p,%msg)
{
   IRCClient::send("KICK" SPC $IRCClient::currentChannel.getName() SPC %p.getName() @ " :" @ %msg);
}

//------------------------------------------------------------------------------
function IRCClient::ban(%p,%add)
{
	if (%add)
   	IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " +b " @ %p.getName());
	else
		IRCClient::send("MODE " @ $IRCClient::currentChannel.getName() @ " -b " @ %p.getName());
}

//------------------------------------------------------------------------------
function IRCClient::ignore(%p,%tf)
{
   if ($IRCClient::currentChannel.getName() $= $CHANNEL_STATUS)
   {
      echo("IRCClient::ignore: no current channel.");

	  return;
   }

   if (%p)
   {   
      if (%tf)
      {
         %p.flags = %p.flags | $PERSON_IGNORE;
         IRCClient::newMessage("","You are now ignoring " @ IRCClient::taggedNick(%p) @ ".");
      }
      else
      {
         %p.flags = %p.flags & ~$PERSON_IGNORE;
         IRCClient::newMessage("","You are no longer ignoring " @ IRCClient::taggedNick(%p) @ ".");
      }
		IRCClient::notify(IDIRC_SORT);
	}
}

//------------------------------------------------------------------------------
function IRCClient::invite(%p,%c)
{
	IRCClient::send("INVITE" SPC %p.getName() SPC %c.getName());
}

//------------------------------------------------------------------------------
function IRCClient::who(%p)
{
	IRCClient::connecting();
	IRCClient::send("WHO" SPC %p.getName());
}

//------------------------------------------------------------------------------
function IRCClient::whois(%p)
{
	IRCClient::connecting();
	IRCClient::send("WHOIS" SPC %p.getName());
}

//------------------------------------------------------------------------------
function IRCClient::topic(%c,%t)
{
	IRCClient::send("TOPIC" SPC %c.getName() SPC ":" @ %t);
}

//------------------------------------------------------------------------------
function IRCClient::setInvite(%c,%i)
{
	if (%i)
		IRCClient::send("MODE" SPC %c.getName() SPC "+i");
	else
		IRCClient::send("MODE" SPC %c.getName() SPC "-i");
}

//------------------------------------------------------------------------------
function IRCClient::setModerate(%c,%m)
{
	if (%m)
		IRCClient::send("MODE" SPC %c.getName() SPC "+m");
	else
		IRCClient::send("MODE" SPC %c.getName() SPC "-m");
}

//------------------------------------------------------------------------------
function IRCClient::setLimit(%c,%l,%m)
{
	if (%l)
		IRCClient::send("MODE" SPC %c.getName() SPC "+l" SPC %m);
	else
		IRCClient::send("MODE" SPC %c.getName() SPC "-l");
}

//------------------------------------------------------------------------------
function IRCClient::setKey(%c,%k,%p)
{
	if (%k)
		IRCClient::send("MODE" SPC %c.getName() SPC "+k" SPC %p);
	else
		IRCClient::send("MODE" SPC %c.getName() SPC "-k");
}

//------------------------------------------------------------------------------
function IRCClient::requestChannelList()
{
   // clear the global channel List
   $IRCClient::numChannels = 0;

   $IRCClient::silentList = true;
   IRCClient::send("LIST");
	IRCClient::connecting();
}

//------------------------------------------------------------------------------
function IRCClient::requestBanList(%c)
{
   // clear the global banned list
	$IRCClient::banChannel = %c;
   $IRCClient::numBanned = 0;

   $IRCClient::silentBanList = true;
   IRCClient::send("MODE " @ %c.getName() @ " +b");
	IRCClient::connecting();
}

//------------------------------------------------------------------------------
function IRCClient::onJoinServer(%mission,%server,%address,%mayprequire,%prequire)
{
   // server and address are required, and we have to be connected
   if (strlen(%server) && strlen(%address) && $IRCClient::state == $IDIRC_CONNECTED)
   {
      %buf = "left to play " @ %mission @ " on server " @ %server @ " [" @ %address @ ":" @ %mayprequire @ %prequire @ "]";

      IRCClient::send("PRIVMSG " @ $IRCClient::currentChannel.getName() @ ":\x01ACTION " @ %buf @ "\x01");
      IRCClient::newMessage($IRCClient::currentChannel,
      						IRCClient::taggedNick($IRCClient::people.getObject(0)) @ " " @ %buf);
   }
}

//------------------------------------------------------------------------------
function IRCClient::onJoinGame(%address,%desc)
{
	%msg = "launched into <t2server:" @ %address @ ">" @ %desc @ "</t2server>.";
	IRCClient::away(%msg);
	for (%i = 1; %i < $IRCClient::channels.getCount(); %i++)
	{
		%c = $IRCClient::channels.getObject(%i);
		if (!%c.private)
			IRCClient::send2(%msg,%c.getName());
	}
}

IRCClient::init();
IRCClient::connect();