//-----------------------------------------------------------------------------
// Master Server - Version 3.0
// By HiGuy
// Server-sided code by HiGuy as well
//-----------------------------------------------------------------------------

function masterStartGame() {
   //Send-lock
   if (masterIsSending())
      return;

   //Don't send this if the game isn't running
   if (!$Game::Running && !$Server::Lobby)
      return;

   if ($Game::Running) {
      //Grab variables for query
      %name       = $MPPref::Server::Name;
      %level      = MissionInfo.name;
      %mode       = $MP::Teammode;
	   %players    = getTotalPlayerCount() - max(0, $Server::SpectateCount);
      %maxplayers = $pref::Server::MaxPlayers;
      %password   = $MPPref::ServerPassword !$= "";
      %submitting = $MPPref::CalculateScores;
      %display    = $MPPref::DisplayOnMaster;
      %dedicated  = $Server::Dedicated;
      %version    = $MP::RevisionOn;
      %dev        = false;
      %mod        = $Master::Mod;
      %port       = $pref::Server::Port;
      %os         = $platform;
      %info       = URLEncode($MPPref::Server::Info);
      %handicap   = serverGetHandicaps();
      %host       = $LB::Username;
   } else if ($Server::Lobby) {
      //Grab variables for query
      %name       = $MPPref::Server::Name;
      %level      = "Lobby";
      %mode       = $MP::Teammode;
      %players    = getTotalPlayerCount() - max(0, $Server::SpectateCount);
      %maxplayers = $pref::Server::MaxPlayers;
      %password   = $MPPref::ServerPassword !$= "";
      %submitting = $MPPref::CalculateScores;
      %display    = $MPPref::DisplayOnMaster;
      %dedicated  = $Server::Dedicated;
      %version    = $MP::RevisionOn;
      %dev        = false;
      %mod        = $Master::Mod;
      %port       = $pref::Server::Port;
      %os         = $platform;
      %info       = URLEncode($MPPref::Server::Info);
      %handicap   = serverGetHandicaps();
      %host       = $LB::Username;
   }

   //Generate query data
   %data = "name="       @ %name       @
          "&level="      @ %level      @
          "&mode="       @ %mode       @
          "&players="    @ %players    @
          "&maxPlayers=" @ %maxplayers @
          "&password="   @ %password   @
          "&submitting=" @ %submitting @
          "&display="    @ %display    @
          "&dedicated="  @ %dedicated  @
          "&version="    @ %version    @
          "&dev="        @ %dev        @
          "&mod="        @ %mod        @
          "&port="       @ %port       @
          "&os="         @ %os         @
          "&info="       @ %info       @
          "&handicap="   @ %handicap   @
          "&hoster="     @ %host       ;

   //Create the TCP request
   if (!isObject(MasterStarter))
      new TCPObject(MasterStarter);
   %server = $Master::Server;
   %page = $Master::Path @ "startgame.php";
   MasterStarter.post(%server, %page, %data);
   MasterStarter.keyReceived = false;

   cancel($Master::Heartbeat);

   $Master::Active = false;
   $Master::Sending = true;
}

function MasterSender::onRetrySend(%this) {

}

function MasterStarter::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   if (getSubStr(%line, 0, 4) $= "ERR:") {
      //Parse error code

      %next = strPos(%line, ":", 4);
      if (%next == -1) {
         %errCode = 0;
         %error = getSubStr(%line, 4, strlen(%line));
      } else {
         %errCode = getSubStr(%line, 4, %next - 4);
         %error = getSubStr(%line, %next + 1, strlen(%line));
      }

      //Success error code
      if (%errCode == -1) {
         //Do the success things here!
         if (!%this.keyReceived) {
            %this.keyReceived = true;
            $Master::Key = %error;
            %this.echo("Received key from server:" SPC %error);
            $Master::Active = true;
         }

         //echo("HEARTBEAT WILL BE HAPPENING IN" SPC $Master::UpdateFreq);
         cancel($Master::Heartbeat);
         $Master::Heartbeat = schedule($Master::UpdateFreq, 0, masterHeartbeat);
      } else {
         //Do error things here
         %this.echo("Server returned error: (" @ %errCode @ ")" SPC %error);

         cancel($Master::Heartbeat);
         //echo("HEARTBEAT WILL BE HAPPENING IN" SPC $Master::RestartFreq);
         $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterStartGame);
      }
   }
   if (%line $= "FINISH") {
      $Master::Sending = false;
      if ($Master::EndQueued) {
         //There's little chance of this happening before the server has even
         //started, but we want to cover all corner cases!
         $Master::EndQueued = false;
         masterEndGame();
      } else if ($Master::CalcQueued) {
         //Oh snaps!
         $Master::CalcQueued = false;
         masterCalcScores();
      }
   }
}

