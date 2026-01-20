// ═══════════════════════════════════════════════════════════════════════════════
// AchievementProgressTests.cs
// Unit tests for AchievementProgress and ConditionProgress records.
// Version: 0.12.1b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Application.Models;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.DTOs;

/// <summary>
/// Unit tests for <see cref="AchievementProgress"/> record.
/// </summary>
[TestFixture]
public class AchievementProgressTests
{
    private AchievementDefinition _definition = null!;

    [SetUp]
    public void SetUp()
    {
        var conditions = new List<AchievementCondition>
        {
            new("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100)
        };

        _definition = AchievementDefinition.Create(
            "monster-slayer",
            "Monster Slayer",
            "Defeat 100 monsters",
            AchievementCategory.Combat,
            AchievementTier.Silver,
            conditions);
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void IsPartiallyComplete_WhenHasProgressButNotUnlocked_ReturnsTrue()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>
        {
            new(new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100),
                50, 100, 0.5, false)
        };
        var progress = new AchievementProgress(_definition, false, conditionProgress, 0.5);

        // Act & Assert
        progress.IsPartiallyComplete.Should().BeTrue();
    }

    [Test]
    public void IsPartiallyComplete_WhenUnlocked_ReturnsFalse()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>();
        var progress = new AchievementProgress(_definition, true, conditionProgress, 1.0);

        // Act & Assert
        progress.IsPartiallyComplete.Should().BeFalse();
    }

    [Test]
    public void IsPartiallyComplete_WhenNoProgress_ReturnsFalse()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>
        {
            new(new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100),
                0, 100, 0.0, false)
        };
        var progress = new AchievementProgress(_definition, false, conditionProgress, 0.0);

        // Act & Assert
        progress.IsPartiallyComplete.Should().BeFalse();
    }

    [Test]
    public void ProgressPercent_ReturnsConvertedPercentage()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>();
        var progress = new AchievementProgress(_definition, false, conditionProgress, 0.67);

        // Act & Assert
        progress.ProgressPercent.Should().Be(67);
    }

    [Test]
    public void DisplayName_WhenUnlocked_ReturnsActualName()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>();
        var progress = new AchievementProgress(_definition, true, conditionProgress, 1.0);

        // Act & Assert
        progress.DisplayName.Should().Be("Monster Slayer");
    }

    [Test]
    public void DisplayDescription_WhenUnlocked_ReturnsActualDescription()
    {
        // Arrange
        var conditionProgress = new List<ConditionProgress>();
        var progress = new AchievementProgress(_definition, true, conditionProgress, 1.0);

        // Act & Assert
        progress.DisplayDescription.Should().Be("Defeat 100 monsters");
    }

    [Test]
    public void DisplayName_SecretAchievementWhenLocked_ReturnsHidden()
    {
        // Arrange
        var secretDef = AchievementDefinition.Create(
            "secret",
            "Secret Name",
            "Secret Description",
            AchievementCategory.Secret,
            AchievementTier.Gold,
            new List<AchievementCondition>(),
            isSecret: true);

        var conditionProgress = new List<ConditionProgress>();
        var progress = new AchievementProgress(secretDef, false, conditionProgress, 0.0);

        // Act & Assert
        progress.DisplayName.Should().Be("???");
        progress.DisplayDescription.Should().Contain("Hidden");
    }
}

/// <summary>
/// Unit tests for <see cref="ConditionProgress"/> record.
/// </summary>
[TestFixture]
public class ConditionProgressTests
{
    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProgressPercent_ReturnsConvertedPercentage()
    {
        // Arrange
        var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
        var progress = new ConditionProgress(condition, 75, 100, 0.75, false);

        // Act & Assert
        progress.ProgressPercent.Should().Be(75);
    }

    [Test]
    public void Description_ReturnsConditionDescription()
    {
        // Arrange
        var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
        var progress = new ConditionProgress(condition, 50, 100, 0.5, false);

        // Act & Assert
        progress.Description.Should().Be("monstersKilled >= 100");
    }

    [Test]
    public void ProgressDisplay_ReturnsCurrentSlashTarget()
    {
        // Arrange
        var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
        var progress = new ConditionProgress(condition, 82, 100, 0.82, false);

        // Act & Assert
        progress.ProgressDisplay.Should().Be("82/100");
    }

    [Test]
    public void IsMet_WhenConditionMet_ReturnsTrue()
    {
        // Arrange
        var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
        var progress = new ConditionProgress(condition, 100, 100, 1.0, true);

        // Act & Assert
        progress.IsMet.Should().BeTrue();
    }

    [Test]
    public void IsMet_WhenConditionNotMet_ReturnsFalse()
    {
        // Arrange
        var condition = new AchievementCondition("monstersKilled", ComparisonOperator.GreaterThanOrEqual, 100);
        var progress = new ConditionProgress(condition, 50, 100, 0.5, false);

        // Act & Assert
        progress.IsMet.Should().BeFalse();
    }
}
