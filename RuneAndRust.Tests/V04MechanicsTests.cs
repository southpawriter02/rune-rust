using RuneAndRust.Core;
using RuneAndRust.Engine;
using Xunit;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.4 Mechanics Validation Tests
/// Validates new features: loot placement, talk mechanic, environmental hazards, etc.
/// </summary>
public class V04MechanicsTests
{
    #region Loot Placement Tests

    [Fact]
    public void V04_OperationsCenter_ShouldHaveClanForgedLoot()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "Test");

        // Act
        world.AddStartingLoot(player);
        var operationsCenter = world.GetRoom("Operations Center");

        // Assert
        Assert.NotEmpty(operationsCenter.ItemsOnGround);
        Assert.Equal(2, operationsCenter.ItemsOnGround.Count);

        var clanForgedItems = operationsCenter.ItemsOnGround
            .Where(i => i.Quality == QualityTier.ClanForged)
            .ToList();

        Assert.Equal(2, clanForgedItems.Count);
        Assert.Contains(clanForgedItems, i => i.Type == EquipmentType.Weapon);
        Assert.Contains(clanForgedItems, i => i.Type == EquipmentType.Armor);
    }

    [Fact]
    public void V04_SecretRoom_ShouldHaveMythForgedLoot()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Mystic, "Test");

        // Act
        world.AddSecretRoomLoot(player);
        var supplyCache = world.GetRoom("Supply Cache");

        // Assert
        Assert.NotEmpty(supplyCache.ItemsOnGround);
        Assert.Equal(3, supplyCache.ItemsOnGround.Count);

        var mythForgedItems = supplyCache.ItemsOnGround
            .Where(i => i.Quality == QualityTier.MythForged)
            .ToList();

        Assert.Equal(3, mythForgedItems.Count);
    }

    [Fact]
    public void V04_SecretRoom_ShouldOnlyPlaceLootOnce()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Scavenger, "Test");

        // Act
        world.AddSecretRoomLoot(player);
        var firstCount = world.GetRoom("Supply Cache").ItemsOnGround.Count;

        world.AddSecretRoomLoot(player); // Call again
        var secondCount = world.GetRoom("Supply Cache").ItemsOnGround.Count;

        // Assert
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(3, secondCount);
    }

    [Theory]
    [InlineData(CharacterClass.Warrior)]
    [InlineData(CharacterClass.Scavenger)]
    [InlineData(CharacterClass.Mystic)]
    public void V04_LootPlacement_ShouldBeClassAppropriate(CharacterClass characterClass)
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(characterClass, "Test");

        // Act
        world.AddStartingLoot(player);
        world.AddSecretRoomLoot(player);

        var operationsCenter = world.GetRoom("Operations Center");
        var supplyCache = world.GetRoom("Supply Cache");

        // Assert - Operations Center weapon should be class-appropriate
        var weapon = operationsCenter.ItemsOnGround.FirstOrDefault(i => i.Type == EquipmentType.Weapon);
        Assert.NotNull(weapon);

        var expectedCategory = characterClass switch
        {
            CharacterClass.Warrior => new[] { WeaponCategory.Axe, WeaponCategory.Greatsword },
            CharacterClass.Scavenger => new[] { WeaponCategory.Spear, WeaponCategory.Dagger },
            CharacterClass.Mystic => new[] { WeaponCategory.Staff, WeaponCategory.Focus },
            _ => throw new ArgumentException("Unknown class")
        };

        Assert.Contains(weapon.WeaponCategory!.Value, expectedCategory);

        // Assert - Secret room weapons should also be class-appropriate
        var secretWeapons = supplyCache.ItemsOnGround.Where(i => i.Type == EquipmentType.Weapon).ToList();
        Assert.NotEmpty(secretWeapons);

        foreach (var secretWeapon in secretWeapons)
        {
            Assert.Contains(secretWeapon.WeaponCategory!.Value, expectedCategory);
        }
    }

    #endregion

    #region Room Structure Tests

    [Fact]
    public void V04_GameWorld_ShouldHave15Rooms()
    {
        // Arrange & Act
        var world = new GameWorld();

        // Assert
        Assert.Equal(15, world.Rooms.Count);
    }

    [Fact]
    public void V04_OperationsCenter_ShouldHaveBranchingExits()
    {
        // Arrange & Act
        var world = new GameWorld();
        var hub = world.GetRoom("Operations Center");

        // Assert
        Assert.Equal(3, hub.Exits.Count);
        Assert.True(hub.Exits.ContainsKey("south")); // Back to Salvage Bay
        Assert.True(hub.Exits.ContainsKey("east"));  // To Arsenal (combat path)
        Assert.True(hub.Exits.ContainsKey("west"));  // To Research Archives (exploration path)
        Assert.True(hub.HasBeenCleared); // Should be safe zone
    }

    [Fact]
    public void V04_VaultCorridor_ShouldHaveBossChoice()
    {
        // Arrange & Act
        var world = new GameWorld();
        var vaultCorridor = world.GetRoom("Vault Corridor");

        // Assert
        Assert.True(vaultCorridor.Exits.ContainsKey("west")); // Arsenal Vault (Ruin-Warden)
        Assert.True(vaultCorridor.Exits.ContainsKey("east")); // Energy Core (Aberration)
        Assert.Equal("Arsenal Vault", vaultCorridor.Exits["west"]);
        Assert.Equal("Energy Core", vaultCorridor.Exits["east"]);
    }

    [Fact]
    public void V04_SecretRoom_ShouldUnlockAfterPuzzle()
    {
        // Arrange
        var world = new GameWorld();
        var vaultCorridor = world.GetRoom("Vault Corridor");

        // Act - Before unlock
        var exitsBeforeUnlock = vaultCorridor.Exits.Count;

        // Act - Unlock secret room
        world.UnlockSecretRoom();

        // Act - After unlock
        var exitsAfterUnlock = vaultCorridor.Exits.Count;

        // Assert
        Assert.Equal(2, exitsBeforeUnlock); // west, east (boss choices)
        Assert.Equal(3, exitsAfterUnlock);  // + south (secret room)
        Assert.True(vaultCorridor.Exits.ContainsKey("south"));
        Assert.Equal("Supply Cache", vaultCorridor.Exits["south"]);
    }

    #endregion

    #region Environmental Hazard Tests

    [Fact]
    public void V04_AmmunitionForge_ShouldHaveEnvironmentalHazard()
    {
        // Arrange & Act
        var world = new GameWorld();
        var ammunitionForge = world.GetRoom("Ammunition Forge");

        // Assert
        Assert.True(ammunitionForge.HasEnvironmentalHazard);
        Assert.True(ammunitionForge.IsHazardActive);
        Assert.Equal(6, ammunitionForge.HazardDamagePerTurn);
        Assert.NotEmpty(ammunitionForge.HazardDescription);
    }

    [Fact]
    public void V04_PuzzleSolution_ShouldDisableHazard()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "Test");
        var gameState = new GameState();
        gameState.World = world;
        gameState.Player = player;
        gameState.CurrentRoom = world.GetRoom("Ammunition Forge");

        // Act
        Assert.True(gameState.CurrentRoom.IsHazardActive); // Before
        gameState.SolvePuzzle();

        // Assert
        Assert.False(gameState.CurrentRoom.IsHazardActive); // After
    }

    #endregion

    #region NPC Interaction Tests

    [Fact]
    public void V04_ObservationDeck_ShouldHaveTalkableNPC()
    {
        // Arrange & Act
        var world = new GameWorld();
        var observationDeck = world.GetRoom("Observation Deck");

        // Assert
        Assert.True(observationDeck.HasTalkableNPC);
        Assert.False(observationDeck.HasTalkedToNPC); // Initially false
        Assert.Single(observationDeck.Enemies);
        Assert.Equal(EnemyType.ForlornScholar, observationDeck.Enemies[0].Type);
    }

    [Fact]
    public void V04_TalkableNPC_ShouldNotAutoTriggerCombat()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Mystic, "Test");
        var gameState = new GameState();
        gameState.World = world;
        gameState.Player = player;
        gameState.CurrentPhase = GamePhase.Exploration;

        // Navigate to Observation Deck
        gameState.CurrentRoom = world.GetRoom("Entrance");
        gameState.MoveToRoom("north"); // Corridor
        gameState.MoveToRoom("north"); // Salvage Bay
        gameState.MoveToRoom("north"); // Operations Center
        gameState.MoveToRoom("west");  // Research Archives
        gameState.MoveToRoom("west");  // Specimen Containment
        gameState.MoveToRoom("west");  // Observation Deck

        // Act
        var shouldTriggerCombat = gameState.ShouldTriggerCombat();

        // Assert
        Assert.False(shouldTriggerCombat); // Should NOT auto-trigger
        Assert.True(gameState.CurrentRoom.HasTalkableNPC);
        Assert.False(gameState.CurrentRoom.HasTalkedToNPC);
    }

    [Fact]
    public void V04_TalkableNPC_AfterTalking_ShouldTriggerCombat()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Mystic, "Test");
        var gameState = new GameState();
        gameState.World = world;
        gameState.Player = player;
        gameState.CurrentPhase = GamePhase.Exploration;
        gameState.CurrentRoom = world.GetRoom("Observation Deck");

        // Act - Mark as talked
        gameState.CurrentRoom.HasTalkedToNPC = true;

        // Assert
        var shouldTriggerCombat = gameState.ShouldTriggerCombat();
        Assert.True(shouldTriggerCombat); // Now should trigger
    }

    #endregion

    #region Puzzle System Tests

    [Theory]
    [InlineData("Ammunition Forge", true, 3)]
    [InlineData("Research Archives", true, 4)]
    [InlineData("Vault Corridor", true, 5)]
    public void V04_PuzzleRooms_ShouldHaveCorrectDifficulty(string roomName, bool hasPuzzle, int dc)
    {
        // Arrange & Act
        var world = new GameWorld();
        var room = world.GetRoom(roomName);

        // Assert
        Assert.Equal(hasPuzzle, room.HasPuzzle);
        Assert.Equal(dc, room.PuzzleSuccessThreshold);
        Assert.False(room.IsPuzzleSolved); // Initially unsolved
    }

    [Fact]
    public void V04_VaultCorridorPuzzle_ShouldUnlockSecretRoom()
    {
        // Arrange
        var world = new GameWorld();
        var player = CharacterFactory.CreateCharacter(CharacterClass.Warrior, "Test");
        var gameState = new GameState();
        gameState.World = world;
        gameState.Player = player;
        gameState.CurrentRoom = world.GetRoom("Vault Corridor");

        // Act - Before solving
        var exitsBeforePuzzle = gameState.CurrentRoom.Exits.Count;

        // Act - Solve puzzle
        gameState.SolvePuzzle();

        // Act - After solving
        var exitsAfterPuzzle = gameState.CurrentRoom.Exits.Count;

        // Assert
        Assert.Equal(2, exitsBeforePuzzle);
        Assert.Equal(3, exitsAfterPuzzle);
        Assert.True(gameState.CurrentRoom.Exits.ContainsKey("south"));
    }

    #endregion

    #region Enemy Distribution Tests

    [Fact]
    public void V04_EastWing_ShouldHaveMoreCombat()
    {
        // Arrange
        var world = new GameWorld();

        var eastWingRooms = new[]
        {
            "Arsenal",
            "Training Chamber",
            "Ammunition Forge"
        };

        // Act
        var totalEastEnemies = eastWingRooms
            .Select(name => world.GetRoom(name))
            .Sum(room => room.Enemies.Count);

        // Assert
        Assert.True(totalEastEnemies >= 6, "East Wing should have at least 6 enemies");
    }

    [Fact]
    public void V04_WestWing_ShouldHaveFewerCombatMorePuzzles()
    {
        // Arrange
        var world = new GameWorld();

        var westWingRooms = new[]
        {
            "Research Archives",
            "Specimen Containment",
            "Observation Deck"
        };

        // Act
        var totalWestEnemies = westWingRooms
            .Select(name => world.GetRoom(name))
            .Sum(room => room.Enemies.Count);

        var westPuzzleRooms = westWingRooms
            .Select(name => world.GetRoom(name))
            .Count(room => room.HasPuzzle);

        // Assert
        Assert.True(totalWestEnemies <= 5, "West Wing should have fewer enemies");
        Assert.True(westPuzzleRooms >= 1, "West Wing should have at least 1 puzzle");
    }

    [Fact]
    public void V04_AllNewEnemyTypes_ShouldAppearInWorld()
    {
        // Arrange
        var world = new GameWorld();
        var allEnemies = world.Rooms.Values
            .SelectMany(r => r.Enemies)
            .Select(e => e.Type)
            .Distinct()
            .ToList();

        // Assert
        Assert.Contains(EnemyType.ScrapHound, allEnemies);
        Assert.Contains(EnemyType.TestSubject, allEnemies);
        Assert.Contains(EnemyType.WarFrame, allEnemies);
        Assert.Contains(EnemyType.ForlornScholar, allEnemies);
        Assert.Contains(EnemyType.AethericAberration, allEnemies);
    }

    #endregion

    #region Legend Gain Validation

    [Fact]
    public void V04_AllNewEnemies_ShouldGrantLegend()
    {
        // Arrange
        var newEnemyTypes = new[]
        {
            EnemyType.ScrapHound,
            EnemyType.TestSubject,
            EnemyType.WarFrame,
            EnemyType.ForlornScholar,
            EnemyType.AethericAberration
        };

        // Act & Assert
        foreach (var type in newEnemyTypes)
        {
            var enemy = EnemyFactory.CreateEnemy(type);
            Assert.True(enemy.LegendValue > 0, $"{type} should grant Legend");
        }
    }

    [Fact]
    public void V04_BossEnemies_ShouldGrantSignificantLegend()
    {
        // Arrange
        var ruinWarden = EnemyFactory.CreateEnemy(EnemyType.RuinWarden);
        var aberration = EnemyFactory.CreateEnemy(EnemyType.AethericAberration);

        // Assert
        Assert.True(ruinWarden.IsBoss);
        Assert.True(aberration.IsBoss);
        Assert.True(ruinWarden.LegendValue >= 100, "Boss should grant at least 100 Legend");
        Assert.True(aberration.LegendValue >= 100, "Boss should grant at least 100 Legend");
    }

    #endregion

    #region Command Parser Tests

    [Theory]
    [InlineData("talk", CommandType.Talk)]
    [InlineData("speak", CommandType.Talk)]
    [InlineData("negotiate", CommandType.Talk)]
    [InlineData("convince", CommandType.Talk)]
    public void V04_TalkCommand_ShouldParseCorrectly(string input, CommandType expectedType)
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var command = parser.Parse(input);

        // Assert
        Assert.Equal(expectedType, command.Type);
    }

    #endregion
}
