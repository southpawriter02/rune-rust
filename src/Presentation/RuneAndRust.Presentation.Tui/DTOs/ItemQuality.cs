// ═══════════════════════════════════════════════════════════════════════════════
// ItemQuality.cs
// Represents the quality tier of a crafted item.
// Version: 0.13.3c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Represents the quality tier of a crafted item.
/// </summary>
/// <remarks>
/// <para>Quality tiers are displayed using star ratings (★).</para>
/// <para>Higher quality items have better stats and are more valuable.</para>
/// <list type="table">
///   <listheader>
///     <term>Quality</term>
///     <description>Stars</description>
///   </listheader>
///   <item><term>Common</term><description>★</description></item>
///   <item><term>Uncommon</term><description>★★</description></item>
///   <item><term>Rare</term><description>★★★</description></item>
///   <item><term>Epic</term><description>★★★★</description></item>
///   <item><term>Legendary</term><description>★★★★★</description></item>
/// </list>
/// </remarks>
public enum ItemQuality
{
    /// <summary>Basic quality (★).</summary>
    Common = 1,

    /// <summary>Slightly improved quality (★★).</summary>
    Uncommon = 2,

    /// <summary>Notable quality (★★★).</summary>
    Rare = 3,

    /// <summary>Exceptional quality (★★★★).</summary>
    Epic = 4,

    /// <summary>Masterwork quality (★★★★★).</summary>
    Legendary = 5
}
