namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of applying a hazard's effect to a target.
/// </summary>
/// <remarks>
/// This value object captures all information about a hazard effect application,
/// including damage dealt, saving throw results, and any status effects applied.
/// It is used by the HazardZoneService to communicate results to the presentation layer.
/// </remarks>
public readonly record struct HazardEffectResult
{
    /// <summary>Gets the name of the hazard that was applied.</summary>
    public string HazardName { get; init; }

    /// <summary>Gets the total damage dealt after any modifications (e.g., halving from saves).</summary>
    public int DamageDealt { get; init; }

    /// <summary>Gets the damage type (e.g., "fire", "poison", "cold").</summary>
    public string? DamageType { get; init; }

    /// <summary>Gets the individual dice roll results for damage.</summary>
    public IReadOnlyList<int> DamageRolls { get; init; }

    /// <summary>Gets whether a saving throw was attempted.</summary>
    public bool SaveAttempted { get; init; }

    /// <summary>Gets whether the saving throw succeeded.</summary>
    public bool SaveSucceeded { get; init; }

    /// <summary>Gets the total saving throw roll (d20 + modifier).</summary>
    public int? SaveRoll { get; init; }

    /// <summary>Gets the Difficulty Class that was rolled against.</summary>
    public int? SaveDC { get; init; }

    /// <summary>Gets the attribute used for the saving throw.</summary>
    public string? SaveAttribute { get; init; }

    /// <summary>Gets the list of status effects that were applied.</summary>
    public IReadOnlyList<string> StatusEffectsApplied { get; init; }

    /// <summary>Gets whether the effect was completely negated by a successful save.</summary>
    public bool WasNegated { get; init; }

    /// <summary>Gets the formatted message describing the effect result.</summary>
    public string Message { get; init; }

    /// <summary>
    /// Creates a result for damage dealt without a saving throw.
    /// </summary>
    /// <param name="hazardName">Name of the hazard.</param>
    /// <param name="damage">Total damage dealt.</param>
    /// <param name="damageType">Type of damage.</param>
    /// <param name="rolls">Individual dice rolls.</param>
    /// <param name="message">Formatted message.</param>
    /// <returns>A damage-only result.</returns>
    public static HazardEffectResult DamageOnly(
        string hazardName,
        int damage,
        string damageType,
        IEnumerable<int> rolls,
        string message) => new()
    {
        HazardName = hazardName,
        DamageDealt = damage,
        DamageType = damageType,
        DamageRolls = rolls.ToList(),
        SaveAttempted = false,
        SaveSucceeded = false,
        StatusEffectsApplied = Array.Empty<string>(),
        WasNegated = false,
        Message = message
    };

    /// <summary>
    /// Creates a result for damage dealt with a saving throw.
    /// </summary>
    public static HazardEffectResult DamageWithSave(
        string hazardName,
        int damage,
        string damageType,
        IEnumerable<int> rolls,
        string message,
        int saveRoll,
        int saveDC,
        string saveAttribute,
        bool saveSucceeded) => new()
    {
        HazardName = hazardName,
        DamageDealt = damage,
        DamageType = damageType,
        DamageRolls = rolls.ToList(),
        SaveAttempted = true,
        SaveSucceeded = saveSucceeded,
        SaveRoll = saveRoll,
        SaveDC = saveDC,
        SaveAttribute = saveAttribute,
        StatusEffectsApplied = Array.Empty<string>(),
        WasNegated = false,
        Message = message
    };

    /// <summary>
    /// Creates a result for an effect that was completely negated by a successful save.
    /// </summary>
    /// <param name="hazardName">Name of the hazard.</param>
    /// <param name="saveRoll">The saving throw roll result.</param>
    /// <param name="saveDC">The Difficulty Class.</param>
    /// <param name="saveAttribute">The attribute used for the save.</param>
    /// <param name="message">Formatted message.</param>
    /// <returns>A negated effect result.</returns>
    public static HazardEffectResult Negated(
        string hazardName,
        int saveRoll,
        int saveDC,
        string saveAttribute,
        string message) => new()
    {
        HazardName = hazardName,
        DamageDealt = 0,
        DamageType = null,
        DamageRolls = Array.Empty<int>(),
        SaveAttempted = true,
        SaveSucceeded = true,
        SaveRoll = saveRoll,
        SaveDC = saveDC,
        SaveAttribute = saveAttribute,
        StatusEffectsApplied = Array.Empty<string>(),
        WasNegated = true,
        Message = message
    };

    /// <summary>
    /// Creates a result for combined damage and status effects.
    /// </summary>
    public static HazardEffectResult Combined(
        string hazardName,
        int damage,
        string? damageType,
        IEnumerable<int> rolls,
        IEnumerable<string> statusEffects,
        string message,
        bool saveAttempted = false,
        bool saveSucceeded = false,
        int? saveRoll = null,
        int? saveDC = null,
        string? saveAttribute = null) => new()
    {
        HazardName = hazardName,
        DamageDealt = damage,
        DamageType = damageType,
        DamageRolls = rolls.ToList(),
        SaveAttempted = saveAttempted,
        SaveSucceeded = saveSucceeded,
        SaveRoll = saveRoll,
        SaveDC = saveDC,
        SaveAttribute = saveAttribute,
        StatusEffectsApplied = statusEffects.ToList(),
        WasNegated = false,
        Message = message
    };

    /// <summary>
    /// Creates a result indicating no effect was applied.
    /// </summary>
    /// <param name="hazardName">Name of the hazard.</param>
    /// <returns>An empty result.</returns>
    public static HazardEffectResult NoEffect(string hazardName) => new()
    {
        HazardName = hazardName,
        DamageDealt = 0,
        DamageRolls = Array.Empty<int>(),
        StatusEffectsApplied = Array.Empty<string>(),
        Message = string.Empty
    };
}
