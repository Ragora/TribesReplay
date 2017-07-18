//--------------------------------------------------------------------------
//
// Tribes 2 startup
//
//--------------------------------------------------------------------------

//--------------------------------------------------------------------------
// parse arguments:

$TestObjectFileName = "";
$LaunchMode = "Normal";
$Login = false;
$clientprefs = "prefs/clientPrefs.cs";
$serverprefs = "prefs/serverPrefs.cs";
$fromLauncher = false;

//------------------------------------------------------------------------------
function prepBuild()
{
   // this compiles all the scripts and guis
   for(%file = findFirstFile("*.cs"); %file !$= ""; %file = findNextFile("*.cs"))
      compile(%file);
   for(%file = findFirstFile("*.gui"); %file !$= ""; %file = findNextFile("*.gui"))
      compile(%file);
}

//------------------------------------------------------------------------------
function cleanupAudio()
{
   alxStopAll();
   AudioGui.delete();
   sButtonDown.delete();
   sButtonOver.delete();
}

function startAudio()
{
   //--------------------------------------
   // temp Audio Profiles
   new AudioDescription(AudioGui)
   {
      volume   = 1.0;
      isLooping= false;
      is3D     = false;
      type     = $GuiAudioType;
   };

   new AudioProfile(sButtonDown)
   {
      filename = "gui/buttonDown.wav";
      description = "audioGui";
      preload = true;
   };

   new AudioProfile(sButtonOver)
   {
      filename = "gui/buttonOver.wav";
      description = "audioGui";
      preload = true;
   };

   $Audio::defaultDriver = "miles";
   audioDetect();
   if(!$noloadAudio)
   {
      // make sure there is a driver list
      if($pref::Audio::drivers $= "")
         $pref::Audio::drivers = $Audio::defaultDriver;

      // make sure that there is an active driver
      if($pref::Audio::activeDriver $= "")
         $pref::Audio::activeDriver = $Audio::defaultDriver;

      // install the active driver
      $Audio::initialized = audioSetDriver($pref::Audio::activeDriver);
      if(!$Audio::initialized)
      {
         error("Audio: failed to initialize using [" @ $pref::Audio::activeDriver @ "] driver");

         // if the driver was not the default then attempt to load the default
         if($pref::Audio::activeDriver !$= $Audio::defaultDriver)
         {
            error("Audio: attempting to initialize [" @ $Audio::defaultDriver @ "]");
            $Audio::initialized = audioSetDriver($Audio::defaultDriver);
            if($Audio::initialized)
               $pref::Audio::activeDriver = $Audio::defaultDriver;
         }
      }

      // just set the volumes that are needed now
      if($Audio::initialized)
      {
         alxListenerf( AL_GAIN_LINEAR, $pref::Audio::masterVolume );
         alxSetChannelVolume( $GuiAudioType, $pref::Audio::guiVolume );
      }
   }
}

//------------------------------------------------------------------------------
function repaintCanvas()
{
	if ( isObject( Canvas ) )
		Canvas.repaint();
}

function resetCanvas()
{
	if ( isObject( Canvas ) )
		Canvas.reset();
}

//------------------------------------------------------------------------------
for($i = 1; $i < $Game::argc ; $i++)
{
   $arg = $Game::argv[$i];
   $nextArg = $Game::argv[$i+1];
   $nextArg2 = $Game::argv[$i+2];
   $hasNextArg = $Game::argc - $i > 1;
   $has2NextArgs = $Game::argc - $i > 2;
   
   if (!stricmp(fileExt($arg), ".dif"))
   {
      $LaunchMode = "InteriorView";
      //$SkipLogin = true;
      $TestObjectFileName = $arg;
      echo($TestObjectFileName);
   }
   else if(!stricmp(fileExt($arg), ".dif\""))
   {
      $LaunchMode = "InteriorView";
      //$SkipLogin = true;
      $TestObjectFileName = getSubStr($arg,1, strlen($arg) - 2);
   }
   else if ( $arg $= "-mod" && $hasNextArg )
   {
      setModPaths( $nextArg );
      $i += 2;
   }
   else if($arg $= "-dedicated")
   {
      $LaunchMode = "DedicatedServer";
   }
   else if($arg $= "-clientprefs" && $hasNextArg)
   {
      $i++;
      $clientprefs = $nextArg;
   }
   else if($arg $= "-serverprefs" && $hasNextArg)
   {
      $i++;
      $serverprefs = $nextArg;
   }
   else if($arg $= "-host")
   {
      $LaunchMode = "HostGame";
   }
   else if($arg $= "-mission" && $has2NextArgs)
   {
      $i += 2;
      $mission = $nextArg;
      $missionType = $nextArg2;
   }
   else if($arg $= "-connect" && $hasNextArg)
   {
      $i++;
      $LaunchMode = "Connect";
      $JoinGameAddress = $nextArg;
   }
   else if($arg $= "-jload" && $hasNextArg)
   {
      $i++;
      $JournalFile = $nextArg;
      $JournalMode = "LoadJournal";
   }
   else if($arg $= "-jsave" && $hasNextArg)
   {                     
      $i++;
      $JournalFile = $nextArg;
      $JournalMode = "SaveJournal";
   }
   else if($arg $= "-jplay" && $hasNextArg)
   {
      $i++;
      $JournalFile = $nextArg;
      $journalMode = "PlayJournal";
   }
   else if($arg $= "-navBuild" && $has2NextArgs)
   {
      $i += 2;
      $LaunchMode = "NavBuild";
      $mission = $nextArg;
      $missionType = $nextArg2;
   }
   else if($arg $= "-spnBuild" && $has2NextArgs)
   {
      $i += 2;
      $LaunchMode = "SpnBuild";
      $mission = $nextArg;
      $missionType = $nextArg2;
   }
   else if($arg $= "-demo")
   {
      $LaunchMode = "Demo";
   }
   else if($arg $= "-login" && $has2NextArgs)
   {
      $i += 2;
      $Login = true;
      $LoginName = $nextArg;
      $LoginPassword = $nextArg2;
   }
   else if($arg $= "-show")
   {
      $LaunchMode = "TSShow";
   }
   else if($arg $= "-con")
   {
      $LaunchMode = "Console";
   }
   else if ($arg $= "-bot" && $hasNextArg)
   {
      $i++;
      $CmdLineBotCount = $nextArg;
   }
   else if ($arg $= "-light" && $hasNextArg)
   {
      $LaunchMode = "SceneLight";
      $mission = $nextArg;
   }
   else if ($arg $= "-prepbuild")
   {
      enableWinConsole(true);
      prepBuild();
      setLogMode(1);
      setEchoFileLoads(true);
   }
   else if($arg $= "-quit")
   {
      quit();
      return;
   }
   else if ($arg $= "-nologin")
   {
      $SkipLogin = true;
      $LaunchMode = "Offline";
   }
   else if ( $arg $= "-online" )
      $fromLauncher = true;
}

