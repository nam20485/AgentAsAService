using System.ComponentModel.DataAnnotations;

namespace OrchestratorWebApp.Models;

/// <summary>
/// Enhanced Agent model for UI layer with support for different agent types
/// </summary>
public class Agent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AgentType Type { get; set; } = AgentType.Collaborator;
    public AgentStatus Status { get; set; } = AgentStatus.Inactive;
    public List<string> Capabilities { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string? TeamId { get; set; }
    public string? ProjectId { get; set; }
}

/// <summary>
/// Agent types supported by the system
/// </summary>
public enum AgentType
{
    Orchestrator,
    Collaborator
}

/// <summary>
/// Agent status states
/// </summary>
public enum AgentStatus
{
    Inactive,
    Active,
    Busy,
    Error,
    Offline
}

/// <summary>
/// Enhanced Team model with validation and workflow support
/// </summary>
public class Team
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Agent> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TeamStatus Status { get; set; } = TeamStatus.Inactive;
    
    /// <summary>
    /// Validates team composition (1 orchestrator + N collaborators)
    /// </summary>
    public bool IsValidComposition => 
        Members.Count(m => m.Type == AgentType.Orchestrator) == 1;
    
    /// <summary>
    /// Gets the orchestrator agent for this team
    /// </summary>
    public Agent? Orchestrator => Members.FirstOrDefault(m => m.Type == AgentType.Orchestrator);
    
    /// <summary>
    /// Gets all collaborator agents for this team
    /// </summary>
    public IEnumerable<Agent> Collaborators => Members.Where(m => m.Type == AgentType.Collaborator);
}

/// <summary>
/// Team status states
/// </summary>
public enum TeamStatus
{
    Inactive,
    Ready,
    Active,
    Working
}

/// <summary>
/// Enhanced Project model with configuration state management
/// </summary>
public class Project
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GitHubRepository? Repository { get; set; }
    public Team? Team { get; set; }
    public ProjectSpecification? Specification { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Configuring;
    public List<AgentSession> Sessions { get; set; } = new();
    
    /// <summary>
    /// Checks if project is properly configured and ready to start
    /// </summary>
    public bool IsConfigured => 
        !string.IsNullOrEmpty(Name) &&
        Repository != null && Repository.IsValid &&
        Team != null && Team.IsValidComposition &&
        Specification != null && Specification.IsValid;
    
    /// <summary>
    /// Gets configuration status details
    /// </summary>
    public List<string> ConfigurationIssues
    {
        get
        {
            var issues = new List<string>();
            if (string.IsNullOrEmpty(Name)) issues.Add("Project name is required");
            if (Repository == null || !Repository.IsValid) issues.Add("Valid GitHub repository is required");
            if (Team == null) issues.Add("Team is required");
            else if (!Team.IsValidComposition) issues.Add("Team must have exactly 1 orchestrator and 0+ collaborators");
            if (Specification == null || !Specification.IsValid) issues.Add("Valid project specification is required");
            return issues;
        }
    }
}

/// <summary>
/// Project status states following the workflow
/// </summary>
public enum ProjectStatus
{
    Configuring,
    Configured,
    Ready,
    Running,
    Paused,
    Completed,
    Failed
}

/// <summary>
/// GitHub repository model (simplified for initial implementation)
/// </summary>
public class GitHubRepository
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Validates that this is a valid GitHub repository
    /// </summary>
    public bool IsValid => 
        !string.IsNullOrEmpty(Name) &&
        !string.IsNullOrEmpty(Owner) &&
        !string.IsNullOrEmpty(Url) &&
        Uri.IsWellFormedUriString(Url, UriKind.Absolute) &&
        Uri.TryCreate(Url, UriKind.Absolute, out var uri) &&
        (uri.Host == "github.com" || uri.Host.EndsWith(".github.com"));
}

/// <summary>
/// Project specification model
/// </summary>
public class ProjectSpecification
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public SpecificationType Type { get; set; } = SpecificationType.Markdown;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Enhanced properties for UI workflow
    public List<string> RequiredCapabilities { get; set; } = new();
    public string EstimatedComplexity { get; set; } = "Medium";
    public string Priority { get; set; } = "Medium";
    
    /// <summary>
    /// Validates that specification has required content
    /// </summary>
    public bool IsValid => 
        !string.IsNullOrEmpty(Title) &&
        !string.IsNullOrEmpty(Description) &&
        !string.IsNullOrEmpty(Content);
}

/// <summary>
/// Specification types supported
/// </summary>
public enum SpecificationType
{
    Markdown,
    PlainText,
    Structured,
    Feature,
    Enhancement,
    BugFix
}

/// <summary>
/// Agent session model for tracking work
/// </summary>
public class AgentSession
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string SessionType { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Starting;
    public List<SessionLog> Logs { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Enhanced properties for UI workflow
    public DateTime StartTime => StartedAt;
    public string Activity { get; set; } = string.Empty;
    public int Progress { get; set; } = 0;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Session status states
/// </summary>
public enum SessionStatus
{
    Starting,
    Running,
    Paused,
    Completed,
    Failed,
    Terminated
}

/// <summary>
/// Session log entry
/// </summary>
public class SessionLog
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; } = LogLevel.Info;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Log levels for session logging
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
