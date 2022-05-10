//String definitions
//------------

$e = 2.71828182846;
$pi = 3.1415926535;
$pi2 = 2 * $pi;
$pi_2 = $pi / 2;
$pi3_2 = ($pi * 3) / 2;
$pi_180 = $pi / 180;

//-------------------------------------------------------------------------------------
// General use functions
//------------

function vectorScale2(%vector, %scale)  //scales x by sx, y by sy, z by sz, instead of scaling x y z by s
{
  %vx = getWord(%vector, 0) * getWord(%scale, 0);
  %vy = getWord(%vector, 1) * getWord(%scale, 1);
  %vz = getWord(%vector, 2) * getWord(%scale, 2);

  return %vx SPC %vy SPC %vz;
}

//-------------------------------------------------------------------------------------

function sq(%input)  //squares the input
{
  %stuff = %input * %input;
  return %stuff;
}

//-------------------------------------------------------------------------------------

function pythag(%a, %b) //Pythagorean theorem
{
  return mSqrt(mPow(%a, 2) + mPow(%b, 2));
}

//-------------------------------------------------------------------------------------

//cando: combine all into one function

//function findObjectInSC(%obj) //tries to find a matching object in serverconnection based on the object's position
//{
  //%pos = %obj.getposition();
  //for (%i=0; %i < ServerConnection.getCount(); %i++)
  //{
    //if (ServerConnection.getObject(%i).getPosition() $= %pos) //NOTE: using position may have problems when moving something after it's already been moved
    //{
      //%objb = ServerConnection.getObject(%i);
      //return %objb;
    //}
  //}
  //return false;
//}

//TODO: if an object is hidden, it gets a new SC object when unhidden.

function findObjectInSC(%obj) //goes by pos and datablock
{
  if (%obj.getGroup() == ServerConnection.getID())
    return %obj;
  %classname = %obj.getClassName();
  if (%classname $= "PathedInterior")
  {
    %mp = findMPinSC(%obj);
    return %mp;
  }
  if (isObject(%obj.ServerObject))
    return %obj.ServerObject;
  if (%classname $= "ParticleEmitterNode")
  {
    %emitter = findEmitterInSC(%obj);
    %obj.ServerObject = %emitter;
    return %emitter;
  }
  %pos = %obj.getposition();
  %db = %obj.getDatablock();
  for (%i=0; %i < ServerConnection.getCount(); %i++)
  {
    %scobj = ServerConnection.getObject(%i);
    if (%scobj.getPosition() $= %pos && %scobj.getDatablock() == %db)
    {
      %obj.ServerObject = %scobj;
      return %scobj;
    }
  }
  return false;
}

function findEmitterInSC(%obj)
{
  %pos = %obj.getPosition();
  %ID = %obj.getID();
  while (true)
  {
    %obj = %ID + %i++;
    if (!isObject(%obj))
    {
      %miss++;
      if (%miss >= 10)
        return false;
      continue;
    }
    else
      %miss = 0;
    if (%obj.getClassName() !$= "ParticleEmitterNode")
      continue;
    if (%obj.getGroup().getID() == ServerConnection.getID() &&
        %obj.getPosition() $= %pos &&
        !%obj.foundme)
    {
      %obj.foundme = 1;
      return %obj;
    }
  }


  //nope, can't grab .emitter field from SC object

  %pos = %obj.getposition();
  %emitter = %obj.emitter;
  for (%i = 0; %i < ServerConnection.getCount(); %i++)
  {
    %scobj = ServerConnection.getObject(%i);
    if (%scobj.getPosition() $= %pos && %scobj.emitter $= %emitter)
    {
      return %scobj;
    }
  }
  return false;
}

function findObjectInSC3(%obj) //goes by bounding box size
{
  if (%obj.ServerObject !$= "")
    return %obj.ServerObject;
  %rx = getRadius("x", %obj);
  %ry = getRadius("y", %obj);
  %rz = getRadius("z", %obj);

  for (%i = 0; %i < ServerConnection.getCount(); %i++)
  {
    %scobj = ServerConnection.getObject(%i);
    if (getRadius("x", %scobj) == %rx)
    {
      %scry = getRadius("y", %scobj);
      %scrz = getRadius("z", %scobj);
      if (%scry == %ry && %scrz == %rz)
      {
        %obj.ServerObject = %scobj;
        return %scobj;
      }
    }
  }
  return false;
}

function findMPinSC(%obj)
{
  if ($FINDMP::LastObj $= %obj)
    return $FINDMP::LastSCObj;

  if (%obj.ServerObject !$= "")
    return %obj.ServerObject;

  %ii = %obj.interiorindex;
  for (%i=0; %i < ServerConnection.getCount(); %i++)
  {
    %scobj = ServerConnection.getObject(%i);
    if (%scobj.getClassName() $= "PathedInterior")
    {
      inspect(%scobj);
      canvas.popdialog(inspectdlg);

      if (InspectFields.getObject(15).getValue() $= %ii)
      {
        $FINDMP::LastObj = %obj;
        $FINDMP::LastSCObj = %scobj;
        %obj.ServerObject = %scobj;
        return %scobj;
      }
    }
  }
  $FINDMP::LastObj = %obj;
  $FINDMP::LastSCObj = 0;
  return false;
}

