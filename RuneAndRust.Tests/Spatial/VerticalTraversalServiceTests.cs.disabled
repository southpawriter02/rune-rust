using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core;
using RuneAndRust.Core.Spatial;
using RuneAndRust.Engine.Spatial;
using Serilog;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for VerticalTraversalService (v0.39.1)
/// Tests traversal mechanics, skill checks, and fall damage
/// </summary>
[TestClass]
public class VerticalTraversalServiceTests
{
    private VerticalTraversalService _service = null!;
    private ILogger _logger = null!;
    private MockDiceRoller _mockDiceRoller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        _mockDiceRoller = new MockDiceRoller();
        _service = new VerticalTraversalService(_logger, _mockDiceRoller);
    }

    #region Helper Methods & Classes

    private PlayerCharacter CreateTestCharacter(int might = 2)
    {
        return new PlayerCharacter
        {
            Name = "Test Character",
            Attributes = new Attributes { MIGHT = might }
        };
    }

    /// <summary>
    /// Mock dice roller for predictable test results
    /// </summary>
    private class MockDiceRoller : IDiceRoller
    {
        public int NextRoll { get; set; } = 10;
        public List<int> RollSequence { get; set; } = new List<int>();
        private int _rollIndex = 0;

        public int Roll(int count, int sides)
        {
            if (RollSequence.Count > 0 && _rollIndex < RollSequence.Count)
            {
                var roll = RollSequence[_rollIndex];
                _rollIndex++;
                return roll;
            }

            return NextRoll;
        }

        public void Reset()
        {
            _rollIndex = 0;
        }
    }

    #endregion

    #region GetConnectionBetween Tests

    [TestMethod]
    public void GetConnectionBetween_DirectConnection_ReturnsConnection()
    {
        // Arrange
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2")
        };

        // Act
        var result = _service.GetConnectionBetween("room1", "room2", connections);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("room1", result.FromRoomId);
        Assert.AreEqual("room2", result.ToRoomId);
    }

    [TestMethod]
    public void GetConnectionBetween_BidirectionalReverse_ReturnsConnection()
    {
        // Arrange
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2") // Bidirectional
        };

        // Act
        var result = _service.GetConnectionBetween("room2", "room1", connections);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetConnectionBetween_NoConnection_ReturnsNull()
    {
        // Arrange
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2")
        };

        // Act
        var result = _service.GetConnectionBetween("room1", "room999", connections);

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region CanTraverse Tests

    [TestMethod]
    public void CanTraverse_Stairs_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanTraverse_BlockedConnection_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateStairs("room1", "room2");
        connection.IsBlocked = true;

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanTraverse_CollapsedConnection_ReturnsFalse()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", "Blocked by debris");

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanTraverse_PoweredElevator_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateElevator("room1", "room2", 3, isPowered: true);

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void CanTraverse_UnpoweredElevator_ReturnsTrue()
    {
        // Arrange - unpowered elevator can still be climbed manually
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateElevator("room1", "room2", 3, isPowered: false);

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsTrue(result); // Can traverse by climbing
    }

    [TestMethod]
    public void CanTraverse_Shaft_ReturnsTrue()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateShaft("room1", "room2");

        // Act
        var result = _service.CanTraverse(character, connection);

        // Assert
        Assert.IsTrue(result);
    }

    #endregion

    #region AttemptTraversal Tests - Stairs

    [TestMethod]
    public void AttemptTraversal_Stairs_AlwaysSucceeds()
    {
        // Arrange
        var character = CreateTestCharacter(might: 0); // Low might shouldn't matter
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var result = _service.AttemptTraversal(character, connection, "down");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
        Assert.IsTrue(result.Message.Contains("stairs"));
    }

    [TestMethod]
    public void AttemptTraversal_BlockedStairs_Fails()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateStairs("room1", "room2");
        connection.IsBlocked = true;
        connection.BlockageDescription = "Locked gate";

        // Act
        var result = _service.AttemptTraversal(character, connection, "down");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("blocked"));
    }

    #endregion

    #region AttemptTraversal Tests - Elevator

    [TestMethod]
    public void AttemptTraversal_PoweredElevator_Succeeds()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateElevator("room1", "room2", 3, isPowered: true);

        // Act
        var result = _service.AttemptTraversal(character, connection, "down");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
        Assert.IsTrue(result.Message.Contains("elevator"));
    }

    [TestMethod]
    public void AttemptTraversal_UnpoweredElevator_Fails()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateElevator("room1", "room2", 3, isPowered: false);

        // Act
        var result = _service.AttemptTraversal(character, connection, "down");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("no power"));
    }

    #endregion

    #region AttemptTraversal Tests - Shaft with Skill Checks

    [TestMethod]
    public void AttemptTraversal_Shaft_HighRoll_Succeeds()
    {
        // Arrange
        var character = CreateTestCharacter(might: 2);
        var connection = VerticalConnection.CreateShaft("room1", "room2", dc: 12);
        _mockDiceRoller.NextRoll = 15; // 15 + 2 = 17 vs DC 12

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
        Assert.IsTrue(result.Message.Contains("climb"));
    }

    [TestMethod]
    public void AttemptTraversal_Shaft_LowRoll_FailsWithDamage()
    {
        // Arrange
        var character = CreateTestCharacter(might: 2);
        var connection = VerticalConnection.CreateShaft("room1", "room2", levelsSpanned: 2, dc: 12);
        _mockDiceRoller.NextRoll = 5; // 5 + 2 = 7 vs DC 12 (fails by 5)

        // Damage will be rolled, set it to a specific value
        _mockDiceRoller.RollSequence = new List<int> { 5, 6, 4 }; // d20 roll, then 2d6 fall damage
        _mockDiceRoller.Reset();

        // Act
        var result = _service.AttemptTraversal(character, connection, "down");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Damage > 0);
        Assert.IsTrue(result.Message.Contains("fall"));
    }

    [TestMethod]
    public void AttemptTraversal_Shaft_ExactlyMeetsDC_Succeeds()
    {
        // Arrange
        var character = CreateTestCharacter(might: 3);
        var connection = VerticalConnection.CreateShaft("room1", "room2", dc: 15);
        _mockDiceRoller.NextRoll = 12; // 12 + 3 = 15 vs DC 15 (exact match)

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
    }

    #endregion

    #region AttemptTraversal Tests - Ladder

    [TestMethod]
    public void AttemptTraversal_Ladder_Success()
    {
        // Arrange
        var character = CreateTestCharacter(might: 3);
        var connection = VerticalConnection.CreateLadder("room1", "room2", dc: 10);
        _mockDiceRoller.NextRoll = 10; // 10 + 3 = 13 vs DC 10

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
    }

    [TestMethod]
    public void AttemptTraversal_Ladder_Failure()
    {
        // Arrange
        var character = CreateTestCharacter(might: 1);
        var connection = VerticalConnection.CreateLadder("room1", "room2", dc: 12);
        _mockDiceRoller.RollSequence = new List<int> { 5, 3 }; // Fail check, then damage roll
        _mockDiceRoller.Reset();

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Damage > 0);
    }

    #endregion

    #region AttemptClearBlockage Tests

    [TestMethod]
    public void AttemptClearBlockage_NotBlocked_Fails()
    {
        // Arrange
        var character = CreateTestCharacter();
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var result = _service.AttemptClearBlockage(character, connection);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not blocked"));
    }

    [TestMethod]
    public void AttemptClearBlockage_Success_ClearsBlockage()
    {
        // Arrange
        var character = CreateTestCharacter(might: 4);
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", "Debris blocks passage", clearanceDC: 15);
        _mockDiceRoller.NextRoll = 15; // 15 + 4 = 19 vs DC 15

        // Act
        var result = _service.AttemptClearBlockage(character, connection);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsFalse(connection.IsBlocked);
        Assert.IsTrue(result.Message.Contains("cleared"));
    }

    [TestMethod]
    public void AttemptClearBlockage_Failure_TakesDamage()
    {
        // Arrange
        var character = CreateTestCharacter(might: 1);
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", "Heavy rubble", clearanceDC: 18);
        _mockDiceRoller.NextRoll = 10; // 10 + 1 = 11 vs DC 18 (fail by 7)

        // Act
        var result = _service.AttemptClearBlockage(character, connection);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(connection.IsBlocked); // Still blocked
        Assert.IsTrue(result.Damage > 0);
        Assert.IsTrue(result.Message.Contains("strain"));
    }

    #endregion

    #region GetReachableLayers Tests

    [TestMethod]
    public void GetReachableLayers_SingleRoom_ReturnsOnlyCurrentLayer()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var layers = _service.GetReachableLayers("room1", positions, connections);

        // Assert
        Assert.AreEqual(1, layers.Count);
        Assert.IsTrue(layers.Contains(VerticalLayer.GroundLevel));
    }

    [TestMethod]
    public void GetReachableLayers_TwoConnectedRooms_ReturnsBothLayers()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2")
        };

        // Act
        var layers = _service.GetReachableLayers("room1", positions, connections);

        // Assert
        Assert.AreEqual(2, layers.Count);
        Assert.IsTrue(layers.Contains(VerticalLayer.GroundLevel));
        Assert.IsTrue(layers.Contains(VerticalLayer.LowerTrunk));
    }

    [TestMethod]
    public void GetReachableLayers_BlockedConnection_OnlyStartLayer()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1)
        };
        var connection = VerticalConnection.CreateStairs("room1", "room2");
        connection.IsBlocked = true;
        var connections = new List<VerticalConnection> { connection };

        // Act
        var layers = _service.GetReachableLayers("room1", positions, connections);

        // Assert
        Assert.AreEqual(1, layers.Count);
        Assert.IsTrue(layers.Contains(VerticalLayer.GroundLevel));
    }

    [TestMethod]
    public void GetReachableLayers_MultiLevelChain_ReturnsAllConnected()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1),
            ["room3"] = new RoomPosition(0, 0, 2)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2"),
            VerticalConnection.CreateLadder("room2", "room3")
        };

        // Act
        var layers = _service.GetReachableLayers("room1", positions, connections);

        // Assert
        Assert.AreEqual(3, layers.Count);
        Assert.IsTrue(layers.Contains(VerticalLayer.GroundLevel));
        Assert.IsTrue(layers.Contains(VerticalLayer.LowerTrunk));
        Assert.IsTrue(layers.Contains(VerticalLayer.UpperTrunk));
    }

    [TestMethod]
    public void GetReachableLayers_InvalidRoomId_ReturnsEmpty()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0)
        };
        var connections = new List<VerticalConnection>();

        // Act
        var layers = _service.GetReachableLayers("room999", positions, connections);

        // Assert
        Assert.AreEqual(0, layers.Count);
    }

    [TestMethod]
    public void GetReachableLayers_ResultsSorted_AscendingOrder()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, -1),
            ["room3"] = new RoomPosition(0, 0, 1)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2"),
            VerticalConnection.CreateStairs("room1", "room3")
        };

        // Act
        var layers = _service.GetReachableLayers("room1", positions, connections);

        // Assert
        Assert.AreEqual(3, layers.Count);
        Assert.AreEqual(VerticalLayer.UpperRoots, layers[0]); // -1
        Assert.AreEqual(VerticalLayer.GroundLevel, layers[1]); // 0
        Assert.AreEqual(VerticalLayer.LowerTrunk, layers[2]); // 1
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void AttemptTraversal_CriticalSuccess_NoExtraBenefit()
    {
        // Arrange
        var character = CreateTestCharacter(might: 5);
        var connection = VerticalConnection.CreateShaft("room1", "room2", dc: 12);
        _mockDiceRoller.NextRoll = 20; // Natural 20

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(0, result.Damage);
        // No special crit effect, just success
    }

    [TestMethod]
    public void AttemptTraversal_NegativeMight_StillRolls()
    {
        // Arrange
        var character = CreateTestCharacter(might: -2); // Penalty
        var connection = VerticalConnection.CreateShaft("room1", "room2", dc: 10);
        _mockDiceRoller.NextRoll = 15; // 15 + (-2) = 13 vs DC 10 (still succeeds)

        // Act
        var result = _service.AttemptTraversal(character, connection, "up");

        // Assert
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void GetReachableLayers_BidirectionalConnections_WorksInBothDirections()
    {
        // Arrange
        var positions = new Dictionary<string, RoomPosition>
        {
            ["room1"] = new RoomPosition(0, 0, 0),
            ["room2"] = new RoomPosition(0, 0, 1)
        };
        var connections = new List<VerticalConnection>
        {
            VerticalConnection.CreateStairs("room1", "room2") // Bidirectional
        };

        // Act
        var layersFrom1 = _service.GetReachableLayers("room1", positions, connections);
        var layersFrom2 = _service.GetReachableLayers("room2", positions, connections);

        // Assert
        Assert.AreEqual(2, layersFrom1.Count);
        Assert.AreEqual(2, layersFrom2.Count);
    }

    #endregion
}
