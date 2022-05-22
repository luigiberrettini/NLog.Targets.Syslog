var gitRemote = Argument<string>("gitRemote");
var srcDir = Argument<string>("srcDir");
var artifactsDir = Argument<string>("artifactsDir");
var target = Argument<string>("target", "Test");
var buildConfiguration = Argument<string>("buildConfiguration", "Release");
var buildVerbosity = (DotNetVerbosity)Enum.Parse(typeof(DotNetVerbosity), Argument<string>("buildVerbosity", "Minimal"));
var softwareVersion = target.ToLower() == "nugetpack" || target.ToLower() == "nugetpush" ? Argument<string>("softwareVersion") : Argument<string>("softwareVersion", string.Empty);
var buildId = Argument<string>("buildId", null);
var buildNumber = buildId == null ? -1 : Argument<int>("buildNumber");
var commitHash = Argument<string>("commitHash");
var nuGetSource = Argument<string>("nuGetSource", null);
var nuGetApiKey = Argument<string>("nuGetApiKey", string.Empty);

var srcDirInfo = new DirectoryInfo(srcDir);
var childDirInfos = srcDirInfo.GetDirectories();
var toBuildDirInfo = childDirInfos
    .Where(x => x.GetFiles("*.csproj").Length > 0 && x.GetFiles("*.cs").Length > 0)
    .ToList();
var toBuildFolders = toBuildDirInfo
    .Select(x => x.FullName)
    .ToList();

var perProjectMsBuildSettings = new Dictionary<string, DotNetMSBuildSettings>();

Task("MSBuildSettings")
    .Does(() =>
    {
        var invalidSemVer = ("", "", "", "", "");

        (string Major, string Minor, string Patch, string PreRelease, string BuildMetadata) GetSemVerParts(string s)
        {
            var semanticVersioningPattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?(\+[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*)?$";
            var semanticVersioningRegEx = new System.Text.RegularExpressions.Regex(semanticVersioningPattern);
            var match = semanticVersioningRegEx.Match(s);
            if (!match.Success)
                return invalidSemVer;
            return
            (
                match.Groups["1"].Value,
                match.Groups["2"].Value,
                match.Groups["3"].Value,
                match.Groups["4"].Value,
                match.Groups["5"].Value.Replace("commitHash", commitHash)
            );
        }

        (string InformationalVersion, string UsesSourceLink) GetProjectInfo(string projectFilePath)
        {
            var projectFileContent = System.IO.File.ReadAllText(projectFilePath);
            var document = new System.Xml.XmlDocument();
            document.LoadXml(projectFileContent);
            var propertyGroup = document.DocumentElement["PropertyGroup"];
            var informationalVersion = propertyGroup["InformationalVersion"]?.InnerText ?? propertyGroup["Version"].InnerText + "+commitHash";
            var usesSourceLink = document.SelectSingleNode("descendant::ItemGroup[PackageReference/@Include='Microsoft.SourceLink.GitHub']") != null;
            return (informationalVersion, usesSourceLink.ToString());
        }

        int ApplyRevisionBounds(int number)
        {
            // Assembly Version metadata restricts major, minor, build, revision to a maximum value of UInt16.MaxValue - 1
            // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyname.version
            //
            // Windows Installer ProductVersion has the format major.minor.build with a maximum value of 255.255.65535
            // https://docs.microsoft.com/en-us/windows/win32/msi/productversion
            //
            // The maximum version respecting all of the above limits is 255.255.65534.65534
            // https://binary-studio.com/2017/08/18/software-versioning-windows-net
            const int maxSupportedRevision = 65534;
            var remainder = number % maxSupportedRevision;
            return remainder == 0 ? maxSupportedRevision : remainder;
        }

        // softwareVersion
        // 1.2.3(-alpha.1+commitHash)
        var providedSemVer = GetSemVerParts($"{softwareVersion}");

        var csprojFiles = childDirInfos
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName)
            .ToList();

        foreach (var project in csprojFiles)
        {
            Information($"Project: {project}");

            var projectInfo = GetProjectInfo(project);
            var (projectSemVer, projectUsesSourceLink) = (GetSemVerParts(projectInfo.InformationalVersion), projectInfo.UsesSourceLink);

            if (providedSemVer == invalidSemVer && projectSemVer == invalidSemVer)
            {
                Error("Version does not follow the semantic version specification");
                Environment.Exit(1);
            }
            var semVer = providedSemVer != invalidSemVer ? providedSemVer : projectSemVer;

            // PackageVersion
            // 1.2.3(-alpha.1)
            //
            // AssemblyVersion
            // 1.0.0.0
            //
            // AssemblyFileVersion
            // 1.2.3.<BUILD_NUMBER>
            //
            // AssemblyInformationalVersion
            // 1.2.3(-alpha.1+commitHash)
            // The GenerateAssemblyInfo target changes AssemblyInformationalVersion appending the SourceRevisionId if present:
            // +<SourceRevisionId> if no build metadata is present or .<SourceRevisionId> otherwise
            // SourceLink sets SourceRevisionId to the commitHash therefore the AssemblyInformationalVersion needs to be adjusted
            // If some projects use SourceLink and some don't it is better to handle the AssemblyInformationalVersion manually:
            // on build some dependencies are rebuilt using the AssemblyInformationalVersion of the project being built
            // Setting IncludeSourceRevisionInInformationalVersion to false avoid changes to the AssemblyInformationalVersion
            // An alternative could be preventing dependencies from being built but the script should build in the proper order
            var packageVersion = buildId == null ?
                $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}{semVer.PreRelease}" :
                $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}{semVer.PreRelease}-postRelease.{buildId}";
            var packageReleaseNotesUrl = $"{gitRemote}/releases/tag/v{packageVersion}";
            var assemblyVersion = $"{semVer.Major}.0.0.0";
            var assemblyFileVersion = buildNumber == -1 ?
                $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}.0" :
                $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}.{ApplyRevisionBounds(buildNumber)}";
            var assemblyInformationalVersion = $"{packageVersion}{semVer.BuildMetadata}";

            Information($"SourceLink: {projectUsesSourceLink}");
            Information($"AssemblyVersion: {assemblyVersion}");
            Information($"AssemblyFileVersion: {assemblyFileVersion}");
            Information($"AssemblyInformationalVersion: {assemblyInformationalVersion}");
            Information($"NuGet package version: {packageVersion}");
            Information($"Package release notes URL: {packageReleaseNotesUrl}");
            Information(Environment.NewLine);

            perProjectMsBuildSettings[project] = new DotNetMSBuildSettings { NoLogo = true, Verbosity = buildVerbosity }
                .WithProperty("AssemblyVersion", assemblyVersion)
                .WithProperty("FileVersion", assemblyFileVersion)
                .WithProperty("InformationalVersion", assemblyInformationalVersion)
                .WithProperty("Version", packageVersion)
                .WithProperty("PackageReleaseNotes", packageReleaseNotesUrl)
                .WithProperty("Deterministic", projectUsesSourceLink)
                .WithProperty("ContinuousIntegrationBuild", projectUsesSourceLink);
        }
    });