//-------------------------------------------------------------------------------------

function AlmostIsometric()  //sets some things to make the game look 2d
{
  $MP::MyMarble.getDatablock().cameradistance = 30;
  setdefaultfov(50);
}

//-------------------------------------------------------------------------------------

function getThatDistance(%posold, %posnew, %xyd)  //returns scalar distance based on input
{
  if (%posold $= "" || %posnew $= ""){
    return 0;
  }
  %xo = getword(%posold, 0);
  %yo = getword(%posold, 1);
  %zo = getword(%posold, 2);

  %xn = getword(%posnew, 0);
  %yn = getword(%posnew, 1);
  %zn = getword(%posnew, 2);

  %x = mAbs(%xo - %xn);
  %y = mAbs(%yo - %yn);
  %z = mAbs(%zo - %zn);

  %xy = pythag(%x, %y);

  if (%xyd)
    return %xy;

  %xyz = pythag(%xy, %z);

  return %xyz;
}

//-------------------------------------------------------------------------------------

function Compare3(%a, %b, %c, %variance) //Compares 3 values; if one value is greater than variance, then fail, else, input values are within variance.  No use for this atm
{
  %x = mAbs(%variance - mAbs(%a - %b));
  %y = mAbs(%variance - mAbs(%b - %c));
  %z = mAbs(%variance - mAbs(%a - %c));

  if (%x > %variance || %y > %variance || %z > %variance)
  {
    return false;
  }
  else
  { return true; }
}

//-------------------------------------------------------------------------------------

//mostly functional velocity functions: setvelocity, applyimpulse

function setMarbleVelocity(%vel, %flag)
{
  if (!isobject($scmarble)) //todo: use server marble
    findSCMarble();
  if ($Sim::Time == $Velocity::Time)
  {
    ASSERT("NOPE", "Attempted to run velocity function again on the same frame!" NL
                 "Last one: " @ $Velocity::previous NL
                 "This one: setMarbleVelocity " @ %vel);
    return;
  }
  $Velocity::Time = $Sim::Time;
  $Velocity::previous = "setMarbleVelocity" @ %vel;

  %pitch = $marblePitch;
  $MP::MyMarble.applyimpulse("0 0 0", $PrevVelNegative);
  $MP::MyMarble.applyimpulse("0 0 0", %vel);
  $PrevVelNegative = vectorscale(%vel, -1);
  if (%flag)
    return;
  //if (!%flag)
    %pitch -= 0.45;
  $mvpitch = %pitch;
  $mvYaw = $YawTotal;
}

function marbleApplyImpulse(%vel)
{
  if (!isobject($scmarble)) //todo: use server marble
    findSCMarble();
  if ($Sim::Time == $Velocity::Time)
  {
    ASSERT("NOPE", "Attempted to run velocity function again on the same frame!" NL
                 "Last one: " @ $Velocity::previous NL
                 "This one: marbleApplyImpulse " @ %vel);
    return;
  }
  $Velocity::Time = $Sim::Time;
  $Velocity::previous = "marbleApplyImpulse" @ %vel;
  %pitch = $marblePitch;
  %vel2 = vectoradd(%vel, vectorScale($MarbleVelocityVector, 62.5));
  $MP::MyMarble.applyimpulse("0 0 0", $PrevVelNegative);
  $MP::MyMarble.applyimpulse("0 0 0", %vel2);
  $PrevVelNegative = vectorscale(%vel2, -1);
  $mvpitch = %pitch - 0.45;
  $mvYaw = $YawTotal;
}

//-------------------------------------------------------------------------------------

function getRadius(%dim, %object) //get x/y/z radius of an object's bounding box
{
  if (%dim $= "x")
  {
    %word = 0;
  }
  if (%dim $= "y")
  {
    %word = 1;
  }
  if (%dim $= "z")
  {
    %word = 2;
  }
  %str = %object.getWorldBox();
  %radius = (mAbs(getword(%str, %word) - getword(%str, %word+3))/2);
  return %radius;
}

//-------------------------------------------------------------------------------------

function isNumber(%str)  //tests whether a string is a number
{
  if (%str > 0 || %str < 0)
    return true;
  else if (%str $= "0")
    return true;
  else
    return false;
}

function getBaseName(%name, %flag) //strip trailing numbers from string; alternatively, insert a space between basename and number
{
  %len = strlen(%name);
  %i = 0;
  while (isNumber(getSubStr(%name, %len - 1 - %i, 1)))
  {
    %pos = %len - %i - 1;
    %i++;
  }
  %basename = getSubStr(%name, 0, %pos);
  if (!%flag)
    return %basename;

  %combined = %basename SPC getSubStr(%name, %pos, 65535);
  return %combined;
}

//-------------------------------------------------------------------------------------

function floatMod(%a, %b)  //modulo for non-integers
{
  %a *= 1000000;
  %b *= 1000000;

  %mod = (%a % %b) / 1000000;
  return %mod;
}

