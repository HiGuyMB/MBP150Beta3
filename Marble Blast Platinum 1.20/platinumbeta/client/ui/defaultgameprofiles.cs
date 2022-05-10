//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Override base controls
GuiButtonProfile.soundButtonOver = AudioButtonOver;
GuiButtonProfile.soundButtonDown = AudioButtonDown;

GuiDefaultProfile.soundButtonDown = AudioButtonDown;

//-----------------------------------------------------------------------------
// Chat Hud profiles


new GuiControlProfile ("ChatHudMessageProfile")
{
   fontType = "Arial";
   fontSize = 16;
   fontColor = "255 255 0";      // default color (death msgs, scoring, inventory)
   fontColors[1] = "4 235 105";   // client join/drop, tournament mode
   fontColors[2] = "219 200 128"; // gameplay, admin/voting, pack/deployable
   fontColors[3] = "77 253 95";   // team chat, spam protection message, client tasks
   fontColors[4] = "40 231 240";  // global chat
   fontColors[5] = "200 200 50 200";  // used in single player game
   // WARNING! Colors 6-9 are reserved for name coloring
   autoSizeWidth = true;
   autoSizeHeight = true;
};

// HiGuy: User list profile colors
new GuiControlProfile ("LBChatUserlistProfile")
{
	fontType = "Arial";
	fontSize = 16;
	fontColor = "0 0 0";
	fontColors[1] = "80 80 80"; //
	fontColors[2] = "255 0 0"; // Admin
	fontColors[3] = "0 0 255"; // Mod
	fontColors[4] = "0 0 0"; // Normal person
	fontColors[5] = "0 0 0"; //
	fontColors[6] = "0 0 0"; //
	fontColors[7] = "0 0 0"; //
	fontColors[8] = "0 0 0"; //
	fontColors[9] = "0 0 0"; //
	autoSizeWidth = true;
    autoSizeHeight = true;
};

new GuiControlProfile ("LBPlayChatProfile")
{
	fontType = "Arial";
	fontSize = 16;
	fontColor = "0 0 0";
	fontColors[1] = "80 80 80"; // Player name in chat
	fontColors[2] = "0 140 0"; // Welcome messages
	fontColors[3] = "0 0 255"; // Mod
	fontColors[4] = "255 0 0"; // Admin
	fontColors[5] = "255 0 0"; // Server messages
	fontColors[6] = "128 0 255"; // Emotion messages
	fontColors[7] = "176 100 0"; // Notifications
	fontColors[8] = "100 50 0"; // Whisper from
	fontColors[9] = "50 50 50"; // Whisper msg
	autoSizeWidth = true;
    autoSizeHeight = true;
};

//HiGuy : WHY ARENT THESE IN HERE

new GuiControlProfile ("GuiLBScrollProfile") {
   opaque = true;
   fillColor = "255 255 255 255";
   fillColorHL = "244 244 244 255";
   fillColorNA = "244 244 244 255";
   border = 3;
   borderThickness = 2;
   borderColor = "151 39 0 255";
   borderColorHL = "151 39 0 255";
   borderColorNA = "64 64 64 255";
   bitmap = ($platform $= "macos") ? $usermods @ "/core/ui/osxScroll" : $usermods @ "/core/ui/darkScroll";
};

new GuiControlProfile ("GuiLBCheckBoxProfile" : GuiCheckBoxProfile) {
   //Higuy: Nothing?
   opaque = false;
};

new GuiControlProfile ("GuiLBPopupMenuProfile" : GuiPopupMenuProfile) {
   fontColor = "0 0 0 255";
   fontColors = "0 0 0 255";
   fontColorHL = "32 100 100 255";
   fontColorNA = "0 0 0 255";
   fontColorSEL = "32 100 100 255";
   borderColor = "0 0 0 255";
   borderColorHL = "151 39 0 255";
   borderColorNA = "64 64 64 255";
};

new GuiControlProfile ("ChatHudScrollProfile")
{
   opaque = false;
   bitmap = "~/core/ui/darkScroll";
   hasBitmapArray = true;
};


