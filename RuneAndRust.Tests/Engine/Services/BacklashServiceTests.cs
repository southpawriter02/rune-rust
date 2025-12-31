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
/// Unit tests for BacklashService (v0.4.3d - The Backlash).
/// Validates backlash mechanics, corruption tracking, and risk calculations.
/// </summary>
public class BacklashServiceTests
{
    private readonly Mock<IAetherService> _mockAether;
    private readonly Mock<IDiceService> _mockDice;
    private readonly Mock<IStatusEffectService> _mockStatus;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<BacklashService>> _mockLogger;
    private readonly BacklashService _sut;

    public BacklashServiceTests()
    {
        _mockAether = new Mock<IAetherService>();
        _mockDice = new Mock<IDiceService>();
        _mockStatus = new Mock<IStatusEffectService>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<BacklashService>>();

        _sut = new BacklashService(
            _mockAether.Object,
            _mockDice.Object,
            _mockStatus.Object,
            _mockEventBus.Object,
            _mockLogger.Object);
    }

    #region Helper Methods

    private static Combatant CreateCaster(int corruption = 0)
    {
        var character = new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Mage",
            Archetype = ArchetypeType.Adept,
            Corruption = corruption
        };

        return new Combatant
        {
            Id = Guid.NewGuid(),
            Name = character.Name,
            IsPlayer = true,
            CurrentHp = 50,
            MaxHp = 50,
            CurrentAp = 10,
            CharacterSource = character
        };
    }

    private static Character CreateCharacter(int corruption = 0)
    {
        return new Character
        {
            Id = Guid.NewGuid(),
            Name = "Test Character",
            Archetype = ArchetypeType.Mystic,
            Corruption = corruption
        };
    }

    #endregion

    #region GetCurrentRisk Tests

    [Fact]
    public void GetCurrentRisk_FluxBelowCritical_ReturnsZero()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(40);

        // Act
        var risk = _sut.GetCurrentRisk();

        // Assert
        risk.Should().Be(0);
    }

    [Fact]
    public void GetCurrentRisk_FluxAtCritical_ReturnsZero()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(50);

        // Act
        var risk = _sut.GetCurrentRisk();

        // Assert
        risk.Should().Be(0);
    }

    [Fact]
    public void GetCurrentRisk_FluxAboveCritical_ReturnsCorrectRisk()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(75);

        // Act
        var risk = _sut.GetCurrentRisk();

        // Assert
        risk.Should().Be(25); // 75 - 50 = 25%
    }

    [Fact]
    public void GetCurrentRisk_FluxAtMax_ReturnsFifty()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(100);

        // Act
        var risk = _sut.GetCurrentRisk();

        // Assert
        risk.Should().Be(50); // 100 - 50 = 50%
    }

    #endregion

    #region IsAtRisk Tests

    [Fact]
    public void IsAtRisk_FluxBelowCritical_ReturnsFalse()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(40);

        // Act
        var atRisk = _sut.IsAtRisk();

        // Assert
        atRisk.Should().BeFalse();
    }

    [Fact]
    public void IsAtRisk_FluxAboveCritical_ReturnsTrue()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(60);

        // Act
        var atRisk = _sut.IsAtRisk();

        // Assert
        atRisk.Should().BeTrue();
    }

    #endregion

    #region CheckBacklash Tests - No Risk

    [Fact]
    public void CheckBacklash_FluxBelowCritical_ReturnsNoBacklash()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(30);
        var caster = CreateCaster();

        // Act
        var result = _sut.CheckBacklash(caster, "Fireball");

        // Assert
        result.Triggered.Should().BeFalse();
        result.Severity.Should().Be(BacklashSeverity.None);
        result.DamageDealt.Should().Be(0);
    }

    #endregion

    #region CheckBacklash Tests - Roll Succeeds

    [Fact]
    public void CheckBacklash_RollBeatRisk_ReturnsNoBacklash()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(75); // 25% risk
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(50); // Rolled 50 > 25
        var caster = CreateCaster();

        // Act
        var result = _sut.CheckBacklash(caster, "Fireball");

        // Assert
        result.Triggered.Should().BeFalse();
        result.RiskChance.Should().Be(25);
        result.Roll.Should().Be(50);
    }

    #endregion

    #region CheckBacklash Tests - Minor Backlash

    [Fact]
    public void CheckBacklash_MinorBacklash_AppliesCorrectEffects()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(60); // 10% risk
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(5); // Rolled 5 <= 10 (margin 5)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(4); // 1d6 damage = 4
        var caster = CreateCaster();

        // Act
        var result = _sut.CheckBacklash(caster, "Fireball");

        // Assert
        result.Triggered.Should().BeTrue();
        result.Severity.Should().Be(BacklashSeverity.Minor);
        result.DamageDealt.Should().Be(4);
        result.AetherSicknessDuration.Should().Be(0);
        result.CorruptionAdded.Should().Be(0);
        caster.CurrentHp.Should().Be(46); // 50 - 4
    }

    #endregion

    #region CheckBacklash Tests - Major Backlash

    [Fact]
    public void CheckBacklash_MajorBacklash_AppliesCorrectEffects()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(80); // 30% risk
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(15); // margin = 15 (Major)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(3); // 2d6 damage = 6
        var caster = CreateCaster();

        // Act
        var result = _sut.CheckBacklash(caster, "Fireball");

        // Assert
        result.Triggered.Should().BeTrue();
        result.Severity.Should().Be(BacklashSeverity.Major);
        result.DamageDealt.Should().Be(6); // 3 + 3
        result.AetherSicknessDuration.Should().Be(2);
        result.CorruptionAdded.Should().Be(0);

        // Verify Aether Sickness was applied
        _mockStatus.Verify(s => s.ApplyEffect(
            caster,
            StatusEffectType.AetherSickness,
            2,
            caster.Id), Times.Once);
    }

    #endregion

    #region CheckBacklash Tests - Catastrophic Backlash

    [Fact]
    public void CheckBacklash_CatastrophicBacklash_AppliesCorrectEffects()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(100); // 50% risk
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(10); // margin = 40 (Catastrophic)
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(5); // 3d6 damage = 15
        var caster = CreateCaster();

        // Act
        var result = _sut.CheckBacklash(caster, "Fireball");

        // Assert
        result.Triggered.Should().BeTrue();
        result.Severity.Should().Be(BacklashSeverity.Catastrophic);
        result.DamageDealt.Should().Be(15); // 5 + 5 + 5
        result.AetherSicknessDuration.Should().Be(5);
        result.CorruptionAdded.Should().Be(1);

        // Verify Aether Sickness was applied
        _mockStatus.Verify(s => s.ApplyEffect(
            caster,
            StatusEffectType.AetherSickness,
            5,
            caster.Id), Times.Once);

        // Verify corruption was added to character
        caster.CharacterSource!.Corruption.Should().Be(1);
    }

    [Fact]
    public void CheckBacklash_Catastrophic_PublishesCorruptionEvent()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(100);
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(10);
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(3);
        var caster = CreateCaster();

        // Act
        _sut.CheckBacklash(caster, "Fireball");

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.IsAny<CorruptionChangedEvent>()), Times.Once);
    }

    #endregion

    #region CheckBacklash Tests - Event Publishing

    [Fact]
    public void CheckBacklash_BacklashTriggered_PublishesBacklashEvent()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(60);
        _mockDice.Setup(d => d.RollSingle(100, It.IsAny<string>())).Returns(5);
        _mockDice.Setup(d => d.RollSingle(6, It.IsAny<string>())).Returns(3);
        var caster = CreateCaster();

        // Act
        _sut.CheckBacklash(caster, "Fireball");

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.Is<BacklashEvent>(evt =>
            evt.CasterId == caster.Id &&
            evt.SpellAttempted == "Fireball")), Times.Once);
    }

    #endregion

    #region CorruptionLevel Tests

    [Theory]
    [InlineData(0, CorruptionLevel.Untouched)]
    [InlineData(5, CorruptionLevel.Untouched)]
    [InlineData(9, CorruptionLevel.Untouched)]
    [InlineData(10, CorruptionLevel.Tainted)]
    [InlineData(20, CorruptionLevel.Tainted)]
    [InlineData(24, CorruptionLevel.Tainted)]
    [InlineData(25, CorruptionLevel.Afflicted)]
    [InlineData(40, CorruptionLevel.Afflicted)]
    [InlineData(49, CorruptionLevel.Afflicted)]
    [InlineData(50, CorruptionLevel.Blighted)]
    [InlineData(60, CorruptionLevel.Blighted)]
    [InlineData(74, CorruptionLevel.Blighted)]
    [InlineData(75, CorruptionLevel.Lost)]
    [InlineData(90, CorruptionLevel.Lost)]
    [InlineData(100, CorruptionLevel.Lost)]
    public void GetCorruptionLevel_ReturnsCorrectLevel(int corruption, CorruptionLevel expected)
    {
        // Arrange
        var character = CreateCharacter(corruption);

        // Act
        var level = _sut.GetCorruptionLevel(character);

        // Assert
        level.Should().Be(expected);
    }

    [Fact]
    public void GetCorruptionLevel_CombatantWithoutCharacter_ReturnsUntouched()
    {
        // Arrange
        var combatant = new Combatant
        {
            Id = Guid.NewGuid(),
            Name = "Enemy",
            IsPlayer = false,
            CharacterSource = null
        };

        // Act
        var level = _sut.GetCorruptionLevel(combatant);

        // Assert
        level.Should().Be(CorruptionLevel.Untouched);
    }

    #endregion

    #region CorruptionPenalties Tests

    [Fact]
    public void GetCorruptionPenalties_Untouched_NoPenalties()
    {
        // Arrange
        var character = CreateCharacter(5);

        // Act
        var penalties = _sut.GetCorruptionPenalties(character);

        // Assert
        penalties.Level.Should().Be(CorruptionLevel.Untouched);
        penalties.MaxApMultiplier.Should().Be(1.0);
        penalties.WillPenalty.Should().Be(0);
        penalties.WitsPenalty.Should().Be(0);
        penalties.CanCastSpells.Should().BeTrue();
    }

    [Fact]
    public void GetCorruptionPenalties_Tainted_ApReduction()
    {
        // Arrange
        var character = CreateCharacter(15);

        // Act
        var penalties = _sut.GetCorruptionPenalties(character);

        // Assert
        penalties.Level.Should().Be(CorruptionLevel.Tainted);
        penalties.MaxApMultiplier.Should().Be(0.9);
        penalties.WillPenalty.Should().Be(0);
        penalties.CanCastSpells.Should().BeTrue();
    }

    [Fact]
    public void GetCorruptionPenalties_Afflicted_ApAndWillPenalty()
    {
        // Arrange
        var character = CreateCharacter(35);

        // Act
        var penalties = _sut.GetCorruptionPenalties(character);

        // Assert
        penalties.Level.Should().Be(CorruptionLevel.Afflicted);
        penalties.MaxApMultiplier.Should().Be(0.8);
        penalties.WillPenalty.Should().Be(-1);
        penalties.WitsPenalty.Should().Be(0);
        penalties.CanCastSpells.Should().BeTrue();
    }

    [Fact]
    public void GetCorruptionPenalties_Blighted_AllPenalties()
    {
        // Arrange
        var character = CreateCharacter(60);

        // Act
        var penalties = _sut.GetCorruptionPenalties(character);

        // Assert
        penalties.Level.Should().Be(CorruptionLevel.Blighted);
        penalties.MaxApMultiplier.Should().Be(0.7);
        penalties.WillPenalty.Should().Be(-2);
        penalties.WitsPenalty.Should().Be(-1);
        penalties.CanCastSpells.Should().BeTrue();
    }

    [Fact]
    public void GetCorruptionPenalties_Lost_CannotCast()
    {
        // Arrange
        var character = CreateCharacter(80);

        // Act
        var penalties = _sut.GetCorruptionPenalties(character);

        // Assert
        penalties.Level.Should().Be(CorruptionLevel.Lost);
        penalties.MaxApMultiplier.Should().Be(0.0);
        penalties.CanCastSpells.Should().BeFalse();
    }

    #endregion

    #region AddCorruption Tests

    [Fact]
    public void AddCorruption_IncreasesCorruption()
    {
        // Arrange
        var character = CreateCharacter(5);

        // Act
        _sut.AddCorruption(character, 10, "Test");

        // Assert
        character.Corruption.Should().Be(15);
    }

    [Fact]
    public void AddCorruption_ClampsTo100()
    {
        // Arrange
        var character = CreateCharacter(95);

        // Act
        _sut.AddCorruption(character, 20, "Test");

        // Assert
        character.Corruption.Should().Be(100);
    }

    [Fact]
    public void AddCorruption_TierChange_PublishesEvent()
    {
        // Arrange
        var character = CreateCharacter(8);

        // Act
        _sut.AddCorruption(character, 5, "Test"); // 8 + 5 = 13 (Tainted)

        // Assert
        _mockEventBus.Verify(e => e.Publish(It.Is<CorruptionChangedEvent>(evt =>
            evt.TierChanged == true &&
            evt.PreviousLevel == CorruptionLevel.Untouched &&
            evt.NewLevel == CorruptionLevel.Tainted)), Times.Once);
    }

    #endregion

    #region PurgeCorruption Tests

    [Fact]
    public void PurgeCorruption_ReducesCorruption()
    {
        // Arrange
        var character = CreateCharacter(30);

        // Act
        var result = _sut.PurgeCorruption(character, 10, "Ritual");

        // Assert
        result.Should().BeTrue();
        character.Corruption.Should().Be(20);
    }

    [Fact]
    public void PurgeCorruption_ClampsToZero()
    {
        // Arrange
        var character = CreateCharacter(5);

        // Act
        var result = _sut.PurgeCorruption(character, 20, "Ritual");

        // Assert
        result.Should().BeTrue();
        character.Corruption.Should().Be(0);
    }

    [Fact]
    public void PurgeCorruption_NoCorruption_ReturnsFalse()
    {
        // Arrange
        var character = CreateCharacter(0);

        // Act
        var result = _sut.PurgeCorruption(character, 10, "Ritual");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CanCastSpells Tests

    [Fact]
    public void CanCastSpells_BelowLost_ReturnsTrue()
    {
        // Arrange
        var character = CreateCharacter(70);

        // Act
        var canCast = _sut.CanCastSpells(character);

        // Assert
        canCast.Should().BeTrue();
    }

    [Fact]
    public void CanCastSpells_AtLost_ReturnsFalse()
    {
        // Arrange
        var character = CreateCharacter(75);

        // Act
        var canCast = _sut.CanCastSpells(character);

        // Assert
        canCast.Should().BeFalse();
    }

    #endregion

    #region GetRiskWarning Tests

    [Fact]
    public void GetRiskWarning_NoRisk_ReturnsEmpty()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(40);

        // Act
        var warning = _sut.GetRiskWarning();

        // Assert
        warning.Should().BeEmpty();
    }

    [Fact]
    public void GetRiskWarning_LowRisk_ReturnsCaution()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(55);

        // Act
        var warning = _sut.GetRiskWarning();

        // Assert
        warning.Should().Contain("Caution");
        warning.Should().Contain("5%");
    }

    [Fact]
    public void GetRiskWarning_HighRisk_ReturnsCritical()
    {
        // Arrange
        _mockAether.Setup(a => a.CurrentFlux).Returns(95);

        // Act
        var warning = _sut.GetRiskWarning();

        // Assert
        warning.Should().Contain("CRITICAL");
        warning.Should().Contain("45%");
    }

    #endregion

    #region BacklashResult Factory Tests

    [Fact]
    public void BacklashResult_NoBacklash_CorrectValues()
    {
        // Act
        var result = BacklashResult.NoBacklash(15, 30);

        // Assert
        result.Triggered.Should().BeFalse();
        result.Severity.Should().Be(BacklashSeverity.None);
        result.RiskChance.Should().Be(15);
        result.Roll.Should().Be(30);
        result.FailMargin.Should().Be(0);
    }

    [Fact]
    public void BacklashResult_Backlash_CorrectValues()
    {
        // Act
        var result = BacklashResult.Backlash(
            BacklashSeverity.Major,
            damage: 8,
            sicknessDuration: 2,
            corruption: 0,
            message: "Test message",
            riskChance: 30,
            roll: 15);

        // Assert
        result.Triggered.Should().BeTrue();
        result.Severity.Should().Be(BacklashSeverity.Major);
        result.DamageDealt.Should().Be(8);
        result.AetherSicknessDuration.Should().Be(2);
        result.CorruptionAdded.Should().Be(0);
        result.Message.Should().Be("Test message");
        result.FailMargin.Should().Be(15); // 30 - 15
    }

    #endregion

    #region CorruptionPenalties Factory Tests

    [Fact]
    public void CorruptionPenalties_Untouched_HasCorrectDescription()
    {
        // Act
        var penalties = CorruptionPenalties.Untouched(5);

        // Assert
        penalties.Description.Should().Contain("untouched");
        penalties.HasPenalties.Should().BeFalse();
    }

    [Fact]
    public void CorruptionPenalties_Lost_HasCorrectDescription()
    {
        // Act
        var penalties = CorruptionPenalties.Lost(80);

        // Assert
        penalties.Description.Should().Contain("consumed");
        penalties.HasPenalties.Should().BeTrue();
        penalties.EffectiveApPenalty.Should().Be(10); // 100% reduction
    }

    #endregion
}
