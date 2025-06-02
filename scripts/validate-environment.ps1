#!/usr/bin/env pwsh
# PowerShell Core script to validate development environment tools
# This script checks if Node.js, npm, Firebase CLI, Google Cloud CLI, and GitHub CLI are properly installed

# Enable strict error handling but continue on validation failures to report all issues
$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest

# Track validation results
$ValidationResults = @()
$OverallSuccess = $true

function Test-ToolInstallation {
    param(
        [string]$ToolName,
        [string]$Command,
        [string]$VersionCommand,
        [string]$ExpectedVersionPattern = "",
        [string]$Description = ""
    )
    
    $Result = @{
        Tool = $ToolName
        Command = $Command
        Installed = $false
        InPath = $false
        Version = ""
        ExpectedVersion = $ExpectedVersionPattern
        Description = $Description
        Error = ""
    }
    
    try {
        # Check if command exists in PATH
        $CommandInfo = Get-Command $Command -ErrorAction SilentlyContinue
        if ($CommandInfo) {
            $Result.InPath = $true
            $Result.Installed = $true
            
            # Get version information
            if ($VersionCommand) {
                try {
                    $VersionOutput = Invoke-Expression $VersionCommand 2>&1
                    $Result.Version = ($VersionOutput | Out-String).Trim()
                    
                    # Check if version matches expected pattern
                    if ($ExpectedVersionPattern -and $Result.Version -notmatch $ExpectedVersionPattern) {
                        $Result.Error = "Version mismatch. Expected: $ExpectedVersionPattern"
                        $Result.Installed = $false
                    }
                } catch {
                    $Result.Error = "Failed to get version: $_"
                    $Result.Installed = $false
                }
            }
        } else {
            $Result.Error = "Command '$Command' not found in PATH"
        }
    } catch {
        $Result.Error = "Unexpected error: $_"
    }
    
    return $Result
}

function Write-ValidationResult {
    param($Result)
    
    $Status = if ($Result.Installed) { "✅ PASS" } else { "❌ FAIL" }
    $Color = if ($Result.Installed) { "Green" } else { "Red" }
    
    Write-Host "`n$($Result.Tool) ($($Result.Command))" -ForegroundColor Cyan
    Write-Host "  Status: $Status" -ForegroundColor $Color
    Write-Host "  In PATH: $($Result.InPath)" -ForegroundColor $(if ($Result.InPath) { "Green" } else { "Red" })
    
    if ($Result.Version) {
        Write-Host "  Version: $($Result.Version)" -ForegroundColor Yellow
    }
    
    if ($Result.ExpectedVersion) {
        Write-Host "  Expected: $($Result.ExpectedVersion)" -ForegroundColor Gray
    }
    
    if ($Result.Description) {
        Write-Host "  Description: $($Result.Description)" -ForegroundColor Gray
    }
    
    if ($Result.Error) {
        Write-Host "  Error: $($Result.Error)" -ForegroundColor Red
    }
}

Write-Host "=== Development Environment Validation ===" -ForegroundColor Magenta
Write-Host "Running on: $($PSVersionTable.OS)" -ForegroundColor Cyan
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Cyan
Write-Host ""

# Validate Node.js
Write-Host "Validating Node.js..." -ForegroundColor Yellow
$NodeResult = Test-ToolInstallation -ToolName "Node.js" -Command "node" -VersionCommand "node --version" -Description "JavaScript runtime"
Write-ValidationResult $NodeResult
$ValidationResults += $NodeResult

if (-not $NodeResult.Installed) { $OverallSuccess = $false }

# Validate npm
Write-Host "`nValidating npm..." -ForegroundColor Yellow
$NpmResult = Test-ToolInstallation -ToolName "npm" -Command "npm" -VersionCommand "npm --version" -Description "Node Package Manager"
Write-ValidationResult $NpmResult
$ValidationResults += $NpmResult

if (-not $NpmResult.Installed) { $OverallSuccess = $false }

# Validate NVM
Write-Host "`nValidating NVM..." -ForegroundColor Yellow
if ($IsWindows) {
    $NvmResult = Test-ToolInstallation -ToolName "NVM (Windows)" -Command "nvm" -VersionCommand "nvm version" -Description "Node Version Manager for Windows"
} else {
    # For Linux, check if nvm.sh exists
    $NvmPath = "$env:HOME/.nvm/nvm.sh"
    $NvmResult = @{
        Tool = "NVM (Linux)"
        Command = "nvm"
        Installed = (Test-Path $NvmPath)
        InPath = (Test-Path $NvmPath)
        Version = if (Test-Path $NvmPath) { "Installed at $NvmPath" } else { "" }
        ExpectedVersion = ""
        Description = "Node Version Manager for Linux"
        Error = if (-not (Test-Path $NvmPath)) { "nvm.sh not found at $NvmPath" } else { "" }
    }
}
Write-ValidationResult $NvmResult
$ValidationResults += $NvmResult

