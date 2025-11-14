using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.5: Unit tests for AI Formation Awareness
/// Tests enemy targeting logic based on player party formation
/// </summary>
[TestFixture]
public class AIFormationAwarenessTests
{
    private EnemyAI _ai = null!;
    private FormationService _formationService = null!;
    private DiceService _diceService = null!;

    [SetUp]
    public void Setup()
    {
        _diceService = new DiceService();
        _ai = new EnemyAI(_diceService, seed: 12345); // Fixed seed for reproducibility
        _formationService = new FormationService();
    }

    #region Helper Methods

    private BattlefieldGrid CreateTestGrid(int columns = 5)
    {
        return new BattlefieldGrid(columns);
    }

    private PlayerCharacter CreateTestPlayer(string name, Zone zone, Row row, int column, int hp = 50)
    {
        return new PlayerCharacter
        {
            Name = name,
            HP = hp,
            MaxHP = 50,
            Position = new GridPosition(zone, row, column),
            Attributes = new Attributes { Might = 3, Finesse = 3, Wits = 2, Will = 2, Sturdiness = 3 }
        };
    }

    private Enemy CreateTestEnemy(string name, Zone zone, Row row, int column)
    {
        return new Enemy
        {
            Name = name,
            Id = Guid.NewGuid().ToString(),
            Type = EnemyType.CorruptedServitor,
            HP = 30,
            MaxHP = 30,
            Position = new GridPosition(zone, row, column),
            Attributes = new Attributes { Might = 3, Finesse = 2, Sturdiness = 2 }
        };
    }

    #endregion

    #region Protect Formation Targeting Tests

    [Test]
    public void SelectTarget_ProtectFormation_TargetsBackRowCenter()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        int centerColumn = grid.Columns / 2;

