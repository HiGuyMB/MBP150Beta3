//------------------------------------------------------------------------------
// Multiplayer Package
// clientLobby.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function clientCmdOpenLobby() {
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   $MP::Overview = false;
   if (!$Server::Hosting)
      MPPlayMissionDlg.showClient();
   else if (Canvas.getContent().getName() !$= "MPPlayMissionDlg")
      MPPlayMissionDlg.showHost();
}

function clientCmdLobbyMissionInfo(%file, %type, %row, %name, %artist, %score, %pscore, %time, %bitmap, %plat, %ult, %pplat, %pult) {
   if ($Server::Hosting || $Server::ServerType !$= "MultiPlayer")
      return;

   //echo(%file SPC %type SPC %row SPC %name SPC %artist SPC %score SPC %pscore SPC %time SPC %bitmap SPC %plat SPC %ult SPC %pplat SPC %pult);

   if ($LB::MissionType !$= %type)
      MPSetMissionType(%type);

   //HiGuy: Store these for later
   $MP::MissionFile     = %file;
   $MP::MissionRow      = %row;
   $MP::MissionName     = %name;
   $MP::MissionArtist   = %artist;
   $MP::MissionScore[0] = %score;
   $MP::MissionScore[1] = %pscore;
   $MP::MissionTime     = %time;
   $MP::MissionBitmap   = %bitmap;
   $MP::MissionPlat[0]  = %plat;
   $MP::MissionUlt[0]   = %ult;
   $MP::MissionPlat[1]  = %pplat;
   $MP::MissionUlt[1]   = %pult;

   //HiGuy: This will be sent soon after
   $MP::MissionInfo = "";

   //HiGuy: Update missionlist
   MPSetSelected2(%row, %name, %artist, %score, %pscore, %time, %bitmap, %plat, %ult, %pplat, %pult);
}

function clientCmdLobbyMissionDescStart() {
   if ($Server::Hosting || $Server::ServerType !$= "MultiPlayer")
      return;

   //HiGuy: Heere it comes!
   $MP::MissionDesc = "";
}

function clientCmdLobbyMissionDescPart(%part) {
   if ($Server::Hosting || $Server::ServerType !$= "MultiPlayer")
      return;

   $MP::MissionDesc = $MP::MissionDesc @ %part;
}

function clientCmdLobbyMissionDescEnd() {
   if ($Server::Hosting || $Server::ServerType !$= "MultiPlayer")
      return;

   //HiGuy: We've finished sending the description, update the screen
   MPSetSelected2($MP::MissionRow, $MP::MissionName, $MP::MissionArtist, $MP::MissionScore[0], $MP::MissionScore[1], $MP::MissionTime, $MP::MissionBitmap, $MP::MissionPlat[0], $MP::MissionUlt[0], $MP::MissionPlat[1], $MP::MissionUlt[1]);
}

function clientCmdLobbyFlags(%matan) {
   $MPPref::Server::MatanMode = %matan;
}

//-----------------------------------------------------------------------------
// Player list
//
// Client Ready States:
// 0 - Lobby
// 1 - Preloading
// 2 - Ready for Start
//
//-----------------------------------------------------------------------------

function clientCmdPlayerlistStart(%send, %maxIdx) {
   $MP::PlayerlistSend = %send;
   //HiGuy: Clear the list
   if (isObject(PlayerList))
      PlayerList.delete();
   Array("PlayerList");

   $MP::ClientIndexMax = %maxIdx;
}

function clientCmdPlayerlistPlayer(%send, %idx, %name, %state, %team, %marble, %progress, %ping, %state, %color, %provis, %rating, %score, %handicap, %sstate) {
   if (%send != $MP::PlayerlistSend)
      return;

   PlayerList.replaceEntryByIndex(%idx, %name NL %state NL %team NL %marble NL %progress NL %ping NL %state NL %color NL %provis NL %rating NL %score NL %handicap NL %sstate);
}

function clientCmdClientLoadProgress(%idx, %progress, %state) {
   if (%idx == $MP::ClientIndex)
      return;
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   if (!isObject(PlayerList))
      return;
   %entry = PlayerList.getEntry(%idx);
   %entry = setRecord(%entry, 4, %progress);
   %entry = setRecord(%entry, 6, %state);
   PlayerList.replaceEntryByIndex(%idx, %entry);

   if (%progress < $MP::LerpProgress[%idx]) {
      $MP::LerpProgress[%idx] = %progress;
      $MP::LerpStart[%idx] = %progress;
      $MP::LerpFinish[%idx] = %progress;
   }

   lobbyUpdatePlayerList();
   lerpLoadProgress();
}

