namespace RuneAndRust.Application.DTOs;

/// <summary>DTO for daily quest status display.</summary>
public record DailyQuestStatusDto(
    string QuestId,
    string QuestName,
    string Status,
    DateTime? ResetTime,
    TimeSpan? TimeUntilReset)
{
    /// <summary>Gets formatted time until reset.</summary>
    public string FormattedTimeUntilReset => TimeUntilReset.HasValue
        ? $"{(int)TimeUntilReset.Value.TotalHours}h {TimeUntilReset.Value.Minutes}m"
        : "Available";
}
