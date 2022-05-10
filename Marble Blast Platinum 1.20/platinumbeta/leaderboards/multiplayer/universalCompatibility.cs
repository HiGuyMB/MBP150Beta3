//-----------------------------------------------------------------------------
// universalCompatibility.cs
//
// Copyright (c) 2013 The Platinum Team
//
// Jeff: Allows cross compatible Multiplayer between windows/mac/linux
//       SUCK IT PLEBS WHO DIDN'T THINK THIS WAS POSSIBLE, SUCK IT
//       As shadowmarble said, "mac [users of a certain orientation]" they'll be happy now <3
//-----------------------------------------------------------------------------

// Jeff: register a superclass with all objects that have a typemask > 0
function applySuperClass(%group)
{
   // Jeff: this was for compatibility, please no.
   return;

   %count = %group.getCount();
   for (%i = 0; %i < %count; %i ++)
   {
      %obj = %group.getObject(%i);
      if (%obj.getClassName() $= "SimGroup")
         applySuperClass(%obj);
      else if (%obj.getType()) // Jeff: if we are not 0, we can have a superclass
         %obj.superClass = "RenderObject";
   }
}

// Jeff: was used with compatibility, commenting for now
//package SimGroupAdd
//{
   //function SimGroup::add(%this, %obj)
   //{
      //if (%obj.getType())
         //%obj.superClass = "RenderObject";
      //Parent::add(%this, %obj);
   //}

   // Jeff: needed for client sided stuff for compatibility
   //function ServerConnection::add(%this, %obj)
   //{
      //if (%obj.getType())
         //%obj.superClass = "RenderObject";
      //Parent::add(%this, %obj);
   //}
//};
//activatePackage(SimGroupAdd);