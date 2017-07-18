//------------------------------------------------------------------------------
//
// OptionsDlg.cs
//
//------------------------------------------------------------------------------
$max_screenerror = 25;
$min_TSScreenError = 2;
$max_TSScreenError = 20;
$min_TSDetailAdjust = 0.6;
$max_TSDetailAdjust = 1.0;
//------------------------------------------------------------------------------
function OptionsDlg::onWake( %this )
{
   // Save the cursor state since it's possible we will be coming from the sim:
   %this.wasCursorOn = Canvas.isCursorOn();
   if ( !%this.wasCursorOn )
      CursorOn();

   $enableDirectInput = "1";
   enableDirectInput();
   
	OP_VideoPane.setVisible( false ); 
	OP_GraphicsPane.setVisible( false );
	OP_TexturesPane.setVisible( false );
	OP_SoundPane.setVisible( false );
	OP_ControlsPane.setVisible( false );
	OP_NetworkPane.setVisible( false );
	OP_GamePane.setVisible( false );

	OP_VideoTab.setValue( false ); 
	OP_GraphicsTab.setValue( false );
	OP_TexturesTab.setValue( false );
	OP_SoundTab.setValue( false );
	OP_ControlsTab.setValue( false );
	OP_NetworkTab.setValue( false );
	OP_GameTab.setValue( false );

	// Initialize the Video Pane controls:
	// First the Video Driver menu:
	%buffer = getDisplayDeviceList();
	%count = getFieldCount( %buffer );
	for ( %i = 0; %i < %count; %i++ )
		OP_VideoDriverMenu.add( getField( %buffer, %i ), %i );

	// Select the current device:
	%selId = OP_VideoDriverMenu.findText( $pref::Video::displayDevice );
	if ( %selId == -1 )
		%selId = 0; // How did THAT happen?
	OP_VideoDriverMenu.setSelected( %selId );
	OP_VideoDriverMenu.onSelect( %selId, "" );

	OP_FullScreenTgl.setValue( $pref::Video::fullScreen );
	OP_FullScreenTgl.onAction();

	OP_ApplyBtn.setActive( false );

	// Initialize the Graphics Options controls:
	OptionsDlg::deviceDependent( %this );

   OP_GammaSlider.setValue( $pref::OpenGL::gammaCorrection );
   OP_TerrainSlider.setValue( $max_screenerror - $pref::Terrain::screenError );
   OP_ShapeSlider.setValue( ( $max_TSScreenError - $pref::TS::screenError ) / ( $max_TSScreenError - $min_TSScreenError ) );
   OP_ShadowSlider.setValue( $pref::Shadows );
   OP_SkyDetailMenu.init();
   if ( !$pref::SkyOn )
      %selId = 5;
   else if ( $pref::numCloudLayers >= 0 && $pref::numCloudLayers < 4 )
      %selId = 4 - $pref::numCloudLayers;
   else
      %selId = 1;
   OP_SkyDetailMenu.setSelected( %selId );
   OP_SkyDetailMenu.setText( OP_SkyDetailMenu.getTextById( %selId ) );
   OP_VertexLightTgl.setValue( $pref::Interior::VertexLighting );
   OP_PlayerRenderMenu.init();
   %selId = $pref::Player::renderMyPlayer | ( $pref::Player::renderMyItems << 1 );
   OP_PlayerRenderMenu.setSelected( %selId );
   OP_VisibleDistanceSlider.setValue( $pref::VisibleDistanceMod );

	// Initialize the Textures Options controls:
   OP_ShapeTexSlider.setValue( 5 - $pref::OpenGL::mipReduction );
   OP_TerrainTexSlider.setValue( 6 - $pref::Terrain::texDetail );
   OP_BuildingTexSlider.setValue( 5 - $pref::OpenGL::interiorMipReduction );
   OP_SkyTexSlider.setValue( 5 - $pref::OpenGL::skyMipReduction );

	// Initialize the Sound Options controls:
   // provider menu
   %count = alxGetContexti(ALC_PROVIDER_COUNT);
   for(%i = 0; %i < %count; %i++)
      OP_AudioProviderMenu.add(alxGetContextstr(ALC_PROVIDER_NAME, %i), %i);
   %selId = alxGetContexti(ALC_PROVIDER);
   OP_AudioProviderMenu.setSelected(%selId);
   OP_AudioResetProvider.setActive(false);

   %active = audioIsEnvironmentProvider(alxGetContextstr(ALC_PROVIDER_NAME, %selId));
   OP_AudioEnvironmentTgl.setActive(%active);

   // speaker menu
   %count = alxGetContexti(ALC_SPEAKER_COUNT);
   for(%i = 0; %i < %count; %i++)
      OP_AudioSpeakerMenu.add(alxGetContextstr(ALC_SPEAKER_NAME, %i), %i);
   %selId = alxGetContexti(ALC_SPEAKER);
   OP_AudioSpeakerMenu.setSelected(%selId);
	OP_AudioSpeakerMenu.onSelect(%selId, "");

   OP_MasterVolumeSlider.setValue( $pref::Audio::masterVolume );
   OP_EffectsVolumeSlider.setValue( $pref::Audio::effectsVolume );
   OP_VoiceBindVolumeSlider.setValue( $pref::Audio::radioVolume );
   OP_GuiVolumeSlider.setValue( $pref::Audio::guiVolume );
   OP_MusicTgl.onAction();
   OP_MusicVolumeSlider.setValue( $pref::Audio::musicVolume );
   OP_MicrophoneEnabledTgl.onAction();
   OP_MicrophoneVolumeSlider.setValue( $pref::Audio::voiceVolume );
   OP_InputBoostSlider.setValue( $pref::Audio::captureGainScale );
   updateInputBoost();

	// Initialize the Control Options controls:
	OP_RemapList.fillList();
   // JOYSTICK SUPPORT WILL BE RE-ENABLED IN THE PATCH
   //if ( isJoystickDetected() )
   if ( false )
   {      
      OP_JoystickTgl.setActive( true );
      OP_ConfigureJoystickBtn.setActive( $pref::Input::JoystickEnabled );
   }
   else
   {
      OP_JoystickTgl.setVisible( false ); // PATCH
      OP_JoystickTgl.setActive( false );
      $pref::Input::JoystickEnabled = false;
      OP_ConfigureJoystickBtn.setVisible( false ); // PATCH
      OP_ConfigureJoystickBtn.setActive( false );
   }

	// Initialize the Network Options controls:
   OP_PacketRateSlider.setValue( $pref::Net::PacketRateToClient );
   OP_PacketSizeSlider.setValue( $pref::Net::PacketSize );
   OP_UpdateRateSlider.setValue( $pref::Net::PacketRateToServer );
   if ( !OP_MasterServerMenu.size() )
      OP_MasterServerMenu.init();
   %selId = OP_MasterServerMenu.findText( $pref::Net::DisplayOnMaster );
   if ( %selId == -1 )
      %selId = 1;
   OP_MasterServerMenu.setSelected( %selId );
   if ( !OP_RegionMenu.size() )
      OP_RegionMenu.init();
   OP_RegionMenu.setSelected( $pref::Net::RegionMask );

	// Initialize the Game Options controls:
   OP_ZoomSpeedSlider.setValue( 500 - $pref::Player::zoomSpeed );
   OP_LaunchScreenMenu.init();

   %selId = OP_LaunchScreenMenu.findText( $pref::Shell::LaunchGui );
   if ( %selId == -1 )
      %selId = 1;
   OP_LaunchScreenMenu.setText( OP_LaunchScreenMenu.getTextById( %selId ) );
   OP_LaunchScreenMenu.setSelected( %selId );

	%this.setPane( %this.pane );
}

//------------------------------------------------------------------------------
function OptionsDlg::deviceDependent( %this )
{
   if ( $SwapIntervalSupported )
   {
      OP_VSyncTgl.setValue( $pref::Video::disableVerticalSync );   
      OP_VSyncTgl.setActive( true );
   }
   else
   {
      OP_VSyncTgl.setValue( false );   
      OP_VSyncTgl.setActive( false );   
   }

	OP_TexQualityMenu.init();
   if ( $pref::OpenGL::forcePalettedTexture )
   {
      $pref::OpenGL::force16bittexture = false;
      %selId = 1;
   }
   else if ( $pref::OpenGL::force16bittexture )
      %selId = 2;
   else
      %selId = 3;
   OP_TexQualityMenu.setSelected( %selId );

	OP_CompressMenu.init();
   if ( $TextureCompressionSupported && !$pref::OpenGL::disableARBTextureCompression )
   {
      OP_CompressLabel.setVisible( true );
      OP_CompressLabel_Disabled.setVisible( false );
      OP_CompressMenu.setActive( true );
      if ( !$pref::OpenGL::allowCompression )
         OP_CompressMenu.setSelected( 1 );
      else if ( $pref::OpenGL::compressionHint $= "GL_NICEST" )
         OP_CompressMenu.setSelected( 3 );
      else
         OP_CompressMenu.setSelected( 2 );
   }
   else
   {
      OP_CompressLabel_Disabled.setVisible( true );
      OP_CompressLabel.setVisible( false );
      OP_CompressMenu.setActive( false );
      OP_CompressMenu.setText( "None" );
   }

   if ( $FogCoordSupported )
   {
      OP_IntTexturedFogTgl.setValue( $pref::Interior::TexturedFog );
      OP_IntTexturedFogTgl.setActive( true );
   }
   else
   {
      OP_IntTexturedFogTgl.setValue( true );
      OP_IntTexturedFogTgl.setActive( false );
   }

	OP_AnisotropySlider.setValue( $pref::OpenGL::anisotropy );
   OP_AnisotropySlider.setActive( $AnisotropySupported );
   if ( $AnisotropySupported )
   {
      OP_AnisotropyLabel.setVisible( true );
      OP_AnisotropyLabel_Disabled.setVisible( false );
   }
   else
   {
      OP_AnisotropyLabel_Disabled.setVisible( true );
      OP_AnisotropyLabel.setVisible( false );
   }
}

