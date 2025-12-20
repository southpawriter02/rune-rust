using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Handles the mechanics of attack resolution including hit determination,
/// damage calculation, and outcome classification.
/// </summary>
public class AttackResolutionService : IAttackResolutionService
{
    private readonly IDiceService _dice;
    private readonly ILogger<AttackResolutionService> _logger;

    /// <summary>
    /// Stamina costs for each attack type.
    /// </summary>
    private static readonly Dictionary<AttackType, int> StaminaCosts = new()
    {
        { AttackType.Light, 15 },
        { AttackType.Standard, 25 },
        { AttackType.Heavy, 40 }
    };

    /// <summary>
    /// Weapon die sizes for each attack type.
    /// </summary>
    private static readonly Dictionary<AttackType, int> WeaponDice = new()
    {
        { AttackType.Light, 4 },    // d4
        { AttackType.Standard, 6 }, // d6
        { AttackType.Heavy, 8 }     // d8
    };

    /// <summary>
    /// Hit modifiers for each attack type (added to dice pool).
    /// </summary>
    private static readonly Dictionary<AttackType, int> HitModifiers = new()
    {
        { AttackType.Light, 1 },    // +1 to hit
        { AttackType.Standard, 0 }, // No modifier
        { AttackType.Heavy, -1 }    // -1 to hit
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackResolutionService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for rolling.</param>
    /// <param name="logger">The logger for traceability.</param>
    public AttackResolutionService(IDiceService dice, ILogger<AttackResolutionService> logger)
    {
        _dice = dice;
        _logger = logger;
    }

    /// <inheritdoc/>
    public AttackResult ResolveMeleeAttack(Combatant attacker, Combatant defender, AttackType attackType)
    {
        _logger.LogInformation(
            "Resolving {AttackType} attack: {Attacker} vs {Defender}",
            attackType, attacker.Name, defender.Name);

        // 1. Calculate attack pool = Might + hit modifier
        var basePool = attacker.GetAttribute(CharacterAttribute.Might);
        var hitModifier = HitModifiers[attackType];
        var attackPool = Math.Max(1, basePool + hitModifier); // Minimum 1 die

        _logger.LogDebug(
            "Attack pool: Base {Base} + Modifier {Mod} = {Pool}",
            basePool, hitModifier, attackPool);

        // 2. Roll the attack
        var roll = _dice.Roll(attackPool, $"{attacker.Name} Attack");

        // 3. Calculate defender's Defense Score and Success Threshold
        var defenseScore = CalculateDefenseScore(defender);
        var threshold = GetSuccessThreshold(defenseScore);

        _logger.LogDebug(
            "{Attacker} rolled {Successes} successes (Botches: {Botches}) vs Defense {Defense} (Threshold: {Threshold})",
            attacker.Name, roll.Successes, roll.Botches, defenseScore, threshold);

        // 4. Calculate net successes
        var netSuccesses = roll.Successes - threshold;

        // 5. Determine outcome
        var outcome = DetermineOutcome(roll.Successes, roll.Botches, netSuccesses);
        var isHit = outcome >= AttackOutcome.Glancing;

        _logger.LogDebug(
            "Net successes: {Net}, Outcome: {Outcome}, IsHit: {IsHit}",
            netSuccesses, outcome, isHit);

        // 6. Calculate damage (only if hit)
        int rawDamage = 0;
        int finalDamage = 0;

        if (isHit)
        {
            // Raw damage = Might + Weapon Die
            var weaponDie = WeaponDice[attackType];
            var weaponDamage = _dice.RollSingle(weaponDie, $"{attacker.Name} Weapon Damage");
            rawDamage = attacker.GetAttribute(CharacterAttribute.Might) + weaponDamage;

            _logger.LogDebug(
                "Raw damage: Might {Might} + d{Die} ({Roll}) = {Raw}",
                attacker.GetAttribute(CharacterAttribute.Might), weaponDie, weaponDamage, rawDamage);

            // Apply outcome modifiers
            rawDamage = ApplyDamageModifier(rawDamage, outcome);

            // Calculate soak (Sturdiness + future armor)
            var soak = defender.GetAttribute(CharacterAttribute.Sturdiness);

            // Final damage (minimum 1 on hit)
            finalDamage = Math.Max(1, rawDamage - soak);

            _logger.LogDebug(
                "Damage calc: Raw {Raw} - Soak {Soak} = Final {Final}",
                rawDamage, soak, finalDamage);
        }

        _logger.LogInformation(
            "Attack resolved: {Outcome} - {Attacker} dealt {Damage} damage to {Defender}",
            outcome, attacker.Name, finalDamage, defender.Name);

        return new AttackResult(outcome, netSuccesses, rawDamage, finalDamage, isHit);
    }

    /// <inheritdoc/>
    public int GetStaminaCost(AttackType attackType)
    {
        return StaminaCosts.GetValueOrDefault(attackType, 25);
    }

    /// <inheritdoc/>
    public bool CanAffordAttack(Combatant combatant, AttackType attackType)
    {
        var cost = GetStaminaCost(attackType);
        return combatant.CurrentStamina >= cost;
    }

    /// <inheritdoc/>
    public int CalculateDefenseScore(Combatant defender)
    {
        // Defense = 10 + FINESSE - Stress
        // Note: Stress is not yet implemented, defaults to 0
        var finesse = defender.GetAttribute(CharacterAttribute.Finesse);
        const int stress = 0; // TODO: Implement stress system in future version

        var defense = 10 + finesse - stress;

        _logger.LogTrace(
            "Defense calculation for {Defender}: 10 + Finesse({Finesse}) - Stress({Stress}) = {Defense}",
            defender.Name, finesse, stress, defense);

        return defense;
    }

    /// <inheritdoc/>
    public int GetSuccessThreshold(int defenseScore)
    {
        // Threshold = Defense / 5 (rounded down)
        return defenseScore / 5;
    }

    /// <summary>
    /// Determines the attack outcome based on roll results and net successes.
    /// </summary>
    /// <param name="successes">The number of successes rolled.</param>
    /// <param name="botches">The number of botches rolled.</param>
    /// <param name="netSuccesses">The net successes after subtracting threshold.</param>
    /// <returns>The attack outcome classification.</returns>
    private AttackOutcome DetermineOutcome(int successes, int botches, int netSuccesses)
    {
        // Fumble: 0 successes AND at least 1 botch
        if (successes == 0 && botches > 0)
        {
            return AttackOutcome.Fumble;
        }

        // Defender wins ties (net <= 0 is a miss)
        if (netSuccesses <= 0)
        {
            return AttackOutcome.Miss;
        }

        // Hit quality based on net successes
        return netSuccesses switch
        {
            1 or 2 => AttackOutcome.Glancing,
            3 or 4 => AttackOutcome.Solid,
            _ => AttackOutcome.Critical // 5+
        };
    }

    /// <summary>
    /// Applies damage modifiers based on the attack outcome.
    /// </summary>
    /// <param name="rawDamage">The base damage before modifiers.</param>
    /// <param name="outcome">The attack outcome.</param>
    /// <returns>The modified damage value.</returns>
    private int ApplyDamageModifier(int rawDamage, AttackOutcome outcome)
    {
        return outcome switch
        {
            AttackOutcome.Glancing => rawDamage / 2,
            AttackOutcome.Critical => rawDamage * 2,
            _ => rawDamage
        };
    }
}
