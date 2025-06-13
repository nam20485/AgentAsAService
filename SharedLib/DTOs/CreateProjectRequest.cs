using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLib.DTOs;

public class CreateProjectRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string RepositoryName { get; set; } = string.Empty;

    [Required]
    [Url]
    public string RepositoryAddress { get; set; } = string.Empty;

    [Required]
    public string OrchestratorName { get; set; } = string.Empty;
}

public class ProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OrchestratorId { get; set; } = string.Empty;
    public RepositoryInfo Repository { get; set; } = new RepositoryInfo();
    public TeamInfo Team { get; set; } = new TeamInfo();
    public DateTime CreatedAt { get; set; }
}

public class RepositoryInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class TeamInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
}
