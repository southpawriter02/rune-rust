using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements psychic stress and corruption mechanics.
/// Stress uses WILL-based resolve checks for mitigation.
/// Corruption accumulates directly without mitigation (permanent scars).
/// </summary>
public class TraumaService : ITraumaService
{
    private readonly IDiceService _dice;
    private readonly ILogger<TraumaService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TraumaService"/> class.
    /// </summary>
    /// <param name="dice">The dice service for resolve check rolls.</param>
    /// <param name="logger">The logger for traceability.</param>
    public TraumaService(IDiceService dice, ILogger<TraumaService> logger)
    {
        _dice = dice;
        _logger = logger;
        _logger.LogInformation("TraumaService initialized");
    }

    /// <inheritdoc/>
    public StressResult InflictStress(Combatant target, int amount, string source)
    {
        _logger.LogInformation(
            "Inflicting {Amount} stress on {Target} from {Source}",
            amount, target.Name, source);

        // Capture previous state
        var previousStress = target.CurrentStress;
        var previousStatus = GetStressStatus(previousStress);

        // Roll resolve check: WILL dice pool
        var willValue = target.GetAttribute(CharacterAttribute.Will);
        var resolveRoll = _dice.Roll(willValue, $"{target.Name} Resolve Check");

        // Each success reduces stress by 1
        var mitigation = resolveRoll.Successes;
        var netStress = Math.Max(0, amount - mitigation);

        _logger.LogDebug(
            "Resolve check: {Will}d10 = {Successes} successes. " +
            "Raw {Raw} - Mitigated {Mitigated} = Net {Net}",
            willValue, resolveRoll.Successes, amount, mitigation, netStress);

        // Apply stress, clamping to 0-100
        target.CurrentStress = Math.Clamp(target.CurrentStress + netStress, 0, 100);

        var newStatus = GetStressStatus(target.CurrentStress);
        var isBreakingPoint = target.CurrentStress >= 100 && previousStress < 100;

        if (isBreakingPoint)
        {
            _logger.LogWarning("{Target} has reached the BREAKING POINT!", target.Name);
            HandleBreakingPoint(target);
        }
        else if (newStatus != previousStatus)
        {
            _logger.LogInformation(
                "{Target} stress status changed: {Previous} -> {New}",
                target.Name, previousStatus, newStatus);
        }

        _logger.LogDebug(
            "{Target} stress: {Previous} -> {Current} (Status: {Status})",
            target.Name, previousStress, target.CurrentStress, newStatus);

        return new StressResult(
            RawStress: amount,
            MitigatedAmount: mitigation,
            NetStressApplied: netStress,
            CurrentTotal: target.CurrentStress,
            PreviousStatus: previousStatus,
            NewStatus: newStatus,
            IsBreakingPoint: isBreakingPoint,
            ResolveSuccesses: resolveRoll.Successes,
            Source: source
        );
    }

    /// <inheritdoc/>
    public StressResult RecoverStress(Combatant target, int amount, string source)
    {
        _logger.LogInformation(
            "Recovering {Amount} stress from {Target} via {Source}",
            amount, target.Name, source);

        var previousStress = target.CurrentStress;
        var previousStatus = GetStressStatus(previousStress);

        // Calculate actual recovery (can't go below 0)
        var actualRecovery = Math.Min(amount, target.CurrentStress);

        // Apply recovery
        target.CurrentStress = Math.Max(0, target.CurrentStress - amount);

        var newStatus = GetStressStatus(target.CurrentStress);

        if (newStatus != previousStatus)
        {
            _logger.LogInformation(
                "{Target} stress status improved: {Previous} -> {New}",
                target.Name, previousStatus, newStatus);
        }

        _logger.LogDebug(
            "{Target} stress recovered: {Previous} -> {Current} (Status: {Status})",
            target.Name, previousStress, target.CurrentStress, newStatus);

        return new StressResult(
            RawStress: -amount,
            MitigatedAmount: 0,
            NetStressApplied: -actualRecovery,
            CurrentTotal: target.CurrentStress,
            PreviousStatus: previousStatus,
            NewStatus: newStatus,
            IsBreakingPoint: false,
            ResolveSuccesses: 0,
            Source: source
        );
    }

