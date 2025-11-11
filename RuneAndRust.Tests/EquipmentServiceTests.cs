using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

[TestFixture]
public class EquipmentServiceTests
{
    private EquipmentService _equipmentService;

    [SetUp]
    public void Setup()
    {
        _equipmentService = new EquipmentService();
    }

    #region Weapon Equip Tests

    [Test]
    public void EquipWeapon_ValidWeapon_EquipsSuccessfully()
    {
        // Arrange
        var player = CreateTestPlayer();
        var weapon = CreateTestWeapon("Test Axe", QualityTier.Scavenged, 1, 2);

        // Act
        var result = _equipmentService.EquipWeapon(player, weapon);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(player.EquippedWeapon, Is.EqualTo(weapon));
    }

    [Test]
    public void EquipWeapon_ReplacesExistingWeapon_UnequipsOld()
    {
        // Arrange
        var player = CreateTestPlayer();
        var oldWeapon = CreateTestWeapon("Old Axe", QualityTier.JuryRigged, 1, 0);
        var newWeapon = CreateTestWeapon("New Axe", QualityTier.Scavenged, 1, 2);

        player.EquippedWeapon = oldWeapon;

        // Act
        var result = _equipmentService.EquipWeapon(player, newWeapon);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(player.EquippedWeapon, Is.EqualTo(newWeapon));
        Assert.That(player.EquippedWeapon, Is.Not.EqualTo(oldWeapon));
    }

    [Test]
    public void EquipWeapon_NullWeapon_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var result = _equipmentService.EquipWeapon(player, null);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Armor Equip Tests

    [Test]
    public void EquipArmor_ValidArmor_EquipsAndRecalculatesStats()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMaxHP = player.MaxHP;
        var armor = CreateTestArmor("Test Plating", QualityTier.Scavenged, hpBonus: 10, defenseBonus: 2);

