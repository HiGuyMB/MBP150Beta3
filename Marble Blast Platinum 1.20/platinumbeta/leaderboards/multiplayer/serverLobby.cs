//------------------------------------------------------------------------------
// Multiplayer Package
// serverLobby.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function GameConnection::loadLobby(%this) {
   if (%this.fake) return;

   if ($Server::ServerType $= "SinglePlayer")
      return;

   //HiGuy: Update scores
   serverLoadScores();

   //HiGuy: Send clients to the lobby
   commandToClient(%this, 'OpenLobby');

   %this.loadState = -1;

   //HiGuy: Send them stats about MPPlayMission
   %this.updatePlaymission();

   //HiGuy: Notify the crowd
   updatePlayerlist();

   //HiGuy: Preload if needed
   %this.readyState = 0;

   if ($Server::Preloading || $Server::Preloaded) {
      %this.preloadMission();
      commandToClient(%this, 'PreloadStart');
   }
}

function serverCmdSetMission(%client, %file, %row, %type) {
	if (%client.isHost()) {
		if ($Server::Dedicated) {
			if (!isFile(%file)) {
				//We don't have their file!
				commandToClient(%client, 'InvalidMission', %file);
				return;
			}

//			%file = MPGetMissionFile(%mission);
			%info = _MPGetMissionInfo(%file);

			$MP::MissionObj = %info;
			$MP::MissionFile = %info.file;
			$MP::MissionDesc = %info.desc;

			$LB::MissionType = %type;
			$MPPref::SelectedRow[%type] = %row;
		}
		reloadAllPlayMission();
	}
}

function serverCmdSetTeamMode(%client, %teammode) {
	if (%client.isHost()) {
		$MP::TeamMode = %teammode;
		updateTeams();
	}
}

function reloadAllPlaymission() {
   if ($Server::ServerType $= "SinglePlayer")
      return;
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      if (ClientGroup.getObject(%i).isReal())
         ClientGroup.getObject(%i).updatePlaymission();
   }
   updatePlayerList();
}

function GameConnection::updatePlaymission(%this) {
   if (%this.fake) return;
   if ($Server::Lobby) {
      //HiGuy: Basic mission info that is used for playmission
      commandToClient(%this, 'LobbyMissionInfo', $MP::MissionFile, $LB::MissionType, $MPPref::SelectedRow[$MP::MissionObj.type], $MP::MissionObj.name, $MP::MissionObj.artist, $MP::MissionObj.score[0], $MP::MissionObj.score[1], $MP::MissionObj.time, MPGetLevelBitmap($MP::MissionObj), $MP::MissionObj.platinumScore[0], $MP::MissionObj.ultimateScore[0], $MP::MissionObj.platinumScore[1], $MP::MissionObj.ultimateScore[1]);

      //HiGuy: Send over the mission desc
      %desc = $MP::MissionObj.desc;
      %maxLength = 255;

      //HiGuy: Let 'em know it's coming!
      commandToClient(%this, 'LobbyMissionDescStart');

      for (%i = 0; %i < mCeil(strLen(%desc) / %maxLength); %i ++) {
         commandToClient(%this, 'LobbyMissionDescPart', getSubStr(%desc, %maxLength * %i, %maxLength));
      }

      //HiGuy: Let 'em know we finished
      commandToClient(%this, 'LobbyMissionDescEnd');

      commandToClient(%this, 'LobbyFlags', $MPPref::Server::MatanMode);
   }
}

function serverCmdLobbyReturn(%client) {
	if (%client.isHost()) {
		commandToAll('CloseEndGame');
		lobbyReturn();
	}
}

function lobbyReturn() {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   HudMessageVector.clear();

   if (isObject(MusicPlayer))
      MusicPlayer.stop();

   commandToAll('clearBottomPrint');
   commandToAll('clearCenterPrint');

   commandToAll('finishSpectating');
	commandToAll('SpectateFull', false);

   endMission(true);
   resetSpawnGroup();
   deleteSpawnGroups();
   $Server::Lobby = true;
   $Game::Running = false;
   $missionRunning = false;

	//HiGuy: Reset peoples' states back to default when they finish loading
	for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
		%client = ClientGroup.getObject(%i);
   	%client.loading = false;
		%client.loadState = -1;
		%client.loadProgress = 0;
		%client.preloadPhase = -1;
		%client.readyState = -1;
	}

   commandToAll('OpenLobby');
   updatePlayerlist();
}

