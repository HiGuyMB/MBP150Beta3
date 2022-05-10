//-----------------------------------------------------------------------------
// Multiplayer Package
// server.cs
// Copyright (c) 2013 MBP Team
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// The cool start function
function serverStartGame() {
   // Jeff: start the game
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %client.setPregame(false);

      if (%client.spectating) {
         %client.cancelOverview();
         %client.setSpectating(true);
      } else
         %client.stopOverview();
   }

   $Server::Started = true;

   resetSpawnGroup();
   updatePlayerlist();
   schedule($MPPref::OverviewFinishSpeed * 1010, 0, serverStartFinish);
}

function serverStartFinish() {
   //Finish starting things
   onMissionReset();
   restartLevel();

   // Jeff: update the score list
   updateScores();

   if ($Server::Dedicated) {
		echo("Canceling the game end timer");
		cancel($Game::EndTimer);

   	//HiGuy: Gotta start the timer
   	//Ready+Set+Go = 2000+1500+1500 = 5000
   	echo("Ending the game in" SPC MissionInfo.time + 5000 SPC "ms");
   	$Game::EndTimer = schedule(MissionInfo.time + 5000, 0, "endGameSetup");
   }
}

function GameConnection::setPregame(%this, %set) {
   if (%this.fake) return;
   commandToClient(%this, 'setPregame', %set);
   commandToClient(%this, 'SpectateFull', $Server::SpectateCount >= (getRealPlayerCount() - 1) && !%client.spectating);
}

//-----------------------------------------------------------------------------

// Jeff: update the pre game user list and ready list
function updateReadyUserList() {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   %list = "";
   $MP::ReadyCount = 0;
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (isRealClient(%client) && !%client.connected)
         continue;

      %name = LBResolveName(%client.nameBase, true);

      %px = 370;
      if ($MP::TeamMode)
         %px = 150;

      // Jeff: add on stuff to the list
      if (%client.isHost()) {
         %name = stripMLControlChars(clipPx("<font:Marker Felt:18>" @ %name, %px - 50, true));
         %name = %name SPC "[Host]";
      } else
         %name = clipPx(%name, %px, true);

      if (%client.ready) {
         $MP::ReadyCount ++;
         %status = "[Ready]";
      } else if (%client.loading)
         %status = "[Loading...]";
      else if (%client.fake) {
         %status = "[Disconnected]";
      } else
         %status = "[Waiting]";

      if ($MP::TeamMode) {
         %teamname = clipPx(Team::getTeamName(%client.team), 150, true);
         %color = Team::getTeamColor(%client.team);
         switch (%color) {
         case -1: %color = "\c1";
         case  0: %color = "\c2";
         case  1: %color = "\c3";
         case  2: %color = "\c4";
         case  3: %color = "\c5";
         case  4: %color = "\c6";
         case  5: %color = "\c7";
         case  6: %color = "\c8";
         case  7: %color = "\c9";
         }
         %teamname = "\cp" @ %color @ %teamname @ "\co";
      }
      %next = %name TAB %teamName TAB %client.rating TAB %client.bestScore[filebase($MP::MissionObj.file)] TAB %status;

      %list = (%list $= "") ? %next : %list NL %next;
   }

   // Jeff: update the userlist for pregame
   commandToAll('PreGameUserList', %list);
   messageAll('MsgReadyCount', "", $MP::ReadyCount, getRealPlayerCount());
}

//-----------------------------------------------------------------------------

// Jeff: determine if the server has moving platforms or not
function serverHasMovingPlatforms(%group) {
   %count = %group.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %obj = %group.getObject(%i);
      %class = %obj.getClassName();
      if (%class $= "SimGroup") {
         // Jeff: if we find it, stop execution
         if (serverHasMovingPlatforms(%obj))
            return true;
      } else if (%class $= "PathedInterior")
         return true;
   }
   return false;
}

//-----------------------------------------------------------------------------

// Jeff: out of bounds loop check, also does mouse fire for powerups and stuff
function MPOutofBounds() {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   cancel($MP::Schedule::OOB);

   if (!$Server::Hosting)
      return;

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.mouseFire && isObject(%client.player)) {
         if (%client.isOOB) {
            if (%client.checkpointed)
               %client.respawnOnCheckpoint();
            else
               %client.respawnPlayer();
         } else if (%client.player.powerUpId !$= "")
            %client.player.onPowerUpUsed();
      }
   }
   $MP::Schedule::OOB = schedule(50, 0, MPOutofBounds);
}

//-----------------------------------------------------------------------------

// Jeff: update score
//       this function is called when one of the following events happen:
// - Player joins the match
// - Player leaves the match
// - Match Starts
// - Match Ends
// - Player collects a Gem
// - Every 10 seconds to make sure (insanity check)
function updateScores() {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   cancel($MP::Schedule::Scores);

   for (%i = 0; %i < ClientGroup.getCount(); %i ++)
      if (isRealClient(ClientGroup.getObject(%i)))
         ClientGroup.getObject(%i).updateScores();

   $MP::Schedule::Scores = schedule(10000, 0, updateScores);
}

