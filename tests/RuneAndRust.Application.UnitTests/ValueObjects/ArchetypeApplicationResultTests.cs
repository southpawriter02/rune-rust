// ═══════════════════════════════════════════════════════════════════════════════
// ArchetypeApplicationResultTests.cs
// Unit tests for ArchetypeApplicationResult (v0.17.3f).
// Tests cover factory methods, display methods, and property verification
// for both success and failure scenarios.
// Version: 0.17.3f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="ArchetypeApplicationResult"/> (v0.17.3f).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Succeeded factory creates result with all properties populated</description></item>
///   <item><description>Failed factory creates result with failure reason and empty grants</description></item>
///   <item><description>AlreadyHasArchetype factory creates descriptive failure message</description></item>
///   <item><description>CharacterRequired static property creates null character failure</description></item>
///   <item><description>GetSummary formats correctly for success and failure cases</description></item>
///   <item><description>GetGrantedAbilitiesList returns formatted ability display strings</description></item>
///   <item><description>GetSpecializationsList returns formatted specialization display strings</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class ArchetypeApplicationResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SUCCEEDED FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Succeeded factory creates a result with Success=true and
    /// all properties populated correctly.
    /// </summary>
    [Test]
    public void Succeeded_WithValidData_CreatesSuccessfulResult()
    {
        // Arrange
        var bonuses = ArchetypeResourceBonuses.Warrior;
        var abilities = new List<ArchetypeAbilityGrant>
        {
            ArchetypeAbilityGrant.CreateActive(
                "power-strike", "Power Strike", "Basic melee attack."),
            ArchetypeAbilityGrant.CreateStance(
                "defensive-stance", "Defensive Stance", "+2 Defense, -1 Attack."),
            ArchetypeAbilityGrant.CreatePassive(
                "iron-will", "Iron Will", "+10% HP regeneration while resting.")
        };
        var specializations = ArchetypeSpecializationMapping.Warrior;

        // Act
        var result = ArchetypeApplicationResult.Succeeded(
            Archetype.Warrior, bonuses, abilities, specializations);

        // Assert
        result.Success.Should().BeTrue();
        result.AppliedArchetype.Should().Be(Archetype.Warrior);
        result.ResourceBonusesApplied.Should().Be(bonuses);
        result.AbilitiesGranted.Should().HaveCount(3);
        result.AvailableSpecializations.Should().Be(specializations);
        result.FailureReason.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Succeeded factory populates ability list with correct ability data.
    /// </summary>
    [Test]
    public void Succeeded_AbilitiesGranted_ContainsCorrectAbilities()
    {
        // Arrange
        var abilities = new List<ArchetypeAbilityGrant>
        {
            ArchetypeAbilityGrant.CreateActive(
                "aether-bolt", "Aether Bolt", "Ranged Aether attack.")
        };

        // Act
        var result = ArchetypeApplicationResult.Succeeded(
            Archetype.Mystic,
            ArchetypeResourceBonuses.Mystic,
            abilities,
            ArchetypeSpecializationMapping.Mystic);

        // Assert
        result.AbilitiesGranted.Should().ContainSingle();
        result.AbilitiesGranted[0].AbilityId.Should().Be("aether-bolt");
        result.AbilitiesGranted[0].AbilityName.Should().Be("Aether Bolt");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FAILED FACTORY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Failed factory creates a result with Success=false,
    /// the specified failure reason, and empty grant lists.
    /// </summary>
    [Test]
    public void Failed_WithReason_CreatesFailedResult()
    {
        // Act
        var result = ArchetypeApplicationResult.Failed("Test failure reason");

        // Assert
        result.Success.Should().BeFalse();
        result.AppliedArchetype.Should().BeNull();
        result.ResourceBonusesApplied.Should().BeNull();
        result.AbilitiesGranted.Should().BeEmpty();
        result.AvailableSpecializations.Should().BeNull();
        result.FailureReason.Should().Be("Test failure reason");
    }

    /// <summary>
    /// Verifies that AlreadyHasArchetype factory creates a failure result
    /// containing the existing archetype name and permanence message.
    /// </summary>
    [Test]
    public void AlreadyHasArchetype_ContainsArchetypeNameAndPermanenceMessage()
    {
        // Act
        var result = ArchetypeApplicationResult.AlreadyHasArchetype(Archetype.Skirmisher);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Skirmisher");
        result.FailureReason.Should().Contain("permanent choice");
    }

    /// <summary>
    /// Verifies that CharacterRequired static property creates a failure result
    /// with a descriptive message about needing a character.
    /// </summary>
    [Test]
    public void CharacterRequired_CreatesFailedResultWithDescriptiveMessage()
    {
        // Act
        var result = ArchetypeApplicationResult.CharacterRequired;

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("character is required");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetSummary TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSummary on a successful result contains the applied
    /// archetype name, resource bonuses, abilities count, and specializations count.
    /// </summary>
    [Test]
    public void GetSummary_OnSuccess_ContainsAppliedArchetypeAndDetails()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Succeeded(
            Archetype.Warrior,
            ArchetypeResourceBonuses.Warrior,
            new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "power-strike", "Power Strike", "Attack."),
                ArchetypeAbilityGrant.CreateStance(
                    "defensive-stance", "Defensive Stance", "Defend."),
                ArchetypeAbilityGrant.CreatePassive(
                    "iron-will", "Iron Will", "Passive.")
            },
            ArchetypeSpecializationMapping.Warrior);

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().Contain("Applied Warrior");
        summary.Should().Contain("Abilities Granted: 3");
        summary.Should().Contain("Specializations Available: 6");
    }

    /// <summary>
    /// Verifies that GetSummary on a failed result contains the failure prefix
    /// and the specific failure reason.
    /// </summary>
    [Test]
    public void GetSummary_OnFailure_ContainsFailedPrefixAndReason()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Failed("Character already has archetype");

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().Be("Failed: Character already has archetype");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetGrantedAbilitiesList TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetGrantedAbilitiesList returns formatted ability display strings
    /// using the short display format (e.g., "Power Strike [ACTIVE]").
    /// </summary>
    [Test]
    public void GetGrantedAbilitiesList_ReturnsFormattedAbilityDisplayStrings()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Succeeded(
            Archetype.Warrior,
            ArchetypeResourceBonuses.Warrior,
            new List<ArchetypeAbilityGrant>
            {
                ArchetypeAbilityGrant.CreateActive(
                    "power-strike", "Power Strike", "Attack."),
                ArchetypeAbilityGrant.CreatePassive(
                    "iron-will", "Iron Will", "Passive.")
            },
            ArchetypeSpecializationMapping.Warrior);

        // Act
        var list = result.GetGrantedAbilitiesList();

        // Assert
        list.Should().HaveCount(2);
        list.Should().Contain("Power Strike [ACTIVE]");
        list.Should().Contain("Iron Will [PASSIVE]");
    }

    /// <summary>
    /// Verifies that GetGrantedAbilitiesList returns an empty list on failure.
    /// </summary>
    [Test]
    public void GetGrantedAbilitiesList_OnFailure_ReturnsEmptyList()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Failed("Test failure");

        // Act
        var list = result.GetGrantedAbilitiesList();

        // Assert
        list.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetSpecializationsList TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSpecializationsList returns formatted specialization names
    /// from the available specializations mapping.
    /// </summary>
    [Test]
    public void GetSpecializationsList_OnSuccess_ReturnsFormattedSpecializationNames()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Succeeded(
            Archetype.Mystic,
            ArchetypeResourceBonuses.Mystic,
            new List<ArchetypeAbilityGrant>(),
            ArchetypeSpecializationMapping.Mystic);

        // Act
        var list = result.GetSpecializationsList();

        // Assert
        list.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that GetSpecializationsList returns an empty list on failure.
    /// </summary>
    [Test]
    public void GetSpecializationsList_OnFailure_ReturnsEmptyList()
    {
        // Arrange
        var result = ArchetypeApplicationResult.Failed("Test failure");

        // Act
        var list = result.GetSpecializationsList();

        // Assert
        list.Should().BeEmpty();
    }
}
