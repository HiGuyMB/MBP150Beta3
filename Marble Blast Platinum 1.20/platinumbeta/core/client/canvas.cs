//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Function to construct and initialize the default canvas window
// used by the games

function initCanvas(%windowName)
{
   videoSetGammaCorrection($pref::OpenGL::gammaCorrection);
   if (!createCanvas(%windowName)) {
      quit();
      return;
   }

   setOpenGLTextureCompressionHint( $pref::OpenGL::compressionHint );
   setOpenGLAnisotropy( $pref::OpenGL::anisotropy );
   setOpenGLMipReduction( $pref::OpenGL::mipReduction );
   setOpenGLInteriorMipReduction( $pref::OpenGL::interiorMipReduction );
   setOpenGLSkyMipReduction( $pref::OpenGL::skyMipReduction );

   // Declare default GUI Profiles.
   exec("~/core/ui/defaultProfiles.cs");

   // Common GUI's
   exec("~/core/ui/GuiEditorGui.gui");
   exec("~/core/ui/ConsoleDlg.gui");
   exec("~/core/ui/InspectDlg.gui");
   exec("~/core/ui/LoadFileDlg.gui");
   exec("~/core/ui/SaveFileDlg.gui");
   exec("~/core/ui/MessageBoxOKDlg.gui");
   exec("~/core/ui/MessageBoxYesNoDlg.gui");
   exec("~/core/ui/MessageBoxOKCancelDlg.gui");
   exec("~/core/ui/MessagePopupDlg.gui");
   exec("~/core/ui/HelpDlg.gui");

   // 100% more PQ
   exec("~/core/ui/misc.cs");

   // Commonly used helper scripts
   exec("./metrics.cs");
   exec("./messageBox.cs");
   exec("./screenshot.cs");
   exec("./cursor.cs");
   exec("./help.cs");

   // Init the audio system
   OpenALInit();
}

function resetCanvas()
{
   if (isObject(Canvas))
   {
      Canvas.repaint();
   }
}
