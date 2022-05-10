//-----------------------------------------------------------------------------
// clientCompatibility.cs
//
// Copyright (c) 2013 The Platinum Team
//
// Jeff: Allows cross compatible Multiplayer between windows/mac/linux
//       SUCK IT PLEBS WHO DIDN'T THINK THIS WAS POSSIBLE, SUCK IT
//       As shadowmarble said, "mac [users of a certain orientation]" they'll be happy now <3
//-----------------------------------------------------------------------------

function clientCmdReceiveSequence(%seq, %datas)
{
   $clientSequence = %seq;
   $DatablockCount = %datas;
   $ReceivedDataBlocks = 0;

   onMissionDownloadPhase1();
}

// Jeff: when a new datablock is being sent across the wire
function clientCmdReceivingNewDataBlock(%seq)
{
   if (%seq != $clientSequence)
   {
      error("Data transmission sequence failed.");
      return;
   }
   $ReceivedDataBlock = "";
}

// Jeff: receiving a datablock chunk
function clientCmdReceiveDataBlockChunk(%data, %seq)
{
   if (%seq != $clientSequence)
   {
      error("Data transmission sequence failed.");
      return;
   }
   $ReceivedDataBlock = $ReceivedDataBlock @ %data;
}

// Jeff: the automatic-datablock-creation-machine
function clientCmdReceivedDataBlockDone(%seq)
{
   if (%seq != $clientSequence)
   {
      error("Data transmission sequence failed.");
      return;
   }

   %data = eval("return" SPC $ReceivedDataBlock);
   ServerConnection.add(%data);

   $ReceivedDataBlocks ++;
   onPhase1Progress($ReceivedDataBlocks / $DatablockCount);

}

// Jeff: we have gotten all datablocks!
function clientCmdDataBlocksReceived()
{
   onPhase1Complete();
   commandToServer('ReceivedAllDataBlocks', $clientSequence);
}

// Jeff: parse a method on the sync'd object
function clientCmdGhostMethod(%id, %method, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8)
{
   %obj = nameToId("ServerObject_" @ %id);
   if (!isObject(%obj))
   {
      error("Missing ghost object with server id as:" SPC %id);
      return;
   }

   // Jeff: parse the method!
   // we don't just eval, cuz hackers are evil.  This will help stop that
   // as we are limited, as well as passing in a correct arguement list
   switch$ (%method)
   {
      case "hide":
         %obj.hide(%arg1);
      case "setTransform":
         %obj.setTransform(%arg1);
      case "startFade":
         %obj.startFade(%arg1, %arg2, %arg3);
      case "setDamageState":
         %obj.setDamageState(%arg1);
      case "setSkinName":
         %obj.setSkinName(%arg1);
      case "setDataBlock":
         %obj.setDataBlock(%arg1);
      default:
         warn("Invalid method to parse on object:" SPC %obj.getName());
   }
}

function clientCmdGetServerPlatform(%platform)
{
   $Server::Platform = %platform;
}

// Jeff: send the marble's transform to the server to be updated!
function sendMarbleTransform()
{
   cancel($MP::Schedule::MarbleTransform);
   if (!isObject($MP::MyMarble) && MPgetMyMarble() == -1)
      return;
   commandToServer('SendMarbleTransform', $MP::MyMarble.getTransform());
   $MP::Schedule::MarbleTransform = schedule($MP::ClientTransformUpdate, 0, sendMarbleTransform);
}

//-----------------------------------------------------------------------------
// Jeff: ghosting of objects
//-----------------------------------------------------------------------------

function clientCmdCreateNewGhostObject(%id)
{
   $ReceivedGhost = "";
   $ReceivedGhostId = %id;
}

function clientCmdReceiveGhostChunk(%chunk)
{
   $ReceivedGhost = $ReceivedGhost @ %chunk;
}

function clientCmdGhostReceived()
{
   %obj = eval("return" SPC $ReceivedGhost);
   %obj.setName("ServerObject_" @ $ReceivedGhostId);
   ServerConnection.add(%obj);
}

// Jeff: received all of the current ghost objects upon loading
// of the server for the first time, does not keep calling
// whenever an individual object gets ghosted.
function clientCmdGhostObjectsDone()
{
   commandToServer('GhostsReceivedAck');
}

//-----------------------------------------------------------------------------
// Jeff: misc functions
//-----------------------------------------------------------------------------

function serverHasDiffPlatform()
{
   return ($Server::Platform !$= "") && (!compareOS($Server::Platform));
}

// Jeff: returns the ghosted Id number from a compatibility object
function resolveGhostId(%obj)
{
   %name = %obj.getName();
   %pos = strpos(%name, "_");
   if (%pos == -1)
      return -1;
   return getSubStr(%name, %pos + 1, strLen(%name));
}