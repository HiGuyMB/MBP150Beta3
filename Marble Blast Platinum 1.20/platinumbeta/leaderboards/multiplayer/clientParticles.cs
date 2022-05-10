//------------------------------------------------------------------------------
// Multiplayer Package
// clientParticles.cs
//
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

// Jeff: this global simset is used for client emitters that need updates.
// much faster than reorganizing a list of global array variables!
if (!isObject(ClientEmitterSet)) {
   new SimSet(ClientEmitterSet);
   RootGroup.add(ClientEmitterSet);
}
if (!isObject(ClientTrailEmitterSet)) {
   new SimSet(ClientTrailEmitterSet);
   RootGroup.add(ClientTrailEmitterSet);
}

// Jeff: this recursivly calls every 50 milliseconds to determine if we have emitters
// that we need to attach to the clients.  If we do, add it to the emitter list
// and update the transforms!
function lookForEmitters() {
   if ($Server::ServerType $= "" || !isObject(ServerConnection) || $fast)
      return;
   cancel($MP::Schedule::LookEmitters);
   if (!isObject(ServerConnection))
      return;

   // Jeff: this will save CPU time if the amount of objects
   // remain the same, why would we want to continue looping again?
   %count = ServerConnection.getCount();
   if ($MP::LastServerConnectionCount != %count) {
      for (%i = 0; %i < %count; %i ++) {
         %obj = ServerConnection.getObject(%i);

         // Jeff: only particleEmitterNodes have time multiples, and only
         // the particles we need have this time multiple.
         //
         // time multiple of 1.001 means that it will never hide until the
         // object is deleted on the server.  However, a time multiple of
         // 1.002 will be hidden if it is our own emitter.  for example, the
         // powerups render there effects on your own marble already, why waste
         // CPU showing another emitter when we see one?
         if (%obj.getClassName() $= "ParticleEmitterNode") {
            %time = %obj.getDatablock().timeMultiple;
            if (!isClientEmitter(%time))
               continue;
            //devecho("Found emitter with time multiple" SPC %time SPC "with scale" SPC %obj.getScale());

            // Jeff: it is assigned to a marble object already, why are we still
            // in here?!  Don't waste CPU, get out of here.
            if (isObject(%obj.marble))
               continue;

            %scale = %obj.getScale();

            // Jeff: if we are a powerup emitter and it is our own, hide it and
            // do not add it to the list.  We are done.  Note: this is for
            // engine powerups only, as the new defined powerups still need this
            if (%time $= "1.002" && %scale $= $MP::MyScale) {
               //devecho("We have our own particle emitter.  We don't want to waste CPU.");
               //devecho("Because of this, we are now hiding it at location -999999 -999999 -999999");
               %obj.setTransform("-999999 -999999 -999999 1 0 0 0");
               continue;
            }

            // Jeff: find the marble that the object will need from the lookup
            // the lookup variable is based off of scales.
            %ghost = $MP::GhostLookup[%scale];
            %ghost = $MP::GhostIndex[%ghost];

            // Jeff: if the ghost is not found, just continue and quit wasting
            // CPU already. ermergerd why do we want to waste so much CPU!
            if (!isObject(%ghost))
               continue;

            //HiGuy: this will speed up execution for your own emitters
            // makes it buttery smooth.
            if (%ghost.getScale() $= $MP::MyScale)
               %ghost = $MP::MyMarble;

            // Jeff: add it to the list, hackish but works.  We have to force
            // update anyways!
            %obj.marble = %ghost;

            // Jeff: if we are a trail emitter, then we are special and added to
            // a different group.
            if (%time $= "1.003") {
               %obj.gold = true;
               ClientTrailEmitterSet.add(%obj);
            } else if (%time $= "1.004") {
               // Jeff: white emitter.
               %obj.gold = false;
               ClientTrailEmitterSet.add(%obj);
            } else
               ClientEmitterSet.add(%obj);
         }
      }
   }


   $MP::LastServerConnectionCount = %count;

   // Jeff: reloop in 50 milliseconds to check again!
   $MP::Schedule::LookEmitters = schedule(50, 0, lookForEmitters);
}

// Jeff: determines if we are a emitter that we need to check for
function isClientEmitter(%time) {
   return (%time $= "1.001" || %time $= "1.002" || %time $= "1.003" || %time $= "1.004");
}

// Jeff: this function updates the emitter positions client sided, approx every
// 10 milliseconds SO WE GET GOOD UPDATES.  Also no performance hits noticed
// even going this fast.
function updateEmitterPositions()  {
   if ($Server::ServerType $= "" || !isObject(ServerConnection) || $fast)
      return;
   cancel($MP::Schedule::EmitterPosition);

   %count = ClientEmitterSet.getCount();
   %removeAmount = 0;
   for (%i = 0; %i < %count; %i ++) {
      %obj = ClientEmitterSet.getObject(%i);

      // Jeff: the ghost object got deleted, so we need to remove this from
      // the set.
      if (!isObject(%obj.marble)) {
         %remove[%removeAmount] = %obj;
         %removeAmount ++;
         continue;
      }

      %obj.setTransform(%obj.marble.getPosition() SPC "1 0 0 0");
   }

   // Jeff: clean up old emitters
   for (%i = 0; %i < %removeAmount; %i ++)
      ClientEmitterSet.remove(%remove[%i]);

   // Jeff: trail emitters
   updateTrailEmitters();

   $MP::Schedule::EmitterPosition = schedule(10, 0, updateEmitterPositions);
}

