//------------------------------------------------------------------------------
// Multiplayer Package
// clientGhost.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

// Jeff: get the client-sided marble by iterating through each object
//       within the serverConnection - gameConnection
function MPgetMyMarble() {
   if (isObject(ServerConnection) && isObject(ServerConnection.getControlObject())) {
      if (ServerConnection.getControlObject().getClassName() $= "Marble")
         ServerConnection.marble = ServerConnection.getControlObject();
      if (ServerConnection.marble $= "")
         ServerConnection.marble = -1;
      if (!isObject(ServerConnection.marble))
         ServerConnection.marble = -1;
      $MP::MyMarble = ServerConnection.marble;
      if (isObject(ServerConnection.marble))
         $MP::MyMarble.setScale($MP::MyScale);
   } else
      $MP::MyMarble = -1;
   ServerConnection.player = $MP::MyMarble;
   return $MP::MyMarble;
}

// Jeff: check to see if client sided marble
//       is stored in memory as an object
function MPMyMarbleExists() {
   cancel($MP::Schedule::MyMarbleExists);
	if (isObject($MP::MyMarble))
		return true;
	else if (MPgetMyMarble() != -1)
		return true;
   $MP::Schedule::MyMarbleExists = schedule(100, 0, "MPMyMarbleExists");
	return false;
}

// Jeff: sends the ghost rotation to the server to make ghosts
// update their rotation
function MPGiveRotToServer() {
	cancel($MP::Schedule::GhostRot);
   if (!isObject(ServerConnection) || $Server::ServerType $= "SinglePlayer")
      return;
	if (!MPMyMarbleExists())
		return;

	%rot = getWords($MP::MyMarble.getTransform(),3,6);
	commandToServer('GR' , %rot);

	$MP::Schedule::GhostRot = schedule($MP::ClientRotUpdate, 0, "MPGiveRotToServer");
}

// Jeff: this method cancels all client sided schedules,
//       useful when disconnecting from a server
function MPCancelClientSchedules() {
	cancel($MP::Schedule::GhostRot);
	cancel($MP::Schedule::MyMarbleExists);
	cancel($MP::Schedule::FixGhost);
	cancel($MP::Schedule::ItemCollision);
	cancel($MP::Schedule::LookEmitters);
	cancel($MP::Schedule::EmitterPosition);
	cancel($MP::Schedule::MarbleTransform);
}

//HiGuy: Builds the ghost list
function MPBuildGhostList() {
	if (!isObject(ServerConnection))
		return false;

   //HiGuy: Iterate
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "StaticShape") {
         //HiGuy: It's a StaticShape
         if (strStr(%obj.getDataBlock().shapeFile, "ghost-superball") != -1) {
            //Collision mesh!
            if ($MP::MeshLookup[%obj.getScale()] !$= "") {
               //HiGuy: It *is* a mesh, add it's data
               %obj.index = $MP::MeshLookup[%obj.getScale()];
               $MP::MeshIndex[%obj.index] = %obj;
               $MP::MeshIndexMax = ($MP::MeshIndexMax <= %obj.index ? %obj.index : $MP::MeshIndexMax);
            }
         } else if (strStr(%obj.getDataBlock().shapeFile, "/balls/") != -1) {
            //HiGuy: It's probably a ghost
            if ($MP::GhostLookup[%obj.getScale()] !$= "") {
               //HiGuy: It *is* a ghost, add it's data
               %obj.index = $MP::GhostLookup[%obj.getScale()];
               $MP::GhostIndex[%obj.index] = %obj;
               $MP::GhostIndexMax = ($MP::GhostIndexMax <= %obj.index ? %obj.index : $MP::GhostIndexMax);
            }
         }
      }
   }

   //HiGuy: Flush out old missing ghosts
   for (%i = 0; %i < $MP::GhostIndexMax; %i ++) {
      %ghost = $MP::GhostIndex[%i];
      if (%ghost $= "")
         continue;
      if (!isObject(%ghost)) {
         $MP::GhostIndex[%i] = "";
         continue;
      }
   }
   for (%i = 0; %i < $MP::MeshIndexMax; %i ++) {
      %ghost = $MP::MeshIndex[%i];
      if (%ghost $= "")
         continue;
      if (!isObject(%ghost)) {
         $MP::GhostIndex[%i] = "";
         continue;
      }
   }
   $MP::BuiltGhostList = true;
   return true;
}

