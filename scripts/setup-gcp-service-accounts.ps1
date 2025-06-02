# Google Cloud Service Account Setup Script for AgentAsAService (PowerShell)
# This script creates the necessary service accounts and IAM bindings for production deployment

param(
    [string]$ProjectId = "agent-as-a-service-459620",
    [string]$Region = "us-west1"
)

$ErrorActionPreference = "Stop"

Write-Host "Setting up Google Cloud service accounts for AgentAsAService..." -ForegroundColor Green
Write-Host "Project ID: $ProjectId" -ForegroundColor Cyan
Write-Host "Region: $Region" -ForegroundColor Cyan

# Set the project
gcloud config set project $ProjectId

# Enable necessary APIs
Write-Host "Enabling required Google Cloud APIs..." -ForegroundColor Yellow
$apis = @(
    "run.googleapis.com",
    "cloudbuild.googleapis.com", 
    "artifactregistry.googleapis.com",
    "iam.googleapis.com",
    "firestore.googleapis.com",
    "firebase.googleapis.com",
    "iamcredentials.googleapis.com"
)

gcloud services enable $apis

# Create service accounts
Write-Host "Creating service accounts..." -ForegroundColor Yellow

# OrchestratorService service account
try {
    gcloud iam service-accounts create orchestrator `
        --display-name="Orchestrator Service" `
        --description="Service account for OrchestratorService in Cloud Run" `
        --quiet
} catch {
    Write-Host "Orchestrator service account already exists" -ForegroundColor Orange
}

# AgentService service account
try {
    gcloud iam service-accounts create agent-service `
        --display-name="Agent Service" `
        --description="Service account for AgentService in Cloud Run" `
        --quiet
} catch {
    Write-Host "Agent service account already exists" -ForegroundColor Orange
}

# Set up IAM roles for OrchestratorService
Write-Host "Setting up IAM roles for OrchestratorService..." -ForegroundColor Yellow

$orchestratorSA = "serviceAccount:orchestrator@$ProjectId.iam.gserviceaccount.com"
$orchestratorRoles = @(
    "roles/firestore.user",
    "roles/logging.logWriter", 
    "roles/monitoring.metricWriter"
)

foreach ($role in $orchestratorRoles) {
    gcloud projects add-iam-policy-binding $ProjectId `
        --member=$orchestratorSA `
        --role=$role
}

# Allow orchestrator to impersonate itself for service-to-service auth
gcloud iam service-accounts add-iam-policy-binding `
    "orchestrator@$ProjectId.iam.gserviceaccount.com" `
    --member=$orchestratorSA `
    --role="roles/iam.serviceAccountTokenCreator"

# Set up IAM roles for AgentService
Write-Host "Setting up IAM roles for AgentService..." -ForegroundColor Yellow

$agentSA = "serviceAccount:agent-service@$ProjectId.iam.gserviceaccount.com"
$agentRoles = @(
    "roles/firestore.user",
    "roles/logging.logWriter",
    "roles/monitoring.metricWriter"
)

foreach ($role in $agentRoles) {
    gcloud projects add-iam-policy-binding $ProjectId `
        --member=$agentSA `
        --role=$role
}

# Allow agent-service to impersonate itself
gcloud iam service-accounts add-iam-policy-binding `
    "agent-service@$ProjectId.iam.gserviceaccount.com" `
    --member=$agentSA `
    --role="roles/iam.serviceAccountTokenCreator"

# Set up Cloud Build service account permissions
Write-Host "Setting up Cloud Build service account permissions..." -ForegroundColor Yellow

# Get the Cloud Build service account email
$projectNumber = gcloud projects describe $ProjectId --format="value(projectNumber)"
$cloudBuildSA = "serviceAccount:$projectNumber@cloudbuild.gserviceaccount.com"

# Grant Cloud Build necessary permissions
$cloudBuildRoles = @(
    "roles/run.admin",
    "roles/iam.serviceAccountUser", 
    "roles/artifactregistry.writer"
)

foreach ($role in $cloudBuildRoles) {
    gcloud projects add-iam-policy-binding $ProjectId `
        --member=$cloudBuildSA `
        --role=$role
}

# Allow Cloud Build to use the service accounts
gcloud iam service-accounts add-iam-policy-binding `
    "orchestrator@$ProjectId.iam.gserviceaccount.com" `
    --member=$cloudBuildSA `
    --role="roles/iam.serviceAccountUser"

gcloud iam service-accounts add-iam-policy-binding `
    "agent-service@$ProjectId.iam.gserviceaccount.com" `
    --member=$cloudBuildSA `
    --role="roles/iam.serviceAccountUser"

# Create GitHub App private key secret (placeholder)
Write-Host "Creating GitHub App private key secret placeholder..." -ForegroundColor Yellow
try {
    "# GitHub App Private Key - Replace with actual key" | gcloud secrets create github-app-private-key `
        --data-file=- `
        --replication-policy="automatic"
} catch {
    Write-Host "Secret already exists" -ForegroundColor Orange
}

# Grant agent-service access to the GitHub App private key
gcloud secrets add-iam-policy-binding github-app-private-key `
    --member=$agentSA `
    --role="roles/secretmanager.secretAccessor"

Write-Host ""
Write-Host "✅ Service account setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "1. Update the GitHub App private key secret:" -ForegroundColor White
Write-Host "   gcloud secrets versions add github-app-private-key --data-file=path/to/your/github-app-key.pem" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Configure Cloud Build trigger environment variables:" -ForegroundColor White
Write-Host "   - GOOGLE_CLIENT_ID: Your Google OAuth 2.0 client ID" -ForegroundColor Gray
Write-Host "   - GITHUB_APP_ID: Your GitHub App ID" -ForegroundColor Gray
Write-Host "   - GITHUB_INSTALLATION_ID: Your GitHub App installation ID" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Service account emails created:" -ForegroundColor White
Write-Host "   - orchestrator@$ProjectId.iam.gserviceaccount.com" -ForegroundColor Gray
Write-Host "   - agent-service@$ProjectId.iam.gserviceaccount.com" -ForegroundColor Gray
Write-Host ""
Write-Host "4. For local development, download service account keys:" -ForegroundColor White
Write-Host "   gcloud iam service-accounts keys create orchestrator-key.json --iam-account=orchestrator@$ProjectId.iam.gserviceaccount.com" -ForegroundColor Gray
Write-Host "   gcloud iam service-accounts keys create agent-service-key.json --iam-account=agent-service@$ProjectId.iam.gserviceaccount.com" -ForegroundColor Gray
