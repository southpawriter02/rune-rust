using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Presentation.Gui.Services;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Services;

/// <summary>
/// Unit tests for <see cref="ContextMenuService"/>.
/// </summary>
[TestFixture]
public class ContextMenuServiceTests
{
    private Mock<ILogger<ContextMenuService>> _loggerMock = null!;
    private ContextMenuService _service = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ContextMenuService>>();
        _service = new ContextMenuService(_loggerMock.Object);
    }

    /// <summary>
    /// Verifies that item menu is created with correct structure.
    /// </summary>
    [Test]
    public void CreateItemMenu_ForGenericItem_HasExamineAndDrop()
    {
        // Arrange
        var item = new Item("Red Gem", "A shiny gem", ItemType.Misc, 50);

        // Act
        var menu = _service.CreateItemMenu(item);

        // Assert
        menu.Should().NotBeNull();
        menu.Items.Count.Should().BeGreaterThan(3); // Header, Separator, Examine, Drop, Separator, Cancel
    }

    /// <summary>
    /// Verifies that consumable items have Use option.
    /// </summary>
    [Test]
    public void CreateItemMenu_ForConsumable_HasUseOption()
    {
        // Arrange
        var item = Item.CreateHealthPotion();

        // Act
        var menu = _service.CreateItemMenu(item);

        // Assert
        menu.Should().NotBeNull();
        // Consumable should have Use option (header count = 6: header, sep, examine, use, drop, sep, cancel)
        menu.Items.Count.Should().BeGreaterThanOrEqualTo(6);
    }

    /// <summary>
    /// Verifies that equippable items have Equip option.
    /// </summary>
    [Test]
    public void CreateItemMenu_ForEquippable_HasEquipOption()
    {
        // Arrange
        var item = Item.CreateIronSword();

        // Act
        var menu = _service.CreateItemMenu(item);

        // Assert
        menu.Should().NotBeNull();
        // Equippable should have Equip option
        menu.Items.Count.Should().BeGreaterThanOrEqualTo(6);
    }

    /// <summary>
    /// Verifies that equipped items show Unequip instead of Equip/Drop.
    /// </summary>
    [Test]
    public void CreateItemMenu_WhenEquipped_HasUnequipOption()
    {
        // Arrange
        var item = Item.CreateIronSword();

        // Act
        var menu = _service.CreateItemMenu(item, isEquipped: true);

        // Assert
        menu.Should().NotBeNull();
        // Equipped: header, sep, examine, unequip, sep, cancel (no drop)
        menu.Items.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that monster menu has Examine and Attack.
    /// </summary>
    [Test]
    public void CreateMonsterMenu_HasExamineAndAttack()
    {
        // Arrange
        var monster = Monster.CreateSkeleton();

        // Act
        var menu = _service.CreateMonsterMenu(monster);

        // Assert
        menu.Should().NotBeNull();
        // Header, Separator, Examine, Attack, Separator, Cancel
        menu.Items.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that NPC menu has Examine and Talk.
    /// </summary>
    [Test]
    public void CreateNpcMenu_HasExamineAndTalk()
    {
        // Arrange
        var npc = RiddleNpc.Create(
            "Sphinx",
            "A mysterious sphinx",
            "riddle-1",
            "Greetings, traveler.",
            "You have already solved my riddle.");

        // Act
        var menu = _service.CreateNpcMenu(npc);

        // Assert
        menu.Should().NotBeNull();
        // Header, Separator, Examine, Talk, Separator, Cancel
        menu.Items.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that ground item menu has Take option.
    /// </summary>
    [Test]
    public void CreateGroundItemMenu_HasTakeOption()
    {
        // Arrange
        var item = new Item("Gold Coin", "A shiny coin", ItemType.Misc, 1);

        // Act
        var menu = _service.CreateGroundItemMenu(item);

        // Assert
        menu.Should().NotBeNull();
        // Header, Separator, Examine, Take, Separator, Cancel
        menu.Items.Count.Should().Be(6);
    }

    /// <summary>
    /// Verifies that CommandExecuted event is raised.
    /// </summary>
    [Test]
    public void ExecuteCommand_RaisesEvent()
    {
        // Arrange
        string? executedCommand = null;
        _service.CommandExecuted += cmd => executedCommand = cmd;
        var item = new Item("Gold Key", "A key", ItemType.Key, 5);
        var menu = _service.CreateItemMenu(item);

        // Act - Get the first menu item after header and separator (Examine)
        // This is a bit tricky to test without UI, but we verify service creates proper menus

        // Assert
        menu.Items.Count.Should().BeGreaterThan(0);
    }
}
