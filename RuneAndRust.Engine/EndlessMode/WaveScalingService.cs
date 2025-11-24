using RuneAndRust.Core.EndlessMode;
using Serilog;

namespace RuneAndRust.Engine.EndlessMode;

/// <summary>
/// v0.40.4: Wave Scaling Service
/// Calculates wave difficulty scaling formulas
/// </summary>
public class WaveScalingService
{
    private static readonly ILogger _log = Log.ForContext<WaveScalingService>();

    // ═══════════════════════════════════════════════════════════
    // WAVE CONFIGURATION GENERATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate complete wave configuration for a given wave number
    /// </summary>
    public WaveConfiguration GenerateWaveConfig(int waveNumber)
    {
        if (waveNumber < 1)
        {
            throw new ArgumentException("Wave number must be >= 1", nameof(waveNumber));
        }

        var config = new WaveConfiguration
        {
            WaveNumber = waveNumber,
            EnemyCount = CalculateEnemyCount(waveNumber),
            EnemyLevel = CalculateEnemyLevel(waveNumber),
            DifficultyMultiplier = CalculateDifficultyMultiplier(waveNumber),
            IsBossWave = IsBossWave(waveNumber)
        };

        _log.Debug("Generated wave {Wave}: {Count} enemies, Level {Level}, {Diff}x difficulty, Boss={Boss}",
            waveNumber, config.EnemyCount, config.EnemyLevel, config.DifficultyMultiplier, config.IsBossWave);

        return config;
    }

    // ═══════════════════════════════════════════════════════════
    // SCALING FORMULAS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate enemy count for a wave
    /// Formula: 3 + (wave / 2)
    /// </summary>
    public int CalculateEnemyCount(int wave)
    {
        // Wave 1: 3 enemies
        // Wave 10: 8 enemies
        // Wave 20: 13 enemies
        // Wave 50: 28 enemies
        // Wave 100: 53 enemies
        return 3 + (wave / 2);
    }

    /// <summary>
    /// Calculate enemy level for a wave
    /// Formula: 5 + wave
    /// </summary>
    public int CalculateEnemyLevel(int wave)
    {
        // Wave 1: Level 6
        // Wave 10: Level 15
        // Wave 30: Level 35
        // Wave 50: Level 55
        // Wave 100: Level 105
        return 5 + wave;
    }

    /// <summary>
    /// Calculate difficulty multiplier for a wave
    /// Formula: 1.0 + (wave * 0.1)
    /// </summary>
    public float CalculateDifficultyMultiplier(int wave)
    {
        // Wave 1: 1.0x
        // Wave 10: 2.0x
        // Wave 20: 3.0x
        // Wave 50: 6.0x
        // Wave 100: 11.0x
        return 1.0f + (wave * 0.1f);
    }

    /// <summary>
    /// Determine if a wave is a boss wave
    /// Boss waves occur every 5 waves
    /// </summary>
    public bool IsBossWave(int wave)
    {
        return wave % 5 == 0;
    }

    // ═══════════════════════════════════════════════════════════
    // SCALING ESTIMATES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Estimate total enemies for reaching a specific wave
    /// </summary>
    public int EstimateTotalEnemiesForWave(int targetWave)
    {
        int total = 0;
        for (int wave = 1; wave <= targetWave; wave++)
        {
            total += CalculateEnemyCount(wave);
        }
        return total;
    }

    /// <summary>
    /// Estimate total time for reaching a specific wave (minutes)
    /// Assumes average wave time scales with difficulty
    /// </summary>
    public float EstimateTotalTimeForWave(int targetWave)
    {
        float totalMinutes = 0;

        for (int wave = 1; wave <= targetWave; wave++)
        {
            // Base time: 2 minutes per wave
            // Scales with difficulty
            float baseTime = 2.0f;
            float difficultyMultiplier = CalculateDifficultyMultiplier(wave);
            float waveTime = baseTime * (1 + (difficultyMultiplier - 1.0f) / 2);

            totalMinutes += waveTime;
        }

        return totalMinutes;
    }
}

/// <summary>
/// Wave configuration data
/// </summary>
public class WaveConfiguration
{
    /// <summary>Wave number</summary>
    public int WaveNumber { get; set; }

    /// <summary>Number of enemies</summary>
    public int EnemyCount { get; set; }

    /// <summary>Enemy level</summary>
    public int EnemyLevel { get; set; }

    /// <summary>Difficulty multiplier</summary>
    public float DifficultyMultiplier { get; set; }

    /// <summary>Is this a boss wave?</summary>
    public bool IsBossWave { get; set; }

    /// <summary>Display text</summary>
    public string DisplayText => IsBossWave
        ? $"BOSS WAVE {WaveNumber}: {EnemyCount} enemies, Level {EnemyLevel}, {DifficultyMultiplier:F1}x"
        : $"Wave {WaveNumber}: {EnemyCount} enemies, Level {EnemyLevel}, {DifficultyMultiplier:F1}x";
}
