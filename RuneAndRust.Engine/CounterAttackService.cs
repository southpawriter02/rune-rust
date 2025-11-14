using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.21.4: Counter-Attack Service
/// Manages the universal parry system and riposte mechanics
/// Based on v2.0 canonical Parry System and Hólmgangr Reactive Parry
/// </summary>
public class CounterAttackService
{
    private static readonly ILogger _log = Log.ForContext<CounterAttackService>();
    private readonly DiceService _diceService;
    private readonly CounterAttackRepository _repository;
    private readonly TraumaEconomyService _traumaService;
    private readonly SpecializationService? _specializationService;

    // Parry constants (v2.0 canonical)
    private const int BaseParryLimit = 1; // Per round
    private const int CriticalParryThreshold = 5; // Parry > Accuracy by 5+

    // Stress values for trauma economy
    private const int FailedParryStress = 5;
    private const int SuperiorParryStressRelief = -3;
    private const int CriticalParryStressRelief = -8;
    private const int RiposteMissStress = 3;
    private const int RiposteKillStressRelief = -10;

    public CounterAttackService(
        DiceService diceService,
        CounterAttackRepository repository,
        TraumaEconomyService traumaService,
        SpecializationService? specializationService = null)
    {
        _diceService = diceService;
        _repository = repository;
        _traumaService = traumaService;
        _specializationService = specializationService;
        _log.Debug("CounterAttackService initialized");
    }

    #region Parry Execution

    /// <summary>
    /// Execute a parry attempt against an incoming attack
    /// v2.0 canonical: Parry Pool = FINESSE + Weapon Skill + Modifiers
    /// </summary>
    /// <param name="defender">Character attempting to parry</param>
    /// <param name="attacker">Character making the attack</param>
    /// <param name="attackAccuracy">The accuracy roll of the incoming attack</param>
    /// <param name="combatInstanceId">Combat instance ID for logging</param>
    /// <returns>ParryResult with outcome and riposte information</returns>
    public ParryResult ExecuteParry(
        PlayerCharacter defender,
        Enemy attacker,
        int attackAccuracy,
        int combatInstanceId = 0,
        string? attackAbility = null)
    {
        _log.Information("Parry attempt: Defender={DefenderName}, Attacker={AttackerName}, AttackAccuracy={Accuracy}",
            defender.Name, attacker.Name, attackAccuracy);

        var result = new ParryResult
        {
            DefenderID = GetCharacterId(defender),
            AttackerID = GetEnemyId(attacker),
            AccuracyRoll = attackAccuracy,
            AttackAbility = attackAbility
        };

        // 1. Calculate Parry Pool (v2.0 canonical: FINESSE + Weapon Skill + Modifiers)
        int parryPool = CalculateParryPool(defender);
        result.ParryRoll = parryPool;

        _log.Debug("Parry Pool calculated: Defender={DefenderName}, ParryPool={ParryPool}",
            defender.Name, parryPool);

        // 2. Determine Parry Outcome
        result.Outcome = DetermineParryOutcome(parryPool, attackAccuracy);
        result.Success = result.Outcome != ParryOutcome.Failed;

        _log.Information("Parry outcome: Defender={DefenderName}, Outcome={Outcome}, ParryRoll={Parry}, AccuracyRoll={Accuracy}",
            defender.Name, result.Outcome, parryPool, attackAccuracy);

        // 3. Check for Riposte
        if (CanRiposte(defender, result.Outcome))
        {
            _log.Information("Riposte triggered: Defender={DefenderName}, Outcome={Outcome}",
                defender.Name, result.Outcome);

            result.RiposteTriggered = true;
            result.Riposte = ExecuteRiposte(defender, attacker);
        }

        // 4. Update statistics
        _repository.UpdateParryStatistics(result);

        // 5. Record combat log
        if (combatInstanceId > 0)
        {
            _repository.RecordParryAttempt(new ParryAttempt
            {
                CombatInstanceID = combatInstanceId,
                DefenderID = result.DefenderID,
                AttackerID = result.AttackerID,
                AttackAbility = attackAbility,
                ParryPoolRoll = parryPool,
                AttackerAccuracyRoll = attackAccuracy,
                Outcome = result.Outcome,
                RiposteTriggered = result.RiposteTriggered,
                RiposteDamage = result.Riposte?.DamageDealt ?? 0,
                Timestamp = DateTime.Now
            });
        }

        // 6. Apply trauma economy effects
        result.StressChange = ApplyTraumaEconomyEffects(defender, result);

        return result;
    }

