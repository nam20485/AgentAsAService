#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the AgentAsAService application (OrchestratorService + OrchestratorWebApp)

.DESCRIPTION
    This script automatically starts both the backend API service and the frontend web application,
    then opens the web app and Swagger documentation in your default browser.

.PARAMETER NoBrowser
    Skip opening browsers automatically

.PARAMETER Port
    Override the default API port (8080)

.PARAMETER WebPort
    Override the default web app port (5264)

.PARAMETER ProjectId
    Google Cloud Project ID for authentication (default: agent-as-a-service-459620)

.PARAMETER NoToken
    Skip generating authentication token for Swagger

.PARAMETER Windows
    Start services in separate PowerShell windows instead of background processes

.EXAMPLE
    .\start-services.ps1
    Starts both services and opens browsers

.EXAMPLE
    .\start-services.ps1 -NoBrowser
    Starts both services without opening browsers

.EXAMPLE
    .\start-services.ps1 -Windows
    Starts services in separate PowerShell windows for easy management

.EXAMPLE
    .\start-services.ps1 -Port 8081 -WebPort 5265
    Starts services on custom ports

.EXAMPLE
    .\start-services.ps1 -ProjectId "my-project-id" -NoToken
    Starts services with custom project ID but skip token generation
#>

[CmdletBinding()]
param(
    [switch]$NoBrowser,
    [int]$Port = 8080,
    [int]$WebPort = 5264,
    [string]$ProjectId = "agent-as-a-service-459620",
    [switch]$NoToken,
    [switch]$Windows
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

function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect('localhost', $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Stop-ServiceOnPort {
    param([int]$Port, [string]$ServiceName)
    
    Write-ColorOutput "🔍 Checking if port $Port is in use..." $Yellow
    
    $processes = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($processes) {
        Write-ColorOutput "⚠️  Port $Port is in use. Stopping existing $ServiceName process..." $Yellow
        $processes | ForEach-Object {
            $processId = $_.OwningProcess
            try {
                Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                Write-ColorOutput "✅ Stopped process $processId on port $Port" $Green
            }
            catch {
                Write-ColorOutput "❌ Failed to stop process $processId" $Red
            }
        }
        Start-Sleep -Seconds 2
    }
}

function Start-BackgroundService {
    param([string]$WorkingDirectory, [string]$ServiceName)
    
    Write-ColorOutput "🚀 Starting $ServiceName..." $Blue
    
    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = "dotnet"
    $processInfo.Arguments = "run"
    $processInfo.WorkingDirectory = $WorkingDirectory
    $processInfo.UseShellExecute = $false
    $processInfo.CreateNoWindow = $false
    
    
    $process = [System.Diagnostics.Process]::Start($processInfo)
    
    if ($process) {
        Write-ColorOutput "✅ $ServiceName started (PID: $($process.Id))" $Green
        return $process
    }
    else {
        Write-ColorOutput "❌ Failed to start $ServiceName" $Red
        return $null
    }
}

function Wait-ForService {
    param([string]$Url, [string]$ServiceName, [int]$TimeoutSeconds = 30)
    
    Write-ColorOutput "⏳ Waiting for $ServiceName to be ready..." $Yellow
    
    $timeout = (Get-Date).AddSeconds($TimeoutSeconds)
    
    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-ColorOutput "✅ $ServiceName is ready!" $Green
                return $true
            }
        }
        catch {
            # Service not ready yet, continue waiting
        }
        
        Start-Sleep -Seconds 2
        Write-Host "." -NoNewline
        
    } while ((Get-Date) -lt $timeout)
    
    Write-Host ""
    Write-ColorOutput "❌ $ServiceName failed to start within $TimeoutSeconds seconds" $Red
    return $false
}

