// ═══════════════════════════════════════════════════════════════════════════════
// RallyBuffTests.cs
// Unit tests for the RallyBuff value object.
// Version: 0.20.1b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

[TestFixture]
public class RallyBuffTests
{
    private static readonly Guid AllyId = Guid.NewGuid();
    private static readonly Guid CasterId = Guid.NewGuid();

    [Test]
    public void Create_InitializesWithCorrectDefaults()
    {
        // Arrange & Act
        var buff = RallyBuff.Create(AllyId, CasterId);

        // Assert
        buff.AffectedCharacterId.Should().Be(AllyId);
        buff.CasterCharacterId.Should().Be(CasterId);
        buff.SaveBonus.Should().Be(2);
        buff.IsConsumed.Should().BeFalse();
        buff.IsActive().Should().BeTrue();
        buff.AppliedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Consume_MarksBuffAsConsumed()
    {
        // Arrange
        var buff = RallyBuff.Create(AllyId, CasterId);

        // Act
        var consumed = buff.Consume();

        // Assert
        consumed.IsConsumed.Should().BeTrue();
        consumed.IsActive().Should().BeFalse();

        // Original should be unchanged (immutability)
        buff.IsConsumed.Should().BeFalse();
        buff.IsActive().Should().BeTrue();
    }

    [Test]
    public void GetBonusForCharacter_WhenActiveAndMatchingId_ReturnsBonus()
    {
        // Arrange
        var buff = RallyBuff.Create(AllyId, CasterId);

        // Act & Assert
        buff.GetBonusForCharacter(AllyId).Should().Be(2);
    }

    [Test]
    public void GetBonusForCharacter_WhenActiveAndWrongId_ReturnsZero()
    {
        // Arrange
        var buff = RallyBuff.Create(AllyId, CasterId);
        var wrongId = Guid.NewGuid();

        // Act & Assert
        buff.GetBonusForCharacter(wrongId).Should().Be(0);
    }

    [Test]
    public void GetBonusForCharacter_WhenConsumed_ReturnsZero()
    {
        // Arrange
        var buff = RallyBuff.Create(AllyId, CasterId).Consume();

        // Act & Assert
        buff.GetBonusForCharacter(AllyId).Should().Be(0);
    }
}
