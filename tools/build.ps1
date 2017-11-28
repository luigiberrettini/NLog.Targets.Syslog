# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

dotnet restore .\src\NLog.Targets.Syslog\
dotnet restore .\src\TestApp\
dotnet restore .\src\TestAppCore\
dotnet build .\src\TestApp\  --configuration release 
dotnet build .\src\TestAppCore\  --configuration release
dotnet pack .\src\NLog.Targets.Syslog\  --configuration release -o ..\..\ /p:Version=$args /p:PackageOutputPath=..\..\

exit $LASTEXITCODE