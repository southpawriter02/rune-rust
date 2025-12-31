using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Core.Entities;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Manages Mystic Aetheric Resonance—personal attunement to ambient Flux.
/// </summary>
public class ResonanceService : IResonanceService
{
    private readonly ILogger<ResonanceService> _logger;
    private readonly IEventBus _eventBus;

    private const int SignificantChangeThreshold = 15;
    private const int OverflowDischargeAmount = 50;
    private const int SoulFractureRiskThreshold = 3;

    public ResonanceService(
        ILogger<ResonanceService> logger,
        IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;

        _logger.LogTrace("ResonanceService initialized");
    }

    /// <inheritdoc />
    public ResonanceResult ModifyResonance(Character character, int amount, string source)
    {
        _logger.LogTrace(
            "ModifyResonance called: Character={Name}, Amount={Amount}, Source={Source}",
            character.Name, amount, source);

        // Validate character is a Mystic
        if (character.Archetype != ArchetypeType.Mystic)
        {
            _logger.LogWarning(
                "ModifyResonance called for non-Mystic character {Name} (Archetype={Archetype})",
                character.Name, character.Archetype);
            return ResonanceResult.NoChange(source);
        }

        // Ensure ResonanceState exists
        character.ResonanceState ??= new ResonanceState();

        // Capture previous state
        var previousValue = character.ResonanceState.CurrentValue;
        var previousThreshold = CalculateThreshold(previousValue);

        _logger.LogTrace(
            "Previous state: Value={Value}, Threshold={Threshold}",
            previousValue, previousThreshold);

        // Calculate new value with clamping
        var rawNewValue = previousValue + amount;
        var clampedNewValue = Math.Clamp(rawNewValue, ResonanceState.MinResonance, ResonanceState.MaxResonance);
        var actualAmount = clampedNewValue - previousValue;

        // Apply change
        character.ResonanceState.CurrentValue = clampedNewValue;

        // Calculate new threshold
        var newThreshold = CalculateThreshold(clampedNewValue);

        _logger.LogTrace(
            "New state: Value={Value}, Threshold={Threshold}, ActualChange={Actual}",
            clampedNewValue, newThreshold, actualAmount);

        // Determine if event should be published
        var thresholdChanged = previousThreshold != newThreshold;
        var largeChange = Math.Abs(actualAmount) >= SignificantChangeThreshold;
        var hitBoundary = clampedNewValue == ResonanceState.MinResonance ||
                          clampedNewValue == ResonanceState.MaxResonance;

        if (thresholdChanged)
        {
            _logger.LogInformation(
                "Resonance threshold changed for {Name}: {OldThreshold} -> {NewThreshold} (Value: {Value})",
                character.Name, previousThreshold, newThreshold, clampedNewValue);
        }

        // Publish event if significant
        if (thresholdChanged || largeChange || hitBoundary)
        {
            var resonanceEvent = new ResonanceChangedEvent(
                CharacterId: character.Id,
                CharacterName: character.Name,
                OldValue: previousValue,
                NewValue: clampedNewValue,
                ChangeAmount: actualAmount,
                Source: source,
                OldThreshold: previousThreshold,
                NewThreshold: newThreshold);

            _eventBus.Publish(resonanceEvent);

            _logger.LogDebug(
                "Published ResonanceChangedEvent: {OldValue} -> {NewValue} ({Source})",
                previousValue, clampedNewValue, source);
        }

        // Check for overflow trigger
        if (clampedNewValue >= ResonanceState.MaxResonance && previousValue < ResonanceState.MaxResonance)
        {
            _logger.LogWarning(
                "OVERFLOW TRIGGERED: {Name} resonance reached {Value}!",
                character.Name, clampedNewValue);
        }

        var result = new ResonanceResult(
            PreviousValue: previousValue,
            NewValue: clampedNewValue,
            RequestedAmount: amount,
            ActualAmount: actualAmount,
            PreviousThreshold: previousThreshold,
            NewThreshold: newThreshold,
            Source: source);

        _logger.LogDebug(
            "ModifyResonance completed: {PrevValue} -> {NewValue}, ThresholdChanged={Changed}",
            previousValue, clampedNewValue, thresholdChanged);

        return result;
    }

