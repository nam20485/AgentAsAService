using System.Collections.Generic;

namespace SharedLib.DTOs;

/// <summary>
/// Request model for creating a new agent session
/// </summary>
public class CreateAgentSessionRequest
{
    /// <summary>
    /// Repository URL for the session
    /// </summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>
    /// Optional repository branch
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Optional configuration parameters
    /// </summary>
    public Dictionary<string, object>? Configuration { get; set; }

    /// <summary>
    /// User or service creating the session
    /// </summary>
    public string? CreatedBy { get; set; }
}
