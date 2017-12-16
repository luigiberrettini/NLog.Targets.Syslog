#!/bin/bash

function findPackageAssembly {
    dir=$1
    packageName=$2
    assemblyName=$3
    echo $(find "$dir/$packageName" -name "$assemblyName")
}

function installPackage {
    dir=$1
    packageName=$2
    echo "Installing $packageName in $dir..."
    mkdir -p $dir
    contents='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup></Project>'
    proj="$packagesDir/project.csproj"
    echo $contents > $proj
    dotnet add "$proj" package $packageName --package-directory $dir
}

scriptDir=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

rootDir=$scriptDir; while [ $(ls -Ald "$rootDir/.git" 2>/dev/null | wc -l) -eq 0 ]; do rootDir="$rootDir/.."; done
srcDir="$rootDir/src"
toolsDir="$rootDir/tools"
packagesDir="$toolsDir/cake"
cakePackageName='cake.coreclr'
cakeAssemblyName='Cake.dll'
cakeAssemblyPath=$(findPackageAssembly $packagesDir $cakePackageName $cakeAssemblyName)
if [ ! -f "$cakeAssemblyPath" ]; then
    installPackage $packagesDir $cakePackageName
    cakeAssemblyPath=$(findPackageAssembly $packagesDir $cakePackageName $cakeAssemblyName)
    if [ ! -f "$cakeAssemblyPath" ]; then
        echo "Cannot find Cake assembly '$cakeDll'"
        exit 1
    fi
fi
cakeScript="$scriptDir/build.cake"

dotnet "$cakeAssemblyPath" "$cakeScript" "-srcDir=$srcDir" "-toolsDir=$toolsDir" "-commitHash=$(git rev-parse --short HEAD)" "$@"