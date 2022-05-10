//--------------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Additional code for Marble Blast Platinum by Matan Weissman and Alex Swanson
// Old Leaderboards coding by Spy47
// New Leaderboards coding by Jeff and HiGuy
//--------------------------------------------------------------------------------

//HiGuy: How many highscores should be shown?
$Game::HighscoreCount = 5;

//--------------------------------------------------------------------------------
// Game start / end events sent from the server
//--------------------------------------------------------------------------------

function clientCmdGameStart() {
   //HiGuy: I know this function is probably used somewhere else...
   if ($LB::SuperChallengeMode)
      LBSCD_UpdateProgress();

   $Server::Preloaded = false;
   $Server::Preloading = false;
}

//HiGuy: Called when you respawn
function clientCmdGameRespawn() {
   if ($LB::LoggedIn && $LB::SuperChallengeMode)
      $LB::SuperAllTime = PlayGui.allTime;
}

function getBestTimes(%mis)
{
	%mis = strreplace(%mis, "lbmission", "mission");
	echo("Getting highscores for" SPC %mis);
   for(%i = 0; %i < $Game::HighscoreCount; %i++)
   {
      $hs[%i] = $pref::highScores[%mis, %i];
      if($hs[%i] $= "")
      {
			if (MissionInfo.time)
				$hs[%i] = MissionInfo.time @ "\tMatan W.";
			else
				$hs[%i] = "5998999\tMatan W.";
      }
   }
}

function clientCmdGameEnd()
{
   if($playingDemo)
      return;

   //HiGuy: Multiplayer has its own things
   if ($Server::ServerType $= "MultiPlayer") {
      Canvas.pushDialog(MPEndGameDlg);
      return;
   }

	getBestTimes($Client::MissionFile);

   $highScoreIndex = "";
   for(%i = 0; %i < $Game::HighscoreCount; %i++)
   {
   	echo("Checking score" SPC %i SPC $Game::ScoreTime SPC "?<" SPC getField($hs[%i], 0));
      if($Game::ScoreTime < getField($hs[%i], 0))
      {
	   	echo("Score" SPC %i SPC $Game::ScoreTime SPC "<" SPC getField($hs[%i], 0));
         for(%j = $Game::HighscoreCount - 1; %j > %i; %j--)
         {
            $hs[%j] = $hs[%j - 1];
         }
         $highScoreIndex = %i;
         $hs[%i] = $Game::ScoreTime @ "\t" @ $pref::highScoreName;
         break;
      }
   }
   if ($LB::LoggedIn) {
      if ($LB::SuperChallengeMode) {
         if (!$Game::Qualified) {
            restartLevel();
            return;
         }

         highScoreNameChanged();
         highScoreNameAccept();

         //HiGuy: Increment this, but remove the addition of previous times
         $LB::SuperTotalTime += PlayGui.totalTime - $LB::SuperTotalTime;
         //HiGuy: All time at the start of the level
         $LB::SuperAllTime = PlayGui.allTime;
         $LB::SuperChallengeTime[$LB::SuperChallengeLevel] = PlayGui.elapsedTime;

         $LB::SuperChallengeLevel ++;
         $LB::SuperChallengeTotalTime += $Game::ScoreTime;

         //HiGuy: Update the progress damnit
         LBSCD_SendUpdateScore();
         LBSCD_UpdateProgress();

         if ($LB::SuperChallengeLevel == LBSCD_GetMissionList().rowCount()) {
            // Jeff: this is the end of the super challenges and no higuy,
            //       not everybody is a winner like you had coded previously
            //HiGuy: D=
            if ($LB::SuperChallengePractice) {
               LBSCS_Clear();
               LBSCD_PushFinish6();
               return;
            }

            // Jeff: not practice, the real deal, not practice
            LBSCD_Winner();

            //HiGuy: SEND DAMNIT
            schedule(4000, 0, LBSCD_Winner);
            //HiGuy: This is causing the endgame screen to show too soon!
            //LBSCD_PushFinish();
            return;
         }

         // Jeff/HiGuy: play the next mission in super challenges
         %nextMission = getField(LBSCD_GetMissionList().getRowTextById($LB::SuperChallengeLevel), 0);
         LBStartMission(%nextMission, 5, true);
         $LB::SuperChallengeMode = true;
         return;
      } else if ($LB::ChallengeMode) { // Jeff: challenge mode
         // Jeff: sometimes the best score does not get sent, so we have to
         // do this in another place
         //LBEndChallenge();
         sendChallengeGemUpdates();
         return;
      } else if ($LBPref::SelectedRow < MissionInfo.level)
         $LBPref::SelectedRow ++;
         //HiGuy: We shouldn't change this if they're in a challenge
   }
   reformatGameEndText();
   EndGameGui.loadNextBitmap();

   Canvas.pushDialog(EndGameGui);
   if($highScoreIndex !$= "")
   {
      // Jeff: modify highscore system to support 5
      if ($highScoreIndex == 0)
         %msgIn = "";
      else if ($highScoreIndex == 1)
         %msgIn = " second";
      else if ($highScoreIndex == 2)
         %msgIn = " third";
      else if ($highScoreIndex == 3)
         %msgIn = " fourth";
      else
         %msgIn = " fifth";

      // Jeff: fix phil's system for "nil" entries.
      EnterNameAcceptButton.setActive($pref::HighScoreName !$= "");

      EnterNameText.setText("<just:center><shadow:1:1><shadowcolor:7777777F><color:FFFFFF><font:DomCasualD:48>Well Done!\n<font:DomCasualD:32>You have the" @ %msgIn @ " top time!");
      Canvas.pushDialog(EnterNameDlg);
      EnterNameEdit.setSelectionRange(0, 100000);
   }
}

