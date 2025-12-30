using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks character level.
/// Example: [Level >= 5] for advanced dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class LevelCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Level;

    /// <summary>
    /// Minimum level required.
    /// </summary>
    public int MinLevel { get; set; } = 1;

    /// <inheritdoc/>
    public override string GetDisplayHint() => $"[Level {MinLevel}]";
}