if (-not $NvmResult.Installed) { $OverallSuccess = $false }

# Validate Firebase CLI
Write-Host "`nValidating Firebase CLI..." -ForegroundColor Yellow
$FirebaseResult = Test-ToolInstallation -ToolName "Firebase CLI" -Command "firebase" -VersionCommand "firebase --version" -ExpectedVersionPattern "14\.3\.1" -Description "Firebase command line tools"
Write-ValidationResult $FirebaseResult
$ValidationResults += $FirebaseResult

if (-not $FirebaseResult.Installed) { $OverallSuccess = $false }

# Validate Google Cloud CLI
Write-Host "`nValidating Google Cloud CLI..." -ForegroundColor Yellow
$GcloudResult = Test-ToolInstallation -ToolName "Google Cloud CLI" -Command "gcloud" -VersionCommand "gcloud --version" -Description "Google Cloud command line tools"
Write-ValidationResult $GcloudResult
$ValidationResults += $GcloudResult

if (-not $GcloudResult.Installed) { $OverallSuccess = $false }

# Validate GitHub CLI
Write-Host "`nValidating GitHub CLI..." -ForegroundColor Yellow
$GhResult = Test-ToolInstallation -ToolName "GitHub CLI" -Command "gh" -VersionCommand "gh --version" -Description "GitHub command line interface"
Write-ValidationResult $GhResult
$ValidationResults += $GhResult

if (-not $GhResult.Installed) { $OverallSuccess = $false }

# Additional System Validation
Write-Host "`n=== System Validation ===" -ForegroundColor Magenta

# Check PowerShell version compatibility
Write-Host "`nPowerShell Core Compatibility:" -ForegroundColor Yellow
$PSVersion = $PSVersionTable.PSVersion
if ($PSVersion.Major -ge 7) {
    Write-Host "  ✅ PowerShell Core $PSVersion is compatible" -ForegroundColor Green
} elseif ($PSVersion.Major -eq 6) {
    Write-Host "  ⚠️ PowerShell Core $PSVersion may work but v7+ recommended" -ForegroundColor Yellow
} else {
    Write-Host "  ❌ PowerShell $PSVersion is not PowerShell Core. Please use pwsh" -ForegroundColor Red
    $OverallSuccess = $false
}

# Check if running on supported platform
Write-Host "`nPlatform Support:" -ForegroundColor Yellow
if ($IsWindows) {
    Write-Host "  ✅ Windows platform detected and supported" -ForegroundColor Green
} elseif ($IsLinux) {
    Write-Host "  ✅ Linux platform detected and supported" -ForegroundColor Green
} elseif ($IsMacOS) {
    Write-Host "  ⚠️ macOS detected but not fully tested" -ForegroundColor Yellow
} else {
    Write-Host "  ❌ Unknown platform - may not be supported" -ForegroundColor Red
}

# Check PATH environment variable
Write-Host "`nPATH Environment:" -ForegroundColor Yellow
$PathValues = $env:Path -split [System.IO.Path]::PathSeparator
$ImportantPaths = @()

if ($IsWindows) {
    $ImportantPaths = @(
        "C:\Program Files\nodejs",
        "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin",
        "C:\Program Files\GitHub CLI",
        "C:\ProgramData\chocolatey\bin"
    )
} else {
    $ImportantPaths = @(
        "/usr/local/bin",
        "/usr/bin",
        "$env:HOME/.nvm"
    )
}

foreach ($ImportantPath in $ImportantPaths) {
    $InPath = $PathValues -contains $ImportantPath
    $Exists = Test-Path $ImportantPath -ErrorAction SilentlyContinue
    $Status = if ($InPath -and $Exists) { "✅" } elseif ($Exists) { "⚠️" } else { "❌" }
    Write-Host "  $Status $ImportantPath (In PATH: $InPath, Exists: $Exists)" -ForegroundColor $(if ($InPath -and $Exists) { "Green" } elseif ($Exists) { "Yellow" } else { "Red" })
}

# Summary
Write-Host "`n=== Validation Summary ===" -ForegroundColor Magenta
$PassedTools = @($ValidationResults | Where-Object { $_.Installed }).Count
$TotalTools = $ValidationResults.Count

Write-Host "Tools validated: $PassedTools/$TotalTools" -ForegroundColor $(if ($PassedTools -eq $TotalTools) { "Green" } else { "Red" })

if ($OverallSuccess) {
    Write-Host "🎉 All development tools are properly installed and accessible!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠️ Some development tools are missing or misconfigured." -ForegroundColor Red
    Write-Host "Please run the install-environment.ps1 script to fix issues." -ForegroundColor Yellow
    exit 1
}
