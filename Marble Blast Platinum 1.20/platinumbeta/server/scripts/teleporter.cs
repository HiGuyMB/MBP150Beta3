//-----------------------------------------------------------------------------
// Spy47
datablock TriggerData(DestinationTrigger)
{
	tickPeriodMS = 100;
};

datablock TriggerData(TeleportTrigger)
{
   tickPeriodMS = 100;
};

datablock AudioProfile(TeleportSound)
{
   fileName = "~/data/sound/teleport.wav";
   description = AudioClose3d;
	preload = true;
};

function TeleportTrigger::checkDest(%group, %destination)
{
   for (%i = 0; %i < %group.getCount(); %i++)
   {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
	  %name = %object.getName();
	  //echo("This object is called " @ %name @ ", but destination is " @ %destination);
      //if (%type $= "SimGroup")
      //   return TeleportTrigger::checkDest(%group, %destination);
      //else
         if (%name $= %destination)
            return %object;
   }
   return -1;
}

function teleportPlayer(%player, %obj)
{
   //echo("DEBUG: Object is " @ %obj);
   %destPos = %obj.getPosition();
   %x = getWord(%destPos, 0);
   %y = getWord(%destPos, 1);
   %z = getWord(%destPos, 2);
   %z += 3.0;
   %pos = %x SPC %y SPC %z;
   //echo("DEBUG: Going to " @ %pos);
   serverPlay2d(spawnSfx);
   %player.setTransform(%pos);
}

function TeleportTrigger::onEnterTrigger(%data, %obj, %colObj)
{
   %name = %obj.getName();
   %client = %colObj.client;
   %destination = %obj.destination;
   %delay = %obj.delay;

   // Error handler : If no destination specified...
   if(%destination $= "")
   {
      ASSERT("Error Handler", "There's no destination specified! Please check the .mis file.");
      return;
   }

	if(%delay $= "")
		%delay = 2000;

//   if(!%client)
//   {
//      echo("not a client!");
//      return;
//   }
//   echo("Teleport client:" SPC %client);

   %destination_obj = TeleportTrigger::checkDest(MissionGroup, %destination);
   //echo("It returned " @ %destination_obj);
   if(%destination_obj == -1)
   {
      ASSERT("Error Handler", "checkDest() returned -1. Maybe the specified destination doesn't exist.");
	  return;
   }

   messageClient(%client, 'teleportMsg', "\c0Teleporter has been activated, please wait.");
   $teleSched = schedule(%delay,0,"teleportPlayer",%colObj,%destination_obj);
   $teleSound = serverPlay3D(TeleportSound,%client.player.getTransform());
   %client.player.setCloaked(true);
   
}

function TeleportTrigger::onLeaveTrigger(%data, %obj, %colObj)
{
   %checkname = %obj.getName();
   %client = %colObj.client;
   //echo("TeleportTrigger::onLeaveTrigger called!");
   cancel($teleSched);
   alxStop($teleSound);
   %client.player.setCloaked(false);
}
