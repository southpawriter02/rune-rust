using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="DamageService"/>.
/// </summary>
[TestFixture]
public class DamageServiceTests
{
    private Mock<ISkjaldmaerAbilityService> _mockAbilityService = null!;
    private DamageService _damageService = null!;
    private Player _player = null!;

    [SetUp]
    public void Setup()
    {
        _mockAbilityService = new Mock<ISkjaldmaerAbilityService>();
        _damageService = new DamageService(
            _mockAbilityService.Object,
            Mock.Of<ILogger<DamageService>>());

        _player = new Player("Test Skjaldmaer");
        _player.SetSpecialization("skjaldmaer");
    }

    [Test]
    public void CalculateFinalDamage_WithUnbreakable_ReducesByThree()
    {
        // Arrange
        _mockAbilityService
            .Setup(s => s.GetDamageReduction(_player))
            .Returns(3);

        // Act
        var finalDamage = _damageService.CalculateFinalDamage(_player, 10);

        // Assert
        finalDamage.Should().Be(7); // 10 - 3
    }

    [Test]
    public void CalculateFinalDamage_WithUnbreakable_MinimumOneDamage()
    {
        // Arrange
        _mockAbilityService
            .Setup(s => s.GetDamageReduction(_player))
            .Returns(3);

        // Act
        var finalDamage = _damageService.CalculateFinalDamage(_player, 2);

        // Assert
        finalDamage.Should().Be(1); // max(1, 2 - 3) = 1
    }

    [Test]
    public void CalculateFinalDamage_WithoutUnbreakable_ReturnsFullDamage()
    {
        // Arrange
        _mockAbilityService
            .Setup(s => s.GetDamageReduction(_player))
            .Returns(0);

        // Act
        var finalDamage = _damageService.CalculateFinalDamage(_player, 10);

        // Assert
        finalDamage.Should().Be(10);
    }

    [Test]
    public void CheckLethalDamageProtection_WhenWallLivesActive_CapsDamage()
    {
        // Arrange
        _player.TheWallLivesState = new TheWallLivesState();
        _player.TheWallLivesState.Activate();
        // Give player some health — Player starts at MaxHealth (Stats.MaxHealth)
        // TakeDamage reduces health, so player is at full HP after creation

        // Act
        var result = _damageService.CheckLethalDamageProtection(_player, 999);

        // Assert — should cap damage so player stays at 1 HP
        result.Should().BeLessThan(999);
        result.Should().Be(_player.Health - 1); // Caps to leave exactly 1 HP
    }

    [Test]
    public void CheckLethalDamageProtection_WhenInactive_ReturnsFullDamage()
    {
        // Act
        var result = _damageService.CheckLethalDamageProtection(_player, 50);

        // Assert
        result.Should().Be(50);
    }

    [Test]
    public void ApplyDamage_WithZeroDamage_ReturnsZero()
    {
        // Act
        var result = _damageService.ApplyDamage(_player, 0);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void ApplyDamage_WithNegativeDamage_ReturnsZero()
    {
        // Act
        var result = _damageService.ApplyDamage(_player, -5);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void ApplyDamage_ReducesPlayerHealth()
    {
        // Arrange
        _mockAbilityService
            .Setup(s => s.GetDamageReduction(_player))
            .Returns(0);
        var healthBefore = _player.Health;

        // Act
        var damage = _damageService.ApplyDamage(_player, 5);

        // Assert
        damage.Should().Be(5);
        _player.Health.Should().Be(healthBefore - 5);
    }
}