# Main script execution
try {
    Write-ColorOutput "🤖 Starting AgentAsAService Application" $Blue
    Write-ColorOutput "========================================" $Blue
    
    # Ensure we're in the right directory
    $projectRoot = Split-Path $PSScriptRoot -Parent
    if (-not (Test-Path "$projectRoot/AgentAsAService.sln")) {
        Write-ColorOutput "❌ Could not find AgentAsAService.sln. Please run this script from the scripts directory." $Red
        exit 1
    }
    
    # Check prerequisites
    Write-ColorOutput "🔧 Checking prerequisites..." $Yellow
      if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-ColorOutput "❌ .NET SDK not found. Please install .NET 9.0 SDK." $Red
        exit 1
    }    $dotnetVersion = dotnet --version
    Write-ColorOutput "✅ .NET SDK version: $dotnetVersion" $Green
    
    # If Windows parameter is specified, delegate to the windowed script
    if ($Windows) {
        Write-ColorOutput "🪟 Delegating to windowed startup script..." $Blue
        $windowsScript = Join-Path $PSScriptRoot "start-services-windows.ps1"
        if (Test-Path $windowsScript) {
            if ($NoBrowser) {
                & $windowsScript -NoBrowser
            } else {
                & $windowsScript
            }
            exit 0
        } else {
            Write-ColorOutput "❌ Windowed script not found: $windowsScript" $Red
            Write-ColorOutput "💡 You can still use the regular startup mode." $Yellow
        }
    }
    
    # Set Google Cloud Project ID environment variable
    Write-ColorOutput "🔧 Setting up environment..." $Yellow
    $env:GOOGLE_CLOUD_PROJECT = $ProjectId
    $env:GCLOUD_PROJECT = $ProjectId
    Write-ColorOutput "✅ Project ID set to: $ProjectId" $Green
    
    # Stop any existing services
    Stop-ServiceOnPort -Port $Port -ServiceName "OrchestratorService"
    Stop-ServiceOnPort -Port $WebPort -ServiceName "OrchestratorWebApp"
    
    # Ensure database directory exists
    $dbPath = "$projectRoot/OrchestratorService/local-storage"
    if (-not (Test-Path $dbPath)) {
        Write-ColorOutput "📁 Creating database directory..." $Yellow
        New-Item -ItemType Directory -Path $dbPath -Force | Out-Null
        Write-ColorOutput "✅ Database directory created" $Green
    }
    
    # Start OrchestratorService
    $serviceProcess = Start-BackgroundService -WorkingDirectory "$projectRoot/OrchestratorService" -ServiceName "OrchestratorService"
    if (-not $serviceProcess) {
        Write-ColorOutput "❌ Failed to start OrchestratorService" $Red
        exit 1
    }
    
    # Wait for API to be ready
    $apiReady = Wait-ForService -Url "http://localhost:$Port/health" -ServiceName "OrchestratorService API"
    if (-not $apiReady) {
        Write-ColorOutput "❌ OrchestratorService failed to start properly" $Red
        $serviceProcess.Kill()
        exit 1
    }
    
    # Start OrchestratorWebApp
    $webProcess = Start-BackgroundService -WorkingDirectory "$projectRoot/OrchestratorWebApp" -ServiceName "OrchestratorWebApp"
    if (-not $webProcess) {
        Write-ColorOutput "❌ Failed to start OrchestratorWebApp" $Red
        $serviceProcess.Kill()
        exit 1
    }
    
    # Wait for Web App to be ready
    $webReady = Wait-ForService -Url "http://localhost:$WebPort" -ServiceName "OrchestratorWebApp"
    if (-not $webReady) {
        Write-ColorOutput "❌ OrchestratorWebApp failed to start properly" $Red
        $serviceProcess.Kill()
        $webProcess.Kill()
        exit 1
    }
    
    # Display service information
    Write-ColorOutput "========================================" $Green
    Write-ColorOutput "🎉 AgentAsAService Started Successfully!" $Green
    Write-ColorOutput "========================================" $Green
    Write-ColorOutput "📊 Service Information:" $Blue
    Write-ColorOutput "  • OrchestratorService: http://localhost:$Port" $Blue
    Write-ColorOutput "  • OrchestratorWebApp:  http://localhost:$WebPort" $Blue
    Write-ColorOutput "  • Swagger API Docs:    http://localhost:$Port/swagger" $Blue
    Write-ColorOutput "  • API Health Check:    http://localhost:$Port/health" $Blue
    Write-ColorOutput "" $Reset
    Write-ColorOutput "🔧 Process Information:" $Blue
    Write-ColorOutput "  • OrchestratorService PID: $($serviceProcess.Id)" $Blue
    Write-ColorOutput "  • OrchestratorWebApp PID:  $($webProcess.Id)" $Blue
    Write-ColorOutput "" $Reset
    
    # Open browsers if requested
    if (-not $NoBrowser) {
        Write-ColorOutput "🌐 Opening browsers..." $Yellow
        
        try {
            Start-Process "http://localhost:$WebPort"
            Write-ColorOutput "✅ Opened web application" $Green
            
            Start-Sleep -Seconds 2
            
            Start-Process "http://localhost:$Port/swagger"
            Write-ColorOutput "✅ Opened Swagger documentation" $Green
        }
        catch {
            Write-ColorOutput "⚠️  Could not open browsers automatically. Please open manually:" $Yellow
            Write-ColorOutput "   • Web App: http://localhost:$WebPort" $Yellow
            Write-ColorOutput "   • Swagger: http://localhost:$Port/swagger" $Yellow
        }    }
    
    Write-ColorOutput "" $Reset
    Write-ColorOutput "💡 Tips:" $Blue
    Write-ColorOutput "  • Use 'stop-services.ps1' to stop all services" $Blue
    Write-ColorOutput "  • Press Ctrl+C in service terminals to stop individual services" $Blue
    Write-ColorOutput "  • Check logs in the terminal windows for debugging" $Blue
    Write-ColorOutput "" $Reset
    
    # Generate authentication token for Swagger
    if (-not $NoToken) {
        Write-ColorOutput "🔐 Generating authentication token for Swagger..." $Yellow
        try {
            $tokenScriptPath = Join-Path $PSScriptRoot "get-auth-token.ps1"
            if (Test-Path $tokenScriptPath) {
                # Try gcloud method first, fallback to firebase if needed
                Write-ColorOutput "📋 Using project ID: $ProjectId" $Blue
                & pwsh -File $tokenScriptPath -Method "gcloud" -ProjectId $ProjectId
                Write-ColorOutput "✅ Authentication token generated and copied to clipboard!" $Green
                Write-ColorOutput "💡 You can now paste the token in Swagger UI for authenticated requests" $Blue
            } else {
                Write-ColorOutput "⚠️  get-auth-token.ps1 not found. Skipping token generation." $Yellow
            }
        }
        catch {
            Write-ColorOutput "⚠️  Failed to generate token: $($_.Exception.Message)" $Yellow
            Write-ColorOutput "💡 You can manually run: .\scripts\get-auth-token.ps1 -Method gcloud -ProjectId $ProjectId" $Blue
        }
        Write-ColorOutput "" $Reset
    }
    
    Write-ColorOutput "✅ Setup complete! Services are running in the background." $Green
    
}
catch {
    Write-ColorOutput "❌ An error occurred: $($_.Exception.Message)" $Red
    exit 1
}