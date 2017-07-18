function serverCmdStartNewVote(%client, %typeName, %actionMsg, %arg1, %arg2, %arg3, %arg4, %playerVote)
{
   if ( !%client.isAdmin || ( ( %arg1.isAdmin && ( %client != %arg1 ) ) ) )
   {
      %teamSpecific = false;
	   %gender = (%client.sex $= "Male" ? 'he' : 'she');
      if ( Game.scheduleVote $= "" ) 
      {
			//send a message to everyone about the vote...
         if (%playerVote)
	      {   
            if((%client.team != %arg1.team && Game.numTeams > 1) && %typeName !$= "VoteAdminPlayer")
            {
               messageClient(%client, '', "\c2Player votes must be team based.");
               return;
            }
            
            // kicking and banning are team specific
            if(%typeName $= "VoteKickPlayer" || %typeName $= "VoteBanPlayer")
            {
               if(%arg1.isSuperAdmin)
               {
                  messageClient(%client, '', '\c2You can not %1 %2, %3 is a Super Admin!', %actionMsg, %arg1.name, %gender);
                  return;
               }
               
               Game.kickClient = %arg1;
               %clientsVoting = 0;
               %teamSpecific = Game.numTeams > 1;
               if(%arg1.team != 0 && %teamSpecific)
               {   
                  for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
                  {
                     %cl = ClientGroup.getObject( %clientIndex );
            
                     if(%cl.team == %client.team)
                     {   
                        %clients[%clientsVoting++] = %clientIndex;
                        messageClient( %client, 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
                     }
                  }
               }
               else
                  messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
            }
            else
            {
               messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1.name); 
            }   
         }
         else if ( %typeName $= "VoteChangeMission" )
            messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 %3 (%4).', %client.name, %actionMsg, %arg1, %arg2 );
         else if (%arg1 !$= 0)
         {
				if (%arg2 !$= 0)
		      {   
               if(%typeName $= "VoteTournamentMode")
               {   
                  %admin = getAdmin();
                  if(%admin > 0)
                     messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 Tournament Mode (%3).', %client.name, %actionMsg, %arg1); 
                  else
                  {   
                     messageClient( %client, 'clientMsg', 'There must be a server admin to play in Tournament Mode.');
                     return; 
                  }
               }
               else
                  messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 %3 %4.', %client.name, %actionMsg, %arg1, %arg2); 
				
            }
            else
		         messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2 %3.', %client.name, %actionMsg, %arg1);
         }
			else
	         messageAll( 'VoteStarted', '\c2%1 initiated a vote to %2.', %client.name, %actionMsg); 

         // open the vote hud for all clients that will participate in this vote
         if(%teamSpecific)
         {
            for(%i = 0; %i < %clientsVoting; %i++)
            {   
               %cl = ClientGroup.getObject(%clients[%i]);
               messageClient(%cl, 'openVoteHud', "", %clientsVoting, ($Host::VotePassPercent / 100));    
            }
         }
         else
         {
            for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
            {
               %cl = ClientGroup.getObject( %clientIndex );
               messageClient(%cl, 'openVoteHud', "", $HostGamePlayerCount, ($Host::VotePassPercent / 100));    
            }
         }
         
         clearVotes();
         Game.voteType = %typeName;
         Game.scheduleVote = schedule( ($Host::VoteTime * 1000), 0, "calcVotes", %typeName, %arg1, %arg2, %arg3, %arg4 );
         %client.vote = true;
         messageAll('addYesVote', "");
         
         if(!%client.team == 0)
            clearBottomPrint(%client);
      }
      else
         messageClient( %client, 'voteAlreadyRunning', '\c2A vote is already in progress.' );	                       
   }
   else 
   {
      if ( Game.scheduleVote !$= "" && Game.voteType $= %typeName ) 
      {
         messageAll('closeVoteHud', "");
         cancel(Game.scheduleVote);
         Game.scheduleVote = "";
      }
      
      // if this is a superAdmin, don't kick or ban
      if(%arg1 != %client)
      {   
         if(!%arg1.isSuperAdmin)
            eval( "Game." @ %typeName @ "(true,\"" @ %arg1 @ "\",\"" @ %arg2 @ "\",\"" @ %arg3 @ "\",\"" @ %arg4 @ "\");" );
         else
            messageClient(%client, '', '\c2You can not %1 %2, %3 is a Super Admin!', %actionMsg, %arg1.name, %gender);
      }      
   }
}

function serverCmdSetPlayerVote(%client, %vote)
{
   // players can only vote once
   if(%client.vote $= "")
   {
      %client.vote = %vote;
      if(%client.vote == 1)
         messageAll('addYesVote', "");
      else
         messageAll('addNoVote', "");
   
      commandToClient(%client, 'voteSubmitted', %vote);
   }
}

function calcVotes(%typeName, %arg1, %arg2, %arg3, %arg4)
{
   if(%typeName $= "voteMatchStart")
      if($MatchStarted || $countdownStarted)
         return;
   
   for ( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) 
   {
      %cl = ClientGroup.getObject( %clientIndex );
      messageClient(%cl, 'closeVoteHud', "");
      
      if ( %cl.vote !$= "" ) 
      {
         if ( %cl.vote ) 
         {
            Game.votesFor[%cl.team]++;
            Game.totalVotesFor++;
         } 
         else 
         {
            Game.votesAgainst[%cl.team]++;
            Game.totalVotesAgainst++;
         }
      }
      else 
      {
         Game.votesNone[%cl.team]++;
         Game.totalVotesNone++;
      }
   }   
   eval( "Game." @ %typeName @ "(false,\"" @ %arg1 @ "\",\"" @ %arg2 @ "\",\"" @ %arg3 @ "\",\"" @ %arg4 @ "\");" );
   Game.scheduleVote = "";
   Game.kickClient = "";
}

function clearVotes()
{
   for(%clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++)
   {   
      %client = ClientGroup.getObject(%clientIndex);
      %client.vote = "";
      messageClient(%client, 'clearVoteHud', ""); 
   }
   
   for(%team = 1; %team < 5; %team++) 
   {
      Game.votesFor[%team] = 0;
      Game.votesAgainst[%team] = 0;
      Game.votesNone[%team] = 0;
      Game.totalVotesFor = 0;
      Game.totalVotesAgainst = 0;
      Game.totalVotesNone = 0;
   }
}

// Tournament mode Stuff-----------------------------------

function setModeFFA( %mission, %missionType )
{
   if( $Host::TournamentMode )
   {
      $TeamDamage = 1;
      $Host::TournamentMode = false;
      
      if( isObject( Game ) )
         Game.gameOver();
      
      loadMission(%mission, %missionType, false);   
   }
}

//------------------------------------------------------------------

function setModeTournament( %mission, %missionType )
{
   if( !$Host::TournamentMode )
   {
      $TeamDamage = 1;
      $Host::TournamentMode = true;
      
      if( isObject( Game ) )
         Game.gameOver();
         
      loadMission(%mission, %missionType, false);   
   }
}
