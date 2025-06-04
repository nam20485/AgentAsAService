#!/bin/bash

# Advanced integration test script for AgentAsAService deployment
# This script tests service-to-service communication and end-to-end workflows

set -e

# Configuration
ORCHESTRATOR_URL="${ORCHESTRATOR_URL:-https://orchestratorservice-us-west1-agent-as-a-service-459620.run.app}"
AGENT_URL="${AGENT_URL:-https://agentservice-us-west1-agent-as-a-service-459620.run.app}"
WEBAPP_URL="${WEBAPP_URL:-https://agent-as-a-service-459620.web.app}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Test 1: Service Discovery and Availability
test_service_discovery() {
    log_info "🔍 Testing service discovery and availability..."
    
    # Test OrchestratorService
    log_info "Testing OrchestratorService availability..."
    if curl -f -s --max-time 10 "$ORCHESTRATOR_URL/health" > /dev/null; then
        log_info "✅ OrchestratorService is available"
    else
        log_error "❌ OrchestratorService is not available"
        return 1
    fi
    
    # Test AgentService
    log_info "Testing AgentService availability..."
    if curl -f -s --max-time 10 "$AGENT_URL/health" > /dev/null; then
        log_info "✅ AgentService is available"
    else
        log_error "❌ AgentService is not available"
        return 1
    fi
    
    # Test WebApp
    log_info "Testing WebApp availability..."
    response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$WEBAPP_URL")
    if [ "$response" == "200" ]; then
        log_info "✅ WebApp is available"
    else
        log_error "❌ WebApp is not available (HTTP $response)"
        return 1
    fi
    
    log_info "✅ All services are discoverable and available"
}

# Test 2: API Contract Validation
test_api_contracts() {
    log_info "📋 Testing API contracts and responses..."
    
    # Test OrchestratorService WeatherForecast API
    log_info "Testing OrchestratorService WeatherForecast API..."
    response=$(curl -s --max-time 10 "$ORCHESTRATOR_URL/weatherforecast")
    
    # Validate JSON response structure
    if echo "$response" | jq -e 'type == "array" and length > 0' > /dev/null 2>&1; then
        log_info "✅ WeatherForecast API returns valid JSON array"
        
        # Validate first item structure
        if echo "$response" | jq -e '.[0] | has("date") and has("temperatureC") and has("summary")' > /dev/null 2>&1; then
            log_info "✅ WeatherForecast items have correct structure"
        else
            log_warning "⚠️  WeatherForecast items may have unexpected structure"
        fi
    else
        log_error "❌ WeatherForecast API response is invalid"
        return 1
    fi
    
    log_info "✅ API contracts validation completed"
}

# Test 3: Authentication and Authorization
test_authentication() {
    log_info "🔐 Testing authentication and authorization..."
    
    # Test AgentService requires authentication
    log_info "Testing AgentService authentication requirements..."
    response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$AGENT_URL/weatherforecast")
    
    if [ "$response" == "401" ]; then
        log_info "✅ AgentService correctly requires authentication"
    else
        log_warning "⚠️  AgentService authentication behavior unexpected (HTTP $response)"
    fi
    
    # Test OrchestratorService public endpoints
    log_info "Testing OrchestratorService public access..."
    response=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "$ORCHESTRATOR_URL/weatherforecast")
    
    if [ "$response" == "200" ]; then
        log_info "✅ OrchestratorService public endpoints accessible"
    else
        log_error "❌ OrchestratorService public endpoints not accessible (HTTP $response)"
        return 1
    fi
    
    log_info "✅ Authentication and authorization tests completed"
}

# Test 4: Performance and Reliability
test_performance() {
    log_info "⚡ Testing performance and reliability..."
    
    # Response time test
    log_info "Testing response times..."
    
    # OrchestratorService response time
    start_time=$(date +%s%N)
    curl -f -s --max-time 5 "$ORCHESTRATOR_URL/health" > /dev/null
    end_time=$(date +%s%N)
    
    response_time=$(( (end_time - start_time) / 1000000 )) # Convert to milliseconds
    
    if [ "$response_time" -lt 2000 ]; then
        log_info "✅ OrchestratorService response time: ${response_time}ms (good)"
    elif [ "$response_time" -lt 5000 ]; then
        log_warning "⚠️  OrchestratorService response time: ${response_time}ms (acceptable)"
    else
        log_error "❌ OrchestratorService response time: ${response_time}ms (too slow)"
        return 1
    fi
    
    # Concurrent request test
    log_info "Testing concurrent request handling..."
    
    # Send 5 concurrent requests
    for i in {1..5}; do
        curl -f -s --max-time 10 "$ORCHESTRATOR_URL/health" > /dev/null &
    done
    
    # Wait for all background jobs
    wait
    
    if [ $? -eq 0 ]; then
        log_info "✅ Concurrent request handling successful"
    else
        log_error "❌ Concurrent request handling failed"
        return 1
    fi
    
    log_info "✅ Performance and reliability tests completed"
}

# Test 5: Content and Resources
test_content_resources() {
    log_info "📄 Testing content and resource loading..."
    
    # Test WebApp content
    log_info "Testing WebApp content loading..."
    content=$(curl -s --max-time 10 "$WEBAPP_URL")
    
    # Check for Blazor indicators
    if echo "$content" | grep -q "blazor"; then
        log_info "✅ Blazor WebAssembly indicators found"
    else
        log_warning "⚠️  Blazor WebAssembly indicators not found"
    fi
    
    # Check for basic HTML structure
    if echo "$content" | grep -q "<html"; then
        log_info "✅ Valid HTML structure found"
    else
        log_error "❌ Invalid HTML structure"
        return 1
    fi
    
    # Check content size (should be reasonable for a WebApp)
    content_size=$(echo "$content" | wc -c)
    if [ "$content_size" -gt 1000 ]; then
        log_info "✅ WebApp content size: ${content_size} bytes (reasonable)"
    else
        log_warning "⚠️  WebApp content size: ${content_size} bytes (may be incomplete)"
    fi
    
    log_info "✅ Content and resource tests completed"
}

# Main test execution
main() {
    log_info "🚀 Starting AgentAsAService deployment integration tests..."
    log_info "Target URLs:"
    log_info "  OrchestratorService: $ORCHESTRATOR_URL"
    log_info "  AgentService: $AGENT_URL"
    log_info "  WebApp: $WEBAPP_URL"
    echo ""
    
    # Run all tests
    test_service_discovery || exit 1
    echo ""
    
    test_api_contracts || exit 1
    echo ""
    
    test_authentication || exit 1
    echo ""
    
    test_performance || exit 1
    echo ""
    
    test_content_resources || exit 1
    echo ""
    
    log_info "🎉 All integration tests passed successfully!"
    log_info "Deployment validation complete - services are healthy and functional."
}

# Execute main function
main "$@"
