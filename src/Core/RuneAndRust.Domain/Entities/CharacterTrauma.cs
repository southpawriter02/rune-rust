// ═══════════════════════════════════════════════════════════════════════════════
// CharacterTrauma.cs
// Per-character trauma instance tracking entity.
// Version: 0.18.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents a trauma instance for a specific character.
/// </summary>
/// <remarks>
/// <para>
/// CharacterTrauma tracks the acquisition and progression of a specific trauma
/// on a character. Traumas are permanent, but their effects can be suppressed
/// through therapy or management (to be implemented in v0.22.x).
/// </para>
/// <para>
/// For stackable traumas, StackCount tracks the number of times the trauma
/// has been acquired or intensified. For non-stackable traumas, StackCount
/// still tracks event intensity or number of near-misses.
/// </para>
/// <para>
/// Constraints:
/// <list type="bullet">
///   <item><description>No duplicate traumas: UNIQUE (CharacterId, TraumaDefinitionId)</description></item>
///   <item><description>StackCount >= 1 always</description></item>
///   <item><description>ManagedSince >= AcquiredAt if set</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var trauma = CharacterTrauma.Create(
///     characterId: characterId,
///     traumaDefinitionId: "survivors-guilt",
///     source: "AllyDeath",
///     acquiredAt: DateTime.UtcNow
/// );
/// </code>
/// </example>
public class CharacterTrauma
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Constants
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maximum allowed stack count for trauma instances.
    /// </summary>
    /// <remarks>
    /// Arbitrary safety limit to prevent data corruption scenarios.
    /// </remarks>
    private const int MaxStackCount = 100;

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the unique identifier for this CharacterTrauma instance.
    /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    /// Gets the character who has acquired this trauma.
    /// </summary>
    /// <remarks>
    /// Foreign key to Character aggregate root.
    /// </remarks>
    public Guid CharacterId { get; private init; }

    /// <summary>
    /// Gets the trauma definition ID this instance is based on.
    /// </summary>
    /// <remarks>
    /// Foreign key to TraumaDefinition. String ID matches config key (e.g., "survivors-guilt").
    /// </remarks>
    public string TraumaDefinitionId { get; private init; }

    /// <summary>
    /// Gets the date/time when this trauma was first acquired.
    /// </summary>
    /// <remarks>
    /// Immutable. Set at creation time. Represents the original traumatic event.
    /// </remarks>
    public DateTime AcquiredAt { get; private init; }

    /// <summary>
    /// Gets the source/trigger that caused this trauma.
    /// </summary>
    /// <remarks>
    /// Examples: "AllyDeath", "CorruptionThreshold75", "ForlornContact".
    /// Used for narrative and mechanical understanding of origin.
    /// </remarks>
    public string Source { get; private init; }

    /// <summary>
    /// Gets the number of times this trauma has been reinforced.
    /// </summary>
    /// <remarks>
    /// <para>For stackable traumas: Number of acquisitions.</para>
    /// <para>For non-stackable traumas: Intensity/near-miss counter.</para>
    /// <para>Always >= 1.</para>
    /// </remarks>
    public int StackCount { get; private set; }

    /// <summary>
    /// Gets whether this trauma is currently applying its effects.
    /// </summary>
    /// <remarks>
    /// <para>Traumas are always acquired as IsActive=true.</para>
    /// <para>Future systems (v0.22.x) may allow suppression via therapy.</para>
    /// </remarks>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the date when trauma management/therapy began (if any).
    /// </summary>
    /// <remarks>
    /// <para>Null if trauma has not begun being managed.</para>
    /// <para>Must be >= AcquiredAt if set.</para>
    /// </remarks>
    public DateTime? ManagedSince { get; private set; }

    /// <summary>
    /// Gets optional campaign-specific notes about this trauma.
    /// </summary>
    /// <remarks>
    /// Used by GMs to track special circumstances or modifications.
    /// </remarks>
    public string? Notes { get; private set; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructors
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private CharacterTrauma()
    {
        TraumaDefinitionId = null!;
        Source = null!;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new CharacterTrauma instance.
    /// </summary>
    /// <param name="characterId">The character acquiring the trauma.</param>
    /// <param name="traumaDefinitionId">The trauma definition ID (e.g., "survivors-guilt").</param>
    /// <param name="source">The source/trigger of the trauma (e.g., "AllyDeath").</param>
    /// <param name="acquiredAt">When the trauma was acquired.</param>
    /// <returns>A new CharacterTrauma instance with StackCount=1 and IsActive=true.</returns>
    /// <exception cref="ArgumentException">If characterId is empty Guid.</exception>
    /// <exception cref="ArgumentException">If traumaDefinitionId is null or whitespace.</exception>
    /// <exception cref="ArgumentException">If source is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// var trauma = CharacterTrauma.Create(
    ///     characterId: playerId,
    ///     traumaDefinitionId: "reality-doubt",
    ///     source: "WitnessingHorror",
    ///     acquiredAt: DateTime.UtcNow
    /// );
    /// </code>
    /// </example>
    public static CharacterTrauma Create(
        Guid characterId,
        string traumaDefinitionId,
        string source,
        DateTime acquiredAt)
    {
        // Validate characterId
        if (characterId == Guid.Empty)
        {
            throw new ArgumentException("CharacterId cannot be empty.", nameof(characterId));
        }

        // Validate traumaDefinitionId
        if (string.IsNullOrWhiteSpace(traumaDefinitionId))
        {
            throw new ArgumentException("TraumaDefinitionId cannot be null or empty.", nameof(traumaDefinitionId));
        }

        // Validate source
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source cannot be null or empty.", nameof(source));
        }

        return new CharacterTrauma
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            TraumaDefinitionId = traumaDefinitionId.ToLowerInvariant(),
            AcquiredAt = acquiredAt,
            Source = source,
            StackCount = 1,
            IsActive = true,
            ManagedSince = null,
            Notes = null
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Instance Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Increments the stack count for this trauma.
    /// </summary>
    /// <remarks>
    /// Called when a stackable trauma is acquired multiple times.
    /// Stack count is capped at <see cref="MaxStackCount"/> for safety.
    /// </remarks>
    /// <exception cref="InvalidOperationException">If stack count would exceed maximum.</exception>
    /// <example>
    /// <code>
    /// // Reality Doubt stacking up (triggers retirement at 5+)
    /// trauma.IncrementStackCount(); // Now x2
    /// trauma.IncrementStackCount(); // Now x3
    /// </code>
    /// </example>
    public void IncrementStackCount()
    {
        if (StackCount >= MaxStackCount)
        {
            throw new InvalidOperationException(
                $"Cannot increment StackCount beyond {MaxStackCount}.");
        }

        StackCount++;
    }

    /// <summary>
    /// Marks this trauma as being actively managed/treated.
    /// </summary>
    /// <param name="managedSince">When management started.</param>
    /// <remarks>
    /// <para>Can only be set to a date >= AcquiredAt.</para>
    /// <para>Used by therapy systems (future implementation v0.22.x).</para>
    /// </remarks>
    /// <exception cref="ArgumentException">If managedSince is before AcquiredAt.</exception>
    /// <example>
    /// <code>
    /// // Character begins therapy after acquiring trauma
    /// trauma.SetManagementStart(DateTime.UtcNow);
    /// </code>
    /// </example>
    public void SetManagementStart(DateTime managedSince)
    {
        if (managedSince < AcquiredAt)
        {
            throw new ArgumentException(
                "Management cannot start before trauma acquisition.",
                nameof(managedSince));
        }

        ManagedSince = managedSince;
    }

    /// <summary>
    /// Updates the campaign notes for this trauma.
    /// </summary>
    /// <param name="notes">New notes content (null to clear).</param>
    /// <example>
    /// <code>
    /// trauma.SetNotes("Acquired during the Siege of Thornhold");
    /// </code>
    /// </example>
    public void SetNotes(string? notes)
    {
        Notes = notes;
    }

    /// <summary>
    /// Toggles whether this trauma is actively applying effects.
    /// </summary>
    /// <param name="active">True to enable effects, false to suppress.</param>
    /// <remarks>
    /// <para>IsActive can be false if trauma is managed/suppressed.</para>
    /// <para>Always true for newly acquired traumas.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Suppress trauma effects during therapy
    /// trauma.SetActive(false);
    /// </code>
    /// </example>
    public void SetActive(bool active)
    {
        IsActive = active;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ToString
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the display string for this trauma instance.
    /// </summary>
    /// <returns>A formatted string showing trauma ID, stack count, and management status.</returns>
    /// <example>
    /// Output examples:
    /// <code>
    /// "[survivors-guilt] x1"
    /// "[reality-doubt] x5 [MANAGED] (since 2026-01-15)"
    /// </code>
    /// </example>
    public override string ToString()
    {
        var result = $"[{TraumaDefinitionId}] x{StackCount}";

        if (!IsActive)
        {
            result += " [MANAGED]";
        }

        if (ManagedSince.HasValue)
        {
            result += $" (since {ManagedSince:yyyy-MM-dd})";
        }

        return result;
    }
}