//HiGuy: Formerly named "hide freddies," renamed for the sake of professionalism
function MPHideGhosts() {
   if (!isObject(ServerConnection) || (!isObject($MP::MyMarble) && MPgetMyMarble() == -1 && !$SpectateMode))
      return;

   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++) {
		%obj = ServerConnection.getObject(%i);

		if (%obj.getClassName() $= "Marble") {
		   //HiGuy: Don't DARE do this to our marble!
		   if (nameToId(%obj) == nameToId($MP::MyMarble))
		      continue;

		   //HiGuy: Record this just in case
		   %obj.previousPos = %obj.getTransform();

		   //HiGuy: Get them hidden...
		   %obj.setScale("0 0 0");

		   //HiGuy: ...and out of the way!
		   %obj.setTransform("-10000000 -10000000 -10000000 1 0 0 0");
		}
   }
}

function MPHideMyGhost() {
   if (!$MP::BuiltGhostList || !MPBuildGhostList())
     return;
   if ($MP::MyScale $= "")
      return;
   if (isObject($MP::MyMarble) && MPGetMyMarble() == -1)
      return;

   //HiGuy: Get our ghost
   %index = $MP::GhostLookup[$MP::MyScale];
   %ghost = $MP::GhostIndex[%index];

   //HiGuy: Get our mesh
   %meshIndex = $MP::MeshLookup[$MP::MyScale];
   %mesh = $MP::MeshIndex[%meshIndex];

   if (isObject(%ghost)) {
      if (%ghost.getScale() !$= $MP::MyScale) {
         devecho("MyScale is" SPC $MP::MyScale);
         devecho("GhostIndex is" SPC %index);
         devecho("Ghost is" SPC %ghost);
         devecho("Ghost Scale is (should be the same as MyScale)" SPC %ghost.getScale());
      }
   }

   if (%ghost !$= "" && isObject(%ghost)) {
      //HiGuy: Hide our ghost!
      %ghost.setScale("0 0 0");
      %ghost.hide(true);
   }

   if (%mesh !$= "" && isObject(%mesh)) {
      //HiGuy: Hide our mesh!
      %mesh.setScale("0 0 0");
      %mesh.hide(true);
   }
}

function MPResetGhosts() {
   if (!isObject(ServerConnection))
      return;
   if (!$MP::BuiltGhostList || !MPBuildGhostList())
     return;

   for (%i = 0; %i < $MP::GhostIndexMax; %i ++) {
      %ghost = $MP::GhostIndex[%i];
      if (%ghost $= "" || !isObject(%ghost))
         continue;

      // Jeff: don't do this to ourselves
      if (%ghost.getScale() $= $MP::MyScale)
         continue;

//      %ghost.setScale("1 1 1");
      //HiGuy: We only hide the marble, collision still applies (see mesh)
      %ghost.hide($MPPref::DisableMarbles);
//      echo("Unhiding" SPC %ghost);
   }
   for (%i = 0; %i < $MP::MeshIndexMax; %i ++) {
      %mesh = $MP::MeshIndex[%i];
      if (%mesh $= "" || !isObject(%mesh))
         continue;

      // Jeff: don't do this to ourselves
      if (%mesh.getScale() $= $MP::MyScale)
         continue;

      //%mesh.setScale("1 1 1");
      //HiGuy: Show it regardless of whether we show the actual shape
      %mesh.hide(false);
   }

   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++) {
		%obj = ServerConnection.getObject(%i);

		if (%obj.getClassName() $= "Marble") {
         if (%obj.previousPos !$= "" && (MPGetMyMarble() == -1 || %obj.getId() != MPGetMyMarble().getId()))
            %obj.setTransform(%obj.previousPos);
      }
   }
}

//HiGuy: Compatibility
function MPFixGhost() {
   fixGhost();
}

