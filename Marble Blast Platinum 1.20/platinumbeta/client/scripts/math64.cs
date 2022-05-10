//--------------------------------------------------------------------------
// HiGuy: I fucking hate how torque decides to use scientific notation
//        for numbers > 99999. It ruins precision and makes large math
//        impossible. Therefore, I'm making this method, to share.
//
//        Update 6/13/12: Added support for negatives by combining add64 and
//        sub64 together. Don't define %force from a call though.
//
//        Update 10/15/12:
//        Jeff redid some of the coding by making it more efficient
//
//        Update 4/21/13:
//        Jeff: Added Dividing and exponents (power)
//
//        This is used for $LB::ChatLast which is overflowing and causing
//        problems with chat receiving
//
// sub64 (a, b)
//
//   a
// - b
// ------
//   c
//
//  a1 a2 a3 a4 a5 a6 a7 a8 a9 a0
//  -  b1 b2 b3 b4 b5 b6 b7 b8 b9
// ---------------------------------
//  c1 c2 c3 c4 c5 c6 c7 c8 c9 c0
//--------------------------------------------------------------------------

function add64(%a, %b, %force) {
   return add(%a,%b,%force);
}

function sub64(%a, %b, %force) {
   return sub(%a,%b,%force);
}

function power64(%a, %b)
{
   return power(%a, %b);
}

function div64(%a, %b)
{
	return div(%a, %b);
}

function abs64(%a) {
   return abs(%a);
}

function abs(%a)
{
   // Jeff: strip negative
   if (getSubStr(%a,0,1) $= "-")
      return getSubStr(%a, 1, strLen(%a));
   return %a;
}

function add(%a,%b,%force) {
   // Jeff: check for negatives by HiGuy
   if (!%force) {
      %aneg = getSubStr(%a, 0, 1) $= "-";
      %bneg = getSubStr(%b, 0, 1) $= "-";
      if (%aneg && !%bneg)
         return sub(%b, getSubStr(%a, 1, strlen(%a)), true);
      if (%aneg && %bneg)
         return "-" @ add(getSubStr(%a, 1, strlen(%a)), getSubStr(%b, 1, strlen(%b)), true);
      if (!%aneg && %bneg)
         return sub(%a, getSubStr(%b, 1, strlen(%b)), true);
   }

   //HiGuy: Decimals
   %aDec = strPos(%a, ".");
   if (%aDec != -1) {
      %a = getSubStr(%a, 0, %aDec) @ getSubStr(%a, %aDec + 1, strLen(%a));
      %aDec = strLen(%a) - %aDec;
   }

   %bDec = strPos(%b, ".");
   if (%bDec != -1) {
      %b = getSubStr(%b, 0, %bDec) @ getSubStr(%b, %bDec + 1, strLen(%b));
      %bDec = strLen(%b) - %bDec;
   }

   if (%aDec == -1) %aDec = 0;
   if (%bDec == -1) %bDec = 0;

   while (%aDec > %bDec) {
      %b = %b @ "0";
      %bDec ++;
   }
   while (%bDec > %aDec) {
      %a = %a @ "0";
      %aDec ++;
   }

   while (strLen(%a) < strLen(%b))
      %a = "0" @ %a;
   while (strLen(%b) < strLen(%a))
      %b = "0" @ %b;
   %length = strLen(%a) - 1;

   %remainder = 0;
   for (%i = %length; %i >= 0; %i--)
      %sum[%i] = getSubStr(%a,%i,1) + getSubStr(%b,%i,1);
   for (%i = %length; %i >= 0; %i --) {
      %sum[%i] += %remainder;
      %remainder = 0;
      while (%sum[%i] > 9) {
         %sum[%i] -= 10;
         %remainder ++;
      }
      %value = %sum[%i] @ %value;
   }
   if (%remainder)
      %value = %remainder @ %value;

   //HiGuy: Decimals
   if (%aDec != 0)
      %value = getSubStr(%value, 0, strlen(%value) - %aDec) @ "." @ getSubStr(%value, strlen(%value) - %aDec, strlen(%value));

   if (getSubStr(%value, 0, 1) $= ".")
      %value = "0" @ %value;

   return %value;
}

