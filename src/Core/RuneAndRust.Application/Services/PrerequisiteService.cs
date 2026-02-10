// ═══════════════════════════════════════════════════════════════════════════════
// PrerequisiteService.cs
// Validates ability unlock prerequisites based on Proficiency Point (PP)
// investment thresholds for the Skjaldmær specialization.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Validates ability unlock prerequisites for the Skjaldmær specialization.
/// </summary>
/// <remarks>
/// <para>
/// PP cost per tier: Tier 1 = 0 PP (free), Tier 2 = 4 PP each,
/// Tier 3 = 5 PP each, Capstone = 6 PP.
/// </para>
/// <para>
/// Tier thresholds: Tier 2 requires 8 PP invested, Tier 3 requires 16 PP,
/// Capstone requires 24 PP.
/// </para>
/// </remarks>
public class PrerequisiteService : IPrerequisiteService
{
    private readonly ILogger<PrerequisiteService> _logger;

    /// <summary>PP threshold required to unlock Tier 2 abilities.</summary>
    public const int Tier2Threshold = 8;

    /// <summary>PP threshold required to unlock Tier 3 abilities.</summary>
    public const int Tier3Threshold = 16;

    /// <summary>PP threshold required to unlock Capstone ability.</summary>
    public const int CapstoneThreshold = 24;

    /// <summary>
    /// Initializes a new instance of <see cref="PrerequisiteService"/>.
    /// </summary>
    /// <param name="logger">Logger for prerequisite validation audit trail.</param>
    public PrerequisiteService(ILogger<PrerequisiteService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public PrerequisiteCheckResult CanUnlockAbility(
        SkjaldmaerAbilityId abilityId,
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var ppInvested = GetTotalPPInvested(unlockedAbilities);
        var requiredThreshold = GetTierThreshold(abilityId);

        _logger.LogDebug(
            "Prerequisite check: Ability {AbilityId}, PP invested {PPInvested}, " +
            "required threshold {RequiredThreshold}",
            abilityId, ppInvested, requiredThreshold);

        if (ppInvested >= requiredThreshold)
        {
            _logger.LogInformation(
                "Prerequisite check passed: Ability {AbilityId} can be unlocked. " +
                "PP invested {PPInvested} >= threshold {Threshold}",
                abilityId, ppInvested, requiredThreshold);

            return PrerequisiteCheckResult.Success();
        }

        var missing = GetMissingPrerequisites(abilityId, unlockedAbilities);
        var reason = $"Insufficient PP invested: {ppInvested}/{requiredThreshold} PP";

        _logger.LogWarning(
            "Prerequisite check failed: Ability {AbilityId} cannot be unlocked. " +
            "PP invested {PPInvested} < threshold {Threshold}. " +
            "Missing prerequisites: {MissingPrerequisites}",
            abilityId, ppInvested, requiredThreshold, string.Join(", ", missing));

        return PrerequisiteCheckResult.Failure(reason, missing);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetMissingPrerequisites(
        SkjaldmaerAbilityId abilityId,
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var missing = new List<string>();
        var ppInvested = GetTotalPPInvested(unlockedAbilities);
        var requiredThreshold = GetTierThreshold(abilityId);

        if (ppInvested < requiredThreshold)
        {
            var deficit = requiredThreshold - ppInvested;
            missing.Add($"Requires {deficit} more PP invested (current: {ppInvested}/{requiredThreshold})");
        }

        return missing.AsReadOnly();
    }

    /// <inheritdoc />
    public int GetTotalPPInvested(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities)
    {
        ArgumentNullException.ThrowIfNull(unlockedAbilities);

        var total = 0;
        foreach (var ability in unlockedAbilities)
        {
            total += GetAbilityPPCost(ability);
        }

        return total;
    }

    /// <summary>
    /// Gets the PP cost for a specific ability based on its tier.
    /// </summary>
    /// <param name="abilityId">The ability to look up.</param>
    /// <returns>PP cost: 0 (Tier 1), 4 (Tier 2), 5 (Tier 3), 6 (Capstone).</returns>
    public static int GetAbilityPPCost(SkjaldmaerAbilityId abilityId) => abilityId switch
    {
        // Tier 1: Free
        SkjaldmaerAbilityId.ShieldWall => 0,
        SkjaldmaerAbilityId.Intercept => 0,
        SkjaldmaerAbilityId.Bulwark => 0,

        // Tier 2: 4 PP each
        SkjaldmaerAbilityId.HoldTheLine => 4,
        SkjaldmaerAbilityId.CounterShield => 4,
        SkjaldmaerAbilityId.Rally => 4,

        // Tier 3: 5 PP each
        SkjaldmaerAbilityId.Unbreakable => 5,
        SkjaldmaerAbilityId.GuardiansSacrifice => 5,

        // Capstone: 6 PP
        SkjaldmaerAbilityId.TheWallLives => 6,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown ability: {abilityId}")
    };

    /// <summary>
    /// Gets the PP threshold required for an ability's tier.
    /// </summary>
    /// <param name="abilityId">The ability to determine threshold for.</param>
    /// <returns>PP threshold: 0 (Tier 1), 8 (Tier 2), 16 (Tier 3), 24 (Capstone).</returns>
    private static int GetTierThreshold(SkjaldmaerAbilityId abilityId) => abilityId switch
    {
        // Tier 1: No threshold
        SkjaldmaerAbilityId.ShieldWall or
        SkjaldmaerAbilityId.Intercept or
        SkjaldmaerAbilityId.Bulwark => 0,

        // Tier 2: 8 PP threshold
        SkjaldmaerAbilityId.HoldTheLine or
        SkjaldmaerAbilityId.CounterShield or
        SkjaldmaerAbilityId.Rally => Tier2Threshold,

        // Tier 3: 16 PP threshold
        SkjaldmaerAbilityId.Unbreakable or
        SkjaldmaerAbilityId.GuardiansSacrifice => Tier3Threshold,

        // Capstone: 24 PP threshold
        SkjaldmaerAbilityId.TheWallLives => CapstoneThreshold,

        _ => throw new ArgumentOutOfRangeException(nameof(abilityId), abilityId,
            $"Unknown ability: {abilityId}")
    };
}
