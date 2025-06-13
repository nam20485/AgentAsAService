using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib.Model;

namespace SharedLib.Abstractions.Stores;

/// <summary>
/// Domain store for Project entities with validation and business logic.
/// Provides high-level operations for Project management.
/// </summary>
public interface IProjectStore
{
    /// <summary>
    /// Save a project (create or update) with validation
    /// </summary>
    /// <param name="project">Project to save</param>
    /// <returns>The saved project</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Project> SaveAsync(Project project);

    /// <summary>
    /// Get a project by its ID
    /// </summary>
    /// <param name="id">Project identifier</param>
    /// <returns>The project if found, null otherwise</returns>
    Task<Project?> GetByIdAsync(string id);

    /// <summary>
    /// Get all projects
    /// </summary>
    /// <returns>All projects</returns>
    Task<IEnumerable<Project>> GetAllAsync();

    /// <summary>
    /// Find projects by orchestrator ID
    /// </summary>
    /// <param name="orchestratorId">Orchestrator identifier</param>
    /// <returns>Projects belonging to the orchestrator</returns>
    Task<IEnumerable<Project>> FindByOrchestratorAsync(string orchestratorId);

    /// <summary>
    /// Find projects by name (case-insensitive contains)
    /// </summary>
    /// <param name="name">Project name or partial name</param>
    /// <returns>Projects matching the name criteria</returns>
    Task<IEnumerable<Project>> FindByNameAsync(string name);

    /// <summary>
    /// Delete a project by ID
    /// </summary>
    /// <param name="id">Project identifier</param>
    /// <returns>True if the project was deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Check if a project exists with the specified ID
    /// </summary>
    /// <param name="id">Project identifier</param>
    /// <returns>True if the project exists</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Check if a project exists with the specified name
    /// </summary>
    /// <param name="name">Project name</param>
    /// <returns>True if a project with this name exists</returns>
    Task<bool> ExistsByNameAsync(string name);
}
