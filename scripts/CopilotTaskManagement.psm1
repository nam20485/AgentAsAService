# GitHub Copilot Task Management Functions
# Automates the task-based workflow for AgentAsAService project

<#
.SYNOPSIS
    Creates a new GitHub issue using the copilot-task template
.PARAMETER Title
    The title of the issue
.PARAMETER Description
    Brief description of what needs to be accomplished
.PARAMETER ImplementationSteps
    Array of implementation steps (will be converted to checkboxes)
.PARAMETER AcceptanceCriteria
    Array of acceptance criteria (will be converted to checkboxes)
.PARAMETER AssignTo
    GitHub username to assign the issue to (defaults to current user)
#>
function New-CopilotTaskIssue {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,
        
        [Parameter(Mandatory = $true)]
        [string]$Description,
        
        [Parameter(Mandatory = $true)]
        [string[]]$ImplementationSteps,
        
        [Parameter(Mandatory = $true)]
        [string[]]$AcceptanceCriteria,
        
        [Parameter(Mandatory = $false)]
        [string]$AssignTo = "@me",
        
        [Parameter(Mandatory = $false)]
        [string[]]$Labels = @(),
        
        [Parameter(Mandatory = $false)]
        [string]$RelatedIssues = "",
        
        [Parameter(Mandatory = $false)]
        [string]$TestPlan = "Testing plan to be determined during implementation.",
        
        [Parameter(Mandatory = $false)]
        [string]$ImplementationNotes = "Implementation notes to be added during development."
    )
    
    # Build the issue body using the template format
    $body = @"
# Task Description

$Description

## Implementation Plan

$($ImplementationSteps | ForEach-Object { "- [ ] $_" } | Out-String)

## Acceptance Criteria

$($AcceptanceCriteria | ForEach-Object { "- [ ] $_" } | Out-String)

## Related Issues/PRs

$RelatedIssues

## Test Plan

$TestPlan

## Implementation Notes

$ImplementationNotes
"@

    # Create the issue
    $labelsParam = if ($Labels.Count -gt 0) { $Labels | ForEach-Object { "--label", $_ } } else { @() }
    
    $issueResult = & gh issue create --title $Title --body $body --assignee $AssignTo @labelsParam
    
    if ($LASTEXITCODE -eq 0) {
        # Extract issue number from URL
        $issueNumber = ($issueResult -split '/')[-1]
        Write-Host "✅ Created issue #$issueNumber`: $Title" -ForegroundColor Green
        Write-Host "🔗 $issueResult" -ForegroundColor Cyan
        
        return @{
            Number = $issueNumber
            Url = $issueResult
            Title = $Title
        }
    } else {
        Write-Error "❌ Failed to create issue: $issueResult"
        return $null
    }
}

<#
.SYNOPSIS
    Creates a new feature branch for an issue
.PARAMETER IssueNumber
    The GitHub issue number
.PARAMETER Description
    Short description for the branch name
.PARAMETER BaseBranch
    The base branch to create from (defaults to 'copilot')
#>
function New-CopilotTaskBranch {
    param(
        [Parameter(Mandatory = $true)]
        [string]$IssueNumber,
        
        [Parameter(Mandatory = $true)]
        [string]$Description,
        
        [Parameter(Mandatory = $false)]
        [string]$BaseBranch = "copilot"
    )
    
    # Sanitize description for branch name
    $sanitizedDescription = $Description -replace '[^a-zA-Z0-9\-]', '-' -replace '-+', '-' -replace '^-|-$', ''
    $branchName = "issues/$IssueNumber-$sanitizedDescription"
    
    # Ensure we're on the base branch and it's up to date
    Write-Host "🔄 Switching to base branch '$BaseBranch'..." -ForegroundColor Yellow
    & git checkout $BaseBranch
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Failed to checkout base branch '$BaseBranch'"
        return $null
    }
    
    Write-Host "🔄 Pulling latest changes..." -ForegroundColor Yellow
    & git pull origin $BaseBranch
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "⚠️ Failed to pull latest changes, continuing with local branch"
    }
    
    # Create and switch to new branch
    Write-Host "🌟 Creating branch '$branchName'..." -ForegroundColor Yellow
    & git checkout -b $branchName
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Created and switched to branch '$branchName'" -ForegroundColor Green
        
        return @{
            Name = $branchName
            IssueNumber = $IssueNumber
            BaseBranch = $BaseBranch
        }
    } else {
        Write-Error "❌ Failed to create branch '$branchName'"
        return $null
    }
}

<#
.SYNOPSIS
    Updates an issue's checkbox progress
.PARAMETER IssueNumber
    The GitHub issue number
.PARAMETER StepText
    The text of the step to mark as completed (partial match)
.PARAMETER Completed
    Whether to mark as completed (true) or uncompleted (false)
#>
function Update-CopilotTaskProgress {
    param(
        [Parameter(Mandatory = $true)]
        [string]$IssueNumber,
        
        [Parameter(Mandatory = $true)]
        [string]$StepText,
        
        [Parameter(Mandatory = $false)]
        [bool]$Completed = $true
    )
    
    # Get current issue body
    $issueBody = & gh issue view $IssueNumber --json body -q .body
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Failed to retrieve issue #$IssueNumber"
        return $false
    }
    
    # Update checkbox - find line containing the step text and toggle checkbox
    $checkbox = if ($Completed) { "- [x]" } else { "- [ ]" }
    $lines = $issueBody -split "`n"
    $updated = $false
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match "^- \[[ x]\] .*$StepText") {
            $lines[$i] = $lines[$i] -replace "^- \[[ x]\]", $checkbox
            $updated = $true
            Write-Host "✅ Updated step: $($lines[$i])" -ForegroundColor Green
            break
        }
    }
    
    if (-not $updated) {
        Write-Warning "⚠️ Could not find step containing text: $StepText"
        return $false
    }
    
    # Update the issue
    $newBody = $lines -join "`n"
    $tempFile = [System.IO.Path]::GetTempFileName()
    $newBody | Out-File -FilePath $tempFile -Encoding UTF8
    
    & gh issue edit $IssueNumber --body-file $tempFile
    $success = $LASTEXITCODE -eq 0
    
    Remove-Item $tempFile -Force
    
    if ($success) {
        Write-Host "✅ Updated issue #$IssueNumber progress" -ForegroundColor Green
    } else {
        Write-Error "❌ Failed to update issue #$IssueNumber"
    }
    
    return $success
}

