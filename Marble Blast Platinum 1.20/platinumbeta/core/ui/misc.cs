
//package CursorPack {
  //
  //function Canvas::setCursor(%this, %cursor)
  //{
    //Canvas.activeCursor = %cursor;
    //deactivatePackage("CursorPack");
    //Canvas.setCursor(%cursor);
    //activatePackage("CursorPack");
  //}
//};
//activatePackage("CursorPack");

if (getCompileTimeString() $= "Mar 19 2003 at 15:04:04")
  $Version::Blaster = true;

function shadow(%offset, %color)
{
  if ($Version::Blaster)
    return "";
  if (%offset !$= "")
    %string = "<shadow:" @ getWord(%offset, 0) @ ":" @ getWord(%offset, 1) @ ">";
  if (%color !$= "")
  {
    %string = %string @ "<shadowcolor:" @ %color;
    //%string = %string @ baseDecimalToHex(getWord(%color, 0))
                      //@ baseDecimalToHex(getWord(%color, 1))
                      //@ baseDecimalToHex(getWord(%color, 2));
    //if (%alpha !$= "")
      //%string = %string @ baseDecimalToHex(%alpha);
    %string = %string @ ">";
  }
  return %string;
}

function Canvas::getCursor(%this)
{
  //return %this.activeCursor;
  return DefaultCursor;
}

function vectorAdd2(%a, %b)
{
  %a1 = getWord(%a, 0); %a2 = getWord(%a, 1);
  %b1 = getWord(%b, 0); %b2 = getWord(%b, 1);
  return %a1 + %b1 SPC %a2 + %b2;
}

function vectorSub2(%a, %b)
{
  %a1 = getWord(%a, 0); %a2 = getWord(%a, 1);
  %b1 = getWord(%b, 0); %b2 = getWord(%b, 1);
  return %a1 - %b1 SPC %a2 - %b2;
}

// Returns the lowest GUI object that isn't obstructed by other GUIs

function Canvas::getCursorContent(%this)
{
  %active = %this.getObject(%this.getCount()-1);
  return %active.cursorLowestMember();
}

function GuiControl::cursorLowestMember(%this, %offset)
{
  if (%offset $= "")
    %offset = "0 0";  //0 0 is top left corner

  //Guis are layered from the bottom up, so start at the end of the list
  for (%i = %this.getCount()-1; %i > -1; %i--)
  {
    %obj = %this.getObject(%i);
    if (!%obj.isVisible())
      continue;
    if (%obj.isCursorOn(%offset))
    {
      //The cursor is over this child gui,
      //and it has no children of its own; stop here.
      if (%obj.getCount() == 0)
        return %obj;
      //if it does have children, recursion time
      else
        return %obj.cursorLowestMember(vectorAdd2(%offset, %obj.position));
    }
  }
  //The cursor isn't over any of this gui's children
  return %this;
}

function GuiControl::isHover(%this) {
   return Canvas.getCursorContent().getId() == %this.getId();
}

// Save the calculated offset for this long (ms)
$_offdecaymax = 2000;

// Check whether the cursor is within the boundaries of a specific GUI

function GuiControl::isCursorOn(%this, %offset, %extent)
{
  if (%offset $= "")
  {
    if ($_offdecay[%this] && getRealTime() - $_offdecay[%this] > $_offdecaymax) {
      $_off[%this] = "";
      $_offdecay[%this] = 0;
    }
    if ($_off[%this] !$= "")
      %offset = $_off[%this];
    else {
      %obj = %this;
      while(true)
      {
        %group = %obj.getGroup();
        if (%group == -1)
        {
          warn(%this @ ".isCursorOn() - Gui is not active");
          return 0;
        }
        if (%group == Canvas.getID())
          break;
        %offset = vectorAdd2(%offset, %group.position);
        %obj = %group;
      }

      $_off[%this] = %offset;
      $_offdecay[%this] = getRealTime();
    }
  }
  if (%extent $= "")
    %extent = %this.extent;
  if (getWord(%extent, 0) $= "-")
    %extent = setWord(%extent, 0, getWord(%this.extent, 0));
  if (getWord(%extent, 1) $= "-")
    %extent = setWord(%extent, 1, getWord(%this.extent, 1));

  %cursorPos = vectorAdd2(Canvas.getCursorPos(), Canvas.getCursor().hotspot);
  %lowBound = vectorAdd2(%this.position, %offset);
  %highBound = vectorAdd2(%lowBound, %this.extent);
  %subtract = vectorSub2(%this.extent, %extent);

  %cursorPosX = getWord(%cursorPos, 0);
  %cursorPosY = getWord(%cursorPos, 1);
  %lowBoundX = getWord(%lowBound, 0);
  %lowBoundY = getWord(%lowBound, 1);
  %highBoundX = getWord(%highBound, 0) - getWord(%subtract, 0);
  %highBoundY = getWord(%highBound, 1) - getWord(%subtract, 1);

  return (%cursorPosX >= %lowBoundX  &&
          %cursorPosX <  %highBoundX &&
          %cursorPosY >= %lowBoundY  &&
          %cursorPosY <  %highBoundY);
}

//-------------------------------------------------


function GuiControl::setExtent(%gui, %extent)
{
  %p1 = getWord(%gui.position, 0);
  %p2 = getWord(%gui.position, 1);
  %e1 = getWord(%extent, 0);
  %e2 = getWord(%extent, 1);

  %gui.resize(%p1, %p2, %e1, %e2);
}

function GuiControl::setPosition(%gui, %position)
{
  %p1 = getWord(%position, 0);
  %p2 = getWord(%position, 1);
  %e1 = getWord(%gui.extent, 0);
  %e2 = getWord(%gui.extent, 1);

  %gui.resize(%p1, %p2, %e1, %e2);
}

function GuiControl::resize2(%gui, %values)
{
  %gui.resize(getWord(%values, 0), getWord(%values, 1), getWord(%values, 2), getWord(%values, 3));
}
