// ═══════════════════════════════════════════════════════════════════════════════
// BackgroundApplicationServiceTests.cs
// Unit tests for BackgroundApplicationService (v0.17.1e).
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Application.ValueObjects;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

#pragma warning disable NUnit2045 // Use Assert.Multiple

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="BackgroundApplicationService"/> (v0.17.1e).
/// </summary>
/// <remarks>
/// <para>Tests cover:</para>
/// <list type="bullet">
///   <item><description>Constructor null validation for dependencies</description></item>
///   <item><description>Successful background application with skill grants</description></item>
///   <item><description>Successful background application with equipment grants</description></item>
///   <item><description>Auto-equip behavior for equipped items</description></item>
///   <item><description>Failure result when background is not found</description></item>
///   <item><description>Failure result when character is null</description></item>
///   <item><description>Preview generation for valid backgrounds</description></item>
///   <item><description>Preview generation returns empty for invalid backgrounds</description></item>
///   <item><description>GetAllBackgroundPreviews returns all six previews</description></item>
///   <item><description>CanApplyBackground delegates to provider</description></item>
///   <item><description>Background is set on character after successful application</description></item>
///   <item><description>Village Smith specific grant verification</description></item>
///   <item><description>Clan Guard specific equip verification</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class BackgroundApplicationServiceTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════════════════

    private Mock<IBackgroundProvider> _mockProvider = null!;
    private Mock<ILogger<BackgroundApplicationService>> _mockLogger = null!;
    private BackgroundApplicationService _service = null!;

    /// <summary>
    /// Sets up test dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockProvider = new Mock<IBackgroundProvider>();
        _mockLogger = new Mock<ILogger<BackgroundApplicationService>>();
        _service = new BackgroundApplicationService(_mockProvider.Object, _mockLogger.Object);

        // Set up default provider responses
        SetupVillageSmithProvider();
        SetupClanGuardProvider();
        SetupAllBackgroundsProvider();
        SetupHasBackgroundProvider();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that the constructor throws when the provider is null.
    /// </summary>
    [Test]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new BackgroundApplicationService(
            null!,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("backgroundProvider");
    }

    /// <summary>
    /// Verifies that the constructor throws when the logger is null.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new BackgroundApplicationService(
            _mockProvider.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ApplyBackgroundToCharacter TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that applying Village Smith background grants Craft +2 and Might +1
    /// skill bonuses to the character.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_VillageSmith_AppliesSkillGrants()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyBackgroundToCharacter(player, Background.VillageSmith);

        // Assert - Result is successful
        result.Success.Should().BeTrue();
        result.BackgroundId.Should().Be(Background.VillageSmith);

        // Assert - Skills were granted
        result.SkillsGranted.Should().HaveCount(2);
        result.SkillsGranted.Should().Contain(s => s.SkillId == "craft" && s.Amount == 2);
        result.SkillsGranted.Should().Contain(s => s.SkillId == "might" && s.Amount == 1);

        // Assert - Skills exist on the player
        player.HasSkill("craft").Should().BeTrue();
        player.HasSkill("might").Should().BeTrue();

        // Assert - Success message is present
        result.Messages.Should().ContainSingle()
            .Which.Should().Contain("Village Smith");
    }

    /// <summary>
    /// Verifies that applying Village Smith background creates equipment items
    /// and adds them to the character's inventory.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_VillageSmith_AppliesEquipmentGrants()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyBackgroundToCharacter(player, Background.VillageSmith);

        // Assert - Equipment was granted
        result.EquipmentGranted.Should().HaveCount(2);
        result.EquipmentGranted.Should().Contain(e => e.ItemId == "smiths-hammer");
        result.EquipmentGranted.Should().Contain(e => e.ItemId == "leather-apron");

        // Assert - Items were added to inventory
        player.Inventory.Items.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that equipped items are auto-equipped to their designated slots.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_ClanGuard_EquipsAutoEquipItems()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyBackgroundToCharacter(player, Background.ClanGuard);

        // Assert - Result indicates items were equipped
        result.Success.Should().BeTrue();
        result.EquipmentGranted.Should().Contain(e =>
            e.ItemId == "shield" && e.WasEquipped && e.Slot == EquipmentSlot.Shield);
        result.EquipmentGranted.Should().Contain(e =>
            e.ItemId == "spear" && e.WasEquipped && e.Slot == EquipmentSlot.Weapon);

        // Assert - Items are in equipment slots on the player
        player.GetEquippedItem(EquipmentSlot.Shield).Should().NotBeNull();
        player.GetEquippedItem(EquipmentSlot.Weapon).Should().NotBeNull();
        player.GetEquippedItem(EquipmentSlot.Shield)!.Name.Should().Be("Shield");
        player.GetEquippedItem(EquipmentSlot.Weapon)!.Name.Should().Be("Spear");
    }

    /// <summary>
    /// Verifies that applying an invalid (unknown) background returns a failure result
    /// and does not modify the character.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_InvalidBackground_ReturnsFailure()
    {
        // Arrange
        var player = new Player("TestHero");
        var unknownBackground = (Background)99;
        _mockProvider.Setup(p => p.GetBackground(unknownBackground))
            .Returns((BackgroundDefinition?)null);

        // Act
        var result = _service.ApplyBackgroundToCharacter(player, unknownBackground);

        // Assert
        result.Success.Should().BeFalse();
        result.SkillsGranted.Should().BeEmpty();
        result.EquipmentGranted.Should().BeEmpty();
        result.Messages.Should().ContainSingle()
            .Which.Should().Contain("not found");

        // Assert - Character was not modified
        player.HasBackground.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that applying a background to a null character returns a failure result.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_NullCharacter_ReturnsFailure()
    {
        // Arrange & Act
        var result = _service.ApplyBackgroundToCharacter(null!, Background.VillageSmith);

        // Assert
        result.Success.Should().BeFalse();
        result.Messages.Should().ContainSingle()
            .Which.Should().Contain("null");
    }

    /// <summary>
    /// Verifies that the background is set on the character entity after successful application.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_SetsBackgroundOnCharacter()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        _service.ApplyBackgroundToCharacter(player, Background.VillageSmith);

        // Assert
        player.HasBackground.Should().BeTrue();
        player.SelectedBackground.Should().Be(Background.VillageSmith);
    }

    /// <summary>
    /// Verifies that skill summary in the result formats correctly.
    /// </summary>
    [Test]
    public void ApplyBackgroundToCharacter_ResultSkillSummary_FormatsCorrectly()
    {
        // Arrange
        var player = new Player("TestHero");

        // Act
        var result = _service.ApplyBackgroundToCharacter(player, Background.VillageSmith);

        // Assert
        result.GetSkillSummary().Should().Be("craft +2, might +1");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetBackgroundPreview TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetBackgroundPreview returns correct preview data for a valid background.
    /// </summary>
    [Test]
    public void GetBackgroundPreview_ValidBackground_ReturnsCorrectPreview()
    {
        // Arrange & Act
        var preview = _service.GetBackgroundPreview(Background.VillageSmith);

        // Assert
        preview.BackgroundId.Should().Be(Background.VillageSmith);
        preview.DisplayName.Should().Be("Village Smith");
        preview.Description.Should().Contain("forge");
        preview.ProfessionBefore.Should().Be("Blacksmith and metalworker");
        preview.SocialStanding.Should().Contain("Respected");
        preview.NarrativeHookCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that GetBackgroundPreview returns an empty preview for an invalid background.
    /// </summary>
    [Test]
    public void GetBackgroundPreview_InvalidBackground_ReturnsEmptyPreview()
    {
        // Arrange
        var unknownBackground = (Background)99;
        _mockProvider.Setup(p => p.GetBackground(unknownBackground))
            .Returns((BackgroundDefinition?)null);

        // Act
        var preview = _service.GetBackgroundPreview(unknownBackground);

        // Assert
        preview.DisplayName.Should().Be("Unknown");
        preview.Description.Should().Be("Background not found");
        preview.SkillSummary.Should().Be("None");
        preview.EquipmentSummary.Should().Be("None");
        preview.NarrativeHookCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GetAllBackgroundPreviews TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that GetAllBackgroundPreviews returns previews for all six backgrounds.
    /// </summary>
    [Test]
    public void GetAllBackgroundPreviews_ReturnsAllSixPreviews()
    {
        // Arrange & Act
        var previews = _service.GetAllBackgroundPreviews();

        // Assert
        previews.Should().HaveCount(6);
        previews.Should().Contain(p => p.BackgroundId == Background.VillageSmith);
        previews.Should().Contain(p => p.BackgroundId == Background.TravelingHealer);
        previews.Should().Contain(p => p.BackgroundId == Background.RuinDelver);
        previews.Should().Contain(p => p.BackgroundId == Background.ClanGuard);
        previews.Should().Contain(p => p.BackgroundId == Background.WanderingSkald);
        previews.Should().Contain(p => p.BackgroundId == Background.OutcastScavenger);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CanApplyBackground TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that CanApplyBackground returns true for valid backgrounds.
    /// </summary>
    [Test]
    public void CanApplyBackground_ValidBackground_ReturnsTrue()
    {
        // Arrange & Act
        var result = _service.CanApplyBackground(Background.VillageSmith);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanApplyBackground returns false for invalid backgrounds.
    /// </summary>
    [Test]
    public void CanApplyBackground_InvalidBackground_ReturnsFalse()
    {
        // Arrange
        var unknownBackground = (Background)99;
        _mockProvider.Setup(p => p.HasBackground(unknownBackground)).Returns(false);

        // Act
        var result = _service.CanApplyBackground(unknownBackground);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configures the mock provider with Village Smith background data.
    /// </summary>
    private void SetupVillageSmithProvider()
    {
        var skillGrants = new List<BackgroundSkillGrant>
        {
            BackgroundSkillGrant.Permanent("craft", 2),
            BackgroundSkillGrant.Permanent("might", 1)
        };

        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Equipped("smiths-hammer", EquipmentSlot.Weapon),
            BackgroundEquipmentGrant.Equipped("leather-apron", EquipmentSlot.Armor)
        };

        var definition = BackgroundDefinition.Create(
            Background.VillageSmith,
            "Village Smith",
            "You worked the forge, shaping metal into tools of war and peace",
            "The ring of hammer on anvil was your morning song.",
            "Blacksmith and metalworker",
            "Respected craftsperson, essential to any settlement",
            narrativeHooks: new List<string>
            {
                "Recognize craftsmanship in ruins",
                "Repair broken equipment more easily",
                "Clan smiths may offer discounts or quests"
            },
            skillGrants: skillGrants,
            equipmentGrants: equipmentGrants);

        _mockProvider.Setup(p => p.GetBackground(Background.VillageSmith))
            .Returns(definition);
    }

    /// <summary>
    /// Configures the mock provider with Clan Guard background data.
    /// </summary>
    private void SetupClanGuardProvider()
    {
        var skillGrants = new List<BackgroundSkillGrant>
        {
            BackgroundSkillGrant.Permanent("combat", 2),
            BackgroundSkillGrant.Permanent("vigilance", 1)
        };

        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield),
            BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon)
        };

        var definition = BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood on the walls, shield in hand, protecting your people",
            "The weight of the shield, the length of the spear.",
            "Warrior and protector",
            "Honored defender, trusted by clan leadership",
            narrativeHooks: new List<string>
            {
                "Recognize military formations and tactics",
                "Other guards trust you more quickly",
                "May have oaths to uphold or avenge"
            },
            skillGrants: skillGrants,
            equipmentGrants: equipmentGrants);

        _mockProvider.Setup(p => p.GetBackground(Background.ClanGuard))
            .Returns(definition);
    }

    /// <summary>
    /// Configures the mock provider to return all six background definitions.
    /// </summary>
    private void SetupAllBackgroundsProvider()
    {
        var backgrounds = new List<BackgroundDefinition>();

        foreach (var bg in Enum.GetValues<Background>())
        {
            var definition = BackgroundDefinition.Create(
                bg,
                bg.ToString(),
                $"Description for {bg}",
                $"Selection text for {bg}",
                $"Profession for {bg}",
                $"Standing for {bg}",
                narrativeHooks: new List<string> { "Hook 1", "Hook 2", "Hook 3" },
                skillGrants: new List<BackgroundSkillGrant>
                {
                    BackgroundSkillGrant.Permanent("skill1", 2),
                    BackgroundSkillGrant.Permanent("skill2", 1)
                },
                equipmentGrants: new List<BackgroundEquipmentGrant>
                {
                    BackgroundEquipmentGrant.Inventory("item1")
                });

            // Only set up if not already set up by specific methods
            if (bg != Background.VillageSmith && bg != Background.ClanGuard)
            {
                _mockProvider.Setup(p => p.GetBackground(bg)).Returns(definition);
            }

            backgrounds.Add(definition);
        }

        _mockProvider.Setup(p => p.GetAllBackgrounds()).Returns(backgrounds);
    }

    /// <summary>
    /// Configures HasBackground to return true for all valid Background enum values.
    /// </summary>
    private void SetupHasBackgroundProvider()
    {
        foreach (var bg in Enum.GetValues<Background>())
        {
            _mockProvider.Setup(p => p.HasBackground(bg)).Returns(true);
        }
    }
}
