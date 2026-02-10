// ═══════════════════════════════════════════════════════════════════════════════
// MyrkgengrTier3AbilityServiceTests.cs
// Unit tests for the MyrkgengrTier3AbilityService, validating Merge with
// Darkness, Shadow Snare, Eclipse, and PP prerequisite logic.
// Version: 0.20.4c
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
/// Tests for <see cref="MyrkgengrTier3AbilityService"/>.
/// </summary>
[TestFixture]
public class MyrkgengrTier3AbilityServiceTests
{
    private MyrkgengrTier3AbilityService _service = null!;
    private Mock<ILogger<MyrkgengrTier3AbilityService>> _mockLogger = null!;
    private Mock<IShadowCorruptionService> _mockCorruptionService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MyrkgengrTier3AbilityService>>();
        _mockCorruptionService = new Mock<IShadowCorruptionService>();

        // Default: corruption service returns safe results
        _mockCorruptionService
            .Setup(s => s.EvaluateRisk(
                It.IsAny<MyrkgengrAbilityId>(),
                It.IsAny<LightLevelType>(),
                It.IsAny<bool>()))
            .Returns((MyrkgengrAbilityId ability, LightLevelType light, bool _) =>
                CorruptionRiskResult.CreateSafe(ability.ToString().ToLowerInvariant(), light));

        _service = new MyrkgengrTier3AbilityService(
            _mockLogger.Object, _mockCorruptionService.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Merge with Darkness Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanExecuteMergeWithDarkness_SufficientEssenceInDarkness_ReturnsTrue()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        _service.CanExecuteMergeWithDarkness(resource, LightLevelType.Darkness)
            .Should().BeTrue();
    }

