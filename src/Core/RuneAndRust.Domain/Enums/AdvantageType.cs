namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents advantage/disadvantage state for dice rolls.
/// </summary>
/// <remarks>
/// When rolling with advantage, roll twice and take the higher result.
/// When rolling with disadvantage, roll twice and take the lower result.
/// These mechanics represent situational bonuses and penalties.
/// </remarks>
public enum AdvantageType
{
    /// <summary>Normal roll - single roll used.</summary>
    Normal,

    /// <summary>Advantage - roll twice, take higher result.</summary>
    Advantage,

    /// <summary>Disadvantage - roll twice, take lower result.</summary>
    Disadvantage
}
