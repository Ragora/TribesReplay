//--------------------------------------------------------
function QueryServers( %searchCriteria )
{
   GMJ_Browser.lastQuery = %searchCriteria;
   LaunchGame( "JOIN" );
}

//--------------------------------------------------------
function QueryOnlineServers()
{
   QueryServers("Master");
}

//--------------------------------------------------------
// Launch gui functions
//--------------------------------------------------------
function PlayOffline()
{
   $FirstLaunch = true;
   setNetPort(0);
   $PlayingOnline = false;
   Canvas.setContent(LaunchGui);
}

//--------------------------------------------------------
function OnlineLogIn()
{
   $FirstLaunch = true;
   setNetPort(0);
   $PlayingOnline = true;
   FilterEditGameType.clear();
   FilterEditMissionType.clear();
   queryMasterGameTypes();
   Canvas.setContent(LaunchGui);
}

//--------------------------------------------------------
function LaunchToolbarMenu::onSelect(%this, %id, %text)
{
   switch(%id)
   {
      case 0:
         LaunchGame();
      case 1: // Start Training Mission
         LaunchTraining();
      case 2:
         LaunchNews();
      case 3:
         LaunchForums();
      case 4:
         LaunchEmail();
      case 5: // Join Chat Room
         Canvas.pushDialog(JoinChatDlg);
      case 6:
         LaunchBrowser();
		case 7: // Options
			Canvas.pushDialog(OptionsDlg);
      //case 8: // Play Recording
      //   Canvas.pushDialog(RecordingsDlg);
      case 9: // Quit
         LaunchTabView.closeAllTabs();
         quit();
      //case 10: // Log Off
      //   LaunchTabView.closeAllTabs();
      //   PlayOffline();
      //case 11: // Log On
      //   LaunchTabView.closeAllTabs();
      //   OnlineLogIn();
      case 12:
         LaunchCredits();
   }
}

//--------------------------------------------------------
function LaunchToolbarDlg::onWake(%this)
{
   // Play the shell hum:
   if ( $HudHandle['shellScreen'] $= "" )
      $HudHandle['shellScreen'] = alxPlay( ShellScreenHumSound, 0, 0, 0 );

   LaunchToolbarMenu.clear();

   if ( $PlayingOnline )
   {
      LaunchToolbarMenu.add( 0, "GAME" );
      LaunchToolbarMenu.add( 2, "NEWS" );
      LaunchToolbarMenu.add( 3, "FORUMS" );
      LaunchToolbarMenu.add( 4, "EMAIL" );
      LaunchToolbarMenu.add( 5, "CHAT" );
      LaunchToolbarMenu.add( 6, "BROWSER" );
   }
   else
   {
      LaunchToolbarMenu.add( 1, "TRAINING" );
      LaunchToolbarMenu.add( 0, "LAN GAME" );
   }

   LaunchToolbarMenu.addSeparator();
   LaunchToolbarMenu.add( 7, "SETTINGS" );
//    LaunchToolbarMenu.add( 8, "RECORDINGS" );
   LaunchToolbarMenu.add( 12, "CREDITS" );

   LaunchToolbarMenu.addSeparator();

//    if ( $PlayingOnline )
//       LaunchToolbarMenu.add( 10, "LOG OFF" );
//    else
//       LaunchToolbarMenu.add( 11, "LOG ON" );
   LaunchToolbarMenu.add( 9, "QUIT" );

   LaunchToolbarCloseButton.setVisible( LaunchTabView.tabCount() > 0 );
}

//----------------------------------------------------------------------------
// Launch Tab Group functions:
//----------------------------------------------------------------------------
function OpenLaunchTabs( %gotoWarriorSetup )
{
   if ( LaunchTabView.tabCount() > 0 || !$FirstLaunch )
      return;

   $FirstLaunch = "";

   // Set up all of the launch bar tabs:
   if ( $PlayingOnline )
   {
      LaunchTabView.addLaunchTab( "GAME",    GameGui );
      LaunchTabView.addLaunchTab( "NEWS",    NewsGui );
      LaunchTabView.addLaunchTab( "FORUMS",  ForumsGui );
      LaunchTabView.addLaunchTab( "EMAIL",   EmailGui );
      LaunchTabView.addLaunchTab( "CHAT",    ChatGui );
      LaunchTabView.addLaunchTab( "BROWSER", TribeAndWarriorBrowserGui );

      switch$ ( $pref::Shell::LaunchGui )
      {
         case "News":      %launchGui = NewsGui;
         case "Forums":    %launchGui = ForumsGui;
         case "Email":     %launchGui = EmailGui;
         case "Chat":      %launchGui = ChatGui;
         case "Browser":   %launchGui = TribeAndWarriorBrowserGui;
         default:          %launchGui = GameGui;
      }
   }
   else
   {
      LaunchTabView.addLaunchTab( "TRAINING",   TrainingGui );
      LaunchTabView.addLaunchTab( "LAN GAME",   GameGui );
      %launchGui = TrainingGui;
   }

   if ( %gotoWarriorSetup )
      LaunchGame( "WARRIOR" );
   else
      LaunchTabView.viewTab( "", %launchGui, 0 );
}

