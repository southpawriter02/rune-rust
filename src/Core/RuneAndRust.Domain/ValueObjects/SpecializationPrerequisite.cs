// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationPrerequisite.cs
// Value object defining the requirements for selecting a specialization,
// including archetype compatibility, PP investment, and unlock costs.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the prerequisites and costs for selecting a specialization.
/// </summary>
/// <remarks>
/// <para>
/// Each specialization has a set of requirements that must be met before
/// a character can select it. These include:
/// </para>
/// <list type="bullet">
///   <item><description>Correct parent <see cref="Archetype"/> assignment</description></item>
///   <item><description>Minimum PP invested in the archetype tree</description></item>
///   <item><description>PP cost to unlock (0 for first specialization, 3 for additional)</description></item>
///   <item><description>Optional prerequisite specializations already selected</description></item>
/// </list>
/// <para>
/// During migration, the <see cref="AvailableAsFreeSelection"/> flag determines
/// whether a specialization can be selected at no cost as part of the legacy
/// class migration process. All specializations marked as suggested in
/// <see cref="LegacyClassMapping"/> are available for free selection.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="LegacyClassMapping"/>
public sealed record SpecializationPrerequisite
{
    /// <summary>
    /// The specialization these prerequisites apply to.
    /// </summary>
    public required SpecializationId SpecializationId { get; init; }

    /// <summary>
    /// The archetype required to select this specialization.
    /// Characters must have this archetype assigned before selection.
    /// </summary>
    public required Archetype RequiredArchetype { get; init; }

    /// <summary>
    /// Minimum Progression Points that must be invested in the archetype
    /// tree before this specialization can be selected. Value of 0 means
    /// no minimum investment required.
    /// </summary>
    public required int MinimumArchetypePP { get; init; }

    /// <summary>
    /// PP cost to unlock this specialization. First specialization is always
    /// free (0 PP); additional specializations cost 3 PP.
    /// </summary>
    public required int UnlockCost { get; init; }

    /// <summary>
    /// Whether this specialization can be selected for free during legacy
    /// class migration. All suggested specializations in the migration
    /// mapping are available for free selection.
    /// </summary>
    public required bool AvailableAsFreeSelection { get; init; }

    /// <summary>
    /// Optional list of specializations that must already be selected before
    /// this one can be chosen. Null or empty means no prerequisite
    /// specializations required.
    /// </summary>
    public IReadOnlyList<SpecializationId>? RequiredSpecializations { get; init; }
}
