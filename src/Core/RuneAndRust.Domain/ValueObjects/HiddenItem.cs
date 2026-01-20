using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an item that is hidden and requires discovery.
/// </summary>
/// <param name="Id">Unique identifier for tracking discovery.</param>
/// <param name="Item">The hidden item.</param>
/// <param name="DiscoveryDC">Difficulty class for discovery.</param>
/// <param name="Hint">Optional hint for near-misses.</param>
/// <param name="IsDiscovered">Whether this item has been discovered.</param>
public record HiddenItem(
    Guid Id,
    Item Item,
    int DiscoveryDC,
    string? Hint = null,
    bool IsDiscovered = false)
{
    /// <summary>
    /// Creates a new hidden item with a generated ID.
    /// </summary>
    public static HiddenItem Create(Item item, int discoveryDC, string? hint = null) =>
        new(Guid.NewGuid(), item, discoveryDC, hint, false);

    /// <summary>
    /// Returns a copy marked as discovered.
    /// </summary>
    public HiddenItem AsDiscovered() => this with { IsDiscovered = true };
}
