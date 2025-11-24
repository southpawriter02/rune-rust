namespace RuneAndRust.Core.AI;

/// <summary>
/// Represents an action decided by the AI for an enemy.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class EnemyAction
{
    /// <summary>
    /// The enemy performing the action.
    /// </summary>
    public Enemy Actor { get; set; } = null!;

    /// <summary>
    /// The selected target (if applicable).
    /// </summary>
    public object? Target { get; set; }

    /// <summary>
    /// Selected ability ID (0 = basic attack).
    /// </summary>
    public int SelectedAbilityId { get; set; }

    /// <summary>
    /// Position to move to (if moving).
    /// </summary>
    public (int X, int Y)? MoveTo { get; set; }

    /// <summary>
    /// Aggression modifier for this action (-1.0 to +1.0).
    /// </summary>
    public decimal AggressionModifier { get; set; }

    /// <summary>
    /// Decision context (for debugging).
    /// </summary>
    public DecisionContext? Context { get; set; }

    /// <summary>
    /// Action priority (higher = execute first).
    /// </summary>
    public int Priority { get; set; } = 1;
}
