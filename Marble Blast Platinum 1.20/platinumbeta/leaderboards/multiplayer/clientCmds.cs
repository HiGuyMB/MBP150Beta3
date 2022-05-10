//------------------------------------------------------------------------------
// Multiplayer Package
// clientCmds.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// HiGuy: PlayGui Commands

function clientCmdStartTimer() {
   PlayGui.startTimer();
}

function clientCmdStopTimer() {
   PlayGui.stopTimer();
}

function clientCmdResetTimer() {
   PlayGui.resetTimer();
}

function clientCmdSetMessage(%message, %timeout) {
   PlayGui.setMessage(%message, %timeout);
}

function clientCmdSetGemCount(%gems, %best) {
   PlayGui.setGemCount(%gems, %best);
}

function clientCmdSetMaxGems(%gems) {
   PlayGui.setMaxGems(%gems);
}

function clientCmdAddHelpLine(%line, %playBeep) {
   addHelpLine(%line, %playBeep);
}

function clientCmdAdjustTimer(%time) {
   PlayGui.adjustTimer(%time);
}

function clientCmdAddBonusTime(%time) {
   PlayGui.addBonusTime(%time);
}

function clientCmdSetTime(%time) {
   PlayGui.setTime(%time);
}

function clientCmdSetPowerUp(%powerUp) {
   PlayGui.setPowerUp(%powerUp);
}

function clientCmdActivatePowerUp(%powerUpId) {
   //if (%powerUpId == 6)
      //MegaRollingHardSfx.filename = RollingHardSfx.filename = "~/data/sound/mega_roll.wav";
}
function clientCmdDeactivatePowerUp(%powerUpId) {
   //if (%powerUpId == 6)
      //MegaRollingHardSfx.filename = RollingHardSfx.filename = "~/data/sound/Rolling_Hard.wav";
}

function clientCmdSetGravityDir(%dir, %reset, %rotation) {
   $Game::LastGravityDir = %dir;
   $Game::GravityDir = %dir;
   $Game::GravityRot = %rotation;
   setGravityDir(%dir, %reset);
   calcGravityUV();
}

function clientCmdOOBMessageHack() {
   // Jeff: hacking this in so that the oob message works properly
   CenterMessageDlg.timer = CenterMessageDlg.schedule(2000,"setVisible",false);
}

function clientCmdOOBMessageHack2() {
   //PlayGui.setMessage("outOfBounds",2000);
   // Jeff: since the oob message appears to be "broken", i'll hack it in my way
   cancel(CenterMessageDlg.timer);
   CenterMessageDlg.setBitmap($userMods @ "/client/ui/game/outOfBounds.png",true);
   CenterMessageDlg.setVisible(true);
   CenterMessageDlg.timer = CenterMessageDlg.schedule(2000,"setVisible",false);
}

function clientCmdIncrementOOBCounter() {
    // SPY47 - HOORAY FOR THE OOB COUNTER !!!
    // Matan - HOORAY!
    // Jeff  - YAY!
   if ($LB::LoggedIn && $LB::Username !$= "") {
      $LBPref::OOBCount ++;
   }
	if(!$playingDemo && $Server::ServerType !$= "Multiplayer") { // Jeff: no MP mode
    	$PREF::OOBCOUNT ++;
		if ($Pref::ShowOOBMessages) {
			if (OOBCounter::check())
				echo("You got owned.");
		}
	}
}

function clientCmdSetGameState(%state) {
   //HiGuy: Host gets these already
   if (!$Server::Hosting || $Server::_Dedicated)
      $Game::State = %state;
}

//-----------------------------------------------------------------------------
// HiGuy: Ghost / Marble commands

// Jeff: applys an impulse to each client
function clientCmdApplyImpulse(%position,%impulse) {
	if (MPMyMarbleExists())
		$MP::MyMarble.applyImpulse(%position,%impulse);
}

//HiGuy: Multiplies impulse vector by gravity, then impulses
function clientCmdGravityImpulse(%position,%impulse) {
	if (MPMyMarbleExists())
		$MP::MyMarble.applyImpulse(%position,VectorMult(VectorScale(%impulse, -1), getGravityDir()));
}

// Jeff: start the ghost rotation by sending rot values to the server
function clientCmdStartRotation() {
   MPgetMyMarble();
	MPGiveRotToServer();
}

