using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Hlekkr-master ability operation
/// </summary>
public class HlekkmasterAbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public int TargetsHit { get; set; } = 1;
    public bool IsKill { get; set; }
    public int PsychicStressGained { get; set; }
    public int CorruptionPurged { get; set; }
    public Row? TargetMovedTo { get; set; }
    public List<string> StatusEffectsApplied { get; set; } = new();

    public static HlekkmasterAbilityResult Failure(string reason)
    {
        return new HlekkmasterAbilityResult
        {
            Success = false,
            Message = reason
        };
    }
}

/// <summary>
/// v0.25.2: Service for Hlekkr-master (Chain-Master) specialization abilities
/// Implements all 9 Hlekkr-master abilities with forced movement and control mechanics
/// </summary>
public class HlekkmasterService
{
    private static readonly ILogger _log = Log.ForContext<HlekkmasterService>();
    private readonly ForcedMovementService _forcedMovementService;
    private readonly DiceService _diceService;
    private readonly AdvancedStatusEffectService _statusEffectService;

    public HlekkmasterService(string connectionString)
    {
        _forcedMovementService = new ForcedMovementService();
        _diceService = new DiceService();
        // TODO: Pass connection string to AdvancedStatusEffectService if needed
        // For now, we'll create a simplified version
        _log.Debug("HlekkmasterService initialized");
    }

