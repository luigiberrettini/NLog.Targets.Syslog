var srcDir = Argument<string>("srcDir");
var toolsDir = Argument<string>("toolsDir");
var target = Argument<string>("target", "Test");
var buildConfiguration = Argument<string>("buildConfiguration", "Release");
var buildVerbosity = (DotNetCoreVerbosity)Enum.Parse(typeof(DotNetCoreVerbosity), Argument<string>("buildVerbosity", "Minimal"));
var softwareVersion = target.ToLower() == "nugetpush" ? Argument<string>("softwareVersion") : Argument<string>("softwareVersion", string.Empty);
var buildNumber = Argument<int>("buildNumber", 0);
var commitHash = Argument<string>("commitHash");
var nuGetApiKey = Argument<string>("nuGetApiKey", string.Empty);

var srcDirInfo = new DirectoryInfo(srcDir);
var artifactsFolder = System.IO.Path.Combine(toolsDir, "artifacts");
var childDirInfos = srcDirInfo.GetDirectories();
var toBuildFolders = childDirInfos
    .Where(x => x.GetFiles("*.csproj").Length > 0 && x.GetFiles("*.cs").Length > 0)
    .Select(x => x.FullName)
    .ToList();


Task("Clean")
    .Does(() =>
    {
        var deleteDirectorySettings = new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        };

        if (DirectoryExists(artifactsFolder))
            DeleteDirectory(artifactsFolder, deleteDirectorySettings);

        var cleanSettings = new DotNetCoreCleanSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true },
            Verbosity = buildVerbosity
        };

        foreach (var folder in toBuildFolders)
        {
            Information(folder);
            DotNetCoreClean(folder, cleanSettings);

            var binFolder = System.IO.Path.Combine(folder, "bin");
            if (DirectoryExists(binFolder))
                DeleteDirectory(binFolder, deleteDirectorySettings);
            var objFolder = System.IO.Path.Combine(folder, "obj");
            if (DirectoryExists(objFolder))
                DeleteDirectory(objFolder, deleteDirectorySettings);
        }
    });

Task("Build")
    .Does(() =>
    {
        var buildSettings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true },
            Configuration = buildConfiguration,
            Verbosity = buildVerbosity
        };

        foreach (var folder in toBuildFolders)
        {
            Information(folder);
            DotNetCoreBuild(folder, buildSettings);
        }
    });

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var testFolders = toBuildFolders.Where(x => x.Contains("Tests"));
        foreach (var folder in testFolders)
        {
            Information(folder);
            var testSettings = new DotNetCoreTestSettings { NoBuild = true, Configuration = buildConfiguration, Verbosity = buildVerbosity };
            DotNetCoreTest(folder, testSettings);
        }
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        CreateDirectory(artifactsFolder);

        var solutionFileName = srcDirInfo
            .GetFiles("*.sln")
            .Select(x => x.Name)
            .First();
        var solutionName = System.IO.Path.GetFileNameWithoutExtension(solutionFileName);
        var toPackProjects = childDirInfos
            .Where(x => x.Name.Contains(solutionName) && !x.Name.Contains("Tests"))
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName);

        var providedVersion = softwareVersion.Split(new [] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (providedVersion.Length == 1)
            providedVersion = new [] { providedVersion[0], ""};
        
        foreach (var projectToPack in toPackProjects)
        {
            var content = System.IO.File.ReadAllText(projectToPack);
            var document = new System.Xml.XmlDocument();
            document.LoadXml(content);
            var csprojVersion = document.DocumentElement["PropertyGroup"]["Version"].InnerText.Split(new [] { '-' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var versionPrefix = providedVersion.Length == 0 ? csprojVersion[0].Split('.') : providedVersion[0].Split('.');
            var versionSuffix = (providedVersion.Length == 0 ? csprojVersion[1] : providedVersion[1]).Replace("commitHash", commitHash);
            if (versionSuffix.Length > 0)
                versionSuffix = "-" + versionSuffix;

            // AssemblyVersion
            // 1.0.0
            //
            // AssemblyFileVersion
            // 1.2.3.<BUILD_NUMBER>
            //
            // AssemblyInformationalVersion
            // 1.2.3(-alpha-commitHash)
            var assemblyVersion = String.Format("{0}.{1}.{1}.{1}", versionPrefix[0], 0);
            var assemblyFileVersion = String.Format("{0}.{1}.{2}.{3}", versionPrefix[0], versionPrefix[1], versionPrefix[2], buildNumber);
            var assemblyInformationalVersion = String.Format("{0}.{1}.{2}{3}", versionPrefix[0], versionPrefix[1], versionPrefix[2], versionSuffix);
            var packageReleaseNotesUrl = String.Format("{0}{1}", "https://github.com/graffen/NLog.Targets.Syslog/releases/tag/v", assemblyInformationalVersion);

            Information("Project: {0}", projectToPack);
            Information("AssemblyVersion: {0}", assemblyVersion);
            Information("AssemblyFileVersion: {0}", assemblyFileVersion);
            Information("AssemblyInformationalVersion/NuGet package version: {0}", assemblyInformationalVersion);
            Information("Package release notes URL: {0}", packageReleaseNotesUrl);

            var packSettings = new DotNetCorePackSettings
            {
                MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true }
                    .WithProperty("AssemblyVersion", assemblyVersion)
                    .WithProperty("FileVersion", assemblyFileVersion)
                    .WithProperty("InformationalVersion", assemblyInformationalVersion)
                    .WithProperty("Version", assemblyInformationalVersion)
                    .WithProperty("PackageReleaseNotes", packageReleaseNotesUrl),
                Configuration = buildConfiguration,
                Verbosity = buildVerbosity,
                NoBuild = true,
                OutputDirectory = artifactsFolder
            };
            DotNetCorePack(projectToPack, packSettings);
        }
    });

Task("NuGetPush")
    .IsDependentOn("Clean")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        var packageSearchPattern = System.IO.Path.Combine(artifactsFolder, "*.nupkg");
        var nuGetPushSettings = new DotNetCoreNuGetPushSettings { Source = "https://www.nuget.org/api/v2/package", ApiKey = nuGetApiKey };
        DotNetCoreNuGetPush(packageSearchPattern, nuGetPushSettings);
    });

RunTarget(target);