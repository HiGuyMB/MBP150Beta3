//-----------------------------------------------------------------------------
// arrayObject.cs
// Copyright (c) The Platinum Team
// Portions Copyright (c) GarageGames and TGE
//-----------------------------------------------------------------------------


// Jeff: make the array group
if (!isObject(ArrayGroup)) {
   new SimGroup(ArrayGroup);
   RootGroup.add(ArrayGroup);
}

// Jeff: create a new array
function Array(%name, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg0, %arga, %argb, %argc, %argd, %arge, %argf, %argg, %argh, %argi, %argj, %argk, %argl, %argm, %argn, %argo, %argp, %argq, %argr, %args, %argt, %argu, %argv, %argw, %argx, %argy, %argz, %arg00, %arg01, %arg02, %arg03, %arg04, %arg05, %arg06, %arg07, %arg08, %arg09, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15, %arg16, %arg17, %arg18, %arg19, %arg20) {
   %array = new ScriptObject(%name) {
      class = "Array";
      size = 0;
   };
   ArrayGroup.add(%array);

   //HiGuy: You can now specify up to 56 args for the array to have
   //HiGuy: NEW CHANGE: Now you can do 56! YAY!
   %array.addArgs(%arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg0, %arga, %argb, %argc, %argd, %arge, %argf, %argg, %argh, %argi, %argj, %argk, %argl, %argm, %argn, %argo, %argp, %argq, %argr, %args, %argt, %argu, %argv, %argw, %argx, %argy, %argz, %arg00, %arg01, %arg02, %arg03, %arg04, %arg05, %arg06, %arg07, %arg08, %arg09, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15, %arg16, %arg17, %arg18, %arg19, %arg20);
   return %array;
}

//HiGuy: Duplicate the array
function Array::duplicate(%this) {
   %new = Array(%this.getName());
   for (%i = 0; %i < %this.size; %i ++)
      %new.addEntry(%this.val[%i]);
   return %new;
}

// Jeff: fill an array all at once
function Array::fillArray(%this, %list) {
   %this.clear();
   %size = getFieldCount(%list);
   for (%i = 0; %i < %size; %i ++)
      %this.val[%i] = getField(%list, %i);
   %this.size = %size;
   return %this;
}

// Jeff: add an entry to the array object
function Array::addEntry(%this, %entry) {
   %this.val[%this.size] = %entry;
   %this.size ++;
   return %this;
}

// Jeff: replaces the value of an entry by index
function Array::replaceEntryByIndex(%this, %index, %entry) {
   %this.val[%index] = %entry;
   if (%index > %this.size)
      %this.size = %index + 1;
   return %this;
}

//HiGuy: In case you want to insert an entry before
function Array::insertEntryBefore(%this, %entry, %before) {
   if (%before > %this.size || %before < 0) {
      error("Array::insertEntryBefore() Index" SPC %before SPC "out of bounds [0 .." SPC %this.size @ "]");
      return;
   }
   for (%i = %this.size; %i > %before; %i --)
      %this.val[%i] = %this.val[%i - 1];
   %this.val[%before] = %entry;
   %this.size ++;
   return %this;
}

//HiGuy: More fun for inserting
function Array::insertEntryAfter(%this, %entry, %after) {
   if (%this.size >= %after || %after < 0) {
      error("Array::insertEntryAfter() Index" SPC %after SPC "out of bounds [0 .." SPC %this.size - 1 @ "]");
      return;
   }
   for (%i = %this.size; %i > %after + 1; %i --)
      %this.val[%i] = %this.val[%i - 1];
   %this.val[%after + 1] = %entry;
   %this.size ++;
   return %this;
}

// Jeff: remove an entry from the array object by specifing the index
function Array::removeEntryByIndex(%this, %index) {
   %this.size --;
   for (%i = %index; %i < %this.size; %i ++)
      %this.val[%i] = %this.val[%i + 1];
   %this.val[%this.size] = "";
   return %this;
}

