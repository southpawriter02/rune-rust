namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Data transfer object for quest definitions loaded from config/quests.json.
/// Immutable record type for thread-safe sharing across services.
/// </summary>
public record QuestDefinitionDto
{
    /// <summary>Unique quest identifier matching config key (e.g., "faction_ironbane_purge_rust").</summary>
    public string QuestId { get; init; } = string.Empty;

    /// <summary>Display name shown in quest journal (e.g., "Purge the Rust").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Full narrative description of the quest.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Quest category: "Main", "Side", "Daily", "Repeatable", "Event".</summary>
    public string Category { get; init; } = "Side";

    /// <summary>Optional quest type for subcategorization: "Faction", "Exploration", "Challenge".</summary>
    public string? Type { get; init; }

    /// <summary>Optional faction affiliation (e.g., "IronBanes").</summary>
    public string? Faction { get; init; }

    /// <summary>NPC definition ID that gives this quest. Null for auto-accepted quests.</summary>
    public string? GiverNpcId { get; init; }

    /// <summary>NPC definition ID to turn in the completed quest. Null if same as giver.</summary>
    public string? TurnInNpcId { get; init; }

    /// <summary>Minimum legend level required to accept this quest. 0 = no requirement.</summary>
    public int MinimumLegend { get; init; }

    /// <summary>Quest IDs that must be completed before this quest becomes available.</summary>
    public IReadOnlyList<string> PrerequisiteQuestIds { get; init; } = [];

    /// <summary>Optional chain ID grouping related quests into a narrative sequence.</summary>
    public string? ChainId { get; init; }

    /// <summary>
    /// Order within the quest chain (1-based). Null if not part of a chain.
    /// </summary>
    /// <remarks>
    /// Used to present quests in sequence in the quest journal and to determine
    /// the "next quest" in a chain. Quests with the same ChainId are ordered
    /// by this field. Added in SPEC-FACTION-QUESTS-001.
    /// </remarks>
    public int? ChainOrder { get; init; }

    /// <summary>
    /// Minimum reputation value required with the quest's <see cref="Faction"/> to accept this quest.
    /// 0 = no reputation requirement (default). Requires <see cref="Faction"/> to be set for the check to apply.
    /// </summary>
    /// <remarks>
    /// <para>Examples: 0 (Neutral, chain opener), 25 (Friendly, mid-chain), 50 (Allied, late-chain).</para>
    /// <para>Used by <c>QuestChainService</c> to filter available quests based on player reputation.
    /// Added in SPEC-FACTION-QUESTS-001.</para>
    /// </remarks>
    public int MinimumReputation { get; init; } = 0;

    /// <summary>Whether this quest has a turn limit.</summary>
    public bool IsTimed { get; init; }

    /// <summary>Number of turns allowed to complete the quest. Null if not timed.</summary>
    public int? TimeLimit { get; init; }

    /// <summary>Whether this quest can be completed multiple times.</summary>
    public bool IsRepeatable { get; init; }

    /// <summary>Hours before a repeatable quest can be accepted again. Null if not repeatable.</summary>
    public int? RepeatCooldownHours { get; init; }

    /// <summary>Ordered list of objectives that must be completed.</summary>
    public IReadOnlyList<QuestObjectiveDto> Objectives { get; init; } = [];

    /// <summary>Conditions that cause automatic quest failure.</summary>
    public IReadOnlyList<FailureConditionDto> FailureConditions { get; init; } = [];

    /// <summary>Rewards granted upon quest completion.</summary>
    public QuestRewardDto Rewards { get; init; } = new();
}

/// <summary>
/// Individual quest objective definition.
/// </summary>
public record QuestObjectiveDto
{
    /// <summary>Human-readable objective description (e.g., "Destroy corrupted machinery").</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Objective type string that maps to QuestObjectiveType enum.
    /// Values: "KillEnemy", "CollectItem", "ExploreRoom", "InteractWithObject",
    /// "MakeChoice", "SurviveEncounter", "TalkToNpc", "ReachLevel".
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Target entity ID to match against game events.
    /// For KillEnemy: monster definition ID. For CollectItem: item definition ID.
    /// For ExploreRoom: room template ID. For TalkToNpc: NPC definition ID.
    /// </summary>
    public string TargetId { get; init; } = string.Empty;

    /// <summary>Number of times the objective must be fulfilled (e.g., kill 3 enemies).</summary>
    public int RequiredCount { get; init; } = 1;
}

/// <summary>
/// Failure condition definition for automatic quest failure detection.
/// </summary>
public record FailureConditionDto
{
    /// <summary>
    /// Failure type: "TimeExpired", "NPCDied", "ItemLost", "ReputationDropped", "LeftArea".
    /// Maps to FailureType enum.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Optional target ID (NPC ID, item ID, area ID, faction ID).</summary>
    public string? TargetId { get; init; }

    /// <summary>Optional threshold value (e.g., reputation minimum).</summary>
    public int? Threshold { get; init; }

