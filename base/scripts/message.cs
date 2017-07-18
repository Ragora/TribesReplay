$numDeathMsgLines = 10;
$currentDeathMsgLine = 0;
$deathMsgTimeOut = 5 * 1000;
$MaxMessageWavLength = 5200;

function addMessageCallback(%msgType, %func)
{
   for(%i = 0; (%afunc = $MSGCB[%msgType, %i]) !$= ""; %i++)
   {
      // only add each callback once
      if(%afunc $= %func)
         return;
   }
   $MSGCB[%msgType, %i] = %func;
}

function messagePump(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7 ,%a8, %a9, %a10)
{
   clientCmdServerMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
}

function clientCmdServerMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   %tag = getWord(%msgType, 0);
   for(%i = 0; (%func = $MSGCB["", %i]) !$= ""; %i++)
      call(%func, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
   
   if(%tag !$= "")
      for(%i = 0; (%func = $MSGCB[%tag, %i]) !$= ""; %i++)
         call(%func, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
}

function defaultMessageCallback(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   if ( %msgString $= "" )
      return;
      
   %message = detag( %msgString );
   // search for wav tag marker
   %wavStart = strstr( %message, "~w" );
   if ( %wavStart != -1 )
   {
      %wav = getSubStr( %message, %wavStart + 2, 1000 );
      %wavLengthMS = alxGetWaveLen( %wav );
      if ( %wavLengthMS <= $MaxMessageWavLength )
      {
         %handle = alxCreateSource( AudioChat, %wav );
         alxPlay( %handle );
      }
      else
         error( "WAV file \"" @ %wav @ "\" is too long! **" );

      %message = getSubStr( %message, 0, %wavStart );
      if ( %message !$= "" )
      	addMessageHudLine( %message );
   }
   else
	  addMessageHudLine( %message );
   //else if (strstr(%message, "~x") != -1)
   //{
   //	  %mess = getSubStr(%message, 2, 1000);
   //	  addDeathMsgHudLine(%mess);
   //}
}

function getPlayerPrefs( %player )
{
   // test against %player.guid

   // For now, mute smurfs and listen to real players:
   commandToServer( 'ListenTo', %player.clientId, !%player.isSmurf, false ); 

	// MES -- queryClientPlayerDatabase function call causes console error

   //if(queryClientPlayerDatabase(%player.guid, "muted"))
   //{
   //   %player.chatMuted = true;
   //   addMessageHudLine("Spamming punk \c3" @ %player.name @ "\cr joined and has been auto muted!");
   //}
}

//--------------------------------------------------------------------------
function handleClientJoin(%msgType, %msgString, %clientName, %clientId, %targetId, %isAI, %isAdmin, %isSuperAdmin, %isSmurf, %guid)
{
   logEcho("got client join: " @ detag(%clientName) @ " : " @ %clientId);

	//create the player list group, and add it to the ClientConnectionGroup...
   if(!isObject("PlayerListGroup"))
	{
      %newGroup = new SimGroup("PlayerListGroup");
		ClientConnectionGroup.add(%newGroup);
	}

   %player = new ScriptObject() 
   {
      className = "PlayerRep";
      name = detag(%clientName);
      guid = %guid;
      clientId = %clientId;
      targetId = %targetId;
      teamId = 0; // start unassigned
      score = 0;
      ping = 0;
      packetLoss = 0;
      chatMuted = false;
      canListen = false;
      voiceEnabled = false;
      isListening = false;
      isBot = %isAI;
      isAdmin = %isAdmin;
      isSuperAdmin = %isSuperAdmin;
      isSmurf = %isSmurf;
   };
   PlayerListGroup.add(%player);
   $PlayerList[%clientId] = %player;

   if ( !%isAI )
      getPlayerPrefs(%player);
   lobbyUpdatePlayer( %clientId );
}

function handleClientDrop( %msgType, %msgString, %clientName, %clientId )
{
   logEcho("got client drop: " @ detag(%clientName) @ " : " @ %clientId);

   %player = $PlayerList[%clientId];
   if( %player )
   {
      %player.delete();
      $PlayerList[%clientId] = "";
      lobbyRemovePlayer( %clientId );
   }
}

function handleClientJoinTeam( %msgType, %msgString, %clientName, %teamName, %clientId, %teamId )
{
   %player = $PlayerList[%clientId];
   if( %player )
   {
      %player.teamId = %teamId;
      lobbyUpdatePlayer( %clientId );
   }
}

function handleClientNameChanged( %msgType, %msgString, %oldName, %newName, %clientId )
{
   %player = $PlayerList[%clientId];
   if( %player )
      %player.name = detag( %newName );
}

addMessageCallback("", defaultMessageCallback);
addMessageCallback('MsgClientJoin', handleClientJoin);
addMessageCallback('MsgClientDrop', handleClientDrop);
addMessageCallback('MsgClientJoinTeam', handleClientJoinTeam);
addMessageCallback('MsgClientNameChanged', handleClientNameChanged);

//---------------------------------------------------------------------------
// Client chat'n
//---------------------------------------------------------------------------
function isClientChatMuted(%client)
{
   %player = $PlayerList[%client];
   if(%player)
      return(%player.chatMuted ? true : false);
   return(true);
}

//---------------------------------------------------------------------------
function clientCmdChatMessage(%sender, %voice, %pitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
{
   %message = detag( %msgString );

   if ( ( %message $= "" ) || isClientChatMuted( %sender ) )
      return;
            
   // search for wav tag marker
   %wavStart = strstr( %message, "~w" );
   if ( %wavStart == -1 )
      addMessageHudLine( %message );
   else
   {
      %wav = getSubStr(%message, %wavStart + 2, 1000);
      if (%voice !$= "")
         %wavFile = "voice/" @ %voice @ "/" @ %wav @ ".wav";
      else
         %wavFile = %wav;

      //only play voice files that are < 5000ms in length
      if (%pitch < 0.5 || %pitch > 2.0)
         %pitch = 1.0;
      %wavLengthMS = alxGetWaveLen(%wavFile) * %pitch;
      if (%wavLengthMS < $MaxMessageWavLength )
      {
         if ( $ClientChatHandle[%sender] != 0 )
            alxStop( $ClientChatHandle[%sender] );
         $ClientChatHandle[%sender] = alxCreateSource( AudioChat, %wavFile );

         //pitch the handle
         if (%pitch != 1.0)
            alxSourcef($ClientChatHandle[%sender], "AL_PITCH", %pitch);
         alxPlay( $ClientChatHandle[%sender] );
      }
      else
         error( "** WAV file \"" @ %wavFile @ "\" is too long! **" );

      %message = getSubStr(%message, 0, %wavStart);
      addMessageHudLine(%message);
   }
}

//---------------------------------------------------------------------------
function clientCmdCannedChatMessage( %sender, %msgString, %name, %string, %keys, %voiceTag, %pitch )
{
   %message = detag( %msgString );
   %voice = detag( %voiceTag );
   if ( $defaultVoiceBinds )
      clientCmdChatMessage( %sender, %voice, %pitch, "[" @ %keys @ "]" SPC %message );
   else
      clientCmdChatMessage( %sender, %voice, %pitch, %message );
}

//---------------------------------------------------------------------------
// silly spam protection...
$SPAM_PROTECTION_PERIOD     = 10000;
$SPAM_MESSAGE_THRESHOLD     = 4;
$SPAM_PENALTY_PERIOD        = 10000;
$SPAM_MESSAGE               = '\c3FLOOD PROTECTION:\cr You must wait another %1 seconds.';

function GameConnection::spamMessageTimeout(%this)
{
   if(%this.spamMessageCount > 0)
      %this.spamMessageCount--;
}

function GameConnection::spamReset(%this)
{
   %this.isSpamming = false;
}

function spamAlert(%client)
{
   if($Host::FloodProtectionEnabled != true)
      return(false);

   if(!%client.isSpamming && (%client.spamMessageCount >= $SPAM_MESSAGE_THRESHOLD))
   {
      %client.spamProtectStart = getSimTime();
      %client.isSpamming = true;
      %client.schedule($SPAM_PENALTY_PERIOD, spamReset);
   }

   if(%client.isSpamming)
   {
      %wait = mFloor(($SPAM_PENALTY_PERIOD - (getSimTime() - %client.spamProtectStart)) / 1000);
      messageClient(%client, "", $SPAM_MESSAGE, %wait);
      return(true);
   }

   %client.spamMessageCount++;
   %client.schedule($SPAM_PROTECTION_PERIOD, spamMessageTimeout);
   return(false);
}

function chatMessageClient( %client, %sender, %voiceTag, %voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
	//see if the client has muted the sender
	if ( !%client.muted[%sender] )
	   commandToClient( %client, 'ChatMessage', %sender, %voiceTag, %voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
}

function cannedChatMessageClient( %client, %sender, %msgString, %name, %string, %keys )
{
   if ( !%client.muted[%sender] )
      commandToClient( %client, 'CannedChatMessage', %sender, %msgString, %name, %string, %keys, %sender.voiceTag, %sender.voicePitch );
}

function chatMessageTeam( %sender, %team, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %obj = ClientGroup.getObject( %i );
      if ( %obj.team == %sender.team )
         chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
   }
}

function cannedChatMessageTeam( %sender, %team, %msgString, %name, %string, %keys )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;

   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %obj = ClientGroup.getObject( %i );
      if ( %obj.team == %sender.team )
         cannedChatMessageClient( %obj, %sender, %msgString, %name, %string, %keys );
   }
}

