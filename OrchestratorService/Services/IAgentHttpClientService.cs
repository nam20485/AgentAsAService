using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using System.Text;

using System.Text.Json;

namespace OrchestratorService.Services;

/// <summary>
/// Service for communicating with AgentService using authenticated HTTP requests
/// </summary>
public interface IAgentHttpClientService
{
    /// <summary>
    /// Get agent status
    /// </summary>
    Task<AgentStatusResponse?> GetAgentStatusAsync();

    /// <summary>
    /// Create a new agent session
    /// </summary>
    Task<AgentSessionResponse?> CreateAgentSessionAsync(CreateAgentSessionRequest request);

    /// <summary>
    /// Get agent session details
    /// </summary>
    Task<AgentSessionResponse?> GetAgentSessionAsync(string sessionId);
}

public class AgentHttpClientService : IAgentHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentHttpClientService> _logger;
    private readonly IConfiguration _configuration;

    public AgentHttpClientService(
        HttpClient httpClient,
        ILogger<AgentHttpClientService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get an authenticated access token for service-to-service communication
    /// </summary>
    private async Task<string> GetServiceTokenAsync()
    {
        try
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            var scopes = new[] { "https://www.googleapis.com/auth/cloud-platform" };
            var scopedCredential = credential.CreateScoped(scopes);
            
            var token = await ((ITokenAccess)scopedCredential).GetAccessTokenForRequestAsync();
            return token ?? throw new InvalidOperationException("Failed to obtain service token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service authentication token");
            throw;
        }
    }

    /// <summary>
    /// Add authentication header to HTTP request
    /// </summary>
    private async Task AddAuthenticationAsync(HttpRequestMessage request)
    {
        var token = await GetServiceTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<AgentStatusResponse?> GetAgentStatusAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/agent/status");
            await AddAuthenticationAsync(request);

            var response = await _httpClient.SendAsync(request);            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var status = JsonSerializer.Deserialize<AgentStatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully retrieved agent status");
                return status;
            }

            _logger.LogWarning("Failed to get agent status. Status: {StatusCode}, Reason: {Reason}", 
            response.StatusCode, response.ReasonPhrase);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent status");
            return null;
        }
    }

    public async Task<AgentSessionResponse?> CreateAgentSessionAsync(CreateAgentSessionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/agent/session")
            {
                Content = content
            };
            await AddAuthenticationAsync(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<AgentSessionResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully created agent session {SessionId}", session?.SessionId);
                return session;
            }

            _logger.LogWarning("Failed to create agent session. Status: {StatusCode}, Reason: {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent session for repository {Repository}", request.RepositoryUrl);
            return null;
        }
    }

    public async Task<AgentSessionResponse?> GetAgentSessionAsync(string sessionId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/agent/session/{sessionId}");
            await AddAuthenticationAsync(request);

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<AgentSessionResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully retrieved agent session {SessionId}", sessionId);
                return session;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Agent session {SessionId} not found", sessionId);
                return null;
            }

            _logger.LogWarning("Failed to get agent session {SessionId}. Status: {StatusCode}, Reason: {Reason}", 
                sessionId, response.StatusCode, response.ReasonPhrase);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent session {SessionId}", sessionId);
            return null;
        }
    }
}

// Response DTOs
public class AgentStatusResponse
{
    public string AgentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? AuthenticatedAs { get; set; }
    public string? Email { get; set; }
}

public class AgentSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateAgentSessionRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}
