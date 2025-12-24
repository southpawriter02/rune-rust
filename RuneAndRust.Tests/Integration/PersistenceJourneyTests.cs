using FluentAssertions;
using RuneAndRust.Core.Enums;
using RuneAndRust.Tests.Infrastructure;

namespace RuneAndRust.Tests.Integration;

/// <summary>
/// E2E integration tests for persistence (save/load) journeys.
/// These tests verify that game state survives a simulated application restart
/// by using two TestGameHost instances sharing the same database.
/// </summary>
public class PersistenceJourneyTests
{
    /// <summary>
    /// Tests that the current room location is preserved across save/load.
    /// Session A: Create character, set location, save.
    /// Session B: Load from same database, verify location matches.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesLocation()
    {
        // Arrange - Generate a shared database name for both sessions
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        Guid expectedRoomId;
        const int saveSlot = 1;

        // --- SESSION A: The Life ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("LocationTester");
            expectedRoomId = hostA.GameState.CurrentRoomId!.Value;

            // Save the game
            var saveResult = await hostA.SaveGameAsync(saveSlot);
            saveResult.Should().BeTrue("Save should succeed");
        }
        // hostA is disposed here (simulating app shutdown)

        // --- SESSION B: The Afterlife ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            // Load the saved game
            var loadResult = await hostB.LoadGameAsync(saveSlot);
            loadResult.Should().BeTrue("Load should succeed");

