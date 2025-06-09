using System;
using Microsoft.AspNetCore.Mvc;
using SharedLib.DTOs;
using SharedLib.Model;
using OrchestratorService.Services;

namespace OrchestratorService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamController> _logger;

    public TeamController(ITeamService teamService, ILogger<TeamController> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }

    /// <summary>Add a new agent to a project team</summary>
    [HttpPost("projects/{projectId}/agents")]
    public async Task<IActionResult> AddAgentToTeam(string projectId, [FromBody] AddAgentToTeamRequest request)
    {
        try
        {
            // Ensure projectId matches
            request.ProjectId = projectId;
            
            var updatedProject = await _teamService.AddAgentToTeamAsync(projectId, request);
            
            var response = new ProjectResponse
            {
                Id = updatedProject.Id,
                Name = updatedProject.Name,
                OrchestratorId = updatedProject.OrchestratorId,
                Repository = new RepositoryInfo
                {
                    Id = updatedProject.Repository?.Id ?? "",
                    Name = updatedProject.Repository?.Name ?? "",
                    Address = updatedProject.Repository?.Address ?? ""
                },
                Team = new TeamInfo
                {
                    Id = updatedProject.Team.Id,
                    Name = updatedProject.Team.Name,
                    MemberCount = updatedProject.Team.Members.Count
                },
                CreatedAt = updatedProject.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding agent to team");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get all team members for a project</summary>
    [HttpGet("projects/{projectId}/agents")]
    public async Task<IActionResult> GetTeamMembers(string projectId)
    {
        try
        {
            var members = await _teamService.GetTeamMembersAsync(projectId);
            
            var responses = members.Select(m => new AgentResponse
            {
                Id = m.Id,
                Name = m.Name,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team members");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Remove an agent from a project team</summary>
    [HttpDelete("projects/{projectId}/agents/{agentId}")]
    public async Task<IActionResult> RemoveAgentFromTeam(string projectId, string agentId)
    {
        try
        {
            var success = await _teamService.RemoveAgentFromTeamAsync(projectId, agentId);
            
            if (!success)
            {
                return NotFound(new { error = "Agent not found in team" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing agent from team");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get team information for a project</summary>
    [HttpGet("projects/{projectId}")]
    public async Task<IActionResult> GetTeam(string projectId)
    {
        try
        {
            var team = await _teamService.GetTeamByProjectIdAsync(projectId);
            
            if (team == null)
            {
                return NotFound(new { error = "Team not found" });
            }

            var response = new TeamInfo
            {
                Id = team.Id,
                Name = team.Name,
                MemberCount = team.Members.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team");
            return BadRequest(new { error = ex.Message });
        }
    }
}
