//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Penalty and bonus times.
$Game::TimeTravelBonus = 5000;

// Item respawn values, only powerups currently respawn
$Item::RespawnTime = 7 * 1000;
$Item::PopTime = 10 * 1000;

// Game duration in secs, no limit if the duration is set to 0
$Game::Duration = 0;

// Pause while looking over the end game screen (in secs)
$Game::EndGamePause = 5;

//-----------------------------------------------------------------------------
// Variables extracted from the mission
$Game::GemCount = 0;
$Game::StartPad = 0;
$Game::EndPad = 0;

// Spy47 : Change this if you want !!
$powerupDelay = 600; // This makes sure that you're not going to left-click too long and use the powerup by accident.


//-----------------------------------------------------------------------------
//  Functions that implement game-play
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------

function onServerCreated()
{
   // Server::GameType is sent to the master server.
   // This variable should uniquely identify your game and/or mod.
   $Server::GameType = "Marble Blast";

   // Server::MissionType sent to the master server.  Clients can
   // filter servers based on mission type.
   $Server::MissionType = "Race";

   // GameStartTime is the sim time the game started. Used to calculated
   // game elapsed time.
   $Game::StartTime = 0;

   //HiGuy: Reset teams
   $MP::TeamMode = 0;
   $MP::Teams = 0;
   if (isObject(TeamGroup))
      TeamGroup.delete();

   Team::createDefaultTeam();

   // HiGuy: Clear random powerup variables. We don't want these floating over
   deleteVariables("$Game::RandomPowerup*");
   $Game::RandomPowerups = 0;
   $Game::RandomPowerupOn = 0;
   %id = PD_DemoList.getSelectedId();
   //HiGuy: I'm not sure what this is, but it's causing errors
   if (isObject(DemoGroup)) {
      %demo = DemoGroup.getObject(%id - 1);
      if ($playingDemo && %demo.randoms) {
         for (%i = 0; %i < %demo.randoms; %i ++) {
            $Game::RandomPowerup[%i] = %demo.random[%i];
         }

         // Jeff; higuy, I fixed your fucking code. n00b
         $Game::RandomPowerupOn = %demo.randoms;
      }
   }

   // Load up all datablocks, objects etc.  This function is called when
   // a server is constructed.
   exec("./audioProfiles.cs");
   exec("./camera.cs");
//   Nobody uses it and it sits there and taking up space in the Level Editor.
//   exec("./markers.cs");
   exec("./triggers.cs");
   exec("./inventory.cs");
   exec("./shapebase.cs");
   exec("./staticshape.cs");
   exec("./item.cs");

   // Basic items
   exec("./marble.cs");
   exec("./gems.cs");
   exec("./powerUps.cs");
   exec("./buttons.cs");
   exec("./hazards.cs");
   exec("./pads.cs");
   exec("./bumpers.cs");
   exec("./signs.cs");
   exec("./fireworks.cs");

   // Spy47 stuff
   exec("./checkpoint.cs");
   exec("./teleporter.cs");

   // Jeff and HiGuy stuff:
   // exec("./ghost.cs"); //HiGuy: Moved

   // Marble Blast Gold stuff - Yea, we don't use that anymore, it's not sexy enough.
   // All the data inside it is now in the correct files as before. Hooray?
// exec("./MarbleBlastGold.cs");

   // Jeff: SINGLE PLAYER ONLY!!!!!!
   // maybe...not....
   //if ($Server::ServerType $= "SinglePlayer") {
      // Platforms and interior doors
      exec("./pathedInteriors.cs");
   //}

   // Keep track of when the game started
   $Game::StartTime = $Sim::Time;

   if ($LB::ChallengeMode && $LB::LoggedIn)
      sendChallengeGemUpdates();

   // Jeff: multiplayer scripts for multiplayer mode
   // screw it, keep it for single player too, can't hurt....
   //if ($Server::ServerType $= "Multiplayer") {
      $MP::SpawnCount = 0;
      $MP::GameInSession = false;
      $MP::LastClientCount = 0;
      initMultiplayerServer();
      MPUpdateGhostPositions();
      MPOutofBounds();
      velocityLoop();
      MPUpdateGhostCollision();
      //HiGuy: We want this to be called!
      schedule(1000, 0, MPUpdateGhostCollision);
   //}

   // Jeff: multiplayer compatibility ticker
   if (isObject(ServerTickerObject)) {
      //devecho("Turning on ServerTickerObject");
      //ServerTickerObject.setTickable(true);
   }

   // Jeff: this drives me nuts, I want a server variable NOW
   $Game::ServerRunning = true;
}

function onServerDestroyed()
{
   // Jeff: by now if we havn't stopped, just stop it already
   if ($LB::ChallengeMode && $LB::LoggedIn)
      stopChallengeGemUpdates();

   // Perform any game cleanup without actually ending the game
   destroyGame();

   //if ($Server::ServerType $= "MultiPlayer") {
      // Jeff: stop any server loops going on
      cancel($MP::Schedule::Collision);
      cancel($MP::Schedule::GhostPosition);
      cancel($MP::Schedule::OOB);
      cancel($MP::Schedule::VelocityLoop);
      cancel($MP::Schedule::BlastUpdate);
      cancel($MP::Schedule::Scores);

      //HiGuy: FOUND IT! This is  why servers kept disappearing
   //if ($Server::ServerType $= "Multiplayer") {
      //masterEndGame();
   //}
   //}

   if (isObject($Game::SpawnTriggers))
      $Game::SpawnTriggers.delete();

   // Jeff: multiplayer compatibility ticker
   if (isObject(ServerTickerObject)) {
      //devecho("Turning off ServerTickerObject");
      //ServerTickerObject.setTickable(false);
   }

   // Jeff: this drives me nuts, I want a server variable NOW
   $Game::ServerRunning = false;
   $Game::State = "";
}


//-----------------------------------------------------------------------------

function onMissionLoaded()
{
   // Called by loadMission() once the mission is finished loading.
   // Nothing special for now, just start up the game play.

   $Game::GemCount = countGems(MissionGroup);

   // Start the game here if multiplayer...
   if ($Server::ServerType $= "MultiPlayer") {
      // Jeff: amount of spectators
      $Server::SpectateCount = 0;
      setGameState("Waiting");
      startGame();
      schedule(0, 0, masterHeartbeat);
   }

   if ($MPPref::Server::GlassMode)
      initGlassMode();
   if ($MPPref::Server::GemVision)
      initGemVision();
}

function onMissionEnded()
{
   // Called by endMission(), right before the mission is destroyed
   // This part of a normal mission cycling or end.
   //$enableEditor = 0;
   endGame();
}

function onWaitingEnd() {
   onMissionReset();
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++)
      ClientGroup.getObject(%i).respawnPlayer();
}

