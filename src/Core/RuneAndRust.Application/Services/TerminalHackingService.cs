// ------------------------------------------------------------------------------
// <copyright file="TerminalHackingService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for executing multi-layer terminal hacking attempts using the System
// Bypass skill. Handles layer progression, DC calculation, outcome processing,
// and fumble consequence management.
// Part of v0.15.4b Terminal Hacking System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for executing multi-layer terminal hacking attempts.
/// </summary>
/// <remarks>
/// <para>
/// Implements the terminal hacking subsystem of the System Bypass skill.
/// Terminal hacking follows cargo cult mechanics—characters manipulate
/// incomprehensible systems through observed patterns.
/// </para>
/// <para>
/// Infiltration proceeds through three layers:
/// <list type="bullet">
///   <item><description>Layer 1 (Access): Establish connection, bypass firewall</description></item>
///   <item><description>Layer 2 (Authentication): Verify identity, gain access level</description></item>
///   <item><description>Layer 3 (Navigation): Locate and access specific data</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class TerminalHackingService : ITerminalHackingService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The skill identifier for terminal hacking checks.
    /// </summary>
    private const string SystemBypassSkillId = "system-bypass";

    /// <summary>
    /// The DC for covering tracks after successful infiltration.
    /// </summary>
    private const int CoverTracksDc = 14;

    /// <summary>
    /// The skill identifier for covering tracks (uses stealth).
    /// </summary>
    private const string StealthSkillId = "stealth";

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly SkillCheckService _skillCheckService;
    private readonly IFumbleConsequenceService _fumbleService;
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<TerminalHackingService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalHackingService"/> class.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="fumbleService">Service for managing fumble consequences.</param>
    /// <param name="configProvider">Provider for game configuration data.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public TerminalHackingService(
        SkillCheckService skillCheckService,
        IFumbleConsequenceService fumbleService,
        IGameConfigurationProvider configProvider,
        ILogger<TerminalHackingService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _fumbleService = fumbleService ?? throw new ArgumentNullException(nameof(fumbleService));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("TerminalHackingService initialized");
    }

    // -------------------------------------------------------------------------
    // Core Infiltration Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public TerminalInfiltrationState BeginInfiltration(Player player, TerminalContext context)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(context);

        var infiltrationId = $"inf-{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Player {PlayerId} ({PlayerName}) beginning infiltration of {TerminalType} terminal {TerminalId} " +
            "(Layer1DC={Layer1Dc}, Layer2DC={Layer2Dc}, Layer3DC={Layer3Dc}, Tools={ToolQuality})",
            player.Id,
            player.Name,
            context.TerminalType,
            context.TerminalId,
            context.Layer1Dc,
            context.Layer2Dc,
            context.Layer3Dc,
            context.ToolQuality);

        var state = TerminalInfiltrationState.Create(
            infiltrationId,
            player.Id.ToString(),
            context.TerminalType,
            context.TerminalId);

        _logger.LogDebug(
            "Created infiltration state {InfiltrationId} for player {PlayerId} on terminal {TerminalId}",
            infiltrationId,
            player.Id,
            context.TerminalId);

        return state;
    }

    /// <inheritdoc />
    public LayerResult AttemptCurrentLayer(
        Player player,
        TerminalInfiltrationState state,
        TerminalContext context)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        if (state.IsComplete)
        {
            throw new InvalidOperationException("Cannot attempt layer on completed infiltration.");
        }

        var layer = state.CurrentLayerEnum;
        var dc = GetLayerDc(layer, context);
        var skillContext = context.ToSkillContext(layer);

        _logger.LogInformation(
            "Player {PlayerId} attempting {Layer} on {TerminalType} terminal {TerminalId} (DC={DC})",
            player.Id,
            layer,
            context.TerminalType,
            context.TerminalId,
            dc);

        _logger.LogDebug(
            "Skill context for layer {Layer}: DiceModifier={DiceMod}, DcModifier={DcMod}",
            layer,
            skillContext.TotalDiceModifier,
            skillContext.TotalDcModifier);

        // Perform the skill check
        var checkResult = _skillCheckService.PerformCheckWithContext(
            player,
            SystemBypassSkillId,
            dc,
            $"{context.TerminalType} {layer}",
            skillContext);

        _logger.LogDebug(
            "Terminal hack check result for {Layer}: NetSuccesses={NetSuccesses}, " +
            "DC={DC}, Margin={Margin}, Outcome={Outcome}, IsFumble={IsFumble}",
            layer,
            checkResult.NetSuccesses,
            checkResult.DifficultyClass,
            checkResult.Margin,
            checkResult.Outcome,
            checkResult.IsFumble);

        // Process the result
        var layerResult = ProcessLayerResult(player, state, context, layer, dc, checkResult);

        // Record in state
        state.RecordLayerResult(layerResult);

        _logger.LogInformation(
            "Layer {Layer} attempt result: {IsSuccess} (Access={AccessLevel}, Status={Status})",
            layer,
            layerResult.IsSuccess ? "SUCCESS" : (layerResult.IsFumble ? "FUMBLE" : "FAILURE"),
            state.AccessLevel,
            state.Status);

        // Create fumble consequence if needed
        if (layerResult.IsFumble)
        {
            CreateSystemLockoutConsequence(player, state, context);
        }

        return layerResult;
    }

    /// <inheritdoc />
    public bool AttemptCoverTracks(Player player, TerminalInfiltrationState state)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(state);

        if (!state.IsSuccessful)
        {
            throw new InvalidOperationException("Can only cover tracks after successful infiltration.");
        }

        _logger.LogDebug(
            "Player {PlayerId} attempting to cover tracks on terminal {TerminalId}",
            player.Id,
            state.TerminalId);

        // Use stealth skill for covering tracks
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            StealthSkillId,
            CoverTracksDc,
            "Cover Tracks");

        var success = checkResult.Outcome >= SkillOutcome.MarginalSuccess;

        if (success)
        {
            state.MarkTracksCovered();
            _logger.LogInformation(
                "Player {PlayerId} successfully covered tracks on terminal {TerminalId}",
                player.Id,
                state.TerminalId);
        }
        else
        {
            _logger.LogDebug(
                "Player {PlayerId} failed to cover tracks on terminal {TerminalId} (Outcome={Outcome})",
                player.Id,
                state.TerminalId,
                checkResult.Outcome);
        }

        return success;
    }

    // -------------------------------------------------------------------------
    // Information Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public int GetLayerDc(InfiltrationLayer layer, TerminalContext context)
    {
        return layer switch
        {
            InfiltrationLayer.Layer1_Access => context.Layer1Dc,
            InfiltrationLayer.Layer2_Authentication => context.Layer2Dc,
            InfiltrationLayer.Layer3_Navigation => context.Layer3Dc,
            _ => context.Layer1Dc
        };
    }

    /// <inheritdoc />
    public bool CanAttempt(Player player, TerminalContext context)
    {
        return GetAttemptBlockedReason(player, context) == null;
    }

    /// <inheritdoc />
    public string? GetAttemptBlockedReason(Player player, TerminalContext context)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(context);

        // Check for existing lockout on this terminal
        var isBlocked = _fumbleService.IsCheckBlocked(
            player.Id.ToString(),
            SystemBypassSkillId,
            context.TerminalId);

        if (isBlocked)
        {
            _logger.LogDebug(
                "Terminal {TerminalId} blocked for player {PlayerId}: System Lockout active",
                context.TerminalId,
                player.Id);
            return "This terminal has been locked out and cannot be accessed.";
        }

        // Note: Additional checks could be added here for skill training requirements
        // when the skill training system is implemented.

        return null;
    }

    // -------------------------------------------------------------------------
    // Private Processing Methods
    // -------------------------------------------------------------------------

    private LayerResult ProcessLayerResult(
        Player player,
        TerminalInfiltrationState state,
        TerminalContext context,
        InfiltrationLayer layer,
        int dc,
        SkillCheckResult checkResult)
    {
        var timeRounds = layer == InfiltrationLayer.Layer1_Access
            ? context.Layer1TimeRounds
            : 1;

        return checkResult.Outcome switch
        {
            SkillOutcome.CriticalFailure => CreateFumbleResult(layer, dc, checkResult),
            SkillOutcome.Failure => CreateFailureResult(layer, dc, timeRounds, checkResult),
            SkillOutcome.CriticalSuccess => CreateCriticalSuccessResult(layer, dc, timeRounds, checkResult),
            _ => CreateSuccessResult(layer, dc, timeRounds, checkResult)
        };
    }

    private LayerResult CreateSuccessResult(
        InfiltrationLayer layer,
        int dc,
        int timeRounds,
        SkillCheckResult checkResult)
    {
        var (accessGranted, narrative) = layer switch
        {
            InfiltrationLayer.Layer1_Access => (
                (AccessLevel?)null,
                "Connection established. Firewall bypassed. Proceeding to authentication."
            ),
            InfiltrationLayer.Layer2_Authentication => (
                AccessLevel.UserLevel,
                "Identity verified. User-level access granted. Proceeding to data navigation."
            ),
            InfiltrationLayer.Layer3_Navigation => (
                (AccessLevel?)null,
                "Data located and accessed successfully."
            ),
            _ => (null, "Layer completed.")
        };

        _logger.LogDebug(
            "Terminal hack {Layer} success: {Narrative}",
            layer,
            narrative);

        return LayerResult.Success(
            layer,
            checkResult,
            checkResult.Outcome,
            dc,
            timeRounds,
            accessGranted,
            narrative);
    }

    private LayerResult CreateCriticalSuccessResult(
        InfiltrationLayer layer,
        int dc,
        int timeRounds,
        SkillCheckResult checkResult)
    {
        var (accessGranted, narrative) = layer switch
        {
            InfiltrationLayer.Layer1_Access => (
                AccessLevel.AdminLevel,
                "Masterful bypass! Full system access achieved. Admin privileges granted immediately."
            ),
            InfiltrationLayer.Layer2_Authentication => (
                AccessLevel.AdminLevel,
                "Authentication spoofed flawlessly. Administrative access granted."
            ),
            InfiltrationLayer.Layer3_Navigation => (
                (AccessLevel?)null,
                "Expert navigation reveals hidden data caches and archived files."
            ),
            _ => (null, "Exceptional success.")
        };

        _logger.LogInformation(
            "Terminal hack {Layer} CRITICAL SUCCESS: {Narrative}",
            layer,
            narrative);

        return LayerResult.Success(
            layer,
            checkResult,
            SkillOutcome.CriticalSuccess,
            dc,
            timeRounds,
            accessGranted,
            narrative);
    }

    private LayerResult CreateFailureResult(
        InfiltrationLayer layer,
        int dc,
        int timeRounds,
        SkillCheckResult checkResult)
    {
        var narrative = layer switch
        {
            InfiltrationLayer.Layer1_Access =>
                "Connection rejected. The terminal locks you out for 1 minute. " +
                "Security protocols have been alerted—next attempt will be harder (+2 DC).",
            InfiltrationLayer.Layer2_Authentication =>
                "Authentication failed. Security alert triggered! " +
                "ICE countermeasures may be activating.",
            InfiltrationLayer.Layer3_Navigation =>
                "Navigation unsuccessful. You can only access partial data—" +
                "classified and hidden files remain out of reach.",
            _ => "Layer failed."
        };

        _logger.LogDebug(
            "Terminal hack {Layer} failed: {Narrative}",
            layer,
            narrative);

        return LayerResult.Failure(
            layer,
            checkResult,
            checkResult.Outcome,
            dc,
            timeRounds,
            narrative);
    }

    private LayerResult CreateFumbleResult(
        InfiltrationLayer layer,
        int dc,
        SkillCheckResult checkResult)
    {
        var narrative = layer switch
        {
            InfiltrationLayer.Layer1_Access =>
                "Catastrophic failure! Your intrusion triggers ancient security protocols. " +
                "The terminal emits a harsh klaxon and goes dark. [System Lockout]",
            InfiltrationLayer.Layer2_Authentication =>
                "Critical authentication failure! The system detects your intrusion and " +
                "initiates full lockdown. An alert broadcasts your location. [System Lockout]",
            InfiltrationLayer.Layer3_Navigation =>
                "Fatal navigation error! A cascading failure corrupts the system. " +
                "The terminal shuts down permanently, data irretrievably scrambled. [System Lockout]",
            _ => "Terminal locked out permanently."
        };

        _logger.LogWarning(
            "Terminal hack {Layer} FUMBLE - system lockout triggered: {Narrative}",
            layer,
            narrative);

        return LayerResult.Fumble(layer, checkResult, dc, narrative);
    }

    private void CreateSystemLockoutConsequence(
        Player player,
        TerminalInfiltrationState state,
        TerminalContext context)
    {
        var description = $"Terminal {context.TerminalName ?? context.TerminalType.ToString()} " +
                         $"permanently disabled. Security alert broadcast.";

        _fumbleService.CreateConsequence(
            player.Id.ToString(),
            SystemBypassSkillId,
            FumbleType.SystemLockout,
            context.TerminalId,
            description);

        _logger.LogWarning(
            "Created [System Lockout] consequence for terminal {TerminalId} " +
            "after fumble by player {PlayerId}",
            context.TerminalId,
            player.Id);
    }
}
