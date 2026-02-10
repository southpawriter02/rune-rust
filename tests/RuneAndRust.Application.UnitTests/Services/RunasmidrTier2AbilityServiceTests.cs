// ═══════════════════════════════════════════════════════════════════════════════
// RunasmidrTier2AbilityServiceTests.cs
// Unit tests for the RunasmidrTier2AbilityService, validating Empowered
// Inscription, Runic Trap, Dvergr Techniques, and PP prerequisite logic.
// Version: 0.20.2b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="RunasmidrTier2AbilityService"/>.
/// </summary>
[TestFixture]
public class RunasmidrTier2AbilityServiceTests
{
    private RunasmidrTier2AbilityService _service = null!;
    private Mock<ILogger<RunasmidrTier2AbilityService>> _mockLogger = null!;
    private Random _seededRandom = null!;

    private readonly Guid _weaponId = Guid.NewGuid();
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _enemyId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<RunasmidrTier2AbilityService>>();
        _seededRandom = new Random(42);
        _service = new RunasmidrTier2AbilityService(_mockLogger.Object, _seededRandom);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Empowered Inscription Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CreateEmpoweredRune_WithValidElement_CreatesRune()
    {
        // Arrange & Act
        var rune = _service.CreateEmpoweredRune(_weaponId, "fire");

        // Assert
        rune.Should().NotBeNull();
        rune!.ElementalDamageTypeId.Should().Be("fire");
        rune.TargetItemId.Should().Be(_weaponId);
        rune.BonusDice.Should().Be("1d6");
        rune.RemainingDuration.Should().Be(10);
    }

    [Test]
    public void CreateEmpoweredRune_WithInvalidElement_ReturnsNull()
    {
        // Arrange & Act
        var rune = _service.CreateEmpoweredRune(_weaponId, "poison");

        // Assert
        rune.Should().BeNull();
    }

    [Test]
    public void TickEmpoweredRune_DecrementsDuration()
    {
        // Arrange
        var rune = EmpoweredRune.Create(_weaponId, "cold");

        // Act
        var ticked = _service.TickEmpoweredRune(rune);

        // Assert
        ticked.RemainingDuration.Should().Be(9);
        ticked.IsExpired.Should().BeFalse();
    }

    [Test]
    public void TickEmpoweredRune_AtExpiry_LogsExpiration()
    {
        // Arrange — create a rune with 1 turn remaining
        var rune = EmpoweredRune.Create(_weaponId, "lightning");
        for (var i = 0; i < EmpoweredRune.DefaultDuration - 1; i++)
            rune = rune.Tick();

        // Act
        var expired = _service.TickEmpoweredRune(rune);

        // Assert
        expired.IsExpired.Should().BeTrue();
        expired.RemainingDuration.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Runic Trap Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void PlaceRunicTrap_WithCapacityAvailable_CreatesTrap()
    {
        // Arrange
        var activeTraps = new List<RunicTrap>();

        // Act
        var trap = _service.PlaceRunicTrap(_ownerId, (5, 3), activeTraps);

        // Assert
        trap.Should().NotBeNull();
        trap!.OwnerId.Should().Be(_ownerId);
        trap.Position.Should().Be((5, 3));
        trap.DamageDice.Should().Be("3d6");
        trap.IsActive.Should().BeTrue();
    }

    [Test]
    public void PlaceRunicTrap_AtMaxCapacity_ReturnsNull()
    {
        // Arrange — 3 active traps (max)
        var activeTraps = new List<RunicTrap>
        {
            RunicTrap.Create(_ownerId, (1, 1)),
            RunicTrap.Create(_ownerId, (2, 2)),
            RunicTrap.Create(_ownerId, (3, 3))
        };

        // Act
        var trap = _service.PlaceRunicTrap(_ownerId, (4, 4), activeTraps);

        // Assert
        trap.Should().BeNull();
    }

    [Test]
    public void TriggerTrap_ActiveTrap_DealsDamage()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, (5, 3));

        // Act
        var (result, updatedTrap) = _service.TriggerTrap(trap, _enemyId);

