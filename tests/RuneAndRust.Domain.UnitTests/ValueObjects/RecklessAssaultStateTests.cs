using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="RecklessAssaultState"/> value object.
/// </summary>
[TestFixture]
public class RecklessAssaultStateTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesCorrectly()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var state = RecklessAssaultState.Create(characterId);

        // Assert
        state.CharacterId.Should().Be(characterId);
        state.TurnsActive.Should().Be(0);
        state.CorruptionAccrued.Should().Be(0);
        state.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        state.StateId.Should().NotBeEmpty();
    }

    // ===== Attack Bonus Tests =====

    [Test]
    public void GetCurrentAttackBonus_BaseWithNoRage_ReturnsFour()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act
        var bonus = state.GetCurrentAttackBonus(0);

        // Assert
        bonus.Should().Be(4);
    }

    [Test]
    public void GetCurrentAttackBonus_ScalesWithRage()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act & Assert — verify scaling at key breakpoints
        state.GetCurrentAttackBonus(0).Should().Be(4);   // 4 + 0/20 = 4
        state.GetCurrentAttackBonus(19).Should().Be(4);  // 4 + 0 = 4
        state.GetCurrentAttackBonus(20).Should().Be(5);  // 4 + 1 = 5
        state.GetCurrentAttackBonus(40).Should().Be(6);  // 4 + 2 = 6
        state.GetCurrentAttackBonus(60).Should().Be(7);  // 4 + 3 = 7
        state.GetCurrentAttackBonus(80).Should().Be(8);  // 4 + 4 = 8
        state.GetCurrentAttackBonus(100).Should().Be(9); // 4 + 5 = 9
    }

    // ===== Defense Penalty Tests =====

    [Test]
    public void DefensePenalty_IsAlwaysMinusTwo()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Assert
        state.DefensePenalty.Should().Be(-2);
    }

    // ===== Corruption Generation Tests =====

    [Test]
    public void GeneratesCorruptionThisTurn_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act & Assert
        state.GeneratesCorruptionThisTurn(79).Should().BeFalse();
        state.GeneratesCorruptionThisTurn(0).Should().BeFalse();
    }

    [Test]
    public void GeneratesCorruptionThisTurn_AtOrAboveThreshold_ReturnsTrue()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act & Assert
        state.GeneratesCorruptionThisTurn(80).Should().BeTrue();
        state.GeneratesCorruptionThisTurn(85).Should().BeTrue();
        state.GeneratesCorruptionThisTurn(100).Should().BeTrue();
    }

    // ===== Tick Tests =====

    [Test]
    public void Tick_IncrementsTurnsActive()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act
        state.Tick();
        state.Tick();
        state.Tick();

        // Assert
        state.TurnsActive.Should().Be(3);
    }

    [Test]
    public void Tick_AccumulatesCorruption()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act — 3 turns with corruption, 1 without
        state.Tick(1);
        state.Tick(1);
        state.Tick(0);
        state.Tick(1);

        // Assert
        state.TurnsActive.Should().Be(4);
        state.CorruptionAccrued.Should().Be(3);
    }

    // ===== IsActive Tests =====

    [Test]
    public void IsActive_AlwaysReturnsTrue()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act & Assert — always true while the state object exists
        state.IsActive().Should().BeTrue();
        state.End();
        state.IsActive().Should().BeTrue(); // Still true; lifecycle managed by service
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_IncludesCorruptionRisk_WhenEnraged()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act
        var description = state.GetDescription(85);

        // Assert
        description.Should().Contain("+8 Attack"); // 4 + 85/20 = 4 + 4 = 8
        description.Should().Contain("-2 Defense");
        description.Should().Contain("[Corruption Risk]");
    }

    [Test]
    public void GetDescription_NoCorruptionWarning_WhenBelowThreshold()
    {
        // Arrange
        var state = RecklessAssaultState.Create(Guid.NewGuid());

        // Act
        var description = state.GetDescription(40);

        // Assert
        description.Should().Contain("+6 Attack"); // 4 + 40/20 = 4 + 2 = 6
        description.Should().NotContain("[Corruption Risk]");
    }
}
