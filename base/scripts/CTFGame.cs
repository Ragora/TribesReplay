// DisplayName = Capture the Flag

//--- GAME RULES BEGIN ---
//Prevent enemy from capturing your flag
//Capture enemy flag and bring it to your team's flag stand
//Score a point each time enemy flag is "capped"
//To score, your flag must be at its stand when the enemy flag arrives
//--- GAME RULES END ---

//exec the AI scripts
exec("scripts/aiCTF.cs");

package CTFGame {

function Generator::onDestroyed(%data, %obj)
{
   if (Game.testGenDestroyed(%obj))
      Game.awardScoreGenDestroy(%obj.lastDamagedBy);
}

function Flag::objectiveInit(%data, %flag)
{
   if (!%flag.isTeamSkinned)
   {
      %pos = %flag.getTransform();
      %group = %flag.getGroup();
   }
   %flag.originalPosition = %flag.getTransform();
   %flag.isHome = true;
   %flag.carrier = "";
   setTargetSkin(%flag.getTarget(), $TeamSkin[%flag.team]);
   setTargetSensorGroup(%flag.getTarget(), %flag.team);
   setTargetAlwaysVisMask(%flag.getTarget(), 0x7);
   setTargetRenderMask(%flag.getTarget(), getTargetRenderMask(%flag.getTarget()) | 0x2);
   %flag.scopeWhenSensorVisible(true);
   $flagStatus[%flag.team] = "<At Base>";

   //Point the flag and stand at each other
   %group = %flag.getGroup();
   %count = %group.getCount();
   %flag.stand = "";
   for(%i = 0; %i < %count; %i++)
   {
      %this = %group.getObject(%i);
      if((%this.getClassName() !$= "InteriorInstance") && (%this.getClassName() !$= "SimGroup"))
      {
         if(%this.getDataBlock().getName() $= "ExteriorFlagStand")
         {
            %flag.stand = %this;
            %this.flag = %flag;
         }
      }
   }

   // set the nametag on the target
   setTargetName(%flag.getTarget(), $teamName[%flag.team]);

   // create a marker on this guy
   %flag.waypoint = new MissionMarker() {
      position = %flag.getTransform();
      dataBlock = "FlagMarker";
   };
   MissionCleanup.add(%flag.waypoint);

   // create a target for this (there is no MissionMarker::onAdd script call)
   %target = createTarget(%flag.waypoint, $teamName[%flag.team], "", "", 'Base', %flag.team, 0);
   setTargetAlwaysVisMask(%target, 0xffffffff);

   //store the flag in an array
   $TeamFlag[%flag.team] = %flag;
}

};

//-- tracking  ---
// .deaths .kills .genDefends .carrierKills .flagDefends .escortAssists .suicides .teamKills .turretKills .flagCaps
// .flagReturns .genDestroys .genRepairs

function CTFGame::initGameVars(%game)
{
   %game.SCORE_PER_SUICIDE = -1; 
   %game.SCORE_PER_TEAMKILL = -1;
   %game.SCORE_PER_DEATH = -1;  

   %game.SCORE_PER_KILL = 1; 
   %game.SCORE_PER_PLYR_FLAG_CAP = 3;
   %game.SCORE_PER_TEAM_FLAG_CAP = 1;  
   %game.SCORE_PER_GEN_DESTROY = 2;
   %game.SCORE_PER_ESCORT_ASSIST = 1;

   %game.SCORE_PER_TURRET_KILL = 1; 
   %game.SCORE_PER_FLAG_DEFEND = 1; 
   %game.SCORE_PER_CARRIER_KILL = 1;
   %game.SCORE_PER_FLAG_RETURN = 1;
   %game.SCORE_PER_GEN_DEFEND = 1;
   %game.SCORE_PER_GEN_REPAIR = 1;
   
   %game.FLAG_RETURN_DELAY = 45 * 1000; //45 seconds

   %game.TIME_CONSIDERED_FLAGCARRIER_THREAT = 3 * 1000;  //after damaging enemy flag carrier
   %game.RADIUS_GEN_DEFENSE = 20;  //meters
   %game.RADIUS_FLAG_DEFENSE = 20;  //meters 

   %game.fadeTimeMS = 2000;

   %game.notifyMineDist = 7.5;

   %game.stalemate = false;
   %game.stalemateObjsVisible = false;
   %game.stalemateTimeMS = 60000;
   %game.stalemateFreqMS = 15000;
   %game.stalemateDurationMS = 6000;
}

//--------------------------------------------------------------------------
// need to have this for the corporate maps which could not be fixed
function SimObject::clearFlagWaypoints(%this)
{
}

