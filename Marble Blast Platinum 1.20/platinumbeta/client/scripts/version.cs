//$THIS_VERSION = 6;
//$THIS_VERSION_NAME = "1.10 Alpha 2";
$SERVER_VERSION = 1;
$SERVER_VERSION_NAME = "n/a";
$BUILD_VERSION = 1;
$VERSION_MESSAGE = "";

//-------------------------------------
function startVersionCheck()
{
   if(!$pref::noVersionCheck && !$madeVersionCheck)
   {
      %server = $checkserver;
      %page = $checkpath @ "status.php";

      new TCPObject(Version);
      Version.post(%server, %page, "key=" @ strRand(40) @ "&version=" @ $MP::RevisionOn);
      Version.retries = 3;

      echo("Checking Server Version");
      $madeVersionCheck = true;
   }
}

//-------------------------------------
function Version::onLine(%this, %line)
{
   Parent::onLine(%this, %line);

   %cmd  = firstWord(%line);
   %rest = restWords(%line);
   switch$ (%cmd) {
   case "SIG":
      %this.check();
      //echo("Server Version = (" @ $SERVER_VERSION @ ") " @ $SERVER_VERSION_NAME);
      //echo("This Version = (" @ $THIS_VERSION @ ") " @ $THIS_VERSION_NAME);

      //Parent::onDisconnect(%this);
      %this.destroy();

      //Version.disconnect(); //HiGuy: This was the thing that was crashing PF
      // HiGuy: After he commented this out, he got 0 crashes in 25 launches,
      // versus ~17 crashes in 25

   case "UPTODATE":
      $SERVER_UPDATED = true;
   case "NEWVERSION":
      $SERVER_UPDATED = false;
      %this.handleVersion(%rest);
   case "TITLE":
      %this.handleTitle(%rest);
   case "URL":
      %this.handleURL(%rest);
   case "TIME":
      %this.handleTime(%rest);
   case "DESC":
      %this.handleDesc(%rest);
   }
}

//-------------------------------------
function Version::handleVersion(%this, %line) {
   $SERVER_VERSION  = %line;
}

function Version::handleTitle(%this, %line) {
   $SERVER_VERSION_NAME  = %line;
}

function Version::handleURL(%this, %line) {
   $SERVER_VERSION_URL  = %line;
}

function Version::handleTime(%this, %line) {
   $SERVER_VERSION_TIME  = %line;
}

function Version::handleDesc(%this, %line) {
   $SERVER_VERSION_DESC  = collapseEscape(%line);
}

//-------------------------------------
function Version::check()
{
   // only pop-up the notice on the "MainMenuGui" which calls this onWake
   // just in case we missed it.

   //$content = canvas.getContent().getName();

	$VersionKnown = true;

	if($dontShowUpdate)
		return;

   if ($SERVER_VERSION > $THIS_VERSION)
   {
      MOTDGuiText.setText("New Version Available!\n" @ $VERSION_MESSAGE);
      Canvas.pushDialog(VersionGui);
	  $dontShowUpdate = true;
   }

}

