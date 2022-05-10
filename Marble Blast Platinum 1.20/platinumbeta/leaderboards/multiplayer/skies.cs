// Skybox data

// Cloudy
// DO NOT USE IN MP - Gemlights disappear due to transparency

//datablock StaticShapeData(Cloudy)
//{
//   className = "Skies";
//   category = "Skies";
//   shapefile = $usermods @ "/data/shapes/Skies/Cloudy/Cloudy.dts";
//};
//
//function Cloudy::onAdd(%this, %obj)
//{
//  %obj.playThread(0, "Rotate");
//}


// Clear skies

datablock StaticShapeData(Clear)
{
   className = "Skies";
   category = "Skies";
   shapefile = $usermods @ "/data/shapes/Skies/Clear/Clear.dts";
};

function Clear::onAdd(%this, %obj)
{
  %obj.playThread(0, "Rotate");
}


// Dusk / Twilight

datablock StaticShapeData(Dusk)
{
   className = "Skies";
   category = "Skies";
   shapefile = $usermods @ "/data/shapes/Skies/Dusk/Dusk.dts";
};

function Dusk::onAdd(%this, %obj)
{
  %obj.playThread(0, "Rotate");
}


// Wintry

datablock StaticShapeData(Wintry)
{
   className = "Skies";
   category = "Skies";
   shapefile = $usermods @ "/data/shapes/Skies/Wintry/Wintry.dts";
};

function Wintry::onAdd(%this, %obj)
{
  %obj.playThread(0, "Rotate");
}
