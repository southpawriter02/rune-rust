using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for <see cref="HuntersEyeResult"/> value object.
/// Tests cover evaluation logic, cover penalty calculations, and description generation.
/// </summary>
/// <remarks>
/// <para>Hunter's Eye is a Tier 2 passive ability for the Veiðimaðr (Hunter) specialization.
/// It evaluates whether a target's cover should be ignored during a ranged attack.</para>
/// <para>Key behaviors tested:</para>
/// <list type="bullet">
/// <item><see cref="HuntersEyeResult.ShouldIgnoreCover"/> — true for Partial, false for Full/None</item>
/// <item><see cref="HuntersEyeResult.GetCoverPenalty"/> — 2 for Partial, 0 for others</item>
/// <item><see cref="HuntersEyeResult.GetDescription"/> — narrative text for combat logging</item>
/// <item><see cref="HuntersEyeResult.WasCoverIgnored"/> — accessor for CoverIgnored property</item>
/// </list>
/// <para>Introduced in v0.20.7b. Coherent path — zero Corruption risk.</para>
/// </remarks>
[TestFixture]
public class HuntersEyeResultTests
{
    // ===== ShouldIgnoreCover Tests =====

    [Test]
    public void ShouldIgnoreCover_PartialCover_ReturnsTrue()
    {
        // Act
        var result = HuntersEyeResult.ShouldIgnoreCover(CoverType.Partial);

        // Assert
        result.Should().BeTrue(
            "Hunter's Eye ignores partial cover penalties on ranged attacks");
    }

    [Test]
    public void ShouldIgnoreCover_FullCover_ReturnsFalse()
    {
        // Act
        var result = HuntersEyeResult.ShouldIgnoreCover(CoverType.Full);

        // Assert
        result.Should().BeFalse(
            "Hunter's Eye cannot bypass full cover — target is not attackable");
    }

    [Test]
    public void ShouldIgnoreCover_NoCover_ReturnsFalse()
    {
        // Act
        var result = HuntersEyeResult.ShouldIgnoreCover(CoverType.None);

        // Assert
        result.Should().BeFalse(
            "no cover means no penalty to ignore");
    }

    // ===== GetCoverPenalty Tests =====

    [Test]
    public void GetCoverPenalty_PartialCover_Returns2()
    {
        // Act
        var penalty = HuntersEyeResult.GetCoverPenalty(CoverType.Partial);

        // Assert
        penalty.Should().Be(2,
            "partial cover normally imposes a +2 AC penalty that Hunter's Eye negates");
    }

    [Test]
    public void GetCoverPenalty_FullCover_Returns0()
    {
        // Act
        var penalty = HuntersEyeResult.GetCoverPenalty(CoverType.Full);

        // Assert
        penalty.Should().Be(0,
            "full cover blocks attacks entirely — no numeric penalty to negate");
    }

    [Test]
    public void GetCoverPenalty_NoCover_Returns0()
    {
        // Act
        var penalty = HuntersEyeResult.GetCoverPenalty(CoverType.None);

        // Assert
        penalty.Should().Be(0,
            "no cover means no AC penalty");
    }

    // ===== GetDescription Tests =====

    [Test]
    public void GetDescription_CoverIgnored_IncludesIgnoredText()
    {
        // Arrange
        var result = new HuntersEyeResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Draugr Archer",
            OriginalCoverType = CoverType.Partial,
            CoverIgnored = true,
            BonusFromCoverIgnored = 2,
            Distance = 8
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("ignored",
            "description should indicate that the cover was successfully bypassed");
        description.Should().Contain("Draugr Archer");
        description.Should().Contain("+2");
        description.Should().Contain("8 spaces");
    }

    [Test]
    public void GetDescription_FullCover_IndicatesNotAttackable()
    {
        // Arrange
        var result = new HuntersEyeResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Troll Warden",
            OriginalCoverType = CoverType.Full,
            CoverIgnored = false,
            BonusFromCoverIgnored = 0,
            Distance = 5
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("full cover",
            "description should explain why the attack is blocked");
        description.Should().Contain("Troll Warden");
    }

    [Test]
    public void GetDescription_NoCover_IndicatesNoEffect()
    {
        // Arrange
        var result = new HuntersEyeResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Corrupted Wolf",
            OriginalCoverType = CoverType.None,
            CoverIgnored = false,
            BonusFromCoverIgnored = 0,
            Distance = 6
        };

        // Act
        var description = result.GetDescription();

        // Assert
        description.Should().Contain("no cover",
            "description should indicate there was nothing to bypass");
        description.Should().Contain("Corrupted Wolf");
    }

    // ===== WasCoverIgnored Tests =====

    [Test]
    public void WasCoverIgnored_CoverIgnoredTrue_ReturnsTrue()
    {
        // Arrange
        var result = new HuntersEyeResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            OriginalCoverType = CoverType.Partial,
            CoverIgnored = true,
            BonusFromCoverIgnored = 2,
            Distance = 5
        };

        // Act & Assert
        result.WasCoverIgnored().Should().BeTrue();
    }

    [Test]
    public void WasCoverIgnored_CoverIgnoredFalse_ReturnsFalse()
    {
        // Arrange
        var result = new HuntersEyeResult
        {
            HunterId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            TargetName = "Target",
            OriginalCoverType = CoverType.Full,
            CoverIgnored = false,
            BonusFromCoverIgnored = 0,
            Distance = 5
        };

        // Act & Assert
        result.WasCoverIgnored().Should().BeFalse();
    }
}
