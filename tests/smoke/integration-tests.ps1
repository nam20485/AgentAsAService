# PowerShell version of integration tests for Windows environments

param(
    [string]$OrchestratorUrl = "https://orchestratorservice-us-west1-agent-as-a-service-459620.run.app",
    [string]$AgentUrl = "https://agentservice-us-west1-agent-as-a-service-459620.run.app",
    [string]$WebAppUrl = "https://agent-as-a-service-459620.web.app"
)

# Colors for output
$Global:Red = [System.ConsoleColor]::Red
$Global:Green = [System.ConsoleColor]::Green
$Global:Yellow = [System.ConsoleColor]::Yellow
$Global:White = [System.ConsoleColor]::White

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Global:Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Global:Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Global:Red
}

# Test 1: Service Discovery and Availability
function Test-ServiceDiscovery {
    Write-Info "🔍 Testing service discovery and availability..."
    
    # Test OrchestratorService
    Write-Info "Testing OrchestratorService availability..."
    try {
        $response = Invoke-RestMethod -Uri "$OrchestratorUrl/health" -Method Get -TimeoutSec 10
        Write-Info "✅ OrchestratorService is available"
    }
    catch {
        Write-Error "❌ OrchestratorService is not available: $($_.Exception.Message)"
        return $false
    }
    
    # Test AgentService
    Write-Info "Testing AgentService availability..."
    try {
        $response = Invoke-RestMethod -Uri "$AgentUrl/health" -Method Get -TimeoutSec 10
        Write-Info "✅ AgentService is available"
    }
    catch {
        Write-Error "❌ AgentService is not available: $($_.Exception.Message)"
        return $false
    }
    
    # Test WebApp
    Write-Info "Testing WebApp availability..."
    try {
        $response = Invoke-WebRequest -Uri $WebAppUrl -Method Get -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Info "✅ WebApp is available"
        }
        else {
            Write-Error "❌ WebApp is not available (HTTP $($response.StatusCode))"
            return $false
        }
    }
    catch {
        Write-Error "❌ WebApp is not available: $($_.Exception.Message)"
        return $false
    }
    
    Write-Info "✅ All services are discoverable and available"
    return $true
}

# Test 2: API Contract Validation
function Test-ApiContracts {
    Write-Info "📋 Testing API contracts and responses..."
    
    # Test OrchestratorService WeatherForecast API
    Write-Info "Testing OrchestratorService WeatherForecast API..."
    try {
        $response = Invoke-RestMethod -Uri "$OrchestratorUrl/weatherforecast" -Method Get -TimeoutSec 10
        
        if ($response -is [Array] -and $response.Count -gt 0) {
            Write-Info "✅ WeatherForecast API returns valid array"
            
            # Validate first item structure
            $firstItem = $response[0]
            if ($firstItem.PSObject.Properties.Name -contains "date" -and
                $firstItem.PSObject.Properties.Name -contains "temperatureC" -and
                $firstItem.PSObject.Properties.Name -contains "summary") {
                Write-Info "✅ WeatherForecast items have correct structure"
            }
            else {
                Write-Warning "⚠️  WeatherForecast items may have unexpected structure"
            }
        }
        else {
            Write-Error "❌ WeatherForecast API response is invalid"
            return $false
        }
    }
    catch {
        Write-Error "❌ WeatherForecast API test failed: $($_.Exception.Message)"
        return $false
    }
    
    Write-Info "✅ API contracts validation completed"
    return $true
}

# Test 3: Authentication and Authorization
function Test-Authentication {
    Write-Info "🔐 Testing authentication and authorization..."
    
    # Test AgentService requires authentication
    Write-Info "Testing AgentService authentication requirements..."
    try {
        $response = Invoke-WebRequest -Uri "$AgentUrl/weatherforecast" -Method Get -TimeoutSec 10 -ErrorAction Stop
        Write-Warning "⚠️  AgentService authentication behavior unexpected (HTTP $($response.StatusCode))"
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 401) {
            Write-Info "✅ AgentService correctly requires authentication"
        }
        else {
            Write-Warning "⚠️  AgentService authentication behavior unexpected: $($_.Exception.Message)"
        }
    }
    
    # Test OrchestratorService public endpoints
    Write-Info "Testing OrchestratorService public access..."
    try {
        $response = Invoke-WebRequest -Uri "$OrchestratorUrl/weatherforecast" -Method Get -TimeoutSec 10
        if ($response.StatusCode -eq 200) {
            Write-Info "✅ OrchestratorService public endpoints accessible"
        }
        else {
            Write-Error "❌ OrchestratorService public endpoints not accessible (HTTP $($response.StatusCode))"
            return $false
        }
    }
    catch {
        Write-Error "❌ OrchestratorService public endpoints test failed: $($_.Exception.Message)"
        return $false
    }
    
    Write-Info "✅ Authentication and authorization tests completed"
    return $true
}

