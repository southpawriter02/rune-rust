// ═══════════════════════════════════════════════════════════════════════════════
// AchievementServiceTests.cs
// Unit tests for AchievementService.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="AchievementService"/> class.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Achievement checking and unlocking</description></item>
///   <item><description>Progress calculation</description></item>
///   <item><description>Query methods</description></item>
///   <item><description>Edge cases and validation</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class AchievementServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST SETUP
    // ═══════════════════════════════════════════════════════════════

    private IAchievementProvider _achievementProvider = null!;
    private IStatisticsService _statisticsService = null!;
    private IDiceHistoryService _diceHistoryService = null!;
    private IGameRenderer _gameRenderer = null!;
    private ILogger<AchievementService> _logger = null!;
    private AchievementService _service = null!;
    private Player _player = null!;
    private PlayerStatistics _statistics = null!;
    private DiceRollHistory _diceHistory = null!;

    [SetUp]
    public void SetUp()
    {
        _achievementProvider = Substitute.For<IAchievementProvider>();
        _statisticsService = Substitute.For<IStatisticsService>();
        _diceHistoryService = Substitute.For<IDiceHistoryService>();
        _gameRenderer = Substitute.For<IGameRenderer>();
        _logger = Substitute.For<ILogger<AchievementService>>();

        _player = new Player("TestHero");
        _statistics = PlayerStatistics.Create(_player.Id);
        _diceHistory = DiceRollHistory.Create(_player.Id);

        _statisticsService.GetPlayerStatistics(_player).Returns(_statistics);
        _diceHistoryService.GetHistory(_player).Returns(_diceHistory);
        _achievementProvider.GetAchievementCount().Returns(0);

        _service = new AchievementService(
            _achievementProvider,
            _statisticsService,
            _diceHistoryService,
            _gameRenderer,
            _logger);
    }

    // ═══════════════════════════════════════════════════════════════
    // CHECK ACHIEVEMENTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CheckAchievements_WhenConditionsMet_UnlocksAchievement()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        var achievement = AchievementDefinition.Create(
            "first-blood", "First Blood", "Kill a monster",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });

        // Set player stats to meet condition
        _statistics.RecordMonsterKill("goblin");

        // Act
        var unlocked = _service.CheckAchievements(_player);

        // Assert
        unlocked.Should().Contain("first-blood");
        _player.HasAchievement("first-blood").Should().BeTrue();
    }

    [Test]
    public void CheckAchievements_WhenConditionsNotMet_DoesNotUnlock()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100)
        };

        var achievement = AchievementDefinition.Create(
            "monster-slayer", "Monster Slayer", "Kill 100 monsters",
            AchievementCategory.Combat, AchievementTier.Silver, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });

        // Stats have 0 kills

        // Act
        var unlocked = _service.CheckAchievements(_player);

        // Assert
        unlocked.Should().BeEmpty();
        _player.HasAchievement("monster-slayer").Should().BeFalse();
    }

    [Test]
    public void CheckAchievements_WhenAlreadyUnlocked_SkipsAchievement()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        var achievement = AchievementDefinition.Create(
            "first-blood", "First Blood", "Kill a monster",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });
        _statistics.RecordMonsterKill("goblin");

        // Pre-unlock the achievement
        _player.AddAchievement("first-blood", 10);

        // Act
        var unlocked = _service.CheckAchievements(_player);

        // Assert
        unlocked.Should().BeEmpty();
    }

    [Test]
    public void CheckAchievements_WithMultipleConditions_RequiresAllMet()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 10),
            new("bossesKilled", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        var achievement = AchievementDefinition.Create(
            "veteran", "Veteran", "Kill 10 monsters and a boss",
            AchievementCategory.Combat, AchievementTier.Silver, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });

        // Only one condition met
        for (int i = 0; i < 15; i++)
        {
            _statistics.RecordMonsterKill("goblin");
        }

        // Act
        var unlocked = _service.CheckAchievements(_player);

        // Assert
        unlocked.Should().BeEmpty(); // Need boss kill too
    }

    // ═══════════════════════════════════════════════════════════════
    // QUERY METHODS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetUnlockedAchievements_ReturnsPlayerAchievements()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);
        _player.AddAchievement("monster-slayer", 25);

        // Act
        var unlocked = _service.GetUnlockedAchievements(_player);

        // Assert
        unlocked.Should().HaveCount(2);
    }

    [Test]
    public void GetTotalPoints_ReturnsSumOfAchievementPoints()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);
        _player.AddAchievement("monster-slayer", 25);

        // Act
        var points = _service.GetTotalPoints(_player);

        // Assert
        points.Should().Be(35);
    }

    [Test]
    public void IsUnlocked_WhenAchievementUnlocked_ReturnsTrue()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);

        // Act
        var result = _service.IsUnlocked(_player, "first-blood");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsUnlocked_WhenAchievementNotUnlocked_ReturnsFalse()
    {
        // Arrange & Act
        var result = _service.IsUnlocked(_player, "first-blood");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetUnlockedCount_ReturnsCountOfUnlockedAchievements()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);
        _player.AddAchievement("monster-slayer", 25);
        _player.AddAchievement("boss-killer", 50);

        // Act
        var count = _service.GetUnlockedCount(_player);

        // Assert
        count.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // PROGRESS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetProgress_CalculatesProgressForAllAchievements()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100)
        };

        var achievement = AchievementDefinition.Create(
            "monster-slayer", "Monster Slayer", "Kill 100 monsters",
            AchievementCategory.Combat, AchievementTier.Silver, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });

        // Player has 50 kills
        for (int i = 0; i < 50; i++)
        {
            _statistics.RecordMonsterKill("goblin");
        }

        // Act
        var progress = _service.GetProgress(_player);

        // Assert
        progress.Should().HaveCount(1);
        progress[0].OverallProgress.Should().BeApproximately(0.5, 0.01);
        progress[0].IsUnlocked.Should().BeFalse();
    }

    [Test]
    public void GetProgress_ForUnlockedAchievement_ReturnsFullProgress()
    {
        // Arrange
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        var achievement = AchievementDefinition.Create(
            "first-blood", "First Blood", "Kill a monster",
            AchievementCategory.Combat, AchievementTier.Bronze, conditions);

        _achievementProvider.GetAllAchievements().Returns(new[] { achievement });
        _player.AddAchievement("first-blood", 10);

        // Act
        var progress = _service.GetProgress(_player);

        // Assert
        progress[0].OverallProgress.Should().Be(1.0);
        progress[0].IsUnlocked.Should().BeTrue();
    }

    [Test]
    public void GetProgressByCategory_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var combatConditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 1)
        };

        var explorationConditions = new List<AchievementCondition>
        {
            new("roomsDiscovered", ComparisonOperator.GreaterThanOrEqual, 10)
        };

        var combatAchievement = AchievementDefinition.Create(
            "first-blood", "First Blood", "Kill a monster",
            AchievementCategory.Combat, AchievementTier.Bronze, combatConditions);

        var explorationAchievement = AchievementDefinition.Create(
            "explorer", "Explorer", "Discover 10 rooms",
            AchievementCategory.Exploration, AchievementTier.Bronze, explorationConditions);

        _achievementProvider.GetAchievementsByCategory(AchievementCategory.Combat)
            .Returns(new[] { combatAchievement });

        // Act
        var progress = _service.GetProgressByCategory(_player, AchievementCategory.Combat);

        // Assert
        progress.Should().HaveCount(1);
        progress[0].Definition.Category.Should().Be(AchievementCategory.Combat);
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CheckAchievements_WithNullPlayer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _service.CheckAchievements(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsUnlocked_WithNullAchievementId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _service.IsUnlocked(_player, null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