function highScoreNameAccept()
{
   Canvas.popDialog(EnterNameDlg);

   // Jeff: let LB missions save times for offline!
   %file = strreplace($Client::MissionFile, "lbmission", "mission");

   for(%i = 0; %i < $Game::HighscoreCount; %i++) {
      // Jeff: save lb times!
      if ($LB::LoggedIn && $LB::username !$= "")
         $pref::HighScoreName[$Client::MissionFile, %i] = $hs[%i];
		$pref::highScores[%file, %i] = $hs[%i];
   }
	//HiGuy: Save prefs
	savePrefs(true);
}

function highScoreNameChanged()
{
	// Phil - prevent nil name entries
	// Jeff: phil you need to check it before onWake!
	if ($pref::highScoreName $= "")
		EnterNameAcceptButton.setActive(false);
	else
		EnterNameAcceptButton.setActive(true);

	$hs[$highScoreIndex] = $Game::ScoreTime @ "\t" @ (($LB::LoggedIn) && ($LB::SuperChallengeMode || $LB::ChallengeMode) ? $LB::username : $pref::highScoreName);
   reformatGameEndText();
}

// Spy47 : Fuck Natural Logarithms, I'm making my own handy mathematical functions here.
function mLog10(%n) {
	return (mLog(%n) / mLog(10));
}
// Spy47 : I'm not using this function but I made it because...I don't know why.
function mLogb(%n,%b) {
	return (mLog(%n) / mLog(%b));
}

