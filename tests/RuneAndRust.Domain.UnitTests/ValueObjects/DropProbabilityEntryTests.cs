using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="DropProbabilityEntry"/> value object.
/// </summary>
[TestFixture]
public class DropProbabilityEntryTests
{
    /// <summary>
    /// Verifies that Create with valid probabilities creates an entry.
    /// </summary>
    [Test]
    public void Create_WithValidProbabilities_CreatesEntry()
    {
        // Arrange
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0.6m },
            { QualityTier.Scavenged, 0.3m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0m },
            { QualityTier.MythForged, 0m }
        };

        // Act
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Trash, probs, 0.1m);

        // Assert
        entry.EnemyClass.Should().Be(EnemyDropClass.Trash);
        entry.NoDropChance.Should().Be(0.1m);
        entry.CanDropNothing.Should().BeTrue();
        entry.GuaranteedDrop.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create throws when probabilities don't sum to 1.0.
    /// </summary>
    [Test]
    public void Create_WithProbabilitiesNotSummingTo100_ThrowsArgumentException()
    {
        // Arrange - sum is only 0.5
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0.3m },
            { QualityTier.Scavenged, 0.2m }
        };

        // Act
        var act = () => DropProbabilityEntry.Create(EnemyDropClass.Trash, probs, 0m);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*sum to 1.0*");
    }

    /// <summary>
    /// Verifies that Trash enemies never return high tier drops.
    /// </summary>
    [Test]
    public void RollDrop_WithTrashEnemy_NeverReturnsHighTier()
    {
        // Arrange - Trash: 60% T0, 30% T1, 10% None
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0.6m },
            { QualityTier.Scavenged, 0.3m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0m },
            { QualityTier.MythForged, 0m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Trash, probs, 0.1m);
        var random = new Random(42);

        // Act - Roll 1000 times
        var highTierCount = 0;
        for (var i = 0; i < 1000; i++)
        {
            var result = entry.RollDrop(random);
            if (result.HasDrop && (int)result.Tier >= 2)
            {
                highTierCount++;
            }
        }

        // Assert - Should never get ClanForged or higher
        highTierCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Boss enemies never drop nothing.
    /// </summary>
    [Test]
    public void RollDrop_WithBossEnemy_NeverReturnsNoDrop()
    {
        // Arrange - Boss: 70% MythForged, 30% Optimized, 0% None
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0m },
            { QualityTier.Scavenged, 0m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0.3m },
            { QualityTier.MythForged, 0.7m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Boss, probs, 0m);
        var random = new Random(42);

        // Act - Roll 1000 times
        var noDropCount = 0;
        for (var i = 0; i < 1000; i++)
        {
            var result = entry.RollDrop(random);
            if (result.IsNoDrop)
            {
                noDropCount++;
            }
        }

        // Assert - Should never be no-drop
        noDropCount.Should().Be(0);
        entry.GuaranteedDrop.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that GetTierProbability returns correct values.
    /// </summary>
    [Test]
    public void GetTierProbability_ReturnsCorrectValue()
    {
        // Arrange
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0m },
            { QualityTier.Scavenged, 0m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0.3m },
            { QualityTier.MythForged, 0.7m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Boss, probs, 0m);

        // Assert
        entry.GetTierProbability(QualityTier.MythForged).Should().Be(0.7m);
        entry.GetTierProbability(QualityTier.Optimized).Should().Be(0.3m);
        entry.GetTierProbability(QualityTier.Scavenged).Should().Be(0m);
    }

    /// <summary>
    /// Verifies that HighestPossibleTier returns the correct tier.
    /// </summary>
    [Test]
    public void HighestPossibleTier_ReturnsCorrectTier()
    {
        // Arrange - Standard: T1-T3 drops
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0m },
            { QualityTier.Scavenged, 0.4m },
            { QualityTier.ClanForged, 0.4m },
            { QualityTier.Optimized, 0.2m },
            { QualityTier.MythForged, 0m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Standard, probs, 0m);

        // Assert
        entry.HighestPossibleTier.Should().Be(QualityTier.Optimized);
    }

    /// <summary>
    /// Verifies that Validate returns true for valid entry.
    /// </summary>
    [Test]
    public void Validate_WithValidEntry_ReturnsTrue()
    {
        // Arrange
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0.6m },
            { QualityTier.Scavenged, 0.3m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0m },
            { QualityTier.MythForged, 0m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Trash, probs, 0.1m);

        // Assert
        entry.Validate().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ToString returns a formatted string.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var probs = new Dictionary<QualityTier, decimal>
        {
            { QualityTier.JuryRigged, 0.6m },
            { QualityTier.Scavenged, 0.3m },
            { QualityTier.ClanForged, 0m },
            { QualityTier.Optimized, 0m },
            { QualityTier.MythForged, 0m }
        };
        var entry = DropProbabilityEntry.Create(EnemyDropClass.Trash, probs, 0.1m);

        // Act
        var str = entry.ToString();

        // Assert
        str.Should().Contain("Trash");
        str.Should().Contain("T0:");
        str.Should().Contain("T1:");
        str.Should().Contain("None:");
    }
}