//HiGuy: My brain wants this to be shorter
function Array::removeEntry(%this, %index) {
   %this.removeEntryByIndex(%index);
   return %this;
}

//HiGuy: Removes all entries that are the same as %conts
function Array::removeEntriesByContents(%this, %conts) {
   for (%i = 0; %i < %this.size; %i ++) {
      %entry = %this.getEntry(%i);
      if (%entry $= %conts) {
         %this.removeEntryByIndex(%i);
         %i --;
      }
   }
   return %this;
}

//HiGuy: Shorthand
function Array::removeMatching(%this, %conts) {
   %this.removeEntriesByContents(%conts);
   return %this;
}

// Jeff: get a value from the specified index of the array
function Array::getEntryByIndex(%this, %index) {
   return %this.val[%index];
}

//HiGuy: Shorthand
function Array::getEntry(%this, %index) {
   return %this.getEntryByIndex(%index);
}

//HiGuy: Get the index of the first object with %value at field %field
function Array::getIndexByField(%this, %value, %field) {
   for (%i = 0; %i < %this.size; %i ++) {
      if (getField(%this.val[%i], %field) $= %value)
         return %i;
   }
   return -1;
}

//HiGuy: Get the index of the first object with %value at record %record
function Array::getIndexByRecord(%this, %value, %record) {
   for (%i = 0; %i < %this.size; %i ++) {
      if (getRecord(%this.val[%i], %record) $= %value)
         return %i;
   }
   return -1;
}

//HiGuy: Get the first object with %value at field %field
function Array::getEntryByField(%this, %value, %field) {
   for (%i = 0; %i < %this.size; %i ++) {
      if (getField(%this.val[%i], %field) $= %value)
         return %this.val[%i];
   }
   return "";
}

//HiGuy: Get the first object with %value at record %record
function Array::getEntryByRecord(%this, %value, %record) {
   for (%i = 0; %i < %this.size; %i ++) {
      if (getRecord(%this.val[%i], %record) $= %value)
         return %this.val[%i];
   }
   return "";
}

// Jeff: get the index of the specified entry
function Array::getIndexByEntry(%this, %value) {
   for (%i = 0; %i < %this.size; %i ++) {
      if (%this.val[%i] $= %value)
         return %i;
   }
   return -1;
}

//HiGuy: Shorthand again!
function Array::getIndex(%this, %value) {
   return %this.getIndexByEntry(%value);
}

// Jeff: replace an index with null
function Array::replaceEntryByIndexWithNull(%this, %index) {
   %this.val[%index] = "";
   return %this;
}

//HiGuy: Wow, that is a long name
function Array::nullifyEntry(%this, %index) {
   %this.replaceEntryByIndexWithNull(%index);
   return %this;
}

//HiGuy: Returns true if the array contains an entry equal to "entry"
function Array::containsEntry(%this, %entry) {
   for (%i = 0; %i < %this.size; %i ++) {
      %ientry = %this.getEntry(%i);
      if (%ientry $= %entry)
         return true;
   }
   return false;
}

//HiGuy: Shorthand
function Array::contains(%this, %entry) {
   return %this.containsEntry(%entry);
}

function Array::containsEntryAtField(%this, %entry, %field) {
   for (%i = 0; %i < %this.size; %i ++) {
      %ientry = getField(%this.getEntry(%i), %field);
      if (%ientry $= %entry)
         return true;
   }
   return false;
}

//HiGuy: I like shortness
function Array::containsField(%this, %entry, %field) {
   return %this.containsEntryAtField(%entry, %field);
}

function Array::containsEntryAtRecord(%this, %entry, %record) {
   for (%i = 0; %i < %this.size; %i ++) {
      %ientry = getRecord(%this.getEntry(%i), %record);
      if (%ientry $= %entry)
         return true;
   }
   return false;
}

