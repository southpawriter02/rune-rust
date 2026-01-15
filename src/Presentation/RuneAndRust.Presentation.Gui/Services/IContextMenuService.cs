namespace RuneAndRust.Presentation.Gui.Services;

using global::Avalonia;
using global::Avalonia.Controls;
using RuneAndRust.Domain.Entities;

/// <summary>
/// Creates and manages context menus for game elements.
/// </summary>
public interface IContextMenuService
{
    /// <summary>
    /// Creates a context menu for an inventory item.
    /// </summary>
    /// <param name="item">The item to create a menu for.</param>
    /// <param name="isEquipped">Whether the item is currently equipped.</param>
    /// <returns>A context menu with appropriate actions.</returns>
    ContextMenu CreateItemMenu(Item item, bool isEquipped = false);

    /// <summary>
    /// Creates a context menu for a monster.
    /// </summary>
    /// <param name="monster">The monster to create a menu for.</param>
    /// <returns>A context menu with attack and examine options.</returns>
    ContextMenu CreateMonsterMenu(Monster monster);

    /// <summary>
    /// Creates a context menu for a RiddleNpc.
    /// </summary>
    /// <param name="npc">The RiddleNpc to create a menu for.</param>
    /// <returns>A context menu with talk and examine options.</returns>
    ContextMenu CreateNpcMenu(RiddleNpc npc);

    /// <summary>
    /// Creates a context menu for an item on the ground.
    /// </summary>
    /// <param name="item">The ground item to create a menu for.</param>
    /// <returns>A context menu with take and examine options.</returns>
    ContextMenu CreateGroundItemMenu(Item item);

    /// <summary>
    /// Shows a context menu at the specified location.
    /// </summary>
    /// <param name="menu">The menu to display.</param>
    /// <param name="target">The control that triggered the menu.</param>
    /// <param name="position">The position relative to the target.</param>
    void ShowMenu(ContextMenu menu, Control target, Point position);
}
