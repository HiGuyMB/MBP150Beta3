// Cando: The Ragedar is normally used only for Hunt levels, but let's expand it to *any* level unless we make a enableRadar = "1"; field.

// TODO: Allow Radar Distance to be modified in optionsdlg

//stuff todo:
//[10:51:04 PM] Matan Weissman: 1) show start/end pads, gems, tts only
//[10:51:07 PM] Matan Weissman: 2) hunt: show normally
//[10:51:24 PM] Matan Weissman: 3) gem madness: limit to 10 nearest gems, doesn't matter distance
//[10:51:36 PM] Matan Weissman: OR limit to 10-15 gems within radius X
//[10:51:40 PM] Matan Weissman: i think the former is easier
//[10:51:49 PM] Matan Weissman: and thats all we need
//[10:51:58 PM] Matan Weissman: we dont need coins/eggs/powerups/chkpt etc.

//todo: mission-specific radar distances
$Game::RMD = 35; //Radar minimum search distance in dynamic mode
$Game::RMDL = 70; //(large) Radar minimum search distance in dynamic mode
$Game::RD = 50;  //Radar search distance
//todo: discuss about radar prefs (maxdots, timemult, maybe others, like distances)
$Radar::TimeMultiplier = 2; //Multiply schedule time by this amount.  For example, 2 = only calculate (close to) every other frame
$Radar::Fast = 1;  //Radar will not do an expensive check for 100% accurate camera yaw
$Radar::MaxDots = 25; //To stop being agitated by my dots

function Radar::onMissionLoaded() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	// Stop radar if it's running, and disable it
	// TODO: Enable it based on custom rules (gamemode/level's MissionInfo)
	Radar::Stop();
	RadarToggle(1);

	// Radar distance code that should be changed.
	$Game::RD = MissionInfo.RadarDistance !$= "" ? MissionInfo.RadarDistance : 50;
}

function Radar::onMissionReset() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	if ($MPPref::RadarZ $= "")
		$MPPref::RadarZ = 1;
	Radar::Init();
}

function Radar::onMissionEnded() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	Radar::ClearTargets();
}

function Radar::Stop(%val) {
	$Radar::PreviousMode = %val ? $Game::RadarMode : "";
	cancel($Radar::Schedule);
}

function Radar::Init() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode) {
      Radar::ClearTargets();
      return;
   }

	if (!isObject(TargetGroup)) {
		new SimGroup(TargetGroup);
		($Server::Hosting && !$Server::_Dedicated ? MissionCleanup : ServerConnection).add(TargetGroup);
	}
	Radar::ClearTargets();
	echo($Radar::PreviousMode);
	if ($Radar::PreviousMode !$= "")
		// Simulate a toggle cycle
		for (%i = 0; %i <= $Radar::PreviousMode; %i++)
			schedule(250 * %i, 0, "RadarToggle", 0, %i);

	RadarBuildSearch();
	RadarLoop();
}

function clientCmdRadarBuildSearch() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode) {
      Radar::ClearTargets();
      return;
   }

   RadarBuildSearch();
   if (!$Server::Hosting || $Server::_Dedicated) {
      schedule(ServerConnection.getPing(), 0, RadarBuildSearch);
      schedule(100, 0, RadarBuildSearch);
      schedule(300, 0, RadarBuildSearch);
      schedule(600, 0, RadarBuildSearch);
   }
}

function clientCmdRadarStart() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode) {
      Radar::ClearTargets();
      return;
   }

   //HiGuy: We init the radar here, because it's client-sided and on the PlayGui
   Radar::init();

   //HiGuy: Defaults to off mode
   RadarToggle(true);

   //HiGuy: If you forget to call RadarLoop, it never starts up!
   RadarLoop();
}

