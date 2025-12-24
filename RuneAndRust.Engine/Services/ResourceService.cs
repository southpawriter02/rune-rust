using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements resource management during combat.
/// Handles spending, regeneration, and validation of resources (Stamina, Aether).
/// </summary>
/// <remarks>See: SPEC-RESOURCE-001 for Resource Management System design.</remarks>
public class ResourceService : IResourceService
{
    private readonly ILogger<ResourceService> _logger;

    /// <summary>
    /// Base stamina regenerated per turn.
    /// </summary>
    private const int BaseStaminaRegen = 5;

    /// <summary>
    /// HP cost ratio for Overcast (2 HP per 1 missing Aether).
    /// </summary>
    private const int OvercastHpRatio = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public ResourceService(ILogger<ResourceService> logger)
    {
        _logger = logger;
        _logger.LogInformation("ResourceService initialized");
    }

    /// <inheritdoc/>
    public bool CanAfford(Combatant combatant, ResourceType type, int cost)
    {
        if (cost <= 0)
        {
            _logger.LogTrace(
                "[Resource] {Name} checked cost {Cost} for {Type}: free action",
                combatant.Name, cost, type);
            return true;
        }

        _logger.LogTrace(
            "[Resource] {Name} checking affordability: {Type} cost {Cost}",
            combatant.Name, type, cost);

        return type switch
        {
            ResourceType.Health => combatant.CurrentHp >= cost,
            ResourceType.Stamina => combatant.CurrentStamina >= cost,
            ResourceType.Aether => CanAffordAether(combatant, cost),
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool Deduct(Combatant combatant, ResourceType type, int cost)
    {
        if (cost <= 0)
        {
            _logger.LogTrace(
                "[Resource] {Name} deducting {Cost} {Type}: no cost, success",
                combatant.Name, cost, type);
            return true;
        }

        if (!CanAfford(combatant, type, cost))
        {
            _logger.LogWarning(
                "[Resource] {Name} cannot afford {Cost} {Type}. Current: {Current}",
                combatant.Name, cost, type, GetCurrent(combatant, type));
            return false;
        }

        _logger.LogDebug(
            "[Resource] {Name} deducting {Cost} {Type}. Before: {Before}",
            combatant.Name, cost, type, GetCurrent(combatant, type));

        var success = type switch
        {
            ResourceType.Health => DeductHealth(combatant, cost),
            ResourceType.Stamina => DeductStamina(combatant, cost),
            ResourceType.Aether => DeductAether(combatant, cost),
            _ => false
        };

        if (success)
        {
            _logger.LogInformation(
                "[Resource] {Name} spent {Cost} {Type}. After: {After}",
                combatant.Name, cost, type, GetCurrent(combatant, type));

            // Sync back to character source if applicable
            SyncToSource(combatant, type);
        }

        return success;
    }

    /// <inheritdoc/>
    public int RegenerateStamina(Combatant combatant)
    {
        // Check for regeneration-blocking status effects
        if (HasStatusEffect(combatant, StatusEffectType.Stunned))
        {
            _logger.LogDebug(
                "[Resource] {Name} is Stunned - no stamina regeneration",
                combatant.Name);
            return 0;
        }

        // Calculate regeneration: Base 5 + (Finesse / 2)
        var finesse = combatant.GetAttribute(Core.Enums.Attribute.Finesse);
        var regenAmount = BaseStaminaRegen + (finesse / 2);

        // Clamp to max stamina
        var actualRegen = Math.Min(regenAmount, combatant.MaxStamina - combatant.CurrentStamina);

        if (actualRegen <= 0)
        {
            _logger.LogTrace(
                "[Resource] {Name} stamina at max ({Current}/{Max}), no regeneration",
                combatant.Name, combatant.CurrentStamina, combatant.MaxStamina);
            return 0;
        }

        combatant.CurrentStamina += actualRegen;

        _logger.LogInformation(
            "[Resource] {Name} regenerates {Amount} stamina (Finesse: {Finesse}). Stamina: {Current}/{Max}",
            combatant.Name, actualRegen, finesse, combatant.CurrentStamina, combatant.MaxStamina);

        // Sync back to source
        SyncToSource(combatant, ResourceType.Stamina);

        return actualRegen;
    }

    /// <inheritdoc/>
    public int GetCurrent(Combatant combatant, ResourceType type)
    {
        return type switch
        {
            ResourceType.Health => combatant.CurrentHp,
            ResourceType.Stamina => combatant.CurrentStamina,
            ResourceType.Aether => combatant.CurrentAp,
            _ => 0
        };
    }

    /// <inheritdoc/>
    public int GetMax(Combatant combatant, ResourceType type)
    {
        return type switch
        {
            ResourceType.Health => combatant.MaxHp,
            ResourceType.Stamina => combatant.MaxStamina,
            ResourceType.Aether => combatant.MaxAp,
            _ => 0
        };
    }

    /// <inheritdoc/>
    public bool IsMystic(Combatant combatant)
    {
        // Check if the combatant is a player with Mystic archetype
        if (combatant.CharacterSource != null)
        {
            return combatant.CharacterSource.Archetype == ArchetypeType.Mystic;
        }

        return false;
    }

    #region Private Methods

    /// <summary>
    /// Checks if a combatant can afford an Aether cost.
    /// Mystics can always afford via Overcast if they have sufficient HP.
    /// </summary>
    private bool CanAffordAether(Combatant combatant, int cost)
    {
        // Non-Mystics cannot use Aether abilities at all
        if (!IsMystic(combatant))
        {
            _logger.LogDebug(
                "[Resource] {Name} is not a Mystic - cannot use Aether abilities",
                combatant.Name);
            return false;
        }

        // If we have enough AP, we can afford it normally
        if (combatant.CurrentAp >= cost)
        {
            return true;
        }

        // Calculate Overcast HP cost: (cost - currentAp) * 2
        var apShortfall = cost - combatant.CurrentAp;
        var hpCost = apShortfall * OvercastHpRatio;

        // Mystic can Overcast if they have enough HP (must survive the cost)
        var canOvercast = combatant.CurrentHp > hpCost;

        _logger.LogDebug(
            "[Resource] {Name} Overcast check: Need {ApCost} AP, have {CurrentAp}. " +
            "Shortfall: {Shortfall}, HP cost: {HpCost}, Current HP: {CurrentHp}, Can Overcast: {CanOvercast}",
            combatant.Name, cost, combatant.CurrentAp, apShortfall, hpCost,
            combatant.CurrentHp, canOvercast);

        return canOvercast;
    }

    /// <summary>
    /// Deducts HP from a combatant.
    /// </summary>
    private bool DeductHealth(Combatant combatant, int cost)
    {
        combatant.CurrentHp -= cost;
        return true;
    }

    /// <summary>
    /// Deducts stamina from a combatant.
    /// </summary>
    private bool DeductStamina(Combatant combatant, int cost)
    {
        combatant.CurrentStamina -= cost;
        return true;
    }

    /// <summary>
    /// Deducts Aether from a combatant.
    /// If insufficient AP, Mystics Overcast by spending HP at 2:1 ratio.
    /// </summary>
    private bool DeductAether(Combatant combatant, int cost)
    {
        if (combatant.CurrentAp >= cost)
        {
            // Normal deduction
            combatant.CurrentAp -= cost;
            return true;
        }

        // Overcast: spend remaining AP, then convert HP
        var apSpent = combatant.CurrentAp;
        var apShortfall = cost - apSpent;
        var hpCost = apShortfall * OvercastHpRatio;

        combatant.CurrentAp = 0;
        combatant.CurrentHp -= hpCost;

        _logger.LogWarning(
            "[Resource] {Name} OVERCASTS! Spent {ApSpent} AP + {HpCost} HP for {TotalCost} AP ability. " +
            "HP: {CurrentHp}/{MaxHp}",
            combatant.Name, apSpent, hpCost, cost, combatant.CurrentHp, combatant.MaxHp);

        // Sync HP back to source since Overcast affects HP
        SyncToSource(combatant, ResourceType.Health);

        return true;
    }

    /// <summary>
    /// Syncs the resource value back to the character source.
    /// </summary>
    private void SyncToSource(Combatant combatant, ResourceType type)
    {
        if (combatant.CharacterSource == null) return;

        switch (type)
        {
            case ResourceType.Health:
                combatant.CharacterSource.CurrentHP = combatant.CurrentHp;
                break;
            case ResourceType.Stamina:
                combatant.CharacterSource.CurrentStamina = combatant.CurrentStamina;
                break;
            case ResourceType.Aether:
                combatant.CharacterSource.CurrentAp = combatant.CurrentAp;
                break;
        }

        _logger.LogTrace(
            "[Resource] Synced {Type} to CharacterSource: {Current}",
            type, GetCurrent(combatant, type));
    }

    /// <summary>
    /// Checks if a combatant has a specific status effect.
    /// </summary>
    private static bool HasStatusEffect(Combatant combatant, StatusEffectType effectType)
    {
        return combatant.StatusEffects.Any(e => e.Type == effectType);
    }

    #endregion
}
