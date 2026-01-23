// ------------------------------------------------------------------------------
// <copyright file="CulturalProtocolService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Implementation of cultural protocol management for social interactions.
// Manages cant fluency, Veil-Speech, protocol compliance, and violation tracking.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implementation of cultural protocol management for social interactions.
/// </summary>
/// <remarks>
/// <para>
/// This service provides the core functionality for the Cultural Protocol System,
/// managing culture-specific social interaction mechanics including:
/// </para>
/// <list type="bullet">
///   <item><description>Protocol retrieval and management for five cultures</description></item>
///   <item><description>Cant fluency modifiers (None -1d10, Basic +0, Fluent +1d10)</description></item>
///   <item><description>Veil-Speech state tracking and transitions</description></item>
///   <item><description>Protocol compliance checking with violation detection</description></item>
///   <item><description>Protocol violation recording with consequence calculation</description></item>
/// </list>
/// <para>
/// The service maintains in-memory state for Veil-Speech states and violation
/// history. In a full implementation, this state would be persisted to game storage.
/// </para>
/// </remarks>
public class CulturalProtocolService : ICulturalProtocolService
{
    /// <summary>
    /// The logger instance for comprehensive logging of protocol operations.
    /// </summary>
    private readonly ILogger<CulturalProtocolService> _logger;

    /// <summary>
    /// Configuration options for cultural protocols.
    /// </summary>
    private readonly CulturalProtocolOptions _options;

    /// <summary>
    /// Dictionary of loaded cultural protocols, keyed by culture ID (case-insensitive).
    /// </summary>
    private readonly Dictionary<string, CultureProtocol> _protocols;

    /// <summary>
    /// Tracks Veil-Speech states between character-NPC pairs.
    /// Key: (CharacterId, NpcId), Value: Current VeilSpeechState.
    /// </summary>
    private readonly Dictionary<(string CharacterId, string NpcId), VeilSpeechState> _veilSpeechStates;

    /// <summary>
    /// Tracks recorded protocol violations for characters.
    /// Key: (CharacterId, CultureId), Value: List of violation records.
    /// </summary>
    private readonly Dictionary<(string CharacterId, string CultureId), List<ViolationRecord>> _violationHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CulturalProtocolService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for comprehensive logging.</param>
    /// <param name="options">Configuration options for cultural protocols.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger or options is null.</exception>
    public CulturalProtocolService(
        ILogger<CulturalProtocolService> logger,
        IOptions<CulturalProtocolOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _protocols = new Dictionary<string, CultureProtocol>(StringComparer.OrdinalIgnoreCase);
        _veilSpeechStates = new Dictionary<(string, string), VeilSpeechState>();
        _violationHistory = new Dictionary<(string, string), List<ViolationRecord>>();

        // Load protocols from configuration or use defaults
        LoadProtocols();

        _logger.LogInformation(
            "CulturalProtocolService initialized with {ProtocolCount} cultural protocols",
            _protocols.Count);
    }

    #region Protocol Retrieval Methods

    /// <inheritdoc/>
    public CultureProtocol? GetProtocol(string cultureId)
    {
        if (string.IsNullOrWhiteSpace(cultureId))
        {
            _logger.LogWarning("GetProtocol called with null or empty culture ID");
            return null;
        }

        _protocols.TryGetValue(cultureId, out var protocol);

        _logger.LogDebug(
            "GetProtocol for culture '{CultureId}': {Found}",
            cultureId,
            protocol != null ? $"Found ({protocol.ProtocolName})" : "Not found");

        return protocol;
    }

    /// <inheritdoc/>
    public IReadOnlyList<CultureProtocol> GetAllProtocols()
    {
        var protocols = _protocols.Values.ToList().AsReadOnly();

        _logger.LogDebug(
            "GetAllProtocols returning {Count} protocols: {Cultures}",
            protocols.Count,
            string.Join(", ", protocols.Select(p => p.CultureId)));

        return protocols;
    }

    #endregion

    #region Cant Modifier Methods

