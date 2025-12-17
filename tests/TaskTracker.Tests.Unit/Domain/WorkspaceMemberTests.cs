using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Domain.ValueObjects;
using FluentAssertions;

namespace TaskTracker.Tests.Unit.Domain;

public class WorkspaceMemberTests
{
    private Workspace CreateTestWorkspace()
    {
        return Workspace.Create("Test", Slug.Create("test").Value, Guid.NewGuid()).Value;
    }

    private User CreateTestUser(string email = "test@example.com")
    {
        return User.Create(Email.Create(email).Value, "Test User", Guid.NewGuid()).Value;
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var user = CreateTestUser();

        // Act
        var result = WorkspaceMember.Create(workspace, user, WorkspaceRole.Member, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WorkspaceId.Should().Be(workspace.Id);
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Role.Should().Be(WorkspaceRole.Member);
    }

    [Theory]
    [InlineData(WorkspaceRole.Owner)]
    [InlineData(WorkspaceRole.Admin)]
    [InlineData(WorkspaceRole.Member)]
    [InlineData(WorkspaceRole.Guest)]
    public void Create_WithAllRoles_ShouldSucceed(WorkspaceRole role)
    {
        // Arrange
        var workspace = CreateTestWorkspace();
        var user = CreateTestUser();

        // Act
        var result = WorkspaceMember.Create(workspace, user, role, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Role.Should().Be(role);
    }

    [Fact]
    public void ChangeRole_FromMemberToAdmin_ShouldSucceed()
    {
        // Arrange
        var member = CreateValidMember(WorkspaceRole.Member);

        // Act
        var result = member.ChangeRole(WorkspaceRole.Admin, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        member.Role.Should().Be(WorkspaceRole.Admin);
    }

    [Fact]
    public void ChangeRole_FromOwnerToMember_ShouldReturnFailure()
    {
        // Arrange
        var member = CreateValidMember(WorkspaceRole.Owner);

        // Act
        var result = member.ChangeRole(WorkspaceRole.Member, Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("owner");
    }

    [Fact]
    public void ChangeRole_ToOwner_ShouldSucceed()
    {
        // Arrange
        var member = CreateValidMember(WorkspaceRole.Admin);

        // Act
        var result = member.ChangeRole(WorkspaceRole.Owner, Guid.NewGuid());

        // Assert - domain allows this, business logic should prevent if needed
        result.IsSuccess.Should().BeTrue();
        member.Role.Should().Be(WorkspaceRole.Owner);
    }

    [Fact]
    public void ChangeRole_ToSameRole_ShouldSucceed()
    {
        // Arrange
        var member = CreateValidMember(WorkspaceRole.Member);

        // Act
        var result = member.ChangeRole(WorkspaceRole.Member, Guid.NewGuid());

        // Assert - domain allows this, idempotent operation
        result.IsSuccess.Should().BeTrue();
        member.Role.Should().Be(WorkspaceRole.Member);
    }

    private WorkspaceMember CreateValidMember(WorkspaceRole role = WorkspaceRole.Member)
    {
        var workspace = CreateTestWorkspace();
        var user = CreateTestUser();
        return WorkspaceMember.Create(workspace, user, role, Guid.NewGuid()).Value;
    }
}
