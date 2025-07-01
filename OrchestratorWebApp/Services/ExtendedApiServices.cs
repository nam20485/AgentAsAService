using OrchestratorWebApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrchestratorWebApp.Services;

/// <summary>
/// Repository service implementation for GitHub repositories
/// </summary>
public class RepositoryApiService : BaseApiService, IRepositoryService
{
    public RepositoryApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<IEnumerable<GitHubRepository>> GetAllRepositoriesAsync()
    {
        try
        {
            // Use existing API endpoint
            var response = await _httpClient.GetStringAsync("/api/repositories");
            var apiRepos = JsonSerializer.Deserialize<List<SharedLib.Model.Repository>>(response, _jsonOptions);
            
            return apiRepos?.Select(r => new GitHubRepository
            {
                Id = r.Id,
                Name = r.Name,
                Url = r.Address,
                Owner = ExtractOwnerFromUrl(r.Address),
                CreatedAt = DateTime.UtcNow // API doesn't provide this info yet
            }) ?? new List<GitHubRepository>();
        }
        catch (Exception)
        {
            return new List<GitHubRepository>();
        }
    }

    public async Task<GitHubRepository?> GetRepositoryAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"/api/repositories/{id}");
            var apiRepo = JsonSerializer.Deserialize<SharedLib.Model.Repository>(response, _jsonOptions);
            
            if (apiRepo == null) return null;
            
            return new GitHubRepository
            {
                Id = apiRepo.Id,
                Name = apiRepo.Name,
                Url = apiRepo.Address,
                Owner = ExtractOwnerFromUrl(apiRepo.Address),
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GitHubRepository> CreateRepositoryAsync(string name, string owner, string url, string branch = "main")
    {
        var repository = new GitHubRepository
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Owner = owner,
            Url = url,
            Branch = branch,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // Convert to existing API model
            var apiRepo = new SharedLib.Model.Repository
            {
                Id = repository.Id,
                Name = repository.Name,
                Address = repository.Url
            };

            var response = await _httpClient.PostAsJsonAsync("/api/repositories", apiRepo, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var createdRepo = JsonSerializer.Deserialize<SharedLib.Model.Repository>(content, _jsonOptions);
                if (createdRepo != null)
                {
                    repository.Id = createdRepo.Id;
                }
            }
            else
            {
                Console.WriteLine($"Failed to create repository in API: {response.StatusCode} - {response.ReasonPhrase}");
                // Note: For this implementation, we still return the local object for graceful degradation
                // TODO: Consider returning null or throwing exception based on business requirements
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating repository: {ex.Message}");
            // Note: For this implementation, we still return the local object for graceful degradation
            // TODO: Consider returning null or throwing exception based on business requirements
        }

        return repository;
    }

    public async Task<GitHubRepository> UpdateRepositoryAsync(GitHubRepository repository)
    {
        try
        {
            var apiRepo = new SharedLib.Model.Repository
            {
                Id = repository.Id,
                Name = repository.Name,
                Address = repository.Url
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/repositories/{repository.Id}", apiRepo, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to update repository {repository.Id}: {response.StatusCode} - {response.ReasonPhrase}");
                // For now, still return the repository object for graceful degradation
                // TODO: Consider throwing exception or returning error state in future
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating repository {repository.Id}: {ex.Message}");
            // For now, return repository for graceful degradation
            // TODO: Consider throwing exception or returning error state in future
        }

        return repository;
    }

    public async Task<bool> DeleteRepositoryAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/repositories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ValidateRepositoryAsync(string url)
    {
        // For now, assume all repositories are accessible
        // TODO: Implement actual GitHub API validation
        return await Task.FromResult(!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute));
    }

    public async Task<GitHubRepository?> DiscoverRepositoryAsync(string url)
    {
        try
        {
            if (await ValidateRepositoryAsync(url))
            {
                var owner = ExtractOwnerFromUrl(url);
                var name = ExtractNameFromUrl(url);
                
                return new GitHubRepository
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Owner = owner,
                    Url = url,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }
        catch
        {
            // Handle error
        }
        
        return null;
    }

    public async Task<IEnumerable<string>> GetRepositoryBranchesAsync(string repositoryId)
    {
        // Mock branches for now - TODO: Implement GitHub API integration
        return await Task.FromResult(new List<string> { "main", "develop", "feature/ui-refactor" });
    }

    private static string ExtractOwnerFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return "";
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 0 ? segments[0] : "";
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine($"Failed to parse URL '{url}': {ex.Message}");
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error parsing URL '{url}': {ex.Message}");
            return "";
        }
    }

    private static string ExtractNameFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return "";
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 1 ? segments[1].Replace(".git", "") : "";
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine($"Failed to parse URL '{url}': {ex.Message}");
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error parsing URL '{url}': {ex.Message}");
            return "";
        }
    }
}

/// <summary>
/// Specification service implementation
/// </summary>
public class SpecificationApiService : BaseApiService, ISpecificationService
{
    public SpecificationApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ProjectSpecification?> GetSpecificationAsync(string id)
    {
        // Mock data for now - TODO: Implement backend storage
        var specs = await GetAllSpecificationsAsync();
        return specs.FirstOrDefault(s => s.Id == id);
    }

    public async Task<ProjectSpecification> CreateSpecificationAsync(string title, string description, string content, SpecificationType type = SpecificationType.Markdown)
    {
        var specification = new ProjectSpecification
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description,
            Content = content,
            Type = type,
            RequiredCapabilities = ExtractCapabilities(content),
            EstimatedComplexity = EstimateComplexity(content),
            Priority = "Medium",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // TODO: Implement API call to save specification
        return await Task.FromResult(specification);
    }