function onMissionReset()
{
   //Only a problem on MP
   if (!$Server::Started && $Server::ServerType !$= "SinglePlayer")
      error("onMissionReset - No server running!");
   // Cancels any on-going teleporter....
   cancel($teleSched);

	// Reset the finished variable, this var is set true in State::end() of this file - Phil
	$Game::Finished = false;

   setAllGravityDirs("1 0 0 0 -1 0 0 0 -1",true,"0 0 -1");
   endFireWorks();

   // Reset the players and inform them we're starting
   %count = ClientGroup.getCount();
   for( %clientIndex = 0; %clientIndex < %count; %clientIndex++ ) {
      %cl = ClientGroup.getObject( %clientIndex );
      if (%cl.fake)
         continue;
      commandToClient(%cl, 'GameStart');
      %cl.resetStats();
      if ($Game::IsHunt && !%cl.spectating)
         %cl.respawnPlayer(%cl.spawnPoint);
   }

   // Start the game duration timer
   if ($Game::Duration)
      $Game::CycleSchedule = schedule($Game::Duration * 1000, 0, "onGameDurationEnd" );
   $Game::Running = true;

      ServerGroup.onMissionReset();

   $Game::ResetTime = $Sim::Time;

   // Set the initial state
   if ($Server::ServerType $= "SinglePlayer")
      setGameState("Start");
   else
      syncClients();

   if ($Game::IsHunt) {
   	resetSpawnWeights();
		resetSpawnGroup();
   	hideGems();
      spawnHuntGemGroup();
	}

   if ($MPPref::Server::MatanMode)
      matanModeUpdate();

//   if ($Server::ServerType $= "MultiPlayer") //HiGuy: Game start
//      showGems();
}

function SimGroup::onMissionReset(%this)
{
   if (%this.resetting) //It's apparently inside itself.. Shit
      return;
   %this.resetting = true;
   %count = %this.getCount();
   for(%i = 0; %i < %count; %i++)
      %this.getObject(%i).onMissionReset();
   %this.resetting = "";
}

function SimObject::onMissionReset(%this)
{
}

function GameBase::onMissionReset(%this)
{
   %this.getDataBlock().onMissionReset(%this);
}

//-----------------------------------------------------------------------------

function startGame()
{
   if ($Game::Running) {
      error("startGame: End the game first!");
      return;
   }
   $Game::Running = true;
   $Game::Qualified = false;
   if ($LB::LoggedIn && (($LB::SuperChallengeMode && $LB::SuperChallengeWaiting) || $LB::ChallengeMode)) {
      setGameState("waiting");
      return;
   }

   onMissionReset();
}

function endGameSetup() {
   commandToAll('WaitForEndgame');

   setGameState("end");

	$Game::Finished = true;
	$Game::State = "End";
   $Game::StateSchedule = schedule(2000, 0, "endGame");

   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      ClientGroup.getObject(%i).setBlastValue(0);
      ClientGroup.getObject(%i).setSpecialBlast(false);
   }

   if ($Server::ServerType $= "MultiPlayer") {
      // Jeff: update the score list
      MPsendScores();

      if ($MPPref::Server::GemVision)
			commandToAll('StopGemVision');
   }
}

function endGame()
{
   if (!$Game::Running) {
      //error("endGame: No game running!");
      return;
   }

   if ($Game::Recording)
   	finishRecording();

   // Jeff: send the challenge to the server
   // If it is attempts mode and we did not run out,
   // continue playing by just increasing the next attempt.
   if ($LB::LoggedIn && $LB::Challenge::Playing) {
      if ($LB::Challenge::Type $= "attempts") {
         if ($LB::Challenge::CurrentAttempts < $LB::Challenge::MaxAttempts) {
            onMissionReset();
            localClientConnection.respawnPlayer();
            $Game::ScoreTime = PlayGui.elapsedTime; // need to set this.
            $LB::Challenge::ScoreHelper = false;
            sendScoreToLB(89637);
            return;
         } else if ($LB::Challenge::CurrentAttempts == $LB::Challenge::MaxAttempts) {
            // Jeff: this is used so that we can
            $LB::Challenge::ScoreHelper = true;
            $Game::ScoreTime = PlayGui.elapsedTime; // need to set this.
            sendScoreToLB(89637);
            return;
         }
      } else {
         // Jeff: ugly hack, but works.  If cheaters gonna cheat,
         // then cheaters gonna cheat.  Send the correct time!!
         $Game::ScoreTime = PlayGui.elapsedTime;
         $LB::Challenge::GetThoseUpdates = true;
         sendChallengeGemUpdates();
      }
   }

   destroyGame();

   // Inform the clients the game is over
   commandToAll('EndGameSetup');
   commandToAll('GameEnd');

   if ($Server::ServerType $= "MultiPlayer")
      commandToAll('ResetTimer');
}

function pauseGame()
{
   // Jeff: if we are in lbs do not let them pause the game
   if ($Server::ServerType $= "SinglePlayer")
	{
	   if(alxIsPlaying($PlayTimerAlarmHandle))
	      alxStop($PlayTimerAlarmHandle);
	   if (!$LB::LoggedIn)
         $gamePaused = true;
	}
}

function resumeGame()
{
   // Jeff: resume game
   $gamePaused = false;
}

function destroyGame()
{
   // Cancel any client timers
   %count = ClientGroup.getCount();
   for (%index = 0; %index < %count; %index++)
      cancel(ClientGroup.getObject(%index).respawnSchedule);

   // Perform cleanup to reset the game.
   cancel($Game::CycleSchedule);
   cancel($Game::StateSchedule);

	if ($Server::ServerType !$= "MultiPlayer")
	   $Game::Running = false;
}


//-----------------------------------------------------------------------------

function setGameState(%state) {
	cancel($Game::StateSchedule);
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++)
      ClientGroup.getObject(%i).setGameState(%state);
   $Game::State = %state; // Jeff: needed for Single player
   commandToAll('SetGameState', %state);
}

function GameConnection::setGameState(%this, %state) {
   if ($LB::Challenge::DisableGameStates)
      return;
   if ($Server::Lobby)
      return;
   if (!%this.isReal())
      return;

   cancel(%this.stateSchedule);
   commandToClient(%this, 'SetGameState', %state);
   %this.state = %state;
   %state = alphaNum(%state); //Strip other chars

   // Jeff: use the call method, not eval!
   eval(%this @ ".state" @ %state @ "();");
}

