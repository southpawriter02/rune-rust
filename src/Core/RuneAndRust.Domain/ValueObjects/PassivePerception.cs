namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a character's calculated passive perception value.
/// </summary>
/// <remarks>
/// <para>
/// Passive perception is calculated as WITS ÷ 2 (round up), then modified
/// by status effects, environmental factors, and equipment bonuses.
/// </para>
/// <para>
/// This value is used for automatic detection of hidden elements on room entry.
/// Higher passive perception reveals more hidden elements without active checks.
/// </para>
/// </remarks>
/// <param name="CharacterId">The ID of the character this perception belongs to.</param>
/// <param name="WitsAttribute">The character's WITS attribute value (1-30).</param>
/// <param name="PassiveValue">The base passive value (WITS ÷ 2, rounded up).</param>
/// <param name="ModifierBreakdown">List of all active modifiers affecting perception.</param>
public readonly record struct PassivePerception(
    string CharacterId,
    int WitsAttribute,
    int PassiveValue,
    IReadOnlyList<PerceptionModifier> ModifierBreakdown)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the effective passive perception after all modifiers.
    /// </summary>
    /// <remarks>
    /// This is the value compared against hidden element DCs.
    /// Minimum effective value is 0 (negative results clamp to 0).
    /// </remarks>
    public int EffectiveValue
    {
        get
        {
            var totalModifier = ModifierBreakdown.Sum(m => m.Value);
            return Math.Max(0, PassiveValue + totalModifier);
        }
    }

    /// <summary>
    /// Gets the total modifier value from all sources.
    /// </summary>
    public int TotalModifier => ModifierBreakdown.Sum(m => m.Value);

    /// <summary>
    /// Gets whether any modifiers are currently active.
    /// </summary>
    public bool HasModifiers => ModifierBreakdown.Count > 0;

    /// <summary>
    /// Gets whether the effective value differs from the base passive value.
    /// </summary>
    public bool IsModified => TotalModifier != 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates passive perception for a character with the given WITS value.
    /// </summary>
    /// <param name="characterId">The character's unique identifier.</param>
    /// <param name="witsAttribute">The character's WITS attribute (1-30).</param>
    /// <param name="modifiers">Active modifiers to apply.</param>
    /// <returns>A new PassivePerception with calculated values.</returns>
    /// <exception cref="ArgumentException">Thrown when characterId is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when witsAttribute is out of range.</exception>
    /// <example>
    /// <code>
    /// // Calculate passive perception for WITS 7 with Alert status
    /// var modifiers = new List&lt;PerceptionModifier&gt; { PerceptionModifier.Alert() };
    /// var passive = PassivePerception.Calculate("player-1", 7, modifiers);
    /// // passive.PassiveValue = 4 (7 ÷ 2 = 3.5 → 4)
    /// // passive.EffectiveValue = 6 (4 + 2 from Alert)
    /// </code>
    /// </example>
    public static PassivePerception Calculate(
        string characterId,
        int witsAttribute,
        IReadOnlyList<PerceptionModifier>? modifiers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentOutOfRangeException.ThrowIfLessThan(witsAttribute, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(witsAttribute, 30);

        // Formula: WITS ÷ 2, round up
        var passiveValue = (int)Math.Ceiling(witsAttribute / 2.0);

        return new PassivePerception(
            characterId,
            witsAttribute,
            passiveValue,
            modifiers ?? Array.Empty<PerceptionModifier>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string showing the calculation breakdown.
    /// </summary>
    /// <returns>A formatted string showing base value and modifiers.</returns>
    public string ToDisplayString()
    {
        if (!HasModifiers)
        {
            return $"Passive Perception: {PassiveValue} (WITS {WitsAttribute} ÷ 2)";
        }

        var modifierText = string.Join(", ",
            ModifierBreakdown.Select(m =>
                $"{(m.Value >= 0 ? "+" : "")}{m.Value} ({m.Description})"));

        return $"Passive Perception: {EffectiveValue} (base {PassiveValue} {modifierText})";
    }
}
