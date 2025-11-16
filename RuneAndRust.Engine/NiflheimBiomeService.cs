using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.30.4: Orchestration service for Niflheim biome.
/// Coordinates generation, hazards, enemies, and frigid cold mechanics.
/// </summary>
public class NiflheimBiomeService
{
    private static readonly ILogger _log = Log.ForContext<NiflheimBiomeService>();

    private readonly NiflheimDataRepository _dataRepository;
    private readonly FrigidColdService _frigidColdService;
    private readonly SlipperyTerrainService _slipperyTerrainService;
    private readonly BrittlenessService _brittlenessService;
    private readonly DiceService _diceService;
    private readonly EnvironmentalObjectService _environmentalObjectService;

    public NiflheimBiomeService(
        NiflheimDataRepository dataRepository,
        FrigidColdService frigidColdService,
        SlipperyTerrainService slipperyTerrainService,
        BrittlenessService brittlenessService,
        DiceService diceService,
        EnvironmentalObjectService environmentalObjectService)
    {
        _dataRepository = dataRepository;
        _frigidColdService = frigidColdService;
        _slipperyTerrainService = slipperyTerrainService;
        _brittlenessService = brittlenessService;
        _diceService = diceService;
        _environmentalObjectService = environmentalObjectService;

        _log.Information("NiflheimBiomeService initialized");
    }

    #region Initialization

    /// <summary>
    /// Check if party is prepared for Niflheim.
    /// Logs warnings if Ice Resistance is insufficient.
    /// </summary>
    public NiflheimPreparednessReport CheckPartyPreparedness(List<PlayerCharacter> party)
    {
        _log.Information("Checking party preparedness for Niflheim ({Count} characters)", party.Count);

        var report = new NiflheimPreparednessReport
        {
            Characters = new List<CharacterPreparedness>()
        };

        foreach (var character in party)
        {
            // Note: GetIceResistancePercent() is a placeholder until full resistance system
            int iceResistance = 0; // TODO: Implement actual resistance lookup

            var charPrep = new CharacterPreparedness
            {
                CharacterName = character.Name,
                IceResistancePercent = iceResistance,
                Finesse = character.Attributes.Finesse,
                IsAdequatelyPrepared = iceResistance >= 50,
                RecommendedIceResistance = 50
            };

            if (iceResistance < 25)
            {
                charPrep.WarningLevel = "Critical";
                charPrep.WarningMessage = $"{character.Name} has CRITICAL lack of Ice Resistance ({iceResistance}%). Expect severe cold damage in [Frigid Cold].";
                _log.Warning("{Character}: CRITICAL Ice Resistance ({Percent}%)", character.Name, iceResistance);
            }
            else if (iceResistance < 50)
            {
                charPrep.WarningLevel = "Warning";
                charPrep.WarningMessage = $"{character.Name} has low Ice Resistance ({iceResistance}%). Consider acquiring cold-resistant equipment.";
                _log.Warning("{Character}: Low Ice Resistance ({Percent}%)", character.Name, iceResistance);
            }
            else
            {
                charPrep.WarningLevel = "Adequate";
                charPrep.WarningMessage = $"{character.Name} is prepared for Niflheim ({iceResistance}% Ice Resistance).";
                _log.Information("{Character}: Adequate Ice Resistance ({Percent}%)", character.Name, iceResistance);
            }

            // Check FINESSE for slippery terrain
            if (character.Attributes.Finesse < 12)
            {
                charPrep.FinesseWarning = $"Low FINESSE ({character.Attributes.Finesse}). High risk of knockdown on [Slippery Terrain] (DC 12).";
                _log.Warning("{Character}: Low FINESSE ({Finesse}) for slippery terrain (DC 12)",
                    character.Name, character.Attributes.Finesse);
            }

            report.Characters.Add(charPrep);
        }

        report.AverageIceResistance = report.Characters.Average(c => c.IceResistancePercent);
        report.PartyIsAdequatelyPrepared = report.AverageIceResistance >= 40;

        _log.Information("Party preparedness check complete: Avg Ice Resistance={Avg}%, Adequate={IsAdequate}",
            report.AverageIceResistance, report.PartyIsAdequatelyPrepared);

        return report;
    }

    #endregion

    #region Enemy Resistance Loading