function sub(%a1,%b1,%force) {
   %a = %a1;
   %b = %b1;
   // Jeff: negative number check, by HiGuy
   if (!%force) {
      %aneg = getSubStr(%a, 0, 1) $= "-";
      %bneg = getSubStr(%b, 0, 1) $= "-";
      if (%aneg && !%bneg)
         return "-" @ add(getSubStr(%a, 1, strlen(%a)), %b, true);
      if (%aneg && %bneg)
         return sub(getSubStr(%b, 1, strlen(%b)), getSubStr(%a, 1, strlen(%a)), true);
      if (!%aneg && %bneg)
         return add(%a, getSubStr(%b, 1, strlen(%b)), true);
   }

   //HiGuy: Decimals
   %aDec = strPos(%a, ".");
   if (%aDec != -1) {
      %a = getSubStr(%a, 0, %aDec) @ getSubStr(%a, %aDec + 1, strLen(%a));
      %aDec = strLen(%a) - %aDec;
   }

   %bDec = strPos(%b, ".");
   if (%bDec != -1) {
      %b = getSubStr(%b, 0, %bDec) @ getSubStr(%b, %bDec + 1, strLen(%b));
      %bDec = strLen(%b) - %bDec;
   }

   if (%aDec == -1) %aDec = 0;
   if (%bDec == -1) %bDec = 0;

   while (%aDec > %bDec) {
      %b = %b @ "0";
      %bDec ++;
   }
   while (%bDec > %aDec) {
      %a = %a @ "0";
      %aDec ++;
   }

   while (strLen(%a) < strLen(%b))
      %a = "0" @ %a;
   while (strLen(%b) < strLen(%a))
      %b = "0" @ %b;
   %length = strLen(%a) - 1;

   // Jeff: size check
   for (%i = 0; %i <= %length; %i ++) {
      %subA = getSubStr(%a,%i,1);
      %subB = getSubStr(%b,%i,1);
      if (%subA > %subB)
         break;
      else if (%subA == %subB)
         continue;
      return "-" @ sub(%b1,%a1);
   }

   // Jeff: convert to array
   for (%i = %length; %i >= 0; %i--) {
      %letA[%i] = getSubStr(%a,%i,1);
      %letB[%i] = getSubStr(%b,%i,1);
   }

   // Jeff: subtraction time
   for (%i = %length; %i >= 0; %i --) {
      %diff = %letA[%i] - %letB[%i];
      if (%diff < 0) {
         %nextPlace = %i - 1;
         %letA[%nextPlace] --;
         %letA[%i] += 10;
         %diff = %letA[%i] - %letB[%i];
      }
      %value = %diff @ %value;
   }

   while (getSubStr(%value,0,1) $= "0") // Jeff: strip excess zeros
      %value = getSubStr(%value, 1, strLen(%value));

   if (%value $= "")
      return 0;

   //HiGuy: Then it has zeros
   %value = strrepeat("0", %aDec - strlen(%value)) @ %value;

   //HiGuy: Decimals
   if (%aDec != 0)
      %value = getSubStr(%value, 0, strlen(%value) - %aDec) @ "." @ getSubStr(%value, strlen(%value) - %aDec, strlen(%value));

   if (getSubStr(%value, 0, 1) $= ".")
      %value = "0" @ %value;

   return %value;
}

//For jeff :3
function mod64(%a, %b) {
   %strip = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~`!@#$%^&*()=_+|\\\"\':;,<>/? ";
   %a = stripChars(%a, %strip);

   if (%a $= "" || %a $= "0")
      return %a;
   if (%b $= "" || %b $= "0")
      return "NaN"; //1%0 = CRASH (Don't do it!)

   %isNeg = getSubStr(%a, 0, 1) $= "-";
   if (%isNeg)
      %a = getSubStr(%a, 1, strlen(%a));

   if (getSubStr(%b, 0, 1) $= "-")
      %b = getSubStr(%b, 1, strlen(%b));

   //HiGuy: I've heard so many instances of how teachers badly teach the concept
   // of decimals, and that they could be fun if done right. Oh well. Screw them.
   %a = floor64(%a);
   %b = floor64(%b);

   if (strlen(%a) < strlen(%b))
      return %a;

   %sub = sub64(%a, %b);
   while (getSubStr(%sub, 0, 1) !$= "-") {
      %a = %sub;
      for (%on = strlen(%sub) - strlen(%b); %on > 2; %on --) {
         %sub = sub64(%sub, %b @ strRepeat("0", %on));
         while (getSubStr(%sub, 0, 1) $= "-") {
            %sub = add64(%sub, %b @ strRepeat("0", %on - 1));
         }
      }
      //echo("%on was" SPC %on);
      %sub = sub64(%sub, %b);
   }
   return %a;
}

function floor64(%a) {
   %strip = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ~`!@#$%^&*()=_+|\\\"\':;,<>/? ";
   %a = stripChars(%a, %strip);

   if (%a $= "" || %a $= "0")
      return %a;

   if (strPos(%a, ".") == -1)
      return %a;

   %a = getSubStr(%a, 0, strpos(%a, "."));
   return %a;
}

function mult(%a,%b) {
   %out = 0;

   %aM1 = strlen(%a) - 1; //Because jeff says so
   %bM1 = strlen(%b) - 1;

   for (%i = 0; %i < strlen(%a); %i ++) {
      %iDig = getSubStr(%a, %i, 1);
      if (%iDig $= "0") continue;

      for (%j = 0; %j < strlen(%b); %j ++) {
         %jDig = getSubStr(%b, %j, 1);
         if (%jDig $= "0") continue;

         %multed = %iDig * %jDig;

         %max = (%aM1 - %i) + (%bM1 - %j);
         for (%k = 0; %k < %max; %k ++)
            %multed = %multed @ "0";

         %out = add64(%out, %multed);
      }
   }

   return %out;
}

