using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrchestratorService.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Kestrel to use the PORT environment variable if available
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
        builder.WebHost.UseUrls($"http://*:{port}");

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add HTTP client factory
        builder.Services.AddHttpClient();

        // Register HTTP client services
        builder.Services.AddHttpClient<IAgentHttpClientService, AgentHttpClientService>();

        // Add Google Cloud Firestore
        builder.Services.AddSingleton(provider =>
        {
            var projectId = builder.Configuration["GoogleCloud:ProjectId"];
            return FirestoreDb.Create(projectId);
        });

        // Add Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://securetoken.google.com/{builder.Configuration["GoogleCloud:ProjectId"]}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{builder.Configuration["GoogleCloud:ProjectId"]}",
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["GoogleCloud:ProjectId"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add Authorization
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser());
        });

        // Add CORS for Blazor WebAssembly
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("BlazorPolicy", policy =>
            {
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7001" })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Only use HTTPS redirection in development - Cloud Run handles HTTPS termination
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("BlazorPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        var firebaseApp = FirebaseApp.Create();

        var summaries = new[]
        {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();



        app.Run();
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