    /// <summary>
    /// Load Ice/Fire resistances for a Niflheim enemy into BrittlenessService.
    /// Called when enemy is spawned.
    /// </summary>
    public void LoadEnemyResistances(Enemy enemy, NiflheimEnemySpawn spawnData)
    {
        var resistances = _dataRepository.GetEnemyResistances(spawnData.SpawnRulesJson);

        foreach (var kvp in resistances)
        {
            _brittlenessService.SetEnemyResistance(enemy.EnemyID, kvp.Key, kvp.Value);
            _log.Debug("Set {Enemy} resistance: {Type} = {Percent}%",
                enemy.Name, kvp.Key, kvp.Value);
        }

        // Check if enemy is Brittle-eligible (Ice Resistance > 0%)
        bool eligible = _brittlenessService.IsBrittleEligibleNiflheim(enemy);
        if (eligible)
        {
            _log.Information("{Enemy} is [Brittle]-eligible (Ice Resistance > 0%)", enemy.Name);
        }

        // Check for Ice-Walker passive (immunity to slippery terrain)
        var tags = _dataRepository.GetEnemyTags(spawnData.SpawnRulesJson);
        if (tags.Contains("ice_walker"))
        {
            _log.Information("{Enemy} has [Ice-Walker] passive (ignores [Slippery Terrain])", enemy.Name);
        }
    }

