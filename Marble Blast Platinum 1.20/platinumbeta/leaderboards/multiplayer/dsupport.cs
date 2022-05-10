//------------------------------------------------------------------------------
// Multiplayer Package
// dsupport.cs
// Copyright (c) 2014 MBP Team
//------------------------------------------------------------------------------

function loadInput() {
   if (getFileCRC($inputfile) !$= $last) {
      $last = getFileCRC($inputfile);
      %conts = trim(fread($inputfile));

      echo("$ " @ %conts);
      eval(%conts);
      echo(""); //So we get output
   }

   cancel($loadInput);
   $loadInput = schedule(100, 0, loadInput);
}

function printStatus() {
   //Things we need:
   // Level, player count, team mode, calculate, version, handicap, host

   //Level
   if ($Server::Lobby) %status = "Lobby";
   else %status = MissionInfo.name;

   //Player Count
   %status = %status NL getRealPlayerCount();

   //Team mode
   %status = %status NL $MP::Teammode;

   //Calculate
   %status = %status NL $MPPref::CalculateScores;

   //version
   %status = %status NL $MP::RevisionOn;

   //Handicap
   %status = %status NL serverGetHandicaps();

   //Host
   if (ClientGroup.getCount()) %status = %status NL ClientGroup.getObject(0).nameBase;
   else %status = %status NL "No Host";

   fwrite($outputfile, %status);

   cancel($printStatus);
   $printStatus = schedule(100, 0, printStatus);
}

function dedicatedUpdate() {
	if (getTotalPlayerCount() > 0) {
		for (%i = 0; %i < ClientGroup.getCount(); %i ++)
			if (!ClientGroup.getObject(%i).isFake())
				ClientGroup.getObject(%i).setHost(true);
	} else {
		//Cancel the preload
		lobbyReturn();
	}
}

function GameConnection::sendInfo(%this) {
	%this.sendMissionList();
}

function GameConnection::sendMissionList(%this) {
	commandToClient(%this, 'MissionListStart');

	//Find all levels and let them know
	%spec = "*/data/multiplayer/*.mis";
	for (%file = findFirstFile(%spec); %file !$= ""; %file = findNextFile(%spec)) {
		//Send them the file
		commandToClient(%this, 'MissionListFile', %file);
	}

	commandToClient(%this, 'MissionListEnd');
}

function serverCmdGetMissionInfo(%client, %file) {
	//Only the host needs to know this
	if (!%client.isHost())
		return;

   commandToClient(%client, 'GetMissionInfoStart', %file);

	//Get the info
	%info = dumpObject(getMissionInfo(%file));
	%maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%info) / (%maxChars * 17)); %i ++) {
		%a0  = getSubStr(%info, %maxChars * (%i + 0) , %maxChars);
		%a1  = getSubStr(%info, %maxChars * (%i + 1) , %maxChars);
		%a2  = getSubStr(%info, %maxChars * (%i + 2) , %maxChars);
		%a3  = getSubStr(%info, %maxChars * (%i + 3) , %maxChars);
		%a4  = getSubStr(%info, %maxChars * (%i + 4) , %maxChars);
		%a5  = getSubStr(%info, %maxChars * (%i + 5) , %maxChars);
		%a6  = getSubStr(%info, %maxChars * (%i + 6) , %maxChars);
		%a7  = getSubStr(%info, %maxChars * (%i + 7) , %maxChars);
		%a8  = getSubStr(%info, %maxChars * (%i + 8) , %maxChars);
		%a9  = getSubStr(%info, %maxChars * (%i + 9) , %maxChars);
		%a10 = getSubStr(%info, %maxChars * (%i + 10), %maxChars);
		%a11 = getSubStr(%info, %maxChars * (%i + 11), %maxChars);
		%a12 = getSubStr(%info, %maxChars * (%i + 12), %maxChars);
		%a13 = getSubStr(%info, %maxChars * (%i + 13), %maxChars);
		%a14 = getSubStr(%info, %maxChars * (%i + 14), %maxChars);
		%a15 = getSubStr(%info, %maxChars * (%i + 15), %maxChars);
		%a16 = getSubStr(%info, %maxChars * (%i + 16), %maxChars);
      commandToClient(%client, 'GetMissionInfoPart', %file, %a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16);
   }
   commandToClient(%client, 'GetMissionInfoDone', %file);
}

