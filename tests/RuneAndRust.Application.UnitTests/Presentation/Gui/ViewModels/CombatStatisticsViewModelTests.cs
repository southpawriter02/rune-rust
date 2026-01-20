using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Presentation.Gui.ViewModels;

namespace RuneAndRust.Application.UnitTests.Presentation.Gui.ViewModels;

/// <summary>
/// Unit tests for <see cref="CombatStatisticsViewModel"/>.
/// </summary>
[TestFixture]
public class CombatStatisticsViewModelTests
{
    /// <summary>
    /// Verifies all text properties are formatted correctly.
    /// </summary>
    [Test]
    public void Constructor_WithValues_FormatsTextCorrectly()
    {
        // Arrange & Act
        var stats = new CombatStatisticsViewModel(
            rounds: 5,
            damageDealt: 127,
            damageTaken: 45,
            criticals: 3,
            abilitiesUsed: 7,
            itemsUsed: 2);

        // Assert
        stats.RoundsText.Should().Be("Rounds: 5");
        stats.DamageDealtText.Should().Contain("127");
        stats.DamageTakenText.Should().Contain("45");
        stats.CriticalsText.Should().Be("Criticals: 3");
        stats.AbilitiesText.Should().Be("Abilities: 7");
        stats.ItemsUsedText.Should().Be("Items Used: 2");
    }

    /// <summary>
    /// Verifies default constructor creates zeroed stats.
    /// </summary>
    [Test]
    public void DefaultConstructor_CreatesZeroedStats()
    {
        // Arrange & Act
        var stats = new CombatStatisticsViewModel();

        // Assert
        stats.Rounds.Should().Be(0);
        stats.DamageDealt.Should().Be(0);
        stats.RoundsText.Should().Be("Rounds: 0");
    }
}
