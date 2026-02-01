// ═══════════════════════════════════════════════════════════════════════════════
// TraumaService.cs
// Service implementation for managing character traumas and trauma mechanics.
// Version: 0.18.3e
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Records;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for managing character traumas and trauma mechanics.
/// </summary>
/// <remarks>
/// <para>
/// TraumaService coordinates:
/// </para>
/// <list type="bullet">
///   <item><description>Trauma definition loading from traumas.json</description></item>
///   <item><description>Per-character trauma tracking via repository</description></item>
///   <item><description>Trauma acquisition and stacking</description></item>
///   <item><description>Retirement condition evaluation</description></item>
///   <item><description>Trauma effect aggregation</description></item>
///   <item><description>Check coordination with ITraumaCheckService</description></item>
/// </list>
/// <para>
/// Caches TraumaDefinitions after initial load to minimize file I/O.
/// All database operations go through ITraumaRepository.
/// </para>
/// </remarks>
public class TraumaService : ITraumaService
{
    private readonly ITraumaCheckService _checkService;
    private readonly ITraumaRepository _traumaRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<TraumaService> _logger;

    private Dictionary<string, TraumaDefinition> _traumaDefinitions = new();
    private bool _definitionsLoaded;

    /// <summary>
    /// Creates a new TraumaService.
    /// </summary>
    /// <param name="checkService">The trauma check service for performing checks.</param>
    /// <param name="traumaRepository">Repository for trauma persistence.</param>
    /// <param name="playerRepository">Repository for player lookup.</param>
    /// <param name="configProvider">Configuration provider for loading definitions.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
    public TraumaService(
        ITraumaCheckService checkService,
        ITraumaRepository traumaRepository,
        IPlayerRepository playerRepository,
        IGameConfigurationProvider configProvider,
        ILogger<TraumaService> logger)
    {
        ArgumentNullException.ThrowIfNull(checkService);
        ArgumentNullException.ThrowIfNull(traumaRepository);
        ArgumentNullException.ThrowIfNull(playerRepository);
        ArgumentNullException.ThrowIfNull(configProvider);
        ArgumentNullException.ThrowIfNull(logger);

        _checkService = checkService;
        _traumaRepository = traumaRepository;
        _playerRepository = playerRepository;
        _configProvider = configProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CharacterTrauma>> GetTraumasAsync(Guid characterId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);

        try
        {
            _logger.LogDebug(
                "Getting traumas for character {CharacterId}",
                characterId);

            var traumas = await _traumaRepository.GetByCharacterIdAsync(characterId);
            return traumas.OrderBy(t => t.AcquiredAt).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting traumas for character {CharacterId}",
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TraumaDefinition> GetTraumaDefinitionAsync(string traumaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traumaId);

        try
        {
            await EnsureDefinitionsLoadedAsync();

            var normalizedId = traumaId.ToLowerInvariant();
            if (!_traumaDefinitions.TryGetValue(normalizedId, out var definition))
            {
                _logger.LogWarning(
                    "Trauma definition not found: {TraumaId}",
                    traumaId);
                throw new KeyNotFoundException($"Trauma '{traumaId}' not found in definitions");
            }

            return definition;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(
                ex,
                "Error getting trauma definition {TraumaId}",
                traumaId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasTraumaAsync(Guid characterId, string traumaId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(traumaId);

        try
        {
            var trauma = await _traumaRepository.GetByCharacterAndTraumaIdAsync(
                characterId,
                traumaId);

            return trauma is { IsActive: true };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking trauma {TraumaId} for character {CharacterId}",
                traumaId,
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetTraumaCountAsync(Guid characterId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);

        try
        {
            var traumas = await GetTraumasAsync(characterId);
            return traumas.Count(t => t.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting trauma count for character {CharacterId}",
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> GetTraumaStackCountAsync(Guid characterId, string traumaId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(traumaId);

        try
        {
            var trauma = await _traumaRepository.GetByCharacterAndTraumaIdAsync(
                characterId,
                traumaId);

            return trauma?.StackCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting stack count for {TraumaId} on {CharacterId}",
                traumaId,
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TraumaEffect>> GetActiveEffectsAsync(Guid characterId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);

        try
        {
            var traumas = await GetTraumasAsync(characterId);
            var activeTraumas = traumas.Where(t => t.IsActive).ToList();

            var effects = new List<TraumaEffect>();
            foreach (var trauma in activeTraumas)
            {
                var definition = await GetTraumaDefinitionAsync(trauma.TraumaDefinitionId);
                effects.AddRange(definition.Effects);
            }

            return effects.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting active effects for character {CharacterId}",
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TraumaAcquisitionResult> AcquireTraumaAsync(
        Guid characterId,
        string traumaId,
        string source)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(traumaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        try
        {
            // Load definition
            TraumaDefinition definition;
            try
            {
                definition = await GetTraumaDefinitionAsync(traumaId);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning(
                    "Attempted to acquire unknown trauma: {TraumaId}",
                    traumaId);
                return TraumaAcquisitionResult.CreateFailure(
                    traumaId: traumaId,
                    traumaName: traumaId,
                    source: source);
            }

            // Verify character exists
            var player = await _playerRepository.GetByIdAsync(characterId);
            if (player == null)
            {
                _logger.LogWarning(
                    "Attempted to add trauma to non-existent character: {CharacterId}",
                    characterId);
                return TraumaAcquisitionResult.CreateFailure(
                    traumaId: traumaId,
                    traumaName: definition.Name,
                    source: source);
            }

            // Check if character already has this trauma
            var existingTrauma = await _traumaRepository.GetByCharacterAndTraumaIdAsync(
                characterId,
                traumaId);

            TraumaAcquisitionResult result;

            if (existingTrauma == null)
            {
                // New trauma acquisition
                var newTrauma = CharacterTrauma.Create(
                    characterId: characterId,
                    traumaDefinitionId: traumaId,
                    source: source,
                    acquiredAt: DateTime.UtcNow);

                await _traumaRepository.AddAsync(newTrauma);
                await _traumaRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Trauma acquired: {CharacterId} gained {TraumaId} from {Source}",
                    characterId,
                    traumaId,
                    source);

                result = TraumaAcquisitionResult.CreateNew(
                    traumaId: traumaId,
                    traumaName: definition.Name,
                    source: source,
                    triggersRetirementCheck: definition.IsRetirementTrauma &&
                        definition.RetirementCondition == "On acquisition");
            }
            else if (definition.IsStackable)
            {
                // Stack existing trauma
                existingTrauma.IncrementStackCount();
                await _traumaRepository.UpdateAsync(existingTrauma);
                await _traumaRepository.SaveChangesAsync();

                _logger.LogWarning(
                    "Trauma stacked: {CharacterId} {TraumaId} now at {StackCount}",
                    characterId,
                    traumaId,
                    existingTrauma.StackCount);

                // Check if stacking triggers retirement
                var triggersRetirement = false;
                if (definition.IsRetirementTrauma && definition.RetirementCondition != null)
                {
                    var parts = definition.RetirementCondition.Split('+');
                    if (parts.Length > 0 && int.TryParse(parts[0], out var threshold))
                    {
                        triggersRetirement = existingTrauma.StackCount >= threshold;
                    }
                }

                result = TraumaAcquisitionResult.CreateStacked(
                    traumaId: traumaId,
                    traumaName: definition.Name,
                    source: source,
                    newStackCount: existingTrauma.StackCount,
                    triggersRetirementCheck: triggersRetirement);
            }
            else
            {
                // Non-stackable and already has it
                _logger.LogDebug(
                    "Attempted to stack non-stackable trauma: {CharacterId} {TraumaId}",
                    characterId,
                    traumaId);

                result = TraumaAcquisitionResult.CreateFailure(
                    traumaId: traumaId,
                    traumaName: definition.Name,
                    source: source);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error acquiring trauma {TraumaId} for {CharacterId}",
                traumaId,
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RetirementCheckResult> CheckRetirementConditionAsync(Guid characterId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);

        try
        {
            var traumas = await GetTraumasAsync(characterId);

            var immediateRetirementTraumas = new List<string>();
            var stackingRetirementTraumas = new List<string>();
            var moderateStackedTraumas = new List<string>();

            foreach (var trauma in traumas.Where(t => t.IsActive))
            {
                var definition = await GetTraumaDefinitionAsync(trauma.TraumaDefinitionId);

                if (definition.IsRetirementTrauma)
                {
                    if (definition.RetirementCondition == "On acquisition")
                    {
                        immediateRetirementTraumas.Add(trauma.TraumaDefinitionId);
                    }
                    else if (definition.RetirementCondition?.Contains('+') == true)
                    {
                        // Parse "5+" or "3+" condition
                        var parts = definition.RetirementCondition.Split('+');
                        if (int.TryParse(parts[0], out var threshold) &&
                            trauma.StackCount >= threshold)
                        {
                            stackingRetirementTraumas.Add(trauma.TraumaDefinitionId);
                        }
                    }
                }

                if (trauma.StackCount > 1)
                {
                    moderateStackedTraumas.Add(trauma.TraumaDefinitionId);
                }
            }

            var allRetirementTraumas = immediateRetirementTraumas
                .Concat(stackingRetirementTraumas)
                .ToList();

            if (immediateRetirementTraumas.Count > 0)
            {
                _logger.LogWarning(
                    "Character {CharacterId} must retire due to immediate trauma: {Traumas}",
                    characterId,
                    string.Join(", ", immediateRetirementTraumas));

                return RetirementCheckResult.CreateMustRetire(
                    characterId: characterId,
                    retirementReason: "Severe trauma forces immediate retirement",
                    traumasCausingRetirement: allRetirementTraumas);
            }

            if (stackingRetirementTraumas.Count > 0)
            {
                _logger.LogWarning(
                    "Character {CharacterId} must retire due to stacking trauma: {Traumas}",
                    characterId,
                    string.Join(", ", stackingRetirementTraumas));

                return RetirementCheckResult.CreateMustRetire(
                    characterId: characterId,
                    retirementReason: "Critical trauma stacking forces retirement",
                    traumasCausingRetirement: allRetirementTraumas);
            }

            if (moderateStackedTraumas.Count >= 3)
            {
                _logger.LogInformation(
                    "Character {CharacterId} eligible for optional retirement with {Count} stacked traumas",
                    characterId,
                    moderateStackedTraumas.Count);

                return RetirementCheckResult.CreateOptional(
                    characterId: characterId,
                    traumasCausingRetirement: moderateStackedTraumas);
            }

            return RetirementCheckResult.CreateNoRetirement(characterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking retirement condition for {CharacterId}",
                characterId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TraumaCheckResult> PerformTraumaCheckAsync(
        Guid characterId,
        TraumaCheckTrigger trigger)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(characterId, Guid.Empty);

        try
        {
            // Perform check using ITraumaCheckService
            var checkResult = await _checkService.PerformTraumaCheckAsync(characterId, trigger);

            // If failed, acquire trauma
            if (!checkResult.Passed && !string.IsNullOrEmpty(checkResult.TraumaAcquired))
            {
                await AcquireTraumaAsync(
                    characterId: characterId,
                    traumaId: checkResult.TraumaAcquired,
                    source: trigger.ToString());

                _logger.LogWarning(
                    "Trauma check failed: {CharacterId} {Trigger} → acquired {Trauma}",
                    characterId,
                    trigger,
                    checkResult.TraumaAcquired);
            }
            else if (checkResult.Passed)
            {
                _logger.LogInformation(
                    "Trauma check passed: {CharacterId} {Trigger}",
                    characterId,
                    trigger);
            }

            return checkResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error performing trauma check for {CharacterId}",
                characterId);
            throw;
        }
    }

    /// <summary>
    /// Ensures trauma definitions are loaded from configuration.
    /// </summary>
    private async Task EnsureDefinitionsLoadedAsync()
    {
        if (_definitionsLoaded)
            return;

        await LoadTraumaDefinitionsAsync();
    }

    /// <summary>
    /// Loads trauma definitions from the configuration provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no traumas found in configuration.</exception>
    private Task LoadTraumaDefinitionsAsync()
    {
        try
        {
            var config = _configProvider.GetTraumaConfiguration();

            if (config?.Traumas == null || config.Traumas.Count == 0)
            {
                _logger.LogError("No traumas found in configuration");
                throw new InvalidOperationException("No traumas found in configuration");
            }

            foreach (var traumaConfig in config.Traumas)
            {
                var effects = traumaConfig.Effects
                    .Select(e => TraumaEffect.Create(
                        effectType: e.EffectType,
                        target: e.Target,
                        value: e.Value,
                        condition: e.Condition,
                        description: e.Description))
                    .ToList();

                var triggers = traumaConfig.Triggers
                    .Select(t => TraumaTrigger.Create(
                        triggerType: t.TriggerType,
                        condition: t.Condition,
                        checkRequired: t.CheckRequired,
                        checkDifficulty: t.CheckDifficulty))
                    .ToList();

                if (!Enum.TryParse<TraumaType>(traumaConfig.Type, ignoreCase: true, out var traumaType))
                {
                    _logger.LogWarning(
                        "Unknown trauma type '{Type}' for trauma {TraumaId}, defaulting to Cognitive",
                        traumaConfig.Type,
                        traumaConfig.Id);
                    traumaType = TraumaType.Cognitive;
                }

                var definition = TraumaDefinition.Create(
                    traumaId: traumaConfig.Id,
                    name: traumaConfig.Name,
                    type: traumaType,
                    description: traumaConfig.Description,
                    flavorText: traumaConfig.FlavorText,
                    isRetirementTrauma: traumaConfig.IsRetirementTrauma,
                    retirementCondition: traumaConfig.RetirementCondition,
                    isStackable: traumaConfig.IsStackable,
                    acquisitionSources: traumaConfig.AcquisitionSources.ToList(),
                    triggers: triggers,
                    effects: effects);

                var normalizedId = traumaConfig.Id.ToLowerInvariant();
                _traumaDefinitions[normalizedId] = definition;
            }

            _definitionsLoaded = true;
            _logger.LogInformation(
                "Loaded {TraumaCount} trauma definitions",
                _traumaDefinitions.Count);

            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error loading trauma definitions");
            throw;
        }
    }
}
