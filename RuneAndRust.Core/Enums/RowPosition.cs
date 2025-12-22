namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the tactical row positions for combat (v0.3.6a).
/// Row position affects melee targeting - Back Row is protected by Front Row.
/// </summary>
public enum RowPosition
{
    /// <summary>
    /// Frontline position. Melee combatants. Can be targeted by melee attacks.
    /// </summary>
    Front = 0,

    /// <summary>
    /// Backline position. Ranged/support combatants. Protected from melee unless Front is empty.
    /// </summary>
    Back = 1
}
