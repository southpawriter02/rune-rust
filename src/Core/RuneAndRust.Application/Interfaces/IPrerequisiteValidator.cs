using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Validates whether a player meets the prerequisites for a talent node.
/// </summary>
/// <remarks>
/// <para>IPrerequisiteValidator checks both node prerequisites (other talents
/// that must be unlocked) and stat prerequisites (minimum attribute values).</para>
/// <para>Implementation details are deferred to v0.10.2c. This interface serves
/// as the contract for prerequisite validation in the talent point service.</para>
/// </remarks>
public interface IPrerequisiteValidator
{
    /// <summary>
    /// Validates that a player meets all prerequisites for a talent node.
    /// </summary>
    /// <param name="player">The player attempting to allocate to the node.</param>
    /// <param name="node">The talent node to validate against.</param>
    /// <returns>A PrerequisiteResult indicating whether prerequisites are met.</returns>
    /// <remarks>
    /// <para>Validation includes:</para>
    /// <list type="bullet">
    ///   <item><description>Node prerequisites: All required nodes must have at least 1 rank</description></item>
    ///   <item><description>Stat prerequisites: Player's stats must meet minimum values</description></item>
    /// </list>
    /// </remarks>
    PrerequisiteResult ValidatePrerequisites(Player player, AbilityTreeNode node);

    /// <summary>
    /// Checks if a player meets the node prerequisites for a talent.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="node">The talent node to check against.</param>
    /// <returns>True if all node prerequisites are met; otherwise, false.</returns>
    bool MeetsNodePrerequisites(Player player, AbilityTreeNode node);

    /// <summary>
    /// Checks if a player meets the stat prerequisites for a talent.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="node">The talent node to check against.</param>
    /// <returns>True if all stat prerequisites are met; otherwise, false.</returns>
    bool MeetsStatPrerequisites(Player player, AbilityTreeNode node);
}

/// <summary>
/// Result of prerequisite validation.
/// </summary>
/// <remarks>
/// Provides both a validity flag and detailed failure reasons for UI feedback.
/// </remarks>
public record PrerequisiteResult
{
    /// <summary>
    /// Gets whether all prerequisites are met.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of unmet prerequisites (empty if IsValid is true).
    /// </summary>
    /// <remarks>
    /// Each string describes a specific unmet requirement, e.g.,
    /// "Requires Frenzy rank 1" or "Requires Strength 14 (have 12)".
    /// </remarks>
    public IReadOnlyList<string> FailureReasons { get; init; } = [];

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a result indicating all prerequisites are met.
    /// </summary>
    /// <returns>A valid PrerequisiteResult.</returns>
    public static PrerequisiteResult Valid()
        => new() { IsValid = true };

    /// <summary>
    /// Creates a result indicating prerequisites are not met.
    /// </summary>
    /// <param name="reasons">The list of unmet prerequisites.</param>
    /// <returns>An invalid PrerequisiteResult with failure reasons.</returns>
    public static PrerequisiteResult Invalid(IEnumerable<string> reasons)
        => new() { IsValid = false, FailureReasons = reasons.ToList() };

    /// <summary>
    /// Creates a result indicating prerequisites are not met.
    /// </summary>
    /// <param name="reason">A single unmet prerequisite.</param>
    /// <returns>An invalid PrerequisiteResult with failure reason.</returns>
    public static PrerequisiteResult Invalid(string reason)
        => new() { IsValid = false, FailureReasons = [reason] };
}
