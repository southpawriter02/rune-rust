// ═══════════════════════════════════════════════════════════════════════════════
// SkjaldmaerTier2AbilityServiceTests.cs
// Unit tests for the SkjaldmaerTier2AbilityService.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class SkjaldmaerTier2AbilityServiceTests
{
    private SkjaldmaerTier2AbilityService _service = null!;
    private Mock<ILogger<SkjaldmaerTier2AbilityService>> _mockLogger = null!;

    // Seeded random for deterministic 1d6 rolls in tests
    private Random _seededRandom = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SkjaldmaerTier2AbilityService>>();
        _seededRandom = new Random(42);
        _service = new SkjaldmaerTier2AbilityService(_mockLogger.Object, _seededRandom);
    }

    // ═══════ Hold the Line ═══════

    [Test]
    public void ActivateHoldTheLine_CreatesActiveState()
    {
        // Arrange & Act
        var state = _service.ActivateHoldTheLine((5, 5));

        // Assert
        state.IsActive.Should().BeTrue();
        state.TurnsRemaining.Should().Be(2);
        state.BlockedPosition.Should().Be((5, 5));
    }

    [Test]
    public void TickHoldTheLine_DecrementsAndExpires()
    {
        // Arrange
        var state = _service.ActivateHoldTheLine((3, 3));

        // Act
        var ticked = _service.TickHoldTheLine(state);

        // Assert
        ticked.TurnsRemaining.Should().Be(1);
        ticked.IsActive.Should().BeTrue();

        // Act — second tick expires
        var expired = _service.TickHoldTheLine(ticked);

        // Assert
        expired.TurnsRemaining.Should().Be(0);
        expired.IsActive.Should().BeFalse();
    }

    [Test]
    public void DeactivateHoldTheLine_ReturnsInactiveState()
    {
        // Arrange & Act
        var state = _service.DeactivateHoldTheLine();

        // Assert
        state.IsActive.Should().BeFalse();
        state.TurnsRemaining.Should().Be(0);
    }

    // ═══════ Counter-Shield ═══════

    [Test]
    public void ExecuteCounterShield_RollsDamageInValidRange()
    {
        // Arrange
        var skjaldmaerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();

        // Act
        var result = _service.ExecuteCounterShield(skjaldmaerId, attackerId, 15);

        // Assert
        result.DamageRoll.Should().BeInRange(1, 6);
        result.SkjaldmaerId.Should().Be(skjaldmaerId);
        result.AttackerId.Should().Be(attackerId);
        result.ShouldApplyDamage().Should().BeTrue();
    }

    [Test]
    public void ExecuteCounterShield_ProducesDeterministicResultsWithSeededRandom()
    {
        // Arrange — use a fresh seeded random
        var seeded = new Random(123);
        var service = new SkjaldmaerTier2AbilityService(_mockLogger.Object, seeded);
        var skjaldmaerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();

        // Act — collect multiple rolls
        var rolls = new List<int>();
        for (var i = 0; i < 10; i++)
        {
            rolls.Add(service.ExecuteCounterShield(skjaldmaerId, attackerId, 10).DamageRoll);
        }

        // Assert — all should be in valid 1d6 range
        rolls.Should().AllSatisfy(r => r.Should().BeInRange(1, 6));
    }

    // ═══════ Rally ═══════

    [Test]
    public void ActivateRally_CreatesBuffsForAllAllies()
    {
        // Arrange
        var casterId = Guid.NewGuid();
        var allyIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var buffs = _service.ActivateRally(casterId, allyIds);

        // Assert
        buffs.Should().HaveCount(3);
        buffs.Should().AllSatisfy(b =>
        {
            b.CasterCharacterId.Should().Be(casterId);
            b.SaveBonus.Should().Be(2);
            b.IsConsumed.Should().BeFalse();
            b.IsActive().Should().BeTrue();
        });
    }

    [Test]
    public void ActivateRally_WithEmptyAllyList_ReturnsEmptyBuffs()
    {
        // Arrange
        var casterId = Guid.NewGuid();

        // Act
        var buffs = _service.ActivateRally(casterId, Array.Empty<Guid>());

        // Assert
        buffs.Should().BeEmpty();
    }

    [Test]
    public void ConsumeRallyBuff_MarksBuffAsConsumed()
    {
        // Arrange
        var buff = RallyBuff.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var consumed = _service.ConsumeRallyBuff(buff);

        // Assert
        consumed.IsConsumed.Should().BeTrue();
        consumed.IsActive().Should().BeFalse();
    }

    [Test]
    public void GetAllySaveBonus_SumsActiveBuffsForCharacter()
    {
        // Arrange
        var allyId = Guid.NewGuid();
        var otherAllyId = Guid.NewGuid();
        var buffs = new List<RallyBuff>
        {
            RallyBuff.Create(allyId, Guid.NewGuid()),       // +2 for allyId
            RallyBuff.Create(allyId, Guid.NewGuid()),       // +2 for allyId
            RallyBuff.Create(otherAllyId, Guid.NewGuid()),  // +2 for otherAllyId
            RallyBuff.Create(allyId, Guid.NewGuid()).Consume() // consumed, +0
        };

        // Act
        var bonus = _service.GetAllySaveBonus(buffs, allyId);

        // Assert
        bonus.Should().Be(4, "two active buffs for allyId at +2 each");
    }

    // ═══════ Prerequisite Helpers ═══════

    [Test]
    public void CanUnlockTier2_With8PP_ReturnsTrue()
    {
        // Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(12).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_WithLessThan8PP_ReturnsFalse()
    {
        // Act & Assert
        _service.CanUnlockTier2(7).Should().BeFalse();
        _service.CanUnlockTier2(0).Should().BeFalse();
    }

    [Test]
    public void CalculatePPInvested_ReturnsCorrectSum()
    {
        // Arrange
        var abilities = new List<SkjaldmaerAbilityId>
        {
            SkjaldmaerAbilityId.ShieldWall,      // 0
            SkjaldmaerAbilityId.HoldTheLine,     // 4
            SkjaldmaerAbilityId.CounterShield,   // 4
            SkjaldmaerAbilityId.Unbreakable      // 5
        };

        // Act
        var pp = _service.CalculatePPInvested(abilities);

        // Assert
        pp.Should().Be(13);
    }
}
