using RuneAndRust.Core.ChallengeSectors;
using RuneAndRust.Persistence;
using Serilog;

namespace RuneAndRust.Engine.ChallengeSectors;

/// <summary>
/// v0.40.2: Challenge Sector Service
/// Manages challenge sector selection, modifiers, and completion tracking
/// </summary>
public class ChallengeSectorService
{
    private static readonly ILogger _log = Log.ForContext<ChallengeSectorService>();
    private readonly ChallengeSectorRepository _repository;
    private readonly Dictionary<string, ChallengeModifier> _modifierCache;

    public ChallengeSectorService(ChallengeSectorRepository repository)
    {
        _repository = repository;

        // Cache modifiers for faster lookup
        _modifierCache = _repository.GetAllModifiers()
            .ToDictionary(m => m.ModifierId, m => m);

        _log.Information("ChallengeSectorService initialized with {ModifierCount} modifiers",
            _modifierCache.Count);
    }

    // ═════════════════════════════════════════════════════════════
    // SECTOR ACCESS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all challenge sectors
    /// </summary>
    public List<ChallengeSector> GetAllSectors()
    {
        return _repository.GetAllSectors();
    }

    /// <summary>
    /// Get a specific sector by ID
    /// </summary>
    public ChallengeSector? GetSectorById(string sectorId)
    {
        return _repository.GetSectorById(sectorId);
    }

    /// <summary>
    /// Get sectors available for a character (based on NG+ tier and prerequisites)
    /// </summary>
    public List<ChallengeSector> GetAvailableSectors(int characterId, int ngPlusTier)
    {
        var sectors = _repository.GetAvailableSectorsForCharacter(characterId, ngPlusTier);

        _log.Debug("Character {CharacterId} has {Count} available sectors at NG+{Tier}",
            characterId, sectors.Count, ngPlusTier);

        return sectors;
    }

    // ═════════════════════════════════════════════════════════════
    // MODIFIER ACCESS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all modifiers
    /// </summary>
    public List<ChallengeModifier> GetAllModifiers()
    {
        return _modifierCache.Values.ToList();
    }

    /// <summary>
    /// Get a specific modifier by ID
    /// </summary>
    public ChallengeModifier? GetModifierById(string modifierId)
    {
        return _modifierCache.TryGetValue(modifierId, out var modifier)
            ? modifier
            : null;
    }

    /// <summary>
    /// Get all modifiers for a specific sector
    /// </summary>
    public List<ChallengeModifier> GetModifiersForSector(string sectorId)
    {
        var sector = _repository.GetSectorById(sectorId);
        if (sector == null)
        {
            return new List<ChallengeModifier>();
        }

        return sector.ModifierIds
            .Select(id => GetModifierById(id))
            .Where(m => m != null)
            .Cast<ChallengeModifier>()
            .ToList();
    }

    // ═════════════════════════════════════════════════════════════
    // COMPLETION TRACKING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Complete a challenge sector
    /// </summary>
    public int CompleteChallenge(int characterId, string sectorId,
        int? completionTimeSeconds = null, int deaths = 0,
        int damageTaken = 0, int damageDealt = 0, int enemiesKilled = 0, int ngPlusTier = 0)
    {
        var completionId = _repository.LogCompletion(
            characterId, sectorId, completionTimeSeconds,
            deaths, damageTaken, damageDealt, enemiesKilled, ngPlusTier);

        _log.Information(
            "Challenge sector completed: CharacterId={CharacterId}, SectorId={SectorId}, Deaths={Deaths}, Time={Time}s",
            characterId, sectorId, deaths, completionTimeSeconds);

        return completionId;
    }

    /// <summary>
    /// Check if a character has completed a sector
    /// </summary>
    public bool HasCompleted(int characterId, string sectorId)
    {
        return _repository.HasCompletedSector(characterId, sectorId);
    }

    /// <summary>
    /// Get completion history for a character
    /// </summary>
    public List<ChallengeCompletion> GetCompletions(int characterId)
    {
        return _repository.GetCompletions(characterId);
    }

    /// <summary>
    /// Get overall progress for a character
    /// </summary>
    public ChallengeSectorProgress GetProgress(int characterId)
    {
        return _repository.GetProgress(characterId);
    }

    // ═════════════════════════════════════════════════════════════
    // MODIFIER APPLICATION (Hooks for Integration)
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if a specific modifier is active for a sector
    /// </summary>
    public bool HasModifier(ChallengeSector sector, string modifierId)
    {
        return sector.ModifierIds.Contains(modifierId);
    }

    /// <summary>
    /// Calculate total difficulty multiplier for a sector
    /// </summary>
    public float CalculateTotalDifficulty(ChallengeSector sector)
    {
        var modifiers = GetModifiersForSector(sector.SectorId);
        return modifiers.Sum(m => m.DifficultyMultiplier);
    }
}
