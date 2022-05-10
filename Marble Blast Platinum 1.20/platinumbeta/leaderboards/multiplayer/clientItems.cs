//------------------------------------------------------------------------------
// Multiplayer Package
// clientItems.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

// Jeff: make mega marbles have the correct skinning for the world
// objects
function reskinMegaMarbles()
{
   %dts[0] = $usermods @ "/data/shapes/balls/ball-superball.dts";
   %dts[1] = $usermods @ "/data/shapes/balls/3dMarble.dts";
   %dts[2] = $usermods @ "/data/shapes/balls/midp.dts";
   %dts[3] = $usermods @ "/data/shapes/balls/garageGames.dts";
   %select = MPMarbleSelectionDlg.getSelection();
   %marble = fileBase(%dts[getField(%select, 1)]);
   %skin   = getField(%select, 2);

   %skin = (%skin $= "") ? "base" : %skin;
   //echo(%skin);
   %datablock = findMarbleDataBlock(%marble);

   // Jeff: itterate through the connection to get each object, we will
   // then check for a mega marble through a couple of checks.
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "Item")
      {
         %file = %obj.getDataBlock().shapeFile;
         if (strStr(%file, "megamarble") != -1)
         {
            // Jeff: now we actually preform the reskinning and changing the
            // shape itself.
            if (isObject(%datablock))
               %obj.setDataBlock(%datablock);
            %obj.setSkinName(%skin);
            //echo(%obj.getID());
            //$MM = %obj.getID();
         }
      }
   }
}

// Jeff: this finds the correct datablock to display
function findMarbleDataBlock(%name)
{
   %name = (%name $= "ball-superball") ? "megamarble" : "megamarble-" @ %name;
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj  = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "ItemData")
      {
         %file = %obj.shapeFile;
         if (strStr(%file, "megamarble") != -1)
         {
            if (fileBase(%file) $= %name)
            {
               //devecho("Found Mega Marble Datablock!");
               return %obj.getID();
            }
         }
      }
   }
   return -1;
}

//-----------------------------------------------------------------------------
// Jeff: below this line is for handicaps
//-----------------------------------------------------------------------------

function clientCmdUpdateHandicapItems() {
   updateClientHandiCapItems();
   //HiGuy: Just run this a fuckton of times. It takes 1.001 ms to run on my
   // PoC laptop, you can stand to run it quite a bit.
   schedule(100, 0, updateClientHandiCapItems);

   //HiGuy: Because we can never be sure of the timing
   schedule(ServerConnection.getPing(), 0, updateClientHandiCapItems);
   schedule(ServerConnection.getPing() / 2, 0, updateClientHandiCapItems);
   schedule(ServerConnection.getPing() * 1.1, 0, updateClientHandiCapItems);
}

