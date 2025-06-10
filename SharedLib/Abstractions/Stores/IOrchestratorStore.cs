using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib.Model;
using SharedLib.DTOs;

namespace SharedLib.Abstractions.Stores;

/// <summary>
/// Domain store for Orchestrator entities with validation and business logic.
/// Provides high-level operations for Orchestrator management.
/// </summary>
public interface IOrchestratorStore
{
    /// <summary>
    /// Save an orchestrator (create or update) with validation
    /// </summary>
    /// <param name="orchestrator">Orchestrator to save</param>
    /// <returns>The saved orchestrator</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Orchestrator> SaveAsync(Orchestrator orchestrator);

    /// <summary>
    /// Create an orchestrator from a request with validation
    /// </summary>
    /// <param name="request">Create orchestrator request</param>
    /// <returns>The created orchestrator</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<Orchestrator> CreateAsync(CreateOrchestratorRequest request);

    /// <summary>
    /// Get an orchestrator by its ID
    /// </summary>
    /// <param name="id">Orchestrator identifier</param>
    /// <returns>The orchestrator if found, null otherwise</returns>
    Task<Orchestrator?> GetByIdAsync(string id);

    /// <summary>
    /// Get all orchestrators
    /// </summary>
    /// <returns>All orchestrators</returns>
    Task<IEnumerable<Orchestrator>> GetAllAsync();

    /// <summary>
    /// Find orchestrators by name (case-insensitive contains)
    /// </summary>
    /// <param name="name">Orchestrator name or partial name</param>
    /// <returns>Orchestrators matching the name criteria</returns>
    Task<IEnumerable<Orchestrator>> FindByNameAsync(string name);

    /// <summary>
    /// Delete an orchestrator by ID
    /// </summary>
    /// <param name="id">Orchestrator identifier</param>
    /// <returns>True if the orchestrator was deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Check if an orchestrator exists with the specified ID
    /// </summary>
    /// <param name="id">Orchestrator identifier</param>
    /// <returns>True if the orchestrator exists</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Check if an orchestrator exists with the specified name
    /// </summary>
    /// <param name="name">Orchestrator name</param>
    /// <returns>True if an orchestrator with this name exists</returns>
    Task<bool> ExistsByNameAsync(string name);
}
