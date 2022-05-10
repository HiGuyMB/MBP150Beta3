//------------------------------------------------------------------------------
// Multiplayer Package
// serverParticles.cs
//
// Copyright (c) 2013 MBP Team
//
//------------------------------------------------------------------------------

datablock ParticleEmitterNodeData(MPEmitterNode) {
   // Jeff: hack, timeMultiple sends across the network, so use this
   // to determine if we are a MPEmitterNode
   timeMultiple = 1.001;
};

// Jeff: see documentation on this in clientParticles.cs
// for the use of powerup emitters only in multiplayer.
datablock ParticleEmitterNodeData(MPPowerupNode) {
   timeMultiple = 1.002;
};

// Jeff: transfer particles to the clients
//
// %isPowerup is optional and lets us know if the particle if from a powerup
// this is used so that we can hide particles if they are our own on the client
function GameConnection::transferParticles(%this, %emitter, %isEnginePowerup) {
   // Jeff: failsafe checks to make sure this doesn't happen if we don't need it
   if ($Server::ServerType !$= "Multiplayer" || !isObject(%this.player) || !isObject(%emitter))
      return;

   // Jeff: %scale is based off of the current player's scale, used to determine
   // which particle emitter goes with which on the client side.
   %scale     = %this.player.getScale();
   %position  = %this.player.getPosition();
   %datablock = %isEnginePowerup ? MPPowerupNode : MPEmitterNode;

   %particle = new ParticleEmitterNode() {
      datablock = %datablock;
      emitter   = %emitter;
      position  = %position;
      rotation  = "1 0 0 0";
      scale     = %scale;
   };
   MissionCleanup.add(%particle);

   // Jeff: if we are a powerup, use that time, else use emitter.lifeTimeMS
   switch (%emitter) {
      case MarbleSuperJumpEmitter:
         %emitterTime = %this.player.getDataBlock().powerUpTime[1];
      case MarbleSuperSpeedEmitter:
         %emitterTime = %this.player.getDataBlock().powerUpTime[2];
      default:
         %emitterTime = %emitter.lifeTimeMS;
   }

   // Jeff: local host gets 0 for this but we want to help those a tiny bit
   // with latency issues.  ServerConnection is actually 15MS according to
   // the method, however for some odd reason client.getping on server is 0Ms
   %ping = %this.getPing();
   if (%ping < $PingMin)
      %ping = $PingMin;

   // Jeff: give extra time before deleting for latency.
   %time = getAveragePing() + %ping + %emitterTime + 100;
   %particle.schedule(%time, "delete");
}
