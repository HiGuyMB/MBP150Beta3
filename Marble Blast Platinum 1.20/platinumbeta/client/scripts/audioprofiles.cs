//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

// Channel assignments (channel 0 is unused in-game).


// UNCOMMENT ONCE SOUNDS ARE IMPLEMENTED IN CODE
$GameAudioType = 0; //HiGuy: Fuck dedicated servers
$AchievementAudioType  = 1;
$GuiAudioType     = 1;
$SimAudioType     = 1;
$MessageAudioType = 1;
$EffectAudioType = 1;
$MusicAudioType = 2;

new AudioDescription(AudioGui)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = $GuiAudioType;
};

new AudioDescription(LoopingAudioGui)
{
   volume   = 1.0;
   isLooping= true;
   is3D     = false;
   type     = $GuiAudioType;
};
new AudioDescription(AudioChatGui)
{
   volume   = 0.8;
   isLooping= false;
   is3D     = false;
   type  = $GuiAudioType;
};
new AudioDescription(AudioAchievementGui)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = $AchievementAudioType;
};

new AudioDescription(AudioMessage)
{
   volume   = 1.0;
   isLooping= false;
   is3D     = false;
   type     = $MessageAudioType;
};

new AudioDescription(ClientAudioLooping2D)
{
   volume = 1.0;
   isLooping = true;
   is3D = false;
   type = $EffectAudioType;
};

new AudioProfile(GetAchievement) {
   fileName = "~/data/sound/getachievement.wav";
   description = AudioAchievementGui;
   preload = true;
};

new AudioProfile(LB_Recieve) {
   fileName = "~/data/sound/lb_recieve.wav";
   description = AudioChatGui;
   preload = true;
};

new AudioProfile(LB_Nudge) {
   fileName = "~/data/sound/lb_nudge.wav";
   description = AudioChatGui;
   preload = true;
};

new AudioProfile(LB_Logout)
{
   filename = "~/data/sound/lb_signout.wav";
   description = "AudioGui";
	preload = true;
};

new AudioProfile(LB_Login)
{
   filename = "~/data/sound/lb_signin.wav";
   description = "AudioGui";
	preload = true;
};

new AudioProfile(TimeTravelLoopSfx)
{
   filename    = "~/data/sound/TimeTravelActive.wav";
   description = ClientAudioLooping2d;
   preload = true;
};

new AudioProfile(AudioButtonOver)
{
   filename = "~/data/sound/buttonOver.wav";
   description = "AudioGui";
	preload = true;
};

new AudioProfile(AudioButtonDown)
{
   filename = "~/data/sound/ButtonPress.wav";
   description = "AudioGui";
	preload = true;
};

new AudioProfile(TimerAlarm)
{
   filename = "~/data/sound/alarm.wav";
   description = "LoopingAudioGui";
	preload = true;
};

new AudioProfile(TimerFailed)
{
   filename = "~/data/sound/alarm_timeout.wav";
   description = "AudioGui";
	preload = true;
};

new AudioProfile(LBError) {
   filename = "~/data/sound/lb_error.wav";
   description = "AudioGui";
   preload = true;
};

new AudioProfile(LBNope) {
   filename = "~/data/sound/lb_nopeville.wav";
   description = "AudioGui";
   preload = true;
};


new AudioDescription(AudioMusic)
{
   volume   = 1.0;
   isLooping = true;
   isStreaming = true;
   is3D     = false;
   type     = $MusicAudioType;
};

function playMusic(%musicFileBase)
{
   alxStop($currentMusicHandle);
   if(isObject(MusicProfile))
      MusicProfile.delete();

   %file = "~/data/sound/music/" @ %musicFileBase;
   new AudioProfile(MusicProfile) {
      fileName = %file;
      description = "AudioMusic";
      preload = false;
   };
   $currentMusicBase = %musicFileBase;
   $currentMusicHandle = alxPlay(MusicProfile);  //add this line

   JukeboxDlg::selectPlayingSong();
   $JukeboxDlg::isPlaying = true;
}

function playShellMusic()
{
   if ($currentMusicBase !$= "Pianoforte.ogg" || !alxIsPlaying($currentMusicHandle))
      playMusic("Pianoforte.ogg");
}

function playManualMusic()
{
   if ($currentMusicBase !$= "Quiet Lab.ogg" || !alxIsPlaying($currentMusicHandle))
	playMusic("Quiet Lab.ogg");
}

function playGameMusic()
{
	if(MissionInfo.music !$= "" && MissionInfo.music !$= "Pianoforte.ogg")
	{
		echo("Playing " @ MissionInfo.music);
		playMusic(MissionInfo.music);
		return;
	}

   if(!$musicFound)
   {
      $NumMusicFiles = 0;
      for(%file = findFirstFile($usermods @ "/data/sound/music/*.ogg"); %file !$= ""; %file = findNextFile($usermods @ "/data/sound/music/*.ogg"))
      {
         if(fileBase(%file) !$= "Pianoforte" && fileBase(%file) !$= "Comforting Mystery" && fileBase(%file) !$= "Quiet Lab")
         {
            $Music[$NumMusicFiles] = fileBase(%file) @ ".ogg";
            $NumMusicFiles++;
         }
      }
      $musicFound = true;
   }
   if($NumMusicFiles)
      playMusic($Music[MissionInfo.level % $NumMusicFiles]);
   else
      playMusic("Pianoforte.ogg");
}

function playLBMusic() {
   if ($currentMusicBase !$= "Comforting Mystery.ogg" || !alxIsPlaying($currentMusicHandle))
      playMusic("Comforting Mystery.ogg");
}

function pauseMusic()
{
   alxStop($currentMusicHandle);
   $JukeboxDlg::isPlaying = false;
}

function resumeMusic()
{
   playMusic($currentMusicBase);
   $JukeboxDlg::isPlaying = true;
}

