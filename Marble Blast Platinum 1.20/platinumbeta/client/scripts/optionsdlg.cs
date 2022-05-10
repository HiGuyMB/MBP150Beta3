function optionsDlg::setPane(%this, %pane)
{
   OptAudioPane.setVisible(false);
   OptGraphicsPane.setVisible(false);
   OptControlsPane.setVisible(false);
   if(%pane $= "Graphics")
   {
      OptGraphicsPane.setVisible(true);
      if ($platform $= "x86UNIX") {
         StencilShadowsBitmap.setVisible(true);
         StencilShadowsButton.setVisible(true);
         OptGfxVideoDriver.setVisible(false);
         OptGfxOpenGL.setVisible(false);
         OptGfxD3D.setVisible(false);
      } else if ($platform $= "macos") {
         StencilShadowsBitmap.setVisible(false);
         StencilShadowsButton.setVisible(false);
         OptGfxVideoDriver.setVisible(false);
         OptGfxOpenGL.setVisible(false);
         OptGfxD3D.setVisible(false);
      }

		// Always have the marble controls showing, and the button pressed down - Phil
		OptMarbleControlsPaneButton.performClick();
   }
   else
      ("Opt" @ %pane @ "Pane").setVisible(true);
   OptControlsPane.showMappings();
   %par = OptBoxFrame.getGroup();

   //HiGuy: More console warnings for old stuff.

   //RootGroup.add(OptBoxFrame);
   //RootGroup.add("Opt" @ %pane @ "Tab");
   //%par.add(OptBoxFrame);
   //%par.add("Opt" @ %pane @ "Tab");
}

// Jeff: redoing the Home button because this
//       gui is now used for lb support
function optionsHome() {
   OptionsDlg.applyGraphics();
   %gui = $LB::LoggedIn ? "LBChatGui" : "MainMenuGui";
   Canvas.setContent(%gui);
}

function OptGraphicsPane::setResolution(%this, %res)
{
   $pref::Video::resolution = %res SPC getWord($pref::Video::resolution, 2);
   $OldConfig::isChanged = true;
}

function OptGraphicsPane::setMode(%this, %mode)
{
   $pref::Video::fullScreen = (%mode $= "Full");
   $OldConfig::isChanged = true;
}

function OptGraphicsPane::setDriver(%this, %driver)
{
   $pref::Video::displayDevice = %driver;
   $OldConfig::isChanged = true;
}

function OptGraphicsPane::setDepth(%this, %depth)
{
   $pref::Video::resolution = getWords($pref::Video::resolution, 0, 1) SPC %depth;
   $OldConfig::isChanged = true;
}

function OptionsDlg::onWake(%this, %dontDiscard)
{
   OptionsDlg.setPane("Graphics");
   %buffer = getDisplayDeviceList();
   %count = getFieldCount( %buffer );

   OptGfx800600.setValue(false);
   OptGfx1024768.setValue(false);
   OptGfxCustom.setValue(false);

   // Jeff: this was missing until now
   OptGfx12801024.setValue(false);

   %res = getWords($pref::Video::resolution, 0, 1);
   switch$(%res)
   {
      case "800 600":
         OptGfx800600.setValue(true);
      case "1024 768":
         OptGfx1024768.setValue(true);
	  case "1280 1024":
		 OptGfx12801024.setValue(true);
	  default:
	     OptGfxCustom.setValue(true);
   }

   OptGfxD3D.setVisible($platform $= "windows");
   OptGfxOpenGL.setVisible($platform $= "windows");
   OptGfxVideoDriver.setVisible($platform $= "windows");

   OptAudioExtras.setActive(0);

   // Matan: removing a bunch of code that according to HiGuy is unnecessary.
   //	     basically we used to have a driver/audio list for users to select
   //        but it was removed in later MB but this code was not removed

   // HiGuy: Most of it was. I re-added the OpenGL / D3D video driver selection
   //        buttons. Tell me what you think of them. You can reposition them
   //        above, if needed.
   %isOgl = $pref::Video::displayDevice $= "OpenGL";
   if ($platform $= "windows") { //HiGuy : n/a on mac
      OptGfxOpenGL.setValue(%isOgl);
      OptGfxD3D.setValue(!%isOgl);
   }

   OptGfxFull.setValue($pref::Video::fullScreen);
   OptGfxWindow.setValue(!$pref::Video::fullScreen);

   %is32 = getWord($pref::Video::resolution, 2) == 32;
   OptGfx16.setValue(!%is32);
   OptGfx32.setValue(%is32);

   // Audio
   OptAudioUpdate();

   // Matan: there is a master volume and channel volume 1/2 which work fine
   // even when these are out. That said, MasterVolume MAY be unnecessary
   // but I like the idea (plus it works when modding the prefs file.) TODO?
   //OptAudioVolumeMaster.setValue( $pref::Audio::masterVolume);

	// Spy47 : SAVE OLD CONFIGS!!!
	// update: don't do if given %dontDiscard true.
	if( !%dontDiscard )
	{
		$OldConfig::Video::displayDevice = $pref::Video::displayDevice;
		$OldConfig::Video::resolution = $pref::Video::resolution;
		$OldConfig::Video::fullScreen = $pref::Video::fullScreen;
		$OldConfig::isChanged = false;
	}
}

