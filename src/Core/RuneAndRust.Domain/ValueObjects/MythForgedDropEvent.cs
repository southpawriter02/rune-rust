namespace RuneAndRust.Domain.ValueObjects;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Represents a Myth-Forged item drop event for presentation purposes.
/// Contains all context needed to render an exciting drop announcement.
/// </summary>
/// <remarks>
/// <para>
/// MythForgedDropEvent captures the complete context of a legendary item drop,
/// including the item itself, the source of the drop, timing information,
/// and player context. This value object is immutable and thread-safe.
/// </para>
/// <para>
/// The event provides atmospheric text generation that varies based on the
/// drop source type, creating a more immersive experience when legendary
/// items are obtained through different means (boss kills, container looting,
/// quest completion, etc.).
/// </para>
/// <para>
/// Special handling is included for the first Myth-Forged drop of a run,
/// allowing for enhanced presentation of this milestone moment.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a drop event for a boss kill
/// var dropEvent = MythForgedDropEvent.Create(
///     item: shadowfangBlade,
///     sourceType: DropSourceType.Boss,
///     sourceId: "shadow-lord",
///     isFirstOfRun: true,
///     playerLevel: 10);
/// 
/// // Get atmospheric text based on source
/// var atmosphericText = dropEvent.GetAtmosphericText();
/// // "The air hums with ancient power. A legendary relic lies before you."
/// 
/// // Check for first drop message
/// var firstDropMessage = dropEvent.GetFirstDropMessage();
/// // "Your first legendary of this journey!"
/// </code>
/// </example>
/// <seealso cref="UniqueItem"/>
/// <seealso cref="DropSourceType"/>
public readonly record struct MythForgedDropEvent
{
    #region Properties

    /// <summary>
    /// Gets the unique item that dropped.
    /// </summary>
    /// <value>The <see cref="UniqueItem"/> instance that was dropped.</value>
    public UniqueItem Item { get; }

    /// <summary>
    /// Gets the type of source that generated the drop.
    /// </summary>
    /// <value>The <see cref="DropSourceType"/> indicating how the item was obtained.</value>
    public DropSourceType SourceType { get; }

    /// <summary>
    /// Gets the specific source identifier.
    /// </summary>
    /// <value>
    /// A lowercase string identifying the specific source (e.g., "shadow-lord" for a boss).
    /// </value>
    public string SourceId { get; }

    /// <summary>
    /// Gets the timestamp when the drop occurred.
    /// </summary>
    /// <value>A UTC <see cref="DateTime"/> representing when the item dropped.</value>
    public DateTime DroppedAt { get; }

    /// <summary>
    /// Gets whether this is the first Myth-Forged drop of the current run.
    /// </summary>
    /// <value>
    /// <c>true</c> if this is the player's first legendary item this run;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsFirstOfRun { get; }

    /// <summary>
    /// Gets the player's level when the drop occurred.
    /// </summary>
    /// <value>A positive integer representing the player's character level.</value>
    public int PlayerLevel { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Private constructor for creating MythForgedDropEvent instances.
    /// </summary>
    /// <param name="item">The unique item that dropped.</param>
    /// <param name="sourceType">The type of source that generated the drop.</param>
    /// <param name="sourceId">The specific source identifier.</param>
    /// <param name="droppedAt">When the drop occurred.</param>
    /// <param name="isFirstOfRun">Whether this is the first Myth-Forged drop of the run.</param>
    /// <param name="playerLevel">The player's level when the drop occurred.</param>
    private MythForgedDropEvent(
        UniqueItem item,
        DropSourceType sourceType,
        string sourceId,
        DateTime droppedAt,
        bool isFirstOfRun,
        int playerLevel)
    {
        Item = item;
        SourceType = sourceType;
        SourceId = sourceId;
        DroppedAt = droppedAt;
        IsFirstOfRun = isFirstOfRun;
        PlayerLevel = playerLevel;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new MythForgedDropEvent with validation.
    /// </summary>
    /// <param name="item">The unique item that dropped (required).</param>
    /// <param name="sourceType">The type of source that generated the drop.</param>
    /// <param name="sourceId">The specific source identifier (required).</param>
    /// <param name="isFirstOfRun">Whether this is the first Myth-Forged drop of the run.</param>
    /// <param name="playerLevel">The player's level when the drop occurred (minimum 1).</param>
    /// <param name="droppedAt">
    /// Optional timestamp for when the drop occurred. Defaults to <see cref="DateTime.UtcNow"/>.
    /// </param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <returns>A new validated MythForgedDropEvent instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sourceId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="playerLevel"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var dropEvent = MythForgedDropEvent.Create(
    ///     item: legendaryBlade,
    ///     sourceType: DropSourceType.Boss,
    ///     sourceId: "final-boss",
    ///     isFirstOfRun: true,
    ///     playerLevel: 15);
    /// </code>
    /// </example>
    public static MythForgedDropEvent Create(
        UniqueItem item,
        DropSourceType sourceType,
        string sourceId,
        bool isFirstOfRun = false,
        int playerLevel = 1,
        DateTime? droppedAt = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId, nameof(sourceId));
        ArgumentOutOfRangeException.ThrowIfLessThan(playerLevel, 1, nameof(playerLevel));

        var normalizedSourceId = sourceId.ToLowerInvariant();
        var timestamp = droppedAt ?? DateTime.UtcNow;

        logger?.LogDebug(
            "Creating MythForgedDropEvent for item {ItemId} from {SourceType}:{SourceId} at level {PlayerLevel}",
            item.ItemId,
            sourceType,
            normalizedSourceId,
            playerLevel);

        if (isFirstOfRun)
        {
            logger?.LogInformation(
                "First Myth-Forged drop of run: {ItemId} ({ItemName})",
                item.ItemId,
                item.Name);
        }

        return new MythForgedDropEvent(
            item,
            sourceType,
            normalizedSourceId,
            timestamp,
            isFirstOfRun,
            playerLevel);
    }

    #endregion

    #region Instance Methods

    /// <summary>
    /// Gets atmospheric text based on the drop source type.
    /// </summary>
    /// <returns>
    /// A descriptive string appropriate for the source type that created the drop.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The atmospheric text varies based on how the item was obtained,
    /// providing an immersive presentation experience. Each source type
    /// has unique flavor text:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Boss: Ancient power themes</description></item>
    ///   <item><description>Container: Discovery and light themes</description></item>
    ///   <item><description>Quest: Reward and accomplishment themes</description></item>
    ///   <item><description>Monster: Treasure from fallen foes</description></item>
    ///   <item><description>Vendor: Merchant presentation themes</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var bossEvent = MythForgedDropEvent.Create(item, DropSourceType.Boss, "boss-id");
    /// var text = bossEvent.GetAtmosphericText();
    /// // "The air hums with ancient power. A legendary relic lies before you."
    /// </code>
    /// </example>
    public string GetAtmosphericText() =>
        SourceType switch
        {
            DropSourceType.Boss => "The air hums with ancient power. A legendary relic lies before you.",
            DropSourceType.Container => "As the lid opens, golden light spills forth. Something legendary awaits.",
            DropSourceType.Quest => "Your deeds have been rewarded. A legend is now yours to wield.",
            DropSourceType.Monster => "From the fallen foe, a treasure beyond measure emerges.",
            DropSourceType.Vendor => "The merchant's eyes widen as they present their finest work.",
            _ => "A legendary item has been discovered."
        };

    /// <summary>
    /// Gets a special message for the first drop of the run.
    /// </summary>
    /// <returns>
    /// A celebratory message if this is the first Myth-Forged drop of the run;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This message is intended as an additional line in the drop announcement
    /// to celebrate the milestone of obtaining the first legendary item.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (dropEvent.IsFirstOfRun)
    /// {
    ///     var message = dropEvent.GetFirstDropMessage();
    ///     // "Your first legendary of this journey!"
    /// }
    /// </code>
    /// </example>
    public string? GetFirstDropMessage() =>
        IsFirstOfRun ? "Your first legendary of this journey!" : null;

    /// <summary>
    /// Gets the time elapsed since the drop occurred.
    /// </summary>
    /// <returns>A <see cref="TimeSpan"/> representing the time since the drop.</returns>
    /// <remarks>
    /// Useful for determining if the drop is recent enough to show animated effects
    /// or for logging and analytics purposes.
    /// </remarks>
    public TimeSpan TimeSinceDrop => DateTime.UtcNow - DroppedAt;

    /// <summary>
    /// Returns a string representation of the drop event.
    /// </summary>
    /// <returns>A string containing the item ID and source information.</returns>
    public override string ToString() =>
        $"MythForgedDropEvent[{Item.ItemId} from {SourceType}:{SourceId}]";

    #endregion
}
