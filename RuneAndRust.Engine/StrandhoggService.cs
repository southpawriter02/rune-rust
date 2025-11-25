using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// Result of a Strandhogg ability operation
/// </summary>
public class StrandhoggAbilityResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DamageDealt { get; set; }
    public bool IsCritical { get; set; }
    public bool IsKill { get; set; }
    public int MomentumGenerated { get; set; }
    public int MomentumSpent { get; set; }
    public int StaminaRefunded { get; set; }
    public int MomentumRefunded { get; set; }
    public int TempHPGained { get; set; }
    public int PsychicStressGained { get; set; }
    public List<string> StatusEffectsApplied { get; set; } = new();
}

/// <summary>
/// v0.25.1: Service for Strandhogg (Glitch-Raider) specialization abilities
/// Implements all 9 Strandhogg abilities with Momentum resource mechanics
/// </summary>
public class StrandhoggService
{
    private static readonly ILogger _log = Log.ForContext<StrandhoggService>();
    private readonly MomentumService _momentumService;
    private readonly DiceService _diceService;
    private readonly AdvancedStatusEffectService _statusEffectService;

    public StrandhoggService(string connectionString)
    {
        _momentumService = new MomentumService();
        _diceService = new DiceService();

        // Initialize dependencies for AdvancedStatusEffectService
        var repository = new Persistence.StatusEffectRepository(connectionString);
        var traumaService = new TraumaEconomyService();
        _statusEffectService = new AdvancedStatusEffectService(repository, traumaService, _diceService);

        _log.Debug("StrandhoggService initialized");
    }

    #region Tier 1 Abilities

    /// <summary>
    /// Harrier's Alacrity I (Passive)
    /// Start combat with Momentum and bonus to Vigilance
    /// </summary>
    public int GetHarriersAlacrityVigilanceBonus(int abilityRank)
    {
        return _momentumService.GetHarriersAlacrityVigilanceBonus(abilityRank);
    }

    /// <summary>
    /// Initialize combat Momentum from Harrier's Alacrity
    /// </summary>
    public void InitializeHarriersAlacrity(PlayerCharacter character, int abilityRank)
    {
        _momentumService.InitializeCombatMomentum(character, abilityRank);
    }