    /// <summary>
    /// Create an enemy from spawn data.
    /// </summary>
    public Enemy CreateEnemyFromSpawn(NiflheimEnemySpawn spawnData, int level)
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
            "Boss" => 250,          // Frost-Giant
            "Beast" => 95,          // Ice-Adapted Beast
            "Undying" => 70,        // Frost-Rimed Undying, Cryo-Drone
            "Forlorn" => 50,        // Forlorn Echo
            _ => 60
        };

        return baseHP + (level * 8);
    }

    private static int _nextEnemyId = 6000; // Start Niflheim enemies at 6000
    private int GenerateEnemyId()
    {
        return System.Threading.Interlocked.Increment(ref _nextEnemyId);
    }

    #endregion

    #region Enemy Selection

    /// <summary>
    /// Select a random enemy based on spawn weights.
    /// </summary>
    public NiflheimEnemySpawn SelectWeightedEnemy(List<NiflheimEnemySpawn> eligibleEnemies, Random random)
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

    #region Frigid Cold Integration

    /// <summary>
    /// Apply [Frigid Cold] ambient condition to all combatants at combat start.
    /// Grants Ice Vulnerability (+50% Ice damage).
    /// </summary>
    public void ApplyFrigidColdToCombat(List<PlayerCharacter> characters, List<Enemy> enemies)
    {
        _log.Information("Applying [Frigid Cold] ambient condition to combat ({Characters} characters, {Enemies} enemies)",
            characters.Count, enemies.Count);

        int affectedCount = 0;

        foreach (var character in characters)
        {
            _frigidColdService.ApplyFrigidCold(character);
            affectedCount++;
        }

        foreach (var enemy in enemies)
        {
            _frigidColdService.ApplyFrigidCold(enemy);
            affectedCount++;
        }

        _log.Information("[Frigid Cold] applied to {Count} combatants (all Vulnerable to Ice damage +50%)",
            affectedCount);
    }

    /// <summary>
    /// Process combat end for Niflheim.
    /// Applies Psychic Stress from environmental anxiety.
    /// </summary>
    public List<ColdStressResult> ProcessCombatEndStress(List<PlayerCharacter> characters)
    {
        _log.Information("Processing [Frigid Cold] environmental stress for {Count} characters", characters.Count);

        var results = new List<ColdStressResult>();

        foreach (var character in characters)
        {
            var result = new ColdStressResult
            {
                CharacterName = character.Name,
                PreviousStress = character.PsychicStress
            };

            // Apply +5 Psychic Stress (environmental anxiety from extreme cold)
            const int stressAmount = 5;
            character.PsychicStress += stressAmount;

            result.StressGained = stressAmount;
            result.CurrentStress = character.PsychicStress;
            result.Message = $"{character.Name} gains +{stressAmount} Psychic Stress from [Frigid Cold] exposure (anxiety from extreme cold)";

            results.Add(result);

            _log.Information("{Character} gains +{Stress} Psychic Stress from [Frigid Cold] ({Previous} → {Current})",
                character.Name, stressAmount, result.PreviousStress, result.CurrentStress);
        }

        return results;
    }

    #endregion

    #region Slippery Terrain Integration

    /// <summary>
    /// Process movement through slippery terrain.
    /// FINESSE DC 12 check or [Knocked Down] + 1d4 Physical damage.
    /// </summary>
    public SlipperyTerrainResult ProcessSlipperyMovement(PlayerCharacter character)
    {
        _log.Information("{Character} attempting movement on [Slippery Terrain]", character.Name);

        var result = new SlipperyTerrainResult
        {
            CharacterName = character.Name,
            FinesseValue = character.Attributes.Finesse
        };

        // Make FINESSE check (DC 12)
        var rollResult = _diceService.Roll(character.Attributes.Finesse);
        result.SuccessesRolled = rollResult.Successes;
        result.CheckPassed = rollResult.Successes >= 12;

        if (result.CheckPassed)
        {
            // Success: Movement succeeds
            result.WasKnockedDown = false;
            result.DamageDealt = 0;
            result.Message = $"✓ {character.Name} navigates [Slippery Terrain] safely (FINESSE check passed)";

            _log.Information("{Character} passed [Slippery Terrain] check: {Successes} successes (DC 12)",
                character.Name, rollResult.Successes);
        }
        else
        {
            // Failure: [Knocked Down] + fall damage
            result.WasKnockedDown = true;

            // Roll 1d4 fall damage
            int fallDamage = _diceService.RollDamage(1, 4);
            character.HP = Math.Max(0, character.HP - fallDamage);

            result.DamageDealt = fallDamage;
            result.Message = $"✗ {character.Name} slips on [Slippery Terrain] ({rollResult.Successes} successes)\n" +
                           $"   ❄️ You lose your footing on the ice!\n" +
                           $"   [Knocked Down] + Fall Damage: 1d4 = {fallDamage}\n" +
                           $"   HP: {character.HP}/{character.MaxHP}";

            _log.Information("{Character} failed [Slippery Terrain] check: {Successes} successes, [Knocked Down] + {Damage} fall damage (HP: {HP}/{MaxHP})",
                character.Name, rollResult.Successes, fallDamage, character.HP, character.MaxHP);

            // TODO: Apply [Knocked Down] status effect when status system is implemented
        }

        return result;
    }

    #endregion

    #region Brittleness Integration

    /// <summary>
    /// Apply Ice damage to enemy and check for [Brittle] application.
    /// [Brittle] = Vulnerable to Physical damage (+50%).
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

        // Apply Ice damage
        enemy.HP = Math.Max(0, enemy.HP - iceDamage);
        result.EnemyCurrentHP = enemy.HP;

        // Check if enemy is eligible for [Brittle]
        bool eligible = _brittlenessService.IsBrittleEligibleNiflheim(enemy);
        result.WasEligibleForBrittle = eligible;

        if (eligible)
        {
            // Apply [Brittle] debuff
            _brittlenessService.ApplyBrittle(enemy);
            result.BrittleApplied = true;
            result.Message = $"❄️ {enemy.Name} takes {iceDamage} Ice damage\n" +
                           $"   [Brittle] applied! Vulnerable to Physical damage (+50%) for 1 turn\n" +
                           $"   HP: {enemy.HP}/{enemy.MaxHP}";

            _log.Information("{Enemy} hit with Ice damage: [Brittle] applied (Physical Vuln +50% for 1 turn)",
                enemy.Name);
        }
        else
        {
            result.BrittleApplied = false;
            result.Message = $"❄️ {enemy.Name} takes {iceDamage} Ice damage\n" +
                           $"   HP: {enemy.HP}/{enemy.MaxHP}";

            _log.Information("{Enemy} hit with Ice damage: Not eligible for [Brittle] (Ice Resistance <= 0%)",
                enemy.Name);
        }

        return result;
    }

    /// <summary>
    /// Apply Physical damage to [Brittle] enemy.
    /// Physical damage is amplified by +50% if enemy has [Brittle].
    /// </summary>
    public PhysicalDamageResult ApplyPhysicalDamageToEnemy(Enemy enemy, int baseDamage, string sourceName)
    {
        _log.Information("Applying {Damage} Physical damage to {Enemy} from {Source}",
            baseDamage, enemy.Name, sourceName);

        var result = new PhysicalDamageResult
        {
            EnemyName = enemy.Name,
            BaseDamage = baseDamage
        };

        // Check if enemy has [Brittle]
        bool hasBrittle = _brittlenessService.HasBrittle(enemy);
        result.TargetWasBrittle = hasBrittle;

        int finalDamage = baseDamage;

        if (hasBrittle)
        {
            // Amplify damage by +50%
            finalDamage = (int)(baseDamage * 1.5);
            result.FinalDamage = finalDamage;
            result.DamageAmplified = finalDamage - baseDamage;

            result.Message = $"⚔️ {enemy.Name} takes {baseDamage} Physical damage\n" +
                           $"   [Brittle] vulnerability: +{result.DamageAmplified} bonus damage (+50%)\n" +
                           $"   Total: {finalDamage} damage";

            _log.Information("{Enemy} hit with Physical damage while [Brittle]: {Base} → {Final} (+50%)",
                enemy.Name, baseDamage, finalDamage);
        }
        else
        {
            result.FinalDamage = baseDamage;
            result.DamageAmplified = 0;

            result.Message = $"⚔️ {enemy.Name} takes {baseDamage} Physical damage";

            _log.Debug("{Enemy} hit with Physical damage (not [Brittle]): {Damage}",
                enemy.Name, baseDamage);
        }

        // Apply damage
        enemy.HP = Math.Max(0, enemy.HP - finalDamage);
        result.EnemyCurrentHP = enemy.HP;
        result.Message += $"\n   HP: {enemy.HP}/{enemy.MaxHP}";

        return result;
    }

    #endregion

    #region Critical Hit Slow

    /// <summary>
    /// Process critical hit in [Frigid Cold].
    /// Critical hits apply [Slowed] status for 2 turns.
    /// </summary>
    public CriticalHitResult ProcessCriticalHitSlow(PlayerCharacter target, string attackerName)
    {
        _log.Information("{Attacker} scored critical hit on {Target} in [Frigid Cold]",
            attackerName, target.Name);

        var result = new CriticalHitResult
        {
            TargetName = target.Name,
            AttackerName = attackerName
        };

        // Apply [Slowed] status for 2 turns
        // TODO: Implement when status effect system is complete
        result.SlowedApplied = true;
        result.SlowedDuration = 2;
        result.Message = $"💥 Critical hit in [Frigid Cold]!\n" +
                       $"   {target.Name} is [Slowed] for 2 turns (-50% Movement Speed)\n" +
                       $"   The extreme cold amplifies the shock of the blow.";

        _log.Information("{Target} is [Slowed] for 2 turns from critical hit in [Frigid Cold]",
            target.Name);

        return result;
    }

    #endregion
}

