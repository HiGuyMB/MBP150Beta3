//------------------------------------------------------------------------------
// Multiplayer Package
// clientChat.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

function onServerChat(%user, %message) {
   if (getSubStr(%message, 0, 1) $= "/") {
      %cmd = getSubStr(firstWord(%message), 1, strlen(%message));

      //HiGuy: Vtaunt
      if (%cmd $= "vtaunt") {
         %tnum = getWord(%message, 1);
         MPAddServerChat(LBResolveName(%user) @ ":" SPC tauntText(%tnum));
         playTaunt(%tnum);
         return;
      } else if (getSubStr(%cmd, 0, 1) $= "vtaunt") {
         %tnum = getSubStr(%cmd, 6, strlen(%cmd));
         MPAddServerChat(LBResolveName(%user) @ ":" SPC tauntText(%tnum));
         playTaunt(%tnum);
         return;
      } else if (getSubStr(%cmd, 0, 1) $= "v") {
         %tnum = getSubStr(%cmd, 1, strlen(%cmd));
         MPAddServerChat(LBResolveName(%user) @ ":" SPC tauntText(%tnum));
         playTaunt(%tnum);
         return;
      }
   }

   if (%user $= "")
      return;
   if (%message $= "")
      MPAddServerChat(%user);
   else
      MPAddServerChat(LBResolveName(%user) @ ":" SPC %message);
}

function mpSendChat(%message) {
	if (%message $= "")
		return;
   commandToServer('PrivateChat', %message);
   onServerChat(LBResolveName($LB::Username), %message);
}

function MPAddServerChat(%message) {
   $MP::ServerChat = $MP::ServerChat @ ($MP::ServerChat $= "" ? "" : "\n") @  "<spush>" @ %message @ "<spop>";

   MPMissionServerChat.setText("<font:Marker Felt:16>" @ LBResolveChatColors($MP::ServerChat, "mp"));
   PG_ServerChatText.setText("<font:Marker Felt:16>" @ LBResolveChatColors($MP::ServerChat, "ingame"));

	if (Canvas.getContent().getName() $= "MPPlayMissionDlg")
		MPMissionServerChat.forceReflow();
	if (Canvas.getContent().getName() $= "PlayGUI")
		PG_ServerChatText.forceReflow();

   MPMissionServerChatScroll.scrollToBottom();
	MPMissionServerChatScroll.schedule(0, scrollToBottom);
	MPMissionServerChatScroll.schedule(100, scrollToBottom);
	MPMissionServerChatScroll.schedule(1000, scrollToBottom);
   PG_ServerChatScroll.scrollToBottom();
	PG_ServerChatScroll.schedule(0, scrollToBottom);
	PG_ServerChatScroll.schedule(100, scrollToBottom);
	PG_ServerChatScroll.schedule(1000, scrollToBottom);
}
