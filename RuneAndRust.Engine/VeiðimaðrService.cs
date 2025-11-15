using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Veiðimaðr ability operation
/// </summary>
public class VeiðimaðrAbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public bool IsCritical { get; set; }
    public bool IsKill { get; set; }
    public int CorruptionPurged { get; set; }
    public int StaminaRefunded { get; set; }
    public int FocusRefunded { get; set; }
    public List<string> StatusEffectsApplied { get; set; } = new();
}

/// <summary>
/// v0.24.1: Service for Veiðimaðr (Hunter) specialization abilities
/// Implements all 9 Hunter abilities with corruption tracking and precision archery
/// </summary>
public class VeiðimaðrService
{
    private static readonly ILogger _log = Log.ForContext<VeiðimaðrService>();
    private readonly CorruptionTrackingService _corruptionService;
    private readonly MarkingService _markingService;
    private readonly DiceService _diceService;

    public VeiðimaðrService(string connectionString)
    {
        _corruptionService = new CorruptionTrackingService(connectionString);
        _markingService = new MarkingService(connectionString);
        _diceService = new DiceService();
        _log.Debug("VeiðimaðrService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Wilderness Acclimation I (Passive)
    /// Provides bonus to WITS-based tracking/perception checks
    /// </summary>
    public int GetWildernessAcklimationBonus(int abilityRank)
    {
        return abilityRank switch
        {
            3 => 3, // +3d10
            2 => 2, // +2d10
            _ => 1  // +1d10
        };
    }

    /// <summary>
    /// Aimed Shot (Active)
    /// FINESSE-based ranged attack, bread-and-butter damage ability
    /// </summary>
    public VeiðimaðrAbilityResult ExecuteAimedShot(PlayerCharacter hunter, Enemy target, int abilityRank)
    {
        _log.Information("Executing Aimed Shot: Hunter={Hunter}, Target={Target}, Rank={Rank}",
            hunter.Name, target.Name, abilityRank);

        try
        {
            // Calculate resource cost
            int staminaCost = abilityRank >= 2 ? 35 : 40;

            if (hunter.Stamina < staminaCost)
            {
                return new VeiðimaðrAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {hunter.Stamina})"
                };
            }

            // Deduct stamina
            hunter.Stamina -= staminaCost;

            // Calculate damage
            int weaponDamage = hunter.EquippedWeapon?.Damage ?? 10; // Default to 10 if no weapon
            int bonusDice = abilityRank switch
            {
                3 => 2, // +2d6
                2 => 1, // +1d6
                _ => 0
            };

            int totalDamage = weaponDamage;

            // Add rank bonus damage
            if (bonusDice > 0)
            {
                totalDamage += _diceService.Roll(bonusDice, 6);
            }

            // Always use FINESSE attribute modifier
            totalDamage += hunter.GetAttributeModifier("FINESSE");

            // Check for critical hit
            int baseCritChance = 10; // 10% base crit chance
            int corruptionCritBonus = _corruptionService.CalculateCritChanceBonus(hunter, target);
            int totalCritChance = baseCritChance + corruptionCritBonus;

            bool isCritical = _diceService.Roll(1, 100) <= totalCritChance;

            if (isCritical)
            {
                totalDamage = (int)(totalDamage * 1.5); // 50% bonus for crits

                // Rank 3: Apply Bleeding on crit
                if (abilityRank >= 3)
                {
                    var bleedingEffect = new StatusEffect
                    {
                        Type = StatusEffectType.Bleeding,
                        Duration = 2,
                        DamagePerTurn = _diceService.Roll(1, 6)
                    };
                    target.StatusEffects.Add(bleedingEffect);
                    _log.Information("Aimed Shot critical: Applied [Bleeding] for 2 turns");
                }
            }

            // Apply damage
            target.HP = Math.Max(0, target.HP - totalDamage);
            bool isKill = target.HP == 0;

            _log.Information(
                "Aimed Shot: Damage={Damage}, Critical={IsCrit}, Kill={IsKill}",
                totalDamage, isCritical, isKill);

            return new VeiðimaðrAbilityResult
            {
                Success = true,
                Message = $"Aimed Shot deals {totalDamage} damage{(isCritical ? " (CRITICAL!)" : "")}",
                DamageDealt = totalDamage,
                IsCritical = isCritical,
                IsKill = isKill
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Aimed Shot");
            return new VeiðimaðrAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Set Snare (Active)
    /// Place trap that Roots enemies
    /// </summary>
    public VeiðimaðrAbilityResult SetSnare(PlayerCharacter hunter, int abilityRank)
    {
        _log.Information("Setting Snare: Hunter={Hunter}, Rank={Rank}", hunter.Name, abilityRank);

        int staminaCost = 35;

        if (hunter.Stamina < staminaCost)
        {
            return new VeiðimaðrAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {hunter.Stamina})"
            };
        }

        hunter.Stamina -= staminaCost;

        int rootDuration = abilityRank switch
        {
            3 => 3,
            2 => 2,
            _ => 1
        };

        int trapDamage = abilityRank >= 3 ? _diceService.Roll(2, 6) : 0;

        _log.Information("Snare set: Root duration={Duration}, Damage={Damage}", rootDuration, trapDamage);

        return new VeiðimaðrAbilityResult
        {
            Success = true,
            Message = $"Snare set (Root for {rootDuration} turns{(trapDamage > 0 ? $", {trapDamage} damage" : "")})"
        };
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Mark for Death (Active)
    /// Apply [Marked] debuff, gain bonus damage vs target
    /// </summary>
    public MarkingResult ExecuteMarkForDeath(PlayerCharacter hunter, Enemy target, int abilityRank)
    {
        _log.Information("Executing Mark for Death: Hunter={Hunter}, Target={Target}, Rank={Rank}",
            hunter.Name, target.Name, abilityRank);

        int staminaCost = 30;

        if (hunter.Stamina < staminaCost)
        {
            return new MarkingResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {hunter.Stamina})"
            };
        }

        hunter.Stamina -= staminaCost;

        return _markingService.ApplyMarkForDeath(hunter, target, abilityRank);
    }

