using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a player's reputation with a single faction.
/// </summary>
/// <remarks>
/// <para>Reputation values are clamped to the range [<see cref="MinValue"/> (-100),
/// <see cref="MaxValue"/> (+100)]. The <see cref="Tier"/> is automatically derived
/// from the <see cref="Value"/> using the tier boundaries defined in the
/// faction-reputation design doc (v1.2, Section 2).</para>
///
/// <para>Follows the immutable pattern established by <c>Stats</c> and
/// <c>PlayerAttributes</c>. All mutation operations return new instances
/// rather than modifying existing ones.</para>
///
/// <para><b>Tier Boundaries:</b></para>
/// <list type="table">
///   <listheader><term>Tier</term><description>Range</description></listheader>
///   <item><term>Hated</term><description>-100 to -76</description></item>
///   <item><term>Hostile</term><description>-75 to -26</description></item>
///   <item><term>Neutral</term><description>-25 to +24</description></item>
///   <item><term>Friendly</term><description>+25 to +49</description></item>
///   <item><term>Allied</term><description>+50 to +74</description></item>
///   <item><term>Exalted</term><description>+75 to +100</description></item>
/// </list>
///
/// <para><b>Price Modifiers by Tier:</b></para>
/// <list type="table">
///   <listheader><term>Tier</term><description>Modifier</description></listheader>
///   <item><term>Hated</term><description>+50% (1.50)</description></item>
///   <item><term>Hostile</term><description>+25% (1.25)</description></item>
///   <item><term>Neutral</term><description>Standard (1.00)</description></item>
///   <item><term>Friendly</term><description>-10% (0.90)</description></item>
///   <item><term>Allied</term><description>-20% (0.80)</description></item>
///   <item><term>Exalted</term><description>-30% (0.70)</description></item>
/// </list>
/// </remarks>
public readonly record struct FactionReputation
{
    /// <summary>
    /// The minimum reputation value (floor of the Hated tier).
    /// </summary>
    public const int MinValue = -100;

    /// <summary>
    /// The maximum reputation value (ceiling of the Exalted tier).
    /// </summary>
    public const int MaxValue = 100;

    // Tier boundary constants — lower bound of each tier
    private const int HatedUpperBound = -76;
    private const int HostileLowerBound = -75;
    private const int HostileUpperBound = -26;
    private const int NeutralLowerBound = -25;
    private const int NeutralUpperBound = 24;
    private const int FriendlyLowerBound = 25;
    private const int FriendlyUpperBound = 49;
    private const int AlliedLowerBound = 50;
    private const int AlliedUpperBound = 74;
    private const int ExaltedLowerBound = 75;

    /// <summary>
    /// Gets the faction ID this reputation is associated with.
    /// </summary>
    public string FactionId { get; }

    /// <summary>
    /// Gets the current reputation value, always within [<see cref="MinValue"/>, <see cref="MaxValue"/>].
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets the reputation tier derived from <see cref="Value"/>.
    /// </summary>
    public ReputationTier Tier { get; }

    /// <summary>
    /// Gets the price modifier for this faction's current tier.
    /// </summary>
    /// <value>
    /// A multiplier applied to merchant prices:
    /// 1.50 (Hated), 1.25 (Hostile), 1.00 (Neutral),
    /// 0.90 (Friendly), 0.80 (Allied), 0.70 (Exalted).
    /// </value>
    public double PriceModifier { get; }

    /// <summary>
    /// Private constructor — use factory methods <see cref="Neutral(string)"/>
    /// or <see cref="Create(string, int)"/> instead.
    /// </summary>
    /// <param name="factionId">The faction identifier.</param>
    /// <param name="value">The raw reputation value (will be clamped).</param>
    private FactionReputation(string factionId, int value)
    {
        FactionId = factionId ?? throw new ArgumentNullException(nameof(factionId));
        Value = Math.Clamp(value, MinValue, MaxValue);
        Tier = GetTierForValue(Value);
        PriceModifier = GetPriceModifierForTier(Tier);
    }

    /// <summary>
    /// Creates a new <see cref="FactionReputation"/> at Neutral standing (value = 0).
    /// </summary>
    /// <param name="factionId">The faction identifier.</param>
    /// <returns>A new FactionReputation with Value = 0 and Tier = Neutral.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factionId"/> is null.</exception>
    public static FactionReputation Neutral(string factionId)
    {
        return new FactionReputation(factionId, 0);
    }

    /// <summary>
    /// Creates a new <see cref="FactionReputation"/> at a specific value.
    /// </summary>
    /// <param name="factionId">The faction identifier.</param>
    /// <param name="value">The reputation value (clamped to [-100, +100]).</param>
    /// <returns>A new FactionReputation with the specified value and auto-derived tier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factionId"/> is null.</exception>
    public static FactionReputation Create(string factionId, int value)
    {
        return new FactionReputation(factionId, value);
    }

    /// <summary>
    /// Returns a new <see cref="FactionReputation"/> with the value adjusted by the given delta.
    /// </summary>
    /// <param name="delta">The amount to add (positive) or subtract (negative).</param>
    /// <returns>
    /// A new FactionReputation with an updated Value (clamped to [-100, +100])
    /// and recalculated Tier and PriceModifier.
    /// </returns>
    /// <remarks>
    /// The original instance is unchanged (immutable pattern).
    /// The delta is applied to the current Value, then the result is clamped.
    /// </remarks>
    public FactionReputation WithDelta(int delta)
    {
        return new FactionReputation(FactionId, Value + delta);
    }

    /// <summary>
    /// Derives the <see cref="ReputationTier"/> for a given reputation value.
    /// </summary>
    /// <param name="value">The reputation value to evaluate.</param>
    /// <returns>The tier corresponding to the value.</returns>
    /// <remarks>
    /// Boundary values per faction-reputation design doc (v1.2, Section 2):
    /// Hated: -100 to -76, Hostile: -75 to -26, Neutral: -25 to +24,
    /// Friendly: +25 to +49, Allied: +50 to +74, Exalted: +75 to +100.
    /// </remarks>
    public static ReputationTier GetTierForValue(int value)
    {
        return value switch
        {
            <= HatedUpperBound => ReputationTier.Hated,       // -100 to -76
            <= HostileUpperBound => ReputationTier.Hostile,    // -75 to -26
            <= NeutralUpperBound => ReputationTier.Neutral,    // -25 to +24
            <= FriendlyUpperBound => ReputationTier.Friendly,  // +25 to +49
            <= AlliedUpperBound => ReputationTier.Allied,      // +50 to +74
            _ => ReputationTier.Exalted                        // +75 to +100
        };
    }

    /// <summary>
    /// Gets the price modifier for a given reputation tier.
    /// </summary>
    /// <param name="tier">The reputation tier.</param>
    /// <returns>
    /// A price multiplier: 1.50 (Hated), 1.25 (Hostile), 1.00 (Neutral),
    /// 0.90 (Friendly), 0.80 (Allied), 0.70 (Exalted).
    /// </returns>
    public static double GetPriceModifierForTier(ReputationTier tier)
    {
        return tier switch
        {
            ReputationTier.Hated => 1.50,
            ReputationTier.Hostile => 1.25,
            ReputationTier.Neutral => 1.00,
            ReputationTier.Friendly => 0.90,
            ReputationTier.Allied => 0.80,
            ReputationTier.Exalted => 0.70,
            _ => 1.00 // Fallback to neutral pricing
        };
    }

    /// <summary>
    /// Returns a human-readable representation of this reputation.
    /// </summary>
    /// <returns>A string like "Iron-Banes: Friendly (+35)".</returns>
    public override string ToString()
    {
        return $"{FactionId}: {Tier} ({Value:+#;-#;0})";
    }
}
