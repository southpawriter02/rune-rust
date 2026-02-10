// ═══════════════════════════════════════════════════════════════════════════════
// CharacterMigrationService.cs
// Service that orchestrates the migration of characters from legacy classes
// to the Aethelgard archetype/specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Orchestrates the full migration lifecycle for characters transitioning
/// from legacy classes to the Aethelgard archetype/specialization system.
/// </summary>
/// <remarks>
/// <para>
/// This service manages legacy-to-archetype mappings and coordinates the
/// multi-step migration process:
/// </para>
/// <list type="number">
///   <item><description>Look up the mapping for the character's legacy class</description></item>
///   <item><description>Create a <see cref="CharacterMigration"/> record</description></item>
///   <item><description>Present available specializations for player selection</description></item>
///   <item><description>Record the specialization choice and evaluate abilities</description></item>
///   <item><description>Complete the migration with PP refunds and ability resolution</description></item>
/// </list>
/// <para>
/// All actions are logged via <see cref="MigrationLog"/> entries for full
/// auditability. Mappings are configured via the constructor-injected
/// dictionary or registered individually.
/// </para>
/// </remarks>
/// <seealso cref="ICharacterMigrationService"/>
/// <seealso cref="CharacterMigration"/>
/// <seealso cref="MigrationResult"/>
public sealed class CharacterMigrationService : ICharacterMigrationService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Legacy class-to-archetype mappings loaded from configuration.
    /// </summary>
    private readonly Dictionary<LegacyClassId, LegacyClassMapping> _mappings = new();

    /// <summary>
    /// Active migration records keyed by migration ID.
    /// </summary>
    private readonly Dictionary<Guid, CharacterMigration> _migrations = new();

    /// <summary>
    /// Character ID to migration ID lookup for quick status queries.
    /// </summary>
    private readonly Dictionary<Guid, Guid> _characterMigrationIndex = new();

    /// <summary>
    /// Migration audit log entries keyed by character ID.
    /// </summary>
    private readonly Dictionary<Guid, List<MigrationLog>> _migrationLogs = new();

    /// <summary>
    /// Legacy class detection service for validating migration candidates.
    /// </summary>
    private readonly ILegacyClassDetectionService _detectionService;

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<CharacterMigrationService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // ARCHETYPE-TO-SPECIALIZATION MAPPINGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps each archetype to its available specializations.
    /// Used by <see cref="GetAvailableSpecializations"/> to return valid
    /// choices for a migration's target archetype.
    /// </summary>
    private static readonly Dictionary<Archetype, IReadOnlyList<SpecializationId>> ArchetypeSpecializations = new()
    {
        [Archetype.Warrior] = new[]
        {
            SpecializationId.Berserkr,
            SpecializationId.IronBane,
            SpecializationId.Skjaldmaer,
            SpecializationId.SkarHorde,
            SpecializationId.AtgeirWielder,
            SpecializationId.GorgeMaw
        },
        [Archetype.Skirmisher] = new[]
        {
            SpecializationId.Veidimadr,
            SpecializationId.MyrkGengr,
            SpecializationId.Strandhogg,
            SpecializationId.HlekkrMaster
        },
        [Archetype.Mystic] = new[]
        {
            SpecializationId.Seidkona,
            SpecializationId.EchoCaller
        },
        [Archetype.Adept] = new[]
        {
            SpecializationId.BoneSetter,
            SpecializationId.JotunReader,
            SpecializationId.Skald,
            SpecializationId.ScrapTinker,
            SpecializationId.Einbui
        }
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMigrationService"/> class.
    /// </summary>
    /// <param name="detectionService">
    /// The legacy class detection service for validating migration candidates.
    /// Must not be null.
    /// </param>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="detectionService"/> or
    /// <paramref name="logger"/> is null.
    /// </exception>
    public CharacterMigrationService(
        ILegacyClassDetectionService detectionService,
        ILogger<CharacterMigrationService> logger)
    {
        ArgumentNullException.ThrowIfNull(detectionService, nameof(detectionService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _detectionService = detectionService;
        _logger = logger;

        InitializeDefaultMappings();

        _logger.LogDebug(
            "CharacterMigrationService initialized with {MappingCount} legacy class mappings",
            _mappings.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — ICharacterMigrationService
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public LegacyClassMapping GetMappingForLegacyClass(LegacyClassId legacyClass)
    {
        if (!_mappings.TryGetValue(legacyClass, out var mapping))
        {
            _logger.LogError(
                "No mapping found for legacy class. LegacyClass={LegacyClass}",
                legacyClass);

            throw new KeyNotFoundException(
                $"No migration mapping exists for legacy class '{legacyClass}'.");
        }

        _logger.LogDebug(
            "Retrieved mapping. LegacyClass={LegacyClass}, TargetArchetype={TargetArchetype}, " +
            "SuggestedSpecializations={SuggestedSpecializations}",
            legacyClass,
            mapping.TargetArchetype,
            string.Join(", ", mapping.SuggestedSpecializations));

        return mapping;
    }

    /// <inheritdoc />
    public CharacterMigration InitiateMigration(Guid characterId)
    {
        _logger.LogInformation(
            "Initiating migration. CharacterId={CharacterId}",
            characterId);

        // Validate: character must have a legacy class
        var legacyClass = _detectionService.GetLegacyClass(characterId);
        if (legacyClass is null)
        {
            throw new InvalidOperationException(
                $"Cannot initiate migration for character {characterId}: no legacy class found.");
        }

        // Validate: no active migration for this character
        if (_characterMigrationIndex.ContainsKey(characterId))
        {
            throw new InvalidOperationException(
                $"Cannot initiate migration for character {characterId}: migration already exists.");
        }

        // Look up the target archetype
        var mapping = GetMappingForLegacyClass(legacyClass.Value);

        // Create migration record
        var migration = new CharacterMigration
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            OriginalClass = legacyClass.Value,
            TargetArchetype = mapping.TargetArchetype,
            CreatedAt = DateTime.UtcNow
        };

        migration.BeginMigration();

        // Store in indices
        _migrations[migration.Id] = migration;
        _characterMigrationIndex[characterId] = migration.Id;

        // Create audit log entry
        AddLogEntry(migration.Id, characterId, "MigrationInitiated",
            $"Migration initiated from {legacyClass.Value} to {mapping.TargetArchetype}.");

        _logger.LogInformation(
            "Migration initiated. MigrationId={MigrationId}, CharacterId={CharacterId}, " +
            "LegacyClass={LegacyClass}, TargetArchetype={TargetArchetype}",
            migration.Id,
            characterId,
            legacyClass.Value,
            mapping.TargetArchetype);

        return migration;
    }

    /// <inheritdoc />
    public IReadOnlyList<SpecializationId> GetAvailableSpecializations(Guid migrationId)
    {
        var migration = GetMigrationOrThrow(migrationId);

        if (!ArchetypeSpecializations.TryGetValue(migration.TargetArchetype, out var specs))
        {
            _logger.LogWarning(
                "No specializations found for archetype. MigrationId={MigrationId}, " +
                "Archetype={Archetype}",
                migrationId,
                migration.TargetArchetype);

            return Array.Empty<SpecializationId>();
        }

        _logger.LogDebug(
            "Retrieved available specializations. MigrationId={MigrationId}, " +
            "Archetype={Archetype}, Count={Count}, " +
            "Specializations={Specializations}",
            migrationId,
            migration.TargetArchetype,
            specs.Count,
            string.Join(", ", specs));

        return specs;
    }

    /// <inheritdoc />
    public void SelectSpecialization(Guid migrationId, SpecializationId specialization)
    {
        var migration = GetMigrationOrThrow(migrationId);

        // Validate specialization is available for this archetype
        var available = GetAvailableSpecializations(migrationId);
        if (!available.Contains(specialization))
        {
            throw new InvalidOperationException(
                $"Specialization {specialization} is not available for archetype {migration.TargetArchetype}.");
        }

        migration.SelectSpecialization(specialization);

        AddLogEntry(migration.Id, migration.CharacterId, "SpecializationSelected",
            $"Player selected specialization {specialization}.");

        _logger.LogInformation(
            "Specialization selected. MigrationId={MigrationId}, " +
            "CharacterId={CharacterId}, Specialization={Specialization}",
            migrationId,
            migration.CharacterId,
            specialization);
    }

    /// <inheritdoc />
    public MigrationResult CompleteMigration(Guid migrationId)
    {
        var migration = GetMigrationOrThrow(migrationId);

        _logger.LogInformation(
            "Completing migration. MigrationId={MigrationId}, " +
            "CharacterId={CharacterId}, Status={Status}",
            migrationId,
            migration.CharacterId,
            migration.Status);

        // Validate: specialization must be selected
        if (migration.SelectedSpecialization is null)
        {
            throw new InvalidOperationException(
                $"Cannot complete migration {migrationId}: no specialization has been selected.");
        }

        try
        {
            migration.Complete();

            AddLogEntry(migration.Id, migration.CharacterId, "MigrationCompleted",
                $"Migration completed. Archetype={migration.TargetArchetype}, " +
                $"Specialization={migration.SelectedSpecialization}, " +
                $"PpRefunded={migration.PpRefunded}.");

            var result = new MigrationResult
            {
                CharacterId = migration.CharacterId,
                OriginalClass = migration.OriginalClass,
                AssignedArchetype = migration.TargetArchetype,
                SelectedSpecialization = migration.SelectedSpecialization,
                PpRefunded = migration.PpRefunded,
                PreservedAbilities = Array.Empty<string>(),
                RemovedAbilities = Array.Empty<string>(),
                Status = MigrationStatus.Completed,
                CompletedAt = migration.CompletedAt ?? DateTime.UtcNow
            };

            _logger.LogInformation(
                "Migration completed successfully. MigrationId={MigrationId}, " +
                "CharacterId={CharacterId}, OriginalClass={OriginalClass}, " +
                "AssignedArchetype={AssignedArchetype}, " +
                "SelectedSpecialization={SelectedSpecialization}, " +
                "PpRefunded={PpRefunded}",
                migrationId,
                result.CharacterId,
                result.OriginalClass,
                result.AssignedArchetype,
                result.SelectedSpecialization,
                result.PpRefunded);

            return result;
        }
        catch (Exception ex)
        {
            migration.Fail(ex.Message);

            AddLogEntry(migration.Id, migration.CharacterId, "MigrationFailed",
                $"Migration failed: {ex.Message}.");

            _logger.LogError(ex,
                "Migration failed. MigrationId={MigrationId}, " +
                "CharacterId={CharacterId}, Error={Error}",
                migrationId,
                migration.CharacterId,
                ex.Message);

            return new MigrationResult
            {
                CharacterId = migration.CharacterId,
                OriginalClass = migration.OriginalClass,
                AssignedArchetype = migration.TargetArchetype,
                PpRefunded = 0,
                PreservedAbilities = Array.Empty<string>(),
                RemovedAbilities = Array.Empty<string>(),
                Status = MigrationStatus.Failed,
                ErrorMessage = ex.Message,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public CharacterMigration? GetMigrationStatus(Guid characterId)
    {
        if (_characterMigrationIndex.TryGetValue(characterId, out var migrationId)
            && _migrations.TryGetValue(migrationId, out var migration))
        {
            _logger.LogDebug(
                "Retrieved migration status. CharacterId={CharacterId}, " +
                "MigrationId={MigrationId}, Status={Status}",
                characterId,
                migrationId,
                migration.Status);

            return migration;
        }

        _logger.LogDebug(
            "No migration found. CharacterId={CharacterId}",
            characterId);

        return null;
    }

    /// <inheritdoc />
    public IReadOnlyList<MigrationLog> GetMigrationLogs(Guid characterId)
    {
        if (_migrationLogs.TryGetValue(characterId, out var logs))
        {
            _logger.LogDebug(
                "Retrieved migration logs. CharacterId={CharacterId}, LogCount={LogCount}",
                characterId,
                logs.Count);

            return logs;
        }

        _logger.LogDebug(
            "No migration logs found. CharacterId={CharacterId}",
            characterId);

        return Array.Empty<MigrationLog>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes the default legacy class-to-archetype mappings.
    /// </summary>
    private void InitializeDefaultMappings()
    {
        _mappings[LegacyClassId.Rogue] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Rogue,
            TargetArchetype = Archetype.Skirmisher,
            SuggestedSpecializations = new[] { SpecializationId.MyrkGengr, SpecializationId.Veidimadr },
            MigrationDescription = "Your training in stealth and precision translates naturally to the Skirmisher's mobile combat style."
        };

        _mappings[LegacyClassId.Fighter] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Fighter,
            TargetArchetype = Archetype.Warrior,
            SuggestedSpecializations = new[] { SpecializationId.Skjaldmaer, SpecializationId.Berserkr },
            MigrationDescription = "Your frontline combat experience makes you a natural Warrior, excelling in both defense and devastating melee attacks."
        };

        _mappings[LegacyClassId.Mage] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Mage,
            TargetArchetype = Archetype.Mystic,
            SuggestedSpecializations = new[] { SpecializationId.Seidkona },
            MigrationDescription = "Your arcane knowledge aligns with the Mystic's command of the corrupted Aether."
        };

        _mappings[LegacyClassId.Healer] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Healer,
            TargetArchetype = Archetype.Adept,
            SuggestedSpecializations = new[] { SpecializationId.BoneSetter },
            MigrationDescription = "Your healing arts find their truest expression in the Adept's Bone-Setter specialization."
        };

        _mappings[LegacyClassId.Scholar] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Scholar,
            TargetArchetype = Archetype.Adept,
            SuggestedSpecializations = new[] { SpecializationId.JotunReader },
            MigrationDescription = "Your scholarly pursuits align with the Adept's knowledge-based specializations."
        };

        _mappings[LegacyClassId.Crafter] = new LegacyClassMapping
        {
            LegacyClass = LegacyClassId.Crafter,
            TargetArchetype = Archetype.Adept,
            SuggestedSpecializations = new[] { SpecializationId.Skald, SpecializationId.ScrapTinker },
            MigrationDescription = "Your crafting expertise translates to the Adept's creation-focused specializations."
        };
    }

    /// <summary>
    /// Retrieves a migration record or throws if not found.
    /// </summary>
    private CharacterMigration GetMigrationOrThrow(Guid migrationId)
    {
        if (!_migrations.TryGetValue(migrationId, out var migration))
        {
            throw new KeyNotFoundException(
                $"Migration {migrationId} not found.");
        }

        return migration;
    }

    /// <summary>
    /// Adds an audit log entry for a migration action.
    /// </summary>
    private void AddLogEntry(
        Guid migrationId,
        Guid characterId,
        string actionType,
        string description,
        int ppDelta = 0,
        IReadOnlyList<string>? affectedAbilities = null)
    {
        var logEntry = new MigrationLog
        {
            Id = Guid.NewGuid(),
            MigrationId = migrationId,
            CharacterId = characterId,
            ActionType = actionType,
            Description = description,
            PpDelta = ppDelta,
            AffectedAbilities = affectedAbilities,
            Timestamp = DateTime.UtcNow
        };

        if (!_migrationLogs.ContainsKey(characterId))
        {
            _migrationLogs[characterId] = new List<MigrationLog>();
        }

        _migrationLogs[characterId].Add(logEntry);

        _logger.LogDebug(
            "Migration log entry added. MigrationId={MigrationId}, " +
            "CharacterId={CharacterId}, Action={ActionType}, " +
            "Description={Description}",
            migrationId,
            characterId,
            actionType,
            description);
    }
}
