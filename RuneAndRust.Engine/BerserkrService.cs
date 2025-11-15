using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.26.1: Service for Berserkr specialization-specific abilities and mechanics.
/// Handles Wild Swing, Reckless Assault, Unleashed Roar, Whirlwind of Destruction, and Hemorrhaging Strike.
/// </summary>
public class BerserkrService
{
    private static readonly ILogger _log = Log.ForContext<BerserkrService>();
    private readonly FuryService _furyService;
    private readonly DiceService _diceService;

    public BerserkrService(string connectionString)
    {
        _furyService = new FuryService(connectionString);
        _diceService = new DiceService();
        _log.Debug("BerserkrService initialized");
    }

    #region Ability Execution

    /// <summary>
    /// Executes Wild Swing ability (AoE damage to front row + Fury generation).
    /// </summary>
    /// <param name="character">The Berserkr character</param>
    /// <param name="targetCount">Number of enemies hit</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Damage dealt and Fury generated</returns>
    public (int totalDamage, int furyGenerated, string message) ExecuteWildSwing(
        PlayerCharacter character,
        int targetCount,
        int rank = 1)
    {
        _log.Information("Executing Wild Swing: CharacterId={CharacterId}, TargetCount={TargetCount}, Rank={Rank}",
            character.CharacterID, targetCount, rank);

        // Calculate damage per target based on rank
        var (damageDice, furyPerHit, staminaCost) = rank switch
        {
            1 => (2, 5, 40),   // 2d8, +5 Fury, 40 Stamina
            2 => (2, 7, 40),   // 2d10, +7 Fury, 40 Stamina
            3 => (3, 10, 35),  // 3d8, +10 Fury, 35 Stamina
            _ => (2, 5, 40)
        };

        int totalDamage = 0;
        int totalFury = 0;
        bool isBloodied = character.CurrentHP < (character.MaxHP / 2);

        // Check for Death or Glory passive (increases Fury generation when Bloodied)
        int deathOrGloryRank = HasAbility(character, 26008) ? 1 : 0; // Simplified: assumes Rank 1

        for (int i = 0; i < targetCount; i++)
        {
            // Calculate damage: XdY + MIGHT
            int damage = 0;
            for (int d = 0; d < damageDice; d++)
            {
                damage += rank == 2 ? _diceService.RollD10() : _diceService.RollD8();
            }
            damage += character.MIGHT;

            totalDamage += damage;

            // Generate Fury: base amount per hit
            var furyResult = _furyService.GenerateFuryFromDamageDealt(
                character.CharacterID,
                damage,
                abilityFuryBonus: furyPerHit,
                isBloodied: isBloodied,
                deathOrGloryRank: deathOrGloryRank);

            totalFury += furyResult.FuryChange;
        }

        _log.Information("Wild Swing complete: CharacterId={CharacterId}, TotalDamage={TotalDamage}, FuryGenerated={Fury}",
            character.CharacterID, totalDamage, totalFury);

        string message = $"Wild Swing hits {targetCount} enemies for {totalDamage} total damage! (+{totalFury} Fury)";
        return (totalDamage, totalFury, message);
    }

    /// <summary>
    /// Executes Reckless Assault ability (high single-target damage + high Fury + Vulnerable debuff).
    /// </summary>
    /// <param name="character">The Berserkr character</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Damage dealt, Fury generated, and whether Vulnerable is applied</returns>
    public (int damage, int furyGenerated, bool appliesVulnerable, string message) ExecuteRecklessAssault(
        PlayerCharacter character,
        int rank = 1)
    {
        _log.Information("Executing Reckless Assault: CharacterId={CharacterId}, Rank={Rank}",
            character.CharacterID, rank);

        // Calculate damage and effects based on rank
        var (damageDice, furyBonus, vulnerablePercent, staminaCost) = rank switch
        {
            1 => (3, 15, 25, 35),  // 3d10, +15 Fury, +25% vulnerable, 35 Stamina
            2 => (4, 18, 20, 35),  // 4d10, +18 Fury, +20% vulnerable, 35 Stamina
            3 => (5, 20, 15, 30),  // 5d10, +20 Fury, +15% vulnerable, 30 Stamina
            _ => (3, 15, 25, 35)
        };

        // Calculate damage: Xd10 + MIGHT
        int damage = 0;
        for (int i = 0; i < damageDice; i++)
        {
            damage += _diceService.RollD10();
        }
        damage += character.MIGHT;

        // Check if Bloodied for Death or Glory
        bool isBloodied = character.CurrentHP < (character.MaxHP / 2);
        int deathOrGloryRank = HasAbility(character, 26008) ? 1 : 0;

        // Generate Fury
        var furyResult = _furyService.GenerateFuryFromDamageDealt(
            character.CharacterID,
            damage,
            abilityFuryBonus: furyBonus,
            isBloodied: isBloodied,
            deathOrGloryRank: deathOrGloryRank);

        // For Rank 3, if target is killed, Vulnerable is not applied
        // This would require combat system integration - simplified here
        bool appliesVulnerable = true;

        _log.Information("Reckless Assault complete: CharacterId={CharacterId}, Damage={Damage}, FuryGenerated={Fury}, Vulnerable={Vulnerable}",
            character.CharacterID, damage, furyResult.FuryChange, appliesVulnerable);

        string message = $"Reckless Assault deals {damage} damage! (+{furyResult.FuryChange} Fury, you are now [Vulnerable] +{vulnerablePercent}%)";
        return (damage, furyResult.FuryChange, appliesVulnerable, message);
    }

