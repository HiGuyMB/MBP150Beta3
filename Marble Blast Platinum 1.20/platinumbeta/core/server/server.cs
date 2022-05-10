//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------

function portInit(%port)
{
   %failCount = 0;
   while(%failCount < 14 && !setNetPort(%port)) {
      echo("Port init failed on port " @ %port @ " trying next port.");
      %port++; %failCount++;
   }
}

function createServer(%serverType, %mission)
{
   destroyServer();

   //
   $missionSequence = 0;
   $Server::Hosting = true;
   $Server::PlayerCount = 0;
   $Server::ServerType = %serverType;
   $Server::Started = false;

   // Setup for multi-player, the network must have been
   // initialized before now.
   if (%serverType $= "MultiPlayer") {
//      echo("Starting multiplayer mode");

      // Make sure the network port is set to the correct pref.
      portInit($Pref::Server::Port);
      allowConnections(true);

      //HiGuy: Banlists
      loadBanlist();

//HiGuy: Nope
//      if ($pref::Net::DisplayOnMaster !$= "Never" )
//         schedule(0,0,startHeartbeat);
      stopHeartbeat();
   }

   // Load the mission
   $ServerGroup = new SimGroup(ServerGroup);
   onServerCreated();
   initServerCRC();
}


//-----------------------------------------------------------------------------

function destroyServer()
{
   $Server::ServerType = "";
   $Server::Hosting = false;
   $Server::Preloaded = false;
   $Server::Preloading = false;
   $missionRunning = false;
   allowConnections(false);
   stopHeartbeat();

   //Write this
   saveBanlist();
   BanList.delete();

   // Clean up the game scripts
   onServerDestroyed();

   // Delete all the server objects
   while (isObject(MissionGroup))
      MissionGroup.delete();
   while (isObject(MissionCleanup))
      MissionCleanup.delete();
   while (isObject($ServerGroup))
      $ServerGroup.delete();
   while (isObject(MissionInfo))
      MissionInfo.delete();

   // Delete all the connections:
   while (ClientGroup.getCount())
   {
      %client = ClientGroup.getObject(0);
      if (%client.isReal())
         %client.delete("SERVER_CLOSE");
      else
         %client.delete();
   }

   $Server::GuidList = "";

   // Delete all the data blocks...
   deleteDataBlocks();

   // Save any server settings

   // Dump anything we're not using
   purgeResources();

   masterEndGame();
}


//--------------------------------------------------------------------------

function resetServerDefaults()
{
   echo( "Resetting server defaults..." );

   // Override server defaults with prefs:
//   exec( "~/core/defaults.cs" );
//   exec( "~/core/mbpPrefs.cs" );

   loadMission( $Server::MissionFile );
}


//------------------------------------------------------------------------------
// Guid list maintenance functions:
function addToServerGuidList( %guid )
{
   %count = getFieldCount( $Server::GuidList );
   for ( %i = 0; %i < %count; %i++ )
   {
      if ( getField( $Server::GuidList, %i ) == %guid )
         return;
   }

   $Server::GuidList = $Server::GuidList $= "" ? %guid : $Server::GuidList TAB %guid;
}

function removeFromServerGuidList( %guid )
{
   %count = getFieldCount( $Server::GuidList );
   for ( %i = 0; %i < %count; %i++ )
   {
      if ( getField( $Server::GuidList, %i ) == %guid )
      {
         $Server::GuidList = removeField( $Server::GuidList, %i );
         return;
      }
   }

   // Huh, didn't find it.
}


//-----------------------------------------------------------------------------

function onServerInfoQuery()
{
   // When the server is queried for information, the value
   // of this function is returned as the status field of
   // the query packet.  This information is accessible as
   // the ServerInfo::State variable.
   return "Doing Ok";
}

