using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Combat;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine.Services;

/// <summary>
/// Unit tests for MagicService (v0.4.3c - The Incantation).
/// Validates spell casting mechanics including validation, flux generation, and effect execution.
/// </summary>
public class MagicServiceTests
{
    private readonly Mock<IAetherService> _mockAether;
    private readonly Mock<IStatusEffectService> _mockStatus;
    private readonly Mock<EffectScriptExecutor> _mockExecutor;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<IBacklashService> _mockBacklash;
    private readonly Mock<ILogger<MagicService>> _mockLogger;
    private readonly MagicService _sut;

    public MagicServiceTests()
    {
        _mockAether = new Mock<IAetherService>();
        _mockStatus = new Mock<IStatusEffectService>();
        _mockEventBus = new Mock<IEventBus>();
        _mockBacklash = new Mock<IBacklashService>();
        _mockLogger = new Mock<ILogger<MagicService>>();

        // Configure backlash service to not trigger by default
        _mockBacklash.Setup(b => b.IsAtRisk()).Returns(false);
        _mockBacklash.Setup(b => b.CanCastSpells(It.IsAny<Character>())).Returns(true);

        // Mock EffectScriptExecutor - requires mocking its dependencies
        var mockDice = new Mock<IDiceService>();
        var mockStatusForExecutor = new Mock<IStatusEffectService>();
        var mockExecutorLogger = new Mock<ILogger<EffectScriptExecutor>>();
        var executor = new EffectScriptExecutor(
            mockDice.Object,
            mockStatusForExecutor.Object,
            mockExecutorLogger.Object);

        _sut = new MagicService(
            _mockAether.Object,
            _mockStatus.Object,
            executor,
            _mockEventBus.Object,
            _mockBacklash.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private static Combatant CreatePlayerCaster(ArchetypeType archetype = ArchetypeType.Adept, int currentAp = 10)
    {
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Mage",
            Archetype = archetype,
            CurrentAp = currentAp,
            MaxAp = 20
        };

        return new Combatant
        {
            Id = character.Id,
            Name = character.Name,
            CurrentHp = 50,
            MaxHp = 50,
            CurrentAp = currentAp,
            IsPlayer = true,
            CharacterSource = character
        };
    }

    private static Combatant CreateEnemy(string name = "Goblin", int hp = 20)
    {
        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrentHp = hp,
            MaxHp = hp,
            CurrentAp = 2,
            IsPlayer = false
        };
    }

