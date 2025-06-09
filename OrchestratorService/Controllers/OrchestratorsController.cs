using System;
using Microsoft.AspNetCore.Mvc;
using SharedLib.DTOs;
using SharedLib.Model;
using OrchestratorService.Services;

namespace OrchestratorService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrchestratorsController : ControllerBase
{
    private readonly IFirestoreService _firestoreService;
    private readonly ILogger<OrchestratorsController> _logger;

    public OrchestratorsController(IFirestoreService firestoreService, ILogger<OrchestratorsController> logger)
    {
        _firestoreService = firestoreService;
        _logger = logger;
    }

    /// <summary>Create a new orchestrator</summary>
    [HttpPost]
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
    [HttpGet]
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
    [HttpGet("{id}")]
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
