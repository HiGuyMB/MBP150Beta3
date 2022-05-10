//-----------------------------------------------------------------------------
// Transparency.cs
//
// Because Andrew's PoS computer can't handle GuiBitmapBorderCtrls. (sigh)
// They were a pain to implement, and they worked SO NICELY TOO!
//-----------------------------------------------------------------------------

if(!isObject(GuiTransparencyProfile)) new GuiControlProfile(GuiTransparencyProfile)
{
   border = false;
   opaque = false;
   fillColor = "0 0 0 0";
};


if(!isObject(GuiTransparency75Profile)) new GuiControlProfile(GuiTransparency75Profile)
{
   border = false;
   opaque = false;
   fillColor = "0 0 0 0";
};

if(!isObject(GuiTransparencyTextProfile)) new GuiControlProfile(GuiTransparencyTextProfile)
{
   border = false;
   opaque = false;
   fillColor = "0 0 0 0";
};


if(!isObject(GuiTransparencyText75Profile)) new GuiControlProfile(GuiTransparencyText75Profile)
{
   border = false;
   opaque = false;
   fillColor = "0 0 0 0";
};

package SexyTransparency {
   function GuiControl::onAdd(%this) {
      %this.checkTransparency();
   }
   function GuiControl::checkTransparency(%this) {
      %transType = "";
      //HiGuy: Different types of transparency get different profiles
      if (%this.profile $= "GuiTransparencyProfile")
         %transType = "50";
      if (%this.profile $= "GuiTransparency75Profile")
         %transType = "75";
      if (%transType $= "")
         return;
      if (%this.hasTransparency)
         return;
      %extent = %this.extent;

      //HiGuy: Scoot it out 2 pixels for the completely transparent bits
      %extent = VectorAdd(%extent, "2 2");
      %this.minExtent = "27 27";
      %this.hasTransparency = true;

      %trans = new GuiControl() {
         profile = "GuiDefaultProfile";
         horizSizing = "width";
         vertSizing = "height";
         position = "-1 -1";
         minExtent = "29 29";
         extent = %extent;
         visible = "1";
            transparency = "1";

         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "right";
            vertSizing = "bottom";
            position = "0 0";
            extent = "14 14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-TL";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "left";
            vertSizing = "bottom";
            position = getWord(%extent, 0) - 14 SPC "0";
            extent = "14 14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-TR";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "right";
            vertSizing = "top";
            position = "0" SPC getWord(%extent, 1) - 14;
            extent = "14 14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-BL";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "left";
            vertSizing = "top";
            position = getWord(%extent, 0) - 14 SPC getWord(%extent, 1) - 14;
            extent = "14 14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-BR";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "right";
            vertSizing = "height";
            position = "0 14";
            extent = "14" SPC getWord(%extent, 1) - 28;
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-L";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "width";
            vertSizing = "bottom";
            position = "14 0";
            extent = getWord(%extent, 0) - 28 SPC "14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-T";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "left";
            vertSizing = "height";
            position = getWord(%extent, 0) - 14 SPC "14";
            extent = "14" SPC getWord(%extent, 1) - 28;
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-R";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "width";
            vertSizing = "top";
            position = "14" SPC getWord(%extent, 1) - 14;
            extent = getWord(%extent, 0) - 28 SPC "14";
            visible = "1";
            bitmap = "./" @ %transType @ "/transparency-B";
         };
         new GuiBitmapCtrl() {
            profile = "GuiDefaultProfile";
            horizSizing = "width";
            vertSizing = "height";
            position = "14 14";
            extent = getWord(%extent, 0) - 28 SPC getWord(%extent, 1) - 28;
            visible = "1";
            bitmap = "./" @ %transType @ "/transparencyfill";
         };
      };
      %this.add(%trans);
      %this.bringToFront(%trans);
   }
   function GuiControl::onInspectApply(%this) {
      %this.checkTransparency();
   }
};

activatePackage(SexyTransparency);
