# Firebase Deployment Script for OrchestratorWebApp (PowerShell)
# This script builds and deploys the Blazor WebAssembly app to Firebase Hosting

param(
    [string]$ProjectId = "agent-as-a-service-459620",
    [string]$BuildConfiguration = "Release",
    [string]$WebAppProject = "OrchestratorWebApp"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting Firebase deployment for $WebAppProject..." -ForegroundColor Green

# Ensure we're in the project root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
Set-Location $projectRoot

# Set Firebase project
Write-Host "📋 Setting Firebase project to $ProjectId..." -ForegroundColor Yellow
firebase use $ProjectId

# Build the Blazor WebAssembly app
Write-Host "🔨 Building $WebAppProject in $BuildConfiguration mode..." -ForegroundColor Yellow
dotnet publish "$WebAppProject/$WebAppProject.csproj" `
    --configuration $BuildConfiguration `
    --output "$WebAppProject/publish" `
    --verbosity minimal

# Verify build output
$buildOutput = "$WebAppProject/publish/wwwroot"
if (-not (Test-Path $buildOutput)) {
    Write-Host "❌ Error: Build output directory not found at $buildOutput" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build completed successfully" -ForegroundColor Green

# Copy production configuration
Write-Host "📄 Copying production configuration..." -ForegroundColor Yellow
$prodConfig = "$WebAppProject/wwwroot/appsettings.Production.json"
$targetConfig = "$WebAppProject/publish/wwwroot/appsettings.json"

if (Test-Path $prodConfig) {
    Copy-Item $prodConfig $targetConfig -Force
    Write-Host "✅ Production configuration applied" -ForegroundColor Green
} else {
    Write-Host "⚠️  Warning: No production configuration found, using default" -ForegroundColor Orange
}

# Deploy to Firebase Hosting
Write-Host "🌐 Deploying to Firebase Hosting..." -ForegroundColor Yellow
firebase deploy --only hosting

Write-Host ""
Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "🔗 Your app is available at:" -ForegroundColor Cyan
Write-Host "   https://$ProjectId.web.app" -ForegroundColor White
Write-Host "   https://$ProjectId.firebaseapp.com" -ForegroundColor White
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "1. Test the authentication flow" -ForegroundColor White
Write-Host "2. Verify API connectivity to OrchestratorService" -ForegroundColor White
Write-Host "3. Check browser developer tools for any errors" -ForegroundColor White
