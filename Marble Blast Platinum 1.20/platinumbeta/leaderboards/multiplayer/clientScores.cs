//------------------------------------------------------------------------------
// Multiplayer Package
// clientScores.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function clientCmdScoreListStart(%teamMode) {
   //HiGuy: Prepare for a list!
   if (isObject(ScoreList))
      ScoreList.delete();
   if (isObject(TeamScoreList))
      TeamScoreList.delete();
   if (isObject(TeamScorePlayerList))
      TeamScorePlayerList.delete();
   $MP::ScoreTeams = 0;
   $MP::ScorePlayers = 0;
   if ($MP::TeamMode) {
      Array(TeamScoreList);
      Array(TeamScorePlayerList);
   }
   Array(ScoreList);
}

function clientCmdScoreListPlayer(%name, %score, %index, %skin) {
   ScoreList.addEntry(%name NL %score NL %index NL %skin);
   $MP::ScorePlayers ++;
}

function clientCmdScoreListTeam(%name, %score, %number, %color) {
   TeamScoreList.addEntry(%name NL %score NL 0 NL %number NL %color);
   $MP::ScoreTeams ++;
}

function clientCmdScoreListTeamPlayer(%team, %name, %score, %index, %marble) {
   TeamScorePlayerList.addEntry(%team NL %name NL %score NL %index NL %marble);
   $MP::ScorePlayers ++;

   for (%i = $MP::ScoreTeams - 1; %i >= 0; %i --) {
      %_team = TeamScoreList.getEntry(%i);
      if (getRecord(%_team, 0) $= %team) {
         %_team = setRecord(%_team, 2, getRecord(%_team, 2) + 1);
         TeamScoreList.replaceEntryByIndex(%i, %_team);
         break;
      }
   }
}

// Jeff: obv. updates the score list for the client
function clientCmdScoreListEnd() {
   scoreListUpdate();
}

//HiGuy: When the server owner starts a new level

function clientCmdServerLoading() {
   if (!$Server::Hosting)
      LBMessage("Waiting for Server...");
}

//HiGuy: Totally not disguised name!
function clientCmdGameStatus(%status) {
   $Editor::Opened = %status;
}

function clientCmdNoCollision(%item) {
   if (isObject(%item))
      %item.hide(false);
}

//-----------------------------------------------------------------------------

