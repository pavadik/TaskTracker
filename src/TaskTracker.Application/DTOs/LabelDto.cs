namespace TaskTracker.Application.DTOs;

/// <summary>
/// Label DTO
/// </summary>
public record LabelDto(
    Guid Id,
    string Name,
    string Color,
    string? Description);
