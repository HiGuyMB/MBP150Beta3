//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Console onEditorRender functions:
//------------------------------------------------------------------------------
// Functions:
//   - renderSphere([pos], [radius], <sphereLevel>);
//   - renderCircle([pos], [normal], [radius], <segments>);
//   - renderTriangle([pnt], [pnt], [pnt]);
//   - renderLine([start], [end], <thickness>);
//
// Variables:
//   - consoleFrameColor - line prims are rendered with this
//   - consoleFillColor
//   - consoleSphereLevel - level of polyhedron subdivision
//   - consoleCircleSegments
//   - consoleLineWidth
//------------------------------------------------------------------------------

function SpawnSphere::onEditorRender(%this, %editor, %selected, %expanded)
{
   if(%selected $= "true")
   {
      %editor.consoleFrameColor = "255 0 0";
      %editor.consoleFillColor = "0 0 0 0";
      %editor.renderSphere(%this.getWorldBoxCenter(), %this.radius, 1);
   }
}

function AudioEmitter::onEditorRender(%this, %editor, %selected, %expanded)
{
   if(%selected $= "true" && %this.is3D && !%this.useProfileDescription)
   {
      %editor.consoleFillColor = "0 0 0 0";

      %editor.consoleFrameColor = "255 0 0";
      %editor.renderSphere(%this.getTransform(), %this.minDistance, 1);

      %editor.consoleFrameColor = "0 0 255";
      %editor.renderSphere(%this.getTransform(), %this.maxDistance, 1);
   }
}

function renderTest() {
   EWorldEditor.consoleFrameColor = "255 0 0";
   EWorldEditor.consoleFillColor = "0 0 0 0";
   EWorldEditor.renderSphere("0 0 0", 10, 1);
   EWorldEditor.rendercircle("10 0 0", "0 0 0", 1, 360);
}

function PathedInterior::onEditorRender(%this, %editor, %selected, %expanded) {
   if (%selected $= "false")
      return;

   %group = %this.getGroup();
   for (%i = 0; %i < %group.getCount(); %i ++)
      if (%group.getObject(%i).getClassName() $= "Path")
         %group.getObject(%i).onEditorRender(%editor, %selected, %expanded);
}

function Path::onEditorRender(%this, %editor, %selected, %expanded) {
   if (%selected $= "false")
      return;
   if (%this.getCount() == 0)
      return;

  // Make sure we only render this set once per frame
  if ($Editor::LastRender[%this] == $Sim::Time)
    return;

  $Editor::LastRender[%this] = $Sim::Time;

  %time = 999999999999;
  // Test for a TriggerGotoTarget
  %group = %this.getGroup();
  %count = %group.getCount();
  for (%i = 0; %i < %count; %i ++) {
    %obj = %group.getObject(%i);
    if (%obj.getClassName() $= "Trigger" && %obj.getDataBlock().getName() $= "TriggerGotoTarget")
      %time = %obj.targetTime;
  }

  %group = %this;
  %count = %group.getCount();
  for (%i = 1; %i < %count; %i++)
  {
    if (%prevObj $= "")
      %prevObj = %group.getObject(0);

    if (%time < 0)
      break;
    %amt = 1.0;
    %time -= %prevObj.msToNext;
    if (%time < 0) {
      %amt = (%prevObj.msToNext + %time) / %prevObj.msToNext;
    }
    %amt = max(min(1, %amt), 0);

    %obj = %group.getObject(%i);
    %color = (%i / (%count-1)) * 255;
    %colorI = (%count-%i) / %count * 255;

    //echo(%color / 255 SPC %colori / 255);

    %new = VectorAdd(%prevObj.getPosition(), VectorScale(VectorSub(%obj.getPosition(), %prevObj.getPosition()), %amt));

    %editor.consoleFrameColor = "0 0 0";
    %editor.renderLine(%prevObj.getPosition(), %new, 8);
    %editor.consoleFrameColor = (%color * 255 / 255 + 0) SPC (%colorI * 255 / 255 + 0) SPC 100;
    %editor.renderLine(%prevObj.getPosition(), %new, 4);

    %prevObj = %obj;
  }
}

//HiGuy: Yoink'd from PQ
function Marker::onEditorRender(%this, %editor, %selected, %expanded)
{
  if (%selected $= "false")
    return;

  %group = %this.getGroup();
  %group.onEditorRender(%editor, %selected, %expanded);
}

//function Item::onEditorRender(%this, %editor, %selected, %expanded)
//{
//   if(%this.getDataBlock().getName() $= "MineDeployed")
//   {
//      %editor.consoleFillColor = "0 0 0 0";
//      %editor.consoleFrameColor = "255 0 0";
//      %editor.renderSphere(%this.getWorldBoxCenter(), 6, 1);
//   }
//}
