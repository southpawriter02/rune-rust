using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Effects;

/// <summary>
/// Effect that sets a game flag.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class SetFlagEffect : DialogueEffect
{
    /// <inheritdoc/>
    public override DialogueEffectType Type => DialogueEffectType.SetFlag;

    /// <summary>
    /// The flag key to set.
    /// </summary>
    public string FlagKey { get; set; } = string.Empty;

    /// <summary>
    /// The value to set (default: true).
    /// </summary>
    public bool Value { get; set; } = true;

    /// <inheritdoc/>
    public override string GetDescription() =>
        Value ? $"Set: {FlagKey}" : $"Clear: {FlagKey}";
}
