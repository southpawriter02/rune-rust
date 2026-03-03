namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Value object for tracking and calculating [Echo] chain propagation state.
/// The [Echo] chain system allows abilities to bounce damage to adjacent targets
/// with scaling based on the Echo-Caller's invested abilities.
/// </summary>
/// <remarks>
/// <para>Echo chains propagate damage without applying additional effects (fear, disorientation, etc.).
/// Range and damage scaling depend on whether EchoCascade (28013) passive is unlocked and its rank:
/// <list type="table">
///   <listheader><term>State</term><description>Range / Damage / Max Targets</description></listheader>
///   <item><term>No EchoCascade</term><description>1 tile / 50% / 1 target</description></item>
///   <item><term>EchoCascade Rank 2</term><description>2 tiles / 70% / 2 targets</description></item>
///   <item><term>EchoCascade Rank 3</term><description>3 tiles / 80% / 2 targets</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed record EchoChainTracker
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EchoChainTracker"/> record.
    /// </summary>
    /// <param name="targetId">Unique identifier of the primary target that triggered the echo.</param>
    /// <param name="targetName">Human-readable name of the primary target.</param>
    /// <param name="chainRange">Range of the echo in tiles (1/2/3 depending on EchoCascade rank).</param>
    /// <param name="chainDamagePercent">Percentage of primary damage applied to echoed targets (50/70/80).</param>
    /// <param name="maxChainTargets">Maximum number of targets the echo can hit (1 or 2).</param>
    /// <param name="hasEchoCascade">Whether EchoCascade passive is unlocked.</param>
    /// <exception cref="ArgumentException">Thrown if chainRange is not 1, 2, or 3.</exception>
    /// <exception cref="ArgumentException">Thrown if chainDamagePercent is not 50, 70, or 80.</exception>
    /// <exception cref="ArgumentException">Thrown if maxChainTargets is not 1 or 2.</exception>
    public EchoChainTracker(
        Guid targetId,
        string targetName,
        int chainRange,
        int chainDamagePercent,
        int maxChainTargets,
        bool hasEchoCascade)
    {
        if (chainRange is not (1 or 2 or 3))
            throw new ArgumentException("Chain range must be 1, 2, or 3 tiles.", nameof(chainRange));

        if (chainDamagePercent is not (50 or 70 or 80))
            throw new ArgumentException("Chain damage percent must be 50, 70, or 80.", nameof(chainDamagePercent));

        if (maxChainTargets is not (1 or 2))
            throw new ArgumentException("Max chain targets must be 1 or 2.", nameof(maxChainTargets));

        TargetId = targetId;
        TargetName = targetName;
        ChainRange = chainRange;
        ChainDamagePercent = chainDamagePercent;
        MaxChainTargets = maxChainTargets;
        HasEchoCascade = hasEchoCascade;
    }

    /// <summary>Unique identifier of the primary target triggering the echo.</summary>
    public Guid TargetId { get; }

    /// <summary>Human-readable name of the primary target.</summary>
    public string TargetName { get; }

    /// <summary>Range of the echo chain in tiles (1/2/3).</summary>
    public int ChainRange { get; }

    /// <summary>Percentage of primary damage applied to echoed targets (50/70/80).</summary>
    public int ChainDamagePercent { get; }

    /// <summary>Maximum number of targets the echo can affect (1 or 2).</summary>
    public int MaxChainTargets { get; }

    /// <summary>Whether the Echo-Caller has unlocked the EchoCascade passive (28013).</summary>
    public bool HasEchoCascade { get; }

    /// <summary>
    /// Enhances the echo chain with EchoCascade passive data.
    /// Returns a new tracker with upgraded range, damage, and target count based on the rank.
    /// </summary>
    /// <param name="echoCascadeRank">The rank of the EchoCascade passive (2 or 3).</param>
    /// <returns>A new EchoChainTracker with enhanced cascade properties.</returns>
    /// <exception cref="ArgumentException">Thrown if rank is not 2 or 3.</exception>
    public EchoChainTracker WithEnhancedCascade(int echoCascadeRank)
    {
        if (!HasEchoCascade)
            return this;

        if (echoCascadeRank is not (2 or 3))
            throw new ArgumentException("EchoCascade rank must be 2 or 3.", nameof(echoCascadeRank));

        return echoCascadeRank switch
        {
            2 => new EchoChainTracker(TargetId, TargetName, 2, 70, 2, true),
            3 => new EchoChainTracker(TargetId, TargetName, 3, 80, 2, true),
            _ => this
        };
    }

    /// <summary>
    /// Calculates the echo chain damage based on the primary ability's base damage.
    /// </summary>
    /// <param name="baseDamage">The primary ability's damage to the main target.</param>
    /// <returns>The calculated echo damage (percentage of base damage).</returns>
    /// <exception cref="ArgumentException">Thrown if baseDamage is less than 0.</exception>
    public int CalculateChainDamage(int baseDamage)
    {
        if (baseDamage < 0)
            throw new ArgumentException("Base damage cannot be negative.", nameof(baseDamage));

        return (baseDamage * ChainDamagePercent) / 100;
    }
}
