//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// This script function is called before a client connection
// is accepted.  Returning "" will accept the connection,
// anything else will be sent back as an error to the client.
// All the connect args are passed also to onConnectRequest
//
function GameConnection::onConnectRequest( %client, %netAddress, %name, %number, %password, %marble, %platform, %bolonga )
{
   if (%name $= "" && %number !$= "" && %number != -1) {
      devecho("Ping request from" SPC %netAddress);
      return "";
   }

   if (%client.getAddress() $= "local")
      %password = $MP::ServerPassword = $MPPref::ServerPassword;

   //HiGuy: Included for the lulz, jeff/matan can remove this if they dislike it
   if (%bolonga !$= "bologna")
      devecho("One more connection, saved by bologna!");

   return "";
}
//-----------------------------------------------------------------------------
// This script function is the first called on a client accept
//
function GameConnection::onConnect( %client, %name, %number, %password, %marble, %platform, %bologna )
{
   if ($Server::ServerType $= "MultiPlayer" && %client.getAddress() !$= "local") {
      if (%password !$= $MPPref::ServerPassword) {
         %client.schedule(1000, delete, "CHR_PASSWORD");
   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);
         return;
      }

      echo("Connect request from: " @ %netAddress);
      if(getRealPlayerCount() >= $pref::Server::MaxPlayers + 1) {
         %client.schedule(1000, delete, "CR_SERVERFULL");
   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);
         return;
      }

      // Jeff: if you are on a mac, and connecting to a pc/linux server or
      // vice-versa.  What the hell you doing. GET OUT OF HERE.
      if (!compareOS(%platform)) {
         if ($Server::Dedicated && $Server::AltPort)
            commandToClient(%client, 'AltPort', $Server::AltPort);

         %client.schedule(1000, delete, "CHR_INVALID_PLATFORM");
   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);
         return;
      }

      tryCreateBanList();
      if (BanList.containsEntryAtField(%name, 0)) {
         %client.schedule(1000, delete, "CR_YOUAREBANNED");
   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);
         return;
      }
      if (BanList.containsEntryAtField(%client.getAddress(), 1)) {
         %client.schedule(1000, delete, "CR_YOUAREBANNED");
   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);
         return;
      }
   }

   %client.connected = true;

   if (%name $= "" && %number !$= "" && %number != -1) {
      //HiGuy: We have to keep them out of ClientGroup, so they don't mess up
      //       our counters
      if (!isObject(PingGroup)) RootGroup.add(new SimGroup(PingGroup));
      PingGroup.add(%client);

      //HiGuy: Set a couple variables on the client
      %client.number = %number;
      %client.pinging = true;

      //HiGuy: Schedule these events
      %client.schedule(1000, "sendPing");
      %client.schedule(2000, "delete", "PING");

      //HiGuy: Don't continue with this method, as they are just pinging.
      return;
   }

   // if hosting this server, set this client to superAdmin
   if (%client.getAddress() $= "local") {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
   }

   //HiGuy: Store these, but we need to check their CRC before they can connect
   %client._name     = %name;
   %client._number   = %number;
   %client._password = %password;
   %client._marble   = %marble;
   %client._bologna  = %bologna;
   %client._platform = %platform; // Jeff: used for compatibility

   //HiGuy: Check leaderboards/multiplayer/serverCmds and clientCmds for CRC
   // checking. Singleplayer folks get easy free access, as we don't want to
   // screw that up!
   if ($Server::ServerType $= "MultiPlayer")
      %client.validateCRC();
   else
      %client.finishConnect();
}

