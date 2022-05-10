//-----------------------------------------------------------------------------
// support.cs
// Copyright (c) The Platinum Team
// Mainly written by Jeff and HiGuy
//
// Jeff: this file supports core material and important functions
//-----------------------------------------------------------------------------

// Jeff: destroysTorqueML torqueML from a string, so that we can't have nuttcases
// using differnt colors in the chat and stuff :)
function destroyTorqueML(%string) {
   %finish = "";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %subStr = getSubStr(%string,%i,1);
      // Jeff: will goof up torqueML and embarass the user
      if (%subStr $= "<")
         %subStr = "<<spush><spop>";
      %finish = (%finish $= "") ? %subStr : %finish @ %subStr;
   }
   return %finish;
}

// Jeff: check to see if a person is a guest
function isGuest(%user) {
   return (strStr(%user, "Guest_") != -1);
}

function GameConnection::isGuest(%this) {
   return isGuest(%this.nameBase);
}

// Jeff: disconnect all tcp objects that are challenges except forfeit
// used in forfeiting and login so that we don't get that silly "end game"
// screen for challenges when that happens sometimes
function disconnectChallengeTCP() {
   %count = TCPGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %obj  = TCPGroup.getObject(%i);
      %name = %obj.getName();
      if (strpos(%name, "Challenge") == -1 && strpos(%name, "challenge") == -1)
         continue;
      if (%name $= "LBChallengeForfeitNetwork")
         continue;
      %obj.destroy();
   }
}

// Jeff: convert number to string for status
function LBResolveStatus(%status) {
   switch (%status) {
      case -3: //HiGuy: Banned
         %status = "(Banned)";
      case -2: //HiGuy: Loading
         %status = "(Loading)";
      case -1: // Jeff: invisible
         %status = "(Invisible)";
      case 1: // Jeff: level select
         %status = "(Level Select)";
      case 2: // Jeff: playing
         %status = "(Playing)";
      case 3: // Jeff: webchat
         %status = "(Webchat)";
      case 4: // Jeff: challenge
         %status = "(Challenge)";
      case 5: // Jeff: super challenge
         %status = "(Super Challenge)";
      case 6: //HiGuy: Hosting MP
         %status = "(Hosting)";
      case 7: //HiGuy: Joining MP
         %status = "(Game Lobby)";
      default: // Jeff: normal
         %status = "";
   }
   return %status;
}

function LBColorFormat(%username, %access) {
   switch (%access) {
      case 0:
         %username = LBChatColor("normal") @ %username;
      case 1:
         %username = LBChatColor("mod") @ %username;
      case 2:
         %username = LBChatColor("admin") @ %username;
   }
   %username = "<spush>" @ %username @ "<spop>";
   return %username;
}

function LBAccountType(%access) {
   switch (%access) {
      case 0:
         %username = LBChatColor("normal") @ "User";
      case 1:
         %username = LBChatColor("mod") @ "Moderator";
      case 2:
         %username = LBChatColor("admin") @ "Administrator";
   }
   %username = "<spush>" @ %username @ "<spop>";
   return %username;
}

function LBSpecialColor(%number, %alt) {
	if (%number == 1)
		%color = "<color:DAA520><shadow:1:1><shadowcolor:0000007f>";
	else if (%number == 2)
		%color = "<color:E3E4E5><shadow:1:1><shadowcolor:0000007f>";
	else if (%number == 3)
		%color = "<color:A67B3D><shadow:1:1><shadowcolor:0000007f>";
	else if (%number <= 5 && %alt)
		%color = "<color:000000><shadow:1:1><shadowcolor:FF00FF3f>";
	return %color;
}

function decodeName(%name) {
   %name = strReplace(%name, "-SPC-", " ");
   return %name;
}

function encodeName(%name) {
   %name = strReplace(%name, " ", "-SPC-");
   return %name;
}

