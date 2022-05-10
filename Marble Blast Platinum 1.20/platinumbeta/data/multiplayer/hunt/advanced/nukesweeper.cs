//-----------------------------------------------------------------------------
//HiGuy: Modify these to however many you want

$Game::Nukesweeper::UltraBlasts = 3;
$Game::Nukesweeper::MegaMarbles = 2;
$Game::NukesweeperSpawn = 10;

datablock TriggerData(NukesweeperTrigger) {
   tickPeriodMS = 100;
   nukeTrigger = true;
};

function NukesweeperTrigger::onEnterTrigger(%this, %trigger, %obj) {
   %trigger.holding ++;
}

function NukesweeperTrigger::onLeaveTrigger(%this, %trigger, %obj) {
   %trigger.holding --;
}

function NukesweeperTrigger::createItem(%this, %trigger) {
   %spawnClass = "";
   %spawnData = "";
   %scale = "1 1 1";
   %add = "0 0 0";
   switch$ (%trigger.item) {
   case "ultrablast":
      %spawnClass = "Item";
      %spawnData = "BlastItem";
      %add = "0 0 0";
      %scale = "2.5 2.5 1";
   case "megamarble":
      %spawnClass = "Item";
      %spawnData = "MegaMarbleItem";
      %add = "0 0 0";
      %scale = "2.35 2.35 0.5";
   case "gemormine":
      %nuke = getRandom(0, 10) > 5 && $Game::NukesweeperSpawned + 1 != $Game::NukesweeperSpawn;

      if (!%nuke) {
         $Hunt::CurrentGemCount ++;
         %trigger.item = "gem";
         //HiGuy: Originally was 3 3 1, but I could collect gems outside of the
         // square.
         %scale = "2.35 2.35 0.4";
      } else {
         %trigger.item = "nuke";
         %scale = "5.4 5.4 6";
      }
      %spawnClass = (%nuke ? "StaticShape" : "Item");
      %spawnData = (%nuke ? "Nuke" : "GemItem");
   }

   %trigger.object = new (%spawnClass)() {
      position    = VectorAdd(%trigger.position, %add);
      rotation    = %trigger.rotation;
      scale       = %scale;
      dataBlock   = %spawnData;
      nukesweeper = true;
      trigger     = %trigger;

      rotate = 1;
      collideable = 0;
      static = 1;
   };
   MissionCleanup.add(%trigger.object);
}

function NukesweeperTrigger::setItem(%this, %trigger, %item) {
   if (%trigger.item !$= "")
      %this.reset(%trigger);
   %trigger.item = %item;
   if (%item $= "")
      %this.reset(%trigger);
   else
      %this.createItem(%trigger);
}

function nukesweeperSpawnPowerups() {
   cancel($NukesweeperPowerupSchedule);

   //HiGuy: The game has to be running for this to work, of course!
   if (!$Game::Running)
      return;

   //HiGuy: Get reference vars
   makeNukesweeperGroup(MissionGroup, true);
   makeNukesweeperGroup(MissionCleanup);

   //HiGuy: Clear all powerups
   for (%i = 0; %i < $NukesweeperTriggersCount; %i ++) {
      %trigger = $NukesweeperTrigger[%i];
      if (%trigger.item $= "ultrablast" || %trigger.item $= "megamarble")
         %trigger.getDataBlock().setItem(%trigger, "");
   }

   %possible = Array();

   for (%i = 0; %i < $NukesweeperTriggersCount; %i ++) {
      %trigger = $NukesweeperTrigger[%i];
      if (%trigger.item $= "")
         %possible.addEntry(%trigger);
   }

   //HiGuy: Spawn ultrablasts
   for (%i = 0; %i < $Game::Nukesweeper::UltraBlasts; %i ++) {
      %index = getRandom(0, %possible.getSize());
      %trigger = %possible.getEntry(%index);
      if (%trigger.item $= "") {
         %trigger.getDataBlock().setItem(%trigger, "ultrablast");
         %possible.removeEntry(%index);
      }
   }

   //HiGuy: Spawn MegaMarbles
   for (%i = 0; %i < $Game::Nukesweeper::UltraBlasts; %i ++) {
      %index = getRandom(0, %possible.getSize());
      %trigger = %possible.getEntry(%index);
      if (%trigger.item $= "") {
         %trigger.getDataBlock().setItem(%trigger, "megamarble");
         %possible.removeEntry(%index);
      }
   }

   %possible.delete();

   //HiGuy: Schedule this to reset every 10 secs
   $NukesweeperPowerupSchedule = schedule(10000, 0, "nukesweeperSpawnPowerups");
}

