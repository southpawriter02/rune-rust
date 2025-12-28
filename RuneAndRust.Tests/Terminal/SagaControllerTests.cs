using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using RuneAndRust.Terminal.Controllers;
using Xunit;
using Character = RuneAndRust.Core.Entities.Character;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Terminal;

/// <summary>
/// Unit tests for the SagaController class (v0.4.0c).
/// Validates navigation, upgrade actions, and menu exit behavior in The Shrine UI.
/// </summary>
/// <remarks>See: v0.4.0c (The Shrine) for Saga UI implementation.</remarks>
public class SagaControllerTests
{
    private readonly Mock<IProgressionService> _mockProgression;
    private readonly Mock<ILogger<SagaController>> _mockLogger;
    private readonly SagaController _sut;

    public SagaControllerTests()
    {
        _mockProgression = new Mock<IProgressionService>();
        _mockLogger = new Mock<ILogger<SagaController>>();
        _sut = new SagaController(_mockProgression.Object, _mockLogger.Object);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a test character with default attributes (all 5) and specified PP.
    /// </summary>
    private static Character CreateTestCharacter(int pp = 5)
    {
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "TestHero",
            ProgressionPoints = pp,
            MaxHP = 100,
            CurrentHP = 100,
            MaxStamina = 50,
            CurrentStamina = 50
        };

        character.SetAttribute(CharacterAttribute.Might, 5);
        character.SetAttribute(CharacterAttribute.Finesse, 5);
        character.SetAttribute(CharacterAttribute.Wits, 5);
        character.SetAttribute(CharacterAttribute.Will, 5);
        character.SetAttribute(CharacterAttribute.Sturdiness, 5);

        return character;
    }

    /// <summary>
    /// Sets up the progression service mock to return a success result.
    /// </summary>
    private void SetupProgressionSuccess()
    {
        _mockProgression
            .Setup(p => p.UpgradeAttribute(It.IsAny<Character>(), It.IsAny<CharacterAttribute>()))
            .Returns(AttributeUpgradeResult.Ok("Upgraded", CharacterAttribute.Might, 5, 6, 1));
    }

    /// <summary>
    /// Sets up the progression service mock to return a failure result.
    /// </summary>
    private void SetupProgressionFailure()
    {
        _mockProgression
            .Setup(p => p.UpgradeAttribute(It.IsAny<Character>(), It.IsAny<CharacterAttribute>()))
            .Returns(AttributeUpgradeResult.Failure("Insufficient PP"));
    }

    /// <summary>
    /// Sets the controller's SelectedIndex to a specific value using repeated key presses.
    /// </summary>
    private void SetSelectedIndex(int targetIndex, Character character)
    {
        // Reset to 0 first
        _sut.ResetSelection();

        // Navigate to target index
        for (int i = 0; i < targetIndex; i++)
        {
            _sut.HandleInput(ConsoleKey.DownArrow, character);
        }
    }

    #endregion

    #region Navigation - Up Arrow Tests

    [Fact]
    public void HandleInput_UpArrow_DecrementsIndex()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(2, character);
        int initialIndex = _sut.SelectedIndex;

        // Act
        var result = _sut.HandleInput(ConsoleKey.UpArrow, character);