new GuiControlProfile (GuiTPTextEditProfile)
{
   opaque = false;
   fillColor = "255 255 255";
   fillColorHL = "128 128 128";
   border = false;
   borderColor = "0 0 0";
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   textOffset = "0 2";
   autoSizeWidth = false;
   autoSizeHeight = true;
   tab = true;
   canKeyFocus = true;
};

new GuiControlProfile (OverlayScreenProfile)
{
   opaque = true;
   fillColor = "0 0 0 96";
   fillColorHL = "128 128 128";
   border = false;
   borderColor = "0 0 0";
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   textOffset = "0 2";
   autoSizeWidth = false;
   autoSizeHeight = true;
   tab = true;
   canKeyFocus = true;
};

new GuiControlProfile (GuiBigTextEditProfile)
{
   fontType = "DomCasualD";
   fontSize = 32;
   opaque = false;
   fillColor = "255 255 255";
   fillColorHL = "128 128 128";
   border = false;
   borderColor = "0 0 0";
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   textOffset = "0 2";
   autoSizeWidth = false;
   autoSizeHeight = true;
   tab = true;
   canKeyFocus = true;
};

new GuiControlProfile (GuiSearchTextEditProfile)
{
   fontType = "Arial Bold";
   fontSize = 20;
   opaque = false;
   fillColor = "255 255 255";
   fillColorHL = "128 128 128";
   border = false;
   borderColor = "0 0 0";
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   textOffset = "0 2";
   autoSizeWidth = false;
   autoSizeHeight = true;
   tab = true;
   canKeyFocus = true;
};

new GuiControlProfile (BevelPurpleProfile)
{
   // fill color
   opaque = true;
   border = 2;
   fillColor   = "161 150 229";
   fillColorHL = "255 0 0";
   fillColorNA = "0 0 255";

   // border color
   borderColor   = "0 255 0";
   borderColorNA = "92 86 131";

   textOffset = "6 6";

};


new GuiControlProfile (GuiRaceProfile : GuiButtonProfile) {
   fontType = "Marker Felt";
   fontSize = 24;
   justify = "center";
   fontColor = "255 255 255";
   fontColorHL = "230 230 230";
   fontColorNA = "128 128 128";
};


//-----------------------------------------------------------------------------
// Common Hud profiles

new GuiControlProfile ("HudScrollProfile")
{
   opaque = false;
   border = true;
   borderColor = "0 255 0";
   bitmap = "~/core/ui/darkScroll";
   hasBitmapArray = true;
};

new GuiControlProfile ("HudTextProfile")
{
   opaque = false;
   fillColor = "128 128 128";
   fontColor = "0 255 0";
   border = true;
   borderColor = "0 255 0";
};


//-----------------------------------------------------------------------------
// Center and bottom print

new GuiControlProfile ("CenterPrintProfile")
{
   opaque = false;
   fillColor = "128 128 128";
   fontColor = "0 255 0";
   border = true;
   borderColor = "0 255 0";
};

new GuiControlProfile ("CenterPrintTextProfile")
{
   opaque = false;
   fontType = "Arial";
   fontSize = 12;
   fontColor = "0 255 0";
};

if (!isObject(GuiBlastDisabledProfile)) {
   new GuiControlProfile(GuiBlastDisabledProfile) {
      opaque = false;
      fillColor = "152 152 152 100";
      border = true;
      borderColor = "78 88 120";
   };
}

if (!isObject(GuiBlastEnabledProfile)) {
   new GuiControlProfile(GuiBlastEnabledProfile) {
      opaque = false;
      fillColor = "44 152 162 100";
      border = true;
      borderColor   = "78 88 120";
   };
}

if (!isObject(GuiUltraBlastProfile)) {
   new GuiControlProfile(GuiUltraBlastProfile) {
      opaque = false;
      fillColor = "247 161 0 100";
      border = true;
      borderColor   = "78 88 120";
   };
}

if (!isObject(GuiMLProgressProfile)) {
   new GuiControlProfile(GuiMLProgressProfile) {
      opaque = false;
      fillColor = "255 255 255 100";
      border = false;
   };
}
