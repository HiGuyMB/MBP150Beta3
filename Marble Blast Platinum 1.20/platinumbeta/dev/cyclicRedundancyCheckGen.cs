//-----------------------------------------------------------------------------
// cyclicRedundancyCheckGen.cs
// Copyright (c) The Platinum Team
//-----------------------------------------------------------------------------

function GenCRC() {
   %debugCount = 0;
   %primer  = "   %b = 0;";
   %cs = ""; %gui = ""; %mis = ""; %dts = ""; %dif = "";
   %csFile = ""; %guiFile = ""; %misFile = ""; %dtsFile = ""; %difFile = "";
   %crcFile = $usermods @ "/client/scripts/redundancycheck.cs";
   %fo = new FileObject();
   MessageBoxOk("Generating CRC", "Generating CRC for file:\n<n/a>", "", true);
   Canvas.repaint();
   %index = 0;
   %count[0] = 0; //cs
   %count[1] = 0; //gui
   %count[2] = 0; //mis
   %count[3] = 0; //dts
   %count[4] = 0; //dif
   findFirstFile("*.cs.dso");
   while(findNextFile("*.cs.dso")!$="")%count[0]++;
      findFirstFile("*.gui.dso");
   while(findNextFile("*.gui.dso")!$="")%count[1]++;
      findFirstFile("*.mis");
   while((%f=findNextFile("*.mis"))!$=""){if(strlwr(strPos(%f,"lbmissions")==-1)){if(strlwr(strpos(%f,"lb_custom")==-1))continue;}%count[2]++;}
      findFirstFile("*.dts");
   while(findNextFile("*.dts")!$="")%count[3]++;
      findFirstFile("*.dif");
   //Sanity! Created via insanity! Whenceforth doth this heathen beseech us.
   //Ok. Too much shakespeare. *Hits head repeatedly*
   while((%f=findNextFile("*.dif"))!$=""){if(strlwr(strPos(%f,"lbinterior")==-1)){continue;}%count[4]++;}

   %count[-1] = %count[0] + %count[1] + %count[2] + %count[3] + %count[4];

   %csDirs = 0;
   %guiDirs = 0;
   %misDirs = 0;
   %dtsDirs = 0;
   %difDirs = 0;

   %csDirFile = "";

   // Jeff: cs extension
   %csExt = "   %e = \".cs.dso\";";

   for (%f = findFirstFile("*.cs.dso"); %f !$= ""; %f = findNextFile("*.cs.dso")) {
      if (strpos(%f, "/custom") != -1)
         continue;
      %counter ++;
      %countall ++;
      if (%index % 10 == 0) {
         MBSetText(MBOKText, MBOKFrame, "Generating CRC for script:\n" @ fileBase(%f) @ "\n\n" @ %counter SPC "/" SPC %count[0] NL %countall SPC "/" SPC %count[-1]);
         Canvas.repaint();
      }
      if (%f $= (%crcFile @ ".dso"))    continue;
      if (strStr(%f,"Pref") != -1)      continue;
      if (strStr(%f,"pref") != -1)      continue;
      if (strStr(%f,"config") != -1)    continue;
      if (strStr(%f,"demo") != -1)      continue;
      if (strStr(%f,"ignition") != -1)  continue;
      if (strStr(%f, "dev") != -1)      continue;
      if (strStr(%f, "debugger") != -1) continue;
      if (strStr(%f,"spy47") != -1)     continue; // Jeff: spy47 old CRC file

      %crc = getFileCRC(%f);
      if (%csDir[filePath(%f)] $= "") {
         %csDir[filePath(%f)] = %csDirs;
         if (%csDirFile $= "")
            %csDirFile = "   %d[" @ %csDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         else
            %csDirFile = %csDirFile NL "   %d[" @ %csDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         %csDirs ++;
      }

      // Jeff: correct the base, strip extensions
      %base = fileBase(%f);
      %dotPos = strpos(%base, ".cs");
      %base = getSubStr(%base, 0, %dotPos);

      if (%csFile $= "")
         %csFile = "   %f[" @ %index @ "] = %d[" @ %csDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      else
         %csFile = %csFile NL "   %f[" @ %index @ "] = %d[" @ %csDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      %index ++;
   }
   %csFile = %csExt NL %csDirFile NL %csFile;
   %cs = "   for (%i = 0; %i < " @ %index @ "; %i ++) {" NL
          "      if (getFileCRC(getField(%f[%i],0)) !$= getField(%f[%i],1)) {" NL
          "         %c[%b] = getField(%f[%i],0);" NL
          "         %b ++;" NL
          "      }" NL
          "   }";
   %index = 0;
   %counter = 0;
   %guiDirFile = "";

   %guiExt = "   %e = \".gui.dso\";";

   for (%f = findFirstFile("*.gui.dso"); %f !$= ""; %f = findNextFile("*.gui.dso")) {
      %counter ++;
      %countall ++;
      if (%index % 10 == 0) {
         MBSetText(MBOKText, MBOKFrame, "Generating CRC for gui:\n" @ fileBase(%f) @ "\n\n" @ %counter SPC "/" SPC %count[1] NL %countall SPC "/" SPC %count[-1]);
         Canvas.repaint();
      }
      if (strpos(%f, "/custom") != -1)
         continue;

      if (strStr(%f,"ignition") != -1)  continue;
      if (strStr(%f,"dev") != -1)       continue;
      if (strStr(%f, "debugger") != -1) continue;
      if (strStr(%f, "cache") != -1) continue;
      %crc = getFileCRC(%f);
      if (%guiDir[filePath(%f)] $= "") {
         %guiDir[filePath(%f)] = %guiDirs;
         if (%guiDirFile $= "")
            %guiDirFile = "   %d[" @ %guiDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         else
            %guiDirFile = %guiDirFile NL "   %d[" @ %guiDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         %guiDirs ++;
      }

      // Jeff: correct the base, strip extensions
      %base = fileBase(%f);
      %dotPos = strpos(%base, ".gui");
      %base = getSubStr(%base, 0, %dotPos);

      if (%guiFile $= "")
         %guiFile = "   %f[" @ %index @ "] = %d[" @ %guiDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      else
         %guiFile = %guiFile NL "   %f[" @ %index @ "] = %d[" @ %guiDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      %index ++;
   }
   %guiFile = %guiExt NL %guiDirFile NL %guiFile;
   %gui = "   for (%i = 0; %i < " @ %index @ "; %i ++) {" NL
          "      if (getFileCRC(getField(%f[%i],0)) !$= getField(%f[%i],1)) {" NL
          "         %c[%b] = getField(%f[%i],0);" NL
          "         %b ++;" NL
          "      }" NL
          "   }";
   %index = 0;
   %counter = 0;
   %misDirFile = "";

   %misExt = "   %e = \".mis\";";

   for (%f = findFirstFile("*.mis"); %f !$= ""; %f = findNextFile("*.mis")) {
      if (strlwr(strPos(%f,"lbmissions") == -1)) {
			if (strlwr(strpos(%f,"lb_custom") == -1))
				continue;
		}
      if (strpos(%f, "/custom") != -1)
         continue;

		%counter ++;
      %countall ++;
      if (%index % 10 == 0) {
         MBSetText(MBOKText, MBOKFrame, "Generating CRC for mission:\n" @ fileBase(%f) @ "\n\n" @ %counter SPC "/" SPC %count[2] NL %countall SPC "/" SPC %count[-1]);
         Canvas.repaint();
      }

      %crc = getFileCRC(%f);
      if (%misDir[filePath(%f)] $= "") {
         %misDir[filePath(%f)] = %misDirs;
         if (%misDirFile $= "")
            %misDirFile = "   %d[" @ %misDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         else
            %misDirFile = %misDirFile NL "   %d[" @ %misDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         %misDirs ++;
      }

      // Jeff: correct the base, strip extensions
      %base = fileBase(%f);

      if (%misFile $= "")
         %misFile = "   %f[" @ %index @ "] = %d[" @ %misDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      else
         %misFile = %misFile NL "   %f[" @ %index @ "] = %d[" @ %misDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      %index ++;
   }
   %misFile = %misExt NL %misDirFile NL %misFile;
   %mis = "   for (%i = 0; %i < " @ %index @ "; %i ++) {" NL
          "      if (getFileCRC(getField(%f[%i],0)) !$= getField(%f[%i],1)) {" NL
          "         %c[%b] = getField(%f[%i],0);" NL
          "         %b ++;" NL
          "      }" NL
          "   }";
   %index = 0;
   %counter = 0;
   // %dtsDirFile = "";

   // %dtsExt = "   %e = \".dts\";";

   // for (%f = findFirstFile("*.dts"); %f !$= ""; %f = findNextFile("*.dts")) {
   //    if (strpos(%f, "/custom") != -1)
   //       continue;
   //    %counter ++;
   //    %countall ++;
   //    if (%index % 10 == 0) {
   //       MBSetText(MBOKText, MBOKFrame, "Generating CRC for shape:\n" @ fileBase(%f) @ "\n\n" @ %counter SPC "/" SPC %count[3] NL %countall SPC "/" SPC %count[-1]);
   //       Canvas.repaint();
   //    }
   //    %crc = getFileCRC(%f);
   //    if (%dtsDir[filePath(%f)] $= "") {
   //       %dtsDir[filePath(%f)] = %dtsDirs;
   //       if (%dtsDirFile $= "")
   //          %dtsDirFile = "   %d[" @ %dtsDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
   //       else
   //          %dtsDirFile = %dtsDirFile NL "   %d[" @ %dtsDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
   //       %dtsDirs ++;
   //    }

   //    // Jeff: correct the base, strip extensions
   //    %base = fileBase(%f);

   //    if (%dtsFile $= "")
   //       %dtsFile = "   %f[" @ %index @ "] = %d[" @ %dtsDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
   //    else
   //       %dtsFile = %dtsFile NL "   %f[" @ %index @ "] = %d[" @ %dtsDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
   //    %index ++;
   // }
   // %dtsFile = %dtsExt NL %dtsDirFile NL %dtsFile;
   // %dts = "   for (%i = 0; %i < " @ %index @ "; %i ++) {" NL
   //        "      if (getFileCRC(getField(%f[%i],0)) !$= getField(%f[%i],1)) {" NL
   //        "         %c[%b] = getField(%f[%i],0);" NL
   //        "         %b ++;" NL
   //        "      }" NL
   //        "   }";
   %index = 0;
   %counter = 0;
   %difDirFile = "";

   %difExt = "   %e = \".dif\";";

   for (%f = findFirstFile("*.dif"); %f !$= ""; %f = findNextFile("*.dif")) {
      if (strlwr(strPos(%f,"lbinterior") == -1)) continue;
      if (strpos(%f, "/custom") != -1)
         continue;

      %counter ++;
      %countall ++;
      if (%index % 50 == 0) {
         MBSetText(MBOKText, MBOKFrame, "Generating CRC for interior:\n" @ fileBase(%f) @ "\n\n" @ %counter SPC "/" SPC %count[4] NL %countall SPC "/" SPC %count[-1]);
         Canvas.repaint();
      }

      %crc = getFileCRC(%f);
      if (%difDir[filePath(%f)] $= "") {
         %difDir[filePath(%f)] = %difDirs;
         if (%difDirFile $= "")
            %difDirFile = "   %d[" @ %difDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         else
            %difDirFile = %difDirFile NL "   %d[" @ %difDirs @ "] = " @ appendString(filePath(%f) @ "/") @ ";";
         %difDirs ++;
      }

      // Jeff: correct the base, strip extensions
      %base = fileBase(%f);

      if (%difFile $= "")
         %difFile = "   %f[" @ %index @ "] = %d[" @ %difDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      else
         %difFile = %difFile NL "   %f[" @ %index @ "] = %d[" @ %difDir[filePath(%f)] @ "] @ " @ appendString(%base) @ " @ %e TAB " @ appendString(%crc) @ ";";
      %index ++;
   }
   %difFile = %difExt NL %difDirFile NL %difFile;
   %dif = "   for (%i = 0; %i < " @ %index @ "; %i ++) {" NL
          "      if (getFileCRC(getField(%f[%i],0)) !$= getField(%f[%i],1)) {" NL
          "         %c[%b] = getField(%f[%i],0);" NL
          "         %b ++;" NL
          "      }" NL
          "   }";
   MBSetText(MBOKText, MBOKFrame, "Writing out...\nPlease Wait");
   Canvas.repaint();

   %crcMain = "//-----------------------------------------------------------------------------" NL
          "// redundancyCheck.cs" NL
          "// Copyright (c) The Platinum Team" NL
          "//-----------------------------------------------------------------------------\n";

   %functionCRC = "function queryCRC() {" NL
          "   %letters = " @ appendString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.-\'\"[],()!$&^#@+=-_;") @ ";" NL
          "   %b = 0;" NL
          "   %data[0] = crcScripts();" NL
          "   %data[1] = crcGuis();" NL
          "   %data[2] = crcMissions();" NL
//          "   %data[3] = crcShapes();" NL
          "   %data[3] = crcDifs();" NL
          "   for (%i = 0; %i < 4; %i ++) {" NL
          "      %b += getField(%data[%i],0);" NL
          "      %blah = getFields(%data[%i],1);" NL
          "      if (%val $= \"\") {" NL
          "         %val = %val @ %blah;" NL
          "      } else {" NL
          "         %val = %val TAB %blah;" NL
          "      }" NL
          "   }" NL
          "   if (%val $= \"\") {" NL
          "      %val = " @ appendString("crcGood") @ ";" NL
          "   }" NL
          "   return %b TAB %val;" NL
          "}";

   %crcCS = "\nfunction crcScripts() {" NL
         %primer NL
         %csFile NL
         %cs NL
          "   %b = (%b > 3 ? 3 : %b);" NL
          "   for (%i = 0; %i < %b; %i ++) {" NL
          "      if (%string $= \"\") %string = %string @ %c[%i];" NL
          "      else %string = %string TAB %c[%i];" NL
          "   }" NL
          "   %val = %b TAB %string;" NL
          "   return %val;" NL
          "}";

   %crcGui = "\nfunction crcGuis() {" NL
         %primer NL
         %guiFile NL
         %gui NL
          "   %b = (%b > 3 ? 3 : %b);" NL
          "   for (%i = 0; %i < %b; %i ++) {" NL
          "      if (%string $= \"\") %string = %string @ %c[%i];" NL
          "      else %string = %string TAB %c[%i];" NL
          "   }" NL
          "   %val = %b TAB %string;" NL
          "   return %val;" NL
          "}";

   %crcMis = "\nfunction crcMissions() {" NL
         %primer NL
         %misFile NL
         %mis NL
          "   %b = (%b > 3 ? 3 : %b);" NL
          "   for (%i = 0; %i < %b; %i ++) {" NL
          "      if (%string $= \"\") %string = %string @ %c[%i];" NL
          "      else %string = %string TAB %c[%i];" NL
          "   }" NL
          "   %val = %b TAB %string;" NL
          "   return %val;" NL
          "}";

   // %crcShapes = "\nfunction crcShapes() {" NL
   //       %primer NL
   //       %dtsFile NL
   //       %dts NL
   //        "   %b = (%b > 3 ? 3 : %b);" NL
   //        "   for (%i = 0; %i < %b; %i ++) {" NL
   //        "      if (%string $= \"\") %string = %string @ %c[%i];" NL
   //        "      else %string = %string TAB %c[%i];" NL
   //        "   }" NL
   //        "   %val = %b TAB %string;" NL
   //        "   return %val;" NL
   //        "}";

   %crcDifs = "\nfunction crcDifs() {" NL
         %primer NL
         %difFile NL
         %dif NL
          "   %b = (%b > 3 ? 3 : %b);" NL
          "   for (%i = 0; %i < %b; %i ++) {" NL
          "      if (%string $= \"\") %string = %string @ %c[%i];" NL
          "      else %string = %string TAB %c[%i];" NL
          "   }" NL
          "   %val = %b TAB %string;" NL
          "   return %val;" NL
          "}";

   if (%fo.openForWrite(%crcFile)) {
      %fo.writeLine(%crcMain);
      %fo.writeLine(%functionCRC);
      %fo.writeLine(%crcCS);
      %fo.writeLine(%crcGui);
      %fo.writeLine(%crcMis);
//      %fo.writeLine(%crcShapes);
      %fo.writeLine(%crcDifs);
   }
   %fo.close();
   %fo.delete();
   exec(%crcFile);
   Canvas.popDialog(MessageBoxOkDlg);
}

