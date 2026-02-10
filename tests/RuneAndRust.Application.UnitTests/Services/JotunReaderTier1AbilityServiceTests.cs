// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderTier1AbilityServiceTests.cs
// Unit tests for the JotunReaderTier1AbilityService, validating Deep Scan,
// Pattern Recognition, Ancient Tongues, and PP prerequisite logic.
// Version: 0.20.3a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.UnitTests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Tests for <see cref="JotunReaderTier1AbilityService"/>.
/// </summary>
[TestFixture]
public class JotunReaderTier1AbilityServiceTests
{
    private JotunReaderTier1AbilityService _service = null!;
    private Mock<ILogger<JotunReaderTier1AbilityService>> _mockLogger = null!;
    private Random _seededRandom = null!;

    private readonly Guid _targetId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JotunReaderTier1AbilityService>>();
        _seededRandom = new Random(42);
        _service = new JotunReaderTier1AbilityService(_mockLogger.Object, _seededRandom);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Deep Scan Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ExecuteDeepScan_WithValidTarget_ReturnsResult()
    {
        // Arrange & Act
        var result = _service.ExecuteDeepScan(_targetId, "machinery", 15, 3);

        // Assert
        result.Should().NotBeNull();
        result!.TargetId.Should().Be(_targetId);
        result.TargetType.Should().Be("machinery");
        result.BaseRoll.Should().Be(15);
        result.Modifiers.Should().BeGreaterThan(3);   // perception + 2d10 bonus
        result.TotalResult.Should().BeGreaterThan(18); // 15 + 3 + bonus
    }

    [Test]
    public void ExecuteDeepScan_WithInvalidTarget_ReturnsNull()
    {
        // Arrange & Act
        var result = _service.ExecuteDeepScan(_targetId, "wooden-chair", 15, 3);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ExecuteDeepScan_GeneratesInsightOnSuccess()
    {
        // Arrange — use a high roll to ensure success
        var result = _service.ExecuteDeepScan(_targetId, "terminal", 18, 5);

        // Assert
        result.Should().NotBeNull();
        result!.InsightGenerated.Should().BeGreaterThan(0);
    }

    [Test]
    public void ExecuteDeepScan_SuccessRevealsLayers()
    {
        // Arrange — high roll should reveal multiple layers
        var result = _service.ExecuteDeepScan(_targetId, "jotun-tech", 18, 5);

        // Assert
        result.Should().NotBeNull();
        result!.LayersRevealed.Should().BeGreaterThan(0);
        result.Information.Should().NotBeEmpty();
        result.IsSuccess().Should().BeTrue();
    }

    [Test]
    public void IsValidDeepScanTarget_Machinery_ReturnsTrue()
    {
        // Act & Assert
        _service.IsValidDeepScanTarget("machinery").Should().BeTrue();
        _service.IsValidDeepScanTarget("terminal").Should().BeTrue();
        _service.IsValidDeepScanTarget("jotun-tech").Should().BeTrue();
    }

    [Test]
    public void IsValidDeepScanTarget_NonTech_ReturnsFalse()
    {
        // Act & Assert
        _service.IsValidDeepScanTarget("wooden-door").Should().BeFalse();
        _service.IsValidDeepScanTarget("goblin").Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Pattern Recognition Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void ApplyPatternRecognition_JotunTech_AutoSucceeds()
    {
        // Act & Assert
        _service.ApplyPatternRecognition("jotun-tech").Should().BeTrue();
        _service.ApplyPatternRecognition("jotun-machinery").Should().BeTrue();
        _service.ApplyPatternRecognition("terminal").Should().BeTrue();
    }

    [Test]
    public void ApplyPatternRecognition_NonJotunTech_ReturnsFalse()
    {
        // Act & Assert
        _service.ApplyPatternRecognition("wooden-crate").Should().BeFalse();
        _service.ApplyPatternRecognition("stone-wall").Should().BeFalse();
    }

    [Test]
    public void IsJotunTechnology_CaseInsensitive_ReturnsTrue()
    {
        // Act & Assert
        _service.IsJotunTechnology("MACHINERY").Should().BeTrue();
        _service.IsJotunTechnology("Terminal").Should().BeTrue();
        _service.IsJotunTechnology("Jotun-Tech").Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ancient Tongues Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanReadScript_UnlockedScript_ReturnsTrue()
    {
        // Act & Assert
        _service.CanReadScript(ScriptType.JotunFormal).Should().BeTrue();
        _service.CanReadScript(ScriptType.JotunTechnical).Should().BeTrue();
        _service.CanReadScript(ScriptType.DvergrStandard).Should().BeTrue();
        _service.CanReadScript(ScriptType.DvergrRunic).Should().BeTrue();
    }

    [Test]
    public void GetUnlockedScripts_ReturnsFourScriptTypes()
    {
        // Act
        var scripts = _service.GetUnlockedScripts();

        // Assert
        scripts.Should().HaveCount(4);
        scripts.Should().Contain(ScriptType.JotunFormal);
        scripts.Should().Contain(ScriptType.JotunTechnical);
        scripts.Should().Contain(ScriptType.DvergrStandard);
        scripts.Should().Contain(ScriptType.DvergrRunic);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Prerequisite Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Test]
    public void CanUnlockTier2_WithSufficientPP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(8).Should().BeTrue();
        _service.CanUnlockTier2(12).Should().BeTrue();
    }

    [Test]
    public void CanUnlockTier2_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier2(7).Should().BeFalse();
        _service.CanUnlockTier2(0).Should().BeFalse();
    }

    [Test]
    public void CalculatePPInvested_WithTier1Abilities_ReturnsZero()
    {
        // Arrange
        var abilities = new List<JotunReaderAbilityId>
        {
            JotunReaderAbilityId.DeepScan,
            JotunReaderAbilityId.PatternRecognition,
            JotunReaderAbilityId.AncientTongues
        };

        // Act
        var total = _service.CalculatePPInvested(abilities);

        // Assert — Tier 1 abilities are free
        total.Should().Be(0);
    }

    [Test]
    public void GetAbilityPPCost_Tier1_ReturnsZero()
    {
        // Act & Assert
        _service.GetAbilityPPCost(JotunReaderAbilityId.DeepScan).Should().Be(0);
        _service.GetAbilityPPCost(JotunReaderAbilityId.PatternRecognition).Should().Be(0);
        _service.GetAbilityPPCost(JotunReaderAbilityId.AncientTongues).Should().Be(0);
    }

    [Test]
    public void GetAbilityPPCost_Tier2_ReturnsFour()
    {
        // Act & Assert
        _service.GetAbilityPPCost(JotunReaderAbilityId.TechnicalMemory).Should().Be(4);
        _service.GetAbilityPPCost(JotunReaderAbilityId.ExploitWeakness).Should().Be(4);
        _service.GetAbilityPPCost(JotunReaderAbilityId.DataRecovery).Should().Be(4);
    }

    [Test]
    public void GetAbilityPPCost_Tier3_ReturnsFive()
    {
        // Act & Assert
        _service.GetAbilityPPCost(JotunReaderAbilityId.LoreKeeper).Should().Be(5);
        _service.GetAbilityPPCost(JotunReaderAbilityId.AncientKnowledge).Should().Be(5);
    }

    [Test]
    public void GetAbilityPPCost_Capstone_ReturnsSix()
    {
        // Act & Assert
        _service.GetAbilityPPCost(JotunReaderAbilityId.VoiceOfTheGiants).Should().Be(6);
    }
}