    public async Task<ProjectSpecification> UpdateSpecificationAsync(ProjectSpecification specification)
    {
        specification.UpdatedAt = DateTime.UtcNow;
        // TODO: Implement API call
        return await Task.FromResult(specification);
    }

    public async Task<bool> DeleteSpecificationAsync(string id)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> ValidateSpecificationAsync(string id)
    {
        var specification = await GetSpecificationAsync(id);
        return specification != null &&
               !string.IsNullOrWhiteSpace(specification.Title) &&
               !string.IsNullOrWhiteSpace(specification.Content);
    }

    public async Task<string> ProcessSpecificationAsync(string id)
    {
        var specification = await GetSpecificationAsync(id);
        if (specification == null) return "Specification not found";
        
        // Mock processing result - TODO: Implement AI-powered processing
        return $"Processed specification '{specification.Title}' with {specification.RequiredCapabilities.Count} required capabilities.";
    }

    private async Task<IEnumerable<ProjectSpecification>> GetAllSpecificationsAsync()
    {
        // Mock data for now - TODO: Implement backend storage
        return await Task.FromResult(new List<ProjectSpecification>
        {
            new ProjectSpecification
            {
                Id = "spec-1",
                Title = "E-commerce Website",
                Description = "A complete e-commerce solution",
                Type = SpecificationType.Feature,
                Content = "Build a modern e-commerce website with product catalog, shopping cart, and payment processing.",
                RequiredCapabilities = new List<string> { "Frontend Development", "Backend Development", "Database Design" },
                EstimatedComplexity = "High",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        });
    }

    private static List<string> ExtractCapabilities(string content)
    {
        var capabilities = new List<string>();
        var lowercaseContent = content.ToLowerInvariant();

        var capabilityMap = new Dictionary<string, string[]>
        {
            ["Frontend Development"] = new[] { "frontend", "ui", "interface", "react", "angular", "vue", "html", "css", "javascript" },
            ["Backend Development"] = new[] { "backend", "api", "server", "rest", "graphql", "microservice" },
            ["Database Design"] = new[] { "database", "sql", "nosql", "mongodb", "postgresql", "mysql" },
            ["Mobile Development"] = new[] { "mobile", "ios", "android", "app", "xamarin", "flutter" },
            ["DevOps"] = new[] { "devops", "ci/cd", "docker", "kubernetes", "deployment", "pipeline" },
            ["Testing"] = new[] { "test", "testing", "qa", "quality", "unit test", "integration test" },
            ["Security"] = new[] { "security", "authentication", "authorization", "encryption", "jwt" },
            ["Performance"] = new[] { "performance", "optimization", "caching", "scaling", "load" }
        };

        foreach (var (capability, keywords) in capabilityMap)
        {
            if (keywords.Any(keyword => lowercaseContent.Contains(keyword)))
            {
                capabilities.Add(capability);
            }
        }

        return capabilities.Any() ? capabilities : new List<string> { "General Development" };
    }

    private static string EstimateComplexity(string content)
    {
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var complexityIndicators = new[] { "integrate", "complex", "advanced", "multiple", "system", "architecture" };
        var hasComplexityIndicators = complexityIndicators.Any(indicator => 
            content.Contains(indicator, StringComparison.OrdinalIgnoreCase));

        return (wordCount, hasComplexityIndicators) switch
        {
            (< 50, false) => "Low",
            (< 100, false) => "Medium",
            (_, true) => "High",
            _ => "Medium"
        };
    }
}

/// <summary>
/// Session service implementation for agent session monitoring
/// </summary>
public class SessionApiService : BaseApiService, ISessionService
{
    public SessionApiService(HttpClient httpClient) : base(httpClient) { }

    public async Task<IEnumerable<AgentSession>> GetActiveSessionsAsync()
    {
        // Mock data for now - TODO: Implement real session tracking
        return await Task.FromResult(new List<AgentSession>
        {
            new AgentSession
            {
                Id = "session-1",
                ProjectId = "project-1",
                AgentId = "agent-1",
                Status = SessionStatus.Running,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                Activity = "Designing user interface components",
                Progress = 45,
                LastHeartbeat = DateTime.UtcNow.AddSeconds(-15)
            }
        });
    }

    public async Task<AgentSession?> GetSessionAsync(string sessionId)
    {
        var sessions = await GetActiveSessionsAsync();
        return sessions.FirstOrDefault(s => s.Id == sessionId);
    }

    public async Task<IEnumerable<AgentSession>> GetAgentSessionsAsync(string agentId)
    {
        var sessions = await GetActiveSessionsAsync();
        return sessions.Where(s => s.AgentId == agentId);
    }

    public async Task<IEnumerable<AgentSession>> GetProjectSessionsAsync(string projectId)
    {
        var sessions = await GetActiveSessionsAsync();
        return sessions.Where(s => s.ProjectId == projectId);
    }

    public async Task<bool> StartSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> StopSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> PauseSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> ResumeSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<bool> TerminateSessionAsync(string sessionId)
    {
        // TODO: Implement API call
        return await Task.FromResult(true);
    }

    public async Task<IEnumerable<SessionLog>> GetSessionLogsAsync(string sessionId)
    {
        // Mock logs - TODO: Implement real logging
        return await Task.FromResult(new List<SessionLog>
        {
            new SessionLog
            {
                Id = "log-1",
                SessionId = sessionId,
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Level = Models.LogLevel.Info,
                Message = "Session started successfully",
                Source = "SessionManager"
            }
        });
    }

    public async Task<SessionLog> AddSessionLogAsync(string sessionId, Models.LogLevel level, string message, Dictionary<string, object>? data = null)
    {
        var log = new SessionLog
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Source = "System",
            Data = data ?? new Dictionary<string, object>()
        };

        // TODO: Implement API call
        return await Task.FromResult(log);
    }
}