function WayPoint::clearFlagWaypoints(%this)
{
   logEcho("Removing flag waypoint: " @ %this);
   if(%this.nameTag $= "Flag")
      %this.delete();
}

function SimGroup::clearFlagWaypoints(%this)
{
   for(%i = %this.getCount() - 1; %i >= 0; %i--)
      %this.getObject(%i).clearFlagWaypoints();
}

//--------------------------------------------------------------------------
function CTFGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

   for(%i = 1; %i < (%game.numTeams + 1); %i++)
      $teamScore[%i] = 0;

   // remove 
   MissionGroup.clearFlagWaypoints();
}

function CTFGame::playerTouchFlag(%game, %player, %flag)
{
   %client = %player.client;
   
   if ((%flag.carrier $= "") && (%player.getState() !$= "Dead"))
   {
      //flag isn't held and has been touched by a live player
      if (%client.team == %flag.team)
         %game.playerTouchOwnFlag(%player, %flag);
      else
         %game.playerTouchEnemyFlag(%player, %flag);
   }

   // toggle visibility of the flag
   setTargetRenderMask(%flag.waypoint.getTarget(), %flag.isHome ? 0 : 1);
}

function CTFGame::playerTouchOwnFlag(%game, %player, %flag)
{   
   if(%flag.isHome)
   {
      if (%player.holdingFlag !$= "")      
         %game.flagCap(%player);
   }
   else      
      %game.flagReturn(%flag, %player);
            
   //call the AI function
   %game.AIplayerTouchOwnFlag(%player, %flag);            
}

function CTFGame::playerTouchEnemyFlag(%game, %player, %flag)
{
   %client = %player.client;
   %player.holdingFlag = %flag;  //%player has this flag
   %flag.carrier = %player;  //this %flag is carried by %player

   %player.mountImage(FlagImage, $FlagSlot, true, $teamSkin[%flag.team]);

   %game.playerGotFlagTarget(%player);
   //only cancel the return timer if the player is in bounds...
   if (!%client.outOfBounds)
   {
      cancel($FlagReturnTimer[%flag]);
      $FlagReturnTimer[%flag] = "";
   }

   //if this flag was "at home", see if both flags have now been taken
   if (%flag.isHome)
   {
      %startStalemate = false;
      if ($TeamFlag[1] == %flag)
         %startStalemate = !$TeamFlag[2].isHome;
      else
         %startStalemate = !$TeamFlag[1].isHome;

      if (%startStalemate)
         %game.stalemateSchedule = %game.schedule(%game.stalemateTimeMS, beginStalemate);
   }

   %flag.hide(true);
   %flag.isHome = false;
   if(%flag.stand)
      %flag.stand.getDataBlock().onFlagTaken(%flag.stand);//animate, if exterior stand

   $flagStatus[%flag.team] = %client.nameBase;
   messageTeamExcept(%client, 'MsgCTFFlagTaken', '\c2Teammate %1 took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, $teamName[%flag.team], %flag.team, %client.nameBase);
   messageTeam(%flag.team, 'MsgCTFFlagTaken', '\c2Your flag has been taken by %1!~wfx/misc/flag_taken.wav',%client.name, 0, %flag.team, %client.nameBase);
   messageTeam(0, 'MsgCTFFlagTaken', '\c2%1 took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, $teamName[%flag.team], %flag.team, %client.nameBase);
   messageClient(%client, 'MsgCTFFlagTaken', '\c2You took the %2 flag.~wfx/misc/flag_snatch.wav', %client.name, $teamName[%flag.team], %flag.team, %client.nameBase);     
   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") took team "@%flag.team@" flag");
   
   //call the AI function
   %game.AIplayerTouchEnemyFlag(%player, %flag);

   //if the player is out of bounds, then in 3 seconds, it should be thrown back towards the in bounds area...
   if (%client.outOfBounds)
      %game.schedule(3000, "boundaryLoseFlag", %player);
}

function CTFGame::playerGotFlagTarget(%game, %player)
{
   %player.scopeWhenSensorVisible(true);
   %target = %player.getTarget();
   setTargetRenderMask(%target, getTargetRenderMask(%target) | 0x2);
   if(%game.stalemateObjsVisible)
      setTargetAlwaysVisMask(%target, 0x7);
}

function CTFGame::playerLostFlagTarget(%game, %player)
{
   %player.scopeWhenSensorVisible(false);
   %target = %player.getTarget();
   setTargetRenderMask(%target, getTargetRenderMask(%target) & ~0x2);
   // clear his always vis target mask
   setTargetAlwaysVisMask(%target, (1 << getTargetSensorGroup(%target)));
}

function CTFGame::playerDroppedFlag(%game, %player)
{
   %client = %player.client;
   %flag = %player.holdingFlag;

   %game.playerLostFlagTarget(%player);

   %player.holdingFlag = ""; //player isn't holding a flag anymore
   %flag.carrier = "";  //flag isn't held anymore 
   $flagStatus[%flag.team] = "<In the Field>";
   
   %player.unMountImage($FlagSlot);   
   %flag.hide(false); //Does the throwItem function handle this?   

   messageTeamExcept(%client, 'MsgCTFFlagDropped', '\c2Teammate %1 dropped the %2 flag.~wfx/misc/flag_drop.wav', %client.name, $teamName[%flag.team], %flag.team);
   messageTeam(%flag.team, 'MsgCTFFlagDropped', '\c2Your flag has been dropped by %1!~wfx/misc/flag_drop.wav', %client.name, 0, %flag.team);
   messageTeam(0, 'MsgCTFFlagDropped', '\c2%1 dropped the %2 flag.~wfx/misc/flag_drop.wav', %client.name, $teamName[%flag.team], %flag.team);
   if(!%player.client.outOfBounds)
      messageClient(%client, 'MsgCTFFlagDropped', '\c2You dropped the %2 flag.~wfx/misc/flag_drop.wav', 0, $teamName[%flag.team], %flag.team);
   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") dropped team "@%flag.team@" flag");

   //don't duplicate the schedule if there's already one in progress...
   if ($FlagReturnTimer[%flag] <= 0)
      $FlagReturnTimer[%flag] = %game.schedule(%game.FLAG_RETURN_DELAY - %game.fadeTimeMS, "flagReturnFade", %flag);
     
   //call the AI function
   %game.AIplayerDroppedFlag(%player, %flag);
}