// Jeff: hide these items depending on our preferences
function updateClientHandiCapItems()
{
   if ($Server::ServerType !$= "MultiPlayer")
      return;

   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = ServerConnection.getObject(%i);

      switch$ (%obj.getClassName()) {
      case "ItemData":
         if (!$Server::Hosting || $Server::_Dedicated) {
         	%base = fileBase(%obj.shapeFile);
            switch$ (%base) {
            case "easterEgg":     %obj.setName("EasterEgg");
            case "superjump":     %obj.setName("SuperJumpItem");
            case "superbounce":   %obj.setName("SuperBounceItem");
            case "superspeed":    %obj.setName("SuperSpeedItem");
            case "shockabsorber": %obj.setName("ShockAbsorberItem");
            case "helicopter":    %obj.setName("HelicopterItem");
            case "MegaMarble":    %obj.setName("MegaMarbleItem");
            case "random":        %obj.setName("RandomPowerUpItem");
            case "timetravel":    %obj.setName("TimeTravelItem");
            case "antiGravity":   %obj.setName("AntiGravityItem");
            case "Blast":         %obj.setName("BlastItem");
            }
         }
      case "AudioProfile":
         if (!$Server::Hosting || $Server::_Dedicated) {
         	%base = fileBase(%obj.fileName);
            switch$ (%base) {
            case "easter":              %obj.setName("EasterEggSfx");
            case "easterfound":         %obj.setName("EasterEggFoundSfx");
            case "doSuperJump":         %obj.setName("doSuperJumpSfx");
            case "puSuperJumpVoice":    %obj.setName("PuSuperJumpVoiceSfx");
            case "doSuperBounce":       %obj.setName("doSuperBounceSfx");
            case "puSuperBounceVoice":  %obj.setName("PuSuperBounceVoiceSfx");
            case "forcefield":          %obj.setName("SuperBounceLoopSfx");
            case "doSuperSpeed":        %obj.setName("DoSuperSpeedSfx");
            case "puSuperSpeedVoice":   %obj.setName("PuSuperSpeedVoiceSfx");
            case "doShockAbsorber":     %obj.setName("doShockAbsorberSfx");
            case "puShockAbsorberVoice":%obj.setName("PuShockAbsorberVoiceSfx");
            case "superbounceactive":   %obj.setName("ShockLoopSfx");
            case "doHelicopter":        %obj.setName("doHelicopterSfx");
            case "puGyrocopterVoice":   %obj.setName("PuGyrocopterVoiceSfx");
            case "Use_Gyrocopter":      %obj.setName("HelicopterLoopSfx");
            case "puRandomVoice":       %obj.setName("PuRandomVoiceSfx");
            case "puTimeTravelVoice":   %obj.setName("PuTimeTravelVoiceSfx");
            case "gravitychange":       %obj.setName("PuAntiGravityVoiceSfx");
            case "puBlastVoice":        %obj.setName("PuBlastVoiceSfx");
            }
         }
      case "ShapeBaseImageData":
         if (!$Server::Hosting || $Server::_Dedicated) {
         	%base = fileBase(%obj.shapeFile);
            switch$ (%base) {
            case "glow_bounce": %obj.setName("ShockAbsorberImage");
            case "helicopter": %obj.setName("HelicopterImage");
            }
			}
      case "MarbleData":
         if (!$Server::Hosting || $Server::_Dedicated) {
         	%base = fileBase(%obj.shapeFile);
            switch$ (%base) {
            case "ball-superball":      if (!isObject(DefaultMarble)) %obj.setName("DefaultMarble");
            case "3dMarble":            %obj.setName("ThreeDMarble");
            case "midp":                %obj.setName("MidPMarble");
            case "garageGames":         %obj.setName("ggMarble");
            case "sm1":                 %obj.setName("sm1Marble");
            case "sm2":                 %obj.setName("sm2Marble");
            case "sm3":                 %obj.setName("sm3Marble");
            case "bm1":                 %obj.setName("bm1Marble");
            case "bm2":                 %obj.setName("bm2Marble");
            case "bm3":                 %obj.setName("bm3Marble");
            case "megamarble":          %obj.setName("MegaMarble");
            case "megamarble-3dMarble": %obj.setName("MegaMarble3dMarble");
            case "megamarble-midp":     %obj.setName("MegaMarbleMidP");
            }
			}
      case "StaticShape":
      	%base = fileBase(%obj.getDataBlock().shapeFile);
         switch$ (%base) {
         case "gemlight":
            if ($MPPref::DisableRadar)
               %obj.hide(true);
         case "ball-superball" or "3dMarble" or "midp" or "megamarble" or "megamarble-3dMarble" or "megamarble-midp":
            if ($MPPref::DisableMarbles) {
               //echo("probably should hide" SPC %obj);
               //echo(%obj.getClassName() SPC %obj.getDataBlock() SPC %obj.getDataBlock().getName());
               %obj.hide(true);
            }
         }
      case "Item":
      	%base = fileBase(%obj.getDataBlock().shapeFile);
         switch$ (%base) {
         case "gem":
            if (!$Server::Hosting || $Server::_Dedicated) {
               %id = "";
               switch$ (%obj.getSkinName()) {
               case "base":      %id = "";
               case "blue":      %id = "Blue";
               case "red":       %id = "Red";
               case "yellow":    %id = "Yellow";
               case "purple":    %id = "Purple";
               case "orange":    %id = "Orange";
               case "green":     %id = "Green";
               case "turquoise": %id = "Turquoise";
               case "black":     %id = "Black";
               case "platinum":  %id = "Platinum";
               }
               if (%obj.getDataBlock().getName() $= "" && !isObject("GemItem" @ %id))
                  %obj.getDataBlock().setName("GemItem" @ %id);
            }
         // Jeff: blast
         case "easterEgg":
            if ($MPPref::DisableBlast)
               %obj.hide(true);

         // Jeff: super jump
         case "superjump":
            if ($MPPref::DisablePowerup1)
               %obj.hide(true);

         // Jeff: super speed
         case "superspeed":
            if ($MPPref::DisablePowerup2)
               %obj.hide(true);

         // Jeff: helicopter
         case "helicopter":
            if ($MPPref::DisablePowerup5)
               %obj.hide(true);

         //HiGuy: Blast
         case "Blast":
            if ($MPPref::DisableBlast)
               %obj.hide(true);

         // Jeff: mega marble
         case "megamarble" or "megamarble-midp" or "megamarble-3dmarble":
            if ($MPPref::DisablePowerup6)
               %obj.hide(true);
         }
         if (!$Server::Hosting || $Server::_Dedicated) {
            MissionGroup.add(%obj);
         }
      case "StaticShape":
         if (!$Server::Hosting || $Server::_Dedicated) {
            MissionGroup.add(%obj);
         }
      }
   }
   cancel($MP::Schedule::HandicapItems);
