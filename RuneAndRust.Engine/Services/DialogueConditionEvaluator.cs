using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Evaluates dialogue conditions against a character's current state.
/// Supports 8 condition types with AND logic for option evaluation.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class DialogueConditionEvaluator : IDialogueConditionEvaluator
{
    private readonly IFactionService _factionService;
    private readonly IInventoryService _inventoryService;
    private readonly ISpecializationRepository _specRepository;
    private readonly IDiceService _diceService;
    private readonly GameState _gameState;
    private readonly ILogger<DialogueConditionEvaluator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogueConditionEvaluator"/> class.
    /// </summary>
    public DialogueConditionEvaluator(
        IFactionService factionService,
        IInventoryService inventoryService,
        ISpecializationRepository specRepository,
        IDiceService diceService,
        GameState gameState,
        ILogger<DialogueConditionEvaluator> logger)
    {
        _factionService = factionService;
        _inventoryService = inventoryService;
        _specRepository = specRepository;
        _diceService = diceService;
        _gameState = gameState;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Single Condition Evaluation
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<ConditionResult> EvaluateConditionAsync(
        Character character,
        DialogueCondition condition)
    {
        _logger.LogTrace(
            "[DialogueCondition] Evaluating {ConditionType} for {CharName}",
            condition.Type, character.Name);

        var result = condition switch
        {
            AttributeCondition attr => EvaluateAttribute(character, attr),
            LevelCondition lvl => EvaluateLevel(character, lvl),
            ReputationCondition rep => await EvaluateReputationAsync(character, rep),
            FlagCondition flag => EvaluateFlag(flag),
            ItemCondition item => await EvaluateItemAsync(character, item),
            SpecializationCondition spec => await EvaluateSpecializationAsync(character, spec),
            NodeCondition node => await EvaluateNodeAsync(character, node),
            SkillCheckCondition skill => EvaluateSkillCheck(character, skill),
            _ => ConditionResult.Fail(
                condition.Type,
                condition.GetDisplayHint(),
                "Unknown condition type")
        };

        _logger.LogDebug(
            "[DialogueCondition] {ConditionType} = {Result} ({Hint})",
            condition.Type, result.Passed ? "PASS" : "FAIL", result.DisplayHint);

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Option Evaluation
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<OptionVisibilityResult> EvaluateOptionAsync(
        Character character,
        DialogueOption option)
    {
        _logger.LogTrace(
            "[DialogueCondition] Evaluating option '{OptionText}' ({ConditionCount} conditions)",
            option.Text.Length > 30 ? option.Text[..30] + "..." : option.Text,
            option.Conditions.Count);

        // No conditions = always available
        if (!option.HasConditions)
        {
            _logger.LogDebug("[DialogueCondition] No conditions, option available");
            return OptionVisibilityResult.Available(option.Id);
        }

        // Evaluate all conditions (AND logic)
        var results = new List<ConditionResult>();
        var allPassed = true;
        var anyHideWhenFailed = false;
        string? firstFailureReason = null;
        string? firstFailureHint = null;

        foreach (var condition in option.Conditions)
        {
            var result = await EvaluateConditionAsync(character, condition);
            results.Add(result);

            if (!result.Passed)
            {
                allPassed = false;
                firstFailureReason ??= result.FailureReason ?? "Requirement not met";
                firstFailureHint ??= result.DisplayHint;

                if (condition.HideWhenFailed)
                {
                    anyHideWhenFailed = true;
                }
            }
        }

        // All conditions passed
        if (allPassed)
        {
            _logger.LogDebug("[DialogueCondition] All {Count} conditions passed", results.Count);
            return OptionVisibilityResult.Available(option.Id);
        }

        // Check visibility mode
        // If any failing condition has HideWhenFailed=true, OR the option visibility is Hidden, hide the option
        if (anyHideWhenFailed || option.VisibilityMode == OptionVisibility.Hidden)
        {
            _logger.LogDebug("[DialogueCondition] Option hidden (HideWhenFailed or Hidden mode)");
            return OptionVisibilityResult.Hidden(option.Id);
        }

        // Show as locked with requirement hint
        _logger.LogDebug(
            "[DialogueCondition] Option locked: {Reason} {Hint}",
            firstFailureReason, firstFailureHint);

        return OptionVisibilityResult.Locked(
            option.Id,
            firstFailureReason!,
            firstFailureHint!,
            results);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OptionVisibilityResult>> EvaluateNodeOptionsAsync(
        Character character,
        DialogueNode node)
    {
        _logger.LogTrace(
            "[DialogueCondition] Evaluating {OptionCount} options for node '{NodeId}'",
            node.Options.Count, node.NodeId);

        var results = new List<OptionVisibilityResult>();

        foreach (var option in node.Options.OrderBy(o => o.DisplayOrder))
        {
            var result = await EvaluateOptionAsync(character, option);
            results.Add(result);
        }

        var visible = results.Count(r => r.IsVisible);
        var available = results.Count(r => r.IsAvailable);

        _logger.LogDebug(
            "[DialogueCondition] Node evaluation complete: {Total} options, {Visible} visible, {Available} available",
            results.Count, visible, available);

        return results;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Condition Type Evaluators
    // ═══════════════════════════════════════════════════════════════════════

    private ConditionResult EvaluateAttribute(Character character, AttributeCondition condition)
    {
        var value = character.GetEffectiveAttribute(condition.Attribute);
        var passed = CompareValues(value, condition.Comparison, condition.Threshold);
        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[AttributeCondition] {Attr} = {Value}, Threshold = {Threshold}, Comparison = {Comp}, Result = {Result}",
            condition.Attribute, value, condition.Threshold, condition.Comparison, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Attribute, hint);
        }

        return ConditionResult.Fail(
            DialogueConditionType.Attribute,
            hint,
            $"Requires {condition.Attribute} {GetComparisonSymbol(condition.Comparison)} {condition.Threshold}");
    }

    private ConditionResult EvaluateLevel(Character character, LevelCondition condition)
    {
        var passed = character.Level >= condition.MinLevel;
        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[LevelCondition] CharLevel = {Level}, Required = {Min}, Result = {Result}",
            character.Level, condition.MinLevel, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Level, hint);
        }

        return ConditionResult.Fail(
            DialogueConditionType.Level,
            hint,
            $"Requires level {condition.MinLevel}");
    }

    private async Task<ConditionResult> EvaluateReputationAsync(
        Character character,
        ReputationCondition condition)
    {
        var passed = await _factionService.MeetsDispositionRequirementAsync(
            character,
            condition.Faction,
            condition.MinDisposition);

        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[ReputationCondition] Faction = {Faction}, Required = {Disposition}, Result = {Result}",
            condition.Faction, condition.MinDisposition, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Reputation, hint);
        }

        return ConditionResult.Fail(
            DialogueConditionType.Reputation,
            hint,
            $"Requires {condition.MinDisposition} standing with {condition.Faction}");
    }

    private ConditionResult EvaluateFlag(FlagCondition condition)
    {
        var flagValue = _gameState.GetFlag(condition.FlagKey);
        var passed = flagValue == condition.RequiredValue;
        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[FlagCondition] Flag '{Key}' = {Value}, Required = {Required}, Result = {Result}",
            condition.FlagKey, flagValue, condition.RequiredValue, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Flag, hint);
        }

        var reason = condition.RequiredValue
            ? $"Requires: {condition.FlagKey}"
            : $"Blocked by: {condition.FlagKey}";

        return ConditionResult.Fail(DialogueConditionType.Flag, hint, reason);
    }

    private async Task<ConditionResult> EvaluateItemAsync(
        Character character,
        ItemCondition condition)
    {
        var hasItem = await _inventoryService.HasItemAsync(
            character.Id,
            condition.ItemId,
            condition.MinQuantity);

        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[ItemCondition] Item '{Item}' x{Qty} = {Result}",
            condition.ItemId, condition.MinQuantity, hasItem);

        if (hasItem)
        {
            return ConditionResult.Success(DialogueConditionType.Item, hint);
        }

        var reason = condition.MinQuantity > 1
            ? $"Requires {condition.ItemId} x{condition.MinQuantity}"
            : $"Requires {condition.ItemId}";

        return ConditionResult.Fail(DialogueConditionType.Item, hint, reason);
    }

    private async Task<ConditionResult> EvaluateSpecializationAsync(
        Character character,
        SpecializationCondition condition)
    {
        // Check if character has unlocked any node in this specialization
        var unlockedNodes = await _specRepository.GetUnlockedNodesAsync(character.Id);
        var passed = unlockedNodes.Any(n => n.SpecializationId == condition.SpecializationId);

        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[SpecializationCondition] Spec '{Name}' unlocked = {Result}",
            condition.SpecializationName, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Specialization, hint);
        }

        return ConditionResult.Fail(
            DialogueConditionType.Specialization,
            hint,
            $"Requires {condition.SpecializationName} specialization");
    }

    private async Task<ConditionResult> EvaluateNodeAsync(
        Character character,
        NodeCondition condition)
    {
        var unlockedNodes = await _specRepository.GetUnlockedNodesAsync(character.Id);
        var passed = unlockedNodes.Any(n => n.Id == condition.NodeId);

        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[NodeCondition] Node '{Name}' ({Id}) unlocked = {Result}",
            condition.NodeName, condition.NodeId, passed);

        if (passed)
        {
            return ConditionResult.Success(DialogueConditionType.Node, hint);
        }

        return ConditionResult.Fail(
            DialogueConditionType.Node,
            hint,
            $"Requires {condition.NodeName} ability");
    }

    private ConditionResult EvaluateSkillCheck(
        Character character,
        SkillCheckCondition condition)
    {
        var poolSize = character.GetEffectiveAttribute(condition.Attribute);
        var result = _diceService.Roll(poolSize, $"Dialogue skill check: {condition.CheckDescription ?? condition.Attribute.ToString()}");

        var netSuccesses = result.Successes - result.Botches;
        var passed = netSuccesses >= condition.DifficultyClass;
        var hint = condition.GetDisplayHint();

        _logger.LogTrace(
            "[SkillCheckCondition] {Attr} pool={Pool}, Rolled={Successes}s/{Botches}b, Net={Net}, DC={DC}, Result = {Result}",
            condition.Attribute, poolSize, result.Successes, result.Botches, netSuccesses, condition.DifficultyClass, passed);

        if (passed)
        {
            return ConditionResult.SkillCheckSuccess(hint, result.Rolls, netSuccesses);
        }

        var reason = netSuccesses < 0
            ? $"Botched! ({netSuccesses} successes vs DC {condition.DifficultyClass})"
            : $"Failed ({netSuccesses} successes vs DC {condition.DifficultyClass})";

        return ConditionResult.SkillCheckFail(hint, reason, result.Rolls, netSuccesses);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════

    private static bool CompareValues(int value, ComparisonType comparison, int threshold)
    {
        return comparison switch
        {
            ComparisonType.GreaterThanOrEqual => value >= threshold,
            ComparisonType.Equal => value == threshold,
            ComparisonType.GreaterThan => value > threshold,
            ComparisonType.LessThan => value < threshold,
            ComparisonType.LessThanOrEqual => value <= threshold,
            ComparisonType.NotEqual => value != threshold,
            _ => false
        };
    }

    private static string GetComparisonSymbol(ComparisonType comparison)
    {
        return comparison switch
        {
            ComparisonType.GreaterThanOrEqual => ">=",
            ComparisonType.Equal => "=",
            ComparisonType.GreaterThan => ">",
            ComparisonType.LessThan => "<",
            ComparisonType.LessThanOrEqual => "<=",
            ComparisonType.NotEqual => "!=",
            _ => ""
        };
    }
}
