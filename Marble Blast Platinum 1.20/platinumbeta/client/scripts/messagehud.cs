//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//----------------------------------------------------------------------------
// Enter Chat Message Hud
//----------------------------------------------------------------------------

function PlayGui::positionMessageHud(%this) {
   //Sizing variables
   %w             = getWord(%this.getExtent(), 0);
   %h             = getWord(%this.getExtent(), 1);
   %challenge     = ($LB::SuperChallengeMode || $LB::ChallengeMode);
   %progress      = (($LB::SuperChallengeMode && $LBPref::SuperChallengeOptionProgress) || $LB::ChallengeMode);
   %timeout       = %challenge && ($LB::SuperChallengeTimeout || $LB::Challenge::Timeout);
   %updateTimeout = %timeout && floor64($Sim::Time) !$= $LB::LastPGTime1000;
   %lb            = $LB::LoggedIn && $LB::Username !$= "";
   %mp            = %lb && $Server::ServerType $= "Multiplayer";

   %height = 180;

   if (%challenge && (LBSuperChallengeDlg.max == 4 || $LB::Challenge::Type $= "attempts"))
      %height = 200;

   %chatWidth = %w;
   %chatStartX = 0;

   if (%challenge) {
      %chatWidth -= 200;
      %chatStartX += 200;
   }

   %chatHeight = %height - 60;
   %timeoutHeight = 45;

   %entryStart = %mp && $chathud ? 135 : 0;

   //Resize the FPS meter
   %fps_w = ($LB::LoggedIn ? 118 : 96);
   %fps_h = 32;

   if ($LB::LoggedIn) {
   	LBMessageHudDlg.setExtent(PlayGui.getExtent());

      PG_LBChatBackground.resize(0, %h - %height, %w, %height);

      PG_LBTimeoutContainer.resize(%chatWidth - %fps_w, 15, 136, 45);
      PG_LBTimeoutContainer.setVisible(%timeout);
      PG_LBTimeoutContainer.setBitmap($usermods @ "/leaderboards/play/pc_trans/" @ ($pref::showFPSCounter ? 9 : 5) @ ".png");

      PG_LBChatScorePanel.setVisible(%challenge);
      PG_LBChatTextPanel.resize(%chatStartX, 0, %chatWidth, %height);

      PG_LBChatText.setPosition(1, -(getWord(PG_LBChatText.extent, 1)));

      // Jeff: set the attempts counter visible if we are attempts
      if ($LB::ChallengeMode && $LB::Challenge::Type $= "attempts") {

         // Jeff: adjust the position of the attempts counter, if we have
         // gems it goes under, else it goes where the gem position is
         if (countGems(MissionGroup) == 0)
            PG_AttemptsContainer.resize(0, 0, 160, 55);
         else
            PG_AttemptsContainer.resize(0, 53, 160, 55);

         PG_AttemptsContainer.setVisible(true);

         //Update the HUD marble skin
         %selection = LBMarbleSelectionDlg.getSelection();
         %marble = getField(%selection, 0);
         %skin   = getField(%selection, 1);
         %skin   = (%skin $= "") ? "base" : %skin;
         %marble = isFile(%marble) ? %marble : $usermods @ "/data/shapes/balls/ball-superball.dts";
         HUD_Attempts.setModel(%marble, %skin);
      }

		PG_ServerChatScroll.setVisible($Server::ServerType $= "Multiplayer");
		PG_LBChatScroll.setExtent(($Server::ServerType $= "Multiplayer" ? %chatWidth / 2 : %chatWidth) SPC getWord(PG_LBChatScroll.extent, 1));
   }

   %this.updateMessageHud();
}