//------------------------------------------------------------------------------
function OptionsDlg::onSleep( %this )
{
   // Restore the cursor state:
   if ( !%this.wasCursorOn )
      CursorOff();

   $enableDirectInput = "0";
   disableDirectInput();
   
	OP_VideoDriverMenu.clear();
	OP_ResMenu.clear();
	OP_BPPMenu.clear();
   OP_AudioProviderMenu.clear();
   OP_AudioSpeakerMenu.clear();

   if ( isObject( ServerConnection ) && isTextureFlushRequired() )
      MessageBoxYesNo( "WARNING", "You have made changes that require Tribes 2 to flush the texture cache.  "  
            @ "Doing this while the game is running can take a long time.  "
            @ "Do you wish to continue?",
            "OptionsDlg.saveSettings();", "returnFromSettings();" );
   else
	   %this.saveSettings();
}

//------------------------------------------------------------------------------
function isTextureFlushRequired()
{
   if ( $pref::Interior::VertexLighting != OP_VertexLightTgl.getValue() )
      return( true );

   if ( $pref::OpenGL::mipReduction != 5 - mFloor( OP_ShapeTexSlider.getValue() ) )
      return( true );

   if ( $pref::OpenGL::interiorMipReduction != 5 - mFloor( OP_BuildingTexSlider.getValue() ) )
      return( true );

   if ( $pref::OpenGL::skyMipReduction != 5 - mFloor( OP_SkyTexSlider.getValue() ) )
      return( true );

   if ( $AnisotropySupported && $pref::OpenGL::anisotropy != OP_AnisotropySlider.getValue() )
      return( true );

   %id = OP_TexQualityMenu.getSelected();
   if ( $pref::OpenGL::forcePalettedTexture )
   {
      if ( %id != 1 )
         return( true );
   }
   else if ( $pref::OpenGL::force16bittexture )
   {
      if ( %id != 2 )
         return( true );
   }
   else if ( %id != 3 )
      return( true );

   if ( $TextureCompressionSupported && !$pref::OpenGL::disableARBTextureCompression )
   {
      %id = OP_CompressMenu.getSelected();
      if ( $pref::OpenGL::allowCompression )
      {
         if ( $pref::OpenGL::compressionHint $= "GL_FASTEST" )
         {
            if ( %id != 2 )
               return( true );
         }
         else if ( $pref::OpenGL::compressionHint $= "GL_NICEST" )
         {
            if ( %id != 3 )
               return( true );
         }
         else if ( %id == 1 )
            return( true );
      }
      else if ( %id > 1 )
         return( true );
   }

   return( false );
}

//------------------------------------------------------------------------------
function returnFromSettings()
{
	// to unpause singlePlayerGame when returning from options
   if ( isObject( Game ) )
	   Game.OptionsDlgSleep();
}

//------------------------------------------------------------------------------
function OptionsDlg::saveSettings( %this )
{
   // Save off any prefs that don't auto-update:
   %flushTextures = false;

   if ( $SwapIntervalSupported && OP_VSyncTgl.getValue() != $pref::Video::disableVerticalSync )
   {
      $pref::Video::disableVerticalSync = OP_VSyncTgl.getValue();
      setVerticalSync( !$pref::Video::disableVerticalSync );
   }

   %temp = OP_SkyDetailMenu.getSelected();
   if ( %temp == 5 )
      $pref::SkyOn = false;
   else
   {
      $pref::SkyOn = true;
      $pref::numCloudLayers = ( 4 - %temp );
   }

   if ( $FogCoordSupported )
      $pref::Interior::TexturedFog = OP_IntTexturedFogTgl.getValue();

   if ( $pref::Interior::VertexLighting != OP_VertexLightTgl.getValue() )
   {
      $pref::Interior::VertexLighting = OP_VertexLightTgl.getValue();
      %flushTextures = true;
   }

   %temp = OP_PlayerRenderMenu.getSelected();
   $pref::Player::renderMyPlayer = %temp & 1;
   $pref::Player::renderMyItems = %temp & 2;

   switch ( OP_TexQualityMenu.getSelected() )
   {
      case 1:  // 8-bit
         if ( !$pref::OpenGL::forcePalettedTexture || $pref::OpenGL::force16bittexture )
         {
            $pref::OpenGL::forcePalettedTexture = true;
            $pref::OpenGL::force16bittexture = false;
            %flushTextures = true;
         }
      case 2:  // 16-bit
         if ( $pref::OpenGL::forcePalettedTexture || !$pref::OpenGL::force16bittexture )
         {
            $pref::OpenGL::forcePalettedTexture = false;
            $pref::OpenGL::force16bittexture = true;
            %flushTextures = true;
         }
      case 3:  // 32-bit
         if ( $pref::OpenGL::forcePalettedTexture || $pref::OpenGL::force16bittexture )
         {
            $pref::OpenGL::forcePalettedTexture = false;
            $pref::OpenGL::force16bittexture = false;
            %flushTextures = true;
         }
   }
   OP_TexQualityMenu.clear();

   $pref::Terrain::texDetail = 6 - mFloor( OP_TerrainTexSlider.getValue() );

   %temp = 5 - mFloor( OP_ShapeTexSlider.getValue() );
   if ( $pref::OpenGL::mipReduction != %temp )
   {
      $pref::OpenGL::mipReduction = %temp;
      setOpenGLMipReduction( $pref::OpenGL::mipReduction );
      %flushTextures = true;
   }

   %temp = 5 - mFloor( OP_BuildingTexSlider.getValue() );
   if ( $pref::OpenGL::interiorMipReduction != %temp )
   {
      $pref::OpenGL::interiorMipReduction = %temp;
      setOpenGLInteriorMipReduction( $pref::OpenGL::interiorMipReduction );
      %flushTextures = true;
   }

   %temp = 5 - mFloor( OP_SkyTexSlider.getValue() );
   if ( $pref::OpenGL::skyMipReduction != %temp )
   {
      $pref::OpenGL::skyMipReduction = %temp;
      setOpenGLSkyMipReduction( $pref::OpenGL::skyMipReduction ); 
      %flushTextures = true;
   }

   if ( $TextureCompressionSupported && !$pref::OpenGL::disableARBTextureCompression )
   {
      %temp = OP_CompressMenu.getSelected();
      if ( $pref::OpenGL::allowCompression )
      {
         switch ( %temp )
         {
            case 2:
               if ( $pref::OpenGL::compressionHint !$= "GL_FASTEST" )
               {
                  $pref::OpenGL::compressionHint = "GL_FASTEST";
                  setOpenGLTextureCompressionHint( $pref::OpenGL::compressionHint );
                  %flushTextures = true;
               }

            case 3:
               if ( $pref::OpenGL::compressionHint !$= "GL_NICEST" )
               {
                  $pref::OpenGL::compressionHint = "GL_NICEST";
                  setOpenGLTextureCompressionHint( $pref::OpenGL::compressionHint );
                  %flushTextures = true;
               }

            default:  // None
               $pref::OpenGL::allowCompression = false;
               %flushTextures = true;
         }
      }
      else if ( %temp > 1 )
      {
         $pref::OpenGL::allowCompression = true;
         if ( %temp == 3 )
            $pref::OpenGL::compressionHint = "GL_NICEST";
         else
            $pref::OpenGL::compressionHint = "GL_FASTEST";
         setOpenGLTextureCompressionHint( $pref::OpenGL::compressionHint );
         %flushTextures = true;
      }
   }
   OP_CompressMenu.clear();
   
   if ( $AnisotropySupported )
   {
      %temp = OP_AnisotropySlider.getValue();
      if ( $pref::OpenGL::anisotropy != %temp )
      {
         $pref::OpenGL::anisotropy = %temp;
         setOpenGLAnisotropy( $pref::OpenGL::anisotropy );
         %flushTextures = true;
      }
   }

   $pref::Terrain::screenError = $max_screenerror - mFloor( OP_TerrainSlider.getValue() );    
   $pref::TS::screenError = $max_TSScreenError - mFloor( OP_ShapeSlider.getValue() * ( $max_TSScreenError - $min_TSScreenError ) ); 
   $pref::TS::detailAdjust = $min_TSDetailAdjust + OP_ShapeSlider.getValue() * ( $max_TSDetailAdjust - $min_TSDetailAdjust );
   $pref::Shadows = OP_ShadowSlider.getValue();
   setShadowDetailLevel( $pref::Shadows );
   $pref::VisibleDistanceMod = OP_VisibleDistanceSlider.getValue();

   $pref::Audio::musicVolume = OP_MusicVolumeSlider.getValue();
   $pref::Audio::masterVolume = OP_MasterVolumeSlider.getValue();
   $pref::Audio::effectsVolume = OP_EffectsVolumeSlider.getValue();
   alxSetChannelVolume( $EffectAudioType, $pref::Audio::effectsVolume );
   $pref::Audio::voiceVolume = OP_MicrophoneVolumeSlider.getValue();
   alxSetChannelVolume( $VoiceAudioType, $pref::Audio::voiceVolume );
   $pref::Audio::radioVolume = OP_VoiceBindVolumeSlider.getValue();
   alxSetChannelVolume( $ChatAudioType, $pref::Audio::radioVolume );
   $pref::Audio::guiVolume = OP_GuiVolumeSlider.getValue();
   alxSetChannelVolume( $GuiAudioType, $pref::Audio::guiVolume);
   $pref::Audio::captureGainScale = OP_InputBoostSlider.getValue();
   if ( !$missionRunning )
      MusicPlayer.stop();

   $pref::Net::PacketRateToClient = mFloor( OP_PacketRateSlider.getValue() );
   $pref::Net::PacketSize = mFloor( OP_PacketSizeSlider.getValue() );
   $pref::Net::PacketRateToServer = mFloor( OP_UpdateRateSlider.getValue() );
   // check the max rate:
   if ( isObject( ServerConnection ) )
      ServerConnection.checkMaxRate();
   if ( isObject( ClientGroup ) )
   {
      %count = ClientGroup.getCount();
      for ( %i = 0; %i < %count; %i++ )
      {
         %cl = ClientGroup.getObject( %i );
         %cl.checkMaxRate();
      }
   }

   $pref::Player::zoomSpeed = 500 - mFloor( OP_ZoomSpeedSlider.getValue() );
   setZoomSpeed( $pref::Player::zoomSpeed );

   $pref::Shell::LaunchGui = OP_LaunchScreenMenu.getText();

   export( "$pref::*", "prefs/ClientPrefs.cs", false );
   saveActiveMapFile();

	if ( %flushTextures )
	{
      // Give the Options Dialog a chance to go away:
      OptionsDlg.schedule( 0, doTextureFlush ); 
   }

   returnFromSettings();
}

