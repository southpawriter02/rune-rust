using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Effects;

/// <summary>
/// Effect that modifies faction reputation.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class ModifyReputationEffect : DialogueEffect
{
    /// <inheritdoc/>
    public override DialogueEffectType Type => DialogueEffectType.ModifyReputation;

    /// <summary>
    /// The faction to modify reputation with.
    /// </summary>
    public FactionType Faction { get; set; }

    /// <summary>
    /// The amount to add (positive) or subtract (negative).
    /// </summary>
    public int Amount { get; set; }

    /// <inheritdoc/>
    public override string GetDescription() =>
        Amount >= 0 ? $"+{Amount} with {Faction}" : $"{Amount} with {Faction}";
}