function GameConnection::updateScores(%this) {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   if (%this.fake)
      return;

   //HiGuy: The list is coming!
   commandToClient(%this, 'ScoreListStart', $MP::TeamMode);

   //HiGuy: We need to send different things if we're in different modes
   if ($MP::TeamMode) {
      //HiGuy: Send a team total, and then player scores for that team
      %count = TeamGroup.getCount();

      for (%i = 0; %i < %count; %i ++) {
         %team = TeamGroup.getObject(%i);
         %score = 0;

         //HiGuy: Calculate group score from all players
         %players = %team.getCount();
         for (%j = 0; %j < %players; %j ++)
            %score += %team.getObject(%j).gemCount;

         commandToClient(%this, 'ScoreListTeam', Team::getTeamName(%team), %score, %team.number, %team.color);

         //HiGuy: Send player scores from the team
         for (%j = 0; %j < %players; %j ++) {
            %client = %team.getObject(%j);
            %score = %client.gemCount;
            %skinChoice = %client.skinChoice;
            //echo("Skin choice is" SPC %skinChoice);
            commandToClient(%this, 'ScoreListTeamPlayer', Team::getTeamName(%team), %client.nameBase, %score, %client.index, %skinChoice);
         }
      }
   } else {
      //HiGuy: Simple send: each individual and score is sent
      %count = ClientGroup.getCount();

      for (%i = 0; %i < %count; %i ++) {
         %client = ClientGroup.getObject(%i);
         if (isRealClient(%client) && !%client.connected)
            continue;
         %score = %client.gemCount;
         commandToClient(%this, 'ScoreListPlayer', %client.nameBase, %score, %client.index, %client.skinChoice);
      }
   }

   //HiGuy: Let them know to update their displays
   commandToClient(%this, 'ScoreListEnd');
}

//HiGuy: Compare everyone's scores and get a client's current place in the game
function FakeGameConnection::getPlace(%this) {
   %place = 1;

   if ($MP::TeamMode) {
      //HiGuy: Calculate group score from all players
      %players = %this.team.getCount();
      for (%j = 0; %j < %players; %j ++)
         %teamscore += %this.team.getObject(%j).gemCount;

      for (%i = 0; %i < TeamGroup.getCount(); %i ++) {
         %team = TeamGroup.getObject(%i);

         //HiGuy: Calculate group score from all players
         %players = %team.getCount();
         for (%j = 0; %j < %players; %j ++)
            %score += %team.getObject(%j).gemCount;

         if (%team.getId() == %this.team.getId())
            continue;
         if (%score > %teamscore)
            %place ++;
      }
   } else {
      for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
         %player = ClientGroup.getObject(%i);
         if (%player.getId() == %this.getId())
            continue;
         if (%player.gemCount > %this.gemCount)
            %place ++;
      }
   }

   return %place;
}

//HiGuy: Compare everyone's scores and get a client's current place in the game
function GameConnection::getPlace(%this) {
   %place = 1;

   if ($MP::TeamMode) {
      //HiGuy: Calculate group score from all players
      %players = %this.team.getCount();
      for (%j = 0; %j < %players; %j ++)
         %teamscore += %this.team.getObject(%j).gemCount;

      for (%i = 0; %i < TeamGroup.getCount(); %i ++) {
         %team = TeamGroup.getObject(%i);

         //HiGuy: Calculate group score from all players
         %players = %team.getCount();
         for (%j = 0; %j < %players; %j ++)
            %score += %team.getObject(%j).gemCount;

         if (%team.getId() == %this.team.getId())
            continue;
         if (%score > %teamscore)
            %place ++;
      }
   } else {
      for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
         %player = ClientGroup.getObject(%i);
         if (%player.getId() == %this.getId())
            continue;
         if (%player.gemCount > %this.gemCount)
            %place ++;
      }
   }

   return %place;
}

function GameConnection::checkSpam(%this) {
	if (%this.getAddress() $= "local")
		return false;
   if (%this.spamming) {
      commandToClient(%this, 'PrivateMessage', LBChatColor("notification") @ "You have been muted for spamming. You will be unmuted in" SPC mRound(25 - (getRealTime() - %this.spamTime) / 1000) SPC "seconds.");
      return true;
   }
   %this.messages ++;
   %this.schedule(5000, "unmessage");
   if (%this.messages > 10 && !%this.isSuperAdmin) {
      %this.mute();
      return true;
   }
   return false;
}

function GameConnection::unmessage(%this) {
   %this.messages --;
}

