using SharedLib.Model;
using SharedLib.DTOs;

namespace OrchestratorService.Services;

public interface ITeamService
{
    // Team operations
    Task<Project> AddAgentToTeamAsync(string projectId, AddAgentToTeamRequest request);
    Task<List<Collaborator>> GetTeamMembersAsync(string projectId);
    Task<bool> RemoveAgentFromTeamAsync(string projectId, string agentId);
    Task<Team?> GetTeamByProjectIdAsync(string projectId);
}
