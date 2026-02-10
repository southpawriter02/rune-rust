// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrTier2AbilityServiceTests.cs
// Unit tests for Myrk-gengr Tier 2 abilities:
// Umbral Strike, Shadow Clone, and Void Touched.
// Version: 0.20.4b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class MyrkgengrTier2AbilityServiceTests
{
    private MyrkgengrTier2AbilityService _service = null!;
    private Mock<ILogger<MyrkgengrTier2AbilityService>> _mockLogger = null!;
    private Mock<IShadowCorruptionService> _mockCorruptionService = null!;
    private Random _seededRandom = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MyrkgengrTier2AbilityService>>();
        _mockCorruptionService = new Mock<IShadowCorruptionService>();

        // Default: corruption not triggered
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(
                It.IsAny<MyrkgengrAbilityId>(),
                It.IsAny<LightLevelType>(),
                It.IsAny<bool>()))
            .Returns(CorruptionRiskResult.CreateSafe("test-ability", LightLevelType.Darkness));

        _seededRandom = new Random(42); // Seeded for deterministic rolls
        _service = new MyrkgengrTier2AbilityService(
            _mockLogger.Object,
            _mockCorruptionService.Object,
            _seededRandom);
    }

    // ═══════ Umbral Strike Tests ═══════

    [Test]
    public void ExecuteUmbralStrike_InShadowWithSufficientEssence_ReturnsHitOrMiss()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);

        // Act
        var result = _service.ExecuteUmbralStrike(
            resource, LightLevelType.Darkness,
            attackModifier: 5, targetDefense: 10,
            weaponDamage: 8);

        // Assert
        result.FailureReason.Should().BeNull();
        result.UpdatedResource.CurrentEssence.Should().Be(35); // 50 - 15
    }

    [Test]
    public void ExecuteUmbralStrike_InBrightLight_ReturnsFailure()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);

        // Act
        var result = _service.ExecuteUmbralStrike(
            resource, LightLevelType.BrightLight,
            attackModifier: 5, targetDefense: 10,
            weaponDamage: 8);

        // Assert
        result.IsHit.Should().BeFalse();
        result.FailureReason.Should().Contain("shadow");
        result.UpdatedResource.CurrentEssence.Should().Be(50); // Unchanged
    }

    [Test]
    public void ExecuteUmbralStrike_InsufficientEssence_ReturnsFailure()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);
        // Drain essence to below cost
        var (_, drained) = resource.TrySpend(40); // 50 - 40 = 10

        // Act
        var result = _service.ExecuteUmbralStrike(
            drained, LightLevelType.Darkness,
            attackModifier: 5, targetDefense: 10,
            weaponDamage: 8);

        // Assert
        result.IsHit.Should().BeFalse();
        result.FailureReason.Should().Contain("Essence");
    }

    [Test]
    public void ExecuteUmbralStrike_WithCloneConsumption_GrantsBonus()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);

        // Act
        var result = _service.ExecuteUmbralStrike(
            resource, LightLevelType.Darkness,
            attackModifier: 5, targetDefense: 5, // Low defense to likely hit
            weaponDamage: 8,
            consumeClone: true);

        // Assert
        result.CloneConsumed.Should().BeTrue();
        result.FailureReason.Should().BeNull();
    }

    [Test]
    public void CanExecuteUmbralStrike_InShadowWithEssence_ReturnsTrue()
    {
        // Arrange & Act
        var resource = ShadowEssenceResource.Create(50);
        var canExecute = _service.CanExecuteUmbralStrike(resource, LightLevelType.DimLight);

        // Assert
        canExecute.Should().BeTrue();
    }

    [Test]
    public void CanExecuteUmbralStrike_InBrightLight_ReturnsFalse()
    {
        // Arrange & Act
        var resource = ShadowEssenceResource.Create(50);
        var canExecute = _service.CanExecuteUmbralStrike(resource, LightLevelType.NormalLight);

        // Assert
        canExecute.Should().BeFalse();
    }

    // ═══════ Shadow Clone Tests ═══════

    [Test]
    public void CreateShadowClone_WithSufficientEssence_ReturnsActiveClone()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);
        var ownerId = Guid.NewGuid();

        // Act
        var (clone, updatedResource, corruption, failureReason) =
            _service.CreateShadowClone(
                resource, ownerId, 10, 15,
                CloneBehavior.Mirror, LightLevelType.Darkness,
                activeCloneCount: 0);

        // Assert
        clone.Should().NotBeNull();
        clone!.OwnerId.Should().Be(ownerId);
        clone.X.Should().Be(10);
        clone.Y.Should().Be(15);
        clone.Behavior.Should().Be(CloneBehavior.Mirror);
        clone.IsActive().Should().BeTrue();
        clone.HitPoints.Should().Be(1);
        clone.DetectionDC.Should().Be(14);
        updatedResource.CurrentEssence.Should().Be(30); // 50 - 20
        failureReason.Should().BeNull();
    }

    [Test]
    public void CreateShadowClone_MaxClonesReached_ReturnsFailure()
    {
        // Arrange
        var resource = ShadowEssenceResource.Create(50);
        var ownerId = Guid.NewGuid();

        // Act
        var (clone, updatedResource, _, failureReason) =
            _service.CreateShadowClone(
                resource, ownerId, 10, 15,
                CloneBehavior.Decoy, LightLevelType.Darkness,
                activeCloneCount: 2); // Already at max

        // Assert
        clone.Should().BeNull();
        failureReason.Should().Contain("Maximum");
        updatedResource.CurrentEssence.Should().Be(50); // Unchanged
    }

    [Test]
    public void DestroyShadowClone_ActiveClone_ReturnsDestroyedClone()
    {
        // Arrange
        var clone = ShadowClone.Create(Guid.NewGuid(), 5, 5, CloneBehavior.Static);

        // Act
        var destroyed = _service.DestroyShadowClone(clone);

        // Assert
        destroyed.IsActive().Should().BeFalse();
        destroyed.WasDestroyed.Should().BeTrue();
        destroyed.HitPoints.Should().Be(0);
    }

    [Test]
    public void ConsumeShadowClone_ActiveClone_ReturnsConsumedClone()
    {
        // Arrange
        var clone = ShadowClone.Create(Guid.NewGuid(), 5, 5, CloneBehavior.Independent);

        // Act
        var consumed = _service.ConsumeShadowClone(clone);

        // Assert
        consumed.IsActive().Should().BeFalse();
        consumed.WasConsumed.Should().BeTrue();
        consumed.HitPoints.Should().Be(0);
    }

    // ═══════ Void Touched Tests ═══════

    [Test]
    public void ApplyVoidTouched_HalvesAethericDamage()
    {
        // Arrange & Act
        var result = _service.ApplyVoidTouched(25);

        // Assert
        result.Should().Be(12); // floor(25 / 2) = 12
    }

    [Test]
    public void ApplyVoidTouched_OddDamage_FloorsResult()
    {
        // Arrange & Act
        var result = _service.ApplyVoidTouched(7);

        // Assert
        result.Should().Be(3); // floor(7 / 2) = 3
    }

    [Test]
    public void ApplyVoidTouched_ZeroDamage_ReturnsZero()
    {
        // Arrange & Act
        var result = _service.ApplyVoidTouched(0);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void GrantsAethericSaveAdvantage_AlwaysReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.GrantsAethericSaveAdvantage().Should().BeTrue();
    }

    // ═══════ Prerequisite Tests ═══════

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(12).Should().BeTrue();
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
        var abilities = new List<MyrkgengrAbilityId>
        {
            MyrkgengrAbilityId.ShadowStep,
            MyrkgengrAbilityId.CloakOfNight,
            MyrkgengrAbilityId.DarkAdapted
        };

        // Act
        var total = _service.CalculatePPInvested(abilities);

        // Assert
        total.Should().Be(0); // Tier 1 are free
    }

    [Test]
    public void CalculatePPInvested_WithMixedTiers_ReturnsTotalCost()
    {
        // Arrange
        var abilities = new List<MyrkgengrAbilityId>
        {
            MyrkgengrAbilityId.ShadowStep,     // Tier 1: 0 PP
            MyrkgengrAbilityId.CloakOfNight,   // Tier 1: 0 PP
            MyrkgengrAbilityId.DarkAdapted,    // Tier 1: 0 PP
            MyrkgengrAbilityId.UmbralStrike,   // Tier 2: 4 PP
            MyrkgengrAbilityId.ShadowClone     // Tier 2: 4 PP
        };

        // Act
        var total = _service.CalculatePPInvested(abilities);

        // Assert
        total.Should().Be(8); // 0+0+0+4+4
    }

    [Test]
    public void GetAbilityPPCost_ReturnsCorrectCostPerTier()
    {
        // Tier 1: Free
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.ShadowStep).Should().Be(0);
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.CloakOfNight).Should().Be(0);
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.DarkAdapted).Should().Be(0);

        // Tier 2: 4 PP each
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.UmbralStrike).Should().Be(4);
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.ShadowClone).Should().Be(4);
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.VoidTouched).Should().Be(4);

        // Tier 3: 5 PP each
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.MergeWithDarkness).Should().Be(5);
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.ShadowSnare).Should().Be(5);

        // Capstone: 6 PP
        MyrkgengrTier2AbilityService.GetAbilityPPCost(MyrkgengrAbilityId.Eclipse).Should().Be(6);
    }
}
