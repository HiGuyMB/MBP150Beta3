//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Sign base class
//-----------------------------------------------------------------------------

function Sign::onAdd(%this,%obj)
{
   if (%this.skin !$= "")
      %obj.setSkinName(%this.skin);
}

//-----------------------------------------------------------------------------
// Different signs...
//-----------------------------------------------------------------------------

datablock StaticShapeData(Sign)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/sign.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignDown)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/signdown.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignUp)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/signup.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignSide)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/signside.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignDownSide)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/signdown-side.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignUpSide)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/signup-side.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

//-----------------------------------------------------------------------------

datablock StaticShapeData(Arrow)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/sign.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

//-----------------------------------------------------------------------------

datablock StaticShapeData(SignCaution)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/cautionsign.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignCautionCaution: SignCaution)
{
   skin = "caution";
};

datablock StaticShapeData(SignCautionDanger: SignCaution)
{
   skin = "danger";
};


//-----------------------------------------------------------------------------

datablock StaticShapeData(SignFinish)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/finishlinesign.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

function SignFinish::onAdd(%this,%obj)
{
   %obj.playThread(0,"ambient");
}

//-----------------------------------------------------------------------------
datablock StaticShapeData(SignPlain)
{
   // Mission editor category
   category = "Signs";
   className = "Sign";

   // Basic Item properties
   shapeFile = "~/data/shapes/signs/plainsign.dts";
   mass = 1;
   friction = 1;
   elasticity = 0.3;
};

datablock StaticShapeData(SignPlainUp: SignPlain)
{
   skin = "up";
};

datablock StaticShapeData(SignPlainDown: SignPlain)
{
   skin = "down";
};

datablock StaticShapeData(SignPlainLeft: SignPlain)
{
   skin = "left";
};

datablock StaticShapeData(SignPlainRight: SignPlain)
{
   skin = "right";
};