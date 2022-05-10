//-----------------------------------------------------------------------------
// Blast.cs
//
// Hax central, but works
//
// Implemented for any Marble Blast; any version
// So long as a few things are kept similar
//
// Copyright (c) 2011-2012 HiGuy Smith
// Copyright (c) 2011-2012 Jeff Hutchinson
//
// From Project Revolution
// The MultiPlayer MarbleBlast experience
//
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Blast Update Stuff

// Jeff: I am updating this to be client sided so that it makes it faster
// it is called in onFrameAdvance in playGui.cs
function serverBlastUpdate(%timeDelta) {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %blastValue = %client.blastValue;
      //HiGuy: Update blast value
      %blastValue += (%timeDelta / $MP::BlastChargeTime);
      //HiGuy: Normalize blast value
      //Keep it 0 < value < 1
      if (%client.usingSpecialBlast)
         %blastValue = 1;
      if (%blastValue < 0)
         %blastValue = 0;
      else if (%blastValue > 1)
         %blastValue = 1;
      %client.blastValue = %blastValue; //HiGuy: Doesn't send to client
   }
}

//-----------------------------------------------------------------------------
// Blast function
// Where the knitty gritty is done

function serverCmdBlast(%client, %gravity) {
   //Display blast particles and make explosion
   %client.makeBlastParticle(%gravity);
   ServerPlay3D(blastSfx, %client.player.getWorldBoxCenter());
   %client.setSpecialBlast(false);
   %client.setBlastValue(0); //HiGuy: Sends to client
}

//-----------------------------------------------------------------------------
// Blast Particle
//-----------------------------------------------------------------------------

datablock ParticleData(BlastSmoke) {
   textureName          = "~/data/particles/smoke";
   dragCoefficient      = 1;
   gravityCoefficient   = 0;
   inheritedVelFactor   = 0;
   windCoefficient      = 0;
   constantAcceleration = 0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 100;
   spinSpeed     = 20;
   spinRandomMin = 0.0;
   spinRandomMax = 0.0;
   useInvAlpha   = true;

   colors[0]     = "0 1 1 0.1";
   colors[1]     = "0 1 1 0.5";
   colors[2]     = "0 1 1 0.9";

   sizes[0]      = 0.125;
   sizes[1]      = 0.125;
   sizes[2]      = 0.125;

   times[0]      = 0.0;
   times[1]      = 0.4;
   times[2]      = 1.0;
};

datablock ParticleData(UltraBlastSmoke) {
   textureName          = "~/data/particles/smoke";
   dragCoefficient      = 1;
   gravityCoefficient   = 0;
   inheritedVelFactor   = 0;
   windCoefficient      = 0;
   constantAcceleration = 0;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 100;
   spinSpeed     = 20;
   spinRandomMin = 0.0;
   spinRandomMax = 0.0;
   useInvAlpha   = true;

   colors[0]     = "0 0.8 0.2 0.1";
   colors[1]     = "0 0.8 0.2 0.5";
   colors[2]     = "0 0.8 0.2 0.9";

   sizes[0]      = 0.125;
   sizes[1]      = 0.125;
   sizes[2]      = 0.125;

   times[0]      = 0.0;
   times[1]      = 0.4;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(BlastEmitter) {
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionVelocity = 4;
   velocityVariance = 0;
   ejectionOffset   = 0;
   thetaMin         = 90;
   thetaMax         = 100;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   lifetimeMS       = 500;
   particles        = "BlastSmoke";
};

datablock ParticleEmitterData(UltraBlastEmitter) {
   ejectionPeriodMS = 1;
   periodVarianceMS = 0;
   ejectionVelocity = 4;
   velocityVariance = 0;
   ejectionOffset   = 0;
   thetaMin         = 90;
   thetaMax         = 100;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   lifetimeMS       = 500;
   particles        = "UltraBlastSmoke";
};

function Marble::sendShockwave(%this, %strength) {
   %mePos = %this.getWorldBoxCenter();
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %myMod = %this.getDataBlock().blastModifier;
      %theyMod = ClientGroup.getObject(%i).player.getDataBlock().blastModifier;
      commandToAllExcept(%this.client, 'Shockwave', %mePos, %strength, %myMod, %theyMod);
   }
}
