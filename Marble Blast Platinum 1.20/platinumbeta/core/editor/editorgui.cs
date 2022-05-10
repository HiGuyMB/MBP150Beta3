//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------
//
// Matan: Terrain editor should not pop up in the Level Editor!!!
// It's functional as you can see below but doesn't appear anymore!
// Also commented out bits not used in the Level Editor by anyone.
//


//HiGuy: All level editor prefs are saved here
$Editor::PrefsFile = "~/core/editor/WEprefs.cs";

function EditorGui::getPrefs()
{
   //HiGuy: Load level editor prefs ($WEpref::) from disk
   exec($Editor::PrefsFile);
   EWorldEditor.dropType = getPrefSetting($WEpref::dropType, "atCamera");

   // same defaults as WorldEditor ctor
   EWorldEditor.planarMovement = getPrefSetting($WEpref::planarMovement, true);
   EWorldEditor.undoLimit = getPrefSetting($WEpref::undoLimit, 40);
   EWorldEditor.dropType = getPrefSetting($WEpref::dropType, "screenCenter");
   EWorldEditor.projectDistance = getPrefSetting($WEpref::projectDistance, 2000);
   EWorldEditor.boundingBoxCollision = getPrefSetting($WEpref::boundingBoxCollision, true);
   EWorldEditor.renderPlane = getPrefSetting($WEpref::renderPlane, true);
   EWorldEditor.renderPlaneHashes = getPrefSetting($WEpref::renderPlaneHashes, true);
   EWorldEditor.gridColor = getPrefSetting($WEpref::gridColor, "255 255 255 20");
   EWorldEditor.planeDim = getPrefSetting($WEpref::planeDim, 500);
   EWorldEditor.gridSize = getPrefSetting($WEpref::gridSize, "10 10 10");
   EWorldEditor.renderPopupBackground = getPrefSetting($WEpref::renderPopupBackground, true);
   EWorldEditor.popupBackgroundColor = getPrefSetting($WEpref::popupBackgroundColor, "100 100 100");
   EWorldEditor.popupTextColor = getPrefSetting($WEpref::popupTextColor, "255 255 0");
   EWorldEditor.selectHandle = getPrefSetting($WEpref::selectHandle, "gui/Editor_SelectHandle.png");
   EWorldEditor.defaultHandle = getPrefSetting($WEpref::defaultHandle, "gui/Editor_DefaultHandle.png");
   EWorldEditor.lockedHandle = getPrefSetting($WEpref::lockedHandle, "gui/Editor_LockedHandle.png");
   EWorldEditor.objectTextColor = getPrefSetting($WEpref::objectTextColor, "255 255 255");
   EWorldEditor.objectsUseBoxCenter = getPrefSetting($WEpref::objectsUseBoxCenter, true);
   EWorldEditor.axisGizmoMaxScreenLen = getPrefSetting($WEpref::axisGizmoMaxScreenLen, 200);
   EWorldEditor.axisGizmoActive = getPrefSetting($WEpref::axisGizmoActive, true);
   EWorldEditor.mouseMoveScale = getPrefSetting($WEpref::mouseMoveScale, 0.2);
   EWorldEditor.mouseRotateScale = getPrefSetting($WEpref::mouseRotateScale, 0.01);
   EWorldEditor.mouseScaleScale = getPrefSetting($WEpref::mouseScaleScale, 0.01);
   EWorldEditor.minScaleFactor = getPrefSetting($WEpref::minScaleFactor, 0.1);
   EWorldEditor.maxScaleFactor = getPrefSetting($WEpref::maxScaleFactor, 4000);
   EWorldEditor.objSelectColor = getPrefSetting($WEpref::objSelectColor, "255 0 0");
   EWorldEditor.objMouseOverSelectColor = getPrefSetting($WEpref::objMouseOverSelectColor, "0 0 255");
   EWorldEditor.objMouseOverColor = getPrefSetting($WEpref::objMouseOverColor, "0 255 0");
   EWorldEditor.showMousePopupInfo = getPrefSetting($WEpref::showMousePopupInfo, true);
   EWorldEditor.dragRectColor = getPrefSetting($WEpref::dragRectColor, "255 255 0");
   EWorldEditor.renderObjText = getPrefSetting($WEpref::renderObjText, true);
   EWorldEditor.renderObjHandle = getPrefSetting($WEpref::renderObjHandle, true);
   EWorldEditor.faceSelectColor = getPrefSetting($WEpref::faceSelectColor, "0 0 100 100");
   EWorldEditor.renderSelectionBox = getPrefSetting($WEpref::renderSelectionBox, true);
   EWorldEditor.selectionBoxColor = getPrefSetting($WEpref::selectionBoxColor, "255 255 0");
   EWorldEditor.snapToGrid = getPrefSetting($WEpref::snapToGrid, false);
   EWorldEditor.snapRotations = getPrefSetting($WEpref::snapRotations, false);
   EWorldEditor.rotationSnap = getPrefSetting($WEpref::rotationSnap, "15");

   //ETerrainEditor.softSelecting = 1;
   //ETerrainEditor.currentAction = "raiseHeight";
   //ETerrainEditor.currentMode = "select";
}

function EditorGui::setPrefs()
{
   $WEpref::dropType = EWorldEditor.dropType;
   $WEpref::planarMovement = EWorldEditor.planarMovement;
   $WEpref::undoLimit = EWorldEditor.undoLimit;
   $WEpref::dropType = EWorldEditor.dropType;
   $WEpref::projectDistance = EWorldEditor.projectDistance;
   $WEpref::boundingBoxCollision = EWorldEditor.boundingBoxCollision;
   $WEpref::renderPlane = EWorldEditor.renderPlane;
   $WEpref::renderPlaneHashes = EWorldEditor.renderPlaneHashes;
   $WEpref::gridColor = EWorldEditor.GridColor;
   $WEpref::planeDim = EWorldEditor.planeDim;
   $WEpref::gridSize = EWorldEditor.GridSize;
   $WEpref::renderPopupBackground = EWorldEditor.renderPopupBackground;
   $WEpref::popupBackgroundColor = EWorldEditor.PopupBackgroundColor;
   $WEpref::popupTextColor = EWorldEditor.PopupTextColor;
   $WEpref::selectHandle = EWorldEditor.selectHandle;
   $WEpref::defaultHandle = EWorldEditor.defaultHandle;
   $WEpref::lockedHandle = EWorldEditor.lockedHandle;
   $WEpref::objectTextColor = EWorldEditor.ObjectTextColor;
   $WEpref::objectsUseBoxCenter = EWorldEditor.objectsUseBoxCenter;
   $WEpref::axisGizmoMaxScreenLen = EWorldEditor.axisGizmoMaxScreenLen;
   $WEpref::axisGizmoActive = EWorldEditor.axisGizmoActive;
   $WEpref::mouseMoveScale = EWorldEditor.mouseMoveScale;
   $WEpref::mouseRotateScale = EWorldEditor.mouseRotateScale;
   $WEpref::mouseScaleScale = EWorldEditor.mouseScaleScale;
   $WEpref::minScaleFactor = EWorldEditor.minScaleFactor;
   $WEpref::maxScaleFactor = EWorldEditor.maxScaleFactor;
   $WEpref::objSelectColor = EWorldEditor.objSelectColor;
   $WEpref::objMouseOverSelectColor = EWorldEditor.objMouseOverSelectColor;
   $WEpref::objMouseOverColor = EWorldEditor.objMouseOverColor;
   $WEpref::showMousePopupInfo = EWorldEditor.showMousePopupInfo;
   $WEpref::dragRectColor = EWorldEditor.dragRectColor;
   $WEpref::renderObjText = EWorldEditor.renderObjText;
   $WEpref::renderObjHandle = EWorldEditor.renderObjHandle;
   $WEpref::raceSelectColor = EWorldEditor.faceSelectColor;
   $WEpref::renderSelectionBox = EWorldEditor.renderSelectionBox;
   $WEpref::selectionBoxColor = EWorldEditor.selectionBoxColor;
   $WEpref::snapToGrid = EWorldEditor.snapToGrid;
   $WEpref::snapRotations = EWorldEditor.snapRotations;
   $WEpref::rotationSnap = EWorldEditor.rotationSnap;

   //HiGuy: Save level editor prefs ($WEpref::) to disk
   export("$WEpref*", $Editor::PrefsFile);
}

function EditorGui::onSleep(%this)
{
   %this.setPrefs();
}

