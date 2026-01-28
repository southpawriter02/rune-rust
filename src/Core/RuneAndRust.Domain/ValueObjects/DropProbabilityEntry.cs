namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the drop probability distribution for an enemy class.
/// </summary>
/// <remarks>
/// <para>
/// Each enemy class has a fixed probability distribution that determines
/// what quality tier of item drops when the enemy is defeated. The sum
/// of all tier probabilities plus the no-drop chance must equal 1.0 (100%).
/// </para>
/// <para>
/// The <see cref="RollDrop"/> method implements weighted random selection
/// using cumulative probability thresholds.
/// </para>
/// </remarks>
public sealed class DropProbabilityEntry
{
    /// <summary>
    /// Gets the enemy class this entry applies to.
    /// </summary>
    public EnemyDropClass EnemyClass { get; }

    /// <summary>
    /// Gets the probability for each quality tier (0.0-1.0).
    /// </summary>
    public IReadOnlyDictionary<QualityTier, decimal> TierProbabilities { get; }

    /// <summary>
    /// Gets the chance of no drop occurring (0.0-1.0).
    /// </summary>
    public decimal NoDropChance { get; }

    /// <summary>
    /// Gets whether this entry can result in no drop.
    /// </summary>
    public bool CanDropNothing => NoDropChance > 0;

    /// <summary>
    /// Gets whether this entry guarantees a drop.
    /// </summary>
    public bool GuaranteedDrop => NoDropChance == 0;

    /// <summary>
    /// Gets the highest tier that can drop from this class.
    /// </summary>
    public QualityTier HighestPossibleTier =>
        TierProbabilities
            .Where(kvp => kvp.Value > 0)
            .OrderByDescending(kvp => (int)kvp.Key)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

    /// <summary>
    /// Gets the lowest tier that can drop from this class.
    /// </summary>
    public QualityTier LowestPossibleTier =>
        TierProbabilities
            .Where(kvp => kvp.Value > 0)
            .OrderBy(kvp => (int)kvp.Key)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

    // Private constructor for factory method
    private DropProbabilityEntry(
        EnemyDropClass enemyClass,
        IReadOnlyDictionary<QualityTier, decimal> tierProbabilities,
        decimal noDropChance)
    {
        EnemyClass = enemyClass;
        TierProbabilities = tierProbabilities;
        NoDropChance = noDropChance;
    }

    /// <summary>
    /// Creates a new DropProbabilityEntry with validation.
    /// </summary>
    /// <param name="enemyClass">The enemy drop class.</param>
    /// <param name="tierProbabilities">Probability for each quality tier.</param>
    /// <param name="noDropChance">Chance of no drop.</param>
    /// <returns>A new DropProbabilityEntry instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when tierProbabilities is null.</exception>
    /// <exception cref="ArgumentException">Thrown when probabilities don't sum to 1.0.</exception>
    public static DropProbabilityEntry Create(
        EnemyDropClass enemyClass,
        IReadOnlyDictionary<QualityTier, decimal> tierProbabilities,
        decimal noDropChance)
    {
        ArgumentNullException.ThrowIfNull(tierProbabilities);
        ArgumentOutOfRangeException.ThrowIfNegative(noDropChance);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(noDropChance, 1.0m);

        // Validate probabilities sum to 1.0
        var totalProbability = tierProbabilities.Values.Sum() + noDropChance;
        if (Math.Abs(totalProbability - 1.0m) > 0.001m)
        {
            throw new ArgumentException(
                $"Probabilities must sum to 1.0, but got {totalProbability:F3}",
                nameof(tierProbabilities));
        }

        // Validate no negative probabilities
        if (tierProbabilities.Any(kvp => kvp.Value < 0))
        {
            throw new ArgumentException(
                "Tier probabilities cannot be negative",
                nameof(tierProbabilities));
        }

        return new DropProbabilityEntry(enemyClass, tierProbabilities, noDropChance);
    }

    /// <summary>
    /// Rolls for a drop result using weighted random selection.
    /// </summary>
    /// <param name="random">Random number generator.</param>
    /// <returns>The drop roll result indicating tier or no-drop.</returns>
    /// <exception cref="ArgumentNullException">Thrown when random is null.</exception>
    public DropRollResult RollDrop(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var roll = (decimal)random.NextDouble();
        var cumulative = 0m;

        // Check each tier in order (lowest to highest)
        foreach (var tier in Enum.GetValues<QualityTier>())
        {
            if (TierProbabilities.TryGetValue(tier, out var probability) && probability > 0)
            {
                cumulative += probability;
                if (roll < cumulative)
                {
                    return DropRollResult.Dropped(tier, roll);
                }
            }
        }

        // If we get here, it's a no-drop
        return DropRollResult.NoDrop(roll);
    }

    /// <summary>
    /// Gets the probability for a specific tier.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <returns>The probability (0.0-1.0), or 0 if tier not in table.</returns>
    public decimal GetTierProbability(QualityTier tier)
    {
        return TierProbabilities.TryGetValue(tier, out var prob) ? prob : 0m;
    }

    /// <summary>
    /// Validates that this entry is properly configured.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool Validate()
    {
        var total = TierProbabilities.Values.Sum() + NoDropChance;
        return Math.Abs(total - 1.0m) < 0.001m;
    }

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    public override string ToString()
    {
        var tierStr = string.Join(", ",
            TierProbabilities
                .Where(kvp => kvp.Value > 0)
                .OrderBy(kvp => (int)kvp.Key)
                .Select(kvp => $"T{(int)kvp.Key}:{kvp.Value:P0}"));
        var noDropStr = NoDropChance > 0 ? $", None:{NoDropChance:P0}" : "";
        return $"{EnemyClass}: {tierStr}{noDropStr}";
    }
}
