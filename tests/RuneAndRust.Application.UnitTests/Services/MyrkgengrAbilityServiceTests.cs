// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrAbilityServiceTests.cs
// Unit tests for the MyrkgengrAbilityService, validating Shadow Step,
// Cloak of Night, Dark-Adapted, and PP prerequisite logic.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="MyrkgengrAbilityService"/>.
/// </summary>
[TestFixture]
public class MyrkgengrAbilityServiceTests
{
    private MyrkgengrAbilityService _service = null!;
    private Mock<ILogger<MyrkgengrAbilityService>> _mockLogger = null!;
    private Mock<IShadowCorruptionService> _mockCorruptionService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MyrkgengrAbilityService>>();
        _mockCorruptionService = new Mock<IShadowCorruptionService>();

        // Default: corruption service returns safe results
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(
                It.IsAny<MyrkgengrAbilityId>(),
                It.IsAny<LightLevelType>(),
                It.IsAny<bool>()))
            .Returns((MyrkgengrAbilityId ability, LightLevelType light, bool _) =>
                CorruptionRiskResult.CreateSafe(ability.ToString().ToLowerInvariant(), light));

        _service = new MyrkgengrAbilityService(_mockLogger.Object, _mockCorruptionService.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shadow Step Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ExecuteShadowStep_ValidTarget_SucceedsAndSpendsEssence()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var origin = ShadowPosition.Create(5, 5, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(8, 8, LightLevelType.Darkness);

        // Act
        var (success, updated, corruption) = _service.ExecuteShadowStep(resource, origin, target);

        // Assert
        success.Should().BeTrue();
        updated.CurrentEssence.Should().Be(45); // 50 - 10 + 5 (Darkness arrival bonus)
    }

    [Test]
    public void ExecuteShadowStep_TargetInBrightLight_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var origin = ShadowPosition.Create(5, 5, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(8, 8, LightLevelType.BrightLight);

        // Act
        var (success, updated, _) = _service.ExecuteShadowStep(resource, origin, target);

        // Assert — target must be in shadow
        success.Should().BeFalse();
        updated.CurrentEssence.Should().Be(50, "essence should not be spent");
    }

    [Test]
    public void ExecuteShadowStep_TargetOutOfRange_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var origin = ShadowPosition.Create(0, 0, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(10, 10, LightLevelType.Darkness); // ~14 spaces

        // Act
        var (success, _, _) = _service.ExecuteShadowStep(resource, origin, target);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void ExecuteShadowStep_InsufficientEssence_Fails()
    {
        // Arrange — deplete to below cost
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(45); // 5 remaining, need 10
        var origin = ShadowPosition.Create(5, 5, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(8, 8, LightLevelType.Darkness);

        // Act
        var (success, _, _) = _service.ExecuteShadowStep(depleted, origin, target);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void ExecuteShadowStep_OccupiedTarget_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var origin = ShadowPosition.Create(5, 5, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(8, 8, LightLevelType.Darkness, isOccupied: true);

        // Act
        var (success, _, _) = _service.ExecuteShadowStep(resource, origin, target);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void CanExecuteShadowStep_ValidConditions_ReturnsTrue()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var origin = ShadowPosition.Create(5, 5, LightLevelType.NormalLight);
        var target = ShadowPosition.Create(8, 8, LightLevelType.Darkness);

        // Act & Assert
        _service.CanExecuteShadowStep(resource, origin, target).Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cloak of Night Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ActivateCloakOfNight_WithSufficientEssence_Succeeds()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (success, _) = _service.ActivateCloakOfNight(resource);

        // Assert
        success.Should().BeTrue();
    }

    [Test]
    public void ActivateCloakOfNight_InsufficientEssence_Fails()
    {
        // Arrange — deplete below maintenance cost (5)
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(47); // 3 remaining

        // Act
        var (success, _) = _service.ActivateCloakOfNight(depleted);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void MaintainCloakOfNight_SpendsFiveEssencePerTurn()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (stanceActive, updated, _) = _service.MaintainCloakOfNight(
            resource, LightLevelType.Darkness);

        // Assert
        stanceActive.Should().BeTrue();
        updated.CurrentEssence.Should().Be(45); // 50 - 5
    }

    [Test]
    public void MaintainCloakOfNight_InsufficientEssence_EndsStance()
    {
        // Arrange — exactly at maintenance cost
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(47); // 3 remaining, need 5

        // Act
        var (stanceActive, _, _) = _service.MaintainCloakOfNight(
            depleted, LightLevelType.Darkness);

        // Assert
        stanceActive.Should().BeFalse();
    }

    [Test]
    public void MaintainCloakOfNight_InBrightLight_TriggersCorruptionCheck()
    {
        // Arrange — set up corruption service to return triggered result in bright light
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(
                MyrkgengrAbilityId.CloakOfNight,
                LightLevelType.BrightLight,
                false))
            .Returns(CorruptionRiskResult.CreateTriggered(
                corruptionGained: 1,
                reason: "Cloak of Night in bright light",
                abilityUsed: "cloakofnight",
                lightCondition: LightLevelType.BrightLight));

        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (stanceActive, _, corruptionResult) = _service.MaintainCloakOfNight(
            resource, LightLevelType.BrightLight);

        // Assert
        stanceActive.Should().BeTrue();
        corruptionResult.Should().NotBeNull();
        corruptionResult!.RiskTriggered.Should().BeTrue();
        corruptionResult.CorruptionGained.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Stealth Modifier Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetCloakOfNightStealthModifier_ReturnsCorrectValues()
    {
        // Act & Assert
        _service.GetCloakOfNightStealthModifier(LightLevelType.Darkness).Should().Be(3);
        _service.GetCloakOfNightStealthModifier(LightLevelType.DimLight).Should().Be(3);
        _service.GetCloakOfNightStealthModifier(LightLevelType.NormalLight).Should().Be(1);
        _service.GetCloakOfNightStealthModifier(LightLevelType.BrightLight).Should().Be(-1);
        _service.GetCloakOfNightStealthModifier(LightLevelType.Sunlight).Should().Be(-1);
    }

    [Test]
    public void GrantsSilentMovement_OnlyInShadow()
    {
        // Act & Assert
        _service.GrantsSilentMovement(LightLevelType.Darkness).Should().BeTrue();
        _service.GrantsSilentMovement(LightLevelType.DimLight).Should().BeTrue();
        _service.GrantsSilentMovement(LightLevelType.NormalLight).Should().BeFalse();
        _service.GrantsSilentMovement(LightLevelType.BrightLight).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dark-Adapted Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetDarkAdaptedGeneration_InDarkness_ReturnsTwo()
    {
        // Act & Assert
        _service.GetDarkAdaptedGeneration(LightLevelType.Darkness).Should().Be(2);
    }

    [Test]
    public void GetDarkAdaptedGeneration_NotInDarkness_ReturnsZero()
    {
        // Act & Assert
        _service.GetDarkAdaptedGeneration(LightLevelType.DimLight).Should().Be(0);
        _service.GetDarkAdaptedGeneration(LightLevelType.NormalLight).Should().Be(0);
        _service.GetDarkAdaptedGeneration(LightLevelType.BrightLight).Should().Be(0);
    }

    [Test]
    public void RemovesDimLightPenalties_OnlyInDimLight()
    {
        // Act & Assert
        _service.RemovesDimLightPenalties(LightLevelType.DimLight).Should().BeTrue();
        _service.RemovesDimLightPenalties(LightLevelType.Darkness).Should().BeFalse();
        _service.RemovesDimLightPenalties(LightLevelType.NormalLight).Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PP Cost Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void GetAbilityPPCost_Tier1_ReturnsZero()
    {
        // Act & Assert
        _service.GetAbilityPPCost(MyrkgengrAbilityId.ShadowStep).Should().Be(0);
        _service.GetAbilityPPCost(MyrkgengrAbilityId.CloakOfNight).Should().Be(0);
        _service.GetAbilityPPCost(MyrkgengrAbilityId.DarkAdapted).Should().Be(0);
    }

    [Test]
    public void GetAbilityPPCost_Tier2_ReturnsFour()
    {
        // Act & Assert
        _service.GetAbilityPPCost(MyrkgengrAbilityId.UmbralStrike).Should().Be(4);
        _service.GetAbilityPPCost(MyrkgengrAbilityId.ShadowClone).Should().Be(4);
        _service.GetAbilityPPCost(MyrkgengrAbilityId.VoidTouched).Should().Be(4);
    }

    [Test]
    public void GetAbilityPPCost_Tier3_ReturnsFive()
    {
        // Act & Assert
        _service.GetAbilityPPCost(MyrkgengrAbilityId.MergeWithDarkness).Should().Be(5);
        _service.GetAbilityPPCost(MyrkgengrAbilityId.ShadowSnare).Should().Be(5);
    }

    [Test]
    public void GetAbilityPPCost_Capstone_ReturnsSix()
    {
        // Act & Assert
        _service.GetAbilityPPCost(MyrkgengrAbilityId.Eclipse).Should().Be(6);
    }

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(12).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_WithInsufficientPP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockTier2(7).Should().BeFalse();
        _service.CanUnlockTier2(0).Should().BeFalse();
    }

    [Test]
    public void CalculatePPInvested_Tier1Abilities_ReturnsZero()
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

        // Assert — Tier 1 abilities are free
        total.Should().Be(0);
    }
}