// load autoexec once for command-line overrides:
exec("autoexec.cs", true); 

switch$( $JournalMode )
{
   case "LoadJournal":
      echo("Loading event log from journal: " @ $JournalFile);
      loadJournal($JournalFile);
   case "SaveJournal":
      echo("Saving event log to journal: " @ $JournalFile);
      saveJournal($JournalFile);
   case "PlayJournal":
      playJournal($JournalFile);
}

//--------------------------------------------------------------------------
// load defaults

//error("FIX THIS: SHOULD CHECK FOR LOGIN PARAM");

exec("scripts/clientDefaults.cs", true);
exec("scripts/serverDefaults.cs", true);
exec($clientprefs, true, true);
exec($serverprefs, true, true);

// load autoexec again to override video settings/window creation

exec("autojournal.cs", true, true); // put journal'd startup options in here
                                    // so you can autoconnect to servers, etc.
exec("autoexec.cs", true); 

// Go through the command line for setting overrides
// Added mostly for the Linux client (Sam Lantinga)
for($i = 1; $i < $Game::argc ; $i++)
{
   $arg = $Game::argv[$i];
   $nextArg = $Game::argv[$i+1];
   $hasNextArg = $Game::argc - $i > 1;
   
   if($arg $= "--nosound" || $arg $= "-s")
   {
      // This is a Linux null audio device specifier
      $pref::OpenAL::driver = "'( ( devices '( null )))";
   }
   else if($arg $= "--fullscreen" || $arg $= "-f")
   {
      $pref::Video::fullScreen = 1;
   }
   else if($arg $= "--windowed" || $arg $= "-w")
   {
      $pref::Video::fullScreen = 0;
   }
   else if(($arg $= "--gllibrary" || $arg $= "-g") && $hasNextArg)
   {
      $i++;
      $pref::OpenGL::driver = $nextArg;
   }
}

//note - no argument means the seed will be set according to Platform::getRealMilliseconds()...
setRandomSeed();

if($winConsoleEnabled)
   enableWinConsole(true);
   
if( $Pref::useImmersion )
   enableImmersion( true );   

$showImmersionDialog = $Pref::useImmersion && $ImmEnabled;

switch( $pref::Shell::lastBackground )
{
   case 0:
      $ShellBackground = "gui/bg_Hammers.png";
   case 1:
      $ShellBackground = "gui/bg_BloodEagle.png";
   case 2:
      $ShellBackground = "gui/bg_DiamondSword.png";
   case 3:
      $ShellBackground = "gui/bg_Starwolf.png";
   case 4:
      $ShellBackground = "gui/bg_Harbingers.png";
   default:
      $ShellBackground = "gui/bg_Bioderm.png";
}

//------------------------------------------------------------------------------
function dedCheckLoginDone()
{
   %res = WONLoginResult();
   %status = getField(%res, 0);
   %msg = getField(%res, 1);
   if(%status $= "Waiting")
      schedule(1000, 0, dedCheckLoginDone);
   else
   {
      if(%status $= "OK")
      {
         $LoginName = "";
         $LoginPassword = "";
         exec("console_end.cs");
      }
      else
      {
         echo("ERROR STARTING DEDICATED SERVER: " @ %msg);
         quit();
      }
   }
}

//------------------------------------------------------------------------------
function SetLoginResponder(%time)
{
   if ( $LoginName $= "" )
      LoginEditBox.schedule( %time, makeFirstResponder, 1 );
   else
      LoginPasswordBox.schedule( %time, makeFirstResponder, 1 );
}

//------------------------------------------------------------------------------
function EULADlg::onWake( %this )
{
   EULAInstructions.setText( "<just:center>Please read the following License Agreement carefully."
         NL "You must accept this agreement in order to play"
         NL "Tribes 2." );

   %file = new FileObject();
   %fileOpenSuccess = false;
   if (isT2UkBuild())
   {
      // Uk EULA
      %fileOpenSuccess = %file.openForRead("UKEULA.TXT");
   } 
   else
   {
      // US+ EULA
      %fileOpenSuccess = %file.openForRead("EULA.TXT");
   }


   if ( %fileOpenSuccess )
   {
      while ( !%file.isEOF() )
      {
         %line = %file.readLine();
         if ( %text $= "" )
            %text = "<tab:5 10 15>" @ %line;
         else
            %text = %text NL %line;
      }
   }
   else
      error( "Failed to open the EULA file!" ); 
   %file.delete();

   EULAText.setText( %text );
}

//------------------------------------------------------------------------------
function EULADlg::accepted( %this )
{
   $pref::AcceptedEULA = true;
   Canvas.popDialog( %this );
   StartLoginProcess();
}

//------------------------------------------------------------------------------
function LoginDlg::onWake( %this )
{
   SetLoginResponder( 100 );
}

//------------------------------------------------------------------------------
function LoginProcess(%editAcct)
{
   Canvas.popDialog( LoginDlg );
   LoginMessagePopup( "PLEASE WAIT", "Validating login..." );
   WONStartLogin( $LoginName, $LoginPassword, %editAcct );
   StartupGui.loginSchedule = StartupGui.schedule( 1000, checkLoginDone,%editAcct );
}

function PasswordProcess()
{
   LoginMessagePopup( "PLEASE WAIT", "Attempting to email you your login password..." );
   WONStartEmailFetch($LoginName);
   StartupGui.loginSchedule = StartupGui.schedule( 1000, checkLoginDone,false,true );
}

