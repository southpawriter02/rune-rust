using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single Quarry Mark placed on a target by a Veiðimaðr (Hunter).
/// Contains all metadata about the mark including target identity, hit bonuses,
/// duration tracking, and current status.
/// </summary>
/// <remarks>
/// <para>Each Quarry Mark provides a base +2 hit bonus against the marked target.
/// Additional bonuses may be added by Tier 2+ abilities (e.g., Predator's Patience +3).</para>
/// <para>Marks are created via the <see cref="Create"/> factory method and managed by
/// <see cref="QuarryMarksResource"/> which enforces the maximum of 3 simultaneous marks
/// with FIFO replacement policy.</para>
/// <para>Mutable state fields (<see cref="Status"/>, <see cref="TurnsActive"/>) use
/// <c>private set</c> with dedicated mutation methods, following the <see cref="RageResource"/>
/// mutable sealed record pattern.</para>
/// <para>Introduced in v0.20.7a as part of the Veiðimaðr specialization framework.</para>
/// </remarks>
public sealed record QuarryMark
{
    /// <summary>
    /// Default hit bonus provided by a Quarry Mark (+2 to all attack rolls against target).
    /// </summary>
    public const int DefaultHitBonus = 2;

    /// <summary>
    /// Unique identifier for this specific mark instance.
    /// </summary>
    public Guid MarkId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Unique identifier of the marked target entity.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the marked target (for UI and combat log output).
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp of when this mark was created.
    /// Used for FIFO ordering (oldest mark replaced first when at capacity).
    /// </summary>
    public DateTime MarkedAt { get; init; }

    /// <summary>
    /// Unique identifier of the hunter who placed this mark.
    /// </summary>
    public Guid MarkedBy { get; init; }

    /// <summary>
    /// Current lifecycle status of the mark.
    /// Transitions: Active → Defeated/Escaped/Expired → (mark removed).
    /// </summary>
    /// <seealso cref="SetStatus"/>
    public QuarryStatus Status { get; private set; } = QuarryStatus.Active;

    /// <summary>
    /// Base hit bonus this mark provides to the hunter against the target.
    /// Default is +2 to all attack rolls.
    /// </summary>
    public int HitBonus { get; init; } = DefaultHitBonus;

    /// <summary>
    /// Encounter ID if the mark was created during combat; null if marked outside combat.
    /// Used for mark lifecycle management (all marks clear at encounter end).
    /// </summary>
    public Guid? EncounterId { get; init; }

    /// <summary>
    /// Number of complete turns this mark has been active since creation.
    /// Incremented each turn via <see cref="IncrementTurn"/>.
    /// Used by Tier 2+ abilities for duration-based mechanics.
    /// </summary>
    /// <seealso cref="IncrementTurn"/>
    public int TurnsActive { get; private set; }

    /// <summary>
    /// Additional hit bonuses applied from other sources (e.g., Tier 2 abilities).
    /// Key format: source identifier (e.g., "predators-patience"), Value: bonus amount.
    /// </summary>
    public IReadOnlyDictionary<string, int> AdditionalBonuses { get; private set; }
        = new Dictionary<string, int>();

    /// <summary>
    /// Creates a new Quarry Mark with validated properties.
    /// </summary>
    /// <param name="targetId">Unique identifier of the target to mark. Must not be <see cref="Guid.Empty"/>.</param>
    /// <param name="targetName">Display name of the target. Must not be null or whitespace.</param>
    /// <param name="markedBy">Unique identifier of the hunter placing the mark. Must not be <see cref="Guid.Empty"/>.</param>
    /// <param name="encounterId">Optional encounter ID if marking during combat.</param>
    /// <returns>A new <see cref="QuarryMark"/> instance with Active status and 0 turns active.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetId"/> or <paramref name="markedBy"/> is <see cref="Guid.Empty"/>, or when <paramref name="targetName"/> is null/whitespace.</exception>
    public static QuarryMark Create(Guid targetId, string targetName, Guid markedBy, Guid? encounterId = null)
    {
        if (targetId == Guid.Empty)
            throw new ArgumentException("Target ID must not be empty.", nameof(targetId));
        if (string.IsNullOrWhiteSpace(targetName))
            throw new ArgumentException("Target name must not be null or whitespace.", nameof(targetName));
        if (markedBy == Guid.Empty)
            throw new ArgumentException("Marked-by ID must not be empty.", nameof(markedBy));

        return new QuarryMark
        {
            MarkId = Guid.NewGuid(),
            TargetId = targetId,
            TargetName = targetName,
            MarkedAt = DateTime.UtcNow,
            MarkedBy = markedBy,
            Status = QuarryStatus.Active,
            HitBonus = DefaultHitBonus,
            EncounterId = encounterId,
            TurnsActive = 0,
            AdditionalBonuses = new Dictionary<string, int>()
        };
    }

    /// <summary>
    /// Calculates the total hit bonus including base bonus and all additional bonuses.
    /// </summary>
    /// <returns>Sum of <see cref="HitBonus"/> and all values in <see cref="AdditionalBonuses"/>.</returns>
    public int GetTotalHitBonus()
    {
        var total = HitBonus;
        foreach (var bonus in AdditionalBonuses.Values)
        {
            total += bonus;
        }
        return total;
    }

    /// <summary>
    /// Returns a human-readable description of this mark for combat log display.
    /// </summary>
    /// <returns>A formatted string such as "Marked: Corrupted Wolf (+2 to hit, 3 turns)".</returns>
    public string GetDescription()
    {
        var totalBonus = GetTotalHitBonus();
        return $"Marked: {TargetName} (+{totalBonus} to hit, {TurnsActive} turns)";
    }

    /// <summary>
    /// Checks whether this mark has expired and should be auto-cleared.
    /// In v0.20.7a, marks do not expire by turn count — expiration mechanics
    /// are introduced in Tier 2 (v0.20.7b).
    /// </summary>
    /// <returns>False in v0.20.7a. Future tiers may implement turn-based expiration.</returns>
    public bool IsExpired()
    {
        // v0.20.7a: Marks persist until target defeated or encounter ends.
        // Tier 2+ abilities may add turn-based expiration.
        return Status == QuarryStatus.Expired;
    }

    /// <summary>
    /// Increments the turn counter by 1. Called at the start of each new turn
    /// via <see cref="QuarryMarksResource.RefreshMarksForNewTurn"/>.
    /// </summary>
    public void IncrementTurn()
    {
        TurnsActive++;
    }

    /// <summary>
    /// Updates the mark's lifecycle status (e.g., when target is defeated or escapes).
    /// </summary>
    /// <param name="status">The new status to assign.</param>
    public void SetStatus(QuarryStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// Adds a bonus from an external source (e.g., Tier 2 ability like Predator's Patience).
    /// </summary>
    /// <param name="source">Identifier for the bonus source (e.g., "predators-patience").</param>
    /// <param name="value">The bonus amount to add.</param>
    public void AddBonus(string source, int value)
    {
        var dict = new Dictionary<string, int>(AdditionalBonuses)
        {
            [source] = value
        };
        AdditionalBonuses = dict;
    }
}
