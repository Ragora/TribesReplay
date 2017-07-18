if($Host::TimeLimit $= "")
   $Host::TimeLimit = 20;

$SB::WODec = 0.004; // whiteout 
$SB::DFDec = 0.02; // damageFlash

function VerifyCDCheck(%func)
{
   if (!cdFileCheck())
      messageBoxOkCancel("TRIBES 2 CD CHECK", "You must have the Tribes 2 CD in the CD-ROM drive while playing Tribes 2.  Please insert the CD.", "schedule(0, 0, VerifyCDCheck, " @ %func @ ");", "quit();"); 
   else
      call(%func);
}

function CreateServer(%mission, %missionType)
{
   DestroyServer();

   // Load server data blocks
   exec("scripts/commanderMapIcons.cs");
   exec("scripts/markers.cs"); 
   exec("scripts/serverAudio.cs");
   exec("scripts/damageTypes.cs");
   exec("scripts/deathMessages.cs");
   exec("scripts/inventory.cs");
   exec("scripts/camera.cs");
   exec("scripts/particleEmitter.cs");    // Must exist before item.cs and explosion.cs
   exec("scripts/particleDummies.cs");
   exec("scripts/projectiles.cs");        // Must exits before item.cs
   exec("scripts/player.cs");
   exec("scripts/gameBase.cs");
   exec("scripts/staticShape.cs");
   exec("scripts/weapons.cs");
   exec("scripts/turret.cs");
   exec("scripts/weapTurretCode.cs");
   exec("scripts/pack.cs");
   exec("scripts/vehicles/vehicle_spec_fx.cs");    // Must exist before other vehicle files or CRASH BOOM
   exec("scripts/vehicles/serverVehicleHud.cs");
   exec("scripts/vehicles/vehicle_shrike.cs");
   exec("scripts/vehicles/vehicle_bomber.cs");
   exec("scripts/vehicles/vehicle_havoc.cs");
   exec("scripts/vehicles/vehicle_wildcat.cs");
   exec("scripts/vehicles/vehicle_tank.cs");
   exec("scripts/vehicles/vehicle_mpb.cs");
   exec("scripts/vehicles/vehicle.cs");            // Must be added after all other vehicle files or EVIL BAD THINGS
   exec("scripts/ai.cs");
   exec("scripts/item.cs");
   exec("scripts/station.cs");
   exec("scripts/simGroup.cs");
   exec("scripts/trigger.cs");
   exec("scripts/forceField.cs");
   exec("scripts/lightning.cs");
   exec("scripts/weather.cs");
   exec("scripts/deployables.cs");
   exec("scripts/stationSetInv.cs");
   exec("scripts/navGraph.cs");
   exec("scripts/targetManager.cs");
   exec("scripts/serverCommanderMap.cs");
   exec("scripts/environmentals.cs");
   exec("scripts/power.cs");
   exec("scripts/serverTasks.cs");
   exec("scripts/admin.cs");
   exec("prefs/banlist.cs");
   
   //automatically load any mission type that follows naming convention typeGame.name.cs
   %search = "scripts/*Game.cs";    

   for(%file = findFirstFile(%search); %file !$= ""; %file = findNextFile(%search))
   {
      %type = fileBase(%file); // get the name of the script      
     exec("scripts/" @ %type @ ".cs");
   }
   
   $missionSequence = 0;
   $CurrentMissionType = %missionType;
   $HostGameBotCount = 0;
   $HostGamePlayerCount = 0;
   if ( $HostGameType !$= "SinglePlayer" )
      allowConnections(true);
   $ServerGroup = new SimGroup (ServerGroup);
   if(%mission $= "")
   {
      %mission = $HostMissionFile[$HostMission[0,0]];
      %missionType = $HostTypeName[0];
   }

   if ( $HostGameType $= "Online" && $pref::Net::DisplayOnMaster !$= "Never" )
      schedule(0,0,startHeartbeat);

   // setup the bots for this server
   if( $Host::BotsEnabled )
      initGameBots( %mission, %missionType ); 
      
   // load the mission...
   loadMission(%mission, %missionType, true);
}

function initGameBots( %mission, %mType )
{
   echo( "adding bots..." );
   
   AISystemEnabled( false );
   if ( $Host::BotCount > 0 && %mType !$= "SinglePlayer" )
   {
      // Make sure this mission is bot enabled:
      for ( %idx = 0; %idx < $HostMissionCount; %idx++ )
      {
         if ( $HostMissionFile[%idx] $= %mission )
            break;
      }

      if ( $BotEnabled[%idx] )
      {
         if ( $Host::BotCount > 16 )
            $HostGameBotCount = 16;
         else
            $HostGameBotCount = $Host::BotCount;

         if ( $Host::BotCount > $Host::MaxPlayers - 1 )
            $HostGameBotCount = $Host::MaxPlayers - 1;

         //set the objective reassessment timeslice var
         $AITimeSliceReassess = 0;
         aiConnectMultiple( $HostGameBotCount, $Host::MinBotDifficulty, $Host::MaxBotDifficulty, -1 );
      }
      else
      {   
         $HostGameBotCount = 0;
      }
   }
}

function findNextCycleMission()
{
   %numPlayers = ClientGroup.getCount();
   %tempMission = $CurrentMission;
   %failsafe = 0;
   while (1)
   {
      %nextMissionIndex = getNextMission(%tempMission, $CurrentMissionType);
      %nextPotentialMission = $HostMissionFile[%nextMissionIndex];

      //just cycle to the next if we've gone all the way around...
      if (%nextPotentialMission $= $CurrentMission || %failsafe >= 1000)
      {
         %nextMissionIndex = getNextMission($CurrentMission, $CurrentMissionType);
         return $HostMissionName[%nextMissionIndex];
      }

      //get the player count limits for this mission
      %limits = $Host::MapPlayerLimits[%nextPotentialMission, $CurrentMissionType];
      if (%limits $= "")
         return %nextPotentialMission;
      else
      {
         %minPlayers = getWord(%limits, 0);
         %maxPlayers = getWord(%limits, 1);

         if ((%minPlayers < 0 || %numPlayers >= %minPlayers) && (%maxPlayers < 0 || %numPlayers <= %maxPlayers))
            return %nextPotentialMission;
      }

      //since we didn't return the mission, we must not have an acceptable number of players - check the next
      %tempMission = %nextPotentialMission;
      %failsafe++;
   }
}

function CycleMissions()
{
   echo( "cycling mission. " @ ClientGroup.getCount() @ " clients in game." );
   %nextMission = findNextCycleMission();
   messageAll( 'MsgClient', 'Loading %1 (%2)...', %nextMission, $MissionTypeDisplayName );
   loadMission( %nextMission, $CurrentMissionType );
}

