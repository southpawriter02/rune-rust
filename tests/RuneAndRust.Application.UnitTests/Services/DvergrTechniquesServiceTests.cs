using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="DvergrTechniquesService"/>.
/// Validates crafting cost and time reduction logic.
/// </summary>
[TestFixture]
public class DvergrTechniquesServiceTests
{
    private DvergrTechniquesService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new DvergrTechniquesService(
            Mock.Of<ILogger<DvergrTechniquesService>>());
    }

    /// <summary>
    /// Creates a Rúnasmiðr player with Dvergr Techniques unlocked.
    /// </summary>
    private static Player CreateRunasmidrWithDvergrTechniques()
    {
        var player = new Player("Test Rúnasmiðr");
        player.SetSpecialization("runasmidr");
        player.InitializeRuneCharges();
        player.UnlockRunasmidrAbility(RunasmidrAbilityId.DvergrTechniques);
        return player;
    }

    /// <summary>
    /// Creates a Rúnasmiðr player WITHOUT Dvergr Techniques.
    /// </summary>
    private static Player CreateRunasmidrWithoutDvergr()
    {
        var player = new Player("Test Rúnasmiðr");
        player.SetSpecialization("runasmidr");
        player.InitializeRuneCharges();
        return player;
    }

    // ===== Material Cost Tests =====

    [Test]
    public void ModifyMaterialCost_WithDvergrTechniques_Reduces20Percent()
    {
        // Arrange
        var player = CreateRunasmidrWithDvergrTechniques();

        // Act
        var reducedCost = _service.ModifyMaterialCost(player, 50);

        // Assert — 50 * 0.80 = 40
        reducedCost.Should().Be(40);
    }

    [Test]
    public void ModifyMaterialCost_WithoutDvergrTechniques_ReturnsOriginal()
    {
        // Arrange
        var player = CreateRunasmidrWithoutDvergr();

        // Act
        var cost = _service.ModifyMaterialCost(player, 50);

        // Assert
        cost.Should().Be(50);
    }

    [Test]
    public void ModifyMaterialCost_SmallCost_NeverGoesBelow1()
    {
        // Arrange
        var player = CreateRunasmidrWithDvergrTechniques();

        // Act — 1 * 0.80 = 0.8, should round to minimum 1
        var reducedCost = _service.ModifyMaterialCost(player, 1);

        // Assert
        reducedCost.Should().Be(1);
    }

    [Test]
    public void ModifyMaterialCost_ZeroCost_ReturnsZero()
    {
        // Arrange
        var player = CreateRunasmidrWithDvergrTechniques();

        // Act
        var cost = _service.ModifyMaterialCost(player, 0);

        // Assert
        cost.Should().Be(0);
    }

    // ===== Crafting Time Tests =====

    [Test]
    public void ModifyCraftingTime_WithDvergrTechniques_Reduces20Percent()
    {
        // Arrange
        var player = CreateRunasmidrWithDvergrTechniques();

        // Act — 60 * 0.80 = 48
        var reducedTime = _service.ModifyCraftingTime(player, 60);

        // Assert
        reducedTime.Should().Be(48);
    }

    [Test]
    public void ModifyCraftingTime_WithoutDvergrTechniques_ReturnsOriginal()
    {
        // Arrange
        var player = CreateRunasmidrWithoutDvergr();

        // Act
        var time = _service.ModifyCraftingTime(player, 60);

        // Assert
        time.Should().Be(60);
    }

    // ===== Percentage Query Tests =====

    [Test]
    public void GetCostReductionPercentage_Returns20Percent()
    {
        // Act & Assert
        _service.GetCostReductionPercentage().Should().Be(0.20m);
    }

    // ===== Validation Tests =====

    [Test]
    public void ModifyMaterialCost_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => _service.ModifyMaterialCost(null!, 50);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ModifyMaterialCost_NegativeCost_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var player = CreateRunasmidrWithDvergrTechniques();

        // Act
        var act = () => _service.ModifyMaterialCost(player, -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
