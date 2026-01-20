namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the type of die used in a dice pool.
/// </summary>
/// <remarks>
/// The numeric value of each enum member corresponds to the number of faces on the die.
/// All skill checks and resolutions use d10 increments. The d4, d6, and d8 are primarily
/// for damage and effect scaling.
/// </remarks>
public enum DiceType
{
    /// <summary>Four-sided die for minor damage and small effects.</summary>
    D4 = 4,

    /// <summary>Six-sided die for standard damage and ability rolls.</summary>
    D6 = 6,

    /// <summary>Eight-sided die for heavy damage and moderate effects.</summary>
    D8 = 8,

    /// <summary>Ten-sided die for resolution rolls, skill checks, and major effects.</summary>
    D10 = 10
}
