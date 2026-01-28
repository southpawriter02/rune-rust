// ═══════════════════════════════════════════════════════════════════════════════
// TrainingResultTests.cs
// Unit tests for the TrainingResult value object.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TrainingResult"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify training result functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>CreateSuccess factory method</description></item>
///   <item><description>CreateFailure factory method</description></item>
///   <item><description>Derived properties (LevelChanged, TotalCost)</description></item>
///   <item><description>FormatResult and ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class TrainingResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CreateSuccess Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateSuccess creates a successful result.
    /// </summary>
    [Test]
    public void CreateSuccess_WithValidParameters_CreatesSuccessResult()
    {
        // Arrange & Act
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 150,
            weeks: 4,
            trainerId: "master-swordsman",
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert);

        // Assert
        result.Success.Should().BeTrue();
        result.Category.Should().Be(WeaponCategory.Swords);
        result.PsCost.Should().Be(150);
        result.TrainingWeeks.Should().Be(4);
        result.TrainerId.Should().Be("master-swordsman");
        result.OldLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        result.NewLevel.Should().Be(WeaponProficiencyLevel.Expert);
        result.FailureReason.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateSuccess sets LevelChanged true.
    /// </summary>
    [Test]
    public void CreateSuccess_LevelChangedIsTrue()
    {
        // Arrange & Act
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Axes,
            psCost: 50,
            weeks: 2,
            trainerId: "axe-trainer",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        result.LevelChanged.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateSuccess sets TotalCost correctly.
    /// </summary>
    [Test]
    public void CreateSuccess_TotalCost_ReturnsTrainingCost()
    {
        // Arrange & Act
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Hammers,
            psCost: 400,
            weeks: 8,
            trainerId: "hammer-master",
            WeaponProficiencyLevel.Expert,
            WeaponProficiencyLevel.Master);

        // Assert
        result.TotalCost.PiecesSilver.Should().Be(400);
        result.TotalCost.TrainingWeeks.Should().Be(8);
        result.TotalCost.IsFree.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateSuccess throws on zero psCost.
    /// </summary>
    [Test]
    public void CreateSuccess_WithZeroPsCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 0,
            weeks: 2,
            trainerId: "trainer",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("psCost");
    }

    /// <summary>
    /// Verifies that CreateSuccess throws on zero weeks.
    /// </summary>
    [Test]
    public void CreateSuccess_WithZeroWeeks_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 50,
            weeks: 0,
            trainerId: "trainer",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("weeks");
    }

    /// <summary>
    /// Verifies that CreateSuccess throws on null trainerId.
    /// </summary>
    [Test]
    public void CreateSuccess_WithNullTrainerId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 50,
            weeks: 2,
            trainerId: null!,
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("trainerId");
    }

    /// <summary>
    /// Verifies that CreateSuccess throws on empty trainerId.
    /// </summary>
    [Test]
    public void CreateSuccess_WithEmptyTrainerId_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 50,
            weeks: 2,
            trainerId: "",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("trainerId");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CreateFailure Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CreateFailure creates a failure result.
    /// </summary>
    [Test]
    public void CreateFailure_WithValidParameters_CreatesFailureResult()
    {
        // Arrange & Act
        var result = TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            "Already at maximum proficiency level");

        // Assert
        result.Success.Should().BeFalse();
        result.Category.Should().Be(WeaponCategory.Swords);
        result.OldLevel.Should().Be(WeaponProficiencyLevel.Master);
        result.NewLevel.Should().Be(WeaponProficiencyLevel.Master);
        result.FailureReason.Should().Be("Already at maximum proficiency level");
    }

    /// <summary>
    /// Verifies that CreateFailure sets LevelChanged false.
    /// </summary>
    [Test]
    public void CreateFailure_LevelChangedIsFalse()
    {
        // Arrange & Act
        var result = TrainingResult.CreateFailure(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.Proficient,
            "Insufficient funds for training");

        // Assert
        result.LevelChanged.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateFailure sets TotalCost to None.
    /// </summary>
    [Test]
    public void CreateFailure_TotalCost_IsNone()
    {
        // Arrange & Act
        var result = TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            "Trainer not available");

        // Assert
        result.TotalCost.IsFree.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateFailure sets trainer ID to null.
    /// </summary>
    [Test]
    public void CreateFailure_TrainerIdIsNull()
    {
        // Arrange & Act
        var result = TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            "No trainer available");

        // Assert
        result.TrainerId.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateFailure throws on null failure reason.
    /// </summary>
    [Test]
    public void CreateFailure_WithNullReason_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("failureReason");
    }

    /// <summary>
    /// Verifies that CreateFailure throws on empty failure reason.
    /// </summary>
    [Test]
    public void CreateFailure_WithEmptyReason_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("failureReason");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatResult Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FormatResult for success returns correct format.
    /// </summary>
    [Test]
    public void FormatResult_Success_ReturnsCorrectFormat()
    {
        // Arrange
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Swords,
            psCost: 150,
            weeks: 4,
            trainerId: "master-swordsman",
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert);

        // Act & Assert
        result.FormatResult().Should().Be(
            "Swords: Proficient → Expert (150 PS, 4 weeks with master-swordsman)");
    }

    /// <summary>
    /// Verifies that FormatResult for single week uses singular.
    /// </summary>
    [Test]
    public void FormatResult_SingleWeek_UsesSingular()
    {
        // Arrange
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Daggers,
            psCost: 25,
            weeks: 1,
            trainerId: "quick-trainer",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Act & Assert
        result.FormatResult().Should().Contain("1 week");
    }

    /// <summary>
    /// Verifies that FormatResult for failure returns correct format.
    /// </summary>
    [Test]
    public void FormatResult_Failure_ReturnsFailedFormat()
    {
        // Arrange
        var result = TrainingResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            "Insufficient funds for training");

        // Act & Assert
        result.FormatResult().Should().Be("FAILED: Swords - Insufficient funds for training");
    }

    /// <summary>
    /// Verifies that ToString returns same as FormatResult.
    /// </summary>
    [Test]
    public void ToString_ReturnsSameAsFormatResult()
    {
        // Arrange
        var result = TrainingResult.CreateSuccess(
            WeaponCategory.Axes,
            psCost: 50,
            weeks: 2,
            trainerId: "axe-instructor",
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient);

        // Assert
        result.ToString().Should().Be(result.FormatResult());
    }
}
