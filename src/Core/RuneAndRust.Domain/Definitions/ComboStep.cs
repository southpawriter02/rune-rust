using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Definitions;

/// <summary>
/// Represents a single step in a combo sequence.
/// </summary>
/// <remarks>
/// <para>ComboStep defines one ability that must be executed as part of a combo:</para>
/// <list type="bullet">
///   <item><description><see cref="StepNumber"/> - Position in the combo sequence (1-indexed)</description></item>
///   <item><description><see cref="AbilityId"/> - The ability that must be used for this step</description></item>
///   <item><description><see cref="TargetRequirement"/> - Constraints on the target relative to previous steps</description></item>
///   <item><description><see cref="CustomRequirement"/> - Optional custom validation expression</description></item>
/// </list>
/// <para>
/// Steps are validated using <see cref="Matches"/> during combo detection to ensure
/// the player is executing the correct ability with proper targeting.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a step requiring the player to hit the same target as the previous step
/// var step = new ComboStep
/// {
///     StepNumber = 2,
///     AbilityId = "ice-shard",
///     TargetRequirement = ComboTargetRequirement.SameTarget
/// };
///
/// // Check if an ability usage matches this step
/// bool matches = step.Matches("ice-shard", isSameTarget: true, isSelfTarget: false);
/// </code>
/// </example>
public class ComboStep
{
    // ===== Properties =====

    /// <summary>
    /// Gets or sets the position of this step in the combo sequence.
    /// </summary>
    /// <remarks>
    /// <para>Step numbers are 1-indexed (first step is 1, not 0).</para>
    /// <para>Steps are ordered by this value when loaded from configuration.</para>
    /// </remarks>
    public int StepNumber { get; set; }

    /// <summary>
    /// Gets or sets the ability identifier required for this step.
    /// </summary>
    /// <remarks>
    /// <para>Must match an existing ability ID in the ability system.</para>
    /// <para>Comparison is case-insensitive during validation.</para>
    /// </remarks>
    public string AbilityId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the target requirement for this step.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to <see cref="ComboTargetRequirement.Any"/> (no restriction).</para>
    /// <para>Used to enforce targeting patterns across combo steps.</para>
    /// </remarks>
    public ComboTargetRequirement TargetRequirement { get; set; } = ComboTargetRequirement.Any;

    /// <summary>
    /// Gets or sets an optional custom requirement expression.
    /// </summary>
    /// <remarks>
    /// <para>Reserved for future extensibility (e.g., "target.health &lt; 50%").</para>
    /// <para>Currently not evaluated - included for forward compatibility.</para>
    /// </remarks>
    public string? CustomRequirement { get; set; }

    // ===== Methods =====

    /// <summary>
    /// Checks if an ability usage matches this combo step.
    /// </summary>
    /// <param name="abilityId">The ability that was used.</param>
    /// <param name="isSameTarget">Whether the target is the same as the previous step's target.</param>
    /// <param name="isSelfTarget">Whether the ability was self-targeted.</param>
    /// <returns>True if the ability usage satisfies this step's requirements; otherwise, false.</returns>
    /// <remarks>
    /// <para>Validation checks:</para>
    /// <list type="number">
    ///   <item><description>Ability ID must match (case-insensitive)</description></item>
    ///   <item><description>Target requirement must be satisfied based on targeting context</description></item>
    /// </list>
    /// <para>
    /// For the first step of a combo, <paramref name="isSameTarget"/> should be true
    /// since there is no previous target to compare against.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var step = new ComboStep
    /// {
    ///     StepNumber = 2,
    ///     AbilityId = "ice-shard",
    ///     TargetRequirement = ComboTargetRequirement.SameTarget
    /// };
    ///
    /// // Returns true - correct ability and same target
    /// step.Matches("ice-shard", isSameTarget: true, isSelfTarget: false);
    ///
    /// // Returns false - correct ability but different target
    /// step.Matches("ice-shard", isSameTarget: false, isSelfTarget: false);
    ///
    /// // Returns false - wrong ability
    /// step.Matches("fire-bolt", isSameTarget: true, isSelfTarget: false);
    /// </code>
    /// </example>
    public bool Matches(string abilityId, bool isSameTarget, bool isSelfTarget)
    {
        // Check ability ID (case-insensitive)
        if (!AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check target requirement
        return TargetRequirement switch
        {
            ComboTargetRequirement.Any => true,
            ComboTargetRequirement.SameTarget => isSameTarget,
            ComboTargetRequirement.DifferentTarget => !isSameTarget,
            ComboTargetRequirement.Self => isSelfTarget,
            _ => true // Default to allowing unknown requirements
        };
    }

    /// <summary>
    /// Returns a string representation of this combo step.
    /// </summary>
    /// <returns>A string showing the step number and ability ID.</returns>
    public override string ToString() => $"Step {StepNumber}: {AbilityId}";
}