function chatMessageAll( %sender, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
		%obj = ClientGroup.getObject( %i );
		if(%sender.team != 0)
	      chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
		else
		{
			// message sender is an observer -- only send message to other observers
			if(%obj.team == %sender.team)
		      chatMessageClient( %obj, %sender, %sender.voiceTag, %sender.voicePitch, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10 );
		}
	}
}

function cannedChatMessageAll( %sender, %msgString, %name, %string, %keys )
{
   if ( ( %msgString $= "" ) || spamAlert( %sender ) )
      return;

   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
      cannedChatMessageClient( ClientGroup.getObject(%i), %sender, %msgString, %name, %string, %keys );
}

//---------------------------------------------------------------------------
function messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   commandToClient(%client, 'ServerMessage', %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
}

function messageTeam(%team, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
	  if(%recipient.team == %team)
	      messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageTeamExcept(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %team = %client.team;
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
	  if((%recipient.team == %team) && (%recipient != %client))
	      messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageAll(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{
   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %client = ClientGroup.getObject(%cl);
      messageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

function messageAllExcept(%client, %team, %msgtype, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13)
{  
   //can exclude a client, a team or both. A -1 value in either field will ignore that exclusion, so
   //messageAllExcept(-1, -1, $Mesblah, 'Blah!'); will message everyone (since there shouldn't be a client -1 or client on team -1).
   %count = ClientGroup.getCount();
   for(%cl= 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
      if((%recipient != %client) && (%recipient.team != %team))
         messageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13);
   }
}

//#####################################################
////--------------------------------------------------------------------------- 
////	Command Message
////--------------------------------------------------------------------------- 
//
//function clientCmdServerCommandMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
//{
//   commandMsgHud.addLine(detag(%msgString));
//}
//
//function commandMessageClient(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
//{
//   commandToClient(%client, 'ServerCommandMessage', %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
//}
//
//function commandMessageTeam(%team, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
//{
//   %count = ClientGroup.getCount();
//   for(%cl= 0; %cl < %count; %cl++)
//   {
//      %recipient = ClientGroup.getObject(%cl);
//	  if(%recipient.team == %team)
//	      commandMessageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
//   }
//}
//
//function commandMessageTeamExcept(%client, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
//{
//   %team = %client.team;
//   %count = ClientGroup.getCount();
//   for(%cl= 0; %cl < %count; %cl++)
//   {
//      %recipient = ClientGroup.getObject(%cl);
//	  if((%recipient.team == %team) && (%recipient != %client))
//	      commandMessageClient(%recipient, %msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
//   }
//}
//#####################################################

//modified defaultgame and added deathmessages.cs to handle this - EL
// addMessageCallback('MsgSuicide', suicideMessage);
// 
// function suicideMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
// {
// 	%victim = detag(%a1);
// 	%suicideMsg = %victim @ " hits CTRL-K. What a wimp!";
// 	addDeathMsgHudLine(%suicideMsg);
// }

//modified defaultgame and added deathmessages.cs to handle this - EL
// addMessageCallback('MsgSelfKill', selfKillMessage);
// 
// function selfKillMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
// {
// 	%victim = detag(%a1);
// 	%gender = detag(%a2);
// 	// gender will be HIM or HER
// 	%poss = detag(%a3);
// 	// poss will be HIS or HER	 (possessive)
// 	%damageType = detag(%a4);
// 	switch$ (%damageType) {
// 		case $DamageType::Plasma :
// 			%suicideMsg = %victim @ " gets a taste of " @ %poss @ " own plasma.";
// 		case $DamageType::Bullet :
// 			%suicideMsg = %victim @ " shoots " @ %gender @ "self in the foot.";
// 		case $DamageType::Disc :
// 			%suicideMsg = %victim @ " blasts " @ %gender @ "self with a disc.";
// 		case $DamageType::Grenade :
// 			%suicideMsg = %victim @ " eats " @ %poss @ " own grenade.";
// 		case $DamageType::Mortar :
// 			%suicideMsg = %victim @ " launches a mortar too close to home.";
// 		case $DamageType::Missile :
// 			%suicideMsg = %victim @ " blows " @ %gender @ "self up with a missile.";
// 		case $DamageType::Explosion :
// 			%suicideMsg = %victim @ " gets too close to an explosion.";
// 		case $DamageType::Ground :
// 			%suicideMsg = %victim @ " hits the ground hard.";
// 		default:
// 			%suicideMsg = %victim @ " kills " @ %gender @ "self.";
// 	}
// 	addDeathMsgHudLine(%suicideMsg);
// }

//modified defaultgame and added deathmessages.cs to handle this - EL
// addMessageCallback('MsgPlayerTeamKill', playerTeamKillMessage);
// 
// function playerTeamKillMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
// {
// 	%victim = detag(%a1);
// 	%killer = detag(%a2);
// 	%killPoss = detag(%a3);
// 	%teamKillMsg = %killer @ " mows down " @ %killPoss @ " teammate " @ %victim @ ".";
// 	addDeathMsgHudLine(%teamKillMsg);
// }

//modified defaultgame and added deathmessages.cs to handle this - EL
// addMessageCallback('MsgPlayerIsKilled', playerIsKilledMessage);
// 
// function playerIsKilledMessage(%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
// {
// 	%victim = detag(%a1);
// 	%killer = detag(%a2);
// 	%vicGen = detag(%a3);
// 	%vicPoss = detag(%a4);
// 	//%kilGen = detag(%a5);
// 	//%kilPoss = detag(%a6);
// 	// vicGen and kilGen will be HIM or HER
// 	// vicPoss and kilPoss will be HIS or HER
// 	%damType = detag(%a5);
// 	switch$ (%damType) {
// 		case $DamageType::Blaster :
// 			%dMsg = %killer @ " blasts the life out of " @ %victim @ ".";
// 		case $DamageType::Plasma :
// 			%dMsg = %killer @ " gives " @ %victim @ " a plasma transfusion.";
// 		case $DamageType::Bullet :
// 			%dMsg = %killer @ " gives " @ %victim @ " " @ %vicPoss @ " daily dose of lead.";
// 		case $DamageType::Disc :
// 			%dMsg = %killer @ " gives " @ %victim @ " a Stormhammer salute.";
// 		case $DamageType::Grenade :
// 			%dMsg = %killer @ " takes " @ %victim @ " out with a grenade.";
// 		case $DamageType::Laser :
// 			%dMsg = %killer @ " picks off " @ %victim @ " with a sniper shot.";
// 		case $DamageType::ELF :
// 			%dMsg = %killer @ " drains " @ %victim @ "\'s life force.";
// 		case $DamageType::Mortar :
// 			%dMsg = %killer @ " mortars " @ %victim @ " into oblivion.";
// 		case $DamageType::Missile :
// 			%dMsg = %killer @ "\'s missile catches up with " @ %victim @ ".";
// 		case $DamageType::ShockLance :
// 			%dMsg = %killer @ " assassinates " @ %victim @ ".";
// 		case $DamageType::Mine :
// 			%dMsg = %killer @ "\'s mine takes out " @ %victim @ ".";
// 		case $DamageType::Explosion :
// 			%dMsg = %killer @ " blows up " @ %victim @ ".";
// 		case $DamageType::Impact :
// 			%dMsg = %killer @ " runs over " @ %victim @ ".";
// 		case $DamageType::Turret :
// 			%dMsg = %killer @ "\'s turret kills " @ %victim @ ".";
// 		default:
// 			%dMsg = %victim @ " falls victim to " @ %killer @ ".";
// 	}
// 	addDeathMsgHudLine(%dMsg);
// }

//function addDeathMsgHudLine(%msg)
//{
//	if(!isObject(deathMsgHud)){
//		%dmHudX = firstWord(getResolution()) - 245;
//		%dmHudY = (getWord(objectiveHud.extent, 1) + getWord(objectiveHud.position, 1)) + 10;
//		%dmhPos = %dmHudX @ " " @ %dmHudY;
//		//error("y coords = " @ getWord(objectiveHud.extent, 1) @ " + " @ getWord(objectiveHud.position, 1) @ " + 10");
//		%dmHud = new GuiControl(deathMsgHud) {
//			profile = "GuiDeathMsgHudProfile";
//			horizSizing = "left";
//			vertSizing = "bottom";
//			position = %dmhPos;
//			extent = "240 170";
//			visible = "1";
//			modal = "0";
//		};
//		PlayGui.add(%dmHud);
//	}
//	%msgName = "deathMsg" @ $currentDeathMsgLine;
//	if(nameToId(%msgName) > 0)
//		%msgName.delete();
//	%msgYCoord = ($currentDeathMsgLine * 15);
//	%msgCoords = "3 " @ %msgYCoord;
//	// calculate how many lines the message will need to use
//	%multiLine = false;
//	if(strLen(%msg) > 52) {
//		%multiLine = true;
//		// find first space before character 52 and break the line there
//		%lBr = -1;
//		for(%i = 51; %i > 0; %i--)
//			if(getSubStr(%msg, %i, 1) $= " ") {
//				%lBr = %i;
//				break;
//			}
//		if(%lBr > -1) {
//			%msg1 = getSubStr(%msg, 0, %lBr);
//			%msg2 = getSubStr(%msg, %lBr + 1, strLen(%msg));
//			//error("Message line 1: |" @ %msg1 @ "|");
//			//error("Message line 2: |" @ %msg2 @ "|");
//		}
//	}
//	else
//		%msg1 = %msg;
//	%addDeathMsg = new GuiTextCtrl(%msgName) {
//		profile = "DeathMsgTextProfile";
//		horizSizing = "left";
//		vertSizing = "bottom";
//		position = %msgCoords;
//		extent = "235 15";
//		minExtent = "8 8";
//		visible = "1";
//		helpTag = "0";
//		text = "";
//	};
//	deathMsgHud.add(%msgName);
//	%msgName.setValue(%msg1);
//	$currentDeathMsgLine++;
//	%msgName.schedule($deathMsgTimeOut, "delete");
//	if(%multiLine) {
//		%msgName2 = "deathMsg" @ ($currentDeathMsgLine + 1);
//		if(nameToId(%msgName2) > 0)
//			%msgName2.delete();
//		%msgYCoord = ($currentDeathMsgLine * 15);
//		%msgCoords = "3 " @ %msgYCoord;
//		%addDeathMsg2 = new GuiTextCtrl(%msgName2) {
//			profile = "DeathMsgTextProfile";
//			horizSizing = "left";
//			vertSizing = "bottom";
//			position = %msgCoords;
//			extent = "235 15";
//			minExtent = "8 8";
//			visible = "1";
//			helpTag = "0";
//			text = "";
//		};
//		deathMsgHud.add(%msgName2);
//		%msgName2.setValue(%msg2);
//		$currentDeathMsgLine++;
//		%msgName2.schedule($deathMsgTimeOut, "delete");
//	}
//	if($currentDeathMsgLine >= $numDeathMsgLines)
//		$currentDeathMsgLine = 0;
//}