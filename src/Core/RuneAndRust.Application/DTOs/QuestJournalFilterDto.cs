namespace RuneAndRust.Application.DTOs;

/// <summary>DTO for quest journal filter display.</summary>
public record QuestJournalFilterDto(
    string? Category,
    string? Status,
    bool TimedOnly,
    bool ChainsOnly,
    string? SearchText,
    string DisplayName);
