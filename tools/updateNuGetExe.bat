@echo off

set scriptDir=%~dp0
set scriptDir=%scriptDir:~0,-1%
set NuGetExe="%scriptDir%\NuGet.exe"

%NuGetExe% update -verbosity quiet -self