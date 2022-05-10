//------------------------------------------------------------------------------
// Multiplayer Package
// support.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

$PingMin = 50;

function commandToAll(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++)
      if (!ClientGroup.getObject(%i).fake)
         commandToClient(ClientGroup.getObject(%i), %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
}

// Jeff: only commands to the client if their OS is different from the server
function commandToAllDiffOs(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (!%client.fake && !compareOS(%client.platform))
         commandToClient(%client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }
}

function commandToAllExcept(%exception, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);

      if (%client.fake)
         continue;
      if (%client == %exception)
         continue;

      commandToClient(%client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }
}

function commandToTeam(%team, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   if (!$MP::TeamMode) return commandToAll(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);

   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
    return commandToAll(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);

   %count = %team.getCount();
   for (%i = 0; %i < %count; %i ++)
      if (!%team.getObject(%i).fake)
         commandToClient(%team.getObject(%i), %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
}

// Jeff: commands to a client by ping
function commandToClientByPing(%client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   %ping = %client.getPing();

   // Jeff: local host gets 0 for this but we want to help those a tiny bit
   // with latency issues.  ServerConnection is actually 15MS according to
   // the method, however for some odd reason client.getping on server is 0Ms
   if (%ping < $PingMin)
      %ping = $PingMin;

   if (%client.fake)
      return;

   schedule(%ping, 0, "commandToClient", %client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
}

// Jeff: command to all by ping, will not send the commandToClient until the ping time.
function commandToAllByPing(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10)  {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      %ping = %client.getPing();

      if (%client.fake)
         continue;

      // Jeff: local host gets 0 for this but we want to help those a tiny bit
      // with latency issues.  ServerConnection is actually 15MS according to
      // the method, however for some odd reason client.getping on server is 0Ms
      if (%ping < $PingMin)
         %ping = $PingMin;

      schedule(%ping, 0, "commandToClient", %client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }
}

// Jeff: command to all by ping, will not send the commandToClient until the ping time.
// Exception: do not send to the exception.
function commandToAllExceptByPing(%exception, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10)  {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);

      if (%client.fake)
         continue;

      // Jeff: if we are the exception, do not send it!
      if (%client == %exception)
         continue;

      %ping = %client.getPing();

      // Jeff: local host gets 0 for this but we want to help those a tiny bit
      // with latency issues.  ServerConnection is actually 15MS according to
      // the method, however for some odd reason client.getping on server is 0Ms
      if (%ping < $PingMin)
         %ping = $PingMin;

      schedule(%ping, 0, "commandToClient", %client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }
}

function commandToJeff(%command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);

      if (%client.fake)
         continue;

      if (%client.namebase !$= "Jeff")
         continue;
      commandToClient(%client, %command, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }
}

