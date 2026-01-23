namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of skill check modifiers.
/// </summary>
/// <remarks>
/// <para>
/// Used to classify and organize modifiers within a <see cref="ValueObjects.SkillContext"/>.
/// Each category has different sources and persistence characteristics:
/// <list type="bullet">
///   <item><description>Equipment: From inventory items, persists until unequipped</description></item>
///   <item><description>Situational: From temporary conditions, varies in duration</description></item>
///   <item><description>Environment: From physical surroundings, changes with location</description></item>
///   <item><description>Target: From target characteristics, specific to interaction</description></item>
/// </list>
/// </para>
/// </remarks>
public enum ModifierCategory
{
    /// <summary>
    /// Modifiers from equipped tools, gear, and weapons.
    /// </summary>
    /// <remarks>
    /// Examples: Tinker's Toolkit, Climbing Gear, Lockpicks
    /// </remarks>
    Equipment = 0,

    /// <summary>
    /// Modifiers from temporary or situational conditions.
    /// </summary>
    /// <remarks>
    /// Examples: Time Pressure, Familiarity, Assistance, Fatigue
    /// </remarks>
    Situational = 1,

    /// <summary>
    /// Modifiers from environmental or physical conditions.
    /// </summary>
    /// <remarks>
    /// Examples: Surface Type, Lighting Level, Corruption Tier
    /// </remarks>
    Environment = 2,

    /// <summary>
    /// Modifiers based on the target of the skill check.
    /// </summary>
    /// <remarks>
    /// Examples: Disposition, Suspicion Level, Relative Strength
    /// </remarks>
    Target = 3,

    /// <summary>
    /// Modifiers specific to social interactions.
    /// </summary>
    /// <remarks>
    /// Examples: Faction Standing, Argument Alignment, Evidence, Reputation
    /// </remarks>
    Social = 4
}
