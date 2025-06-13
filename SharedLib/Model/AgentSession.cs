using System;
using System.Collections.Generic;

namespace SharedLib.Model;

/// <summary>
/// Represents an agent session for repository-based AI agent interactions
/// </summary>
public class AgentSession
{
    /// <summary>
    /// Unique identifier for the session
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Repository URL associated with this session
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional repository branch (defaults to main/master if not specified)
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// User or service that created the session
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the session was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Current status of the session (Created, Active, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = "Created";

    /// <summary>
    /// Optional configuration parameters for the session
    /// </summary>
    public Dictionary<string, object>? Configuration { get; set; }    /// <summary>
    /// When the session was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional error message if session failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