    /// <summary>
    /// Calculate the Parry Pool for a character
    /// v2.0 canonical: FINESSE + Weapon Skill + Bonus Dice
    /// </summary>
    public int CalculateParryPool(PlayerCharacter character)
    {
        // Base: FINESSE + Weapon Skill
        int finesse = character.Attributes.Finesse;
        int weaponSkill = GetWeaponSkill(character);
        int basePool = finesse + weaponSkill;

        // Get bonus dice from specializations/equipment
        int bonusDice = GetParryBonusDice(character);

        // Roll bonus dice (d10s)
        int bonusRoll = 0;
        if (bonusDice > 0)
        {
            bonusRoll = _diceService.Roll(bonusDice, 10);
            _log.Debug("Parry bonus dice rolled: BonusDice={BonusDice}, BonusRoll={BonusRoll}",
                bonusDice, bonusRoll);
        }

        int totalPool = basePool + bonusRoll;

        _log.Debug("Parry Pool breakdown: FINESSE={Finesse}, WeaponSkill={Weapon}, BonusDice={Bonus}x d10 ({BonusRoll}), Total={Total}",
            finesse, weaponSkill, bonusDice, bonusRoll, totalPool);

        return totalPool;
    }

    /// <summary>
    /// Determine the parry outcome based on parry roll vs accuracy
    /// v2.0 canonical thresholds:
    /// - Failed: Parry < Accuracy
    /// - Standard: Parry = Accuracy
    /// - Superior: Parry > Accuracy (by 1-4)
    /// - Critical: Parry > Accuracy (by 5+)
    /// </summary>
    public ParryOutcome DetermineParryOutcome(int parryRoll, int accuracyRoll)
    {
        int difference = parryRoll - accuracyRoll;

        if (difference < 0)
            return ParryOutcome.Failed;
        else if (difference == 0)
            return ParryOutcome.Standard;
        else if (difference >= CriticalParryThreshold)
            return ParryOutcome.Critical;
        else
            return ParryOutcome.Superior;
    }

    /// <summary>
    /// Check if character can riposte based on parry outcome
    /// v2.0 canonical:
    /// - Critical Parry: ALL characters can riposte
    /// - Superior Parry: Only Hólmgangr with Reactive Parry can riposte
    /// </summary>
    public bool CanRiposte(PlayerCharacter character, ParryOutcome outcome)
    {
        // Critical parries always trigger riposte for all characters
        if (outcome == ParryOutcome.Critical)
        {
            _log.Debug("Critical Parry: Riposte available for all characters");
            return true;
        }

        // Superior parries only trigger for characters with Superior Riposte bonus
        if (outcome == ParryOutcome.Superior && HasSuperiorRiposte(character))
        {
            _log.Debug("Superior Parry: Riposte available for character with Superior Riposte bonus");
            return true;
        }

        return false;
    }

    #endregion

    #region Riposte Execution

