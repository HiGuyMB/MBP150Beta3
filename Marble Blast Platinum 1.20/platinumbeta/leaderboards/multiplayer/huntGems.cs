//------------------------------------------------------------------------------
// Multiplayer Package
// huntGems.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

$Hunt::LastSpawnPosition = "-10000 -10000 -10000";
$Hunt::RadiusFromGem = 15;
$Hunt::GemPickCount = 6;
$Hunt::CurrentGemCount = 0;
$Hunt::NoParticles = false;
$Hunt::MaxGemsPerSpawn = 10;

datablock StaticShapeData(GemLight) {
   shapeFile = "~/data/shapes/gemlights/gemlight.dts";
   emap = false;
};

//le gem

// Jeff: spawn the gem group
function spawnHuntGemGroup(%exclude) {
   //HiGuy: No recursing!
   if ($Game::SpawningGems)
      return;
   if (!$Server::Hosting || $Server::_Dedicated)
      return;

   //HiGuy: We want to hide the gemlights
   commandToAll('UpdateHandicapItems');

   $Game::SpawningGems = true;
   //HiGuy: Prerecorded
   if ($Hunt::Prerecorded)
   	replaySpawnHuntGroup();
   else
	   doSpawnHuntGemGroup(%exclude);
   $Game::SpawningGems = false;
   commandToAll('RadarBuildSearch');
}

function doSpawnHuntGemGroup(%exclude) {
   if (MissionInfo.nukesweeper) {
      %ret = nukesweeperSpawn(%exclude);
      return %ret;
   }

   if (isObject(GemGroups) && MissionInfo.gemGroups && GemGroups.getCount()) {
      //HiGuy: We need to do a gemgroups spawn
      hideGems();
      devecho("Doing a gemgroup spawn!");

      //HiGuy: We can do a gemGroup spawn here!
      %count = 0;

      while ($Hunt::CurrentGemCount == 0 && (%count ++) < 20) {
         %groupCount = GemGroups.getCount();
         %spawnables = new ScriptObject() {
            count = 0;
         };
         %max = 0;
         if (%groupCount == 1)
            %toSpawn = GemGroups.getObject(0);
         else {
            devecho("Found" SPC %groupCount SPC "gemgroups");
            for (%i = 0; %i < %groupCount; %i ++) {
               %group = GemGroups.getObject(%i);
               if (%group.spawnCount > %max)
                  %max = %group.spawnCount;
            }
            for (%i = 0; %i < %groupCount; %i ++) {
               %group = GemGroups.getObject(%i);
               for (%j = %group.spawnCount; %j <= %max + 1; %j ++) {
                  %spawnables.group[%spawnables.count] = %group;
                  %spawnables.count ++;
               }
            }

            while (!isObject(%spawnGroup))
               %spawnGroup = getRandom(0, %spawnables.count);

            //%spawnables.dump();
            //echo("%max is" SPC %max);

            %toSpawn = %spawnables.group[%spawnGroup];
            %spawnables.delete();

            %toSpawn.spawnCount ++;
         }

         devecho("Spawning group" SPC %toSpawn);

         if (MissionInfo.gemGroups == 2) {
            spawnHuntGemsInGroup(%toSpawn, %exclude);

            return;
         } else {
            for (%i = 0; %i < %toSpawn.getCount(); %i ++) {
               if (%toSpawn.getObject(%i) != %exclude)
                  spawnGem(%toSpawn.getObject(%i));
            }
         }
      }
      return;
   }
   spawnHuntGemsInGroup("MissionGroup;MissionCleanup", %exclude);
}

