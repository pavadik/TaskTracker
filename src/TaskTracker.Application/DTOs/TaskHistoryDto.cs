namespace TaskTracker.Application.DTOs;

/// <summary>
/// Task history entry DTO
/// </summary>
public record TaskHistoryDto(
    Guid Id,
    string FieldName,
    string OldValue,
    string NewValue,
    Guid ChangedById,
    string ChangedByName,
    DateTime ChangedAt);
