// ═══════════════════════════════════════════════════════════════════════════════
// ICharacterMigrationService.cs
// Interface for orchestrating the migration of characters from legacy classes
// to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates the migration of characters from legacy classes to the
/// Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// This is the primary service for managing the full migration lifecycle.
/// A typical migration flow is:
/// </para>
/// <list type="number">
///   <item><description>Retrieve mapping via <see cref="GetMappingForLegacyClass"/></description></item>
///   <item><description>Initiate migration via <see cref="InitiateMigration"/></description></item>
///   <item><description>Present available specializations via <see cref="GetAvailableSpecializations"/></description></item>
///   <item><description>Record player's choice via <see cref="SelectSpecialization"/></description></item>
///   <item><description>Complete migration via <see cref="CompleteMigration"/></description></item>
/// </list>
/// <para>
/// Every action is logged via <see cref="MigrationLog"/> entries that can be
/// retrieved with <see cref="GetMigrationLogs"/>.
/// </para>
/// </remarks>
/// <seealso cref="ILegacyClassDetectionService"/>
/// <seealso cref="CharacterMigration"/>
/// <seealso cref="MigrationResult"/>
public interface ICharacterMigrationService
{
    /// <summary>
    /// Retrieves the migration mapping for a legacy class, including the
    /// target archetype and suggested specializations.
    /// </summary>
    /// <param name="legacyClass">The legacy class to get the mapping for.</param>
    /// <returns>
    /// The <see cref="LegacyClassMapping"/> defining the migration path.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no mapping exists for the specified legacy class.
    /// </exception>
    LegacyClassMapping GetMappingForLegacyClass(LegacyClassId legacyClass);

    /// <summary>
    /// Initiates a new migration for the specified character.
    /// Creates a <see cref="CharacterMigration"/> record and assigns the
    /// target archetype based on the legacy class mapping.
    /// </summary>
    /// <param name="characterId">The character to migrate.</param>
    /// <returns>
    /// The newly created <see cref="CharacterMigration"/> record.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the character does not have a legacy class or already
    /// has an active migration.
    /// </exception>
    CharacterMigration InitiateMigration(Guid characterId);

    /// <summary>
    /// Returns the specializations available for selection during migration.
    /// </summary>
    /// <param name="migrationId">The active migration ID.</param>
    /// <returns>
    /// A read-only list of available <see cref="SpecializationId"/> values
    /// for the migration's target archetype.
    /// </returns>
    IReadOnlyList<SpecializationId> GetAvailableSpecializations(Guid migrationId);

    /// <summary>
    /// Records the player's specialization selection for the migration.
    /// </summary>
    /// <param name="migrationId">The active migration ID.</param>
    /// <param name="specialization">The selected specialization.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the migration is not in progress or the specialization
    /// is not available for the target archetype.
    /// </exception>
    void SelectSpecialization(Guid migrationId, SpecializationId specialization);

    /// <summary>
    /// Completes the migration, finalizing all changes including archetype
    /// assignment, ability evaluation, PP refunds, and specialization setup.
    /// </summary>
    /// <param name="migrationId">The migration to complete.</param>
    /// <returns>
    /// A <see cref="MigrationResult"/> containing the full outcome.
    /// </returns>
    MigrationResult CompleteMigration(Guid migrationId);

    /// <summary>
    /// Retrieves the current migration status for a character.
    /// </summary>
    /// <param name="characterId">The character to check.</param>
    /// <returns>
    /// The <see cref="CharacterMigration"/> if one exists; <c>null</c> otherwise.
    /// </returns>
    CharacterMigration? GetMigrationStatus(Guid characterId);

    /// <summary>
    /// Retrieves all migration log entries for a character.
    /// </summary>
    /// <param name="characterId">The character to retrieve logs for.</param>
    /// <returns>
    /// A read-only list of <see cref="MigrationLog"/> entries in
    /// chronological order.
    /// </returns>
    IReadOnlyList<MigrationLog> GetMigrationLogs(Guid characterId);
}
