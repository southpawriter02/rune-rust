using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using RuneAndRust.Engine.Commands;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.37.3: Unit tests for Inventory & Equipment Commands
/// Tests for: inventory, equipment, take, drop, use
/// </summary>
[TestClass]
public class InventoryCommandsTests
{
    private EquipmentService _equipmentService = null!;

    [TestInitialize]
    public void Setup()
    {
        _equipmentService = new EquipmentService();
    }

    #region Test Helpers

    /// <summary>
    /// Create a basic test game state with player and room
    /// </summary>
    private GameState CreateTestGameState()
    {
        var player = new PlayerCharacter
        {
            CharacterID = 1,
            Name = "Test Hero",
            Class = CharacterClass.Warrior,
            HP = 100,
            MaxHP = 110,
            Stamina = 80,
            MaxStamina = 80,
            MaxInventorySize = 5,
            MaxConsumables = 10,
            Currency = 50,
            Attributes = new Attributes
            {
                Might = 4,
                Finesse = 3,
                Wits = 2,
                Will = 2,
                Sturdiness = 4
            }
        };

        var room = new Room
        {
            RoomId = "test_room_1",
            Name = "Test Chamber",
            Description = "A test room for unit tests."
        };

        return new GameState
        {
            Player = player,
            CurrentRoom = room,
            CurrentPhase = GamePhase.Exploration
        };
    }

    /// <summary>
    /// Create a test weapon
    /// </summary>
    private Equipment CreateWeapon(string name, int damageDice = 1, int damageBonus = 0, QualityTier quality = QualityTier.Scavenged)
    {
        return new Equipment
        {
            Name = name,
            Type = EquipmentType.Weapon,
            Quality = quality,
            WeaponCategory = WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = damageDice,
            DamageBonus = damageBonus,
            StaminaCost = 5,
            AccuracyBonus = 0
        };
    }

    /// <summary>
    /// Create a test armor
    /// </summary>
    private Equipment CreateArmor(string name, int defenseBonus = 0, int hpBonus = 0, QualityTier quality = QualityTier.Scavenged)
    {
        return new Equipment
        {
            Name = name,
            Type = EquipmentType.Armor,
            Quality = quality,
            ArmorCategory = ArmorCategory.Medium,
            DefenseBonus = defenseBonus,
            HPBonus = hpBonus
        };
    }

    /// <summary>
    /// Create a test consumable
    /// </summary>
    private Consumable CreateConsumable(string name, int hpRestore = 0, int staminaRestore = 0)
    {
        return new Consumable
        {
            Name = name,
            Type = ConsumableType.Medicine,
            HPRestore = hpRestore,
            StaminaRestore = staminaRestore
        };
    }

    #endregion

    #region InventoryCommand Tests

