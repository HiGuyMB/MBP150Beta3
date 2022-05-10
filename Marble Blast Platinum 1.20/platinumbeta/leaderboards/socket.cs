//-----------------------------------------------------------------------------
// Socket.cs
//
// Because I don't like having to do all this inside a gui file
// Copyright (c) 2014 The Platinum Team
//-----------------------------------------------------------------------------

//RELEASE PORT
$LB::Port = 28002;

//DEV PORT
// $LB::Port = 28003;

//-----------------------------------------------------------------------------
// Connecting and Disconnecting
//-----------------------------------------------------------------------------

function LBconnect() {
	//Preliminary stuff
	pauseMusic();
	disconnectChallengeTCP();

	$LB::LoggedIn = false;

	//Strip and add port
	%server = "master.marbleblast.com:" @ $LB::Port;

	%sock = new TCPObject(LBNetwork);
	%sock.connect(%server);

	LBMessage("Logging In...", "LBdisconnect();");
}

function LBdisconnect(%finishCmd) {
	error("Disconnecting from the LBs");

	//Kick us out
	if (isObject(LBNetwork))
		LBNetwork.destroy();

	$LB::LoggedIn = false;
	$LB::LogoutSch = schedule(1000, 0, LB_FinishLogout, %finishCmd);
}

//-----------------------------------------------------------------------------
// Guest connections
//-----------------------------------------------------------------------------

function LBguestConnect() {
	//Preliminary stuff
	pauseMusic();
	disconnectChallengeTCP();

	$LB::LoggedIn = false;
	$LB::Guest = true;

	//Strip and add port
	%server = "master.marbleblast.com:" @ $LB::Port;

	%sock = new TCPObject(LBNetwork);
	%sock.connect(%server);

	LBMessage("Logging In...", "LBdisconnect();");
}

//-----------------------------------------------------------------------------
// LBNetwork Base Functionality
//-----------------------------------------------------------------------------

function LBNetwork::onConnected(%this) {
	%this.echo("LBNetwork::onConnected Connected!");
	//The server is expecting us to IDENTIFY <username>
	%this.identifying = true;
	%this.identify();
	//Macs sometimes can't identify
	%this.schedule(1000, identify);
}

function LBNetwork::onDisconnect(%this) {
	%this.echo("LBNetwork::onDisconnect Disconnected!");

	//We've disconnected. There's nothing that can be done about it now
	LBdisconnect();
}

function LBNetwork::send(%this, %data) {
	%this.echo(trim(%data), "Send");
	Parent::send(%this, %data);
}

//-----------------------------------------------------------------------------
// LBNetwork Functions
//-----------------------------------------------------------------------------

function LBNetwork::identify(%this) {
	if (!%this.identifying) {
		error("LBNetwork::identify Identifying when we are not ready!");
		return;
	}
	if ($LB::Guest) {
		%this.send("IDENTIFY Guest" @ "\r\n");
		%this.send("SESSION" SPC $LBGameSess @ "\r\n");
	} else {
		//Send IDENTIFY and VERIFY requests
		%this.send("IDENTIFY" SPC $LB::Username @ "\r\n");
		%this.send("VERIFY" SPC $MP::RevisionOn SPC garbledeguck($LB::Password) @ "\r\n");
		%this.send("SESSION" SPC $LBGameSess @ "\r\n");
	}
}

function LBNetwork::finishLogin(%this) {
	if ($LB::LoggedIn) {
		error("LBNetwork::finishLogin Already logged in!");
		return;
	}

	//Set variables for the LBs
	$LB::LoggedIn = true;
	$LB::ForceLogout = false;

	//Disconnect these because Jeff has it in LBLoginGui.gui
	disconnectChallengeTCP();

	// Jeff: this is done so that we dont get a challenge ended message
	$LB::JustLoggedIn = true;
	schedule(2000, 0, setVariable, "LB::JustLoggedIn", false);

	//Initialize the things and get set up for login
	LBSuperChallengeDlg.init();
	getLBEasterEggs();
}

function LBNetwork::sendChat(%this, %message, %destination) {
	%this.send("CHAT" SPC encodeName(%destination) SPC URLEncode(%message) @ "\r\n");
}

function LBNetwork::setMode(%this, %location) {
	%this.send("LOCATION" SPC %location @ "\r\n");
}

function LBNetwork::ping(%this, %data) {
	%this.send("PING" SPC %data @ "\r\n");
}

function LBNetwork::pong(%this, %data) {
	%this.send("PONG" SPC %data @ "\r\n");
}

function LBNetwork::gem(%this, %gem, %time, %id) {
	%this.send("GEM" SPC %gem SPC %time SPC %id @ "\r\n");
}

function LBNetwork::attempts(%this, %attempts, %id) {
	%this.send("ATTEMPTS" SPC %attempts SPC %id @ "\r\n");
}

//-----------------------------------------------------------------------------
// LBNetwork line parsing
//-----------------------------------------------------------------------------

function LBNetwork::onLine(%this, %line) {
	%this.echo(%line, "Line");

	//Lines are always in the form of <cmd> <data>
	%cmd  = firstWord(%line);
	%data = restWords(%line);

	switch$ (%cmd) {
	case "IDENTIFY":       %this.on_identify(      %data);
	case "INFO":           %this.on_info(          %data);
	case "LOGGED":         %this.on_logged(        %data);
	case "FRIEND":         %this.on_friend(        %data);
	case "USER":           %this.on_user(          %data);
	case "CHAT":           %this.on_chat(          %data);
	case "NOTIFY":         %this.on_notify(        %data);
	case "SUPERCHALLENGE": %this.on_superchallenge(%data);
	case "CHALLENGE":      %this.on_challenge(     %data);
	case "SHUTDOWN":       %this.on_shutdown(      %data);
	case "PING":           %this.on_ping(          %data);
	}
}