function masterEndGame() {
   //error("We\'re taking ourselves off the master!");
   //backtrace();

   //Don't end if we never started!
   if (!$Master::Active)
      return;

   //If the game is running, don't end it!
   if ($Game::Running)
      return;

   //If we're sending, queue the end, as it's more important than heartbeats
   if (masterIsSending()) {
      $Master::EndQueued = true;
      return;
   }

   //All we need is a key
   %key = $Master::Key;

   //Generate query data
   %data = "key=" @ %key;
   //Create the TCP request
   if (!isObject(MasterEnder))
      new TCPObject(MasterEnder);
   %server = $Master::Server;
   %page = $Master::Path @ "endgame.php";
   //Spoiler: Ender kills the Formics
   MasterEnder.post(%server, %page, %data);

   //echo("HEARTBEAT BE CANCELLING");
   cancel($Master::Heartbeat);
   $Master::Sending = true;
}

function MasterEnder::onRetrySend(%this) {

}

function MasterEnder::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   if (getSubStr(%line, 0, 4) $= "ERR:") {
      //Parse error code

      %next = strPos(%line, ":", 4);
      if (%next == -1) {
         %errCode = 0;
         %error = getSubStr(%line, 4, strlen(%line));
      } else {
         %errCode = getSubStr(%line, 4, %next - 4);
         %error = getSubStr(%line, %next + 1, strlen(%line));
      }

      //Success error code
      if (%errCode == -1) {
         //Do the success things here!
         %this.echo("Game ended successfully with message:" SPC %error);
         $Master::Active = false;
      } else {
         //Do error things here
         %this.echo("Server returned error: (" @ %errCode @ ")" SPC %error);

         switch (%errCode) {
         case 4 or 5:
            //Server not running!
            //Nothing to do here, the server has already ended!

            $Master::Active = false;
         case 3:
            cancel($Master::Heartbeat);
            $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterEndGame);
         default:
         }
      }
   }
   if (%line $= "FINISH")
      $Master::Sending = false;
}

function masterHeartbeat() {
   //Send-lock
   if (!$Master::Active && $Master::Key $= "") {
      devecho("Ah triyed to give u a hartbeet ah reely ded");
      return;
   }

   if (!$Game::Running && !$Server::Lobby) {
      devecho("Ah triyed to give u a hartbeet ah reely ded 2");
      masterEndGame();
      return;
   }

   if ($Game::Running) {
      //Grab variables for query
      %name       = $MPPref::Server::Name;
      %level      = MissionInfo.name;
      %mode       = $MP::Teammode;
      %players    = getTotalPlayerCount() - max(0, $Server::SpectateCount);
      %maxplayers = $pref::Server::MaxPlayers;
      %password   = $MPPref::ServerPassword !$= "";
      %submitting = $MPPref::CalculateScores;
      %display    = $MPPref::DisplayOnMaster;
      %port       = $pref::Server::Port;
      %info       = URLEncode($MPPref::Server::Info);
      %handicap   = serverGetHandicaps();
      %host       = $LB::Username;
      %key        = $Master::Key;
   } else if ($Server::Lobby) {
      //Grab variables for query
      %name       = $MPPref::Server::Name;
      %level      = "Lobby";
      %mode       = $MP::Teammode;
      %players    = getTotalPlayerCount() - max(0, $Server::SpectateCount);
      %maxplayers = $pref::Server::MaxPlayers;
      %password   = $MPPref::ServerPassword !$= "";
      %submitting = $MPPref::CalculateScores;
      %display    = $MPPref::DisplayOnMaster;
      %port       = $pref::Server::Port;
      %info       = URLEncode($MPPref::Server::Info);
      %handicap   = serverGetHandicaps();
      %host       = $LB::Username;
      %key        = $Master::Key;
   }

   //Generate query data
   %data = "name="       @ %name       @
          "&level="      @ %level      @
          "&mode="       @ %mode       @
          "&key="        @ %key        @
          "&players="    @ %players    @
          "&maxPlayers=" @ %maxplayers @
          "&password="   @ %password   @
          "&submitting=" @ %submitting @
          "&display="    @ %display    @
          "&port="       @ %port       @
          "&info="       @ %info       @
          "&handicap="   @ %handicap   @
          "&hoster="     @ %host       ;

   //Generate player query data
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %player = ClientGroup.getObject(%i);
      %skin = MPMarbleList.findTextIndex(%player.skinChoice);
      %data = %data @ "&player[]=" @ %player.nameBase;
      %data = %data @ "&score[]=" @ %player.gemCount;
      %data = %data @ "&place[]=" @ GameConnection::getPlace(%player);
      %data = %data @ "&host[]=" @ %player.isHost();
      %data = %data @ "&marble[]=" @ %skin;
      %data = %data @ "&gems[1][]=" @ %player.gemsFound[1];
      %data = %data @ "&gems[2][]=" @ %player.gemsFound[2];
      %data = %data @ "&gems[5][]=" @ %player.gemsFound[5];
      if ($MP::TeamMode)
         %data = %data @ "&team[]=" @ %player.team.number;
      else
         %data = %data @ "&team[]=-1";
   }

   //Create the TCP request
   if (!isObject(MasterBeater))
      new TCPObject(MasterBeater);
   %server = $Master::Server;
   %page = $Master::Path @ "heartbeat.php";
   MasterBeater.post(%server, %page, %data);

   //echo("HEARTBEAT BE HAPPENING");
   cancel($Master::Heartbeat);
   $Master::Sending = true;
}

