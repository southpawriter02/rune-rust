using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Effects;

/// <summary>
/// Effect that gives an item to the character.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class GiveItemEffect : DialogueEffect
{
    /// <inheritdoc/>
    public override DialogueEffectType Type => DialogueEffectType.GiveItem;

    /// <summary>
    /// The item ID to give.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Display name for logging.
    /// </summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity to give (default: 1).
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <inheritdoc/>
    public override string GetDescription() =>
        Quantity > 1 ? $"Receive: {ItemName} x{Quantity}" : $"Receive: {ItemName}";
}
