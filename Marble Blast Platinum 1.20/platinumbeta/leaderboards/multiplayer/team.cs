//------------------------------------------------------------------------------
// Multiplayer Package
// team.cs
// Copyright (c) 2013 MBP Team
//------------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Team Creation and deletion
//-----------------------------------------------------------------------------

//HiGuy: Creates a new team with name %name with the leader %leader. If
// %permanent is set, then the team will be registered as a default (permanent)
// team, and will not be deleted when it has 0 players. If %private is set, then
// only the team leader or server host can add players to the team.
//
// Variables:
// %name        - String
// %leader      - GameConnection
// %permanent   - Boolean
// %private     - Boolean
// %description - String
// %color       - Integer
//
// Returns:
// <id>  of the team if team creation was successful
// false if there was an error

function Team::createTeam(%name, %leader, %permanent, %private, %description, %color) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: If the team already exists with that name, it can't be created twice
   if (Team::getTeam(%name) != -1)
      return false;

   //HiGuy: Resolve the leader
   if (!isObject(%leader) && !isObject(%leader = nameToId(%leader)) && (%leader = GameConnection::resolveName(%leader)) == -1)
      return false;

   //HiGuy: Create a new SimSet for the team object and set its fields
   TeamGroup.add(%newTeam = new SimSet());
   %newTeam.name      = %name;
   %newTeam.number    = $MP::Teams;
   %newTeam.leader    = %leader;
   %newTeam.permanent = %permanent;
   %newTeam.private   = %private;
   %newTeam.desc      = %description;
   %newTeam.color     = %color;

   //HiGuy: Increment the total team counter
   $MP::Teams ++;

   //HiGuy: Add the leader to their new team
   Team::addPlayer(%newTeam, %leader);

   //HiGuy: Update the master team list
   updateTeams();

   return %newTeam;
}

//HiGuy: Deletes a specified team with name %name.
// Note that if %team is the default team, there may be an explosion. Please
// use precaution when deleting permanent teams, as this method does not protect
// against that.
//
// Variables:
// %name - String
//
// Returns:
// true  if team deletion was successful
// false if there was an error

function Team::deleteTeam(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Take all the players out of this team
   %count = %team.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %player = %team.getObject(%i);

      //HiGuy: And add them to the default team
      Team::addPlayer(Team::getDefaultTeam(), %player);
   }

   //HiGuy: If it hasn't been deleted already
   if (isObject(%team))
      %team.delete();

   //HiGuy: Update the master team list
   updateTeams();

   return true;
}

//-----------------------------------------------------------------------------
// Modifying Team Player Lists
//-----------------------------------------------------------------------------

//HiGuy: Adds the player %player to the team %newTeam. They will be removed from
// any teams they currently are in, and will become leader of any teams with
// zero players.
//
// Variables:
// %newTeam - SimSet
// %player  - GameConnection
//
// Returns:
// true  if the player was added to the team
// false if there was an error

function Team::addPlayer(%newTeam, %player) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %newTeam to be a SimSet team instance or fail
   if (!isObject(%newTeam) && (%newTeam = Team::getTeam(%newTeam)) == -1)
      return false;

   //HiGuy: Resolve the player
   if (!isObject(%player) && !isObject(%player = nameToId(%player)) && (%player = GameConnection::resolveName(%player)) == -1)
      return false;

   //HiGuy: Don't recurse!
   if (%player.adding)
      return false;

   %player.adding = true;

   //HiGuy: Make sure they're not on any other teams
   %count = TeamGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);

      if (%team.isMember(%player)) {
         //HiGuy: They're on another team!
         // If they're the leader, we may have problems

         if (%team.getId() == %newTeam.getId()) {
            //HiGuy: They just tried to add themselves to the same team...
            //HiGuy: We'll just set a thing or two to make sure, because SimSets
            // can't have duplicates

            %newTeam.add(%player);
            %player.team = %newTeam.getId();
            continue;
         }

         //HiGuy: Take them out of this team, regardless of if they were the
         // leader or not
         Team::removePlayer(%team, %player);
      }
   }

   %player.adding = false;

   if (%newTeam.getCount() == 0) {
      Team::setTeamLeader(%player);
   }

   //HiGuy: At this point, they should be ready to add to their new team
   %newTeam.add(%player);
   %player.team = %newTeam.getId();

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Removes the player %player from the team %team. If they were the leader
// of the team, then the second person on the team becomes leader. If the team
// is left with zero players, it will be deleted unless it is the default team.
//
// Variables:
// %team   - SimSet
// %player - GameConnection
//
// Returns:
// true  if the player was removed from the team
// false if there was an error

