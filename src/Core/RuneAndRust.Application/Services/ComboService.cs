namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;

/// <summary>
/// Service for detecting and tracking ability combos during combat.
/// </summary>
/// <remarks>
/// <para>ComboService manages the combo detection system, rewarding players for executing
/// specific sequences of abilities within a time window. Key features:</para>
/// <list type="bullet">
///   <item><description>Automatic combo start detection when first ability is used</description></item>
///   <item><description>Progress tracking with target requirement validation</description></item>
///   <item><description>Bonus effect application on combo completion</description></item>
///   <item><description>Window expiration handling</description></item>
/// </list>
/// <para>
/// The service maintains scoped state per combat session using dictionaries keyed by
/// combatant ID. State is automatically cleared when <see cref="ResetProgress"/> is called.
/// </para>
/// <para>
/// Combo definitions are loaded from <see cref="IComboProvider"/> and include class restrictions,
/// target requirements, and bonus effects as defined in v0.10.3a.
/// </para>
/// </remarks>
public class ComboService : IComboService
{
    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Provider for combo definitions loaded from configuration.
    /// </summary>
    private readonly IComboProvider _comboProvider;

    /// <summary>
    /// Service for applying status effects from combo bonuses.
    /// </summary>
    private readonly IBuffDebuffService _buffDebuffService;

    /// <summary>
    /// Service for rolling dice for damage and healing bonuses.
    /// </summary>
    private readonly IDiceService _diceService;

    /// <summary>
    /// Logger for game events (combat log integration).
    /// </summary>
    private readonly IGameEventLogger _eventLogger;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<ComboService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Active combo progress by combatant ID.
    /// </summary>
    /// <remarks>
    /// <para>Each combatant can have multiple in-progress combos simultaneously.</para>
    /// </remarks>
    private readonly Dictionary<Guid, List<ComboProgress>> _activeProgress = new();