# Test 4: Performance and Reliability
function Test-Performance {
    Write-Info "⚡ Testing performance and reliability..."
    
    # Response time test
    Write-Info "Testing response times..."
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-RestMethod -Uri "$OrchestratorUrl/health" -Method Get -TimeoutSec 5
        $stopwatch.Stop()
        
        $responseTime = $stopwatch.ElapsedMilliseconds
        
        if ($responseTime -lt 2000) {
            Write-Info "✅ OrchestratorService response time: ${responseTime}ms (good)"
        }
        elseif ($responseTime -lt 5000) {
            Write-Warning "⚠️  OrchestratorService response time: ${responseTime}ms (acceptable)"
        }
        else {
            Write-Error "❌ OrchestratorService response time: ${responseTime}ms (too slow)"
            return $false
        }
    }
    catch {
        Write-Error "❌ Response time test failed: $($_.Exception.Message)"
        return $false
    }
    
    # Concurrent request test
    Write-Info "Testing concurrent request handling..."
    
    $jobs = @()
    for ($i = 1; $i -le 5; $i++) {
        $jobs += Start-Job -ScriptBlock {
            param($url)
            try {
                Invoke-RestMethod -Uri "$url/health" -Method Get -TimeoutSec 10
                return $true
            }
            catch {
                return $false
            }
        } -ArgumentList $OrchestratorUrl
    }
    
    # Wait for all jobs and check results
    $results = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job
    
    $successCount = ($results | Where-Object { $_ -eq $true }).Count
    
    if ($successCount -eq 5) {
        Write-Info "✅ Concurrent request handling successful (5/5)"
    }
    else {
        Write-Warning "⚠️  Concurrent request handling partial success ($successCount/5)"
    }
    
    Write-Info "✅ Performance and reliability tests completed"
    return $true
}

# Test 5: Content and Resources
function Test-ContentResources {
    Write-Info "📄 Testing content and resource loading..."
    
    # Test WebApp content
    Write-Info "Testing WebApp content loading..."
    try {
        $response = Invoke-WebRequest -Uri $WebAppUrl -Method Get -TimeoutSec 10
        $content = $response.Content
        
        # Check for Blazor indicators
        if ($content -match "blazor") {
            Write-Info "✅ Blazor WebAssembly indicators found"
        }
        else {
            Write-Warning "⚠️  Blazor WebAssembly indicators not found"
        }
        
        # Check for basic HTML structure
        if ($content -match "<html") {
            Write-Info "✅ Valid HTML structure found"
        }
        else {
            Write-Error "❌ Invalid HTML structure"
            return $false
        }
        
        # Check content size
        $contentSize = $content.Length
        if ($contentSize -gt 1000) {
            Write-Info "✅ WebApp content size: $contentSize bytes (reasonable)"
        }
        else {
            Write-Warning "⚠️  WebApp content size: $contentSize bytes (may be incomplete)"
        }
    }
    catch {
        Write-Error "❌ Content and resource test failed: $($_.Exception.Message)"
        return $false
    }
    
    Write-Info "✅ Content and resource tests completed"
    return $true
}

# Main test execution
function Main {
    Write-Info "🚀 Starting AgentAsAService deployment integration tests..."
    Write-Info "Target URLs:"
    Write-Info "  OrchestratorService: $OrchestratorUrl"
    Write-Info "  AgentService: $AgentUrl"
    Write-Info "  WebApp: $WebAppUrl"
    Write-Host ""
    
    $allTestsPassed = $true
    
    # Run all tests
    if (-not (Test-ServiceDiscovery)) { $allTestsPassed = $false }
    Write-Host ""
    
    if (-not (Test-ApiContracts)) { $allTestsPassed = $false }
    Write-Host ""
    
    if (-not (Test-Authentication)) { $allTestsPassed = $false }
    Write-Host ""
    
    if (-not (Test-Performance)) { $allTestsPassed = $false }
    Write-Host ""
    
    if (-not (Test-ContentResources)) { $allTestsPassed = $false }
    Write-Host ""
    
    if ($allTestsPassed) {
        Write-Info "🎉 All integration tests passed successfully!"
        Write-Info "Deployment validation complete - services are healthy and functional."
        exit 0
    }
    else {
        Write-Error "❌ Some integration tests failed!"
        Write-Error "Deployment validation incomplete - please check the failed tests."
        exit 1
    }
}

# Execute main function
Main
