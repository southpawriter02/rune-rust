using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.29.5: Tests for movement blocking by environmental hazards
/// </summary>
[TestClass]
public class MovementBlockingTests
{
    private PositioningService _positioningService = null!;
    private BattlefieldGrid _grid = null!;
    private List<EnvironmentalObject> _environmentalObjects = null!;

    [TestInitialize]
    public void Setup()
    {
        _positioningService = new PositioningService();
        _grid = new BattlefieldGrid(5); // 5 columns
        _environmentalObjects = new List<EnvironmentalObject>();
    }

    [TestMethod]
    public void MoveCombatant_IntoLavaRiver_BlocksMovement()
    {
        // Arrange
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Add lava river at target position
        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Chasm/Lava River]",
            GridPosition = targetPosition.ToString(),
            BlocksMovement = true,
            IsHazard = true
        };
        _environmentalObjects.Add(lavaRiver);

        // Act
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, _environmentalObjects);

        // Assert
        Assert.IsFalse(result.Success, "Movement should be blocked by lava river");
        Assert.IsTrue(result.Message.Contains("[Chasm/Lava River]"), "Message should mention the blocking hazard");
        Assert.AreEqual(new GridPosition(Zone.Player, Row.Front, 0), player.Position, "Position should not change");
        Assert.AreEqual(100, player.Stamina, "Stamina should not be consumed");
    }

    [TestMethod]
    public void MoveCombatant_IntoPassableTile_AllowsMovement()
    {
        // Arrange
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // No environmental objects blocking

        // Act
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success, "Movement should succeed");
        Assert.AreEqual(targetPosition, player.Position, "Position should update");
        Assert.IsTrue(player.Stamina < 100, "Stamina should be consumed");
    }

    [TestMethod]
    public void MoveCombatant_IntoBurningGround_AllowsMovement()
    {
        // Arrange: [Burning Ground] is passable, just deals damage
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Add burning ground (passable but damaging)
        var burningGround = new EnvironmentalObject
        {
            Name = "[Burning Ground]",
            GridPosition = targetPosition.ToString(),
            BlocksMovement = false, // Passable
            IsHazard = true,
            DamageFormula = "2d6 Fire"
        };
        _environmentalObjects.Add(burningGround);

        // Act
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, _environmentalObjects);

        // Assert
        Assert.IsTrue(result.Success, "Movement into [Burning Ground] should succeed");
        Assert.AreEqual(targetPosition, player.Position, "Position should update");
    }

    [TestMethod]
    public void MoveCombatant_IntoCollapsedStructure_BlocksMovement()
    {
        // Arrange
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Add collapsed structure (blocks but not lethal)
        var collapsedStructure = new EnvironmentalObject
        {
            Name = "[Collapsed Structure]",
            GridPosition = targetPosition.ToString(),
            BlocksMovement = true,
            IsHazard = false
        };
        _environmentalObjects.Add(collapsedStructure);

        // Act
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, _environmentalObjects);

        // Assert
        Assert.IsFalse(result.Success, "Movement should be blocked");
        Assert.IsTrue(result.Message.Contains("[Collapsed Structure]"), "Message should mention the obstacle");
        Assert.AreEqual(100, player.Stamina, "Stamina should not be consumed");
    }

    [TestMethod]
    public void MoveCombatant_InsufficientStamina_FailsButDoesNotMentionBlocking()
    {
        // Arrange: Player has no stamina
        var player = CreateTestPlayer("TestPlayer", Zone.Player, Row.Front, 0);
        player.Stamina = 0;
        var targetPosition = new GridPosition(Zone.Player, Row.Front, 1);

        // Add lava river at target (both obstacles present)
        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Chasm/Lava River]",
            GridPosition = targetPosition.ToString(),
            BlocksMovement = true
        };
        _environmentalObjects.Add(lavaRiver);

        // Act
        var result = _positioningService.MoveCombatant(player, targetPosition, _grid, _environmentalObjects);

        // Assert
        Assert.IsFalse(result.Success);
        // Blocking check happens BEFORE stamina check in the validation order
        Assert.IsTrue(result.Message.Contains("[Chasm/Lava River]") || result.Message.Contains("Stamina"));
    }

    [TestMethod]
    public void GetBlockingFeature_NoBlockingFeatures_ReturnsNull()
    {
        // Arrange
        var tile = _grid.GetTile(new GridPosition(Zone.Player, Row.Front, 0));

        // Act
        var feature = tile!.GetBlockingFeature();

        // Assert
        Assert.IsNull(feature, "Should return null when no blocking features");
    }

    [TestMethod]
    public void GetBlockingFeature_LavaRiverPresent_ReturnsFeatureName()
    {
        // Arrange
        var position = new GridPosition(Zone.Player, Row.Front, 0);
        var tile = _grid.GetTile(position);

        var lavaRiver = new EnvironmentalObject
        {
            Name = "[Chasm/Lava River]",
            GridPosition = position.ToString(),
            BlocksMovement = true
        };

        var envObjects = new List<EnvironmentalObject> { lavaRiver };

        // Act
        var feature = tile!.GetBlockingFeature(envObjects);

        // Assert
        Assert.AreEqual("[Chasm/Lava River]", feature);
    }

    // Helper method to create test player
    private PlayerCharacter CreateTestPlayer(string name, Zone zone, Row row, int column)
    {
        return new PlayerCharacter
        {
            Name = name,
            Position = new GridPosition(zone, row, column),
            Stamina = 100,
            MaxStamina = 100,
            CurrentHP = 50,
            MaxHP = 50,
            KineticEnergy = 0,
            MaxKineticEnergy = 100,
            Attributes = new RuneAndRust.Core.Attributes
            {
                Finesse = 3,
                Wits = 3
            }
        };
    }
}