function GameConnection::stateWaiting(%this) {
   %this.stopTimer();
   %this.resetTimer();
   %this.setMessage("");
   %this.schedule(500, setMessage, "waiting");
   %this.setGemCount(0);
   %this.setMaxGems($Game::GemCount);

   if ($LB::SuperChallengeMode) {
      if ($LB::SuperChallengePractice)
         onMissionReset();
      else
         LBSCD_SendWaiting();
   } else if ($LB::ChallengeMode) { // Jeff: challenge mode
      schedule(500, 0, LBWaitChallenge);
      sendChallengeGemUpdates();
   } else if ($Server::ServerType $= "Multiplayer") {

   }

   if ($Game::IsHunt)
      %this.setTime(MissionInfo.time ? MissionInfo.time : 300000);
}

function GameConnection::stateStart(%this)
{
   if ($LB::Challenge::DisableGameStates)
      return;
   %this.resetTimer();
   %this.setMessage("");
   %this.setGemCount(0);
   %this.setMaxGems($Game::GemCount);
   %this.stateSchedule = %this.schedule(($Server::ServerType $= "Multiplayer" ? 2000 : 500), "setGameState", "Ready");
   %this.setSpecialBlast(false);
   //HiGuy: This should be here!
   if (!$Game::isFree)
      %this.player.setMode(Start);
   if (MissionInfo.startHelpText !$= "")
      %this.addHelpLine(MissionInfo.startHelpText, false);
   if ($Game::IsHunt)
      %this.setTime(MissionInfo.time ? MissionInfo.time : 300000);
}

function GameConnection::stateReady(%this)
{
   if ($LB::Challenge::DisableGameStates)
      return;
   %this.play2d(ReadyVoiceSfx);
   %this.setMessage("ready");
   %this.stateSchedule = %this.schedule( 1500, "setGameState", "set");
}

function GameConnection::stateSet(%this)
{
   if ($LB::Challenge::DisableGameStates)
      return;
   %this.play2d(SetVoiceSfx);
   %this.setMessage("set");

   %this.stateSchedule = %this.schedule( 1500, "setGameState", "Go");
}

function GameConnection::stateGo(%this)
{
   if ($LB::Challenge::DisableGameStates)
      return;
   %this.play2d(GetRollingVoiceSfx);
   %this.setMessage("go");
   %this.startTimer();
   %this.oobMessageHack(); // :D

   // Target the players to the end pad and let them lose
   %this.player.setPad($Game::EndPad);
   %this.player.setMode(Normal);
}

function GameConnection::stateEnd(%this)
{
   if ($LB::Challenge::DisableGameStates)
      return;

   //HiGuy: For highscores
   if ($Server::ServerType $= "MultiPlayer" && !%this.fake && !%this.spectating)
      commandToClient(%this, 'EndGameScore', %this.gemCount, getActivePlayerCount() <= 1, !!$MP::TeamMode);

	%this.lastScore = %this.gemCount;

   // Do score calculations, messages to winner, losers, etc.
   %this.stopTimer();
   %this.player.setMode("Victory"); //No more moving!
   %this.play2d(WonRaceSfx);

   if (!$Server::Dedicated) //HiGuy: This kills the server
	   startFireWorks(%this.player);
}


//-----------------------------------------------------------------------------

//HiGuy: This isn't used anywhere and is just taking up space. Leaving it commented in here in case we ever need it, though

//function onGameDurationEnd()
//{
   // This "redirect" is here so that we can abort the game cycle if
   // the $Game::Duration variable has been cleared, without having
   // to have a function to cancel the schedule.
   //if ($Game::Duration && !isObject(EditorGui))
      //cycleGame();
//}

//function cycleGame()
//{
   // This is setup as a schedule so that this function can be called
   // directly from object callbacks.  Object callbacks have to be
   // carefull about invoking server functions that could cause
   // their object to be deleted.
   //if (!$Game::Cycling) {
      //$Game::Cycling = true;
      //$Game::CycleSchedule = schedule(0, 0, "onCycleExec");
   //}
//}

//function onCycleExec()
//{
   // End the current game and start another one, we'll pause for a little
   // so the end game victory screen can be examined by the clients.
   //endGame();
   //$Game::CycleSchedule = schedule($Game::EndGamePause * 1000, 0, "onCyclePauseEnd");
//}

//function onCyclePauseEnd()
//{
   //$Game::Cycling = false;
   //loadNextMission();
//}

//function loadNextMission()
//{
   //%nextMission = "";

   // Cycle to the next level, or back to the start if there aren't
   // any more levels.
   //for (%file = findFirstFile($Server::MissionFileSpec);
         //%file !$= ""; %file = findNextFile($Server::MissionFileSpec))
      //if (strStr(%file, "CVS/") == -1 && strStr(%file, "common/") == -1)
      //{
         //%mission = getMissionObject(%file);
         //if (%mission.type $= MissionInfo.type) {
            //if (%mission.level == 1)
               //%nextMission = %file;
            //if ((%mission.level + 0) == MissionInfo.level + 1) {
               //echo("Found one!");
               //%nextMission = %file;
               //break;
            //}
         //}
      //}
   //loadMission(%nextMission);
//}

//-----------------------------------------------------------------------------
// GameConnection Methods
// These methods are extensions to the GameConnection class. Extending
// GameConnection make is easier to deal with some of this functionality,
// but these could also be implemented as stand-alone functions.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function GameConnection::incPenaltyTime(%this,%dt)
{
   %this.adjustTimer(%dt);
   %this.penaltyTime += %dt;
}

function GameConnection::incBonusTime(%this,%dt)
{
   %this.addBonusTime(%dt);
   %this.bonusTime += %dt;
}


function GameConnection::onClientEnterGame(%this)
{
   // Create a new camera object.
   %this.camera = new Camera() {
      dataBlock = Observer;
   };
   MissionCleanup.add( %this.camera );
   %this.camera.scopeToClient(%this);

   // Setup game parameters and create the player
   if (!%this.restored)
      %this.resetStats();

   if ($Server::ServerType $= "SinglePlayer")
      %this.spawnPlayer();

   // Jeff: used for OOB Click in Multiplayer
   %this.isOOB = false;

   // Anchor the player to the start pad
   if (!$Game::isFree)
      %this.player.setMode(Start);

   //HiGuy: Unset the loading state
   %this.loading = false;

   // Start the game here for single player
   if ($Server::ServerType $= "SinglePlayer")
      startGame();
   else {
      %this.radarInit();
      updateScores(); // Jeff: update the score list
      updateReadyUserList(); //HiGuy: Update the user list
      %this.setQuickRespawnStatus(true); // Jeff: enable the 1st quick respawn
		%this.forceSpectate = false;
      if ($Server::Started) {
         %this.setPregame(false);
         %this.resetTimer();
         %this.setTime(PlayGui.elapsedTime);

         //Spectating...
         if (%this.restored && !%this._spectating && !$Game::Finished) {
            %this.spawnPlayer();
            %this.respawnPlayer();
         } else {
            %this.setSpectating(true);
            %this.setGameState("go");

            if (!$MPPref::ForceSpectators) {
					schedule(2000, 0, commandToClient, %this, 'SpectateChoice');
					%this.forceSpectate = true;
				}
         }

         if ($Game::Finished) {
            //HiGuy: You're too late!
            schedule(1000, 0, commandToClient, %this, 'EndGameSetup');
            schedule(1000, 0, commandToClient, %this, 'GameEnd');
            schedule(1000, 0, commandToClient, %this, 'ResetTimer');

            schedule(1000, 0, serverSendScores);
            %this.setGameState("waiting");
            %this.schedule(1000, setMessage, "");
            %this.setTime(0);
         } else
            %this.setGameState("go");
      } else {
         %this.setGameState("waiting");
         %this.setPregame(true);
         %this.stopTimer();
         %this.startOverview();
      }
   }
}

