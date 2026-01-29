// ═══════════════════════════════════════════════════════════════════════════════
// ArmorTrainingResultTests.cs
// Unit tests for the ArmorTrainingResult value object.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArmorTrainingResult"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover:
/// </para>
/// <list type="bullet">
///   <item><description>Success factory methods</description></item>
///   <item><description>Failure factory methods</description></item>
///   <item><description>Computed properties</description></item>
///   <item><description>Formatting methods</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArmorTrainingResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Success Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Successful_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.Successful(
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.NonProficient,
            ArmorProficiencyLevel.Proficient,
            currencySpent: 50,
            timeSpentWeeks: 2);

        // Assert
        result.Success.Should().BeTrue();
        result.ArmorCategory.Should().Be(ArmorCategory.Heavy);
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        result.CurrencySpent.Should().Be(50);
        result.TimeSpentWeeks.Should().Be(2);
        result.LevelImproved.Should().BeTrue();
        result.HasFailureReasons.Should().BeFalse();
    }

    [Test]
    public void Successful_WhenNewLevelNotGreater_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorTrainingResult.Successful(
            ArmorCategory.Light,
            ArmorProficiencyLevel.Expert,
            ArmorProficiencyLevel.Proficient, // Less than previous
            currencySpent: 100,
            timeSpentWeeks: 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than previous*");
    }

    [Test]
    public void TrainedToProficient_CreatesCorrectResult()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.TrainedToProficient(
            ArmorCategory.Medium,
            currencySpent: 50,
            timeSpentWeeks: 2);

        // Assert
        result.Success.Should().BeTrue();
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        result.Message.Should().Contain("proficient");
    }

    [Test]
    public void TrainedToExpert_CreatesCorrectResult()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.TrainedToExpert(
            ArmorCategory.Heavy,
            currencySpent: 200,
            timeSpentWeeks: 4);

        // Assert
        result.Success.Should().BeTrue();
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Expert);
        result.Message.Should().Contain("expert");
    }

    [Test]
    public void TrainedToMaster_CreatesCorrectResult()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.TrainedToMaster(
            ArmorCategory.Shields,
            currencySpent: 500,
            timeSpentWeeks: 8);

        // Assert
        result.Success.Should().BeTrue();
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.Expert);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Master);
        result.Message.Should().Contain("mastery");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Failure Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Failed_CreatesFailureResult()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.Failed(
            ArmorCategory.Light,
            ArmorProficiencyLevel.NonProficient,
            "Test failure reason");

        // Assert
        result.Success.Should().BeFalse();
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        result.CurrencySpent.Should().Be(0);
        result.TimeSpentWeeks.Should().Be(0);
        result.HasFailureReasons.Should().BeTrue();
        result.PrimaryFailureReason.Should().Be("Test failure reason");
    }

    [Test]
    public void InsufficientCurrency_CreatesCorrectFailure()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.InsufficientCurrency(
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Proficient,
            required: 500,
            available: 100);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Insufficient funds");
        result.PrimaryFailureReason.Should().Contain("500 PS");
        result.PrimaryFailureReason.Should().Contain("100 PS");
    }

    [Test]
    public void LevelTooLow_CreatesCorrectFailure()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.LevelTooLow(
            ArmorCategory.Medium,
            ArmorProficiencyLevel.Proficient,
            requiredLevel: 10,
            actualLevel: 5);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("Character level too low");
        result.PrimaryFailureReason.Should().Contain("Level 10");
        result.PrimaryFailureReason.Should().Contain("Level 5");
    }

    [Test]
    public void AlreadyMaster_CreatesCorrectFailure()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.AlreadyMaster(ArmorCategory.Heavy);

        // Assert
        result.Success.Should().BeFalse();
        result.PreviousLevel.Should().Be(ArmorProficiencyLevel.Master);
        result.NewLevel.Should().Be(ArmorProficiencyLevel.Master);
        result.PrimaryFailureReason.Should().Contain("Already at Master");
    }

    [Test]
    public void NoTrainerAvailable_CreatesCorrectFailure()
    {
        // Arrange & Act
        var result = ArmorTrainingResult.NoTrainerAvailable(
            ArmorCategory.Shields,
            ArmorProficiencyLevel.Expert);

        // Assert
        result.Success.Should().BeFalse();
        result.PrimaryFailureReason.Should().Contain("No trainer available");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ResourceSummary_WhenNoResources_ReturnsNoResourcesMessage()
    {
        // Arrange
        var result = ArmorTrainingResult.Failed(
            ArmorCategory.Light,
            ArmorProficiencyLevel.NonProficient,
            "Test reason");

        // Act & Assert
        result.ResourceSummary.Should().Be("No resources consumed");
    }

    [Test]
    public void ResourceSummary_WithResources_ReturnsFormattedString()
    {
        // Arrange
        var result = ArmorTrainingResult.TrainedToExpert(
            ArmorCategory.Heavy,
            currencySpent: 200,
            timeSpentWeeks: 4);

        // Act & Assert
        result.ResourceSummary.Should().Contain("200 PS");
        result.ResourceSummary.Should().Contain("4 weeks");
    }

    [Test]
    public void FormatForDisplay_Success_IncludesCheckmark()
    {
        // Arrange
        var result = ArmorTrainingResult.TrainedToProficient(
            ArmorCategory.Medium,
            currencySpent: 50,
            timeSpentWeeks: 2);

        // Act
        var display = result.FormatForDisplay();

        // Assert
        display.Should().StartWith("✓");
    }

    [Test]
    public void FormatForDisplay_Failure_IncludesX()
    {
        // Arrange
        var result = ArmorTrainingResult.InsufficientCurrency(
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.NonProficient,
            required: 100,
            available: 25);

        // Act
        var display = result.FormatForDisplay();

        // Assert
        display.Should().StartWith("✗");
    }
}
