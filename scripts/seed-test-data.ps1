#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Seeds test data into the AgentAsAService databases for development

.DESCRIPTION
    Creates sample orchestrators, projects, and agent sessions in both
    OrchestratorService and AgentService databases for testing and development

.PARAMETER Reset
    If specified, attempts to clear existing data before seeding

.PARAMETER Environment
    Target environment (Development, Testing, Staging, Production)

.PARAMETER OrchestratorServiceUrl
    Base URL for the OrchestratorService API

.PARAMETER AgentServiceUrl
    Base URL for the AgentService API

.EXAMPLE
    .\seed-test-data.ps1
    
.EXAMPLE
    .\seed-test-data.ps1 -Reset -Environment Testing

.EXAMPLE
    .\seed-test-data.ps1 -Environment Staging -OrchestratorServiceUrl "https://orchestrator-staging.example.com"
#>

param(
    [switch]$Reset,
    [ValidateSet("Development", "Testing", "Staging", "Production")]
    [string]$Environment = "Development",
    [string]$OrchestratorServiceUrl,
    [string]$AgentServiceUrl
)

# Set default URLs based on environment
if (-not $OrchestratorServiceUrl) {
    $OrchestratorServiceUrl = switch ($Environment) {
        "Development" { "http://localhost:8080" }
        "Testing" { "http://localhost:8080" }
        "Staging" { "https://orchestrator-staging.example.com" }
        "Production" { "https://orchestratorservice-region-agent-as-a-service-459620.run.app" }
    }
}

if (-not $AgentServiceUrl) {
    $AgentServiceUrl = switch ($Environment) {
        "Development" { "http://localhost:7001" }
        "Testing" { "http://localhost:7001" }
        "Staging" { "https://agentservice-staging.example.com" }
        "Production" { "https://agentservice-region-agent-as-a-service-459620.run.app" }
    }
}

# Colors for output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"
$WarningColor = "Yellow"

function Write-ColoredOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Test-ServiceAvailability {
    param([string]$ServiceUrl, [string]$ServiceName)
    
    try {
        Write-ColoredOutput "Checking $ServiceName availability at $ServiceUrl..." $InfoColor
        $response = Invoke-RestMethod -Uri "$ServiceUrl/health" -Method Get -TimeoutSec 10
        Write-ColoredOutput "✓ $ServiceName is running" $SuccessColor
        return $true
    }
    catch {
        Write-ColoredOutput "✗ $ServiceName is not available at $ServiceUrl" $ErrorColor
        Write-ColoredOutput "  Error: $($_.Exception.Message)" $ErrorColor
        return $false
    }
}

function Invoke-ApiCall {
    param(
        [string]$Url,
        [string]$Method = "POST",
        [object]$Body = $null,
        [string]$Description
    )
    
    try {
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $headers
            TimeoutSec = 30
        }
        
        if ($Body -and $Method -ne "GET") {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        Write-ColoredOutput "Creating: $Description..." $InfoColor
        $response = Invoke-RestMethod @params
        Write-ColoredOutput "✓ Successfully created: $Description" $SuccessColor
        return $response
    }
    catch {
        Write-ColoredOutput "✗ Failed to create: $Description" $ErrorColor
        Write-ColoredOutput "  Error: $($_.Exception.Message)" $ErrorColor
        if ($_.Exception.Response) {
            $errorContent = $_.Exception.Response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
            if ($errorContent.error) {
                Write-ColoredOutput "  Details: $($errorContent.error)" $ErrorColor
            }
        }
        return $null
    }
}

function Clear-ExistingData {
    param([string]$ServiceUrl)
    
    Write-ColoredOutput "Attempting to clear existing data (if endpoints exist)..." $WarningColor
    # Note: This would require DELETE endpoints which may not exist yet
    # For now, this is a placeholder for future implementation
}

# Main script execution
Write-ColoredOutput "=== AgentAsAService Seed Data Script ===" $InfoColor
Write-ColoredOutput "Environment: $Environment" $InfoColor
Write-ColoredOutput "OrchestratorService: $OrchestratorServiceUrl" $InfoColor
Write-ColoredOutput "AgentService: $AgentServiceUrl" $InfoColor
Write-ColoredOutput "Started at: $(Get-Date)" $InfoColor
Write-ColoredOutput ""

# Check service availability
$orchestratorServiceAvailable = Test-ServiceAvailability -ServiceUrl $OrchestratorServiceUrl -ServiceName "OrchestratorService"
$agentServiceAvailable = Test-ServiceAvailability -ServiceUrl $AgentServiceUrl -ServiceName "AgentService"

if (-not $orchestratorServiceAvailable) {
    Write-ColoredOutput "OrchestratorService must be running to seed data. Please start it first." $ErrorColor
    Write-ColoredOutput "Run: dotnet run --project OrchestratorService" $InfoColor
    exit 1
}

Write-ColoredOutput ""

