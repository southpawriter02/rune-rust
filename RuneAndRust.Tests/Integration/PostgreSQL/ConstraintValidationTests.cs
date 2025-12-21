using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;

namespace RuneAndRust.Tests.Integration.PostgreSQL;

/// <summary>
/// Tests for PostgreSQL database constraints including unique indexes,
/// foreign keys, and required field validation.
/// </summary>
[Collection("PostgreSQL")]
[Trait("Category", "PostgreSQL")]
public class ConstraintValidationTests
{
    private readonly PostgreSqlTestFixture _fixture;

    public ConstraintValidationTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Unique Constraint Tests

    [Fact]
    public async Task Character_Name_UniqueConstraintEnforced()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var uniqueName = $"UniqueChar_{Guid.NewGuid():N}";

        var character1 = new Character
        {
            Name = uniqueName,
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        context.Characters.Add(character1);
        await context.SaveChangesAsync();

        // Act - Try to add another character with the same name
        var character2 = new Character
        {
            Name = uniqueName,
            Lineage = LineageType.IronBlooded,
            Archetype = ArchetypeType.Adept
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.Characters.Add(character2);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23505"); // Unique violation
    }

    [Fact]
    public async Task SaveGame_SlotNumber_UniqueConstraintEnforced()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var slotNumber = 1000 + new Random().Next(1, 1000);

        var save1 = new SaveGame
        {
            SlotNumber = slotNumber,
            CharacterName = "First Save",
            SerializedState = "{}"
        };

        context.SaveGames.Add(save1);
        await context.SaveChangesAsync();

        // Act - Try to add another save in the same slot
        var save2 = new SaveGame
        {
            SlotNumber = slotNumber,
            CharacterName = "Second Save",
            SerializedState = "{}"
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.SaveGames.Add(save2);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23505");
    }

    [Fact]
    public async Task CodexEntry_Title_UniqueConstraintEnforced()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var uniqueTitle = $"UniqueCodex_{Guid.NewGuid():N}";

        var entry1 = new CodexEntry
        {
            Title = uniqueTitle,
            Category = EntryCategory.Bestiary,
            FullText = "First entry content",
            TotalFragments = 3
        };

        context.CodexEntries.Add(entry1);
        await context.SaveChangesAsync();

        // Act
        var entry2 = new CodexEntry
        {
            Title = uniqueTitle,
            Category = EntryCategory.Factions,
            FullText = "Different content",
            TotalFragments = 5
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.CodexEntries.Add(entry2);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23505");
    }

    [Fact]
    public async Task ActiveAbility_Name_UniqueConstraintEnforced()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var uniqueName = $"UniqueAbility_{Guid.NewGuid():N}";

        var ability1 = new ActiveAbility
        {
            Name = uniqueName,
            Description = "First ability",
            EffectScript = "damage:10",
            Archetype = ArchetypeType.Warrior,
            Tier = 1
        };

        context.ActiveAbilities.Add(ability1);
        await context.SaveChangesAsync();

        // Act
        var ability2 = new ActiveAbility
        {
            Name = uniqueName,
            Description = "Different ability",
            EffectScript = "heal:5",
            Archetype = ArchetypeType.Adept,
            Tier = 2
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.ActiveAbilities.Add(ability2);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23505");
    }

    #endregion

    #region Foreign Key Constraint Tests

    [Fact]
    public async Task InventoryItem_RequiresValidCharacter()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var item = new Item
        {
            Name = "FK Test Item",
            ItemType = ItemType.Material,
            Description = "Testing foreign key"
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Act - Try to create InventoryItem with non-existent character
        var inventoryItem = new InventoryItem
        {
            CharacterId = Guid.NewGuid(), // Non-existent
            ItemId = item.Id,
            Quantity = 1
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.InventoryItems.Add(inventoryItem);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23503"); // Foreign key violation
    }

    [Fact]
    public async Task InventoryItem_RequiresValidItem()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            Name = $"FK_Test_Char_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Act - Try to create InventoryItem with non-existent item
        var inventoryItem = new InventoryItem
        {
            CharacterId = character.Id,
            ItemId = Guid.NewGuid(), // Non-existent
            Quantity = 1
        };

        using var freshContext = _fixture.CreateContext();
        freshContext.InventoryItems.Add(inventoryItem);

        // Assert
        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            async () => await freshContext.SaveChangesAsync());

        var pgException = exception.InnerException as PostgresException;
        pgException.Should().NotBeNull();
        pgException!.SqlState.Should().Be("23503");
    }

