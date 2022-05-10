//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Additional codes for Marble Blast Platinum by Matan Weissman
//-----------------------------------------------------------------------------

// You will need to play with these values for balanced game play with Marble Blast Platinum

//HiGuy: This was in PQ, just puts them into a nice SimGroup for us :)
new SimGroup(MaterialGroup) {
   new MaterialProperty(GrassFrictionMaterial) {
      friction = 1.50;
      restitution = 0.35;
   };

   new MaterialProperty(TarmacFrictionMaterial) {
      friction = 0.35;
      restitution = 0.7;
   };

   new MaterialProperty(RugFrictionMaterial) {
      friction = 6;
      restitution = 0.5;
   };

   new MaterialProperty(IceFrictionMaterial) {
      friction = 0.03;
      restitution = 0.95;
   };

   new MaterialProperty(CarpetFrictionMaterial) {
      friction = 6;
      restitution = 0.5;
   };

   new MaterialProperty(SandFrictionMaterial) {
      friction = 4;
      restitution = 0.1;
   };

   new MaterialProperty(WaterFrictionMaterial) {
      friction = 6;
      restitution = 0;
   };

   new MaterialProperty(BouncyFrictionMaterial) {
      friction = 0.2;
      restitution = 0;
      force = 15;
   };

   new MaterialProperty(RandomForceMaterial) {
      friction = -1;
      restitution = 1;
   };

   // MBU/O values...

   new MaterialProperty(HighFrictionMultiplayerMaterial) {
      friction = 6;
      restitution = 0.3;
   };

   // added to stop the game from popping an error in the console log that it cannot find the
   // material property even though the game runs fine without that minor piece of code
   // so these lines of code are an extra to shut the game up and you can remove them if you wish

   new MaterialProperty(DefaultMaterial) {
      friction = 1;
      restitution = 1;
      force = 0;
   };

   // these values are for a balanced game play with Mini Marble Golf (the levels)
   // they might be a tad different to their MBP equivalent and are more realistic

   new MaterialProperty(MMGGrassMaterial) {
      friction = 0.9;
      restitution = 0.5;
   };

   new MaterialProperty(MMGSandMaterial) {
      friction = 6;
      restitution = 0.1;
   };

   new MaterialProperty(MMGWaterMaterial) {
      friction = 6;
      restitution = 0;
   };

   new MaterialProperty(MMGIceMaterial) {
      friction = 0.03;
      restitution = 0.95;
   };

   new MaterialProperty(MMGIceShadowMaterial) {
      friction = 0.2;
      restitution = 0.95;
   };

   // These are some old values

   new MaterialProperty(BumperMaterial) {
      friction = 0.5;
      restitution = 0;
      force = 15;
   };

   new MaterialProperty(ButtonMaterial) {
      friction = 1;
      restitution = 1;
   };

   // MBG Values for their frictions. MBP ones still rock, though.

   // Space

   new MaterialProperty(NoFrictionMaterial) {
      friction = 0.01;
      restitution = 0.5;
   };

   // Mud

   new MaterialProperty(LowFrictionMaterial) {
      friction = 0.20;
      restitution = 0.5;
   };

   // Grass

   new MaterialProperty(HighFrictionMaterial) {
      friction = 1.50;
      restitution = 0.5;
   };

   // Yellow ramps with arrows from Three Fold Maze and Escher's Race

   new MaterialProperty(VeryHighFrictionMaterial) {
      friction = 2;
      restitution = 1;
   };

   // Normal floor

   new MaterialProperty(RubberFloorMaterial) {
      friction = 1;
      restitution = 1;
   };

   // Oil Slick

   new MaterialProperty(IceMaterial) {
      friction = 0.05;
      restitution = 0.5;
   };
};

// YAY FOR MBP
//
addMaterialMapping( "", DefaultMaterial);

// Used for levels in Marble Blast Platinum
addMaterialMapping( "grass" , GrassFrictionMaterial);
addMaterialMapping( "ice1" , IceFrictionMaterial);
addMaterialMapping( "rug" ,    RugFrictionMaterial);
addMaterialMapping( "tarmac" ,    TarmacFrictionMaterial);
addMaterialMapping( "carpet" ,    CarpetFrictionMaterial);
addMaterialMapping( "sand" ,    SandFrictionMaterial);
addMaterialMapping( "water" ,    WaterFrictionMaterial);
addMaterialMapping( "floor_bounce" ,    BouncyFrictionMaterial);
addMaterialMapping( "mbp_chevron_friction" ,    RandomForceMaterial);
addMaterialMapping( "mbp_chevron_friction2" ,    RandomForceMaterial);
addMaterialMapping( "mbp_chevron_friction3" ,    RandomForceMaterial);

