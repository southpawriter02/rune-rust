using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks a game flag value.
/// Example: [HasCompletedTutorial] for progression-gated dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class FlagCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Flag;

    /// <summary>
    /// The flag key to check.
    /// </summary>
    public string FlagKey { get; set; } = string.Empty;

    /// <summary>
    /// The required value (default: true).
    /// </summary>
    public bool RequiredValue { get; set; } = true;

    /// <inheritdoc/>
    public override string GetDisplayHint() =>
        RequiredValue ? $"[{FlagKey}]" : $"[NOT {FlagKey}]";
}
