// ═══════════════════════════════════════════════════════════════════════════════
// JotunReaderTier3AbilityServiceTests.cs
// Unit tests for the Jötun-Reader Tier 3 and Capstone abilities:
// Lore Keeper, Ancient Knowledge, and Voice of the Giants.
// Version: 0.20.3c
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Tests for <see cref="JotunReaderTier3AbilityService"/> covering Lore Keeper,
/// Ancient Knowledge, and Voice of the Giants abilities.
/// </summary>
[TestFixture]
public class JotunReaderTier3AbilityServiceTests
{
    private Mock<ILogger<JotunReaderTier3AbilityService>> _mockLogger = null!;
    private JotunReaderTier3AbilityService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<JotunReaderTier3AbilityService>>();
        _service = new JotunReaderTier3AbilityService(_mockLogger.Object);
    }

    // ═══════ Lore Keeper Tests ═══════

    /// <summary>
    /// Verifies that Lore Keeper auto-succeeds Layer 3 for historical objects.
    /// </summary>
    [Test]
    public void LoreKeeper_AutoSucceedsLayer3_ForHistoricalObjects()
    {
        // Arrange
        var historicalTags = new List<string> { "Historical", "Ancient" }.AsReadOnly();

        // Act
        var result = _service.ShouldAutoSucceedLayer3(historicalTags, layerNumber: 3);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Lore Keeper does NOT auto-succeed for non-Layer 3.
    /// </summary>
    [Test]
    public void LoreKeeper_DoesNotAutoSucceed_ForNonLayer3()
    {
        // Arrange
        var historicalTags = new List<string> { "Historical" }.AsReadOnly();

        // Act
        var result = _service.ShouldAutoSucceedLayer3(historicalTags, layerNumber: 2);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Lore Keeper does NOT auto-succeed for non-historical objects.
    /// </summary>
    [Test]
    public void LoreKeeper_DoesNotAutoSucceed_ForNonHistoricalObjects()
    {
        // Arrange
        var nonHistoricalTags = new List<string> { "Mechanical", "Common" }.AsReadOnly();

        // Act
        var result = _service.ShouldAutoSucceedLayer3(nonHistoricalTags, layerNumber: 3);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Lore Keeper generates 2 Lore Insight on first examination.
    /// </summary>
    [Test]
    public void LoreKeeper_GeneratesTwoInsight_OnFirstExamination()
    {
        // Arrange & Act
        var insight = _service.GenerateLoreKeeperInsight(isFirstExamination: true);

        // Assert
        insight.Should().Be(JotunReaderTier3AbilityService.LoreKeeperFirstExamInsight);
        insight.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Lore Keeper generates 0 insight on re-examination.
    /// </summary>
    [Test]
    public void LoreKeeper_GeneratesZeroInsight_OnReExamination()
    {
        // Arrange & Act
        var insight = _service.GenerateLoreKeeperInsight(isFirstExamination: false);

        // Assert
        insight.Should().Be(0);
    }

    // ═══════ Ancient Knowledge Tests ═══════

    /// <summary>
    /// Verifies that Ancient Knowledge reveals 1 bonus lore on Expert success.
    /// </summary>
    [Test]
    public void AncientKnowledge_RevealsBonusLore_OnExpertSuccess()
    {
        // Arrange & Act
        var count = _service.CalculateBonusLoreCount(
            isExpertSuccess: true, isMasterSuccess: false);

        // Assert
        count.Should().Be(JotunReaderTier3AbilityService.AncientKnowledgeExpertLoreCount);
        count.Should().Be(1);
    }

    /// <summary>
    /// Verifies that Ancient Knowledge reveals 2 bonus lore on Master success.
    /// </summary>
    [Test]
    public void AncientKnowledge_RevealsTwoBonusLore_OnMasterSuccess()
    {
        // Arrange & Act
        var count = _service.CalculateBonusLoreCount(
            isExpertSuccess: true, isMasterSuccess: true);

        // Assert
        count.Should().Be(JotunReaderTier3AbilityService.AncientKnowledgeMasterLoreCount);
        count.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Ancient Knowledge generates 1 Lore Insight per bonus lore entry.
    /// </summary>
    [Test]
    public void AncientKnowledge_GeneratesInsight_PerBonusLore()
    {
        // Arrange
        var loreCount = 2;

        // Act
        var insight = _service.CalculateAncientKnowledgeInsight(loreCount);

        // Assert
        insight.Should().Be(loreCount * JotunReaderTier3AbilityService.AncientKnowledgeInsightPerLore);
        insight.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GenerateBonusLore creates a valid LoreRevelation.
    /// </summary>
    [Test]
    public void AncientKnowledge_GenerateBonusLore_CreatesValidRevelation()
    {
        // Arrange
        var examinationId = Guid.NewGuid();

        // Act
        var revelation = _service.GenerateBonusLore(
            examinationId,
            LoreRevelationType.HistoricalContext,
            "Origins of the Seal",
            "This rune-carved seal was forged in the Age of Twilight.");

        // Assert
        revelation.Should().NotBeNull();
        revelation.RevelationId.Should().NotBeEmpty();
        revelation.SourceExaminationId.Should().Be(examinationId);
        revelation.RevelationType.Should().Be(LoreRevelationType.HistoricalContext);
        revelation.Title.Should().Be("Origins of the Seal");
        revelation.InsightGenerated.Should().Be(1);
    }

    // ═══════ Voice of the Giants Tests ═══════

    /// <summary>
    /// Verifies that Voice of the Giants activates dormant technology.
    /// </summary>
    [Test]
    public void VoiceOfTheGiants_ActivatesTechnology()
    {
        // Arrange
        var techId = Guid.NewGuid();
        var activatorId = Guid.NewGuid();

        // Act
        var activation = _service.ActivateTechnology(
            techId,
            JotunTechnologyType.MedicalBay,
            "Ancient Healing Chamber",
            activatorId);

        // Assert
        activation.Should().NotBeNull();
        activation.ActivationId.Should().NotBeEmpty();
        activation.TechnologyId.Should().Be(techId);
        activation.TechnologyType.Should().Be(JotunTechnologyType.MedicalBay);
        activation.TechnologyName.Should().Be("Ancient Healing Chamber");
        activation.ActivatorId.Should().Be(activatorId);
        activation.Effects.Should().HaveCount(1);
        activation.Effects[0].EffectType.Should().Be("AreaHealing");
        activation.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Voice of the Giants is usable only once per long rest.
    /// </summary>
    [Test]
    public void VoiceOfTheGiants_OncePerLongRest()
    {
        // Arrange
        var usedRestIds = new List<Guid>();
        var currentRestId = Guid.NewGuid();

        // Act — first use
        var hasUsedBefore = _service.HasUsedVoiceOfGiants(usedRestIds, currentRestId);
        var updatedLog = _service.MarkVoiceOfGiantsUsed(usedRestIds, currentRestId);

        // Assert — first use should be allowed
        hasUsedBefore.Should().BeFalse();
        updatedLog.Should().Contain(currentRestId);

        // Act — second use
        var hasUsedAfter = _service.HasUsedVoiceOfGiants(updatedLog, currentRestId);

        // Assert — second use should be blocked
        hasUsedAfter.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Voice of the Giants is locked until 24 PP invested.
    /// </summary>
    [Test]
    public void VoiceOfTheGiants_LockedUntil24PPInvested()
    {
        // Arrange & Act & Assert
        _service.CanUnlockCapstone(23).Should().BeFalse();
        _service.CanUnlockCapstone(24).Should().BeTrue();
        _service.CanUnlockCapstone(25).Should().BeTrue();
    }

    // ═══════ Prerequisite Tests ═══════

    /// <summary>
    /// Verifies that Tier 3 abilities require 16 PP invested.
    /// </summary>
    [Test]
    public void CanUnlockTier3_WithSufficientPP_ReturnsTrue()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier3(16).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Tier 3 is locked with insufficient PP.
    /// </summary>
    [Test]
    public void CanUnlockTier3_WithInsufficientPP_ReturnsFalse()
    {
        // Arrange & Act & Assert
        _service.CanUnlockTier3(15).Should().BeFalse();
    }

    // ═══════ Constructor Tests ═══════

    /// <summary>
    /// Verifies that the service throws when constructed with a null logger.
    /// </summary>
    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new JotunReaderTier3AbilityService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    // ═══════ Constants Validation ═══════

    /// <summary>
    /// Verifies that constants match the design specification.
    /// </summary>
    [Test]
    public void Constants_MatchDesignSpecification()
    {
        // Assert — PP thresholds
        JotunReaderTier3AbilityService.Tier3Threshold.Should().Be(16);
        JotunReaderTier3AbilityService.CapstoneThreshold.Should().Be(24);

        // Assert — Lore Keeper
        JotunReaderTier3AbilityService.LoreKeeperFirstExamInsight.Should().Be(2);
        JotunReaderTier3AbilityService.LoreKeeperAutoSucceedLayer.Should().Be(3);

        // Assert — Ancient Knowledge
        JotunReaderTier3AbilityService.AncientKnowledgeExpertLoreCount.Should().Be(1);
        JotunReaderTier3AbilityService.AncientKnowledgeMasterLoreCount.Should().Be(2);
        JotunReaderTier3AbilityService.AncientKnowledgeInsightPerLore.Should().Be(1);

        // Assert — Voice of the Giants
        JotunReaderTier3AbilityService.VoiceOfTheGiantsAPCost.Should().Be(5);
        JotunReaderTier3AbilityService.VoiceOfTheGiantsInsightCost.Should().Be(8);
        JotunReaderTier3AbilityService.DefaultActivationDurationMinutes.Should().Be(60);
    }
}
