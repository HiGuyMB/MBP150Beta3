//-----------------------------------------------------------------------------
// taunts.cs
//
// Copyright (c) 2013 The Platinum Team
//
// Jeff: taunt list:
//
// 1  - chicken
// 2  - random chars aka Confusion Taunt
// 3  - laughter
// 4  - you are a loser (aka anybody besides matan and xelna)
// 5  - mega marble kids!!
// 6  - MULTIPLAYER WHERe
// 7  - Come on!
// 8  - Pomp
// 9  - PQ WHERe
// 10 - RAISE UR DONGERS
// 11 - You got owned!
// 12 - WORTH IT
// 13 - Do da la la la...
//-----------------------------------------------------------------------------

function resolveTaunt(%name) {
   switch$ (%name) {
   case "chick":  return 1;
   case "asdf":   return 2;
   case "haha":   return 3;
   case "loser":  return 4;
   case "mega":   return 5;
   case "mp":     return 6;
   case "cmon":   return 7;
   case "pomp":   return 8;
   case "pq":     return 9;
   case "raise":  return 10;
   case "owned":  return 11;
   case "worth":  return 12;
   case "lala":   return 13;
   }
   return %name;
}

function tauntText(%number) {
   switch (%number) {
   case  1: return "You chicken? Bwk bwk bwk!";
   case  2: return "Gashfdklafaashn.zx,cbvz.e";
   case  3: return "Hahahahaha!";
   case  4: return "Welcome to loserville, population: You!";
   case  5: return "Mega Marble, Kids!";
   case  6: return "Multiplayer WHERe?";
   case  7: return "Oh, come on!";
   case  8: return "Pomp!";
   case  9: return "PQ WHERe?";
   case 10: return "Raise your dongers!";
   case 11: return "You got owned.";
   case 12: return "WOOOOORTH IT!!";
   case 13: return "Do da la la la la la la la la la la...";
   }
   return "";
}

function tauntFile(%number) {
   %base = $usermods @ "/data/sound/taunts/";
   switch (%number) {
   case  1: return %base @ "Chicken.wav";
   case  2: return %base @ "Asdfasdf.wav";
   case  3: return %base @ "Hahahaha.wav";
   case  4: return %base @ "Loserville.wav";
   case  5: return %base @ "MegaMarble.wav";
   case  6: return %base @ "MPWHERe.wav";
   case  7: return %base @ "Cmon.wav";
   case  8: return %base @ "Pomp.wav";
   case  9: return %base @ "PQWHERe.wav";
   case 10: return %base @ "Dongers.wav";
   case 11: return %base @ "YouGotOwned.wav";
   case 12: return %base @ "WorthIt.wav";
   case 13: return %base @ "DoDaLaLaLa.wav";
   }
}

function playTaunt(%number) {
   if (!isObject(LBTaunt @ %number)) {
      new AudioProfile(LBTaunt @ %number) {
         filename = tauntFile(%number);
         description = "AudioGui";
         preload = true;
      };
   }
   alxPlay(LBTaunt @ %number);
}

function getTaunt(%number, %audio) {

}

function sendTaunt(%number) {
   mpSendChat("/v" @ %number);
}

//-----------------------------------------------------------------------------
// Jeff: keybindings to taunts
//-----------------------------------------------------------------------------

function taunt1(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(1);
}

function taunt2(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(2);
}

function taunt3(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(3);
}

function taunt4(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(4);
}

function taunt5(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(5);
}

function taunt6(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(6);
}

function taunt7(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(7);
}

function taunt8(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(8);
}

function taunt9(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(9);
}

function taunt10(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(10);
}

function taunt11(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(11);
}

function taunt12(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(12);
}

function taunt13(%val)
{
   if (%val && $Server::ServerType $= "Multiplayer")
      sendTaunt(13);
}

// Jeff: bind the default keys
function bindDefaultTauntKeys()
{
   MoveMap.bind(keyboard, 1,  taunt1);
   MoveMap.bind(keyboard, 2,  taunt2);
   MoveMap.bind(keyboard, 3,  taunt3);
   MoveMap.bind(keyboard, 4,  taunt4);
   MoveMap.bind(keyboard, 5,  taunt5);
   MoveMap.bind(keyboard, 6,  taunt6);
   MoveMap.bind(keyboard, 7,  taunt7);
   MoveMap.bind(keyboard, 8,   taunt8);
   MoveMap.bind(keyboard, 9,    taunt9);
   MoveMap.bind(keyboard, 0,     taunt10);
   MoveMap.bind(keyboard, "-",    taunt11);
   MoveMap.bind(keyboard, "+",     taunt12);
   MoveMap.bind(keyboard, "ctrl 1", taunt13);
}
