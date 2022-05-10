//------------------------------------------------------------------------------
// Multiplayer Package
// lolmodes.cs
// :D
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function matanModeUpdate() {
   cancel($matanModeSchedule);
   if (!$MPPref::Server::MatanMode)
      return;
   if (!$Game::Running)
      return;
   if ($Server::ServerType !$= "MultiPlayer")
      return;
   spawnHuntGemGroup();
   $matanModeSchedule = schedule($MPPref::Server::MatanModeTime, 0, matanModeUpdate);
}

function createGlass(%max, %center) {
   while (isObject(GLASSYEAHBABY))
      GLASSYEAHBABY.delete();
   for (%i = 0; %i < %max; %i ++) {
      %angle = ((360 * %i) / %max);
      %rads = mDegToRad(%angle);
      //HiGuy: Create a hugeass window
      %change = VectorScale(mSin(%rads) SPC mCos(%rads) SPC -1, 919.73);
      %window = new TSStatic(GLASSYEAHBABY) {
         position = VectorAdd(%center, %change);
         rotation = "0 0 1" SPC %angle;
         scale = "1 1000 1000";
         shapeName = "~/data/shapes/Window03.dts";
      };
      MissionGroup.add(%window);
   }
}

function initGlassMode() {
   while (isObject(GLASSYEAHBABY))
      GLASSYEAHBABY.delete();
   if (!$MPPref::Server::GlassMode)
      return;
   if (ClientGroup.getCount() < 2 && !$MP::AlwaysGlass)
      return -1;
   //HiGuy: Get level center
   %max = max(2, ClientGroup.getCount());
   %center = (isObject(MissionInfo.glassCenter) ? MissionInfo.glassCenter.getPosition() : getMissionCenter());

   createGlass(%max, %center);
}


function GameConnection::getGlassSection(%this) {
   if (%this.glassSection !$= "")
      return %this.glassSection;
   %tries = 0;
   while (%this.glassSection $= "") {
      %max = ClientGroup.getCount();
      for (%section = 0; %section < %max; %section ++) {
         %good = true;
         %checks = 0;
         for (%i = 0; %i < %max; %i ++) {
            %cl = ClientGroup.getObject(%i);
            if (%cl.glassSection == %section) {
               %checks ++;
               if (%checks > %tries) {
                  %good = false;
                  break;
               }
            }
         }
         if (%good) {
            %this.glassSection = %section;
            break;
         }
      }
      %tries ++;
   }
   return %this.glassSection;
}

function GameConnection::getGlassSpawnTrigger(%this) {
   if (!$MPPref::Server::GlassMode)
      return -1;
   if (ClientGroup.getCount() < 2 && !$MP::AlwaysGlass)
      return -1;
   while (isObject(TESTAHEDRON))
      TESTAHEDRON.delete();
   %section = %this.getGlassSection();
   %max = max(2, ClientGroup.getCount());
   %angleMin = ((($pi * 2) * %this.glassSection) / %max) + 0.1 + MissionInfo.glassRotate;
   %angleMax = ((($pi * 2) * (%this.glassSection + 1)) / %max) - 0.1 + MissionInfo.glassRotate;

   //echo("Spawning" SPC %this.nameBase SPC "between" SPC %angleMin SPC "and" SPC %angleMax);
   //echo("Client section is" SPC %this.getGlassSection());

   %center = (isObject(MissionInfo.glassCenter) ? MissionInfo.glassCenter.getPosition() : getMissionCenter());

   if (!isObject(SpawnPointSet))
      return -1;

   %size = SpawnPointSet.getCount();

   if (%size == 0)
      return -1;

   // Jeff: SHOULD THIS SIMSET BE DEALLOCATED?!!?
   %spawnable = new SimSet();

   for (%i = 0; %i < %size; %i ++) {
      %obj = SpawnPointSet.getObject(%i);
      if (!isObject(%obj))
         continue;

      %pos = %obj.position;
      %offset = VectorSub(%pos, %center);
      %angle = mAtan(getWord(%offset, 1), getWord(%offset, 0));
//      %angle += $pi / 2;

      while (%angle > ($pi * 2))
         %angle -= ($pi * 2);
      while (%angle < 0)
         %angle += ($pi * 2);

      if (%angle > %angleMin && %angle < %angleMax) {
         %spawnable.add(%obj);
         new TSStatic(TESTAHEDRON) {
            position = %obj.position;
            rotation = "0 0 0 0";
            scale = "1 1 1";
            shapeName = "~/data/shapes/markers/octahedron.dts";
         };
         //echo("trigger" SPC %obj SPC"angle is" SPC %angle);
      }
   }

   if (%spawnable.getCount() == 0)
      return -1;

   %triggerNum = getRandom(0, %spawnable.getCount() - 1);
   %trigger = %spawnable.getObject(%triggerNum);
   //echo("spawntrigger is" SPC %trigger);

   return %trigger;
}

function testGlass(%players) {
   %max = max(2, ClientGroup.getCount());
   %center = (isObject(MissionInfo.glassCenter) ? MissionInfo.glassCenter.getPosition() : getMissionCenter());
}


function testahedron(%pos, %clear) {
   if (%clear)
      while (isObject(TESTAHEDRON))
         TESTAHEDRON.delete();
   new TSStatic(TESTAHEDRON) {
      position = %pos;
      rotation = "0 0 0 0";
      scale = "1 1 1";
      shapeName = "~/data/shapes/markers/octahedron.dts";
   };
}

function megaScale(%scale) {
   scaleGroup(%scale, MissionGroup);
}

function scaleGroup(%scale, %group) {
   for (%i = 0; %i < %group.getCount(); %i ++) {
      %obj = %group.getObject(%i);
      if (%obj.getClassName() $= "SimGroup")
         scaleGroup(%scale, %obj);
      else {
         %obj.setScale(VectorScale(%obj.scale, %scale));
         %obj.setTransform(VectorScale(getWords(%obj.getTransform(), 0, 2), %scale) SPC getWords(%obj.getTransform(), 3, 6));
      }
   }
}

function gemVisionUpdate() {
   cancel($gemVisionSchedule);
   if (!$MP::Server::GemVision)
      return;
   ServerConnection.pointToNearestGem();

   $gemVisionSchedule = schedule(1000/$fps::real, 0, gemVisionUpdate);
}

function clientCmdStartGemVision() {
   gemVisionUpdate();
}

function clientCmdStopGemVision() {
   cancel($gemVisionSchedule);
}

function initGemVision() {
   commandToAll('StartGemVision');
}
