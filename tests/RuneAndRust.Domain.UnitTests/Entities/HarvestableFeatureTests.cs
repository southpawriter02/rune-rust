using FluentAssertions;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;

namespace RuneAndRust.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for HarvestableFeature (v0.11.0b).
/// </summary>
[TestFixture]
public class HarvestableFeatureTests
{
    // ═══════════════════════════════════════════════════════════════
    // TEST FIXTURES
    // ═══════════════════════════════════════════════════════════════

    private HarvestableFeatureDefinition _basicDefinition = null!;
    private HarvestableFeatureDefinition _replenishingDefinition = null!;

    [SetUp]
    public void SetUp()
    {
        // Basic non-replenishing feature
        _basicDefinition = HarvestableFeatureDefinition.Create(
            "iron-ore-vein",
            "Iron Ore Vein",
            "A vein of iron ore embedded in the rock wall",
            "iron-ore",
            minQuantity: 1,
            maxQuantity: 5,
            difficultyClass: 12);

        // Replenishing feature (herb patch)
        _replenishingDefinition = HarvestableFeatureDefinition.Create(
            "herb-patch",
            "Herb Patch",
            "A patch of healing herbs growing in the shade",
            "healing-herb",
            minQuantity: 2,
            maxQuantity: 6,
            difficultyClass: 10,
            replenishTurns: 100);
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHOD TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Create_FromDefinition_SetsPropertiesCorrectly()
    {
        // Act
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Assert
        feature.Should().NotBeNull();
        feature.DefinitionId.Should().Be("iron-ore-vein");
        feature.Name.Should().Be("Iron Ore Vein");
        feature.Description.Should().Be("A vein of iron ore embedded in the rock wall");
        feature.RemainingQuantity.Should().Be(5);
        feature.InitialQuantity.Should().Be(5);
        feature.IsDepleted.Should().BeFalse();
        feature.IsInteractable.Should().BeTrue();
        feature.InteractionVerb.Should().Be("gather");
        feature.Id.Should().NotBeEmpty();
    }

    [Test]
    public void Create_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Act
        var act = () => HarvestableFeature.Create(null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("definition");
    }

    [Test]
    public void Create_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => HarvestableFeature.Create(_basicDefinition, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("initialQuantity");
    }

    [Test]
    public void Create_WithZeroQuantity_SucceedsAndIsDepleted()
    {
        // Act
        var feature = HarvestableFeature.Create(_basicDefinition, 0);

        // Assert
        feature.RemainingQuantity.Should().Be(0);
        feature.IsDepleted.Should().BeTrue();
    }

    [Test]
    public void CreateWithRandomQuantity_ReturnsQuantityInRange()
    {
        // Arrange
        var random = new Random(42); // Seeded for reproducibility

        // Act
        var feature = HarvestableFeature.CreateWithRandomQuantity(_basicDefinition, random);

        // Assert
        feature.RemainingQuantity.Should().BeInRange(1, 5);
        feature.InitialQuantity.Should().Be(feature.RemainingQuantity);
    }

    [Test]
    public void CreateWithRandomQuantity_WithNullRandom_UsesSharedRandom()
    {
        // Act
        var feature = HarvestableFeature.CreateWithRandomQuantity(_basicDefinition, null);

        // Assert
        feature.RemainingQuantity.Should().BeInRange(1, 5);
    }

    [Test]
    public void CreateWithRandomQuantity_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Act
        var act = () => HarvestableFeature.CreateWithRandomQuantity(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("definition");
    }

    // ═══════════════════════════════════════════════════════════════
    // HARVEST TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Harvest_ReducesRemainingQuantity()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act
        feature.Harvest(3);

        // Assert
        feature.RemainingQuantity.Should().Be(2);
        feature.IsDepleted.Should().BeFalse();
    }