function DestroyServer()
{
   $missionRunning = false;
   allowConnections(false);
   stopHeartbeat();
   MissionGroup.delete();
   MissionCleanup.delete();
   if(isObject(game))
   {
      game.deactivatePackages();
      game.delete();
   }
   if(isObject($ServerGroup))
      $ServerGroup.delete();

   // delete all the connections:
   while(ClientGroup.getCount())
   {
      %client = ClientGroup.getObject(0);
      if (%client.isAIControlled())
         %client.drop();
      else
         %client.delete();
   }

   // delete all the data blocks... 
   // this will cause problems if there are any connections
   deleteDataBlocks();
   
   // reset the target manager
   resetTargetManager();

   echo( "exporting server prefs..." );
   export( "$Host::*", "prefs/ServerPrefs.cs", false );
   purgeResources();
}

function Disconnect()
{
   if ( isObject( ServerConnection ) )
      ServerConnection.delete();
   DisconnectedCleanup();
   DestroyServer();
}

function DisconnectedCleanup()
{
   // clear the chat hud message vector
   HudMessageVector.clear();
   if ( isObject( PlayerListGroup ) )
      PlayerListGroup.delete();
   
   // terminate all playing sounds
   alxStopAll();
   
   // clean up voting
   voteHud.voting = false;
   mainVoteHud.setvisible(0);
   
   // clear all print messages
   clientCmdclearBottomPrint();
   clientCmdClearCenterPrint();

   // clear the inventory and weapons hud
   weaponsHud.clearAll();
   inventoryHud.clearAll();
   
   // back to the launch screen
   Canvas.setContent(LaunchGui);
   if ( isObject( MusicPlayer ) )
      MusicPlayer.stop();
   clearTextureHolds();
   purgeResources();

   // Restart the email check:
   if ( !EmailGui.checkingEmail && EmailGui.checkSchedule $= "" )
      CheckEmail( true );

   IRCClient::onLeaveGame();
}

// we pass the guid as well, in case this guy leaves the server.
function kick( %client, %admin, %guid )
{
   if(%admin)   
      messageAll( 'MsgAdminForce', '\c2The Admin has kicked %1.', %client.name );
   else   
      messageAll( 'MsgVotePassed', '\c2%1 was kicked by vote.', %client.name );
   
   messageClient(%client, 'onClientKicked', "");
   messageAllExcept( %client, -1, 'MsgClientDrop', "", %client.name, %client );
   
   if( %client.isAIControlled() )
   {
      $HostGameBotCount--;
      %client.drop();
   }
   else
   {
      %count = ClientGroup.getCount();
      for( %i = 0; %i < %count; %i++ )
      {
         %cl = ClientGroup.getObject( %i );
         if( %cl.guid == %guid )
         {
            // kill and delete this client
            if( isObject( %cl.player ) )
               %cl.player.scriptKill(0);
            
            %cl.schedule(700, "delete");
            
            BanList::add( %guid, "0", $Host::KickBanTime );
            Game.kickGuid = "";
         }   
      }
   }
}

function ban( %client, %admin )
{
   if ( %admin )
      messageAll('MsgAdminForce', '\c2The Admin has banned %1.', %client.name);
   else
      messageAll( 'MsgVotePassed', '\c2%1 was banned by vote.', %client.name );
   
   messageClient(%client, 'onClientBanned', "");
   messageAllExcept( %client, -1, 'MsgClientDrop', "", %client.name, %client );
   
   // kill and delete this client
   if( isObject(%client.player) )
      %client.player.scriptKill(0);
   
   %client.schedule(700, "delete");
   
   BanList::add(%client.guid, %client.getAddress(), $Host::BanTime);
}

function getValidVoicePitch(%voice, %voicePitch)
{
   if (%voicePitch < -1.0)
      %voicePitch = -1.0;
   else if (%voicePitch > 1.0)
      %voicePitch = 1.0;

   //Voice pitch range is from 0.5 to 2.0, however, we should tighten the range to
   //avoid players sounding like mickey mouse, etc...
   //see if we're pitching down - clamp the min pitch at 0.875
   if (%voicePitch < 0)
      return (1.0 + (0.125 * %voicePitch));

   //max voice pitch is 1.125
   else if (%voicePitch > 0)
      return 1.0 + (0.125 * %voicePitch);

   else
      return 1.0;
}

function GameConnection::onConnect( %client, %name, %raceGender, %skin, %voice, %voicePitch )
{
   %client.setMissionCRC($missionCRC);
   sendLoadInfoToClient( %client );

   //%client.setSimulatedNetParams(0.1, 30);
   if ( isDemo() )
   {
      %client.armor = "Light";
      %client.sex = "Male";
      %client.race = "Human";
      %client.name = addTaggedString( "DemoGuy" );
      %client.nameBase = "DemoGuy";
      %client.voice = "Male1";
      %client.voiceTag = addtaggedString( "Male1" );
   }
   else
   {
      // if hosting this server, set this client to superAdmin
      if(%client.getAddress() $= "Local")
      {   
         %client.isAdmin = true;
         %client.isSuperAdmin = true;
      }

      // Get the client's unique id:
      %authInfo = %client.getAuthInfo();
      %client.guid = getField( %authInfo, 3 );
      
      // check admin and super admin list, and set status accordingly
      if ( !%client.isSuperAdmin )
      {
         if ( isOnSuperAdminList( %client ) )
         {   
            %client.isAdmin = true;
            %client.isSuperAdmin = true;   
         }
         else if( isOnAdminList( %client ) )
         {
            %client.isAdmin = true;
         }
      }
            
      // Sex/Race defaults
      switch$ ( %raceGender )
      {
         case "Human Male":
            %client.sex = "Male";
            %client.race = "Human";
         case "Human Female":
            %client.sex = "Female";
            %client.race = "Human";
         case "Bioderm":
            %client.sex = "Male";
            %client.race = "Bioderm";
         default:
            error("Invalid race/gender combo passed: " @ %raceGender);
            %client.sex = "Male";
            %client.race = "Human";
      }
      %client.armor = "Light";

      // Override the connect name if this server does not allow smurfs:
      %realName = getField( %authInfo, 0 );
      if ( $PlayingOnline && $Host::NoSmurfs )
         %name = %realName;

      if ( strcmp( %name, %realName ) == 0 )
      {
         %client.isSmurf = false;

         // Add the tribal tag:
         %tag = getField( %authInfo, 1 );
         %append = getField( %authInfo, 2 );
         if ( %append )
            %name = "\cp\c6" @ %name @ "\c7" @ %tag @ "\co";
         else
            %name = "\cp\c7" @ %tag @ "\c6" @ %name @ "\co";

         %client.sendGuid = %client.guid;
      }
      else
      {
         %client.isSmurf = true;
         %client.sendGuid = 0;
         %name = stripTrailingSpaces( strToPlayerName( %name ) );
         if ( strlen( %name ) < 3 )
            %name = "Poser";
         
         // Make sure the alias is unique:
         %isUnique = true;
         %count = ClientGroup.getCount();
         for ( %i = 0; %i < %count; %i++ )
         {
            %test = ClientGroup.getObject( %i );
            %rawName = stripChars( detag( getTaggedString( %test.name ) ), "\cp\co\c6\c7\c8\c9" );
            if ( strcmp( %name, %rawName ) == 0 )
            {
               %isUnique = false;
               break;
            }
         }

         // Append a number to make the alias unique:
         if ( !%isUnique )
         {
            %suffix = 1;
            while ( !%isUnique )
            {
               %nameTry = %name @ "." @ %suffix;
               %isUnique = true;

               %count = ClientGroup.getCount();
               for ( %i = 0; %i < %count; %i++ )
               {
                  %test = ClientGroup.getObject( %i );
                  %rawName = stripChars( detag( getTaggedString( %test.name ) ), "\cp\co\c6\c7\c8\c9" );
                  if ( strcmp( %nameTry, %rawName ) == 0 )
                  {
                     %isUnique = false;
                     break;
                  }
               }

               %suffix++;
            }

            // Success!
            %name = %nameTry;
         }

         %smurfName = %name;
         // Tag the name with the "smurf" color:
         %name = "\cp\c8" @ %name @ "\co";
      }

      %client.name = addTaggedString(%name);
      if(%client.isSmurf)
         %client.nameBase = %smurfName;
      else
         %client.nameBase = %realName;

      // Make sure that the connecting client is not trying to use a bot skin:
      %temp = detag( %skin );
      if ( %temp $= "basebot" || %temp $= "basebbot" )
         %client.skin = addTaggedString( "base" );
      else
         %client.skin = addTaggedString( %skin );

      %client.voice = %voice;
      %client.voiceTag = addtaggedString(%voice);
      
      //set the voice pitch based on a lookup table from their chosen voice
      %client.voicePitch = getValidVoicePitch(%voice, %voicePitch);
   }

   %client.justConnected = true;
   %client.isReady = false;

   // full reset of client target manager
   clientResetTargets(%client, false);

   %client.target = allocClientTarget(%client, %client.name, %client.skin, %client.voiceTag, '_ClientConnection', 0, 0, %client.voicePitch);
   %client.score = 0;
   %client.team = 0;
   
   $instantGroup = ServerGroup;
   $instantGroup = MissionCleanup;

   echo("CADD: " @ %client @ " " @ %client.getAddress());

   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %recipient = ClientGroup.getObject(%cl);
      if((%recipient != %client))
      {
         // These should be "silent" versions of these messages...
         messageClient(%client, 'MsgClientJoin', "", 
               %recipient.name, 
               %recipient, 
               %recipient.target, 
               %recipient.isAIControlled(), 
               %recipient.isAdmin, 
               %recipient.isSuperAdmin, 
               %recipient.isSmurf, 
               %recipient.sendGuid);

         messageClient(%client, 'MsgClientJoinTeam', "", %recipient.name, $teamName[%recipient.team], %recipient, %recipient.team ); 
      }
   }