        // Assert
        _sut.SelectedIndex.Should().Be(initialIndex - 1);
        result.Should().Be(GamePhase.SagaMenu, "Should stay in SagaMenu");
    }

    [Fact]
    public void HandleInput_UpArrow_AtZero_StaysZero()
    {
        // Arrange
        var character = CreateTestCharacter();
        _sut.ResetSelection();
        _sut.SelectedIndex.Should().Be(0, "Precondition: Start at 0");

        // Act
        var result = _sut.HandleInput(ConsoleKey.UpArrow, character);

        // Assert
        _sut.SelectedIndex.Should().Be(0, "Should not go below 0");
        result.Should().Be(GamePhase.SagaMenu);
    }

    [Fact]
    public void HandleInput_W_DecrementsIndex()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(2, character);
        int initialIndex = _sut.SelectedIndex;

        // Act
        var result = _sut.HandleInput(ConsoleKey.W, character);

        // Assert
        _sut.SelectedIndex.Should().Be(initialIndex - 1);
        result.Should().Be(GamePhase.SagaMenu);
    }

    #endregion

    #region Navigation - Down Arrow Tests

    [Fact]
    public void HandleInput_DownArrow_IncrementsIndex()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(2, character);
        int initialIndex = _sut.SelectedIndex;

        // Act
        var result = _sut.HandleInput(ConsoleKey.DownArrow, character);

        // Assert
        _sut.SelectedIndex.Should().Be(initialIndex + 1);
        result.Should().Be(GamePhase.SagaMenu, "Should stay in SagaMenu");
    }

    [Fact]
    public void HandleInput_DownArrow_AtMax_StaysMax()
    {
        // Arrange
        var character = CreateTestCharacter();
        int maxIndex = SagaController.AttributeCount - 1;
        SetSelectedIndex(maxIndex, character);
        _sut.SelectedIndex.Should().Be(maxIndex, $"Precondition: Start at {maxIndex}");

        // Act
        var result = _sut.HandleInput(ConsoleKey.DownArrow, character);

        // Assert
        _sut.SelectedIndex.Should().Be(maxIndex, "Should not exceed max index");
        result.Should().Be(GamePhase.SagaMenu);
    }

    [Fact]
    public void HandleInput_S_IncrementsIndex()
    {
        // Arrange
        var character = CreateTestCharacter();
        _sut.ResetSelection();
        int initialIndex = _sut.SelectedIndex;

        // Act
        var result = _sut.HandleInput(ConsoleKey.S, character);

        // Assert
        _sut.SelectedIndex.Should().Be(initialIndex + 1);
        result.Should().Be(GamePhase.SagaMenu);
    }

    #endregion

    #region Upgrade Action Tests

    [Fact]
    public void HandleInput_Enter_CallsProgressionService()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetupProgressionSuccess();
        SetSelectedIndex(0, character);

        // Act
        var result = _sut.HandleInput(ConsoleKey.Enter, character);

        // Assert
        _mockProgression.Verify(
            p => p.UpgradeAttribute(character, CharacterAttribute.Might),
            Times.Once);
        result.Should().Be(GamePhase.SagaMenu, "Should stay in menu after upgrade");
    }

    [Fact]
    public void HandleInput_Spacebar_CallsProgressionService()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetupProgressionSuccess();
        SetSelectedIndex(1, character);

        // Act
        var result = _sut.HandleInput(ConsoleKey.Spacebar, character);

        // Assert
        _mockProgression.Verify(
            p => p.UpgradeAttribute(character, CharacterAttribute.Finesse),
            Times.Once);
        result.Should().Be(GamePhase.SagaMenu);
    }

    [Fact]
    public void HandleInput_Enter_UpgradesSelectedAttribute()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetupProgressionSuccess();
        SetSelectedIndex(2, character); // Sturdiness

        // Act
        _sut.HandleInput(ConsoleKey.Enter, character);

        // Assert
        _mockProgression.Verify(
            p => p.UpgradeAttribute(character, CharacterAttribute.Sturdiness),
            Times.Once);
    }

    [Fact]
    public void HandleInput_Enter_FailedUpgrade_StaysInMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetupProgressionFailure();

        // Act
        var result = _sut.HandleInput(ConsoleKey.Enter, character);

        // Assert
        result.Should().Be(GamePhase.SagaMenu, "Should stay in menu even on failed upgrade");
    }

    #endregion

    #region Exit Tests

    [Fact]
    public void HandleInput_Escape_ReturnsExploration()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = _sut.HandleInput(ConsoleKey.Escape, character);

        // Assert
        result.Should().Be(GamePhase.Exploration, "Escape should exit to Exploration");
    }

    [Fact]
    public void HandleInput_Escape_ResetsSelection()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(3, character);
        _sut.SelectedIndex.Should().Be(3, "Precondition: Index should be 3");

        // Act
        _sut.HandleInput(ConsoleKey.Escape, character);

        // Assert
        _sut.SelectedIndex.Should().Be(0, "Selection should reset to 0 on exit");
    }

    [Fact]
    public void HandleInput_Q_ReturnsExploration()
    {
        // Arrange
        var character = CreateTestCharacter();

        // Act
        var result = _sut.HandleInput(ConsoleKey.Q, character);

        // Assert
        result.Should().Be(GamePhase.Exploration, "Q should exit to Exploration");
    }

    [Fact]
    public void HandleInput_Q_ResetsSelection()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(2, character);

        // Act
        _sut.HandleInput(ConsoleKey.Q, character);

        // Assert
        _sut.SelectedIndex.Should().Be(0, "Selection should reset to 0 on exit");
    }

    #endregion

    #region Other Key Tests

    [Fact]
    public void HandleInput_OtherKey_NoEffect()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(2, character);
        int initialIndex = _sut.SelectedIndex;

        // Act
        var result = _sut.HandleInput(ConsoleKey.X, character);

        // Assert
        _sut.SelectedIndex.Should().Be(initialIndex, "Unhandled key should not change selection");
        result.Should().Be(GamePhase.SagaMenu, "Should stay in SagaMenu");
        _mockProgression.Verify(
            p => p.UpgradeAttribute(It.IsAny<Character>(), It.IsAny<CharacterAttribute>()),
            Times.Never,
            "Should not call ProgressionService");
    }

    [Fact]
    public void HandleInput_UnhandledKeys_ReturnSagaMenu()
    {
        // Arrange
        var character = CreateTestCharacter();
        var unhandledKeys = new[]
        {
            ConsoleKey.A, ConsoleKey.B, ConsoleKey.C, ConsoleKey.Tab,
            ConsoleKey.F1, ConsoleKey.Home, ConsoleKey.Delete
        };

        // Act & Assert
        foreach (var key in unhandledKeys)
        {
            var result = _sut.HandleInput(key, character);
            result.Should().Be(GamePhase.SagaMenu, $"Key {key} should return SagaMenu");
        }
    }

    #endregion

    #region Static Method Tests

    [Theory]
    [InlineData(0, CharacterAttribute.Might)]
    [InlineData(1, CharacterAttribute.Finesse)]
    [InlineData(2, CharacterAttribute.Sturdiness)]
    [InlineData(3, CharacterAttribute.Wits)]
    [InlineData(4, CharacterAttribute.Will)]
    public void GetAttributeAtIndex_ReturnsCorrectAttribute(int index, CharacterAttribute expected)
    {
        // Act
        var result = SagaController.GetAttributeAtIndex(index);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetAttributeAtIndex_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var action = () => SagaController.GetAttributeAtIndex(-1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetAttributeAtIndex_IndexTooHigh_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var action = () => SagaController.GetAttributeAtIndex(5);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AttributeCount_Returns5()
    {
        // Act & Assert
        SagaController.AttributeCount.Should().Be(5);
    }

    #endregion

    #region ResetSelection Tests

    [Fact]
    public void ResetSelection_SetsIndexToZero()
    {
        // Arrange
        var character = CreateTestCharacter();
        SetSelectedIndex(3, character);
        _sut.SelectedIndex.Should().Be(3, "Precondition");

        // Act
        _sut.ResetSelection();

        // Assert
        _sut.SelectedIndex.Should().Be(0);
    }

    #endregion
}
