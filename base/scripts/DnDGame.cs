// DisplayName = Defend and Destroy

//--- GAME RULES BEGIN ---
//Whoever blows up the other team's crap first wins!
//W00T!
//--- GAME RULES END ---

// Defend and Destroy type script for TRIBES 2
//
// You should know how this works by now, boys and girls -- it's "classic"
// TRIBES-style D&D! Whoever blows up the other team's crap first wins!
// W00T!
//
// Objects will be identified as "Objectives" by a property called scoreValue
// being non-zero.
//
// Like Siege, this mission type doesn't have a scoreLimit because it doesn't need one.



package DnDGame {

function StaticShapeData::onDisabled(%data, %obj, %prevstate)
{
   Parent::onDisabled(%data, %obj, %prevState);
	%team = %obj.team;
	if(%obj.scoreValue && !%obj.objectiveCompleted)
	{
		%obj.objectiveCompleted = true;
		$objDestroyed[%team]++;
		%game.scoreObjective(%obj, %obj.lastDamagedBy);
		%game.updateObjectives();
		%game.checkObjectives();
		//echo("Objective #" @ %obj @ "(" @ %obj.getClassName() @ ") was disabled by " @ %obj.lastDamagedBy);
	}
}

function StaticShapeData::objectiveInit(%data, %object)
{
	if(%object.scoreValue) {
		%team = %object.team;
		//error(%team @ " : Found objective #" @ %object @ " (" @ %object.getClassName() @ ") worth " @ %object.scoreValue @ " points");
	   %object.objectiveCompleted = false;
		$numObjectives[%team]++;
		%objSet = nameToID("MissionCleanup/Objectives" @ %team);
		if(%objSet <= 0) {
			%objSet = new SimSet("Objectives" @ %team);
			MissionCleanup.add(%objSet);
		}
		%objSet.add(%object);
	}
}

function FlipFlop::objectiveInit(%data, %flipflop)
{
	// add this flipflop to missioncleanup (for HUD reasons)
	%team = %flipflop.team;
	$numObjectives[%team]++;
	%flipflopSet = nameToID("MissionCleanup/Objectives" @ %team);
	if(%flipflopSet <= 0) {
		%flipflopSet = new SimSet("Objectives" @ %team);
		MissionCleanup.add(%flipflopSet);
	}
	%flipflopSet.add(%flipflop);
	$flipflopStatus[%flipflop] = "<unclaimed>";
   %flipFlop.objectiveCompleted = false;
}

function FlipFlop::playerTouch(%data, %flipflop, %player)
{
   //echo("function DnDGame::playerTouchFlipFlop(Game "@Game@",%player "@%player@",%this "@%this@")");
	%client = %player.client;
	%claimName = %client.name;
	%flipName = %flipflop.Name;
	%flipTeam = %flipflop.team;

	if(%flipTeam $= %player.team)
		return;

	%flipflop.team = %player.team;
	%flipflop.objectiveCompleted = !(%flipflop.objectiveCompleted);
	$flipflopStatus[%flipflop] = $teamName[%client.team];
   messageAll('MsgDnDClaimFlipFlop', '%1 claimed %2 for %3.~wfx/misc/switch_taken.wav', %claimName, %flipName, $teamName[%client.team], %flipflop.number);

	// change the skin on the switch to claiming team's
	setTargetSkin(%flipflop.getTarget(), $teamSkin[%player.team]);

	%client.touches++;  
	Game.computeScore(%client);
	Game.computeTeamScore(%client.team);

   %flipflop.lastClaimedBy = %client; //Do I care?
   Game.updateObjectives();		
	Game.checkObjectives();
}

};


function DnDGame::missionLoadDone(%game)
{
   //default version sets up teams - must be called first...
   DefaultGame::missionLoadDone(%game);

	for(%i = 1; %i <= %game.numTeams; %i++)
		$objDisabled[%i] = 0;
}

function DnDGame::setUpTeams(%game)
{
	DefaultGame::setupTeams(%game);

	for(%i = 1; %i <= %game.numTeams; %i++) {
		// $numObjectives[N] is the number of objectives that team N possesses
		// (and must defend).  $objDestroyed[N] is how many of team N's objectives
		// have been destroyed.
		$numObjectives[%i] = 0;
		$objDestroyed[%i] = 0;
	}
}

function DnDGame::startMatch(%game)
{
	DefaultGame::startMatch(%game);
   // jff: team change
	//for(%i = 1; %i < %game.numTeams; %i++)
	//	error("Number of team #" @ %i @ " 's objectives: " @ $numObjectives[%i]);
}

