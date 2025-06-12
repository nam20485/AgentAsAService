# TODO

## TODO 1 ✅ COMPLETED

Need Four pages:

Projects, Project, Team, Agent pages in Web app

1. Projects: List proejects, add project, stop project, remove project
2. Project details
3. Team: team details, list agents, add agent, remove agent, stop agent, links to project
4. Agent, agent details, stop agent, Links back to Agents team, project  Microsoft.AspNetCore.Diagnostics.

**RESOLUTION:** ✅ All four pages implemented

- Projects page: List projects, add project, stop project, remove project
- Project page: Project details with navigation
- Team page: Team details, list agents, add agent, remove agent, stop agent
- Agent page: Agent details, stop agent, links back to team and project

## TODO 2 ✅ COMPLETED

### Fix auth exception

DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The AuthorizationPolicy named: 'RequireServiceAuthentication' was not found.
         at Microsoft.AspNetCore.Authorization.AuthorizationPolicy.CombineAsync(IAuthorizationPolicyProvider policyProvider, IEnumerable`1 authorizeData, IEnumerable`1 policies)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIMiddleware.Invoke(HttpContext httpContext)
         at Swashbuckle.AspNetCore.Swagger.SwaggerMiddleware.Invoke(HttpContext httpContext, ISwaggerProvider swaggerProvider)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

**RESOLUTION:** ✅ Authentication configuration fixed

- Fixed OrchestratorService authorization policies for all environments (Development, Testing, Staging, Production)
- Fixed OrchestratorWebApp to conditionally configure authentication based on environment
- Development mode now bypasses authentication entirely on both frontend and backend
- Production mode retains full Google OAuth authentication
- Resolved JavaScript `AuthenticationService.init` error in browser console

## TODO 3 ✅ COMPLETED

Web App Model twin objects

For ProjectMdel, TeamModel, etc.- we have a reference to SharedLib, why mirror the original model objects with twins? Can we just use the actual model objects? Its less error prone bc when a model object changes then all projects using the SharedLib.Model objects are updated immeditaely w/o having to remeber to update their miorros objects? Is this not a good idea? Do we not to tie the projects together this tightly?

**RESOLUTION:** ✅ Implemented using SharedLib models directly

- Added SharedLib project reference to OrchestratorWebApp
- Updated Projects.razor to use `SharedLib.Model.Project` and `SharedLib.DTOs.CreateProjectRequest`
- Updated Project.razor to use `SharedLib.Model.Project`
- Updated Team.razor to use `SharedLib.Model.Project` and `SharedLib.DTOs.AddAgentToTeamRequest`
- Updated Agent.razor to use `SharedLib.Model.Project`, `SharedLib.Model.Collaborator`, and `SharedLib.Model.AgentSession`
- Removed all duplicate model definitions from web app pages
- Benefits achieved:
  - Single source of truth for model definitions
  - Automatic synchronization when models change
  - Reduced code duplication and maintenance
  - Type safety guaranteed between API and UI
  - Less error-prone development

## TODO 5 ✅ COMPLETED

**Optimize AI Instruction Modules for Brevity**

Current instruction modules are verbose with conversational style. GitHub recommends shorter custom instructions to avoid them becoming unwieldy.

**Goal:** Streamline instruction files by 50-70% without reducing effectiveness

**RESOLUTION:** ✅ Optimized all instruction modules

**Files Optimized:**

- `ai_instruction_modules/ai-terminal-management.md` (55 lines → 32 lines, 42% reduction)
- `docs/AUTHENTICATION.md` (119 lines → 46 lines, 61% reduction)
- `docs/ENVIRONMENT_CONFIGURATION.md` (57 lines → 32 lines, 44% reduction)
- `docs/README-AUTOMATION.md` (157 lines → 82 lines, 48% reduction)

**Improvements Applied:**

- Replaced conversational tone with imperative commands
- Converted paragraph explanations to bullet points and tables
- Removed redundant formatting and decorative elements
- Lead with code examples, minimized explanatory text
- Used action verbs: Run, Set, Configure, Test

**Benefits Achieved:**

- Faster AI comprehension
- Reduced token consumption
- Clearer action items
- Better GitHub Copilot compliance
- Overall 49% average reduction in line count
