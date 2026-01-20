using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Events;

/// <summary>
/// Base event for all crafting-related activities.
/// </summary>
/// <remarks>
/// <para>
/// Crafting events track the complete lifecycle of crafting attempts,
/// including validation, dice checks, success/failure outcomes, and item creation.
/// </para>
/// <para>
/// All crafting events include the recipe being crafted and use the
/// <see cref="EventCategory.Interaction"/> category since crafting is
/// fundamentally an interaction with crafting stations.
/// </para>
/// </remarks>
public record CraftingEvent : GameEvent
{
    /// <summary>
    /// Gets the recipe ID being crafted.
    /// </summary>
    public string? RecipeId { get; init; }

    /// <summary>
    /// Gets the crafting station ID where crafting occurred.
    /// </summary>
    public string? StationId { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CraftingEvent"/>.
    /// </summary>
    public CraftingEvent() => Category = EventCategory.Interaction;
}

/// <summary>
/// Event raised when a player attempts to craft an item.
/// </summary>
/// <remarks>
/// <para>
/// This event is published immediately after the dice check is performed,
/// regardless of success or failure. It captures the complete roll information
/// for logging and analytics purposes.
/// </para>
/// <para>
/// Use this event to:
/// <list type="bullet">
///   <item><description>Track crafting attempt statistics</description></item>
///   <item><description>Log dice roll outcomes for debugging</description></item>
///   <item><description>Trigger UI updates showing the roll result</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Publishing a craft attempt event
/// var attemptEvent = CraftAttemptedEvent.Create(
///     playerId: player.Id,
///     recipeId: "iron-sword",
///     roll: 15,
///     modifier: 4,
///     difficultyClass: 12);
///
/// gameEventLogger.Log(attemptEvent);
/// // Event contains: Total = 19, WasSuccessful = true, Margin = 7
/// </code>
/// </example>
public sealed record CraftAttemptedEvent : CraftingEvent
{
    /// <summary>
    /// Gets the raw d20 roll value (1-20).
    /// </summary>
    public int Roll { get; init; }

    /// <summary>
    /// Gets the skill modifier applied to the roll.
    /// </summary>
    /// <remarks>
    /// The modifier is derived from the player's skill level for the
    /// crafting station's associated skill (e.g., smithing for anvil).
    /// </remarks>
    public int Modifier { get; init; }

    /// <summary>
    /// Gets the total roll result (Roll + Modifier).
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// Gets the difficulty class (DC) of the recipe.
    /// </summary>
    public int DifficultyClass { get; init; }

    /// <summary>
    /// Gets whether the crafting attempt succeeded.
    /// </summary>
    /// <remarks>
    /// True if <see cref="Total"/> >= <see cref="DifficultyClass"/>.
    /// </remarks>
    public bool WasSuccessful => Total >= DifficultyClass;

    /// <summary>
    /// Gets the margin (how much the roll beat or missed the DC).
    /// </summary>
    /// <remarks>
    /// Positive values indicate success margin; negative values indicate failure margin.
    /// For failures, the margin determines resource loss percentage:
    /// <list type="bullet">
    ///   <item><description>Margin >= -5: Close failure (25% loss)</description></item>
    ///   <item><description>Margin &lt; -5: Bad failure (50% loss)</description></item>
    /// </list>
    /// </remarks>
    public int Margin => Total - DifficultyClass;

    /// <summary>
    /// Gets whether this was a natural 20 roll.
    /// </summary>
    /// <remarks>
    /// Natural 20 always results in Legendary quality items.
    /// </remarks>
    public bool IsNatural20 => Roll == 20;

