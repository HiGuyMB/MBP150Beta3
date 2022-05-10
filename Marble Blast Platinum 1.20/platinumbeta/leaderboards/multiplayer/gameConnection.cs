//------------------------------------------------------------------------------
// Multiplayer Package
// gameConnection.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// HiGuy: GameConnection Commands

function GameConnection::startTimer(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'startTimer');
}

function GameConnection::stopTimer(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'stopTimer');
}

function GameConnection::resetTimer(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'resetTimer');
}

function GameConnection::setMessage(%this, %message, %timeout) {
   if (%this.fake) return;
   commandToClient(%this, 'setMessage', %message, %timeout);
}

// Jeff: sets quick respawn status
function GameConnection::setQuickRespawnStatus(%this, %status) {
   if (%this.fake) return;
   %this.canQuickRespawn = %status;
}

function GameConnection::setGemCount(%this, %gems) {
   if (%this.fake) return;
   if ($Server::ServerType $= "MultiPlayer") {
      %count = ClientGroup.getCount();
      %best = true;
      for (%i = 0; %i < %count; %i ++) {
         %client = ClientGroup.getObject(%i);
         if (%client.gemCount > %gems) {
            %best = false;
            break;
         }
      }
   } else
      %best = false;
   if (%best) {
      %count = ClientGroup.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %client = ClientGroup.getObject(%i);
         if (%client.fake)
            continue;

         commandToClient(%client, 'setGemCount', %client.gemCount, false);
      }
   }
   commandToClient(%this, 'setGemCount', %gems, %best);
}

function GameConnection::setMaxGems(%this, %gems) {
   if (%this.fake) return;
   commandToClient(%this, 'setMaxGems', %gems);
}

function GameConnection::addHelpLine(%this, %line, %playBeep) {
   if (%this.fake) return;
   commandToClient(%this, 'addHelpLine', %line, %playBeep);
}

function GameConnection::adjustTimer(%this, %time) {
   if (%this.fake) return;
   commandToClient(%this, 'adjustTimer', %time);
}

function GameConnection::addBonusTime(%this, %time) {
   if (%this.fake) return;
   commandToClient(%this, 'addBonusTime', %time);
}

function GameConnection::setTime(%this, %time) {
   if (%this.fake) return;
   commandToClient(%this, 'setTime', %time);
}

function GameConnection::setPowerUp(%this, %powerUp) {
   if (%this.fake) return;
   commandToClient(%this, 'setPowerUp', %powerUp);
}

function GameConnection::setGravityDir(%this, %dir, %reset, %rot) {
   if (%this.fake) return;
   %this.gravityDir = %dir;
   %this.gravityRot = %rot;
   commandToClient(%this, 'setGravityDir', %dir, %reset, %rot);
}

function GameConnection::applyImpulse(%this, %position, %impulse) {
   if (%this.fake) return;
	commandToClient(%this, 'ApplyImpulse', %position, %impulse);
}

function GameConnection::gravityImpulse(%this, %position, %impulse) {
   if (%this.fake) return;
	commandToClient(%this, 'GravityImpulse', %position, %impulse);
}

function GameConnection::fixGhost(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'FixGhost');
}

function GameConnection::hideGhosts(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'HideGhosts');
}

function GameConnection::resetGhost(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'ResetGhosts');
}

function GameConnection::hideGhost(%this) { //HiGuy: Because of possessives
   if (%this.fake) return;
   commandToClient(%this, 'HideMyGhost');
}

function GameConnection::hideMyGhost(%this) { //HiGuy: Grammatically incorrect
   if (%this.fake) return;
   commandToClient(%this, 'HideMyGhost');
}

function GameConnection::oobMessageHack(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'oobMessageHack');
}

function GameConnection::oobMessageHack2(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'oobMessageHack2');
}

function GameConnection::endGameSetup(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'EndGameSetup');
}

function GameConnection::incrementOOBCounter(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'incrementOOBCounter');
}

function GameConnection::setBlastValue(%this, %blastValue) {
   if (%this.fake) return;
   %this.blastValue = %blastValue;
   commandToClient(%this, 'setBlastValue', %blastValue);
}

function GameConnection::setSpecialBlast(%this, %specialBlast) {
   if (%this.fake) return;
   %user.client.usingSpecialBlast = %specialBlast;
   commandToClient(%this, 'setSpecialBlast', %specialBlast);
}

function GameConnection::radarInit(%this) {
   if (%this.fake) return;
   commandToClient(%this, 'RadarStart');
   commandToClient(%this, 'RadarBuildSearch');
}

// Jeff: TODO: UTALIZE THE GRAVITY (its getGravityDir() but MP friendly)
function GameConnection::makeBlastParticle(%this, %gravity) {
   if (%this.fake) return;
   %this.player.sendShockwave(%this.blastValue * (%this.usingSpecialBlast ? $MP::BlastShockwaveStrength : $MP::BlastRechargeShockwaveStrength));

   // Jeff: get the blast particles
   %emitter = (%this.usingSpecialBlast ? UltraBlastEmitter : BlastEmitter);
   %this.transferParticles(%emitter);
}