function RadarBuildSearch() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;

   cancel($RadarBuild);
	if ($Game::RadarMode == 0)
	   return;

   if (!isObject(TargetGroup))
      return;

   Radar::ClearTargets();

   //HiGuy: Flush out old objects that may not exist anymore
   for (%i = 0; %i < TargetGroup.getCount(); %i ++) {
      %script = TargetGroup.getObject(%i);
      if (!isObject(%script.obj) || %script.obj.isHidden()) {
         if (isObject(%script.dot))
            %script.dot.delete();
         if (isObject(%script)) {
            %script.delete();
            %i --;
         }
      }
   }
	for (%i = 0; %i < PlayGui.getCount(); %i ++) {
		%obj = PlayGui.getObject(%i);
		if (strPos(%obj.getName(), "RadarDot") == 0) {
         if (!isObject(%obj.object) || %obj.object.isHidden()) {
            %obj.delete();
            %i --;
         }
		}
	}

   %search = ($Server::Hosting && !$Server::_Dedicated ? SpawnedSet : ServerConnection);

   if (isObject(%search)) {
      //HiGuy: Add in new objects in case they appear
      %count = %search.getCount();

      %gems = 0;

      for (%i = 0; %i < %count; %i ++) {
         %obj = %search.getObject(%i);

         if (!%obj)
            continue;

         if (%obj.getClassName() !$= "Item")
            continue;

         for (%j = 0; %j < TargetGroup.getCount(); %j ++)
            if (TargetGroup.getObject(%j).obj == %obj) {
               %cont = true;
               break;
            }

         if (%cont)
            continue;

         %db = %obj.getDatablock();
         if (strPos(%db.shapeFile, "gem") != -1) {
            %gems ++;
            if (%gems > $Radar::MaxDots)
               break;

            %obj.setRadarTarget();
         }
      }
   }
   $RadarBuild = schedule(1000, 0, "RadarBuildSearch");

   RadarLoop();
}

function Radar::AddTarget(%object, %bitmap) {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	%script = new ScriptObject("RadarTarget" @ %object.getID()) {
		obj = %object;
		dot = Radar::AddDot(%object, %bitmap);
	};
	TargetGroup.add(%script);
}

function Radar::RemoveTarget(%object) {
   if (isObject(%object))
   	%script = "RadarTarget" @ %object.getID();
   else
      %script = "RadarTarget" @ %object;
	if (isObject(%script)) {
		%script.dot.delete();
		%script.delete();
	}
   if (isObject(%object))
   	%dot = "RadarDot" @ %object.getID();
   else
      %dot = "RadarDot" @ %object;
   while (isObject(%dot))
      %dot.delete();
}

function Radar::ClearTargets() {
   if (isObject(TargetGroup)) {
      while (TargetGroup.getCount() > 0) {
         %script = TargetGroup.getObject(0);
         if (isObject(%script.dot))
	         %script.dot.delete();
         %script.delete();
      }
	}
	for (%i = 0; %i < PlayGui.getCount(); %i ++) {
		%obj = PlayGui.getObject(%i);
		if (strPos(%obj.getName(), "RadarDot") == 0) {
         %obj.delete();
         %i --;
      } else if (strPos(%obj.getName(), "RadarTarget") == 0) {
         %obj.delete();
         %i --;
      }
	}

	$Radar::NumDots = 0;
}

