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
        // Arrange - roll of 3 on d10 = 0 successes, DC 1 → failure
        var mockCheck = CreateFailedSkillCheckResult();

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
        // Arrange - roll of 9 on d10 = 1 success, DC 1 → marginal success
        var mockCheck = CreateSuccessfulSkillCheckResult();
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
        var mockCheck = CreateSuccessfulSkillCheckResult();
        var exits = new List<Direction> { Direction.North };

        // Act
        var result = SearchResult.Success(mockCheck, exits, Array.Empty<Guid>());

        // Assert
        result.FoundSomething.Should().BeTrue();
    }

    /// <summary>
    /// Creates a SkillCheckResult that succeeds using success-counting mechanics.
    /// Rolling a 9 on 1d10 produces 1 net success (9 >= 8 threshold), beating DC 1.
    /// </summary>
    private static SkillCheckResult CreateSuccessfulSkillCheckResult()
    {
        var dicePool = DicePool.Parse("1d10");
        var diceResult = new DiceRollResult(pool: dicePool, rolls: [9]);

        return new SkillCheckResult(
            skillId: "perception",
            skillName: "Perception",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");
    }

    /// <summary>
    /// Creates a SkillCheckResult that fails using success-counting mechanics.
    /// Rolling a 3 on 1d10 produces 0 net successes (3 &lt; 8 threshold), failing DC 1.
    /// </summary>
    private static SkillCheckResult CreateFailedSkillCheckResult()
    {
        var dicePool = DicePool.Parse("1d10");
        var diceResult = new DiceRollResult(pool: dicePool, rolls: [3]);

        return new SkillCheckResult(
            skillId: "perception",
            skillName: "Perception",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");
    }
}