function spawnHuntGemsInGroup(%groups, %exclude) {
   //echo("Spawning in groups" SPC %groups);
   %groups = nextToken(%groups, "group", ";");
   makeGemGroup(%group, true);

   while (%groups !$= "") {
      %groups = nextToken(%groups, "group", ";");
      makeGemGroup(%group);
   }
   hideGems();

   %spawnCount = (MissionInfo.maxGemsPerSpawn ? MissionInfo.maxGemsPerSpawn : $Hunt::MaxGemsPerSpawn);
   %spawnRadius = (MissionInfo.radiusFromGem ? MissionInfo.radiusFromGem : $Hunt::RadiusFromGem);
   %spawnBlock = (MissionInfo.spawnBlock ? MissionInfo.spawnBlock : %spawnRadius * 2);

   echo(%spawnBlock);

   %lastSpawn = $Game::LastGemSpawner;

   //echo("Last spawn is" SPC %lastSpawn);
   if (isObject(%lastSpawn))
      %lastPos = getWords(%lastSpawn.getTransform(), 0, 2);
   //echo("Last pos is" SPC %lastPos);
   %furthest = 0;
   %furthestDist = 0;

   %best = ClientGroup.getObject(0);
   for (%i = 1; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.getPlace() < %best.getPlace())
         %best = %client;
   }
   echo("Best is" SPC %best.namebase);
   if (%best.gemCount == 0)
      %ratio = 1;
   else
      %ratio = max(0.01, min(4, mFloor(%best.gemCount * (MissionInfo.time / (MissionInfo.time - PlayGui.elapsedTime))) / max(MissionInfo.ultimateScore[getRealPlayerCount() == 1], 1)));

   echo("Ratio is" SPC %ratio);
   %ratio = %ratio * %ratio * (%ratio > 1 ? %ratio * (%ratio > 1.25 ? %ratio : 1) : 1);
   echo("Final ratio is" SPC %ratio);

   if ($MPPref::JerkSpawns) {
      if (%ratio > 1)
         %lastPos = getWords(%best.player.getTransform(), 0, 2);
      echo("Prev spawnblock is:" SPC %spawnBlock);
      %spawnBlock *= %ratio;
      echo("Jerked spawnblock is:" SPC %spawnBlock);
   }

   //HiGuy: Fuck getRandomSeed. Fuck it. Fuck. This is the most confusing thing
   // ever. It resets every time you call getRandom, unless it is 0, but then it
   // will only ever give you 0 for a random. FUCK. THIS. GAAAAAAAAAAAAAAH
   if (getRandomSeed() == 0)
      setRandomSeed(getRealTime());

   %checked = array();

   if ($MPPref::NewSpawnCode) {
         //HiGuy: Try six times to get a spawn group
      if (isObject(%lastSpawn))
         for (%i = 0; %i < $Hunt::GemPickCount || !%furthest; %i ++) {
            %gem = $Gems[getRandom(0, $GemsCount)];
            //echo("Checking" SPC %gem);
            if (!isObject(%gem))
               continue;
            if (%gem == %exclude)
               continue;

            //HiGuy: Stop looking at gems twice!
            if (%checked.containsEntry(%gem) && %checked.getSize() < $GemsCount / 1.5)
               continue;

            %checked.addEntry(%gem);
            //HiGuy: Compare positions
            %gemPos = getWords(%gem.getTransform(), 0, 3);
            %dist = VectorDist(%gemPos, %lastPos) + %gem.spawnWeight;
            if ($MPPref::JerkSpawns) {
               echo("For a" SPC %gem.getSkinName() SPC "gem (" @ %gem @ ")...");
               echo("Prev dist is:" SPC %dist);
               %dist *= %ratio * (5 + %gem.getDataBlock().huntExtraValue * (1 - %ratio)) / 5;
               echo("Jerked dist is:" SPC %dist);
            }

            %block = %spawnBlock - mLogb(max(%i, $Hunt::GemPickCount) - 6, 1.25);

            if (%dist > %block) {
               //echo(%dist SPC ">" SPC %block);
               //HiGuy: Store furthest group out of all X
               if (%dist > %furthestDist) {
                  //echo("New furthest:" SPC %gem);
                  %furthestDist = %dis;
                  %furthest = %gem;
               }
               continue;
            }// else
               //error(%dist SPC "<" SPC %block);
         }
      else
         %furthest = $Gems[getRandom(0, $GemsCount)];
   } else {
      //HiGuy: Try six times to get a spawn group
      for (%i = 0; %i < 6; %i ++) {
         %gem = $Gems[getRandom(0, $GemsCount)];
         if (!isObject(%gem))
            continue;
         if (%gem == %exclude)
            continue;
         if (isObject(%lastSpawn) && %lastSpawn.getClassName() $= "Item") {
            //HiGuy: Compare positions
            %gemPos = getWords(%gem.getTransform(), 0, 3);
            %dist = VectorDist(%gemPos, %lastPos) + %gem.spawnWeight;
            if ($MPPref::JerkSpawns) {
               echo("For a" SPC %gem.getSkinName() SPC "gem (" @ %gem @ ")...");
               echo("Prev dist is:" SPC %dist);
               //(r d (5 + p (1 - r))) / 5
               %dist *= %ratio * (5 + %gem.getDataBlock().huntExtraValue * (1 - %ratio)) / 5;
               echo("Jerked dist is:" SPC %dist);
            }

            //HiGuy: OK I JUST FIGURED THIS OUT
            // APPARENTLY I'M SMARTER THAN MYSELF.... WHY?!

            //HiGuy: If the gem is not a candidate for spawning, store it in
            // case we can't spawn any. If it is the furthest of the bad spawns,
            // we'll spawn it.
            if (%dist < %spawnBlock) {
               //HiGuy: Store furthest group out of all 6 in case no gem can
               // be found to spawn.
               if (%dist > %furthestDist) {
                  %furthestDist = %dist - %gem.spawnWeight;
                  %furthest = %gem;
               }
               continue;
            } else {
               //HiGuy: If this gem works as a spawn center, USE IT USE IT
               break;
            }
         } else {
            //HiGuy: If lastSpawn is not a Gem, then any spawn should work
            break;
         }
      }

      //HiGuy: If no group was acceptable, use the furthest group
      if (%i == 6) {
         %gem = %furthest;
         devecho("settling");
      }
   }
   //echo("Furthest is" SPC %furthest);
   //echo("Furthest dist is" SPC %furthestDist);

   if (%furthest == 0)
      %furthest = %gem;

   %center = %furthest;

   devecho("Spawning a gem group around gem" SPC %furthest);
   if (%furthest $= "") {
      error("Could not spawn a gem group!");
      return;
   }

   %centerPos = getWords(%center.getTransform(), 0, 3);
   %spawnables = new ScriptObject() {
      count = 0;
   };

   //HiGuy: Spawn all gems inside the spawnsphere around the gem
   for (%i = 0; %i < $GemsCount; %i ++) {
      %gem = $Gems[%i];
      if (%gem == %exclude)
         continue;
      %gemPos = getWords(%gem.getTransform(), 0, 3);
      if (VectorDist(%gemPos, %centerPos) < %spawnRadius) {
         //HiGuy: Add it to the list
         %spawnables.item[%spawnables.count] = %gem;
         %spawnables.weight[%spawnables.count] = %spawnRadius - (VectorDist(%gemPos, %centerPos)) + getRandom(0, (%gem.getDataBlock().huntExtraValue * 1) + 3);
         if ($MPPref::JerkSpawns) {
            echo("For a" SPC %gem.getSkinName() SPC "gem (" @ %gem @ ")...");
            echo("Prev weight:" SPC %spawnables.weight[%spawnables.count]);
            %spawnables.weight[%spawnables.count] *= %ratio * (5 + %gem.getDataBlock().huntExtraValue * (1 - %ratio)) / 5;
            echo("Jerked weight:" SPC %spawnables.weight[%spawnables.count]);
         }
         //devecho("Gem" SPC %gem SPC "dist" SPC VectorDist(%gemPos, %centerPos) SPC "weight" SPC %spawnables.weight[%spawnables.count]);
         %spawnables.count ++;
      }
   }

   //HiGuy: Order by spawn weight
   for (%i = 0; %i < %spawnables.count; %i ++) {
      %max = -9999999;
      %maxNum = -1;
      for (%j = 0; %j < %spawnables.count; %j ++) {
         %item = %spawnables.item[%j];
         %weight = %spawnables.weight[%j];
         if (%weight !$= "" && %weight > %max) {
            %max = %weight;
            %maxNum = %j;
         }
      }
      //devecho("Spawning gem pos" SPC %i SPC "goes to" SPC %spawnables.item[%maxNum]);
      %spawnables.gem[%i] = %spawnables.item[%maxNum];
      %spawnables.item[%maxNum] = "";
      %spawnables.weight[%maxNum] = "";
   }

   devecho("Found" SPC %spawnables.count SPC "gems, can only spawn up to" SPC %spawnCount);

   %spawned = 0;

   //HiGuy: Spawn them!
   if (%spawnables.count > %spawnCount) {
      %on = 0;
      for (%i = 0; %i < %spawnCount; %i ++) {
         if (isObject(%spawnables.gem[%on])) {
            //HiGuy: If we can spawn them, remove them from the list. Otherwise
            // we want to spawn the next gem
            if (spawnGem(%spawnables.gem[%on])) {
               %spawnables.spawned[%spawned ++] = %spawnables.gem[%on];
               %spawnables.gem[%on] = "";
               for (%j = %on; %j < %spawnables.count; %j ++)
                  %spawnables.gem[%j] = %spawnables.gem[%j + 1];
               %spawnables.count --;
            } else {
               %on ++;
               %i --;
            }
         } else {
            %on ++;
            %i --;
         }
      }
   } else {
      for (%i = 0; %i < %spawnables.count; %i ++) {
         if (spawnGem(%spawnables.gem[%i]))
            %spawnables.spawned[%spawned ++] = %spawnables.gem[%i];
      }
   }

   spawnGem(%center);
   %spawnables.spawned[%spawned ++] = %center;

	recordSpawnGroup();

   %maxDist = 0;

   //HiGuy: Get the furthest gem
   for (%i = 0; %i < %spawned; %i ++) {
      %gem = %spawnables.spawned[%i];
      %dist = VectorDist(%gem.position, %centerPos);
      if (%dist > %maxDist)
         %maxDist = %dist;
   }

   //HiGuy: Apply spawn weights
   for (%i = 0; %i < %spawned; %i ++) {
      %gem = %spawnables.spawned[%i];
      %dist = VectorDist(%gem.position, %centerPos);
      %dist /= %maxDist;
      %dist = mFloor((1 - %dist) * 10);
      %gem.spawnWeight += %dist;
   }

   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);

   //HiGuy: Fix spawn weights so we don't get gems with 10000 spawn weight
   %min = 9999;
   for (%i = 0; %i < $GemsCount; %i ++) {
      %gem = $Gems[%i];
      if (%gem.spawnWeight < %min) {
         %min = %gem.spawnWeight;
         echo(%gem SPC %gem.spawnWeight SPC %min);
      }
   }

   //echo("Min is" SPC %min);

   if (%min) {
      //echo("Lowering total gem weight by" SPC %min);
      for (%i = 0; %i < $GemsCount; %i ++) {
         %gem = $Gems[%i];
         %gem.spawnWeight -= %min;
      }
   }

   %spawnables.delete();
   $Game::LastGemSpawner = %center;

   //Flash
   if (!$MP::FinalSpawn) {
      %count = ClientGroup.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %client = ClientGroup.getObject(%i);
         if (isObject(%client.player))
            %client.player.setWhiteOut(0.10);
      }
   }

