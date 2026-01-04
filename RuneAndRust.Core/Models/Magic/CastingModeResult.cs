using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models.Magic;

/// <summary>
/// Modifiers applied based on the selected casting mode.
/// </summary>
public record CastingModeResult(
    CastingMode Mode,
    int ResonanceGain,
    int CastTimeModifier,
    int FluxModifier)
{
    /// <summary>
    /// True if this mode can only be used outside of combat.
    /// </summary>
    public bool IsOutOfCombatOnly => Mode == CastingMode.Ritual;

    /// <summary>
    /// True if this mode grants bonus action casting.
    /// </summary>
    public bool IsBonusAction => Mode == CastingMode.Quick;

    /// <summary>
    /// True if this mode requires multiple turns.
    /// </summary>
    public bool IsExtendedCast => Mode == CastingMode.Channeled;

    /// <summary>
    /// Descriptive text for the casting mode.
    /// </summary>
    public string Description => Mode switch
    {
        CastingMode.Quick => "Quick cast (bonus action, +15 resonance)",
        CastingMode.Standard => "Standard cast (1 turn, +10 resonance)",
        CastingMode.Channeled => "Channeled cast (2 turns, +5 resonance)",
        CastingMode.Ritual => "Ritual cast (out of combat, +0 resonance)",
        _ => "Unknown casting mode"
    };
}
