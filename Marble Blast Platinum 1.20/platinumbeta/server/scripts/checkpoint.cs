// SPY47 CHECKPOINT BASE CODE

datablock AudioProfile(CheckpointSfx)
{
   filename    = "~/data/sound/checkpoint.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock TriggerData(CheckpointTrigger)
{
   tickPeriodMS = 100;
   checkpointNum = "0";
   add = "0 0 3";
   sub = "0 0 0";
};

datablock StaticShapeData(checkPoint)
{
   className = "Button";
   category = "Buttons";
   shapeFile = "~/data/shapes/buttons/checkPoint.dts";
   resetTime = 5000;
};

// CHECKPOINT BUTTON SPY47
function CheckpointTrigger::onEnterTrigger(%this,%obj,%col)
{
   //echo("got into trigger");
   //%obj.setCheckpoint($Game::Checkpoint[%this.checkpointNum]);

   %player = %col.client;

   //if(!%obj.checkpointNum)
   //   echo("ERROR!!! No checkpointNum specified.");
   if( %obj != %player.curCheckpoint )
   {
      %col.client.setCheckpointTrigger(%obj);
   }
}

function checkPoint::onCollision(%this,%obj,%col,%vec, %vecLen, %material)
{
   //echo("got into trigger");
   //if(!%obj.checkpointNum)
   //   echo("ERROR!!! No checkpointNum specified.");

   %player = %col.client;
   if( %obj != %col.client.curCheckpoint )
   {
      %col.client.setCheckpointButton(%obj);
   }
}

function GameConnection::setCheckpointButton(%this, %object)
{
   // This is for checkpoint button.

   // SPY47 DO WEIRD STUFF YOU WON'T UNDERSTAND NO MATTER YOU TRY.

   //echo("DEBUG: Current gravity dir:" SPC $Game::GravityDir);
   //echo("DEBUG: Saving direction: " @ %object.direction);
   //if (%object != %player.checkPoint.pad) {
   //   %player.checkPoint.delete();
   //   %player.checkPoint = new ScriptObject() {
   //      pad = %object;
   //      time = %gravity;
   //      gemCount = %player.gemCount;
   //      penaltyTime = %player.penaltyTime;
   //      bonusTime = %player.bonusTime;
   //      powerUp = %player.player.getPowerUp();
   //      add = %object.add;
   //      sub = %object.sub;
   //      gravityDir = $Game::GravityDir;
   //   };
   %this.checkpointPad = %object;
	%this.checkpointTime = %gravity;
	%this.checkpointGemCount = %this.gemCount;
	%this.checkpointPenaltyTime = %this.penaltyTime;
	%this.checkpointBonusTime = %this.bonusTime;
	%this.checkpointPowerUp = %this.player.getPowerUp();
	%this.checkpointAdd = %object.add;
	%this.checkpointSub = %object.sub;
	%this.checkpointGravityDir = %this.gravityDir;
	%this.checkpointGravityRot = %this.gravityRot;

   messageClient(%this, 'MsgCheckpoint', "\c0Checkpoint reached!");
   %this.play2d(CheckpointSfx);
   //echo("gem count" @ %player.gemCount);
   %this.checkpointed = 1; // Spy47 : We've hit a checkpoint once.

   %this.curCheckpoint = %object;
   %this.curCheckpointNum++;
}

function GameConnection::setCheckpointTrigger(%this, %object)
{
   // Store the last checkpoint which will be used to restore
   // the player when he goes out of bounds.

   // SPY47 DO WEIRD STUFF YOU WON'T UNDERSTAND NO MATTER YOU TRY.

   %respawnPoint = %object.respawnPoint;


   if(%respawnPoint $= "")
   {
	   ASSERT("Error Handler", "You didn\'t specify the DTS name for respawning!\nPlease refer to the readme.");
	   return;
   }
   %dts = %this.getCheckpointDTS(MissionGroup, %respawnPoint);

   // Nice and beautiful errors.
   if(%dts == -1)
   {
      ASSERT("Error Handler", "Can\'t find the respawn point!\nMake sure you made a DTS for respawning over.\nPlease refer to the readme.");
	  return;
   }


   %objectType = %dts.getClassName();
  //echo("DEBUG: Classname " @ %objectType);
   %objectShape = %dts.shapeName;
  //echo("DEBUG: Objectshape " @ %objectShape);

  if(%objectType !$= "TSStatic")
   {
	   ASSERT("Error Handler", "You must specify a TSStatic shape for respawning on!");
	   return;
   }
   if(%objectShape !$= $usermods @ "/data/shapes/buttons/checkpoint.dts")
   {
	   //ASSERT("You got owned", "HAHA YOU GOTTA USE CHECKPOINT.DTS!!!!\nWANTED TO USE ANOTHER DTS!! DREAM ON\nHOW LONG DO U THINK IT TOOK TO MAKE CHECKPOINT.DTS HUH??\nAFTER SO MANY HOURS MAKING THE FUCKING DTS SO YOU COME AND USE ANOTHER ONE???\nFUCK OFF, GO TO THE LE AND SELECT CHECKPOINT.DTS!!!");
	   ASSERT("Error Handler", "You have to use platinum/data/shapes/buttons/checkpoint.dts as the checkpoint DTS!");
	   return;
   }

    //echo("DEBUG: Current gravity dir:" SPC $Game::GravityDir);
   //echo("DEBUG: Saving direction: " @ %object.direction);
   //if (%object != %player.checkPoint.pad) {
   //   %player.checkPoint.delete();
   //   %player.checkPoint = new ScriptObject() {
   //      pad = %object;
   //      time = %gravity;
   //      gemCount = %player.gemCount;
   //      penaltyTime = %player.penaltyTime;
   //      bonusTime = %player.bonusTime;
   //      powerUp = %player.player.getPowerUp();
   //      add = %object.add;
   //      sub = %object.sub;
   //      gravityDir = $Game::GravityDir;
   //   };
   %this.checkpointPad = %dts;
	%this.checkpointTime = %gravity;
	%this.checkpointGemCount = %this.gemCount;
	%this.checkpointPenaltyTime = %this.penaltyTime;
	%this.checkpointBonusTime = %this.bonusTime;
	%this.checkpointPowerUp = %this.player.getPowerUp();
	%this.checkpointAdd = %object.add;
	%this.checkpointSub = %object.sub;
	%this.checkpointGravityDir = %this.gravityDir;
	%this.checkpointGravityRot = %this.gravityRot;

   messageClient(%this, 'MsgCheckpoint', "\c0Checkpoint reached!");
   %this.play2d(CheckpointSfx);
   //echo("gem count" @ %player.gemCount);
   %this.checkpointed = 1; // Spy47 : We've hit a checkpoint once.

   %this.curCheckpoint = %object;
   %this.curCheckpointNum++;
}

// Jeff: CALM YO BUTT
// I FIXED DIS STUPID abuse where you would spawn at 0 0 300
// horray for simsets?
function GameConnection::getCheckpointPos(%this,%num,%add,%sub)
{
   // Return the point a little above the object's center
   if (!isObject(%this.checkpointPad)) {
      if ($Server::ServerType $= "MultiPlayer" || $Game::IsHunt) {
         //HiGuy: Gets a random spawn trigger
         %trigger = %this.getSpawnTrigger();
         //HiGuy: Muahaha anti-spawn abuse code goes here. I can't wait to see
         // people think they can abuse quick respawn, only to find themselves
         // way across the level >:D
         if (%this.quickRespawning)
            %trigger = %this.getFurthestSpawnTrigger();
         if (%this.restarting)
            %trigger = %this.getRandomSpawnTrigger();
         if (!isObject(%trigger))
            return "0 0 300 1 0 0 0\t1 0 0 3.1415926\t0.45"; // Jeff: If this happens, I will stop coding.

         %this.lastSpawnTrigger = %trigger;
         %trigger.getDataBlock().blockSpawning(%trigger);

         %add = %trigger.add !$= "" ? %trigger.add : %add;
         %sub = %trigger.sub !$= "" ? %trigger.sub : %sub;
         if (%add $= "" && %sub $= "") {
            %add = (%trigger.g ? VectorMult("-3 -3 -3", getWords(VectorOrthoBasis(getWords(%trigger.getTransform(), 3, 6)), 6, 8)) : "0 0 3");
            %spawnVec = vectorAdd(getWords(%trigger.getTransform(), 0, 2), %add);
            if (%trigger.center !$= "") {
               %spawnVec = vectorAdd(getWords(%trigger.getWorldBoxCenter(), 0, 1) SPC getWord(%trigger.getTransform(), 2), %add);
               //echo("Center spawn at" SPC %trigger);
               //echo("WBC is" SPC %trigger.getWorldBoxCenter());
               //echo("Vec is" SPC %spawnVec);
            }

            %rotation = getWords(%trigger.getTransform(), 3, 6);
            %pitch = 0.45;
            if (isObject(SpawnedSet)) {
               %gem = %this.getNearestGem();
               %playerPos = %spawnVec;

               %gemPos = %gem.getPosition();
               %difference = VectorSub(%gemPos, %playerPos);
               %angle = mAtan(getWord(%difference, 0), getWord(%difference, 1));
               %rotation = "0 0 1" SPC %angle;

               %hypo = VectorLen(%difference);
               %difference = setWord(%difference, 2, getWord(%difference, 2) - (%hypo / 2));
               %pitch = -mAsin((getWord(%difference, 2) / %hypo) * 0.7);
            }
            return %spawnVec SPC %rotation TAB (%trigger.g ? getWords(%trigger.getTransform(), 3, 6) : "1 0 0 3.1415926") TAB %pitch;
         }

         %vector = vectorAdd(%trigger.getTransform(),%add);
         %vector = vectorSub(%vector,%sub);
         return %vector SPC getWords(%trigger.getTransform(), 3, 6) TAB (%trigger.g ? getWords(%trigger.getTransform(), 3, 6) : "1 0 0 3.1415926") TAB 0.45;
      } else //HiGuy: Just die here
         return "0 0 300 1 0 0 0\t1 0 0 3.1415926\t0.45"; // Jeff: If this happens, I will stop coding.
   }

   //echo("DEBUG: direction is " @ %add @ %sub);
   // SPY47 MODIFICATION

   // If no ADD and SUB was specified... respawn normally
   if(%add $= "" && %sub $= "")
      return vectorAdd(%this.checkpointPad.getTransform(),"0 0 3") SPC getWords(%this.checkpointPad.getTransform(), 3) TAB "1 0 0 3.1415926" TAB 0.45;


   %vector = vectorAdd(%this.checkpointPad.getTransform(),%add);
   %vector = vectorSub(%vector,%sub);
   //return vectorAdd(%this.checkPoint.pad.getTransform(),"0 0 3") SPC getWords(%this.checkPoint.pad.getTransform(), 3);
   return %vector SPC getWords(%this.checkpointPad.getTransform(), 3) TAB "1 0 0 3.1415926" TAB 0.45;

}

function GameConnection::getCheckpointDTS(%this, %group, %respawnPoint)
{
   for (%i = 0; %i < %group.getCount(); %i++)
   {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
	  %name = %object.getName();
	  //echo("This object is called " @ %name @ ", but destination is " @ %destination);
         if (%name $= %respawnPoint)
            return %object;
   }
   return -1;
}

function setPowerUpOnCheckpoint(%player)
{
   %player.player.setPowerUp(%player.checkPointPowerUp,true);
}

// SPY47 FUNCTION...
function GameConnection::respawnOnCheckpoint(%this)
{
   //echo("DEBUG: Gravitydir:" SPC %this.checkPoint.gravityDir);

   // Reset the player back to the last checkpoint
   cancel(%this.respawnSchedule);

   //setGravityDir("1 0 0 0 -1 0 0 0 -1",true);
   endFireWorks();

   %this.isOOB = false;

   %this.player.setOOB(false);
   %this.player.setMode(Normal);

   //echo("DEBUG: Reading direction: " @ %this.checkPoint.direction);
   %pos = %this.getCheckpointPos(0, %this.checkPointAdd, %this.checkPointSub);
   %this.player.setPosition(getField(%pos, 0), 0.45);

   schedule($powerupDelay, 0, "setPowerUpOnCheckpoint", %this);
   //%this.player.setPowerUp(%this.checkPoint.powerUp,true);

   %this.gemCount = %this.checkPointGemCount;
   %this.penaltyTime = %this.checkPointPenaltyTime;
   %this.bonusTime = %this.checkPointBonusTime;
   %this.respawnGems(MissionGroup); // Spy47 : Respawn gems.
   //echo("DEBUG: Respawn gems");
   //echo("DEBUG: Gravitydir:" SPC %this.checkPoint.gravityDir);

   %this.setGravityDir(%this.checkPointGravityDir, true, %this.checkPointGravityRot); // Spy47 : Set Gravitydir.
   //echo("LOADING GRAVITY: " @ %this.checkPoint.gravityDir);

   // PlayGUI.setTime(%this.checkPoint.time); // Spy47 : keep timer running.
   %this.setGemCount(%this.gemCount);
   %this.play2d(spawnSfx);
   //setGameState("Play"); // Spy47 : Remove onscreen message
   // Jeff: stop the oob message
   cancel(CenterMessageDlg.timer);
   %this.setMessage("");
}

// Spy47 : Respawn gems not in checkpoint.
function GameConnection::respawnGems(%this, %group)
{
   //echo("DEBUG: Starting respawnGems.");
   // Count up all gems out there are in the world
   for (%i = 0; %i < %group.getCount(); %i++)
   {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
      if (%type $= "SimGroup")
         %this.respawnGems(%object);
      else
         if (%type $= "Item" && %object.getDatablock().classname $= "Gem")
         {
             devecho("This is gem. pickUpCheckpoint:" SPC %object.pickUpCheckpoint SPC "CurCheckpoint:" SPC %this.curCheckpointNum);
             devecho("Owned by" SPC %object.pickUp SPC "%this id is" SPC %this.getId());
             if (%object.pickUpCheckpoint >= %this.curCheckpointNum && %object.pickUp.getId() == %this.getId())
             {
                 //echo("Respawning gem" SPC %object);
                 %object.onMissionReset(); // Spy47 : respawn gem.
                 %object.pickUpCheckpoint = 0;
                 //echo("DEBUG: Gem respawned.");
             }
         }
   }
}
