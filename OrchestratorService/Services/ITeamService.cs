using SharedLib.Model;
using SharedLib.DTOs;
using SharedLib.Abstractions.Stores;

namespace OrchestratorService.Services;

public interface ITeamService
{
    // Team operations
    Task<Project> AddAgentToTeamAsync(string projectId, AddAgentToTeamRequest request);
    Task<List<Collaborator>> GetTeamMembersAsync(string projectId);
    Task<bool> RemoveAgentFromTeamAsync(string projectId, string agentId);
    Task<Team?> GetTeamByProjectIdAsync(string projectId);
}

public class TeamService : ITeamService
{
    private readonly IProjectStore _projectStore;
    private readonly ILogger<TeamService> _logger;

    public TeamService(IProjectStore projectStore, ILogger<TeamService> logger)
    {
        _projectStore = projectStore ?? throw new ArgumentNullException(nameof(projectStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Project> AddAgentToTeamAsync(string projectId, AddAgentToTeamRequest request)
    {
        _logger.LogInformation("Adding agent '{AgentName}' to team in project '{ProjectId}'", 
            request.AgentName, projectId);

        // Validate input
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));
        }

        if (string.IsNullOrEmpty(request.AgentName))
        {
            throw new ArgumentException("Agent name cannot be empty", nameof(request));
        }

        // Get project
        var project = await _projectStore.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        // Create a new agent
        var agent = new Collaborator(request.AgentName)
        {
            CreatedAt = DateTime.UtcNow
        };

        // Add agent to team
        project.Team.Members.Add(agent);
        project.UpdatedAt = DateTime.UtcNow;

        // Save project
        var updatedProject = await _projectStore.SaveAsync(project);
        _logger.LogInformation("Added agent '{AgentName}' with ID '{AgentId}' to team in project '{ProjectId}'", 
            agent.Name, agent.Id, projectId);

        return updatedProject;
    }

    public async Task<List<Collaborator>> GetTeamMembersAsync(string projectId)
    {
        _logger.LogInformation("Getting team members for project '{ProjectId}'", projectId);

        // Validate input
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));
        }

        // Get project
        var project = await _projectStore.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        return project.Team.Members.ToList();
    }

    public async Task<bool> RemoveAgentFromTeamAsync(string projectId, string agentId)
    {
        _logger.LogInformation("Removing agent '{AgentId}' from team in project '{ProjectId}'", 
            agentId, projectId);

        // Validate input
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));
        }

        if (string.IsNullOrEmpty(agentId))
        {
            throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
        }

        // Get project
        var project = await _projectStore.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID '{projectId}' not found");
        }

        // Find and remove agent
        var agent = project.Team.Members.FirstOrDefault(a => a.Id == agentId);
        if (agent == null)
        {
            _logger.LogWarning("Agent '{AgentId}' not found in team for project '{ProjectId}'", 
                agentId, projectId);
            return false;
        }

        // Remove agent from team
        bool removed = project.Team.Members.Remove(agent);
        if (removed)
        {
            project.UpdatedAt = DateTime.UtcNow;
            await _projectStore.SaveAsync(project);
            _logger.LogInformation("Agent '{AgentId}' removed from team in project '{ProjectId}'", 
                agentId, projectId);
        }

        return removed;
    }

    public async Task<Team?> GetTeamByProjectIdAsync(string projectId)
    {
        _logger.LogInformation("Getting team for project '{ProjectId}'", projectId);

        // Validate input
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));
        }

        // Get project
        var project = await _projectStore.GetByIdAsync(projectId);
        if (project == null)
        {
            _logger.LogWarning("Project '{ProjectId}' not found when retrieving team", projectId);
            return null;
        }

        return project.Team;
    }
}
