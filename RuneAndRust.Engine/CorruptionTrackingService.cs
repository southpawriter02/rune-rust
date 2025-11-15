using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.24.1: Service for tracking and exploiting enemy corruption
/// Used by Veiðimaðr (Hunter) specialization to identify corruption levels
/// and apply corruption-based bonuses
/// </summary>
public class CorruptionTrackingService
{
    private static readonly ILogger _log = Log.ForContext<CorruptionTrackingService>();
    private readonly string _connectionString;

    public CorruptionTrackingService(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("CorruptionTrackingService initialized");
    }

    /// <summary>
    /// Corruption level categories for gameplay mechanics
    /// </summary>
    public enum CorruptionLevel
    {
        None = 0,      // 0 Corruption
        Low = 1,       // 1-29 Corruption
        Medium = 2,    // 30-59 Corruption
        High = 3,      // 60-89 Corruption
        Extreme = 4    // 90+ Corruption
    }

    /// <summary>
    /// Get the corruption level category for an entity
    /// </summary>
    public CorruptionLevel GetCorruptionLevel(int corruptionAmount)
    {
        return corruptionAmount switch
        {
            >= 90 => CorruptionLevel.Extreme,
            >= 60 => CorruptionLevel.High,
            >= 30 => CorruptionLevel.Medium,
            >= 1 => CorruptionLevel.Low,
            _ => CorruptionLevel.None
        };
    }

    /// <summary>
    /// Get the corruption level for an enemy entity
    /// </summary>
    public CorruptionLevel GetEnemyCorruptionLevel(Enemy enemy)
    {
        int corruption = enemy.Corruption;
        var level = GetCorruptionLevel(corruption);

        _log.Debug("Enemy {EnemyName} corruption level: {Corruption} ({Level})",
            enemy.Name, corruption, level);

        return level;
    }

    /// <summary>
    /// Calculate Exploit Corruption critical hit bonus based on rank and corruption level
    /// </summary>
    public int GetExploitCorruptionCritBonus(int abilityRank, CorruptionLevel corruptionLevel)
    {
        // Base bonuses by corruption level (Rank 1)
        int baseBonus = corruptionLevel switch
        {
            CorruptionLevel.Low => 5,
            CorruptionLevel.Medium => 10,
            CorruptionLevel.High => 15,
            CorruptionLevel.Extreme => 20,
            _ => 0
        };

        // Rank 2 doubles the bonus
        if (abilityRank >= 2)
        {
            baseBonus *= 2;
        }

        _log.Debug("Exploit Corruption crit bonus: Rank={Rank}, Level={Level}, Bonus={Bonus}%",
            abilityRank, corruptionLevel, baseBonus);

        return baseBonus;
    }

    /// <summary>
    /// Calculate critical hit chance bonus vs corrupted target
    /// </summary>
    public int CalculateCritChanceBonus(PlayerCharacter hunter, Enemy target)
    {
        // Check if hunter has Exploit Corruption ability
        var exploitCorruptionRank = GetAbilityRank(hunter, "Exploit Corruption");
        if (exploitCorruptionRank == 0)
        {
            return 0; // No bonus if ability not learned
        }

        var corruptionLevel = GetEnemyCorruptionLevel(target);
        return GetExploitCorruptionCritBonus(exploitCorruptionRank, corruptionLevel);
    }

    /// <summary>
    /// Purge corruption from a target, returning the amount purged
    /// </summary>
    public int PurgeCorruption(Enemy target, int maxPurgeAmount)
    {
        int initialCorruption = target.Corruption;
        int purged = Math.Min(maxPurgeAmount, initialCorruption);

        target.Corruption = Math.Max(0, initialCorruption - purged);

        _log.Information(
            "Corruption purged: Target={TargetName}, Amount={Purged}, Remaining={Remaining}",
            target.Name, purged, target.Corruption);

        return purged;
    }

    /// <summary>
    /// Check if target meets corruption threshold for abilities (e.g., Blight-Tipped Arrow Glitch)
    /// </summary>
    public bool MeetsCorruptionThreshold(Enemy target, int threshold)
    {
        return target.Corruption >= threshold;
    }

    /// <summary>
    /// Get ability rank for a character (helper method)
    /// </summary>
    private int GetAbilityRank(PlayerCharacter character, string abilityName)
    {
        var ability = character.Abilities.FirstOrDefault(a => a.Name == abilityName);
        return ability?.CurrentRank ?? 0;
    }
}
