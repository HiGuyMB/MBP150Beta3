//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

if ( isObject( moveMap ) )
   moveMap.delete();
new ActionMap(moveMap);

if ( isObject( demoMap) )
   demoMap.delete();
new ActionMap(demoMap);

if ( isObject( gamepadMap ) )
   gamepadMap.delete();
new ActionMap(gamepadMap);


demoMap.bindCmd(keyboard, "escape", "", "onDemoPlayDone(true);");

//------------------------------------------------------------------------------
// Non-remapable binds
//------------------------------------------------------------------------------

function escapeFromGame()
{
   if ( $Game::State $= "End" || $asserted)
      return;
   //HiGuy: We don't want the disconnect DLG for LB peoples
   if ( $Server::ServerType $= "SinglePlayer" || $LB::LoggedIn)  {
      // In single player, we'll pause the game while the dialog box is up.
		pauseGame();
      //ExitGameText.setText("<just:center><font:DomCasualD:32>Exit from this Level?");
      if ($Server::ServerType $= "Multiplayer") {
			if (!$Server::Lobby && !$Game::Pregame)  {
				Canvas.pushDialog(MPExitGameDlg);
			}
      } else
         Canvas.pushDialog(ExitGameDlg);
   }
   else
      MessageBoxYesNo( "Disconnect", "Disconnect from the server?",
         "disconnect();", "");
}

moveMap.bindCmd(keyboard, "escape", "", "escapeFromGame();");
moveMap.bind(keyboard, t, toggleChatHUD);
moveMap.bind(keyboard, p, togglePrivateChatHUD);

//------------------------------------------------------------------------------
// Movement Keys
//------------------------------------------------------------------------------

$movementSpeed = 1; // m/s

function setSpeed(%speed)
{
   if(%speed)
      $movementSpeed = %speed;
}

function moveleft(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
   {
      if (%val)
         pickPrevSpectator();
      return;
   }
   $mvLeftAction = %val;

   if ($MPPref::DisableDiagonal && $Server::ServerType $= "MultiPlayer")
      checkDiagonal();
}

function moveright(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
   {
      if (%val)
         pickNextSpectator();
      return;
   }
   $mvRightAction = %val;

   if ($MPPref::DisableDiagonal && $Server::ServerType $= "MultiPlayer")
      checkDiagonal();
}

function moveforward(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvForwardAction = %val;

   if ($MPPref::DisableDiagonal && $Server::ServerType $= "MultiPlayer")
      checkDiagonal();
}

function movebackward(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvBackwardAction = %val;

   if ($MPPref::DisableDiagonal && $Server::ServerType $= "MultiPlayer")
      checkDiagonal();
}

function moveup(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvUpAction = %val;
}

function movedown(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($MP::Overview)
      return;
   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvDownAction = %val;
}

//-----------------------------------------------------------------------------
// Jeff: spectating mode

// Jeff: false means chase
//       true means flying
$SpectateFlying = false;

// Jeff: toggles spectating between flying and orbiting
function toggleSpectateModeType(%val)
{
   if ($SpectateMode && %val)
   {
      $SpectateFlying = !$SpectateFlying;

      // Jeff: display the spectator menu to the clients via the PlayGui
      showSpectatorMenu(true);

      // Jeff: if we are not flying, we need to pick a client.
      if (!$SpectateFlying)
         pickNextSpectator();
      else
      {
         // Jeff: stop lerping, we are flying.
         cancel($SpectateCameraLerp);
         $spectatorPerson = -1;
         commandToServer('ToggleCamera');
      }
   }
}

MoveMap.bind(keyboard, c, toggleSpectateModeType);

//-----------------------------------------------------------------------------

// The following were determined after many code tests.
//
// Please note where each of them goes in the current code.
// These codes are suited to have the XBOX360 Controller to be as practically the same
// as possible as the keyboard and mouse.
//
// Change values where you want to modify the speed. The ones below are deafults.
//
// moveYAxis and moveXAxis determine marble movement speed across the two axises
// moveYawAxis is camera movement along X axis (left and right)
// movePitchAxis is camera movement along Y axis (up and down)
//
//    if(mAbs(%val) < 0.23) // else the marble will roll by itself forwards/backwards. DO NOT TOUCH IT. Dead zone area (MBU).
// $mv(X)Action = -%val * (1.68);    // increase the value for faster marble speed
// $mvYaw(X)Speed = -%val * (0.1);   // decrease the value for faster camera movement along X axis (Left/Right)
// Don't ever touch the Pitch. It's annoying the heck out of me!
//         %val = (%val * 0.35);
//        %destPitch = (2 * %val) + 0.45;   // together they affect the camera speed and how much you can
//                                          look up/down. Right now it's perfect.
//                       		    If you change it then restore to the values above if you don't like it
//
// Attempting to configure the XBOX360 Controller to match the Marble speed like the keyboard.
// Differences might occur but VERY SLIGHTLY IF ANY.

