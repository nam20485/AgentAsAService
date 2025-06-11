# Document Store Implementation

This document describes the new two-layer document store abstraction implemented in the AgentAsAService project.

## Overview

The implementation provides database independence through two abstraction layers:

1. **Layer 1**: Generic document repository (`IDocumentRepository<T>`) - Abstracts different NoSQL providers
2. **Layer 2**: Domain stores (`IProjectStore`, etc.) - Provides domain-specific operations with validation

## Supported Providers

### 1. LiteDB (Local Development)
- **Use case**: Local development, embedded scenarios
- **Storage**: Single file database
- **Configuration**: File path to database file

### 2. JSON File (Development/Testing)
- **Use case**: Development, testing, simple scenarios
- **Storage**: Individual JSON files in directory structure
- **Configuration**: Directory path for data storage

### 3. Firestore (Production)
- **Use case**: Production deployment on Google Cloud
- **Storage**: Google Cloud Firestore
- **Configuration**: Google Cloud Project ID

## Configuration

### appsettings.Development.json
```json
{
  "DocumentStore": {
    "Provider": "LiteDb",
    "ConnectionString": "local-storage/orchestrator.db"
  }
}
```

### appsettings.Production.json
```json
{
  "DocumentStore": {
    "Provider": "Firestore",
    "ProjectId": "your-google-cloud-project-id"
  }
}
```

### Alternative: JSON File Provider
```json
{
  "DocumentStore": {
    "Provider": "JsonFile",
    "DataDirectory": "local-storage"
  }
}
```

## Usage in Services

### 1. Register Services in Program.cs
```csharp
using SharedLib.Extensions;

// Add document store services
builder.Services.AddDocumentStore(builder.Configuration);
```

### 2. Use Domain Stores in Controllers
```csharp
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly IProjectStore _projectStore;

    public ProjectController(IProjectStore projectStore)
    {
        _projectStore = projectStore;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.ProjectName,
            OrchestratorId = request.OrchestratorId,
            Repository = new Repository
            {
                Name = request.RepositoryName,
                Address = request.RepositoryAddress
            }
        };

        // Validation happens automatically in the store
        var savedProject = await _projectStore.SaveAsync(project);
        return Ok(savedProject);
    }
}
```

## Features

### Validation
- **Centralized**: All validation logic is in the domain stores
- **Automatic**: Validation runs on every save operation
- **Consistent**: Same validation rules regardless of provider

### Provider Switching
- **Configuration-driven**: Change providers via appsettings
- **No code changes**: Application logic remains the same
- **Environment-specific**: Different providers for different environments

### Data Consistency
- **ID Management**: Automatic ID generation if not provided
- **Timestamps**: Automatic CreatedAt/UpdatedAt handling
- **Relationships**: Automatic team creation for projects

## Migration Strategy

The implementation supports gradual migration:

1. **Phase 1**: Project entity migrated (✅ Complete)
2. **Phase 2**: Orchestrator entity (Planned)
3. **Phase 3**: AgentSession entity (Planned)

During transition, both old (`IFirestoreService`) and new (`IProjectStore`) services coexist.

## File Structure

```
SharedLib/
├── Abstractions/
│   ├── IDocumentRepository.cs          # Layer 1 abstraction
│   └── Stores/
│       └── IProjectStore.cs            # Layer 2 abstraction
├── Implementation/
│   ├── Firestore/
│   │   └── FirestoreDocumentRepository.cs
│   ├── LiteDb/
│   │   └── LiteDbDocumentRepository.cs
│   └── JsonFile/
│       └── JsonFileDocumentRepository.cs
├── Stores/
│   └── ProjectStore.cs                 # Domain store with validation
├── Configuration/
│   └── DocumentStoreOptions.cs         # Configuration classes
└── Extensions/
    └── DocumentStoreServiceExtensions.cs # DI registration
```

## Benefits

1. **Database Independence**: Switch between NoSQL providers easily
2. **Clean Architecture**: Follows dependency inversion principle
3. **Testability**: Easy to mock interfaces for unit tests
4. **Validation**: Centralized domain validation
5. **Development**: Easy local development with LiteDB/JSON files
6. **Production**: Scales with Firestore in production

## Testing

Use the provided `DocumentStoreTest.cs` to verify functionality:

```bash
# Test all providers locally
dotnet run --project DocumentStoreTest.cs
```

## Future Enhancements

1. **Transaction Support**: Add transactions when needed
2. **Caching**: Add caching layer for performance
3. **Query Optimization**: Provider-specific query optimizations
4. **Schema Migration**: Add schema versioning support
5. **Additional Providers**: MongoDB, CosmosDB, etc.
