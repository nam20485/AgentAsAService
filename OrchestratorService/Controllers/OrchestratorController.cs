using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using SharedLib.DTOs;
using SharedLib.Model;
using OrchestratorService.Services;

namespace OrchestratorService.Controllers;

/// <summary>
/// Controller for orchestrating agent operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAuthenticatedUser")]
public class OrchestratorController : ControllerBase
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<OrchestratorController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFirestoreService _firestoreService;

    public OrchestratorController(
        FirestoreDb firestoreDb,
        ILogger<OrchestratorController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IFirestoreService firestoreService)
    {
        _firestoreDb = firestoreDb;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _firestoreService = firestoreService;
    }

    /// <summary>
    /// Get orchestrator status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        try
        {
            var status = new
            {
                Service = "OrchestratorService",
                Status = "Running",
                Timestamp = DateTime.UtcNow,
                User = User.Identity?.Name,
                UserId = User.FindFirst("sub")?.Value ?? User.FindFirst("uid")?.Value
            };

            _logger.LogInformation("Orchestrator status requested by user {UserId}", status.UserId);

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orchestrator status");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new orchestration session that manages agents
    /// </summary>
    [HttpPost("orchestration")]
    public async Task<IActionResult> CreateOrchestration([FromBody] CreateOrchestrationRequest request)
    {
        try
        {
            var orchestrationId = Guid.NewGuid().ToString();
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("uid")?.Value;

            var orchestration = new
            {
                OrchestrationId = orchestrationId,
                UserId = userId,
                RepositoryUrl = request.RepositoryUrl,
                AgentCount = request.AgentCount,
                CreatedAt = DateTime.UtcNow,
                Status = "Created",
                Agents = new List<object>()
            };

            // Save to Firestore
            var collection = _firestoreDb.Collection("orchestrations");
            await collection.Document(orchestrationId).SetAsync(orchestration);

            _logger.LogInformation("Created orchestration {OrchestrationId} for user {UserId}", 
                orchestrationId, userId);

            return Ok(orchestration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating orchestration for repository {Repository}", request.RepositoryUrl);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get orchestration details
    /// </summary>
    [HttpGet("orchestration/{orchestrationId}")]
    public async Task<IActionResult> GetOrchestration(string orchestrationId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("uid")?.Value;

            var collection = _firestoreDb.Collection("orchestrations");
            var doc = await collection.Document(orchestrationId).GetSnapshotAsync();

            if (!doc.Exists)
            {
                return NotFound($"Orchestration {orchestrationId} not found");
            }

            var orchestration = doc.ToDictionary();
            
            // Check if user owns this orchestration
            if (orchestration.TryGetValue("UserId", out var ownerIdObj) && 
                ownerIdObj?.ToString() != userId)
            {
                return Forbid("You don't have access to this orchestration");
            }

            _logger.LogInformation("Retrieved orchestration {OrchestrationId} for user {UserId}", 
                orchestrationId, userId);

            return Ok(orchestration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestration {OrchestrationId}", orchestrationId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user's orchestrations
    /// </summary>
    [HttpGet("orchestrations")]
    public async Task<IActionResult> GetUserOrchestrations()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("uid")?.Value;

            var collection = _firestoreDb.Collection("orchestrations");
            var query = collection.WhereEqualTo("UserId", userId);
            var snapshot = await query.GetSnapshotAsync();

            var orchestrations = snapshot.Documents.Select(doc => new
            {
                OrchestrationId = doc.Id,
                Data = doc.ToDictionary()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} orchestrations for user {UserId}", 
                orchestrations.Count, userId);

            return Ok(orchestrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestrations for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>Create a new orchestrator</summary>
    [HttpPost("orchestrators")]
    public async Task<IActionResult> CreateOrchestrator([FromBody] CreateOrchestratorRequest request)
    {
        try
        {
            var orchestrator = await _firestoreService.CreateOrchestratorAsync(request);
            
            var response = new OrchestratorResponse
            {
                Id = orchestrator.Id,
                Name = orchestrator.Name,
                CreatedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetOrchestrator), new { id = orchestrator.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating orchestrator");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get all orchestrators</summary>
    [HttpGet("orchestrators")]
    public async Task<IActionResult> GetOrchestrators()
    {
        try
        {
            var orchestrators = await _firestoreService.GetAllOrchestratorsAsync();
            
            var responses = orchestrators.Select(o => new OrchestratorResponse
            {
                Id = o.Id,
                Name = o.Name,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestrators");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Get an orchestrator by ID</summary>
    [HttpGet("orchestrators/{id}")]
    public async Task<IActionResult> GetOrchestrator(string id)
    {
        try
        {
            var orchestrators = await _firestoreService.GetAllOrchestratorsAsync();
            var orchestrator = orchestrators.FirstOrDefault(o => o.Id == id);

            if (orchestrator == null)
            {
                return NotFound(new { error = "Orchestrator not found" });
            }

            var response = new OrchestratorResponse
            {
                Id = orchestrator.Id,
                Name = orchestrator.Name,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestrator");
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for creating a new orchestration
/// </summary>
public class CreateOrchestrationRequest
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public int AgentCount { get; set; } = 1;
    public string? Branch { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}


