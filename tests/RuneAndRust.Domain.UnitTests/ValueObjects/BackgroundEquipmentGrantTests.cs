using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="BackgroundEquipmentGrant"/> value object.
/// </summary>
/// <remarks>
/// Verifies factory method behavior, parameter validation, computed properties,
/// normalization, convenience factory methods, auto-equip logic, and string
/// formatting for the BackgroundEquipmentGrant value object.
/// </remarks>
[TestFixture]
public class BackgroundEquipmentGrantTests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - SUCCESSFUL CREATION
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create with valid parameters produces a correctly populated grant.
    /// </summary>
    [Test]
    public void Create_WithValidParameters_CreatesGrant()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Create("spear", 1, true, EquipmentSlot.Weapon);

        // Assert
        grant.ItemId.Should().Be("spear");
        grant.Quantity.Should().Be(1);
        grant.IsEquipped.Should().BeTrue();
        grant.Slot.Should().Be(EquipmentSlot.Weapon);
        grant.HasAutoEquip.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Create with inventory-only item works correctly.
    /// </summary>
    [Test]
    public void Create_WithInventoryOnlyItem_CreatesGrant()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Create("healers-kit");

        // Assert
        grant.ItemId.Should().Be("healers-kit");
        grant.Quantity.Should().Be(1);
        grant.IsEquipped.Should().BeFalse();
        grant.Slot.Should().BeNull();
        grant.IsInventoryOnly.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS - VALIDATION FAILURES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create throws when itemId is null.
    /// </summary>
    [Test]
    public void Create_WithNullItemId_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundEquipmentGrant.Create(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that Create throws when itemId is whitespace.
    /// </summary>
    [Test]
    public void Create_WithWhitespaceItemId_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundEquipmentGrant.Create("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("itemId");
    }

    /// <summary>
    /// Verifies that Create throws when quantity is zero.
    /// </summary>
    [Test]
    public void Create_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Action act = () => BackgroundEquipmentGrant.Create("bandages", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    /// <summary>
    /// Verifies that Create throws when isEquipped is true but slot is null.
    /// </summary>
    /// <remarks>
    /// Auto-equipped items must specify which slot to equip to. Without a
    /// slot, the equipment system cannot place the item.
    /// </remarks>
    [Test]
    public void Create_WithEquippedButNoSlot_ThrowsArgumentException()
    {
        // Arrange
        Action act = () => BackgroundEquipmentGrant.Create("spear", 1, true, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("slot");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NORMALIZATION TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Create normalizes itemId to lowercase.
    /// </summary>
    /// <remarks>
    /// Item IDs are normalized to lowercase for consistent matching
    /// with the configuration-driven item system.
    /// </remarks>
    [Test]
    public void Create_NormalizesItemIdToLowercase()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Create("SMITHS-HAMMER");

        // Assert
        grant.ItemId.Should().Be("smiths-hammer",
            "item IDs should be normalized to lowercase for consistent matching");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONVENIENCE FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that Equipped creates an auto-equip grant with the specified slot.
    /// </summary>
    [Test]
    public void Equipped_CreatesAutoEquipGrant()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Equipped("shield", EquipmentSlot.Shield);

        // Assert
        grant.ItemId.Should().Be("shield");
        grant.Quantity.Should().Be(1);
        grant.IsEquipped.Should().BeTrue();
        grant.Slot.Should().Be(EquipmentSlot.Shield);
        grant.HasAutoEquip.Should().BeTrue();
        grant.IsInventoryOnly.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Inventory creates an inventory-only grant.
    /// </summary>
    [Test]
    public void Inventory_CreatesInventoryOnlyGrant()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Inventory("lockpicks");

        // Assert
        grant.ItemId.Should().Be("lockpicks");
        grant.Quantity.Should().Be(1);
        grant.IsEquipped.Should().BeFalse();
        grant.Slot.Should().BeNull();
        grant.IsInventoryOnly.Should().BeTrue();
        grant.HasAutoEquip.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Consumable creates a grant with the specified quantity.
    /// </summary>
    [Test]
    public void Consumable_CreatesQuantityGrant()
    {
        // Act
        var grant = BackgroundEquipmentGrant.Consumable("bandages", 5);

        // Assert
        grant.ItemId.Should().Be("bandages");
        grant.Quantity.Should().Be(5);
        grant.IsEquipped.Should().BeFalse();
        grant.IsStackable.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that HasAutoEquip returns true when equipped with a slot.
    /// </summary>
    [Test]
    public void HasAutoEquip_WithEquippedAndSlot_ReturnsTrue()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon);

        // Assert
        grant.HasAutoEquip.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStackable returns true for quantity greater than 1.
    /// </summary>
    [Test]
    public void IsStackable_WithQuantityGreaterThan1_ReturnsTrue()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Consumable("rations", 3);

        // Assert
        grant.IsStackable.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that IsStackable returns false for quantity of 1.
    /// </summary>
    [Test]
    public void IsStackable_WithQuantity1_ReturnsFalse()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Inventory("lockpicks");

        // Assert
        grant.IsStackable.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TOSTRING TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verifies that ToString formats equipped items with slot name.
    /// </summary>
    [Test]
    public void ToString_ForEquippedItem_IncludesSlotName()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Equipped("spear", EquipmentSlot.Weapon);

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("spear (Weapon)");
    }

    /// <summary>
    /// Verifies that ToString formats stackable items with quantity.
    /// </summary>
    [Test]
    public void ToString_ForStackableItem_IncludesQuantity()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Consumable("bandages", 5);

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("bandages x5");
    }

    /// <summary>
    /// Verifies that ToString formats single inventory items without extras.
    /// </summary>
    [Test]
    public void ToString_ForSingleInventoryItem_ShowsItemIdOnly()
    {
        // Arrange
        var grant = BackgroundEquipmentGrant.Inventory("lockpicks");

        // Act
        var result = grant.ToString();

        // Assert
        result.Should().Be("lockpicks");
    }
}
