using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.UnitTests.Definitions;

/// <summary>
/// Unit tests for AbilityDefinition extended range properties (v0.5.1b).
/// </summary>
[TestFixture]
public class AbilityExtendedRangeTests
{
    [Test]
    public void GetOptimalRange_WhenSet_ReturnsConfiguredValue()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "fireball",
            Name = "Fireball",
            Range = 8,
            RangeType = RangeType.Ranged,
            OptimalRange = 5
        };

        // Act
        var optimal = ability.GetOptimalRange();

        // Assert
        optimal.Should().Be(5);
    }

    [Test]
    public void GetOptimalRange_WhenNull_ReturnsRange()
    {
        // Arrange - most spells have no penalty so optimal = range
        var ability = new AbilityDefinition
        {
            Id = "fireball",
            Name = "Fireball",
            Range = 8,
            RangeType = RangeType.Ranged
        };

        // Act
        var optimal = ability.GetOptimalRange();

        // Assert
        optimal.Should().Be(8);
    }

    [Test]
    public void GetPenaltyAtDistance_ZeroPenalty_AlwaysZero()
    {
        // Arrange - no penalty configured
        var ability = new AbilityDefinition
        {
            Id = "fireball",
            Name = "Fireball",
            Range = 8,
            RangeType = RangeType.Ranged,
            RangePenalty = 0
        };

        // Act
        var penalty = ability.GetPenaltyAtDistance(10);

        // Assert
        penalty.Should().Be(0);
    }

    [Test]
    public void GetPenaltyAtDistance_WithPenalty_CalculatesCorrectly()
    {
        // Arrange
        var ability = new AbilityDefinition
        {
            Id = "longshot",
            Name = "Long Shot",
            Range = 10,
            RangeType = RangeType.Ranged,
            OptimalRange = 5,
            RangePenalty = 2
        };

        // Act - distance 8 = 3 cells beyond optimal
        var penalty = ability.GetPenaltyAtDistance(8);

        // Assert
        penalty.Should().Be(6); // 3 * 2
    }
}
