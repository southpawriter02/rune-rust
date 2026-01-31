// ═══════════════════════════════════════════════════════════════════════════════
// CharacterCreationStepTests.cs
// Unit tests for the CharacterCreationStep enum and its extension methods,
// including step count, ordering, navigation logic, permanent choice
// identification, display names, and thematic descriptions.
// Version: 0.17.5a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.UnitTests.Enums;

/// <summary>
/// Unit tests for the <see cref="CharacterCreationStep"/> enum and its
/// <see cref="CharacterCreationStepExtensions"/> extension methods.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that:
/// </para>
/// <list type="bullet">
///   <item><description>CharacterCreationStep has exactly 6 values (Lineage through Summary)</description></item>
///   <item><description>Enum values are assigned 0-5 for arithmetic navigation</description></item>
///   <item><description>GetStepNumber returns 1-based numbers (1-6)</description></item>
///   <item><description>GetTotalSteps returns 6</description></item>
///   <item><description>CanGoBack returns false only for Lineage</description></item>
///   <item><description>IsPermanentChoice returns true only for Archetype</description></item>
///   <item><description>GetNextStep and GetPreviousStep navigate correctly with null at boundaries</description></item>
///   <item><description>GetDisplayName returns correct human-readable names for all steps</description></item>
///   <item><description>GetDescription returns correct atmospheric descriptions for all steps</description></item>
/// </list>
/// </remarks>
/// <seealso cref="CharacterCreationStep"/>
/// <seealso cref="CharacterCreationStepExtensions"/>
[TestFixture]
public class CharacterCreationStepTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ENUM VALUE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the CharacterCreationStep enum has exactly six values,
    /// one for each step in the character creation wizard.
    /// </summary>
    [Test]
    public void CharacterCreationStep_ShouldHave_ExactlySixValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CharacterCreationStep>();

        // Assert
        values.Should().HaveCount(6);
        values.Should().Contain(CharacterCreationStep.Lineage);
        values.Should().Contain(CharacterCreationStep.Background);
        values.Should().Contain(CharacterCreationStep.Attributes);
        values.Should().Contain(CharacterCreationStep.Archetype);
        values.Should().Contain(CharacterCreationStep.Specialization);
        values.Should().Contain(CharacterCreationStep.Summary);
    }

    /// <summary>
    /// Verifies that each CharacterCreationStep enum value has the correct explicit
    /// integer assignment (0-5) for stable serialization and arithmetic navigation.
    /// </summary>
    /// <param name="step">The CharacterCreationStep enum value to verify.</param>
    /// <param name="expected">The expected integer value.</param>
    [Test]
    [TestCase(CharacterCreationStep.Lineage, 0)]
    [TestCase(CharacterCreationStep.Background, 1)]
    [TestCase(CharacterCreationStep.Attributes, 2)]
    [TestCase(CharacterCreationStep.Archetype, 3)]
    [TestCase(CharacterCreationStep.Specialization, 4)]
    [TestCase(CharacterCreationStep.Summary, 5)]
    public void CharacterCreationStep_HasCorrectIntegerValues(CharacterCreationStep step, int expected)
    {
        // Act
        var intValue = (int)step;

        // Assert
        intValue.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // STEP NUMBER TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetStepNumber returns the correct 1-based step number
    /// for all six creation steps.
    /// </summary>
    /// <param name="step">The CharacterCreationStep to get the number for.</param>
    /// <param name="expectedNumber">The expected 1-based step number.</param>
    [Test]
    [TestCase(CharacterCreationStep.Lineage, 1)]
    [TestCase(CharacterCreationStep.Background, 2)]
    [TestCase(CharacterCreationStep.Attributes, 3)]
    [TestCase(CharacterCreationStep.Archetype, 4)]
    [TestCase(CharacterCreationStep.Specialization, 5)]
    [TestCase(CharacterCreationStep.Summary, 6)]
    public void GetStepNumber_ReturnsCorrectOneBasedNumber(CharacterCreationStep step, int expectedNumber)
    {
        // Act
        var stepNumber = step.GetStepNumber();

        // Assert
        stepNumber.Should().Be(expectedNumber);
    }

    /// <summary>
    /// Verifies that GetTotalSteps returns the fixed count of 6.
    /// </summary>
    [Test]
    public void GetTotalSteps_ReturnsSix()
    {
        // Act
        var totalSteps = CharacterCreationStepExtensions.GetTotalSteps();

        // Assert
        totalSteps.Should().Be(6);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PERMANENT CHOICE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that IsPermanentChoice returns true only for the Archetype step,
    /// which is the only permanent choice in the character creation workflow.
    /// </summary>
    [Test]
    public void IsPermanentChoice_OnlyArchetypeReturnsTrue()
    {
        // Arrange
        var allSteps = Enum.GetValues<CharacterCreationStep>();

        // Act & Assert
        foreach (var step in allSteps)
        {
            var isPermanent = step.IsPermanentChoice();
            isPermanent.Should().Be(step == CharacterCreationStep.Archetype,
                because: $"{step} should {(step == CharacterCreationStep.Archetype ? "" : "not ")}be permanent");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION TESTS — CanGoBack
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanGoBack returns false only for the Lineage step (first step)
    /// and true for all other steps.
    /// </summary>
    [Test]
    public void CanGoBack_ReturnsFalseOnlyForLineage()
    {
        // Arrange
        var allSteps = Enum.GetValues<CharacterCreationStep>();

        // Act & Assert
        foreach (var step in allSteps)
        {
            var canGoBack = step.CanGoBack();
            canGoBack.Should().Be(step != CharacterCreationStep.Lineage,
                because: $"{step} should {(step == CharacterCreationStep.Lineage ? "not " : "")}allow going back");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NAVIGATION TESTS — GetNextStep / GetPreviousStep
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetNextStep returns null when called on the Summary step,
    /// as there is no step after the final confirmation screen.
    /// </summary>
    [Test]
    public void GetNextStep_ReturnsNullForSummary()
    {
        // Act
        var nextStep = CharacterCreationStep.Summary.GetNextStep();

        // Assert
        nextStep.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetNextStep returns the correct subsequent step for all
    /// non-final steps.
    /// </summary>
    /// <param name="step">The current step.</param>
    /// <param name="expectedNext">The expected next step.</param>
    [Test]
    [TestCase(CharacterCreationStep.Lineage, CharacterCreationStep.Background)]
    [TestCase(CharacterCreationStep.Background, CharacterCreationStep.Attributes)]
    [TestCase(CharacterCreationStep.Attributes, CharacterCreationStep.Archetype)]
    [TestCase(CharacterCreationStep.Archetype, CharacterCreationStep.Specialization)]
    [TestCase(CharacterCreationStep.Specialization, CharacterCreationStep.Summary)]
    public void GetNextStep_ReturnsCorrectNextStep(CharacterCreationStep step, CharacterCreationStep expectedNext)
    {
        // Act
        var nextStep = step.GetNextStep();

        // Assert
        nextStep.Should().Be(expectedNext);
    }

    /// <summary>
    /// Verifies that GetPreviousStep returns null when called on the Lineage step,
    /// as there is no step before the first selection screen.
    /// </summary>
    [Test]
    public void GetPreviousStep_ReturnsNullForLineage()
    {
        // Act
        var previousStep = CharacterCreationStep.Lineage.GetPreviousStep();

        // Assert
        previousStep.Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetPreviousStep returns the correct preceding step for all
    /// non-first steps.
    /// </summary>
    /// <param name="step">The current step.</param>
    /// <param name="expectedPrevious">The expected previous step.</param>
    [Test]
    [TestCase(CharacterCreationStep.Background, CharacterCreationStep.Lineage)]
    [TestCase(CharacterCreationStep.Attributes, CharacterCreationStep.Background)]
    [TestCase(CharacterCreationStep.Archetype, CharacterCreationStep.Attributes)]
    [TestCase(CharacterCreationStep.Specialization, CharacterCreationStep.Archetype)]
    [TestCase(CharacterCreationStep.Summary, CharacterCreationStep.Specialization)]
    public void GetPreviousStep_ReturnsCorrectPreviousStep(CharacterCreationStep step, CharacterCreationStep expectedPrevious)
    {
        // Act
        var previousStep = step.GetPreviousStep();

        // Assert
        previousStep.Should().Be(expectedPrevious);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY NAME TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDisplayName returns the correct human-readable name for
    /// each creation step, matching the creation-workflow.json configuration.
    /// </summary>
    /// <param name="step">The creation step to get the display name for.</param>
    /// <param name="expectedName">The expected display name string.</param>
    [Test]
    [TestCase(CharacterCreationStep.Lineage, "Choose Your Lineage")]
    [TestCase(CharacterCreationStep.Background, "Choose Your Background")]
    [TestCase(CharacterCreationStep.Attributes, "Allocate Attributes")]
    [TestCase(CharacterCreationStep.Archetype, "Choose Your Archetype")]
    [TestCase(CharacterCreationStep.Specialization, "Choose Your Specialization")]
    [TestCase(CharacterCreationStep.Summary, "Confirm Your Survivor")]
    public void GetDisplayName_ReturnsCorrectName(CharacterCreationStep step, string expectedName)
    {
        // Act
        var displayName = step.GetDisplayName();

        // Assert
        displayName.Should().Be(expectedName);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DESCRIPTION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetDescription returns the correct atmospheric description for
    /// each creation step, matching the creation-workflow.json configuration.
    /// </summary>
    /// <param name="step">The creation step to get the description for.</param>
    /// <param name="expectedDesc">The expected description string.</param>
    [Test]
    [TestCase(CharacterCreationStep.Lineage, "Your bloodline carries echoes of the world before.")]
    [TestCase(CharacterCreationStep.Background, "What were you before the world broke?")]
    [TestCase(CharacterCreationStep.Attributes, "Define your character's core capabilities.")]
    [TestCase(CharacterCreationStep.Archetype, "Your fundamental approach to survival.")]
    [TestCase(CharacterCreationStep.Specialization, "Your tactical identity and unique abilities.")]
    [TestCase(CharacterCreationStep.Summary, "Review your choices and begin your saga.")]
    public void GetDescription_ReturnsCorrectDescription(CharacterCreationStep step, string expectedDesc)
    {
        // Act
        var description = step.GetDescription();

        // Assert
        description.Should().Be(expectedDesc);
    }

    /// <summary>
    /// Verifies that all steps have non-empty display names and descriptions.
    /// Ensures no step is missing display information.
    /// </summary>
    [Test]
    public void AllSteps_HaveNonEmptyDisplayNamesAndDescriptions()
    {
        // Arrange
        var allSteps = Enum.GetValues<CharacterCreationStep>();

        // Act & Assert
        foreach (var step in allSteps)
        {
            step.GetDisplayName().Should().NotBeNullOrWhiteSpace(
                because: $"{step} should have a display name");
            step.GetDescription().Should().NotBeNullOrWhiteSpace(
                because: $"{step} should have a description");
        }
    }
}
