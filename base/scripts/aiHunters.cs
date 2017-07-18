//-----------------------------------------------
// AI functions for Hunters
//---------------------------------------------------------------------------

// globals
$AIHuntersFlagSearchRadius     	= 300;
$AIHuntersCloseFlagDist        	= 40;
$AIHuntersAttackClientFlagCount	= 10;
$AIHuntersMinFlagsToCap				= 3;

//---------------------------------------------------------------------------

function HuntersGame::onAIRespawn(%game, %client)
{
   //add the default task
	if (! %client.defaultTasksAdded)
	{
		%client.defaultTasksAdded = true;
	   %client.addTask(AIEngageTask);
	   %client.addTask(AIPickupItemTask);                                             
	   %client.addTask(AIUseInventoryTask);
	   %client.addTask(AITauntCorpseTask);
		%client.addTask(AIEngageTurretTask);
		%client.addtask(AIDetectMineTask);
	   %client.addTask(AIPatrolTask);
      %client.huntersTask = %client.addTask(AIHuntersTask);
	}

   //set the inv flag
   %client.spawnUseInv = true;
}

//---------------------------------------------------------------------------
   
function HuntersGame::AIInit(%game)
{
   //call the default AIInit() function
   AIInit();
}

//---------------------------------------------------------------------------
//AIHuntersTask functions
//---------------------------------------------------------------------------
function AIHuntersTask::init(%task, %client)
{
}

//---------------------------------------------------------------------------

function AIHuntersTask::assume(%task, %client)
{
   %task.setWeightFreq(10);
   %task.setMonitorFreq(10);
   %task.pickupFlag = -1;
	%task.engageTarget = -1;
   %task.capFlags = false;
}

function AIHuntersTask::retire(%task, %client)
{
   %task.pickupFlag = -1;
	%task.engageTarget = -1;
   %task.capFlags = false;
}

//---------------------------------------------------------------------------

