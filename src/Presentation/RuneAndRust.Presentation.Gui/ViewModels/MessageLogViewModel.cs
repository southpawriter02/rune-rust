namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// View model for the message log panel.
/// </summary>
/// <remarks>
/// Manages a scrollable list of game messages with:
/// - Auto-scroll toggle
/// - Category filtering (Combat, Loot, System)
/// - Maximum message limit to prevent memory bloat
/// </remarks>
public partial class MessageLogViewModel : ViewModelBase
{
    /// <summary>
    /// Maximum number of messages to retain.
    /// </summary>
    public const int MaxMessages = 500;

    /// <summary>
    /// Gets or sets the messages collection.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GameMessage> _messages = [];

    /// <summary>
    /// Gets or sets whether auto-scroll is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _autoScrollEnabled = true;

    /// <summary>
    /// Gets or sets whether combat messages are shown.
    /// </summary>
    [ObservableProperty]
    private bool _showCombatMessages = true;

    /// <summary>
    /// Gets or sets whether loot messages are shown.
    /// </summary>
    [ObservableProperty]
    private bool _showLootMessages = true;

    /// <summary>
    /// Gets or sets whether system messages are shown.
    /// </summary>
    [ObservableProperty]
    private bool _showSystemMessages = true;

    /// <summary>
    /// Initializes a new instance with design-time sample data.
    /// </summary>
    public MessageLogViewModel()
    {
        AddSampleMessages();
    }

    /// <summary>
    /// Clears all messages from the log.
    /// </summary>
    [RelayCommand]
    private void ClearLog()
    {
        Messages.Clear();
        Log.Debug("Message log cleared");
    }

    /// <summary>
    /// Toggles auto-scroll behavior.
    /// </summary>
    [RelayCommand]
    private void ToggleAutoScroll()
    {
        AutoScrollEnabled = !AutoScrollEnabled;
        Log.Debug("Auto-scroll set to {Enabled}", AutoScrollEnabled);
    }

    /// <summary>
    /// Adds a new message to the log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    public void AddMessage(GameMessage message)
    {
        // Check category filter
        if (!ShouldShowMessage(message))
            return;

        Messages.Add(message);

        // Trim old messages if over limit
        while (Messages.Count > MaxMessages)
        {
            Messages.RemoveAt(0);
        }
    }

    /// <summary>
    /// Adds a message with text and type.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <param name="type">The message type.</param>
    public void AddMessage(string text, MessageType type)
    {
        var category = DetermineCategory(type);
        var message = new GameMessage(DateTime.Now, text, type, category);
        AddMessage(message);
    }

    private bool ShouldShowMessage(GameMessage message)
    {
        return message.Category switch
        {
            MessageCategory.Combat => ShowCombatMessages,
            MessageCategory.Loot => ShowLootMessages,
            MessageCategory.System => ShowSystemMessages,
            _ => true
        };
    }

    private static MessageCategory DetermineCategory(MessageType type)
    {
        return type switch
        {
            MessageType.CombatHit or MessageType.CombatMiss or
            MessageType.CombatHeal or MessageType.CombatCritical => MessageCategory.Combat,

            MessageType.LootCommon or MessageType.LootUncommon or
            MessageType.LootRare or MessageType.LootEpic or
            MessageType.LootLegendary => MessageCategory.Loot,

            MessageType.Warning or MessageType.Error => MessageCategory.System,

            MessageType.Dialogue => MessageCategory.Dialogue,

            _ => MessageCategory.General
        };
    }

    private void AddSampleMessages()
    {
        var baseTime = DateTime.Now.AddMinutes(-5);

        Messages.Add(new GameMessage(baseTime.AddSeconds(0), "You wake up in a small village.", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(3), "You see a sword shop to the north.", MessageType.Info));
        Messages.Add(new GameMessage(baseTime.AddSeconds(47), "You enter the sword shop.", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(50), "The shopkeeper greets you warmly.", MessageType.Dialogue));
        Messages.Add(new GameMessage(baseTime.AddSeconds(120), "You purchased: Iron Sword (50 gold)", MessageType.LootCommon, MessageCategory.Loot));
        Messages.Add(new GameMessage(baseTime.AddSeconds(165), "You leave the shop and head east.", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(247), "You enter the dark cave.", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(250), "A skeleton blocks your path!", MessageType.Warning));
        Messages.Add(new GameMessage(baseTime.AddSeconds(255), "══════ COMBAT BEGINS ══════", MessageType.Info));
        Messages.Add(new GameMessage(baseTime.AddSeconds(260), "You attack the Skeleton with Iron Sword!", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(260), "Hit! 12 damage to Skeleton.", MessageType.CombatHit, MessageCategory.Combat));
        Messages.Add(new GameMessage(baseTime.AddSeconds(265), "Skeleton attacks you!", MessageType.Default));
        Messages.Add(new GameMessage(baseTime.AddSeconds(265), "Miss! The skeleton's attack misses.", MessageType.CombatMiss, MessageCategory.Combat));
        Messages.Add(new GameMessage(baseTime.AddSeconds(270), "Critical Hit! 24 damage to Skeleton.", MessageType.CombatCritical, MessageCategory.Combat));
        Messages.Add(new GameMessage(baseTime.AddSeconds(275), "Skeleton defeated! +50 XP", MessageType.Success));
        Messages.Add(new GameMessage(baseTime.AddSeconds(280), "══════ COMBAT ENDS ══════", MessageType.Info));
        Messages.Add(new GameMessage(baseTime.AddSeconds(285), "You found: Rusty Key", MessageType.LootUncommon, MessageCategory.Loot));
        Messages.Add(new GameMessage(baseTime.AddSeconds(290), "The key might unlock something nearby.", MessageType.Info));

        Log.Debug("MessageLogViewModel initialized with {Count} sample messages", Messages.Count);
    }
}
