//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// PlayGui is the main TSControl through which the game is viewed.
// The PlayGui also contains the hud controls.
//-----------------------------------------------------------------------------

function PlayGui::onWake(%this)
{
	// Performance - Phil
	activatePackage(frameAdvance);
   %this.doFPSCounter();

   // Turn off any shell sounds...
   // alxStop( ... );

	// Set the player's FOV - only place here - Phil
	%this.forceFOV = $pref::Player::defaultFov;

   $PlayGuiGem = true;
   $InPlayGUI = true;

   // Jeff: this screws my computer up, making this a pref
   if ($Pref::EnableDirectInput)
      $enableDirectInput = "1";
   activateDirectInput();

   // Message hud dialog
   //Canvas.pushDialog( MainChatHud );
   //chatHud.attach(HudMessageVector);

   // Make sure the display is up to date
   %this.setGemCount(%this.gemCount);
   %this.setMaxGems(%this.maxGems);
   %this.timerInc = 50;

   // just update the action map here
   if ($playingDemo)
      demoMap.push();
   else
      moveMap.push();

	// Check if enable showing FPS - Phil
   FPSMetreCtrl.setVisible($pref::showFPSCounter);

   // hack city - these controls are floating around and need to be clamped
   schedule(0, 0, "refreshCenterTextCtrl");
   schedule(0, 0, "refreshBottomTextCtrl");
   playGameMusic();

   LagIcon.setVisible(false);

   PG_Timer.setVisible(true);
   showSpectatorMenu(false);

   PG_LBChatEntry.setTickable(true);

   // Jeff: leaderboard support code
   // make the marble match what marble you are using
   PG_AttemptsContainer.setVisible(false);

   if ($LB::LoggedIn && $LB::username !$= "") {
      // Jeff: decide which mode we have to set
      if ($LB::SuperChallengeMode)
         LBSetMode(5);
      else if ($LB::ChallengeMode)
         LBSetMode(4);
      else if ($Server::ServerType $= "MultiPlayer" && $Server::Hosting)
         LBSetMode(6);
      else
         LBSetMode(2);

      // Jeff: additional stuff for challenge mode attempts only
      if ($LB::ChallengeMode && $LB::Challenge::Type $= "attempts")
         %this.updateAttempts();

      Canvas.pushDialog(LBMessageHudDlg);

      disableChatHUD();
      PG_LBChatScroll.setVisible(true);
      LBscrollChat();
   } else {
      PG_LBChatScroll.setVisible(false);
      Canvas.popDialog(LBMessageHudDlg);
      PG_AttemptsContainer.setVisible(false);
   }

   // Jeff: if we are multiplayer, show the pregame dialog
   %multiplayer = ($Server::ServerType $= "Multiplayer");
   if (%multiplayer && $Game::Pregame) {
      Canvas.pushDialog(MPPreGameDlg);
      %this.setTime(0);
      %this.stopTimer();
      %this.updateBlastBar();
      resetScoreList();
   }
   PGScoreListContainer.setVisible(%multiplayer && !$Game::IsFree);
   PG_BlastBar.setVisible(%multiplayer && !$MPPref::DisableBlast);
   RadarToggle($Game::RadarMode);

   %this.positionMessageHud();
   %this.updateGems();

   // Jeff: update the fader status
   //       this variable is set true in missiondownload.cs
   PG_Fader.setVisible($PlayGuiFader);
   $PlayGuiFader = false;

   if ($Server::_Dedicated) {
   	//HiGuy: My ears, my ears!

   	//HiGuy: Better version only affects game sounds
   	alxSetChannelVolume($GameAudioType, $pref::Audio::channelVolume1);
   }
}

function PlayGui::onSleep(%this)
{
	// Performance - Phil
	if (!$clampFps)
   	deactivatePackage(frameAdvance);
   %this.stopFPSCounter();

	$InPlayGUI = false;

   //Canvas.popDialog(MainChatHud);
   // Terminate all playing sounds
   alxStopAll();

   //HiGuy: Play the right song!
   if ($LB::LoggedIn && $LB::Username !$= "")
      playLBMusic();
   else
      playShellMusic();

   RadarToggle(0);
   Radar::ShowDots(false);
   Radar::ClearTargets();

   // pop the keymaps
   moveMap.pop();
   demoMap.pop();
}

function PlayGui::resetAllTime(%this) {
   %this.allTime = 0;
   %this.totalTime = 0;
}

// The FPS counter only updates per second now - there is no need to repeatedly be setting
// the text on frame advance since it's just wasting CPU and could possibly be contributing
// to the crashing going on - phil