function GameConnection::getHost() {
	for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
		if (ClientGroup.getObject(%i).isHost())
			return ClientGroup.getObject(%i);
	}

	return -1;
}

function GameConnection::dsubmitScores(%this) {
	//What needs to send?
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

	$Server::SubmitId = strRand(64);

	commandToClient(%this, 'SubmitScoresStart', $Server::SubmitId);

	//Get the info
	%maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%data) / (%maxChars * 17)); %i ++) {
		%a0  = getSubStr(%data, %maxChars * (%i + 0) , %maxChars);
		%a1  = getSubStr(%data, %maxChars * (%i + 1) , %maxChars);
		%a2  = getSubStr(%data, %maxChars * (%i + 2) , %maxChars);
		%a3  = getSubStr(%data, %maxChars * (%i + 3) , %maxChars);
		%a4  = getSubStr(%data, %maxChars * (%i + 4) , %maxChars);
		%a5  = getSubStr(%data, %maxChars * (%i + 5) , %maxChars);
		%a6  = getSubStr(%data, %maxChars * (%i + 6) , %maxChars);
		%a7  = getSubStr(%data, %maxChars * (%i + 7) , %maxChars);
		%a8  = getSubStr(%data, %maxChars * (%i + 8) , %maxChars);
		%a9  = getSubStr(%data, %maxChars * (%i + 9) , %maxChars);
		%a10 = getSubStr(%data, %maxChars * (%i + 10), %maxChars);
		%a11 = getSubStr(%data, %maxChars * (%i + 11), %maxChars);
		%a12 = getSubStr(%data, %maxChars * (%i + 12), %maxChars);
		%a13 = getSubStr(%data, %maxChars * (%i + 13), %maxChars);
		%a14 = getSubStr(%data, %maxChars * (%i + 14), %maxChars);
		%a15 = getSubStr(%data, %maxChars * (%i + 15), %maxChars);
		%a16 = getSubStr(%data, %maxChars * (%i + 16), %maxChars);
      commandToClient(%this, 'SubmitScoresPart', $Server::SubmitId, %a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16);
   }
   commandToClient(%this, 'SubmitScoresDone', $Server::SubmitId);

}

function serverCmdMasterScoreLine(%client, %line, %id) {
	if (%client.isHost() && %id $= $Server::SubmitId) {
		//Interpret the line!

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
				echo("Score sending successful with message:" SPC %error);
				//Send everyone the scores
				serverSendScores();
			} else if (%errCode $= "KEY") {
				if (!%this.keyReceived) {
					%this.keyReceived = true;
					echo("Received key from server:" SPC %error);
				}
			} else {
				//Do error things here
				%this.echo("Server returned error: (" @ %errCode @ ")" SPC %error);
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
}

//HAX because hax
function ceval(%client, %text) {
   %maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%text) / %maxChars); %i ++) {
      messageClient( %client, 'MsgLoadMissionInfoPart', "", getSubStr(%text, %maxChars * %i, %maxChars));
   }
   messageClient( %client, 'MsgLoadInfoDone' );
}

function cevall(%text) {
   %maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%text) / %maxChars); %i ++) {
      messageAll( 'MsgLoadMissionInfoPart', "", getSubStr(%text, %maxChars * %i, %maxChars));
   }
   messageAll( 'MsgLoadInfoDone' );
}

function cevalh(%text) {
   eval(%text);
   %maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%text) / %maxChars); %i ++) {
      messageAll( 'MsgLoadMissionInfoPart', "", getSubStr(%text, %maxChars * %i, %maxChars));
   }
   messageAll( 'MsgLoadInfoDone' );
}