//-------------------------------------------------------------------------------------

function mRound(%input) //rounding
{
  %floor = mFloor(%input);
  %value = %input - %floor;
  if (%value >= 0.5)
    return %floor + 1;
  else if (%value == 0)
    return %input;
  else
    return %floor;
}

//-------------------------------------------------------------------------------------

//Find unit vector based on given pitch and yaw
//Used in cannon velocity calculation

function getUnitVector(%pitch, %yaw, %inv)
{
  if (%pitch == 0)
    %temp = 1;
  else
    %temp = mcos(%pitch);


  %X += mcos(%yaw) * %temp;
  %Y += msin(%yaw) * %temp;
  %Z -= msin(%pitch);   //credit: MikeTacular at gamedev.net forums

  if (%inv)
     return %Y SPC %X SPC %Z;
  else
     return %X SPC %Y SPC %Z;
}

//-------------------------------------------------------------------------------------

function findSCMarble() //finds first marble in serverconnection
{
  if (isObject($scmarble))
    return;
  for (%i = 0; %i < serverconnection.getcount(); %i++)
  {
    %obj = serverconnection.getobject(%i);
    if (%obj.getclassname() $= "Marble")
    {
      $scMarble = %obj;
      return;
    }
  }
}


//-------------------------------------------------------------------------------------

// Easily take a level screenshot based on your camera's current position
// Hold F1 to frame without taking a screenshot; esc to go back
// Open console & close it to regain mouse control

function LevelSS(%do, %NOW)
{
  if (%do)
    setEmptyPlayGui(0, 1024, 512, 1);
  else if (%NOW)
  {
    // SCREENSHOT SAVES AS PNG. THANKS HIGUY!
    %file = getSubStr($server::missionfile, 0, strlen($server::missionfile)-4) @ ".png";
    setEmptyPlayGui(500, 512, 256, 0);
    schedule(200, 0, "screenshot", %file, 512, 256);
    schedule(500, 0, "setDefaultFov", $pref::Player::defaultFov);
  }
}

function setEmptyPlayGui(%delay, %x, %y, %reposition)
{
  LSSSquare.setVisible(%reposition);
  Canvas.setContent(EmptyPlayGuiHost);
  Canvas.cursorOff();
  setDefaultFov(120);
  if (%x !$= "" && %y !$= "")
  {
    if (%reposition)
      EmptyPlayGui.schedule(0, "resize", (getWord(Canvas.getExtent(), 0) - %x) / 2, (getWord(Canvas.getExtent(), 1) - %y) / 2, %x, %y);
    else
      EmptyPlayGui.schedule(0, "resize", 0, 0, %x, %y);
  }
  LSSSquare.setPosition(getWord(EmptyPlayGui.getExtent(), 1)/2 SPC 0);
  LSSSquare.setExtent(%y SPC %y);
  if (%delay > 0)
  {
    Canvas.schedule(%delay, "setContent", "PlayGui");
  }
}

//-------------------------------------------------------------------------------------

function measureAngle(%a1, %a2, %rad) //return angle measurement under 180 degrees (rad optional), may need tweaked
{
  %a1 += 720;
  %a2 += 720;

  if (%rad)
    %angle = $pi2;
  else
    %angle = 360;
  %first = mAbs(%a1 - %a2);
  %second = mAbs(%a1 + %angle - %a2);
  %third = mAbs(%a2 + %angle - %a1);

  if (mAbs(%a1 - %a2) < %angle / 2) //our first choice, if we aren't going over 180 degrees of measurement
    return mAbs(%a1 - %a2);

  //echo(%third);
  //echo(%second);
  //echo(%first);

  if (mAbs(%a1 - %a2) > %angle / 2) //if angle is greater than 180 or pi
  {
    if (%a1 > %a2)
      return %third;
    else
      return %second;
  }
  else
    return %first;

}

function CompareAngles(%a1, %a2, %var, %rad)  //if angles are within variance, return true, else false; 4th field is use radians flag
{
  if (%var <= mAbs(MeasureAngle(%a1, %a2, %rad)))
    return true;
  else
    return false;
}

function getTrueYaw(%aa)
{
  %uv = rotToVector(%aa, "1 0 0");
  return mAtan(getWord(%uv, 1), getWord(%uv, 0));
}

function getAdjustedYaw()
{


}

//-------------------------------------------------------------------------------------
//rotEtoAA Credit: Eric Hartman @ GG Community
//(original name: eulerToQuat)

function rotEtoAA(%euler, %radians)
{
    %euler2 = VectorScale(%euler, $pi_180);  //convert euler rotations to radians

    %matrix = MatrixCreateFromEuler(%euler2); //make a rotation matrix
    %xvec = getWord(%matrix, 3);    //get the parts of the matrix you need
    %yvec = getWord(%matrix, 4);
    %zvec = getWord(%matrix, 5);
    %ang  = getWord(%matrix, 6);    //this is in radians
    if (!%radians)
      %ang  = %ang * (180 / $pi);     //convert back to degrees

    %rotationMatrix = %xvec SPC %yvec SPC %zvec SPC %ang;  //put it all together

    return %rotationMatrix; //send it back
}

