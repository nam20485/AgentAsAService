#!/bin/bash

# Google Cloud Service Account Setup Script for AgentAsAService
# This script creates the necessary service accounts and IAM bindings for production deployment

set -e

# Configuration
PROJECT_ID="agent-as-a-service-459620"
REGION="us-west1"

echo "Setting up Google Cloud service accounts for AgentAsAService..."
echo "Project ID: $PROJECT_ID"
echo "Region: $REGION"

# Set the project
gcloud config set project $PROJECT_ID

# Enable necessary APIs
echo "Enabling required Google Cloud APIs..."
gcloud services enable \
    run.googleapis.com \
    cloudbuild.googleapis.com \
    artifactregistry.googleapis.com \
    iam.googleapis.com \
    firestore.googleapis.com \
    firebase.googleapis.com \
    iamcredentials.googleapis.com

# Create service accounts
echo "Creating service accounts..."

# OrchestratorService service account
gcloud iam service-accounts create orchestrator \
    --display-name="Orchestrator Service" \
    --description="Service account for OrchestratorService in Cloud Run" \
    --quiet || echo "Orchestrator service account already exists"

# AgentService service account
gcloud iam service-accounts create agent-service \
    --display-name="Agent Service" \
    --description="Service account for AgentService in Cloud Run" \
    --quiet || echo "Agent service account already exists"

# Set up IAM roles for OrchestratorService
echo "Setting up IAM roles for OrchestratorService..."

# Grant roles to orchestrator service account
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:orchestrator@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/firestore.user"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:orchestrator@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/logging.logWriter"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:orchestrator@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/monitoring.metricWriter"

# Allow orchestrator to impersonate itself for service-to-service auth
gcloud iam service-accounts add-iam-policy-binding \
    orchestrator@$PROJECT_ID.iam.gserviceaccount.com \
    --member="serviceAccount:orchestrator@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/iam.serviceAccountTokenCreator"

# Set up IAM roles for AgentService
echo "Setting up IAM roles for AgentService..."

# Grant roles to agent-service service account
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:agent-service@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/firestore.user"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:agent-service@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/logging.logWriter"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:agent-service@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/monitoring.metricWriter"

# Allow agent-service to impersonate itself
gcloud iam service-accounts add-iam-policy-binding \
    agent-service@$PROJECT_ID.iam.gserviceaccount.com \
    --member="serviceAccount:agent-service@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/iam.serviceAccountTokenCreator"

# Set up Cloud Build service account permissions
echo "Setting up Cloud Build service account permissions..."

# Get the Cloud Build service account email
CLOUD_BUILD_SA=$(gcloud projects describe $PROJECT_ID --format="value(projectNumber)")@cloudbuild.gserviceaccount.com

# Grant Cloud Build necessary permissions
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$CLOUD_BUILD_SA" \
    --role="roles/run.admin"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$CLOUD_BUILD_SA" \
    --role="roles/iam.serviceAccountUser"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$CLOUD_BUILD_SA" \
    --role="roles/artifactregistry.writer"

# Allow Cloud Build to use the service accounts
gcloud iam service-accounts add-iam-policy-binding \
    orchestrator@$PROJECT_ID.iam.gserviceaccount.com \
    --member="serviceAccount:$CLOUD_BUILD_SA" \
    --role="roles/iam.serviceAccountUser"

gcloud iam service-accounts add-iam-policy-binding \
    agent-service@$PROJECT_ID.iam.gserviceaccount.com \
    --member="serviceAccount:$CLOUD_BUILD_SA" \
    --role="roles/iam.serviceAccountUser"

# Create GitHub App private key secret (placeholder)
echo "Creating GitHub App private key secret placeholder..."
echo "# GitHub App Private Key - Replace with actual key" | gcloud secrets create github-app-private-key \
    --data-file=- \
    --replication-policy="automatic" || echo "Secret already exists"

# Grant agent-service access to the GitHub App private key
gcloud secrets add-iam-policy-binding github-app-private-key \
    --member="serviceAccount:agent-service@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor"

echo ""
echo "✅ Service account setup complete!"
echo ""
echo "📋 Next steps:"
echo "1. Update the GitHub App private key secret:"
echo "   gcloud secrets versions add github-app-private-key --data-file=path/to/your/github-app-key.pem"
echo ""
echo "2. Configure Cloud Build trigger environment variables:"
echo "   - GOOGLE_CLIENT_ID: Your Google OAuth 2.0 client ID"
echo "   - GITHUB_APP_ID: Your GitHub App ID"
echo "   - GITHUB_INSTALLATION_ID: Your GitHub App installation ID"
echo ""
echo "3. Service account emails created:"
echo "   - orchestrator@$PROJECT_ID.iam.gserviceaccount.com"
echo "   - agent-service@$PROJECT_ID.iam.gserviceaccount.com"
echo ""
echo "4. For local development, download service account keys:"
echo "   gcloud iam service-accounts keys create orchestrator-key.json --iam-account=orchestrator@$PROJECT_ID.iam.gserviceaccount.com"
echo "   gcloud iam service-accounts keys create agent-service-key.json --iam-account=agent-service@$PROJECT_ID.iam.gserviceaccount.com"
