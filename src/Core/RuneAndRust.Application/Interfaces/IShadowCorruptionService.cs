// ═══════════════════════════════════════════════════════════════════════════════
// IShadowCorruptionService.cs
// Interface for evaluating and tracking Corruption risk from Myrk-gengr
// (Heretical path) shadow ability usage.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for evaluating Corruption risk associated with
/// Myrk-gengr shadow abilities.
/// </summary>
/// <remarks>
/// <para>
/// The Myrk-gengr is a Heretical specialization, meaning its abilities carry
/// the risk of Corruption — a near-permanent negative resource. Corruption
/// risk is primarily triggered by:
/// </para>
/// <list type="bullet">
///   <item><description>Using shadow abilities in bright light conditions</description></item>
///   <item><description>Targeting Coherent creatures with shadow abilities</description></item>
///   <item><description>Maintaining shadow stances in unfavorable light</description></item>
/// </list>
/// <para>
/// This service evaluates risk without applying it — application of corruption
/// is handled by the <see cref="ICorruptionService"/> via the ability services.
/// </para>
/// </remarks>
/// <seealso cref="CorruptionRiskResult"/>
/// <seealso cref="ICorruptionService"/>
/// <seealso cref="MyrkgengrAbilityId"/>
public interface IShadowCorruptionService
{
    /// <summary>
    /// Evaluates the corruption risk for using a specific ability under
    /// the given light conditions.
    /// </summary>
    /// <param name="ability">The Myrk-gengr ability being used.</param>
    /// <param name="lightLevel">Current light level at the character's position.</param>
    /// <param name="targetIsCoherent">Whether the target is a Coherent creature.</param>
    /// <returns>A result indicating whether corruption was triggered and the amount.</returns>
    CorruptionRiskResult EvaluateRisk(
        MyrkgengrAbilityId ability,
        LightLevelType lightLevel,
        bool targetIsCoherent = false);

    /// <summary>
    /// Gets the corruption amount for a specific ability and light combination.
    /// </summary>
    /// <param name="ability">The Myrk-gengr ability.</param>
    /// <param name="lightLevel">Light level to check.</param>
    /// <returns>Corruption amount (0 if no risk at this combination).</returns>
    int GetCorruptionAmount(MyrkgengrAbilityId ability, LightLevelType lightLevel);

    /// <summary>
    /// Gets all possible corruption triggers for a given ability.
    /// Useful for UI tooltips and ability information display.
    /// </summary>
    /// <param name="ability">The Myrk-gengr ability.</param>
    /// <returns>List of potential corruption triggers across all light levels.</returns>
    IReadOnlyList<CorruptionRiskResult> GetCorruptionTriggers(MyrkgengrAbilityId ability);
}