function OptionsDlg::onSleep(%this)
{
   // write out the control config into the fps/config.cs file
   moveMap.save( "~/client/config.cs" );
}

function OptGraphicsDriverMenu::onSelect( %this, %id, %text )
{
	// Attempt to keep the same res and bpp settings:
	if ( OptGraphicsResolutionMenu.size() > 0 )
		%prevRes = OptGraphicsResolutionMenu.getText();
	else
		%prevRes = getWords( $pref::Video::resolution, 0, 1 );

	// Check if this device is full-screen only:
	if ( isDeviceFullScreenOnly( %this.getText() ) )
	{
		OptGraphicsFullscreenToggle.setValue( true );
		OptGraphicsFullscreenToggle.setActive( false );
		OptGraphicsFullscreenToggle.onAction();
	}
	else
		OptGraphicsFullscreenToggle.setActive( true );

	if ( OptGraphicsFullscreenToggle.getValue() )
	{
		if ( OptGraphicsBPPMenu.size() > 0 )
			%prevBPP = OptGraphicsBPPMenu.getText();
		else
			%prevBPP = getWord( $pref::Video::resolution, 2 );
	}

	// Fill the resolution and bit depth lists:
	OptGraphicsResolutionMenu.init( %this.getText(), OptGraphicsFullscreenToggle.getValue() );
	OptGraphicsBPPMenu.init( %this.getText() );

	// Try to select the previous settings:
	%selId = OptGraphicsResolutionMenu.findText( %prevRes );
	if ( %selId == -1 )
		%selId = 0;
	OptGraphicsResolutionMenu.setSelected( %selId );

	if ( OptGraphicsFullscreenToggle.getValue() )
	{
		%selId = OptGraphicsBPPMenu.findText( %prevBPP );
		if ( %selId == -1 )
			%selId = 0;
		OptGraphicsBPPMenu.setSelected( %selId );
		OptGraphicsBPPMenu.setText( OptGraphicsBPPMenu.getTextById( %selId ) );
	}
	else
		OptGraphicsBPPMenu.setText( "Default" );

}

function OptGraphicsResolutionMenu::init( %this, %device, %fullScreen )
{
	%this.clear();
	%resList = getResolutionList( %device );
	%resCount = getFieldCount( %resList );
	%deskRes = getDesktopResolution();

   %count = 0;
	for ( %i = 0; %i < %resCount; %i++ )
	{
		%res = getWords( getField( %resList, %i ), 0, 1 );

		if ( !%fullScreen )
		{
			if ( firstWord( %res ) >= firstWord( %deskRes ) )
				continue;
			if ( getWord( %res, 1 ) >= getWord( %deskRes, 1 ) )
				continue;
		}

		// Only add to list if it isn't there already:
		if ( %this.findText( %res ) == -1 )
      {
			%this.add( %res, %count );
         %count++;
      }
	}
}

function OptGraphicsFullscreenToggle::onAction(%this)
{
   Parent::onAction();
   %prevRes = OptGraphicsResolutionMenu.getText();

   // Update the resolution menu with the new options
   OptGraphicsResolutionMenu.init( OptGraphicsDriverMenu.getText(), %this.getValue() );

   // Set it back to the previous resolution if the new mode supports it.
   %selId = OptGraphicsResolutionMenu.findText( %prevRes );
   if ( %selId == -1 )
   	%selId = 0;
 	OptGraphicsResolutionMenu.setSelected( %selId );
}


