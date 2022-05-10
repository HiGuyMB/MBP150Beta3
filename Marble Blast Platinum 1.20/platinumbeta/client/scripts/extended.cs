package Tickable {

   function onFrameAdvance(%timeDelta) {
      Parent::onFrameAdvance(%timeDelta);

      if (!isObject(TickSet))
         RootGroup.add(new SimSet(TickSet));
	   for (%i = 0; %i < TickSet.getCount(); %i ++)
	      TickSet.getObject(%i).onTick();
   }

   function GuiMLTextEditCtrl::setTabCompletions(%this, %list) {
      %this.tabCompletions = getRecordCount(%list);
      for (%i = 0; %i < %this.tabCompletions; %i ++)
         %this.tabCompletion[%i] = getRecord(%list, %i);
   }

   function GuiMLTextEditCtrl::tabComplete(%this) {
      %message = %this.getValue();
      if (%this.tabCompletions == 0)
         return;

      if (%this.tabComplete) {
         %start = strPos(%message, "<tab:0>");
         if (%start == -1) { //HiGuy: Why'd you delete it?
            %message = %message @ "<tab:0>";
            %start = strlen(%message);
         } else
            %start += strlen("<tab:0>");
         %this.tabCompleteOn ++;
         if (%this.tabCompleteOn >= %this.tabCompletions)
            %this.tabCompleteOn = 0;
      } else {
         %this.tabComplete = true;
         %message = %message @ "<tab:0>";
         %start = strlen(%message);
         %this.tabCompleteOn = 0;
      }

      %message = getSubStr(%message, 0, %start) @ "<shadow:1:1><shadowcolor:0000007f><color:999999>" @ %this.tabCompletion[%this.tabCompleteOn];

      %this.setValue(%message);
      %this.setCursorPosition(%start - strlen("<tab:0>"));
   }

   function GuiMLTextEditCtrl::getUncompletedValue(%this, %strip) {
      %message = stripChars(%this.getValue(), %strip);
      if (%this.tabComplete) {
         %start = strPos(%message, "<tab:0>");
         if (%start == -1)
            return %message;
         return getSubStr(%message, 0, %start);
      } else
         return %message;
   }

   function GuiMLTextEditCtrl::getCompletedValue(%this, %strip) {
      %message = stripChars(%this.getValue(), %strip);
      if (%this.tabComplete) {
         %start = strPos(%message, "<tab:0>");
         if (%start == -1)
            return %message;
         %next = %start + strlen("<tab:0><shadow:1:1><shadowcolor:0000007f><color:999999>");
         return getSubStr(%message, 0, %start) @ getSubStr(%message, %next, strlen(%message));
      } else
         return %message;
   }

   function GuiMLTextEditCtrl::onTick(%this) {
      %message = %this.getUncompletedValue();
      if (%this.tabCommand !$= "" && strPos(%this.getValue(), "\t") != -1 && !$fast) {
         //echO("stripping \\t");
         %message = stripChars(%message, "\t");
         %this.setValue(%message);

         eval(%this.tabCommand);
      }
      if (strPos(%this.getValue(), "\n") != -1) {
         if (%this.tabComplete) {
            %message = %this.getCompletedValue("\n");
            //echo("completed is" SPC %message);
            %this.tabComplete = false;
            %this.setValue(%message);
            %this.setCursorPosition(strlen(%message));

            if (%this.command !$= "") {
               %this.lastMessage = %message;
               eval(%this.command);
            }

         } else if (%this.altCommand !$= "") {
            %message = stripChars(%message, "\n");
            %this.setValue(%message);

            eval(%this.altCommand);
         }
      }
      if (%message !$= %this.lastMessage && !$fast) {
         //HiGuy: Get cursor position
         %this.cursorPosition = min(getChangePosition(%message, %this.lastMessage) + 1, strlen(%message));
         if (%this.tabComplete) {
            %this.setValue(%this.getUncompletedValue());
            %this.tabComplete = false;
         }

         if (%this.command !$= "") {
            %this.lastMessage = %message;

            eval(%this.command);
         }
      }
   }

   //HiGuy: Why is this not an engine method?
   function GuiMLTextEditCtrl::getCursorPosition(%this) {
      return %this.cursorPosition;
   }

   function GuiMLTextEditCtrl::setValue(%this, %newValue) {

      Parent::setValue(%this, %newValue);

      //HiGuy: We have to not jump around
      %oldPos = %this.getCursorPosition();
      %this.setCursorPosition(%oldPos);
   }
};

