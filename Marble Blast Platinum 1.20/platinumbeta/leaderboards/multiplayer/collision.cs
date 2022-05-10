//------------------------------------------------------------------------------
// Multiplayer Package
// collision.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function MPupdateGhostCollision() {
   cancel($MP::Schedule::Collision);

   if (!$Server::Hosting || $Server::_Dedicated || $Server::ServerType $= "SinglePlayer")
      return;

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (!isObject(%client.player))
         continue;
      %player = %client.player;
      %ghost = %client.ghost;
      if (isObject(%player) && isObject(%ghost)) {
         %datablock1 = %client.player.getDatablock();
         %pos_p = %client.player.getPosition();
         for (%j = 0; %j < %count; %j ++) {
            if (%j == %i)
               continue;
            %clientJ = ClientGroup.getObject(%j);
            if (!isObject(%clientJ.player))
               continue;

            %client.speed = max(%client.speed, 0.001);
            %clientJ.speed = max(%clientJ.speed, 0.001);

            %datablock2 = %clientJ.player.getDatablock();
            %pos_o = %clientJ.player.getPosition();

            %dist = VectorDist(%pos_p, %pos_o);
            %d2 = %dist - %datablock1.shapeRadius - %datablock2.shapeRadius;

            if ($MP::Collision::EnableNudge && %d2 < 0) {
               %vec = VectorSub(%pos_p, %pos_o);
               if (%vec $= "0 0 0")
                  %vec = "0.1 0 0.0";
               %vec = VectorScale(VectorNormalize(%vec), (%datablock1.shapeRadius + %datablock2.shapeRadius) / 2);
               %inv = VectorScale(%vec, -1);
               %client.nudge(%vec);
               %clientJ.nudge(%inv);
               %client.noCol = 6;
               %clientJ.noCol = 6;
               continue;
            }
            if ($MP::Collision::EnableClip && %d2 < 0) {
               %client.clipping = true;
               %clientJ.clipping = true;
               %client.updateGhostDatablock();
               %clientJ.updateGhostDatablock();
            } else if ($MP::Collision::EnableClip) {
               if (%client.clipping) {
                  %client.clipping = false;
                  %client.updateGhostDatablock();
               }
               if (%clientJ.clipping) {
                  %client.clipping = false;
                  %clientJ.updateGhostDatablock();
               }
            }

            %dist -= %datablock1.impactRadius;
            %dist -= %datablock2.impactRadius;
            if (%dist < 0) {

               if (%client.lastCollision == %clientJ)
                  continue;
               if (%clientJ.lastCollision == %client)
                  continue;
               if (%client.lastColTime[%clientJ] + 1000 > getRealTime())
                  continue;
               if (%clientJ.lastColTime[%client] + 1000 > getRealTime())
                  continue;
               //echo("Contact was happen");

               //HiGuy: The faster player wins, unless one player has a Mega
               if ((%client.player.megaMarble == %clientJ.player.megaMarble && %client.speed < %clientJ.speed) || (%clientJ.player.megaMarble && !%client.player.megaMarble))
                  continue;

//               if (%client.speed < %datablock1.impactMinimum)
//                  continue;

//               %ray = ContainerRayCast(%client.player.position, VectorAdd(%client.player.position, %client.lastDifference), $TypeMasks::StaticShapeObjectType, %client.player);

               %skip = false;
               if (%client.noCol) {
                  %client.noCol --;
                  %skip = true;
               }
               if (%clientJ.noCol) {
                  %clientJ.noCol --;
                  %skip = true;
               }
               if (%skip)
                  continue;

               //collide
               %client.lastCollision = %clientJ;
               %clientJ.lastCollision = %client;
               %client.lastColTime[%clientJ] = getRealTime();
               %clientJ.lastColTime[%client] = getRealTime();

               //Maximum impulse multiplier =D
               %maximum  = %datablock1.impactMaximum;
               %maximum2 = %datablock2.impactMaximum;

               //Default multiplier
               %multiplier  = %datablock1.impactMultiplier;
               %multiplier2 = %datablock2.impactMultiplier;

               //Default reduction
               %reduction  = %datablock1.impactReduction;
               %reduction2 = %datablock2.impactReduction;

               //Calculate marble speed
               %bSpeed = %client.speed + (%clientJ.speed * %datablock1.impactBounceBack);

               //Get source vectors
               %source  = VectorSub(%pos_o, %pos_p);
               %source2 = VectorSub(%pos_p, %pos_o);

               //Get impulse vector
               %affect = %source;
               %affect2 = %source2;

               //Get impulse strength
               %affection  = min(%bSpeed * %multiplier,  %maximum);
               %affection2 = min(%bSpeed * %multiplier2, %maximum2);

               if (isObject(%clientJ.team) && isObject(%client.team)) {
                  if (%clientJ.team.getId() == %client.team.getId()) {
                     %affection  /= $MP::CollisionTeamDampen;
                     %affection2 /= $MP::CollisionTeamDampen;
                  }
               }

               //Scale impulse vector to stength
               %affect  = VectorScale(%affect,  %affection);
               %affect2 = VectorScale(%affect2, %reduction2);

               //echo("//---------" NL "Max" SPC %maximum NL "Mult" SPC %multiplier NL "BSpeed" SPC %bSpeed NL "Source" SPC %source NL "Affect" SPC %affect NL "Affection" SPC %affection NL "ClientSpeed" SPC %client.speed NL "ClientJSpeed" SPC %clientJ.speed NL "I" SPC %i NL "J" SPC %j NL "//---------");

               //only affect the ghosted client...
               if (!%client.disableCollision)
                  commandToClient(%clientJ,'applyImpulse',%source,%affect);
               if (!%clientJ.disableCollision)
                  commandToClient(%client,'applyImpulse',%source2,%affect2);

               if (%client.player.megaMarble || %clientJ.player.megaMarble) {
                  %sfx = eval("return MegaMarble.bounce" @ getRandom(1, 4) @ ";");
                  %sfx2 = eval("return MegaMarble.bounce" @ getRandom(1, 4) @ ";");
                  %client.play2d(%sfx);
                  %clientJ.play2d(%sfx);
               }
            } else {
               %client.lastCollision = "";
            }
         }
      }
   }
   $MP::Schedule::Collision = schedule($MP::Collision::Delta, 0, "MPupdateGhostCollision");
}

function Marble::getFrontPosition(%this) {
   %center = %this.getWorldBoxCenter();
   %diff = %this.client.posDiff;
   %diff = VectorNormalize(%diff);
   %diff = VectorScale(%diff, 0.2);
   return VectorAdd(%center, %diff);
}

function Marble::getBackPosition(%this) {
   %center = %this.getWorldBoxCenter();
   %diff = %this.client.posDiff;
   %diff = VectorNormalize(%diff);
   %diff = VectorScale(%diff, 0.2);
   return VectorSub(%center, %diff);
}

function Marble::getWidth(%this) {
   return VectorDot(%this.scale, "0.2 0.2 0.2") / 3;
}
