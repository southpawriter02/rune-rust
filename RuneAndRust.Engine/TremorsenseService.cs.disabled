using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of applying Tremorsense detection
/// </summary>
public class TremorsenseDetectionResult
{
    public bool Success { get; set; }
    public List<int> GroundEnemiesDetected { get; set; } = new();
    public int FlyingEnemiesCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Attack modifiers applied when targeting flying enemies
/// </summary>
public class AttackModifiers
{
    public float MissChance { get; set; }
    public int DefenseBonus { get; set; }
}

/// <summary>
/// v0.26.2: Service for Tremorsense perception mechanics
/// Handles seismic perception system for GorgeMawAscetic specialization
/// </summary>
public class TremorsenseService
{
    private static readonly ILogger _log = Log.ForContext<TremorsenseService>();
    private readonly DatabaseService _database;

    public TremorsenseService(string connectionString)
    {
        _database = new DatabaseService(connectionString);
        _log.Debug("TremorsenseService initialized");
    }

    /// <summary>
    /// Detect all ground-based enemies through earth vibrations
    /// </summary>
    public TremorsenseDetectionResult DetectGroundEnemies(int characterId, List<Enemy> allEnemies)
    {
        _log.Information(
            "Tremorsense detection initiated: CharacterID={CharacterId}, TotalEnemies={Count}",
            characterId, allEnemies.Count);

        var groundEnemies = allEnemies
            .Where(e => !e.IsFlying)
            .Select(e => e.EnemyID)
            .ToList();

        var flyingCount = allEnemies.Count(e => e.IsFlying);

        _log.Information(
            "Tremorsense detection complete: CharacterID={CharacterId}, GroundEnemies={GroundCount}, FlyingEnemies={FlyingCount}",
            characterId, groundEnemies.Count, flyingCount);

        return new TremorsenseDetectionResult
        {
            Success = true,
            GroundEnemiesDetected = groundEnemies,
            FlyingEnemiesCount = flyingCount,
            Message = $"Tremorsense detects {groundEnemies.Count} ground enemies ({flyingCount} flying enemies undetectable)"
        };
    }

    /// <summary>
    /// Check if enemy is flying and apply appropriate penalties
    /// </summary>
    public AttackModifiers ApplyFlyingPenalty(int characterId, Enemy target)
    {
        if (!target.IsFlying)
        {
            return new AttackModifiers { MissChance = 0, DefenseBonus = 0 };
        }

        _log.Warning(
            "Tremorsense flying penalty applied: CharacterID={CharacterId}, TargetID={TargetId}, TargetName={TargetName}, MissChance=50%",
            characterId, target.EnemyID, target.Name);

        return new AttackModifiers
        {
            MissChance = 0.5f, // 50% miss chance
            DefenseBonus = 0
        };
    }

    /// <summary>
    /// Check immunity to vision impairment effects
    /// </summary>
    public bool IsImmuneToVisionImpairment(int characterId)
    {
        _log.Debug("Tremorsense vision immunity check: CharacterID={CharacterId}", characterId);
        return true; // Always immune with Tremorsense
    }

    /// <summary>
    /// Auto-detect hidden/stealth ground enemies
    /// </summary>
    public List<int> DetectStealthedGroundEnemies(int characterId, List<Enemy> enemies)
    {
        var stealthedGroundEnemies = enemies
            .Where(e => !e.IsFlying && (e.IsHidden || e.IsStealth))
            .Select(e => e.EnemyID)
            .ToList();

        if (stealthedGroundEnemies.Any())
        {
            _log.Information(
                "Tremorsense auto-detects stealthed ground enemies: CharacterID={CharacterId}, StealthedCount={Count}",
                characterId, stealthedGroundEnemies.Count);
        }

        return stealthedGroundEnemies;
    }

    /// <summary>
    /// Calculate defense modifier when attacked by flying enemy
    /// </summary>
    public int GetDefenseVsFlyingAttack(int characterId)
    {
        _log.Warning(
            "Tremorsense cannot defend vs flying attack: CharacterID={CharacterId}, Defense=0",
            characterId);

        return 0; // 0 defense against flying enemies
    }
}