function GameConnection::onClientLeaveGame(%this)
{
   if (isObject(%this.camera))
      %this.camera.delete();
   if (isObject(%this.player))
      %this.player.delete();

   // Jeff: delete the trail emitter
   if (isObject(%this.trailEmitter))
      %this.trailEmitter.delete();
   if (isObject(%this.trailWhiteEmitter))
      %this.trailWhiteEmitter.delete();

   if ($Server::ServerType $= "Multiplayer") {
      // Jeff: delete MP ghost
      if (isObject(%this.ghost))
         %this.ghost.delete();
      if (isObject(%this.mesh))
         %this.mesh.delete();

      // Jeff: update the score list
      updateScores();
   }
}

function GameConnection::resetStats(%this)
{
   // Reset game stats
   %this.bonusTime = 0;
   %this.penaltyTime = 0;
   %this.gemCount = 0;

   // Reset the checkpoint
   //if (isObject(%this.checkPoint))
   //   %this.checkPoint.delete();
   //%this.checkPoint = new ScriptObject() {
   //   pad = $Game::StartPad;
   //   time = 0;
   //   gemCount = 0;
   //   penaltyTime = 0;
   //   bonusTime = 0;
   //   powerUp = 0;
   //};

   %this.gemsFound[1] = 0;
   %this.gemsFound[2] = 0;
   %this.gemsFound[5] = 0;

   %this.checkpointed = 0; // Spy47 : Reset checkpoint counter.
   %this.curCheckpoint = 0;
   %this.curCheckpointNum = 0;

   %this.checkPointPad = $Game::StartPad;
	%this.checkPointTime = ($Game::IsHunt ? (MissionInfo.time ? MissionInfo.time : 300000) : 0);
	%this.checkPointGemCount = 0;
	%this.checkPointPenaltyTime = 0;
	%this.checkPointBonusTime = 0;
	%this.checkPointPowerUp = 0;
}


//-----------------------------------------------------------------------------

function GameConnection::onEnterPad(%this)
{
	if($CurrentGame $= "Gold")
		%item_name = "gem";
	else
		%item_name = "diamond";

   if (%this.player.getPad() == $Game::EndPad) {

      if ($Game::GemCount && %this.gemCount < $Game::GemCount) {
         %this.play2d(MissingGemsSfx);
         messageClient(%this, 'MsgMissingGems', "\c0You may not finish without all the " @ %item_name @ "s!");
      }
      else {
         if ($LB::LoggedIn && $LB::SuperChallengeMode) {
            %scoreTime = PlayGui.elapsedTime;
            %elapsedTime = PlayGui.elapsedTime - %client.penaltyTime + PlayGui.totalBonus;
            $Game::PenaltyTime = %client.penaltyTime;
            %bonusTime = PlayGui.totalBonus;

            // Not all missions have time qualifiers
            %qualified = MissionInfo.time? %scoreTime < MissionInfo.time: true;
            if (!%qualified) {
               %this.play2d(MissingGemsSfx);
               messageClient(%this, 'MsgMissingGems', "\c0You failed to qualify!");
               return;
            }
         }
         %this.player.setMode(Victory);
         messageClient(%this, 'MsgRaceOver', '\c0Congratulations! You\'ve finished!');
         endGameSetup();
      }
   }
}

function GameConnection::onLeavePad(%this)
{
   // Don't care if the leave
   // Jeff: thats right, we don't care.
}


//-----------------------------------------------------------------------------