function PlayGui::stopFPSCounter(%this) {
   cancel(%this.fpsCounterSched);
}

// Just more of a shorthand
function PlayGui::doFPSCounter(%this) {
   %pingnum = "high";
   if (ServerConnection.getPing() >= 100) %pingnum = "medium";
   if (ServerConnection.getPing() >= 250) %pingnum = "low";
   if (ServerConnection.getPing() >= 500) %pingnum = "matanny";
   if (ServerConnection.getPing() >= 1000) %pingnum = "unknown";
   %fps = $fps::real;
   if (%fps >= 100) %fps = mRound(%fps) @ " ";
   FPSMetreText.setText("<font:DomCasualD:24><just:left>FPS:" SPC %fps @ ($Server::ServerType $= "MultiPlayer" ? "<bitmap:" @ $usermods @ "/leaderboards/play/connection-" @ %pingnum @ ".png>" : ""));
   cancel(%this.fpsCounterSched);
   %this.fpsCounterSched = %this.schedule(1000, doFPSCounter);
}

//-----------------------------------------------------------------------------

function PlayGui::setMessage(%this,%bitmap,%timer)
{
   // Set the center message bitmap
   if (%bitmap !$= "")  {
      CenterMessageDlg.setBitmap($userMods @ "/client/ui/game/" @ %bitmap @ ".png",true);
      CenterMessageDlg.setVisible(true);
      cancel(CenterMessageDlg.timer);
      if (%timer)
         CenterMessageDlg.timer = CenterMessageDlg.schedule(%timer,"setVisible",false);
   }
   else
      CenterMessageDlg.setVisible(false);
}


//-----------------------------------------------------------------------------

function PlayGui::setPowerUp(%this,%shapeFile)
{
   // Update the power up hud control
   if (%shapeFile $= "")
      HUD_ShowPowerUp.setEmpty();
   else
      HUD_ShowPowerUp.setModel(%shapeFile, "");
}


//-----------------------------------------------------------------------------

function PlayGui::setMaxGems(%this,%count)
{
   %this.maxGems = %count;
   %this.updateGems();
}

function PlayGui::setGemCount(%this,%count,%green)
{
   %this.gemCount = %count;
   %this.gemGreen = %green;
   %this.updateGems();
}

function PlayGui::updateGems(%this) {
   %count = %this.gemCount;
   %max = %this.maxGems;

   PG_GemCounter.setVisible(%max && !$Game::IsHunt && !$Game::IsFree);
   PG_HuntCounter.setVisible($Game::IsHunt && !$Game::IsFree);

   PG_Timer.setVisible(!$Game::isFree);

   if (!%max && !$Game::IsHunt) return;

   if ($PlayGuiGem) {
      %skins = "base black blue green orange platinum purple red turquoise yellow";
      HUD_ShowGem.setModel($usermods @ "/data/shapes/items/gem.dts", getWord(%skins, getRandom(0, getWordCount(%skins))));
      Hunt_ShowGem.setModel($usermods @ "/data/shapes/items/gem.dts", getWord(%skins, getRandom(0, getWordCount(%skins))));
      $PlayGuiGem = false;
   }

   if ($Game::IsHunt) {
      %one = %count % 10;
      %ten = ((%count - %one) / 10) % 10;
      %hundred = ((%count - %one - (%ten * 10)) / 100) % 10;
      %thousand = ((%count - %one - (%ten * 10) - (%hundred * 100)) / 1000) % 10;

      %pfx = %this.gemGreen ? "_green" : "";

      HuntGemsFoundOne.setVisible(true);
      HuntGemsFoundTen.setVisible(true);
      HuntGemsFoundHundred.setVisible(true);
      HuntGemsFoundThousand.setVisible(true);

      if (%count < 10) {
         HuntGemsFoundTen.setVisible(false);
         HuntGemsFoundHundred.setVisible(false);
         HuntGemsFoundThousand.setVisible(false);

         HuntGemsFoundOne.setTimeNumberPfx(%one, %pfx);
      } else if (%count < 100) {
         HuntGemsFoundHundred.setVisible(false);
         HuntGemsFoundThousand.setVisible(false);

         HuntGemsFoundOne.setTimeNumberPfx(%one, %pfx);
         HuntGemsFoundTen.setTimeNumberPfx(%ten, %pfx);
      } else if (%count < 1000) {
         HuntGemsFoundThousand.setVisible(false);

         HuntGemsFoundOne.setTimeNumberPfx(%one, %pfx);
         HuntGemsFoundTen.setTimeNumberPfx(%ten, %pfx);
         HuntGemsFoundHundred.setTimeNumberPfx(%hundred, %pfx);
      } else {
         HuntGemsFoundOne.setTimeNumberPfx(%one, %pfx);
         HuntGemsFoundTen.setTimeNumberPfx(%ten, %pfx);
         HuntGemsFoundHundred.setTimeNumberPfx(%hundred, %pfx);
         HuntGemsFoundThousand.setTimeNumberPfx(%thousand, %pfx);
      }
   } else {
      %one = %count % 10;
      %ten = (%count - %one) / 10;
      GemsFoundTen.setTimeNumberPfx(%ten, %pfx);
      GemsFoundOne.setTimeNumberPfx(%one, %pfx);

      %one = %max % 10;
      %ten = (%max - %one) / 10;
      GemsTotalTen.setTimeNumberPfx(%ten, %pfx);
      GemsTotalOne.setTimeNumberPfx(%one, %pfx);
   }
}

