using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.44.3: Encounter generation service for dungeon exploration.
/// Handles random encounters, room encounters, search triggers, and rest interrupts.
/// Uses Aethelgard terminology: enemies are "Undying", encounters are "patrols".
/// </summary>
public class EncounterService : IEncounterService
{
    private readonly ILogger _logger;
    private readonly Random _random;

    // Encounter chance configuration
    private const float BaseRandomEncounterChance = 0.15f;  // 15% base chance on room entry
    private const float SearchEncounterChance = 0.10f;       // 10% chance when searching
    private const float RestInterruptChance = 0.20f;         // 20% chance during non-sanctuary rest

    public EncounterService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    /// <inheritdoc/>
    public Task<RandomEncounterResult> RollForRandomEncounterAsync(GameState gameState)
    {
        if (gameState.Player == null || gameState.CurrentRoom == null)
        {
            return Task.FromResult(RandomEncounterResult.NoEncounter());
        }

        var room = gameState.CurrentRoom;
        var player = gameState.Player;

        // No random encounters in sanctuaries
        if (room.IsSanctuary)
        {
            _logger.Debug("No encounter - sanctuary room: {RoomId}", room.RoomId);
            return Task.FromResult(RandomEncounterResult.NoEncounter());
        }

        // No random encounters in cleared rooms (reduced chance)
        var encounterChance = room.HasBeenCleared
            ? BaseRandomEncounterChance * 0.25f  // 75% reduction in cleared rooms
            : BaseRandomEncounterChance;

        // Modify by psychic resonance
        encounterChance *= GetPsychicResonanceModifier(room.PsychicResonance);

        // Roll for encounter
        var roll = _random.NextDouble();
        if (roll > encounterChance)
        {
            _logger.Debug("No encounter triggered - roll {Roll} > chance {Chance}", roll, encounterChance);
            return Task.FromResult(RandomEncounterResult.NoEncounter());
        }

        // Generate encounter
        var enemies = GenerateRandomEnemies(room, player.Level);
        var difficulty = CalculateDifficulty(enemies, player.Level);
        var description = GenerateEncounterDescription(enemies, room);

        _logger.Information("Random Undying patrol encountered: {Count} enemies, difficulty {Difficulty}",
            enemies.Count, difficulty);

        return Task.FromResult(RandomEncounterResult.WithEncounter(enemies, difficulty, description));
    }

    /// <inheritdoc/>
    public List<Enemy> GenerateRoomEncounter(Room room, int playerLevel)
    {
        // If room already has enemies defined, return those
        if (room.Enemies.Count > 0)
        {
            return room.Enemies;
        }

        // Generate appropriate enemies for room type
        var enemyCount = DetermineEnemyCount(room, playerLevel);
        var enemies = new List<Enemy>();

        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(CreateEnemyForRoom(room, playerLevel, i));
        }