//   %center.light.setSkinName("platinum");
}

function resetSpawnWeights() {
   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);

   for (%i = 0; %i < $GemsCount; %i ++) {
      %gem = $Gems[%i];
      %gem.spawnWeight = 0;
   }
}

function spawnGem(%gem) {
   if (!isObject(%gem))
      return false;
   if (%gem.getDataBlock().huntExtraValue $= "") {
   	//HiGuy: If it's not a red/yellow/blue gem, make it a red gem
   	%gem.setDataBlock(GemItemRed);
   	%gem.setSkinName("red");
	}
   if (!addGemLight(%gem) && !$_nodespawn)
      return false;
   if (!isObject(SpawnedSet))
      RootGroup.add(new SimSet(SpawnedSet));

   SpawnedSet.add(%gem);

   if ($MP::FinalSpawn)
      return true;

   if (%gem.isHidden()) {
      %gem.hide(false);
      %gem.setRadarTarget();
      $Hunt::CurrentGemCount ++;
   }

   return true;
}

function unspawnGem(%gem, %nocheck) {
   if (!isObject(%gem))
      return;
   if (!isObject(SpawnedSet))
      RootGroup.add(new SimSet(SpawnedSet));
   if (SpawnedSet.isMember(%gem))
      SpawnedSet.remove(%gem);
   if ($Hunt::CurrentGemCount > 0)
      $Hunt::CurrentGemCount --;

   if ($Hunt::CurrentGemCount <= 0 && !%nocheck)
      spawnHuntGemGroup(%gem);

   if ($_nodespawn)
      return;
   removeGemLight(%gem);
   %gem.hide(true);
}