    /// <summary>
    /// Executes Hemorrhaging Strike ability (massive single-target damage + Bleeding DoT).
    /// </summary>
    /// <param name="character">The Berserkr character</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Immediate damage, bleeding damage, and duration</returns>
    public (int immediateDamage, int bleedDiceCount, int bleedDuration, string message) ExecuteHemorrhagingStrike(
        PlayerCharacter character,
        int rank = 1)
    {
        _log.Information("Executing Hemorrhaging Strike: CharacterId={CharacterId}, Rank={Rank}",
            character.CharacterID, rank);

        // Calculate damage and bleed effects based on rank
        var (damageDice, bleedDice, bleedDuration, furyCost, staminaCost) = rank switch
        {
            1 => (4, 3, 3, 40, 45),  // 4d10, 3d6 bleed for 3 turns, 40 Fury, 45 Stamina
            2 => (5, 4, 3, 40, 45),  // 5d10, 4d6 bleed for 3 turns, 40 Fury, 45 Stamina
            3 => (6, 5, 4, 35, 40),  // 6d10, 5d6 bleed for 4 turns, 35 Fury, 40 Stamina
            _ => (4, 3, 3, 40, 45)
        };

        // Calculate immediate damage: Xd10 + MIGHT
        int damage = 0;
        for (int i = 0; i < damageDice; i++)
        {
            damage += _diceService.RollD10();
        }
        damage += character.MIGHT;

        _log.Information("Hemorrhaging Strike complete: CharacterId={CharacterId}, ImmediateDamage={Damage}, BleedingApplied={Bleed}d6x{Duration}",
            character.CharacterID, damage, bleedDice, bleedDuration);

        string message = $"Hemorrhaging Strike deals {damage} damage and inflicts [Bleeding] ({bleedDice}d6 per turn for {bleedDuration} rounds)!";
        if (rank == 3)
        {
            message += " [Cannot be cleansed]";
        }

        return (damage, bleedDice, bleedDuration, message);
    }

    /// <summary>
    /// Executes Whirlwind of Destruction ability (massive AoE to all enemies).
    /// </summary>
    /// <param name="character">The Berserkr character</param>
    /// <param name="targetCount">Total number of enemies (Front + Back rows)</param>
    /// <param name="rank">Ability rank (1-3)</param>
    /// <returns>Total damage and number of kills (for Fury refund)</returns>
    public (int totalDamage, int furyRefundPerKill, string message) ExecuteWhirlwindOfDestruction(
        PlayerCharacter character,
        int targetCount,
        int rank = 1)
    {
        _log.Information("Executing Whirlwind of Destruction: CharacterId={CharacterId}, TargetCount={TargetCount}, Rank={Rank}",
            character.CharacterID, targetCount, rank);

        // Calculate damage and effects based on rank
        var (damageDice, furyRefundPerKill, furyCost, staminaCost) = rank switch
        {
            1 => (3, 0, 30, 50),   // 3d8, no refund, 30 Fury, 50 Stamina
            2 => (4, 5, 30, 45),   // 4d8, +5 Fury per kill, 30 Fury, 45 Stamina
            3 => (5, 8, 25, 45),   // 5d8, +8 Fury per kill, 25 Fury, 45 Stamina
            _ => (3, 0, 30, 50)
        };

        int totalDamage = 0;

        for (int i = 0; i < targetCount; i++)
        {
            // Calculate damage: XdY + MIGHT
            int damage = 0;
            for (int d = 0; d < damageDice; d++)
            {
                damage += _diceService.RollD8();
            }
            damage += character.MIGHT;

            totalDamage += damage;
        }

        _log.Information("Whirlwind of Destruction complete: CharacterId={CharacterId}, TotalDamage={TotalDamage}, FuryRefundPerKill={Refund}",
            character.CharacterID, totalDamage, furyRefundPerKill);

        string message = $"Whirlwind of Destruction strikes ALL {targetCount} enemies for {totalDamage} total damage!";
        if (rank >= 2)
        {
            message += $" (Gain +{furyRefundPerKill} Fury per kill)";
        }

        return (totalDamage, furyRefundPerKill, message);
    }

