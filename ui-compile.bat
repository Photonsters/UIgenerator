@echo off
set /p folder=IMAGE FOLDER:
set /p file=UI NEW UI FILENAME: 
echo. 
echo Compiling... %folder% sprites to %file%
echo. 
UIgenerator.exe %folder% %file%
echo.
echo done.
echo.
pause