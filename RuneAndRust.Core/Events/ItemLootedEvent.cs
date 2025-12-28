namespace RuneAndRust.Core.Events;

/// <summary>
/// Published when an item is added to a character's inventory (v0.3.19b).
/// Consumed by AudioEventListener to trigger loot pickup sound cues.
/// </summary>
/// <param name="CharacterId">The unique identifier of the character receiving the item.</param>
/// <param name="ItemName">The display name of the looted item.</param>
/// <param name="Quantity">The number of items added.</param>
/// <param name="ItemValue">The Scrip value of the item(s).</param>
public record ItemLootedEvent(
    Guid CharacterId,
    string ItemName,
    int Quantity,
    int ItemValue)
{
    /// <summary>
    /// Returns true if this is a valuable item (worth more than 50 Scrip).
    /// </summary>
    public bool IsValuable => ItemValue > 50;
}
