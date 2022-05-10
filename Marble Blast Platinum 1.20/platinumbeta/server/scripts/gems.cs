//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Gem base class
//-----------------------------------------------------------------------------

datablock AudioProfile(GotGemSfx)
{
   filename    = "~/data/sound/gotDiamond.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(OpponentGemSfx)
{
   filename    = "~/data/sound/opponentDiamond.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(GotAllGemsSfx)
{
   filename    = "~/data/sound/gotalldiamonds.wav";
   description = AudioDefault3d;
   preload = true;
};


//-----------------------------------------------------------------------------

$GemSkinColors[0] = "base";
$GemSkinColors[1] = "base";
$GemSkinColors[2] = "blue";
$GemSkinColors[3] = "red";
$GemSkinColors[4] = "yellow";
$GemSkinColors[5] = "purple";
$GemSkinColors[6] = "orange";
$GemSkinColors[7] = "green";
$GemSkinColors[8] = "turquoise";
$GemSkinColors[9] = "black";
$GemSkinColors[10] = "platinum";

function Gem::onAdd(%this,%obj)
{
   if (%this.skin !$= "")
      %obj.setSkinName(%this.skin);
   else {
      // Random skin if none assigned
      %obj.setSkinName($GemSkinColors[getRandom(10)]);
   }
}

function Gem::onPickup(%this,%obj,%user,%amount)
{
   Parent::onPickup(%this,%obj,%user,%amount);

   // Jeff: hunt mode code
   if ($Game::isHunt) {
      if (!%user.client.disableGems[%this.huntExtraValue + 1]) {
         %user.client.gemCount += %this.huntExtraValue;
         %user.client.gemsFound[%this.huntExtraValue + 1] ++;
      } else
         %user.client.gemsFound[1] ++;

      unspawnGem(%obj);
   } else {
      // Spy47 : Which checkpoint did you pick up this gem?
      //echo("DEBUG: CurCheckpointNum:" SPC %user.client.CurCheckpointNum);
      //echo("DEBUG: %user:" SPC %user);
      %obj.pickUp = %user.client;
      %obj.pickUpCheckpoint = %user.client.curCheckpointNum;
      //echo("DEBUG: %obj.pickUpCheckpoint:" SPC %obj.pickUpCheckpoint);
      //echo("DEBUG: %obj.pickUp:" SPC %obj.pickUp);
   }
   %user.client.onFoundGem(%amount);
   if (%obj.nukesweeper)
      %obj.trigger.reset();
   return true;
}

function Gem::saveState(%this,%obj,%state)
{
   %state.object[%obj.getId()] = %obj.isHidden();
}

function Gem::restoreState(%this,%obj,%state)
{
   %obj.hide(%state.object[%obj.getId()]);
}

//-----------------------------------------------------------------------------

datablock ItemData(GemItem)
{
   // Mission editor category
   category = "Gems";
   className = "Gem";

   // Basic Item properties
   shapeFile = "~/data/shapes/items/gem.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;

   // Dynamic properties defined by the scripts
   pickupName = "a diamond!";
   maxInventory = 1;
   noRespawn = true;
   gemType = 1;
   noPickupMessage = true;
   pickUpCheckpoint = "0";
   huntPointValue = 0;
};

datablock ItemData(GemItemBlue: GemItem)
{
   skin = "blue";
   huntExtraValue = 4; //1 less because you get 1 point for collecting it
};

datablock ItemData(GemItemRed: GemItem)
{
   skin = "red";
   huntExtraValue = 0; //1 pt
};

datablock ItemData(GemItemYellow: GemItem)
{
   skin = "yellow";
   huntExtraValue = 1; //2 pts
};

datablock ItemData(GemItemPurple: GemItem)
{
   skin = "purple";
};

datablock ItemData(GemItemGreen: GemItem)
{
   skin = "Green";
};

datablock ItemData(GemItemTurquoise: GemItem)
{
   skin = "Turquoise";
};

datablock ItemData(GemItemOrange: GemItem)
{
   skin = "orange";
};

datablock ItemData(GemItemBlack: GemItem)
{
   skin = "black";
};

datablock ItemData(GemItemPlatinum: GemItem)
{
   skin = "platinum";
   //huntExtraValue = 9; //10 pts
};
