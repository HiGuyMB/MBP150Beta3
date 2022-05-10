//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

// The master server is declared with the server defaults, which is
// loaded on both clients & dedicated servers.  If the server mod
// is not loaded on a client, then the master must be defined.
//$pref::Master[0] = "2:v12master.dyndns.org:28002";

// Stuff ported and organised to reflect PQ, some stuff modified.

$pref::Player::Name = "Platinum Player";
$pref::Player::defaultFov = 90;
$pref::Player::zoomSpeed = 0;
$pref::showFPSCounter = 1;

$pref::QualifiedLevel["Beginner"] = 1;
$pref::QualifiedLevel["Intermediate"] = 1;
$pref::QualifiedLevel["Advanced"] = 1;
$pref::QualifiedLevel["Custom"] = 5000;
$pref::QualifiedLevel["Expert"] = 1;
$pref::QualifiedLevel["GG"] = 1;
$pref::QualifiedLevel["Hunt"] = 1000;
$pref::QualifiedLevel["Coop"] = 1000;
$pref::MBGQualifiedLevel["Beginner"] = 1;
$pref::MBGQualifiedLevel["Intermediate"] = 1;
$pref::MBGQualifiedLevel["Advanced"] = 1;
$pref::MBGQualifiedLevel["Custom"] = 5000;

$pref::checkMOTDAndVersion = "1"; // check the version by default
$pref::checkLETip = "1";
$pref::FirstRun[$THIS_VERSION] = true;
$pref::ShowOOBMessages = false;

$Pref::Net::LagThreshold = "400";
$pref::Net::PacketRateToClient = "32";
$pref::Net::PacketRateToServer = "32";
$pref::Net::PacketSize = "400";

$pref::shadows = "2";
$pref::HudMessageLogSize = 40;
$pref::ChatHudLength = 1;
$pref::useStencilShadows = 0;

$pref::Input::LinkMouseSensitivity = 1;
// Direct Input keyboard, mouse, and joystick prefs
$pref::Input::KeyboardEnabled = 1;
$pref::Input::MouseEnabled = 1;
$pref::Input::JoystickEnabled = 0;
$pref::Input::KeyboardTurnSpeed = 0.025;
$pref::Input::MouseSensitivity = 0.50;
$pref::Input::InvertYAxis = 0;
$pref::Input::AlwaysFreeLook = 1;

$pref::Interior::TexturedFog = 0;
$pref::Video::displayDevice = "D3D"; // when we get a new teleport pad shape, switch to OpenGL as default

$pref::Video::allowOpenGL = 1;
$pref::Video::allowD3D = 1;
$pref::Video::preferOpenGL = 1;
$pref::Video::appliedPref = 0;
$pref::Video::disableVerticalSync = 1;
$pref::Video::monitorNum = 0;
$pref::Video::resolution = "800 600 32"; // keep this for mbp, even though we can do higher now
$pref::Video::windowedRes = "800 600"; // keep this for mbp, even though we can do higher now
$pref::Video::fullScreen = "0";

$pref::OpenGL::force16BitTexture = "0";
$pref::OpenGL::forcePalettedTexture = "0";
$pref::OpenGL::maxHardwareLights = 3;
$pref::OpenGL::textureTrilinear = "1"; // I don't think this works unless you enter it in console... and then it resets itself when you quit MB.
$pref::VisibleDistanceMod = 1.0;

$pref::Audio::driver = "OpenAL";
$pref::Audio::forceMaxDistanceUpdate = 0;
$pref::Audio::environmentEnabled = 0;
$pref::Audio::masterVolume   = 1.0;
$pref::Audio::channelVolume1 = 0.65;
$pref::Audio::channelVolume2 = 0.5;
$pref::Audio::channelVolume3 = 0.5;
$pref::Audio::channelVolume4 = 0.5;
$pref::Audio::channelVolume5 = 0.5;
$pref::Audio::channelVolume6 = 0.5;
$pref::Audio::channelVolume7 = 0.5;
$pref::Audio::channelVolume8 = 0.5;

$pref::LastReadMOTD = "Welcome to Marble Blast Platinum!";
$Pref::EnableDirectInput = true;
$Pref::Unix::OpenALFrequency = 44100;

//LBPrefs

$LBPref::Server = 0;
$LBPref::Categories = "DirectorsCut levelpacks1-9 levelpacks10-19 levelpacks20-29";
$LBPref::SuperChallengeSlideshow = true;