<#
.SYNOPSIS
    Creates a pull request for the current branch
.PARAMETER IssueNumber
    The GitHub issue number this PR addresses
.PARAMETER Title
    PR title (defaults to issue title)
.PARAMETER BaseBranch
    Target branch for the PR (defaults to 'copilot')
.PARAMETER Reviewer
    GitHub username to assign as reviewer
#>
function New-CopilotTaskPR {
    param(
        [Parameter(Mandatory = $true)]
        [string]$IssueNumber,
        
        [Parameter(Mandatory = $false)]
        [string]$Title,
        
        [Parameter(Mandatory = $false)]
        [string]$BaseBranch = "copilot",
        
        [Parameter(Mandatory = $false)]
        [string]$Reviewer = "nam20485"
    )
    
    # Get issue details if title not provided
    if ([string]::IsNullOrEmpty($Title)) {
        $issueData = & gh issue view $IssueNumber --json title -q .title
        if ($LASTEXITCODE -eq 0) {
            $Title = $issueData
        } else {
            $Title = "Implement issue #$IssueNumber"
        }
    }
    
    # Create PR body referencing the issue
    $prBody = @"
Implements changes for issue #$IssueNumber

## Changes Made
- Implementation details will be added during development

## Testing
- Testing details will be added during development

Closes #$IssueNumber
"@

    # Create the pull request
    $prResult = & gh pr create --title $Title --body $prBody --base $BaseBranch --reviewer $Reviewer
    
    if ($LASTEXITCODE -eq 0) {
        $prNumber = ($prResult -split '/')[-1]
        Write-Host "✅ Created PR #$prNumber`: $Title" -ForegroundColor Green
        Write-Host "🔗 $prResult" -ForegroundColor Cyan
        
        return @{
            Number = $prNumber
            Url = $prResult
            Title = $Title
            IssueNumber = $IssueNumber
        }
    } else {
        Write-Error "❌ Failed to create PR: $prResult"
        return $null
    }
}

<#
.SYNOPSIS
    Complete workflow: Create issue, branch, and prepare for development
.PARAMETER Title
    The title of the task/issue
.PARAMETER Description
    Brief description of what needs to be accomplished
.PARAMETER ImplementationSteps
    Array of implementation steps
.PARAMETER AcceptanceCriteria
    Array of acceptance criteria
#>
function Start-CopilotTask {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,
        
        [Parameter(Mandatory = $true)]
        [string]$Description,
        
        [Parameter(Mandatory = $true)]
        [string[]]$ImplementationSteps,
        
        [Parameter(Mandatory = $true)]
        [string[]]$AcceptanceCriteria
    )
    
    Write-Host "🚀 Starting new Copilot task workflow..." -ForegroundColor Cyan
    
    # Step 1: Create issue
    $issue = New-CopilotTaskIssue -Title $Title -Description $Description -ImplementationSteps $ImplementationSteps -AcceptanceCriteria $AcceptanceCriteria
    if (-not $issue) {
        Write-Error "❌ Failed to create issue, aborting workflow"
        return $null
    }
    
    # Step 2: Create branch
    $branchDescription = $Title -replace '[^a-zA-Z0-9\s]', '' -replace '\s+', '-'
    $branch = New-CopilotTaskBranch -IssueNumber $issue.Number -Description $branchDescription
    if (-not $branch) {
        Write-Error "❌ Failed to create branch, workflow incomplete"
        return $issue
    }
    
    Write-Host "`n🎯 Task workflow ready!" -ForegroundColor Green
    Write-Host "📋 Issue: #$($issue.Number) - $($issue.Title)" -ForegroundColor White
    Write-Host "🌿 Branch: $($branch.Name)" -ForegroundColor White
    Write-Host "🔗 Issue URL: $($issue.Url)" -ForegroundColor Cyan
    
    return @{
        Issue = $issue
        Branch = $branch
    }
}

<#
.SYNOPSIS
    List all open issues assigned to the current user
#>
function Get-CopilotAssignedIssues {
    Write-Host "📋 Fetching assigned issues..." -ForegroundColor Yellow
    
    $issues = & gh issue list --assignee "@me" --state open --json number,title,url
    if ($LASTEXITCODE -eq 0 -and $issues -ne "[]") {
        $issueData = $issues | ConvertFrom-Json
        
        Write-Host "`n📋 Your assigned issues:" -ForegroundColor Green
        $issueData | ForEach-Object {
            Write-Host "  #$($_.number): $($_.title)" -ForegroundColor White
            Write-Host "    🔗 $($_.url)" -ForegroundColor Cyan
        }
        
        return $issueData
    } else {
        Write-Host "✨ No open issues assigned to you" -ForegroundColor Green
        return @()
    }
}

# Export functions
Export-ModuleMember -Function New-CopilotTaskIssue, New-CopilotTaskBranch, Update-CopilotTaskProgress, New-CopilotTaskPR, Start-CopilotTask, Get-CopilotAssignedIssues
