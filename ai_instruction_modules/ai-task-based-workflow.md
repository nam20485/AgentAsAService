# Task-based Workflow: Document and Work By Task Issues

This file describes your task-based workflow where you plan your task(s) out prior to performing the work, create a GH issue, and use then it to document progress and completion of the task.

## Task Workflow Overview

For all non-trivial sized tasks you will complete the following process as you work through the task:

### Task Workflow Steps

#### Review Current State

0. Before making a plan, review the current state of the workspace/codebase
1. Especially in the area where you will be implementing your new task(s)
	1. Check for previous progress on this task:
		1. Have you already been working on this task? 		
		1. Does there already exist a branch and/or an issue? 
			1. If so, inspect the changes in the issue and/or branch to see what you have already accomplished. 
			1. What checkboxes have been checked off? What do the previous updates say?
			1. Are there remote changes? Pull the branch
			1. Are there upstream changes? Merge any upstream changes in
	1. If no previous work/you are starting initially on the task
		1. Is there already similar functionality in place to build on?
1. When you present your plan, include a review of the current status.

#### Plan

1. After determining plan for implementation, create new GH issue and outline steps there.
	1. For each step of the task create a short few sentences that describe the step and a checkbox. 
	1. The more detailed, the better. 
	1. It is better to have many small steps than a few large ones.
1. Include a short paragraph in the body as a description. 
	1. Outline what you intend to change
	1. How you intend to change it?
	1. The outcome?
	1. and why?
	1. Risks?
		1. Risk mitigations?
	1. Acceptance Criteria

#### Approval

1. Print copy of issue in your display and ask user if they would like to view the issue in the browser.
1. Ask user if the issue plan is acceptable.
	1. If not iterate until it is.
	1. Stake holder will leave comments in the issue.
	1. Once you receive approval, mark the issue as "state:in-progress" and begin work

#### Initial Implementation

1. As you work through the task, mark your progress using the checkboxes.
1. This is important. **As you complete items mark the checkboxes and update the issue!**
	1. If you have to stop, then when you come back this will provide you and I the context we need to know where to pick back up thew work.
1. After completion inform user and ask if resolution is acceptable.
1. Iterate on review with stake holder until accepted.
1. When writing commit messages including the issue's task, add to bottom of commit message.
1. Commit, push and create PR.
1. Add stake-holder and /gemini as approvers

### Assignment: Pull Request (PR) Approval and Merge 
**(pr-approval-and-merge)**

**Stage:** Pull Request (PR) Approval and Merge **(pr-approval-and-merge)**

**Goal:** Resolve all PR comments and get the PR approved
**Acceptance Criteria:** 
	1. All PR comments resolved.
	2. PR approved.
	3. Branch merged upstream.
	4. Issue closed.

**Assignment:**

In this stage your assignment will be to iterate on the PR comments until they 
are all resolved and the PR is approved. Once PR comments are available
you will work systematically to resolve each comment, one after another, 

**Detailed Steps:**

1. For each unresolved comment, review the comment:

	1. If you have already submitted a reply with a plan, then you must wait
	for the stake holder to review your plan and approve it before you can implement the changes.
		1. Move on to the next comment.

	1. If you have not yet submitted a reply, then you will need to review the comment and the entire context of this comment. 
		1. This includes:
			1. All the replies in this comment's thread
			1. The PR's original code changes
			1. Any code changes or plans resulting from previous iterations on this comment.
			1. Have you already submitted any plans that were rejected? 
			1. At this point, go read the issue and PR again
		1. Given the context determine the options to resolve the comment.
			1. If it's trivial, there is only one option, and requires no input, then make the code change(s) and update the reply with 
			a short explanation.
				1. If there are multiple options, or only one but its not trivial, is risky, and/or requires further input then leave a reply explaining the plan, or different options.
				1. If you have a recommendation, then state so and why.
				1. Ask for approval, direction, or other required input/feedback to proceed.
				1. Move on the next comment.
				1. If you are unsure how to proceed, then ask for help in the chat or reply.
	1. If the stake-holder reply contains approval for a previously submitted plan, then you will be able to implement the changes now.
		1. If the stake-holder reply contains a request for changes, then you will need to review the comment and ensure that you understand the feedback provided.
		1. Address the feedback and update your implementation accordingly.
		1. Communicate your changes and seek further clarification if needed.

Iterate in this manner until all comments and replies have been addressed.

Once all PR comments have been resolved, the stake-holder will approve the PR.
Ask the stake-holder if they want you to merge the PR or if they would like to do so themselves.
If so, then merge the PR, and close the issue and branch. 
Eat a cookie and have an espresso or a milkshake. You can have both if you want. 
Ask the stake-holder for your next assignment. 
  
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
* **Link the issue to the branch** when creating the feature branch (or create the branch from the issue which automatically accomplishes the link)
* Branch should be deleted after successful merge
* Create branch and commit any work that is ready, should you have need to switch context. **Important**. We could expose ourselves to confusion and possible code loss if we leave changes 'dangling' in the working copy. Commit frequently and often. Many small commits are always better than fewer large ones.
* Once I move us to teams plan you will have your own assigned account so you can then assign yourself as issue owner
* Once project/milestone integration is completed you can assign project and I will review and assign milestone (or you can ask if obvious)

#### Pull Request (PR) Requirements

* **Assign nam20485 as a reviewer** when creating the PR
* **Add `/Closes #<issue_number>`** to the PR description to automatically close the issue when merged
* **Add `/gemini review`** to the PR to add Gemini Code Assist as a reviewer
* Once finished you will create a PR following these requirements

#### Commit Message Format

```markdown
{ feat,defect,etc. }: implement user authentication

- Add Google OAuth integration
- Update user service with token validation
- Add authentication middleware

/Closes #123
```
