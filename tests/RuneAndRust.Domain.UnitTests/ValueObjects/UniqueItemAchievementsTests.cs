namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="UniqueItemAchievements"/> value object.
/// </summary>
/// <remarks>
/// These tests verify:
/// <list type="bullet">
///   <item><description>Factory method creates with zero progress</description></item>
///   <item><description>RecordDrop updates collection state correctly</description></item>
///   <item><description>Progress calculations are accurate</description></item>
///   <item><description>Achievement earning is correctly determined</description></item>
///   <item><description>Duplicate tracking prevents re-counting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class UniqueItemAchievementsTests
{
    #region Create Tests

    /// <summary>
    /// Verifies that Create initializes with zero progress.
    /// </summary>
    [Test]
    public void Create_InitializesWithZeroProgress()
    {
        // Act
        var achievements = UniqueItemAchievements.Create(totalUniquesAvailable: 20);

        // Assert
        achievements.TotalUniquesFound.Should().Be(0);
        achievements.TotalUniquesAvailable.Should().Be(20);
        achievements.CollectionProgress.Should().Be(0m);
        achievements.FirstMythForgedAt.Should().BeNull();
        achievements.LastDropAt.Should().BeNull();
        achievements.HasFoundAny.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Create throws for negative total available.
    /// </summary>
    [Test]
    public void Create_WithNegativeTotal_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => UniqueItemAchievements.Create(totalUniquesAvailable: -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("totalUniquesAvailable");
    }

    #endregion

    #region RecordDrop Tests

    /// <summary>
    /// Verifies that first drop sets timestamps correctly.
    /// </summary>
    [Test]
    public void RecordDrop_FirstDrop_SetsTimestamps()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        var dropTime = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var isNew = achievements.RecordDrop("sword-of-legends", new[] { "warrior" }, dropTime);

        // Assert
        isNew.Should().BeTrue();
        achievements.TotalUniquesFound.Should().Be(1);
        achievements.FirstMythForgedAt.Should().Be(dropTime);
        achievements.LastDropAt.Should().Be(dropTime);
        achievements.HasFoundAny.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that duplicate drops return false.
    /// </summary>
    [Test]
    public void RecordDrop_Duplicate_ReturnsFalse()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("sword-of-legends", new[] { "warrior" });

        // Act
        var isNew = achievements.RecordDrop("sword-of-legends", new[] { "warrior" });

        // Assert
        isNew.Should().BeFalse();
        achievements.TotalUniquesFound.Should().Be(1); // Still 1, not 2
    }

    /// <summary>
    /// Verifies that ID comparison is case-insensitive.
    /// </summary>
    [Test]
    public void RecordDrop_CaseInsensitive_DetectsDuplicate()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("Sword-Of-Legends", new[] { "warrior" });

        // Act
        var isNew = achievements.RecordDrop("SWORD-OF-LEGENDS", new[] { "warrior" });

        // Assert
        isNew.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that class counts are updated correctly.
    /// </summary>
    [Test]
    public void RecordDrop_UpdatesClassCounts()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);

        // Act
        achievements.RecordDrop("warrior-sword", new[] { "warrior", "skirmisher" });
        achievements.RecordDrop("warrior-shield", new[] { "warrior" });

        // Assert
        achievements.UniquesByClass.Should().ContainKey("warrior");
        achievements.UniquesByClass["warrior"].Should().Be(2);
        achievements.UniquesByClass["skirmisher"].Should().Be(1);
    }

    #endregion

    #region CollectionProgress Tests

    /// <summary>
    /// Verifies that CollectionProgress calculates correctly.
    /// </summary>
    [Test]
    public void CollectionProgress_CalculatesCorrectly()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(10);
        achievements.RecordDrop("item1", Array.Empty<string>());
        achievements.RecordDrop("item2", Array.Empty<string>());

        // Act
        var progress = achievements.CollectionProgress;

        // Assert
        progress.Should().Be(0.2m); // 2/10 = 0.2
    }

    /// <summary>
    /// Verifies that CollectionProgress returns 0 when no items available.
    /// </summary>
    [Test]
    public void CollectionProgress_ZeroAvailable_ReturnsZero()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(0);

        // Act
        var progress = achievements.CollectionProgress;

        // Assert
        progress.Should().Be(0m);
    }

    #endregion

    #region GetProgressToward Tests

    /// <summary>
    /// Verifies progress toward CollectorBronze calculates correctly.
    /// </summary>
    [Test]
    public void GetProgressToward_CollectorBronze_CalculatesCorrectly()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("item1", Array.Empty<string>());
        achievements.RecordDrop("item2", Array.Empty<string>());
        achievements.RecordDrop("item3", Array.Empty<string>());

        // Act
        var progress = achievements.GetProgressToward(UniqueAchievementType.CollectorBronze);

        // Assert
        progress.Should().Be(0.6m); // 3/5 = 0.6
    }

    /// <summary>
    /// Verifies that progress is capped at 1.0.
    /// </summary>
    [Test]
    public void GetProgressToward_ExceedsRequired_CapsAtOne()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        for (int i = 1; i <= 10; i++)
        {
            achievements.RecordDrop($"item{i}", Array.Empty<string>());
        }

        // Act
        var progress = achievements.GetProgressToward(UniqueAchievementType.CollectorBronze);

        // Assert
        progress.Should().Be(1.0m); // 10 items, but capped at 1.0
    }

    #endregion

    #region IsAchievementEarned Tests

    /// <summary>
    /// Verifies FirstMythForged is earned after first drop.
    /// </summary>
    [Test]
    public void IsAchievementEarned_FirstMythForged_TrueAfterFirstDrop()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("first-item", Array.Empty<string>());

        // Act
        var earned = achievements.IsAchievementEarned(UniqueAchievementType.FirstMythForged);

        // Assert
        earned.Should().BeTrue();
    }

    /// <summary>
    /// Verifies FirstMythForged is not earned before any drops.
    /// </summary>
    [Test]
    public void IsAchievementEarned_FirstMythForged_FalseBeforeDrop()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);

        // Act
        var earned = achievements.IsAchievementEarned(UniqueAchievementType.FirstMythForged);

        // Assert
        earned.Should().BeFalse();
    }

    /// <summary>
    /// Verifies CollectorBronze is earned at 5 items.
    /// </summary>
    [Test]
    public void IsAchievementEarned_CollectorBronze_TrueAtFiveItems()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        for (int i = 1; i <= 5; i++)
        {
            achievements.RecordDrop($"item{i}", Array.Empty<string>());
        }

        // Act
        var earned = achievements.IsAchievementEarned(UniqueAchievementType.CollectorBronze);

        // Assert
        earned.Should().BeTrue();
    }

    #endregion

    #region GetEarnedAchievements Tests

    /// <summary>
    /// Verifies GetEarnedAchievements includes FirstMythForged after first drop.
    /// </summary>
    [Test]
    public void GetEarnedAchievements_IncludesFirstMythForged()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("first-item", Array.Empty<string>());

        // Act
        var earned = achievements.GetEarnedAchievements();

        // Assert
        earned.Should().Contain(UniqueAchievementType.FirstMythForged);
    }

    /// <summary>
    /// Verifies GetEarnedAchievements returns empty list when no drops.
    /// </summary>
    [Test]
    public void GetEarnedAchievements_NoDrops_ReturnsEmptyList()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);

        // Act
        var earned = achievements.GetEarnedAchievements();

        // Assert
        earned.Should().BeEmpty();
    }

    #endregion

    #region HasFound Tests

    /// <summary>
    /// Verifies HasFound returns true for found items.
    /// </summary>
    [Test]
    public void HasFound_FoundItem_ReturnsTrue()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("sword-of-legends", Array.Empty<string>());

        // Act
        var found = achievements.HasFound("sword-of-legends");

        // Assert
        found.Should().BeTrue();
    }

    /// <summary>
    /// Verifies HasFound is case-insensitive.
    /// </summary>
    [Test]
    public void HasFound_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("sword-of-legends", Array.Empty<string>());

        // Act
        var found = achievements.HasFound("SWORD-OF-LEGENDS");

        // Assert
        found.Should().BeTrue();
    }

    #endregion

    #region ToString Tests

    /// <summary>
    /// Verifies that ToString returns a useful representation.
    /// </summary>
    [Test]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var achievements = UniqueItemAchievements.Create(20);
        achievements.RecordDrop("item1", Array.Empty<string>());

        // Act
        var result = achievements.ToString();

        // Assert
        result.Should().Contain("UniqueItemAchievements");
        result.Should().Contain("1/20");
    }

    #endregion
}