function OOBCounter::check()
{
    %oobRandom[0] = "Let\'s be clear of the blatant truth: You suck!";
	%oobRandom[1] = "Honestly, do you have any control over the marble? It seems to have a life on its own...";
	%oobRandom[2] = "Are you sure you know how to play Marble Blast?";
	%oobRandom[3] = "You are contributing to the increasing water levels in the sea below you way too much!";
	%oobRandom[4] = "Look at the bright side, it\'s part of the learning experience, but it doesn\'t change the fact that you still suck.";
	%oobRandom[5] = "If we ever had a \'You suck\' achievement, you\'d be having the honour to wear it today.";
	%oobRandom[6] = "200 more times to go Out of Bounds before you see this message again. For your sake, try and do better.";
	%oobRandom[7] = "\"I didn\'t play on the computer! It...it was.. my auntie!\" Yeah, right. Admit it, you suck.";
	%oobRandom[8] = "Are you having fun going Out of Bounds all the time? It seriously looks like it.";
	%oobRandom[9] = "Don\'t you just hate all these messages that make a mockery of your suckiness? It\'s a joke of course, but it\'s a nice easter egg.\nIf you don\'t want to see them anymore, then stop going Out of Bounds so many times!";
	%oobRandom[10] = "My grandmother is better than you!";
	%oobRandom[11] = "We\'ll see what happens first: You finishing the level, or the clock hitting the 100 minute mark.";
	%oobRandom[12] = "Can we put this on the video show? I mean, that was absolutely stupid of you to go Out of Bounds like that!";
	%oobRandom[13] = "While we\'re on the subject of you going Out of Bounds, you should try and find out all the possible ways to go Out of Bounds, including the stupid ways which you seem to excel in.";
	%oobRandom[14] = "This level isn\'t made out completely out of tiny thin tightropes! You have no excuse whatsoever on failing this badly. If you see this message on Tightropes, Catwalks or Slopwropes, ignore it. Instead, change it to: hahahahahahahahahaha fail!";
	%oobRandom[15] = "Excuse of the Day: \"I was pushed Out of Bounds by an invisible Mega Marble!\"";
	%oobRandom[16] = "Congratulations, you win--- wait, no, no you don\'t. You went Out of Bounds. Sorry, you lose. Again.";
	%oobRandom[17] = "I found a way for you not to go Out of Bounds. We\'ll change the shape of the marble to a cube. Wait, never mind, you\'ll still find a way, because you can.";
	%oobRandom[18] = "You sure you played the beginner levels? You did? Doesn\'t look like it.";
	%oobRandom[19] = "You know what would be hilarious? This message popping up on \'Let\'s Roll\'. I hope you aren\'t playing that level right now... are you?";
	%oobRandom[20] = "Mind if we\'ll change your name to \'Mr. McFail?\'";
	%oobRandom[21] = "Excuse of the Day: \"But I was distracted by ________ and he/she/it wouldn\'t stop and forced me to go Out of Bounds.\"";
	%oobRandom[22] = "Which one are you: a bad player or a bad player? We willl go with option C: a really bad player.";
	%oobRandom[23] = "Excuse of the Day: WHO PUT THAT GRAVITY MODIFIER IN THERE??!?!";
	%oobRandom[24] = "Excuse of the Day: That In Bounds Trigger WAS NOT in the level last time I played it! Somebody hacked the level and put one in there!";
	%oobRandom[25] = "Excuse of the Day: My awesome marble was abducted by aliens and was replaced by a really crap one!";
	%oobRandom[26] = "Excuse of the Day: That Out of Bounds trigger was NOT there before! I swear!";
	%oobRandom[27] = "Excuse of the Day: I\'m not Pascal :(";
	%oobRandom[28] = "Excuse of the Day: I don\'t suck, I fell off because I wanted to get to the next 200 Out of Bounds multiplier so I can see the awesome messages that are written down.";
	%oobRandom[29] = "You know, you won\'t beat the level if you keep falling off. You will, however, see more of these messages. Try and stay on the level next time. Our guess is that you can\'t, because you\'re bad.";
	%oobRandom[30] = "Look at the statistics page! I bet you fell more times than the amount of levels you beat!";
	%oobRandom[31] = "Excuse of the Day: I\'m learning to play... the hard way.";
	%oobRandom[32] = "Apparently your marble isn\'t supermarble. It is suckmarble.";
	%oobRandom[33] = "Foo-Foo Marble laughs at how bad you are.";
	%oobRandom[34] = "A Rock Can Do Better!";
	%oobRandom[35] = "Please, Quit Embarassing Yourself.";
	%oobRandom[36] = "Keep this up and you\'ll win the \'Award of LOL\', courtesy of Marble Blast Fubar creators!";
	%oobRandom[37] = "Marble Blast Fubar creators would like to give you the title of \'Official NOOB of the Year\'. Congratulations!";
	%oobRandom[38] = "Did you hear that \'Practice Makes Perfect\'? Apparently not.";
	%oobRandom[39] = "You should create a new level and title it \'Learn the In Bounds and Out of Bounds Triggers\' because you\'re so experienced with them.";
	%oobRandom[40] = "We\'ve seen the ways you fell while playing this game and we gotta admit, some of their are epic fails. We still can\'t stop laughing!";
	%oobRandom[41] = "SING WITH ME:\n\nOne hundred and ninety nine times Out of Bounds, one hundred and ninety nine times Out of Bounds, throw the marble off the level, two hundred times Out of Bounds!";
	%oobRandom[42] = "*sigh*, you just can\'t stop yourself from going Out of Bounds, can you?";
	%oobRandom[43] = "Excuse of the Day: I\'m playing one of those special levels from Technostick where you must fall off in order to beat them.";
	%oobRandom[44] = "Excuse of the Day: I\'m having a bad karma today.";
	%oobRandom[45] = "Excuse of the Day: So THAT\'S what my astronomer referred to when he said I\'ll keep falling off today.";
	%oobRandom[46] = "What do you have against the marble that you keep making it fall off the level?!";
	%oobRandom[47] = "I bet you wish you had a Blast or an Ultra Blast powerup to save you. Perhaps even the World\'s Greatest Blast. Well, reality to player, reality to player: we don\'t have such a thing existing in this game, so stop playing so badly!";
	%oobRandom[48] = "And how is it OUR fault that you\'re playing so badly?";
	%oobRandom[49] = "Do you ever think about the marble\'s safety when you\'re playing? Apparently not because you\'re really careless with it.";






	%oobSpecial[0] = "You went Out of Bounds for 1,250 times. This program will now sit in the corner and cry about how bad you are and hope that when you open it again you won\'t repeat it. False hopes are still hopes.";
	%oobSpecial[1] = "You went Out of Bounds for 2,500 times. If you aren\'t tired of going Out of Bounds all the time, we sure did. Stop it already!";
	%oobSpecial[2] = "Another 1,250 marbles had fallen to the great sea below, and you\'ve reached the 3,750 Out of Bounds mark. You definitely suck. Ah yes, greenpeace would like to see you in court for your \"contribution\" to rising sea levels.";
	%oobSpecial[3] = "If I had a nickel for every marble that fell Out of Bounds I\'d be rich right now and all thanks to you. However, I\'m not going to give you any money. Instead, I\'ll stick my tongue out at you and then laugh at you. Ah yes, congratulations on hitting the 5,000 Out of Bounds mark.";
	%oobSpecial[4] = "6,750 times Out of Bounds. Let\'s assume, hypothetically, that you won\'t go Out of Bounds ever again. Actually, never mind that, you will still suck even if you don\'t go Out of Bounds again.";
	%oobSpecial[5] = "I have an awesome gut feeling that you are going 7,500 times Out of Bounds on purpose if only to see these messages and to hear about how bad you are.\nWell then, I won\'t keep it away from you.\nYou suck!";
	%oobSpecial[6] = "8,750 times Out of Bounds. For reaching this landmark, I\'m giving you a nice Australian Slang sentence to answer the question: Will you ever stop sucking in this game and go Out of Bounds? Answer:\nTill it rains in Marble Bar\n\n\nIn your language it means:\nNever.";
	%oobSpecial[7] = "Wow, you truly are bad, probably one of the worst Marble Blast players to ever live on this planet. Or you just keep failing to good runs. Are you sure you aren\'t playing an easy level while this message pops up? Whatever, those messages will now repeat themselves (with a few exceptions), but for now, please remember this:\n\n\nYOU suck!";
	%oobSpecial[8] = "SING WITH ME:\n\nForty nine thousand nine hundred and ninety nine times Out of Bounds, forty nine thousand nine hundred and ninety nine times Out of Bounds, knock a marble off the level, fifty thousand times Out of Bounds!";
	%oobSpecial[9] = "What\'s that in the sky? Is it a plane? Is it a bird? No! It\'s the marble! And it\'s way off the level!!! Congratulations on hitting 300,000 Out of Bounds mark. You may now suck more.";
	%oobSpecial[10] = "1,000,000 times Out of Bounds?!?! You seriously love this game, don\'t you? Well then, thanks for playing Marble Blast Platinum! Please keep this bad playing up and continue to go Out of Bounds. We\'ll just laugh at how bad you are. Also, this is the final message as from now on they\'re all repeats. Thank you for sucking at Marble Blast Platinum!";
	%oobSpecial[11] = "You have no life. This is official.";

	//HiGuy: So many unescaped apostrophes were making my IDE go bonkers. ESCAPED

	 switch($PREF::OOBCOUNT) {
		case 1250:
			ASSERT("Out of Bounds",%oobSpecial[0]);
			return 1;
		case 2500:
			ASSERT("Out of Bounds",%oobSpecial[1]);
			return 1;
		case 3750:
			ASSERT("Out of Bounds",%oobSpecial[2]);
			return 1;
		case 5000:
			ASSERT("Out of Bounds",%oobSpecial[3]);
			return 1;
		case 6250:
			ASSERT("Out of Bounds",%oobSpecial[4]);
			return 1;
		case 7500:
			ASSERT("Out of Bounds",%oobSpecial[5]);
			return 1;
		case 8750:
			ASSERT("Out of Bounds",%oobSpecial[6]);
			return 1;
		case 10000:
			ASSERT("Out of Bounds",%oobSpecial[7]);
			return 1;
		case 50000:
			ASSERT("Out of Bounds",%oobSpecial[8]);
			return 1;
		case 300000:
			ASSERT("Out of Bounds",%oobSpecial[9]);
			return 1;
		case 1000000:
			ASSERT("Out of Bounds",%oobSpecial[10]);
			return 1;
		case 30000000:
			ASSERT("Out of Bounds",%oobSpecial[11]);
			return 1;
	 }
// Matan: leave it here, we will implement it again if the community wants to.
// Spy47: Oh, no need to delete it completely. Just delete the "oobcrash" =)

	if($PREF::OOBCOUNT != 0 && $PREF::OOBCOUNT % 200 == 0)
        ASSERT("Out of Bounds" SPC $PREF::OOBCOUNT SPC "times",%oobRandom[getRandom(0,49)]);

}