    /// <summary>
    /// Constructor for testing with dependency injection
    /// </summary>
    public HlekkmasterService(
        ForcedMovementService forcedMovementService,
        DiceService diceService,
        AdvancedStatusEffectService statusEffectService)
    {
        _forcedMovementService = forcedMovementService;
        _diceService = diceService;
        _statusEffectService = statusEffectService;
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Pragmatic Preparation I (Passive)
    /// Returns control duration extension bonus
    /// </summary>
    public int GetPragmaticPreparationControlBonus(int abilityRank)
    {
        return abilityRank >= 3 ? 2 : 1;
    }

    /// <summary>
    /// Pragmatic Preparation I (Passive)
    /// Returns trap check bonus dice
    /// </summary>
    public int GetPragmaticPreparationTrapBonus(int abilityRank)
    {
        return abilityRank switch
        {
            3 => 3, // +3d10
            2 => 2, // +2d10
            _ => 1  // +1d10
        };
    }

    /// <summary>
    /// Netting Shot (Active)
    /// Cheap spam control - apply [Rooted] with low damage
    /// </summary>
    public HlekkmasterAbilityResult ExecuteNettingShot(
        PlayerCharacter attacker,
        List<Enemy> targets,
        int abilityRank,
        int pragmaticPrepRank = 0)
    {
        _log.Information("Executing Netting Shot: Attacker={Attacker}, Targets={TargetCount}, Rank={Rank}",
            attacker.Name, targets.Count, abilityRank);

        try
        {
            // Calculate resource cost
            int staminaCost = abilityRank >= 3 ? 15 : 20;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Rank 2+: Can target 2 enemies
            int maxTargets = abilityRank >= 2 ? 2 : 1;
            var actualTargets = targets.Take(maxTargets).ToList();

            int totalDamage = 0;
            var statusApplied = new List<string>();

            foreach (var target in actualTargets)
            {
                // Calculate damage (minimal)
                int damage = _diceService.Roll(1, 6); // 1d6
                damage += attacker.GetAttributeModifier("FINESSE");

                // Apply damage
                target.HP = Math.Max(0, target.HP - damage);
                totalDamage += damage;

                // Calculate [Rooted] duration
                int rootedDuration = abilityRank >= 2 ? 3 : 2;

                // Add Pragmatic Preparation bonus
                if (pragmaticPrepRank > 0)
                {
                    rootedDuration += GetPragmaticPreparationControlBonus(pragmaticPrepRank);
                }

                // Apply [Rooted] status
                ApplyControlEffect(target, "Rooted", rootedDuration);
                statusApplied.Add("Rooted");

                // Rank 3: vs highly corrupted (60+), also apply [Slowed]
                if (abilityRank >= 3 && target.Corruption >= 60)
                {
                    ApplyControlEffect(target, "Slowed", 2);
                    statusApplied.Add("Slowed");
                }
            }

            _log.Information(
                "Netting Shot complete: Targets={Count}, TotalDamage={Damage}, StatusEffects={Effects}",
                actualTargets.Count, totalDamage, string.Join(", ", statusApplied));

            return new HlekkmasterAbilityResult
            {
                Success = true,
                Message = $"Netting Shot hit {actualTargets.Count} target(s), dealt {totalDamage} damage",
                DamageDealt = totalDamage,
                TargetsHit = actualTargets.Count,
                StatusEffectsApplied = statusApplied
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Netting Shot");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Grappling Hook Toss (Active)
    /// SIGNATURE ABILITY - Pull enemy from Back Row to Front Row
    /// </summary>
    public HlekkmasterAbilityResult ExecuteGrapplingHookToss(
        PlayerCharacter attacker,
        Enemy target,
        int abilityRank,
        int snagTheGlitchRank = 0)
    {
        _log.Information("Executing Grappling Hook Toss: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 30;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Verify target is in Back Row
            if (!_forcedMovementService.CanPullTarget(target))
            {
                return HlekkmasterAbilityResult.Failure("Target must be in Back Row to pull");
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int numDice = abilityRank >= 2 ? 3 : 2;
            int damage = _diceService.Roll(numDice, 8); // 2d8 or 3d8
            damage += attacker.GetAttributeModifier("FINESSE");

            // Apply corruption bonus from Snag the Glitch
            if (snagTheGlitchRank > 0)
            {
                int bonusDamage = CalculateStagTheGlitchDamageBonus(target.Corruption, snagTheGlitchRank);
                damage += bonusDamage;
            }

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Attempt forced movement (Pull)
            var pullResult = _forcedMovementService.AttemptForcedMovement(
                target,
                ForcedMovementService.MovementDirection.Pull,
                "Grappling Hook Toss");

            if (!pullResult.Success)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Pull failed: {pullResult.Message}. Dealt {damage} damage but target not moved.");
            }

            // Apply [Disoriented]
            ApplyControlEffect(target, "Disoriented", 1);

            // Rank 3: Generate Focus vs corrupted enemies
            if (abilityRank >= 3 && target.Corruption > 0)
            {
                // TODO: Implement Focus resource when available
                _log.Information("Grappling Hook Toss: Would generate 10 Focus (not yet implemented)");
            }

            _log.Information(
                "Grappling Hook Toss complete: Damage={Damage}, Pull={Success}, NewRow={Row}",
                damage, pullResult.Success, pullResult.NewRow);

            return new HlekkmasterAbilityResult
            {
                Success = true,
                Message = $"Pulled target from Back Row, dealt {damage} damage",
                DamageDealt = damage,
                IsKill = isKill,
                TargetMovedTo = pullResult.NewRow,
                StatusEffectsApplied = new List<string> { "Disoriented" }
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Grappling Hook Toss");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Snag the Glitch (Passive)
    /// Returns control success bonus based on corruption
    /// </summary>
    public int GetSnagTheGlitchSuccessBonus(int corruption, int abilityRank)
    {
        int baseBonus = _forcedMovementService.CalculatePullSuccessBonus(corruption);

        // Rank 2: Doubled bonuses
        if (abilityRank >= 2)
        {
            return baseBonus * 2;
        }

        return baseBonus;
    }

    /// <summary>
    /// Snag the Glitch (Passive)
    /// Calculate damage bonus vs corrupted enemies
    /// </summary>
    private int CalculateStagTheGlitchDamageBonus(int corruption, int abilityRank)
    {
        if (corruption == 0) return 0;

        if (abilityRank >= 3)
        {
            return _diceService.Roll(3, 10); // +3d10
        }
        else if (abilityRank >= 2)
        {
            return _diceService.Roll(1, 10); // +1d10
        }

        return 0;
    }

    /// <summary>
    /// Unyielding Grip (Active)
    /// Apply [Seized] to machines (complete lockdown)
    /// </summary>
    public HlekkmasterAbilityResult ExecuteUnyieldingGrip(
        PlayerCharacter attacker,
        Enemy target,
        int abilityRank,
        int snagTheGlitchRank = 0)
    {
        _log.Information("Executing Unyielding Grip: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 25;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int damage = _diceService.Roll(2, 8); // 2d8
            damage += attacker.GetAttributeModifier("FINESSE");

            // Apply corruption bonus from Snag the Glitch
            if (snagTheGlitchRank > 0)
            {
                damage += CalculateStagTheGlitchDamageBonus(target.Corruption, snagTheGlitchRank);
            }

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Determine if target is mechanical (TODO: implement proper enemy type system)
            bool isMechanical = IsMechanicalEnemy(target);

            // Calculate [Seized] success chance
            int successChance = 0;
            int duration = abilityRank >= 3 ? 3 : (abilityRank >= 2 ? 2 : 1);

            if (isMechanical)
            {
                successChance = abilityRank >= 2 ? 80 : 60;
            }
            else if (abilityRank >= 3)
            {
                successChance = 40; // Rank 3 works on all enemies at reduced rate
            }

            if (successChance == 0)
            {
                _log.Information("Unyielding Grip: Target not mechanical, ability has no effect");
                return new HlekkmasterAbilityResult
                {
                    Success = true,
                    Message = $"Dealt {damage} damage, but target is not mechanical ([Seized] requires Rank 3 for non-mechanical)",
                    DamageDealt = damage,
                    IsKill = isKill
                };
            }

            // Apply Snag the Glitch bonus if vs corrupted
            if (snagTheGlitchRank > 0)
            {
                int corruptionBonus = GetSnagTheGlitchSuccessBonus(target.Corruption, snagTheGlitchRank);
                successChance = Math.Min(100, successChance + corruptionBonus);
            }

            // Roll for success
            var roll = new Random().Next(1, 101);

            if (roll <= successChance)
            {
                ApplyControlEffect(target, "Seized", duration);

                // Rank 3: Apply crushing damage over time
                if (abilityRank >= 3)
                {
                    // TODO: Implement DoT system when available
                    _log.Information("Unyielding Grip: Would apply 1d6 crushing damage/turn (not yet implemented)");
                }

                _log.Information(
                    "Unyielding Grip SUCCESS: Damage={Damage}, Seized={Duration}, Roll={Roll}/{Chance}",
                    damage, duration, roll, successChance);

                return new HlekkmasterAbilityResult
                {
                    Success = true,
                    Message = $"Target seized for {duration} turns, dealt {damage} damage",
                    DamageDealt = damage,
                    IsKill = isKill,
                    StatusEffectsApplied = new List<string> { "Seized" }
                };
            }
            else
            {
                _log.Information(
                    "Unyielding Grip FAILED: Roll={Roll}/{Chance}",
                    roll, successChance);

                return new HlekkmasterAbilityResult
                {
                    Success = true,
                    Message = $"Dealt {damage} damage, but [Seized] failed (rolled {roll}, needed {successChance} or less)",
                    DamageDealt = damage,
                    IsKill = isKill
                };
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Unyielding Grip");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Punish the Helpless (Passive)
    /// Calculate damage bonus vs controlled enemies
    /// </summary>
    public float GetPunishTheHelplessDamageMultiplier(Enemy target, int abilityRank)
    {
        if (!IsControlled(target))
        {
            return 1.0f; // No bonus
        }

        return abilityRank switch
        {
            3 => 2.0f,   // +100% (double damage)
            2 => 1.75f,  // +75%
            _ => 1.5f    // +50%
        };
    }

    /// <summary>
    /// Punish the Helpless (Passive)
    /// Check if target is controlled (for damage bonus)
    /// </summary>
    private bool IsControlled(Enemy target)
    {
        // Check for control effects
        return target.StatusEffects.Any(e =>
            e.EffectType == "Rooted" ||
            e.EffectType == "Slowed" ||
            e.EffectType == "Stunned" ||
            e.EffectType == "Seized" ||
            e.EffectType == "Disoriented");
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Chain Scythe (Active)
    /// AoE row damage with [Slowed] and knockdown chance
    /// </summary>
    public HlekkmasterAbilityResult ExecuteChainScythe(
        PlayerCharacter attacker,
        List<Enemy> targetRow,
        Row rowToTarget,
        int abilityRank,
        int pragmaticPrepRank = 0)
    {
        _log.Information("Executing Chain Scythe: Attacker={Attacker}, Row={Row}, TargetCount={Count}, Rank={Rank}",
            attacker.Name, rowToTarget, targetRow.Count, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 35;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int numDice = abilityRank >= 2 ? 3 : 2;
            int totalDamage = 0;
            int knockdownCount = 0;
            var statusApplied = new List<string>();

            foreach (var target in targetRow)
            {
                int damage = _diceService.Roll(numDice, 8); // 2d8 or 3d8
                damage += attacker.GetAttributeModifier("FINESSE");

                // Apply damage
                target.HP = Math.Max(0, target.HP - damage);
                totalDamage += damage;

                // Apply [Slowed]
                int slowedDuration = abilityRank >= 2 ? 3 : 2;

                // Add Pragmatic Preparation bonus
                if (pragmaticPrepRank > 0)
                {
                    slowedDuration += GetPragmaticPreparationControlBonus(pragmaticPrepRank);
                }

                ApplyControlEffect(target, "Slowed", slowedDuration);
                if (!statusApplied.Contains("Slowed"))
                {
                    statusApplied.Add("Slowed");
                }

                // Rank 3: Apply [Disoriented]
                if (abilityRank >= 3)
                {
                    ApplyControlEffect(target, "Disoriented", 1);
                    if (!statusApplied.Contains("Disoriented"))
                    {
                        statusApplied.Add("Disoriented");
                    }
                }

                // Check for knockdown vs corrupted enemies (60+)
                if (target.Corruption >= 60)
                {
                    int knockdownChance = abilityRank >= 3 ? 80 : (abilityRank >= 2 ? 60 : 40);
                    var roll = new Random().Next(1, 101);

                    if (roll <= knockdownChance)
                    {
                        ApplyControlEffect(target, "Knocked Down", 1);
                        knockdownCount++;
                        if (!statusApplied.Contains("Knocked Down"))
                        {
                            statusApplied.Add("Knocked Down");
                        }
                    }
                }
            }

            _log.Information(
                "Chain Scythe complete: Targets={Count}, Damage={Damage}, Knockdowns={Knockdowns}",
                targetRow.Count, totalDamage, knockdownCount);

            return new HlekkmasterAbilityResult
            {
                Success = true,
                Message = $"Chain Scythe hit {targetRow.Count} enemies for {totalDamage} total damage ({knockdownCount} knockdowns)",
                DamageDealt = totalDamage,
                TargetsHit = targetRow.Count,
                StatusEffectsApplied = statusApplied
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Chain Scythe");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Corruption Siphon Chain (Active)
    /// High-risk stun with corruption-scaling success rate
    /// </summary>
    public HlekkmasterAbilityResult ExecuteCorruptionSiphonChain(
        PlayerCharacter attacker,
        Enemy target,
        int abilityRank)
    {
        _log.Information("Executing Corruption Siphon Chain: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 30;
            int stressCost = abilityRank >= 3 ? 3 : 5;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Check corruption requirement
            if (target.Corruption == 0)
            {
                return HlekkmasterAbilityResult.Failure("Target has no Corruption to siphon");
            }

            // Deduct resources
            attacker.Stamina -= staminaCost;

            // Determine success chance based on corruption
            int successChance = target.Corruption switch
            {
                >= 90 => 90,  // Extreme
                >= 60 => 70,  // High
                >= 30 => 40,  // Medium
                >= 1 => 20,   // Low
                _ => 0
            };

            // Roll for success
            var roll = new Random().Next(1, 101);

            if (roll <= successChance)
            {
                // Apply [Stunned]
                int duration = abilityRank >= 2 ? 2 : 1;
                ApplyControlEffect(target, "Stunned", duration);

                // Rank 3: Purge corruption from Extreme targets
                int corruptionPurged = 0;
                if (abilityRank >= 3 && target.Corruption >= 90)
                {
                    corruptionPurged = 10;
                    target.Corruption = Math.Max(0, target.Corruption - corruptionPurged);
                }

                // Apply stress cost to attacker
                attacker.PsychicStress = Math.Min(attacker.PsychicStress + stressCost, 100);

                _log.Information(
                    "Corruption Siphon Chain SUCCESS: Corruption={Corruption}, Stunned={Duration}, Purged={Purged}",
                    target.Corruption, duration, corruptionPurged);

                return new HlekkmasterAbilityResult
                {
                    Success = true,
                    Message = $"Target stunned for {duration} turns (gained {stressCost} Psychic Stress){(corruptionPurged > 0 ? $", purged {corruptionPurged} Corruption" : "")}",
                    PsychicStressGained = stressCost,
                    CorruptionPurged = corruptionPurged,
                    StatusEffectsApplied = new List<string> { "Stunned" }
                };
            }
            else
            {
                // Apply stress cost even on failure
                attacker.PsychicStress = Math.Min(attacker.PsychicStress + stressCost, 100);

                _log.Information(
                    "Corruption Siphon Chain FAILED: Roll={Roll}/{Chance}",
                    roll, successChance);

                return new HlekkmasterAbilityResult
                {
                    Success = false,
                    Message = $"Stun failed (rolled {roll}, needed {successChance} or less). Gained {stressCost} Psychic Stress.",
                    PsychicStressGained = stressCost
                };
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Corruption Siphon Chain");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    #endregion

    #region Capstone Ability

    /// <summary>
    /// Master of Puppets (Passive Component)
    /// Apply [Vulnerable] when pulling/pushing enemies
    /// </summary>
    public void ApplyMasterOfPuppetsVulnerable(Enemy target, int abilityRank)
    {
        int duration = abilityRank >= 2 ? 2 : 1;
        ApplyControlEffect(target, "Vulnerable", duration);

        _log.Information(
            "Master of Puppets (Passive): Applied [Vulnerable] for {Duration} turns to {Target}",
            duration, target.Name);
    }

    /// <summary>
    /// Master of Puppets (Active Component)
    /// Corruption bomb - once per combat
    /// </summary>
    public HlekkmasterAbilityResult ExecuteMasterOfPuppetsActive(
        PlayerCharacter attacker,
        Enemy primaryTarget,
        List<Enemy> otherEnemies,
        int abilityRank)
    {
        _log.Warning("Executing Master of Puppets (Active): Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, primaryTarget.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 50;

            if (attacker.Stamina < staminaCost)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})");
            }

            // Check corruption requirement
            int minCorruption = abilityRank >= 3 ? 60 : 90;

            if (primaryTarget.Corruption < minCorruption)
            {
                return HlekkmasterAbilityResult.Failure(
                    $"Target must have {minCorruption}+ Corruption (has {primaryTarget.Corruption})");
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Opposed FINESSE check
            int attackerRoll = _diceService.Roll(1, 20) + attacker.GetAttributeModifier("FINESSE");
            int targetRoll = _diceService.Roll(1, 20) + primaryTarget.GetAttributeValue("finesse") / 3; // Approximate modifier

            if (attackerRoll > targetRoll)
            {
                // Success: Corruption explosion
                int numDice = abilityRank >= 2 ? 10 : 8;
                int totalDamage = 0;
                int hitCount = 0;

                foreach (var enemy in otherEnemies)
                {
                    int damage = _diceService.Roll(numDice, 10); // 8d10 or 10d10 Psychic
                    enemy.HP = Math.Max(0, enemy.HP - damage);
                    totalDamage += damage;
                    hitCount++;

                    _log.Information("Master of Puppets explosion: {Enemy} took {Damage} Psychic damage",
                        enemy.Name, damage);
                }

                _log.Warning(
                    "Master of Puppets SUCCESS: AttackerRoll={AttackerRoll}, TargetRoll={TargetRoll}, Damage={Damage}, Hits={Hits}",
                    attackerRoll, targetRoll, totalDamage, hitCount);

                return new HlekkmasterAbilityResult
                {
                    Success = true,
                    Message = $"Corruption bomb dealt {totalDamage} Psychic damage to {hitCount} enemies",
                    DamageDealt = totalDamage,
                    TargetsHit = hitCount
                };
            }
            else
            {
                _log.Warning(
                    "Master of Puppets FAILED: AttackerRoll={AttackerRoll}, TargetRoll={TargetRoll}",
                    attackerRoll, targetRoll);

                return HlekkmasterAbilityResult.Failure(
                    $"Opposed check failed ({attackerRoll} vs {targetRoll})");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Master of Puppets");
            return HlekkmasterAbilityResult.Failure($"Error: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Apply a control effect to an enemy
    /// Simplified version - in production, use AdvancedStatusEffectService
    /// </summary>
    private void ApplyControlEffect(Enemy target, string effectType, int duration)
    {
        var effect = new StatusEffect
        {
            TargetID = target.EnemyID,
            EffectType = effectType,
            DurationRemaining = duration,
            Category = StatusEffectCategory.ControlDebuff,
            StackCount = 1
        };

        // Remove any existing effect of same type
        target.StatusEffects.RemoveAll(e => e.EffectType == effectType);

        // Add new effect
        target.StatusEffects.Add(effect);

        _log.Debug("Applied {EffectType} to {Target} for {Duration} turns",
            effectType, target.Name, duration);
    }

    /// <summary>
    /// Check if an enemy is mechanical type
    /// TODO: Implement proper enemy type system
    /// </summary>
    private bool IsMechanicalEnemy(Enemy target)
    {
        // Check if enemy type suggests mechanical
        var mechanicalTypes = new[]
        {
            EnemyType.BlightDrone,
            EnemyType.WarFrame,
            EnemyType.MaintenanceConstruct,
            EnemyType.VaultCustodian,
            EnemyType.OmegaSentinel,
            EnemyType.CorrodedSentry,
            EnemyType.ArcWelderUnit,
            EnemyType.ServitorSwarm,
            EnemyType.FailureColossus,
            EnemyType.SentinelPrime
        };

        return mechanicalTypes.Contains(target.Type);
    }

    #endregion
}