function reformatGameEndText()
{
   // Clear everything first
   EG_Result.setText("");
   EG_Description.setText("");
   EG_1stLine.setText("");
   EG_2ndLine.setText("");
   EG_3rdLine.setText("");

	// -------------------------------------------------------------------------------
	// Final Time
	// -------------------------------------------------------------------------------
	EG_TitleText.setText("<font:DomCasualD:64><color:FFFFFF><shadow:1:1><shadowcolor:777777>Your Time:");
	EG_TopThreeText.setText("<font:DomCasualD:32><color:FFFFFF><shadow:1:1><shadowColor:777777>Top " @ $Game::HighscoreCount @ " Times:");
	EG_Result.setText("<font:DomCasualD:64><color:FFFFFF><shadow:1:1><shadowcolor:777777><just:right>" @ formatTime($Game::ScoreTime));

	// -------------------------------------------------------------------------------
	// Decision on which Qualification message to display
	// -------------------------------------------------------------------------------
	%text = "<color:FFFFFF><shadow:1:1><shadowcolor:7777777F><just:center><font:DomCasualD:32>";

	if (MissionInfo.ultimateTime && $Game::ScoreTime < MissionInfo.ultimateTime)
		%text = %text @ "You beat the <color:FFDD22><shadowcolor:AA99227F>Ultimate<color:FFFFFF><shadowcolor:7777777F> Time!";

	else if (MissionInfo.goldTime && $Game::ScoreTime < MissionInfo.goldTime)
	{
		if ($CurrentGame $= "Gold" && $MissionType !$= "Custom")
			%text = %text @ "You beat the <color:FFCC00><shadowcolor:9966007F>Gold<color:FFFFFF><shadowcolor:7777777F> Time!";
		else
			%text = %text @ "You beat the <color:CCCCCC><shadowcolor:5555557F>Platinum<color:FFFFFF><shadowcolor:7777777F> Time!";
	}
	else if ($Game::Qualified)
		%text = %text @ "You beat the Par Time!";
	else
		%text = %text @ "<color:f55555><shadowcolor:800000>You didn\'t pass the Par Time!";

	// -------------------------------------------------------------------------------
	// Time stats from Mission
	// -------------------------------------------------------------------------------

	%text = %text @ "<font:DomCasualD:9>\n\n<tab:208><color:FFFFFF><font:DomCasualD:24><shadowcolor:7777777F><just:left>";

	if (MissionInfo.time)
		%text = %text @ "<just:left>Par Time:\t<just:right>" @ formatTime(MissionInfo.time) @ "\n";
	else
		%text = %text @ "<just:left>Par Time:\t<just:right>99:59.99\n";

	if(MissionInfo.goldTime)
	{
		if ($CurrentGame $= "Gold" && $MissionType !$= "Custom")
			%text = %text @ "<just:left><shadowcolor:AA88007F><color:FFCC00>Gold Time:\t<just:right>" @ formatTime(MissionInfo.goldTime) @ "\n";
		else
			%text = %text @ "<just:left><shadowcolor:5555557F><color:CCCCCC>Platinum Time:\t<just:right>" @ formatTime(MissionInfo.goldTime) @ "\n";
	}

	if(MissionInfo.UltimateTime)
		%text = %text @ "<just:left><shadowcolor:AA99227F><color:FFDD22>Ultimate Time:\t<just:right>" @ formatTime(MissionInfo.UltimateTime) @ "\n";

	%text = %text @ "<font:DomCasualD:9>\n\n<color:FFFFFF><font:DomCasualD:24><shadowcolor:7777777F>";

	%text = %text @
	"<just:left>Time Passed:<just:right>" @ formatTime($Game::ElapsedTime) @ "\n" @
		"<just:left>Clock Bonuses:<just:right>" @ formatTime($Game::BonusTime) @ "\n";

   // Jeff: you can't be a guest to get rating.
	if ($LB::Username !$= "" && $LB == 1 && !$LB::Guest) {
	   %text = %text @
	   "<just:left>Rating:<just:right>" @ formatRating($LB::Rating) @ "\n";
	   %text = %text @
	   "<just:left>General Rating:<just:right>" @ formatRating($LB::TotalRating) @ "\n";
	}

	// Display the left-side text
	EG_Description.setText(%text);

	// -------------------------------------------------------------------------------
	// Grab the times from the preferences file
	// -------------------------------------------------------------------------------

	for(%i = 0; %i < $Game::HighscoreCount; %i++)
	{
		%time = getField($hs[%i], 0);
		%name = getField($hs[%i], 1);

		%scoreText = "<shadow:1:1><font:DomCasualD:24>";

		switch(%i)
		{
			case 0:
				%scoreText = %scoreText @ "<color:eec884><shadowcolor:816d48>1. ";
			case 1:
				%scoreText = %scoreText @ "<color:cdcdcd><shadowcolor:7e7e7e>2. ";
			case 2:
				%scoreText = %scoreText @ "<color:c9afa0><shadowcolor:7f6f65>3. ";
			case 3:
				%scoreText = %scoreText @ "<color:a4a4a4><shadowcolor:7e7e7e>4. ";
			case 4:
				%scoreText = %scoreText @ "<color:949494><shadowcolor:7f6f65>5. ";
		}
		%scoreText = %scoreText @ "<shadowcolor:7777777F><color:FFFFFF>" @ %name @ "\t<just:right>";

		if (%time < MissionInfo.staffTime)
		{
			%scoreText = %scoreText @ "<shadowcolor:AA44447F><color:FF4444>";
		}
		else if (%time < MissionInfo.ultimateTime)
		{
			%scoreText = %scoreText @ "<shadowcolor:AA99227F><color:FFDD22>";
		}
		else if (%time < MissionInfo.goldTime)
		{
			if($CurrentGame $= "Gold" && $MissionType !$= "Custom")
				%scoreText = %scoreText @ "<shadowcolor:AA88007F><color:FFCC00>";
			else
				%scoreText = %scoreText @ "<shadowcolor:5555557F><color:CCCCCC>";
		}

		%scoreText = %scoreText @ formatTime(%time);

		switch(%i + 1)
		{
			case 1:
				EG_1stLine.setText(%scoreText);

			case 2:
				EG_2ndLine.setText(%scoreText);

			case 3:
				EG_3rdLine.setText(%scoreText);

			case 4:
				EG_4thLine.setText(%scoreText);

			case 5:
				EG_5thLine.setText(%scoreText);
		}
   }

	// Display the left-side text
   EG_Description.setText(%text);
}

