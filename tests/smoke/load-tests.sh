#!/bin/bash

# Load testing script for AgentAsAService deployment
# Uses Apache Bench (ab) and curl for performance testing

set -e

# Configuration
ORCHESTRATOR_URL="${ORCHESTRATOR_URL:-https://orchestratorservice-us-west1-agent-as-a-service-459620.run.app}"
AGENT_URL="${AGENT_URL:-https://agentservice-us-west1-agent-as-a-service-459620.run.app}"
WEBAPP_URL="${WEBAPP_URL:-https://agent-as-a-service-459620.web.app}"

# Test configurations
LIGHT_REQUESTS=50
LIGHT_CONCURRENCY=5
MEDIUM_REQUESTS=200
MEDIUM_CONCURRENCY=10
HEAVY_REQUESTS=500
HEAVY_CONCURRENCY=25

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

log_test() {
    echo -e "${BLUE}[TEST]${NC} $1"
}

# Check if Apache Bench is available
check_dependencies() {
    if ! command -v ab &> /dev/null; then
        log_error "Apache Bench (ab) is not installed. Please install it:"
        log_error "  Ubuntu/Debian: sudo apt-get install apache2-utils"
        log_error "  CentOS/RHEL: sudo yum install httpd-tools"
        log_error "  macOS: brew install apache2-utils"
        exit 1
    fi
    
    if ! command -v jq &> /dev/null; then
        log_warning "jq is not installed. JSON parsing will be limited."
    fi
}

# Extract metrics from Apache Bench output
extract_ab_metrics() {
    local ab_output="$1"
    
    echo "=== Apache Bench Results ==="
    echo "$ab_output" | grep -E "(Requests per second|Time per request|Transfer rate|Failed requests|Total:|Connect:|Processing:|Waiting:)"
    
    # Extract key metrics for validation
    local failed_requests=$(echo "$ab_output" | grep "Failed requests" | awk '{print $3}')
    local requests_per_sec=$(echo "$ab_output" | grep "Requests per second" | awk '{print $4}')
    local time_per_request=$(echo "$ab_output" | grep "Time per request" | head -1 | awk '{print $4}')
    
    echo ""
    echo "Key Metrics:"
    echo "  Failed Requests: $failed_requests"
    echo "  Requests/sec: $requests_per_sec"
    echo "  Time/request: ${time_per_request}ms"
    
    # Return failed requests count for validation
    echo "$failed_requests"
}

