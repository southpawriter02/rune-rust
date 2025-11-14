using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.5: Unit tests for CoordinatedMovementService
/// Tests party-wide coordinated movement while maintaining formation cohesion
/// </summary>
[TestFixture]
public class CoordinatedMovementServiceTests
{
    private CoordinatedMovementService _service = null!;
    private PositioningService _positioning = null!;

    [SetUp]
    public void Setup()
    {
        _positioning = new PositioningService();
        _service = new CoordinatedMovementService(_positioning);
    }

    #region Helper Methods

    private BattlefieldGrid CreateTestGrid(int columns = 5)
    {
        return new BattlefieldGrid(columns);
    }

    private PlayerCharacter CreateTestPlayer(string name, Zone zone, Row row, int column, int stamina = 100)
    {
        var player = new PlayerCharacter
        {
            Name = name,
            HP = 50,
            MaxHP = 50,
            Stamina = stamina,
            MaxStamina = 100,
            Position = new GridPosition(zone, row, column),
            Attributes = new Attributes { Might = 3, Finesse = 3, Wits = 2, Will = 2, Sturdiness = 3 }
        };

        return player;
    }

    private void SetupPartyOnGrid(List<object> party, BattlefieldGrid grid)
    {
        foreach (var member in party)
        {
            var position = member switch
            {
                PlayerCharacter player => player.Position,
                Enemy enemy => enemy.Position,
                _ => null
            };

            if (position.HasValue)
            {
                var tile = grid.GetTile(position.Value);
                if (tile != null)
                {
                    tile.IsOccupied = true;
                    tile.OccupantId = member is PlayerCharacter ? "player" : ((Enemy)member).Id;
                }
            }
        }
    }

    #endregion

    #region Forward Movement Tests

    [Test]
    public void MoveFormation_Forward_MovesAllMembers()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 1, stamina: 20),
            CreateTestPlayer("Player2", Zone.Player, Row.Back, 2, stamina: 20),
            CreateTestPlayer("Player3", Zone.Player, Row.Back, 3, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(((PlayerCharacter)party[1]).Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(((PlayerCharacter)party[2]).Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    [Test]
    public void MoveFormation_Forward_CostsStamina()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 1, stamina: 20),
            CreateTestPlayer("Player2", Zone.Player, Row.Back, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.TotalStaminaCost, Is.EqualTo(20)); // 10 stamina per person for row change
        Assert.That(((PlayerCharacter)party[0]).Stamina, Is.EqualTo(10));
        Assert.That(((PlayerCharacter)party[1]).Stamina, Is.EqualTo(10));
    }

    [Test]
    public void MoveFormation_Forward_AlreadyInFront_NoChange()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 1, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act - Trying to move forward when already in front row
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert - Should remain in front row, no stamina cost
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    #endregion

    #region Backward Movement Tests

    [Test]
    public void MoveFormation_Backward_MovesAllMembers()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 1, stamina: 20),
            CreateTestPlayer("Player2", Zone.Player, Row.Front, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Backward, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Row, Is.EqualTo(Row.Back));
        Assert.That(((PlayerCharacter)party[1]).Position!.Value.Row, Is.EqualTo(Row.Back));
    }

    #endregion

    #region Left Movement Tests

    [Test]
    public void MoveFormation_Left_ShiftsColumns()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 2, stamina: 20),
            CreateTestPlayer("Player2", Zone.Player, Row.Front, 3, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Left, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Column, Is.EqualTo(1));
        Assert.That(((PlayerCharacter)party[1]).Position!.Value.Column, Is.EqualTo(2));
    }

    [Test]
    public void MoveFormation_Left_CostsStamina()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Left, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.TotalStaminaCost, Is.EqualTo(5)); // 5 stamina per column
        Assert.That(((PlayerCharacter)party[0]).Stamina, Is.EqualTo(15));
    }

    [Test]
    public void MoveFormation_Left_AtEdge_Blocked()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 0, stamina: 20) // Already at left edge
        };

        SetupPartyOnGrid(party, grid);

        // Act - Try to move left when already at column 0
        var result = _service.MoveFormation(party, Direction.Left, grid);

        // Assert - Should remain at column 0 (can't go negative)
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Column, Is.EqualTo(0));
    }

    #endregion

    #region Right Movement Tests

    [Test]
    public void MoveFormation_Right_ShiftsColumns()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 1, stamina: 20),
            CreateTestPlayer("Player2", Zone.Player, Row.Front, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Right, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(((PlayerCharacter)party[0]).Position!.Value.Column, Is.EqualTo(2));
        Assert.That(((PlayerCharacter)party[1]).Position!.Value.Column, Is.EqualTo(3));
    }

    #endregion

    #region Stamina Validation Tests

    [Test]
    public void MoveFormation_InsufficientStamina_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 1, stamina: 5), // Not enough for row change
            CreateTestPlayer("Player2", Zone.Player, Row.Back, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Stamina"));
    }

    [Test]
    public void MoveFormation_AllMembersHaveSufficientStamina_Success()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 1, stamina: 15),
            CreateTestPlayer("Player2", Zone.Player, Row.Back, 2, stamina: 15)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.True);
    }

    #endregion

    #region Blocked Movement Tests

    [Test]
    public void MoveFormation_PathBlocked_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 1, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Block the target position
        var blockedTile = grid.GetTile(new GridPosition(Zone.Player, Row.Front, 1))!;
        blockedTile.IsOccupied = true;
        blockedTile.OccupantId = "enemy";

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("occupied"));
    }

    [Test]
    public void MoveFormation_InvalidPosition_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 3); // Small grid
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 2, stamina: 20) // Column 2 (edge)
        };

        SetupPartyOnGrid(party, grid);

        // Act - Try to move right beyond grid boundary
        var result = _service.MoveFormation(party, Direction.Right, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("invalid"));
    }

    #endregion

    #region Helper Method Tests

    [Test]
    public void CanMoveInDirection_ValidMove_ReturnsTrue()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Act
        bool canMoveForward = _service.CanMoveInDirection(party, Direction.Forward, grid);
        bool canMoveLeft = _service.CanMoveInDirection(party, Direction.Left, grid);
        bool canMoveRight = _service.CanMoveInDirection(party, Direction.Right, grid);

        // Assert
        Assert.That(canMoveForward, Is.True);
        Assert.That(canMoveLeft, Is.True);
        Assert.That(canMoveRight, Is.True);
    }

    [Test]
    public void CanMoveInDirection_Blocked_ReturnsFalse()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Back, 2, stamina: 20)
        };

        SetupPartyOnGrid(party, grid);

        // Block forward position
        var blockedTile = grid.GetTile(new GridPosition(Zone.Player, Row.Front, 2))!;
        blockedTile.IsOccupied = true;
        blockedTile.OccupantId = "enemy";

        // Act
        bool canMoveForward = _service.CanMoveInDirection(party, Direction.Forward, grid);

        // Assert
        Assert.That(canMoveForward, Is.False);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void MoveFormation_EmptyParty_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>();

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("empty"));
    }

    [Test]
    public void MoveFormation_MemberWithoutPosition_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var player = CreateTestPlayer("Player1", Zone.Player, Row.Front, 1);
        player.Position = null; // Remove position

        var party = new List<object> { player };

        // Act
        var result = _service.MoveFormation(party, Direction.Forward, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("no position"));
    }

    #endregion
}