//--------------------------------------------------------
function LaunchTabView::addLaunchTab( %this, %text, %gui )
{
   %tabCount = %this.tabCount();
   %this.gui[%tabCount] = %gui;
   %this.key[%tabCount] = 0;
   %this.addTab( %tabCount, %text );
}

//--------------------------------------------------------
function LaunchTabView::onSelect( %this, %id, %text )
{
   // Ignore the ID - it can't be trusted.
   %tab = %this.getSelectedTab();

   Canvas.setContent( %this.gui[%tab] );
   %this.gui[%tab].setKey( %this.key[%tab] );
   %this.lastTab = %tab;
}

//--------------------------------------------------------
function LaunchTabView::viewLastTab( %this )
{
   if ( %this.tabCount() == 0 || %this.lastTab $= "" )
      return;

   %this.setSelectedByIndex( %this.lastTab );
}

//--------------------------------------------------------
function LaunchTabView::viewTab( %this, %text, %gui, %key )
{
   %tabCount = %this.tabCount();
   for ( %tab = 0; %tab < %tabCount; %tab++ )
      if ( %this.gui[%tab] $= %gui && %this.key[%tab] $= %key )
         break;

   if ( %tab == %tabCount )
   {
      // Add a new tab:
      %this.gui[%tab] = %gui;
      %this.key[%tab] = %key;
      // WARNING! This id may not be unique and therefore should
      // not be relied on!  Use index instead!
      %this.addTab( %tab, %text );
   }

   if ( %this.getSelectedTab() != %tab )
      %this.setSelectedByIndex( %tab );
}

//--------------------------------------------------------
function LaunchTabView::closeCurrentTab( %this )
{
   %tab = %this.getSelectedTab();
   %this.closeTab( %this.gui[%tab], %this.key[%tab] );
}

//--------------------------------------------------------
function LaunchTabView::closeTab( %this, %gui, %key )
{
   %tabCount = %this.tabCount();
   for ( %tab = 0; %tab < %tabCount; %tab++ )
   {
      if( %this.gui[%tab] $= %gui && %this.key[%tab] $= %key )
         break;
   }

   if ( %tab == %tabCount )
      return;
   
   for( %i = %tab; %i < %tabCount; %i++ )
   {
      %this.gui[%i] = %this.gui[%i+1];
      %this.key[%i] = %this.key[%i+1];
   }

   %this.removeTabByIndex( %tab );
   %gui.onClose( %key );
   
   if ( %tabCount == 1 )
   {
      %this.lastTab = "";
      Canvas.setContent( LaunchGui );
   }
}

//--------------------------------------------------------
function LaunchTabView::closeAllTabs( %this )
{
   %tabCount = %this.tabCount();
   for ( %i = 0; %i < %tabCount; %i++ )
   {
      %this.gui[%i].onClose( %this.key[%i] );
      %this.gui[%i] = "";
      %this.key[%i] = "";
   }

   %this.clear();
}

//----------------------------------------------------------------------------
// LaunchGui functions:
//----------------------------------------------------------------------------
function LaunchGui::onAdd(%this)
{
   %this.getWarrior = true;
}

//----------------------------------------------------------------------------
function LaunchGui::onWake(%this)
{
   if ( !Canvas.isCursorOn() )
      CursorOn();
   $enableDirectInput = "0";
   disableDirectInput();
   Canvas.pushDialog(LaunchToolbarDlg);
   if ( !$FirstLaunch )
      LaunchTabView.viewLastTab();

   checkNamesAndAliases();
}

