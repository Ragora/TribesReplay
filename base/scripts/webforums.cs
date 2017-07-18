//------------------------------------------
// Forums code
//------------------------------------------
$ForumCacheVersion = 8; //lucky seven...NOT!
$ForumCachePath = "webcache/" @ getField(wonGetAuthInfo(),3) @ "/";
$ForumsConnecting = "CONNECTING";
$ForumsGetForums = "FETCHING FORUM LIST ";
$ForumsGetTopics = "FETCHING TOPICS ";
$ForumsTitle = "FORUMS";
$ForumsGetPosts = "FETCHING POSTS ";

$TopicColumnCount    = 0;
$TopicColumnName[0]  = "Topic";
$TopicColumnRange[0] = "50 1000";
$TopicColumnCount++;
$TopicColumnName[1]  = "Posts";
$TopicColumnRange[1] = "25 100";
$TopicColumnFlags[1] = "numeric center";
$TopicColumnCount++;
$TopicColumnName[2]  = "Posted By";
$TopicColumnRange[2] = "50 300";
$TopicColumnCount++;
$TopicColumnName[3]  = "Last Post Date";
$TopicColumnRange[3] = "50 300";
$TopicColumnCount++;

$ForumColumnCount = 0;
$ForumColumnName[0]  = "Message Tree";
$ForumColumnRange[0] = "50 1000";
$ForumColumnCount++;
$ForumColumnName[1]  = "Posted By";
$ForumColumnRange[1] = "50 300";
$ForumColumnCount++;
$ForumColumnName[2]  = "Date Posted";
$ForumColumnRange[2] = "50 300";
$ForumColumnCount++;

// format of a forum post is:
// Post ID
// Parent Post ID
// subject
// Author
// Post date
// Text lines

//Forums message vector post:
// postId
// parentId
// Topic
// Poster
// Date
// Message Line0
// MessageLine 1
// MessageLine 2

// Update is defined as:
// PostId
// UpdateId
// parentId
// poster
// Date
// topic
// body lines

