$WebLinkCount = 0;
$currentPage = 0;
function LaunchNews()
{
   LaunchTabView.viewTab( "NEWS", NewsGui, 0 );
}
//-----------------------------------------------------------------------------
function updatePageBtn(%prev,%next)
{		
   NewsPrevBtn.setVisible( 0 );
   NewsNextBtn.setVisible( 0 );
//   NewsPrevBtn.setActive( %prev );
//   NewsNextBtn.setActive( %next );
}
//-----------------------------------------------------------------------------
function NewsGui::onWake(%this)
{
   Canvas.pushDialog(LaunchToolbarDlg);
   %this.articleCount = 0;
   %this.set = 1; // signifies the first (latest) set
   NewsText.setValue("");
	if ( isDemo() )
	{
      NewsPrevBtn.setVisible( false );
      NewsNextBtn.setVisible( false );
		NewsSubmitBtn.setVisible( false );
		NewsMOTDText.setValue( "Welcome to the Tribes 2 Demo!" );
		%this.addStaticArticle( "What's In The Demo?", "There are two training missions, and two multiplayer maps in this demo.\nIf you're new to the Tribes experience, you should consider trying out the training missions so you can become familiar with the basics of the game. Then, after you learn how to use your jets and weapons, jump into multiplayer on one of our Demo Servers to fight against other players like yourself." );
		%this.addStaticArticle( "How Do I Change Settings?", "There is a LAUNCH button in the lower left of this screen. Click on it and choose the SETTINGS option. There you will find ways to modify your graphics, textures, network settings and more. \nMost of these settings will be configured automatically based on your hardware configs, so try the game with the default settings for a while and see how it plays. Then, optimize your settings accordingly thereafter." );
//		%this.addStaticArticle( "Two Game Types", "Talk about CTF and Hunters" );
	}
	else
	{
	   	Canvas.SetCursor(ArrowWaitCursor);
   		%this.state = "status";
	   	%this.key = LaunchGui.key++;
	    %this.caller = "GETNEWS";
	    DatabaseQueryArray(0,0,"0" TAB "0",%this,%this.key);
	    // Fetch the message of the day:
	    NewsMOTDText.key = LaunchGui.key++;
	    NewsMOTDText.state = "isvalid";
	    DatabaseQuery(0,"",NewsMOTDText,NewsMOTDText.key);
		weblinksmenu.clear();
		weblinksmenu.key = launchgui.key++;
		weblinksmenu.state = "fetchWeblink";
		DatabaseQueryArray(15,0,"WEBLINK",weblinksmenu,weblinksmenu.key);
	}
   WebLinksMenu.setSelected( 0 );
   NewsPrevBtn.setActive( false );
   NewsNextBtn.setActive( false );
}
//-----------------------------------------------------------------------------
function NewsGui::onSleep(%this)
{
   Canvas.popDialog(LaunchToolbarDlg);
}
//-----------------------------------------------------------------------------
function NewsGui::setKey( %this, %key )
{
}
//-----------------------------------------------------------------------------
function NewsGui::onClose( %this, %key )
{
}
//-----------------------------------------------------------------------------
function NewsGui::rebuildText(%this)
{
	NewsHeadlines.clear();
	for(%i = 0; %i < %this.articleCount; %i++)
	{	  
   	%article = %this.article[%i];

		if ( isDemo() )
		{
			%topic = getField( %article, 0 );
			%body = getFields( %article, 1 );

			%text = %text @ "<lmargin:10><color:adfffa><font:univers:22><tag:" @ %i @ ">"
					@ %topic
					@ "<lmargin:30><rmargin%:80><color:82beb9><font:univers:16>\n"
					NL %body
					@ "<sbreak>\n\n<rmargin%:100>";
		}
		else
		{
			%ai = wonGetAuthInfo();
			%isMem = 0;
			for(%east=0;%east<getField(getRecord(%ai,1),0);%east++)
			{
				%tpv = GetRecord(%ai,2+%east);
				if(getField(%tpv,3)==1401 || getField(%tpv,3)==1402)
					%isMem = 1;
			}
	   	%editable = %isMem;

      	%topicid = getField(%article,1);
      	%articleid = getField(%article, 2);
	  		%postcount = getField(%article,3)-1;
      	%date = getField(%article,4);
	  		%update_id = getField(%article,5);
	  		%author_id = getField(%article,6);
      	%nameLink = getLinkName(getField(%article,8) TAB getField(%article,9) TAB getField(%article,10) TAB getField(%article,11),0);
      	%category = getField(%article, 12);
      	%topic = getField(%article, 13);
	  		%body = getFields(%article,14);
      	%rc = getRecordCount(%body);
      	%atxt = "";
      	if ( %editable )
         	%editText = "<a:editnews" TAB %i TAB %topicid TAB %articleid TAB %update_id @
		     		         ">[edit]</a> <a:deletenews" TAB %i TAB %articleid TAB %topicid @
		     		         ">[delete]</a> <a:forumlink" TAB "NEWS" TAB %articleid TAB %topic @
		     		         ">[comments ("@%postcount@")]</a>";
      	else
         	%editText = "<a:forumlink" TAB "NEWS" TAB %articleid TAB %topic @
		     					">[comments ("@%postcount@"]</a>";

      	for(%l = 0; %l < %rc; %l++)
         	%atxt = %atxt @ getRecord(%body,%l) @ "\n";

      	%text = %text @ "<lmargin:10><color:ADFFFA><font:Univers:22><tag:" @ %i @ ">" @
                 %topic @ " <font:Univers Condensed:18>" @ %editText @ "\nPosted by: " @ %nameLink @ " on " @ %date NL
                 "\n<lmargin:30><rmargin%:80><font:Univers:16><color:82BEB9>" @ %atxt @ "<sbreak>\n\n<rmargin%:100>";
		}

      NewsHeadlines.addRow( %i, %topic );
   }
   NewsText.setValue(%text);
}
//-----------------------------------------------------------------------------
function NewsGui::onDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
    	return;