function clientCmdPlayerlistEnd(%send) {
   if (%send != $MP::PlayerlistSend)
      return;

   lobbyUpdatePlayerList();

   if (MPPlayerSettingsDlg.isAwake()) {
      MPPlayerSettingsDlg.reset();
      MPPlayerSettingsDlg.load();
      MPPlayerSettingsDlg.layoutButtons();
   }
}

//100% ack-only function
function clientCmdPreloadSaveSuccess() {
	//Do the round-a-bout!
	commandToServer('PreloadFinish');

	$Hunt::Prerecorded = true;
}

function clientCmdPreloadSaveFailed(%save) {
	//Server doesn't have our save
	if (!$Server::_Dedicated) {
		//If it's not a dedicated sever, then we probably have the file. It's
		// probably just some random error in the save.
		commandToServer('PreloadFinish');
		schedule(1000, 0, MessageBoxOk, "Spawnsave load failure!", "Could not load the spawn save: loadSpawnGroup() failed!");
		return;
	}

	//We need to send the save to the server!
	%conts = fread(%save);

	commandToServer('PreloadSaveStart', %save);

	%maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%conts) / %maxChars); %i ++) {
		%part = getSubStr(%conts, %maxChars * %i , %maxChars);
      commandToServer('PreloadSavePart', %save, %part);
   }

   commandToServer('PreloadSaveDone', %save);
}

function clientCmdPreloadSaveSadface(%save) {
	if (!$Server::_Dedicated) {
		//Wut?
		return;
	}

	//Unfortunate. Just start the game then
	MessageBoxOk("Spawnsave send failure!", "Could not send the spawnsave to the server!", "commandToServer(\'PreloadFinish\');");
}

//-----------------------------------------------------------------------------
// Team Support

function clientCmdTeamMode(%teamMode) {
   $MP::TeamMode = %teamMode;
   MPPlayMissionDlg.updateTeams();
   if (!$MP::TeamMode) {
      MPTeamOptionsDlg.close();
      MPTeamSelectDlg.close();
      MPTeamCreateDlg.close();
      MPTeamJoinDlg.close();
   }
}

