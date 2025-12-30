using FluentAssertions;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Core;

/// <summary>
/// Tests for the GamePhase enum.
/// Validates the game phase values and structure.
/// </summary>
public class GamePhaseTests
{
    [Fact]
    public void GamePhase_ShouldHaveExactlySixValues()
    {
        // Arrange
        var values = Enum.GetValues<GamePhase>();

        // Assert
        values.Should().HaveCount(6, "GamePhase should have exactly 6 phases: MainMenu, Exploration, Combat, Quit, SagaMenu, SpecializationMenu");
    }

    [Fact]
    public void GamePhase_ShouldContain_MainMenu()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.MainMenu).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_ShouldContain_Exploration()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.Exploration).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_ShouldContain_Combat()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.Combat).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_ShouldContain_Quit()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.Quit).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_ShouldContain_SagaMenu()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.SagaMenu).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_ShouldContain_SpecializationMenu()
    {
        // Assert
        Enum.IsDefined(typeof(GamePhase), GamePhase.SpecializationMenu).Should().BeTrue();
    }

    [Fact]
    public void GamePhase_EnumValues_ShouldBeSequential()
    {
        // Assert
        ((int)GamePhase.MainMenu).Should().Be(0);
        ((int)GamePhase.Exploration).Should().Be(1);
        ((int)GamePhase.Combat).Should().Be(2);
        ((int)GamePhase.Quit).Should().Be(3);
        ((int)GamePhase.SagaMenu).Should().Be(4);
        ((int)GamePhase.SpecializationMenu).Should().Be(5);
    }

    [Theory]
    [InlineData(GamePhase.MainMenu, "MainMenu")]
    [InlineData(GamePhase.Exploration, "Exploration")]
    [InlineData(GamePhase.Combat, "Combat")]
    [InlineData(GamePhase.Quit, "Quit")]
    [InlineData(GamePhase.SagaMenu, "SagaMenu")]
    [InlineData(GamePhase.SpecializationMenu, "SpecializationMenu")]
    public void GamePhase_ToString_ReturnsExpectedName(GamePhase phase, string expectedName)
    {
        // Assert
        phase.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0, GamePhase.MainMenu)]
    [InlineData(1, GamePhase.Exploration)]
    [InlineData(2, GamePhase.Combat)]
    [InlineData(3, GamePhase.Quit)]
    [InlineData(4, GamePhase.SagaMenu)]
    [InlineData(5, GamePhase.SpecializationMenu)]
    public void GamePhase_FromInt_ReturnsCorrectPhase(int value, GamePhase expected)
    {
        // Act
        var phase = (GamePhase)value;

        // Assert
        phase.Should().Be(expected);
    }

    [Fact]
    public void GamePhase_DefaultValue_ShouldBeMainMenu()
    {
        // Arrange & Act
        var defaultPhase = default(GamePhase);

        // Assert
        defaultPhase.Should().Be(GamePhase.MainMenu);
    }
}