function mRound(%num, %places) {
   %mult = mPow(10, (%places $= "" ? 0 : %places));
   %num *= %mult;
   if ((%num * 2) % 2) return mCeil(%num) / %mult;
   else return mFloor(%num) / %mult;
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

function min(%a, %b, %c, %d, %e, %f, %g, %h) {
   %min = %a;
   if (%b < %min && %b !$= "")
      %min = %b;
   if (%c < %min && %c !$= "")
      %min = %c;
   if (%d < %min && %d !$= "")
      %min = %d;
   if (%e < %min && %e !$= "")
      %min = %e;
   if (%f < %min && %f !$= "")
      %min = %f;
   if (%g < %min && %g !$= "")
      %min = %g;
   if (%h < %min && %h !$= "")
      %min = %h;
   return %min;
}

//-----------------------------------------------------------------------------
// String encryption / decryption by HiGuy.
// Don't ask, I'm not willing to explain it.
//-----------------------------------------------------------------------------

function strEnc(%string, %method) {
   %start = getRealTime();
   %table = "abcdefghijklmnopqrstuvwxyzABCDEFGHJIKLMNOPQRSTUVWXYZ1234567890 -=_+`~[]{}\\|;:\'\",.<>/?()*&^%$#@!";
   setRandomSeed(getRealTime());
   %seed = getRandom(getRealTime());
   while (%seed < 100000)
      %seed *= 9;
   while (%seed > 100000)
      %seed /= 9;
   %seed = mFloor(%seed);
   if (%seed == 100000)
      %seed --;
   %tableSc = strScr(%table, %seed, (%method == 0 ? 5 : 15));
   %finished = strLet(%seed);
   for (%i = 0; %i < strLen(%string); %i ++) {
//      devecho("TABLESC IS" SPC %tableSc);
      %char = getSubStr(%string, %i, 1);
      %pos = strPos(%tableSc, %char);
      if (%pos == -1)
         %char2 = %char;
      else
         %char2 = getSubStr(%table, %pos, 1);
      %finished = %finished @ %char2;
      if (%method == 2)
         %tableSc = getSubStr(%tableSc, 7, strlen(%tableSc)) @ getSubStr(%tableSc, 0, 7);
   }
   return %finished;
}

function strDec(%string, %method) {
   %table = "abcdefghijklmnopqrstuvwxyzABCDEFGHJIKLMNOPQRSTUVWXYZ1234567890 -=_+`~[]{}\\|;:\'\",.<>/?()*&^%$#@!";
   %seed = strNum(getSubStr(%string, 0, 5));
   %tableSc = strScr(%table, %seed, (%method == 0 ? 5 : 15));
   %string = getSubStr(%string, 5, strLen(%string));
   %finished = "";
   for (%i = 0; %i < strLen(%string); %i ++) {
//      devecho("TABLESC IS" SPC %tableSc);
      %char = getSubStr(%string, %i, 1);
      %pos = strPos(%table, %char);
      if (%pos == -1)
         %char2 = %char;
      else
         %char2 = getSubStr(%tableSc, %pos, 1);
      %finished = %finished @ %char2;
      if (%method == 2)
         %tableSc = getSubStr(%tableSc, 7, strlen(%tableSc)) @ getSubStr(%tableSc, 0, 7);
   }
   return %finished;
}

function strScr(%string, %seed, %count) {
   %oldSeed = getRandomSeed();
   if (%seed)
      setRandomSeed(%seed);
   for (%i = 0; %i < strLen(%string) * %count; %i ++) {
      %pos = getRandom(strLen(%string));
      %pos2 = getRandom(strLen(%string));
      %char = getSubStr(%string, %pos, 1);
      %before = getSubStr(%string, 0, %pos);
      %after = getSubStr(%string, %pos + 1, strLen(%string));
      %string = %before @ %after;
      %before = getSubStr(%string, 0, %pos2);
      %after = getSubStr(%string, %pos2, strLen(%string));
      %string = %before @ %char @ %after;
   }
   setRandomSeed(%oldSeed);
   return %string;
}

function strLet(%number) {
   %let = "abcdefghij";
   %fin = "";
   for (%i = 0; %i < strLen(%number); %i ++) {
      %char = getSubStr(%number, %i, 1);
      %char2 = getSubStr(%let, %char, 1);
      %fin = %fin @ %char2;
   }
   return %fin;
}

function strNum(%string) {
   %let = "abcdefghij";
   %fin = "";
   for (%i = 0; %i < strLen(%string); %i ++) {
      %char = getSubStr(%string, %i, 1);
      %pos = strPos(%let, %char);
      %fin = %fin @ %pos;
   }
   return %fin;
}

//-----------------------------------------------------------------------------

function lastPos(%string, %search) {
   //Return the last case of %search in %string
   //Example:
   //lastPos("this_text_is_text", "_")
   //Would return strLen("this_text_is");
   for (%i = strLen(%string) - 1; %i >= 0; %i --) {
      %sub = getSubStr(%string, %i, strLen(%search));
      if (%sub $= %search)
         return %i;
   }
   return strLen(%string);
}

function lastPos2(%string, %search) {
   //Return the last case of %search in %string
   //Example:
   //lastPos("this_text_is_text", "_")
   //Would return strLen("this_text_is");
   for (%i = strLen(%string) - 1; %i >= 0; %i --) {
      %sub = getSubStr(%string, %i, strLen(%search));
      if (%sub $= %search)
         return %i;
   }
   return -1;
}

//-----------------------------------------------------------------------------

// HiGuy: strrpos is a wonderful C function which was *NOT* included...
function strrpos(%string, %blah) {
   %lastPos = 0;
   if (strPos(%string, %blah) == -1)
      return -1;
   while ((%a = strPos(%string, %blah, %lastPos + 1)) != -1)
      %lastPos = %a;
   return %lastPos;
}

//HiGuy: Weak "encrypts" a string so it can't be seen in clear-text
function garbledeguck(%string) {
   %finish = "";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %char = getSubStr(%string, %i, 1);
      %val = chrValue(%char);
      %val = 128 - %val;
      %hex = dec2hex(%val, 2);
      %finish = %hex @ %finish; //Why not?
   }
   return "gdg" @ %finish;
}