function RadarLoop() {
	cancel($Radar::Schedule);

   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer")
      return;
	if (!isObject($MP::MyMarble))
		return;
	if ($Game::RadarMode == 0)
	   return;
	if ($SpectateMode) {
	   RadarToggle(true, true);
	   return;
	}

	%time = GetRealTime();

	%mpos = $MP::MyMarble.getPosition();

	// Default "down" gravity; run expensive calculations only when needed
	if ($Game::GravityUV !$= "0 0 -1") {
		//%calcRot = true;
	}

	// Hunt and no custom rule: check VisibleGems
   %count = %scripts = TargetGroup.getCount();
   for (%i = 0; %i < %count; %i++) {
      %script[%i] = TargetGroup.getObject(%i);
      %hit[%i] = TargetGroup.getObject(%i).obj;
   }
   Radar::ShowDots(0);

	// Adjust radar range for Hunt
   %maxdistance = 0;
   for (%i = 0; %i < %count; %i++) {
      %obj = %hit[%i];
      if (!isObject(%obj) || %obj.isHidden()) {
         Radar::RemoveTarget(%obj);
         continue;
      }

      %tpos = %obj.getPosition();
      if (%calcRot) { // Run expensive calculations only when needed
         %dist = VectorDist(%mpos, %tpos);

         // Normalize
         %uvec = VectorNormalize(vectorSub(%mpos, %tpos));

         // Rotate by negative direction
         %newvec = rotQToVector($GRotI, %uvec);

         // Scale up again
         %newvec = vectorScale(%newvec, %dist);
         %newvec = vectorAdd(%mpos, %newvec);

         %dist = getThatDistance(%mpos, %newvec, 1);
      } else
         %dist = getThatDistance(%mpos, %tpos, 1);
      if (%dist > %maxdistance)
         %maxdistance = %dist;
   }

   %min = $Game::RadarMode == 1 ? $Game::RMD : $Game::RMDL;

   if (%maxdistance != $Game::RD && %maxdistance >= %min)
      $Game::RD = %maxdistance;
   else if (%maxdistance != $Game::RD)
      $Game::RD = %min;

	%mx = getWord(%mpos, 0);
	%my = getWord(%mpos, 1);
	%mz = getWord(%mpos, 2); //Z changes dot size if $MPPref::RadarZ == 1

	%minsize = 10;
	%maxsize = 22;

	%rdata = Radar::GetData(); //radar data:
	%cx = getWord(%rdata, 0); //centre x
	%cy = getWord(%rdata, 1); //centre y
	%radx = getWord(%rdata, 2); //radius x
	%rady = getWord(%rdata, 3); //radius y

	for (%i = 0; %i < %count; %i++) {
      %obj = %script[%i].obj;
      if (!isObject(%obj) || %obj.isHidden()) {
         Radar::RemoveTarget(%obj);
         continue;
      }

		%tpos = %obj.getPosition();

		if (%calcRot) {
			//find distance
			%dist = VectorDist(%tpos, %mpos);

			//normalize
			%uvec = VectorNormalize(vectorSub(%mpos, %tpos));
			%uvec = vectorScale(%uvec, -1);

			//rotate by negative direction
			%newvec = rotQToVector($GRotI, %uvec);

			//scale up again
			%newvec = VectorScale(%newvec, %dist);
			%newvec = vectoradd(%mpos, %newvec);

			%tx = getWord(%newvec, 0);
			%ty = getWord(%newvec, 1);
			%tz = getWord(%newvec, 2);

			%dx = %tx - %mx;
			%dy = %ty - %my;
			%dz = %tz - %mz;
			%dy *= -1;
		} else {
			%tx = getWord(%tpos, 0);
			%ty = getWord(%tpos, 1);
			%tz = getWord(%tpos, 2);

			%dx = %tx - %mx;
			%dy = %ty - %my;
			%dz = %tz - %mz;
		}

		//Initial positions
		%x = (%dx * 2) * (%radx / $Game::RD * 0.5);
		%y = (%dy * 2) * (%rady / $Game::RD * 0.5);

      %newangle = $Marbleyaw;
      // WHA-BAM!
//      if (!%calcRot && $Radar::Fast)
//        %newangle = $MarbleYaw;
//      else
//        %newangle = getMarbleCamYaw();

		// Sizing based on Z location
		if (!$MPPref::RadarZ) {
			// Change x y positions based on newangle
			%nx = mFloor((%y * mCos(%newangle) + %x * mSin(%newangle)) + %cx) - 8;
			%ny = mFloor((%y * -1 * mSin(%newangle) + %x * mCos(%newangle)) + %cy) - 8;
			%exy = 16; //EXY: extent XY, default 16
		} else {
			//cando: scaling might be inverted on gravity dirs
			//%step = 8;
			//adjusts by 1 pixel per %step units starting at +/- (%step/2) from current position (I think)
			%exy = mFloor(((%dz + 4) / 8) * 2) + 16;
			if (%exy < %minsize)
				%exy = %minsize;
			else if (%exy > %maxsize)
				%exy = %maxsize;

			%nx = mFloor((%y * mCos(%newangle) + %x * mSin(%newangle)) + %cx) - (%exy / 2);
			%ny = mFloor((%y * -1 * mSin(%newangle) + %x * mCos(%newangle)) + %cy) - (%exy / 2);
		}

		if (%i < %scripts) {
			%dot = %script[%i].dot;

			// Set dot position and size
			%dot.resize(%nx, %ny, %exy, %exy);
			%dot.setVisible(1);
		}
	}

   $Radar::Schedule = schedule(1000 / $fps::real, 0, "RadarLoop");
}