//------------------------------------------------------------------------------
function LoginMessageBox( %title, %message, %buttonText, %callback )
{
   LoginMessageBoxFrame.setTitle( %title );
   LoginMessageBoxText.setText( "<just:center>" @ %message );
   LoginMessageBoxButton.setValue( %buttonText );
   LoginMessageBoxDlg.callback = %callback;
   Canvas.pushDialog( LoginMessageBoxDlg );
}

//------------------------------------------------------------------------------
function LoginMessagePopup( %title, %message )
{
   LoginMessagePopupFrame.setTitle( %title );
   LoginMessagePopupText.setText( "<just:center>" @ %message );
   Canvas.pushDialog( LoginMessagePopupDlg );
}

//------------------------------------------------------------------------------
function LoginMessageBoxButtonProcess()
{
   Canvas.popDialog( LoginMessageBoxDlg );
   if ( LoginMessageBoxDlg.callback !$= "" )
      eval( LoginMessageBoxDlg.callback );
}

//------------------------------------------------------------------------------

function EditAccountDlg::onUpdate(%this)
{
   if ( strcmp( $CreateAccountPassword, $CreateAccountConfirmPassword ) )
   {
      LoginMessageBox( "ERROR", "Passwords don't match.", "OK" );
      return;
   }
   WONStartUpdateAccount( $CreateAccountPassword,
                    $CreateAccountEmail,
                    $CreateAccountSendInfo );
   Canvas.popDialog(EditAccountDlg);                 
   StartupGui.loginSchedule = StartupGui.schedule( 1000, checkLoginDone );
   StartupGui.updatingAccount = true;
   LoginMessagePopup( "PLEASE WAIT", "Updating account information..." );
}

function EditAccountDlg::onDontUpdate(%this)
{
   schedule(0,0,LoginDone);
}

function StartupGui::checkLoginDone( %this, %editAcct,%emailCheck )
{
   %result = WONLoginResult();
   %code = getField( %result, 1 );
   %codeText = getField(%result, 2);
   %status = getField( %result, 0 );

   if ( %status $= "Waiting" )
   {
      LoginMessagePopupText.setValue( "<just:center>" @ %code );
      %this.loginSchedule = %this.schedule( 1000, checkLoginDone, %editAcct );
   }
   else if ( %status !$= "OK" )
   {
      echo(%codeText);
      echo(%code);
      switch$(%codeText)
      {
         case "WS_DBProxyServ_InvalidUserName":
            if(%emailCheck)
               %msg = "Email Password Failed - Invalid login name.  Please check the login name and try again.";
            else
               %msg = "Account Creation Failed - Invalid login name.  Login names may only contain letters, numbers and underlines, and must be from 3 to 16 characters in length.";
         case "WS_AuthServ_BadCDKey":
         case "WS_DBProxyServ_InvalidCDKey":
            %msg = "Account Creation Failed - Invalid CD Key.  Please check the CD key for errors.";
         case "WS_TimedOut":
            %msg = "Login Failed - Server timed out.  Your internet connection may be having problems or the servers may be temporarily unavailable.";
         case "WS_DBProxyServ_KeyInUse":
            %msg = "Account Creation Failed - That CD Key has already been used to create an account.";
         case "WS_DBProxyServ_AccountCreationDisabled":
            %msg = "Account Creation Failed - Account creation is temporarily disabled.  Please try again later.";
         case "WS_DBProxyServ_UserExists":
            %msg = "Account Creation Failed - That login name is already taken.  Please choose another login name.";
         case "WS_DBProxyServ_DBError":
            %msg = "Account Creation Failed - The database is temporarily offline (error code - " @ getField(%result, 3) @ ")";
         case "WS_AuthServ_CDKeyBanned":
            %msg = "Login Failed - Your account has been banned.  You may not play Tribes 2.";
         case "WS_AuthServ_UserSeqInUse":
            %msg = "Login Failed - Someone is already playing using this account.  If you are not logged in already, please wait 20 minutes and try again.";
         case "WS_AuthServ_CRCFailed":
            %msg = "Login Failed - Your version of Tribes 2 is out of date or has been modified.  You may have a virus.";
         case "WS_AuthServ_UserNotFound" or "WS_AuthServ_BadPassword" or "WS_AuthServ_InvalidUserName":
            %msg = "Login Failed - Please check your login name and password and try again.";
         case "WS_AuthServ_NotInCommunity":
            %msg = "Login Failed - This account is not a valid Tribes 2 account.";
         case "AuthServerListFail":
            %msg = "Login Failed - Unable to fetch authentication server list.  Please try again soon.";
         case "ProfileServerListFail":
            %msg = "Login Failed - Unable to fetch profile server list.  Please try again soon.";
         case "BadWords":
            %msg = "Account Creation Failed - Your warrior name may not contain profanity.  Please choose another warrior name.";
         default:
            if(%code <= -2900 && %code >= -2999)
            {				
			   if(%code == -2902)
					%msg = "Account Creation Failed - That CDKey is already in use." @ %code;
			   else
               		%msg = "Account Creation Failed - That warrior name is already in use.  Please choose another warrior name and try again. Code = " @ %code;
            }
            else
               %msg = "Login Failed - Your internet connection may be having problems or the servers may be temporarily unavailable.  (Error code: " @ %codeText @ ")";
      }
      Canvas.popDialog( LoginMessagePopupDlg );
      if(StartupGui.updatingAccount)
         LoginMessageBox( "UPDATE FAILED", %msg, "OK", "schedule(0,0,LoginDone);" );
      else
         LoginMessageBox( "LOGIN FAILED", %msg, "OK", "StartupGui::dumbFunction();" );
   }
   else // we're logged in...
   {
      // Since we've successfully logged in, save the password:
      if ( $pref::RememberPassword )
      {
         if ( StartupGui.updatingAccount )
            EditAccountPasswordBox.savePassword();
         else
            LoginPasswordBox.savePassword();
      }

      if(%editAcct)
      {
         $CreateAccountLoginName = $LoginName;
         $CreateAccountPassword = $LoginPassword;
         $CreateAccountConfirmPassword = $LoginPassword;
         $CreateAccountEmail = %codeText;
         $CreateAccountSendInfo = %code;
         Canvas.pushDialog(EditAccountDlg);
      }
      else if(%emailCheck)
      {
         Canvas.popDialog( LoginMessagePopupDlg );
      }
      else
      {
         schedule( 0, 0, "LoginDone" );
      }
   }
}