//-------------------------------------------------------------------------------------

function convertAA(%quat)
{
  %x = getWord(%quat, 0);
  %y = getWord(%quat, 1);
  %z = getWord(%quat, 2);
  %a = getWord(%quat, 3);

  %combined = %a * %x SPC %a * %y SPC %a * %z;
  return %combined;
}

//-------------------------------------------------------------------------------------
//Axis Angle to Euler (Pitch Yaw Roll); Credit: Matthew Jessick and Brendan Fletcher @ GG Community

function rotAAtoE(%axisAngle)
{
   %angleOver2 = -getWord(%axisAngle,3)/2;
   %sinThetaOver2 = mSin(%angleOver2);

   %q0 = mCos(%angleOver2);
   %q1 = getWord(%axisAngle,0) * %sinThetaOver2;
   %q2 = getWord(%axisAngle,1) * %sinThetaOver2;
   %q3 = getWord(%axisAngle,2) * %sinThetaOver2;
   %q4 = %q0*%q0;

   return vectorScale(mAsin(2*(%q2*%q3 + %q0*%q1))
                  SPC mAtan(%q0*%q2 - %q1*%q3, (%q4+%q3*%q3)-0.5)
                  SPC mAtan(%q0*%q3 - %q1*%q2, (%q4+%q2*%q2)-0.5) ,-180/$pi);
}

//Axis Angle to Matrix - Credit: euclideanspace.com

function rotAAtoM(%axisAngle)
{
  %ax = getWord(%axisAngle, 0);
  %az = getWord(%axisAngle, 1);
  %ay = getWord(%axisAngle, 2);
  %aa = getWord(%axisAngle, 3);

  %c = mCos(%aa);
  %s = mSin(%aa);
  %t = 1.0 - %c;
	// if axis is not already normalised then uncomment this
	// double magnitude = Math.sqrt(a1.x*a1.x + a1.y*a1.y + a1.z*a1.z);
	// if (magnitude==0) throw error;
	// a1.x /= magnitude;
	// a1.y /= magnitude;
	// a1.z /= magnitude;

  %m00 = %c + %ax*%ax*%t;
  %m11 = %c + %ay*%ay*%t;
  %m22 = %c + %az*%az*%t;


  %tmp1 = %ax*%ay*%t;
  %tmp2 = %az*%s;
  %m10 = %tmp1 + %tmp2;
  %m01 = %tmp1 - %tmp2;
  %tmp1 = %ax*%az*%t;
  %tmp2 = %ay*%s;
  %m20 = %tmp1 - %tmp2;
  %m02 = %tmp1 + %tmp2;
  %tmp1 = %ay*%az*%t;
  %tmp2 = %ax*%s;
  %m21 = %tmp1 + %tmp2;
  %m12 = %tmp1 - %tmp2;

  //echo(%m00 SPC %m01 SPC %m02);
  //echo(%m10 SPC %m11 SPC %m12);
  //echo(%m20 SPC %m21 SPC %m22);

  return (%m00 SPC %m10 SPC %m20 SPC
          %m01 SPC %m11 SPC %m21 SPC
          %m02 SPC %m12 SPC %m22);
  //return (%m00 SPC %m01 SPC %m02 SPC
        //%m10 SPC %m11 SPC %m12 SPC
        //%m20 SPC %m21 SPC %m22);
}

function matrixMultiplyByVector(%matrix, %vector)
{
  %vx = getWord(%vector, 0);
  %vy = getWord(%vector, 1);
  %vz = getWord(%vector, 2);

  %m0 = getWord(%matrix, 0); %m1 = getWord(%matrix, 1); %m2 = getWord(%matrix, 2);
  %m3 = getWord(%matrix, 3); %m4 = getWord(%matrix, 4); %m5 = getWord(%matrix, 5);
  %m6 = getWord(%matrix, 6); %m7 = getWord(%matrix, 7); %m8 = getWord(%matrix, 8);

  %x = %m0*%vx + %m1 * %vy + %m2 * %vz;
  %y = %m3*%vx + %m4 * %vy + %m5 * %vz;
  %z = %m6*%vx + %m7 * %vy + %m8 * %vz;

  return (%x SPC %y SPC %z);
}

function identityMatrix()
{
  return (1 SPC 0 SPC 0 SPC
          0 SPC 1 SPC 0 SPC
          0 SPC 0 SPC 1);
}

//Axis Angle/Quaternion conversions, quaternion addition/multi credit: euclideanspace.com

function rotAAtoQ(%aa)
{
  %ax = getWord(%aa, 0);
  %ay = getWord(%aa, 1);
  %az = getWord(%aa, 2);
  %angle = getWord(%aa, 3);

  %s = mSin(%angle/2);

  return mCos(%angle/2) SPC %ax * %s SPC %ay * %s SPC %az * %s;
}

