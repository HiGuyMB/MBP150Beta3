//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

datablock ParticleData(BounceParticle)
{
   textureName          = "~/data/particles/star";
   dragCoeffiecient     = 1.0;
   gravityCoefficient   = 0;
   windCoefficient      = 0;
   inheritedVelFactor   = 0;
   constantAcceleration = -2;
   lifetimeMS           = 500;
   lifetimeVarianceMS   = 100;
   useInvAlpha =  true;
   spinSpeed     = 90;
   spinRandomMin = -90.0;
   spinRandomMax =  90.0;

   colors[0]     = "0.9 0.0 0.0 1.0";
   colors[1]     = "0.9 0.9 0.0 1.0";
   colors[2]     = "0.9 0.9 0.0 0.0";

   sizes[0]      = 0.25;
   sizes[1]      = 0.25;
   sizes[2]      = 0.25;

   times[0]      = 0;
   times[1]      = 0.75;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(MarbleBounceEmitter)
{
   ejectionPeriodMS = 80;
   periodVarianceMS = 0;
   ejectionVelocity = 3.0;
   velocityVariance = 0.25;
   thetaMin         = 80.0;
   thetaMax         = 90.0;
   lifetimeMS       = 250;
   particles = "BounceParticle";
};

//-----------------------------------------------------------------------------
// Jeff: these aren't used but we are keeping them so the engine don't go
// KABOOM
//
// Reason for not using them: they aren't sexy enough.

datablock ParticleData(TrailParticle)
{
   textureName          = "~/data/particles/smoke";
   dragCoeffiecient     = 1.0;
   gravityCoefficient   = 0;
   windCoefficient      = 0;
   inheritedVelFactor   = 1;
   constantAcceleration = 0;
   lifetimeMS           = 100;
   lifetimeVarianceMS   = 10;
   useInvAlpha =  true;
   spinSpeed     = 0;

   colors[0]     = "1 1 0 0.0";
   colors[1]     = "1 1 0 1";
   colors[2]     = "1 1 1 0.0";

   sizes[0]      = 0.7;
   sizes[1]      = 0.4;
   sizes[2]      = 0.1;

   times[0]      = 0;
   times[1]      = 0.15;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(MarbleTrailOldEmitter)
{
   ejectionPeriodMS = 5;
   periodVarianceMS = 0;
   ejectionVelocity = 0.12; //HiGuy: Fixed values
   velocityVariance = 0.12; // Jeff: but my new sexy emitters are fixed better!
   thetaMin         = 80.0;
   thetaMax         = 90.0;
   lifetimeMS       = 10000;
   particles = "TrailParticle";
};

//-----------------------------------------------------------------------------

datablock ParticleData(SuperJumpParticle)
{
   textureName          = "~/data/particles/twirl";
   dragCoefficient      = 0.25;
   gravityCoefficient   = 0;
   inheritedVelFactor   = 0.1;
   constantAcceleration = 0;
   lifetimeMS           = 1000;
   lifetimeVarianceMS   = 150;
   spinSpeed     = 90;
   spinRandomMin = -90.0;
   spinRandomMax =  90.0;

   colors[0]     = "0 0.5 1 0";
   colors[1]     = "0 0.6 1 1.0";
   colors[2]     = "0 0.6 1 0.0";

   sizes[0]      = 0.25;
   sizes[1]      = 0.25;
   sizes[2]      = 0.5;

   times[0]      = 0;
   times[1]      = 0.75;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(MarbleSuperJumpEmitter)
{
   ejectionPeriodMS = 10;
   periodVarianceMS = 0;
   ejectionVelocity = 1.0;
   velocityVariance = 0.25;
   thetaMin         = 150.0;
   thetaMax         = 170.0;
   lifetimeMS       = 5000;
   particles = "SuperJumpParticle";
};

//-----------------------------------------------------------------------------

datablock ParticleData(SuperSpeedParticle)
{
   textureName          = "~/data/particles/spark";
   dragCoefficient      = 0.25;
   gravityCoefficient   = 0;
   inheritedVelFactor   = 0.25;
   constantAcceleration = 0;
   lifetimeMS           = 1500;
   lifetimeVarianceMS   = 150;

   // Phil: I've always felt the super speed's colour is too yellow, and feels more suited to MBG.
   // These changes make it closer to a white colour:
   colors[0]     = "0.8 0.8 0.2 0.0";
   colors[1]     = "0.8 0.8 0.2 1.0";
   colors[2]     = "0.8 0.8 0.2 0.0";

   sizes[0]      = 0.25;
   sizes[1]      = 0.25;
   sizes[2]      = 1.0;

   times[0]      = 0;
   times[1]      = 0.25;
   times[2]      = 1.0;
};

datablock ParticleEmitterData(MarbleSuperSpeedEmitter)
{
   ejectionPeriodMS = 5;
   periodVarianceMS = 0;
   ejectionVelocity = 1.0;
   velocityVariance = 0.25;
   thetaMin         = 130.0;
   thetaMax         = 170.0;
   lifetimeMS       = 5000;
   particles = "SuperSpeedParticle";
};

//-----------------------------------------------------------------------------
// Jeff: new trail emitter

datablock ParticleData(MarbleWhiteTrailParticle) {
   textureName          = "~/data/particles/smoke";
   dragCoefficient      = 0.25;
   gravityCoefficient   = 0;
   inheritedVelFactor   = 0.25;
   constantAcceleration = 0;
   lifetimeMS           = 1000;
   lifetimeVarianceMS   = 150;
   spinSpeed     = 20;
   spinRandomMin = 0.0;
   spinRandomMax = 0.0;
   useInvAlpha   = true;

   colors[0]     = "1 1 1 0.35";
   colors[1]     = "1 1 1 1.0";
   colors[2]     = "1 1 1 0.85";

   sizes[0]      = 0.05;
   sizes[1]      = 0.10;
   sizes[2]      = 0.15;

   times[0]      = 0;
   times[1]      = 0.5;
   times[2]      = 1.0;
};

datablock ParticleData(MarbleTrailParticle : MarbleWhiteTrailParticle) {
   textureName = "~/data/particles/spark";

   // Jeff: lifetime until particle disappears
   lifeTimeMS = 1000;

   colors[0] = "1 1 0.25 0.25";
   colors[1] = "1 1 0.75 1";
   colors[2] = "1 1 0.15 0.0";

   sizes[0]      = 0.05;
   sizes[1]      = 0.10;
   sizes[2]      = 0.35;
};

datablock ParticleEmitterData(MarbleTrailEmitter) {
   className = "HelicopterEmitter";
   ejectionPeriodMS = 20;
   periodVarianceMS = 8;
   ejectionVelocity = 1.0;
   velocityVariance = 0.25;
   thetaMin         = 90;
   thetaMax         = 100;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   lifetimeMS       = 0;
   particles = "MarbleTrailParticle";
};

datablock ParticleEmitterData(MarbleWhiteTrailEmitter : MarbleTrailEmitter) {
   ejectionPeriodMS = 10;
   lifetimeMS = 0;
   particles  = "MarbleWhiteTrailParticle";
};

datablock ParticleEmitterNodeData(ParticleTrailNode) {
   timeMultiple = 1.003;
};

datablock ParticleEmitterNodeData(ParticleWhiteTrailNode) {
   timeMultiple = 1.004;
};

//-----------------------------------------------------------------------------
// ActivePowerUp
// 0 - no active powerup
// 1 - Super Jump
// 2 - Super Speed
// 3 - Super Bounce
// 4 - Indestructible

datablock AudioProfile(Bounce1Sfx)
{
   filename    = "~/data/sound/bouncehard1.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(Bounce2Sfx)
{
   filename    = "~/data/sound/bouncehard2.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(Bounce3Sfx)
{
   filename    = "~/data/sound/bouncehard3.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(Bounce4Sfx)
{
   filename    = "~/data/sound/bouncehard4.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(JumpSfx)
{
   filename    = "~/data/sound/Jump.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(RollingHardSfx)
{
   filename    = "~/data/sound/Rolling_Hard.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock AudioProfile(SlippingSfx)
{
   filename    = "~/data/sound/Sliding.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock MarbleData(DefaultMarble)
{
   shapeFile = "~/data/shapes/balls/ball-superball.dts";
   emap = true;
   renderFirstPerson = true;
// maxRollVelocity = 55;
// angularAcceleration = 120;
   maxRollVelocity = 15;
   angularAcceleration = 75;
   brakingAcceleration = 30;
   gravity = 20;
   staticFriction = 1.1;
   kineticFriction = 0.7;
   bounceKineticFriction = 0.2;
   maxDotSlide = 0.5;
   bounceRestitution = 0.5;
   jumpImpulse = 7.5;
   maxForceRadius = 50;

   bounce1 = Bounce1Sfx;
   bounce2 = Bounce2Sfx;
   bounce3 = Bounce3Sfx;
   bounce4 = Bounce4Sfx;

   rollHardSound = RollingHardSfx;
   slipSound = SlippingSfx;
   jumpSound = JumpSfx;

   // Emitters
   // Jeff: 1.50 update: changed minTrailSpeed to 20 as per matan's request.
   // WE use our own, lets make it impossible to have!!!!
   minTrailSpeed = 99999999;            // Trail threshold
   trailEmitter = MarbleTrailOldEmitter;

   minBounceSpeed = 3;           // Bounce threshold
   bounceEmitter = MarbleBounceEmitter;

   powerUpEmitter[1] = MarbleSuperJumpEmitter; 		// Super Jump
   powerUpEmitter[2] = MarbleSuperSpeedEmitter; 	// Super Speed
// powerUpEmitter[3] = MarbleSuperBounceEmitter; 	// Super Bounce
// powerUpEmitter[4] = MarbleShockAbsorberEmitter; 	// Shock Absorber

   // Jeff: 1.50 update: helicopter now has an emitter.  But we still don't use
   // this, have a new way of doing it.
// powerUpEmitter[5] = MarbleHelicopterEmitter; 	// Helicopter

   // Power up timouts. Timeout on the speed and jump only affect
   // the particle trail
   // Jeff: 1.50 update: helicopter now has emitter.  it is the same time
   // as powerupTime[5]
   powerUpTime[1] = 1000;	// Super Jump
   powerUpTime[2] = 1000; 	// Super Speed
   powerUpTime[3] = 5000; 	// Super Bounce
   powerUpTime[4] = 5000; 	// Shock Absorber
   powerUpTime[5] = 5000; 	// Helicopter

   // Allowable Inventory Items
   maxInv[SuperJumpItem] = 20;
   maxInv[SuperSpeedItem] = 20;
   maxInv[SuperBounceItem] = 20;
   maxInv[IndestructibleItem] = 20;
   maxInv[TimeTravelItem] = 20;
//   maxInv[GoodiesItem] = 10;

   // Jeff: new fields for multiplayer.

   shapeRadius = 0.12; //If you're closer than this, you're conjoined :)

   // Jeff & HiGuy: Impact forces for collision
   impactRadius = 0.27; //Do not touch
   impactMinimum = 0.3;
   impactMultiplier = $MP::Collision::Multiplier; //Works best
   impactMaximum = $MP::Collision::Maximum;
   impactReduction = 0.25;
   impactBounceBack = 0.5;

   blastModifier = $MP::NormalBlastModifier; //Multiplies blast values by this
};

// Phil - Less marble datablocks now...
datablock MarbleData(ThreeDMarble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/3dMarble.dts";
};

datablock MarbleData(MidPMarble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/midp.dts";
};

datablock MarbleData(ggMarble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/garageGames.dts";
};

datablock MarbleData(sm1Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/sm1.dts";
};

datablock MarbleData(sm2Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/sm2.dts";
};

datablock MarbleData(sm3Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/sm3.dts";
};

datablock MarbleData(bm1Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/bm1.dts";
};

datablock MarbleData(bm2Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/bm2.dts";
};

datablock MarbleData(bm3Marble : DefaultMarble)
{
	shapeFile = $usermods @ "/data/shapes/balls/bm3.dts";
};

if (!$LB::LoggedIn && !$Server::Dedicated) {
   datablock MarbleData(CustomMarble : DefaultMarble)
   {
      shapeFile = getField(MarbleSelectGui.getSelection(), 0);
      skin = getField(MarbleSelectGui.getSelection(), 1);
   };
} else {
   // Jeff: doing this for lb marble
   datablock MarbleData(LBDefaultMarble : DefaultMarble)
   {
      shapeFile = $usermods @ "/data/shapes/balls/ball-superball.dts";
   };
   datablock MarbleData(LB3DMarble : DefaultMarble)
   {
      shapeFile = $usermods @ "/data/shapes/balls/3dMarble.dts";
   };
   datablock MarbleData(LBMidPMarble : DefaultMarble)
   {
      shapeFile = $usermods @ "/data/shapes/balls/midp.dts";
   };
   datablock MarbleData(LBGarageGamesMarble : DefaultMarble)
   {
      shapeFile = $usermods @ "/data/shapes/balls/garageGames.dts";
   };
//   datablock MarbleData(LBPack1Marble : DefaultMarble)
//   {
//      shapeFile = $usermods @ "/data/shapes/balls/.pack1/pack1marble.dts";
//   };
   $LB::MarbleDatablock[0] = LBDefaultMarble;
   $LB::MarbleDatablock[1] = LB3DMarble;
   $LB::MarbleDatablock[2] = LBMidPMarble;
   $LB::MarbleDatablock[3] = LBGarageGamesMarble;
//   $LB::MarbleDatablock[4] = LBPack1Marble;
}
//------------------------------------------------------------------------------

datablock AudioProfile(MegaBounce1Sfx)
{
   filename    = "~/data/sound/mega_bouncehard1.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MegaBounce2Sfx)
{
   filename    = "~/data/sound/mega_bouncehard2.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MegaBounce3Sfx)
{
   filename    = "~/data/sound/mega_bouncehard3.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MegaBounce4Sfx)
{
   filename    = "~/data/sound/mega_bouncehard4.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MegaJumpSfx)
{
   filename    = "~/data/sound/Jump.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(MegaRollingHardSfx)
{
   filename    = "~/data/sound/mega_roll.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock AudioProfile(MegaSlippingSfx)
{
   filename    = "~/data/sound/Sliding.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock MarbleData(MegaMarble : DefaultMarble) {
   shapeFile = "~/data/shapes/balls/megamarble.dts";
   emap = true;
   renderFirstPerson = true;
   maxRollVelocity = 12;
   angularAcceleration = 60;
   brakingAcceleration = 25;
   gravity = 22;
   staticFriction = 1.0;
   kineticFriction = 0.8;
   bounceKineticFriction = 0.3;
   maxDotSlide = 0.3;
   bounceRestitution = 0.5;
   jumpImpulse = 7.5;
   maxForceRadius = 75;

   bounce1 = MegaBounce1Sfx;
   bounce2 = MegaBounce2Sfx;
   bounce3 = MegaBounce3Sfx;
   bounce4 = MegaBounce4Sfx;

   rollHardSound = MegaRollingHardSfx;
   slipSound = MegaSlippingSfx;
   jumpSound = MegaJumpSfx;

   shapeRadius = 0.45; //If you're closer than this, you're conjoined :)

   // Jeff & HiGuy: Impact forces for collision
   impactRadius = 0.93;
   impactMinimum = 0.1;
   impactMultiplier = $MP::Collision::MegaMultiplier;
   impactMaximum = $MP::Collision::MegaMaximum;
   impactReduction = 0.1;
   impactBounceBack = 0.10;

   blastModifier = $MP::MegaBlastModifier; //Multiplies blast values by this
};

datablock MarbleData(MegaMarble3dMarble : MegaMarble) {
   shapeFile = "~/data/shapes/balls/megamarble-3dMarble.dts";
};

datablock MarbleData(MegaMarbleMidP : MegaMarble) {
   shapeFile = "~/data/shapes/balls/megamarble-midp.dts";
};

datablock MarbleData(MegaMarblePack1 : MegaMarble) {
   shapeFile = "~/data/shapes/balls/.pack1/pack1mega.dts";
};

//HiGuy: Adjusted datablocks for disable diagonal handicap
if ($Server::ServerType $= "MultiPlayer" && !$Server::Dedicated) {
   datablock MarbleData(__DefaultMarble : DefaultMarble) { angularAcceleration = DefaultMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__LBDefaultMarble : LBDefaultMarble) { angularAcceleration = DefaultMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__LB3DMarble : LB3DMarble) { angularAcceleration = LB3DMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__LBMidPMarble : LBMidPMarble) { angularAcceleration = LBMidPMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__MegaMarble : MegaMarble) { angularAcceleration = MegaMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__MegaMarble3dMarble : MegaMarble3dMarble) { angularAcceleration = MegaMarble3dMarble.angularAcceleration / $sqrt_2; };
   datablock MarbleData(__MegaMarbleMidP : MegaMarbleMidP) { angularAcceleration = MegaMarbleMidP.angularAcceleration / $sqrt_2; };
}

$MP::MegaDatablock[0] = MegaMarble;
$MP::MegaDatablock[1] = MegaMarble3dMarble;
$MP::MegaDatablock[2] = MegaMarbleMidP;
$MP::MegaDatablock[4] = MegaMarblePack1;

datablock ShapeBaseImageData(TeamRingImage)
{
   // Basic Item properties
   shapeFile = "~/data/shapes/images/teamring.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0.2";
   ignoreMountRotation = true;
};


//-----------------------------------------------------------------------------

function MarbleData::onAdd(%this, %obj)
{
   //echo("New Marble: " @ %obj);
}

function MarbleData::onTrigger(%this, %obj, %triggerNum, %val)
{
}


//-----------------------------------------------------------------------------

function MarbleData::onCollision(%this,%obj,%col)
{
   // Try and pickup all items
   if (%col.getClassName() $= "Item")
   {
      //HiGuy: No after-finish pickups (sorry, Me)
      if (%obj.client.state $= "End")
         return;

      %data = %col.getDatablock();
      %obj.pickup(%col,1);
   }
}


//-----------------------------------------------------------------------------
// The following event callbacks are punted over to the connection
// for processing

function MarbleData::onEnterPad(%this,%object)
{
   %object.client.onEnterPad();
}

function MarbleData::onLeavePad(%this,%object)
{
   %object.client.onLeavePad();
}

function MarbleData::onStartPenalty(%this,%object)
{
   %object.client.onStartPenalty();
}

function MarbleData::onOutOfBounds(%this,%object)
{
   %object.client.onOutOfBounds();
}

function MarbleData::setCheckpoint(%this,%object,%check)
{
   %object.client.setCheckpoint(%check);
}

//-----------------------------------------------------------------------------
// Marble object
//-----------------------------------------------------------------------------

function Marble::setPowerUp(%this,%item,%reset,%obj)
{
   cancel(%this.powerupRespawn);
   if (!$MPPref::FastPowerups || $Server::ServerType $= "SinglePlayer" || %this.client.isHost()) {
		commandToClient(%this.client, 'SetPowerUp', %item.shapeFile);
		%this.powerUpData = %item;
	}
   if (%item.powerUpId > 5) {
      %this.setPowerUpId(0, false);
      %this.powerUpId = %item.powerUpId;
      %this.powerUpObj = %obj;
      return;
   }

	if (!$MPPref::FastPowerups || $Server::ServerType $= "SinglePlayer" || %this.client.isHost())
		%this.setPowerUpId(%item.powerUpId,%reset);
}

function Marble::getPowerUp(%this)
{
   return %this.powerUpData;
}

function GameConnection::activatePowerup(%this, %powerUpId) {
	%this.player.powerupActive[%powerupId] = true;
   commandToClient(%this, 'ActivatePowerUp', %this.powerUpId);
}

function GameConnection::deactivatePowerup(%this, %powerUpId) {
	%this.player.powerupActive[%powerupId] = false;
   commandToClient(%this, 'DeactivatePowerUp', %this.powerUpId);
}

function GameConnection::mountPlayerImage(%this, %powerUp, %slot) {
	%image = %powerUp.image;
	%ghostImage = %powerUp.ghostImage;
	if (%this.player.megaMarble && %powerUp.megaImage !$= "") {
		%image = %powerUp.megaImage;
		%ghostImage = %powerUp.megaGhostImage;
	}
	//echo("Image is {" SPC %image SPC %ghostImage SPC "}");
	if (isObject(%this.ghost))
		%this.ghost.mountImage(%ghostimage, %slot);

	//HiGuy: These are hard-coded into the engine (shame on you, GG)
//	if (%image $= "HelicopterImage" || %image $= "SuperBounceImage" || %image $= "ShockAbsorberImage")
//		return;
	%this.player.mountImage(%image, %slot);
}

function GameConnection::unmountPlayerImage(%this, %slot) {
	if (isObject(%this.ghost))
		%this.ghost.unmountImage(%slot);
	%this.player.unmountImage(%slot);
}

// Jeff: changed %obj to %this
function Marble::onPowerUpUsed(%this)
{
   if ($Server::ServerType $= "SinglePlayer" || !$MPPref::FastPowerups || %this.client.isHost())
      commandToClient(%this.client, 'SetPowerUp', "");
   %this.playAudio(0, %this.powerUpData.activeAudio);

   %name = %this.powerUpData.getName();
   devecho(%name);

   // Jeff: do stuff for multiplayer
   if ($Server::ServerType $= "Multiplayer") {
      cancel(%this.powerupSchedule[%this.powerUpData.powerUpId]);
      %this.client.activatePowerup(%this.powerUpData.powerUpId);
      %this.powerupSchedule[%this.powerUpData.powerUpId] = %this.client.schedule(%this.getDataBlock().powerUpTime[%this.powerUpData.powerUpId], "deactivatePowerup", %this.powerUpData.powerUpId);

      // Jeff: particles for clients
      if (%name $= "SuperJumpItem")
         %this.client.transferParticles(MarbleSuperJumpEmitter, true);
      else if (%name $= "SuperSpeedItem")
         %this.client.transferParticles(MarbleSuperSpeedEmitter, true);

      if (%this.powerUpData.image !$= "") {
         cancel(%this.client.ghost.unmount[%this.powerUpData.powerUpId]);
         %this.client.mountPlayerImage(%this.powerUpData, %this.powerUpData.imageSlot);
         %this.client.unmount[%this.powerUpData.powerUpId] = %this.client.schedule(%this.getDataBlock().powerUpTime[%this.powerUpData.powerUpId], "unmountPlayerImage", %this.powerUpData.imageSlot);
      }
   }

   if (%this.powerUpId !$= "") {
      %this.powerUpData.onUse(%this.powerUpObj, %this);
      %this.powerUpObj = "";
      %this.powerUpId = "";
   }

   %this.powerUpData = "";
   %this.oldMPPowerupData = "";
   %this.oldMPPowerupObj = "";
}

// Jeff: override Marble::setPosition so we can send the yaw to the client!
package setMarblePos
{
   function Marble::setPosition(%this, %transform, %pitch)
   {
      Parent::setPosition(%this, %transform, %pitch);

      // Jeff: send the information to the client that we have forced
      // set the yaw
      %angle = getAAYaw(getWords(%transform, 3, 6));
      //echo("Setposition:" SPC %transform SPC %picth);
      //echo("Angle:" SPC %angle);
      if (%this.client)
         commandToClient(%this.client, 'setYaw', %angle);
   }
};
activatePackage(setMarblePos);

//-----------------------------------------------------------------------------

function marbleVel()
{
   return $MarbleVelocity;
}

function metricsMarble()
{
   Canvas.pushDialog(FrameOverlayGui, 1000);
   TextOverlayControl.setValue("$MarbleVelocity");
}
