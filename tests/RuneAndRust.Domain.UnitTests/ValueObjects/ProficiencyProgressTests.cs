// ═══════════════════════════════════════════════════════════════════════════════
// ProficiencyProgressTests.cs
// Unit tests for the ProficiencyProgress value object.
// Version: 0.16.1d
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ProficiencyProgress"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// These tests verify proficiency progress functionality including:
/// </para>
/// <list type="bullet">
///   <item><description>Factory method creation and validation</description></item>
///   <item><description>Derived properties (IsAtMaxLevel, CanAdvance, thresholds)</description></item>
///   <item><description>Experience tracking and progress percentage</description></item>
///   <item><description>AddExperience and AdvanceToNextLevel methods</description></item>
///   <item><description>Formatting methods</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ProficiencyProgressTests
{
    private static readonly ProficiencyThresholds DefaultThresholds =
        ProficiencyThresholds.Default;

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests - Valid Input
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters creates progress.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesProgress()
    {
        // Arrange & Act
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 10,
            DefaultThresholds);

        // Assert
        progress.Category.Should().Be(WeaponCategory.Swords);
        progress.CurrentLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        progress.CombatExperience.Should().Be(10);
    }

    /// <summary>
    /// Verifies that Create with zero experience succeeds.
    /// </summary>
    [Test]
    public void Create_WithZeroExperience_Succeeds()
    {
        // Arrange & Act
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.NonProficient,
            combatExperience: 0,
            DefaultThresholds);

        // Assert
        progress.CombatExperience.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Create throws on negative experience.
    /// </summary>
    [Test]
    public void Create_WithNegativeExperience_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: -1,
            DefaultThresholds);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("combatExperience");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IsAtMaxLevel and CanAdvance Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Master level is at max level.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.Master)]
    public void IsAtMaxLevel_ForMaster_ReturnsTrue(WeaponProficiencyLevel level)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            level,
            combatExperience: 0,
            DefaultThresholds);

        // Assert
        progress.IsAtMaxLevel.Should().BeTrue();
        progress.CanAdvance.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that non-Master levels are not at max level.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient)]
    [TestCase(WeaponProficiencyLevel.Proficient)]
    [TestCase(WeaponProficiencyLevel.Expert)]
    public void IsAtMaxLevel_ForNonMaster_ReturnsFalse(WeaponProficiencyLevel level)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            level,
            combatExperience: 0,
            DefaultThresholds);

        // Assert
        progress.IsAtMaxLevel.Should().BeFalse();
        progress.CanAdvance.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ThresholdForNextLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that threshold returns correct value for each level.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, 10)]
    [TestCase(WeaponProficiencyLevel.Proficient, 25)]
    [TestCase(WeaponProficiencyLevel.Expert, 50)]
    [TestCase(WeaponProficiencyLevel.Master, 0)]
    public void ThresholdForNextLevel_ReturnsCorrectValue(
        WeaponProficiencyLevel level,
        int expectedThreshold)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            level,
            combatExperience: 0,
            DefaultThresholds);

        // Assert
        progress.ThresholdForNextLevel.Should().Be(expectedThreshold);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ExperienceToNextLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that experience to next level calculates correctly.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.Proficient, 0, 25)]
    [TestCase(WeaponProficiencyLevel.Proficient, 10, 15)]
    [TestCase(WeaponProficiencyLevel.Proficient, 24, 1)]
    [TestCase(WeaponProficiencyLevel.Proficient, 25, 0)]
    [TestCase(WeaponProficiencyLevel.Proficient, 30, 0)] // Clamped at 0
    public void ExperienceToNextLevel_CalculatesCorrectly(
        WeaponProficiencyLevel level,
        int experience,
        int expectedRemaining)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            level,
            experience,
            DefaultThresholds);

        // Assert
        progress.ExperienceToNextLevel.Should().Be(expectedRemaining);
    }

    /// <summary>
    /// Verifies that Master level returns 0 experience to next.
    /// </summary>
    [Test]
    public void ExperienceToNextLevel_ForMaster_ReturnsZero()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            combatExperience: 100,
            DefaultThresholds);

        // Assert
        progress.ExperienceToNextLevel.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ProgressPercentage Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that progress percentage calculates correctly.
    /// </summary>
    [Test]
    [TestCase(0, 0)]
    [TestCase(5, 20)]   // 5/25 = 20%
    [TestCase(12, 48)]  // 12/25 = 48%
    [TestCase(25, 100)] // 25/25 = 100%
    public void ProgressPercentage_CalculatesCorrectly(
        int experience,
        int expectedPercentage)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            experience,
            DefaultThresholds);

        // Assert
        progress.ProgressPercentage.Should().Be(expectedPercentage);
    }

    /// <summary>
    /// Verifies that Master level returns 100% progress.
    /// </summary>
    [Test]
    public void ProgressPercentage_ForMaster_Returns100()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            combatExperience: 0,
            DefaultThresholds);

        // Assert
        progress.ProgressPercentage.Should().Be(100m);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HasReachedNextThreshold Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasReachedNextThreshold returns correctly.
    /// </summary>
    [Test]
    [TestCase(24, false)]
    [TestCase(25, true)]
    [TestCase(30, true)]
    public void HasReachedNextThreshold_ReturnsCorrectly(
        int experience,
        bool expectedResult)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            experience,
            DefaultThresholds);

        // Assert
        progress.HasReachedNextThreshold.Should().Be(expectedResult);
    }

    /// <summary>
    /// Verifies that Master level never has reached next threshold.
    /// </summary>
    [Test]
    public void HasReachedNextThreshold_ForMaster_ReturnsFalse()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            combatExperience: 100,
            DefaultThresholds);

        // Assert
        progress.HasReachedNextThreshold.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AddExperience Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AddExperience increments experience correctly.
    /// </summary>
    [Test]
    public void AddExperience_WithDefaultIncrement_AddsOne()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 10,
            DefaultThresholds);

        // Act
        var updated = progress.AddExperience();

        // Assert
        updated.CombatExperience.Should().Be(11);
        progress.CombatExperience.Should().Be(10); // Original unchanged
    }

    /// <summary>
    /// Verifies that AddExperience with specific value works correctly.
    /// </summary>
    [Test]
    public void AddExperience_WithSpecificValue_AddsCorrectAmount()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 10,
            DefaultThresholds);

        // Act
        var updated = progress.AddExperience(5);

        // Assert
        updated.CombatExperience.Should().Be(15);
    }

    /// <summary>
    /// Verifies that AddExperience throws on zero.
    /// </summary>
    [Test]
    public void AddExperience_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 10,
            DefaultThresholds);

        // Act
        var act = () => progress.AddExperience(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("combatsToAdd");
    }

    /// <summary>
    /// Verifies that AddExperience throws on negative.
    /// </summary>
    [Test]
    public void AddExperience_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 10,
            DefaultThresholds);

        // Act
        var act = () => progress.AddExperience(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("combatsToAdd");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AdvanceToNextLevel Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that AdvanceToNextLevel advances correctly.
    /// </summary>
    [Test]
    [TestCase(WeaponProficiencyLevel.NonProficient, WeaponProficiencyLevel.Proficient)]
    [TestCase(WeaponProficiencyLevel.Proficient, WeaponProficiencyLevel.Expert)]
    [TestCase(WeaponProficiencyLevel.Expert, WeaponProficiencyLevel.Master)]
    public void AdvanceToNextLevel_AdvancesCorrectly(
        WeaponProficiencyLevel currentLevel,
        WeaponProficiencyLevel expectedNextLevel)
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            currentLevel,
            combatExperience: 50,
            DefaultThresholds);

        // Act
        var advanced = progress.AdvanceToNextLevel();

        // Assert
        advanced.CurrentLevel.Should().Be(expectedNextLevel);
        advanced.CombatExperience.Should().Be(0); // Experience resets
        advanced.Category.Should().Be(WeaponCategory.Swords);
    }

    /// <summary>
    /// Verifies that AdvanceToNextLevel from Master throws exception.
    /// </summary>
    [Test]
    public void AdvanceToNextLevel_FromMaster_ThrowsInvalidOperationException()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            combatExperience: 0,
            DefaultThresholds);

        // Act
        var act = () => progress.AdvanceToNextLevel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot*Master*");
    }

    /// <summary>
    /// Verifies that original progress is unchanged after advancement.
    /// </summary>
    [Test]
    public void AdvanceToNextLevel_DoesNotMutateOriginal()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 25,
            DefaultThresholds);

        // Act
        _ = progress.AdvanceToNextLevel();

        // Assert
        progress.CurrentLevel.Should().Be(WeaponProficiencyLevel.Proficient);
        progress.CombatExperience.Should().Be(25);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FormatProgress Tests
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that FormatProgress returns expected format.
    /// </summary>
    [Test]
    public void FormatProgress_ForProficient_ReturnsExpectedFormat()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Proficient,
            combatExperience: 24,
            DefaultThresholds);

        // Act
        var result = progress.FormatProgress();

        // Assert
        result.Should().Be("Proficient (24/25 to Expert)");
    }

    /// <summary>
    /// Verifies that FormatProgress for Master shows MAX.
    /// </summary>
    [Test]
    public void FormatProgress_ForMaster_ShowsMax()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Master,
            combatExperience: 0,
            DefaultThresholds);

        // Act
        var result = progress.FormatProgress();

        // Assert
        result.Should().Be("Master (MAX)");
    }

    /// <summary>
    /// Verifies that FormatWithCategory includes category name.
    /// </summary>
    [Test]
    public void FormatWithCategory_IncludesCategoryName()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Swords,
            WeaponProficiencyLevel.Expert,
            combatExperience: 30,
            DefaultThresholds);

        // Act
        var result = progress.FormatWithCategory();

        // Assert
        result.Should().Be("Swords: Expert (30/50 to Master)");
    }

    /// <summary>
    /// Verifies that ToString returns same as FormatProgress.
    /// </summary>
    [Test]
    public void ToString_ReturnsSameAsFormatProgress()
    {
        // Arrange
        var progress = ProficiencyProgress.Create(
            WeaponCategory.Axes,
            WeaponProficiencyLevel.NonProficient,
            combatExperience: 5,
            DefaultThresholds);

        // Assert
        progress.ToString().Should().Be(progress.FormatProgress());
    }
}
