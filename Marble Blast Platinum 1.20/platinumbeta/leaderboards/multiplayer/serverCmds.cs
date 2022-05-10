//------------------------------------------------------------------------------
// Multiplayer Package
// serverCmds.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

// Jeff: private chat
function serverCmdPrivateChat(%client, %message) {
   // Jeff: don't send it to yourself you fool
   if (%client.checkSpam())
      return;
	if (%message $= "")
		return;
   commandToAllExcept(%client, 'PrivateMessage', %client.nameBase, %message);
}

function serverCmdTeamChat(%client, %message) {
   commandToTeam(%client.team, 'TeamChat', %client.nameBase, Team::getTeamName(%client.team), Team::isTeamLeader(%client.team, %client), %message);
}

function serverCmdReady(%client, %ready) {
   %client.ready = %ready;
   updateReadyUserList();
}

// Jeff: pregame play - start the match
function serverCmdPreGamePlay(%client) {
   // Jeff: make sure ALL clients are ready
   pruneFakeClients();
   if (getRealPlayerCount() != $MP::ReadyCount) {
      commandToClient(%client, 'HostNotReady');
      return;
   }

   if (%client.isHost()) {
      serverStartGame();
   }
}

// Jeff: gets the ghost rotation from the client, labled GR to
//       to save bandwidth
function serverCmdGR(%client,%rot) {
   %client.rotUpdate = %rot;
   %client.assistRotation();
}

// Jeff: gets the mouse button for server sided check clicking
function serverCmdMouseFire(%client, %mousefire) {
   %client.mouseFire = %mousefire;
}

function serverCmdGameEnd(%client) {
   if (ClientGroup.getCount() == 0)
      return;
   if (ClientGroup.getObject(0).getId() == %client.getId())
      endGameSetup();
}

// Jeff: for the respond key
function serverCmdQuickRespawn(%client) {
   // Jeff: saw phil used this, figured why not
   if (!$Game::Finished && $MPPref::AllowQuickRespawn) {
      if (%client.canQuickRespawn) {
         %client.quickRespawnPlayer();
         %client.setQuickRespawnStatus(false);
         %client.schedule($MP::QuickSpawnTimeout, "setQuickRespawnStatus", true);
      }
   }
}

function serverCmdUpdateMarble(%client, %marble) {
   %client.skinChoice = %marble;
   if ($Game::Running && isObject(%client.player)) {
      //HiGuy: Update their player
      %client.updateGhostDatablock();
   }
}

function serverCmdItemCollision(%client, %position, %cid) {
   //HiGuy: They said they collided. Did they really?
   %obj = -1;

   //HiGuy: Grab it from MissionGroup
   for (%i = 0; %i < MissionGroup.getCount(); %i ++) {
      %temp = MissionGroup.getObject(%i);

      // Jeff: only check items!
      if (%temp.getClassName() $= "Item") {
         if (%temp.getPosition() $= %position) {
            %obj = %temp;
            break;
         }
      }
   }

   //HiGuy: Try MissionCleanup too
   if (%obj == -1) {
      for (%i = 0; %i < MissionCleanup.getCount(); %i ++) {
         %temp = MissionCleanup.getObject(%i);
         // Jeff: only check items!
         if (%temp.getClassName() $= "Item") {
            if (%temp.getPosition() $= %position) {
               %obj = %temp;
               break;
            }
         }
      }
   }

   //HiGuy: If it doesn't exist, they didn't collide
   if (%obj == -1) {
      commandToClient(%client, 'NoCollision', %cid);
      return;
   }

   //HiGuy: If it's still showing, they didn't collide either
   if (!%obj.isHidden()) {
      commandToClient(%client, 'NoCollision', %cid);
      return;
   }
}

function serverCmdHandicapFlags(%client, %flags) {
   %client.setHandicaps(%flags);
}

function serverCmdSetHandicaps(%client, %flags) {
   %client.setHandicaps(%flags);
}

function serverCmdSetHandicap(%client, %number, %flag) {
   %client.setHandicap(%number, %flag);
}

//-----------------------------------------------------------------------------
// Jeff: CRC check
//       CRC validation will check to ensure that clients are not cheating.
//       although not totally perfect, it will provide a sufficient amount
//       of protection and will also prevent hacked up servers from being used.
//       Note this will only CRC the .cs.dso files
//
//       Also will check .cs.dso file counts to ensure that we dont have
//       additional scripts