// Jeff: it divides b into a.
// TODO: DECIMALS
function div(%a, %b)
{
   // Jeff: trying to divide by zero eh?
   // or we are just zero
   if (%b == 0 || %a == 0)
      return 0;

   // Jeff: if we are dividing by 1, why would we even...
   if (%b == 1)
      return %a;

   // Jeff: determine negative
   %negative = false;
   if (getSubStr(%a, 0, 1) $= "-")
   {
      %negative = true;
      %a = getSubStr(%a, 1, strlen(%a));
   }
   if (getSubStr(%b, 0, 1) $= "-")
   {
      %negative = !%negative;
      %b = getSubStr(%b, 1, strlen(%b));
   }

   // Jeff: if a < b why would we even...
   if (strlen(%a) < strlen(%b))
      return 0;
   if (%a < %b)
      return 0;

   %out = 0;
   %len = strlen(%a);
   %remainder = 0;
   %keep = false;
   for (%i = 0; %i < %len; %i ++)
   {
      %num  = getSubStr(%a, %i, 1);
      %dig  = %keep ? %dig + %num : %num;
      %dig += %remainder;
      %remainder = 0;
      %keep = false;

      //echo("%DIG" SPC %dig);

      // is the divisor too big?
      if (%b > %dig)
      {
         // Jeff: we may have a remainder if it can't continue
         if (%i == (%len - 1))
         {
            echo("We have a remainder.");
            %remainder = %dig;
            break;
         }
         %remainder = 0;
         %keep = true;
         continue;
      }

      // Jeff: %c is temporary so that %b isn't overridden
      %c = %b;

      // Jeff: lets see how many times it goes in!
      %count = 0;
      while (%c >= %dig && %c > 0)
      {
         %c -= %dig;
         %count ++;
      }
      %remainder = %c > 0 ? %c : 0;

      // Jeff: calculate place value
      %place = %count @ ((%len - (%i + 1)) - strlen(%count));

      // Jeff: strip zeros, unless it has to be a zero
      %placeLen = strlen(%place) + 1;
      while (getSubStr(%place, 0, 1) $= "0" && (%placeLen --) > 1)
         %place = getSubStr(%place, 1, %placeLen);

      //echo("STEP" SPC %i SPC "PLACE" SPC %place);

      // Jeff: if we have scientific notation, then we must add using
      // the slow method.  Long addition.
      if (strstr((%out + %place), "e") == -1)
         %out += %place;
      else
         %out = add(%out, %place);
   }

   // Jeff: if we have a remainder, IT'S DECIMAL TIME
   if (%remainder)
   {
      // Jeff: im just gonna start crying now
      //echo("Remainder:" SPC %remainder);
   }

   if (%negative)
      return "-" @ %out;
   return %out;
}

function power(%a, %b)
{
   // Jeff: should always be 1 less since we start out at a^1
   while ((%b --) > 0)
      %a = mult(%a, %a);
   return %a;
}

//-----------------------------------------------------------------------------
// Jeff: Binary Calculations
//
// http://www.wikihow.com/Convert-from-Decimal-to-Binary
//-----------------------------------------------------------------------------

// Jeff: converts a number from decimal to bin
// number range of 0 - 255 (1 byte)
// stressed dec2Bin(45); it took approx 0.0261ms to execute
function dec2Bin(%int)
{
   if (!isObject(Base2LookupTable))
   {
      new ScriptObject(Base2LookupTable);
      for (%i = 0; %i < 8; %i ++)
         Base2LookupTable.lookup[%i] = mPow(2, %i);
      RootGroup.add(Base2LookupTable);
   }

   if (%int <= 0)
      return 0;

   %bin = "";
   for (%i = 0; %i < 8; %i ++)
       %bin  = (!!(%int & Base2LookupTable.lookup[%i])) @ %bin;
   return %bin;
}

// Jeff: convert a binary sequence into a decimal number
// only works with up to 8 digits (1 byte)
// number range of 0 - 255
// stressed bin2dec("100001"); it took approx 0.0192ms to execute
function bin2Dec(%bin)
{
   if (!isObject(Base2LookupTable))
   {
      new ScriptObject(Base2LookupTable);
      for (%i = 0; %i < 8; %i ++)
         Base2LookupTable.lookup[%i] = mPow(2, %i);
      RootGroup.add(Base2LookupTable);
   }

   %int = 0;
   for (%i = strlen(%bin) - 1; %i > -1; %i --)
   {
      if (getSubStr(%bin, %i, 1))
         %int += Base2LookupTable.lookup[%i];
   }
   return %int;
}
