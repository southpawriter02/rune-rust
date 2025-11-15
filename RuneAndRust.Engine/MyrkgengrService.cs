using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Myrk-gengr ability operation
/// </summary>
public class MyrkgengrAbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public bool IsCritical { get; set; }
    public bool IsKill { get; set; }
    public int PsychicStressInflicted { get; set; }
    public int CorruptionGained { get; set; }
    public bool StealthMaintained { get; set; }
    public int StaminaRefunded { get; set; }
    public List<string> StatusEffectsApplied { get; set; } = new();
}

/// <summary>
/// v0.24.2: Service for Myrk-gengr (Shadow-Walker) specialization abilities
/// Implements all 9 Shadow-Walker abilities with stealth mechanics and psychological warfare
/// </summary>
public class MyrkgengrService
{
    private static readonly ILogger _log = Log.ForContext<MyrkgengrService>();
    private readonly AdvancedStatusEffectService _statusService;
    private readonly DiceService _diceService;
    private readonly ResolveCheckService _resolveService;

    public MyrkgengrService(string connectionString)
    {
        _statusService = new AdvancedStatusEffectService(connectionString);
        _diceService = new DiceService();
        _resolveService = new ResolveCheckService();
        _log.Debug("MyrkgengrService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// One with the Static I (Passive)
    /// Provides bonus to Stealth checks, enhanced in Psychic Resonance zones
    /// </summary>
    public int GetStealthBonus(int abilityRank, bool inPsychicResonanceZone = false)
    {
        int baseBonus = abilityRank switch
        {
            3 => 3, // +3d10
            2 => 2, // +2d10
            _ => 1  // +1d10
        };

        // Additional bonus in Psychic Resonance zones
        if (inPsychicResonanceZone)
        {
            baseBonus += 2; // +2d10 additional
        }

        return baseBonus;
    }

    /// <summary>
    /// Enter the Void (Active)
    /// Attempt to enter [Hidden] state via stealth check
    /// </summary>
    public MyrkgengrAbilityResult EnterTheVoid(PlayerCharacter shadowWalker, int abilityRank, bool inPsychicResonanceZone = false)
    {
        _log.Information("Attempting Enter the Void: Character={Name}, Rank={Rank}",
            shadowWalker.Name, abilityRank);

        try
        {
            // Calculate stamina cost
            int staminaCost = abilityRank switch
            {
                3 => 35,
                2 => 35,
                _ => 40
            };

            if (shadowWalker.Stamina < staminaCost)
            {
                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {shadowWalker.Stamina})"
                };
            }

            // Deduct stamina
            shadowWalker.Stamina -= staminaCost;

            // Calculate stealth DC
            int dc = abilityRank switch
            {
                3 => 12,
                2 => 14,
                _ => 16
            };

            // Get stealth bonus from One with the Static
            int stealthBonus = GetStealthBonus(abilityRank, inPsychicResonanceZone);

            // Make stealth check (simplified - would use proper skill check in full implementation)
            int stealthRoll = _diceService.Roll(stealthBonus, 10);
            bool success = stealthRoll >= dc;

            if (success)
            {
                // Apply Hidden status effect
                var hiddenEffect = new StatusEffect
                {
                    TargetID = shadowWalker.CharacterID,
                    EffectType = "Hidden",
                    DurationRemaining = -1, // Lasts until broken
                    Category = StatusEffectCategory.Buff,
                    CanStack = false,
                    Metadata = "{ \"Source\": \"Enter the Void\" }"
                };

                _statusService.ApplyStatusEffect(shadowWalker, hiddenEffect);

                _log.Information("Enter the Void success: Roll={Roll}, DC={DC}",
                    stealthRoll, dc);

                return new MyrkgengrAbilityResult
                {
                    Success = true,
                    Message = $"You vanish into the psychic static ([Hidden] state entered)",
                    StatusEffectsApplied = new List<string> { "Hidden" }
                };
            }
            else
            {
                _log.Information("Enter the Void failed: Roll={Roll}, DC={DC}",
                    stealthRoll, dc);

                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = $"Failed to enter [Hidden] state (rolled {stealthRoll}, needed {dc})"
                };
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Enter the Void");
            return new MyrkgengrAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Shadow Strike (Active)
    /// Guaranteed critical hit attack from [Hidden] state
    /// </summary>
    public MyrkgengrAbilityResult ExecuteShadowStrike(PlayerCharacter attacker, Enemy target, int abilityRank,
        int terrorFromVoidRank = 0, int ghostlyFormRank = 0)
    {
        _log.Information("Executing Shadow Strike: Attacker={Name}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Verify Hidden state
            if (!HasStatusEffect(attacker, "Hidden"))
            {
                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = "Must be in [Hidden] state to use Shadow Strike"
                };
            }

            // Resource check
            int staminaCost = 35;
            if (attacker.Stamina < staminaCost)
            {
                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            attacker.Stamina -= staminaCost;

            // Calculate damage (guaranteed crit = double damage)
            int weaponDamage = (attacker.EquippedWeapon?.DamageDice * 3 + attacker.EquippedWeapon?.DamageBonus) ?? 10;
            int finesseModifier = attacker.GetAttributeModifier("FINESSE");

            // Rank bonus damage
            int bonusDice = abilityRank switch
            {
                3 => 4, // +4d6
                2 => 2, // +2d6
                _ => 0
            };

            int bonusDamage = bonusDice > 0 ? _diceService.Roll(bonusDice, 6) : 0;

            // Total damage (doubled for crit)
            int baseDamage = weaponDamage + finesseModifier + bonusDamage;
            int totalDamage = baseDamage * 2; // Guaranteed critical hit

            var statusEffectsApplied = new List<string>();
            int psychicStressInflicted = 0;

            // Check if this is first Shadow Strike (Terror from the Void)
            bool isFirstStrike = !attacker.CombatFlags.GetValueOrDefault("TerrorFromVoidUsed", false);
            if (isFirstStrike && terrorFromVoidRank > 0)
            {
                // Apply Psychic Stress
                psychicStressInflicted = terrorFromVoidRank switch
                {
                    3 => 18,
                    2 => 15,
                    _ => 12
                };

                target.PsychicStress = Math.Min(100, target.PsychicStress + psychicStressInflicted);

                // Apply Fear
                int fearChance = terrorFromVoidRank switch
                {
                    3 => 100,
                    2 => 85,
                    _ => 70
                };

                int fearDuration = terrorFromVoidRank >= 2 ? 3 : 2;

                if (_diceService.Roll(1, 100) <= fearChance)
                {
                    var fearEffect = new StatusEffect
                    {
                        TargetID = target.EnemyID,
                        EffectType = "Feared",
                        DurationRemaining = fearDuration,
                        Category = StatusEffectCategory.ControlDebuff
                    };
                    target.StatusEffects.Add(fearEffect);
                    statusEffectsApplied.Add("Feared");

                    _log.Information("Terror from the Void: {Stress} Stress, Fear applied",
                        psychicStressInflicted);
                }

                // Rank 3: AoE fear check for witnesses (simplified - would check grid positions)
                if (terrorFromVoidRank >= 3)
                {
                    _log.Information("Terror from the Void Rank 3: AoE fear would trigger for witnesses");
                }

                attacker.CombatFlags["TerrorFromVoidUsed"] = true;
            }

            // Apply damage
            target.HP = Math.Max(0, target.HP - totalDamage);
            bool isKill = target.HP == 0;

            // Rank 3: Apply Bleeding
            if (abilityRank >= 3)
            {
                var bleedingEffect = new StatusEffect
                {
                    TargetID = target.EnemyID,
                    EffectType = "Bleeding",
                    DurationRemaining = 2,
                    DamageBase = "2d6",
                    Category = StatusEffectCategory.DamageOverTime
                };
                target.StatusEffects.Add(bleedingEffect);
                statusEffectsApplied.Add("Bleeding");
            }

            // Rank 2: Refund stamina on kill
            int staminaRefunded = 0;
            if (abilityRank >= 2 && isKill)
            {
                staminaRefunded = 20;
                attacker.Stamina = Math.Min(attacker.MaxStamina, attacker.Stamina + staminaRefunded);
                _log.Information("Shadow Strike kill: Refunded 20 Stamina");
            }

            // Check Ghostly Form persistence
            bool stealthMaintained = false;
            if (ghostlyFormRank > 0)
            {
                int persistChance = ghostlyFormRank switch
                {
                    3 => 80,
                    2 => 65,
                    _ => 50
                };

                if (_diceService.Roll(1, 100) <= persistChance)
                {
                    stealthMaintained = true;
                    _log.Information("Ghostly Form: Stealth persisted ({Chance}%)", persistChance);

                    // Rank 3: Grant Free Move (simplified - would be handled by action system)
                    if (ghostlyFormRank >= 3)
                    {
                        _log.Information("Ghostly Form Rank 3: Free Move granted");
                    }
                }
                else
                {
                    // Break stealth
                    RemoveStatusEffect(attacker, "Hidden");
                    _log.Information("Stealth broken");
                }
            }
            else
            {
                // No Ghostly Form = always break stealth
                RemoveStatusEffect(attacker, "Hidden");
            }

            _log.Information(
                "Shadow Strike: Damage={Damage}, Critical=true, Kill={IsKill}, Stress={Stress}",
                totalDamage, isKill, psychicStressInflicted);

            return new MyrkgengrAbilityResult
            {
                Success = true,
                Message = $"Shadow Strike deals {totalDamage} damage (CRITICAL!){(isKill ? " - TARGET ELIMINATED" : "")}",
                DamageDealt = totalDamage,
                IsCritical = true,
                IsKill = isKill,
                PsychicStressInflicted = psychicStressInflicted,
                StaminaRefunded = staminaRefunded,
                StealthMaintained = stealthMaintained,
                StatusEffectsApplied = statusEffectsApplied
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Shadow Strike");
            return new MyrkgengrAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Throat-Cutter (Active)
    /// Melee attack with [Silenced] from flanking/Hidden
    /// </summary>
    public MyrkgengrAbilityResult ExecuteThroatCutter(PlayerCharacter attacker, Enemy target, int abilityRank,
        bool isFlankingOrHidden = false)
    {
        _log.Information("Executing Throat-Cutter: Attacker={Name}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        int staminaCost = 45;
        if (attacker.Stamina < staminaCost)
        {
            return new MyrkgengrAbilityResult
            {
                Success = false,
                Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
            };
        }

        attacker.Stamina -= staminaCost;

        // Calculate damage
        int weaponDamage = attacker.EquippedWeapon?.Damage ?? 10;
        int bonusDice = abilityRank switch
        {
            3 => 4, // 4d8
            2 => 3, // 3d8
            _ => 2  // 2d8
        };

        int bonusDamage = _diceService.Roll(bonusDice, 8);
        int totalDamage = weaponDamage + bonusDamage + attacker.GetAttributeModifier("FINESSE");

        // Apply damage
        target.HP = Math.Max(0, target.HP - totalDamage);

        var statusEffectsApplied = new List<string>();

        // Apply Silenced if from flank/Hidden
        if (isFlankingOrHidden)
        {
            int silenceDuration = abilityRank >= 2 ? 2 : 1;

            var silencedEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "Silenced",
                DurationRemaining = silenceDuration,
                Category = StatusEffectCategory.ControlDebuff
            };
            target.StatusEffects.Add(silencedEffect);
            statusEffectsApplied.Add("Silenced");

            _log.Information("Throat-Cutter: Silenced applied for {Duration} turns", silenceDuration);
        }

        // Rank 3: Apply Bleeding if target was Feared
        if (abilityRank >= 3 && target.StatusEffects.Any(e => e.EffectType == "Feared"))
        {
            var bleedingEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "Bleeding",
                DurationRemaining = 3,
                DamageBase = "2d6",
                Category = StatusEffectCategory.DamageOverTime
            };
            target.StatusEffects.Add(bleedingEffect);
            statusEffectsApplied.Add("Bleeding");

            _log.Information("Throat-Cutter Rank 3: Bleeding applied to Feared target");
        }

        return new MyrkgengrAbilityResult
        {
            Success = true,
            Message = $"Throat-Cutter deals {totalDamage} damage{(statusEffectsApplied.Any() ? $" + {string.Join(", ", statusEffectsApplied)}" : "")}",
            DamageDealt = totalDamage,
            StatusEffectsApplied = statusEffectsApplied
        };
    }

    /// <summary>
    /// Mind of Stillness (Passive)
    /// Regenerate Stamina and remove Psychic Stress while Hidden
    /// </summary>
    public void ApplyMindOfStillness(PlayerCharacter shadowWalker, int abilityRank)
    {
        if (!HasStatusEffect(shadowWalker, "Hidden"))
            return;

        int stressReduction = abilityRank switch
        {
            3 => 7,
            2 => 5,
            _ => 3
        };

        int staminaRegen = abilityRank switch
        {
            3 => 10,
            2 => 8,
            _ => 5
        };

        shadowWalker.PsychicStress = Math.Max(0, shadowWalker.PsychicStress - stressReduction);
        shadowWalker.Stamina = Math.Min(shadowWalker.MaxStamina, shadowWalker.Stamina + staminaRegen);

        _log.Debug("Mind of Stillness: Removed {Stress} stress, regenerated {Stamina} stamina",
            stressReduction, staminaRegen);
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// Ghostly Form (Passive)
    /// Provides Defense bonus while Hidden and chance to persist stealth after Shadow Strike
    /// NOTE: Persistence logic is handled in ExecuteShadowStrike
    /// </summary>
    public int GetGhostlyFormDefenseBonus(int abilityRank, bool isHidden)
    {
        if (!isHidden)
            return 0;

        return abilityRank switch
        {
            3 => 4, // +4d10
            2 => 3, // +3d10
            _ => 2  // +2d10
        };
    }

    #endregion

    #region Capstone Ability

    /// <summary>
    /// Living Glitch (Active)
    /// Ultimate assassination: guaranteed hit, massive damage, catastrophic Psychic Stress, high self-Corruption
    /// </summary>
    public MyrkgengrAbilityResult ExecuteLivingGlitch(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Living Glitch: Attacker={Name}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Verify Hidden state
            if (!HasStatusEffect(attacker, "Hidden"))
            {
                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = "Must be in [Hidden] state to use Living Glitch"
                };
            }

            // Resource check
            int staminaCost = 60;
            int focusCost = 75; // Note: Focus resource not yet implemented

            if (attacker.Stamina < staminaCost)
            {
                return new MyrkgengrAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            // Deduct resources
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int damageDice = abilityRank switch
            {
                3 => 12, // 12d10
                2 => 10, // 10d10
                _ => 8   // 8d10
            };

            int diceDamage = _diceService.Roll(damageDice, 10);
            int weaponDamage = (attacker.EquippedWeapon?.DamageDice * 3 + attacker.EquippedWeapon?.DamageBonus) ?? 10;
            int finesseModifier = attacker.GetAttributeModifier("FINESSE");

            // Total damage (weapon + dice + FINESSE x2)
            int totalDamage = diceDamage + weaponDamage + (finesseModifier * 2);

            // Apply Psychic Stress to target
            int psychicStress = abilityRank switch
            {
                3 => 35,
                2 => 30,
                _ => 25
            };

            target.PsychicStress = Math.Min(100, target.PsychicStress + psychicStress);

            // Apply self-Corruption
            int selfCorruption = abilityRank switch
            {
                3 => 12,
                2 => 15,
                _ => 18
            };

            attacker.Corruption = Math.Min(100, attacker.Corruption + selfCorruption);

            // Guaranteed hit - apply damage
            target.HP = Math.Max(0, target.HP - totalDamage);
            bool isKill = target.HP == 0;

            // Rank 3: Don't break stealth if kill
            bool stealthMaintained = false;
            if (abilityRank >= 3 && isKill)
            {
                stealthMaintained = true;
                _log.Information("Living Glitch Rank 3: Kill without breaking stealth");
            }
            else
            {
                // Break stealth
                RemoveStatusEffect(attacker, "Hidden");
            }

            _log.Warning(
                "Living Glitch: Guaranteed hit, {Damage} damage, {Stress} Stress to target, {Corruption} self-Corruption",
                totalDamage, psychicStress, selfCorruption);

            return new MyrkgengrAbilityResult
            {
                Success = true,
                Message = $"LIVING GLITCH: {totalDamage} damage (GUARANTEED HIT) + {psychicStress} Stress{(isKill ? " - TARGET ELIMINATED" : "")}",
                DamageDealt = totalDamage,
                IsCritical = true, // Effectively a crit due to guaranteed hit + massive damage
                IsKill = isKill,
                PsychicStressInflicted = psychicStress,
                CorruptionGained = selfCorruption,
                StealthMaintained = stealthMaintained
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Living Glitch");
            return new MyrkgengrAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if character has a specific status effect
    /// </summary>
    private bool HasStatusEffect(PlayerCharacter character, string effectType)
    {
        return character.StatusEffects.Any(e => e.EffectType == effectType);
    }

    /// <summary>
    /// Remove a specific status effect from character
    /// </summary>
    private void RemoveStatusEffect(PlayerCharacter character, string effectType)
    {
        character.StatusEffects.RemoveAll(e => e.EffectType == effectType);
    }

    #endregion
}