function PlayGui::updateMessageHud(%this) {
	showSpectatorMenu($SpectateMode);

   //Sizing variables
   %w             = getWord(%this.getExtent(), 0);
   %h             = getWord(%this.getExtent(), 1);
   %challenge     = ($LB::SuperChallengeMode || $LB::ChallengeMode);
   %progress      = (($LB::SuperChallengeMode && $LBPref::SuperChallengeOptionProgress) || $LB::ChallengeMode);
   %timeout       = %challenge && ($LB::SuperChallengeTimeout || $LB::Challenge::Timeout);
   %updateTimeout = %timeout && floor64($Sim::Time) !$= $LB::LastPGTime1000;
   %lb            = $LB::LoggedIn && $LB::Username !$= "";
   %mp            = %lb && $Server::ServerType $= "Multiplayer";

   %height = 180;

   if (%challenge && (LBSuperChallengeDlg.max == 4 || $LB::Challenge::Type $= "attempts"))
      %height = 200;

   %chatWidth = %w;
   %chatStartX = 0;

   if (%challenge) {
      %chatWidth -= 200;
      %chatStartX += 200;
   }

   %chatHeight = %height - 60;
   %timeoutHeight = 45;

   %entryStart = %mp && $chathud ? 135 : 0;

   //Resize the FPS meter
   %fps_w = ($LB::LoggedIn ? 118 : 96);
   %fps_h = 32;

   if (%timeout)
      FPSMetreCtrl.resize(%w - %fps_w + 9, %h - %fps_h - %chatHeight - %timeoutHeight, %fps_w, %fps_h);
   else if (%lb)
      FPSMetreCtrl.resize(%w - %fps_w, %h - %fps_h - %chatHeight, %fps_w, %fps_h);
   else
      FPSMetreCtrl.resize(%w - %fps_w, %h - %fps_h, %fps_w, %fps_h);

   //Fix the bitmap
   %bmp = $LB::LoggedIn ? $usermods @ "/leaderboards/play/pc_trans/fps" @ (%timeout ? 2 : "") : $usermods @ "/client/ui/game/transparency_fps-flipped";
   if (FPSMetreBitmap.bitmap !$= %bmp)
      FPSMetreBitmap.setBitmap(%bmp);

   FPSMetreBitmap.resize(0, 0, %fps_w, %fps_h);
   FPSMetreText.resize(%lb && !%mp ? 20 : 10, %mp || !%lb ? 3 : 10, 106, 28);


   if ($LB::LoggedIn) {
      PG_LBTimeoutContainer.resize(%chatWidth - %fps_w, 15, 136, 45);
      PG_LBTimeoutContainer.setVisible(%timeout);
      PG_LBTimeoutContainer.setBitmap($usermods @ "/leaderboards/play/pc_trans/" @ ($pref::showFPSCounter ? 9 : 5) @ ".png");

      if (%timeout) {
			//HiGuy: Sub64 is laggy as hell
			%updateTimeout = floor64($Sim::Time) !$= $LB::LastPGTime1000;

			if (%updateTimeout) {
				$LB::LastPGTime1000 = floor64($Sim::Time);
				%time = $LB::SuperChallengeMode ? $LB::SuperChallengeTimeout : $LB::Challenge::Timeout;
				%difference = max(sub64(%time, LBGetServerTime()), 0);
				%secs = mFloor(%difference % 60);
				%mins = mFloor(%difference - %secs) / 60;

				%secOne = (%secs % 10);
				%secTen = (%secs - %secOne) / 10;
				%minOne = (%mins % 10);
				%minTen = (%mins - %minOne) / 10;

				%pfx = %difference < 30 ? (%difference % 2 == 0 || mFloor(%difference) == 0 ? "_red" : "") : "";

				PG_LBTimeoutMin_Ten.setTimeNumberPfx(%minTen, %pfx);
				PG_LBTimeoutMin_One.setTimeNumberPfx(%minOne, %pfx);
				PG_LBTimeoutSec_Ten.setTimeNumberPfx(%secTen, %pfx);
				PG_LBTimeoutSec_One.setTimeNumberPfx(%secOne, %pfx);
				PG_LBTimeoutColon.setTimeNumberPfx("colon",   %pfx);
			}
      }

      if ($pref::showFPSCounter)
         PG_LBTopShadow.resize(0, 52, ($chathud ? %entryStart : %chatWidth - %fps_w), 8);
      else
         PG_LBTopShadow.resize(0, 52, ($chathud ? %entryStart : %chatWidth - %entryStart), 8);

      if ($chathud) {
         if ($pref::showFPSCounter)
            PG_LBChatEntryContainer.resize(%entryStart, 15, %chatWidth - %fps_w - %entryStart, 45);
         else
            PG_LBChatEntryContainer.resize(%entryStart, 15, %chatWidth - %start, 45);
      }

		LBScrollChat();
   }
}

function LBscrollChat() {
   if (isObject(PG_LBChatScroll)) {
   	if (Canvas.getContent().getName() $= "PlayGui") {
	   	PG_LBChatText.forceReflow();
	   	PG_ServerChatText.forceReflow();
		}
      PG_LBChatScroll.scrollToBottom();
      PG_LBChatScroll.schedule(100, scrollToBottom);
      PG_LBChatScroll.schedule(1000, scrollToBottom);
		PG_ServerChatScroll.scrollToBottom();
		PG_ServerChatScroll.schedule(100, scrollToBottom);
		PG_ServerChatScroll.schedule(1000, scrollToBottom);
   }
}

//------------------------------------------------------------------------------

function PlayGui::sendChat(%this) {
   %message = trim(PG_LBChatEntry.getValue());
   if ($chatPrivate) {
   	mpSendChat(%message);
   } else if (%message !$= "" && $LB::LoggedIn) {
      %line = strlwr(%message); // Jeff: used for comparisons
      %dest = (getWord(%line,0) $= "/whisper") ? getWord(%message,1) : "";
      LBSendChat(%message, %dest);

      PG_LBChatEntry.setValue("");
   }
	disableChatHUD();
}

function PlayGui::chatTabComplete(%this) {
   PG_LBChatEntry.tabComplete();
}