function serverCmdLobbyRestart(%client) {
	if (%client.isHost()) {
		commandToAll('CloseEndGame');
		echo("Canceling the game end timer");
		cancel($Game::EndTimer);
		lobbyRestart();
	}
}

function serverCmdRestartLevel(%client) {
	if (%client.isHost()) {
		commandToAll('CloseEndGame');
		echo("Canceling the game end timer");
		cancel($Game::EndTimer);
		restartLevel();

		if ($Server::Dedicated) {
	   	echo("Ending the game in" SPC MissionInfo.time + 5000 SPC "ms");
	   	$Game::EndTimer = schedule(MissionInfo.time + 5000, 0, "endGameSetup");
		}
	}
}

function lobbyRestart() {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   $Server::Started = false;
   setGameState("waiting");
	commandToAll('SpectateFull', false);

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;

      %client.startOverview();
      %client.setPregame(true);
      %client.stopTimer();
      %client.gemCount = 0;
      %client.setGemCount(0);
      %client.ready = false;
   }
}

//-----------------------------------------------------------------------------
// Player list
//
// Client Ready States:
// 0 - Lobby
// 1 - Preloading
// 2 - Ready for Start
//
// Client Load States:
// 0 - Loading
// 1 - Sending Files
// 2 - Confirming
// 3 - Ready
// 4 - Playing
//
//-----------------------------------------------------------------------------

function updatePlayerlist() {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   //HiGuy: Only once per frame, please!
   if (!$PlayerlistUpdate)
      schedule(0, 0, _updatePlayerList);
   $PlayerlistUpdate = true;

	cancel($MP::PlayerListSchedule);
	$MP::PlayerListSchedule = schedule(10000, 0, updatePlayerlist);
}

function _updatePlayerList() {
	$PlayerlistUpdate = false;
	if ($Server::Dedicated)
		refreshPlayerList();
	else
		commandToServer('refreshPlayerList');
}

function serverCmdRefreshPlayerList(%client) {
	if (!%client.isHost()) {
		%client.updatePlayerlist();
	} else {
		refreshPlayerList();
	}
}

function refreshPlayerList() {
	if ($Server::ServerType $= "SinglePlayer")
		return;

	//HiGuy: Update the lists of all the clients
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i ++) {
		%client = ClientGroup.getObject(%i);
		if (!%client.fake)
			%client.updatePlayerlist();
	}
	updateReadyUserList();
}

function serverCmdUpdatePlayerlist(%client) {
   //HiGuy: Punt this over to the GameConnection object (easier this way)
   %client.updatePlayerlist();
}

function GameConnection::updatePlayerlist(%this) {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   if (%this.fake) return;
   //HiGuy: Score list... Why not?
   %this.updateScores();
   //HiGuy: Send them the complete player list and ready states
   %this.playerlistSend ++;
   commandToClient(%this, 'PlayerlistStart', %this.playerlistSend, $Game::ClientIndex);

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;

      %state    = %client.readyState;
      %team     = Team::getTeamName(%client.team);
      %marble   = %client.skinChoice;
      %color    = Team::getTeamColor(%client.team);
      %ping     = %client.getPing();
      %provis   = %client.provisional;
      %rating   = %client.rating TAB %client.ratingGames TAB %client.ratingTeamGames TAB %client.ratingTotalGems TAB %client.ratingTotalScore TAB %client.ratingWinStreak;
      %level    = filebase($MP::MissionObj.file);
      %score    = %client.bestScore[%level] TAB %client.bestTeamScore[%level] TAB %client.lastScore;
      %handicap = %client.getHandicaps();
      %sstate   = (isRealClient(%client) ? (%client.spectating ? 2 : 1) : 0);
      commandToClient(%this, 'PlayerlistPlayer', %this.playerlistSend, %client.index, %client.nameBase, %state, %team, %marble, %client.loadProgress, %ping, %client.loadState, %color, %provis, %rating, %score, %handicap, %sstate);
   }

   commandToClient(%this, 'PlayerlistEnd', %this.playerlistSend);
}