//   commandToClient(%client, 'getManagerID', %client);

   commandToClient(%client, 'setBeaconNames', "Target Beacon", "Marker Beacon", "Bomb Target");

   if ( $CurrentMissionType !$= "SinglePlayer" ) 
   {
      messageClient(%client, 'MsgClientJoin', '\c2Welcome to Tribes2 %1.', 
            %client.name, 
            %client, 
            %client.target, 
            false,   // isBot 
            %client.isAdmin, 
            %client.isSuperAdmin, 
            %client.isSmurf, 
            %client.sendGuid );

      messageAllExcept(%client, -1, 'MsgClientJoin', '\c1%1 joined the game.', 
            %client.name, 
            %client, 
            %client.target, 
            false,   // isBot 
            %client.isAdmin, 
            %client.isSuperAdmin, 
            %client.isSmurf,
            %client.sendGuid );
   }
   else
      messageClient(%client, 'MsgClientJoin', "\c0Mission Insertion complete...", 
            %client.name, 
            %client, 
            %client.target, 
            false,   // isBot 
            false,   // isAdmin 
            false,   // isSuperAdmin 
            false,   // isSmurf
            %client.sendGuid );

   //Game.missionStart(%client);
   setDefaultInventory(%client);

   if($missionRunning)
      %client.startMission();
   $HostGamePlayerCount++;
}

function GameConnection::onDrop(%client, %reason)
{
   if(isObject(Game))
      Game.onClientLeaveGame(%client);
   
   if ( $CurrentMissionType $= "SinglePlayer" ) 
      messageAllExcept(%client, -1, 'MsgClientDrop', "", %client.name, %client);
   else
      messageAllExcept(%client, -1, 'MsgClientDrop', '\c1%1 has left the game.', %client.name, %client);

   if ( isObject( %client.camera ) )
      %client.camera.delete();

   freeClientTarget(%client);
   removeTaggedString(%client.name);
   echo("CDROP: " @ %client @ " " @ %client.getAddress());
   $HostGamePlayerCount--;
   
   // reset the server if everyone has left the game
   if( $HostGamePlayerCount - $HostGameBotCount == 0 && $Host::Dedicated)
      schedule(0, 0, "resetServerDefaults");
}

function dismountPlayers()
{
   // make sure all palyers are dismounted from vehicles and have normal huds
   %count = ClientGroup.getCount();
   for(%cl = 0; %cl < %count; %cl++)
   {
      %client = ClientGroup.getObject(%cl);
      %player = %client.player;
      if(%player.isMounted()) {
         %player.unmount();
         commandToClient(%client, 'setHudMode', 'Standard', "", 0);
      }
   }
}

function loadMission( %missionName, %missionType, %firstMission )
{
   if (!$pref::NoClearConsole)
      cls();
   buildLoadInfo( %missionName, %missionType );
   
   // reset all of these
   ClearCenterPrintAll();
   ClearBottomPrintAll();
   
   if( $Host::TournamentMode )
      resetTournamentPlayers();
   
   // Send load info to all the connected clients:
   %count = ClientGroup.getCount();
   for ( %cl = 0; %cl < %count; %cl++ )
   {
      %client = ClientGroup.getObject( %cl );
      if ( !%client.isAIControlled() )
      {   
         sendLoadInfoToClient( %client );
      }   
   }

   // allow load condition to exit out
   schedule(0,ServerGroup,loadMissionStage1,%missionName,%missionType,%firstMission);
}
   
function loadMissionStage1(%missionName, %missionType, %firstMission)
{
   // if a mission group was there, delete prior mission stuff
   if(isObject(MissionGroup))
   {
      // clear out the previous mission paths
      for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
      {
         // clear ghosts and paths from all clients
         %cl = ClientGroup.getObject(%clientIndex);
         %cl.resetGhosting();
         %cl.clearPaths();
         %cl.isReady = "";
         %cl.matchStartReady = false;
      }
      Game.endMission();
      $lastMissionTeamCount = Game.numTeams;
      
      MissionGroup.delete();
      MissionCleanup.delete();
      Game.deactivatePackages();
      Game.delete();
      $ServerGroup.delete();
      $ServerGroup = new SimGroup(ServerGroup);
   }
   
   $CurrentMission = %missionName;
   $CurrentMissionType = %missionType;
   
   createInvBanCount();
   echo("LOADING MISSION: " @ %missionName);
   
   // increment the mission sequence (used for ghost sequencing)
   $missionSequence++;
   
   // if this isn't the first mission, allow some time for the server
   // to transmit information to the clients:
  
// jff: $currentMission  already being used for this purpose, used in 'finishLoadMission'
   $MissionName = %missionName;
   $missionRunning = false;

   if(!%firstMission)
      schedule(15000, ServerGroup, loadMissionStage2);
   else
      loadMissionStage2();
}


