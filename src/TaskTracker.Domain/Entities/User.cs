using TaskTracker.Domain.Common;
using TaskTracker.Domain.Events;
using TaskTracker.Domain.ValueObjects;

namespace TaskTracker.Domain.Entities;

/// <summary>
/// User entity representing a system user
/// </summary>
public class User : AuditableEntity
{
    public Email Email { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; }
    public string? ExternalId { get; private set; } // For OAuth providers
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    private readonly List<WorkspaceMember> _workspaceMemberships = new();
    public IReadOnlyCollection<WorkspaceMember> WorkspaceMemberships => _workspaceMemberships.AsReadOnly();

    private User() { }

    public static Result<User> Create(Email email, string displayName, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<User>("Display name cannot be empty");

        if (displayName.Length > 100)
            return Result.Failure<User>("Display name cannot exceed 100 characters");

        var user = new User
        {
            Email = email,
            DisplayName = displayName.Trim(),
            IsActive = true
        };

        user.SetCreated(createdBy);
        user.AddDomainEvent(new UserCreatedEvent(user.Id, email.Value, displayName));

        return Result.Success(user);
    }

    public static Result<User> Create(Email email, string passwordHash, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<User>("Display name cannot be empty");

        if (displayName.Length > 100)
            return Result.Failure<User>("Display name cannot exceed 100 characters");

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName.Trim(),
            IsActive = true
        };

        user.SetCreated(user.Id);
        user.AddDomainEvent(new UserCreatedEvent(user.Id, email.Value, displayName));

        return Result.Success(user);
    }

    public Result UpdateProfile(string displayName, string? avatarUrl, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure("Display name cannot be empty");

        if (displayName.Length > 100)
            return Result.Failure("Display name cannot exceed 100 characters");

        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl?.Trim();
        SetUpdated(updatedBy);

        return Result.Success();
    }

    public void SetRefreshToken(string? refreshToken, DateTime? expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void SetExternalId(string externalId)
    {
        ExternalId = externalId;
    }

    public void Deactivate(Guid deactivatedBy)
    {
        IsActive = false;
        SetUpdated(deactivatedBy);
    }

    public void Activate(Guid activatedBy)
    {
        IsActive = true;
        SetUpdated(activatedBy);
    }
}
