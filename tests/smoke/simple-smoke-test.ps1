# Simple smoke test to validate production deployment
param(
    [string]$AgentServiceUrl = "https://agentservice-us-west1-agent-as-a-service-459620.run.app",
    [string]$OrchestratorServiceUrl = "https://orchestratorservice-us-west1-agent-as-a-service-459620.run.app",
    [string]$WebAppUrl = "https://agent-as-a-service-459620.web.app"
)

Write-Host "🚀 Running Production Smoke Tests" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Agent Service: $AgentServiceUrl" -ForegroundColor Gray
Write-Host "Orchestrator Service: $OrchestratorServiceUrl" -ForegroundColor Gray
Write-Host "Web App: $WebAppUrl" -ForegroundColor Gray
Write-Host ""

$testsPassed = 0
$testsFailed = 0

function Test-Endpoint {
    param(
        [string]$Url,
        [string]$TestName,
        [int]$TimeoutSec = 30
    )
    
    Write-Host "🔍 Testing: $TestName..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec $TimeoutSec -UseBasicParsing
        
        if ($response.StatusCode -eq 200) {
            Write-Host "   ✅ PASSED - Status: $($response.StatusCode)" -ForegroundColor Green
            $global:testsPassed++
            return $true
        }
        else {
            Write-Host "   ❌ FAILED - Status: $($response.StatusCode)" -ForegroundColor Red
            $global:testsFailed++
            return $false
        }
    }
    catch {
        Write-Host "   ❌ FAILED - Error: $($_.Exception.Message)" -ForegroundColor Red
        $global:testsFailed++
        return $false
    }
}

function Test-HealthEndpoint {
    param(
        [string]$BaseUrl,
        [string]$ServiceName
    )
    
    Write-Host "🏥 Testing health endpoints for $ServiceName..." -ForegroundColor Cyan
    
    # Test /health endpoint
    Test-Endpoint -Url "$BaseUrl/health" -TestName "$ServiceName /health endpoint"
    
    # Test /health/ready endpoint
    Test-Endpoint -Url "$BaseUrl/health/ready" -TestName "$ServiceName /health/ready endpoint"
    
    # Test /health/live endpoint
    Test-Endpoint -Url "$BaseUrl/health/live" -TestName "$ServiceName /health/live endpoint"
}

# Run the tests
Write-Host "Starting health endpoint tests..." -ForegroundColor Cyan

# Test Agent Service
Test-HealthEndpoint -BaseUrl $AgentServiceUrl -ServiceName "AgentService"

# Test Orchestrator Service
Test-HealthEndpoint -BaseUrl $OrchestratorServiceUrl -ServiceName "OrchestratorService"

# Test Web App
Write-Host "🌐 Testing Web App accessibility..." -ForegroundColor Cyan
Test-Endpoint -Url $WebAppUrl -TestName "Blazor WebApp accessibility"

# Test a few API endpoints if accessible
Write-Host "🔌 Testing API endpoints..." -ForegroundColor Cyan

# These might return 401/403 if authentication is required, which is expected
try {
    Write-Host "🔍 Testing: Agent Service WeatherForecast API..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "$AgentServiceUrl/weatherforecast" -TimeoutSec 30 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ PASSED - API accessible (Status: $($response.StatusCode))" -ForegroundColor Green
        $global:testsPassed++
    }
    elseif ($response.StatusCode -eq 401 -or $response.StatusCode -eq 403) {
        Write-Host "   ✅ PASSED - API secured as expected (Status: $($response.StatusCode))" -ForegroundColor Green
        $global:testsPassed++
    }
    else {
        Write-Host "   ⚠️  UNEXPECTED - Status: $($response.StatusCode)" -ForegroundColor Yellow
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401 -or $statusCode -eq 403) {
        Write-Host "   ✅ PASSED - API secured as expected (Status: $statusCode)" -ForegroundColor Green
        $global:testsPassed++
    }
    else {
        Write-Host "   ⚠️  INFO - WeatherForecast API: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

try {
    Write-Host "🔍 Testing: Orchestrator Service WeatherForecast API..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "$OrchestratorServiceUrl/weatherforecast" -TimeoutSec 30 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ PASSED - API accessible (Status: $($response.StatusCode))" -ForegroundColor Green
        $global:testsPassed++
    }
    elseif ($response.StatusCode -eq 401 -or $response.StatusCode -eq 403) {
        Write-Host "   ✅ PASSED - API secured as expected (Status: $($response.StatusCode))" -ForegroundColor Green
        $global:testsPassed++
    }
    else {
        Write-Host "   ⚠️  UNEXPECTED - Status: $($response.StatusCode)" -ForegroundColor Yellow
    }
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401 -or $statusCode -eq 403) {
        Write-Host "   ✅ PASSED - API secured as expected (Status: $statusCode)" -ForegroundColor Green
        $global:testsPassed++
    }
    else {
        Write-Host "   ⚠️  INFO - WeatherForecast API: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Generate summary
Write-Host ""
Write-Host "📋 Smoke Test Summary" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "Tests Passed: $testsPassed" -ForegroundColor Green
Write-Host "Tests Failed: $testsFailed" -ForegroundColor Red
Write-Host "Total Tests: $($testsPassed + $testsFailed)" -ForegroundColor Gray

$successRate = if (($testsPassed + $testsFailed) -gt 0) {
    [math]::Round(($testsPassed / ($testsPassed + $testsFailed)) * 100, 2)
} else { 0 }

Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })

if ($testsFailed -eq 0) {
    Write-Host ""
    Write-Host "🎉 All smoke tests passed! 🎉" -ForegroundColor Green
    Write-Host "✅ Production deployment is healthy and ready for traffic." -ForegroundColor Green
    exit 0
}
else {
    Write-Host ""    Write-Host "⚠️ Some smoke tests failed" -ForegroundColor Yellow
    Write-Host "❌ Please review the results above." -ForegroundColor Red
    exit 1
}
