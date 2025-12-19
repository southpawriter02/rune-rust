using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Engine.Services;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;
using Character = RuneAndRust.Core.Entities.Character;

namespace RuneAndRust.Tests.Engine;

/// <summary>
/// Tests for the InitiativeService class.
/// Validates initiative calculation and turn order sorting.
/// </summary>
public class InitiativeServiceTests
{
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<ILogger<InitiativeService>> _mockLogger;
    private readonly InitiativeService _sut;

    public InitiativeServiceTests()
    {
        _mockDice = new Mock<IDiceService>();
        _mockLogger = new Mock<ILogger<InitiativeService>>();
        _sut = new InitiativeService(_mockDice.Object, _mockLogger.Object);
    }

    #region RollInitiative Tests

    [Fact]
    public void RollInitiative_SetsInitiativeProperty()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(10, It.IsAny<string>())).Returns(5);
        var combatant = new Combatant { Name = "TestCombatant" };

        // Act
        _sut.RollInitiative(combatant);

        // Assert
        combatant.Initiative.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RollInitiative_AddsAttributesToRoll()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(10, It.IsAny<string>())).Returns(5);
        var character = new Character
        {
            Name = "Player",
            Finesse = 4,
            Wits = 3
        };
        var combatant = Combatant.FromCharacter(character);

        // Act
        _sut.RollInitiative(combatant);

        // Assert
        // Formula: d10(5) + Finesse(4) + Wits(3) = 12
        combatant.Initiative.Should().Be(12);
    }

    [Fact]
    public void RollInitiative_UsesEnemyAttributes()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(10, It.IsAny<string>())).Returns(7);
        var enemy = new Enemy
        {
            Name = "Goblin",
            Attributes = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Finesse, 6 },
                { CharacterAttribute.Wits, 2 }
            }
        };
        var combatant = Combatant.FromEnemy(enemy);

        // Act
        _sut.RollInitiative(combatant);

        // Assert
        // Formula: d10(7) + Finesse(6) + Wits(2) = 15
        combatant.Initiative.Should().Be(15);
    }

    [Fact]
    public void RollInitiative_CallsDiceServiceWithCorrectContext()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(10, It.IsAny<string>())).Returns(5);
        var combatant = new Combatant { Name = "TestFighter" };

        // Act
        _sut.RollInitiative(combatant);

        // Assert
        _mockDice.Verify(d => d.RollSingle(10, "Initiative:TestFighter"), Times.Once);
    }

    [Fact]
    public void RollInitiative_WithZeroAttributes_ReturnsRollValueOnly()
    {
        // Arrange
        _mockDice.Setup(d => d.RollSingle(10, It.IsAny<string>())).Returns(8);
        var combatant = new Combatant { Name = "EmptyCombatant" };
        // No source set, so attributes return 0

        // Act
        _sut.RollInitiative(combatant);

        // Assert
        combatant.Initiative.Should().Be(8);
    }

    #endregion

    #region SortTurnOrder Tests

    [Fact]
    public void SortTurnOrder_HigherInitiativeFirst()
    {
        // Arrange
        var fast = new Combatant { Name = "Fast", Initiative = 20 };
        var slow = new Combatant { Name = "Slow", Initiative = 5 };
        var combatants = new List<Combatant> { slow, fast };

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.First().Should().Be(fast);
        sorted.Last().Should().Be(slow);
    }

    [Fact]
    public void SortTurnOrder_FinesseBreaksTies()
    {
        // Arrange
        var nimble = CreateCombatantWithFinesse("Nimble", initiative: 15, finesse: 8);
        var clumsy = CreateCombatantWithFinesse("Clumsy", initiative: 15, finesse: 3);
        var combatants = new List<Combatant> { clumsy, nimble };

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.First().Should().Be(nimble);
    }

    [Fact]
    public void SortTurnOrder_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var combatants = new List<Combatant>();

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.Should().BeEmpty();
    }

    [Fact]
    public void SortTurnOrder_SingleCombatant_ReturnsSame()
    {
        // Arrange
        var solo = new Combatant { Name = "Solo", Initiative = 10 };
        var combatants = new List<Combatant> { solo };

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.Should().HaveCount(1);
        sorted.First().Should().Be(solo);
    }

    [Fact]
    public void SortTurnOrder_MultipleCombatants_SortsCorrectly()
    {
        // Arrange
        var combatants = new List<Combatant>
        {
            new Combatant { Name = "C", Initiative = 10 },
            new Combatant { Name = "A", Initiative = 20 },
            new Combatant { Name = "B", Initiative = 15 }
        };

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.Select(c => c.Name).Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public void SortTurnOrder_ReturnsNewList()
    {
        // Arrange
        var combatants = new List<Combatant>
        {
            new Combatant { Name = "Test", Initiative = 10 }
        };

        // Act
        var sorted = _sut.SortTurnOrder(combatants);

        // Assert
        sorted.Should().NotBeSameAs(combatants);
    }

    #endregion

    #region Helper Methods

    private static Combatant CreateCombatantWithFinesse(string name, int initiative, int finesse)
    {
        var character = new Character
        {
            Name = name,
            Finesse = finesse
        };
        var combatant = Combatant.FromCharacter(character);
        combatant.Initiative = initiative;
        return combatant;
    }

    #endregion
}
