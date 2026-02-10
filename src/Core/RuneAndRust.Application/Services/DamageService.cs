using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for applying damage with protective ability integration.
/// Handles Unbreakable reduction and The Wall Lives protection.
/// </summary>
/// <remarks>
/// <para>Damage pipeline order:</para>
/// <list type="number">
/// <item>Apply Unbreakable reduction (-3, minimum 1)</item>
/// <item>Check The Wall Lives protection (cap damage to preserve 1 HP)</item>
/// <item>Apply final damage to character HP</item>
/// </list>
/// </remarks>
public class DamageService : IDamageService
{
    private readonly ISkjaldmaerAbilityService _abilityService;
    private readonly ILogger<DamageService> _logger;

    public DamageService(
        ISkjaldmaerAbilityService abilityService,
        ILogger<DamageService> logger)
    {
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public int ApplyDamage(Player target, int incomingDamage)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (incomingDamage <= 0)
            return 0;

        // Calculate final damage after all reductions
        var finalDamage = CalculateFinalDamage(target, incomingDamage);

        // Apply damage via Player's TakeDamage method
        target.TakeDamage(finalDamage);

        _logger.LogInformation(
            "Damage applied: {Target} ({TargetId}) took {FinalDamage} damage " +
            "(incoming {IncomingDamage}). HP now: {CurrentHP}/{MaxHP}",
            target.Name, target.Id, finalDamage, incomingDamage,
            target.Health, target.Stats.MaxHealth);

        return finalDamage;
    }

    /// <inheritdoc />
    public int CalculateFinalDamage(Player target, int incomingDamage)
    {
        ArgumentNullException.ThrowIfNull(target);

        var effectiveDamage = incomingDamage;

        // Step 1: Apply Unbreakable reduction
        var unbreakableReduction = _abilityService.GetDamageReduction(target);
        if (unbreakableReduction > 0)
        {
            effectiveDamage = Math.Max(1, effectiveDamage - unbreakableReduction);

            _logger.LogDebug(
                "Unbreakable damage reduction applied to {Target} ({TargetId}): " +
                "reduced {IncomingDamage} by {Reduction} to {EffectiveDamage}",
                target.Name, target.Id, incomingDamage, unbreakableReduction, effectiveDamage);
        }

        // Step 2: Apply The Wall Lives protection
        effectiveDamage = CheckLethalDamageProtection(target, effectiveDamage);

        return effectiveDamage;
    }

    /// <inheritdoc />
    public int CheckLethalDamageProtection(Player player, int damageAmount)
    {
        if (player?.TheWallLivesState?.IsActive != true)
            return damageAmount;

        var protectedDamage = player.TheWallLivesState.PreventLethalDamage(
            player.Health,
            damageAmount);

        if (protectedDamage < damageAmount)
        {
            _logger.LogInformation(
                "The Wall Lives protection activated for {Player} ({PlayerId}): " +
                "prevented lethal damage (capped {Original} to {Capped})",
                player.Name, player.Id, damageAmount, protectedDamage);
        }

        return protectedDamage;
    }
}