    [Fact]
    public async Task DataCapture_CodexEntryId_AllowsNull()
    {
        // Arrange - DataCapture can exist without being linked to a CodexEntry
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            Name = $"DataCapture_Test_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        context.Characters.Add(character);
        await context.SaveChangesAsync();

        var capture = new DataCapture
        {
            CharacterId = character.Id,
            CodexEntryId = null, // Explicitly null - unassigned fragment
            Type = CaptureType.TextFragment,
            FragmentContent = "An unassigned knowledge fragment",
            Source = "Field observation",
            Quality = 15
        };

        // Act
        context.DataCaptures.Add(capture);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.DataCaptures.FindAsync(capture.Id);

        loaded.Should().NotBeNull();
        loaded!.CodexEntryId.Should().BeNull();
    }

    #endregion

    #region Cascade Delete Tests

    [Fact]
    public async Task Character_Delete_CascadesInventoryItems()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            Name = $"Cascade_Test_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior
        };

        var item = new Item
        {
            Name = $"Cascade_Item_{Guid.NewGuid():N}",
            ItemType = ItemType.Material,
            Description = "Will be orphaned"
        };

        context.Characters.Add(character);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var inventoryItem = new InventoryItem
        {
            CharacterId = character.Id,
            ItemId = item.Id,
            Quantity = 5
        };

        context.InventoryItems.Add(inventoryItem);
        await context.SaveChangesAsync();

        // Act - Delete the character
        context.Characters.Remove(character);
        await context.SaveChangesAsync();

        // Assert - InventoryItem should be deleted, Item should remain
        using var freshContext = _fixture.CreateContext();

        var remainingInventory = await freshContext.InventoryItems
            .Where(ii => ii.CharacterId == character.Id)
            .ToListAsync();
        remainingInventory.Should().BeEmpty();

        var remainingItem = await freshContext.Items.FindAsync(item.Id);
        remainingItem.Should().NotBeNull(); // Item itself is not deleted
    }

    [Fact]
    public async Task Item_Delete_CascadesItemProperties()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var item = new Item
        {
            Name = $"PropertyCascade_{Guid.NewGuid():N}",
            ItemType = ItemType.Weapon,
            Description = "Has properties"
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        var property = new ItemProperty
        {
            Name = "Test Enchantment",
            StatModifiers = new Dictionary<string, int> { { "Might", 2 } }
        };

        item.Properties.Add(property);
        await context.SaveChangesAsync();

        var propertyId = property.Id;

        // Act - Delete the item
        context.Items.Remove(item);
        await context.SaveChangesAsync();

        // Assert - ItemProperty should be deleted
        using var freshContext = _fixture.CreateContext();
        var remainingProperty = await freshContext.ItemProperties.FindAsync(propertyId);
        remainingProperty.Should().BeNull();
    }

    #endregion

    #region Required Field Tests

    [Fact]
    public async Task Room_Name_Required()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var room = new Room
        {
            Name = null!, // Required field
            Description = "Has description"
        };

        context.Rooms.Add(room);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task Item_Description_Required()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var item = new Item
        {
            Name = "Test Item",
            ItemType = ItemType.Material,
            Description = null! // Required field
        };

        context.Items.Add(item);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            async () => await context.SaveChangesAsync());
    }

    #endregion

    #region TPH Discriminator Tests

    [Fact]
    public async Task TPH_Equipment_LoadsCorrectType()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var weapon = new Equipment
        {
            Name = $"TPH_Weapon_{Guid.NewGuid():N}",
            ItemType = ItemType.Weapon,
            Description = "Testing TPH inheritance",
            Slot = EquipmentSlot.MainHand,
            DamageDie = 6,
            SoakBonus = 0
        };

        context.Equipment.Add(weapon);
        await context.SaveChangesAsync();

        // Act - Query via base Item type
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Items.FindAsync(weapon.Id);

        // Assert - Should be loaded as Equipment, not Item
        loaded.Should().BeOfType<Equipment>();
        var loadedEquipment = (Equipment)loaded!;
        loadedEquipment.DamageDie.Should().Be(6);
        loadedEquipment.Slot.Should().Be(EquipmentSlot.MainHand);
    }

    [Fact]
    public async Task TPH_Item_LoadsCorrectType()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var material = new Item
        {
            Name = $"TPH_Material_{Guid.NewGuid():N}",
            ItemType = ItemType.Material,
            Description = "A basic material",
            IsStackable = true
        };

        context.Items.Add(material);
        await context.SaveChangesAsync();

        // Act
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Items.FindAsync(material.Id);

        // Assert - Should be Item, not Equipment
        loaded.Should().BeOfType<Item>();
        loaded.Should().NotBeOfType<Equipment>();
    }

    #endregion
}
