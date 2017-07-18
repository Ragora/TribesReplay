//----------------------------------------------------------------------------

//----------------------------------------------------------------------------

function SAD(%password)
{
   if(%password !$= "")
      commandToServer('SAD', %password);
}

function SADSetPassword(%password)
{
   commandToServer('SADSetPassword', %password);
}

function use(%data)
{
   // %data is currently the datablock name of the item
   commandToServer('use',%data);
}

function throw(%data)
{
   // %data is currently the datablock name of the item
   commandToServer('throw',%data);
}

function giveAll()
{
   commandToServer('giveAll');
}

function clientCmdSetPlayContent()
{
   if ( $LaunchMode $= "InteriorView" )
      Canvas.setContent( interiorPreviewGui );
   else
      Canvas.setContent( Playgui );
} 

function clientCmdPickTeamMenu( %teamA, %teamB )
{
   ClientCmdSetHudMode("PickTeam");
   PickTeamAButton.text = %teamA;
   PickTeamBButton.text = %teamB;
   PickTeamFrame.setTitle( "Pick Team" );
   
   Canvas.pushDialog( PickTeamDlg );
}

function clientCmdProcessPickTeam( %option )
{
   if( %option <= 4 )
      CommandToServer( 'clientPickedTeam', %option );
   else if( %option == 5 )     
      disconnect();
   
   Canvas.popDialog( PickTeamDlg );
}

new MessageVector(HudMessageVector);

$LastHudTarget = 0;

function addMessageHudLine(%text)
{
   //first, see if we're "scrolled up"...
   %textHeight = chatHud.profile.fontSize;
   if (%textHeight <= 0)
      %textHeight = 12;
   %chatScrollHeight = getWord(chatHud.getGroup().getGroup().extent, 1);
   %chatPosition = getWord(chatHud.extent, 1) - %chatScrollHeight + getWord(chatHud.position, 1);
   %linesToScroll = mFloor((%chatPosition / %textHeight) + 0.5);
   if (%linesToScroll > 0)
      %origPosition = chatHud.position;
      
   //add the message...
   while(HudMessageVector.getNumLines() && (HudMessageVector.getNumLines() >= $pref::HudMessageLogSize))
   {
      %tag = HudMessageVector.getLineTag(0);
      if(%tag != 0)
         %tag.delete();
      HudMessageVector.popFrontLine();
   }
   HudMessageVector.pushBackLine(%text, $LastHudTarget);
   $LastHudTarget = 0;

   //now that we've added the message, see if we need to reset the position
   if (%linesToScroll > 0)
   {
      chatPageDown.setVisible(true);
      ChatPageDown.position = ( firstWord( outerChatHud.extent ) - 20 ) @ " " @ ( $chatScrollLenY[$chatHudLength] - 6 );
      chatHud.position = %origPosition;
   }
   else
      chatPageDown.setVisible(false);
      
}

function pageUpMessageHud()
{
   //find out the text line height
   %textHeight = chatHud.profile.fontSize;
   if (%textHeight <= 0)
      %textHeight = 12;

   //find out how many lines per page are visible
   %chatScrollHeight = getWord(chatHud.getGroup().getGroup().extent, 1);
   if (%chatScrollHeight <= 0)
      return;

   %pageLines = mFloor(%chatScrollHeight / %textHeight) - 1;
   if (%pageLines <= 0)
      %pageLines = 1;

   //see how many lines we actually can scroll up:
   %chatPosition = -1 * getWord(chatHud.position, 1);
   %linesToScroll = mFloor((%chatPosition / %textHeight) + 0.5);
   if (%linesToScroll <= 0)
      return;

   if (%linesToScroll > %pageLines)
      %scrollLines = %pageLines;
   else
      %scrollLines = %linesToScroll;

   //now set the position
   chatHud.position = firstWord(chatHud.position) SPC (getWord(chatHud.position, 1) + (%scrollLines * %textHeight));

   //display the pageup icon
   ChatPageDown.position = ( firstWord( outerChatHud.extent ) - 20 ) @ " " @ ( $chatScrollLenY[$pref::chatHudLength] - 6 );
   chatPageDown.setVisible(true);
}

function pageDownMessageHud()
{
   //find out the text line height
   %textHeight = chatHud.profile.fontSize;
   if (%textHeight <= 0)
      %textHeight = 12;

   //find out how many lines per page are visible
   %chatScrollHeight = getWord(chatHud.getGroup().getGroup().extent, 1);
   if (%chatScrollHeight <= 0)
      return;

   %pageLines = mFloor(%chatScrollHeight / %textHeight) - 1;
   if (%pageLines <= 0)
      %pageLines = 1;

   //see how many lines we actually can scroll down:
   %chatPosition = getWord(chatHud.extent, 1) - %chatScrollHeight + getWord(chatHud.position, 1);
   %linesToScroll = mFloor((%chatPosition / %textHeight) + 0.5);
   if (%linesToScroll <= 0)
      return;

   if (%linesToScroll > %pageLines)
      %scrollLines = %pageLines;
   else
      %scrollLines = %linesToScroll;

   //now set the position
   chatHud.position = firstWord(chatHud.position) SPC (getWord(chatHud.position, 1) - (%scrollLines * %textHeight));

   //see if we have should (still) display the pagedown icon
   if (%scrollLines < %linesToScroll)
   {  
      chatPageDown.setVisible(true);
      ChatPageDown.position = ( firstWord( outerChatHud.extent ) - 20 ) @ " " @ ( $chatScrollLenY[$Pref::chatHudLength] - 6 );
   }
   else
      chatPageDown.setVisible(false);
}

$cursorControlled = true;

function CursorOff()
{
   if ( $cursorControlled )
      lockMouse(true);
   Canvas.cursorOff();
}

function CursorOn()
{
   if ( $cursorControlled )
      lockMouse(false);
   Canvas.cursorOn();
   Canvas.setCursor(DefaultCursor); 
}

function toggleCursorControl()
{
   // If the user manually toggles the mouse control, lock or unlock for them
   if ( $cursorControlled )
      $cursorControlled = false;
   else
      $cursorControlled = true;
   lockMouse($cursorControlled);
}

if ( $platform $= "linux" )
   GlobalActionMap.bindCmd(keyboard, "ctrl g", "", "toggleCursorControl();");

