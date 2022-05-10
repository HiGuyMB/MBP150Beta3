//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

// Functions dealing with connecting to a server


//-----------------------------------------------------------------------------
// Server connection error
//-----------------------------------------------------------------------------

addMessageCallback( 'MsgConnectionError', handleConnectionErrorMessage );

function handleConnectionErrorMessage(%msgType, %msgString, %msgError)
{
   // On connect the server transmits a message to display if there
   // are any problems with the connection.  Most connection errors
   // are game version differences, so hopefully the server message
   // will tell us where to get the latest version of the game.
   $ServerConnectionErrorMessage = %msgError;
}


//----------------------------------------------------------------------------
// GameConnection client callbacks
//----------------------------------------------------------------------------

function GameConnection::initialControlSet(%this)
{
   if (%this.pinging) return;

   echo ("*** Initial Control Object");

   // The first control object has been set by the server
   // and we are now ready to go.

   // first check if the editor is active
   if (!Editor::checkActiveLoadDone())
   {
      MPadjustFrictions();
      EndGameGui.resetBitmap();
      if (Canvas.getContent() != PlayGui.getId())
         Canvas.setContent(PlayGui);
   }
}

function GameConnection::setLagIcon(%this, %state)
{
   if (%this.pinging) return;

   if (%this.getAddress() $= "local")
      return;
   LagIcon.setVisible(%state $= "true");
}

function GameConnection::onConnectionAccepted(%this)
{
   if (%this.pinging) return PingConnection::onConnectionAccepted(%this, %msg);

   // Called on the new connection object after connect() succeeds.
   LagIcon.setVisible(false);
}

function GameConnection::onConnectionTimedOut(%this)
{
   if (%this.pinging) return PingConnection::onConnectionTimedOut(%this, %msg);

   // Called when an established connection times out
   if ($LB::loggedIn && $LB::SuperChallengeMode)
      LBSCD_SendError();
   disconnectedCleanup($Server::Reconnecting);
   if (!$Server::Reconnecting)
	   MessageBoxOK( "TIMED OUT", "The server connection has timed out.", "", true);
}

function GameConnection::onConnectionDropped(%this, %msg)
{
   if (%this.pinging) return PingConnection::onConnectionDropped(%this, %msg);
   switch$(%msg)
   {
      case "CR_INVALID_PROTOCOL_VERSION":
         %error = "Incompatible protocol version: Your game version is not compatible with this server.";
      case "CR_INVALID_CONNECT_PACKET":
         %error = "Internal Error: badly formed network packet";
      case "CR_YOUAREBANNED":
         %error = "You are banned from this server.";
      case "CR_SERVERFULL":
         %error = "This server is full.";
      case "CHR_PASSWORD":
         // XXX Should put up a password-entry dialog.
         Canvas.setContent(LBChatGui);
         Canvas.pushDialog(MPJoinServerGui);
         MPJoinServerGui.pushPassword(MPJoinServerGui.joinIP, MPJoinServerGui.joinPort, $MP::ServerPassword !$= "");
         $MP::ServerPassword = "";
         MPJoinServerGui.joining = false;

         disconnectedCleanup(true);
         return;
      case "CHR_PROTOCOL":
         %error = "Incompatible protocol version: Your game version is not compatible with this server.";
      case "CHR_CLASSCRC":
         %error = "Incompatible game classes: Your game version is not compatible with this server.";
      case "CHR_INVALID_CHALLENGE_PACKET":
         %error = "Internal Error: Invalid server response packet";
      case "CHR_INVALID_PLATFORM": // Jeff: mac/pc trying to play together? haha.  Yeah...right.
         %error = "The current server is not compatible with your computer platform.";
      case "SERVER_CLOSE":
         %error = "The server was closed.";
      case "CRC_FAKE":
         %error = "Your leaderboards session was invalid. Please log out of the leaderboards and log in again.";
      case "CRC_NOPE":
         %error = "Incompatible game scripts: Your game version is not compatible with this server.";
         MessageBoxOk("Invalid Files!", "You or the host have invalid or modified script files. Please check your console.log file, and send it to marbleblastforums@gmail.com. If this problem persists, either you or the host may need to reinstall Marble Blast Platinum.\n\nThank you,\n~The Marble Blast Platinum Team", "gotoWebPage(\"mailto://marbleblastforums@gmail.com?subject=1.50%20Multiplayer%20CRC%20Errors&body=Please%20attach%20your%20console.log%20(find%20it%20at%20" @ expandEscape(strreplace(strreplace(getFullPath("console.log"), "&", "<and>"), " ", "%20")) @ ").\");", true);
      case "":
         %error = "Connection timed out.";
      default:
         %error = %msg;
   }
   // Established connection was dropped by the server
   if ($LB::loggedIn && $LB::SuperChallengeMode)
      LBSCD_SendError();
   disconnectedCleanup($Server::Reconnecting);
   if (!$Server::Reconnecting)
	   MessageBoxOK( "DISCONNECT", "The server has dropped the connection:" NL %error, "", true);
}

function GameConnection::onConnectionError(%this, %msg)
{
   if (%this.pinging) return PingConnection::onConnectionError(%this, %msg);

   // General connection error, usually raised by ghosted objects
   // initialization problems, such as missing files.  We'll display
   // the server's connection error message.
   if ($LB::loggedIn && $LB::SuperChallengeMode)
      LBSCD_SendError();
   disconnectedCleanup($Server::Reconnecting);
   if (!$Server::Reconnecting)
	   MessageBoxOK( "DISCONNECT", $ServerConnectionErrorMessage @ " (" @ %msg @ ")" , "", true);
}


