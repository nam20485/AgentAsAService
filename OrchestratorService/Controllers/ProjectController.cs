using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedLib.Model;
using SharedLib.DTOs;
using SharedLib.Abstractions.Stores;
using OrchestratorService.Services;

namespace OrchestratorService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectStore _projectStore;
        private readonly IFirestoreService _firestoreService; // Keep for orchestrator operations during transition
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProjectStore projectStore, IFirestoreService firestoreService, ILogger<ProjectController> logger)
        {
            _projectStore = projectStore;
            _firestoreService = firestoreService;
            _logger = logger;
        }

        /// <summary>Create a new project with repository and orchestrator</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                // Create the project entity
                var project = new Project
                {
                    Name = request.ProjectName,
                    OrchestratorId = "", // Will be set after orchestrator creation
                    Repository = new Repository
                    {
                        Name = request.RepositoryName,
                        Address = request.RepositoryAddress
                    }
                };

                // Create orchestrator first (using legacy service for now)
                var orchestratorRequest = new CreateOrchestratorRequest
                {
                    Name = request.OrchestratorName
                };
                var orchestrator = await _firestoreService.CreateOrchestratorAsync(orchestratorRequest);
                
                // Set the orchestrator ID
                project.OrchestratorId = orchestrator.Id;

                // Save the project using the new store
                var savedProject = await _projectStore.SaveAsync(project);
                
                var response = new ProjectResponse
                {
                    Id = savedProject.Id,
                    Name = savedProject.Name,
                    OrchestratorId = savedProject.OrchestratorId,
                    Repository = new RepositoryInfo
                    {
                        Id = savedProject.Repository?.Id ?? "",
                        Name = savedProject.Repository?.Name ?? "",
                        Address = savedProject.Repository?.Address ?? ""
                    },
                    Team = new TeamInfo
                    {
                        Id = savedProject.Team.Id,
                        Name = savedProject.Team.Name,
                        MemberCount = savedProject.Team.Members.Count
                    },
                    CreatedAt = savedProject.CreatedAt
                };

                return CreatedAtAction(nameof(GetProject), new { id = savedProject.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating project");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Get all projects</summary>
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _projectStore.GetAllAsync();
                
                var responses = projects.Select(p => new ProjectResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    OrchestratorId = p.OrchestratorId,
                    Repository = new RepositoryInfo
                    {
                        Id = p.Repository?.Id ?? "",
                        Name = p.Repository?.Name ?? "",
                        Address = p.Repository?.Address ?? ""
                    },
                    Team = new TeamInfo
                    {
                        Id = p.Team.Id,
                        Name = p.Team.Name,
                        MemberCount = p.Team.Members.Count
                    },
                    CreatedAt = p.CreatedAt
                }).ToList();

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Get a project by ID</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(string id)
        {
            try
            {
                var project = await _projectStore.GetByIdAsync(id);

                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                var response = new ProjectResponse
                {
                    Id = project.Id,
                    Name = project.Name,
                    OrchestratorId = project.OrchestratorId,
                    Repository = new RepositoryInfo
                    {
                        Id = project.Repository?.Id ?? "",
                        Name = project.Repository?.Name ?? "",
                        Address = project.Repository?.Address ?? ""
                    },
                    Team = new TeamInfo
                    {
                        Id = project.Team.Id,
                        Name = project.Team.Name,
                        MemberCount = project.Team.Members.Count
                    },
                    CreatedAt = project.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
