using System;

namespace SharedLib.Model;

public class Project
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string OrchestratorId { get; set; } = string.Empty;
    public Repository Repository { get; set; } = new Repository();
    public Team Team { get; set; } = new Team();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ProjectStatus Status { get; set; } = ProjectStatus.Unconfigured;
}