// Jeff: this is sent whenever the marble is created to the client so that
//       they always have their scale ready to be used
function clientCmdGetMyScale(%scale) {
	$MP::MyScale = %scale;
}

//HiGuy: Hide Ghosts
function clientCmdHideGhosts() {
   MPHideGhosts();
}

//HiGuy: Reset Ghosts
function clientCmdResetGhosts() {
   MPResetGhosts();
}

//HiGuy: Hide Player Ghost
function clientCmdHideMyGhost() {
   MPHideMyGhost();
}

//HiGuy: Fix them!
function clientCmdFixGhost() {
   fixGhost();
}

function clientCmdWaitForEndgame() {
   //HiGuy: Called right when you start doing the "victory" stage of the endgame
   Canvas.popDialog(ExitGameDlg);
}

function clientCmdEndGameSetup() {
   // Single player... grab the playgui as the elapsed time and
   // roll in clients penalty and bonus
   PlayGUI.stopTimer();

   $Game::ScoreTime = PlayGui.elapsedTime;
   $Game::ElapsedTime = PlayGui.elapsedTime - %client.penaltyTime + PlayGui.totalBonus;
   $Game::PenaltyTime = %client.penaltyTime;
   $Game::BonusTime = PlayGui.totalBonus;

   // Not all missions have time qualifiers
   if ($Game::Finished) // Fix to prevent the next mission from getting qualified when creating a level in the editor - phil
      $Game::Qualified = MissionInfo.time? $Game::ScoreTime < MissionInfo.time: true;

   // Jeff: leaderboard stuff: if we qualify send time!
   if ($LB::LoggedIn && $Game::Qualified && $Server::ServerType !$= "MultiPlayer") {
      %securityCode = 89637;
      sendScoreToLB(%securityCode);
   }

   // Jeff: get right qualification
   if ($LB::LoggedIn)
      %type = ($LB::MarbleChoice $= "GGMarble") ? MissionInfo.type @ "_GG" : MissionInfo.type;
   else
      %type = MissionInfo.type;

   // Bump up the max level
	if($CurrentGame $= "Gold") {
		if (!$DemoBuild && !$playingDemo && $Game::Qualified && (MissionInfo.level + 1) > $Pref::MBGQualifiedLevel[%type])
         $Pref::MBGQualifiedLevel[%type] = MissionInfo.level + 1;
	} else {
		if (!$DemoBuild && !$playingDemo && $Game::Qualified && (MissionInfo.level + 1) > $Pref::QualifiedLevel[%type])
         $Pref::QualifiedLevel[%type] = MissionInfo.level + 1;
	}
}

function clientCmdEndGameScore(%score, %practice, %teams) {
   %practice = !!%practice;
   %teams = !!%teams;
   //HiGuy: Multiplayer high score prefs
   for (%i = 0; %i < $Game::HighscoreCount; %i ++) {
      if (%score > getField($pref::highscores[$Client::MissionFile, %i, %practice, %teams], 0)) {
         for (%j = $Game::HighscoreCount - 1; %j > %i; %j --)
            $pref::highscores[$Client::MissionFile, %j, %practice, %teams] = $pref::highscores[$Client::MissionFile, %j - 1, %practice, %teams];
         $highScoreIndex = %i;
         $pref::highscores[$Client::MissionFile, %i, %practice, %teams] = %score;
         break;
      }
   }
}

//HiGuy: Called when adding a ghost to the list
function clientCmdGhostIndex(%scale, %index) {
   $MP::GhostLookup[%scale] = %index;
   $MP::MeshLookup[%scale] = %index;
   MPBuildGhostList();
}

//-----------------------------------------------------------------------------
// Jeff: other commands

function clientCmdTeamChat(%sender, %team, %leader, %message) {
   MPTeamSelectDlg.receiveChat(%sender, %team, %leader, %message);
}

// Jeff: if you try to somehow play when clients are not ready
function clientCmdHostNotReady() {
   LBAssert("Error!", "Not all clients are ready.  Please wait..");
}

function clientCmdPregameUserList(%list) {
   MPPreGameDlg.updateUserlist(%list);
}

// Jeff: push the dialog
function clientCmdPushDialog(%dialog) {
   if (isObject(%dialog))
      Canvas.pushDialog(%dialog);
}

// Jeff: pop the dialog
function clientCmdPopDialog(%dialog) {
   if (isObject(%dialog))
      Canvas.popDialog(%dialog);
}