function GameConnection::finishConnect( %client )
{
   //HiGuy: I don't even
   if (%this.verified) {
      error("Dongers from somewhere");
      return;
   }

   //HiGuy: Grab these back
   %name     = %client._name;

   //echo("Name is" SPC %name);
   %number   = %client._number;
   %password = %client._password;
   %marble   = %client._marble;
   %bologna  = %client._bologna;
   %platform = %client._platform;

   //HiGuy: We don't need these silly fields anymore
   %client._name     = "";
   %client._number   = "";
   %client._password = "";
   %client._marble   = "";
   %client._bologna  = "";
   %client._platform = "";

   // Send down the connection error info, the client is
   // responsible for displaying this message if a connection
   // error occures.
   messageClient(%client,'MsgConnectionError',"",$Pref::Server::ConnectionError);

   // Send mission information to the client
   sendLoadInfoToClient( %client );

   // Simulated client lag for testing...
   // %client.setSimulatedNetParams(0.1, 30);

   // Get the client's unique id:
   // %authInfo = %client.getAuthInfo();
   // %client.guid = getField( %authInfo, 3 );
   %client.guid = 0;
   addToServerGuidList( %client.guid );

   if ($MP::TeamMode) {
      //HiGuy: Add them to the default team, or create it if none exists
      %defaultTeam = Team::createDefaultTeam();
      Team::addPlayer(%defaultTeam, %client);
   }

   // Set admin status
   %client.isAdmin = false;
   %client.isSuperAdmin = false;

   // if hosting this server, set this client to superAdmin
   if (%client.getAddress() $= "local") {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
   }

   //HiGuy: Custom player skins
   %client.skinChoice = %marble;

   // Jeff: used for operating system compatibility
   %client.platform = %platform;

   // Jeff: send the client the server platform for compatibility
   commandToClient(%client, 'GetServerPlatform', $platform);

   // Jeff: used for ghosting objects, stores which objects are ghosted
   %client.ghostManager = new SimSet();

   // Save client preferences on the connection object for later use.
//   %client.gender = "Male";
//   %client.armor = "Light";
//   %client.race = "Human";
//   %client.skin = addTaggedString( "base" );
//   %client.score = 0;

   if (isReturningName(%name)) {
      //HiGuy: THOUGHT YOU COULD LEAVE, DID YOU? WELL SCREW THAT. HAVE YOUR
      // POINTS BACK.

      //echo("Is returning:" SPC %name);

      %client.restore(%name);

      %client.restored = true;

      //commandToClient(%client, '_', "d();");
      //kick(%client);
      //return;
   } else {
      %client.setPlayerName(%name);

      // Jeff: add the index to actually make MP WORK
      $Game::ClientIndex += 0;
      %client.index = $Game::ClientIndex;
      $Game::ClientIndex ++;
   }
   messageClient(%client, 'MsgClientIndex', "", %client.index);

   $instantGroup = ServerGroup;
   $instantGroup = MissionCleanup;
   echo("CADD: " @ %client @ " " @ %client.getAddress());

   // Inform the client of all the other clients
   %count = ClientGroup.getCount();
   for (%cl = 0; %cl < %count; %cl++) {
      %other = ClientGroup.getObject(%cl);
      if ((%other != %client)) {
         // These should be "silent" versions of these messages...
         messageClient(%client, 'MsgClientJoin', "",
               "",
               %other,
               %other.sendGuid,
               %other.score,
               %other.isAIControlled(),
               %other.isAdmin,
               %other.isSuperAdmin);
         commandToClient(%client, 'GhostIndex', %other.scale, %other.index);
      }
   }

   commandToClient(%client, 'ServerSetHandicaps', serverGetHandicaps());
   commandToClient(%client, 'SetHandicaps', serverGetHandicaps());

   // Inform the client we've joined up
   messageClient(%client,
      'MsgClientJoin', "",
      "",
      %client,
      %client.sendGuid,
      %client.score,
      %client.isAiControlled(),
      %client.isAdmin,
      %client.isSuperAdmin);

   // Inform all the other clients of the new guy
   messageAllExcept(%client, -1, 'MsgClientJoin', "",
      "",
      %client,
      %client.sendGuid,
      %client.score,
      %client.isAiControlled(),
      %client.isAdmin,
      %client.isSuperAdmin);

	if ($Server::ServerType $= "Multiplayer") {
		messageAllExcept(%client, -1, 'MsgClientJoin', '\c1%1 has joined the game.', LBResolveName(%client.namebase), %client);

		commandToAllExcept(%client, 'PrivateMessage', LBChatColor("notification") @ LBResolveName(%client.nameBase) SPC "has joined the game.");

		%client.sendSettings();
		%client.setHandicaps(serverGetHandicaps());
	}

	if ($Server::Dedicated && $Server::Controllable) {
		if (%client.isHost() || getRealPlayerCount() < 2) {
			 //You're the host
			 %client.setHost(true);
		}
	}
	commandToClient(%client, 'Dedicated', $Server::Dedicated);

   if ($Server::ServerType $= "MultiPlayer" && $Server::Lobby && !$missionRunning)
      %client.loadLobby();
   else {
      %client.loadMission();

      //HiGuy: Set the loading state
      %client.loading = true;
      updatePlayerlist(); //HiGuy: Update the user list
   }

   if ($Server::ServerType $= "MultiPlayer") {
      masterHeartbeat();
      commandToClient(%client, 'GameStatus', $Editor::Opened);
   }
}

