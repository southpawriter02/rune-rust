using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for ContentPlacementService.
/// </summary>
[TestFixture]
public class ContentPlacementServiceTests
{
    private ContentPlacementService _service = null!;
    private SeededRandomService _random = null!;
    private const int TestSeed = 12345;

    [SetUp]
    public void SetUp()
    {
        _random = new SeededRandomService(TestSeed, NullLogger<SeededRandomService>.Instance);
        _service = new ContentPlacementService(_random, NullLogger<ContentPlacementService>.Instance);
    }

    [Test]
    public void ShouldFillSlot_RequiredSlot_ReturnsTrue()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01", required: true);
        var position = new Position3D(0, 0, 0);

        // Act
        var result = _service.ShouldFillSlot(slot, position);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ShouldFillSlot_OptionalSlot_RespectsProbability()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01", probability: 0.5f);
        var filledCount = 0;

        // Act - Test with different positions
        for (int i = 0; i < 100; i++)
        {
            var position = new Position3D(i, 0, 0);
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new ContentPlacementService(random, NullLogger<ContentPlacementService>.Instance);

            if (service.ShouldFillSlot(slot, position))
                filledCount++;
        }

        // Assert - Should be roughly 50% with some variance
        filledCount.Should().BeInRange(30, 70);
    }

    [Test]
    public void SelectMonsterTier_SafeRoom_ReturnsNone()
    {
        // Arrange
        var difficulty = new DifficultyRating { Level = 50, RoomTypeModifier = 0.0f };
        var slot = TemplateSlot.Monster("monster_01");
        var position = new Position3D(0, 0, 0);

        // Act
        var result = _service.SelectMonsterTier(difficulty, slot, position);

        // Assert
        result.Should().Be("none");
    }

    [Test]
    public void SelectMonsterTier_HigherDifficulty_HigherTiersMoreLikely()
    {
        // Arrange
        var lowDifficulty = new DifficultyRating { Level = 10, RoomTypeModifier = 1.0f };
        var highDifficulty = new DifficultyRating { Level = 80, RoomTypeModifier = 1.0f };
        var slot = TemplateSlot.Monster("monster_01");

        // Count elite/boss selections for each difficulty
        var lowTierHighCount = 0;
        var highTierHighCount = 0;

        for (int i = 0; i < 100; i++)
        {
            var position = new Position3D(i, i, 0);
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new ContentPlacementService(random, NullLogger<ContentPlacementService>.Instance);

            var lowTier = service.SelectMonsterTier(lowDifficulty, slot, position);
            var highTier = service.SelectMonsterTier(highDifficulty, slot, position);

            if (lowTier is "elite" or "boss") lowTierHighCount++;
            if (highTier is "elite" or "boss") highTierHighCount++;
        }

        // Assert - Higher difficulty should have more elite/boss
        highTierHighCount.Should().BeGreaterThan(lowTierHighCount);
    }

    [Test]
    public void SelectMonsterTier_RespectsMaxTierConstraint()
    {
        // Arrange
        var difficulty = new DifficultyRating { Level = 80, RoomTypeModifier = 1.0f };
        var constraints = new Dictionary<string, string> { ["maxTier"] = "named" };
        var slot = TemplateSlot.Monster("monster_01", constraints: constraints);

        // Act - Check many selections
        for (int i = 0; i < 50; i++)
        {
            var position = new Position3D(i, 0, 0);
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new ContentPlacementService(random, NullLogger<ContentPlacementService>.Instance);

            var tier = service.SelectMonsterTier(difficulty, slot, position);

            // Assert
            tier.Should().BeOneOf("common", "named");
        }
    }

    [Test]
    public void DetermineMonsterQuantity_ReturnsValueInRange()
    {
        // Arrange
        var slot = TemplateSlot.Monster("monster_01", min: 2, max: 5);
        var position = new Position3D(5, 5, 0);

        // Act
        var result = _service.DetermineMonsterQuantity(slot, position);

        // Assert
        result.Should().BeInRange(2, 5);
    }

    [Test]
    public void SelectItemRarity_HigherDifficulty_BetterRaritiesMoreLikely()
    {
        // Arrange
        var lowDifficulty = new DifficultyRating { Level = 10, RoomTypeModifier = 1.0f };
        var highDifficulty = new DifficultyRating { Level = 80, RoomTypeModifier = 1.0f };
        var slot = TemplateSlot.Item("item_01");

        var lowRareCount = 0;
        var highRareCount = 0;

        for (int i = 0; i < 100; i++)
        {
            var position = new Position3D(i, i, 0);
            var random = new SeededRandomService(i, NullLogger<SeededRandomService>.Instance);
            var service = new ContentPlacementService(random, NullLogger<ContentPlacementService>.Instance);

            var lowRarity = service.SelectItemRarity(lowDifficulty, slot, position);
            var highRarity = service.SelectItemRarity(highDifficulty, slot, position);

            if (lowRarity is "rare" or "epic" or "legendary") lowRareCount++;
            if (highRarity is "rare" or "epic" or "legendary") highRareCount++;
        }

        // Assert
        highRareCount.Should().BeGreaterThan(lowRareCount);
    }

    [Test]
    public void DetermineItemQuantity_ReturnsValueInRange()
    {
        // Arrange
        var slot = TemplateSlot.Item("item_01", min: 1, max: 3);
        var position = new Position3D(3, 3, 0);

        // Act
        var result = _service.DetermineItemQuantity(slot, position);

        // Assert
        result.Should().BeInRange(1, 3);
    }

    [Test]
    public void GetMonsterLevelBonus_ReturnsCorrectBonus()
    {
        // Arrange
        var difficulty = new DifficultyRating { Level = 35, RoomTypeModifier = 1.0f };

        // Act
        var result = _service.GetMonsterLevelBonus(difficulty);

        // Assert
        result.Should().Be(3); // 35 / 10 = 3
    }
}
