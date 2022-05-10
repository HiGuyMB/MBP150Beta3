//-----------------------------------------------------------------------------
// serverCompatibility.cs
//
// Copyright (c) 2013 The Platinum Team
// Portions Copyright (c) 2001 GarageGames.com
//
// Jeff: Allows cross compatible Multiplayer between windows/mac/linux
//       SUCK IT PLEBS WHO DIDN'T THINK THIS WAS POSSIBLE, SUCK IT
//       As shadowmarble said, "mac [users of a certain orientation]" they'll be happy now <3
//
// Jeff: hacky ghosting is hacky - sounds like PQ
// WE HAVE NOW CODED OUR OWN TORQUESCRIPT BASED GHOSTING SYSTEM!!!!!!!!!
// IF YOUR BRAIN MELTS, I AM NOT LIABLE FOR ANY DAMAGES
//HiGuy: Brain melting is classified under "intended behavior"
//-----------------------------------------------------------------------------

if (!isObject(ServerTickerObject))
{
   new SimObject(ServerTickerObject);
   RootGroup.add(ServerTickerObject);
}

// Jeff: called every frame
function ServerTickerObject::onTick(%this)
{
   if (!isObject(ServerGroup))
   {
      %this.setTickable(false);
      return;
   }



   // Jeff: if the server count is different from the last one
   // then we ghost the objects!  Same goes for client count
   // Lets go CPU, don't die now!
   if (%clientCount != $LastClientCount)
   {
      // Jeff: horray for saving CPU by only counting the objects
      // if the clients are different!
      %serverCount = ServerGroup.getObjectCount();
      if (%serverCount != $LastServerCount)
         ghostObjects(%serverCount, %clientCount);
   }
   $LastClientCount = %clientCount;
}

// Jeff: called when a client joins for ghosting and automatically
// ghosts new objects by checking every frame in the function
// above
// passing %count so that we should only have to call
// ServerGroup.getObjectCount() ONLY 1 TIME PER FRAME, PLEASE
// FOR THE LOVE DO NOT CALL IT MORE OFTEN, ITS not speedy
// although I made it optional for those who forget
// or not wish to pass %count c:
function ghostObjects(%count, %clientCount)
{
   %count = %count ? %count : ServerGroup.getObjectCount();
   %clientCount = %clientCount ? %clientCount : ClientGroup.getCount();
   for (%i = 0; %i < %clientCount; %i ++)
   {
      %client = ClientGroup.getObject(%i);

      // Jeff: do not sync to the same OS people, they have the default
      // ghosting system provided by the engine
      // Also, fake clients?  What were you thinking HiGuy....
      // SERIOUSLY, YOU'RE GOING TO ALLOW FAKE CLIENTS IN CLIENT GROUP?
      // NOW YOU MADE MY LIFE HARDER BECAUSE NOW I HAVE TO CHECK FOR
      // CLIENT.FAKE NOW, THANKS A LOT.
      if (%client.fake || compareOS(%client.platform))
         continue;

      ServerGroup.ghostObject(%client);
   }
   $LastServerCount = %count;
}

function serverCmdGhostsReceivedAck(%client)
{
   %client.onGhostAlwaysObjectsReceived();
}

//-----------------------------------------------------------------------------

function serverCmdSendMarbleTransform(%client, %transform)
{
   %client.transform = %transform;
}

// Jeff: get the datablocks and send them one at a time
function GameConnection::sendDataBlocks(%this)
{
   // Jeff: defines the max datablock length per chunk to send
   %maxLength = 250;

   %count = DataBlockGroup.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = DataBlockGroup.getObject(%i);

      //echo("SENDING DATABLOCK TO CLIENT:" SPC %obj.getName());

      // Jeff: if it is an explosion datablock, we must send our own data
      // across the wire without dumping it, that is because inspecting
      // an explosion datablock will cause the game to crash.  This is now
      // seriously a hack for the hacks.
      %str = "";
      if (%obj.getClassName() $= "ExplosionData")
         %str = getExplosionDataBlockString(%obj.getName());
      else
         %str = dumpObject(%obj, 0);

      // Jeff: sanity checking cuz paranoia
      if (%str $= "")
      {
         devecho("Something went wrong in datablock transmission.  There is an empty string...");
         continue;
      }

      // Jeff: send the datablock in pieces because torque remote commands
      // allow a max of 255 chars
      %length = mCeil(strLen(%str) / %maxLength);

      // Jeff: send the information off
      commandToClient(%this, 'ReceivingNewDataBlock', $PreloadSequence);
      for (%j = 0; %j < %length; %j ++)
      {
         %send = getSubStr(%str, %j * %maxLength, %maxLength);
         commandToClient(%this, 'ReceiveDataBlockChunk', %send, $PreloadSequence);
      }
      commandToClient(%this, 'ReceivedDataBlockDone', $PreloadSequence);
   }
   commandToClient(%this, 'DataBlocksReceived');
}

