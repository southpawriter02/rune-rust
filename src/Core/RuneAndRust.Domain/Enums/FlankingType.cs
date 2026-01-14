namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of flanking positions relative to a target.
/// </summary>
/// <remarks>
/// Flanking types determine attack and damage bonuses:
/// <list type="bullet">
/// <item><description>None: No positional advantage</description></item>
/// <item><description>Side: Perpendicular to facing (no bonus, but noted)</description></item>
/// <item><description>Behind: Opposite to facing (attack + damage bonus)</description></item>
/// <item><description>Flanked: Ally on opposite side (attack bonus)</description></item>
/// </list>
/// </remarks>
public enum FlankingType
{
    /// <summary>No flanking advantage.</summary>
    None = 0,

    /// <summary>Attacking from the side (no bonus).</summary>
    Side = 1,

    /// <summary>Attacking from behind (bonus attack and damage).</summary>
    Behind = 2,

    /// <summary>Full flank with ally on opposite side (bonus attack).</summary>
    Flanked = 3
}