function moveXAxis(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if (mAbs(%val) < 0.23)
      %val = 0;
   if (%val < 0)
   {
      $mvLeftAction = -%val * (1.68);
      $mvRightAction = 0;
   }
   else
   {
      $mvLeftAction = 0;
      $mvRightAction  = %val * (1.68);
   }
}

function moveYAxis(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if (mAbs(%val) < 0.23)
      %val = 0;
   if (%val < 0)
   {
      $mvForwardAction = -%val * (1.68);
      $mvBackwardAction = 0;
   }
   else
   {
      $mvForwardAction = 0;
      $mvBackwardAction  = %val * (1.68);
   }
}

function moveYawAxis(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if(mAbs(%val) < 0.23)
      %val = 0;
   if(%val < 0)
   {
      $mvYawRightSpeed = -%val * (0.1);
      $mvYawLeftSpeed = 0;
   }
   else
   {
      $mvYawRightSpeed = 0;
      $mvYawLeftSpeed = %val * (0.1);
   }
}

function movePitchAxis(%val)
{
   %ival = %val;
   if(%val < 0)
   {
      if(%val < -0.23)
      {
         %val = (%val * 0.4);
         %destPitch = (%val * 2) + 0.45;
      }
      else
         %destPitch = 0.45;
   }
   else
   {
      if(%val > 0.23)
      {
         %val = (%val * 0.4);
         %destPitch = (2 * %val) + 0.45;
      }
      else
         %destPitch = 0.45;
   }
   $mvPitch = %destPitch - $marblePitch;
}

// Xbox360 controller specific gamepad buttons and stuff so it works perfect
// The button numbers below equal to the following triggers as if:
// gamepadMap.bind(joystick, button3, mouseFire);
// which means powerup activation is done when you press Y button
// 0 is A
// 1 is B
// 2 is X
// 3 is Y
// 4 is left bumper
// 5 is right bumper
// 6 is back
// 7 is start
// 8 is left thumbstick click
// 9 if right thumbstick click

gamepadMap.bind(joystick, xaxis, moveXAxis);
gamepadMap.bind(joystick, yaxis, moveYAxis);
gamepadMap.bind(joystick, rxaxis, moveYawAxis);
gamepadMap.bind(joystick, ryaxis, movePitchAxis);
gamepadMap.bind(joystick, button0, jump);
gamepadMap.bind(joystick, button1, mouseFire);
gamepadMap.bind(joystick, button7, escapeFromGame);

gamepadMap.push();

function moveup(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvUpAction = %val;
}

function movedown(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($SpectateMode && !$SpectateFlying)
      return;
   $mvDownAction = %val;
}

function turnLeft( %val )
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $mvYawRightSpeed = %val ? $Pref::Input::KeyboardTurnSpeed : 0;
}

function turnRight( %val )
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $mvYawLeftSpeed = %val ? $Pref::Input::KeyboardTurnSpeed : 0;
}

function panUp( %val )
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $mvPitchDownSpeed = %val ? $Pref::Input::KeyboardTurnSpeed : 0;
}

function panDown( %val )
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $mvPitchUpSpeed = %val ? $Pref::Input::KeyboardTurnSpeed : 0;
}

function getMouseAdjustAmount(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return 0;

   // based on a default camera fov of 90'
   return($pref::Input::MouseSensitivity * %val * ($cameraFov / 90) * 0.01);
}

function yaw(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $mvYaw += getMouseAdjustAmount(%val);
   $marbleYaw += getMouseAdjustAmount(%val);
}

function pitch(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if($freelooking || $pref::Input::AlwaysFreeLook)
   {
      if ($pref::input::InvertYAxis) {
         $mvPitch -= getMouseAdjustAmount(%val);
         $pitch   -= getMouseAdjustAmount(%val);
      } else {
         $mvPitch += getMouseAdjustAmount(%val);
         $pitch   += getMouseAdjustAmount(%val);
      }
   }
}

