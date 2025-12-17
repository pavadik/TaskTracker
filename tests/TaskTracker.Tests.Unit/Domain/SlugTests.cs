using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class SlugTests
{
    [Theory]
    [InlineData("my-project")]
    [InlineData("project123")]
    [InlineData("my-awesome-project")]
    public void Create_WithValidSlug_ShouldReturnSuccess(string slugValue)
    {
        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(slugValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptySlug_ShouldReturnFailure(string? slugValue)
    {
        // Act
        var result = Slug.Create(slugValue!);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_WithShortSlug_ShouldReturnFailure()
    {
        // Act
        var result = Slug.Create("a");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("at least 2 characters");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Act
        var result = Slug.Create("My-Project");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("my-project");
    }

    [Fact]
    public void Create_ShouldReplaceSpacesWithHyphens()
    {
        // Act
        var result = Slug.Create("my project");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("my-project");
    }
}
