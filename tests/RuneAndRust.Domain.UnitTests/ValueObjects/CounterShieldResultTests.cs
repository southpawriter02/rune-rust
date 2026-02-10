// ═══════════════════════════════════════════════════════════════════════════════
// CounterShieldResultTests.cs
// Unit tests for the CounterShieldResult value object.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class CounterShieldResultTests
{
    private static readonly Guid SkjaldmaerId = Guid.NewGuid();
    private static readonly Guid AttackerId = Guid.NewGuid();

    [Test]
    public void Create_WithValidDamageRoll_InitializesCorrectly()
    {
        // Arrange & Act
        var result = CounterShieldResult.Create(SkjaldmaerId, AttackerId, 4);

        // Assert
        result.SkjaldmaerId.Should().Be(SkjaldmaerId);
        result.AttackerId.Should().Be(AttackerId);
        result.DamageRoll.Should().Be(4);
        result.ExecutedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ShouldApplyDamage().Should().BeTrue();
    }

    [Test]
    public void Create_WithMinDamageRoll_Succeeds()
    {
        // Arrange & Act
        var result = CounterShieldResult.Create(SkjaldmaerId, AttackerId, 1);

        // Assert
        result.DamageRoll.Should().Be(1);
        result.ShouldApplyDamage().Should().BeTrue();
    }

    [Test]
    public void Create_WithMaxDamageRoll_Succeeds()
    {
        // Arrange & Act
        var result = CounterShieldResult.Create(SkjaldmaerId, AttackerId, 6);

        // Assert
        result.DamageRoll.Should().Be(6);
    }

    [Test]
    public void Create_WithDamageRollBelowMin_ThrowsArgumentOutOfRange()
    {
        // Arrange & Act
        var act = () => CounterShieldResult.Create(SkjaldmaerId, AttackerId, 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Create_WithDamageRollAboveMax_ThrowsArgumentOutOfRange()
    {
        // Arrange & Act
        var act = () => CounterShieldResult.Create(SkjaldmaerId, AttackerId, 7);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ShouldApplyDamage_AlwaysReturnsTrue()
    {
        // Arrange — test across all valid rolls
        for (var roll = 1; roll <= 6; roll++)
        {
            var result = CounterShieldResult.Create(SkjaldmaerId, AttackerId, roll);

            // Act & Assert
            result.ShouldApplyDamage().Should().BeTrue(
                $"Counter-Shield damage is unconditional for roll {roll}");
        }
    }
}
