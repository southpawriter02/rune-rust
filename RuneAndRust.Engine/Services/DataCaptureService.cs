using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Service for generating and managing Data Captures during gameplay.
/// Handles discovery of lore fragments and auto-assignment to Codex entries.
/// </summary>
/// <remarks>See: SPEC-CAPTURE-001 for Data Capture System design.</remarks>
public class DataCaptureService : IDataCaptureService
{
    private readonly ILogger<DataCaptureService> _logger;
    private readonly IDataCaptureRepository _captureRepository;
    private readonly ICodexEntryRepository _codexRepository;
    private readonly Random _random;

    /// <summary>
    /// Base percentage chance to generate a capture during container search.
    /// </summary>
    private const int BaseSearchCaptureChance = 25;

    /// <summary>
    /// Percentage chance to generate a capture on Expert tier examination.
    /// </summary>
    private const int ExpertExamCaptureChance = 75;

    /// <summary>
    /// Percentage chance to generate a capture on Detailed tier examination.
    /// </summary>
    private const int DetailedExamCaptureChance = 37;

    /// <summary>
    /// Quality value for standard captures (search, detailed exam).
    /// </summary>
    private const int StandardQuality = 15;

    /// <summary>
    /// Quality value for specialist captures (expert exam).
    /// </summary>
    private const int SpecialistQuality = 30;

    /// <summary>
    /// Expert tier threshold (3+ net successes).
    /// </summary>
    private const int ExpertTier = 2;