//function oobcrash()
//{
//	schedule(500, 0, "oobassert");
//}

//function oobassert()
//{
//	ASSERT("Error Handler","Due to the Out of Bounds addon notifying us that you reached another 1,250 times that you //went Out of Bounds, Marble Blast Platinum will now co-operate with it and will now close.\n\nGoodbye.","quit();");
//}

function GameConnection::onOutOfBounds(%this)
{
   if ($Game::State $= "End")
      return;

   // Jeff: used for OOB Click in Multiplayer
   %this.isOOB = true;

   // Jeff: powerup code for multiplayer
   if ($Server::ServerType $= "Multiplayer") {
      %this.player.oldMPPowerupData = %this.player.powerUpData;
      %this.player.oldMPPowerupObj = %this.player.powerUpObj;

      // Jeff: keep second paramater false so that gyrocopter image and
      // stuff remains on it until reset is complete
      %this.player.setPowerUp(0, false);
   }

   // Reset the player back to the last checkpoint
   %this.oobMessageHack2();

   %this.play2d(OutOfBoundsVoiceSfx);
   %this.player.setOOB(true);

   %this.incrementOOBCounter(); //HiGuy: Moved to clientCmds

   if (!isEventPending(%this.respawnSchedule))
   {
      // If checkpointed, don't do READY, SET, GO.
      if(%this.checkpointed)
         %this.respawnSchedule = %this.schedule(2500, respawnonCheckpoint);
      else
         %this.respawnSchedule = %this.schedule(2500, respawnPlayer);
   }
}

function Marble::onOOBClick(%this)
{
   //if ($doRecordDemo)
      //return;
   //HiGuy: So it turns out, %this is actually the CLIENT marble, and not the
   // server marble. This makes things infinitely weirder, because no other
   // Marble:: methods call on the client object. We're going to have to use
   // LocalClientConnection here, as there is no alternative.

   // Jeff: i changed this to go because i deleted play, should work
   // fine now
   if (LocalClientConnection.state $= "Go" && $Server::ServerType $= "SinglePlayer")
   {
      // If checkpointed, don't do READY, SET, GO.
      if (LocalClientConnection.checkpointed)
         LocalClientConnection.call(respawnOnCheckpoint);
      else {
         // HAAACK - have to detach this call from within this scope - phil
         // The click behaviour can randomly get buggy - TODO
         // Jeff: replaced eval, makes it less hackish
         LocalClientConnection.call(respawnPlayer);
      }
   }

}

function GameConnection::onDestroyed(%this)
{
   if ($Game::State $= "End")
      return;

   // Reset the player back to the last checkpoint
   %this.setMessage("destroyed",2000);
   %client.play2d(DestroyedVoiceSfx);
   %this.player.setOOB(true);
   if(!isEventPending(%this.respawnSchedule))
      %this.respawnSchedule = %this.schedule(2500, respawnPlayer);
}

function GameConnection::onFoundGem(%this,%amount,%gem)
{
	if($CurrentGame $= "Gold")
		%item_name = "gem";
	else
		%item_name = "diamond";

   %this.gemCount += %amount;

   if ($Game::IsHunt) //HiGuy: Play the noise, the rest is already taken care of
      %this.play2d(GotGemSfx);
   else {
      %remaining = $Game::gemCount - %this.gemCount;
      if (%remaining <= 0) {
         messageClient(%this, 'MsgHaveAllGems', "\c0You have all the "@%item_name@"s, head for the finish!");
         %this.play2d(GotAllGemsSfx);
         %this.gemCount = $Game::GemCount;
      }
      else
      {
         if(%remaining == 1)
            %msg = "\c0You picked up a " @ %item_name @ "! Only one " @ %item_name @ " to go!";
         else
            %msg = "\c0You picked up a " @ %item_name @ "!  " @ %remaining @" "@%item_name@ "s to go!";

         messageClient(%this, 'MsgItemPickup', %msg, %remaining);
         %this.play2d(GotGemSfx);
      }
   }

   // Jeff: update the score list
   if ($Server::ServerType $= "Multiplayer") {
      updateScores();
      %count = ClientGroup.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %client = ClientGroup.getObject(%i);
         if (%client == %this)
            continue;

         // Jeff: update whiteout and sound
         if (isObject(%client.player))
            %client.player.setWhiteOut(0.05);
         %client.play2d(OpponentGemSfx);
      }
   }
   %this.setGemCount(%this.gemCount);
}

//-----------------------------------------------------------------------------

function GameConnection::spawnPlayer(%this, %spawnPoint)
{
   // Combination create player and drop him somewhere
   if (%spawnPoint $= "")
      %spawnPoint = %this.getCheckpointPos(0);
   %this.createPlayer(%spawnPoint);
   %this.updateGhostDatablock();
   %this.setGravityDir("1 0 0 0 -1 0 0 0 -1", true, "0 0 -1");

   if ($Server::ServerType $= "SinglePlayer")
      %this.play2d(spawnSfx);
}

