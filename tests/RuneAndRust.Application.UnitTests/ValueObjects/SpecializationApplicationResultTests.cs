// ═══════════════════════════════════════════════════════════════════════════════
// SpecializationApplicationResultTests.cs
// Unit tests for SpecializationApplicationResult (v0.17.4e).
// Tests cover factory methods, property values, and display methods for both
// success and failure scenarios.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SpecializationApplicationResult"/> (v0.17.4e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Succeeded factory method with correct property initialization</description></item>
///   <item><description>Failed factory method with failure reason</description></item>
///   <item><description>AlreadyHasSpecialization convenience factory</description></item>
///   <item><description>CharacterRequired static property</description></item>
///   <item><description>ArchetypeMismatch convenience factory</description></item>
///   <item><description>GetSummary display method for success and failure</description></item>
///   <item><description>GetGrantedAbilitiesList display method</description></item>
///   <item><description>Heretical path detection and Corruption warning</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class SpecializationApplicationResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid Berserkr specialization definition for testing.
    /// </summary>
    private static SpecializationDefinition CreateBerserkrDefinition()
    {
        var resource = SpecialResourceDefinition.Create(
            "rage", "Rage", 0, 100, 0, 0, 5,
            "Accumulated fury that powers devastating abilities");

        return SpecializationDefinition.Create(
            SpecializationId.Berserkr,
            "Berserkr",
            "Fury Unleashed",
            "The Berserkr channels primal rage into devastating attacks.",
            "Embrace the chaos within.",
            Archetype.Warrior,
            SpecializationPathType.Heretical,
            0,
            resource);
    }

    /// <summary>
    /// Creates a valid Skald specialization definition for testing (Coherent, no resource).
    /// </summary>
    private static SpecializationDefinition CreateSkaldDefinition()
    {
        return SpecializationDefinition.Create(
            SpecializationId.Skald,
            "Skald",
            "Voice of Legends",
            "The Skald inspires allies through song and story.",
            "Let your voice carry the weight of legend.",
            Archetype.Adept,
            SpecializationPathType.Coherent,
            0);
    }

    /// <summary>
    /// Creates a sample list of Tier 1 abilities for testing.
    /// </summary>
    private static IReadOnlyList<SpecializationAbility> CreateSampleAbilities()
    {
        return new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "rage-strike", "Rage Strike",
                "A devastating rage-powered attack.", 20, "rage", 0, 5),
            SpecializationAbility.CreateActive(
                "blood-frenzy", "Blood Frenzy",
                "Enter a frenzy that increases attack speed.", 30, "rage", 3, 0),
            SpecializationAbility.CreatePassive(
                "primal-toughness", "Primal Toughness",
                "Gain bonus HP from Constitution.", 0)
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUCCEEDED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Succeeded creates a result with all properties set correctly.
    /// </summary>
    [Test]
    public void Succeeded_WithBerserkr_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var definition = CreateBerserkrDefinition();
        var abilities = CreateSampleAbilities();

        // Act
        var result = SpecializationApplicationResult.Succeeded(
            definition, abilities, specialResourceInitialized: true, ppCost: 0);

        // Assert
        result.Success.Should().BeTrue();
        result.AppliedSpecializationId.Should().Be(SpecializationId.Berserkr);
        result.AppliedDefinition.Should().Be(definition);
        result.GrantedAbilities.Should().HaveCount(3);
        result.SpecialResourceInitialized.Should().BeTrue();
        result.IsHeretical.Should().BeTrue();
        result.CorruptionWarning.Should().NotBeNullOrEmpty();
        result.PpCost.Should().Be(0);
        result.FailureReason.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Succeeded with a Coherent spec has no Heretical warnings.
    /// </summary>
    [Test]
    public void Succeeded_WithCoherentSpec_HasNoCorruptionWarning()
    {
        // Arrange
        var definition = CreateSkaldDefinition();
        var abilities = CreateSampleAbilities();

        // Act
        var result = SpecializationApplicationResult.Succeeded(
            definition, abilities, specialResourceInitialized: false, ppCost: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.IsHeretical.Should().BeFalse();
        result.CorruptionWarning.Should().BeNull();
        result.SpecialResourceInitialized.Should().BeFalse();
        result.PpCost.Should().Be(3);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FAILED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Failed creates a result with correct failure properties.
    /// </summary>
    [Test]
    public void Failed_WithReason_SetsFailureProperties()
    {
        // Act
        var result = SpecializationApplicationResult.Failed("Requires Warrior archetype");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be("Requires Warrior archetype");
        result.AppliedSpecializationId.Should().BeNull();
        result.AppliedDefinition.Should().BeNull();
        result.GrantedAbilities.Should().BeEmpty();
        result.SpecialResourceInitialized.Should().BeFalse();
        result.IsHeretical.Should().BeFalse();
        result.CorruptionWarning.Should().BeNull();
        result.PpCost.Should().Be(0);
    }

    /// <summary>
    /// Verifies AlreadyHasSpecialization creates correct failure message.
    /// </summary>
    [Test]
    public void AlreadyHasSpecialization_CreatesDescriptiveFailureMessage()
    {
        // Act
        var result = SpecializationApplicationResult.AlreadyHasSpecialization(
            SpecializationId.Berserkr);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Berserkr");
        result.FailureReason.Should().Contain("already has");
    }

    /// <summary>
    /// Verifies CharacterRequired static property creates correct failure.
    /// </summary>
    [Test]
    public void CharacterRequired_CreatesFailureResult()
    {
        // Act
        var result = SpecializationApplicationResult.CharacterRequired;

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("character is required");
    }

    /// <summary>
    /// Verifies ArchetypeMismatch creates correct failure message.
    /// </summary>
    [Test]
    public void ArchetypeMismatch_CreatesDescriptiveFailureMessage()
    {
        // Act
        var result = SpecializationApplicationResult.ArchetypeMismatch(
            Archetype.Warrior, "mystic");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Warrior");
        result.FailureReason.Should().Contain("mystic");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies GetSummary returns formatted success summary.
    /// </summary>
    [Test]
    public void GetSummary_OnSuccess_ReturnsFormattedSummary()
    {
        // Arrange
        var definition = CreateBerserkrDefinition();
        var abilities = CreateSampleAbilities();
        var result = SpecializationApplicationResult.Succeeded(
            definition, abilities, true, 0);

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().Contain("Berserkr");
        summary.Should().Contain("Heretical");
        summary.Should().Contain("3");
        summary.Should().Contain("WARNING");
    }

    /// <summary>
    /// Verifies GetSummary returns failure reason on failure.
    /// </summary>
    [Test]
    public void GetSummary_OnFailure_ReturnsFailureReason()
    {
        // Arrange
        var result = SpecializationApplicationResult.Failed("Test failure reason");

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().StartWith("Failed:");
        summary.Should().Contain("Test failure reason");
    }

    /// <summary>
    /// Verifies GetGrantedAbilitiesList returns formatted ability strings.
    /// </summary>
    [Test]
    public void GetGrantedAbilitiesList_ReturnsFormattedAbilities()
    {
        // Arrange
        var definition = CreateBerserkrDefinition();
        var abilities = CreateSampleAbilities();
        var result = SpecializationApplicationResult.Succeeded(
            definition, abilities, true, 0);

        // Act
        var abilityList = result.GetGrantedAbilitiesList();

        // Assert
        abilityList.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies GetGrantedAbilitiesList returns empty list on failure.
    /// </summary>
    [Test]
    public void GetGrantedAbilitiesList_OnFailure_ReturnsEmptyList()
    {
        // Arrange
        var result = SpecializationApplicationResult.Failed("reason");

        // Act
        var abilityList = result.GetGrantedAbilitiesList();

        // Assert
        abilityList.Should().BeEmpty();
    }
}
