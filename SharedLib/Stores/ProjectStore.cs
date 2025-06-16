using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLib.Abstractions;
using SharedLib.Abstractions.Stores;
using SharedLib.Model;

namespace SharedLib.Stores;

/// <summary>
/// Domain store for Project entities with validation and business logic
/// </summary>
public class ProjectStore : IProjectStore
{
    private readonly IDocumentRepository<Project> _repository;

    public ProjectStore(IDocumentRepository<Project> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Project> SaveAsync(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        // Validate the project
        ValidateProject(project);

        // Set ID if not provided
        if (string.IsNullOrEmpty(project.Id))
            project.Id = Guid.NewGuid().ToString();

        // Set timestamps
        if (project.CreatedAt == default)
            project.CreatedAt = DateTime.UtcNow;
        
        project.UpdatedAt = DateTime.UtcNow;

        // Initialize team if not provided
        if (project.Team == null)
        {
            project.Team = new Team
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{project.Name} Team"
            };
        }

        return await _repository.UpsertAsync(project.Id, project);
    }

    public async Task<Project?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return await _repository.GetAsync(id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Project>> FindByOrchestratorAsync(string orchestratorId)
    {
        if (string.IsNullOrEmpty(orchestratorId))
            return Enumerable.Empty<Project>();

        return await _repository.QueryAsync(p => p.OrchestratorId == orchestratorId);
    }

    public async Task<IEnumerable<Project>> FindByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return Enumerable.Empty<Project>();

        var searchTerm = name.ToLowerInvariant();
        return await _repository.QueryAsync(p => p.Name.ToLowerInvariant().Contains(searchTerm));
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

        var projects = await FindByNameAsync(name);
        return projects.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidateProject(Project project)
    {
        var errors = new List<string>();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(project.Name))
            errors.Add("Project name is required and cannot be empty or whitespace");

        if (project.Name?.Length > 100)
            errors.Add("Project name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(project.OrchestratorId))
            errors.Add("OrchestratorId is required");

        // Validate repository if provided
        if (project.Repository != null)
        {
            if (string.IsNullOrWhiteSpace(project.Repository.Name))
                errors.Add("Repository name is required when repository is specified");

            if (string.IsNullOrWhiteSpace(project.Repository.Address))
                errors.Add("Repository address is required when repository is specified");

            if (!string.IsNullOrEmpty(project.Repository.Address) && 
                !Uri.TryCreate(project.Repository.Address, UriKind.Absolute, out _))
                errors.Add("Repository address must be a valid URL");
        }

        // Validate team if provided
        if (project.Team != null)
        {
            if (string.IsNullOrWhiteSpace(project.Team.Name))
                errors.Add("Team name is required when team is specified");

            if (project.Team.Name?.Length > 100)
                errors.Add("Team name cannot exceed 100 characters");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Project validation failed: {string.Join("; ", errors)}");
        }
    }
}
