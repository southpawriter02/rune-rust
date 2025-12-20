namespace RuneAndRust.Core.Enums;

/// <summary>
/// Represents the quality of an attack roll result.
/// Determines damage modifiers applied to the attack.
/// </summary>
public enum AttackOutcome
{
    /// <summary>
    /// Critical failure: 0 successes with 1+ botches.
    /// May trigger negative consequences for the attacker.
    /// </summary>
    Fumble = 0,

    /// <summary>
    /// Attack failed to connect. Net successes &lt;= 0.
    /// No damage dealt.
    /// </summary>
    Miss = 1,

    /// <summary>
    /// Weak hit. Net successes 1-2.
    /// Deals half damage.
    /// </summary>
    Glancing = 2,

    /// <summary>
    /// Clean hit. Net successes 3-4.
    /// Deals full damage.
    /// </summary>
    Solid = 3,

    /// <summary>
    /// Devastating hit. Net successes 5+.
    /// Deals double damage.
    /// </summary>
    Critical = 4
}