Task("Clean")
    .Does(() =>
    {
        var deleteDirectorySettings = new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        };

        if (DirectoryExists(artifactsDir))
            DeleteDirectory(artifactsDir, deleteDirectorySettings);

        var toCleanFolders = childDirInfos
            .Where(x => x.GetFiles("*.csproj").Length > 0)
            .Select(x => x.FullName)
            .ToList();

        var cleanSettings = new DotNetCleanSettings
        {
            MSBuildSettings = new DotNetMSBuildSettings { NoLogo = true, Verbosity = buildVerbosity }
        };

        foreach (var folder in toCleanFolders)
        {
            Information($"Cleaning project in folder {folder}");
            DotNetClean(folder, cleanSettings);
        }

        foreach (var folder in toCleanFolders)
        {
            var binFolder = System.IO.Path.Combine(folder, "bin");
            if (DirectoryExists(binFolder))
            {
                Information($"Deleting folder {binFolder}");
                DeleteDirectory(binFolder, deleteDirectorySettings);
            }

            var objFolder = System.IO.Path.Combine(folder, "obj");
            if (DirectoryExists(objFolder))
            {
                Information($"Deleting folder {objFolder}");
                DeleteDirectory(objFolder, deleteDirectorySettings);
            }
        }
    });

Task("RestorePackages")
    .Does(() =>
    {
        var toRestoreProjects = childDirInfos
            .Except(toBuildDirInfo)
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName)
            .ToList();

        foreach (var projectToRestore in toRestoreProjects)
            DotNetRestore(projectToRestore);
    });

Task("Build")
    .IsDependentOn("MSBuildSettings")
    .Does(() =>
    {
        var toBuildProjects = toBuildDirInfo
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName)
            .ToList();

        foreach (var projectToBuild in toBuildProjects)
        {
            var buildSettings = new DotNetBuildSettings
            {
                MSBuildSettings = perProjectMsBuildSettings[projectToBuild],
                Configuration = buildConfiguration
            };
            DotNetBuild(projectToBuild, buildSettings);
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
            var testSettings = new DotNetTestSettings { NoBuild = true, Configuration = buildConfiguration, Verbosity = buildVerbosity };
            DotNetTest(folder, testSettings);
        }
    });

Task("NuGetPack")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("Test")
    .Does(() =>
    {
        CreateDirectory(artifactsDir);

        var solutionFileName = srcDirInfo
            .GetFiles("*.sln")
            .Select(x => x.Name)
            .First();
        var solutionName = System.IO.Path.GetFileNameWithoutExtension(solutionFileName);
        var toPackProjects = childDirInfos
            .Where(x => x.Name.Contains(solutionName) && !x.Name.Contains("Tests"))
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName);

        foreach (var projectToPack in toPackProjects)
        {
            var packSettings = new DotNetPackSettings
            {
                MSBuildSettings = perProjectMsBuildSettings[projectToPack],
                Configuration = buildConfiguration,
                NoBuild = true,
                OutputDirectory = artifactsDir
            };
            DotNetPack(projectToPack, packSettings);
        }
    });

Task("NuGetPush")
    .IsDependentOn("Clean")
    .IsDependentOn("NuGetPack")
    .Does(() =>
    {
        if (nuGetSource == null)
        {
            Information("Missing NuGet source:");
            Information(" - test source = https://apiint.nugettest.org/v3/index.json");
            Information(" - live source = https://api.nuget.org/v3/index.json");
            return;
        }
        Information($"NuGet source: {nuGetSource}");

        var packageSearchPattern = System.IO.Path.Combine(artifactsDir, "*.nupkg");
        var nuGetPushSettings = new DotNetNuGetPushSettings { Source = nuGetSource, ApiKey = nuGetApiKey };
        DotNetNuGetPush(packageSearchPattern, nuGetPushSettings);
    });

Task("ZipArtifacts")
    .Does(() =>
    {
        if (!DirectoryExists(artifactsDir) || !System.IO.Directory.EnumerateFileSystemEntries(artifactsDir).Any())
        {
            Information("The artifacts directory is missing or empty");
            return;
        }

        var artifactsDirFullName = new DirectoryInfo(artifactsDir).FullName;
        var artifactsZipFile = System.IO.Path.Combine(artifactsDirFullName, "artifacts.zip");
        Zip(artifactsDirFullName, artifactsZipFile);
    });

RunTarget(target);