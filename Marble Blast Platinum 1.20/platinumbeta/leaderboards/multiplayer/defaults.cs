//------------------------------------------------------------------------------
// Multiplayer Package
// defaults.cs
// Copyright (c) 2013 MBP Team
//
// These values are set for proper gameplay, and cannot be changed by clients.
//------------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Control
//-----------------------------------------------------------------------------

// MP Revision (only updated when changes to MP happen), probably reliable
$MP::RevisionOn = 10000;

echo("Initializing Multiplayer Scripts Revision" SPC $MP::RevisionOn);

//-----------------------------------------------------------------------------
// Update Times
//-----------------------------------------------------------------------------

// Client-side rotation fixing (by Jeff)
$MP::ClientRotUpdate = 100; // ms

// Client sided position and rotation (by Jeff)
// used for only clients that have different platform than server
$MP::ClientTransformUpdate = 40; // ms

// Client-side positional fixing
$MP::ClientTween = 40; //ms

// Server-side positional fixing
$MP::ServerTween = 100; //ms

// Calls fixGhost() on interval
$MP::ClientFixTime = 2000; //ms

// Server-side collision detection
$MP::Collision::Delta = 20; //ms

// Dampening for team hits
$MP::CollisionTeamDampen = 6;

// Ghosts update this often
$MP::GhostUpdateTime = 100; //ms

//-----------------------------------------------------------------------------
// Collision
//-----------------------------------------------------------------------------

// "Nudges" players out of the way if they happen to collide
$MP::Collision::EnableNudge = false; //true/false

// "Clips" player meshes (sets them to non-collision) if two players collide
$MP::Collision::EnableClip = true; //true/false

$MP::Collision::Multiplier = 4;
$MP::Collision::Maximum = 25;
$MP::Collision::MegaMultiplier = 7;
$MP::Collision::MegaMaximum = 50;

//-----------------------------------------------------------------------------
// Blast
//-----------------------------------------------------------------------------

// Blast strength applied to surrounding players
$MP::BlastShockwaveStrength = 7.5;

// Blast Recharge - Blast strength applied to surrounding players
$MP::BlastRechargeShockwaveStrength = 8.5;

// Blast Impulse - Blast power, this is the amount of up-boost you get
// Higher values make the marbles go crazy. Default is 10
$MP::BlastPower = 10;

// Blast Recharge - Blast power
// Add just a bit power to our blast more if we collected Blast Recharge
$MP::BlastRechargePower = 1.03;

// Amount of blast bar filled (x / 1.0) required to use blast
$MP::BlastRequiredAmount = 0.2;

// Time required for blast to fully recharge from 0.0 - 1.0
$MP::BlastChargeTime = 25000;

// Normal blast divisor Default is 1 (lower = stronger)
$MP::NormalBlastModifier = 1.0;

// Mega blast divisor (lower values = more blasty)
$MP::MegaBlastModifier = 0.7;

//-----------------------------------------------------------------------------
// Master Server
//-----------------------------------------------------------------------------

// The mod name for the master server to interpret
// Only mods with this set for their modname will show up on the list
$Master::Mod = "Platinum";

// The server on which the master server's PHP files are hosted
// Rot13 for lols
$Master::Server = rot13("zneoyroynfg.pbz:80");
//$Master::Server = rot13("zosbehzf.vaanphengr.pbz:80");
//$Master::Server = "127.0.0.1:3791";

// The path to the master server's PHP files on the server
// Also rot13'd
$Master::Path = rot13("/yrnqre/ZC_Znfgre/");
//$Master::Path = "/mpsite/leader/MP_Master/";

// The update frequency (in milliseconds) between heartbeats
// to the master server.
$Master::UpdateFreq = 30000;

// The frequency (in milliseconds) between attempts to connect to the master
// server if the first connection fails.
$Master::RestartFreq = 4000;

//-----------------------------------------------------------------------------
// Teams
//-----------------------------------------------------------------------------

// The default team will have this name applied when it is created, and it
// cannot be changed by the leader.
$MP::DefaultTeamName = "Default Team";

// The default team will have this description applied when it is created, and
// it cannot be changed by the leader.
$MP::DefaultTeamDesc = "This is the default team. Any players who join the game or leave their team will be part of this team until they join another team. This team will not be deleted if it has zero players, and cannot be renamed.";

// The team description before it has been sent to the server
$MP::NewTeamDesc = "Incomplete team description.";

// Useful to stop crashing!
$MP::TeamDescMaxLength = 1024;

// Displayed role for the team's leader
$MP::TeamLeaderRoleName = "Leader";

// Displayed role for anyone not on a specific team
$MP::TeamUnaffiliatedRoleName = "If you see this, contact Platinum Team ASAP.";

// Displayed role for general team players
$MP::TeamGeneralRoleName = "Player";

//-----------------------------------------------------------------------------
// Misc
//-----------------------------------------------------------------------------

// Allow quickrespawn (backspace / tab key) in MP games
$MPPref::AllowQuickRespawn = true;

// Quick Respawn is disabled for this many ms after being used (to prevent
// abuse)
$MP::QuickSpawnTimeout = 25000;

// Enables the "direct connect" dialog in JoinServer
$MP::EnableDirectConnect = true;

// Server max players limits. Can't set MaxPlayers to anything outside this
// range.
$MP::PlayerMaximum = 64;
$MP::PlayerMinimum = 2;

// Enable scaling of radar gem icons based on height
$MPPref::RadarZ = true;

// Time in ms before the gems randomly respawn in a level
// Used to imitate what happens when you play again Matan
// and he gets all the gems really fast
$MPPref::Server::MatanModeTime = 10000;

// Amount of segments to send loading in so that way I don't slow down
$MP::LoadSegments = 20;

// Overview time for a full 360° (seconds)
$MPPref::OverviewSpeed = 100;

// Overview finish time (seconds)
$MPPref::OverviewFinishSpeed = 0;

// Ban list file
$MP::BanlistFile = $usermods @ "/leaderboards/multiplayer/banlist.cs";

// We want people to experience preloading
$MPPref::Preload = true;

// So they can find their friends
$MPPref::DisplayOnMaster = true;

// So their scores go through
$MPPref::CalculateScores = true;

// So they can chat!
$MPPref::AllowServerChat = true;

// Back up clients
$MPPref::BackupClients = true;

// Client-sided powerups
$MPPref::FastPowerups = false;

//-----------------------------------------------------------------------------
// Prefs
//-----------------------------------------------------------------------------

function MPloadPrefs() {
   if ($tracing) trace(false);
   %file = expandFilename("./prefs.cs");
   if (isFile(%file))
      safeExecPrefs(%file);
}

function MPsavePrefs() {
   export("$MPPref::*", "./prefs.cs");
}

//HiGuy: Load em!
MPloadPrefs();
