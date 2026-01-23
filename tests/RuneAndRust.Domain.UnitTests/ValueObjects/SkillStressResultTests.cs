namespace RuneAndRust.Domain.UnitTests.ValueObjects;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="SkillStressResult"/> value object.
/// </summary>
[TestFixture]
public class SkillStressResultTests
{
    [Test]
    public void None_ReturnsZeroStressResult()
    {
        // Act
        var result = SkillStressResult.None();

        // Assert
        result.TotalStress.Should().Be(0);
        result.CorruptionStress.Should().Be(0);
        result.FumbleStress.Should().Be(0);
        result.Source.Should().Be(StressSource.None);
        result.HasStress.Should().BeFalse();
        result.TriggersBreakingPoint.Should().BeFalse();
        result.CorruptionTier.Should().Be(CorruptionTier.Normal);
    }

    [Test]
    [TestCase(CorruptionTier.Normal, 0)]
    [TestCase(CorruptionTier.Glitched, 2)]
    [TestCase(CorruptionTier.Blighted, 5)]
    [TestCase(CorruptionTier.Resonance, 10)]
    public void FromCorruption_ReturnsCorrectStress(CorruptionTier tier, int expectedStress)
    {
        // Act
        var result = SkillStressResult.FromCorruption(tier);

        // Assert
        result.TotalStress.Should().Be(expectedStress);
        result.CorruptionStress.Should().Be(expectedStress);
        result.FumbleStress.Should().Be(0);
        result.CorruptionTier.Should().Be(tier);
    }

    [Test]
    [TestCase(CorruptionTier.Normal, 1)]
    [TestCase(CorruptionTier.Glitched, 2)]
    [TestCase(CorruptionTier.Blighted, 4)]
    [TestCase(CorruptionTier.Resonance, 8)]
    public void GetFumbleStress_ReturnsCorrectBonusStress(CorruptionTier tier, int expectedFumbleStress)
    {
        // Act
        var result = SkillStressResult.GetFumbleStress(tier);

        // Assert
        result.Should().Be(expectedFumbleStress);
    }

    [Test]
    public void FromFumble_CombinesCorruptionAndFumbleStress()
    {
        // Act
        var result = SkillStressResult.FromFumble(CorruptionTier.Glitched);

        // Assert
        result.TotalStress.Should().Be(4); // 2 corruption + 2 fumble
        result.CorruptionStress.Should().Be(2);
        result.FumbleStress.Should().Be(2);
        result.Source.Should().Be(StressSource.Fumble);
        result.HadFumble.Should().BeTrue();
    }

    [Test]
    public void FromFumble_InResonanceZone_TriggersBreakingPoint()
    {
        // Act
        var result = SkillStressResult.FromFumble(CorruptionTier.Resonance);

        // Assert
        result.TotalStress.Should().Be(18); // 10 corruption + 8 fumble
        result.TriggersBreakingPoint.Should().BeTrue();
    }

    [Test]
    public void InCorruptedArea_TrueForNonNormalTiers()
    {
        // Arrange & Act
        var normal = SkillStressResult.FromCorruption(CorruptionTier.Normal);
        var glitched = SkillStressResult.FromCorruption(CorruptionTier.Glitched);

        // Assert
        normal.InCorruptedArea.Should().BeFalse();
        glitched.InCorruptedArea.Should().BeTrue();
    }

    [Test]
    public void ToDescription_FormatsStressCorrectly()
    {
        // Arrange
        var result = SkillStressResult.FromFumble(CorruptionTier.Glitched);

        // Act
        var description = result.ToDescription();

        // Assert
        description.Should().Contain("4");
        description.Should().Contain("Glitched");
        description.Should().Contain("fumble");
    }
}
