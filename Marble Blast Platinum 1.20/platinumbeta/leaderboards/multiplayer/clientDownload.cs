//-----------------------------------------------------------------------------
// Multiplayer Package
// clientDownload.cs
// Copyright (c) 2013 MBP Team
//
// Jeff: used for downloading mission files
//-----------------------------------------------------------------------------

function downloadMission() {
   commandToServer('RequestMissionFile');
}

// Jeff: incoming new mission file, set-up/reset variables.
function clientCmdMissionFileStart(%file, %packets)
{
   $DownloadMissionFile    = %file;
   $DownloadMissionPackets = %packets;
   $DownloadMissionData    = "";
}

// Jeff: upon receiving a chunk of the mission file.
function clientCmdMissionFileChunk(%chunk, %packet)
{
   $DownloadMissionData = $DownloadMissionData @ %chunk;
   %file = $DownloadMissionFile;
   MPTeamChoiceTitle.setValue("Receiving" SPC fileBase(%file) @ "." @
                          fileExt(%file) SPC "-" SPC
                          mfloor((100 * %packet) / $DownloadMissionPackets)
                          @ "%");
   MPLoadingProgress.setValue(%packet / $DownloadMissionPackets);
}

function clientCmdMissionFileEnd(%file)
{
   // Jeff: sanity check to make sure we are still on the same file
   if (%file !$= $DownloadMissionFile)
      return;

   %fo = new FileObject();
   if (%fo.openForWrite(%file))
   {
      %fo.writeLine($DownloadMissionData);
      MessageBoxOK("Transmission Successful", "The file" SPC $DownloadMissionFile SPC "successfully downloaded.", "", true);
   }
   else
      MessageBoxOK("Transmission Error", "The file" SPC $DownloadMissionFile SPC "was unable to download.", "", true);
   %fo.destroy();
}

// Jeff: if it could not send, inform the client
function clientCmdMissionFileError(%file)
{
   MessageBoxOK("File Transfer Failure", "The file" SPC %file SPC "can not be sent from the server because it is damaged or missing.", "", true);
}


function clientCmdRequireDif(%file, %dif) {
   commandToServer('DifExists', %file, %dif, isFile(%dif));
}