//----------------------------------------------------------------------------
function LaunchGui::onSleep(%this)
{
   //alxStop($HudHandle['shellScreen']);
   Canvas.popDialog( LaunchToolbarDlg );
}

//----------------------------------------------------------------------------
// Login/Warrior creation functions:
//---------------------------------------------------------------------------
function CreateWarriorDlg::onAdd( %this )
{
   %this.created = "";
}

//----------------------------------------------------------------------------
function CreateWarriorDlg::onWake( %this )
{
   CreateWarriorText.setText( "<just:center>Enter your warrior name.\nThis is the name by which you will be represented on forums, chat, and in e-mail."
      @ " It is also the name you will use when becoming a member of a tribe." );
   CreateWarriorBtn.setActive( false );
   CreateWarriorNameEdit.schedule( 100, makeFirstResponder, 1 );
}

//----------------------------------------------------------------------------
function CreateWarriorDlg::onLine( %this, %line, %key )
{
   //error( "** line = \"" @ %line @ "\" **" );
   if ( %key != %this.key )
      return;
   
   if ( %this.checkState $= "createplayer" )
   {
      if ( getField( %line, 0 ) $= "0" )
         %this.checkState = "getquad";
      else
      {
         MessageBoxOK( "FAILED", getField( %line, 1 ), "resetCreateWarrior();" );
         %this.checkState = "";
      }
   }
   else if ( %this.checkState $= "getquad" )
   {
      %this.created = true;
      CloseMessagePopup();
      MessageBoxOK( "SUCCESS", "You successfully created the new warrior \"" @ $CreateAccountWarriorName @ "\".", "checkNamesAndAliases();" );
   }
}

//----------------------------------------------------------------------------
function resetCreateWarrior()
{
   $CreateAccountWarriorName = "";
   Canvas.pushDialog( CreateWarriorDlg );
}

//----------------------------------------------------------------------------
function CreateWarriorDlg::connectionTerminated( %this, %key )
{
   if ( %key != %this.key )
      return;
}

//----------------------------------------------------------------------------
function CreateWarriorNameEdit::checkValidPlayerName( %this )
{
   %name = strToPlayerName( %this.getValue() );
   %this.setValue( %name );

   CreateWarriorBtn.setActive( strlen( stripTrailingSpaces( %name ) ) > 2 );
}

//----------------------------------------------------------------------------
function LoginCreateWarrior()
{
   CreateWarriorNameEdit.checkValidPlayerName();
   if ( CreateWarriorBtn.isActive() )
   {
      Canvas.popDialog(CreateWarriorDlg);
      MessagePopup( "PLEASE WAIT", "Creating warrior..." );
      CreateWarriorDlg.key = LaunchGui.key++;
      CreateWarriorDlg.checkState = "createplayer";

      HTTPSecureRequest("update_createplayer", $CreateAccountWarriorName, CreateWarriorDlg, CreateWarriorDlg.key);
   }
}

//----------------------------------------------------------------------------
function checkNamesAndAliases()
{
   %gotoWarriorSetup = false;
   if ( $PlayingOnline )
   {
      // When first launching, make sure we have a valid warrior:
      if ( LaunchGui.getWarrior )
      {
         %cert = WONGetAuthInfo();
         if ( %cert $= "" && !CreateWarriorDialog.created )
         {
            // THIS WILL GO INTO THE CREATE ACCOUNT PROCESS SOON!
            // Must create our player nickname:
            Canvas.pushDialog( CreateWarriorDlg );
            return;
         }
         else
         {
            LaunchGui.getWarrior = "";
            if ( %cert $= "" )
               %warrior = $CreateAccountWarriorName;
            else
               %warrior = getField( %cert, 0 );

            %warriorIdx = -1;
            for ( %i = 0; %i < $pref::Player::count; %i++ )
            {
               if ( %warrior $= getField( $pref::Player[%i], 0 ) )
               {
                  %warriorIdx = %i;
                  break;
               }
            }

            if ( %warriorIdx == -1 )
            {
               // Create new warrior:
               $pref::Player[$pref::Player::Count] = %warrior @ "\tHuman Male\tbeagle\tMale1";
               $pref::Player::Current = $pref::Player::Count;
               $pref::Player::Count++;
               %gotoWarriorSetup = true;
            }
         }
      }
   }
   else if ( $pref::Player::Count == 0 )
   {
      %gotoWarriorSetup = true;
   }

   OpenLaunchTabs( %gotoWarriorSetup );
}
