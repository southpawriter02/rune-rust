// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderTier2AbilityServiceTests.cs
// Unit tests for Jötun-Reader Tier 2 abilities: Technical Memory, Exploit
// Weakness, Data Recovery, and prerequisite validation.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tests for <see cref="JotunReaderTier2AbilityService"/>.
/// </summary>
[TestFixture]
public class JotunReaderTier2AbilityServiceTests
{
    private Mock<ILogger<JotunReaderTier2AbilityService>> _mockLogger = null!;
    private Random _seededRandom = null!;
    private JotunReaderTier2AbilityService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JotunReaderTier2AbilityService>>();
        _seededRandom = new Random(42); // Deterministic seed
        _service = new JotunReaderTier2AbilityService(
            _mockLogger.Object, _seededRandom);
    }

    // ═══════ Technical Memory Tests ═══════

    [Test]
    public void RecordAndRecall_ExactMatch_ProvidesSolution()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var record = _service.RecordPuzzleSolution(
            "Mechanical", "Gear alignment puzzle with three rotors",
            "Align gears: top-right, bottom-left, center-top",
            locationId, "Bright Halls", 14);

        var memories = new List<TechnicalMemoryRecord> { record };

        // Act — exact same category + description
        var result = _service.AttemptRecall(
            "Mechanical",
            "Gear alignment puzzle with three rotors",
            memories);

        // Assert
        result.Found.Should().BeTrue();
        result.IsExactMatch.Should().BeTrue();
        result.SolutionOrHint.Should().Be(
            "Align gears: top-right, bottom-left, center-top");
        result.DCReduction.Should().Be(0);
    }

    [Test]
    public void RecordAndRecall_SimilarPattern_ReducesDC()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var record = _service.RecordPuzzleSolution(
            "Mechanical", "Gear alignment puzzle with three rotors and springs",
            "Align gears: top-right, bottom-left, center-top",
            locationId, "Bright Halls", 14);

        var memories = new List<TechnicalMemoryRecord> { record };

        // Act — same category, different but similar description (>50% keyword overlap)
        var result = _service.AttemptRecall(
            "Mechanical",
            "Gear alignment puzzle with four rotors and levers",
            memories);

        // Assert
        result.Found.Should().BeTrue();
        result.IsExactMatch.Should().BeFalse();
        result.SolutionOrHint.Should().NotBeNullOrEmpty();
        result.DCReduction.Should().Be(
            TechnicalMemoryRecord.SimilarPatternDCReduction);
    }

    [Test]
    public void AttemptRecall_NoMatch_ReturnsNotFound()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var record = _service.RecordPuzzleSolution(
            "Mechanical", "Gear alignment puzzle",
            "Align gears: top-right",
            locationId, "Bright Halls", 14);

        var memories = new List<TechnicalMemoryRecord> { record };

        // Act — completely different category
        var result = _service.AttemptRecall(
            "Electrical", "Wire crossover matrix", memories);

        // Assert
        result.Found.Should().BeFalse();
        result.IsExactMatch.Should().BeFalse();
        result.SolutionOrHint.Should().BeNull();
    }

    // ═══════ Exploit Weakness Tests ═══════

    [Test]
    public void AnalyzeEnemy_RevealsVulnerabilities()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var weakPoints = new[]
        {
            WeakPoint.Create("Head", "Exposed thin skull"),
            WeakPoint.Create("Left arm", "Scarred and weakened")
        };

        // Act
        var analysis = _service.AnalyzeEnemy(
            targetId, "Goblin Shaman",
            new[] { DamageType.Fire, DamageType.Psychic },
            new[] { DamageType.Cold },
            new[] { DamageType.Necrotic },
            weakPoints,
            new[] { "Moves to cover after casting" });

        // Assert
        analysis.Should().NotBeNull();
        analysis.TargetName.Should().Be("Goblin Shaman");
        analysis.Vulnerabilities.Should().HaveCount(2);
        analysis.Vulnerabilities.Should().Contain(DamageType.Fire);
        analysis.Vulnerabilities.Should().Contain(DamageType.Psychic);
        analysis.Resistances.Should().ContainSingle()
            .Which.Should().Be(DamageType.Cold);
        analysis.Immunities.Should().ContainSingle()
            .Which.Should().Be(DamageType.Necrotic);
        analysis.WeakPoints.Should().HaveCount(2);
        analysis.BehavioralPatterns.Should().ContainSingle();
    }

    [Test]
    public void CalculateExploitDamageBonus_WhenTargetingWeakness_GrantsBonus()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var analysis = WeaknessAnalysis.Create(
            targetId, "Test Enemy",
            new[] { DamageType.Fire },
            Array.Empty<DamageType>(),
            Array.Empty<DamageType>(),
            Array.Empty<WeakPoint>(),
            Array.Empty<string>());

        // Act
        var bonus = _service.CalculateExploitDamageBonus(
            DamageType.Fire, analysis);

        // Assert — seeded random produces 1-6, must be > 0
        bonus.Should().BeInRange(1, 6);
    }

    [Test]
    public void CalculateHitBonus_WithWeakPoint_GrantsBonus()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var analysis = WeaknessAnalysis.Create(
            targetId, "Test Enemy",
            Array.Empty<DamageType>(),
            Array.Empty<DamageType>(),
            Array.Empty<DamageType>(),
            new[] { WeakPoint.Create("Head", "Thin skull", 3) },
            Array.Empty<string>());

        // Act
        var bonus = JotunReaderTier2AbilityService.CalculateHitBonus(
            "Head", analysis);

        // Assert
        bonus.Should().Be(3);
    }

    // ═══════ Data Recovery Tests ═══════

    [Test]
    public void AttemptDataRecovery_Success_RecoversFragments()
    {
        // Arrange
        var terminalId = Guid.NewGuid();
        var fragments = new List<string>
        {
            "Override codes v2.14",
            "Project: SENTINEL",
            "Vault section: Black Vault",
            "Dates: 2156-03-15"
        };

        // DC for Corrupted = 16. Check of 18 = success (but not critical)
        // Act
        var (data, bonusInsight) = _service.AttemptDataRecovery(
            terminalId, "Main Server",
            TerminalState.Corrupted, 18, fragments);

        // Assert
        data.Should().NotBeNull();
        data!.SourceTerminalName.Should().Be("Main Server");
        data.GetFragmentCount().Should().BeGreaterThan(0);
        data.IsComplete.Should().BeFalse();
        bonusInsight.Should().BeFalse();
    }

    [Test]
    public void AttemptDataRecovery_CriticalSuccess_GeneratesBonusInsight()
    {
        // Arrange
        var terminalId = Guid.NewGuid();
        var fragments = new List<string>
        {
            "Override codes v2.14",
            "Project: SENTINEL",
            "Vault section: Black Vault",
            "Dates: 2156-03-15",
            "Access level: Alpha",
            "Status: Active"
        };

        // DC for Corrupted = 16. Check of 26 = critical success (DC + 10)
        // Act
        var (data, bonusInsight) = _service.AttemptDataRecovery(
            terminalId, "Main Server",
            TerminalState.Corrupted, 26, fragments);

        // Assert
        data.Should().NotBeNull();
        data!.IsComplete.Should().BeTrue();
        data.GetFragmentCount().Should().BeGreaterThan(0);
        bonusInsight.Should().BeTrue();
    }

    // ═══════ Prerequisite Tests ═══════

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(10).Should().BeTrue();
        _service.CanUnlockTier2(7).Should().BeFalse();
        _service.CanUnlockTier2(0).Should().BeFalse();
    }

    [Test]
    public void GetAbilityPPCost_Tier2_ReturnsFour()
    {
        // Arrange & Act & Assert
        JotunReaderTier2AbilityService.GetAbilityPPCost(
            JotunReaderAbilityId.TechnicalMemory).Should().Be(4);
        JotunReaderTier2AbilityService.GetAbilityPPCost(
            JotunReaderAbilityId.ExploitWeakness).Should().Be(4);
        JotunReaderTier2AbilityService.GetAbilityPPCost(
            JotunReaderAbilityId.DataRecovery).Should().Be(4);
    }
}
