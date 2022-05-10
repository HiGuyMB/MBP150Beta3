//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//----------------------------------------------------------------------------
// Mission Loading & Mission Info
// The mission loading server handshaking is handled by the
// common/client/missingLoading.cs.  This portion handles the interface
// with the game GUI.
//----------------------------------------------------------------------------

//----------------------------------------------------------------------------
// Loading Phases:
// Phase 1: Download Datablocks
// Phase 2: Download Ghost Objects
// Phase 3: Scene Lighting

//----------------------------------------------------------------------------
// Phase 1
//----------------------------------------------------------------------------

function onMissionDownloadPhase1(%missionName, %musicTrack)
{
   // Close and clear the message hud (in case it's open)
   //MessageHud.close();
   //cls();

   // Reset the loading progress controls:
   LoadingProgress.setValue(0);
   MPLoadingProgress.setValue(0);
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(0);

   // Jeff: this variable is used for the fader effect in playGui::onWake
   $PlayGuiFader = true;

   commandToServer('MissionLoadProgress', 0, 0);
}

function onPhase1Progress(%progress)
{
   LoadingProgress.setValue(%progress);
   MPLoadingProgress.setValue(%progress / 2);
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(%progress / 2);
   Canvas.repaint();

   %progress = mFloor(%progress * $MP::LoadSegments) / $MP::LoadSegments;
   if (!$fast && %progress !$= $MP::LastLoadProgress) {
      commandToServer('MissionLoadProgress', %progress / 2, 0);
      $MP::LastLoadProgress = %progress;
   }
}

function onPhase1Complete()
{
   MPLoadingProgress.setValue(0.5);
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(0.5);

   commandToServer('MissionLoadProgress', 0.5, 0);
}

//----------------------------------------------------------------------------
// Phase 2
//----------------------------------------------------------------------------

function onMissionDownloadPhase2()
{
   // Reset the loading progress controls:
   LoadingProgress.setValue(0);
   MPLoadingProgress.setValue(0.5);
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(0.5);
   Canvas.repaint();

   commandToServer('MissionLoadProgress', 0.5, 0);
}

function onPhase2Progress(%progress)
{
   LoadingProgress.setValue(%progress);
   MPLoadingProgress.setValue(0.5 + (%progress / 2));
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(0.5 + (%progress / 2));
   Canvas.repaint();

   //serverConnection.listObjects();
   //ServerConnection.withAll("dump");

   %progress = mFloor(%progress * $MP::LoadSegments) / $MP::LoadSegments;
   if (!$fast && %progress !$= $MP::LastLoadProgress) {
      commandToServer('MissionLoadProgress', 0.5 + (%progress / 2), 0);
      $MP::LastLoadProgress = %progress;
   }
}

function onPhase2Complete()
{
   MPLoadingProgress.schedule(2500, setValue, 0);

   commandToServer('MissionLoadProgress', 1, 2);
}

function clientCmdLoadFinish() {
   //HiGuy: Acknowledge finish
   commandToServer('MissionLoadProgress', 1, 3);
}

//----------------------------------------------------------------------------
// Phase 3
//----------------------------------------------------------------------------

function onMissionDownloadPhase3()
{
}

function onPhase3Progress(%progress)
{
   LoadingProgress.setValue(%progress);
}

function onPhase3Complete()
{
   LoadingProgress.setValue( 1 );
   $lightingMission = false;
}

//----------------------------------------------------------------------------
// Mission loading done!
//----------------------------------------------------------------------------

function onMissionDownloadComplete()
{
   // Client will shortly be dropped into the game, so this is
   // good place for any last minute gui cleanup.
}

//----------------------------------------------------------------------------
// Receiving files
//----------------------------------------------------------------------------

function onFileChunkReceived(%file, %recv, %total) {
   LoadingProgress.setValue(%recv / %total);
   MPLoadingProgress.setValue(%recv / %total);
   if (isObject(MPPlayerListBox @ $MP::ClientIndex))
      (MPPlayerListBox @ $MP::ClientIndex).setValue(%recv / %total);
   Canvas.repaint();

   if ($downloadStart[%file] $= "" || %recv < $downloadCurrent[%file])
      $downloadStart[%file] = getRealTime();

   $downloadCurrent[%file] = %recv;

   %rateS = (getRealTime() - $downloadStart[%file]) / 1000;
   %rate = %recv / %rateS;

   %estimated = ((getRealTime() - $downloadStart[%file]) / (%recv / %total)) - (getRealTime() - $downloadStart[%file]);

   if (mRound(%recv * $MP::LoadSegments / %total) > $downloadSend[%file]) {
      commandToServer('MissionLoadProgress', mRound(%recv * $MP::LoadSegments / %total) / $MP::LoadSegments, 1);
      $downloadSend[%file] = mRound(%recv * $MP::LoadSegments / %total);
   }
   MPTeamChoiceTitle.setValue("<font:DomCasualD:24><just:center>Receiving" SPC fileBase(%file) @ fileExt(%file) SPC "-" SPC mfloor((100 * %recv) / %total) @ "% (" @ mFloor(%rate / 100) / 10 SPC "kB/s)" SPC getSubStr(formatTimeSeconds(%estimated), 1, 999));
	LOAD_MapName.setText( "<font:DomCasualD:32><just:center>" @ $Game::MapName NL "Downloading" SPC fileBase(%file) @ fileExt(%file) SPC "-" SPC mfloor((100 * %recv) / %total) @ "% (" @ mFloor(%rate / 100) / 10 SPC "kB/s)" SPC getSubStr(formatTimeSeconds(%estimated), 1, 999));
}

