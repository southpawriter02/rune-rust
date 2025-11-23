using RuneAndRust.Core.Population;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.3: Global content density budget calculator
/// Prevents over-saturation by calculating sector-wide budgets instead of per-room budgets
/// Philosophy: 2.0-2.5 enemies per room average, not 4-5
/// </summary>
public class ContentDensityService
{
    private static readonly ILogger _log = Log.ForContext<ContentDensityService>();

    // Base budget per room (lower than v0.11's per-room budgets)
    private const float BaseEnemiesPerRoom = 2.2f;
    private const float BaseHazardsPerRoom = 1.5f;
    private const float BaseLootPerRoom = 0.8f;

    /// <summary>
    /// Calculates global content budget for an entire sector
    /// </summary>
    /// <param name="roomCount">Number of rooms in the sector</param>
    /// <param name="difficulty">Difficulty tier (affects enemy/hazard scaling)</param>
    /// <param name="biomeId">Biome identifier (affects density multiplier)</param>
    /// <returns>Global budget for the sector</returns>
    public GlobalBudget CalculateGlobalBudget(
        int roomCount,
        DifficultyTier difficulty,
        string biomeId)
    {
        if (roomCount <= 0)
        {
            _log.Warning("Invalid room count {RoomCount}, returning empty budget", roomCount);
            return new GlobalBudget();
        }

        // Calculate base budgets
        var baseEnemies = (int)(roomCount * BaseEnemiesPerRoom);
        var baseHazards = (int)(roomCount * BaseHazardsPerRoom);
        var baseLoot = (int)(roomCount * BaseLootPerRoom);

        // Apply difficulty multiplier
        var difficultyMultiplier = GetDifficultyMultiplier(difficulty);

        // Apply biome multiplier
        var biomeMultiplier = GetBiomeMultiplier(biomeId);

        // Calculate final budgets
        var budget = new GlobalBudget
        {
            TotalEnemyBudget = (int)(baseEnemies * difficultyMultiplier * biomeMultiplier),
            TotalHazardBudget = (int)(baseHazards * difficultyMultiplier * biomeMultiplier),
            TotalLootBudget = baseLoot, // Loot NOT scaled by difficulty
            EnemiesSpawned = 0,
            HazardsSpawned = 0,
            LootSpawned = 0
        };

        _log.Information(
            "Global budget calculated: {Rooms} rooms, {Difficulty} difficulty, {Biome} biome → " +
            "Enemies={Enemies}, Hazards={Hazards}, Loot={Loot}",
            roomCount, difficulty, biomeId,
            budget.TotalEnemyBudget, budget.TotalHazardBudget, budget.TotalLootBudget);

        return budget;
    }

    /// <summary>
    /// Gets difficulty multiplier for budget scaling
    /// </summary>
    private float GetDifficultyMultiplier(DifficultyTier difficulty)
    {
        return difficulty switch
        {
            DifficultyTier.Easy => 0.8f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Lethal => 1.6f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// Gets biome-specific density multiplier
    /// Some biomes are inherently more dangerous than others
    /// </summary>
    private float GetBiomeMultiplier(string biomeId)
    {
        return biomeId.ToLowerInvariant() switch
        {
            "the_roots" or "theroots" => 1.0f,          // Baseline
            "muspelheim" => 1.2f,                        // Fire realm - more dangerous
            "niflheim" => 1.2f,                          // Ice realm - more dangerous
            "alfheim" => 0.9f,                           // Light realm - slightly less dense
            "jotunheim" => 1.3f,                         // Giant realm - most dangerous
            _ => 1.0f                                    // Default for unknown biomes
        };
    }
}
