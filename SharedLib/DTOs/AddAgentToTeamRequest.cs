using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLib.DTOs;

public class AddAgentToTeamRequest
{
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string AgentName { get; set; } = string.Empty;
}

public class AgentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
