#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts AgentAsAService in separate PowerShell windows

.DESCRIPTION
    Opens each service in its own PowerShell window for easy management

.PARAMETER NoBrowser
    Skip opening browsers automatically

.EXAMPLE
    .\start-services-windows.ps1
    Starts both services in separate PowerShell windows
#>

[CmdletBinding()]
param(
    [switch]$NoBrowser
)

# Colors for output
$Green = "`e[32m"
$Red = "`e[31m"
$Yellow = "`e[33m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = $Reset)
    Write-Host "${Color}${Message}${Reset}"
}

# Ensure we're in the right directory
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path "$projectRoot/AgentAsAService.sln")) {
    Write-ColorOutput "❌ Could not find AgentAsAService.sln. Please run this script from the scripts directory." $Red
    exit 1
}

Write-ColorOutput "🤖 Starting AgentAsAService in separate windows..." $Blue

# Stop any existing processes on ports 8080 and 5264
Write-ColorOutput "🔧 Stopping existing processes..." $Yellow
$processes = Get-NetTCPConnection -LocalPort 8080, 5264 -ErrorAction SilentlyContinue
if ($processes) {
    $processes | ForEach-Object {
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
}

# Ensure database directory exists
$dbPath = "$projectRoot/OrchestratorService/local-storage"
if (-not (Test-Path $dbPath)) {
    New-Item -ItemType Directory -Path $dbPath -Force | Out-Null
}

# Start OrchestratorService in new PowerShell window
Write-ColorOutput "🚀 Starting OrchestratorService in new window..." $Blue
$serviceScript = @"
Set-Location '$projectRoot/OrchestratorService'
Write-Host 'Starting OrchestratorService on port 8080...' -ForegroundColor Green
Write-Host 'Press Ctrl+C to stop the service' -ForegroundColor Yellow
Write-Host ''
dotnet run
Write-Host ''
Write-Host 'Service stopped. Press any key to close window...' -ForegroundColor Red
Read-Host
"@

Start-Process powershell -ArgumentList "-NoExit", "-Command", $serviceScript -WindowStyle Normal

# Wait a bit for the service to start
Write-ColorOutput "⏳ Waiting for OrchestratorService to start..." $Yellow
Start-Sleep -Seconds 5

# Start OrchestratorWebApp in new PowerShell window
Write-ColorOutput "🌐 Starting OrchestratorWebApp in new window..." $Blue
$webAppScript = @"
Set-Location '$projectRoot/OrchestratorWebApp'
Write-Host 'Starting OrchestratorWebApp on port 5264...' -ForegroundColor Green
Write-Host 'Press Ctrl+C to stop the service' -ForegroundColor Yellow
Write-Host ''
dotnet run
Write-Host ''
Write-Host 'Service stopped. Press any key to close window...' -ForegroundColor Red
Read-Host
"@

Start-Process powershell -ArgumentList "-NoExit", "-Command", $webAppScript -WindowStyle Normal

# Wait for services to be ready
Write-ColorOutput "⏳ Waiting for services to be ready..." $Yellow
Start-Sleep -Seconds 10

Write-ColorOutput "" $Reset
Write-ColorOutput "🎉 Services started in separate windows!" $Green
Write-ColorOutput "=======================================" $Green
Write-ColorOutput "📊 OrchestratorService API: http://localhost:8080" $Blue
Write-ColorOutput "📝 Swagger Documentation: http://localhost:8080/swagger" $Blue
Write-ColorOutput "🌐 OrchestratorWebApp: http://localhost:5264" $Blue
Write-ColorOutput "❤️  Health Check: http://localhost:8080/health" $Blue
Write-ColorOutput "" $Reset
Write-ColorOutput "💡 Each service is running in its own PowerShell window." $Yellow
Write-ColorOutput "💡 To stop a service, press Ctrl+C in its window." $Yellow
Write-ColorOutput "" $Reset

# Open browsers if requested
if (-not $NoBrowser) {
    Write-ColorOutput "🌐 Opening browsers..." $Blue
    Start-Sleep -Seconds 2
    try {
        Start-Process "http://localhost:5264"
        Start-Sleep -Seconds 1
        Start-Process "http://localhost:8080/swagger"
        Write-ColorOutput "✅ Browsers opened successfully" $Green
    }
    catch {
        Write-ColorOutput "⚠️  Could not open browsers automatically" $Yellow
    }
}

Write-ColorOutput "" $Reset
Write-ColorOutput "✨ Ready for development!" $Green
