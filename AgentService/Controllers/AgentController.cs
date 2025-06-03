using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Services;
using Google.Cloud.Firestore;

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
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<AgentController> _logger;
    private readonly IConfiguration _configuration;

    public AgentController(
        IServiceAuthenticationService authService,
        FirestoreDb firestoreDb,
        ILogger<AgentController> logger,
        IConfiguration configuration)
    {
        _authService = authService;
        _firestoreDb = firestoreDb;
        _logger = logger;
        _configuration = configuration;
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

            var sessionId = Guid.NewGuid().ToString();
            var session = new
            {
                SessionId = sessionId,
                RepositoryUrl = request.RepositoryUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.FindFirst("email")?.Value,
                Status = "Created"
            };

            // Save to Firestore
            var collection = _firestoreDb.Collection(_configuration["Firestore:CollectionNames:Sessions"] ?? "sessions");
            await collection.Document(sessionId).SetAsync(session);

            _logger.LogInformation("Created session {SessionId} for repository {Repository}", 
                sessionId, request.RepositoryUrl);

            return Ok(session);
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

            var collection = _firestoreDb.Collection(_configuration["Firestore:CollectionNames:Sessions"] ?? "sessions");
            var doc = await collection.Document(sessionId).GetSnapshotAsync();

            if (!doc.Exists)
            {
                return NotFound($"Session {sessionId} not found");
            }

            var session = doc.ToDictionary();
            _logger.LogInformation("Retrieved session {SessionId}", sessionId);

            return Ok(session);
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
