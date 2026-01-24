// ------------------------------------------------------------------------------
// <copyright file="IceCountermeasureService.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Service for handling ICE (Intrusion Countermeasures Electronics) encounters
// during terminal hacking attempts. Manages ICE triggering, resolution, and
// consequence application.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service for handling ICE (Intrusion Countermeasures Electronics) encounters
/// during terminal hacking attempts.
/// </summary>
/// <remarks>
/// <para>
/// ICE represents ancient automated defense programs that protect secured terminals
/// in Aethelgard. This service manages:
/// <list type="bullet">
///   <item><description>Determining what ICE protects each terminal type</description></item>
///   <item><description>Triggering ICE encounters on authentication failure</description></item>
///   <item><description>Resolving encounters through contested checks or saves</description></item>
///   <item><description>Applying consequences (damage, stress, lockout, alerts)</description></item>
/// </list>
/// </para>
/// <para>
/// ICE types and their resolution:
/// <list type="bullet">
///   <item><description>Passive (Trace): Contested System Bypass check</description></item>
///   <item><description>Active (Attack): Contested System Bypass check</description></item>
///   <item><description>Lethal (Neural): WILL save DC 16</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class IceCountermeasureService : IIceCountermeasureService
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The skill identifier for contested ICE checks.
    /// </summary>
    private const string SystemBypassSkillId = "system-bypass";

    /// <summary>
    /// The save identifier for Lethal ICE resistance.
    /// </summary>
    private const string WillSaveId = "will";

    /// <summary>
    /// Fixed DC for Lethal ICE WILL saves.
    /// </summary>
    private const int LethalIceSaveDc = 16;

    /// <summary>
    /// Bonus dice granted for defeating Active ICE.
    /// </summary>
    private const int ActiveIceVictoryBonusDice = 1;

    // -------------------------------------------------------------------------
    // ICE Rating by Terminal Type
    // -------------------------------------------------------------------------

    /// <summary>
    /// Static mapping of terminal types to their ICE ratings.
    /// </summary>
    private static readonly IReadOnlyDictionary<TerminalType, int> IceRatings =
        new Dictionary<TerminalType, int>
        {
            { TerminalType.CivilianDataPort, 0 },    // No ICE
            { TerminalType.CorporateMainframe, 12 }, // Passive ICE
            { TerminalType.SecurityHub, 16 },        // Active ICE
            { TerminalType.MilitaryServer, 20 },     // Active + Lethal
            { TerminalType.JotunArchive, 24 },       // Lethal ICE
            { TerminalType.GlitchedManifold, 0 }     // Variable/unpredictable
        };

    /// <summary>
    /// Static mapping of terminal types to their ICE types.
    /// </summary>
    private static readonly IReadOnlyDictionary<TerminalType, IceType[]> IceByTerminal =
        new Dictionary<TerminalType, IceType[]>
        {
            { TerminalType.CivilianDataPort, Array.Empty<IceType>() },
            { TerminalType.CorporateMainframe, new[] { IceType.Passive } },
            { TerminalType.SecurityHub, new[] { IceType.Active } },
            { TerminalType.MilitaryServer, new[] { IceType.Active, IceType.Lethal } },
            { TerminalType.JotunArchive, new[] { IceType.Lethal } },
            { TerminalType.GlitchedManifold, Array.Empty<IceType>() } // Determined at runtime
        };

    // -------------------------------------------------------------------------
    // Dependencies
    // -------------------------------------------------------------------------

    private readonly SkillCheckService _skillCheckService;
    private readonly IDiceService _diceService;
    private readonly ILogger<IceCountermeasureService> _logger;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="IceCountermeasureService"/> class.
    /// </summary>
    /// <param name="skillCheckService">Service for performing skill checks.</param>
    /// <param name="diceService">Service for rolling dice.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is null.
    /// </exception>
    public IceCountermeasureService(
        SkillCheckService skillCheckService,
        IDiceService diceService,
        ILogger<IceCountermeasureService> logger)
    {
        _skillCheckService = skillCheckService ?? throw new ArgumentNullException(nameof(skillCheckService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("IceCountermeasureService initialized");
    }

    // -------------------------------------------------------------------------
    // ICE Information Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyList<IceType> GetIceForTerminal(TerminalType terminalType)
    {
        if (IceByTerminal.TryGetValue(terminalType, out var iceTypes))
        {
            _logger.LogDebug(
                "Terminal type {TerminalType} has {IceCount} ICE type(s): [{IceTypes}]",
                terminalType,
                iceTypes.Length,
                string.Join(", ", iceTypes));
            return iceTypes;
        }

        _logger.LogDebug(
            "Terminal type {TerminalType} has no ICE configuration (defaulting to empty)",
            terminalType);
        return Array.Empty<IceType>();
    }

    /// <inheritdoc />
    public int GetIceRating(TerminalType terminalType)
    {
        if (IceRatings.TryGetValue(terminalType, out var rating))
        {
            _logger.LogDebug(
                "Terminal type {TerminalType} has ICE rating {Rating}",
                terminalType,
                rating);
            return rating;
        }

        _logger.LogDebug(
            "Terminal type {TerminalType} has no ICE rating (defaulting to 0)",
            terminalType);
        return 0;
    }

    /// <inheritdoc />
    public bool HasIce(TerminalType terminalType)
    {
        var hasIce = GetIceForTerminal(terminalType).Count > 0;
        _logger.LogDebug(
            "Terminal type {TerminalType} has ICE: {HasIce}",
            terminalType,
            hasIce);
        return hasIce;
    }

    // -------------------------------------------------------------------------
    // ICE Encounter Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IceEncounter TriggerIce(IceType iceType, int iceRating)
    {
        var encounter = IceEncounter.CreateTriggered(iceType, iceRating);

        _logger.LogInformation(
            "ICE triggered: Type={IceType}, Rating={Rating}, EncounterId={EncounterId}",
            iceType,
            iceRating,
            encounter.EncounterId);

        return encounter;
    }

    /// <inheritdoc />
    public IceResolutionResult ResolveIce(IceEncounter encounter, Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!encounter.IsPending)
        {
            throw new InvalidOperationException(
                $"Cannot resolve ICE encounter {encounter.EncounterId}: already resolved ({encounter.EncounterResult}).");
        }

        _logger.LogInformation(
            "Resolving ICE encounter {EncounterId} for player {PlayerId} ({PlayerName}): " +
            "Type={IceType}, Rating={Rating}",
            encounter.EncounterId,
            player.Id,
            player.Name,
            encounter.IceType,
            encounter.IceRating);

        // Resolution method depends on ICE type
        return encounter.IceType switch
        {
            IceType.Passive => ResolvePassiveIce(encounter, player),
            IceType.Active => ResolveActiveIce(encounter, player),
            IceType.Lethal => ResolveLethalIce(encounter, player),
            _ => throw new InvalidOperationException($"Unknown ICE type: {encounter.IceType}")
        };
    }

    // -------------------------------------------------------------------------
    // Consequence Application Methods
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public void ApplyIceConsequences(
        IceResolutionResult result,
        Player player,
        TerminalInfiltrationState infiltrationState)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(infiltrationState);

        _logger.LogInformation(
            "Applying ICE consequences for encounter {EncounterId}: Outcome={Outcome}, " +
            "Damage={Damage}, Stress={Stress}, Lockout={Lockout}, Alert={Alert}",
            result.Encounter.EncounterId,
            result.Outcome,
            result.PsychicDamage,
            result.StressGained,
            result.LockoutDuration,
            result.AlertLevelChange);

        // Record the encounter in infiltration state
        infiltrationState.AddIceEncounter(result.Encounter);
        _logger.LogDebug(
            "Recorded ICE encounter {EncounterId} in infiltration state {InfiltrationId}",
            result.Encounter.EncounterId,
            infiltrationState.InfiltrationId);

        // Apply psychic damage if any
        if (result.HasDamage)
        {
            ApplyPsychicDamage(player, result.PsychicDamage);
        }

        // Apply stress if any
        if (result.HasStress)
        {
            ApplyStress(player, result.StressGained);
        }

        // Handle disconnection and lockout
        if (result.ForcedDisconnect)
        {
            if (result.IsPermanentLockout)
            {
                infiltrationState.MarkAsLockedOut();
                _logger.LogWarning(
                    "Player {PlayerId} permanently locked out of terminal {TerminalId} by Lethal ICE",
                    player.Id,
                    infiltrationState.TerminalId);
            }
            else
            {
                infiltrationState.SetDisconnected(result.LockoutDuration);
                _logger.LogInformation(
                    "Player {PlayerId} disconnected from terminal {TerminalId} with {Minutes} minute lockout",
                    player.Id,
                    infiltrationState.TerminalId,
                    result.LockoutDuration);
            }
        }

        // Apply alert level change
        if (result.AlertLevelChange > 0)
        {
            infiltrationState.IncreaseAlertLevel(result.AlertLevelChange);
            _logger.LogDebug(
                "Alert level increased by {Amount} to {NewLevel} on terminal {TerminalId}",
                result.AlertLevelChange,
                infiltrationState.AlertLevel,
                infiltrationState.TerminalId);
        }

        _logger.LogInformation(
            "ICE consequences applied for {EncounterId}. Terminal status: {Status}, Alert: {Alert}",
            result.Encounter.EncounterId,
            infiltrationState.Status,
            infiltrationState.AlertLevel);
    }

    // -------------------------------------------------------------------------
    // Private Resolution Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves a Passive (Trace) ICE encounter using contested System Bypass.
    /// </summary>
    /// <param name="encounter">The ICE encounter.</param>
    /// <param name="player">The player character.</param>
    /// <returns>The resolution result.</returns>
    private IceResolutionResult ResolvePassiveIce(IceEncounter encounter, Player player)
    {
        var iceDc = encounter.GetDc();

        _logger.LogDebug(
            "Resolving Passive ICE: Rating {Rating} → DC {DC}",
            encounter.IceRating,
            iceDc);

        // Perform contested System Bypass check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            SystemBypassSkillId,
            iceDc,
            "Evade Trace ICE");

        _logger.LogDebug(
            "Passive ICE check result: NetSuccesses={NetSuccesses}, DC={DC}, " +
            "Margin={Margin}, Outcome={Outcome}",
            checkResult.NetSuccesses,
            checkResult.DifficultyClass,
            checkResult.Margin,
            checkResult.Outcome);

        // Character wins if outcome is MarginalSuccess or better
        var characterWon = checkResult.IsSuccess;

        if (characterWon)
        {
            _logger.LogInformation(
                "Player {PlayerId} evaded Passive ICE: trace unsuccessful",
                player.Id);
            return IceResolutionResult.PassiveEvaded(encounter, checkResult.NetSuccesses, iceDc);
        }
        else
        {
            _logger.LogWarning(
                "Player {PlayerId} failed to evade Passive ICE: location revealed!",
                player.Id);
            return IceResolutionResult.PassiveFailed(encounter, checkResult.NetSuccesses, iceDc);
        }
    }

    /// <summary>
    /// Resolves an Active (Attack) ICE encounter using contested System Bypass.
    /// </summary>
    /// <param name="encounter">The ICE encounter.</param>
    /// <param name="player">The player character.</param>
    /// <returns>The resolution result.</returns>
    private IceResolutionResult ResolveActiveIce(IceEncounter encounter, Player player)
    {
        var iceDc = encounter.GetDc();

        _logger.LogDebug(
            "Resolving Active ICE: Rating {Rating} → DC {DC}",
            encounter.IceRating,
            iceDc);

        // Perform contested System Bypass check
        var checkResult = _skillCheckService.PerformCheckWithDC(
            player,
            SystemBypassSkillId,
            iceDc,
            "Counter Attack ICE");

        _logger.LogDebug(
            "Active ICE check result: NetSuccesses={NetSuccesses}, DC={DC}, " +
            "Margin={Margin}, Outcome={Outcome}",
            checkResult.NetSuccesses,
            checkResult.DifficultyClass,
            checkResult.Margin,
            checkResult.Outcome);

        // Character wins if outcome is MarginalSuccess or better
        var characterWon = checkResult.IsSuccess;

        if (characterWon)
        {
            _logger.LogInformation(
                "Player {PlayerId} defeated Active ICE: +{BonusDice}d10 to next layer",
                player.Id,
                ActiveIceVictoryBonusDice);
            return IceResolutionResult.ActiveDefeated(encounter, checkResult.NetSuccesses, iceDc);
        }
        else
        {
            _logger.LogWarning(
                "Player {PlayerId} defeated by Active ICE: forced disconnect!",
                player.Id);
            return IceResolutionResult.ActiveFailed(encounter, checkResult.NetSuccesses, iceDc);
        }
    }

    /// <summary>
    /// Resolves a Lethal (Neural) ICE encounter using a WILL save.
    /// </summary>
    /// <param name="encounter">The ICE encounter.</param>
    /// <param name="player">The player character.</param>
    /// <returns>The resolution result.</returns>
    private IceResolutionResult ResolveLethalIce(IceEncounter encounter, Player player)
    {
        _logger.LogDebug(
            "Resolving Lethal ICE: Rating {Rating}, Save DC {DC}",
            encounter.IceRating,
            LethalIceSaveDc);

        // Perform WILL save against fixed DC 16
        var saveResult = _skillCheckService.PerformCheckWithDC(
            player,
            WillSaveId,
            LethalIceSaveDc,
            "Resist Neural ICE");

        _logger.LogDebug(
            "Lethal ICE save result: NetSuccesses={NetSuccesses}, DC={DC}, " +
            "Margin={Margin}, Outcome={Outcome}",
            saveResult.NetSuccesses,
            saveResult.DifficultyClass,
            saveResult.Margin,
            saveResult.Outcome);

        // Character survives if save succeeds (MarginalSuccess or better)
        var saveMade = saveResult.IsSuccess;

        if (saveMade)
        {
            // Roll stress: 1d6
            var stressRoll = RollStressDice(1);

            _logger.LogInformation(
                "Player {PlayerId} resisted Lethal ICE: disconnected with {Stress} stress",
                player.Id,
                stressRoll);
            return IceResolutionResult.LethalSaved(encounter, saveResult.NetSuccesses, stressRoll);
        }
        else
        {
            // Roll damage: 3d10
            var damageRoll = RollDamageDice(3);

            // Roll stress: 2d6
            var stressRoll = RollStressDice(2);

            _logger.LogWarning(
                "Player {PlayerId} FAILED Lethal ICE save: {Damage} psychic damage, " +
                "{Stress} stress, PERMANENT LOCKOUT!",
                player.Id,
                damageRoll,
                stressRoll);
            return IceResolutionResult.LethalFailed(encounter, saveResult.NetSuccesses, damageRoll, stressRoll);
        }
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rolls damage dice for Lethal ICE (d10s).
    /// </summary>
    /// <param name="count">Number of d10s to roll.</param>
    /// <returns>Total damage rolled.</returns>
    private int RollDamageDice(int count)
    {
        var pool = DicePool.D10(count);
        var result = _diceService.Roll(pool);

        _logger.LogDebug(
            "Lethal ICE damage roll: {Count}d10 = [{Rolls}] = {Total}",
            count,
            string.Join(", ", result.Rolls),
            result.Total);

        return result.Total;
    }

    /// <summary>
    /// Rolls stress dice for Lethal ICE (d6s).
    /// </summary>
    /// <param name="count">Number of d6s to roll.</param>
    /// <returns>Total stress rolled.</returns>
    private int RollStressDice(int count)
    {
        var pool = DicePool.D6(count);
        var result = _diceService.Roll(pool);

        _logger.LogDebug(
            "Lethal ICE stress roll: {Count}d6 = [{Rolls}] = {Total}",
            count,
            string.Join(", ", result.Rolls),
            result.Total);

        return result.Total;
    }

    /// <summary>
    /// Applies psychic damage to a player.
    /// </summary>
    /// <param name="player">The player to damage.</param>
    /// <param name="damage">Amount of damage.</param>
    private void ApplyPsychicDamage(Player player, int damage)
    {
        // Note: Actual damage application would integrate with combat/health systems.
        // For now, log the damage that should be applied.
        _logger.LogWarning(
            "Player {PlayerId} ({PlayerName}) takes {Damage} psychic damage from Lethal ICE",
            player.Id,
            player.Name,
            damage);

        // TODO: Integrate with health/damage system when available
        // player.TakeDamage(damage, DamageType.Psychic);
    }

    /// <summary>
    /// Applies stress to a player.
    /// </summary>
    /// <param name="player">The player to stress.</param>
    /// <param name="stress">Amount of stress.</param>
    private void ApplyStress(Player player, int stress)
    {
        // Note: Actual stress application would integrate with stress/trauma systems.
        // For now, log the stress that should be applied.
        _logger.LogWarning(
            "Player {PlayerId} ({PlayerName}) gains {Stress} stress from Lethal ICE encounter",
            player.Id,
            player.Name,
            stress);

        // TODO: Integrate with stress/trauma system when available
        // player.AddStress(stress);
    }
}