function radarSwitch(%val) {
   if (!%val)
      return;
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
   RadarToggle($Game::RadarMode == 2);
}

function RadarToggle(%off, %forceMode) {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode) {
      RadarBitmap.setVisible(0);
      $Game::RadarMode = 0;
      return;
   }
	if (%forceMode $= "") {
		if (%off)
			$Game::RadarMode = 0;
		else {
			$Game::RadarMode++;

			if ($Game::RadarMode > 2)
				$Game::RadarMode = 0;
		}
	} else
		$Game::RadarMode = %forceMode;

	%x = getWord(Canvas.getExtent(), 0);
	%y = getWord(Canvas.getExtent(), 1);

	switch ($Game::RadarMode) {
	case 0:
		Radar::Stop();
		Radar::ShowDots(0);
		RadarBitmap.setVisible(0);
	case 1:
		RadarLoop();
		RadarBitmap.resize(%x - 330, 10, 256, 256);
		RadarBitmap.setVisible(1);
	case 2:
		RadarBitmap.resize((%x/2) - %y/2, (%y/2) - %y/2, %y, %y);
		RadarBitmap.setVisible(0);
	}
	//Update positions
	RadarLoop();
}

function Radar::AddDot(%object, %bitmap) {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	if (%bitmap $= "")
		%bitmap = $usermods @ "/leaderboards/mp/radar/GemItem" @ %object.getSkinName() @ ".png";

	%dot = new GuiBitmapCtrl("RadarDot" @ %object.getID()) {
		profile = "GuiDefaultProfile";
		horizSizing = "left";
		vertSizing = "bottom";
		extent = "16 16";
		minExtent = "8 8";
		visible = "1";
		bitmap = %bitmap;
		object = %object.getId();
	};

	PlayGui.add(%dot);
	return %dot;
}

function Radar::GetData() {
   if ($MPPref::DisableRadar || $Server::ServerType $= "SinglePlayer" || $SpectateMode)
      return;
	%pos = RadarBitmap.getPosition();
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);

	%extent = RadarBitmap.getExtent();
	%xe = getWord(%extent, 0);
	%ye = getWord(%extent, 1);

	//           x centre point             y centre point             x radius            y radius
	%radardata = mFloor(%x + (%xe / 2)) SPC mFloor(%y + (%ye / 2)) SPC mFloor(%xe / 2) SPC mFloor(%ye / 2);
	return %radardata;
}

function Radar::ShowDots(%val) {
	for (%i = 0; %i < $Radar::NumDots; %i++) {
		%obj = "RadarDot" @ %i;
		%obj.setVisible(%val);
	}

	if (!isObject(TargetGroup))
		return;
	%count = TargetGroup.getCount();
	for (%i = 0; %i < %count; %i++) {
		%obj = TargetGroup.getObject(%i).dot;
		%obj.setVisible(%val);
	}
}
