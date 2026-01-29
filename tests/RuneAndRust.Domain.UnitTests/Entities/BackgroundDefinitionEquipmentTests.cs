using FluentAssertions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the equipment grant extension of <see cref="BackgroundDefinition"/>.
/// </summary>
/// <remarks>
/// Verifies that BackgroundDefinition correctly stores, accesses, and summarizes
/// equipment grants added in v0.17.1c. Tests cover the EquipmentGrants property,
/// helper methods, and background-specific equipment grant configurations.
/// </remarks>
[TestFixture]
public class BackgroundDefinitionEquipmentTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // EQUIPMENT GRANTS PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with equipment grants stores them correctly.
    /// </summary>
    [Test]
    public void Create_WithEquipmentGrants_StoresGrants()
    {
        // Arrange
        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield),
            BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon)
        };

        // Act
        var definition = CreateDefinitionWithEquipmentGrants(equipmentGrants);

        // Assert
        definition.EquipmentGrants.Should().HaveCount(2);
        definition.EquipmentGrants[0].ItemId.Should().Be("shield");
        definition.EquipmentGrants[0].IsEquipped.Should().BeTrue();
        definition.EquipmentGrants[1].ItemId.Should().Be("spear");
        definition.EquipmentGrants[1].Slot.Should().Be(EquipmentSlot.Weapon);
    }

    /// <summary>
    /// Verifies that Create without equipment grants defaults to an empty list.
    /// </summary>
    [Test]
    public void Create_WithoutEquipmentGrants_DefaultsToEmptyList()
    {
        // Act
        var definition = BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood on the walls, shield in hand, protecting your people.",
            "The weight of the shield, the length of the spear—these were your teachers.",
            "Warrior and protector",
            "Honored defender, trusted by clan leadership");

        // Assert
        definition.EquipmentGrants.Should().BeEmpty(
            "equipment grants should default to an empty list when not provided");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HELPER METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasEquipmentGrants returns true when grants are defined.
    /// </summary>
    [Test]
    public void HasEquipmentGrants_WithGrants_ReturnsTrue()
    {
        // Arrange
        var definition = CreateClanGuardWithEquipmentGrants();

        // Act & Assert
        definition.HasEquipmentGrants().Should().BeTrue(
            "Clan Guard has 2 equipment grants defined");
    }

    /// <summary>
    /// Verifies that HasEquipmentGrants returns false when no grants exist.
    /// </summary>
    [Test]
    public void HasEquipmentGrants_WithoutGrants_ReturnsFalse()
    {
        // Arrange
        var definition = BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood on the walls, shield in hand, protecting your people.",
            "The weight of the shield, the length of the spear—these were your teachers.",
            "Warrior and protector",
            "Honored defender");

        // Act & Assert
        definition.HasEquipmentGrants().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetEquippedItems returns only grants with IsEquipped true.
    /// </summary>
    [Test]
    public void GetEquippedItems_ReturnsOnlyEquippedGrants()
    {
        // Arrange - mix of equipped and inventory items
        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon),
            BackgroundEquipmentGrant.Inventory("lockpicks"),
            BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield)
        };
        var definition = CreateDefinitionWithEquipmentGrants(equipmentGrants);

        // Act
        var equipped = definition.GetEquippedItems().ToList();

        // Assert
        equipped.Should().HaveCount(2,
            "only 2 of 3 grants are auto-equipped");
        equipped.Should().OnlyContain(g => g.IsEquipped,
            "all returned grants should have IsEquipped = true");
        equipped.Select(g => g.ItemId).Should().Contain("spear");
        equipped.Select(g => g.ItemId).Should().Contain("shield");
    }

    /// <summary>
    /// Verifies that GetInventoryItems returns only grants with IsEquipped false.
    /// </summary>
    [Test]
    public void GetInventoryItems_ReturnsOnlyNonEquippedGrants()
    {
        // Arrange - Traveling Healer style: all inventory items
        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Inventory("healers-kit"),
            BackgroundEquipmentGrant.Consumable("bandages", 5)
        };
        var definition = CreateDefinitionWithEquipmentGrants(equipmentGrants);

        // Act
        var inventoryItems = definition.GetInventoryItems().ToList();

        // Assert
        inventoryItems.Should().HaveCount(2,
            "both Traveling Healer items are inventory-only");
        inventoryItems.Should().OnlyContain(g => !g.IsEquipped,
            "all returned grants should have IsEquipped = false");
    }

    /// <summary>
    /// Verifies that GetTotalItemCount sums all quantities.
    /// </summary>
    [Test]
    public void GetTotalItemCount_SumsAllQuantities()
    {
        // Arrange - Traveling Healer: 1 kit + 5 bandages = 6 total
        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Inventory("healers-kit"),
            BackgroundEquipmentGrant.Consumable("bandages", 5)
        };
        var definition = CreateDefinitionWithEquipmentGrants(equipmentGrants);

        // Act
        var total = definition.GetTotalItemCount();

        // Assert
        total.Should().Be(6,
            "Traveling Healer grants 1 kit + 5 bandages = 6 total items");
    }

    /// <summary>
    /// Verifies that GetEquipmentGrantSummary formats grants as a readable string.
    /// </summary>
    [Test]
    public void GetEquipmentGrantSummary_FormatsCorrectly()
    {
        // Arrange
        var definition = CreateClanGuardWithEquipmentGrants();

        // Act
        var summary = definition.GetEquipmentGrantSummary();

        // Assert
        summary.Should().Contain("shield");
        summary.Should().Contain("spear");
        summary.Should().Contain(", ",
            "grants should be comma-separated");
    }

    /// <summary>
    /// Verifies that GetEquipmentGrantSummary returns fallback text when no grants exist.
    /// </summary>
    [Test]
    public void GetEquipmentGrantSummary_WithNoGrants_ReturnsFallbackText()
    {
        // Arrange
        var definition = BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood on the walls, shield in hand, protecting your people.",
            "The weight of the shield, the length of the spear—these were your teachers.",
            "Warrior and protector",
            "Honored defender");

        // Act
        var summary = definition.GetEquipmentGrantSummary();

        // Assert
        summary.Should().Be("No starting equipment");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BACKGROUND-SPECIFIC TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Clan Guard grants Shield (Shield slot) and Spear (Weapon slot).
    /// </summary>
    [Test]
    public void ClanGuard_GrantsShieldAndSpearEquipped()
    {
        // Arrange
        var definition = CreateClanGuardWithEquipmentGrants();

        // Assert
        definition.EquipmentGrants.Should().HaveCount(2,
            "Clan Guard grants exactly 2 equipment items");

        var shieldGrant = definition.EquipmentGrants.First(g => g.ItemId == "shield");
        shieldGrant.IsEquipped.Should().BeTrue("Shield should be auto-equipped");
        shieldGrant.Slot.Should().Be(EquipmentSlot.Shield, "Shield goes to Shield slot");
        shieldGrant.Quantity.Should().Be(1);

        var spearGrant = definition.EquipmentGrants.First(g => g.ItemId == "spear");
        spearGrant.IsEquipped.Should().BeTrue("Spear should be auto-equipped");
        spearGrant.Slot.Should().Be(EquipmentSlot.Weapon, "Spear goes to Weapon slot");
        spearGrant.Quantity.Should().Be(1);
    }

    /// <summary>
    /// Verifies that Traveling Healer grants Bandages x5 as a consumable.
    /// </summary>
    [Test]
    public void TravelingHealer_GrantsBandagesx5()
    {
        // Arrange
        var equipmentGrants = new List<BackgroundEquipmentGrant>
        {
            BackgroundEquipmentGrant.Inventory("healers-kit"),
            BackgroundEquipmentGrant.Consumable("bandages", 5)
        };
        var definition = CreateDefinitionWithEquipmentGrants(equipmentGrants);

        // Assert
        var bandagesGrant = definition.EquipmentGrants.First(g => g.ItemId == "bandages");
        bandagesGrant.Quantity.Should().Be(5, "Traveling Healer grants 5 bandages");
        bandagesGrant.IsStackable.Should().BeTrue();
        bandagesGrant.IsEquipped.Should().BeFalse("Bandages are inventory-only");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString includes equipment grant count.
    /// </summary>
    [Test]
    public void ToString_IncludesEquipmentGrantCount()
    {
        // Arrange
        var definition = CreateClanGuardWithEquipmentGrants();

        // Act
        var result = definition.ToString();

        // Assert
        result.Should().Contain("Equipment=2",
            "ToString should include the number of equipment grants");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TEST HELPERS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a Clan Guard definition with standard equipment grants for testing.
    /// </summary>
    /// <returns>A BackgroundDefinition with Shield and Spear equipment grants.</returns>
    private static BackgroundDefinition CreateClanGuardWithEquipmentGrants()
    {
        return CreateDefinitionWithEquipmentGrants(
            new List<BackgroundEquipmentGrant>
            {
                BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield),
                BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon)
            });
    }

    /// <summary>
    /// Creates a BackgroundDefinition with specified equipment grants for testing.
    /// </summary>
    /// <param name="equipmentGrants">The equipment grants to include.</param>
    /// <returns>A BackgroundDefinition with the specified equipment grants.</returns>
    private static BackgroundDefinition CreateDefinitionWithEquipmentGrants(
        IReadOnlyList<BackgroundEquipmentGrant> equipmentGrants)
    {
        return BackgroundDefinition.Create(
            Background.ClanGuard,
            "Clan Guard",
            "You stood on the walls, shield in hand, protecting your people.",
            "The weight of the shield, the length of the spear—these were your teachers.",
            "Warrior and protector",
            "Honored defender, trusted by clan leadership",
            new List<string>
            {
                "Recognize military formations and tactics",
                "Other guards trust you more quickly",
                "May have oaths to uphold or avenge"
            },
            new List<BackgroundSkillGrant>
            {
                BackgroundSkillGrant.Permanent("combat", 2),
                BackgroundSkillGrant.Permanent("vigilance", 1)
            },
            equipmentGrants);
    }
}
