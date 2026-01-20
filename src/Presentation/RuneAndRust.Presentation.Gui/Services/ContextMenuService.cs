namespace RuneAndRust.Presentation.Gui.Services;

using global::Avalonia;
using global::Avalonia.Controls;
using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.ViewModels;

/// <summary>
/// Creates and manages context menus for game elements.
/// </summary>
public class ContextMenuService : IContextMenuService
{
    private readonly ILogger<ContextMenuService> _logger;

    /// <summary>
    /// Event raised when a command should be executed.
    /// </summary>
    public event Action<string>? CommandExecuted;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextMenuService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ContextMenuService(ILogger<ContextMenuService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ContextMenu CreateItemMenu(Item item, bool isEquipped = false)
    {
        ArgumentNullException.ThrowIfNull(item);

        var menu = new ContextMenu();

        // Header with icon and equipped indicator
        var icon = ItemSlotViewModel.GetCategoryIcon(item.Type);
        var equippedIndicator = isEquipped ? " [E]" : "";
        menu.Items.Add(CreateHeader($"{icon} {item.Name}{equippedIndicator}"));
        menu.Items.Add(new Separator());

        // Examine (always available)
        menu.Items.Add(CreateMenuItem(MenuActions.Examine(), () => ExecuteCommand($"examine {item.Name}")));

        // Type-specific actions
        if (isEquipped)
        {
            menu.Items.Add(CreateMenuItem(MenuActions.Unequip(), () => ExecuteCommand($"unequip {item.Name}")));
        }
        else
        {
            // Consumable items can be used
            if (item.Type == ItemType.Consumable)
            {
                menu.Items.Add(CreateMenuItem(MenuActions.Use(), () => ExecuteCommand($"use {item.Name}")));
            }

            // Equippable items can be equipped
            if (item.IsEquippable)
            {
                menu.Items.Add(CreateMenuItem(MenuActions.Equip(), () => ExecuteCommand($"equip {item.Name}")));
            }

            // All non-equipped items can be dropped
            menu.Items.Add(CreateMenuItem(MenuActions.Drop(), () => ExecuteCommand($"drop {item.Name}")));
        }

        // Cancel
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem(MenuActions.Cancel(), () => menu.Close()));

        _logger.LogDebug("Created item menu for {ItemName} (equipped: {IsEquipped})", item.Name, isEquipped);
        return menu;
    }

    /// <inheritdoc />
    public ContextMenu CreateMonsterMenu(Monster monster)
    {
        ArgumentNullException.ThrowIfNull(monster);

        var menu = new ContextMenu();

        // Header
        menu.Items.Add(CreateHeader($"☠ {monster.Name}"));
        menu.Items.Add(new Separator());

        // Actions
        menu.Items.Add(CreateMenuItem(MenuActions.Examine(), () => ExecuteCommand($"examine {monster.Name}")));
        menu.Items.Add(CreateMenuItem(MenuActions.Attack(), () => ExecuteCommand($"attack {monster.Name}")));

        // Cancel
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem(MenuActions.Cancel(), () => menu.Close()));

        _logger.LogDebug("Created monster menu for {MonsterName}", monster.Name);
        return menu;
    }

    /// <inheritdoc />
    public ContextMenu CreateNpcMenu(RiddleNpc npc)
    {
        ArgumentNullException.ThrowIfNull(npc);

        var menu = new ContextMenu();

        // Header
        menu.Items.Add(CreateHeader($"👤 {npc.Name}"));
        menu.Items.Add(new Separator());

        // Actions
        menu.Items.Add(CreateMenuItem(MenuActions.Examine(), () => ExecuteCommand($"examine {npc.Name}")));
        menu.Items.Add(CreateMenuItem(MenuActions.Talk(), () => ExecuteCommand($"talk {npc.Name}")));

        // Cancel
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem(MenuActions.Cancel(), () => menu.Close()));

        _logger.LogDebug("Created NPC menu for {NpcName}", npc.Name);
        return menu;
    }

    /// <inheritdoc />
    public ContextMenu CreateGroundItemMenu(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var menu = new ContextMenu();

        // Header with [Ground] indicator
        var icon = ItemSlotViewModel.GetCategoryIcon(item.Type);
        menu.Items.Add(CreateHeader($"{icon} {item.Name} [Ground]"));
        menu.Items.Add(new Separator());

        // Actions
        menu.Items.Add(CreateMenuItem(MenuActions.Examine(), () => ExecuteCommand($"examine {item.Name}")));
        menu.Items.Add(CreateMenuItem(MenuActions.Take(), () => ExecuteCommand($"take {item.Name}")));

        // Cancel
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateMenuItem(MenuActions.Cancel(), () => menu.Close()));

        _logger.LogDebug("Created ground item menu for {ItemName}", item.Name);
        return menu;
    }

    /// <inheritdoc />
    public void ShowMenu(ContextMenu menu, Control target, Point position)
    {
        menu.PlacementTarget = target;
        menu.Placement = global::Avalonia.Controls.PlacementMode.Pointer;
        menu.Open(target);

        _logger.LogDebug("Showing context menu at ({X}, {Y})", position.X, position.Y);
    }

    /// <summary>
    /// Creates a disabled header menu item.
    /// </summary>
    /// <param name="text">The header text.</param>
    /// <returns>A disabled menu item styled as a header.</returns>
    private static MenuItem CreateHeader(string text)
    {
        return new MenuItem
        {
            Header = text,
            IsEnabled = false,
            Classes = { "menu-header" }
        };
    }

    /// <summary>
    /// Creates a menu item with a click action.
    /// </summary>
    /// <param name="header">The menu item text with icon.</param>
    /// <param name="action">The action to execute on click.</param>
    /// <returns>A configured menu item.</returns>
    private static MenuItem CreateMenuItem(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => action();
        return item;
    }

    /// <summary>
    /// Executes a command via the game session.
    /// </summary>
    /// <param name="command">The command string to execute.</param>
    private void ExecuteCommand(string command)
    {
        _logger.LogInformation("Executing context menu command: {Command}", command);
        CommandExecuted?.Invoke(command);
    }
}
