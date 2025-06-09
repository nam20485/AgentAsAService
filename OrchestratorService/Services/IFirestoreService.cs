using SharedLib.Model;
using SharedLib.DTOs;

namespace OrchestratorService.Services;

public interface IFirestoreService
{
    // Orchestrator operations
    Task<Orchestrator> CreateOrchestratorAsync(CreateOrchestratorRequest request);
    Task<List<Orchestrator>> GetAllOrchestratorsAsync();
    Task<Orchestrator?> GetOrchestratorByNameAsync(string name);

    // Project operations
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(string projectId);
    Task<Project> UpdateProjectAsync(Project project);
}