//------------------------------------------------------------------------------
function StartupGui::dumbFunction( %this )
{
   if ( !CreateAccountDlg.open )
      Canvas.pushDialog( LoginDlg );
}

//------------------------------------------------------------------------------
function CreateAccount()
{
   $CreateAccountLoginName = "";
   $CreateAccountWarriorName = "";
   $CreateAccountPassword = "";
   $CreateAccountConfirmPassword = "";
   CreateAccountCDKey1.setText( "" );
   CreateAccountCDKey2.setText( "" );
   CreateAccountCDKey3.setText( "" );
   CreateAccountCDKey4.setText( "" );
   CreateAccountCDKey5.setText( "" );
   $CreateAccountEmail = "";
   $CreateAccountSendInfo = 0;

   Canvas.pushDialog( CreateAccountDlg );
}

//------------------------------------------------------------------------------
function CreateAccountDlg::onWake( %this )
{
   %this.open = true;

   CreateAccountSubmitBtn.setActive( false );
   schedule( 0, 0, updateSubmitButton );
}

//------------------------------------------------------------------------------
function CreateAccountDlg::onSleep( %this )
{
   %this.open = false;
}

//------------------------------------------------------------------------------
function WarriorNameEntry::validateWarriorName( %this )
{
   %name = %this.getValue();
   %test = strToPlayerName( %name );
   if ( %name !$= %test )
      %this.setText( %test );   
}

//------------------------------------------------------------------------------
function buildCDKey()
{
   $CreateAccountCDKey = strupr( CreateAccountCDKey1.getValue() 
                       @ "-" @ CreateAccountCDKey2.getValue() 
                       @ "-" @ CreateAccountCDKey3.getValue() 
                       @ "-" @ CreateAccountCDKey4.getValue()
                       @ "-" @ CreateAccountCDKey5.getValue() ) ;
}

//------------------------------------------------------------------------------
function CreateAccountCDKey1::process( %this )
{
   buildCDKey();
   if ( strlen( %this.getValue() ) == 4 )
      CreateAccountCDKey2.makeFirstResponder( true );
}

//------------------------------------------------------------------------------
function CreateAccountCDKey2::process( %this )
{
   buildCDKey();
   if ( strlen( %this.getValue() ) == 4 )
      CreateAccountCDKey3.makeFirstResponder( true );
}

//------------------------------------------------------------------------------
function CreateAccountCDKey3::process( %this )
{
   buildCDKey();
   if ( strlen( %this.getValue() ) == 4 )
      CreateAccountCDKey4.makeFirstResponder( true );
}

//------------------------------------------------------------------------------
function CreateAccountCDKey4::process( %this )
{
   buildCDKey();
   if ( strlen( %this.getValue() ) == 4 )
      CreateAccountCDKey5.makeFirstResponder( true );
}

//------------------------------------------------------------------------------
function CreateAccountCDKey5::process( %this )
{
   buildCDKey();
}

//------------------------------------------------------------------------------
function updateSubmitButton()
{
   if ( !CreateAccountDlg.open )
      return;

   %allDone = true;
   if ( strlen( $CreateAccountLoginName ) < 3 )
      %allDone = false;
   else if ( strlen( $CreateAccountWarriorName ) < 3 )
      %allDone = false;
   else if ( strlen( $CreateAccountCDKey ) != 24 )
      %allDone = false;
   else if ( strlen( $CreateAccountEmail ) < 5 || strpos( $CreateAccountEmail, "@" ) == -1  )
      %allDone = false;
   else if (!$CreateAccountAgeGood)
      %allDone = false;

   CreateAccountSubmitBtn.setActive( %allDone );

   schedule( 1000, 0, updateSubmitButton );
}

//------------------------------------------------------------------------------
function CleanUpAndGo()
{
   Canvas.popDialog( LoginMessagePopupDlg );
   Canvas.setContent( "" );
   Canvas.setCursor( "" );
   LoginMessagePopupDlg.delete();
   LoginMessageBoxDlg.delete();
   ImmSplashDlg.delete();
   EULADlg.delete();
   if ( !$pref::SkipIntro )
   {
      IntroProfile.delete();
      IntroGui.delete();
   }
   if ( !$SkipLogin )
   {
      EditAccountDlg.delete();
      LoginDlg.delete();
      CreateAccountDlg.delete();
      CloseButtonProfile.delete();
      NewTextEditProfile.delete();
      ShellPopupProfile.delete();
      ShellRadioProfile.delete();
      ShellTextRightProfile.delete();
   }
   StartupGui.delete();

   DefaultCursor.delete();
   DlgBackProfile.delete();
   GuiContentProfile.delete();
   ShellDlgPaneProfile.delete();
   NewScrollCtrlProfile.delete();
   ShellMediumTextProfile.delete();
   ShellMessageTextProfile.delete();
   ShellButtonProfile.delete();
   
   $CreateAccountPassword = "";
   $CreateAccountLoginName = "";
   $CreateAccountConfirmPassword = "";
   $LoginName = "";
   $LoginPassword = "";
   WONDisableFutureCalls();
   cleanupAudio();
   exec("console_end.cs");
}

//------------------------------------------------------------------------------
function LoginDone()
{
   // Save off the login name for next time:
   $pref::LastLoginName = $LoginName;
   export( "$pref::*", "prefs/ClientPrefs.cs", False );

   CleanUpAndGo();
}

//------------------------------------------------------------------------------
function CreateAccountDlg::onCancel()
{
   Canvas.popDialog( CreateAccountDlg );
}