function CTFGame::flagCap(%game, %player)
{
   %client = %player.client;
   %flag = %player.holdingFlag;
   %flag.carrier = "";

   %game.playerLostFlagTarget(%player);
   //award points to player and team
   messageTeamExcept(%client, 'MsgCTFFlagCapped', '\c2%1 captured the %2 flag!~wfx/misc/flag_capture.wav', %client.name, $teamName[%flag.team], %flag.team, %client.team);
   messageTeam(%flag.team, 'MsgCTFFlagCapped', '\c2Your flag was captured by %1.~wfx/misc/flag_lost.wav', %client.name, 0, %flag.team, %client.team); 
   messageTeam(0, 'MsgCTFFlagCapped', '\c2%1 captured the %2 flag!~wfx/misc/flag_capture.wav', %client.name, $teamName[%flag.team], %flag.team, %client.team); 
   messageClient(%client, 'MsgCTFFlagCapped', '\c2You captured the %2 flag!~wfx/misc/flag_capture.wav', 0, $teamName[%flag.team], %flag.team, %client.team);

   logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") capped team "@%client.team@" flag");
   %player.holdingFlag = ""; //no longer holding it.
   %player.unMountImage($FlagSlot);
   %game.flagReset(%flag);
   %game.awardScoreFlagCap(%client, %flag);   
     
   //call the AI function
   %game.AIflagCap(%player, %flag);

   //if this cap didn't end the game, play the announcer...
   if ($missionRunning)
   {
      if ($teamName[%client.team] $= 'Inferno')
         messageAll("", '~wvoice/announcer/ann.infscores.wav');
      else if ($teamName[%client.team] $= 'Storm')
         messageAll("", '~wvoice/announcer/ann.stoscores.wav');
   }
}

function CTFGame::flagReturnFade(%game, %flag)
{
   $FlagReturnTimer[%flag] = %game.schedule(%game.fadeTimeMS, "flagReturn", %flag);
   %flag.startFade(%game.fadeTimeMS, 0, true);
}

function CTFGame::flagReturn(%game, %flag, %player)
{
   cancel($FlagReturnTimer[%flag]);
   $FlagReturnTimer[%flag] = "";

   if(%flag.team == 1)
      %otherTeam = 2;
   else
      %otherTeam = 1;
   if (%player !$= "")
   {
      //a player returned it
      %client = %player.client;
      messageTeamExcept(%client, 'MsgCTFFlagReturned', '\c2Teammate %1 returned your flag to base.~wfx/misc/flag_return.wav', %client.name, 0, %flag.team);
      messageTeam(%otherTeam, 'MsgCTFFlagReturned', '\c2Enemy %1 returned the %2 flag.~wfx/misc/flag_return.wav', %client.name, $teamName[%flag.team], %flag.team);
      messageTeam(0, 'MsgCTFFlagReturned', '\c2%1 returned the %2 flag.~wfx/misc/flag_return.wav', %client.name, $teamName[%flag.team], %flag.team);
      messageClient(%client, 'MsgCTFFlagReturned', '\c2You returned your flag.~wfx/misc/flag_return.wav', 0, $teamName[%flag.team], %flag.team);
      logEcho(%client.nameBase@" (pl "@%player@"/cl "@%client@") returned team "@%flag.team@" flag");
      %game.awardScoreFlagReturn(%player.client); 
   }      
   else
   {
      //returned due to timer
      messageTeam(%otherTeam, 'MsgCTFFlagReturned', '\c2The %2 flag was returned to base.~wfx/misc/flag_return.wav', 0, $teamName[%flag.team], %flag.team);  //because it was dropped for too long
      messageTeam(%flag.team, 'MsgCTFFlagReturned', '\c2Your flag was returned.~wfx/misc/flag_return.wav', 0, 0, %flag.team);
      messageTeam(0, 'MsgCTFFlagReturned', '\c2The %2 flag was returned to base.~wfx/misc/flag_return.wav', 0, $teamName[%flag.team], %flag.team);
      logEcho("team "@%flag.team@" flag returned (timeout)");
   }
   %game.flagReset(%flag);
}