//------------------------------------------------------------------------------
// Before downloading a mission, the server transmits the mission
// information through these messages.
//------------------------------------------------------------------------------

addMessageCallback( 'MsgClientIndex', handleClientIndexMessage );
addMessageCallback( 'MsgLoadInfo', handleLoadInfoMessage );
addMessageCallback( 'MsgLoadDescripition', handleLoadDescriptionMessage );
addMessageCallback( 'MsgServerDedicated', handleServerDedicatedMessage );
addMessageCallback( 'MsgServerDescription', handleServerInfoMessage );
addMessageCallback( 'MsgServerName', handleServerNameMessage );
addMessageCallback( 'MsgReadyCount', handleReadyCountMessage );
addMessageCallback( 'MsgLoadInfoDone', handleLoadInfoDoneMessage );
addMessageCallback( 'MsgLoadMissionInfoPart', handleLoadMissionInfoPartMessage );
addMessageCallback( 'MsgServerPrefs', handleServerPrefsMessage );

//------------------------------------------------------------------------------

function clientCmdShowLoadScreen() {
	// Need to pop up the loading gui to display this stuff.
	Canvas.setContent("LoadingGui");
}

function handleClientIndexMessage(%msgType, %msgString, %index) {
   $MP::ClientIndex = %index;
}

//------------------------------------------------------------------------------

function handleLoadInfoMessage( %msgType, %msgString, %mapName ) {

	// Clear all of the loading info lines:
	// for( %line = 0; %line < LoadingGui.qLineCount; %line++ )
		//LoadingGui.qLine[%line] = "";
	//LoadingGui.qLineCount = 0;

//	Canvas.setContent("LoadingGui");

   //
	LOAD_MapName.setText( "<font:DomCasualD:32><just:center>" @ %mapName );
	$Game::MapName = %mapName;

   MPPreGameDlg.mapName = %mapName;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleLoadDescriptionMessage( %msgType, %msgString, %line )
{
	//LoadingGui.qLine[LoadingGui.qLineCount] = %line;
	//LoadingGui.qLineCount++;

   // Gather up all the previous lines, append the current one
   // and stuff it into the control
	//%text = "<spush><font:Arial:16>";

	//for( %line = 0; %line < LoadingGui.qLineCount - 1; %line++ )
		//%text = %text @ LoadingGui.qLine[%line] @ " ";
   //%text = %text @ LoadingGui.qLine[%line] @ "<spop>";

	//LOAD_MapName.setText( %text );

   MPPreGameDlg.mapDesc = %line;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleServerDedicatedMessage( %msgType, %msgString, %dedicated )
{
   MPPreGameDlg.dedicated = %dedicated;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleServerInfoMessage( %msgType, %msgString, %info )
{
   MPPreGameDlg.info = %info;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleServerNameMessage( %msgType, %msgString, %serverName )
{
   MPPreGameDlg.serverName = %serverName;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleReadyCountMessage( %msgType, %msgString, %readyCount, %playerCount )
{
   MPPreGameDlg.readyCount = %readyCount;
   MPPreGameDlg.playerCount = %playerCount;
   MPPreGameDlg.updateInfo();
}

//------------------------------------------------------------------------------

function handleLoadMissionInfoPartMessage( %msgType, %msgString, %part ) {
   // This spits out the mission info
   $MP::MissionInfoString = $MP::MissionInfoString @ %part;
}

//------------------------------------------------------------------------------

function handleLoadInfoDoneMessage( %msgType, %msgString ) {
   // This will get called after the last description line is sent.

   //HiGuy: Only place where eval can kinda be haxed. I really don't want to
   // use it here, but there's no alternative options. Let's just hope nobody
   // figures out how to send this via console!
   if ((!$Server::Hosting || $Server::_Dedicated) && $Server::ServerType $= "MultiPlayer")
      %missionInfo = eval($MP::MissionInfoString);
   echo($MP::MissionInfoString);
   $MP::MissionInfoString = "";
}

//------------------------------------------------------------------------------

function handleServerPrefsMessage( %msgType, %msgString, %requiredAmount, %chargeTime, %quickRespawn, %blastPower, %rechargePower, %matanMode, %glassMode, %info, %fast, %gemvision )
{
   $MP::BlastRequiredAmount = %requiredAmount;
   $MP::BlastChargeTime = %chargeTime;
   $MP::AllowQuickRespawn = %quickRespawn;
   $MP::BlastPower = %blastPower;
   $MP::BlastRechargePower = %rechargePower;
   $MP::Server::MatanMode = %matanMode;
   $MP::Server::GlassMode = %glassMode;
   $MP::Server::GemVision = %gemVision;
   $MP::ServerInfo = %info;
   $MP::FastPowerups = %fast;
   if ($MP::ServerChat $= "")
      onServerChat(LBChatColor("welcome") @ %info @ "\n");
}