//HiGuy: We love this method :D
function fixGhost() {
   //HiGuy: Check for ServerConnection!
   if (!isObject(ServerConnection))
      return;

   //HiGuy: Fix the timer!
   cancel($MP::Schedule::FixGhost);

   // Jeff: only do this if we have a different OS from the server
   if (serverHasDiffPlatform()) {
      // Jeff: fix superclasses in ServerConnection!
      // hackish but should get the job done!
      applySuperClass(ServerConnection);
      sendMarbleTransform();
   } else {
      // Jeff: ensure we are rotating!
      //       only update rotation if we have same platform of the server
      //       because sendMarbleTransform gets pos and rot, why send double
      //       data
      MPGiveRotToServer();
   }

   //HiGuy: Build the list!
   MPBuildGhostList();

   //HiGuy: Reset the ghosts!
   MPResetGhosts();

   //HiGuy: Get my marble!
   MPgetMyMarble();

   //HiGuy: Hide my ghost!
   MPHideMyGhost();

   //HiGuy: Hide everyone else's ghost!
   MPHideGhosts();

   //HiGuy: Update tweening!
   tween();

   // Jeff: item collision! (only for non local host!)
   if (ServerConnection.getPing() > 35) {
      buildItemList();
      updateItemCollision();
   }

   // Jeff: update the mega marble datablocks to show the correct
   // shapes and skins!
   reskinMegaMarbles();

   // Jeff: hide items for handicaps
   updateClientHandiCapItems();

   // Jeff: start particle loops
   lookForEmitters();
   updateEmitterPositions();

   //HiGuy: Restart the timer!
   $MP::Schedule::FixGhost = schedule($MP::ClientFixTime, 0, fixGhost);

   //HiGuy: Breathe a sigh of relief, and feel a weight lifted off your
   // shoulders, for the ghosts have been fixed. You will not have to see
   // their derpy faces around here, but instead bask in the bliss from only
   // seeing the things you were meant to see. No more ghosts following you,
   // nobody sitting lifeless on the start pad. Nobody standing still in
   // various places, and never self-collide again. Ahh. That is bliss.
   //
   // Jeff: man is this function slow :D
}

function stripRot(%vec) {
   return getWords(%vec, 0, 2);
}

//HiGuy: Called to update ghost tweening
function tween() {
   if ($fast || !isObject(ServerConnection) || $Server::ServerType $= "SinglePlayer")
      return;

   cancel($MP::Schedule::Tween);

   if ($MP::Delta::Tween $= "")
      $MP::Delta::Tween = getRealTime();

   %delta = getRealTime() - $MP::Delta::Tween;
   $MP::Delta::Tween = getRealTime();

   if (!$MP::BuiltGhostList && !MPBuildGhostList())
      return;
   for (%i = 0; %i < $MP::GhostIndexMax; %i ++) {
      %ghost = $MP::GhostIndex[%i];
      %mesh = $MP::MeshIndex[%i];
      if ((%ghost $= "" || !isObject(%ghost)) || (%mesh $= "" || !isObject(%mesh)) || %ghost == $MP::GhostIndex[$MP::GhostLookup[$MP::MyScale]])
         continue;
      //HiGuy: the tween value should be * 1000/%delta so
      // we need to divide it by 1000/%delta, or * by %delta/1000

      %difference = %ghost.difference;
      %difference = VectorScale(%difference, %delta / 1000);

      %old = %ghost.oldPos;
      %new = VectorAdd(stripRot(%ghost.getTransform()), stripRot(%difference));

      // Jeff: tween rotation
      %ghost.tweenRotation();

      //HiGuy: Now we update the ghost's position
      %vec = %new SPC %ghost.ghostRot;
      %ghost.setTransform(%vec);
      %mesh.setTransform(%vec);
   }

   $MP::Schedule::Tween = schedule($MP::ClientTween, 0, "tween");
}

//HiGuy: Called when we get an update on tweening data
function tweenUpdate(%index, %difference, %speed, %isMegaMarble) {
   //HiGuy: Make sure we built the list
   if ($fast || !$MP::BuiltGhostList || !MPBuildGhostList() || $Server::ServerType $= "SinglePlayer")
      return;

   //HiGuy: Find the one to update
   %ghost = $MP::GhostIndex[%index];
   if (%ghost $= "" || !isObject(%ghost)) //HiGuy: They left!
      return;

   %ghost.difference = getWords(%difference, 0, 2);
   %ghost.tweenRot   = getWords(%difference, 3, 6);
   %ghost.speed      = %speed;
   %ghost.oldPos     = %ghost.getPosition();
   %ghost.megaMarble = %isMegaMarble;

   //HiGuy: Update tweening
   tween();
}