function PlayGui::onWake(%this)
{
   // Make sure the shell hum is off:
   if ( $HudHandle['shellScreen'] !$= "" )
   {
      alxStop( $HudHandle['shellScreen'] );
      $HudHandle['shellScreen'] = "";
   }

   $enableDirectInput = "1";
   activateDirectInput();
   
   // chat hud dialog
   Canvas.pushDialog( MainChatHud );
   chatHud.attach(HudMessageVector);

   // just update the action map here, the huds should be properly setup
   updateActionMaps();
   
   // hack city - these controls are floating around and need to be clamped
   schedule(0, 0, "refreshCenterTextCtrl");
   schedule(0, 0, "refreshBottomTextCtrl");
   
   CursorOff();
}

function refreshBottomTextCtrl()
{
   BottomPrintText.position = "0 0";
}

function refreshCenterTextCtrl()
{
   CenterPrintText.position = "0 0";
}

function PlayGui::onSleep(%this)
{
   Canvas.popDialog( MainChatHud  );
   
   //pop all possible keymaps
   moveMap.pop();
   if ( isObject( passengerKeys ) )
      passengerKeys.pop();
   if ( isObject( observerBlockMap ) )
      observerBlockMap.pop();
   if ( isObject( observerMap ) )
      observerMap.pop();
   //flyingCameraMove.pop();

   CursorOn();
}

function onConnectRequestRejected( %msg )
{
   switch$(%msg)
   {
      case "CR_INVALID_CONNECT_PACKET":
         %error = "Network error - badly formed network packet - this should not happen!";
      case "CR_AUTHENTICATION_FAILED":
         %error = "Failed to authenticate with server.  Please restart TRIBES 2 and try again.";
      case "CR_YOUAREBANNED":
         %error = "You are not allowed to play on this server.";
      case "CR_SERVERFULL":
         %error = "This server is full.";
      default:
         %error = "Connection error.  Please try another server.  Error code: (" @ %msg @ ")";
   }
   DisconnectedCleanup();
   MessageBoxOK( "REJECTED", %error);
}

function onChallengeRequestRejected( %msg )
{
   CloseMessagePopup();
   DisconnectedCleanup();
   switch$(%msg)
   {
      case "PASSWORD":
         if ( $JoinGamePassword $= "" )
            Canvas.pushDialog( PasswordDlg );
         else
         {
            $JoinGamePassword = "";
            MessageBoxOK( "REJECTED", "That password is incorrect.");
         }
         return;
      case "CHR_PROTOCOL":
         %error = "Incompatible protocol version: You must upgrade your game version to play on this server.";
      case "CHR_NOT_AUTHENTICATED":
         %error = "This is an online server - you must be logged in to play on it.";
      case "CHR_INVALID_SERVER_PACKET":
         %error = "Invalid server response packet.  This should not happen.";
      case "WS_PeerAuthServer_ExpiredClientCertificate":
         %error = "Authentication error - please restart TRIBES 2 and try again.";
      default:
         %error = "Connection challenge error.  Please try another server.  Error code: (" @ %msg @ ")";
   }
   MessageBoxOK( "REJECTED", %error );
}

function onConnectRequestTimedOut()
{
   DisconnectedCleanup();
   MessageBoxOK( "TIMED OUT", "Your connection to the server timed out." );
}

function onConnectionToServerTimedOut()
{
   DisconnectedCleanup();
   MessageBoxOK( "TIMED OUT", "Your connection to the server timed out.");
}

function onConnectionToServerLost( %msg )
{
   DisconnectedCleanup();
   MessageBoxOK( "DISCONNECT", "Your connection to the server was lost." NL %msg);
}

// Client voting functions:
function startNewVote(%name, %actionMsg, %arg1, %arg2, %arg3, %arg4, %playerVote)
{
   if ( %arg1 $= "" )
      %arg1 = 0;
   if ( %arg2 $= "" )
      %arg2 = 0;
   if ( %arg3 $= "" )
      %arg3 = 0;
   if ( %arg4 $= "" )
      %arg4 = 0;
   if ( %playerVote $= "" )
      %playerVote = 0;

   commandToServer('startNewVote', %name, %actionMsg, %arg1, %arg2, %arg3, %arg4, %playerVote);   
}

function setPlayerVote(%vote)
{
   commandToServer('setPlayerVote', %vote);
}

function ClientCmdVoteSubmitted(%type)
{
   clientCmdClearBottomPrint();
   
   if(%type)
      alxPlay(VoteAgainstSound, 0, 0, 0);
   else
      alxPlay(VoteForSound, 0, 0, 0);
}
// End client voting functions.

function clientCmdEndBomberSight()
{
   PlayGui.remove($bombSightHud);
}

function clientCmdSetBeaconNames(%target, %marker, %vehicle)
{
   setBeaconNames(%target, %marker, %vehicle);
}

function clientCmdStartBomberSight()
{
   $bombSightHud =  new HudBombSight(bombSightName) {
      profile = "GuiDefaultProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "286 206";
      extent = "67 67";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      minDisplayHeight = "20";
   };
   PlayGui.add($bombSightHud);
}

function tempShowSpeed(%client)
{
   if(!$tmpSpeedShow)
      $tmpSpeedShow = true;
   else
      $tmpSpeedShow = false;
   commandToClient(%client, 'toggleSpeed', %client, $tmpSpeedShow);
}

function clientCmdToggleSpeed(%client, %toggle)
{
   if(%toggle) {
      %tempSpeedHud = new GuiTextCtrl(tmpSpeed) {
         profile = "GuiTempSpeedProfile";
         horizSizing = "center";
         vertSizing = "top";
         position = "175 200";
         extent = "120 50";
         minExtent = "8 8";
         visible = "1";
         setFirstResponder = "0";
         modal = "1";
      };
      PlayGui.add(%tempSpeedHud);
      %client.updateTempSpeed(%client);
   }
   else {
      cancel(%client.tmpSpeedCheck);
      tmpSpeed.delete();
   }
}

function GameConnection::updateTempSpeed(%client)
{
   commandToClient(%client, 'getTempSpeed');
   %client.tmpSpeedCheck = %client.schedule(100, "updateTempSpeed", %client);
}

function clientCmdGetTempSpeed()
{
   %vel = getControlObjectSpeed();
   tmpSpeed.setValue(%vel);
}

function clientCmdInitLoadClientFavorites()
{
   loadFavorite($pref::FavCurrentSelect);
}