    /// <summary>
    /// Reaver's Strike (Active)
    /// FINESSE-based melee attack that generates Momentum
    /// </summary>
    public StrandhoggAbilityResult ExecuteReaversStrike(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Reaver's Strike: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Calculate resource cost
            int staminaCost = abilityRank >= 2 ? 30 : 35;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int weaponDamage = (attacker.EquippedWeapon?.DamageDice * 3 + attacker.EquippedWeapon?.DamageBonus) ?? 10;
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

            // Always use FINESSE + MIGHT attributes
            totalDamage += attacker.GetAttributeModifier("FINESSE");
            totalDamage += attacker.GetAttributeModifier("MIGHT");

            // Apply damage
            target.HP = Math.Max(0, target.HP - totalDamage);
            bool isKill = target.HP == 0;

            // Generate Momentum
            int baseMomentum = 15;
            int momentumGain = baseMomentum;

            // Rank 3: Bonus Momentum vs debuffed enemies
            if (abilityRank >= 3 && _momentumService.IsTargetDebuffed(target))
            {
                momentumGain = 25; // 15 base + 10 bonus
                _log.Information("Reaver's Strike: Bonus Momentum vs debuffed target");
            }

            _momentumService.GenerateMomentum(attacker, momentumGain, "Reaver's Strike");

            _log.Information(
                "Reaver's Strike: Damage={Damage}, Momentum={Momentum}, Kill={IsKill}",
                totalDamage, momentumGain, isKill);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Reaver's Strike deals {totalDamage} damage, gains {momentumGain} Momentum",
                DamageDealt = totalDamage,
                MomentumGenerated = momentumGain,
                IsKill = isKill
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Reaver's Strike");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Dread Charge (Active)
    /// Move + attack with [Disoriented] debuff and Psychic Stress
    /// </summary>
    public StrandhoggAbilityResult ExecuteDreadCharge(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Dread Charge: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 40;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Generate Momentum from movement
            _momentumService.GenerateMomentum(attacker, MomentumService.MOMENTUM_PER_MOVE, "Dread Charge movement");

            // Calculate damage
            int numDice = abilityRank >= 2 ? 3 : 2;
            int damage = _diceService.Roll(numDice, 10);
            damage += attacker.GetAttributeModifier("MIGHT");

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Apply [Disoriented] status effect
            int disorientedDuration = abilityRank >= 2 ? 2 : 1;
            var disorientedEffect = new StatusEffect
            {
                TargetID = target.EnemyID,
                EffectType = "Disoriented",
                DurationRemaining = disorientedDuration,
                Category = StatusEffectCategory.ControlDebuff
            };
            target.StatusEffects.Add(disorientedEffect);

            // Apply Psychic Stress
            target.PsychicStress = Math.Min(target.PsychicStress + 10, 100);

            // Generate Momentum from hit
            int momentumGain = abilityRank >= 2 ? 15 : 10;
            _momentumService.GenerateMomentum(attacker, momentumGain, "Dread Charge");

            _log.Information(
                "Dread Charge: Damage={Damage}, Momentum={Momentum}, Disoriented={Duration}",
                damage, momentumGain + MomentumService.MOMENTUM_PER_MOVE, disorientedDuration);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Dread Charge deals {damage} damage, applies [Disoriented] for {disorientedDuration} turns",
                DamageDealt = damage,
                MomentumGenerated = momentumGain + MomentumService.MOMENTUM_PER_MOVE,
                IsKill = isKill,
                StatusEffectsApplied = new List<string> { "Disoriented" }
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Dread Charge");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Tier 2 Abilities

    /// <summary>
    /// Tidal Rush (Passive)
    /// Bonus Momentum when hitting debuffed enemies
    /// This is calculated in other ability methods
    /// </summary>
    public int GetTidalRushBonus(Enemy target, int abilityRank)
    {
        return _momentumService.CalculateTidalRushBonus(target, abilityRank);
    }

    /// <summary>
    /// Harrier's Whirlwind (Active)
    /// Attack then immediately reposition for free
    /// </summary>
    public StrandhoggAbilityResult ExecuteHarriersWhirlwind(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Harrier's Whirlwind: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = abilityRank >= 2 ? 40 : 45;
            int momentumCost = 30;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            if (!_momentumService.SpendMomentum(attacker, momentumCost, "Harrier's Whirlwind"))
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Momentum (need {momentumCost}, have {attacker.Momentum})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int damage = _diceService.Roll(4, 10);
            damage += attacker.GetAttributeModifier("MIGHT");

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Free move generates Momentum
            int moveBonus = abilityRank >= 3 ? 10 : 5;
            _momentumService.GenerateMomentum(attacker, moveBonus, "Harrier's Whirlwind free move");

            _log.Information(
                "Harrier's Whirlwind: Damage={Damage}, Momentum={Momentum}",
                damage, moveBonus);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Harrier's Whirlwind deals {damage} damage, free reposition gains {moveBonus} Momentum",
                DamageDealt = damage,
                MomentumSpent = momentumCost,
                MomentumGenerated = moveBonus,
                IsKill = isKill
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Harrier's Whirlwind");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Vicious Flank (Active)
    /// Massive damage vs debuffed targets (+50% damage)
    /// </summary>
    public StrandhoggAbilityResult ExecuteViciousFlank(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Vicious Flank: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 40;
            int momentumCost = abilityRank >= 2 ? 20 : 25;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            if (!_momentumService.SpendMomentum(attacker, momentumCost, "Vicious Flank"))
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Momentum (need {momentumCost}, have {attacker.Momentum})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int damage = _diceService.Roll(4, 10);
            damage += attacker.GetAttributeModifier("MIGHT");

            // Check if target is debuffed for +50% damage
            bool isDebuffed = _momentumService.IsTargetDebuffed(target);
            if (isDebuffed)
            {
                damage = (int)(damage * 1.5);
                _log.Information("Vicious Flank: +50% damage vs debuffed target");
            }

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Rank 3: Refund Momentum on kill
            int momentumRefund = 0;
            if (abilityRank >= 3 && isKill)
            {
                momentumRefund = 10;
                _momentumService.GenerateMomentum(attacker, momentumRefund, "Vicious Flank kill refund");
            }

            _log.Information(
                "Vicious Flank: Damage={Damage}, Debuffed={IsDebuffed}, Kill={IsKill}, Refund={Refund}",
                damage, isDebuffed, isKill, momentumRefund);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Vicious Flank deals {damage} damage{(isDebuffed ? " (+50% vs debuffed)" : "")}",
                DamageDealt = damage,
                MomentumSpent = momentumCost,
                MomentumRefunded = momentumRefund,
                IsKill = isKill
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Vicious Flank");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Tier 3 Abilities

    /// <summary>
    /// No Quarter (Passive)
    /// Free move + Momentum on kill
    /// Called when character kills an enemy
    /// </summary>
    public void TriggerNoQuarter(PlayerCharacter character, int abilityRank)
    {
        _log.Information("Triggering No Quarter: Character={Character}, Rank={Rank}",
            character.Name, abilityRank);

        // Free move generates Momentum
        int momentumGain = abilityRank >= 2 ? 10 : 5;
        _momentumService.GenerateMomentum(character, momentumGain, "No Quarter");

        // Rank 3: Gain temporary HP
        if (abilityRank >= 3)
        {
            character.TempHP += 15;
            _log.Information("No Quarter: Gained 15 temp HP");
        }
    }

    /// <summary>
    /// Savage Harvest (Active)
    /// Massive execution damage with resource refunds on kill
    /// </summary>
    public StrandhoggAbilityResult ExecuteSavageHarvest(PlayerCharacter attacker, Enemy target, int abilityRank)
    {
        _log.Information("Executing Savage Harvest: Attacker={Attacker}, Target={Target}, Rank={Rank}",
            attacker.Name, target.Name, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 50;
            int momentumCost = 40;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            if (!_momentumService.SpendMomentum(attacker, momentumCost, "Savage Harvest"))
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Momentum (need {momentumCost}, have {attacker.Momentum})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Calculate damage
            int numDice = abilityRank >= 2 ? 10 : 8;
            int damage = _diceService.Roll(numDice, 10);
            damage += attacker.GetAttributeModifier("MIGHT");

            // Apply damage
            target.HP = Math.Max(0, target.HP - damage);
            bool isKill = target.HP == 0;

            // Refunds on kill
            int staminaRefund = 0;
            int momentumRefund = 0;
            int healing = 0;

            if (isKill)
            {
                staminaRefund = 20;
                momentumRefund = 20;
                attacker.Stamina = Math.Min(attacker.Stamina + staminaRefund, attacker.MaxStamina);
                _momentumService.GenerateMomentum(attacker, momentumRefund, "Savage Harvest kill refund");

                // Rank 3: Heal for 20% max HP
                if (abilityRank >= 3)
                {
                    healing = (int)(attacker.MaxHP * 0.2);
                    attacker.HP = Math.Min(attacker.HP + healing, attacker.MaxHP);
                    _log.Information("Savage Harvest: Healed for {Healing} HP", healing);
                }
            }

            _log.Information(
                "Savage Harvest: Damage={Damage}, Kill={IsKill}, StaminaRefund={Stamina}, MomentumRefund={Momentum}",
                damage, isKill, staminaRefund, momentumRefund);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Savage Harvest deals {damage} damage{(isKill ? ", refunds 20 Stamina & Momentum" : "")}",
                DamageDealt = damage,
                MomentumSpent = momentumCost,
                IsKill = isKill,
                StaminaRefunded = staminaRefund,
                MomentumRefunded = momentumRefund
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Savage Harvest");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Capstone Ability

    /// <summary>
    /// Riptide of Carnage (Active)
    /// 3-4 attacks in one turn against multiple enemies
    /// </summary>
    public StrandhoggAbilityResult ExecuteRiptideOfCarnage(PlayerCharacter attacker, List<Enemy> targets, int abilityRank)
    {
        _log.Warning("Executing Riptide of Carnage: Attacker={Attacker}, Targets={TargetCount}, Rank={Rank}",
            attacker.Name, targets.Count, abilityRank);

        try
        {
            // Resource cost
            int staminaCost = 60;
            int momentumCost = 75;

            if (attacker.Stamina < staminaCost)
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Stamina (need {staminaCost}, have {attacker.Stamina})"
                };
            }

            if (!_momentumService.SpendMomentum(attacker, momentumCost, "Riptide of Carnage"))
            {
                return new StrandhoggAbilityResult
                {
                    Success = false,
                    Message = $"Insufficient Momentum (need {momentumCost}, have {attacker.Momentum})"
                };
            }

            // Deduct stamina
            attacker.Stamina -= staminaCost;

            // Determine number of attacks
            int attackCount = abilityRank >= 2 ? 4 : 3;
            int actualAttacks = Math.Min(attackCount, targets.Count);

            int totalDamage = 0;
            int kills = 0;

            // Execute attacks
            for (int i = 0; i < actualAttacks; i++)
            {
                int damage = _diceService.Roll(4, 10);
                damage += attacker.GetAttributeModifier("MIGHT");

                targets[i].HP = Math.Max(0, targets[i].HP - damage);
                totalDamage += damage;

                if (targets[i].HP == 0)
                {
                    kills++;
                }

                _log.Information("Riptide attack {Index}: Target={Target}, Damage={Damage}",
                    i + 1, targets[i].Name, damage);
            }

            // Rank 3: Refund Momentum per kill
            int momentumRefund = 0;
            if (abilityRank >= 3 && kills > 0)
            {
                momentumRefund = kills * 10;
                _momentumService.GenerateMomentum(attacker, momentumRefund, "Riptide of Carnage kill refunds");
            }

            // Apply Psychic Stress cost
            int stressCost = abilityRank >= 3 ? 10 : 15;
            attacker.PsychicStress = Math.Min(attacker.PsychicStress + stressCost, 100);

            _log.Warning(
                "Riptide of Carnage: Attacks={Attacks}, Damage={Damage}, Kills={Kills}, Stress={Stress}",
                actualAttacks, totalDamage, kills, stressCost);

            return new StrandhoggAbilityResult
            {
                Success = true,
                Message = $"Riptide of Carnage: {actualAttacks} attacks, {totalDamage} total damage, {kills} kills",
                DamageDealt = totalDamage,
                MomentumSpent = momentumCost,
                MomentumRefunded = momentumRefund,
                PsychicStressGained = stressCost,
                IsKill = kills > 0
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error executing Riptide of Carnage");
            return new StrandhoggAbilityResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    #endregion
}
