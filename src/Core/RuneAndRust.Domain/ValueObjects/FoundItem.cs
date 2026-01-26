namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an item discovered during an active search operation,
/// including location details and discovery information.
/// </summary>
/// <remarks>
/// <para>
/// FoundItem captures not just what was found but where it was found and
/// how difficult it was to discover. This enables contextual descriptions
/// and appropriate reward for player perception investment.
/// </para>
/// </remarks>
public sealed record FoundItem
{
    /// <summary>
    /// The unique identifier of the discovered item.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// The display name of the item for rendering purposes.
    /// </summary>
    public required string ItemName { get; init; }

    /// <summary>
    /// Description of where the item was found within the search area.
    /// </summary>
    /// <remarks>
    /// Examples: "hidden under the console", "inside a false-bottom drawer",
    /// "wedged behind the shelving unit"
    /// </remarks>
    public required string Location { get; init; }

    /// <summary>
    /// The difficulty class required to discover this item.
    /// </summary>
    /// <remarks>
    /// Items with higher DCs are more difficult to find and typically
    /// represent more valuable or deliberately hidden objects.
    /// </remarks>
    public required int DiscoveryDc { get; init; }

    /// <summary>
    /// Flavor text describing the discovery moment.
    /// </summary>
    /// <remarks>
    /// This text is displayed when the item is found, providing atmospheric
    /// description of the discovery. Should be written in second person.
    /// Example: "You notice a glint of metal beneath the debris..."
    /// </remarks>
    public required string DiscoveryText { get; init; }

    /// <summary>
    /// Optional container identifier if the item was found in a container.
    /// </summary>
    public string? ContainerId { get; init; }

    /// <summary>
    /// Indicates whether this item was hidden (required active search).
    /// </summary>
    public bool WasHidden { get; init; }

    /// <summary>
    /// Gets whether this item was found in a container.
    /// </summary>
    public bool WasInContainer => !string.IsNullOrEmpty(ContainerId);

    /// <summary>
    /// Creates a simple found item for testing.
    /// </summary>
    public static FoundItem Create(
        string itemId,
        string itemName,
        string location,
        int discoveryDc,
        string discoveryText,
        string? containerId = null,
        bool wasHidden = false) =>
        new()
        {
            ItemId = itemId,
            ItemName = itemName,
            Location = location,
            DiscoveryDc = discoveryDc,
            DiscoveryText = discoveryText,
            ContainerId = containerId,
            WasHidden = wasHidden
        };
}