    [Test]
    public void Harvest_ToZero_SetsDepleted()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);

        // Act
        feature.Harvest(3);

        // Assert
        feature.RemainingQuantity.Should().Be(0);
        feature.IsDepleted.Should().BeTrue();
    }

    [Test]
    public void Harvest_BeyondRemaining_ClampsToZero()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);

        // Act
        feature.Harvest(10); // More than remaining

        // Assert
        feature.RemainingQuantity.Should().Be(0);
        feature.IsDepleted.Should().BeTrue();
    }

    [Test]
    public void Harvest_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act
        var act = () => feature.Harvest(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("amount");
    }

    [Test]
    public void Harvest_WhenDepleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3); // Deplete it

        // Act
        var act = () => feature.Harvest(1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*depleted*");
    }

    [Test]
    public void Harvest_ZeroAmount_DoesNotChangeQuantity()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act
        feature.Harvest(0);

        // Assert
        feature.RemainingQuantity.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════
    // CAN HARVEST TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CanHarvest_WithRemainingQuantity_ReturnsTrue()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act & Assert
        feature.CanHarvest.Should().BeTrue();
    }

    [Test]
    public void CanHarvest_WhenDepleted_ReturnsFalse()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3);

        // Act & Assert
        feature.CanHarvest.Should().BeFalse();
    }

    [Test]
    public void CanHarvest_WhenAwaitingReplenishment_ReturnsFalse()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3);
        feature.SetReplenishTimer(100, 100);

        // Act & Assert
        feature.CanHarvest.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // REPLENISHMENT TIMER TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void SetReplenishTimer_SetsCorrectTurn()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3); // Deplete

        // Act
        feature.SetReplenishTimer(currentTurn: 50, replenishTurns: 100);

        // Assert
        feature.ReplenishAtTurn.Should().Be(150);
        feature.IsAwaitingReplenishment.Should().BeTrue();
    }

    [Test]
    public void SetReplenishTimer_WithZeroReplenishTurns_DoesNotSetTimer()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3);

        // Act
        feature.SetReplenishTimer(currentTurn: 50, replenishTurns: 0);

        // Assert
        feature.ReplenishAtTurn.Should().BeNull();
        feature.IsAwaitingReplenishment.Should().BeFalse();
    }

    [Test]
    public void SetReplenishTimer_WithNegativeReplenishTurns_DoesNotSetTimer()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3);

        // Act
        feature.SetReplenishTimer(currentTurn: 50, replenishTurns: -10);

        // Assert
        feature.ReplenishAtTurn.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // SHOULD REPLENISH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ShouldReplenish_BeforeTargetTurn_ReturnsFalse()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3);
        feature.SetReplenishTimer(50, 100); // Replenishes at turn 150

        // Act & Assert
        feature.ShouldReplenish(149).Should().BeFalse();
    }

    [Test]
    public void ShouldReplenish_AtTargetTurn_ReturnsTrue()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3);
        feature.SetReplenishTimer(50, 100); // Replenishes at turn 150

        // Act & Assert
        feature.ShouldReplenish(150).Should().BeTrue();
    }

    [Test]
    public void ShouldReplenish_AfterTargetTurn_ReturnsTrue()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3);
        feature.SetReplenishTimer(50, 100); // Replenishes at turn 150

        // Act & Assert
        feature.ShouldReplenish(200).Should().BeTrue();
    }

    [Test]
    public void ShouldReplenish_WithNoTimerSet_ReturnsFalse()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3);

        // Act & Assert
        feature.ShouldReplenish(1000).Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // REPLENISH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Replenish_RestoresToInitialQuantity()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 5);
        feature.Harvest(5);
        feature.SetReplenishTimer(50, 100);

        // Act
        feature.Replenish();

        // Assert
        feature.RemainingQuantity.Should().Be(5);
        feature.IsDepleted.Should().BeFalse();
    }

    [Test]
    public void Replenish_ClearsReplenishTimer()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 5);
        feature.Harvest(5);
        feature.SetReplenishTimer(50, 100);

        // Act
        feature.Replenish();

        // Assert
        feature.ReplenishAtTurn.Should().BeNull();
        feature.IsAwaitingReplenishment.Should().BeFalse();
    }

    [Test]
    public void Replenish_WithQuantity_RestoresToSpecificQuantity()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 5);
        feature.Harvest(5);
        feature.SetReplenishTimer(50, 100);

        // Act
        feature.Replenish(3);

        // Assert
        feature.RemainingQuantity.Should().Be(3);
        feature.ReplenishAtTurn.Should().BeNull();
    }

    [Test]
    public void Replenish_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 5);
        feature.Harvest(5);

        // Act
        var act = () => feature.Replenish(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("quantity");
    }

    // ═══════════════════════════════════════════════════════════════
    // DISPLAY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetStatusDisplay_WithRemainingQuantity_ShowsRemaining()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act
        var result = feature.GetStatusDisplay();

        // Assert
        result.Should().Contain("5 remaining");
    }

    [Test]
    public void GetStatusDisplay_WhenDepleted_ShowsDepleted()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 3);
        feature.Harvest(3);

        // Act
        var result = feature.GetStatusDisplay();

        // Assert
        result.Should().Contain("Depleted");
    }

    [Test]
    public void GetStatusDisplay_WhenAwaitingReplenishment_ShowsReplenishTurn()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_replenishingDefinition, 3);
        feature.Harvest(3);
        feature.SetReplenishTimer(50, 100);

        // Act
        var result = feature.GetStatusDisplay();

        // Assert
        result.Should().Contain("Depleted");
        result.Should().Contain("Replenishes");
        result.Should().Contain("150");
    }

    [Test]
    public void ToString_ReturnsNameWithStatus()
    {
        // Arrange
        var feature = HarvestableFeature.Create(_basicDefinition, 5);

        // Act
        var result = feature.ToString();

        // Assert
        result.Should().Contain("Iron Ore Vein");
        result.Should().Contain("5 remaining");
    }
}