            // Assert: Location survived the reboot
            hostB.GameState.CurrentRoomId.Should().Be(expectedRoomId);
        }
    }

    /// <summary>
    /// Tests that character data is preserved across save/load.
    /// Verifies name, archetype, lineage, and derived stats are restored.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesCharacter()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const string expectedName = "Persistence Warrior";
        const int saveSlot = 1;

        // --- SESSION A ---
        int expectedMaxHP;
        int expectedMaxStamina;
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync(expectedName);
            var charA = hostA.GameState.CurrentCharacter!;
            expectedMaxHP = charA.MaxHP;
            expectedMaxStamina = charA.MaxStamina;

            await hostA.SaveGameAsync(saveSlot);
        }

        // --- SESSION B ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.LoadGameAsync(saveSlot);

            // Assert: Character survived the reboot
            var charB = hostB.GameState.CurrentCharacter;
            charB.Should().NotBeNull();
            charB!.Name.Should().Be(expectedName);
            charB.MaxHP.Should().Be(expectedMaxHP);
            charB.MaxStamina.Should().Be(expectedMaxStamina);
            charB.Lineage.Should().Be(LineageType.Human);
            charB.Archetype.Should().Be(ArchetypeType.Warrior);
        }
    }

    /// <summary>
    /// Tests that visited rooms (fog of war) are preserved across save/load.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesVisitedRooms()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;
        HashSet<Guid> expectedVisitedRooms;

        // --- SESSION A ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("FogOfWarTester");

            // Add the north room to visited (simulating navigation)
            var startRoomId = hostA.GameState.CurrentRoomId!.Value;
            hostA.GameState.VisitedRoomIds.Add(startRoomId);
            hostA.GameState.VisitedRoomIds.Add(Guid.NewGuid()); // Simulate visiting another room

            expectedVisitedRooms = new HashSet<Guid>(hostA.GameState.VisitedRoomIds);

            await hostA.SaveGameAsync(saveSlot);
        }

        // --- SESSION B ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.LoadGameAsync(saveSlot);

            // Assert: Visited rooms survived the reboot
            hostB.GameState.VisitedRoomIds.Should().BeEquivalentTo(expectedVisitedRooms);
        }
    }

    /// <summary>
    /// Tests that game phase is preserved across save/load.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesPhase()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;

        // --- SESSION A ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("PhaseTester");
            hostA.GameState.Phase.Should().Be(GamePhase.Exploration);

            await hostA.SaveGameAsync(saveSlot);
        }

        // --- SESSION B ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.LoadGameAsync(saveSlot);

            // Assert: Phase survived the reboot
            hostB.GameState.Phase.Should().Be(GamePhase.Exploration);
        }
    }

    /// <summary>
    /// Tests that turn count is preserved across save/load.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesTurnCount()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;
        const int expectedTurnCount = 42;

        // --- SESSION A ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("TurnCounter");
            hostA.GameState.TurnCount = expectedTurnCount;

            await hostA.SaveGameAsync(saveSlot);
        }

        // --- SESSION B ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.LoadGameAsync(saveSlot);

            // Assert: Turn count survived the reboot
            hostB.GameState.TurnCount.Should().Be(expectedTurnCount);
        }
    }

    /// <summary>
    /// Tests that different save slots are isolated from each other.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_DifferentSlotsAreIsolated()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";

        // --- Create two different saves in different slots ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("Warrior One");
            await hostA.SaveGameAsync(1);
        }

        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.SetupExplorationAsync("Warrior Two");
            await hostB.SaveGameAsync(2);
        }

        // --- Load slot 1, verify it's "Warrior One" ---
        using (var hostC = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostC.LoadGameAsync(1);
            hostC.GameState.CurrentCharacter!.Name.Should().Be("Warrior One");
        }

        // --- Load slot 2, verify it's "Warrior Two" ---
        using (var hostD = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostD.LoadGameAsync(2);
            hostD.GameState.CurrentCharacter!.Name.Should().Be("Warrior Two");
        }
    }

    /// <summary>
    /// Tests that saving to an existing slot overwrites the previous save.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_OverwritesExistingSlot()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;

        // --- First save ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("Original Save");
            hostA.GameState.TurnCount = 10;
            await hostA.SaveGameAsync(saveSlot);
        }

        // --- Overwrite with new save ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.SetupExplorationAsync("Overwritten Save");
            hostB.GameState.TurnCount = 99;
            await hostB.SaveGameAsync(saveSlot);
        }

        // --- Load should get the overwritten data ---
        using (var hostC = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostC.LoadGameAsync(saveSlot);

            hostC.GameState.CurrentCharacter!.Name.Should().Be("Overwritten Save");
            hostC.GameState.TurnCount.Should().Be(99);
        }
    }

    /// <summary>
    /// Tests that loading a non-existent slot returns false.
    /// </summary>
    [Fact]
    public async Task Journey_Load_NonExistentSlot_ReturnsFalse()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";

        using var host = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName);

        // Act - Try to load from empty database
        var loadResult = await host.LoadGameAsync(1);

        // Assert
        loadResult.Should().BeFalse("Loading from non-existent slot should fail");
    }

    /// <summary>
    /// Tests that the database name property is correctly exposed for reuse.
    /// </summary>
    [Fact]
    public void Journey_DatabaseName_IsExposedForReuse()
    {
        // Arrange
        var expectedDbName = $"CustomDb_{Guid.NewGuid()}";

        // Act
        using var host = TestGameHost.Create(seed: 42, new[] { "quit" }, expectedDbName);

        // Assert
        host.DatabaseName.Should().Be(expectedDbName);
    }

    /// <summary>
    /// Tests that hosts with the same database name share persisted data.
    /// </summary>
    [Fact]
    public async Task Journey_SharedDatabase_PersistsAcrossHosts()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;

        // --- Host A: Save data ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("SharedDbTest");
            await hostA.SaveGameAsync(saveSlot);
        }

        // --- Host B: Different host, same database, should see the save ---
        using (var hostB = TestGameHost.Create(seed: 99, new[] { "quit" }, sharedDbName))
        {
            // Verify the save exists by loading it
            var loadResult = await hostB.LoadGameAsync(saveSlot);
            loadResult.Should().BeTrue("Save from Host A should be visible in Host B");
            hostB.GameState.CurrentCharacter!.Name.Should().Be("SharedDbTest");
        }
    }

    /// <summary>
    /// Tests that character inventory count is preserved across save/load.
    /// Uses item IDs and quantities since InventoryItem references Item entities.
    /// </summary>
    [Fact]
    public async Task Journey_SaveLoad_PreservesInventoryCount()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;
        var testItemId1 = Guid.NewGuid();
        var testItemId2 = Guid.NewGuid();

        // --- SESSION A ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync("InventoryTester");

            // Add test items to inventory (just IDs and quantities for serialization)
            var character = hostA.GameState.CurrentCharacter!;
            character.Inventory.Add(new RuneAndRust.Core.Entities.InventoryItem
            {
                ItemId = testItemId1,
                Quantity = 5
            });
            character.Inventory.Add(new RuneAndRust.Core.Entities.InventoryItem
            {
                ItemId = testItemId2,
                Quantity = 1
            });

            await hostA.SaveGameAsync(saveSlot);
        }

        // --- SESSION B ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostB.LoadGameAsync(saveSlot);

            // Assert: Inventory count survived the reboot
            var inventory = hostB.GameState.CurrentCharacter!.Inventory;
            inventory.Should().HaveCount(2);
            inventory.Should().Contain(i => i.ItemId == testItemId1 && i.Quantity == 5);
            inventory.Should().Contain(i => i.ItemId == testItemId2 && i.Quantity == 1);
        }
    }

    /// <summary>
    /// Tests a complete "Survivor's Cycle" journey with multiple state changes.
    /// Simulates a realistic play session: explore, take damage, advance turns, save, quit, reload.
    /// </summary>
    [Fact]
    public async Task Journey_SurvivorsCycle_CompleteStatePreservation()
    {
        // Arrange
        var sharedDbName = $"PersistenceTest_{Guid.NewGuid()}";
        const int saveSlot = 1;

        Guid expectedRoomId;
        int expectedTurnCount = 15;
        int expectedCurrentHP;
        string expectedCharName = "The Survivor";
        var testItemId = Guid.NewGuid();

        // --- SESSION A: The Life ---
        using (var hostA = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            await hostA.SetupExplorationAsync(expectedCharName);

            var gameState = hostA.GameState;
            var character = gameState.CurrentCharacter!;

            // Simulate gameplay: take damage, advance turns
            character.CurrentHP -= 20; // Take some damage
            expectedCurrentHP = character.CurrentHP;
            gameState.TurnCount = expectedTurnCount;
            expectedRoomId = gameState.CurrentRoomId!.Value;

            // Add some inventory
            character.Inventory.Add(new RuneAndRust.Core.Entities.InventoryItem
            {
                ItemId = testItemId,
                Quantity = 3
            });

            // Visit another room (fog of war)
            gameState.VisitedRoomIds.Add(Guid.NewGuid());

            // Save the game
            var saveResult = await hostA.SaveGameAsync(saveSlot);
            saveResult.Should().BeTrue();
        }
        // hostA disposed - app shutdown

        // --- SESSION B: The Afterlife ---
        using (var hostB = TestGameHost.Create(seed: 42, new[] { "quit" }, sharedDbName))
        {
            var loadResult = await hostB.LoadGameAsync(saveSlot);
            loadResult.Should().BeTrue();

            var gameState = hostB.GameState;
            var character = gameState.CurrentCharacter;

            // Assert: Complete state verification
            character.Should().NotBeNull();
            character!.Name.Should().Be(expectedCharName);
            character.CurrentHP.Should().Be(expectedCurrentHP);
            character.Inventory.Should().ContainSingle(i => i.ItemId == testItemId && i.Quantity == 3);

            gameState.CurrentRoomId.Should().Be(expectedRoomId);
            gameState.TurnCount.Should().Be(expectedTurnCount);
            gameState.VisitedRoomIds.Should().HaveCountGreaterThan(1);
            gameState.Phase.Should().Be(GamePhase.Exploration);
        }
    }
}
