//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Loading info is text displayed on the client side while the mission
// is being loaded.  This information is extracted from the mission file
// and sent to each the client as it joins.
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// clearLoadInfo
//
// Clears the mission info stored
//------------------------------------------------------------------------------
function clearLoadInfo() {
   if (isObject(MissionInfo))
      MissionInfo.delete();
}

//------------------------------------------------------------------------------
// buildLoadInfo
//
// Extract the map description from the .mis file
//------------------------------------------------------------------------------
function buildLoadInfo( %mission ) {
	clearLoadInfo();

	%infoObject = "";
	%file = new FileObject();

	if ( %file.openForRead( %mission ) ) {
		%inInfoBlock = false;

		while ( !%file.isEOF() ) {
			%line = %file.readLine();
			%line = trim( %line );

			if( %line $= "new ScriptObject(MissionInfo) {" )
				%inInfoBlock = true;
			else if( %inInfoBlock && %line $= "};" ) {
				%inInfoBlock = false;
				%infoObject = %infoObject @ %line;
				break;
			}

			if( %inInfoBlock )
			   %infoObject = %infoObject @ %line @ " ";
		}

		%file.close();
	}

   // Will create the object "MissionInfo"
	eval( %infoObject );
	%file.delete();

	if (isObject(MissionGroup)) {
	   MissionGroup.add(MissionInfo);
	   MissionGroup.bringToFront(MissionInfo);
   }
}

//------------------------------------------------------------------------------
// dumpLoadInfo
//
// Echo the mission information to the console
//------------------------------------------------------------------------------
function dumpLoadInfo()
{
	echo( "Mission Name: " @ MissionInfo.name );
   echo( "Mission Description:" );
}

//------------------------------------------------------------------------------
// sendLoadInfoToClient
//
// Sends mission description to the client
//------------------------------------------------------------------------------
function sendLoadInfoToClient( %client )
{
   echo("You just lost the game");
   %missionInfo = dumpObject(MissionInfo);

	messageClient( %client, 'MsgLoadInfo', "", MissionInfo.name );
	messageClient( %client, 'MsgLoadMode', "", MissionInfo.gameMode );
   messageClient( %client, 'MsgLoadDescripition', "", MissionInfo.desc );
   messageClient( %client, 'MsgServerDedicated', "", $Server::Dedicated );
   messageClient( %client, 'MsgServerDescription', "", $MPPref::Server::Info );
   messageClient( %client, 'MsgServerName', "", $MPPref::Server::Name );
   %maxChars = 255;
   for (%i = 0; %i < mCeil(strLen(%missionInfo) / %maxChars); %i ++) {
      messageClient( %client, 'MsgLoadMissionInfoPart', "", getSubStr(%missionInfo, %maxChars * %i, %maxChars));
   }
   messageClient( %client, 'MsgLoadInfoDone' );
}
