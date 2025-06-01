#!/usr/bin/env pwsh
# PowerShell Core script to install development environment tools
# This script works on both Windows and Linux and installs Node.js, npm, Firebase CLI, Google Cloud CLI, and GitHub CLI

# Enable strict error handling - fail fast on any error
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "Installing development environment tools..." -ForegroundColor Green
Write-Host "Running on: $($PSVersionTable.OS)" -ForegroundColor Cyan

#
# NVM & Node.js/npm (Cross-platform)
#
if ($IsWindows) {
    # Windows: Use nvm-windows via Chocolatey
    $NvmCmd = Get-Command nvm -ErrorAction SilentlyContinue
      if (-not $NvmCmd) {
        Write-Host "Installing NVM for Windows..." -ForegroundColor Yellow
        if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
            Write-Error "Chocolatey is required but not installed. Please install Chocolatey first."
            exit 1
        }
        
        try {
            $ChocoResult = choco install nvm -y 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Chocolatey NVM installation failed with exit code: $LASTEXITCODE"
                Write-Host $ChocoResult -ForegroundColor Red
                exit 1
            }
        } catch {
            Write-Error "Failed to install NVM via Chocolatey: $_"
            exit 1
        }
        
        # Refresh environment variables
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        
        # Verify NVM installation
        $NvmCmd = Get-Command nvm -ErrorAction SilentlyContinue
        if (-not $NvmCmd) {
            Write-Error "NVM installation failed - nvm command not found in PATH"
            exit 1
        }
    } else {
        Write-Host "NVM for Windows already installed." -ForegroundColor Green
    }    
    # Install and use latest LTS Node.js
    Write-Host "Installing latest Node.js LTS..." -ForegroundColor Yellow
    try {
        nvm install lts
        if ($LASTEXITCODE -ne 0) {
            Write-Error "NVM install lts failed with exit code: $LASTEXITCODE"
            exit 1
        }
        
        nvm use lts
        if ($LASTEXITCODE -ne 0) {
            Write-Error "NVM use lts failed with exit code: $LASTEXITCODE"
            exit 1
        }
    } catch {
        Write-Error "Failed to install/use Node.js LTS: $_"
        exit 1
    }
} elseif ($IsLinux) {
    # Linux: Use nvm via curl
    $NvmDir = "$env:HOME/.nvm"
    
    if (-not (Test-Path "$NvmDir/nvm.sh")) {
        Write-Host "Installing NVM for Linux..." -ForegroundColor Yellow
        # Install curl if not present
        if (-not (Get-Command curl -ErrorAction SilentlyContinue)) {
            if (Get-Command apt -ErrorAction SilentlyContinue) {
                sudo apt install curl -y
            } elseif (Get-Command yum -ErrorAction SilentlyContinue) {
                sudo yum install curl -y
            }
        }
        
        # Install nvm
        curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.7/install.sh | bash
        
        # Source nvm
        $env:NVM_DIR = $NvmDir
        if (Test-Path "$NvmDir/nvm.sh") {
            bash -c ". $NvmDir/nvm.sh && nvm install --lts && nvm use --lts"
        }
    } else {
        Write-Host "NVM for Linux already installed." -ForegroundColor Green
        # Source nvm and ensure latest LTS is installed
        bash -c ". $NvmDir/nvm.sh && nvm install --lts && nvm use --lts"
    }
} else {
    Write-Error "Unsupported operating system"
    exit 1
}

# Verify Node.js and npm installation
if (Get-Command node -ErrorAction SilentlyContinue) {
    node -v
    npm -v
} else {
    Write-Error "Node.js installation failed or not in PATH"
    exit 1
}

#
# Firebase CLI (v14.3.1)
#
$RequiredFirebaseVersion = "14.3.1"
$InstalledFirebaseVersion = ""

try {
    $InstalledFirebaseVersion = (firebase --version 2>$null) -replace "firebase-tools@", ""
} catch {
    $InstalledFirebaseVersion = ""
}

