//-----------------------------------------------------------------------------

// Variables used by client scripts & code.  The ones marked with (c)
// are accessed from code.  Variables preceeded by Pref:: are client
// preferences and stored automatically in the ~/client/	file
// in between sessions.
//
//    (c) Client::MissionFile             Mission file name
//    ( ) Client::Password                Password for server join

//    (?) Pref::Player::CurrentFOV
//    (?) Pref::Player::DefaultFov
//    ( ) Pref::Input::KeyboardTurnSpeed

//    (c) pref::Master[n]                 List of master servers
//    (c) pref::Net::RegionMask
//    (c) pref::Client::ServerFavoriteCount
//    (c) pref::Client::ServerFavorite[FavoriteCount]
//    .. Many more prefs... need to finish this off

// Moves, not finished with this either...
//    (c) firstPerson
//    $mv*Action...

//-----------------------------------------------------------------------------
// These are variables used to control the shell scripts and
// can be overriden by mods:

//-----------------------------------------------------------------------------
function initClient()
{
   echo("\n--------- Initializing FPS: Client ---------");

   // Make sure this variable reflects the correct state.
   $Server::Dedicated = false;

	//ToTD and Patches
   $checkserver = "marbleblast.com:80";
   $checkpath = "/leader/";
   $checkport = 80;
   $checkuserAgent = "Torque/1.0";

   $versionRetryWait = 1200;
   $versionRetryCount = 5;

	// DATA VARIABLES
	$Files::MBPMissionsFolder = "missions_mbp";
	$Files::MBGMissionsFolder = "missions_mbg";
	$Files::CustomMissionsFolder = "missions/custom";
	$Files::LB::MBPMissionsFolder = "lbmissions_mbp";
	$Files::LB::MBGMissionsFolder = "lbmissions_mbg";
	$Files::LB::CustomMissionsFolder = "lb_custom";

   // Game information used to query the master server
   $Client::GameTypeQuery = "Marble Blast";
   $Client::MissionTypeQuery = "Any";

   // Default level qualification
   // Do we need hunt/coop added here?
   // Is this section even needed or not?

   if (!$pref::QualifiedLevel["Beginner"])
      $pref::QualifiedLevel["Beginner"] = 1;
   if (!$pref::QualifiedLevel["Intermediate"])
      $pref::QualifiedLevel["Intermediate"] = 1;
   if (!$pref::QualifiedLevel["Advanced"])
      $pref::QualifiedLevel["Advanced"] = 1;
   if (!$pref::QualifiedLevel["Custom"])
      $pref::QualifiedLevel["Custom"] = 5000;
   if (!$pref::QualifiedLevel["Expert"])
      $pref::QualifiedLevel["Expert"] = 1;
   if (!$pref::QualifiedLevel["GG"])
      $pref::QualifiedLevel["GG"] = 1;
	if (!$pref::QualifiedLevel["LBCustom"])
      $pref::QualifiedLevel["LBCustom"] = 5000;

   if (!$pref::MBGQualifiedLevel["Beginner"])
      $pref::MBGQualifiedLevel["Beginner"] = 1;
   if (!$pref::MBGQualifiedLevel["Intermediate"])
      $pref::MBGQualifiedLevel["Intermediate"] = 1;
   if (!$pref::MBGQualifiedLevel["Advanced"])
      $pref::MBGMBGQualifiedLevel["Advanced"] = 1;
   if (!$pref::QualifiedLevel["Custom"])
      $pref::MBGQualifiedLevel["Custom"] = 1000;

   // The common module provides basic client functionality
   initBaseClient();

   // InitCanvas starts up the graphics system.
   // The canvas needs to be constructed before the gui scripts are
   // run because many of the controls assume the canvas exists at
   // load time.
   initCanvas("Marble Blast Platinum" SPC $THIS_VERSION_NAME SPC "Beta");

	// HiGuy: Detect ignition vs non-ignition
	$Con::LogBufferEnabled = false;
	%ig = new IgnitionObject();
	$Con::LogBufferEnabled = true;
	$ignitionVersion = isObject(%ig);

	Canvas.setContent(new GuiChunkedBitmapCtrl(){extent=Canvas.extent;});

   // HiGuy: Logging for Phil
   if ($ignitionVersion) {
      echo("\c3Detected ignition version of game...");
   } else {
      echo("\c3Detected non-ignition version of game...");
   }

	// HiGuy: Use the REGULAR ignition
	if (isScriptFile(expandFilename("./ui/ignitionGui.gui")))
   	exec("./ui/ignitionGui.gui");
	if (isScriptFile(expandFilename("./ui/ignitionStatusGui.gui")))
   	exec("./ui/ignitionStatusGui.gui");

   /// Load client-side Audio Profiles/Descriptions
   exec("./scripts/audioProfiles.cs");
   exec("./scripts/redundancycheck.cs");

   // Load up the Game GUIs
   exec("./ui/defaultGameProfiles.cs");
   exec("./ui/PlayGui.gui");

   // Load up the shell GUIs
	exec("./ui/playMissionGui.gui");
	exec("./ui/mainMenuGui.gui");
   exec("./ui/aboutDlg.gui");
   exec("./ui/chooseGui.gui");
   exec("./ui/endGameGui.gui");
   EndGameGui.preload();
   exec("./ui/loadingGui.gui");
   exec("./ui/optionsDlg.gui");
	exec("./ui/optionsExtrasDlg.gui");
   exec("./ui/remapDlg.gui");
   exec("./ui/MOTDGui.gui");
   exec("./ui/EnterNameDlg.gui");
   EnterNameDlg.preload();
   //exec("./ui/gameManualGui.gui");
   exec("./ui/ExitGameDlg.gui");
   exec("./ui/MiniShotGui.gui");
	exec("./ui/thankYouGui.gui");
	exec("./ui/voiceTauntsDlg.gui");

   // Spy47 GUIs
   exec("./ui/CustomResDlg.gui");
   exec("./ui/marbleSelectGui.gui");
   exec("./ui/StatisticsGui.gui");
   exec("./ui/DemoNameDlg.gui");
   exec("./ui/PlayDemoGui.gui");
   exec("./ui/HowToLEGui.gui");
   exec("./ui/errorHandlerGui.gui");
   exec("./ui/CompleteDemoDlg.gui");
   exec("./ui/AchievementsGui.gui");
   exec("./ui/SearchGui.gui");
   exec("./ui/VersionGui.gui");
   exec("./ui/JukeboxDlg.gui");

   // Client scripts
   exec("./scripts/client.cs");
   exec("./scripts/endGameGui.cs");
   exec("./scripts/missionDownload.cs");
   exec("./scripts/serverConnection.cs");
   exec("./scripts/loadingGui.cs");
   exec("./scripts/optionsDlg.cs");
	exec("./scripts/optionsExtrasDlg.cs");
	exec("./scripts/math64.cs");
   exec("./scripts/chatHud.cs");
   exec("./scripts/messageHud.cs");
   exec("./scripts/playGui.cs");
   exec("./scripts/centerPrint.cs");
   exec("./scripts/game.cs");
   exec("./scripts/tcp.cs");
   exec("./scripts/radar.cs");
   exec("./scripts/miscfunctions.cs");
   exec("./scripts/taunt.cs");
   exec("./scripts/replay.cs");

   //GFX Extender
//   exec("./scripts/graphicsAdjust.cs");
//   exec("./scripts/graphicsExtender.cs");
//   exec("./ui/GraphicsExtender.gui");

   if (isScriptFile(expandFileName("./scripts/demo.cs"))) // Jeff: avoid console spam
      exec("./scripts/demo.cs");      // non-ignition MB needs it (RealArcade)

   // Default player key bindings
   exec("./scripts/default.bind.cs");

   // Jeff: taunt.cs also contains keybindings for the taunts
   bindDefaultTauntKeys();

   if (isScriptFile(expandFilename("./config.cs")))
      exec("./config.cs");

   // Jeff: load leaderboard main script!
   exec($usermods @ "/leaderboards/main.cs");

   // Is this comment here needed? if not, delete this.
   // Really shouldn't be starting the networking unless we are
   // going to connect to a remote server, or host a multi-player
   // game.
   // setNetPort(0);

   // Copy saved script prefs into C++ code.
   setShadowDetailLevel( $pref::shadows );
   setDefaultFov( $pref::Player::defaultFov );
   setZoomSpeed( $pref::Player::zoomSpeed );

   // Spy47 : Set MBP levels as default.
   $CurrentGame = "Platinum";

   // Start up the main menu... this is separated out into a
   // method for easier mod override.
   loadMainMenu();

   // Connect to server if requested.
   if ($JoinGameAddress !$= "")
      schedule(1000, 0, joinServer, $JoinGameAddress, $Pref::Player::Name);
   else if($missionArg !$= "") {
      %file = findNamedFile($missionArg, ".mis");
      if(%file $= "") {
         ASSERT("Mission not found.", "The mission " @ $missionArg @ " couldn't be found.");
         error("Error: Mission file at -mission argument not found.");
      } else {
         %this.io = new IgnitionObject();
         %status = %this.io.validate();
         echo("Ignition: " @ %status);

         Canvas.setContent(LoadingGui);
         schedule(1000, 0, doCreateGame, %file);
      }
   }
   else if($interiorArg !$= "") {
      doInteriorTest($interiorArg);
   }
}