//----------------------------------------------------------------------------
// Connection Failed Events
// These events aren't attached to a GameConnection object because they
// occur before one exists.
//----------------------------------------------------------------------------

function GameConnection::onConnectRequestRejected( %this, %msg )
{
   if (%this.pinging) return PingConnection::onConnectRequestRejected(%this, %msg);

   switch$(%msg)
   {
      case "CR_INVALID_PROTOCOL_VERSION":
         %error = "Incompatible protocol version: Your game version is not compatible with this server.";
      case "CR_INVALID_CONNECT_PACKET":
         %error = "Internal Error: badly formed network packet";
      case "CR_YOUAREBANNED":
         %error = "You are banned from this server.";
      case "CR_SERVERFULL":
         %error = "This server is full.";
      case "CHR_PASSWORD":
         // XXX Should put up a password-entry dialog.
         Canvas.setContent(LBChatGui);
         Canvas.pushDialog(MPJoinServerGui);
         MPJoinServerGui.pushPassword(MPJoinServerGui.joinIP, MPJoinServerGui.joinPort, $MP::ServerPassword !$= "");
         $MP::ServerPassword = "";
         MPJoinServerGui.joining = false;

         disconnectedCleanup(true);
         return;
      case "CHR_PROTOCOL":
         %error = "Incompatible protocol version: Your game version is not compatible with this server.";
      case "CHR_CLASSCRC":
         %error = "Incompatible game classes: Your game version is not compatible with this server.";
      case "CHR_INVALID_CHALLENGE_PACKET":
         %error = "Internal Error: Invalid server response packet";
      case "CHR_INVALID_PLATFORM": // Jeff: mac/pc trying to play together? haha.  Yeah...right.
         %error = "The current server is not compatible with your computer platform.";
      default:
         %error = "Connection error.  Please try another server.  Error code: (" @ %msg @ ")";
   }
   if ($LB::loggedIn && $LB::SuperChallengeMode)
      LBSCD_SendError();
   disconnectedCleanup($Server::Reconnecting);
   if (!$Server::Reconnecting)
	   MessageBoxOK( "REJECTED", %error, "", true);
}

function GameConnection::onConnectRequestTimedOut(%this)
{
   if (%this.pinging) return PingConnection::onConnectRequestTimedOut(%this);

   if ($LB::loggedIn && $LB::SuperChallengeMode && !$LB::SuperChallengePractice)
      LBSCD_SendError();
   disconnectedCleanup($Server::Reconnecting);
   if (!$Server::Reconnecting)
	   MessageBoxOK( "TIMED OUT", "Your connection to the server timed out.", "", true );
}


//-----------------------------------------------------------------------------
// Disconnect
//-----------------------------------------------------------------------------

function exitGame(%force) {
   if ($LB::loggedIn) {
      if ($LB::SuperChallengeMode) {
         LBSCD_Nag(%force);
         return;
      } else if ($LB::ChallengeMode) {
         if (!%force) {
            disconnect();
            schedule(30, 0, setVariable, "LB::ChallengeMode", false);
         } else
            exitChallenge();
         return;
      }
   }
   resumeGame();
   disconnect();

   //HiGuy: Copy from EndGameGui.gui

   // Doing this is probably bad, but it gets what I need
   percentGui::listComplete();

   // Check to see if we should show the ending screen. The statement above fills up the variables below
   if ($qual_beginner + $qual_intermediate + $qual_advanced + $qual_expert == $Game::MissionsTotal)
   {
      if ($pref::thankYouMessageReceived[$pref::highestFlag] == true)
         return; // We don't want the message appearing twice do we?

      Canvas.setContent("thankYouGui");
   }
}

function disconnect(%auto)
{
   // Delete the connection if it's still there.
   while (isObject(ServerConnection))
      ServerConnection.delete();
   disconnectedCleanup(%auto);

   // Call destroyServer in case we're hosting
   destroyServer();
}

function disconnectedCleanup(%auto)
{
   // Clear misc script stuff
   HudMessageVector.clear();

   if (isObject(MusicPlayer))
      MusicPlayer.stop();
   //
   //LagIcon.setVisible(false);
   //PlayerListGui.clear();

   // Clear all print messages
   clientCmdclearBottomPrint();
   clientCmdClearCenterPrint();

   // Back to the launch screen
   if (!%auto) {
      if ($LB::loggedin) {
         Canvas.setContent(LBChatGui);
         if (MPJoinServerGui.joining || $Server::ServerType $= "MultiPlayer") {
            Canvas.pushDialog(MPJoinServerGui);
            MPJoinServerGui.joining = false;
         } else
            Canvas.pushDialog(LBPlayMissionDlg);
      } else {
         Canvas.setContent(PlayMissionGui);
      }

      //HiGuy: Reset These
      $LB::Kill = false;
      $LB::SuperChallengeMode = false;
      $LB::ChallengeMode = false;
   }

   // Jeff: resetting spectating
   $spectatorIndex = 0;
   $spectatorPerson = -1;
   $SpectateFlying = false;
   $SpectateMode = false;
   $randomMission = false;
   $MP::Camera = -1;
   $MP::ServerChat = "";
   $Server::ServerType = "";

   if ($CurrentGame $= "MultiPlayer")
      $CurrentGame = $previousGame;
   if ($Game::Playback)
   	finishPlayback();

   //HiGuy: If we've gotten here, we're quitting without saving
   if ($Game::Recording)
   	scrapRecording();

   cancel(updateClientHandiCapItems);

   // Jeff: reset ghost list
   $MP::BuiltGhostList = false;

   // Jeff: reset server platform
   $Server::Platform = "";

   // Dump anything we're not using
   clearTextureHolds();
   purgeResources();
   stopDemo();
}