//   $MP::Schedule::HandicapItems = schedule(1000 / $fps::real, 0, updateClientHandiCapItems);
}

//-----------------------------------------------------------------------------
// Item Collision and Other Goodies
//-----------------------------------------------------------------------------

// Jeff: update collision with items
function updateItemCollision() {
   cancel($MP::Schedule::ItemCollision);

   if ($fast || !isObject($MP::MyMarble) && MPGetMyMarble() == -1 || $Server::ServerType $= "SinglePlayer")
      return;

   %mesh    = $MP::MeshIndex[$MP::MeshLookup[$MP::MyScale]];
   %pos     = $MP::MyMarble.getWorldBoxCenter();
   %marbBox = $MP::MyMarble.getWorldBox();
//   if (!isObject(%mesh))
//      return;
//
//   if (isObject(%mesh)) {
//      %box = %mesh.getWorldBox();
//      %width  = getWord(%box, 3) - getWord(%box, 0);
//      %depth  = getWord(%box, 4) - getWord(%box, 1);
//      %height = getWord(%box, 5) - getWord(%box, 2);
//
//      %width  /= 2;
//      %depth  /= 2;
//      %height /= 2;
//
//      %marbBox = VectorSub(%pos, %width SPC %depth SPC %height) SPC VectorAdd(%pos, %width SPC %depth SPC %height);
//   }

   %count = ItemArray.getSize();
   %rebuild = ServerConnection.getCount() != $LastSCCount;
   for (%i = 0; %i < %count; %i ++) {
      %obj = ItemArray.getEntryByIndex(%i);
      %box = getField(%obj, 1);
      %obj = nameToID(getField(%obj, 0));
      if (%obj == -1)
         continue;

//      if (VectorDist(%pos, %obj.getPosition()) < 10)
         //echo(VectorDist(%pos, %obj.getPosition()) TAB %box TAB %marbBox TAB boxInterceptsBox(%box, %marbBox));
      if (VectorDist(%pos, %obj.getPosition()) < 10 && boxInterceptsBox(%box, %marbBox)) {
         // Jeff: only hide not permantent items
         if (%obj.getDatablock().density != 9001) {
            echo("hiding object:" SPC %obj);

            // Jeff: If the server is a different OS than us, we have to accept
            // this collision and call oncollision if the object isn't hidden
            %acceptAuto = serverHasDiffPlatform();
            commandToServer('ItemCollision', %obj.getPosition(), %obj, %acceptAuto);
            %obj.hide(true);
         }
         %obj.onClientCollision($MP::MyMarble);

         %rebuild = true;
      }
   }

   // Jeff: reset the list to make execution faster
   // hidden items do not stay within the serverConnection
   // object
   if (%rebuild)
      buildItemList();
   $MP::Schedule::ItemCollision = schedule(15, 0, "updateItemCollision");
}

