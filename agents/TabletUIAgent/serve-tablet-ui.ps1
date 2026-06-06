<#
.SYNOPSIS
    Publishes TabletUIAgent (Blazor WASM PWA), serves the published output over
    HTTP with `dotnet serve`, and exposes it publicly through ngrok.

.DESCRIPTION
    TabletUIAgent is a PWA, so testing it requires a real HTTP host rather than
    the in-memory dev server. This script chains the three steps:
        1. dotnet publish  -> static files in a publish folder
        2. dotnet serve    -> local HTTP server over the published wwwroot
        3. ngrok http       -> public HTTPS tunnel to the local server

    Press Ctrl+C to tear everything down (serve + ngrok are stopped on exit).

.PARAMETER Port
    Local port for dotnet serve. Default 5080.

.PARAMETER Configuration
    Build configuration to publish. Default Release.

.PARAMETER SkipPublish
    Reuse the existing published output instead of republishing.
#>
[CmdletBinding()]
param(
    [int]    $Port          = 5080,
    [string] $Configuration = "Release",
    [switch] $SkipPublish
)

$ErrorActionPreference = "Stop"

# --- Paths -----------------------------------------------------------------
$ScriptRoot  = $PSScriptRoot
$ProjectPath = Join-Path $ScriptRoot "TabletUIAgent\TabletUIAgent.csproj"
$PublishDir  = Join-Path $ScriptRoot "publish"
# Blazor WASM publishes the static site under wwwroot of the publish folder.
$WebRoot     = Join-Path $PublishDir "wwwroot"

if (-not (Test-Path $ProjectPath)) {
    throw "Project not found at $ProjectPath"
}

# --- 1. Publish ------------------------------------------------------------
if (-not $SkipPublish) {
    Write-Host "==> Publishing TabletUIAgent ($Configuration)..." -ForegroundColor Cyan
    dotnet publish $ProjectPath -c $Configuration -o $PublishDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)." }
} else {
    Write-Host "==> Skipping publish (using existing output)." -ForegroundColor Yellow
}

if (-not (Test-Path $WebRoot)) {
    throw "Published web root not found at $WebRoot. Run without -SkipPublish first."
}

# --- ngrok resolution ------------------------------------------------------
# The bare `ngrok` on PATH resolves to a broken npm .ps1 shim; ngrok.cmd works.
$Ngrok = (Get-Command ngrok.cmd -ErrorAction SilentlyContinue).Source
if (-not $Ngrok) { $Ngrok = (Get-Command ngrok.exe -ErrorAction SilentlyContinue).Source }
if (-not $Ngrok) { throw "ngrok not found on PATH (looked for ngrok.cmd / ngrok.exe)." }

$serveProc = $null
$ngrokProc = $null

try {
    # --- 2. dotnet serve ---------------------------------------------------
    Write-Host "==> Serving $WebRoot on http://localhost:$Port ..." -ForegroundColor Cyan
    $serveProc = Start-Process -FilePath "dotnet" `
        -ArgumentList @("serve", "-d", $WebRoot, "-p", $Port, "--cors") `
        -PassThru -NoNewWindow

    # Wait until the local server answers before opening the tunnel.
    $ready = $false
    foreach ($i in 1..30) {
        Start-Sleep -Milliseconds 500
        try {
            Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 2 | Out-Null
            $ready = $true; break
        } catch { }
    }
    if (-not $ready) { throw "dotnet serve did not become ready on port $Port." }
    Write-Host "    Local server is up." -ForegroundColor Green

    # --- 3. ngrok ----------------------------------------------------------
    Write-Host "==> Opening ngrok tunnel to http://localhost:$Port ..." -ForegroundColor Cyan
    $ngrokProc = Start-Process -FilePath $Ngrok `
        -ArgumentList @("http", "$Port", "--log=stdout") `
        -PassThru -NoNewWindow

    # ngrok exposes a local API; query it for the public HTTPS URL.
    $publicUrl = $null
    foreach ($i in 1..20) {
        Start-Sleep -Milliseconds 500
        try {
            $tunnels = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -TimeoutSec 2
            $publicUrl = ($tunnels.tunnels | Where-Object { $_.proto -eq "https" } |
                          Select-Object -First 1).public_url
            if ($publicUrl) { break }
        } catch { }
    }

    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Green
    if ($publicUrl) {
        Write-Host "  Public URL : $publicUrl" -ForegroundColor Green
    } else {
        Write-Host "  ngrok started, but public URL not read from API." -ForegroundColor Yellow
        Write-Host "  Check the dashboard: http://localhost:4040" -ForegroundColor Yellow
    }
    Write-Host "  Local URL  : http://localhost:$Port" -ForegroundColor Green
    Write-Host "  Dashboard  : http://localhost:4040" -ForegroundColor Green
    Write-Host "============================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Press Ctrl+C to stop." -ForegroundColor DarkGray

    # Keep the script alive while the child processes run.
    while (-not $serveProc.HasExited -and -not $ngrokProc.HasExited) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host "`n==> Shutting down..." -ForegroundColor Cyan
    foreach ($p in @($ngrokProc, $serveProc)) {
        if ($p -and -not $p.HasExited) {
            try { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue } catch { }
        }
    }
}