function OptGraphicsBPPMenu::init( %this, %device )
{
	%this.clear();

	if ( %device $= "Voodoo2" )
		%this.add( "16", 0 );
	else
	{
		%resList = getResolutionList( %device );
		%resCount = getFieldCount( %resList );
      %count = 0;
		for ( %i = 0; %i < %resCount; %i++ )
		{
			%bpp = getWord( getField( %resList, %i ), 2 );

			// Only add to list if it isn't there already:
			if ( %this.findText( %bpp ) == -1 )
         {
				%this.add( %bpp, %count );
            %count++;
         }
		}
	}
}

function optionsDlg::discardGraphics()
{
	$pref::Video::displayDevice = $OldConfig::Video::displayDevice;
	$pref::Video::resolution = $OldConfig::Video::resolution;
	$pref::Video::fullScreen = $OldConfig::Video::fullScreen;
}

function optionsDlg::applyOldGraphics( %this )
{
	pauseMusic();

	if ($OldConfig::Video::displayDevice !$= getDisplayDeviceName())
	{
		setDisplayDevice( $OldConfig::Video::displayDevice,
		            firstWord( $OldConfig::Video::resolution ),
		            getWord( $OldConfig::Video::resolution, 1 ),
		            getWord( $OldConfig::Video::resolution, 2),
		            $OldConfig::Video::fullScreen );
		//OptionsDlg::deviceDependent( %this );
	}
	else if($OldConfig::Video::resolution !$= getResolution())
   {
		setScreenMode( firstWord( $OldConfig::Video::resolution ),
		            getWord( $OldConfig::Video::resolution, 1 ),
		            getWord( $OldConfig::Video::resolution, 2),
		            $OldConfig::Video::fullScreen );
   }
   else if($OldConfig::Video::fullScreen != isFullScreen())
      toggleFullScreen();
   resumeMusic();
}

function Options_timeback()
{
	$OldConfig::Timer -= 1;
		echo("TIME BACK" @ $OldConfig::Timer);
	if($OldConfig::Timer == 0)
	{
		MessageCallback(MessageBoxYesNoDlg,MessageBoxYesNoDlg.noCallback);
		return;
	}

	MBSetText(MBYesNoText, MBYesNoFrame, "<spush><font:Comic Sans MS Bold:24><just:center>Keep Resolution?<spop>\n\nDo you want to keep this video mode?\n"@$OldConfig::Timer@" seconds remaining...");
	$Options_schedule = schedule(1000,0,"Options_timeback");
}


function optionsDlg::applyGraphics( %this )
{
//	%newDriver = OptGraphicsDriverMenu.getText();
//	%newRes = OptGraphicsResolutionMenu.getText();
//	%newBpp = OptGraphicsBPPMenu.getText();
//	%newFullScreen = OptGraphicsFullscreenToggle.getValue();

	// Spy47: If you didn't change anything, why would you want to apply, dumbass.
	if(!$OldConfig::isChanged)
		return;

    pauseMusic();

	if ($pref::Video::displayDevice !$= getDisplayDeviceName())
	{
		setDisplayDevice( $pref::Video::displayDevice,
		            firstWord( $pref::Video::resolution ),
		            getWord( $pref::Video::resolution, 1 ),
		            getWord( $pref::Video::resolution, 2),
		            $pref::Video::fullScreen );
		//OptionsDlg::deviceDependent( %this );
	}
	else if($pref::Video::resolution !$= getResolution())
   {
		setScreenMode( firstWord( $pref::Video::resolution ),
		            getWord( $pref::Video::resolution, 1 ),
		            getWord( $pref::Video::resolution, 2),
		            $pref::Video::fullScreen );
   }
   else if($pref::Video::fullScreen != isFullScreen())
      toggleFullScreen();
   resumeMusic();

	$OldConfig::Timer = 10;
	$Options_schedule = schedule(1000,0,"Options_timeback");
	MessageBoxYesNo("Keep Resolution?","Do you want to keep this video mode?\n"@$OldConfig::Timer@" seconds remaining...","cancel($Options_schedule);optionsDlg.onWake();","optionsDlg.applyOldGraphics();cancel($Options_schedule);optionsDlg.onWake();");
}