function EditorGui::init(%this)
{
   %this.getPrefs();

//   if(!isObject("terraformer"))
//      new Terraformer("terraformer");
   $SelectedOperation = -1;
   $NextOperationId   = 1;
//   $HeightfieldDirtyRow = -1;

   EditorMenuBar.clearMenus();
   EditorMenuBar.addMenu("File", 0);
   EditorMenuBar.addMenuItem("File", "New Mission...", 1);
   EditorMenuBar.addMenuItem("File", "Open Mission...", 2, "Ctrl O");
   EditorMenuBar.addMenuItem("File", "Save Mission...", 3, "Ctrl S");
   EditorMenuBar.addMenuItem("File", "Save Mission As...", 4);
//   EditorMenuBar.addMenuItem("File", "-", 0);
//   EditorMenuBar.addMenuItem("File", "Import Terraform Data...", 6);
//   EditorMenuBar.addMenuItem("File", "Import Texture Data...", 5);
//   EditorMenuBar.addMenuItem("File", "-", 0);
//   EditorMenuBar.addMenuItem("File", "Export Terraform Bitmap...", 5);

   EditorMenuBar.addMenu("Edit", 1);
   EditorMenuBar.addMenuItem("Edit", "Undo", 1, "Ctrl Z");
   EditorMenuBar.setMenuItemBitmap("Edit", "Undo", 1);
   EditorMenuBar.addMenuItem("Edit", "Redo", 2, "Ctrl R");
   EditorMenuBar.setMenuItemBitmap("Edit", "Redo", 2);
   EditorMenuBar.addMenuItem("Edit", "-", 0);
   EditorMenuBar.addMenuItem("Edit", "Cut", 3, "Ctrl X");
   EditorMenuBar.setMenuItemBitmap("Edit", "Cut", 3);
   EditorMenuBar.addMenuItem("Edit", "Copy", 4, "Ctrl C");
   EditorMenuBar.setMenuItemBitmap("Edit", "Copy", 4);
   EditorMenuBar.addMenuItem("Edit", "Paste", 5, "Ctrl V");
   EditorMenuBar.setMenuItemBitmap("Edit", "Paste", 5);
   EditorMenuBar.addMenuItem("Edit", "-", 0);
   EditorMenuBar.addMenuItem("Edit", "Select All", 6, "Ctrl A");
   EditorMenuBar.addMenuItem("Edit", "Select None", 7, "Ctrl N");
   EditorMenuBar.addMenuItem("Edit", "-", 0);
//   EditorMenuBar.addMenuItem("Edit", "Relight Scene", 14, "Alt L");
//   EditorMenuBar.addMenuItem("Edit", "-", 0);
   EditorMenuBar.addMenuItem("Edit", "World Editor Settings...", 12);
//   EditorMenuBar.addMenuItem("Edit", "Terrain Editor Settings...", 13);

   EditorMenuBar.addMenu("Camera", 7);
   EditorMenuBar.addMenuItem("Camera", "Drop Camera at Player", 1, "Alt Q");
   EditorMenuBar.addMenuItem("Camera", "Drop Player at Camera", 2, "Alt W");
   EditorMenuBar.addMenuItem("Camera", "Toggle Camera", 10, "Alt C");
   EditorMenuBar.addMenuItem("Camera", "-", 0);
   EditorMenuBar.addMenuItem("Camera", "Slowest", 3, "Shift 1", 1);
   EditorMenuBar.addMenuItem("Camera", "Very Slow", 4, "Shift 2", 1);
   EditorMenuBar.addMenuItem("Camera", "Slow", 5, "Shift 3", 1);
   EditorMenuBar.addMenuItem("Camera", "Medium Pace", 6, "Shift 4", 1);
   EditorMenuBar.addMenuItem("Camera", "Fast", 7, "Shift 5", 1);
   EditorMenuBar.addMenuItem("Camera", "Very Fast", 8, "Shift 6", 1);
   EditorMenuBar.addMenuItem("Camera", "Fastest", 9, "Shift 7", 1);

   EditorMenuBar.addMenu("World", 6);
   EditorMenuBar.addMenuItem("World", "Lock Selection", 10, "Ctrl L");
   EditorMenuBar.addMenuItem("World", "Unlock Selection", 11, "Ctrl Shift L");
   EditorMenuBar.addMenuItem("World", "-", 0);
   EditorMenuBar.addMenuItem("World", "Hide Selection", 12, "Ctrl H");
   EditorMenuBar.addMenuItem("World", "Show Selection", 13, "Ctrl Shift H");
   EditorMenuBar.addMenuItem("World", "-", 0);
   EditorMenuBar.addMenuItem("World", "Delete Selection", 17, "Delete");
   EditorMenuBar.addMenuItem("World", "Camera To Selection", 14);
   EditorMenuBar.addMenuItem("World", "Reset Transforms", 15);
   EditorMenuBar.addMenuItem("World", "Drop Selection", 16, "Ctrl D");
   EditorMenuBar.addMenuItem("World", "Add Selection to Instant Group", 17);
   EditorMenuBar.addMenuItem("World", "-", 0);
   EditorMenuBar.addMenuItem("World", "Drop at Origin", 0, "Alt 1", 1);
   EditorMenuBar.addMenuItem("World", "Drop at Camera", 1, "Alt 2", 1);
   EditorMenuBar.addMenuItem("World", "Drop at Average Camera", 7, "Alt 3", 1);
   EditorMenuBar.addMenuItem("World", "Drop at Camera w/Rot", 2, "Alt 4", 1);
   EditorMenuBar.addMenuItem("World", "Drop below Camera", 3, "Alt 5", 1);
   EditorMenuBar.addMenuItem("World", "Drop at Screen Center", 4, "Alt 6", 1);
   EditorMenuBar.addMenuItem("World", "Drop at Centroid", 5, "Alt 7", 1);
   EditorMenuBar.addMenuItem("World", "Drop to Ground", 6, "Alt 8", 1);

//   EditorMenuBar.addMenu("Action", 3);
//   EditorMenuBar.addMenuItem("Action", "Select", 1, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Adjust Selection", 2, "", 1);
//   EditorMenuBar.addMenuItem("Action", "-", 0);
//   EditorMenuBar.addMenuItem("Action", "Add Dirt", 6, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Excavate", 6, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Adjust Height", 6, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Flatten", 4, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Smooth", 5, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Set Height", 7, "", 1);
//   EditorMenuBar.addMenuItem("Action", "-", 0);
//  EditorMenuBar.addMenuItem("Action", "Set Empty", 8, "", 1);
//   EditorMenuBar.addMenuItem("Action", "Clear Empty", 8, "", 1);
//   EditorMenuBar.addMenuItem("Action", "-", 0);
//   EditorMenuBar.addMenuItem("Action", "Paint Material", 9, "", 1);

//   EditorMenuBar.addMenu("Brush", 4);
//   EditorMenuBar.addMenuItem("Brush", "Box Brush", 91, "", 1);
//   EditorMenuBar.addMenuItem("Brush", "Circle Brush", 92, "", 1);
//   EditorMenuBar.addMenuItem("Brush", "-", 0);
//   EditorMenuBar.addMenuItem("Brush", "Soft Brush", 93, "", 2);
//   EditorMenuBar.addMenuItem("Brush", "Hard Brush", 94, "", 2);
//   EditorMenuBar.addMenuItem("Brush", "-", 0);
//   EditorMenuBar.addMenuItem("Brush", "Size 1 x 1", 1, "Alt 1", 3);
//   EditorMenuBar.addMenuItem("Brush", "Size 3 x 3", 3, "Alt 2", 3);
//   EditorMenuBar.addMenuItem("Brush", "Size 5 x 5", 5, "Alt 3", 3);
//   EditorMenuBar.addMenuItem("Brush", "Size 9 x 9", 9, "Alt 4", 3);
//   EditorMenuBar.addMenuItem("Brush", "Size 15 x 15", 15, "Alt 5", 3);
//   EditorMenuBar.addMenuItem("Brush", "Size 25 x 25", 25, "Alt 6", 3);

   EditorMenuBar.addMenu("Create", 7);
   EditorMenuBar.addMenuItem("Create", "Red Gem", 1, "1", 1);
   EditorMenuBar.addMenuItem("Create", "Yellow Gem", 2, "2", 1);
   EditorMenuBar.addMenuItem("Create", "Blue Gem", 3, "3", 1);
   EditorMenuBar.addMenuItem("Create", "Spawn Trigger", 4, "4", 1);
   EditorMenuBar.addMenuItem("Create", "Super Jump", 5, "5", 1);
   EditorMenuBar.addMenuItem("Create", "Super Speed", 6, "6", 1);
   EditorMenuBar.addMenuItem("Create", "Gyrocopter", 7, "7", 1);
   EditorMenuBar.addMenuItem("Create", "Mega Marble", 8, "8", 1);
   EditorMenuBar.addMenuItem("Create", "Ultra Blast", 9, "9", 1);
   EditorMenuBar.addMenuItem("Create", "Bounds Trigger", 10, "", 1);
   EditorMenuBar.addMenuItem("Create", "Gem Group", 11, "", 1);

   EditorMenuBar.addMenu("Window", 2);
   EditorMenuBar.addMenuItem("Window", "World Editor", 2, "F2", 1);
   EditorMenuBar.addMenuItem("Window", "World Editor Inspector", 3, "F3", 1);
   EditorMenuBar.addMenuItem("Window", "World Editor Creator", 4, "F4", 1);
//   EditorMenuBar.addMenuItem("Window", "Mission Area Editor", 5, "F5", 1);
//   EditorMenuBar.addMenuItem("Window", "-", 0);
//   EditorMenuBar.addMenuItem("Window", "Terrain Editor", 6, "F6", 1);
//   EditorMenuBar.addMenuItem("Window", "Terrain Terraform Editor", 7, "F7", 1);
//   EditorMenuBar.addMenuItem("Window", "Terrain Texture Editor", 8, "F8", 1);
//   EditorMenuBar.addMenuItem("Window", "Terrain Texture Painter", 9, "", 1);

//   EditorMenuBar.onActionMenuItemSelect(0, "Adjust Height");
//   EditorMenuBar.onBrushMenuItemSelect(0, "Circle Brush");
//   EditorMenuBar.onBrushMenuItemSelect(0, "Soft Brush");
//   EditorMenuBar.onBrushMenuItemSelect(9, "Size 9 x 9");
   EditorMenuBar.onCameraMenuItemSelect(6, "Medium Pace");
   EditorMenuBar.onWorldMenuItemSelect(0, "Drop at Average Camera");

   EWorldEditor.init();
//   ETerrainEditor.attachTerrain();
//   TerraformerInit();
//   TextureInit();

   //
   Creator.init();
   EditorTree.init();
   ObjectBuilderGui.init();

   EWorldEditor.isDirty = false;
//   ETerrainEditor.isDirty = false;
//   ETerrainEditor.isMissionDirty = false;
   EditorGui.saveAs = false;
}

