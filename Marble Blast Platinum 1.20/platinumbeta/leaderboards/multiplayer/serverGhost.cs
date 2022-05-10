//------------------------------------------------------------------------------
// Multiplayer Package
// serverGhost.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

// Jeff: default ghost datablock
datablock StaticShapeData(DefaultGhost) {
   shapeFile = "~/data/shapes/balls/ball-superball.dts";
   emap = false;
};
//HiGuy: ghosts now support 3d marble
datablock StaticShapeData(DefaultGhost3dMarble) {
   shapeFile = "~/data/shapes/balls/3dMarble.dts";
   emap = false;
};
//HiGuy: They support mid p as well
datablock StaticShapeData(DefaultGhostMidP) {
   shapeFile = "~/data/shapes/balls/midp.dts";
   emap = false;
};
//HiGuy: They support mid p as well
//datablock StaticShapeData(DefaultGhostPack1) {
//   shapeFile = "~/data/shapes/balls/.pack1/pack1marble.dts";
//   emap = false;
//};


//HiGuy: Mega marble ghost
datablock StaticShapeData(MegaGhost) {
   shapeFile = "~/data/shapes/balls/megamarble.dts";
   emap = false;
};

//HiGuy: For the 3d marble
datablock StaticShapeData(MegaGhost3dMarble) {
   shapeFile = "~/data/shapes/balls/megamarble-3dMarble.dts";
   emap = false;
};
//HiGuy: For mid P
datablock StaticShapeData(MegaGhostMidP) {
   shapeFile = "~/data/shapes/balls/megamarble-midp.dts";
   emap = false;
};
//HiGuy: For mbu
//datablock StaticShapeData(MegaGhostPack1) {
//   shapeFile = "~/data/shapes/balls/.pack1/pack1mega.dts";
//   emap = false;
//};

$MP::GhostDatablock[0] = DefaultGhost;
$MP::GhostDatablock[1] = DefaultGhost3dMarble;
$MP::GhostDatablock[2] = DefaultGhostMidP;
//$MP::GhostDatablock[4] = DefaultGhostPack1;
$MP::MegaGDatablock[0] = MegaGhost;
$MP::MegaGDatablock[1] = MegaGhost3dMarble;
$MP::MegaGDatablock[2] = MegaGhostMidP;
//$MP::MegaGDatablock[4] = MegaGhostPack1;

//HiGuy: Updated marble shape with a collision mesh, but invisible so you don't have to deal with alternate skins
datablock StaticShapeData(GhostMesh) {
   shapeFile = "~/data/shapes/balls/ghost-superball.dts";
   emap = false;
};
datablock StaticShapeData(GhostNoMesh) {
   shapeFile = "~/data/shapes/balls/ghost-meshless-superball.dts";
   emap = false;
};

//HiGuy: Mega marble ghost collision mesh
datablock StaticShapeData(MegaGhostMesh) {
   shapeFile = "~/data/shapes/balls/ghost-megamarble.dts";
   emap = false;
};
datablock StaticShapeData(MegaGhostNoMesh) {
   shapeFile = "~/data/shapes/balls/ghost-meshless-megamarble.dts";
   emap = false;
};


// Jeff: creats the stupid marble...I mean ghost
function GameConnection::createGhost(%this) {
   if ($Server::ServerType !$= "Multiplayer")
      return;
   if (%this.fake) return;
   %skinChoice = %this.skinChoice;
   devecho("Creating ghost for client with address of" SPC %this.getAddress());
   %ghostdb = $MP::MegaGDatablock[getField(%skinChoice, 1)];
   %skin = getField(%skinChoice, 2);
   %ghost = new StaticShape() {
      datablock = %ghostdb;
      scale = %this.scale;
   };
   MissionCleanup.add(%ghost);
   %mesh = new StaticShape() {
      datablock = GhostMesh;
      scale = %this.scale;
   };
   MissionCleanup.add(%mesh);

   // Jeff: set the skin that was selected
   %ghost.setSkinName(%skin);

   // Jeff: attach the ghost to the client
   %this.ghost = %ghost;
   %this.mesh = %mesh;
   %this.ghostRot = "1 0 0 0";

   // Jeff: inform the client to start sending rotation data
   %time = %this.getPing() + 50;
   %this.schedule(%time, "finishCreatingGhost");
}

// Jeff: update the ghosting stuff
function GameConnection::finishCreatingGhost(%this) {
   if (%this.fake) return;
   commandToAll('fixGhost');
   commandToClient(%this, 'StartRotation');
}

