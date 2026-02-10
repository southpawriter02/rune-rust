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
/// Unit tests for <see cref="RunasmidrAbilityService"/>.
/// Validates Tier 1 and Tier 2 ability execution, prerequisite validation, PP tracking,
/// and tier unlock checks.
/// </summary>
[TestFixture]
public class RunasmidrAbilityServiceTests
{
    private Mock<IRuneChargeService> _mockRuneChargeService = null!;
    private RunasmidrAbilityService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRuneChargeService = new Mock<IRuneChargeService>();
        _service = new RunasmidrAbilityService(
            _mockRuneChargeService.Object,
            Mock.Of<ILogger<RunasmidrAbilityService>>());
    }

    /// <summary>
    /// Creates a Rúnasmiðr player with the specified abilities unlocked.
    /// </summary>
    private static Player CreateRunasmidr(params RunasmidrAbilityId[] abilities)
    {
        var player = new Player("Test Rúnasmiðr");
        player.SetSpecialization("runasmidr");
        player.InitializeRuneCharges();
        player.CurrentAP = 10;
        foreach (var ability in abilities)
        {
            player.UnlockRunasmidrAbility(ability);
        }
        return player;
    }

    // ===== ExecuteInscribeRune Tests =====

    [Test]
    public void ExecuteInscribeRune_OnWeapon_AddsDamageBonus()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 1))
            .Returns(true);

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: true);

        // Assert
        rune.Should().NotBeNull();
        rune!.RuneType.Should().Be(RuneType.Enhancement);
        rune.EnhancementBonus.Should().Be(InscribedRune.Tier1WeaponDamageBonus);
        rune.Duration.Should().Be(InscribedRune.DefaultDuration);
        rune.TargetItemId.Should().Be(itemId);
        player.CurrentAP.Should().Be(7); // 10 - 3
        player.ActiveRunes.Should().ContainSingle();
        _mockRuneChargeService.Verify(s => s.SpendCharges(player, 1), Times.Once);
    }

    [Test]
    public void ExecuteInscribeRune_OnArmor_AddsDefenseBonus()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 1))
            .Returns(true);

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: false);

        // Assert
        rune.Should().NotBeNull();
        rune!.RuneType.Should().Be(RuneType.Protection);
        rune.EnhancementBonus.Should().Be(InscribedRune.Tier1ArmorDefenseBonus);
    }

    [Test]
    public void ExecuteInscribeRune_InsufficientCharges_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(false);
        _mockRuneChargeService
            .Setup(s => s.GetCurrentValue(player))
            .Returns(0);

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: true);

        // Assert
        rune.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteInscribeRune_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);
        player.CurrentAP = 2; // Need 3
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: true);

        // Assert
        rune.Should().BeNull();
    }

    [Test]
    public void ExecuteInscribeRune_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(); // No abilities unlocked
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: true);

        // Assert
        rune.Should().BeNull();
    }

    [Test]
    public void ExecuteInscribeRune_NonRunasmidr_ReturnsNull()
    {
        // Arrange
        var player = new Player("Warrior");
        player.SetSpecialization("berserkr");
        player.CurrentAP = 10;
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteInscribeRune(player, itemId, isWeapon: true);

        // Assert
        rune.Should().BeNull();
    }

    // ===== ExecuteRunestoneWard Tests =====

    [Test]
    public void ExecuteRunestoneWard_CreatesWard_WithAbsorption()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunestoneWard);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 1))
            .Returns(true);

        // Act
        var ward = _service.ExecuteRunestoneWard(player);

        // Assert
        ward.Should().NotBeNull();
        ward!.DamageAbsorption.Should().Be(RunestoneWard.DefaultAbsorption);
        ward.RemainingAbsorption.Should().Be(RunestoneWard.DefaultAbsorption);
        ward.OwnerId.Should().Be(player.Id);
        player.CurrentAP.Should().Be(8); // 10 - 2
        player.ActiveWard.Should().NotBeNull();
        _mockRuneChargeService.Verify(s => s.SpendCharges(player, 1), Times.Once);
    }

    [Test]
    public void ExecuteRunestoneWard_ReplacesExistingWard()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunestoneWard);
        var existingWard = RunestoneWard.Create(player.Id);
        player.SetActiveWard(existingWard);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 1))
            .Returns(true);

        // Act
        var newWard = _service.ExecuteRunestoneWard(player);

        // Assert
        newWard.Should().NotBeNull();
        player.ActiveWard!.WardId.Should().Be(newWard!.WardId);
        player.ActiveWard.WardId.Should().NotBe(existingWard.WardId);
    }

    [Test]
    public void ExecuteRunestoneWard_InsufficientCharges_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunestoneWard);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 1))
            .Returns(false);
        _mockRuneChargeService
            .Setup(s => s.GetCurrentValue(player))
            .Returns(0);

        // Act
        var ward = _service.ExecuteRunestoneWard(player);

        // Assert
        ward.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    // ===== ExecuteEmpoweredInscription Tests (v0.20.2b) =====

    [Test]
    public void ExecuteEmpoweredInscription_ValidElement_CreatesRune()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.EmpoweredInscription);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 2))
            .Returns(true);

        // Act
        var rune = _service.ExecuteEmpoweredInscription(player, itemId, "fire");

        // Assert
        rune.Should().NotBeNull();
        rune!.TargetItemId.Should().Be(itemId);
        rune.ElementalTypeId.Should().Be("fire");
        rune.BonusDice.Should().Be("1d6");
        rune.Duration.Should().Be(EmpoweredRune.DefaultDuration);
        player.CurrentAP.Should().Be(6); // 10 - 4
        player.EmpoweredRunes.Should().ContainSingle();
        _mockRuneChargeService.Verify(s => s.SpendCharges(player, 2), Times.Once);
    }

    [Test]
    public void ExecuteEmpoweredInscription_InsufficientCharges_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.EmpoweredInscription);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(false);
        _mockRuneChargeService
            .Setup(s => s.GetCurrentValue(player))
            .Returns(1);

        // Act
        var rune = _service.ExecuteEmpoweredInscription(player, itemId, "fire");

        // Assert
        rune.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
        player.EmpoweredRunes.Should().BeEmpty();
    }

    [Test]
    public void ExecuteEmpoweredInscription_InsufficientAP_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.EmpoweredInscription);
        player.CurrentAP = 3; // Need 4
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteEmpoweredInscription(player, itemId, "fire");

        // Assert
        rune.Should().BeNull();
    }

    [Test]
    public void ExecuteEmpoweredInscription_InvalidElement_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.EmpoweredInscription);
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteEmpoweredInscription(player, itemId, "poison");

        // Assert
        rune.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteEmpoweredInscription_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange — Only Tier 1 unlocked, not EmpoweredInscription
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);
        var itemId = Guid.NewGuid();

        // Act
        var rune = _service.ExecuteEmpoweredInscription(player, itemId, "fire");

        // Assert
        rune.Should().BeNull();
    }

    [Test]
    public void ExecuteEmpoweredInscription_ReplacesExistingOnSameWeapon()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.EmpoweredInscription);
        var itemId = Guid.NewGuid();

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 2))
            .Returns(true);

        // First inscription
        _service.ExecuteEmpoweredInscription(player, itemId, "fire");
        player.CurrentAP = 10; // Reset AP for second test

        // Act — second inscription on same weapon
        var secondRune = _service.ExecuteEmpoweredInscription(player, itemId, "cold");

        // Assert
        secondRune.Should().NotBeNull();
        secondRune!.ElementalTypeId.Should().Be("cold");
        player.EmpoweredRunes.Should().ContainSingle(); // Only one on this weapon
    }

    // ===== ExecuteRunicTrap Tests (v0.20.2b) =====

    [Test]
    public void ExecuteRunicTrap_ValidPosition_CreatesTrap()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunicTrap);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 2))
            .Returns(true);

        // Act
        var trap = _service.ExecuteRunicTrap(player, 5, 10);

        // Assert
        trap.Should().NotBeNull();
        trap!.OwnerId.Should().Be(player.Id);
        trap.PositionX.Should().Be(5);
        trap.PositionY.Should().Be(10);
        trap.Damage.Should().Be("3d6");
        trap.DetectionDc.Should().Be(14);
        trap.IsTriggered.Should().BeFalse();
        player.CurrentAP.Should().Be(7); // 10 - 3
        player.GetActiveTraps().Should().ContainSingle();
        _mockRuneChargeService.Verify(s => s.SpendCharges(player, 2), Times.Once);
    }

    [Test]
    public void ExecuteRunicTrap_MaxTrapsReached_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunicTrap);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(true);
        _mockRuneChargeService
            .Setup(s => s.SpendCharges(player, 2))
            .Returns(true);

        // Place 3 traps
        _service.ExecuteRunicTrap(player, 1, 1);
        player.CurrentAP = 10; // Reset
        _service.ExecuteRunicTrap(player, 2, 2);
        player.CurrentAP = 10; // Reset
        _service.ExecuteRunicTrap(player, 3, 3);
        player.CurrentAP = 10; // Reset

        // Act — attempt 4th trap
        var trap = _service.ExecuteRunicTrap(player, 4, 4);

        // Assert
        trap.Should().BeNull();
        player.GetActiveTraps().Should().HaveCount(3);
    }

    [Test]
    public void ExecuteRunicTrap_InsufficientCharges_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.RunicTrap);

        _mockRuneChargeService
            .Setup(s => s.CanSpend(player, 2))
            .Returns(false);
        _mockRuneChargeService
            .Setup(s => s.GetCurrentValue(player))
            .Returns(1);

        // Act
        var trap = _service.ExecuteRunicTrap(player, 5, 10);

        // Assert
        trap.Should().BeNull();
        player.CurrentAP.Should().Be(10); // Unchanged
    }

    [Test]
    public void ExecuteRunicTrap_AbilityNotUnlocked_ReturnsNull()
    {
        // Arrange
        var player = CreateRunasmidr(RunasmidrAbilityId.InscribeRune);

        // Act
        var trap = _service.ExecuteRunicTrap(player, 5, 10);

        // Assert
        trap.Should().BeNull();
    }

    // ===== PP Validation Tests =====

    [Test]
    public void GetPPInvested_WithTier1Only_ReturnsZero()
    {
        // Arrange
        var player = CreateRunasmidr(
            RunasmidrAbilityId.InscribeRune,      // T1: 0 PP
            RunasmidrAbilityId.ReadTheMarks,       // T1: 0 PP
            RunasmidrAbilityId.RunestoneWard);     // T1: 0 PP

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(0);
    }

    [Test]
    public void GetPPInvested_CalculatesCorrectly()
    {
        // Arrange
        var player = CreateRunasmidr(
            RunasmidrAbilityId.InscribeRune,           // T1: 0 PP
            RunasmidrAbilityId.ReadTheMarks,            // T1: 0 PP
            RunasmidrAbilityId.EmpoweredInscription,    // T2: 4 PP
            RunasmidrAbilityId.RunicTrap,               // T2: 4 PP
            RunasmidrAbilityId.MasterScrivener);         // T3: 5 PP
        // Total: 0 + 0 + 4 + 4 + 5 = 13 PP

        // Act
        var pp = _service.GetPPInvested(player);

        // Assert
        pp.Should().Be(13);
    }

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange — need 8+ PP, unlock 2 T2 abilities (4+4 = 8 PP)
        var player = CreateRunasmidr(
            RunasmidrAbilityId.EmpoweredInscription,    // T2: 4 PP
            RunasmidrAbilityId.RunicTrap);               // T2: 4 PP

        // Act
        var result = _service.CanUnlockTier2(player);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange — only T1 unlocked (0 PP)
        var player = CreateRunasmidr(
            RunasmidrAbilityId.InscribeRune);

        // Act
        var result = _service.CanUnlockTier2(player);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void CanUnlockTier2_NonRunasmidr_ReturnsFalse()
    {
        // Arrange
        var player = new Player("Warrior");
        player.SetSpecialization("berserkr");

        // Act
        var result = _service.CanUnlockTier2(player);

        // Assert
        result.Should().BeFalse();
    }
}