    /// <inheritdoc/>
    public CantModifier GetCantModifier(CantFluency fluency, string cultureId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        var protocol = GetProtocol(cultureId);
        var cantName = protocol?.CantName ?? "Common";

        var modifier = CantModifier.FromFluency(fluency, cultureId, cantName);

        _logger.LogDebug(
            "GetCantModifier for culture '{CultureId}': Fluency {Fluency} = {Modifier}d10, Cant '{CantName}'",
            cultureId,
            fluency.GetDisplayName(),
            modifier.DiceModifier,
            cantName);

        if (modifier.IsPenalty)
        {
            _logger.LogInformation(
                "Character lacks fluency in {CantName}. Social interaction penalty: {Modifier}d10",
                cantName,
                modifier.DiceModifier);
        }
        else if (modifier.IsBonus)
        {
            _logger.LogInformation(
                "Character is fluent in {CantName}. Social interaction bonus: +{Modifier}d10",
                cantName,
                modifier.DiceModifier);
        }

        return modifier;
    }

    #endregion

    #region Protocol Compliance Methods

    /// <inheritdoc/>
    public ProtocolCheckResult CheckProtocolCompliance(
        string characterId,
        string cultureId,
        SocialContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        _logger.LogInformation(
            "Checking protocol compliance: Character '{CharacterId}' with culture '{CultureId}', " +
            "Interaction type: {InteractionType}",
            characterId,
            cultureId,
            context.InteractionType);

        // Get the protocol for this culture
        var protocol = GetProtocol(cultureId);
        if (protocol == null)
        {
            _logger.LogDebug(
                "No protocol found for culture '{CultureId}'. Returning neutral result.",
                cultureId);
            return ProtocolCheckResult.NoProtocol();
        }

        var modifiers = new List<SocialModifier>();

        // Get cant modifier (would typically look up character's cant fluency)
        var cantFluency = GetCharacterCantFluency(characterId, cultureId);
        var cantModifier = GetCantModifier(cantFluency, cultureId);
        modifiers.Add(cantModifier.ToSocialModifier());

        _logger.LogDebug(
            "Cant modifier applied: {Fluency} = {Modifier}d10",
            cantFluency.GetDisplayName(),
            cantModifier.DiceModifier);

        // Check for Veil-Speech special rules (Utgard)
        if (protocol.IsVeilSpeech)
        {
            _logger.LogDebug(
                "Protocol '{ProtocolName}' requires Veil-Speech. Applying special rules.",
                protocol.ProtocolName);

            var veilSpeechContext = ApplyVeilSpeechLogic(
                context.TargetId,
                context.InteractionType == SocialInteractionType.Deception,
                isDirectTruth: false, // Would be determined from dialogue choice
                isProperVeilSpeech: false);

            modifiers.Add(veilSpeechContext.ToSocialModifier());
        }

        // Check for previous violations affecting this interaction
        var violationPenalty = GetViolationPenalty(characterId, cultureId);
        if (violationPenalty.DcIncrease > 0 || violationPenalty.DicePenalty > 0)
        {
            _logger.LogInformation(
                "Previous violations affect interaction: +{DcIncrease} DC, -{DicePenalty}d10",
                violationPenalty.DcIncrease,
                violationPenalty.DicePenalty);

            modifiers.Add(new SocialModifier(
                Source: "Previous Violations",
                Description: $"Past protocol violations (+{violationPenalty.DcIncrease} DC, -{violationPenalty.DicePenalty}d10)",
                DiceModifier: -violationPenalty.DicePenalty,
                DcModifier: violationPenalty.DcIncrease,
                ModifierType: SocialModifierType.Cultural,
                AppliesToInteractionTypes: null));
        }

        // Evaluate requirements compliance
        var complianceResult = EvaluateRequirements(protocol, context, characterId);

        if (complianceResult.IsCompliant)
        {
            _logger.LogInformation(
                "Character '{CharacterId}' complied with '{ProtocolName}' protocol. " +
                "Total modifiers: {DcMod:+0;-0;0} DC, {DiceMod:+0;-0;0}d10",
                characterId,
                protocol.ProtocolName,
                complianceResult.TotalDcModifier,
                complianceResult.TotalDiceModifier);

            return ProtocolCheckResult.Compliant(protocol.CultureId, protocol.ProtocolName, modifiers);
        }

        // Violation occurred
        _logger.LogWarning(
            "Character '{CharacterId}' violated '{ProtocolName}' protocol: {ViolationType}. " +
            "Consequences: +{DcIncrease} DC, -{DicePenalty}d10",
            characterId,
            protocol.ProtocolName,
            complianceResult.ViolationType.GetDisplayName(),
            complianceResult.ViolationType.GetDcIncrease(),
            complianceResult.ViolationType.GetDicePenalty());

        return ProtocolCheckResult.Violation(
            protocol.CultureId,
            protocol.ProtocolName,
            complianceResult.ViolationType,
            null,
            modifiers);
    }

