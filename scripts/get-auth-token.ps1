#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Gets an authentication token for testing the services locally

.DESCRIPTION
    This script helps you get a valid JWT token for testing your services in Swagger.
    It supports multiple authentication methods:
    1. Google Cloud Service Account token (requires gcloud CLI)
    2. Firebase ID token (requires Firebase CLI)
    3. Custom service account token

.PARAMETER Method
    The authentication method to use: 'gcloud', 'firebase', or 'service-account'

.PARAMETER ProjectId
    Your Google Cloud/Firebase project ID

.PARAMETER ServiceAccountEmail
    Email of the service account (for service-account method)

.EXAMPLE
    .\get-auth-token.ps1 -Method gcloud -ProjectId "your-project-id"

.EXAMPLE
    .\get-auth-token.ps1 -Method firebase -ProjectId "your-project-id"
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("gcloud", "firebase", "service-account")]
    [string]$Method,
    
    [Parameter(Mandatory=$true)]
    [string]$ProjectId,
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceAccountEmail
)

function Get-GCloudToken {
    param([string]$ProjectId)
    
    Write-Host "Getting Google Cloud access token..." -ForegroundColor Yellow
    
    try {
        # Set the project
        gcloud config set project $ProjectId
        
        # Get access token
        $token = gcloud auth print-access-token
        
        if ($LASTEXITCODE -eq 0 -and $token) {
            Write-Host "✓ Successfully obtained gcloud access token" -ForegroundColor Green
            Write-Host "Token (copy this to Swagger Authorization header):" -ForegroundColor Cyan
            Write-Host "Bearer $token" -ForegroundColor White
            return $token
        }
        else {
            throw "Failed to get access token"
        }
    }
    catch {
        Write-Host "✗ Error getting gcloud token: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Make sure you're logged in with: gcloud auth login" -ForegroundColor Yellow
        return $null
    }
}

function Get-FirebaseToken {
    param([string]$ProjectId)
    
    Write-Host "Getting Firebase ID token..." -ForegroundColor Yellow
    
    try {
        # Login to Firebase (if not already logged in)
        $loginResult = firebase login --no-localhost 2>$null
        
        # Get ID token
        $tokenResult = firebase auth:print-token --project $ProjectId
        
        if ($LASTEXITCODE -eq 0 -and $tokenResult) {
            Write-Host "✓ Successfully obtained Firebase ID token" -ForegroundColor Green
            Write-Host "Token (copy this to Swagger Authorization header):" -ForegroundColor Cyan
            Write-Host "Bearer $tokenResult" -ForegroundColor White
            return $tokenResult
        }
        else {
            throw "Failed to get Firebase token"
        }
    }
    catch {
        Write-Host "✗ Error getting Firebase token: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Make sure you're logged in with: firebase login" -ForegroundColor Yellow
        return $null
    }
}

function Get-ServiceAccountToken {
    param([string]$ProjectId, [string]$ServiceAccountEmail)
    
    if (-not $ServiceAccountEmail) {
        $ServiceAccountEmail = Read-Host "Enter service account email"
    }
    
    Write-Host "Getting service account token for $ServiceAccountEmail..." -ForegroundColor Yellow
    
    try {
        # Impersonate service account and get token
        $token = gcloud auth print-access-token --impersonate-service-account=$ServiceAccountEmail
        
        if ($LASTEXITCODE -eq 0 -and $token) {
            Write-Host "✓ Successfully obtained service account token" -ForegroundColor Green
            Write-Host "Token (copy this to Swagger Authorization header):" -ForegroundColor Cyan
            Write-Host "Bearer $token" -ForegroundColor White
            return $token
        }
        else {
            throw "Failed to get service account token"
        }
    }
    catch {
        Write-Host "✗ Error getting service account token: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Make sure you have permission to impersonate the service account" -ForegroundColor Yellow
        return $null
    }
}

# Main execution
Write-Host "=== Authentication Token Generator ===" -ForegroundColor Magenta
Write-Host "Project ID: $ProjectId" -ForegroundColor Gray
Write-Host "Method: $Method" -ForegroundColor Gray
Write-Host ""

switch ($Method) {
    "gcloud" {
        $token = Get-GCloudToken -ProjectId $ProjectId
    }
    "firebase" {
        $token = Get-FirebaseToken -ProjectId $ProjectId
    }
    "service-account" {
        $token = Get-ServiceAccountToken -ProjectId $ProjectId -ServiceAccountEmail $ServiceAccountEmail
    }
}

if ($token) {
    Write-Host ""
    Write-Host "=== Instructions for Swagger UI ===" -ForegroundColor Magenta
    Write-Host "1. Open your service's Swagger UI (e.g., https://localhost:7001/swagger)" -ForegroundColor White
    Write-Host "2. Click the Authorize button (🔒)" -ForegroundColor White
    Write-Host "3. In the Value field, paste: Bearer $token" -ForegroundColor White
    Write-Host "4. Click Authorize" -ForegroundColor White
    Write-Host "5. You can now test authenticated endpoints!" -ForegroundColor White
    
    # Copy to clipboard if possible
    try {
        "Bearer $token" | Set-Clipboard
        Write-Host ""
        Write-Host "✓ Token copied to clipboard!" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "Note: Could not copy to clipboard automatically" -ForegroundColor Yellow
    }
}
else {
    Write-Host ""
    Write-Host "Failed to obtain authentication token. Check the error messages above." -ForegroundColor Red
    exit 1
}
