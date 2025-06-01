#!/usr/bin/env pwsh
param(
    [switch]$SimulateClean,
    [switch]$SkipBackup,
    [switch]$RestoreOnly,
    [string]$LogFile = "test-environment.log"
)

# PowerShell Core script to test the install-environment.ps1 script in a clean environment simulation
# This script helps verify that the installation works from scratch

# Enable strict error handling
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$ScriptDir = Split-Path $MyInvocation.MyCommand.Path -Parent
$InstallScript = Join-Path $ScriptDir "install-environment.ps1"
$ValidateScript = Join-Path $ScriptDir "validate-environment.ps1"

# Backup and restore functions
$BackupFile = Join-Path $ScriptDir "path-backup.json"

function Backup-Environment {
    Write-Host "Backing up current environment..." -ForegroundColor Yellow
    
    $Backup = @{
        Path = $env:Path
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Platform = if ($IsWindows) { "Windows" } else { "Linux" }
    }
    
    $Backup | ConvertTo-Json | Set-Content $BackupFile
    Write-Host "Environment backed up to: $BackupFile" -ForegroundColor Green
}

function Restore-Environment {
    if (Test-Path $BackupFile) {
        Write-Host "Restoring environment from backup..." -ForegroundColor Yellow
        $Backup = Get-Content $BackupFile | ConvertFrom-Json
        $env:Path = $Backup.Path
        Write-Host "Environment restored from backup created at: $($Backup.Timestamp)" -ForegroundColor Green
    } else {
        Write-Warning "No backup file found at: $BackupFile"
    }
}

function Test-CleanEnvironment {
    Write-Host "Simulating clean environment by removing tool paths..." -ForegroundColor Yellow
    
    # Tools to remove from PATH
    $ToolPaths = @(
        "*nodejs*",
        "*nvm*",
        "*Google*Cloud*",
        "*gcloud*",
        "*GitHub*CLI*",
        "*gh*",
        "*firebase*",
        "*chocolatey*"
    )
    
    $CurrentPath = $env:Path
    $PathItems = $CurrentPath -split [System.IO.Path]::PathSeparator
    
    # Filter out tool paths
    $CleanedItems = $PathItems | Where-Object {
        $item = $_
        $shouldKeep = $true
        foreach ($pattern in $ToolPaths) {
            if ($item -like $pattern) {
                $shouldKeep = $false
                Write-Host "  Removing from PATH: $item" -ForegroundColor Red
                break
            }
        }
        $shouldKeep
    }
    
    $env:Path = $CleanedItems -join [System.IO.Path]::PathSeparator
    Write-Host "PATH cleaned for testing" -ForegroundColor Green
}

function Write-TestLog {
    param([string]$Message)
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogEntry = "[$Timestamp] $Message"
    Write-Host $LogEntry
    Add-Content -Path $LogFile -Value $LogEntry
}

function Test-Prerequisites {
    Write-Host "`n=== Testing Prerequisites ===" -ForegroundColor Magenta
    
    # Check if PowerShell Core
    if ($PSVersionTable.PSVersion.Major -lt 6) {
        Write-Error "This test requires PowerShell Core (pwsh). Current version: $($PSVersionTable.PSVersion)"
        exit 1
    }
    
    # Check if scripts exist
    if (-not (Test-Path $InstallScript)) {
        Write-Error "Install script not found: $InstallScript"
        exit 1
    }
    
    if (-not (Test-Path $ValidateScript)) {
        Write-Error "Validation script not found: $ValidateScript"
        exit 1
    }
    
    # Check if running as administrator on Windows (for some installations)
    if ($IsWindows) {
        $IsAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
        if (-not $IsAdmin) {
            Write-Warning "Not running as Administrator. Some installations may fail."
            Write-Host "Consider running PowerShell as Administrator for full testing." -ForegroundColor Yellow
        } else {
            Write-Host "✅ Running as Administrator" -ForegroundColor Green
        }
    }
    
    Write-Host "✅ Prerequisites check completed" -ForegroundColor Green
}

# Handle restore-only mode
if ($RestoreOnly) {
    Restore-Environment
    exit 0
}

# Start logging
if (Test-Path $LogFile) { Remove-Item $LogFile }
Write-TestLog "Starting environment installation test"
Write-TestLog "Platform: $($PSVersionTable.OS)"
Write-TestLog "PowerShell: $($PSVersionTable.PSVersion)"