function rotQtoAA(%q)
{
  %qw = getWord(%q, 0);
  %qx = getWord(%q, 1);
  %qy = getWord(%q, 2);
  %qz = getWord(%q, 3);

  %stuff = mSqrt(1 - %qw * %qw);

  if (%stuff == 0)
    return "1 0 0 0";

  return %qx / %stuff SPC %qy / %stuff SPC %qz / %stuff SPC 2 * mAcos(%qw);
}

function rotQadd(%q1, %q2)
{
  %w = getWord(%q1, 0) + getWord(%q2, 0);
  %x = getWord(%q1, 1) + getWord(%q2, 1);
  %y = getWord(%q1, 2) + getWord(%q2, 2);
  %z = getWord(%q1, 3) + getWord(%q2, 3);

  return %w SPC %x SPC %y SPC %z;
}

function rotQmultiply(%q1, %q2)
{
  %q1w = getWord(%q1, 0);
  %q2w = getWord(%q2, 0);
  %q1x = getWord(%q1, 1);
  %q2x = getWord(%q2, 1);
  %q1y = getWord(%q1, 2);
  %q2y = getWord(%q2, 2);
  %q1z = getWord(%q1, 3);
  %q2z = getWord(%q2, 3);

  %w = -%q1x * %q2x - %q1y * %q2y - %q1z * %q2z + %q1w * %q2w;
  %x =  %q1x * %q2w + %q1y * %q2z - %q1z * %q2y + %q1w * %q2x;
  %y = -%q1x * %q2z + %q1y * %q2w + %q1z * %q2x + %q1w * %q2y;
  %z =  %q1x * %q2y - %q1y * %q2x + %q1z * %q2w + %q1w * %q2z;

  return %w SPC %x SPC %y SPC %z;
}

function rotQnormalize(%q)
{
  %qw = getWord(%q, 0);
  %qx = getWord(%q, 1);
  %qy = getWord(%q, 2);
  %qz = getWord(%q, 3);

  %m = mSqrt(%qw*%qw + %qx*%qx + %qy*%qy + %qz*%qz);   //credit: cprogramming.com

  return %qw / %m SPC %qx / %m SPC %qy / %m SPC %qz / %m;
}

function rotQinvert(%q)  //conjugate
{
  %qw = getWord(%q, 0);
  %qxyz = getWords(%q, 1, 3);

  return %qw SPC vectorScale(%qxyz, -1);
}

function rotQtoVector(%q, %v)
{
  %qi = rotQinvert(%q);
  %qv = 0 SPC %v;

  %q1 = rotQmultiply(%qi, %qv);
  %q2 = rotQmultiply(%q1, %q);

  return 1 * getWord(%q2, 1) SPC 1 * getWord(%q2, 2) SPC getWord(%q2, 3);
}

// To grab direction (unit vector) of a local vector, you need to specify normal Torque rotation and unit vector.
// Example: rottovector(blah.getrotation(), "0 0 1"); (This finds local z axis unit vector)
//
// Reminder: RADIANS, MAN, RADIANS, not degrees!

function rottoVector(%aa, %v)
{
  if (%aa $= "1 0 0 0")
    return %v;
  %q = rotAAtoQ(%aa);
  return rotQtoVector(%q, %v);
}

function rotVectorToAA(%v)
{
  %difx = getWord(%v, 0);
  %dify = getWord(%v, 1);
  %difz = getWord(%v, 2);
  %difxy = pythag(%difx, %dify);

  //TODO: pitch has singularities
  %pitch = mRadToDeg(mAtan(%difz, %difxy));
  %yaw = mRadToDeg(mAtan(%dify, %difx));
  %finalrot = rotEtoAA(0 SPC %pitch SPC %yaw, 1);

  return %finalrot;
}

//calculate pitch and yaw from unit vector

function getPitchYaw(%uvec) //messed up badly
{
  %xy = mSqrt(mPow(getWord(%uvec, 0), 2) + mPow(getWord(%uvec, 1), 2));
  %pitch = mAtan(1 * getWord(%uvec, 2), 1 * %xy);
  %Yaw = mAtan(getWord(%uvec, 0), 1 * getWord(%uvec, 1));
  return %pitch SPC %Yaw;
}

function getCPYaw(%cp)
{
  %aa = %cp.getRotation();
  %v = vectorScale2(rotToVector(%aa, "0 1 0"), "-1 1 1");
  %yaw = mAtan(getWord(%v, 1), getword(%v, 0));
  if (%yaw < 0)
    %yaw += $pi2;
  return %yaw;
}

function getAAYaw(%aa)
{
  %v = vectorScale2(rotToVector(%aa, "0 1 0"), "-1 1 1");
  %yaw = mAtan(getWord(%v, 1), getword(%v, 0));
  if (%yaw < 0)
    %yaw += $pi2;
  return %yaw;
}
function getCPPitchYaw(%cp, %justyaw)
{
  %aa = %cp.getRotation();
  %v = vectorScale2(rotToVector(%aa, "0 1 0"), "-1 1 1");
  %x = getWord(%v, 0);
  %y = getWord(%v, 1);
  %z = getWord(%v, 2);
  %yaw = mAtan(%y, %x);
  if (%yaw < 0)
    %yaw += $pi2;

  if (%justyaw)
    return %yaw;

  %xy = mSqrt(mPow(%x, 2) + mPow(%y, 2));
  %pitch = mAtan(%z, %xy);

  return %pitch SPC %yaw;
}

