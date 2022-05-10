//-----------------------------------------------------------------------------
// tcp support
// Programmed from project revolution by Jeff Hutchinson and HiGuy
// Portions from the GG forums, documented below.
//
// Jeff: these methods that are listed here are basically support template
//       methods that hook into the tcpObject class. Some of them can be
//       over-ridden if necessary.  These methods will make it easier for the
//       programmer to send requests and queries to the server.
//-----------------------------------------------------------------------------

// Jeff: declaration of "constants" - play with them for faster/slower execution of scripts
$TcpObject::reconnectTimer = 2000;
$TcpObject::retryTimer     = ($platform $= "macos" ? 800 : 3000);
$TcpObject::firstRetry     = ($platform $= "macos" ? 500 : 5000);
$TcpObject::retryCount     = 5;
$TcpObject::queryStart     = getRealTime();
$TcpObject::totalQueries   = 0;

function TCPObject::get(%this, %server, %file, %values, %timer) {
   %this.auto = true;
   %this.file = %file @ (%values !$= "" ? "?" @ %values : "");
   %this.host = %server;
   %this.mode = "get";
   %this.server = %server;
   %this.keepAlive = false;
   %this.finished = false;
   %this.retries = $TcpObject::retryCount;
   %this.timer = %timer ? %timer : $TcpObject::RetryTimer;
   %this.getCookies();
   %this.connect(%server);
   %this.schedule($TcpObject::reconnectTimer, 0, "reconnect");
}

function TCPObject::post(%this, %server, %file, %values, %timer) {
   %this.auto = true;
   %this.file = %file;
   %this.values = %values;
   %this.host = %server;
   %this.mode = "post";
   %this.server = %server;
   %this.keepAlive = false;
   %this.finished = false;
   %this.retries = $TcpObject::retryCount;
   %this.timer = %timer ? %timer : $TcpObject::RetryTimer;
   %this.retryTimer = %this.timer;
   %this.getCookies();
   %this.connect(%server);
   %this.schedule($TcpObject::reconnectTimer, 0, "reconnect"); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
}

function TCPObject::reconnect(%this) {
   if (!%this.auto || %this.destroying)
      return;

   cancelAll(%this);

   if (%this.reconnects > %this.retries) {
      %this.recreate();
      return;
   }

   %this.onReconnect();
   %this.reconnects  ++;

   //%this.disconnect();
   cancelAll(%this);
   %this.retry = %this.schedule($TcpObject::reconnectTimer, 0, "reconnect"); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   %this.connect(%this.server);
}

function TCPObject::recreate(%this) {
   error("TCP Object getting recreated");
   return;

   %new = new TCPObject(%this.getName());
   if (%this.mode $= "get")
      %new.get(%this.server, %this.file, %this.values, %this.timer);
   else
      %new.post(%this.server, %this.file, %this.values, %this.timer);
   %this.delete();
}

function TCPObject::onConnected(%this) {
   if (!%this.auto || %this.destroying)
      return;
   cancelAll(%this);

   %file = %this.file;
   %host = %this.host;
   if (%this.mode $= "get") {
      %this.lineCount = 0;
      %this.performGet(%file, %host);
      %this.schedule($TcpObject::firstRetry, "retryGet", 0, %file, %host); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   } else if (%this.mode $= "post") {
      %this.performPost(%file, %host, URLEncode(%this.values));
      %this.schedule($TcpObject::firstRetry, "retryPost", 0, %file, %host, URLEncode(%this.values)); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   }
}