function resetSpawnGroup() {
	if ($Hunt::Prerecorded)
		$Hunt::CurrentSpawn = 0;
	else
		deleteSpawnGroups();
}

function deleteSpawnGroups() {
	 while (isObject(SpawnRecordGroup))
		SpawnRecordGroup.delete();
}

function recordSpawnGroup() {
   if (!$MP::FinalSpawn && (!$Server::Started || $Game::Finished || !$Game::Running) && !$Hunt::Prerecorded)
      return;

   //All gems in SpawnedSet are spawned
	if (!isObject(SpawnRecordGroup))
		RootGroup.add(new SimGroup(SpawnRecordGroup));

	SpawnRecordGroup.add(%set = new SimSet());
	for (%i = 0; %i < SpawnedSet.getCount(); %i ++) {
		%gem = SpawnedSet.getObject(%i);
		%set.add(%gem);
		%set.value ++;
		%set.value += %gem.getDataBlock().huntExtraValue;
	}

	%set.final = $MP::FinalSpawn;
	SpawnRecordGroup.totalPoints[SpawnRecordGroup.getCount()] = SpawnRecordGroup.totalPoints[SpawnRecordGroup.getCount() - 1] + %set.value;
}

function saveSpawnGroup(%file) {
	//HiGuy: Fill up the group full of things
	$MP::FinalSpawn = true;
	while (SpawnRecordGroup.getCount() < 40)
		spawnHuntGemGroup();
	$MP::FinalSpawn = false;

	%exp = SpawnRecordGroup.getCount();
	%exp = %exp @ ";";
	//HiGuy: Export the spawn group
	for (%i = 0; %i < SpawnRecordGroup.getCount(); %i ++) {
		%exp = %exp @ mFloor(SpawnRecordGroup.totalPoints[%i]);
		%exp = %exp @ ";";

		%set = SpawnRecordGroup.getObject(%i);
		%exp = %exp @ mFloor(%set.value);
		%exp = %exp @ ";";
		%exp = %exp @ %set.getCount();
		%exp = %exp @ ";";

		//HiGuy: Export Gems
		for (%j = 0; %j < %set.getCount(); %j ++) {
			%gem = %set.getObject(%j);
			%exp = %exp @ %gem.position;
			%exp = %exp @ ";";
			%exp = %exp @ %gem.getDataBlock().getName();
			%exp = %exp @ ";";
			%exp = %exp @ 1 + %gem.getDataBlock().huntExtraValue;
			%exp = %exp @ ";";
		}
	}
	%exp = %exp @ mFloor(SpawnRecordGroup.totalPoints[%i]);
	%exp = %exp @ ";";

	%t = getRealTime();
	//HiGuy: By now, %exp is huge. Let's run it through a few algorithms to
	// make it even larger :D
	%exp = strEnc(%exp, 2);
	echo("Encrypt took" SPC getRealTime() - %t SPC "ms");

	%t = getRealTime();
	//HiGuy: Write %exp to a file
	%o = new FileObject();
	if (!%o.openForWrite(%file)) {
		%o.close();
		%o.delete();
		return false;
	}
	%start = 0;
	%len = strlen(%exp);

	//HiGuy: The newlines don't really matter... or do they?
	for (%i = 20; %start < %len; %i = getRandom(40, 100)) {
		//HiGuy: Confuse people because it's fun
		%o.writeLine(chrForValue(getRandom(33, 126)) @ getSubStr(%exp, %start, %i) @ chrForValue(getRandom(33, 126)));

		%start += %i;
	}
	%o.close();
	%o.delete();
	return true;
}

