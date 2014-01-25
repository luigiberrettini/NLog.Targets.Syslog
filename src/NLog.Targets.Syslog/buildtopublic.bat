@echo off
call build.bat
echo Publishing package...
for /f %%a in ('dir /b /s .\deploy\*.nupkg') do call ..\..\tools\NuGet.exe push %%a