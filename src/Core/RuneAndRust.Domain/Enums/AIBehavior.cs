namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the behavioral pattern for monster AI decision-making.
/// </summary>
/// <remarks>
/// <para>Each behavior type determines how a monster prioritizes actions and selects targets.</para>
/// <para>Behaviors can be configured per monster type via JSON configuration.</para>
/// </remarks>
public enum AIBehavior
{
    /// <summary>
    /// Always attacks, prioritizes lowest HP target.
    /// Uses strongest available ability.
    /// </summary>
    Aggressive,

    /// <summary>
    /// Attacks when HP is above 50%, otherwise heals or defends.
    /// Balanced between offense and self-preservation.
    /// </summary>
    Defensive,

    /// <summary>
    /// Attempts to flee when HP drops below 30%.
    /// Otherwise attacks the weakest target.
    /// </summary>
    Cowardly,

    /// <summary>
    /// Prioritizes healing and buffing allies.
    /// Only attacks when alone or no allies need help.
    /// </summary>
    Support,

    /// <summary>
    /// Unpredictable random actions.
    /// May attack, heal, or do nothing.
    /// </summary>
    Chaotic
}