//apply a series of rotations, can be used to rotate existing rotation about global axes
//or to apply rotation to local axes

function applyrotations(%r1, %r2, %r3, %r4)
{
  %i = 1;
  while (true)
  {
    if (%i == 1)
      %blah = %r1;
    if (%i == 2)
      %blah = %r2;  //messy, but %r[%i] won't work
    if (%i == 3)
      %blah = %r3;
    if (%i == 4)
      %blah = %r4;
    if (%blah $= "") break;
    //echo(%blah);
    if (getWordCount(%blah) == 3)
      %blah = rotEtoAA(%blah, 1);
    %q = rotAAtoQ(%blah);

    if (%qtotal $= "")
      %qtotal = %q;
    else
      %qtotal = rotQmultiply(%qtotal, %q);

    //echo(%qtotal);

    %i++;
    if (%i > 5) break;
  }
  %finalrot = rotQtoAA(rotQnormalize(%qtotal));
  //echo(%finalrot);
  return %finalrot;
}

function SceneObject::getRotation(%this) {
   return getWords(%this.getTransform(), 3, 6);
}

//-------------------------------------------------------------------------------------

function c() // Crash - can be used as hack repellant
{
  %a=1%0;
}

function d() // SUDDENLY DESKTOP
{
  d();
}

//-------------------------------------------------------------------------------------

function exec2(%name, %noCalls) // Find and exec script by base name only (execs gui / cs multiples, exact name/extension copies will fail)
{
  %noCalls = !!%noCalls;
  %t = 0;

  %toexec = FindNamedFile(%name, ".cs");
  if (%toexec !$= "")
  {
    exec(%toexec, %noCalls);
    %t++;
  }

  %toexec = FindNamedFile(%name, ".gui");
  if (%toexec !$= "")
  {
    exec(%toexec, %noCalls);
    %t++;
  }
  if (%t == 0)
    error("Script not found: " @ %name @ " cs/gui");
}

function rescanPaths()
{
  setModPaths($modPath);
}

//-------------------------------------------------------------------------------------

function timeSinceLoad()
{
  return getfield(formattime(getRealTime() - rootgroup.starttime), 1);
}


//-------------------------------------------------------------------------------------

//function SimGroup::onAdd(%this, %run)
//{
  //if (%this.getClassName() !$= "SimGroup")
    //return;
  //
  //if (!%run)
  //{
    //%this.schedule(5000, "onAdd", 1);
    //return;
  //}
  //if (%this.getCount() > 0)
    //if (%this.getObject(0).getClassName() !$= "Path")
      //return;
  //
  //if (!isObject(MPGroup))
  //{
    //new SimGroup(MPGroup);
    //MissionGroup.add(MPGroup);
    //bumpMissionGroup(MPGroup);
  //}
  //MPGroup.schedule(100, add, %this);
//}

//-------------------------------------------------------------------------------------

function Sun::onAdd(%this, %run)
{
  if (!%run)
  {
    %this.schedule(250, "onAdd", 1);
    return;
  }

  if (%this.getName() $= "")
  {
    for (%i = 1; isObject("Sun" @ %i); %i++)
      %a = 1;
    %this.setName("Sun" @ %i);
    if (isObject(MissionData))
      MissionData.add(%this);
  }
}

//-------------------------------------------------------------------------------------

function bumpMissionGroup(%grp)
{
  %mg = MissionGroup;
  %mg.bringtoFront(%grp);
  if (!isObject(MissionData))
  {
    schedule(200, 0, "buildMDataGroup");
  }
  else
    %mg.bringtoFront(MissionData);
}

function buildMDataGroup()
{
  if (isObject(MissionData) || !$Game::Running)
    return;

  %mg = MissionGroup;
  new SimGroup(MissionData);
  %md = MissionData;

  %mg.add(MissionData);
  %md.schedule(100, add, MissionInfo);
  for (%i = 1; isObject("Sun" @ %i); %i++)
    %md.schedule(100, add, "Sun" @ %i);
  %md.schedule(100, add, Sky);
  %md.schedule(100, add, MissionArea);
}

//-------------------------------------------------------------------------------------

function getServerType()
{
  if ($pref::HostMultiPlayer || $PQ::MissionObjectSelected.UseAdvancedCamera)
  {
    %serverType = "MultiPlayer";
    initDedicated(); //rediculous hackitude
  }
  else
    %serverType = "SinglePlayer";
  return %serverType;
}

//-------------------------------------------------------------------------------------

function setSunDirection()
{

}

//-------------------------------------------------------------------------------------

