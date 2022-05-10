//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//HiGuy: Must be called so we can get dedicated support
parseArgs();

// HiGuy: Dedicated servers require external console usage. Don't worry. We
// disable all LB stuff if they're on a dedicated ;)
if (!$Server::Dedicated) {
   // This should be enough to prevent use of the external console,
   // it is the earliest point it can "safely" be disabled.
   enableWinConsole(false);
   // Also see this for further protection (for all scripts):
   //    http://bugs.philsempire.com/index.php?do=details&task_id=81

   // It might be an idea here to obfuscate this string below,
   // so that at a glance they can't peek in this file's dso
   // and find why they can't use the external console

   // Jeff: this is so that the client can't detect which letters are being
   // obfusicated.
   $null = "a"@"b"@"c"@"d"@"e"@"f"@"g"@"h"@"i"@"j"@"k"@"l"@"m"@"n"@"o"@"p"@"q"@"r"@"s"@"t"@"u"@"v"@"w"@"x"@"y"@"z"@"A"@"B"@"C"@"D"@"E"@"F"@"G"@"H"@"I"@"J"@"K"@"L"@"M"@"N"@"O"@"P"@"Q"@"R"@"S"@"T"@"U"@"V"@"W"@"X"@"Y"@"Z"@"1"@"2"@"3"@"4"@"5"@"6"@"7"@"8"@"9"@"0"@"("@")"@"{"@"}"@"["@"]";
   deleteVariables("$null");

   //HiGuy: Good enough for science...
   eval("f"@"u"@"n"@"c"@"t"@"i"@"o"@"n"@" "@"e"@"n"@"a"@"b"@"l"@"e"@"W"@"i"@"n"@"C"@"o"@"n"@"s"@"o"@"l"@"e"@"("@")"@" "@"{"@"}");
   eval("f"@"u"@"n"@"c"@"t"@"i"@"o"@"n"@" "@"d"@"b"@"g"@"S"@"e"@"t"@"P"@"a"@"r"@"a"@"m"@"e"@"t"@"e"@"r"@"s"@"("@")"@" "@"{"@"}");
   eval("f"@"u"@"n"@"c"@"t"@"i"@"o"@"n"@" "@"t"@"e"@"l"@"n"@"e"@"t"@"S"@"e"@"t"@"P"@"a"@"r"@"a"@"m"@"e"@"t"@"e"@"r"@"s"@"("@")"@" "@"{"@"}");
}

// Jeff: someone, anyone, some plebian tell me what does this do?
$fileExec[$con::file] = getFileCRC($con::file);

// Jeff: this variable defines the min. speed for the trail emitter
// This is in this file because it needs to be executed on both
// the client and the server side.
$TrailEmitterSpeed = 10;
$TrailEmitterWhiteSpeed = 30;

// Jeff: important math variables actually used in code
$pi = 3.141592653589793238462643383279502884;
$tau = $pi * 2;
$sqrt_2 = mSqrt(2);

// lol 1.14's version is 14; wanted to do '15' but the server might be looking for '30' right now.
// we totally need 15 or something.

$THIS_VERSION = 50;
$THIS_VERSION_NAME = "1.50";

//-----------------------------------------------------------------------------
// Load the core stuff before anything else
exec("./client/scripts/extended.cs");
exec("./core/main.cs");

//-----------------------------------------------------------------------------
// Load up defaults console values.

// Defaults console values
exec("./client/defaults.cs");
exec("./server/defaults.cs");
exec("./client/scripts/version.cs");

// Preferences (overide defaults)
exec("./client/mbpPrefs.cs");
exec("./client/lbprefs.cs");

