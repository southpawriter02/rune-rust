// ═══════════════════════════════════════════════════════════════════════════════
// TierUnlockResultTests.cs
// Unit tests for TierUnlockResult (v0.17.4e).
// Tests cover factory methods, property values, and display methods for both
// success and failure scenarios.
// Version: 0.17.4e
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TierUnlockResult"/> (v0.17.4e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Succeeded factory method with correct property initialization</description></item>
///   <item><description>Failed factory method with failure reason</description></item>
///   <item><description>GetSummary display method for success and failure</description></item>
///   <item><description>PP cost tracking in results</description></item>
///   <item><description>Granted abilities list population</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class TierUnlockResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a sample Tier 2 ability tier for testing.
    /// </summary>
    private static SpecializationAbilityTier CreateSampleTier2()
    {
        var abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "iron-curtain", "Iron Curtain",
                "Create a wall that blocks all projectiles.", 2, "block_charges", 0, 0),
            SpecializationAbility.CreateActive(
                "retaliatory-strike", "Retaliatory Strike",
                "When you successfully block, make a free attack.", 0, "", 2, 0),
            SpecializationAbility.CreatePassive(
                "unbreakable", "Unbreakable",
                "You cannot be knocked prone or pushed.", 0)
        };

        return SpecializationAbilityTier.CreateTier2(
            "Unleashed Beast",
            abilities);
    }

    /// <summary>
    /// Creates a sample Tier 3 ability tier for testing.
    /// </summary>
    private static SpecializationAbilityTier CreateSampleTier3()
    {
        var abilities = new List<SpecializationAbility>
        {
            SpecializationAbility.CreateActive(
                "last-stand", "Last Stand",
                "Double your current HP for 3 rounds.", 3, "block_charges", 0, 0),
            SpecializationAbility.CreatePassive(
                "living-fortress", "Living Fortress",
                "All allies within 2 squares gain Soak +1.", 0)
        };

        return SpecializationAbilityTier.CreateTier3(
            "Avatar of Defense",
            abilities);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SUCCEEDED FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Succeeded with Tier 2 sets all properties correctly.
    /// </summary>
    [Test]
    public void Succeeded_WithTier2_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var tier = CreateSampleTier2();

        // Act
        var result = TierUnlockResult.Succeeded(tier);

        // Assert
        result.Success.Should().BeTrue();
        result.UnlockedTier.Should().NotBeNull();
        result.UnlockedTier!.Value.Tier.Should().Be(2);
        result.GrantedAbilities.Should().HaveCount(3);
        result.PpCost.Should().Be(2);
        result.FailureReason.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Succeeded with Tier 3 has correct PP cost.
    /// </summary>
    [Test]
    public void Succeeded_WithTier3_HasCorrectPpCost()
    {
        // Arrange
        var tier = CreateSampleTier3();

        // Act
        var result = TierUnlockResult.Succeeded(tier);

        // Assert
        result.Success.Should().BeTrue();
        result.PpCost.Should().Be(3);
        result.GrantedAbilities.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that Succeeded populates granted abilities from the tier.
    /// </summary>
    [Test]
    public void Succeeded_PopulatesGrantedAbilitiesFromTier()
    {
        // Arrange
        var tier = CreateSampleTier2();

        // Act
        var result = TierUnlockResult.Succeeded(tier);

        // Assert
        result.GrantedAbilities.Should().BeSameAs(tier.Abilities);
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
        var result = TierUnlockResult.Failed("Tier 1 must be unlocked first");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be("Tier 1 must be unlocked first");
        result.UnlockedTier.Should().BeNull();
        result.GrantedAbilities.Should().BeEmpty();
        result.PpCost.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Failed with insufficient PP message is descriptive.
    /// </summary>
    [Test]
    public void Failed_InsufficientPP_HasDescriptiveMessage()
    {
        // Act
        var result = TierUnlockResult.Failed("Requires 2 PP (0 available)");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("PP");
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
        var tier = CreateSampleTier2();
        var result = TierUnlockResult.Succeeded(tier);

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().Contain("Tier 2");
        summary.Should().Contain("Unleashed Beast");
        summary.Should().Contain("3"); // ability count
        summary.Should().Contain("2"); // PP cost
    }

    /// <summary>
    /// Verifies GetSummary returns failure reason on failure.
    /// </summary>
    [Test]
    public void GetSummary_OnFailure_ReturnsFailureReason()
    {
        // Arrange
        var result = TierUnlockResult.Failed("Test failure reason");

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().StartWith("Failed:");
        summary.Should().Contain("Test failure reason");
    }

    /// <summary>
    /// Verifies GetSummary for Tier 3 includes correct tier number.
    /// </summary>
    [Test]
    public void GetSummary_WithTier3_IncludesCorrectTierNumber()
    {
        // Arrange
        var tier = CreateSampleTier3();
        var result = TierUnlockResult.Succeeded(tier);

        // Act
        var summary = result.GetSummary();

        // Assert
        summary.Should().Contain("Tier 3");
    }
}