    private static Spell CreateTestSpell(
        string name = "Fireball",
        int apCost = 3,
        int fluxCost = 5,
        SpellSchool school = SpellSchool.Destruction,
        SpellTargetType targetType = SpellTargetType.SingleEnemy,
        int chargeTurns = 0,
        bool requiresConcentration = false)
    {
        return new Spell
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test spell",
            School = school,
            TargetType = targetType,
            Range = SpellRange.Medium,
            ApCost = apCost,
            FluxCost = fluxCost,
            BasePower = 10,
            EffectScript = "DAMAGE:Fire:2d6",
            ChargeTurns = chargeTurns,
            RequiresConcentration = requiresConcentration
        };
    }

    #endregion

    #region CanCast Tests

    [Fact]
    public void CanCast_AdeptWithSufficientAP_ReturnsTrue()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanCast_MysticWithSufficientAP_ReturnsTrue()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Mystic, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanCast_WarriorArchetype_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Warrior, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanCast_InsufficientAP_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 2);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 5);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanCast_DeadTarget_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy(hp: 0);
        target.CurrentHp = 0;
        var spell = CreateTestSpell();

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanCast_SilencedCaster_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell();

        _mockStatus.Setup(s => s.HasEffect(caster, StatusEffectType.Silenced))
            .Returns(true);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanCast_ConcentrationConflict_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(requiresConcentration: true);

        _mockStatus.Setup(s => s.HasEffect(caster, StatusEffectType.Concentrating))
            .Returns(true);

        // Act
        var result = _sut.CanCast(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetFailureReason Tests

    [Fact]
    public void GetFailureReason_ValidCast_ReturnsNone()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3);

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.None);
    }

    [Fact]
    public void GetFailureReason_NonMagicUser_ReturnsNotMagicUser()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Warrior, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell();

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.NotMagicUser);
    }

    [Fact]
    public void GetFailureReason_InsufficientAP_ReturnsInsufficientAP()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 2);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 5);

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.InsufficientAP);
    }

    [Fact]
    public void GetFailureReason_TargetDead_ReturnsTargetDead()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        target.CurrentHp = 0;
        var spell = CreateTestSpell();

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.TargetDead);
    }

    [Fact]
    public void GetFailureReason_Silenced_ReturnsSilenced()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell();

        _mockStatus.Setup(s => s.HasEffect(caster, StatusEffectType.Silenced))
            .Returns(true);

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.Silenced);
    }

    [Fact]
    public void GetFailureReason_ConcentrationConflict_ReturnsConcentrationConflict()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(requiresConcentration: true);

        _mockStatus.Setup(s => s.HasEffect(caster, StatusEffectType.Concentrating))
            .Returns(true);

        // Act
        var result = _sut.GetFailureReason(caster, spell, target);

        // Assert
        result.Should().Be(CastFailureReason.ConcentrationConflict);
    }

    #endregion

    #region CastSpell Tests

    [Fact]
    public void CastSpell_ValidInstantCast_ReturnsSuccess()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3, fluxCost: 5);

        _mockAether.Setup(a => a.AddFlux(5)).Returns(5);

        // Act
        var result = _sut.CastSpell(caster, spell, target);

        // Assert
        result.Success.Should().BeTrue();
        result.FailureReason.Should().Be(CastFailureReason.None);
    }

    [Fact]
    public void CastSpell_ValidCast_DeductsAP()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3, fluxCost: 5);

        // Act
        _sut.CastSpell(caster, spell, target);

        // Assert
        caster.CurrentAp.Should().Be(7); // 10 - 3
    }

    [Fact]
    public void CastSpell_ValidCast_GeneratesFlux()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3, fluxCost: 5);

        // Act
        _sut.CastSpell(caster, spell, target);

        // Assert
        _mockAether.Verify(a => a.AddFlux(5), Times.Once);
    }

    [Fact]
    public void CastSpell_ConcentrationSpell_AppliesConcentratingStatus()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(requiresConcentration: true);

        // Act
        _sut.CastSpell(caster, spell, target);

        // Assert
        _mockStatus.Verify(s =>
            s.ApplyEffect(caster, StatusEffectType.Concentrating, 99, caster.Id),
            Times.Once);
    }

    [Fact]
    public void CastSpell_ValidCast_PublishesSpellCastEvent()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(apCost: 3, fluxCost: 5);

        // Act
        _sut.CastSpell(caster, spell, target);

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.IsAny<SpellCastEvent>()), Times.Once);
    }

    [Fact]
    public void CastSpell_FailedValidation_ReturnsFailure()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Warrior, 10); // Non-magic user
        var target = CreateEnemy();
        var spell = CreateTestSpell();

        // Act
        var result = _sut.CastSpell(caster, spell, target);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(CastFailureReason.NotMagicUser);
    }

    #endregion

    #region InitiateCharge Tests

    [Fact]
    public void InitiateCharge_ChargedSpell_AppliesChantingStatus()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var spell = CreateTestSpell(chargeTurns: 2);

        // Act
        var result = _sut.InitiateCharge(caster, spell);

        // Assert
        _mockStatus.Verify(s =>
            s.ApplyEffect(caster, StatusEffectType.Chanting, 2, caster.Id),
            Times.Once);
    }

    [Fact]
    public void InitiateCharge_ChargedSpell_ReturnsChargeInitiated()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var spell = CreateTestSpell(chargeTurns: 2);

        // Act
        var result = _sut.InitiateCharge(caster, spell);

        // Assert
        result.Success.Should().BeTrue();
        result.IsChargeInitiation.Should().BeTrue();
    }

    [Fact]
    public void InitiateCharge_ChargedSpell_StoresSpellId()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var spell = CreateTestSpell(chargeTurns: 2);

        // Act
        _sut.InitiateCharge(caster, spell);

        // Assert
        caster.ChanneledSpellId.Should().Be(spell.Id);
    }

    [Fact]
    public void InitiateCharge_ChargedSpell_DeductsAPImmediately()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var spell = CreateTestSpell(apCost: 4, chargeTurns: 2);

        // Act
        _sut.InitiateCharge(caster, spell);

        // Assert
        caster.CurrentAp.Should().Be(6); // 10 - 4
    }

    #endregion

    #region ReleaseCharge Tests

    [Fact]
    public void ReleaseCharge_AfterCharge_ReturnsSuccess()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(chargeTurns: 2);

        _mockStatus.Setup(s => s.HasEffect(caster, StatusEffectType.Chanting))
            .Returns(true);

        // Act
        var result = _sut.ReleaseCharge(caster, spell, target);

        // Assert
        result.Success.Should().BeTrue();
        result.IsChargeInitiation.Should().BeFalse();
    }

    [Fact]
    public void ReleaseCharge_ClearsChantingStatus()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept, 10);
        var target = CreateEnemy();
        var spell = CreateTestSpell(chargeTurns: 2);
        caster.ChanneledSpellId = spell.Id;

        // Act
        _sut.ReleaseCharge(caster, spell, target);

        // Assert
        _mockStatus.Verify(s => s.RemoveEffect(caster, StatusEffectType.Chanting), Times.Once);
        caster.ChanneledSpellId.Should().BeNull();
    }

    #endregion

    #region ValidateTarget Tests

    [Fact]
    public void ValidateTarget_SelfSpellOnSelf_ReturnsTrue()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept);
        var spell = CreateTestSpell(targetType: SpellTargetType.Self);

        // Act
        var result = _sut.ValidateTarget(caster, spell, caster);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateTarget_SelfSpellOnEnemy_ReturnsFalse()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept);
        var target = CreateEnemy();
        var spell = CreateTestSpell(targetType: SpellTargetType.Self);

        // Act
        var result = _sut.ValidateTarget(caster, spell, target);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateTarget_SingleEnemyOnEnemy_ReturnsTrue()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept);
        var target = CreateEnemy();
        var spell = CreateTestSpell(targetType: SpellTargetType.SingleEnemy);

        // Act
        var result = _sut.ValidateTarget(caster, spell, target);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateTarget_SingleAny_AlwaysReturnsTrue()
    {
        // Arrange
        var caster = CreatePlayerCaster(ArchetypeType.Adept);
        var target = CreateEnemy();
        var spell = CreateTestSpell(targetType: SpellTargetType.SingleAny);

        // Act
        var result = _sut.ValidateTarget(caster, spell, target);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region MagicResult Factory Tests

    [Fact]
    public void MagicResult_Ok_CreatesSuccessfulResult()
    {
        // Act
        var result = MagicResult.Ok("Test message", damage: 10, healing: 5, flux: 3);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Test message");
        result.TotalDamage.Should().Be(10);
        result.TotalHealing.Should().Be(5);
        result.FluxGenerated.Should().Be(3);
        result.FailureReason.Should().Be(CastFailureReason.None);
    }

    [Fact]
    public void MagicResult_Failure_CreatesFailedResult()
    {
        // Act
        var result = MagicResult.Failure(CastFailureReason.InsufficientAP);

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be(CastFailureReason.InsufficientAP);
        result.Message.Should().Contain("Aether Points");
    }

    [Fact]
    public void MagicResult_ChargeInitiated_SetsIsChargeInitiation()
    {
        // Act
        var result = MagicResult.ChargeInitiated("Charging...", flux: 5);

        // Assert
        result.Success.Should().BeTrue();
        result.IsChargeInitiation.Should().BeTrue();
        result.FluxGenerated.Should().Be(5);
    }

    #endregion

    #region SpellCastEvent Tests

    [Fact]
    public void SpellCastEvent_IsDamaging_WhenDamageDealt()
    {
        // Arrange
        var evt = new SpellCastEvent(
            Guid.NewGuid(), "Caster", Guid.NewGuid(), "Fireball",
            SpellSchool.Destruction, Guid.NewGuid(), "Target",
            5, false, DamageDealt: 10);

        // Assert
        evt.IsDamaging.Should().BeTrue();
        evt.IsHealing.Should().BeFalse();
    }

    [Fact]
    public void SpellCastEvent_IsHealing_WhenHealingDone()
    {
        // Arrange
        var evt = new SpellCastEvent(
            Guid.NewGuid(), "Caster", Guid.NewGuid(), "Heal",
            SpellSchool.Restoration, Guid.NewGuid(), "Target",
            3, false, HealingDone: 15);

        // Assert
        evt.IsHealing.Should().BeTrue();
        evt.IsDamaging.Should().BeFalse();
    }

    [Fact]
    public void SpellCastEvent_IsSelfTargeted_WhenCasterEqualsTarget()
    {
        // Arrange
        var casterId = Guid.NewGuid();
        var evt = new SpellCastEvent(
            casterId, "Caster", Guid.NewGuid(), "Shield",
            SpellSchool.Alteration, casterId, "Caster",
            2, false);

        // Assert
        evt.IsSelfTargeted.Should().BeTrue();
    }

    #endregion
}
