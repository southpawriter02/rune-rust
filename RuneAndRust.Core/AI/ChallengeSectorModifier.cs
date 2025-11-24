namespace RuneAndRust.Core.AI;

/// <summary>
/// Types of Challenge Sector modifiers that affect AI behavior.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public enum SectorModifierType
{
    /// <summary>
    /// No healing allowed - AI prioritizes burst damage.
    /// </summary>
    NoHealing = 1,

    /// <summary>
    /// Double speed - AI uses abilities more aggressively.
    /// </summary>
    DoubleSpeed = 2,

    /// <summary>
    /// Players have 1 HP - AI plays cautiously.
    /// </summary>
    OneHP = 3,

    /// <summary>
    /// No abilities allowed - AI can be more reckless.
    /// </summary>
    NoAbilities = 4,

    /// <summary>
    /// Half damage - AI conserves resources for longer fight.
    /// </summary>
    HalfDamage = 5,

    /// <summary>
    /// Permadeath - Players are cautious, AI can be aggressive.
    /// </summary>
    Permadeath = 6
}

/// <summary>
/// Challenge Sector modifier configuration.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public class ChallengeSectorModifier
{
    /// <summary>
    /// Modifier type.
    /// </summary>
    public SectorModifierType Type { get; set; }

    /// <summary>
    /// Modifier display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Modifier description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Aggression adjustment (-1.0 to +1.0).
    /// </summary>
    public decimal AggressionModifier { get; set; }
}
