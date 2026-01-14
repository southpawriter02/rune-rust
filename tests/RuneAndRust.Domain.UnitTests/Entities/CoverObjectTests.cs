using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for the CoverObject entity.
/// </summary>
[TestFixture]
public class CoverObjectTests
{
    private CoverDefinition _partialDef = null!;
    private CoverDefinition _fullDef = null!;
    private CoverDefinition _destructibleDef = null!;

    [SetUp]
    public void Setup()
    {
        _partialDef = CoverDefinition.Create(
            id: "partial-cover",
            name: "Partial Cover",
            coverType: CoverType.Partial,
            defenseBonus: 2,
            isDestructible: false);

        _fullDef = CoverDefinition.Create(
            id: "full-cover",
            name: "Full Cover",
            coverType: CoverType.Full,
            blocksLOS: true);

        _destructibleDef = CoverDefinition.Create(
            id: "crate",
            name: "Crate",
            coverType: CoverType.Partial,
            defenseBonus: 2,
            isDestructible: true,
            maxHitPoints: 10);
    }

    [Test]
    public void Create_WithValidDefinition_CreatesCoverObject()
    {
        // Arrange
        var position = new GridPosition(3, 4);

        // Act
        var cover = CoverObject.Create(_partialDef, position);

        // Assert
        cover.Id.Should().NotBeEmpty();
        cover.DefinitionId.Should().Be("partial-cover");
        cover.Name.Should().Be("Partial Cover");
        cover.Position.Should().Be(position);
        cover.CoverType.Should().Be(CoverType.Partial);
        cover.DefenseBonus.Should().Be(2);
        cover.IsDestructible.Should().BeFalse();
    }

    [Test]
    public void Create_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => CoverObject.Create(null!, new GridPosition(0, 0));

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Create_WithDestructibleDef_SetsHpValues()
    {
        // Act
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));

        // Assert
        cover.CurrentHitPoints.Should().Be(10);
        cover.MaxHitPoints.Should().Be(10);
        cover.IsDestructible.Should().BeTrue();
        cover.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void TakeDamage_WhenDestructible_ReducesHitPoints()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));

        // Act
        var destroyed = cover.TakeDamage(4);

        // Assert
        destroyed.Should().BeFalse();
        cover.CurrentHitPoints.Should().Be(6);
        cover.IsDestroyed.Should().BeFalse();
    }

    [Test]
    public void TakeDamage_WhenKillingBlow_ReturnsTrue()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));

        // Act
        var destroyed = cover.TakeDamage(15);

        // Assert
        destroyed.Should().BeTrue();
        cover.CurrentHitPoints.Should().Be(0);
        cover.IsDestroyed.Should().BeTrue();
    }

    [Test]
    public void TakeDamage_WhenNotDestructible_ReturnsFalse()
    {
        // Arrange
        var cover = CoverObject.Create(_partialDef, new GridPosition(0, 0));

        // Act
        var destroyed = cover.TakeDamage(100);

        // Assert
        destroyed.Should().BeFalse();
    }

    [Test]
    public void TakeDamage_WhenAlreadyDestroyed_ReturnsFalse()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));
        cover.TakeDamage(20); // Destroy it

        // Act
        var destroyed = cover.TakeDamage(5);

        // Assert
        destroyed.Should().BeFalse();
    }

    [Test]
    public void GetHpPercentage_WhenFull_Returns100()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));

        // Assert
        cover.GetHpPercentage().Should().Be(100);
    }

    [Test]
    public void GetHpPercentage_WhenHalf_Returns50()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));
        cover.TakeDamage(5);

        // Assert
        cover.GetHpPercentage().Should().Be(50);
    }

    [Test]
    public void GetHpPercentage_WhenDestroyed_Returns0()
    {
        // Arrange
        var cover = CoverObject.Create(_destructibleDef, new GridPosition(0, 0));
        cover.TakeDamage(20);

        // Assert
        cover.GetHpPercentage().Should().Be(0);
    }

    [Test]
    public void Create_WithFullCover_SetsBlocksLOS()
    {
        // Act
        var cover = CoverObject.Create(_fullDef, new GridPosition(0, 0));

        // Assert
        cover.CoverType.Should().Be(CoverType.Full);
        cover.BlocksLOS.Should().BeTrue();
    }
}
