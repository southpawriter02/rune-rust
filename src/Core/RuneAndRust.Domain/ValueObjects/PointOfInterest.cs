using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a point of interest detected during scouting operations.
/// </summary>
/// <remarks>
/// <para>
/// PointOfInterest captures notable features discovered during reconnaissance
/// that may reward exploration or interaction. This helps players identify
/// opportunities in adjacent rooms before committing to entry.
/// </para>
/// <para>
/// Categories of points of interest:
/// <list type="bullet">
///   <item><description>Container: Searchable storage (chests, crates, corpses)</description></item>
///   <item><description>ScavengerSign: Faction markings that can be interpreted</description></item>
///   <item><description>Mechanism: Devices that can be activated or bypassed</description></item>
///   <item><description>ResourceNode: Sources of harvestable materials</description></item>
///   <item><description>Landmark: Notable features for navigation or lore</description></item>
///   <item><description>Other: Miscellaneous interesting features</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="InterestType">Category of point of interest.</param>
/// <param name="Description">Brief description of what was observed.</param>
/// <param name="InteractionHint">Hint for how to interact with this POI.</param>
public readonly record struct PointOfInterest(
    PointOfInterestType InterestType,
    string Description,
    string InteractionHint)
{
    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this POI typically contains items.
    /// </summary>
    public bool MayContainItems => InterestType.ContainsItems();

    /// <summary>
    /// Gets whether interacting with this POI requires a skill check.
    /// </summary>
    public bool RequiresSkillCheck => InterestType.RequiresSkillCheck();

    /// <summary>
    /// Gets whether this POI may be dangerous to interact with.
    /// </summary>
    public bool MayBeDangerous => InterestType.MayBeDangerous();

    /// <summary>
    /// Gets the skill ID associated with this POI type, if any.
    /// </summary>
    public string? AssociatedSkillId => InterestType.GetAssociatedSkillId();

    /// <summary>
    /// Gets the display name for this POI's type.
    /// </summary>
    public string TypeDisplayName => InterestType.GetDisplayName();

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a PointOfInterest with default interaction hint.
    /// </summary>
    /// <param name="interestType">The type of POI.</param>
    /// <param name="description">Description of the POI.</param>
    /// <returns>A new PointOfInterest instance.</returns>
    public static PointOfInterest Create(
        PointOfInterestType interestType,
        string description)
    {
        return new PointOfInterest(
            InterestType: interestType,
            Description: description,
            InteractionHint: interestType.GetDefaultInteractionHint());
    }

    /// <summary>
    /// Creates a PointOfInterest with custom interaction hint.
    /// </summary>
    /// <param name="interestType">The type of POI.</param>
    /// <param name="description">Description of the POI.</param>
    /// <param name="interactionHint">Custom interaction hint.</param>
    /// <returns>A new PointOfInterest instance.</returns>
    public static PointOfInterest CreateCustom(
        PointOfInterestType interestType,
        string description,
        string interactionHint)
    {
        return new PointOfInterest(
            InterestType: interestType,
            Description: description,
            InteractionHint: interactionHint);
    }

    /// <summary>
    /// Creates a Container point of interest.
    /// </summary>
    /// <param name="description">Description of the container.</param>
    /// <returns>A new PointOfInterest for a container.</returns>
    public static PointOfInterest Container(string description) =>
        Create(PointOfInterestType.Container, description);

    /// <summary>
    /// Creates a ScavengerSign point of interest.
    /// </summary>
    /// <param name="description">Description of the sign.</param>
    /// <returns>A new PointOfInterest for a scavenger sign.</returns>
    public static PointOfInterest ScavengerSign(string description) =>
        Create(PointOfInterestType.ScavengerSign, description);

    /// <summary>
    /// Creates a Mechanism point of interest.
    /// </summary>
    /// <param name="description">Description of the mechanism.</param>
    /// <returns>A new PointOfInterest for a mechanism.</returns>
    public static PointOfInterest Mechanism(string description) =>
        Create(PointOfInterestType.Mechanism, description);

    /// <summary>
    /// Creates a ResourceNode point of interest.
    /// </summary>
    /// <param name="description">Description of the resource node.</param>
    /// <returns>A new PointOfInterest for a resource node.</returns>
    public static PointOfInterest ResourceNode(string description) =>
        Create(PointOfInterestType.ResourceNode, description);

    /// <summary>
    /// Creates a Landmark point of interest.
    /// </summary>
    /// <param name="description">Description of the landmark.</param>
    /// <returns>A new PointOfInterest for a landmark.</returns>
    public static PointOfInterest Landmark(string description) =>
        Create(PointOfInterestType.Landmark, description);

    /// <summary>
    /// Creates an Other point of interest.
    /// </summary>
    /// <param name="description">Description of the feature.</param>
    /// <returns>A new PointOfInterest for an unclassified feature.</returns>
    public static PointOfInterest Other(string description) =>
        Create(PointOfInterestType.Other, description);

    // ═══════════════════════════════════════════════════════════════════════════
    // STRING FORMATTING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a display string for this point of interest.
    /// </summary>
    /// <returns>A formatted string suitable for player display.</returns>
    /// <example>
    /// "Container: A rusted metal locker"
    /// </example>
    public string ToDisplayString()
    {
        return $"{TypeDisplayName}: {Description}";
    }

    /// <summary>
    /// Creates a detailed display string including interaction hint.
    /// </summary>
    /// <returns>A formatted string with full details.</returns>
    public string ToDetailedString()
    {
        return $"[{InterestType.GetIconCharacter()}] {TypeDisplayName}\n" +
               $"  {Description}\n" +
               $"  {InteractionHint}";
    }

    /// <summary>
    /// Creates a brief notification for this POI detection.
    /// </summary>
    /// <returns>A short notification string.</returns>
    public string ToNotificationString()
    {
        return $"[INTEREST] {TypeDisplayName} observed: {Description}";
    }

    /// <summary>
    /// Returns a human-readable summary of the point of interest.
    /// </summary>
    /// <returns>A formatted string describing the POI.</returns>
    public override string ToString() => ToDisplayString();
}
