// ═══════════════════════════════════════════════════════════════════════════════
// RarityBadge.cs
// Renders rarity tier badges for achievements.
// Version: 0.13.4a
// ═══════════════════════════════════════════════════════════════════════════════

using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Presentation.Tui.UI;

/// <summary>
/// Renders rarity tier badges for achievements.
/// </summary>
/// <remarks>
/// <para>
/// Each achievement rarity tier (Bronze, Silver, Gold, Platinum) has a distinct
/// color and point value. This component provides consistent badge rendering
/// across the achievement UI.
/// </para>
/// <para>
/// Tier point values:
/// </para>
/// <list type="bullet">
///   <item><description>Bronze - 10 points (common achievements)</description></item>
///   <item><description>Silver - 25 points (moderate difficulty)</description></item>
///   <item><description>Gold - 50 points (challenging achievements)</description></item>
///   <item><description>Platinum - 100 points (elite achievements)</description></item>
/// </list>
/// </remarks>
public class RarityBadge
{
    private readonly ITerminalService _terminalService;

    /// <summary>
    /// Creates a new instance of the RarityBadge component.
    /// </summary>
    /// <param name="terminalService">The terminal output service for rendering.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="terminalService"/> is null.
    /// </exception>
    public RarityBadge(ITerminalService terminalService)
    {
        _terminalService = terminalService ?? throw new ArgumentNullException(nameof(terminalService));
    }

    /// <summary>
    /// Renders the rarity badge as a formatted string.
    /// </summary>
    /// <param name="tier">The achievement tier.</param>
    /// <returns>
    /// A formatted badge string like "Bronze (10)" or "Platinum (100)".
    /// </returns>
    /// <example>
    /// <code>
    /// var badge = new RarityBadge(terminalService);
    /// var text = badge.RenderBadge(AchievementTier.Gold);
    /// // Returns: "Gold (50)"
    /// </code>
    /// </example>
    public string RenderBadge(AchievementTier tier)
    {
        var tierName = GetTierName(tier);
        var pointValue = GetPointValue(tier);
        return $"{tierName} ({pointValue})";
    }

    /// <summary>
    /// Renders the rarity badge with color at a specific terminal position.
    /// </summary>
    /// <param name="tier">The achievement tier.</param>
    /// <param name="x">The X coordinate on the terminal.</param>
    /// <param name="y">The Y coordinate on the terminal.</param>
    public void RenderBadgeAt(AchievementTier tier, int x, int y)
    {
        var badgeText = RenderBadge(tier);
        var color = GetTierColor(tier);
        _terminalService.WriteColoredAt(x, y, badgeText, color);
    }

    /// <summary>
    /// Gets the display color for a rarity tier.
    /// </summary>
    /// <param name="tier">The achievement tier.</param>
    /// <returns>
    /// The <see cref="ConsoleColor"/> for the tier:
    /// <list type="bullet">
    ///   <item><description>Bronze - DarkYellow (brown-ish)</description></item>
    ///   <item><description>Silver - Gray</description></item>
    ///   <item><description>Gold - Yellow</description></item>
    ///   <item><description>Platinum - Cyan</description></item>
    /// </list>
    /// </returns>
    public ConsoleColor GetTierColor(AchievementTier tier)
    {
        return tier switch
        {
            AchievementTier.Bronze => ConsoleColor.DarkYellow,
            AchievementTier.Silver => ConsoleColor.Gray,
            AchievementTier.Gold => ConsoleColor.Yellow,
            AchievementTier.Platinum => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };
    }

    /// <summary>
    /// Gets the point value for a rarity tier.
    /// </summary>
    /// <param name="tier">The achievement tier.</param>
    /// <returns>
    /// The point value for the tier (10, 25, 50, or 100).
    /// Returns the enum integer value directly, or 0 for unknown tiers.
    /// </returns>
    /// <remarks>
    /// The <see cref="AchievementTier"/> enum values are defined as their
    /// point values (Bronze = 10, Silver = 25, etc.), so this method simply
    /// casts the enum to int.
    /// </remarks>
    public int GetPointValue(AchievementTier tier)
    {
        return tier switch
        {
            AchievementTier.Bronze => 10,
            AchievementTier.Silver => 25,
            AchievementTier.Gold => 50,
            AchievementTier.Platinum => 100,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the display name for a rarity tier.
    /// </summary>
    /// <param name="tier">The achievement tier.</param>
    /// <returns>
    /// The tier name as a string ("Bronze", "Silver", "Gold", or "Platinum").
    /// Returns "Unknown" for unrecognized tier values.
    /// </returns>
    public string GetTierName(AchievementTier tier)
    {
        return tier switch
        {
            AchievementTier.Bronze => "Bronze",
            AchievementTier.Silver => "Silver",
            AchievementTier.Gold => "Gold",
            AchievementTier.Platinum => "Platinum",
            _ => "Unknown"
        };
    }
}
