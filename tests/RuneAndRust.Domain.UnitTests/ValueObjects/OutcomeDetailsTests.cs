using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="OutcomeDetails"/> value object.
/// </summary>
[TestFixture]
public class OutcomeDetailsTests
{
    [Test]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var details = new OutcomeDetails(
            outcomeType: SkillOutcome.FullSuccess,
            margin: 2,
            isFumble: false,
            isCritical: false);

        // Assert
        details.OutcomeType.Should().Be(SkillOutcome.FullSuccess);
        details.Margin.Should().Be(2);
        details.IsFumble.Should().BeFalse();
        details.IsCritical.Should().BeFalse();
        details.DescriptorCategory.Should().Be(DescriptorCategory.Competent);
    }

    [Test]
    public void IsSuccess_WhenMarginalSuccess_ReturnsTrue()
    {
        // Arrange
        var details = new OutcomeDetails(
            SkillOutcome.MarginalSuccess, margin: 0, isFumble: false, isCritical: false);

        // Assert
        details.IsSuccess.Should().BeTrue();
        details.IsFailure.Should().BeFalse();
    }

    [Test]
    public void IsFailure_WhenFailure_ReturnsTrue()
    {
        // Arrange
        var details = new OutcomeDetails(
            SkillOutcome.Failure, margin: -2, isFumble: false, isCritical: false);

        // Assert
        details.IsFailure.Should().BeTrue();
        details.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void DescriptorCategory_MapsCorrectlyToSkillOutcome()
    {
        // Arrange & Act
        var criticalFailure = new OutcomeDetails(
            SkillOutcome.CriticalFailure, margin: -5, isFumble: true, isCritical: false);
        var failure = new OutcomeDetails(
            SkillOutcome.Failure, margin: -2, isFumble: false, isCritical: false);
        var marginal = new OutcomeDetails(
            SkillOutcome.MarginalSuccess, margin: 0, isFumble: false, isCritical: false);
        var full = new OutcomeDetails(
            SkillOutcome.FullSuccess, margin: 2, isFumble: false, isCritical: false);
        var exceptional = new OutcomeDetails(
            SkillOutcome.ExceptionalSuccess, margin: 4, isFumble: false, isCritical: false);
        var critical = new OutcomeDetails(
            SkillOutcome.CriticalSuccess, margin: 6, isFumble: false, isCritical: true);

        // Assert
        criticalFailure.DescriptorCategory.Should().Be(DescriptorCategory.Catastrophic);
        failure.DescriptorCategory.Should().Be(DescriptorCategory.Failed);
        marginal.DescriptorCategory.Should().Be(DescriptorCategory.Marginal);
        full.DescriptorCategory.Should().Be(DescriptorCategory.Competent);
        exceptional.DescriptorCategory.Should().Be(DescriptorCategory.Impressive);
        critical.DescriptorCategory.Should().Be(DescriptorCategory.Masterful);
    }

    [Test]
    public void ToDescription_ReturnsHumanReadableString()
    {
        // Arrange
        var success = new OutcomeDetails(
            SkillOutcome.FullSuccess, margin: 2, isFumble: false, isCritical: false);
        var fumble = new OutcomeDetails(
            SkillOutcome.CriticalFailure, margin: -5, isFumble: true, isCritical: false);

        // Act
        var successDesc = success.ToDescription();
        var fumbleDesc = fumble.ToDescription();

        // Assert
        successDesc.Should().Contain("Success");
        successDesc.Should().Contain("Margin: +2");
        fumbleDesc.Should().Contain("Fumble");
    }

    [Test]
    public void ToDescription_WithNegativeMargin_FormatsCorrectly()
    {
        // Arrange
        var failure = new OutcomeDetails(
            SkillOutcome.Failure, margin: -3, isFumble: false, isCritical: false);

        // Act
        var description = failure.ToDescription();

        // Assert
        description.Should().Contain("Failure");
        description.Should().Contain("Margin: -3");
    }
}