$RemapCount = 0;
$RemapName[$RemapCount] = "Move Forward";
$RemapCmd[$RemapCount] = "moveforward";
$RemapCount++;
$RemapName[$RemapCount] = "Move Backward";
$RemapCmd[$RemapCount] = "movebackward";
$RemapCount++;
$RemapName[$RemapCount] = "Move Left";
$RemapCmd[$RemapCount] = "moveleft";
$RemapCount++;
$RemapName[$RemapCount] = "Move Right";
$RemapCmd[$RemapCount] = "moveright";
$RemapCount++;
$RemapName[$RemapCount] = "Jump";
$RemapCmd[$RemapCount] = "jump";
$RemapCount++;
$RemapName[$RemapCount] = "Use PowerUp";
$RemapCmd[$RemapCount] = "mouseFire";
$RemapCount++;
$RemapName[$RemapCount] = "Rotate Camera Left";
$RemapCmd[$RemapCount] = "turnLeft";
$RemapCount++;
$RemapName[$RemapCount] = "Rotate Camera Right";
$RemapCmd[$RemapCount] = "turnRight";
$RemapCount++;
$RemapName[$RemapCount] = "Rotate Camera Up";
$RemapCmd[$RemapCount] = "panUp";
$RemapCount++;
$RemapName[$RemapCount] = "Rotate Camera Down";
$RemapCmd[$RemapCount] = "panDown";
$RemapCount++;
$RemapName[$RemapCount] = "Free Look";
$RemapCmd[$RemapCount] = "freelook";
$RemapCount++;
$RemapName[$RemapCount] = "Respawn Key";
$RemapCmd[$RemapCount] = "forceRespawn";
$RemapCount++;
$RemapName[$RemapCount] = "Use Blast";
$RemapCmd[$RemapCount] = "useblast";
$RemapCount++;
$RemapName[$RemapCount] = "Score List";
$RemapCmd[$RemapCount] = "displayScoreList";
$RemapCount++;
$RemapName[$RemapCount] = "Change Spectator Mode";
$RemapCmd[$RemapCount] = "toggleSpectateModeType";
$RemapCount++;
$RemapName[$RemapCount] = "Switch Radar Mode";
$RemapCmd[$RemapCount] = "radarSwitch";
$RemapCount++;
$RemapName[$RemapCount] = "Leaderboards Chat";
$RemapCmd[$RemapCount] = "toggleChatHUD";
$RemapCount++;
$RemapName[$RemapCount] = "Private Server Chat";
$RemapCmd[$RemapCount] = "togglePrivateChatHUD";
$RemapCount++;

// Jeff: Taunts (v1.50)
$RemapName[$RemapCount] = "Chicken Taunt";
$RemapCmd[$RemapCount] = "taunt1";
$RemapCount++;
$RemapName[$RemapCount] = "Confusion Taunt";
$RemapCmd[$RemapCount] = "taunt2";
$RemapCount++;
$RemapName[$RemapCount] = "Laughter Taunt";
$RemapCmd[$RemapCount] = "taunt3";
$RemapCount++;
$RemapName[$RemapCount] = "Loser Taunt";
$RemapCmd[$RemapCount] = "taunt4";
$RemapCount++;
$RemapName[$RemapCount] = "Mega Marble Taunt";
$RemapCmd[$RemapCount] = "taunt5";
$RemapCount++;
$RemapName[$RemapCount] = "Multiplayer WHERe Taunt";
$RemapCmd[$RemapCount] = "taunt6";
$RemapCount++;
$RemapName[$RemapCount] = "Come On Taunt";
$RemapCmd[$RemapCount] = "taunt7";
$RemapCount++;
$RemapName[$RemapCount] = "Pomp Taunt";
$RemapCmd[$RemapCount] = "taunt8";
$RemapCount++;
$RemapName[$RemapCount] = "PQ WHERe Taunt";
$RemapCmd[$RemapCount] = "taunt9";
$RemapCount++;
$RemapName[$RemapCount] = "RAISE UR DONGERS Taunt";
$RemapCmd[$RemapCount] = "taunt10";
$RemapCount++;
$RemapName[$RemapCount] = "You Got Owned Taunt";
$RemapCmd[$RemapCount] = "taunt11";
$RemapCount++;


function restoreDefaultMappings()
{
   moveMap.delete();
   exec( "~/client/scripts/default.bind.cs" );
   OptRemapList.fillList();
}