function serverCmdMissionLoadProgress(%client, %progress, %state) {
   if (%state != %client.loadState) //HiGuy: Detach this!
      schedule(0, 0, updatePlayerlist);
   echo(%client.namebase SPC "progress:" SPC %progress SPC "state:" SPC %state);
   %client.loadProgress = %progress;
   %client.loadState = %state;
   %last = mFloor(%client.loadProgress * $MP::LoadSegments);

   if (%last == 0 != %client.lastProgress)
      commandToAllExcept(%client, 'ClientLoadProgress', %client.index, %last / $MP::LoadSegments, %state);
   %client.lastProgress = %last;

   if (%state == 2)
      commandToClient(%client, 'LoadFinish');
}
//-----------------------------------------------------------------------------
// Team Support

function GameConnection::updateTeam(%this) {
   if ($Server::ServerType $= "SinglePlayer")
      return;

	Team::fix();

   if (%this.fake) return;
   commandToClient(%this, 'TeamMode', $MP::TeamMode);
   if ($MP::TeamMode) {
      %team = %this.team;

      //HiGuy: Send them basic infos
      commandToClient(%this, 'TeamStatus', Team::isDefaultTeam(%team));
      commandToClient(%this, 'TeamName', Team::getTeamName(%team));
      %maxLength = 255;
      %descSend = Team::getTeamDescription(%team);
      commandToClient(%this, 'TeamDescStart');
      for (%i = 0; %i < mCeil(strLen(%descSend) / %maxLength); %i ++)
         commandToClient(%this, 'TeamDescPart', getSubStr(%descSend, %i * %maxLength, %maxLength));
      commandToClient(%this, 'TeamDescEnd');
      commandToClient(%this, 'TeamColor', Team::getTeamColor(%team));
      commandToClient(%this, 'TeamLeader', Team::getTeamLeader(%team));
      commandToClient(%this, 'TeamLeaderStatus', Team::isTeamLeader(%team, %this));
      commandToClient(%this, 'TeamRole', Team::getTeamRole(%team, %this));
      commandToClient(%this, 'TeamPrivate', Team::getTeamPrivate(%team));

      //HiGuy: Send them a team listing
      commandToClient(%this, 'TeamPlayerListStart');
      %count = %team.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %player = %team.getObject(%i);
         if (%player.fake)
            continue;
         commandToClient(%this, 'TeamPlayerListPlayer', %player.nameBase, Team::isTeamLeader(%team, %player), Team::getTeamRole(%team, %player));
      }
      commandToClient(%this, 'TeamPlayerListEnd');
   }
}

function serverCmdTeamList(%client) {
   %client.sendTeamList();
}

function GameConnection::sendTeamList(%this) {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   if (%this.fake) return;
   commandToClient(%this, 'TeamListStart');
   %count = TeamGroup.getCount();
   %used = "0\t0\t0\t0\t0\t0\t0\t0";
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);
      if (%team.color >= 0)
         %used = setField(%used, %team.color, 1);
      if (%team.permanent || !%team.private || %this.isHost())
         commandToClient(%this, 'TeamListTeam', Team::getTeamName(%team), %team.color);
   }
   commandToClient(%this, 'TeamListEnd');

   commandToClient(%this, 'TeamColorsUsed', %used);
}

function updateTeams() {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   //HiGuy: Only update once per frame call, we don't want any race conditions.
   // This lets any working team logic settle down (removing/adding players,
   // creating/deleting teams) before we update the peoples
   if (!$TeamsUpdate)
      schedule(0, 0, refreshTeams);
   $TeamsUpdate = true;
}

function refreshTeams() {
   if ($Server::ServerType $= "SinglePlayer")
      return;

   $TeamsUpdate = false;
   //HiGuy: Fix any non-teamed players

	Team::fix();

   %count = TeamGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);
      %tcount = %team.getCount();
      for (%j = 0; %j < %tcount; %j ++) {
         %client = %team.getObject(%j);
         if (%client.team $= "")
            %client.team = %team.getId();
      }
   }

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;
      if (%client.team $= "" || !isObject(%client.team))
         Team::addPlayer(Team::getDefaultTeam(), %client);
   }

   //HiGuy: Send the clients the public team list
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;
      %client.sendTeamList();
      %client.updateTeam();
   }

   updatePlayerlist();
}

function serverCmdTeamLeave(%client) {
   //HiGuy: You left a team, sending you back to the default team!
   Team::removePlayer(%client.team);
   Team::addPlayer(Team::getDefaultTeam(), %client);
   refreshTeams();
}

function serverCmdTeamDelete(%client) {
   //HiGuy: Why would you ever want to delete your team?
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Ohhhhhhhhhh nooooooooooo!
      Team::deleteTeam(%team);
      refreshTeams();
   }
}