function TCPObject::onLine(%this, %line) {
   if (!%this.auto || %this.destroying)
      return;
	// Jeff: a base method, be sure to parent the current tcp object to this method,
	// so that it will provide extended functionallity to the getOutput() method
	// purpose: keeps track of every single line that is received from the server

   if ($platform $= "macos") //HiGuy: Macs connect after a little help
      $TcpObject::firstRetry = 2500;

   if (getSubStr(%line, 0, 1) $= "\x12")
      %this.echo(getSubStr(%line, 1, strlen(%line)), "Line");

   if ((getSubStr(%this.getName(), 0, 2) $= "LB" && $LBShowTCP) || $TCPShowOutput || %this.showOutput)
	   %this.echo(%line, "Line");

   cancelAll(%this);
   %this.line[%this.lineCount] = trim(%line);
   %this.lineCount ++;

   if (%this.receivingHeaders) {
      if (%line $= "")
         %this.receivingHeaders = false;
      if (strPos(%line, "Set-Cookie") == 0) {
         //Set-Cookie: 33096fb4c4fd7002efa2f081b0b33797=4D1A4D1155435B+0+C+717+E122B5870454C17141B11554A4311+B415746+2164E575656+74850+6431F; expires=Sun, 04-Jan-2015 02:36:39 GMT; path=/; httponly
         %cookie = getSubStr(%line, strPos(%line, " ") + 1, strlen(%line));
         //33096fb4c4fd7002efa2f081b0b33797=4D1A4D1155435B+0+C+717+E122B5870454C17141B11554A4311+B415746+2164E575656+74850+6431F; expires=Sun, 04-Jan-2015 02:36:39 GMT; path=/; httponly
         %cname = getSubStr(%cookie, 0, strPos(%cookie, "="));
         //33096fb4c4fd7002efa2f081b0b33797
         %cdata = getSubStr(%cookie, strPos(%cookie, "=") + 1, strPos(%cookie, ";") - (strPos(%cookie, "=") + 1));
         //E122B5870454C17141B11554A4311+B415746+2164E575656+74850+6431F

         setCookie(%this.host, %cname, %cdata);
      }
      if (strPos(%line, "HTTP") == 0) {
         //HTTP/1.1 200 OK
         %response   = getWord(%line, 1);
         %respstring = getWords(%line, 2);
         if (%response != 200) {
            //HiGuy: The amount of conditions that can activate this if-statement
            // is incredible. Let's just hope nobody finds any of them
            if ((getSubStr(%this.getName(), 0, 2) $= "LB" && $LBShowTCP) || $TCPShowOutput || %this.showOutput || %this.echoSigs || $LBShowTCP || $LBShowSigs || %this.showRepsonse || $TCPShowResponse)
               %this.echo("Got Server Response:" SPC %respstring SPC "(" @ %response @ ")", "Response");
            switch (%response) {
            case 400:
            case 401:
            case 403:
            case 404:
            case 500:
            case 502:
            }
         }
      }
   }
}

function setCookie(%host, %name, %data) {
   if ($cookie[%host, %name] $= "") {
      $cookies[%host] += 0;
      $cookie[%host, $cookies[%host]] = %name;
      $cookie[%host, %name] = %data;
      $cookies[%host, %name] = $cookies[%host];
      $cookies[%host] ++;
   } else {
      if (%data $= "deleted") {
         deleteVariables("$cookie" @ %host @ "_" @ %name);
         %num = $cookies[%host, %name];
         //Move them all down one
         for (%i = %num + 1; %i < $cookies[%host]; %i ++) {
            $cookie[%host, %i] = %cookie[%host, %i + 1];
            $cookie[%host, $cookie[%host, %i]] = %cookie[%host, $cookie[%host, %i + 1]];
            $cookies[%host, $cookie[%host, %i + 1]] = %i;
         }
         $cookies[%host] --;

      } else
         $cookies[%host, %name] = %data;
   }
}

function clearCookies(%host) {
   deleteVariables("$cookie" @ %host @ "*");
   deleteVariables("$cookies" @ %host);
}

function clearAllCookies(%host) {
   deleteVariables("$cookie*");
}

function TCPObject::getCookies(%this) {
   if ($cookies[%this.host] > 0)
      %this.cookie = "Cookie:";
   for (%i = 0; %i < $cookies[%this.host]; %i ++) {
      %cname = $cookie[%this.host, %i];
      %cdata = $cookie[%this.host, %cname];
      %this.cookie = %this.cookie SPC URLEncode(%cname) @ "=" @ URLEncode(%cdata) @ ";";
   }
}

function TCPObject::clearLines(%this) {
   for (%i = 0; %i < %this.lineCount; %i ++)
      %this.line[%i] = "";
   %this.lineCount = 0;
   for (%i = 0; %i < %this.queries; %i ++)
      %this.query[%i] = "";
   %this.queries = 0;
}

