# Production Deployment Validation Script (PowerShell)
# This script tests all components of the AgentAsAService production deployment

param(
    [string]$ProjectId = "agent-as-a-service-459620",
    [string]$Region = "us-west1"
)

$ErrorActionPreference = "Stop"

# Configuration
$webAppUrl = "https://$ProjectId.web.app"
$orchestratorUrl = "https://orchestratorservice-$Region-$ProjectId.run.app"
$agentUrl = "https://agentservice-$Region-$ProjectId.run.app"

Write-Host "🧪 Starting production deployment validation..." -ForegroundColor Green
Write-Host "Project ID: $ProjectId" -ForegroundColor Cyan
Write-Host "Region: $Region" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if services are deployed
Write-Host "📋 Test 1: Checking Cloud Run services..." -ForegroundColor Yellow

# Check OrchestratorService
Write-Host "  Checking OrchestratorService..." -ForegroundColor White
try {
    $null = gcloud run services describe orchestratorservice --region $Region --project $ProjectId --format="value(metadata.name)" 2>$null
    Write-Host "  ✅ OrchestratorService is deployed" -ForegroundColor Green
    $orchestratorStatus = gcloud run services describe orchestratorservice --region $Region --project $ProjectId --format="value(status.conditions[0].status)"
    Write-Host "     Status: $orchestratorStatus" -ForegroundColor Gray
} catch {
    Write-Host "  ❌ OrchestratorService not found" -ForegroundColor Red
    exit 1
}

# Check AgentService
Write-Host "  Checking AgentService..." -ForegroundColor White
try {
    $null = gcloud run services describe agentservice --region $Region --project $ProjectId --format="value(metadata.name)" 2>$null
    Write-Host "  ✅ AgentService is deployed" -ForegroundColor Green
    $agentStatus = gcloud run services describe agentservice --region $Region --project $ProjectId --format="value(status.conditions[0].status)"
    Write-Host "     Status: $agentStatus" -ForegroundColor Gray
} catch {
    Write-Host "  ❌ AgentService not found" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Check service health endpoints
Write-Host "📋 Test 2: Testing service health endpoints..." -ForegroundColor Yellow

# Test OrchestratorService health
Write-Host "  Testing OrchestratorService health..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$orchestratorUrl/health" -Method GET -TimeoutSec 10 -UseBasicParsing
    Write-Host "  ✅ OrchestratorService health endpoint responding" -ForegroundColor Green
} catch {
    Write-Host "  ⚠️  OrchestratorService health endpoint not responding (may not be implemented)" -ForegroundColor Orange
}

# Test OrchestratorService API
Write-Host "  Testing OrchestratorService API..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$orchestratorUrl/api/orchestrator/test" -Method GET -TimeoutSec 10 -UseBasicParsing
    Write-Host "  ✅ OrchestratorService API endpoint responding (HTTP $($response.StatusCode))" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__ 
    if ($statusCode -eq 401) {
        Write-Host "  ✅ OrchestratorService API endpoint responding (HTTP $statusCode)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ OrchestratorService API endpoint not responding (HTTP $statusCode)" -ForegroundColor Red
    }
}

# Test AgentService (should be protected)
Write-Host "  Testing AgentService protection..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$agentUrl/api/agent/test" -Method GET -TimeoutSec 10 -UseBasicParsing
    Write-Host "  ⚠️  AgentService is not protected - this may be intentional for testing" -ForegroundColor Orange
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401 -or $statusCode -eq 403) {
        Write-Host "  ✅ AgentService is properly protected (HTTP $statusCode)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ AgentService not responding (HTTP $statusCode)" -ForegroundColor Red
    }
}

Write-Host ""

# Test 3: Check Firebase Hosting
Write-Host "📋 Test 3: Testing Firebase Hosting..." -ForegroundColor Yellow

Write-Host "  Testing Blazor WebApp..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri $webAppUrl -Method GET -TimeoutSec 15 -UseBasicParsing
    Write-Host "  ✅ Blazor WebApp is accessible (HTTP $($response.StatusCode))" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "  ❌ Blazor WebApp not accessible (HTTP $statusCode)" -ForegroundColor Red
}

# Test SPA routing
Write-Host "  Testing SPA routing..." -ForegroundColor White
try {
    $response = Invoke-WebRequest -Uri "$webAppUrl/authentication" -Method GET -TimeoutSec 15 -UseBasicParsing
    Write-Host "  ✅ SPA routing working (HTTP $($response.StatusCode))" -ForegroundColor Green
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "  ❌ SPA routing not working (HTTP $statusCode)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Check service accounts and permissions
Write-Host "📋 Test 4: Checking service accounts..." -ForegroundColor Yellow

# Check if service accounts exist
try {
    $null = gcloud iam service-accounts describe "orchestrator@$ProjectId.iam.gserviceaccount.com" --project $ProjectId --format="value(email)" 2>$null
    Write-Host "  ✅ Orchestrator service account exists" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Orchestrator service account not found" -ForegroundColor Red
}

try {
    $null = gcloud iam service-accounts describe "agent-service@$ProjectId.iam.gserviceaccount.com" --project $ProjectId --format="value(email)" 2>$null
    Write-Host "  ✅ Agent service account exists" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Agent service account not found" -ForegroundColor Red
}

Write-Host ""

# Test 5: Check Firestore
Write-Host "📋 Test 5: Testing Firestore..." -ForegroundColor Yellow

try {
    $null = gcloud firestore databases describe --project $ProjectId --format="value(name)" 2>$null
    Write-Host "  ✅ Firestore database exists" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Firestore database not found" -ForegroundColor Red
}

Write-Host ""

# Test 6: Check secrets
Write-Host "📋 Test 6: Checking secrets..." -ForegroundColor Yellow

try {
    $null = gcloud secrets describe github-app-private-key --project $ProjectId --format="value(name)" 2>$null
    Write-Host "  ✅ GitHub App private key secret exists" -ForegroundColor Green
    $versions = (gcloud secrets versions list github-app-private-key --project $ProjectId --format="value(name)").Count
    Write-Host "     Versions: $versions" -ForegroundColor Gray
} catch {
    Write-Host "  ❌ GitHub App private key secret not found" -ForegroundColor Red
}

Write-Host ""

# Summary
Write-Host "🎯 Validation Summary:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor White
Write-Host "  🌐 Blazor WebApp: $webAppUrl" -ForegroundColor Gray
Write-Host "  🔧 OrchestratorService: $orchestratorUrl" -ForegroundColor Gray
Write-Host "  🤖 AgentService: $agentUrl" -ForegroundColor Gray
Write-Host ""
Write-Host "📋 Next steps for complete testing:" -ForegroundColor White
Write-Host "1. Test Google OAuth login at $webAppUrl" -ForegroundColor Gray
Write-Host "2. Test authenticated API calls from the Blazor app" -ForegroundColor Gray
Write-Host "3. Monitor logs in Google Cloud Console" -ForegroundColor Gray
Write-Host "4. Verify Firestore data persistence" -ForegroundColor Gray
Write-Host ""
Write-Host "✅ Production deployment validation completed!" -ForegroundColor Green