    /// <inheritdoc/>
    public StressStatus GetStressStatus(int stressValue)
    {
        return stressValue switch
        {
            >= 100 => StressStatus.Breaking,
            >= 80 => StressStatus.Fractured,
            >= 60 => StressStatus.Distressed,
            >= 40 => StressStatus.Shaken,
            >= 20 => StressStatus.Unsettled,
            _ => StressStatus.Stable
        };
    }

    /// <inheritdoc/>
    public int GetDefensePenalty(int stressValue)
    {
        // Defense penalty = stress / 20, max 5
        // 0-19 = 0, 20-39 = 1, 40-59 = 2, 60-79 = 3, 80-99 = 4, 100 = 5
        return Math.Min(5, stressValue / 20);
    }

    /// <inheritdoc/>
    public void HandleBreakingPoint(Combatant target)
    {
        // For combatants with a Character source, resolve the Breaking Point
        if (target.CharacterSource != null)
        {
            var result = ResolveBreakingPoint(target.CharacterSource, "Stress threshold reached");

            // Sync stress back to combatant
            target.CurrentStress = target.CharacterSource.PsychicStress;

            _logger.LogWarning(
                "BREAKING POINT RESOLVED: {Target} - Outcome: {Outcome}, New Stress: {Stress}",
                target.Name, result.Outcome, result.NewStressLevel);
        }
        else
        {
            // Non-player combatants: just reset stress to 75 (no trauma mechanic)
            _logger.LogWarning(
                "BREAKING POINT: {Target} (non-player) stress reset to 75",
                target.Name);
            target.CurrentStress = 75;
        }
    }

    #region Breaking Point Resolution (v0.3.0c)

    /// <inheritdoc/>
    public BreakingPointResult ResolveBreakingPoint(Character character, string source)
    {
        _logger.LogWarning(
            "BREAKING POINT: {Name} triggered. Source: {Source}. Rolling WILL resolve check...",
            character.Name, source);

        // Roll WILL dice pool for resolve check (difficulty 3)
        var willValue = character.GetAttribute(CharacterAttribute.Will);
        var resolveRoll = _dice.Roll(willValue, $"{character.Name} Breaking Point Resolve");

        _logger.LogInformation(
            "Resolve check: {Will}d10 = {Successes} successes, {Botches} botches",
            willValue, resolveRoll.Successes, resolveRoll.Botches);

        // Determine outcome based on successes and botches
        BreakingPointOutcome outcome;
        Trauma? acquiredTrauma = null;
        bool wasStunned = false;
        bool wasDisoriented = false;

        if (resolveRoll.Successes >= 3)
        {
            // STABILIZED: 3+ successes
            outcome = BreakingPointOutcome.Stabilized;
            character.PsychicStress = 75;
            wasDisoriented = true;

            _logger.LogInformation(
                "{Name} STABILIZED from Breaking Point. Stress reset to 75. Gains Disoriented status.",
                character.Name);
        }
        else if (resolveRoll.Successes == 0 && resolveRoll.Botches > 0)
        {
            // CATASTROPHE: 0 successes AND at least 1 botch
            outcome = BreakingPointOutcome.Catastrophe;
            character.PsychicStress = 50;
            wasStunned = true;

            // Acquire a random trauma
            var traumaDefinition = TraumaRegistry.GetRandom();
            acquiredTrauma = traumaDefinition.CreateInstance(source);
            character.ActiveTraumas.Add(acquiredTrauma);

            // Apply permanent attribute penalties
            ApplyTraumaPenalties(character, acquiredTrauma);

            _logger.LogCritical(
                "{Name} CATASTROPHE at Breaking Point! Acquired trauma: {Trauma}. Stress reset to 50. Gains Stunned.",
                character.Name, acquiredTrauma.Name);
        }
        else
        {
            // TRAUMA: <3 successes, no catastrophe condition
            outcome = BreakingPointOutcome.Trauma;
            character.PsychicStress = 50;

            // Acquire a random trauma
            var traumaDefinition = TraumaRegistry.GetRandom();
            acquiredTrauma = traumaDefinition.CreateInstance(source);
            character.ActiveTraumas.Add(acquiredTrauma);

            // Apply permanent attribute penalties
            ApplyTraumaPenalties(character, acquiredTrauma);

            _logger.LogWarning(
                "{Name} acquired trauma at Breaking Point: {Trauma}. Stress reset to 50.",
                character.Name, acquiredTrauma.Name);
        }

        return new BreakingPointResult(
            Outcome: outcome,
            AcquiredTrauma: acquiredTrauma,
            NewStressLevel: character.PsychicStress,
            ResolveSuccesses: resolveRoll.Successes,
            ResolveBotches: resolveRoll.Botches,
            WasStunned: wasStunned,
            WasDisoriented: wasDisoriented
        );
    }

