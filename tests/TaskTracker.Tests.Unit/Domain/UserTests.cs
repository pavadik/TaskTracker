using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;
using TaskTracker.Domain.Enums;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = "John Doe";
        var createdBy = Guid.NewGuid();

        // Act
        var result = User.Create(email, displayName, createdBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
        result.Value.DisplayName.Should().Be(displayName);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyDisplayName_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        var result = User.Create(email, "", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Display name cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongDisplayName_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = new string('a', 101);

        // Act
        var result = User.Create(email, displayName, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot exceed 100 characters");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var user = User.Create(email, "Test User", Guid.NewGuid()).Value;

        // Act
        user.Deactivate(Guid.NewGuid());

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var user = User.Create(email, "Test User", Guid.NewGuid()).Value;
        user.Deactivate(Guid.NewGuid());

        // Act
        user.Activate(Guid.NewGuid());

        // Assert
        user.IsActive.Should().BeTrue();
    }
}
