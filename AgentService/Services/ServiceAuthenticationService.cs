using Google.Apis.Auth.OAuth2;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AgentService.Services;

/// <summary>
/// Implementation of service-to-service authentication for Google Cloud
/// </summary>
public class ServiceAuthenticationService : IServiceAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServiceAuthenticationService> _logger;

    public ServiceAuthenticationService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServiceAuthenticationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }    /// <summary>
    /// Creates a JWT token for service-to-service authentication using Google Cloud Service Accounts
    /// </summary>
    public async Task<string> CreateServiceTokenAsync(string serviceAccountEmail, string[] scopes)
    {
        try
        {
            // Use Application Default Credentials to get service account credentials
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            
            // Create scoped credential for the specified scopes
            var scopedCredential = credential.CreateScoped(scopes);
            
            // Get access token
            var token = await ((ITokenAccess)scopedCredential).GetAccessTokenForRequestAsync();
            return token ?? throw new InvalidOperationException("Failed to obtain access token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service token for {ServiceAccount}", serviceAccountEmail);
            throw;
        }
    }

    /// <summary>
    /// Validates if the current user is authorized based on service account email
    /// </summary>
    public bool IsAuthorized(string[] allowedEmails)
    {
        var context = _httpContextAccessor.HttpContext;
        
        // In development, check if we're in development environment
        var environment = context?.RequestServices.GetService<IWebHostEnvironment>();
        if (environment?.IsDevelopment() == true)
        {
            _logger.LogInformation("Development mode: bypassing authentication check");
            return true;
        }
        
        if (context?.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("User is not authenticated");
            return false;
        }

        var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value 
                        ?? context.User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(emailClaim))
        {
            _logger.LogWarning("No email claim found in token");
            return false;
        }

        var isAuthorized = allowedEmails.Contains(emailClaim, StringComparer.OrdinalIgnoreCase);
        
        if (!isAuthorized)
        {
            _logger.LogWarning("Email {Email} is not in allowed list: {AllowedEmails}", 
                emailClaim, string.Join(", ", allowedEmails));
        }

        return isAuthorized;
    }
}
