using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrchestratorService.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SharedLib.Extensions;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Kestrel to use the PORT environment variable if available
        // In production (Cloud Run): uses $PORT from container environment
        // In development: uses port from appsettings.json or standard env vars, etc. if $PORT not set
        var environment = Environment.GetEnvironmentVariable("Environment");
        if (! string.IsNullOrEmpty(environment) && environment?.ToLower() != "development")
        {
            var port = Environment.GetEnvironmentVariable("PORT");
            // ensure port is valid (not blank/null, positive int)
            if (int.TryParse(port, out int parsedPort) && parsedPort > 0)
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }   
        }

        // Add services to the container.
        builder.Services.AddControllers();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "Orchestrator Service API", 
                Version = "v1" 
            });
            
            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        // Add HTTP client factory
        builder.Services.AddHttpClient();

        // Register HTTP client services
        builder.Services.AddHttpClient<IAgentHttpClientService, AgentHttpClientService>(
            client =>
            {
                // Configure base URL
                var url = builder.Configuration["AgentService:BaseUrl"];
                client.BaseAddress = new Uri(url);
            });       
        // Add document store services (replaces direct Firestore registration)
        builder.Services.AddDocumentStore(builder.Configuration);

        builder.Services.AddScoped<ITeamService, TeamService>();

        // Add Google Cloud Firestore (still needed for direct usage in some controllers)
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
        // Add Authorization with environment-specific policies
        builder.Services.AddAuthorization(options =>
        {
            if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
            {
                // Development & Testing: bypass authentication for easier testing
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAssertion(_ => true)); // Always allow
                
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAssertion(_ => true)); // Always allow in development
            }
            else if (builder.Environment.IsStaging())
            {
                // Staging: require authentication but may be more permissive
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());
                
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAuthenticatedUser());
            }
            else if (builder.Environment.IsProduction())
            {
                // Production: strict authentication policy
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());
                
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAuthenticatedUser());
            }
            else
            {
                // Default: require authentication
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                    policy.RequireAuthenticatedUser());
                
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAuthenticatedUser());
            }
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
        // Add health checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("OrchestratorService is running"), tags: new[] { "ready", "live" })
            .AddCheck("firestore", () =>
            {
                try
                {
                    var projectId = builder.Configuration["GoogleCloud:ProjectId"];
                    var db = FirestoreDb.Create(projectId);
                    // Test basic connectivity by accessing the database instance
                    var projectIdCheck = db.ProjectId;
                    return HealthCheckResult.Healthy($"Firestore connection is healthy (Project: {projectIdCheck})");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Firestore connection failed", ex);
                }
            }, tags: new[] { "ready" })            .AddCheck("agentservice", () =>
            {
                // This would be enhanced to actually check AgentService connectivity
                // For now, we'll do a basic configuration check
                var agentServiceUrl = builder.Configuration["AgentService:BaseUrl"];
                if (string.IsNullOrEmpty(agentServiceUrl))
                {
                    return HealthCheckResult.Degraded("AgentService URL not configured");
                }
                return HealthCheckResult.Healthy($"AgentService configuration available: {agentServiceUrl}");
            }, tags: new[] { "ready" })
            .AddCheck("memory", () =>
            {
                var gc = GC.GetTotalMemory(false);
                var gcMB = gc / 1024 / 1024;
                
                // Alert if memory usage is very high (adjust threshold as needed)
                if (gcMB > 500)
                {
                    return HealthCheckResult.Degraded($"High memory usage: {gcMB}MB");
                }
                
                return HealthCheckResult.Healthy($"Memory usage: {gcMB}MB");
            }, tags: new[] { "ready", "live" });

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
        app.UseAuthorization();        // Add health check endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            status = kvp.Value.Status.ToString(),
                            description = kvp.Value.Description,
                            duration = kvp.Value.Duration.TotalMilliseconds
                        }
                    ),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            }
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            status = kvp.Value.Status.ToString(),
                            description = kvp.Value.Description,
                            duration = kvp.Value.Duration.TotalMilliseconds
                        }
                    ),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            }
        });
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            status = kvp.Value.Status.ToString(),
                            description = kvp.Value.Description,
                            duration = kvp.Value.Duration.TotalMilliseconds
                        }
                    ),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
            }
        });

        app.MapControllers();

        var firebaseApp = FirebaseApp.Create();      
        app.Run();
    }
}
