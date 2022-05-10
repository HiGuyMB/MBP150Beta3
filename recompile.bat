@echo off
echo.
echo Whee yo again! I'm the constructive CS AND GUI FUCKING RECOMPILER.
echo I'll remake all your damn DSOs again.
set /p choice= Are you sure you want to continue? If you are sure, type "gay". 

if "%choice%" == "gay" (goto ok) else (goto bad)

:ok
echo ..RECOMPILING!!!
pause
marbleblast.exe -compileall
echo Done.
pause
goto end

:bad
echo Exiting...
goto end

:end
echo Done.