function doInteriorTest(%interiorName) {
   if (%interiorName $= "") {
      ASSERT("Interior test failed.", "The interior filename was missing.");
      error("doInteriorTest: Interior filename missing.");
      return;
   }

   %file = findNamedFile(%interiorName, ".dif");
   if(%file $= "") {
      ASSERT("Interior test failed.", "The interior " @ %interiorName @ " couldn't be found.");
      error("doInteriorTest: Interior with filename " @ %interiorName @ " not found.");
      return;
   }

   onServerCreated(); // gotta hack here to get the datablocks loaded...

   %this.io = new IgnitionObject();
   %status = %this.io.validate();
   echo("Ignition: " @ %status);

   %missionGroup = createEmptyMission(%interiorName);
   %interior = new InteriorInstance() {
                  position = "0 0 0";
                  rotation = "1 0 0 0";
                  scale = "1 1 1";
                  interiorFile = %file;
               };
   %missionGroup.add(%interior);
   %interior.magicButton();

   if(!isObject(StartPoint))
   {
      %pt = new StaticShape(StartPoint) {
         position = "0 -5 100";
         rotation = "1 0 0 0";
         scale = "1 1 1";
         dataBlock = "StartPad";
      };
      MissionGroup.add(%pt);
   }

   if(!isObject(EndPoint))
   {
      %pt = new StaticShape(EndPoint) {
         position = "0 5 100";
         rotation = "1 0 0 0";
         scale = "1 1 1";
         dataBlock = "EndPad";
      };
      MissionGroup.add(%pt);
   }
   %box = %interior.getWorldBox();
   %mx = getWord(%box, 0);
   %my = getWord(%box, 1);
   %mz = getWord(%box, 2);
   %MMx = getWord(%box, 3);
   %MMy = getWord(%box, 4);
   %MMz = getWord(%box, 5);
   %pos = (%mx - 25) SPC (%MMy + 25) SPC (%mz - 25);
   %scale = (%MMx - %mx + 25) SPC (%MMy - %my + 25) SPC (%MMz - %mz + 25);
   echo(%box);
   echo(%pos);
   echo(%scale);

   new Trigger(Bounds) {
      position = %pos;
      scale = %scale;
      rotation = "1 0 0 0";
      dataBlock = "InBoundsTrigger";
      polyhedron = "0.0000000 0.0000000 0.0000000 1.0000000 0.0000000 0.0000000 0.0000000 -1.0000000 0.0000000 0.0000000 0.0000000 1.0000000";
   };
   MissionGroup.add(Bounds);

   %missionGroup.save($usermods @ "/data/missions/testMission.mis");
   %missionGroup.delete();
   doCreateGame($usermods @ "/data/missions/testMission.mis");
}


