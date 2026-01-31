// ═══════════════════════════════════════════════════════════════════════════════
// ISpecializationApplicationService.cs
// Interface for the service that applies specializations to characters during
// creation Step 5, manages tier unlocks during progression, and provides
// validation and preview capabilities. Also defines SpecializationValidationResult
// and SpecializationApplicationPreview.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides services for applying specializations to characters and managing ability tier unlocks.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ISpecializationApplicationService"/> orchestrates the application of specializations
/// to a <see cref="Player"/> entity during character creation Step 5 (Specialization Selection)
/// and manages ability tier unlocks during character progression. It coordinates with
/// <see cref="ISpecializationProvider"/> for definition lookups and applies the specialization
/// in the following order:
/// </para>
/// <list type="number">
///   <item><description>Validate preconditions (definition exists, archetype matches, no duplicate, sufficient PP)</description></item>
///   <item><description>Deduct Progression Points (0 for first specialization, 3 for additional)</description></item>
///   <item><description>Register specialization on the player with Tier 1 unlocked</description></item>
///   <item><description>Initialize special resource if the specialization has one (e.g., Rage, Block Charges)</description></item>
///   <item><description>Grant Tier 1 abilities (3 abilities, free at selection)</description></item>
/// </list>
/// <para>
/// <strong>Specialization Selection:</strong> The first specialization is free during character
/// creation. Additional specializations cost 3 Progression Points each. Specializations are
/// scoped to the character's archetype (Warrior, Skirmisher, Mystic, Adept).
/// </para>
/// <para>
/// <strong>Tier Unlock System:</strong> Each specialization has 3 tiers of abilities:
/// <list type="bullet">
///   <item><description>Tier 1: 3 abilities, free at selection, defines core specialization identity</description></item>
///   <item><description>Tier 2: 2-4 abilities, costs 2 PP, requires Tier 1 + Rank 2</description></item>
///   <item><description>Tier 3: 2-3 abilities, costs 3 PP, requires Tier 2 + Rank 3</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Heretical Path Warning:</strong> 5 of 17 specializations are classified as Heretical
/// (interfacing with corrupted Aether). When a Heretical specialization is applied, the result
/// includes a Corruption warning for UI display. Heretical specializations: Berserkr, Gorge-Maw,
/// Myrk-gengr, Seiðkona, Echo-Caller.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// <list type="number">
///   <item><description>Call <see cref="GetAvailableSpecializations"/> to list specializations for the character's archetype</description></item>
///   <item><description>Call <see cref="CanApplySpecialization"/> to validate before applying</description></item>
///   <item><description>Call <see cref="ApplySpecialization"/> to apply the specialization and grant Tier 1 abilities</description></item>
///   <item><description>Later, call <see cref="CanUnlockTier"/> and <see cref="UnlockTier"/> for progression</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe.
/// Specialization application is a single-threaded character creation or progression operation.
/// </para>
/// </remarks>
/// <seealso cref="ISpecializationProvider"/>
/// <seealso cref="SpecializationApplicationResult"/>
/// <seealso cref="TierUnlockResult"/>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationAbilityTier"/>
public interface ISpecializationApplicationService
{
    /// <summary>
    /// Applies a specialization to a player, granting Tier 1 abilities and initializing
    /// special resources.
    /// </summary>
    /// <param name="player">
    /// The player character to apply the specialization to. Must not be null.
    /// Must have an archetype that matches the specialization's parent archetype.
    /// </param>
    /// <param name="specializationId">
    /// The specialization to apply, identifying which Tier 1 abilities to grant
    /// and which special resource (if any) to initialize.
    /// </param>
    /// <returns>
    /// A <see cref="SpecializationApplicationResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Success status and any failure reason</description></item>
    ///   <item><description>The applied specialization definition</description></item>
    ///   <item><description>List of Tier 1 abilities granted (3 per specialization)</description></item>
    ///   <item><description>Whether a special resource was initialized</description></item>
    ///   <item><description>Heretical path warning if applicable</description></item>
    ///   <item><description>PP cost deducted (0 for first, 3 for additional)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations in order:
    /// <list type="number">
    ///   <item><description>Validates preconditions via <see cref="CanApplySpecialization"/></description></item>
    ///   <item><description>Retrieves the specialization definition from <see cref="ISpecializationProvider"/></description></item>
    ///   <item><description>Deducts PP cost (0 for first specialization, 3 for additional)</description></item>
    ///   <item><description>Registers the specialization on the player with Tier 1 unlocked</description></item>
    ///   <item><description>Initializes special resource if the specialization has one</description></item>
    ///   <item><description>Grants Tier 1 abilities to the player</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If validation fails or the specialization definition is not found, a failure result is
    /// returned and the player is not modified.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.ApplySpecialization(player, SpecializationId.Berserkr);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine(result.GetSummary());
    ///     // "Applied Berserkr (Heretical):
    ///     //   Abilities Granted: 3
    ///     //   Special Resource: Rage (0-100) (initialized)
    ///     //   PP Cost: 0
    ///     //   WARNING: Heretical path — abilities may trigger Corruption gain"
    /// }
    /// </code>
    /// </example>
    SpecializationApplicationResult ApplySpecialization(
        Player player,
        SpecializationId specializationId);