    /// <summary>
    /// Detailed tier threshold (1+ net successes).
    /// </summary>
    private const int DetailedTier = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCaptureService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="captureRepository">The data capture repository.</param>
    /// <param name="codexRepository">The codex entry repository.</param>
    public DataCaptureService(
        ILogger<DataCaptureService> logger,
        IDataCaptureRepository captureRepository,
        ICodexEntryRepository codexRepository)
    {
        _logger = logger;
        _captureRepository = captureRepository;
        _codexRepository = codexRepository;
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCaptureService"/> class with a seeded random.
    /// Used for testing to ensure deterministic results.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="captureRepository">The data capture repository.</param>
    /// <param name="codexRepository">The codex entry repository.</param>
    /// <param name="seed">The random seed for deterministic generation.</param>
    public DataCaptureService(
        ILogger<DataCaptureService> logger,
        IDataCaptureRepository captureRepository,
        ICodexEntryRepository codexRepository,
        int seed)
    {
        _logger = logger;
        _captureRepository = captureRepository;
        _codexRepository = codexRepository;
        _random = new Random(seed);
    }

    /// <inheritdoc/>
    public async Task<CaptureResult> TryGenerateFromSearchAsync(
        Guid characterId,
        InteractableObject container,
        int witsBonus = 0)
    {
        _logger.LogDebug("Attempting capture generation for Character {CharacterId} from search of {ContainerName}",
            characterId, container.Name);

        // Calculate effective chance: base + (wits bonus * 5%)
        var effectiveChance = BaseSearchCaptureChance + (witsBonus * 5);
        var roll = _random.Next(100);

        _logger.LogDebug("Capture roll: {Roll} vs target {Target} (WITS bonus: {WitsBonus})",
            roll, effectiveChance, witsBonus);

        if (roll >= effectiveChance)
        {
            _logger.LogDebug("Capture roll failed, no capture generated");
            return CaptureResult.NoCapture("No lore fragments discovered.");
        }

        // Select a template based on the container
        var template = SelectTemplate(container);
        if (template == null)
        {
            _logger.LogDebug("No suitable capture template found for container {ContainerName}", container.Name);
            return CaptureResult.NoCapture("No lore fragments discovered.");
        }

        // Create the capture
        var capture = new DataCapture
        {
            CharacterId = characterId,
            Type = template.Type,
            FragmentContent = template.FragmentContent,
            Source = $"{template.Source} ({container.Name})",
            Quality = StandardQuality,
            IsAnalyzed = false
        };

        // Try to auto-assign to a matching Codex entry
        var wasAutoAssigned = await TryAutoAssignAsync(capture, template.MatchingKeywords);

        // Persist the capture
        await _captureRepository.AddAsync(capture);
        await _captureRepository.SaveChangesAsync();

        _logger.LogInformation("Generated {CaptureType} capture for Character {CharacterId}",
            capture.Type, characterId);

        var message = wasAutoAssigned
            ? $"You discovered a {capture.Type} fragment and added it to your Codex."
            : $"You discovered a {capture.Type} fragment. It may relate to something you haven't encountered yet.";

        return CaptureResult.Generated(message, capture, wasAutoAssigned);
    }

    /// <inheritdoc/>
    public async Task<CaptureResult> TryGenerateFromExaminationAsync(
        Guid characterId,
        InteractableObject target,
        int tierRevealed,
        int witsBonus = 0)
    {
        _logger.LogDebug("Attempting capture generation for Character {CharacterId} from examination of {TargetName} (tier {Tier})",
            characterId, target.Name, tierRevealed);

        // Base tier (0) never generates captures
        if (tierRevealed < DetailedTier)
        {
            _logger.LogDebug("Base tier examination, no capture chance");
            return CaptureResult.NoCapture("Basic examination reveals no hidden knowledge.");
        }

        // Calculate effective chance based on tier
        var baseChance = tierRevealed >= ExpertTier ? ExpertExamCaptureChance : DetailedExamCaptureChance;
        var effectiveChance = baseChance + (witsBonus * 3);
        var roll = _random.Next(100);

        _logger.LogDebug("Capture roll: {Roll} vs target {Target} (WITS bonus: {WitsBonus})",
            roll, effectiveChance, witsBonus);

        if (roll >= effectiveChance)
        {
            _logger.LogDebug("Capture roll failed, no capture generated");
            return CaptureResult.NoCapture("Your examination reveals no additional knowledge.");
        }

        // Select a template based on the target
        var template = SelectTemplate(target);
        if (template == null)
        {
            _logger.LogDebug("No suitable capture template found for target {TargetName}", target.Name);
            return CaptureResult.NoCapture("Your examination reveals no additional knowledge.");
        }

        // Quality is higher for expert tier
        var quality = tierRevealed >= ExpertTier ? SpecialistQuality : StandardQuality;
        _logger.LogTrace("Assigned quality {Quality} based on tier {Tier} (Expert threshold: {ExpertTier})",
            quality, tierRevealed, ExpertTier);

        // Create the capture
        var capture = new DataCapture
        {
            CharacterId = characterId,
            Type = template.Type,
            FragmentContent = template.FragmentContent,
            Source = $"{template.Source} ({target.Name})",
            Quality = quality,
            IsAnalyzed = false
        };

        // Try to auto-assign to a matching Codex entry
        var wasAutoAssigned = await TryAutoAssignAsync(capture, template.MatchingKeywords);

        // Persist the capture
        await _captureRepository.AddAsync(capture);
        await _captureRepository.SaveChangesAsync();

        _logger.LogInformation("Generated {CaptureType} capture for Character {CharacterId}",
            capture.Type, characterId);

        var tierName = tierRevealed >= ExpertTier ? "expert" : "detailed";
        var message = wasAutoAssigned
            ? $"Your {tierName} examination reveals a {capture.Type} fragment, added to your Codex."
            : $"Your {tierName} examination reveals a {capture.Type} fragment.";

        return CaptureResult.Generated(message, capture, wasAutoAssigned);
    }

    /// <inheritdoc/>
    public async Task<int> GetCompletionPercentageAsync(Guid entryId, Guid characterId)
    {
        _logger.LogDebug("Calculating completion percentage for Entry {EntryId} and Character {CharacterId}",
            entryId, characterId);

        var entry = await _codexRepository.GetByIdAsync(entryId);
        if (entry == null)
        {
            _logger.LogWarning("CodexEntry {EntryId} not found", entryId);
            return 0;
        }

        var fragmentCount = await _captureRepository.GetFragmentCountAsync(entryId, characterId);
        var percentage = entry.TotalFragments > 0
            ? (fragmentCount * 100) / entry.TotalFragments
            : 0;

        // Cap at 100%
        percentage = Math.Min(percentage, 100);

        _logger.LogDebug("Character {CharacterId} has {Percentage}% completion for Entry {EntryId} ({FragmentCount}/{TotalFragments})",
            characterId, percentage, entryId, fragmentCount, entry.TotalFragments);

        return percentage;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetUnlockedThresholdsAsync(Guid entryId, Guid characterId)
    {
        _logger.LogDebug("Getting unlocked thresholds for Entry {EntryId} and Character {CharacterId}",
            entryId, characterId);

        var entry = await _codexRepository.GetByIdAsync(entryId);
        if (entry == null)
        {
            _logger.LogWarning("CodexEntry {EntryId} not found", entryId);
            return Enumerable.Empty<string>();
        }

        var percentage = await GetCompletionPercentageAsync(entryId, characterId);

        var unlockedThresholds = entry.UnlockThresholds
            .Where(kv => kv.Key <= percentage)
            .OrderBy(kv => kv.Key)
            .Select(kv => kv.Value)
            .ToList();

        _logger.LogTrace("Filtered thresholds at {Percentage}%: {Thresholds}",
            percentage, string.Join(", ", unlockedThresholds));

        _logger.LogDebug("Character {CharacterId} has {Count} unlocked thresholds for Entry {EntryTitle}",
            characterId, unlockedThresholds.Count, entry.Title);

        return unlockedThresholds;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<(CodexEntry Entry, int CompletionPercent)>> GetDiscoveredEntriesAsync(Guid characterId)
    {
        _logger.LogDebug("Fetching discovered entries for Character {CharacterId}", characterId);

        var entryIds = await _captureRepository.GetDiscoveredEntryIdsAsync(characterId);
        var results = new List<(CodexEntry, int)>();

        foreach (var entryId in entryIds)
        {
            _logger.LogTrace("Processing discovered entry {EntryId}", entryId);

            var entry = await _codexRepository.GetByIdAsync(entryId);
            if (entry != null)
            {
                var pct = await GetCompletionPercentageAsync(entryId, characterId);
                results.Add((entry, pct));
                _logger.LogTrace("Added entry {EntryTitle} with {Pct}% completion", entry.Title, pct);
            }
            else
            {
                _logger.LogWarning("Discovered entry {EntryId} not found in CodexRepository", entryId);
            }
        }

        _logger.LogDebug("Retrieved {Count} discovered entries for Character {CharacterId}", results.Count, characterId);
        return results;
    }

    #region Private Methods

    /// <summary>
    /// Selects a capture template based on the interactable object.
    /// </summary>
    private CaptureTemplate? SelectTemplate(InteractableObject obj)
    {
        _logger.LogTrace("Selecting capture template for object {ObjectName}", obj.Name);

        var objectName = obj.Name.ToLowerInvariant();
        var objectDesc = obj.Description.ToLowerInvariant();
        var combined = $"{objectName} {objectDesc}";

        // Check for specific object types
        if (ContainsAny(combined, "servitor", "automaton", "machine", "mechanical"))
        {
            _logger.LogTrace("Selected RustedServitor template category for {ObjectName}", obj.Name);
            return SelectRandomTemplate(CaptureTemplates.RustedServitor);
        }

        if (ContainsAny(combined, "blight", "corrupted", "infected", "mutation"))
        {
            _logger.LogTrace("Selected BlightedCreature template category for {ObjectName}", obj.Name);
            return SelectRandomTemplate(CaptureTemplates.BlightedCreature);
        }

        if (ContainsAny(combined, "industrial", "forge", "foundry", "factory", "mechanism"))
        {
            _logger.LogTrace("Selected IndustrialSite template category for {ObjectName}", obj.Name);
            return SelectRandomTemplate(CaptureTemplates.IndustrialSite);
        }

        if (ContainsAny(combined, "ruin", "ancient", "inscription", "tomb", "temple"))
        {
            _logger.LogTrace("Selected AncientRuin template category for {ObjectName}", obj.Name);
            return SelectRandomTemplate(CaptureTemplates.AncientRuin);
        }

        // For containers, use generic templates
        if (obj.IsContainer)
        {
            _logger.LogTrace("Selected GenericContainer template category for {ObjectName}", obj.Name);
            return SelectRandomTemplate(CaptureTemplates.GenericContainer);
        }

        // No matching template
        _logger.LogTrace("No template category matched for {ObjectName}", obj.Name);
        return null;
    }

    /// <summary>
    /// Checks if the text contains any of the specified keywords.
    /// </summary>
    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Selects a random template from the array.
    /// </summary>
    private CaptureTemplate SelectRandomTemplate(CaptureTemplate[] templates)
    {
        _logger.LogTrace("Selecting random template from {TemplateCount} templates", templates.Length);

        if (templates.Length == 0)
        {
            _logger.LogWarning("Template array is empty, returning null");
            return null!;
        }

        var index = _random.Next(templates.Length);
        var selected = templates[index];
        _logger.LogTrace("Selected template at index {Index}: Type {TemplateType}", index, selected.Type);
        return selected;
    }

    /// <summary>
    /// Attempts to auto-assign a capture to a matching Codex entry.
    /// </summary>
    private async Task<bool> TryAutoAssignAsync(DataCapture capture, string[] keywords)
    {
        _logger.LogDebug("Attempting auto-assignment for capture using keywords: {Keywords}",
            string.Join(", ", keywords));

        // Get all codex entries and try to match
        var entries = await _codexRepository.GetAllAsync();

        foreach (var entry in entries)
        {
            _logger.LogTrace("Checking CodexEntry {EntryId} ({EntryTitle}) for keyword match",
                entry.Id, entry.Title);

            var titleLower = entry.Title.ToLowerInvariant();

            // Check if any keyword matches the entry title
            if (keywords.Any(k => titleLower.Contains(k.ToLowerInvariant())))
            {
                capture.CodexEntryId = entry.Id;
                _logger.LogDebug("Auto-assigned capture to CodexEntry {EntryTitle}", entry.Title);
                return true;
            }
        }

        _logger.LogDebug("No matching CodexEntry found, capture remains unassigned");
        return false;
    }

    #endregion
}