function clientCmdToggleDashHud(%val)
{
   if(!%val) {
      if(isObject(vDiagramHud))
      {
         vDiagramHud.delete();
         cancel(dashboardHud.speedCheck);
         vSpeedBox.delete();
      }
      if(isObject(vOverheadHud))
         vOverheadHud.delete();
      if(isObject(vEnergyFrame))
         vEnergyFrame.delete();
      if(isObject(vDamageFrame))
         vDamageFrame.delete();
      if(isObject(vAltitudeBox))
      {
         cancel(dashboardHud.altitudeCheck);
         vAltitudeBox.delete();
      }
      if(isObject(vWeaponOne))
         vWeaponOne.delete();
      if(isObject(vWeaponTwo))
         vWeaponTwo.delete();
      if(isObject(vWeaponThree))
         vWeaponThree.delete();
      if(isObject(vWeapHiliteOne))
         vWeapHiliteOne.delete();
      if(isObject(vWeapHiliteTwo))
         vWeapHiliteTwo.delete();
      if(isObject(vWeapHiliteThree))
         vWeapHiliteThree.delete();
      if(isObject(vPassengerHud))
         vPassengerHud.delete();
      if(isObject(bombardierHud))
         bombardierHud.delete();
      if(isObject(turreteerHud))
         turreteerHud.delete();
      // reset in case of vehicle-specific reticle
      reticleHud.setBitmap("");
      reticleFrameHud.setVisible(false);
   }
   dashboardHud.setVisible(%val);
}