function GameConnection::mute(%this) {
   %this.spamming = true;
   %this.spamTime = getRealTime();
   %this.schedule(25000, "unspam");
   commandToClient(%this, 'PrivateMessage', LBChatColor("notification") @ "You have been muted for spamming. You will be unmuted in 25 seconds.");
}

function GameConnection::unspam(%this) {
   %this.spamming = false;
   commandToClient(%this, 'PrivateMessage', LBChatColor("notification") @ "You have been unmuted.");
}

function GameConnection::getHandicaps(%this) {
   return !!(%this.disableGems[2])    << 0  |
          !!(%this.disableGems[5])    << 1  |
          !!(%this.disableCollision)  << 2  |
          !!(%this.disableDiagonal)   << 3  |
          !!(%this.disableJump)       << 4  |
          !!(%this.disableBlast)      << 5  |
          !!(%this.disablePowerup[1]) << 6  |
          !!(%this.disablePowerup[2]) << 7  |
          !!(%this.disablePowerup[5]) << 8  |
          !!(%this.disablePowerup[6]) << 9  |
          !!(%this.disableRadar)      << 10 |
          !!(%this.disableMarbles)    << 11;
}

function GameConnection::getHandicap(%this, %handicap) {
	switch (%handicap) {
	case 0:  return !!%this.disableGems[2];
	case 1:  return !!%this.disableGems[5];
	case 2:  return !!%this.disableCollision;
	case 3:  return !!%this.disableDiagonal;
	case 4:  return !!%this.disableJump;
	case 5:  return !!%this.disableBlast;
	case 6:  return !!%this.disablePowerup[1];
	case 7:  return !!%this.disablePowerup[2];
	case 8:  return !!%this.disablePowerup[5];
	case 9:  return !!%this.disablePowerup[6];
	case 10: return !!%this.disableRadar;
	case 11: return !!%this.disableMarbles;
	}
}

function GameConnection::setHandicaps(%this, %flags) {
   %this.disableGems[2]    = !!(%flags & (1 << 0));
   %this.disableGems[5]    = !!(%flags & (1 << 1));
   %this.disableCollision  = !!(%flags & (1 << 2));
   %this.disableDiagonal   = !!(%flags & (1 << 3));
   %this.disableJump       = !!(%flags & (1 << 4));
   %this.disableBlast      = !!(%flags & (1 << 5));
   %this.disablePowerup[1] = !!(%flags & (1 << 6));
   %this.disablePowerup[2] = !!(%flags & (1 << 7));
   %this.disablePowerup[5] = !!(%flags & (1 << 8));
   %this.disablePowerup[6] = !!(%flags & (1 << 9));
   %this.disableRadar      = !!(%flags & (1 << 10));
   %this.disableMarbles    = !!(%flags & (1 << 11));
   updatePlayerlist();
}

function GameConnection::setHandicap(%this, %handicap, %flag) {
	switch (%handicap) {
	case 0:  %this.disableGems[2]    = !!%flag;
	case 1:  %this.disableGems[5]    = !!%flag;
	case 2:  %this.disableCollision  = !!%flag;
	case 3:  %this.disableDiagonal   = !!%flag;
	case 4:  %this.disableJump       = !!%flag;
	case 5:  %this.disableBlast      = !!%flag;
	case 6:  %this.disablePowerup[1] = !!%flag;
	case 7:  %this.disablePowerup[2] = !!%flag;
	case 8:  %this.disablePowerup[5] = !!%flag;
	case 9:  %this.disablePowerup[6] = !!%flag;
	case 10: %this.disableRadar      = !!%flag;
	case 11: %this.disableMarbles    = !!%flag;
	}
   updatePlayerlist();
}

function serverGetHandicaps() {
   return !!($MPPref::Server::DisableGems2)     << 0  |
          !!($MPPref::Server::DisableGems5)     << 1  |
          !!($MPPref::Server::DisableCollision) << 2  |
          !!($MPPref::Server::DisableDiagonal)  << 3  |
          !!($MPPref::Server::DisableJump)      << 4  |
          !!($MPPref::Server::DisableBlast)     << 5  |
          !!($MPPref::Server::DisablePowerup1)  << 6  |
          !!($MPPref::Server::DisablePowerup2)  << 7  |
          !!($MPPref::Server::DisablePowerup5)  << 8  |
          !!($MPPref::Server::DisablePowerup6)  << 9  |
          !!($MPPref::Server::DisableRadar)     << 10 |
          !!($MPPref::Server::DisableMarbles)   << 11;
}