//	echo("RECV: " @ %status);
   	%this.maxDate = " ";
	%this.minDate = " ";
	if(getField(%status,0)==0)
	{
		switch$(%this.caller)
		{
			case "GETNEWS": // record count
		   			%this.acl = getField(%status,3);
					%this.ttlRecords = getField( %status,2 );
					%this.recordCount = getField( %RowCount_Result,0 );
					if(%this.recordCount > 23)
					{
						if($currentPage == 0)
							updatePageBtn(0,1);
						else if($currentPage > 0)
							updatePageBtn(1,1);
					}
					else
					{
						if($currentPage == 0)
							updatePageBtn(0,0);
						else if($currentPage > 0)
							updatePageBtn(1,0);
					}
						
			default: // result string
		}
	}
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "error";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
	{
    	%this.state = "halt";		
  	     MessageBoxOK("NOTICE",getField(%status,1));
	}
	Canvas.setCursor(DefaultCursor);
}
//-----------------------------------------------------------------------------
function NewsGui::onDatabaseRow(%this, %row,%isLastRow,%key)
{
   	if ( %key != %this.key )
    	return;
//	echo("RECV: " @ %row);
	%this.article[%this.articleCount] = %row;
	%this.recordSet--;
	if ( %this.articleCount == 0 )
	{
		%this.maxDate = getField( %this.article[%this.articleCount],4);
     	%this.Max = getField(%this.article[%this.articleCount],5);
	}
			
	if ( %this.recordSet == 0 )
	{
		%this.minDate = getField( %this.article[%this.articleCount],4);
     	%this.Min = getField(%this.article[%this.articleCount],5);
	}
	%this.recordCount--;
	%this.articleCount++;
	%this.rebuildText();
	canvas.setCursor(DefaultCursor);
	return;
}
//-----------------------------------------------------------------------------
function NewsGui::addStaticArticle( %this, %topic, %body )
{
	%tag = %this.articleCount;
	%this.article[%tag] = %topic TAB %body;
	%this.articleCount++;
	%this.rebuildText();
}
//-----------------------------------------------------------------------------
function PostNews()
{
	messageBoxYesNo("CONFIRM","Please do not submit bug reports without a tested solution, test posts or recruiting posts." NL " " NL "Continue with your submittal?","StartPostNews();");
}
function StartPostNews()
{
   $NewsCategory = "";
   $NewsTitle = "";
   Canvas.pushDialog(NewsPostDlg);
   NewsPostDlg.postId = -1;
   NewsPostBodyText.setValue("");
}
//-----------------------------------------------------------------------------
function NewsPostDlg::onWake( %this )
{
   // Get the window pos and extent from prefs:
   %res = getResolution();
   %resW = firstWord( %res );
   %resH = getWord( %res, 1 );
   %w = firstWord( $pref::News::PostWindowExtent );
   if ( %w > %resW )
      %w = %resW;
   %h = getWord( $pref::News::PostWindowExtent, 1 );
   if ( %h > %resH )
      %h = %resH;
   %x = firstWord( $pref::News::PostWindowPos );
   if ( %x > %resW - %w )
      %x = %resW - %w;
   %y = getWord( $pref::News::PostWindowPos, 1 );
   if ( %y > %resH - %h )
      %y = %resH - %h;
   NP_Window.resize( %x, %y, %w, %h );

   // Fill the category menu (should we get this from somewhere?):
   NewsCategoryMenu.clear();
   NewsCategoryMenu.add( "General", 0 );
   NewsCategoryMenu.add( "Announcements", 1 );
   NewsCategoryMenu.add( "Events", 2 );
   NewsCategoryMenu.add( "Updates", 3 );
   if ( $NewsCategory !$= "" )
   {
      %selId = NewsCategoryMenu.findText( $NewsCategory );
      if ( %selId <= 0 )
         %selId = 1;
   }
   else
      %selId = 1;
   NewsCategoryMenu.setSelected( %selId );
}
//-----------------------------------------------------------------------------
function NewsPostDlg::onSleep( %this )
{
   $pref::News::PostWindowPos = NP_Window.getPosition();
   $pref::News::PostWindowExtent = NP_Window.getExtent();
}
//-----------------------------------------------------------------------------
function NewsCategoryMenu::onSelect( %this, %id, %text )
{
   $NewsCategory = %text;
}
//-----------------------------------------------------------------------------
function PostNewsProcess()
{

	NewsPostDlg.key = LaunchGui.key++;
	if ( NewsPostDlg.postId == -1 )
	{
		NewsPostDlg.state = "post";
 	    canvas.SetCursor(ArrowWaitCursor);
      	DatabaseQuery(1, NewsCategoryMenu.getSelected() TAB $NewsTitle TAB NewsPostBodyText.getValue(),NewsPostDlg,NewsPostDlg.key);
	}
	else
	{
		NewsPostDlg.state = "edit"; //tid.pid.aid.updid.cat.title.body
 	    canvas.SetCursor(ArrowWaitCursor);
		DatabaseQuery(2, NewsPostDlg.EditUrl TAB NewsCategoryMenu.getSelected() TAB $NewsTitle TAB NewsPostBodyText.getValue(),NewsPostDlg,NewsPostDlg.key);
	}
   Canvas.popDialog(NewsPostDlg);
}
//-----------------------------------------------------------------------------
function NewsPostDlg::onDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
		return;
