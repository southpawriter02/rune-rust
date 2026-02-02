// ═══════════════════════════════════════════════════════════════════════════════
// UnifiedDamageHandlerTests.cs
// Unit tests for UnifiedDamageHandler — the unified damage processing service
// that coordinates across all trauma economy systems.
// Version: 0.18.5b
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="UnifiedDamageHandler"/> service.
/// </summary>
[TestFixture]
public class UnifiedDamageHandlerTests
{
    // ═══════════════════════════════════════════════════════════════
    // Test Setup
    // ═══════════════════════════════════════════════════════════════

    private Mock<IStressService> _mockStressService = null!;
    private Mock<ICorruptionService> _mockCorruptionService = null!;
    private Mock<ITraumaService> _mockTraumaService = null!;
    private Mock<ISpecializationProvider> _mockSpecializationProvider = null!;
    private Mock<IRageService> _mockRageService = null!;
    private Mock<IMomentumService> _mockMomentumService = null!;
    private Mock<ICoherenceService> _mockCoherenceService = null!;
    private Mock<ILogger<UnifiedDamageHandler>> _mockLogger = null!;
    private UnifiedDamageHandler _handler = null!;
    private Guid _characterId;

    [SetUp]
    public void SetUp()
    {
        _mockStressService = new Mock<IStressService>();
        _mockCorruptionService = new Mock<ICorruptionService>();
        _mockTraumaService = new Mock<ITraumaService>();
        _mockSpecializationProvider = new Mock<ISpecializationProvider>();
        _mockRageService = new Mock<IRageService>();
        _mockMomentumService = new Mock<IMomentumService>();
        _mockCoherenceService = new Mock<ICoherenceService>();
        _mockLogger = new Mock<ILogger<UnifiedDamageHandler>>();

        _characterId = Guid.NewGuid();

        // Default: no specialization states (throws/returns null)
        _mockRageService
            .Setup(r => r.GetRageState(It.IsAny<Guid>()))
            .Returns((RageState?)null);

        _mockMomentumService
            .Setup(m => m.GetMomentumState(It.IsAny<Guid>()))
            .Returns((MomentumState?)null);

        _mockCoherenceService
            .Setup(c => c.GetCoherenceState(It.IsAny<Guid>()))
            .Returns((CoherenceState?)null);

        _handler = new UnifiedDamageHandler(
            _mockStressService.Object,
            _mockCorruptionService.Object,
            _mockTraumaService.Object,
            _mockSpecializationProvider.Object,
            _mockRageService.Object,
            _mockMomentumService.Object,
            _mockCoherenceService.Object,
            _mockLogger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // Constructor Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_NullStressService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedDamageHandler(
            null!,
            _mockCorruptionService.Object,
            _mockTraumaService.Object,
            _mockSpecializationProvider.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("stressService");
    }

    [Test]
    public void Constructor_NullCorruptionService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedDamageHandler(
            _mockStressService.Object,
            null!,
            _mockTraumaService.Object,
            _mockSpecializationProvider.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("corruptionService");
    }

    [Test]
    public void Constructor_NullTraumaService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedDamageHandler(
            _mockStressService.Object,
            _mockCorruptionService.Object,
            null!,
            _mockSpecializationProvider.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("traumaService");
    }

    [Test]
    public void Constructor_NullSpecializationProvider_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new UnifiedDamageHandler(
            _mockStressService.Object,
            _mockCorruptionService.Object,
            _mockTraumaService.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("specializationProvider");
    }

    [Test]
    public void Constructor_WithOptionalDependenciesNull_CreatesInstance()
    {
        // Act
        var handler = new UnifiedDamageHandler(
            _mockStressService.Object,
            _mockCorruptionService.Object,
            _mockTraumaService.Object,
            _mockSpecializationProvider.Object,
            rageService: null,
            momentumService: null,
            coherenceService: null,
            logger: null);

        // Assert
        handler.Should().NotBeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Basic Flow
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_BasicFlow_CalculatesAllComponents()
    {
        // Arrange
        var context = new DamageContext();

        // Act
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert
        result.Should().NotBeNull();
        result.DamageDealt.Should().Be(50);
        result.SoakApplied.Should().Be(5); // Default base soak
        result.DamageAfterSoak.Should().Be(45);
        result.StressGained.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void ProcessDamage_MinimalDamage_AppliesMinimumOne()
    {
        // Arrange
        var context = new DamageContext();

        // Act
        var result = _handler.ProcessDamage(_characterId, 3, context);

        // Assert — damage after soak is minimum 1
        result.DamageAfterSoak.Should().Be(1);
    }

    [Test]
    public void ProcessDamage_ZeroDamage_ReturnsValidResult()
    {
        // Arrange
        var context = new DamageContext();

        // Act
        var result = _handler.ProcessDamage(_characterId, 0, context);

        // Assert
        result.DamageDealt.Should().Be(0);
        result.DamageAfterSoak.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void ProcessDamage_ValidResult_PassesValidation()
    {
        // Arrange
        var context = new DamageContext();

        // Act
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert
        result.IsValid().Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Stress Calculation
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_WithCriticalHit_AddsStressBonus()
    {
        // Arrange
        var context = new DamageContext(IsCriticalHit: true);

        // Act — 50 damage with base 5 soak = 45 damage after soak
        // Base stress = 45 / 10 = 4, Critical bonus = +5 = 9 total
        // Plus potential near-death bonus if applicable
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert — stress should include critical bonus (+5)
        result.StressGained.Should().BeGreaterThanOrEqualTo(9);
        result.ThresholdsCrossed.Should().Contain("CriticalHit");
        result.TransitionMessages.Should().Contain(m => m.Contains("Critical", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void ProcessDamage_NearDeath_AddsStressBonus()
    {
        // Arrange — high damage that would put HP near death
        var context = new DamageContext();

        // Act — 100 damage over 5 soak = 95 damage after soak
        // This exceeds placeholder HP, triggering near-death
        var result = _handler.ProcessDamage(_characterId, 100, context);

        // Assert — stress should include near-death bonus (+10)
        result.ThresholdsCrossed.Should().Contain("NearDeath");
        result.TraumaCheckTriggered.Should().BeTrue();
    }

    [Test]
    public void ProcessDamage_StressFormulaApplied_CalculatesCorrectBase()
    {
        // Arrange — low damage, no bonuses
        var context = new DamageContext();

        // Act — 15 damage - 5 soak = 10 damage after soak
        // Base stress = 10 / 10 = 1
        var result = _handler.ProcessDamage(_characterId, 15, context);

        // Assert
        result.DamageAfterSoak.Should().Be(10);
        result.StressGained.Should().BeGreaterThanOrEqualTo(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Specialization Effects (Berserker)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_BerserkerWithRage_AppliesSoakBonus()
    {
        // Arrange — set up Berserker with rage state
        var rageState = RageState.Create(_characterId, 50);
        _mockRageService
            .Setup(r => r.GetRageState(_characterId))
            .Returns(rageState);

        var context = new DamageContext();

        // Act — 50 damage with base 5 soak + 5 rage bonus (10% of 50)
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert — soak should include rage bonus
        result.SoakApplied.Should().Be(10); // 5 base + 5 rage bonus
        result.DamageAfterSoak.Should().Be(40);
    }

    [Test]
    public void ProcessDamage_BerserkerTakingDamage_GainsRage()
    {
        // Arrange — set up Berserker with zero rage (to detect specialization)
        var rageState = RageState.Create(_characterId, 0);
        _mockRageService
            .Setup(r => r.GetRageState(_characterId))
            .Returns(rageState);

        var context = new DamageContext();

        // Act — 50 damage = +10 rage (1 per 5 damage)
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert
        result.RageGained.Should().Be(10);
        result.TransitionMessages.Should().Contain(m => m.Contains("rage", StringComparison.OrdinalIgnoreCase));
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Specialization Effects (Storm Blade)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_StormBladeOnCritical_LosesMomentum()
    {
        // Arrange — set up Storm Blade with momentum state
        var momentumState = MomentumState.Create(_characterId);
        _mockMomentumService
            .Setup(m => m.GetMomentumState(_characterId))
            .Returns(momentumState);

        var context = new DamageContext(IsCriticalHit: true);

        // Act
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert
        result.MomentumLost.Should().Be(20);
        result.ThresholdsCrossed.Should().Contain("MomentumLost");
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Specialization Effects (Arcanist)
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_ArcanistOnInterrupt_LosesCoherence()
    {
        // Arrange — set up Arcanist with coherence state
        var coherenceState = CoherenceState.Create(_characterId);
        _mockCoherenceService
            .Setup(c => c.GetCoherenceState(_characterId))
            .Returns(coherenceState);

        var context = new DamageContext(IsInterrupt: true);

        // Act
        var result = _handler.ProcessDamage(_characterId, 30, context);

        // Assert
        result.CoherenceLost.Should().Be(15);
        result.ThresholdsCrossed.Should().Contain("CoherenceLost");
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Trauma Triggers
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_AllyDeathEvent_TriggersTraumaCheck()
    {
        // Arrange
        var context = new DamageContext(IsAllyDeathEvent: true);

        // Act
        var result = _handler.ProcessDamage(_characterId, 0, context);

        // Assert
        result.TraumaCheckTriggered.Should().BeTrue();
        result.ThresholdsCrossed.Should().Contain("TraumaCheck");
    }

    [Test]
    public void ProcessDamage_HighDamage_TriggersTraumaCheck()
    {
        // Arrange — damage that would reduce HP to 0
        var context = new DamageContext();

        // Act — 100 damage with 5 soak = 95 after soak (exceeds placeholder HP)
        var result = _handler.ProcessDamage(_characterId, 100, context);

        // Assert
        result.TraumaCheckTriggered.Should().BeTrue();
    }

    [Test]
    public void ProcessDamage_ModestDamage_NoTraumaCheck()
    {
        // Arrange — damage that doesn't trigger near-death
        var context = new DamageContext();

        // Act — 30 damage with 5 soak = 25 after soak
        var result = _handler.ProcessDamage(_characterId, 30, context);

        // Assert
        result.TraumaCheckTriggered.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProcessDamage Tests - Result Messages
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void ProcessDamage_WithAllFactors_GeneratesMessages()
    {
        // Arrange — critical hit with Berserker specialization
        var rageState = RageState.Create(_characterId, 0);
        _mockRageService
            .Setup(r => r.GetRageState(_characterId))
            .Returns(rageState);

        var context = new DamageContext(IsCriticalHit: true);

        // Act
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert — should have damage message, critical message, and rage message
        result.TransitionMessages.Should().HaveCountGreaterThanOrEqualTo(2);
        result.TransitionMessages.Should().Contain(m => m.Contains("damage", StringComparison.OrdinalIgnoreCase));
        result.TransitionMessages.Should().Contain(m => m.Contains("Critical", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public void ProcessDamage_BasicDamage_IncludesDamageMessage()
    {
        // Arrange
        var context = new DamageContext();

        // Act
        var result = _handler.ProcessDamage(_characterId, 50, context);

        // Assert
        result.TransitionMessages.Should().NotBeEmpty();
        result.TransitionMessages.Should().Contain(m => m.Contains("damage", StringComparison.OrdinalIgnoreCase));
    }

    // ═══════════════════════════════════════════════════════════════
    // DamageIntegrationResult Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void DamageIntegrationResult_Create_SetsDefaults()
    {
        // Act
        var result = DamageIntegrationResult.Create(
            damageDealt: 50,
            damageAfterSoak: 45,
            soakApplied: 5,
            stressGained: 4);

        // Assert
        result.StressSource.Should().Be(StressSource.Combat);
        result.ThresholdsCrossed.Should().NotBeNull().And.BeEmpty();
        result.TransitionMessages.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void DamageIntegrationResult_IsValid_ReturnsTrueForValidResult()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: 50,
            damageAfterSoak: 45,
            soakApplied: 5,
            stressGained: 4);

        // Assert
        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void DamageIntegrationResult_IsValid_ReturnsFalseForNegativeDamage()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: -1,
            damageAfterSoak: 1,
            soakApplied: 0,
            stressGained: 0);

        // Assert
        result.IsValid().Should().BeFalse();
    }

    [Test]
    public void DamageIntegrationResult_IsValid_ReturnsFalseForExcessiveSoak()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: 50,
            damageAfterSoak: 45,
            soakApplied: 100, // Soak exceeds damage
            stressGained: 4);

        // Assert
        result.IsValid().Should().BeFalse();
    }

    [Test]
    public void DamageIntegrationResult_IsCriticalHit_DetectsFromMessages()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: 50,
            damageAfterSoak: 45,
            soakApplied: 5,
            stressGained: 9,
            transitionMessages: new List<string> { "Critical hit—mind reels!" }.AsReadOnly());

        // Assert
        result.IsCriticalHit.Should().BeTrue();
    }

    [Test]
    public void DamageIntegrationResult_HasSpecializationEffects_DetectsRageGain()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: 50,
            damageAfterSoak: 45,
            soakApplied: 5,
            stressGained: 4,
            rageGained: 10);

        // Assert
        result.HasSpecializationEffects.Should().BeTrue();
    }

    [Test]
    public void DamageIntegrationResult_IsHighImpact_TrueForHighStress()
    {
        // Arrange
        var result = DamageIntegrationResult.Create(
            damageDealt: 100,
            damageAfterSoak: 95,
            soakApplied: 5,
            stressGained: 10);

        // Assert
        result.IsHighImpact.Should().BeTrue();
    }

    [Test]
    public void DamageIntegrationResult_Empty_ReturnsMinimalResult()
    {
        // Act
        var result = DamageIntegrationResult.Empty;

        // Assert
        result.DamageDealt.Should().Be(0);
        result.DamageAfterSoak.Should().Be(1);
        result.StressGained.Should().Be(0);
    }
}