//------------------------------------------------------------------------------
function CreateAccountDlg::onSubmit()
{
   if ( strcmp( $CreateAccountPassword, $CreateAccountConfirmPassword ) )
   {
      LoginMessageBox( "ERROR", "Passwords don't match.", "OK" );
      return;
   }

   if ( !$CreateAccountAgeGood )
   {
      LoginMessageBox( "INVALID ENTRY", "You must be at least thirteen years old to create a Tribes 2 account.", "OK" );
      return;
   }

   $LoginName = $CreateAccountLoginName;

   WONStartCreateAccount( $CreateAccountLoginName,
                    $CreateAccountWarriorName,
                    $CreateAccountPassword,
                    $CreateAccountCDKey,
                    $CreateAccountEmail,
                    $CreateAccountSendInfo );
                    
   StartupGui.loginSchedule = StartupGui.schedule( 1000, checkLoginDone );
   LoginMessagePopup( "PLEASE WAIT", "Validating account information..." );
}

//------------------------------------------------------------------------------
function hideImmSplashDlg()
{
   Canvas.popDialog( ImmSplashDlg );
   StartLoginProcess();
}

//------------------------------------------------------------------------------
function checkIntroDone()
{
   if (IntroGui.done)
   {
      Canvas.popDialog( IntroGui );
      Canvas.showCursor();
      StartLoginProcess();
   }
   else
      schedule( 100, 0, checkIntroDone );
}

//------------------------------------------------------------------------------

function StartLoginProcess()
{
   if ( !$pref::AcceptedEULA )
   {
      Canvas.pushDialog( EULADlg );
      return;
   }
   
   if ( $showImmersionDialog )
   {
      $showImmersionDialog = false;
      Canvas.pushDialog( ImmSplashDlg );
      schedule( 2500, 0, hideImmSplashDlg );
      return;
   }
   
   if ( !$SkipLogin )
   {
      if ( $LaunchMode $= "Normal" && !$fromLauncher )
         LoginMessageBox( "ERROR", "In order to play Tribes 2 online, you must launch the game using the supplied shortcuts.", "OK", "quit();" );
      else
      {
         Canvas.pushDialog( LoginDlg );

         if ( $LaunchMode $= "Demo" )
            LoginDone();
         else
         {
            if ( $Login == true )
               LoginProcess();
         }
      }
   }
   else
   {
      $PlayingOnline = false;
      LoginMessagePopup( "INITIALIZING", "Please wait..." );
      Canvas.repaint();

      CleanUpAndGo();
   }
}

if(!$SkipLogin)
   WONInit();

