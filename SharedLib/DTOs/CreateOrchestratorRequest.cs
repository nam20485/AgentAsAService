using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLib.DTOs;

public class CreateOrchestratorRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}

public class OrchestratorResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
