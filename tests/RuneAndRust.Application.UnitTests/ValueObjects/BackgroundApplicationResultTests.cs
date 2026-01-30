// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundApplicationResultTests.cs
// Unit tests for BackgroundApplicationResult, GrantedSkill, and GrantedEquipment
// value objects (v0.17.1e).
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Enums;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="BackgroundApplicationResult"/>, <see cref="GrantedSkill"/>,
/// and <see cref="GrantedEquipment"/> value objects (v0.17.1e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Successful result creation with grants populated</description></item>
///   <item><description>Failed result creation with error message</description></item>
///   <item><description>Skill summary formatting for display</description></item>
///   <item><description>Equipment summary formatting for display</description></item>
///   <item><description>GrantedSkill ToString formatting for all grant types</description></item>
///   <item><description>GrantedEquipment ToString formatting for equipped and inventory items</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class BackgroundApplicationResultTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Succeeded TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Succeeded creates a result with Success=true, correct background,
    /// populated grant lists, and the success message.
    /// </summary>
    [Test]
    public void Succeeded_CreatesSuccessfulResult()
    {
        // Arrange
        var skills = new[]
        {
            new GrantedSkill("craft", 2, SkillGrantType.Permanent),
            new GrantedSkill("might", 1, SkillGrantType.Permanent)
        };
        var equipment = new[]
        {
            new GrantedEquipment("smiths-hammer", 1, true, EquipmentSlot.Weapon),
            new GrantedEquipment("leather-apron", 1, true, EquipmentSlot.Armor)
        };

        // Act
        var result = BackgroundApplicationResult.Succeeded(
            Background.VillageSmith,
            skills,
            equipment,
            "Applied Village Smith background successfully");

        // Assert
        result.Success.Should().BeTrue();
        result.BackgroundId.Should().Be(Background.VillageSmith);
        result.SkillsGranted.Should().HaveCount(2);
        result.SkillsGranted[0].SkillId.Should().Be("craft");
        result.SkillsGranted[0].Amount.Should().Be(2);
        result.SkillsGranted[1].SkillId.Should().Be("might");
        result.SkillsGranted[1].Amount.Should().Be(1);
        result.EquipmentGranted.Should().HaveCount(2);
        result.EquipmentGranted[0].ItemId.Should().Be("smiths-hammer");
        result.EquipmentGranted[0].WasEquipped.Should().BeTrue();
        result.EquipmentGranted[1].ItemId.Should().Be("leather-apron");
        result.Messages.Should().ContainSingle()
            .Which.Should().Contain("Village Smith");
    }

    /// <summary>
    /// Verifies that Succeeded without a message creates a result with an empty messages list.
    /// </summary>
    [Test]
    public void Succeeded_WithoutMessage_HasEmptyMessagesList()
    {
        // Arrange & Act
        var result = BackgroundApplicationResult.Succeeded(
            Background.ClanGuard,
            Array.Empty<GrantedSkill>(),
            Array.Empty<GrantedEquipment>());

        // Assert
        result.Success.Should().BeTrue();
        result.Messages.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Failed TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Failed creates a result with Success=false, empty grant lists,
    /// and the error message.
    /// </summary>
    [Test]
    public void Failed_CreatesFailedResult()
    {
        // Arrange & Act
        var result = BackgroundApplicationResult.Failed(
            Background.VillageSmith,
            "Background 'VillageSmith' not found");

        // Assert
        result.Success.Should().BeFalse();
        result.BackgroundId.Should().Be(Background.VillageSmith);
        result.SkillsGranted.Should().BeEmpty();
        result.EquipmentGranted.Should().BeEmpty();
        result.Messages.Should().ContainSingle()
            .Which.Should().Contain("not found");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetSkillSummary TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetSkillSummary returns a comma-separated string of skill grants.
    /// </summary>
    [Test]
    public void GetSkillSummary_WithSkills_FormatsCorrectly()
    {
        // Arrange
        var skills = new[]
        {
            new GrantedSkill("craft", 2, SkillGrantType.Permanent),
            new GrantedSkill("might", 1, SkillGrantType.Permanent)
        };
        var result = BackgroundApplicationResult.Succeeded(
            Background.VillageSmith,
            skills,
            Array.Empty<GrantedEquipment>());

        // Act
        var summary = result.GetSkillSummary();

        // Assert
        summary.Should().Be("craft +2, might +1");
    }

    /// <summary>
    /// Verifies that GetSkillSummary returns fallback text when no skills were granted.
    /// </summary>
    [Test]
    public void GetSkillSummary_WithNoSkills_ReturnsFallbackText()
    {
        // Arrange
        var result = BackgroundApplicationResult.Failed(
            Background.VillageSmith,
            "Not found");

        // Act
        var summary = result.GetSkillSummary();

        // Assert
        summary.Should().Be("No skills granted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetEquipmentSummary TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetEquipmentSummary returns a comma-separated string of equipment grants.
    /// </summary>
    [Test]
    public void GetEquipmentSummary_WithEquipment_FormatsCorrectly()
    {
        // Arrange
        var equipment = new[]
        {
            new GrantedEquipment("shield", 1, true, EquipmentSlot.Shield),
            new GrantedEquipment("spear", 1, true, EquipmentSlot.Weapon)
        };
        var result = BackgroundApplicationResult.Succeeded(
            Background.ClanGuard,
            Array.Empty<GrantedSkill>(),
            equipment);

        // Act
        var summary = result.GetEquipmentSummary();

        // Assert
        summary.Should().Be("shield (Shield), spear (Weapon)");
    }

    /// <summary>
    /// Verifies that GetEquipmentSummary returns fallback text when no equipment was granted.
    /// </summary>
    [Test]
    public void GetEquipmentSummary_WithNoEquipment_ReturnsFallbackText()
    {
        // Arrange
        var result = BackgroundApplicationResult.Failed(
            Background.ClanGuard,
            "Not found");

        // Act
        var summary = result.GetEquipmentSummary();

        // Assert
        summary.Should().Be("No equipment granted");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GrantedSkill ToString TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GrantedSkill.ToString formats correctly for all three grant types.
    /// </summary>
    [TestCase(SkillGrantType.Permanent, "craft +2")]
    [TestCase(SkillGrantType.StartingBonus, "craft +2 (starting)")]
    [TestCase(SkillGrantType.Proficiency, "craft (proficient)")]
    public void GrantedSkill_ToString_FormatsCorrectlyByGrantType(
        SkillGrantType grantType,
        string expected)
    {
        // Arrange
        var skill = new GrantedSkill("craft", 2, grantType);

        // Act
        var result = skill.ToString();

        // Assert
        result.Should().Be(expected);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GrantedEquipment ToString TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GrantedEquipment.ToString formats equipped items with slot name.
    /// </summary>
    [Test]
    public void GrantedEquipment_ToString_EquippedItem_IncludesSlot()
    {
        // Arrange
        var equipment = new GrantedEquipment("spear", 1, true, EquipmentSlot.Weapon);

        // Act
        var result = equipment.ToString();

        // Assert
        result.Should().Be("spear (Weapon)");
    }

    /// <summary>
    /// Verifies that GrantedEquipment.ToString formats stackable items with quantity.
    /// </summary>
    [Test]
    public void GrantedEquipment_ToString_StackableItem_IncludesQuantity()
    {
        // Arrange
        var equipment = new GrantedEquipment("bandages", 5, false, null);

        // Act
        var result = equipment.ToString();

        // Assert
        result.Should().Be("bandages ×5");
    }

    /// <summary>
    /// Verifies that GrantedEquipment.ToString formats single inventory items with just the name.
    /// </summary>
    [Test]
    public void GrantedEquipment_ToString_SingleInventoryItem_JustName()
    {
        // Arrange
        var equipment = new GrantedEquipment("lockpicks", 1, false, null);

        // Act
        var result = equipment.ToString();

        // Assert
        result.Should().Be("lockpicks");
    }
}