        // Act
        var result = _equipmentService.EquipArmor(player, armor);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(player.EquippedArmor, Is.EqualTo(armor));
        Assert.That(player.MaxHP, Is.EqualTo(baseMaxHP + 10));
    }

    [Test]
    public void EquipArmor_WithAttributeBonuses_AppliesBonuses()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMight = player.Attributes.Might;

        var armor = CreateTestArmor("Warrior Plate", QualityTier.ClanForged, hpBonus: 15, defenseBonus: 3);
        armor.Bonuses.Add(new EquipmentBonus
        {
            AttributeName = "MIGHT",
            BonusValue = 2,
            Description = "+2 MIGHT"
        });

        // Act
        var result = _equipmentService.EquipArmor(player, armor);

        // Assert
        Assert.That(result, Is.True);
        var effectiveMight = _equipmentService.GetEffectiveAttributeValue(player, "MIGHT");
        Assert.That(effectiveMight, Is.EqualTo(baseMight + 2));
    }

    [Test]
    public void EquipArmor_ReplacingArmor_RecalculatesStatsCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMaxHP = player.MaxHP;

        var oldArmor = CreateTestArmor("Old Armor", QualityTier.JuryRigged, hpBonus: 5, defenseBonus: 1);
        var newArmor = CreateTestArmor("New Armor", QualityTier.Scavenged, hpBonus: 15, defenseBonus: 3);

        _equipmentService.EquipArmor(player, oldArmor);
        var hpWithOldArmor = player.MaxHP;

        // Act
        _equipmentService.EquipArmor(player, newArmor);

        // Assert
        Assert.That(player.MaxHP, Is.EqualTo(baseMaxHP + 15)); // Should have new armor's bonus, not old
        Assert.That(player.MaxHP, Is.Not.EqualTo(hpWithOldArmor));
    }

    #endregion

    #region Stat Recalculation Tests

    [Test]
    public void RecalculatePlayerStats_NoEquipment_MaintainsBaseStats()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMaxHP = player.MaxHP;
        var baseStamina = player.MaxStamina;

        // Act
        _equipmentService.RecalculatePlayerStats(player);

        // Assert
        Assert.That(player.MaxHP, Is.EqualTo(baseMaxHP));
        Assert.That(player.MaxStamina, Is.EqualTo(baseStamina));
    }

    [Test]
    public void RecalculatePlayerStats_WithArmorHPBonus_IncreasesMaxHP()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMaxHP = player.MaxHP;

        var armor = CreateTestArmor("Heavy Plate", QualityTier.Optimized, hpBonus: 25, defenseBonus: 4);
        player.EquippedArmor = armor;

        // Act
        _equipmentService.RecalculatePlayerStats(player);

        // Assert
        Assert.That(player.MaxHP, Is.EqualTo(baseMaxHP + 25));
    }

    [Test]
    public void RecalculatePlayerStats_PreservesCurrentHPRatio()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.HP = player.MaxHP / 2; // 50% health
        var currentHP = player.HP;

        var armor = CreateTestArmor("Test Armor", QualityTier.Scavenged, hpBonus: 20, defenseBonus: 2);
        player.EquippedArmor = armor;

        // Act
        _equipmentService.RecalculatePlayerStats(player);

        // Assert - Current HP should be adjusted proportionally
        var expectedHP = (player.MaxHP / 2); // Still 50% of new max
        Assert.That(player.HP, Is.EqualTo(expectedHP));
    }

    #endregion

    #region Attribute Bonus Tests

    [Test]
    public void GetEffectiveAttributeValue_NoEquipment_ReturnsBaseValue()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseMight = player.Attributes.Might;

        // Act
        var effectiveMight = _equipmentService.GetEffectiveAttributeValue(player, "MIGHT");

        // Assert
        Assert.That(effectiveMight, Is.EqualTo(baseMight));
    }

    [Test]
    public void GetEffectiveAttributeValue_WithWeaponBonus_AddsBonus()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseFinesse = player.Attributes.Finesse;

        var weapon = CreateTestWeapon("Balanced Spear", QualityTier.ClanForged, 1, 3);
        weapon.Bonuses.Add(new EquipmentBonus
        {
            AttributeName = "FINESSE",
            BonusValue = 3,
            Description = "+3 FINESSE"
        });
        player.EquippedWeapon = weapon;

        // Act
        var effectiveFinesse = _equipmentService.GetEffectiveAttributeValue(player, "FINESSE");

        // Assert
        Assert.That(effectiveFinesse, Is.EqualTo(baseFinesse + 3));
    }

    [Test]
    public void GetEffectiveAttributeValue_WithBothEquipmentBonuses_AddsBoth()
    {
        // Arrange
        var player = CreateTestPlayer();
        var baseWill = player.Attributes.Will;

        var weapon = CreateTestWeapon("Mystic Staff", QualityTier.Optimized, 2, 2);
        weapon.Bonuses.Add(new EquipmentBonus { AttributeName = "WILL", BonusValue = 2, Description = "+2 WILL" });

        var armor = CreateTestArmor("Sage Robes", QualityTier.Optimized, hpBonus: 10, defenseBonus: 1);
        armor.Bonuses.Add(new EquipmentBonus { AttributeName = "WILL", BonusValue = 3, Description = "+3 WILL" });

        player.EquippedWeapon = weapon;
        player.EquippedArmor = armor;

        // Act
        var effectiveWill = _equipmentService.GetEffectiveAttributeValue(player, "WILL");

        // Assert
        Assert.That(effectiveWill, Is.EqualTo(baseWill + 5)); // +2 from weapon, +3 from armor
    }

    #endregion

    #region Equipment Comparison Tests

    [Test]
    public void CompareEquipment_BetterWeapon_ShowsPositiveDifference()
    {
        // Arrange
        var current = CreateTestWeapon("Old Axe", QualityTier.JuryRigged, 1, 0);
        var proposed = CreateTestWeapon("New Axe", QualityTier.Scavenged, 1, 2);

        // Act
        var comparison = _equipmentService.CompareEquipment(current, proposed);

        // Assert
        Assert.That(comparison.DamageDiceDiff, Is.EqualTo(0));
        Assert.That(comparison.DamageBonusDiff, Is.EqualTo(2));
        Assert.That(comparison.IsUpgrade, Is.True);
    }

    [Test]
    public void CompareEquipment_WorseWeapon_ShowsNegativeDifference()
    {
        // Arrange
        var current = CreateTestWeapon("Good Sword", QualityTier.Optimized, 2, 3);
        var proposed = CreateTestWeapon("Old Dagger", QualityTier.JuryRigged, 1, 0);

        // Act
        var comparison = _equipmentService.CompareEquipment(current, proposed);

        // Assert
        Assert.That(comparison.DamageDiceDiff, Is.EqualTo(-1));
        Assert.That(comparison.DamageBonusDiff, Is.EqualTo(-3));
        Assert.That(comparison.IsUpgrade, Is.False);
    }

    [Test]
    public void CompareEquipment_NoCurrentEquipment_IsAlwaysUpgrade()
    {
        // Arrange
        var proposed = CreateTestWeapon("New Weapon", QualityTier.Scavenged, 1, 1);

        // Act
        var comparison = _equipmentService.CompareEquipment(null, proposed);

        // Assert
        Assert.That(comparison.IsUpgrade, Is.True);
    }

    [Test]
    public void CompareEquipment_ArmorComparison_ComparesHPAndDefense()
    {
        // Arrange
        var current = CreateTestArmor("Light Armor", QualityTier.Scavenged, hpBonus: 10, defenseBonus: 2);
        var proposed = CreateTestArmor("Heavy Armor", QualityTier.ClanForged, hpBonus: 20, defenseBonus: 4);

        // Act
        var comparison = _equipmentService.CompareEquipment(current, proposed);

        // Assert
        Assert.That(comparison.HPBonusDiff, Is.EqualTo(10));
        Assert.That(comparison.DefenseBonusDiff, Is.EqualTo(2));
        Assert.That(comparison.IsUpgrade, Is.True);
    }

    #endregion

    #region Inventory Management Tests

    [Test]
    public void PickupItem_WithInventorySpace_AddsToInventory()
    {
        // Arrange
        var player = CreateTestPlayer();
        var room = CreateTestRoom();
        var item = CreateTestWeapon("Loot Weapon", QualityTier.Scavenged, 1, 2);
        room.ItemsOnGround.Add(item);

        // Act
        var result = _equipmentService.PickupItem(player, room, item);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(player.Inventory, Contains.Item(item));
        Assert.That(room.ItemsOnGround, Does.Not.Contain(item));
    }

    [Test]
    public void PickupItem_FullInventory_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        var room = CreateTestRoom();

        // Fill inventory
        for (int i = 0; i < player.MaxInventorySize; i++)
        {
            player.Inventory.Add(CreateTestWeapon($"Item {i}", QualityTier.JuryRigged, 1, 0));
        }

        var item = CreateTestWeapon("Extra Item", QualityTier.Scavenged, 1, 2);
        room.ItemsOnGround.Add(item);

        // Act
        var result = _equipmentService.PickupItem(player, room, item);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(player.Inventory, Does.Not.Contain(item));
        Assert.That(room.ItemsOnGround, Contains.Item(item));
    }

    [Test]
    public void DropItem_FromInventory_AddsToRoom()
    {
        // Arrange
        var player = CreateTestPlayer();
        var room = CreateTestRoom();
        var item = CreateTestWeapon("Drop This", QualityTier.Scavenged, 1, 2);
        player.Inventory.Add(item);

        // Act
        var result = _equipmentService.DropItem(player, room, item);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(player.Inventory, Does.Not.Contain(item));
        Assert.That(room.ItemsOnGround, Contains.Item(item));
    }

    [Test]
    public void DropItem_NotInInventory_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        var room = CreateTestRoom();
        var item = CreateTestWeapon("Not Owned", QualityTier.Scavenged, 1, 2);

        // Act
        var result = _equipmentService.DropItem(player, room, item);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(room.ItemsOnGround, Does.Not.Contain(item));
    }

    #endregion

    #region Helper Methods

    private PlayerCharacter CreateTestPlayer()
    {
        return new PlayerCharacter
        {
            Name = "TestPlayer",
            Class = CharacterClass.Warrior,
            CurrentMilestone = 1,
            Attributes = new Attributes(might: 3, finesse: 2, wits: 2, will: 1, sturdiness: 3),
            HP = 30,
            MaxHP = 30,
            Stamina = 20,
            MaxStamina = 20,
            AP = 10,
            Inventory = new List<Equipment>(),
            MaxInventorySize = 5
        };
    }

    private Equipment CreateTestWeapon(string name, QualityTier quality, int damageDice, int damageBonus)
    {
        return new Equipment
        {
            Name = name,
            Quality = quality,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = damageDice,
            DamageBonus = damageBonus,
            StaminaCost = 5,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>()
        };
    }

    private Equipment CreateTestArmor(string name, QualityTier quality, int hpBonus, int defenseBonus)
    {
        return new Equipment
        {
            Name = name,
            Quality = quality,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = hpBonus,
            DefenseBonus = defenseBonus,
            Bonuses = new List<EquipmentBonus>()
        };
    }

    private Room CreateTestRoom()
    {
        return new Room
        {
            Id = 1,
            Name = "Test Room",
            Description = "A test room",
            Exits = new Dictionary<string, string>(),
            Enemies = new List<Enemy>(),
            ItemsOnGround = new List<Equipment>()
        };
    }

    #endregion
}
