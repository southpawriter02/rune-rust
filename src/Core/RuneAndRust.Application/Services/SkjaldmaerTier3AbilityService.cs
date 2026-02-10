// ═══════════════════════════════════════════════════════════════════════════════
// SkjaldmaerTier3AbilityService.cs
// Implements Tier 3 and Capstone ability operations for the Skjaldmær:
// Unbreakable, Guardian's Sacrifice, and The Wall Lives.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implements Tier 3 and Capstone Skjaldmær abilities: Unbreakable, Guardian's Sacrifice,
/// and The Wall Lives.
/// </summary>
/// <remarks>
/// <para>
/// This service handles the Tier 3 and Capstone ability operations for the Skjaldmær.
/// It operates on immutable value objects and returns new instances for all state transitions.
/// </para>
/// <para>
/// <b>Tier 3 requires 16 PP invested.</b> Each Tier 3 ability costs 5 PP to unlock.
/// </para>
/// <para>
/// <b>Capstone requires 24 PP invested.</b> The Wall Lives costs 6 PP to unlock.
/// </para>
/// </remarks>
public class SkjaldmaerTier3AbilityService
{
    private readonly ILogger<SkjaldmaerTier3AbilityService> _logger;

    /// <summary>Flat damage reduction from Unbreakable passive.</summary>
    public const int UnbreakableReduction = 3;

    /// <summary>
    /// Initializes a new instance of <see cref="SkjaldmaerTier3AbilityService"/>.
    /// </summary>
    /// <param name="logger">Logger for ability audit trail.</param>
    public SkjaldmaerTier3AbilityService(ILogger<SkjaldmaerTier3AbilityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ═══════ Unbreakable (Tier 3 Passive) ═══════

    /// <summary>
    /// Gets damage reduction from Unbreakable passive.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Damage reduction value (3 if Unbreakable is unlocked, 0 otherwise).</returns>
    public int GetDamageReduction(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        if (!unlockedAbilities.Contains(SkjaldmaerAbilityId.Unbreakable))
        {
            _logger.LogDebug("Unbreakable not unlocked, no damage reduction applied");
            return 0;
        }

        _logger.LogDebug(
            "Unbreakable passive active: applying {Reduction} flat damage reduction",
            UnbreakableReduction);

        return UnbreakableReduction;
    }

    // ═══════ The Wall Lives (Capstone) ═══════

    /// <summary>
    /// Activates The Wall Lives capstone ability.
    /// </summary>
    /// <returns>An active TheWallLivesState with 3 turns remaining.</returns>
    public TheWallLivesState ActivateTheWallLives()
    {
        var state = TheWallLivesState.Activate();

        _logger.LogInformation(
            "The Wall Lives activated: {TurnsRemaining} turns of lethal damage protection",
            state.TurnsRemaining);

        return state;
    }

    /// <summary>
    /// Advances The Wall Lives effect by one turn.
    /// </summary>
    /// <param name="state">Current The Wall Lives state.</param>
    /// <returns>Updated state with decremented turns (deactivated at 0).</returns>
    public TheWallLivesState TickTheWallLives(TheWallLivesState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!state.IsActive)
        {
            _logger.LogDebug("The Wall Lives tick skipped: effect is inactive");
            return state;
        }

        var ticked = state.Tick();

        if (ticked.IsExpired())
        {
            _logger.LogInformation("The Wall Lives has expired after all turns consumed");
        }
        else
        {
            _logger.LogDebug(
                "The Wall Lives ticked: {TurnsRemaining} turns remaining",
                ticked.TurnsRemaining);
        }

        return ticked;
    }

    /// <summary>
    /// Checks if the capstone ability can be used this combat.
    /// </summary>
    /// <param name="hasUsedCapstoneThisCombat">Whether the capstone has already been used.</param>
    /// <returns>True if capstone is available (not already used this combat).</returns>
    public bool CanUseCapstone(bool hasUsedCapstoneThisCombat)
    {
        var canUse = !hasUsedCapstoneThisCombat;

        _logger.LogDebug(
            "Capstone availability check: hasUsedThisCombat={HasUsed}, canUse={CanUse}",
            hasUsedCapstoneThisCombat, canUse);

        return canUse;
    }

    // ═══════ PP Threshold Checks ═══════

    /// <summary>
    /// Checks whether Tier 3 abilities can be unlocked based on PP invested.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Skjaldmær tree.</param>
    /// <returns>True if the 16 PP threshold is met.</returns>
    public bool CanUnlockTier3(int ppInvested)
    {
        var canUnlock = ppInvested >= PrerequisiteService.Tier3Threshold;

        _logger.LogDebug(
            "Tier 3 unlock check: PP invested {PPInvested}, threshold {Threshold}, " +
            "canUnlock={CanUnlock}",
            ppInvested, PrerequisiteService.Tier3Threshold, canUnlock);

        return canUnlock;
    }

    /// <summary>
    /// Checks whether Capstone ability can be unlocked based on PP invested.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in the Skjaldmær tree.</param>
    /// <returns>True if the 24 PP threshold is met.</returns>
    public bool CanUnlockCapstone(int ppInvested)
    {
        var canUnlock = ppInvested >= PrerequisiteService.CapstoneThreshold;

        _logger.LogDebug(
            "Capstone unlock check: PP invested {PPInvested}, threshold {Threshold}, " +
            "canUnlock={CanUnlock}",
            ppInvested, PrerequisiteService.CapstoneThreshold, canUnlock);

        return canUnlock;
    }
}
