using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class FriendlyIdTests
{
    [Theory]
    [InlineData("TASK", 1, "TASK-1")]
    [InlineData("PROJ", 123, "PROJ-123")]
    [InlineData("BUG", 999, "BUG-999")]
    public void Create_WithValidPrefixAndNumber_ShouldReturnSuccess(string prefix, int number, string expectedValue)
    {
        // Act
        var result = FriendlyId.Create(prefix, number);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPrefix_ShouldReturnFailure(string? prefix)
    {
        // Act
        var result = FriendlyId.Create(prefix!, 1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("prefix");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeNumber_ShouldReturnFailure(int number)
    {
        // Act
        var result = FriendlyId.Create("TASK", number);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("positive");
    }

    [Fact]
    public void Create_WithTooLongPrefix_ShouldReturnFailure()
    {
        // Arrange
        var longPrefix = new string('A', 11); // More than 10 characters

        // Act
        var result = FriendlyId.Create(longPrefix, 1);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("TASK-1")]
    [InlineData("PROJ-123")]
    [InlineData("BUG-999")]
    public void Parse_WithValidFormat_ShouldReturnSuccess(string value)
    {
        // Act
        var result = FriendlyId.Parse(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("TASK")]
    [InlineData("123")]
    [InlineData("TASK123")]
    [InlineData("-123")]
    public void Parse_WithInvalidFormat_ShouldReturnFailure(string value)
    {
        // Act
        var result = FriendlyId.Parse(value);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void FriendlyIds_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var id1 = FriendlyId.Create("TASK", 123).Value;
        var id2 = FriendlyId.Create("TASK", 123).Value;

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void FriendlyIds_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = FriendlyId.Create("TASK", 1).Value;
        var id2 = FriendlyId.Create("TASK", 2).Value;

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var id = FriendlyId.Create("TEST", 42).Value;

        // Assert
        id.ToString().Should().Be("TEST-42");
    }
}