function getMapDisplayName( %device, %action, %fullText )
{
	if ( %device $= "keyboard" )
   {
	   if(%action $= "space")
         return "Space Bar";
		return( upperFirst(%action) );
   }
	else if ( strstr( %device, "mouse" ) != -1 )
	{
		// Substitute "mouse" for "button" in the action string:
		%pos = strstr( %action, "button" );
		if ( %pos != -1 )
		{
			%mods = getSubStr( %action, 0, %pos );
			%object = getSubStr( %action, %pos, 1000 );
			%instance = getSubStr( %object, strlen( "button" ), 1000 );
			if(%fullText)
         {
            if(%instance < 2)
            {
   		      if(%mods $= "")
                  %mods = "the ";
   		      if($platform $= "macos" && %instance == 0)
                  return %mods @ "Mouse Button";
               if(%instance == 0)
                  return %mods @ "Left Mouse Button";
               return %mods @ "Right Mouse Button";
            }
            else
      			return( %mods @ "Mouse Button " @ ( %instance + 1 ) );

	      }
	      else
         {
            if(%instance < 2)
            {
               if($platform $= "macos" && %instance == 0)
                  return %mods @ "Mouse Button";
               if(%instance == 0)
                  return %mods @ "Left Mouse";
               return %mods @ "Right Mouse";
            }
   	   	else
   	   	   return( %mods @ "Mouse Btn. " @ ( %instance + 1 ) );
         }
		}
		else
			error( "Mouse input object other than button passed to getDisplayMapName!" );
	}
	else if ( strstr( %device, "joystick" ) != -1 )
	{
		// Substitute "joystick" for "button" in the action string:
		%pos = strstr( %action, "button" );
		if ( %pos != -1 )
		{
			%mods = getSubStr( %action, 0, %pos );
			%object = getSubStr( %action, %pos, 1000 );
			%instance = getSubStr( %object, strlen( "button" ), 1000 );
			return( %mods @ "Joystick" @ ( %instance + 1 ) );
		}
		else
	   {
	      %pos = strstr( %action, "pov" );
         if ( %pos != -1 )
         {
            %wordCount = getWordCount( %action );
            %mods = %wordCount > 1 ? getWords( %action, 0, %wordCount - 2 ) @ " " : "";
            %object = getWord( %action, %wordCount - 1 );
            switch$ ( %object )
            {
               case "upov":   %object = "POV1 up";
               case "dpov":   %object = "POV1 down";
               case "lpov":   %object = "POV1 left";
               case "rpov":   %object = "POV1 right";
               case "upov2":  %object = "POV2 up";
               case "dpov2":  %object = "POV2 down";
               case "lpov2":  %object = "POV2 left";
               case "rpov2":  %object = "POV2 right";
               default:       %object = "??";
            }
            return( %mods @ %object );
         }
         else
            error( "Unsupported Joystick input object passed to getDisplayMapName!" );
      }
	}

	return( "??" );
}

function buildFullMapString( %index )
{
   %name       = $RemapName[%index];
   %cmd        = $RemapCmd[%index];

	%temp = moveMap.getBinding( %cmd );
   %device = getField( %temp, 0 );
   %object = getField( %temp, 1 );
   if ( %device !$= "" && %object !$= "" )
	   %mapString = getMapDisplayName( %device, %object );
   else
      %mapString = "";

	return( %cmd TAB %mapString );
}

function OptRemapList::fillList( %this )
{
}

//------------------------------------------------------------------------------

function OptControlsPane::showMappings()
{
   for ( %i = 0; %i < $RemapCount; %i++ )
   {
      %str = buildFullMapString(%i);
      %ctrl = nameToId("remap_" @ getField(%str, 0));
      %ctrl.setText(getField(%str, 1));
   }
}

function OptControlsPane::remap(%this, %ctrl, %name )
{
   OptRemapText.setText( "<just:center><font:DomCasualD:24>Press a new key or button for \"" @ %name @ "\"" );
   OptRemapInputCtrl.ctrl = %ctrl;
   OptRemapInputCtrl.nameText = %name;
	Canvas.pushDialog( RemapDlg );
}

//------------------------------------------------------------------------------
function redoMapping( %device, %action, %cmd)
{
	//%actionMap.bind( %device, %action, $RemapCmd[%newIndex] );
	moveMap.bind( %device, %action, %cmd );
   OptControlsPane.showMappings();
}

