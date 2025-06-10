using System;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLib.Abstractions;
using SharedLib.Abstractions.Stores;
using SharedLib.Configuration;
using SharedLib.Implementation.Firestore;
using SharedLib.Implementation.JsonFile;
using SharedLib.Implementation.LiteDb;
using SharedLib.Model;
using SharedLib.Stores;

namespace SharedLib.Extensions;

/// <summary>
/// Extension methods for registering document store services
/// </summary>
public static class DocumentStoreServiceExtensions
{
    /// <summary>
    /// Add document store services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDocumentStore(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        var options = new DocumentStoreOptions();
        configuration.GetSection(DocumentStoreOptions.SectionName).Bind(options);
        services.Configure<DocumentStoreOptions>(configuration.GetSection(DocumentStoreOptions.SectionName));        // Register the appropriate provider based on configuration
        switch (options.Provider.ToLowerInvariant())
        {
            case "firestore":
                services.AddFirestoreProvider(options);
                break;
            
            case "litedb":
                services.AddLiteDbProvider(options);
                break;
            
            case "jsonfile":
                services.AddJsonFileProvider(options);
                break;
            
            default:
                throw new InvalidOperationException($"Unknown document store provider: {options.Provider}");
        }

        // Register domain stores
        services.AddScoped<IProjectStore, ProjectStore>();
        services.AddScoped<IOrchestratorStore, OrchestratorStore>();
        services.AddScoped<IAgentSessionStore, AgentSessionStore>();

        return services;
    }

    private static void AddFirestoreProvider(this IServiceCollection services, DocumentStoreOptions options)
    {
        // Register FirestoreDb
        services.AddSingleton(provider =>
        {
            var projectId = options.ProjectId ?? options.ConnectionString;
            if (string.IsNullOrEmpty(projectId))
                throw new InvalidOperationException("ProjectId or ConnectionString must be provided for Firestore provider");
            
            return FirestoreDb.Create(projectId);
        });        // Register document repositories
        services.AddScoped<IDocumentRepository<Project>>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            return new FirestoreDocumentRepository<Project>(firestoreDb);
        });

        services.AddScoped<IDocumentRepository<Orchestrator>>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            return new FirestoreDocumentRepository<Orchestrator>(firestoreDb);
        });

        services.AddScoped<IDocumentRepository<AgentSession>>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            return new FirestoreDocumentRepository<AgentSession>(firestoreDb);
        });
    }

    private static void AddLiteDbProvider(this IServiceCollection services, DocumentStoreOptions options)
    {
        var connectionString = options.ConnectionString ?? "data.db";        // Register document repositories
        services.AddScoped<IDocumentRepository<Project>>(provider =>
            new LiteDbDocumentRepository<Project>(connectionString));

        services.AddScoped<IDocumentRepository<Orchestrator>>(provider =>
            new LiteDbDocumentRepository<Orchestrator>(connectionString));

        services.AddScoped<IDocumentRepository<AgentSession>>(provider =>
            new LiteDbDocumentRepository<AgentSession>(connectionString));
    }

    private static void AddJsonFileProvider(this IServiceCollection services, DocumentStoreOptions options)
    {
        var dataDirectory = options.DataDirectory ?? options.ConnectionString ?? "data";        // Register document repositories
        services.AddScoped<IDocumentRepository<Project>>(provider =>
            new JsonFileDocumentRepository<Project>(dataDirectory));

        services.AddScoped<IDocumentRepository<Orchestrator>>(provider =>
            new JsonFileDocumentRepository<Orchestrator>(dataDirectory));

        services.AddScoped<IDocumentRepository<AgentSession>>(provider =>
            new JsonFileDocumentRepository<AgentSession>(dataDirectory));
    }
}