function loadMissionStage2()
{
   // create the mission group off the ServerGroup
   echo("Stage 2 load");
   $instantGroup = ServerGroup;

   new SimGroup (MissionCleanup);
   
   if($CurrentMissionType $= "")
   {
      new ScriptObject(Game) {
         class = DefaultGame;
      };
   }
   else
   {
      new ScriptObject(Game) {
         class = $CurrentMissionType @ "Game";
         superClass = DefaultGame;
      };
   }
   // allow the game to activate any packages.
   Game.activatePackages();

   // reset the target manager
   resetTargetManager();

   %file = "missions/" @ $missionName @ ".mis";
   if(!isFile(%file))
      return;

   // send the mission file crc to the clients (used for mission lighting)
   $missionCRC = getFileCRC(%file);
   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %client = ClientGroup.getObject(%i);
      if(!%client.isAIControlled())
         %client.setMissionCRC($missionCRC);
   }

   $countDownStarted = false;
   exec(%file);
   $instantGroup = MissionCleanup;
   
   // pre-game mission stuff
   if(!isObject(MissionGroup))
   {
      error("No 'MissionGroup' found in mission \"" @ $missionName @ "\".");
      schedule(3000, ServerGroup, CycleMissions);
      return;
   }

   MissionGroup.cleanNonType($CurrentMissionType);
   
   // construct paths
   pathOnMissionLoadDone();
   
   $ReadyCount = 0;
   $MatchStarted = false;
   $CountdownStarted = false;
   AISystemEnabled( false );

   // Set the team damage here so that the game type can override it:
   if ( $Host::TournamentMode )
      $TeamDamage = 1;
   else
      $TeamDamage = $Host::TeamDamageOn;

   //the demo version always has team damage off
   if (isDemo() || isDemoServer())
      $TeamDamage = 0;

   Game.missionLoadDone();
   
   // start all the clients in the mission
   $missionRunning = true;
   for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
      ClientGroup.getObject(%clientIndex).startMission();
      
   if(!$MatchStarted && $LaunchMode !$= "NavBuild" && $LaunchMode !$= "SpnBuild" )
   {
      if( $Host::TournamentMode )
         checkTourneyMatchStart();
      else if( $currentMissionType !$= "SinglePlayer" )
         checkMissionStart();
   }
   
   // offline graph builder... 
   if( $LaunchMode $= "NavBuild" )
      buildNavigationGraph( "Nav" );
      
   if( $LaunchMode $= "SpnBuild" )
      buildNavigationGraph( "Spn" );
   purgeResources();
      
}

   
function ShapeBase::cleanNonType(%this, %type)
{
   if(%this.missionTypesList $= "")
      return;
      
   for(%i = 0; (%typei = getWord(%this.missionTypesList, %i)) !$= ""; %i++)
      if(%typei $= %type)
         return;
         
   // first 32 targets are team targets (never allocated/freed)
   // - must reallocate the target if unhiding
   if(%this.getTarget() >= 32)
   {
      freeTarget(%this.getTarget());
      %this.setTarget(-1);
   }

   %this.hide(true);
}

function SimObject::cleanNonType(%this, %type)
{
}

function SimGroup::cleanNonType(%this, %type)
{
   for (%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).cleanNonType(%type);
}

function GameConnection::endMission(%this)
{
   commandToClient(%this, 'MissionEnd', $missionSequence);
}

//--------------------------------------------------------------------------
// client start phases:
// 0: start mission
// 1: got phase1 done
// 2: got datablocks done
// 3: got phase2 done
// 4: got phase3 done
function GameConnection::startMission(%this)
{
   // send over the information that will display the server info
   // when we learn it got there, we'll send the data blocks
   %this.currentPhase = 0;
   commandToClient(%this, 'MissionStartPhase1', $missionSequence, $MissionName, MissionGroup.musicTrack);
}

function serverCmdMissionStartPhase1Done(%client, %seq)
{
   if(%seq != $missionSequence || !$MissionRunning)
      return;

   if(%client.currentPhase != 0)
      return;
   %client.currentPhase = 1;
      
   // when the datablocks are transmitted, we'll send the ghost always objects   
   %client.transmitDataBlocks($missionSequence);
}

function GameConnection::dataBlocksDone( %client, %missionSequence )
{
   echo("GOT DATA BLOCKS DONE FOR: " @ %client);
   if(%missionSequence != $missionSequence)
      return;

   if(%client.currentPhase != 1)
      return;
   %client.currentPhase = 2;

   // only want to set this once... (targets will not be updated/sent until a 
   // client has this flag set)
   if(!%client.getReceivedDataBlocks())
   {
      %client.setReceivedDataBlocks(true);
      sendTargetsToClient(%client);
   }

   commandToClient(%client, 'MissionStartPhase2', $missionSequence);
}

function serverCmdMissionStartPhase2Done(%client, %seq)
{
   if(%seq != $missionSequence || !$MissionRunning)
      return;

   if(%client.currentPhase != 2)
      return;
   %client.currentPhase = 3;

   // when all this good love is over, we'll know that the mission lighting is done
   %client.transmitPaths();
   
   // setup the client team state
   if ( $CurrentMissionType !$= "SinglePlayer" ) 
      serverSetClientTeamState( %client );
   
   // start ghosting
   %client.activateGhosting();
   %client.camera.scopeToClient(%client);
   
   // to the next phase...
   commandToClient(%client, 'MissionStartPhase3', $missionSequence, $CurrentMission);
}

function serverCmdMissionStartPhase3Done(%client, %seq)
{
   if(%seq != $missionSequence || !$MissionRunning)
      return;

   if(%client.currentPhase != 3)
      return;
   %client.currentPhase = 4;

   %client.isReady = true;
   Game.clientMissionDropReady(%client);
}