    /// <inheritdoc />
    public int GetResonance(Character character)
    {
        _logger.LogTrace("GetResonance called for {Name}", character.Name);

        if (character.Archetype != ArchetypeType.Mystic)
        {
            _logger.LogTrace("Character {Name} is not a Mystic, returning 0", character.Name);
            return 0;
        }

        var resonance = character.ResonanceState?.CurrentValue ?? 0;
        _logger.LogDebug("Resonance for {Name}: {Value}", character.Name, resonance);
        return resonance;
    }

    /// <inheritdoc />
    public ResonanceThreshold GetThreshold(Character character)
    {
        _logger.LogTrace("GetThreshold called for {Name}", character.Name);

        var resonance = GetResonance(character);
        var threshold = CalculateThreshold(resonance);

        _logger.LogDebug("Threshold for {Name}: {Threshold} (Value={Value})",
            character.Name, threshold, resonance);
        return threshold;
    }

    /// <inheritdoc />
    public decimal GetPotencyModifier(Character character)
    {
        _logger.LogTrace("GetPotencyModifier called for {Name}", character.Name);

        var threshold = GetThreshold(character);
        var modifier = threshold switch
        {
            ResonanceThreshold.Overflow => 1.50m,
            ResonanceThreshold.Blazing => 1.30m,
            ResonanceThreshold.Bright => 1.15m,
            ResonanceThreshold.Steady => 1.00m,
            ResonanceThreshold.Dim => 0.90m,
            _ => 1.00m
        };

        _logger.LogDebug(
            "Potency modifier for {Name}: {Modifier}x at {Threshold} threshold",
            character.Name, modifier, threshold);

        return modifier;
    }

    /// <inheritdoc />
    public CastingModeResult ApplyCastingModeModifiers(CastingMode mode)
    {
        _logger.LogTrace("ApplyCastingModeModifiers called for mode: {Mode}", mode);

        var (resonanceGain, castTimeModifier, fluxModifier) = mode switch
        {
            CastingMode.Quick => (15, 0, 5),
            CastingMode.Standard => (10, 1, 0),
            CastingMode.Channeled => (5, 2, -5),
            CastingMode.Ritual => (0, -1, -10),
            _ => (10, 1, 0) // Default to Standard
        };

        var result = new CastingModeResult(
            Mode: mode,
            ResonanceGain: resonanceGain,
            CastTimeModifier: castTimeModifier,
            FluxModifier: fluxModifier);

        _logger.LogDebug(
            "Casting mode {Mode}: ResonanceGain={Res}, CastTime={Time}, FluxMod={Flux}",
            mode, resonanceGain, castTimeModifier, fluxModifier);

        return result;
    }

    /// <inheritdoc />
    public int ProcessResonanceDecay(Character character, string context = "Unspecified")
    {
        _logger.LogTrace("ProcessResonanceDecay called for {Name} in context {Context}", character.Name, context);

        if (character.Archetype != ArchetypeType.Mystic)
        {
            _logger.LogTrace("Character {Name} is not a Mystic, no decay", character.Name);
            return 0;
        }

        if (character.ResonanceState == null || character.ResonanceState.CurrentValue == 0)
        {
            _logger.LogTrace("Character {Name} has no resonance to decay", character.Name);
            return 0;
        }

        var currentValue = character.ResonanceState.CurrentValue;
        var decayRate = character.ResonanceState.DecayRate;
        var actualDecay = Math.Min(decayRate, currentValue);

        // Apply decay through ModifyResonance to handle events properly
        var result = ModifyResonance(character, -actualDecay, context);

        _logger.LogInformation(
            "Resonance decayed for {Name}: {OldValue} -> {NewValue} (-{Decay})",
            character.Name, currentValue, result.NewValue, actualDecay);

        _logger.LogDebug("ProcessResonanceDecay completed for {Name}", character.Name);

        return actualDecay;
    }

