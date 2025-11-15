using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.26.3: Service for Skjaldmaer specialization-specific abilities and mechanics.
/// Handles Oath of the Protector, Guardian's Taunt, Bastion of Sanity, and other protection abilities.
/// The bastion of coherence - shields both body and mind.
/// </summary>
public class SkjaldmaerService
{
    private static readonly ILogger _log = Log.ForContext<SkjaldmaerService>();
    private readonly TraumaEconomyService _traumaService;
    private readonly DiceService _diceService;

    public SkjaldmaerService(string connectionString)
    {
        _traumaService = new TraumaEconomyService();
        _diceService = new DiceService();
        _log.Debug("SkjaldmaerService initialized");
    }

    #region Ability Execution

    /// <summary>
    /// Executes Oath of the Protector ability (single-target buff: +Soak and +Psychic Stress resistance).
    /// </summary>
    /// <param name="caster">The Skjaldmaer character</param>
    /// <param name="target">Target ally to protect</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Buff details and duration</returns>
    public (int soakBonus, int stressDiceBonus, int duration, bool cleansedDebuff, string message) ExecuteOathOfProtector(
        PlayerCharacter caster,
        PlayerCharacter target,
        int rank = 1)
    {
        _log.Information("Executing Oath of the Protector: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.CharacterID, rank);

        // Calculate bonuses based on rank
        var (soakBonus, stressDiceBonus, duration, staminaCost) = rank switch
        {
            1 => (2, 1, 2, 35),   // +2 Soak, +1 die, 2 turns, 35 Stamina
            2 => (3, 2, 2, 35),   // +3 Soak, +2 dice, 2 turns, 35 Stamina
            3 => (4, 2, 3, 35),   // +4 Soak, +2 dice, 3 turns, 35 Stamina
            _ => (2, 1, 2, 35)
        };

        // Rank 3: Cleanse 1 mental debuff
        bool cleansedDebuff = false;
        string cleansedEffect = "";
        if (rank >= 3)
        {
            // Check for mental debuffs (in priority order)
            var mentalDebuffs = new[] { "Fear", "Disoriented", "Charmed" };
            foreach (var debuff in mentalDebuffs)
            {
                // Simplified: assumes we have a way to check and remove status effects
                // In real implementation, this would use StatusEffectService
                cleansedDebuff = true;
                cleansedEffect = debuff;
                _log.Information("Oath of Protector cleansed mental debuff: TargetID={TargetId}, Debuff={Debuff}",
                    target.CharacterID, debuff);
                break; // Only cleanse 1
            }
        }

        _log.Information("Oath of Protector applied: CasterID={CasterId}, TargetID={TargetId}, SoakBonus={Soak}, StressDice={Dice}, Duration={Duration}",
            caster.CharacterID, target.CharacterID, soakBonus, stressDiceBonus, duration);

        string message = $"Oath of the Protector shields {target.Name} (+{soakBonus} Soak, +{stressDiceBonus} Stress resistance for {duration} turns)!";
        if (cleansedDebuff)
        {
            message += $" [{cleansedEffect} cleansed]";
        }

        return (soakBonus, stressDiceBonus, duration, cleansedDebuff, message);
    }

    /// <summary>
    /// Executes Guardian's Taunt ability (taunt enemies to attack caster; costs Psychic Stress).
    /// </summary>
    /// <param name="caster">The Skjaldmaer character</param>
    /// <param name="frontRowEnemies">List of front row enemies</param>
    /// <param name="backRowEnemies">List of back row enemies</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Taunt details and Psychic Stress cost</returns>
    public (int enemiesTaunted, int psychicStressCost, int tauntDuration, string message) ExecuteGuardiansTaunt(
        PlayerCharacter caster,
        List<Enemy> frontRowEnemies,
        List<Enemy> backRowEnemies,
        int rank = 1)
    {
        _log.Information("Executing Guardians Taunt: CasterID={CasterId}, Rank={Rank}",
            caster.CharacterID, rank);

        // Calculate effects based on rank
        var (psychicStressCost, tauntDuration, tauntAllRows, staminaCost) = rank switch
        {
            1 => (5, 2, false, 30),  // 5 Stress, 2 rounds, Front Row only, 30 Stamina
            2 => (3, 2, false, 30),  // 3 Stress, 2 rounds, Front Row only, 30 Stamina
            3 => (5, 2, true, 30),   // 5 Stress, 2 rounds, ALL enemies, 30 Stamina
            _ => (5, 2, false, 30)
        };

        // Count enemies taunted
        int enemiesTaunted = frontRowEnemies.Count;
        if (tauntAllRows)
        {
            enemiesTaunted += backRowEnemies.Count;
        }

        // Apply Psychic Stress cost to caster
        var (stressGained, traumaAcquired) = _traumaService.AddStress(
            caster,
            psychicStressCost,
            "Guardians Taunt (cost of drawing trauma)");

        _log.Information("Guardians Taunt executed: CasterID={CasterId}, Taunted={Count}, StressCost={Stress}, NewStress={NewStress}",
            caster.CharacterID, enemiesTaunted, psychicStressCost, caster.PsychicStress);

        string message = $"Guardian's Taunt draws {enemiesTaunted} enemies! (Skjaldmaer gains {stressGained} Psychic Stress)";

        if (traumaAcquired != null)
        {
            message += $" [WARNING: Breaking Point reached! Trauma: {traumaAcquired.Name}]";
            _log.Warning("Guardian's Taunt triggered Breaking Point: CasterID={CasterId}, Trauma={Trauma}",
                caster.CharacterID, traumaAcquired.Name);
        }

        return (enemiesTaunted, stressGained, tauntDuration, message);
    }

    /// <summary>
    /// Executes Shield Bash ability (melee attack with Stagger chance).
    /// </summary>
    /// <param name="caster">The Skjaldmaer character</param>
    /// <param name="target">Target enemy</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Damage dealt and Stagger status</returns>
    public (int damage, bool staggered, bool pushedToBackRow, string message) ExecuteShieldBash(
        PlayerCharacter caster,
        Enemy target,
        int rank = 1)
    {
        _log.Information("Executing Shield Bash: CasterID={CasterId}, TargetID={TargetId}, Rank={Rank}",
            caster.CharacterID, target.EnemyID, rank);

        // Calculate damage and effects based on rank
        var (damageDice, staggerChance, pushOnStagger, staminaCost) = rank switch
        {
            1 => (1, 50, false, 40),   // 1d8, 50% Stagger, no push, 40 Stamina
            2 => (2, 65, false, 40),   // 2d8, 65% Stagger, no push, 40 Stamina
            3 => (3, 75, true, 40),    // 3d8, 75% Stagger + push, 40 Stamina
            _ => (1, 50, false, 40)
        };

        // Calculate damage: XdY + MIGHT
        int damage = 0;
        for (int i = 0; i < damageDice; i++)
        {
            damage += _diceService.RollD8();
        }
        damage += caster.MIGHT;

        // Check for Stagger
        bool staggered = _diceService.RollPercentage() <= staggerChance;
        bool pushedToBackRow = staggered && pushOnStagger;

        _log.Information("Shield Bash complete: CasterID={CasterId}, Damage={Damage}, Staggered={Staggered}, Pushed={Pushed}",
            caster.CharacterID, damage, staggered, pushedToBackRow);

        string message = $"Shield Bash deals {damage} damage!";
        if (staggered)
        {
            message += " [Staggered]";
            if (pushedToBackRow)
            {
                message += " [Pushed to Back Row]";
            }
        }

        return (damage, staggered, pushedToBackRow, message);
    }

    /// <summary>
    /// Triggers Bastion of Sanity reaction (absorb ally's permanent Trauma).
    /// Once per combat, when an ally would gain permanent Trauma from Breaking Point,
    /// Skjaldmaer absorbs it—ally avoids Trauma, Skjaldmaer takes 40 Psychic Stress + 1 Corruption.
    /// </summary>
    /// <param name="skjaldmaer">The Skjaldmaer character</param>
    /// <param name="allyInCrisis">Ally who would gain Trauma</param>
    /// <param name="traumaThatWouldBeGained">The Trauma that would have been gained</param>
    /// <param name="alreadyTriggeredThisCombat">True if already triggered this combat</param>
    /// <returns>True if triggered successfully, Psychic Stress and Corruption gained</returns>
    public (bool triggered, int stressGained, int corruptionGained, string message) TriggerBastionOfSanity(
        PlayerCharacter skjaldmaer,
        PlayerCharacter allyInCrisis,
        Trauma traumaThatWouldBeGained,
        bool alreadyTriggeredThisCombat)
    {
        _log.Information("Attempting to trigger Bastion of Sanity: SkjaldmaerID={SkjaldmaerId}, AllyID={AllyId}, Trauma={Trauma}",
            skjaldmaer.CharacterID, allyInCrisis.CharacterID, traumaThatWouldBeGained.Name);

        // Check if already triggered this combat
        if (alreadyTriggeredThisCombat)
        {
            _log.Warning("Bastion of Sanity already triggered this combat: SkjaldmaerID={SkjaldmaerId}",
                skjaldmaer.CharacterID);
            return (false, 0, 0, "Bastion of Sanity already used this combat!");
        }

        // Constants for Bastion of Sanity
        const int stressCost = 40;
        const int corruptionCost = 1;

        // Ally avoids Trauma (would be handled by combat system)
        // Skjaldmaer takes the hit
        var (stressGained, _) = _traumaService.AddStress(
            skjaldmaer,
            stressCost,
            "Bastion of Sanity (Trauma Absorption)");

        var (corruptionGained, thresholdsCrossed) = _traumaService.AddCorruption(
            skjaldmaer,
            corruptionCost,
            "Bastion of Sanity (Trauma Absorption)");

        _log.Information(
            "BASTION OF SANITY TRIGGERED: SkjaldmaerID={SkjaldmaerId} absorbed Trauma '{Trauma}' for AllyID={AllyId}, taking {Stress} Stress + {Corruption} Corruption",
            skjaldmaer.CharacterID, traumaThatWouldBeGained.Name, allyInCrisis.CharacterID, stressGained, corruptionGained);

        string message = $"⚔️ BASTION OF SANITY! {skjaldmaer.Name} absorbs {allyInCrisis.Name}'s Trauma ({traumaThatWouldBeGained.Name})! " +
                        $"Takes {stressGained} Psychic Stress + {corruptionGained} Corruption.";

        if (thresholdsCrossed.Count > 0)
        {
            message += $" [Corruption thresholds crossed: {string.Join(", ", thresholdsCrossed)}]";
        }

        return (true, stressGained, corruptionGained, message);
    }

    #endregion

    #region Passive Bonuses

    /// <summary>
    /// Gets the Sanctified Resolve bonus dice vs. Fear and Psychic Stress.
    /// </summary>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Bonus dice to add to WILL Resolve Checks</returns>
    public int GetSanctifiedResolveBonus(int rank)
    {
        return rank switch
        {
            1 => 1,  // +1 die
            2 => 2,  // +2 dice
            3 => 3,  // +3 dice
            _ => 1
        };
    }

    /// <summary>
    /// Gets the ambient Psychic Stress reduction from Sanctified Resolve Rank 3.
    /// </summary>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Percentage reduction (0.0 to 1.0)</returns>
    public float GetSanctifiedResolveStressReduction(int rank)
    {
        return rank >= 3 ? 0.10f : 0f; // 10% reduction at Rank 3
    }

    /// <summary>
    /// Gets the Bastion of Sanity passive aura bonuses.
    /// PASSIVE AURA: While in Front Row, all allies in row gain +1 WILL and -10% ambient Psychic Stress gain.
    /// </summary>
    /// <returns>WILL bonus and Stress reduction percentage</returns>
    public (int willBonus, float stressReduction) GetBastionOfSanityAura()
    {
        return (1, 0.10f); // +1 WILL, -10% Stress gain
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if character has a specific ability (simplified version).
    /// In real implementation, this would check the character's Abilities list.
    /// </summary>
    private bool HasAbility(PlayerCharacter character, int abilityId)
    {
        // Simplified: In real implementation, check character.Abilities for abilityId
        return false;
    }

    #endregion
}
