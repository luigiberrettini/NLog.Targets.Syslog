@echo off

set scriptDir=%~dp0
set scriptDir=%scriptDir:~0,-1%
set rootDir=%scriptDir%\..

set NuGetExe="%scriptDir%\NuGet.exe"
set MSBuildExe="%ProgramFiles%\MSBuild\14.0\Bin\MsBuild.exe"
if exist "%ProgramFiles(x86)%\MSBuild\14.0\Bin\MsBuild.exe" set MSBuildExe="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MsBuild.exe"

set softwareName=NLog.Targets.Syslog
set schemaName=%softwareName%.Schema
set deployDir="%rootDir%\deploy"
set srcDir=%rootDir%\src
set packagesDir="%srcDir%\packages"
set solutionFile="%srcDir%\%softwareName%.sln"
set projectDir=%srcDir%\%softwareName%
set projectFile="%projectDir%\%softwareName%.csproj"
set versionInfoFile="%projectDir%\properties\AssemblyInfo.cs"
set schemaFile="%projectDir%\%softwareName%.xsd"
set schemaNuSpecFile="%projectDir%\%schemaName%.nuspec"
set toBeDeployed="%deployDir%\*.nupkg"

set publish="%1"
set NuGetApiKey=%2


if exist %deployDir% (
    @rmdir /S /Q %deployDir%
)
mkdir %deployDir%

if not exist %packagesDir% (
    set Configuration=Release
    set Platform=AnyCPU
    echo Restoring dependency packages...
    %NuGetExe% restore %solutionFile% -verbosity quiet -msbuildversion 14 -noninteractive
    echo.
)

echo Building...
%MSBuildExe% %solutionFile% /t:Build /p:Configuration=Release /property:"Platform=Any CPU" /v:minimal /nologo
echo.

echo Creating %softwareName% NuGet package...
%NuGetExe% pack %projectFile% -msbuildversion 14 -outputdirectory %deployDir% -properties Configuration=Release -properties Platform=AnyCPU -properties version=%packageVersion%
echo.

echo Creating %schemaName% NuGet package...
for /f %%i in ('PowerShell -File %scriptDir%\get-version.ps1 -versionInfoFile %versionInfoFile%') do set packageVersion=%%i
%NuGetExe% pack %schemaNuSpecFile% -outputdirectory %deployDir% -properties version=%packageVersion%

if %publish%=="publish" (
echo.
echo Publishing NuGet package...
%NuGetExe% push %toBeDeployed% -verbosity quiet -source https://www.nuget.org/api/v2/package %NuGetApiKey%
)