function loadSpawnGroup(%file) {
   while (isObject(SpawnRecordGroup))
      SpawnRecordGroup.delete();
   RootGroup.add(new SimGroup(SpawnRecordGroup));

	devecho("\c1Loading spawn groups from:" SPC %file);

   if (parseSpawnGroup(%file)) {
      devecho("\c3Loaded spawn groups!");
      $Hunt::Prerecorded = true;
      $Hunt::CurrentSpawn = 0;
      return true;
   } else {
      SpawnRecordGroup.delete();
      devecho("\c2Could not load spawn groups!");
      return false;
   }
}

function parseSpawnGroup(%file) {
   //HiGuy: Read and parse the file
   %o = new FileObject();
   if (!%o.openForRead(%file)) {
      %o.close();
      %o.delete();
      return false;
   }
   %exp = "";
   %t = getRealTime();
   while (!%o.isEOF()) {
      %line = %o.readLine();

      //HiGuy: First and last chars are garbage
      %line = getSubStr(%line, 1, strlen(%line));
      %line = getSubStr(%line, 0, strlen(%line) - 1);
      %exp = %exp @ %line;
   }
   echo("Read took" SPC getRealTime() - %t SPC "ms");
   %o.close();
   %o.delete();

   //HiGuy: Decrypt it
   %t = getRealTime();
   %exp = strDec(%exp, 2);
   echo("Decrypt took" SPC getRealTime() - %t SPC "ms");

   %next = -1;
   %score = 0;

   //File format:
   //SpawnSetCount;SpawnSet;SpawnSet;...;FinalTotal;
   //
   //SpawnSet format:
   //PreviousValue;SetValue;SetCount;Gem;Gem;...;
   //
   //Gem format:
   //Position;DataBlock;PointValue;

   //HiGuy: Data read function
   //
   // %start = %next + 1;
   // %next = strPos(%exp, ";", %start);
   // if (%next == -1)
   //    return
   // %length = %next - %start;
   // %data = getSubStr(%exp, %start, %length);
   //

   %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
   %spawnsets = getSubStr(%exp,%start,%next-%start);

   devecho("Read spawnset count:" SPC %spawnsets);

   for (%i = 0; %i < %spawnsets; %i ++) {
      %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
      %prevValue = getSubStr(%exp,%start,%next-%start);

      devecho("Read previous value:" SPC %prevValue);

      %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
      %setValue = getSubStr(%exp,%start,%next-%start);

      devecho("Read set value:" SPC %setValue);

      %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
      %setCount = getSubStr(%exp,%start,%next-%start);

      devecho("Read set count:" SPC %setCount);

      %setScore = 0;

      SpawnRecordGroup.add(%set = new SimSet());

      for (%j = 0; %j < %setCount; %j ++) {
         %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
         %position = getSubStr(%exp,%start,%next-%start);

         devecho("Read gem position:" SPC %position);

         %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
         %datablock = getSubStr(%exp,%start,%next-%start);

         devecho("Read gem datablock:" SPC %datablock);

         %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
         %gemValue = getSubStr(%exp,%start,%next-%start);

         devecho("Read gem value:" SPC %gemValue);

         //HiGuy: Check the gem
         %gem = getGemAtPosition(%position, %datablock);
         if (%gem == -1) {
            devecho("Invalid gem pos:" SPC %position);
            return 0;
         }

         if (%gem.getDataBlock().getName() !$= %datablock) {
            devecho("Invalid gem datablock:" SPC %datablock);
            return 0;
         }

         if (%gem.getDataBlock().huntExtraValue + 1 != %gemValue) {
            devecho("Invalid gem value:" SPC %gemValue);
            return 0;
         }
         %set.add(%gem);

         %setScore += %gemValue;
      }

      if (%setScore != %setValue) {
         devecho("Invalid set value:" SPC %setValue);
         return 0;
      }
      %set.value = %setValue;

      %score += %setValue;
   }

   %start=%next+1;%next=strPos(%exp,";",%start);if(%next==-1)return 0;
   %finalscore = getSubStr(%exp,%start,%next-%start);

   devecho("Read final score:" SPC %finalscore);

   if (%finalscore != %score) {
      devecho("Invalid final score:" SPC %finalscore);
      return 0;
   }

   return 1;
}