    /// <summary>
    /// Unlocks a tier of abilities for a player's specialization.
    /// </summary>
    /// <param name="player">
    /// The player unlocking the tier. Must not be null.
    /// Must have the specified specialization.
    /// </param>
    /// <param name="specializationId">
    /// The specialization containing the tier to unlock.
    /// </param>
    /// <param name="tierNumber">
    /// The tier to unlock (2 or 3). Tier 1 is automatically unlocked
    /// when the specialization is applied.
    /// </param>
    /// <returns>
    /// A <see cref="TierUnlockResult"/> containing:
    /// <list type="bullet">
    ///   <item><description>Success status and any failure reason</description></item>
    ///   <item><description>The unlocked tier definition</description></item>
    ///   <item><description>List of abilities granted by the tier</description></item>
    ///   <item><description>PP cost deducted</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="tierNumber"/> is less than 2 or greater than 3.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Tier unlock requirements:
    /// <list type="bullet">
    ///   <item><description>Tier 2: Requires Tier 1 unlocked + Rank 2 + 2 PP</description></item>
    ///   <item><description>Tier 3: Requires Tier 2 unlocked + Rank 3 + 3 PP</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = service.UnlockTier(player, SpecializationId.Berserkr, 2);
    /// if (result.Success)
    /// {
    ///     Console.WriteLine($"Unlocked {result.GrantedAbilities.Count} abilities");
    /// }
    /// </code>
    /// </example>
    TierUnlockResult UnlockTier(
        Player player,
        SpecializationId specializationId,
        int tierNumber);

    /// <summary>
    /// Checks if a player can apply a specific specialization.
    /// </summary>
    /// <param name="player">
    /// The player to check. Must not be null.
    /// </param>
    /// <param name="specializationId">
    /// The specialization to validate for application.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description><c>CanApply</c>: Whether the specialization can be applied</description></item>
    ///   <item><description><c>Reason</c>: Failure reason if cannot apply, or <c>null</c> if valid</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Validation checks performed:
    /// <list type="number">
    ///   <item><description>Specialization definition exists in the provider</description></item>
    ///   <item><description>Player's archetype matches the specialization's parent archetype</description></item>
    ///   <item><description>Player does not already have this specialization</description></item>
    ///   <item><description>Player has sufficient PP (0 for first, 3 for additional)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is read-only and does not modify any state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var (canApply, reason) = service.CanApplySpecialization(player, SpecializationId.Berserkr);
    /// if (canApply)
    /// {
    ///     var result = service.ApplySpecialization(player, SpecializationId.Berserkr);
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Cannot apply: {reason}");
    /// }
    /// </code>
    /// </example>
    (bool CanApply, string? Reason) CanApplySpecialization(
        Player player,
        SpecializationId specializationId);

    /// <summary>
    /// Checks if a player can unlock a specific ability tier.
    /// </summary>
    /// <param name="player">
    /// The player to check. Must not be null.
    /// </param>
    /// <param name="specializationId">
    /// The specialization containing the tier to check.
    /// </param>
    /// <param name="tierNumber">
    /// The tier to check (2 or 3).
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description><c>CanUnlock</c>: Whether the tier can be unlocked</description></item>
    ///   <item><description><c>Reason</c>: Failure reason if cannot unlock, or <c>null</c> if valid</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// Checks: has specialization, tier exists, not already unlocked, previous tier
    /// requirement met, rank sufficient, PP available (via <see cref="SpecializationAbilityTier.CanUnlock"/>).
    /// </para>
    /// <para>
    /// This method is read-only and does not modify any state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var (canUnlock, reason) = service.CanUnlockTier(player, SpecializationId.Berserkr, 2);
    /// </code>
    /// </example>
    (bool CanUnlock, string? Reason) CanUnlockTier(
        Player player,
        SpecializationId specializationId,
        int tierNumber);

    /// <summary>
    /// Gets specializations available to a player based on their archetype.
    /// </summary>
    /// <param name="player">The player whose available specializations to retrieve.</param>
    /// <returns>
    /// A read-only list of <see cref="SpecializationDefinition"/> objects available
    /// to the player's archetype. Returns an empty list if no archetype is set.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Delegates to <see cref="ISpecializationProvider.GetByArchetype"/> using the
    /// player's archetype. Expected counts by archetype:
    /// <list type="bullet">
    ///   <item><description>Warrior: 6 specializations</description></item>
    ///   <item><description>Skirmisher: 4 specializations</description></item>
    ///   <item><description>Mystic: 2 specializations</description></item>
    ///   <item><description>Adept: 5 specializations</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var available = service.GetAvailableSpecializations(player);
    /// foreach (var spec in available)
    /// {
    ///     Console.WriteLine($"[{spec.PathType}] {spec.DisplayName}: {spec.Tagline}");
    /// }
    /// </code>
    /// </example>
    IReadOnlyList<SpecializationDefinition> GetAvailableSpecializations(Player player);