function clientCmdShowVehicleGauges(%vehType, %node)
{
   //if(!((%vehType $= "Bomber" || %vehType $= "Assault") && %node > 0))
   if(%node == 0)
   {
      // common elements that show up on all vehicle pilot HUDs
      dashboardHud.diagram = new HudBitmapCtrl(vDiagramHud) {
         profile = "GuiDashBoxProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "200 10";
         extent = "176 108";
         minExtent = "8 8";
         visible = "1";
         bitmap = "gui/hud_veh_new_dash.png";
         opacity = "0.8";
      };
      dashboardHud.add(dashboardHud.diagram);

      dashboardHud.vehDiagram = new HudBitmapCtrl(vOverheadHud) {
         profile = "GuiDashBoxProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "256 0";
         extent = "128 128";
         minExtent = "8 8";
         visible = "1";
         bitmap = "";
         opacity = "1.0";
      };
      dashboardHud.add(dashboardHud.vehDiagram);

      dashboardHud.nrgBar = new HudBitmapCtrl(vEnergyFrame) {
         profile = "GuiDashBoxProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "160 80";
         extent = "118 19";
         minExtent = "8 8";
         visible = "1";
         bitmap = "gui/hud_veh_new_dashpiece_5.png";
         opacity = "0.8";

         new HudEnergy(vEnergyBar) {
            profile = "GuiDefaultProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "0 0";
            extent = "118 19";
            minExtent = "8 8";
            visible = "1";
            fillColor = "0.353000 0.373000 0.933000 0.800000";
            frameColor = "0.000000 1.000000 0.000000 1.000000";
            autoCenter = "0";
            autoResize = "0";
            displayMounted = true;
            bitmap = "gui/hud_veh_new_dashpiece_5.png";
            verticalFill = false;
            subRegion = "4 5 98 10";
            pulseRate = "500";
            pulseThreshold = "0.3";
            //modColor = "1.000000 0.500000 0.000000 1.000000";
         };
      };
      dashboardHud.add(dashboardHud.nrgBar);

      dashboardHud.dmgBar = new HudBitmapCtrl(vDamageFrame) {
         profile = "GuiDashBoxProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "361 80";
         extent = "118 19";
         minExtent = "8 8";
         visible = "1";
         bitmap = "gui/hud_veh_new_dashpiece_4.png";
         opacity = "0.8";

         new HudDamage(vDamageBar) {
            profile = "GuiDefaultProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "0 0";
            extent = "118 19";
            minExtent = "8 8";
            visible = "1";
            fillColor = "0.000000 1.0000 0.000000 0.800000";
            frameColor = "0.000000 1.000000 0.000000 0.000000";
            bitmap = "gui/hud_veh_new_dashpiece_4.png";
            verticalFill = false;
            displayMounted = true;
            opacity = "0.8";
            subRegion = "18 5 97 10";
            pulseRate = "500";
            pulseThreshold = "0.3";
            //modColor = "1.000000 0.500000 0.000000 1.000000";
         };
      };
      dashboardHud.add(dashboardHud.dmgBar);

      dashboardHud.speedBox = new GuiControl(vSpeedBox) {
         profile = "GuiDashBoxProfile";
         horizSizing = "right";
         vertSizing = "top";
         position = "210 47";
         extent = "40 40";
         minExtent = "8 8";
         visible = "1";

         new GuiTextCtrl(vSpeedText) {
            profile = "GuiDashTextProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "3 15";
            extent = "18 15";
            minExtent = "8 8";
            visible = "1";
            text = "test";
         };
         new GuiTextCtrl(vSpeedTxtLbl) {
            profile = "GuiDashTextProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "18 15";
            extent = "27 15";
            minExtent = "8 8";
            visible = "1";
            text = "KPH";
         };
      };
      dashboardHud.add(dashboardHud.speedBox);

      dashboardHud.updateSpeed();
   }

   switch$ (%vehType) {
      case "Shrike" :
         vOverheadHud.setBitmap("gui/hud_veh_icon_shrike.png");
         // add altitude box for flying vehicles
         dashboardHud.altBox = new HudBitmapCtrl(vAltitudeBox) {
            profile = "GuiDashBoxProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "371 56";
            extent = "68 22";
            minExtent = "8 8";
            bitmap = "gui/hud_veh_new_dashpiece_1.png";
            visible = "1";
            opacity = "0.8";

            new GuiTextCtrl(vAltitudeText) {
               profile = "GuiDashTextProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "19 5";
               extent = "18 15";
               minExtent = "8 8";
               visible = "1";
               text = "test";
            };
            new GuiTextCtrl(vAltitudeTxtLbl) {
               profile = "GuiDashTextProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "40 5";
               extent = "12 15";
               minExtent = "8 8";
               visible = "1";
               text = "M";
            };
         };
         dashboardHud.add(dashboardHud.altBox);
         dashboardHud.updateAltitude();
         // add right-hand weapons box and highlight
         dashboardHud.weapon = new GuiControl(vWeapHiliteOne) {
            profile = "GuiDashBoxProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "358 22";
            extent = "80 33";
            minExtent = "8 8";
            visible = "1";

            new HudBitmapCtrl(vWeapBkgdOne) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "0 0";
               extent = "82 40";
               minExtent = "8 8";
               bitmap = "gui/hud_veh_new_dashpiece_2.png";
               visible = "1";
               opacity = "0.8";

               new HudBitmapCtrl(vWeapIconOne) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "28 6";
                  extent = "25 25";
                  minExtent = "8 8";
                  bitmap = "gui/hud_blaster.png";
                  visible = "1";
                  opacity = "0.8";
               };
            };
         };
         dashboardHud.add(dashboardHud.weapon);
         // change to shrike reticle
         reticleHud.setBitmap("gui/hud_ret_shrike.png");
         reticleFrameHud.setVisible(false);

      case "Bomber" :
         if(%node == 1)
         {
            // bombardier hud
            dashboardHud.bHud = new GuiControl(bombardierHud) {
               profile = "GuiDefaultProfile";
               horizSizing = "center";
               vertSizing = "top";
               position = "200 75";
               extent = "240 50";
               minExtent = "8 8";
               visible = "1";

               new HudBitmapCtrl(vWeap1Hilite) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "18 9";
                  extent = "80 44";
                  minExtent = "8 8";
                  visible = "1";
                  bitmap = "gui/hud_veh_new_hilite_left.png";
                  opacity = "0.3";
               };
               new HudBitmapCtrl(vWeap2Hilite) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "141 9";
                  extent = "80 44";
                  minExtent = "8 8";
                  visible = "0";
                  bitmap = "gui/hud_veh_new_hilite_right.png";
                  opacity = "0.3";
               };
               new HudBitmapCtrl(vWeap3Hilite) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "99 9";
                  extent = "40 44";
                  minExtent = "8 8";
                  visible = "0";
                  bitmap = "gui/hud_veh_new_hilite_middle.png";
                  opacity = "0.3";
               };

               new HudBitmapCtrl(bombardierFrame) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "center";
                  vertSizing = "bottom";
                  position = "20 8";
                  extent = "200 40";
                  minExtent = "8 8";
                  visible = "1";
                  bitmap = "gui/hud_veh_new_bombardier_dash.png";
                  opacity = "1.0";

                  new HudBitmapCtrl(vWeaponOne) {
                     profile = "GuiDashBoxProfile";
                     horizSizing = "right";
                     vertSizing = "bottom";
                     position = "28 5";
                     extent = "25 25";
                     minExtent = "8 8";
                     visible = "1";
                     bitmap = "gui/hud_blaster.png";
                  };

                  new HudBitmapCtrl(vWeaponTwo) {
                     profile = "GuiDashBoxProfile";
                     horizSizing = "right";
                     vertSizing = "bottom";
                     position = "87 6";
                     extent = "25 25";
                     minExtent = "8 8";
                     visible = "1";
                     bitmap = "gui/hud_targetlaser.png";
                  };

                  new HudBitmapCtrl(vWeaponThree) {
                     profile = "GuiDashBoxProfile";
                     horizSizing = "right";
                     vertSizing = "bottom";
                     position = "147 6";
                     extent = "25 25";
                     minExtent = "8 8";
                     visible = "1";
                     bitmap = "gui/hud_veh_bomb.png";
                  };
               };
            };
            dashboardHud.add(dashboardHud.bHud);

            dashboardHud.nrgBar = new HudBitmapCtrl(vEnergyFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "110 95";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               flipVertical = true;
               bitmap = "gui/hud_veh_new_dashpiece_5.png";
               opacity = "0.8";

               new HudEnergy(vEnergyBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.353000 0.373000 0.933000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 1.000000";
                  autoCenter = "0";
                  autoResize = "0";
                  displayMounted = true;
                  bitmap = "gui/hud_veh_new_dashpiece_5.png";
                  verticalFill = false;
                  subRegion = "4 5 98 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
                  //modColor = "1.000000 0.500000 0.000000 1.000000";
               };
            };
            dashboardHud.add(dashboardHud.nrgBar);

            dashboardHud.dmgBar = new HudBitmapCtrl(vDamageFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "410 95";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               flipVertical = true;
               bitmap = "gui/hud_veh_new_dashpiece_4.png";
               opacity = "0.8";

               new HudDamage(vDamageBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.000000 1.0000 0.000000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 0.000000";
                  bitmap = "gui/hud_veh_new_dashpiece_4.png";
                  verticalFill = false;
                  displayMounted = true;
                  opacity = "0.8";
                  subRegion = "18 5 97 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
                  //modColor = "1.000000 0.500000 0.000000 1.000000";
               };
            };
            dashboardHud.add(dashboardHud.dmgBar);
            $numVWeapons = 3;
            reticleHud.setBitmap("gui/hud_ret_shrike.png");
            reticleFrameHud.setVisible(false);
         }
         else if(%node == 0)
         {
            // pilot dashboard hud
            vOverheadHud.setBitmap("gui/hud_veh_icon_bomber.png");
            // add altitude box for flying vehicles
            dashboardHud.altBox = new HudBitmapCtrl(vAltitudeBox) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "371 56";
               extent = "68 22";
               minExtent = "8 8";
               bitmap = "gui/hud_veh_new_dashpiece_1.png";
               visible = "1";
               opacity = "0.8";

               new GuiTextCtrl(vAltitudeText) {
                  profile = "GuiDashTextProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "19 5";
                  extent = "18 15";
                  minExtent = "8 8";
                  visible = "1";
                  text = "test";
               };
               new GuiTextCtrl(vAltitudeTxtLbl) {
                  profile = "GuiDashTextProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "40 5";
                  extent = "12 15";
                  minExtent = "8 8";
                  visible = "1";
                  text = "M";
               };
            };
            dashboardHud.add(dashboardHud.altBox);
            dashboardHud.updateAltitude();
         }
         else
         {
            // tailgunner hud
            dashboardHud.vehDiagram = new HudBitmapCtrl(vOverheadHud) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "256 0";
               extent = "128 128";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_icon_bomber.png";
               opacity = "1.0";
            };
            dashboardHud.add(dashboardHud.vehDiagram);

            dashboardHud.nrgBar = new HudBitmapCtrl(vEnergyFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "177 50";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_new_dashpiece_5.png";
               flipVertical = true;
               opacity = "0.8";

               new HudEnergy(vEnergyBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.353000 0.373000 0.933000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 1.000000";
                  autoCenter = "0";
                  autoResize = "0";
                  displayMounted = true;
                  bitmap = "gui/hud_veh_new_dashpiece_5.png";
                  verticalFill = false;
                  subRegion = "4 5 98 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
               };
            };
            dashboardHud.add(dashboardHud.nrgBar);

            dashboardHud.dmgBar = new HudBitmapCtrl(vDamageFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "345 50";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_new_dashpiece_4.png";
               flipVertical = true;
               opacity = "0.8";

               new HudDamage(vDamageBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.000000 1.0000 0.000000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 0.000000";
                  bitmap = "gui/hud_veh_new_dashpiece_4.png";
                  verticalFill = false;
                  displayMounted = true;
                  opacity = "0.8";
                  subRegion = "18 5 97 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
               };
            };
            dashboardHud.add(dashboardHud.dmgBar);
         }
         if(%node != 1)
         {
            // passenger slot "dots"
            vOverheadHud.passengerHud = new GuiControl(vPassengerHud) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "0 0";
               extent = "101 101";
               minExtent = "8 8";
               visible = "1";

               new GuiBitmapCtrl(vPassenger0Slot) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "59 24";
                  extent = "10 10";
                  minExtent = "3 3";
                  visible = "0";
                  bitmap = "gui/hud_veh_seatdot.png";
               };
               new GuiBitmapCtrl(vPassenger1Slot) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "59 39";
                  extent = "10 10";
                  minExtent = "3 3";
                  visible = "0";
                  bitmap = "gui/hud_veh_seatdot.png";
               };
               new GuiBitmapCtrl(vPassenger2Slot) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "59 84";
                  extent = "10 10";
                  minExtent = "3 3";
                  visible = "0";
                  bitmap = "gui/hud_veh_seatdot.png";
               };
            };
            vOverheadHud.add(vOverheadHud.passengerHud);
         }
      case "HAPC" :
         if(%node == 0)
         {
            vOverheadHud.setBitmap("gui/hud_veh_icon_hapc.png");
            // add altitude box for flying vehicles
            dashboardHud.altBox = new HudBitmapCtrl(vAltitudeBox) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "371 56";
               extent = "68 22";
               minExtent = "8 8";
               bitmap = "gui/hud_veh_new_dashpiece_1.png";
               visible = "1";
               opacity = "0.8";

               new GuiTextCtrl(vAltitudeText) {
                  profile = "GuiDashTextProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "19 5";
                  extent = "18 15";
                  minExtent = "8 8";
                  visible = "1";
                  text = "test";
               };
               new GuiTextCtrl(vAltitudeTxtLbl) {
                  profile = "GuiDashTextProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "40 5";
                  extent = "12 15";
                  minExtent = "8 8";
                  visible = "1";
                  text = "M";
               };
            };
            dashboardHud.add(dashboardHud.altBox);
            updateVehicleAltitude();
            dashboardHud.updateAltitude();

         }
         else
         {
            // passenger hud
            dashboardHud.vehDiagram = new HudBitmapCtrl(vOverheadHud) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "256 0";
               extent = "128 128";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_icon_hapc.png";
               opacity = "1.0";
            };
            dashboardHud.add(dashboardHud.vehDiagram);

            dashboardHud.nrgBar = new HudBitmapCtrl(vEnergyFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "180 30";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_new_dashpiece_5.png";
               flipVertical = true;
               opacity = "0.8";

               new HudEnergy(vEnergyBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.353000 0.373000 0.933000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 1.000000";
                  autoCenter = "0";
                  autoResize = "0";
                  displayMounted = true;
                  bitmap = "gui/hud_veh_new_dashpiece_5.png";
                  verticalFill = false;
                  subRegion = "4 5 98 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
               };
            };
            dashboardHud.add(dashboardHud.nrgBar);

            dashboardHud.dmgBar = new HudBitmapCtrl(vDamageFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "342 30";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               bitmap = "gui/hud_veh_new_dashpiece_4.png";
               flipVertical = true;
               opacity = "0.8";

               new HudDamage(vDamageBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.000000 1.0000 0.000000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 0.000000";
                  bitmap = "gui/hud_veh_new_dashpiece_4.png";
                  verticalFill = false;
                  displayMounted = true;
                  opacity = "0.8";
                  subRegion = "18 5 97 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
               };
            };
            dashboardHud.add(dashboardHud.dmgBar);
         }
         // passenger slot "dots"
         vOverheadHud.passengerHud = new GuiControl(vPassengerHud) {
            profile = "GuiDashBoxProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "0 0";
            extent = "101 101";
            minExtent = "8 8";
            visible = "1";
            setFirstResponder = "0";
            modal = "1";
            helpTag = "0";

            new GuiBitmapCtrl(vPassenger0Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "59 65";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
            new GuiBitmapCtrl(vPassenger1Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "59 84";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
            new GuiBitmapCtrl(vPassenger2Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "38 29";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
            new GuiBitmapCtrl(vPassenger3Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "38 50";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
            new GuiBitmapCtrl(vPassenger4Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "80 50";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
            new GuiBitmapCtrl(vPassenger5Slot) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "80 29";
               extent = "10 10";
               minExtent = "3 3";
               visible = "0";
               setFirstResponder = "0";
               modal = "1";
               bitmap = "gui/hud_veh_seatdot.png";
               wrap = "0";
            };
         };
         vOverheadHud.add(vOverheadHud.passengerHud);

      case "Assault" :
         if(%node == 1)
         {
            // turreteer hud
            dashboardHud.tHud = new GuiControl(turreteerHud) {
               profile = "GuiDefaultProfile";
               horizSizing = "center";
               vertSizing = "top";
               position = "225 70";
               extent = "240 50";
               minExtent = "8 8";
               visible = "1";

               new HudBitmapCtrl(vWeap1Hilite) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "10 11";
                  extent = "80 44";
                  minExtent = "8 8";
                  visible = "1";
                  bitmap = "gui/hud_veh_new_hilite_left.png";
                  opacity = "0.4";
               };
               new HudBitmapCtrl(vWeap2Hilite) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "bottom";
                  position = "103 11";
                  extent = "80 44";
                  minExtent = "8 8";
                  visible = "0";
                  bitmap = "gui/hud_veh_new_hilite_right.png";
                  opacity = "0.4";
               };

               new HudBitmapCtrl(turreteerFrame) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "center";
                  vertSizing = "bottom";
                  position = "20 8";
                  extent = "152 36";
                  minExtent = "8 8";
                  visible = "1";
                  bitmap = "gui/hud_veh_new_tankgunner_dash.png";
                  opacity = "0.8";

                  new HudBitmapCtrl(vWeaponOne) {
                     profile = "GuiDashBoxProfile";
                     horizSizing = "right";
                     vertSizing = "bottom";
                     position = "25 8";
                     extent = "25 25";
                     minExtent = "8 8";
                     visible = "1";
                     bitmap = "gui/hud_chaingun.png";
                  };

                  new HudBitmapCtrl(vWeaponTwo) {
                     profile = "GuiDashBoxProfile";
                     horizSizing = "right";
                     vertSizing = "bottom";
                     position = "99 8";
                     extent = "25 25";
                     minExtent = "8 8";
                     visible = "1";
                     bitmap = "gui/hud_mortor.png";
                  };
               };
            };
            dashboardHud.add(dashboardHud.tHud);

            dashboardHud.nrgBar = new HudBitmapCtrl(vEnergyFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "134 95";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               flipVertical = true;
               bitmap = "gui/hud_veh_new_dashpiece_5.png";
               opacity = "0.8";

               new HudEnergy(vEnergyBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.353000 0.373000 0.933000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 1.000000";
                  autoCenter = "0";
                  autoResize = "0";
                  displayMounted = true;
                  bitmap = "gui/hud_veh_new_dashpiece_5.png";
                  verticalFill = false;
                  subRegion = "4 5 98 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
                  //modColor = "1.000000 0.500000 0.000000 1.000000";
               };
            };
            dashboardHud.add(dashboardHud.nrgBar);

            dashboardHud.dmgBar = new HudBitmapCtrl(vDamageFrame) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "390 95";
               extent = "118 19";
               minExtent = "8 8";
               visible = "1";
               flipVertical = true;
               bitmap = "gui/hud_veh_new_dashpiece_4.png";
               opacity = "0.8";

               new HudDamage(vDamageBar) {
                  profile = "GuiDefaultProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "0 0";
                  extent = "118 19";
                  minExtent = "8 8";
                  visible = "1";
                  fillColor = "0.000000 1.0000 0.000000 0.800000";
                  frameColor = "0.000000 1.000000 0.000000 0.000000";
                  bitmap = "gui/hud_veh_new_dashpiece_4.png";
                  verticalFill = false;
                  displayMounted = true;
                  opacity = "0.8";
                  subRegion = "18 5 97 10";
                  pulseRate = "500";
                  pulseThreshold = "0.3";
                  //modColor = "1.000000 0.500000 0.000000 1.000000";
               };
            };
            dashboardHud.add(dashboardHud.dmgBar);

            $numVWeapons = 2;
            // add tank chaingun reticle
            reticleHud.setBitmap("gui/hud_ret_tankchaingun.png");
            reticleFrameHud.setVisible(false);
         }
         else
         {
            // node 0 == driver
            vOverheadHud.setBitmap("gui/hud_veh_icon_assault.png");
            // passenger slot "dots"
            vOverheadHud.passengerHud = new GuiControl(vPassengerHud) {
               profile = "GuiDashBoxProfile";
               horizSizing = "right";
               vertSizing = "top";
               position = "0 0";
               extent = "101 101";
               minExtent = "8 8";
               visible = "1";

               new GuiBitmapCtrl(vPassenger0Slot) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "64 30";
                  extent = "10 10";
                  minExtent = "3 3";
                  visible = "0";
                  bitmap = "gui/hud_veh_seatdot.png";
               };
               new GuiBitmapCtrl(vPassenger1Slot) {
                  profile = "GuiDashBoxProfile";
                  horizSizing = "right";
                  vertSizing = "top";
                  position = "53 53";
                  extent = "10 10";
                  minExtent = "3 3";
                  visible = "0";
                  bitmap = "gui/hud_veh_seatdot.png";
               };
            };
            vOverheadHud.add(vOverheadHud.passengerHud);
         }

      case "Hoverbike" :
         vOverheadHud.setBitmap("gui/hud_veh_icon_hoverbike.png");

      case "MPB" :
         vOverheadHud.setBitmap("gui/hud_veh_icon_mpb.png");

   }
   if(%node == 0)
      vDiagramHud.setVisible(true);
   else
      if(isObject(vDiagramHud))
         vDiagramHud.setVisible(false);
}