// Jeff: update challenge mode attempts, direct copy from
// the update gems
function PlayGui::updateAttempts(%this) {
   %count = $LB::Challenge::CurrentAttempts;
   %max   = $LB::Challenge::MaxAttempts;
   %count = (%count > %max) ? %max : %count;
   %one = %count % 10;
   %ten = (%count - %one) / 10;
   PG_AttemptsTen.setNumber(%ten);
   PG_AttemptsOne.setNumber(%one);

   %one = %max % 10;
   %ten = (%max - %one) / 10;
   PG_AttemptsMaxTen.setNumber(%ten);
   PG_AttemptsMaxOne.setNumber(%one);
}

//-----------------------------------------------------------------------------
// Blast Bar

function PlayGui::setBlastValue(%this, %value) {
   %this.blastValue = %value;
   %this.updateBlastBar();
}

function PlayGui::updateBlastBar(%this) {
   PG_BlastBar.setVisible($Server::ServerType $= "MultiPlayer" && !$MPPref::DisableBlast && !$SpectateMode);

   //Empty: 5 5 0   17
   //Full:  5 5 110 17
   //Partial: 5 5 (total * 110) 17
   PG_BlastFill.resize(5, 5, %this.blastValue * 110, 17);
   %oldBitmap = PG_BlastFrame.bitmap;
   %newBitmap = $usermods @ "/client/ui/game/blastbar";
   if ($MP::SpecialBlast)
      %newBitmap = %newBitmap @ "_charged";
   if (%oldBitmap !$= %newBitmap)
      PG_BlastFrame.setBitmap(%newBitmap);

   %oldBitmap = PG_BlastFill.bitmap;
   %newBitmap = $usermods @ "/client/ui/game/blastbar_bar";
   if (%this.blastValue >= $MP::BlastRequiredAmount)
      %newBitmap = %newBitmap @ "green";
   else
      %newBitmap = %newBitmap @ "gray";
   if (%oldBitmap !$= %newBitmap)
      PG_BlastFill.setBitmap(%newBitmap);
}

//-----------------------------------------------------------------------------
// Elapsed Timer Display

function PlayGui::setTime(%this,%dt)
{
   %this.elapsedTime = %dt;
   %this.updateControls();
}

function PlayGui::resetTimer(%this,%dt)
{
   $PlayTimerPrefix = "_green";
   $PlayerTimerFailedText = false;
   $PlayerTimerAlarmText = false;
   %this.elapsedTime = 0;
   %this.bonusTime = 0;
   %this.totalBonus = 0;
   %this.totalTime = $LB::SuperTotalTime;
   $MP::BlastValue = 0;
   if($BonusSfx !$= "")
   {
      alxStop($BonusSfx);
      $BonusSfx = "";
   }

   PG_Timer.setVisible(true);

   %this.updateControls();
   %this.stopTimer();
}

function PlayGui::adjustTimer(%this,%dt)
{
   if ($Game::IsHunt) {
      %this.totalTime += %dt;
      %this.elapsedTime += %dt;
   } else {
      %this.totalTime += %dt;
      %this.elapsedTime += %dt;
   }
   %this.updateControls();
}

function PlayGui::addBonusTime(%this, %dt)
{
   %this.bonusTime += %dt;
   if($BonusSfx $= "" && !alxIsPlaying($PlayTimerAlarmHandle))
      $BonusSfx = alxPlay(TimeTravelLoopSfx);
}