// Jeff: update the ghost positions for the clients to see each other
//       updated about every 5 milliseconds for fast execution
//       as this is a core function obviously
function MPUpdateGhostPositions() {
   cancel($MP::Schedule::GhostPosition);

   if (!$Server::Hosting || $Server::ServerType $= "SinglePlayer")
      return;

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (!isObject(%client.player))
         continue;
      %position = %client.player.getPosition();
      %client.assistRotation();
      %rot = %client.rotUpdate;
      if (isObject(%client.ghost))
         %client.ghost.setTransform(%position SPC %rot);
      if (isObject(%client.mesh))
         %client.mesh.setTransform(%position SPC "1 0 0 0");
   }
   $MP::Schedule::GhostPosition = schedule($MP::GhostUpdateTime, 0, MPUpdateGhostPositions);
}

//------------------------------------------------------------------------------

// HiGuy: Velocity Loop
// Jeff:  This also updates the server tweening to the client
function velocityLoop() {
   cancel($MP::Schedule::VelocityLoop);

   if (!$Server::Hosting)
      return;

   if ($MP::Delta::Velocity $= "")
      $MP::Delta::Velocity = getRealTime();

   %delta = getRealTime() - $MP::Delta::Velocity;
   $MP::Delta::Velocity = getRealTime();

   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      %player = %client.player;

      if (!isObject(%player))
         continue;

      %position = %player.getEyeTransform();
      //HiGuy: Get position difference
      if (%client.lastPos $= "")
         %client.lastPos = %position;

      if (%client.lastPos $= %position) {
         %client.speed = 0;
         continue;
      }

      %posDiff = VectorSub(%position, %client.lastPos);
      %client.lastPos = %position;

      //HiGuy: Normalize
      %posDiff = VectorScale(%posDiff, 1000 / %delta);
      %speed = VectorLen(%posDiff);


      %client.speed = %speed;
      %client.posDiff = %posDiff;

      %ground = ContainerBoxEmpty($TypeMasks::InteriorObjectType, %position, mCeil(getWord(%player.getWorldBox(), 3) - getWord(%player.getWorldBox(), 0)));
//      echo(%ground);
      if (!%ground)
         %player.lastTouch = %position;

      //HiGuy: And send it off!
      commandToAll('tweenUpdate', %client.index, %posDiff SPC %client.ghostRot, %speed, !!%client.player.megaMarble);
   }

   $MP::Schedule::VelocityLoop = schedule($MP::ServerTween, 0, "velocityLoop");
}

function GameConnection::updateGhostDatablock(%this) {
   if (%this.fake) return;
   %skinChoice = %this.skinChoice;
   //echo("Skin choice is" SPC %skinChoice);
   if (%this.player.megaMarble) {
      %skin = getField(%skinChoice, 2);
      %playerdb = $MP::MegaDatablock[getField(%skinChoice, 1)];
      %this.player.setDatablock(%playerdb);
      %this.player.setSkinName(%skin);

      if (isObject(%this.ghost)) {
         %ghostdb = $MP::MegaGDatablock[getField(%skinChoice, 1)];
         %this.ghost.setDataBlock(%ghostdb);
         %this.ghost.setSkinName(%skin);
      }
      if (isObject(%this.mesh))
         %this.mesh.setDataBlock(%this.clipping ? MegaGhostNoMesh : MegaGhostMesh);
   } else {
      if (($LB::LoggedIn && $LB::Username !$= "") || $Server::Dedicated) {
         %skin = getField(%skinChoice, 2);
         %playerdb = $LB::MarbleDatablock[getField(%skinChoice, 1)];
         %this.player.setDatablock(%playerdb);
         %this.player.setSkinName(%skin);

         if (isObject(%this.ghost)) {
            %ghostdb = $MP::GhostDatablock[getField(%skinChoice, 1)];
            %this.ghost.setDataBlock(%ghostdb);
            %this.ghost.setSkinName(%skin);
         }
      } else {
         //HiGuy: Copied custom marble code from game.cs *by jeff*
         %marbleSelection = MarbleSelectGui.getSelection();
         %shape = getField(%marbleSelection, 0);
         %skin = getField(%marbleSelection, 1);

         %db = "DefaultMarble";
         if ((%skin $= "base" && filePath(%shape) !$= "ball-superball") || filePath(%shape) !$= $usermods @ "/data/shapes/balls/")
            %db = "CustomMarble";
         %db.shapeFile = %shape;

         %this.player.setDatablock(%db);
         %this.player.setSkinName(%skin);
      }
      if (isObject(%this.mesh))
         %this.mesh.setDataBlock(%this.clipping ? GhostNoMesh : GhostMesh);
   }
}

function serverCmdMegaMarble(%client, %mega) {
	if ($MPPref::FastPowerups) {
		//HiGuy: Oh well, just listen to them
		%client.player.megaMarble = %mega;
		%client.updateGhostDatablock();
	}
}
