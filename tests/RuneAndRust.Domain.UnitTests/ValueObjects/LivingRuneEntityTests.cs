// ═══════════════════════════════════════════════════════════════════════════════
// LivingRuneEntityTests.cs
// Unit tests for the LivingRuneEntity value object covering creation,
// damage, duration ticking, movement, attack rolls, and display.
// Version: 0.20.2c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class LivingRuneEntityTests
{
    private static readonly Guid TestOwnerId = Guid.NewGuid();
    private static readonly (int X, int Y) TestPosition = (5, 3);

    // ═══════ Creation Tests ═══════

    [Test]
    public void Create_WithValidInputs_CreatesEntityWithDefaults()
    {
        // Act
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Assert
        rune.EntityId.Should().NotBe(Guid.Empty);
        rune.OwnerId.Should().Be(TestOwnerId);
        rune.CurrentHp.Should().Be(LivingRuneEntity.DefaultMaxHp);
        rune.MaxHp.Should().Be(LivingRuneEntity.DefaultMaxHp);
        rune.Defense.Should().Be(LivingRuneEntity.DefaultDefense);
        rune.AttackBonus.Should().Be(LivingRuneEntity.DefaultAttackBonus);
        rune.DamageDice.Should().Be(LivingRuneEntity.DefaultDamageDice);
        rune.Movement.Should().Be(LivingRuneEntity.DefaultMovement);
        rune.Position.Should().Be(TestPosition);
        rune.TurnsRemaining.Should().Be(LivingRuneEntity.DefaultDuration);
        rune.OriginalDuration.Should().Be(LivingRuneEntity.DefaultDuration);
        rune.IsActive.Should().BeTrue();
        rune.IsDestroyed.Should().BeFalse();
        rune.IsExpired.Should().BeFalse();
    }

    [Test]
    public void Create_TwoEntities_HaveUniqueEntityIds()
    {
        // Act
        var rune1 = LivingRuneEntity.Create(TestOwnerId, TestPosition);
        var rune2 = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Assert
        rune1.EntityId.Should().NotBe(rune2.EntityId);
    }

    // ═══════ Damage Tests ═══════

    [Test]
    public void TakeDamage_WithPartialDamage_ReducesHp()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var damaged = rune.TakeDamage(4);

        // Assert
        damaged.CurrentHp.Should().Be(6);
        damaged.IsDestroyed.Should().BeFalse();
        damaged.IsActive.Should().BeTrue();
        rune.CurrentHp.Should().Be(10, "original should be unchanged (immutable)");
    }

    [Test]
    public void TakeDamage_WithLethalDamage_ClampsToZeroAndDestroyed()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var destroyed = rune.TakeDamage(15);

        // Assert
        destroyed.CurrentHp.Should().Be(0);
        destroyed.IsDestroyed.Should().BeTrue();
        destroyed.IsActive.Should().BeFalse();
    }

    [Test]
    public void TakeDamage_WithNegativeDamage_ThrowsArgumentOutOfRange()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var act = () => rune.TakeDamage(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════ Tick Tests ═══════

    [Test]
    public void Tick_WithTurnsRemaining_DecrementsDuration()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var ticked = rune.Tick();

        // Assert
        ticked.TurnsRemaining.Should().Be(4);
        ticked.IsActive.Should().BeTrue();
        ticked.IsExpired.Should().BeFalse();
        rune.TurnsRemaining.Should().Be(5, "original should be unchanged (immutable)");
    }

    [Test]
    public void Tick_AtExpiry_SetsInactiveAndExpired()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Tick down to 1 remaining, then tick to 0
        var current = rune;
        for (var i = 0; i < LivingRuneEntity.DefaultDuration; i++)
        {
            current = current.Tick();
        }

        // Assert
        current.TurnsRemaining.Should().Be(0);
        current.IsActive.Should().BeFalse();
        current.IsExpired.Should().BeTrue();
        current.IsDestroyed.Should().BeFalse("expiry is different from destruction");
    }

    // ═══════ Movement Tests ═══════

    [Test]
    public void MoveTo_ValidPosition_UpdatesPosition()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);
        var newPosition = (8, 7);

        // Act
        var moved = rune.MoveTo(newPosition);

        // Assert
        moved.Position.Should().Be(newPosition);
        rune.Position.Should().Be(TestPosition, "original should be unchanged (immutable)");
    }

    // ═══════ Attack Tests ═══════

    [Test]
    public void RollAttackDamage_WithSeededRandom_ReturnsValueInRange()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);
        var seeded = new Random(42);

        // Act
        var damage = rune.RollAttackDamage(seeded);

        // Assert
        damage.Should().BeInRange(1, 8);
    }

    // ═══════ Display Tests ═══════

    [Test]
    public void GetStatusDisplay_ActiveEntity_ContainsStatusAndHp()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var display = rune.GetStatusDisplay();

        // Assert
        display.Should().Contain("Living Rune");
        display.Should().Contain("ACTIVE");
        display.Should().Contain("10/10 HP");
        display.Should().Contain("5 turns");
    }

    [Test]
    public void ToString_ReturnsHumanReadableRepresentation()
    {
        // Arrange
        var rune = LivingRuneEntity.Create(TestOwnerId, TestPosition);

        // Act
        var str = rune.ToString();

        // Assert
        str.Should().Contain("Living Rune");
        str.Should().Contain("(5, 3)");
        str.Should().Contain("10/10 HP");
        str.Should().Contain("+4 ATK");
        str.Should().Contain("1d8 DMG");
    }

    // ═══════ Constant Tests ═══════

    [Test]
    public void Constants_MatchDesignSpecification()
    {
        // Assert (verifies design spec values)
        LivingRuneEntity.DefaultMaxHp.Should().Be(10);
        LivingRuneEntity.DefaultDefense.Should().Be(12);
        LivingRuneEntity.DefaultAttackBonus.Should().Be(4);
        LivingRuneEntity.DefaultDamageDice.Should().Be("1d8");
        LivingRuneEntity.DefaultMovement.Should().Be(4);
        LivingRuneEntity.DefaultDuration.Should().Be(5);
        LivingRuneEntity.SummonCount.Should().Be(2);
        LivingRuneEntity.DamageDiceCount.Should().Be(1);
        LivingRuneEntity.DamageDiceSides.Should().Be(8);
    }
}
