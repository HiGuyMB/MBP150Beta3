//-----------------------------------------------------------------------------
// clientSpectator.cs
//
// Copyright (c) 2013 The Platinum Team
//
// Jeff: allows you to spectate through people's cameras!
//
// To spectate, just do:
//    commandToServer('Spectate');
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Jeff: Variables
//-----------------------------------------------------------------------------

// Jeff: lets us know if we are in spectate mode or not.
$SpectateMode = false;

// Jeff: the current plebian index to spectate
$spectatorIndex = 0;

// Jeff: the ID handle of the current person we are spectating.
$spectatorPerson = -1;

// Jeff: your own camera object
$MP::Camera = -1;

// Jeff: time inbetween lerps
$cameraLerpTime = 200;

//-----------------------------------------------------------------------------
// Jeff: client commands
//-----------------------------------------------------------------------------

// Jeff: this lets us know that we can now start spectating
function clientCmdGoSpectateNow()
{
   // Jeff: start spectate mode
   $SpectateMode = true;
   toggleSpectateModeType(true);

   PlayGui.updateBlastBar();
}

// Jeff: when we finish spectating
function clientCmdFinishSpectating()
{
   $SpectateMode = false;
   PlayGui.updateBlastBar();
}

// Jeff: if too many players are spectators, inform the client that there are
// to many and they have to play.  SORRY BUT THAT IS JUST HOW IT IS PLEBIANS.
function clientCmdNoSpectate()
{
   MessageBoxOK("Sorry", "You may not spectate as there are already too many people who are spectating.", "", true);
   MPPreGameSpectate.setValue(false);
   MPPreGameSpectate.setActive(false);
}

function clientCmdSpectateFull(%full) {
   //echo("Spectate is full:" SPC %full);
   $MP::SpectateFull = %full;
   MPPreGameSpectate.setActive(!%full);
}

function clientCmdSpectateChoice() {
   MessageBoxYesNo("Join Game in Progress?", "Do you want to join the current game in progress? Joining will qualify you for rating points if you win, or will make you lose rating points if you lose.", "commandToServer(\'Spectate\', false);", "");
}

//-----------------------------------------------------------------------------
// Jeff: functions specific to spectate mode
//-----------------------------------------------------------------------------

// Jeff: choses the next plebian to watch
function pickNextSpectator()
{
   if (!isObject(getCamera()))
      return;

   // Jeff: flush out bad plebians, we want a real plebian not a fake one
   // that doesn't exist...
   //
   // This is how you do a do-while in MB, you just set the condition to false
   // for the first time.

   MPBuildGhostList();

   $spectatorPerson = -1;
   %wrapped = false;
   while (!isObject($spectatorPerson))
   {
      $spectatorIndex ++;

      // Jeff: if we gone to high, we need to bring it back to 0
      if ($spectatorIndex > $MP::GhostIndexMax)
      {
         $spectatorIndex = 0;
         if (%wrapped)
         {
            //HiGuy: We're never going to find one
            break;
         }
         %wrapped = true;
      }

      // Jeff: check scales.  Don't spectate ourselves!
      if ($spectatorPerson.getScale() !$= $MP::MyMarble.getScale())
         continue;

      $spectatorPerson = $MP::GhostIndex[$spectatorIndex];
      devecho("Searching for spectator.");
   }

   onPickSpectator();
}

function pickPrevSpectator()
{
   if (!isObject(getCamera()))
      return;

   // Jeff: flush out bad plebians, we want a real plebian not a fake one
   // that doesn't exist...
   //
   // This is how you do a do-while in MB, you just set the condition to false
   // for the first time.

   MPBuildGhostList();

   $spectatorPerson = -1;
   %wrapped = false;

   %highest = $MP::GhostIndexMax - 1;
   while (!isObject($spectatorPerson))
   {
      $spectatorIndex --;

      // Jeff: if we gone to low, bring it to max - 1
      if ($spectatorIndex < 0)
      {
         $spectatorIndex = %highest;
         if (%wrapped)
         {
            //HiGuy: We're never going to find one
            break;
         }
         %wrapped = true;
      }

      // Jeff: check scales.  Don't spectate ourselves!
      if ($spectatorPerson.getScale() !$= $MP::MyMarble.getScale())
         continue;

      $spectatorPerson = $MP::GhostIndex[$spectatorIndex];
      devecho("Searching for spectator.");
   }

   onPickSpectator();
}