function isScriptFile(%file) {
   if (isFile(%file) || isFile(%file @ ".dso"))
      return true;
   return false;
}

function doCreateGame(%file)
{
   MarbleSelectGui.update();

	$playingDemo = false; //HiGuy: For some reason, this isn't reset.
   %id = PM_missionList.getSelectedId();
   //echo("ID IS" SPC %id);
   %mission = getMissionInfo(%file);
   //echo("MISSION IS" SPC %mission.file);
   $LastMissionType = %mission.type;

   // Jeff: last mission played, save it
   %name = (%mission.name !$= "") ? %mission.name : fileBase(%mission.file);
   if ($CurrentGame $= "Platinum")
      $Pref::LastMissionPlayed[$MissionType] = %name;
   else
      $Pref::LastMBGMissionPlayed[$MissionType] = %name;
   savePrefs(true); // Jeff: moved save prefs to here

	// Spy47: CRC checker. Nobody likes it, remember?
	//if(%mission.getGroup().getName() !$= "MTYPE_Custom")
	//{
        //if(!checkMissionCRC(%mission.file))
        //{
        //    ASSERT("Invalid Mission File","Probably your mission file has been modified.\nPlease reinstall the latest MBP patch or contact the MBP Team.");
        //    return;
   	//	}
	//}

   while (!$Server::Hosting && isObject(ServerConnection))
      ServerConnection.delete();

   %multiplayer = strStr(%file, "multiplayer") != -1;
   if ($pref::HostMultiPlayer || %multiplayer)
      %serverType = "MultiPlayer";
   else
      %serverType = "SinglePlayer";

   // We need to start a server if one isn't already running
   if ($Server::ServerType $= "") {

      // Spy47 : Modifying demo recording system
      if($doRecordDemo)
      {
         //echo("Recording as: ~/client/demos/" @ $recordDemoName @ ".rec");
         recordDemo("~/client/demos/" @ $recordDemoName @ ".rec", %mission.file);
         //recordDemo("~/client/demos/demo.rec", %mission.file);
      }
      createServer(%serverType, %mission.file);
      loadMission(%mission.file, true);
      %conn = new GameConnection(ServerConnection);
      RootGroup.add(ServerConnection);
      %conn.setConnectArgs($LB::Username, -1, "", "", "bologna");
//      %conn.setJoinPassword($Client::Password);
      %conn.connectLocal();
   }
   else
      loadMission(%mission.file);
   if(isObject(MissionInfo))
      MissionInfo.level = %mission.level;
}

