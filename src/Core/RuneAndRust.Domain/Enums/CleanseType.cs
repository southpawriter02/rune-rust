namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines what types of effects a cleanse removes.
/// </summary>
/// <remarks>
/// <para>Different cleanse sources target different effect categories.</para>
/// </remarks>
public enum CleanseType
{
    /// <summary>Removes all negative effects (debuffs).</summary>
    AllNegative,

    /// <summary>Removes all positive effects (dispel).</summary>
    AllPositive,

    /// <summary>Removes physical debuffs (Bleeding, Poisoned, Exhausted, Knocked Down).</summary>
    Physical,

    /// <summary>Removes magical debuffs (Cursed, Silenced, Feared, Weakened).</summary>
    Magical,

    /// <summary>Removes elemental effects (Burning, Frozen, Wet, Chilled, On Fire, Electrified).</summary>
    Elemental,

    /// <summary>Removes a specific named effect.</summary>
    Specific
}
