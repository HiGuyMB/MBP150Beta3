//-----------------------------------------------------------------------------
// main.cs
// Copyright (c) The Platinum Team
// Mainly written by Jeff
// Jeff: this file holds core material for the leaderboards
//
// Leaderboard Variable namespace: $LB::*
// Pref Leaderboard Variable namespace: $LBPref::*
//-----------------------------------------------------------------------------

// Jeff: core function for loading leaderboards
function initLeaderboards() {
   // Jeff: delete old leaderboard stuff before making a new one
   closeLeaderboards();

   disconnect(true);

   exec("./LBMessageGui.gui");
   LBMessage("Loading Marble Blast Platinum Online...");
   Canvas.repaint();

   devecho("Initilizing leaderboards in Marble Blast Platinum 1.50 Beta 3.");
   initLBConstants(); // Jeff: grab core constants

   // Jeff: Guis
   exec("./LBLoginGui.gui");
   exec("./LBChatGui.gui");
   exec("./LBGuestLoginGui.gui");

   // Jeff: dialogs
   exec("./LBPlayMissionDlg.gui");
   exec("./LBErrorHandlerDlg.gui");
   exec("./LBMarbleSelectionDlg.gui");
   exec("./LBSuperChallengeDlg.gui");
   exec("./LBStatsDlg.gui");
   exec("./LBRegisterDlg.gui");
   exec("./LBTotalDlg.gui");
   exec("./LBGeneralDlg.gui");
   exec("./LBChallengeDlg.gui");
   exec("./LBReportDlg.gui");
   exec("./LBSCAchievementsDlg.gui");
   exec("./LBUserDlg.gui");
   exec("./LBChallengeEndDlg.gui");
   exec("./LBMessageHudDlg.gui");
   exec("./LBAchievementsDlg.gui");

   // Jeff: scripts
   exec("./score.cs");
   exec("./socket.cs");
//   exec("./pauseGame.cs");

   //HiGuy: Multiplayer scripts/guis/support
   exec("./multiplayer/main.cs");

   %letters = "a"@"b"@"c"@"d"@"e"@"f"@"g"@"h"@"i"@"j"@"k"@"l"@"m"@"n"@"o"@"p"@"q"@"r"@"s"@"t"@"u"@"v"@"w"@"x"@"y"@"z"@"A"@"B"@"C"@"D"@"E"@"F"@"G"@"H"@"I"@"J"@"K"@"L"@"M"@"N"@"O"@"P"@"Q"@"R"@"S"@"T"@"U"@"V"@"W"@"X"@"Y"@"Z"@"1"@"2"@"3"@"4"@"5"@"6"@"7"@"8"@"9"@"0"@"."@"-"@"\'"@"\""@"["@"]"@","@"("@")"@"!"@"$"@"&"@"^"@"#"@"@"@"+"@"="@"-"@"_"@";";
   // %crcdata = call("q"@"u"@"e"@"r"@"y"@"C"@"R"@"C");

   // if (getField(%crcdata, 0) != 0 && getField(%crcdata, 1) !$= "c"@"r"@"c"@"g"@"o"@"o"@"d") {
   //    //Blow up here
   //    exec("./LBErrorHandlerDlg.gui");
   //    LBAssert("Redundancy Error!", "It appears one or more game files have been modified. Marble Blast Platinum Online requires that no modifications have been made to the game files. If this was done either by you or a virus, please re-download Marble Blast Platinum at <a:marbleblast.com>marbleblast.com</a>. Thank you.\n\nAffected file:" SPC getField(%crcData, 1), "closeLeaderboards(); Canvas.setContent(MainMenuGui);");
   //    alxPlay(LBNope);
   //    return;
   // }
   //HiGuy: Doesn't work ingame... weird
   MPMarbleSelectionDlg.checkMarbles();

   // Jeff: now show the login gui, as everything has loaded
   Canvas.setContent(LBLoginGui);

   //HiGuy: Used for quickie reference. Do *not* rely on this!
   $LB = 1;
   $LB::JustLoggedIn = false;
}

