using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchestratorWebApp
{
    public class DevelopmentAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // In development mode, always return an authenticated user
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Development User"),
                new Claim(ClaimTypes.Email, "dev@localhost.com"),
                new Claim("sub", "dev-user-123")
            }, "development");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