function serverSetClientTeamState( %client )
{
   // set all player states prior to mission drop ready
   
   // create a new camera for this client
   %client.camera = new Camera() 
   {
      dataBlock = Observer;
   };

   if( isObject( %client.rescheduleVote ) )
      Cancel( %client.rescheduleVote );
   %client.canVote = true;
   %client.rescheduleVote = "";
   
   MissionCleanup.add( %client.camera ); // we get automatic cleanup this way.
   
   %observer = false;
   if( !$Host::TournamentMode )
   {
      if( %client.justConnected )
      {
         %client.justConnected = false;
         %client.camera.getDataBlock().setMode( %client.camera, "justJoined" );
      }
      else
      {
         // server just changed maps - this guy was here before
         if( %client.lastTeam !$= "" )
         {   
            // see if this guy was an observer from last game
            if(%client.lastTeam == 0)
            {
               %observer = true;
               
               %client.camera.getDataBlock().setMode( %client.camera, "ObserverFly" );
            }
            else  // let this player join the team he was on last game
            {
               if(Game.numTeams > 1 && %client.lastTeam <= Game.numTeams )
               {   
                  Game.clientJoinTeam( %client, %client.lastTeam, false );
               }
               else
               {   
                  Game.assignClientTeam( %client );
                  
                  // spawn the player
                  Game.spawnPlayer( %client, false );
               }
            }
         }
         else
         {   
            Game.assignClientTeam( %client );
            
            // spawn the player
            Game.spawnPlayer( %client, false );
         }
         
         if( !%observer )
         {
            if(!$MatchStarted && !$CountdownStarted)              
               %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
            else if(!$MatchStarted && $CountdownStarted)         
               %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
         }   
      }
   }
   else
   {                                                                                                                      
      %client.camera.getDataBlock().setMode( %client.camera, "ObserverFly" );
      
      if( !$MatchStarted && !$CountdownStarted)
      {   
         if($TeamDamage)
            %damMess = "ENABLED";
         else
            %damMess = "DISABLED";
         
         if(Game.numTeams > 1)
            BottomPrint(%client, "Server is Running in Tournament Mode.\nPick a Team\nTeam Damage is " @ %damMess, 0, 3 ); 
      }
      else
      {
         BottomPrint( %client, "\nServer is Running in Tournament Mode", 0, 3 ); 
      }   
   }
}

//function serverCmdPreviewDropReady( %client )
//{
//   $MatchStarted = true;
//   commandToClient( %client, 'SetMoveKeys', true);
//   %markerObj = "0 0 0";
//   %client.camera.mode = "PreviewMode";
//   %client.camera.setTransform( %markerObj );
//   %client.camera.setFlyMode();
//   
//   %client.setControlObject( %client.camera );
//}

function HideHudHACK(%visible)
{
   //compassHud.setVisible(%visible);
   //enerDamgHud.setVisible(%visible);
   retCenterHud.setVisible(%visible);
   reticleFrameHud.setVisible(%visible);
   //invPackHud.setVisible(%visible);
   weaponsHud.setVisible(%visible);
   outerChatHud.setVisible(%visible);
   objectiveHud.setVisible(%visible);
   chatHud.setVisible(%visible);
   navHud.setVisible(%visible);
   //watermarkHud.setVisible(%visible);
   hudClusterBack.setVisible(%visible);
   inventoryHud.setVisible(%visible);
   clockHUD.setVisible(%visible);
   lagHudIndicator.setVisible(false);
}

function ServerPlay2D(%profile)
{
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
      ClientGroup.getObject(%idx).play2D(%profile);
}

function ServerPlay3D(%profile,%transform)
{
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++)
      ClientGroup.getObject(%idx).play3D(%profile,%transform);
}

function clientCmdSetFirstPerson(%value)
{
   $firstPerson = %value;
   if(%value)
      ammoHud.setVisible(true);
   else
      ammoHud.setVisible(false);
}

function clientCmdVehicleMount()
{
   if ( $pref::toggleVehicleView )
   {
      $wasFirstPerson = $firstPerson;
      $firstPerson = false;
   }
}
      
function clientCmdVehicleDismount()
{
   if ( $pref::toggleVehicleView )
      $firstPerson = $wasFirstPerson;
}

function serverCmdSAD( %client, %password )
{
   if( %password !$= "" && %password $= $Host::AdminPassword)
   {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
      %name = getTaggedString( %client.name );
      MessageAll( 'MsgSuperAdminPlayer', '\c2%2 has become a Super Admin by force.', %client, %client.name );   
   }
}

function serverCmdSADSetPassword(%client, %password)
{
   if(%client.isSuperAdmin)
      $Host::AdminPassword = %password;
}

function serverCmdSuicide(%client)
{
   if( $MatchStarted )
      %client.player.scriptKill($DamageType::Suicide);
}   

function serverCmdToggleCamera(%client)
{
   if ($testcheats || $CurrentMissionType $= "SinglePlayer")
   {
      %control = %client.getControlObject();
      if (%control == %client.player)
      {
         %control = %client.camera;
         %control.mode = toggleCameraFly;
         %control.setFlyMode();
      }
      else
      {
         %control = %client.player;
         %control.mode = observerFly;
         %control.setFlyMode();
      }
      %client.setControlObject(%control);
   }
}

function serverCmdDropPlayerAtCamera(%client)
{
   if ($testcheats)
   {
      %client.player.setTransform(%client.camera.getTransform());
      %client.player.setVelocity("0 0 0");
      %client.setControlObject(%client.player);
   }
}

function serverCmdDropCameraAtPlayer(%client)
{
   if ($testcheats)
   {
      %client.camera.setTransform(%client.player.getTransform());
      %client.camera.setVelocity("0 0 0");
      %client.setControlObject(%client.camera);
   }
}

function serverCmdToggleRace(%client)
{
   if ($testcheats)
   {
      if (%client.race $= "Human")
         %client.race = "Bioderm";
      else
         %client.race = "Human";
      %client.player.setArmor(%client.armor);
   }
}

function serverCmdToggleGender(%client)
{
   if ($testcheats)
   {
      if (%client.sex $= "Male")
         %client.sex = "Female";
      else
         %client.sex = "Male";
      %client.player.setArmor(%client.armor);
   }
}

function serverCmdToggleArmor(%client)
{
   if ($testcheats)
   {
      if (%client.armor $= "Light")
         %client.armor = "Medium";
      else
         if (%client.armor $= "Medium")
            %client.armor = "Heavy";
         else
            %client.armor = "Light";
      %client.player.setArmor(%client.armor);
   }
}

function serverCmdPlayCel(%client,%anim)
{
   if ($testcheats)
   {
      %anim = %client.player.celIdx;
      if (%anim++ > 8)
         %anim = 1;
      %client.player.setActionThread("cel"@%anim);
      %client.player.celIdx = %anim;
   }
}

// NOTENOTENOTE: Review
function serverCmdPlayAnim(%client, %anim)
{
   if( %anim $= "Death1" || %anim $= "Death2" || %anim $= "Death3" || %anim $= "Death4" || %anim $= "Death5" ||
       %anim $= "Death6" || %anim $= "Death7" || %anim $= "Death8" || %anim $= "Death9" || %anim $= "Death10" || %anim $= "Death11" )
      return;

   %player = %client.player;
   // don't play animations if player is in a vehicle
   if (%player.isMounted())
      return;

   %weapon = ( %player.getMountedImage($WeaponSlot) == 0 ) ? "" : %player.getMountedImage($WeaponSlot).getName().item;
   if(%weapon $= "MissileLauncher" || %weapon $= "SniperRifle")
   {
      %player.animResetWeapon = true;
      %player.lastWeapon = %weapon;  
      %player.unmountImage($WeaponSlot);
      %obj.setArmThread(look);
   }      
   %player.setActionThread(%anim);
}

function serverCmdPlayDeath(%client,%anim)
{
   if ($testcheats)
   {
      %anim = %client.player.deathIdx;
      if (%anim++ > 11)
         %anim = 1;
      %client.player.setActionThread("death"@%anim,true);
      %client.player.deathIdx = %anim;
   }
}

