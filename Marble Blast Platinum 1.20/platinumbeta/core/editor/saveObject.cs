//-----------------------------------------------------------------------------
// saveObject.cs
//
// Copyright (c) 2013 The Platinum Team
//-----------------------------------------------------------------------------

package Save {
   function SimObject::_presave(%this) {
      $_presave[%this.getID(), "super"] = %this.superClass;
      %this.superClass = "";

      $_presave[%this.getID(), "class"] = %this.class;
      %this.class = "";

      // Jeff: used for spawn grouping stuff, ask higuy
      $_presave[%this.getID(), "spawnWeight"] = %this.spawnWeight;
      %this.spawnWeight = "";

      //HiGuy: Spawn blocking
      $_presave[%this.getID(), "block"] = %this.block;
      %this.block = "";

      // Jeff: gem light for multiplayer
      $_presave[%this.getID(), "light"] = %this.light;
      %this.light = "";

      //HiGuy: Iterate all the items
      if (%this.getClassName() $= "SimGroup")
         for (%i = 0; %i < %this.getCount(); %i ++)
            %this.getObject(%i)._presave();
   }
   function SimObject::_postSave(%this) {
      %this.superClass  = $_presave[%this.getID(), "super"];
      %this.class       = $_presave[%this.getID(), "class"];
      %this.spawnWeight = $_presave[%this.getID(), "spawnWeight"];
      %this.block       = $_presave[%this.getID(), "block"];
      %this.light       = $_presave[%this.getID(), "light"];

      //HiGuy: Clean up
      deleteVariables("$_presave" @ %this.getID() @ "_*");

      //HiGuy: Iterate all the items
      if (%this.getClassName() $= "SimGroup")
         for (%i = 0; %i < %this.getCount(); %i ++)
            %this.getObject(%i)._postsave();
   }
   function SimObject::save(%this, %file) {
      %this._presave();

      Parent::save(%this, %file);

      %this._postsave();
   }
};

//HiGuy: JEFF YOU BROKE SAVING
activatePackage(Save);