function GuiControl::updateAltitude(%this)
{
   %alt = getControlObjectAltitude();
   vAltitudeText.setValue(%alt);
   %this.altitudeCheck = %this.schedule(500, "updateAltitude");
}

function GuiControl::updateSpeed(%this)
{
   %vel = getControlObjectSpeed();
   // convert from m/s to km/h
   %cVel = mFloor(%vel * 3.6); // m/s * (3600/1000) = km/h
   vSpeedText.setValue(%cVel);
   %this.speedCheck = %this.schedule(500, "updateSpeed");
}

//function clientCmdShowVehicleWeapons(%vehicleType)
//{
   // all vehicle weapons are energy based; a -1 displays an infinity symbol
   // for that weapon's ammo amount
   //switch$ (%vehicleType)
   //{
   //   case "ScoutFlyer":
   //    // blaster
   //    vWeaponsBox.addWeapon(0, -1);
   //   case "BomberFlyer":
   //    // plasma, bomb and targeting laser
   //    vWeaponsBox.addWeapon(1, -1);
   //    vWeaponsBox.addWeapon(3, -1);
   //    vWeaponsBox.addWeapon(4, -1);
   //   case "AssaultVehicle":
   //    vWeaponsBox.addWeapon(1, -1);
   //    vWeaponsBox.addWeapon(3, -1);
   //}
   //vWeaponsBox.setVisible(true);