function clientCmdSetPregame(%isPregame) {
   $Game::Pregame = %isPregame;
   if (%isPregame && Canvas.getContent().getName() $= "PlayGui") {
      disableChatHUD();
      Canvas.pushDialog(MPPreGameDlg);
   } else
      Canvas.popDialog(MPPreGameDlg);
}

function clientCmdTweenUpdate(%index, %difference, %speed) {
   tweenUpdate(%index, %difference, %speed);
}

// Jeff: for particles
function clientCmdGetParticle(%timeMultiple, %scale, %playerScale) {
   getClientParticle(%timeMultiple, %scale, %playerScale);
}

function clientCmdSetBlastValue(%blastValue) {
   $MP::BlastValue = %blastValue;
   PlayGui.setBlastValue(%blastValue);
   PlayGui.updateBlastBar();
}

function clientCmdSetSpecialBlast(%special) {
   $MP::SpecialBlast = %special;
   PlayGui.updateBlastBar();
}

// Jeff: used for reseting camera yaw
function clientCmdSetYaw(%yaw) {
   $marbleYaw = %yaw;
}

addMessageCallback('MsgLoadMode', "clientCmdSetGameMode");

function clientCmdSetGameMode(%msgType, %msgString, %mode) {
   %mode = %mode $= "" ? (%msgString $= "" ? %msgType : %msgString) : %mode;
   devecho("Game mode is" SPC %mode);
   $Game::isHunt = $Server::ServerType $= "Multiplayer" && (%mode !$= "" && strPos(strLwr(%mode), "hunt") != -1);
   $Game::isFree = $Server::ServerType $= "Multiplayer" && (%mode !$= "" && strPos(strLwr(%mode), "free") != -1);
}

// Jeff: receive private chat
function clientCmdPrivateMessage(%name, %message) {
   onServerChat(%name, %message);
//   addLBChatLine(%message);
}

function clientCmdHostStatus(%status) {
	$Server::Hosting = %status;
	MPPlayMissionDlg.determineVisibility();
	MPPreGameDlg.updateActive();
	MPEndGameDlg.updateActive();
}

function clientCmdDedicated(%status) {
	$Server::_Dedicated = %status;
}

//-----------------------------------------------------------------------------
// Handicaps and the likes

function MPgetHandicaps() {
   return !!($MPPref::DisableGems2)     << 0  |
          !!($MPPref::DisableGems5)     << 1  |
          !!($MPPref::DisableCollision) << 2  |
          !!($MPPref::DisableDiagonal)  << 3  |
          !!($MPPref::DisableJump)      << 4  |
          !!($MPPref::DisableBlast)     << 5  |
          !!($MPPref::DisablePowerup1)  << 6  |
          !!($MPPref::DisablePowerup2)  << 7  |
          !!($MPPref::DisablePowerup5)  << 8  |
          !!($MPPref::DisablePowerup6)  << 9  |
          !!($MPPref::DisableRadar)     << 10 |
          !!($MPPref::DisableMarbles)   << 11;
}

function MPgetHandicap(%flag) {
	switch (%flag) {
   case 0:  return !!$MPPref::DisableGems2;
	case 1:  return !!$MPPref::DisableGems5;
	case 2:  return !!$MPPref::DisableCollision;
	case 3:  return !!$MPPref::DisableDiagonal;
	case 4:  return !!$MPPref::DisableJump;
	case 5:  return !!$MPPref::DisableBlast;
	case 6:  return !!$MPPref::DisablePowerup1;
	case 7:  return !!$MPPref::DisablePowerup2;
	case 8:  return !!$MPPref::DisablePowerup5;
	case 9:  return !!$MPPref::DisablePowerup6;
	case 10: return !!$MPPref::DisableRadar;
	case 11: return !!$MPPref::DisableMarbles;
   }
}

function MPsetHandicaps(%flags) {
   $MPPref::DisableGems2      = !!(%flags & (1 << 0));
   $MPPref::DisableGems5      = !!(%flags & (1 << 1));
   $MPPref::DisableCollision  = !!(%flags & (1 << 2));
   $MPPref::DisableDiagonal   = !!(%flags & (1 << 3));
   $MPPref::DisableJump       = !!(%flags & (1 << 4));
   $MPPref::DisableBlast      = !!(%flags & (1 << 5));
   $MPPref::DisablePowerup1   = !!(%flags & (1 << 6));
   $MPPref::DisablePowerup2   = !!(%flags & (1 << 7));
   $MPPref::DisablePowerup5   = !!(%flags & (1 << 8));
   $MPPref::DisablePowerup6   = !!(%flags & (1 << 9));
   $MPPref::DisableRadar      = !!(%flags & (1 << 10));
   $MPPref::DisableMarbles    = !!(%flags & (1 << 11));
}