// Added to stop the game from popping an error in the console log that it cannot find the
// material property even though the game runs fine without that minor piece of code
// so this line of code is an extra to shut the game up and you can remove it if you wish

addMaterialMapping( "stripe_caution", DefaultMaterial);

// Used for Mini Marble Golf
addMaterialMapping( "mmg_grass" ,    MMGGrassMaterial);
addMaterialMapping( "mmg_sand" ,    MMGSandMaterial);
addMaterialMapping( "mmg_water" ,    MMGWaterMaterial);
addMaterialMapping( "mmg_ice" ,    MMGIceMaterial);

// Multiplayer
addMaterialMapping( "mmg_ice_shadow" ,    MMGIceShadowMaterial);
addMaterialMapping( "friction_mp_high" ,    HighFrictionMultiplayerMaterial);
addMaterialMapping( "friction_mp_high_shadow" ,    HighFrictionMultiplayerMaterial);

// Some DTS textures from MBG
addMaterialMapping( "bumper-rubber" ,    BumperMaterial);
addMaterialMapping( "triang-side" ,      BumperMaterial);
addMaterialMapping( "triang-top" ,      BumperMaterial);
addMaterialMapping( "pball-round-side" , BumperMaterial);
addMaterialMapping( "pball-round-top" , BumperMaterial);
addMaterialMapping( "pball-round-bottm" , BumperMaterial);
addMaterialMapping( "button" , ButtonMaterial);

// Back to MBG... BOOOOOOOO
// Textures listed in BrianH texture document
addMaterialMapping( "grid_warm" ,    DefaultMaterial);
addMaterialMapping( "grid_cool" ,    DefaultMaterial);
addMaterialMapping( "grid_neutral" , DefaultMaterial);

addMaterialMapping( "stripe_cool" ,    DefaultMaterial);
addMaterialMapping( "stripe_neutral" , DefaultMaterial);
addMaterialMapping( "stripe_warm" ,    DefaultMaterial);
addMaterialMapping( "tube_cool" ,      DefaultMaterial);
addMaterialMapping( "tube_neutral" ,   DefaultMaterial);
addMaterialMapping( "tube_warm" ,      DefaultMaterial);

addMaterialMapping( "solid_cool1" ,      DefaultMaterial);
addMaterialMapping( "solid_cool2" ,      DefaultMaterial);
addMaterialMapping( "solid_neutral1" ,   DefaultMaterial);
addMaterialMapping( "solid_neutral2" ,   DefaultMaterial);
addMaterialMapping( "solid_warm1" ,      DefaultMaterial);
addMaterialMapping( "solid_warm2" ,      DefaultMaterial);

addMaterialMapping( "pattern_cool1" ,      DefaultMaterial);
addMaterialMapping( "pattern_cool2" ,      DefaultMaterial);
addMaterialMapping( "pattern_neutral1" ,   DefaultMaterial);
addMaterialMapping( "pattern_neutral2" ,   DefaultMaterial);
addMaterialMapping( "pattern_warm1" ,      DefaultMaterial);
addMaterialMapping( "pattern_warm2" ,      DefaultMaterial);

addMaterialMapping( "friction_none" ,    NoFrictionMaterial);
addMaterialMapping( "friction_low" ,     LowFrictionMaterial);
addMaterialMapping( "friction_high" ,    HighFrictionMaterial);
// used for ramps in Escher level [and three fold maze]
addMaterialMapping( "friction_ramp_yellow" ,    VeryHighFrictionMaterial);

// old textures (to be removed?)
addMaterialMapping( "grid1" , RubberFloorMaterial);
addMaterialMapping( "grid2" , RubberFloorMaterial);
addMaterialMapping( "grid3" , RubberFloorMaterial);
addMaterialMapping( "grid4" , RubberFloorMaterial);

// some part textures
addMaterialMapping( "oilslick" , IceMaterial);
addMaterialMapping( "base.slick" , IceMaterial);
addMaterialMapping( "ice.slick" , IceMaterial);

function MPadjustFrictions() {
   if ($Server::ServerType $= "MultiPlayer") {
      //Multiplayer stuff goes in here
      //<material>.friction = whatever
      MMGIceMaterial.friction = 0.2;
   } else {
      //Single player stuff goes in here
      MMGIceMaterial.friction = 0.03;
   }
}
