// ═══════════════════════════════════════════════════════════════════════════════
// IShadowEssenceService.cs
// Interface for managing the Myrk-gengr Shadow Essence resource pool,
// including spending, generation, and darkness-based generation.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for managing Shadow Essence resource operations.
/// </summary>
/// <remarks>
/// <para>
/// Shadow Essence is the Myrk-gengr's special resource, spent on shadow
/// abilities and regenerated through proximity to darkness. This service
/// provides the application-layer API for all essence operations.
/// </para>
/// <para>
/// <strong>Operations:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Spending:</b> Deducts essence for ability costs (Shadow Step: 10,
///     Cloak of Night maintenance: 5/turn).
///   </description></item>
///   <item><description>
///     <b>Generation:</b> Adds essence from darkness exposure or ability effects
///     (Shadow Step: +5 on arrival in Darkness).
///   </description></item>
///   <item><description>
///     <b>Darkness Generation:</b> Per-turn essence generation scaled by light level
///     (Darkness: +5, DimLight: +3, other: +0).
///   </description></item>
/// </list>
/// </remarks>
/// <seealso cref="ShadowEssenceResource"/>
/// <seealso cref="LightLevelType"/>
public interface IShadowEssenceService
{
    /// <summary>
    /// Attempts to spend the specified amount of Shadow Essence.
    /// </summary>
    /// <param name="resource">Current essence resource state.</param>
    /// <param name="amount">Amount to spend. Must be positive.</param>
    /// <param name="sourceAbility">The ability requesting the expenditure.</param>
    /// <returns>
    /// A tuple of (success, updatedResource). Returns (false, original) if
    /// insufficient essence.
    /// </returns>
    (bool Success, ShadowEssenceResource Resource) TrySpendEssence(
        ShadowEssenceResource resource,
        int amount,
        string sourceAbility);

    /// <summary>
    /// Generates a specific amount of Shadow Essence.
    /// </summary>
    /// <param name="resource">Current essence resource state.</param>
    /// <param name="amount">Amount to generate. Must be positive.</param>
    /// <param name="source">Description of the generation source.</param>
    /// <returns>Updated resource with generated essence, capped at max.</returns>
    ShadowEssenceResource GenerateEssence(
        ShadowEssenceResource resource,
        int amount,
        string source);

    /// <summary>
    /// Generates Shadow Essence based on current light level.
    /// Called once per turn for passive darkness generation.
    /// </summary>
    /// <param name="resource">Current essence resource state.</param>
    /// <param name="lightLevel">Current light level at character position.</param>
    /// <returns>Updated resource with darkness-generated essence.</returns>
    ShadowEssenceResource GenerateFromDarkness(
        ShadowEssenceResource resource,
        LightLevelType lightLevel);

    /// <summary>
    /// Gets the essence generation amount for the specified light level.
    /// </summary>
    /// <param name="lightLevel">Light level to check.</param>
    /// <returns>Essence points generated per turn at this light level.</returns>
    int GetGenerationAmountForLightLevel(LightLevelType lightLevel);
}
