using FluentAssertions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="FuryCleavesResult"/> value object.
/// </summary>
[TestFixture]
public class FuryCleavesResultTests
{
    // ===== Init Property Tests =====

    [Test]
    public void FuryCleavesResult_InitProperties_StoreCorrectValues()
    {
        // Arrange
        var hitIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var missIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = new FuryCleavesResult
        {
            AttackRoll = 17,
            TargetsHit = hitIds,
            TargetsMissed = missIds,
            DamageDealt = 19
        };

        // Assert
        result.AttackRoll.Should().Be(17);
        result.TargetsHit.Should().BeEquivalentTo(hitIds);
        result.TargetsMissed.Should().BeEquivalentTo(missIds);
        result.DamageDealt.Should().Be(19);
    }

    // ===== Computed Property Tests =====

    [Test]
    public void TotalDamageAcrossTargets_CalculatesCorrectly()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            DamageDealt = 18,
            TargetsHit = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act & Assert
        result.TotalDamageAcrossTargets.Should().Be(54); // 18 × 3
    }

    [Test]
    public void TotalDamageAcrossTargets_ZeroHits_ReturnsZero()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            DamageDealt = 18,
            TargetsHit = new List<Guid>()
        };

        // Act & Assert
        result.TotalDamageAcrossTargets.Should().Be(0);
    }

    // ===== Default Value Tests =====

    [Test]
    public void RageSpent_DefaultsToThirty()
    {
        // Arrange & Act
        var result = new FuryCleavesResult();

        // Assert
        result.RageSpent.Should().Be(30);
    }

    [Test]
    public void CorruptionTriggered_DefaultsToTrue()
    {
        // Arrange & Act
        var result = new FuryCleavesResult();

        // Assert
        result.CorruptionTriggered.Should().BeTrue();
    }

    [Test]
    public void CorruptionAmount_AlwaysReturnsOne()
    {
        // Arrange & Act
        var result = new FuryCleavesResult();

        // Assert
        result.CorruptionAmount.Should().Be(1);
    }

    // ===== Method Tests =====

    [Test]
    public void GetHitCount_ReturnsCorrectCount()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            TargetsHit = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act & Assert
        result.GetHitCount().Should().Be(2);
    }

    [Test]
    public void GetMissCount_ReturnsCorrectCount()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            TargetsMissed = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act & Assert
        result.GetMissCount().Should().Be(3);
    }

    [Test]
    public void WasEffective_WithHits_ReturnsTrue()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            TargetsHit = new List<Guid> { Guid.NewGuid() }
        };

        // Act & Assert
        result.WasEffective().Should().BeTrue();
    }

    [Test]
    public void WasEffective_WithNoHits_ReturnsFalse()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            TargetsHit = new List<Guid>()
        };

        // Act & Assert
        result.WasEffective().Should().BeFalse();
    }

    [Test]
    public void GetDamageBreakdown_WithHits_ShowsMultiplication()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            DamageDealt = 19,
            TargetsHit = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var breakdown = result.GetDamageBreakdown();

        // Assert
        breakdown.Should().Be("19 × 3 targets = 57 total damage");
    }

    [Test]
    public void GetDamageBreakdown_WithNoHits_ShowsNoTargetsHit()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            DamageDealt = 19,
            TargetsHit = new List<Guid>()
        };

        // Act
        var breakdown = result.GetDamageBreakdown();

        // Assert
        breakdown.Should().Be("No targets hit");
    }

    [Test]
    public void GetResultString_WithHitsAndMisses_FormatsCorrectly()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            AttackRoll = 14,
            DamageDealt = 18,
            TargetsHit = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            TargetsMissed = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var resultString = result.GetResultString();

        // Assert
        resultString.Should().Contain("Fury of the Forlorn");
        resultString.Should().Contain("Attack Roll: 14");
        resultString.Should().Contain("18 × 2 targets = 36 total damage");
        resultString.Should().Contain("(1 missed)");
        resultString.Should().Contain("[+1 CORRUPTION]");
    }

    [Test]
    public void GetResultString_AllMissed_ShowsAllMissed()
    {
        // Arrange
        var result = new FuryCleavesResult
        {
            AttackRoll = 3,
            DamageDealt = 18,
            TargetsHit = new List<Guid>(),
            TargetsMissed = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var resultString = result.GetResultString();

        // Assert
        resultString.Should().Contain("ALL MISSED!");
        resultString.Should().Contain("(2 missed)");
        resultString.Should().Contain("[+1 CORRUPTION]");
    }
}