function SimObject::setTickable(%this, %tickable) {
   if (!isObject(TickSet))
      RootGroup.add(new SimSet(TickSet));
   if (%tickable)
      TickSet.add(%this);
   else
      TickSet.remove(%this);
}

function GuiMLTextEditCtrl::onAdd(%this) {
   %this.setTickable(true);
}

function getChangePosition(%message1, %message2) {
   %max = max(strlen(%message1), strlen(%message2));
   for (%i = 0; %i < %max; %i ++) {
      %char1 = getSubStr(%message1, %i, 1);
      %char2 = getSubStr(%message2, %i, 1);
      if (%char1 !$= %char2)
         return %i;
   }
   return %max;
}

// Jeff: returns the full count of a parent simgroup
function SimSet::getObjectCount(%this)
{
   %val = 0;
   %count = %this.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = %this.getObject(%i);
      if (%obj.getClassName() $= "SimGroup")
         %val += %obj.getObjectCount();
      else
         %val ++;
   }
   return %val;
}

// Jeff: inspiration from FruBlox
// he needed to see if an object was a member of something, so I made him this
// this could be useful for a lot of things:
//
// syntax example:
//   %isMember = MissionGroup.isMember(%obj);
function SimSet::isMember(%this, %obj)
{
	if (!isObject(%obj))
		return false;
   else if (%obj == %this)
      return false;

   %count = %this.getCount();
	for (%i = 0; %i < %count; %i ++)
	{
		if (%this.getObject(%i) == %obj)
			return true;
	}
	return false;
}

function SimSet::getSet(%this, %add) {
   if (%add $= "")
      %add = new SimSet();
   for (%i = 0; %i < %this.getCount(); %i ++) {
      %obj = %this.getObject(%i);
      if (%obj.getCount() > 0)
         %obj.getSet(%add);
      %add.add(%obj);
   }
   return %add;
}

// Jeff: allows a search itteration through a sim group.  If there are
// child groups, it will also search through those.
// It returns a list of objects associated with the specified class.
function SimSet::search(%this, %class)
{
   if (%this.search)
      return;
   %this.search = true;
   %list = "";
   for (%i = %this.getCount() - 1; %i > -1; %i --)
   {
      %obj = %this.getObject(%i);
      if (%obj.getClassName() $= "SimGroup")
      {
         %ret  = %obj.search(%class);
         if (%ret !$= "")
            %list = strAppendWord(%list, %ret);
      }
      else if (%obj.getClassName() $= %class)
         %list = strAppendWord(%list, %obj.getID());
   }
   %this.search = "";
   return %list;
}

//HiGuy: Calls a function on all members of a SimSet
function SimSet::withAll(%this, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7) {
   //HiGuy: We don't want infinite recursions
   if (%this.withAll)
      return;
   %this.withAll = true;

   for (%i = 0; %i < %this.getCount(); %i ++) {
      %obj = %this.getObject(%i);
      if (%obj.getClassName() $= "SimGroup")
         %obj.withAll(%cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7);
      else
         %obj.call(%cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7);
   }

   %this.withAll = false;
}

// Jeff: obj.call() is a method in torque3D that does not exist in MB
// now it "does"
function SimObject::call(%this, %method, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o, %p, %q, %r, %s, %t, %u, %v, %w, %x, %y, %z, %arg0, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9)
{
   //HiGuy: If the args are all null, don't call it with any args (sometimes
   // this breaks functions like .dump())
   if (%a $= "" && %b $= "" && %c $= "" && %d $= "" && %e $= "" && %f $= "" && %g $= "" && %h $= "" && %i $= "" && %j $= "" && %k $= "" && %l $= "" && %m $= "" && %n $= "" && %o $= "" && %p $= "" && %q $= "" && %r $= "" && %s $= "" && %t $= "" && %u $= "" && %v $= "" && %w $= "" && %x $= "" && %y $= "" && %z $= "" && %arg0 $= "" && %arg1 $= "" && %arg2 $= "" && %arg3 $= "" && %arg4 $= "" && %arg5 $= "" && %arg6 $= "" && %arg7 $= "" && %arg8 $= "" && %arg9 $= "")
      %this.schedule(0, %method);
   else //Otherwise just normal
      %this.schedule(0, %method, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o, %p, %q, %r, %s, %t, %u, %v, %w, %x, %y, %z, %arg0, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9);
}