function serverCmdTeamNameUpdate(%client, %name) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Send it to them
      Team::setTeamName(%team, %name);
      refreshTeams();
   }
}

function serverCmdTeamColorUpdate(%client, %color) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Send it to them
      Team::setTeamColor(%team, %color);
      refreshTeams();
   }
}

function serverCmdTeamDescUpdateStart(%client) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Prepare for desc update
      %team = Team::getTeam(%team);
      %team.descParts = 0;
   }
}

function serverCmdTeamDescUpdatePart(%client, %part) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Stash the part until we're good
      %team = Team::getTeam(%team);
      %team.descPart[%team.descParts] = %part;
      %team.descParts ++;
   }
}

function serverCmdTeamDescUpdateEnd(%client) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      %team = Team::getTeam(%team);
//      %team.dump();
      //HiGuy: And finish the send
      %descFinal = "";
      for (%i = 0; %i < %team.descParts; %i ++) {
         %descFinal = %descFinal @ %team.descPart[%i];
         %team.descPart[%i] = "";
      }
      //HiGuy: If this is too long, echo() will crash D:
      if (strLen(%descFinal) > $MP::TeamDescMaxLength)
         %descFinal = getSubStr(%descFinal, 0, $MP::TeamDescMaxLength);
      %team.descParts = "";
      Team::setTeamDescription(%team, %descFinal);
   }
}

function serverCmdTeamPrivateUpdate(%client, %private) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client) && !Team::isDefaultTeam(%team)) {
      //HiGuy: Send it to them
      Team::setTeamPrivate(%team, %private);
   }
}

function serverCmdTeamInfo(%client, %team) {
   %team = Team::getTeam(%team);
   if (%team == -1)
      return;

   //HiGuy: Send them basic infos
   commandToClient(%client, 'TeamInfoStatus', Team::isDefaultTeam(%team));
   commandToClient(%client, 'TeamInfoName', Team::getTeamName(%team));

   %maxLength = 255;
   %descSend = Team::getTeamDescription(%team);
   commandToClient(%client, 'TeamInfoDescStart');
   for (%i = 0; %i < mCeil(strLen(%descSend) / %maxLength); %i ++)
      commandToClient(%client, 'TeamInfoDescPart', getSubStr(%descSend, %i * %maxLength, %maxLength));
   commandToClient(%client, 'TeamInfoDescEnd');

   //HiGuy: Send them a team listing
   commandToClient(%client, 'TeamInfoPlayerListStart');
   %count = %team.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %player = %team.getObject(%i);
      commandToClient(%client, 'TeamInfoPlayerListPlayer', %player.nameBase, Team::isTeamLeader(%team, %player));
   }
   commandToClient(%client, 'TeamInfoPlayerListEnd');

   commandToClient(%client, 'TeamInfoEnd');
}

function serverCmdTeamJoin(%client, %team) {
   %team = Team::getTeam(%team);

   //HiGuy: No private joins!
   if (%team.private)
      return;

   Team::addPlayer(%team, %client);
   updatePlayerlist();
}

function serverCmdTeamCreate(%client, %name, %private, %color) {
   if (Team::createTeam(%name, %client, false, %private, $MP::NewTeamDesc, %color)) {
      updateTeams();
      updatePlayerlist();
      commandToClient(%client, 'TeamCreateSucceeded');
   } else {
      commandToClient(%client, 'TeamCreateFailed');
   }
}

function serverCmdTeamInvite(%client, %player) {
   %team = %client.team;
   if (Team::isTeamLeader(%team, %client)) {
      //HiGuy: Send it to them
      %recp = GameConnection::resolveName(%player);
      commandToClient(%recp, 'TeamInvite', %client.nameBase, Team::getTeamName(%team));

      //HiGuy: Store the invite
      %team.invite[%recp] = true;
   }
}

function serverCmdTeamInviteAccept(%client, %team) {
   %team = Team::getTeam(%team);

   //HiGuy: No private joins! Unless you're invited, that is.
   if (%team.private && !%team.invite[%client])
      return;

   //HiGuy: Clear this, they only get one invite
   %team.invite[%client] = false;

   Team::addPlayer(%team, %client);
   updatePlayerlist();

   //echo(%team.leader.namebase SPC "invited" SPC %client.namebase);
}

