using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLib.Abstractions;
using SharedLib.Abstractions.Stores;
using SharedLib.Model;
using SharedLib.DTOs;

namespace SharedLib.Stores;

/// <summary>
/// Domain store for AgentSession entities with validation and business logic
/// </summary>
public class AgentSessionStore : IAgentSessionStore
{
    private readonly IDocumentRepository<AgentSession> _repository;

    public AgentSessionStore(IDocumentRepository<AgentSession> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<AgentSession> SaveAsync(AgentSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // Validate the session
        ValidateAgentSession(session);

        // Set ID if not provided
        if (string.IsNullOrEmpty(session.Id))
            session.Id = Guid.NewGuid().ToString();

        // Set timestamps
        if (session.CreatedAt == default)
            session.CreatedAt = DateTime.UtcNow;
        
        session.UpdatedAt = DateTime.UtcNow;

        // Set default status if not provided
        if (string.IsNullOrEmpty(session.Status))
            session.Status = "Created";

        return await _repository.UpsertAsync(session.Id, session);
    }

    public async Task<AgentSession> CreateAsync(string repositoryUrl, string? createdBy = null, string? branch = null, Dictionary<string, object>? configuration = null)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            throw new ArgumentException("Repository URL is required", nameof(repositoryUrl));

        var session = new AgentSession
        {
            Id = Guid.NewGuid().ToString(),
            RepositoryUrl = repositoryUrl.Trim(),
            Branch = branch?.Trim(),
            CreatedBy = createdBy?.Trim(),
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Configuration = configuration
        };

        // Validate before saving
        ValidateAgentSession(session);

        return await _repository.UpsertAsync(session.Id, session);
    }

    public async Task<AgentSession> CreateAsync(CreateAgentSessionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await CreateAsync(request.RepositoryUrl, request.CreatedBy, request.Branch, request.Configuration);
    }

    public async Task<AgentSession?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return await _repository.GetAsync(id);
    }

    public async Task<IEnumerable<AgentSession>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<AgentSession>> FindByRepositoryAsync(string repositoryUrl)
    {
        if (string.IsNullOrEmpty(repositoryUrl))
            return Enumerable.Empty<AgentSession>();

        var searchUrl = repositoryUrl.Trim().ToLowerInvariant();
        return await _repository.QueryAsync(s => s.RepositoryUrl.ToLowerInvariant() == searchUrl);
    }

    public async Task<IEnumerable<AgentSession>> FindByCreatorAsync(string createdBy)
    {
        if (string.IsNullOrEmpty(createdBy))
            return Enumerable.Empty<AgentSession>();

        var searchCreator = createdBy.Trim().ToLowerInvariant();
        return await _repository.QueryAsync(s => s.CreatedBy != null && s.CreatedBy.ToLowerInvariant() == searchCreator);
    }

    public async Task<IEnumerable<AgentSession>> FindByStatusAsync(string status)
    {
        if (string.IsNullOrEmpty(status))
            return Enumerable.Empty<AgentSession>();

        var searchStatus = status.Trim().ToLowerInvariant();
        return await _repository.QueryAsync(s => s.Status.ToLowerInvariant() == searchStatus);
    }

    public async Task<AgentSession?> UpdateStatusAsync(string id, string status)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(status))
            return null;

        var session = await _repository.GetAsync(id);
        if (session == null)
            return null;

        // Validate status
        ValidateStatus(status);

        session.Status = status.Trim();
        session.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpsertAsync(session.Id, session);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return await _repository.ExistsAsync(id);
    }

    private static void ValidateAgentSession(AgentSession session)
    {
        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(session.RepositoryUrl))
            errors.Add("Repository URL is required and cannot be empty or whitespace");

        // Validate repository URL format
        if (!string.IsNullOrEmpty(session.RepositoryUrl) && 
            !Uri.TryCreate(session.RepositoryUrl, UriKind.Absolute, out var uri))
            errors.Add("Repository URL must be a valid absolute URL");

        // Validate repository URL is HTTP/HTTPS
        if (!string.IsNullOrEmpty(session.RepositoryUrl) && 
            Uri.TryCreate(session.RepositoryUrl, UriKind.Absolute, out var repoUri) && 
            !new[] { "http", "https" }.Contains(repoUri.Scheme.ToLowerInvariant()))
            errors.Add("Repository URL must use HTTP or HTTPS protocol");

        // Validate status
        if (!string.IsNullOrEmpty(session.Status))
            ValidateStatus(session.Status, errors);

        // Validate branch name if provided
        if (!string.IsNullOrEmpty(session.Branch))
        {
            if (session.Branch.Length > 255)
                errors.Add("Branch name cannot exceed 255 characters");

            // Basic validation for git branch name format
            if (session.Branch.StartsWith('/') || session.Branch.EndsWith('/') ||
                session.Branch.Contains("//") || session.Branch.Contains(' '))
                errors.Add("Branch name format is invalid");
        }

        // Validate CreatedBy if provided
        if (!string.IsNullOrEmpty(session.CreatedBy) && session.CreatedBy.Length > 255)
            errors.Add("CreatedBy cannot exceed 255 characters");

        if (errors.Any())
        {
            throw new ArgumentException($"Agent session validation failed: {string.Join("; ", errors)}");
        }
    }

    private static void ValidateStatus(string status, List<string>? errors = null)
    {
        errors ??= new List<string>();

        var validStatuses = new[] { "Created", "Running", "Completed", "Failed", "Cancelled", "Paused" };
        
        if (!validStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Status must be one of: {string.Join(", ", validStatuses)}");
            
            if (errors.Count == 1) // Only throw if this is the only validation
                throw new ArgumentException($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");
        }
    }
}
