namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a modifier to passive perception from a specific source.
/// </summary>
/// <remarks>
/// <para>
/// Modifiers can come from various sources:
/// <list type="bullet">
///   <item><description>Status effects ([Alert], [Fatigued], [Exhausted])</description></item>
///   <item><description>Environmental factors (lighting, zone effects)</description></item>
///   <item><description>Equipment bonuses (perception-enhancing items)</description></item>
///   <item><description>Specialization abilities (Ruin-Stalker, Veiðimaðr)</description></item>
/// </list>
/// </para>
/// <para>
/// Positive values increase perception, negative values decrease it.
/// </para>
/// </remarks>
/// <param name="ModifierId">Unique identifier for this modifier instance.</param>
/// <param name="Source">The source granting this modifier (e.g., "status:alert", "equipment:spy-glass").</param>
/// <param name="Value">The modifier value (positive for bonuses, negative for penalties).</param>
/// <param name="Description">Human-readable description for display.</param>
public readonly record struct PerceptionModifier(
    string ModifierId,
    string Source,
    int Value,
    string Description)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DERIVED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this modifier is a bonus (positive value).
    /// </summary>
    public bool IsBonus => Value > 0;

    /// <summary>
    /// Gets whether this modifier is a penalty (negative value).
    /// </summary>
    public bool IsPenalty => Value < 0;

    /// <summary>
    /// Gets whether this modifier has no effect (zero value).
    /// </summary>
    public bool IsNeutral => Value == 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // STATUS EFFECT FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates an [Alert] status effect modifier.
    /// </summary>
    /// <returns>A +2 perception modifier from Alert status.</returns>
    public static PerceptionModifier Alert() =>
        new("status-alert", "status:alert", 2, "[Alert] heightened awareness");

    /// <summary>
    /// Creates a [Fatigued] status effect modifier.
    /// </summary>
    /// <returns>A -1 perception modifier from Fatigued status.</returns>
    public static PerceptionModifier Fatigued() =>
        new("status-fatigued", "status:fatigued", -1, "[Fatigued] dulled senses");

    /// <summary>
    /// Creates an [Exhausted] status effect modifier.
    /// </summary>
    /// <returns>A -2 perception modifier from Exhausted status.</returns>
    public static PerceptionModifier Exhausted() =>
        new("status-exhausted", "status:exhausted", -2, "[Exhausted] severely impaired");

    // ═══════════════════════════════════════════════════════════════════════════
    // ENVIRONMENTAL FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a familiar territory modifier.
    /// </summary>
    /// <returns>A +1 perception modifier for familiar areas.</returns>
    public static PerceptionModifier FamiliarTerritory() =>
        new("env-familiar", "environment:familiar", 1, "Familiar territory");

    /// <summary>
    /// Creates a dim lighting modifier.
    /// </summary>
    /// <returns>A -1 perception modifier for dim light conditions.</returns>
    public static PerceptionModifier DimLighting() =>
        new("env-dim-light", "environment:dim", -1, "Dim lighting");

    /// <summary>
    /// Creates a total darkness modifier.
    /// </summary>
    /// <returns>A -3 perception modifier for complete darkness.</returns>
    public static PerceptionModifier TotalDarkness() =>
        new("env-darkness", "environment:dark", -3, "Total darkness");

    /// <summary>
    /// Creates a [Psychic Resonance] zone modifier.
    /// </summary>
    /// <returns>A -2 perception modifier for psychic interference zones.</returns>
    public static PerceptionModifier PsychicResonanceZone() =>
        new("zone-psychic", "zone:psychic-resonance", -2, "[Psychic Resonance] interference");

    // ═══════════════════════════════════════════════════════════════════════════
    // CUSTOM FACTORY METHOD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a custom modifier from configuration.
    /// </summary>
    /// <param name="id">The modifier ID.</param>
    /// <param name="source">The source type.</param>
    /// <param name="value">The modifier value.</param>
    /// <param name="description">The display description.</param>
    /// <returns>A new PerceptionModifier instance.</returns>
    public static PerceptionModifier Custom(string id, string source, int value, string description) =>
        new(id, source, value, description);
}
