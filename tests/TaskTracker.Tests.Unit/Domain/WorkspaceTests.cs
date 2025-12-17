using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class WorkspaceTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var name = "My Workspace";
        var slug = Slug.Create("my-workspace").Value;
        var ownerId = Guid.NewGuid();

        // Act
        var result = Workspace.Create(name, slug, ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Slug.Should().Be(slug);
        result.Value.CreatedBy.Should().Be(ownerId);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var slug = Slug.Create("my-workspace").Value;

        // Act
        var result = Workspace.Create("", slug, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var name = new string('a', 101);
        var slug = Slug.Create("my-workspace").Value;

        // Act
        var result = Workspace.Create(name, slug, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot exceed 100 characters");
    }
}