function deGarbledeguck(%string) {
	if (getSubStr(%string, 0, 3) !$= "gdg")
		return %string;
	%finish = "";
	for (%i = 3; %i < strLen(%string); %i += 2) {
		%hex = getSubStr(%string, %i, 2);
		%val = hex2dec(%hex);
		%char = chrForValue(128 - %val);
		%finish = %char @ %finish;
	}
	return %finish;
}

function strRepeat(%string, %times) {
   if (%times < 1)
      return;
   %ret = %string;
   for (%i = 1; %i < %times; %i ++)
      %ret = %ret @ %string;
   return %ret;
}

function stripNot(%str, %strip) {
   %finish = "";
   for (%i = 0; %i < strLen(%str); %i ++) {
      if (strPos(%strip, (%char = getSubStr(%str, %i, 1))) != -1)
         %finish = %finish @ %char;
   }
   return %finish;
}

function LBGetMissionFile(%name) {
   %mission = findFirstFile($usermods @ "/data/lb*/*/" @ %name @ ".mis");
   //HiGuy: After getting quite a few GG missions, I've added this in
   // to fix the problem.
   while (strStr(%mission, "gg/") != -1 || strStr(%mission, ".svn/") != -1) {
      %mission = findNextFile($usermods @ "/data/lb*/*/" @ %name @ ".mis");
   }
   //HiGuy: At this point, it's safe to assume this mission is multiplayer
   if (%mission $= "")
      %mission = findFirstFile($usermods @ "/data/multiplayer/hunt/*/" @ %name @ ".mis");
   return %mission;
}

// Jeff: grab mission info
function getMissionInfo(%file, %keep) {
   if (!isFile(%file))
      %file = LBGetMissionFile(%file);
   %fo = new FileObject();
   %inInfoBlock = false;
   %egg = false;
   %gems = 0;
   %info = "";
   if (%fo.openForRead(%file)) {
      while (!%fo.isEOF()) {
         %line = trim(%fo.readLine());
         if ((!%inInfoBlock) && (strStr(%line,"new ScriptObject(MissionInfo) {") != -1)) {
            %info = "new ScriptObject() {";
            %inInfoBlock = true;
            continue;
         } else if ((%inInfoBlock) && (strStr(%line, "};") != -1)) {
            %info = %info NL %line;
            %inInfoBlock = false;
            continue;
         } else if (%inInfoBlock) {
            %info = %info NL %line;
            continue;
         }
         if (strStr(%line,"EasterEgg") != -1) // Jeff: easter egg!
            %egg = true;
         else if (strStr(%line,"Gem") != -1) // Jeff: gems!
            %gems ++;
      }
      %fo.close();
   }
   %fo.delete();
   if (%info $= "")
      return -1;
   eval("%id=" @ %info);
   if (!isObject(%id))
      return -1;
   %info = %id.getId();
   if (%info.game $= "")
      %info.game = (strStr(%file, "_mbp/") != -1 ? "Platinum" : (strStr(%file, "_mbg/") != -1 ? "Gold" : (strStr(%file, "lb_custom/") != -1 ? "LBCustom" : "Custom")));
   %info.file = %file;
   %info.easterEgg = %egg;
   %info.gems = %gems;
   if (!%keep)
      %info.schedule(5000,"delete");
   return %info.getId();
}