    /// <summary>
    /// Creates a new craft attempted event.
    /// </summary>
    /// <param name="playerId">The player who attempted crafting.</param>
    /// <param name="recipeId">The recipe being crafted.</param>
    /// <param name="roll">The d20 roll value.</param>
    /// <param name="modifier">The skill modifier applied.</param>
    /// <param name="difficultyClass">The recipe's difficulty class.</param>
    /// <param name="stationId">Optional crafting station ID.</param>
    /// <returns>A new craft attempted event.</returns>
    public static CraftAttemptedEvent Create(
        Guid playerId,
        string recipeId,
        int roll,
        int modifier,
        int difficultyClass,
        string? stationId = null)
    {
        var total = roll + modifier;
        var wasSuccessful = total >= difficultyClass;
        var modSign = modifier >= 0 ? "+" : "";

        return new CraftAttemptedEvent
        {
            EventType = "CraftAttempted",
            Message = $"Crafting {recipeId}: d20({roll}) {modSign}{modifier} = {total} vs DC {difficultyClass} [{(wasSuccessful ? "SUCCESS" : "FAILED")}]",
            PlayerId = playerId,
            RecipeId = recipeId,
            StationId = stationId,
            Roll = roll,
            Modifier = modifier,
            Total = total,
            DifficultyClass = difficultyClass
        };
    }
}

/// <summary>
/// Event raised when a player successfully crafts an item.
/// </summary>
/// <remarks>
/// <para>
/// This event is published only on successful crafting, after the item
/// has been created and added to the player's inventory. It captures
/// the created item's details and quality tier.
/// </para>
/// <para>
/// Use this event to:
/// <list type="bullet">
///   <item><description>Track crafting success statistics</description></item>
///   <item><description>Log item creation for auditing</description></item>
///   <item><description>Trigger achievements or quests related to crafting</description></item>
///   <item><description>Display special UI for high-quality items</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Publishing an item crafted event
/// var craftedEvent = ItemCraftedEvent.Create(
///     playerId: player.Id,
///     recipeId: "iron-sword",
///     itemId: newItem.Id,
///     itemName: "Fine Iron Sword",
///     quality: CraftedItemQuality.Fine);
///
/// gameEventLogger.Log(craftedEvent);
/// </code>
/// </example>
public sealed record ItemCraftedEvent : CraftingEvent
{
    /// <summary>
    /// Gets the unique identifier of the created item.
    /// </summary>
    public Guid ItemId { get; init; }

    /// <summary>
    /// Gets the display name of the created item.
    /// </summary>
    /// <remarks>
    /// May include quality prefix (e.g., "Fine Iron Sword", "Legendary Iron Sword").
    /// </remarks>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the quality tier of the crafted item.
    /// </summary>
    public CraftedItemQuality Quality { get; init; }

    /// <summary>
    /// Gets whether the crafted item is of exceptional quality.
    /// </summary>
    /// <remarks>
    /// True for <see cref="CraftedItemQuality.Masterwork"/> or
    /// <see cref="CraftedItemQuality.Legendary"/> items.
    /// </remarks>
    public bool IsExceptionalQuality =>
        Quality is CraftedItemQuality.Masterwork or CraftedItemQuality.Legendary;

    /// <summary>
    /// Creates a new item crafted event.
    /// </summary>
    /// <param name="playerId">The player who crafted the item.</param>
    /// <param name="recipeId">The recipe used.</param>
    /// <param name="itemId">The unique ID of the created item.</param>
    /// <param name="itemName">The display name of the created item.</param>
    /// <param name="quality">The quality tier of the item.</param>
    /// <param name="stationId">Optional crafting station ID.</param>
    /// <returns>A new item crafted event.</returns>
    public static ItemCraftedEvent Create(
        Guid playerId,
        string recipeId,
        Guid itemId,
        string itemName,
        CraftedItemQuality quality,
        string? stationId = null)
    {
        var qualityText = quality switch
        {
            CraftedItemQuality.Legendary => " [LEGENDARY]",
            CraftedItemQuality.Masterwork => " [MASTERWORK]",
            CraftedItemQuality.Fine => " [FINE]",
            _ => ""
        };

        return new ItemCraftedEvent
        {
            EventType = "ItemCrafted",
            Message = $"Crafted {itemName}{qualityText} from {recipeId}",
            PlayerId = playerId,
            RecipeId = recipeId,
            StationId = stationId,
            ItemId = itemId,
            ItemName = itemName,
            Quality = quality
        };
    }
}
