using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneAndRust.Core.Spatial;

namespace RuneAndRust.Tests.Spatial;

/// <summary>
/// Unit tests for VerticalConnection model and factory methods (v0.39.1)
/// </summary>
[TestClass]
public class VerticalConnectionTests
{
    #region Factory Method Tests

    [TestMethod]
    public void CreateStairs_BasicConnection_SetsCorrectProperties()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Assert
        Assert.AreEqual("room1", connection.FromRoomId);
        Assert.AreEqual("room2", connection.ToRoomId);
        Assert.AreEqual(VerticalConnectionType.Stairs, connection.Type);
        Assert.AreEqual(0, connection.TraversalDC);
        Assert.AreEqual(1, connection.LevelsSpanned);
        Assert.IsTrue(connection.IsBidirectional);
        Assert.IsFalse(connection.IsBlocked);
        Assert.IsTrue(connection.ConnectionId.Contains("stairs"));
    }

    [TestMethod]
    public void CreateStairs_CustomLevels_UsesProvidedValue()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateStairs("room1", "room2", levelsSpanned: 3);

        // Assert
        Assert.AreEqual(3, connection.LevelsSpanned);
    }

    [TestMethod]
    public void CreateShaft_DefaultValues_SetsCorrectProperties()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateShaft("room1", "room2");

        // Assert
        Assert.AreEqual(VerticalConnectionType.Shaft, connection.Type);
        Assert.AreEqual(12, connection.TraversalDC);
        Assert.AreEqual(2, connection.LevelsSpanned);
        Assert.IsTrue(connection.IsBidirectional);
        Assert.IsTrue(connection.ConnectionId.Contains("shaft"));
    }

    [TestMethod]
    public void CreateShaft_CustomDC_UsesProvidedValue()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateShaft("room1", "room2", levelsSpanned: 3, dc: 15);

        // Assert
        Assert.AreEqual(15, connection.TraversalDC);
        Assert.AreEqual(3, connection.LevelsSpanned);
    }

    [TestMethod]
    public void CreateElevator_Powered_SetsCorrectProperties()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 4, isPowered: true);

        // Assert
        Assert.AreEqual(VerticalConnectionType.Elevator, connection.Type);
        Assert.AreEqual(0, connection.TraversalDC);
        Assert.AreEqual(4, connection.LevelsSpanned);
        Assert.IsTrue(connection.IsPowered.HasValue);
        Assert.IsTrue(connection.IsPowered.Value);
        Assert.IsTrue(connection.Description.Contains("functional"));
    }

    [TestMethod]
    public void CreateElevator_Unpowered_SetsHigherDC()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 4, isPowered: false);

        // Assert
        Assert.AreEqual(15, connection.TraversalDC);
        Assert.IsFalse(connection.IsPowered.Value);
        Assert.IsTrue(connection.Description.Contains("no power"));
    }

    [TestMethod]
    public void CreateLadder_DefaultValues_SetsCorrectProperties()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateLadder("room1", "room2");

        // Assert
        Assert.AreEqual(VerticalConnectionType.Ladder, connection.Type);
        Assert.AreEqual(10, connection.TraversalDC);
        Assert.AreEqual(1, connection.LevelsSpanned);
        Assert.IsTrue(connection.IsBidirectional);
    }

    [TestMethod]
    public void CreateCollapsed_WithBlockage_SetsCorrectProperties()
    {
        // Arrange
        var blockageDesc = "Rubble blocks the passage";

        // Act
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", blockageDesc, clearanceDC: 18);

        // Assert
        Assert.AreEqual(VerticalConnectionType.Collapsed, connection.Type);
        Assert.IsTrue(connection.IsBlocked);
        Assert.AreEqual(blockageDesc, connection.BlockageDescription);
        Assert.AreEqual(18, connection.ClearanceDC);
        Assert.AreEqual(10, connection.ClearanceTimeMinutes);
    }

    #endregion

    #region CanTraverse Tests

    [TestMethod]
    public void CanTraverse_NormalStairs_ReturnsTrue()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert
        Assert.IsTrue(canTraverse);
    }

    [TestMethod]
    public void CanTraverse_BlockedConnection_ReturnsFalse()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");
        connection.IsBlocked = true;

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert
        Assert.IsFalse(canTraverse);
    }

    [TestMethod]
    public void CanTraverse_CollapsedConnection_ReturnsFalse()
    {
        // Arrange
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", "Debris blocks passage");

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert
        Assert.IsFalse(canTraverse);
    }

    [TestMethod]
    public void CanTraverse_PoweredElevator_ReturnsTrue()
    {
        // Arrange
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 3, isPowered: true);

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert
        Assert.IsTrue(canTraverse);
    }

    [TestMethod]
    public void CanTraverse_UnpoweredElevator_ReturnsFalse()
    {
        // Arrange
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 3, isPowered: false);

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert
        Assert.IsFalse(canTraverse);
    }

    #endregion

    #region GetTraversalRequirementsDescription Tests

    [TestMethod]
    public void GetTraversalRequirements_Stairs_ReturnsFreeTraversal()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("Free traversal"));
        Assert.IsTrue(description.Contains("No skill check"));
    }

    [TestMethod]
    public void GetTraversalRequirements_Shaft_IncludesDC()
    {
        // Arrange
        var connection = VerticalConnection.CreateShaft("room1", "room2", dc: 15);

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("Athletics"));
        Assert.IsTrue(description.Contains("15"));
        Assert.IsTrue(description.Contains("fall damage"));
    }

    [TestMethod]
    public void GetTraversalRequirements_Ladder_IncludesDC()
    {
        // Arrange
        var connection = VerticalConnection.CreateLadder("room1", "room2", dc: 12);

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("Athletics"));
        Assert.IsTrue(description.Contains("12"));
    }

    [TestMethod]
    public void GetTraversalRequirements_PoweredElevator_NoCheckRequired()
    {
        // Arrange
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 3, isPowered: true);

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("powered"));
        Assert.IsTrue(description.Contains("No check"));
    }

    [TestMethod]
    public void GetTraversalRequirements_UnpoweredElevator_RequiresRepair()
    {
        // Arrange
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 3, isPowered: false);

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("unpowered"));
        Assert.IsTrue(description.Contains("Repair"));
    }

    [TestMethod]
    public void GetTraversalRequirements_Collapsed_ShowsClearanceInfo()
    {
        // Arrange
        var connection = VerticalConnection.CreateCollapsed("room1", "room2", "Rubble blocks passage", clearanceDC: 18);

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("Impassable"));
        Assert.IsTrue(description.Contains("18"));
        Assert.IsTrue(description.Contains("10 minutes"));
    }

    [TestMethod]
    public void GetTraversalRequirements_Blocked_ShowsBlockageDescription()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");
        connection.IsBlocked = true;
        connection.BlockageDescription = "A locked gate bars the way";

        // Act
        var description = connection.GetTraversalRequirementsDescription();

        // Assert
        Assert.IsTrue(description.Contains("Blocked"));
        Assert.IsTrue(description.Contains("locked gate"));
    }

    #endregion

    #region Success/Failure Description Tests

    [TestMethod]
    public void GetSuccessDescription_Stairs_ReturnsFlavorText()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var description = connection.GetSuccessDescription("down");

        // Assert
        Assert.IsTrue(description.Contains("stairs"));
        Assert.IsTrue(description.Contains("down"));
    }

    [TestMethod]
    public void GetSuccessDescription_Shaft_ReturnsClimbingText()
    {
        // Arrange
        var connection = VerticalConnection.CreateShaft("room1", "room2");

        // Act
        var description = connection.GetSuccessDescription("up");

        // Assert
        Assert.IsTrue(description.Contains("climb"));
        Assert.IsTrue(description.Contains("up"));
    }

    [TestMethod]
    public void GetSuccessDescription_Elevator_ReturnsElevatorText()
    {
        // Arrange
        var connection = VerticalConnection.CreateElevator("room1", "room2", levelsSpanned: 3, isPowered: true);

        // Act
        var description = connection.GetSuccessDescription("down");

        // Assert
        Assert.IsTrue(description.Contains("elevator"));
        Assert.IsTrue(description.Contains("down"));
    }

    [TestMethod]
    public void GetFailureDescription_Shaft_IncludesDamage()
    {
        // Arrange
        var connection = VerticalConnection.CreateShaft("room1", "room2");

        // Act
        var description = connection.GetFailureDescription(damage: 15);

        // Assert
        Assert.IsTrue(description.Contains("fall"));
        Assert.IsTrue(description.Contains("15"));
        Assert.IsTrue(description.Contains("Physical damage"));
    }

    [TestMethod]
    public void GetFailureDescription_Ladder_IncludesDamage()
    {
        // Arrange
        var connection = VerticalConnection.CreateLadder("room1", "room2");

        // Act
        var description = connection.GetFailureDescription(damage: 8);

        // Assert
        Assert.IsTrue(description.Contains("8"));
        Assert.IsTrue(description.Contains("Physical damage"));
    }

    #endregion

    #region GetTraversalDirection Tests

    [TestMethod]
    public void GetTraversalDirection_FromSource_ReturnsDown()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var direction = connection.GetTraversalDirection("room1");

        // Assert
        Assert.AreEqual("down", direction);
    }

    [TestMethod]
    public void GetTraversalDirection_FromDestination_ReturnsUp()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Act
        var direction = connection.GetTraversalDirection("room2");

        // Assert
        Assert.AreEqual("up", direction);
    }

    #endregion

    #region Property Tests

    [TestMethod]
    public void IsBidirectional_AllFactoryMethods_DefaultsToTrue()
    {
        // Arrange & Act
        var stairs = VerticalConnection.CreateStairs("room1", "room2");
        var shaft = VerticalConnection.CreateShaft("room1", "room2");
        var elevator = VerticalConnection.CreateElevator("room1", "room2", 3);
        var ladder = VerticalConnection.CreateLadder("room1", "room2");
        var collapsed = VerticalConnection.CreateCollapsed("room1", "room2", "Blocked");

        // Assert
        Assert.IsTrue(stairs.IsBidirectional);
        Assert.IsTrue(shaft.IsBidirectional);
        Assert.IsTrue(elevator.IsBidirectional);
        Assert.IsTrue(ladder.IsBidirectional);
        Assert.IsTrue(collapsed.IsBidirectional);
    }

    [TestMethod]
    public void Hazards_NewConnection_EmptyList()
    {
        // Arrange & Act
        var connection = VerticalConnection.CreateStairs("room1", "room2");

        // Assert
        Assert.IsNotNull(connection.Hazards);
        Assert.AreEqual(0, connection.Hazards.Count);
    }

    [TestMethod]
    public void Hazards_AddingHazards_StoresCorrectly()
    {
        // Arrange
        var connection = VerticalConnection.CreateShaft("room1", "room2");

        // Act
        connection.Hazards.Add("Steam vents");
        connection.Hazards.Add("Electrical discharge");

        // Assert
        Assert.AreEqual(2, connection.Hazards.Count);
        Assert.IsTrue(connection.Hazards.Contains("Steam vents"));
        Assert.IsTrue(connection.Hazards.Contains("Electrical discharge"));
    }

    #endregion

    #region ToString Tests

    [TestMethod]
    public void ToString_ValidConnection_ReturnsFormattedString()
    {
        // Arrange
        var connection = VerticalConnection.CreateStairs("room_d1_n1", "room_d1_n2", levelsSpanned: 2);

        // Act
        var str = connection.ToString();

        // Assert
        Assert.IsTrue(str.Contains("Stairs"));
        Assert.IsTrue(str.Contains("room_d1_n1"));
        Assert.IsTrue(str.Contains("room_d1_n2"));
        Assert.IsTrue(str.Contains("2 levels"));
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void CanTraverse_PoweredNullElevator_ReturnsTrue()
    {
        // Arrange
        var connection = VerticalConnection.CreateShaft("room1", "room2");
        connection.Type = VerticalConnectionType.Elevator;
        connection.IsPowered = null; // Not specified

        // Act
        var canTraverse = connection.CanTraverse();

        // Assert - null power means it's not explicitly unpowered, so treated as traversable
        Assert.IsTrue(canTraverse);
    }

    [TestMethod]
    public void ConnectionId_UniqueBetweenTypes()
    {
        // Arrange & Act
        var stairs = VerticalConnection.CreateStairs("room1", "room2");
        var shaft = VerticalConnection.CreateShaft("room1", "room2");
        var elevator = VerticalConnection.CreateElevator("room1", "room2", 3);

        // Assert
        Assert.AreNotEqual(stairs.ConnectionId, shaft.ConnectionId);
        Assert.AreNotEqual(stairs.ConnectionId, elevator.ConnectionId);
        Assert.AreNotEqual(shaft.ConnectionId, elevator.ConnectionId);
    }

    [TestMethod]
    public void Description_AllFactoryMethods_NonEmpty()
    {
        // Arrange & Act
        var stairs = VerticalConnection.CreateStairs("room1", "room2");
        var shaft = VerticalConnection.CreateShaft("room1", "room2");
        var elevator = VerticalConnection.CreateElevator("room1", "room2", 3);
        var ladder = VerticalConnection.CreateLadder("room1", "room2");
        var collapsed = VerticalConnection.CreateCollapsed("room1", "room2", "Blocked");

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(stairs.Description));
        Assert.IsFalse(string.IsNullOrEmpty(shaft.Description));
        Assert.IsFalse(string.IsNullOrEmpty(elevator.Description));
        Assert.IsFalse(string.IsNullOrEmpty(ladder.Description));
        Assert.IsFalse(string.IsNullOrEmpty(collapsed.Description));
    }

    #endregion
}
