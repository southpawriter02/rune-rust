// ═══════════════════════════════════════════════════════════════════════════════
// ILegacyClassDetectionService.cs
// Interface for detecting characters with legacy class assignments that
// require migration to the Aethelgard specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Detects characters that still have legacy class assignments and require
/// migration to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// This service is the entry point for the migration pipeline. It scans
/// character data to identify characters with legacy class IDs and provides
/// methods for individual character queries and aggregate counts.
/// </para>
/// <para>
/// The detection service does not modify any character data — it is strictly
/// read-only. Migration is handled by
/// <see cref="ICharacterMigrationService"/>.
/// </para>
/// </remarks>
/// <seealso cref="ICharacterMigrationService"/>
/// <seealso cref="LegacyClassId"/>
public interface ILegacyClassDetectionService
{
    /// <summary>
    /// Scans all characters and returns those with legacy class assignments.
    /// </summary>
    /// <returns>
    /// A read-only list of tuples containing the character ID and their
    /// legacy class. Empty if no characters require migration.
    /// </returns>
    IReadOnlyList<(Guid CharacterId, LegacyClassId LegacyClass)> GetCharactersWithLegacyClasses();

    /// <summary>
    /// Checks whether a specific character has a legacy class assignment.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>
    /// <c>true</c> if the character has a legacy class; <c>false</c> otherwise.
    /// </returns>
    bool HasLegacyClass(Guid characterId);

    /// <summary>
    /// Retrieves the legacy class ID for a specific character.
    /// </summary>
    /// <param name="characterId">The character to query.</param>
    /// <returns>
    /// The <see cref="LegacyClassId"/> if the character has a legacy class;
    /// <c>null</c> otherwise.
    /// </returns>
    LegacyClassId? GetLegacyClass(Guid characterId);

    /// <summary>
    /// Returns the total number of characters with pending migrations.
    /// </summary>
    /// <returns>
    /// The count of characters that have legacy classes and have not yet
    /// completed migration.
    /// </returns>
    int GetPendingMigrationCount();
}
