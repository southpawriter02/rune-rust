namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of social modifiers.
/// </summary>
/// <remarks>
/// <para>
/// Social modifiers are specific to social interaction checks and capture
/// factors like faction relationships, argument quality, and target state.
/// </para>
/// </remarks>
public enum SocialModifierType
{
    /// <summary>
    /// Modifier from faction relationship.
    /// </summary>
    /// <remarks>
    /// Applied based on player's standing with the target's faction.
    /// Honored = +1d10, Hostile = -2d10.
    /// </remarks>
    FactionRelation = 0,

    /// <summary>
    /// Modifier from argument quality/alignment.
    /// </summary>
    /// <remarks>
    /// Applied when persuasion arguments align (+1d10) or contradict (-1d10)
    /// the NPC's values and beliefs.
    /// </remarks>
    ArgumentQuality = 1,

    /// <summary>
    /// Modifier from supporting evidence.
    /// </summary>
    /// <remarks>
    /// Applied when player provides proof that supports their argument.
    /// Typically +2d10 for solid evidence.
    /// </remarks>
    Evidence = 2,

    /// <summary>
    /// Modifier from target's current state.
    /// </summary>
    /// <remarks>
    /// Applied based on target conditions like [Suspicious] (DC +4),
    /// [Trusting] (DC -4), or strength comparison for intimidation.
    /// </remarks>
    TargetState = 3,

    /// <summary>
    /// Modifier from player reputation.
    /// </summary>
    /// <remarks>
    /// Applied based on player reputation status like [Honored] (+1d10),
    /// [Feared] (+1d10 to intimidation), or [Untrustworthy] (DC +3).
    /// </remarks>
    Reputation = 4,

    /// <summary>
    /// Modifier from cultural factors.
    /// </summary>
    /// <remarks>
    /// Applied when interacting with NPCs of specific cultures.
    /// Knowledge of cant or protocol may grant bonuses.
    /// </remarks>
    Cultural = 5,

    /// <summary>
    /// Modifier from equipment (visible artifacts, etc.).
    /// </summary>
    /// <remarks>
    /// Applied when wielding impressive items like artifacts
    /// that enhance intimidation (+1d10).
    /// </remarks>
    Equipment = 6
}
