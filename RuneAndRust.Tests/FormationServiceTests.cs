using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.5: Unit tests for FormationService
/// Tests formation application, position calculation, bonuses, and position swapping
/// </summary>
[TestFixture]
public class FormationServiceTests
{
    private FormationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new FormationService();
    }

    #region Helper Methods

    private BattlefieldGrid CreateTestGrid(int columns = 5)
    {
        return new BattlefieldGrid(columns);
    }

    private PlayerCharacter CreateTestPlayer(string name, int stamina = 100, GridPosition? position = null)
    {
        var player = new PlayerCharacter
        {
            Name = name,
            HP = 50,
            MaxHP = 50,
            Stamina = stamina,
            MaxStamina = 100,
            Position = position,
            Attributes = new Attributes { Might = 3, Finesse = 3, Wits = 2, Will = 2, Sturdiness = 3 }
        };
        return player;
    }

    private Enemy CreateTestEnemy(string id, int stamina = 100, GridPosition? position = null)
    {
        var enemy = new Enemy
        {
            Name = $"Enemy-{id}",
            Id = id,
            HP = 30,
            MaxHP = 30,
            Position = position,
            Attributes = new Attributes { Might = 3, Finesse = 2, Sturdiness = 2 }
        };
        return enemy;
    }

    #endregion

    #region Line Formation Tests

    [Test]
    public void ApplyFormation_LineFormation_PositionsCorrectly()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Tank1"),
            CreateTestPlayer("Tank2"),
            CreateTestPlayer("Tank3"),
            CreateTestPlayer("Support1")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Line, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Formation, Is.EqualTo(FormationType.Line));

        // Verify positions: 3 front row, 1 back row
        var player1 = (PlayerCharacter)party[0];
        var player2 = (PlayerCharacter)party[1];
        var player3 = (PlayerCharacter)party[2];
        var player4 = (PlayerCharacter)party[3];

        Assert.That(player1.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player2.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player3.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player4.Position!.Value.Row, Is.EqualTo(Row.Back));
    }

    [Test]
    public void ApplyFormation_LineFormation_AppliesDefenseBonuses()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Tank1"),
            CreateTestPlayer("Tank2"),
            CreateTestPlayer("Tank3"),
            CreateTestPlayer("Support1")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Line, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Bonuses.Count, Is.EqualTo(3)); // 3 front-row members get +1 Defense
        Assert.That(result.Bonuses.All(b => b.Type == BonusType.Defense), Is.True);
        Assert.That(result.Bonuses.All(b => b.Amount == 1), Is.True);
    }

    [Test]
    public void ApplyFormation_LineFormation_SingleMember_PlacesInFront()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object> { CreateTestPlayer("Solo") };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Line, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        var player = (PlayerCharacter)party[0];
        Assert.That(player.Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    #endregion

    #region Wedge Formation Tests

    [Test]
    public void ApplyFormation_WedgeFormation_PositionsCorrectly()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Striker1"),
            CreateTestPlayer("Striker2"),
            CreateTestPlayer("Striker3"),
            CreateTestPlayer("Support1")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Wedge, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Formation, Is.EqualTo(FormationType.Wedge));

        // Verify heavy front row
        var player1 = (PlayerCharacter)party[0];
        var player2 = (PlayerCharacter)party[1];
        var player3 = (PlayerCharacter)party[2];

        Assert.That(player1.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player2.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player3.Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    [Test]
    public void ApplyFormation_WedgeFormation_AppliesAccuracyBonuses()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Striker1"),
            CreateTestPlayer("Striker2"),
            CreateTestPlayer("Support1")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Wedge, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Bonuses.Count, Is.GreaterThanOrEqualTo(2)); // Front-row members get +1 Accuracy
        Assert.That(result.Bonuses.Any(b => b.Type == BonusType.Accuracy), Is.True);
    }

    #endregion

    #region Scattered Formation Tests

    [Test]
    public void ApplyFormation_ScatteredFormation_PositionsCorrectly()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Scout1"),
            CreateTestPlayer("Scout2"),
            CreateTestPlayer("Scout3"),
            CreateTestPlayer("Scout4")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Scattered, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Formation, Is.EqualTo(FormationType.Scattered));

        // Verify alternating rows
        var player1 = (PlayerCharacter)party[0];
        var player2 = (PlayerCharacter)party[1];

        Assert.That(player1.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(player2.Position!.Value.Row, Is.EqualTo(Row.Back));
    }

    [Test]
    public void ApplyFormation_ScatteredFormation_AppliesAntiFlankingBonuses()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Scout1"),
            CreateTestPlayer("Scout2"),
            CreateTestPlayer("Scout3")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Scattered, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Bonuses.Count, Is.EqualTo(3)); // All members get anti-flanking
        Assert.That(result.Bonuses.All(b => b.Type == BonusType.AntiFlanking), Is.True);
    }

    #endregion

    #region Protect Formation Tests

    [Test]
    public void ApplyFormation_ProtectFormation_PositionsCorrectly()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Guard1"),
            CreateTestPlayer("Guard2"),
            CreateTestPlayer("Guard3"),
            CreateTestPlayer("VIP")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Protect, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Formation, Is.EqualTo(FormationType.Protect));

        // Verify 3 front guards and 1 protected back-row
        var frontRowCount = party.Cast<PlayerCharacter>()
            .Count(p => p.Position!.Value.Row == Row.Front);
        var backRowCount = party.Cast<PlayerCharacter>()
            .Count(p => p.Position!.Value.Row == Row.Back);

        Assert.That(frontRowCount, Is.EqualTo(3));
        Assert.That(backRowCount, Is.EqualTo(1));
    }

    [Test]
    public void ApplyFormation_ProtectFormation_AppliesProtectedBonus()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Guard1"),
            CreateTestPlayer("Guard2"),
            CreateTestPlayer("Guard3"),
            CreateTestPlayer("VIP")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Protect, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Bonuses.Count, Is.EqualTo(1)); // Only protected member gets +2 Defense
        Assert.That(result.Bonuses[0].Type, Is.EqualTo(BonusType.Defense));
        Assert.That(result.Bonuses[0].Amount, Is.EqualTo(2));
    }

    [Test]
    public void ApplyFormation_ProtectFormation_TwoMembers_PositionsCorrectly()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Guard1"),
            CreateTestPlayer("VIP")
        };

        // Act
        var result = _service.ApplyFormation(party, FormationType.Protect, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        var guard = (PlayerCharacter)party[0];
        var vip = (PlayerCharacter)party[1];

        Assert.That(guard.Position!.Value.Row, Is.EqualTo(Row.Front));
        Assert.That(vip.Position!.Value.Row, Is.EqualTo(Row.Back));
    }

    #endregion

    #region Position Swapping Tests

    [Test]
    public void SwapPositions_ValidSwap_Success()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 1);
        actor2.Position = new GridPosition(Zone.Player, Row.Front, column: 2);

        // Mark tiles as occupied
        var tile1 = grid.GetTile(actor1.Position.Value)!;
        var tile2 = grid.GetTile(actor2.Position.Value)!;
        tile1.IsOccupied = true;
        tile1.OccupantId = "player1";
        tile2.IsOccupied = true;
        tile2.OccupantId = "player2";

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(actor1.Position!.Value.Column, Is.EqualTo(2));
        Assert.That(actor2.Position!.Value.Column, Is.EqualTo(1));
        Assert.That(result.StaminaCost, Is.EqualTo(10)); // 5 per person
    }

    [Test]
    public void SwapPositions_InsufficientStamina_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 3);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 1);
        actor2.Position = new GridPosition(Zone.Player, Row.Front, column: 2);

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("insufficient Stamina"));
    }

    [Test]
    public void SwapPositions_DifferentZones_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestEnemy("enemy1", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 1);
        actor2.Position = new GridPosition(Zone.Enemy, Row.Front, column: 1);

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("enemies"));
    }

    [Test]
    public void SwapPositions_TooFarApart_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 0);
        actor2.Position = new GridPosition(Zone.Player, Row.Front, column: 3); // 3 tiles apart

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Too far"));
    }

    [Test]
    public void SwapPositions_AdjacentTiles_Success()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 1);
        actor2.Position = new GridPosition(Zone.Player, Row.Front, column: 2);

        var tile1 = grid.GetTile(actor1.Position.Value)!;
        var tile2 = grid.GetTile(actor2.Position.Value)!;
        tile1.IsOccupied = true;
        tile2.IsOccupied = true;

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(actor1.Stamina, Is.EqualTo(5)); // 10 - 5 = 5
        Assert.That(actor2.Stamina, Is.EqualTo(5)); // 10 - 5 = 5
    }

    [Test]
    public void SwapPositions_DifferentRows_Success()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        actor1.Position = new GridPosition(Zone.Player, Row.Front, column: 1);
        actor2.Position = new GridPosition(Zone.Player, Row.Back, column: 1);

        var tile1 = grid.GetTile(actor1.Position.Value)!;
        var tile2 = grid.GetTile(actor2.Position.Value)!;
        tile1.IsOccupied = true;
        tile2.IsOccupied = true;

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(actor1.Position!.Value.Row, Is.EqualTo(Row.Back));
        Assert.That(actor2.Position!.Value.Row, Is.EqualTo(Row.Front));
    }

    #endregion

    #region Formation Detection Tests

    [Test]
    public void DetectFormation_LinePattern_ReturnsLine()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Tank1", position: new GridPosition(Zone.Player, Row.Front, 1)),
            CreateTestPlayer("Tank2", position: new GridPosition(Zone.Player, Row.Front, 2)),
            CreateTestPlayer("Tank3", position: new GridPosition(Zone.Player, Row.Front, 3)),
            CreateTestPlayer("Support1", position: new GridPosition(Zone.Player, Row.Back, 2))
        };

        // Act
        var formation = _service.DetectFormation(party, grid);

        // Assert
        Assert.That(formation, Is.EqualTo(FormationType.Line));
    }

    [Test]
    public void DetectFormation_ProtectPattern_ReturnsProtect()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        int centerColumn = grid.Columns / 2;

        var party = new List<object>
        {
            CreateTestPlayer("Guard1", position: new GridPosition(Zone.Player, Row.Front, 1)),
            CreateTestPlayer("Guard2", position: new GridPosition(Zone.Player, Row.Front, 2)),
            CreateTestPlayer("Guard3", position: new GridPosition(Zone.Player, Row.Front, 3)),
            CreateTestPlayer("VIP", position: new GridPosition(Zone.Player, Row.Back, centerColumn))
        };

        // Act
        var formation = _service.DetectFormation(party, grid);

        // Assert
        Assert.That(formation, Is.EqualTo(FormationType.Protect));
    }

    [Test]
    public void DetectFormation_ScatteredPattern_ReturnsScattered()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Scout1", position: new GridPosition(Zone.Player, Row.Front, 0)),
            CreateTestPlayer("Scout2", position: new GridPosition(Zone.Player, Row.Back, 2)),
            CreateTestPlayer("Scout3", position: new GridPosition(Zone.Player, Row.Front, 4))
        };

        // Act
        var formation = _service.DetectFormation(party, grid);

        // Assert
        Assert.That(formation, Is.EqualTo(FormationType.Scattered));
    }

    [Test]
    public void DetectFormation_EmptyParty_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>();

        // Act
        var formation = _service.DetectFormation(party, grid);

        // Assert
        Assert.That(formation, Is.EqualTo(FormationType.None));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void ApplyFormation_OccupiedPosition_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Player1"),
            CreateTestPlayer("Player2")
        };

        // Occupy a position that formation would use
        var blockerPos = new GridPosition(Zone.Player, Row.Front, 2);
        var tile = grid.GetTile(blockerPos)!;
        tile.IsOccupied = true;
        tile.OccupantId = "blocker";

        // Act
        var result = _service.ApplyFormation(party, FormationType.Line, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("occupied"));
    }

    [Test]
    public void ApplyFormation_EmptyParty_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>();

        // Act
        var result = _service.ApplyFormation(party, FormationType.Line, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("empty"));
    }

    [Test]
    public void SwapPositions_NoPosition_Fails()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var actor1 = CreateTestPlayer("Player1", stamina: 10);
        var actor2 = CreateTestPlayer("Player2", stamina: 10);

        // Don't set positions

        // Act
        var result = _service.SwapPositions(actor1, actor2, grid);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("no position"));
    }

    #endregion
}