//HiGuy: I like shortness
function Array::containsRecord(%this, %entry, %record) {
   return %this.containsEntryAtRecord(%entry, %record);
}

// Jeff: get all the values of the array in a TAB based list
function Array::listValues(%this) {
   %list = %this.val[0];
   for (%i = 1; %i < %this.size; %i ++)
      %list = %list TAB %this.val[%i];
   return %list;
}

//HiGuy: Print all the values of the array to console
function Array::dumpValues(%this, %indent) {
   //echo(strRepeat("   ", %indent) @ "{");

   //HiGuy: Format and print, don't even try to understand :)
   for (%i = 0; %i < %this.size; %i ++) {
      %print = strRepeat("   ", %indent);
      %print = %print @ "   ";
      %print = %print @ strRepeat(" ", mFloor(mLog10(%this.size)) - mFloor(mLog10(%i)));
      %print = %print @ "[" @ %i @ "]";
      //%print = %print SPC (%this.val[%i] * 1 $= %this.val[%i] ? (isObject(%this.val[%i]) ? "<" : "") : "\"") @ %this.val[%i] @ (%this.val[%i] * 1 $= %this.val[%i] ? (isObject(%this.val[%i]) ? ">" : "") : "\"") @ (%i == %this.size - 1 ? "" : ","));
      //echo(%print);
   }
   //echo(strRepeat("   ", %indent) @ "}");
}

// Jeff: clear array
function Array::clear(%this) {
   for (%i = 0; %i < %this.size; %i ++)
      %this.val[%i] = "";
   %this.size = 0;
   return %this;
}

// Jeff: get size of the array
function Array::getSize(%this) {
   return %this.size;
}

// Jeff: sort an array from lowest to highest values
function Array::sortLowToHigh(%this, %field) {
   %fake = Array();
   while (%this.size) {
      %fake.addEntry(%this.val[0]);
      %this.removeEntryByIndex(0);
   }

   while (%fake.size) {
      %val = getField(%fake.val[0], %field);
      %full = %fake.val[0];
      %pos = 0;
      for (%i = 1; %i < %fake.size; %i ++) {
         if (%val >= %fake.val[%i]) {
            %val = getField(%fake.val[%i], %field);
            %full = %fake.val[%i];
            %pos = %i;
         }
      }
      %this.addEntry(%full);
      %fake.removeEntryByIndex(%pos);
   }
   %fake.delete();
   return %this;
}

// Jeff: sort an array from highest to lowest values
function Array::sortHighToLow(%this, %field) {
   %fake = Array();
   while (%this.size) {
      %fake.addEntry(%this.val[0]);
      %this.removeEntryByIndex(0);
   }

   while (%fake.size) {
      %val = getField(%fake.val[0], %field);
      %full = %fake.val[0];
      %pos = 0;
      for (%i = 1; %i < %fake.size; %i ++) {
         if (%val <= %fake.val[%i]) {
            %val = getField(%fake.val[%i], %field);
            %full = %fake.val[%i];
            %pos = %i;
         }
      }
      %this.addEntry(%full);
      %fake.removeEntryByIndex(%pos);
   }
   %fake.delete();
   return %this;
}

//HiGuy: Sort the array, method is {0: high-to-low, 1: low-to-high}
function Array::sort(%this, %method, %field) {
   switch (%method) {
   case 0:
      %this.sortHighToLow(%field);
   case 1:
      %this.sortLowToHigh(%field);
   default:
      error("Array::sort() Unknown method!");
   }
   return %this;
}

//HiGuy: Duplicate + sort
function Array::sortedArray(%this, %method) {
   %new = %this.duplicate();
   %new.sort(%method);
   return %new;
}

//HiGuy: For single-value arrays
function Array::removeDuplicates(%this) {
   for (%i = 0; %i < %this.size; %i ++) {
      for (%j = %i + 1; %j < %this.size; %j ++) {
         if (%this.val[%i] $= %this.val[%j]) {
            %this.removeEntryByIndex(%j);
            %j --;
         }
      }
   }
   return %this;
}

