using FluentAssertions;
using SharedLib.Model;
using Xunit;

namespace SharedLib.Tests.Model;

public class ProjectTests
{
    [Fact]
    public void Project_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        project.Id.Should().NotBeEmpty();
        project.Name.Should().Be(string.Empty);
        project.OrchestratorId.Should().Be(string.Empty);
        project.Repository.Should().NotBeNull();
        project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Project_Should_Set_Properties_Correctly()
    {
        // Arrange
        var projectId = "project-123";
        var projectName = "Test Project";
        var orchestratorId = "orchestrator-123";
        var repository = new Repository 
        { 
            Name = "test-repo", 
            Address = "https://github.com/test/repo" 
        };
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var project = new Project
        {
            Id = projectId,
            Name = projectName,
            OrchestratorId = orchestratorId,
            Repository = repository,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        project.Id.Should().Be(projectId);
        project.Name.Should().Be(projectName);
        project.OrchestratorId.Should().Be(orchestratorId);
        project.Repository.Should().Be(repository);
        project.CreatedAt.Should().Be(createdAt);
        project.UpdatedAt.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Project_Should_Handle_Empty_Name_Values(string? name)
    {
        // Arrange & Act
        var project = new Project { Name = name };

        // Assert
        project.Name.Should().Be(name);
    }
}
