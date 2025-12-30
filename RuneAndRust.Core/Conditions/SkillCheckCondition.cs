using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that requires a dice roll skill check.
/// Unlike AttributeCondition, this involves randomness.
/// Example: [Roll WITS DC 3] for uncertain outcomes.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class SkillCheckCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.SkillCheck;

    /// <summary>
    /// The attribute to use for the dice pool.
    /// </summary>
    public CharacterAttribute Attribute { get; set; }

    /// <summary>
    /// The difficulty class (required net successes).
    /// </summary>
    public int DifficultyClass { get; set; } = 1;

    /// <summary>
    /// Optional description of what the check represents.
    /// </summary>
    public string? CheckDescription { get; set; }

    /// <inheritdoc/>
    public override string GetDisplayHint() =>
        $"[{Attribute.ToString().ToUpperInvariant()} DC {DifficultyClass}]";
}