//------------------------------------------------------------------------------
if ($LaunchMode $= "DedicatedServer" ||
    $LaunchMode $= "Console" ||
    $LaunchMode $= "NavBuild" ||
    $LaunchMode $= "SpnBuild")
{
   $Con::logBufferEnabled = false;
   if($Login)
   {
      $PlayingOnline = true;
      LoginProcess();
   }
   else
   {
      if($SkipLogin)
      {
         $PlayingOnline = false;
         exec("console_end.cs");
      }
      else
      {
         $PlayingOnline = true;
         WONServerLogin();
         schedule(1000, 0, dedCheckLoginDone);
      }
   }
}
else
{
   videoSetGammaCorrection($pref::OpenGL::gammaCorrection);
   if(!createCanvas())
   {
      quit();
      return;
   }
   startAudio();
   setOpenGLTextureCompressionHint( $pref::OpenGL::compressionHint );
   setOpenGLAnisotropy( $pref::OpenGL::anisotropy );
   setOpenGLMipReduction( $pref::OpenGL::mipReduction );
   setOpenGLInteriorMipReduction( $pref::OpenGL::interiorMipReduction );
   setOpenGLSkyMipReduction( $pref::OpenGL::skyMipReduction );

   setShadowDetailLevel( $pref::shadows );

   setDefaultFov( $pref::Player::defaultFov );
   setZoomSpeed( $pref::Player::zoomSpeed );
   
   // create control and object profiles:
   new GuiCursor (DefaultCursor)
   {
      hotSpot = "1 1";
      bitmapName = "gui/CUR_3darrow";
   };

   new GuiControlProfile (DlgBackProfile)
   {
      opaque = true;
      fillColor = "0 0 0 160";
   };

   new GuiControlProfile (GuiContentProfile)
   {
      opaque = true;
      fillColor = "255 255 255";
   };

   new GuiControlProfile (ShellDlgPaneProfile)
   {
      fontType = "Sui Generis";
      fontSize = 22;
      fontColor = "5 5 5";
      autoSizeWidth = false;
      autoSizeHeight = false;
      bitmapBase = "gui/dlg";
   };

   new GuiControlProfile (NewScrollCtrlProfile)
   {
      bitmapBase = "gui/shll_scroll";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
   };

   new GuiControlProfile (ShellMediumTextProfile)
   {
      fontType = "Univers";
      fontSize = 18;
      fontColor = "6 245 215";
      autoSizeWidth = false;
      autoSizeHeight = true;
   };

   new GuiControlProfile (ShellMessageTextProfile)
   {
      fontType = "Univers";
      fontSize = 18;
      fontColor = "66 219 234";
      fontColorHL = "25 68 56";
      fillColorHL = "50 233 206";
      cursorColor = "200 240 240";
      autoSizeWidth = true;
      autoSizeHeight = true;
   };

   new GuiControlProfile (ShellButtonProfile)
   {
      fontType = "Univers Condensed";
      fontSize = 16;
      fontColor = "8 19 6";
      fontColorHL = "25 68 56";
      fontColorNA = "5 5 5";
      fontColorSEL = "25 68 56";
      fixedExtent = true;
      justify = "center";
      bitmap = "gui/shll_button";
      textOffset = "0 10";
      soundButtonDown = sButtonDown;
      soundButtonOver = sButtonOver;
      tab = true;
      canKeyFocus = true;
   };

   new GuiControlProfile (IntroProfile)
   {
      opaque = true;
      fillColor = "8 8 8 255";
      tab = true;
      canKeyFocus = true;
   };

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
   // create guis for this file 
   // Startup gui:
   new GuiChunkedBitmapCtrl (StartupGui) {
      profile = "GuiContentProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      bitmap = $ShellBackground;
   };

   // EULA dialog:
   new GuiControl(EULADlg) {
      profile = "DlgBackProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      helpTag = "0";

      new ShellPaneCtrl() {
         profile = "ShellDlgPaneProfile";
         horizSizing = "center";
         vertSizing = "center";
         position = "100 60";
         extent = "440 360";
         minExtent = "48 92";
         visible = "1";
         helpTag = "0";
         text = "LICENSE AGREEMENT";
         noTitleBar = "0";

         new GuiMLTextCtrl(EULAInstructions) {
            profile = "ShellMediumTextProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "26 34";
            extent = "390 14";
            minExtent = "8 8";
            visible = "1";
            helpTag = "0";
            lineSpacing = "2";
         };
         new ShellScrollCtrl() {
            profile = "NewScrollCtrlProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "22 90";
            extent = "396 213";
            minExtent = "24 52";
            childMargin = "3 3";
            visible = "1";
            helpTag = "0";
            willFirstRespond = "1";
            hScrollBar = "alwaysOff";
            vScrollBar = "alwaysOn";
            constantThumbHeight = "0";
            fieldBase = "gui/shll_field";

            new GuiScrollContentCtrl() {
               profile = "GuiDefaultProfile";
               horizSizing = "width";
               vertSizing = "height";
               position = "4 4";
               extent = "370 175";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";

               new GuiMLTextCtrl(EULAText) {
                  profile = "ShellMessageTextProfile";
                  horizSizing = "width";
                  vertSizing = "height";
                  position = "0 0";
                  extent = "366 16";
                  minExtent = "8 8";
                  visible = "1";
                  helpTag = "0";
                  lineSpacing = "2";
               };
            };
         };
         new ShellBitmapButton() {
            profile = "ShellButtonProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "61 305";
            extent = "140 38";
            minExtent = "32 38";
            visible = "1";
            command = "quit();";
            helpTag = "0";
            text = "QUIT";
            simpleStyle = "0";
         };
         new ShellBitmapButton() {
            profile = "ShellButtonProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "251 305";
            extent = "140 38";
            minExtent = "32 38";
            visible = "1";
            command = "EULADlg.accepted();";
            helpTag = "0";
            text = "ACCEPT";
            simpleStyle = "0";
         };
      };
   };

   // Message Popup dialog:
   new GuiControl (LoginMessagePopupDlg) {
      profile = "DlgBackProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      helpTag = "0";

      new ShellPaneCtrl (LoginMessagePopupFrame) {
         profile = "ShellDlgPaneProfile";
         horizSizing = "center";
         vertSizing = "center";
         position = "165 194";
         extent = "310 108";
         minExtent = "48 92";
         visible = "1";
         helpTag = "0";

         new GuiMLTextCtrl (LoginMessagePopupText) {
            profile = "ShellMediumTextProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "32 40";
            extent = "246 32";
            minExtent = "8 8";
            visible = "1";
            helpTag = "0";
         };
      };
   };

   // Message Box dialog:
   new GuiControl(LoginMessageBoxDlg) {
      profile = "DlgBackProfile";
      horizSizing = "width";
      vertSizing = "height";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      helpTag = "0";

      new ShellPaneCtrl(LoginMessageBoxFrame) {
         profile = "ShellDlgPaneProfile";
         horizSizing = "center";
         vertSizing = "center";
         position = "170 137";
         extent = "300 206";
         minExtent = "48 92";
         visible = "1";
         helpTag = "0";

         new GuiMLTextCtrl(LoginMessageBoxText) {
            profile = "ShellMediumTextProfile";
            horizSizing = "center";
            vertSizing = "bottom";
            position = "32 39";
            extent = "236 18";
            minExtent = "8 8";
            visible = "1";
            helpTag = "0";
            lineSpacing = "2";
         };
         new ShellBitmapButton(LoginMessageBoxButton) {
            profile = "ShellButtonProfile";
            horizSizing = "center";
            vertSizing = "bottom";
            position = "70 140";
            extent = "120 38";
            minExtent = "32 38";
            visible = "1";
            command = "LoginMessageBoxButtonProcess();";
            accelerator = "return";
            helpTag = "0";
            text = "OK";
            simpleStyle = "0";
         };
      };
   };

   // Immersion splash dialog:
   new GuiControl (ImmSplashDlg) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      helpTag = "0";

      new GuiControl() {
         profile = "GuiModelessDialogProfile";
         horizSizing = "center";
         vertSizing = "center";
         position = "40 150";
         extent = "540 168";
         minExtent = "8 8";
         visible = "1";
         helpTag = "0";

         new GuiChunkedBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "width";
            vertSizing = "height";
            position = "0 0";
            extent = "540 168";
            minExtent = "8 8";
            visible = "1";
            helpTag = "0";
            bitmap = "gui/Immersion.jpg";
            wrap = "0";
         };
      };
   };
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

   if ( !$pref::SkipIntro )
   {
      new GuiAviBitmapCtrl(IntroGui) 
      {
         profile = "IntroProfile";
         horizSizing = "width";
         vertSizing = "height";
         position = "0 0";
         extent = "640 480";
         minExtent = "8 8";
         visible = "1";
         variable = "";
         helpTag = "0";
         useVariable = "1";
         aviFileName = "T2IntroC15.avi";
         wavFileName = "T2Intro.wav";
         setFirstResponder = "1";
         letterBox = true;
         swapRB = false;
      };
   }
   

   if ( !$SkipLogin )
   {
      // Login-only profiles:
      new GuiControlProfile (CloseButtonProfile)
      {
         bitmap = "gui/shll_menuclose";
         soundButtonDown = sButtonDown;
         soundButtonOver = sButtonOver;
         tab = true;
         canKeyFocus = true;
      };

      new GuiControlProfile (NewTextEditProfile)
      {
         fillColorHL = "130 190 185";
         fontType = "Univers";
         fontSize = 16;
         fontColor = "60 140 140";
         fontColorHL = "25 68 56";
         cursorColor = "173 255 250";
         bitmap = "gui/shll_entryfield";
         textOffset = "14 0";
         autoSizeWidth = false;
         autoSizeHeight = false;
         tab = true;
         canKeyFocus = true;
      };

      new GuiControlProfile (ShellPopupProfile)
      {
         fontType = "Univers";
         fontSize = 16;
         fontColor = "8 19 6";
         fontColorHL = "25 68 56";
         fontColorNA = "5 5 5";
         fontColorSEL = "8 19 6";
         fixedExtent = true;
         justify = "center";
         soundButtonDown = sButtonDown;
         soundButtonOver = sButtonOver;
         bitmapBase = "gui/shll_scroll";
         tab = true;
         canKeyFocus = true;
      };

      new GuiControlProfile (ShellRadioProfile)
      {
         fontType = "Univers Condensed";
         fontSize = 16;
         fontColor = "8 19 6";
         fontColorHL = "25 68 56";
         fontColorNA = "5 5 5";
         fixedExtent = true;
         justify = "center";
         bitmap = "gui/shll_radio";
         soundButtonDown = sButtonDown;
         soundButtonOver = sButtonOver;
         tab = true;
         canKeyFocus = true;
      };

      new GuiControlProfile (ShellTextRightProfile)
      {
         fontType = "Univers Condensed";
         fontSize = 18;
         fontColor = "66 229 244";
         justify = "right";
         autoSizeWidth = false;
         autoSizeHeight = true;
      };

      // (copied from LoginGui.gui, LoginMessageBoxDlg.gui and CreateAccountDlg.gui)
      // Login dialog:
      new GuiControl(LoginDlg) {
	      profile = "GuiDefaultProfile";
	      horizSizing = "right";
	      vertSizing = "bottom";
	      position = "0 0";
	      extent = "640 480";
	      minExtent = "8 8";
	      visible = "1";
	      helpTag = "0";

	      new ShellPaneCtrl() {
		      profile = "ShellDlgPaneProfile";
		      horizSizing = "center";
		      vertSizing = "center";
		      position = "72 143";
		      extent = "495 194";
		      minExtent = "48 92";
		      visible = "1";
		      helpTag = "0";
		      text = "LOGIN";
		      maxLength = "255";
		      noTitleBar = "0";

		      new GuiTextCtrl() {
			      profile = "ShellTextRightProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "37 47";
			      extent = "85 22";
			      minExtent = "8 8";
			      visible = "1";
			      helpTag = "0";
			      text = "Account Name:";
			      maxLength = "255";
		      };
		      new ShellTextEditCtrl(LoginEditBox) {
			      profile = "NewTextEditProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "118 39";
			      extent = "180 38";
			      minExtent = "32 38";
			      visible = "1";
			      variable = "$LoginName";
			      altCommand = "LoginProcess();";
			      helpTag = "0";
			      maxLength = "16";
			      historySize = "0";
			      password = "0";
			      glowOffset = "9 9";
		      };
		      new GuiTextCtrl() {
			      profile = "ShellTextRightProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "37 77";
			      extent = "85 22";
			      minExtent = "8 8";
			      visible = "1";
			      helpTag = "0";
			      text = "Password:";
			      maxLength = "255";
		      };
		      new GuiLoginPasswordCtrl(LoginPasswordBox) {
			      profile = "NewTextEditProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "118 69";
			      extent = "180 38";
			      minExtent = "32 38";
			      visible = "1";
			      variable = "$LoginPassword";
			      altCommand = "LoginProcess();";
			      helpTag = "0";
			      maxLength = "16";
			      historySize = "0";
			      password = "1";
			      glowOffset = "9 9";
		      };
		      new ShellBitmapButton() {
			      profile = "ShellButtonProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "300 39";
			      extent = "147 38";
			      minExtent = "32 38";
			      visible = "1";
			      command = "LoginProcess(false);";
			      helpTag = "0";
			      text = "LOG IN";
			      simpleStyle = "0";
		      };
		      new ShellBitmapButton() {
			      profile = "ShellButtonProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "300 69";
			      extent = "147 38";
			      minExtent = "32 38";
			      visible = "1";
			      command = "CreateAccount();";
			      helpTag = "0";
			      text = "CREATE NEW ACCOUNT";
			      simpleStyle = "0";
		      };
		      new ShellBitmapButton() {
			      profile = "ShellButtonProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "300 99";
			      extent = "147 38";
			      minExtent = "32 38";
			      visible = "1";
			      command = "LoginProcess(true);";
			      helpTag = "0";
			      text = "EDIT ACCOUNT";
			      simpleStyle = "0";
		      };
		      new ShellBitmapButton() {
			      profile = "ShellButtonProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "300 129";
			      extent = "147 38";
			      minExtent = "32 38";
			      visible = "1";
			      command = "quit();";
			      accelerator = "escape";
			      helpTag = "0";
			      text = "QUIT";
			      simpleStyle = "0";
		      };
		      new ShellToggleButton() {
			      profile = "ShellRadioProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "122 104";
			      extent = "167 27";
			      minExtent = "26 27";
			      visible = "1";
			      variable = "$pref::RememberPassword";
			      helpTag = "0";
			      text = "REMEMBER PASSWORD";
			      maxLength = "255";
		      };
		      new ShellBitmapButton() {
			      profile = "ShellButtonProfile";
			      horizSizing = "right";
			      vertSizing = "bottom";
			      position = "118 129";
			      extent = "180 38";
			      minExtent = "32 38";
			      visible = "1";
			      command = "PasswordProcess(true);";
			      helpTag = "0";
			      text = "EMAIL ME MY PASSWORD";
			      simpleStyle = "0";
		      };
	      };
      };

      // Edit Account dialog:
      new GuiControl(EditAccountDlg) {
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "0 0";
         extent = "640 480";
         minExtent = "8 8";
         visible = "1";
         helpTag = "0";
            open = "0";

         new ShellPaneCtrl() {
            profile = "ShellDlgPaneProfile";
            horizSizing = "center";
            vertSizing = "center";
            position = "70 105";
            extent = "500 255";
            minExtent = "48 92";
            visible = "1";
            helpTag = "0";
            text = "ACCOUNT INFORMATION";
            noTitleBar = "0";

            new ShellBitmapButton() {
               profile = "ShellButtonProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "282 200";
               extent = "128 38";
               minExtent = "32 38";
               visible = "1";
               command = "EditAccountDlg.onUpdate();";
               helpTag = "0";
               text = "UPDATE";
               simpleStyle = "0";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "126 51";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Password:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "126 81";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Confirm Password:";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "133 115";
               extent = "269 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountEmail";
               helpTag = "0";
               historySize = "0";
               maxLength = "128";
               password = "0";
               glowOffset = "9 9";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "37 125";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Email:";
            };
            new ShellToggleButton() {
               profile = "ShellRadioProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "63 156";
               extent = "366 30";
               minExtent = "26 27";
               visible = "1";
               variable = "$CreateAccountSendInfo";
               helpTag = "0";
               text = "SEND ME INFORMATION ABOUT TRIBES 2 AND OTHER PRODUCTS";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "222 73";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountConfirmPassword";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "1";
               glowOffset = "9 9";
            };
            new GuiLoginPasswordCtrl(EditAccountPasswordBox) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "222 43";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountPassword";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "1";
               glowOffset = "9 9";
            };
            new ShellBitmapButton() {
               profile = "ShellButtonProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "72 200";
               extent = "128 38";
               minExtent = "32 38";
               visible = "1";
               command = "EditAccountDlg.onDontUpdate();";
               accelerator = "escape";
               helpTag = "0";
               text = "DON\'T UPDATE";
               simpleStyle = "0";
            };
         };
      };

      // Create Account dialog:
      new GuiControl(CreateAccountDlg) {
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "0 0";
         extent = "640 480";
         minExtent = "8 8";
         visible = "1";
         helpTag = "0";
            open = "0";

         new ShellPaneCtrl() {
            profile = "ShellDlgPaneProfile";
            horizSizing = "center";
            vertSizing = "center";
            position = "70 36";
            extent = "500 408";
            minExtent = "48 92";
            visible = "1";
            helpTag = "0";
            text = "ACCOUNT INFORMATION";
            noTitleBar = "0";

            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 35";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Login Name:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 110";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Password:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 140";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Confirm Password:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 170";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "CD Key:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 218";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Email:";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "174 289";
               extent = "134 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "We are COPPA compliant.";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "73 305";
               extent = "351 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "If you are under 13, you are not allowed to create a Tribes 2 account.";
            };
            new GuiTextCtrl() {
               profile = "ShellTextRightProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "35 64";
               extent = "100 22";
               minExtent = "8 8";
               visible = "1";
               helpTag = "0";
               text = "Warrior Name:";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 27";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountLoginName";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(WarriorNameEntry) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 56";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountWarriorName";
               command = "WarriorNameEntry.validateWarriorName();";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 102";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountPassword";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "1";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 132";
               extent = "180 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountConfirmPassword";
               helpTag = "0";
               historySize = "0";
               maxLength = "16";
               password = "1";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(CreateAccountCDKey1) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 162";
               extent = "72 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountCDKey1.process();";
               helpTag = "0";
               historySize = "0";
               maxLength = "4";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(CreateAccountCDKey2) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "191 162";
               extent = "72 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountCDKey2.process();";
               helpTag = "0";
               historySize = "0";
               maxLength = "4";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(CreateAccountCDKey3) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "251 162";
               extent = "72 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountCDKey3.process();";
               helpTag = "0";
               historySize = "0";
               maxLength = "4";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(CreateAccountCDKey4) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "311 162";
               extent = "72 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountCDKey4.process();";
               helpTag = "0";
               historySize = "0";
               maxLength = "4";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl(CreateAccountCDKey5) {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "371 162";
               extent = "72 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountCDKey5.process();";
               helpTag = "0";
               historySize = "0";
               maxLength = "4";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellTextEditCtrl() {
               profile = "NewTextEditProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "131 209";
               extent = "269 38";
               minExtent = "32 38";
               visible = "1";
               variable = "$CreateAccountEmail";
               helpTag = "0";
               historySize = "0";
               maxLength = "128";
               password = "0";
               glowOffset = "9 9";
            };
            new ShellToggleButton() {
               profile = "ShellRadioProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "63 244";
               extent = "366 30";
               minExtent = "26 27";
               visible = "1";
               variable = "$CreateAccountSendInfo";
               helpTag = "0";
               text = "SEND ME INFORMATION ABOUT TRIBES 2 AND OTHER PRODUCTS";
            };
            new ShellToggleButton() {
               profile = "ShellRadioProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "148 325";
               extent = "201 30";
               minExtent = "26 27";
               visible = "1";
               variable = "$CreateAccountAgeGood";
               helpTag = "0";
               text = "I AM AT LEAST 13 YEARS OF AGE";
            };
            new ShellBitmapButton(CreateAccountSubmitBtn) {
               profile = "ShellButtonProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "282 351";
               extent = "128 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountDlg.onSubmit();";
               helpTag = "0";
               text = "SUBMIT";
               simpleStyle = "0";
            };
            new ShellBitmapButton() {
               profile = "ShellButtonProfile";
               horizSizing = "right";
               vertSizing = "bottom";
               position = "72 351";
               extent = "128 38";
               minExtent = "32 38";
               visible = "1";
               command = "CreateAccountDlg.onCancel();";
               accelerator = "escape";
               helpTag = "0";
               text = "CANCEL";
               simpleStyle = "0";
            };
         };
      };

      if($Login == false)
         $LoginName = $pref::LastLoginName;
   }
   
   // Set the default cursor so it shows in all login modes
   Canvas.setCursor( "DefaultCursor" );

   // Check for software rendering and bail, if that's what it is...
   if ( ($platform $= "Linux") &&
        ((strstr($pref::Video::defaultsRenderer, "Indirect") != -1) ||
         (strstr($pref::Video::defaultsRenderer, "Mesa X11") != -1))  ) {
      LoginMessageBox( "ERROR", "Your 3D renderer (" @ $pref::Video::defaultsRenderer @ ") does not appear to be configured for hardware acceleration.", "OK", "quit();" );
      return;
   }

   // Finally, let's get it on:   
   Canvas.setContent( StartupGui );

   if ( !$pref::SkipIntro )
   {
      Canvas.pushDialog( IntroGui );
      Canvas.hideCursor();
      IntroGui.play();
      schedule( 100, 0, checkIntroDone );
   }
   else
      StartLoginProcess();
}