try {
    Test-Prerequisites
    
    # Backup current environment unless skipped
    if (-not $SkipBackup) {
        Backup-Environment
    }
    
    # Simulate clean environment if requested
    if ($SimulateClean) {
        Test-CleanEnvironment
    }
    
    Write-Host "`n=== Pre-Installation Validation ===" -ForegroundColor Magenta
    Write-TestLog "Running pre-installation validation"
    
    # Run pre-installation validation
    try {
        & $ValidateScript
        $PreInstallResult = $LASTEXITCODE
        Write-TestLog "Pre-installation validation exit code: $PreInstallResult"
    } catch {
        Write-TestLog "Pre-installation validation failed: $_"
        $PreInstallResult = 1
    }
    
    Write-Host "`n=== Running Installation Script ===" -ForegroundColor Magenta
    Write-TestLog "Starting installation script"
    
    # Run installation script
    $InstallStartTime = Get-Date
    try {
        & $InstallScript
        $InstallResult = $LASTEXITCODE
        $InstallEndTime = Get-Date
        $InstallDuration = $InstallEndTime - $InstallStartTime
        
        Write-TestLog "Installation completed in $($InstallDuration.TotalSeconds) seconds"
        Write-TestLog "Installation exit code: $InstallResult"
        
        if ($InstallResult -eq 0) {
            Write-Host "✅ Installation script completed successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Installation script failed with exit code: $InstallResult" -ForegroundColor Red
        }
    } catch {
        Write-TestLog "Installation script failed with exception: $_"
        Write-Host "❌ Installation script failed with exception: $_" -ForegroundColor Red
        $InstallResult = 1
    }
    
    Write-Host "`n=== Post-Installation Validation ===" -ForegroundColor Magenta
    Write-TestLog "Running post-installation validation"
    
    # Run post-installation validation
    try {
        & $ValidateScript
        $PostInstallResult = $LASTEXITCODE
        Write-TestLog "Post-installation validation exit code: $PostInstallResult"
        
        if ($PostInstallResult -eq 0) {
            Write-Host "✅ Post-installation validation passed" -ForegroundColor Green
        } else {
            Write-Host "❌ Post-installation validation failed" -ForegroundColor Red
        }
    } catch {
        Write-TestLog "Post-installation validation failed: $_"
        Write-Host "❌ Post-installation validation failed: $_" -ForegroundColor Red
        $PostInstallResult = 1
    }
    
    # Summary
    Write-Host "`n=== Test Summary ===" -ForegroundColor Magenta
    Write-TestLog "Test summary:"
    Write-TestLog "  Pre-install validation: $(if ($PreInstallResult -eq 0) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  Installation: $(if ($InstallResult -eq 0) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  Post-install validation: $(if ($PostInstallResult -eq 0) { 'PASS' } else { 'FAIL' })"
    
    $OverallSuccess = ($InstallResult -eq 0) -and ($PostInstallResult -eq 0)
    
    if ($OverallSuccess) {
        Write-Host "🎉 All tests passed! Environment setup is working correctly." -ForegroundColor Green
        Write-TestLog "OVERALL RESULT: SUCCESS"
    } else {
        Write-Host "⚠️ Some tests failed. Check the log for details." -ForegroundColor Red
        Write-TestLog "OVERALL RESULT: FAILURE"
    }
    
    Write-Host "`nTest log saved to: $LogFile" -ForegroundColor Cyan
    
    # Ask about restoring environment
    if (-not $SkipBackup -and (Test-Path $BackupFile)) {
        $Restore = Read-Host "`nRestore original environment? (y/N)"
        if ($Restore -eq 'y' -or $Restore -eq 'Y') {
            Restore-Environment
        } else {
            Write-Host "Environment not restored. Run with -RestoreOnly to restore later." -ForegroundColor Yellow
        }
    }
    
    exit $(if ($OverallSuccess) { 0 } else { 1 })
    
} catch {
    Write-TestLog "Test failed with exception: $_"
    Write-Host "❌ Test failed with exception: $_" -ForegroundColor Red
    
    # Try to restore environment on failure
    if (-not $SkipBackup) {
        Write-Host "Attempting to restore environment due to failure..." -ForegroundColor Yellow
        Restore-Environment
    }
    
    exit 1
}