    /// <summary>
    /// Execute a riposte counter-attack
    /// v2.0 canonical: Free basic melee attack, cannot be parried
    /// </summary>
    public RiposteResult ExecuteRiposte(PlayerCharacter attacker, Enemy target)
    {
        _log.Information("Riposte execution: Attacker={AttackerName}, Target={TargetName}",
            attacker.Name, target.Name);

        var result = new RiposteResult();

        // Riposte is a free basic melee attack (v2.0 canonical)
        // Hit Roll: FINESSE + Weapon Skill + d10
        int finesse = attacker.Attributes.Finesse;
        int weaponSkill = GetWeaponSkill(attacker);
        int rollBonus = _diceService.Roll(1, 10);
        int attackRoll = finesse + weaponSkill + rollBonus;

        int defenseScore = target.Defense;

        result.AttackRoll = attackRoll;
        result.DefenseScore = defenseScore;

        _log.Debug("Riposte attack roll: FINESSE={Finesse}, WeaponSkill={Weapon}, Roll={Roll}, Total={Total} vs Defense={Defense}",
            finesse, weaponSkill, rollBonus, attackRoll, defenseScore);

        // Check if hit
        if (attackRoll >= defenseScore)
        {
            result.Hit = true;

            // Calculate damage: Weapon Damage + MIGHT modifier
            int weaponDamage = GetWeaponDamage(attacker);
            int mightBonus = attacker.Attributes.Might;
            int totalDamage = weaponDamage + mightBonus;

            // Apply damage (riposte respects armor, as per standard melee attacks)
            int effectiveDamage = ApplyDamageToEnemy(target, totalDamage);
            result.DamageDealt = effectiveDamage;

            // Check if target killed
            result.KilledTarget = target.HP <= 0;

            _log.Information("RIPOSTE HIT! Attacker={AttackerName}, Target={TargetName}, Damage={Damage}, Killed={Killed}",
                attacker.Name, target.Name, effectiveDamage, result.KilledTarget);
        }
        else
        {
            result.Hit = false;
            _log.Information("Riposte missed: AttackRoll={AttackRoll} < Defense={Defense}",
                attackRoll, defenseScore);
        }

        return result;
    }

    /// <summary>
    /// Apply damage to an enemy (simplified version, respects armor)
    /// </summary>
    private int ApplyDamageToEnemy(Enemy target, int damage)
    {
        // Apply Soak (armor)
        int effectiveSoak = Math.Max(0, target.Soak - (target.CorrodedStacks * 2));
        int effectiveDamage = Math.Max(0, damage - effectiveSoak);

        // Apply damage
        target.HP -= effectiveDamage;

        _log.Debug("Damage applied: BaseDamage={Base}, Soak={Soak}, EffectiveDamage={Effective}, RemainingHP={HP}",
            damage, effectiveSoak, effectiveDamage, Math.Max(0, target.HP));

        return effectiveDamage;
    }

    #endregion

    #region Bonus Management

    /// <summary>
    /// Get the number of bonus dice for parry pool (from specializations/equipment)
    /// </summary>
    public int GetParryBonusDice(PlayerCharacter character)
    {
        var bonuses = _repository.GetParryBonuses(GetCharacterId(character));
        return bonuses.Sum(b => b.BonusDice);
    }

    /// <summary>
    /// Check if character has Superior Riposte ability (Hólmgangr Reactive Parry)
    /// </summary>
    public bool HasSuperiorRiposte(PlayerCharacter character)
    {
        var bonuses = _repository.GetParryBonuses(GetCharacterId(character));
        return bonuses.Any(b => b.AllowsSuperiorRiposte);
    }

    /// <summary>
    /// Get the number of parries allowed per round (usually 1, Hólmgangr Rank 3 = 2)
    /// </summary>
    public int GetParriesPerRound(PlayerCharacter character)
    {
        var bonuses = _repository.GetParryBonuses(GetCharacterId(character));

        if (bonuses.Count == 0)
            return BaseParryLimit;

        return bonuses.Max(b => b.ParriesPerRound);
    }

    /// <summary>
    /// Add a parry bonus for a character
    /// </summary>
    public int AddParryBonus(ParryBonus bonus)
    {
        return _repository.AddParryBonus(bonus);
    }

