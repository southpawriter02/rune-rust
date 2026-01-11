namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Types of content that can fill template slots.
/// </summary>
/// <remarks>
/// Each slot type has associated selection logic and constraints:
/// - Monster: Uses monster pools with tier/category filtering
/// - Item: Uses item pools with type/rarity filtering
/// - Feature: Places interactive room features
/// - Exit: Creates room connections
/// - Description: Resolves to descriptor pool text
/// - Hazard: Places environmental dangers
/// - Container: Places lootable containers
/// </remarks>
public enum SlotType
{
    /// <summary>
    /// Monster spawn point.
    /// </summary>
    /// <remarks>
    /// Constraints: maxTier, minTier, categories (comma-separated)
    /// </remarks>
    Monster = 0,

    /// <summary>
    /// Item placement point.
    /// </summary>
    /// <remarks>
    /// Constraints: types (comma-separated), rarity, maxValue
    /// </remarks>
    Item = 1,

    /// <summary>
    /// Interactive feature (lever, altar, statue, etc.).
    /// </summary>
    /// <remarks>
    /// Constraints: featureType, interactive (true/false)
    /// </remarks>
    Feature = 2,

    /// <summary>
    /// Potential exit direction.
    /// </summary>
    /// <remarks>
    /// Constraints: direction, hidden (true/false), discoveryDC
    /// </remarks>
    Exit = 3,

    /// <summary>
    /// Variable description segment.
    /// </summary>
    /// <remarks>
    /// Uses DescriptorPool property to reference a descriptor pool path.
    /// </remarks>
    Description = 4,

    /// <summary>
    /// Environmental hazard or trap.
    /// </summary>
    /// <remarks>
    /// Constraints: hazardType, damage, triggerType
    /// </remarks>
    Hazard = 5,

    /// <summary>
    /// Loot container (chest, barrel, crate, etc.).
    /// </summary>
    /// <remarks>
    /// Constraints: containerType, lootQuality, locked (true/false)
    /// </remarks>
    Container = 6
}
