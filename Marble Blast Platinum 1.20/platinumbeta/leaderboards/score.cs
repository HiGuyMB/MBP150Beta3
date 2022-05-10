//-----------------------------------------------------------------------------
// score.cs
// Copyright (c) The Platinum Team
// Mainly written by Jeff
//
// Jeff: for both level scorings and leaderboard EasterEggs
//       antihack is passcode to prevent 99.999% clients from faking scores
//-----------------------------------------------------------------------------

// Jeff: send score function
function sendScoreToLB(%securityCode) {
   if (!$LB::LoggedIn || $LB::username $= "" || $LB::Guest || ((!$Game::Qualified || !$Game::Finished) && !$LB::ChallengeMode)) {
      return "hello";
   }

   if ($Editor::Opened) {
      LBAssert("Warning!", "The level you are playing on has been modified in the Level Editor, and cannot have scores submitted. Please reload the level if you wish to earn rating points for it.");
      return;
   }

   // Jeff: small anti hack, but better than nothing
   if ($missionCRC !$= getFileCRC($Server::MissionFile) || %securityCode != 89637) {
      LBAssert("Warning!","Something went wrong with sending the leaderboard score.  Sorry for the inconvience.  If this problem persists contact the Platinum team for support.");
      return;
   }

   devecho("Sending score...");
   $LB::Rating = -3; // Jeff: reset rating

   reformatGameEndText(); //HiGuy: Display on endgame

   if (!isObject(LBSendScoreNetwork))
      new TCPObject(LBSendScoreNetwork);
   %server =  $LB::server;
   %page = $LB::serverPath @ "sendscore.php";
   %mission = stripNot(strlwr(fileBase($Server::MissionFile)), "abcdefghijklmnopqrstuvwxyz0123456789");
   //HiGuy: $CurrentGame doesn't update correctly in SuperChallenge
   %gameType = LBGetGameMode($Server::MissionFile);
   %score = (%gameType $= "MultiPlayer" ? PlayGui.gemCount : $Game::ScoreTime);
   %passcode = 32577 * getRandom(1,10);
   %gg = ($LB::MarbleChoice $= "GGMarble");
   %query = LBDefaultQuery() @ "&score=" @ %score;
   %query = %query @ "&level=" @ %mission @ "&gametype=" @ %gameType @ "&passcode=" @ %passcode @ "&gg=" @ %gg @ ($LB::SuperChallengeMode ? "&num=" @ $LB::SuperChallengeLevel : "") @ ($LB::ChallengeMode ? "&num=" @ $LB::Challenge::CurrentAttempts : "");
   LBSendScoreNetwork.post(%server,%page,%query);
}

function LBSendScoreNetwork::onLine(%this,%line) {
   //echo(%line);
   Parent::onLine(%this,%line);
   %this.parseSigs(%line);
   if (getWord(%line,0) $= "SIG") {
      switch (getWord(%line,1)) {
         case 15: // Jeff: success!
            %this.echo("Score Sent!");

         // Jeff: for challenge mode end the game if we can.
         if ($LB::ChallengeMode) {
            if ($LB::Challenge::Type !$= "race" && $LB::Challenge::ScoreHelper)
               LBEndChallenge();
         }
      }
   } else if (getWord(%line,0) $= "RATING") {
      // Jeff: once we get rating, show it
      $LB::Rating = getWords(%line,1);
      devecho("Got rating! Which is:" SPC $LB::Rating);

      reformatGameEndText(); //HiGuy: Display on endgame
   } else if (getWord(%line,0) $= "NEWRATING") {
      // Jeff: once we get rating, show it
      $LB::TotalRating = getWords(%line,1);
      devecho("Got total rating! Which is:" SPC $LB::TotalRating);

      reformatGameEndText(); //HiGuy: Display on endgame
   }
}

//-----------------------------------------------------------------------------
// Jeff: EASTER EGG!
//-----------------------------------------------------------------------------

function sendEasterEggToLB(%securityCode) {
   %mission = $Server::MissionFile;
   if ($LB::LoggedIn && $LB::username !$= "" && $Game::Running && !$LB::Guest) {
      // Jeff: small anti hack, but better than nothing
      if ($missionCRC !$= getFileCRC(%mission) || %securityCode != 47392) {
         LBAssert("Warning!","Something went wrong with sending the leaderboard easter egg.  Sorry for the inconvience.  If this problem persists contact the Platinum team for support.");
         return;
      }
      devecho("Sending easter egg.........");
      %server = $LB::server;
      %page = $LB::serverPath @ "easteregg.php";
      %mission = stripNot(strlwr(fileBase(%mission)), "abcdefghijklmnopqrstuvwxyz0123456789");
      %passcode = 41933 * getRandom(1,10); // Jeff: anti hack
      %query = LBDefaultQuery() @ "&passcode=" @ %passcode @ "&level=" @ %mission;
      if (!isObject(LBEasterEggNetwork))
         new TCPObject(LBEasterEggNetwork);
      LBEasterEggNetwork.post(%server,%page,%query);
   }
}

function LBEasterEggNetwork::onLine(%this,%line) {
   Parent::onLine(%this,%line);
   %this.parseSigs(%line);
   if (getWord(%line,0) $= "SIG") {
      switch (getWord(%line,1)) {
         case 11: // Jeff: missing level
            %this.echo("No Level Found!");
         case 15: // Jeff: success!
            %this.echo("Easter Egg Sent!");
            // Jeff: update the client list
            cancel($LB::getEasterEggsSchedule);
            $LB::getEasterEggsSchedule = schedule(750, 0, "getLBEasterEggs");
      }
   }
}

// Jeff: get the easter eggs (for pickup messages)
function getLBEasterEggs() {
   while (isObject(LBEasterEggArray))
      LBEasterEggArray.delete();
   Array(LBEasterEggArray);
   %server = $LB::server;
   %page = $LB::serverPath @ "geteastereggs.php";
   %query = LBDefaultQuery();
   if (!isObject(LBGetEasterEggsNetwork))
      new TCPObject(LBGetEasterEggsNetwork);
   LBGetEasterEggsNetwork.post(%server, %page, %query);
}

function LBGetEasterEggsNetwork::onLine(%this, %line) {
   Parent::onLine(%this, %line);
   %this.parseSigs();

   if (getWords(%line, 0, 1) $= "SIG 1")
      %this.destroy(); //HiGuy: Goodbye

   if (getWords(%line, 0, 1) $= "SIG 20")
      %this.echo("EGG LIST FINISHED");

   // Jeff: easter egg received
   if (getWord(%line, 0) $= "EGG") {

      %egg = URLDecode(getWord(%line, 1));
      %dif = getWord(%line, 2);
      %cat = "lbmissions" @ getWord(%line, 3);
      if (getWord(%line, 3) $= "custom") {
         //HiGuy: Custom missions are different
         %egg = LBGetMissionFile(%egg);
         %egg = alphaNum(%egg);
      } else
         %egg = $usermods @ "data" @ %cat @ %dif @ %egg @ "mis";
      //devecho("EGG FILE:" SPC %egg);
      LBEasterEggArray.addEntry(%egg);
   }
}