//------------------------------------------------------------------------------
function findRemapCmdIndex( %command )
{
	for ( %i = 0; %i < $RemapCount; %i++ )
	{
		if ( %command $= $RemapCmd[%i] )
			return( %i );
	}
	return( -1 );
}

function OptRemapInputCtrl::onInputEvent( %this, %device, %action )
{
	//error( "** onInputEvent called - device = " @ %device @ ", action = " @ %action @ " **" );
	Canvas.popDialog( RemapDlg );

	// Test for the reserved keystrokes:
	if ( %device $= "keyboard" )
	{
      // Cancel...
      if ( %action $= "escape" )
      {
         // Do nothing...
		   return;
      }
	}
	if(%action $= "")
      return;

   %cmd  = %this.ctrl;
   %name = %this.nameText;

	// First check to see if the given action is already mapped:
	%prevMap = moveMap.getCommand( %device, %action );
   if ( %prevMap !$= %cmd )
   {
	   if ( %prevMap $= "" )
	   {
         moveMap.bind( %device, %action, %cmd );
		   OptControlsPane.showMappings();
	   }
	   else
	   {
         %mapName = getMapDisplayName( %device, %action );
		   %prevMapIndex = findRemapCmdIndex( %prevMap );
		   if ( %prevMapIndex == -1 )
			   MessageBoxOK( "REMAP FAILED", "\"" @ %mapName @ "\" is already bound to a non-remappable command!", "", true );
		   else
         {
            %prevCmdName = $RemapName[%prevMapIndex];
			   MessageBoxYesNo( "WARNING",
				   "\"" @ %mapName @ "\" is already bound to \""
					   @ %prevCmdName @ "\"!\nDo you want to undo this mapping?",
				   "redoMapping(" @ %device @ ", \"" @ %action @ "\", \"" @ %cmd @ "\");", "" );
         }
		   return;
	   }
   }
}

// Audio
function OptAudioUpdate()
{
   // set the driver text
   %text =   "Vendor: " @ alGetString("AL_VENDOR") @
           "\nVersion: " @ alGetString("AL_VERSION") @
           "\nRenderer: " @ alGetString("AL_RENDERER");

   // don't display the extensions on linux.  there's too many of them and
   // they mess up the control
   if ($platform $= "x86UNIX")
      %text = %text @ "\nExtensions: (See Console) ";
   else
      %text = %text @ "\nExtensions: " @ alGetString("AL_EXTENSIONS");
   OptAudioInfo.setText(%text);
}


// Channel 0 is unused in-game, but is used here to test master volume.

new AudioDescription(AudioChannel0)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 0;
};

new AudioDescription(AudioChannel1)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 1;
};

new AudioDescription(AudioChannel2)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 2;
};

new AudioDescription(AudioChannel3)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 3;
};

new AudioDescription(AudioChannel4)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 4;
};

new AudioDescription(AudioChannel5)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 5;
};

new AudioDescription(AudioChannel6)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 6;
};

new AudioDescription(AudioChannel7)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 7;
};

new AudioDescription(AudioChannel8)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = 8;
};

$AudioTestHandle = 0;

function OptAudioUpdateMasterVolume(%volume)
{
   if (%volume == $pref::Audio::masterVolume)
      return;
   alxListenerf(AL_GAIN_LINEAR, %volume);
   $pref::Audio::masterVolume = %volume;
   if (!alxIsPlaying($AudioTestHandle))
   {
      $AudioTestHandle = alxCreateSource("AudioChannel0", expandFilename("~/data/sound/testing.wav"));
      alxPlay($AudioTestHandle);
   }
}

function OptAudioUpdateChannelVolume(%channel)
{
   if (%channel < 1 || %channel > 8)
      return;

   alxSetChannelVolume(%channel, $pref::Audio::channelVolume[%channel]);
   if (!alxIsPlaying($AudioTestHandle) && %channel == 1)
   {
      $AudioTestHandle = alxCreateSource("AudioChannel"@%channel, expandFilename("~/data/sound/testing.wav"));
      alxPlay($AudioTestHandle);
   }
}


function OptAudioDriverList::onSelect( %this, %id, %text )
{
   if (%text $= "")
      return;

   if ($pref::Audio::driver $= %text)
      return;

   $pref::Audio::driver = %text;
   OpenALInit();
}

