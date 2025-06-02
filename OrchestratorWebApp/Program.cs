using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using OrchestratorWebApp;
using OrchestratorWebApp.Services;

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

// Configure HTTP client with authentication
builder.Services.AddHttpClient("OrchestratorAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OrchestratorService:BaseUrl"] ?? builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Configure default HTTP client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("OrchestratorAPI"));

// Register authorization message handler
builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();

// Register application services
builder.Services.AddScoped<IOrchestratorApiService, OrchestratorApiService>();

await builder.Build().RunAsync();