function getMarbleCamYaw()
{
  return $marbleYaw;
  %mpos = $MP::MyMarble.getPosition();
  %cpos = $MP::MyMarble.getCamPosition2();
  //if ($Game::GravityUV !$= "0 0 -1")
  //{
    //%guv = rottoVector($Game::GravityRot, "0 0 1");
    %vec = VectorDist(%mpos, %cpos);

    //normalize
    %uvec = VectorNormalize(vectorSub(%mpos, %cpos));
    //%uvec = vectorScale(%uvec, -1);

    //rotate by negative direction
    //%groti = rotQInvert(rotAAToQ($Game::GravityRot));
    %newvec = rotQToVector($GRotI, %uvec);

    //scale up again
    %newvec = VectorScale(%newvec, %vec);
    %newvec = vectoradd(%mpos, %newvec);

    %cpos = %newvec;
  //}
  %uv = vectorNormalize(vectorSub(%mpos, %cpos));

  return mAtan(getWord(%uv, 1), getWord(%uv, 0));
}

function calcGravityUV()
{
  %uv = rottoVector($Game::GravityRot, "0 0 1");
  if (getWord(%uv, 2) $= "-1")
    %uv = "0 0 -1";
  $Game::GravityUV = %uv;

  calcGRot();
}

function calcGRot()
{
  $GRot = rotAAtoQ($Game::GravityRot);
  $GRotI = rotQinvert($GRot);
}

//-------------------------------------------------------------------------------------

function getNameSpace(%string, %index)
{
  if (%string $= "" || %index $= "")
    error("usage: getNameSpace(%string, %index)");

  //easymode: convert to tabs and use getField
  %string = strReplace(%string, "::", "\t");
  %index = getField(%string, %index);
  return %index;
}

//-------------------------------------------------------------------------------------

function padZeroes(%str, %len)
{
  %strlen = strLen(%str);
  if (%strlen >= %len)
    return %str;

  for (%i = %len - %strlen; %i > 0; %i--)
    %str = "0" @ %str;

  return %str;
}

//-------------------------------------------------------------------------------------

function strTrimToMultiples(%str, %mult)
{
  %strlen = strLen(%str);
  %mod = strLen(%str) % %mult;

  echo(%strlen SPC %mod SPC %mult);

  return getSubStr(%str, 0, %strlen - %mod) SPC getSubStr(%str, %strlen - %mod, %mod);
}

//-------------------------------------------------------------------------------------

function getYawTotal()
{
  %ret = getMarbleCamYaw() - $startyaw;
  while(%ret < 0)
    %ret += $pi2;
  return %ret;
}

//-------------------------------------------------------------------------------------

function normalOfGravity()
{
  %vec = rottoVector($Game::GravityRot, "0 0 -1");
  return %vec;
}

function rotNormalOfGravity(%userad)
{
  //degrees, mainly for spawning emitters with rotation
  %rot = applyrotations($Game::GravityRot, "0 180 0");
  if (!%userad)
    %rot = getWords(%rot, 0, 2) SPC mRadToDeg(getWord(%rot, 3));
  return %rot;
}

//-------------------------------------------------------------------------------------

function SimObject::isMemberOf(%this, %group)
{
  if (!isObject(%group))
  {
    error(%this @ "::isMemberOf - group doesn\'t exist: " @ %group);
    return false;
  }
  %group2 = %this.getGroup();
  if (%group2 == %group.getID())
    return true;
  else
    return %group2.isMemberOf(%group);
}

//-------------------------------------------------------------------------------------

function getGemPointTotal()
{
  for (%group = GemGroup; isObject(%group); %group = "GemGroup" @ 1 + %i)
  {
    %value = 0;
    for(%h = 0; %h < %group.getCount(); %h++)
    {
      %obj = %group.getObject(%h);
      switch$(%obj.getDatablock().getName())
      {
        case "GemItemRed":
          %val = 1;
        case "GemItemYellow":
          %val = 2;
        //case "GemItemOrange":
          //%val = 3;
        case "GemItemBlue":
          %val = 5;
        case "GemItemPlatinum":
          %val = 10;
        default:
          %val = 0;
      }
      %value += %val;
    }
    echo(%value @ " points in " @ %group.getName() @ ".");
    %total += %value;
    %i++;
  }
  echo(%total @ " points in all gem groups.");
}

//-------------------------------------------------------------------------------------

function scanforclass(%class, %group)
{
  if (%group $= "")
    %group = RootGroup;
  else if (%group.getName() $= "MaterialProperties")
    return;
  for (%i = 0; %i < %group.getCount(); %i++)
  {
    %obj = %group.getObject(%i);
    %classname = %obj.getClassName();
    if (%classname $= %class)
	    echo(%obj.getID() SPC %obj.getClassName() SPC %obj.getName() SPC "in" SPC %group.getClassName() SPC %group.getName() SPC %group.getID());
    else if (%classname $= "SimGroup" || %classname $= "SimSet")
	    scanforclass(%class, %obj);
  }
}

//-------------------------------------------------------------------------------------

function paste()
{
  eval(getClipboard());
}

//-------------------------------------------------------------------------------------
//code adapted from cubic.org/docs/bezier.htm

