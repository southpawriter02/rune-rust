// ═══════════════════════════════════════════════════════════════════════════════
// LegacyClassDetectionService.cs
// Service that detects characters with legacy class assignments requiring
// migration to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Detects characters with legacy class assignments that require migration
/// to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// This service scans registered character data to find characters still
/// assigned to one of the six legacy classes. It operates as a read-only
/// service — no character data is modified.
/// </para>
/// <para>
/// The service maintains an in-memory registry of legacy class assignments.
/// Characters are registered via <see cref="RegisterLegacyCharacter"/> and
/// queried via the <see cref="ILegacyClassDetectionService"/> interface
/// methods.
/// </para>
/// <para>
/// In v0.20.1+, this service will be backed by a persistent repository.
/// </para>
/// </remarks>
/// <seealso cref="ILegacyClassDetectionService"/>
/// <seealso cref="LegacyClassId"/>
public sealed class LegacyClassDetectionService : ILegacyClassDetectionService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// In-memory registry of characters with legacy class assignments.
    /// </summary>
    private readonly Dictionary<Guid, LegacyClassId> _legacyCharacters = new();

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<LegacyClassDetectionService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyClassDetectionService"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public LegacyClassDetectionService(ILogger<LegacyClassDetectionService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;

        _logger.LogDebug("LegacyClassDetectionService initialized");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — ILegacyClassDetectionService
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<(Guid CharacterId, LegacyClassId LegacyClass)> GetCharactersWithLegacyClasses()
    {
        var results = _legacyCharacters
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();

        _logger.LogInformation(
            "Scanned for legacy class characters. Found {Count} characters requiring migration",
            results.Count);

        return results;
    }

    /// <inheritdoc />
    public bool HasLegacyClass(Guid characterId)
    {
        var hasLegacy = _legacyCharacters.ContainsKey(characterId);

        _logger.LogDebug(
            "Checked legacy class status. CharacterId={CharacterId}, HasLegacyClass={HasLegacyClass}",
            characterId,
            hasLegacy);

        return hasLegacy;
    }

    /// <inheritdoc />
    public LegacyClassId? GetLegacyClass(Guid characterId)
    {
        if (_legacyCharacters.TryGetValue(characterId, out var legacyClass))
        {
            _logger.LogDebug(
                "Retrieved legacy class. CharacterId={CharacterId}, LegacyClass={LegacyClass}",
                characterId,
                legacyClass);

            return legacyClass;
        }

        _logger.LogDebug(
            "No legacy class found. CharacterId={CharacterId}",
            characterId);

        return null;
    }

    /// <inheritdoc />
    public int GetPendingMigrationCount()
    {
        var count = _legacyCharacters.Count;

        _logger.LogDebug(
            "Counted pending migrations. PendingCount={PendingCount}",
            count);

        return count;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — REGISTRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registers a character as having a legacy class assignment.
    /// </summary>
    /// <param name="characterId">The character ID to register.</param>
    /// <param name="legacyClass">The legacy class the character has.</param>
    /// <remarks>
    /// This method is used during character loading to populate the legacy
    /// character registry. In v0.20.1+, this will be replaced by repository
    /// queries.
    /// </remarks>
    public void RegisterLegacyCharacter(Guid characterId, LegacyClassId legacyClass)
    {
        _legacyCharacters[characterId] = legacyClass;

        _logger.LogInformation(
            "Registered legacy character. CharacterId={CharacterId}, LegacyClass={LegacyClass}",
            characterId,
            legacyClass);
    }

    /// <summary>
    /// Removes a character from the legacy registry (after successful migration).
    /// </summary>
    /// <param name="characterId">The character ID to remove.</param>
    public void UnregisterLegacyCharacter(Guid characterId)
    {
        if (_legacyCharacters.Remove(characterId))
        {
            _logger.LogInformation(
                "Unregistered legacy character after migration. CharacterId={CharacterId}",
                characterId);
        }
    }
}
