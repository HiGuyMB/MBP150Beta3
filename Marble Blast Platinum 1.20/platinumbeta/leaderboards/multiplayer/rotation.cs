//------------------------------------------------------------------------------
// Multiplayer Package
// rotation.cs
// Copyright (c) 2013 MBP Team
//
// Jeff: special credit is given to Zot because without him,
// this would have looked like crap.  His awesome library.cs for Torque Game
// Engine 1.3 and under is pure win.  A big thank you!
//
// Jeff: If you understand this code, you sir are a genious!  Because I
// programmed it and it makes about .....zilch sense to me.
//------------------------------------------------------------------------------

$tau    = 6.28319;
$maxRot = 4.18879;
$minRot = -1.5708;

// Jeff: assist the rotation matrix to make smoother rotation
// on objects that need to be updated (ghosting for example in multiplayer)
function GameConnection::assistRotation(%this) {
   if (!isObject(%this.player) || $Server::ServerType $= "SinglePlayer")
      return;
   // Jeff: if we didn't move, just stop
   %position = roundPosVec(%this.player.getPosition(), %this.speed, %this.player.megaMarble);
   if (%position $= %this.lastPosVecPos)
      return;

   %xAxis = getWord(%this.rotUpdate, 0);
   %yAxis = getWord(%this.rotUpdate, 1);
   %zAxis = getWord(%this.rotUpdate, 2);
   %aAxis = getWord(%this.rotUpdate, 3);

   // Jeff: used for negative stuff
   %negative = %this.negative;

   // Jeff: initialize variables
   // according to Zot in library.cs .....
   // the Z axis and angle can be added together to form 1 angle
   // so doing so...
   //
   // basically im trying to smoothen out the intervals
   %angle = 0;
   %z = mAtan(%xAxis, %yAxis); // Jeff: matan :3
   if (%negative) {
      %modifier = (%z > 0) ? %z * -1 : %z;
      %angle = (%zAxis + (%zAxis + %modifier)) * %aAxis;
   } else {
      %modifier = (%z < 0) ? %z * -1 : %z;
      %angle = (%zAxis - (%zAxis - %modifier)) * %aAxis;
   }

   %rotation = %xAxis SPC %yAxis SPC %angle;
   %direction = %this.getDirection();

   // Jeff: take the cross product of the current
   // rotation vector and the direction the marble
   // is traveling in, then add it to the current
   // matrix vector, followed by normalization to get
   // it down to the unit vector on the unit circle
   //
   // depending on negative, add or subtract
   // this helps to smooth out the "jerkiness" even though
   // its not 100%
   %vector = VectorCross(%rotation, %direction);
   if (%negative)
      %vector = VectorSub(%rotation, %vector);
   else
      %vector = VectorAdd(%rotation, %vector);
   %vector = VectorNormalize(%vector);

   // Jeff: use law of cosines to calculate angle for
   // every other update, it tends to make it slightly better
   %theta = angleFromCosLaw(%vector, %direction);

   // Jeff: add or subtract the current angle based if we are
   // going negative or positive
   if (%negative)
      %theta -= %angle;
   else
      %theta += %angle;

   // Jeff: from library.cs (thanks Zot!)
   // so appearently there is a maximum radius (defined at the top of this script)
   // that an object can rotate in radians until it goes to
   // "- pi / 2" which is why we subtract from tau (6.28)
   // this helps correct glichyness to approx 98% on rotation
   while (%theta > $maxRot)
      %theta -= $tau;
   while (%theta < $minRot)
      %theta += $tau;

   // Jeff: the rotation matrix!
   // [x y]
   // [z a]
   %matrix = %vector SPC %theta;

   %this.rotUpdate     = %matrix;
   %this.lastPosVecPos = %position;
}

// Jeff: does the same thing as the above function, except used as a standalone
// function and not attached to a specific client.  It is for the ghost objects
// even though it is shapebase, not recommended for regular objects.
function ShapeBase::tweenRotation(%this) {
   if (!isObject(%this))
      return;

   // Jeff: if we didn't move, just stop
   %position = roundPosVec(%this.getPosition(), %this.speed, %this.megaMarble);
   if (%position $= %this.lastPosVecPos)
      return;

   %xAxis = getWord(%this.tweenRot, 0);
   %yAxis = getWord(%this.tweenRot, 1);
   %zAxis = getWord(%this.tweenRot, 2);
   %aAxis = getWord(%this.tweenRot, 3);

   // Jeff: used for negative stuff
   %negative = %this.negative;

   %angle = 0;
   %z = mAtan(%xAxis, %yAxis); // Jeff: matan :3
   if (%negative) {
      %modifier = (%z > 0) ? %z * -1 : %z;
      %angle = (%zAxis + (%zAxis + %modifier)) * %aAxis;
   } else {
      %modifier = (%z < 0) ? %z * -1 : %z;
      %angle = (%zAxis - (%zAxis - %modifier)) * %aAxis;
   }

   %rotation = %xAxis SPC %yAxis SPC %angle;
   %direction = %this.getDirection();

   %vector = VectorCross(%rotation, %direction);
   if (%negative)
      %vector = VectorSub(%rotation, %vector);
   else
      %vector = VectorAdd(%rotation, %vector);
   %vector = VectorNormalize(%vector);

   %theta = angleFromCosLaw(%vector, %direction);

   if (%negative)
      %theta -= %angle;
   else
      %theta += %angle;

   while (%theta > $maxRot)
      %theta -= $tau;
   while (%theta < $minRot)
      %theta += $tau;

   %matrix = %vector SPC %theta;

   %this.tweenRot      = %matrix;
   %this.lastPosVecPos = %position;
}

