var gitRemote = Argument<string>("gitRemote");
var srcDir = Argument<string>("srcDir");
var artifactsDir = Argument<string>("artifactsDir");
var target = Argument<string>("target", "Test");
var buildConfiguration = Argument<string>("buildConfiguration", "Release");
var buildVerbosity = (DotNetCoreVerbosity)Enum.Parse(typeof(DotNetCoreVerbosity), Argument<string>("buildVerbosity", "Minimal"));
var softwareVersion = target.ToLower() == "nugetpack" || target.ToLower() == "nugetpush" ? Argument<string>("softwareVersion") : Argument<string>("softwareVersion", string.Empty);
var buildNumber = Argument<int>("buildNumber", 0);
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

var perProjectMsBuildSettings = new Dictionary<string, DotNetCoreMSBuildSettings>();

Task("MSBuildSettings")
    .Does(() =>
    {
        var invalidVersion = ("", "", "", "", "");

        (string major, string minor, string patch, string preRelease, string buildMetadata) GetSemanticVersionParts(string s)
        {
            var semanticVersioningPattern = @"([0-9]+\.[0-9]+\.[0-9]+)(\-[0-9A-Za-z-\.]+){0,1}(\+[0-9A-Za-z-\.]+){0,1}";
            var semanticVersioningRegEx = new System.Text.RegularExpressions.Regex(semanticVersioningPattern);
            var match = semanticVersioningRegEx.Match(s);
            if (!match.Success)
                return invalidVersion;
            var core = match.Groups[1].Value.Split('.');
            if (match.Groups.Count == 2)
                return (core[0], core[1], core[2], "", "");
            if (match.Groups.Count == 3)
                return match.Groups[1].Value.StartsWith("-") ?
                    (core[0], core[1], core[2], match.Groups[2].Value, "") :
                    (core[0], core[1], core[2], "", match.Groups[2].Value.Replace("commitHash", commitHash));
            return (core[0], core[1], core[2], match.Groups[2].Value, match.Groups[3].Value.Replace("commitHash", commitHash));
        }

        string GetVersionFromProjectFile(string projectFileContent)
        {
            var document = new System.Xml.XmlDocument();
            document.LoadXml(projectFileContent);
            var csprojProps = document.DocumentElement["PropertyGroup"];
            return (csprojProps["InformationalVersion"] ?? csprojProps["Version"]).InnerText;
        }

        bool ProjectUsesSourceLink(string projectFileContent)
        {
            var document = new System.Xml.XmlDocument();
            document.LoadXml(projectFileContent);
            return document.SelectSingleNode("descendant::ItemGroup[PackageReference/@Include='Microsoft.SourceLink.GitHub']") != null;
        }

        // softwareVersion
        // 1.2.3(-alpha-01)
        var providedVersion = GetSemanticVersionParts($"{softwareVersion}+commitHash");

        var csprojFiles = childDirInfos
            .SelectMany(x => x.GetFiles("*.csproj"))
            .Select(x => x.FullName)
            .ToList();

        foreach (var project in csprojFiles)
        {
            var projectFileContent = System.IO.File.ReadAllText(project);

            var csprojVersion = GetSemanticVersionParts(GetVersionFromProjectFile(projectFileContent));
            var usesSourceLink = ProjectUsesSourceLink(projectFileContent);

            var version = providedVersion != invalidVersion ? providedVersion : csprojVersion;
            if (version == invalidVersion)
            {
                Error("Version is invalid");
                Environment.Exit(1);
            }

            // PackageVersion
            // 1.2.3(-alpha-01)
            //
            // AssemblyVersion
            // 1.0.0.0
            //
            // AssemblyFileVersion
            // 1.2.3.<BUILD_NUMBER>
            //
            // AssemblyInformationalVersion
            // 1.2.3(-alpha-01+commitHash)
            // The GenerateAssemblyInfo target changes AssemblyInformationalVersion appending the SourceRevisionId if present:
            // +<SourceRevisionId> if no build metadata is present or .<SourceRevisionId> otherwise
            // SourceLink sets SourceRevisionId to the commitHash therefore the AssemblyInformationalVersion needs to be adjusted
            // If some projects use SourceLink and some don't it is better to handle the AssemblyInformationalVersion manually:
            // on build some dependencies are rebuilt using the AssemblyInformationalVersion of the project being built
            // Setting IncludeSourceRevisionInInformationalVersion to false avoid changes to the AssemblyInformationalVersion
            // An alternative could be preventing dependencies from being built but the script should build in the proper order
            var packageVersion = $"{version.major}.{version.minor}.{version.patch}{version.preRelease}";
            var packageReleaseNotesUrl = $"{gitRemote}/releases/tag/v{packageVersion}";
            var assemblyVersion = $"{version.major}.0.0.0";
            var assemblyFileVersion = $"{version.major}.{version.minor}.{version.patch}.{buildNumber}";
            var assemblyInformationalVersion = $"{packageVersion}{version.buildMetadata}";

            Information($"Project: {project}");
            Information($"SourceLink: {usesSourceLink}");
            Information($"AssemblyVersion: {assemblyVersion}");
            Information($"AssemblyFileVersion: {assemblyFileVersion}");
            Information($"AssemblyInformationalVersion: {assemblyInformationalVersion}");
            Information($"NuGet package version: {packageVersion}");
            Information($"Package release notes URL: {packageReleaseNotesUrl}");
            Information(Environment.NewLine);

            perProjectMsBuildSettings[project] = new DotNetCoreMSBuildSettings { NoLogo = true, Verbosity = buildVerbosity }
                .WithProperty("AssemblyVersion", assemblyVersion)
                .WithProperty("FileVersion", assemblyFileVersion)
                .WithProperty("InformationalVersion", assemblyInformationalVersion)
                .WithProperty("Version", packageVersion)
                .WithProperty("PackageReleaseNotes", packageReleaseNotesUrl)
                .WithProperty("Deterministic", usesSourceLink.ToString())
                .WithProperty("ContinuousIntegrationBuild", usesSourceLink.ToString());
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

        var cleanSettings = new DotNetCoreCleanSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings { NoLogo = true, Verbosity = buildVerbosity }
        };

        foreach (var folder in toCleanFolders)
        {
            Information($"Cleaning project in folder {folder}");
            DotNetCoreClean(folder, cleanSettings);
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
            DotNetCoreRestore(projectToRestore);
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
            var buildSettings = new DotNetCoreBuildSettings
            {
                MSBuildSettings = perProjectMsBuildSettings[projectToBuild],
                Configuration = buildConfiguration
            };
            DotNetCoreBuild(projectToBuild, buildSettings);
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
            var packSettings = new DotNetCorePackSettings
            {
                MSBuildSettings = perProjectMsBuildSettings[projectToPack],
                Configuration = buildConfiguration,
                NoBuild = true,
                OutputDirectory = artifactsDir
            };
            DotNetCorePack(projectToPack, packSettings);
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
        var nuGetPushSettings = new DotNetCoreNuGetPushSettings { Source = nuGetSource, ApiKey = nuGetApiKey };
        DotNetCoreNuGetPush(packageSearchPattern, nuGetPushSettings);
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