// Jeff: this function updates the trail emitters to the correct positions
// based upon velocities.
function updateTrailEmitters() {
   if ($Server::ServerType $= "" || !isObject(ServerConnection) || $fast)
      return;
   // Jeff: used to calculate speed
   %time  = getRealTime();
   %delta = %time - $MP::LastTrailTime;
   if (%delta < 1)
      return;
   $MP::LastTrailTime = %time;

   %scale = 1000 / %delta;

   %count = ClientTrailEmitterSet.getCount();
   %removeAmount = 0;
   for (%i = 0; %i < %count; %i ++) {
      %obj = ClientTrailEmitterSet.getObject(%i);

      // Jeff: if the marble got deleted, we need to remove it form the set
      if (!isObject(%obj.marble)) {
         %remove[%removeAmount] = %obj;
         %removeAmount ++;
         continue;
      }

      // Jeff: speed calculation
      %pos   = %obj.marble.getPosition();
      %speed = vectorDist(%pos, %obj.lastPos) * %scale;
      %obj.lastPos = %pos;

      // Jeff: the new position to display the client particle.
      %position = %obj.marble.getPosition();

      // Jeff: determine visibility
      // if we have a high enough velocity, display it, else hide it.
      if (%obj.gold) {
         if (%speed > $TrailEmitterWhiteSpeed)
            %obj.setTransform("-999999 -999999 -999999 1 0 0 0");
         else if (%speed > $TrailEmitterSpeed)
            %obj.setTransform(%position SPC "1 0 0 0");
         else
            %obj.setTransform("-999999 -999999 -999999 1 0 0 0");
      } else {
         if (%speed > $TrailEmitterWhiteSpeed)
            %obj.setTransform(%position SPC "1 0 0 0");
         else
            %obj.setTransform("-999999 -999999 -999999 1 0 0 0");
      }
   }

   // Jeff: clean up old trail emitters
   for (%i = 0; %i < %removeAmount; %i ++)
      ClientTrailEmitterSet.remove(%remove[%i]);
}


//-----------------------------------------------------------------------------
// Jeff: marble trail functions (SINGLE PLAYER ONLY)
//-----------------------------------------------------------------------------

// Jeff: attaches the trail emitter to the marble (single player only)
//
// This is one ugly method, so kind of just ignore it please.
// It gets the job done.
function attachMarbleTrail(%scale) {
   if (!isObject(ServerConnection) || !isObject(localClientConnection))
      return;

   %player = localClientConnection.player;
   if (!isObject(%player))
      return;

   //echo("on frame");

   // Jeff: if we found the object, don't just waste CPU all day!
   if (isObject($MarbleTrail) && isObject($MarbleWhiteTrail)) {

      // Jeff: calculate speed
      %position = %player.getPosition();
      %_velocity = vectorDist(%position, %player.lastTrailPosition) * %player.lastDelta;
      //echo("Scale:" SPC %scale SPC "Pos:" SPC %position SPC "Last:" SPC  %player.lastTrailPosition SPC "Dist:" SPC vectorDist(%position, %player.lastTrailPosition) SPC "Vel:" SPC %velocity);
      %player.lastDelta = 1000 / %scale;
      %player.lastTrailPosition = %position;
      %velocity = (%_velocity + %player.lastVelocity) / 2;
      %player.lastVelocity = %_velocity;

      // Jeff: if we have a high enough velocity, display it, else hide it.
      if (%velocity > $TrailEmitterWhiteSpeed) {
         $MarbleWhiteTrail.setTransform(%position SPC "1 0 0 0");
         $MarbleTrail.setTransform("-999999 -999999 -999999 1 0 0 0");
      } else if (%velocity > $TrailEmitterSpeed) {
         $MarbleWhiteTrail.setTransform("-999999 -999999 -999999 1 0 0 0");
         $MarbleTrail.setTransform(%position SPC "1 0 0 0");
      } else {
         $MarbleTrail.setTransform("-999999 -999999 -999999 1 0 0 0");
         $MarbleWhiteTrail.setTransform("-999999 -999999 -999999 1 0 0 0");
      }
   } else {

      // Jeff: we don't have it yet, so find it!
      %count = ServerConnection.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %obj = ServerConnection.getObject(%i);
         if (%obj.getClassName() $= "ParticleEmitterNode") {
            // Jeff: we do scale comparison for single player to see
            // which one is which

            %scale = %obj.getScale();

            if (%scale $= "0.9998 0.9998 0.9998") {
               $MarbleTrail = %obj;
               %player.lastTrailPosition = %player.getPosition();
               %player.lastTrailDelta = getRealTime();
            } else if (%scale $= "0.9997 0.9997 0.9997")
               $MarbleWhiteTrail = %obj;
         }
      }
   }
}
