# AI Workflow Roles

This module defines the ai instruction module workflow roles concept. When working in the repository and using the rest of the instrutcions modules, the actual and specific workflow you will follow will be defined by the role that is assigned to you.

## AI Instruction Modules
The instruction modules files define how everyone, in all roles, behave and perform their work in general. Instructions foud in your role definition explain what you are expected to do specifcally in your type of role.

## Role Assignment
You will be assigned your role at the beginning of your session. If you make it this far and do not know your role yet, you need to ask for your role assignment at this point before going any further.

## Role Definition and Guidelines
Once assigned a role you need to go find that role definition role file and read it. Role defintion files can be found
in the ai-workflow-roles directory. Inside the directory you will find one sub-directory for each role, named by the name of the role. Inside each named role definition sub-directory yo will a file named: ai-workflow-role.md. Read this file to undertand your role-specific definiton and instrucitons. At the end of this you may 0 or more other files linked to uspport that roles instructions. If given you will need to read all these files as well.

AgentAsAService/
    ai_instructions_modules/
        ai-workflow-roles/
            *<role_name_subdir>*/        // e.g. "collaborator" or "orchestrator"
                ai-workflow-role.md
                *<any more linked files>.md*

## Expectationa in your Role
Once you have finished reading your role definition instructions you will be expected to start performing your role's workflow.

* If you don't understand, need clarification, have questions, or see any problems make sure to ask until you acheive sufficient clarity.
* Before starting, analyze your current repository and present a short outline of what your instructions specify when applied in this context.
* Ask for any specific instructions user would like to add.
* Ask for appoval of your plan.
* Upon approval, begin executing your workflow.
* When finished, notify user and present a summary to the user.