//}

// if set, then static shapes with data member 'noIndividualDamage' set will
// not display their damage bars
function clientCmdProtectingStaticObjects(%val)
{
   NavHud.protectedStatics = %val;
}

function clientCmdCheckPassengers(%pString)
{
   // since each slot is represented by a "1" or a "0" followed by a space, the length
   // of the string divided by 2 is equal to the number of slots in the vehicle
   %numSlots = strlen(%pString) / 2;
   for(%i = 0; %i < %numSlots; %i++)
   {
      %pass = "vPassenger" @ %i @ "Slot";
      if(isObject(%pass))
         if(getWord(%pString, %i) $= "1")
            %pass.setVisible(true);
         else
            %pass.setVisible(false);
   }
}

function clientCmdShowPassenger(%slot, %full)
{
   %dotNum = "vPassenger" @ %slot @ "Slot";
   if(isObject(%dotNum))
      %dotNum.setVisible(%full);
}

function clientCmdClearPassengers()
{
   for(%i = 1; %i < 6; %i++)
   {
      %pass = "vPassenger" @ %i @ "Slot";
      %pass.setVisible(false);
   }
}

addMessageCallback( 'MsgMissionDropInfo', handleDropInfoMessage );
addMessageCallback( 'MsgTeamList', handleTeamListMessage );
addMessageCallback( 'LeaveMissionArea', HandleLeaveMissionAreaAlarmMessage );
addMessageCallback( 'EnterMissionArea', HandleEnterMissionAreaAlarmMessage );
addMessageCallback( 'msgBountyStreakBonus', HandleBountyStreakMessage );
addMessageCallback( 'onClientKicked', handleIveBeenKicked );
addMessageCallback( 'onClientBanned', handleIveBeenBanned );
addMessageCallback( 'msgDeploySensorRed', clientDeploySensorRed );
addMessageCallback( 'msgDeploySensorGrn', clientDeploySensorGrn );
addMessageCallback( 'msgDeploySensorOff', clientDeploySensorOff );
addMessageCallback( 'msgPackIconOff', clientPackIconOff );
addMessageCallback( 'MsgForceObserver', HandleForceObserver );