function PlayGui::refreshRed(%this) {
	if($PlayTimerActive && $InPlayGUI)
	{
		if(%this.bonusTime || $Editor::Opened)
			$PlayTimerPrefix = "_green";
		else {
         if ($Game::IsHunt) {
            $PlayTimerPrefix = "";
            if (%this.ElapsedTime <= $PlayTimerAlarmStartTime && %this.ElapsedTime > 0) {
               if (!alxIsPlaying($PlayTimerAlarmHandle))
                  $PlayTimerAlarmHandle = alxPlay(TimerAlarm);

               if (!$PlayerTimerAlarmText) {
                  addHelpLine("You have " @ ($PlayTimerAlarmStartTime / 1000) @ " seconds left.", true);
                  $PlayerTimerAlarmText = true;
               }
               $PlayTimerPrefix = (((%this.ElapsedTime / 1000) % 2) ? "_red" : "");
            } else if (%this.elapsedTime == 0) {
               if (alxIsPlaying($PlayTimerAlarmHandle))
                  alxStop($PlayTimerAlarmHandle);
               $PlayTimerPrefix = "_green";
            }
         } else {
            if (!MissionInfo.time || %this.ElapsedTime < (MissionInfo.time - $PlayTimerAlarmStartTime))
               $PlayTimerPrefix = "";
            else if (%this.ElapsedTime >= (MissionInfo.time - $PlayTimerAlarmStartTime) && %this.ElapsedTime < MissionInfo.time) {
               if (!alxIsPlaying($PlayTimerAlarmHandle))
                  $PlayTimerAlarmHandle = alxPlay(TimerAlarm);

               if (!$PlayerTimerAlarmText) {
                  addHelpLine("You have " @ ($PlayTimerAlarmStartTime / 1000) @ " seconds left.", true);
                  $PlayerTimerAlarmText = true;
               }

                  $PlayTimerPrefix = (((%this.ElapsedTime / 1000) % 2) ? "_red" : "");
            } else {
               if (alxIsPlaying($PlayTimerAlarmHandle))
                  alxStop($PlayTimerAlarmHandle);

               if (!$PlayerTimerFailedText) {
                  addHelpLine("The clock has passed the Par Time - please retry the level.", true);
                  alxPlay(TimerFailed);
                  $PlayerTimerFailedText = true;
               }
               $PlayTimerPrefix = "_red";
            }
         }
      }

		%this.schedule(100,"refreshRed");
	}
}

function PlayGui::startTimer(%this) {
   $PlayTimerActive = true;
   if(MissionInfo.alarmStartTime)
   		$PlayTimerAlarmStartTime = MissionInfo.AlarmStartTime * 1000;
	else
		$PlayTimerAlarmStartTime = 10000;
   %this.refreshRed();
}

// -----------------------------------------------------
// Doing this to hopefully save some CPU usage - Phil
package frameAdvance
{
	function onFrameAdvance(%timeDelta)
	{
	   if ($clampFps) {
	      if (%timeDelta < 1000 / $clampFps)
	         lag((1000 / $clampFps) - %timeDelta);
	   }

      Parent::onFrameAdvance(%timeDelta);

      // Jeff: adjust yaw
      $marbleYaw += $mvYawLeftSpeed;
      $marbleYaw -= $mvYawRightSpeed;
      $pitch += $mvPitchUpSpeed;
      $pitch -= $mvPitchDownSpeed;

      // Jeff: wrap yaw between -pi and pi
      while ($marbleYaw > $pi)
         $marbleYaw -= $tau;
      while ($marbleYaw < -$pi)
         $marbleYaw += $tau;

      //HiGuy: Engine-defined max/min pitch vars
      if ($pitch < -0.95)
         $pitch = -0.95;
      if ($pitch > 1.5)
         $pitch = 1.5;

	   //HiGuy: For LB Super Challenges
	   if ($LB::LoggedIn && ($LB::SuperChallengeMode || $LB::ChallengeMode) && !$LB::Kill) {
   	   PlayGui.updateControls();
   	   if ($LB::SuperChallengeMode)
            LBSCD_UpdateProgress();
         if ($LB::ChallengeMode)
   	      updateChallengeGemStatus();
	   }
		if ($mvTriggerCount0 & 1 && (!$Server::Hosting || $Server::_Dedicated) && $Server::ServerType $= "MultiPlayer") {
			if ($MP::MyMarble._powerUpId && $MP::FastPowerups)
				$MP::MyMarble.onPowerUpUsed();
			PlayGui.setPowerup("");
		}

	   if (Canvas.getContent().getName() $= "PlayGui")
         PlayGui.updateMessageHud();

		if($PlayTimerActive) {
			PlayGui.updateTimer(%timeDelta);

         if ($Server::ServerType $= "MultiPlayer") {
            // Jeff: blast code update
            $MP::BlastValue += (%timeDelta / $MP::BlastChargeTime);
            if ($MP::BlastValue > 1)
               $MP::BlastValue = 1;
            if ($MP::BlastValue < 0)
               $MP::BlastValue = 0;
            PlayGui.setBlastValue($MP::BlastValue);
         }
		}

		// Jeff: marble trails, single player only!
		if ($Server::ServerType $= "SinglePlayer")
		   attachMarbleTrail(%timeDelta);
      else if ($SpectateMode)
         interpolateCamera(%timeDelta);
	}
};
if ($clampFps)
   activatePackage(frameAdvance);

