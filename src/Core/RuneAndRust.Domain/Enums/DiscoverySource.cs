namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the ways a player can discover and learn new recipes.
/// </summary>
/// <remarks>
/// <para>
/// Discovery sources track how recipes were learned for statistics, achievements,
/// and event logging purposes. Different discovery methods may trigger different
/// UI feedback and achievement tracking.
/// </para>
/// <para>
/// Discovery sources are used by the recipe learning system to:
/// <list type="bullet">
///   <item><description>Track player progression statistics</description></item>
///   <item><description>Enable discovery-specific achievements</description></item>
///   <item><description>Provide appropriate UI feedback messages</description></item>
///   <item><description>Log events with context about how recipes were learned</description></item>
/// </list>
/// </para>
/// </remarks>
public enum DiscoverySource
{
    /// <summary>
    /// Recipe learned by using a recipe scroll item.
    /// </summary>
    /// <remarks>
    /// Recipe scrolls are found as loot throughout the dungeon and teach
    /// specific recipes when used. This is the primary discovery method
    /// for non-default recipes during exploration.
    /// </remarks>
    Scroll,

    /// <summary>
    /// Recipe learned from an NPC trainer.
    /// </summary>
    /// <remarks>
    /// Trainers can teach recipes directly, often in exchange for currency
    /// or after completing prerequisites. This provides a deterministic
    /// way to acquire specific recipes.
    /// </remarks>
    Trainer,

    /// <summary>
    /// Recipe learned as a quest reward.
    /// </summary>
    /// <remarks>
    /// Some quests grant recipe knowledge directly as a reward, distinct
    /// from finding a recipe scroll. This represents specialized knowledge
    /// passed down through quest givers.
    /// </remarks>
    Quest,

    /// <summary>
    /// Recipe discovered through experimentation.
    /// </summary>
    /// <remarks>
    /// Players may discover recipes by attempting to craft with certain
    /// ingredient combinations. This discovery method rewards exploration
    /// and creativity in the crafting system.
    /// </remarks>
    Experimentation,

    /// <summary>
    /// Recipe known by default at character creation.
    /// </summary>
    /// <remarks>
    /// Default recipes are automatically known by new characters and do not
    /// require discovery. This source is used for tracking purposes when
    /// initializing a player's recipe book.
    /// </remarks>
    Default
}
