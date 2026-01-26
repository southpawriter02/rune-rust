using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service interface for combined skill checks in the Exploration Synergies system.
/// </summary>
/// <remarks>
/// <para>
/// The Combined Check Service implements multi-skill exploration actions where
/// a primary Wasteland Survival check gates access to a secondary skill check.
/// The four exploration synergies are:
/// <list type="bullet">
///   <item><description><b>Find Hidden Path</b>: Navigation → Acrobatics (traverse)</description></item>
///   <item><description><b>Track to Lair</b>: Tracking → System Bypass (enter)</description></item>
///   <item><description><b>Avoid Patrol</b>: Hazard Detection → Acrobatics (stealth)</description></item>
///   <item><description><b>Find and Loot</b>: Foraging → System Bypass (lockpick)</description></item>
/// </list>
/// </para>
/// <para>
/// Each synergy follows the primary-secondary pattern:
/// <list type="number">
///   <item><description>Execute primary Wasteland Survival check</description></item>
///   <item><description>If primary succeeds, execute secondary skill check</description></item>
///   <item><description>Combine results with appropriate narrative</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="SynergyType"/>
/// <seealso cref="CombinedCheckResult"/>
/// <seealso cref="CombinedCheckContext"/>
public interface ICombinedCheckService
{
    /// <summary>
    /// Executes a combined skill check with primary and secondary components.
    /// </summary>
    /// <param name="player">The player making the check.</param>
    /// <param name="synergyType">The type of synergy being executed.</param>
    /// <param name="context">Context for both checks.</param>
    /// <returns>Combined result of both checks.</returns>
    /// <remarks>
    /// <para>
    /// The method follows this execution flow:
    /// <list type="number">
    ///   <item><description>Retrieve the synergy definition</description></item>
    ///   <item><description>Execute the primary skill check</description></item>
    ///   <item><description>Check if secondary should execute based on timing</description></item>
    ///   <item><description>If applicable, execute the secondary skill check</description></item>
    ///   <item><description>Generate narrative and return combined result</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    CombinedCheckResult ExecuteCombinedCheck(
        Player player,
        SynergyType synergyType,
        CombinedCheckContext context);

    /// <summary>
    /// Gets the definition for a skill synergy.
    /// </summary>
    /// <param name="synergyType">The synergy type to look up.</param>
    /// <returns>The synergy definition.</returns>
    /// <exception cref="ArgumentException">Thrown when an unknown synergy type is provided.</exception>
    SkillSynergyDefinition GetSynergyDefinition(SynergyType synergyType);

    /// <summary>
    /// Gets all synergies available in the current context.
    /// </summary>
    /// <param name="context">The current exploration context.</param>
    /// <returns>List of available synergies.</returns>
    /// <remarks>
    /// <para>
    /// Synergy availability is determined by context flags:
    /// <list type="bullet">
    ///   <item><description>AllowsNavigation → FindHiddenPath available</description></item>
    ///   <item><description>HasActiveTracking → TrackToLair available</description></item>
    ///   <item><description>HasPatrols → AvoidPatrol available</description></item>
    ///   <item><description>AllowsForaging → FindAndLoot available</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    IReadOnlyList<SynergyType> GetAvailableSynergies(ExplorationContext context);

    /// <summary>
    /// Determines if the secondary check should execute based on primary result.
    /// </summary>
    /// <param name="synergyType">The synergy being executed.</param>
    /// <param name="primaryResult">The result of the primary check.</param>
    /// <returns>True if secondary check should execute.</returns>
    /// <remarks>
    /// <para>
    /// The decision is based on the synergy's <see cref="SecondaryCheckTiming"/>:
    /// <list type="bullet">
    ///   <item><description>OnPrimarySuccess: Secondary executes only if primary succeeds</description></item>
    ///   <item><description>OnPrimaryCritical: Secondary executes only on critical success</description></item>
    ///   <item><description>Always: Secondary always executes</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool ShouldExecuteSecondary(SynergyType synergyType, SimpleCheckOutcome primaryResult);

    /// <summary>
    /// Gets the primary skill for a synergy.
    /// </summary>
    /// <param name="synergyType">The synergy type.</param>
    /// <returns>The primary Wasteland Survival check type.</returns>
    WastelandSurvivalCheckType GetPrimarySkill(SynergyType synergyType);

    /// <summary>
    /// Gets the secondary skill identifier for a synergy.
    /// </summary>
    /// <param name="synergyType">The synergy type.</param>
    /// <returns>The secondary skill identifier (e.g., "acrobatics", "system-bypass").</returns>
    string GetSecondarySkillId(SynergyType synergyType);

    /// <summary>
    /// Generates narrative description for a combined check result.
    /// </summary>
    /// <param name="synergyType">The synergy type.</param>
    /// <param name="primaryResult">The primary check result.</param>
    /// <param name="secondaryResult">The secondary check result (if executed).</param>
    /// <returns>Player-facing narrative description.</returns>
    /// <remarks>
    /// <para>
    /// Narrative is generated based on the combination of outcomes:
    /// <list type="bullet">
    ///   <item><description>Both succeed: Full success narrative</description></item>
    ///   <item><description>Primary succeeds, secondary fails: Partial success narrative</description></item>
    ///   <item><description>Primary fails: Primary failure narrative</description></item>
    ///   <item><description>Either fumbles: Fumble-specific narrative</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    string GenerateNarrative(
        SynergyType synergyType,
        SimpleCheckOutcome primaryResult,
        SimpleCheckOutcome? secondaryResult);
}
