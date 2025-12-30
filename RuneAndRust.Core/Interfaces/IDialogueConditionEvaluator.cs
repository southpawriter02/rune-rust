using RuneAndRust.Core.Conditions;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for evaluating dialogue conditions against a character.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public interface IDialogueConditionEvaluator
{
    /// <summary>
    /// Evaluates a single condition.
    /// </summary>
    /// <param name="character">The character to evaluate against.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>The evaluation result.</returns>
    Task<ConditionResult> EvaluateConditionAsync(Character character, DialogueCondition condition);

    /// <summary>
    /// Evaluates all conditions on a dialogue option.
    /// All conditions must pass for the option to be available (AND logic).
    /// </summary>
    /// <param name="character">The character to evaluate against.</param>
    /// <param name="option">The dialogue option to evaluate.</param>
    /// <returns>The visibility result for the option.</returns>
    Task<OptionVisibilityResult> EvaluateOptionAsync(Character character, DialogueOption option);

    /// <summary>
    /// Evaluates all options on a dialogue node.
    /// Returns visibility results for each option.
    /// </summary>
    /// <param name="character">The character to evaluate against.</param>
    /// <param name="node">The dialogue node containing options.</param>
    /// <returns>Visibility results for all options.</returns>
    Task<IReadOnlyList<OptionVisibilityResult>> EvaluateNodeOptionsAsync(
        Character character,
        DialogueNode node);
}
