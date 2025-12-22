using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;
using Xunit;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Tests for the TitleScreenService and TitleScreenResult classes (v0.3.4a).
/// Validates menu result factory methods and option handling.
/// </summary>
public class TitleScreenServiceTests
{
    #region TitleScreenResult Factory Method Tests

    [Fact]
    public void CreateNewGame_ReturnsNewGameOption()
    {
        // Act
        var result = TitleScreenResult.CreateNewGame();

        // Assert
        result.SelectedOption.Should().Be(MainMenuOption.NewGame);
    }

    [Fact]
    public void CreateNewGame_HasNoSaveSlotNumber()
    {
        // Act
        var result = TitleScreenResult.CreateNewGame();

        // Assert
        result.SaveSlotNumber.Should().BeNull();
    }

    [Fact]
    public void LoadGame_ReturnsLoadGameOption()
    {
        // Arrange
        var slotNumber = 1;

        // Act
        var result = TitleScreenResult.LoadGame(slotNumber);

        // Assert
        result.SelectedOption.Should().Be(MainMenuOption.LoadGame);
    }

    [Fact]
    public void LoadGame_ContainsSaveSlotNumber()
    {
        // Arrange
        var slotNumber = 2;

        // Act
        var result = TitleScreenResult.LoadGame(slotNumber);

        // Assert
        result.SaveSlotNumber.Should().Be(slotNumber);
    }

    [Fact]
    public void Quit_ReturnsQuitOption()
    {
        // Act
        var result = TitleScreenResult.Quit();

        // Assert
        result.SelectedOption.Should().Be(MainMenuOption.Quit);
    }

    [Fact]
    public void Quit_HasNoSaveSlotNumber()
    {
        // Act
        var result = TitleScreenResult.Quit();

        // Assert
        result.SaveSlotNumber.Should().BeNull();
    }

    #endregion

    #region MainMenuOption Enum Tests

    [Fact]
    public void MainMenuOption_HasFourValues()
    {
        // Act
        var values = Enum.GetValues<MainMenuOption>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(MainMenuOption.NewGame, 0)]
    [InlineData(MainMenuOption.LoadGame, 1)]
    [InlineData(MainMenuOption.Options, 2)]
    [InlineData(MainMenuOption.Quit, 3)]
    public void MainMenuOption_HasCorrectValues(MainMenuOption option, int expectedValue)
    {
        // Assert
        ((int)option).Should().Be(expectedValue);
    }

    [Fact]
    public void MainMenuOption_AllValuesAreDistinct()
    {
        // Arrange
        var options = Enum.GetValues<MainMenuOption>().ToList();

        // Assert
        options.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region TitleScreenResult Immutability Tests

    [Fact]
    public void TitleScreenResult_LoadGame_PreservesSlotNumber()
    {
        // Arrange
        var slotNumber = 3;

        // Act
        var result = TitleScreenResult.LoadGame(slotNumber);

        // Assert
        result.SaveSlotNumber.Should().Be(slotNumber);
    }

    [Fact]
    public void TitleScreenResult_MultipleCalls_ReturnIndependentInstances()
    {
        // Act
        var result1 = TitleScreenResult.CreateNewGame();
        var result2 = TitleScreenResult.CreateNewGame();

        // Assert
        result1.Should().NotBeSameAs(result2);
    }

    #endregion
}