if ($InstalledFirebaseVersion -ne $RequiredFirebaseVersion) {
    Write-Host "Installing Firebase CLI version $RequiredFirebaseVersion..." -ForegroundColor Yellow
    try {
        npm install -g firebase-tools@$RequiredFirebaseVersion
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Firebase CLI installation failed with exit code: $LASTEXITCODE"
            exit 1
        }
    } catch {
        Write-Error "Failed to install Firebase CLI: $_"
        exit 1
    }
} else {
    Write-Host "Firebase CLI version $RequiredFirebaseVersion already installed." -ForegroundColor Green
}

# Verify Firebase CLI installation
try {
    $FirebaseVersionCheck = firebase --version 2>&1
    Write-Host "Firebase CLI installed: $FirebaseVersionCheck" -ForegroundColor Green
} catch {
    Write-Error "Failed to verify Firebase CLI installation: $_"
    exit 1
}

#
# Google Cloud CLI (Cross-platform)
#
$GcloudCmd = Get-Command gcloud -ErrorAction SilentlyContinue

if (-not $GcloudCmd) {
    Write-Host "Installing Google Cloud CLI..." -ForegroundColor Yellow
    if ($IsWindows) {
        # Use official Google Cloud SDK installer for Windows
        $TempDir = "$env:TEMP\gcloud-installer"
        $InstallerPath = "$TempDir\google-cloud-cli-installer.exe"
        
        # Create temp directory
        if (-not (Test-Path $TempDir)) {
            New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
        }
        
        # Download installer
        Write-Host "Downloading Google Cloud CLI installer..." -ForegroundColor Yellow
        $InstallerUrl = "https://dl.google.com/dl/cloudsdk/channels/rapid/GoogleCloudSDKInstaller.exe"
        try {
            Invoke-WebRequest -Uri $InstallerUrl -OutFile $InstallerPath -UseBasicParsing
        } catch {
            Write-Error "Failed to download Google Cloud CLI installer: $_"
            exit 1
        }
        
        # Run installer silently
        Write-Host "Running Google Cloud CLI installer..." -ForegroundColor Yellow
        try {
            $Process = Start-Process -FilePath $InstallerPath -ArgumentList "/S" -Wait -PassThru
            if ($Process.ExitCode -ne 0) {
                Write-Error "Google Cloud CLI installer failed with exit code: $($Process.ExitCode)"
                exit 1
            }
        } catch {
            Write-Error "Failed to run Google Cloud CLI installer: $_"
            exit 1
        }
        
        # Clean up
        Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue
        
        # Refresh environment variables
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        
        # Verify installation
        $GcloudCmd = Get-Command gcloud -ErrorAction SilentlyContinue
        if (-not $GcloudCmd) {
            Write-Error "Google Cloud CLI installation failed - gcloud command not found in PATH"
            exit 1
        }
    } elseif ($IsLinux) {
        # Linux installation using official repository
        try {
            sudo apt-get update -y
            sudo apt-get install -y apt-transport-https ca-certificates gnupg curl
            
            # Add Google Cloud SDK repository
            bash -c 'echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] https://packages.cloud.google.com/apt cloud-sdk main" | sudo tee /etc/apt/sources.list.d/google-cloud-sdk.list'
            curl https://packages.cloud.google.com/apt/doc/apt-key.gpg | sudo apt-key --keyring /usr/share/keyrings/cloud.google.gpg add -
            
            # Install Google Cloud CLI
            sudo apt-get update -y
            sudo apt-get install -y google-cloud-cli
            
            # Verify installation
            if (-not (Get-Command gcloud -ErrorAction SilentlyContinue)) {
                Write-Error "Google Cloud CLI installation failed - gcloud command not found"
                exit 1
            }
        } catch {
            Write-Error "Failed to install Google Cloud CLI on Linux: $_"
            exit 1
        }
    }
} else {
    Write-Host "Google Cloud CLI already installed." -ForegroundColor Green
}

