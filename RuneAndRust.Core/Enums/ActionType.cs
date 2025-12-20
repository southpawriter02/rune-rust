namespace RuneAndRust.Core.Enums;

/// <summary>
/// Types of actions an enemy can take during their turn.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Melee or ranged attack against a target.
    /// </summary>
    Attack,

    /// <summary>
    /// Defensive stance - trade damage for soak bonus.
    /// </summary>
    Defend,

    /// <summary>
    /// Flee from combat (cowardly enemies at low HP).
    /// </summary>
    Flee,

    /// <summary>
    /// Skip turn (stunned, out of stamina, no valid action).
    /// </summary>
    Pass
}
