//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function ServerPlay2D(%profile)
{
   // Play the given sound profile on every client.
   // The sounds will be transmitted as an event, not attached to any object.
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++) {
   	if (!ClientGroup.getObject(%idx).loading)
	      ClientGroup.getObject(%idx).play2D(%profile);
	}
}

function ServerPlay3D(%profile,%transform)
{
   // Play the given sound profile at the given position on every client
   // The sound will be transmitted as an event, not attached to any object.
   for(%idx = 0; %idx < ClientGroup.getCount(); %idx++) {
   	if (!ClientGroup.getObject(%idx).loading)
	      ClientGroup.getObject(%idx).play3D(%profile,%transform);
	}
}

//HiGuy: No more console errors
function FakeGameConnection::play2D(%this) {}
function FakeGameConnection::play3D(%this) {}
