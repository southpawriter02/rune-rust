namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categories of points of interest detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// PointOfInterestType categorizes notable features discovered during reconnaissance.
/// Each type suggests different interaction possibilities and potential rewards.
/// </para>
/// <para>
/// Point of interest categories:
/// <list type="bullet">
///   <item><description>Container: Searchable storage (chests, crates, lockers)</description></item>
///   <item><description>ScavengerSign: Faction markings interpretable with Wasteland Survival</description></item>
///   <item><description>Mechanism: Activatable or bypassable devices (terminals, switches)</description></item>
///   <item><description>ResourceNode: Harvestable material sources (salvage piles, mineral deposits)</description></item>
///   <item><description>Landmark: Notable features useful for navigation or story</description></item>
///   <item><description>Other: Miscellaneous points of interest</description></item>
/// </list>
/// </para>
/// </remarks>
public enum PointOfInterestType
{
    /// <summary>
    /// Container that may hold items.
    /// </summary>
    /// <remarks>
    /// Includes chests, crates, lockers, corpses, and other searchable storage.
    /// Can be searched for items, though some may be locked or trapped.
    /// </remarks>
    Container = 0,

    /// <summary>
    /// Scavenger sign that can be interpreted.
    /// </summary>
    /// <remarks>
    /// Faction markings left by wasteland groups. Can be interpreted using
    /// the Wasteland Survival skill to reveal warnings, directions, or
    /// information about nearby resources and dangers.
    /// </remarks>
    ScavengerSign = 1,

    /// <summary>
    /// Mechanism that can be activated or bypassed.
    /// </summary>
    /// <remarks>
    /// Includes terminals, switches, levers, control panels, and automated systems.
    /// May require specific skills (Technology, Athletics) or items to interact with.
    /// </remarks>
    Mechanism = 2,

    /// <summary>
    /// Resource node that can be harvested.
    /// </summary>
    /// <remarks>
    /// Sources of raw materials such as salvage piles, mineral deposits,
    /// plant growth, or water sources. Can be harvested for crafting materials.
    /// </remarks>
    ResourceNode = 3,

    /// <summary>
    /// Landmark useful for navigation.
    /// </summary>
    /// <remarks>
    /// Notable features that serve as waypoints or contain lore.
    /// Includes monuments, distinctive ruins, natural formations, or
    /// significant pre-Fall structures.
    /// </remarks>
    Landmark = 4,

    /// <summary>
    /// Other notable feature.
    /// </summary>
    /// <remarks>
    /// Miscellaneous points of interest that don't fit other categories.
    /// May include unusual phenomena, camp sites, corpses with story significance,
    /// or environmental anomalies.
    /// </remarks>
    Other = 5
}

/// <summary>
/// Extension methods for <see cref="PointOfInterestType"/>.
/// </summary>
public static class PointOfInterestTypeExtensions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the human-readable display name for this point of interest type.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>A display name suitable for UI presentation.</returns>
    public static string GetDisplayName(this PointOfInterestType poiType)
    {
        return poiType switch
        {
            PointOfInterestType.Container => "Container",
            PointOfInterestType.ScavengerSign => "Scavenger Sign",
            PointOfInterestType.Mechanism => "Mechanism",
            PointOfInterestType.ResourceNode => "Resource Node",
            PointOfInterestType.Landmark => "Landmark",
            PointOfInterestType.Other => "Point of Interest",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a detailed description of the point of interest type.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>A descriptive string explaining the POI type.</returns>
    public static string GetDescription(this PointOfInterestType poiType)
    {
        return poiType switch
        {
            PointOfInterestType.Container =>
                "Storage that may contain items. Can be searched for loot.",
            PointOfInterestType.ScavengerSign =>
                "Faction markings that can be interpreted with Wasteland Survival.",
            PointOfInterestType.Mechanism =>
                "Device that can be activated, deactivated, or bypassed.",
            PointOfInterestType.ResourceNode =>
                "Source of raw materials that can be harvested.",
            PointOfInterestType.Landmark =>
                "Notable feature useful for navigation or containing lore.",
            PointOfInterestType.Other =>
                "Unusual feature that may warrant closer inspection.",
            _ => "Unknown point of interest."
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INTERACTION METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the default interaction hint for this point of interest type.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>A string describing how to interact with this POI type.</returns>
    public static string GetDefaultInteractionHint(this PointOfInterestType poiType)
    {
        return poiType switch
        {
            PointOfInterestType.Container => "Can be searched for items.",
            PointOfInterestType.ScavengerSign => "Can be interpreted with Wasteland Survival.",
            PointOfInterestType.Mechanism => "May be activated or bypassed.",
            PointOfInterestType.ResourceNode => "Can be harvested for materials.",
            PointOfInterestType.Landmark => "Notable for navigation.",
            PointOfInterestType.Other => "May warrant closer inspection.",
            _ => "Interaction unknown."
        };
    }

    /// <summary>
    /// Gets the primary skill associated with interacting with this POI type.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>The skill ID typically used for interaction, or null if no specific skill.</returns>
    public static string? GetAssociatedSkillId(this PointOfInterestType poiType)
    {
        return poiType switch
        {
            PointOfInterestType.Container => "perception",         // Finding hidden items
            PointOfInterestType.ScavengerSign => "wasteland-survival",
            PointOfInterestType.Mechanism => "technology",
            PointOfInterestType.ResourceNode => "wasteland-survival",
            PointOfInterestType.Landmark => null,                  // No skill required
            PointOfInterestType.Other => null,
            _ => null
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHARACTERISTIC METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this POI type typically contains items.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>True if items can typically be found at this POI type.</returns>
    public static bool ContainsItems(this PointOfInterestType poiType)
    {
        return poiType is PointOfInterestType.Container or PointOfInterestType.ResourceNode;
    }

    /// <summary>
    /// Determines whether this POI type requires a skill check to interact with.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>True if interaction typically requires a skill check.</returns>
    public static bool RequiresSkillCheck(this PointOfInterestType poiType)
    {
        return poiType is PointOfInterestType.ScavengerSign or
                         PointOfInterestType.Mechanism or
                         PointOfInterestType.ResourceNode;
    }

    /// <summary>
    /// Determines whether this POI type may have narrative significance.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>True if the POI may contain lore or story elements.</returns>
    public static bool MayHaveNarrativeSignificance(this PointOfInterestType poiType)
    {
        return poiType is PointOfInterestType.Landmark or
                         PointOfInterestType.ScavengerSign or
                         PointOfInterestType.Other;
    }

    /// <summary>
    /// Determines whether this POI type may be dangerous to interact with.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>True if the POI may have traps or other dangers.</returns>
    public static bool MayBeDangerous(this PointOfInterestType poiType)
    {
        return poiType is PointOfInterestType.Container or
                         PointOfInterestType.Mechanism;
    }

    /// <summary>
    /// Gets the icon character for displaying this POI type.
    /// </summary>
    /// <param name="poiType">The point of interest type.</param>
    /// <returns>A character suitable for ASCII/console display.</returns>
    public static char GetIconCharacter(this PointOfInterestType poiType)
    {
        return poiType switch
        {
            PointOfInterestType.Container => 'C',
            PointOfInterestType.ScavengerSign => 'S',
            PointOfInterestType.Mechanism => 'M',
            PointOfInterestType.ResourceNode => 'R',
            PointOfInterestType.Landmark => 'L',
            PointOfInterestType.Other => '?',
            _ => '?'
        };
    }
}