    /// <summary>
    /// Completed combo count by combatant ID.
    /// </summary>
    /// <remarks>
    /// <para>Tracks total combos completed this combat session for statistics.</para>
    /// </remarks>
    private readonly Dictionary<Guid, int> _completedCounts = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new instance of the ComboService.
    /// </summary>
    /// <param name="comboProvider">Provider for combo definitions.</param>
    /// <param name="buffDebuffService">Service for applying status effects.</param>
    /// <param name="diceService">Service for dice rolling.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
    public ComboService(
        IComboProvider comboProvider,
        IBuffDebuffService buffDebuffService,
        IDiceService diceService,
        IGameEventLogger eventLogger,
        ILogger<ComboService> logger)
    {
        _comboProvider = comboProvider ?? throw new ArgumentNullException(nameof(comboProvider));
        _buffDebuffService = buffDebuffService ?? throw new ArgumentNullException(nameof(buffDebuffService));
        _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "ComboService initialized with {ComboCount} combo definitions available",
            _comboProvider.GetAllCombos().Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION - ABILITY PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public ComboResult OnAbilityUsed(Combatant user, string abilityId, Combatant? target)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(abilityId);

        _logger.LogDebug(
            "OnAbilityUsed: {User} used {Ability} on {Target}",
            user.DisplayName,
            abilityId,
            target?.DisplayName ?? "self");

        var actions = new List<ComboActionResult>();
        var completedCombos = new List<ComboDefinition>();

        // Get targeting information for combo step validation
        var targetId = target?.Id;
        var isSelfTarget = target == null || target.Id == user.Id;

        // Step 1: Try to advance active combos
        var advanceResults = TryAdvanceActiveCombos(user, abilityId, targetId, isSelfTarget);
        actions.AddRange(advanceResults.actions);
        completedCombos.AddRange(advanceResults.completed);

        // Step 2: Check for new combo starts (only if class allows)
        var startResults = TryStartNewCombos(user, abilityId, targetId);
        actions.AddRange(startResults);

        // Step 3: Apply bonus effects for completed combos
        foreach (var combo in completedCombos)
        {
            ApplyBonusEffects(user, combo, target);
        }

        // Log summary
        if (actions.Count > 0)
        {
            _logger.LogInformation(
                "OnAbilityUsed: {User} triggered {ActionCount} combo actions ({Summary})",
                user.DisplayName,
                actions.Count,
                GetActionSummary(actions));
        }
        else
        {
            _logger.LogDebug(
                "OnAbilityUsed: {User}'s {Ability} did not affect any combos",
                user.DisplayName,
                abilityId);
        }

        return new ComboResult
        {
            Actions = actions,
            CompletedCombos = completedCombos
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION - QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<ComboProgress> GetActiveProgress(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        if (_activeProgress.TryGetValue(combatant.Id, out var progress))
        {
            _logger.LogDebug(
                "GetActiveProgress: {Combatant} has {Count} active combos",
                combatant.DisplayName,
                progress.Count);

            return progress.AsReadOnly();
        }

        _logger.LogDebug(
            "GetActiveProgress: {Combatant} has no active combos",
            combatant.DisplayName);

        return [];
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboDefinition> GetAvailableCombos(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var classId = GetClassId(combatant);

        if (string.IsNullOrEmpty(classId))
        {
            _logger.LogDebug(
                "GetAvailableCombos: {Combatant} has no class, returning empty list",
                combatant.DisplayName);

            return [];
        }

        var combos = _comboProvider.GetCombosForClass(classId);

        _logger.LogDebug(
            "GetAvailableCombos: {Combatant} (class={Class}) has {Count} available combos",
            combatant.DisplayName,
            classId,
            combos.Count);

        return combos;
    }

    /// <inheritdoc />
    public IReadOnlyList<ComboHint> GetComboHints(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        if (!_activeProgress.TryGetValue(combatant.Id, out var progressList) || progressList.Count == 0)
        {
            _logger.LogDebug(
                "GetComboHints: {Combatant} has no active combos to hint",
                combatant.DisplayName);

            return [];
        }

        var hints = new List<ComboHint>();

        foreach (var progress in progressList)
        {
            var combo = _comboProvider.GetCombo(progress.ComboId);
            if (combo == null)
            {
                _logger.LogWarning(
                    "GetComboHints: Combo definition not found for active progress: {ComboId}",
                    progress.ComboId);
                continue;
            }

            var nextAbilityId = combo.GetAbilityForStep(progress.NextStep);
            if (string.IsNullOrEmpty(nextAbilityId))
            {
                _logger.LogWarning(
                    "GetComboHints: No ability found for step {Step} of combo {ComboId}",
                    progress.NextStep,
                    progress.ComboId);
                continue;
            }

            var hint = ComboHint.FromProgress(progress, nextAbilityId);
            hints.Add(hint);

            _logger.LogDebug(
                "GetComboHints: Generated hint for {ComboName}: use {Ability} ({Progress})",
                progress.ComboName,
                nextAbilityId,
                progress.GetProgressString());
        }

        return hints;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION - STATE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void TickCombos(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        if (!_activeProgress.TryGetValue(combatant.Id, out var progressList) || progressList.Count == 0)
        {
            _logger.LogDebug(
                "TickCombos: {Combatant} has no active combos to tick",
                combatant.DisplayName);
            return;
        }

        _logger.LogDebug(
            "TickCombos: Ticking {Count} active combos for {Combatant}",
            progressList.Count,
            combatant.DisplayName);

        // Decrement windows and collect expired combos
        var expired = new List<ComboProgress>();

        foreach (var progress in progressList)
        {
            progress.DecrementWindow();

            _logger.LogDebug(
                "TickCombos: {ComboName} window decremented to {Remaining}",
                progress.ComboName,
                progress.WindowRemaining);

            if (progress.IsExpired)
            {
                expired.Add(progress);

                _logger.LogInformation(
                    "TickCombos: {Combatant}'s {ComboName} combo expired (reached step {Step}/{Total})",
                    combatant.DisplayName,
                    progress.ComboName,
                    progress.CurrentStep,
                    progress.TotalSteps);

                // Log event
                _eventLogger.LogCombat(
                    "ComboFailed",
                    $"{combatant.DisplayName}'s {progress.ComboName} combo expired",
                    data: new Dictionary<string, object>
                    {
                        ["combatantId"] = combatant.Id,
                        ["comboId"] = progress.ComboId,
                        ["comboName"] = progress.ComboName,
                        ["reason"] = "Window expired",
                        ["stepReached"] = progress.CurrentStep
                    });
            }
        }

        // Remove expired combos
        foreach (var progress in expired)
        {
            progressList.Remove(progress);
        }

        if (expired.Count > 0)
        {
            _logger.LogDebug(
                "TickCombos: Removed {Count} expired combos for {Combatant}",
                expired.Count,
                combatant.DisplayName);
        }
    }

    /// <inheritdoc />
    public void ResetProgress(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        var clearedCount = 0;

        if (_activeProgress.TryGetValue(combatant.Id, out var progressList))
        {
            clearedCount = progressList.Count;
            progressList.Clear();

            _logger.LogInformation(
                "ResetProgress: Cleared {Count} active combos for {Combatant}",
                clearedCount,
                combatant.DisplayName);

            // Log event
            _eventLogger.LogCombat(
                "ComboProgressReset",
                $"{combatant.DisplayName}'s combo progress cleared",
                data: new Dictionary<string, object>
                {
                    ["combatantId"] = combatant.Id,
                    ["combosCleared"] = clearedCount
                });
        }

        // Also reset completed count
        if (_completedCounts.ContainsKey(combatant.Id))
        {
            _completedCounts[combatant.Id] = 0;
        }
    }

    /// <inheritdoc />
    public bool HasActiveProgress(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        return _activeProgress.TryGetValue(combatant.Id, out var progress) && progress.Count > 0;
    }

    /// <inheritdoc />
    public int GetCompletedComboCount(Combatant combatant)
    {
        ArgumentNullException.ThrowIfNull(combatant);

        return _completedCounts.TryGetValue(combatant.Id, out var count) ? count : 0;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - COMBO ADVANCEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Attempts to advance active combos with the used ability.
    /// </summary>
    /// <param name="user">The combatant who used the ability.</param>
    /// <param name="abilityId">The ability that was used.</param>
    /// <param name="targetId">The target ID (null for self).</param>
    /// <param name="isSelfTarget">Whether the ability was self-targeted.</param>
    /// <returns>A tuple containing action results and completed combo definitions.</returns>
    private (List<ComboActionResult> actions, List<ComboDefinition> completed) TryAdvanceActiveCombos(
        Combatant user,
        string abilityId,
        Guid? targetId,
        bool isSelfTarget)
    {
        var actions = new List<ComboActionResult>();
        var completed = new List<ComboDefinition>();

        if (!_activeProgress.TryGetValue(user.Id, out var progressList) || progressList.Count == 0)
        {
            return (actions, completed);
        }

        // Track combos to remove (can't modify while iterating)
        var toRemove = new List<ComboProgress>();

        foreach (var progress in progressList)
        {
            var combo = _comboProvider.GetCombo(progress.ComboId);
            if (combo == null)
            {
                _logger.LogWarning(
                    "TryAdvanceActiveCombos: Combo definition not found: {ComboId}",
                    progress.ComboId);
                toRemove.Add(progress);
                continue;
            }

            // Get the next step to match
            var nextStep = combo.GetStep(progress.NextStep);
            if (nextStep == null)
            {
                _logger.LogWarning(
                    "TryAdvanceActiveCombos: Step {Step} not found in combo {ComboId}",
                    progress.NextStep,
                    progress.ComboId);
                continue;
            }

            // Check if this ability matches the next step
            var isSameTarget = progress.LastTargetId.HasValue &&
                               targetId.HasValue &&
                               progress.LastTargetId.Value == targetId.Value;

            // Check if ability ID matches (for detecting target requirement failures)
            var abilityIdMatches = nextStep.AbilityId.Equals(abilityId, StringComparison.OrdinalIgnoreCase);

            if (!nextStep.Matches(abilityId, isSameTarget, isSelfTarget))
            {
                // If ability ID matches but full match fails, it's a target requirement failure
                // This should fail the combo
                if (abilityIdMatches)
                {
                    var failReason = nextStep.TargetRequirement switch
                    {
                        ComboTargetRequirement.SameTarget => "Target mismatch - required same target as previous step",
                        ComboTargetRequirement.DifferentTarget => "Target mismatch - required different target from previous step",
                        ComboTargetRequirement.Self => "Target mismatch - required self-targeting",
                        _ => "Target requirement not met"
                    };

                    _logger.LogInformation(
                        "TryAdvanceActiveCombos: {ComboName} FAILED for {Combatant}: {Reason}",
                        progress.ComboName,
                        user.DisplayName,
                        failReason);

                    actions.Add(ComboActionResult.Failed(
                        progress.ComboId,
                        progress.ComboName,
                        failReason));

                    toRemove.Add(progress);

                    // Log event
                    _eventLogger.LogCombat(
                        "ComboFailed",
                        $"{user.DisplayName}'s {progress.ComboName} failed: {failReason}",
                        data: new Dictionary<string, object>
                        {
                            ["combatantId"] = user.Id,
                            ["comboId"] = progress.ComboId,
                            ["comboName"] = progress.ComboName,
                            ["reason"] = failReason,
                            ["stepReached"] = progress.CurrentStep
                        });

                    continue;
                }

                _logger.LogDebug(
                    "TryAdvanceActiveCombos: {Ability} does not match step {Step} of {ComboName} " +
                    "(expected {Expected}, target requirement: {Requirement})",
                    abilityId,
                    progress.NextStep,
                    progress.ComboName,
                    nextStep.AbilityId,
                    nextStep.TargetRequirement);
                continue;
            }

            // Ability matches! Advance the combo
            progress.AdvanceStep(targetId);

            _logger.LogDebug(
                "TryAdvanceActiveCombos: Advanced {ComboName} to step {Step}/{Total}",
                progress.ComboName,
                progress.CurrentStep,
                progress.TotalSteps);

            // Check if combo is now complete
            if (progress.IsComplete)
            {
                completed.Add(combo);
                toRemove.Add(progress);

                // Increment completed count
                if (!_completedCounts.ContainsKey(user.Id))
                {
                    _completedCounts[user.Id] = 0;
                }
                _completedCounts[user.Id]++;

                actions.Add(ComboActionResult.Completed(
                    progress.ComboId,
                    progress.ComboName,
                    progress.TotalSteps));

                _logger.LogInformation(
                    "TryAdvanceActiveCombos: {Combatant} COMPLETED {ComboName}!",
                    user.DisplayName,
                    progress.ComboName);

                // Log event
                _eventLogger.LogCombat(
                    "ComboCompleted",
                    $"{user.DisplayName} completed {progress.ComboName}!",
                    data: new Dictionary<string, object>
                    {
                        ["combatantId"] = user.Id,
                        ["comboId"] = progress.ComboId,
                        ["comboName"] = progress.ComboName,
                        ["bonusEffects"] = combo.BonusEffects.Count
                    });
            }
            else
            {
                actions.Add(ComboActionResult.Progressed(
                    progress.ComboId,
                    progress.ComboName,
                    progress.CurrentStep,
                    progress.TotalSteps));

                _logger.LogInformation(
                    "TryAdvanceActiveCombos: {Combatant} progressed {ComboName} to step {Step}/{Total}",
                    user.DisplayName,
                    progress.ComboName,
                    progress.CurrentStep,
                    progress.TotalSteps);

                // Log event
                _eventLogger.LogCombat(
                    "ComboProgressed",
                    $"{user.DisplayName} progressed {progress.ComboName} ({progress.GetProgressString()})",
                    data: new Dictionary<string, object>
                    {
                        ["combatantId"] = user.Id,
                        ["comboId"] = progress.ComboId,
                        ["comboName"] = progress.ComboName,
                        ["currentStep"] = progress.CurrentStep,
                        ["totalSteps"] = progress.TotalSteps,
                        ["windowRemaining"] = progress.WindowRemaining
                    });
            }
        }

        // Remove completed combos
        foreach (var progress in toRemove)
        {
            progressList.Remove(progress);
        }

        return (actions, completed);
    }

    /// <summary>
    /// Attempts to start new combos with the used ability.
    /// </summary>
    /// <param name="user">The combatant who used the ability.</param>
    /// <param name="abilityId">The ability that was used.</param>
    /// <param name="targetId">The target ID (null for self).</param>
    /// <returns>A list of started combo action results.</returns>
    private List<ComboActionResult> TryStartNewCombos(Combatant user, string abilityId, Guid? targetId)
    {
        var actions = new List<ComboActionResult>();

        // Get the user's class for filtering
        var classId = GetClassId(user);

        // Find combos that start with this ability
        var potentialCombos = _comboProvider.GetCombosStartingWith(abilityId);

        _logger.LogDebug(
            "TryStartNewCombos: Found {Count} combos starting with {Ability}",
            potentialCombos.Count,
            abilityId);

        foreach (var combo in potentialCombos)
        {
            // Check class restriction
            if (!string.IsNullOrEmpty(classId) && !combo.IsAvailableForClass(classId))
            {
                _logger.LogDebug(
                    "TryStartNewCombos: {ComboName} not available for class {Class}",
                    combo.Name,
                    classId);
                continue;
            }

            // Check if we're already tracking this combo
            if (IsComboAlreadyTracked(user.Id, combo.ComboId))
            {
                _logger.LogDebug(
                    "TryStartNewCombos: {ComboName} already being tracked for {Combatant}",
                    combo.Name,
                    user.DisplayName);
                continue;
            }

            // Start tracking this combo
            var progress = ComboProgress.Start(combo, targetId);
            EnsureProgressList(user.Id).Add(progress);

            actions.Add(ComboActionResult.Started(combo.ComboId, combo.Name, combo.StepCount));

            _logger.LogInformation(
                "TryStartNewCombos: {Combatant} started {ComboName} ({Window} turn window)",
                user.DisplayName,
                combo.Name,
                combo.WindowTurns);

            // Log event
            _eventLogger.LogCombat(
                "ComboStarted",
                $"{user.DisplayName} started {combo.Name}",
                data: new Dictionary<string, object>
                {
                    ["combatantId"] = user.Id,
                    ["comboId"] = combo.ComboId,
                    ["comboName"] = combo.Name,
                    ["totalSteps"] = combo.StepCount,
                    ["windowTurns"] = combo.WindowTurns
                });
        }

        return actions;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - BONUS EFFECTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies bonus effects for a completed combo.
    /// </summary>
    /// <param name="user">The combatant who completed the combo.</param>
    /// <param name="combo">The completed combo definition.</param>
    /// <param name="target">The last target (for effect application).</param>
    private void ApplyBonusEffects(Combatant user, ComboDefinition combo, Combatant? target)
    {
        if (!combo.HasBonusEffects())
        {
            _logger.LogDebug(
                "ApplyBonusEffects: {ComboName} has no bonus effects",
                combo.Name);
            return;
        }

        _logger.LogDebug(
            "ApplyBonusEffects: Applying {Count} bonus effects for {ComboName}",
            combo.BonusEffects.Count,
            combo.Name);

        foreach (var effect in combo.BonusEffects)
        {
            ApplyBonusEffect(user, combo, effect, target);
        }
    }

    /// <summary>
    /// Applies a single bonus effect.
    /// </summary>
    /// <param name="user">The combatant who completed the combo.</param>
    /// <param name="combo">The completed combo definition.</param>
    /// <param name="effect">The bonus effect to apply.</param>
    /// <param name="target">The last target (for effect application).</param>
    private void ApplyBonusEffect(
        Combatant user,
        ComboDefinition combo,
        ComboBonusEffect effect,
        Combatant? target)
    {
        var description = effect.GetDescription();

        _logger.LogDebug(
            "ApplyBonusEffect: Applying {Effect} from {ComboName}",
            description,
            combo.Name);

        // Determine the actual target based on ComboBonusTarget
        var effectTarget = ResolveEffectTarget(user, target, effect.Target);

        switch (effect.EffectType)
        {
            case ComboBonusType.ExtraDamage:
                ApplyExtraDamage(user, combo, effect, effectTarget);
                break;

            case ComboBonusType.DamageMultiplier:
                LogDamageMultiplier(user, combo, effect);
                break;

            case ComboBonusType.ApplyStatus:
                ApplyStatusEffect(user, combo, effect, effectTarget);
                break;

            case ComboBonusType.Heal:
                ApplyHealing(user, combo, effect, effectTarget);
                break;

            case ComboBonusType.ResetCooldown:
                LogResetCooldown(user, combo, effect);
                break;

            case ComboBonusType.RefundResource:
                LogRefundResource(user, combo, effect);
                break;

            case ComboBonusType.AreaEffect:
                LogAreaEffect(user, combo, effect);
                break;

            default:
                _logger.LogWarning(
                    "ApplyBonusEffect: Unknown effect type {Type} in {ComboName}",
                    effect.EffectType,
                    combo.Name);
                break;
        }
    }

    /// <summary>
    /// Applies extra damage from a combo bonus.
    /// </summary>
    private void ApplyExtraDamage(Combatant user, ComboDefinition combo, ComboBonusEffect effect, Combatant? target)
    {
        if (target == null)
        {
            _logger.LogWarning(
                "ApplyExtraDamage: No target for damage bonus from {ComboName}",
                combo.Name);
            return;
        }

        // Roll the damage dice
        var diceNotation = effect.Value;
        var rollResult = _diceService.Roll(diceNotation);
        var damage = rollResult.Total;
        var damageType = effect.DamageType ?? "physical";

        _logger.LogInformation(
            "ApplyExtraDamage: {User} deals {Damage} extra {Type} damage to {Target} from {ComboName} ({Notation}={Result})",
            user.DisplayName,
            damage,
            damageType,
            target.DisplayName,
            combo.Name,
            diceNotation,
            rollResult);

        // Apply damage to target
        var effectTarget = GetEffectTarget(target);
        if (effectTarget != null)
        {
            var actualDamage = effectTarget.TakeDamage(damage);

            _logger.LogDebug(
                "ApplyExtraDamage: {Target} took {Damage} damage (actual)",
                target.DisplayName,
                actualDamage);
        }

        // Log event
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} dealt {damage} extra {damageType} damage to {target.DisplayName}",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["damage"] = damage,
                ["damageType"] = damageType,
                ["targetId"] = target.Id
            });
    }

    /// <summary>
    /// Logs a damage multiplier bonus (for external systems to apply).
    /// </summary>
    private void LogDamageMultiplier(Combatant user, ComboDefinition combo, ComboBonusEffect effect)
    {
        _logger.LogInformation(
            "LogDamageMultiplier: {User}'s {ComboName} grants {Value}x damage multiplier",
            user.DisplayName,
            combo.Name,
            effect.Value);

        // Log event for external systems to handle
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} grants {effect.Value}x damage multiplier",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["multiplier"] = effect.Value
            });
    }