function AIHuntersTask::weight(%task, %client)
{
   // init flag search vars
   %player = %client.player;
	if (!isObject(%player))
		return;

   %clientPos = %player.getWorldBoxCenter();
   %task.pickupFlag = -1;
	%task.engageTarget = -1;
   %task.capFlags = false;

	//find the closest flag
   %flagVars = AIFindClosestFlag(%client, $AIHuntersFlagSearchRadius);
	%closestFlag = getWord(%flagVars, 0);
	%closestFlagDist = getWord(%flagVars, 1);

	//find the dist to the nexus
	%nexusPos = Game.Nexus.getId().getWorldBoxCenter();
	%nexusDist = %client.getPathDistance(%nexusPos);
	if (%nexusDist < 0)
		%nexusDist = 32767;

	//find the dist to the closest health
	%healthDist = 32767;
	%damage = %client.player.getDamagePercent();
	if (%damage > 0.7)
	{
		//search for a health kit
		%closestHealth = AIFindSafeItem(%client, "Health");
		if (%closestHealth > 0)
		{
			%healthDist = %client.getPathDistance(%closestHealth);
			if (%healthDist < 0)
				%healthDist = 32767;
		}

		//else search for an inventory station
		else
		{
			%result = AIFindClosestInventory(%client, false);
			%closestInv = getWord(%result, 0);
			%closestDist = getWord(%result, 1);
			if (%closestInv > 0)
				%healthDist = %closestDist;
		}
	}

	//see if we need to cap - make sure we're actually able first 
	%mustCap = false;
	%shouldCap = false;
	%numToScore = %client.flagCount - 1;
	%hoardModeOn = Game.hoardModeActive();
	if ((!Game.greedMode || %numToScore >= Game.greedMinFlags) && !Game.hoardModeActive() && %numToScore >= $AIHuntersMinFlagsToCap)
	{
		//find out how many points would be scored
      %potentialScore = (%numToScore * (%numToScore + 1)) / 2;

		//find out how many flags we need to take the lead...
		%needFlagsForLead = 0;
		%highestScore = 0;
		%clientIsInLead = false;
		if (Game.teamMode)
		{
			%teamCount = Game.numTeams;
			for (%i = 1; %i <= %teamCount; %i++)
			{
				if ($teamScore[%i] > %highestScore)
					%highestScore = $teamScore[%i];
			}

			//see if we're in the lead...
			if (%highestScore == $teamScore[%client.team])
				%clientIsInLead = true;
			else
			{
				%tempScore = $teamScore[%client.team] + %potentialScore;
				%flagValue = %numToScore + 1;
				while (%tempScore < %highestScore)
				{
					%tempScore += %flagValue;
					%flagValue++;
					%needFlagsForLead++;
				}
			}
		}
		else
		{
			%clientCount = ClientGroup.getCount();
			for (%i = 0; %i < %clientCount; %i++)
			{
				%cl = ClientGroup.getObject(%i);
				if (%cl.score > %highestScore)
					%highestScore = %cl.score;
			}

			//see if we're in the lead
			if (%highestScore == %client.score)
				%clientIsInLead = true;
			else
			{
				%tempScore = %client.score + %potentialScore;
				%flagValue = %numToScore + 1;
				while (%tempScore < %highestScore)
				{
					%tempScore += %flagValue;
					%flagValue++;
					%needFlagsForLead++;
				}
			}
		}

		//the current target is more dangerous than the closest enemy
		%currentTarget = %client.getEngageTarget();
		if (AIClientIsAlive(%currentTarget))
		{
			%closestEnemy = %currentTarget;
		   %closestEnemydist = %client.getPathDistance(%currentTarget.player.position);
			if (%closestEnemyDist < 0)
				%closestEnemyDist = 32767;
		}
		else
		{
         %losTimeout = $AIClientMinLOSTime + ($AIClientLOSTimeout * %client.getSkillLevel());
			%result = AIFindClosestEnemy(%client, $AIHuntersCloseFlagDist, %losTimeout);
	      %closestEnemy = getWord(%result, 0);
		   %closestEnemydist = getWord(%result, 1);
		}

		//find out how much time is left...
	 	%curTimeLeftMS = ($Host::TimeLimit * 60 * 1000) + $missionStartTime - getSimTime();

		//If there's a tough or equal enemy nearby, or no flags, think about capping
		//ie.  never cap if there are flags nearby and no enemies...
		if ((AIClientIsAlive(%closestEnemy) && AIEngageWhoWillWin(%closestEnemy, %client) != %client) ||
			(!isObject(%closestFlag) || %closestFlagDist > $AIHuntersCloseFlagDist))
		{
			//if we've got enough to take the lead, and there are no flags in the vicinity
			if ((!%clientIsInLead && %needFlagsForLead == 0) || %highestScore == 0)
				%mustCap = true;

			//else if we're about to get our butt kicked and we're much closer to the nexus than to health...
			else if ((AIClientIsAlive(%closestEnemy) && AIEngageWhoWillWin(%closestEnemy, %client) == %closestEnemy) &&
																												(%healthDist - %nexusDist > 100))
			{
				%mustCap = true;
			}

			//else if there's no time left in the mission, cap whatever we've got now...
			else if (%curTimeLeftMS <= 30000)
				%mustCap = true;

			//else we don't need to cap - see if we should to play it smart
			else
			{
				//if we'd need more than just a couple to take the lead...
				%waitForFlagsTolerance = %client.getSkillLevel() * $AIHuntersMinFlagsToCap * 2;
				if (%needFlagsForLead == 0 || (%needFlagsForLead > %waitForFlagsTolerance))
				{
					%numEnemyFlags = 0;
					%clientCount = ClientGroup.getCount();
					for (%i = 0; %i < %clientCount; %i++)
					{
						%cl = ClientGroup.getObject(%i);
						if (%cl != %client && %cl.flagCount > %numEnemyFlags)
							%numEnemyFlags = %cl.flagCount;
					}

					//if we're in the lead, or no one has the flags we need, or it's team mode,
					//decide whether to cap based on skill level
					if (%needFlagsForLead == 0 || %numEnemyFlags < %needFlagsForLead || Game.teamMode)
					{
						if (%numToScore >= $AIHuntersMinFlagsToCap + (%client.getSkillLevel() * %client.getSkillLevel() * 15))
							%shouldCap = true;
					}
				}
			}
		}

		//now that we've checked all the possibilities, see if we should or must cap
		if (%mustCap || %shouldCap)
		{
			if (%mustCap)
				%task.setWeight($AIHuntersWeightMustCap);
			else
				%task.setWeight($AIHuntersWeightShouldCap);

			%task.capFlags = true;
			return;
		}
	}

	//if we've made it this far, we either can't cap, or there's no need to cap...

	//if we're engaging someone that's going to kill us, return 0 and let the patrol take over...
	%currentTarget = %client.getEngageTarget();
	if ((AIClientIsAlive(%currentTarget) && AIEngageWhoWillWin(%currentTarget, %client) == %currentTarget) && %healthDist < 300)
	{
		%task.setWeight(0);
		return;
	}
	
	//find the closest player with the most flags (that we have los to)
   %losTimeout = $AIClientMinLOSTime + ($AIClientLOSTimeout * %client.getSkillLevel());
	%bestClientToEngage = findClientWithMostFlags(%client, %losTimeout);
	%bestClientDist = 32767;
	if (AIClientIsAlive(%bestClientToEngage))
	{
		%bestClientDist = %client.getPathDistance(%bestClientToEngage.player.position);
		if (%bestClientDist < 0)
			%bestClientDist = 32767;
	}

	//see if there's a flag
	if (isObject(%closestFlag))
	{
		//see if there's a client to shoot
		if (AIClientIsAlive(%bestClientToEngage))
		{
			//calc weight base on closesness to the nearest flag vs. and number of flags the client has...
			%engageDistFactor = 30 + %bestClientDist - (%bestClientToEngage.flagCount * 5);
			if (%closestFlagDist < %engageDistFactor)
			{
			   %task.pickupFlag = %closestFlag;
				%task.engageTarget = %bestClientToEngage;
				%task.setWeight($AIHuntersWeightPickupFlag);
			}

			//otherwise, ignore the flag, and go for the client
			else
			{
			   %task.pickupFlag = -1;
				%task.engageTarget = %bestClientToEngage;
				%task.setWeight($AIHuntersWeightMustEngage);
			}
		}

		//else no one to attack
		else
		{
		   %task.pickupFlag = %closestFlag;
			%task.engageTarget = -1;
			%task.setWeight($AIHuntersWeightPickupFlag);
		}
	}

	//else no flag, see if we have someone to attack
	else if (AIClientIsAlive(%bestClientToEngage))
	{
	   %task.pickupFlag = -1;
		%task.engageTarget = %bestClientToEngage;
		%task.setWeight($AIHuntersWeightShouldEngage);
	}

	//nothing hunter's related to do...
	else
	{
	   %task.pickupFlag = -1;
		%task.engageTarget = -1;
		%task.setWeight(0);
	}
}