function Team::removePlayer(%team, %player) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Resolve the player
   if (!isObject(%player) && !isObject(%player = nameToId(%player)) && (%player = GameConnection::resolveName(%player)) == -1)
      return false;

   //HiGuy: Take %player out of %team, and remove %team if it has no players
   %team.remove(%player);

   //HiGuy: Add them to the default team so we don't get any errors
   Team::addPlayer(Team::getDefaultTeam(), %player);

   //HiGuy: If we're removing the team leader, we have to find a new leader
   if (%team.leader.getId() == %player.getId()) {
      Team::resolveLeader(%team);
   }

   //HiGuy: It's that easy
   if (%team.getCount() == 0) {
      //HiGuy: Leader is removed, incase this is the default team and it won't
      // be destroyed
      %team.leader = "";

      //HiGuy: Don't delete the default team! Then we wouldn't have anywhere to
      // dump people who have their team deleted.
      if (%team.permanent)
         return false;

      Team::deleteTeam(%team);
   }

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//-----------------------------------------------------------------------------
// Accessing Teams
//-----------------------------------------------------------------------------

//HiGuy: Access to the team with the name %teamName, or -1 if no team with that
// name currently exists.
//
// Variables:
// %teamName - String
//
// Returns:
// <id> of the team with the given name
// -1   if no team with that name exists

function Team::getTeam(%teamName) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Now why would you be sending a whole team?
   if (isObject(%teamName))
      return %teamName;

   //HiGuy: Iterate through the list and return the matching team
   %count = TeamGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);
      if (%team.name $= %teamName)
         return %team.getId();
   }

   //HiGuy: If none exist, return -1
   return -1;
}

//HiGuy: Access to the default team, or the first permanent team.
//
// Returns:
// <id> of the default team or the first permanent team
// -1   if no permanent teams exist

function Team::getDefaultTeam() {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Iterate through the list and return the first permanent team.
   // Technically, only the default team should be permanent, but in the event
   // that someone *really* likes their team, then they should be allright with
   // random people accidentally joining them.

   %count = TeamGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);
      if (%team.permanent)
         return %team.getId();
   }

   //HiGuy: If none exist, return -1
   return -1;
}

//-----------------------------------------------------------------------------
// Player / Team Information
//-----------------------------------------------------------------------------

//HiGuy: Access to the team that the player %player is part of.
//
// Variables:
// %player - GameConnection
//
// Returns:
// <id>  of the team that the player is in
// -1    if the player is not on a team
// false if there was an error

function Team::getPlayerTeam(%player) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve the player
   if (!isObject(%player) && !isObject(%player = nameToId(%player)) && (%player = GameConnection::resolveName(%player)) == -1)
      return false;

   //HiGuy: Iterate through the list and return the matching team
   %count = TeamGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %team = TeamGroup.getObject(%i);
      if (%team.isMember(%player))
         return %team.getId();
   }

   return -1;
}

//HiGuy: Access to the name of %team.
//
// Variables:
// %team - SimSet
//
// Returns:
// <string> of the team name
// false    if there was an error

function Team::getTeamName(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.name;
}

//HiGuy: Changes the name of %team to %name.
//
// Variables:
// %team - SimSet
// %name - String
//
// Returns:
// true  if the name was changed
// false if there was an error

function Team::setTeamName(%team, %name) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Can't be having two teams with the same name!
   if (Team::getTeam(%name) != -1 && Team::getTeam(%name).getId() != %team.getId())
      return false;

   //HiGuy: Can't set a blank name
   if (trim(%name) $= "")
      return false;

   //HiGuy: Simple set
   %team.name = %name;

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Access to the description of %team.
//
// Variables:
// %team - SimSet
//
// Returns:
// <string> of the team description
// false    if there was an error

function Team::getTeamDescription(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.desc;
}

//HiGuy: Changes the description of %team to %description.
//
// Variables:
// %team        - SimSet
// %description - String
//
// Returns:
// true  if the description was changed
// false if there was an error

function Team::setTeamDescription(%team, %description) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Simple set
   %team.desc = %description;

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Access to the team color of %team. The team color is
// stored as an integer to the color (see color list) that all players on the
// team are shown with.
//
// Variables:
// %team - SimSet
//
// Returns:
// <int> of the team color
// false if there was an error

function Team::getTeamColor(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.color;
}