//	echo("RECV: " @ %status);
	if(getField(%status,0)==0)
	{
		switch$(%this.state)
		{
			case "post":	 			
				if(%this.FromForums)
				{
					%this.FromForums = false;
					ForumsMessageVector.deleteLine( %this.FIndex );
					ForumsTopicsList.refreshFlag = true;
					CacheForumTopic();
					ForumsGoTopics(0);
				}
				%this.state = "OK";
     		 	MessageBoxOK("NOTICE","Your article has been submitted. You may need to refresh your Browser.");

			case "edit":				
				%this.state = "OK";
     		 	MessageBoxOK("NOTICE","Article Updated. You may need to refresh your Browser.");      
			case "delete":	 			
				%this.state = "OK";
				MessageBoxOK("NOTICE","Article Deleted. You may need to refresh your Browser.");
		}
	}
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "error";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
	{
		%this.state = "ERROR";
		MessageBoxOK( "ERROR", getField(%status,1));
	}

	if(%this.state $= "OK")
    	NewsGui.onWake();

	canvas.setCursor(DefaultCursor);
}
//-----------------------------------------------------------------------------
function NewsText::onURL(%this, %url)
{
   canvas.SetCursor(ArrowWaitCursor);
   switch$(getField(%url,0))
   {
      case "editnews":
         %txt = NewsGui.article[getField(%url,1)];
         $NewsCategory = getField(%txt, 12);
         $NewsTitle = getField(%txt, 13);
		 %body = getFields(%txt,14);
         %rc = getRecordCount(%body);
         for(%i = 0; %i < %rc; %i++)
            %rtxt = %rtxt @ getRecord(%body, %i) @ "\n";

         NewsPostBodyText.setValue(%rtxt);
         NewsPostDlg.postId = getField(%url,2);
		 NewsPostDlg.EditUrl = getFields(%url,2);
         Canvas.pushDialog( NewsPostDlg );

      case "deletenews":
		 MessageBoxYesNo("CONFIRM","Delete this Article?","NewsPostDlg.doNewsDelete(\"" @ getField(%url,2) TAB getField(%url,3) @ "\");","canvas.setCursor(DefaultCursor);");

      case "topiclink":
        %articleId = getField(%url,2);
	    MessageBoxOK("COMMENTS","To Post comments for any news article, go to the Forums, select the News Forum and then select the Topic you want to comment on.  This link will soon be live.");

      default:
         Parent::onURL(%this, %url);
   }
}
//-----------------------------------------------------------------------------
function NewsPostDlg::doNewsDelete(%this,%fields)
{
	%this.key = LaunchGui.key++;
	%this.state = "delete";
	DatabaseQuery(3,%fields, %this, %this.key);
}
//-----------------------------------------------------------------------------
function NewsHeadlines::onSelect( %this, %id, %text )
{
   NewsText.scrollToTag( %id );
}
//-----------------------------------------------------------------------------
function NewsGui::getPreviousNewsItems( %this )
{
   // Fetch the next batch of newer news items:
   if($currentPage == 0)
	return;
   canvas.SetCursor(ArrowWaitCursor);
   NewsGui.key = LaunchGui.key++;
   %this.state = "status";
   %this.articleCount = 0;
   $currentPage--;
   DatabaseQueryArray(0,$currentPage,"1" TAB getField(%this.setList[%this.set],1), NewsGui, NewsGui.key );
}
//-----------------------------------------------------------------------------
function NewsGui::getNextNewsItems( %this )
{
   // Fetch the next batch of older news items:
   canvas.SetCursor(ArrowWaitCursor);
   NewsGui.key = LaunchGui.key++;
   %this.state = "status";
   %this.articleCount = 0;
   $currentPage++;
   DatabaseQueryArray(0,$currentPage,"1" TAB getField(%this.setList[%this.set],0), NewsGui, NewsGui.key );
}
//-----------------------------------------------------------------------------
function NewsMOTDText::onDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
    	return;