//---------------------------------------------------------------------------

function AIHuntersTask::monitor(%task, %client)
{
   //see if we should cap
   if (%task.capFlags)
	{
		%nexusPos = Game.Nexus.getId().getWorldBoxCenter();
      %client.stepMove(%nexusPos, 0.25);
	}

	//see if we've got a flag to pick up and/or someone to engage
   else if (isObject(%task.pickupFlag))
   {   
      %client.stepMove(%task.pickupFlag.getWorldBoxCenter(), 0.25);

		if (AIClientIsAlive(%task.engageTarget))
			%client.setEngageTarget(%task.engageTarget);
   }

	//else see if there's just someone to engage
	else if (AIClientIsAlive(%task.engageTarget))
		%client.stepEngage(%task.engageTarget);
	
	//if we're not engaging someone related to the hunters task, engage whoever the AIEngageTask wants...
	else
		%client.setEngageTarget(%client.shouldEngage);
}

//---------------------------------------------------------------------------
// AIHunters utility functions
//---------------------------------------------------------------------------

function AIFindClosestFlag(%client, %radius)
{
   %closestFlag = -1;
	%closestDist = %radius;
	%flagCount = $FlagGroup.getCount();
	for (%i = 0; %i < %flagCount; %i++)
	{
		%flag = $FlagGroup.getObject(%i);
		%flagPos = %flag.getWorldBoxCenter();
		%dist = %client.getPathDistance(%flagPos);

		if (%dist > 0 && %dist < %closestDist)
		{
			%closestDist = %dist;
			%closestFlag = %flag;
		}
	}
	return %closestFlag @ " " @ %closestDist;
}

//---------------------------------------------------------------------------

function findClientWithMostFlags(%srcClient, %losTimeout)
{
   %clientCount = 0;
   %closestClient = -1;
   %highestFlagFactor = -1;		//take both distance and flag count into consideration

   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
		%cl = ClientGroup.getObject(%i);

		//make sure we find someone who's alive, and on an opposing team
		if (AIClientIsAlive(%cl) && %cl.team != %srcClient.team)
		{
			%clIsCloaked = false;
			if (%cl.player.getInventory("CloakingPack") > 0 && %cl.player.getImageState($BackpackSlot) $= "activate")
				%clIsCloaked = true;

			//make sure the client can see the enemy
			%hasLOS = %srcClient.hasLOSToClient(%cl);
			%losTime = %srcClient.getClientLOSTime(%cl);
			if (%hasLOS || (%losTime < %losTimeout && AIClientIsAlive(%cl, %losTime + 1000)))
			{
	         %testPos = %cl.player.getWorldBoxCenter();
	         %distance = %srcClient.getPathDistance(%testPos);
				if (%distance < 0)
					%distance = 32767;

				//calculate the flag factor
				%flagFactor = (100 - %distance) + (%cl.flagCount * 5);

				//see if it's the most suitable client...
	         if (%flagFactor > %highestFlagFactor && (!%clIsCloaked || %distance < 8))
	         {
	            %closestClient = %cl;
	            %highestFlagFactor = %flagFactor;
	         }
			}
      }
   }
   
   return %closestClient;
}

//---------------------------------------------------------------------------

function aih()
{
	exec("scripts/aiHunters.cs");
}

function aiHlist()
{
	$timescale = 0.01;
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (%cl.isAIControlled())
			error(%cl SPC getTaggedString(%cl.name) SPC "score:" SPC %cl.score SPC "flags:" SPC %cl.flagCount - 1 SPC "capFlags:" SPC %cl.huntersTask.capFlags);
	}
}