function MPsetHandicap(%number, %flag) {
   switch (%number) {
   case 0:  $MPPref::DisableGems2     = !!%flag;
	case 1:  $MPPref::DisableGems5     = !!%flag;
	case 2:  $MPPref::DisableCollision = !!%flag;
	case 3:  $MPPref::DisableDiagonal  = !!%flag;
	case 4:  $MPPref::DisableJump      = !!%flag;
	case 5:  $MPPref::DisableBlast     = !!%flag;
	case 6:  $MPPref::DisablePowerup1  = !!%flag;
	case 7:  $MPPref::DisablePowerup2  = !!%flag;
	case 8:  $MPPref::DisablePowerup5  = !!%flag;
	case 9:  $MPPref::DisablePowerup6  = !!%flag;
	case 10: $MPPref::DisableRadar     = !!%flag;
	case 11: $MPPref::DisableMarbles   = !!%flag;
   }
}

function clientCmdSetHandicaps(%flags) {
	MPsetHandicaps(%flags);
}

function clientCmdSetHandicap(%number, %flag) {
	MPsetHandicap(%number, %flag);
}

function MPgetServerHandicaps() {
   return !!($MP::Server::DisableGems2)     << 0  |
          !!($MP::Server::DisableGems5)     << 1  |
          !!($MP::Server::DisableCollision) << 2  |
          !!($MP::Server::DisableDiagonal)  << 3  |
          !!($MP::Server::DisableJump)      << 4  |
          !!($MP::Server::DisableBlast)     << 5  |
          !!($MP::Server::DisablePowerup1)  << 6  |
          !!($MP::Server::DisablePowerup2)  << 7  |
          !!($MP::Server::DisablePowerup5)  << 8  |
          !!($MP::Server::DisablePowerup6)  << 9  |
          !!($MP::Server::DisableRadar)     << 10 |
          !!($MP::Server::DisableMarbles)   << 11;
}

function clientCmdServerSetHandicaps(%flags) {
	$MP::Server::DisableGems2      = !!(%flags & (1 << 0));
   $MP::Server::DisableGems5      = !!(%flags & (1 << 1));
   $MP::Server::DisableCollision  = !!(%flags & (1 << 2));
   $MP::Server::DisableDiagonal   = !!(%flags & (1 << 3));
   $MP::Server::DisableJump       = !!(%flags & (1 << 4));
   $MP::Server::DisableBlast      = !!(%flags & (1 << 5));
   $MP::Server::DisablePowerup1   = !!(%flags & (1 << 6));
   $MP::Server::DisablePowerup2   = !!(%flags & (1 << 7));
   $MP::Server::DisablePowerup5   = !!(%flags & (1 << 8));
   $MP::Server::DisablePowerup6   = !!(%flags & (1 << 9));
   $MP::Server::DisableRadar      = !!(%flags & (1 << 10));
   $MP::Server::DisableMarbles    = !!(%flags & (1 << 11));
}

function clientCmdServerSetHandicap(%number, %flag) {
	switch (%number) {
   case 0:  $MP::Server::DisableGems2     = !!%flag;
	case 1:  $MP::Server::DisableGems5     = !!%flag;
	case 2:  $MP::Server::DisableCollision = !!%flag;
	case 3:  $MP::Server::DisableDiagonal  = !!%flag;
	case 4:  $MP::Server::DisableJump      = !!%flag;
	case 5:  $MP::Server::DisableBlast     = !!%flag;
	case 6:  $MP::Server::DisablePowerup1  = !!%flag;
	case 7:  $MP::Server::DisablePowerup2  = !!%flag;
	case 8:  $MP::Server::DisablePowerup5  = !!%flag;
	case 9:  $MP::Server::DisablePowerup6  = !!%flag;
	case 10: $MP::Server::DisableRadar     = !!%flag;
	case 11: $MP::Server::DisableMarbles   = !!%flag;
   }
}

