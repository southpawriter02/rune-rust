using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Helpers;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the CommandToActionMapper static helper (v0.3.23a).
/// Validates command string to GameAction enum mapping.
/// </summary>
public class CommandToActionMapperTests
{
    #region TryMapCommand Tests

    [Theory]
    [InlineData("north", GameAction.MoveNorth)]
    [InlineData("south", GameAction.MoveSouth)]
    [InlineData("east", GameAction.MoveEast)]
    [InlineData("west", GameAction.MoveWest)]
    [InlineData("up", GameAction.MoveUp)]
    [InlineData("down", GameAction.MoveDown)]
    [InlineData("confirm", GameAction.Confirm)]
    [InlineData("cancel", GameAction.Cancel)]
    [InlineData("menu", GameAction.Menu)]
    [InlineData("help", GameAction.Help)]
    [InlineData("inventory", GameAction.Inventory)]
    [InlineData("character", GameAction.Character)]
    [InlineData("journal", GameAction.Journal)]
    [InlineData("bench", GameAction.Crafting)]
    [InlineData("interact", GameAction.Interact)]
    [InlineData("look", GameAction.Look)]
    [InlineData("search", GameAction.Search)]
    [InlineData("wait", GameAction.Wait)]
    [InlineData("attack", GameAction.Attack)]
    [InlineData("light", GameAction.LightAttack)]
    [InlineData("heavy", GameAction.HeavyAttack)]
    public void TryMapCommand_ValidCommand_ReturnsTrueWithCorrectAction(string command, GameAction expected)
    {
        // Act
        var result = CommandToActionMapper.TryMapCommand(command, out var action);

        // Assert
        result.Should().BeTrue($"'{command}' should be a valid command");
        action.Should().Be(expected);
    }

    [Theory]
    [InlineData("NORTH", GameAction.MoveNorth)]
    [InlineData("North", GameAction.MoveNorth)]
    [InlineData("CONFIRM", GameAction.Confirm)]
    [InlineData("Attack", GameAction.Attack)]
    [InlineData("INVENTORY", GameAction.Inventory)]
    public void TryMapCommand_IsCaseInsensitive(string command, GameAction expected)
    {
        // Act
        var result = CommandToActionMapper.TryMapCommand(command, out var action);

        // Assert
        result.Should().BeTrue($"'{command}' should match regardless of case");
        action.Should().Be(expected);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("move_north")]
    [InlineData("northward")]
    [InlineData("go north")]
    [InlineData("123")]
    public void TryMapCommand_InvalidCommand_ReturnsFalse(string command)
    {
        // Act
        var result = CommandToActionMapper.TryMapCommand(command, out _);

        // Assert
        result.Should().BeFalse($"'{command}' should not be a valid command");
    }

    #endregion

    #region GetAllMappings Tests

    [Fact]
    public void GetAllMappings_Returns21Entries()
    {
        // Act
        var mappings = CommandToActionMapper.GetAllMappings();

        // Assert
        mappings.Should().HaveCount(21, "there should be 21 command-to-action mappings");
    }

    [Fact]
    public void GetAllMappings_ReturnsReadOnlyDictionary()
    {
        // Act
        var mappings = CommandToActionMapper.GetAllMappings();

        // Assert
        mappings.Should().BeAssignableTo<IReadOnlyDictionary<string, GameAction>>();
    }

    [Fact]
    public void GetAllMappings_ContainsAllMovementCommands()
    {
        // Act
        var mappings = CommandToActionMapper.GetAllMappings();

        // Assert
        mappings.Should().ContainKeys("north", "south", "east", "west", "up", "down");
    }

    [Fact]
    public void GetAllMappings_ContainsAllCoreCommands()
    {
        // Act
        var mappings = CommandToActionMapper.GetAllMappings();

        // Assert
        mappings.Should().ContainKeys("confirm", "cancel", "menu", "help");
    }

    [Fact]
    public void GetAllMappings_ContainsAllCombatCommands()
    {
        // Act
        var mappings = CommandToActionMapper.GetAllMappings();

        // Assert
        mappings.Should().ContainKeys("attack", "light", "heavy");
    }

    #endregion
}
