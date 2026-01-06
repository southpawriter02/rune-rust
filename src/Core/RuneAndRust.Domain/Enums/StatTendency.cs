namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Indicates the general stat focus of an archetype or class.
/// </summary>
public enum StatTendency
{
    /// <summary>High attack, low defense.</summary>
    Offensive,

    /// <summary>High defense, lower attack.</summary>
    Defensive,

    /// <summary>No strong bias in either direction.</summary>
    Balanced,

    /// <summary>Focus on supporting allies (healing, buffs).</summary>
    Support
}
