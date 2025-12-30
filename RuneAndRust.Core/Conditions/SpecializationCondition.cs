using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks specialization access.
/// Example: [Is: Berserkr] for class-specific dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class SpecializationCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Specialization;

    /// <summary>
    /// The specialization ID to check for.
    /// </summary>
    public Guid SpecializationId { get; set; }

    /// <summary>
    /// Display name for the hint (cached from spec).
    /// </summary>
    public string SpecializationName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override string GetDisplayHint() => $"[Is: {SpecializationName}]";
}
