namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the type of armor, affecting defense bonuses and restrictions.
/// </summary>
/// <remarks>
/// Armor types represent a trade-off between protection and mobility:
/// <list type="bullet">
/// <item><description>Light: Low defense, no penalties, no requirements</description></item>
/// <item><description>Medium: Moderate defense, slight penalty, some requirements</description></item>
/// <item><description>Heavy: High defense, significant penalties, strict requirements</description></item>
/// </list>
/// </remarks>
public enum ArmorType
{
    /// <summary>Light armor (leather, cloth). Minimal protection, no restrictions.</summary>
    Light,

    /// <summary>Medium armor (chain, scale). Balanced protection with minor penalties.</summary>
    Medium,

    /// <summary>Heavy armor (plate, full mail). Maximum protection, significant penalties.</summary>
    Heavy
}
