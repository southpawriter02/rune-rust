namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories for organizing achievements by gameplay area.
/// </summary>
/// <remarks>
/// <para>
/// Achievement categories help players navigate the achievement list:
/// <list type="bullet">
///   <item><description>Combat — Killing monsters, dealing damage, combat feats</description></item>
///   <item><description>Exploration — Discovering rooms, finding secrets</description></item>
///   <item><description>Progression — Leveling up, gaining XP, completing quests</description></item>
///   <item><description>Collection — Finding items, gathering resources</description></item>
///   <item><description>Challenge — Difficult feats, speed runs, no-death runs</description></item>
///   <item><description>Secret — Hidden achievements revealed only when unlocked</description></item>
/// </list>
/// </para>
/// </remarks>
public enum AchievementCategory
{
    /// <summary>Achievements related to combat: kills, damage, abilities.</summary>
    Combat,

    /// <summary>Achievements related to exploration: rooms, secrets, discoveries.</summary>
    Exploration,

    /// <summary>Achievements related to progression: levels, XP, quests.</summary>
    Progression,

    /// <summary>Achievements related to collecting: items, gold, resources.</summary>
    Collection,

    /// <summary>Achievements for difficult challenges: rare feats, special conditions.</summary>
    Challenge,

    /// <summary>Hidden achievements revealed only upon unlock.</summary>
    Secret
}
