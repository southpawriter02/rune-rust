namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Global content budget for an entire sector
/// Prevents over-saturation by setting sector-wide limits instead of per-room budgets
/// Philosophy: 2.0-2.5 enemies per room average, not 4-5
/// </summary>
public class GlobalBudget
{
    /// <summary>
    /// Total enemies allowed in this sector
    /// Calculated: roomCount × 2.2 × difficultyMultiplier × biomeMultiplier
    /// </summary>
    public int TotalEnemyBudget { get; set; }

    /// <summary>
    /// Total hazards allowed in this sector
    /// Calculated: roomCount × 1.5 × difficultyMultiplier × biomeMultiplier
    /// </summary>
    public int TotalHazardBudget { get; set; }

    /// <summary>
    /// Total loot nodes allowed in this sector
    /// Calculated: roomCount × 0.8 (NOT scaled by difficulty)
    /// </summary>
    public int TotalLootBudget { get; set; }

    /// <summary>
    /// Number of enemies actually spawned
    /// Updated as rooms are populated
    /// </summary>
    public int EnemiesSpawned { get; set; } = 0;

    /// <summary>
    /// Number of hazards actually spawned
    /// Updated as rooms are populated
    /// </summary>
    public int HazardsSpawned { get; set; } = 0;

    /// <summary>
    /// Number of loot nodes actually spawned
    /// Updated as rooms are populated
    /// </summary>
    public int LootSpawned { get; set; } = 0;

    /// <summary>
    /// Remaining enemy budget available for allocation
    /// </summary>
    public int RemainingEnemyBudget => TotalEnemyBudget - EnemiesSpawned;

    /// <summary>
    /// Remaining hazard budget available for allocation
    /// </summary>
    public int RemainingHazardBudget => TotalHazardBudget - HazardsSpawned;

    /// <summary>
    /// Remaining loot budget available for allocation
    /// </summary>
    public int RemainingLootBudget => TotalLootBudget - LootSpawned;

    /// <summary>
    /// Whether the enemy budget has been exhausted
    /// </summary>
    public bool IsEnemyBudgetExhausted => RemainingEnemyBudget <= 0;

    /// <summary>
    /// Whether the hazard budget has been exhausted
    /// </summary>
    public bool IsHazardBudgetExhausted => RemainingHazardBudget <= 0;

    /// <summary>
    /// Whether the loot budget has been exhausted
    /// </summary>
    public bool IsLootBudgetExhausted => RemainingLootBudget <= 0;
}
