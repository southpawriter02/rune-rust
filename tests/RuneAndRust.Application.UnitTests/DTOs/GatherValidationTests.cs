using FluentAssertions;
using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.UnitTests.DTOs;

/// <summary>
/// Unit tests for <see cref="GatherValidation"/> (v0.11.0c).
/// </summary>
[TestFixture]
public class GatherValidationTests
{
    // ═══════════════════════════════════════════════════════════════
    // SUCCESS FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Success_WithDC_CreatesValidResult()
    {
        // Arrange & Act
        var validation = GatherValidation.Success(dc: 12);

        // Assert
        validation.IsValid.Should().BeTrue();
        validation.DifficultyClass.Should().Be(12);
        validation.FailureReason.Should().BeNull();
    }

    [TestCase(10)]
    [TestCase(15)]
    [TestCase(20)]
    public void Success_WithVariousDCs_SetsDifficultyClassCorrectly(int dc)
    {
        // Arrange & Act
        var validation = GatherValidation.Success(dc);

        // Assert
        validation.DifficultyClass.Should().Be(dc);
    }

    // ═══════════════════════════════════════════════════════════════
    // FAILED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Failed_WithReason_CreatesInvalidResult()
    {
        // Arrange & Act
        var validation = GatherValidation.Failed("This resource has been depleted.");

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.DifficultyClass.Should().BeNull();
        validation.FailureReason.Should().Be("This resource has been depleted.");
    }

    [Test]
    public void Failed_ToolRequired_ReturnsAppropriateMessage()
    {
        // Arrange & Act
        var validation = GatherValidation.Failed("You need a pickaxe to gather this.");

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.FailureReason.Should().Contain("pickaxe");
    }

    [Test]
    public void Failed_FeatureNotFound_ReturnsAppropriateMessage()
    {
        // Arrange & Act
        var validation = GatherValidation.Failed("Unknown harvestable feature type.");

        // Assert
        validation.IsValid.Should().BeFalse();
        validation.FailureReason.Should().Contain("Unknown");
    }

    // ═══════════════════════════════════════════════════════════════
    // RECORD EQUALITY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Success_TwoWithSameDC_AreEqual()
    {
        // Arrange
        var validation1 = GatherValidation.Success(dc: 12);
        var validation2 = GatherValidation.Success(dc: 12);

        // Act & Assert
        validation1.Should().Be(validation2);
    }

    [Test]
    public void Failed_TwoWithSameReason_AreEqual()
    {
        // Arrange
        var validation1 = GatherValidation.Failed("Resource depleted.");
        var validation2 = GatherValidation.Failed("Resource depleted.");

        // Act & Assert
        validation1.Should().Be(validation2);
    }

    [Test]
    public void Success_DifferentDCs_AreNotEqual()
    {
        // Arrange
        var validation1 = GatherValidation.Success(dc: 10);
        var validation2 = GatherValidation.Success(dc: 15);

        // Act & Assert
        validation1.Should().NotBe(validation2);
    }
}