function CTFGame::showStalemateTargets(%game)
{
   cancel(%game.stalemateSchedule);

   //show the targets
   for (%i = 1; %i <= 2; %i++)
   {
      %flag = $TeamFlag[%i];

      //find the object to scope/waypoint....
      //render the target hud icon for slot 1 (a centermass flag)
      //if we just set him as always sensor vis, it'll work fine.
      if (isObject(%flag.carrier))
         setTargetAlwaysVisMask(%flag.carrier.getTarget(), 0x7);
   }

   //schedule the targets to hide
   %game.stalemateObjsVisible = true;
   %game.stalemateSchedule = %game.schedule(%game.stalemateDurationMS, hideStalemateTargets);
}

function CTFGame::hideStalemateTargets(%game)
{
   cancel(%game.stalemateSchedule);

   //hide the targets
   for (%i = 1; %i <= 2; %i++)
   {
      %flag = $TeamFlag[%i];
      if (isObject(%flag.carrier))
      {
         %target = %flag.carrier.getTarget();
         setTargetAlwaysVisMask(%target, (1 << getTargetSensorGroup(%target)));
      }
   }

   //schedule the targets to show again
   %game.stalemateObjsVisible = false;
   %game.stalemateSchedule = %game.schedule(%game.stalemateFreqMS, showStalemateTargets);
}

function CTFGame::beginStalemate(%game)
{
   %game.stalemate = true;
   %game.showStalemateTargets();
}

function CTFGame::endStalemate(%game)
{
   %game.stalemate = false;
   %game.hideStalemateTargets();
   cancel(%game.stalemateSchedule);
}

function CTFGame::flagReset(%game, %flag)
{
   //any time a flag is reset, kill the stalemate schedule
   %game.endStalemate();

   //make sure if there's a player carrying it (probably one out of bounds...), it is stripped first
   if (isObject(%flag.carrier))
   {
      //hide the target hud icon for slot 2 (a centermass flag - visible only as part of a teams sensor network)
      %game.playerLostFlagTarget(%flag.carrier);
      %flag.carrier.holdingFlag = ""; //no longer holding it.
      %flag.carrier.unMountImage($FlagSlot);
   }

   //fades, restore default position, home, velocity, general status, etc.
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%flag.originalPosition);
   %flag.isHome = true;
   %flag.carrier = "";
   $flagStatus[%flag.team] = "<At Base>";
   %flag.hide(false);
   if(%flag.stand)
      %flag.stand.getDataBlock().onFlagReturn(%flag.stand);//animate, if exterior stand

   //fade the flag in...
   %flag.startFade(%game.fadeTimeMS, 0, false);         

   // dont render base target
   setTargetRenderMask(%flag.waypoint.getTarget(), 0);

   //call the AI function
   %game.AIflagReset(%flag);
}

function CTFGame::timeLimitReached(%game)
{
   logEcho("game over (timelimit)");
   %game.gameOver();
   cycleMissions();
}

function CTFGame::scoreLimitReached(%game)
{
   logEcho("game over (scorelimit)");
   %game.gameOver();
   cycleMissions();
}

function CTFGame::notifyMineDeployed(%game, %mine)
{
   //see if the mine is within 5 meters of the flag stand...
   %mineTeam = %mine.sourceObject.team;
   %homeFlag = $TeamFlag[%mineTeam];
   if (isObject(%homeFlag))
   {
      %dist = VectorDist(%homeFlag.originalPosition, %mine.position);
      if (%dist <= %game.notifyMineDist)
      {
         messageTeam(%mineTeam, 'MsgCTFFlagMined', "The flag has been mined.~wvoice/announcer/flag_minedFem.wav" );
      }
   }
}