# Light load test (warm-up)
run_light_load_test() {
    log_test "🔥 Running light load test (warm-up)..."
    log_info "Configuration: $LIGHT_REQUESTS requests, $LIGHT_CONCURRENCY concurrent"
    
    local ab_output
    ab_output=$(ab -n $LIGHT_REQUESTS -c $LIGHT_CONCURRENCY -q "$ORCHESTRATOR_URL/health" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ Light load test passed - no failed requests"
    elif [ "$failed_requests" -le 2 ]; then
        log_warning "⚠️  Light load test passed with warnings - $failed_requests failed requests"
    else
        log_error "❌ Light load test failed - $failed_requests failed requests"
        return 1
    fi
}

# Medium load test
run_medium_load_test() {
    log_test "⚡ Running medium load test..."
    log_info "Configuration: $MEDIUM_REQUESTS requests, $MEDIUM_CONCURRENCY concurrent"
    
    local ab_output
    ab_output=$(ab -n $MEDIUM_REQUESTS -c $MEDIUM_CONCURRENCY -q "$ORCHESTRATOR_URL/health" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ Medium load test passed - no failed requests"
    elif [ "$failed_requests" -le 5 ]; then
        log_warning "⚠️  Medium load test passed with warnings - $failed_requests failed requests"
    else
        log_error "❌ Medium load test failed - $failed_requests failed requests"
        return 1
    fi
}

# Heavy load test (stress test)
run_heavy_load_test() {
    log_test "💪 Running heavy load test (stress test)..."
    log_info "Configuration: $HEAVY_REQUESTS requests, $HEAVY_CONCURRENCY concurrent"
    log_warning "This test may take a few minutes..."
    
    local ab_output
    ab_output=$(ab -n $HEAVY_REQUESTS -c $HEAVY_CONCURRENCY -q "$ORCHESTRATOR_URL/health" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    # More lenient thresholds for heavy load
    local failure_rate
    failure_rate=$(echo "scale=2; $failed_requests * 100 / $HEAVY_REQUESTS" | bc)
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ Heavy load test passed - no failed requests"
    elif (( $(echo "$failure_rate < 5.0" | bc -l) )); then
        log_warning "⚠️  Heavy load test passed with warnings - $failed_requests failed requests (${failure_rate}% failure rate)"
    else
        log_error "❌ Heavy load test failed - $failed_requests failed requests (${failure_rate}% failure rate)"
        return 1
    fi
}

# API endpoint load test
run_api_load_test() {
    log_test "🔌 Running API endpoint load test..."
    log_info "Testing WeatherForecast API under load"
    
    local ab_output
    ab_output=$(ab -n $MEDIUM_REQUESTS -c $MEDIUM_CONCURRENCY -q "$ORCHESTRATOR_URL/weatherforecast" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ API load test passed - no failed requests"
    elif [ "$failed_requests" -le 10 ]; then
        log_warning "⚠️  API load test passed with warnings - $failed_requests failed requests"
    else
        log_error "❌ API load test failed - $failed_requests failed requests"
        return 1
    fi
}

# WebApp load test
run_webapp_load_test() {
    log_test "🌐 Running WebApp load test..."
    log_info "Testing static content delivery under load"
    
    local ab_output
    ab_output=$(ab -n $LIGHT_REQUESTS -c $LIGHT_CONCURRENCY -q "$WEBAPP_URL/" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ WebApp load test passed - no failed requests"
    elif [ "$failed_requests" -le 2 ]; then
        log_warning "⚠️  WebApp load test passed with warnings - $failed_requests failed requests"
    else
        log_error "❌ WebApp load test failed - $failed_requests failed requests"
        return 1
    fi
}

# Sustained load test (longer duration)
run_sustained_load_test() {
    log_test "⏰ Running sustained load test..."
    log_info "Running moderate load for extended period (60 seconds)"
    
    # Run for 60 seconds with moderate concurrency
    local ab_output
    ab_output=$(ab -t 60 -c 5 -q "$ORCHESTRATOR_URL/health" 2>&1)
    
    local failed_requests
    failed_requests=$(extract_ab_metrics "$ab_output")
    
    local total_requests=$(echo "$ab_output" | grep "Complete requests" | awk '{print $3}')
    local failure_rate
    
    if [ "$total_requests" -gt 0 ]; then
        failure_rate=$(echo "scale=2; $failed_requests * 100 / $total_requests" | bc)
    else
        failure_rate="100"
    fi
    
    log_info "Total requests completed: $total_requests"
    
    if [ "$failed_requests" -eq 0 ]; then
        log_info "✅ Sustained load test passed - no failed requests"
    elif (( $(echo "$failure_rate < 2.0" | bc -l) )); then
        log_warning "⚠️  Sustained load test passed with warnings - $failed_requests failed requests (${failure_rate}% failure rate)"
    else
        log_error "❌ Sustained load test failed - $failed_requests failed requests (${failure_rate}% failure rate)"
        return 1
    fi
}

# Recovery test (after heavy load)
run_recovery_test() {
    log_test "🔄 Running recovery test..."
    log_info "Testing service recovery after load"
    
    # Wait a moment for services to recover
    sleep 5
    
    # Test basic functionality
    if curl -f -s --max-time 10 "$ORCHESTRATOR_URL/health" > /dev/null; then
        log_info "✅ Service recovered successfully - health check passed"
    else
        log_error "❌ Service recovery failed - health check failed"
        return 1
    fi
    
    # Test API functionality
    local response
    response=$(curl -s --max-time 10 "$ORCHESTRATOR_URL/weatherforecast")
    
    if command -v jq &> /dev/null && echo "$response" | jq -e 'type == "array"' > /dev/null 2>&1; then
        log_info "✅ API functionality recovered - WeatherForecast API working"
    elif [ -n "$response" ]; then
        log_warning "⚠️  API responding but format may be unexpected"
    else
        log_error "❌ API recovery failed - no response"
        return 1
    fi
}

# Main test execution
main() {
    log_info "🚀 Starting AgentAsAService load testing..."
    log_info "Target URLs:"
    log_info "  OrchestratorService: $ORCHESTRATOR_URL"
    log_info "  AgentService: $AGENT_URL"
    log_info "  WebApp: $WEBAPP_URL"
    echo ""
    
    # Check dependencies
    check_dependencies
    
    # Run progressive load tests
    run_light_load_test || exit 1
    echo ""
    
    run_medium_load_test || exit 1
    echo ""
    
    run_api_load_test || exit 1
    echo ""
    
    run_webapp_load_test || exit 1
    echo ""
    
    run_heavy_load_test || exit 1
    echo ""
    
    run_sustained_load_test || exit 1
    echo ""
    
    run_recovery_test || exit 1
    echo ""
    
    log_info "🎉 All load tests completed successfully!"
    log_info "Services demonstrated good performance under various load conditions."
}

# Show usage information
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Environment Variables:"
    echo "  ORCHESTRATOR_URL   URL for OrchestratorService (default: production URL)"
    echo "  AGENT_URL          URL for AgentService (default: production URL)"
    echo "  WEBAPP_URL         URL for WebApp (default: production URL)"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run with default production URLs"
    echo "  ORCHESTRATOR_URL=http://localhost:8080 $0   # Test local development"
    echo ""
    echo "Prerequisites:"
    echo "  - Apache Bench (ab) must be installed"
    echo "  - bc calculator must be available"
    echo "  - jq (optional, for better JSON parsing)"
}

# Handle command line arguments
case "${1:-}" in
    -h|--help)
        usage
        exit 0
        ;;
    *)
        main "$@"
        ;;
esac