//-----------------------------------------------------------------------------

function formatTime(%time)
{
   %isNeg = "\t";
   if (%time < 0)
   {
      %time = -%time;
      %isNeg = "\t-";
   }
   %hundredth = mFloor((%time % 1000) / 10);
   %totalSeconds = mFloor(%time / 1000);
   %seconds = %totalSeconds % 60;
   %minutes = (%totalSeconds - %seconds) / 60;

   %secondsOne   = %seconds % 10;
   %secondsTen   = (%seconds - %secondsOne) / 10;
   %minutesOne   = %minutes % 10;
   %minutesTen   = (%minutes - %minutesOne) / 10;
   %hundredthOne = %hundredth % 10;
   %hundredthTen = (%hundredth - %hundredthOne) / 10;

   return %isNeg @ %minutesTen @ %minutesOne @ ":" @
       %secondsTen @ %secondsOne @ "." @
       %hundredthTen @ %hundredthOne;
}

function formatTimeSeconds(%time)
{
   %isNeg = "\t";
   if (%time < 0)
   {
      %time = -%time;
      %isNeg = "\t-";
   }
   %totalSeconds = mFloor(%time / 1000);
   %seconds = %totalSeconds % 60;
   %minutes = (%totalSeconds - %seconds) / 60;

   %secondsOne   = %seconds % 10;
   %secondsTen   = (%seconds - %secondsOne) / 10;
   %minutesOne   = %minutes % 10;
   %minutesTen   = (%minutes - %minutesOne) / 10;

   return %isNeg @ %minutesTen @ %minutesOne @ ":" @
       %secondsTen @ %secondsOne;
}

