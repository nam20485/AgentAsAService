# Assignment-Based Workflow

The assignment-based workflow builds on the task-based workflow process by including the  new concept of assignments. Workflow assignments are specifically-defined sets of goals,  acceptance criteria, and steps.

## Workflow Assignments

* Each workflow assignment is unique and describes how to accomplish a specific task or stage of the project.
* Workflow assignments are assigned to you by the orchestrator
* When assigned, you are to perform the assignment until finished and/or assigned something new.
* Each type of assignment is described in a workflow assignment definiton file.
* Each definition file contains everything you need to know to be able to perform and complete an assignment successfully.
* Definition files are found under the `ai_instruction_modules` directory in a subdirectory named exactly as the workflow assignment definition's "short ID" (See sections below for definition of an "assignment short ID").

## Workflow Assignment Definition Format

The format is made up of the following sections:

* Assignment Title: A descriptive title for the assignment.
* Assignment Short ID: A unique identifier for the assignment, typically in parentheses.
* Goal: A clear statement of what the assignment aims to achieve.
* Acceptance Criteria: A list of conditions that must be met for the assignment to be considered complete.
* Assignment: A detailed description of the assignment, including any specific tasks or actions required.
* Detailed Steps: A step-by-step guide on how to complete the assignment, including any specific instructions or considerations.
* Completion: Instructions on how to finalize the assignment, including any follow-up actions or confirmations needed.

Below find a real example of a workflow assignment definition, including

# Pull Request (PR) Approval and Merge

## (pr-approval-and-merge)

### Goal

Resolve all PR comments and get the PR approved

### Acceptance Criteria

1. All PR comments resolved.
...
4. Issue closed.

### Assignment

In this stage your assignment will be to iterate on the PR comments until they
...

### Detailed Steps

1. For each unresolved comment, review the comment:
...

    1. If there are multiple options, or only one but its not trivial, is risky, and/or requires further input then leave a reply explaining the plan, or different options.
    ...
    1. If you are unsure how to proceed, then ask for help in the chat or reply.
...

* Iterate in this manner until all comments and replies have been addressed.
* Once all PR comments have been resolved, the stake-holder will approve the PR.

### Completion

Ask the stake-holder if they want you to merge the PR or if they would like to do so themselves.
...