// Jeff: when we receive all datablocks
function serverCmdReceivedAllDataBlocks(%client, %seq)
{
   %client.preloadDataBlocksDone(%seq);
}

// Jeff: returns the datablock string needed, coppied from various files
function getExplosionDataBlockString(%name)
{
   error("getExplosionDataBlockString() :: should not be called");
   return "new ExplosionData(" @ %name @ ") {foo=bar;};";

   // %str = "";
   // switch$ (%name)
   // {
   //    case "LandMineSubExplosion1":
   //       %str = "offset = 1.0; emitter[0] = LandMineSmokeEmitter; emitter[1] = LandMineSparkEmitter;";
   //    case "LandMineSubExplosion2":
   //       %str = "offset = 1.0; emitter[0] = LandMineSmokeEmitter; emitter[1] = LandMineSparkEmitter;";
   //    case "LandMineExplosion":
   //       %str = "soundProfile = ExplodeSfx; lifeTimeMS = 1200; particleEmitter = LandMineEmitter;" SPC
   //              "particleDensity = 80; particleRadius = 1; emitter[0] = LandMineSmokeEmitter;" SPC
   //              "emitter[1] = LandMineSparkEmitter; subExplosion[0] = LandMineSubExplosion1;" SPC
   //              "subExplosion[1] = LandMineSubExplosion2; shakeCamera = true; camShakeFreq = \"10.0 11.0 10.0\";" SPC
   //              "camShakeAmp = \"1.0 1.0 1.0\"; camShakeDuration = 0.5; camShakeRadius = 10.0;" SPC
   //              "impulseRadius = 10; impulseForce = 15; lightStartRadius = 6; lightEndRadius = 3;" SPC
   //              "lightStartColor = \"0.5 0.5 0\"; lightEndColor = \"0 0 0\";";
   //    case "NukeSubBlow1":
   //       %str = "offset = 1.0; emitter[0] = NukeSmokeEmitter; emitter[1] = NukeSparkEmitter;";
   //    case "NukeSubBlow2":
   //       %str = "offset = 1.0; emitter[0] = NukeSmokeEmitter; emitter[1] = NukeSparkEmitter;";
   //    case "NukeExplosion":
   //       %str = "soundProfile = ExplodeSfx; lifeTimeMS = 10000; particleEmitter = NukeEmitter;" SPC
   //              "particleDensity = 120; particleRadius = 3; emitter[0] = NukeSmokeEmitter;" SPC
   //              "emitter[1] = NukeSparkEmitter; subExplosion[0] = NukeSubBlow1; subExplosion[1] = NukeSubBlow2;" SPC
   //              "shakeCamera = true; camShakeFreq = \"10.0 11.0 10.0\"; camShakeAmp = \"1.0 1.0 1.0\";" SPC
   //              "camShakeDuration = 5; camShakeRadius = 50.0; impulseRadius = 10; impulseForce = 100;" SPC
   //              "lightStartRadius = 6; lightEndRadius = 3; lightStartColor = \"0.5 0.5 0\"; lightEndColor = \"0 0 0\";";
   //    case "RedFireWorkSparkExplosion":
   //       %str = "emitter[0] = RedFireWorkSparkEmitter; shakeCamera = false; impulseRadius = 0;" SPC
   //              "lightStartRadius = 0; lightEndRadius = 0;";
   //    case "RedFireWorkExplosion":
   //       %str = "lifeTimeMS = 1200; offset = 0.1; debris = RedFireWork; debrisThetaMin = 0; debrisThetaMax = 90;" SPC
   //              "debrisPhiMin = 0; debrisPhiMax = 360; debrisNum = 10; debrisNumVariance = 2; debrisVelocity = 3;" SPC
   //              "debrisVelocityVariance = 0.5; shakeCamera = false; impulseRadius = 0; lightStartRadius = 0;" SPC
   //              "lightEndRadius = 0;";
   //    case "BlueFireWorkSparkExplosion":
   //       %str = "emitter[0] = BlueFireWorkSparkEmitter; shakeCamera = false; impulseRadius = 0;" SPC
   //              "lightStartRadius = 0; lightEndRadius = 0;";
   //    case "BlueFireWorkExplosion":
   //       %str = "lifeTimeMS = 1200; offset = 0.2; debris = BlueFireWork; debrisThetaMin = 0; debrisThetaMax = 90;" SPC
   //              "debrisPhiMin = 0; debrisPhiMax = 360; debrisNum = 10; debrisNumVariance = 2; debrisVelocity = 3;" SPC
   //              "debrisVelocityVariance = 0.5; shakeCamera = false; impulseRadius = 0; lightStartRadius = 0;" SPC
   //              "lightEndRadius = 0;";
   // }

   // // Jeff: I hope you don't get a big pile of NOPE
   // if (%str $= "" || !isObject(%name))
   //    return "";
   // //echo("new ExplosionData(" @ %name @ ") {" @ %str @ "};");
   // return "new ExplosionData(" @ %name @ ") {" @ %str @ "};";
}