function serverCmdTeamInviteDecline(%client, %team) {
   %team = Team::getTeam(%team);

   //HiGuy: Clear this, they only get one invite
   %team.invite[%client] = false;

   commandToClient(%team.leader, 'TeamInviteDecline', %client.nameBase);
}

function serverCmdTeamPromote(%client, %player) {
   //HiGuy: Dunno why you would want to do this
   %team = %client.team;

   //HiGuy: Make sure only the leader can promote people :)
   if (Team::isTeamLeader(%team, %client)) {
      %recp = GameConnection::resolveName(%player);

      //HiGuy: They are the new leader!
      Team::setTeamLeader(%recp);
   }
}

function serverCmdTeamKick(%client, %player) {
   %team = %client.team;

   //HiGuy: Make sure only the leader can kick people
   if (Team::isTeamLeader(%team, %client)) {
      %recp = GameConnection::resolveName(%player);

      //HiGuy: Get the fuck off my team.
      Team::removePlayer(%recp);
   }
}

function updateTeamMode() {
   if ($Server::Hosting) {
      //HiGuy: Have to do some setting up / tearing down team support
      if ($MP::TeamMode) {
         //HiGuy: Add all players to the default team, and create it if none
         // exists.
         %defaultTeam =  Team::createDefaultTeam();

         %count = ClientGroup.getCount();
         for (%i = 0; %i < %count; %i ++) {
            %client = ClientGroup.getObject(%i);

            //HiGuy: If they have a saved team, don't take them out of it!
            if (%client.oldTeam) {
               if (Team::addPlayer(%client.oldTeam, %client))
                  continue;
            }

            //HiGuy: Otherwise, add them to the default team
            Team::addPlayer(%defaultTeam, %client);
            %client.updateTeam();
         }

         updateTeams();
      } else {
         //HiGuy: Team mode disabled.
         %count = ClientGroup.getCount();
         for (%i = 0; %i < %count; %i ++) {
            %client = ClientGroup.getObject(%i);
            %client.oldTeam = %client.team;
            %client.team = "";
            %client.updateTeam();
         }

         updateTeams();
      }
   }
   updatePlayerlist();
}

function serverLoadScores(%mission) {
	cancel($MP::ScoreSch);
	if (ClientGroup.getCount() == 0)
		return;
	$MP::ScoreSch = schedule(400, 0, loadScores, %mission);
}

function loadScores(%mission) {
	%server = $Master::Server;
	%page = $Master::Path @ "scores.php";
	%level = filebase(%mission);

	%data = "level=" @ %level;

	for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
		%player = ClientGroup.getObject(%i);
		%data = %data @ "&player[]=" @ %player.namebase;
	}

	if (!isObject(MasterScoreLoader))
		new TCPObject(MasterScoreLoader);
	MasterScoreLoader.post(%server, %page, %data);
	MasterScoreLoader.received = false;
}

function MasterScoreLoader::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   //HiGuy: Invalid lines
   if (strPos(%line, ":") == -1)
      return;

   %client = %this.client;

   //HiGuy: What does the master server want?
   %cmd  = getSubStr(%line, 0, strPos(%line, ":"));
   %rest = getSubStr(%line, strPos(%line, ":") + 1, strlen(%line));
   switch$ (%cmd) {
   case "ERR":
      //HiGuy: Uhhhhhhh. Shit
   case "SCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      //echo("Player" SPC %player SPC "has" SPC %score SPC "points on" SPC %level);

      %client = GameConnection::resolveName(%player);
      %client.bestScore[%level] = %score;
      //echo("Client is" SPC %client);
      //echo(%client.bestScore[%level] @ "=" @ %score);

      $Master::ServerScore[%level, %index, false, false] = %player TAB %score;
      commandToAll('MasterServerScore', %level, %index, %player, %score, false, false);
   case "PSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::ServerScore[%level, %index, true, false] = %player TAB %score;
      commandToAll('MasterServerScore', %level, %index, %player, %score, true, false);
   case "TOPSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::TopScore[%level, %index, false, false] = %player TAB %score;
      commandToAll('MasterTopScore', %level, %index, %player, %score, false, false);
   case "PTOPSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::TopScore[%level, %index, true, false] = %player TAB %score;
      commandToAll('MasterTopScore', %level, %index, %player, %score, true, false);
   case "TSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      //echo("Player" SPC %player SPC "has" SPC %score SPC "team points on" SPC %level);
      %client = GameConnection::resolveName(%player);
      %client.bestTeamScore[%level] = %score;

      $Master::ServerScore[%level, %index, false, true] = %player TAB %score;
      commandToAll('MasterServerScore', %level, %index, %player, %score, false, true);
   case "TPSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::ServerScore[%level, %index, true, true] = %player TAB %score;
      commandToAll('MasterServerScore', %level, %index, %player, %score, true, true);
   case "TTOPSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::TopScore[%level, %index, false, true] = %player TAB %score;
      commandToAll('MasterTopScore', %level, %index, %player, %score, false, true);
   case "TPTOPSCORE":
      %level  = getWord(%rest, 0);
      %player = decodeName(getWord(%rest, 1));
      %score  = getWord(%rest, 2);
      %index  = getWord(%rest, 3);

      $Master::TopScore[%level, %index, true, true] = %player TAB %score;
      commandToAll('MasterTopScore', %level, %index, %player, %score, true, true);
   case "SCORESSTART":
      deleteVariables("$Master::TopScore" @ fileBase($MP::MissionFile) @ "*");
      commandToAll('MasterLevelScores');
   case "SCORESDONE":
      updatePlayerList();
      commandToAll('MasterLevelScoresEnd');
   }
}

