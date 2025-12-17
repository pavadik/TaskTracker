using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    public void Create_WithValidEmail_ShouldReturnSuccess(string emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(emailAddress.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldReturnFailure(string? emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cannot be empty");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Create_WithInvalidEmail_ShouldReturnFailure(string emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid email format");
    }

    [Fact]
    public void Emails_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("TEST@example.com").Value;
        var email2 = Email.Create("test@Example.COM").Value;

        // Assert
        email1.Should().Be(email2);
    }
}