function formatTimeHours(%time)
{
	%hours = mFloor(%time / 3600);
	%minutes = mFloor(%time / 60) - (%hours * 60) - (%days * 1440);
	%seconds = %time - (%minutes * 60) - (%hours * 3600) - (%days * 86400);

	%secondsOne   = %seconds % 10;
    %secondsTen   = mFloor(%seconds / 10);
    %minutesOne   = %minutes % 10;
    %minutesTen   = mFloor(%minutes / 10);
	%hoursOne	  = %hours % 10;
	%hoursTen     = mFloor(%hours / 10);

	return %hoursTen @ %hoursOne @ ":" @
		%minutesTen @ %minutesOne @ ":" @
		%secondsTen @ %secondsOne;
}

function formatTimeHoursMs(%time)
{
	%hours = mFloor(mFloor(%time / 1000) / 3600);
	%minutes = mFloor(mFloor(%time / 1000) / 60) - (%hours * 60) - (%days * 1440);
	%seconds = mFloor(%time / 1000) - (%minutes * 60) - (%hours * 3600) - (%days * 86400);
   %hundredth = mFloor((%time % 1000) / 10);

	%secondsOne   = %seconds % 10;
   %secondsTen   = mFloor(%seconds / 10);
   %minutesOne   = %minutes % 10;
   %minutesTen   = mFloor(%minutes / 10);
	%hoursOne	  = %hours % 10;
	%hoursTen     = mFloor(%hours / 10);
   %hundredthOne = %hundredth % 10;
   %hundredthTen = (%hundredth - %hundredthOne) / 10;

	return (%hours > 0 ? (%hoursTen > 0 ? %hoursTen : "") @ %hoursOne @ ":" : "") @
		%minutesTen @ %minutesOne @ ":" @
		%secondsTen @ %secondsOne @ "." @
		%hundredthTen @ %hundredthOne;
}

function formatTimeDays(%time)
{
    %days = $pref::TotalTimerDaysAdd;
	%hours = mFloor(%time / 3600);
	%minutes = mFloor(%time / 60) - (%hours * 60);
	%seconds = %time - (%minutes * 60) - (%hours * 3600);

	%secondsOne   = %seconds % 10;
    %secondsTen   = mFloor(%seconds / 10);
    %minutesOne   = %minutes % 10;
    %minutesTen   = mFloor(%minutes / 10);
	%hoursOne	  = %hours % 10;
	%hoursTen     = mFloor(%hours / 10);
	%daysOne	  = %days % 10;
	%daysTen      = mFloor(%days / 10);

	return %daysTen @ %daysOne @ ":" @
	    %hoursTen @ %hoursOne @ ":" @
		%minutesTen @ %minutesOne @ ":" @
		%secondsTen @ %secondsOne;
}

function ActionMap::isPushed(%this) {
   if (!isObject(%this))
      return false;
   for (%i = 0; %i < ActiveActionMapSet.getCount(); %i ++) {
      if (ActiveActionMapSet.getObject(%i).getId() == %this.getId())
         return true;
   }
   return false;
}

function formatCommas(%number) {
   %fin = "";
   %c = -1;
   for (%i = strlen(%number); %i >= 0; %i --) {
      if (%c % 3 == 0 && %c > 0)
         %fin = "," @ %fin;
      %fin = getSubStr(%number, %i, 1) @ %fin;
      %c ++;
   }
   return %fin;
}

function formatScore(%score) {
   return "\t" @ formatCommas(%score);
}

function formatRating(%rating) {
   //HiGuy: Error Messages
   if (%rating == -1)    return "Level Error";   // Level not found
   if (%rating == -2)    return "Invalid Time";  // Score too low...
   if (%rating == -3)    return "Submitting..."; // Submitting score
   if (%rating == -4)    return "Still a WIP";   // Multiplayer Ratings
   if (%rating == -5)    return "Still a WIP";   // Other multiplayer stuffs
   if (%rating $= "INF") return "Server Error";  // The crap?

   return formatCommas(%rating);
}
