using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for DiceDescriptorService.
/// </summary>
[TestFixture]
public class DiceDescriptorServiceTests
{
    private Mock<IGameConfigurationProvider> _configProviderMock = null!;
    private DiceDescriptorService _service = null!;
    private Random _seededRandom = null!;

    [SetUp]
    public void SetUp()
    {
        _configProviderMock = new Mock<IGameConfigurationProvider>();
        _seededRandom = new Random(42); // Deterministic for testing
    }

    private void SetupDescriptors(Dictionary<string, IReadOnlyList<string>> descriptors)
    {
        _configProviderMock.Setup(p => p.GetDiceDescriptors())
            .Returns(descriptors);
        _service = new DiceDescriptorService(
            _configProviderMock.Object,
            NullLogger<DiceDescriptorService>.Instance,
            _seededRandom);
    }

    [Test]
    public void GetDescriptor_WithValidCategory_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_max"] = new List<string> { "Perfect roll!", "Excellent!" }
        };
        SetupDescriptors(descriptors);

        // Act
        var result = _service.GetDescriptor("dice.natural_max");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOneOf("Perfect roll!", "Excellent!");
    }

    [Test]
    public void GetDescriptor_WithUnknownCategory_ReturnsNull()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_max"] = new List<string> { "Perfect!" }
        };
        SetupDescriptors(descriptors);

        // Act
        var result = _service.GetDescriptor("unknown.category");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetDescriptor_WithEmptyCategory_ReturnsNull()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_max"] = new List<string>()
        };
        SetupDescriptors(descriptors);

        // Act
        var result = _service.GetDescriptor("dice.natural_max");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetDiceRollDescriptor_WithCriticalSuccess_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_max"] = new List<string> { "Fortune smiles!" }
        };
        SetupDescriptors(descriptors);

        // v0.15.0a: IsCriticalSuccess requires 5+ net successes
        // Roll 5d10 all showing 8+ to get 5 successes
        var pool = new DicePool(5, Domain.Enums.DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 8, 9, 10, 8, 9 }); // 5 successes

        // Act
        var descriptor = _service.GetDiceRollDescriptor(result);

        // Assert
        descriptor.Should().Be("Fortune smiles!");
        result.IsCriticalSuccess.Should().BeTrue();
    }

    [Test]
    public void GetDiceRollDescriptor_WithNaturalOne_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["dice.natural_one"] = new List<string> { "Bad luck!" }
        };
        SetupDescriptors(descriptors);

        var pool = DicePool.D10();
        var result = new DiceRollResult(pool, new[] { 1 }); // Natural 1

        // Act
        var descriptor = _service.GetDiceRollDescriptor(result);

        // Assert
        descriptor.Should().Be("Bad luck!");
    }

    [Test]
    public void GetSkillCheckDescriptor_WithCriticalSuccess_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["skill.critical_success"] = new List<string> { "Masterfully done!" }
        };
        SetupDescriptors(descriptors);

        // v0.15.0a: IsCriticalSuccess requires 5+ net successes
        // Roll 5d10 all showing 8+ to get 5 successes
        var pool = new DicePool(5, Domain.Enums.DiceType.D10);
        var diceResult = new DiceRollResult(pool, new[] { 8, 9, 10, 8, 9 }); // 5 successes
        var result = new SkillCheckResult(
            "perception", "Perception", diceResult, 2, 0, 12, "Moderate");

        // Act
        var descriptor = _service.GetSkillCheckDescriptor(result);

        // Assert
        descriptor.Should().Be("Masterfully done!");
        result.IsCriticalSuccess.Should().BeTrue();
    }

    [Test]
    public void GetSkillCheckDescriptor_WithCriticalFailure_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["skill.critical_failure"] = new List<string> { "Complete disaster!" }
        };
        SetupDescriptors(descriptors);

        // Natural 1 on d10 triggers critical failure
        var pool = DicePool.D10();
        var diceResult = new DiceRollResult(pool, new[] { 1 });
        var result = new SkillCheckResult(
            "stealth", "Stealth", diceResult, 2, 0, 15, "Hard");

        // Act
        var descriptor = _service.GetSkillCheckDescriptor(result);

        // Assert
        descriptor.Should().Be("Complete disaster!");
        result.IsCriticalFailure.Should().BeTrue();
    }

    [Test]
    public void GetCombatCriticalDescriptor_CriticalHit_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["combat.critical_hit"] = new List<string> { "Devastating blow!" }
        };
        SetupDescriptors(descriptors);

        // Act
        var descriptor = _service.GetCombatCriticalDescriptor(isCriticalHit: true);

        // Assert
        descriptor.Should().Be("Devastating blow!");
    }

    [Test]
    public void GetCombatCriticalDescriptor_CriticalMiss_ReturnsDescriptor()
    {
        // Arrange
        var descriptors = new Dictionary<string, IReadOnlyList<string>>
        {
            ["combat.critical_miss"] = new List<string> { "Clumsy miss!" }
        };
        SetupDescriptors(descriptors);

        // Act
        var descriptor = _service.GetCombatCriticalDescriptor(isCriticalHit: false);

        // Assert
        descriptor.Should().Be("Clumsy miss!");
    }
}