    /// <summary>
    /// Gets the PP cost for a player's next specialization.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>
    /// 0 if the player has no specializations (first is free); 3 for any additional.
    /// </returns>
    /// <example>
    /// <code>
    /// int cost = service.GetNextSpecializationCost(player);
    /// Console.WriteLine($"Next specialization costs {cost} PP");
    /// </code>
    /// </example>
    int GetNextSpecializationCost(Player player);
}

/// <summary>
/// Result of validating whether a specialization can be applied to a character.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpecializationValidationResult"/> is a lightweight record used to report
/// validation status and any issues found during specialization application checks.
/// </para>
/// <para>
/// Use <see cref="Valid"/> for a successful validation or <see cref="Invalid"/>
/// to create a result with specific validation issues.
/// </para>
/// </remarks>
/// <param name="IsValid">Whether the specialization can be applied.</param>
/// <param name="Issues">List of validation issues, if any. Empty when valid.</param>
/// <seealso cref="ISpecializationApplicationService"/>
/// <seealso cref="SpecializationApplicationResult"/>
public readonly record struct SpecializationValidationResult(
    bool IsValid,
    IReadOnlyList<string> Issues)
{
    /// <summary>
    /// Creates a valid result indicating the specialization can be applied.
    /// </summary>
    /// <value>
    /// A <see cref="SpecializationValidationResult"/> with <see cref="IsValid"/> set to
    /// <c>true</c> and an empty <see cref="Issues"/> list.
    /// </value>
    public static SpecializationValidationResult Valid => new(true, Array.Empty<string>());

    /// <summary>
    /// Creates an invalid result with the specified validation issues.
    /// </summary>
    /// <param name="issues">
    /// One or more human-readable descriptions of validation problems.
    /// </param>
    /// <returns>
    /// A <see cref="SpecializationValidationResult"/> with <see cref="IsValid"/> set to
    /// <c>false</c> and the provided issues.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = SpecializationValidationResult.Invalid(
    ///     "Requires Warrior archetype (character has: Mystic).");
    /// </code>
    /// </example>
    public static SpecializationValidationResult Invalid(params string[] issues) =>
        new(false, issues);
}

/// <summary>
/// Preview of specialization application for UI display before selection.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SpecializationApplicationPreview"/> aggregates specialization data into a
/// single object suitable for display in the character creation interface. It shows
/// what the player would receive if they confirm their specialization selection.
/// </para>
/// <para>
/// This is a read-only preview — no character modifications are made.
/// </para>
/// </remarks>
/// <param name="SpecializationId">The specialization being previewed.</param>
/// <param name="DisplayName">The human-readable display name (e.g., "Berserkr", "Skjaldmaer").</param>
/// <param name="PathType">The path type classification (Coherent or Heretical).</param>
/// <param name="Tagline">Short thematic tagline (e.g., "Fury Unleashed", "The Living Shield").</param>
/// <param name="SpecialResourceSummary">Description of special resource, or null if none.</param>
/// <param name="Tier1Abilities">The 3 Tier 1 abilities that would be granted.</param>
/// <param name="TotalAbilityCount">Total abilities across all 3 tiers.</param>
/// <param name="IsHeretical">Whether this is a Heretical path (Corruption risk).</param>
/// <param name="HereticalWarning">Corruption warning for Heretical paths, or null.</param>
/// <seealso cref="ISpecializationApplicationService"/>
/// <seealso cref="SpecializationDefinition"/>
/// <seealso cref="SpecializationAbility"/>
public readonly record struct SpecializationApplicationPreview(
    SpecializationId SpecializationId,
    string DisplayName,
    SpecializationPathType PathType,
    string Tagline,
    string? SpecialResourceSummary,
    IReadOnlyList<SpecializationAbility> Tier1Abilities,
    int TotalAbilityCount,
    bool IsHeretical,
    string? HereticalWarning)
{
    /// <summary>
    /// Gets the total number of distinct elements in this preview.
    /// </summary>
    /// <value>
    /// The count of Tier 1 abilities plus 1 for each of: special resource (if any),
    /// Heretical warning (if any). Useful for UI layout calculations.
    /// </value>
    public int TotalElements =>
        Tier1Abilities.Count +
        (SpecialResourceSummary != null ? 1 : 0) +
        (IsHeretical ? 1 : 0);
}
