// ═══════════════════════════════════════════════════════════════════════════════
// DispelEffectResult.cs
// Immutable value object reporting the results of a dispel action, such as the
// Word of Unmaking capstone ability. Tracks effects removed, entities destroyed,
// items affected, and characters impacted.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Reports the results of a dispel action (e.g., Word of Unmaking).
/// </summary>
/// <remarks>
/// <para>
/// DispelEffectResult is created after a dispel ability is executed, aggregating
/// all effects that were removed from the affected area. It provides a complete
/// audit trail of what the dispel accomplished.
/// </para>
/// <para>
/// Categories tracked:
/// </para>
/// <list type="bullet">
///   <item><description><b>Effects Removed:</b> Names of buff/debuff status effects stripped</description></item>
///   <item><description><b>Entities Destroyed:</b> IDs of summoned creatures destroyed</description></item>
///   <item><description><b>Items Affected:</b> IDs of items with temporary enchantments removed</description></item>
///   <item><description><b>Affected Characters:</b> IDs of characters who had effects removed</description></item>
/// </list>
/// <para>
/// This is an immutable value object — once created, it cannot be modified.
/// Use the <see cref="Create"/> factory method to build a result, or
/// <see cref="Empty"/> for a no-op result.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var result = DispelEffectResult.Create(
///     effectsRemoved: new[] { "Shield of Faith", "Bless" },
///     entitiesDestroyed: new[] { summonId },
///     itemsAffected: Array.Empty&lt;Guid&gt;(),
///     affectedCharacters: new[] { charId1, charId2 });
///
/// // result.TotalEffectsDispelled = 4
/// // result.DestroyedEntities = true
/// </code>
/// </example>
/// <seealso cref="LivingRuneEntity"/>
/// <seealso cref="RuneAndRust.Domain.Enums.RunasmidrAbilityId"/>
public sealed record DispelEffectResult
{
    // ═══════ Properties ═══════

    /// <summary>
    /// Gets the names of status effects that were removed by the dispel.
    /// </summary>
    /// <remarks>
    /// Includes both buff and debuff effects (e.g., "Shield of Faith",
    /// "Poison", "Bless", "Curse").
    /// </remarks>
    public IReadOnlyList<string> EffectsRemoved { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the IDs of summoned entities that were destroyed by the dispel.
    /// </summary>
    /// <remarks>
    /// Includes all summoned creatures in the area, including the caster's
    /// own Living Runes.
    /// </remarks>
    public IReadOnlyList<Guid> EntitiesDestroyed { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the IDs of items that had temporary enchantments removed.
    /// </summary>
    /// <remarks>
    /// Only temporary enchantments are removed; permanent magical properties
    /// are preserved.
    /// </remarks>
    public IReadOnlyList<Guid> ItemsAffected { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the IDs of characters who had effects removed from them.
    /// </summary>
    public IReadOnlyList<Guid> AffectedCharacters { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets the total count of all effects dispelled across all categories.
    /// </summary>
    /// <remarks>
    /// Sum of <see cref="EffectsRemoved"/>.Count + <see cref="EntitiesDestroyed"/>.Count +
    /// <see cref="ItemsAffected"/>.Count.
    /// </remarks>
    public int TotalEffectsDispelled { get; init; }

    /// <summary>
    /// Gets the timestamp when the dispel occurred.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    // ═══════ Computed Properties ═══════

    /// <summary>
    /// Gets whether any summoned entities were destroyed by the dispel.
    /// </summary>
    public bool DestroyedEntities => EntitiesDestroyed.Count > 0;

    // ═══════ Factory Methods ═══════

    /// <summary>
    /// Creates a new DispelEffectResult with the specified effects.
    /// </summary>
    /// <param name="effectsRemoved">Names of status effects removed.</param>
    /// <param name="entitiesDestroyed">IDs of summoned entities destroyed.</param>
    /// <param name="itemsAffected">IDs of items with enchantments removed.</param>
    /// <param name="affectedCharacters">IDs of characters affected.</param>
    /// <returns>A new DispelEffectResult with computed totals.</returns>
    public static DispelEffectResult Create(
        IReadOnlyList<string> effectsRemoved,
        IReadOnlyList<Guid> entitiesDestroyed,
        IReadOnlyList<Guid> itemsAffected,
        IReadOnlyList<Guid> affectedCharacters)
    {
        ArgumentNullException.ThrowIfNull(effectsRemoved);
        ArgumentNullException.ThrowIfNull(entitiesDestroyed);
        ArgumentNullException.ThrowIfNull(itemsAffected);
        ArgumentNullException.ThrowIfNull(affectedCharacters);

        return new DispelEffectResult
        {
            EffectsRemoved = effectsRemoved,
            EntitiesDestroyed = entitiesDestroyed,
            ItemsAffected = itemsAffected,
            AffectedCharacters = affectedCharacters,
            TotalEffectsDispelled = effectsRemoved.Count
                                    + entitiesDestroyed.Count
                                    + itemsAffected.Count,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an empty DispelEffectResult representing a no-op dispel.
    /// </summary>
    /// <returns>A result with all lists empty and all counts at zero.</returns>
    public static DispelEffectResult Empty()
    {
        return new DispelEffectResult
        {
            EffectsRemoved = Array.Empty<string>(),
            EntitiesDestroyed = Array.Empty<Guid>(),
            ItemsAffected = Array.Empty<Guid>(),
            AffectedCharacters = Array.Empty<Guid>(),
            TotalEffectsDispelled = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ═══════ Display Methods ═══════

    /// <summary>
    /// Gets a formatted summary of the dispel results for the combat log.
    /// </summary>
    /// <returns>
    /// A multi-line string summarizing effects removed, entities destroyed,
    /// and characters affected.
    /// </returns>
    public string GetSummaryDisplay()
    {
        var lines = new List<string>
        {
            $"Word of Unmaking: Dispelled {TotalEffectsDispelled} effects"
        };

        if (EffectsRemoved.Count > 0)
        {
            var effectNames = string.Join(", ", EffectsRemoved.Take(5));
            var suffix = EffectsRemoved.Count > 5
                ? $" (+{EffectsRemoved.Count - 5} more)"
                : string.Empty;
            lines.Add($"  - Effects removed: {effectNames}{suffix}");
        }

        if (EntitiesDestroyed.Count > 0)
            lines.Add($"  - Entities destroyed: {EntitiesDestroyed.Count}");

        if (ItemsAffected.Count > 0)
            lines.Add($"  - Items affected: {ItemsAffected.Count}");

        if (AffectedCharacters.Count > 0)
            lines.Add($"  - Characters affected: {AffectedCharacters.Count}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Returns a human-readable representation of the dispel result.
    /// </summary>
    public override string ToString() =>
        $"Dispel Result: {TotalEffectsDispelled} effects dispelled " +
        $"({EffectsRemoved.Count} statuses, {EntitiesDestroyed.Count} entities, " +
        $"{ItemsAffected.Count} items), {AffectedCharacters.Count} characters affected";
}
