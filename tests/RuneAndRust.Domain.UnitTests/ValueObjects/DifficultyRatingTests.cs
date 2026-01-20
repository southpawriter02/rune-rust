using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Tests for DifficultyRating value object.
/// </summary>
[TestFixture]
public class DifficultyRatingTests
{
    [Test]
    public void Calculate_AtStartPosition_ReturnsBaseDifficulty()
    {
        // Arrange
        var position = new Position3D(0, 0, 0);
        var startPosition = new Position3D(0, 0, 0);
        var rules = new ScalingRules { BaseDifficulty = 5 };

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Standard, rules);

        // Assert
        result.Level.Should().Be(5);
        result.DepthFactor.Should().Be(0);
        result.DistanceFactor.Should().Be(0);
    }

    [Test]
    public void Calculate_DeeperPosition_ReturnsHigherDifficulty()
    {
        // Arrange
        var position = new Position3D(0, 0, -3); // 3 levels deep
        var startPosition = new Position3D(0, 0, 0);
        var rules = new ScalingRules { BaseDifficulty = 5, DepthMultiplier = 10.0f, DistanceMultiplier = 0.0f };

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Standard, rules);

        // Assert
        result.Level.Should().Be(35); // 5 + (3 * 10) = 35
        result.DepthFactor.Should().Be(3);
    }

    [Test]
    public void Calculate_FartherPosition_ReturnsHigherDifficulty()
    {
        // Arrange
        var position = new Position3D(5, 5, 0);
        var startPosition = new Position3D(0, 0, 0);
        var rules = new ScalingRules { BaseDifficulty = 5, DistanceMultiplier = 0.5f };

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Standard, rules);

        // Assert
        result.Level.Should().Be(10); // 5 + (10 * 0.5) = 10
        result.DistanceFactor.Should().Be(10);
    }

    [Test]
    public void Calculate_TreasureRoom_AppliesModifier()
    {
        // Arrange
        var position = new Position3D(0, 0, -2);
        var startPosition = new Position3D(0, 0, 0);
        var rules = new ScalingRules { BaseDifficulty = 5, DepthMultiplier = 10.0f, DistanceMultiplier = 0.0f, TreasureRoomModifier = 1.5f };

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Treasure, rules);

        // Assert
        result.Level.Should().Be(25); // 5 + (2 * 10) = 25
        result.RoomTypeModifier.Should().Be(1.5f);
        result.EffectiveLevel.Should().Be(37); // 25 * 1.5 = 37.5 -> 37
    }

    [Test]
    public void Calculate_SafeRoom_ReturnsZeroEffective()
    {
        // Arrange
        var position = new Position3D(0, 0, -5);
        var startPosition = new Position3D(0, 0, 0);

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Safe);

        // Assert
        result.RoomTypeModifier.Should().Be(0.0f);
        result.EffectiveLevel.Should().Be(0);
        result.HasCombat.Should().BeFalse();
    }

    [Test]
    public void EffectiveLevel_ClampedTo100Maximum()
    {
        // Arrange
        var position = new Position3D(0, 0, -10);
        var startPosition = new Position3D(0, 0, 0);
        var rules = new ScalingRules { BaseDifficulty = 95, DepthMultiplier = 10.0f, BossRoomModifier = 2.0f };

        // Act
        var result = DifficultyRating.Calculate(position, startPosition, RoomType.Boss, rules);

        // Assert
        result.EffectiveLevel.Should().BeLessThanOrEqualTo(100);
    }

    [Test]
    public void SuggestedMonsterTier_ReturnsCorrectTierForLevel()
    {
        // Arrange & Act/Assert
        new DifficultyRating { Level = 10, RoomTypeModifier = 1.0f }.SuggestedMonsterTier.Should().Be("common");
        new DifficultyRating { Level = 20, RoomTypeModifier = 1.0f }.SuggestedMonsterTier.Should().Be("named");
        new DifficultyRating { Level = 45, RoomTypeModifier = 1.0f }.SuggestedMonsterTier.Should().Be("elite");
        new DifficultyRating { Level = 75, RoomTypeModifier = 1.0f }.SuggestedMonsterTier.Should().Be("boss");
        new DifficultyRating { Level = 50, RoomTypeModifier = 0.0f }.SuggestedMonsterTier.Should().Be("none");
    }

    [Test]
    public void LootQualityMultiplier_ScalesWithLevel()
    {
        // Arrange
        var lowDifficulty = new DifficultyRating { Level = 10, RoomTypeModifier = 1.0f };
        var highDifficulty = new DifficultyRating { Level = 50, RoomTypeModifier = 1.0f };

        // Assert
        lowDifficulty.LootQualityMultiplier.Should().BeApproximately(1.2f, 0.01f); // 1.0 + (10 * 0.02)
        highDifficulty.LootQualityMultiplier.Should().BeApproximately(2.0f, 0.01f); // 1.0 + (50 * 0.02)
    }

    [Test]
    public void MonsterLevelBonus_ScalesWithLevel()
    {
        // Arrange & Assert
        new DifficultyRating { Level = 15, RoomTypeModifier = 1.0f }.MonsterLevelBonus.Should().Be(1);
        new DifficultyRating { Level = 35, RoomTypeModifier = 1.0f }.MonsterLevelBonus.Should().Be(3);
        new DifficultyRating { Level = 50, RoomTypeModifier = 1.0f }.MonsterLevelBonus.Should().Be(5);
    }

    [Test]
    public void Starting_ReturnsDefaultDifficulty()
    {
        // Act
        var result = DifficultyRating.Starting;

        // Assert
        result.Level.Should().Be(1);
        result.DepthFactor.Should().Be(0);
        result.DistanceFactor.Should().Be(0);
        result.RoomTypeModifier.Should().Be(1.0f);
        result.EffectiveLevel.Should().Be(1);
    }
}