function CTFGame::gameOver(%game)
{
   //call the default
   DefaultGame::gameOver(%game);

   //send the winner message
   %winner = "";
   if ($teamScore[1] > $teamScore[2])
      %winner = $teamName[1];
   else if ($teamScore[2] > $teamScore[1])
      %winner = $teamName[2];

   if (%winner $= 'Storm')
      messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.stowins.wav" );
   else if (%winner $= 'Inferno')
      messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.infwins.wav" );
   else
      messageAll('MsgGameOver', "Match has ended.~wvoice/announcer/ann.gameover.wav" );

   messageAll('MsgClearObjHud', "");
   for(%i = 0; %i < ClientGroup.getCount(); %i ++) 
   {
      %client = ClientGroup.getObject(%i);
      %game.resetScore(%client);
   }
   for(%j = 1; %j <= %game.numTeams; %j++)
      $TeamScore[%j] = 0;
}

function CTFGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc)
{ 
   //the DefaultGame will set some vars
   DefaultGame::onClientDamaged(%game, %clVictim, %clAttacker, %damageType, %implement, %damageLoc);
   
     
   //if victim is carrying a flag and is not on the attackers team, mark the attacker as a threat for x seconds(for scoring purposes)
   if ((%clVictim.holdingFlag !$= "") && (%clVictim.team != %clAttacker.team))
   {
      %clAttacker.dmgdFlagCarrier = true;
      cancel(%clAttacker.threatTimer);  //restart timer    
      %clAttacker.threatTimer = schedule(%game.TIME_CONSIDERED_FLAGCARRIER_THREAT, %clAttacker.dmgdFlagCarrier = false);
   }
}
      
////////////////////////////////////////////////////////////////////////////////////////
function CTFGame::clientMissionDropReady(%game, %client)
{   
   messageClient(%client, 'MsgClientReady',"", %game.class);
   %game.resetScore(%client);
   for(%i = 1; %i <= %game.numTeams; %i++)
   {
      $Teams[%i].score = 0;
      messageClient(%client, 'MsgCTFAddTeam', "", %i, $TeamName[%i], $flagStatus[%i], $TeamScore[%i]);
   }
   //%game.populateTeamRankArray(%client);

   //messageClient(%client, 'MsgYourRankIs', "", -1);
      
   messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 

   DefaultGame::clientMissionDropReady(%game, %client);
}

function CTFGame::assignClientTeam(%game, %client, %respawn)
{
   DefaultGame::assignClientTeam(%game, %client, %respawn);
   // if player's team is not on top of objective hud, switch lines
   messageClient(%client, 'MsgCheckTeamLines', "", %client.team);
}

function CTFGame::recalcScore(%game, %cl)
{
   %killValue = %cl.kills * %game.SCORE_PER_KILL;
   %deathValue = %cl.deaths * %game.SCORE_PER_DEATH;

   if (%killValue - %deathValue == 0)
      %killPoints = 0;
   else
      %killPoints = (%killValue * %killValue) / (%killValue - %deathValue);

   %cl.offenseScore = %killPoints +
                        %cl.suicides * %game.SCORE_PER_SUICIDE + //-1
                        %cl.escortAssists * %game.SCORE_PER_ESCORT_ASSIST + // 1
                        %cl.teamKills * %game.SCORE_PER_TEAMKILL + // -1
                        %cl.flagCaps * %game.SCORE_PER_PLYR_FLAG_CAP + // 3
                        %cl.genDestroys * %game.SCORE_PER_GEN_DESTROY; // 2

   %cl.defenseScore =   %cl.genDefends * %game.SCORE_PER_GEN_DEFEND +   // 1
                        %cl.carrierKills * %game.SCORE_PER_CARRIER_KILL +  // 1
                        %cl.escortAssists * %game.SCORE_PER_ESCORT_ASSIST + // 1
                        %cl.turretKills * %game.SCORE_PER_TURRET_KILL +  // 1
                        %cl.flagReturns * %game.SCORE_PER_FLAG_RETURN +  // 1
                        %cl.genRepairs * %game.SCORE_PER_GEN_REPAIR;  // 1

   %cl.score = mFloor(%cl.offenseScore + %cl.defenseScore);

   %game.recalcTeamRanks(%cl);
}