//HiGuy: This method brakes off from onConnect, clients have to pass this check
// in order to finish connecting to the server
function GameConnection::validateCRC(%this) {
   //HiGuy: It's the client's job to send the correct data.
   // The server doesn't care that they might not like using the little extra
   // bit of power to figure out which files to send. It's their job. They have
   // no say in it. If they say no, then they miss out on onConnect.

   //HiGuy: Hold the client in a separate group until they've finished. We don't
   // want them clogging up ClientGroup, now do we?
   if (!isObject(HoldGroup))
      RootGroup.add(new SimGroup(HoldGroup));

   HoldGroup.add(%this);

   //HiGuy: They should know what to send.
   commandToClient(%this, 'CheckCRC');
}

function serverCmdStartCRC(%client) {
   //HiGuy: Oh boy. The client is sending us CRCs. How joyful. </sarcasm>
   // Let's just get this over with and kick 'em if we can!

   %client.checkingCRC = true;
   %client.failedCRC = false;
}

function serverCmdFileCRC(%client, %file, %crc) {
   //HiGuy: Here's a CRC coming in from %client! Let's hope they get it wrong
   // so we can kick them off the server!

   //HiGuy: No point wasting precious server CPU if they've already failed
//   if (%client.failedCRC)
//      return;

    if (strPos(strlwr(%file), "demo.cs.dso") != -1) {
      //HiGuy: We might not have demo.cs
      //Use a precalculated CRC
      if (%crc $= "-451410106") {
         //HiGuy: You passed, jerk
         %client.crcSuccess[%file] = true;
         %client.crcExtra ++;
         return;
      }

      commandToClient(%client, 'CRCError', "demo.cs invalid crc (" @ %crc SPC "!=" SPC "-451410106)");
      devecho("\c2" @ %client.nameBase SPC "demo.cs invalid crc (" @ %crc SPC "!=" SPC "-451410106)");
   }
   if (strPos(strlwr(%file), "ignitiongui.gui.dso") != -1) {
      //HiGuy: We might not have igntionGui.gui
      //Use a precalculated CRC
      if (%crc $= "1742745627") {
         //HiGuy: You passed, jerk
         %client.crcSuccess[%file] = true;
         %client.crcExtra ++;
         return;
      }

      commandToClient(%client, 'CRCError', "ignitiongui.gui invalid crc (" @ %crc SPC "!=" SPC "1742745627)");
      devecho("\c2" @ %client.nameBase SPC "ignitiongui.gui invalid crc (" @ %crc SPC "!=" SPC "1742745627)");
   }
   if (strPos(strlwr(%file), "ignitionstatusgui.gui.dso") != -1) {
      //HiGuy: We might not have ignitionStatusGui.gui
      //Use a precalculated CRC
      if (%crc $= "-1703286268") {
         //HiGuy: You passed, jerk
         %client.crcSuccess[%file] = true;
         %client.crcExtra ++;
         return;
      }

      commandToClient(%client, 'CRCError', "ignitiongui.gui invalid crc (" @ %crc SPC "!=" SPC "1742745627)");
      devecho("\c2" @ %client.nameBase SPC "ignitionstatusgui.gui invalid crc (" @ %crc SPC "!=" SPC "-1703286268)");
   }

   //HiGuy: If they try to send us a file that we don't have, it's probably hax
   // or something we couldn't handle if we tried. Just kick 'em off.
   if (!isFile(%file)) {
      devecho("\c2" @ %client.nameBase SPC "unknown file" SPC %file @ "!");
      //HiGuy: Haha, sucker!
      %client.failedCRC = true;
      return;
   }

   //HiGuy: THOUGHT YOU COULD GET AWAY WITH IT, HUH? NOPE.
   if ($MP::ServerCRC[%file] !$= %crc) {
      devecho("\c2" @ %client.nameBase SPC "invalid file crc" SPC %file SPC "(" @ %crc SPC "!=" SPC $MP::ServerCRC[%file] @ ")");
      //HiGuy: Haha, sucker!
      %client.failedCRC = true;
      return;
   }

   //HiGuy: Hopefully they reach here, as then the file is correct. The next
   // message will come through any moment, let's see how they fare.

   %client.crcSuccess[%file] = true;
}

