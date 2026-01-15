using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.Models;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.Models;

/// <summary>
/// Unit tests for <see cref="GameMessage"/>.
/// </summary>
[TestFixture]
public class GameMessageTests
{
    /// <summary>
    /// Verifies that FormattedTime returns correct format.
    /// </summary>
    [Test]
    public void FormattedTime_ReturnsHHmmssFormat()
    {
        // Arrange
        var timestamp = new DateTime(2026, 1, 15, 14, 30, 45);
        var message = new GameMessage(timestamp, "Test", MessageType.Info);

        // Assert
        message.FormattedTime.Should().Be("14:30:45");
    }

    /// <summary>
    /// Verifies that Info factory creates correct message.
    /// </summary>
    [Test]
    public void Info_CreatesMessageWithInfoType()
    {
        // Act
        var message = GameMessage.Info("Test info");

        // Assert
        message.Text.Should().Be("Test info");
        message.Type.Should().Be(MessageType.Info);
        message.Category.Should().Be(MessageCategory.General);
    }

    /// <summary>
    /// Verifies that Combat factory creates message with Combat category.
    /// </summary>
    [Test]
    public void Combat_CreatesMessageWithCombatCategory()
    {
        // Act
        var message = GameMessage.Combat("Hit for 10 damage", MessageType.CombatHit);

        // Assert
        message.Text.Should().Be("Hit for 10 damage");
        message.Type.Should().Be(MessageType.CombatHit);
        message.Category.Should().Be(MessageCategory.Combat);
    }

    /// <summary>
    /// Verifies that Loot factory creates message with Loot category.
    /// </summary>
    [Test]
    public void Loot_CreatesMessageWithLootCategory()
    {
        // Act
        var message = GameMessage.Loot("Found a sword", MessageType.LootRare);

        // Assert
        message.Text.Should().Be("Found a sword");
        message.Type.Should().Be(MessageType.LootRare);
        message.Category.Should().Be(MessageCategory.Loot);
    }

    /// <summary>
    /// Verifies that Dialogue factory creates message with Dialogue category.
    /// </summary>
    [Test]
    public void Dialogue_CreatesMessageWithDialogueCategory()
    {
        // Act
        var message = GameMessage.Dialogue("Hello, traveler!");

        // Assert
        message.Text.Should().Be("Hello, traveler!");
        message.Type.Should().Be(MessageType.Dialogue);
        message.Category.Should().Be(MessageCategory.Dialogue);
    }

    /// <summary>
    /// Verifies that Success factory creates correct message.
    /// </summary>
    [Test]
    public void Success_CreatesMessageWithSuccessType()
    {
        // Act
        var message = GameMessage.Success("Quest complete!");

        // Assert
        message.Text.Should().Be("Quest complete!");
        message.Type.Should().Be(MessageType.Success);
    }
}
