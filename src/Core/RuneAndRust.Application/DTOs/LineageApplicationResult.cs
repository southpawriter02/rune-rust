// ═══════════════════════════════════════════════════════════════════════════════
// LineageApplicationResult.cs
// Data transfer object representing the result of applying a lineage to a character.
// Version: 0.17.0f
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.DTOs;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the result of applying a lineage to a character during creation.
/// </summary>
/// <remarks>
/// <para>
/// LineageApplicationResult captures the complete outcome of a lineage application
/// operation, including success/failure status and all changes that were applied
/// to the character. This allows the UI to display a summary of what was changed
/// and provides an audit trail for debugging.
/// </para>
/// <para>
/// On success, all component fields are populated:
/// <list type="bullet">
///   <item><description><see cref="AppliedLineage"/>: Which lineage was applied</description></item>
///   <item><description><see cref="AttributeChanges"/>: All attribute modifications made</description></item>
///   <item><description><see cref="BonusesApplied"/>: Passive bonuses granted</description></item>
///   <item><description><see cref="TraitRegistered"/>: The unique trait that was registered</description></item>
///   <item><description><see cref="TraumaBaselineSet"/>: The trauma baseline values configured</description></item>
/// </list>
/// </para>
/// <para>
/// On failure, only <see cref="FailureReason"/> is populated; all other fields are <c>null</c>.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Success"/> and <see cref="Failure"/>
/// to create instances.
/// </para>
/// </remarks>
/// <param name="IsSuccess">Whether the lineage application succeeded.</param>
/// <param name="FailureReason">The reason for failure, or <c>null</c> if successful.</param>
/// <param name="AppliedLineage">The lineage that was applied, or <c>null</c> on failure.</param>
/// <param name="AttributeChanges">
/// Dictionary mapping each modified <see cref="CoreAttribute"/> to the amount it changed,
/// or <c>null</c> on failure. Only attributes with non-zero modifications are included.
/// </param>
/// <param name="BonusesApplied">
/// The passive bonuses that were applied (HP, AP, Soak, Movement, Skills),
/// or <c>null</c> on failure.
/// </param>
/// <param name="TraitRegistered">
/// The unique lineage trait that was registered on the character,
/// or <c>null</c> on failure.
/// </param>
/// <param name="TraumaBaselineSet">
/// The trauma baseline values (Corruption, Stress, resistance modifiers) that were set,
/// or <c>null</c> on failure.
/// </param>
/// <seealso cref="LineageValidationResult"/>
public record LineageApplicationResult(
    bool IsSuccess,
    string? FailureReason,
    Lineage? AppliedLineage,
    IReadOnlyDictionary<CoreAttribute, int>? AttributeChanges,
    LineagePassiveBonuses? BonusesApplied,
    LineageTrait? TraitRegistered,
    LineageTraumaBaseline? TraumaBaselineSet)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a successful lineage application result with all applied changes.
    /// </summary>
    /// <param name="lineage">The lineage that was applied.</param>
    /// <param name="attributeChanges">
    /// Dictionary of attribute modifications. Only non-zero changes should be included.
    /// </param>
    /// <param name="bonuses">The passive bonuses that were applied.</param>
    /// <param name="trait">The unique trait that was registered.</param>
    /// <param name="baseline">The trauma baseline values that were set.</param>
    /// <returns>
    /// A <see cref="LineageApplicationResult"/> with <see cref="IsSuccess"/> set to <c>true</c>
    /// and all component fields populated.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = LineageApplicationResult.Success(
    ///     Lineage.RuneMarked,
    ///     new Dictionary&lt;CoreAttribute, int&gt; { { CoreAttribute.Will, 2 }, { CoreAttribute.Sturdiness, -1 } },
    ///     LineagePassiveBonuses.RuneMarked,
    ///     LineageTrait.AetherTainted,
    ///     LineageTraumaBaseline.RuneMarked);
    /// </code>
    /// </example>
    public static LineageApplicationResult Success(
        Lineage lineage,
        Dictionary<CoreAttribute, int> attributeChanges,
        LineagePassiveBonuses bonuses,
        LineageTrait trait,
        LineageTraumaBaseline baseline) =>
        new(true, null, lineage, attributeChanges, bonuses, trait, baseline);

    /// <summary>
    /// Creates a failed lineage application result with the specified reason.
    /// </summary>
    /// <param name="reason">
    /// A human-readable description of why the lineage application failed.
    /// Must not be null or whitespace.
    /// </param>
    /// <returns>
    /// A <see cref="LineageApplicationResult"/> with <see cref="IsSuccess"/> set to <c>false</c>,
    /// <see cref="FailureReason"/> set to the provided reason, and all component fields set to <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="reason"/> is null or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// var result = LineageApplicationResult.Failure("Character already has a lineage assigned");
    /// // result.IsSuccess == false
    /// // result.FailureReason == "Character already has a lineage assigned"
    /// </code>
    /// </example>
    public static LineageApplicationResult Failure(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));
        return new(false, reason, null, null, null, null, null);
    }
}
