using TaskTracker.Domain.Entities;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class SprintTests
{
    private Project CreateTestProject()
    {
        var workspace = Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
        return Project.Create(workspace, "Test Project", Slug.Create("test-project").Value, "TEST", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var project = CreateTestProject();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(14);

        // Act
        var result = Sprint.Create(project, "Sprint 1", startDate, endDate, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Sprint 1");
        result.Value.StartDate.Should().Be(startDate);
        result.Value.EndDate.Should().Be(endDate);
        result.Value.IsActive.Should().BeFalse();
        result.Value.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithGoal_ShouldSetGoal()
    {
        // Arrange
        var project = CreateTestProject();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(14);

        // Act
        var result = Sprint.Create(project, "Sprint 1", startDate, endDate, Guid.NewGuid(), goal: "Complete feature X");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Goal.Should().Be("Complete feature X");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = Sprint.Create(project, "", DateTime.UtcNow, DateTime.UtcNow.AddDays(14), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var longName = new string('a', 101);

        // Act
        var result = Sprint.Create(project, longName, DateTime.UtcNow, DateTime.UtcNow.AddDays(14), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("100");
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(-1);

        // Act
        var result = Sprint.Create(project, "Sprint 1", startDate, endDate, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("End date");
    }

    [Fact]
    public void Create_WithSameStartAndEndDate_ShouldReturnFailure()
    {
        // Arrange
        var project = CreateTestProject();
        var date = DateTime.UtcNow.Date;

        // Act
        var result = Sprint.Create(project, "Sprint 1", date, date, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Start_WhenNotActive_ShouldActivateSprint()
    {
        // Arrange
        var sprint = CreateValidSprint();

        // Act
        var result = sprint.Start(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        sprint.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_WhenAlreadyActive_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start(Guid.NewGuid());

        // Act
        var result = sprint.Start(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already active");
    }

    [Fact]
    public void Start_WhenCompleted_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start(Guid.NewGuid());
        sprint.Complete(Guid.NewGuid());

        // Act
        var result = sprint.Start(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("completed");
    }

    [Fact]
    public void Complete_WhenActive_ShouldCompleteSprint()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start(Guid.NewGuid());

        // Act
        var result = sprint.Complete(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        sprint.IsCompleted.Should().BeTrue();
        sprint.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Complete_WhenNotActive_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();

        // Act
        var result = sprint.Complete(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("inactive");
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();
        sprint.Start(Guid.NewGuid());
        sprint.Complete(Guid.NewGuid());

        // Act
        var result = sprint.Complete(Guid.NewGuid());

        // Assert - after completion, sprint is inactive
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("inactive");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateSprint()
    {
        // Arrange
        var sprint = CreateValidSprint();
        var newStart = DateTime.UtcNow.Date.AddDays(7);
        var newEnd = newStart.AddDays(14);

        // Act
        var result = sprint.Update("Updated Sprint", "Sprint goal", newStart, newEnd, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        sprint.Name.Should().Be("Updated Sprint");
        sprint.Goal.Should().Be("Sprint goal");
        sprint.StartDate.Should().Be(newStart);
        sprint.EndDate.Should().Be(newEnd);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();

        // Act
        var result = sprint.Update("", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Update_WithInvalidDateRange_ShouldReturnFailure()
    {
        // Arrange
        var sprint = CreateValidSprint();
        var date = DateTime.UtcNow.Date;

        // Act
        var result = sprint.Update("Sprint", null, date, date.AddDays(-1), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    private Sprint CreateValidSprint()
    {
        var project = CreateTestProject();
        return Sprint.Create(project, "Sprint 1", DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(14), Guid.NewGuid()).Value;
    }
}
