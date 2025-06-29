using System.Net.Http.Json;
using System.Text.Json;
using OrchestratorWebApp.Models;

namespace OrchestratorWebApp.Services;

/// <summary>
/// Base API client service with common functionality
/// </summary>
public abstract class BaseApiService
{
    protected readonly HttpClient _httpClient;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected BaseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    protected async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            return default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    protected async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

/// <summary>
/// Agent service implementation using OrchestratorService API
/// </summary>
public class AgentApiService : BaseApiService, IAgentService
{
    public AgentApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<IEnumerable<Agent>> GetAllAgentsAsync()
    {
        // For now, return mock data until API endpoints are implemented
        return await Task.FromResult(new List<Agent>
        {
            new Agent 
            { 
                Id = "agent-1", 
                Name = "UI Designer Agent", 
                Type = AgentType.Collaborator,
                Status = "Active", 
                Capabilities = new List<string> { "UI Design", "Figma", "Prototyping" },
                LastActivity = DateTime.UtcNow.AddMinutes(-15)
            },
            new Agent 
            { 
                Id = "orchestrator-1", 
                Name = "Main Orchestrator", 
                Type = AgentType.Orchestrator,
                Status = "Active", 
                Capabilities = new List<string> { "Project Management", "Task Planning", "Team Coordination" },
                LastActivity = DateTime.UtcNow.AddMinutes(-2)
            }
        });
    }

    public async Task<Agent?> GetAgentAsync(string id)
    {
        var agents = await GetAllAgentsAsync();
        return agents.FirstOrDefault(a => a.Id == id);
    }

    public async Task<Agent> CreateOrchestratorAsync(string name, Dictionary<string, object>? configuration = null)
    {
        var agent = new Agent
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = AgentType.Orchestrator,
            Status = "Inactive",
            Capabilities = new List<string> { "Project Management", "Task Planning", "Team Coordination" },
            Configuration = configuration ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        // TODO: Call API to create orchestrator
        // var result = await PostAsync<Agent>("/api/orchestrators", agent);
        // return result ?? agent;

        return await Task.FromResult(agent);
    }

    public async Task<Agent> CreateCollaboratorAsync(string name, List<string> capabilities, Dictionary<string, object>? configuration = null)
    {
        var agent = new Agent
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = AgentType.Collaborator,
            Status = "Inactive",
            Capabilities = capabilities,
            Configuration = configuration ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        // TODO: Call API to create collaborator
        // var result = await PostAsync<Agent>("/api/agents", agent);
        // return result ?? agent;

        return await Task.FromResult(agent);
    }

    public async Task<Agent> UpdateAgentAsync(Agent agent)
    {
        // TODO: Implement API call
        return await Task.FromResult(agent);
    }

    public async Task<bool> DeleteAgentAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<string> GetAgentStatusAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult("Active");
    }

    public async Task<bool> StartAgentAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> StopAgentAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> RestartAgentAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<Agent>> GetAgentsByTypeAsync(AgentType type)
    {
        var agents = await GetAllAgentsAsync();
        return agents.Where(a => a.Type == type);
    }

    public async Task<IEnumerable<Agent>> GetAvailableAgentsAsync()
    {
        var agents = await GetAllAgentsAsync();
        return agents.Where(a => string.IsNullOrEmpty(a.TeamId));
    }