if(!isObject(ForumsMessageVector))
{
   new MessageVector(ForumsMessageVector);
}
//-----------------------------------------------------------------------------
function DateStrCompare(%date1,%date2)
{   
   %d1 = getSubStr(%date1,0,2);
   %d2 = getSubStr(%date2,0,2);

   if(%d1 == %d2)
   {
       %d1 = getSubStr(%date1,3,2);
       %d2 = getSubStr(%date2,3,2);
       if(%d1 == %d2)
       {
           %d1 = getSubStr(%date1,6,4);
           %d2 = getSubStr(%date2,6,4);
           if(%d1 == %d2)
           {
               if(getSubStr(%date1,17,1)$="a")
                   %d1 = getSubStr(%date1,11,2)+12;
               else
                   %d1 = getSubStr(%date1,11,2);
                   
               if(getSubStr(%date2,17,1)$="a")
                   %d2 = getSubStr(%date2,11,2)+12;
               else
                   %d2 = getSubStr(%date2,11,2);
                   
               if(%d1 == %d2)
               {
                   %d1 = getSubStr(%date1,14,2);
                   %d2 = getSubStr(%date2,14,2);
                   if(%d1 >= %d2)
                       return true;
                   else
                       return false;                         
               }
               else if(%d1 > %d2)
                   return true;
               else
                   return false;               
           }
           else if(%d1 > %d2)
               return true;
           else
               return false;
       }
       else if(%d1 > %d2)
           return true;
       else
           return false;
   }
   else if (%d1 > %d2)
       return true;
   else
       return false;
}
//-----------------------------------------------------------------------------
function BackToTopics()
{
   CacheForumTopic();
	ForumsGui.eid = schedule(250,ForumsGui,ForumsGoTopics);
}
//-----------------------------------------------------------------------------
function CacheForumTopic()
{
	%allRead = true;
	%numLines = ForumsMessageVector.getNumLines();
	for ( %line = 0; %line < %numLines; %line++ )
	{
		%lineText = ForumsMessageVector.getLineText( %line );
		if ( getRecord( %lineText, 0 ) $= "0" )
			%allRead = false;
           
     	// Keep track of the latest date:
      	%lineDate = getRecord( %lineText, 5 );
      	if ( %latest $= "" || strcmp( %lineDate, %latest ) > 0 )
        	%latest = %lineDate;
	}
   if(!ForumsMessageList.highestUpdate)
       ForumsMessageList.highestUpdate = 0;

   %newGroup = TopicsListGroup.getObject(ForumsTopicsList.getSelectedRow());
   ForumsMessageList.lastID = %newGroup.updateid;
   %latest = GetField(ForumsTopicsList.getRowTextbyID(ForumsTopicsList.getSelectedID()),3);
       
	ForumsMessageVector.dump( $ForumCachePath @ "tpc" @ ForumsMessageVector.tid , ForumsMessageList.lastID TAB $ForumCacheVersion TAB %allRead TAB %latest);
}
//-----------------------------------------------------------------------------
function ForumsAcceptPost()
{
   %parentId = ForumsMessageList.getSelectedId();
   %text = ForumsMessageVector.getLineTextByTag(%parentId);
   %author = getRecord( %text, 4 );
   %dev = getLinkName(%author);
   %date = getRecord(%text, 5);
   %body = getRecords(%text, 7);
   ForumsGui.ebstat = FO_EditBtn.Visible;
   ForumsGui.destat = FO_DeleteBtn.Visible;
   $NewsTitle = getRecord(%text, 3);
   ForumsMessageList.state = "newsAccept";
   Canvas.pushDialog(NewsPostDlg);
   NewsCategoryMenu.clear();
   NewsCategoryMenu.add( "General", 0 );
   NewsCategoryMenu.add( "Announcements", 1 );
   NewsCategoryMenu.add( "Events", 2 );
   NewsCategoryMenu.add( "Updates", 3 );
   NewsCategoryMenu.setSelected( 0 );
   NewsPostBodyText.setValue("submitted by " @ %dev @ "\n\n" @ ForumsGetTextDisplay(%body));
   NewsPostDlg.postID = -1;
   NewsPostDlg.action = "News";
}
//-----------------------------------------------------------------------------
function ForumsEditPost()
{
   ForumsGui.ebstat = FO_EditBtn.Visible;
   ForumsGui.destat = FO_DeleteBtn.Visible;
   %text = ForumsMessageVector.getLineTextByTag(ForumsComposeDlg.parentPost);
   $ForumsSubject = getRecord(%text, 3);
   Canvas.pushDialog(ForumsComposeDlg);
   ForumsBodyText.setValue(ForumsGetTextDisplay(%text,7));
   ForumsComposeDlg.post = ForumsComposeDlg.parentPost;
   ForumsComposeDlg.action = "Edit";
}
//-----------------------------------------------------------------------------
function ForumsGetTextDisplay(%text, %offSet)
{
   %msgText = "";
   %rc = getRecordCount(%text);

   for(%i = %offSet; %i < %rc; %i++)
      %msgText = %msgText @ getRecord(%text, %i) @ "\n";
   return %msgText;
}
//-----------------------------------------------------------------------------
function ForumsGoTopics(%direction)
{
//   ForumsTopicsList.direction = %direction;
   ForumsTopicsList.MaxDate = "";
   ForumsTopicsList.MinDate = "";
   ForumShell.setTitle($ForumsConnecting);
   ForumsThreadPane.setVisible(false);
   ForumsTopicsPane.setVisible(true);
   FO_RejectBtn.visible = false;
   FO_EditBtn.visible = false;
   FO_AcceptBtn.visible = false;
   if ( ForumsTopicsList.rowCount() == 0 || ForumsTopicsList.refreshFlag )
   {
		FM_NewTopic.setActive(true);
		ForumShell.setTitle($ForumsConnecting);
		ForumsGui.eid = schedule(250,ForumsGui,GetTopicsList);
   }	
   else
	ForumsTopicsList.updateReadStatus();  //looks at file if any posts have been added/edited/deleted...
	ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
}
//-----------------------------------------------------------------------------
function ForumsRefreshTopics()
{
	ForumsTopicsList.refreshFlag = true;
//   ForumsMessageVector.clear();
//   ForumsMessageVector.tid = "";
	ForumsGui.eid = schedule(250,ForumsGui,GetTopicsList);
}
//-----------------------------------------------------------------------------
function ForumsMessageAddRow(%text)
{
   %rc = ForumsMessageList.rowCount();
   %isRead = getRecord( %text, 0 );
   %id = getRecord(%text, 1);
   %parentId = getRecord(%text, 2);
   %subject = getRecord(%text, 3);
   %author = getField(getTextName(getRecord(%text, 4), 0), 0);
   %authorName = getField(getRecord(%text,4),0);
   %date = getRecord(%text, 5);
   %ref = getRecord(%text, 6);
   %oldRow = ForumsMessageList.getRowNumById(%id);
   %selId = ForumsMessageList.getSelectedId();
   
	if(!%selID)
	{
		%selID = ForumsGui.lastSelected;
		ForumsGui.lastSelected = "";
	}
   
   if(%parentId)
   {
      for(%i = 0; %i < %rc; %i++)
      {
		// check for existing?
         if(ForumsMessageList.getRowId(%i) == %parentId)
         {
            %parentRow = ForumsMessageList.getRowText(%i);
//            echo("Found parent");
            break;
         }
      }
      %indentLevel = getField(%parentRow, 3) + 1;
      %indentSpace = "";
      for(%j = 0; %j < %indentLevel; %j++)
         %indentSpace = %indentSpace @ "    ";
   }
   else
      %indentSpace = "";
	
   %rowText = %indentSpace @ %subject TAB %author TAB %date TAB %indentLevel TAB %parentId TAB %ref TAB %authorName;

	if(%oldRow != -1) //if there's a rownumber - message exists
	{
    	ForumsMessageList.removeRow(%oldRow);
    	ForumsMessageList.addRow(%id, %rowText, %oldRow);
	}
	else if(!%parentId) //if a first post
	{
    	ForumsMessageList.addRow(%id, %rowText, 0);
	}
	else //continue from %i
	{
			for(%i++; %i < %rc; %i++)
			{
				%row = ForumsMessageList.getRowText(%i);
         		while(%row !$= "")
				{
            		%rowParent = getField(%row, 4);
            		if(%rowParent == %parentId)
               		break;

           	 		%row = ForumsMessageList.getRowTextById(%rowParent);
				}
         		if(%row $= "")
            	break;
			}
			if(%i <= %rc)
       			ForumsMessageList.addRow(%id, %rowText, %i);
			else
   		    	ForumsMessageList.addRow(%id, %rowText);
	}
   ForumsMessageList.setRowStyleById( %id, !%isRead );

}
//-----------------------------------------------------------------------------
function ForumsNewTopic()
{
	if(ForumsList.getSelectedID()==105)
	{
		$NewsCategory = "";
		$NewsTitle = "";
		Canvas.pushDialog(NewsPostDlg);
		NewsPostDlg.postId = -1;
		NewsPostBodyText.setValue("");
	}
	else
	{	
//  		ForumsGui.LaunchForum = getField(ForumsList.getRowTextById(ForumsList.getSelectedID()),0);
		$ForumsSubject = "";
		Canvas.pushDialog( ForumsComposeDlg );
		ForumsBodyText.setValue( "" );
		ForumsComposeDlg.parentPost = 0;
		ForumsComposeDlg.action = "Post";
	}
}
//-----------------------------------------------------------------------------
function ForumsNext()
{
   %Currow = ForumsMessageList.getSelectedRow();
   if( %Currow < ForumsMessageList.rowCount() )
      ForumsMessageList.setSelectedRow( %Currow + 1 );
}
//-----------------------------------------------------------------------------
function ForumsOpenThread(%tid)
{
	ForumsGui.eid = schedule(250,ForumsGui,GetTopicPosts);
}   
//-----------------------------------------------------------------------------
function ForumsPost()
{
//   error("FORUMPOST: " @ ForumsComposeDlg.action TAB ForumsComposeDlg.parentPost TAB $ForumsSubject);
   $ForumsSubject = FP_SubjectEdit.getValue();
   if ( trim($ForumsSubject) $= "" )
   {
      MessageBoxOK( "POST FAILED", "Your post cannot be accepted without text in the Subject line.",
         "FP_SubjectEdit.makeFirstResponder(1);");
      return;
   }
   
	TextCheck($ForumsSubject,ForumsGui);
	if(!ForumsGui.textCheck)
	{
//   if(ForumsComposeDlg.action !$= "Post" || ForumsComposeDlg.parentPost != 0)
//      {
//         %proxy = ForumsMessageList;
//         %newGroup = TopicsListGroup.getObject(ForumsTopicsList.getSelectedRow());
//         %proxy.lastId = %newGroup.updateid;
//         %proxy.highestUpdate = %proxy.lastId;
//         %proxy.state = "updateCheck";
//      }
      
      ForumsTopicsList.refreshFlag = 1;
      if(ForumsComposeDlg.parentPost == 0) //this is a new topic request
      {
         if(ForumsComposeDlg.action $= "Post")
         {
           %ord = 12;
           %proxy = ForumsGui;
           %proxy.state = "newTopic";
   	    ForumsGui.LaunchTopic = $ForumsSubject;
           %fieldData = ForumsComposeDlg.forum TAB
						 ForumsComposeDlg.topic TAB 
						 ForumsComposeDlg.parentPost TAB
						 $ForumsSubject TAB
						 ForumsBodyText.getValue();
         }
         else if(ForumsComposeDlg.action $="News")
		  {
           %ord = 14;
		    %proxy.state = "postNews";
           %fieldData = ForumsComposeDlg.post TAB
						 $ForumsSubject TAB
						 ForumsBodyText.getValue();
         }
      }
      else if(ForumsComposeDlg.parentPost != 0)
      {
         if(ForumsComposeDlg.action $= "Reply")
         {
           %ord = 12;
           %proxy = ForumsMessageList;
		    %proxy.state = "replyPost";
           %fieldData = ForumsComposeDlg.forum TAB
						 ForumsComposeDlg.topic TAB 
						 ForumsComposeDlg.parentPost TAB
						 $ForumsSubject TAB
						 ForumsBodyText.getValue();
         }
	      else if(ForumsComposeDlg.action $="Edit")
         {
           %ord = 13;
           %proxy = ForumsMessageList;
			%proxy.state = "editPost";
           %fieldData = ForumsComposeDlg.parentPost TAB
      					 $ForumsSubject TAB
      					 ForumsBodyText.getValue();
         }
      }
      %proxy.key = LaunchGui.key++;
      canvas.SetCursor(ArrowWaitCursor);
//      error("DQ: " @ %ord NL %fieldData);
      DatabaseQuery(%ord,%fieldData,%proxy, %proxy.key);
	   Canvas.popDialog(ForumsComposeDlg);
	}
	else
	{
		messageBoxOK("ERROR","Please remove any of the following characters contained in your subject line and resubmit" NL "" NL " : < > * ^ | ~ @ % & / \\ ` \"");
		FP_SubjectEdit.makeFirstResponder(1);
	}
}
//-----------------------------------------------------------------------------
function ForumsPrevious()
{
   %Currow = ForumsMessageList.getSelectedRow();
   if( %Currow > 0 )
      ForumsMessageList.setSelectedRow( %Currow - 1 );
}
//-----------------------------------------------------------------------------
function ForumsRejectPost() //forumsDeletePost()
{
   ForumsGui.ebstat = FO_EditBtn.Visible;
   ForumsGui.destat = FO_DeleteBtn.Visible;
   ForumsMessageList.key = LaunchGui.key++;
   ForumsMessageList.state = "deletePost";
   canvas.SetCursor(ArrowWaitCursor);
//   error("REJECT: " @ ForumsComposeDlg.parentPost);
   MessageBoxYesNo("CONFIRM", "Are you sure you wish to remove the selected post?",
  	      "DatabaseQuery(14," @ ForumsComposeDlg.parentPost @ "," @ ForumsMessagelist @ "," @ ForumsMessagelist.key @ ");", "canvas.SetCursor(defaultCursor);");
}
//-----------------------------------------------------------------------------
function ForumsReply()
{
   %text = ForumsMessageVector.getLineTextByTag(ForumsComposeDlg.parentPost);
   ForumsGui.ebstat = FO_EditBtn.Visible;
   ForumsGui.destat = FO_DeleteBtn.Visible;
   $ForumsSubject = getRecord(%text, 3);
   Canvas.pushDialog(ForumsComposeDlg);
   ForumsBodyText.setValue("");   
   QuoteBtn.setVisible(ForumsMessageVector.getNumLines() > 0);
//      MessageBoxYesNo("QUOTE?","Include Topic Post Text?","GetQuotedText();","ForumsBodyText.setValue(\"\");");
      
   ForumsComposeDlg.action = "Reply";
}
//-----------------------------------------------------------------------------
function GetQuotedText()
{
   if(ForumsComposeDlg.parentPost == 0)
   {
       ForumsBodyText.setValue("<spush><color:FFCCAA>ALL YOUR BASE ARE BELONG TO US<spop>\n\n");
       ForumsBodyText.MakeFirstResponder(1);
       ForumsBodyText.setCursorPosition(3600);
   }
   else
   {
      ForumsBodyText.setValue("<spush><color:FFCCAA>\"" @ trim(ForumsText.getText()) @ "\"<spop>\n\n");
       ForumsBodyText.MakeFirstResponder(1);
      ForumsBodyText.setCursorPosition(3600);
   }
//   ForumsBodyText.setCursorPosition(strLen(ForumsBodyTExt.getText())+5);
}
//-----------------------------------------------------------------------------
function LaunchForums( %forum, %topic )
{
	ForumsGui.setVisible(false);
	ForumsGui.launchForum = %forum;
	ForumsGui.launchTopic = %topic;
	forumsList.clear();
   if(trim(ForumsGui.launchTopic) $= "")
   {
      ForumsThreadPane.setVisible(false);
       ForumsTopicsPane.setVisible(true);
   }
       
	LaunchTabView.viewTab( "FORUMS", ForumsGui, 0 );
}
//-----------------------------------------------------------------------------
function GetForumsList()
{
	ForumsList.clear();
	ForumsGui.onWake();
}
//-----------------------------------------------------------------------------
function GetTopicsList()
{
//   error("GTL: " @ ForumsComposeDlg.forum);
	ForumsGui.key = LaunchGui.key++;
	ForumShell.setTitle($ForumsGetTopics);
   ForumsGui.state = "getTopicList";
	ForumsTopicsList.clear();
   canvas.SetCursor(ArrowWaitCursor);
   ForumsTopicsList.clearList();
//   error("DQA:" @ 8 TAB ForumsComposeDlg.forum);
	DatabaseQueryArray(8,80,ForumsComposeDlg.forum,ForumsGui,ForumsGui.key);
	ForumsTopicsList.refreshFlag = 0;
}
//-----------------------------------------------------------------------------
function GetTopicPosts()
{
	ForumsGui.key = LaunchGui.key++;
	ForumShell.setTitle($ForumsGetPosts);
	ForumsGui.state = "getPostList";
   canvas.SetCursor(ArrowWaitCursor);

   ForumsThreadPane.setVisible(true);
	ForumsTopicsPane.setVisible(false);
	ForumsText.setValue("");
   
	FO_TopicText.setValue(strupr(getField(ForumsTopicsList.getRowTextByID(ForumsComposeDlg.Topic),0)));      
   if(!ForumsComposeDlg.Topic)
       ForumsComposeDlg.topic = ForumsTopicsList.getSelectedID();

   ForumsMessageList.loadCache(getField(ForumsList.getRowTextByID(ForumsList.getSelectedID()),1));
   if(ForumsMessageList.lastID == 0)
   {
       ForumsMessageVector.clear();
       ForumsMessageList.clear();
   }   
//   error("DQA:" @ 8 TAB ForumsComposeDlg.topic);
   DatabaseQueryArray(9,0,ForumsComposeDlg.Topic TAB ForumsMessageList.lastID,ForumsGui,ForumsGui.key);
}
//-----------------------------------------------------------------------------
//-- ForumsGui ---------------------------------------------------------------
//-----------------------------------------------------------------------------
function ForumsGui::onAdd( %this )
{
   %this.initialized = false;
}
//-----------------------------------------------------------------------------
function ForumsGui::onWake(%this)
{
   // First time only:
   if ( !%this.initialized )
   {
      FM_NewTopic.setActive(false);
      ForumsThreadPane.setVisible(false);
      ForumsTopicsPane.setVisible(true);
      ForumsMessageList.thread = "";
      ForumsMessageList.lastId = "";
	   // Both panes should have the same minimum extents...
	   %minExtent = FO_MessagePane.getMinExtent();
	   FO_Frame.frameMinExtent( 0, firstWord( %minExtent ), restWords( %minExtent ) );
	   FO_Frame.frameMinExtent( 1, firstWord( %minExtent ), restWords( %minExtent ) );
      %this.initialized = true;
   }
   Canvas.pushDialog(LaunchToolbarDlg);
   if ( ForumsList.rowCount() == 0)
   {
      ForumsGui.key = LaunchGui.key++;
      ForumShell.setTitle($ForumsConnecting);
      ForumsGui.state = "getForumList";
     canvas.SetCursor(ArrowWaitCursor);
	  DatabaseQueryArray(7,100,"",ForumsGui,ForumsGui.key);
   }
	// Make these buttons inactive until a message is selected:
	FO_ReplyBtn.setActive( false );
	FO_NextBtn.setActive( false );
	FO_PreviousBtn.setActive( false );
}
//-----------------------------------------------------------------------------
function ForumsGui::onSleep(%this)
{
   Canvas.popDialog(LaunchToolbarDlg);
   // Stop the scheduled refreshes:
   cancel( %this.messageRefresh );
}
//-----------------------------------------------------------------------------
function ForumsGui::setKey( %this, %key )
{
}
//-----------------------------------------------------------------------------
function ForumsGui::onClose( %this, %key )
{
}
//-----------------------------------------------------------------------------
function ForumsGui::onDatabaseQueryResult(%this,%status,%resultString,%key)
{
	if(%this.key != %key)
		return;
//	echo("RECV: " @ %status TAB %resultString);
	if(getField(%status,0)==0)
	{
		switch$(%this.state)
		{
			case "getForumList":
				if(getField(%resultString,0)>0)
				{
					%this.forumCount = -1;
					ForumShell.setTitle($ForumsGetForums @ ": " @ getField(%resultString,0));
					%this.state = "ForumList";
					ForumsList.clear();
				}
				else
				{
					%this.state = "done";
					MessageBoxOK("NO DATA","No Forums found");
				}
			case "getTopicList":
				if(getField(%resultString,0)>0)
				{
					ForumShell.setTitle($ForumsGetTopics @ ": " @ getField(%resultString,0));
					%this.state = "TopicList";
				}
				else
               {
					%this.state = "done";
                   ForumsTopicsList.updateReadStatus();
               }

			case "getPostList":
				%statFlag = getField(%status,2);
				%forumFlag = getField(ForumsList.getRowTextbyId(ForumsList.getSelectedID()),1);
               %forumID = getField(ForumsList.getRowTextById(ForumsList.getSelectedID()),2);
               %forumTID = ForumsTopicsList.getSelectedID();

	   			%this.bflag = %statFlag;
 
 		   		switch$ ( %this.bflag )
 		   		{
 		      		case 0: 
 						FO_RejectBtn.visible = false;
 						FO_EditBtn.visible = false;
 			   		case 1:
 						FO_RejectBtn.visible = false;
 						FO_EditBtn.visible = false;
 			   		case 2:
 						switch(%forumID)
 						{
 							case 1402:
 			      				FO_AcceptBtn.visible = true;
 			      				FO_RejectBtn.text = "REJECT";
			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 			      				FO_AcceptBtn.text = "ACCEPT";
 							default:
 			      				FO_RejectBtn.text = "DELETE";
 			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 						}
 			   		case 3:
 						switch(%forumID)
 						{
 							case 1402:
 			      				FO_AcceptBtn.visible = true;
 			      				FO_RejectBtn.text = "REJECT";
 			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 			      				FO_AcceptBtn.text = "ACCEPT";
 							default:
 			      				FO_RejectBtn.text = "DELETE";
 			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 						}
 			   		case 4:
 						switch(%forumID)
 						{
 							case 1402:
 			      				FO_AcceptBtn.visible = true;
 			      				FO_RejectBtn.text = "REJECT";
 			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 			      				FO_AcceptBtn.text = "ACCEPT";
 							default:
 			      				FO_RejectBtn.text = "DELETE";
 			      				FO_RejectBtn.visible = true;
 			      				FO_EditBtn.visible = true;
 						}
 		   		}
				if(getField(%resultString,0)>0)
				{
					ForumShell.setTitle($ForumsGetPosts @ ": " @ getField(%resultString,0));
					%this.state = "PostList";
					if(!ForumsGui.visible)
						ForumsGui.setVisible(true);
				}
				else
				{
					%this.state = "done";
					ForumsMessageList.loadCache(%forumID);
				}
			case "postNews":
				%this.state = "done";
				messageBoxOK("CONFIRMED","Your News Reply has been submitted");
           case "newTopic":
               %this.state = "done";
               ForumsRefreshTopics();
		}
	}
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "error";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
	{
		%this.state = "error";
		MessageBoxOk("ERROR",getField(%status,1));
	}
	ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
   canvas.SetCursor(defaultCursor);
}
//-----------------------------------------------------------------------------
function ForumsGui::onDatabaseRow(%this,%row,%isLastRow,%key)
{
	if(%this.key != %key)
		return;
//	echo("RECV: " @ %row);
	%forumTID = getField(ForumsList.getRowTextbyId(ForumsList.getSelectedID()),2);
	switch$(%this.state)
	{
		case "ForumList":
			ForumsList.addRow(getField(%row,0),getField(%row,1) TAB getField(%row,2) TAB getField(%row,3));
//           error("ISLASTROW:" TAB %isLastRow);
			if ( %isLastRow ) //is last line
	   		{
               %ai = wonGetAuthInfo();
               for(%east=0;%east<getField(getRecord(%ai,1),0);%east++)
               {
                   %wonTribe = getRecord(%ai,2+%east);
                   ForumsList.addRow(getField(%wonTribe,3)*-1,getField(%wonTribe,0) TAB getField(%wonTribe,4) TAB getField(%wonTribe,2));
               }
	      		if ( ForumsGui.launchForum !$= "" )
	      		{
	         		ForumsList.selectForum( ForumsGui.LaunchForum );
			   		ForumsGui.LaunchForum = "";
				}
	      		else
	         		ForumsList.setSelectedRow( 1 );
			}
		case "TopicList":
			%id = getField(%row, 1);
      		%topic = getField(%row, 2);
      		%postCount = getField(%row, 3);
      		%date = getField(%row, 6);
      		%name = getField(%row, 8);
	   		%hasDeletes = getField(%row,12);
           %slevel = getField(%row,13);
           %maxUpdateId = getField(%row,14);
 			ForumsTopicsList.addRow( %id, %topic TAB %postCount TAB %name TAB %date TAB %hasDeletes);
           ForumsTopicsList.addTopic( %id, %topic, %date, %maxUpdateID, %slevel, %row);
 			if ( %isLastRow ) //is last line
 	   		{
 				%this.state = "done";
 				ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
				ForumsTopicsList.updateReadStatus();
      			%this.refreshFlag = false;
 				if ( ForumsGui.LaunchTopic !$= "" )
 				{
 					ForumsTopicsList.selectTopic( ForumsGui.LaunchTopic );
 					ForumsGui.LaunchTopic = "";
 				}
               else if( ForumsMessageVector.tid !$= "" )
                   ForumsTopicsList.setSelectedbyId(ForumsMessageVector.tid);
 			}
		case "PostList":
				%isAuthor = getField(%row,0);
         		%postId = getField(%row,2);
            	%parent = getField(%row,3);
         		%high = getField(%row,4);
            	%poster = getFields(%row,5,8);
            	%date = getField(%row,10);
            	%topic = getField(%row,13);
            	%body = getFields(%row,14);
         		if ( %high > ForumsMessageList.highestUpdate )
            		ForumsMessageList.highestUpdate = %high;
            	if(%parent == %postId)
               		%parent = 0;

            	%text = 0 NL
                   		%postId NL
                   		%parent NL
                   		%topic NL
                   		%poster NL
                   		%date NL
						    %isAuthor NL
                   		%body;

            	%li = ForumsMessageVector.getLineIndexByTag(%postId);
            	if(%li != -1)
               		ForumsMessageVector.deleteLine(%li);

               if(ForumsMessageList.allRead && DateStrCompare(ForumsMessageList.lastDate,%date))
                   %text = setRecord( %text, 0, "1" );
                   
            	ForumsMessageVector.pushBackLine(%text, %postId);
				if(%isLastRow)
				{
                   ForumsMessageVector.tid = ForumsTopicsList.getSelectedID();
				    CacheForumTopic();
					ForumsMessageList.loadCache(ForumsTopicsList.getSelectedID());
				}
	}
}
//-----------------------------------------------------------------------------
//-- ForumsList --------------------------------------------------------------
//-----------------------------------------------------------------------------
function ForumsList::onSelect(%this)
{	
	if(isEventPending(ForumsGUI.eid))
		cancel(ForumsGui.eid);
	FM_NewTopic.setActive(true);
	ForumsComposeDlg.forum = ForumsList.getSelectedID();
	ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
	ForumsGui.eid = schedule(250,ForumsGui,GetTopicsList);
}
//-----------------------------------------------------------------------------
function ForumsList::connectionTerminated( %this, %key )
{
	ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
   if ( %key != %this.key )
      return;
}
//-----------------------------------------------------------------------------
function ForumsList::selectForum( %this, %forum )
{
  %rowCount = %this.rowCount();
  for ( %row = 0; %row < %rowCount; %row++ )
  {
     if ( %forum $= getField( %this.getRowText( %row ), 0 ) )
     {
        %this.setSelectedRow( %row );
        break;
     }
  }
  if ( %row == %rowCount )
     warn( "\"" @ %forum @ "\" forum not found!" );
}
//-----------------------------------------------------------------------------
//-- ForumsTopicsList --------------------------------------------------------
//-----------------------------------------------------------------------------
function ForumsTopicsList::onAdd( %this )
{
   new GuiControl(TopicsPopupDlg) {
      profile = "GuiModelessDialogProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";

      new ShellPopupMenu( TopicsPopupMenu ) {
         profile = "ShellPopupProfile";
         position = "0 0";
         extent = "0 0";
         minExtent = "0 0";
         visible = "1";
         maxPopupHeight = "200";
         noButtonStyle = "1";
      };
   };

   // Add the columns from the prefs:
   for ( %i = 0; %i < $TopicColumnCount; %i++ )
   {
      %this.addColumn( %i,
                       $TopicColumnName[%i],
                       $pref::Topics::Column[%i],
                       firstWord( $TopicColumnRange[%i] ),
                       getWord( $TopicColumnRange[%i], 1 ),
                       $TopicColumnFlags[%i] );
   }

   %this.setSortColumn( $pref::Topics::SortColumnKey );
   %this.setSortIncreasing( $pref::Topics::SortInc );
   // Add the "Unread" style:
   %this.addStyle( 1, $ShellBoldFont, $ShellFontSize, "80 220 200", "30 255 225", "10 60 40" );
   // Add the "Ignored" style:
   %this.addStyle( 2, $ShellFont, $ShellFontSize, "100 100 100", "100 100 000", "100 100 100" );
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::AddTopic(%this, %id, %topicname, %date, %mid, %slevel, %vline)
{
   if(!isObject(TopicsListGroup))
       new SimGroup(TopicsListGroup);
   %topic = new scriptObject()
   {
      className = "TTopic";
      Id = %id;
      name = %topicname;
      date = %date;
      updateid = %mid;
      slevel = %slevel;
	   rcvrec = %vline;
   };
   TopicsListGroup.Add(%topic);
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::ClearList()
{
	if(isObject(TopicsListGroup))
		TopicsListGroup.Delete();
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::onRightMouseDown( %this, %column, %row, %mousePos )
{  
   ForumsTopicsList.setSelectedRow(%row);
	TopicsPopupMenu.topic = TopicsListGroup.getObject(%row);
   if ( trim(TopicsPopupMenu.topic.name) !$= "")
   {
      Canvas.pushDialog(TopicsPopupDlg);
      TopicsPopupMenu.position = %mousePos;
	   TopicsPopupDlg.onWake();
	   TopicsPopupMenu.forceOnAction();
   }
   else
      error( "Locate Error!" );
}
//-----------------------------------------------------------------------------
function TopicsPopupDlg::onWake( %this )
{
	ForumsGui.TDialogOpen = true;
   TopicsPopupMenu.clear();
	TopicsPopupMenu.add( TopicsPopupMenu.topic.name SPC ": RESET CACHE", 0 );
   TopicsPopupMenu.add( TopicsPopupMenu.topic.name SPC ": IGNORE THIS TOPIC",1);
   TopicsPopupMenu.add( TopicsPopupMenu.topic.name SPC ": FLAG ALL READ",2);
   Canvas.rePaint();      
}
//-----------------------------------------------------------------------------
function TopicsPopupMenu::onSelect( %this, %id, %text )
{
//   echo("TPM RECV: " @ %id TAB %text);
   switch( %id )
   {
      case 0: //	0 Reset Cache              
               ForumsMessageVector.clear();
               ForumsMessageVector.dump( $ForumCachePath @ "tpc" @ TopicsPopupMenu.topic.id , 0 TAB $ForumCacheVersion TAB 0 TAB "2000-12-31 12:01am");
               ForumsMessageVector.updateID = 0;
               ForumsTopicsList.UpdateReadStatus();
//               ForumsRefreshTopics();
      case 1: //	1 Flag TO IGNORE (ok, 45 years...)
               ForumsMessageVector.clear();
               ForumsMessageVector.dump( $ForumCachePath @ "tpc" @ TopicsPopupMenu.topic.id , 99999999 TAB $ForumCacheVersion TAB 1 TAB "2045-12-31 12:01am");
               ForumsMessageVector.updateID = 99999999;
               ForumsTopicsList.UpdateReadStatus();
//               ForumsRefreshTopics();
      case 2: //  2 Flag To ALL Read              
               ForumsMessageVector.updateID = 99999999;
               %cacheFile = $ForumCachePath @ "tpc" @ TopicsPopupMenu.topic.id;
               if(ForumsMessageVector.tid == TopicsPopupMenu.topic.id)
               {
                   new MessageVector(TempVector);
                   %numLines = ForumsMessageVector.getNumLines();
                   for(%x=0;%x<%numLines;%x++)
                   {
                       %lineText = ForumsMessageVector.getLineText( %x );
                       %postID = getRecord( %lineText, 1 );
                       %lineText = setRecord(%lineText,0,"1");
                       TempVector.pushBackLine(%lineText, %postID);                       
                   }
                   ForumsMessageVector.clear();
	                for ( %line = 0; %line < %numLines; %line++ )
                   {
                       %lineText = TempVector.getLineText( %line );
                       %postID = getRecord( %lineText, 1 );
                       ForumsMessageVector.pushBackLine( %lineText, %postID );
                   }
                   TempVector.delete();
                   TopicsPopupMenu.topic.updateId = 0;
                   CacheForumTopic();
                   ForumsTopicsList.UpdateReadStatus();
//                   ForumsRefreshTopics();
               }               
               else
               {               
                   ForumsMessageVector.clear();
                   ForumsMessageVector.tid = TopicsPopupMenu.topic.id;
                   %file = new FileObject();
                   if ( %file.openForRead( %cacheFile ) )
                   {
                      if ( !%file.isEOF() )
                      {
                         // First line is the update id:
                         %line = %file.readLine();
                         if ( getField( %line, 1 ) == $ForumCacheVersion )
                         {
                            if ( !%file.isEOF() )
                            {
                               // Second line is the message count:
                               %count = %file.readLine();

                               // Now push all of the messages into the message vector:
                                while ( !%file.isEOF() )
                                {
					                %line = %file.readLine();
                	                %postId = firstWord( %line );
                	                %text = collapseEscape( restWords( %line ) );
					                // RESET THE FIELDS IF THE POST IS BEING VISITED BY THE AUTHOR.
                                   %text = setRecord( %text, 0, "1" );                                   
                	                ForumsMessageVector.pushBackLine( %text, %postId );
                                }
                            }
                         }
                         else
                         {
                         }
                      }
                      else
                      {
                      }
                      %file.close();
                      TopicsPopupMenu.topic.updateId = 0;
                      CacheForumTopic();
                   }
                   else
                   {
                      ForumsMessageVector.clear();
                      %latest = GetField(ForumsTopicsList.getRowText(ForumsTopicsList.getSelectedRow()),3);
                      %newGroup = TopicsListGroup.getObject(ForumsTopicsList.getSelectedRow());
//                      ForumsMessageList.lastID = %newGroup.updateid;
                      ForumsMessageList.lastID = 0;
                      ForumsMessageVector.dump( %cacheFile , 0 TAB $ForumCacheVersion TAB 1 TAB %latest);
                   }
                   %file.delete();
                   ForumsTopicsList.UpdateReadStatus();
//                   ForumsRefreshTopics();
               }
   }           
   canvas.popDialog(TopicsPopupDlg);
}
//-----------------------------------------------------------------------------
function TopicsPopupDlg::onSleep(%this)
{
	ForumsGui.TDialogOpen = false;
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::onSetSortKey( %this, %sortKey, %isIncreasing )
{
   $pref::Topics::SortColumnKey = %sortKey;
   $pref::Topics::SortInc = %isIncreasing;
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::onColumnResize( %this, %column, %newSize )
{
   $pref::Topics::Column[%column] = %newSize;
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::onSelect(%this)
{
	if(isEventPending(ForumsGUI.eid))
		cancel(ForumsGui.eid);
 	ForumsComposeDlg.topic = %this.getSelectedID();
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::selectTopic( %this, %topic )
{
  %rowCount = %this.rowCount();
  for ( %row = 0; %row < %rowCount; %row++ )
  {
     if ( %topic $= getField( %this.getRowText( %row ), 0 ) )
     {
        %this.setSelectedById( %this.getRowId( %row ) );        
        break;
     }
  }
   if(%this.getSelectedID > -1)
       getTopicPosts();
   else
       if ( %row == %rowCount )
           warn( "\"" @ %topic @ "\" Topic not found!" );
}
//-----------------------------------------------------------------------------
function ForumsTopicsList::updateReadStatus( %this )
{
   for ( %row = 0; %row < %this.rowCount(); %row++ )
   {
      %style = 1; // unread
      %cacheFile = $ForumCachePath @ "tpc" @ %this.getRowId( %row );
      %file = new FileObject();
      if ( %file.openForRead( %cacheFile ) )
      {
         %header = %file.readLine();
         %topicDate = getField( %this.getRowText( %row ), 3 );
         %updateID = getField(%header,0);
         if ( getField( %header, 1 ) == $ForumCacheVersion        // Must have same cache version
              && getField( %header, 2 ) == 1                         // "all read" flag must be set
              && strcmp( getField( %header, 3 ), %topicDate ) >= 0
              && %updateID !$= "99999999" ) // date must be current
           %style = 0; // read
		  else if (%updateID $= "99999999")
           %style = 2; //ignored
         else
			%style = 1;
      }
      %file.delete();
      %this.setRowStyle( %row, %style );
   }
}
//-----------------------------------------------------------------------------
//-- ForumsMessageList -------------------------------------------------------
//-----------------------------------------------------------------------------
function ForumsMessageList::onAdd( %this )
{
   // Add columns from the prefs:
   for ( %i = 0; %i < $ForumColumnCount; %i++ )
      %this.addColumn( %i,
                       $ForumColumnName[%i],
                       $pref::Forum::Column[%i],
                    	firstWord( $ForumColumnRange[%i] ),
                    	getWord( $ForumColumnRange[%i], 1 ) );
   // We want no sorting done on this list -- leave them in the order that they are entered.
   // Add the "Unread" style:
   %this.addStyle( 1, $ShellBoldFont, $ShellFontSize, "80 220 200", "30 255 225", "0 0 0" );
}
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
function ForumsMessageList::connectionTerminated(%this, %key)
{
	ForumShell.setTitle("FORUMS: " @ getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),0));
}
//-----------------------------------------------------------------------------
function ForumsMessageList::loadCache( %this, %forumTID)
{
   ForumsMessageVector.clear();
   ForumsMessageList.clear();
   ForumsMessageVector.tid = %forumTID;
 	switch( ForumsGui.bflag )
 	{
 		case 0: 
 		FO_RejectBtn.visible = false;
 		FO_EditBtn.visible = false;
 		case 1:
 		FO_RejectBtn.visible = false;
 		FO_EditBtn.visible = false;
 		case 2:
 		switch(%forumTID)
 		{
 			case 1402:
   				FO_AcceptBtn.visible = true;
   				FO_RejectBtn.text = "REJECT";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
   				FO_AcceptBtn.visible = true;
 			default:
   				FO_RejectBtn.text = "DELETE";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
 		}
 		case 3:
 		switch(%forumTID)
 		{
 			case 1402:
   				FO_AcceptBtn.visible = true;
   				FO_RejectBtn.text = "REJECT";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
   				FO_AcceptBtn.visible = true;
 			default:
   				FO_RejectBtn.text = "DELETE";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
 		}
 		case 4:
 		switch(%forumTID)
 		{
 			case 1402:
   				FO_AcceptBtn.visible = true;
   				FO_RejectBtn.text = "REJECT";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
   				FO_AcceptBtn.visible = true;
 			default:
   				FO_RejectBtn.text = "DELETE";
   				FO_RejectBtn.visible = true;
   				FO_EditBtn.visible = true;
 		}
 	}
    
   %this.lastId = 0;
   %this.highestUpdate = %this.lastID;
   %cacheFile = $ForumCachePath @ "tpc" @ ForumsComposeDlg.topic;
   %file = new FileObject();
   if ( %file.openForRead( %cacheFile ) )
   {
//   %newGroup = TopicsListGroup.getObject(ForumsTopicsList.getSelectedRow());
      if ( !%file.isEOF() )
      {
         // First line is the update id:
         %line = %file.readLine();
         if ( getField( %line, 1 ) == $ForumCacheVersion )
         {
            %this.lastID = getField(%line,0);
            %this.highestUpdate = %this.lastID;
            %this.allRead = getField(%line,2);
            %this.lastDate = getField(%line,3);
            if ( !%file.isEOF() )
            {
               // Second line is the message count:
               %line = %file.readLine();
               %count = getField( %line, 0 );

               // Now push all of the messages into the message vector:
                while ( !%file.isEOF() )
                {
					%line = %file.readLine();
                	%postId = firstWord( %line );
                	%text = collapseEscape( restWords( %line ) );
                   %date = getRecord(%text,5);
                   %isRead = getRecord(%text,0);

					// RESET THE FIELDS IF THE POST IS BEING VISITED BY THE AUTHOR.
				 	%ref = getRecord(%text,6);
					if(%ref > 1)
					{
						switch(%forumTID)
						{
							case 1402:
	      						FO_AcceptBtn.visible = false;
		      					FO_RejectBtn.text = "REJECT";
		      					FO_RejectBtn.visible = false;
			      				FO_EditBtn.visible = false;
		    	  				FO_AcceptBtn.visible = false;
							default:
		      					FO_RejectBtn.text = "DELETE";
		      					FO_RejectBtn.visible = true;
	    	  					FO_EditBtn.visible = true;
						}
			   		}
                   if(%this.allRead && DateStrCompare(%this.lastDate,%date))
                       %text = setRecord( %text, 0, "1" );
                       
//                	echo( "** ADDING MESSAGE FROM CACHE - " @ %postId @ " **" );
                	ForumsMessageVector.pushBackLine( %text, %postId );
               }
            }
         }
      }
   }
   %file.delete();

   %numLines = ForumsMessageVector.getNumLines();
   for(%x=0;%x<%numLines;%x++)
   {
       %lineText = ForumsMessageVector.getLineText( %x );
  	    ForumsMessageAddRow( %lineText );
   }
   if(ForumsMessageList.getSelectedId() == -1)
       ForumsMessageList.setSelectedRow(0);
}
//-----------------------------------------------------------------------------
function ForumsMessageList::onColumnResize( %this, %column, %newSize )
{
   $pref::Forum::Column[%column] = %newSize;
}
//-----------------------------------------------------------------------------
function ForumsMessageList::onSelect(%this, %id, %text)
{

	if(!ForumsMessageList.getSelectedID())
		%parentId = 0;
	else
		%parentId = ForumsMessageList.getSelectedId();

   ForumsComposeDlg.parentPost = %parentId;
   %rawText = ForumsMessageVector.getLineTextByTag( ForumsComposeDlg.parentPost );
	%offSet = 7;
   if ( getRecord( %rawText, 0 ) $= "0" )
   {
      // Set the "read" flag:
      %this.setRowStyleById( %id, 0 );
      %line = ForumsMessageVector.getLineIndexByTag( %parentId );
      ForumsMessageVector.deleteLine( %line );
      %rawText = setRecord( %rawText, 0, "1" );
      ForumsMessageVector.pushBackLine( %rawText, %parentId );
      CacheForumTopic();
   }
	%text = ForumsGetTextDisplay( %rawText,%offSet );
	ForumsText.setValue(%text);
	FO_ReplyBtn.setActive( true );
	FO_NextBtn.setActive( true );
	FO_PreviousBtn.setActive( true );
 	%ref = getRecord(%rawText,6);
	if(%ref > 0 && %ref > ForumsGui.bflag) //if this is the author
	{
		switch(getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),2))
		{
			case 1402:
	    		FO_AcceptBtn.visible = false;
		    	FO_RejectBtn.text = "REJECT";
		    	FO_RejectBtn.visible = false;
				FO_EditBtn.visible = false;
		    	FO_AcceptBtn.visible = false;
			default:
		    	FO_RejectBtn.text = "DELETE";
		    	FO_RejectBtn.visible = true;
	    		FO_EditBtn.visible = true;
		}
	}
	else if(ForumsGui.bflag == 0)
	{
		switch(getField(ForumsList.getRowTextbyID(ForumsList.getSelectedID()),2))
		{
			case 1402:
	    		FO_AcceptBtn.visible = false;
		    	FO_RejectBtn.text = "REJECT";
		    	FO_RejectBtn.visible = false;
				FO_EditBtn.visible = false;
		    	FO_AcceptBtn.visible = false;
			default:
		    	FO_RejectBtn.text = "DELETE";
		    	FO_RejectBtn.visible = false;
	    		FO_EditBtn.visible = false;
		}
	}

}
//-----------------------------------------------------------------------------
function ForumsMessagelist::onDatabaseQueryResult(%this,%status,%resultString,%key)
{
	if(%this.key != %key)
		return;
//	echo("RECV: " @ %status TAB %resultString);
	if(getField(%status,0)==0)
	{
		switch$(%this.state)
		{
			case "replyPost":
               %this.state = "done";
				ForumsOpenThread();
			case "editPost":
               %this.state = "done";
          		%postId = getField( %status, 2 );
           	%index = ForumsMessageVector.getLineIndexByTag( %postId );
           	%text = ForumsMessageVector.getLineTextByTag( %postId );
           	%parent = getRecord( %text, 2 );
          	    ForumsMessageVector.deleteLine( %index );
               %text = setRecord(%text,0,"1");
               ForumsMessageVector.pushBackLine(%text, %postID);
      		    CacheForumTopic();
               GetTopicPosts();
			case "deletePost":
				%this.state = "done";
          		%postId = getField( %status, 2 );
           	%index = ForumsMessageVector.getLineIndexByTag( %postId );
           	%text = ForumsMessageVector.getLineTextByTag( %postId );
           	%parent = getRecord( %text, 2 );
 		    	ForumsTopicsList.refreshFlag = true;
          	    ForumsMessageVector.deleteLine( %index );
      		    CacheForumTopic();
//               ForumsMessageList.clear();
//               ForumsMessageList.loadCache();
            	if ( %parent != 0 )
            	{
               	    %row = ForumsMessageList.getRowNumById( %postId );
               	    ForumsMessageList.removeRowById( %postId );
               	    if ( %row < ForumsMessageList.rowCount() )
                        ForumsMessageList.setSelectedRow( %row );
               	    else
                	    ForumsMessageList.setSelectedRow( %row - 1 );
 
                    %this.state = "done";
 				    ForumsOpenThread();
            	}
            	else
            	{
   			  	    ForumsTopicsList.refreshFlag = true;
                    ForumsGoTopics(0);
            	} 													   
		}
	}
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "error";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
	{
		MessageBoxOK("ERROR",getFields(%status,1));
	}
   canvas.SetCursor(DefaultCursor);
}