function setPlayMissionGui()
{
	resumeGame();
	disconnect();
   Canvas.setContent(playMissionGui);
}

//-----------------------------------------------------------------------------
function startTotalTimer()
{
	// Spy47 This function is for.... THE TOTAL TIMER DUDE!!!
	// This function is damn loop.
	if (!$TotalTimer::Stopped)
		schedule(1000, 0, "startTotalTimer");
	else {
		$TotalTimer::Stopped = false;
		return;
	}

	if($pref::TotalTimer >= 86400)
	{
	    $pref::TotalTimerDaysAdd++;
		$pref::TotalTimer = 0;
	}
	else
		$pref::TotalTimer++;
}

function loadMainMenu()
{
   Canvas.setCursor("DefaultCursor");
	if (!$retinaAsk && $pref::Video::fullScreen && getWord($pref::Video::Resolution, 0) > getWord(getDesktopResolution(), 0) && getWord($pref::Video::Resolution, 1) > getWord(getDesktopResolution(), 1)) {
		//You're in retina mode
		$retinaAsk = true;
		MessageBoxYesNo("High Resolution Detected!", "You seem to be using a retina device, or have a higher window size than your screen resolution. Activate 2x retina mode? Most images will look low-quality, but at least you\'ll be able to see them. Some features may break, and there is no support for this feature. For the best experience, try switching to a resolution that is not in retina scale.", "activatePackage(retina); loadMainMenu();", "loadMainMenu();");
		return;
	}
   // Startup the client with the Main menu...
   runIgnition(); // No more introduction screen, straight to the main menu - Phil
   JukeboxDlg::getSongList();
   buildMissionList();
   playShellMusic();

   // Check if we got a presentable error from bad command-line arguments. (ASSERT isn't defined during platinum/main.cs' execution)
   if ($argError > 0)
      presentCommandLineError($argError);

   //HiGuy: Updated TCP scripts (see my blog on the forums) which have repeated sending allow for Mac clients to persistantly connect to the internet. Works 100%

   //HiGuy: Nononono, this works _fine_ on a mac, and doing this just breaks it
   // for windows too!
//   if ($interiorArg !$= "")
   doMotdCheck();
   startTotalTimer();
}

function presentCommandLineError() {
   switch ($argError) {
      case 1: ASSERT("Mission launch failed.", "The mission filename was missing.");
      case 2: ASSERT("Interior test failed.", "The interior filename was missing.");
   }
}

