namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a single mechanical effect of a trauma.
/// </summary>
/// <remarks>
/// <para>
/// Traumas can have multiple effects that apply mechanically to character capabilities.
/// Each effect specifies what type of effect it is (penalty, bonus, disadvantage)
/// and what it targets (skill, check, attribute).
/// </para>
/// <para>
/// Effect Types:
/// <list type="bullet">
/// <item>Penalty: Numeric penalty to specified target</item>
/// <item>Bonus: Numeric bonus to specified target</item>
/// <item>Disadvantage: Disadvantage on checks of specified type</item>
/// <item>StressIncrease: Increases stress when condition met</item>
/// <item>Custom: Special effect handled by game logic</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var effect = TraumaEffect.Create(
///     effectType: "Penalty",
///     target: "social-skills",
///     value: -3,
///     description: "-3 penalty to Social checks"
/// );
/// </code>
/// </example>
public readonly record struct TraumaEffect
{
    /// <summary>
    /// Gets the type of effect (e.g., "Penalty", "Disadvantage").
    /// </summary>
    /// <remarks>
    /// Standard types: Penalty, Bonus, Disadvantage, StressIncrease, Custom
    /// </remarks>
    public string EffectType { get; init; }

    /// <summary>
    /// Gets the target of the effect (e.g., "social-skills", "morale-checks").
    /// </summary>
    /// <remarks>
    /// Can be skill name, check type, or custom target identifier.
    /// </remarks>
    public string Target { get; init; }

    /// <summary>
    /// Gets the numeric value of the effect (if applicable).
    /// </summary>
    /// <remarks>
    /// Null for non-numeric effects like Disadvantage.
    /// Positive for bonuses, negative for penalties.
    /// </remarks>
    public int? Value { get; init; }

    /// <summary>
    /// Gets the condition under which this effect applies.
    /// </summary>
    /// <remarks>
    /// Examples: "OnRest", "OnAllyTakeCriticalHit", "InCombat"
    /// Null means effect is always active.
    /// </remarks>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets the human-readable description of this effect.
    /// </summary>
    /// <remarks>
    /// Shown in UI tooltips and help text.
    /// </remarks>
    public string Description { get; init; }

    /// <summary>
    /// Creates a new TraumaEffect with the specified properties.
    /// </summary>
    /// <param name="effectType">Type of effect</param>
    /// <param name="target">What the effect targets</param>
    /// <param name="value">Numeric value (if applicable)</param>
    /// <param name="condition">Activation condition (if any)</param>
    /// <param name="description">Human-readable description</param>
    /// <returns>A new TraumaEffect instance</returns>
    /// <exception cref="ArgumentException">If effectType or target is empty</exception>
    public static TraumaEffect Create(
        string effectType,
        string target,
        int? value = null,
        string? condition = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(effectType))
            throw new ArgumentException("EffectType cannot be empty", nameof(effectType));
        if (string.IsNullOrWhiteSpace(target))
            throw new ArgumentException("Target cannot be empty", nameof(target));

        return new TraumaEffect
        {
            EffectType = effectType,
            Target = target,
            Value = value,
            Condition = condition,
            Description = description ?? $"{effectType} to {target}"
        };
    }

    /// <summary>
    /// Gets the display string for this effect.
    /// </summary>
    public override string ToString() =>
        $"{EffectType}: {Description}" +
        (Condition != null ? $" [{Condition}]" : "");
}
