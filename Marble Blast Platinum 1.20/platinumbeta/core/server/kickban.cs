//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function kick(%client) {
   //HiGuy: Tell people
   messageAll( 'MsgAdminForce', '\c2The Server Admin has kicked %1.', %client.name);
   commandToAllExcept(%client, 'PrivateMessage', LBChatColor("lagout") @ "The Server Admin has kicked" SPC LBResolveName(%client.nameBase));

   //HiGuy: Kick them
   %client.delete("You have been kicked from this server");

   //HiGuy: Fix the interface
   updatePlayerlist();
   commandToAll('FixGhost');

   MPUserKickDlg.updatePlayerList();
}

function ban(%client) {
   //HiGuy: Tell people
   messageAll('MsgAdminForce', '\c2The Server Admin has banned %1.', %client.name);
   commandToAllExcept(%client, 'PrivateMessage', LBChatColor("lagout") @ "The Server Admin has banned" SPC LBResolveName(%client.nameBase));

   //HiGuy: Add them to the ban list
   if (!%client.isAIControlled() && !%cl.fake) {
      tryCreateBanlist();

      //HiGuy: Add their name and IP (we're going serious here)
      BanList.addEntry(%client.nameBase TAB %client.getAddress());
      saveBanlist();
   }

   //HiGuy: Kick them as well :D
   %client.delete("You have been banned from this server");

   //HiGuy: Pretty some things up
   updatePlayerlist();
   commandToAll('FixGhost');

   MPUserKickDlg.updatePlayerList();
}

function unban(%player) {
   //HiGuy: If you don't have a banlist, why are you unbanning people. Moreover,
   // HOW are you unbanning people?
   tryCreateBanlist();

   //HiGuy: Try to remove them if they're a player
   if (BanList.containsEntryAtField(%player, 0))
      BanList.removeEntryByIndex(Banlist.getIndexByField(%player, 0));

   //HiGuy: Try to remove them if they're an IP
   if (BanList.containsEntryAtField(%player, 1))
      BanList.removeEntryByIndex(Banlist.getIndexByField(%player, 1));

   //HiGuy: Well, we tried :(

   //HiGuy: Pretend it's ok
   MPUserKickDlg.updatePlayerList();
}

function loadBanlist() {
   //HiGuy: Try to load from the file
   %fo = new FileObject();
   if (!%fo.openForRead($MP::BanlistFile)) {
      //HiGuy: No ban list, let's just create a new one
      error("Could not open ban list - creating a new one!");

      //HiGuy: Reset this if it exists
      createDefaultBanlist();

      //HiGuy: Clean up
      %fo.close();
      %fo.delete();
      return;
   }

   //HiGuy: Reset this if it exists
   createDefaultBanlist();

   //HiGuy: Read in the banlist, adding the entries
   while (!%fo.isEOF()) {
      %line = %fo.readLine();
      BanList.addEntry(%line);
   }

   //HiGuy: Clean up
   %fo.close();
   %fo.delete();
}

function saveBanlist() {
   //HiGuy: If we have no banlist, why are we saving it? I don't know. Just
   // make one exist so we don't throw an error.
   tryCreateBanlist();

   //HiGuy: Try to open the file
   %fo = new FileObject();
   if (!%fo.openForWrite($MP::BanlistFile)) {
      //HiGuy: If this happens, you won't be able to save your bans :(
      error("Could not write to ban list file! Banlist will not be saved!");

      //HiGuy: Clean up
      %fo.close();
      %fo.delete();
      return;
   }

   //HiGuy: Write out each ban
   for (%i = 0; %i < BanList.getSize(); %i ++) {
      %fo.writeLine(BanList.getEntry(%i));
   }

   //HiGuy: Clean up
   %fo.close();
   %fo.delete();
}

function tryCreateBanlist() {
   if (!isObject(BanList))
      createDefaultBanlist();
}

function createDefaultBanlist() {
   //HiGuy: Delete the old one
   if (isObject(BanList))
      BanList.delete();
   Array(BanList);

   //HiGuy: Add game-wide global bans here
   // BanList.addEntry("BlazerYO");
   // BanList.addEntry("Luke Skywalker");
   // BanList.addEntry("Robot Marble");
   // BanList.addEntry("Tech Geek");
   // BanList.addEntry("Matan Weissman");
   // BanList.addEntry("Matthieu Parizeau");
   // BanList.addEntry("Trace");
   // BanList.addEntry("Anyone that's not HiGuy :D");
   // BanList.addEntry("*");
   // BanList.addEntry("x.x.x.x"); // historical note: this used to be matan's ip
   // BanList.addEntry("0.0.0.0/16");
   // BanList.addEntry("127.0.0.1");
   // BanList.addEntry("192.168.*.*");
   // BanList.addEntry("10.0.*.*");
   // BanList.addEntry("25.*.*.*");
   // BanList.addEntry("8.8.4.4");

   // Damn I can't wait for someone to find this list in CURRENT_YEAR
   // Hello person, looking at the MBP source code! What do you think?
   // Did you know that this entire feature doesn't work and never did?
}