    [TestMethod]
    public void Inventory_EmptyInventory_ShowsEmptyMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new InventoryCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("(empty)"));
    }

    [TestMethod]
    public void Inventory_WithItems_DisplaysAllItems()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Inventory.Add(CreateWeapon("Iron Axe", 2, 0));
        state.Player.Inventory.Add(CreateArmor("Scavenged Plate", 6, 10));
        state.Player.Consumables.Add(CreateConsumable("Healing Poultice", 20));
        state.Player.Consumables.Add(CreateConsumable("Healing Poultice", 20));
        state.Player.CraftingComponents[ComponentType.IronOre] = 5;

        var command = new InventoryCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Iron Axe"));
        Assert.IsTrue(result.Message.Contains("Scavenged Plate"));
        Assert.IsTrue(result.Message.Contains("Healing Poultice"));
        Assert.IsTrue(result.Message.Contains("(x2)")); // Two potions grouped
        Assert.IsTrue(result.Message.Contains("IronOre"));
    }

    [TestMethod]
    public void Inventory_ShowsCapacity_Correctly()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Inventory.Add(CreateWeapon("Iron Axe", 2, 0));
        state.Player.MaxInventorySize = 5;

        var command = new InventoryCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("1/5"));
        Assert.IsTrue(result.Message.Contains("Normal")); // Capacity status
    }

    #endregion

    #region EquipmentCommand Tests

    [TestMethod]
    public void Equipment_NoEquipment_ShowsEmptySlots()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new EquipmentCommand(_equipmentService);

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("(empty)"));
        Assert.IsTrue(result.Message.Contains("MainHand"));
    }

    [TestMethod]
    public void Equipment_WithEquipment_DisplaysEquippedItems()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.EquippedWeapon = CreateWeapon("Iron Axe", 2, 1);
        state.Player.EquippedArmor = CreateArmor("Scavenged Plate", 6, 10);

        var command = new EquipmentCommand(_equipmentService);

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Iron Axe"));
        Assert.IsTrue(result.Message.Contains("Scavenged Plate"));
        Assert.IsTrue(result.Message.Contains("2d6+1")); // Weapon damage
    }

    [TestMethod]
    public void Equipment_DisplaysStats_Correctly()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.HP = 95;
        state.Player.MaxHP = 110;
        state.Player.Stamina = 60;
        state.Player.MaxStamina = 80;
        state.Player.EquippedArmor = CreateArmor("Test Armor", 5, 0);

        var command = new EquipmentCommand(_equipmentService);

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("95/110")); // HP
        Assert.IsTrue(result.Message.Contains("60/80")); // Stamina
        Assert.IsTrue(result.Message.Contains("Defense:  5")); // Defense from armor
    }

    #endregion

    #region TakeCommand Tests

    [TestMethod]
    public void Take_ItemOnGround_AddsToInventory()
    {
        // Arrange
        var state = CreateTestGameState();
        var item = CreateWeapon("Iron Axe", 2, 0);
        state.CurrentRoom.ItemsOnGround.Add(item);

        var command = new TakeCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "axe" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("You take the Iron Axe"));
        Assert.AreEqual(1, state.Player.Inventory.Count);
        Assert.AreEqual(0, state.CurrentRoom.ItemsOnGround.Count);
    }

    [TestMethod]
    public void Take_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new TakeCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "nonexistent" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("no 'nonexistent' here"));
    }

    [TestMethod]
    public void Take_InventoryFull_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.MaxInventorySize = 2;
        state.Player.Inventory.Add(CreateWeapon("Sword 1", 1, 0));
        state.Player.Inventory.Add(CreateWeapon("Sword 2", 1, 0));

        var item = CreateWeapon("Sword 3", 1, 0);
        state.CurrentRoom.ItemsOnGround.Add(item);

        var command = new TakeCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "sword 3" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("inventory is full"));
    }

    [TestMethod]
    public void Take_NoArguments_ReturnsUsageMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new TakeCommand(_equipmentService);

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Take what?"));
    }

    #endregion

    #region DropCommand Tests

    [TestMethod]
    public void Drop_ItemInInventory_RemovesFromInventory()
    {
        // Arrange
        var state = CreateTestGameState();
        var item = CreateWeapon("Iron Axe", 2, 0);
        state.Player.Inventory.Add(item);

        var command = new DropCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "axe" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("You drop the Iron Axe"));
        Assert.AreEqual(0, state.Player.Inventory.Count);
        Assert.AreEqual(1, state.CurrentRoom.ItemsOnGround.Count);
    }

    [TestMethod]
    public void Drop_ItemNotFound_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new DropCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "nonexistent" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("don't have"));
    }

    [TestMethod]
    public void Drop_EmptyInventory_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new DropCommand(_equipmentService);

        // Act
        var result = command.Execute(state, new[] { "anything" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("inventory is empty"));
    }

    [TestMethod]
    public void Drop_NoArguments_ReturnsUsageMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new DropCommand(_equipmentService);

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Drop what?"));
    }

    #endregion

    #region UseCommand Tests

    [TestMethod]
    public void Use_HealingConsumable_RestoresHP()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.HP = 75;
        state.Player.MaxHP = 110;
        state.Player.Consumables.Add(CreateConsumable("Healing Poultice", 20));

        var command = new UseCommand();

        // Act
        var result = command.Execute(state, new[] { "poultice" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(95, state.Player.HP); // 75 + 20
        Assert.AreEqual(0, state.Player.Consumables.Count); // Consumed
        Assert.IsTrue(result.Message.Contains("+20 HP"));
    }

    [TestMethod]
    public void Use_StaminaConsumable_RestoresStamina()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.Stamina = 30;
        state.Player.MaxStamina = 80;
        state.Player.Consumables.Add(CreateConsumable("Stamina Tonic", 0, 25));

        var command = new UseCommand();

        // Act
        var result = command.Execute(state, new[] { "tonic" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(55, state.Player.Stamina); // 30 + 25
        Assert.AreEqual(0, state.Player.Consumables.Count);
        Assert.IsTrue(result.Message.Contains("+25 Stamina"));
    }

    [TestMethod]
    public void Use_ConsumableAtMaxHP_CapAtMax()
    {
        // Arrange
        var state = CreateTestGameState();
        state.Player.HP = 110;
        state.Player.MaxHP = 110;
        state.Player.Consumables.Add(CreateConsumable("Healing Poultice", 20));

        var command = new UseCommand();

        // Act
        var result = command.Execute(state, new[] { "poultice" });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(110, state.Player.HP); // Still at max
        Assert.IsTrue(result.Message.Contains("+0 HP")); // No overheal
    }

    [TestMethod]
    public void Use_ConsumableNotFound_ReturnsFailure()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new UseCommand();

        // Act
        var result = command.Execute(state, new[] { "nonexistent" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("don't have"));
    }

    [TestMethod]
    public void Use_NoArguments_ReturnsUsageMessage()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new UseCommand();

        // Act
        var result = command.Execute(state, Array.Empty<string>());

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Use what?"));
    }

    [TestMethod]
    public void Use_WithOnKeyword_ReturnsNotImplemented()
    {
        // Arrange
        var state = CreateTestGameState();
        var command = new UseCommand();

        // Act
        var result = command.Execute(state, new[] { "key", "on", "door" });

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not yet implemented"));
    }

    #endregion
}