function MasterBeater::onRetrySend(%this) {
   //echo("HEARTBEAT BE RETRYING");
}

function MasterBeater::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   if (getSubStr(%line, 0, 4) $= "ERR:") {
      //Parse error code

      %next = strPos(%line, ":", 4);
      if (%next == -1) {
         %errCode = 0;
         %error = getSubStr(%line, 4, strlen(%line));
      } else {
         %errCode = getSubStr(%line, 4, %next - 4);
         %error = getSubStr(%line, %next + 1, strlen(%line));
      }

      //Success error code
      if (%errCode == -1) {
         //Do the success things here!
         //$Master::Key = %error;
         %this.echo("Heartbeat successful with message:" SPC %error);
         $Master::Active = true;

         cancel($Master::Heartbeat);
         //echo("HEARTBEAT BE HAPPENING IN" SPC $Master::UpdateFreq);
         $Master::Heartbeat = schedule($Master::UpdateFreq, 0, masterHeartbeat);

         %this.clearLines();
      } else {
         //Do error things here
         %this.echo("Server returned error: (" @ %errCode @ ")" SPC %error);

         switch (%errCode) {
         case 4 or 5:
            //Server not running!
            $Master::Active = false;

            if ($Server::Hosting) {
               //echo("HEARTBEAT BE HAPPENING IN" SPC $Master::RestartFreq);
               cancel($Master::Heartbeat);
               $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterStartGame);
            }
         case 3:
            cancel($Master::Heartbeat);
            //echo("HEARTBEAT BE HAPPENING IN" SPC $Master::RestartFreq);
            $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterHeartbeat);
         default:
         }

         %this.clearLines();
      }
   }
   if (%line $= "FINISH") {
      $Master::Sending = false;
      if ($Master::EndQueued) {
         $Master::EndQueued = false;
         masterEndGame();
      }
   }
}

function masterIsSending() {
   return $Master::Sending && (isObject(MasterSender) || isObject(MasterBeater));
}