function jump(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   if ($MPPref::DisableJump && $Server::ServerType $= "MultiPlayer")
      return;
   $mvTriggerCount2 = %val;
}

function freelook(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($MP::Overview)
      return;

   $freeLooking = %val;
}

moveMap.bind( keyboard, a, moveleft );
moveMap.bind( keyboard, d, moveright );
moveMap.bind( keyboard, w, moveforward );
moveMap.bind( keyboard, s, movebackward );
moveMap.bind( keyboard, space, jump );
moveMap.bind( mouse, xaxis, yaw );
moveMap.bind( mouse, yaxis, pitch );
moveMap.bind(keyboard, up, panUp);
moveMap.bind(keyboard, down, panDown);
moveMap.bind(keyboard, left, turnLeft);
moveMap.bind(keyboard, right, turnRight);
moveMap.bind(keyboard, backspace, forceRespawn);


//------------------------------------------------------------------------------
// Mouse Trigger
//------------------------------------------------------------------------------

function mouseFire(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if (%val && (!$Server::Hosting || $Server::_Dedicated) && $Server::ServerType $= "MultiPlayer") {
		if ($MP::MyMarble._powerUpId && $MP::FastPowerups)
   		$MP::MyMarble.onPowerUpUsed();
      PlayGui.setPowerup("");
   }

   $mvTriggerCount0 = %val;
   commandToServer('MouseFire', %val);
}

function altTrigger(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   $mvTriggerCount1 ++;
}

moveMap.bind( mouse, button0, mouseFire );
moveMap.bind( mouse, button1, freelook );

//-----------------------------------------------------------------------------
// Diagonal
// Thanks to FruBlox, Source:
// http://marbleblast.com/index.cgi?board=mbdkcode&action=display&thread=14182
// Adapted to Multiplayer
//-----------------------------------------------------------------------------

function isMovingDiagonally()
{
   return (($mvForwardAction + $mvBackwardAction + $mvLeftAction + $mvRightAction) % 2 == 0) && ($mvForwardAction + $mvBackwardAction + $mvLeftAction + $mvRightAction) > 0;
}

function checkDiagonal()
{
   if (!isObject($MP::MyMarble) && MPGetMyMarble() == -1)
      return;
   //HiGuy: Server-sided is a little bit different
   %player = ($Server::Hosting && !$Server::_Dedicated ? LocalClientConnection.player : $MP::MyMarble);
   %datablock = %player.getDatablock();

   if ($MPPref::DisableDiagonal && $Server::ServerType $= "MultiPlayer") {
      if (isMovingDiagonally()) {
         if ($Server::Hosting && !$Server::_Dedicated) {
            if (getSubStr(%datablock.getName(), 0, 2) !$= "__")
               %player.setDatablock("__" @ %datablock.getName());
         } else {
            if (!%datablock.tonedDown) {
               %datablock._angularAcceleration = %datablock.angularAcceleration;
               %datablock._maxRollVelocity     = %datablock.maxRollVelocity;
               %datablock._airAcceleration     = %datablock.airAcceleration;
               %datablock.angularAcceleration  = 75 / $sqrt_2;
               %datablock.maxRollVelocity      = 15 / $sqrt_2;
               %datablock.airAcceleration      = 5 / $sqrt_2;
               %datablock.tonedDown = true;
            }
         }
      } else {
         if ($Server::Hosting && !$Server::_Dedicated) {
            if (getSubStr(%datablock.getName(), 0, 2) $= "__")
               %player.setDatablock(getSubStr(%datablock.getName(), 2, strlen(%datablock.getName())));
         } else {
            if (%datablock.tonedDown) {
               %datablock.angularAcceleration = 75;
               %datablock.maxRollVelocity     = 15;
               %datablock.airAcceleration     = 5;
               %datablock.tonedDown = false;
            }
         }
      }
   }
}

//-----------------------------------------------------------------------------
// Blast
//-----------------------------------------------------------------------------