// NOTENOTENOTE: Review these!
//------------------------------------------------------------                            
// TODO - make this function specify a team to switch to...
function serverCmdClientTeamChange( %client )
{
   // pass this to the game object to handle:
   if ( isObject( Game ) && Game.kickClient != %client)
   {      
      %fromObs = %client.team == 0;
      
      if(%fromObs)
         clearBottomPrint(%client);
      
      Game.clientChangeTeam( %client, "", %fromObs );
   }
}

function serverCanAddBot()
{
   //find out how many bots are already playing
   %botCount = 0;
   %numClients = ClientGroup.getCount();
   for (%i = 0; %i < %numClients; %i++)
   {
      %cl = ClientGroup.getObject(%i);
      if (%cl.isAIcontrolled())
         %botCount++;
   }

   //add only if we have less bots than the bot count, and if there would still be room for a 
   if ($HostGameBotCount > 0 && %botCount < $Host::botCount && %numClients < $Host::maxPlayers - 1)
      return true;
   else
      return false;
}

function serverCmdAddBot( %client )
{
   //only admins can add bots...
   if (%client.isAdmin)
   {
      if (serverCanAddBot())
         aiConnectMultiple( 1, $Host::MinBotDifficulty, $Host::MaxBotDifficulty, -1 );
   }
}

function serverCmdClientJoinTeam( %client, %team )
{
   if ( isObject( Game ) && Game.kickClient != %client)
   {   
      if(%client.team != %team)   
      {   
         %fromObs = %client.team == 0;
         
         if(%fromObs)
            clearBottomPrint(%client);
         
         if( %client.isAIControlled() )
            Game.AIChangeTeam( %client, %team );
         else
            Game.clientChangeTeam( %client, %team, %fromObs );
      }
   }   
}

// this should only happen in single team games
function serverCmdClientAddToGame( %client, %targetClient )
{
   if ( isObject( Game ) )
      Game.clientJoinTeam( %targetClient, 0, $matchstarted );
   
   clearBottomPrint(%targetClient);
   
   if($matchstarted)
   {
      %targetClient.setControlObject( %targetClient.player );
      commandToClient(%targetClient, 'setHudMode', 'Standard');
   }
   else
   {
      %client.notReady = true;
      %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
      %client.setControlObject( %client.camera );
   }
   
   if($Host::TournamentMode && !$CountdownStarted)
   {   
      %client.notReady = true;
      centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
   }
}

function serverCmdClientJoinGame( %client )
{
   if ( isObject( Game ) )
      Game.clientJoinTeam( %client, 0, 1 );
      
   %client.setControlObject( %client.player );
   clearBottomPrint(%client);
   commandToClient(%client, 'setHudMode', 'Standard');
}

function serverCmdClientMakeObserver( %client )
{
   if ( isObject( Game ) && Game.kickClient != %client )
      Game.forceObserver( %client, "playerChoose" );
}

function serverCmdChangePlayersTeam( %clientRequesting, %client, %team)
{
   if( isObject( Game ) && %client != Game.kickClient && %clientRequesting.isAdmin)
   {   
      // clear the pickteam menu
      if( !$MatchStarted && $Host::TournamentMode && !$CountdownStarted && %client.camera.Mode $= "pickingTeam" )
         commandToClient( %client, 'processPickTeam' ); 
      
      serverCmdClientJoinTeam(%client, %team);
      
      if(!$MatchStarted)
      {
         %client.observerMode = "pregame";
         %client.notReady = true;
         %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
         %client.setControlObject( %client.camera );
         
         if($Host::TournamentMode && !$CountdownStarted)
         {   
            %client.notReady = true;
            centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
         }
      }
      else
         commandToClient(%client, 'setHudMode', 'Standard', "", 0);
      
      %multiTeam = (Game.numTeams > 1);
      if(%multiTeam)
      {
         messageClient( %client, 'MsgClient', '\c1The Admin has changed your team.');
         messageAllExcept( %client, -1, 'MsgClient', '\c1The Admin forced %1 to join the %2 team.', %client.name, $teamName[%client.team]);
      }
      else
      {
         messageClient( %client, 'MsgClient', '\c1The Admin has added you to the game.');
         messageAllExcept( %client, -1, 'MsgClient', '\c1The Admin added %1 to the game.', %client.name);
      }
   }
}

function serverCmdForcePlayerToObserver( %clientRequesting, %client )
{
   if( isObject( Game ) && %clientRequesting.isAdmin)
      Game.forceObserver( %client, "adminForce" );
}

//--------------------------------------------------------------------------

function serverCmdTogglePlayerMute(%client, %who)
{
   if (%client.muted[%who])
   {
      %client.muted[%who] = false;
      messageClient(%client, 'MsgPlayerUnmuted', '%1 has been unmuted.', %who.name);
   }
   else
   {
      %client.muted[%who] = true;
      messageClient(%client, 'MsgPlayerMuted', '%1 has been muted.', %who.name);
   }
}

//--------------------------------------------------------------------------
// VOTE MENU FUNCTIONS:
function serverCmdGetVoteMenu( %client, %key )
{
   if ( isObject( Game ) )
      Game.sendGameVoteMenu( %client, %key );
}

function serverCmdGetPlayerPopupMenu( %client, %targetClient, %key )
{
   if ( isObject( Game ) )
      Game.sendGamePlayerPopupMenu( %client, %targetClient, %key );
}

function serverCmdGetTeamList( %client, %key )
{
   if ( isObject( Game ) )
      Game.sendGameTeamList( %client, %key );
}

function serverCmdGetMissionTypes( %client, %key )
{
   for ( %type = 0; %type < $HostTypeCount; %type++ )
      messageClient( %client, 'MsgVoteItem', "", %key, %type, "", $HostTypeDisplayName[%type], true );
}

function serverCmdGetMissionList( %client, %key, %type )
{
   if ( %type < 0 || %type >= $HostTypeCount )
      return;

   for ( %i = $HostMissionCount[%type] - 1; %i >= 0; %i-- )
   {
      %idx = $HostMission[%type, %i];

      // If we have bots, don't change to a mission that doesn't support bots:
      if ( $HostGameBotCount > 0 )
      {
         if( !$BotEnabled[%idx] )
            continue;
      }

      messageClient( %client, 'MsgVoteItem', "", %key, 
            %idx, // mission index, will be stored in $clVoteCmd 
            "", 
            $HostMissionName[%idx], 
            true );
   }
}

function serverCmdGetTimeLimitList( %client, %key, %type )
{
   if ( isObject( Game ) )
      Game.sendTimeLimitList( %client, %key );
}

function serverCmdClientPickedTeam( %client, %option )
{
   if( %option == 1 || %option == 2 )
      Game.clientJoinTeam( %client, %option, false );
   
   else if( %option == 3)
   {   
      Game.assignClientTeam( %client, $MatchStarted );
      Game.spawnPlayer( %client, false );
   }
   else
   {
      Game.forceObserver( %client, "playerChoose" );
      %client.observerMode = "observer";
      %client.notReady = false;
      return;
   }   
         
   ClearBottomPrint(%client);
   %client.observerMode = "pregame";
   %client.notReady = true;
   %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
   commandToClient(%client, 'setHudMode', 'Observer');
   
   
   %client.setControlObject( %client.camera );
   centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
}

