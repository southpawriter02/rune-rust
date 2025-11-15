using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.29.4: Orchestration service for Muspelheim biome.
/// Coordinates generation, hazards, enemies, and heat mechanics.
/// </summary>
public class MuspelheimBiomeService
{
    private static readonly ILogger _log = Log.ForContext<MuspelheimBiomeService>();

    private readonly MuspelheimDataRepository _dataRepository;
    private readonly IntenseHeatService _heatService;
    private readonly BrittlenessService _brittlenessService;
    private readonly DiceService _diceService;

    public MuspelheimBiomeService(
        MuspelheimDataRepository dataRepository,
        IntenseHeatService heatService,
        BrittlenessService brittlenessService,
        DiceService diceService)
    {
        _dataRepository = dataRepository;
        _heatService = heatService;
        _brittlenessService = brittlenessService;
        _diceService = diceService;

        _log.Information("MuspelheimBiomeService initialized");
    }

    #region Initialization

    /// <summary>
    /// Check if party is prepared for Muspelheim.
    /// Logs warnings if Fire Resistance is insufficient.
    /// </summary>
    public MuspelheimPreparednessReport CheckPartyPreparedness(List<PlayerCharacter> party)
    {
        _log.Information("Checking party preparedness for Muspelheim ({Count} characters)", party.Count);

        var report = new MuspelheimPreparedness Report
        {
            Characters = new List<CharacterPreparedness>()
        };

        foreach (var character in party)
        {
            // Note: GetFireResistancePercent() is a placeholder until full resistance system
            int fireResistance = 0; // TODO: Implement actual resistance lookup

            var charPrep = new CharacterPreparedness
            {
                CharacterName = character.Name,
                FireResistancePercent = fireResistance,
                Sturdiness = character.Attributes.Sturdiness,
                IsAdequatelyPrepared = fireResistance >= 50,
                RecommendedFireResistance = 50
            };

            if (fireResistance < 25)
            {
                charPrep.WarningLevel = "Critical";
                charPrep.WarningMessage = $"{character.Name} has CRITICAL lack of Fire Resistance ({fireResistance}%). Expect frequent deaths from [Intense Heat].";
                _log.Warning("{Character}: CRITICAL Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }
            else if (fireResistance < 50)
            {
                charPrep.WarningLevel = "Warning";
                charPrep.WarningMessage = $"{character.Name} has low Fire Resistance ({fireResistance}%). Consider acquiring heat-resistant equipment.";
                _log.Warning("{Character}: Low Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }
            else
            {
                charPrep.WarningLevel = "Adequate";
                charPrep.WarningMessage = $"{character.Name} is prepared for Muspelheim ({fireResistance}% Fire Resistance).";
                _log.Information("{Character}: Adequate Fire Resistance ({Percent}%)", character.Name, fireResistance);
            }

            report.Characters.Add(charPrep);
        }

        report.AverageFireResistance = report.Characters.Average(c => c.FireResistancePercent);
        report.PartyIsAdequatelyPrepared = report.AverageFireResistance >= 40;

        _log.Information("Party preparedness check complete: Avg Fire Resistance={Avg}%, Adequate={IsAdequate}",
            report.AverageFireResistance, report.PartyIsAdequatelyPrepared);

        return report;
    }

    #endregion

    #region Enemy Resistance Loading

    /// <summary>
    /// Load Fire/Ice resistances for a Muspelheim enemy into BrittlenessService.
    /// Called when enemy is spawned.
    /// </summary>
    public void LoadEnemyResistances(Enemy enemy, MuspelheimEnemySpawn spawnData)
    {
        var resistances = _dataRepository.GetEnemyResistances(spawnData.SpawnRulesJson);

        foreach (var kvp in resistances)
        {
            _brittlenessService.SetEnemyResistance(enemy.EnemyID, kvp.Key, kvp.Value);
            _log.Debug("Set {Enemy} resistance: {Type} = {Percent}%",
                enemy.Name, kvp.Key, kvp.Value);
        }

        // Check if enemy is Brittle-eligible
        bool eligible = _brittlenessService.IsBrittleEligible(enemy);
        if (eligible)
        {
            _log.Information("{Enemy} is [Brittle]-eligible (Fire Resistance > 0%)", enemy.Name);
        }
    }

    /// <summary>
    /// Create an enemy from spawn data.
    /// </summary>
    public Enemy CreateEnemyFromSpawn(MuspelheimEnemySpawn spawnData, int level)
    {
        // Basic enemy creation (simplified)
        var enemy = new Enemy
        {
            EnemyID = GenerateEnemyId(),
            Name = spawnData.EnemyName,
            Level = level,
            HP = CalculateEnemyHP(spawnData.EnemyType, level),
            MaxHP = CalculateEnemyHP(spawnData.EnemyType, level),
            StatusEffects = new List<StatusEffect>()
        };

        // Load resistances
        LoadEnemyResistances(enemy, spawnData);

        _log.Information("Created enemy: {Name} (Level {Level}, HP {HP})",
            enemy.Name, enemy.Level, enemy.HP);

        return enemy;
    }

    private int CalculateEnemyHP(string enemyType, int level)
    {
        // Simplified HP calculation
        int baseHP = enemyType switch
        {
            "Boss" => 200,
            "Construct" => 80,
            "Humanoid" => 60,
            "Undying" => 70,
            _ => 50
        };

        return baseHP + (level * 10);
    }

    private static int _nextEnemyId = 5000; // Start Muspelheim enemies at 5000
    private int GenerateEnemyId()
    {
        return System.Threading.Interlocked.Increment(ref _nextEnemyId);
    }

    #endregion

    #region Enemy Selection

    /// <summary>
    /// Select a random enemy based on spawn weights.
    /// </summary>
    public MuspelheimEnemySpawn SelectWeightedEnemy(List<MuspelheimEnemySpawn> eligibleEnemies, Random random)
    {
        int totalWeight = eligibleEnemies.Sum(e => e.SpawnWeight);
        int roll = random.Next(totalWeight);

        int cumulativeWeight = 0;
        foreach (var enemy in eligibleEnemies)
        {
            cumulativeWeight += enemy.SpawnWeight;
            if (roll < cumulativeWeight)
            {
                _log.Debug("Selected enemy: {Name} (weight {Weight}/{Total}, roll {Roll})",
                    enemy.EnemyName, enemy.SpawnWeight, totalWeight, roll);
                return enemy;
            }
        }

        // Fallback (should not happen)
        return eligibleEnemies.Last();
    }

    #endregion

    #region Heat Damage Integration

    /// <summary>
    /// Process [Intense Heat] damage for all characters at end of turn.
    /// Integrates with IntenseHeatService.
    /// </summary>
    public List<HeatDamageResult> ProcessEndOfTurnHeat(List<PlayerCharacter> characters)
    {
        _log.Information("Processing [Intense Heat] for {Count} characters", characters.Count);

        var results = new List<HeatDamageResult>();

        foreach (var character in characters)
        {
            var result = ProcessHeatForCharacter(character);
            results.Add(result);
        }

        int totalDamage = results.Sum(r => r.DamageDealt);
        int charactersAffected = results.Count(r => r.DamageDealt > 0);

        _log.Information("[Intense Heat] processing complete: {Affected}/{Total} characters took damage, total {TotalDamage} damage",
            charactersAffected, characters.Count, totalDamage);

        return results;
    }

    private HeatDamageResult ProcessHeatForCharacter(PlayerCharacter character)
    {
        var result = new HeatDamageResult
        {
            CharacterName = character.Name,
            SturdynessValue = character.Attributes.Sturdiness
        };

        // Make STURDINESS check (DC 12)
        var rollResult = _diceService.Roll(character.Attributes.Sturdiness);
        result.SuccessesRolled = rollResult.Successes;
        result.CheckPassed = rollResult.Successes >= 12;

        if (result.CheckPassed)
        {
            // Success: No damage
            result.DamageDealt = 0;
            result.Message = $"✓ {character.Name} resists [Intense Heat] (STURDINESS check passed)";

            _log.Information("{Character} passed [Intense Heat] check: {Successes} successes (DC 12)",
                character.Name, rollResult.Successes);
        }
        else
        {
            // Failure: Roll 2d6 Fire damage
            int rawDamage = _diceService.RollDamage(2, 6);

            // Apply Fire Resistance (placeholder - actual resistance system not yet implemented)
            int finalDamage = rawDamage; // TODO: Apply Fire Resistance

            // Apply damage
            character.HP = Math.Max(0, character.HP - finalDamage);

            result.DamageDealt = finalDamage;
            result.RawDamageRolled = rawDamage;
            result.Message = $"✗ {character.Name} fails [Intense Heat] check ({rollResult.Successes} successes)\n" +
                           $"   🔥 The searing heat overwhelms your defenses!\n" +
                           $"   Damage: 2d6 = {rawDamage}\n" +
                           $"   HP: {character.HP}/{character.MaxHP}";

            _log.Information("{Character} failed [Intense Heat] check: {Successes} successes, took {Damage} Fire damage (HP: {HP}/{MaxHP})",
                character.Name, rollResult.Successes, finalDamage, character.HP, character.MaxHP);

            // Check for death
            if (character.HP <= 0)
            {
                result.CharacterDied = true;
                result.Message += $"\n💀 {character.Name} has succumbed to the [Intense Heat]!";
                _log.Warning("{Character} died from [Intense Heat]", character.Name);
            }
        }

        return result;
    }

    #endregion

    #region Brittleness Integration

    /// <summary>
    /// Apply Ice damage to enemy and check for [Brittle] application.
    /// </summary>
    public BrittlenessResult ApplyIceDamageToEnemy(Enemy enemy, int iceDamage, string sourceName)
    {
        _log.Information("Applying {Damage} Ice damage to {Enemy} from {Source}",
            iceDamage, enemy.Name, sourceName);

        var result = new BrittlenessResult
        {
            EnemyName = enemy.Name,
            IceDamageDealt = iceDamage
        };

        // Apply damage to enemy
        int damageAfterResistance = ApplyIceResistance(enemy, iceDamage);
        enemy.HP = Math.Max(0, enemy.HP - damageAfterResistance);

        result.FinalDamageDealt = damageAfterResistance;

        _log.Information("{Enemy} took {FinalDamage} Ice damage ({RawDamage} before resistance, HP: {HP}/{MaxHP})",
            enemy.Name, damageAfterResistance, iceDamage, enemy.HP, enemy.MaxHP);

        // Try to apply [Brittle]
        _brittlenessService.TryApplyBrittle(enemy, damageAfterResistance);

        // Check if [Brittle] was applied
        bool hasBrittle = enemy.StatusEffects.Any(s =>
            s.EffectType.Equals("Brittle", StringComparison.OrdinalIgnoreCase) &&
            s.DurationRemaining > 0);

        result.BrittleApplied = hasBrittle;

        if (hasBrittle)
        {
            result.Message = _brittlenessService.GetBrittleApplicationMessage(enemy);
            _log.Information("{Enemy} is now [Brittle] for 2 turns", enemy.Name);
        }
        else
        {
            result.Message = $"{enemy.Name} is not eligible for [Brittle] (no Fire Resistance)";
        }

        return result;
    }

    /// <summary>
    /// Apply Physical damage to enemy, with [Brittle] bonus if applicable.
    /// </summary>
    public PhysicalDamageResult ApplyPhysicalDamageToEnemy(Enemy enemy, int physicalDamage, string sourceName)
    {
        _log.Information("Applying {Damage} Physical damage to {Enemy} from {Source}",
            physicalDamage, enemy.Name, sourceName);

        var result = new PhysicalDamageResult
        {
            EnemyName = enemy.Name,
            BaseDamage = physicalDamage
        };

        // Check for [Brittle] bonus
        int finalDamage = _brittlenessService.ApplyBrittleBonus(enemy, physicalDamage);
        result.FinalDamage = finalDamage;
        result.BrittleBonusApplied = finalDamage > physicalDamage;

        if (result.BrittleBonusApplied)
        {
            int bonusDamage = finalDamage - physicalDamage;
            result.Message = _brittlenessService.GetBrittleBonusMessage(enemy, bonusDamage);
            _log.Information("[Brittle] bonus: {Base} Physical damage → {Final} (+{Bonus})",
                physicalDamage, finalDamage, bonusDamage);
        }

        // Apply damage
        enemy.HP = Math.Max(0, enemy.HP - finalDamage);

        _log.Information("{Enemy} took {FinalDamage} Physical damage (HP: {HP}/{MaxHP})",
            enemy.Name, finalDamage, enemy.HP, enemy.MaxHP);

        result.EnemyDefeated = enemy.HP <= 0;

        return result;
    }

    private int ApplyIceResistance(Enemy enemy, int iceDamage)
    {
        int iceResistance = _brittlenessService.GetEnemyResistance(enemy.EnemyID, "Ice");

        if (iceResistance == 0)
        {
            return iceDamage;
        }

        // Negative resistance = vulnerability (takes MORE damage)
        if (iceResistance < 0)
        {
            int vulnerabilityPercent = Math.Abs(iceResistance);
            int bonusDamage = iceDamage * vulnerabilityPercent / 100;
            int totalDamage = iceDamage + bonusDamage;

            _log.Information("Ice Vulnerability {Percent}%: {RawDamage} → {TotalDamage} (+{Bonus})",
                vulnerabilityPercent, iceDamage, totalDamage, bonusDamage);

            return totalDamage;
        }

        // Positive resistance = reduces damage
        int reducedDamage = iceDamage - (iceDamage * iceResistance / 100);
        _log.Information("Ice Resistance {Percent}%: {RawDamage} → {ReducedDamage}",
            iceResistance, iceDamage, reducedDamage);

        return Math.Max(0, reducedDamage);
    }

    #endregion

    #region Data Access Helpers

    /// <summary>
    /// Get all room templates for Muspelheim
    /// </summary>
    public List<MuspelheimRoomTemplate> GetRoomTemplates()
    {
        return _dataRepository.GetRoomTemplates();
    }

    /// <summary>
    /// Get enemy spawns for a specific level range
    /// </summary>
    public List<MuspelheimEnemySpawn> GetEnemySpawnsForLevel(int level)
    {
        return _dataRepository.GetEnemySpawns(minLevel: level - 1, maxLevel: level + 1);
    }

    /// <summary>
    /// Get environmental hazards for a hazard density level
    /// </summary>
    public List<MuspelheimHazard> GetHazardsForDensity(string hazardDensity)
    {
        return _dataRepository.GetEnvironmentalHazards(hazardDensity);
    }

    /// <summary>
    /// Get biome metadata
    /// </summary>
    public MuspelheimBiomeMetadata GetBiomeMetadata()
    {
        return _dataRepository.GetBiomeMetadata();
    }

    #endregion
}

#region Result Data Transfer Objects

public class MuspelheimPreparednessReport
{
    public List<CharacterPreparedness> Characters { get; set; } = new();
    public double AverageFireResistance { get; set; }
    public bool PartyIsAdequatelyPrepared { get; set; }
}

public class CharacterPreparedness
{
    public string CharacterName { get; set; } = string.Empty;
    public int FireResistancePercent { get; set; }
    public int Sturdiness { get; set; }
    public bool IsAdequatelyPrepared { get; set; }
    public int RecommendedFireResistance { get; set; }
    public string WarningLevel { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}

public class HeatDamageResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int SturdynessValue { get; set; }
    public int SuccessesRolled { get; set; }
    public bool CheckPassed { get; set; }
    public int RawDamageRolled { get; set; }
    public int DamageDealt { get; set; }
    public bool CharacterDied { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BrittlenessResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int IceDamageDealt { get; set; }
    public int FinalDamageDealt { get; set; }
    public bool BrittleApplied { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PhysicalDamageResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int BaseDamage { get; set; }
    public int FinalDamage { get; set; }
    public bool BrittleBonusApplied { get; set; }
    public bool EnemyDefeated { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