//-----------------------------------------------------------------------------
// Mission Preloading A.K.A the fun Section

function serverCmdLoadMission(%client, %file) {
	if (%client.isHost()) {
		//HiGuy: Let them know we're not in the lobby any more
      lobbyEnterGame();

      //HiGuy: Load the actual mission here
      $Server::MissionFile = %file;

      //HiGuy: Normal load sequence
      loadMission(%file);

		if (MPPlayMissionDlg.recording !$= "")
			loadSpawnGroup(MPPlayMissionDlg.recording);
	}
}

function serverCmdPreload(%client, %file) {
	if (%client.isHost())
		lobbyPreload(%file);
}

function lobbyPreload(%missionFile) {
   //HiGuy: Preloading the mission is a tricky process.
   // The basic routine is like this:
   //
   // Mission Selected for Preload
   // Exec the datablocks and mission file
   // For all clients, call:
   // setMissionCRC($missionCRC)
   // transmitDataBlocks($missionSequence)
   // transmitPaths()
   // activateGhosting()
   //
   // By that point, they should have everything they need.
   // The difficulty is calling all these :D
   // So we're going to have some fun here

   if (!isFile(%missionFile))
      return error("Could not preload mission: no mission file");

   //Check crc really quickly
   %base = fileBase(%missionFile);
   $MP::MissionPassed = !$CRC_NOPE;
	MPMissionSuperChecker(%base, %missionFile);

   if (!$MP::MissionPassed)
      return;

   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %client.ready = false;
   }
   pruneFakeClients();

   $Server::Preloading = true;
   $Server::Preloaded = false;
   $Server::PreloadedClients = 0;

   schedule(1000, 0, preloadContinue, %missionFile);
}

function preloadContinue(%missionFile) {
   if ($PreloadSequence $= "")
      $PreloadSequence = -1;
   $PreloadSequence ++;

   //Check crc really quickly
   %base = fileBase(%missionFile);
   $MP::MissionPassed = !$CRC_NOPE;
	MPMissionSuperChecker(%base, %missionFile);

   if (!$MP::MissionPassed)
      return;

   //HiGuy: Most of this comes from core/server/missionload.cs

   // Reset all of these
   clearCenterPrintAll();
   clearBottomPrintAll();
   clearServerPaths();

   $missionRunning = false;
   $Server::MissionFile = %missionFile;
   $Server::Started = false;
   $Editor::Opened = false;
   $Game::Running = false;

   $instantGroup = ServerGroup;
   $missionCRC = getFileCRC($Server::MissionFile);

   exec($Server::MissionFile);

   // If there was a problem with the load, let's try another mission
   if( !isObject(MissionGroup) ) {
      error( "No \'MissionGroup\' found in mission \"" @ $Server::MissionFile @ "\"." );
      return;
   }

   // Mission cleanup group
   new SimGroup( MissionCleanup );
   $instantGroup = MissionCleanup;

   // Construct MOD paths
   schedule(100, 0, pathOnMissionLoadDone);

   onMissionLoaded();
   purgeResources();

   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      //HiGuy: If we don't detach here, fake clients will be deleted on the
      // same frame and cause our index to be off.
      ClientGroup.getObject(%i).schedule(100, preloadMission);
   }

   updatePlayerlist();

   // Jeff: apply super classes!
   applySuperClass(ServerGroup);
}