# Reset data if requested
if ($Reset) {
    Clear-ExistingData -ServiceUrl $OrchestratorServiceUrl
    Clear-ExistingData -ServiceUrl $AgentServiceUrl
}

# Seed Orchestrators
Write-ColoredOutput "=== Creating Orchestrators ===" $InfoColor

$orchestrators = @()

$orchestrator1 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/orchestrators" -Body @{
    name = "Development Orchestrator"
} -Description "Development Orchestrator"

if ($orchestrator1) { $orchestrators += $orchestrator1 }

$orchestrator2 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/orchestrators" -Body @{
    name = "Testing Orchestrator"
} -Description "Testing Orchestrator"

if ($orchestrator2) { $orchestrators += $orchestrator2 }

$orchestrator3 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/orchestrators" -Body @{
    name = "Production Orchestrator"
} -Description "Production Orchestrator"

if ($orchestrator3) { $orchestrators += $orchestrator3 }

Write-ColoredOutput ""

# Seed Projects
Write-ColoredOutput "=== Creating Projects ===" $InfoColor

$projects = @()

if ($orchestrator1) {
    $project1 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/projects" -Body @{
        name = "AgentAsAService Core"
        orchestratorId = $orchestrator1.id
        description = "Core agent orchestration service"
        repository = @{
            name = "AgentAsAService"
            address = "https://github.com/nam20485/AgentAsAService"
            branch = "main"
        }
    } -Description "AgentAsAService Core Project"
    
    if ($project1) { $projects += $project1 }

    $project2 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/projects" -Body @{
        name = "AI Workflow Automation"
        orchestratorId = $orchestrator1.id
        description = "Automated AI workflows and task management"
        repository = @{
            name = "ai-workflow-automation"
            address = "https://github.com/example/ai-workflow-automation"
            branch = "develop"
        }
    } -Description "AI Workflow Automation Project"
    
    if ($project2) { $projects += $project2 }
}

if ($orchestrator2) {
    $project3 = Invoke-ApiCall -Url "$OrchestratorServiceUrl/api/projects" -Body @{
        name = "Test Automation Suite"
        orchestratorId = $orchestrator2.id
        description = "Comprehensive test automation framework"
        repository = @{
            name = "test-automation-suite"
            address = "https://github.com/example/test-automation-suite"
            branch = "main"
        }
    } -Description "Test Automation Suite Project"
    
    if ($project3) { $projects += $project3 }
}

Write-ColoredOutput ""

# Seed Agent Sessions (if AgentService is available)
if ($agentServiceAvailable) {
    Write-ColoredOutput "=== Creating Agent Sessions ===" $InfoColor

    $agentSessions = @()

    $session1 = Invoke-ApiCall -Url "$AgentServiceUrl/api/agentsessions" -Body @{
        repositoryUrl = "https://github.com/nam20485/AgentAsAService"
        branch = "main"
        createdBy = "developer@example.com"
        configuration = @{
            environment = "development"
            autoStart = $true
            maxDuration = 3600
        }
    } -Description "Main Repository Agent Session"

    if ($session1) { $agentSessions += $session1 }

    $session2 = Invoke-ApiCall -Url "$AgentServiceUrl/api/agentsessions" -Body @{
        repositoryUrl = "https://github.com/example/ai-workflow-automation"
        branch = "develop"
        createdBy = "tester@example.com"
        configuration = @{
            environment = "testing"
            autoStart = $false
            maxDuration = 7200
        }
    } -Description "AI Workflow Agent Session"

    if ($session2) { $agentSessions += $session2 }

    $session3 = Invoke-ApiCall -Url "$AgentServiceUrl/api/agentsessions" -Body @{
        repositoryUrl = "https://github.com/example/test-automation-suite"
        branch = "main"
        createdBy = "qa@example.com"
        configuration = @{
            environment = "production"
            autoStart = $true
            maxDuration = 1800
        }
    } -Description "Test Automation Agent Session"

    if ($session3) { $agentSessions += $session3 }
} else {
    Write-ColoredOutput "AgentService not available - skipping agent session creation" $WarningColor
}

Write-ColoredOutput ""

# Summary
Write-ColoredOutput "=== Seed Data Summary ===" $InfoColor
Write-ColoredOutput "Orchestrators created: $($orchestrators.Count)" $SuccessColor
Write-ColoredOutput "Projects created: $($projects.Count)" $SuccessColor
if ($agentServiceAvailable) {
    Write-ColoredOutput "Agent Sessions created: $($agentSessions.Count)" $SuccessColor
}

Write-ColoredOutput ""
Write-ColoredOutput "=== Database Files ===" $InfoColor
Write-ColoredOutput "You can now inspect the data using LiteDB Studio:" $InfoColor
Write-ColoredOutput "• OrchestratorService: local-storage/orchestrator.db" $InfoColor
if ($agentServiceAvailable) {
    Write-ColoredOutput "• AgentService: local-storage/agentservice.db" $InfoColor
}

Write-ColoredOutput ""
Write-ColoredOutput "Seed data creation completed at: $(Get-Date)" $SuccessColor
