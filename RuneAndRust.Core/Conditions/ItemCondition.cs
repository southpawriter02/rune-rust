using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Conditions;

/// <summary>
/// Condition that checks item possession.
/// Example: [Has: Iron Key] for key-gated dialogue.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public class ItemCondition : DialogueCondition
{
    /// <inheritdoc/>
    public override DialogueConditionType Type => DialogueConditionType.Item;

    /// <summary>
    /// The item ID or name to check for.
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Minimum quantity required (default: 1).
    /// </summary>
    public int MinQuantity { get; set; } = 1;

    /// <inheritdoc/>
    public override string GetDisplayHint() =>
        MinQuantity > 1 ? $"[Has: {ItemId} x{MinQuantity}]" : $"[Has: {ItemId}]";
}
