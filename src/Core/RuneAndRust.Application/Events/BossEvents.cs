namespace RuneAndRust.Application.Events;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Published when a boss is spawned into combat.
/// </summary>
/// <remarks>
/// <para>This event is raised when the boss mechanics service spawns a boss monster
/// from a boss definition.</para>
/// <para>The event contains the spawned monster's ID, the boss definition ID, and
/// optional title text for dramatic UI display.</para>
/// </remarks>
/// <param name="MonsterId">The unique identifier of the spawned boss monster instance.</param>
/// <param name="BossId">The boss definition ID (e.g., "skeleton-king").</param>
/// <param name="TitleText">Optional dramatic title text (e.g., "The Skeleton King Awakens!").</param>
public record BossSpawnedEvent(
    Guid MonsterId,
    string BossId,
    string? TitleText)
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether this boss has title text to display.
    /// </summary>
    public bool HasTitleText => !string.IsNullOrEmpty(TitleText);
}

/// <summary>
/// Published when a boss transitions to a new phase.
/// </summary>
/// <remarks>
/// <para>This event is raised when damage to a boss causes its health to drop
/// below a phase threshold, triggering a phase transition.</para>
/// <para>Phase transitions are one-way (phases only increase, never decrease)
/// and may include dramatic transition effects and text.</para>
/// </remarks>
/// <param name="MonsterId">The boss monster instance ID.</param>
/// <param name="BossId">The boss definition ID.</param>
/// <param name="OldPhaseNumber">The previous phase number (1-based).</param>
/// <param name="NewPhaseNumber">The new phase number (1-based).</param>
/// <param name="PhaseName">The new phase's display name (e.g., "Enraged").</param>
/// <param name="TransitionText">Optional dramatic text for the transition (e.g., "The Skeleton King raises his arms!").</param>
public record BossPhaseChangedEvent(
    Guid MonsterId,
    string BossId,
    int OldPhaseNumber,
    int NewPhaseNumber,
    string PhaseName,
    string? TransitionText)
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether this phase transition has dramatic text to display.
    /// </summary>
    public bool HasTransitionText => !string.IsNullOrEmpty(TransitionText);
}

/// <summary>
/// Published when a boss becomes vulnerable to increased damage.
/// </summary>
/// <remarks>
/// <para>This event is raised when a boss enters a vulnerability window,
/// typically after exhausting a major ability or during specific mechanics.</para>
/// <para>While vulnerable, the boss takes increased damage (default 1.5x multiplier).</para>
/// </remarks>
/// <param name="MonsterId">The boss monster instance ID.</param>
/// <param name="DurationTurns">Number of turns the vulnerability lasts.</param>
public record BossVulnerableEvent(
    Guid MonsterId,
    int DurationTurns)
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Published when a boss's vulnerability window ends.
/// </summary>
/// <remarks>
/// <para>This event is raised when a boss's <see cref="Tracking.ActiveBossState.VulnerableTurns"/>
/// countdown reaches zero.</para>
/// <para>After this event, the boss returns to normal combat state with standard damage taken.</para>
/// </remarks>
/// <param name="MonsterId">The boss monster instance ID.</param>
public record BossVulnerabilityEndedEvent(
    Guid MonsterId)
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Published when a boss is defeated.
/// </summary>
/// <remarks>
/// <para>This event is raised when a boss's health reaches zero or below.</para>
/// <para>The event includes the boss's loot table entries for processing by
/// the loot system to generate drops.</para>
/// </remarks>
/// <param name="MonsterId">The boss monster instance ID.</param>
/// <param name="BossId">The boss definition ID.</param>
/// <param name="LootEntries">The loot table entries for drop processing.</param>
public record BossDefeatedEvent(
    Guid MonsterId,
    string BossId,
    IReadOnlyList<BossLootEntry> LootEntries)
{
    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether this boss has any loot to drop.
    /// </summary>
    public bool HasLoot => LootEntries.Count > 0;
}
