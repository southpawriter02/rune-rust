using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Calculators;

/// <summary>
/// Static calculator for XP (Legend) rewards in Rune &amp; Rust.
/// Maps ThreatTier to XP values and provides room discovery rewards.
/// </summary>
/// <remarks>See: v0.4.0d (The Reward) for XP Integration implementation.</remarks>
public static class XpCalculator
{
    /// <summary>
    /// XP reward for discovering a room for the first time.
    /// </summary>
    public const int RoomDiscoveryXp = 10;

    /// <summary>
    /// Gets the XP reward for defeating an enemy of the specified threat tier.
    /// </summary>
    /// <param name="tier">The threat tier of the defeated enemy.</param>
    /// <returns>XP value: Minion=25, Standard=100, Elite=250, Boss=1000.</returns>
    public static int GetXpForTier(ThreatTier tier) => tier switch
    {
        ThreatTier.Minion => 25,
        ThreatTier.Standard => 100,
        ThreatTier.Elite => 250,
        ThreatTier.Boss => 1000,
        _ => 100 // Default to Standard if unknown
    };
}