function playerPickTeam( %client )
{
   %numTeams = Game.numTeams;
   
   if(%numTeams > 1)
   {
      %client.camera.mode = "PickingTeam";
      schedule( 0, 0, "commandToClient", %client, 'pickTeamMenu', getTaggedString($TeamName[1]), getTaggedString($TeamName[2]));
   }
   else
   {
      Game.clientJoinTeam(%client, 0, 0);
      %client.observerMode = "pregame";
      %client.notReady = true;
      %client.camera.getDataBlock().setMode( %client.camera, "pre-game", %client.player );
      centerprint( %client, "\nPress FIRE when ready.", 0, 3 );
      %client.setControlObject( %client.camera );
   }
}

function serverCmdPlayContentSet( %client )
{
   if( $Host::TournamentMode && !$CountdownStarted && !$MatchStarted )
      playerPickTeam( %client );
}

//--------------------------------------------------------------------------
// This will probably move elsewhere...
function getServerStatusString()
{
   return isObject(Game) ? Game.getServerStatusString() : "NoGame";
}


function dumpGameString()
{
   error( getServerStatusString() );
}

function isOnAdminList(%client)
{
   if( !%totalRecords = getFieldCount( $Host::AdminList ) )
   {   
      return false;
   }
   
   for(%i = 0; %i < %totalRecords; %i++)
   {
      %record = getField( getRecord( $Host::AdminList, 0 ), %i);
      if(%record == %client.guid)
         return true;
   }
   
   return false;
}   

function isOnSuperAdminList(%client)
{
   if( !%totalRecords = getFieldCount( $Host::superAdminList ) )
   {   
      return false;
   }
   
   for(%i = 0; %i < %totalRecords; %i++)
   {
      %record = getField( getRecord( $Host::superAdminList, 0 ), %i);
      if(%record == %client.guid)
         return true;
   }
   
   return false;
}

function ServerCmdAddToAdminList( %admin, %client )
{
   if( !%admin.isSuperAdmin )
      return;
   
   %count = getFieldCount( $Host::AdminList );

   for ( %i = 0; %i < %count; %i++ )
   {
      %id = getField( $Host::AdminList, %i );
      if ( %id == %client.guid )
      {   
         return;  // They're already there!
      }
   }

   if( %count == 0 )
      $Host::AdminList = %client.guid;
   else
      $Host::AdminList = $Host::AdminList TAB %client.guid;
}

function ServerCmdAddToSuperAdminList( %admin, %client )
{
   if( !%admin.isSuperAdmin )
      return;

   %count = getFieldCount( $Host::SuperAdminList );

   for ( %i = 0; %i < %count; %i++ )
   {
      %id = getField( $Host::SuperAdminList, %i );
      if ( %id == %client.guid )
         return;  // They're already there!
   }

   if( %count == 0 )
      $Host::SuperAdminList = %client.guid;
   else
      $Host::SuperAdminList = $Host::SuperAdminList TAB %client.guid;
}

function resetTournamentPlayers()
{
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      %cl.notready = 1;
      %cl.notReadyCount = "";
   }
}

function forceTourneyMatchStart()
{
   %playerCount = 0;
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.camera.Mode $= "pre-game")
         %playerCount++;
   }
   
   // don't start the mission until we have players
   if(%playerCount == 0)
   {   
      return false; 
   }
   
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.camera.Mode $= "pickingTeam")
      {
         // throw these guys into observer mode
         if(Game.numTeams > 1)
            commandToClient( %cl, 'processPickTeam'); // clear the pickteam menu
         Game.forceObserver( %cl, "adminForce" );
      }
   }      
   return true;
}

function startTourneyCountdown()
{
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      ClearCenterPrint(%cl);
      ClearBottomPrint(%cl);
   }
   
   // lets get it on!
   Countdown( 30 * 1000 );
}

function checkTourneyMatchStart()
{
   if( $CountdownStarted || $matchStarted )
      return;
      
   // loop through all the clients and see if any are still notready
   %playerCount = 0;
   %notReadyCount = 0;
   
   %count = ClientGroup.getCount();
   for( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject(%i);
      if(%cl.camera.mode $= "pickingTeam")
      {
         %notReady[%notReadyCount] = %cl;
         %notReadyCount++;
      }   
      else if(%cl.camera.Mode $= "pre-game")
      {
         if(%cl.notready)
         {
            %notReady[%notReadyCount] = %cl;
            %notReadyCount++;
         }
         else
         {   
            %playerCount++;
         }
      }
      else if(%cl.camera.Mode $= "observer")
      {
         // this guy is watching
      }
   }
   
   if(%notReadyCount)
   {
      if(%notReadyCount == 1)
         MessageAll( 'msgHoldingUp', '\c1%1 is holding things up!', %notReady[0].name);
      else if(%notReadyCount < 4)
      {
         for(%i = 0; %i < %notReadyCount - 2; %i++)
            %str = getTaggedString(%notReady[%i].name) @ ", " @ %str;

         %str = "\c2" @ %str @ getTaggedString(%notReady[%i].name) @ " and " @ getTaggedString(%notReady[%i+1].name) 
                     @ " are holding things up!";
         MessageAll( 'msgHoldingUp', %str );
      }
      return;
   }

   if(%playerCount != 0)
   {
      %count = ClientGroup.getCount();
      for( %i = 0; %i < %count; %i++ )
      {
         %cl = ClientGroup.getObject(%i);
         %cl.notready = "";
         %cl.notReadyCount = "";
         ClearCenterPrint(%cl);
         ClearBottomPrint(%cl);
      }
      
      if ( Game.scheduleVote !$= "" && Game.voteType $= "VoteMatchStart") 
      {
         messageAll('closeVoteHud', "");
         cancel(Game.scheduleVote);
         Game.scheduleVote = "";
      }
      
      Countdown(30 * 1000);
   }
}

function checkMissionStart()
{
   %readyToStart = false;
   for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
   {   
      %client = ClientGroup.getObject(%clientIndex);
      if(%client.isReady)
      {   
         %readyToStart = true;
         break;
      }
   }
   
   if(%readyToStart || ClientGroup.getCount() < 1)
   {   
      if($Host::warmupTime > 0 && $CurrentMissionType !$= "SinglePlayer")
         countDown($Host::warmupTime * 1000);
      else
         Game.startMatch();   
   
      for(%x = 0; %x < $NumVehiclesDeploy; %x++)
         $VehiclesDeploy[%x].getDataBlock().schedule(%timeMS / 2, "vehicleDeploy", $VehiclesDeploy[%x], 0, 1);
      $NumVehiclesDeploy = 0;
    }
   else
   {   
      schedule(2000, ServerGroup, "checkMissionStart");     
   }
}