    /// <summary>
    /// Remove a parry bonus by ID
    /// </summary>
    public void RemoveParryBonus(int bonusId)
    {
        _repository.RemoveParryBonus(bonusId);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get parry statistics for a character
    /// </summary>
    public ParryStatistics GetParryStatistics(PlayerCharacter character)
    {
        return _repository.GetParryStatistics(GetCharacterId(character));
    }

    #endregion

    #region Specialization Bonus Application

    /// <summary>
    /// Apply Hólmgangr Reactive Parry bonus (v2.0 canonical + v0.21.4 extension)
    /// Rank 1: +1d10 to Parry Pool, Superior Riposte
    /// Rank 2 (Expert): +2d10 to Parry Pool
    /// Rank 3 (Mastery): Can parry twice per round
    /// </summary>
    public void ApplyHolmgangrReactiveParry(PlayerCharacter character, int rank = 1)
    {
        _log.Information("Applying Hólmgangr Reactive Parry: Character={CharacterName}, Rank={Rank}",
            character.Name, rank);

        int characterId = GetCharacterId(character);

        // Remove any existing Reactive Parry bonuses (upgrade case)
        var existingBonuses = _repository.GetParryBonuses(characterId)
            .Where(b => b.Source == "Reactive Parry")
            .ToList();

        foreach (var bonus in existingBonuses)
        {
            _repository.RemoveParryBonus(bonus.BonusID);
        }

        // Apply new bonus based on rank
        var newBonus = new ParryBonus
        {
            CharacterID = characterId,
            Source = "Reactive Parry",
            BonusDice = rank >= 2 ? 2 : 1, // Rank 2+ = +2d10, Rank 1 = +1d10
            AllowsSuperiorRiposte = true, // Hólmgangr always gets Superior Riposte
            ParriesPerRound = rank >= 3 ? 2 : 1 // Rank 3 = 2 parries per round
        };

        _repository.AddParryBonus(newBonus);

        // Update character's parries per turn
        character.ParriesRemainingThisTurn = newBonus.ParriesPerRound;

        _log.Information("Hólmgangr Reactive Parry applied: BonusDice={BonusDice}, SuperiorRiposte={SuperiorRiposte}, ParriesPerRound={ParriesPerRound}",
            newBonus.BonusDice, newBonus.AllowsSuperiorRiposte, newBonus.ParriesPerRound);
    }

    /// <summary>
    /// Apply Atgeir-wielder parry bonus (v0.21.4 extension)
    /// +1d10 to Parry Pool (reach weapon advantage)
    /// </summary>
    public void ApplyAtgeirWielderParryBonus(PlayerCharacter character)
    {
        _log.Information("Applying Atgeir-wielder Parry Bonus: Character={CharacterName}",
            character.Name);

        int characterId = GetCharacterId(character);

        // Check if already has this bonus
        var existingBonus = _repository.GetParryBonuses(characterId)
            .FirstOrDefault(b => b.Source == "Atgeir-wielder Reach");

        if (existingBonus != null)
        {
            _log.Debug("Atgeir-wielder bonus already applied");
            return;
        }

        // Apply bonus
        var newBonus = new ParryBonus
        {
            CharacterID = characterId,
            Source = "Atgeir-wielder Reach",
            BonusDice = 1, // +1d10 to Parry Pool
            AllowsSuperiorRiposte = false, // No Superior Riposte (Hólmgangr exclusive)
            ParriesPerRound = 1
        };

        _repository.AddParryBonus(newBonus);

        _log.Information("Atgeir-wielder parry bonus applied: +1d10 to Parry Pool");
    }

    /// <summary>
    /// Remove all specialization parry bonuses for a character
    /// (useful for respec or testing)
    /// </summary>
    public void RemoveAllParryBonuses(PlayerCharacter character)
    {
        int characterId = GetCharacterId(character);
        _repository.RemoveAllParryBonuses(characterId);

        _log.Information("All parry bonuses removed for character: {CharacterName}", character.Name);
    }

    /// <summary>
    /// Reset parry attempts for a new turn
    /// </summary>
    public void ResetParriesForNewTurn(PlayerCharacter character)
    {
        int parriesPerRound = GetParriesPerRound(character);
        character.ParriesRemainingThisTurn = parriesPerRound;
        character.ParryReactionPrepared = false;

        _log.Debug("Parries reset for new turn: Character={CharacterName}, ParriesAvailable={Parries}",
            character.Name, parriesPerRound);
    }

    /// <summary>
    /// Check if character can attempt a parry this turn
    /// </summary>
    public bool CanParryThisTurn(PlayerCharacter character)
    {
        return character.ParriesRemainingThisTurn > 0;
    }

    /// <summary>
    /// Consume a parry attempt
    /// </summary>
    public void ConsumeParryAttempt(PlayerCharacter character)
    {
        if (character.ParriesRemainingThisTurn > 0)
        {
            character.ParriesRemainingThisTurn--;
            _log.Debug("Parry attempt consumed: Character={CharacterName}, ParriesRemaining={Remaining}",
                character.Name, character.ParriesRemainingThisTurn);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get weapon skill for a character based on equipped weapon
    /// </summary>
    private int GetWeaponSkill(PlayerCharacter character)
    {
        // If character has a weapon equipped, use its skill level
        if (character.EquippedWeapon != null)
        {
            // Weapon skill could be based on character level or equipment stats
            // For now, assume a base skill of 3 (can be expanded later)
            return 3;
        }

        // Unarmed default
        return 1;
    }

    /// <summary>
    /// Get weapon damage for a character
    /// </summary>
    private int GetWeaponDamage(PlayerCharacter character)
    {
        if (character.EquippedWeapon != null)
        {
            // Roll weapon damage dice
            return _diceService.RollDamage(character.EquippedWeapon.DamageDice);
        }

        // Unarmed default (1d6)
        return _diceService.RollDamage(1);
    }

    /// <summary>
    /// Apply trauma economy effects based on parry result
    /// </summary>
    private int ApplyTraumaEconomyEffects(PlayerCharacter character, ParryResult result)
    {
        int stressChange = 0;

        if (result.Success)
        {
            // Successful parry reduces stress
            if (result.Outcome == ParryOutcome.Critical)
            {
                stressChange = CriticalParryStressRelief;
                _traumaService.AddStress(character, stressChange, "Critical Parry");
                _log.Debug("Critical Parry stress relief: {Stress}", stressChange);
            }
            else if (result.Outcome == ParryOutcome.Superior)
            {
                stressChange = SuperiorParryStressRelief;
                _traumaService.AddStress(character, stressChange, "Superior Parry");
                _log.Debug("Superior Parry stress relief: {Stress}", stressChange);
            }

            // Riposte kill provides additional stress relief
            if (result.Riposte?.KilledTarget == true)
            {
                int killRelief = RiposteKillStressRelief;
                _traumaService.AddStress(character, killRelief, "Riposte Kill");
                stressChange += killRelief;
                _log.Debug("Riposte kill stress relief: {Stress}", killRelief);
            }
            // Riposte miss generates minor stress
            else if (result.RiposteTriggered && result.Riposte?.Hit == false)
            {
                int missStress = RiposteMissStress;
                _traumaService.AddStress(character, missStress, "Riposte Missed");
                stressChange += missStress;
                _log.Debug("Riposte miss stress gain: {Stress}", missStress);
            }
        }
        else
        {
            // Failed parry generates stress
            stressChange = FailedParryStress;
            _traumaService.AddStress(character, stressChange, "Failed Parry");
            _log.Debug("Failed parry stress gain: {Stress}", stressChange);
        }

        return stressChange;
    }

    /// <summary>
    /// Get character ID for database operations
    /// </summary>
    private int GetCharacterId(PlayerCharacter character)
    {
        // Use character name hash as ID (same pattern as SpecializationService)
        return character.Name.GetHashCode();
    }

    /// <summary>
    /// Get enemy ID for database operations
    /// </summary>
    private int GetEnemyId(Enemy enemy)
    {
        // Use enemy ID hash for consistent targeting
        return enemy.Id.GetHashCode();
    }

    #endregion
}
