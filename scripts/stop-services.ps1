#!/usr/bin/env pwsh
# Agent as a Service - Stop Services Script
# This script stops the running OrchestratorService and OrchestratorWebApp

param(
    [switch]$Help
)

if ($Help) {
    Write-Host @"
🛑 Agent as a Service - Stop Services Script

USAGE:
    pwsh stop-services.ps1

DESCRIPTION:
    This script stops all running Agent as a Service processes:
    1. Reads process IDs from .running-services.json
    2. Stops OrchestratorService and OrchestratorWebApp
    3. Cleans up any remaining processes on ports 8080 and 5264

"@ -ForegroundColor Cyan
    exit 0
}

# Function to kill processes using specific ports
function Stop-PortProcesses {
    param([int[]]$Ports)
    
    $stopped = $false
    foreach ($port in $Ports) {
        try {
            $netstat = netstat -ano | Select-String ":$port "
            foreach ($line in $netstat) {
                if ($line -match "LISTENING\s+(\d+)") {
                    $processId = $Matches[1]
                    Write-Host "🛑 Stopping process $processId using port $port" -ForegroundColor Yellow
                    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                    $stopped = $true
                }
            }
        }
        catch {
            # Port not in use or unable to stop
        }
    }
    return $stopped
}

Write-Host @"
🛑 Agent as a Service - Stop Services
====================================
"@ -ForegroundColor Red

# Check if we're in the right directory
if (-not (Test-Path "AgentAsAService.sln")) {
    Write-Host "❌ Error: Please run this script from the AgentAsAService root directory" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    exit 1
}

$processFile = "scripts/.running-services.json"
$stoppedAny = $false

# Try to stop services using stored process IDs
if (Test-Path $processFile) {
    Write-Host "📖 Reading stored process IDs..." -ForegroundColor Yellow
    
    try {
        $processIds = Get-Content $processFile | ConvertFrom-Json
        
        if ($processIds.OrchestratorService) {
            try {
                Write-Host "🛑 Stopping OrchestratorService (PID: $($processIds.OrchestratorService))" -ForegroundColor Yellow
                Stop-Process -Id $processIds.OrchestratorService -Force -ErrorAction Stop
                Write-Host "   ✅ OrchestratorService stopped" -ForegroundColor Green
                $stoppedAny = $true
            }
            catch {
                Write-Host "   ⚠️ Could not stop OrchestratorService process" -ForegroundColor Yellow
            }
        }
        
        if ($processIds.OrchestratorWebApp) {
            try {
                Write-Host "🛑 Stopping OrchestratorWebApp (PID: $($processIds.OrchestratorWebApp))" -ForegroundColor Yellow
                Stop-Process -Id $processIds.OrchestratorWebApp -Force -ErrorAction Stop
                Write-Host "   ✅ OrchestratorWebApp stopped" -ForegroundColor Green
                $stoppedAny = $true
            }
            catch {
                Write-Host "   ⚠️ Could not stop OrchestratorWebApp process" -ForegroundColor Yellow
            }
        }
        
        # Remove the process file
        Remove-Item $processFile -Force -ErrorAction SilentlyContinue
    }
    catch {
        Write-Host "⚠️ Could not read stored process IDs, falling back to port-based cleanup" -ForegroundColor Yellow
    }
}

# Fallback: Stop any processes using our ports
Write-Host "🧹 Cleaning up processes on ports 8080 and 5264..." -ForegroundColor Yellow
$portStopped = Stop-PortProcesses -Ports @(8080, 5264)

if ($portStopped) {
    $stoppedAny = $true
    Write-Host "   ✅ Stopped processes using service ports" -ForegroundColor Green
}

if ($stoppedAny) {
    Write-Host @"

✅ Services Stopped Successfully!
=================================
All Agent as a Service processes have been stopped.

You can now:
- Restart services with: pwsh start-services.ps1
- Make changes to the code
- Run individual services manually

"@ -ForegroundColor Green
} else {
    Write-Host @"

ℹ️  No Running Services Found
=============================
No Agent as a Service processes were found running.

To start services:
    pwsh start-services.ps1

"@ -ForegroundColor Cyan
}
