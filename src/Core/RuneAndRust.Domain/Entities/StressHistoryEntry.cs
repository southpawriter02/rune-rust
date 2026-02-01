// ═══════════════════════════════════════════════════════════════════════════════
// StressHistoryEntry.cs
// Domain entity representing a single stress event in a character's history.
// Records all stress applications for analytics, debugging, and UI display,
// capturing original stress amount, resistance check details, before/after
// stress values, and threshold crossings.
// Part of the Database Persistence layer for the Trauma Economy system.
// Version: 0.18.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Entity representing a single stress event in a character's history.
/// </summary>
/// <remarks>
/// <para>
/// Records all stress applications for analytics, debugging, and history display.
/// Each entry captures the complete context of a stress event:
/// </para>
/// <list type="bullet">
/// <item><description>Original stress amount and source category before any resistance.</description></item>
/// <item><description>Resistance check details: DC, whether resistance succeeded, reduction applied.</description></item>
/// <item><description>Before/after stress values showing the actual state change.</description></item>
/// <item><description>Threshold crossings for tracking psychological state transitions.</description></item>
/// <item><description>UTC timestamp for chronological ordering and session analysis.</description></item>
/// </list>
/// <para>
/// <strong>Creation:</strong> Use <see cref="FromApplicationResult"/> to create entries from
/// <see cref="StressApplicationResult"/> objects returned by <c>IStressService.ApplyStress</c>,
/// or <see cref="Create"/> for manual entry construction.
/// </para>
/// <para>
/// <strong>Persistence:</strong> Entries are stored via <c>IStressHistoryRepository</c> and
/// mapped to the <c>StressHistory</c> database table via EF Core configuration.
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
/// // Create from an ApplyStress result
/// var result = stressService.ApplyStress(characterId, 20, StressSource.Combat);
/// var entry = StressHistoryEntry.FromApplicationResult(characterId, result);
/// await repository.AddAsync(entry);
///
/// // Create manually
/// var manual = StressHistoryEntry.Create(
///     characterId,
///     amount: 25,
///     source: StressSource.Heretical,
///     resistDc: 3,
///     resisted: true,
///     finalAmount: 6,
///     previousStress: 75,
///     newStress: 81,
///     thresholdCrossed: StressThreshold.Breaking);
/// </code>
/// </example>
/// <seealso cref="StressApplicationResult"/>
/// <seealso cref="StressCheckResult"/>
/// <seealso cref="StressSource"/>
/// <seealso cref="StressThreshold"/>
public class StressHistoryEntry : IEntity
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this history entry.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies this stress event record.</value>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the character this stress event applies to.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> of the character (Player entity) who experienced this stress event.
    /// Used as a foreign key for per-character history queries.
    /// </value>
    public Guid CharacterId { get; private set; }

    /// <summary>
    /// Gets the original stress amount before any resistance reduction.
    /// </summary>
    /// <value>
    /// The base stress amount that was attempted. When a resistance check was performed,
    /// this is the <see cref="StressCheckResult.BaseStress"/> value. When no resistance
    /// was attempted, this equals <see cref="FinalAmount"/> (the actual stress gained).
    /// </value>
    /// <remarks>
    /// This value represents the "raw" stress from the source, before WILL-based
    /// resistance reduces it. Compare with <see cref="FinalAmount"/> to see the
    /// effectiveness of resistance checks.
    /// </remarks>
    public int Amount { get; private set; }

    /// <summary>
    /// Gets the source category of the stress event.
    /// </summary>
    /// <value>
    /// The <see cref="StressSource"/> enum value categorizing the origin of this stress
    /// (e.g., Combat, Exploration, Narrative, Heretical, Environmental, Corruption).
    /// </value>
    public StressSource Source { get; private set; }

    /// <summary>
    /// Gets the resistance DC if a resistance check was attempted.
    /// </summary>
    /// <value>
    /// The number of resistance successes rolled when a WILL-based resistance check was
    /// performed; <c>null</c> if no resistance check was allowed for this stress event
    /// (e.g., unavoidable Narrative or Corruption stress).
    /// </value>
    /// <remarks>
    /// <para>
    /// This stores the number of successes from the resistance check result, since the
    /// original resist DC value is not captured on <see cref="StressApplicationResult"/>.
    /// The successes determine the reduction percentage via the resistance reduction table:
    /// 0 → 0%, 1 → 50%, 2-3 → 75%, 4+ → 100%.
    /// </para>
    /// </remarks>
    public int? ResistDc { get; private set; }

    /// <summary>
    /// Gets whether any stress was reduced by a resistance check.
    /// </summary>
    /// <value>
    /// <c>true</c> if a resistance check was performed and the reduction percentage
    /// was greater than zero; otherwise, <c>false</c>.
    /// </value>
    public bool Resisted { get; private set; }

    /// <summary>
    /// Gets the final stress amount after any resistance reduction.
    /// </summary>
    /// <value>
    /// The actual stress gained by the character, after applying any resistance
    /// reduction. This is the delta between <see cref="NewStress"/> and
    /// <see cref="PreviousStress"/>.
    /// </value>
    /// <remarks>
    /// When <see cref="Resisted"/> is <c>true</c>, this will be less than <see cref="Amount"/>.
    /// When no resistance was applied, this equals <see cref="Amount"/>.
    /// May also differ from the base amount due to clamping at the maximum stress value (100).
    /// </remarks>
    public int FinalAmount { get; private set; }

    /// <summary>
    /// Gets the character's stress value before this event.
    /// </summary>
    /// <value>
    /// The character's <see cref="StressState.CurrentStress"/> immediately before
    /// this stress event was applied. Expected to be in the range [0, 100].
    /// </value>
    public int PreviousStress { get; private set; }

    /// <summary>
    /// Gets the character's stress value after this event.
    /// </summary>
    /// <value>
    /// The character's <see cref="StressState.CurrentStress"/> immediately after
    /// this stress event was applied, clamped to [0, 100].
    /// </value>
    public int NewStress { get; private set; }

    /// <summary>
    /// Gets the stress threshold that was crossed, if any.
    /// </summary>
    /// <value>
    /// The new <see cref="StressThreshold"/> tier the character entered as a result of
    /// this stress event; <c>null</c> if no threshold boundary was crossed (i.e., the
    /// character remained in the same stress tier).
    /// </value>
    /// <remarks>
    /// Threshold crossings trigger UI notifications, sound effects, and combat log entries.
    /// A value of <see cref="StressThreshold.Trauma"/> indicates the character reached
    /// maximum stress and a Trauma Check is required.
    /// </remarks>
    public StressThreshold? ThresholdCrossed { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this stress event occurred.
    /// </summary>
    /// <value>
    /// The <see cref="DateTime"/> in UTC when this stress event was recorded.
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
    /// when loading from the database. Use <see cref="FromApplicationResult"/> or
    /// <see cref="Create"/> factory methods for application-level creation.
    /// </remarks>
    private StressHistoryEntry()
    {
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new stress history entry from a <see cref="StressApplicationResult"/>.
    /// </summary>
    /// <param name="characterId">
    /// The unique identifier of the character who experienced the stress event.
    /// </param>
    /// <param name="result">
    /// The <see cref="StressApplicationResult"/> returned by
    /// <c>IStressService.ApplyStress</c>, containing all stress event details.
    /// </param>
    /// <returns>
    /// A new <see cref="StressHistoryEntry"/> populated from the application result,
    /// with a unique <see cref="Id"/> and <see cref="CreatedAt"/> set to <see cref="DateTime.UtcNow"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Property mapping from <see cref="StressApplicationResult"/>:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>History Property</term>
    /// <description>Source</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Amount"/></term>
    /// <description>
    /// <c>result.ResistanceResult?.BaseStress ?? result.StressGained</c> —
    /// the original stress before resistance, or the actual gain if no check was performed.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="ResistDc"/></term>
    /// <description>
    /// <c>result.ResistanceResult?.Successes</c> when resistance was attempted (non-null);
    /// <c>null</c> when no resistance check was performed.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="Resisted"/></term>
    /// <description>
    /// <c>true</c> when <c>result.ResistanceResult?.ReductionPercent &gt; 0</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="FinalAmount"/></term>
    /// <description><c>result.StressGained</c> — the actual stress delta.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ThresholdCrossed"/></term>
    /// <description>
    /// <c>result.NewThreshold</c> when <c>result.ThresholdCrossed</c> is <c>true</c>;
    /// <c>null</c> otherwise.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = stressService.ApplyStress(characterId, 20, StressSource.Combat, resistDc: 2);
    /// var entry = StressHistoryEntry.FromApplicationResult(characterId, result);
    /// await historyRepository.AddAsync(entry);
    /// </code>
    /// </example>
    /// <seealso cref="StressApplicationResult"/>
    /// <seealso cref="StressCheckResult"/>
    public static StressHistoryEntry FromApplicationResult(
        Guid characterId,
        StressApplicationResult result)
    {
        return new StressHistoryEntry
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Amount = result.ResistanceResult?.BaseStress ?? result.StressGained,
            Source = result.Source,
            ResistDc = result.ResistanceResult.HasValue
                ? result.ResistanceResult.Value.Successes
                : null,
            Resisted = result.ResistanceResult?.ReductionPercent > 0,
            FinalAmount = result.StressGained,
            PreviousStress = result.PreviousStress,
            NewStress = result.NewStress,
            ThresholdCrossed = result.ThresholdCrossed
                ? result.NewThreshold
                : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new stress history entry with explicit property values.
    /// </summary>
    /// <param name="characterId">The unique identifier of the character.</param>
    /// <param name="amount">The original stress amount before resistance.</param>
    /// <param name="source">The stress source category.</param>
    /// <param name="resistDc">
    /// The number of resistance successes if a check was attempted; <c>null</c> if no
    /// resistance check was allowed.
    /// </param>
    /// <param name="resisted">Whether any stress was reduced by resistance.</param>
    /// <param name="finalAmount">The final stress amount after resistance reduction.</param>
    /// <param name="previousStress">The character's stress before the event.</param>
    /// <param name="newStress">The character's stress after the event.</param>
    /// <param name="thresholdCrossed">
    /// The threshold tier crossed, if any; <c>null</c> if no threshold boundary was crossed.
    /// Defaults to <c>null</c>.
    /// </param>
    /// <returns>
    /// A new <see cref="StressHistoryEntry"/> with a unique <see cref="Id"/> and
    /// <see cref="CreatedAt"/> set to <see cref="DateTime.UtcNow"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var entry = StressHistoryEntry.Create(
    ///     characterId,
    ///     amount: 25,
    ///     source: StressSource.Heretical,
    ///     resistDc: 3,
    ///     resisted: true,
    ///     finalAmount: 6,
    ///     previousStress: 75,
    ///     newStress: 81,
    ///     thresholdCrossed: StressThreshold.Breaking);
    /// </code>
    /// </example>
    public static StressHistoryEntry Create(
        Guid characterId,
        int amount,
        StressSource source,
        int? resistDc,
        bool resisted,
        int finalAmount,
        int previousStress,
        int newStress,
        StressThreshold? thresholdCrossed = null)
    {
        return new StressHistoryEntry
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            Amount = amount,
            Source = source,
            ResistDc = resistDc,
            Resisted = resisted,
            FinalAmount = finalAmount,
            PreviousStress = previousStress,
            NewStress = newStress,
            ThresholdCrossed = thresholdCrossed,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation of this stress history entry for debugging and logging.
    /// </summary>
    /// <returns>
    /// A formatted string showing the character ID, source, stress transition, and
    /// resistance/threshold details.
    /// </returns>
    /// <example>
    /// <code>
    /// var entry = StressHistoryEntry.Create(charId, 25, StressSource.Combat, null, false, 25, 35, 60);
    /// var display = entry.ToString();
    /// // Returns "StressHistory[charId]: Combat 25→25 (35→60) [Threshold: Panicked]"
    /// </code>
    /// </example>
    public override string ToString() =>
        $"StressHistory[{CharacterId:N}]: {Source} {Amount}→{FinalAmount} " +
        $"({PreviousStress}→{NewStress})" +
        $"{(ThresholdCrossed.HasValue ? $" [Threshold: {ThresholdCrossed.Value}]" : "")}" +
        $"{(Resisted ? " [Resisted]" : "")}";
}