function createEmptyMission(%interiorArg)
{
   $testcheats = 1; //HiGuy: So they can open the LE
   $enableEditor = 1; //HiGuy: Because I don't know which var to use anymore D:

   return new SimGroup(MissionGroup) {
     new ScriptObject(MissionInfo) {
           name = "Interior Test: " @ %interiorArg;
           desc = "A test mission to test an interior.";
           startHelpText = "Press F11 to open the level editor!"; //HiGuy: You said you didn't want this to be blank, here's the solution.
           type = "Custom";
           level = "1";
           time = "0";
           goldTime = "0";
     };
   new MissionArea(MissionArea) {
      area = "-360 -648 720 1296";
      flightCeiling = "300";
      flightCeilingRange = "20";
         locked = "true";
     };
   new Sky(Sky) {
      position = "336 136 0";
      rotation = "1 0 0 0";
      scale = "1 1 1";
      cloudHeightPer[0] = "0";
      cloudHeightPer[1] = "0";
      cloudHeightPer[2] = "0";
      cloudSpeed1 = "0.0001";
      cloudSpeed2 = "0.0002";
      cloudSpeed3 = "0.0003";
      visibleDistance = "500";
      useSkyTextures = "1";
      renderBottomTexture = "1";
      SkySolidColor = "0.600000 0.600000 0.600000 1.000000";
      fogDistance = "300";
      fogColor = "0.600000 0.600000 0.600000 1.000000";
      fogVolume1 = "-1 7.45949e-031 1.3684e-038";
      fogVolume2 = "-1 1.07208e-014 8.756e-014";
      fogVolume3 = "-1 5.1012e-010 2.05098e-008";
      materialList = "~/data/skies/Intermediate/Intermediate_Sky.dml";
      windVelocity = "1 0 0";
      windEffectPrecipitation = "0";
      noRenderBans = "1";
      fogVolumeColor1 = "128.000000 128.000000 128.000000 0.000000";
      fogVolumeColor2 = "128.000000 128.000000 128.000000 0.000004";
      fogVolumeColor3 = "128.000000 128.000000 128.000000 14435505.000000";
     };
   new Sun() {
      direction = "0.638261 0.459006 -0.61801";
      color = "1.400000 1.200000 0.400000 1.000000";
      ambient = "0.300000 0.300000 0.400000 1.000000";
     };
   };
}

//HiGuy: Get's the hash (homebrew because getFileCRC is *not* working)
//Returns: file hash or -1 if file does not exist
function getFileHash(%path) {
   %reader = new FileObject();
   if (!%reader.openForRead(%path)) {
      %reader.close();
      %reader.delete();
      return -1;
   }
   %c = 0;
   %l = 0;
   while (!%reader.isEOF()) {
      %line = %reader.readLine();
      if (%line $= "")
         continue;
      %line = strreplace(%line, "\n", "");
      %line = strreplace(%line, "\r", "");
      %line = strreplace(%line, "\t", "");
      %line = strreplace(%line, " ", "");
      %l += strLen(trim(%line));
      %c += %l % 32 + (strPos(%line, "$") == -1 ? 2 : 3);
   }

   devecho(%c SPC %l SPC %ll SPC %h);
   if (%l == 0 || %l == -1)
      return;
   %ll = %l;
   if (%ll * %l < %c)
      %ll = %ll * %l;
   %h = %c % %ll; //Hash
   %reader.close();
   %reader.delete();
   return %h;
}

// Random background function - Phil
function getRandomBg(%addon)
{
   %background = getRandom((%addon $= "platinum" ? 12 : 0), (%addon $= "gold" ? 11 : 39));
   %start = %background;
   while ($pref::backgroundSet[%background]) {
      %background ++;
      %background %= 40;
      if (%background == %start) {
         for (%i = 0; %i < 40; %i ++)
            $pref::backgroundSet[%i] = false;
         break;
      }
   }
   $pref::backgroundSet[%background] = true;
   %background ++;

   if (%background > 12) {
      %addon = "platinum";
      %background -= 12;
   } else
      %addon = "gold";
	%bgFolderPrefix = "backgrounds";
	return $usermods @ "/client/ui/" @ %bgFolderPrefix @ "/" @ %addon @ "/" @ %background;
}

package randomBitmap {
   function GuiCanvas::setContent(%this, %ctrl) {
      Parent::setContent(%this, %ctrl);
      %ctrl.setRandomBg();
   }
   function GuiControl::setRandomBg(%this) {
      %name = %this.followBitmap !$= "" ? %this.followBitmap : %this.getName();
      %addon = %this.addon;
      if ($randomBitmap[%name, %addon] $= "")
         $randomBitmap[%name, %addon] = getRandomBg(%addon);
      %this.setBitmap($randomBitmap[%name, %addon]);
   }
};

activatePackage(randomBitmap);
