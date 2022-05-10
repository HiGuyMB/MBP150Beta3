//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

function EndGameGui::onWake(%this)
{
   EG_Continue.setVisible($Server::ServerType $= "SinglePlayer" || $Server::Hosting);
   EG_Restart.setVisible($Server::ServerType $= "SinglePlayer" || $Server::Hosting);
   EG_NextLevel.setVisible($Server::ServerType $= "SinglePlayer" && !$randomMission);
}

function getLevelBitmap(%mission) {
   //HiGuy: If the image exists, no need to looking for it
   if (isFile(filePath(%mission.file) @ "/" @ fileBase(%mission.file) @ ".png"))
         return (filePath(%mission.file) @ "/" @ fileBase(%mission.file));
   else if (isFile(filePath(%mission.file) @ "/" @ fileBase(%mission.file) @ ".jpg"))
         return (filePath(%mission.file) @ "/" @ fileBase(%mission.file));
   else {
      // Jeff: i think i went a bit insane here, but it seems to find the image
      if (%mission.type $= "LBCustom" || %mission.game $= "LBCustom")
         %path = $usermods @ "/data/lb_custom/*.mis";
      else
         %path = $usermods @ "/data/missions" @ ((%mission.game $= "Platinum") ? "_mbp" : "_mbg") @ "/" @ %mission.type @ "/*.mis";
      for (%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path)) {
         if (fileBase(%file) $= fileBase(%mission.file)) {
            %file = filePath(%file) @ "/" @ fileBase(%file);
            break;
         }
      }
      return %file;
   }
}

function getQualifyInfo(%type, %game) {
   if (%game $= "Gold")
      return $pref::MBGQualifiedLevel[%type];
   if (%game $= "Platinum")
      return $pref::QualifiedLevel[%type];
   return $pref::QualifiedLevel[%type];
}

function EndGameGui::loadNextBitmap(%this) {
   if (%this.loadedBitmap)
      return;
   EG_NextLevel.setVisible($Server::ServerType $= "SinglePlayer" && !$randomMission);

   %this.loadedBitmap = true;

   %list = ($LB::LoggedIn ? $LB::PlayMissionList : PM_MissionList);
   %next = ($LB::LoggedIn ? $LBPref::SelectedRow : PM_MissionList.getSelectedId());


   echo(%next SPC %list.rowCount());

   //HiGuy: Next mission in the same difficulty is easy
   if (%next < %list.rowCount()) {
      if (!$LB::LoggedIn)
         %next ++;
      //if ($LB::LoggedIn && $LB::MissionType $= "Custom")
         //%next ++;

      %mission = getField(%list.getRowTextById(%next), ($LB::LoggedIn ? 0 : 1));
      if (%mission.file $= $Server::MissionFile) {
         %next ++;
         %mission = getField(%list.getRowTextById(%next), ($LB::LoggedIn ? 0 : 1));
      }
      %file = getLevelBitmap(%mission);
      EG_Preview.setBitmap(%file);

      %qualified = $QualifyAllLevels || $MissionType $= "" || (%next < getQualifyInfo(%mission.type, $CurrentGame));

      //TODO: Qualifications

      EG_Next.command = "nextMission(\"" @ %mission.file @ "\");";
      if ($LB::LoggedIn)
         $LBPref::SelectedRow = %next;
      else
         PM_MissionList.setSelectedById(%next);
   } else {
      //HiGuy: The last mission is more confusing

      //HiGuy: Get the next difficulty, or Beginner if we've finished them all
      %nextDiff = "Intermediate";
      switch$ (($LB::LoggedIn ? $LB::MissionType : $MissionType)) {
      case "Beginner":
         %nextDiff = "Intermediate";
      case "Intermediate":
         %nextDiff = "Advanced";
      case "Advanced":
//         if ($CurrentGame $= "Platinum")
//            %nextDiff = "Expert";
//         else
            %nextDiff = "Beginner";
//      case "Expert":
//         %nextDiff = "Beginner";
      default:
         %nextDiff = "Beginner";
      }

      //HiGuy: Set the playmissions to match the new difficulty so we can grab
      // the mission from them. This is different for LB / non-LB
      echo("Loading bitmap from next difficulty" SPC %nextDiff);
      if ($LB::LoggedIn) {
         %preDiff = $LB::MissionType;
         %preSel = $LBPref::SelectedRow;
         LBSetMissionType(%nextDiff);
         LBSetSelected(0);

         %list = $LB::PlayMissionList;
         $LB::PlayMissionList.setSelectedById(1);
         %next = 0;
      } else {
         %preDiff = $MissionType;
         %preSel = PM_MissionList.getSelectedId();
         PMSetMissionTab(%nextDiff);
         PM_SetSelected(0);
         PM_MissionList.setSelectedById(1);
         %next = 1;
      }

      //HiGuy: Easy snatch
      %mission = getField(%list.getRowTextById(%next), ($LB::LoggedIn ? 0 : 1));
      %file = getLevelBitmap(%mission);

      EG_Preview.setBitmap(%file);
      EG_Next.command = "nextMission(\"" @ %mission.file @ "\", \"" @ %nextDiff @ "\");";

      //HiGuy: Revert to our old difficulty so as not to confuse people if they
      // don't press next
      if ($LB::LoggedIn) {
         LBSetMissionType(%preDiff);
         LBSetSelected(%preSel);
         $LB::PlayMissionList.setSelectedById(%preSel);
      } else {
         PMSetMissionTab(%preDiff);
         PM_SetSelected(%preSel - 1);
         PM_MissionList.setSelectedById(%preSel);
      }
   }
}

function EndGameGui::resetBitmap(%this) {
   if ($Server::MissionFile !$= $LastMissionFile) {
      $LastMissionFile = $Server::MissionFile;
      %this.loadedBitmap = false;
   }
}

function nextMission(%file, %difficulty) {
	if ($doRecordDemo) {
      Canvas.pushDialog(CompleteDemoDlg);
   } else {
      if (%difficulty !$= "") {
         if ($LB::LoggedIn) {
            LBSetMissionType(%difficulty);
            LBSetSelected(0);
            $LB::PlayMissionList.setSelectedById(0);
         } else {
            PMSetMissionTab(%difficulty);
            PM_SetSelected(0);
            PM_MissionList.setSelectedById(1);
         }
      }
      %info = getMissionInfo(%file);
      if ($CurrentGame $= "Platinum")
         $Pref::LastMissionPlayed[$MissionType] = %info.name !$= "" ? %info.name : filebase(%file);
      else
         $Pref::LastMBGMissionPlayed[$MissionType] = %info.name !$= "" ? %info.name : filebase(%file);
      $Server::Started = false;
      loadMission(%file);
   }
}