function EditorNewMission()
{
   if(//ETerrainEditor.isMissionDirty || ETerrainEditor.isDirty ||
    EWorldEditor.isDirty)
   {
      MessageBoxYesNo("Mission Modified", "Would you like to save changes to the current mission \"" @
         $Server::MissionFile @ "\" before creating a new mission?", "EditorDoNewMission(true);", "EditorDoNewMission(false);");
   }
   else
      EditorDoNewMission(false);
}

function EditorSaveMissionMenu()
{
   if(EditorGui.saveAs)
      EditorSaveMissionAs();
   else
      EditorSaveMission();
}

function EditorSaveMission()
{
   // just save the mission without renaming it

   // first check for dirty and read-only files:
   if((EWorldEditor.isDirty) && // || ETerrainEditor.isMissionDirty) &&
    !isWriteableFileName($Server::MissionFile))
   {
      MessageBoxOK("Error", "Mission file \""@ $Server::MissionFile @ "\" is read-only.");
      return false;
   }
//   if(ETerrainEditor.isDirty && !isWriteableFileName(Terrain.terrainFile))
//   {
//     MessageBoxOK("Error", "Terrain file \""@ Terrain.terrainFile @ "\" is read-only.");
//      return false;
//   }

   // now write the terrain and mission files out:

   if (EWorldEditor.isDirty) { // || ETerrainEditor.isMissionDirty) {
      // Update the base transforms of the moving platforms incase the user never hit Apply in the inspector - phil
      updatePathedInteriorBaseTransforms();
      deactivatePackage(save);
      ActivatePackage(save);
      MissionGroup.save($Server::MissionFile);
   }
//   if(ETerrainEditor.isDirty)
//      Terrain.save(Terrain.terrainFile);
   EWorldEditor.isDirty = false;
//   ETerrainEditor.isDirty = false;
//   ETerrainEditor.isMissionDirty = false;
   EditorGui.saveAs = false;

   return true;
}

function EditorDoSaveAs(%missionName)
{
   activatePackage(Save);
//   ETerrainEditor.isDirty = true;
   EWorldEditor.isDirty = true;
   %saveMissionFile = $Server::MissionFile;
//   %saveTerrName = Terrain.terrainFile;

   $Server::MissionFile = %missionName;
//   Terrain.terrainFile = filePath(%missionName) @ "/" @ fileBase(%missionName) @ ".ter";

   if(!EditorSaveMission())
   {
      $Server::MissionFile = %saveMissionFile;
  //    Terrain.terrainFile = %saveTerrName;
   }
}

function EditorSaveMissionAs()
{
   getSaveFilename("*.mis", "EditorDoSaveAs", $Server::MissionFile);

}

function EditorDoLoadMission(%file)
{
   // close the current editor, it will get cleaned up by MissionCleanup
   Editor.close();

   loadMission( %file, true ) ;

   // recreate and open the editor
   Editor::create();
   MissionCleanup.add(Editor);
   EditorGui.loadingMission = true;
   Editor.open();
}

function EditorSaveBeforeLoad()
{
   if(EditorSaveMission())
      getLoadFilename("*.mis", "EditorDoLoadMission");
}

function EditorDoNewMission(%saveFirst)
{
   if(%saveFirst)
      EditorSaveMission();

   %file = $Server::ServerType $= "MultiPlayer" ? "ExampleMission.mis" : "MissionTemplate.mis";
   %mission = findFirstFile("*/" @ %file);
   if(%mission $= "")
   {
      MessageBoxOk("Error", "Missing mission template \"" @ %file @ "\".");
      return;
   }
   EditorDoLoadMission(%mission);
   EditorGui.saveAs = true;
   EWorldEditor.isDirty = true;
//   ETerrainEditor.isDirty = true;
}

function EditorOpenMission()
{
   if(//ETerrainEditor.isMissionDirty || ETerrainEditor.isDirty ||
    EWorldEditor.isDirty)
   {
      MessageBoxYesNo("Mission Modified", "Would you like to save changes to the current mission \"" @
         $Server::MissionFile @ "\" before opening a new mission?", "EditorSaveBeforeLoad();", "getLoadFilename(\"*.mis\", \"EditorDoLoadMission\");");
   }
   else
      getLoadFilename("*.mis", "EditorDoLoadMission");
}

function EditorMenuBar::onMenuSelect(%this, %menuId, %menu)
{
   if(%menu $= "File")
   {
//      %editingHeightfield = ETerrainEditor.isVisible() && EHeightField.isVisible();
//      EditorMenuBar.setMenuItemEnable("File", "Export Terraform Bitmap...", %editingHeightfield);
      EditorMenuBar.setMenuItemEnable("File", "Save Mission...", //ETerrainEditor.isDirty || ETerrainEditor.isMissionDirty ||
       EWorldEditor.isDirty);
   }
   else if(%menu $= "Edit")
   {
      // enable/disable undo, redo, cut, copy, paste depending on editor settings

      if(EWorldEditor.isVisible())
      {
         // do actions based on world editor...
         EditorMenuBar.setMenuItemEnable("Edit", "Select All", true);
         EditorMenuBar.setMenuItemEnable("Edit", "Paste", EWorldEditor.canPasteSelection());
         %canCutCopy = EWorldEditor.getSelectionSize() > 0;

         EditorMenuBar.setMenuItemEnable("Edit", "Cut", %canCutCopy);
         EditorMenuBar.setMenuItemEnable("Edit", "Copy", %canCutCopy);

      }
      //else if(ETerrainEditor.isVisible())
      //{
         //EditorMenuBar.setMenuItemEnable("Edit", "Cut", false);
         //EditorMenuBar.setMenuItemEnable("Edit", "Copy", false);
         //EditorMenuBar.setMenuItemEnable("Edit", "Paste", false);
         //EditorMenuBar.setMenuItemEnable("Edit", "Select All", false);
      //}
   }
   else if(%menu $= "World")
   {
      %selSize = EWorldEditor.getSelectionSize();
      %lockCount = EWorldEditor.getSelectionLockCount();
      %hideCount = EWorldEditor.getSelectionHiddenCount();

      EditorMenuBar.setMenuItemEnable("World", "Lock Selection", %lockCount < %selSize);
      EditorMenuBar.setMenuItemEnable("World", "Unlock Selection", %lockCount > 0);
      EditorMenuBar.setMenuItemEnable("World", "Hide Selection", %hideCount < %selSize);
      EditorMenuBar.setMenuItemEnable("World", "Show Selection", %hideCount > 0);

      EditorMenuBar.setMenuItemEnable("World", "Add Selection to Instant Group", %selSize > 0);
      EditorMenuBar.setMenuItemEnable("World", "Camera To Selection", %selSize > 0);
      EditorMenuBar.setMenuItemEnable("World", "Reset Transforms", %selSize > 0 && %lockCount == 0);
      EditorMenuBar.setMenuItemEnable("World", "Drop Selection", %selSize > 0 && %lockCount == 0);
      EditorMenuBar.setMenuItemEnable("World", "Delete Selection", %selSize > 0 && %lockCount == 0);
   }
}

