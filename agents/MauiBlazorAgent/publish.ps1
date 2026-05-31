<#
.SYNOPSIS
    Publishes MauiBlazorAgent as an unpackaged, self-contained Windows app.

.DESCRIPTION
    Wraps the `dotnet publish` invocation used to produce a WindowsPackageType=None
    (unpackaged) build with a self-contained Windows App SDK runtime.

    Output lands in:
        MauiBlazorAgent\bin\<Configuration>\<framework>\win-x64\publish\

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.EXAMPLE
    .\publish.ps1

.EXAMPLE
    .\publish.ps1 -Configuration Debug
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Resolve paths relative to this script so it works from any working directory.
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $scriptRoot "MauiBlazorAgent\MauiBlazorAgent.csproj"
$framework = "net10.0-windows10.0.19041.0"

Write-Host "Publishing $project ($Configuration, $framework)..." -ForegroundColor Cyan

dotnet publish $project `
    -f $framework `
    -c $Configuration `
    -p:WindowsPackageType=None `
    -p:RuntimeIdentifierOverride=win10-x64 `
    -p:WindowsAppSDKSelfContained=true

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$publishDir = Join-Path $scriptRoot "MauiBlazorAgent\bin\$Configuration\$framework\win-x64\publish"
Write-Host "Publish succeeded. Output: $publishDir" -ForegroundColor Green