function updateSpawnSet(%grp) {
   if (!isObject(SpawnPointSet)) {
      new SimSet(SpawnPointSet);
      RootGroup.add(SpawnPointSet);
   }
   for (%i = 0; %i < %grp.getCount(); %i ++) {
      %obj = %grp.getObject(%i);
      if (%obj.getClassName() $= "Trigger" && %obj.getDataBlock().getName() $= "SpawnTrigger")
         SpawnPointSet.add(%obj);
      if (%obj.getClassName() $= "SimGroup")
         updateSpawnSet(%obj);
   }
}

function GameConnection::spawningBlocked(%this) {
   updateSpawnSet(MissionGroup);
   if (%this.fake) return;
   if (!isObject(SpawnPointSet))
      return false;

   %size = SpawnPointSet.getCount();
   if (%size == 0)
      return false;

   for (%i = 0; %i < %size; %i ++) {
      %obj = SpawnPointSet.getObject(%i);
      if (!isObject(%obj))
         continue;
      if (!%obj.block)
         return false;
   }
   return !isObject(StartPoint);
}

function GameConnection::getSpawnTrigger(%this) {
   if (%this.fake) return;
   if ($MPPref::Server::GlassMode && (%trigger = %this.getGlassSpawnTrigger()) != -1)
      return %trigger;
   if (!isObject(SpawnPointSet))
      return -1;

   %size = SpawnPointSet.getCount();
   if (%size == 0)
      return -1;

   %closest = 0;
   %distance = -1;

   if (getRealTime() - %this.lastSpawnTime > 4000)
      %this.lastSpawnTrigger = "";

   %this.lastSpawnTime = getRealTime();

   %playerPos = getWords((%this.player.lastTouch !$= "" ? %this.player.lastTouch : %this.player.getTransform()), 0, 2);

   if (!isObject(%this.player)) //HiGuy: We don't know *where* we'll spawn!
      return SpawnPointSet.getObject(getRandom(0, %size - 1));

   for (%i = 0; %i < %size; %i ++) {
      %obj = SpawnPointSet.getObject(%i);
      if (!isObject(%obj) || %obj == %this.lastSpawnTrigger || %obj.block)
         continue;
      %dist = VectorDist(%obj.getPosition(), %playerPos);
      if (%dist < %distance || %distance == -1) {
         %closest = %obj;
         %distance = %dist;
      }
   }

   if (%closest == 0) {
      error("Closest is 0!");
      error("Random spawning!");
      return SpawnPointSet.getObject(getRandom(0, %size - 1));
   }

   return %closest;
}

function GameConnection::getFurthestSpawnTrigger(%this) {
   if (%this.fake) return;
   if ($MPPref::Server::GlassMode && (%trigger = %this.getGlassSpawnTrigger()) != -1)
      return %trigger;
   if (!isObject(SpawnPointSet))
      return -1;

   %spawnCount = SpawnPointSet.getCount();
   if (%spawnCount == 0)
      return -1;

   %furthest = 0;
   %distance = -1;

   if (getRealTime() - %this.lastSpawnTime > 4000)
      %this.lastSpawnTrigger = "";

   %this.lastSpawnTime = getRealTime();

   %playerPos = getWords((%this.player.lastTouch !$= "" ? %this.player.lastTouch : %this.player.getTransform()), 0, 2);

   //HiGuy: The gem positions are kinda like the marble's pos... I guess?
   // If this happens, see ya on the other side of the level, sucker
   if ($Game::IsHunt && getRandom(0, 50) > 5)
      %playerPos = %this.getNearestGem().getPosition();

   if (!isObject(%this.player)) //HiGuy: We don't know *where* we'll spawn!
      return SpawnPointSet.getObject(getRandom(0, %spawnCount - 1));

   //HiGuy: Random entry
   %count = getRandom(0, %spawnCount) + 3;

   for (%i = 0; %i < %count; %i ++) {
      %obj = SpawnPointSet.getObject(%i);
      if (!isObject(%obj) || %obj == %this.lastSpawnTrigger || %obj.block)
         continue;
      %dist = VectorDist(%obj.getPosition(), %playerPos);

      //HiGuy: Pick the furthest trigger. This'll teach you to quickspawn abuse,
      // you ass! Next time roll for the damned gems.
      if (%dist > %distance || %distance == -1) {
         %furthest = %obj;
         %distance = %dist;
      }
   }

   if (%furthest == 0) {
      error("RS Furthest is 0!");
      error("RS Random spawning!");
      return SpawnPointSet.getObject(getRandom(0, %spawnCount - 1));
   }

   //HiGuy: random enough
   return %furthest;
}

function GameConnection::getRandomSpawnTrigger(%this) {
   if (%this.fake) return;
   if ($MPPref::Server::GlassMode && (%trigger = %this.getGlassSpawnTrigger()) != -1)
      return %trigger;
   if (!isObject(SpawnPointSet))
      return -1;

   %size = SpawnPointSet.getCount();
   if (%size == 0)
      return -1;

   if (getRealTime() - %this.lastSpawnTime > 4000)
      %this.lastSpawnTrigger = "";

   %this.lastSpawnTime = getRealTime();

   return SpawnPointSet.getObject(getRandom(0, %size - 1));
}