function EditorMenuBar::onMenuItemSelect(%this, %menuId, %menu, %itemId, %item)
{
   switch$(%menu)
   {
      case "File":
         %this.onFileMenuItemSelect(%itemId, %item);
      case "Edit":
         %this.onEditMenuItemSelect(%itemId, %item);
      case "World":
         %this.onWorldMenuItemSelect(%itemId, %item);
      case "Window":
         %this.onWindowMenuItemSelect(%itemId, %item);
//      case "Action":
//         %this.onActionMenuItemSelect(%itemId, %item);
//      case "Brush":
//         %this.onBrushMenuItemSelect(%itemId, %item);
      case "Create":
         %this.onCreateMenuItemSelect(%itemId, %item);
      case "Testing":
         %this.onTestingMenuItemSelect(%itemId, %item);
      case "Camera":
         %this.onCameraMenuItemSelect(%itemId, %item);
   }
}

function EditorMenuBar::onFileMenuItemSelect(%this, %itemId, %item)
{
   switch$(%item)
   {
      case "New Mission...":
         EditorNewMission();
      case "Open Mission...":
         EditorOpenMission();
      case "Save Mission...":
         EditorSaveMissionMenu();
      case "Save Mission As...":
         EditorSaveMissionAs();
//      case "Import Texture Data...":
//         Texture::import();
//      case "Import Terraform Data...":
//         Heightfield::import();
//      case "Export Terraform Bitmap...":
//         Heightfield::saveBitmap("");
      case "Quit":
   }
}

function EditorMenuBar::onCameraMenuItemSelect(%this, %itemId, %item)
{
   switch$(%item)
   {
      case "Drop Camera at Player":
         commandToServer('dropCameraAtPlayer');
      case "Drop Player at Camera":
         commandToServer('DropPlayerAtCamera');
      case "Toggle Camera":
         commandToServer('ToggleCamera');
      default:
         // all the rest are camera speeds:
         // item ids go from 3 (slowest) to 9 (fastest)
         %this.setMenuItemChecked("Camera", %itemId, true);
         // camera movement speed goes from 5 to 200:
         $Camera::movementSpeed = ((%itemId - 3) / 6.0) * 195 + 5;
   }
}

//function EditorMenuBar::onActionMenuItemSelect(%this, %itemId, %item)
//{
   //EditorMenuBar.setMenuItemChecked("Action", %item, true);
   //switch$(%item)
   //{
      //case "Select":
         //ETerrainEditor.currentMode = "select";
         //ETerrainEditor.selectionHidden = false;
         //ETerrainEditor.renderVertexSelection = true;
         //ETerrainEditor.setAction("select");
      //case "Adjust Selection":
         //ETerrainEditor.currentMode = "adjust";
         //ETerrainEditor.selectionHidden = false;
         //ETerrainEditor.setAction("adjustHeight");
         //ETerrainEditor.currentAction = brushAdjustHeight;
         //ETerrainEditor.renderVertexSelection = true;
      //default:
         //ETerrainEditor.currentMode = "paint";
         //ETerrainEditor.selectionHidden = true;
         //ETerrainEditor.setAction(ETerrainEditor.currentAction);
         //switch$(%item)
         //{
            //case "Add Dirt":
               //ETerrainEditor.currentAction = raiseHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Paint Material":
               //ETerrainEditor.currentAction = paintMaterial;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Excavate":
               //ETerrainEditor.currentAction = lowerHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Set Height":
               //ETerrainEditor.currentAction = setHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Adjust Height":
               //ETerrainEditor.currentAction = brushAdjustHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Flatten":
               //ETerrainEditor.currentAction = flattenHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Smooth":
               //ETerrainEditor.currentAction = smoothHeight;
               //ETerrainEditor.renderVertexSelection = true;
            //case "Set Empty":
               //ETerrainEditor.currentAction = setEmpty;
               //ETerrainEditor.renderVertexSelection = false;
            //case "Clear Empty":
               //ETerrainEditor.currentAction = clearEmpty;
               //ETerrainEditor.renderVertexSelection = false;
         //}
         //if(ETerrainEditor.currentMode $= "select")
            //ETerrainEditor.processAction(ETerrainEditor.currentAction);
         //else if(ETerrainEditor.currentMode $= "paint")
            //ETerrainEditor.setAction(ETerrainEditor.currentAction);
   //}
//}

//function EditorMenuBar::onBrushMenuItemSelect(%this, %itemId, %item)
//{
   //EditorMenuBar.setMenuItemChecked("Brush", %item, true);
   //switch$(%item)
   //{
      //case "Box Brush":
         //ETerrainEditor.setBrushType(box);
      //case "Circle Brush":
         //ETerrainEditor.setBrushType(ellipse);
      //case "Soft Brush":
         //ETerrainEditor.enableSoftBrushes = true;
      //case "Hard Brush":
         //ETerrainEditor.enableSoftBrushes = false;
      //default:
         // the rest are brush sizes:
         //ETerrainEditor.brushSize = %itemId;

         //ETerrainEditor.setBrushSize(%itemId, %itemId);
   //}
//}


function EditorMenuBar::onWorldMenuItemSelect(%this, %itemId, %item)
{
   // edit commands for world editor...
   switch$(%item)
   {
      case "Lock Selection":
         EWorldEditor.lockSelection(true);
      case "Unlock Selection":
         EWorldEditor.lockSelection(false);
      case "Hide Selection":
         EWorldEditor.hideSelection(true);
      case "Show Selection":
         EWorldEditor.hideSelection(false);
      case "Camera To Selection":
         EWorldEditor.dropCameraToSelection();
      case "Reset Transforms":
         EWorldEditor.resetTransforms();
      case "Drop Selection":
         EWorldEditor.dropSelection();
      case "Delete Selection":
         EWorldEditor.deleteSelection();
      case "Add Selection to Instant Group":
         EWorldEditor.addSelectionToAddGroup();
      default:
         EditorMenuBar.setMenuItemChecked("World", %item, true);
         EWorldEditor.dropAvg = false;
         switch$(%item)
         {
            case "Drop at Origin":
               EWorldEditor.dropType = "atOrigin";
            case "Drop at Camera":
               EWorldEditor.dropType = "atCamera";
            case "Drop at Average Camera":
               EWorldEditor.dropType = "atCamera";
               EWorldEditor.dropAvg = true;
            case "Drop at Camera w/Rot":
               EWorldEditor.dropType = "atCameraRot";
            case "Drop below Camera":
               EWorldEditor.dropType = "belowCamera";
            case "Drop at Screen Center":
               EWorldEditor.dropType = "screenCenter";
            case "Drop to Ground":
               EWorldEditor.dropType = "toGround";
            case "Drop at Centroid":
               EWorldEditor.dropType = "atCentroid";
         }
   }
}

