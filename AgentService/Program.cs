using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth.OAuth2;
using AgentService.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SharedLib.Extensions;

internal class Program
{
    private const string DEFAULT_LISTEN_PORT = "7001";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure Kestrel to use the PORT environment variable if available
        var port = Environment.GetEnvironmentVariable("PORT") ?? DEFAULT_LISTEN_PORT;
        builder.WebHost.UseUrls($"http://*:{port}");// Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "Agent Service API", 
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
        builder.Services.AddHttpContextAccessor();
          // Add health checks
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("AgentService is running"), tags: new[] { "ready", "live" })            .AddCheck("firestore", () =>
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

        // Register authentication services
        builder.Services.AddScoped<IServiceAuthenticationService, ServiceAuthenticationService>();

        // Add document store services for AgentSession management
        builder.Services.AddDocumentStore(builder.Configuration);

        // Add Google Cloud Firestore (legacy - still needed for health checks)
        builder.Services.AddSingleton(provider =>
        {
            var projectId = builder.Configuration["GoogleCloud:ProjectId"];
            return FirestoreDb.Create(projectId);
        });

        // Add Service-to-Service Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("ServiceToService", options =>
            {
                // For service-to-service communication using Google Cloud Service Accounts
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
            })
            .AddJwtBearer("GoogleServiceAccount", options =>
            {
                // For Google Cloud Service Account authentication
                options.Authority = "https://accounts.google.com";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://accounts.google.com",
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["GoogleCloud:ProjectId"],
                    ValidateLifetime = true
                };
            });

        // Add Authorization policies
        builder.Services.AddAuthorization(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // In development, allow all requests to bypass authentication for easier testing
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAssertion(_ => true)); // Always allow in development
                
                options.AddPolicy("RequireOrchestratorService", policy =>
                    policy.RequireAssertion(_ => true)); // Always allow in development
            }
            else
            {
                // Production authentication policies
                options.AddPolicy("RequireServiceAuthentication", policy =>
                    policy.RequireAuthenticatedUser()
                          .AddAuthenticationSchemes("ServiceToService", "GoogleServiceAccount"));
                
                options.AddPolicy("RequireOrchestratorService", policy =>
                    policy.RequireClaim("email", builder.Configuration["AgentService:AllowedServiceEmails"]?.Split(',') ?? Array.Empty<string>()));
            }
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

        app.UseAuthentication();
        app.UseAuthorization();

        // Add health check endpoints
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
