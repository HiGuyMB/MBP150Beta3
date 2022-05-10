//-----------------------------------------------------------------------------
// serverSpectator.cs
//
// Copyright (c) 2013 The Platinum Team
//
// Jeff: allows you to spectate through people's cameras!
//-----------------------------------------------------------------------------

function serverCmdSetSpectate(%client, %spectate) {
   if (%client.forceSpectate)
   	return;

	//Take them off
	%client.spectating = false;

   $Server::SpectateCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      if (ClientGroup.getObject(%i).spectating)
         $Server::SpectateCount ++;
   }

   %client.spectating = %spectate;

   %max = getRealPlayerCount() - 1;

   // Jeff: one person must play!!
   if (%client.spectating)
   {
      if ($Server::SpectateCount >= %max)
      {
         commandToClient(%client, 'NoSpectate');
         commandToClient(%client, 'SpectateFull', true);
         %client.spectating = false;
         return;
      }

      $Server::SpectateCount ++;

      if ($Server::SpectateCount >= %max)
         commandToAll('SpectateFull', !%client.spectating);
      else
         commandToAll('SpectateFull', false);
   }

//   %client.setSpectating(%client.spectating);
   updatePlayerlist();
   updateScores();
}

function serverCmdSpectate(%client, %spectating)
{
   if (%spectating $= "")
      %spectating = !%client.spectating;
   serverCmdSetSpectate(%client, %spectating);
   if (%client.spectating == %spectating)
	   %client.setSpectating(%spectating);
}

function GameConnection::setSpectating(%this, %spectating)
{
   %this.spectating = %spectating;

   if (%spectating)
   {
      // Jeff: a bit hackish but should work.
      if (isObject(%this.camera))
         %this.toggleCamera();

      // Jeff: delete a marble, ghost, and mesh if they exist
      if (isObject(%this.player))
         %this.player.delete();
      if (isObject(%this.ghost))
         %this.ghost.delete();
      if (isObject(%this.mesh))
         %this.mesh.delete();

      // Jeff: give the clients time to have the objects deleted.
      schedule(50, 0, finishSpectate, %this);
   }
   else
   {
      %this.spawnPlayer();
      %this.respawnPlayer();

      // Jeff: make keys enabled again for controlling the client!
      commandToClient(%this, 'FinishSpectating');

      // Jeff: we got rid of a spectator.  Update the ghosts already!
      schedule(500, 0, commandToAll, 'FixGhost');
   }

   updatePlayerlist();
}

// Jeff: finishes the spectate, forces all clients to update
function finishSpectate(%client)
{
   // Jeff: tell clients that they need to update there ghost lists! We have
   // a spectator
   commandToAll('FixGhost');

   // Jeff: let the client know that they can spectate now
   commandToClient(%client, 'GoSpectateNow');
}


function GameConnection::startOverview(%this) {
   if (!isObject(%this.camera)) {
      //HiGuy: Overview isn't much good without a camera!
      %this.camera = new Camera() {
         dataBlock = Observer;
      };
      MissionCleanup.add( %this.camera );
      %this.camera.scopeToClient(%this);
   }

   //HiGuy: Let them use their new camera
   %this.setToggleCamera(true);

   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();
   if (isObject(%this.mesh))
      %this.mesh.delete();

   %this.overview = true;
   %this.spectating = false;
   commandToClient(%this, 'clearCenterPrint');

   commandToClient(%this, 'StartOverview', MissionInfo.overviewHeight, MisionInfo.overviewWidth);
}

function GameConnection::stopOverview(%this) {
   %this.overview = false;
   %this.spectating = false;

   if ($MPPref::OverviewFinishSpeed == 0) {
      %this.finishOverview();
      return;
   }

   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();
   if (isObject(%this.mesh))
      %this.mesh.delete();

   %this.spawnPlayer();
   %pos = %this.respawnPlayer();
   %this.player.setMode(Start);
   %this.setToggleCamera(true);
   %this.schedule($MPPref::OverviewFinishSpeed * 1000, finishOverview, %pos);

   %aimPos = getWords(getField(%pos, 0), 0, 2);
   %finalRot = getWord(getField(%pos, 0), 6);
   %finalPos = VectorSub(%aimPos, mSin(%finalRot) SPC mCos(%finalRot) SPC 0);
//   testahedron(%finalpos, true);

   //HiGuy: Finished overviewing, let them have control
   commandToClient(%this, 'FinishOverview', $MPPref::OverviewFinishSpeed * 1000, %aimPos, %finalPos, getField(%pos, 2));
}

function GameConnection::finishOverview(%this, %pos) {
   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();
   if (isObject(%this.mesh))
      %this.mesh.delete();

   commandToClient(%this, 'StopOverview');
   %this.spawnPoint = %pos;

   //HiGuy: Respawn the player, otherwise we won't be able to ghost anything!
   %this.spawnPlayer(%pos);
   %this.respawnPlayer(%pos);
   commandToAll('FixGhost');
}

function GameConnection::cancelOverview(%this, %pos) {
   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();
   if (isObject(%this.mesh))
      %this.mesh.delete();

   commandToClient(%this, 'StopOverview');
   commandToAll('FixGhost');
}