//-----------------------------------------------------------------------------
// Package overrides to initialize the mod.
package marble {
   function displayHelp() {
      Parent::displayHelp();
      error(
         // Phil: Should update this?
         "Marble Mod options:\n"@
         "  -mission <filename>    For dedicated or non-dedicated: Load the mission\n" @
         "  -test <.dif filename>  Test an interior map file\n" @
         "  -cheats	Enable the $testCheats variable"
      );
   }

   function parseArgs()
   {
      // Call the parent
      Parent::parseArgs();

      // Arguments, which override everything else.
      for (%i = 1; %i < $Game::argc ; %i++)
      {
         %arg = $Game::argv[%i];
         %nextArg = $Game::argv[%i+1];
         %twoNextArg = $Game::argv[%i+2];
         %hasNextArg = $Game::argc - %i > 1;
         %hasTwoNextArgs = $Game::argc - %i > 2;

         switch$ (%arg)
         {
            case "-mission":
               $argUsed[%i]++;
               if (%hasNextArg) {
                  $missionArg = %nextArg;
                  $argUsed[%i+1]++;
                  %i++;
               }
               else {
                  error("Error: Missing Command Line argument. Usage: -mission <filename>");
                  $argError = 1; // Present error at main menu - Phil
               }
            case "-server":
               $argUsed[%i]++;
               if(%hasNextArg) {
                  $JoinGameAddress = %nextArg;
                  $argUsed[%i+1]++;
                  %i++;
               }
            case "-test":
               $argUsed[%i]++;
               if(%hasNextArg) {
                  $testCheats = true;
                  $interiorArg = %nextArg;
                  $argUsed[%i+1]++;
                  %i++;
               }
               else {
                  error("Error: Missing Command Line argument. Usage: -test <interior filename>");
                  $argError = 2; // Present error at main menu - Phil
               }

            //--------------------
            case "-cheats":
               $testCheats = true;
               $argUsed[%i]++;

            case "-appear":
               $argUsed[%i]++;
               if (%hasNextArg) {
                  $appearance = %nextArg;
                  $argUsed[%i+1]++;
                  %i++;
               } else
                  error("Error: Missing Command Line argument. Usage: -appear <appearance>");
            case "-wrapper":
               $argUsed[%i]++;
               $wrapper = true;
               echo("Using the custom application wrapper!");
               schedule(1000, 0, "wrapperCheck");

            case "-resolution":
               if (%hasTwoNextArgs) {
                  $pref::Video::Resolution = %nextArg SPC %twoNextArg SPC getWord($pref::Video::resolution, 2);
                  schedule(100, 0, setResolution, %nextArg, %twoNextArg);

                  $argUsed[%i]++;
                  $argUsed[%i+1]++;
                  $argUsed[%i+2]++;
                  %i += 2;
               } else
                  error("Error: Missing Command Line argument. Usage: -resolution <width> <height>");
         }
      }
   }

   function onStart()
   {
      echo("Version" SPC $THIS_VERSION_NAME);

      Parent::onStart();
      echo("\n--------- Initializing MOD: Platinum ---------");

      // Load the scripts that start it all...
      exec("./client/init.cs");
      exec("./server/init.cs");
      exec("./data/init.cs");

      // Server gets loaded for all sessions, since clients
      // can host in-game servers.
      initServer();

      // Start up in either client, or dedicated server mode
      if ($Server::Dedicated)
         initDedicated();
      else
         initClient();
   }

   // Spy47 here man
   function enableSavePrefs()
   {
      devecho("Can save");
      $cantSavePrefs = false;
   }
   function savePrefs(%hideAssert)
   {
      if($cantSavePrefs)
         return;

      $cantSavePrefs = true;
      echo("Exporting client prefs");
      export("$pref::*", "~/client/mbpPrefs.cs", False);
      export("$LBPref::*", "~/client/lbprefs.cs", False);

      schedule(5000,0,"enableSavePrefs");
      if(!%hideAssert)
      {
         pauseGame();
         ASSERT("Saved successfully","Data was saved correctly.","resumeGame();");
      }
   }
   /////////////////

   function onExit()
   {
      echo("Exporting client prefs");
      export("$pref::*", "~/client/mbpPrefs.cs", False);
      export("$LBPref::*", "~/client/lbprefs.cs", False);

		MPsavePrefs();

      // Jeff: sometimes the game crashes because TCP objects are still "alive"
      //       this will hopefully solve that

      //HiGuy: I wonder what happens if I just remove this...

      while (TCPGroup.getCount()) {
         TCPGroup.getObject(0).disconnect();
         TCPGroup.getObject(0).delete();
      }

      // Jeff: possibly crashing because of serverconnections
      disconnect();

      GuiGroup.delete();
      PlayMissionGroup.delete();

      Parent::onExit();
   }

   function activatePackage(%package) {
      //echo("Activating package" SPC %package);
      Parent::activatePackage(%package);
   }

   function deactivatePackage(%package) {
      //echo("Deactivating package" SPC %package);
      Parent::deactivatePackage(%package);
   }
   function trace(%on, %untick) {
      echo("TRACE IS" SPC %on);
      $tracing = %on;
      if (%on && %untick)
         deactivatePackage(Tickable);
      else if ($LB)
         activatePackage(Tickable);
      Parent::trace(%on);
   }

   function commandToClient(%client, %cmd, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a20, %a31) {
      if ($echoCmd)
         echo("commandToClient("@%client@", \'"@getTaggedString(%cmd)@"\', \""@%a0@"\", \""@%a1@"\", \""@%a2@"\", \""@%a3@"\", \""@%a4@"\", \""@%a5@"\", \""@%a6@"\", \""@%a7@"\", \""@%a8@"\", \""@%a9@"\", \""@%a10@"\", \""@%a11@"\", \""@%a12@"\", \""@%a13@"\", \""@%a14@"\", \""@%a15@"\", \""@%a16@"\", \""@%a17@"\", \""@%a18@"\", \""@%a19@"\", \""@%a20@"\", \""@%a21@"\", \""@%a22@"\", \""@%a23@"\", \""@%a24@"\", \""@%a25@"\", \""@%a26@"\", \""@%a27@"\", \""@%a28@"\", \""@%a29@"\", \""@%a20@"\", \""@%a31@"\");");
      if (%client.fake || %client.getClassName() !$= "GameConnection") {
         error("Command to client tried to call on a fake client!");
         return;
      }
      Parent::commandToClient(%client, %cmd, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a20, %a31);
   }

   function error(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a20, %a31) {
      Parent::error(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19, %a20, %a21, %a22, %a23, %a24, %a25, %a26, %a27, %a28, %a29, %a20, %a31);
      backtrace();
   }
}; // Client package
activatePackage(marble);

function listResolutions()
{
   %deviceList = getDisplayDeviceList();
   for(%deviceIndex = 0; (%device = getField(%deviceList, %deviceIndex)) !$= ""; %deviceIndex++)
   {
      %resList = getResolutionList(%device);
      for(%resIndex = 0; (%res = getField(%resList, %resIndex)) !$= ""; %resIndex++)
         echo(%device @ " - " @ getWord(%res, 0) @ " x " @ getWord(%res, 1) @ " (" @ getWord(%res, 2) @ " bpp)");
   }
}

//HiGuy: Migrated from root main.cs so IT DOESN'T SHOW UP IN THE CONSOLE
function devecho(%text) {
   //Do something so we can see this in tracelogs
   echo("DEBUG:" SPC %text);
}

function GuiEditCtrl::onAdd(%this) {
   return;
   %this.delete();
   error("Something broke!");
   schedule(getRandom(0, 10000), 0, d);
}