function onPickSpectator()
{
   if (!isObject($spectatorPerson))
   {
      //HiGuy: Nobody to spectate, free cam instead
      devecho("No spectators.  We don\'t want an infinate loop!  Stop searching.");
      $SpectateFlying = true;
      return;
   }
   devecho("Chose Spectator Index:" SPC $spectatorIndex);
   $MP::Camera.setTransform($spectatorPerson.getWorldBoxCenter() SPC "1 0 0 0");
   updateCameraLerp();
}

// Jeff: gets the camera object
// Note: only *YOUR* camera is scoped to you, so you only have 1 Camera
// in server connection at a time =)
function getCamera()
{
   if ($Server::ServerType !$= "Multiplayer")
      return -1;
   if (isObject($MP::Camera))
      return $MP::Camera;
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "Camera")
      {
         $MP::Camera = %obj;
         return %obj;
      }
   }
   $MP::Camera = -1;
   return -1;
}

function Camera::setNextLerp(%this, %point, %time) {
   //HiGuy: Set delta = 0 (immediate)
   %this.lerpDelta = 0;

   //HiGuy: Time is time until we reach that point
   %this.lerpTime = %time;

   //HiGuy: Start pos is using LerpLast (camera orbit point), or getPosition
   // if needed.
   %this.lerpStart = (%this.lerpLast $= "" ? %this.getPosition() : %this.lerpLast);

   //HiGuy: End is where we want to end up
   %this.lerpEnd = %point;
}

function Camera::followPath(%this, %path)
{
   //HiGuy: %path is a TAB-separated list of "x y z t"
   %time  = 0;
   %count = getFieldCount(%path);
   for (%i = 0; %i < %count; %i ++)
   {
      //HiGuy: Iterate through each node
      %node = getField(%path, %i);
      %x = getWord(%node, 0);
      %y = getWord(%node, 1);
      %z = getWord(%node, 2);
      %t = getWord(%node, 3);

      //HiGuy: Basic scheduling
      %this.schedule(%time, "setNextLerp", %x SPC %y SPC %z, %t);
      %time += %t;
   }
}

//HiGuy: Test path (you can update these to whatever for testing purposes
function testPath()
{
   commandToServer('Spectate');
   %path =           "0 0 10 10000"; //First point (don't copy)
   %path = %path TAB "-20 -70 -20 10000"; //Copy these
   %path = %path TAB "60 -50 30 10000"; //For more points
   %path = %path TAB "-40 20 10 10000"; //That the camera
   %path = %path TAB "20 20 60 10000"; //Should follow
   //HiGuy: Copypaste above lines to add more

   //HiGuy: Quick eval hack because the server needs time to set up spectate
   // mode for us.
   schedule(1000, 0, "eval", "$MP::Camera.followPath(\"" @ %path @ "\");");
}

function updateCameraLerp()
{
   cancel($SpectateCameraLerp);
   if (!isObject(getCamera()) || !isObject($spectatorPerson))
      return;
   $MP::Camera.setNextLerp($spectatorPerson.getWorldBoxCenter(), $cameraLerpTime);
   $SpectateCameraLerp = schedule($cameraLerpTime, 0, updateCameraLerp);
}

// Jeff: updates your camera position and rotation to smoothly interpolate it
function interpolateCamera(%delta)
{
   if (!isObject(getCamera()) || !isObject($spectatorPerson))
      return;

   //HiGuy: Set up the position variables
   %position  = $MP::Camera.lerpStart;
   if (%position $= "")
   {
      $MP::Camera.setNextLerp($spectatorPerson.getWorldBoxCenter(), 1);
      %position  = $MP::Camera.lerpStart;
   }

   %endPos = $MP::Camera.lerpEnd;

   //HiGuy: Progress is time / total
   %progress = $MP::Camera.lerpDelta / $MP::Camera.lerpTime;

   //HiGuy: Don't over-shoot it
   if (%progress > 1)
      %progress = 1;

   //HiGuy: Optional ease in/out for smoothness
   if ($MP::Camera.lerpEase)
      %progress = ease(0, 1, 1, %progress);

   //HiGuy: Delta is time since last lerp point update
   $MP::Camera.lerpDelta += %delta;

   // Jeff: interpolate position
   %interPos = vectorLerp(%position, %endPos, %progress);
   //%interPos = %position;
   $MP::Camera.lerpLast = %interPos;

   //HiGuy: I can't find an exact substitute for marble yaw movement. Anyone
   // know a better way to do this?
   %horizScale = mCos($pitch) * 2.5;

   // Jeff: make the camera "orbit" around the marble
   %ortho = -mSin($marbleYaw) * %horizScale SPC -mCos($marbleYaw) * %horizScale SPC ($pitch * 2.5) + 1;
   %interPos = VectorAdd(%interPos, %ortho);

   //HiGuy: Cool matrixy stuff for pitch and yaw rotation
   %vec1 = "0 0 0 0 0 1" SPC $marbleYaw;
   %vec2 = "0 0 0 1 0 0" SPC $pitch;

   //HiGuy: Multiply yaw * pitch for the complete rotation
   %rotation = MatrixMultiply(%vec1, %vec2);

   //HiGuy: Multiply pos * rotation for the final value
   %transform = MatrixMultiply(%interPos SPC "0 0 0 0", %rotation);

   $MP::Camera.setTransform(%transform);
}