    /// <inheritdoc/>
    public void ApplyTraumaPenalties(Character character, Trauma trauma)
    {
        // Look up the trauma definition to get penalties
        var definition = TraumaRegistry.GetById(trauma.DefinitionId);
        if (definition == null)
        {
            _logger.LogWarning(
                "No trauma definition found for {DefinitionId}, skipping penalty application",
                trauma.DefinitionId);
            return;
        }

        foreach (var penalty in definition.AttributePenalties)
        {
            var currentValue = character.GetAttribute(penalty.Key);
            var newValue = Math.Max(1, currentValue + penalty.Value); // Penalty is negative, min 1
            character.SetAttribute(penalty.Key, newValue);

            _logger.LogInformation(
                "{Name} trauma penalty applied: {Attribute} {CurrentValue} -> {NewValue} (from {Trauma})",
                character.Name, penalty.Key, currentValue, newValue, trauma.Name);
        }
    }

    /// <inheritdoc/>
    public int GetTraumaAttributePenalty(Character character, CharacterAttribute attribute)
    {
        var totalPenalty = 0;

        foreach (var trauma in character.ActiveTraumas.Where(t => t.IsActive))
        {
            var definition = TraumaRegistry.GetById(trauma.DefinitionId);
            if (definition != null && definition.AttributePenalties.TryGetValue(attribute, out var penalty))
            {
                totalPenalty += penalty;
            }
        }

        return totalPenalty;
    }

    #endregion

    #region Corruption Methods (v0.3.0b)

    /// <inheritdoc/>
    public CorruptionResult AddCorruption(Character character, int amount, string source)
    {
        if (amount <= 0)
        {
            _logger.LogDebug("AddCorruption called with non-positive amount {Amount}, skipping", amount);
            var currentState = GetCorruptionState(character.Corruption);
            return new CorruptionResult(
                RawCorruption: amount,
                NetCorruptionApplied: 0,
                CurrentTotal: character.Corruption,
                PreviousTier: currentState.Tier,
                NewTier: currentState.Tier,
                TierChanged: false,
                IsTerminal: currentState.IsTerminal,
                Source: source
            );
        }

        _logger.LogWarning(
            "{Name} corrupted: +{Amount} (Source: {Source})",
            character.Name, amount, source);

        // Capture previous state
        var previousCorruption = character.Corruption;
        var previousState = GetCorruptionState(previousCorruption);

        // Corruption accumulates directly - NO mitigation from WILL
        character.Corruption = Math.Min(100, character.Corruption + amount);

        var netApplied = character.Corruption - previousCorruption;
        var newState = GetCorruptionState(character.Corruption);
        var tierChanged = newState.Tier != previousState.Tier;
        var isTerminal = newState.IsTerminal && !previousState.IsTerminal;

        if (tierChanged)
        {
            _logger.LogInformation(
                "{Name} corruption threshold reached: {OldTier} -> {NewTier}",
                character.Name, previousState.Tier, newState.Tier);
        }

        _logger.LogDebug(
            "Applied corruption penalty: MaxAP x{Mult}, WILL -{WillPen}, WITS -{WitsPen}",
            newState.MaxApMultiplier, newState.WillPenalty, newState.WitsPenalty);

        if (isTerminal)
        {
            _logger.LogCritical(
                "TERMINAL ERROR: {Name} reached 100 Corruption. Entity Lost.",
                character.Name);
            HandleTerminalError(character);
        }

        return new CorruptionResult(
            RawCorruption: amount,
            NetCorruptionApplied: netApplied,
            CurrentTotal: character.Corruption,
            PreviousTier: previousState.Tier,
            NewTier: newState.Tier,
            TierChanged: tierChanged,
            IsTerminal: isTerminal,
            Source: source
        );
    }

