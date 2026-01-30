// ═══════════════════════════════════════════════════════════════════════════════
// AttributeDescriptionTests.cs
// Unit tests for the AttributeDescription value object verifying creation,
// validation, normalization, and helper method behavior.
// Version: 0.17.2a
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="AttributeDescription"/> value object.
/// </summary>
/// <remarks>
/// <para>
/// Verifies that AttributeDescription correctly:
/// </para>
/// <list type="bullet">
///   <item><description>Creates instances with valid parameters</description></item>
///   <item><description>Rejects null or whitespace string parameters</description></item>
///   <item><description>Normalizes display names to uppercase</description></item>
///   <item><description>Performs case-insensitive stat and skill lookups</description></item>
///   <item><description>Generates formatted summary and tooltip strings</description></item>
///   <item><description>Handles empty affected stats and skills gracefully</description></item>
/// </list>
/// </remarks>
/// <seealso cref="AttributeDescription"/>
/// <seealso cref="CoreAttribute"/>
[TestFixture]
public class AttributeDescriptionTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> produces a valid
    /// description with all properties correctly assigned when given valid parameters.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesDescription()
    {
        // Arrange
        var affectedStats = new List<string> { "Melee Damage", "Stamina" };
        var affectedSkills = new List<string> { "Combat", "Athletics" };

        // Act
        var description = AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            "Physical power and strength",
            "Might represents your character's physical power. It affects melee damage, carrying capacity, and physical feats of strength.",
            affectedStats,
            affectedSkills);

        // Assert
        description.Attribute.Should().Be(CoreAttribute.Might);
        description.DisplayName.Should().Be("MIGHT");
        description.ShortDescription.Should().Be("Physical power and strength");
        description.DetailedDescription.Should().Contain("physical power");
        description.AffectedStats.Should().HaveCount(2);
        description.AffectedSkills.Should().HaveCount(2);
        description.HasAffectedStats.Should().BeTrue();
        description.HasAffectedSkills.Should().BeTrue();
        description.AffectedStatCount.Should().Be(2);
        description.AffectedSkillCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> throws
    /// <see cref="ArgumentException"/> when the display name is null.
    /// </summary>
    [Test]
    public void Create_WithNullDisplayName_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AttributeDescription.Create(
            CoreAttribute.Might,
            null!,
            "Short description here",
            "Detailed description that is long enough to pass validation.");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> throws
    /// <see cref="ArgumentException"/> when the short description is null or whitespace.
    /// </summary>
    [Test]
    public void Create_WithNullShortDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            null!,
            "Detailed description that is long enough to pass validation.");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> throws
    /// <see cref="ArgumentException"/> when the detailed description is null or whitespace.
    /// </summary>
    [Test]
    public void Create_WithNullDetailedDescription_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            "Short description here",
            null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> normalizes the
    /// display name to uppercase regardless of input casing.
    /// </summary>
    [Test]
    public void Create_NormalizesDisplayNameToUppercase()
    {
        // Arrange & Act
        var description = AttributeDescription.Create(
            CoreAttribute.Might,
            "might",
            "Physical power and strength",
            "Might represents your character's physical power. It affects melee damage.");

        // Assert
        description.DisplayName.Should().Be("MIGHT");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.Create"/> defaults null
    /// affected stats and skills lists to empty collections.
    /// </summary>
    [Test]
    public void Create_WithNullLists_DefaultsToEmptyCollections()
    {
        // Arrange & Act
        var description = AttributeDescription.Create(
            CoreAttribute.Wits,
            "WITS",
            "Perception and knowledge",
            "Wits represents your character's mental acuity and learned knowledge.");

        // Assert
        description.AffectedStats.Should().BeEmpty();
        description.AffectedSkills.Should().BeEmpty();
        description.HasAffectedStats.Should().BeFalse();
        description.HasAffectedSkills.Should().BeFalse();
        description.AffectedStatCount.Should().Be(0);
        description.AffectedSkillCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.AffectsStat"/> performs
    /// case-insensitive matching and returns the correct result.
    /// </summary>
    [Test]
    public void AffectsStat_WithMatchingStat_ReturnsTrue()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Sturdiness,
            "STURDINESS",
            "Endurance and physical resilience",
            "Sturdiness represents your character's toughness and endurance.",
            new List<string> { "Max HP", "Soak" });

        // Act & Assert
        description.AffectsStat("Max HP").Should().BeTrue();
        description.AffectsStat("max hp").Should().BeTrue(); // Case insensitive
        description.AffectsStat("MAX HP").Should().BeTrue(); // Case insensitive
        description.AffectsStat("Initiative").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.AffectsSkill"/> performs
    /// case-insensitive matching and returns the correct result.
    /// </summary>
    [Test]
    public void AffectsSkill_WithMatchingSkill_ReturnsTrue()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Finesse,
            "FINESSE",
            "Agility and precision",
            "Finesse represents your character's agility and hand-eye coordination.",
            affectedSkills: new List<string> { "Stealth", "Acrobatics", "Lockpicking" });

        // Act & Assert
        description.AffectsSkill("Stealth").Should().BeTrue();
        description.AffectsSkill("stealth").Should().BeTrue(); // Case insensitive
        description.AffectsSkill("Combat").Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.GetAffectedStatsSummary"/>
    /// returns a comma-separated formatted string of affected stats.
    /// </summary>
    [Test]
    public void GetAffectedStatsSummary_ReturnsFormattedString()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            "Physical power and raw strength",
            "Might represents your character's physical power. It affects melee damage.",
            new List<string> { "Melee Damage", "Carrying Capacity" });

        // Act
        var summary = description.GetAffectedStatsSummary();

        // Assert
        summary.Should().Be("Melee Damage, Carrying Capacity");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.GetAffectedStatsSummary"/>
    /// returns "None" when the attribute has no affected stats.
    /// </summary>
    [Test]
    public void GetAffectedStatsSummary_WithNoStats_ReturnsNone()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            "Physical power and raw strength",
            "Might represents your character's physical power. It affects melee damage.");

        // Act
        var summary = description.GetAffectedStatsSummary();

        // Assert
        summary.Should().Be("None");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.GetAffectedSkillsSummary"/>
    /// returns a comma-separated formatted string of affected skills.
    /// </summary>
    [Test]
    public void GetAffectedSkillsSummary_ReturnsFormattedString()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Will,
            "WILL",
            "Mental fortitude and magical affinity",
            "Will represents your character's mental fortitude and connection to the Aether.",
            affectedSkills: new List<string> { "Rhetoric", "Concentration", "Willpower" });

        // Act
        var summary = description.GetAffectedSkillsSummary();

        // Assert
        summary.Should().Be("Rhetoric, Concentration, Willpower");
    }

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.GetTooltipText"/> returns
    /// a formatted tooltip containing the display name, short description,
    /// and detailed description.
    /// </summary>
    [Test]
    public void GetTooltipText_ReturnsFormattedTooltip()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Will,
            "WILL",
            "Mental fortitude",
            "Full detailed explanation here.");

        // Act
        var tooltip = description.GetTooltipText();

        // Assert
        tooltip.Should().Contain("WILL");
        tooltip.Should().Contain("Mental fortitude");
        tooltip.Should().Contain("Full detailed explanation");
        tooltip.Should().Be("WILL\nMental fortitude\n\nFull detailed explanation here.");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that <see cref="AttributeDescription.ToString"/> returns
    /// the display name and short description in the expected format.
    /// </summary>
    [Test]
    public void ToString_ReturnsDisplayNameAndShortDescription()
    {
        // Arrange
        var description = AttributeDescription.Create(
            CoreAttribute.Might,
            "MIGHT",
            "Physical power and raw strength",
            "Might represents your character's physical power. It affects melee damage.");

        // Act
        var result = description.ToString();

        // Assert
        result.Should().Be("MIGHT: Physical power and raw strength");
    }
}
