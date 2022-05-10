@echo off
echo.
echo Yo man. I'm the destructive CS AND GUI FUCKING DELETER.
echo Don't cry if I delete them. They won't come back.
set /p choice= Are you sure you want to continue? If you are sure, type "fuck". 

if "%choice%" == "fuck" (goto ok) else (goto bad)

:ok
echo ..DELETING FILES!!!
pause
echo Deleting cs/guis.
del /s *.cs
del /s *.gui
echo Done.
set /p second= Type "fuck" if you want to delete PREFS too.
if "%second%" == "fuck" (goto prefs) else (goto end)
goto end

:prefs
echo Deleting prefs
del /s config.cs.dso
del /s prefs.cs.dso
echo Done.
pause
goto end

:bad
echo Exiting...
goto end

:end
echo Done.