function DnDGame::scoreObjective(%game, %object, %client)
{
	//echo("scoreObjective: object " @ %object @ ", client " @ %client);
	if(%object.team != %client.team) {
		messageAll('MsgDnDObjDisabled', '%1 disabled an enemy objective worth %2 points!', %client.name, %object.scoreValue, 0, %object.number);
		// if objective doesn't belong to the destroyer's team, positive points
		%client.objScore += %object.scoreValue;
		$objDisabled[%client.team]++;
	}
	else {
	   if(%client.sex $= "Male")
	      %tempGen = 'his';
		else
			%tempGen = 'her';
		messageAll('MsgDnDObjDisabled', '%1 disabled %2 own team\'s objective worth %3 points!', %client.name, %tempGen, %object.scoreValue, %object.number);
		// otherwise, score negative points
		%client.objScore -= %object.scoreValue;
	}
	%game.computeScore(%client);
	%game.computeTeamScore(%client.team);
}

function DnDGame::checkObjectives(%game)  
{
	%teamDone = 0;
	// check all the objectives for both teams
	for(%i = 1; %i <= %game.numTeams; %i++) {
		%objSet = nameToID("MissionCleanup/Objectives" @ %i);
		%numObjectives = %objSet.getCount();
		%numObjsCompleted = 0;
		for(%j = 0; %j < %numObjectives; %j++) {
			%curObj = %objSet.getObject(%j);
			if(%curObj.objectiveCompleted)
				%numObjsCompleted++;
		}
	   // if all objectives have been destroyed, set flag
	   // otherwise, just update the objective screen
		if(%numObjsCompleted == %numObjectives)
			%teamDone++;
		messageAll('MsgDnDTeamObjLeft', "", %i, (%numObjectives - %numObjsCompleted));
	}
	// if at least one team is done, go to allObjectivesDestroyed
	if(%teamDone)
		%game.allObjectivesDestroyed();
}

function DnDGame::allObjectivesDestroyed(%game)
{
	messageAll('MsgDnDAllObjDestr', 'All objectives have been disabled!');
	%game.gameOver();
   cycleMissions();
}

function DnDGame::timeLimitReached(%game)
{
	//messageAll('MsgDnDTimeLimitReached', 'Time is up!');
	%game.gameOver();
   cycleMissions();
}

//function DnDGame::scoreLimitReached(%game)
//{
//	%game.gameOver();
//}

function DnDGame::gameOver(%game)
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

	AIMissionEnd();
	messageAll('MsgClearObjHud', "");
	for(%i = 0; %i = %group.getCount(); %i ++) {
		%client = %group.getObject(%i);
		%game.resetScore(%client);
	}
   for ( %team = 1; %team <= %game.numTeams; %team++ )
      $TeamScore[%team] = 0;
}

function DnDGame::computeScore(%game, %client)
{
	%client.score = %client.scoreKills - %client.scoreSelfKills - %client.teamKills
		+ %client.objScore;
	%game.recalcTeamRanks(%client);
}

function DnDGame::computeTeamScore(%game, %team)
{
	$teamScore[%team] = $objDisabled[%team];
}

function DnDGame::updateObjectives(%game)
{
	//echo("Updating objectives...");
}

function DnDGame::clientMissionDropReady(%game, %client)
{
	messageClient(%client, 'MsgClientReady', "", %game.class);
	for(%i = 1; %i <= %game.numTeams; %i++)
		messageClient(%client, 'MsgDnDAddTeam', "", %i, $teamName[%i], $teamScore[%i]);
	%game.populateTeamRankArray(%client);
	messageClient(%client, 'MsgYourRankIs', "", -1);
	%numFlips = 0;
	for(%i = 1; %i <= %game.numTeams; %i++) {
		%numObj[%i] = 0;
		%objSet = nameToId("MissionCleanup/Objectives" @ %i);
		for(%j = 0; %j < %objSet.getCount(); %j++) {
			%curObj = %objSet.getObject(%j);
		}
		messageAll('MsgDnDTeamObjLeft', "", %i, %numObj[%i]);
	}

   	messageClient(%client, 'MsgMissionDropInfo', '\c0You are in mission %1 (%2).', $MissionDisplayName, $MissionTypeDisplayName, $ServerName ); 
	
	DefaultGame::clientMissionDropReady(%game, %client);
}

function DnDGame::resetScore(%game, %client)
{
	%client.score = 0;
	%client.scoreKills = 0;
	%client.scoreSelfKills = 0;
	%client.teamKills = 0;
	%client.objScore = 0;
}