// -----------------------------------------------------

function PlayGui::stopTimer(%this)
{
   $PlayTimerPrefix = "_green";
   if(alxIsPlaying($PlayTimerAlarmHandle))
		alxStop($PlayTimerAlarmHandle);

   $PlayTimerActive = false;
   %this.updateControls();
   if($BonusSfx !$= "")
   {
      alxStop($BonusSfx);
      $BonusSfx = "";
   }
}

function PlayGui::updateTimer(%this, %timeInc)
{
   %this.totalTime += %timeInc;
   %this.allTime += %timeInc;
   if(%this.bonusTime)
   {
      if(%this.bonusTime > %timeInc)
      {
         %this.bonusTime -= %timeInc;
         %this.totalBonus += %timeInc;
         %timeInc = 0;
      }
      else
      {
         %timeInc -= %this.bonusTime;
         %this.totalBonus += %this.bonusTime;
         %this.bonusTime = 0;
         alxStop($BonusSfx);
         $BonusSfx = "";
      }
   }
   if ($Game::IsHunt)
      %this.elapsedTime -= %timeInc;
   else
      %this.elapsedTime += %timeInc;

   // Some sanity checking
   if (%this.elapsedTime > 5998999)
      %this.elapsedTime = 5998999;

   if ($Game::IsHunt && %this.elapsedTime <= 0 && !$Editor::Opened) {
      %this.elapsedTime = 0;
      if ($Server::Hosting && !$MP::Restarting)
         commandToServer('GameEnd');
   }
   if (%this.elapsedTime > 0)
	   schedule(1000, 0, setVariable, "$MP::Restarting", false);

   %this.updateControls();
}

function PlayGui::updateControls(%this)
{
   %et = %this.elapsedTime;
   %drawNeg = false;
   if(%et < 0)
   {
      %et = - %et;
      %drawNeg = true;
   }

   %hundredth = mFloor((%et % 1000) / 10);
   %totalSeconds = mFloor(%et / 1000);
   %seconds = %totalSeconds % 60;
   %minutes = (%totalSeconds - %seconds) / 60;

   %secondsOne      = %seconds % 10;
   %secondsTen      = (%seconds - %secondsOne) / 10;
   %minutesOne      = %minutes % 10;
   %minutesTen      = (%minutes - %minutesOne) / 10;
   %hundredthOne    = %hundredth % 10;
   %hundredthTen    = (%hundredth - %hundredthOne) / 10;

   // Update the controls
   Min_Ten.setTimeNumber(%minutesTen);
   Min_One.setTimeNumber(%minutesOne);
   Sec_Ten.setTimeNumber(%secondsTen);
   Sec_One.setTimeNumber(%secondsOne);
   Sec_Tenth.setTimeNumber(%hundredthTen);
   Sec_Hundredth.setTimeNumber(%hundredthOne);
   PG_NegSign.setVisible(%drawNeg);

   MinSec_Colon.setTimeNumber("colon");
   MinSec_Point.setTimeNumber("point");
}


//-----------------------------------------------------------------------------

function GuiBitmapCtrl::setNumber(%this,%number)
{
		%this.setBitmap($userMods @ "/client/ui/game/numbers/" @ %number @ ".png");
}
function GuiBitmapCtrl::setTimeNumber(%this,%number)
{
		%this.setBitmap($userMods @ "/client/ui/game/numbers/" @ %number @ $PlayTimerPrefix @ ".png");
}

function GuiBitmapCtrl::setTimeNumberPfx(%this,%number,%prefix)
{
		%this.setBitmap($userMods @ "/client/ui/game/numbers/" @ %number @ %prefix @ ".png");
}

//-----------------------------------------------------------------------------

function refreshBottomTextCtrl()
{
   BottomPrintText.position = "0 0";
}

function refreshCenterTextCtrl()
{
   CenterPrintText.position = "0 0";
}

//-----------------------------------------------------------------------------