function serverGetHandicap(%flag) {
	switch (%flag) {
   case 0:  return !!$MPPref::Server::DisableGems2;
	case 1:  return !!$MPPref::Server::DisableGems5;
	case 2:  return !!$MPPref::Server::DisableCollision;
	case 3:  return !!$MPPref::Server::DisableDiagonal;
	case 4:  return !!$MPPref::Server::DisableJump;
	case 5:  return !!$MPPref::Server::DisableBlast;
	case 6:  return !!$MPPref::Server::DisablePowerup1;
	case 7:  return !!$MPPref::Server::DisablePowerup2;
	case 8:  return !!$MPPref::Server::DisablePowerup5;
	case 9:  return !!$MPPref::Server::DisablePowerup6;
	case 10: return !!$MPPref::Server::DisableRadar;
	case 11: return !!$MPPref::Server::DisableMarbles;
   }
}

function serverSetHandicaps(%flags) {
   $MPPref::Server::DisableGems2      = !!(%flags & (1 << 0));
   $MPPref::Server::DisableGems5      = !!(%flags & (1 << 1));
   $MPPref::Server::DisableCollision  = !!(%flags & (1 << 2));
   $MPPref::Server::DisableDiagonal   = !!(%flags & (1 << 3));
   $MPPref::Server::DisableJump       = !!(%flags & (1 << 4));
   $MPPref::Server::DisableBlast      = !!(%flags & (1 << 5));
   $MPPref::Server::DisablePowerup1   = !!(%flags & (1 << 6));
   $MPPref::Server::DisablePowerup2   = !!(%flags & (1 << 7));
   $MPPref::Server::DisablePowerup5   = !!(%flags & (1 << 8));
   $MPPref::Server::DisablePowerup6   = !!(%flags & (1 << 9));
   $MPPref::Server::DisableRadar      = !!(%flags & (1 << 10));
   $MPPref::Server::DisableMarbles    = !!(%flags & (1 << 11));
   commandToAll('ServerSetHandicaps', %flags);
   commandToAll('SetHandicaps', %flags);
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;

      %client.setHandicaps(%flags);
   }
   updatePlayerlist();
}

function serverSetHandicap(%number, %flag) {
   switch (%number) {
   case 0:  $MPPref::Server::DisableGems2     = !!%flag;
	case 1:  $MPPref::Server::DisableGems5     = !!%flag;
	case 2:  $MPPref::Server::DisableCollision = !!%flag;
	case 3:  $MPPref::Server::DisableDiagonal  = !!%flag;
	case 4:  $MPPref::Server::DisableJump      = !!%flag;
	case 5:  $MPPref::Server::DisableBlast     = !!%flag;
	case 6:  $MPPref::Server::DisablePowerup1  = !!%flag;
	case 7:  $MPPref::Server::DisablePowerup2  = !!%flag;
	case 8:  $MPPref::Server::DisablePowerup5  = !!%flag;
	case 9:  $MPPref::Server::DisablePowerup6  = !!%flag;
	case 10: $MPPref::Server::DisableRadar     = !!%flag;
	case 11: $MPPref::Server::DisableMarbles   = !!%flag;
   }
   commandToAll('ServerSetHandicap', %number, %flag);
   commandToAll('SetHandicap', %number, %flag);
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;

      %client.setHandicap(%number, %flag);
   }
   updatePlayerlist();
}

function serverResetScores() {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      $Master::ScoreRating[%i] = 0;
      $Master::ScoreChange[%i] = 0;
   }
}

function serverSendScores() {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   if ($MPPref::CalculateScores) {
      commandToAll('MasterScoreCount', $Master::Scores);
      for (%i = 0; %i < $Master::Scores; %i ++) {
         commandToAll('MasterScorePlayer', %i, $Master::ScorePlayer[%i]);
         commandToAll('MasterScoreRating', %i, $Master::ScoreRating[%i]);
         commandToAll('MasterScoreChange', %i, $Master::ScoreChange[%i]);
      }
      for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
         %client = ClientGroup.getObject(%i);
         commandToAll('PlayerGemCount', %client.namebase, %client.gemsFound[1], %client.gemsFound[2], %client.gemsFound[5]);
      }
   } else {
      commandToAll('MasterScoreCount', ClientGroup.getCount());
      for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
         %client = ClientGroup.getObject(%i);
         commandToAll('MasterScorePlayer', %i, %client.namebase);
         commandToAll('MasterScoreRating', %i, -1);
         commandToAll('MasterScoreChange', %i, 0);
         commandToAll('PlayerGemCount', %client.namebase, %client.gemsFound[1], %client.gemsFound[2], %client.gemsFound[5]);
      }
   }
   commandToAll('MasterScoreFinish');
}

function MPsendScores() {
   devecho("DOIN A MP SCORE SEND");
   updateScores();
   //HiGuy: Calculate scores!
   if ($MPPref::CalculateScores && !$Hunt::Prerecorded) {
      devecho("CALCIFY?");
      if (!masterCalcScores()) {
         devecho("MAYBE NOT");
         masterStartGame();
         $Master::CalcQueued = true;
      }
	}
   serverSendScores();
}
