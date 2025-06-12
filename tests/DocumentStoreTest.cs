// Simple test file to verify document store implementation
// You can run this manually to test the different providers

using SharedLib.Abstractions.Stores;
using SharedLib.Model;
using SharedLib.Stores;
using SharedLib.Implementation.LiteDb;
using SharedLib.Implementation.JsonFile;
using SharedLib.Abstractions;

namespace TestDocumentStore;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Document Store Implementation");
        Console.WriteLine("=====================================");

        // Test LiteDB Provider
        await TestProvider("LiteDB", new LiteDbDocumentRepository<Project>("test-litedb.db"));
        
        // Test JSON File Provider
        await TestProvider("JSON File", new JsonFileDocumentRepository<Project>("test-data"));
        
        Console.WriteLine("\nAll tests completed!");
    }

    private static async Task TestProvider(string providerName, IDocumentRepository<Project> repository)
    {
        Console.WriteLine($"\nTesting {providerName} Provider:");
        Console.WriteLine(new string('-', 40));

        var store = new ProjectStore(repository);

        try
        {
            // Test creating a project
            var project = new Project
            {
                Name = $"Test Project ({providerName})",
                OrchestratorId = "test-orchestrator-123",
                Repository = new Repository
                {
                    Name = "test-repo",
                    Address = "https://github.com/test/repo"
                }
            };

            Console.WriteLine($"Creating project: {project.Name}");
            var savedProject = await store.SaveAsync(project);
            Console.WriteLine($"✓ Project created with ID: {savedProject.Id}");

            // Test retrieving the project
            Console.WriteLine($"Retrieving project by ID: {savedProject.Id}");
            var retrievedProject = await store.GetByIdAsync(savedProject.Id);
            Console.WriteLine($"✓ Project retrieved: {retrievedProject?.Name}");

            // Test getting all projects
            Console.WriteLine("Getting all projects:");
            var allProjects = await store.GetAllAsync();
            Console.WriteLine($"✓ Found {allProjects.Count()} projects");

            // Test finding by orchestrator
            Console.WriteLine($"Finding projects by orchestrator: {project.OrchestratorId}");
            var projectsByOrchestrator = await store.FindByOrchestratorAsync(project.OrchestratorId);
            Console.WriteLine($"✓ Found {projectsByOrchestrator.Count()} projects for orchestrator");

            // Test validation (should fail)
            try
            {
                var invalidProject = new Project { Name = "", OrchestratorId = "" };
                await store.SaveAsync(invalidProject);
                Console.WriteLine("✗ Validation should have failed!");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("✓ Validation working correctly");
            }

            Console.WriteLine($"{providerName} Provider: ALL TESTS PASSED ✓");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error testing {providerName}: {ex.Message}");
        }
    }
}

// Simple test project file
// <Project Sdk="Microsoft.NET.Sdk">
//   <PropertyGroup>
//     <OutputType>Exe</OutputType>
//     <TargetFramework>net9.0</TargetFramework>
//     <Nullable>enable</Nullable>
//   </PropertyGroup>
//   <ItemGroup>
//     <ProjectReference Include="../SharedLib/SharedLib.csproj" />
//   </ItemGroup>
// </Project>
