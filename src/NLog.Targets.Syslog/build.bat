@echo off

if not exist ..\packages (
    set Configuration=Release
    set Platform=AnyCPU
    echo Restoring dependency packages...
    ..\..\tools\NuGet.exe restore ..\NLog.Targets.Syslog.sln -msbuildversion 14
)

echo Building...
@rmdir /S /Q deploy
set MSBuildExe="%ProgramFiles%\MSBuild\14.0\Bin\MsBuild.exe"
if exist "%ProgramFiles(x86)%\MSBuild\14.0\Bin\MsBuild.exe" set MSBuildExe="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MsBuild.exe"
%MSBuildExe% NLog.Targets.Syslog.csproj /t:Build /p:Configuration=Release /property:Platform=AnyCPU /v:minimal /nologo

echo Creating NuGet package
..\..\tools\NuGet.exe pack NLog.Targets.Syslog.csproj -msbuildversion 14 -Prop Configuration=Release -Prop Platform=AnyCPU
mkdir deploy
for /f %%a in ('dir /b /s .\*.nupkg') do call move /Y %%a deploy