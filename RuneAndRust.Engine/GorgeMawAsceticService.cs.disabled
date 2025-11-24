using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a GorgeMawAscetic ability operation
/// </summary>
public class GorgeMawAsceticAbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public int EnemiesAffected { get; set; }
    public List<string> StatusEffectsApplied { get; set; } = new();
    public bool TerrainAltered { get; set; }
    public int TerrainSize { get; set; }
}

/// <summary>
/// v0.26.2: Service for GorgeMawAscetic specialization abilities
/// Implements seismic control abilities with unarmed combat and battlefield manipulation
/// </summary>
public class GorgeMawAsceticService
{
    private static readonly ILogger _log = Log.ForContext<GorgeMawAsceticService>();
    private readonly DiceService _diceService;
    private readonly TremorsenseService _tremorsenseService;

    public GorgeMawAsceticService(string connectionString)
    {
        _diceService = new DiceService();
        _tremorsenseService = new TremorsenseService(connectionString);
        _log.Debug("GorgeMawAsceticService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Stone Fist (Active)
    /// Unarmed strike using weighted gauntlets
    /// </summary>
    public GorgeMawAsceticAbilityResult ExecuteStoneFist(PlayerCharacter ascetic, Enemy target, int abilityRank)
    {
        _log.Information("Executing Stone Fist: Ascetic={Ascetic}, Target={Target}, Rank={Rank}",
            ascetic.Name, target.Name, abilityRank);

        try
        {
            int staminaCost = 30;

            if (ascetic.Stamina < staminaCost)
            {
                return new GorgeMawAsceticAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {ascetic.Stamina})"
                };
            }

            ascetic.Stamina -= staminaCost;

            // Check for flying penalty
            var flyingPenalty = _tremorsenseService.ApplyFlyingPenalty(ascetic.CharacterID, target);
            if (flyingPenalty.MissChance > 0 && _diceService.Roll(1, 100) <= (flyingPenalty.MissChance * 100))
            {
                _log.Warning("Stone Fist missed flying target: {TargetName}", target.Name);
                return new GorgeMawAsceticAbilityResult
                {
                    Success = true,
                    Message = $"Stone Fist misses {target.Name} (flying enemy, Tremorsense blind)",
                    DamageDealt = 0
                };
            }

            // Calculate damage based on rank
            int damageDice = abilityRank switch
            {
                3 => 4, // 4d8
                2 => 3, // 3d8
                _ => 2  // 2d8
            };

            int damage = _diceService.Roll(damageDice, 8) + ascetic.GetAttributeModifier("MIGHT");

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            var statusApplied = new List<string>();

            // Rank 3: 10% chance to Stagger
            if (abilityRank >= 3 && _diceService.Roll(1, 100) <= 10)
            {
                var staggerEffect = new StatusEffect
                {
                    TargetID = target.EnemyID,
                    EffectType = "Staggered",
                    DurationRemaining = 1,
                    Category = StatusEffectCategory.ControlDebuff
                };
                target.StatusEffects.Add(staggerEffect);
                statusApplied.Add("Staggered");
                _log.Information("Stone Fist: Applied [Staggered]");
            }

            _log.Information(
                "Stone Fist: Damage={Damage}, Kill={IsKill}",
                damage, isKill);

            return new GorgeMawAsceticAbilityResult
            {
                Success = true,
                Message = $"Stone Fist deals {damage} damage{(statusApplied.Any() ? " + Stagger" : "")}",
                DamageDealt = damage,
                EnemiesAffected = 1,
                StatusEffectsApplied = statusApplied
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Stone Fist");
            return new GorgeMawAsceticAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Concussive Pulse (Active)
    /// Strike ground creating shockwave that pushes enemies back
    /// </summary>
    public GorgeMawAsceticAbilityResult ExecuteConcussivePulse(PlayerCharacter ascetic, List<Enemy> frontRowTargets, int abilityRank)
    {
        _log.Information("Executing Concussive Pulse: Ascetic={Ascetic}, Targets={Count}, Rank={Rank}",
            ascetic.Name, frontRowTargets.Count, abilityRank);

        int staminaCost = 35;

        if (ascetic.Stamina < staminaCost)
        {
            return new GorgeMawAsceticAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {ascetic.Stamina})"
            };
        }

        ascetic.Stamina -= staminaCost;

        int totalDamage = 0;
        var statusApplied = new List<string>();
        int enemiesAffected = 0;

        foreach (var target in frontRowTargets)
        {
            // Only affects ground enemies
            if (target.IsFlying)
            {
                _log.Debug("Concussive Pulse: Skipping flying enemy {TargetName}", target.Name);
                continue;
            }

            // Calculate damage
            int damageDice = abilityRank switch
            {
                3 => 2, // 2d8
                2 => 2, // 2d6
                _ => 1  // 1d6
            };

            int diceSize = abilityRank >= 3 ? 8 : 6;
            int damage = _diceService.Roll(damageDice, diceSize) + ascetic.GetAttributeModifier("MIGHT");

            target.HP = Math.Max(0, target.HP - damage);
            totalDamage += damage;

            // Simulate push to back row (would integrate with TacticalGridService)
            _log.Information("Concussive Pulse: Pushed {TargetName} to back row", target.Name);

            // Rank 3: Stagger on collision
            if (abilityRank >= 3)
            {
                // Simulate collision check
                bool collision = _diceService.Roll(1, 100) <= 60; // 60% collision chance
                if (collision)
                {
                    var staggerEffect = new StatusEffect
                    {
                        TargetID = target.EnemyID,
                        EffectType = "Staggered",
                        DurationRemaining = 1,
                        Category = StatusEffectCategory.ControlDebuff
                    };
                    target.StatusEffects.Add(staggerEffect);
                    if (!statusApplied.Contains("Staggered"))
                        statusApplied.Add("Staggered");
                }
            }

            enemiesAffected++;
        }

        _log.Information(
            "Concussive Pulse: TotalDamage={Damage}, EnemiesAffected={Count}",
            totalDamage, enemiesAffected);

        return new GorgeMawAsceticAbilityResult
        {
            Success = true,
            Message = $"Concussive Pulse pushes {enemiesAffected} enemies back for {totalDamage} total damage!",
            DamageDealt = totalDamage,
            EnemiesAffected = enemiesAffected,
            StatusEffectsApplied = statusApplied
        };
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Sensory Discipline (Passive)
    /// Get bonus dice vs Fear and Disoriented
    /// </summary>
    public int GetSensoryDisciplineBonus(int abilityRank)
    {
        return abilityRank switch
        {
            3 => 4, // +4 dice
            2 => 3, // +3 dice
            _ => 2  // +2 dice
        };
    }

    /// <summary>
    /// Shattering Wave (Active)
    /// Targeted tremor that stuns priority target at range
    /// </summary>
    public GorgeMawAsceticAbilityResult ExecuteShatteringWave(PlayerCharacter ascetic, Enemy target, int abilityRank)
    {
        _log.Information("Executing Shattering Wave: Ascetic={Ascetic}, Target={Target}, Rank={Rank}",
            ascetic.Name, target.Name, abilityRank);

        int staminaCost = 40;

        if (ascetic.Stamina < staminaCost)
        {
            return new GorgeMawAsceticAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {ascetic.Stamina})"
            };
        }

        // Cannot affect flying enemies
        if (target.IsFlying)
        {
            return new GorgeMawAsceticAbilityResult
            {
                Success = false,
                Message = $"Shattering Wave cannot affect flying enemy {target.Name} (Tremorsense limitation)"
            };
        }

        ascetic.Stamina -= staminaCost;

        // Calculate stun chance
        int stunChance = abilityRank switch
        {
            3 => 85,
            2 => 75,
            _ => 60
        };

        int stunDuration = abilityRank >= 3 ? 2 : 1;
        bool stunSuccess = _diceService.Roll(1, 100) <= stunChance;

        var statusApplied = new List<string>();
        int damage = 0;

        if (stunSuccess)
        {
            var stunEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "Stunned",
                DurationRemaining = stunDuration,
                Category = StatusEffectCategory.ControlDebuff
            };
            target.StatusEffects.Add(stunEffect);
            statusApplied.Add("Stunned");
            _log.Information("Shattering Wave: Applied [Stunned] for {Duration} rounds", stunDuration);
        }
        else
        {
            // Guaranteed Staggered as fallback
            var staggerEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "Staggered",
                DurationRemaining = 1,
                Category = StatusEffectCategory.ControlDebuff
            };
            target.StatusEffects.Add(staggerEffect);
            statusApplied.Add("Staggered");
            _log.Information("Shattering Wave: Stun failed, applied [Staggered] instead");
        }

        // Rank 2+: Add damage
        if (abilityRank >= 2)
        {
            int damageDice = abilityRank >= 3 ? 3 : 2;
            damage = _diceService.Roll(damageDice, 6);
            target.HP = Math.Max(0, target.HP - damage);
        }

        return new GorgeMawAsceticAbilityResult
        {
            Success = true,
            Message = $"Shattering Wave {(stunSuccess ? "stuns" : "staggers")} {target.Name}{(damage > 0 ? $" for {damage} damage" : "")}",
            DamageDealt = damage,
            EnemiesAffected = 1,
            StatusEffectsApplied = statusApplied
        };
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Inner Stillness (Passive)
    /// Get mental immunity and bonus dice for allies
    /// </summary>
    public List<string> GetInnerStillnessImmunities(int abilityRank)
    {
        var immunities = new List<string> { "Fear" };

        if (abilityRank >= 2)
            immunities.Add("Disoriented");

        if (abilityRank >= 3)
            immunities.Add("Charmed");

        return immunities;
    }

    public int GetInnerStillnessAuraBonus(int abilityRank)
    {
        return abilityRank >= 3 ? 2 : 1;
    }

    #endregion

    #region Capstone Ability

    /// <summary>
    /// Earthshaker (Active)
    /// Massive earthquake knocking down all ground enemies
    /// </summary>
    public GorgeMawAsceticAbilityResult ExecuteEarthshaker(PlayerCharacter ascetic, List<Enemy> allEnemies, int abilityRank)
    {
        _log.Information("Executing Earthshaker: Ascetic={Ascetic}, Rank={Rank}, TotalEnemies={Count}",
            ascetic.Name, abilityRank, allEnemies.Count);

        int staminaCost = 60;

        if (ascetic.Stamina < staminaCost)
        {
            return new GorgeMawAsceticAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {ascetic.Stamina})"
            };
        }

        ascetic.Stamina -= staminaCost;

        // Filter to ground enemies only
        var groundEnemies = allEnemies.Where(e => !e.IsFlying).ToList();

        int totalDamage = 0;
        var statusApplied = new List<string> { "KnockedDown" };

        foreach (var target in groundEnemies)
        {
            // Calculate damage
            int damageDice = abilityRank switch
            {
                3 => 6,  // 6d10
                2 => 5,  // 5d8
                _ => 4   // 4d8
            };

            int diceSize = abilityRank >= 3 ? 10 : 8;
            int damage = _diceService.Roll(damageDice, diceSize) + ascetic.GetAttributeModifier("MIGHT");

            target.HP = Math.Max(0, target.HP - damage);
            totalDamage += damage;

            // Apply Knocked Down
            int knockedDuration = abilityRank >= 2 ? 2 : 1;
            var knockedEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "KnockedDown",
                DurationRemaining = knockedDuration,
                Category = StatusEffectCategory.ControlDebuff
            };
            target.StatusEffects.Add(knockedEffect);

            // Rank 3: Apply Vulnerable
            if (abilityRank >= 3)
            {
                var vulnerableEffect = new StatusEffect
                {
                    TargetID = target.EnemyID,
                    EffectType = "Vulnerable",
                    DurationRemaining = 1,
                    Magnitude = 25, // +25% damage taken
                    Category = StatusEffectCategory.ControlDebuff
                };
                target.StatusEffects.Add(vulnerableEffect);
                if (!statusApplied.Contains("Vulnerable"))
                    statusApplied.Add("Vulnerable");
            }
        }

        // Create permanent terrain
        int terrainSize = abilityRank switch
        {
            3 => 5, // 5x5
            2 => 4, // 4x4
            _ => 3  // 3x3
        };

        _log.Information(
            "Earthshaker executed: GroundEnemies={Count}, TotalDamage={Damage}, TerrainSize={Size}x{Size}",
            groundEnemies.Count, totalDamage, terrainSize, terrainSize);

        string terrainMessage = abilityRank >= 2 ? " + Cover" : "";

        return new GorgeMawAsceticAbilityResult
        {
            Success = true,
            Message = $"EARTHSHAKER! {groundEnemies.Count} enemies knocked down for {totalDamage} damage! Terrain permanently altered ({terrainSize}x{terrainSize}{terrainMessage})!",
            DamageDealt = totalDamage,
            EnemiesAffected = groundEnemies.Count,
            StatusEffectsApplied = statusApplied,
            TerrainAltered = true,
            TerrainSize = terrainSize
        };
    }

    #endregion
}