//HiGuy: Duplicate + remove
function Array::removedDuplicatesArray(%this) {
   %new = %this.duplicate();
   %new.removeDuplicates();
   return %new;
}

//HiGuy: Stick and stuff
function Array::addArgs(%this, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg0, %arga, %argb, %argc, %argd, %arge, %argf, %argg, %argh, %argi, %argj, %argk, %argl, %argm, %argn, %argo, %argp, %argq, %argr, %args, %argt, %argu, %argv, %argw, %argx, %argy, %argz, %arg00, %arg01, %arg02, %arg03, %arg04, %arg05, %arg06, %arg07, %arg08, %arg09, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15, %arg16, %arg17, %arg18, %arg19, %arg20) {
   //HiGuy: If any arg is non-blank, add all args before it
   %go = false;

   //HiGuy: 56 lines of code on the wall (of text), 56 lines of code! Exec one down, move the curLine one down, 55 lines of code on the wall (of text)!
   if (%arg20 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg20, 0);}
   if (%arg19 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg19, 0);}
   if (%arg18 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg18, 0);}
   if (%arg17 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg17, 0);}
   if (%arg16 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg16, 0);}
   if (%arg15 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg15, 0);}
   if (%arg14 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg14, 0);}
   if (%arg13 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg13, 0);}
   if (%arg12 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg12, 0);}
   if (%arg11 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg11, 0);}
   if (%arg10 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg10, 0);}
   if (%arg09 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg09, 0);}
   if (%arg08 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg08, 0);}
   if (%arg07 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg07, 0);}
   if (%arg06 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg06, 0);}
   if (%arg05 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg05, 0);}
   if (%arg04 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg04, 0);}
   if (%arg03 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg03, 0);}
   if (%arg02 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg02, 0);}
   if (%arg01 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg01, 0);}
   if (%arg00 !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg00, 0);}
   if (%argz  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argz,  0);}
   if (%argy  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argy,  0);}
   if (%argx  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argx,  0);}
   if (%argw  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argw,  0);}
   if (%argv  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argv,  0);}
   if (%argu  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argu,  0);}
   if (%argt  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argt,  0);}
   if (%args  !$= "" || %go) {%go = true; %this.insertEntryBefore(%args,  0);}
   if (%argr  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argr,  0);}
   if (%argq  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argq,  0);}
   if (%argp  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argp,  0);}
   if (%argo  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argo,  0);}
   if (%argn  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argn,  0);}
   if (%argm  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argm,  0);}
   if (%argl  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argl,  0);}
   if (%argk  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argk,  0);}
   if (%argj  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argj,  0);}
   if (%argi  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argi,  0);}
   if (%argh  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argh,  0);}
   if (%argg  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argg,  0);}
   if (%argf  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argf,  0);}
   if (%arge  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arge,  0);}
   if (%argd  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argd,  0);}
   if (%argc  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argc,  0);}
   if (%argb  !$= "" || %go) {%go = true; %this.insertEntryBefore(%argb,  0);}
   if (%arga  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arga,  0);}
   if (%arg0  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg0,  0);}
   if (%arg9  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg9,  0);}
   if (%arg8  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg8,  0);}
   if (%arg7  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg7,  0);}
   if (%arg6  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg6,  0);}
   if (%arg5  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg5,  0);}
   if (%arg4  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg4,  0);}
   if (%arg3  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg3,  0);}
   if (%arg2  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg2,  0);}
   if (%arg1  !$= "" || %go) {%go = true; %this.insertEntryBefore(%arg1,  0);}

   return %this;
}

function strRepeat(%str, %times) {
   %fin = "";
   for (%i = 0; %i < %times; %i ++)
      %fin = %fin @ %str;
   return %fin;
}