// Jeff: get the unit vector axis direction that
// the marble is traveling in
function GameConnection::getDirection(%this) {
   if (!isObject(%this.player))
      return;
   // Jeff: initialize variables
   %opos = %this.player.getPosition();
   %npos = VectorSub(%opos, %this.lastDirectionPosition);
   %posX = getWord(%npos, 0);
   %posY = getWord(%npos, 1);
   %posZ = getWord(%npos, 2);

   // Jeff: determine if we are negative at each axis
   %negX = false;
   %negY = false;
   %negZ = false;
   %this.negative = false;
   if (%posX < 0)  {
      %negX = true;
      %posX *= -1;
   }
   if (%posY < 0)  {
      %negY = true;
      %posY *= -1;
   }
   if (%posZ < 0) {
      %negZ = true;
      %posZ *= -1;
   }

   %this.lastDirectionPosition = %opos;

   // Jeff: get direction check for X Y and if all else fails, return Z
   if (%posX > %posY && %posX > %posZ) {
      %this.negative = %negX;
      return %negX ? "-1 0 0" : "1 0 0";
   } else if (%posY > %posX && %posY > %posZ) {
      %this.negative = %negY;
      return %negY ? "0 -1 0" : "0 1 0";
   }
   %this.negative = %negZ;
   return %negZ ? "0 0 -1" : "0 0 1";
}

function ShapeBase::getDirection(%this) {
   // Jeff: initialize variables
   %opos = %this.getPosition();
   %npos = VectorSub(%opos, %this.lastDirectionPosition);
   %posX = getWord(%npos, 0);
   %posY = getWord(%npos, 1);
   %posZ = getWord(%npos, 2);

   // Jeff: determine if we are negative at each axis
   %negX = false;
   %negY = false;
   %negZ = false;
   %this.negative = false;
   if (%posX < 0)  {
      %negX = true;
      %posX *= -1;
   }
   if (%posY < 0)  {
      %negY = true;
      %posY *= -1;
   }
   if (%posZ < 0) {
      %negZ = true;
      %posZ *= -1;
   }

   %this.lastDirectionPosition = %opos;

   // Jeff: get direction check for X Y and if all else fails, return Z
   if (%posX > %posY && %posX > %posZ) {
      %this.negative = %negX;
      return %negX ? "-1 0 0" : "1 0 0";
   } else if (%posY > %posX && %posY > %posZ) {
      %this.negative = %negY;
      return %negY ? "0 -1 0" : "0 1 0";
   }
   %this.negative = %negZ;
   return %negZ ? "0 0 -1" : "0 0 1";
}

// Jeff: round position vector
function roundPosVec(%vec, %speed, %mega) {
   //echo(%speed);
   %vector = "";
   for (%i = 0; %i < getWordCount(%vec); %i ++) {
      %word = getWord(%vec, %i);
      %point = strpos(%word, ".");
      %len = (%point != -1) ? %point + posDecimalLength(%speed, %mega) : strlen(%word);
      %vector = (%vector $= "") ? getSubStr(%word, 0, %len) : %vector SPC getSubStr(%word, 0, %len);
   }
   return %vector;
}

// Jeff: get the length of how many decimals should exist
// in roundPosVec based on speed
function posDecimalLength(%speed, %mega) {
   %speed = %mega ? %speed / 4.875 : %speed; // Jeff: adjust mega marble
   if (%speed < 2.5)
      return 1;
   else if (%speed < 3.75)
      return 2;
   else if (%speed < 6)
      return 3;
   else if (%speed < 10)
      return 4;
   else if (%speed < 13)
      return 5;
   return 6;
}

// Jeff: get the angle from the law of cosines
//
// solve for the theta, take the arcCosine of the left side
// c^2 = a^2 + b^2 - 2ab(cos[theta])
// 0   = a^2 + b^2 - 2ab(cos[theta]) - c^2
// 2ab(cos[theta]) = a^2 + b^2 - c^2
// cos(theta) = (a^2 + b^2 - c^2) / (2ab)
// theta = (a^2 + b^2 - c^2) / (2ab)
// theta = 1 / cos(theta)
//
// solve for any angle by specifying the axis
// 0 - x axis (a)
// 1 - y axis (b)
// 2 - z axis (c)
function angleFromCosLaw(%vector, %direction) {
   %a = getWord(%vector, 0);
   %b = getWord(%vector, 1);
   %c = getWord(%vector, 2);

   // Jeff: find direction based upon unit vector
   for (%i = 0; %i < getWordCount(%direction); %i ++) {
      if (getWord(%direction, %i)) {
         %direction = %i;
         break;
      }
   }

   // Jeff: solve for the value and then we can take the inverse cosine
   // do  not use mAcos since it since its stupid:
   // mAcos(3) returns -1.#IND <- we can't do equations with a hash! #yoloswag
   // so...use 1 / cos(theta)

   %theta = 0;
   if (%direction == 0) // Jeff: X axis
      %theta = (mPow(%b, 2) + mPow(%c, 2) - mPow(%a, 2)) / (2 * %b * %c);
   else if (%direction == 1) // Jeff: Y axis
      %theta = (mPow(%a, 2) + mPow(%c, 2) - mPow(%b, 2)) / (2 * %a * %c);
   else
      %theta = (mPow(%a, 2) + mPow(%b, 2) - mPow(%c, 2)) / (2 * %a * %b);
   if (mCos(%theta) == 0) {
      return $pi / 2;
   }
   %theta = 1 / mCos(%theta);
   return %theta;
}
