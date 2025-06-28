using OrchestratorWebApp.Models;

namespace OrchestratorWebApp.Services;

/// <summary>
/// Service interface for managing agents
/// </summary>
public interface IAgentService
{
    // Agent CRUD operations
    Task<IEnumerable<Agent>> GetAllAgentsAsync();
    Task<Agent?> GetAgentAsync(string id);
    Task<Agent> CreateOrchestratorAsync(string name, Dictionary<string, object>? configuration = null);
    Task<Agent> CreateCollaboratorAsync(string name, List<string> capabilities, Dictionary<string, object>? configuration = null);
    Task<Agent> UpdateAgentAsync(Agent agent);
    Task<bool> DeleteAgentAsync(string id);
    
    // Agent status and health
    Task<string> GetAgentStatusAsync(string id);
    Task<bool> StartAgentAsync(string id);
    Task<bool> StopAgentAsync(string id);
    Task<bool> RestartAgentAsync(string id);
    
    // Agent filtering and search
    Task<IEnumerable<Agent>> GetAgentsByTypeAsync(AgentType type);
    Task<IEnumerable<Agent>> GetAvailableAgentsAsync();
    Task<IEnumerable<Agent>> SearchAgentsAsync(string searchTerm);
}

/// <summary>
/// Service interface for managing teams
/// </summary>
public interface ITeamService
{
    // Team CRUD operations
    Task<IEnumerable<Team>> GetAllTeamsAsync();
    Task<Team?> GetTeamAsync(string id);
    Task<Team> CreateTeamAsync(string name, string description = "");
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> DeleteTeamAsync(string id);
    
    // Team composition management
    Task<Team> AddAgentToTeamAsync(string teamId, string agentId);
    Task<Team> RemoveAgentFromTeamAsync(string teamId, string agentId);
    Task<bool> ValidateTeamCompositionAsync(string teamId);
    
    // Team status and operations
    Task<TeamStatus> GetTeamStatusAsync(string teamId);
    Task<bool> ActivateTeamAsync(string teamId);
    Task<bool> DeactivateTeamAsync(string teamId);
}

/// <summary>
/// Service interface for managing projects
/// </summary>
public interface IProjectService
{
    // Project CRUD operations
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectAsync(string id);
    Task<Project> CreateProjectAsync(string name, string description = "");
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(string id);
    
    // Project configuration
    Task<Project> AssignRepositoryAsync(string projectId, GitHubRepository repository);
    Task<Project> AssignTeamAsync(string projectId, string teamId);
    Task<Project> SetSpecificationAsync(string projectId, ProjectSpecification specification);
    Task<bool> ValidateProjectConfigurationAsync(string projectId);
    
    // Project lifecycle management
    Task<bool> StartProjectAsync(string projectId);
    Task<bool> StopProjectAsync(string projectId);
    Task<bool> PauseProjectAsync(string projectId);
    Task<bool> ResumeProjectAsync(string projectId);
    Task<ProjectStatus> GetProjectStatusAsync(string projectId);
    
    // Project sessions
    Task<IEnumerable<AgentSession>> GetProjectSessionsAsync(string projectId);
    Task<AgentSession?> GetSessionAsync(string sessionId);
}

/// <summary>
/// Service interface for managing GitHub repositories
/// </summary>
public interface IRepositoryService
{
    // Repository operations
    Task<IEnumerable<GitHubRepository>> GetAllRepositoriesAsync();
    Task<GitHubRepository?> GetRepositoryAsync(string id);
    Task<GitHubRepository> CreateRepositoryAsync(string name, string owner, string url, string branch = "main");
    Task<GitHubRepository> UpdateRepositoryAsync(GitHubRepository repository);
    Task<bool> DeleteRepositoryAsync(string id);
    
    // Repository validation and discovery
    Task<bool> ValidateRepositoryAsync(string url);
    Task<GitHubRepository?> DiscoverRepositoryAsync(string url);
    Task<IEnumerable<string>> GetRepositoryBranchesAsync(string repositoryId);
}

/// <summary>
/// Service interface for managing project specifications
/// </summary>
public interface ISpecificationService
{
    // Specification CRUD operations
    Task<ProjectSpecification?> GetSpecificationAsync(string id);
    Task<ProjectSpecification> CreateSpecificationAsync(string title, string description, string content, SpecificationType type = SpecificationType.Markdown);
    Task<ProjectSpecification> UpdateSpecificationAsync(ProjectSpecification specification);
    Task<bool> DeleteSpecificationAsync(string id);
    
    // Specification validation and processing
    Task<bool> ValidateSpecificationAsync(string id);
    Task<string> ProcessSpecificationAsync(string id);
}

/// <summary>
/// Service interface for managing agent sessions
/// </summary>
public interface ISessionService
{
    // Session monitoring
    Task<IEnumerable<AgentSession>> GetActiveSessionsAsync();
    Task<AgentSession?> GetSessionAsync(string sessionId);
    Task<IEnumerable<AgentSession>> GetAgentSessionsAsync(string agentId);
    Task<IEnumerable<AgentSession>> GetProjectSessionsAsync(string projectId);
    
    // Session control
    Task<bool> StartSessionAsync(string sessionId);
    Task<bool> StopSessionAsync(string sessionId);
    Task<bool> PauseSessionAsync(string sessionId);
    Task<bool> ResumeSessionAsync(string sessionId);
    Task<bool> TerminateSessionAsync(string sessionId);
    
    // Session logs
    Task<IEnumerable<SessionLog>> GetSessionLogsAsync(string sessionId);
    Task<SessionLog> AddSessionLogAsync(string sessionId, Models.LogLevel level, string message, Dictionary<string, object>? data = null);
}