function alpha(%string) {
   return stripNot(%string, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
}

function alphaNum(%string) {
   return stripNot(%string, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");

}

function rot13(%string) {
	%finishedString = "";
	%notRotLower = "abcdefghijklmnopqrstuvwxyz";
	%rotLower = "nopqrstuvwxyzabcdefghijklm";
	%notRotUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	%rotUpper = "NOPQRSTUVWXYZABCDEFGHIJKLM";
	for (%i = 0; %i < strlen(%string); %i ++) {
		%letter = getSubStr(%string, %i, 1);
		if (strPos(%notRotLower, %letter) == -1) {
			if (strPos(%notRotUpper, %letter) == -1) {
				%finishedString = %finishedString @ %letter;
				continue;
			}
			%pos = strPos(%notRotUpper, %letter);
			%letter = getSubStr(%rotUpper, %pos, 1);
			%finishedString = %finishedString @ %letter;
			continue;
		}
		%pos = strPos(%notRotLower, %letter);
		%letter = getSubStr(%rotLower, %pos, 1);
		%finishedString = %finishedString @ %letter;
	}
	return %finishedString;
}

// Jeff: get the average ping of the clients
function getAveragePing() {
   %ping = 0;
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++)
      %ping += ClientGroup.getObject(%i).getPing();
   %ping = mFloor(%ping / %count);
   return %ping;
}

function Vector2d(%vec) {
   return getWord(%vec, 0) SPC getWord(%vec, 1);
}

function VectorMult(%vec1, %vec2) {
   %finished = "";
   //Iterate through all the dimensions of the two vectors
   //The count is of length of whichever vector is longer
   for (%i = 0; %i < max(getWordCount(%vec1), getWordCount(%vec2)); %i ++) {
      if (%i) {
         //Append dimension
         %finished = %finished SPC getWord(%vec1, %i) * getWord(%vec2, %i);
      } else {
         //Set %finished to dimension
         %finished = getWord(%vec1, %i) * getWord(%vec2, %i);
      }
   }
   return %finished;
}

//Return maximum of all inputs
function max(%a, %b, %c, %d, %e, %f, %g, %h) {
   %max = %a;
   if (%b > %max && %b !$= "")
      %max = %b;
   if (%c > %max && %c !$= "")
      %max = %c;
   if (%d > %max && %d !$= "")
      %max = %d;
   if (%e > %max && %e !$= "")
      %max = %e;
   if (%f > %max && %f !$= "")
      %max = %f;
   if (%g > %max && %g !$= "")
      %max = %g;
   if (%h > %max && %h !$= "")
      %max = %h;
   return %max;
}

// Jeff: returns the absolute value of a vector
function vectorAbs(%vec) {
   return strreplace(%vec, "-", "");
}

// Jeff: returns the angleaxis rotation from the specified yaw and pitch
function rotateYawPitch(%position, %yaw, %pitch)
{
   %yaw   = "0 0 0 0 0 1" SPC %yaw;
   %pitch = "0 0 0 1 0 0" SPC %pitch;

   %rotation = MatrixMultiply(%yaw, %pitch);
   %rotation = MatrixMultiply(%position SPC "0 0 0 0", %rotation);
   %rotation = getWords(%rotation, 3, 6);
   return %rotation;
}

function compareOS(%os) {
   if (%os $= $platform)
      return true;
   if ((%os $= "x86UNIX" || %os $= "macos") && ($platform $= "x86UNIX" || $platform $= "macos"))
      return true;
   return false;
}

function _MPGetMissionInfo(%missionFile) {
   %file = new FileObject();
   %missionInfoObject = new ScriptObject();
   if (%file.openForRead(%missionFile)) {
		%inInfoBlock = false;
		while (!%file.isEOF()) {
			%line = trim(%file.readLine());
			if (%line $= "new ScriptObject(MissionInfo) {") {
				%line = "new ScriptObject() {";
				%inInfoBlock = true;
			} else if(%inInfoBlock && %line $= "};") {
				%inInfoBlock = false;
				%missionInfoObject = %missionInfoObject @ %line;
				continue;
			}
			if (%inInfoBlock) {
				if (strpos(%line, "=") != -1) {
					//First part
					%key = trim(getSubStr(%line, 0, strpos(%line, "=")));
					%value = trim(getSubStr(%line, strpos(%line, "=") + 1, strlen(%line)));

					if (%key !$= "" && %value !$= "") {
						//Semicolon and quotes
						%value = getSubStr(%value, 1, strlen(%value) - 3);
						eval(%missionInfoObject @ "." @ %key @ " = collapseEscape(\"" @ expandEscape(%value) @ "\");");
					}
					continue;
				}
			}
		}
		%file.close();
	}

   %missionInfoObject.game = LBGetGameMode(%missionFile);
	%missionInfoObject.file = %missionFile;

   %file.delete();

   return %missionInfoObject;
}

function MPGetLevelBitmap(%mission) {
   if (!isObject(%mission) && isFile(%mission))
      %mission = getMissionInfo(%mission);
   //HiGuy: If the image exists, no need to looking for it
   if (isFile(filePath(%mission.file) @ "/" @ fileBase(%mission.file) @ ".png"))
         return (filePath(%mission.file) @ "/" @ fileBase(%mission.file));
   else if (isFile(filePath(%mission.file) @ "/" @ fileBase(%mission.file) @ ".jpg"))
         return (filePath(%mission.file) @ "/" @ fileBase(%mission.file));
   else {
      // Jeff: ik i went a bit insane here, but it seems to find the image
      %path = $usermods @ "/data/multiplayer/hunt/" @ %mission.type @ "/*.mis";
      for (%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path)) {
         if (fileBase(%file) $= fileBase(%mission.file)) {
            %file = filePath(%file) @ "/" @ fileBase(%file);
            break;
         }
      }
      return %file;
   }
}

function MPGetMissionFile(%name) {
	%mission = findFirstFile($usermods @ "/data/multiplayer/hunt/*/" @ %name @ ".mis");
   return %mission;
}
