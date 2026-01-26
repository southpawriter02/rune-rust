using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of passive perception calculation and room entry checks.
/// </summary>
/// <remarks>
/// <para>
/// Calculates passive perception from WITS attribute using the formula:
/// Passive Value = WITS ÷ 2 (rounded up)
/// </para>
/// <para>
/// In v0.15.6a this is a minimal implementation. Future versions will add:
/// - Status effect modifiers (v0.15.6i)
/// - Environmental modifiers based on room light level
/// - Specialization bonuses (v0.15.6h)
/// </para>
/// </remarks>
public class PassivePerceptionService : IPassivePerceptionService
{
    private readonly IHiddenElementRepository _hiddenElementRepository;
    private readonly ILogger<PassivePerceptionService> _logger;
    private readonly Func<string, int>? _witsProvider;

    /// <summary>
    /// Initializes a new instance of PassivePerceptionService.
    /// </summary>
    /// <param name="hiddenElementRepository">Repository for hidden element queries.</param>
    /// <param name="logger">Logger for perception events.</param>
    /// <param name="witsProvider">Optional function to get WITS value for a character ID. Defaults to 10 if not provided.</param>
    public PassivePerceptionService(
        IHiddenElementRepository hiddenElementRepository,
        ILogger<PassivePerceptionService> logger,
        Func<string, int>? witsProvider = null)
    {
        _hiddenElementRepository = hiddenElementRepository ?? throw new ArgumentNullException(nameof(hiddenElementRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _witsProvider = witsProvider;
    }

    /// <inheritdoc />
    public PassivePerception CalculatePassivePerception(string characterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        // Get WITS attribute - use provider or default to 10
        int wits = _witsProvider?.Invoke(characterId) ?? 10;

        // In v0.15.6a, we don't have status effect integration yet
        // Modifiers will be added in v0.15.6i
        var modifiers = new List<PerceptionModifier>();

        var passive = PassivePerception.Calculate(characterId, wits, modifiers);

        _logger.LogDebug(
            "Calculated passive perception for {CharacterId}: WITS {Wits} → base {Base}, effective {Effective}",
            characterId,
            wits,
            passive.PassiveValue,
            passive.EffectiveValue);

        return passive;
    }

    /// <inheritdoc />
    public PassivePerceptionResult CheckRoomEntry(string characterId, Guid roomId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        var passive = CalculatePassivePerception(characterId);
        var hiddenElements = _hiddenElementRepository.GetUnrevealedByRoomId(roomId);

        if (hiddenElements.Count == 0)
        {
            _logger.LogDebug(
                "Room {RoomId} has no unrevealed hidden elements",
                roomId);
            return PassivePerceptionResult.Empty(roomId.ToString(), passive.EffectiveValue);
        }

        var revealed = new List<HiddenElement>();

        foreach (var element in hiddenElements)
        {
            if (element.CanBeDiscoveredBy(passive.EffectiveValue))
            {
                element.Reveal(characterId);
                _hiddenElementRepository.Update(element);
                revealed.Add(element);

                _logger.LogInformation(
                    "Character {CharacterId} discovered {ElementType} '{ElementName}' (DC {DC} vs Passive {Passive})",
                    characterId,
                    element.ElementType,
                    element.Name,
                    element.DetectionDC,
                    passive.EffectiveValue);
            }
        }

        var result = PassivePerceptionResult.Create(
            roomId.ToString(),
            passive.EffectiveValue,
            revealed,
            hiddenElements.Count);

        if (result.HasMissedElements)
        {
            _logger.LogDebug(
                "Character {CharacterId} missed {Count} hidden elements in room {RoomId}",
                characterId,
                result.ElementsMissed,
                roomId);
        }

        return result;
    }

    /// <inheritdoc />
    public void RevealElement(string elementId, string characterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);

        var element = _hiddenElementRepository.GetByElementId(elementId)
            ?? throw new InvalidOperationException($"Hidden element '{elementId}' not found.");

        element.Reveal(characterId);
        _hiddenElementRepository.Update(element);

        _logger.LogInformation(
            "Element '{ElementId}' manually revealed by character {CharacterId}",
            elementId,
            characterId);
    }
}
