namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the type of action an AI can take during its turn.
/// </summary>
public enum AIAction
{
    /// <summary>Attack a target.</summary>
    Attack,

    /// <summary>Defend, gaining damage reduction for the round.</summary>
    Defend,

    /// <summary>Heal self or ally.</summary>
    Heal,

    /// <summary>Apply buff to self or ally (future use).</summary>
    Buff,

    /// <summary>Attempt to flee combat.</summary>
    Flee,

    /// <summary>Do nothing (skip turn).</summary>
    Wait
}