function GameConnection::preloadMission(%this) {
   if (%this.fake) {
      //HiGuy: If we're preloading, you're too late!
      %this.delete();
      return;
   }
   if (isObject(%this.camera))
      %this.camera.delete();
   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();
   if (isObject(%this.mesh))
      %this.mesh.delete();

   %this.loadState = 0;
   %this.readyState = 1;
   %this.preloadPhase = 0;
   commandToClient(%this, 'PreloadPhase1', $PreloadSequence);
}

function serverCmdPreloadPhase1Ack(%client, %sequence) {
   //HiGuy: This can only happen under the right conditions
   if (%sequence != $PreloadSequence) {
      error("Client" SPC %client SPC "invalid preload phase 1 sequence:" SPC %sequence SPC "!=" SPC $PreloadSequence);
      return;
   }
   if (%client.preloadPhase != 0) {
      error("Client" SPC %client SPC "invalid preload phase 1 phase:" SPC %client.preloadPhase SPC "!= 0");
      return;
   }

   //HiGuy: Update their phase
   %client.preloadPhase = 1;

   //HiGuy: No inconsistencies here!
   %client.setMissionCRC($missionCRC);

   //HiGuy: Send em all the datablocks
   // Jeff: compatibility hack
   //       if the operating system of the client does not match the server
   //       as defined by the rules of function compareOS,
   //       then we have to transmit the datablocks manually
   if (compareOS(%client.platform))
      %client.transmitDataBlocks($PreloadSequence);
   else
   {
      // Jeff: if they got rejected and still managed too do this, well guess
      // what, we'll just inform them AGAIN
      %client.delete("The current server is not compatible with your computer platform.");
      return;

      // Jeff: we don't have compatible code yet, but use this if we ever do
      %count = DataBlockGroup.getCount();
      devecho("Transmitting" SPC %count SPC "DataBlocks via Remote Commands for Client" SPC %client.nameBase);
      devecho("   Reason: Server Platform is" SPC $platform SPC "while the Client Platform is" SPC %client.platform);
      commandToClient(%client, 'ReceiveSequence', $PreloadSequence, %count);
      %client.sendDataBlocks();
   }
}

function GameConnection::preloadDataBlocksDone(%this, %sequence) {
   //echo(%sequence @ "=" @ $PreloadSequence SPC %this.preloadPhase @ "=1");
   //HiGuy: This can only happen under the right conditions
   if (%sequence != $PreloadSequence) {
      error("Client" SPC %client SPC "invalid preload phase 1.5 sequence:" SPC %sequence SPC "!=" SPC $PreloadSequence);
      return;
   }
   if (%this.preloadPhase != 1) {
      error("Client" SPC %this SPC "invalid preload phase 1.5 phase:" SPC %this.preloadPhase SPC "!= 1");
      return;
   }

   //HiGuy: Update their phase
   %this.preloadPhase = 1.5;

   //HiGuy: Send em a notify
   commandToClient(%this, 'PreloadPhase2', $PreloadSequence);
}

function serverCmdPreloadPhase2Ack(%client, %sequence) {
   //HiGuy: This can only happen under the right conditions
   if (%sequence != $PreloadSequence) {
      error("Client" SPC %client SPC "invalid preload phase 2 sequence:" SPC %sequence SPC "!=" SPC $PreloadSequence);
      return;
   }
   if (%client.preloadPhase != 1.5) {
      error("Client" SPC %client SPC "invalid preload phase 2 phase:" SPC %client.preloadPhase SPC "!= 1.5");
      return;
   }

   //HiGuy: Update their phase
   %client.preloadPhase = 2;

   //HiGuy: I'MA SHOVEL PHASE 2 DOWN YOUR GODDAMN THROAT IF I HAVE TO
   // Wow. That got really serious really fast. Phase 2 up next: ghosting

   //HiGuy: Send them mod paths so they know what they need to ghost
   // Jeff: this isn't necessary................................
   %client.transmitPaths();

   // Jeff: if they have a different operating system
   // then they have to use the new ghosting method
   if (!compareOS(%client.platform))
   {
      ghostObjects(0, 0);
      commandToClient(%client, 'GhostObjectsDone');
   }
   else
   {
      //HiGuy: Switch flipped! Ghosting is now ON
      %client.activateGhosting();
   }
}