        // Assert
        result.Success.Should().BeTrue();
        result.DamageRoll.Should().BeGreaterThan(0);
        result.DamageRoll.Should().BeLessThanOrEqualTo(18);
        updatedTrap.IsTriggered.Should().BeTrue();
    }

    [Test]
    public void TriggerTrap_AlreadyTriggered_ReturnsFailed()
    {
        // Arrange
        var trap = RunicTrap.Create(_ownerId, (5, 3));
        var (_, triggered) = _service.TriggerTrap(trap, _enemyId);

        // Act — trigger again
        var (result, _) = _service.TriggerTrap(triggered, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public void CanPlaceTrap_BelowMax_ReturnsTrue()
    {
        // Arrange
        var activeTraps = new List<RunicTrap>
        {
            RunicTrap.Create(_ownerId, (1, 1)),
            RunicTrap.Create(_ownerId, (2, 2))
        };

        // Act & Assert
        _service.CanPlaceTrap(activeTraps).Should().BeTrue();
    }

    [Test]
    public void CanPlaceTrap_AtMax_ReturnsFalse()
    {
        // Arrange
        var activeTraps = new List<RunicTrap>
        {
            RunicTrap.Create(_ownerId, (1, 1)),
            RunicTrap.Create(_ownerId, (2, 2)),
            RunicTrap.Create(_ownerId, (3, 3))
        };

        // Act & Assert
        _service.CanPlaceTrap(activeTraps).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dvergr Techniques Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ApplyDvergrCostReduction_ReducesByTwentyPercent()
    {
        // Arrange & Act
        var reduced = _service.ApplyDvergrCostReduction(100);

        // Assert — 20% of 100 = 20 reduction
        reduced.Should().Be(80);
    }

    [Test]
    public void ApplyDvergrCostReduction_RoundsDown()
    {
        // Arrange & Act — 20% of 7 = 1.4, floor = 1
        var reduced = _service.ApplyDvergrCostReduction(7);

        // Assert
        reduced.Should().Be(6);
    }

    [Test]
    public void ApplyDvergrCostReduction_ZeroCost_ReturnsZero()
    {
        // Arrange & Act
        var reduced = _service.ApplyDvergrCostReduction(0);

        // Assert
        reduced.Should().Be(0);
    }

    [Test]
    public void ApplyDvergrTimeReduction_ReducesByTwentyPercent()
    {
        // Arrange & Act
        var reduced = _service.ApplyDvergrTimeReduction(60);

        // Assert — 20% of 60 = 12 reduction
        reduced.Should().Be(48);
    }

    [Test]
    public void ApplyDvergrTimeReduction_ZeroTime_ReturnsZero()
    {
        // Arrange & Act
        var reduced = _service.ApplyDvergrTimeReduction(0);

        // Assert
        reduced.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Prerequisite Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(10).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(7).Should().BeFalse();
        _service.CanUnlockTier2(0).Should().BeFalse();
    }

    [Test]
    public void CalculatePPInvested_WithTier1Abilities_ReturnsZero()
    {
        // Arrange
        var abilities = new List<RunasmidrAbilityId>
        {
            RunasmidrAbilityId.InscribeRune,
            RunasmidrAbilityId.ReadTheMarks,
            RunasmidrAbilityId.RunestoneWard
        };

        // Act
        var total = _service.CalculatePPInvested(abilities);

        // Assert — Tier 1 abilities are free
        total.Should().Be(0);
    }

    [Test]
    public void CalculatePPInvested_WithTier2Abilities_ReturnsTwelve()
    {
        // Arrange
        var abilities = new List<RunasmidrAbilityId>
        {
            RunasmidrAbilityId.InscribeRune,
            RunasmidrAbilityId.ReadTheMarks,
            RunasmidrAbilityId.RunestoneWard,
            RunasmidrAbilityId.EmpoweredInscription,
            RunasmidrAbilityId.RunicTrap,
            RunasmidrAbilityId.DvergrTechniques
        };

        // Act
        var total = _service.CalculatePPInvested(abilities);

        // Assert — 3 × 0 (Tier 1) + 3 × 4 (Tier 2) = 12
        total.Should().Be(12);
    }

    [Test]
    public void GetAbilityPPCost_Tier1_ReturnsZero()
    {
        // Act & Assert
        RunasmidrTier2AbilityService.GetAbilityPPCost(
            RunasmidrAbilityId.InscribeRune).Should().Be(0);
    }

    [Test]
    public void GetAbilityPPCost_Tier2_ReturnsFour()
    {
        // Act & Assert
        RunasmidrTier2AbilityService.GetAbilityPPCost(
            RunasmidrAbilityId.EmpoweredInscription).Should().Be(4);
        RunasmidrTier2AbilityService.GetAbilityPPCost(
            RunasmidrAbilityId.RunicTrap).Should().Be(4);
        RunasmidrTier2AbilityService.GetAbilityPPCost(
            RunasmidrAbilityId.DvergrTechniques).Should().Be(4);
    }
}
