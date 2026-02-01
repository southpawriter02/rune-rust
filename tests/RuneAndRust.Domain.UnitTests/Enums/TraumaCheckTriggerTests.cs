namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Unit tests for <see cref="TraumaCheckTrigger"/> enum.
/// Verifies enum values, count, and default value.
/// </summary>
[TestFixture]
public class TraumaCheckTriggerTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Enum Value Count
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void TraumaCheckTrigger_ShouldHaveExactlyEightValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<TraumaCheckTrigger>();

        // Assert
        values.Should().HaveCount(8);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Default Value
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void TraumaCheckTrigger_ShouldHaveStressThreshold100AsDefault()
    {
        // Arrange & Act
        var defaultTrigger = default(TraumaCheckTrigger);

        // Assert
        defaultTrigger.Should().Be(TraumaCheckTrigger.StressThreshold100);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Expected Values
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void TraumaCheckTrigger_ShouldContainAllExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<TraumaCheckTrigger>();

        // Assert
        values.Should().Contain(TraumaCheckTrigger.StressThreshold100);
        values.Should().Contain(TraumaCheckTrigger.CorruptionThreshold100);
        values.Should().Contain(TraumaCheckTrigger.AllyDeath);
        values.Should().Contain(TraumaCheckTrigger.NearDeathExperience);
        values.Should().Contain(TraumaCheckTrigger.CriticalFailure);
        values.Should().Contain(TraumaCheckTrigger.ProlongedExposure);
        values.Should().Contain(TraumaCheckTrigger.WitnessingHorror);
        values.Should().Contain(TraumaCheckTrigger.RuinMadnessEscape);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Explicit Values
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    [TestCase(TraumaCheckTrigger.StressThreshold100, 0)]
    [TestCase(TraumaCheckTrigger.CorruptionThreshold100, 1)]
    [TestCase(TraumaCheckTrigger.AllyDeath, 2)]
    [TestCase(TraumaCheckTrigger.NearDeathExperience, 3)]
    [TestCase(TraumaCheckTrigger.CriticalFailure, 4)]
    [TestCase(TraumaCheckTrigger.ProlongedExposure, 5)]
    [TestCase(TraumaCheckTrigger.WitnessingHorror, 6)]
    [TestCase(TraumaCheckTrigger.RuinMadnessEscape, 7)]
    public void TraumaCheckTrigger_ShouldHaveCorrectUnderlyingValue(
        TraumaCheckTrigger trigger,
        int expectedValue)
    {
        // Assert
        ((int)trigger).Should().Be(expectedValue);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Difficulty Tiers
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void TraumaCheckTrigger_EasyTriggers_ShouldBeCriticalFailure()
    {
        // Arrange - CriticalFailure is DC 1 (easiest)
        var easyTrigger = TraumaCheckTrigger.CriticalFailure;

        // Assert
        ((int)easyTrigger).Should().Be(4);
    }

    [Test]
    public void TraumaCheckTrigger_HardTriggers_ShouldBeCorruptionAndRuinMadness()
    {
        // Arrange - DC 4 triggers (hardest)
        var corruptionTrigger = TraumaCheckTrigger.CorruptionThreshold100;
        var ruinMadnessTrigger = TraumaCheckTrigger.RuinMadnessEscape;

        // Assert - These are the hardest checks
        ((int)corruptionTrigger).Should().Be(1);
        ((int)ruinMadnessTrigger).Should().Be(7);
    }

    [Test]
    public void TraumaCheckTrigger_ModerateTriggers_ShouldHaveIntermediateValues()
    {
        // Arrange - DC 2 triggers
        var allyDeath = TraumaCheckTrigger.AllyDeath;
        var nearDeath = TraumaCheckTrigger.NearDeathExperience;
        var prolonged = TraumaCheckTrigger.ProlongedExposure;

        // Assert
        ((int)allyDeath).Should().Be(2);
        ((int)nearDeath).Should().Be(3);
        ((int)prolonged).Should().Be(5);
    }
}
