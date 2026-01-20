namespace RuneAndRust.Application.DTOs;

/// <summary>Result of a quest abandonment attempt.</summary>
public record AbandonmentResult(
    bool Success,
    string QuestId,
    string QuestName,
    string? FailureReason);
