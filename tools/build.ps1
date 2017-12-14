function Find-PackageAssembly {
    Param($dir, $packageName, $assemblyName)
    return (Get-ChildItem -Force -Recurse $dir -Filter $assemblyName).FullName
}

function Install-Package {
    Param($dir, $packageName)
    Write-Host "Installing $packageName in $dir..."
    New-Item -Type Directory -Path $dir -Force | Out-Null
    $contents = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup></Project>'
    $proj = Join-Path $dir 'project.csproj'
    Out-File -InputObject $contents -FilePath $proj
    dotnet add "$proj" package $packageName --package-directory $dir
}

$scriptDir = $PSScriptRoot
$rootDir = $scriptDir; while ($(Get-ChildItem -Force -Filter '.git' $rootDir | Measure-Object).Count -eq 0) { $rootDir = Join-Path $rootDir '..' }
$srcDir = Join-Path $rootDir 'src'
$toolsDir = Join-Path $rootDir 'tools'
$packagesDir = Join-Path $toolsDir 'cake'
$cakePackageName = 'cake.coreclr'
$cakeAssemblyName = 'Cake.dll'
$cakeAssemblyPath = Find-PackageAssembly $packagesDir $cakePackageName $cakeAssemblyName
if (($cakeAssemblyPath -eq $null) -or -not(Test-Path $cakeAssemblyPath)) {
    Install-Package $packagesDir $cakePackageName
    $cakeAssemblyPath = Find-PackageAssembly $packagesDir $cakePackageName $cakeAssemblyName
    if (($cakeAssemblyPath -eq $null) -or -not(Test-Path $cakeAssemblyPath)) {
        Throw "Cannot find $cakeAssemblyName"
        exit 1
    }
}
$cakeScript = Join-Path $scriptDir 'build.cake'

& dotnet "$cakeAssemblyPath" "$cakeScript" "-srcDir=$srcDir" "-toolsDir=$toolsDir" "-commitHash=$(git rev-parse --short HEAD)" $args