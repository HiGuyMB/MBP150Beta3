//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Easter Egg Codes by GarageGames, Alex Swanson and Spy47
// Random PowerUp Code by Spy47
// NoRespawn GravMod code from PQ
// Blast and Mega Marble PowerUp code by Jeff/Higuy
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// PowerUp base class
//-----------------------------------------------------------------------------

function PowerUp::onPickup(%this,%obj,%user,%amount)
{
   // Dont' pickup the power up if it's the same
   // one we already have.
   if (%user.powerUpData == %this)
      return false;

   if (%user.client.disablePowerUp[%this.powerUpId])
      return;

   // Grab it...
   %user.client.play2d(%this.pickupAudio);
   if (%this.powerUpId)
   {
      if(%obj.showHelpOnPickup)
         addHelpLine("Press <func:bind mouseFire> to use the " @ %this.useName @ "!", false);

      %user.setPowerUp(%this, false, %obj);
   }

   Parent::onPickup(%this, %obj, %user, %amount);
   if (%obj.nukesweeper)
      %obj.trigger.reset();
   return true;
}

//-----------------------------------------------------------------------------

datablock AudioProfile(doSuperJumpSfx)
{
   filename    = "~/data/sound/doSuperJump.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(PuSuperJumpVoiceSfx)
{
   filename    = "~/data/sound/puSuperJumpVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(SuperJumpItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 1;

   activeAudio = DoSuperJumpSfx;
   pickupAudio = PuSuperJumpVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/items/superjump.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   pickupName = "a Jump Boost PowerUp!";
   useName = "Jump Boost PowerUp";
   maxInventory = 1;
};


//-----------------------------------------------------------------------------

datablock AudioProfile(doSuperBounceSfx)
{
   filename    = "~/data/sound/doSuperBounce.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(PuSuperBounceVoiceSfx)
{
   filename    = "~/data/sound/puSuperBounceVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(SuperBounceItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 3;

   activeAudio = DoSuperBounceSfx;
   pickupAudio = PuSuperBounceVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/items/superbounce.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;

   // Dynamic properties defined by the scripts
   pickupName = "a Marble Recoil PowerUp!";
   useName = "Marble Recoil PowerUp";
   maxInventory = 1;

      image = SuperBounceImage;
      imageSlot = 0;
};

datablock AudioProfile(SuperBounceLoopSfx)
{
   filename    = "~/data/sound/forcefield.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock ShapeBaseImageData(SuperBounceImage)
{
   // Basic Item properties
   shapeFile = "~/data/shapes/images/glow_bounce.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0";
   stateName[0] = "Blah";
   stateSound[0] = SuperBounceLoopSfx;
};


//-----------------------------------------------------------------------------

datablock AudioProfile(DoSuperSpeedSfx)
{
   filename    = "~/data/sound/doSuperSpeed.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(PuSuperSpeedVoiceSfx)
{
   filename    = "~/data/sound/puSuperSpeedVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(SuperSpeedItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 2;

   activeAudio = DoSuperSpeedSfx;
   pickupAudio = PuSuperSpeedVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/items/superspeed.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   pickupName = "a Speed Booster PowerUp!";
   useName = "Speed Booster PowerUp";
   maxInventory = 1;
};


//-----------------------------------------------------------------------------

datablock AudioProfile(doShockAbsorberSfx)
{
   filename    = "~/data/sound/doShockAbsorber.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(PuShockAbsorberVoiceSfx)
{
   filename    = "~/data/sound/puShockAbsorberVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(ShockAbsorberItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 4;

   activeAudio = DoShockAbsorberSfx;
   pickupAudio = PuShockAbsorberVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/items/shockabsorber.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;

   // Dynamic properties defined by the scripts
   pickupName = "an Anti-Recoil PowerUp!";
   useName = "Anti-Recoil PowerUp";
   maxInventory = 1;
   emap = false;

      image = ShockAbsorberImage;
      imageSlot = 1;
};

datablock AudioProfile(ShockLoopSfx)
{
   filename    = "~/data/sound/superbounceactive.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock ShapeBaseImageData(ShockAbsorberImage)
{
   // Basic Item properties
   shapeFile = "~/data/shapes/images/glow_bounce.dts";
   emap = true;

   // Specify mount point & offset for 3rd person, and eye offset
   // for first person rendering.
   mountPoint = 0;
   offset = "0 0 0";
   stateName[0] = "Blah";
   stateSound[0] = ShockLoopSfx;
};


//-----------------------------------------------------------------------------

datablock AudioProfile(doHelicopterSfx)
{
   filename    = "~/data/sound/doHelicopter.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(PuGyrocopterVoiceSfx)
{
   filename    = "~/data/sound/puGyrocopterVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(HelicopterItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 5;

   activeAudio = DoHelicopterSfx;
   pickupAudio = PuGyrocopterVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/images/helicopter.dts";
//   shapeFile = "~/data/shapes/items/megaHelicopter.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;

   // Dynamic properties defined by the scripts
   pickupName = "a Helicopter PowerUp!";
   useName = "Helicopter PowerUp";
   maxInventory = 1;

      image = HelicopterImage;
      ghostImage = GhostHelicopterImage;
      megaImage = MegaHelicopterImage;
      megaGhostImage = GhostMegaHelicopterImage;
      imageSlot = 2;
};

datablock AudioProfile(HelicopterLoopSfx)
{
   filename    = "~/data/sound/Use_Gyrocopter.wav";
   description = AudioClosestLooping3d;
   preload = true;
};

datablock ShapeBaseImageData(HelicopterImage)
{
   // Basic Item properties
   shapeFile = "~/data/shapes/images/helicopter.dts";
//   shapeFile = "~/data/shapes/items/megaHelicopter.dts";
   emap = true;
   mountPoint = 0;
   offset = "0 0 0";
   stateName[0]                     = "Rotate";
   stateSequence[0]                 = "rotate";
//   stateName[0]                     = "Ambient";
//   stateSequence[0]                 = "ambient";
   stateSound[0] = $Server::ServerType $= "MultiPlayer" ? "" : HelicopterLoopSfx;
   ignoreMountRotation = true;
};

datablock ShapeBaseImageData(MegaHelicopterImage : HelicopterImage)
{
   shapeFile = "~/data/shapes/items/megaHelicopter.dts";
};

datablock ShapeBaseImageData(GhostHelicopterImage : HelicopterImage)
{
   stateSound[0] = $Server::ServerType $= "MultiPlayer" ? HelicopterLoopSfx : "";
};

datablock ShapeBaseImageData(GhostMegaHelicopterImage : GhostHelicopterImage)
{
   shapeFile = "~/data/shapes/items/megaHelicopter.dts";
};
//-----------------------------------------------------------------------------
// Special non-inventory power ups
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

datablock AudioProfile(PuRandomVoiceSfx)
{
   filename    = "~/data/sound/puRandomVoice.wav";
   description = AudioDefault3D;
   preload = true;
};

datablock ItemData(RandomPowerUpItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";

   // Basic Item properties
   pickupAudio = PuTimeTravelVoiceSfx;
   shapeFile = "~/data/shapes/items/random.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   noRespawn = true;
   maxInventory = 1;
};

function RandomPowerUpItem::getPickupName(%this, %obj)
{
   if(%obj.timeBonus !$= "")
      %time = %obj.timeBonus / 1000;
   else
      %time = $Game::TimeTravelBonus / 1000;

   return "a " @ %time @ " second Time Modifier!";
}

function RandomPowerUpItem::OnPickup(%this,%obj,%user,%amount)
{
   if ($playingDemo) {
      %pupIdx = $Game::RandomPowerup[$Game::RandomPowerupOn];
      $Game::RandomPowerupOn ++;
   } else {
      %pupIdx = getRandom(1,6);
   }
   switch (%pupIdx)
   {
      case 1:
         %pup = SuperJumpItem;
      case 2:
         %pup = SuperSpeedItem;
      case 3:
         %pup = HelicopterItem;
      case 4:
         TimeTravelItem::onPickup(%this,%obj,%user,%amount);
         return;
      case 5:
         %pup = SuperBounceItem;
      case 6:
         %pup = ShockAbsorberItem;
   }
   if ($doRecordDemo) {
      $Game::RandomPowerup[$Game::RandomPowerups] = %pupIdx;
      $Game::RandomPowerups ++;
   }
   return PowerUp::OnPickup(%pup.getId(),%obj,%user,%amount);
}

//-----------------------------------------------------------------------------

datablock AudioProfile(PuTimeTravelVoiceSfx)
{
   filename    = "~/data/sound/puTimeTravelVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(TimeTravelItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";

   // Basic Item properties
   pickupAudio = PuTimeTravelVoiceSfx;
   shapeFile = "~/data/shapes/items/timetravel.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   noRespawn = true;
   maxInventory = 1;
};

function TimeTravelItem::getPickupName(%this, %obj)
{
   if(%obj.timeBonus !$= "")
      %time = %obj.timeBonus / 1000;
   else
      %time = $Game::TimeTravelBonus / 1000;

   return "a " @ %time @ " second Time Modifier!";
}

function TimeTravelItem::onPickup(%this,%obj,%user,%amount)
{
   Parent::onPickup(%this, %obj, %user, %amount);
   if(%obj.timeBonus !$= "")
      %user.client.incBonusTime(%obj.timeBonus);
   else
      %user.client.incBonusTime($Game::TimeTravelBonus);
}


//-----------------------------------------------------------------------------

datablock AudioProfile(PuAntiGravityVoiceSfx)
{
   filename    = "~/data/sound/gravitychange.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(AntiGravityItem)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";

   pickupAudio = PuAntiGravityVoiceSfx;
   pickupName = "a Gravity Defier!";

   // Basic Item properties
   shapeFile = "~/data/shapes/items/antiGravity.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   maxInventory = 1;
};

function AntiGravityItem::onAdd(%this, %obj)
{
   %obj.playThread(0,"Ambient");
}

function AntiGravityItem::onPickup(%this,%obj,%user,%amount)
{
   %rotation = getWords(%obj.getTransform(),3);
   %ortho = vectorOrthoBasis(%rotation);
   if (%user.client.gravityDir !$= %ortho) {
      Parent::onPickup(%this, %obj, %user, %amount);
      %user.client.setGravityDir(%ortho, false, %rotation);
      //echo("Checkpint reached, saving gravity: " @ %ortho);
   }
}

//-----------------------------------------------------------------------------

datablock AudioProfile(EasterEggSfx)
{
   filename    = "~/data/sound/easter.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(EasterEggFoundSfx)
{
   filename    = "~/data/sound/easterfound.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock ItemData(EasterEgg)
{
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";

   shapeFile = "~/data/shapes/items/easteregg.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   // Dynamic properties defined by the scripts
   noRespawn = true;
   maxInventory = 1;
   noPickupMessage = true; // Don't display a message here. We're doing it later.
};

function EasterEgg::getPickupName(%this, %obj)
{
   return "an Easter Egg!";
}

function EasterEgg::onPickup(%this,%obj,%user,%amount) {
   Parent::onPickup(%this, %obj, %user, %amount);

   if ($LB::LoggedIn && $LB::username !$= "") {    // Jeff: check to see if we have the easter egg
      %message = '\c0You found an Easter Egg!';
      %strippedMis = alphaNum($Server::MissionFile);
      %found = false;
      for (%i = 0; %i < LBEasterEggArray.getSize(); %i ++) {
         if (LBEasterEggArray.getEntryByIndex(%i) $= %strippedMis) {
            %message = '\c0You already found this Easter Egg.';
            %found = true;
            break;
         }
      }
      %user.client.play2d(EasterEgg @ (%found ? "Found" : "") @ Sfx);
      %securityCode = 47392;
      sendEasterEggToLB(%securityCode);
   } else {
      if ($pref::easterEgg[$Server::MissionFile]) { // Jeff: we have it!
         %message = '\c0You already found this Easter Egg.';
         %user.client.play2d(EasterEggFoundSfx);
      } else {
         $pref::easterEgg[$Server::MissionFile] = 1; // Jeff: save it
         %message = '\c0You found an Easter Egg!';
         %user.client.play2d(EasterEggSfx);
      }
   }
   messageClient(%user.client,'',%message);
   savePrefs(true);
}

//-----------------------------------------------------------------------------

datablock ItemData(NoRespawnAntiGravityItem)
{
   // Mission editor category
   superCategory = "Gameplay";
   category = "PowerUps";
   className = "PowerUp";

   pickupAudio = PuAntiGravityVoiceSfx;
   pickupName = "a Gravity Defier!";

   // Basic Item properties
   shapeFile = "~/data/shapes/items/antiGravity.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
   emap = false;

   noPickup = true;

   // Jeff: haaaacked, allow for MP support
   permanent = true;
   density = 9001; // Jeff: actually transfers, used for item collision
};

function NoRespawnAntiGravityItem::onAdd(%this, %obj)
{
   %obj.playThread(0,"Ambient");
}

function NoRespawnAntiGravityItem::onPickup(%this,%obj,%user,%amount)
{
   %rotation = getWords(%obj.getTransform(),3);
   %ortho = vectorOrthoBasis(%rotation);
   if (%user.client.gravityDir !$= %ortho) {
      Parent::onPickup(%this, %obj, %user, %amount);
      %user.client.setGravityDir(%ortho, false, %rotation);
   }
}

//-----------------------------------------------------------------------------

datablock AudioProfile(PuBlastVoiceSfx) {
   filename    = "~/data/sound/puBlastVoice.wav";
   description = AudioDefault3d;
   preload     = true;
};

datablock ItemData(BlastItem) {
   className = "PowerUp";
   category = "PowerUps";

   pickupAudio = PuBlastVoiceSfx;
   shapeFile = "~/data/shapes/items/blast.dts";
   emap = false;
   pickupName = "a Blast PowerUp!";
};

function BlastItem::onAdd(%this, %obj)
{
   %obj.playThread(0, "ambient");
}

function BlastItem::onPickup(%this, %obj, %user, %amount) {
   if (%user.client.disableBlast)
      return;
   Parent::onPickup(%this, %obj, %user, %amount);
   %user.client.setBlastValue(1);
   %user.client.setSpecialBlast(true);
}

//-----------------------------------------------------------------------------

datablock AudioProfile(doMegaMarbleSfx) {
   filename    = "~/data/sound/doSuperJump.wav";
//   filename    = "~/data/sound/doMegaMarble.wav";
   description = AudioDefault3d;
   preload     = true;
};

datablock AudioProfile(PuMegaMarbleVoiceSfx) {
   filename    = "~/data/sound/puMegaMarbleVoice.wav";
   description = AudioDefault3d;
   preload     = true;
};

datablock ItemData(MegaMarbleItem) {
   // Mission editor category
   category = "PowerUps";
   className = "PowerUp";
   powerUpId = 6;

   activeAudio = DoMegaMarbleSfx;
   pickupAudio = PuMegaMarbleVoiceSfx;

   // Basic Item properties
   shapeFile = "~/data/shapes/items/MegaMarble.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;

   // Dynamic properties defined by the scripts
   pickupName = "a Mega Marble PowerUp!";
   useName = "Mega Marble PowerUp";
   maxInventory = 1;

    defaultTimeout = 10000;
};

function MegaMarbleItem::onAdd(%this, %obj)
{
   %obj.playThread(0, "ambient");
}

function MegaMarbleItem::onUse(%this, %obj, %user) {
   if (!%user.megaMarble) {
      %user.megaMarble = true;
      %user.client.updateGhostDatablock();
      %ray = ContainerRayCast(%user.getPosition(), VectorSub(%user.getPosition(), VectorMult("-1 -1 -1", getWords(%user.client.gravityDir, 6, 8))), $TypeMasks::InteriorObjectType, %user);
      if (isObject(getWord(%ray, 0)))
         %user.client.schedule(10, gravityImpulse, "0 0 -1", "6 6 6");

      if (%user.powerupActive[HelicopterItem.powerUpId]) {
			%user.client.unmountPlayerImage(HelicopterItem.imageSlot);
			%user.client.mountPlayerImage(HelicopterItem, HelicopterItem.imageSlot);
      }
   }
   %timeout = (%obj.timeout > 0 ? %obj.timeout : %this.defaultTimeout);
   cancel(%user.megaSchedule);
   %user.megaSchedule = %this.schedule(%timeout, "onUnuse", %obj, %user);
}

function MegaMarbleItem::onUnuse(%this, %obj, %user) {
   %user.megaMarble = false;
   %user.client.updateGhostDatablock();
   %user.client.schedule(10, gravityImpulse, "0 0 1", "-2 -2 -2");

	if (%user.powerupActive[HelicopterItem.powerUpId]) {
		%user.client.unmountPlayerImage(HelicopterItem.imageSlot);
		%user.client.mountPlayerImage(HelicopterItem, HelicopterItem.imageSlot);
	}

   cancel(%user.megaSchedule);
}