//HiGuy: Changes the team color of %team to %color.
//
// Variables:
// %team  - SimSet
// %color - Integer
//
// Returns:
// true  if the color was changed
// false if there was an error

function Team::setTeamColor(%team, %color) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Simple set
   %team.color = %color;

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Access to the private status of %team.
//
// Variables:
// %team - SimSet
//
// Returns:
// true  if the team is private
// false if the team is public or if there was an error

function Team::getTeamPrivate(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.private;
}

//HiGuy: Changes the private status of %team to %description.
//
// Variables:
// %team    - SimSet
// %private - Boolean
//
// Returns:
// true  if the private status was changed
// false if there was an error

function Team::setTeamPrivate(%team, %private) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Simple set
   %team.private = %private;

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Easy knowledge of whether %team is a default team
//
// Variables:
// %team - SimSet
//
// Returns:
// true  if that team is a default team
// false if it is not a default team, or if there was an error

function Team::isDefaultTeam(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.permanent;
}

function Team::getTeamRole(%team, %player) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Resolve the player
   if (!isObject(%player) && !isObject(%player = nameToId(%player)) && (%player = GameConnection::resolveName(%player)) == -1)
      return false;

   if (Team::isTeamLeader(%team, %player))
      return $MP::TeamLeaderRoleName;
   if (%player.team.getId() != %team.getId())
      return $MP::TeamUnaffiliatedRoleName;
   return $MP::TeamGeneralRoleName;
}

//-----------------------------------------------------------------------------
// Team Leader
//-----------------------------------------------------------------------------

//HiGuy: Changes the leader of %team to %newLeader, keeping the old leader in
// the team, but removing their leader status.
//
// Variables:
// %team      - SimSet
// %newLeader - GameConnection
//
// Returns:
// true  if the leader was changed
// false if there was an error

function Team::setTeamLeader(%team, %newLeader) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Resolve the player
   if (!isObject(%newLeader) && !isObject(%newLeader = nameToId(%newLeader)) && (%newLeader = GameConnection::resolveName(%newLeader)) == -1)
      return false;

   //HiGuy: Simple set
   %team.leader = %newLeader.getId();

   //HiGuy: Send it out
   updateTeams();

   return true;
}

//HiGuy: Access to the leader of %team.
//
// Variables:
// %team - SimSet
//
// Returns:
// <id>  of the team leader
// false if there was an error

function Team::getTeamLeader(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   return %team.leader;
}

//HiGuy: Easy knowledge of whether %player is the leader of %team.
//
// Variables:
// %team   - SimSet
// %player - GameConnection
//
// Returns:
// true  if that player is the leader of that team
// false if they are not the leader, or if there was an error

function Team::isTeamLeader(%team, %player) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: Resolve the player
   if (!isObject(%player) && !isObject(%player = nameToId(%player)) && (%player = GameConnection::resolveName(%player)) == -1)
      return false;

   if (!isObject(%team.leader)) {
      Team::resolveLeader(%team);
   }

   if (%player.getId() == %team.leader.getId())
      return true;
   return false;
}

//HiGuy: Fix a possibly unleadered team and set their leader to the next player
// if necessary. Permanent teams will always have their leader set as the local
// client.
//
// Variables:
// %team - SimSet
//
// Returns:
// true  if the leader was fixed
// false if there was an error
//

function Team::resolveLeader(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   //HiGuy: If we already have a leader, then why are we resolving?
   if (isObject(%team.leader))
      return false;

   //HiGuy: The default team is owned by the local client
   if (%team.permanent) {
      Team::setTeamLeader(%team, ClientGroup.getObject(0));
      return true;
   }

   //HiGuy: We need to find the next leader for %team
   %newLeader = -1;

   //HiGuy: Iterate through
   %teamCount = %team.getCount();
   for (%j = 0; %j < %teamCount; %j ++) {
      %next = %team.getObject(%j);
      %newLeader = %next;
      break;
   }

   //HiGuy: If we found a new leader, let them rule
   if (%newLeader != -1) {
      Team::setTeamLeader(%team, %newLeader);
      return true;

      //HiGuy: Send it out
      updateTeams();
   }

   return false;
}

//-----------------------------------------------------------------------------
// Message Sending
//-----------------------------------------------------------------------------

//HiGuy: Calls a method on all players on a team.
//
// Variables:
// %team   - SimSet
// %method - String
// %arg[]  - Unknown
//
// Returns:
// true  if the method was called on all players
// false if there was an error

