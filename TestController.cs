using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace AgentAsAService.Controllers;

/// <summary>
/// Test controller to validate automatic instruction loading
/// Demonstrates ASP.NET Core best practices for Google Cloud deployment
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Following security best practices
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Test endpoint to verify API functionality
    /// Implements proper error handling and logging for Google Cloud monitoring
    /// </summary>
    /// <param name="message">Test message parameter</param>
    /// <returns>Test response with validation</returns>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ping([FromQuery, Required] string message)
    {
        try
        {
            _logger.LogInformation("Test ping received with message: {Message}", message);

            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message parameter is required");
            }

            var response = new
            {
                Status = "Success",
                Message = $"Pong: {message}",
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                // Nathan's favorite animal reference (from instructions)
                FavoriteAnimal = "🐰 Rabbit"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ping request");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Health check endpoint for Google Cloud Load Balancer
    /// Follows Google Cloud best practices for container health checks
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous] // Health checks don't need authentication
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        _logger.LogDebug("Health check requested");
        
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}
