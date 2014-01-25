@echo off
echo Building package...
rmdir /S /Q deploy
..\..\tools\NuGet.exe pack NLog.Targets.Syslog.csproj -Prop Configuration=Release
mkdir deploy
for /f %%a in ('dir /b /s .\*.nupkg') do call move /Y %%a deploy