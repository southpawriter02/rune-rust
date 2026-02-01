namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaEffect"/> value object.
/// Verifies factory method, validation, and ToString formatting.
/// </summary>
[TestFixture]
public class TraumaEffectTests
{
    // -------------------------------------------------------------------------
    // Factory Method — Valid Creation
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithValidParameters_CreatesEffect()
    {
        // Arrange & Act
        var effect = TraumaEffect.Create(
            effectType: "Penalty",
            target: "social-skills",
            value: -3,
            description: "-3 penalty to Social checks"
        );

        // Assert
        effect.EffectType.Should().Be("Penalty");
        effect.Target.Should().Be("social-skills");
        effect.Value.Should().Be(-3);
        effect.Description.Should().Be("-3 penalty to Social checks");
    }

    [Test]
    public void Create_WithMinimalParameters_UsesDefaultDescription()
    {
        // Arrange & Act
        var effect = TraumaEffect.Create(
            effectType: "Disadvantage",
            target: "morale-checks"
        );

        // Assert
        effect.EffectType.Should().Be("Disadvantage");
        effect.Target.Should().Be("morale-checks");
        effect.Value.Should().BeNull();
        effect.Condition.Should().BeNull();
        effect.Description.Should().Be("Disadvantage to morale-checks");
    }

    [Test]
    public void Create_WithCondition_StoresCondition()
    {
        // Arrange & Act
        var effect = TraumaEffect.Create(
            effectType: "StressIncrease",
            target: "stress",
            value: 5,
            condition: "OnAllyTakeCriticalHit",
            description: "+5 stress when ally takes critical damage"
        );

        // Assert
        effect.Condition.Should().Be("OnAllyTakeCriticalHit");
        effect.Value.Should().Be(5);
    }

    // -------------------------------------------------------------------------
    // Factory Method — Validation Errors
    // -------------------------------------------------------------------------

    [Test]
    public void Create_WithEmptyEffectType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaEffect.Create(
            effectType: "",
            target: "social-skills"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("effectType");
    }

    [Test]
    public void Create_WithWhitespaceEffectType_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaEffect.Create(
            effectType: "   ",
            target: "social-skills"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("effectType");
    }

    [Test]
    public void Create_WithEmptyTarget_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaEffect.Create(
            effectType: "Penalty",
            target: ""
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("target");
    }

    [Test]
    public void Create_WithNullTarget_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TraumaEffect.Create(
            effectType: "Penalty",
            target: null!
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("target");
    }

    // -------------------------------------------------------------------------
    // ToString Formatting
    // -------------------------------------------------------------------------

    [Test]
    public void ToString_WithoutCondition_FormatsCorrectly()
    {
        // Arrange
        var effect = TraumaEffect.Create(
            effectType: "Penalty",
            target: "social-skills",
            description: "-3 penalty to Social checks"
        );

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Be("Penalty: -3 penalty to Social checks");
        result.Should().NotContain("[");
    }

    [Test]
    public void ToString_WithCondition_IncludesConditionBracket()
    {
        // Arrange
        var effect = TraumaEffect.Create(
            effectType: "StressIncrease",
            target: "stress",
            condition: "OnAllyTakeCriticalHit",
            description: "+5 stress when ally takes critical damage"
        );

        // Act
        var result = effect.ToString();

        // Assert
        result.Should().Contain("[OnAllyTakeCriticalHit]");
    }

    // -------------------------------------------------------------------------
    // Record Equality
    // -------------------------------------------------------------------------

    [Test]
    public void TraumaEffect_WithSameValues_AreEqual()
    {
        // Arrange
        var effect1 = TraumaEffect.Create("Penalty", "social-skills", -3);
        var effect2 = TraumaEffect.Create("Penalty", "social-skills", -3);

        // Assert
        effect1.Should().Be(effect2);
        (effect1 == effect2).Should().BeTrue();
    }

    [Test]
    public void TraumaEffect_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var effect1 = TraumaEffect.Create("Penalty", "social-skills", -3);
        var effect2 = TraumaEffect.Create("Bonus", "tech-checks", 2);

        // Assert
        effect1.Should().NotBe(effect2);
        (effect1 != effect2).Should().BeTrue();
    }
}