function Item::onClientCollision(%this, %marble) {
   echo("Collision");
   switch$ (%this.getDataBlock().getName()) {
   case "AntiGravityItem":
      //HiGuy: It's a gravity modifier
      %rotation = getWords(%this.getTransform(),3);
      %ortho = vectorOrthoBasis(%rotation);
      if ($Game::LastGravityDir !$= %ortho) {
         clientCmdSetGravityDir(%ortho, false, %rotation);
      }
   case "SuperJumpItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(1, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuSuperJumpVoiceSfx);
      }
   case "SuperSpeedItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(2, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuSuperSpeedVoiceSfx);
      }
   case "SuperBounceItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(3, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuSuperBounceVoiceSfx);
      }
   case "ShockAbsorberItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(4, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuShockAbsorberVoiceSfx);
      }
   case "HelicopterItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(5, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuGyrocopterVoiceSfx);
      }
   case "MegaMarbleItem":
      if ($MP::FastPowerups && (!$Server::Hosting || $Server::_Dedicated)) {
         $MP::MyMarble.setPowerUpId(6, true);
         PlayGui.setPowerUp(%this.getDataBlock().shapeFile);
         alxPlay(PuMegaMarbleVoiceSfx);
      }
   }
}

package Marbles2 {
   function Marble::setPowerUpId(%this, %id, %reset) {
		Parent::setPowerUpId(%this, %id, %reset);
		if (%this._powerUpId && $MP::FastPowerups) {
			%this._powerUpId = %id;
			if (%id > 5)
				%id = 0;
		}
      echo(%this SPC "setting powerupid to" SPC %id SPC "reset:" SPC %reset);
   }

	function Marble::onPowerUpUsed(%this) {
		Parent::onPowerUpUsed(%this);
		if (%this._powerUpId && $MP::FastPowerups) {
			//HiGuy: When you use a powerup, we want to know
			switch (%this._powerUpId) {
			case 1: alxPlay(doSuperJumpSfx);
			case 2: alxPlay(doSuperSpeedSfx);
			case 3: alxPlay(doSuperBounceSfx);
			case 4: alxPlay(doShockAbsorberSfx);
			case 5:
				alxPlay(doHelicopterSfx);
//				$MP::MyMarble.mountImage(HelicopterImage, 0);
			case 6: //Mega
				%this.setDataBlock(MegaMarble);
				commandToServer('MegaMarble');
			}
			echo(%this SPC "using powerup" SPC %this._powerUpId);
			%this._powerUpId = 0;
		}
	}
};

activatePackage(Marbles2);

function boxInterceptsBox(%box, %box2) {
   if (getWord(%box, 0) > getWord(%box2, 3))
      return false;
   if (getWord(%box, 1) > getWord(%box2, 4))
      return false;
   if (getWord(%box, 2) > getWord(%box2, 5))
      return false;
   if (getWord(%box, 3) < getWord(%box2, 0))
      return false;
   if (getWord(%box, 4) < getWord(%box2, 1))
      return false;
   if (getWord(%box, 5) < getWord(%box2, 2))
      return false;
   return true;
}

// Jeff: build the item list an in array object
function buildItemList() {
   while (isObject(ItemArray))
      ItemArray.delete();
   %array = Array(ItemArray);
   %count = ServerConnection.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %obj = ServerConnection.getObject(%i);

      // Jeff: use bitmasks for fast comparison, faster than using $=
      if (%obj.getType() & $TypeMasks::ItemObjectType) {

         // Jeff: if we are hidden, then don't add it
         if (%obj.isHidden())
            continue;

         %array.addEntry(%obj.getID() TAB %obj.getWorldBox());
      }
   }
   $LastSCCount = ServerConnection.getCount();
}

function letest() {
   exec($usermods @ "/leaderboards/multiplayer/clientGhost.cs");
   localClientConnection.setSimulatedNetParams(0.02, 300);
   MPgetMyMarble();
   buildItemList();
   updateItemCollision();
}