function scoreListUpdate() {
   $MP::ScoreUpdate ++;

   %color[1] = "<color:CFB52B>";
   %color[2] = "<color:CDCDCD>";
   %color[3] = "<color:D19275>";

   %shadow = "<shadowcolor:ffffff7f><shadow:1:1>";
   %pgshadow = "<shadowcolor:0000007f><shadow:1:1>";
   //<shadow:1:1><shadowcolor:776622ff>

   if ($MP::TeamMode) {
      %teams = $MP::ScoreTeams;
      %face     = "<font:DomCasualD:36>";
      %format   = %shadow @ "<color:ffffff>";
      %font     = %face @ %format @ "<tab:40,350>";
      %font2    = %face @ %format @ "<color:ffffff><font:DomCasualD:36><tab:310>";
      %pgface   = "<font:DomCasualD:32>";
      %pgformat = %pgshadow @ "<color:000000>";
      %pgfont   = %pgface @ %pgformat @ "<color:ffee99><tab:25,350>";
      %pgfont2  = %pgface @ %pgformat @ "<color:ffee99><tab:310>";
      %rowIdx   = 0;
      %teamIdx  = 0;

      //HiGuy: Sort it!
      %used = Array();
      for (%i = 0; %i < %teams; %i ++) {
         %bestScore = -1;
         %bestIdx = -1;
         for (%j = 0; %j < %teams; %j ++) {
            %team  = getRecord(TeamScoreList.getEntry(%j), 0);
            %score = getRecord(TeamScoreList.getEntry(%j), 1);
            if (%used.containsEntry(%team))
               continue;
            if (%score > %bestScore) {
               %bestScore = %score;
               %bestIdx = %j;
            } else
               continue;
         }

         %team    = getRecord(TeamScoreList.getEntry(%bestIdx), 0);
         %score   = getRecord(TeamScoreList.getEntry(%bestIdx), 1);
         %players = getRecord(TeamScoreList.getEntry(%bestIdx), 2);
         %number  = getRecord(TeamScoreList.getEntry(%bestIdx), 3);
         %color   = getRecord(TeamScoreList.getEntry(%bestIdx), 4);

         %used.addEntry(%team);

         //HiGuy: Don't show teams with no players
         if (%players == 0)
            continue;

         %teamIdx ++;

         //HiGuy: Size of score rows
         %itemHeight   = 44;
         %pgitemHeight = 32;

         //HiGuy: Add the display for the team
         if ($MP::ScoreListTeamIndex[%team] $= "" || !isObject(MPScoreContainerTeam @ %number)) {
            //HiGuy: Set these for reference
            $MP::ScoreListTeamIndex[%team]    = %number;
            $MP::ScoreListTeamLookup[%number] = %team;

            //HiGuy: Score list text and object
            MPScoreListContainer.add(
               new GuiControl(MPScoreContainerTeam @ %number) {
                  profile = "GuiMLTextProfile";
                  position = "0 0";
                  extent = 500 SPC %itemHeight;
                  visible = "1";

                  new GuiMLTextCtrl(MPScoreTextTeam @ %number) {
                     profile = "GuiMLTextProfile";
                     position = "8 3";
                     extent = "430 14";
                     visible = "1";
                     lineSpacing = "2";
                     maxChars = "-1";
                  };
               }
            );
            //HiGuy: Score list text and object
            PGScoreListContainer.add(
               new GuiControl(PGScoreContainerTeam @ %number) {
                  profile = "GuiMLTextProfile";
                  position = "0 0";
                  extent = 300 SPC %itemHeight;
                  visible = "1";

                  new GuiMLTextCtrl(PGScoreTextTeam @ %number) {
                     profile = "GuiMLTextProfile";
                     position = "8 3";
                     extent = "235 14";
                     visible = "1";
                     lineSpacing = "2";
                     maxChars = "-1";
                  };
               }
            );
         }

         //HiGuy: At this point, they should have a display entry. Set it up!
         %scoreText   = "MPScoreTextTeam"      @ %number;
         %container   = "MPScoreContainerTeam" @ %number;
         %pgscoreText = "PGScoreTextTeam"      @ %number;
         %pgcontainer = "PGScoreContainerTeam" @ %number;

         //HiGuy: Resize these to be at the correct position
         //                  x  y                        w    h
         %container.resize(  0, %rowIdx * %itemHeight,   500, %itemHeight);
         %pgcontainer.resize(0, %rowIdx * %pgitemHeight, 300, %itemHeight);

         %container.team = %team;
         %pgcontainer.team = %team;

         //HiGuy: Total row counter
         %rowIdx ++;

         %estimated = mFloor(%score * (MissionInfo.time / (MissionInfo.time -    PlayGui.elapsedTime)));

         %scoreText.setValue(  %font   @ %teamIdx @ "." TAB clipPx(%face @ %team, 300, true) TAB %score);
         %pgscoreText.setValue(%pgfont @ %color[%teamIdx] @ %teamIdx @ "." TAB clipPx(%pgface @ %team, 180, true) @ "<just:right>" @ %score @ ($MPPref::ScorePredictor ? " " @ %estimated : ""));

         %container.lastUpdate   = $MP::ScoreUpdate;
         %pgcontainer.lastUpdate = $MP::ScoreUpdate;

         //HiGuy: Go through the players and add those on the team
         %teamPlayers = Array();
         %players = $MP::ScorePlayers;

         for (%j = 0; %j < %players; %j ++) {
            %player = TeamScorePlayerList.getEntry(%j);
            if (getRecord(%player, 0) $= %team)
               %teamPlayers.addEntry(%player);
         }

         %usedPlayers = Array();
         %teamPlayerCount = %teamPlayers.getSize();

         //HiGuy: Organize team players by score
         for (%j = 0; %j < %teamPlayerCount; %j ++) {
            %bestScore = -1;
            %bestIdx = -1;
            for (%k = 0; %k < %teamPlayerCount; %k ++) {
               %player = getRecord(%teamPlayers.getEntry(%k), 1);
               %score  = getRecord(%teamPlayers.getEntry(%k), 2);
               if (%usedPlayers.containsEntry(%player))
                  continue;
               if (%score > %bestScore) {
                  %bestScore = %score;
                  %bestIdx = %k;
               } else
                  continue;
            }

            %player = getRecord(%teamPlayers.getEntry(%bestIdx), 1);
            %score  = getRecord(%teamPlayers.getEntry(%bestIdx), 2);
            %index  = getRecord(%teamPlayers.getEntry(%bestIdx), 3);
            %marble = getRecord(%teamPlayers.getEntry(%bestIdx), 4);
            %state  = getRecord(PlayerList.getEntryByRecord(%player, 0), 12);
            %ping   = getRecord(PlayerList.getEntryByRecord(%player, 0), 5);

            //HiGuy: Add the display for the player
            if ($MP::ScoreListIndex[%player] $= "" || !isObject(MPScoreContainer @ %index)) {
               //HiGuy: Set these for reference
               $MP::ScoreListIndex[%player] = %index;
               $MP::ScoreListLookup[%index] = %player;

               //HiGuy: Score list text and object
               MPScoreListContainer.add(
                  new GuiControl(MPScoreContainer @ %index) {
                     profile = "GuiMLTextProfile";
                     position = "0 0";
                     extent = 500 SPC %itemHeight;
                     visible = "1";

                     new GuiMLTextCtrl(MPScoreText @ %index) {
                        profile = "GuiMLTextProfile";
                        position = "48 3";
                        extent = "210 14";
                        visible = "1";
                        lineSpacing = "2";
                        maxChars = "-1";
                     };
                     new GuiObjectView(MPPlayerMarble @ %index) {
                        profile = "GuiDefaultProfile";
                        position = "430 -10";
                        extent = "64 64";
                        visible = "1";
                        model = "~/data/shapes/balls/ball-superball.dts";
                        skin = "base";
                        cameraZRotSpeed = "0.001";
                        orbitDistance = "0.75";
                     };
                  }
               );
               //HiGuy: Score list text and object
               PGScoreListContainer.add(
                  new GuiControl(PGScoreContainer @ %index) {
                     profile = "GuiMLTextProfile";
                     position = "0 0";
                     extent = 300 SPC %pgitemHeight;
                     visible = "1";

                     new GuiMLTextCtrl(PGScoreText @ %index) {
                        profile = "GuiMLTextProfile";
                        position = "33 3";
                        extent = "210 14";
                        visible = "1";
                        lineSpacing = "2";
                        maxChars = "-1";
                     };
                     new GuiObjectView(PGPlayerMarble @ %index) {
                        profile = "GuiDefaultProfile";
                        position = "260 -2";
                        extent = "48 48";
                        visible = "1";
                        model = "~/data/shapes/balls/ball-superball.dts";
                        skin = "base";
                        cameraZRotSpeed = "0.001";
                        orbitDistance = "0.75";
                     };
                     new GuiBitmapCtrl(PGPlayerPing @ %index) {
                        profile = "GuiMLTextProfile";
                        position = "239 4";
                        extent = "32 32";
                        visible = "1";
                        lineSpacing = "2";
                        maxChars = "-1";
                     };
                  }
               );
            }

            //HiGuy: At this point, they should have a display entry. Set it up!
            %scoreText    = "MPScoreText"      @ %index;
            %objectView   = "MPPlayerMarble"   @ %index;
            %container    = "MPScoreContainer" @ %index;
            //%pingctrl     = "MPPlayerPing"     @ %index;
            %pgscoreText  = "PGScoreText"      @ %index;
            %pgobjectView = "PGPlayerMarble"   @ %index;
            %pgcontainer  = "PGScoreContainer" @ %index;
            %pgpingctrl   = "PGPlayerPing"     @ %index;

            //HiGuy: Resize these to be at the correct position
            //                  x  y                        w    h
            %container.resize(  0, %rowIdx * %itemHeight,   500, %itemHeight);
            %pgcontainer.resize(0, %rowIdx * %pgitemHeight, 300, %itemHeight);
            %container.player   = %player;
            %pgcontainer.player = %player;

            //HiGuy: Total row counter
            %rowIdx ++;

            %bitmap = "unknown";
            if (%ping < 100) %bitmap = "high";
            else if (%ping < 250) %bitmap = "medium";
            else if (%ping < 500) %bitmap = "low";
            else if (%ping < 1000) %bitmap = "matanny";
            %pgpingctrl.setBitmap($usermods @ "/leaderboards/play/connection-" @ %bitmap @ ".png");
            //%pingctrl.setBitmap($usermods @ "/leaderboards/play/connection-" @ %bitmap @ ".png");

            %estimated = mFloor(%score * (MissionInfo.time / (MissionInfo.time - PlayGui.elapsedTime)));

            switch (%color) {
            case -1: %tcolor = "<color:000000>";
            case  0: %tcolor = "<color:ff0000>";
            case  1: %tcolor = "<color:ffff00>";
            case  2: %tcolor = "<color:00ff00>";
            case  3: %tcolor = "<color:00ffff>";
            case  4: %tcolor = "<color:0000ff>";
            case  5: %tcolor = "<color:ff00ff>";
            case  6: %tcolor = "<color:ff8000>";
            case  7: %tcolor = "<color:8000ff>";
            }

            %prefix = "";
            if (%state $= "0") %prefix = "[DC] ";
            if (%state $= "2") %prefix = "[S] ";

            %scoreText.setValue(  %font2   @ clipPx(%face @ LBResolveName(%player, true), 300, true) TAB %score);
            %pgscoreText.setValue(%pgfont2 @ "<spush>" @ %tcolor @ clipPx(%pgface @ %prefix @ LBResolveName(%player, true), 170, true) @ "<spop><just:right>" @ %score @ ($MPPref::ScorePredictor ? " " @ %estimated : ""));

            //HiGuy: Shape and skin for the marble
            %shape = MPMarbleSelectionDlg.getMarbleShape(getField(%marble, 1));
            %skin  = getField(%marble, 2);
            %objectView.setModel(%shape, %skin);
            %pgobjectView.setModel(%shape, %skin);

            //HiGuy: Set fields so they don't reset when we pop the gui
            %objectView.model   = %shape;
            %objectView.skin    = %skin;
            %pgobjectView.model = %shape;
            %pgobjectView.skin  = %skin;

            %container.lastUpdate   = $MP::ScoreUpdate;
            %pgcontainer.lastUpdate = $MP::ScoreUpdate;

            %usedPlayers.addEntry(%player);
         }

         %usedPlayers.delete();
         %teamPlayers.delete();
      }
      %used.delete();

      %count = MPScoreListContainer.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %obj = MPScoreListContainer.getObject(%i);
         if ((%obj.team !$= "" && !TeamScoreList.containsEntryAtRecord(%obj.team, 0)) || (%obj.player !$= "" && !TeamScorePlayerList.containsEntryAtRecord(%obj.player, 1)) || %obj.lastUpdate != $MP::ScoreUpdate) {
            //HiGuy: Player/team no longer exists!
            %obj.delete();
            %i --;
            %count --;
         }
      }
      %count = PGScoreListContainer.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %obj = PGScoreListContainer.getObject(%i);
         if ((%obj.team !$= "" && !TeamScoreList.containsEntryAtRecord(%obj.team, 0)) || (%obj.player !$= "" && !TeamScorePlayerList.containsEntryAtRecord(%obj.player, 1)) || %obj.lastUpdate != $MP::ScoreUpdate) {
            //HiGuy: Player/team no longer exists!
            %obj.delete();
            %i --;
            %count --;
         }
      }
   } else {
      %face   = "<font:DomCasualD:36>";
      %font   = %face @ "<color:ffffff><tab:40,350>";
      %pgface  = "<font:DomCasualD:32>";
      %pgfont = %pgshadow @ "<color:ffee99><font:DomCasualD:32>";
      %pgwinfont = %pgshadow @ "<color:88ffcc><font:DomCasualD:32>";
      %players = $MP::ScorePlayers;
      %rowIdx = 0;

      //HiGuy: Sort it!
      %used = Array();
      for (%i = 0; %i < %players; %i ++) {
         %bestScore = -1;
         %bestIdx = 0;
         for (%j = 0; %j < %players; %j ++) {
            %player = getRecord(ScoreList.getEntry(%j), 0);
            %score  = getRecord(ScoreList.getEntry(%j), 1);
            if (%used.containsEntry(%player))
               continue;
            if (%score > %bestScore) {
               %bestScore = %score;
               %bestIdx = %j;
            } else
               continue;
         }

         %player = getRecord(ScoreList.getEntry(%bestIdx), 0);
         %score  = getRecord(ScoreList.getEntry(%bestIdx), 1);
         %index  = getRecord(ScoreList.getEntry(%bestIdx), 2);
         %marble = getRecord(ScoreList.getEntry(%bestIdx), 3);
         %state  = isObject(PlayerList) ? getRecord(PlayerList.getEntryByRecord(%player, 0), 12) : 0;
         %ping   = isObject(PlayerList) ? getRecord(PlayerList.getEntryByRecord(%player, 0), 5) : 0;

         //HiGuy: Size of score rows
         %itemHeight   = 44;
         %pgitemHeight = 32;

         //HiGuy: Add the display for the player
         if ($MP::ScoreListIndex[%player] $= "" || !isObject(MPScoreContainer @ %index)) {
            //HiGuy: Set these for reference
            $MP::ScoreListIndex[%player] = %index;
            $MP::ScoreListLookup[%index] = %player;

            //HiGuy: Score list text and object
            MPScoreListContainer.add(
               new GuiControl(MPScoreContainer @ %index) {
                  profile = "GuiMLTextProfile";
                  position = "0 0";
                  extent = 500 SPC %itemHeight;
                  visible = "1";

                  new GuiMLTextCtrl(MPScoreText @ %index) {
                     profile = "GuiMLTextProfile";
                     position = "8 3";
                     extent = "420 14";
                     visible = "1";
                     lineSpacing = "2";
                     maxChars = "-1";
                  };
                  new GuiObjectView(MPPlayerMarble @ %index) {
                     profile = "GuiDefaultProfile";
                     position = "445 -10";
                     extent = "64 64";
                     visible = "1";
                     model = "~/data/shapes/balls/ball-superball.dts";
                     skin = "base";
                     cameraZRotSpeed = "0.001";
                     orbitDistance = "0.75";
                  };
               }
            );
            //HiGuy: Score list text and object
            PGScoreListContainer.add(
               new GuiControl(PGScoreContainer @ %index) {
                  profile = "GuiMLTextProfile";
                  position = "0 0";
                  extent = 300 SPC %pgitemHeight;
                  visible = "1";

                  new GuiMLTextCtrl(PGScoreText @ %index) {
                     profile = "GuiMLTextProfile";
                     position = "8 3";
                     extent = "235 14";
                     visible = "1";
                     lineSpacing = "2";
                     maxChars = "-1";
                  };
                  new GuiObjectView(PGPlayerMarble @ %index) {
                     profile = "GuiDefaultProfile";
                     position = "260 -2";
                     extent = "48 48";
                     visible = "1";
                     model = "~/data/shapes/balls/ball-superball.dts";
                     skin = "base";
                     cameraZRotSpeed = "0.001";
                     orbitDistance = "0.75";
                  };
                  new GuiBitmapCtrl(PGPlayerPing @ %index) {
                     profile = "GuiMLTextProfile";
                     position = "239 4";
                     extent = "32 32";
                     visible = "1";
                     lineSpacing = "2";
                     maxChars = "-1";
                  };
               }
            );
         }

         //HiGuy: At this point, they should have a display entry. Set it up!
         %scoreText    = "MPScoreText"      @ %index;
         %objectView   = "MPPlayerMarble"   @ %index;
         %container    = "MPScoreContainer" @ %index;
         //%pingctrl     = "MPPlayerPing"     @ %index;
         %pgscoreText  = "PGScoreText"      @ %index;
         %pgobjectView = "PGPlayerMarble"   @ %index;
         %pgcontainer  = "PGScoreContainer" @ %index;
         %pgpingctrl   = "PGPlayerPing"     @ %index;

         //HiGuy: Resize these to be at the correct position
         //                  x  y                        w    h
         %container.resize(  0, %rowIdx * %itemHeight,   500, %itemHeight);
         %pgcontainer.resize(0, %rowIdx * %pgitemHeight, 300, %itemHeight);
         %container.player   = %player;
         %pgcontainer.player = %player;

         //HiGuy: Total row counter
         %rowIdx ++;

         %bitmap = "unknown";
         if (%ping < 100) %bitmap = "high";
         else if (%ping < 250) %bitmap = "medium";
         else if (%ping < 500) %bitmap = "low";
         else if (%ping < 1000) %bitmap = "matanny";
         %pgpingctrl.setBitmap($usermods @ "/leaderboards/play/connection-" @ %bitmap @ ".png");
         //%pingctrl.setBitmap($usermods @ "/leaderboards/play/connection-" @ %bitmap @ ".png");

         %estimated = mFloor(%score * (MissionInfo.time / (MissionInfo.time - PlayGui.elapsedTime)));

         %prefix = "";
         if (%state $= "0") %prefix = "[DC] ";
         if (%state $= "2") %prefix = "[S] ";

         %scoreText.setValue(%font @ %rowIdx @ "." TAB clipPx(%face @ LBResolveName(%player, true), 300, true) TAB %score);
         %pgscoreText.setValue(%pgfont @ %color[%rowIdx] @ %rowIdx @ "." SPC clipPx(%pgface @ %prefix @ LBResolveName(%player, true), 170, true) @ "<just:right>" @ %score @ ($MPPref::ScorePredictor ? " " @ %estimated : ""));

         //HiGuy: Shape and skin for the marble
         %shape = MPMarbleSelectionDlg.getMarbleShape(getField(%marble, 1));
         %skin  = getField(%marble, 2);
         %objectView.setModel(%shape, %skin);
         %pgobjectView.setModel(%shape, %skin);

         //HiGuy: Set fields so they don't reset when we pop the gui
         %objectView.model   = %shape;
         %objectView.skin    = %skin;
         %pgobjectView.model = %shape;
         %pgobjectView.skin  = %skin;

         %container.lastUpdate   = $MP::ScoreUpdate;
         %pgcontainer.lastUpdate = $MP::ScoreUpdate;

         %used.addEntry(%player);
      }
      %used.delete();

      %count = MPScoreListContainer.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %obj = MPScoreListContainer.getObject(%i);
         if (!ScoreList.containsEntryAtRecord(%obj.player, 0) || %obj.lastUpdate != $MP::ScoreUpdate) {
            //HiGuy: Player no longer exists!
            %obj.delete();
            %i --;
            %count --;
         }
      }
      %count = PGScoreListContainer.getCount();
      for (%i = 0; %i < %count; %i ++) {
         %obj = PGScoreListContainer.getObject(%i);
         if (!ScoreList.containsEntryAtRecord(%obj.player, 0) || %obj.lastUpdate != $MP::ScoreUpdate) {
            //HiGuy: Player no longer exists!
            %obj.delete();
            %i --;
            %count --;
         }
      }
   }

   MPScoreHeader.setValue("<font:DomCasualD:36><color:FFFFFF><tab:40,350> \tName\tScore<just:right>Marble");
   // Jeff: display the result
}

function resetScoreList() {
   while (MPScoreListContainer.getCount()) {
      %obj = MPScoreListContainer.getObject(0);
      %obj.delete();
   }
   while (PGScoreListContainer.getCount()) {
      %obj = PGScoreListContainer.getObject(0);
      %obj.delete();
   }
}

//-----------------------------------------------------------------------------
// Master server ratings / scores / change

function clientCmdMasterScoreCount(%scores) {
   $MP::MasterScoreCount = %scores;
}

function clientCmdMasterScorePlayer(%id, %player) {
   $MP::MasterScorePlayer[%id] = %player;
   $MP::MasterScoreLookup[%player] = %id;
}

function clientCmdMasterScoreRating(%id, %rating) {
   $MP::MasterScoreRating[%id] = %rating;
}

function clientCmdMasterScoreChange(%id, %change) {
   $MP::MasterScoreChange[%id] = %change;
}

function clientCmdPlayerGemCount(%player, %gems1, %gems2, %gems5) {
   $MP::PlayerGemCount[%player] = %gems1 TAB %gems2 TAB %gems5;
}

function clientCmdMasterScoreFinish() {
   MPEndGameDlg.updateScores();
}
