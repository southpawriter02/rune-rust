using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Configuration;

/// <summary>
/// Default configuration for respec (talent point reallocation) settings.
/// </summary>
/// <remarks>
/// <para>RespecConfiguration provides sensible default values for the respec system:</para>
/// <list type="bullet">
///   <item><description>BaseRespecCost: 100 gold minimum</description></item>
///   <item><description>LevelMultiplier: 10 gold per level</description></item>
///   <item><description>IsRespecEnabled: true (feature enabled by default)</description></item>
///   <item><description>MinimumLevelToRespec: 2 (available after first level-up)</description></item>
///   <item><description>CurrencyId: "gold" (standard currency)</description></item>
/// </list>
/// <para>Example costs by level:</para>
/// <list type="bullet">
///   <item><description>Level 2: 100 + (2 × 10) = 120 gold</description></item>
///   <item><description>Level 5: 100 + (5 × 10) = 150 gold</description></item>
///   <item><description>Level 10: 100 + (10 × 10) = 200 gold</description></item>
///   <item><description>Level 20: 100 + (20 × 10) = 300 gold</description></item>
/// </list>
/// <para>This class can be extended to load from JSON configuration in the future.</para>
/// </remarks>
public class RespecConfiguration : IRespecConfiguration
{
    /// <inheritdoc />
    /// <remarks>
    /// Default: 100 gold. This is the minimum cost regardless of player level.
    /// </remarks>
    public int BaseRespecCost { get; set; } = 100;

    /// <inheritdoc />
    /// <remarks>
    /// Default: 10 gold per level. Combined with BaseRespecCost for total.
    /// </remarks>
    public int LevelMultiplier { get; set; } = 10;

    /// <inheritdoc />
    /// <remarks>
    /// Default: true. Set to false to temporarily disable respec.
    /// </remarks>
    public bool IsRespecEnabled { get; set; } = true;

    /// <inheritdoc />
    /// <remarks>
    /// Default: 2. Players must reach level 2 before respec is available.
    /// </remarks>
    public int MinimumLevelToRespec { get; set; } = 2;

    /// <inheritdoc />
    /// <remarks>
    /// Default: "gold". Must match a currency ID in the player's inventory.
    /// </remarks>
    public string CurrencyId { get; set; } = "gold";
}