//	echo("RECV: " @ %status);
	%ai = wonGetAuthInfo();
	%isMem = 0;
	if(getField(%status,0)==0)
	{
		for(%east=0;%east<getField(getRecord(%ai,1),0);%east++)
		{
			%tpv = GetRecord(%ai,2+%east);
			if(getField(%tpv,3)==1401 || getField(%tpv,3)==1402)
				%isMem = 1;
		}
      	NewsEditMOTDBtn.setVisible(%isMem);
   		%this.setText( %RowCount_Result );
	}
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "error";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
	{
    	%this.state = "halt";		
  	     MessageBoxOK("NOTICE",getField(%status,1));
	}
	canvas.repaint();
	Canvas.setCursor(defaultCursor);
}
//-----------------------------------------------------------------------------
function NewsEditMOTD()
{
   NewsEditMOTDText.setText( NewsMOTDText.getText() );
   Canvas.pushDialog( NewsEditMOTDDlg );
}
//-----------------------------------------------------------------------------
function NewsUpdateMOTD()
{
   NewsEditMotdDlg.Key = LaunchGui.key++;
   NewsEditMOtdDlg.state = "verify";
   canvas.SetCursor(ArrowWaitCursor);
   DatabaseQuery( 4, NewsEditMOTDText.getText(), NewsEditMotdDlg,NewsEditMotdDlg.key );
}
//-----------------------------------------------------------------------------
function NewsEditMotdDlg::OnDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
    	return;
//	echo("RECV: " @ %status);
	if(getField(%status,0)==0)
		%this.state = "proceed";
	else if (getSubStr(getField(%status,1),0,9) $= "ORA-04061")
	{
		%this.state = "done";
		MessageBoxOK("ERROR","There was an error processing your request, please wait a few moments and try again.");
	}
	else
    	%this.state = "error";		

	switch$(%this.state)
	{
		case "proceed":
			%ai = wonGetAuthInfo();
			%isMem = 0;
			for(%east=0;%east<getField(getRecord(%ai,1),0);%east++)
			{
				%tpv = GetRecord(%ai,2+%east);
				if(getField(%tpv,3)==1401 || getField(%tpv,3)==1402)
					%isMem = 1;
			}
	      	NewsEditMOTDBtn.setVisible(%isMem);			
   			MessageBoxOK("NOTICE",getField(%status,1));
			NewsMOTDText.setText( NewsEditMOTDText.getText() );
			canvas.repaint();
			Canvas.PopDialog(NewsEditMOTDDlg);
		case "error":
   			MessageBoxOK("NOTICE",getField(%status,1));
			Canvas.PopDialog(NewsEditMOTDDlg);
	}
}
//-----------------------------------------------------------------------------
function addWebLink( %name, %address )
{
   $WebLink[$WebLinkCount, name] = %name;
   $WebLink[$WebLinkCount, address] = %address;
   $WebLinkCount++;
}
//-----------------------------------------------------------------------------
function WebLinksMenu::onAdd( %this )
{
}
//-----------------------------------------------------------------------------
function WebLinksMenu::launchWebBrowser( %this )
{
   %address = $WebLink[WebLinksMenu.getSelected(), address];
   error("SELECTED:" SPC WebLinksMenu.getSelected());
   if ( %address !$= "" )
   {
      if ( isFullScreen() )
         MessageBoxYesNo( "WARNING", "This will launch your web browser and minimize Tribes 2."
               @ "  Do you wish to continue?", "gotoWebPage( \"" @ %address @ "\" );" );
      else
         gotoWebPage( %address );
   }
   else
      MessageBoxOK("ERROR","Invalid Link");
}
