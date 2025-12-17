namespace TaskTracker.Application.DTOs;

/// <summary>
/// Comment DTO
/// </summary>
public record CommentDto(
    Guid Id,
    string Content,
    Guid AuthorId,
    string AuthorName,
    string? AuthorAvatarUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