    /// <inheritdoc/>
    public CorruptionResult AddCorruption(Combatant combatant, int amount, string source)
    {
        // Only player combatants can receive persistent corruption
        if (combatant.CharacterSource == null)
        {
            _logger.LogDebug(
                "AddCorruption called on non-player combatant {Name}, updating local state only",
                combatant.Name);

            // Update combatant's local state for display
            var previousCorruption = combatant.CurrentCorruption;
            combatant.CurrentCorruption = Math.Min(100, combatant.CurrentCorruption + Math.Max(0, amount));
            var netApplied = combatant.CurrentCorruption - previousCorruption;
            var previousState = GetCorruptionState(previousCorruption);
            var newState = GetCorruptionState(combatant.CurrentCorruption);

            return new CorruptionResult(
                RawCorruption: amount,
                NetCorruptionApplied: netApplied,
                CurrentTotal: combatant.CurrentCorruption,
                PreviousTier: previousState.Tier,
                NewTier: newState.Tier,
                TierChanged: newState.Tier != previousState.Tier,
                IsTerminal: newState.IsTerminal,
                Source: source
            );
        }

        // Update the underlying character and sync to combatant
        var result = AddCorruption(combatant.CharacterSource, amount, source);
        combatant.CurrentCorruption = combatant.CharacterSource.Corruption;

        return result;
    }

    /// <inheritdoc/>
    public CorruptionResult PurgeCorruption(Character character, int amount, string source)
    {
        if (amount <= 0)
        {
            _logger.LogDebug("PurgeCorruption called with non-positive amount {Amount}, skipping", amount);
            var currentState = GetCorruptionState(character.Corruption);
            return new CorruptionResult(
                RawCorruption: -amount,
                NetCorruptionApplied: 0,
                CurrentTotal: character.Corruption,
                PreviousTier: currentState.Tier,
                NewTier: currentState.Tier,
                TierChanged: false,
                IsTerminal: currentState.IsTerminal,
                Source: source
            );
        }

        _logger.LogInformation(
            "{Name} purged of corruption: -{Amount} (Source: {Source})",
            character.Name, amount, source);

        var previousCorruption = character.Corruption;
        var previousState = GetCorruptionState(previousCorruption);

        // Calculate actual purge (can't go below 0)
        var actualPurge = Math.Min(amount, character.Corruption);
        character.Corruption = Math.Max(0, character.Corruption - amount);

        var newState = GetCorruptionState(character.Corruption);
        var tierChanged = newState.Tier != previousState.Tier;

        if (tierChanged)
        {
            _logger.LogInformation(
                "{Name} corruption tier improved: {OldTier} -> {NewTier}",
                character.Name, previousState.Tier, newState.Tier);
        }

        _logger.LogDebug(
            "{Name} corruption purged: {Previous} -> {Current} (Tier: {Tier})",
            character.Name, previousCorruption, character.Corruption, newState.Tier);

        return new CorruptionResult(
            RawCorruption: -amount,
            NetCorruptionApplied: -actualPurge,
            CurrentTotal: character.Corruption,
            PreviousTier: previousState.Tier,
            NewTier: newState.Tier,
            TierChanged: tierChanged,
            IsTerminal: false,
            Source: source
        );
    }

    /// <inheritdoc/>
    public CorruptionState GetCorruptionState(int corruptionValue)
    {
        return new CorruptionState(corruptionValue);
    }

    /// <inheritdoc/>
    public void HandleTerminalError(Character character)
    {
        // TODO: Implement full terminal mechanics
        // - Lock character as Forlorn
        // - Display Terminal Error modal
        // - Force return to Main Menu
        _logger.LogCritical(
            "TERMINAL ERROR: {Name} has succumbed to the Runic Blight. " +
            "Full Forlorn transformation will be implemented in future version.",
            character.Name);
    }

    #endregion
}
