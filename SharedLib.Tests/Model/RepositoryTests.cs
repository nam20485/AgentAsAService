using FluentAssertions;
using SharedLib.Model;
using Xunit;

namespace SharedLib.Tests.Model;

public class RepositoryTests
{
    [Fact]
    public void Repository_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var repository = new Repository();

        // Assert
        repository.Name.Should().Be(string.Empty);
        repository.Address.Should().Be(string.Empty);
    }

    [Fact]
    public void Repository_Should_Set_Properties_Correctly()
    {
        // Arrange
        var name = "test-repository";
        var address = "https://github.com/user/repo";

        // Act
        var repository = new Repository
        {
            Name = name,
            Address = address
        };

        // Assert
        repository.Name.Should().Be(name);
        repository.Address.Should().Be(address);
    }

    [Theory]
    [InlineData("repo-name", "https://github.com/user/repo")]
    [InlineData("another-repo", "https://gitlab.com/user/another-repo")]
    [InlineData("local-repo", "/local/path/to/repo")]
    public void Repository_Should_Handle_Various_Address_Formats(string name, string address)
    {
        // Arrange & Act
        var repository = new Repository
        {
            Name = name,
            Address = address
        };

        // Assert
        repository.Name.Should().Be(name);
        repository.Address.Should().Be(address);
    }

    [Fact]
    public void Repository_Should_Allow_Null_Values()
    {
        // Arrange & Act
        var repository = new Repository
        {
            Name = null!,
            Address = null!
        };

        // Assert
        repository.Name.Should().BeNull();
        repository.Address.Should().BeNull();
    }
}