#region Result Data Transfer Objects

public class NiflheimPreparednessReport
{
    public List<CharacterPreparedness> Characters { get; set; } = new();
    public double AverageIceResistance { get; set; }
    public bool PartyIsAdequatelyPrepared { get; set; }
}

public class CharacterPreparedness
{
    public string CharacterName { get; set; } = string.Empty;
    public int IceResistancePercent { get; set; }
    public int Finesse { get; set; }
    public bool IsAdequatelyPrepared { get; set; }
    public int RecommendedIceResistance { get; set; }
    public string WarningLevel { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
    public string? FinesseWarning { get; set; }
}

public class ColdStressResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int PreviousStress { get; set; }
    public int StressGained { get; set; }
    public int CurrentStress { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SlipperyTerrainResult
{
    public string CharacterName { get; set; } = string.Empty;
    public int FinesseValue { get; set; }
    public int SuccessesRolled { get; set; }
    public bool CheckPassed { get; set; }
    public bool WasKnockedDown { get; set; }
    public int DamageDealt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BrittlenessResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int IceDamageDealt { get; set; }
    public int EnemyCurrentHP { get; set; }
    public bool WasEligibleForBrittle { get; set; }
    public bool BrittleApplied { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PhysicalDamageResult
{
    public string EnemyName { get; set; } = string.Empty;
    public int BaseDamage { get; set; }
    public int FinalDamage { get; set; }
    public int DamageAmplified { get; set; }
    public bool TargetWasBrittle { get; set; }
    public int EnemyCurrentHP { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CriticalHitResult
{
    public string TargetName { get; set; } = string.Empty;
    public string AttackerName { get; set; } = string.Empty;
    public bool SlowedApplied { get; set; }
    public int SlowedDuration { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion
