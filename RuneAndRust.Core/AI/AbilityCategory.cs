namespace RuneAndRust.Core.AI;

/// <summary>
/// Categories of abilities for AI decision-making.
/// v0.42.2: Ability Usage & Behavior Patterns
/// </summary>
public enum AbilityCategory
{
    /// <summary>
    /// Basic attack (always available).
    /// </summary>
    BasicAttack,

    /// <summary>
    /// Damage-dealing ability (primary purpose is to deal damage).
    /// </summary>
    Damage,

    /// <summary>
    /// Healing ability (restores HP to allies or self).
    /// </summary>
    Healing,

    /// <summary>
    /// Buff ability (provides positive status effects).
    /// </summary>
    Buff,

    /// <summary>
    /// Debuff ability (applies negative status effects).
    /// </summary>
    Debuff,

    /// <summary>
    /// Crowd control ability (stuns, roots, fears, etc.).
    /// </summary>
    CrowdControl,

    /// <summary>
    /// Defensive ability (shields, defensive buffs, evasion).
    /// </summary>
    Defensive,

    /// <summary>
    /// Movement ability (repositioning, teleports, charges).
    /// </summary>
    Movement,

    /// <summary>
    /// AOE ability (area of effect damage or utility).
    /// </summary>
    AOE,

    /// <summary>
    /// Summoning ability (spawns adds or constructs).
    /// </summary>
    Summoning,

    /// <summary>
    /// Ultimate ability (powerful ability with long cooldown).
    /// </summary>
    Ultimate
}