// Jeff: make sure this is defined!
if (!isObject(CharArrayGroup))
{
   new SimGroup(CharArrayGroup);
   RootGroup.add(CharArrayGroup);
}

// Jeff: this function turns a string into a char array
function strToCharArray(%string, %name, %delete)
{
   // Jeff: make sure name is a string as it is an
   // optional parameter!
   if (%name $= "" || !%name)
      %name = "";

   %length = strlen(%string);

   // Jeff: create the script object
   %script = new ScriptObject(%name)
   {
      length = %length;
      string = %string;
   };
   CharArrayGroup.add(%script);

   if (%delete)
      %script.schedule(10000, "delete");

   for (%i = 0; %i < %length; %i ++)
      %script.char[%i] = getSubStr(%string, %i, 1);
   return %script;
}

function LBGetGameMode(%file) {
   return (strStr(%file, "_mbp/") != -1 ? "Platinum" : (strStr(%file, "_mbg/") != -1 ? "Gold" : (strStr(%file, "lb_custom/") != -1 ? "LBCustom" : "Custom")));
}

function fwrite(%file, %text) {
   %o = new FileObject();
   if (!%o.openForWrite(%file)) {
      %o.close();
      %o.delete();
      return false;
   }
   %o.writeLine(%text);
   %o.close();
   %o.delete();
   return true;
}

function fread(%file) {
   %o = new FileObject();
   if (!%o.openForRead(%file)) {
      %o.close();
      %o.delete();
      return "";
   }
   %ret = "";
   while (!%o.isEOF()) {
      %ret = %ret @ %o.readLine();
      %ret = %ret @ "\n";
   }
   %o.close();
   %o.delete();
   return %ret;
}