# Verify gcloud installation
try {
    $GcloudVersion = gcloud version 2>&1
    Write-Host "Google Cloud CLI installed successfully:" -ForegroundColor Green
    Write-Host ($GcloudVersion | Select-String "Google Cloud SDK") -ForegroundColor Cyan
} catch {
    Write-Error "Failed to verify Google Cloud CLI installation: $_"
    exit 1
}

#
# GitHub CLI (Cross-platform)
#
$GhCmd = Get-Command gh -ErrorAction SilentlyContinue

if (-not $GhCmd) {
    Write-Host "Installing GitHub CLI..." -ForegroundColor Yellow
    if ($IsWindows) {
        if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
            Write-Error "Chocolatey is required but not installed. Please install Chocolatey first."
            exit 1
        }
        
        # Check if gh is already installed via Chocolatey but not in PATH
        $ChocoGh = choco list gh --local-only --exact 2>$null
        if ($ChocoGh -match "gh \d+") {
            Write-Host "GitHub CLI package found but not in PATH. Reinstalling..." -ForegroundColor Yellow
            try {
                $ChocoResult = choco uninstall gh -y 2>&1
                Start-Sleep -Seconds 2
                $ChocoResult = choco install gh -y --force 2>&1
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Chocolatey GitHub CLI reinstallation failed with exit code: $LASTEXITCODE"
                    Write-Host $ChocoResult -ForegroundColor Red
                    exit 1
                }
            } catch {
                Write-Error "Failed to reinstall GitHub CLI via Chocolatey: $_"
                exit 1
            }
        } else {
            try {
                $ChocoResult = choco install gh -y 2>&1
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Chocolatey GitHub CLI installation failed with exit code: $LASTEXITCODE"
                    Write-Host $ChocoResult -ForegroundColor Red
                    exit 1
                }
            } catch {
                Write-Error "Failed to install GitHub CLI via Chocolatey: $_"
                exit 1
            }
        }        
        # Refresh environment variables and check for GitHub CLI in common locations
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        
        # Check common GitHub CLI installation paths
        $PossibleGhPaths = @(
            "C:\Program Files\GitHub CLI\gh.exe",
            "C:\Program Files (x86)\GitHub CLI\gh.exe",
            "C:\ProgramData\chocolatey\bin\gh.exe"
        )
        
        $GhPath = $null
        foreach ($Path in $PossibleGhPaths) {
            if (Test-Path $Path) {
                $GhPath = $Path
                break
            }
        }
        
        if ($GhPath) {
            # Add the directory to PATH if not already there
            $GhDir = Split-Path $GhPath -Parent
            $CurrentPath = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
            if ($CurrentPath -notlike "*$GhDir*") {
                Write-Host "Adding GitHub CLI to system PATH: $GhDir" -ForegroundColor Yellow
                $NewPath = $CurrentPath + ";" + $GhDir
                [System.Environment]::SetEnvironmentVariable("Path", $NewPath, "Machine")
                # Update current session PATH
                $env:Path = $env:Path + ";" + $GhDir
            }
        }
        
        # Verify GitHub CLI installation
        $GhCmd = Get-Command gh -ErrorAction SilentlyContinue
        if (-not $GhCmd) {
            Write-Error "GitHub CLI installation failed - gh command not found in PATH after installation"
            exit 1
        }
    } elseif ($IsLinux) {
        # Linux installation
        try {
            curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
            sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
            bash -c 'echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null'
            sudo apt update
            sudo apt install gh -y
            
            # Verify installation
            if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
                Write-Error "GitHub CLI installation failed - gh command not found"
                exit 1
            }
        } catch {
            Write-Error "Failed to install GitHub CLI on Linux: $_"
            exit 1
        }
    }
} else {
    Write-Host "GitHub CLI already installed." -ForegroundColor Green
}

# Verify GitHub CLI installation
try {
    $GhVersion = gh version 2>&1
    Write-Host "GitHub CLI installed successfully:" -ForegroundColor Green
    Write-Host ($GhVersion | Select-String "gh version") -ForegroundColor Cyan
} catch {
    Write-Error "Failed to verify GitHub CLI installation: $_"
    exit 1
}

Write-Host "Environment setup completed successfully!" -ForegroundColor Green