//------------------------------------------------------------------------------
function OptionsDlg::doTextureFlush( %this )
{
   MessagePopup( "PLEASE WAIT", "Flushing texture cache...\nThis may take a while" ); 
   Canvas.repaint();
   flushTextureCache();
   CloseMessagePopup();
}

//------------------------------------------------------------------------------
function OptionsDlg::setPane( %this, %pane )
{
   if((%this.pane $= "Sound") && !$missionRunning)
      MusicPlayer.stop();
   
	if ( %this.pane !$= "None" )
	{
		%paneCtrl = "OP_" @ %this.pane @ "Pane";
		%paneCtrl.setVisible( false );

		%tabCtrl = "OP_" @ %this.pane @ "Tab";
		%tabCtrl.setValue( false );
	}
		
	%paneCtrl = "OP_" @ %pane @ "Pane";
	%paneCtrl.setVisible( true );
	
	%tabCtrl = "OP_" @ %pane @ "Tab";
	%tabCtrl.setValue( true );
	
	%this.pane = %pane; 
}

//------------------------------------------------------------------------------
function OptionsDlg::applyGraphicChanges( %this )
{
	%newDriver = OP_VideoDriverMenu.getText();
	%newRes = OP_ResMenu.getText();
	%newBpp = OP_BPPMenu.getText();
	%newFullScreen = OP_FullScreenTgl.getValue();

	if ( %newDriver !$= $pref::Video::displayDevice )
	{
		setDisplayDevice( %newDriver, firstWord( %newRes ), getWord( %newRes, 1 ), %newBpp, %newFullScreen );
		OptionsDlg::deviceDependent( %this );
	}
	else
		setScreenMode( firstWord( %newRes ), getWord( %newRes, 1 ), %newBpp, %newFullScreen );

	OP_ApplyBtn.updateState();
}


//------------------------------------------------------------------------------
function OP_VideoDriverMenu::onSelect( %this, %id, %text )
{
	// Attempt to keep the same res and bpp settings:
	if ( OP_ResMenu.size() > 0 )
		%prevRes = OP_ResMenu.getText();
	else
		%prevRes = getWords( $pref::Video::resolution, 0, 1 );

	// Check if this device is full-screen only:
	if ( isDeviceFullScreenOnly( %this.getText() ) )
	{
		OP_FullScreenTgl.setValue( true );
		OP_FullScreenTgl.setActive( false );
		OP_FullScreenTgl.onAction();
	}
	else
		OP_FullScreenTgl.setActive( true );

	if ( OP_FullScreenTgl.getValue() )
	{
		if ( OP_BPPMenu.size() > 0 )
			%prevBPP = OP_BPPMenu.getText();
		else
			%prevBPP = getWord( $pref::Video::resolution, 2 );
	}

	// Fill the resolution and bit depth lists:
	OP_ResMenu.init( %this.getText(), OP_FullScreenTgl.getValue() );
	OP_BPPMenu.init( %this.getText() );

	// Try to select the previous settings:
	%selId = OP_ResMenu.findText( %prevRes );
	if ( %selId == -1 )
		%selId = 0;
	OP_ResMenu.setSelected( %selId );

	if ( OP_FullScreenTgl.getValue() )
	{
		%selId = OP_BPPMenu.findText( %prevBPP );
		if ( %selId == -1 )
			%selId = 0;
		OP_BPPMenu.setSelected( %selId );
		OP_BPPMenu.setText( OP_BPPMenu.getTextById( %selId ) );
	}
	else
		OP_BPPMenu.setText( "Default" );

	OP_ApplyBtn.updateState();
}

//------------------------------------------------------------------------------
function OP_ResMenu::init( %this, %device, %fullScreen )
{
	%this.clear();
	%resList = getResolutionList( %device );
	%resCount = getFieldCount( %resList );
	%deskRes = getDesktopResolution();
   %count = 0;
	for ( %i = 0; %i < %resCount; %i++ )
	{
		%res = getWords( getField( %resList, %i ), 0, 1 );

		if ( !%fullScreen )
		{
			if ( firstWord( %res ) >= firstWord( %deskRes ) )
				continue;
			if ( getWord( %res, 1 ) >= getWord( %deskRes, 1 ) )
				continue;
		}

		// Only add to list if it isn't there already:
		if ( %this.findText( %res ) == -1 )
      {
			%this.add( %res, %count );
         %count++;
      }
	}
}

//------------------------------------------------------------------------------
function OP_ResMenu::onSelect( %this, %id, %text )
{
	OP_ApplyBtn.updateState();
}

//------------------------------------------------------------------------------
function OP_BPPMenu::init( %this, %device )
{
	%this.clear();

	if ( %device $= "Voodoo2" )
		%this.add( "16", 0 );
	else
	{
		%resList = getResolutionList( %device );
		%resCount = getFieldCount( %resList );
      %count = 0;
		for ( %i = 0; %i < %resCount; %i++ )
		{
			%bpp = getWord( getField( %resList, %i ), 2 );

			// Only add to list if it isn't there already:
			if ( %this.findText( %bpp ) == -1 )
         {
				%this.add( %bpp, %count );
            %count++;
         }
		}
	}
}

//------------------------------------------------------------------------------
function OP_BPPMenu::onSelect( %this, %id, %text )
{
	OP_ApplyBtn.updateState();
}

//------------------------------------------------------------------------------
function OP_FullScreenTgl::onAction( %this )
{
	// Attempt to maintain current settings:
	%selId = OP_ResMenu.getSelected();
	if ( %selId == -1 )
		%selId = 0;
	%prevRes = OP_ResMenu.getTextById( %selId );

	OP_ResMenu.init( OP_VideoDriverMenu.getText(), %this.getValue() );

	%selId = OP_ResMenu.findText( %prevRes );
	if ( %selId == -1 )
		%selId = 0;
	OP_ResMenu.setSelected( %selId );

	if ( %this.getValue() )
	{
		%selId = OP_BPPMenu.findText( getWord( $pref::Video::resolution, 2 ) );
		if ( %selId == - 1 )
			%selId = 0;
		OP_BPPMenu.setSelected( %selId );
		OP_BPPMenu.setText( OP_BPPMenu.getTextById( %selId ) );
		OP_BPPMenu.setActive( true );
	}
	else
	{
		OP_BPPMenu.setText( "Default" );
		OP_BPPMenu.setActive( false );
	}

	OP_ApplyBtn.updateState();
}

