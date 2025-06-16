#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up environment configuration for AgentAsAService

.DESCRIPTION
    Configures the appropriate environment settings and validates the setup

.PARAMETER Environment
    Target environment to configure (Development, Testing, Staging, Production)

.PARAMETER Validate
    If specified, validates the current environment configuration

.EXAMPLE
    .\setup-environment.ps1 -Environment Development
    
.EXAMPLE
    .\setup-environment.ps1 -Environment Production -Validate
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Development", "Testing", "Staging", "Production")]
    [string]$Environment,
    
    [switch]$Validate
)

$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Set-EnvironmentVariable {
    param([string]$Name, [string]$Value)
    [Environment]::SetEnvironmentVariable($Name, $Value, "Process")
    Write-ColoredOutput "Set $Name = $Value" $InfoColor
}

Write-ColoredOutput "=== AgentAsAService Environment Setup ===" $InfoColor
Write-ColoredOutput "Target Environment: $Environment" $InfoColor
Write-ColoredOutput ""

# Set ASPNETCORE_ENVIRONMENT
Set-EnvironmentVariable -Name "ASPNETCORE_ENVIRONMENT" -Value $Environment

# Environment-specific setup
switch ($Environment) {
    "Development" {
        Write-ColoredOutput "=== Development Environment Setup ===" $SuccessColor
        Write-ColoredOutput "• Authentication: Bypassed" $InfoColor
        Write-ColoredOutput "• Storage: LiteDB (local-storage/)" $InfoColor
        Write-ColoredOutput "• OrchestratorService: http://localhost:8080" $InfoColor
        Write-ColoredOutput "• AgentService: http://localhost:7001" $InfoColor
        Write-ColoredOutput "• CORS: Permissive (localhost)" $InfoColor
    }
    
    "Testing" {
        Write-ColoredOutput "=== Testing Environment Setup ===" $SuccessColor
        Write-ColoredOutput "• Authentication: Bypassed" $InfoColor
        Write-ColoredOutput "• Storage: LiteDB (test-data/)" $InfoColor
        Write-ColoredOutput "• Ports: Dynamic (via PORT env var)" $InfoColor
        Write-ColoredOutput "• CORS: Test origins" $InfoColor
    }
    
    "Staging" {
        Write-ColoredOutput "=== Staging Environment Setup ===" $SuccessColor
        Write-ColoredOutput "• Authentication: Google OAuth (staging)" $WarningColor
        Write-ColoredOutput "• Storage: Firestore (staging project)" $InfoColor
        Write-ColoredOutput "• Deployment: Cloud Run" $InfoColor
        Write-ColoredOutput "• Domain: staging.example.com" $InfoColor
        
        Write-ColoredOutput "" 
        Write-ColoredOutput "Required Environment Variables:" $WarningColor
        Write-ColoredOutput "• GOOGLE_CLIENT_ID (staging)" $WarningColor
        Write-ColoredOutput "• Google Cloud Service Account configured" $WarningColor
    }
    
    "Production" {
        Write-ColoredOutput "=== Production Environment Setup ===" $SuccessColor
        Write-ColoredOutput "• Authentication: Google OAuth (production)" $WarningColor
        Write-ColoredOutput "• Storage: Firestore (agent-as-a-service-459620)" $InfoColor
        Write-ColoredOutput "• Deployment: Cloud Run" $InfoColor
        Write-ColoredOutput "• Domain: agent-as-a-service-459620.web.app" $InfoColor
        
        Write-ColoredOutput ""
        Write-ColoredOutput "Required Environment Variables:" $WarningColor
        Write-ColoredOutput "• GOOGLE_CLIENT_ID (production)" $WarningColor
        Write-ColoredOutput "• CLOUD_RUN_REGION" $WarningColor
        Write-ColoredOutput "• Google Cloud Service Account configured" $WarningColor
    }
}

Write-ColoredOutput ""

# Validation
if ($Validate) {
    Write-ColoredOutput "=== Environment Validation ===" $InfoColor
    
    $currentEnv = [Environment]::GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    if ($currentEnv -eq $Environment) {
        Write-ColoredOutput "✓ ASPNETCORE_ENVIRONMENT correctly set to: $currentEnv" $SuccessColor
    } else {
        Write-ColoredOutput "✗ ASPNETCORE_ENVIRONMENT mismatch. Expected: $Environment, Got: $currentEnv" $ErrorColor
    }
    
    # Check configuration files exist
    $configFiles = @(
        "OrchestratorService/appsettings.$Environment.json",
        "AgentService/appsettings.$Environment.json"
    )
    
    foreach ($file in $configFiles) {
        if (Test-Path $file) {
            Write-ColoredOutput "✓ Configuration file exists: $file" $SuccessColor
        } else {
            Write-ColoredOutput "✗ Configuration file missing: $file" $ErrorColor
        }
    }
}

Write-ColoredOutput ""
Write-ColoredOutput "Environment setup completed!" $SuccessColor
Write-ColoredOutput "You can now run services with the configured environment." $InfoColor