function GameConnection::preloadGhostAlwaysObjectsReceived(%this) {
   //HiGuy: Teeeeeeeechnically, they've finished phase 2
   // At this point, they should be completely preloaded

   //HiGuy: I don't know what to put here :(
   // ヽ༼ຈل͜ຈ༽ﾉ ʀᴀɪsᴇ ᴜʀ ᴅᴏɴɢᴇʀs ヽ༼ຈل͜ຈ༽ﾉ

   //On 8/14/13, at 2:20 PM, Aaron Youch wrote:
   // > Raise your pirates

   if (%this.fake) {
      //HiGuy: Fake people don't preload?
      error("Client" SPC %client SPC "invalid preload phase 2.5: is fake");
      return;
   }

   //HiGuy: This can only happen under the right conditions
   if (%this.preloadPhase != 2) {
      error("Client" SPC %this SPC "invalid preload phase 2.5 phase:" SPC %this.preloadPhase SPC "!= 2");
      return;
   }

   //HiGuy: Update their phase
   %this.preloadPhase = 3;

   %this.preloadDone();
}

function GameConnection::preloadDone(%this) {
   commandToClient(%this, 'PreloadFinish');
   updatePlayerlist();

   $Server::PreloadedClients ++;
	commandToClient(ClientGroup.getObject(0), 'PreloadProgress', $Server::PreloadedClients, getRealPlayerCount());

   if ($Server::PreloadedClients >= getRealPlayerCount()) {
      $Server::Preloading = false;
      $Server::Preloaded = true;

      commandToClient(ClientGroup.getObject(0), 'Preloaded');
   }

   %this.readyState = 2;
   $Game::Running = false;
}

function serverCmdPreloadSave(%client, %save) {
	if (%client.isHost()) {
		if (loadSpawnGroup(%save)) {
			//Let them ack that we loaded, then we start
			commandToClient(%client, 'PreloadSaveSuccess');
			$Hunt::Prerecorded = true;
		} else {
			//We don't have the file, or it's corrupted. Ask them for it!
			commandToClient(%client, 'PreloadSaveFailed', %save);
		}
	}
}

function serverCmdPreloadSaveStart(%client, %save) {
	if (%client.isHost()) {
		$Server::SpawnSaveData[%save] = "";
		commandToAll('ServerLoading');
	}
}

function serverCmdPreloadSavePart(%client, %save, %part) {
	if (%client.isHost()) {
		echo("part:" SPC %part);
		$Server::SpawnSaveData[%save] = $Server::SpawnSaveData[%save] @ %part;
	}
}

function serverCmdPreloadSaveDone(%client, %save) {
	if (%client.isHost()) {
		//We've finished
		fwrite(%save, $Server::SpawnSaveData[%save]);

		//Try to load the save

		//Update file references
		setModPaths($modpath);
		if (loadSpawnGroup(%save)) {
			//We're good!
			commandToClient(%client, 'PreloadSaveSuccess');
			$Hunt::Prerecorded = true;
		} else {
			//Oh well
			commandToClient(%client, 'PreloadSaveSadface', %save);
		}
	}
}

function serverCmdPreloadFinish(%client) {
	if (%client.isHost())
		preloadFinish();
}

function preloadFinish() {
   commandToAll('PreloadPhase3', $Server::MissionFile);
   lobbyEnterGame();
}

function lobbyEnterGame() {
   $Game::isHunt = $Server::ServerType $= "Multiplayer" && (MissionInfo.gameMode !$= "" && strPos(strLwr(MissionInfo.gameMode), "hunt") != -1);
   $Game::isFree = $Server::ServerType $= "Multiplayer" && (MissionInfo.gameMode !$= "" && strPos(strLwr(MissionInfo.gameMode), "free") != -1);

   // Start all the clients in the mission
   $Server::Lobby = false;
   $missionRunning = true;
   $Game::Running = true;

   //HiGuy: Spawn a gem group so the level doesn't appear strange
   hideGems();
   spawnHuntGemGroup();

   //HiGuy: Unset this so people don't load when we're at the lobby
   $Server::Preloaded = false;

   commandToAll('CloseLobby');
}

function serverCmdPreloadPhase3Ack(%client) {
   // Extract mission info from the mission file,
   // including the display name and stuff to send
   // to the client.
   buildLoadInfo( $Server::MissionFile );

   // Download mission info to the clients
   sendLoadInfoToClient(%client);
   %client.startMission();
   %client.onClientEnterGame();
}
