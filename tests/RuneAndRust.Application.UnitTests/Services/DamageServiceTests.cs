// ═══════════════════════════════════════════════════════════════════════════════
// DamageServiceTests.cs
// Unit tests for the DamageService damage pipeline.
// Version: 0.20.1c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class DamageServiceTests
{
    private DamageService _service = null!;
    private SkjaldmaerTier3AbilityService _tier3Service = null!;
    private Mock<ILogger<DamageService>> _mockDamageLogger = null!;
    private Mock<ILogger<SkjaldmaerTier3AbilityService>> _mockTier3Logger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDamageLogger = new Mock<ILogger<DamageService>>();
        _mockTier3Logger = new Mock<ILogger<SkjaldmaerTier3AbilityService>>();
        _tier3Service = new SkjaldmaerTier3AbilityService(_mockTier3Logger.Object);
        _service = new DamageService(_mockDamageLogger.Object, _tier3Service);
    }

    // ═══════ CalculateFinalDamage ═══════

    [Test]
    public void CalculateFinalDamage_WithUnbreakable_ReducesDamage()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId> { SkjaldmaerAbilityId.Unbreakable };

        // Act — 10 damage reduced by 3
        var result = _service.CalculateFinalDamage(abilities, wallState: null, currentHp: 50, incomingDamage: 10);

        // Assert
        result.Should().Be(7);
    }

    [Test]
    public void CalculateFinalDamage_WithUnbreakable_MinimumOneDamage()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId> { SkjaldmaerAbilityId.Unbreakable };

        // Act — 2 damage reduced by 3, minimum 1
        var result = _service.CalculateFinalDamage(abilities, wallState: null, currentHp: 50, incomingDamage: 2);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public void CalculateFinalDamage_WithoutUnbreakable_NoReduction()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId> { SkjaldmaerAbilityId.ShieldWall };

        // Act
        var result = _service.CalculateFinalDamage(abilities, wallState: null, currentHp: 50, incomingDamage: 10);

        // Assert
        result.Should().Be(10);
    }

    [Test]
    public void CalculateFinalDamage_WithWallLives_PreventsLethalDamage()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>();
        var wallState = TheWallLivesState.Activate();

        // Act — 10 damage at 5 HP would be lethal
        var result = _service.CalculateFinalDamage(abilities, wallState, currentHp: 5, incomingDamage: 10);

        // Assert — capped to leave 1 HP
        result.Should().Be(4);
    }

    [Test]
    public void CalculateFinalDamage_WithBothAbilities_AppliesBothReductions()
    {
        // Arrange — Unbreakable + The Wall Lives
        var abilities = new List<SkjaldmaerAbilityId> { SkjaldmaerAbilityId.Unbreakable };
        var wallState = TheWallLivesState.Activate();

        // Act — 50 damage at 5 HP:
        //   Step 1: Unbreakable reduces 50 → 47
        //   Step 2: TWL caps 47 to 4 (5 HP - 1 = 4 max damage)
        var result = _service.CalculateFinalDamage(abilities, wallState, currentHp: 5, incomingDamage: 50);

        // Assert
        result.Should().Be(4);
    }

    [Test]
    public void CalculateFinalDamage_WithEmptyAbilities_NullWall_ReturnsFullDamage()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>();

        // Act
        var result = _service.CalculateFinalDamage(abilities, wallState: null, currentHp: 50, incomingDamage: 15);

        // Assert
        result.Should().Be(15);
    }

    // ═══════ CheckLethalDamageProtection ═══════

    [Test]
    public void CheckLethalDamageProtection_WhenNull_ReturnsUnchanged()
    {
        // Act
        var result = _service.CheckLethalDamageProtection(wallState: null, currentHp: 5, damageAmount: 10);

        // Assert
        result.Should().Be(10);
    }

    [Test]
    public void CheckLethalDamageProtection_WhenInactive_ReturnsUnchanged()
    {
        // Arrange
        var wallState = TheWallLivesState.Deactivate();

        // Act
        var result = _service.CheckLethalDamageProtection(wallState, currentHp: 5, damageAmount: 10);

        // Assert
        result.Should().Be(10);
    }

    [Test]
    public void CheckLethalDamageProtection_WhenActive_CapsLethalDamage()
    {
        // Arrange
        var wallState = TheWallLivesState.Activate();

        // Act
        var result = _service.CheckLethalDamageProtection(wallState, currentHp: 5, damageAmount: 10);

        // Assert
        result.Should().Be(4);
    }

    [Test]
    public void CheckLethalDamageProtection_WhenActiveNonLethal_ReturnsUnchanged()
    {
        // Arrange
        var wallState = TheWallLivesState.Activate();

        // Act
        var result = _service.CheckLethalDamageProtection(wallState, currentHp: 50, damageAmount: 5);

        // Assert
        result.Should().Be(5);
    }
}