function safeExecPrefs(%file) {
   %conts = fread(%file);
   %records = getRecordCount(%conts);
   for (%i = 0; %i < %records; %i ++) {
      %line = getRecord(%conts, %i);
      //HiGuy: Evaluate each line:
      // $variable = value;
      //
      // Now, this can be easily exploited if someone knows how.
      // That's why I've come up with this method that makes it virtually
      // impossible to hack. Read on and find out!

      //HiGuy: Step 0: Cut out comments and ignore blank lines

      //HiGuy: Clip off anything after //
      if (strPos(%line, "//") != -1)
         %line = getSubStr(%line, 0, strPos(%line, "//"));

      //HiGuy: Why would we evaluate this line anyway?
      if (trim(%line) $= "")
         continue;

      %line = trim(%line);

      //HiGuy: Step 1: Make sure the line is valid

      //HiGuy: Must have a "$" at the start
      if (strPos(%line, "$") != 0)
         continue;

      //HiGuy: Must have a ";" at the end
      if (strPos(%line, ";") != strLen(%line) - 1)
         continue;

      //HiGuy: Must have an "=" sign
      if (strPos(%line, "=") == -1)
         continue;

      //HiGuy: Now we've verified that it is probably a prefs line. How can we
      // be sure they haven't done this though: "$foo = bar; hax();"?

      //HiGuy: Step 2: Splitting it into parts

      %nameEnd = strPos(%line, "=");
      if (%nameEnd == -1)
         continue;

      while (getSubStr(%line, %nameEnd - 1, 1) $= " ")
         %nameEnd --;

      %name = getSubStr(%line, 1, %nameEnd - 1);

      %varStart = strPos(%line, "=") + 1;
      while (getSubStr(%line, %varStart, 1) $= " ")
         %varStart ++;

      %var = getSubStr(%line, %varStart, strlen(%line));
      %var = getSubStr(%var, 0, strlen(%var) - 1);

      //echo("Name: \"" @ %name @ "\" Var: \"" @ %var @ "\"");

      //HiGuy: Part 3: Fixing the parts

      %name = stripNot(%name, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890:[]");
      if (getSubStr(%var, 0, 1) $= "\"")
         %var = getSubStr(%var, 1, strlen(%var) - 2);
      %var = "\"" @ expandEscape(%var) @ "\"";

      //HiGuy: Evaluate the bits

      devecho("eval: $" @ %name @ " = " @ %var @ ";");
      devecho("eval: $" @ %name @ " = " @ "collapseEscape($" @ %name @ ");");
      eval("$" @ %name @ " = " @ %var @ ";");
      eval("$" @ %name @ " = " @ "collapseEscape($" @ %name @ ");");
   }
}

function dumpObject(%obj, %format) {
   if (%obj $= "RootGroup") //just no.
      return "";
   if (!isObject(%obj))
      return "";
   if ($Server::Dedicated) {
      //We don't have that fancy snazz
      %saveSpot = $usermods @ "/data/.nullfile";
      %obj.save(%saveSpot);
      %in = new FileObject();
      if (!%in.openForRead(%saveSpot)) {
         %in.close();
         %in.delete();
         return "";
      }
      %object = "";
      while (!%in.isEOF()) {
         %line = %in.readLine();
         if (strPos(%line, "//") == 0)
            continue;
         if (strPos(%line, "new ") == 0) {
            %object = "new" SPC %obj.getClassName() @ "(" @ %obj.getName() @ ") {";
            continue;
         }
         if (strPos(%line, "new ") == 3 || strPos(%line, "};") == 0) {
            %object = (%format ? %object NL "};" : %object @ "};");
            if (%format == 3)
               echo(%object);
            break;
         }
         %spaces = true;
         if (getSubStr(%line, 4, 1) $= " ")
            %spaces = false;
         %line = trim(%line);
         if (%line $= "" || strlen(%line) < 4)
            continue;
         %name = getWord(%line, 0);
         %name = (%format ? (%spaces ? "   " : "      ") : "") @ %name;
         %value = getWords(%line, 2);
         %value = getSubStr(%value, 1, strLen(%value) - 3);
         %object = (%format ? %object NL %name : %object @ %name) @ (%format == 2 ? ":" : " = \"") @ %value @ (%format == 2 ? "" : "\";");
      }
      %in.close();
      %in.delete();
      %j = new FileObject();
      if (%j.openForWrite(%saveSpot)) {
         %j.writeLine("");
      }
      %j.close();
      %j.delete();
      return %object;
   }
   %ret = "new" SPC %obj.getClassName() @ "(" @ %obj.getName() @ ") {";
   if (!isObject(dumpInspectFields)) {
      new GuiInspector(dumpInspectFields) {
         profile = "GuiDefaultProfile";
         horizSizing = "width";
         vertSizing = "bottom";
         position = "0 0";
         extent = "8 8";
         minExtent = "8 8";
         visible = "true";
         setFirstResponder = "false";
         modal = "true";
         helpTag = "0";
      };
      RootGroup.add(dumpInspectFields);
   }
   dumpInspectFields.inspect(%obj);
   //Get each field...
   for (%i = 0; %i < dumpInspectFields.getCount(); %i ++) {
      %field = dumpInspectFields.getObject(%i);
      if (strStr(%field.getName(), "InspectStatic") == 0) {
         %name = (%format == 1 ? "   " : "") @ getSubStr(%field.getName(), strLen("InspectStatic"), strLen(%field.getName()));
         %name = getSubStr(%name, 0, lastPos(%name, "_"));
         %value = %field.getValue();
         if (%value !$= "" && %value !$= "<NULL>")
            %ret = (%format ? %ret NL %name : %ret @ %name) @ (%format == 2 ? ":" : " = \"") @ %value @ (%format == 2 ? "" : "\";");
      }
      if (strStr(%field.getName(), "InspectDynamic") == 0) {
         %name = (%format == 1 ? "      " : "") @ getSubStr(%field.getName(), strLen("InspectDynamic"), strLen(%field.getName()));
         %name = getSubStr(%name, 0, lastPos(%name, "_"));
         %value = %field.getValue();
         if (%value !$= "" && %value !$= "<NULL>")
            %ret = (%format ? %ret NL %name : %ret @ %name) @ (%format == 2 ? ":" : " = \"") @ %value @ (%format == 2 ? "" : "\";");
      }
   }
   %ret = (%format ? %ret NL "};" : %ret @ "};");
   if (%format == 3)
      devecho(%ret);
   return %ret;
}


function LBResolveName(%name, %notitle) {
   for (%i = 0; %i < $LB::UserlistCount; %i ++) {
      %user = $LB::UserlistUser[%i];
      %uname = getWord(%user, 0);
      if (strlwr(%uname) $= strlwr(%name)) {
         %name = getWord(%user, 3);
         %title = getWord(%user, 4);
         if (%title !$= "" && !%notitle)
            %name = decodeName(%name) SPC "[" @ decodeName(%title) @ "]";
         return decodeName(%name);
      }
   }
   return decodeName(%name);
}


// Jeff: chat color reference guide
// <color:000000> - User / normal text
// <color:0000CC> - Mod
// <color:CC0000> - Admin
// <color:FFCC33> - Whisper
// <color:CC9900> - Notification
// <color:669900> - welcome message

//-----------------------------------------------------------------------------
//HiGuy: New colors system allows for having the same text be different colors
// on different interfaces. Basic usage of the system is as follows:
//
// Defining a color:
//    LBRegisterChatColor("Test Color", "00ccff", "00ccff", "ffccff");
// Args are:
//    Color Name, LBChat Color, In-Game Color, MPPlayMission Color
//
// Referencing a color:
//    LBChatColor("Test Color")
// Args are:
//    Color Name
// Returns:
//    Color Code (e.g. \x14\x1A)
//
// Resolving color codes for displaying text:
//    LBResolveChatColors(<block of text>, "chat");
// Args are:
//    Text to Resolve, Color Location (e.g. "chat" or "ingame")
//

$LBChatColors = 0;

function LBRegisterChatColor(%name, %chat, %ingame, %mp) {
   %num = $LBChatColors;
   $LBChatColor[%name] = %num;
   $LBChatColor[%num, "name"]   = %name;
   $LBChatColor[%num, "chat"]   = "<color:" @ %chat @ ">";
   $LBChatColor[%num, "ingame"] = "<color:" @ %ingame @ ">";
   $LBChatColor[%num, "mp"]     = "<color:" @ %mp @ ">";
   %short = "";
   while (%num > 0) {
      %mod = %num % 14;
      %num -= %mod;
      %num /= 14;
      //\\x12 - \\x1F are free
      %char = collapseEscape("\\x" @ dec2hex(%mod + 18));
      %short = %char @ %short;
   }
   $LBChatColor[$LBChatColors, "replace"] = %short;
   $LBChatColors ++;

   return %short;
}

function LBChatColor(%name) {
   return $LBChatColor[$LBChatColor[%name], "replace"];
}

function LBResolveChatColors(%str, %type) {
   for (%i = 0; %i < $LBChatColors; %i ++)
      %str = strReplace(%str, $LBChatColor[%i, "replace"], $LBChatColor[%i, %type]);

   return %str;
}

function LBTestChatColors() {
   for (%i = 0; %i < $LBChatColors; %i ++)
      addLBChatLine(LBChatColor($LBChatColor[%i, "name"]) @ "Color" SPC %i @ ":" SPC upperFirst($LBChatColor[%i, "name"]));
}

//HiGuy: Default colors, change them if wanted
LBRegisterChatColor("normal",       "000000", "000000", "000000");
LBRegisterChatColor("mod",          "0000CC", "0000CC", "000099");
LBRegisterChatColor("admin",        "CC0000", "CC0000", "990000");
LBRegisterChatColor("whisperfrom",  "999999", "999999", "CCCCCC");
LBRegisterChatColor("whispermsg",   "804300", "804300", "FFCC33");
LBRegisterChatColor("notification", "CC9900", "CC9900", "FFEE99");
LBRegisterChatColor("welcome",      "669900", "669900", "99FF99");
LBRegisterChatColor("help",         "669900", "669900", "99FF99");
LBRegisterChatColor("lagout",       "FF0000", "FF0000", "FF6666");
LBRegisterChatColor("usage",        "999999", "999999", "CCCCCC");
LBRegisterChatColor("server",       "0000FF", "0000FF", "000099");
LBRegisterChatColor("me",           "8000FF", "8000FF", "8000FF");
LBRegisterChatColor("visible",      "009900", "009900", "66FF66");
LBRegisterChatColor("invisible",    "999999", "999999", "CCCCCC");