function replaySpawnHuntGroup() {
   if ($Hunt::CurrentGemCount)
   	return;
   hideGems();
   backtrace();
   if (!isObject(SpawnRecordGroup) || SpawnRecordGroup.getCount() == 0)
   	return false;

	%spawnGroup = SpawnRecordGroup.getObject($Hunt::CurrentSpawn);
	echo("Next spawngroup has" SPC %spawnGroup.getCount() SPC "gems");
	for (%i = 0; %i < %spawnGroup.getCount(); %i ++) {
		//Detach
		schedule(0, 0, spawnGem, %spawnGroup.getObject(%i));
		echo("Spawning" SPC %spawnGroup.getObject(%i));
	}
	$Hunt::CurrentSpawn ++;

	return true;
}

function getGemAtPosition(%position, %datablock, %grp) {
   %set = findObjectsAtPosition(%position, %grp);
	%set.schedule(0, "delete"); //Memory mgmt

	//We might have more than one gem
  	for (%i = 0; %i < %set.getSize(); %i ++) {
  		%obj = %set.getEntry(%i);
  		//If it's correct
      if (%obj.getClassName() $= "Item" && %obj.getDataBlock().className $= "Gem" && %obj.position $= %position && %obj.getDataBlock().getName() $= %datablock)
	      return %obj;
   }
   return -1;
}