    #endregion

    #region Veil-Speech Methods

    /// <inheritdoc/>
    public VeilSpeechContext ApplyVeilSpeechLogic(
        string npcId,
        bool isDeception,
        bool isDirectTruth,
        bool isProperVeilSpeech)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        // Get current Veil-Speech state with this NPC
        var currentState = GetVeilSpeechState(string.Empty, npcId);

        _logger.LogDebug(
            "Applying Veil-Speech logic for NPC '{NpcId}': CurrentState={State}, " +
            "Deception={IsDeception}, DirectTruth={IsDirectTruth}, ProperVeilSpeech={IsProper}",
            npcId,
            currentState.GetDisplayName(),
            isDeception,
            isDirectTruth,
            isProperVeilSpeech);

        // Create the Veil-Speech context
        var context = VeilSpeechContext.Create(
            currentState,
            npcId,
            isDeception,
            isDirectTruth,
            isProperVeilSpeech);

        var dcAdjustment = context.GetDcAdjustment();
        var diceModifier = context.GetDiceModifier();

        _logger.LogInformation(
            "Veil-Speech context for NPC '{NpcId}': DC adjustment {DcAdj:+0;-0;0}, " +
            "Dice modifier {DiceMod:+0;-0;0}d10, Approach: '{Approach}'",
            npcId,
            dcAdjustment,
            diceModifier,
            context.GetApproachDescription());

        if (isDeception)
        {
            _logger.LogDebug(
                "Deception within Veil-Speech: DC reduced by {Reduction} (lies within veil-speech are respected)",
                VeilSpeechContext.DeceptionDcReduction);
        }

        if (isDirectTruth)
        {
            _logger.LogWarning(
                "Direct truth told to Utgard NPC '{NpcId}'. Penalty: {Penalty}d10. " +
                "The Utgard consider this a serious breach of etiquette.",
                npcId,
                -VeilSpeechContext.DirectTruthPenalty);
        }

        if (isProperVeilSpeech)
        {
            _logger.LogDebug(
                "Proper Veil-Speech used with NPC '{NpcId}'. Bonus: +{Bonus}d10. " +
                "The NPC appreciates your cultural sophistication.",
                npcId,
                VeilSpeechContext.ProperVeilSpeechBonus);
        }

