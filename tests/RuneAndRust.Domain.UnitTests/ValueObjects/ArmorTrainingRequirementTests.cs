// ═══════════════════════════════════════════════════════════════════════════════
// ArmorTrainingRequirementTests.cs
// Unit tests for the ArmorTrainingRequirement value object.
// Version: 0.16.2e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="ArmorTrainingRequirement"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover:
/// </para>
/// <list type="bullet">
///   <item><description>Factory method creation</description></item>
///   <item><description>Level-specific factory methods</description></item>
///   <item><description>Validation and error handling</description></item>
///   <item><description>Computed properties and formatting</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArmorTrainingRequirementTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Create_WithValidParameters_CreatesRequirement()
    {
        // Arrange & Act
        var requirement = ArmorTrainingRequirement.Create(
            ArmorCategory.Heavy,
            ArmorProficiencyLevel.Proficient,
            currencyCost: 50,
            trainingWeeks: 2,
            minimumCharacterLevel: 1);

        // Assert
        requirement.ArmorCategory.Should().Be(ArmorCategory.Heavy);
        requirement.TargetLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        requirement.RequiredLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        requirement.CurrencyCost.Should().Be(50);
        requirement.TrainingWeeks.Should().Be(2);
        requirement.MinimumCharacterLevel.Should().Be(1);
        requirement.RequiresNpcTrainer.Should().BeTrue();
    }

    [Test]
    public void Create_ToNonProficient_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorTrainingRequirement.Create(
            ArmorCategory.Light,
            ArmorProficiencyLevel.NonProficient,
            currencyCost: 0,
            trainingWeeks: 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Cannot train to NonProficient*");
    }

    [Test]
    public void Create_WithNegativeCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        var act = () => ArmorTrainingRequirement.Create(
            ArmorCategory.Medium,
            ArmorProficiencyLevel.Expert,
            currencyCost: -100,
            trainingWeeks: 2);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Level-Specific Factory Method Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void ForProficient_CreatesCorrectRequirement()
    {
        // Arrange & Act
        var requirement = ArmorTrainingRequirement.ForProficient(ArmorCategory.Light);

        // Assert
        requirement.TargetLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        requirement.RequiredLevel.Should().Be(ArmorProficiencyLevel.NonProficient);
        requirement.CurrencyCost.Should().Be(50);
        requirement.TrainingWeeks.Should().Be(2);
        requirement.TransitionDescription.Should().Be("NonProficient → Proficient");
    }

    [Test]
    public void ForExpert_CreatesCorrectRequirement()
    {
        // Arrange & Act
        var requirement = ArmorTrainingRequirement.ForExpert(ArmorCategory.Medium, 250, 5);

        // Assert
        requirement.TargetLevel.Should().Be(ArmorProficiencyLevel.Expert);
        requirement.RequiredLevel.Should().Be(ArmorProficiencyLevel.Proficient);
        requirement.CurrencyCost.Should().Be(250);
        requirement.TrainingWeeks.Should().Be(5);
        requirement.MinimumCharacterLevel.Should().Be(5);
    }

    [Test]
    public void ForMaster_CreatesCorrectRequirement()
    {
        // Arrange & Act
        var requirement = ArmorTrainingRequirement.ForMaster(ArmorCategory.Heavy);

        // Assert
        requirement.TargetLevel.Should().Be(ArmorProficiencyLevel.Master);
        requirement.RequiredLevel.Should().Be(ArmorProficiencyLevel.Expert);
        requirement.CurrencyCost.Should().Be(500);
        requirement.TrainingWeeks.Should().Be(8);
        requirement.MinimumCharacterLevel.Should().Be(10);
    }

    [Test]
    public void Free_CreatesZeroCostRequirement()
    {
        // Arrange & Act
        var requirement = ArmorTrainingRequirement.Free(
            ArmorCategory.Light,
            ArmorProficiencyLevel.Proficient);

        // Assert
        requirement.CurrencyCost.Should().Be(0);
        requirement.TrainingWeeks.Should().Be(0);
        requirement.RequiresNpcTrainer.Should().BeFalse();
        requirement.HasCost.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Computed Properties Tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void HasCost_WhenCurrencyCostPositive_ReturnsTrue()
    {
        // Arrange
        var requirement = ArmorTrainingRequirement.ForProficient(ArmorCategory.Light);

        // Act & Assert
        requirement.HasCost.Should().BeTrue();
    }

    [Test]
    public void FormatCostSummary_ReturnsFormattedString()
    {
        // Arrange
        var requirement = ArmorTrainingRequirement.ForExpert(ArmorCategory.Heavy);

        // Act
        var summary = requirement.FormatCostSummary();

        // Assert
        summary.Should().Be("200 PS / 4wk");
    }

    [Test]
    public void FormatDescription_ReturnsFullDescription()
    {
        // Arrange
        var requirement = ArmorTrainingRequirement.ForMaster(ArmorCategory.Shields);

        // Act
        var description = requirement.FormatDescription();

        // Assert
        description.Should().Contain("Shields to Master");
        description.Should().Contain("500 PS");
        description.Should().Contain("8 weeks");
        description.Should().Contain("Level 10+");
    }
}