$CRC_NOPE = true;

function serverCmdFinishCRC(%client, %cFiles) {
   if (%client.failedCRC) {
      devecho("\c2" @ %client._name SPC "failed CRC check!");
      //HiGuy: Hahahahahahahahahaha NO.
      if (!%client.isSuperAdmin) {
         if ($CRC_NOPE) {
            %client.delete("CRC_NOPE");
            return;
         }
      }
   }

   //HiGuy: Ok fine, they've passed SO FAR. Will they pass the final test?
   for (%i = 0; %i < $MP::ServerFiles; %i ++) {
      %file = $MP::ServerFile[%i];
      if (%client.crcSuccess[%file] $= "" && $fileExec[%file] !$= "") {
         devecho("\c2" @ %client._name SPC "missing file" SPC %file @ "!");
         //HiGuy: Caught you! Thought you could get away without that one pesky
         // file that we needed. Get off my server, damned kids.
         if (!%client.isSuperAdmin) {
            if ($CRC_NOPE) {
               %client.delete("CRC_NOPE");
               return;
            }
         }
      }

      %client.crcSuccess[%file] = "";
   }

   %cFiles -= %client.crcExtra;

   //HiGuy: Ok, here's the real test. Did they send the right values for %files?
   if ($MP::ServerFiles != %cFiles) {
      //HiGuy: Well, I guess you get to sit and think about what you just did
      // in the naughty corner of NOPE!
      devecho("\c2" @ %client._name SPC "invalid file count! (" @ %cFiles SPC "!=" SPC $MP::ServerFiles @ ")");
      if (!%client.isSuperAdmin) {
         if ($CRC_NOPE) {
            %client.delete("CRC_NOPE");
            return;
         }
      }
   }

   //HiGuy: HOLY SHIT THEY ACTUALLY PASSED THE CRC CHECK. WHAT ARE THE CHANCES
   // OF THIS ACTUALLY HAPPENING? (oh god I hope it's > 50%)

   devecho("\c3" @ %client._name SPC "passed CRC check");

   //HiGuy: Clear this crap, nobody should need to see it
   %client.failedCRC = "";
   %client.checkingCRC = "";

   //HiGuy: Ok so now they think they're getting in. Right. Uh huh. Prove it.
   commandToClient(%client, 'VerifySession');
}

function initServerCRC() {
   $MP::ServerFiles = 0;
   %exp = "*.dso";
   for (%file = findFirstFile(%exp); %file !$= ""; %file = findNextFile(%exp)) {
      if (strPos(strlwr(%file), "prefs.cs") != -1 || strPos(strlwr(%file), ".svn") != -1 || strPos(strlwr(%file), "config.cs") != -1 || strPos(strlwr(%file), "banlist.cs") != -1 || strPos(strlwr(%file), "demo.cs") != -1 || strPos(strlwr(%file), "ignitiongui.gui") != -1 || strPos(strlwr(%file), "ignitionstatusgui.gui") != -1)
         continue;

      $MP::ServerFile[$MP::ServerFiles] = %file;
      $MP::ServerCRC[%file] = getFileCRC(%file);
      $MP::ServerFiles ++;
   }
}

function serverCmdVerifySession(%client, %session) {
   //HiGuy: So we have their session... now let's verify it with the server
   // if (!masterVerifySession(%client, %session)) {
      %client.completeValidation(true);
   // }
}

function GameConnection::completeValidation(%this, %valid, %message) {
   //HiGuy: I don't even
   if (%this.verified)
      return;

   if (!%valid && !%this.isSuperAdmin) {
      //HiGuy: Bahahahahahaha, you fail!
      devecho("Client" SPC %client._name SPC "should have failed the CRC but I\'m too nice for that.");
      //%this.delete(%message $= "" ? "CRC_NOPE" : %message);
      //return;
   }

   if (%this.fake) {
      %this.delete("CRC_FAKE");
      return;
   }

   commandToClient(%this, 'VerifySuccess');

   //HiGuy: We should know if they're verified
   %this.verified = true;

   //HiGuy: Well, we should probably let them in to the clientGroup
   ClientGroup.add(%this);

   //HiGuy: Let them into the pearly gates of ServerVille
   %this.finishConnect();
}
