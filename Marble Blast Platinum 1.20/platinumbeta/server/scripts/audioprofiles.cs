//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// 3D Sounds
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Single shot sounds

datablock AudioDescription(AudioDefault3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   ReferenceDistance= 20.0;
   MaxDistance= 100.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioClose3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   ReferenceDistance= 10.0;
   MaxDistance= 60.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioClosest3d)
{
   volume   = 1.0;
   isLooping= false;

   is3D     = true;
   ReferenceDistance= 5.0;
   MaxDistance= 30.0;
   type     = $EffectAudioType;
};


//-----------------------------------------------------------------------------
// Looping sounds

datablock AudioDescription(AudioDefaultLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   ReferenceDistance= 20.0;
   MaxDistance= 100.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioCloseLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   ReferenceDistance= 10.0;
   MaxDistance= 50.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(AudioClosestLooping3d)
{
   volume   = 1.0;
   isLooping= true;

   is3D     = true;
   ReferenceDistance= 5.0;
   MaxDistance= 30.0;
   type     = $EffectAudioType;
};

datablock AudioDescription(Quieter3D)
{
   volume   = 0.40;
   isLooping= false;

   is3D     = true;
   ReferenceDistance= 20.0;
   MaxDistance= 100.0;
   type     = $EffectAudioType;
};

//-----------------------------------------------------------------------------
// 2d sounds
//-----------------------------------------------------------------------------

// Used for non-looping environmental sounds (like power on, power off)
datablock AudioDescription(Audio2D)
{
   volume = 1.0;
   isLooping = false;
   is3D = false;
   type = $EffectAudioType;
};

// Used for Looping Environmental Sounds
datablock AudioDescription(AudioLooping2D)
{
   volume = 1.0;
   isLooping = true;
   is3D = false;
   type = $EffectAudioType;
};


//-----------------------------------------------------------------------------
// Ready - Set - Get Rolling

datablock AudioProfile(spawnSfx)
{
   filename    = "~/data/sound/spawn.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(pickupSfx)
{
   filename    = "~/data/sound/pickup.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(HelpDingSfx)
{
   filename    = "~/data/sound/InfoTutorial.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(ReadyVoiceSfx)
{
   filename    = "~/data/sound/ready.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(SetVoiceSfx)
{
   filename    = "~/data/sound/set.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(GetRollingVoiceSfx)
{
   filename    = "~/data/sound/go.wav";
   description = AudioDefault3d;
   preload = true;
};

if ($platform $= "x86UNIX")
{
    // fireworks are a bit too piercing with linux openal
    datablock AudioProfile(WonRaceSfx)
    {
		 filename    = "~/data/sound/firewrks.wav";
		 description = Quieter3D;
		 preload = true;
    };
}
else
{
    datablock AudioProfile(WonRaceSfx)
    {
		 filename    = "~/data/sound/firewrks.wav";
		 description = AudioDefault3d;
		 preload = true;
    };
}

datablock AudioProfile(MissingGemsSfx)
{
   filename    = "~/data/sound/missingGems.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(jumpSfx)
{
   filename    = "~/data/sound/bounce.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(bounceSfx)
{
   filename    = "~/data/sound/bounce.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(blastSfx)
{
   filename    = "~/data/sound/blast.wav";
   description = AudioDefault3d;
   preload = true;
};



//-----------------------------------------------------------------------------
// Misc

datablock AudioProfile(PenaltyVoiceSfx)
{
   filename    = "~/data/sound/penalty.wav";
   description = AudioDefault3d;
   preload = true;
};

datablock AudioProfile(DestroyedVoiceSfx)
{
   filename    = "~/data/sound/destroyedVoice.wav";
   description = AudioDefault3d;
   preload = true;
};

function loadAudioPack(%packname) {
   %apk = $userMods @ "/data/sound/ap_" @ %packname @ "/" @ %packname @ ".apk";
   exec(%apk);
   if (!isObject(AudioInfo)) {
      MessageBoxOk("Audio Pack Error!", "Could not load the audio pack \"" @ %packname @ "\"!", "", true);
      return;
   }
   %pack = AudioInfo.getId();
   %pack.setName(""); //HiGuy: Free up the name

   warn("Executed Audio Pack" SPC %pack.name SPC "by" SPC %pack.author @ "...");
   $Audio::CurrentAudioPack = %packname;

   warn("Resetting Audio Pack...");
   audioPackReset(RootGroup);

   warn("Loading Audio Pack...");
   audioPackIterate(RootGroup, %pack);
}

function audioPackReset(%grp) {
   for (%i = 0; %i < %grp.getCount(); %i ++) {
      %obj = %grp.getObject(%i);
      $con::logBufferEnabled = false;
      %class = %obj.getClassName();
      $con::logBufferEnabled = true;
      switch$ (%class) {
      case "SimGroup":
         audioPackReset(%obj, %pack);
      case "AudioProfile":
         %filename = %obj.filename;
         %base = fileBase(%filename);
         %newfilename = $userMods @ "/data/sound/" @ %base @ ".wav";
         if (%filename !$= %newfilename)
            echo("Substituting modified file" SPC %filename SPC "for original audio file" SPC %newfilename);
         %obj.filename = %newfilename;
      default:
         continue;
      }
   }
}

function audioPackIterate(%grp, %pack) {
   for (%i = 0; %i < %grp.getCount(); %i ++) {
      %obj = %grp.getObject(%i);
      $con::logBufferEnabled = false;
      %class = %obj.getClassName();
      $con::logBufferEnabled = true;
      switch$ (%class) {
      case "SimGroup":
         audioPackIterate(%obj, %pack);
      case "AudioProfile":
         %filename = %obj.filename;
         %base = fileBase(%filename);
         if (%pack.include[%base]) {
            %newfilename = $userMods @ "/data/sound/ap_" @ %pack.identifier @ "/" @ %base @ ".wav";
            echo("Substituting original file" SPC %filename SPC "for new audio file" SPC %newfilename);
            if (isFile(%newfilename))
               %obj.filename = %newfilename;
            else
               error("Could not find file" SPC %newfilename);
         }
      default:
         continue;
      }
   }
}