function useBlast(%val) {
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($SpectateMode)
      return;

   if ($MPPref::DisableBlast && $Server::ServerType $= "MultiPlayer")
      return;
   if (%val && $Server::ServerType $= "MultiPlayer") {
      if ($MP::BlastValue >= $MP::BlastRequiredAmount && MPMyMarbleExists() != -1) {
         %blastValue = ($MP::SpecialBlast ? $MP::BlastRechargePower : mSqrt($MP::BlastValue));
         //Best results found when whacked from here
         %attack = "0 0 -1";

         //Confusing, but all this does is set the impulse
         //to the blast value shown * 10 and then adjusted
         //to the gravity (so we don't get blasted sideways
         //after a gravity modifier)
         %vector = %blastValue * -$MP::BlastPower;
         %vector = %vector SPC %vector SPC %vector;
         %gravity = getGravityDir();
         %push = VectorMult(%vector, %gravity);

         //Get the local marble, as impulsing the server one
         //will reset the camera angle; we don't want that
         $MP::MyMarble.applyImpulse(%attack, %push);

         // Jeff: tell the server to make the particle and to also
         // send the shockwave
         commandToServer('Blast', %gravity);

         //Finally, reset
         PG_BlastBar.setValue(0);
         $MP::BlastValue = 0;
      }
   }
}

// DONE: this is a bit messed up, the code above already looks for 'e' as freeLook if Mac.
// Gotta change this to a different key.
//HiGuy: freelook is now rightmouse for all platforms, mac users will just have to get an actual mouse then...
moveMap.bind(keyboard, "e", useblast);

//------------------------------------------------------------------------------
// Camera & View functions
//------------------------------------------------------------------------------

function toggleCamera(%val)
{
	if ($Server::ServerType $= "Multiplayer" && %val && (!$MP::SpectateFull || $SpectateMode))
		commandToServer('Spectate');
   if ($LB::LoggedIn)
      return;

   if (%val)
      commandToServer('ToggleCamera');
}

moveMap.bind(keyboard, "alt c", toggleCamera);

//------------------------------------------------------------------------------
// Force respawn - Phil and Jeff
//------------------------------------------------------------------------------

function forceRespawn(%val)
{
   if ($LB::LoggedIn && $LB::Paused)
      return;

   cancel($forceRespawn);
   if (%val) {
      // Jeff: check for waiting status - challenges
      if ($Game::Finished || $Game::State $= "waiting")
         return;
      if ($Server::ServerType $= "SinglePlayer") {
         if (LocalClientConnection.checkpointed) {
            LocalClientConnection.respawnOnCheckpoint();
            $forceRespawn = schedule(1000, 0, "restartLevel");
         } else
            restartLevel();
      } else if ($MP::AllowQuickRespawn) {
         commandToServer('QuickRespawn');
         $forceRespawn = schedule(3000, 0, "restartLevel");
      }
   }
}

//------------------------------------------------------------------------------
// Scores list - HiGuy
//------------------------------------------------------------------------------

function displayScoreList(%val) {
   if ($LB::LoggedIn && $LB::Paused)
      return;

   if ($Server::ServerType $= "MultiPlayer") {
      if (%val) {
         Canvas.pushDialog(MPScoresDlg);
      } else {
         Canvas.popDialog(MPScoresDlg);
      }
   }
}

moveMap.bind(keyboard, "o", displayScoreList);
moveMap.bind(keyboard, "tab", radarSwitch);

//------------------------------------------------------------------------------
// Helper Functions
//------------------------------------------------------------------------------

function dropCameraAtPlayer(%val)
{
   if ($LB::LoggedIn)
      return;

   if (%val)
      commandToServer('dropCameraAtPlayer');
}

function dropPlayerAtCamera(%val)
{
   if ($LB::LoggedIn)
      return;

   if (%val)
      commandToServer('DropPlayerAtCamera');
}

function reloadConfig(%val) {
   if (!%val) {
//      MoveMap.save($usermods @ "/client/config.cs");
      exec($usermods @ "/client/scripts/default.bind.cs");
      exec($usermods @ "/client/config.cs");

      if (Canvas.getContent().getName() $= "PlayGui")
         MoveMap.push();

      ASSERT("Keybindings Reloaded", "Reloaded all keybinds from config.cs");
   }
}

moveMap.bind(keyboard, "F8", dropCameraAtPlayer);
moveMap.bind(keyboard, "F7", dropPlayerAtCamera);

//------------------------------------------------------------------------------
// Misc.
//------------------------------------------------------------------------------

function trace2(%toggle) {if (!%toggle) trace(!$tracing);}
GlobalActionMap.bind(keyboard, "ctrl T", "trace2");

GlobalActionMap.bindCmd(keyboard, "ctrl F12", "", "savePrefs();");
GlobalActionMap.bindCmd(keyboard, "alt enter", "", "pauseMusic();toggleFullScreen();resumeMusic();");
GlobalActionMap.bind(keyboard, "ctrl F9", reloadConfig);
