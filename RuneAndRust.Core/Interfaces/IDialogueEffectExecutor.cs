using RuneAndRust.Core.Effects;
using RuneAndRust.Core.Models;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service for executing dialogue effects on characters.
/// </summary>
/// <remarks>See: v0.4.2c (The Voice) for DialogueService implementation.</remarks>
public interface IDialogueEffectExecutor
{
    /// <summary>
    /// Executes a single dialogue effect.
    /// </summary>
    /// <param name="character">The character to apply the effect to.</param>
    /// <param name="effect">The effect to execute.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>The result of executing the effect.</returns>
    Task<DialogueEffectResult> ExecuteEffectAsync(
        Character character,
        DialogueEffect effect,
        GameState gameState);

    /// <summary>
    /// Executes multiple dialogue effects in sequence.
    /// </summary>
    /// <param name="character">The character to apply effects to.</param>
    /// <param name="effects">The effects to execute.</param>
    /// <param name="gameState">The current game state.</param>
    /// <returns>Results for all executed effects.</returns>
    Task<IReadOnlyList<DialogueEffectResult>> ExecuteEffectsAsync(
        Character character,
        IEnumerable<DialogueEffect> effects,
        GameState gameState);
}
