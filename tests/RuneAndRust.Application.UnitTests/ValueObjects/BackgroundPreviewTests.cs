// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundPreviewTests.cs
// Unit tests for BackgroundPreview value object (v0.17.1e).
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Enums;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="BackgroundPreview"/> value object (v0.17.1e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Empty factory method creates placeholder with sensible defaults</description></item>
///   <item><description>Properties are correctly set and retrievable</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class BackgroundPreviewTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Empty TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Empty creates a preview with placeholder values for
    /// display when a background is not found.
    /// </summary>
    [Test]
    public void Empty_CreatesEmptyPreview()
    {
        // Arrange & Act
        var preview = BackgroundPreview.Empty(Background.VillageSmith);

        // Assert
        preview.BackgroundId.Should().Be(Background.VillageSmith);
        preview.DisplayName.Should().Be("Unknown");
        preview.Description.Should().Be("Background not found");
        preview.SelectionText.Should().BeEmpty();
        preview.ProfessionBefore.Should().BeEmpty();
        preview.SocialStanding.Should().BeEmpty();
        preview.SkillSummary.Should().Be("None");
        preview.EquipmentSummary.Should().Be("None");
        preview.NarrativeHookCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Properties TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that all properties are correctly set and retrievable when
    /// creating a BackgroundPreview with init properties.
    /// </summary>
    [Test]
    public void Properties_SetCorrectly()
    {
        // Arrange & Act
        var preview = new BackgroundPreview
        {
            BackgroundId = Background.ClanGuard,
            DisplayName = "Clan Guard",
            Description = "You stood on the walls, shield in hand, protecting your people",
            SelectionText = "The weight of the shield, the length of the spear—these were your teachers.",
            ProfessionBefore = "Warrior and protector",
            SocialStanding = "Honored defender, trusted by clan leadership",
            SkillSummary = "combat +2, vigilance +1",
            EquipmentSummary = "shield (Shield), spear (Weapon)",
            NarrativeHookCount = 3
        };

        // Assert
        preview.BackgroundId.Should().Be(Background.ClanGuard);
        preview.DisplayName.Should().Be("Clan Guard");
        preview.Description.Should().Contain("shield in hand");
        preview.SelectionText.Should().Contain("weight of the shield");
        preview.ProfessionBefore.Should().Be("Warrior and protector");
        preview.SocialStanding.Should().Contain("Honored defender");
        preview.SkillSummary.Should().Be("combat +2, vigilance +1");
        preview.EquipmentSummary.Should().Be("shield (Shield), spear (Weapon)");
        preview.NarrativeHookCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that BackgroundPreview is a value type (readonly record struct)
    /// and supports structural equality.
    /// </summary>
    [Test]
    public void BackgroundPreview_SupportsStructuralEquality()
    {
        // Arrange
        var preview1 = new BackgroundPreview
        {
            BackgroundId = Background.RuinDelver,
            DisplayName = "Ruin Delver",
            Description = "Test",
            SelectionText = "Test",
            ProfessionBefore = "Test",
            SocialStanding = "Test",
            SkillSummary = "exploration +2, traps +1",
            EquipmentSummary = "lantern, rope, lockpicks",
            NarrativeHookCount = 3
        };
        var preview2 = new BackgroundPreview
        {
            BackgroundId = Background.RuinDelver,
            DisplayName = "Ruin Delver",
            Description = "Test",
            SelectionText = "Test",
            ProfessionBefore = "Test",
            SocialStanding = "Test",
            SkillSummary = "exploration +2, traps +1",
            EquipmentSummary = "lantern, rope, lockpicks",
            NarrativeHookCount = 3
        };

        // Assert
        preview1.Should().Be(preview2);
    }
}