    public async Task<IEnumerable<Agent>> SearchAgentsAsync(string searchTerm)
    {
        var agents = await GetAllAgentsAsync();
        return agents.Where(a => 
            a.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Capabilities.Any(c => c.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
    }
}

/// <summary>
/// Team service implementation using OrchestratorService API
/// </summary>
public class TeamApiService : BaseApiService, ITeamService
{
    public TeamApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<IEnumerable<Team>> GetAllTeamsAsync()
    {
        // Mock data for now
        return await Task.FromResult(new List<Team>
        {
            new Team 
            { 
                Id = "team-1", 
                Name = "Frontend Team", 
                Description = "UI/UX Development Team",
                Status = TeamStatus.Ready,
                Members = new List<Agent>()
            },
            new Team 
            { 
                Id = "team-2", 
                Name = "Backend Team", 
                Description = "API and Database Team",
                Status = TeamStatus.Active,
                Members = new List<Agent>()
            }
        });
    }

    public async Task<Team?> GetTeamAsync(string id)
    {
        var teams = await GetAllTeamsAsync();
        return teams.FirstOrDefault(t => t.Id == id);
    }

    public async Task<Team> CreateTeamAsync(string name, string description = "")
    {
        var team = new Team
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Status = TeamStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };

        // TODO: Implement API call
        return await Task.FromResult(team);
    }

    public async Task<Team> UpdateTeamAsync(Team team)
    {
        // TODO: Implement API call
        return await Task.FromResult(team);
    }

    public async Task<bool> DeleteTeamAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<Team> AddAgentToTeamAsync(string teamId, string agentId)
    {
        // TODO: Implement API call
        var team = await GetTeamAsync(teamId);
        return team ?? new Team();
    }

    public async Task<Team> RemoveAgentFromTeamAsync(string teamId, string agentId)
    {
        // TODO: Implement API call
        var team = await GetTeamAsync(teamId);
        return team ?? new Team();
    }

    public async Task<bool> ValidateTeamCompositionAsync(string teamId)
    {
        var team = await GetTeamAsync(teamId);
        return team?.IsValidComposition ?? false;
    }

    public async Task<TeamStatus> GetTeamStatusAsync(string teamId)
    {
        var team = await GetTeamAsync(teamId);
        return team?.Status ?? TeamStatus.Inactive;
    }

    public async Task<bool> ActivateTeamAsync(string teamId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> DeactivateTeamAsync(string teamId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }
}

/// <summary>
/// Project service implementation using OrchestratorService API
/// </summary>
public class ProjectApiService : BaseApiService, IProjectService
{
    public ProjectApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        try
        {
            // Use existing API endpoint
            var response = await _httpClient.GetStringAsync("/api/projects");
            var apiProjects = JsonSerializer.Deserialize<List<SharedLib.Model.Project>>(response, _jsonOptions);
            
            // Convert to our enhanced model
            return apiProjects?.Select(p => new Project
            {
                Id = p.Id,
                Name = p.Name,
                Description = "",
                Repository = p.Repository != null ? new GitHubRepository
                {
                    Id = p.Repository.Id,
                    Name = p.Repository.Name,
                    Url = p.Repository.Address,
                    Owner = ExtractOwnerFromUrl(p.Repository.Address)
                } : null,
                CreatedAt = p.CreatedAt,
                Status = ProjectStatus.Configured
            }) ?? new List<Project>();
        }
        catch (Exception)
        {
            return new List<Project>();
        }
    }

    public async Task<Project?> GetProjectAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"/api/projects/{id}");
            var apiProject = JsonSerializer.Deserialize<SharedLib.Model.Project>(response, _jsonOptions);
            
            if (apiProject == null) return null;
            
            return new Project
            {
                Id = apiProject.Id,
                Name = apiProject.Name,
                Description = "",
                Repository = apiProject.Repository != null ? new GitHubRepository
                {
                    Id = apiProject.Repository.Id,
                    Name = apiProject.Repository.Name,
                    Url = apiProject.Repository.Address,
                    Owner = ExtractOwnerFromUrl(apiProject.Repository.Address)
                } : null,
                CreatedAt = apiProject.CreatedAt,
                Status = ProjectStatus.Configured
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Project> CreateProjectAsync(string name, string description = "")
    {
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Status = ProjectStatus.Configuring,
            CreatedAt = DateTime.UtcNow
        };

        // TODO: Implement enhanced project creation API
        return await Task.FromResult(project);
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        // TODO: Implement API call
        return await Task.FromResult(project);
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<Project> AssignRepositoryAsync(string projectId, GitHubRepository repository)
    {
        // TODO: Implement API call
        var project = await GetProjectAsync(projectId);
        if (project != null)
        {
            project.Repository = repository;
        }
        return project ?? new Project();
    }

    public async Task<Project> AssignTeamAsync(string projectId, string teamId)
    {
        // TODO: Implement API call
        var project = await GetProjectAsync(projectId);
        return project ?? new Project();
    }

    public async Task<Project> SetSpecificationAsync(string projectId, ProjectSpecification specification)
    {
        // TODO: Implement API call
        var project = await GetProjectAsync(projectId);
        if (project != null)
        {
            project.Specification = specification;
        }
        return project ?? new Project();
    }

    public async Task<bool> ValidateProjectConfigurationAsync(string projectId)
    {
        var project = await GetProjectAsync(projectId);
        return project?.IsConfigured ?? false;
    }

    public async Task<bool> StartProjectAsync(string projectId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> StopProjectAsync(string projectId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> PauseProjectAsync(string projectId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> ResumeProjectAsync(string projectId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<ProjectStatus> GetProjectStatusAsync(string projectId)
    {
        var project = await GetProjectAsync(projectId);
        return project?.Status ?? ProjectStatus.Configuring;
    }

    public async Task<IEnumerable<AgentSession>> GetProjectSessionsAsync(string projectId)
    {
        // TODO: Implement API call
        return await Task.FromResult(new List<AgentSession>());
    }

    public async Task<AgentSession?> GetSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult<AgentSession?>(null);
    }

    private static string ExtractOwnerFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return "";
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments[0] : "";
        }
        catch
        {
            return "";
        }
    }
}