    /// <summary>
    /// Blight-Tipped Arrow (Active)
    /// Physical damage + DoT + potential Glitch vs corrupted enemies
    /// </summary>
    public VeiðimaðrAbilityResult ExecuteBlightTippedArrow(PlayerCharacter hunter, Enemy target, int abilityRank)
    {
        _log.Information("Executing Blight-Tipped Arrow: Hunter={Hunter}, Target={Target}, Rank={Rank}",
            hunter.Name, target.Name, abilityRank);

        int staminaCost = 45;

        if (hunter.Stamina < staminaCost)
        {
            return new VeiðimaðrAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {hunter.Stamina})"
            };
        }

        hunter.Stamina -= staminaCost;

        // Calculate base damage
        int baseDamageDice = abilityRank >= 2 ? 4 : 3;
        int baseDamage = _diceService.Roll(baseDamageDice, 6);
        baseDamage += hunter.GetAttributeModifier("FINESSE");

        // Apply damage
        target.HP = Math.Max(0, target.HP - baseDamage);

        // Apply Blighted Toxin DoT
        int toxinDuration = abilityRank >= 2 ? 4 : 3;
        int toxinDamageDice = abilityRank >= 3 ? 3 : 2;

        var toxinEffect = new StatusEffect
        {
            Type = StatusEffectType.BlightedToxin,
            Duration = toxinDuration,
            DamagePerTurn = toxinDamageDice * 3 // Average of d6 per die
        };
        target.StatusEffects.Add(toxinEffect);

        var statusApplied = new List<string> { "Blighted Toxin" };

        // Check for Glitch proc vs corrupted targets
        if (_corruptionService.MeetsCorruptionThreshold(target, 30))
        {
            int glitchChance = abilityRank switch
            {
                3 => 80,
                2 => 60,
                _ => 40
            };

            if (_diceService.Roll(1, 100) <= glitchChance)
            {
                var glitchEffect = new StatusEffect
                {
                    Type = StatusEffectType.Glitched,
                    Duration = abilityRank >= 3 ? 1 : 0
                };
                target.StatusEffects.Add(glitchEffect);
                statusApplied.Add("Glitch");

                _log.Information("Blight-Tipped Arrow: Glitch proc'd ({Chance}% chance)", glitchChance);
            }
        }

        _log.Information(
            "Blight-Tipped Arrow: Damage={Damage}, Toxin={ToxinDuration} turns",
            baseDamage, toxinDuration);

        return new VeiðimaðrAbilityResult
        {
            Success = true,
            Message = $"Blight-Tipped Arrow deals {baseDamage} damage + toxin ({toxinDuration} turns)",
            DamageDealt = baseDamage,
            StatusEffectsApplied = statusApplied
        };
    }

    /// <summary>
    /// Predator's Focus (Passive)
    /// Provides Resolve bonus while in back row
    /// </summary>
    public int GetPredatorsFocusBonus(int abilityRank, bool inBackRow)
    {
        if (!inBackRow) return 0;

        return abilityRank switch
        {
            3 => 3, // +3d10
            2 => 2, // +2d10
            _ => 1  // +1d10
        };
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Exploit Corruption (Passive)
    /// Increased crit chance vs corrupted targets
    /// Implemented via CorruptionTrackingService.CalculateCritChanceBonus()
    /// </summary>

    /// <summary>
    /// Heartseeker Shot (Active)
    /// Charge ability: massive damage + corruption purge if target marked
    /// </summary>
    public VeiðimaðrAbilityResult ChargeHeartseekerShot(PlayerCharacter hunter)
    {
        _log.Information("Charging Heartseeker Shot: Hunter={Hunter}", hunter.Name);

        // Apply charging status
        var chargingEffect = new StatusEffect
        {
            Type = StatusEffectType.Charging,
            Duration = 1,
            Metadata = new Dictionary<string, object> { { "AbilityName", "Heartseeker Shot" } }
        };
        hunter.StatusEffects.Add(chargingEffect);

        return new VeiðimaðrAbilityResult
        {
            Success = true,
            Message = "Charging Heartseeker Shot (releases next turn)..."
        };
    }

    /// <summary>
    /// Release Heartseeker Shot after charging
    /// </summary>
    public VeiðimaðrAbilityResult ReleaseHeartseekerShot(PlayerCharacter hunter, Enemy target, int abilityRank)
    {
        _log.Information("Releasing Heartseeker Shot: Hunter={Hunter}, Target={Target}, Rank={Rank}",
            hunter.Name, target.Name, abilityRank);

        // Check if charged
        if (!hunter.StatusEffects.Any(e => e.Type == StatusEffectType.Charging &&
            (string?)e.Metadata?["AbilityName"] == "Heartseeker Shot"))
        {
            return new VeiðimaðrAbilityResult
            {
                Success = false,
                Message = "Must charge Heartseeker Shot for 1 full turn first"
            };
        }

        int staminaCost = 60;
        int focusCost = 30;

        if (hunter.Stamina < staminaCost)
        {
            return new VeiðimaðrAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {hunter.Stamina})"
            };
        }

        // Note: Focus resource not yet implemented in PlayerCharacter, skipping check

        hunter.Stamina -= staminaCost;

        // Calculate base damage
        int damageDice = abilityRank switch
        {
            3 => 10, // 10d10
            2 => 8,  // 8d10
            _ => 6   // 6d10
        };

        int baseDamage = _diceService.Roll(damageDice, 10);
        baseDamage += hunter.GetAttributeModifier("FINESSE");

        int totalDamage = baseDamage;
        int corruptionPurged = 0;

        // If target is marked, purge corruption
        if (_markingService.IsMarked(target))
        {
            int maxPurge = abilityRank switch
            {
                3 => 20,
                2 => 15,
                _ => 10
            };

            corruptionPurged = _corruptionService.PurgeCorruption(target, maxPurge);
            int bonusDamage = corruptionPurged * 2; // +2 damage per corruption purged

            totalDamage += bonusDamage;

            _log.Information(
                "Heartseeker Shot: Purged {Purged} Corruption, +{Bonus} bonus damage",
                corruptionPurged, bonusDamage);
        }

        // Apply damage
        target.HP = Math.Max(0, target.HP - totalDamage);
        bool isKill = target.HP == 0;

        int staminaRefunded = 0;
        int focusRefunded = 0;

        // Rank 3: Refund resources on kill of marked target
        if (abilityRank >= 3 && isKill && _markingService.IsMarked(target))
        {
            staminaRefunded = 30;
            focusRefunded = 15;
            hunter.Stamina = Math.Min(hunter.MaxStamina, hunter.Stamina + staminaRefunded);

            _log.Information("Heartseeker Shot: Killed marked target, refunded resources");
        }

        // Remove charging status
        hunter.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Charging);

        _log.Information(
            "Heartseeker Shot: Damage={Damage}, CorruptionPurged={Purged}, Kill={IsKill}",
            totalDamage, corruptionPurged, isKill);

        return new VeiðimaðrAbilityResult
        {
            Success = true,
            Message = $"Heartseeker Shot deals {totalDamage} damage{(corruptionPurged > 0 ? $" (purged {corruptionPurged} Corruption)" : "")}",
            DamageDealt = totalDamage,
            CorruptionPurged = corruptionPurged,
            IsKill = isKill,
            StaminaRefunded = staminaRefunded,
            FocusRefunded = focusRefunded
        };
    }

    #endregion

    #region Capstone Ability

    /// <summary>
    /// Stalker of the Unseen (Passive + Active)
    /// Passive: Auto-reveal vulnerabilities on Mark
    /// Active: Toggle stance for vision immunity + stagger procs
    /// </summary>
    public VeiðimaðrAbilityResult ActivateStalkerStance(PlayerCharacter hunter, int abilityRank)
    {
        _log.Information("Toggling Stalker of the Unseen stance: Hunter={Hunter}, Rank={Rank}",
            hunter.Name, abilityRank);

        // Check if already in stance
        var existingStance = hunter.StatusEffects.FirstOrDefault(e =>
            e.Type == StatusEffectType.Stance &&
            (string?)e.Metadata?["StanceName"] == "Blight-Stalker Stance");

        if (existingStance != null)
        {
            // Deactivate stance
            hunter.StatusEffects.Remove(existingStance);

            // Apply stress penalty
            int stressPenalty = abilityRank >= 3 ? 5 : 10;
            hunter.PsychicStress = Math.Min(100, hunter.PsychicStress + stressPenalty);

            _log.Information("Deactivated Stalker Stance, {Stress} Psychic Stress inflicted", stressPenalty);

            return new VeiðimaðrAbilityResult
            {
                Success = true,
                Message = $"Blight-Stalker Stance deactivated ({stressPenalty} Psychic Stress)"
            };
        }
        else
        {
            // Activate stance
            int staminaUpkeep = abilityRank >= 2 ? 15 : 20;
            int staggerChance = abilityRank switch
            {
                3 => 90,
                2 => 70,
                _ => 50
            };
            int attackBonus = abilityRank >= 3 ? 2 : 0; // +2d10 at rank 3

            var stanceEffect = new StatusEffect
            {
                Type = StatusEffectType.Stance,
                Duration = -1, // Permanent until toggled off
                Metadata = new Dictionary<string, object>
                {
                    { "StanceName", "Blight-Stalker Stance" },
                    { "StaminaUpkeep", staminaUpkeep },
                    { "ImmuneVisualImpairment", true },
                    { "StaggerChanceVsCorrupted", staggerChance },
                    { "AttackBonusVsCorrupted", attackBonus }
                }
            };

            hunter.StatusEffects.Add(stanceEffect);

            _log.Information("Activated Stalker Stance: Upkeep={Upkeep}, Stagger={Stagger}%, Bonus={Bonus}d10",
                staminaUpkeep, staggerChance, attackBonus);

            return new VeiðimaðrAbilityResult
            {
                Success = true,
                Message = $"Blight-Stalker Stance activated ({staminaUpkeep} Stamina/turn)"
            };
        }
    }

    #endregion
}
