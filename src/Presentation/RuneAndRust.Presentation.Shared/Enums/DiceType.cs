// ═══════════════════════════════════════════════════════════════════════════════
// DiceType.cs
// Dice type enum for dice rolling display.
// Version: 0.13.5e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Represents standard dice types used in the game.
/// </summary>
/// <remarks>
/// <para>
/// The enum values correspond to the number of faces on each die type.
/// This allows conversion between the enum and face count using explicit casting.
/// </para>
/// <para>
/// Used by <see cref="IconUtilities.GetDiceIcon"/> for dice-related display.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var d20 = DiceType.D20;
/// int faces = (int)d20; // 20
/// </code>
/// </example>
public enum DiceType
{
    /// <summary>Four-sided die (d4).</summary>
    D4 = 4,

    /// <summary>Six-sided die (d6).</summary>
    D6 = 6,

    /// <summary>Eight-sided die (d8).</summary>
    D8 = 8,

    /// <summary>Ten-sided die (d10).</summary>
    D10 = 10,

    /// <summary>Twelve-sided die (d12).</summary>
    D12 = 12,

    /// <summary>Twenty-sided die (d20).</summary>
    D20 = 20,

    /// <summary>Hundred-sided die (d100/percentile).</summary>
    D100 = 100
}