function masterCalcScores(%id) {
	error("Calc-ing!");
	backtrace();
	if ($Server::Dedicated) {
		//We need to tell the host client to send the scores
		%host = GameConnection::getHost();
		if (getRealClientCount() > 0 && isObject(%host)) {
			%host.dsubmitScores();
		} else {
			//Save the score for the next guy
			$Master::PendingScores ++;
			echo("TODO: Send score");
			//TODO
		}
	}

	if ($Server::_Dedicated) {
		%data = $Server::SubmitData;
	} else {
		if (!$Master::Active) {
			error("MASTER IS NOT ACTIVE CANNOT CALC SCORES");
			return false;
		}

		//Grab variables for query
		%key     = $Master::Key;
		%level   = fileBase($Server::MissionFile);
		%players = getTotalPlayerCount() - max(0, $Server::SpectateCount);
		%port    = $pref::Server::Port;
		%modes   = trim(($MPPref::Server::MatanMode ? "matan " : "") @ ($MPPref::Server::GlassMode ? "glass " : ""));

		//Generate query data
		%data = "key="     @ %key     @
				 "&level="   @ %level   @
				 "&players=" @ %players @
				 "&port="    @ %port    @
				 "&modes="   @ %modes   ;

		//Generate player query data
		for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
			%player = ClientGroup.getObject(%i);
			//HiGuy: No spectators!
			if (%player.spectating && %player.gemCount == 0)
				continue;
			%skin = MPMarbleList.findTextIndex(%player.skinChoice);
			%data = %data @ "&player[]=" @ %player.nameBase;
			%data = %data @ "&score[]=" @ %player.gemCount;
			%data = %data @ "&place[]=" @ GameConnection::getPlace(%player);
			%data = %data @ "&host[]=" @ %player.isHost();
			%data = %data @ "&guest[]=" @ %player.isGuest();
			%data = %data @ "&handicap[]=" @ %player.getHandicap();
			%data = %data @ "&marble[]=" @ %skin;
			%data = %data @ "&gems[1][]=" @ %player.gemsFound[1];
			%data = %data @ "&gems[2][]=" @ %player.gemsFound[2];
			%data = %data @ "&gems[5][]=" @ %player.gemsFound[5];
			if ($MP::TeamMode)
				%data = %data @ "&team[]=" @ %player.team.number;
			else
				%data = %data @ "&team[]=-1";
		}
	}

   //Create the TCP request
   if (!isObject(MasterScorer))
      new TCPObject(MasterScorer);
   %server = $Master::Server;

   %page = $Master::Path @ ($Server::_Dedicated ? "dcalcscores.php" : "calcscores.php");
   MasterScorer.post(%server, %page, %data);
   MasterScorer.keyReceived = false;

   if ($Server::_Dedicated)
	   MasterScorer.submitId = %id;
   return true;
}

function MasterScorer::onRetrySend(%this) {

}

function MasterScorer::onLine(%this, %line) {
   Parent::onLine(%this, %line);

	if ($Server::_Dedicated) {
		//Tell the server!
		commandToServer('MasterScoreLine', %line, %this.submitId);
		return;
	}

   if (getSubStr(%line, 0, 4) $= "ERR:") {
      //Parse error code

      %next = strPos(%line, ":", 4);
      if (%next == -1) {
         %errCode = 0;
         %error = getSubStr(%line, 4, strlen(%line));
      } else {
         %errCode = getSubStr(%line, 4, %next - 4);
         %error = getSubStr(%line, %next + 1, strlen(%line));
      }

      //Success error code
      if (%errCode == -1) {
         //Do the success things here!
         %this.echo("Score sending successful with message:" SPC %error);
         //Send everyone the scores
         serverSendScores();
      } else if (%errCode $= "KEY") {
         if (!%this.keyReceived) {
            %this.keyReceived = true;
            $Master::Key = %error;
            %this.echo("Received key from server:" SPC %error);
            $Master::Active = true;
            $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterHeartbeat);
         }
      } else {
         //Do error things here
         %this.echo("Server returned error: (" @ %errCode @ ")" SPC %error);

         switch (%errCode) {
         case 4 or 5:
            //Server not running!
            $Master::Active = false;

            if ($Game::Running) {
               cancel($Master::Heartbeat);
               $Master::Heartbeat = schedule($Master::RestartFreq, 0, masterStartGame);
            }
         default:
         }
      }
   }
   if (getSubStr(%line, 0, 4) $= "SCR:") {
      //HiGuy: We're receiving a final score back from the server! Let's get
      // ourselves set up.

      %name = getSubStr(%line, 4, strlen(%line));
      $Master::ScorePlayer[$Master::Scores] = %name;
   }
   if (getSubStr(%line, 0, 4) $= "RAT:") {
      %rating = getSubStr(%line, 4, strlen(%line));
      $Master::ScoreRating[$Master::Scores] = %rating;

      GameConnection::resolveName($Master::ScorePlayer[$Master::Scores]).rating = %rating;
   }
   if (getSubStr(%line, 0, 4) $= "CHG:") {
      %change = getSubStr(%line, 4, strlen(%line));
      $Master::ScoreChange[$Master::Scores] = %change;
      $Master::Scores ++;
   }
   if (%line $= "SCORE DUMP") {
      //HERE THEY COME!
      deleteVariables("$Master::Score*");
      $Master::Scores = 0;
   }
   if (%line $= "FINISH") {
      //Hooray!
   }
}

