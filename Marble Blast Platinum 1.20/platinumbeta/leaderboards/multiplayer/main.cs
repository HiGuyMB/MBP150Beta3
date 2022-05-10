//------------------------------------------------------------------------------
// Multiplayer Package: Iteration 7
// main.cs
// Because this is totally possible, we're totally doing it... AGAIN :D
// Created: 1/1/13 (yay 2013)
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function initMultiplayer() {
   //HiGuy: Gui profiles / etc
   exec("~/leaderboards/core/transparency/transparency.cs");

   //HiGuy: Support functions used by other scripts
   exec("./support.cs");

   //HiGuy: Variables and values
   exec("./defaults.cs");

   //HiGuy: All the client commands
   exec("./clientCmds.cs");

   //HiGuy: All the server commands
   exec("./serverCmds.cs");

   // Jeff: Items that have methods delt client side
   exec("./clientItems.cs");

   //HiGuy: Lobby code for clients
   exec("./clientLobby.cs");

   //HiGuy: Lobby code for servers
   exec("./serverLobby.cs");

   //HiGuy: Game connection commands for sending to clients
   exec("./gameConnection.cs");

   //HiGuy: Ghosting for client-side
   exec("./clientGhost.cs");

   //HiGuy: Master server scripts
   exec("./master.cs");

   //HiGuy: Team support
   exec("./team.cs");

   // Jeff: client particles
   exec("./clientParticles.cs");

   // Jeff: rotation, should be available for both client AND server
   exec("./rotation.cs");

   // Jeff: client spectating
   exec("./clientSpectator.cs");

   //HiGuy: Score list stuff
   exec("./clientScores.cs");

   //HiGuy: WHEEEEEEEEEEEEEEEEEEEEEEEEEEE
   exec("./lolmodes.cs");

   // Jeff: client download of mission files
   exec("./clientDownload.cs");

   // Jeff: compatbility hacks!
   exec("./clientCompatibility.cs");
   exec("./universalCompatibility.cs");

   //HiGuy: Server chat and other things
   exec("./clientChat.cs");

   if (!$Server::Dedicated) {
      //HiGuy: Direct connect dialog
      if ($MP::EnableDirectConnect)
         exec("./MPDirectConnectDlg.gui");

      // Jeff: PRE GAME DIALOG
      exec("./MPPreGameDlg.gui");

      //HiGuy: MP level selector :D
      exec("./MPPlayMissionDlg.gui");

      //HiGuy: Join server GUI
      exec("./MPJoinServerGui.gui");

      //HiGuy: Team select dialog
      exec("./MPTeamSelectDlg.gui");

      //HiGuy: Team join dialog
      exec("./MPTeamJoinDlg.gui");

      //HiGuy: Team create dialog
      exec("./MPTeamCreateDlg.gui");

      //HiGuy: Team options dialog
      exec("./MPTeamOptionsDlg.gui");

      //HiGuy: Team invite dialog
      exec("./MPTeamInviteDlg.gui");

      //HiGuy: Score overlay dialog
      exec("./MPScoresDlg.gui");

      //HiGuy: Server information dialog
      exec("./MPServerDlg.gui");

      //HiGuy: End game screen
      exec("./MPEndGameDlg.gui");

      //HiGuy: Achievement dialog
      exec("./MPAchievementsDlg.gui");

      //HiGuy: Handicaps and things
      exec("./MPPlayerSettingsDlg.gui");

      //HiGuy: Searching
      exec("./MPSearchGui.gui");

      //HiGuy: Marble Select
      exec("./MPMarbleSelectionDlg.gui");

      //HiGuy: Kick Screen
      exec("./MPUserKickDlg.gui");

      //HiGuy: Spawn Save Screen
      exec("./MPSaveSpawnsDlg.gui");

      //HiGuy: Exit Game Screen
      exec("./MPExitGameDlg.gui");
   }
}

function initMultiplayerServer() {
   //HiGuy: Ghosting for server-side
   exec("./serverGhost.cs");

   // Jeff: main server functions
   exec("./server.cs");

   // Jeff: server particles
   exec("./serverParticles.cs");

   //HiGuy: Hunt scripts with gem spawning and the such
   exec("./huntGems.cs");

   //HiGuy: Blast
   exec("./blast.cs");

   //HiGuy: Collision scripts
   exec("./collision.cs");

   // Jeff: rotation, should be available for both client AND server
   exec("./rotation.cs");

   // Jeff: server spectating
   exec("./serverSpectator.cs");

   // Matan: Skies
   exec("./skies.cs");

   // Jeff: compatbility hacks!
   exec("./serverCompatibility.cs");
   exec("./universalCompatibility.cs");

   // Jeff: server download of mission files
   exec("./serverDownload.cs");
}

//-----------------------------------------------------------------------------

function initDedicatedServer() {
	//HiGuy: WHOOOO
	//HiGuy: So we need to replicate an actual server starting up

	//HiGuy: Dedicated things
	exec("./dcon.cs");
	exec("./dsupport.cs");

	//HiGuy: Set the game
	$CurrentGame = "Multiplayer";

	//HiGuy: Server variables
	$Server::Lobby = true;
	$Server::Hosting = true;
	$Server::Preloaded = false;
	$Server::Preloading = false;

	if ($MPPref::Server::Name $= "")
		$MPPref::Server::Name = "Unnamed Dedicated Server";

	//HiGuy: Initialize the connection
	allowConnections(true);
	portInit(28000);

	//HiGuy: Create the server
	createServer("MultiPlayer");

	//HiGuy: Display it
	masterStartGame();

	//HiGuy: Start up our io
	schedule(1000, 0, loadInput);
	schedule(1000, 0, printStatus);
}
