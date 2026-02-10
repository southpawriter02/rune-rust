// ═══════════════════════════════════════════════════════════════════════════════
// LegacyClassMapping.cs
// Value object defining the mapping from a legacy character class to a target
// Aethelgard archetype, including suggested specializations and migration
// metadata.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the mapping from a legacy character class to a target Aethelgard
/// archetype, including suggested specializations and migration description.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="LegacyClassMapping"/> is loaded from the
/// <c>legacy-class-mappings.json</c> configuration file and dictates the
/// migration path for characters with legacy classes. The mapping determines:
/// </para>
/// <list type="bullet">
///   <item><description>Which <see cref="Archetype"/> the character receives</description></item>
///   <item><description>Which <see cref="SpecializationId"/> values are suggested</description></item>
///   <item><description>Whether the first specialization is granted free (0 PP cost)</description></item>
///   <item><description>A player-facing description of the migration rationale</description></item>
/// </list>
/// <para>
/// All six legacy classes have exactly one target archetype. Three legacy classes
/// (Healer, Scholar, Crafter) map to <see cref="Archetype.Adept"/>, each with
/// different suggested specializations.
/// </para>
/// </remarks>
/// <seealso cref="LegacyClassId"/>
/// <seealso cref="Archetype"/>
/// <seealso cref="SpecializationId"/>
public sealed record LegacyClassMapping
{
    /// <summary>
    /// The legacy class being migrated from.
    /// </summary>
    public required LegacyClassId LegacyClass { get; init; }

    /// <summary>
    /// The target Aethelgard archetype to assign to the character.
    /// </summary>
    public required Archetype TargetArchetype { get; init; }

    /// <summary>
    /// Specializations recommended for this legacy class migration.
    /// Players may choose any specialization available to their archetype,
    /// but these are the most thematically appropriate.
    /// </summary>
    public required IReadOnlyList<SpecializationId> SuggestedSpecializations { get; init; }

    /// <summary>
    /// Player-facing description explaining why this archetype was chosen
    /// and what the suggested specializations offer.
    /// </summary>
    public required string MigrationDescription { get; init; }

    /// <summary>
    /// Whether this migration grants the first specialization selection for free
    /// (0 PP cost). Defaults to <c>true</c> to compensate for the forced migration.
    /// </summary>
    public bool GrantsFreeSpecialization { get; init; } = true;
}
