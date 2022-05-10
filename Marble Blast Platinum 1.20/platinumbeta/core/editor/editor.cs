//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------


//------------------------------------------------------------------------------
// Hard coded images referenced from C++ code
//------------------------------------------------------------------------------

//   editor/SelectHandle.png
//   editor/DefaultHandle.png
//   editor/LockedHandle.png


//------------------------------------------------------------------------------
// Functions
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Mission Editor
//------------------------------------------------------------------------------

function Editor::create()
{
   // Not much to do here, build it and they will come...
   // Only one thing... the editor is a gui control which
   // expect the Canvas to exist, so it must be constructed
   // before the editor.
   new EditManager(Editor)
   {
      profile = "GuiContentProfile";
      horizSizing = "right";
      vertSizing = "top";
      position = "0 0";
      extent = "640 480";
      minExtent = "8 8";
      visible = "1";
      setFirstResponder = "0";
      modal = "1";
      helpTag = "0";
      open = false;
   };
}


function Editor::onAdd(%this)
{
   // Basic stuff
   exec("./cursors.cs");

   // Tools
   exec("./editor.bind.cs");
   exec("./objectBuilderGui.gui");

   // New World Editor
   exec("./EditorGui.gui");
   exec("./EditorGui.cs");

   // World Editor
   exec("./WorldEditorSettingsDlg.gui");

   // Jeff: object saving overrides
   exec("./saveObject.cs");

   //HiGuy: Doesn't exist!
   // Terrain Editor
//   exec("./TerrainEditorVSettingsGui.gui");

   // do gui initialization...
   EditorGui.init();

   //
   exec("./editorrender.cs");
}

function Editor::checkActiveLoadDone()
{
   if(isObject(EditorGui) && EditorGui.loadingMission)
   {
      Canvas.setContent(EditorGui);
      EditorGui.loadingMission = false;
      return true;
   }
   return false;
}

//------------------------------------------------------------------------------
function toggleEditor(%make)
{
   if (%make && ($testCheats || $enableEditor))
   {
      if (!$missionRunning)
      {
         MessageBoxOK("Mission Required", "Please load a mission before activating the Level Editor.", "");
//         return;
      }

      //HiGuy: Load all the datablocks (totally won't help with crashes)
      if (!$Server::Hosting && $Server::ServerType $= "MultiPlayer")
         onServerCreated();

      if (!isObject(Editor))
      {
         Editor::create();
         MissionCleanup.add(Editor);
      }
      if (Canvas.getContent() == EditorGui.getId())
         Editor.close();
      else
         Editor.open();
   }
}

//------------------------------------------------------------------------------
//  The editor action maps are defined in editor.bind.cs
GlobalActionMap.bind(keyboard, "f11", toggleEditor);
