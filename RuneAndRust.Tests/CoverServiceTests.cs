using NUnit.Framework;
using RuneAndRust.Core;
using RuneAndRust.Engine;

namespace RuneAndRust.Tests;

/// <summary>
/// v0.20.2: Unit tests for CoverService
/// Tests cover bonus calculation, cover applicability rules, and cover destruction.
/// Target: 30+ tests for 90%+ coverage
/// </summary>
[TestFixture]
public class CoverServiceTests
{
    private CoverService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new CoverService();
    }

    #region Helper Methods

    private BattlefieldGrid CreateTestGrid(int columns = 5)
    {
        return new BattlefieldGrid(columns);
    }

    private BattlefieldTile CreateTile(CoverType coverType, Zone zone = Zone.Player, Row row = Row.Back, int column = 2)
    {
        var position = new GridPosition(zone, row, column);
        var tile = new BattlefieldTile(position);
        tile.Cover = coverType;

        // Set health for physical cover
        if (coverType == CoverType.Physical || coverType == CoverType.Both)
        {
            tile.CoverHealth = 20;
        }

        return tile;
    }

    #endregion

    #region Cover Bonus Calculation - Physical Cover

    [Test]
    public void CalculateCoverBonus_PhysicalCover_RangedAttack_OpposingZones_ReturnsDefenseBonus()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Physical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(4), "Physical cover should grant +4 Defense vs ranged");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0), "Physical cover should not grant Resolve bonus");
    }

    [Test]
    public void CalculateCoverBonus_PhysicalCover_MeleeAttack_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Physical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Melee, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Melee attacks should ignore cover");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_PhysicalCover_PsychicAttack_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Physical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Psychic, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Physical cover should not block psychic attacks");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    #endregion

    #region Cover Bonus Calculation - Metaphysical Cover

    [Test]
    public void CalculateCoverBonus_MetaphysicalCover_PsychicAttack_OpposingZones_ReturnsResolveBonus()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Psychic, grid);

        // Assert
        Assert.That(bonus.ResolveBonus, Is.EqualTo(4), "Metaphysical cover should grant +4 Resolve vs psychic");
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Metaphysical cover should not grant Defense bonus");
    }

    [Test]
    public void CalculateCoverBonus_MetaphysicalCover_RangedAttack_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Metaphysical cover should not block ranged attacks");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_MetaphysicalCover_MeleeAttack_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Melee, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    #endregion

    #region Cover Bonus Calculation - Both Cover Types

    [Test]
    public void CalculateCoverBonus_BothCover_RangedAttack_ReturnsDefenseBonus()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Both;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(4), "Both cover should grant +4 Defense vs ranged");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_BothCover_PsychicAttack_ReturnsResolveBonus()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Both;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Psychic, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(4), "Both cover should grant +4 Resolve vs psychic");
    }

    [Test]
    public void CalculateCoverBonus_BothCover_MeleeAttack_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Both;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Melee, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Melee attacks should ignore all cover types");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    #endregion

    #region Cover Applicability Rules - Same Zone

    [Test]
    public void CalculateCoverBonus_SameZone_PhysicalCover_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Player, Row.Front, 3); // Same zone

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Physical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0), "Cover should not apply in same zone");
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_SameZone_MetaphysicalCover_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Enemy, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 1); // Same zone

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Metaphysical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Psychic, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0), "Cover should not apply in same zone");
    }

    #endregion

    #region Cover Applicability Rules - Null/Missing Data

    [Test]
    public void CalculateCoverBonus_NullTargetPosition_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        // Act
        var bonus = _service.CalculateCoverBonus(null, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_NullAttackerPosition_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.Physical;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, null, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_NullGrid_ReturnsNone()
    {
        // Arrange
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, null!);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    [Test]
    public void CalculateCoverBonus_NoCoverOnTile_ReturnsNone()
    {
        // Arrange
        var grid = CreateTestGrid();
        var targetPosition = new GridPosition(Zone.Player, Row.Back, 2);
        var attackerPosition = new GridPosition(Zone.Enemy, Row.Front, 2);

        var targetTile = grid.GetTile(targetPosition);
        targetTile.Cover = CoverType.None;

        // Act
        var bonus = _service.CalculateCoverBonus(targetPosition, attackerPosition, AttackType.Ranged, grid);

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
    }

    #endregion

    #region Cover Destruction - Physical Cover

    [Test]
    public void DamageCover_PhysicalCover_PartialDamage_ReducesHealth()
    {
        // Arrange
        var tile = CreateTile(CoverType.Physical);
        tile.CoverHealth = 20;
        int damageToTarget = 12; // Cover takes 25% = 3 damage

        // Act
        var message = _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(tile.CoverHealth, Is.EqualTo(17), "Cover should take 25% of damage (3)");
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Physical), "Cover should still exist");
        Assert.That(message, Does.Contain("17 HP remaining"));
    }

    [Test]
    public void DamageCover_PhysicalCover_ExceedsHP_DestroysPhysicalCover()
    {
        // Arrange
        var tile = CreateTile(CoverType.Physical);
        tile.CoverHealth = 10;
        tile.CoverDescription = "Crate";
        int damageToTarget = 60; // Cover takes 25% = 15 damage

        // Act
        var message = _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.None), "Cover should be destroyed");
        Assert.That(tile.CoverHealth, Is.Null, "Cover health should be null");
        Assert.That(message, Does.Contain("COVER DESTROYED"));
        Assert.That(message, Does.Contain("Crate"));
    }

    [Test]
    public void DamageCover_BothCoverType_ExceedsHP_PreservesMetaphysical()
    {
        // Arrange
        var tile = CreateTile(CoverType.Both);
        tile.CoverHealth = 10;
        tile.CoverDescription = "Sanctified Barricade";
        int damageToTarget = 60; // Cover takes 25% = 15 damage

        // Act
        var message = _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Metaphysical), "Should preserve metaphysical cover");
        Assert.That(tile.CoverHealth, Is.Null, "Physical health should be null");
        Assert.That(tile.CoverDescription, Is.EqualTo("Runic Anchor"), "Should update description");
        Assert.That(message, Does.Contain("COVER DESTROYED"));
    }

    [Test]
    public void DamageCover_MetaphysicalCoverOnly_ReturnsNull()
    {
        // Arrange
        var tile = CreateTile(CoverType.Metaphysical);
        tile.CoverDescription = "Runic Anchor";
        int damageToTarget = 50;

        // Act
        var message = _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(message, Is.Null, "Metaphysical cover cannot be damaged");
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Metaphysical), "Cover should be unchanged");
    }

    [Test]
    public void DamageCover_NoCover_ReturnsNull()
    {
        // Arrange
        var tile = CreateTile(CoverType.None);
        int damageToTarget = 50;

        // Act
        var message = _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(message, Is.Null, "No cover to damage");
    }

    [Test]
    public void DamageCover_MinimumOneDamage_EvenForLowDamage()
    {
        // Arrange
        var tile = CreateTile(CoverType.Physical);
        tile.CoverHealth = 20;
        int damageToTarget = 2; // 25% = 0.5, should round to 1

        // Act
        _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(tile.CoverHealth, Is.EqualTo(19), "Cover should take minimum 1 damage");
    }

    [Test]
    public void DamageCover_InitializesHealthIfNull()
    {
        // Arrange
        var tile = CreateTile(CoverType.Physical);
        tile.CoverHealth = null; // No health set
        int damageToTarget = 20;

        // Act
        _service.DamageCover(tile, damageToTarget);

        // Assert
        Assert.That(tile.CoverHealth, Is.EqualTo(15), "Should initialize to 20, then take 5 damage");
    }

    #endregion

    #region Cover Placement

    [Test]
    public void PlaceCover_PhysicalCover_SetsHealthAndDescription()
    {
        // Arrange
        var grid = CreateTestGrid();
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2));

        // Act
        _service.PlaceCover(tile, CoverType.Physical);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Physical));
        Assert.That(tile.CoverHealth, Is.EqualTo(20), "Default health should be 20");
        Assert.That(tile.CoverDescription, Is.Not.Null, "Should have auto-generated description");
    }

    [Test]
    public void PlaceCover_MetaphysicalCover_SetsDescription_NoHealth()
    {
        // Arrange
        var grid = CreateTestGrid();
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2));

        // Act
        _service.PlaceCover(tile, CoverType.Metaphysical);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Metaphysical));
        Assert.That(tile.CoverHealth, Is.Null, "Metaphysical cover has no health");
        Assert.That(tile.CoverDescription, Is.EqualTo("Runic Anchor"));
    }

    [Test]
    public void PlaceCover_BothCover_SetsHealthAndDescription()
    {
        // Arrange
        var grid = CreateTestGrid();
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2));

        // Act
        _service.PlaceCover(tile, CoverType.Both);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.Both));
        Assert.That(tile.CoverHealth, Is.EqualTo(20));
        Assert.That(tile.CoverDescription, Is.EqualTo("Sanctified Barricade"));
    }

    [Test]
    public void PlaceCover_CustomDescription_UsesProvidedDescription()
    {
        // Arrange
        var grid = CreateTestGrid();
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2));

        // Act
        _service.PlaceCover(tile, CoverType.Physical, "Ancient Pillar", 30);

        // Assert
        Assert.That(tile.CoverDescription, Is.EqualTo("Ancient Pillar"));
        Assert.That(tile.CoverHealth, Is.EqualTo(30));
    }

    [Test]
    public void PlaceCover_NullTile_DoesNothing()
    {
        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => _service.PlaceCover(null!, CoverType.Physical));
    }

    [Test]
    public void PlaceCover_NoneCoverType_DoesNothing()
    {
        // Arrange
        var grid = CreateTestGrid();
        var tile = grid.GetTile(new GridPosition(Zone.Player, Row.Back, 2));

        // Act
        _service.PlaceCover(tile, CoverType.None);

        // Assert
        Assert.That(tile.Cover, Is.EqualTo(CoverType.None));
        Assert.That(tile.CoverHealth, Is.Null);
    }

    #endregion

    #region CoverBonus Helper Tests

    [Test]
    public void CoverBonus_None_ReturnsZeroBonuses()
    {
        // Act
        var bonus = CoverBonus.None();

        // Assert
        Assert.That(bonus.DefenseBonus, Is.EqualTo(0));
        Assert.That(bonus.ResolveBonus, Is.EqualTo(0));
        Assert.That(bonus.HasBonus(), Is.False);
    }

    [Test]
    public void CoverBonus_HasBonus_DetectsDefenseBonus()
    {
        // Arrange
        var bonus = new CoverBonus { DefenseBonus = 4, ResolveBonus = 0 };

        // Assert
        Assert.That(bonus.HasBonus(), Is.True);
    }

    [Test]
    public void CoverBonus_HasBonus_DetectsResolveBonus()
    {
        // Arrange
        var bonus = new CoverBonus { DefenseBonus = 0, ResolveBonus = 4 };

        // Assert
        Assert.That(bonus.HasBonus(), Is.True);
    }

    #endregion
}
