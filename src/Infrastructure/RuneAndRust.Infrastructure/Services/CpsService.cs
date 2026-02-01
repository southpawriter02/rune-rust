// ═══════════════════════════════════════════════════════════════════════════════
// CpsService.cs
// Implementation of the Cognitive Paradox Syndrome (CPS) service. CPS tracks
// mental deterioration from processing reality-bending paradoxes, derived from
// Psychic Stress. Unlike Corruption (physical taint), CPS can recover when
// stress is reduced through rest or abilities.
// Version: 0.18.2d
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Infrastructure.Services;

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Implementation of the CPS (Cognitive Paradox Syndrome) service.
/// </summary>
/// <remarks>
/// <para>
/// CpsService manages all aspects of character mental deterioration
/// from processing reality-bending paradoxes. It integrates with:
/// </para>
/// <list type="bullet">
///   <item><description>IStressService — for current stress values</description></item>
///   <item><description>IDiceService — for Panic Table rolls</description></item>
///   <item><description>IStatusEffectService — for applying panic effects</description></item>
/// </list>
/// <para>
/// <strong>CPS Stages:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>None (0-19 stress): Clear-minded, no symptoms</description></item>
///   <item><description>WeightOfKnowing (20-39 stress): Reality feels "off"</description></item>
///   <item><description>GlimmerMadness (40-59 stress): Reality flickers and distorts</description></item>
///   <item><description>RuinMadness (60-79 stress): Panic Table active, mind fracturing</description></item>
///   <item><description>HollowShell (80+ stress): Terminal state, survival check required</description></item>
/// </list>
/// </remarks>
/// <seealso cref="ICpsService"/>
/// <seealso cref="CpsState"/>
/// <seealso cref="CpsStage"/>
/// <seealso cref="PanicEffect"/>
public sealed class CpsService : ICpsService
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════════════════

    #region Dependencies

    /// <summary>
    /// Service for querying character stress values.
    /// </summary>
    private readonly IStressService _stressService;

    /// <summary>
    /// Service for dice rolling (d10 for Panic Table).
    /// </summary>
    private readonly IDiceService _diceService;

    /// <summary>
    /// Service for applying status effects from panic results.
    /// </summary>
    private readonly IStatusEffectService _statusEffectService;

    /// <summary>
    /// Logger for CPS operations.
    /// </summary>
    private readonly ILogger<CpsService> _logger;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // CONFIGURATION DATA
    // ═══════════════════════════════════════════════════════════════════════════

    #region Configuration Data

    /// <summary>
    /// Panic Table entries loaded from configuration, indexed by d10 roll.
    /// </summary>
    private readonly PanicTableEntry[] _panicTable;

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CpsService"/> class.
    /// </summary>
    /// <param name="stressService">Service for stress value queries.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="statusEffectService">Service for status effect application.</param>
    /// <param name="logger">Logger for CPS operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required dependency is null.
    /// </exception>
    public CpsService(
        IStressService stressService,
        IDiceService diceService,
        IStatusEffectService statusEffectService,
        ILogger<CpsService> logger)
    {
        _stressService = stressService ?? throw new ArgumentNullException(nameof(stressService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _statusEffectService = statusEffectService ?? throw new ArgumentNullException(nameof(statusEffectService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize Panic Table with default entries
        // In the future, this could be loaded from config/panic-table.json
        _panicTable = GetDefaultPanicTable();

        _logger.LogInformation(
            "CpsService initialized with {PanicEntries} panic table entries",
            _panicTable.Length);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // STATE QUERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region State Query Methods

    /// <inheritdoc/>
    /// <remarks>
    /// Retrieves the character's current Psychic Stress from IStressService
    /// and constructs an immutable CpsState snapshot.
    /// </remarks>
    public CpsState GetCpsState(Guid characterId)
    {
        _logger.LogDebug(
            "Retrieving CPS state for character {CharacterId}",
            characterId);

        // Get current stress from stress service (StressState.CurrentStress)
        var stressState = _stressService.GetStressState(characterId);
        var stress = stressState.CurrentStress;

        // Create CPS state from stress value
        var state = CpsState.Create(stress);

        _logger.LogDebug(
            "CPS state for {CharacterId}: Stage={Stage}, Stress={Stress}, RequiresPanicCheck={RequiresPanicCheck}",
            characterId,
            state.Stage,
            state.CurrentStress,
            state.RequiresPanicCheck);

        return state;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience method that returns only the CPS stage without
    /// constructing the full CpsState snapshot.
    /// </remarks>
    public CpsStage GetCurrentStage(Guid characterId)
    {
        _logger.LogDebug(
            "Getting current CPS stage for character {CharacterId}",
            characterId);

        var state = GetCpsState(characterId);
        return state.Stage;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns UI distortion parameters based on the character's current
    /// CPS stage. Effects intensify as the stage worsens.
    /// </remarks>
    public CpsUiEffects GetUiEffects(Guid characterId)
    {
        _logger.LogDebug(
            "Getting UI effects for character {CharacterId}",
            characterId);

        var stage = GetCurrentStage(characterId);
        var effects = CpsUiEffects.ForStage(stage);

        _logger.LogDebug(
            "UI effects for {CharacterId}: Stage={Stage}, DistortionIntensity={Distortion:F2}, TextGlitching={TextGlitching}",
            characterId,
            stage,
            effects.DistortionIntensity,
            effects.TextGlitching);

        return effects;
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // STAGE TRANSITION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Stage Transition Methods

    /// <inheritdoc/>
    /// <remarks>
    /// Compares two stress values to detect CPS stage transitions.
    /// Critical transitions (entering RuinMadness or HollowShell) are
    /// logged at higher severity levels.
    /// </remarks>
    public CpsStageChangeResult CheckStageChange(Guid characterId, int previousStress, int newStress)
    {
        _logger.LogDebug(
            "Checking stage change for {CharacterId}: {PreviousStress} → {NewStress}",
            characterId,
            previousStress,
            newStress);

        // Create result from stress change (uses CpsState.DetermineStage internally)
        var result = CpsStageChangeResult.FromStressChange(previousStress, newStress);

        // Log based on transition type
        if (result.StageChanged)
        {
            if (result.EnteredHollowShell)
            {
                // Terminal state - log as error
                _logger.LogError(
                    "CRITICAL: Character {CharacterId} entered Hollow Shell - survival check required. " +
                    "Previous stage: {PreviousStage}, New stage: {NewStage}",
                    characterId,
                    result.PreviousStage,
                    result.NewStage);
            }
            else if (result.EnteredRuinMadness)
            {
                // Critical transition - log as warning
                _logger.LogWarning(
                    "Character {CharacterId} entered Ruin-Madness - Panic Table now active. " +
                    "Previous stage: {PreviousStage}, New stage: {NewStage}",
                    characterId,
                    result.PreviousStage,
                    result.NewStage);
            }
            else if (result.IsCriticalTransition)
            {
                // Other critical transitions
                _logger.LogWarning(
                    "CPS stage change for {CharacterId}: {PreviousStage} → {NewStage} (critical transition)",
                    characterId,
                    result.PreviousStage,
                    result.NewStage);
            }
            else
            {
                // Normal transition
                _logger.LogInformation(
                    "CPS stage change for {CharacterId}: {PreviousStage} → {NewStage}",
                    characterId,
                    result.PreviousStage,
                    result.NewStage);
            }
        }
        else
        {
            _logger.LogDebug(
                "No stage change for {CharacterId}: remaining in {Stage}",
                characterId,
                result.NewStage);
        }

        return result;
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // PANIC TABLE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Panic Table Methods

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// The Panic Table is only available when the character is in RuinMadness
    /// or HollowShell stage. Attempting to roll for characters in lower stages
    /// throws an InvalidOperationException.
    /// </para>
    /// <para>
    /// Uses IDiceService for the d10 roll to ensure consistent randomization
    /// across the game system.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the character is not in RuinMadness or HollowShell stage.
    /// </exception>
    public PanicResult RollPanicTable(Guid characterId)
    {
        _logger.LogDebug(
            "Rolling Panic Table for character {CharacterId}",
            characterId);

        // Check if character is in a stage that requires panic rolls
        var stage = GetCurrentStage(characterId);

        if (stage < CpsStage.RuinMadness)
        {
            _logger.LogWarning(
                "Attempted Panic Table roll for {CharacterId} in {Stage} (requires RuinMadness+)",
                characterId,
                stage);

            throw new InvalidOperationException(
                $"Cannot roll Panic Table: character is in {stage}, requires RuinMadness or higher");
        }

        // Roll d10 via dice service
        var rollResult = _diceService.Roll(DiceType.D10);
        var roll = rollResult.Total;

        _logger.LogDebug(
            "Panic Table d10 roll for {CharacterId}: {Roll}",
            characterId,
            roll);

        // Look up the effect for this roll
        var entry = GetPanicEntryForRoll(roll);

        // Create the panic result
        var panicResult = CreatePanicResult(roll, entry);

        // Log the result
        if (panicResult.IsLuckyBreak)
        {
            _logger.LogInformation(
                "Panic Table roll for {CharacterId}: d10={Roll} → Lucky Break (no effect)",
                characterId,
                roll);
        }
        else
        {
            _logger.LogInformation(
                "Panic Table roll for {CharacterId}: d10={Roll} → {Effect} ({EffectName})",
                characterId,
                roll,
                panicResult.Effect,
                panicResult.EffectName);
        }

        return panicResult;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Applies the mechanical effects from a PanicResult:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Status effects (Stunned, Prone, Unconscious) via IStatusEffectService</description></item>
    ///   <item><description>Forced actions (flee, attack nearest) are flagged for combat system</description></item>
    /// </list>
    /// <para>
    /// Lucky Break results (roll 10) have no mechanical effect and are logged but not applied.
    /// </para>
    /// </remarks>
    public void ApplyPanicEffect(Guid characterId, PanicResult panicResult)
    {
        _logger.LogDebug(
            "Applying panic effect {Effect} to character {CharacterId}",
            panicResult.Effect,
            characterId);

        // Lucky break has no effect to apply
        if (panicResult.IsLuckyBreak)
        {
            _logger.LogInformation(
                "Lucky break for {CharacterId} - no panic effect applied",
                characterId);
            return;
        }

        // Apply each status effect via the status effect service
        foreach (var statusEffect in panicResult.StatusEffects)
        {
            var duration = panicResult.DurationTurns ?? 1;

            _statusEffectService.ApplyEffect(characterId, statusEffect, duration);

            _logger.LogDebug(
                "Applied status effect {Effect} to {CharacterId} for {Duration} turns",
                statusEffect,
                characterId,
                duration);
        }

        // Handle forced actions (integration point for combat system)
        if (panicResult.ForcesAction)
        {
            _logger.LogInformation(
                "Queuing forced action {ActionType} for {CharacterId}",
                panicResult.ForcedActionType,
                characterId);

            // Note: Forced action handling would integrate with combat system
            // This is a hook for future combat integration (v0.18.5+)
        }

        _logger.LogInformation(
            "Applied panic effect {Effect} to {CharacterId}",
            panicResult.EffectName,
            characterId);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // RECOVERY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Recovery Methods

    /// <inheritdoc/>
    /// <remarks>
    /// Recovery is possible from None, WeightOfKnowing, and GlimmerMadness stages.
    /// Once a character enters RuinMadness or HollowShell, normal recovery is not
    /// possible — special intervention (GM discretion, Sanctuary Rest) may be required.
    /// </remarks>
    public bool IsRecoverable(Guid characterId)
    {
        var stage = GetCurrentStage(characterId);
        var isRecoverable = stage < CpsStage.RuinMadness;

        _logger.LogDebug(
            "Recovery check for {CharacterId}: Stage={Stage}, Recoverable={IsRecoverable}",
            characterId,
            stage,
            isRecoverable);

        return isRecoverable;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the appropriate recovery protocol for the character's current
    /// CPS stage. Each stage has specific steps and urgency levels.
    /// </remarks>
    public CpsRecoveryProtocol GetRecoveryProtocol(Guid characterId)
    {
        var stage = GetCurrentStage(characterId);
        var protocol = CpsRecoveryProtocol.ForStage(stage);

        _logger.LogDebug(
            "Recovery protocol for {CharacterId}: Stage={Stage}, Protocol={ProtocolName}, Urgency={Urgency}",
            characterId,
            stage,
            protocol.ProtocolName,
            protocol.Urgency);

        return protocol;
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    #region Private Methods

    /// <summary>
    /// Gets the Panic Table entry for a given d10 roll.
    /// </summary>
    /// <param name="roll">The d10 roll result (1-10).</param>
    /// <returns>The matching PanicTableEntry.</returns>
    private PanicTableEntry GetPanicEntryForRoll(int roll)
    {
        // Find the entry where roll falls within min-max range
        var entry = _panicTable.FirstOrDefault(e => roll >= e.MinRoll && roll <= e.MaxRoll);

        // Return entry or fallback to default (Lucky Break)
        return entry ?? new PanicTableEntry
        {
            MinRoll = roll,
            MaxRoll = roll,
            Effect = PanicEffect.None,
            EffectName = "Unknown",
            Description = "An unexpected result occurred."
        };
    }

    /// <summary>
    /// Creates a PanicResult from a d10 roll and Panic Table entry.
    /// </summary>
    /// <param name="roll">The d10 roll result.</param>
    /// <param name="entry">The matching Panic Table entry.</param>
    /// <returns>A new PanicResult with all effect data.</returns>
    private static PanicResult CreatePanicResult(int roll, PanicTableEntry entry)
    {
        return new PanicResult(
            DieRoll: roll,
            Effect: entry.Effect,
            EffectName: entry.EffectName,
            Description: entry.Description,
            DurationTurns: entry.DurationTurns,
            SelfDamage: entry.SelfDamage,
            StatusEffects: entry.StatusEffects ?? Array.Empty<string>(),
            ForcesAction: entry.ForcesAction,
            ForcedActionType: entry.ForcedActionType);
    }

    /// <summary>
    /// Returns the default Panic Table entries matching the design specification.
    /// </summary>
    /// <remarks>
    /// These entries can be loaded from config/panic-table.json in a future version.
    /// For now, they are hardcoded to match the design specification.
    /// </remarks>
    /// <returns>Array of PanicTableEntry for d10 rolls 1-10.</returns>
    private static PanicTableEntry[] GetDefaultPanicTable()
    {
        return new[]
        {
            // Roll 1: Frozen (Logic Lock)
            new PanicTableEntry
            {
                MinRoll = 1,
                MaxRoll = 1,
                Effect = PanicEffect.Frozen,
                EffectName = "Logic Lock",
                Description = "Your mind freezes, unable to process the paradox before you.",
                DurationTurns = 1,
                StatusEffects = new[] { "Stunned" },
                ForcesAction = false
            },

            // Roll 2: Scream (Involuntary Scream)
            new PanicTableEntry
            {
                MinRoll = 2,
                MaxRoll = 2,
                Effect = PanicEffect.Scream,
                EffectName = "Involuntary Scream",
                Description = "A scream tears from your throat before you can stop it.",
                StatusEffects = Array.Empty<string>(),
                ForcesAction = false
                // Note: Alerts enemies, stealth impossible - handled by combat system
            },

            // Roll 3: Flee (Evacuation Protocol)
            new PanicTableEntry
            {
                MinRoll = 3,
                MaxRoll = 3,
                Effect = PanicEffect.Flee,
                EffectName = "Evacuation Protocol",
                Description = "Your survival instincts override all else. You MUST flee.",
                StatusEffects = Array.Empty<string>(),
                ForcesAction = true,
                ForcedActionType = "FleeFromSource"
            },

            // Roll 4: Fetal (Fetal Position)
            new PanicTableEntry
            {
                MinRoll = 4,
                MaxRoll = 4,
                Effect = PanicEffect.Fetal,
                EffectName = "Fetal Position",
                Description = "You curl into a ball, trying to make yourself as small as possible.",
                StatusEffects = new[] { "Prone" },
                ForcesAction = false
            },

            // Roll 5: Blackout (System Blackout)
            new PanicTableEntry
            {
                MinRoll = 5,
                MaxRoll = 5,
                Effect = PanicEffect.Blackout,
                EffectName = "System Blackout",
                Description = "Your mind shuts down to protect itself. Darkness takes you.",
                DurationTurns = 2, // 1d4 average
                StatusEffects = new[] { "Unconscious" },
                ForcesAction = false
            },

            // Roll 6: Denial (Reality Denial)
            new PanicTableEntry
            {
                MinRoll = 6,
                MaxRoll = 6,
                Effect = PanicEffect.Denial,
                EffectName = "Reality Denial",
                Description = "Your mind refuses to acknowledge the threat. It simply... isn't there.",
                DurationTurns = 2, // 1d4 average
                StatusEffects = Array.Empty<string>(),
                ForcesAction = false
                // Note: Cannot perceive trigger - handled by perception system
            },

            // Roll 7: Violence (Paradox Fury)
            new PanicTableEntry
            {
                MinRoll = 7,
                MaxRoll = 7,
                Effect = PanicEffect.Violence,
                EffectName = "Paradox Fury",
                Description = "Rage fills the void where reason should be. ATTACK!",
                DurationTurns = 1,
                StatusEffects = Array.Empty<string>(),
                ForcesAction = true,
                ForcedActionType = "AttackNearest"
            },

            // Roll 8: Catatonia (System Crash)
            new PanicTableEntry
            {
                MinRoll = 8,
                MaxRoll = 8,
                Effect = PanicEffect.Catatonia,
                EffectName = "System Crash",
                Description = "Your mind shuts down completely. Only pain can reboot you.",
                StatusEffects = new[] { "Prone", "Stunned" },
                ForcesAction = false
                // Note: Wake on damage - handled by combat system
            },

            // Roll 9: Dissociation (Reality Drift)
            new PanicTableEntry
            {
                MinRoll = 9,
                MaxRoll = 9,
                Effect = PanicEffect.Dissociation,
                EffectName = "Reality Drift",
                Description = "Your mind and body disconnect. You act without intent.",
                DurationTurns = 1,
                StatusEffects = Array.Empty<string>(),
                ForcesAction = true,
                ForcedActionType = "RandomAction"
            },

            // Roll 10: None (Lucky Break)
            new PanicTableEntry
            {
                MinRoll = 10,
                MaxRoll = 10,
                Effect = PanicEffect.None,
                EffectName = "Lucky Break",
                Description = "Your mind holds together... for now.",
                StatusEffects = Array.Empty<string>(),
                ForcesAction = false
            }
        };
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════════════
// CONFIGURATION DTOs
// ═══════════════════════════════════════════════════════════════════════════════

#region Configuration DTOs

/// <summary>
/// Configuration DTO for Panic Table entries.
/// </summary>
/// <remarks>
/// Maps d10 roll ranges to panic effects with mechanical impacts.
/// Can be loaded from config/panic-table.json.
/// </remarks>
internal sealed class PanicTableEntry
{
    /// <summary>
    /// Gets or sets the minimum d10 roll for this entry.
    /// </summary>
    public int MinRoll { get; set; }

    /// <summary>
    /// Gets or sets the maximum d10 roll for this entry.
    /// </summary>
    public int MaxRoll { get; set; }

    /// <summary>
    /// Gets or sets the panic effect type.
    /// </summary>
    public PanicEffect Effect { get; set; }

    /// <summary>
    /// Gets or sets the display name for this effect.
    /// </summary>
    public string EffectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the effect.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional duration in turns.
    /// </summary>
    public int? DurationTurns { get; set; }

    /// <summary>
    /// Gets or sets the optional self-damage amount.
    /// </summary>
    public int? SelfDamage { get; set; }

    /// <summary>
    /// Gets or sets the status effects to apply.
    /// </summary>
    public string[]? StatusEffects { get; set; }

    /// <summary>
    /// Gets or sets whether this effect forces an action.
    /// </summary>
    public bool ForcesAction { get; set; }

    /// <summary>
    /// Gets or sets the type of forced action (e.g., "FleeFromSource", "AttackNearest").
    /// </summary>
    public string? ForcedActionType { get; set; }
}

#endregion