        return enemies;
    }

    /// <inheritdoc/>
    public RandomEncounterResult GenerateSearchEncounter(Room room, int playerLevel)
    {
        // Sanctuaries never have search encounters
        if (room.IsSanctuary)
        {
            return RandomEncounterResult.NoEncounter();
        }

        // Already cleared rooms have reduced chance
        var chance = room.HasBeenCleared ? SearchEncounterChance * 0.5f : SearchEncounterChance;

        if (_random.NextDouble() > chance)
        {
            return RandomEncounterResult.NoEncounter();
        }

        // Generate weak encounter (disturbed dormant Undying)
        var enemies = new List<Enemy>
        {
            CreateEnemyForRoom(room, Math.Max(1, playerLevel - 1), 0)
        };

        // 30% chance for second enemy
        if (_random.NextDouble() < 0.3)
        {
            enemies.Add(CreateEnemyForRoom(room, Math.Max(1, playerLevel - 1), 1));
        }

        _logger.Information("Search disturbed dormant Undying in room {RoomId}", room.RoomId);

        return RandomEncounterResult.WithEncounter(
            enemies,
            CalculateDifficulty(enemies, playerLevel),
            "Your searching disturbs something lurking in the shadows...",
            canFlee: true
        );
    }

    /// <inheritdoc/>
    public RandomEncounterResult GenerateRestInterruptEncounter(Room room, bool isSanctuary, int playerLevel)
    {
        // Sanctuaries never have rest interrupts
        if (isSanctuary)
        {
            _logger.Debug("Safe rest in sanctuary - no interrupt possible");
            return RandomEncounterResult.NoEncounter();
        }

        // Roll for interrupt
        if (_random.NextDouble() > RestInterruptChance)
        {
            return RandomEncounterResult.NoEncounter();
        }

        // Generate patrol that found the resting survivor
        var enemies = GenerateRandomEnemies(room, playerLevel);

        _logger.Warning("Field rest interrupted by Undying patrol in room {RoomId}", room.RoomId);

        return RandomEncounterResult.WithEncounter(
            enemies,
            CalculateDifficulty(enemies, playerLevel),
            "Your rest is interrupted! Undying patrol stumbles upon your position!",
            canFlee: true
        );
    }

    #region Private Helper Methods

    private float GetPsychicResonanceModifier(PsychicResonanceLevel resonance)
    {
        return resonance switch
        {
            PsychicResonanceLevel.None => 0.8f,      // -20%
            PsychicResonanceLevel.Light => 1.0f,     // Base
            PsychicResonanceLevel.Moderate => 1.25f, // +25%
            PsychicResonanceLevel.Heavy => 1.5f,     // +50%
            PsychicResonanceLevel.Overwhelming => 2.0f, // +100%
            _ => 1.0f
        };
    }

    private List<Enemy> GenerateRandomEnemies(Room room, int playerLevel)
    {
        var enemies = new List<Enemy>();
        var enemyCount = DetermineRandomEnemyCount(playerLevel);

        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(CreateEnemyForRoom(room, playerLevel, i));
        }

        return enemies;
    }

    private int DetermineRandomEnemyCount(int playerLevel)
    {
        // Base 1-3 enemies, scaling slightly with level
        var baseCount = _random.Next(1, 4);
        var levelBonus = playerLevel / 5; // +1 enemy per 5 levels
        return Math.Min(baseCount + levelBonus, 6); // Cap at 6
    }

    private int DetermineEnemyCount(Room room, int playerLevel)
    {
        if (room.IsBossRoom) return 1; // Boss rooms have single boss

        // Based on room density classification if set
        if (room.DensityClassification.HasValue)
        {
            return room.DensityClassification.Value switch
            {
                Core.Population.RoomDensity.Empty => 0,
                Core.Population.RoomDensity.Light => _random.Next(1, 3),
                Core.Population.RoomDensity.Medium => _random.Next(2, 4),
                Core.Population.RoomDensity.Heavy => _random.Next(3, 6),
                Core.Population.RoomDensity.Boss => 1,
                _ => _random.Next(1, 4)
            };
        }

        return _random.Next(1, 4);
    }

    private Enemy CreateEnemyForRoom(Room room, int playerLevel, int index)
    {
        var biome = room.PrimaryBiome?.ToLower() ?? "the_roots";
        var enemyType = GetEnemyTypeForBiome(biome);
        var name = GetEnemyName(enemyType, index);

        // Scale stats based on player level
        var baseHP = 20 + (playerLevel * 5);
        var hpVariance = _random.Next(-5, 6);

        return new Enemy
        {
            Type = enemyType,
            Name = name,
            HP = baseHP + hpVariance,
            MaxHP = baseHP + hpVariance,
            Attack = 5 + playerLevel,
            Defense = 2 + (playerLevel / 2),
            Level = playerLevel
        };
    }

    private EnemyType GetEnemyTypeForBiome(string biome)
    {
        return biome.ToLower() switch
        {
            "the_roots" or "the roots" => GetRootsEnemy(),
            "muspelheim" => GetMuspelheimEnemy(),
            "niflheim" => GetNiflheimEnemy(),
            "jotunheim" => GetJotunheimEnemy(),
            "alfheim" => GetAlfheimEnemy(),
            _ => GetRootsEnemy()
        };
    }

    private EnemyType GetRootsEnemy()
    {
        // The Roots - rusted machinery, corrupted drones
        var enemies = new[] { EnemyType.CorrodedSentry, EnemyType.ScrapHound, EnemyType.CorruptedServitor, EnemyType.BlightDrone };
        return enemies[_random.Next(enemies.Length)];
    }

    private EnemyType GetMuspelheimEnemy()
    {
        // Muspelheim (Fire) - forge constructs, heat-based enemies
        var enemies = new[] { EnemyType.ArcWelderUnit, EnemyType.HuskEnforcer, EnemyType.WarFrame };
        return enemies[_random.Next(enemies.Length)];
    }

    private EnemyType GetNiflheimEnemy()
    {
        // Niflheim (Ice) - preserved specimens, cold constructs
        var enemies = new[] { EnemyType.MaintenanceConstruct, EnemyType.TestSubject, EnemyType.ForlornScholar };
        return enemies[_random.Next(enemies.Length)];
    }

    private EnemyType GetJotunheimEnemy()
    {
        // Jotunheim (Machine) - heavy constructs, titans
        var enemies = new[] { EnemyType.FailureColossus, EnemyType.VaultCustodian, EnemyType.JotunReaderFragment };
        return enemies[_random.Next(enemies.Length)];
    }

    private EnemyType GetAlfheimEnemy()
    {
        // Alfheim (Light) - aetheric entities, psychic horrors
        var enemies = new[] { EnemyType.Shrieker, EnemyType.AethericAberration, EnemyType.RustWitch };
        return enemies[_random.Next(enemies.Length)];
    }

    private string GetEnemyName(EnemyType type, int index)
    {
        var baseName = type switch
        {
            EnemyType.CorruptedServitor => "Corrupted Servitor",
            EnemyType.BlightDrone => "Blight Drone",
            EnemyType.RuinWarden => "Ruin Warden",
            EnemyType.ScrapHound => "Scrap Hound",
            EnemyType.TestSubject => "Test Subject",
            EnemyType.WarFrame => "War Frame",
            EnemyType.ForlornScholar => "Forlorn Scholar",
            EnemyType.AethericAberration => "Aetheric Aberration",
            EnemyType.MaintenanceConstruct => "Maintenance Construct",
            EnemyType.SludgeCrawler => "Sludge Crawler",
            EnemyType.CorruptedEngineer => "Corrupted Engineer",
            EnemyType.VaultCustodian => "Vault Custodian",
            EnemyType.ForlornArchivist => "Forlorn Archivist",
            EnemyType.OmegaSentinel => "Omega Sentinel",
            EnemyType.CorrodedSentry => "Corroded Sentry",
            EnemyType.HuskEnforcer => "Husk Enforcer",
            EnemyType.ArcWelderUnit => "Arc Welder Unit",
            EnemyType.Shrieker => "Shrieker",
            EnemyType.JotunReaderFragment => "J-Reader Fragment",
            EnemyType.ServitorSwarm => "Servitor Swarm",
            EnemyType.BoneKeeper => "Bone Keeper",
            EnemyType.FailureColossus => "Failure Colossus",
            EnemyType.RustWitch => "Rust Witch",
            EnemyType.SentinelPrime => "Sentinel Prime",
            _ => "Undying"
        };

        return index > 0 ? $"{baseName} {(char)('A' + index)}" : baseName;
    }

    private int CalculateDifficulty(List<Enemy> enemies, int playerLevel)
    {
        if (enemies.Count == 0) return 0;

        var totalEnemyPower = enemies.Sum(e => e.MaxHP + e.Attack * 2);
        var playerPower = playerLevel * 30; // Rough estimate

        var ratio = (float)totalEnemyPower / playerPower;

        return ratio switch
        {
            < 0.5f => 1,  // Very Easy
            < 0.8f => 2,  // Easy
            < 1.0f => 3,  // Normal
            < 1.3f => 4,  // Moderate
            < 1.6f => 5,  // Challenging
            < 2.0f => 6,  // Hard
            < 2.5f => 7,  // Very Hard
            < 3.0f => 8,  // Extreme
            < 4.0f => 9,  // Deadly
            _ => 10       // Overwhelming
        };
    }

    private string GenerateEncounterDescription(List<Enemy> enemies, Room room)
    {
        if (enemies.Count == 0) return string.Empty;

        var countDesc = enemies.Count switch
        {
            1 => "A lone",
            2 => "A pair of",
            3 => "A small group of",
            _ => "A patrol of"
        };

        var enemyDesc = enemies.Count == 1
            ? enemies[0].Name
            : $"Undying ({enemies.Count})";

        return $"{countDesc} {enemyDesc} emerges from the shadows, drawn by your presence.";
    }

    #endregion
}
