using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="EmergencySurgeryResult"/> value object.
/// Tests computed properties, healing breakdown display, and status message formatting.
/// </summary>
[TestFixture]
public class EmergencySurgeryResultTests
{
    // ===== Computed Property Tests =====

    [Test]
    public void TotalHealing_SumsAllComponents()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            HealingRoll = 14,
            QualityBonus = 3,
            SteadyHandsBonus = 2,
            RecoveryBonus = 4
        };

        // Assert
        result.TotalHealing.Should().Be(23); // 14 + 3 + 2 + 4
    }

    [Test]
    public void TotalHealing_WithZeroBonuses_EqualsRollOnly()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            HealingRoll = 10,
            QualityBonus = 0,
            SteadyHandsBonus = 0,
            RecoveryBonus = 0
        };

        // Assert
        result.TotalHealing.Should().Be(10);
    }

    // ===== GetHealingBreakdown Tests =====

    [Test]
    public void GetHealingBreakdown_WithRecoveryBonus_IncludesRecoveryComponent()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            HealingRoll = 14,
            QualityBonus = 3,
            SteadyHandsBonus = 2,
            RecoveryBonus = 3,
            SupplyTypeUsed = "Suture",
            BonusTriggered = true,
            TargetCondition = RecoveryCondition.Recovering
        };

        // Act
        var breakdown = result.GetHealingBreakdown();

        // Assert
        breakdown.Should().Contain("Base (4d6): 14");
        breakdown.Should().Contain("Quality (Suture): +3");
        breakdown.Should().Contain("Steady Hands: +2");
        breakdown.Should().Contain("Recovery (Recovering): +3");
        breakdown.Should().Contain("Total: 22");
    }

    [Test]
    public void GetHealingBreakdown_WithoutRecoveryBonus_ExcludesRecoveryComponent()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            HealingRoll = 14,
            QualityBonus = 2,
            SteadyHandsBonus = 2,
            RecoveryBonus = 0,
            SupplyTypeUsed = "Bandage",
            BonusTriggered = false,
            TargetCondition = null
        };

        // Act
        var breakdown = result.GetHealingBreakdown();

        // Assert
        breakdown.Should().NotContain("Recovery");
        breakdown.Should().Contain("Total: 18");
    }

    // ===== GetStatusMessage Tests =====

    [Test]
    public void GetStatusMessage_WithBonus_IncludesInterventionIndicator()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            TargetName = "Fighter",
            HpBefore = 5,
            HpAfter = 27,
            HealingRoll = 14,
            QualityBonus = 3,
            SteadyHandsBonus = 2,
            RecoveryBonus = 3,
            BonusTriggered = true,
            TargetCondition = RecoveryCondition.Recovering
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("Fighter healed for 22 HP (5 → 27)");
        message.Should().Contain("[EMERGENCY INTERVENTION BONUS: +3]");
    }

    [Test]
    public void GetStatusMessage_WithoutBonus_NoInterventionIndicator()
    {
        // Arrange
        var result = new EmergencySurgeryResult
        {
            TargetName = "Ranger",
            HpBefore = 30,
            HpAfter = 48,
            HealingRoll = 14,
            QualityBonus = 2,
            SteadyHandsBonus = 2,
            RecoveryBonus = 0,
            BonusTriggered = false
        };

        // Act
        var message = result.GetStatusMessage();

        // Assert
        message.Should().Contain("Ranger healed for 18 HP (30 → 48)");
        message.Should().NotContain("EMERGENCY INTERVENTION BONUS");
    }
}
