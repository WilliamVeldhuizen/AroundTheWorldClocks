<#
.SYNOPSIS
    Builds the Around The World Clocks installer (MSI).

.DESCRIPTION
    1. Publishes the app as a self-contained, single-file win-x64 exe (no .NET install needed on the target PC).
    2. Builds installer\AroundTheWorldClocks.Installer.wixproj (WiX Toolset v5, restored from NuGet).
    Output: artifacts\AroundTheWorldClocks-<version>.msi

    The version comes from <Version> in src\AroundTheWorldClocks\AroundTheWorldClocks.csproj.
    Bump it for every release, otherwise an installed copy won't be upgraded.
#>
$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$appProject = Join-Path $root 'src\AroundTheWorldClocks\AroundTheWorldClocks.csproj'
$installerProject = Join-Path $root 'installer\AroundTheWorldClocks.Installer.wixproj'
$artifacts = Join-Path $root 'artifacts'
$publishDir = Join-Path $artifacts 'publish'

$version = (dotnet msbuild $appProject -getProperty:Version).Trim()
Write-Host "Building Around The World Clocks $version" -ForegroundColor Cyan

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

dotnet publish $appProject -c Release -r win-x64 --self-contained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

dotnet build $installerProject -c Release `
    -p:ProductVersion=$version `
    -p:AppPublishDir="$publishDir\" `
    -o $artifacts
if ($LASTEXITCODE -ne 0) { throw "Installer build failed" }

$msi = Join-Path $artifacts "AroundTheWorldClocks-$version.msi"
Write-Host "Installer: $msi ($([math]::Round((Get-Item $msi).Length / 1MB, 1)) MB)" -ForegroundColor Green
