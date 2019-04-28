function Find-ToolAssembly {
    Param($dir, $packageName, $packageVersion, $toolName, $assemblyName)
    $toolDir = Join-Path $dir (Join-Path $packageName $packageVersion)
    if (-Not (Test-Path $toolDir)) {
        return $null
    }
    if (-Not (Get-ChildItem -Force -Recurse $toolDir -Filter $assemblyName)) {
        return $null
    }
    return (Get-ChildItem -Force $toolDir -Filter "$toolName.exe").FullName
}

function Install-Tool {
    Param($dir, $packageName, $packageVersion)
    Write-Host "Installing '$packageName' version '$packageVersion' in '$dir'..."
    $toolDir = Join-Path $dir (Join-Path $packageName $packageVersion)
    if (Test-Path $toolDir) {
        Remove-Item -Force -Recurse -Path $toolDir
    }
    New-Item -Type Directory -Path $toolDir -Force | Out-Null
    dotnet tool install $packageName --tool-path $toolDir --version $packageVersion | Out-Null
}

function Get-ToolAssemblyPath {
    Param($scriptArgs, $toolsDir, $packageName, $packageVersionArgSwitch, $toolName, $assemblyName)
    $packageVersionArg = $scriptArgs | Where-Object { $_ -ne $null -and $_.StartsWith($packageVersionArgSwitch) }
    if ($packageVersionArg -eq $null) {
        $packageVersion = (Find-Package -Source 'http://www.nuget.org/api/v2' -Name $packageName).Version
    }
    else {
        $packageVersion = $packageVersionArg.Split('=')[1]
    }
    $assemblyPath = Find-ToolAssembly $toolsDir $packageName $packageVersion $toolName $assemblyName
    if (($assemblyPath -eq $null) -or -Not (Test-Path $assemblyPath)) {
        Install-Tool $toolsDir $packageName $packageVersion
        $assemblyPath = Find-ToolAssembly $toolsDir $packageName $packageVersion $toolName $assemblyName
        if (($assemblyPath -eq $null) -or -Not (Test-Path $assemblyPath)) {
            Throw "Cannot find '$toolName' and/or '$assemblyName' in directory '$toolsDir' for package '$packageName' of version '$packageVersion'"
            exit 1
        }
    }
    return $assemblyPath
}

$rootDir = $PSScriptRoot; while ($(Get-ChildItem -Force -Filter '.git' $rootDir | Measure-Object).Count -eq 0) { $rootDir = (Get-Item (Join-Path $rootDir '..')).FullName }
$srcDir = Join-Path $rootDir 'src'
$buildDir = Join-Path $rootDir 'build'
$scriptsDir = Join-Path $buildDir 'scripts'
$toolsDir = Join-Path $buildDir 'tools'
$artifactsDir = Join-Path $buildDir 'artifacts'

$cakeAssemblyPath = Get-ToolAssemblyPath $args $toolsDir 'cake.tool' '--cakePackageVersion=' 'dotnet-cake' 'cake.dll'
$cakeScript = Join-Path $scriptsDir 'cake.cs'
$gitRemote = $(git remote get-url origin) -replace '.git$', ''
$commitHash = $(git rev-parse --short HEAD)

$cakeArgs = @("$cakeScript", "--gitRemote=$gitRemote", "--srcDir=$srcDir", "--artifactsDir=$artifactsDir", "--commitHash=$commitHash") + $args
& $cakeAssemblyPath $cakeArgs