function nukesweeperSpawn() {
   updateGemCount();

   if (!isEventPending($NukesweeperPowerupSchedule))
      nukesweeperSpawnPowerups();

   if ($Hunt::CurrentGemCount != 0)
      return;

   //HiGuy: Reference things
   makeNukesweeperGroup(MissionGroup, true);
   makeNukesweeperGroup(MissionCleanup);

   %megamarbles = 0;
   %ultrablasts = 0;
   for (%i = 0; %i < $NukesweeperTriggersCount; %i ++) {
      %trigger = $NukesweeperTrigger[%i];
      if (%trigger.item $= "ultrablast")
         %ultrablasts ++;
      if (%trigger.item $= "megamarble")
         %megamarbles ++;
   }
   if (%megamarbles == 0 || %ultrablasts == 0)
      nukesweeperSpawnPowerups();

   //HiGuy: Clear old gems (mines stay on)
   for (%i = 0; %i < $NukesweeperTriggersCount; %i ++) {
      %trigger = $NukesweeperTrigger[%i];
      if (%trigger.item $= "gem")
         %trigger.getDataBlock().setItem(%trigger, "");
   }

   %array = Array();
   $Hunt::CurrentGemCount = 0;

   //HiGuy: Spawn more gems/mines
   %center = $NukesweeperTrigger[getRandom(0, $NukesweeperTriggersCount)];
   for (%i = 0; %i < $NukesweeperTriggersCount; %i ++) {
      %trigger = $NukesweeperTrigger[%i];

      if (%trigger.item $= "ultrablast" || %trigger.item $= "megamarble")
         continue;

      %distance = VectorDist(%trigger.getPosition(), %center.getPosition());
      %array.addEntry(%distance TAB %trigger);
   }

//   addGemLight(%center);

   %array.sortLowToHigh(0);
   $Game::NukesweeperSpawned = 0;

   for (%i = 0; %i < $Game::NukesweeperSpawn; %i ++) {
      %trigger = getField(%array.getEntry(%i), 1);
      //HiGuy: Get distance from a player
      %center = %trigger.position;
      %spawn = true;
      for (%j = 0; %j < ClientGroup.getCount(); %j ++) {
         %client = ClientGroup.getObject(%j);
         %player = %client.player.getWorldBoxCenter();
         if (VectorDist(%center, %player) < 1) {
            %spawn = false;
            break;
         }
      }
      if (%spawn == false)
         continue;
      %trigger.getDataBlock().setItem(%trigger, "gemormine");
      $Game::NukesweeperSpawned ++;
   }

   %array.delete();

   //Flash
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      if (isObject(ClientGroup.getObject(%i).player))
         ClientGroup.getObject(%i).player.setWhiteOut(0.10);
   }
}

function NukesweeperTrigger::reset(%this, %trigger) {
   if (isObject(%trigger.object)) {
      %trigger.object.delete();
      %trigger.object = "";
   }
}

function makeNukesweeperGroup(%group, %reset) {
   if (%reset) {
      deleteVariables("$Nukesweeper*");
      $NukesweeperTriggersCount = 0;
   }
   // Get all gems out there are in the world
   for (%i = 0; %i < %group.getCount(); %i++) {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
      if (%type $= "SimGroup") {
         makeNukesweeperGroup(%object, false);
      } else {
         if (%type $= "Trigger" && %object.getDatablock().nukeTrigger) {
            $NukesweeperTrigger[$NukesweeperTriggersCount] = %object;
            $NukesweeperTriggersCount ++;
         }
      }
   }
}