function PlayGui::chatUpdate(%this) {
   %message = PG_LBChatEntry.getValue();
   if (PG_LBChatEntry.getValue() !$= %message) {
      PG_LBChatEntry.cursorPosition += strlen(%message) - strlen(PG_LBChatEntry.getValue());
      PG_LBChatEntry.setValue(%message);
   }

	PG_LBChatEntry.setPosition("0" SPC (37 - getWord(PG_LBChatEntry.extent, 1)));
   %message = trim(PG_LBChatEntry.getUncompletedValue());
   PG_LBChatEntry.setTabCompletions(getTabCompletions(%message));
}

//----------------------------------------------------------------------------
// MessageHud key handlers

function toggleChatHUD(%make) {
	//HiGuy: Only when they push down the button
   if (%make) {
	   $chatPrivate = false;
      if ($chatHud)
         disableChatHUD();
      else
         enableChatHUD();
   }
}

function togglePrivateChatHUD(%make) {
	//HiGuy: Only when they push down the button
   if (%make) {
	   $chatPrivate = true;
      if ($chatHud)
         disableChatHUD();
      else
         enableChatHUD();
   }
}

function disableChatHUD() {
   echo("DISABLING CHATHUD");
   PG_LBChatEntryContainer.setVisible(false);
   PG_LBChatEntry.makeFirstResponder(false);

	//HiGuy: We want to place the chat behind everything BUT PlayGui
   %cont = Canvas.getContent();

   //HiGuy: Bring to front actually sends it to back... who wrote this function?
   Canvas.bringToFront(LBMessageHudDlg);
   Canvas.bringToFront(%cont);

   $chatHud = false;
   $chatPrivate = false;
}

function enableChatHUD() {
   echo("ENABLING CHATHUD");
   PG_LBChatEntry.setTickable(true);
   // Jeff: only lbs
   if (!$LB::LoggedIn || $LB::username $= "")
      return;
   PG_LBChatEntryContainer.setVisible(true);
   PG_LBChatEntry.setValue("");
   PlayGui.updateMessageHud();

   //HiGuy: We want to place the chat in front of everything
   //HiGuy: Push to back actually brings it to the front. Seriously.
   Canvas.pushToBack(LBMessageHudDlg);
   PG_LBChatEntry.makeFirstResponder(true);
   $chatHud = true;
}

//------------------------------------------------------------------------------

function getTabCompletions(%message) {
   //HiGuy: Be context-aware
   %line = strlwr(%message); // Jeff: used for comparisons
   %command = (getSubStr(ltrim(%line), 0, 1) $= "/" ? getWord(%line, 0) : "");

   %completions = "";

   if (getSubStr(%message, 0, 1) $= "/" && strPos(%message, " ") == -1) {
      %list = "help\nwhisper\nme\nslap\n" @ ($LB::Access > 1 ? "invisible\n" : "") @ ($Server::ServerType $= "Multiplayer" ? "vtaunt\nvtaunt1\nvtaunt2\nvtaunt3\nvtaunt4\nvtaunt5\nvtaunt6\nvtaunt7\nvtaunt8\nvtaunt9\nvtaunt10\nvtaunt11\n" : "") @ ($Server::ServerType $= "Multiplayer" ? "v1\nv2\nv3\nv4\nv5\nv6\nv7\n8\nv9\nv10\nv11\n" : "") @ "thegame";
      %current = getSubStr(%message, 1, strlen(%message));

      for (%i = 0; %i < getRecordCount(%list); %i ++) {
         %record = getRecord(%list, %i);
         if (strPos(strlwr(%record), strlwr(%current)) == 0) {
            %record = getSubStr(%record, strlen(%current), strlen(%record));
            if (%completions $= "")
               %completions = %record;
            else
               %completions = %completions NL %record;
         }
      }
   }

   if (getWordCount(%message) > 1 || strPos(%message, " ") != -1) {
      switch$ (%command) {
      case "/whisper" or "/slap":
         %current = getSubStr(getWord(%message, 1), 0, strlen(%message));
         for (%i = 0; %i < $LB::UserlistCount; %i ++) {
            %row = $LB::UserListUser[%i];
            %username = decodeName(getWord(%row,0));
            if (%username $= $LB::Username && %command !$= "/slap")
               continue;
            if (strPos(strlwr(%username), strlwr(%current)) == 0) {
               %username = getSubStr(%username, strlen(%current), strlen(%username));
               if (%completions $= "")
                  %completions = %username;
               else
                  %completions = %completions NL %username;
            }
         }
      case "/vtaunt" or "/v":
         %current = getSubStr(getWord(%message, 1), 0, strlen(%message));
         %list = "chick\nasdf\nhaha\nsloser\nmega\nmp\ncmon\npomp\npq\nraise\nowned\n1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11";
         for (%i = 0; %i < getRecordCount(%list); %i ++) {
            %record = getRecord(%list, %i);
            if (strPos(strlwr(%record), strlwr(%current)) == 0) {
               %record = getSubStr(%record, strlen(%current), strlen(%record));
               if (%completions $= "")
                  %completions = %record;
               else
                  %completions = %completions NL %record;
            }
         }
      }
   }
   return %completions;
}