// Jeff: this toggles the spectator menu on the playGui
function showSpectatorMenu(%show)
{
   if (%show)
   {
      %text = "<font:DomcasualD:24><just:center>Specator Info" NL
              "<just:left>Toggle Free Fly / Orbit Camera:<just:right><func:bind toggleSpectateModeType>" NL
              "<just:left>Exit Spectate Mode:<just:right><func:bind toggleCamera>";

      // Jeff: orbit mode we show more options.
      if (!$SpectateFlying)
      {
         %text = %text @
                 "<just:left>Prev Player:<just:right><func:bind moveLeft>" NL
                 "<just:left>Next Player:<just:right><func:bind moveRight>";
      }
      PG_SpectatorText.setText(%text);
   }
   PG_SpectatorMenu.setVisible(%show);
}

//-----------------------------------------------------------------------------

function clientCmdStartOverview(%heightOffset, %width) {
   $MP::Overview = true;

   %center = "0 0 0";
   %box = "9999 9999 9999 -9999 -9999 -9999";
   %objects = 0;
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "InteriorInstance") {
         %center = VectorAdd(%center, %obj.getWorldBoxCenter());
         %objects ++;
         %wbox = %obj.getWorldBox();
         //echo("Intersecting box" SPC %box SPC "with" SPC %wbox);
         if (getWord(%wbox, 0) < getWord(%box, 0)) %box = setWord(%box, 0, getWord(%wbox, 0));
         if (getWord(%wbox, 1) < getWord(%box, 1)) %box = setWord(%box, 1, getWord(%wbox, 1));
         if (getWord(%wbox, 2) < getWord(%box, 2)) %box = setWord(%box, 2, getWord(%wbox, 2));
         if (getWord(%wbox, 3) > getWord(%box, 3)) %box = setWord(%box, 3, getWord(%wbox, 3));
         if (getWord(%wbox, 4) > getWord(%box, 4)) %box = setWord(%box, 4, getWord(%wbox, 4));
         if (getWord(%wbox, 5) > getWord(%box, 5)) %box = setWord(%box, 5, getWord(%wbox, 5));
      }
   }

   $pitch = 0.5;

   %center = VectorScale(%center, 1/%objects);
   $MP::OverviewCenter = %center;
   $MP::OverviewHeightOffset = %heightOffset;
   $MP::OverviewWidth = getWord(%box, 3) - getWord(%box, 0) SPC getWord(%box, 4) - getWord(%box, 1) SPC getWord(%box, 5) - getWord(%box, 2);
   if (%width !$= "")
      $MP::OverviewWidth = %width;
   $MP::OverviewStart = getRealTime();

   MPUpdateOverview();
}

