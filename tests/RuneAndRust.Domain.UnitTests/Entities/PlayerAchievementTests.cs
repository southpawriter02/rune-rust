// ═══════════════════════════════════════════════════════════════════════════════
// PlayerAchievementTests.cs
// Unit tests for PlayerAchievement entity and Player achievement methods.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the <see cref="PlayerAchievement"/> entity.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Factory creation and validation</description></item>
///   <item><description>Property initialization</description></item>
///   <item><description>ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class PlayerAchievementTests
{
    // ═══════════════════════════════════════════════════════════════
    // FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidData_ReturnsAchievement()
    {
        // Arrange & Act
        var achievement = PlayerAchievement.Create("first-blood", 10);

        // Assert
        achievement.Should().NotBeNull();
        achievement.Id.Should().NotBe(Guid.Empty);
        achievement.AchievementId.Should().Be("first-blood");
        achievement.PointsAwarded.Should().Be(10);
        achievement.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Create_WithNullAchievementId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PlayerAchievement.Create(null!, 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithEmptyAchievementId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PlayerAchievement.Create("", 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithWhitespaceAchievementId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => PlayerAchievement.Create("   ", 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WithNegativePoints_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => PlayerAchievement.Create("test", -5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_WithZeroPoints_Succeeds()
    {
        // Arrange & Act
        var achievement = PlayerAchievement.Create("test", 0);

        // Assert
        achievement.PointsAwarded.Should().Be(0);
    }

    [Test]
    public void Create_GeneratesUniqueIds()
    {
        // Arrange & Act
        var achievement1 = PlayerAchievement.Create("test-1", 10);
        var achievement2 = PlayerAchievement.Create("test-2", 25);

        // Assert
        achievement1.Id.Should().NotBe(achievement2.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // TO STRING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var achievement = PlayerAchievement.Create("monster-slayer", 25);

        // Act
        var result = achievement.ToString();

        // Assert
        result.Should().Contain("monster-slayer");
        result.Should().Contain("25 pts");
        result.Should().Contain("PlayerAchievement");
    }
}

/// <summary>
/// Unit tests for Player achievement collection methods.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>AddAchievement method</description></item>
///   <item><description>HasAchievement method</description></item>
///   <item><description>GetTotalAchievementPoints method</description></item>
///   <item><description>Achievements property</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class PlayerAchievementCollectionTests
{
    private Player _player = null!;

    [SetUp]
    public void SetUp()
    {
        _player = new Player("TestHero");
    }

    // ═══════════════════════════════════════════════════════════════
    // ADD ACHIEVEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void AddAchievement_WithValidData_AddsToCollection()
    {
        // Arrange & Act
        _player.AddAchievement("first-blood", 10);

        // Assert
        _player.Achievements.Should().HaveCount(1);
        _player.Achievements[0].AchievementId.Should().Be("first-blood");
        _player.Achievements[0].PointsAwarded.Should().Be(10);
    }

    [Test]
    public void AddAchievement_WithNullId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _player.AddAchievement(null!, 10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddAchievement_WhenAlreadyUnlocked_ThrowsInvalidOperationException()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);

        // Act
        var act = () => _player.AddAchievement("first-blood", 10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already unlocked*");
    }

    [Test]
    public void AddAchievement_MultipleAchievements_AddsAll()
    {
        // Arrange & Act
        _player.AddAchievement("first-blood", 10);
        _player.AddAchievement("monster-slayer", 25);
        _player.AddAchievement("boss-killer", 50);

        // Assert
        _player.Achievements.Should().HaveCount(3);
    }

    // ═══════════════════════════════════════════════════════════════
    // HAS ACHIEVEMENT TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HasAchievement_WhenUnlocked_ReturnsTrue()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);

        // Act
        var result = _player.HasAchievement("first-blood");

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasAchievement_WhenNotUnlocked_ReturnsFalse()
    {
        // Arrange & Act
        var result = _player.HasAchievement("first-blood");

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void HasAchievement_WithNullId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => _player.HasAchievement(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET TOTAL ACHIEVEMENT POINTS TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetTotalAchievementPoints_WithNoAchievements_ReturnsZero()
    {
        // Arrange & Act
        var result = _player.GetTotalAchievementPoints();

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void GetTotalAchievementPoints_WithMultipleAchievements_ReturnsSumOfPoints()
    {
        // Arrange
        _player.AddAchievement("first-blood", 10);       // Bronze = 10
        _player.AddAchievement("monster-slayer", 25);    // Silver = 25
        _player.AddAchievement("boss-killer", 50);       // Gold = 50

        // Act
        var result = _player.GetTotalAchievementPoints();

        // Assert
        result.Should().Be(85); // 10 + 25 + 50
    }
}
