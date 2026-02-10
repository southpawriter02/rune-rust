// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationPrerequisiteService.cs
// Service that validates specialization prerequisites and manages unlock
// costs for the Aethelgard specialization system.
// Version: 0.20.0a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Validates specialization prerequisites including archetype compatibility,
/// PP investment thresholds, and unlock costs.
/// </summary>
/// <remarks>
/// <para>
/// This service enforces all rules governing specialization selection.
/// During migration, the first specialization is always free (0 PP cost).
/// </para>
/// <para>
/// Prerequisite data is maintained in-memory; in v0.20.1+ this will be
/// backed by configuration files.
/// </para>
/// </remarks>
/// <seealso cref="ISpecializationPrerequisiteService"/>
/// <seealso cref="SpecializationPrerequisite"/>
public sealed class SpecializationPrerequisiteService : ISpecializationPrerequisiteService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Prerequisite definitions keyed by specialization ID.
    /// </summary>
    private readonly Dictionary<SpecializationId, SpecializationPrerequisite> _prerequisites = new();

    /// <summary>
    /// Tracks whether a character already has a specialization (for unlock cost).
    /// </summary>
    private readonly HashSet<Guid> _charactersWithSpecialization = new();

    /// <summary>
    /// Logger for structured diagnostic output.
    /// </summary>
    private readonly ILogger<SpecializationPrerequisiteService> _logger;

    // ═══════════════════════════════════════════════════════════════════════════
    // ARCHETYPE-TO-SPECIALIZATION MAPPING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Maps each specialization to its parent archetype for compatibility validation.
    /// </summary>
    private static readonly Dictionary<SpecializationId, Archetype> SpecializationArchetypes = new()
    {
        // Warrior specializations
        [SpecializationId.Berserkr] = Archetype.Warrior,
        [SpecializationId.IronBane] = Archetype.Warrior,
        [SpecializationId.Skjaldmaer] = Archetype.Warrior,
        [SpecializationId.SkarHorde] = Archetype.Warrior,
        [SpecializationId.AtgeirWielder] = Archetype.Warrior,
        [SpecializationId.GorgeMaw] = Archetype.Warrior,

        // Skirmisher specializations
        [SpecializationId.Veidimadr] = Archetype.Skirmisher,
        [SpecializationId.MyrkGengr] = Archetype.Skirmisher,
        [SpecializationId.Strandhogg] = Archetype.Skirmisher,
        [SpecializationId.HlekkrMaster] = Archetype.Skirmisher,

        // Mystic specializations
        [SpecializationId.Seidkona] = Archetype.Mystic,
        [SpecializationId.EchoCaller] = Archetype.Mystic,

        // Adept specializations
        [SpecializationId.BoneSetter] = Archetype.Adept,
        [SpecializationId.JotunReader] = Archetype.Adept,
        [SpecializationId.Skald] = Archetype.Adept,
        [SpecializationId.ScrapTinker] = Archetype.Adept,
        [SpecializationId.Einbui] = Archetype.Adept
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecializationPrerequisiteService"/> class.
    /// </summary>
    /// <param name="logger">
    /// The logger for diagnostic output. Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public SpecializationPrerequisiteService(ILogger<SpecializationPrerequisiteService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;

        InitializePrerequisites();

        _logger.LogDebug(
            "SpecializationPrerequisiteService initialized with {Count} prerequisite definitions",
            _prerequisites.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — ISpecializationPrerequisiteService
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public SpecializationPrerequisite GetPrerequisites(SpecializationId specialization)
    {
        if (!_prerequisites.TryGetValue(specialization, out var prerequisite))
        {
            _logger.LogWarning(
                "No prerequisites defined for specialization. Specialization={Specialization}",
                specialization);

            throw new KeyNotFoundException(
                $"No prerequisites defined for specialization '{specialization}'.");
        }

        _logger.LogDebug(
            "Retrieved prerequisites. Specialization={Specialization}, " +
            "RequiredArchetype={RequiredArchetype}, MinPP={MinPP}, " +
            "UnlockCost={UnlockCost}, FreeSelection={FreeSelection}",
            specialization,
            prerequisite.RequiredArchetype,
            prerequisite.MinimumArchetypePP,
            prerequisite.UnlockCost,
            prerequisite.AvailableAsFreeSelection);

        return prerequisite;
    }

    /// <inheritdoc />
    public bool CanSelectSpecialization(Guid characterId, SpecializationId specialization)
    {
        if (!_prerequisites.TryGetValue(specialization, out var prerequisite))
        {
            _logger.LogDebug(
                "Cannot select specialization: no prerequisites found. " +
                "CharacterId={CharacterId}, Specialization={Specialization}",
                characterId,
                specialization);

            return false;
        }

        // For migration, all specializations within the correct archetype are available
        var isFirstSpec = !_charactersWithSpecialization.Contains(characterId);

        _logger.LogDebug(
            "Checked specialization eligibility. CharacterId={CharacterId}, " +
            "Specialization={Specialization}, IsFirstSpec={IsFirstSpec}, " +
            "AvailableAsFreeSelection={AvailableAsFreeSelection}",
            characterId,
            specialization,
            isFirstSpec,
            prerequisite.AvailableAsFreeSelection);

        // First specialization is always available (free during migration)
        if (isFirstSpec && prerequisite.AvailableAsFreeSelection)
        {
            return true;
        }

        return prerequisite.MinimumArchetypePP == 0;
    }

    /// <inheritdoc />
    public IReadOnlyList<SpecializationId> GetAvailableSpecializations(Guid characterId)
    {
        var available = _prerequisites
            .Where(kvp => CanSelectSpecialization(characterId, kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        _logger.LogDebug(
            "Retrieved available specializations for character. " +
            "CharacterId={CharacterId}, AvailableCount={AvailableCount}",
            characterId,
            available.Count);

        return available;
    }

    /// <inheritdoc />
    public bool ValidateArchetypeCompatibility(Archetype archetype, SpecializationId specialization)
    {
        if (!SpecializationArchetypes.TryGetValue(specialization, out var requiredArchetype))
        {
            _logger.LogWarning(
                "Unknown specialization for archetype compatibility check. " +
                "Archetype={Archetype}, Specialization={Specialization}",
                archetype,
                specialization);

            return false;
        }

        var isCompatible = requiredArchetype == archetype;

        _logger.LogDebug(
            "Validated archetype compatibility. Archetype={Archetype}, " +
            "Specialization={Specialization}, RequiredArchetype={RequiredArchetype}, " +
            "IsCompatible={IsCompatible}",
            archetype,
            specialization,
            requiredArchetype,
            isCompatible);

        return isCompatible;
    }

    /// <inheritdoc />
    public int GetUnlockCost(Guid characterId, SpecializationId specialization)
    {
        var isFirstSpec = !_charactersWithSpecialization.Contains(characterId);

        if (isFirstSpec)
        {
            _logger.LogDebug(
                "First specialization is free. CharacterId={CharacterId}, " +
                "Specialization={Specialization}, UnlockCost=0",
                characterId,
                specialization);

            return 0;
        }

        var cost = _prerequisites.TryGetValue(specialization, out var prereq) ? prereq.UnlockCost : 3;

        _logger.LogDebug(
            "Calculated unlock cost. CharacterId={CharacterId}, " +
            "Specialization={Specialization}, UnlockCost={UnlockCost}",
            characterId,
            specialization,
            cost);

        return cost;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS — REGISTRATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Marks a character as having a specialization (affects unlock cost
    /// calculation for subsequent specializations).
    /// </summary>
    /// <param name="characterId">The character to mark.</param>
    public void MarkCharacterHasSpecialization(Guid characterId)
    {
        _charactersWithSpecialization.Add(characterId);

        _logger.LogDebug(
            "Marked character as having specialization. CharacterId={CharacterId}",
            characterId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes prerequisite definitions for all 17 specializations.
    /// </summary>
    private void InitializePrerequisites()
    {
        foreach (var (specId, archetype) in SpecializationArchetypes)
        {
            _prerequisites[specId] = new SpecializationPrerequisite
            {
                SpecializationId = specId,
                RequiredArchetype = archetype,
                MinimumArchetypePP = 0,
                UnlockCost = 3,
                AvailableAsFreeSelection = true
            };
        }
    }
}
