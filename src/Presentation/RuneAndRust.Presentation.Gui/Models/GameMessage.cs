namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Represents a message in the game log.
/// </summary>
/// <remarks>
/// Contains the message text, timestamp, type (for color coding),
/// and category (for filtering).
/// </remarks>
public record GameMessage(
    DateTime Timestamp,
    string Text,
    MessageType Type,
    MessageCategory Category = MessageCategory.General)
{
    /// <summary>
    /// Gets the formatted timestamp string (HH:mm:ss).
    /// </summary>
    public string FormattedTime => Timestamp.ToString("HH:mm:ss");

    /// <summary>
    /// Creates an info message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new GameMessage with Info type.</returns>
    public static GameMessage Info(string text) =>
        new(DateTime.Now, text, MessageType.Info);

    /// <summary>
    /// Creates a combat message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="type">The combat message type.</param>
    /// <returns>A new GameMessage with Combat category.</returns>
    public static GameMessage Combat(string text, MessageType type = MessageType.CombatHit) =>
        new(DateTime.Now, text, type, MessageCategory.Combat);

    /// <summary>
    /// Creates a loot message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="type">The loot rarity type.</param>
    /// <returns>A new GameMessage with Loot category.</returns>
    public static GameMessage Loot(string text, MessageType type = MessageType.LootCommon) =>
        new(DateTime.Now, text, type, MessageCategory.Loot);

    /// <summary>
    /// Creates a system message.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>A new GameMessage with System category.</returns>
    public static GameMessage System(string text) =>
        new(DateTime.Now, text, MessageType.Info, MessageCategory.System);

    /// <summary>
    /// Creates a dialogue message.
    /// </summary>
    /// <param name="text">The dialogue text.</param>
    /// <returns>A new GameMessage with Dialogue type and category.</returns>
    public static GameMessage Dialogue(string text) =>
        new(DateTime.Now, text, MessageType.Dialogue, MessageCategory.Dialogue);

    /// <summary>
    /// Creates a success message.
    /// </summary>
    /// <param name="text">The success text.</param>
    /// <returns>A new GameMessage with Success type.</returns>
    public static GameMessage Success(string text) =>
        new(DateTime.Now, text, MessageType.Success);

    /// <summary>
    /// Creates a failure message.
    /// </summary>
    /// <param name="text">The failure text.</param>
    /// <returns>A new GameMessage with Failure type.</returns>
    public static GameMessage Failure(string text) =>
        new(DateTime.Now, text, MessageType.Failure);
}
