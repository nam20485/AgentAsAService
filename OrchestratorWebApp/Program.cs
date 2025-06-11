using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using OrchestratorWebApp;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure Google OAuth authentication
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Authentication:Google", options.ProviderOptions);
    options.ProviderOptions.Authority = "https://accounts.google.com";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
});

// Configure HTTP client based on environment
var isDevelopment = builder.HostEnvironment.IsDevelopment();

if (isDevelopment)
{
    // Development: HTTP client without authentication for easier testing
    builder.Services.AddHttpClient("OrchestratorAPI", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["OrchestratorService:BaseUrl"] ?? builder.HostEnvironment.BaseAddress);
    });
}
else
{
    // Production: HTTP client with authentication
    builder.Services.AddHttpClient("OrchestratorAPI", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["OrchestratorService:BaseUrl"] ?? builder.HostEnvironment.BaseAddress);
    }).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    
    // Register authorization message handler for production
    builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();
}

// Configure default HTTP client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("OrchestratorAPI"));

// Register Radzen components
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