function TCPObject::getOutput(%this) {
   if (!%this.auto || %this.destroying)
      return;
	// Jeff: this method here will output all information from the onLine() function
	// of the current TCP object by returning it as a string, so be sure to echo if you
	// want to see the results.

   %string = "";
   for (%i = 0; %i < %this.lineCount; %i ++) {
      if (%string $= "") {
         %string = %string @ %this.line[%i];
      } else {
         %string = %string NL %this.line[%i];
      }
   }
   return %string;
}

function TCPObject::performGet(%this, %file, %host) {
   if (!%this.auto || %this.destroying)
      return;
	// Jeff:  The actual method that performs the get query
   cancelAll(%this);
   %this.query[%this.queries ++] = "GET" SPC %file SPC "HTTP/1.1\r\nHost:" SPC
      %host @ (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL "Accept: text/html\r\n\r\n";

   %this.totalqueries ++;
   $TCPObject::TotalQueries ++;
   if (($TCPObject::TotalQueries / (getRealTime() - $TCPObject::queryStart)) > 0.025) {
      error("HOLD THE PHONE! QUERY OVERLOAD");
      MessageBoxOk("Console", "Would be super nice of you to upload your console.log file to <a:marbleblast.com>MarbleBlast.com</a> or send it to marbleblastforums@gmail.com");
      echo("TCPGroup:");
      for (%i = 0; %i < TCPGroup.getCount(); %i ++)
         dumpObject(TCPGroup.getObject(%i), 3);
   }

   if ((getSubStr(%this.getName(), 0, 2) $= "LB" && $LBShowTCP) || $TCPShowOutput || %this.showOutput)
      %this.echo("GET" SPC %file SPC "HTTP/1.1\r\nHost:" SPC %host @
         (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL "Accept: text/html\r\n\r\n", "Send");

   %this.send("GET" SPC %file SPC "HTTP/1.1\r\nHost:" SPC %host @
      (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL "Accept: text/html\r\n\r\n");
   %this.receivingHeaders = true;
}

function TCPObject::performPost(%this, %file, %host, %values) {
   if (!%this.auto || %this.destroying)
      return;
	// Jeff:  The actual method that performs the post query
	//        %values are sent to the server as arguments
   cancelAll(%this);

   %this.query[%this.queries ++] = "POST" SPC %file SPC "HTTP/1.1\nHost:" SPC
      %host @ (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL
      "User-Agent: Torque 1.0 \n" @ "Accept: text/*\n" @
      "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\n" @
      "Content-Length: " @ strLen(%values) @ "\n\n" @ %values @ "\n";

   %this.totalqueries ++;
   $TCPObject::TotalQueries ++;
   if (($TCPObject::TotalQueries / (getRealTime() - $TCPObject::queryStart)) > 0.025) {
      error("HOLD THE PHONE! QUERY OVERLOAD");
      MessageBoxOk("Console", "Would be super nice of you to upload your console.log file to <a:marbleblast.com>MarbleBlast.com</a> or send it to marbleblastforums@gmail.com");
      echo("TCPGroup:");
      for (%i = 0; %i < TCPGroup.getCount(); %i ++)
         dumpObject(TCPGroup.getObject(%i), 3);
   }

   if ((getSubStr(%this.getName(), 0, 2) $= "LB" && $LBShowTCP) || $TCPShowOutput || %this.showOutput)
      %this.echo("POST" SPC %file SPC "HTTP/1.1\nHost:" SPC %host @
      (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL
      "User-Agent: Torque 1.0 \n" @ "Accept: text/*\n" @
      "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\n" @
      "Content-Length: " @ strLen(%values) @ "\n\n" @ %values @ "\n", "Send");

   %this.send("POST" SPC %file SPC "HTTP/1.1\nHost:" SPC %host @
      (%this.cookie $= "" ? "" : "\n" @ %this.cookie) NL
      "User-Agent: Torque 1.0 \n" @ "Accept: text/*\n" @
      "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\n" @
      "Content-Length: " @ strLen(%values) @ "\n\n" @ %values @ "\n");
   %this.receivingHeaders = true;
}

function TCPObject::retryGet(%this,%count,%file,%host) {
   if (!%this.auto || %this.destroying)
      return;
   cancelAll(%this);
   if (%count >= %this.retries) {
      %this.reconnect();
      return;
   }
   if (%count % 5 == 0)
      %this.retryTimer = %this.timer;
   if (%count % 10 == 0 && %count > 0) {
      %this.reconnect();
      return;
   }

   %this.onRetrySend();
   %this.performGet(%file, %host);

   %this.retryTimer += 100;

   cancelAll(%this);
   %this.retry = %this.schedule(%this.Timer, "retryGet", %count + 1, %file, %host);
}

function TCPObject::retryPost(%this, %count, %file, %host, %values) {
   if (!%this.auto || %this.destroying)
      return;
   cancelAll(%this);
   if (%count >= %this.retries) {
      %this.reconnect();
      return;
   }

   if (%count % 5 == 0)
      %this.retryTimer = %this.timer;
   if (%count % 10 == 0 && %count > 0) {
      %this.reconnect();
      return;
   }

   %this.onRetrySend();
   %this.performPost(%file, %host, %values);

   %this.retryTimer += 100;

   cancelAll(%this);
   %this.retry = %this.schedule(%this.retryTimer, "retryPost", %count + 1, %file, %host, %values);
}

function TCPObject::onDisconnect(%this) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   %this.cancel(true);
}

function TCPObject::cancel(%this, %dontDis) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   //if (!%dontDis)
      //%this.disconnect();
   %this.onFinish();
   if (!%this.keepAlive && isEventPending(%this.destroySch))
      %this.destroy(); //HiGuy: I assume we don't need it anymore
   // Jeff: HIGUY, you almost blew up the whole leaderboards
   //%this.delete();
}

function TCPObject::onFinish(%this) {
   //Jeff: Nothing, just a template for overriding it
}

function TCPObject::onRetrySend(%this) {
   //HiGuy: Nothing here, override this!
}

function TCPObject::onReconnect(%this) {
   //HiGuy: Another overridable
}

function TCPObject::onDNSFailed(%this) {
   %this.cancel();
}

function TCPObject::onConnectFailed(%this) {
   %this.cancel();
}

//HiGuy: Cannot remember which one it is
function TCPObject::onConnectionFailed(%this) {
   %this.cancel();
}

function TCPObject::echo(%this, %text, %abbr) {
   if (!isObject(%this))
      return;

   //Removed due to devmode
   devecho(%this.getName() @ (%abbr $= "" ? "" : " (" @ %abbr @ ")") SPC "::" SPC %text);
}

function TCPObject::dump(%this) {
   // Jeff: I choose to override this because we do not want the client
	// to be able to view our queries.  This is for data security, because
	// all queries and info are stored into the actual tcp object itself.
	// This is a standard c++ error code, implimented to make this thing look legit.
	warn("<input> (0): Unknown command dump.");
	if (%this.getName() $= "")
   	warn("  Object (" @ %this.getID() @ ") TCPObject -> SimObject");
	else
   	warn("  Object " @ %this.getName() @ "(" @ %this.getID() @ ") " @ %this.getName() @ " -> TCPObject -> SimObject");
}

//-----------------------------------------------------------------------------
// http://www.garagegames.com/community/blogs/view/10202
//-----------------------------------------------------------------------------

function dec2hex(%val)
{
	// Converts a decimal number into a 2 digit HEX number
	%digits ="0123456789ABCDEF";	//HEX digit table

	// To get the first number we divide by 16 and then round down, using
	// that number as a lookup into our HEX table.
	%firstDigit = getSubStr(%digits,mFloor(%val/16),1);

	// To get the second number we do a MOD 16 and using that number as a
	// lookup into our HEX table.
	%secDigit = getSubStr(%digits,%val % 16,1);

	// return our two digit HEX number
	return %firstDigit @ %secDigit;
}

function hex2dec(%val)
{
	// Converts a decimal number into a 2 digit HEX number
	%digits ="0123456789ABCDEF";	//HEX digit table

	// To get the first number we divide by 16 and then round down, using
	// that number as a lookup into our HEX table.
	%firstDigit = strPos(%digits, getSubStr(%val, 0, 1));

	// To get the second number we do a MOD 16 and using that number as a
	// lookup into our HEX table.
	%secondDigit = strPos(%digits, getSubStr(%val, 1, 1));

	// return our two digit HEX number
	return (%firstDigit * 16) + %secondDigit;
}

function chrValue(%chr)
{
	// So we don't have to do any C++ changes we approximate the function
	// to return ASCII Values for a character.  This ignores the first 31
	// characters and the last 128.

	// Setup our Character Table.  Starting with ASCII character 32 (SPACE)
	%charTable = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^\'_abcdefghijklmnopqrstuvwxyz{|}~\t\n\r";

	//Find the position in the string for the Character we are looking for the value of
	%value = strpos(%charTable,%chr);

	// Add 32 to the value to get the true ASCII value
	%value = %value + 32;

	//HACK:  Encode TAB, New Line and Carriage Return

	if (%value >= 127)
	{
      if(%value == 127)
         %value = 9;
      if(%value == 128)
         %value = 10;
      if(%value == 129)
         %value = 13;
	}

	//return the value of the character
	return %value;
}

function chrForValue(%chr)
{
	// So we don't have to do any C++ changes we approximate the function
	// to return ASCII Values for a character.  This ignores the first 31
	// characters and the last 128.

	// Setup our Character Table.  Starting with ASCII character 32 (SPACE)
	%charTable = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^\'_abcdefghijklmnopqrstuvwxyz{|}~\t\n\r";

	//HACK:  Decode TAB, New Line and Carriage Return

   if(%chr == 9)
      %chr = 127;
   if(%chr == 10)
      %chr = 128;
   if(%chr == 13)
      %chr = 129;

   %chr -= 32;
	%value = getSubStr(%charTable,%chr, 1);

	return %value;
}

function URLEncode(%rawString)
{
	// Encode strings to be HTTP safe for URL use

	// Table of characters that are valid in an HTTP URL
   %validChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz:/.?=_-$(){}~&";


	// If the string we are encoding has text... start encoding
	if (strlen(%rawString) > 0)
	{
		// Loop through each character in the string
		for(%i=0;%i<strlen(%rawString);%i++)
		{
			// Grab the character at our current index location
			%chrTemp = getSubStr(%rawString,%i,1);

			//  If the character is not valid for an HTTP URL... Encode it
         if (strstr(%validChars,%chrTemp) == -1)
         {
            //Get the HEX value for the character
            %chrTemp = dec2hex(chrValue(%chrTemp));

            // Is it a space?  Change it to a "+" symbol
            if (%chrTemp $= "20")
            {
               %chrTemp = "+";
            }
            else
            {
 		  			// It's not a space, prepend the HEX value with a %
 		  			%chrTemp = "%" @ %chrTemp;
            }
         }
         // Build our encoded string
			%encodeString = %encodeString @ %chrTemp;
		}
	}
	// Return the encoded string value
	return %encodeString;
}
function URLDecode(%rawString)
{
	// Encode strings from HTTP safe for URL use

	// If the string we are encoding has text... start encoding
	if (strlen(%rawString) > 0)
	{
		// Loop through each character in the string
		for(%i=0;%i<strlen(%rawString);%i++)
		{
			// Grab the character at our current index location
			%chrTemp = getSubStr(%rawString,%i,1);

 		   if (%chrTemp $= "+") {
    		  	// Was it a "+" symbol?  Change it to a space
 		      %chrTemp = " ";
 		   }
			//  If the character was not valid for an HTTP URL... Decode it
 		   if (%chrTemp $= "%") {
            //Get the dec value for the character
            %chrTemp = chrForValue(hex2dec(getSubStr(%rawString, %i + 1, 2)));
            %i += 2;
         }
         // Build our encoded string
			%encodeString = %encodeString @ %chrTemp;
		}
	}
	// Return the encoded string value
	return %encodeString;
}
//------------------------------------------------------------------------------

function TCPObject::parseSigs(%this, %line) {
   if (%this.finished) //no, no, no, no, no, no, no
      return true;
   if (getWord(%line, 0) $= "SIG") {
      %sigNum  = getWord( %line, 1);
      %sigName = getWords(%line, 2);
      switch (getWord(%line,1)) {
      case 0: // Jeff: server failure
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 0 Server Failure");
         //%this.disconnect();
         %this.finished = true;
         return true;
      case 1: // Jeff: page finish
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 1 Finish");
         //%this.disconnect();
         %this.finished = true;
         return true;
      case 2: // Jeff: missing information
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 2 Missing Information");
         return true;
      case 3: //HiGuy: Username exists
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 3 Username Exists");
         return false;
      case 4: //HiGuy: Email exists
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 4 Email Exists");
         return false;
      case 5: //HiGuy: User Added
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 5 User Added");
         return false;
      case 6: // Jeff: invalid username/password
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 6 Invalid Username or Password");
         return true;
      case 7: //HiGuy: Logged In
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 7 Logged In");
         return false;
      case 8: //HiGuy: Already logged in
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 8 Already Logged In!");
         return false;
      case 9: // Jeff: not logged in
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 9 Not Logged In!");
         return true;
      case 10: //HiGuy: Logged Out
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 10 Logged Out");
         return false;
      case 11: //HiGuy: No Mission
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 11 Bad Mission!");
         return false;
      case 12: //HiGuy: Bad Score
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 12 Bad Score!");
         return false;
      case 13: //HiGuy: Chat Sent
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 13 Chat Sent");
         return false;
      case 14: // Jeff: invalid passcode (security feature)
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 14 Invalid Passcode");
         return true;
      case 15: //HiGuy: Score Sent
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 15 Score Sent");
         return false;
      case 16: //HiGuy: Starting List
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 16 Starting List");
         return false;
      case 17: //HiGuy: Already Found EE
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 17 Already found EE");
         return false;
      case 18: //HiGuy: Sent Message
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 18 Sent Message");
         return false;
      case 19: //HiGuy: Not Authorized
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 19 Not Authorized");
         return false;
      case 20: //HiGuy: List Finish
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 20 List Finish");
         return false;
      case 21: //HiGuy: CRC Good
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 21 CRC Good");
         if (isObject(LBRedundancyCheckNetwork) && %this.getName() !$= "LBRedundancyCheckNetwork")
            LBRedundancyCheckNetwork.onLine(%line);
         return false;
      case 22: //HiGuy: CRC Bad
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 22 CRC Bad");
         if (isObject(LBRedundancyCheckNetwork) && %this.getName() !$= "LBRedundancyCheckNetwork")
            LBRedundancyCheckNetwork.onLine(%line);
         return false;
      case 23: // Jeff: ID for challenge
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 23 ID");
         return false;
      case 24: //HiGuy: No Key
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 24 No Key");
         return false;
      case 25: //HiGuy: Invalid Key
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 25 Invalid Key");
         return true;
      case 26: //HiGuy: Good Key
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 26 Good Key");
         return true;
      case 27: //HiGuy: Banned
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 27 Banned");
         //HiGuy: Let them know
         if ($LB && $LB::LoggedIn) { //If lbs are active
            if ($Game::Running)
               disconnect();
            $LB::LoggedIn = 0;
            LBAssert("You are Banned", "You have been banned from the MBP leaderboards!", "Canvas.setContent(MainMenuGui);closeLeaderboards();");
         }

         return true;
      case 28: //HiGuy: No category
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("SIG 28 No Category");
         return true;
      default: //HiGuy: Who knows?
         if (%this.echoSigs || $LBShowTCP || $LBShowSigs)
            %this.echo("Unknown sig " SPC %sigNum SPC "named" SPC %sigName);
         return false;
      }
   }
   return false;
}

function LBDefaultQuery(%username, %password) {
	if ($tracing) {
		%trace = true;
		trace(false);
	}
   %username = %username $= "" ? $LB::username : %username;
   %password = %password $= "" ? $LB::Password2 : garbledeguck(%password);
   %key = strRand(40);
   if (%trace)	trace(true);
   %Version = $MP::RevisionOn;
   return "username=" @ %username @ "&password=" @ %password @ "&joomlaAuth=0&key=" @ %key @ "&version=" @ %version;
}

//------------------------------------------------------------------------------

function TCPObject::destroy(%this) {
   //%this.disconnect();
   if (!%this.shhhhh && $LBShowSigs)
      %this.echo("Destroying!");
   %this.destroying = true;
   cancelAll(%this);
   cancel(%this.retry);

   %this.destroySch = %this.schedule(500, "delete");
}

function GameConnection::destroy(%this) {
   %this.disconnect();
   //%this.schedule(500, "delete");
}

//------------------------------------------------------------------------------



$Wrapper::MapStatus = 0;
$Wrapper::LocalIP = "Unavailable";
$Wrapper::GlobalIP = "Unavailable";
$Wrapper::Debug = 0;
$Wrapper::Checks = 0;

function wrapperCheck() {
   cancel($wrapperCheck);
   if (isObject(WrapperCheckNetwork))
      nameToID(WrapperCheckNetwork).destroy();
   new TCPObject(WrapperCheckNetwork);
   WrapperCheckNetwork.connect("127.0.0.1:19535");
   WrapperCheckNetwork.shhhhh = true;
   $Wrapper::Checks ++;
   $wrapperCheck = schedule(3000, 0, cancelWrapperCheck);
}

function cancelWrapperCheck() {
   cancel($wrapperCheck);
   if (isObject(WrapperCheckNetwork))
      nameToID(WrapperCheckNetwork).destroy();
   if ($Wrapper::Checks < 5 && $Wrapper::MapStatus == 0)
      $wrapperCheck = schedule(2000, 0, "wrapperCheck");
}

function WrapperCheckNetwork::onLine(%this, %line) {
   cancel($wrapperCheck);
   %line = trim(%line);
   $Wrapper::MapStatus = getSubStr(%line, 0, 1);
   $Wrapper::LocalIP = getSubStr(%line, 2, strPos(%line, ":", 2) - 2);
   $Wrapper::GlobalIP = getSubStr(%line, strPos(%line, ":", 2) + 1, strlen(%line));
   if ($Wrapper::Debug)
      devecho("Wrapper Status:" SPC $Wrapper::MapStatus NL "Local IP:" SPC $Wrapper::LocalIP NL "Global IP:" SPC $Wrapper::GlobalIP);
   $wrapperCheck = schedule(2000, 0, "wrapperCheck");
}

function checkPort() {
   if ($PortStatus !$= "")
      return;
   $PortStatus = "checking";
   if ($LB) MPUpdatePortLabel();
   if (!isObject(PortChecker))
      new TCPObject(PortChecker);
   PortChecker.retries = 0;
   PortChecker.connect("www.whatsmyip.org:80");
}

function PortChecker::onConnected(%this) {
   %values = "port=" @ $pref::Server::Port @ "&timeout=default";
   //HiGuy: Working request nabbed from Chrome
   %query = "POST /port-scanner/scan.php HTTP/1.1" NL
				"Host: www.whatsmyip.org" NL
				"Connection: keep-alive" NL
				"Content-Length: 26" NL
				"Accept: text/*" NL
				"Origin: http://www.whatsmyip.org" NL
				"X-Requested-With: XMLHttpRequest" NL
				"User-Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1911.0 Safari/537.36" NL
				"Content-Type: application/x-www-form-urlencoded; charset=UTF-8" NL
				"DNT: 1" NL
				"Referer: http://www.whatsmyip.org/port-scanner/" NL
				"Accept-Encoding: text" NL
				"Accept-Language: en-US,en;q=0.8\n" NL
            %values @ "\n";
   if ($TCPShowOutput)
      warn(%query);
   %this.sendLoop(%query); //Local vars for jeffy

   if (!isObject(PortListener))
      new TCPObject(PortListener);
   PortListener.listen($pref::Server::Port);
}

function PortChecker::sendLoop(%this, %query) {
   if (%this.retries > 4) {
      %this.cancel();
      return;
   }
   if ($TCPShowOutput)
      warn(%query);
   %this.retries ++;
   %this.send(%query);
   %this.echo("Trying to send...");
   //HiGuy: Timeouts take > 2 seconds
   %this.schedule(10000, sendLoop, %query);
}

function PortChecker::onLine(%this,%line) {
   Parent::onLine(%this,%line);
   %this.echo(%line);
   if (%line $= "1") { //Open
      $PortStatus = "global";
      PortListener.destroy();
      %this.destroy();
      if ($LB) MPUpdatePortLabel();
   } else if (%line $= "2") { //Timed out
      $PortStatus = "lan";
      %this.destroy();
      PortListener.destroy();
      if ($LB) MPUpdatePortLabel();
   } else if (%line $= "3") { //Closed
      $PortStatus = "lan";
      %this.destroy();
      PortListener.destroy();
      if ($LB) MPUpdatePortLabel();
   }
}
