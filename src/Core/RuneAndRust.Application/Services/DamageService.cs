// ═══════════════════════════════════════════════════════════════════════════════
// DamageService.cs
// Implements the damage pipeline integrating Skjaldmær protective abilities:
// Unbreakable (flat reduction) and The Wall Lives (lethal protection).
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Applies damage with all Skjaldmær protective abilities integrated into the pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Damage reduction order:
/// 1. Unbreakable flat reduction (-3, minimum 1 damage)
/// 2. The Wall Lives lethal protection (cap damage to leave ≥1 HP)
/// </para>
/// </remarks>
public class DamageService : IDamageService
{
    private readonly ILogger<DamageService> _logger;
    private readonly SkjaldmaerTier3AbilityService _abilityService;

    /// <summary>
    /// Initializes a new instance of <see cref="DamageService"/>.
    /// </summary>
    /// <param name="logger">Logger for damage calculation audit trail.</param>
    /// <param name="abilityService">Tier 3 ability service for Unbreakable damage reduction.</param>
    public DamageService(
        ILogger<DamageService> logger,
        SkjaldmaerTier3AbilityService abilityService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
    }

    /// <inheritdoc />
    public int CalculateFinalDamage(
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities,
        TheWallLivesState? wallState,
        int currentHp,
        int incomingDamage)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var finalDamage = incomingDamage;

        // Step 1: Apply Unbreakable flat reduction
        var unbreakableReduction = _abilityService.GetDamageReduction(unlockedAbilities);
        if (unbreakableReduction > 0)
        {
            finalDamage = Math.Max(1, finalDamage - unbreakableReduction);
        }

        // Step 2: Apply The Wall Lives lethal protection
        finalDamage = CheckLethalDamageProtection(wallState, currentHp, finalDamage);

        _logger.LogInformation(
            "Damage calculation complete: incoming {IncomingDamage}, " +
            "Unbreakable reduction {Reduction}, final {FinalDamage}",
            incomingDamage, unbreakableReduction, finalDamage);

        return finalDamage;
    }

    /// <inheritdoc />
    public int CheckLethalDamageProtection(
        TheWallLivesState? wallState,
        int currentHp,
        int damageAmount)
    {
        if (wallState is null || !wallState.IsActive)
        {
            return damageAmount;
        }

        var protectedDamage = wallState.PreventLethalDamage(currentHp, damageAmount);

        if (protectedDamage != damageAmount)
        {
            _logger.LogInformation(
                "The Wall Lives prevented lethal damage: original {Original}, " +
                "capped {Capped}, HP preserved at {MinHp}",
                damageAmount, protectedDamage, TheWallLivesState.MinimumHp);
        }

        return protectedDamage;
    }
}