function CTFGame::updateKillScores(%game, %clVictim, %clKiller, %damageType, %implement)
{
   if(%game.testTurretKill(%implement))   //check for turretkill before awarded a non client points for a kill
      %game.awardScoreTurretKill(%clVictim, %implement);  
   else if (%game.testKill(%clVictim, %clKiller)) //verify victim was an enemy
   {
      %game.awardScoreKill(%clKiller);
      %game.awardScoreDeath(%clVictim);

      if (%game.testGenDefend(%clVictim, %clKiller))
         %game.awardScoreGenDefend(%clKiller);

      if(%game.testCarrierKill(%clVictim, %clKiller))    
         %game.awardScoreCarrierKill(%clKiller);
      else
      {
         if (%game.testFlagDefend(%clVictim, %clKiller))
            %game.awardScoreFlagDefend(%clKiller);
      }
      if (%game.testEscortAssist(%clVictim, %clKiller))
         %game.awardScoreEscortAssist(%clKiller);     
   }       
   else
   {        
      if (%game.testSuicide(%clVictim, %clKiller, %damageType))  //otherwise test for suicide
      {
         %game.awardScoreSuicide(%clVictim);     
      }
      else
      {
         if (%game.testTeamKill(%clVictim, %clKiller)) //otherwise test for a teamkill
            %game.awardScoreTeamKill(%clVictim, %clKiller);
      }
   }        
}

function CTFGame::testFlagDefend(%game, %victimID, %killerID)
{
   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_FLAG_DEFENSE, $TypeMasks::ItemObjectType);
   %objID = containerSearchNext();   
   while(%objID != 0) 
   {
     %objType = %objID.getDataBlock().getName();
     if ((%objType $= "Flag") && (%objID.team == %killerID.team)) 
          return true;  //found the(a) killer's flag near the victim's point of death
     else
        %objID = containerSearchNext();     
   }
   return false; //didn't find a qualifying flag within required radius of victims point of death  
}

function CTFGame::testGenDefend(%game, %victimID, %killerID)
{
   InitContainerRadiusSearch(%victimID.plyrPointOfDeath, %game.RADIUS_GEN_DEFENSE, $TypeMasks::StaticShapeObjectType);
   %objID = containerSearchNext();
   while(%objID != 0)
   {
      %objType = %objID.getDataBlock().ClassName;
     if ((%objType $= "generator") && (%objID.team == %killerID.team)) 
        return true;  //found a killer's generator within required radius of victim's death
     else
        %objID = containerSearchNext();
   }
   return false;  //didn't find a qualifying gen within required radius of victim's point of death 
}

function CTFGame::testCarrierKill(%game, %victimID, %killerID)
{
   %flag = %victimID.plyrDiedHoldingFlag;
   return ((%flag !$= "") && (%flag.team == %killerID.team));  
}

function CTFGame::testEscortAssist(%game, %victimID, %killerID)
{
   return (%victimID.dmgdFlagCarrier); 
}

function CTFGame::testGenDestroyed(%game, %obj)
{
    return (%obj.lastDamagedByTeam != %obj.team);
}

function CTFGame::testValidRepair(%game, %obj)
{
   return ((%obj.lastDamagedByTeam != %obj.team) && (%obj.repairedBy.team == %obj.team));
}  

function CTFGame::awardScoreFlagCap(%game, %cl, %flag)
{
   %cl.flagCaps++;
   $TeamScore[%cl.team] += %game.SCORE_PER_TEAM_FLAG_CAP;
   messageAll('MsgTeamScoreIs', "", %cl.team, $TeamScore[%cl.team]);

   if (%game.SCORE_PER_TEAM_FLAG_CAP > 0)
   {
      %plural = (%game.SCORE_PER_PLYR_FLAG_CAP != 1 ? 's' : "");
      messageClient(%cl, 'msgCTFFriendCap', '\c0You receive %1 point%2 for capturing the enemy flag!', %game.SCORE_PER_PLYR_FLAG_CAP, %plural);
      messageTeamExcept(%cl, 'msgCTFFriendCap', '\c0%1 receives %2 point%3 for capturing the enemy flag!', %cl.name, %game.SCORE_PER_PLYR_FLAG_CAP, %plural); 
      messageTeam(%flag.team, 'msgCTFEnemyCap', '\c0Enemy %1 received %2 point%3 for capturing your flag!', %cl.name, %game.SCORE_PER_PLYR_FLAG_CAP, %plural);
   }
   %game.recalcScore(%cl);
   %game.checkScoreLimit(%cl.team);
}

function CTFGame::checkScoreLimit(%game, %team)
{
   %scoreLimit = MissionGroup.CTF_scoreLimit;
   // default of 5 if scoreLimit not defined
   if(%scoreLimit $= "")
      %scoreLimit = 5;
   if($TeamScore[%team] >= %scoreLimit) 
      %game.scoreLimitReached();
}

