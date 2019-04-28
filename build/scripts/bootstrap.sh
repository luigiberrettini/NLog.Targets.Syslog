#!/bin/bash

function findToolAssembly {
    local resultvar=$1
    local dir=$2
    local packageName=$3
    local packageVersion=$4
    local toolName=$5
    local assemblyName=$6
    local packageAssembly=$(find "$dir/$packageName/$packageVersion" -iname "$assemblyName" 2>/dev/null)
    if [ ! -f "$packageAssembly" ]; then
        eval $resultvar="$packageAssembly"
    else
        local toolAssembly=$(find "$dir/$packageName/$packageVersion" -iname "$toolName" 2>/dev/null)
        eval $resultvar="$toolAssembly"
    fi
}

function installTool {
    local dir=$1
    local packageName=$2
    local packageVersion=$3
    echo "Installing '$packageName' version '$packageVersion' in '$dir'..."
    local toolDir="$dir/$packageName/$packageVersion"
    if [ -d "$toolDir" ]; then
        rm -rf $toolDir
    fi
    mkdir -p $toolDir
    dotnet tool install $packageName --tool-path $toolDir --version $packageVersion &>/dev/null
}

function getToolAssemblyPath {
    local resultvar=$1
    local scriptArgsName=$2[@]
    local toolsDir=$3
    local packageName=$4
    local packageVersionArgSwitch=$5
    local toolName=$6
    local assemblyName=$7
    local scriptArgs=("${!scriptArgsName}")
    for item in ${scriptArgs[@]}; do
        if [[ $item == $packageVersionArgSwitch* ]]; then
            local packageVersion=${item/$packageVersionArgSwitch/}
        fi
    done
    if [ -z $packageVersion ]; then
        local packageVersionSearchUrl="https://api-v2v3search-0.nuget.org/autocomplete?id=$packageName"
        local packageVersion=$(curl --silent $packageVersionSearchUrl | jq .data[-1] | sed -e 's/^"//' -e 's/"$//')
    fi
    findToolAssembly assemblyPath $toolsDir $packageName $packageVersion $toolName $assemblyName
    if [ ! -f "$assemblyPath" ]; then
        installTool $toolsDir $packageName $packageVersion
        findToolAssembly assemblyPath $toolsDir $packageName $packageVersion $toolName $assemblyName
        if [ ! -f "$assemblyPath" ]; then
            echo "Cannot find '$toolName' and/or '$assemblyName' in directory '$toolsDir' for package '$packageName' of version '$packageVersion'"
            exit 1
        fi
    fi
    eval $resultvar="$assemblyPath"
}

externalArgs="$@"

rootDir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd); while [ $(ls -Ald "$rootDir/.git" 2>/dev/null | wc -l) -eq 0 ]; do rootDir="$rootDir/.."; done; rootDir=$(cd "$rootDir" && pwd)
srcDir="$rootDir/src"
buildDir="$rootDir/build"
scriptsDir="$buildDir/scripts"
toolsDir="$buildDir/tools"
artifactsDir="$buildDir/artifacts"

getToolAssemblyPath cakeAssemblyPath externalArgs $toolsDir 'cake.tool' '--cakePackageVersion=' 'dotnet-cake' 'cake.dll'
cakeScript="$scriptsDir/cake.cs"
gitRemote=$(git remote get-url origin | sed -e 's/.git$//')
commitHash=$(git rev-parse --short HEAD)

internalArgs=("$cakeAssemblyPath" "$cakeScript" "--gitRemote=$gitRemote" "--srcDir=$srcDir" "--artifactsDir=$artifactsDir" "--commitHash=$commitHash")
"${internalArgs[@]}" $externalArgs