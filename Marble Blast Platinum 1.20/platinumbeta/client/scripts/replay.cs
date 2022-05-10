//-----------------------------------------------------------------------------
// replay.cs
//
// Yet another runaway project of mine
// COOL
//-----------------------------------------------------------------------------

//FPS for recordings
$recordFps = 15;
$playbackFps = 40;

function missionRecordPath(%file) {
	%file = strreplace(%file, "/data/", "/data/recordings/") @ ".rec";
	return %file;
}

//Starts recording your things! Yay!
function startRecording(%mission) {
	warn("Starting recording for" SPC %mission);

	$Game::Recording = true;
	$Game::RecordMission = %mission;
	$Game::RecordLine = 0;

	recordLoop();
}

//Cancels the recording
function cancelRecording() {
	$Game::Recording = false;
	$Game::RecordMission = "";
	$Game::RecordLine = 0;
	deleteVariables("$Game::RecordData*");
}

function scrapRecording() {
	$Game::Recording = false;
	$Game::RecordMission = "";
	$Game::RecordLine = 0;
	deleteVariables("$Game::RecordData*");
}

function saveRecording() {
	if (!$Game::Recording)
		return;

	%fo = new FileObject();
	%path = missionRecordPath($Game::RecordMission);

	if (!%fo.openForWrite(%path)) {
		//Could not actually record your things :(
		error("Could not record a demo to " SPC %path @ "!");
		%fo.close();
		%fo.delete();
		return;
	}

	%fo.writeLine($Game::RecordMission);
	for (%i = 0; %i < $Game::RecordLine; %i ++)
		%fo.writeLine($Game::RecordData[%i]);

	%fo.close();
	%fo.delete();
}

//Cancel does the same thing
function finishRecording() {
	//Find best recording
	%time = $pref::bestRun[$Game::RecordMission] ? $pref::bestRun[$Game::RecordMission] : 5998999;
	if (PlayGui.elapsedTime < %time) {
		$pref::bestRun[$Game::RecordMission] = PlayGui.elapsedTime;
		saveRecording();
	}

	cancelRecording();
	setModPaths($modpath);
}

function recordLoop() {
	if ($Game::Recording) {
		cancel($recordLoop);
		$recordLoop = schedule(1000 / min($recordFPS, $fps::real), 0, "recordLoop");

		//Don't record if we have no player!
		if (!isObject(ServerConnection))
			return;
		if (!isObject(ServerConnection.getControlObject()))
			return;

		//Player position
		%pos = ServerConnection.getControlObject().getTransform();

		//Smaller files = happy files
		if (%pos $= $Game::RecordLastPos)
			return;
		$Game::RecordLastPos = %pos;

		$Game::RecordData[$Game::RecordLine] = $Sim::Time - $Game::ResetTime TAB PlayGui.elapsedTime TAB %pos;
		$Game::RecordLine ++;
	}
}

function playRecording(%path) {
	warn("Playing back recording at" SPC %path);

	//Play the dang recording
	$Game::Playback = true;
	$Game::PlaybackPath = %path;
	$Game::PlaybackFile = new FileObject();

	if (!$Game::PlaybackFile.openForRead(%path)) {
		//Unfortunate
		error("Could not read demo at" SPC %path @ "!");

		//Cancel
		cancelPlayback();
		return;
	}

	%line = playbackRead();
	if (%line $= "EOF") { //Already?
		error("Could not read invalid demo at" SPC %path @ ". The file is blank!");

		//Cancel
		cancelPlayback();
		return;
	}

	//First line is the mission file
	$Game::PlaybackStart = 0;
	$Game::PlaybackLines = 0;

	//Read the whole file now
	while ((%line = playbackRead()) !$= "EOF") {
		//Think about it
		$Game::PlaybackData[$Game::PlaybackLines] = %line;
		$Game::PlaybackLines ++;
	}
	$Game::PlaybackFile.close();
	$Game::PlaybackFile.delete();

	$Game::PlaybackLine = 0;
	playbackLoop();
}

function cancelPlayback() {
	$Game::Playback = false;
	$Game::PlaybackPath = "";
	$Game::PlaybackFile = "";
	$Game::PlaybackStart = 0;
	$Game::PlaybackLine = 0;
	deleteVariables("$Game::PlaybackData*");

	if (isObject($Game::PlaybackGhost))
		$Game::PlaybackGhost.hide(true);
}

function finishPlayback() {
	cancelPlayback();
}

function playbackRead() {
	if (!isObject($Game::PlaybackFile))
		return "EOF";
	if ($Game::PlaybackFile.isEOF())
		return "EOF";

	%line = trim($Game::PlaybackFile.readLine());
	return %line;
}

function playbackPos() {
	//HiGuy: 0.08 is lag because of ServerConnection <MAGIC NUMBER ALERT>
	%time = $Sim::Time - $Game::ResetTime + 0.08;

//	echo("Current time:" SPC %time);

	//Where should we be?
	%line = $Game::PlaybackData[$Game::PlaybackLine];

	//Catch up
	while (getField(%line, 0) < %time && $Game::PlaybackLine < $Game::PlaybackLines) {
		$Game::PlaybackLine ++;
		%line = $Game::PlaybackData[$Game::PlaybackLine];
	}
	$Game::PlaybackLine --;
	%line = $Game::PlaybackData[$Game::PlaybackLine];

	if ($Game::PlaybackLine >= $Game::PlaybackLines)
		return getField($Game::PlaybackData[$Game::PlaybackLines - 1], 2);

	//To compare
	%nextLine = $Game::PlaybackData[$Game::PlaybackLine + 1];

	//Parse the line!
	%simTime  = getField(%line, 0);
	%elapsed  = getField(%line, 1);
	%position = getField(%line, 2);

	//Parse the next line
	%simTime_  = getField(%nextLine, 0);
	%elapsed_  = getField(%nextLine, 1);
	%position_ = getField(%nextLine, 2);

//	echo("Line time:" SPC %simTime);
//	echo("Next time:" SPC %simTime_);

//	echo("Should lerp" SPC ((%time - %simTime) / (%simTime_ - %simTime)) * 100 @ "% of the way");

	//If we fall behind...
	if (%time > %simTime_)
		$Game::PlaybackLine ++;

	//Interpolate the positions
	return vectorLerp(getWords(%position, 0, 2), getWords(%position_, 0, 2), ((%time - %simTime) / (%simTime_ - %simTime))) SPC getWords(%position, 3, 6);
}

function playbackLoop() {
	if ($Game::Playback) {
		cancel($playbackLoop);
		$playbackLoop = schedule(1000 / min($playbackFps, $fps::real), 0, "playbackLoop");

		//Don't playback if we aren't connected yet!
		if (!isObject(ServerConnection))
			return;
		if (!isObject(ServerConnection.getControlObject()))
			return;

		if ($Game::PlaybackStart == 0) {
			$Game::PlaybackStart = $Sim::Time;

			//Spawn a ghost
			$Game::PlaybackGhost = new StaticShape() {
				datablock = "DefaultGhost";
				ghost = true;
			};

			//Add the ghost
			MissionCleanup.add($Game::PlaybackGhost);
		}

		$Game::PlaybackGhost.setTransform(playbackPos());
	}
}