function Countdown(%timeMS)
{
   if($countdownStarted)
      return;
      
   echo("starting mission countdown...");
   
   if(isObject(Game))
      %game = Game.getId();
   else
      return;
     
   $countdownStarted = true;
   Game.matchStart = Game.schedule( %timeMS, "StartMatch" );

   if (%timeMS > 30000)
      notifyMatchStart(%timeMS);
   
   if(%timeMS >= 30000)
      Game.thirtyCount = schedule(%timeMS - 30000, Game, "notifyMatchStart", 30000);
   if(%timeMS >= 15000)
      Game.fifteenCount = schedule(%timeMS - 15000, Game, "notifyMatchStart", 15000);
   if(%timeMS >= 10000)
      Game.tenCount = schedule(%timeMS - 10000, Game, "notifyMatchStart", 10000);
   if(%timeMS >= 5000)
      Game.fiveCount = schedule(%timeMS - 5000, Game, "notifyMatchStart", 5000);
   if(%timeMS >= 4000)
      Game.fourCount = schedule(%timeMS - 4000, Game, "notifyMatchStart", 4000);
   if(%timeMS >= 3000)
      Game.threeCount = schedule(%timeMS - 3000, Game, "notifyMatchStart", 3000);
   if(%timeMS >= 2000)
      Game.twoCount = schedule(%timeMS - 2000, Game, "notifyMatchStart", 2000);
   if(%timeMS >= 1000)
      Game.oneCount = schedule(%timeMS - 1000, Game, "notifyMatchStart", 1000);
}

function EndCountdown(%timeMS)
{
   echo("mission end countdown...");
   
   if(isObject(Game))
      %game = Game.getId();
   else
      return;
      
   if(%timeMS >= 60000)
      Game.endsixtyCount = schedule(%timeMS - 60000, Game, "notifyMatchEnd", 60000);
   if(%timeMS >= 30000)
      Game.endthirtyCount = schedule(%timeMS - 30000, Game, "notifyMatchEnd", 30000);
   if(%timeMS >= 10000)
      Game.endtenCount = schedule(%timeMS - 10000, Game, "notifyMatchEnd", 10000);
   if(%timeMS >= 5000)
      Game.endfiveCount = schedule(%timeMS - 5000, Game, "notifyMatchEnd", 5000);
   if(%timeMS >= 4000)
      Game.endfourCount = schedule(%timeMS - 4000, Game, "notifyMatchEnd", 4000);
   if(%timeMS >= 3000)
      Game.endthreeCount = schedule(%timeMS - 3000, Game, "notifyMatchEnd", 3000);
   if(%timeMS >= 2000)
      Game.endtwoCount = schedule(%timeMS - 2000, Game, "notifyMatchEnd", 2000);
   if(%timeMS >= 1000)
      Game.endoneCount = schedule(%timeMS - 1000, Game, "notifyMatchEnd", 1000);
}

function CancelCountdown()
{
   if(Game.sixtyCount !$= "")
      cancel(Game.sixtyCount);
   if(Game.thirtyCount !$= "")
      cancel(Game.thirtyCount);
   if(Game.fifteenCount !$= "")
      cancel(Game.fifteenCount);
   if(Game.tenCount !$= "")
      cancel(Game.tenCount);
   if(Game.fiveCount !$= "")
      cancel(Game.fiveCount);
   if(Game.fourCount !$= "")
      cancel(Game.fourCount);
   if(Game.threeCount !$= "")
      cancel(Game.threeCount);
   if(Game.twoCount !$= "")
      cancel(Game.twoCount);
   if(Game.oneCount !$= "")
      cancel(Game.oneCount);
   if(isObject(Game))
      cancel(Game.matchStart);
   
   Game.matchStart = "";
   Game.thirtyCount = "";
   Game.fifteenCount = "";
   Game.tenCount = "";
   Game.fiveCount = "";
   Game.fourCount = "";
   Game.threeCount = "";
   Game.twoCount = "";
   Game.oneCount = "";

   $countdownStarted = false;
}

function CancelEndCountdown()
{
   //cancel the mission end countdown...
   if(Game.endsixtyCount !$= "")
      cancel(Game.endsixtyCount);
   if(Game.endthirtyCount !$= "")
      cancel(Game.endthirtyCount);
   if(Game.endtenCount !$= "")
      cancel(Game.endtenCount);
   if(Game.endfiveCount !$= "")
      cancel(Game.endfiveCount);
   if(Game.endfourCount !$= "")
      cancel(Game.endfourCount);
   if(Game.endthreeCount !$= "")
      cancel(Game.endthreeCount);
   if(Game.endtwoCount !$= "")
      cancel(Game.endtwoCount);
   if(Game.endoneCount !$= "")
      cancel(Game.endoneCount);
   
   Game.endmatchStart = "";
   Game.endthirtyCount = "";
   Game.endtenCount = "";
   Game.endfiveCount = "";
   Game.endfourCount = "";
   Game.endthreeCount = "";
   Game.endtwoCount = "";
   Game.endoneCount = "";
}

function resetServerDefaults()
{
   echo( "Resetting server defaults..." );
   
   allowConnections( false );// no connections when resetting the server   
   
   if( isObject( Game ) )
      Game.gameOver();
   else
      schedule( 500, 0, "resetServerDefaults" );
   
   // Override server defaults with prefs:   
   exec( "scripts/ServerDefaults.cs" );
   exec( "prefs/ServerPrefs.cs" );

   // kick all bots...
   removeAllBots();
   
   // add bots back if they were there before..
   if( $Host::botsEnabled )
      initGameBots( $Host::Map, $Host::MissionType );
   
   // load the mission
   loadMission( $Host::Map, $Host::MissionType );
   
   // allow connections since were ready to go.
   allowConnections( true );    
}

function removeAllBots()
{
   while( ClientGroup.getCount() )
   {
      %client = ClientGroup.getObject(0);
      if(%client.isAIControlled())
         %client.drop();
      else
         %client.delete();
   }
}

//------------------------------------------------------------------------------
function getServerGUIDList()
{
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %cl = ClientGroup.getObject( %i );
      if ( isObject( %cl ) && !%cl.isSmurf && !%cl.isAIControlled() )
      {
         %guid = getField( %cl.getAuthInfo(), 3 );
         if ( %guid != 0 )
         {
            if ( %list $= "" )
               %list = %guid;
            else
               %list = %list TAB %guid; 
         }
      }
   }

   return( %list );
}

//------------------------------------------------------------------------------
// will return the first admin found on the server 
function getAdmin()
{
   %admin = 0;
   for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
   {
      %cl = ClientGroup.getObject( %clientIndex );
      if(%cl.isAdmin || %cl.isSuperAdmin)
      {
         %admin = %cl;
         break;
      }
   }
   return %admin;   
}

function serverCmdSetPDAPose(%client, %val)
{
   if(!isObject(%client.player))
      return;

   // if client is in a vehicle, return
   if(%client.player.isMounted())
      return;

   if(%val)
   {
      // play "PDA" animation thread on player
      %client.player.setActionThread("PDA", false);
   }
   else
   {
      // cancel PDA animation thread
      %client.player.setActionThread("root", true);
   }
}

function serverCmdProcessGameLink(%client, %arg1, %arg2, %arg3, %arg4, %arg5)
{
   Game.processGameLink(%client, %arg1, %arg2, %arg3, %arg4, %arg5);
}