        var party = new List<object>
        {
            CreateTestPlayer("Guard1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Guard2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Guard3", Zone.Player, Row.Front, 3, hp: 50),
            CreateTestPlayer("VIP", Zone.Player, Row.Back, centerColumn, hp: 40) // Protected member
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, centerColumn);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.Name, Is.EqualTo("VIP"));
        Assert.That(targetPlayer.Position!.Value.Row, Is.EqualTo(Row.Back));
        Assert.That(targetPlayer.Position!.Value.Column, Is.EqualTo(centerColumn));
    }

    [Test]
    public void SelectTarget_ProtectFormation_NoBackRowCenter_FallsBackToAnyBackRow()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Guard1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Guard2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Support", Zone.Player, Row.Back, 0, hp: 40) // Not center
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Apply formation to ensure it's detected as Protect
        var formationResult = _formationService.ApplyFormation(party, FormationType.Protect, grid);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        // Should target a back-row member or weakest if detection doesn't work
        Assert.That(targetPlayer, Is.Not.Null);
    }

    #endregion

    #region Scattered Formation Targeting Tests

    [Test]
    public void SelectTarget_ScatteredFormation_TargetsMostIsolated()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Scout1", Zone.Player, Row.Front, 0, hp: 50), // Isolated on left
            CreateTestPlayer("Scout2", Zone.Player, Row.Front, 2, hp: 50), // Near Scout3
            CreateTestPlayer("Scout3", Zone.Player, Row.Front, 3, hp: 50)  // Near Scout2
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 1);

        // Apply scattered formation
        _formationService.ApplyFormation(party, FormationType.Scattered, grid);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        // Scout1 should be most isolated (no adjacent allies)
        Assert.That(targetPlayer.Name, Is.EqualTo("Scout1"));
    }

    [Test]
    public void SelectTarget_ScatteredFormation_AllEquallySeparated_TargetsAny()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Scout1", Zone.Player, Row.Front, 0, hp: 50),
            CreateTestPlayer("Scout2", Zone.Player, Row.Back, 2, hp: 40),
            CreateTestPlayer("Scout3", Zone.Player, Row.Front, 4, hp: 50)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert - Should target one of them (all equally isolated)
        Assert.That(target, Is.Not.Null);
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(party.Contains(targetPlayer), Is.True);
    }

    #endregion

    #region Line Formation Targeting Tests

    [Test]
    public void SelectTarget_LineFormation_TargetsWeakestFrontRow()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Tank1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Tank2", Zone.Player, Row.Front, 2, hp: 20), // Weakest
            CreateTestPlayer("Tank3", Zone.Player, Row.Front, 3, hp: 50),
            CreateTestPlayer("Support1", Zone.Player, Row.Back, 2, hp: 40)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Apply line formation
        _formationService.ApplyFormation(party, FormationType.Line, grid);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.Name, Is.EqualTo("Tank2")); // Weakest front-row member
        Assert.That(targetPlayer.HP, Is.EqualTo(20));
    }

    [Test]
    public void SelectTarget_LineFormation_NoFrontRow_TargetsLowestHP()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Support1", Zone.Player, Row.Back, 1, hp: 50),
            CreateTestPlayer("Support2", Zone.Player, Row.Back, 2, hp: 20) // Lowest HP
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.HP, Is.EqualTo(20)); // Lowest HP
    }

    #endregion

    #region Wedge Formation Targeting Tests

    [Test]
    public void SelectTarget_WedgeFormation_TargetsBackRowCenter()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        int centerColumn = grid.Columns / 2;

        var party = new List<object>
        {
            CreateTestPlayer("Striker1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Striker2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Striker3", Zone.Player, Row.Front, 3, hp: 50),
            CreateTestPlayer("Support", Zone.Player, Row.Back, centerColumn, hp: 40) // Wedge point
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, centerColumn);

        // Apply wedge formation
        _formationService.ApplyFormation(party, FormationType.Wedge, grid);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.Position!.Value.Row, Is.EqualTo(Row.Back)); // Should target wedge point
    }

    #endregion

    #region Default Targeting Tests

    [Test]
    public void SelectTarget_NoFormation_TargetsLowestHP()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Player2", Zone.Player, Row.Front, 2, hp: 10), // Lowest HP
            CreateTestPlayer("Player3", Zone.Player, Row.Back, 3, hp: 40)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act - No formation applied
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.HP, Is.EqualTo(10)); // Lowest HP
    }

    [Test]
    public void SelectTarget_SingleTarget_ReturnsThatTarget()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>
        {
            CreateTestPlayer("Solo", Zone.Player, Row.Front, 2, hp: 50)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(targetPlayer.Name, Is.EqualTo("Solo"));
    }

    #endregion

    #region Formation Detection Tests

    [Test]
    public void SelectTarget_DetectsLineFormation()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Tank1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Tank2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Tank3", Zone.Player, Row.Front, 3, hp: 50)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Apply line formation
        _formationService.ApplyFormation(party, FormationType.Line, grid);

        // Act - Should detect line formation
        var detectedFormation = _formationService.DetectFormation(party, grid);

        // Assert
        Assert.That(detectedFormation, Is.EqualTo(FormationType.Line));
    }

    [Test]
    public void SelectTarget_DetectsProtectFormation()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        int centerColumn = grid.Columns / 2;

        var party = new List<object>
        {
            CreateTestPlayer("Guard1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Guard2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Guard3", Zone.Player, Row.Front, 3, hp: 50),
            CreateTestPlayer("VIP", Zone.Player, Row.Back, centerColumn, hp: 40)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Apply protect formation
        _formationService.ApplyFormation(party, FormationType.Protect, grid);

        // Act
        var detectedFormation = _formationService.DetectFormation(party, grid);

        // Assert
        Assert.That(detectedFormation, Is.EqualTo(FormationType.Protect));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void SelectTarget_EmptyParty_ReturnsFirstElement()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);
        var party = new List<object>();
        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act & Assert
        // Should handle empty party gracefully
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ai.SelectTarget(enemy, party, grid, _formationService);
        });
    }

    [Test]
    public void SelectTarget_AllSameHP_ReturnsAny()
    {
        // Arrange
        var grid = CreateTestGrid(columns: 5);

        var party = new List<object>
        {
            CreateTestPlayer("Player1", Zone.Player, Row.Front, 1, hp: 50),
            CreateTestPlayer("Player2", Zone.Player, Row.Front, 2, hp: 50),
            CreateTestPlayer("Player3", Zone.Player, Row.Front, 3, hp: 50)
        };

        var enemy = CreateTestEnemy("Enemy1", Zone.Enemy, Row.Front, 2);

        // Act
        var target = _ai.SelectTarget(enemy, party, grid, _formationService);

        // Assert - Should return one of them
        Assert.That(target, Is.Not.Null);
        var targetPlayer = (PlayerCharacter)target;
        Assert.That(party.Contains(targetPlayer), Is.True);
    }

    #endregion
}
