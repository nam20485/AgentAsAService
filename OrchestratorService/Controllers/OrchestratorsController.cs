using System;
using Microsoft.AspNetCore.Mvc;
using SharedLib.DTOs;
using SharedLib.Model;
using SharedLib.Abstractions.Stores;
using OrchestratorService.Services;

namespace OrchestratorService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrchestratorsController : ControllerBase
{
    private readonly IOrchestratorStore _orchestratorStore;
    private readonly IFirestoreService _firestoreService; // Keep for transition period
    private readonly ILogger<OrchestratorsController> _logger;

    public OrchestratorsController(IOrchestratorStore orchestratorStore, IFirestoreService firestoreService, ILogger<OrchestratorsController> logger)
    {
        _orchestratorStore = orchestratorStore;
        _firestoreService = firestoreService;
        _logger = logger;
    }

    /// <summary>Create a new orchestrator</summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrchestrator([FromBody] CreateOrchestratorRequest request)
    {        try
        {
            var orchestrator = await _orchestratorStore.CreateAsync(request);
            
            var response = new OrchestratorResponse
            {
                Id = orchestrator.Id,
                Name = orchestrator.Name,
                CreatedAt = orchestrator.CreatedAt
            };

            return CreatedAtAction(nameof(GetOrchestrator), new { id = orchestrator.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating orchestrator");
            return BadRequest(new { error = ex.Message });
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
            var orchestrators = await _orchestratorStore.GetAllAsync();
            
            var responses = orchestrators.Select(o => new OrchestratorResponse
            {
                Id = o.Id,
                Name = o.Name,
                CreatedAt = o.CreatedAt
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
            var orchestrator = await _orchestratorStore.GetByIdAsync(id);

            if (orchestrator == null)
            {
                return NotFound(new { error = "Orchestrator not found" });
            }

            var response = new OrchestratorResponse
            {
                Id = orchestrator.Id,
                Name = orchestrator.Name,
                CreatedAt = orchestrator.CreatedAt
            };            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orchestrator");
            return BadRequest(new { error = ex.Message });
        }
    }
}
