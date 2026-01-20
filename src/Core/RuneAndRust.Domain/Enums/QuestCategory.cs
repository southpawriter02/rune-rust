namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories for organizing quests.
/// </summary>
/// <remarks>
/// Quest categories affect display, behavior (abandonment), and reset mechanics.
/// Main quests cannot be abandoned. Daily and Repeatable quests support reset.
/// </remarks>
public enum QuestCategory
{
    /// <summary>Main story quests. Cannot be abandoned.</summary>
    Main = 0,

    /// <summary>Optional side quests. Can be abandoned.</summary>
    Side = 1,

    /// <summary>Daily repeatable quests. Reset at midnight.</summary>
    Daily = 2,

    /// <summary>Infinitely repeatable quests with optional cooldown.</summary>
    Repeatable = 3,

    /// <summary>Time-limited event/seasonal quests.</summary>
    Event = 4
}