function EditorMenuBar::onEditMenuItemSelect(%this, %itemId, %item)
{
   if(%item $= "World Editor Settings...")
      Canvas.pushDialog(WorldEditorSettingsDlg);
//   else if(%item $= "Terrain Editor Settings...")
//      Canvas.pushDialog(TerrainEditorValuesSettingsGui, 99);
   else if(%item $= "Relight Scene")
      lightScene("", forceAlways);
   else if(EWorldEditor.isVisible())
   {
      // edit commands for world editor...
      switch$(%item)
      {
         case "Undo":
            EWorldEditor.undo();
         case "Redo":
            EWorldEditor.redo();
         case "Copy":
            EWorldEditor.copySelection();
         case "Cut":
            EWorldEditor.copySelection();
            EWorldEditor.deleteSelection();
         case "Paste":
            EWorldEditor.pasteSelection();
         case "Select All":
         case "Select None":
      }
   }
   //else if(ETerrainEditor.isVisible())
   //{
      // do some terrain stuffin'
      //switch$(%item)
      //{
         //case "Undo":
            //ETerrainEditor.undo();
         //case "Redo":
            //ETerrainEditor.redo();
         //case "Select None":
            //ETerrainEditor.clearSelection();
      //}
   //}
}

function EditorMenuBar::onWindowMenuItemSelect(%this, %itemId, %item)
{
   EditorGui.setEditor(%item);
}

function EditorMenuBar::onCreateMenuItemSelect(%this, %itemId, %item) {
   %obj = -1;
   switch$ (%item) {
   case "Red Gem":
      %obj = new Item() {dataBlock = "GemItemRed";rotate=1;static=1;};
   case "Yellow Gem":
      %obj = new Item() {dataBlock = "GemItemYellow";rotate=1;static=1;};
   case "Blue Gem":
      %obj = new Item() {dataBlock = "GemItemBlue";rotate=1;static=1;};
   case "Spawn Trigger":
      %obj = new Trigger() {dataBlock = "SpawnTrigger";polyhedron="0 0 0 1 0 0 0 -1 0 0 0 1";center="1";};
   case "Super Jump":
      %obj = new Item() {dataBlock = "SuperJumpItem";rotate=1;static=1;};
   case "Super Speed":
      %obj = new Item() {dataBlock = "SuperSpeedItem";rotate=1;static=1;};
   case "Gyrocopter":
      %obj = new Item() {dataBlock = "HelicopterItem";rotate=1;static=1;};
   case "Mega Marble":
      %obj = new Item() {dataBlock = "MegaMarbleItem";rotate=1;static=1;};
   case "Ultra Blast":
      %obj = new Item() {dataBlock = "BlastItem";rotate=1;static=1;};
   case "Bounds Trigger":
      generateWorldBox();
   case "Gem Group":
      EWorldEditor.makeGemGroup();
   }
   if (%obj != -1) {
      %obj.setTransform("0 0 0 1 0 0 0");
      $InstantGroup.add(%obj);
      EWorldEditor.clearSelection();
      EWorldEditor.selectObject(%obj);
      EWorldEditor.dropSelection();
   }
}

function EditorMenuBar::onTestingMenuItemSelect(%this, %itemId, %item) {
   switch$ (%item) {
      case "Rotate Gems":
         EWorldEditor.rotateGems();
      case "Make GemGroup":
         EWorldEditor.makeGemGroup();
      case "Destroy GemGroups":
         EWorldEditor.destroyGemGroups();
      case "Generate Bounds":
         generateWorldBox();
      case "Drop at Ground":
         EWorldEditor.dropAtGround();
      case "Round Coordinates":
         EWorldEditor.roundCoords();
      case "Matan\'s Magic Button ;)":
         EWorldEditor.roundCoords();
         EWorldEditor.dropAtGround();
      case "Anti-OCD Button": //:D
         EWorldEditor.malign();
   }
}

function EditorGui::setWorldEditorVisible(%this)
{
   EWorldEditor.setVisible(true);
//   ETerrainEditor.setVisible(false);
//   EditorMenuBar.setMenuVisible("World", true);
//   EditorMenuBar.setMenuVisible("Action", false);
//   EditorMenuBar.setMenuVisible("Brush", false);
   EWorldEditor.makeFirstResponder(true);
}

//function EditorGui::setTerrainEditorVisible(%this)
//{
   //EWorldEditor.setVisible(false);
   //ETerrainEditor.setVisible(true);
   //ETerrainEditor.attachTerrain();
   //EHeightField.setVisible(false);
   //ETexture.setVisible(false);
   //EditorMenuBar.setMenuVisible("World", false);
   //EditorMenuBar.setMenuVisible("Action", true);
   //EditorMenuBar.setMenuVisible("Brush", true);
   //ETerrainEditor.makeFirstResponder(true);
   //EPainter.setVisible(false);
//}

function EditorGui::setEditor(%this, %editor)
{
   EditorMenuBar.setMenuItemBitmap("Window", %this.currentEditor, -1);
   EditorMenuBar.setMenuItemBitmap("Window", %editor, 0);
   %this.currentEditor = %editor;

   switch$(%editor)
   {
      case "World Editor":
         EWFrame.setVisible(false);
         EWMissionArea.setVisible(false);
         %this.setWorldEditorVisible();
      case "World Editor Inspector":
         EWFrame.setVisible(true);
         EWMissionArea.setVisible(false);
         EWCreatorPane.setVisible(false);
         EWInspectorPane.setVisible(true);
         %this.setWorldEditorVisible();
      case "World Editor Creator":
         EWFrame.setVisible(true);
         EWMissionArea.setVisible(false);
         EWCreatorPane.setVisible(true);
         EWInspectorPane.setVisible(false);
         %this.setWorldEditorVisible();
//      case "Mission Area Editor":
//         EWFrame.setVisible(false);
//         EWMissionArea.setVisible(true);
//         %this.setWorldEditorVisible();
//      case "Terrain Editor":
//         %this.setTerrainEditorVisible();
//      case "Terrain Terraform Editor":
//         %this.setTerrainEditorVisible();
//         EHeightField.setVisible(true);
//      case "Terrain Texture Editor":
//         %this.setTerrainEditorVisible();
//         ETexture.setVisible(true);
//      case "Terrain Texture Painter":
//         %this.setTerrainEditorVisible();
//         EPainter.setVisible(true);
//         EPainter.setup();

   }
}

function EditorGui::getHelpPage(%this)
{
   switch$(%this.currentEditor)
   {
      case "World Editor" or "World Editor Inspector" or "World Editor Creator":
         return "5. World Editor";
//      case "Mission Area Editor":
//         return "6. Mission Area Editor";
//      case "Terrain Editor":
//         return "7. Terrain Editor";
//      case "Terrain Terraform Editor":
//         return "8. Terrain Terraform Editor";
//      case "Terrain Texture Editor":
//         return "9. Terrain Texture Editor";
//      case "Terrain Texture Painter":
//         return "10. Terrain Texture Painter";
   }
}


function EWorldEditor::dropSelection(%this) {
   if (%this.dropType $= "toGround")
      %this.dropAtGround();
   else
      Parent::dropSelection(%this);
   if (%this.dropAvg) {
      //HiGuy: Average pos
      %this.roundCoords();
   }
}

//function ETerrainEditor::setPaintMaterial(%this, %matIndex)
//{
   //ETerrainEditor.paintMaterial = EPainter.mat[%matIndex];
//}

//function ETerrainEditor::changeMaterial(%this, %matIndex)
//{
   //EPainter.matIndex = %matIndex;
   //getLoadFilename("*/terrains/*.png\t*/terrains/*.jpg", EPainterChangeMat);
//}

//function EPainterChangeMat(%file)
//{
   // make sure the material isn't already in the terrain.
   //%file = filePath(%file) @ "/" @ fileBase(%file);
   //for(%i = 0; %i < 6; %i++)
      //if(EPainter.mat[%i] $= %file)
         //return;

   //EPainter.mat[EPainter.matIndex] = %file;
   //%mats = "";
   //for(%i = 0; %i < 6; %i++)
      //%mats = %mats @ EPainter.mat[%i] @ "\n";
   //ETerrainEditor.setTerrainMaterials(%mats);
   //EPainter.setup();
   //("ETerrainMaterialPaint" @ EPainter.matIndex).performClick();
//}

