// ═══════════════════════════════════════════════════════════════════════════════
// IPrerequisiteService.cs
// Interface for validating ability unlock prerequisites based on Proficiency
// Point (PP) investment thresholds.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Result of a prerequisite check for ability unlocking.
/// </summary>
/// <param name="CanUnlock">Whether all prerequisites are met.</param>
/// <param name="FailureReason">Reason for failure, or null if prerequisites are met.</param>
/// <param name="MissingPrerequisites">List of unmet prerequisite descriptions.</param>
public sealed record PrerequisiteCheckResult(
    bool CanUnlock,
    string? FailureReason,
    IReadOnlyList<string> MissingPrerequisites)
{
    /// <summary>Creates a successful prerequisite check result.</summary>
    public static PrerequisiteCheckResult Success() => new(true, null, Array.Empty<string>());

    /// <summary>
    /// Creates a failed prerequisite check result with the specified reason.
    /// </summary>
    /// <param name="reason">Human-readable failure reason.</param>
    /// <param name="missing">List of unmet prerequisites.</param>
    public static PrerequisiteCheckResult Failure(string reason, IReadOnlyList<string> missing) =>
        new(false, reason, missing);
}

/// <summary>
/// Validates ability unlock prerequisites for the Skjaldmær specialization.
/// </summary>
/// <remarks>
/// <para>
/// Prerequisites are based on total Proficiency Points (PP) invested in the
/// ability tree. Each tier requires a minimum PP threshold:
/// </para>
/// <list type="bullet">
///   <item><description>Tier 1: Free (0 PP threshold)</description></item>
///   <item><description>Tier 2: 8 PP invested, 4 PP per ability</description></item>
///   <item><description>Tier 3: 16 PP invested, 5 PP per ability</description></item>
///   <item><description>Capstone: 24 PP invested, 6 PP per ability</description></item>
/// </list>
/// </remarks>
public interface IPrerequisiteService
{
    /// <summary>
    /// Checks whether a specific ability can be unlocked given current unlocked abilities.
    /// </summary>
    /// <param name="abilityId">The ability to check prerequisites for.</param>
    /// <param name="unlockedAbilities">Currently unlocked abilities in the tree.</param>
    /// <returns>A result indicating whether prerequisites are met.</returns>
    PrerequisiteCheckResult CanUnlockAbility(
        SkjaldmaerAbilityId abilityId,
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities);

    /// <summary>
    /// Gets human-readable descriptions of missing prerequisites for an ability.
    /// </summary>
    /// <param name="abilityId">The ability to check.</param>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>List of missing prerequisite descriptions (empty if all met).</returns>
    IReadOnlyList<string> GetMissingPrerequisites(
        SkjaldmaerAbilityId abilityId,
        IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities);

    /// <summary>
    /// Calculates the total PP invested across all unlocked abilities.
    /// </summary>
    /// <param name="unlockedAbilities">Currently unlocked abilities.</param>
    /// <returns>Total PP invested.</returns>
    int GetTotalPPInvested(IReadOnlyList<SkjaldmaerAbilityId> unlockedAbilities);
}
