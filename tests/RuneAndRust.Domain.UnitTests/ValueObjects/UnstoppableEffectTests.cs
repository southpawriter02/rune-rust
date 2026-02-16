using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="UnstoppableEffect"/> value object.
/// </summary>
[TestFixture]
public class UnstoppableEffectTests
{
    // ===== Factory Tests =====

    [Test]
    public void Create_InitializesWithTwoTurns()
    {
        // Arrange
        var characterId = Guid.NewGuid();

        // Act
        var effect = UnstoppableEffect.Create(characterId);

        // Assert
        effect.CharacterId.Should().Be(characterId);
        effect.TurnsRemaining.Should().Be(2);
        effect.IsActive().Should().BeTrue();
        effect.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        effect.EffectId.Should().NotBeEmpty();
    }

    // ===== Movement Penalty Immunity Tests =====

    [Test]
    public void IgnoresPenalty_ForAllKnownTypes()
    {
        // Arrange
        var effect = UnstoppableEffect.Create(Guid.NewGuid());

        // Act & Assert — every MovementPenaltyType should be ignored
        effect.IgnoresPenalty(MovementPenaltyType.DifficultTerrain).Should().BeTrue();
        effect.IgnoresPenalty(MovementPenaltyType.Slow).Should().BeTrue();
        effect.IgnoresPenalty(MovementPenaltyType.Root).Should().BeTrue();
        effect.IgnoresPenalty(MovementPenaltyType.Water).Should().BeTrue();
        effect.IgnoresPenalty(MovementPenaltyType.Entangle).Should().BeTrue();
        effect.IgnoresPenalty(MovementPenaltyType.ForcedMovement).Should().BeTrue();
    }

    [Test]
    public void MovementPenaltiesIgnored_ContainsAllSixTypes()
    {
        // Arrange
        var effect = UnstoppableEffect.Create(Guid.NewGuid());

        // Assert
        effect.MovementPenaltiesIgnored.Should().HaveCount(6);
    }

    // ===== Tick / Duration Tests =====

    [Test]
    public void Tick_ReducesTurnsRemaining()
    {
        // Arrange
        var effect = UnstoppableEffect.Create(Guid.NewGuid());

        // Act & Assert — tick from 2 to 1
        effect.Tick();
        effect.TurnsRemaining.Should().Be(1);
        effect.IsActive().Should().BeTrue();

        // Act & Assert — tick from 1 to 0
        effect.Tick();
        effect.TurnsRemaining.Should().Be(0);
        effect.IsActive().Should().BeFalse();
    }

    [Test]
    public void Tick_DoesNotGoBelowZero()
    {
        // Arrange
        var effect = UnstoppableEffect.Create(Guid.NewGuid());
        effect.Tick(); // 1
        effect.Tick(); // 0

        // Act — extra tick
        effect.Tick();

        // Assert
        effect.TurnsRemaining.Should().Be(0);
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_ReturnsFormattedString()
    {
        // Arrange
        var effect = UnstoppableEffect.Create(Guid.NewGuid());

        // Act
        var description = effect.GetDescription();

        // Assert
        description.Should().Contain("Unstoppable (2 turns)");
        description.Should().Contain("Ignores");
    }
}