//function EPainter::setup(%this)
//{
   //EditorMenuBar.onActionMenuItemSelect(0, "Paint Material");
   //%mats = ETerrainEditor.getTerrainMaterials();
   //%valid = true;
   //for(%i = 0; %i < 6; %i++)
   //{
      //%mat = getRecord(%mats, %i);
      //%this.mat[%i] = %mat;
      //("ETerrainMaterialText" @ %i).setText(fileBase(%mat));
      //("ETerrainMaterialBitmap" @ %i).setBitmap(%mat);
      //("ETerrainMaterialChange" @ %i).setActive(true);
      //("ETerrainMaterialPaint" @ %i).setActive(%mat !$= "");
      //if(%mat $= "")
      //{
         //("ETerrainMaterialChange" @ %i).setText("Add...");
         //if(%valid)
            //%valid = false;
         //else
            //("ETerrainMaterialChange" @ %i).setActive(false);
      //}
      //else
         //("ETerrainMaterialChange" @ %i).setText("Change...");
   //}
   //ETerrainMaterialPaint0.performClick();
//}

function EditorGui::onWake(%this)
{
   MoveMap.push();
   EditorMap.push();
   %this.setEditor(%this.currentEditor);
}

function EditorGui::onSleep(%this)
{
   EditorMap.pop();
   MoveMap.pop();
}

function AreaEditor::onUpdate(%this, %area)
{
   AreaEditingText.setValue( "X: " @ getWord(%area,0) @ " Y: " @ getWord(%area,1) @ " W: " @ getWord(%area,2) @ " H: " @ getWord(%area,3));
}

function AreaEditor::onWorldOffset(%this, %offset)
{
}

function EditorTree::init(%this)
{
   %this.open(MissionGroup);

   // context menu
   new GuiControl(ETContextPopupDlg)
   {
      profile = "GuiModelessDialogProfile";
	   horizSizing = "width";
	   vertSizing = "height";
	   position = "0 0";
	   extent = "640 480";
	   minExtent = "8 8";
	   visible = "1";
	   setFirstResponder = "0";
	   modal = "1";

      new GuiPopUpMenuCtrl(ETContextPopup)
      {
         profile = "GuiScrollProfile";
         position = "0 0";
         extent = "0 0";
         minExtent = "0 0";
         maxPopupHeight = "200";
         command = "canvas.popDialog(ETContextPopupDlg);";
      };
   };
   ETContextPopup.setVisible(false);
}

function EditorTree::onInspect(%this, %obj)
{
   Inspector.inspect(%obj);
   ECreateSubsBtn.setVisible(%obj.getClassName() $= "InteriorInstance");
   InspectorNameEdit.setValue(%obj.getName());
}

function EditorTree::onSelect(%this, %obj)
{
   if($AIEdit)
      aiEdit.selectObject(%obj);
   else
      EWorldEditor.selectObject(%obj);

}

function EditorTree::onUnselect(%this, %obj)
{
   if($AIEdit)
      aiEdit.unselectObject(%obj);
   else
      EWorldEditor.unselectObject(%obj);
}

function ETContextPopup::onSelect(%this, %index, %value)
{
   switch(%index)
   {
      case 0:
         EditorTree.contextObj.delete();
   }
}

//------------------------------------------------------------------------------
// Functions
//------------------------------------------------------------------------------

function WorldEditor::createSubs(%this)
{
   for(%i = 0; %i < %this.getSelectionSize(); %i++)
   {
      %obj = %this.getSelectedObject(%i);
      if(%obj.getClassName() $= "InteriorInstance")
         %obj.magicButton();
   }
}

function WorldEditor::init(%this)
{
   // add objclasses which we do not want to collide with
   %this.ignoreObjClass(Sky);

   // editing modes
   %this.numEditModes = 3;
   %this.editMode[0]    = "move";
   %this.editMode[1]    = "rotate";
   %this.editMode[2]    = "scale";

   // context menu
   new GuiControl(WEContextPopupDlg)
   {
      profile = "GuiModelessDialogProfile";
	   horizSizing = "width";
	   vertSizing = "height";
	   position = "0 0";
	   extent = "640 480";
	   minExtent = "8 8";
	   visible = "1";
	   setFirstResponder = "0";
	   modal = "1";

      new GuiPopUpMenuCtrl(WEContextPopup)
      {
         profile = "GuiScrollProfile";
         position = "0 0";
         extent = "0 0";
         minExtent = "0 0";
         maxPopupHeight = "200";
         command = "canvas.popDialog(WEContextPopupDlg);";
      };
   };
   WEContextPopup.setVisible(false);
}

//------------------------------------------------------------------------------

function WorldEditor::onDblClick(%this, %obj)
{
   // Commented out because making someone double click to do this is stupid
   // and has the possibility of moving hte object

   //Inspector.inspect(%obj);
   //InspectorNameEdit.setValue(%obj.getName());
}

function WorldEditor::onClick( %this, %obj )
{
   Inspector.inspect( %obj );
   ECreateSubsBtn.setVisible(%obj.getClassName() $= "InteriorInstance");
   InspectorNameEdit.setValue( %obj.getName() );
}

//------------------------------------------------------------------------------

function WorldEditor::export(%this)
{
   getSaveFilename("~/editor/*.mac", %this @ ".doExport", "selection.mac");
}

function WorldEditor::doExport(%this, %file)
{
   missionGroup.save("~/editor/" @ %file, true);
}

function WorldEditor::import(%this)
{
   getLoadFilename("~/editor/*.mac", %this @ ".doImport");
}

function WorldEditor::doImport(%this, %file)
{
   exec("~/editor/" @ %file);
}

function WorldEditor::onGuiUpdate(%this, %text)
{

}

function WorldEditor::getSelectionLockCount(%this)
{
   %ret = 0;
   for(%i = 0; %i < %this.getSelectionSize(); %i++)
   {
      %obj = %this.getSelectedObject(%i);
      if(%obj.locked $= "true")
         %ret++;
   }
   return %ret;
}

function WorldEditor::getSelectionHiddenCount(%this)
{
   %ret = 0;
   for(%i = 0; %i < %this.getSelectionSize(); %i++)
   {
      %obj = %this.getSelectedObject(%i);
      if(%obj.hidden $= "true")
         %ret++;
   }
   return %ret;
}

function WorldEditor::dropCameraToSelection(%this)
{
   if(%this.getSelectionSize() == 0)
      return;

   %pos = %this.getSelectionCentroid();
   %cam = LocalClientConnection.camera.getTransform();

   // set the pnt
   %cam = setWord(%cam, 0, getWord(%pos, 0));
   %cam = setWord(%cam, 1, getWord(%pos, 1));
   %cam = setWord(%cam, 2, getWord(%pos, 2));

   LocalClientConnection.camera.setTransform(%cam);
}

// * pastes the selection at the same place (used to move obj from a group to another)
function WorldEditor::moveSelectionInPlace(%this)
{
   %saveDropType = %this.dropType;
   %this.dropType = "atCentroid";
   %this.copySelection();
   %this.deleteSelection();
   %this.pasteSelection();
   %this.dropType = %saveDropType;
}

function WorldEditor::addSelectionToAddGroup(%this)
{
   for(%i = 0; %i < %this.getSelectionSize(); %i++) {
      %obj = %this.getSelectedObject(%i);
      $InstantGroup.add(%obj);
   }

}
// resets the scale and rotation on the selection set
function WorldEditor::resetTransforms(%this)
{
   %this.addUndoState();

   for(%i = 0; %i < %this.getSelectionSize(); %i++)
   {
      %obj = %this.getSelectedObject(%i);
      %transform = %obj.getTransform();

      %transform = setWord(%transform, 3, "0");
      %transform = setWord(%transform, 4, "0");
      %transform = setWord(%transform, 5, "1");
      %transform = setWord(%transform, 6, "0");

      //
      %obj.setTransform(%transform);
      %obj.setScale("1 1 1");
   }
}


function WorldEditorToolbarDlg::init(%this)
{
   WorldEditorInspectorCheckBox.setValue(WorldEditorToolFrameSet.isMember("EditorToolInspectorGui"));
   WorldEditorMissionAreaCheckBox.setValue(WorldEditorToolFrameSet.isMember("EditorToolMissionAreaGui"));
   WorldEditorTreeCheckBox.setValue(WorldEditorToolFrameSet.isMember("EditorToolTreeViewGui"));
   WorldEditorCreatorCheckBox.setValue(WorldEditorToolFrameSet.isMember("EditorToolCreatorGui"));
}