function findObjectsAtPosition(%position, %grp, %array) {
	if (%array $= "")
		%array = Array();

   if (%grp $= "") %grp = MissionGroup;

   for (%i = 0; %i < %grp.getCount(); %i ++) {
      %obj = %grp.getObject(%i);
      if (%obj.getPosition() $= %position)
         %array.addEntry(%obj);
      if (%obj.getClassName() $= "SimGroup") //Append to %array
      	findObjectsAtPosition(%position, %obj, %array);
   }

   return %array;
}

// Jeff: hides *ALL* the gems
function hideGems() {
   $Hunt::CurrentGemCount = 0;
   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);
   if ($Game::isHunt) {
      for (%i = 0; %i < $GemsCount; %i ++) {
         %gem = $Gems[%i];
         unspawnGem(%gem, true);
      }
   }
}

function showGems() {
   $Hunt::CurrentGemCount = 0;
   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);
   if ($Game::isHunt) {
      for (%i = 0; %i < $GemsCount; %i ++) {
         %gem = $Gems[%i];
         spawnGem(%gem);
      }
   }
}

//HiGuy: Shows all gems, but without gemlights / gemcount
// Used only by the level editor
function resetGems() {
   hideGems();
   $Hunt::CurrentGemCount = 0;
   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);
   if ($Game::isHunt) {
      for (%i = 0; %i < $GemsCount; %i ++) {
         %gem = $Gems[%i];
         if (!isObject(%gem))
            continue;
         %gem.hide(false);
      }
   }
}

function addGemLight(%gem) {
   if (!isObject(%gem)) return false;
   if (isObject(%gem.light)) return false;
   if (%gem.noLight) return false;

   MissionCleanup.add(%gem.light = new StaticShape() {
      datablock = "GemLight";
      position = %gem.position;
      rotation = %gem.rotation;
      scale = %gem.scale;
   });

   %gem.light.setSkinName(%gem.getDataBlock().skin);
   if (%gem.getDataBlock().skin $= "default" || %gem.getDataBlock().skin $= "")
      %gem.setSkinName("red");
   return %gem.light;
}

function removeGemLight(%gem) {
   if (!isObject(%gem)) return false;
   if (!isObject(%gem.light)) return false;

   %gem.light.delete();
   %gem.light = "";
}

// Jeff: copied HiGuy's gem group function from emerald*
//HiGuy: This was originally in emerald, moved to opal, then PR, then elite, now it's here! Hooray!
function makeGemGroup(%group, %reset) {
   //echo("Making gem group for group" SPC %group);
   if (%reset) {
      deleteVariables("$Gems*");
      $GemsCount = 0;
   }
   // Get all gems out there are in the world
   %count = %group.getCount();
   for (%i = 0; %i < %count; %i++) {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
      if (%type $= "SimGroup") {
         makeGemGroup(%object, false);
      } else {
         if (%type $= "Item" && %object.getDatablock().classname $= "Gem") {
            $Gems[$GemsCount] = %object;
            $GemsCount ++;
         }
      }
   }
}

function updateGemCount() {
   makeGemGroup(MissionGroup, true);
   makeGemGroup(MissionCleanup);

   $Hunt::CurrentGemCount = 0;
   for (%i = 0; %i < $GemsCount; %i ++) {
      %gem = $Gems[%i];
      if (!%gem.isHidden())
         $Hunt::CurrentGemCount ++;
   }
}

//HiGuy: Average all group element positions
function SimSet::getPosition(%this) {
   %center = "";
   %elements = %this.getCount();
   for (%i = 0; %i < %elements; %i ++) {
      %elem = %this.getObject(%i);
      %elemPos = %elem.getPosition();
      %center = VectorAdd(%center, %elemPos);
   }
   %center = VectorScale(%center, 1 / %elements);
   return %center;
}

//HiGuy: Average all group element positions
function getMissionCenter() {
   %center = "";
   %elements = MissionGroup.getCount();
   for (%i = 0; %i < %elements; %i ++) {
      %elem = MissionGroup.getObject(%i);
      if (%elem.getClassName() !$= "InteriorInstance")
         continue;
      %elemPos = %elem.getWorldBoxCenter();
      %center = VectorAdd(%center, %elemPos);
   }
   %center = VectorScale(%center, 1 / %elements);
   return %center;
}
