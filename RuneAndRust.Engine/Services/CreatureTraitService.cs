using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements creature trait generation and runtime effect processing.
/// Traits are procedurally assigned to Elite/Champion enemies to create diverse combat encounters.
/// </summary>
/// <remarks>See: SPEC-TRAIT-001 for Creature Trait System design.</remarks>
public class CreatureTraitService : ICreatureTraitService
{
    private readonly IDiceService _dice;
    private readonly ILogger<CreatureTraitService> _logger;

    /// <summary>
    /// Regeneration percentage per turn (10%).
    /// </summary>
    private const float RegenerationPercent = 0.10f;

    /// <summary>
    /// Vampiric healing percentage (25% of damage dealt).
    /// </summary>
    private const float VampiricPercent = 0.25f;

    /// <summary>
    /// Thorns reflection percentage (25% of damage received).
    /// </summary>
    private const float ThornsPercent = 0.25f;

    /// <summary>
    /// Explosive death damage (flat value for v0.2.2c).
    /// </summary>
    private const int ExplosiveDamage = 15;

    public CreatureTraitService(IDiceService dice, ILogger<CreatureTraitService> logger)
    {
        _dice = dice;
        _logger = logger;
        _logger.LogDebug("CreatureTraitService initialized");
    }

    /// <inheritdoc/>
    public void EnhanceEnemy(Enemy enemy)
    {
        _logger.LogDebug("EnhanceEnemy called for {Name} with Tags: [{Tags}]",
            enemy.Name, string.Join(", ", enemy.Tags));

        // Only enhance Elite or higher tier enemies
        if (!IsEliteTier(enemy))
        {
            _logger.LogDebug("Enemy {Name} is not Elite tier, skipping enhancement", enemy.Name);
            return;
        }

        int traitCount = GetTraitCount(enemy);
        var availableTraits = GetAvailableTraits(enemy);

        _logger.LogInformation(
            "Enhancing {Name} (Tier: {Tier}) with {Count} trait(s). Available traits: {TraitCount}",
            enemy.Name,
            GetTierName(enemy),
            traitCount,
            availableTraits.Count);

        for (int i = 0; i < traitCount && availableTraits.Count > 0; i++)
        {
            var rollIndex = _dice.RollSingle(availableTraits.Count, "Trait Selection") - 1;
            var trait = availableTraits[rollIndex];

            _logger.LogDebug("Selected trait {Trait} (roll index: {Index})", trait, rollIndex);
            ApplyTrait(enemy, trait);
            availableTraits.RemoveAt(rollIndex); // No duplicates
        }

        _logger.LogInformation(
            "Enhancement complete for {Name}. Final ActiveTraits: [{Traits}]",
            enemy.Name, string.Join(", ", enemy.ActiveTraits));
    }

    /// <inheritdoc/>
    public int ProcessTraitTurnStart(Combatant combatant)
    {
        _logger.LogDebug("ProcessTraitTurnStart called for {Name}", combatant.Name);

        if (!HasTrait(combatant, CreatureTraitType.Regenerating))
        {
            _logger.LogDebug("{Name} does not have Regenerating trait", combatant.Name);
            return 0;
        }

        var healAmount = (int)(combatant.MaxHp * RegenerationPercent);
        var previousHp = combatant.CurrentHp;
        combatant.CurrentHp = Math.Min(combatant.MaxHp, combatant.CurrentHp + healAmount);
        var actualHeal = combatant.CurrentHp - previousHp;

        _logger.LogInformation(
            "[Trait] {Name} regenerates {Amount} HP (attempted {Attempted}). HP: {Current}/{Max}",
            combatant.Name, actualHeal, healAmount, combatant.CurrentHp, combatant.MaxHp);

        return actualHeal;
    }