    [Test]
    public void CanExecuteMergeWithDarkness_InBrightLight_ReturnsFalse()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        _service.CanExecuteMergeWithDarkness(resource, LightLevelType.BrightLight)
            .Should().BeFalse();
    }

    [Test]
    public void CanExecuteMergeWithDarkness_InsufficientEssence_ReturnsFalse()
    {
        // Arrange — deplete below 25
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(30); // 20 remaining, need 25

        // Act & Assert
        _service.CanExecuteMergeWithDarkness(depleted, LightLevelType.Darkness)
            .Should().BeFalse();
    }

    [Test]
    public void ExecuteMergeWithDarkness_ValidConditions_ReturnsActiveState()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (state, updated, corruption, error) = _service.ExecuteMergeWithDarkness(
            resource, LightLevelType.Darkness);

        // Assert
        state.Should().NotBeNull();
        state!.IsActive.Should().BeTrue();
        state.RemainingTurns.Should().Be(1);
        state.ExtensionCount.Should().Be(0);
        updated.CurrentEssence.Should().Be(25); // 50 - 25
        error.Should().BeNull();
    }

    [Test]
    public void ExecuteMergeWithDarkness_InBrightLight_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (state, updated, _, error) = _service.ExecuteMergeWithDarkness(
            resource, LightLevelType.BrightLight);

        // Assert
        state.Should().BeNull();
        updated.CurrentEssence.Should().Be(50, "essence should not be spent");
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void ExecuteMergeWithDarkness_InsufficientEssence_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(30); // 20 remaining

        // Act
        var (state, _, _, error) = _service.ExecuteMergeWithDarkness(
            depleted, LightLevelType.Darkness);

        // Assert
        state.Should().BeNull();
        error.Should().Contain("Insufficient");
    }

    [Test]
    public void ExecuteMergeWithDarkness_EvaluatesCorruption()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (_, _, corruption, _) = _service.ExecuteMergeWithDarkness(
            resource, LightLevelType.Darkness);

        // Assert
        corruption.Should().NotBeNull();
        _mockCorruptionService.Verify(s => s.EvaluateRisk(
            MyrkgengrAbilityId.MergeWithDarkness,
            LightLevelType.Darkness,
            false), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Extend Incorporeal Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ExtendIncorporealState_ValidState_AddsOneTurn()
    {
        // Arrange
        var state = IncorporealState.Create();
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (updated, updatedResource, corruptionGain) =
            _service.ExtendIncorporealState(state, resource);

        // Assert
        updated.RemainingTurns.Should().Be(2);
        updated.ExtensionCount.Should().Be(1);
        corruptionGain.Should().Be(1);
        updatedResource.CurrentEssence.Should().Be(35); // 50 - 15
    }

    [Test]
    public void ExtendIncorporealState_AlreadyMaxExtensions_Throws()
    {
        // Arrange — extend twice to max
        var state = IncorporealState.Create();
        var resource = ShadowEssenceResource.CreateDefault();
        state = state.Extend(1).Extend(1); // 2 extensions = max

        // Act
        var act = () => _service.ExtendIncorporealState(state, resource);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ExtendIncorporealState_InsufficientEssence_Throws()
    {
        // Arrange
        var state = IncorporealState.Create();
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(40); // 10 remaining, need 15

        // Act
        var act = () => _service.ExtendIncorporealState(state, depleted);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient*");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Shadow Snare Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanExecuteShadowSnare_SufficientEssence_ReturnsTrue()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        _service.CanExecuteShadowSnare(resource).Should().BeTrue();
    }

    [Test]
    public void CanExecuteShadowSnare_InsufficientEssence_ReturnsFalse()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(35); // 15 remaining, need 20

        // Act & Assert
        _service.CanExecuteShadowSnare(depleted).Should().BeFalse();
    }

    [Test]
    public void ExecuteShadowSnare_ValidConditions_ReturnsActiveSnare()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var casterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var (snare, updated, corruption, error) = _service.ExecuteShadowSnare(
            resource, casterId, targetId, LightLevelType.DimLight);

        // Assert
        snare.Should().NotBeNull();
        snare!.IsActive.Should().BeTrue();
        snare.RemainingTurns.Should().Be(2);
        snare.SaveDC.Should().Be(14);
        snare.CasterId.Should().Be(casterId);
        snare.TargetId.Should().Be(targetId);
        updated.CurrentEssence.Should().Be(30); // 50 - 20
        error.Should().BeNull();
    }

    [Test]
    public void ExecuteShadowSnare_InsufficientEssence_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(35);

        // Act
        var (snare, _, _, error) = _service.ExecuteShadowSnare(
            depleted, Guid.NewGuid(), Guid.NewGuid(), LightLevelType.DimLight);

        // Assert
        snare.Should().BeNull();
        error.Should().Contain("Insufficient");
    }

    [Test]
    public void ExecuteShadowSnare_CoherentTarget_EvaluatesCorruptionWithFlag()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        _service.ExecuteShadowSnare(
            resource, Guid.NewGuid(), Guid.NewGuid(),
            LightLevelType.DimLight, targetIsCoherent: true);

        // Assert — corruption evaluated with targetIsCoherent=true
        _mockCorruptionService.Verify(s => s.EvaluateRisk(
            MyrkgengrAbilityId.ShadowSnare,
            LightLevelType.DimLight,
            true), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Snare Escape Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void AttemptSnareEscape_RollMeetsDC_Escapes()
    {
        // Arrange
        var snare = ShadowSnareEffect.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var (updated, escaped) = _service.AttemptSnareEscape(snare, saveRoll: 14);

        // Assert
        escaped.Should().BeTrue();
        updated.IsActive.Should().BeFalse();
        updated.BrokeCondition.Should().Be(BreakCondition.SaveSucceeded);
    }

    [Test]
    public void AttemptSnareEscape_RollBelowDC_StaysSnared()
    {
        // Arrange
        var snare = ShadowSnareEffect.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var (updated, escaped) = _service.AttemptSnareEscape(snare, saveRoll: 13);

        // Assert
        escaped.Should().BeFalse();
        updated.IsActive.Should().BeTrue();
        updated.EscapeAttempts.Should().Be(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Eclipse Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanExecuteEclipse_SufficientEssenceFirstUse_ReturnsTrue()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        _service.CanExecuteEclipse(resource, hasUsedEclipseThisCombat: false)
            .Should().BeTrue();
    }

    [Test]
    public void CanExecuteEclipse_AlreadyUsedThisCombat_ReturnsFalse()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act & Assert
        _service.CanExecuteEclipse(resource, hasUsedEclipseThisCombat: true)
            .Should().BeFalse();
    }

    [Test]
    public void CanExecuteEclipse_InsufficientEssence_ReturnsFalse()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(15); // 35 remaining, need 40

        // Act & Assert
        _service.CanExecuteEclipse(depleted, hasUsedEclipseThisCombat: false)
            .Should().BeFalse();
    }

    [Test]
    public void ExecuteEclipse_ValidConditions_ReturnsActiveZone()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var casterId = Guid.NewGuid();

        // Act
        var (zone, updated, corruption, error) = _service.ExecuteEclipse(
            resource, casterId, centerX: 10, centerY: 15);

        // Assert
        zone.Should().NotBeNull();
        zone!.IsActive.Should().BeTrue();
        zone.Radius.Should().Be(8);
        zone.RemainingTurns.Should().Be(3);
        zone.CasterId.Should().Be(casterId);
        zone.CenterX.Should().Be(10);
        zone.CenterY.Should().Be(15);
        zone.CorruptionApplied.Should().Be(2);
        updated.CurrentEssence.Should().Be(10); // 50 - 40
        error.Should().BeNull();
    }

    [Test]
    public void ExecuteEclipse_InsufficientEssence_Fails()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, depleted) = resource.TrySpend(15);

        // Act
        var (zone, _, _, error) = _service.ExecuteEclipse(
            depleted, Guid.NewGuid(), 0, 0);

        // Assert
        zone.Should().BeNull();
        error.Should().Contain("Insufficient");
    }

    [Test]
    public void ExecuteEclipse_EvaluatesCorruptionInDarkness()
    {
        // Arrange
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        _service.ExecuteEclipse(resource, Guid.NewGuid(), 0, 0);

        // Assert — always evaluates in Darkness
        _mockCorruptionService.Verify(s => s.EvaluateRisk(
            MyrkgengrAbilityId.Eclipse,
            LightLevelType.Darkness,
            false), Times.Once);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Eclipse Zone Tick Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void TickEclipseZone_ActiveZone_RegeneratesEssenceAndTicksDown()
    {
        // Arrange
        var zone = EclipseZone.Create(Guid.NewGuid(), 10, 15);
        var resource = ShadowEssenceResource.CreateDefault();
        var (_, lowResource) = resource.TrySpend(40); // 10 remaining

        // Act
        var (updatedZone, updatedResource) = _service.TickEclipseZone(zone, lowResource);

        // Assert
        updatedZone.RemainingTurns.Should().Be(2); // 3 - 1
        updatedZone.IsActive.Should().BeTrue();
        updatedResource.CurrentEssence.Should().Be(20); // 10 + 10 regen
    }

    [Test]
    public void TickEclipseZone_LastTurn_ExpiresZone()
    {
        // Arrange
        var zone = EclipseZone.Create(Guid.NewGuid(), 10, 15);
        zone = zone.TickDown().TickDown(); // 1 turn remaining
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (updatedZone, _) = _service.TickEclipseZone(zone, resource);

        // Assert
        updatedZone.IsActive.Should().BeFalse();
        updatedZone.RemainingTurns.Should().Be(0);
    }

    [Test]
    public void TickEclipseZone_InactiveZone_NoOp()
    {
        // Arrange
        var zone = EclipseZone.Create(Guid.NewGuid(), 10, 15);
        zone = zone.TickDown().TickDown().TickDown(); // expired
        var resource = ShadowEssenceResource.CreateDefault();

        // Act
        var (_, updatedResource) = _service.TickEclipseZone(zone, resource);

        // Assert — no regen when zone is inactive
        updatedResource.CurrentEssence.Should().Be(50);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PP / Unlock Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanUnlockTier3_WithSufficientPP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockTier3(16).Should().BeTrue();
        _service.CanUnlockTier3(20).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier3_WithInsufficientPP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockTier3(15).Should().BeFalse();
        _service.CanUnlockTier3(0).Should().BeFalse();
    }

    [Test]
    public void CanUnlockCapstone_WithSufficientPP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockCapstone(24).Should().BeTrue();
        _service.CanUnlockCapstone(30).Should().BeTrue();
    }

    [Test]
    public void CanUnlockCapstone_WithInsufficientPP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockCapstone(23).Should().BeFalse();
        _service.CanUnlockCapstone(0).Should().BeFalse();
    }
}
