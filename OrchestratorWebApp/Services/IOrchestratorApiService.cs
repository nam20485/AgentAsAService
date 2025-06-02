using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrchestratorWebApp.Services;

/// <summary>
/// Service for communicating with the OrchestratorService API
/// </summary>
public interface IOrchestratorApiService
{
    Task<OrchestratorStatusResponse?> GetStatusAsync();
    Task<OrchestrationResponse?> CreateOrchestrationAsync(CreateOrchestrationRequest request);
    Task<OrchestrationResponse?> GetOrchestrationAsync(string orchestrationId);
    Task<List<OrchestrationResponse>> GetUserOrchestrationsAsync();
}

public class OrchestratorApiService : IOrchestratorApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrchestratorApiService> _logger;

    public OrchestratorApiService(HttpClient httpClient, ILogger<OrchestratorApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OrchestratorStatusResponse?> GetStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<OrchestratorStatusResponse>("api/orchestrator/status");
            _logger.LogInformation("Successfully retrieved orchestrator status");
            return response;
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orchestrator status");
            return null;
        }
    }

    public async Task<OrchestrationResponse?> CreateOrchestrationAsync(CreateOrchestrationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/orchestrator/orchestration", request);
            
            if (response.IsSuccessStatusCode)
            {
                var orchestration = await response.Content.ReadFromJsonAsync<OrchestrationResponse>();
                _logger.LogInformation("Successfully created orchestration {OrchestrationId}", orchestration?.OrchestrationId);
                return orchestration;
            }

            _logger.LogWarning("Failed to create orchestration. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating orchestration for repository {Repository}", request.RepositoryUrl);
            return null;
        }
    }

    public async Task<OrchestrationResponse?> GetOrchestrationAsync(string orchestrationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<OrchestrationResponse>($"api/orchestrator/orchestration/{orchestrationId}");
            _logger.LogInformation("Successfully retrieved orchestration {OrchestrationId}", orchestrationId);
            return response;
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogInformation("Orchestration {OrchestrationId} not found", orchestrationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orchestration {OrchestrationId}", orchestrationId);
            return null;
        }
    }

    public async Task<List<OrchestrationResponse>> GetUserOrchestrationsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<UserOrchestrationItem>>("api/orchestrator/orchestrations");
            
            if (response != null)
            {
                var orchestrations = response.Select(item => item.Data).ToList();
                _logger.LogInformation("Successfully retrieved {Count} orchestrations", orchestrations.Count);
                return orchestrations;
            }

            return new List<OrchestrationResponse>();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return new List<OrchestrationResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user orchestrations");
            return new List<OrchestrationResponse>();
        }
    }
}

// DTOs matching the API responses
public class OrchestratorStatusResponse
{
    public string Service { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? User { get; set; }
    public string? UserId { get; set; }
}

public class OrchestrationResponse
{
    public string OrchestrationId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public int AgentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<object> Agents { get; set; } = new();
}

public class CreateOrchestrationRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public int AgentCount { get; set; } = 1;
    public string? Branch { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}

public class UserOrchestrationItem
{
    public string OrchestrationId { get; set; } = string.Empty;
    public OrchestrationResponse Data { get; set; } = new();
}
