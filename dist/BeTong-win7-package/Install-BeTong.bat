@echo off
setlocal

set "APPDIR=%LOCALAPPDATA%\BeTong"
set "DESKTOP=%USERPROFILE%\Desktop"

if not exist "%APPDIR%" mkdir "%APPDIR%"
copy /Y "%~dp0BeTong.exe" "%APPDIR%\BeTong.exe" >nul
copy /Y "%~dp0BeTong.exe.config" "%APPDIR%\BeTong.exe.config" >nul

set "VBS=%TEMP%\CreateBeTongShortcut.vbs"
echo Set shell = CreateObject("WScript.Shell") > "%VBS%"
echo Set shortcut = shell.CreateShortcut("%DESKTOP%\BeTong.lnk") >> "%VBS%"
echo shortcut.TargetPath = "%APPDIR%\BeTong.exe" >> "%VBS%"
echo shortcut.WorkingDirectory = "%APPDIR%" >> "%VBS%"
echo shortcut.IconLocation = "%APPDIR%\BeTong.exe,0" >> "%VBS%"
echo shortcut.Save >> "%VBS%"
cscript //nologo "%VBS%" >nul
del "%VBS%" >nul 2>nul

echo BeTong da duoc cai dat vao: %APPDIR%
echo Shortcut da duoc tao tren Desktop.
pause