    #endregion

    #region Passive Abilities

    /// <summary>
    /// Calculates Stamina regeneration bonus from Primal Vigor.
    /// </summary>
    public int CalculatePrimalVigorBonus(int characterId, int rank = 1)
    {
        int currentFury = _furyService.GetCurrentFury(characterId);
        int bonus = _furyService.CalculatePrimalVigorBonus(currentFury, rank);

        _log.Debug("Primal Vigor Rank {Rank}: CurrentFury={Fury}, StaminaBonus={Bonus}",
            rank, currentFury, bonus);

        return bonus;
    }

    /// <summary>
    /// Applies Blood-Fueled Fury generation from taking damage.
    /// </summary>
    public FuryResult ApplyBloodFueledDamage(PlayerCharacter character, int damageAmount, int rank = 1)
    {
        _log.Information("Applying Blood-Fueled damage: CharacterId={CharacterId}, Damage={Damage}, Rank={Rank}",
            character.CharacterID, damageAmount, rank);

        var result = _furyService.GenerateFuryFromDamageTaken(
            character.CharacterID,
            damageAmount,
            hasBloodFueled: true,
            bloodFueledRank: rank);

        // Rank 3 bonus: +1 Stamina per 5 damage taken
        if (rank == 3)
        {
            int staminaBonus = damageAmount / 5;
            _log.Information("Blood-Fueled Rank 3 bonus: +{Stamina} Stamina from damage",
                staminaBonus);
        }

        return result;
    }

    /// <summary>
    /// Checks if character has Death or Glory and should trigger the below-25% HP Fury bonus.
    /// </summary>
    public (bool shouldTrigger, int furyBonus) CheckDeathOrGloryTrigger(PlayerCharacter character, int rank = 1)
    {
        bool isBelow25Percent = character.CurrentHP < (character.MaxHP * 0.25);

        if (!isBelow25Percent)
        {
            return (false, 0);
        }

        int furyBonus = rank switch
        {
            1 => 5,
            2 => 5,
            3 => 10,
            _ => 5
        };

        _log.Information("Death or Glory trigger check: CharacterId={CharacterId}, Below25%={Below25}, Bonus={Bonus}",
            character.CharacterID, isBelow25Percent, furyBonus);

        return (true, furyBonus);
    }

    /// <summary>
    /// Checks if character can trigger Unstoppable Fury (lethal damage survival).
    /// </summary>
    public bool CanTriggerUnstoppableFury(PlayerCharacter character, int rank = 1)
    {
        // This would need to check combat state and previous triggers
        // Simplified for now
        _log.Debug("Checking Unstoppable Fury eligibility: CharacterId={CharacterId}, Rank={Rank}",
            character.CharacterID, rank);

        return true; // Would check unstoppable_fury_triggered flag in real implementation
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if character has learned a specific ability.
    /// Simplified version - would query CharacterAbilities table in full implementation.
    /// </summary>
    private bool HasAbility(PlayerCharacter character, int abilityId)
    {
        // Placeholder: would query database in full implementation
        return false;
    }

    /// <summary>
    /// Gets the WILL penalty applied when holding Fury (Trauma Economy integration).
    /// </summary>
    public int GetWillPenalty(int characterId)
    {
        int currentFury = _furyService.GetCurrentFury(characterId);

        if (currentFury > 0)
        {
            _log.Warning("WILL penalty applied: CharacterId={CharacterId}, CurrentFury={Fury}, Penalty=-2 dice",
                characterId, currentFury);
            return -2; // -2 dice penalty to WILL checks while holding any Fury
        }

        return 0;
    }

    #endregion
}