function CTFGame::awardScoreFlagReturn(%game, %cl)
{
   %cl.flagReturns++;
   if (%game.SCORE_PER_FLAG_RETURN != 0)
   {
      messageClient(%cl, 'scoreFlaRetMsg', '\c0You received a %1 point bonus for returning your flag.', %game.SCORE_PER_FLAG_RETURN);
      messageTeamExcept(%cl, 'scoreFlaRetMsg', '\c0Teammate %1 received a %2 point bonus for returning your flag.', %cl.name, %game.SCORE_PER_FLAG_RETURN);
   }
   %game.recalcScore(%cl);
}

function CTFGame::awardScoreGenDestroy(%game,%cl)
{
   %cl.genDestroys++;
   if (%game.SCORE_PER_GEN_DESTROY != 0)
      {
         messageClient(%cl, 'msgGenDes', '\c0You received a %1 point bonus for destroying an enemy generator.', %game.SCORE_PER_GEN_DESTROY);
         messageTeamExcept(%cl, 'msgGenDes', '\c0Teammate %1 received a %2 point bonus for destroying an enemy generator.', %cl.name, %game.SCORE_PER_GEN_DESTROY);
      }
      %game.recalcScore(%cl);
}

function CTFGame::awardScoreGenRepair(%game, %cl)
{
   %cl.genRepairs++;
   if (%game.SCORE_PER_GEN_REPAIR != 0)
   {
      messageClient(%cl, 'msgGenRep', '\c0You received a %1 point bonus for repairing a generator.', %game.SCORE_PER_GEN_REPAIR);
      messageTeamExcept(%cl, 'msgGenRep', '\c0Teammate %1 received a %2 point bonus for repairing a generator.', %cl.name, %game.SCORE_PER_GEN_REPAIR);
   }
   %game.recalcScore(%cl);
}

function CTFGame::awardScoreGenDefend(%game, %killerID)
{
   %killerID.genDefends++;
   if (%game.SCORE_PER_GEN_DEFEND != 0)
   {
      messageClient(%killerID, 'msgGenDef', '\c0You received a %1 point bonus for defending a generator.', %game.SCORE_PER_GEN_DEFEND);
      messageTeamExcept(%killerID, 'msgGenDef', '\c0Teammate %1 received a %2 point bonus for defending a generator.', %killerID.name, %game.SCORE_PER_GEN_DEFEND);
   }
   %game.recalcScore(%cl);
}

function CTFGame::awardScoreCarrierKill(%game, %killerID)
{
   %killerID.carrierKills++;
   if (%game.SCORE_PER_CARRIER_KILL != 0)
   {
      messageClient(%killerID, 'msgCarKill', '\c0You received a %1 point bonus for stopping the enemy flag carrier!', %game.SCORE_PER_CARRIER_KILL);
      messageTeamExcept(%killerID, 'msgCarKill', '\c0Teammate %1 received a %2 point bonus for stopping the enemy flag carrier!', %killerID.name, %game.SCORE_PER_CARRIER_KILL);
   }
   %game.recalcScore(%killerID);   
}

function CTFGame::awardScoreFlagDefend(%game, %killerID)
{
   %killerID.flagDefends++;
   if (%game.SCORE_PER_FLAG_DEFEND != 0)
   {
      messageClient(%killerID, 'msgFlagDef', '\c0You received a %1 point bonus for defending your flag!', %game.SCORE_PER_FLAG_DEFEND);
      messageTeamExcept(%killerID, 'msgFlagDef', '\c0Teammate %1 received a %2 point bonus for defending your flag!', %killerID.name, %game.SCORE_PER_FLAG_DEFEND);
   }      
   %game.recalcScore(%killerID);
}

function CTFGame::awardScoreEscortAssist(%game, %killerID)
{
   %killerID.escortAssists++;
   if (%game.SCORE_PER_ESCORT_ASSIST != 0)
   {
      messageClient(%killerID, 'msgEscAsst', '\c0You received a %1 point bonus for protecting the flag carrier!', %game.SCORE_PER_ESCORT_ASSIST);
      messageTeamExcept(%killerID, 'msgEscAsst', '\c0Teammate %1 received a %2 point bonus for protecting the flag carrier!', %killerID.name, %game.SCORE_PER_ESCORT_ASSIST);
   }
   %game.recalcScore(%killerID);
}

function CTFGame::resetScore(%game, %client)
{
   %client.offenseScore = 0;
   %client.kills = 0;
   %client.deaths = 0;
   %client.suicides = 0;
   %client.escortAssists = 0;
   %client.teamKills = 0;
   %client.flagCaps = 0;
   %client.genDestroys = 0;

   %client.defenseScore = 0;
   %client.genDefends = 0;
   %client.carrierKills = 0;
   %client.escortAssists = 0;
   %client.turretKills = 0;
   %client.flagReturns = 0;
   %client.genRepairs = 0;


   %client.score = 0;
}