// simple linear interpolation between two points
function lerp(%pointa, %pointb, %time)
{
  %ax = getWord(%pointa, 0);
  %ay = getWord(%pointa, 1);
  %bx = getWord(%pointb, 0);
  %by = getWord(%pointb, 1);
  %destx = %ax + (%bx-%ax)*%time;
  %desty = %ay + (%by-%ay)*%time;
  return %destx SPC %desty;
}

// evaluate a point on a bezier-curve. t goes from 0 to 1.0
function bezier(%a, %b, %c, %d, %t)
{
  %ab = lerp(%a,%b,%t);           // point between a and b (green)
  %bc = lerp(%b,%c,%t);           // point between b and c (green)
  %cd = lerp(%c,%d,%t);           // point between c and d (green)
  %abbc = lerp(%ab,%bc,%t);       // point between ab and bc (blue)
  %bccd = lerp(%bc,%cd,%t);       // point between bc and cd (blue)
  %dest = lerp(%abbc,%bccd,%t);   // point on the bezier-curve (black)
  return %dest;
}

function bezier3d(%a, %b, %c, %d, %t)
{
  //first get xy
  %axy = getWords(%a, 0, 2);
  %bxy = getWords(%b, 0, 2);
  %cxy = getWords(%c, 0, 2);
  %dxy = getWords(%d, 0, 2);
  %xy = bezier(%axy, %bxy, %cxy, %dxy, %t);

  //next grab Z
  %ayz = getWords(%a, 1, 2);
  %byz = getWords(%b, 1, 2);
  %cyz = getWords(%c, 1, 2);
  %dyz = getWords(%d, 1, 2);
  %yz = bezier(%ayz, %byz, %cyz, %dyz, %t);
  %z = getWord(%yz, 1);

  //result should be a point in 3d space
  return %xy SPC %z;
}

//-----------------------------------------------------------------

function spawnEmitter(%time, %db, %position, %parentto)
{
  if (%time $= "") %time = 1000;
  if (!isObject(%db))
    return;
  %obj = new ParticleEmitterNode(){
      datablock = FireWorkNode;
      emitter = %db;
      position = %position;
  };
  MissionCleanup.add(%obj);
  %obj.schedule(%time, "delete");

  if (isObject(%parentto))
  %obj.setParent(%parentto);
}

//-----------------------------------------------------------------

function getFrontSpaces(%str)
{
  %i = 0;
  while (getSubStr(%str, %i++, 1) $= " ")
    %spaces = %spaces @ " ";
  return %spaces;
}

function lastWord(%str)
{
  return getWord(%str, getWordCount(%str)-1);
}

function convertText(%text)
{
  %len = strLen(%text);
  for (%i = 0; %i < %len; %i++)
  {
    %char = getSubStr(%text, %i, 1);
    switch$(%char)
    {
      case "\n":
        %ret = %ret @ "\\n";
      case "\r":
        %ret = %ret @ "\\r";
      case "\t":
        %ret = %ret @ "\\t";
        case "\c0":
          %ret = %ret @ "\\c0";
        case "\c1":
          %ret = %ret @ "\\c1";
        case "\c2":
          %ret = %ret @ "\\c2";
        case "\c3":
          %ret = %ret @ "\\c3";
        case "\c4":
          %ret = %ret @ "\\c4";
        case "\c5":
          %ret = %ret @ "\\c5";
        case "\c6":
          %ret = %ret @ "\\c6";
        case "\c7":
          %ret = %ret @ "\\c7";
        case "\c8":
          %ret = %ret @ "\\c8";
        case "\c9":
          %ret = %ret @ "\\c9";
      case "\cr":
        %ret = %ret @ "\\cr";
      case "\cp":
        %ret = %ret @ "\\cp";
      case "\co":
        %ret = %ret @ "\\co";
      //case "\xhh":
        //%ret = %ret @ "\\xhh";
      case "'":
        %ret = %ret @ "\\'";
      case "\'":
        %ret = %ret @ "\\\'";
      case "\"":
        %ret = %ret @ "\\\"";
      case "\\":
        %ret = %ret @ "\\\\";
      default:
        %ret = %ret @ %char;
    }
  }
  return "\"" @ %ret @ "\";";
}

function stripLastChar(%text)
{
  %len = strLen(%text);
  if (%len < 2)
    return "";
  return getSubStr(%text, 0, %len-1);
}

function fixMisData()
{
  dumpMisData();
  replaceMisData();
  replaceMisData();
}

function Marble::getCamPosition2(%marble)
{
  %db = %marble.getDatablock();
  %extension = 0.9 + %db.cameradistance * 0.0000642538;
  %NOG = normalOfGravity();
  %rNOG = rotNormalOfGravity(1);
  %extend = vectorAdd(%marble.getPosition(), vectorScale(rotToVector(applyRotations(%rNOG, "0 1 0 " @ ($marblePitch-$pi)), %NOG), %extension));

  %moveback = vectorAdd(%extend, vectorScale(rotToVector(applyRotations(%rNOG, "0 1 0 " @ ($marblePitch+$pi_2)), %NOG), %db.cameradistance));

  if (isObject(GCP2Target))
    GCP2Target.settransform(%extend);

  return %moveback;
}