    /// <inheritdoc/>
    public List<(Combatant Target, int Damage)> ProcessTraitOnDeath(
        Combatant victim,
        IEnumerable<Combatant> allCombatants)
    {
        _logger.LogDebug("ProcessTraitOnDeath called for {Name}", victim.Name);

        var results = new List<(Combatant, int)>();

        if (!HasTrait(victim, CreatureTraitType.Explosive))
        {
            _logger.LogDebug("{Name} does not have Explosive trait", victim.Name);
            return results;
        }

        _logger.LogWarning(
            "[Trait] {Name} EXPLODES on death! Dealing {Damage} damage to all combatants.",
            victim.Name, ExplosiveDamage);

        // Damage all OTHER combatants (not the victim)
        foreach (var target in allCombatants.Where(c => c.Id != victim.Id))
        {
            results.Add((target, ExplosiveDamage));
            _logger.LogDebug("Explosion will damage {Target} for {Damage}", target.Name, ExplosiveDamage);
        }

        _logger.LogInformation(
            "[Trait] Explosive death: {Count} targets will receive {Damage} damage each",
            results.Count, ExplosiveDamage);

        return results;
    }

    /// <inheritdoc/>
    public int ProcessTraitOnDamageDealt(Combatant attacker, int damageDealt)
    {
        _logger.LogDebug("ProcessTraitOnDamageDealt called for {Name}, damage: {Damage}",
            attacker.Name, damageDealt);

        if (!HasTrait(attacker, CreatureTraitType.Vampiric))
        {
            _logger.LogDebug("{Name} does not have Vampiric trait", attacker.Name);
            return 0;
        }

        var healAmount = (int)(damageDealt * VampiricPercent);
        var previousHp = attacker.CurrentHp;
        attacker.CurrentHp = Math.Min(attacker.MaxHp, attacker.CurrentHp + healAmount);
        var actualHeal = attacker.CurrentHp - previousHp;

        _logger.LogInformation(
            "[Trait] {Name} drains {Amount} HP from the wound (attempted {Attempted}). HP: {Current}/{Max}",
            attacker.Name, actualHeal, healAmount, attacker.CurrentHp, attacker.MaxHp);

        return actualHeal;
    }

    /// <inheritdoc/>
    public int ProcessTraitOnDamageReceived(Combatant defender, Combatant attacker, int damageReceived)
    {
        _logger.LogDebug("ProcessTraitOnDamageReceived called: {Defender} took {Damage} from {Attacker}",
            defender.Name, damageReceived, attacker.Name);

        if (!HasTrait(defender, CreatureTraitType.Thorns))
        {
            _logger.LogDebug("{Name} does not have Thorns trait", defender.Name);
            return 0;
        }

        var thornsDamage = (int)(damageReceived * ThornsPercent);

        _logger.LogInformation(
            "[Trait] {Defender}'s thorns reflect {Damage} damage to {Attacker}!",
            defender.Name, thornsDamage, attacker.Name);

        return thornsDamage;
    }

    /// <inheritdoc/>
    public bool IsImmuneToEffect(Combatant combatant, StatusEffectType effectType)
    {
        _logger.LogDebug("IsImmuneToEffect called for {Name}, effect: {Effect}",
            combatant.Name, effectType);

        // Relentless grants stun immunity
        if (effectType == StatusEffectType.Stunned && HasTrait(combatant, CreatureTraitType.Relentless))
        {
            _logger.LogDebug("[Trait] {Name} is immune to {Effect} (Relentless trait)", combatant.Name, effectType);
            return true;
        }

        return false;
    }

    #region Private Helper Methods