// Jeff: leaderboard constants init
function initLBConstants() {
   // Jeff: core constants declaration
//   $LB::servers[0]     = "higuysmb.heliohost.org:80"; //Unofficial
//   $LB::servers[0]     = "mbforums.innacurate.com:80"; //Official
   $LB::servers[0]     = "marbleblast.com:80"; //New Official
//	  $LB::Servers[0] = "localhost:3791";
   $LB::serverPaths[0] = "/leader/";

   //HiGuy: Default Heartbeats
   $LB::ChatHeartbeatTime = 5000;
   $LB::UserListHeartbeatTime = 15000;

   //HiGuy: Slower for in-game and less lag
   $LB::SlowChatHeartbeatTime = 10000;
   $LB::SlowUserListHeartbeatTime = 25000;

   //HiGuy: These have defaults as well
   $LB::ChatStart = 0;
   $LB::NotifyStart = 0;
   $LB::Access = 0;
   $LB::TotalRating = 0;
   $LB::WelcomeMessage = "<spush><color:ff0000><lmargin:2>An error has occurred.<spop>";

   //HiGuy: The server you use is decided in your prefs. 0 - 3 where 0 is most
   //       desirable and 3 is break-neck nothings-left-to-try-so-oh-well
   $LB::server = $LB::servers[$LBPref::Server];
   $LB::serverPath = $LB::serverPaths[$LBPref::Server];

   // The server on which the master server's PHP files are hosted
   $Master::Server = $LB::server;
   $Master::Path = $LB::serverPath @ "MP_Master/";

   // Jeff: do this now so that the leaderboard's
   //       marble selection don't spazz out
   if ($LBPref::Marble $= "")
      $LBPref::Marble = "0";

   //HiGuy: Slide through each image every 2.5? sec
   //HiGuy: This doesn't act like I wanted it to, but I have no clue how to fix it. Screw this.
   $LB::SuperChallengeSlideshowTime = 5;

   //HiGuy: Disable cheats!
   $testCheats = 0;
   $enableEditor = 0;
   $qualifyAllLevels = 0;

   //HiGuy: This should be init'd at 0
   $LB::Schedules = 0;

   //HiGuy: Activate this here
   activatePackage(Tickable);
}

// Jeff: core function for closing the leaderboards
//       deletes leaderboard variables at the end
function closeLeaderboards() {
   if ($Server::Hosting) {
      //HiGuy: You're screwed!
      masterEndGame();
      disconnect();
   }

   savePrefs(true);


   // Jeff: while loop through all Gui/Dlg to ensure all are deleted
   while (isObject(LBLoginGui))
      LBLoginGui.delete();
   while (isObject(LBChatGui))
      LBChatGui.delete();
   while (isObject(LBMessageGui))
      LBMessageGui.delete();
   while (isObject(LBPlayMissionDlg))
      LbPlayMissionDlg.delete();
   while (isObject(LBGeneralDlg))
      LBGeneralDlg.delete();
   while (isObject(LBSuperChallengeDlg))
      LBSuperChallengeDlg.delete();
   while (isObject(LBRegisterDlg))
      LBRegisterDlg.delete();
   while (isObject(LBMarbleSelectionDlg))
      LBMarbleSelectionDlg.delete();
   while (isObject(LBTotalDlg))
      LBTotalDlg.delete();
   while (isObject(LBServersDlg))
      LBServersDlg.delete();
   while (isObject(LBStatsDlg))
      LBStatsDlg.delete();
   while (isObject(LBChallengeDlg))
      LBChallengeDlg.delete();
   while (isObject(LBAchievementsDlg))
      LBAchievementsDlg.delete();
   while (isObject(LBSuperChallengeDlg))
      LBSuperChallengeDlg.delete();
   while (isObject(LBSCAchievementsDlg))
      LBSCAchievementsDlg.delete();
   while (isObject(LBReportDlg))
      LBReportDlg.delete();
   while (isObject(LBGuestLoginGui))
      LBGuestLoginGui.delete();
   while (isObject(LBChallengeEndDlg))
      LBChallengeEndDlg.delete();
   while (isObject(LBUserDlg))
	  LBUserDlg.delete();
	while (isObject(LBSCRestoreDlg))
	   LBSCRestoreDlg.delete();
	while (isObject(LBSCSaveDlg))
	   LBSCSaveDlg.delete();
   if (isObject(LBErrorHandlerDlg) && LBErrorHandlerDlg.isAwake())
      LBErrorOkHandlerButton.command = "while (isObject(LBErrorHandlerDlg))LBErrorHandlerDlg.delete();";
   else {
   	while (isObject(LBErrorHandlerDlg))
      	LBErrorHandlerDlg.delete();
	}

   // Jeff: for loop through tcp group and delete them
   //       this prevents the game from crashing when you exit
   %count = TCPGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      //HiGuy: We don't want to interfere with other things
      if (getSubStr(TCPGroup.getObject(%i).getName(), 0, 2) $= "LB" && TCPGroup.getObject(%i).getName() !$= "LBNetwork")
         TCPGroup.getObject(%i).destroy();
   }

   // Jeff: additional stuff to delete
   while (isObject(LBPlayMissionGroup))
      LBPlayMissionGroup.delete();
   while (isObject(LBMarbleList))
      LBMarbleList.delete();
   while (isObject(LBSuperChallengeGroup))
      LBSuperChallengeGroup.delete();
   deleteVariables("$LB::*");

   $LB = 0;

   //HiGuy: Oh my god shut up
   deactivatePackage(Tickable);
}

$LBGameSess = strRand(64);

//Multiplayer support
exec("./multiplayer/main.cs");
exec("./support.cs");

//Game support
schedule(1, 0, initMultiplayer);


function LB_OH_NO() {
   MessageBoxOk("Bad User, no Cookie", "If you\'ve managed to trigger this then there\'s a serious problem. Please tell us about this on our forums. :)", "gotoWebPage(\"http://marbleblast.com\");", true);
}
