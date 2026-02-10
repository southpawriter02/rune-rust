// ═══════════════════════════════════════════════════════════════════════════════
// IMyrkgengrAbilityService.cs
// Interface for executing Myrk-gengr (Shadow-Walker) specialization abilities.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for executing Myrk-gengr specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Tier 1 Abilities (v0.20.4a):</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Shadow Step:</b> Active — teleport to a shadow within 6 spaces.
///     Costs 2 AP, 10 Shadow Essence. Generates +5 on Darkness arrival.
///   </description></item>
///   <item><description>
///     <b>Cloak of Night:</b> Stance — shadow concealment.
///     Costs 1 AP to enter, 5 Shadow Essence/turn to maintain.
///   </description></item>
///   <item><description>
///     <b>Dark-Adapted:</b> Passive — removes dim light penalties.
///     Generates +2 Shadow Essence/turn in Darkness.
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="MyrkgengrAbilityId"/>
/// <seealso cref="IShadowEssenceService"/>
/// <seealso cref="IShadowCorruptionService"/>
public interface IMyrkgengrAbilityService
{
    // ─────────────────────────────────────────────────────────────────────────
    // Tier 1: Shadow Step
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates whether a Shadow Step to the target position is possible.
    /// Checks light level, occupancy, range, and essence availability.
    /// </summary>
    /// <param name="resource">Current Shadow Essence state.</param>
    /// <param name="origin">Character's current position.</param>
    /// <param name="target">Desired destination position.</param>
    /// <returns><c>true</c> if the Shadow Step is valid.</returns>
    bool CanExecuteShadowStep(
        ShadowEssenceResource resource,
        ShadowPosition origin,
        ShadowPosition target);

    /// <summary>
    /// Executes Shadow Step: validates target, spends essence, and evaluates
    /// corruption risk.
    /// </summary>
    /// <param name="resource">Current Shadow Essence state.</param>
    /// <param name="origin">Character's current position.</param>
    /// <param name="target">Desired destination position.</param>
    /// <returns>
    /// A tuple of (success, updatedResource, corruptionResult) where
    /// corruptionResult may indicate corruption was triggered if used
    /// from bright light.
    /// </returns>
    (bool Success, ShadowEssenceResource Resource, CorruptionRiskResult? CorruptionResult)
        ExecuteShadowStep(
            ShadowEssenceResource resource,
            ShadowPosition origin,
            ShadowPosition target);

    // ─────────────────────────────────────────────────────────────────────────
    // Tier 1: Cloak of Night
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Activates Cloak of Night stance. Requires 1 AP and sufficient essence.
    /// </summary>
    /// <param name="resource">Current Shadow Essence state.</param>
    /// <returns>
    /// A tuple of (success, updatedResource). Returns (false, original) if
    /// insufficient essence.
    /// </returns>
    (bool Success, ShadowEssenceResource Resource) ActivateCloakOfNight(
        ShadowEssenceResource resource);

    /// <summary>
    /// Maintains Cloak of Night for one turn. Deducts 5 Shadow Essence.
    /// If essence is depleted, the stance ends automatically.
    /// </summary>
    /// <param name="resource">Current Shadow Essence state.</param>
    /// <param name="currentLightLevel">Light level at character's position.</param>
    /// <returns>
    /// A tuple of (stanceActive, updatedResource, corruptionResult) where
    /// stanceActive = false if essence was depleted and corruptionResult may
    /// indicate corruption if in bright light.
    /// </returns>
    (bool StanceActive, ShadowEssenceResource Resource, CorruptionRiskResult? CorruptionResult)
        MaintainCloakOfNight(
            ShadowEssenceResource resource,
            LightLevelType currentLightLevel);

    /// <summary>
    /// Gets the stealth modifier granted by Cloak of Night at the given light level.
    /// </summary>
    /// <param name="lightLevel">Current light level.</param>
    /// <returns>
    /// Stealth modifier: +3 in Darkness/DimLight, +1 in NormalLight,
    /// -1 in BrightLight/Sunlight.
    /// </returns>
    int GetCloakOfNightStealthModifier(LightLevelType lightLevel);

    /// <summary>
    /// Whether Cloak of Night grants silent movement at the given light level.
    /// </summary>
    /// <param name="lightLevel">Current light level.</param>
    /// <returns><c>true</c> if silent movement is granted (Darkness/DimLight only).</returns>
    bool GrantsSilentMovement(LightLevelType lightLevel);

    // ─────────────────────────────────────────────────────────────────────────
    // Tier 1: Dark-Adapted
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the passive essence generation amount from Dark-Adapted.
    /// Active only in Darkness.
    /// </summary>
    /// <param name="lightLevel">Current light level.</param>
    /// <returns>Essence generated per turn (2 in Darkness, 0 otherwise).</returns>
    int GetDarkAdaptedGeneration(LightLevelType lightLevel);

    /// <summary>
    /// Gets the penalty removal status provided by Dark-Adapted for
    /// the specified light level. Removes penalties only in DimLight.
    /// </summary>
    /// <param name="lightLevel">Current light level.</param>
    /// <returns><c>true</c> if dim light penalties are removed.</returns>
    bool RemovesDimLightPenalties(LightLevelType lightLevel);

    // ─────────────────────────────────────────────────────────────────────────
    // PP Cost Lookup
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the Proficiency Point (PP) cost for the specified ability.
    /// Tier 1 abilities are free (0 PP).
    /// </summary>
    /// <param name="abilityId">The ability identifier.</param>
    /// <returns>PP cost (0 for Tier 1, 4 for Tier 2, 5 for Tier 3, 6 for Capstone).</returns>
    int GetAbilityPPCost(MyrkgengrAbilityId abilityId);

    /// <summary>
    /// Checks whether the character has enough PP invested to unlock Tier 2.
    /// </summary>
    /// <param name="ppInvested">Total PP invested in Myrk-gengr abilities.</param>
    /// <returns><c>true</c> if at least 8 PP are invested.</returns>
    bool CanUnlockTier2(int ppInvested);

    /// <summary>
    /// Calculates total PP invested from a list of unlocked abilities.
    /// </summary>
    /// <param name="unlockedAbilities">List of unlocked ability IDs.</param>
    /// <returns>Total PP cost of all specified abilities.</returns>
    int CalculatePPInvested(IReadOnlyList<MyrkgengrAbilityId> unlockedAbilities);
}
