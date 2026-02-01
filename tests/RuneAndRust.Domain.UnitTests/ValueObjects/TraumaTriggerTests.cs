namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaTrigger"/> value object.
/// Verifies factory method, validation, and ToString formatting.
/// </summary>
[TestFixture]
public class TraumaTriggerTests
{
    // -------------------------------------------------------------------------
    // Factory Method — Valid Creation
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithValidParameters_CreatesTrigger()
    {
        // Arrange & Act
        var trigger = TraumaTrigger.Create(
            triggerType: "OnRest",
            condition: null,
            checkRequired: true,
            checkDifficulty: 2
        );

        // Assert
        trigger.TriggerType.Should().Be("OnRest");
        trigger.Condition.Should().BeNull();
        trigger.CheckRequired.Should().BeTrue();
        trigger.CheckDifficulty.Should().Be(2);
    }

    [Test]
    public void Create_WithMinimalParameters_UsesDefaults()
    {
        // Arrange & Act
        var trigger = TraumaTrigger.Create(triggerType: "NearForlorn");

        // Assert
        trigger.TriggerType.Should().Be("NearForlorn");
        trigger.Condition.Should().BeNull();
        trigger.CheckRequired.Should().BeFalse();
        trigger.CheckDifficulty.Should().BeNull();
    }

    [Test]
    public void Create_WithCondition_StoresCondition()
    {
        // Arrange & Act
        var trigger = TraumaTrigger.Create(
            triggerType: "OnAllyDeath",
            condition: "AllyWasClose",
            checkRequired: false
        );

        // Assert
        trigger.TriggerType.Should().Be("OnAllyDeath");
        trigger.Condition.Should().Be("AllyWasClose");
    }

    [Test]
    public void Create_CheckNotRequired_AllowsNullDifficulty()
    {
        // Arrange & Act
        var trigger = TraumaTrigger.Create(
            triggerType: "InCombat",
            checkRequired: false,
            checkDifficulty: null
        );

        // Assert
        trigger.CheckRequired.Should().BeFalse();
        trigger.CheckDifficulty.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // Factory Method — Validation Errors
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithEmptyTriggerType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaTrigger.Create(triggerType: "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("triggerType");
    }

    [Test]
    public void Create_WithWhitespaceTriggerType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaTrigger.Create(triggerType: "   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("triggerType");
    }

    [Test]
    public void Create_WithNullTriggerType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaTrigger.Create(triggerType: null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("triggerType");
    }

    [Test]
    public void Create_CheckRequiredWithoutDifficulty_ThrowsInvalidOperationException()
    {
        // Arrange & Act
        var act = () => TraumaTrigger.Create(
            triggerType: "OnRest",
            condition: null,
            checkRequired: true,
            checkDifficulty: null
        );

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CheckDifficulty must be specified*");
    }

    // -------------------------------------------------------------------------
    // ToString Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_WithoutConditionOrCheck_ReturnsBasicFormat()
    {
        // Arrange
        var trigger = TraumaTrigger.Create(triggerType: "NearForlorn");

        // Act
        var result = trigger.ToString();

        // Assert
        result.Should().Be("NearForlorn");
        result.Should().NotContain("[");
    }

    [Test]
    public void ToString_WithCondition_IncludesConditionBracket()
    {
        // Arrange
        var trigger = TraumaTrigger.Create(
            triggerType: "OnAllyDeath",
            condition: "AllyWasClose"
        );

        // Act
        var result = trigger.ToString();

        // Assert
        result.Should().Contain("[AllyWasClose]");
    }

    [Test]
    public void ToString_WithCheckRequired_IncludesDCBracket()
    {
        // Arrange
        var trigger = TraumaTrigger.Create(
            triggerType: "OnRest",
            checkRequired: true,
            checkDifficulty: 3
        );

        // Act
        var result = trigger.ToString();

        // Assert
        result.Should().Contain("[DC 3]");
    }

    [Test]
    public void ToString_WithConditionAndCheck_IncludesBoth()
    {
        // Arrange
        var trigger = TraumaTrigger.Create(
            triggerType: "InCombat",
            condition: "WhenFlanked",
            checkRequired: true,
            checkDifficulty: 2
        );

        // Act
        var result = trigger.ToString();

        // Assert
        result.Should().Contain("[WhenFlanked]");
        result.Should().Contain("[DC 2]");
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaTrigger_WithSameValues_AreEqual()
    {
        // Arrange
        var trigger1 = TraumaTrigger.Create("OnRest", null, true, 2);
        var trigger2 = TraumaTrigger.Create("OnRest", null, true, 2);

        // Assert
        trigger1.Should().Be(trigger2);
        (trigger1 == trigger2).Should().BeTrue();
    }

    [Test]
    public void TraumaTrigger_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var trigger1 = TraumaTrigger.Create("OnRest", null, true, 2);
        var trigger2 = TraumaTrigger.Create("InCombat", null, false, null);

        // Assert
        trigger1.Should().NotBe(trigger2);
        (trigger1 != trigger2).Should().BeTrue();
    }
}
