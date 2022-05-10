#!/bin/bash

WIN_DIRNAME="MBP 1.50 Beta 3.0 - Windows"
MAC_DIRNAME="MBP 1.50 Beta 3.0 - Mac"
NOS_DIRNAME="MBP 1.50 Beta 3.0 - No Executable"

echo "DELETING CS AND GUI FILES"
echo "PRESS ENTER TO DELETE EM"

read

DIR=`echo $0 | sed -E 's/\/[^\/]+$/\//'`
if [ "X$0" != "X$DIR" ]; then
	cd "$DIR"
fi

SAVEIFS=$IFS
IFS=$(echo -en "\n\b");

for i in $(find . -type f \( -iname "*.cs" \))
do
   file=${i}
   if [ -e $file ]
   then
   	echo "Trashing ${file}"
        rm "$file"
   fi
done
for i in $(find . -type f \( -iname "*.gui" \))
do
   file=${i}
   if [ -e $file ]
   then
   	echo "Trashing ${file}"
	rm "$file"
   fi
done

echo "BURP!"
echo "DELETING .DS_STORE AND THUMBS.DB FILES"
echo "YOU KNOW THE DRILL"

read

find . \( -name ".DS_Store" \) -exec sh -c 'echo "Trashing {}"; rm {}' \;
find . \( -name "Thumbs.db" \) -exec sh -c 'echo "Trashing {}"; rm {}' \;

echo "BURP!"
echo "DELETING OTHER RANDOM SCRIPT FILES"
echo "TAKE A GUESS"

read

echo "Trashing ./client/config.cs.dso"
echo "Trashing ./client/lbprefs.cs.dso"
echo "Trashing ./client/mbpprefs.cs.dso"
echo "Trashing ./client/prefs.cs.dso"
echo "Trashing ./client/demos/*"
echo "Trashing ./core/editor/WEprefs.cs.dso"
echo "Trashing ./data/interiorDeleter.sh"
echo "Trashing ./dev/"
echo "Trashing ./leaderboards/multiplayer/prefs.cs.dso"
rm -f ./client/config.cs.dso
rm -f ./client/lbprefs.cs.dso
rm -f ./client/mbpprefs.cs.dso
rm -f ./client/prefs.cs.dso
rm -f ./client/demos/*
rm -f ./core/editor/WEprefs.cs.dso
rm -f ./data/interiorDeleter.sh
rm -f ./data/.nullfile
rm -r ./data/recordings/*
rm -r ./data/screenshots/*
rm -r ./dev/
rm -f ./leaderboards/multiplayer/prefs.cs.dso
rm -f ./Prepare\ Build.command

echo "OK ALL FILES ARE BURPED. SHOULD PROBABLY DO A MANUAL CHECK :)"
echo "DON'T BLAME THE SCRIPT FILE IF IT MISSED SOMETHING."

echo "WAITING TO DO A 7Z JOB UNTIL YOU PRESS ENTER..."
read

cd ..

WIN_INSTALL=1
MAC_INSTALL=1
NOS_INSTALL=1

if [ -d "./${WIN_DIRNAME}" ]
then
   echo "DIR EXISTS AT ${WIN_DIRNAME} DELETE?"
   read del
   if [ -n "$del" ]
   then
      rm -rf "./${WIN_DIRNAME}"
      rm -f "./${WIN_DIRNAME}.7z"
      rm -f "./${WIN_DIRNAME}.zip"
   else
      WIN_INSTALL=0
   fi
fi
if [ -d "./${MAC_DIRNAME}" ]
then
   echo "DIR EXISTS AT ${MAC_DIRNAME} DELETE?"
   read del
   if [ -n "$del" ]
   then
      rm -rf "./${MAC_DIRNAME}"
      rm -f "./${MAC_DIRNAME}.7z"
      rm -f "./${MAC_DIRNAME}.zip"
   else
      MAC_INSTALL=0
   fi
fi
if [ -d "./${NOS_DIRNAME}" ]
then
   echo "DIR EXISTS AT ${NOS_DIRNAME} DELETE?"
   read del
   if [ -n "$del" ]
   then
      rm -rf "./${NOS_DIRNAME}"
      rm -f "./${NOS_DIRNAME}.7z"
      rm -f "./${NOS_DIRNAME}.zip"
   else
      NOS_INSTALL=0
   fi
fi

if [ $WIN_INSTALL -eq 1 ]
then
   mkdir "./${WIN_DIRNAME}"
   echo "PREPARING WINDOWS BUILD. COPYING FILES"
   cd "./${WIN_DIRNAME}"
   cp ../marbleblast.exe .
   cp ../glu2d3d.dll .
   cp ../ogg.dll .
   cp ../openal32.dll .
   cp ../opengl2d3d.dll .
   cp ../vorbis.dll .
   cp ../main.cs .
   cp -R ../platinumbeta .
   cd ..
   echo "7ZIPPING"
   7z a -mx=9 "${WIN_DIRNAME}.7z" "${WIN_DIRNAME}/*"
   zip -9yr "${WIN_DIRNAME}.zip" . -i "${WIN_DIRNAME}/*"
   echo "FINISHED WINDOWS BUILD"
else
   echo "COULD NOT BUILD WINDOWS INSTALL"
fi
read
if [ $MAC_INSTALL -eq 1 ]
then
   mkdir "./${MAC_DIRNAME}"
   echo "PREPARING MAC BUILD. COPYING FILES"
   cd "./${MAC_DIRNAME}"
   mkdir "./${MAC_DIRNAME}.app"
   cd "./${MAC_DIRNAME}.app"
   cp -R ../../Contents .
   cp ../../main.cs .
   cp -R ../../platinumbeta .
   cd ../..
   echo "7ZIPPING"
   7z a -mx=9 "${MAC_DIRNAME}.7z" "${MAC_DIRNAME}/*"
   zip -9yr "${MAC_DIRNAME}.zip" . -i "${MAC_DIRNAME}/*"
   echo "FINISHED MAC BUILD"
else
   echo "COULD NOT BUILD MAC INSTALL"
fi
read
if [ $NOS_INSTALL -eq 1 ]
then
   mkdir "./${NOS_DIRNAME}"
   echo "PREPARING NO EXECUTABLE BUILD. COPYING FILES"
   cd "./${NOS_DIRNAME}"
   cp ../main.cs .
   cp -R ../platinumbeta .
   rm -f ./platinumbeta/client/scripts/demo.cs.dso
   rm -f ./platinumbeta/client/ui/ignitionGui.gui.dso
   rm -f ./platinumbeta/client/ui/ignitionstatusgui.gui.dso
   cd ..
   echo "7ZIPPING"
   7z a -mx=9 "${NOS_DIRNAME}.7z" "${NOS_DIRNAME}/*"
   zip -9yr "${NOS_DIRNAME}.zip" . -i "${NOS_DIRNAME}/*"
   echo "FINISHED NO EXECUTABLE BUILD"
else
   echo "COULD NOT BUILD NO EXECUTABLE INSTALL"
fi

read

IFS=$SAVEIFS