//-----------------------------------------------------------------------------
// A player's name could be obtained from the auth server, but for
// now we use the one passed from the client.
// %realName = getField( %authInfo, 0 );
//
function GameConnection::setPlayerName(%client,%name)
{
   %client.sendGuid = 0;

   // Minimum length requirements
   %name = stripTrailingSpaces( fixName(%name ) );
   if ( strlen( %name ) < 3 )
      %name = "Guest";

   // Make sure the alias is unique, we'll hit something eventually
   if (!isNameUnique(%name))
   {
      %isUnique = false;
      for (%suffix = 1; !%isUnique; %suffix++)  {
         %nameTry = %name @ "_" @ %suffix;
         %isUnique = isNameUnique(%nameTry);
      }
      %name = %nameTry;
   }

   // Tag the name with the "smurf" color:
   %client.nameBase = %name;
   %client.name = "\cp\c8" @ %name @ "\co";
}

function fixName(%name) {
   %mostlyFormatted = ltrim(stripChars(%name, ",.\'`"));
   while (strPos(%mostlyFormatted, "  ") != -1)
      %mostlyFormatted = strReplace(%mostlyFormatted, "  ", " ");
   while (strPos(%mostlyFormatted, " _") != -1)
      %mostlyFormatted = strReplace(%mostlyFormatted, " _", "_");
   %formatted = stripMLControlChars(%mostlyFormatted);
   return %formatted;
}

function isNameUnique(%name)
{
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %test = ClientGroup.getObject( %i );
      if ( strcmp( %name, %test.nameBase ) == 0 )
         return false;
   }
   return true;
}

function isReturningName(%name)
{
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %test = ClientGroup.getObject( %i );
      if ( strcmp( %name, %test.nameBase ) == 0 && %test.fake )
         return true;
   }
   return false;
}

//-----------------------------------------------------------------------------
// This function is called when a client drops for any reason
//
function GameConnection::onDrop(%client, %reason)
{
   if (%client.pinging) return;
   if (%client.fake) return;
   %client.onClientLeaveGame();

   removeFromServerGuidList( %client.guid );
   if (%client.connected && %client.name !$= "" && $Server::ServerType $= "Multiplayer") {
      messageAllExcept(%client, -1, 'MsgClientDrop', '\c1%1 has left the game.', LBResolveName(%client.namebase), %client);
      commandToAllExcept(%client, 'PrivateMessage', LBChatColor("notification") @ LBResolveName(%client.nameBase) SPC "has left the game.");
   }

   if ($Server::Hosting && %client.address !$= "local") {
      Team::removePlayer(%client.team, %client);
      if ($Server::Preloading)
         %client.preloadDone();
      if ($Server::ServerType $= "MultiPlayer") {
         //HiGuy: NOT SO FAST! IF YOU'RE GOING TO QUIT, YOU'RE GOING TO REGRET
         // IT! (Unless you're Matan and winning. Screw you.)

         //HiGuy: Back up client scores

         //HiGuy: Not worth it if we're at the lobby. I mean, c'mon!
         if (!$Server::Lobby && !$Game::Finished)
            %client.backup();
      }
   }

   echo("CDROP: " @ %client @ " " @ %client.getAddress());

   // Reset the server if everyone has left the game
	if ($Server::Dedicated && $Server::Controllable) {
		schedule(100, 0, "dedicatedUpdate");
	}

   if ($Server::Hosting && %client.address !$= "local") {
      updatePlayerlist();

		schedule(100, 0, masterHeartbeat);
   }
}

//-----------------------------------------------------------------------------

//HiGuy: Gets the count of non-fake players on the server
function getRealPlayerCount() {
   %count = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i ++)
      if (!ClientGroup.getObject(%i).fake)
         %count ++;

   return %count;
}

//HiGuy: Gets the count of all players on the server, real and fake.
function getTotalPlayerCount() {
   return ClientGroup.getCount();
}

//HiGuy: Gets the count of all clients on the server, not including fake
// players, and including pinging and verifying players
function getRealClientCount() {
   return getRealPlayerCount() + PingingGroup.getCount() + HoldGroup.getCount();
}

function isRealClient(%client) {
   return (!%client.fake && isObject(%client) && %client.getClassName() $= "GameConnection");
}

function GameConnection::isReal(%this) {
   return isRealClient(%this);
}

function FakeGameConnection::isReal(%this) {
   return false;
}

function GameConnection::setHost(%this, %host) {
   %this.isSuperAdmin = %host;
   commandToClient(%this, 'HostStatus', %host);

   if ($Server::Dedicated)
	   %this.sendInfo();
}

function GameConnection::isHost(%this) {
   return %this.isSuperAdmin;
}

function FakeGameConnection::isAIControlled(%this) {
   return true; //Makes a lot of things shut up
}

function FakeGameConnection::getPing(%this) {
   return 9999; //Well they won't get your messages anyway...
}

function FakeGameConnection::isGuest(%this) {
   //Redirect
   return GameConnection::isGuest(%this);
}

