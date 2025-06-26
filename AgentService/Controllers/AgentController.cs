using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Services;
using SharedLib.Abstractions.Stores;
using SharedLib.Model;
using SharedLib.DTOs;

namespace AgentService.Controllers;

/// <summary>
/// Controller for managing agent operations with authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireServiceAuthentication")]
public class AgentController : ControllerBase
{
    private readonly IServiceAuthenticationService _authService;
    private readonly IAgentSessionStore _agentSessionStore;
    private readonly ILogger<AgentController> _logger;
    private readonly IConfiguration _configuration;

    private readonly IAgentSessionProviderService _agentSessionProviderService;

    public AgentController(
        IServiceAuthenticationService authService,
        IAgentSessionStore agentSessionStore,
        ILogger<AgentController> logger,
        IConfiguration configuration,
        IAgentSessionProviderService agentSessionProviderService)
    {
        _authService = authService;
        _agentSessionStore = agentSessionStore;
        _logger = logger;
        _configuration = configuration;
        _agentSessionProviderService = agentSessionProviderService;
    }

    /// <summary>
    /// Get agent status - requires service authentication
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to access agent");
            }

            var status = new
            {
                AgentId = Guid.NewGuid().ToString(),
                Status = "Running",
                Timestamp = DateTime.UtcNow,
                AuthenticatedAs = User.Identity?.Name ?? "Unknown",
                Email = User.FindFirst("email")?.Value
            };

            _logger.LogInformation("Agent status requested by {Email}", status.Email);

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent status");
            return StatusCode(500, "Internal server error");
        }
    }    /// <summary>
    /// Create a new agent session
    /// </summary>
    [HttpPost("session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to create sessions");
            }

            var createRequest = new CreateAgentSessionRequest
            {
                RepositoryUrl = request.RepositoryUrl,
                Branch = request.Branch,
                Configuration = request.Configuration,
                CreatedBy = User.FindFirst("email")?.Value
            };

            var agentSession = await _agentSessionStore.CreateAsync(createRequest);

            var response = new
            {
                SessionId = agentSession.Id,
                RepositoryUrl = agentSession.RepositoryUrl,
                CreatedAt = agentSession.CreatedAt,
                CreatedBy = agentSession.CreatedBy,
                Status = agentSession.Status
            };

            _logger.LogInformation("Created session {SessionId} for repository {Repository}",
                agentSession.Id, request.RepositoryUrl);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating session");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for repository {Repository}", request.RepositoryUrl);
            return StatusCode(500, "Internal server error");
        }
    }    /// <summary>
    /// Get agent session information
    /// </summary>
    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to access sessions");
            }

            var agentSession = await _agentSessionStore.GetByIdAsync(sessionId);

            if (agentSession == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            var response = new
            {
                SessionId = agentSession.Id,
                RepositoryUrl = agentSession.RepositoryUrl,
                Branch = agentSession.Branch,
                CreatedAt = agentSession.CreatedAt,
                CreatedBy = agentSession.CreatedBy,
                Status = agentSession.Status,
                Configuration = agentSession.Configuration,
                UpdatedAt = agentSession.UpdatedAt,
                ErrorMessage = agentSession.ErrorMessage
            };

            _logger.LogInformation("Retrieved session {SessionId}", sessionId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Request model for creating a new agent session
/// </summary>
public class CreateSessionRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}