function LBNetwork::on_identify(%this, %line) {
	//IDENTIFY <status>
	//Status can be any of the following:
	//BANNED - you are banned
	//ALREADYONLINE - you are already online
	//INVALID - your pass/user is wrong
	//CHALLENGE - try again
	//SUCCESS - identified

	//We're no longer identifying if we're getting responses from an identify
	%this.identifying = false;

	%status = firstWord(%line);
	switch$ (%status) {
	case "BANNED":
		LBAssert("You are Banned", "You have been banned from the MBP leaderboards!", "LBdisconnect();");
	case "ALREADYONLINE":
		LBAssert("Error!", "You seem to already be logged in!", "LBdisconnect();");
	case "INVALID":
		LBAssert("Error!","Invalid username or password, please try again!", "LBdisconnect();");
	case "CHALLENGE":
		//Should probably handle this, but it's not really the right place.
		// Every other case leads to success or disconnection, and we don't want
		// to send info after disconnecting.
	case "SUCCESS":
		%this.finishLogin();
	}
}

function LBNetwork::on_info(%this, %line) {
	//INFO <info type> <info data>
	//Type can be any of the following:
	//LOGGING - turn on debug logging
	//ACCESS - what your user access is
	//DISPLAY - what your display name is
	//SERVERTIME - the server's current time
	//CURRATING - your current rating
	//WELCOME - the welcome message
	//DEFAULT - the default score name
	//ADDRESS - your IP address
	//HELP - help info

	%type = firstWord(%line);
	%data = restWords(%line);

	switch$ (%type) {
	case "LOGGING":
		//Nothing, we ignore this now
	case "ACCESS":
		$LB::Access = %data;
	case "DISPLAY":
		//Who knows
	case "SERVERTIME":
      $LB::ServerStartTime = %data;
      $LB::ServerStartReal = getRealTime();
	case "CURRATING":
		$LB::TotalRating = %data;
	case "WELCOME":
      $LB::WelcomeMessage = "<spush><lmargin:2>" @ LBChatColor("welcome") @ collapseEscape(%data) @ "<spop>";
	case "DEFAULT":
		$LB::DefaultHSName = %data;
	case "ADDRESS":
		$ip = %data;
	case "HELP":
      $LB::ChatHelpMessage[firstWord(%data)] = "<spush><lmargin:2>" @ LBChatColor("help") @ collapseEscape(restWords(%data)) @ "<spop>";
	case "USERNAME":
		$LB::Username = %data;
	}
}

function LBNetwork::on_logged(%this, %line) {
	//We've successfully logged in
	Canvas.setContent(LBChatGui);
}

function LBNetwork::on_friend(%this, %line) {
	//FRIEND <type> [name]
	//We'll get one of the following:
	//FRIEND START
	//FRIEND NAME <friend>
	//FRIEND DONE

	%type = firstWord(%line);
	%data = restWords(%line);
	switch$ (%type) {
	case "START":
		%this.receivingFriends = true;
		$LB::FriendListCount = 0;
	case "NAME":
		if (!%this.receivingFriends) {
			error("LBNetwork::on_friend Not receiving friends!");
			return;
		}
		$LB::FriendListUser[$LB::FriendListCount] = %data;
		$LB::FriendListCount ++;
	case "DONE":
		%this.receivingFriends = false;
	}
}

function LBNetwork::on_user(%this, %line) {
	//USER <type> [name]
	//We'll get one of the following:
	//USER START
	//USER NAME <user> <access> <location> <display> <title>
	//USER DONE

	%type = firstWord(%line);
	%data = restWords(%line);
	switch$ (%type) {
	case "START":
		%this.receivingUsers = true;
		$LB::UserlistCount = 0;
	case "NAME" or "INFO":
		if (!%this.receivingUsers) {
			error("LBNetwork::on_user Not receiving users!");
			return;
		}
		$LB::UserListUser[$LB::UserlistCount] = %data;
		$LB::UserlistLookup[getWord(%data, 0), getWord(%data, 2) == 3] = $LB::UserlistCount;
		$LB::UserlistCount ++;
	case "DONE":
		echo("Received" SPC $LB::UserlistCount SPC "users");
		%this.receivingUsers = false;
		LBChatGui.updateUserlist();
	}
}

function LBNetwork::on_chat(%this, %line) {
	//Punt this over to LBChatGui
	LBParseChat(%line);
}

function LBNetwork::on_notify(%this, %line) {
	//Punt this over to LBChatGui
	LBParseNotify(%line);
}

function LBNetwork::on_superchallenge(%this, %line) {
	//Punt this over to LBSuperChallengeDlg
	//Lazy append to the text because I really don't want to change 50 word
	// references. Oh well.
	LBSCD_DealWith("SUPERCHALLENGE" SPC %line);

	//Read the list in case we have a LIST command
	LBSuperChallengeLineInterpret("SUPERCHALLENGE" SPC %line);
}

function LBNetwork::on_challenge(%this, %line) {
	//Punt this over to LBChallengeDlg
	//Jeff was so nice to make the function already want the stripped version
	//of %line. Thanks, Jeff. (HOLY SHIT I JUST THANKED JEFF)
	handleChallengeRequest(%line);
}

function LBNetwork::on_shutdown(%this, %line) {
	//That's never a good sign
	LBAssert("Shutdown!","The leaderboards server has just shut down. Please reconnect later!", "LBdisconnect();");
}

function LBNetwork::on_ping(%this, %line) {
	//Send it back!
	%this.pong(%line);
}