function HandleForceObserver( %msgType, %msgString )
{
   
}

function handleIveBeenBanned(%msgType, %msgString)
{
   DisconnectedCleanup();
   MessageBoxOk( "DISCONNECT", "You have been banned from this server.", "Disconnect();");
}

function handleIveBeenKicked(%msgType, %msgString)
{
   DisconnectedCleanup();
   MessageBoxOk( "DISCONNECT", "You have been kicked out of the game.", "Disconnect();");
}

function clientDeploySensorRed()
{
   deploySensor.color = "255 0 0";
   deploySensor.setVisible(true);
}

function clientDeploySensorGrn()
{
   deploySensor.color = "0 255 0";
   deploySensor.setVisible(true);
}

function clientDeploySensorOff()
{
   deploySensor.setVisible(false);
}

function clientPackIconOff()
{
   backpackIcon.setBitmap("");
   backpackFrame.setVisible(false);
   backpackText.setValue("");
   backpackText.setVisible(false);
   backpackFrame.pack = false;
}
      
function HandleBountyStreakMessage(%msgType, %msgString, %client, %streak, %award)
{
   %delay = alxGetWaveLen("fx/misc/bounty_bonus.wav");
   %overlap = 0.50;

   alxPlay(BountyBellSound, 0, 0, 0); //first bell 
   for (%loop = 1; %loop < %award; %loop++)  //any repetitions, overlapped.
      schedule((%delay * %loop) * %overlap, 0, "alxPlay", BountyBellSound, 0, 0, 0);   
}

function HandleLeaveMissionAreaAlarmMessage(%msgType, %msgString)
{
   //Tinman - sounds are now sent by the individual game script
   //if(ServerConnection.OutOfBoundsHandle $= "")
   //   ServerConnection.OutOfBoundsHandle = alxPlay(OutOfBoundsSound, 0, 0, 0);
}

function HandleEnterMissionAreaAlarmMessage(%msgType, %msgString)
{
   //Tinman - sounds are now sent by the individual game script
   //if(ServerConnection.OutOfBoundsHandle !$= "")
   //   alxStop(ServerConnection.OutOfBoundsHandle);
   //   
   //ServerConnection.OutOfBoundsHandle = "";   
}

function handleDropInfoMessage( %msgType, %msgString, %map, %gameType, %serverName )
{
   $clServerName = %serverName;
   $clMissionName = %map;
   $clMissionType = %gameType;
}

function handleTeamListMessage( %msgType, %msgString, %teamCount, %teamList )
{
   // Save off the team names:
   $clTeamCount = %teamCount;
   for ( %i = 0; %i < %teamCount; %i++ )
      $clTeamScore[%i + 1, 0] = getRecord( %teamList, %i );

   // Initialize the lobby:
   LobbyPlayerList.initColumns();
}

//----------------------------------------------------------------------------

function clientCmdStartEffect( %effect )
{
   // Put in iterations
   StartEffect( %effect );
}

function clientCmdStopEffect( %effect )
{
   StopEffect( %effect );
}

function clientCmdPickTeam()
{

}

function clientCmdMissionStartPhase1(%seq, %missionName, %musicTrack)
{
   echo( "got client StartPhase1..." );
   
   // Reset the loading progress controls:
   LoadingProgress.setValue( 0 );
   DB_LoadingProgress.setValue( 0 );
   LoadingProgressTxt.setValue( "LOADING MISSION" );
   DB_LoadingProgressTxt.setValue( "LOADING MISSION" );

   clientCmdPlayMusic(%musicTrack);
   commandToServer('MissionStartPhase1Done', %seq);
   clientCmdResetCommandMap();                                          
}

function clientCmdMissionStartPhase2(%seq)
{
   // clean some stuff up.
   MessageHud.close();
   purgeResources();
   cls();
   commandToServer('MissionStartPhase2Done', %seq);
}

function clientCmdMissionStartPhase3(%seq, %missionName)
{
   $MSeq = %seq;
   
   //Reset Inventory Hud... 
   if($Hud['inventoryScreen'] !$= "")
   {
      %favList = $Hud['inventoryScreen'].data[0, 1].type TAB $Hud['inventoryScreen'].data[0, 1].getValue();
      for ( %i = 1; %i < $Hud['inventoryScreen'].count; %i++ )
         if($Hud['inventoryScreen'].data[%i, 1].getValue() $= invalid)
            %favList = %favList TAB $Hud['inventoryScreen'].data[%i, 1].type TAB "EMPTY";  
         else
            %favList = %favList TAB $Hud['inventoryScreen'].data[%i, 1].type TAB $Hud['inventoryScreen'].data[%i, 1].getValue();  
      commandToServer( 'setClientFav', %favList );   
   }   
   else
      commandToServer( 'setClientFav', $pref::Favorite[$pref::FavCurrentSelect]);   
      
   // needed?
   $MissionName = %missionName;
   //commandToServer( 'getScores' );

   // only show dialog if actually lights
   if(lightScene("sceneLightingComplete", $LaunchMode $= "SceneLight" ? "forceWritable" : ""))
   {
      error("beginning SceneLighting....");
      schedule(1, 0, "updateLightingProgress");
      $lightingMission = true;
      LoadingProgress.setValue( 0 );
      DB_LoadingProgress.setValue( 0 );
      LoadingProgressTxt.setValue( "LIGHTING MISSION" );
      DB_LoadingProgressTxt.setValue( "LIGHTING MISSION" );
      $missionLightStarted = true;
      Canvas.repaint();
   }
}

