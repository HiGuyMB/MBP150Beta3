//-----------------------------------------------------------------------------
// Multiplayer Package
// serverDownload.cs
// Copyright (c) 2013 MBP Team
//-----------------------------------------------------------------------------

function serverCmdRequestMissionFile(%client)
{
   //echo("Client" SPC %client.nameBase SPC "is requesting a file transfer.");
   //echo("   File:" SPC $Server::MissionFile);
   %client.sendMissionFile();
}

// Jeff: send a mission file to the client
function GameConnection::sendMissionFile(%this)
{
   %stream = "";

   // Jeff: get the contents of the file
   %fo = new FileObject();
   if (%fo.openForRead($Server::MissionFile))
   {
      while (!%fo.isEOF())
      {
         %line = %fo.readLine();
         if (%stream $= "")
            %stream = %line;
         else
            %stream = %stream NL %line;
      }
   }
   %fo.destroy();

   // Jeff: UH-OH, WE GOT A PROBLEM
   if (%stream $= "")
   {
      error("Could not send mission file" SPC $Server::MissionFile SPC "to Client" SPC %client.nameBase);
      error("   Reason: unable to open the file.  It is either missing or damaged.");

      // Jeff: inform the client as well so they know what the heck happened.
      commandToClient(%this, 'MissionFileError', $Server::MissionFile);
      return;
   }

   // Jeff: TorqueScript packets for remote commands can only have a string
   // length of up to 255 characters
   %count = mCeil(strLen(%stream) / 255);

   // Jeff: inform the client that they are going to be receiving a file
   // send the file's path and name so it knows where to go and the total
   // amounts of packets that it is going to receive.
   commandToClient(%this, 'MissionFileStart', $Server::MissionFile, %count);

   // Jeff: send the contents of the file
   for (%i = 0; %i < %count; %i ++)
   {
      // Jeff: send the content with %send
      // %i + 1 is the amount being sent / total
      // (percent of download completion)
      %send = getSubStr(%stream, %i * 255, 255);
      commandToClient(%this, 'MissionFileChunk', %send, %i + 1);
   }

   // Jeff: inform the client that it sent
   commandToClient(%this, 'MissionFileEnd', $Server::MissionFile);
}

function checkMissionLoad(%file) {
   $Game::RequireDifs[%file] = 0;
   %conts = fread(%file);
   if (%conts $= "")
      return false;

   for (%i = 0; %i < getRecordCount(%conts); %i ++) {
      %line = getRecord(%conts, %i);
      if (strPos(trim(%line), "interiorFile") == 0) {
         //Interior
         %dif = "%" @ trim(getSubStr(%line, 0, strPos(%line, ";"))) @ ";";
         eval(%dif);
         %interiorFile = expandFilename(%interiorFile);
         if (!isFile(%interiorFile))
            return false;

         commandToAll('RequireDif', %file, %interiorFile);
         $Game::RequireDif[%file, $Game::RequireDifs[%file]] = %interiorFile;
         $Game::RequireDifs[%file] ++;
      }
   }
}

function serverCmdDifExists(%client, %file, %dif, %has) {
   %client.hasDif[%file, %dif] = %has;
}

function GameConnection::canLoadMission(%this, %file) {
   for (%i = 0; %i < $Game::RequireDifs[%file]; %i ++) {
      if (!%this.hasDif[%file, $Game::RequireDif[%file, %i]]) {
         echo("Client" SPC %this SPC "missing interior" SPC $Game::RequireDif[%file, %i] SPC "for file" SPC %file);
         return false;
      }
   }
   return true;
}
