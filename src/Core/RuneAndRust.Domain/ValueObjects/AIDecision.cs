using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a decision made by the monster AI.
/// </summary>
/// <remarks>
/// <para>AIDecision is an immutable value object capturing the result of AI deliberation.</para>
/// <para>The <see cref="Reasoning"/> property provides human-readable explanation for debugging and display.</para>
/// </remarks>
/// <param name="Action">The type of action to take.</param>
/// <param name="Target">The target of the action (if applicable).</param>
/// <param name="AbilityId">The ability to use (if applicable, for future use).</param>
/// <param name="Reasoning">Human-readable explanation of the decision.</param>
public readonly record struct AIDecision(
    AIAction Action,
    Combatant? Target,
    string? AbilityId,
    string Reasoning)
{
    /// <summary>
    /// Creates an attack decision targeting a specific combatant.
    /// </summary>
    /// <param name="target">The combatant to attack.</param>
    /// <param name="reasoning">Explanation for why this target was chosen.</param>
    /// <returns>An attack decision.</returns>
    public static AIDecision Attack(Combatant target, string reasoning) =>
        new(AIAction.Attack, target, null, reasoning);

    /// <summary>
    /// Creates a defend decision (no target needed).
    /// </summary>
    /// <param name="reasoning">Explanation for defending.</param>
    /// <returns>A defend decision.</returns>
    public static AIDecision Defend(string reasoning) =>
        new(AIAction.Defend, null, null, reasoning);

    /// <summary>
    /// Creates a heal decision targeting self or ally.
    /// </summary>
    /// <param name="target">The combatant to heal.</param>
    /// <param name="abilityId">Optional ability ID (for future use).</param>
    /// <param name="reasoning">Explanation for healing.</param>
    /// <returns>A heal decision.</returns>
    public static AIDecision Heal(Combatant target, string? abilityId, string reasoning) =>
        new(AIAction.Heal, target, abilityId, reasoning);

    /// <summary>
    /// Creates a flee decision (monster attempts to escape).
    /// </summary>
    /// <param name="reasoning">Explanation for fleeing.</param>
    /// <returns>A flee decision.</returns>
    public static AIDecision Flee(string reasoning) =>
        new(AIAction.Flee, null, null, reasoning);

    /// <summary>
    /// Creates a wait decision (skip turn).
    /// </summary>
    /// <param name="reasoning">Explanation for waiting.</param>
    /// <returns>A wait decision.</returns>
    public static AIDecision Wait(string reasoning) =>
        new(AIAction.Wait, null, null, reasoning);

    /// <summary>
    /// Returns a formatted string describing the decision.
    /// </summary>
    public override string ToString()
    {
        var targetStr = Target != null ? $" -> {Target.DisplayName}" : "";
        return $"{Action}{targetStr}: {Reasoning}";
    }
}
