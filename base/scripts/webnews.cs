$WebLinkCount = 0;

function LaunchNews()
{
   LaunchTabView.viewTab( "NEWS", NewsGui, 0 );
}
//-----------------------------------------------------------------------------
function NewsGui::onWake(%this)
{
   Canvas.SetCursor(ArrowWaitCursor);
   Canvas.pushDialog(LaunchToolbarDlg);
   %this.key = LaunchGui.key++;
   %this.state = "status";
   %this.articleCount = 0;
   %this.set = 1; // signifies the first (latest) set
   NewsText.setValue("");
   %this.caller = "GETNEWS";
   DatabaseQueryArray(0,0,"0" TAB "0",%this,%this.key);
   // Fetch the message of the day:
   NewsMOTDText.key = LaunchGui.key++;
   NewsMOTDText.state = "isvalid";
   NewsMOTDText.lineCount = 0;
   DatabaseQuery(0,"",NewsMOTDText,NewsMOTDText.key);
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
	  if(getField(%article,0)==1 || %this.acl > 2)
      	%editable = 1;
	  else
		%editable = 0;

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

      NewsHeadlines.addRow( %i, %topic );
      %text = %text @ "<lmargin:10><color:ADFFFA><font:Univers:22><tag:" @ %i @ ">" @
              %topic @ " <font:Univers Condensed:18>" @ %editText @ "\nPosted by: " @ %nameLink @ " on " @ %date NL
              "\n<lmargin:30><rmargin%:80><font:Univers:16><color:82BEB9>" @ %atxt @ "<sbreak>\n\n<rmargin%:100>";
   }
   	  NewsText.setValue(%text);
	  %article = "";
}
//-----------------------------------------------------------------------------
function NewsGui::onDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
    	return;
	echo("RECV: " @ %status);
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
					%this.pages = 0;
					// RecordSet = least of 24 or rows returned
					if(getField(%RowCount_Result,0) < 24)
						%this.recordSet = getField(%RowCount_Result,0);
					else
						%this.recordSet = 24;

					// Calculate the Number of Pages
					for( %temprec = %this.ttlRecords; %temprec > 0; %temprec -= 24 )
					{
						%this.pages++;
					}
					// SET = which page we are on
					if( %this.set == 1 && %this.pages >= 2 )
					{
						NewsNextBtn.setActive( true );
						NewsPrevBtn.setActive( false );
					}
					else if ( %this.set == %this.pages && %this.pages >= 2 )
					{
						NewsNextBtn.setActive( false );
						NewsPrevBtn.setActive( true );
					}
					else if (%this.set == %this.pages && %this.pages <= 1)
					{
						NewsNextBtn.setActive( false );
						NewsPrevBtn.setActive( false );						
					}
					else
					{
						NewsNextBtn.setActive( true );
						NewsPrevBtn.setActive( true );
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
	echo("RECV: " @ %row);
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
function PostNews()
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
	echo("RECV: " @ %status);
	if(getField(%status,0)==0)
	{
		switch$(%this.state)
		{
			case "post":	 			
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
	     NewsPostDlg.key = LaunchGui.key++;
		 NewsPostDlg.state = "delete";
         DatabaseQuery(3,getFields(%url,2), NewsPostDlg, NewsPostDlg.Key);

      case "topiclink":
        %articleId = getField(%url,2);
	    MessageBoxOK("COMMENTS","To Post comments for any news article, go to the Forums, select the News Forum and then select the Topic you want to comment on.  This link will soon be live.");

      default:
         Parent::onURL(%this, %url);
   }
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
   error("FETCHING PREV NEWS ITEMS");
   if ( %this.set == 1 )
      return;
   canvas.SetCursor(ArrowWaitCursor);
   NewsGui.key = LaunchGui.key++;
   %this.state = "status";
   %this.articleCount = 0;
   %this.set--;
   DatabaseQueryArray(0,0,"1" TAB getField(%this.setList[%this.set],1), NewsGui, NewsGui.key );
}
//-----------------------------------------------------------------------------
function NewsGui::getNextNewsItems( %this )
{
   // Fetch the next batch of older news items:
   error("FETCHING NEXT NEWS ITEMS");
   canvas.SetCursor(ArrowWaitCursor);
   NewsGui.key = LaunchGui.key++;
   %this.state = "status";
   %this.articleCount = 0;
   DatabaseQueryArray(0,0,"1" TAB getField(%this.setList[%this.set],0), NewsGui, NewsGui.key );
   %this.set++;															  
}
//-----------------------------------------------------------------------------
function NewsMOTDText::onDatabaseQueryResult(%this, %status, %RowCount_Result, %key)
{
	if(%key != %this.key)
    	return;
	echo("RECV: " @ %status);
	if(getField(%status,0)==0)
	{
      	NewsEditMOTDBtn.setVisible( getField(%status,2));
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
	echo("RECV: " @ %status);
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
   			NewsEditMOTDBtn.setVisible( getField(%status,2));
   			MessageBoxOK("NOTICE",getField(%status,1));
			NewsMOTDText.setText( NewsEditMOTDText.getText() );
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
   for ( %i = 0; %i < $WebLinkCount; %i++ )
      %this.add( $WebLink[%i, name], %i );
}
//-----------------------------------------------------------------------------
function WebLinksMenu::launchWebBrowser( %this )
{
   %address = $WebLink[WebLinksMenu.getSelected(), address];
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
