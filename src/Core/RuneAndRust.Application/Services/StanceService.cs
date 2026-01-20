using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing combat stances and their effects on combatants.
/// </summary>
/// <remarks>
/// <para>StanceService coordinates stance changes, enforces the once-per-round limit,
/// and provides stance modifier values for combat calculations.</para>
/// <para>Key responsibilities:</para>
/// <list type="bullet">
///   <item><description>Track which combatants have changed stance this round</description></item>
///   <item><description>Validate stance change requests</description></item>
///   <item><description>Log all stance-related events</description></item>
///   <item><description>Provide stance bonuses for combat calculations</description></item>
/// </list>
/// </remarks>
public class StanceService : IStanceService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IStanceProvider _stanceProvider;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<StanceService> _logger;

    /// <summary>
    /// Tracks which combatants have changed stance this round.
    /// </summary>
    /// <remarks>
    /// Keyed by combatant ID to prevent multiple stance changes per round.
    /// Cleared via <see cref="ResetStanceChange"/> or <see cref="ResetAllStanceChanges"/>.
    /// </remarks>
    private readonly HashSet<Guid> _stanceChangedThisRound = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new StanceService instance.
    /// </summary>
    /// <param name="stanceProvider">Provider for stance definitions.</param>
    /// <param name="eventLogger">Logger for combat events.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public StanceService(
        IStanceProvider stanceProvider,
        IGameEventLogger eventLogger,
        ILogger<StanceService> logger)
    {
        _stanceProvider = stanceProvider ?? throw new ArgumentNullException(nameof(stanceProvider));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "StanceService initialized with {StanceCount} configured stances",
            _stanceProvider.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE QUERY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public CombatStance GetCurrentStance(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return combatant.CurrentStance;
    }

    /// <inheritdoc />
    public StanceDefinition? GetStanceDefinition(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return _stanceProvider.GetStance(combatant.CurrentStance);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE CHANGE METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public StanceChangeResult SetStance(Combatant combatant, CombatStance stance)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        _logger.LogDebug(
            "SetStance called for {Combatant}: current={Current}, requested={Requested}",
            combatant.DisplayName,
            combatant.CurrentStance,
            stance);

        // Check if already changed this round
        if (!CanChangeStance(combatant))
        {
            _logger.LogWarning(
                "{Combatant} already changed stance this round, cannot change to {Stance}",
                combatant.DisplayName,
                stance);

            return StanceChangeResult.Failed("Already changed stance this round");
        }

        // Check if already in the requested stance
        if (combatant.CurrentStance == stance)
        {
            _logger.LogDebug(
                "{Combatant} is already in {Stance} stance",
                combatant.DisplayName,
                stance);

            return StanceChangeResult.AlreadyInStance(stance);
        }

        // Get the new stance definition
        var newStanceDef = _stanceProvider.GetStance(stance);
        if (newStanceDef is null)
        {
            _logger.LogError(
                "Unknown stance requested: {Stance}",
                stance);

            return StanceChangeResult.Failed($"Unknown stance: {stance}");
        }

        // Capture old stance for event
        var oldStance = combatant.CurrentStance;
        var oldStanceDef = _stanceProvider.GetStance(oldStance);

        // Log modifier removal if old stance had modifiers
        if (oldStanceDef is not null && oldStanceDef.HasModifiers())
        {
            _logger.LogDebug(
                "Removing modifiers from {StanceName} (source: {Source})",
                oldStanceDef.Name,
                oldStanceDef.GetModifierSource());
        }

        // Set the new stance on the combatant
        combatant.SetStance(stance);

        // Mark that this combatant has changed stance this round
        _stanceChangedThisRound.Add(combatant.Id);

        // Log modifier application if new stance has modifiers
        if (newStanceDef.HasModifiers())
        {
            LogModifierApplication(combatant, newStanceDef);
        }

        // Log the stance change
        _logger.LogInformation(
            "{Combatant} changed stance from {OldStance} to {NewStance}",
            combatant.DisplayName,
            oldStance,
            stance);

        // Log combat event
        _eventLogger.LogCombat(
            "StanceChanged",
            $"{combatant.DisplayName} changed from {oldStance} to {stance} stance",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = combatant.Id,
                ["combatantName"] = combatant.DisplayName,
                ["oldStance"] = oldStance.ToString(),
                ["newStance"] = stance.ToString(),
                ["attackBonus"] = newStanceDef.AttackBonus,
                ["defenseBonus"] = newStanceDef.DefenseBonus,
                ["saveBonus"] = newStanceDef.SaveBonus
            });

        return StanceChangeResult.Success(oldStance, stance);
    }

    /// <inheritdoc />
    public bool CanChangeStance(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        return !_stanceChangedThisRound.Contains(combatant.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // STANCE MODIFIER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public int GetAttackBonus(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        var stance = _stanceProvider.GetStance(combatant.CurrentStance);
        return stance?.AttackBonus ?? 0;
    }

    /// <inheritdoc />
    public string? GetDamageBonus(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        var stance = _stanceProvider.GetStance(combatant.CurrentStance);
        return stance?.DamageBonus;
    }

    /// <inheritdoc />
    public int GetDefenseBonus(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        var stance = _stanceProvider.GetStance(combatant.CurrentStance);
        return stance?.DefenseBonus ?? 0;
    }

    /// <inheritdoc />
    public int GetSaveBonus(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);
        var stance = _stanceProvider.GetStance(combatant.CurrentStance);
        return stance?.SaveBonus ?? 0;
    }

    // ═══════════════════════════════════════════════════════════════
    // RESET METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void ResetStanceChange(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        if (_stanceChangedThisRound.Remove(combatant.Id))
        {
            _logger.LogDebug(
                "{Combatant} stance change reset for new round",
                combatant.DisplayName);

            _eventLogger.LogCombat(
                "StanceChangeReset",
                $"{combatant.DisplayName} can change stance again",
                data: new Dictionary<string, object>
                {
                    ["combatantId"] = combatant.Id,
                    ["combatantName"] = combatant.DisplayName
                });
        }
    }

    /// <inheritdoc />
    public void ResetAllStanceChanges()
    {
        var count = _stanceChangedThisRound.Count;
        _stanceChangedThisRound.Clear();

        _logger.LogDebug(
            "Reset stance change availability for all combatants ({Count} were tracking)",
            count);
    }

    // ═══════════════════════════════════════════════════════════════
    // UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<StanceDefinition> GetAvailableStances()
    {
        return _stanceProvider.GetAllStances();
    }

    /// <inheritdoc />
    public void InitializeStance(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var defaultStance = _stanceProvider.GetDefaultStance();
        combatant.SetStance(defaultStance.ToCombatStance());

        _logger.LogDebug(
            "{Combatant} initialized with {StanceName} stance",
            combatant.DisplayName,
            defaultStance.Name);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Logs detailed modifier application information for debugging.
    /// </summary>
    /// <param name="combatant">The combatant receiving modifiers.</param>
    /// <param name="stance">The stance being applied.</param>
    private void LogModifierApplication(Combatant combatant, StanceDefinition stance)
    {
        if (stance.AttackBonus != 0)
        {
            _logger.LogDebug(
                "Applied {Bonus:+#;-#;0} attack from {StanceName}",
                stance.AttackBonus,
                stance.Name);
        }

        if (!string.IsNullOrEmpty(stance.DamageBonus))
        {
            _logger.LogDebug(
                "Applied {Bonus} damage bonus from {StanceName}",
                stance.DamageBonus,
                stance.Name);
        }

        if (stance.DefenseBonus != 0)
        {
            _logger.LogDebug(
                "Applied {Bonus:+#;-#;0} defense from {StanceName}",
                stance.DefenseBonus,
                stance.Name);
        }

        if (stance.SaveBonus != 0)
        {
            _logger.LogDebug(
                "Applied {Bonus:+#;-#;0} to all saves from {StanceName}",
                stance.SaveBonus,
                stance.Name);
        }
    }
}
