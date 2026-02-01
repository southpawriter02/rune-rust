// ═══════════════════════════════════════════════════════════════════════════════
// CorruptionHistoryEntry.cs
// Domain entity representing a single corruption change event in a character's
// history. Records all corruption additions, removals, and transfers for
// analytics, debugging, and UI display, capturing the corruption amount, source
// category, before/after totals, threshold crossings, and transfer details.
// Part of the Database Persistence layer for the Runic Blight Corruption system.
// Version: 0.18.1e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Entity representing a single corruption change event in a character's history.
/// </summary>
/// <remarks>
/// <para>
/// Records all corruption changes for analytics, debugging, and history display.
/// Each entry captures the complete context of a corruption event:
/// </para>
/// <list type="bullet">
/// <item><description>Corruption amount and source category (e.g., HereticalAbility, Environmental).</description></item>
/// <item><description>New corruption total after the change was applied.</description></item>
/// <item><description>Threshold crossing value (25, 50, 75, or 100) if a milestone was reached.</description></item>
/// <item><description>Transfer details: whether this was a Blot-Priest corruption transfer and the target character.</description></item>
/// <item><description>UTC timestamp for chronological ordering and session analysis.</description></item>
/// </list>
/// <para>
/// <strong>Creation:</strong> Use <see cref="FromAddResult"/> to create entries from
/// <see cref="CorruptionAddResult"/> objects returned by <c>CorruptionTracker.AddCorruption</c>,
/// or <see cref="Create"/> for manual entry construction (e.g., for transfers or removals).
/// </para>
/// <para>
/// <strong>Persistence:</strong> Entries are stored via <c>ICorruptionRepository.AddHistoryEntryAsync</c>
/// and mapped to the <c>CorruptionHistory</c> database table via EF Core configuration.
/// A composite index on <c>(CharacterId, CreatedAt)</c> supports efficient per-character
/// history queries ordered by most recent first.
/// </para>
/// <para>
/// <strong>Immutability:</strong> All properties have private setters. Once created,
/// a history entry cannot be modified — it represents a factual record of a past event.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create from an AddCorruption result
/// var result = tracker.AddCorruption(15, CorruptionSource.HereticalAbility);
/// var entry = CorruptionHistoryEntry.FromAddResult(characterId, result);
/// await repository.AddHistoryEntryAsync(entry);
///
/// // Create manually for a transfer event
/// var transfer = CorruptionHistoryEntry.Create(
///     characterId: sourceId,
///     amount: -10,
///     source: CorruptionSource.BlightTransfer,
///     newTotal: 40,
///     thresholdCrossed: null,
///     isTransfer: true,
///     transferTargetId: targetId);
/// </code>
/// </example>
/// <seealso cref="CorruptionAddResult"/>
/// <seealso cref="CorruptionSource"/>
/// <seealso cref="CorruptionTracker"/>
public class CorruptionHistoryEntry : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this history entry.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this corruption event record.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the character this corruption event applies to.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> of the character (Player entity) who experienced this corruption event.
    /// Used as a foreign key for per-character history queries.
    /// </value>
    public Guid CharacterId { get; private set; }

    /// <summary>
    /// Gets the corruption amount changed in this event.
    /// </summary>
    /// <value>
    /// The corruption delta for this event. Positive values indicate corruption gained;
    /// negative values indicate corruption removed (extremely rare) or transferred away.
    /// </value>
    /// <remarks>
    /// For standard corruption additions, this matches the amount passed to
    /// <see cref="CorruptionTracker.AddCorruption"/>. For transfers, the source character's
    /// entry has a negative amount and the target character's entry has a positive amount.
    /// The actual change may be less than requested if the corruption was clamped at 0 or 100.
    /// </remarks>
    public int Amount { get; private set; }

    /// <summary>
    /// Gets the source category of the corruption event.
    /// </summary>
    /// <value>
    /// The <see cref="CorruptionSource"/> enum value categorizing the origin of this corruption
    /// (e.g., MysticMagic, HereticalAbility, Environmental, BlightTransfer).
    /// </value>
    public CorruptionSource Source { get; private set; }

    /// <summary>
    /// Gets the character's corruption total after this event was applied.
    /// </summary>
    /// <value>
    /// The character's <see cref="CorruptionTracker.CurrentCorruption"/> immediately after
    /// this corruption event was applied, clamped to [0, 100].
    /// </value>
    public int NewTotal { get; private set; }

    /// <summary>
    /// Gets the corruption threshold that was crossed, if any.
    /// </summary>
    /// <value>
    /// The threshold value (25, 50, 75, or 100) that was crossed as a result of this
    /// corruption event; <c>null</c> if no threshold boundary was crossed. Thresholds
    /// are one-time triggers — once crossed, they are not reported again.
    /// </value>
    /// <remarks>
    /// <para>
    /// Threshold crossings trigger specific effects:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Threshold</term>
    /// <description>Effect</description>
    /// </listheader>
    /// <item><term>25</term><description>UI warning indicator displayed</description></item>
    /// <item><term>50</term><description>Human faction reputation gains locked</description></item>
    /// <item><term>75</term><description>Acquire [MACHINE AFFINITY] trauma</description></item>
    /// <item><term>100</term><description>Terminal Error check triggered</description></item>
    /// </list>
    /// </remarks>
    public int? ThresholdCrossed { get; private set; }

    /// <summary>
    /// Gets whether this event was a Blot-Priest corruption transfer.
    /// </summary>
    /// <value>
    /// <c>true</c> if this corruption change resulted from a Blot-Priest corruption transfer
    /// between characters; <c>false</c> for standard corruption additions, removals, or
    /// other sources.
    /// </value>
    public bool IsTransfer { get; private set; }

    /// <summary>
    /// Gets the target character ID for transfer events.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> of the character receiving corruption in a Blot-Priest transfer,
    /// or <c>null</c> if this was not a transfer event. For the source character's history entry,
    /// this points to the receiving character. For the target character's entry, this is <c>null</c>
    /// (the source is implied by the <see cref="CorruptionSource.BlightTransfer"/> source).
    /// </value>
    public Guid? TransferTargetId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this corruption event occurred.
    /// </summary>
    /// <value>
    /// The <see cref="DateTime"/> in UTC when this corruption event was recorded.
    /// Used for chronological ordering and session analytics.
    /// </value>
    public DateTime CreatedAt { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for Entity Framework Core materialization.
    /// </summary>
    /// <remarks>
    /// EF Core requires a parameterless constructor to create entity instances
    /// when loading from the database. Use <see cref="FromAddResult"/> or
    /// <see cref="Create"/> factory methods for application-level creation.
    /// </remarks>
    private CorruptionHistoryEntry()
    {
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new corruption history entry from a <see cref="CorruptionAddResult"/>.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character who experienced the corruption event.
    /// </param>
    /// <param name="result">
    /// The <see cref="CorruptionAddResult"/> returned by
    /// <see cref="CorruptionTracker.AddCorruption"/>, containing corruption change details.
    /// </param>
    /// <returns>
    /// A new <see cref="CorruptionHistoryEntry"/> populated from the add result,
    /// with a unique <see cref="Id"/> and <see cref="CreatedAt"/> set to <see cref="DateTime.UtcNow"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Property mapping from <see cref="CorruptionAddResult"/>:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>History Property</term>
    /// <description>Source</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Amount"/></term>
    /// <description><c>result.AmountGained</c> — the actual corruption delta after clamping.</description>
    /// </item>
    /// <item>
    /// <term><see cref="Source"/></term>
    /// <description><c>result.Source</c> — the corruption source category.</description>
    /// </item>
    /// <item>
    /// <term><see cref="NewTotal"/></term>
    /// <description><c>result.NewCorruption</c> — corruption total after the change.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ThresholdCrossed"/></term>
    /// <description><c>result.ThresholdCrossed</c> — threshold value if crossed, null otherwise.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = tracker.AddCorruption(15, CorruptionSource.HereticalAbility);
    /// var entry = CorruptionHistoryEntry.FromAddResult(characterId, result);
    /// await repository.AddHistoryEntryAsync(entry);
    /// </code>
    /// </example>
    /// <seealso cref="CorruptionAddResult"/>
    public static CorruptionHistoryEntry FromAddResult(
        Guid characterId,
        CorruptionAddResult result)
    {
        return new CorruptionHistoryEntry
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Amount = result.AmountGained,
            Source = result.Source,
            NewTotal = result.NewCorruption,
            ThresholdCrossed = result.ThresholdCrossed,
            IsTransfer = false,
            TransferTargetId = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new corruption history entry with explicit property values.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="amount">
    /// The corruption change amount. Positive for gains, negative for removals or transfers away.
    /// </param>
    /// <param name="source">The corruption source category.</param>
    /// <param name="newTotal">The corruption total after this change was applied.</param>
    /// <param name="thresholdCrossed">
    /// The threshold value (25, 50, 75, or 100) crossed, if any; <c>null</c> if no threshold
    /// boundary was crossed. Defaults to <c>null</c>.
    /// </param>
    /// <param name="isTransfer">
    /// Whether this event was a Blot-Priest corruption transfer. Defaults to <c>false</c>.
    /// </param>
    /// <param name="transferTargetId">
    /// The target character ID for transfer events; <c>null</c> for non-transfer events.
    /// Defaults to <c>null</c>.
    /// </param>
    /// <returns>
    /// A new <see cref="CorruptionHistoryEntry"/> with a unique <see cref="Id"/> and
    /// <see cref="CreatedAt"/> set to <see cref="DateTime.UtcNow"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// // Standard corruption gain
    /// var entry = CorruptionHistoryEntry.Create(
    ///     characterId, amount: 15, source: CorruptionSource.Environmental,
    ///     newTotal: 45, thresholdCrossed: null);
    ///
    /// // Transfer event (source side)
    /// var transferEntry = CorruptionHistoryEntry.Create(
    ///     characterId: sourceId, amount: -10, source: CorruptionSource.BlightTransfer,
    ///     newTotal: 30, isTransfer: true, transferTargetId: targetId);
    /// </code>
    /// </example>
    public static CorruptionHistoryEntry Create(
        Guid characterId,
        int amount,
        CorruptionSource source,
        int newTotal,
        int? thresholdCrossed = null,
        bool isTransfer = false,
        Guid? transferTargetId = null)
    {
        return new CorruptionHistoryEntry
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Amount = amount,
            Source = source,
            NewTotal = newTotal,
            ThresholdCrossed = thresholdCrossed,
            IsTransfer = isTransfer,
            TransferTargetId = transferTargetId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this corruption history entry for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the character ID, source, corruption amount, new total,
    /// and threshold/transfer details.
    /// </returns>
    /// <example>
    /// <code>
    /// var entry = CorruptionHistoryEntry.Create(charId, 15, CorruptionSource.HereticalAbility, 45, 25);
    /// var display = entry.ToString();
    /// // Returns "CorruptionHistory[charId]: HereticalAbility +15 → 45 [Threshold: 25]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"CorruptionHistory[{CharacterId:N}]: {Source} {Amount:+#;-#;0} → {NewTotal}" +
        $"{(ThresholdCrossed.HasValue ? $" [Threshold: {ThresholdCrossed.Value}]" : "")}" +
        $"{(IsTransfer ? $" [Transfer → {TransferTargetId:N}]" : "")}";
}