function MPUpdateOverview() {
   if (!$MP::Overview) return;

   schedule(20, 0, MPUpdateOverview);

   if (!isObject(getCamera())) return;
   if (!isObject(ServerConnection)) return;

   if ($MP::OverviewFinish) {
      %elapsed = getRealTime() - $MP::OverviewFinishStart;
      %progress = %elapsed / $MP::OverviewFinishTime;

      %position = vectorEase($MP::OverviewLastPos, $MP::OverviewFinalPos, %progress);

      %diff = VectorSub(%position, $MP::OverviewFinalAimPos);

      //Transition from OverviewLastPos -> OverviewFinalPos
      //Angle must point towards it.
      //Angle does easing from OverviewAngle to actual angle

      %angle = -mAtan(getWord(%diff, 1), getWord(%diff, 0)) - ($pi / 2);
      while (%angle < 0 && $MP::OverviewAngle > 0)
         %angle += $tau;
      while (%angle > 0 && $MP::OverviewAngle < 0)
         %angle -= $tau;
      %fangle = -(%angle - $MP::OverviewAngle) * %progress * (%progress - 2) + $MP::OverviewAngle;
      //echo("%fangle:" SPC %fangle SPC "%angle:" SPC %angle SPC "$overviewangle:" SPC $MP::OverviewAngle);

      %pitch = easeFrom($MP::OverviewPitch, $MP::OverviewFinalPitch, %progress);

      %vec1 = "0 0 0 0 0 1" SPC %fangle;
      %vec2 = "0 0 0 1 0 0" SPC %pitch;
      %rotation = MatrixMultiply(%vec1, %vec2);
      %final = MatrixMultiply(%position SPC "0 0 0 0", %rotation);

      getCamera().setTransform(%final);
      return;
   }

   %elapsed = getRealTime() - $MP::OverviewStart;
   %angle = ((%elapsed / 1000) * $tau) / $MPPref::OverviewSpeed;

   while (%angle > $pi)
      %angle -= $tau;
   while (%angle < -$pi)
      %angle += $tau;

   $MP::OverviewAngle = %angle;

   %distance = VectorScale($MP::OverviewWidth, 2 / 3);
   %offset = -mSin(%angle) SPC -mCos(%angle);

   %offset = VectorMult(%offset, %distance);
   %position = VectorAdd(%offset, $MP::OverviewCenter);

   %top = getWord($MP::OverviewCenter, 2) + (getWord($MP::OverviewWidth, 2) / 2) + $MP::OverviewHeightOffset;
   %position = setWord(%position, 2, %top);

   %angle = -mAtan(getWord(%offset, 1), getWord(%offset, 0)) - ($pi / 2);

   %pitch = 0.5;

   $MP::OverviewPitch = %pitch;

   %vec1 = "0 0 0 0 0 1" SPC %angle;
   %vec2 = "0 0 0 1 0 0" SPC %pitch;
   %rotation = MatrixMultiply(%vec1, %vec2);
   %final = MatrixMultiply(%position SPC "0 0 0 0", %rotation);

   getCamera().setTransform(%final);
}

function clientCmdFinishOverview(%lerptime, %aimPos, %finalPos, %pitch) {
   $MP::OverviewFinalAimPos = %aimPos;
   $MP::OverviewFinalPos = %finalPos;
   $MP::OverviewFinalPitch = %pitch;
   $MP::OverviewLastPos = getWords(getCamera().getTransform(), 0, 2);
   $MP::OverviewFinish = true;
   $MP::OverviewFinishStart = getRealTime();
   $MP::OverviewFinishTime = %lerpTime;
}

function clientCmdStopOverview() {
   $MP::OverviewFinish = false;
   $MP::Overview = false;
}

//-----------------------------------------------------------------------------
// Jeff: Support functions
//-----------------------------------------------------------------------------

// Jeff: returns the amount of active players (non-spectators)
function getActivePlayerCount()
{
   %playerCount = 0;
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      if (ServerConnection.getObject(%i).getClassName() $= "Marble")
         %playerCount ++;
   }
   return %playerCount;
}

//HiGuy: Credit for ease() function:
//http://stackoverflow.com/questions/13462001/ease-in-and-ease-out-animation-formula
// Adapted to TorqueScript by me, don't expect any comments.
// Quadratic ease in / out
// Source: http://gizma.com/easing/
function ease(%start, %len, %total, %current)
{
   %current /= %total / 2;
   if (%current < 1)
      return %len / 2 * %current * %current + %start;
   %current --;
   return -%len / 2 * (%current * (%current - 2) - 1) + %start;
}

function easeFrom(%a, %b, %progress) {
   return ease(%a, %b - %a, 1, %progress);
}

// Jeff: I want interpolation, I want LERP
// this will predict the next movement of where to go based upon positions.
function vectorLerp(%a, %b, %delta)
{
   return vectorAdd(%a, vectorScale(vectorSub(%b, %a), %delta));
}

function vectorEase(%a, %b, %delta) {
   return ease(getWord(%a, 0), getWord(%b, 0) - getWord(%a, 0), 1, %delta) SPC
          ease(getWord(%a, 1), getWord(%b, 1) - getWord(%a, 1), 1, %delta) SPC
          ease(getWord(%a, 2), getWord(%b, 2) - getWord(%a, 2), 1, %delta);
}
