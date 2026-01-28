// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyGainResultTests.cs
// Unit tests for the ProficiencyGainResult value object.
// Version: 0.16.1e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ProficiencyGainResult"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify proficiency gain result functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>CreateSuccess factory method</description></item>
///   <item><description>CreateFailure factory method</description></item>
///   <item><description>Derived properties (LevelChanged, WasFree, ReachedMaster)</description></item>
///   <item><description>FormatResult and ToString formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ProficiencyGainResultTests
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
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert,
            AcquisitionMethod.ProgressionPointPurchase,
            AcquisitionCost.FromPP(2));

        // Assert
        result.Success.Should().BeTrue();
        result.Category.Should().Be(WeaponCategory.Swords);
        result.OldLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        result.NewLevel.Should().Be(WeaponProficiencyLevel.Expert);
        result.Method.Should().Be(AcquisitionMethod.ProgressionPointPurchase);
        result.FailureReason.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateSuccess with level change sets LevelChanged true.
    /// </summary>
    [Test]
    public void CreateSuccess_WithLevelChange_LevelChangedIsTrue()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.Archetype,
            AcquisitionCost.None);

        // Assert
        result.LevelChanged.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateSuccess without level change sets LevelChanged false.
    /// </summary>
    [Test]
    public void CreateSuccess_WithoutLevelChange_LevelChangedIsFalse()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.CombatExperience,
            AcquisitionCost.None);

        // Assert
        result.LevelChanged.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateSuccess with free cost sets WasFree true.
    /// </summary>
    [Test]
    public void CreateSuccess_WithFreeCost_WasFreeIsTrue()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Daggers,
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.Archetype,
            AcquisitionCost.None);

        // Assert
        result.WasFree.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateSuccess with paid cost sets WasFree false.
    /// </summary>
    [Test]
    public void CreateSuccess_WithPaidCost_WasFreeIsFalse()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert,
            AcquisitionMethod.ProgressionPointPurchase,
            AcquisitionCost.FromPP(2));

        // Assert
        result.WasFree.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateSuccess advancing to Master sets ReachedMaster true.
    /// </summary>
    [Test]
    public void CreateSuccess_AdvancingToMaster_ReachedMasterIsTrue()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Expert,
            WeaponProficiencyLevel.Master,
            AcquisitionMethod.CombatExperience,
            AcquisitionCost.None);

        // Assert
        result.ReachedMaster.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateSuccess not advancing to Master sets ReachedMaster false.
    /// </summary>
    [Test]
    public void CreateSuccess_NotAdvancingToMaster_ReachedMasterIsFalse()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert,
            AcquisitionMethod.NpcTraining,
            AcquisitionCost.FromTraining(150, 4));

        // Assert
        result.ReachedMaster.Should().BeFalse();
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
        var result = ProficiencyGainResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            AcquisitionMethod.CombatExperience,
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
        var result = ProficiencyGainResult.CreateFailure(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.ProgressionPointPurchase,
            "Insufficient Progression Points");

        // Assert
        result.LevelChanged.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CreateFailure uses no cost.
    /// </summary>
    [Test]
    public void CreateFailure_CostPaidIsNone()
    {
        // Arrange & Act
        var result = ProficiencyGainResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.NpcTraining,
            "Insufficient funds");

        // Assert
        result.CostPaid.IsFree.Should().BeTrue();
        result.WasFree.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CreateFailure throws on null failure reason.
    /// </summary>
    [Test]
    public void CreateFailure_WithNullReason_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => ProficiencyGainResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.NpcTraining,
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
        var act = () => ProficiencyGainResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.NpcTraining,
            "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("failureReason");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatResult Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FormatResult for success with level change returns correct format.
    /// </summary>
    [Test]
    public void FormatResult_SuccessWithLevelChange_ReturnsCorrectFormat()
    {
        // Arrange
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Expert,
            AcquisitionMethod.ProgressionPointPurchase,
            AcquisitionCost.FromPP(2));

        // Act & Assert
        result.FormatResult().Should().Be("Swords: Proficient → Expert (2 PP)");
    }

    /// <summary>
    /// Verifies that FormatResult for success without level change returns correct format.
    /// </summary>
    [Test]
    public void FormatResult_SuccessWithoutLevelChange_ReturnsExperienceRecordedFormat()
    {
        // Arrange
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.Proficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.CombatExperience,
            AcquisitionCost.None);

        // Act & Assert
        result.FormatResult().Should().Be("Axes: Proficient (experience recorded)");
    }

    /// <summary>
    /// Verifies that FormatResult for failure returns correct format.
    /// </summary>
    [Test]
    public void FormatResult_Failure_ReturnsFailedFormat()
    {
        // Arrange
        var result = ProficiencyGainResult.CreateFailure(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            AcquisitionMethod.CombatExperience,
            "Already at maximum proficiency level");

        // Act & Assert
        result.FormatResult().Should().Be("FAILED: Swords - Already at maximum proficiency level");
    }

    /// <summary>
    /// Verifies that ToString returns same as FormatResult.
    /// </summary>
    [Test]
    public void ToString_ReturnsSameAsFormatResult()
    {
        // Arrange
        var result = ProficiencyGainResult.CreateSuccess(
            WeaponCategory.Daggers,
            WeaponProficiencyLevel.NonProficient,
            WeaponProficiencyLevel.Proficient,
            AcquisitionMethod.Archetype,
            AcquisitionCost.None);

        // Assert
        result.ToString().Should().Be(result.FormatResult());
    }
}
