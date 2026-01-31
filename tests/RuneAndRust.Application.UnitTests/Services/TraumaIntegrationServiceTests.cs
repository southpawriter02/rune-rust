namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="TraumaIntegrationService"/>.
/// </summary>
[TestFixture]
public class TraumaIntegrationServiceTests
{
    private Mock<ILogger<TraumaIntegrationService>> _loggerMock = null!;
    private TraumaIntegrationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<TraumaIntegrationService>>();
        _service = new TraumaIntegrationService(_loggerMock.Object);
    }

    [Test]
    public void CalculateSkillStress_NormalArea_NoFumble_ReturnsZeroStress()
    {
        // Arrange
        var context = CreateContext(CorruptionTier.Normal);
        var outcome = CreateOutcome(isFumble: false);

        // Act
        var result = _service.CalculateSkillStress(context, outcome);

        // Assert
        result.TotalStress.Should().Be(0);
        result.HasStress.Should().BeFalse();
        result.Source.Should().Be(StressSource.Exploration);
    }

    [Test]
    [TestCase(CorruptionTier.Glitched, 2)]
    [TestCase(CorruptionTier.Blighted, 5)]
    [TestCase(CorruptionTier.Resonance, 10)]
    public void CalculateSkillStress_CorruptedArea_ReturnsCorruptionStress(
        CorruptionTier tier, int expectedStress)
    {
        // Arrange
        var context = CreateContext(tier);
        var outcome = CreateOutcome(isFumble: false);

        // Act
        var result = _service.CalculateSkillStress(context, outcome);

        // Assert
        result.TotalStress.Should().Be(expectedStress);
        result.CorruptionStress.Should().Be(expectedStress);
        result.Source.Should().Be(StressSource.Corruption);
    }

    [Test]
    public void CalculateSkillStress_FumbleInCorruptedArea_AddsFumbleStress()
    {
        // Arrange
        var context = CreateContext(CorruptionTier.Glitched);
        var outcome = CreateOutcome(isFumble: true);

        // Act
        var result = _service.CalculateSkillStress(context, outcome);

        // Assert
        result.TotalStress.Should().Be(4); // 2 corruption + 2 fumble
        result.CorruptionStress.Should().Be(2);
        result.FumbleStress.Should().Be(2);
        result.Source.Should().Be(StressSource.Combat);
    }

    [Test]
    public void CalculateExtendedCheckStress_AccumulatesStressPerStep()
    {
        // Arrange
        var context = CreateContext(CorruptionTier.Glitched);

        // Act
        var result = _service.CalculateExtendedCheckStress(context, stepCount: 3, fumbleCount: 1);

        // Assert
        result.TotalStress.Should().Be(8); // (2 * 3) + (2 * 1)
        result.CorruptionStress.Should().Be(6);
        result.FumbleStress.Should().Be(2);
    }

    [Test]
    public void CalculateExtendedCheckStress_ResonanceZone_TriggersBreakingPoint()
    {
        // Arrange
        var context = CreateContext(CorruptionTier.Resonance);

        // Act
        var result = _service.CalculateExtendedCheckStress(context, stepCount: 1, fumbleCount: 0);

        // Assert
        result.TotalStress.Should().Be(10);
        result.TriggersBreakingPoint.Should().BeTrue();
    }

    [Test]
    public void CalculateObjectInteractionStress_CorruptedObject_ReturnsStress()
    {
        // Act
        var result = _service.CalculateObjectInteractionStress(
            CorruptionTier.Blighted, isFumble: false);

        // Assert
        result.TotalStress.Should().Be(5);
        result.Source.Should().Be(StressSource.Exploration);
    }

    private static SkillContext CreateContext(CorruptionTier tier)
    {
        var envModifiers = tier == CorruptionTier.Normal
            ? Array.Empty<EnvironmentModifier>()
            : new[] { EnvironmentModifier.FromCorruption(tier) };

        return new SkillContext(
            equipmentModifiers: Array.Empty<EquipmentModifier>(),
            situationalModifiers: Array.Empty<SituationalModifier>(),
            environmentModifiers: envModifiers,
            targetModifiers: Array.Empty<TargetModifier>(),
            appliedStatuses: Array.Empty<string>());
    }

    private static OutcomeDetails CreateOutcome(bool isFumble)
    {
        return new OutcomeDetails(
            outcomeType: isFumble ? SkillOutcome.CriticalFailure : SkillOutcome.FullSuccess,
            margin: isFumble ? -2 : 2,
            isFumble: isFumble,
            isCritical: false);
    }
}