function clientCmdMissionEnd(%seq)
{
   alxStopAll();
   // disable mission lighting if it's going (since the interiors will be gone in a sec)
   $lightingMission = false;
   $sceneLighting::terminateLighting = true;
}

function clientCmdSetPowerAudioProfiles(%up, %down)
{
   setPowerAudioProfiles(%up, %down);
}

function ghostAlwaysStarted(%ghostCount)
{
   echo( "starting to ghost " @ %ghostCount @ " server objects...."); 
   
   LoadingProgress.setValue( 0 );
   DB_LoadingProgress.setValue( 0 );
   LoadingProgressTxt.setValue( "LOADING OBJECTS" );
   DB_LoadingProgressTxt.setValue( "LOADING OBJECTS" );
   Canvas.repaint();
   $ghostCount = %ghostCount;
   $ghostsRecvd = 0;
}

function ghostAlwaysObjectReceived()
{
   $ghostsRecvd++;
   %pct = $ghostsRecvd / $ghostCount;
   LoadingProgress.setValue( %pct );
   DB_LoadingProgress.setValue( %pct );
   Canvas.repaint();
}

function updateLightingProgress()
{
   if( $SceneLighting::lightingProgress == 0)
   {
      if($sceneLightStarted)
      {
         $sceneLightStarted = false;
      }
      else
         $SceneLighting::lightingProgress = 1;    
   } 
   
   LoadingProgress.setValue( $SceneLighting::lightingProgress );
   DB_LoadingProgress.setValue( $SceneLighting::lightingProgress );
   if($lightingMission)
      $lightingProgressThread = schedule(1, 0, "updateLightingProgress");
}

function sceneLightingComplete()
{
   LoadingProgress.setValue( 1 );
   DB_LoadingProgress.setValue( 1 );
   
   echo("Scenelighting done...");
   $lightingMission = false;
   
   cleanUpHuds();
   
   if($LaunchMode $= "SceneLight")
   {
      quit();
      return;
   }

   clientCmdResetHud();
   commandToServer('SetVoiceInfo', $pref::Audio::voiceChannels, $pref::Audio::decodingMask, $pref::Audio::encodingLevel);
   commandToServer('MissionStartPhase3Done', $MSeq);
}

function clientCmdSetVoiceInfo(%channels, %decodingMask, %encodingLevel)
{
   $Audio::serverChannels = %channels;
   $Audio::serverDecodingMask = %decodingMask;
   $Audio::serverEncodingLevel = %encodingLevel;
}

function ClientReceivedDataBlock(%index, %total)
{
   %pct = %index / %total;
   LoadingProgress.setValue( %pct );
   LoadingProgress.setValue( %pct );
   Canvas.repaint();
}

function GameConnection::setLagIcon(%conn, %state)
{
   if(%conn.getAddress() $= "Local")
      return;

   if (%state $= "true")
      $showLagIcon = true;
   else
      $showLagIcon = false;

   //if both icons are not visible, then don't do anything... (probably the F2 score or inv screen is up...)
   if (!clockHud.isVisible() && !lagHudIndicator.isVisible())
      return;

   if (%state $= "true") 
   {  
      clockHud.setVisible(false);
      lagHudIndicator.setVisible(true);
   }
   else
   {
      clockHud.setVisible(true);
      lagHudIndicator.setVisible(false);
   }

   sensorHud.update();
}

function GameConnection::onTargetLocked( %con, %state )
{
   if( %state $= "true" )
   {   
      if( !%con.targetTone )
         %con.targetTone = alxPlay( "sLockedTone", 0, 0, 0 );
   }
   else
   {
      if( %con.targetTone $= "" )
         return;
         
      if( %con.targetTone )
         alxStop( %con.targetTone );
         
      %con.targetTone = "";
   }
}

function GameConnection::onTrackingTarget( %con, %state )
{
   if( %state $= "true" )
   {   
      if( !%con.trackingTargetTone )
         %con.trackingTargetTone = alxPlay( "sSearchingTone", 0, 0, 0 );
   }
   else
   {
      if( %con.trackingTargetTone $= "" )
         return;
      
      if( %con.trackingTargetTone )
         alxStop( %con.trackingTargetTone );
         
      %con.TrackingTargetTone = "";
   } 
}

function GameConnection::onLockWarning( %con, %state )
{
   if( %state $= "true" )
   {   
      if( !%con.lockWarningTone )
         %con.lockWarningTone = alxPlay( "sMissileLockWarningTone", 0, 0, 0 );
         
   }
   else
   {
      if( %con.lockWarningTone $= "" )
         return;
      
      if( %con.lockWarningTone )
         alxStop( %con.lockWarningTone );
         
      %con.lockWarningTone = "";
   }
}

function GameConnection::onHomeWarning( %con, %state )
{
   if( %state $= "true" )
   {   
      if( !%con.homeWarningTone )
         %con.homeWarningTone = alxPlay( "sMissileHomingWarningTone", 0, 0, 0 );
   }
   else
   {
      if( %con.homeWarningTone $= "" )
         return;
      
      if( %con.homeWarningTone )
         alxStop( %con.homeWarningTone );
         
      %con.homeWarningTone = "";
   } 
}

function GameConnection::initialControlSet(%this)
{
   if ( $LaunchMode $= "InteriorView" )
   {
      Canvas.setContent( InteriorPreviewGui );
      return;
   }

   if( Canvas.getContent() != PlayGui.getId() )
   {
      Canvas.setContent( PlayGui );
      Canvas.pushDialog( MainChatHud );
      CommandToServer('PlayContentSet');
   }
}


