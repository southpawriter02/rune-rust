using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Application.ValueObjects;

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Tests for the SearchResult value object.
/// </summary>
[TestFixture]
public class SearchResultTests
{
    [Test]
    public void NothingHidden_CreatesResultWithNothingToFindTrue()
    {
        // Act
        var result = SearchResult.NothingHidden();

        // Assert
        result.NothingToFind.Should().BeTrue();
        result.SkillCheck.Should().BeNull();
        result.DiscoveredExits.Should().BeEmpty();
        result.DiscoveredItems.Should().BeEmpty();
        result.IsSuccess.Should().BeTrue();
        result.FoundSomething.Should().BeFalse();
    }

    [Test]
    public void Failed_CreatesResultWithFailedCheck()
    {
        // Arrange
        var mockCheck = CreateMockSkillCheckResult(8, 12);

        // Act
        var result = SearchResult.Failed(mockCheck);

        // Assert
        result.NothingToFind.Should().BeFalse();
        result.SkillCheck.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.FoundSomething.Should().BeFalse();
    }

    [Test]
    public void Success_CreatesResultWithDiscoveries()
    {
        // Arrange
        var mockCheck = CreateMockSkillCheckResult(15, 12);
        var exits = new List<Direction> { Direction.East };
        var items = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = SearchResult.Success(mockCheck, exits, items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.FoundSomething.Should().BeTrue();
        result.DiscoveredExits.Should().HaveCount(1);
        result.DiscoveredItems.Should().HaveCount(1);
    }

    [Test]
    public void FoundSomething_TrueWhenExitsDiscovered()
    {
        // Arrange
        var mockCheck = CreateMockSkillCheckResult(15, 12);
        var exits = new List<Direction> { Direction.North };

        // Act
        var result = SearchResult.Success(mockCheck, exits, Array.Empty<Guid>());

        // Assert
        result.FoundSomething.Should().BeTrue();
    }

    private static SkillCheckResult CreateMockSkillCheckResult(int total, int dc)
    {
        // Parse a simple dice pool for the test
        var dicePool = DicePool.Parse("1d10");
        var diceValue = total - 12; // Subtract attribute bonus
        
        // Create a dice roll result with the appropriate values
        var diceResult = new DiceRollResult(
            pool: dicePool,
            rolls: [diceValue > 0 ? diceValue : 1],
            total: diceValue > 0 ? diceValue : 1);

        return new SkillCheckResult(
            skillId: "perception",
            skillName: "Perception",
            diceResult: diceResult,
            attributeBonus: 12,
            otherBonus: 0,
            difficultyClass: dc,
            difficultyName: "Moderate");
    }
}