function FakeGameConnection::getHandicap(%this) {
   //Redirect
   return GameConnection::getHandicap(%this);
}

//-----------------------------------------------------------------------------

function GameConnection::startMission(%this)
{
   // Inform the client the mission starting
   commandToClient(%this, 'MissionStart', $missionSequence);
}


function GameConnection::endMission(%this)
{
   // Inform the client the mission is done
   commandToClient(%this, 'MissionEnd', $missionSequence);
}


//--------------------------------------------------------------------------
// Sync the clock on the client.

function GameConnection::syncClock(%client, %time)
{
   if (%time $= "")
      %time = PlayGui.elapsedTime - %client.getPing();
   commandToClient(%client, 'syncClock', %time);
}


function syncClients() {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++)
      ClientGroup.getObject(%i).syncClock();
}

//--------------------------------------------------------------------------
// Update all the clients with the new score

function GameConnection::incScore(%this,%delta)
{
   %this.score += %delta;
   messageAll('MsgClientScoreChanged', "", %this.score, %this);
}

//--------------------------------------------------------------------------

function GameConnection::sendPing(%this) {
   //HiGuy: Just send them their ping / number
   commandToClient(%this, '_getPing', %this.getPing(), %this.number);
   %this.sendInfo();
}

function GameConnection::sendInfo(%this) {
   if (%this.pinging)
      commandToClient(%this, '_serverInfo', $MPPref::Server::Info, $MPPref::Server::MatanMode TAB $MPPref::Server::GlassMode, %this.number);
}

//--------------------------------------------------------------------------

function GameConnection::backup(%this) {
   //HiGuy: What the fuck?
   if (%this.fake) return;
   if (!$MPPref::BackupClients) return;
   if (!%this.connected) return;
   if (%this.namebase $= "") return;

   ClientGroup.add(%fake = new ScriptObject(FakeConnection) {
      class = "FakeGameConnection";
      nameBase = %this.namebase;
      name = %this.name;
      number = -1;
      pinging = false;
      isAdmin = false;
      isSuperAdmin = false;
      score = 0;
      index = %this.index;
      fake = true;

      skinChoice = %this.skinChoice;
      gemCount = %this.gemCount;
      gemsFound[1] = %this.gemsFound[1];
      gemsFound[2] = %this.gemsFound[2];
      gemsFound[5] = %this.gemsFound[5];

      handicap = %this.getHandicap();
      team = Team::getTeamName(%this.team);
      spectating = %this.spectating;
   });
}

function addFakeClient(%name) {
   ClientGroup.add(%fake = new ScriptObject(FakeConnection) {
      class = "FakeGameConnection";
      nameBase = %name;
      name = %name;
      number = -1;
      pinging = false;
      isAdmin = false;
      isSuperAdmin = false;
      score = 0;
      index = $Game::ClientIndex;
      fake = true;

      gemCount = 0;
   });
}

function GameConnection::restore(%this, %name) {
   //echo("Attempting to restore" SPC %name);
   %count = ClientGroup.getCount();
   for ( %i = 0; %i < %count; %i++ )
   {
      %test = ClientGroup.getObject( %i );
      if ( strcmp( %name, %test.nameBase ) == 0 && %test.fake ) {
         //echo("Actually restoring" SPC %name SPC "from" SPC %test);
         %this.index = %test.index;

         //HiGuy: Restore their stats and prefs
         %this.skinChoice = %test.skinChoice;
         %this.gemCount = %test.gemCount;
         %this.gemsFound[1] = %test.gemsFound[1];
         %this.gemsFound[2] = %test.gemsFound[2];
         %this.gemsFound[5] = %test.gemsFound[5];
         %this.setHandicaps(%test.handicap);
         %this._spectating = %test.spectating;

         //HiGuy: Restore their team
         %team = %test.team;
         if ($MP::TeamMode && (%team = Team::getTeam(%team)) != -1 && !%team.private) {
            Team::addPlayer(%team, %this);
         }

         //HiGuy: Clean up
         %test.delete();

         %this.setPlayerName(%name);

         return;
      }
   }
   return false;
}

function GameConnection::sendSettings(%this) {
   messageClient(%this,
      'MsgServerPrefs', "",
      $MP::BlastRequiredAmount,
      $MP::BlastChargeTime,
      $MPPref::AllowQuickRespawn,
      $MP::BlastPower,
      $MP::BlastRechargePower,
      $MPPref::Server::MatanMode,
      $MPPref::Server::GlassMode,
      $MPPref::Server::Info,
      $MPPref::FastPowerups,
      $MPPref::Server::GemVision);
}

function pruneFakeClients() {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);

      if (%client.fake) {
         %client.delete();
         %i --;
      }
   }
}