//-----------------------------------------------------------------------------

// Jeff: non string for now, just testing purposes only
function genMissionCodeCheck() {
   for (%f = findFirstFile("*.mis"); %f !$= ""; %f = findNextFile("*.mis")) {
      %fo = new FileObject();
      %fIsBad = false;
      %i = 0;
      if (%fo.openForRead(%f)) {
         while (!%fo.isEOF()) {
            %line = trim(%fo.readLine());
            // Jeff: exception list, borrowed the or commands from spy47 'cause it was a good idea like he did in the console
            if (strStr(%line,"function") != -1 ||
                  strStr(%line,"exec") != -1 ||
                  strStr(%line,"eval") != -1 ||
                  strStr(%line,"%") != -1 ||
                  strStr(%line,"$") != -1 ||
                  strstr(%line,"package") != -1 ||
                  strStr(%line,"datablock") != -1 ||
                  strStr(%line,"getFileCRC") != -1 ||
                  strStr(%line,"defaultmarble") != -1 ||
                  strStr(%line,"client") != -1) { // Jeff: takes care of both clientGroup and localclientConnection
                  // Jeff: Todo: add more exceptions
               %fIsBad = true;
               %fBad[%i] = %f;
               %i ++;
            }
         }
      }
   }
   // Jeff: set up the pass by values
   %string = "";
   for (%j = 0; %j < %i; %j ++) {
      if (%string $= "")
         %string = %fBad[%j];
      else
         %string = %string TAB %fBad[%j];
   }
   return %i TAB %string;
}

