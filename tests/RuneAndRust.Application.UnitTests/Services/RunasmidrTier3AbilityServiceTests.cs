// ═══════════════════════════════════════════════════════════════════════════════
// RunasmidrTier3AbilityServiceTests.cs
// Unit tests for the RunasmidrTier3AbilityService: Master Scrivener,
// Living Runes, Word of Unmaking, and Tier 3/Capstone prerequisites.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class RunasmidrTier3AbilityServiceTests
{
    private RunasmidrTier3AbilityService _service = null!;
    private Mock<ILogger<RunasmidrTier3AbilityService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<RunasmidrTier3AbilityService>>();
        _service = new RunasmidrTier3AbilityService(_mockLogger.Object, new Random(42));
    }

    // ═══════ Master Scrivener Tests ═══════

    [Test]
    public void ApplyMasterScrivenerDuration_DoublesDuration()
    {
        // Arrange
        var baseDuration = 10;

        // Act
        var result = _service.ApplyMasterScrivenerDuration(baseDuration);

        // Assert
        result.Should().Be(20);
    }

    [Test]
    public void ApplyMasterScrivenerDuration_WithZeroDuration_ReturnsZero()
    {
        // Act
        var result = _service.ApplyMasterScrivenerDuration(0);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void ApplyMasterScrivenerDuration_WithNegative_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => _service.ApplyMasterScrivenerDuration(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ApplyMasterScrivenerTrapDuration_DoublesHours()
    {
        // Arrange
        var baseHours = 1;

        // Act
        var result = _service.ApplyMasterScrivenerTrapDuration(baseHours);

        // Assert
        result.Should().Be(2);
    }

    [Test]
    public void IsMasterScrivenerActive_WhenUnlocked_ReturnsTrue()
    {
        // Arrange
        var abilities = new List<RunasmidrAbilityId>
        {
            RunasmidrAbilityId.InscribeRune,
            RunasmidrAbilityId.MasterScrivener,
            RunasmidrAbilityId.LivingRunes
        };

        // Act
        var result = _service.IsMasterScrivenerActive(abilities);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsMasterScrivenerActive_WhenNotUnlocked_ReturnsFalse()
    {
        // Arrange
        var abilities = new List<RunasmidrAbilityId>
        {
            RunasmidrAbilityId.InscribeRune,
            RunasmidrAbilityId.RunestoneWard
        };

        // Act
        var result = _service.IsMasterScrivenerActive(abilities);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════ Living Runes Tests ═══════

    [Test]
    public void SummonLivingRunes_CreatesTwoEntities()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var position = (5, 3);

        // Act
        var runes = _service.SummonLivingRunes(ownerId, position);

        // Assert
        runes.Should().HaveCount(2);
    }

    [Test]
    public void SummonLivingRunes_EntitiesHaveCorrectStats()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var position = (5, 3);

        // Act
        var runes = _service.SummonLivingRunes(ownerId, position);

        // Assert
        foreach (var rune in runes)
        {
            rune.OwnerId.Should().Be(ownerId);
            rune.CurrentHp.Should().Be(LivingRuneEntity.DefaultMaxHp);
            rune.MaxHp.Should().Be(LivingRuneEntity.DefaultMaxHp);
            rune.Defense.Should().Be(LivingRuneEntity.DefaultDefense);
            rune.AttackBonus.Should().Be(LivingRuneEntity.DefaultAttackBonus);
            rune.DamageDice.Should().Be(LivingRuneEntity.DefaultDamageDice);
            rune.Movement.Should().Be(LivingRuneEntity.DefaultMovement);
            rune.Position.Should().Be(position);
            rune.TurnsRemaining.Should().Be(LivingRuneEntity.DefaultDuration);
            rune.IsActive.Should().BeTrue();
        }
    }

    [Test]
    public void SummonLivingRunes_EntitiesHaveUniqueIds()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var runes = _service.SummonLivingRunes(ownerId, (0, 0));

        // Assert
        runes[0].EntityId.Should().NotBe(runes[1].EntityId);
    }

    [Test]
    public void TickLivingRune_DecrementsDuration()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(Guid.NewGuid(), (0, 0));

        // Act
        var ticked = _service.TickLivingRune(rune);

        // Assert
        ticked.TurnsRemaining.Should().Be(4);
        ticked.IsActive.Should().BeTrue();
    }

    [Test]
    public void ApplyDamageToLivingRune_ReducesHp()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(Guid.NewGuid(), (0, 0));

        // Act
        var damaged = _service.ApplyDamageToLivingRune(rune, 6);

        // Assert
        damaged.CurrentHp.Should().Be(4);
        damaged.IsDestroyed.Should().BeFalse();
        damaged.IsActive.Should().BeTrue();
    }

    [Test]
    public void ApplyDamageToLivingRune_LethalDamage_DestroysEntity()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(Guid.NewGuid(), (0, 0));

        // Act
        var destroyed = _service.ApplyDamageToLivingRune(rune, 12);

        // Assert
        destroyed.CurrentHp.Should().Be(0);
        destroyed.IsDestroyed.Should().BeTrue();
        destroyed.IsActive.Should().BeFalse();
    }

    [Test]
    public void RollLivingRuneAttackDamage_ReturnsValueInRange()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(Guid.NewGuid(), (0, 0));

        // Act
        var damage = _service.RollLivingRuneAttackDamage(rune);

        // Assert
        damage.Should().BeInRange(1, 8);
    }

    // ═══════ Word of Unmaking Tests ═══════

    [Test]
    public void ExecuteWordOfUnmaking_BuildsCorrectResult()
    {
        // Arrange
        var casterId = Guid.NewGuid();
        var effects = new List<string> { "Shield of Faith", "Bless" };
        var entities = new List<Guid> { Guid.NewGuid() };
        var items = new List<Guid> { Guid.NewGuid() };
        var characters = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = _service.ExecuteWordOfUnmaking(
            casterId, (5, 5), effects, entities, items, characters);

        // Assert
        result.EffectsRemoved.Should().HaveCount(2);
        result.EntitiesDestroyed.Should().HaveCount(1);
        result.ItemsAffected.Should().HaveCount(1);
        result.AffectedCharacters.Should().HaveCount(2);
        result.TotalEffectsDispelled.Should().Be(4, "2 effects + 1 entity + 1 item = 4");
        result.DestroyedEntities.Should().BeTrue();
    }

    [Test]
    public void ExecuteWordOfUnmaking_EmptyArea_ReturnsZeroResult()
    {
        // Arrange
        var casterId = Guid.NewGuid();

        // Act
        var result = _service.ExecuteWordOfUnmaking(
            casterId,
            (0, 0),
            Array.Empty<string>(),
            Array.Empty<Guid>(),
            Array.Empty<Guid>(),
            Array.Empty<Guid>());

        // Assert
        result.TotalEffectsDispelled.Should().Be(0);
        result.DestroyedEntities.Should().BeFalse();
    }

    [Test]
    public void HasUsedWordOfUnmaking_WhenUsed_ReturnsTrue()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var usageLog = new List<Guid> { characterId, Guid.NewGuid() };

        // Act
        var result = _service.HasUsedWordOfUnmaking(characterId, usageLog);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void HasUsedWordOfUnmaking_WhenNotUsed_ReturnsFalse()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var usageLog = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = _service.HasUsedWordOfUnmaking(characterId, usageLog);

        // Assert
        result.Should().BeFalse();
    }

    // ═══════ Prerequisite Tests ═══════

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

    // ═══════ Constructor Tests ═══════

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new RunasmidrTier3AbilityService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ═══════ Constant Tests ═══════

    [Test]
    public void Constants_MatchDesignSpecification()
    {
        // Assert
        RunasmidrTier3AbilityService.MasterScrivenerMultiplier.Should().Be(2);
        RunasmidrTier3AbilityService.LivingRunesChargeCost.Should().Be(3);
        RunasmidrTier3AbilityService.WordOfUnmakingChargeCost.Should().Be(4);
        RunasmidrTier3AbilityService.DispelRadius.Should().Be(4);
        RunasmidrTier3AbilityService.Tier3Threshold.Should().Be(16);
        RunasmidrTier3AbilityService.CapstoneThreshold.Should().Be(24);
    }
}
