using Google.Apis.Auth.OAuth2;

namespace AgentService.Services;

/// <summary>
/// Service for handling service-to-service authentication with Google Cloud
/// </summary>
public interface IServiceAuthenticationService
{
    /// <summary>
    /// Creates a JWT token for service-to-service authentication
    /// </summary>
    /// <param name="serviceAccountEmail">The service account email to authenticate as</param>
    /// <param name="scopes">The scopes required for the token</param>
    /// <returns>A JWT token for authentication</returns>
    Task<string> CreateServiceTokenAsync(string serviceAccountEmail, string[] scopes);

    /// <summary>
    /// Validates if the current user is authorized to access the service
    /// </summary>
    /// <param name="allowedEmails">List of allowed service account emails</param>
    /// <returns>True if authorized, false otherwise</returns>
    bool IsAuthorized(string[] allowedEmails);
}
