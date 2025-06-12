# TODO

## TODO 1

Need Four pages:

Projects, Project, Team, Agent pages in Web app

1. Projects: List proejects, add project, stop project, remove project
2. Project details
3. Team: team details, list agents, add agent, remove agent, stop agent, links to project
4. Agent, agent details, stop agent, Links back to Agents team, project  Microsoft.AspNetCore.Diagnostics.

## TODO 2

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

## TODO 3 ✅ COMPLETED

Web App Model twin objects

For ProjectMdel, TeamModel, etc.- we have a reference to SharedLib, why mirror the original model objects with twins? Can we just use the actual model objects? Its less error prone bc when a model object changes then all projects using the SharedLib.Model objects are updated immeditaely w/o having to remeber to update their miorros objects? Is this not a good idea? Do we not to tie the projects together this tightly?

**RESOLUTION:** ✅ Implemented using SharedLib models directly

- Added SharedLib project reference to OrchestratorWebApp
- Updated Projects.razor to use `SharedLib.Model.Project` and `SharedLib.DTOs.CreateProjectRequest`
- Removed duplicate model definitions
- Benefits achieved:
  - Single source of truth for model definitions
  - Automatic synchronization when models change
  - Reduced code duplication and maintenance
  - Type safety guaranteed between API and UI
  - Less error-prone development
  -

**REMAINING:** ✅ Implemented using SharedLib models directly for rest of objects

- only Model.Projects was completed.
- Need to finish the rest of the objects

## TODO 5

**Optimize AI Instruction Modules for Brevity**

Current instruction modules are verbose with conversational style. GitHub recommends shorter custom instructions to avoid them becoming unwieldy.

**Goal:** Streamline instruction files by 50-70% without reducing effectiveness

**Optimization Strategies:**

- Replace conversational tone with imperative commands
- Convert paragraph explanations to bullet points  
- Remove redundant formatting and decorative elements
- Lead with code examples, minimize explanatory text
- Use action verbs: Run, Set, Configure, Test

**Files to Optimize:**

- `ai_instruction_modules/ai-terminal-management.md` (55 lines → ~25 lines)
- `docs/AUTHENTICATION.md` (119 lines → ~60 lines)
- `docs/ENVIRONMENT_CONFIGURATION.md` (57 lines → ~30 lines)
- `docs/README-AUTOMATION.md` (157 lines → ~80 lines)

**Expected Benefits:**

- Faster AI comprehension
- Reduced token consumption
- Clearer action items
- Better GitHub Copilot compliance
