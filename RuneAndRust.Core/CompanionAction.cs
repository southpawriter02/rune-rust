namespace RuneAndRust.Core;

/// <summary>
/// v0.34.2: Represents an AI-selected action for a companion to perform
/// Returned by CompanionAIService.SelectAction() and executed by CompanionService
/// </summary>
public class CompanionAction
{
    /// <summary>
    /// Type of action to perform
    /// Values: "Attack", "UseAbility", "Move", "Wait"
    /// </summary>
    public string ActionType { get; set; } = "Wait";

    /// <summary>
    /// Target enemy for Attack or UseAbility actions
    /// </summary>
    public Enemy? TargetEnemy { get; set; }

    /// <summary>
    /// Target position for Move actions
    /// </summary>
    public GridPosition? TargetPosition { get; set; }

    /// <summary>
    /// Ability name for UseAbility actions
    /// </summary>
    public string? AbilityName { get; set; }

    /// <summary>
    /// If true, the ability targets the companion itself (self-buff/heal)
    /// </summary>
    public bool TargetSelf { get; set; } = false;

    /// <summary>
    /// Human-readable explanation of why the AI chose this action
    /// Used for logging and debugging
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{ActionType}: {Reason}";
    }
}