//-----------------------------------------------------------------------------

// Jeff: crc generator for consistency check - php file (for server crcs)
// looks like spy's old CRC eh? :P
function genConsist() {
   %index = 0;
   %fo = new FileObject();
   for (%f = findFirstFile("*.mis"); %f !$= ""; %f = findNextFile("*.mis")) {
      if (strPos(strlwr(%f), "lbmissions") == -1) {
			if (strpos(strlwr(%f), "lb_custom") == -1 && strpos(strlwr(%f), "multiplayer") == -1)
				continue;
		}
      %crc = getFileCRC(%f);
      %c = "$checkValue[\"" @ expandEscape(fileBase(%f)) @ "\"] = \"" @ %crc @ "\";";
      if (%misFile $= "")
         %misFile = %c;
      else
         %misFile = %misFile NL %c;
      %index ++;
   }
   if (%fo.openForWrite($usermods @ "/check.php"))
      %fo.writeLine(%misFile);
   %fo.close();
   %fo.delete();
}

// JEFF: appendString generator, useful so you dont have to append every single
// instance of a character, let the computer do the work for you.
function appendString(%string) {
   %str = "";
   for (%i = 0; %i < strLen(%string); %i ++) {
      %subStr = expandEscape(getSubStr(%string,%i,1));
      if (%str $= "")
         %str = "\"" @ %subStr;
      else
         %str = %str @ "\"@\"" @ %subStr;
   }
   %str = %str @ "\"";
   return %str;
}

// Jeff: as a test, i did seizure22
// result: %var = "s" @ "e" @ "i" @ "z" @ "u" @ "r" @ "e" @ "2" @ "2";
