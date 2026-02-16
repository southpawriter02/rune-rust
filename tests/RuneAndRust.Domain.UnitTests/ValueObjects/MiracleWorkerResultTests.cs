using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="MiracleWorkerResult"/>.
/// Validates computed properties, condition tracking, and display methods
/// for the Bone-Setter Capstone ability result.
/// </summary>
[TestFixture]
public class MiracleWorkerResultTests
{
    [Test]
    public void HpAfter_EqualsMaxHp()
    {
        // Arrange & Act
        var result = new MiracleWorkerResult
        {
            HpBefore = 15,
            MaxHp = 100
        };

        // Assert — Miracle Worker always restores to max HP
        result.HpAfter.Should().Be(100);
    }

    [Test]
    public void TotalHealing_CalculatesCorrectly()
    {
        // Arrange & Act
        var result = new MiracleWorkerResult
        {
            HpBefore = 25,
            MaxHp = 80
        };

        // Assert — TotalHealing = MaxHp - HpBefore
        result.TotalHealing.Should().Be(55); // 80 - 25
    }

    [Test]
    public void ConditionsCleared_EqualsListCount()
    {
        // Arrange & Act
        var result = new MiracleWorkerResult
        {
            ClearedConditions = new List<string>
            {
                "Poisoned",
                "Blinded",
                "Stunned"
            }.AsReadOnly()
        };

        // Assert
        result.ConditionsCleared.Should().Be(3);
    }

    [Test]
    public void GetStatusMessage_ContainsFullyRestored()
    {
        // Arrange
        var result = new MiracleWorkerResult
        {
            TargetName = "Brave Warrior",
            HpBefore = 10,
            MaxHp = 100
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("FULLY RESTORED");
        message.Should().Contain("Brave Warrior");
        message.Should().Contain("MIRACLE WORKER");
    }

    [Test]
    public void GetStatusMessage_WithConditions_IncludesConditionCount()
    {
        // Arrange
        var result = new MiracleWorkerResult
        {
            TargetName = "Fighter",
            HpBefore = 5,
            MaxHp = 60,
            ClearedConditions = new List<string>
            {
                "Poisoned",
                "Diseased"
            }.AsReadOnly()
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("2 conditions cleared");
    }

    [Test]
    public void GetStatusMessage_WithNoConditions_ExcludesConditionText()
    {
        // Arrange
        var result = new MiracleWorkerResult
        {
            TargetName = "Ranger",
            HpBefore = 30,
            MaxHp = 50,
            ClearedConditions = new List<string>().AsReadOnly()
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().NotContain("conditions cleared");
    }

    [Test]
    public void GetFullBreakdown_ContainsAllSections()
    {
        // Arrange
        var result = new MiracleWorkerResult
        {
            TargetName = "Cleric",
            HpBefore = 20,
            MaxHp = 100,
            ClearedConditions = new List<string>
            {
                "Poisoned",
                "Blinded"
            }.AsReadOnly()
        };

        // Act
        var breakdown = result.GetFullBreakdown();

        // Assert
        breakdown.Should().Contain("=== MIRACLE WORKER ===");
        breakdown.Should().Contain("Target: Cleric");
        breakdown.Should().Contain("HP Restored: 20 -> 100 (+80)");
        breakdown.Should().Contain("Conditions Cleared: 2");
        breakdown.Should().Contain("Removed: Poisoned, Blinded");
        breakdown.Should().Contain("After long rest");
    }
}
