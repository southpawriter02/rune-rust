using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the skill grant extension of <see cref="BackgroundDefinition"/>.
/// </summary>
/// <remarks>
/// Verifies that BackgroundDefinition correctly stores, accesses, and summarizes
/// skill grants added in v0.17.1b. Tests cover the SkillGrants property,
/// helper methods, and background-specific skill grant configurations.
/// </remarks>
[TestFixture]
public class BackgroundDefinitionSkillGrantTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SKILL GRANTS PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with skill grants stores them correctly.
    /// </summary>
    [Test]
    public void Create_WithSkillGrants_StoresGrants()
    {
        // Arrange
        var skillGrants = new List<BackgroundSkillGrant>
        {
            BackgroundSkillGrant.Permanent("craft", 2),
            BackgroundSkillGrant.Permanent("might", 1)
        };

        // Act
        var definition = CreateDefinitionWithSkillGrants(skillGrants);

        // Assert
        definition.SkillGrants.Should().HaveCount(2);
        definition.SkillGrants[0].SkillId.Should().Be("craft");
        definition.SkillGrants[0].BonusAmount.Should().Be(2);
        definition.SkillGrants[1].SkillId.Should().Be("might");
        definition.SkillGrants[1].BonusAmount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that Create without skill grants defaults to an empty list.
    /// </summary>
    [Test]
    public void Create_WithoutSkillGrants_DefaultsToEmptyList()
    {
        // Act
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement");

        // Assert
        definition.SkillGrants.Should().BeEmpty(
            "skill grants should default to an empty list when not provided");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasSkillGrants returns true when grants are defined.
    /// </summary>
    [Test]
    public void HasSkillGrants_WithGrants_ReturnsTrue()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act & Assert
        definition.HasSkillGrants().Should().BeTrue(
            "Village Smith has 2 skill grants defined");
    }

    /// <summary>
    /// Verifies that HasSkillGrants returns false when no grants exist.
    /// </summary>
    [Test]
    public void HasSkillGrants_WithoutGrants_ReturnsFalse()
    {
        // Arrange
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson");

        // Act & Assert
        definition.HasSkillGrants().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetPrimarySkillGrant returns the grant with the highest bonus.
    /// </summary>
    [Test]
    public void GetPrimarySkillGrant_ReturnsHighestBonus()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act
        var primary = definition.GetPrimarySkillGrant();

        // Assert
        primary.Should().NotBeNull();
        primary!.Value.SkillId.Should().Be("craft",
            "Craft is the primary skill (+2) for Village Smith");
        primary.Value.BonusAmount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetSecondarySkillGrant returns the grant with the second highest bonus.
    /// </summary>
    [Test]
    public void GetSecondarySkillGrant_ReturnsSecondHighestBonus()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act
        var secondary = definition.GetSecondarySkillGrant();

        // Assert
        secondary.Should().NotBeNull();
        secondary!.Value.SkillId.Should().Be("might",
            "Might is the secondary skill (+1) for Village Smith");
        secondary.Value.BonusAmount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that GetPrimarySkillGrant returns null when no grants exist.
    /// </summary>
    [Test]
    public void GetPrimarySkillGrant_WithNoGrants_ReturnsNull()
    {
        // Arrange
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson");

        // Act & Assert
        definition.GetPrimarySkillGrant().Should().BeNull();
    }

    /// <summary>
    /// Verifies that GetSkillGrantSummary formats grants as a readable string.
    /// </summary>
    [Test]
    public void GetSkillGrantSummary_FormatsCorrectly()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act
        var summary = definition.GetSkillGrantSummary();

        // Assert
        summary.Should().Contain("craft +2");
        summary.Should().Contain("might +1");
        summary.Should().Contain(", ",
            "grants should be comma-separated");
    }

    /// <summary>
    /// Verifies that GetSkillGrantSummary returns fallback text when no grants exist.
    /// </summary>
    [Test]
    public void GetSkillGrantSummary_WithNoGrants_ReturnsFallbackText()
    {
        // Arrange
        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson");

        // Act
        var summary = definition.GetSkillGrantSummary();

        // Assert
        summary.Should().Be("No skill bonuses");
    }

    /// <summary>
    /// Verifies that GetTotalSkillBonus sums all bonus amounts.
    /// </summary>
    [Test]
    public void GetTotalSkillBonus_SumsAllBonuses()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act
        var total = definition.GetTotalSkillBonus();

        // Assert
        total.Should().Be(3,
            "Village Smith grants Craft +2 and Might +1, totaling +3");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BACKGROUND-SPECIFIC TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Village Smith grants Craft +2 and Might +1 as specified.
    /// </summary>
    [Test]
    public void VillageSmith_GrantsCraftPlus2MightPlus1()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Assert
        definition.SkillGrants.Should().HaveCount(2,
            "Village Smith grants exactly 2 skills");

        var craftGrant = definition.SkillGrants.First(g => g.SkillId == "craft");
        craftGrant.BonusAmount.Should().Be(2, "Craft is the primary skill (+2)");
        craftGrant.IsPermanent.Should().BeTrue();

        var mightGrant = definition.SkillGrants.First(g => g.SkillId == "might");
        mightGrant.BonusAmount.Should().Be(1, "Might is the secondary skill (+1)");
        mightGrant.IsPermanent.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString includes skill grant count.
    /// </summary>
    [Test]
    public void ToString_IncludesSkillGrantCount()
    {
        // Arrange
        var definition = CreateVillageSmithWithSkillGrants();

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Contain("Skills=2",
            "ToString should include the number of skill grants");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a Village Smith definition with standard skill grants for testing.
    /// </summary>
    /// <returns>A BackgroundDefinition with Craft +2 and Might +1 skill grants.</returns>
    private static BackgroundDefinition CreateVillageSmithWithSkillGrants()
    {
        return CreateDefinitionWithSkillGrants(
            new List<BackgroundSkillGrant>
            {
                BackgroundSkillGrant.Permanent("craft", 2),
                BackgroundSkillGrant.Permanent("might", 1)
            });
    }

    /// <summary>
    /// Creates a BackgroundDefinition with specified skill grants for testing.
    /// </summary>
    /// <param name="skillGrants">The skill grants to include.</param>
    /// <returns>A BackgroundDefinition with the specified skill grants.</returns>
    private static BackgroundDefinition CreateDefinitionWithSkillGrants(
        IReadOnlyList<BackgroundSkillGrant> skillGrants)
    {
        return BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace.",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement",
            new List<string>
            {
                "Recognize craftsmanship in ruins",
                "Repair broken equipment more easily",
                "Clan smiths may offer discounts or quests"
            },
            skillGrants);
    }
}
