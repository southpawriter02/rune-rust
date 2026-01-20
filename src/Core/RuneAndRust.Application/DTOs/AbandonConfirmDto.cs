namespace RuneAndRust.Application.DTOs;

/// <summary>DTO for abandonment confirmation display.</summary>
public record AbandonConfirmDto(
    Guid QuestId,
    string QuestName,
    int Progress,
    int ObjectivesCompleted,
    int TotalObjectives,
    bool CanRestart,
    string WarningMessage);
