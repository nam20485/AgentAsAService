#!/bin/bash

# Production Deployment Validation Script
# This script tests all components of the AgentAsAService production deployment

set -e

# Configuration
PROJECT_ID="agent-as-a-service-459620"
REGION="us-west1"
WEBAPP_URL="https://$PROJECT_ID.web.app"
ORCHESTRATOR_URL="https://orchestratorservice-$REGION-$PROJECT_ID.run.app"
AGENT_URL="https://agentservice-$REGION-$PROJECT_ID.run.app"

echo "🧪 Starting production deployment validation..."
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"
echo ""

# Test 1: Check if services are deployed
echo "📋 Test 1: Checking Cloud Run services..."

# Check OrchestratorService
echo "  Checking OrchestratorService..."
if gcloud run services describe orchestratorservice --region $REGION --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ OrchestratorService is deployed"
    ORCHESTRATOR_STATUS=$(gcloud run services describe orchestratorservice --region $REGION --project $PROJECT_ID --format="value(status.conditions[0].status)")
    echo "     Status: $ORCHESTRATOR_STATUS"
else
    echo "  ❌ OrchestratorService not found"
    exit 1
fi

# Check AgentService
echo "  Checking AgentService..."
if gcloud run services describe agentservice --region $REGION --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ AgentService is deployed"
    AGENT_STATUS=$(gcloud run services describe agentservice --region $REGION --project $PROJECT_ID --format="value(status.conditions[0].status)")
    echo "     Status: $AGENT_STATUS"
else
    echo "  ❌ AgentService not found"
    exit 1
fi

echo ""

# Test 2: Check service health endpoints
echo "📋 Test 2: Testing service health endpoints..."

# Test OrchestratorService health
echo "  Testing OrchestratorService health..."
if curl -f -s "$ORCHESTRATOR_URL/health" >/dev/null 2>&1; then
    echo "  ✅ OrchestratorService health endpoint responding"
else
    echo "  ⚠️  OrchestratorService health endpoint not responding (may not be implemented)"
fi

# Test OrchestratorService API
echo "  Testing OrchestratorService API..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$ORCHESTRATOR_URL/api/orchestrator/test" || echo "000")
if [ "$HTTP_STATUS" -eq "200" ] || [ "$HTTP_STATUS" -eq "401" ]; then
    echo "  ✅ OrchestratorService API endpoint responding (HTTP $HTTP_STATUS)"
else
    echo "  ❌ OrchestratorService API endpoint not responding (HTTP $HTTP_STATUS)"
fi

# Test AgentService (should be protected)
echo "  Testing AgentService protection..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$AGENT_URL/api/agent/test" || echo "000")
if [ "$HTTP_STATUS" -eq "401" ] || [ "$HTTP_STATUS" -eq "403" ]; then
    echo "  ✅ AgentService is properly protected (HTTP $HTTP_STATUS)"
elif [ "$HTTP_STATUS" -eq "200" ]; then
    echo "  ⚠️  AgentService is not protected - this may be intentional for testing"
else
    echo "  ❌ AgentService not responding (HTTP $HTTP_STATUS)"
fi

echo ""

# Test 3: Check Firebase Hosting
echo "📋 Test 3: Testing Firebase Hosting..."

echo "  Testing Blazor WebApp..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$WEBAPP_URL" || echo "000")
if [ "$HTTP_STATUS" -eq "200" ]; then
    echo "  ✅ Blazor WebApp is accessible (HTTP $HTTP_STATUS)"
else
    echo "  ❌ Blazor WebApp not accessible (HTTP $HTTP_STATUS)"
fi

# Test SPA routing
echo "  Testing SPA routing..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$WEBAPP_URL/authentication" || echo "000")
if [ "$HTTP_STATUS" -eq "200" ]; then
    echo "  ✅ SPA routing working (HTTP $HTTP_STATUS)"
else
    echo "  ❌ SPA routing not working (HTTP $HTTP_STATUS)"
fi

echo ""

# Test 4: Check service accounts and permissions
echo "📋 Test 4: Checking service accounts..."

# Check if service accounts exist
if gcloud iam service-accounts describe "orchestrator@$PROJECT_ID.iam.gserviceaccount.com" --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ Orchestrator service account exists"
else
    echo "  ❌ Orchestrator service account not found"
fi

if gcloud iam service-accounts describe "agent-service@$PROJECT_ID.iam.gserviceaccount.com" --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ Agent service account exists"
else
    echo "  ❌ Agent service account not found"
fi

echo ""

# Test 5: Check Firestore
echo "📋 Test 5: Testing Firestore..."

# Check if Firestore database exists
if gcloud firestore databases describe --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ Firestore database exists"
else
    echo "  ❌ Firestore database not found"
fi

echo ""

# Test 6: Check secrets
echo "📋 Test 6: Checking secrets..."

# Check GitHub App private key secret
if gcloud secrets describe github-app-private-key --project $PROJECT_ID &>/dev/null; then
    echo "  ✅ GitHub App private key secret exists"
    VERSIONS=$(gcloud secrets versions list github-app-private-key --project $PROJECT_ID --format="value(name)" | wc -l)
    echo "     Versions: $VERSIONS"
else
    echo "  ❌ GitHub App private key secret not found"
fi

echo ""

# Summary
echo "🎯 Validation Summary:"
echo ""
echo "Service URLs:"
echo "  🌐 Blazor WebApp: $WEBAPP_URL"
echo "  🔧 OrchestratorService: $ORCHESTRATOR_URL"
echo "  🤖 AgentService: $AGENT_URL"
echo ""
echo "📋 Next steps for complete testing:"
echo "1. Test Google OAuth login at $WEBAPP_URL"
echo "2. Test authenticated API calls from the Blazor app"
echo "3. Monitor logs in Google Cloud Console"
echo "4. Verify Firestore data persistence"
echo ""
echo "✅ Production deployment validation completed!"
