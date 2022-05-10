//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
function LoadingGui::onAdd(%this)
{
   %this.qLineCount = 0;
}

//------------------------------------------------------------------------------
function LoadingGui::onWake(%this)
{
   $Game::Loading = true;
   // Play sound...
   CloseMessagePopup();

	if($doRecordDemo && $pref::marbleCategory !$= "Official Marbles")
	{
		$doRecordDemo = 0;
		$demoButton = 0;
		disconnect();
		ASSERT("Error Handler","You can\'t record a demo with a custom marble.\n\nPlease go to Marble Selection Screen and select an official MBP marble.");
	}

	// Random background - Phil
	%this.setBitmap(playMissionGui.bitmap);
}

//------------------------------------------------------------------------------
function LoadingGui::onSleep(%this)
{
   $Game::Loading = false;
   // Clear the load info:
   //HiGuy: Unused
   //if ( %this.qLineCount !$= "" )
   //{
      //for ( %line = 0; %line < %this.qLineCount; %line++ )
         //%this.qLine[%line] = "";
   //}
   //%this.qLineCount = 0;

   LOAD_MapName.setText( "" );
   //LOAD_MapDescription.setText( "" );
   LoadingProgress.setValue( 0 );
   //HiGuy: Technically doesn't exist
   //LoadingProgressTxt.setValue( "WAITING FOR SERVER" );

   // Stop sound...
}

// Jeff: if we click cancel in LoadingGui
function LoadingGui::cancel(%this) {
   if ($LB::ChallengeMode)
      exitChallenge();
   else
      exitGame();
}