function clientCmdShockwave(%mePos, %strength, %myMod, %theyMod) {
   %theyPos = $MP::MyMarble.getWorldBoxCenter();
   if (VectorDist(%theypos, %mePos) < %strength) {
      %dist = Vector2d(VectorSub(%theypos, %mepos));
      %dist = VectorScale(VectorNormalize(%dist), %strength);
      $MP::MyMarble.applyImpulse(VectorScale(%dist, -1.2), VectorScale(%dist, (0.7 * %theyMod) / %myMod));
   }
}

function clientCmdAltPort(%port) {
	echo("Connecting to the wrong port! Actual port is" SPC %port);

	//HiGuy: Oh shit we connected to the wrong server!
	%address = ServerConnection.getAddress();

	//HiGuy: What the actual fuck?
	if (%address $= "local")
		return;

	//IP:127.0.0.1:28000
	%address = getSubStr(%address, strpos(%address, ":") + 1, strlen(%address));
	%address = getSubStr(%address, 0, strpos(%address, ":"));

	LBMessage("Resolving Port Issues...");

	$Server::Reconnecting = true;
	disconnect(true);

	schedule(2000, 0, joinServer, %address @ ":" @ %port, $LB::Username, $MP::ServerPassword, %port);
}

//-----------------------------------------------------------------------------
// CRC Checking
//       CRC validation will check to ensure that clients are not cheating.
//       although not totally perfect, it will provide a sufficient amount
//       of protection and will also prevent hacked up servers from being used.
//       Note this will only CRC the .cs.dso files
//
//       Also will check .cs.dso file counts to ensure that we dont have
//       additional scripts

function clientCmdCheckCRC() {
   if (%this.pinging)
      return;

   //HiGuy: The server gives us absolutely no information about what we should
   // send. That bastard! We'll show him!

   //HiGuy: We should probably do a compile, just to make sure we have what the
   // server wants. Don't want to be denied, now do we?

   %patterns = "*.cs;*.gui";

   //HiGuy: Turn off logging, nobody needs to see this!
   %buff = $Con::LogBufferEnabled;
   $Con::LogBufferEnabled = false;

   %compiled = false;

   //HiGuy: Token-separated fields
   while (%patterns !$= "") {
      %patterns = nextToken(%patterns, "exp", ";");

      for (%file = findFirstFile(%exp); %file !$= ""; %file = findNextFile(%exp)) {
         //HiGuy: We should compile all of these files, just to be sure
         if (!isFile(%file @ ".dso")) {
            if (compile(%file))
               %compiled = true;
            else {
               //HiGuy: If it didn't compile, well shit! We'll just have to send
               // the old .dso and hope the server likes it!
               $Con::LogBufferEnabled = %buff;
               devecho("Could not compile file:" SPC %file);
               $Con::LogBufferEnabled = false;
            }
         }
      }
   }

   //HiGuy: If any new files blipped into existence, tell us about them!
   if (%compiled)
      setModPaths($usermods);

   //HiGuy: Turn it back on, we want info!
   $Con::LogBufferEnabled = %buff;

   //HiGuy: Let the server know we're about to flood it with CRCs
   // Otherwise it won't let know we're coming, and will just ignore us.
   commandToServer('StartCRC');

   %files = 0;
   %sendPattern = "*.dso";
   for (%file = findFirstFile(%sendPattern); %file !$= ""; %file = findNextFile(%sendPattern)) {
      if (strPos(strlwr(%file), "prefs.cs") != -1 || strPos(strlwr(%file), ".svn") != -1 || strPos(strlwr(%file), "config.cs") != -1 || strPos(strlwr(%file), "banlist.cs") != -1)
         continue;

      //HiGuy: For each of these files, we should send it to the server
      commandToServer('FileCRC', %file, getFileCRC(%file));

      //HiGuy: The server wants to know how many files we have!
      %files ++;
   }

   //HiGuy: Let the server we've finished. It'll either kick us out, or nicely
   // let us join. Hopefully we've managed to satisfy it's requirements!
   commandToServer('FinishCRC', %files);
}

function clientCmdCRCError(%error) {
   devecho("\c2There was a CRC error:" SPC %error);
}

function clientCmdVerifySession() {
   //HiGuy: We need to verify out session with the server. Let's hope it likes
   // us! (Insider note: the server does not like the client)

   commandToServer('VerifySession', $LBGameSess);
}

function clientCmdVerifySuccess() {
   //HiGuy: Hooray, the server accepts us! Maybe it'll give us a belly rub :3

   //HiGuy: Let's send it some information
   commandToServer('SetHandicaps', MPgetHandicaps());
}
