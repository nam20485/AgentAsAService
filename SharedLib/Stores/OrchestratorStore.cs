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
/// Domain store for Orchestrator entities with validation and business logic
/// </summary>
public class OrchestratorStore : IOrchestratorStore
{
    private readonly IDocumentRepository<Orchestrator> _repository;

    public OrchestratorStore(IDocumentRepository<Orchestrator> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Orchestrator> SaveAsync(Orchestrator orchestrator)
    {
        if (orchestrator == null)
            throw new ArgumentNullException(nameof(orchestrator));

        // Validate the orchestrator
        ValidateOrchestrator(orchestrator);

        // Set ID if not provided
        if (string.IsNullOrEmpty(orchestrator.Id))
            orchestrator.Id = Guid.NewGuid().ToString();

        // Set timestamps
        if (orchestrator.CreatedAt == default)
            orchestrator.CreatedAt = DateTime.UtcNow;

        return await _repository.UpsertAsync(orchestrator.Id, orchestrator);
    }

    public async Task<Orchestrator> CreateAsync(CreateOrchestratorRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Validate the request
        ValidateCreateRequest(request);

        // Check if name already exists
        if (await ExistsByNameAsync(request.Name))
            throw new ArgumentException($"An orchestrator with the name '{request.Name}' already exists");

        var orchestrator = new Orchestrator(request.Name)
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.UpsertAsync(orchestrator.Id, orchestrator);
    }

    public async Task<Orchestrator?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return await _repository.GetAsync(id);
    }

    public async Task<IEnumerable<Orchestrator>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Orchestrator>> FindByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return Enumerable.Empty<Orchestrator>();

        var searchTerm = name.ToLowerInvariant();
        return await _repository.QueryAsync(o => o.Name.ToLowerInvariant().Contains(searchTerm));
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

    public async Task<bool> ExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        var orchestrators = await FindByNameAsync(name);
        return orchestrators.Any(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidateOrchestrator(Orchestrator orchestrator)
    {
        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(orchestrator.Name))
            errors.Add("Orchestrator name is required and cannot be empty or whitespace");

        if (orchestrator.Name?.Length > 100)
            errors.Add("Orchestrator name cannot exceed 100 characters");

        // Validate name format (alphanumeric, spaces, hyphens, underscores only)
        if (!string.IsNullOrEmpty(orchestrator.Name) && 
            !System.Text.RegularExpressions.Regex.IsMatch(orchestrator.Name, @"^[a-zA-Z0-9\s\-_]+$"))
            errors.Add("Orchestrator name can only contain letters, numbers, spaces, hyphens, and underscores");

        if (errors.Any())
        {
            throw new ArgumentException($"Orchestrator validation failed: {string.Join("; ", errors)}");
        }
    }

    private static void ValidateCreateRequest(CreateOrchestratorRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Orchestrator name is required and cannot be empty or whitespace");

        if (request.Name?.Length > 100)
            errors.Add("Orchestrator name cannot exceed 100 characters");

        if (request.Name?.Length < 1)
            errors.Add("Orchestrator name must be at least 1 character long");

        // Validate name format
        if (!string.IsNullOrEmpty(request.Name) && 
            !System.Text.RegularExpressions.Regex.IsMatch(request.Name, @"^[a-zA-Z0-9\s\-_]+$"))
            errors.Add("Orchestrator name can only contain letters, numbers, spaces, hyphens, and underscores");

        if (errors.Any())
        {
            throw new ArgumentException($"Create orchestrator request validation failed: {string.Join("; ", errors)}");
        }
    }
}