function Creator::init( %this )
{
   %this.clear();

   $InstantGroup = "MissionGroup";

   // ---------- INTERIORS
   %base = %this.addGroup( 0, "Interiors" );

   // walk all the interiors and add them to the correct group
   %interiorId = "";
   %file = findFirstFile( "*.dif" );

   while( %file !$= "" )
   {
      // Determine which group to put the file in
      // and build the group heirarchy as we go
      %split    = strreplace(%file, "/", " ");
      %dirCount = getWordCount(%split)-1;
      %parentId = %base;

      for(%i=0; %i<%dirCount; %i++)
      {
         %parent = getWords(%split, 0, %i);
         // if the group doesn't exist create it
         if ( !%interiorId[%parent] )
            %interiorId[%parent] = %this.addGroup( %parentId, getWord(%split, %i));
         %parentId = %interiorId[%parent];
      }
      // Add the file to the group
      %create = "interior" TAB %file;
      %this.addItem( %parentId, fileBase( %file ), %create );

      %file = findNextFile( "*.dif" );
   }


   // ---------- SHAPES - add in all the shapes now...
   %base = %this.addGroup(0, "Shapes");
   %dataGroup = "DataBlockGroup";

   for(%i = 0; %i < %dataGroup.getCount(); %i++)
   {
      %obj = %dataGroup.getObject(%i);
      //HiGuy: Console spaaaam!
//      echo ("Obj: " @ %obj.getName() @ " - " @ %obj.category );
      if(%obj.category !$= "" || %obj.category != 0)
      {
         %grp = %this.addGroup(%base, %obj.category);
         %this.addItem(%grp, %obj.getName(), "create" TAB %obj.getClassName() TAB %obj.getName());
      }
   }


   // ---------- Static Shapes
   %base = %this.addGroup( 0, "Static Shapes" );

   // walk all the statics and add them to the correct group
   %staticId = "";
   %file = findFirstFile( "*.dts" );
   while( %file !$= "" )
   {
      // Determine which group to put the file in
      // and build the group heirarchy as we go
      %split    = strreplace(%file, "/", " ");
      %dirCount = getWordCount(%split)-1;
      %parentId = %base;

      for(%i=0; %i<%dirCount; %i++)
      {
         %parent = getWords(%split, 0, %i);
         // if the group doesn't exist create it
         if ( !%staticId[%parent] )
            %staticId[%parent] = %this.addGroup( %parentId, getWord(%split, %i));
         %parentId = %staticId[%parent];
      }
      // Add the file to the group
      %create = "TSStatic" TAB %file;
      %this.addItem( %parentId, fileBase( %file ), %create );

      %file = findNextFile( "*.dts" );
   }


   // *** OBJECTS - do the objects now...
   // Matan: Mission/Environment only got 1 code each remaining in them so we'll show those.
   // See below to see which code we left in each bit.
   %objGroup[0] = "Environment";
   %objGroup[1] = "Mission";
   %objGroup[2] = "System";
   //%objGroup[3] = "AI";

//   %Environment_Item[0] = "Sky";
//   %Environment_Item[1] = "Sun";
//   %Environment_Item[2] = "Lightning";
//   %Environment_Item[3] = "Water";
//   %Environment_Item[4] = "Terrain";
//   %Environment_Item[5] = "AudioEmitter";
//   %Environment_Item[6] = "Precipitation";
// Matan: We don't use the above anymore, so the one below does not need to be in that order anymore.
//   %Environment_Item[7] = "ParticleEmitter";
   %Environment_Item[0] = "Sky";
   %Environment_Item[1] = "Sun";
   %Environment_Item[2] = "AudioEmitter";
   %Environment_Item[3] = "ParticleEmitter";

//   %Mission_Item[0] = "MissionArea";
//   %Mission_Item[1] = "Marker";
// Matan: We don't use the above anymore, so the one below does not need to be in that order anymore.
//   %Mission_Item[2] = "Trigger";
//   %Mission_Item[3] = "PhysicalZone";
//   %Mission_Item[4] = "Camera";
   //%Mission_Item[5] = "GameType";
   //%Mission_Item[6] = "Forcefield";
   %Mission_Item[0] = "MissionArea";
   %Mission_Item[1] = "Marker";
   %Mission_Item[2] = "Trigger";
   %Mission_Item[3] = "Camera";

   %System_Item[0] = "SimGroup";

   //%AI_Item[0] = "Objective";
   //%AI_Item[1] = "NavigationGraph";

   // objects group
   %base = %this.addGroup(0, "Mission Objects");

   // create 'em
   for(%i = 0; %objGroup[%i] !$= ""; %i++)
   {
      %grp = %this.addGroup(%base, %objGroup[%i]);

      %groupTag = "%" @ %objGroup[%i] @ "_Item";

      %done = false;
      for(%j = 0; !%done; %j++)
      {
         eval("%itemTag = " @ %groupTag @ %j @ ";");
         if(%itemTag $= "")
            %done = true;
         else
            %this.addItem(%grp, %itemTag, "build" TAB %itemTag);
      }
   }
}

function createInterior(%name)
{
   %obj = new InteriorInstance()
   {
      position = "0 0 0";
      rotation = "0 0 0";
      interiorFile = %name;
   };

   return(%obj);
}

function Creator::onAction(%this)
{
//   %this.currentSel = -1;
//   %this.currentRoot = -1;
//   %this.currentObj = -1;

   %sel = %this.getSelected();
   if(%sel == -1 || %this.isGroup(%sel))
      return;

   // the value is the callback function..
   if(%this.getValue(%sel) $= "")
      return;

//   %this.currentSel = %sel;
//   %this.currentRoot = %this.getRootGroup(%sel);

   %val = %this.getValue(%sel);

   %action = getField(%val, 0);
   %rest = getFields(%val, 1, getFieldCount(%val));

   commandToServer('Create', %action, %rest);
}

function Creator::create(%this, %obj)
{
   if(%obj == -1 || %obj == 0)
      return;

//   %this.currentObj = %obj;

   $InstantGroup.add(%obj);

   // drop it from the editor - only SceneObjects can be selected...
   EWorldEditor.clearSelection();
   EWorldEditor.selectObject(%obj);
   EWorldEditor.dropSelection();
}

function serverCmdCreate(%client, %type, %value) {
   switch$ (%type) {
   case "interior":
      %obj = createInterior(%value);
   case "create":
      %data = (getField(%value, 0));
      %obj = eval(alphanum(%data) @ "::create(" @ alphaNum(getField(%value, 1)) @ ");");
   case "TSStatic":
      %obj = TSStatic::create(%value);
   case "build":
		ObjectBuilderGui.call("build" @ %value);
   }

   if (%client.isHost()) {
      EWorldEditor.clearSelection();
		EWorldEditor.selectObject(%obj);
		EWorldEditor.dropSelection();
   } else {
      echo("Client" SPC %client.nameBase SPC "creating an object!");
      %client.createItem = %obj;
      %obj.position = getRandom() SPC getRandom() SPC getRandom();
      MissionGroup.add(%obj);
      commandToClient(%client, 'Create', %obj.position);
   }
}

function clientCmdCreate(%position, %tries) {
   if (%tries > 20) {
      MessageBoxOk("Could not create!", "There was an error creating the object!");
      return;
   }

   //Find it
   updateClientHandicapItems();
   %obj = findObjectAtPosition(%position);

   if (isObject(%obj)) {
      Inspector.inspect( %obj );
      Inspector.object = %obj;
   } else {
      schedule(100, 0, clientCmdCreate, %position, %tries + 1);
   }
}

function serverCmdCreateItemUpdate(%client, %field, %value) {
   %obj = %client.createItem;
   if (!isObject(%obj))
      return;

   echo("Client is setting obj" SPC %obj SPC %field SPC "to" SPC %value);

   eval(%obj @ "." @ alphaNum(%field) @ " = \"" @ expandEscape(%value) @ "\";");
}

function TSStatic::create(%shapeName)
{
   %obj = new TSStatic()
   {
      shapeName = %shapeName;
   };
   return(%obj);
}

function TSStatic::damage(%this)
{
   // prevent console error spam
}

//--------------------------------------
function strip(%stripStr, %strToStrip)
{
   %len = strlen(%stripStr);
   if(strcmp(getSubStr(%strToStrip, 0, %len), %stripStr) == 0)
      return getSubStr(%strToStrip, %len, 100000);
   return %strToStrip;
}

function getPrefSetting(%pref, %default)
{
   //
   if(%pref $= "")
      return(%default);
   else
      return(%pref);
}

//------------------------------------------------------------------------------

