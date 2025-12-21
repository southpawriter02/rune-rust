using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using Xunit;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Tests.Integration.PostgreSQL;

/// <summary>
/// Tests for PostgreSQL JSONB column storage and querying.
/// Validates that complex dictionary/object types are correctly serialized and deserialized.
/// </summary>
[Collection("PostgreSQL")]
[Trait("Category", "PostgreSQL")]
public class JsonbQueryTests
{
    private readonly PostgreSqlTestFixture _fixture;

    public JsonbQueryTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Room.Exits JSONB Tests

    [Fact]
    public async Task Room_Exits_PersistsJsonbDictionary()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var room = new Room
        {
            Name = "Test Chamber",
            Description = "A room for testing JSONB",
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, Guid.NewGuid() },
                { Direction.South, Guid.NewGuid() },
                { Direction.East, Guid.NewGuid() }
            }
        };

        // Act
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        // Assert - Query with fresh context
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Rooms.FindAsync(room.Id);

        loaded.Should().NotBeNull();
        loaded!.Exits.Should().HaveCount(3);
        loaded.Exits.Should().ContainKey(Direction.North);
        loaded.Exits.Should().ContainKey(Direction.South);
        loaded.Exits.Should().ContainKey(Direction.East);
    }

    [Fact]
    public async Task Room_Exits_UpdatesJsonbDictionary()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var room = new Room
        {
            Name = "Changing Room",
            Description = "A room with exits that change",
            Exits = new Dictionary<Direction, Guid>
            {
                { Direction.North, Guid.NewGuid() }
            }
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        // Act - Add more exits
        var westRoomId = Guid.NewGuid();
        room.Exits[Direction.West] = westRoomId;
        room.Exits[Direction.Down] = Guid.NewGuid();
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Rooms.FindAsync(room.Id);

        loaded!.Exits.Should().HaveCount(3);
        loaded.Exits[Direction.West].Should().Be(westRoomId);
    }

    [Fact]
    public async Task Room_Exits_EmptyDictionaryPersists()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var room = new Room
        {
            Name = "Dead End",
            Description = "No exits here",
            Exits = new Dictionary<Direction, Guid>()
        };

        // Act
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Rooms.FindAsync(room.Id);

        loaded!.Exits.Should().NotBeNull();
        loaded.Exits.Should().BeEmpty();
    }

    #endregion

    #region Character.EquipmentBonuses JSONB Tests

    [Fact]
    public async Task Character_EquipmentBonuses_PersistsJsonbDictionary()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var character = new Character
        {
            Name = $"JSONB_Test_{Guid.NewGuid():N}",
            Lineage = LineageType.Human,
            Archetype = ArchetypeType.Warrior,
            EquipmentBonuses = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 2 },
                { CharacterAttribute.Might, 3 },
                { CharacterAttribute.Finesse, 1 }
            }
        };

        // Act
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Characters.FindAsync(character.Id);

        loaded!.EquipmentBonuses.Should().HaveCount(3);
        loaded.EquipmentBonuses[CharacterAttribute.Sturdiness].Should().Be(2);
        loaded.EquipmentBonuses[CharacterAttribute.Might].Should().Be(3);
        loaded.EquipmentBonuses[CharacterAttribute.Finesse].Should().Be(1);
    }

    #endregion

    #region Equipment JSONB Tests

    [Fact]
    public async Task Equipment_AttributeBonuses_PersistsJsonbDictionary()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var sword = new Equipment
        {
            Name = "JSONB Test Sword",
            ItemType = ItemType.Weapon,
            Description = "A sword for testing",
            Slot = EquipmentSlot.MainHand,
            DamageDie = 8,
            AttributeBonuses = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Might, 2 }
            },
            Requirements = new Dictionary<CharacterAttribute, int>
            {
                { CharacterAttribute.Sturdiness, 3 }
            }
        };

        // Act
        context.Equipment.Add(sword);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Equipment.FindAsync(sword.Id);

        loaded!.AttributeBonuses.Should().ContainKey(CharacterAttribute.Might);
        loaded.AttributeBonuses[CharacterAttribute.Might].Should().Be(2);
        loaded.Requirements.Should().ContainKey(CharacterAttribute.Sturdiness);
        loaded.Requirements[CharacterAttribute.Sturdiness].Should().Be(3);
    }

    #endregion

    #region ItemProperty.StatModifiers JSONB Tests

    [Fact]
    public async Task ItemProperty_StatModifiers_PersistsJsonbDictionary()
    {
        // Arrange - Create an item first
        using var context = _fixture.CreateContext();
        var item = new Item
        {
            Name = "Enchanted Ring",
            ItemType = ItemType.Consumable,
            Description = "A ring to test properties"
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Add a property
        var property = new ItemProperty
        {
            Name = "Rune of Fortitude",
            Description = "Increases resilience",
            StatModifiers = new Dictionary<string, int>
            {
                { "Sturdiness", 2 },
                { "MaxHP", 10 },
                { "Soak", 1 }
            }
        };

        item.Properties.Add(property);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Items
            .Include(i => i.Properties)
            .FirstOrDefaultAsync(i => i.Id == item.Id);

        loaded!.Properties.Should().HaveCount(1);
        var loadedProperty = loaded.Properties.First();
        loadedProperty.StatModifiers.Should().HaveCount(3);
        loadedProperty.StatModifiers["Sturdiness"].Should().Be(2);
        loadedProperty.StatModifiers["MaxHP"].Should().Be(10);
        loadedProperty.StatModifiers["Soak"].Should().Be(1);
    }

    [Fact]
    public async Task ItemProperty_MultipleProperties_PersistIndependently()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var item = new Item
        {
            Name = "Multi-Enchanted Blade",
            ItemType = ItemType.Weapon,
            Description = "A blade with multiple enchantments"
        };

        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Add multiple properties
        item.Properties.Add(new ItemProperty
        {
            Name = "Rune of Might",
            StatModifiers = new Dictionary<string, int> { { "Might", 3 } }
        });

        item.Properties.Add(new ItemProperty
        {
            Name = "Rune of Speed",
            StatModifiers = new Dictionary<string, int> { { "Initiative", 2 }, { "Finesse", 1 } }
        });

        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.Items
            .Include(i => i.Properties)
            .FirstOrDefaultAsync(i => i.Id == item.Id);

        loaded!.Properties.Should().HaveCount(2);

        var mightRune = loaded.Properties.First(p => p.Name == "Rune of Might");
        mightRune.StatModifiers["Might"].Should().Be(3);

        var speedRune = loaded.Properties.First(p => p.Name == "Rune of Speed");
        speedRune.StatModifiers.Should().HaveCount(2);
    }

    #endregion

    #region CodexEntry.UnlockThresholds JSONB Tests

    [Fact]
    public async Task CodexEntry_UnlockThresholds_PersistsJsonbDictionary()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var entry = new CodexEntry
        {
            Title = $"JSONB_Codex_{Guid.NewGuid():N}",
            Category = EntryCategory.Bestiary,
            FullText = "Complete text of the entry",
            TotalFragments = 5,
            UnlockThresholds = new Dictionary<int, string>
            {
                { 1, "You've heard rumors of this creature..." },
                { 3, "Field observations suggest..." },
                { 5, "Complete analysis reveals..." }
            }
        };

        // Act
        context.CodexEntries.Add(entry);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.CodexEntries.FindAsync(entry.Id);

        loaded!.UnlockThresholds.Should().HaveCount(3);
        loaded.UnlockThresholds[1].Should().Contain("rumors");
        loaded.UnlockThresholds[3].Should().Contain("Field observations");
        loaded.UnlockThresholds[5].Should().Contain("Complete analysis");
    }

    #endregion

    #region SaveGame.SerializedState JSONB Tests

    [Fact]
    public async Task SaveGame_SerializedState_PersistsLargeJsonb()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var complexState = new Dictionary<string, object>
        {
            { "version", "0.3.1e" },
            { "currentRoom", Guid.NewGuid().ToString() },
            { "questFlags", new[] { "main_quest_started", "tutorial_complete" } },
            { "discoveredRooms", Enumerable.Range(1, 50).Select(_ => Guid.NewGuid().ToString()).ToList() }
        };

        var serializedState = System.Text.Json.JsonSerializer.Serialize(complexState);

        var save = new SaveGame
        {
            SlotNumber = 99,
            CharacterName = "JSONB State Test",
            SerializedState = serializedState
        };

        // Act
        context.SaveGames.Add(save);
        await context.SaveChangesAsync();

        // Assert
        using var freshContext = _fixture.CreateContext();
        var loaded = await freshContext.SaveGames.FindAsync(save.Id);

        loaded!.SerializedState.Should().NotBeNullOrEmpty();
        loaded.SerializedState.Should().Contain("main_quest_started");
        loaded.SerializedState.Should().Contain("0.3.1e");
    }

    #endregion
}
