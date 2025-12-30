using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks faction reputation/disposition.
/// Example: [Iron-Banes: Friendly] for faction-gated dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class ReputationCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Reputation;

    /// <summary>
    /// The faction to check reputation with.
    /// </summary>
    public FactionType Faction { get; set; }

    /// <summary>
    /// The minimum disposition tier required.
    /// </summary>
    public Disposition MinDisposition { get; set; } = Disposition.Neutral;

    /// <inheritdoc/>
    public override string GetDisplayHint() => $"[{Faction}: {MinDisposition}]";
}
