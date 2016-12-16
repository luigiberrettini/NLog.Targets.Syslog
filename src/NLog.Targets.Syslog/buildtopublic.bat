@echo off

call build.bat

echo Publishing NuGet package...

..\..\tools\NuGet.exe push .\deploy\*.nupkg -Source https://www.nuget.org/api/v2/package %1