//-----------------------------------------------------------------------------

// Jeff: ghosts an object to a client by instantiating it on the client
// side.  Once the ghost is sent to the specified client, it will never
// ever ever send again!  If the object is a sim group, ghost ALL
// the children. AND I MEAN ALL THE CHILDREN AND EVEN GRANDCHILDREN ect.
function SimObject::ghostObject(%this, %client)
{
   if (%this.getClassName() $= "SimGroup")
   {
      %grpCount = %this.getCount();
      for (%i = 0; %i < %grpCount; %i ++)
         %this.getObject(%i).ghostObject(%client);
      return;
   }

   // Jeff: if we are already ghosted, do not ghost again to the client!
   %count = %client.ghostManager.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      if (%client.ghostManager.getObject(%i) == %this)
         return;
   }

   // Jeff: defines the max datablock length per chunk to send
   %maxLength = 250;

   %str = dumpObject(%this);
   devecho("Ghosting object:" SPC %this.getName() SPC "To Client" SPC %client.nameBase);

   // Jeff: send the datablock in pieces because torque remote commands
   // allow a max of 255 chars
   %length = mCeil(strLen(%str) / %maxLength);

   // Jeff: send the information off
   commandToClient(%client, 'CreateNewGhostObject', %this.getId());
   for (%j = 0; %j < %length; %j ++)
   {
      %send = getSubStr(%str, %j * %maxLength, %maxLength);
      commandToClient(%client, 'ReceiveGhostChunk', %send);
   }
   commandToClient(%client, 'GhostReceived');

   // Jeff: add it to the ghost manager
   %client.ghostManager.add(%this);
}

//-----------------------------------------------------------------------------

function registerMethod(%method, %argc, %package)
{
   %packName = "RenderObject_" @ %method;
   %argList  = sampleArgList(%argc, true);
   // package Foo_bar
   // {
   //    function Foo::bar(%this, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
   //    {
   //       Foo::bar(%this, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
   //       commandToAllDiffOs('GhostMethod', %this.getId(), %method, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
   //    }
   // };
   // activatePackage(Foo_Bar);
   //echo
   //(
   //   (%package ? "package " @ %packName @ " {" : "") @
   //   "function RenderObject::" @ %method @ "(" @ %argList @ ") {" @
   //      "Parent::" @ %method @ "(" @ %argList @ ");" @
   //      "commandToAllDiffOs(\'GhostMethod\', %this.getId()," SPC %method @ "," SPC %argList @ ");" @
   //   "}" @
   //   (%package ? "}; activatePackage(" @ %packName @ ");" : "")
   //);
   eval
   (
      (%package ? "package " @ %packName @ " {" : "") @
      "function RenderObject::" @ %method @ "(" @ %argList @ ") {" @
         "Parent::" @ %method @ "(" @ %argList @ ");" @
         "commandToAllDiffOs(\'GhostMethod\', %this.getId()," SPC %method @ "," SPC %argList @ ");" @
      "}" @
      (%package ? "}; activatePackage(" @ %packName @ ");" : "")
   );
}

// Jeff: thx higuy
// builds a dynamic argument list
function sampleArgList(%length, %this)
{
   if (%this)
      %list = "%this";
   for (%i = 0; %i < %length; %i ++)
      %list = %list @ (%this || %i ? ", " : "") @ "%a" @ %i;
   return %list;
}

// Jeff: IT'S METHOD REGISTRATION TIME WOOOOOOT
// obviously Parent:: calls will error out if you call Sun.hide()
// so use this with risk and logic, don't be stupid plz
registerMethod("setTransform",   1);
registerMethod("hide",           1);
registerMethod("startFade",      3);
registerMethod("setDamageState", 1);
registerMethod("setSkinName",    1);
registerMethod("setDataBlock",   1);