    /// <summary>Message displayed to the player when this condition triggers.</summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Rewards granted upon quest completion.
/// </summary>
public record QuestRewardDto
{
    /// <summary>Experience points awarded.</summary>
    public int Experience { get; init; }

    /// <summary>Currency (gold/credits) awarded.</summary>
    public int Currency { get; init; }

    /// <summary>Specific items granted as rewards.</summary>
    public IReadOnlyList<ItemRewardDto> Items { get; init; } = [];

    /// <summary>Faction reputation changes. Key = faction ID, Value = change amount.</summary>
    public IReadOnlyDictionary<string, int> ReputationChanges { get; init; } =
        new Dictionary<string, int>();

    /// <summary>Quest IDs unlocked upon completion (next quests in a chain).</summary>
    public IReadOnlyList<string> UnlockedQuestIds { get; init; } = [];
}

/// <summary>
/// Individual item reward within quest rewards.
/// </summary>
public record ItemRewardDto
{
    /// <summary>Item definition ID.</summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>Number of items granted.</summary>
    public int Quantity { get; init; } = 1;
}

/// <summary>
/// Result of accepting a quest through GameSessionService.
/// </summary>
public record QuestAcceptResult
{
    /// <summary>Whether the quest was successfully accepted.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable result message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>The quest ID that was accepted (or attempted).</summary>
    public string QuestId { get; init; } = string.Empty;

    /// <summary>The runtime quest instance ID. Null if acceptance failed.</summary>
    public Guid? QuestInstanceId { get; init; }
}

/// <summary>
/// Result of completing and turning in a quest.
/// </summary>
public record QuestRewardResult
{
    /// <summary>Whether the quest was successfully completed.</summary>
    public bool Success { get; init; }

    /// <summary>Human-readable result message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Experience points awarded.</summary>
    public int ExperienceGranted { get; init; }

    /// <summary>Currency awarded.</summary>
    public int CurrencyGranted { get; init; }

    /// <summary>Items granted as rewards.</summary>
    public IReadOnlyList<ItemRewardDto> ItemsGranted { get; init; } = [];

    /// <summary>Quest IDs now unlocked.</summary>
    public IReadOnlyList<string> UnlockedQuests { get; init; } = [];
}

/// <summary>
/// Result of a quest objective being advanced by the event bus.
/// </summary>
public record QuestProgressResult
{
    /// <summary>The quest instance ID that was advanced.</summary>
    public Guid QuestId { get; init; }

    /// <summary>The quest definition ID.</summary>
    public string QuestDefinitionId { get; init; } = string.Empty;

    /// <summary>The quest name for display.</summary>
    public string QuestName { get; init; } = string.Empty;

    /// <summary>The objective description that was advanced.</summary>
    public string ObjectiveDescription { get; init; } = string.Empty;

    /// <summary>Current progress after advancement.</summary>
    public int CurrentProgress { get; init; }

    /// <summary>Required count to complete the objective.</summary>
    public int RequiredCount { get; init; }

    /// <summary>Whether this advancement completed the objective.</summary>
    public bool ObjectiveCompleted { get; init; }

    /// <summary>Whether all objectives in the quest are now complete.</summary>
    public bool AllObjectivesComplete { get; init; }
}

/// <summary>
/// Data for quest journal UI display.
/// </summary>
public record QuestJournalData
{
    /// <summary>All currently active quests.</summary>
    public IReadOnlyList<QuestJournalEntry> ActiveQuests { get; init; } = [];

    /// <summary>Recently completed quests.</summary>
    public IReadOnlyList<QuestJournalEntry> CompletedQuests { get; init; } = [];

    /// <summary>Recently failed quests.</summary>
    public IReadOnlyList<QuestJournalEntry> FailedQuests { get; init; } = [];
}

/// <summary>
/// Individual entry in the quest journal.
/// </summary>
public record QuestJournalEntry
{
    /// <summary>Quest instance ID.</summary>
    public Guid QuestId { get; init; }

    /// <summary>Quest definition ID.</summary>
    public string DefinitionId { get; init; } = string.Empty;

    /// <summary>Quest name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Quest description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Quest category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Completion percentage (0-100).</summary>
    public int CompletionPercentage { get; init; }

    /// <summary>Remaining turns for timed quests. Null if not timed.</summary>
    public int? TurnsRemaining { get; init; }

    /// <summary>Objective summaries for display.</summary>
    public IReadOnlyList<ObjectiveSummary> Objectives { get; init; } = [];
}

/// <summary>
/// Summary of a single quest objective for journal display.
/// </summary>
public record ObjectiveSummary
{
    /// <summary>Objective description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Current progress count.</summary>
    public int CurrentProgress { get; init; }

    /// <summary>Required count for completion.</summary>
    public int RequiredCount { get; init; }

    /// <summary>Whether this objective is completed.</summary>
    public bool IsCompleted { get; init; }
}
