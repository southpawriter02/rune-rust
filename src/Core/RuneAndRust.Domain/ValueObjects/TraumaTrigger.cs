namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a trigger condition for trauma effect activation.
/// </summary>
/// <remarks>
/// <para>
/// Trauma triggers define when a trauma's special effects activate or when
/// a trauma check is required. A trauma can have multiple triggers for
/// different circumstances.
/// </para>
/// <para>
/// Trigger Types:
/// <list type="bullet">
/// <item>OnRest: Activates during rest period</item>
/// <item>OnAllyDeath: Activates when party member dies</item>
/// <item>NearForlorn: Activates in proximity to corrupted entities</item>
/// <item>InCombat: Activates during combat</item>
/// <item>Custom: Game logic decides activation</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var trigger = TraumaTrigger.Create(
///     triggerType: "OnRest",
///     condition: null,
///     checkRequired: true,
///     checkDifficulty: 2
/// );
/// </code>
/// </example>
public readonly record struct TraumaTrigger
{
    /// <summary>
    /// Gets the type of trigger (e.g., "OnAllyDeath", "OnRest").
    /// </summary>
    /// <remarks>
    /// Determines what event activates this trigger.
    /// </remarks>
    public string TriggerType { get; init; }

    /// <summary>
    /// Gets the specific condition for this trigger.
    /// </summary>
    /// <remarks>
    /// Null means trigger activates on any occurrence of TriggerType.
    /// Examples: "AllyHasDied", "WhileInStress", "OncePerSession"
    /// </remarks>
    public string? Condition { get; init; }

    /// <summary>
    /// Gets whether a check is required to resist this trigger's effect.
    /// </summary>
    /// <remarks>
    /// If true, character must pass a check (CheckDifficulty)
    /// to avoid the effect. If false, effect applies automatically.
    /// </remarks>
    public bool CheckRequired { get; init; }

    /// <summary>
    /// Gets the difficulty of the check (if CheckRequired is true).
    /// </summary>
    /// <remarks>
    /// Typical values: 1-4
    /// Null if CheckRequired is false.
    /// </remarks>
    public int? CheckDifficulty { get; init; }

    /// <summary>
    /// Creates a new TraumaTrigger with the specified properties.
    /// </summary>
    /// <param name="triggerType">Type of trigger</param>
    /// <param name="condition">Specific condition (if any)</param>
    /// <param name="checkRequired">Whether a check is required</param>
    /// <param name="checkDifficulty">Difficulty if check required</param>
    /// <returns>A new TraumaTrigger instance</returns>
    /// <exception cref="ArgumentException">If triggerType is empty</exception>
    /// <exception cref="InvalidOperationException">If check required but difficulty not specified</exception>
    public static TraumaTrigger Create(
        string triggerType,
        string? condition = null,
        bool checkRequired = false,
        int? checkDifficulty = null)
    {
        if (string.IsNullOrWhiteSpace(triggerType))
            throw new ArgumentException("TriggerType cannot be empty", nameof(triggerType));
        if (checkRequired && !checkDifficulty.HasValue)
            throw new InvalidOperationException(
                "CheckDifficulty must be specified when CheckRequired is true");

        return new TraumaTrigger
        {
            TriggerType = triggerType,
            Condition = condition,
            CheckRequired = checkRequired,
            CheckDifficulty = checkDifficulty
        };
    }

    /// <summary>
    /// Gets the display string for this trigger.
    /// </summary>
    public override string ToString() =>
        $"{TriggerType}" +
        (Condition != null ? $"[{Condition}]" : "") +
        (CheckRequired ? $" [DC {CheckDifficulty}]" : "");
}
