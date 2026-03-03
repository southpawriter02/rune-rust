namespace RuneAndRust.Application.DTOs;

/// <summary>
/// Summary of a faction quest chain's state for a specific player.
/// </summary>
/// <remarks>
/// <para>Provides a chain-level view for the quest journal, showing overall progress,
/// the next available quest, and whether the player is reputation-locked.</para>
///
/// <para>Created by <c>IQuestChainService.GetChainSummary()</c> and
/// <c>IQuestChainService.GetAllChainSummaries()</c>.</para>
///
/// <para>Example rendering in quest journal:</para>
/// <code>
/// ┌─────────────────────────────────────────┐
/// │ Iron-Bane Initiation  [██████░░░░] 66%  │
/// │ Iron-Banes · 2 of 3 quests complete     │
/// │ Next: Undying Commander (Friendly req.)  │
/// └─────────────────────────────────────────┘
/// </code>
/// </remarks>
public record QuestChainSummaryDto
{
    /// <summary>
    /// Gets the chain ID (e.g., "ironbane_initiation").
    /// </summary>
    public string ChainId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the faction ID this chain belongs to (e.g., "iron-banes").
    /// </summary>
    public string FactionId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the faction display name (e.g., "Iron-Banes").
    /// </summary>
    public string FactionName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the total number of quests in the chain.
    /// </summary>
    public int TotalQuests { get; init; }

    /// <summary>
    /// Gets the number of quests the player has completed in this chain.
    /// </summary>
    public int CompletedQuests { get; init; }

    /// <summary>
    /// Gets whether all quests in the chain are complete.
    /// </summary>
    public bool IsComplete => CompletedQuests >= TotalQuests;

    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public int CompletionPercent => TotalQuests == 0 ? 0 : (CompletedQuests * 100 / TotalQuests);

    /// <summary>
    /// Gets the quest ID of the next available (or locked) quest in the chain.
    /// Null if the chain is fully complete.
    /// </summary>
    public string? NextQuestId { get; init; }

    /// <summary>
    /// Gets the display name of the next quest in the chain.
    /// Null if the chain is fully complete.
    /// </summary>
    public string? NextQuestName { get; init; }

    /// <summary>
    /// Gets whether the next quest is locked behind a reputation gate
    /// that the player hasn't reached yet.
    /// </summary>
    public bool IsNextQuestReputationLocked { get; init; }

    /// <summary>
    /// Gets the minimum reputation required for the next quest.
    /// Null if the chain is complete or the next quest has no reputation requirement.
    /// </summary>
    public int? NextQuestReputationRequired { get; init; }

    /// <summary>
    /// Gets the player's current reputation value with this chain's faction.
    /// </summary>
    public int PlayerReputation { get; init; }
}
