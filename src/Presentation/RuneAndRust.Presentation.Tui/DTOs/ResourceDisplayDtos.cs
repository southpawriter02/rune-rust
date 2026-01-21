// ═══════════════════════════════════════════════════════════════════════════════
// ResourceDisplayDtos.cs
// Data transfer objects for resource inventory panel display.
// Version: 0.13.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Presentation.Tui.DTOs;

/// <summary>
/// Data transfer object for displaying a resource stack in the inventory panel.
/// </summary>
/// <remarks>
/// <para>Represents a single resource type with its current quantity in
/// the player's inventory, formatted for UI display.</para>
/// <para>Used by ResourceStackRenderer for formatting
/// and by ResourceInventoryPanel for rendering.</para>
/// </remarks>
/// <param name="ResourceId">
/// The unique identifier for the resource type (e.g., "iron-ore", "healing-herb").
/// </param>
/// <param name="DisplayName">
/// The display name of the resource (e.g., "Iron Ore", "Healing Herb").
/// </param>
/// <param name="Description">
/// A brief description of the resource for detail popups.
/// </param>
/// <param name="Category">
/// The resource category for grouping and styling purposes.
/// </param>
/// <param name="Quantity">
/// The current quantity of this resource owned by the player.
/// </param>
/// <example>
/// <code>
/// var dto = new ResourceStackDisplayDto(
///     ResourceId: "iron-ore",
///     DisplayName: "Iron Ore",
///     Description: "A common ore used in smithing.",
///     Category: ResourceCategory.Ore,
///     Quantity: 24);
/// </code>
/// </example>
public record ResourceStackDisplayDto(
    string ResourceId,
    string DisplayName,
    string Description,
    ResourceCategory Category,
    int Quantity);

/// <summary>
/// Data transfer object for displaying a category of resources.
/// </summary>
/// <remarks>
/// <para>Groups multiple ResourceStackDisplayDto instances
/// by category for organized display in columns.</para>
/// <para>Used by ResourceInventoryPanel when rendering
/// categorized resource columns.</para>
/// </remarks>
/// <param name="Category">
/// The category type (Ore, Herb, Leather, Gem, Misc).
/// </param>
/// <param name="DisplayName">
/// The display name for the category header (e.g., "ORE", "HERBS").
/// </param>
/// <param name="Resources">
/// The collection of resources in this category.
/// </param>
/// <param name="TotalQuantity">
/// The sum of all resource quantities in this category.
/// </param>
/// <example>
/// <code>
/// var categoryDto = new ResourceCategoryDisplayDto(
///     Category: ResourceCategory.Ore,
///     DisplayName: "ORE",
///     Resources: oreResources,
///     TotalQuantity: 39);
/// </code>
/// </example>
public record ResourceCategoryDisplayDto(
    ResourceCategory Category,
    string DisplayName,
    IReadOnlyList<ResourceStackDisplayDto> Resources,
    int TotalQuantity);