function Team::call(%team, %method, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

   %count = %team.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %player = %team.getObject(%i);

      //HiGuy: SimObject::call() is defined in platinum/main.cs
      %player.call(%method, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10);
   }

   return true;
}

//-----------------------------------------------------------------------------
// Helper functions
//-----------------------------------------------------------------------------

//HiGuy: Helper method that sets up team support variables. Should be called
// from every function in the Team:: namespace.
//
// Returns:
// nothing

function Team::check() {
   //HiGuy: Init some things that may not exist...
   //HiGuy: This makes sure that $MP::Teams is a number
   $MP::Teams += 0;

   //HiGuy: Create TeamGroup if it doesn't exist
   if (!isObject(TeamGroup))
      RootGroup.add(new SimGroup(TeamGroup));
}

//HiGuy: Helper method that fixes all loose ends on the teams system. Call as
// often as you want.
function Team::fix() {
	//HiGuy: Always want to make sure that these exist
   Team::check();

	//HiGuy: Make sure people have a team to join
	if ($MP::Teams == 0) {
		//HiGuy: Make a team for them
		%team = Team::createDefaultTeam();

		//HiGuy: If we just created the default team, then nobody has a team!
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i ++) {
			%client = ClientGroup.getObject(%i);
			Team::addPlayer(%team, %client);
		}
	}

	%default = Team::getDefaultTeam();

	//HiGuy: Make sure nobody is stranded without a team
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i ++) {
		%client = ClientGroup.getObject(%i);

		//HiGuy: If your team does not exist, add you to the default team
		if (!isObject(%client.team))
			Team::addPlayer(%default, %client);
	}
}

//HiGuy: Resolves a GameConnection from a given name. Call this function from
// the GameConnection:: namespace, or if you want a lazy way to get the id, you
// can do <GameConnection object>.resolveName() to get its id.
//
// Variables:
// %name - String
//
// Returns:
// <id> of the client with the given name
// -1   if no clients with that name exist

function GameConnection::resolveName(%name) {
   //HiGuy: If we passed in a GameConnection, we just want the id
   if (isObject(%name) && %name.getClassName() $= "GameConnection")
      return %name.getId();

   //HiGuy: Iterate the list and check names
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.nameBase $= %name)
         return %client.getId();
   }

   //HiGuy: Return -1 if no clients with %name are found
   return -1;
}

//HiGuy: Creates a default team in which unsorted players are added to, or
// simply returns the ID of the default team if it already exists.
//
// Returns:
// <id> of the default team

function Team::createDefaultTeam() {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   if (Team::getDefaultTeam() != -1)
      return Team::getDefaultTeam();

   //HiGuy: Create the default team with a sample description and title.
   //
   // Default Team Settings:
   //
   // Name - Default Team
   // Leader - Local Client
   // Permanent - true
   // Private - false
   // Description - Default Team Description
   // Color - Any
   //
   return Team::createTeam($MP::DefaultTeamName, ClientGroup.getObject(0), true, false, $MP::DefaultTeamDesc, -1);
}

//-----------------------------------------------------------------------------
// Team Sorting
//-----------------------------------------------------------------------------

//HiGuy: Gets the general "strength" of a team based on its players' ratings.
//
// Variables:
// %team - The team to analyze
//
// Returns:
// <int> for the team strength
// <false> on failure
function Team::getTeamStrength(%team) {
   //HiGuy: Always want to make sure that these exist
   Team::check();

   //HiGuy: Resolve %team to be a SimSet team instance or fail
   if (!isObject(%team) && (%team = Team::getTeam(%team)) == -1)
      return false;

	//HiGuy: Empty teams have a strength of 1, the lowest
	if (%team.getCount() == 0)
		return 1;

	//HiGuy: Lowest is 1 because 0 sends as "false"
	%strength = 1;

	//HiGuy: Get the overall maximum and minimum ratings
	%maxRating = 0;
	%minRating = 9999;

	//HiGuy: Iterate through all clients
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i ++) {
		%player = ClientGroup.getObject(%i);

		//HiGuy: Get their rating
		%rating = %player.rating;

		//HiGuy: Check for min/max
		if (%rating > %maxRating)
			%maxRating = %rating;
		if (%rating < %minRating)
			%minRating = %rating;
	}

	//HiGuy: Go through each player in the team and get their rating
	%count = %team.getCount();
	for (%i = 0; %i < %count; %i ++) {
		%player = %team.getObject(%i);

	}
}