function masterVerifySession(%client, %session) {
	if ($Server::Dedicated) {
		%client.completeValidation(true);
		return true;
	}

   //Grab variables for query
   %username = %client._name $= "" ? %client.namebase : %client._name;

   //Generate query data
   %data = "username=" @ %username @
          "&session="  @ %session  ;

   //Create the TCP request
   if (!isObject(MasterVerifyer))
      new TCPObject(MasterVerifyer);
   %server = $Master::Server;
   %page = $Master::Path @ "verify.php";
   MasterVerifyer.post(%server, %page, %data);
   MasterVerifyer.client = %client;
   //echo("VERIFYING" SPC %username);

   return true;
}

function MasterVerifyer::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   %this.echo(%line);
   //HiGuy: Invalid lines
   if (strPos(%line, ":") == -1)
      return;

   %client = %this.client;
   %username = %client._name $= "" ? %client.namebase : %client._name;

   //HiGuy: What does the master server want?
   %cmd  = getSubStr(%line, 0, strPos(%line, ":"));
   %rest = getSubStr(%line, strPos(%line, ":") + 1, strlen(%line));
   switch$ (%cmd) {
   case "ERR":
      //HiGuy: Uhhhhhhh. Shit
      //HiGuy: Should we let them in? Only if we're not calculating scores.
      %client.completeValidation($MPPref::CalculateScores, "VALID_FAIL");
   case "VERIFY":
      if (%client.provisional $= "")
         %client.provisional = false;
      switch$ (%rest) {
      case "SUCCESS":
         //HiGuy: Hooray!
         %client.completeValidation(true);
         //warn("Client" SPC %username SPC "passed validation.");
      case "FAIL":
         //HiGuy: Haha!
         %client.completeValidation($MPPref::CalculateScores, "VALID_FAIL");
         error("Client" SPC %username SPC "failed validation. They may not be who they say they are!");
      case "SUPERFAIL":
         //HiGuy: Oh shit!
         %client.completeValidation(false, "VALID_FAIL");
         error("Client" SPC %username SPC "failed validation. They had an invalid session and will be disconnected.");
      }
      updatePlayerList();
   case "RATING":
      %type = firstWord(%rest);
      %cont = restWords(%rest);

      switch$ (%type) {
      case "PTS":
         %client.rating = %cont;
      case "PROVISIONAL":
         %client.provisional = true;
      case "GAMES":
         %client.ratingGames = %cont;
      case "TEAMGAMES":
         %client.ratingTeamGames = %cont;
      case "TOTALGEMS":
         %client.ratingTotalGems = %cont;
      case "TOTALSCORE":
         %client.ratingTotalScore = %cont;
      case "WINSTREAK":
         %client.ratingWinStreak = %cont;
      }
   }
}

function masterPlayerInfo(%client) {
   if (!$Master::Active)
      return false;

   //Grab variables for query
   %username = %client._name $= "" ? %client.namebase : %client._name;

   //Generate query data
   %data = "username=" @ %username;

   //Create the TCP request
   if (!isObject(MasterInfoer))
      new TCPObject(MasterInfoer);
   %server = $Master::Server;
   %page = $Master::Path @ "playerinfo.php";
   MasterInfoer.post(%server, %page, %data);
   MasterInfoer.client = %client;
}

function MasterInfoer::onLine(%this, %line) {
   Parent::onLine(%this, %line);

   //HiGuy: Invalid lines
   if (strPos(%line, ":") == -1)
      return;

   %client = %this.client;

   //HiGuy: What does the master server want?
   %cmd  = getSubStr(%line, 0, strPos(%line, ":"));
   %rest = getSubStr(%line, strPos(%line, ":") + 1, strlen(%line));
   switch$ (%cmd) {
   case "ERR":
      //HiGuy: Uhhhhhhh. Shit
   case "RATING":
      %type = firstWord(%rest);
      %cont = restWords(%rest);

      switch$ (%type) {
      case "PTS":
         %client.rating = %cont;
      case "PROVISIONAL":
         %client.provisional = true;
      case "GAMES":
         %client.ratingGames = %cont;
      case "TEAMGAMES":
         %client.ratingTeamGames = %cont;
      case "TOTALGEMS":
         %client.ratingTotalGems = %cont;
      case "TOTALSCORE":
         %client.ratingTotalScore = %cont;
      case "WINSTREAK":
         %client.ratingWinStreak = %cont;
      }
   }
}