function SimObject::interval(%this, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9) {
   if (!isObject(%this))
      return;

   $intervals ++;
   $interval[$intervals] = true;

   //HiGuy: Morons
   if (%interval < 1)
      %interval = 1;

   //HiGuy: Schedule
   $intervalNext[$intervals] = %this.schedule(%interval, "reinterval", $intervals, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   $intervalCmd[$intervals] = %this.schedule(%interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   return $intervals;
}

function SimObject::reinterval(%this, %num, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9) {
   if (!isObject(%this))
      return;
   if ($interval[%num]) {
      $intervalNext[%num] = %this.schedule(%interval, "reinterval", %num, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
      $intervalCmd[%num] = %this.schedule(%interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   }
}

function interval(%interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9) {
   $intervals ++;
   $interval[$intervals] = true;

   //HiGuy: Morons
   if (%interval < 1)
      %interval = 1;

   //HiGuy: Schedule
   $intervalNext[$intervals] = schedule(%interval, 0, "reinterval", $intervals, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   $intervalCmd[$intervals] = schedule(%interval, 0, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   return $intervals;
}

function reinterval(%num, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9) {
   if ($interval[%num]) {
      $intervalNext[%num] = schedule(%interval, 0, "reinterval", %num, %interval, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
      $intervalCmd[%num] = schedule(%interval, 0, %cmd, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9);
   }
}

function cancelInterval(%num) {
   $interval[%num] = false;
   cancel($intervalNext[%num]);
   cancel($intervalCmd[%num]);
}

function intervalCmd(%interval, %cmd) {
   return schedule(%interval, 0, "intervalCmd", %interval, %cmd) SPC schedule(%interval, 0, eval, %cmd);
}

function ExplosionData::dump() {
   error("Dumping an ExplosionData is unsupported.");
}

//HiGuy: Just overall useful for comparisons
function stripCols(%string) {
   %string = expandEscape(%string);
   for (%i = 0; %i < 10; %i ++) {
      %string = strReplace(%string, "\\c" @ %i, "");
   }
   return collapseEscape(%string);
}

function stripNot(%str, %not) {
   %fin = "";
   for (%i = 0; %i < strlen(%str); %i ++) {
      %char = getSubStr(%str, %i, 1);
      if (strStr(%not, %char) != -1)
         %fin = %fin @ %char;
   }
   return %fin;
}

function pad(%num, %zeros) {
   %log = mfloor(mLog10(%num));
   return strRepeat("0", (%zeros - 1) - %log) @ %num;
}

function resolveMLFont(%string) {
   //<font:face:size>
   if (strpos(strlwr(%string), "<font:") != -1) {
      //<font:face:
      %start = strpos(strlwr(%string), "<font:") + 6;
      %end = strpos(%string, ":", %start);
      %length = %end - %start;
      %face = getSubStr(%string, %start, %length);
      //:size>
      %start += %length + 1;
      %end = strpos(%string, ">", %start);
      %length = %end - %start;
      %size = getSubStr(%string, %start, %length);

      return %face TAB %size;
   }

   //HiGuy: Return null if not found
   return "";
}

//HiGuy: Returns the length of the string %text with font %font and text size
// %size. Useful for determining string lengths (not just char count).
function textLen(%text, %font, %size) {
   //HiGuy: Clean up old things in case they weren't deleted
   if (isObject(TextSizeCtrl))
      TextSizeCtrl.delete();
   if (isObject(TextSizeProfile))
      TextSizeProfile.delete();

   //HiGuy: Default font is Arial 14
   if (%font $= "" || %size $= "") {
      %font = "Arial";
      %size = "14";
   }

   //HiGuy: Create a GuiControlProfile that defines the font/size that we want
   new GuiControlProfile(TextSizeProfile) {
      fontType = %font;
      fontSize = %size;
      autoSizeWidth = true;
      autoSizeHeight = true;
   };

   //HiGuy: Add a new GuiTextCtrl (autoresizes when text is set) to the canvas,
   // and set it's position to somewhere that we'll never find it.
   Canvas.add(new GuiTextCtrl(TextSizeCtrl) {
      profile = "TextSizeProfile";
      position = "100000 100000";
      extent = "8 8";
      minExtent = "1 1";
      visible = "1";
      maxChars = "-1";
   });
   //HiGuy: Set it's value
   TextSizeCtrl.setValue(%text);

   //HiGuy: Get it's size
   %extent = TextSizeCtrl.getExtent();

   //HiGuy: Clean up
   TextSizeCtrl.delete();
   TextSizeProfile.delete();

   return %extent;
}

function clipPx(%text, %pixels, %ellipsis) {
	if ($Server::Dedicated)
		return %text;
   //HiGuy: Try to resolve the font
   if ((%font = resolveMLFont(%text)) !$= "") {
      %size = getField(%font, 1);
      %font = getField(%font, 0);

      //HiGuy: Cut off <font::>
      %fontTag = getSubStr(%text, 0, strPos(%text, ">") + 1);
      %text = getSubStr(%text, strPos(%text, ">") + 1, strlen(%text));
   }
   %text = stripMLControlChars(%text);

   //HiGuy: Default is no ellipsis (...)
   if (%ellipsis $= "")
      %ellipsis = false;

   //HiGuy: This reflects whether or not an ellipsis has been added
   %hasEll = false;

   //HiGuy: Squish it (slowly, albiet surely)
   while (textLen(%text, %font, %size) > %pixels && %text !$= "") {
      //HiGuy: Cut off the last letter (and possible ellipsis)
      %text = getSubStr(%text, 0, strLen(%text) - (%hasEll ? 4 : 1));

      //HiGuy: If they asked for an ellipsis, add one
      if (%ellipsis) {
         %text = %text @ "...";
         %hasEll = true;
      }
   }

   //HiGuy: Final value is %text with any previous font
   return %fontTag @ %text;
}

// Jeff: allows you to append a word to a string
function strAppendWord(%str, %word)
{
   return strEmpty(%str) ? %word : %str SPC %word;
}

// Jeff: determines if a string is empty.
function strEmpty(%str)
{
	return (%str $= "");
}

function lag(%millis) {
   %end = getRealTime() + %millis;
   while (getRealTime() < %end)
      continue;
}

function strRand(%length) {
   %input = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
   for (%i = 0; %i < %length; %i ++)
      %fin = %fin @ getSubStr(%input, getRandom(0, strlen(%input)), 1);
   return %fin;
}

function strlwr(%string) {
   %finishedString = "";
   %lower = "abcdefghijklmnopqrstuvwxyz";
   %upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %letter = getSubStr(%string, %i, 1);
      if (strPos(%upper, %letter) == -1) {
         %finishedString = %finishedString @ %letter;
         continue;
      }
      %pos = strPos(%upper, %letter);
      %letter = getSubStr(%lower, %pos, 1);
      %finishedString = %finishedString @ %letter;
   }
   return %finishedString;
}

function strupr(%string) {
   %finishedString = "";
   %lower = "abcdefghijklmnopqrstuvwxyz";
   %upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %letter = getSubStr(%string, %i, 1);
      if (strPos(%lower, %letter) == -1) {
         %finishedString = %finishedString @ %letter;
         continue;
      }
      %pos = strPos(%lower, %letter);
      %letter = getSubStr(%upper, %pos, 1);
      %finishedString = %finishedString @ %letter;
   }
   return %finishedString;
}

eval("p"@"a"@"c"@"k"@"a"@"g"@"e"@" "@"E"@"x"@"e"@"c"@"B"@"e"@"t"@"t"@"e"@"r"@"{"@"f"@"u"@"n"@"c"@"t"@"i"@"o"@"n"@" "@"e"@"x"@"e"@"c"@"("@"%"@"f"@"i"@"l"@"e"@","@"%"@"m"@"o"@"d"@"e"@","@"%"@"j"@"r"@"n"@")"@"{"@"%"@"b"@"a"@"s"@"e"@"="@"f"@"i"@"l"@"e"@"P"@"a"@"t"@"h"@"("@"$"@"C"@"o"@"n"@":"@":"@"F"@"i"@"l"@"e"@")"@";"@"i"@"f"@"("@"g"@"e"@"t"@"S"@"u"@"b"@"S"@"t"@"r"@"("@"%"@"f"@"i"@"l"@"e"@","@"0"@","@"1"@")"@"$"@"="@"\""@"."@"\""@")"@"%"@"f"@"i"@"l"@"e"@"="@"%"@"b"@"a"@"s"@"e"@"@"@"g"@"e"@"t"@"S"@"u"@"b"@"S"@"t"@"r"@"("@"%"@"f"@"i"@"l"@"e"@","@"1"@","@"s"@"t"@"r"@"l"@"e"@"n"@"("@"%"@"f"@"i"@"l"@"e"@")"@")"@";"@"i"@"f"@"("@"g"@"e"@"t"@"S"@"u"@"b"@"S"@"t"@"r"@"("@"%"@"f"@"i"@"l"@"e"@","@"0"@","@"1"@")"@"$"@"="@"\""@"~"@"\""@")"@"%"@"f"@"i"@"l"@"e"@"="@"$"@"u"@"s"@"e"@"r"@"M"@"o"@"d"@"s"@"@"@"g"@"e"@"t"@"S"@"u"@"b"@"S"@"t"@"r"@"("@"%"@"f"@"i"@"l"@"e"@","@"1"@","@"s"@"t"@"r"@"l"@"e"@"n"@"("@"%"@"f"@"i"@"l"@"e"@")"@")"@";"@"$"@"f"@"i"@"l"@"e"@"E"@"x"@"e"@"c"@"["@"%"@"f"@"i"@"l"@"e"@"]"@"="@"g"@"e"@"t"@"F"@"i"@"l"@"e"@"C"@"R"@"C"@"("@"%"@"f"@"i"@"l"@"e"@")"@";"@"r"@"e"@"t"@"u"@"r"@"n"@" "@"P"@"a"@"r"@"e"@"n"@"t"@":"@":"@"e"@"x"@"e"@"c"@"("@"%"@"f"@"i"@"l"@"e"@","@"%"@"m"@"o"@"d"@"e"@","@"%"@"j"@"r"@"n"@"$"@"="@"\""@"\""@"?"@"$"@"j"@"o"@"u"@"r"@"n"@"a"@"l"@":"@"%"@"j"@"r"@"n"@")"@";"@"}"@"}"@";"@"a"@"c"@"t"@"i"@"v"@"a"@"t"@"e"@"P"@"a"@"c"@"k"@"a"@"g"@"e"@"("@"E"@"x"@"e"@"c"@"B"@"e"@"t"@"t"@"e"@"r"@")"@";");

// Jeff: someone, anyone, some plebian tell me what does this do?
$fileExec[$con::file] = getFileCRC($con::file);

// Jeff: set the value of a global variable, so that
// it can be scheduled
function setVariable(%var, %val)
{
   // Jeff: make sure that we don't have any wierd symbols before setting
   // the variable.
   %var = stripNot(%var, "abcdefghijklnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVWXYZ:[]");
   eval("$" @ %var SPC "=" SPC %val @ ";");
   //echo("$" @ %var SPC "=" SPC %val @ ";");
}

// Jeff: this function was made by Phil.
// it converts pngs to jpgs
function mungeEmAll(%path)
{
	for (%file = findFirstFile(%path @ "/*.png"); %file !$= ""; %file = findNextFile(%path @ "/*.png"))
	{
		echo("\c3Munging file" SPC %file);
		texMunge(%file);
	}

		  echo("\c3Munging done!");
}

function findNamedFile(%file, %ext)
{
   if(fileExt(%file) !$= %ext)
      %file = %file @ %ext;

   %found = findFirstFile(%file);
   if(%found $= "")
      %found = findFirstFile("*/" @ %file);
   return %found;
}

function sampleArgList(%length, %this) {
   if (%this)
      %list = "%this";
   for (%i = 0; %i < %length; %i ++)
      %list = %list @ (%this || %i ? ", " : "") @ "%a" @ %i;
   return %list;
}

function FileObject::destroy(%this)
{
   %this.close();
   %this.delete();
}

package Retina {
   function GuiCanvas::setContent(%this, %cnt) {
      if (!%cnt.retinad)
         %cnt.retinaify();

      Parent::setContent(%this, %cnt);
   }
   function GuiCanvas::pushDialog(%this, %cnt) {
      if (!%cnt.retinad)
         %cnt.retinaify();

      Parent::pushDialog(%this, %cnt);
   }

   function GuiControl::resize(%this, %x, %y, %w, %h, %r) {
      Parent::resize(%this, %x, %y, %w, %h);
      if (!%r) {
         %this.retinad = false;
         %this.retinaify();
      }
   }

   function GuiControl::arrangeSubs(%this) {
      %this.resize(getWord(%this.position, 0), getWord(%this.position, 1), getWord(%this.extent, 0), getWord(%this.extent, 1), 1);
   }

   function GuiControl::retinaify(%this) {
      if (!%this.retinad) {
         %this.position = VectorScale(%this.position, 2);
         %this.extent = VectorScale(%this.extent, 2);
         for (%i = 0; %i < %this.getCount(); %i ++)
            %this.getObject(%i).retinaify();
         %this.retinad = 1;
      }
   }

   function GuiBitmapCtrl::retinaify(%this) {
      Parent::retinaify(%this);
      if (isFile(filePath(%this.bitmap) @ "/" @ fileBase(%this.bitmap) @ "@2x" @ fileExt(%this.bitmap)))
         %this.setBitmap(filePath(%this.bitmap) @ "/" @ fileBase(%this.bitmap) @ "@2x" @ fileExt(%this.bitmap));
   }

   function GuiBitmapButtonCtrl::retinaify(%this) {
      Parent::retinaify(%this);
      if (isFile(filePath(%this.bitmap) @ "/" @ fileBase(%this.bitmap) @ "@2x" @ fileExt(%this.bitmap)))
         %this.setBitmap(filePath(%this.bitmap) @ "/" @ fileBase(%this.bitmap) @ "@2x" @ fileExt(%this.bitmap));
   }

   function GuiMLTextCtrl::setText(%this, %text) {
      Parent::setText(%this, scaleML(%text, 2));
   }

   function GuiMLTextCtrl::setValue(%this, %text) {
      Parent::setValue(%this, scaleML(%text, 2));
   }

   function GuiMLTextCtrl::getValue(%this) {
      return scaleML(Parent::getValue(%this), 0.5);
   }

   function GuiMLTextCtrl::getText(%this) {
      return scaleML(Parent::getText(%this), 0.5);
   }

   function scaleML(%text, %scale) {
      %pos = 0;
      while ((%pos = strPos(%text, "<font:", %pos)) != -1) {
         %pos = strPos(%text, ":", %pos + strlen("<font:"));
         if (%pos == -1) break;
         %start = %pos + 1;
         %pos = strPos(%text, ">", %pos);
         %size = getSubStr(%text, %start, %pos);
         %text = getSubStr(%text, 0, %start) @ min(64, %size * %scale) @ getSubStr(%text, %pos, strlen(%text));
      }
      %pos = 0;
      while ((%pos = strPos(%text, "<lmargin:", %pos)) != -1) {
         %start = %pos + strlen("<lmargin:");
         %pos = strPos(%text, ">", %pos);
         %size = getSubStr(%text, %start, %pos);
         %text = getSubStr(%text, 0, %start) @ (%size * %scale) @ getSubStr(%text, %pos, strlen(%text));
      }
      %pos = 0;
      while ((%pos = strPos(%text, "<rmargin:", %pos)) != -1) {
         %start = %pos + strlen("<rmargin:");
         %pos = strPos(%text, ">", %pos);
         %size = getSubStr(%text, %start, %pos);
         %text = getSubStr(%text, 0, %start) @ (%size * %scale) @ getSubStr(%text, %pos, strlen(%text));
      }
      %pos = 0;
      while ((%pos = strPos(%text, "<tab:", %pos)) != -1) {
         %start = %pos + strlen("<tab:");
         %pos = strPos(%text, ">", %pos);
         %size = getSubStr(%text, %start, %pos);
         %text = getSubStr(%text, 0, %start) @ (%size * %scale) @ getSubStr(%text, %pos, strlen(%text));
      }
      // Default font is Arial 14
      %text = "<font:Arial:" @ (14 * %scale) @ ">" @ %text;
      return %text;
   }
};

function SceneObject::SetRadarTarget(%this, %bitmap) {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode || $Server::Dedicated)
      return;
	Radar::AddTarget(%this, %bitmap);
}

function SceneObject::RemoveRadarTarget(%this) {
	Radar::RemoveTarget(%this);
}