//------------------------------------------------------------------------------
function OP_ApplyBtn::updateState( %this )
{
	%active = false;

	if ( OP_VideoDriverMenu.getText() !$= $pref::Video::displayDevice )
		%active = true;
	else if ( OP_ResMenu.getText() !$= getWords( $pref::Video::resolution, 0, 1 ) )
		%active = true;
	else if ( OP_FullScreenTgl.getValue() != $pref::Video::fullScreen )
		%active = true;
	else if ( OP_FullScreenTgl.getValue() )
	{
		if ( OP_BPPMenu.getText() !$= getWord( $pref::Video::resolution, 2 ) )
			%active = true;
	}

	%this.setActive( %active );
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Graphics:
//
function updateGammaCorrection()
{
   $pref::OpenGL::gammaCorrection = OP_GammaSlider.getValue();
   videoSetGammaCorrection( $pref::OpenGL::gammaCorrection ); 
}

//------------------------------------------------------------------------------
function updateTerrainDetail()
{
   $pref::Terrain::screenError = $max_screenerror - mFloor( OP_TerrainSlider.getValue());
   if ( OP_TerrainSlider.getValue() != $max_screenerror - $pref::Terrain::screenError )
      OP_TerrainSlider.setValue( $max_screenerror - $pref::Terrain::screenError );
}

//------------------------------------------------------------------------------
function OP_SkyDetailMenu::init( %this )
{
   %this.clear();
   %this.add( "Full Sky", 1 );
   %this.add( "Two Cloud Layers", 2 );
   %this.add( "One Cloud Layer", 3 );
   %this.add( "Sky Box Only", 4 );
   %this.add( "No Sky", 5 );
}

//------------------------------------------------------------------------------
function OP_PlayerRenderMenu::init( %this )
{
   %this.clear();
   %this.add( "Player and Items", 3 );
   %this.add( "Player only", 1 );
   %this.add( "Items only", 2 );
   %this.add( "Neither Player nor Items", 0 );
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Textures:
//
function OP_CompressMenu::init( %this )
{
   %this.clear();
   %this.add( "None", 1 );
   %this.add( "Fastest", 2 );
   %this.add( "Nicest", 3 );
}

//------------------------------------------------------------------------------
function OP_TexQualityMenu::init( %this )
{
   %this.clear();
   if ( $PalettedTextureSupported )
      %this.add( "Palletized", 1 );
   %this.add( "16 bit", 2 );
   %this.add( "32 bit", 3 );
}

//------------------------------------------------------------------------------
function OP_TexQualityMenu::onSelect( %this, %id, %text )
{
   if ( %id == 1 )
   {
      // Disable these with palletized textures by default:
      OP_EnvMapTgl.setValue( false );
      //OP_EnvMapTgl.setActive( false );
      OP_IntEnvMapTgl.setValue( false );
      //OP_IntEnvMapTgl.setActive( false );
   }
//    else
//    {
//       OP_EnvMapTgl.setActive( true );
//       OP_IntEnvMapTgl.setActive( true );
//    }
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Audio:
//
function setAudioProvider(%idx)
{
   alxContexti(ALC_PROVIDER, %idx);
   $pref::Audio::provider = alxGetContextstr(ALC_PROVIDER_NAME, %idx);

   %active = audioIsEnvironmentProvider($pref::Audio::provider);
   OP_AudioEnvironmentTgl.setActive(%active);

   audioUpdateProvider($pref::Audio::provider);
   OP_AudioProviderMenu.setSelected(%idx);
}

//------------------------------------------------------------------------------
function OP_AudioEnvironmentTgl::onAction(%this)
{
   alxEnableEnvironmental(%this.getValue());
}

//------------------------------------------------------------------------------
function OP_AudioProviderMenu::onSelect(%this, %id, %text)
{
   if(%id != $Audio::originalProvider)
   {
      if(!%this.seenWarning)
      {
         MessageBoxOK("Warning", "Changing sound drivers may result in incompatibilities and game oddities. If you experience such oddities, hit \"Reset\" to restore defaults.", "");
         %this.seenWarning = true;   
      }
      OP_AudioResetProvider.setActive(true);
   }
   setAudioProvider(%id);
}

//------------------------------------------------------------------------------
function OP_AudioResetProvider::onAction(%this)
{
   setAudioProvider($Audio::originalProvider);
   %this.setActive(false);
}

//------------------------------------------------------------------------------
function OP_AudioSpeakerMenu::onSelect(%this, %id, %text)
{
   alxContexti(ALC_SPEAKER, %id);
   $pref::Audio::speakerType = alxGetContextstr(ALC_SPEAKER_NAME, %id);
}

//------------------------------------------------------------------------------
function OP_MusicTgl::onAction( %this )
{
   %on = %this.getValue();
   OP_MusicVolumeLabel.setVisible( %on );
   OP_MusicVolumeLabel_Disabled.setVisible( !%on );
   OP_MusicVolumeSlider.setActive( %on );
   $pref::Audio::musicEnabled = %on;

   if ( %on )
      MusicPlayer.play();
   else
      MusicPlayer.stop();
}

//------------------------------------------------------------------------------
function updateMusicVolume()
{
   %volume = OP_MusicVolumeSlider.getValue();
   alxSetChannelVolume( $MusicAudioType, %volume );
}

//------------------------------------------------------------------------------
function updateGuiVolume()
{
   %volume = OP_GuiVolumeSlider.getValue();
   alxSetChannelVolume( $GuiAudioType, %volume );
}

//------------------------------------------------------------------------------
function updateMasterVolume()
{
   %volume = OP_MasterVolumeSlider.getValue();
   alxListenerf( AL_GAIN_LINEAR, %volume );
}

//------------------------------------------------------------------------------
function OP_MicrophoneEnabledTgl::onAction( %this )
{
   %on = %this.getValue();
   OP_RecordTestBtn.setActive( %on );
   OP_MicVolumeLabel.setVisible( %on );
   OP_MicVolumeLabel_Disabled.setVisible( !%on );
   OP_MicrophoneVolumeSlider.setActive( %on );
   OP_InputBoostLabel.setVisible( %on );
   OP_InputBoostLabel_Disabled.setVisible( !%on );
   OP_InputBoostSlider.setActive( %on );
   OP_InputBoostPercentTxt.setVisible( %on );
}

//------------------------------------------------------------------------------
function updateInputBoost()
{
   %val = OP_InputBoostSlider.getValue();
   alxSetCaptureGainScale( %val );
   %val = mFloor(%val * 100);
   OP_InputBoostPercentTxt.setValue(%val @ "%");
}

//------------------------------------------------------------------------------
function OP_RecordTestBtn::onAction( %this )
{
   alxCaptureStart(true);
}

//------------------------------------------------------------------------------
function localCaptureStart( %method )
{
   if(%method $= "record")
   {
      OP_RecordTestBtn.setActive(false);
      OP_RecordTestBtn.setValue(">> Recording <<");
   }
   else
   {
      OP_RecordTestBtn.setActive(false);
      OP_RecordTestBtn.setValue(">> Playing <<");
   }
}

//------------------------------------------------------------------------------
function localCaptureStop( %method )
{
   if(%method $= "play")
   {
      OP_RecordTestBtn.setActive(true);
      OP_RecordTestBtn.setValue("Test Record");
   }
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Driver Info dialog:
//
function DriverInfoDlg::onWake( %this )
{
   %headerStyle = "<font:" @ $ShellLabelFont @ ":" @ $ShellFontSize @ "><color:00DC00>";
   %infoString = getVideoDriverInfo();
   %displayText = "<spush>" @ %headerStyle @ "VENDOR:<spop>" NL
                  "  " @ getField( %infoString, 0 ) NL
                  "<spush>" @ %headerStyle @ "RENDERER:<spop>" NL
                  "  " @ getField( %infoString, 1 ) NL
                  "<spush>" @ %headerStyle @ "VERSION<spop> " @ getField( %infoString, 2 ) NL
                  "\n" @
                  "<spush>" @ %headerStyle @ "SUPPORTED OPENGL EXTENSIONS:<spop><lmargin:5><just:center>" NL
                  getField( %infoString, 3 );

   DriverInfoText.setText( %displayText );
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Control remapper section:
//
$RemapCount = 0;
$RemapName[$RemapCount] = "Forward";
$RemapCmd[$RemapCount] = "moveforward";
$RemapCount++;
$RemapName[$RemapCount] = "Backward";
$RemapCmd[$RemapCount] = "movebackward";
$RemapCount++;
$RemapName[$RemapCount] = "Strafe Left";
$RemapCmd[$RemapCount] = "moveleft";
$RemapCount++;
$RemapName[$RemapCount] = "Strafe Right";
$RemapCmd[$RemapCount] = "moveright";
$RemapCount++;
$RemapName[$RemapCount] = "Turn Left";
$RemapCmd[$RemapCount] = "turnLeft";
$RemapCount++;
$RemapName[$RemapCount] = "Turn Right";
$RemapCmd[$RemapCount] = "turnRight";
$RemapCount++;
$RemapName[$RemapCount] = "Look Up";
$RemapCmd[$RemapCount] = "panUp";
$RemapCount++;
$RemapName[$RemapCount] = "Look Down";
$RemapCmd[$RemapCount] = "panDown";
$RemapCount++;
$RemapName[$RemapCount] = "Jump";
$RemapCmd[$RemapCount] = "jump";
$RemapCount++;
$RemapName[$RemapCount] = "Jet Pack";
$RemapCmd[$RemapCount] = "mouseJet";
$RemapCount++;
$RemapName[$RemapCount] = "Fire Weapon";
$RemapCmd[$RemapCount] = "mouseFire";
$RemapCount++;
$RemapName[$RemapCount] = "Zoom";
$RemapCmd[$RemapCount] = "toggleZoom";
$RemapCount++;
$RemapName[$RemapCount] = "Cycle Zoom Level";
$RemapCmd[$RemapCount] = "setZoomFOV";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot One";
$RemapCmd[$RemapCount] = "useFirstWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot Two";
$RemapCmd[$RemapCount] = "useSecondWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot Three";
$RemapCmd[$RemapCount] = "useThirdWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot Four";
$RemapCmd[$RemapCount] = "useFourthWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot Five";
$RemapCmd[$RemapCount] = "useFifthWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Weapon Slot Six";
$RemapCmd[$RemapCount] = "useSixthWeaponSlot";
$RemapCount++;
$RemapName[$RemapCount] = "Blaster";
$RemapCmd[$RemapCount] = "useBlaster";
$RemapCount++;
$RemapName[$RemapCount] = "Plasma Rifle";
$RemapCmd[$RemapCount] = "usePlasma";
$RemapCount++;
$RemapName[$RemapCount] = "Chaingun";
$RemapCmd[$RemapCount] = "useChaingun";
$RemapCount++;
$RemapName[$RemapCount] = "Spinfusor";
$RemapCmd[$RemapCount] = "useDisc";
$RemapCount++;
$RemapName[$RemapCount] = "Grenade Launcher";
$RemapCmd[$RemapCount] = "useGrenadeLauncher";
$RemapCount++;
$RemapName[$RemapCount] = "Laser Rifle";
$RemapCmd[$RemapCount] = "useSniperRifle";
$RemapCount++;
$RemapName[$RemapCount] = "ELF Projector";
$RemapCmd[$RemapCount] = "useELFGun";
$RemapCount++;
$RemapName[$RemapCount] = "Fusion Mortar";
$RemapCmd[$RemapCount] = "useMortar";
$RemapCount++;
$RemapName[$RemapCount] = "Missile Launcher";
$RemapCmd[$RemapCount] = "useMissileLauncher";
$RemapCount++;
$RemapName[$RemapCount] = "Shocklance";
$RemapCmd[$RemapCount] = "useShockLance";
$RemapCount++;
$RemapName[$RemapCount] = "Targeting Laser";
$RemapCmd[$RemapCount] = "useTargetingLaser";
$RemapCount++;
$RemapName[$RemapCount] = "Previous Weapon";
$RemapCmd[$RemapCount] = "prevWeapon";
$RemapCount++;
$RemapName[$RemapCount] = "Next Weapon";
$RemapCmd[$RemapCount] = "nextWeapon";
$RemapCount++;
$RemapName[$RemapCount] = "Throw Grenade";
$RemapCmd[$RemapCount] = "throwGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Place Mine";
$RemapCmd[$RemapCount] = "placeMine";
$RemapCount++;
$RemapName[$RemapCount] = "Use Pack";
$RemapCmd[$RemapCount] = "useBackpack";
$RemapCount++;
$RemapName[$RemapCount] = "Use Health Kit";
$RemapCmd[$RemapCount] = "useRepairKit";
$RemapCount++;
$RemapName[$RemapCount] = "Deploy Beacon";
$RemapCmd[$RemapCount] = "placeBeacon";
$RemapCount++;
$RemapName[$RemapCount] = "Inventory";
$RemapCmd[$RemapCount] = "toggleInventoryHud";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 1";
$RemapCmd[$RemapCount] = "selectFavorite1";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 2";
$RemapCmd[$RemapCount] = "selectFavorite2";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 3";
$RemapCmd[$RemapCount] = "selectFavorite3";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 4";
$RemapCmd[$RemapCount] = "selectFavorite4";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 5";
$RemapCmd[$RemapCount] = "selectFavorite5";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 6";
$RemapCmd[$RemapCount] = "selectFavorite6";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 7";
$RemapCmd[$RemapCount] = "selectFavorite7";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 8";
$RemapCmd[$RemapCount] = "selectFavorite8";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 9";
$RemapCmd[$RemapCount] = "selectFavorite9";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 10";
$RemapCmd[$RemapCount] = "selectFavorite10";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 11";
$RemapCmd[$RemapCount] = "selectFavorite11";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 12";
$RemapCmd[$RemapCount] = "selectFavorite12";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 13";
$RemapCmd[$RemapCount] = "selectFavorite13";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 14";
$RemapCmd[$RemapCount] = "selectFavorite14";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 15";
$RemapCmd[$RemapCount] = "selectFavorite15";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 16";
$RemapCmd[$RemapCount] = "selectFavorite16";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 17";
$RemapCmd[$RemapCount] = "selectFavorite17";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 18";
$RemapCmd[$RemapCount] = "selectFavorite18";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 19";
$RemapCmd[$RemapCount] = "selectFavorite19";
$RemapCount++;
$RemapName[$RemapCount] = "Favorite 20";
$RemapCmd[$RemapCount] = "selectFavorite20";
$RemapCount++;
$RemapName[$RemapCount] = "Select Energy Pack";
$RemapCmd[$RemapCount] = "quickPackEnergyPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Repair Pack";
$RemapCmd[$RemapCount] = "quickPackRepairPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Shield Pack";
$RemapCmd[$RemapCount] = "quickPackShieldPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Cloaking Pack";
$RemapCmd[$RemapCount] = "quickPackCloakPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Sensor Jammer";
$RemapCmd[$RemapCount] = "quickPackJammerPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Ammo Pack";
$RemapCmd[$RemapCount] = "quickPackAmmoPack";
$RemapCount++;
$RemapName[$RemapCount] = "Select Satchel Charge";
$RemapCmd[$RemapCount] = "quickPackSatchelCharge";
$RemapCount++;
$RemapName[$RemapCount] = "Select Inv Station";
$RemapCmd[$RemapCount] = "quickPackDeployableStation";
$RemapCount++;
$RemapName[$RemapCount] = "Select Spider Turret";
$RemapCmd[$RemapCount] = "quickPackIndoorTurret";
$RemapCount++;
$RemapName[$RemapCount] = "Select Landspike Turret";
$RemapCmd[$RemapCount] = "quickPackOutdoorTurret";
$RemapCount++;
$RemapName[$RemapCount] = "Select Motion Sensor";
$RemapCmd[$RemapCount] = "quickPackMotionSensor";
$RemapCount++;
$RemapName[$RemapCount] = "Select Deploy Pulse";
$RemapCmd[$RemapCount] = "quickPackPulse";
$RemapCount++;
$RemapName[$RemapCount] = "Select Plasma Barrel";
$RemapCmd[$RemapCount] = "quickPackPlasmaBarrel";
$RemapCount++;
$RemapName[$RemapCount] = "Select Missile Barrel";
$RemapCmd[$RemapCount] = "quickPackMissileBarrel";
$RemapCount++;
$RemapName[$RemapCount] = "Select AA Barrel";
$RemapCmd[$RemapCount] = "quickPackAABarrel";
$RemapCount++;
$RemapName[$RemapCount] = "Select Mortar Barrel";
$RemapCmd[$RemapCount] = "quickPackMortarBarrel";
$RemapCount++;
$RemapName[$RemapCount] = "Select Elf Barrel";
$RemapCmd[$RemapCount] = "quickPackElfBarrel";
$RemapCount++;
$RemapName[$RemapCount] = "Select Grenade";
$RemapCmd[$RemapCount] = "quickPackGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Select Flash Grenade";
$RemapCmd[$RemapCount] = "quickPackFlashGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Select Concussion";
$RemapCmd[$RemapCount] = "quickPackConcussionGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Select Camera";
$RemapCmd[$RemapCount] = "quickPackCameraGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Select Flare Grenade";
$RemapCmd[$RemapCount] = "quickPackFlareGrenade";
$RemapCount++;
$RemapName[$RemapCount] = "Command Circuit";
$RemapCmd[$RemapCount] = "toggleCommanderMap";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Task List";
$RemapCmd[$RemapCount] = "toggleTaskListDlg";
$RemapCount++;
$RemapName[$RemapCount] = "Accept Task";
$RemapCmd[$RemapCount] = "fnAcceptTask";
$RemapCount++;
$RemapName[$RemapCount] = "Decline Task";
$RemapCmd[$RemapCount] = "fnDeclineTask";
$RemapCount++;
$RemapName[$RemapCount] = "Task Completed";
$RemapCmd[$RemapCount] = "fnTaskCompleted";
$RemapCount++;
$RemapName[$RemapCount] = "Reset Task List";
$RemapCmd[$RemapCount] = "fnResetTaskList";
$RemapCount++;
$RemapName[$RemapCount] = "Vote Yes";
$RemapCmd[$RemapCount] = "voteYes";
$RemapCount++;
$RemapName[$RemapCount] = "Vote No";
$RemapCmd[$RemapCount] = "voteNo";
$RemapCount++;
$RemapName[$RemapCount] = "Voice Chat Menu";
$RemapCmd[$RemapCount] = "activateChatMenuHud";
$RemapCount++;
$RemapName[$RemapCount] = "Global Chat";
$RemapCmd[$RemapCount] = "ToggleMessageHud";
$RemapCount++;
$RemapName[$RemapCount] = "Team Chat";
$RemapCmd[$RemapCount] = "TeamMessageHud";
$RemapCount++;
$RemapName[$RemapCount] = "Resize Chat Hud";
$RemapCmd[$RemapCount] = "resizeChatHud";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Microphone";
$RemapCmd[$RemapCount] = "voiceCapture";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Help Text";
$RemapCmd[$RemapCount] = "toggleHelpGui";
$RemapCount++;
$RemapName[$RemapCount] = "Score Screen";
$RemapCmd[$RemapCount] = "toggleScoreScreen";
$RemapCount++;
$RemapName[$RemapCount] = "Free Look";
$RemapCmd[$RemapCount] = "toggleFreeLook";
$RemapCount++;
$RemapName[$RemapCount] = "Exterior View";
$RemapCmd[$RemapCount] = "toggleFirstPerson";
$RemapCount++;
$RemapName[$RemapCount] = "Drop Weapon";
$RemapCmd[$RemapCount] = "throwWeapon";
$RemapCount++;
$RemapName[$RemapCount] = "Drop Pack";
$RemapCmd[$RemapCount] = "throwPack";
$RemapCount++;
$RemapName[$RemapCount] = "Drop Flag";
$RemapCmd[$RemapCount] = "throwFlag";
$RemapCount++;
$RemapName[$RemapCount] = "Suicide";
$RemapCmd[$RemapCount] = "suicide";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Personal Wypts";
$RemapCmd[$RemapCount] = "toggleHudWaypoints";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Mission Wypts";
$RemapCmd[$RemapCount] = "toggleHudMarkers";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Beacons";
$RemapCmd[$RemapCount] = "toggleHudTargets";
$RemapCount++;
$RemapName[$RemapCount] = "Toggle Commands";
$RemapCmd[$RemapCount] = "toggleHudCommands";
$RemapCount++;
// $RemapName[$RemapCount] = "Start Demo Record";
// $RemapCmd[$RemapCount] = "startRecordingDemo";
// $RemapCount++;
// $RemapName[$RemapCount] = "Stop Demo Record";
// $RemapCmd[$RemapCount] = "stopRecordingDemo";
// $RemapCount++;
$RemapName[$RemapCount] = "Chat Page Up";
$RemapCmd[$RemapCount] = "pageMessageHudUp";
$RemapCount++;
$RemapName[$RemapCount] = "Chat Page Down";
$RemapCmd[$RemapCount] = "pageMessageHudDown";
$RemapCount++;

//------------------------------------------------------------------------------
function restoreDefaultMappings()
{
   moveMap.delete();
   exec( "scripts/controlDefaults.cs" );
   $pref::Input::ActiveConfig = "MyConfig";
   OP_RemapList.fillList();
}

//------------------------------------------------------------------------------
function isMapFile( %file )
{
   %fObject = new FileObject();
   if ( !%fObject.openForRead( %file ) )
      return( false );

   while ( !%fObject.isEOF() )
   {
      %line = %fObject.readLine();
      if ( %line $= "// Tribes 2 Input Map File" )
      {
         %fObject.close();
         return( true );
      }         
   }

   %fObject.close();
   return( false );   
}

//------------------------------------------------------------------------------
function isValidMapFileSaveName( %file )
{
   if ( !isWriteableFileName( "base/" @ %file ) )
      return( false );

   if ( isFile( %file ) )
      return( isMapFile( %file ) );

   return( true );
}

//------------------------------------------------------------------------------
function loadMapFile( %filename )
{
   exec( "prefs/" @ %filename @ ".cs" );
   $pref::Input::ActiveConfig = %filename;
   OP_RemapList.fillList();
}

//------------------------------------------------------------------------------
function saveActiveMapFile()
{
   if ( isValidMapFileSaveName( "prefs/" @ $pref::Input::ActiveConfig @ ".cs" ) )
      saveMapFile( $pref::Input::ActiveConfig );
   else
      ShellGetSaveFilename( "SAVE CONTROL CONFIG", "prefs/*.cs", "isMapFile", "saveMapFile", "" );
}

//------------------------------------------------------------------------------
function saveMapFile( %filename )
{
   %mapFile = "prefs/" @ %filename @ ".cs";
   if ( !isWriteableFileName( "base/" @ %mapFile ) )
   {
      MessageBoxOK( "SAVE FAILED", "That is not a writeable file name.  Please choose another file name.",
            "ShellGetSaveFilename( \"SAVE CONTROL CONFIG\", \"prefs/*.cs\", \"isMapFile\", \"saveMapFile\", $pref::Input::ActiveConfig );" );
      return;
   }
   
   if ( isFile( %mapFile ) && !isMapFile( %mapFile ) )
   {
      MessageBoxOK( "SAVE FAILED", "A file of that name already exists and is not an input configuration file.  Please choose another file name.",
            "ShellGetSaveFilename( \"SAVE CONTROL CONFIG\", \"prefs/*.cs\", \"isMapFile\", \"saveMapFile\", $pref::Input::ActiveConfig );" );
      return;
   }

   moveMap.save( %mapFile );

   // Write out the console toggle key:
   %fObject = new FileObject();
   if ( %fObject.openForAppend( %mapFile ) )
   {
      %bind = GlobalActionMap.getBinding( "toggleConsole" );
      %fObject.writeLine( "GlobalActionMap.bind(keyboard, \"" @ getField( %bind, 1 ) @ "\", toggleConsole);" );
      %fObject.close();
   }
   %fObject.delete();

   $pref::Input::ActiveConfig = %filename;
}

//------------------------------------------------------------------------------
function getMapDisplayName( %device, %action )
{
	if ( %device $= "keyboard" )
		return( %action );		
	else if ( strstr( %device, "mouse" ) != -1 )
	{
		// Substitute "mouse" for "button" in the action string:
		%pos = strstr( %action, "button" );
		if ( %pos != -1 )
		{
			%mods = getSubStr( %action, 0, %pos );
			%object = getSubStr( %action, %pos, 1000 );
			%instance = getSubStr( %object, strlen( "button" ), 1000 );
			return( %mods @ "mouse" @ ( %instance + 1 ) );
		}
		else
			error( "Mouse input object other than button passed to getDisplayMapName!" );
	}
	else if ( strstr( %device, "joystick" ) != -1 )
	{
		// TODO - add POV support here...
		// Substitute "joy" for "button" in the action string:
		%pos = strstr( %action, "button" );
		if ( %pos != -1 )
		{
			%mods = getSubStr( %action, 0, %pos );
			%object = getSubStr( %action, %pos, 1000 );
			%instance = getSubStr( %object, strlen( "button" ), 1000 );
			return( %mods @ "joy" @ ( %instance + 1 ) );
		}
		else
	   { 
	      %pos = strstr( %action, "pov" );
         if ( %pos != -1 )
         {
            %mods = getSubStr( %action, 0, %pos );
            %object = getSubStr( %action, %pos, 1000 );
            MessageBoxOK( "WARNING", "Sorry, Joystick POVs are not yet supported." );
            return( "" );
         }
         else
         {
            error( "Unsupported Joystick input object passed to getDisplayMapName!" );
            MessageBoxOK( "ERROR", "That type of input object is not supported." );
            return( "" );
         }
      }
	}
		
	return( "??" );		
}

//------------------------------------------------------------------------------
function buildFullMapString( %index )
{
	%temp = moveMap.getBinding( $RemapCmd[%index] );
   %device = getField( %temp, 0 );
   %object = getField( %temp, 1 );
   if ( %device !$= "" && %object !$= "" )
	   %mapString = getMapDisplayName( %device, %object );
   else
      %mapString = "";

	return( $RemapName[%index] @ "\t" @ %mapString );
}

//------------------------------------------------------------------------------
function OP_RemapList::fillList( %this )
{
	%this.clear();
	for ( %i = 0; %i < $RemapCount; %i++ )
		%this.addRow( %i, buildFullMapString( %i ) );

   // Set the console key:
	%bind = GlobalActionMap.getBinding( "toggleConsole" );
   OP_ConsoleKeyBtn.setValue( getField( %bind, 1 ) );
}

//------------------------------------------------------------------------------
function OP_RemapList::onDeleteKey( %this, %rowId )
{
   clearMapping( %rowId );
   %this.setRowById( %rowId, buildFullMapString( %rowId ) );
}

//------------------------------------------------------------------------------
function OP_RemapList::doRemap( %this )
{
	%selId = %this.getSelectedId();
	RemapFrame.setTitle( "REMAP \"" @ $RemapName[%selId] @ "\"" );
   RemapInputCtrl.mode = "move";
	RemapInputCtrl.index = %selId;
	Canvas.pushDialog( RemapDlg );
}

//------------------------------------------------------------------------------
function OP_ConsoleKeyBtn::doRemap( %this )
{
	RemapFrame.setTitle( "Remap \"Toggle Console\"" );
   RemapInputCtrl.mode = "consoleKey";
	RemapInputCtrl.index = 0;
	Canvas.pushDialog( RemapDlg );
}

//------------------------------------------------------------------------------
function RemapDlg::onWake( %this )
{
   if ( RemapInputCtrl.mode $= "consoleKey" )
	   RemapText.setText( "<just:center>Press a key to assign it to this action" NL "or Esc to cancel..." );
   else
	   RemapText.setText( "<just:center>Press a key or button to assign it to this action" NL "or Esc to cancel..." );
   //CursorOff();
}

//------------------------------------------------------------------------------
function RemapDlg::onSleep( %this )
{
   //CursorOn();
}

//------------------------------------------------------------------------------
function findRemapCmdIndex( %command )
{
	for ( %i = 0; %i < $RemapCount; %i++ )
	{
		if ( %command $= $RemapCmd[%i] )
			return( %i );			
	}

	return( -1 );	
}

//------------------------------------------------------------------------------
function clearMapping( %index )
{
	%fullMapString = moveMap.getBinding( $RemapCmd[%index] );
	%mapCount = getRecordCount( %fullMapString );
	for ( %i = 0; %i < %mapCount; %i++ )
	{
		%temp = getRecord( %fullMapString, %i );
		moveMap.unbind( getField( %temp, 0 ), getField( %temp, 1 ) );
	}
}

//------------------------------------------------------------------------------
function redoMapping( %device, %action, %oldIndex, %newIndex )
{
	moveMap.bind( %device, %action, $RemapCmd[%newIndex] );
	OP_RemapList.setRowById( %oldIndex, buildFullMapString( %oldIndex ) );
	OP_RemapList.setRowById( %newIndex, buildFullMapString( %newIndex ) );
}

//------------------------------------------------------------------------------
function redoConsoleMapping( %action, %oldIndex )
{
   moveMap.unbind( "keyboard", %action );
   GlobalActionMap.bind( "keyboard", %action, "toggleConsole" );
   OP_ConsoleKeyBtn.setValue( %action );
	OP_RemapList.setRowById( %oldIndex, buildFullMapString( %oldIndex ) );
}

//------------------------------------------------------------------------------
function RemapInputCtrl::onInputEvent( %this, %device, %action )
{
	//error( "** onInputEvent called - device = " @ %device @ ", action = " @ %action @ " **" );
	Canvas.popDialog( RemapDlg );

	// Tested for the reserved keystrokes:
	if ( %device $= "keyboard" )
	{
      // Cancel...
      if ( %action $= "escape" )
      {
         // Do nothing...
		   return;
      }
	}
	
   if ( %this.mode $= "move" )
   {
	   // First check to see if the given action is already mapped:
	   %prevMap = moveMap.getCommand( %device, %action );
      if ( %prevMap !$= $RemapCmd[%this.index] )
      {
	      if ( %prevMap $= "" )
	      {
		      moveMap.bind( %device, %action, $RemapCmd[%this.index] );
		      OP_RemapList.setRowById( %this.index, buildFullMapString( %this.index ) );
	      }
	      else
	      {
            %mapName = getMapDisplayName( %device, %action );
		      %prevMapIndex = findRemapCmdIndex( %prevMap );
		      if ( %prevMapIndex == -1 )
			      MessageBoxOK( "REMAP FAILED", "\"" @ %mapName @ "\" is already bound to a non-remappable command!" );
		      else
			      MessageBoxYesNo( "WARNING", 
				      "\"" @ %mapName @ "\" is already bound to \"" 
					      @ $RemapName[%prevMapIndex] @ "\"!\nDo you want to undo this mapping?", 
				      "redoMapping(" @ %device @ ", \"" @ %action @ "\", " @ %prevMapIndex @ ", " @ %this.index @ ");", "" );
		      return;
	      }
      }
   }
   else if ( %this.mode $= "consoleKey" )
   {
      if ( %device !$= "keyboard" )
      {
         MessageBoxOK( "REMAP FAILED", "This command can only be bound to keys on the keyboard!" );
         return;
      }

      %prevMap = GlobalActionMap.getCommand( %device, %action );
      if ( %prevMap !$= "" )
      {
         MessageBoxOK( "REMAP FAILED", "\"" @ getMapDisplayName( %device, %action ) @ "\" is already bound to a non-remappable command!" ); 
         return;
      }

      %mvMap = moveMap.getCommand( %device, %action );
      if ( %mvMap $= "" )
      {
         GlobalActionMap.bind( %device, %action, "toggleConsole" );
         OP_ConsoleKeyBtn.setValue( %action );
      }
      else
      {
         %mapName = getMapDisplayName( %device, %action );
         %mvMapIndex = findRemapCmdIndex( %mvMap );
         if ( %mvMapIndex == -1 )
            MessageBoxOK( "REMAP FAILED", "\"" @ %mapName @ "\" is already bound to a non-remappable command!" );
         else
            MessageBoxYesNo( "WARNING", "\"" @ %mapName @ "\" is already bound to \""
                  @ $RemapName[%mvMapIndex] @ "\"!"
                  NL "Do you want to undo this mapping?",
                  "redoConsoleMapping(\"" @ %action @ "\", " @ %mvMapIndex @ ");", "" ); 
         return;
      }
   }
   else
      error( "The sky is falling, the sky is falling!" );
}

//------------------------------------------------------------------------------
function OP_JoystickTgl::onAction( %this )
{
   OP_ConfigureJoystickBtn.setActive( %this.getValue() );
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
$RemapAxisCount = 0;
$RemapAxisName[$RemapAxisCount] = "Look Up/Down";
$RemapAxisCmd[$RemapAxisCount] = "pitch";
$RemapAxisCount++;
$RemapAxisName[$RemapAxisCount] = "Turn Left/Right";
$RemapAxisCmd[$RemapAxisCount] = "yaw";
$RemapAxisCount++;
$RemapAxisName[$RemapAxisCount] = "Cycle Weapon";
$RemapAxisCmd[$RemapAxisCount] = "cycleWeaponAxis";
$RemapAxisCount++;
$RemapAxisName[$RemapAxisCount] = "Move Forward/Backward";
$RemapAxisCmd[$RemapAxisCount] = "joystickMoveY";
$RemapAxisCount++;
$RemapAxisName[$RemapAxisCount] = "Move Left/Right";
$RemapAxisCmd[$RemapAxisCount] = "joystickMoveX";
$RemapAxisCount++;

//------------------------------------------------------------------------------
function MouseConfigDlg::onWake( %this )
{
	MouseXSlider.setValue( moveMap.getScale( mouse, xaxis ) / 2 );
	MouseYSlider.setValue( moveMap.getScale( mouse, yaxis ) / 2 );
	InvertMouseTgl.setValue( moveMap.isInverted( mouse, yaxis ) );

   MouseZActionMenu.clear();
   MouseZActionMenu.add( "Nothing", 1 );
   MouseZActionMenu.add( "Cycle Weapon", 2 );
   MouseZActionMenu.add( "Next Weapon Only", 3 );

   %bind = moveMap.getCommand( "mouse", "zaxis" );
   %selId = 1;
   switch$ ( %bind )
   {
      case "cycleWeaponAxis":
         %selId = 2;
      case "cycleNextWeaponOnly":
         %selId = 3;
   }
   MouseZActionMenu.setSelected( %selId );
}

//------------------------------------------------------------------------------
function MouseConfigDlg::onOK( %this )
{
 	%xSens = MouseXSlider.getValue() * 2;
 	%ySens = MouseYSlider.getValue() * 2;
 	moveMap.bind( mouse, xaxis, "S", %xSens, "yaw" );
   %yFlags = InvertMouseTgl.getValue() ? "SI" : "S";
 	moveMap.bind( mouse, yaxis, %yFlags, %ySens, "pitch" );

   switch ( MouseZActionMenu.getSelected() )
   {
      case 2:
         moveMap.bind( mouse, zaxis, cycleWeaponAxis );
      case 3:
         moveMap.bind( mouse, zaxis, cycleNextWeaponOnly );
      default:
         moveMap.unbind( mouse, zaxis );
   }

   Canvas.popDialog( MouseConfigDlg );
}

//------------------------------------------------------------------------------
function MouseXSlider::sync( %this )
{
   %thisValue = %this.getValue();
   MouseXText.setValue( "(" @ getSubStr( %thisValue, 0, 4 ) @ ")" );
   if ( $pref::Input::LinkMouseSensitivity )
   {
      if ( MouseYSlider.getValue() != %thisValue )
         MouseYSlider.setValue( %thisValue );
   }
}

//------------------------------------------------------------------------------
function MouseYSlider::sync( %this )
{
   %thisValue = %this.getValue();
   MouseYText.setValue( "(" @ getSubStr( %thisValue, 0, 4 ) @ ")" );
   if ( $pref::Input::LinkMouseSensitivity )
   {
      if ( MouseXSlider.getValue() != %thisValue )
         MouseXSlider.setValue( %thisValue );
   }
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
function JoystickConfigDlg::onWake( %this )
{
   // Add all of the axis tabs:
   %temp = getJoystickAxes( 0 );
   $joyAxisCount = getField( %temp , 0 );

   for ( %i = 0; %i < $joyAxisCount; %i++ )
   {
      %type = getField( %temp, %i + 1 );
      $JoyAxisTab[%i] = new ShellTabButton() {
			profile = "ShellTabProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "29" SPC ( 52 + ( %i * 30 ) );
			extent = "100 38";
			minExtent = "48 38";
			visible = "1";
			command = "JoystickConfigDlg.setPane(" @ %i @ ");";
			helpTag = "0";
			text = %type SPC "Axis";
      };

      switch$ ( %type )
      {
         case "X":   $JoyAxisTab[%i].type = "xaxis";
         case "Y":   $JoyAxisTab[%i].type = "yaxis";
         case "Z":   $JoyAxisTab[%i].type = "zaxis";
         case "R":   $JoyAxisTab[%i].type = "rxaxis";
         case "U":   $JoyAxisTab[%i].type = "ryaxis";
         case "V":   $JoyAxisTab[%i].type = "rzaxis";
      }

      JoystickConfigFrame.add( $JoyAxisTab[%i] );
   }

   // Fill the action menu:
   JoyAxisActionMenu.clear();
   for ( %i = 0; %i < $RemapAxisCount; %i++ )
      JoyAxisActionMenu.add( $RemapAxisName[%i], %i );
   JoyAxisActionMenu.add( "Nothing", 255 );

   // Select the first axis:
   %this.setPane( %this.pane );
}

//------------------------------------------------------------------------------
function JoystickConfigDlg::onSleep( %this )
{
   // Save the current pane's settings:
   bindJoystickAxis( %this.pane, JoyAxisActionMenu.getSelected() );
   for ( %i = 0; %i < $joyAxisCount; %i++ )
   {
      JoystickConfigFrame.remove( $JoyAxisTab[%i] );
      $JoyAxisTab[%i].delete();
   }
}

//------------------------------------------------------------------------------
function JoystickConfigDlg::setPane( %this, %pane )
{
   if ( %this.pane != %pane )
   {
      // Save the previous axes' settings:
      bindJoystickAxis( %this.pane, JoyAxisActionMenu.getSelected() );
      %this.pane = %pane;
   }

   for ( %i = 0; %i < $joyAxisCount; %i++ )
      $JoyAxisTab[%i].setValue( %i == %pane );

   // Update the config controls:
   %axisType =  $JoyAxisTab[%pane].type;
   %bind = moveMap.getCommand( "joystick", %axisType );
   if ( %bind !$= "" )
   {
      for ( %i = 0; %i < $RemapAxisCount; %i++ )
      {
         if ( $RemapAxisCmd[%i] $= %bind )
         {
            JoyAxisActionMenu.setSelected( %i );
            JoyAxisActionMenu.setText( $RemapAxisName[%i] );
            JoyAxisActionMenu.onSelect( %i, "" );
            break;
         }
      }

      if ( %i == $RemapAxisCount )
      {
         JoyAxisActionMenu.setSelected( 255 );  // 255 is the code for "Nothing"
         JoyAxisActionMenu.onSelect( 255, "" );
      }

      %scale = moveMap.getScale( "joystick", %axisType );
      JoyAxisSlider.setValue( %scale / 100 );
      %deadZone = moveMap.getDeadZone( "joystick", %axisType );
      if ( %deadZone $= "0 0" )
         DeadZoneSlider.setValue( 0.0 );
      else
         DeadZoneSlider.setValue( abs( firstWord( %deadZone ) ) / %scale );
      InvertJoyAxisTgl.setValue( moveMap.isInverted( "joystick", %axisType ) ); 
   }
   else
   {
      JoyAxisActionMenu.setSelected( 255 );  // 255 is the code for "Nothing"
      JoyAxisActionMenu.onSelect( 255, "" );
      JoyAxisSlider.setValue( 0.5 );
      DeadZoneSlider.setValue( 0.0 );
      InvertJoyAxisTgl.setValue( false );
   }
}

//------------------------------------------------------------------------------
function JoyAxisActionMenu::onSelect( %this, %id, %text )
{
   if ( %id >= $RemapAxisCount )
   {
      JoyAxisSlider.setActive( false );
      DeadZoneSlider.setActive( false );
      InvertJoyAxisTgl.setActive( false );
   }
   else
   {
      JoyAxisSlider.setActive( true );
      DeadZoneSlider.setActive( true );
      InvertJoyAxisTgl.setActive( true );
   }
}

//------------------------------------------------------------------------------
function JoySensText::update()
{
   %this.setValue( "(" @ getSubStr( JoyAxisSlider.getValue(), 0, 4 ) @ ")" );
}

//------------------------------------------------------------------------------
function DeadZoneText::update()
{
   %val = DeadZoneSlider.getValue();
   %percent = %val * 100;
   %temp = strstr( %percent, "." );
   if ( %temp != -1 )
      %percent = getSubStr( %percent, 0, %temp );

   %this.setValue( "(" @ %percent @ "%)" );
}

//------------------------------------------------------------------------------
function bindJoystickAxis( %axisIndex, %cmdIndex )
{
   %axis = $JoyAxisTab[%axisIndex].type;
   if ( %cmdIndex > $RemapAxisCount )
   {
      // Make sure the axis is unbound:
      moveMap.unbind( "joystick", %axis );
      return;
   }

   %sens = JoyAxisSlider.getValue() * 100;
 	%delta = DeadZoneSlider.getValue() * %sens;
 	if ( %delta > 0 )
   {
      %deadZone = "-" @ %delta SPC %delta;
      if ( InvertJoyAxisTgl.getValue() )
 		   moveMap.bind( "joystick", %axis, "SDI", %deadZone, %sens, $RemapAxisCmd[%cmdIndex] );
      else
 		   moveMap.bind( "joystick", %axis, "SD", %deadZone, %sens, $RemapAxisCmd[%cmdIndex] );
   }
 	else
   {
      if ( InvertJoyAxisTgl.getValue() )
 		   moveMap.bind( "joystick", %axis, "SI", %sens, $RemapAxisCmd[%cmdIndex] );
      else
 		   moveMap.bind( "joystick", %axis, "S", %sens, $RemapAxisCmd[%cmdIndex] );
   }
}

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// Network Settings:
//
function OP_MasterServerMenu::init( %this )
{
   %this.clear();
   // You can change these strings, but NOT THE IDS!
   %this.add( "Always", 1 );
   %this.add( "When Not Full", 2 );
   %this.add( "Never", 3 );
}

//------------------------------------------------------------------------------
function OP_MasterServerMenu::onSelect( %this, %id, %text )
{
   switch( %id )
   {
      case 2:
         $pref::Net::DisplayOnMaster = "NotFull";
      case 3:
         $pref::Net::DisplayOnMaster = "Never";
      default:
         $pref::Net::DisplayOnMaster = "Always";
   }
}

//------------------------------------------------------------------------------
function OP_RegionMenu::init( %this )
{
   %this.clear();
   %this.add( "North America East", 1 );
   %this.add( "North America West", 2 );
   %this.add( "South America", 4 );
   %this.add( "Australia", 8 );
   %this.add( "Asia", 16 );
   %this.add( "Europe", 32 );
}

//------------------------------------------------------------------------------
function OP_RegionMenu::onSelect( %this, %id, %text )
{
   $pref::Net::RegionMask = %id;
}

//------------------------------------------------------------------------------
function OP_LaunchScreenMenu::init( %this )
{
   %this.clear();
   %this.add( "Game", 1 );
   %this.add( "News", 2 );
   %this.add( "Forums", 3 );
   %this.add( "Email", 4 );
   %this.add( "Chat", 5 );
   %this.add( "Browser", 6 );
}

//------------------------------------------------------------------------------
function toggleImmersion()
{
   if( isImmersionEnabled() )
      enableImmersion( $Pref::useImmersion = false );
   else
      enableImmersion( $Pref::useImmersion = true );   
}