    /// <summary>
    /// Applies a status effect from a combo bonus.
    /// </summary>
    private void ApplyStatusEffect(Combatant user, ComboDefinition combo, ComboBonusEffect effect, Combatant? target)
    {
        if (string.IsNullOrEmpty(effect.StatusEffectId))
        {
            _logger.LogWarning(
                "ApplyStatusEffect: No status effect ID specified for {ComboName}",
                combo.Name);
            return;
        }

        if (target == null)
        {
            _logger.LogWarning(
                "ApplyStatusEffect: No target for status effect from {ComboName}",
                combo.Name);
            return;
        }

        var effectTarget = GetEffectTarget(target);
        if (effectTarget == null)
        {
            _logger.LogWarning(
                "ApplyStatusEffect: Target {Target} is not an effect target",
                target.DisplayName);
            return;
        }

        _logger.LogInformation(
            "ApplyStatusEffect: Applying {Effect} to {Target} from {ComboName}",
            effect.StatusEffectId,
            target.DisplayName,
            combo.Name);

        // Apply the status effect
        var result = _buffDebuffService.ApplyEffect(
            effectTarget,
            effect.StatusEffectId,
            user.Id,
            user.DisplayName);

        if (result.WasApplied)
        {
            _logger.LogDebug(
                "ApplyStatusEffect: Successfully applied {Effect} to {Target}",
                effect.StatusEffectId,
                target.DisplayName);
        }
        else
        {
            _logger.LogDebug(
                "ApplyStatusEffect: Failed to apply {Effect} to {Target}: {Reason}",
                effect.StatusEffectId,
                target.DisplayName,
                result.Message);
        }

        // Log event
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} applied {effect.StatusEffectId} to {target.DisplayName}",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["statusEffectId"] = effect.StatusEffectId,
                ["targetId"] = target.Id,
                ["success"] = result.WasApplied
            });
    }

    /// <summary>
    /// Applies healing from a combo bonus.
    /// </summary>
    private void ApplyHealing(Combatant user, ComboDefinition combo, ComboBonusEffect effect, Combatant? target)
    {
        var healTarget = target ?? user;

        // Roll the healing dice
        var diceNotation = effect.Value;
        var rollResult = _diceService.Roll(diceNotation);
        var healing = rollResult.Total;

        _logger.LogInformation(
            "ApplyHealing: {Target} healed for {Amount} from {ComboName} ({Notation}={Result})",
            healTarget.DisplayName,
            healing,
            combo.Name,
            diceNotation,
            rollResult);

        // Apply healing
        var effectTarget = GetEffectTarget(healTarget);
        if (effectTarget != null)
        {
            var actualHealing = effectTarget.Heal(healing);

            _logger.LogDebug(
                "ApplyHealing: {Target} healed for {Amount} (actual)",
                healTarget.DisplayName,
                actualHealing);
        }

        // Log event
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} healed {healTarget.DisplayName} for {healing}",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["healing"] = healing,
                ["targetId"] = healTarget.Id
            });
    }

    /// <summary>
    /// Logs a cooldown reset bonus (for external cooldown manager to handle).
    /// </summary>
    private void LogResetCooldown(Combatant user, ComboDefinition combo, ComboBonusEffect effect)
    {
        _logger.LogInformation(
            "LogResetCooldown: {User}'s {ComboName} resets {Ability} cooldown",
            user.DisplayName,
            combo.Name,
            effect.Value);

        // Log event for external systems (cooldown manager) to handle
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} resets {effect.Value} cooldown",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["abilityId"] = effect.Value
            });
    }

    /// <summary>
    /// Logs a resource refund bonus (for external resource manager to handle).
    /// </summary>
    private void LogRefundResource(Combatant user, ComboDefinition combo, ComboBonusEffect effect)
    {
        _logger.LogInformation(
            "LogRefundResource: {User}'s {ComboName} refunds {Amount} resource",
            user.DisplayName,
            combo.Name,
            effect.Value);

        // Log event for external systems (resource manager) to handle
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} refunds {effect.Value} resource",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["amount"] = effect.Value
            });
    }

    /// <summary>
    /// Logs an area effect expansion (for external combat system to handle).
    /// </summary>
    private void LogAreaEffect(Combatant user, ComboDefinition combo, ComboBonusEffect effect)
    {
        _logger.LogInformation(
            "LogAreaEffect: {User}'s {ComboName} expands to {Radius} cell radius",
            user.DisplayName,
            combo.Name,
            effect.Value);

        // Log event for external systems (combat/targeting) to handle
        _eventLogger.LogCombat(
            "ComboBonusApplied",
            $"{user.DisplayName}'s {combo.Name} expands to {effect.Value} cell radius",
            data: new Dictionary<string, object>
            {
                ["combatantId"] = user.Id,
                ["comboId"] = combo.ComboId,
                ["comboName"] = combo.Name,
                ["effectType"] = effect.EffectType.ToString(),
                ["radius"] = effect.Value
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the class ID for a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>The class ID, or empty string if not applicable.</returns>
    private static string GetClassId(Combatant combatant)
    {
        // Players have a ClassId property
        if (combatant.IsPlayer && combatant.Player != null)
        {
            return combatant.Player.ClassId ?? string.Empty;
        }

        // Monsters don't have class-based combos
        return string.Empty;
    }

    /// <summary>
    /// Gets the IEffectTarget from a combatant.
    /// </summary>
    /// <param name="combatant">The combatant to query.</param>
    /// <returns>The effect target interface, or null if not applicable.</returns>
    /// <remarks>
    /// <para>Uses safe cast (as) since Player/Monster may not implement IEffectTarget.</para>
    /// <para>Returns null if the underlying entity doesn't support effect targeting.</para>
    /// </remarks>
    private static IEffectTarget? GetEffectTarget(Combatant combatant)
    {
        if (combatant.IsPlayer)
        {
            return combatant.Player as IEffectTarget;
        }

        if (combatant.IsMonster)
        {
            return combatant.Monster as IEffectTarget;
        }

        return null;
    }

    /// <summary>
    /// Resolves the actual target based on the bonus target type.
    /// </summary>
    /// <param name="user">The combatant who completed the combo.</param>
    /// <param name="lastTarget">The last target hit during the combo.</param>
    /// <param name="targetType">The bonus effect target type.</param>
    /// <returns>The resolved target combatant.</returns>
    private static Combatant? ResolveEffectTarget(
        Combatant user,
        Combatant? lastTarget,
        ComboBonusTarget targetType)
    {
        return targetType switch
        {
            ComboBonusTarget.Self => user,
            ComboBonusTarget.LastTarget => lastTarget,
            ComboBonusTarget.AllHitTargets => lastTarget, // Simplified - full implementation would iterate
            ComboBonusTarget.Area => lastTarget, // Area targeting uses last target as center
            _ => lastTarget
        };
    }

    /// <summary>
    /// Checks if a combo is already being tracked for a combatant.
    /// </summary>
    /// <param name="combatantId">The combatant ID.</param>
    /// <param name="comboId">The combo ID to check.</param>
    /// <returns>True if the combo is already tracked.</returns>
    private bool IsComboAlreadyTracked(Guid combatantId, string comboId)
    {
        if (!_activeProgress.TryGetValue(combatantId, out var progressList))
        {
            return false;
        }

        return progressList.Any(p =>
            p.ComboId.Equals(comboId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Ensures a progress list exists for a combatant.
    /// </summary>
    /// <param name="combatantId">The combatant ID.</param>
    /// <returns>The progress list for the combatant.</returns>
    private List<ComboProgress> EnsureProgressList(Guid combatantId)
    {
        if (!_activeProgress.TryGetValue(combatantId, out var list))
        {
            list = [];
            _activeProgress[combatantId] = list;
        }

        return list;
    }

    /// <summary>
    /// Gets a summary string of action types.
    /// </summary>
    /// <param name="actions">The list of actions.</param>
    /// <returns>A summary like "1 started, 1 completed".</returns>
    private static string GetActionSummary(List<ComboActionResult> actions)
    {
        var started = actions.Count(a => a.ActionType == ComboActionType.Started);
        var progressed = actions.Count(a => a.ActionType == ComboActionType.Progressed);
        var completed = actions.Count(a => a.ActionType == ComboActionType.Completed);
        var failed = actions.Count(a => a.ActionType == ComboActionType.Failed);

        var parts = new List<string>();
        if (started > 0) parts.Add($"{started} started");
        if (progressed > 0) parts.Add($"{progressed} progressed");
        if (completed > 0) parts.Add($"{completed} completed");
        if (failed > 0) parts.Add($"{failed} failed");

        return string.Join(", ", parts);
    }
}