function restartLevel() {
   onMissionReset();
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      //HiGuy: HACK - Always restart the demo if they press the restart button
      if ($doRecordDemo)
         ClientGroup.getObject(%i).respawns = 999;
      ClientGroup.getObject(%i).restartLevel();
   }
}

// Helper method - phil
function restartDemo() {
   // Jeff: make use of this, don't try to play an empty mission
   %mission = $Server::MissionFile;

   // Kill the server
   disconnect();

   canvas.setContent(LoadingGui);
   LOAD_MapName.setText("<font:DomCasualD:24><just:center>Restarting Mission...");

   // Jeff: give it time to disconnect, i think this is why it is crashing
   //trace(true);
   schedule(1000,0,"resetDemoServer",%mission);
}

// Jeff: start up the demo recorder again...extra helper
function resetDemoServer(%mission) {
   MarbleSelectGui.update();

	$playingDemo = false; //HiGuy: For some reason, this isn't reset.

   while (!$Server::Hosting && isObject(ServerConnection))
      ServerConnection.delete();

   // Start it up again
   // Rerecord the demo - disconnectedCleanup() in ~/client/scripts/serverconnection.cs stops the demo
   recordDemo("~/client/demos/" @ $recordDemoName @ ".rec", %mission);

   createServer("SinglePlayer", %mission);
   loadMission(%mission, true);
   %conn = new GameConnection(ServerConnection);
   RootGroup.add(ServerConnection);
   %conn.setConnectArgs($LB::Username, -1, "", "", "bologna");
//   %conn.setJoinPassword($Client::Password);
   %conn.connectLocal();
   //trace(false);
}

function GameConnection::quickRespawnPlayer(%this) {
   %this.player.oldMPPowerupData = %this.player.powerUpData;
   %this.player.oldMPPowerupObj = %this.player.powerUpObj;

   // Jeff: keep second paramater false so that gyrocopter image and
   // stuff remains on it until reset is complete
   %this.player.setPowerUp(0, false);

   //HiGuy: So... they want to quick respawn, do they?
   // They're not getting off *that* easy. No spawn abusing!
   %this.quickRespawning = true;
   %this.respawnPlayer();
   %this.quickRespawning = false;
}

function GameConnection::respawnPlayer(%this, %respawnPos)
{
   if (%this.fake)
      return;

	if ($Game::Playback) {
		//Restart the playback
		%path = $Game::PlaybackPath;
		cancelPlayback();
		playRecording(%path);
	}
	if ($Game::Recording) {
		//Restart the playback
		cancelRecording();
		startRecording($Server::MissionFile);
	}

   %this.respawns ++;

   // If we're recording a demo, let's reload the level - phil
   // It's okay that we stick this here, since you can't respawn
   // from checkpoints
   if ($doRecordDemo) {
      if (%this.respawns >= $pref::recordDemoOOBs) {
         %this.respawns = 0;
         restartDemo();
         return;
      }
   }

   cancel(%this.respawnSchedule);

   if ($Server::ServerType $= "MultiPlayer" && %this.spawningBlocked()) {
      error("Spawning blocked for client:" SPC %this);
      %this.respawnSchedule = %this.schedule(300, "respawnPlayer");
      return;
   }

   // Jeff: challenge attempts mode
   if ($LB::LoggedIn && $LB::ChallengeMode && $LB::Challenge::Type $= "attempts")
      sendChallengeAttemptsUpdate();

   // Jeff: used for OOB Click in Multiplayer
   %this.isOOB = false;

   //HiGuy: Reset mega marble
   %this.player.megaMarble = false;
   cancel(%this.player.megaSchedule);
   %this.updateGhostDatablock();

   //HiGuy: I figure we won't have more than 100 powerUpIds
   for (%i = 0; %i < 100; %i ++) {
   	%this.deactivatePowerup(%i);
      cancel(%this.player.powerupSchedule[%i]);
      cancel(%this.unmount[%i]);
   }

   // Reset the player back to the last checkpoint
   if (!$Game::IsHunt) {
      if (!$Game::isFree) {
         %this.player.setMode(Start);
      	if ($Server::ServerType $= "MultiPlayer")
      		%this.setGameState("start");
      	else
				onMissionReset();
      }
   }
   %this.player.setOOB(false);
   if (%respawnPos $= "")
      %respawnPos = %this.getCheckpointPos(0);
   //fwrite($usermods @ "/pos.txt", %respawnPos);
   devecho("Respawning at" SPC %respawnPos);
   %this.player.setPosition(getField(%respawnPos, 0), getField(%respawnPos, 2));
   %this.setGravityDir(VectorOrthoBasis(getField(%respawnPos, 1)), true, getField(%respawnPos, 1));

   if ($Server::ServerType $= "MultiPlayer") {
      for (%i = 0; %i < 3; %i ++) {
         %this.player.unmountImage(%i);
         %this.ghost.unmountImage(%i);
      }

      if ($MP::TeamMode) {
         %this.player.mountImage(TeamRingImage, 500, true, "1");
      }

      // Jeff: update powerup!
      //echo("reseting powerup");
      //echo(%this.player.powerUpData);

      %this.pointToNearestGem();
   }

   %this.player.setPowerUp("", true);

   if (!$Game::IsHunt) {
      %this.player.setPowerUp(%this.checkPointPowerUp,true);

      %this.gemCount = %this.checkPointGemCount;
      %this.penaltyTime = %this.checkPointPenaltyTime;
      %this.bonusTime = %this.checkPointBonusTime;

      %this.setTime(%this.checkPointTime);
   }

   if ($Server::ServerType $= "MultiPlayer")
      %this.player.powerupRespawn = %this.player.schedule($powerupDelay, "setPowerUp", %this.player.oldMPPowerupData, true, %this.player.oldMPPowerupObj);

   %this.checkpointed = 0; // Spy47 : Reset checkpoint counter.
   %this.curCheckpoint = 0;
   %this.curCheckpointNum = 0;

   %this.checkPointPad = $Game::StartPad;
	%this.checkPointTime = ($Game::IsHunt ? (MissionInfo.time ? MissionInfo.time : 300000) : 0);
	%this.checkPointGemCount = 0;
	%this.checkPointPenaltyTime = 0;
	%this.checkPointBonusTime = 0;
	%this.checkPointPowerUp = 0;

	commandToClient(%this, 'GameRespawn');

   %this.setGemCount(%this.gemCount);

   if ($Server::Lobby)
      return;
   %this.play2d(spawnSfx);

   return %respawnPos;
}

function GameConnection::restartLevel(%this) {
   %this.player.oldMPPowerupData = "";
   %this.player.oldMPPowerupObj = "";

   // Jeff: keep second paramater false so that gyrocopter image and
   // stuff remains on it until reset is complete
   %this.player.setPowerUp(0, false);

   %this.resetStats();

   if (!%this.spectating) {
      %this.restarting = true;
      %this.respawnPlayer(%this.spawnPoint);
      %this.spawnPoint = "";
      %this.restarting = false;
      %this.setToggleCamera(false);
   }

   // Jeff: hack, reset gamestates to start
   setGameState("start");
}