function Editor::open(%this)
{
   //HiGuy: Load Prefs
   EditorGui.getPrefs();

   %this.prevContent = Canvas.getContent();
   Canvas.setContent(EditorGui);

   if (MissionInfo.gameMode $= "hunt")
      resetGems();

   $Editor::Opened = true;
   commandToAll('GameStatus', $Editor::Opened);
}

function Editor::close(%this)
{
   //HiGuy: Save prefs
   EditorGui.setPrefs();

   if(%this.prevContent == -1 || %this.prevContent $= "")
      %this.prevContent = "PlayGui";

   Canvas.setContent(%this.prevContent);

   MessageHud.close();
}

//------------------------------------------------------------------------------

// From now on, if your moving platform's PathedInterior does not have default position/rotation/scale values, their basePosition/Rotation/Scale equivalent(s) will automatically get set

// This function will do the above. It allows the user to alter a moving platform in the editor, and have the changes actually be reflected in gameplay
function updatePathedInteriorBaseTransforms() {
   // We will find all of the MustChange groups (they should contain PathedInteriors)
   for (%i = 0; %i < MissionGroup.getCount(); %i++) {
      %obj = MissionGroup.getObject(%i); // Get the next object from MissionGroup
      if (%obj.getName() $= "MustChange_g") { // Follow
         // Now we'll search for a PathedInterior inside here
         %count = %obj.getCount(); // Just so we can reuse %obj
         for (%j = 0; %j < %count; %j++) {
            %obk = %obj.getObject(%j); // Get the next object from the current MustChange group
            if (%obk.getClassName() $= "PathedInterior") { // Check if it's a PathedInterior
               if (%obk.position !$= "0 0 0")
                  %obk.basePosition = %obk.position;
               if (%obk.rotation !$= "1 0 0 0")
                  %obk.baseRotation = %obk.rotation;
               if (%obk.scale !$= "1 1 1")
                  %obk.baseScale = %obk.scale;
            }
         }
      }
   }
}

//------------------------------------------------------------------------------

function generateWorldBox() {
   %box = MissionGroup.getWorldBox();
   %pos = getWord(%box, 0) - 20 SPC getWord(%box, 4) + 20 SPC getWord(%box, 2);
   %scale = (getWord(%box, 3) - getWord(%box, 0) + 40) SPC
            (getWord(%box, 4) - getWord(%box, 1) + 40) SPC
            max(($Server::ServerType $= "Multiplayer" ? 1000 : 0), (getWord(%box, 5) - getWord(%box, 2)));

   new Trigger(Bounds) {
      position = %pos;
      scale = %scale;
      rotation = "1 0 0 0";
      dataBlock = "InBoundsTrigger";
      polyhedron = "0.0000000 0.0000000 0.0000000 1.0000000 0.0000000 0.0000000 0.0000000 -1.0000000 0.0000000 0.0000000 0.0000000 1.0000000";
   };
   MissionGroup.add(Bounds);
}

function SimGroup::getWorldBox(%this) {
   if (!isObject(%this))
      return "";
   //HiGuy: SimGroups will return the bounding box of their contents
   %mx = ""; %my = ""; %mz = ""; %MMx = ""; %MMy = ""; %MMz = "";
   for (%i = 0; %i < %this.getCount(); %i ++) {
      //HiGuy: Get each object's bounding box and extend the current box to fit
      %obj = %this.getObject(%i);

      //HiGuy: We only want the usable objects for world boxes
      //HiGuy: Fun fact: the world box of Sky is "-1B -1B -1B +1B +1B +1B"
      %class = %obj.getClassName();
      //HiGuy: Obtained via dumpConsoleClasses()
      if (%class $= "Sky"    || %class $= "ScriptObject" || %class $= "Marble"
       || %class $= "Sun"    || %class $= "AudioProfile" || %class $= "fxLight"
       || %class $= "Camera" || %class $= "PhysicalZone" || %class $= "fxLightDB"
       || %class $= "Player" || %class $= "MissionArea"  || %class $= "Debris")
         continue;
      if (%class $= "StaticShape" && %obj.getDataBlock().className $= "Skies")
         continue;
      %box = %obj.getWorldBox();

      //HiGuy: Update lower corner
      if (%mx > getWord(%box, 0) || %mx $= "")
         %mx = getWord(%box, 0);
      if (%my > getWord(%box, 1) || %my $= "")
         %my = getWord(%box, 1);
      if (%mz > getWord(%box, 2) || %mz $= "")
         %mz = getWord(%box, 2);

      //HiGuy: Update upper corner
      if (%MMx < getWord(%box, 3) || %MMx $= "")
         %MMx = getWord(%box, 3);
      if (%MMy < getWord(%box, 4) || %MMy $= "")
         %MMy = getWord(%box, 4);
      if (%MMz < getWord(%box, 5) || %MMz $= "")
         %MMz = getWord(%box, 5);
   }
   //HiGuy: Format it as a world box
   return %mx SPC %my SPC %mz SPC %MMx SPC %MMy SPC %MMz;
}

function EWorldEditor::makeGemGroup(%this) {
   if (!isObject(GemGroups))
      MissionGroup.add(new SimGroup(GemGroups));
   GemGroups.add(%group = new SimGroup());
   for (%i = 0; %i < %this.getSelectionSize(); %i ++) {
      %obj = %this.getSelectedObject(%i);
      if (%obj.getClassName() $= "Item" && %obj.getDataBlock().className $= "Gem")
         %group.add(%obj);
   }
}

function EWorldEditor::destroyGemGroups(%this) {
   if (%this.getSelectionSize()) {
      for (%i = 0; %i < %this.getSelectionSize(); %i ++) {
         %obj = %this.getSelectedObject(%i);
         if (%obj.getGroup().getName() $= "GemGroups") {
            while (%obj.getCount())
               MissionGroup.add(%obj.getObject(0));
            %obj.delete();
         }
      }
   } else if (isObject(GemGroups)) {
      for (%i = 0; %i < GemGroups.getCount(); %i ++) {
         %obj = GemGroups.getObject(%i);
         while (%obj.getCount())
            MissionGroup.add(%obj.getObject(0));
         %obj.delete();
      }
      GemGroups.delete();
   }
}

//-----------------------------------------------------------------------------

function EWorldEditor::dropAtGround(%this) {
   if (%this.getSelectionSize()) {
      for (%i = 0; %i < %this.getSelectionSize(); %i ++) {
         %obj = %this.getSelectedObject(%i);
         %diff = getWord(%obj.getWorldBoxCenter(), 2) - getWord(%obj.position, 2);
         %top = getWords(%obj.position, 0, 1) SPC getWord(%obj.getWorldBox(), 5);
         %down = VectorSub(%top, "0 0 10");
         %ray = ContainerRayCast(%top, %down, $TypeMasks::InteriorObjectType | $TypeMasks::StaticShapeObjectType, %obj);
         if (%ray) {
            %pos = getWords(%ray, 1, 3);
            %obj.setTransform(VectorSub(VectorAdd(%pos, "0 0" SPC (getWord(%obj.getWorldBox(), 5) - getWord(%obj.getWorldBox(), 2)) / 2), "0 0" SPC %diff) SPC getWords(%obj.getTransform(), 3, 6));
         }
      }
   }
}

function EWorldEditor::roundCoords(%this) {
   if (%this.getSelectionSize()) {
      for (%i = 0; %i < %this.getSelectionSize(); %i ++) {
         %obj = %this.getSelectedObject(%i);
         %pos = %obj.position;
         %pos = mRound(getWord(%pos, 0) / %this.mouseMoveScale) * %this.mouseMoveScale SPC mRound(getWord(%pos, 1) / %this.mouseMoveScale) * %this.mouseMoveScale SPC mRound(getWord(%pos, 2) / %this.mouseMoveScale) * %this.mouseMoveScale;
         %obj.setTransform(%pos SPC getWords(%obj.getTransform(), 3, 6));
      }
   }
}

function EWorldEditor::malign(%this) {
   if (%this.getSelectionSize()) {
      for (%i = 0; %i < %this.getSelectionSize(); %i ++) {
         %obj = %this.getSelectedObject(%i);
         %pos = %obj.position;
         %pos = getWord(%pos, 0)+(-0.5+getRandom()) SPC getWord(%pos, 1)+(-0.5+getRandom()) / 2 SPC getWord(%pos, 2)+(-0.5+getRandom());
         %obj.setTransform(%pos SPC getWords(%obj.getTransform(), 3, 6));
      }
   }
}
