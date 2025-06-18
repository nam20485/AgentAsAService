# Task-based Workflow: Document and Work By Task Issues

This file describes your task-based workflow where you plan your task(s) out prior to performing the work, create a GH issue, and use then it to document progress and completion of the task.

## General Workflow Principles

* **Permission Management**: If it's a read-only operation, asking permission from user is not necessary. Only ask for permission for write operations or potentially dangerous actions.
* **Issue Documentation**: Always read issue comments and reply in the comments to document decisions and maintain conversation history.
* **State Monitoring**: Always check all aspects of issue state including labels, comments, assignees, and milestones before proceeding.
* **Workflow Links**: When creating issues related to workflow failures, include the workflow name, failing step name, and direct links to the workflow run and logs.

## Permission Request Rules

**The Three Core Rules for Permission Requests:**

1. **Read-only operations**: NEVER ask for permission
   - Examples: gh issue view, gh issue list, reading files, checking status
   - These operations cannot cause harm or data loss

2. **Simple operations**: DO NOT ask for permission  
   - Examples: gh issue comment, adding/removing labels, updating issue states
   - These are standard workflow operations with built-in safeguards

3. **Large destructive changes with no recovery**: DO ask for permission
   - Examples: Deleting repositories, mass file deletions, major architecture changes
   - These operations could cause significant data loss or system disruption

**Behavioral Change Strategy:**
- When I feel the urge to ask "Can I..." or "Should I...", I will pause and evaluate against these three rules
- Default assumption: If it's not clearly a large destructive change, proceed without asking
- Focus on execution over hesitation - the workflow is designed to be safe

**Why This Matters:**
- Reduces cognitive overhead and decision fatigue
- Maintains development momentum and flow
- Builds confidence in standard operations
- Reserves permission requests for truly critical decisions

## Internalized Behavioral Changes

### Three Key Rules (Established During CI Workflow Investigation)

#### Rule 1: Record workflow/process instructions in the ai instructions module files

* When receiving new workflow guidance, immediately document it in the appropriate module file
* This creates a persistent record and helps internalize the behavior
* Examples: process rules, behavioral corrections, workflow patterns

#### Rule 2: Permission requests only for large/destructive changes

* Read-only operations (file reading, API queries, log analysis): Proceed without asking
* Simple operations (issue creation, comments, label updates): Proceed without asking
* Large/destructive changes (major refactoring, file deletion, CI/CD changes): Ask for permission
* This eliminates friction and maintains productive workflow momentum

#### Rule 3: Always check and respond to issue state

* Read all issue comments before taking action
* Check issue labels, assignees, and current state
* Reply to issue comments in the issue itself, not just in conversation
* Include workflow names, step details, and links when creating workflow-related issues

### Implementation Notes

* These rules replace the previous pattern of asking permission for routine operations
* The behavioral change strategy emphasizes immediate acceptance and documentation of new guidance
* Focus on maintaining workflow efficiency while preserving safety for truly impactful changes
* Document all new process instructions as they are received to build a comprehensive workflow guide

## Task Workflow Overview

For all non-trivial sized tasks you will complete the following process as you work through the task:

### Task Workflow Steps

1. After determining plan for implementation, create new GH issue and outline steps there. For each step of the task create a short few sentences that describe the step and a checkbox.
1. Include a short paragraph in the body as a description. Outline what you intend to change, how you intend to change it, the outcome, and why.
1. Print copy of issue in your display and ask user if they would like to view the issue in the browser.
1. Ask user if the issue plan is acceptable. If not iterate until it is.
1. As you work through the task, mark your progress using the checkboxes.
1. After completion inform user and ask if resolution is acceptable.
1. When writing commit messages including the issue's task, add to bottom of commit message.
  
### Multiple Tasks / Sizing

Always document entire plan prior to beginning work. If plan is large enough or if you have multiple different tasks to complete, create issues for all of them, in order to plan and record your entire work. As you complete each task issue, select a new one to work on. If the work is large, always break down into two or more tasks. The more you break large work down into smaller easier to digest sized tasks, the easier it will be to plan, complete, and communicate about.

### Work Session Context

Documenting planned work tasks will help you understand what you have completed and what is still remaining. If you have to switch contexts or stop working before completing your planned tasks, then they will remain there. When you come back you will know exactly where you left off, what is remaining, and where to begin. Always check GH Issues for items assigned to you when you begin a session. If they exist ask whether you should begin working on them.

### Details

* Issue Template: [.github/ISSUE_TEMPLATE/copilot-task.md](/.github/ISSUE_TEMPLATE/copilot-task.md)
* You will use your `gh cli` tool to create and manage the issues programmatically. Anytime you create an issue, you need to notify in the chat with `<issue_#>:<name>` as a reference/link.
* Integration with projects, milestones will be added later.
* Application: Most all development requires use of this task-based issue workflow. Only very minor changes are exempt. When in doubt either:
  * Ask
  * Err on side of task-based workflow
* Tags/labels:
  * Tag issues assigned to you with 'assigned:copilot'. This will help with ambiguity in the assigned field until we start using the team repo.
  * Tag your current issue with 'state:in-progress'. Keep this tag correctly updated.
  * Format: Labels using a `key:value` structure (e.g., `type:bug`, `priority:high`) are treated as key-value pairs. For instance, in a label like `module:auth`, 'module' is the key and 'auth' is the value.

### Branch Management

* Create feature branches for each issue using naming convention: `issues/<number>-<short-description>`
* Example: `issues/42-fix-authentication-bug`
* Branch should be created from `copilot` before starting work
* Branch should be deleted after successful merge
* Once finished you will create a PR and assign me as a reviewer.
* Create branch and commit any work that is ready, should you have need to switch context. **Important**. We could expose ourselves to confusion and possible code loss if we leave changes 'dangling' in the working copy. Commit frequently and often. Many small commits are always better than fewer large ones.
* Once I move us to teams plan you will have your own assigned account so you can then assign yourself as issue owner
* Once project/milestone integration is completed you can assign project and I will review and assign milestone (or you can ask if obvious)

#### Commit Message Format

```markdown
{ feat,defect,etc. }: implement user authentication

- Add Google OAuth integration
- Update user service with token validation
- Add authentication middleware

/Closes #123
```

## Internalized Behavioral Rules

Following these specific behavioral rules to maintain efficient workflow:

1. **Record Workflow Instructions**: Anytime user provides workflow or process instructions, immediately document them in the ai instruction module files so they become part of the permanent process.

2. **Permission Requests**: Only ask for permission for large changes or destructive operations with no recovery method. Don't ask for read-only operations or simple operations.

3. **Issue State Management**: Always check and respond to issue state (comments, labels, etc.) and reply in issue comments to document decisions.

4. **Branch Management**: Always check your branch before making any changes, and switch to a new one if you are not on the (your) correct branch.

### Meta-Rule: Documentation of New Rules

Anytime user states "New Rule: xyz" then document that rule in this task-based workflow file.