        return context;
    }

    /// <inheritdoc/>
    public VeilSpeechState GetVeilSpeechState(string characterId, string npcId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        // Use empty string for characterId if not provided (for backwards compatibility)
        var effectiveCharacterId = string.IsNullOrWhiteSpace(characterId) ? "_default_" : characterId;
        var key = (effectiveCharacterId, npcId);

        var state = _veilSpeechStates.TryGetValue(key, out var existingState)
            ? existingState
            : VeilSpeechState.Neutral;

        _logger.LogDebug(
            "GetVeilSpeechState for Character '{CharacterId}', NPC '{NpcId}': {State}",
            effectiveCharacterId,
            npcId,
            state.GetDisplayName());

        return state;
    }

    /// <inheritdoc/>
    public void UpdateVeilSpeechState(string characterId, string npcId, VeilSpeechState newState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(npcId, nameof(npcId));

        var key = (characterId, npcId);
        var previousState = _veilSpeechStates.TryGetValue(key, out var existingState)
            ? existingState
            : VeilSpeechState.Neutral;

        _veilSpeechStates[key] = newState;

        _logger.LogInformation(
            "Veil-Speech state updated: Character '{CharacterId}' with NPC '{NpcId}': " +
            "{PreviousState} -> {NewState}",
            characterId,
            npcId,
            previousState.GetDisplayName(),
            newState.GetDisplayName());

        // Log warnings for negative state transitions
        if (newState == VeilSpeechState.Offended && previousState == VeilSpeechState.Neutral)
        {
            _logger.LogWarning(
                "Character '{CharacterId}' has offended Utgard NPC '{NpcId}'. " +
                "Future interactions will suffer -2d10 penalty until amends are made.",
                characterId,
                npcId);
        }
        else if (newState == VeilSpeechState.DeepOffense)
        {
            _logger.LogError(
                "Character '{CharacterId}' has deeply offended Utgard NPC '{NpcId}'. " +
                "Interaction may be blocked. Recovery requires extraordinary measures.",
                characterId,
                npcId);
        }
        else if (newState == VeilSpeechState.Respected && previousState != VeilSpeechState.Respected)
        {
            _logger.LogInformation(
                "Character '{CharacterId}' has earned respect from Utgard NPC '{NpcId}' " +
                "through proper Veil-Speech. Future interactions receive +1d10 bonus.",
                characterId,
                npcId);
        }
    }

    #endregion

    #region Violation Recording Methods

    /// <inheritdoc/>
    public void RecordProtocolViolation(
        string characterId,
        string cultureId,
        ProtocolViolationType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        var key = (characterId, cultureId);

        if (!_violationHistory.TryGetValue(key, out var violations))
        {
            violations = new List<ViolationRecord>();
            _violationHistory[key] = violations;
        }

        var record = new ViolationRecord(
            ViolationType: type,
            Timestamp: DateTime.UtcNow,
            IsRecovered: false);

        violations.Add(record);

        _logger.LogWarning(
            "Protocol violation recorded: Character '{CharacterId}', Culture '{CultureId}', " +
            "Type: {ViolationType} ({DisplayName}). " +
            "Consequences: +{DcIncrease} DC, -{DicePenalty}d10, {DispositionChange:+0;-0} disposition",
            characterId,
            cultureId,
            type,
            type.GetDisplayName(),
            type.GetDcIncrease(),
            type.GetDicePenalty(),
            type.GetDispositionChange());

        if (type.BlocksInteraction())
        {
            _logger.LogError(
                "UNFORGIVABLE VIOLATION: Character '{CharacterId}' has committed an unforgivable " +
                "transgression against {CultureId} culture. Interaction is blocked. " +
                "Faction reputation may be affected.",
                characterId,
                cultureId);
        }

        if (type.MayTriggerHostility())
        {
            _logger.LogWarning(
                "Severe violation may trigger hostility from {CultureId} NPCs. " +
                "Character '{CharacterId}' should exercise extreme caution.",
                cultureId,
                characterId);
        }

        _logger.LogDebug(
            "Total violations for Character '{CharacterId}' with Culture '{CultureId}': {Count}",
            characterId,
            cultureId,
            violations.Count);
    }

    #endregion

    #region Protocol Requirement Methods

    /// <inheritdoc/>
    public IReadOnlyList<ProtocolRequirement> GetRequirements(string cultureId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        var protocol = GetProtocol(cultureId);
        if (protocol == null)
        {
            _logger.LogDebug(
                "No protocol found for culture '{CultureId}'. Returning empty requirements.",
                cultureId);
            return Array.Empty<ProtocolRequirement>();
        }

        var requirements = protocol.DetailedRequirements;

        _logger.LogDebug(
            "GetRequirements for culture '{CultureId}': {TotalCount} total " +
            "({MandatoryCount} mandatory, {OptionalCount} optional)",
            cultureId,
            requirements.Count,
            requirements.Count(r => r.IsMandatory),
            requirements.Count(r => !r.IsMandatory));

        return requirements;
    }

    /// <inheritdoc/>
    public bool HasSpecialProtocol(string cultureId, SpecialProtocolType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        var protocol = GetProtocol(cultureId);
        if (protocol == null)
        {
            _logger.LogDebug(
                "HasSpecialProtocol: No protocol for culture '{CultureId}'",
                cultureId);
            return false;
        }

        var hasProtocol = protocol.SpecialRuleType == type;

        _logger.LogDebug(
            "HasSpecialProtocol: Culture '{CultureId}' {Has} {ProtocolType}",
            cultureId,
            hasProtocol ? "has" : "does not have",
            type);

        return hasProtocol;
    }

    #endregion

    #region DC and Dice Pool Calculation Methods

    /// <inheritdoc/>
    public int CalculateEffectiveDc(
        string cultureId,
        ProtocolCheckResult compliance,
        CantModifier? cantModifier,
        VeilSpeechContext? veilSpeechContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureId, nameof(cultureId));

        var protocol = GetProtocol(cultureId);
        var baseDc = protocol?.BaseDc ?? 15; // Default DC if no protocol found

        _logger.LogDebug(
            "Calculating effective DC for culture '{CultureId}': Base DC {BaseDc}",
            cultureId,
            baseDc);

        var totalDcModifier = 0;

        // Add protocol compliance DC modifier
        totalDcModifier += compliance.TotalDcModifier;
        _logger.LogDebug(
            "Protocol compliance DC modifier: {Modifier:+0;-0;0}",
            compliance.TotalDcModifier);

        // Add Veil-Speech DC adjustment (if applicable)
        if (veilSpeechContext.HasValue)
        {
            var veilDcAdj = veilSpeechContext.Value.GetDcAdjustment();
            totalDcModifier += veilDcAdj;
            _logger.LogDebug(
                "Veil-Speech DC adjustment: {Adjustment:+0;-0;0}",
                veilDcAdj);
        }

        var effectiveDc = baseDc + totalDcModifier;

        _logger.LogInformation(
            "Effective DC for culture '{CultureId}': {BaseDc} base + {Modifier:+0;-0;0} modifiers = {EffectiveDc}",
            cultureId,
            baseDc,
            totalDcModifier,
            effectiveDc);

        return effectiveDc;
    }

    /// <inheritdoc/>
    public int CalculateDiceModifier(
        CantModifier? cantModifier,
        VeilSpeechContext? veilSpeechContext,
        ProtocolCheckResult compliance)
    {
        var totalModifier = 0;

        _logger.LogDebug("Calculating total dice modifier from protocol sources");

        // Add cant modifier
        if (cantModifier.HasValue)
        {
            totalModifier += cantModifier.Value.DiceModifier;
            _logger.LogDebug(
                "Cant modifier: {Modifier:+0;-0;0}d10 ({CantName})",
                cantModifier.Value.DiceModifier,
                cantModifier.Value.CantName);
        }

        // Add Veil-Speech modifier
        if (veilSpeechContext.HasValue)
        {
            var veilMod = veilSpeechContext.Value.GetDiceModifier();
            totalModifier += veilMod;
            _logger.LogDebug(
                "Veil-Speech modifier: {Modifier:+0;-0;0}d10",
                veilMod);
        }

        // Add compliance modifier (typically penalties from violations)
        totalModifier += compliance.TotalDiceModifier;
        _logger.LogDebug(
            "Compliance modifier: {Modifier:+0;-0;0}d10",
            compliance.TotalDiceModifier);

        _logger.LogInformation(
            "Total dice modifier from cultural protocols: {Modifier:+0;-0;0}d10",
            totalModifier);

        return totalModifier;
    }

    #endregion

    #region State Management Methods

    /// <inheritdoc/>
    public void ClearCharacterState(string characterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId, nameof(characterId));

        _logger.LogInformation(
            "Clearing all protocol state for character '{CharacterId}'",
            characterId);

        // Remove Veil-Speech states
        var veilSpeechKeysToRemove = _veilSpeechStates.Keys
            .Where(k => k.CharacterId == characterId)
            .ToList();

        foreach (var key in veilSpeechKeysToRemove)
        {
            _veilSpeechStates.Remove(key);
        }

        _logger.LogDebug(
            "Removed {Count} Veil-Speech state entries for character '{CharacterId}'",
            veilSpeechKeysToRemove.Count,
            characterId);

        // Remove violation history
        var violationKeysToRemove = _violationHistory.Keys
            .Where(k => k.CharacterId == characterId)
            .ToList();

        foreach (var key in violationKeysToRemove)
        {
            _violationHistory.Remove(key);
        }

        _logger.LogDebug(
            "Removed {Count} violation history entries for character '{CharacterId}'",
            violationKeysToRemove.Count,
            characterId);

        _logger.LogInformation(
            "Protocol state cleared for character '{CharacterId}': " +
            "{VeilSpeechCount} Veil-Speech states, {ViolationCount} violation records removed",
            characterId,
            veilSpeechKeysToRemove.Count,
            violationKeysToRemove.Count);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Loads cultural protocols from configuration or initializes defaults.
    /// </summary>
    private void LoadProtocols()
    {
        _logger.LogDebug("Loading cultural protocols from configuration");

        // Load from options if available
        if (_options.Protocols != null && _options.Protocols.Count > 0)
        {
            foreach (var config in _options.Protocols)
            {
                try
                {
                    var specialType = config.SpecialRuleType ?? SpecialProtocolType.None;

                    var protocol = CultureProtocol.Create(
                        cultureId: config.CultureId,
                        cultureName: config.CultureId, // Use cultureId as cultureName if not configured
                        protocolName: config.ProtocolName,
                        baseDc: config.BaseDc,
                        requirements: config.Requirements,
                        violationConsequence: config.ViolationConsequence,
                        cantName: config.CantName,
                        specialRuleType: specialType);

                    _protocols[config.CultureId] = protocol;

                    _logger.LogDebug(
                        "Loaded protocol '{ProtocolName}' for culture '{CultureId}' (DC {BaseDc})",
                        config.ProtocolName,
                        config.CultureId,
                        config.BaseDc);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to load protocol for culture '{CultureId}'",
                        config.CultureId);
                }
            }
        }

        // If no protocols loaded from config, use defaults
        if (_protocols.Count == 0)
        {
            _logger.LogInformation(
                "No protocols found in configuration. Loading default cultural protocols.");
            LoadDefaultProtocols();
        }

        _logger.LogInformation(
            "Loaded {Count} cultural protocols: {Cultures}",
            _protocols.Count,
            string.Join(", ", _protocols.Keys));
    }

    /// <summary>
    /// Loads the default cultural protocols for all five cultures.
    /// </summary>
    private void LoadDefaultProtocols()
    {
        _logger.LogDebug("Loading default protocols for all cultures");

        // Dvergr - Logic-Chain (DC 18)
        _protocols["dvergr"] = CultureProtocol.Dvergr();
        _logger.LogDebug("Loaded Dvergr protocol (Logic-Chain, DC 18)");

        // Utgard - Veil-Speech (DC 16)
        _protocols["utgard"] = CultureProtocol.Utgard();
        _logger.LogDebug("Loaded Utgard protocol (Veil-Speech, DC 16)");

        // Gorge-Maw - Hospitality Rite (DC 14)
        _protocols["gorge-maw"] = CultureProtocol.GorgeMaw();
        _logger.LogDebug("Loaded Gorge-Maw protocol (Hospitality Rite, DC 14)");

        // Rune-Lupin - Telepathy (DC 12)
        _protocols["rune-lupin"] = CultureProtocol.RuneLupin();
        _logger.LogDebug("Loaded Rune-Lupin protocol (Telepathy, DC 12)");

        // Iron-Bane - Martial Tribute (DC 16)
        _protocols["iron-bane"] = CultureProtocol.IronBane();
        _logger.LogDebug("Loaded Iron-Bane protocol (Martial Tribute/Blood Oath, DC 16)");

        _logger.LogInformation("Loaded {Count} default cultural protocols", _protocols.Count);
    }

    /// <summary>
    /// Gets the character's cant fluency for a specific culture.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="cultureId">The culture identifier.</param>
    /// <returns>The character's fluency level in the culture's cant.</returns>
    /// <remarks>
    /// In a full implementation, this would look up the character's cant knowledge
    /// from game state. For now, returns Basic as the default.
    /// </remarks>
    private CantFluency GetCharacterCantFluency(string characterId, string cultureId)
    {
        // In full implementation, would look up character's cant knowledge
        _logger.LogDebug(
            "GetCharacterCantFluency for Character '{CharacterId}', Culture '{CultureId}': " +
            "Returning default Basic fluency (full implementation would check character data)",
            characterId,
            cultureId);

        return CantFluency.Basic;
    }

    /// <summary>
    /// Gets the cumulative penalty from previous violations.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="cultureId">The culture identifier.</param>
    /// <returns>A tuple of (DcIncrease, DicePenalty) from unrecovered violations.</returns>
    private (int DcIncrease, int DicePenalty) GetViolationPenalty(string characterId, string cultureId)
    {
        var key = (characterId, cultureId);

        if (!_violationHistory.TryGetValue(key, out var violations))
        {
            return (0, 0);
        }

        // Sum penalties from unrecovered violations
        var unrecoveredViolations = violations.Where(v => !v.IsRecovered).ToList();

        var dcIncrease = unrecoveredViolations.Sum(v => v.ViolationType.GetDcIncrease());
        var dicePenalty = unrecoveredViolations.Sum(v => v.ViolationType.GetDicePenalty());

        _logger.LogDebug(
            "GetViolationPenalty for Character '{CharacterId}', Culture '{CultureId}': " +
            "{ViolationCount} unrecovered violations, +{DcIncrease} DC, -{DicePenalty}d10",
            characterId,
            cultureId,
            unrecoveredViolations.Count,
            dcIncrease,
            dicePenalty);

        return (dcIncrease, dicePenalty);
    }

    /// <summary>
    /// Evaluates protocol requirements against the current context.
    /// </summary>
    /// <param name="protocol">The cultural protocol.</param>
    /// <param name="context">The social context.</param>
    /// <param name="characterId">The character identifier.</param>
    /// <returns>A compliance result with any detected violations.</returns>
    private ProtocolCheckResult EvaluateRequirements(
        CultureProtocol protocol,
        SocialContext context,
        string characterId)
    {
        _logger.LogDebug(
            "Evaluating requirements for protocol '{ProtocolName}' ({RequirementCount} requirements)",
            protocol.ProtocolName,
            protocol.DetailedRequirements.Count);

        // In full implementation, would check each requirement against context
        // For now, assume compliance unless specific conditions are met
        var mandatoryRequirements = protocol.GetMandatoryRequirements();

        foreach (var req in mandatoryRequirements)
        {
            _logger.LogDebug(
                "Checking mandatory requirement: {RequirementText} ({Type})",
                req.RequirementText,
                req.RequirementType);
        }

        // For now, return compliant (full implementation would evaluate each requirement)
        _logger.LogDebug(
            "Requirement evaluation complete: Assuming compliance (full implementation pending)");

        return ProtocolCheckResult.Compliant(protocol.CultureId, protocol.ProtocolName, new List<SocialModifier>());
    }

    #endregion

    #region Internal Record Types

    /// <summary>
    /// Record for tracking protocol violations.
    /// </summary>
    /// <param name="ViolationType">The type of violation.</param>
    /// <param name="Timestamp">When the violation occurred.</param>
    /// <param name="IsRecovered">Whether the violation has been recovered from.</param>
    private sealed record ViolationRecord(
        ProtocolViolationType ViolationType,
        DateTime Timestamp,
        bool IsRecovered);

    #endregion
}