    private void ApplyTrait(Enemy enemy, CreatureTraitType trait)
    {
        _logger.LogDebug("Applying trait {Trait} to {Name}", trait, enemy.Name);

        enemy.ActiveTraits.Add(trait);
        var originalName = enemy.Name;

        switch (trait)
        {
            case CreatureTraitType.Armored:
                var previousSoak = enemy.ArmorSoak;
                enemy.ArmorSoak += 3;
                enemy.Name = $"Armored {enemy.Name}";
                _logger.LogDebug("Applied Armored: +3 Soak ({Previous} → {New})",
                    previousSoak, enemy.ArmorSoak);
                break;

            case CreatureTraitType.Relentless:
                var previousHp = enemy.MaxHp;
                enemy.MaxHp = (int)(enemy.MaxHp * 1.5f);
                enemy.CurrentHp = enemy.MaxHp;
                enemy.Tags.Add("ImmuneToStun");
                enemy.Name = $"Relentless {enemy.Name}";
                _logger.LogDebug("Applied Relentless: +50% HP ({Previous} → {New}), added ImmuneToStun tag",
                    previousHp, enemy.MaxHp);
                break;

            case CreatureTraitType.Berserker:
                // Damage modifier handled at runtime in AttackResolutionService (future)
                enemy.Name = $"Berserk {enemy.Name}";
                _logger.LogDebug("Applied Berserker: +25% damage dealt/received (runtime modifier)");
                break;

            case CreatureTraitType.Vampiric:
                enemy.Name = $"Vampiric {enemy.Name}";
                _logger.LogDebug("Applied Vampiric: 25% lifesteal on damage dealt");
                break;

            case CreatureTraitType.Corrosive:
                enemy.Name = $"Corrosive {enemy.Name}";
                _logger.LogDebug("Applied Corrosive: applies Vulnerable on hit (future)");
                break;

            case CreatureTraitType.Explosive:
                enemy.Name = $"Volatile {enemy.Name}";
                _logger.LogDebug("Applied Explosive: {Damage} AoE damage on death", ExplosiveDamage);
                break;

            case CreatureTraitType.Regenerating:
                enemy.Name = $"Regenerating {enemy.Name}";
                _logger.LogDebug("Applied Regenerating: {Percent}% MaxHP heal per turn",
                    RegenerationPercent * 100);
                break;

            case CreatureTraitType.Thorns:
                enemy.Name = $"Thorned {enemy.Name}";
                _logger.LogDebug("Applied Thorns: {Percent}% damage reflection",
                    ThornsPercent * 100);
                break;
        }

        _logger.LogInformation(
            "Trait {Trait} applied to {OriginalName} → {NewName}. ActiveTraits: [{Traits}]",
            trait, originalName, enemy.Name, string.Join(", ", enemy.ActiveTraits));
    }

    private static bool IsEliteTier(Enemy enemy)
    {
        // Check if enemy was created with Elite or higher tier via Tags
        return enemy.Tags.Contains("Elite") ||
               enemy.Tags.Contains("Champion") ||
               enemy.Tags.Contains("Boss");
    }

    private static string GetTierName(Enemy enemy)
    {
        if (enemy.Tags.Contains("Boss")) return "Boss";
        if (enemy.Tags.Contains("Champion")) return "Champion";
        if (enemy.Tags.Contains("Elite")) return "Elite";
        return "Standard";
    }

    private static int GetTraitCount(Enemy enemy)
    {
        if (enemy.Tags.Contains("Boss")) return 3;
        if (enemy.Tags.Contains("Champion")) return 2;
        return 1; // Elite default
    }

    private List<CreatureTraitType> GetAvailableTraits(Enemy enemy)
    {
        var traits = Enum.GetValues<CreatureTraitType>().ToList();

        // Filter incompatible traits based on enemy tags
        if (enemy.Tags.Contains("Mechanical") || enemy.Tags.Contains("Construct"))
        {
            // Mechanical enemies can't be vampiric (no blood to drain)
            traits.Remove(CreatureTraitType.Vampiric);
            _logger.LogDebug("Removed Vampiric from available traits (Mechanical/Construct enemy)");
        }

        return traits;
    }

    private static bool HasTrait(Combatant combatant, CreatureTraitType trait)
    {
        // Check combatant's ActiveTraits directly (copied from enemy source)
        return combatant.ActiveTraits.Contains(trait);
    }

    #endregion
}