function clientCmdTeamStatus(%default) {
   $MP::TeamDefault = %default;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamName(%name) {
   $MP::TeamName = %name;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamDescStart() {
   //HiGuy: Prepare for desc update
   $MP::TeamDescParts = 0;
   $MP::TeamDesc = "";
}

function clientCmdTeamDescPart(%part) {
   $MP::TeamDescPart[$MP::TeamDescParts] = %part;
   $MP::TeamDescParts ++;
}

function clientCmdTeamDescEnd() {
   //HiGuy: And finish the send
   %descFinal = "";
   for (%i = 0; %i < $MP::TeamDescParts; %i ++) {
      %descFinal = %descFinal @ $MP::TeamDescPart[%i];
      $MP::TeamDescPart[%i] = "";
   }
   //HiGuy: If this is too long, echo() will crash D:
   if (strLen(%descFinal) > $MP::TeamDescMaxLength)
      %descFinal = getSubStr(%descFinal, 0, $MP::TeamDescMaxLength);
   $MP::TeamDescParts = "";
   $MP::TeamDesc = %descFinal;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamLeader(%leader) {
   $MP::TeamLeader = %leader;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamLeaderStatus(%leaderStatus) {
   $MP::TeamLeaderStatus = %leaderStatus;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamRole(%role) {
   $MP::TeamRole = %role;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamColor(%color) {
   $MP::TeamColor = %color;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamPrivate(%private) {
   $MP::TeamPrivate = %private;
   MPPlayMissionDlg.updateTeams();
}

function clientCmdTeamPlayerListStart() {
   //HiGuy: Reset / create TeamPlayerList
   if (isObject(TeamPlayerList))
      TeamPlayerList.delete();
   Array("TeamPlayerList");
}

function clientCmdTeamPlayerListEnd() {
   MPTeamSelectDlg.updateTeam();
}

function clientCmdTeamPlayerListPlayer(%playerName, %leader, %role) {
   //HiGuy: Insert the player
   if (!TeamPlayerList.containsEntry(%playerName))
      TeamPlayerList.addEntry(%playerName TAB %leader TAB %role);
}

function clientCmdTeamInfoStatus(%default) {
   $MP::TeamInfoDefault = %default;
   MPTeamJoinDlg.updateTeam();
}

function clientCmdTeamInfoName(%name) {
   $MP::TeamInfoName = %name;
   MPTeamJoinDlg.updateTeam();
}

function clientCmdTeamInfoDescStart() {
   //HiGuy: Prepare for desc update
   $MP::TeamInfoDescParts = 0;
   $MP::TeamInfoDesc = "";
}

function clientCmdTeamInfoDescPart(%part) {
   $MP::TeamInfoDescPart[$MP::TeamInfoDescParts] = %part;
   $MP::TeamInfoDescParts ++;
}

function clientCmdTeamInfoDescEnd() {
   //HiGuy: And finish the send
   %descFinal = "";
   for (%i = 0; %i < $MP::TeamInfoDescParts; %i ++) {
      %descFinal = %descFinal @ $MP::TeamInfoDescPart[%i];
      $MP::TeamInfoDescPart[%i] = "";
   }
   //HiGuy: If this is too long, echo() will crash D:
   if (strLen(%descFinal) > $MP::TeamDescMaxLength)
      %descFinal = getSubStr(%descFinal, 0, $MP::TeamDescMaxLength);
   $MP::TeamInfoDescParts = "";
   $MP::TeamInfoDesc = %descFinal;
   MPTeamJoinDlg.updateTeam();
}

function clientCmdTeamInfoPlayerListStart() {
   //HiGuy: Reset / create TeamPlayerList
   if (isObject(TeamInfoPlayerList))
      TeamInfoPlayerList.delete();
   Array("TeamInfoPlayerList");
}

function clientCmdTeamInfoPlayerListEnd() {
   MPTeamJoinDlg.updateTeam();
}

function clientCmdTeamInfoPlayerListPlayer(%playerName, %leader) {
   //HiGuy: Insert the player
   if (!TeamInfoPlayerList.containsEntry(%playerName))
      TeamInfoPlayerList.addEntry(%playerName TAB %leader);
}

function clientCmdTeamInfoEnd() {
   $MP::TeamInfoLoading = false;
   MPTeamJoinDlg.updateTeam();
}

function clientCmdTeamListStart() {
   //HiGuy: Reset / create TeamList
   if (isObject(TeamList))
      TeamList.delete();
   Array("TeamList");
}

function clientCmdTeamListTeam(%teamName, %color) {
   //HiGuy: Insert the new team
   if (!TeamList.containsRecord(%teamName, 0))
      TeamList.addEntry(%teamName NL %color);
}

function clientCmdTeamListEnd() {
   MPTeamJoinDlg.updateTeamList();
   MPTeamSelectDlg.updateTeam();
}

function clientCmdTeamColorsUsed(%used) {
   $MP::TeamColorsUsed = %used;
}

function clientCmdTeamCreateSucceeded() {
   MPTeamCreateDlg.close();
   MPTeamJoinDlg.close();
}

function clientCmdTeamCreateFailed() {
   MessageBoxOk("Team Creation Failed", "The team could not be created. Either another team with that name already exists, or there was an unknown error.", "", true);
}

function clientCmdTeamInvite(%player, %teamName) {
   MessageBoxYesNo("Team Invite", upperFirst(%player) SPC "has invited you to join team" SPC %teamName @ ". Press yes to join team" SPC %teamName @ ($MP::TeamDefault ? "." : " and to leave your current team."), "acceptTeamInvite(\"" @ %teamName @ "\");", "declineTeamInvite(\"" @ %teamName @ "\");");
}

function acceptTeamInvite(%teamName) {
   commandToServer('TeamInviteAccept', %teamName);
}

function declineTeamInvite(%teamName) {
   commandToServer('TeamInviteDecline', %teamName);
}

function clientCmdTeamInviteDecline(%player) {
   MessageBoxOk("Team Invitation Declined", upperFirst(%player) SPC "has declined your team invitation.", "", true);
}

function clientCmdMasterServerScore(%level, %index, %player, %score, %practice, %teams) {
   //HiGuy: Four-dimensional arrays. Yeah, I went there. Try visualizing a 4D
   // array in your mind. You can't. You know 1D arrays are like lists, 2D
   // arrays are like rectangles, and 3D arrays are like rectangular prisms. So
   // what does a 4D array look like? Good luck trying to think of one.
   $Master::ServerScore[%level, %index, %practice, %teams] = %player TAB %score;

   //echo("Score:" SPC %level SPC %index SPC %player SPC %score SPC %practice SPC %teams);

   //HiGuy: If you got your top score before, it should show up on the list
   if (%player $= $LB::Username) {
      %level = LBGetMissionFile(%level);
      if ($pref::highscores[%level, 0, %practice, %teams] < %score) {
         $Client::MissionFile = %level;
         //echo("Adding score" SPC %score SPC %practice SPC %teams);
         clientCmdEndGameScore(%score, %practice, %teams);
      }
   }
}

function clientCmdMasterTopScore(%level, %index, %player, %score, %practice, %teams) {
   $Master::TopScore[%level, %index, %practice, %teams] = %player TAB %score;

   //echo("Score2:" SPC %level SPC %index SPC %player SPC %score SPC %practice SPC %teams);

   //HiGuy: If you got your top score before, it should show up on the list
   if (%player $= $LB::Username) {
      %level = LBGetMissionFile(%level);
      if ($pref::highscores[%level, 0, %practice, %teams] < %score) {
         $Client::MissionFile = %level;
         //echo("Adding score" SPC %score SPC %practice SPC %teams);
         clientCmdEndGameScore(%score, %practice, %teams);
      }
   }
}

function clientCmdMasterLevelScores() {
   deleteVariables("$Master::TopScore" @ fileBase($MP::MissionFile) @ "*");
}

function clientCmdMasterLevelScoresEnd() {
   MPPlayMissionDlg.updateDisplay();
}

function clientCmdInvalidMission(%file) {
	//Tell MPPMGui
	$MP::InvalidMission[%file] = true;

	MPPlayMissionDlg.updateDisplay();
}

function clientCmdCloseLobby() {
	$Server::Lobby = false;
}

function clientCmdPreloadStart() {
	$Server::Preloading = true;
   $Server::Preloaded = false;

   MPPlayMissionDlg.determineVisibility();
}

function clientCmdMissionListStart() {
	//Clear server mission list
	$Server::MissionCount = 0;
}

function clientCmdMissionListFile(%file) {
	//Check if we have the file
	if (isFile(%file)) {
		//Get its info
		$Server::Mission[$Server::MissionCount] = %file;
		$Server::MissionCount = 0;
	} else {
		//We need to get it
		commandToServer('GetMissionInfo', %file);
	}
}

function clientCmdMissionListEnd() {
	MPPlayMissionDlg.determineVisibility();
}

function clientCmdGetMissionInfoDone(%file) {
	$Server::MissionInfoData[%file] = "";
}

//The most args you can cmdToClient is 18, just a cool fact
function clientCmdGetMissionInfoPart (%file,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17) {
	$Server::MissionInfoData[%file] = $Server::MissionInfoData[%file] @ %a1@%a2@%a3@%a4@%a5@%a6@%a7@%a8@%a9@%a10@%a11@%a12@%a13@%a14@%a15@%a16@%a17;
}

function clientCmdGetMissionInfoDone(%file) {
	//HiGuy: I KNOW IT'S A SIN TO DO THIS BUT THERE'S NO BETTER WAY :(
	eval($Server::MissionInfoData[%file]);
	//HiGuy: <roleplaying being shot by jeff> so sorry.. so many lights... <dies>

}


function clientCmdSubmitScoresStart(%id) {
	$Server::SubmitData = "";
	$Server::SubmitId = %id;
}

//The most args you can cmdToClient is 18, just a cool fact
function clientCmdSubmitScoresPart (%id, %a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17) {
	if (%id $= $Server::SubmitId)
		$Server::SubmitData = $Server::SubmitData @  %a1@%a2@%a3@%a4@%a5@%a6@%a7@%a8@%a9@%a10@%a11@%a12@%a13@%a14@%a15@%a16@%a17;
}

function clientCmdSubmitScoresDone(%id) {
	if (%id $= $Server::SubmitId) {
		//Submit it for the server
		masterCalcScores(%id);
	}
}




//-----------------------------------------------------------------------------
// Mission Preloading A.K.A the fun Section

function clientCmdPreloadPhase1(%sequence) {
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   //HiGuy: The host gets these too, but we already have all the things :(
   $Server::Preloaded = false;
   $Server::Preloading = true;

   onMissionDownloadPhase1();

   commandToServer('PreloadPhase1Ack', %sequence);
}

function clientCmdPreloadPhase2(%sequence) {
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   //HiGuy: Almost forgot this!
   //purgeResources();

   onPhase1Complete();
   onMissionDownloadPhase2();

   commandToServer('PreloadPhase2Ack', %sequence);
}

function clientCmdPreloadFinish() {
   onPhase2Progress(1.0);

   onPhase2Complete();
   schedule(2500, 0, clientFinishPreload);
}

function clientCmdPreloadProgress(%players, %total) {
	$Server::PreloadedClients = %players;
}

function clientCmdPreloaded() {
	$Server::Preloading = false;
	$Server::Preloaded = true;
	MPPlayMissionDlg.determineVisibility();
}

function clientFinishPreload() {
   if ($Server::Hosting) {
      %name = "MultiPlayer -" SPC upperFirst($MP::MissionObj.name) SPC "-" SPC upperFirst($LB::MissionType) SPC "Level" SPC ($LBPref::SelectedRow + 1);
   } else {
      %name = "MultiPlayer -" SPC upperFirst($MP::MissionName) SPC "-" SPC upperFirst($LB::MissionType) SPC "Level" SPC ($MP::MissionRow + 1);
   }
}

function clientCmdPreloadPhase3(%missionName) {
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   $MSeq = %seq;
   $Client::MissionFile = %missionName;
   //$lightingMission = false;

   onMissionDownloadPhase3(%missionName);
   onPhase3Complete();
   // The is also the end of the mission load cycle.
   onMissionDownloadComplete();

   commandToServer('PreloadPhase3Ack');
}
