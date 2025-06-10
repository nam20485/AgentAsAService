using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLib.Model;
using SharedLib.DTOs;

namespace SharedLib.Abstractions.Stores;

/// <summary>
/// Domain store for AgentSession entities with validation and business logic.
/// Provides high-level operations for AgentSession management.
/// </summary>
public interface IAgentSessionStore
{
    /// <summary>
    /// Save an agent session (create or update) with validation
    /// </summary>
    /// <param name="session">Agent session to save</param>
    /// <returns>The saved agent session</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<AgentSession> SaveAsync(AgentSession session);    /// <summary>
    /// Create an agent session from a request DTO with validation
    /// </summary>
    /// <param name="request">Create agent session request</param>
    /// <returns>The created agent session</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<AgentSession> CreateAsync(CreateAgentSessionRequest request);

    /// <summary>
    /// Create an agent session with validation
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="createdBy">Creator identifier (email, user ID, etc.)</param>
    /// <param name="branch">Optional repository branch</param>
    /// <param name="configuration">Optional configuration dictionary</param>
    /// <returns>The created agent session</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    Task<AgentSession> CreateAsync(string repositoryUrl, string? createdBy = null, string? branch = null, Dictionary<string, object>? configuration = null);

    /// <summary>
    /// Get an agent session by its ID
    /// </summary>
    /// <param name="id">Session identifier</param>
    /// <returns>The agent session if found, null otherwise</returns>
    Task<AgentSession?> GetByIdAsync(string id);

    /// <summary>
    /// Get all agent sessions
    /// </summary>
    /// <returns>All agent sessions</returns>
    Task<IEnumerable<AgentSession>> GetAllAsync();

    /// <summary>
    /// Find sessions by repository URL
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <returns>Sessions for the repository</returns>
    Task<IEnumerable<AgentSession>> FindByRepositoryAsync(string repositoryUrl);

    /// <summary>
    /// Find sessions by creator
    /// </summary>
    /// <param name="createdBy">Creator identifier</param>
    /// <returns>Sessions created by the user</returns>
    Task<IEnumerable<AgentSession>> FindByCreatorAsync(string createdBy);

    /// <summary>
    /// Find sessions by status
    /// </summary>
    /// <param name="status">Session status</param>
    /// <returns>Sessions with the specified status</returns>
    Task<IEnumerable<AgentSession>> FindByStatusAsync(string status);

    /// <summary>
    /// Update session status
    /// </summary>
    /// <param name="id">Session identifier</param>
    /// <param name="status">New status</param>
    /// <returns>The updated session, or null if not found</returns>
    Task<AgentSession?> UpdateStatusAsync(string id, string status);

    /// <summary>
    /// Delete an agent session by ID
    /// </summary>
    /// <param name="id">Session identifier</param>
    /// <returns>True if the session was deleted, false if not found</returns>
    Task<bool> DeleteAsync(string id);    /// <summary>
    /// Check if an agent session exists with the specified ID
    /// </summary>
    /// <param name="id">Session identifier</param>
    /// <returns>True if the session exists</returns>
    Task<bool> ExistsAsync(string id);
}
