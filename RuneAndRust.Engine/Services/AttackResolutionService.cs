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
/// <remarks>See: SPEC-ATTACK-001 for Attack Resolution System design.</remarks>
public class AttackResolutionService : IAttackResolutionService
{
    private readonly IDiceService _dice;
    private readonly IStatusEffectService _statusEffects;
    private readonly ITraumaService _traumaService;
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
    /// Damage bonuses for each attack type (added to weapon damage).
    /// Light attacks are faster but deal less damage, heavy attacks are slower but deal more.
    /// </summary>
    private static readonly Dictionary<AttackType, int> DamageBonuses = new()
    {
        { AttackType.Light, 0 },    // No bonus
        { AttackType.Standard, 2 }, // +2 damage
        { AttackType.Heavy, 4 }     // +4 damage
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
    /// <param name="statusEffects">The status effect service for modifier calculations.</param>
    /// <param name="traumaService">The trauma service for stress-based defense penalties.</param>
    /// <param name="logger">The logger for traceability.</param>
    public AttackResolutionService(
        IDiceService dice,
        IStatusEffectService statusEffects,
        ITraumaService traumaService,
        ILogger<AttackResolutionService> logger)
    {
        _dice = dice;
        _statusEffects = statusEffects;
        _traumaService = traumaService;
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
            // Roll weapon damage using attacker's equipped weapon die
            var weaponDamage = _dice.RollSingle(attacker.WeaponDamageDie, $"{attacker.Name} Weapon ({attacker.WeaponName})");

            // Raw damage = Might + Weapon Roll + Attack Type Bonus
            var damageBonus = DamageBonuses[attackType];
            rawDamage = attacker.GetAttribute(CharacterAttribute.Might) + weaponDamage + damageBonus;

            _logger.LogDebug(
                "Raw damage: Might {Might} + {WeaponName} d{Die} ({Roll}) + Bonus {Bonus} = {Raw}",
                attacker.GetAttribute(CharacterAttribute.Might), attacker.WeaponName,
                attacker.WeaponDamageDie, weaponDamage, damageBonus, rawDamage);

            // Apply outcome modifiers (glancing halves, critical doubles)
            rawDamage = ApplyDamageModifier(rawDamage, outcome);

            // Apply damage multiplier from status effects (e.g., Vulnerable)
            var damageMultiplier = _statusEffects.GetDamageMultiplier(defender);
            var modifiedDamage = (int)(rawDamage * damageMultiplier);

            if (damageMultiplier > 1.0f)
            {
                _logger.LogDebug(
                    "Damage amplified by status effect: {Raw} × {Multiplier} = {Modified}",
                    rawDamage, damageMultiplier, modifiedDamage);
            }

            // Calculate soak from defender's armor + status effects (e.g., Fortified)
            var baseSoak = defender.ArmorSoak;
            var soakModifier = _statusEffects.GetSoakModifier(defender);
            var totalSoak = baseSoak + soakModifier;

            if (soakModifier != 0)
            {
                _logger.LogDebug(
                    "Soak modified by status effect: Base {Base} + Modifier {Mod} = {Total}",
                    baseSoak, soakModifier, totalSoak);
            }

            // Final damage (minimum 1 on hit)
            finalDamage = Math.Max(1, modifiedDamage - totalSoak);

            if (modifiedDamage - totalSoak < 1)
            {
                _logger.LogDebug(
                    "Minimum damage enforced: {Calculated} -> 1",
                    modifiedDamage - totalSoak);
            }

            _logger.LogDebug(
                "Damage reduced by soak: {Raw} - {Soak} = {Final}",
                modifiedDamage, totalSoak, finalDamage);
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
        // Defense = 10 + FINESSE - StressPenalty
        // StressPenalty = Stress / 20 (max 5 at 100 stress)
        var finesse = defender.GetAttribute(CharacterAttribute.Finesse);
        var stressPenalty = _traumaService.GetDefensePenalty(defender.CurrentStress);

        var defense = 10 + finesse - stressPenalty;

        _logger.LogTrace(
            "Defense calculation for {Defender}: 10 + Finesse({Finesse}) - StressPenalty({Penalty}) = {Defense}",
            defender.Name, finesse, stressPenalty, defense);

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
