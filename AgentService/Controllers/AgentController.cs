using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Services;
using SharedLib.Abstractions.Stores;
using SharedLib.Model;
using SharedLib.DTOs;
using Google.Cloud.AIPlatform.V1;

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
    }

    /// <summary>
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

            // Incorporate additional properties into the session creation logic
            var agentSession = await _agentSessionStore.CreateAsync(createRequest);

            // Update the response to include the additional properties
            var response = new
            {
                SessionId = agentSession.Id,
                RepositoryUrl = agentSession.RepositoryUrl,
                CreatedAt = agentSession.CreatedAt,
                CreatedBy = agentSession.CreatedBy,
                Status = agentSession.Status,
                OrchestratorAddress = request.OrchestratorAddress,
                ChatServerAddress = request.ChatServerAddress,
                AgentId = request.AgentId,
                Context = request.Context,
                Role = request.Role
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
    }

    /// <summary>
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

    /// <summary>
    /// Start an agent session
    /// </summary>
    [HttpPost("session/{sessionId}/start")]
    public async Task<IActionResult> StartSession(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to start sessions");
            }

            var agentSession = await _agentSessionStore.GetByIdAsync(sessionId);
            if (agentSession == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            var result = await _agentSessionProviderService.StartSessionAsync(agentSession);

            if (result)
            {
                _logger.LogInformation("Started session {SessionId}", sessionId);
                return Ok(new { message = "Session started successfully", sessionId });
            }

            return BadRequest(new { error = "Failed to start session" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot start session {SessionId}", sessionId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Stop an agent session
    /// </summary>
    [HttpPost("session/{sessionId}/stop")]
    public async Task<IActionResult> StopSession(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to stop sessions");
            }

            var result = await _agentSessionProviderService.StopSessionAsync(sessionId);

            if (result)
            {
                _logger.LogInformation("Stopped session {SessionId}", sessionId);
                return Ok(new { message = "Session stopped successfully", sessionId });
            }

            return BadRequest(new { error = "Failed to stop session or session was not running" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID {SessionId}", sessionId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Pause an agent session
    /// </summary>
    [HttpPost("session/{sessionId}/pause")]
    public async Task<IActionResult> PauseSession(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to pause sessions");
            }

            var result = await _agentSessionProviderService.PauseSessionAsync(sessionId);

            if (result)
            {
                _logger.LogInformation("Paused session {SessionId}", sessionId);
                return Ok(new { message = "Session paused successfully", sessionId });
            }

            return BadRequest(new { error = "Failed to pause session or session was not running" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID {SessionId}", sessionId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Resume an agent session
    /// </summary>
    [HttpPost("session/{sessionId}/resume")]
    public async Task<IActionResult> ResumeSession(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to resume sessions");
            }

            var result = await _agentSessionProviderService.ResumeSessionAsync(sessionId);

            if (result)
            {
                _logger.LogInformation("Resumed session {SessionId}", sessionId);
                return Ok(new { message = "Session resumed successfully", sessionId });
            }

            return BadRequest(new { error = "Failed to resume session" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot resume session {SessionId}", sessionId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get session status
    /// </summary>
    [HttpGet("session/{sessionId}/status")]
    public async Task<IActionResult> GetSessionStatus(string sessionId)
    {
        try
        {
            var allowedEmails = _configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>();

            if (!_authService.IsAuthorized(allowedEmails))
            {
                return Forbid("Service not authorized to access session status");
            }

            var status = await _agentSessionProviderService.GetSessionStatusAsync(sessionId);
            var isActive = await _agentSessionProviderService.IsSessionActiveAsync(sessionId);

            if (status == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return Ok(new 
            { 
                sessionId, 
                status, 
                isActive,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session status {SessionId}", sessionId);
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
    public Uri OrchestratorAddress { get; set; } // Default orchestrator address
    public Uri ChatServerAddress { get; set; } // Default chat server address
    public Guid AgentId { get; set; } = Guid.Empty; // Default agent ID
    public string Context { get; set; } = string.Empty; // Default context for the session
    public string Role { get; set; } = string.Empty; // Default role for the session
}
