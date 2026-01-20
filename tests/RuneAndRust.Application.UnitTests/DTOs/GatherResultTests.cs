using FluentAssertions;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.DTOs;

/// <summary>
/// Unit tests for <see cref="GatherResult"/> (v0.11.0c).
/// </summary>
[TestFixture]
public class GatherResultTests
{
    // ═══════════════════════════════════════════════════════════════
    // SUCCESS FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Success_WithAllParameters_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = GatherResult.Success(
            roll: 15,
            modifier: 3,
            total: 18,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Common,
            featureDepleted: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Roll.Should().Be(15);
        result.Modifier.Should().Be(3);
        result.Total.Should().Be(18);
        result.DifficultyClass.Should().Be(12);
        result.ResourceId.Should().Be("iron-ore");
        result.ResourceName.Should().Be("Iron Ore");
        result.Quantity.Should().Be(3);
        result.Quality.Should().Be(ResourceQuality.Common);
        result.FeatureDepleted.Should().BeFalse();
        result.FailureReason.Should().BeNull();
    }

    [Test]
    public void Success_WithFeatureDepleted_SetsDepletedFlag()
    {
        // Arrange & Act
        var result = GatherResult.Success(
            roll: 18,
            modifier: 5,
            total: 23,
            dc: 15,
            resourceId: "gold-ore",
            resourceName: "Gold Ore",
            quantity: 1,
            quality: ResourceQuality.Fine,
            featureDepleted: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.FeatureDepleted.Should().BeTrue();
    }

    [Test]
    public void Success_WithLegendaryQuality_SetsQualityCorrectly()
    {
        // Arrange & Act
        var result = GatherResult.Success(
            roll: 20,
            modifier: 5,
            total: 25,
            dc: 12,
            resourceId: "mithril-ore",
            resourceName: "Mithril Ore",
            quantity: 2,
            quality: ResourceQuality.Legendary,
            featureDepleted: false);

        // Assert
        result.Quality.Should().Be(ResourceQuality.Legendary);
    }

    // ═══════════════════════════════════════════════════════════════
    // FAILED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Failed_WithRollDetails_CreatesFailedResult()
    {
        // Arrange & Act
        var result = GatherResult.Failed(roll: 8, modifier: 3, total: 11, dc: 12);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Roll.Should().Be(8);
        result.Modifier.Should().Be(3);
        result.Total.Should().Be(11);
        result.DifficultyClass.Should().Be(12);
        result.ResourceId.Should().BeNull();
        result.ResourceName.Should().BeNull();
        result.Quantity.Should().Be(0);
        result.Quality.Should().BeNull();
        result.FeatureDepleted.Should().BeFalse();
        result.FailureReason.Should().Be("Gathering check failed.");
    }

    [Test]
    public void Failed_IsNotValidationFailure()
    {
        // Arrange & Act
        var result = GatherResult.Failed(roll: 5, modifier: 2, total: 7, dc: 15);

        // Assert
        result.IsValidationFailure.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // VALIDATION FAILED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ValidationFailed_WithReason_CreatesValidationFailure()
    {
        // Arrange & Act
        var result = GatherResult.ValidationFailed("This resource has been depleted.");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Roll.Should().Be(0);
        result.Modifier.Should().Be(0);
        result.Total.Should().Be(0);
        result.DifficultyClass.Should().Be(0);
        result.FailureReason.Should().Be("This resource has been depleted.");
        result.IsValidationFailure.Should().BeTrue();
    }

    [Test]
    public void ValidationFailed_HasNoResourceData()
    {
        // Arrange & Act
        var result = GatherResult.ValidationFailed("You need a pickaxe to gather this.");

        // Assert
        result.ResourceId.Should().BeNull();
        result.ResourceName.Should().BeNull();
        result.Quantity.Should().Be(0);
        result.Quality.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Margin_WithSuccess_ReturnsPositiveValue()
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 15,
            modifier: 3,
            total: 18,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Common,
            featureDepleted: false);

        // Act & Assert
        result.Margin.Should().Be(6); // 18 - 12 = 6
    }

    [Test]
    public void Margin_WithFailure_ReturnsNegativeValue()
    {
        // Arrange
        var result = GatherResult.Failed(roll: 8, modifier: 3, total: 11, dc: 15);

        // Act & Assert
        result.Margin.Should().Be(-4); // 11 - 15 = -4
    }

    [Test]
    public void Margin_WithValidationFailure_ReturnsZero()
    {
        // Arrange
        var result = GatherResult.ValidationFailed("Cannot gather.");

        // Act & Assert
        result.Margin.Should().Be(0); // 0 - 0 = 0
    }

    [TestCase(18, 8, 10)]  // Exactly 10 margin
    [TestCase(23, 10, 13)] // More than 10 margin
    public void Margin_WithHighRoll_MeetsUpgradeThreshold(int total, int dc, int expectedMargin)
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 15,
            modifier: total - 15,
            total: total,
            dc: dc,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Fine, // Upgraded quality
            featureDepleted: false);

        // Act & Assert
        result.Margin.Should().Be(expectedMargin);
        result.Margin.Should().BeGreaterThanOrEqualTo(10);
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetRollDisplay_WithSuccess_ReturnsFormattedString()
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 15,
            modifier: 3,
            total: 18,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Common,
            featureDepleted: false);

        // Act
        var display = result.GetRollDisplay();

        // Assert
        display.Should().Be("1d20 (15) +3 = 18 vs DC 12");
    }

    [Test]
    public void GetRollDisplay_WithNegativeModifier_FormatsCorrectly()
    {
        // Arrange
        var result = GatherResult.Failed(roll: 10, modifier: -2, total: 8, dc: 12);

        // Act
        var display = result.GetRollDisplay();

        // Assert
        display.Should().Be("1d20 (10) -2 = 8 vs DC 12");
    }

    [Test]
    public void GetRollDisplay_WithZeroModifier_FormatsCorrectly()
    {
        // Arrange
        var result = GatherResult.Failed(roll: 10, modifier: 0, total: 10, dc: 12);

        // Act
        var display = result.GetRollDisplay();

        // Assert
        display.Should().Be("1d20 (10) +0 = 10 vs DC 12");
    }

    [Test]
    public void GetRollDisplay_WithValidationFailure_ReturnsEmptyString()
    {
        // Arrange
        var result = GatherResult.ValidationFailed("Cannot gather.");

        // Act
        var display = result.GetRollDisplay();

        // Assert
        display.Should().BeEmpty();
    }

    [Test]
    public void GetOutcomeDisplay_WithSuccess_ReturnsResourceInfo()
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 15,
            modifier: 3,
            total: 18,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Common,
            featureDepleted: false);

        // Act
        var display = result.GetOutcomeDisplay();

        // Assert
        display.Should().Be("Gathered 3x Iron Ore");
    }

    [Test]
    public void GetOutcomeDisplay_WithNonCommonQuality_IncludesQuality()
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 18,
            modifier: 5,
            total: 23,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Fine,
            featureDepleted: false);

        // Act
        var display = result.GetOutcomeDisplay();

        // Assert
        display.Should().Be("Gathered 3x Iron Ore (Fine)");
    }

    [Test]
    public void GetOutcomeDisplay_WithFeatureDepleted_IncludesNotice()
    {
        // Arrange
        var result = GatherResult.Success(
            roll: 15,
            modifier: 3,
            total: 18,
            dc: 12,
            resourceId: "iron-ore",
            resourceName: "Iron Ore",
            quantity: 3,
            quality: ResourceQuality.Common,
            featureDepleted: true);

        // Act
        var display = result.GetOutcomeDisplay();

        // Assert
        display.Should().Contain("Gathered 3x Iron Ore");
        display.Should().Contain("The resource has been depleted.");
    }

    [Test]
    public void GetOutcomeDisplay_WithFailedRoll_ReturnsFailureMessage()
    {
        // Arrange
        var result = GatherResult.Failed(roll: 8, modifier: 3, total: 11, dc: 15);

        // Act
        var display = result.GetOutcomeDisplay();

        // Assert
        display.Should().Be("Gathering check failed.");
    }

    [Test]
    public void GetOutcomeDisplay_WithValidationFailure_ReturnsReason()
    {
        // Arrange
        var result = GatherResult.ValidationFailed("You need a pickaxe to gather this.");

        // Act
        var display = result.GetOutcomeDisplay();

        // Assert
        display.Should().Be("You need a pickaxe to gather this.");
    }
}