//-----------------------------------------------------------------------------

function getMarbleChoice() {
   if ($playingDemo)
      return $demoMarble;
   else if ($LB::LoggedIn && $LB::Username !$= "")
      return LBMarbleSelectionDlg.getSelection();
   else
      return MarbleSelectGui.getSelection();
}

function GameConnection::createPlayer(%this, %spawnPoint)
{
   if (%this.player > 0)  {
      // The client should not have a player currently
      // assigned.  Assigning a new one could result in
      // a player ghost.
      error( "Attempting to create an angus ghost!" );
   }
   if ($Server::Dedicated) {
      %db = "LBDefaultMarble";
      %skin = "base";
   } else if ($LB::LoggedIn && $LB::Username !$= "") {
      %db = $LB::MarbleDatablock[getField(getMarbleChoice(), 1)];
      %skin = getField(getMarbleChoice(), 2);

      //echo(%db SPC %skin);
      //echo("");
   } else {
      %marbleSelection = getMarbleChoice();
      %shape = getField(%marbleSelection, 0);
      %skin = getField(%marbleSelection, 1);

      %db = "DefaultMarble";
      if ((%skin $= "base" && filePath(%shape) !$= "ball-superball") || filePath(%shape) !$= $usermods @ "/data/shapes/balls/")
         %db = "CustomMarble";

      %db.shapeFile = %shape;
   }

   %scale = "1 1 1";
   // Jeff: this code is used for ghosting movement and scaling for MP
   if ($Server::ServerType $= "MultiPlayer") {
      %scale = 1 - ($MP::SpawnCount / 10000);
      %scale = %scale SPC %scale SPC %scale;
      %this.scale = %scale;
      $MP::SpawnCount ++;

      devecho("Spawning client from ip: " SPC %this.getAddress());
      devecho("Setting scale to \"" @ %scale @ "\"");

      // Jeff: create the ghost
      %this.createGhost();
      commandToClient(%this, 'GetMyScale', %scale);
      commandToAll('GhostIndex', %this.scale, %this.index);
   }

   // Jeff: create marble
   %player = new Marble() {
      dataBlock = %db;
      client = %this;
      scale = %scale;
   };
   // Set the skin based on the player's skin defined above
   %player.setSkinName(%skin);
   MissionCleanup.add(%player);

   // Jeff: this creates the trail emitters, since we are using new ones for
   // the 1.50 update.  If we are in multiplayer, then we have to use the player
   // scale, other wise for single player i hacked it to use a set-in-stone
   // scale to be used. Scale is already set above so we use that for MP
   //

   if ($Server::ServerType $= "SinglePlayer") {
      %scale = "0.9998 0.9998 0.9998";
      %whiteScale = "0.9997 0.9997 0.9997";
   } else
      %whiteScale = %scale;

   %emit = new ParticleEmitterNode() {
      datablock = ParticleTrailNode;
      emitter   = MarbleTrailEmitter;
      position  = "-9999999 -9999999 -99999999";
      rotation  = "1 0 0 0";
      scale     = %scale;
   };
   MissionCleanup.add(%emit);
   %this.trailEmitter = %emit;

   // Jeff: create the white emitter too
   %white = new ParticleEmitterNode() {
      datablock = ParticleWhiteTrailNode;
      emitter   = MarbleWhiteTrailEmitter;
      position  = "-9999999 -9999999 -99999999";
      rotation  = "1 0 0 0";
      scale     = %whiteScale;
   };
   MissionCleanup.add(%white);
   %this.trailWhiteEmitter = %white;

   // Jeff: if we are single player, then we have to attach it to our marble
   // multiplayer code already does that for us however.
   if ($Server::ServerType $= "SinglePlayer")
      schedule(50, 0, attachMarbleTrail);

   // Jeff: this is used to restore the player from mega marble
   %this.origionalDataBlock = %db;

	//echo("PLAYER IS " @ %player);
   // Player setup...
   %player.setPosition(getField(%spawnPoint, 0), getField(%spawnPoint, 2));

   // Update the camera to start with the player
   %this.camera.setTransform(%player.getEyeTransform());

   // Give the client control of the player
   %this.player = %player;
   %this.setControlObject(%player);
}

//-----------------------------------------------------------------------------
// Support functions
//-----------------------------------------------------------------------------

function setAllGravityDirs(%dir, %reset, %rot) {
   %obj = ClientGroup.getCount();
   for (%i = 0; %i < %obj; %i ++)
      ClientGroup.getObject(%i).setGravityDir(%dir, %reset, %rot);
}

//-----------------------------------------------------------------------------

function countGems(%group)
{
   // Count up all gems out there are in the world
   %gems = 0;
   %count = %group.getCount();
   for (%i = 0; %i < %count; %i++)
   {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
      if (%type $= "SimGroup")
         %gems += countGems(%object);
      else
         if (%type $= "Item" &&
               %object.getDatablock().classname $= "Gem")
            %gems++;
   }
   return %gems;
}


function GameConnection::pointToNearestGem(%this) {
	if (!isObject(%this.player))
		return;
   %pos = getWords(%this.player.getTransform(), 0, 2);
   %nearest = %this.getNearestGem();
   if (%nearest == -1)
      return error("No gems found, or all gems too far away (d > 1,000,000)");

   %dist = VectorSub(getWords(%nearest.getTransform(), 0, 2), %pos);

   %angle = mAtan(getWord(%dist, 0), getWord(%dist, 1));

   %dist = setWord(%dist, 2, getWord(%dist, 2) - 2);
   %hypo = VectorLen(%dist);
   %pitch = -mAsin((getWord(%dist, 2) / %hypo) * 0.7);

   //addhelpline(%pitch);

   %this.player.setPosition(getWords(%this.player.getTransform(), 0, 2) SPC "0 0 1" SPC %angle, %pitch);
}

function GameConnection::getNearestGem(%this) {
   %nearest = -1;
   %nearDist = 999999;
   %pos = getWords(%this.player.getTransform(), 0, 2);
   MakeGemGroup(ServerConnection, true);
   for (%i = 0; %i < $GemsCount; %i ++) {
      %gem = $Gems[%i];
      if (%gem.isHidden())
         continue;
      %dist = VectorDist(getWords(%gem.getTransform(), 0, 2), %pos);
      if (%dist < %nearDist) {
         %nearest = %gem;
         %nearDist = %dist;
      }
   }
   return %nearest;
}

function getActivePlayerCount() {
   %players = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.fake)
         continue;
      if (%client.spectating)
         continue;
      %players ++;
   }
   return %players;
}