    /// <inheritdoc />
    public OverflowResult TriggerOverflow(Character character, string context = "Unspecified")
    {
        _logger.LogTrace("TriggerOverflow called for {Name} in context {Context}", character.Name, context);

        if (character.Archetype != ArchetypeType.Mystic)
        {
            _logger.LogWarning("TriggerOverflow called for non-Mystic {Name}", character.Name);
            return OverflowResult.None;
        }

        if (character.ResonanceState == null ||
            character.ResonanceState.CurrentValue < ResonanceState.MaxResonance)
        {
            _logger.LogWarning(
                "TriggerOverflow called but resonance is not at max for {Name} (Current={Value})",
                character.Name, character.ResonanceState?.CurrentValue ?? 0);
            return OverflowResult.None;
        }

        // Mark overflow active
        character.ResonanceState.IsOverflowActive = true;
        character.ResonanceState.OverflowCount++;

        var overflowCount = character.ResonanceState.OverflowCount;
        var soulFractureRisk = overflowCount >= SoulFractureRiskThreshold;

        _logger.LogWarning(
            "AETHERIC OVERFLOW: {Name} resonance peaked! Count={Count}, SoulFractureRisk={Risk}",
            character.Name, overflowCount, soulFractureRisk);

        if (soulFractureRisk)
        {
            _logger.LogCritical(
                "SOUL FRACTURE RISK: {Name} has triggered {Count} overflows! Permanent damage imminent.",
                character.Name, overflowCount);
        }

        var result = new OverflowResult(
            PotencyBonus: 1.50m,
            DurationTurns: 1,
            DischargeAmount: OverflowDischargeAmount,
            SoulFractureRisk: soulFractureRisk,
            TotalOverflowCount: overflowCount);

        // Publish overflow event
        _eventBus.Publish(new OverflowTriggeredEvent(
            CharacterId: character.Id,
            CharacterName: character.Name,
            OverflowCount: overflowCount,
            SoulFractureRisk: soulFractureRisk));

        _logger.LogDebug("TriggerOverflow completed for {Name}", character.Name);

        return result;
    }

    /// <inheritdoc />
    public int ProcessOverflowDischarge(Character character, string context = "Unspecified")
    {
        _logger.LogTrace("ProcessOverflowDischarge called for {Name} in context {Context}", character.Name, context);

        if (character.ResonanceState == null || !character.ResonanceState.IsOverflowActive)
        {
            _logger.LogTrace("No active overflow for {Name}", character.Name);
            return 0;
        }

        // Clear overflow state
        character.ResonanceState.IsOverflowActive = false;

        // Apply forced discharge
        var result = ModifyResonance(character, -OverflowDischargeAmount, context);

        _logger.LogInformation(
            "Overflow discharged for {Name}: Reduced by {Amount} to {NewValue}",
            character.Name, OverflowDischargeAmount, result.NewValue);

        _logger.LogDebug("ProcessOverflowDischarge completed for {Name}", character.Name);

        return result.ActualAmount;
    }

    /// <inheritdoc />
    public void Reset(Character character, string context = "Unspecified")
    {
        _logger.LogTrace("Reset called for {Name} in context {Context}", character.Name, context);

        if (character.ResonanceState == null)
        {
            _logger.LogTrace("No ResonanceState to reset for {Name}", character.Name);
            return;
        }

        var previousValue = character.ResonanceState.CurrentValue;
        character.ResonanceState.Reset();

        _logger.LogInformation(
            "Resonance reset for {Name}: {OldValue} -> 0",
            character.Name, previousValue);

        _logger.LogDebug("Reset completed for {Name}", character.Name);
    }

    /// <summary>
    /// Calculates the threshold for a given resonance value.
    /// </summary>
    private static ResonanceThreshold CalculateThreshold(int resonanceValue)
    {
        return resonanceValue switch
        {
            >= 100 => ResonanceThreshold.Overflow,
            >= 75 => ResonanceThreshold.Blazing,
            >= 50 => ResonanceThreshold.Bright,
            >= 25 => ResonanceThreshold.Steady,
            _ => ResonanceThreshold.Dim
        };
    }
}
