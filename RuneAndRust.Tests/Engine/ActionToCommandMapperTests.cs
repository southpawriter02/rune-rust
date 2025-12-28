using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Engine.Helpers;
using Xunit;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Unit tests for the ActionToCommandMapper helper (v0.3.23b).
/// Tests the reverse mapping from GameAction to command strings for CommandParser.
/// </summary>
public class ActionToCommandMapperTests
{
    #region ToCommand Tests

    [Theory]
    [InlineData(GameAction.MoveNorth, "north")]
    [InlineData(GameAction.MoveSouth, "south")]
    [InlineData(GameAction.MoveEast, "east")]
    [InlineData(GameAction.MoveWest, "west")]
    [InlineData(GameAction.MoveUp, "up")]
    [InlineData(GameAction.MoveDown, "down")]
    public void ToCommand_MovementActions_ReturnsCorrectCommand(GameAction action, string expectedCommand)
    {
        // Act
        var result = ActionToCommandMapper.ToCommand(action);

        // Assert
        result.Should().Be(expectedCommand);
    }

    [Theory]
    [InlineData(GameAction.Confirm, "confirm")]
    [InlineData(GameAction.Cancel, "cancel")]
    [InlineData(GameAction.Menu, "menu")]
    [InlineData(GameAction.Help, "help")]
    public void ToCommand_CoreActions_ReturnsCorrectCommand(GameAction action, string expectedCommand)
    {
        // Act
        var result = ActionToCommandMapper.ToCommand(action);

        // Assert
        result.Should().Be(expectedCommand);
    }

    [Theory]
    [InlineData(GameAction.Inventory, "pack")]
    [InlineData(GameAction.Character, "status")]
    [InlineData(GameAction.Journal, "archive")]
    [InlineData(GameAction.Crafting, "bench")]
    public void ToCommand_ScreenNavigation_ReturnsCorrectCommand(GameAction action, string expectedCommand)
    {
        // Act
        var result = ActionToCommandMapper.ToCommand(action);

        // Assert
        result.Should().Be(expectedCommand);
    }

    [Theory]
    [InlineData(GameAction.Interact, "interact")]
    [InlineData(GameAction.Look, "look")]
    [InlineData(GameAction.Search, "search")]
    [InlineData(GameAction.Wait, "wait")]
    public void ToCommand_GameplayActions_ReturnsCorrectCommand(GameAction action, string expectedCommand)
    {
        // Act
        var result = ActionToCommandMapper.ToCommand(action);

        // Assert
        result.Should().Be(expectedCommand);
    }

    [Theory]
    [InlineData(GameAction.Attack, "attack")]
    [InlineData(GameAction.LightAttack, "light")]
    [InlineData(GameAction.HeavyAttack, "heavy")]
    [InlineData(GameAction.UseAbility, "use")]
    [InlineData(GameAction.Flee, "flee")]
    public void ToCommand_CombatActions_ReturnsCorrectCommand(GameAction action, string expectedCommand)
    {
        // Act
        var result = ActionToCommandMapper.ToCommand(action);

        // Assert
        result.Should().Be(expectedCommand);
    }

    [Fact]
    public void ToCommand_UnmappedAction_ReturnsNull()
    {
        // Arrange - Use a value that's not in the enum (if possible) or test with a placeholder
        // Since all GameAction values are mapped, we verify behavior with invalid cast
        var unmappedAction = (GameAction)(-1);

        // Act
        var result = ActionToCommandMapper.ToCommand(unmappedAction);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllMappings Tests

    [Fact]
    public void GetAllMappings_ReturnsNonEmptyDictionary()
    {
        // Act
        var mappings = ActionToCommandMapper.GetAllMappings();

        // Assert
        mappings.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAllMappings_ContainsAllExpectedActions()
    {
        // Act
        var mappings = ActionToCommandMapper.GetAllMappings();

        // Assert - Check that common actions are mapped
        mappings.Should().ContainKey(GameAction.MoveNorth);
        mappings.Should().ContainKey(GameAction.Confirm);
        mappings.Should().ContainKey(GameAction.Attack);
        mappings.Should().ContainKey(GameAction.Inventory);
    }

    [Fact]
    public void GetAllMappings_IsReadOnly()
    {
        // Act
        var mappings = ActionToCommandMapper.GetAllMappings();

        // Assert
        mappings.Should().BeAssignableTo<IReadOnlyDictionary<GameAction, string>>();
    }

    #endregion

    #region Bidirectional Mapping Tests

    [Theory]
    [InlineData(GameAction.MoveNorth, "north")]
    [InlineData(GameAction.Attack, "attack")]
    [InlineData(GameAction.Confirm, "confirm")]
    public void ToCommand_IsReversibleWithCommandToActionMapper(GameAction action, string expectedCommand)
    {
        // Act - Forward mapping
        var command = ActionToCommandMapper.ToCommand(action);
        command.Should().Be(expectedCommand);

        // Act - Reverse mapping via CommandToActionMapper
        var success = CommandToActionMapper.TryMapCommand(command!, out var reversedAction);

        // Assert
        success.Should().BeTrue();
        reversedAction.Should().Be(action);
    }

    #endregion
}