function CTFGame::genOnRepaired(%game, %obj, %objName)
{
      
   if (%game.testValidRepair(%obj))
   {
      %repairman = %obj.repairedBy;
      messageTeam(%repairman.team, 'msgGenRepaired', '\c0%1 repaired the %2 Generator!', %repairman.name, %obj.nameTag);
      %game.awardScoreGenRepair(%obj.repairedBy);
   }           
}

function CTFGame::stationOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj)) 
   {     
      %repairman = %obj.repairedBy;
      messageTeam(%repairman.team, 'msgStationRepaired', '\c0%1 repaired the %2 Inventory Station!', %repairman.name, %obj.nameTag);
   }
}

function CTFGame::sensorOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj)) 
   {     
      %repairman = %obj.repairedBy;
      messageTeam(%repairman.team, 'msgSensorRepaired', '\c0%1 repaired the %2 Pulse Sensor!', %repairman.name, %obj.nameTag);
   }
}

function CTFGame::turretOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj)) 
   {     
      %repairman = %obj.repairedBy;
      messageTeam(%repairman.team, 'msgTurretRepaired', '\c0%1 repaired the %2 Turret!', %repairman.name, %obj.nameTag);
   }
}

function CTFGame::vStationOnRepaired(%game, %obj, %objName)
{
   if (%game.testValidRepair(%obj)) 
   {     
      %repairman = %obj.repairedBy;
      messageTeam(%repairman.team, 'msgTurretRepaired', '\c0%1 repaired the %2 Vehicle Station!', %repairman.name, %obj.nameTag);
   }
}

function CTFGame::enterMissionArea(%game, %playerData, %player)
{
   %player.client.outOfBounds = false; 
   messageClient(%player.client, 'EnterMissionArea', '\c1You are back in the mission area.');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") entered mission area");

   //the instant a player leaves the mission boundary, the flag is dropped, and the return is scheduled...
   if (%player.holdingFlag > 0)
   {
      cancel($FlagReturnTimer[%player.holdingFlag]);
      $FlagReturnTimer[%player.holdingFlag] = "";
   }
}

function CTFGame::leaveMissionArea(%game, %playerData, %player)
{
   // maybe we'll do this just in case
   %player.client.outOfBounds = true;
   // if the player is holding a flag, strip it and throw it back into the mission area
   // otherwise, just print a message
   if(%player.holdingFlag > 0)
      %game.boundaryLoseFlag(%player);
   else
      messageClient(%player.client, 'MsgLeaveMissionArea', '\c1You have left the mission area.~wfx/misc/warning_beep.wav');
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") left mission area");
}

function CTFGame::boundaryLoseFlag(%game, %player)
{
   // this is called when a player goes out of the mission area while holding
   // the enemy flag. - make sure the player is still out of bounds
   if (!%player.client.outOfBounds || !isObject(%player.holdingFlag))
      return;

   %client = %player.client;
   %flag = %player.holdingFlag;
   %flag.setVelocity("0 0 0");
   %flag.setTransform(%player.getWorldBoxCenter());
   %flag.setCollisionTimeout(%player);

   %game.playerDroppedFlag(%player);

   // now for the tricky part -- throwing the flag back into the mission area
   // let's try throwing it back towards its "home"
   %home = %flag.originalPosition;
   %vecx =  firstWord(%home) - firstWord(%player.getWorldBoxCenter());
   %vecy = getWord(%home, 1) - getWord(%player.getWorldBoxCenter(), 1);
   %vecz = getWord(%home, 2) - getWord(%player.getWorldBoxCenter(), 2);
   %vec = %vecx SPC %vecy SPC %vecz;

   // normalize the vector, scale it, and add an extra "upwards" component
   %vecNorm = VectorNormalize(%vec);
   %vec = VectorScale(%vecNorm, 1500);
   %vec = vectorAdd(%vec, "0 0 500");

   // apply the impulse to the flag object
   %flag.applyImpulse(%player.getWorldBoxCenter(), %vec);

   //don't forget to send the message
   messageClient(%player.client, 'MsgCTFFlagDropped', '\c1You have left the mission area and lost the flag.~wfx/misc/flag_drop.wav', 0, 0, %player.holdingFlag.team);
   logEcho(%player.client.nameBase@" (pl "@%player@"/cl "@%player.client@") lost flag (out of bounds)");
}

function CTFGame::dropFlag(%game, %player)
{
   if(%player.holdingFlag > 0)
   {
      if (!%player.client.outOfBounds)
         %player.throwObject(%player.holdingFlag);
      else
         %game.boundaryLoseFlag(%player);
   }
}

function CTFGame::applyConcussion(%game, %player)
{
   %game.dropFlag( %player );
}

