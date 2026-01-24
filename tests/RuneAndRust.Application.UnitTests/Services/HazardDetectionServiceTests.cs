using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="HazardDetectionService"/>.
/// </summary>
/// <remarks>
/// Tests cover:
/// <list type="bullet">
///   <item><description>Hazard detection DC calculation</description></item>
///   <item><description>Passive perception calculation (WITS ÷ 2)</description></item>
///   <item><description>Passive detection logic</description></item>
///   <item><description>Active detection logic and critical detection</description></item>
///   <item><description>Hazard consequence application</description></item>
///   <item><description>Glitch Effect Table rolling</description></item>
///   <item><description>HazardDetectionResult value object</description></item>
///   <item><description>HazardTriggerResult value object</description></item>
///   <item><description>DetectableHazardType enum extensions</description></item>
///   <item><description>DetectionMethod enum extensions</description></item>
/// </list>
/// </remarks>
[TestFixture]
public class HazardDetectionServiceTests
{
    private SkillCheckService _skillCheckService = null!;
    private DiceService _diceService = null!;
    private ILogger<HazardDetectionService> _logger = null!;
    private HazardDetectionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _skillCheckService = CreateMockSkillCheckService();
        _diceService = CreateDiceService();
        _logger = Substitute.For<ILogger<HazardDetectionService>>();
        _sut = new HazardDetectionService(_skillCheckService, _diceService, _logger);
    }

    #region Hazard Detection DC Tests

    [Test]
    [TestCase(DetectableHazardType.ObviousDanger, 8)]
    [TestCase(DetectableHazardType.HiddenPit, 12)]
    [TestCase(DetectableHazardType.ToxicZone, 16)]
    [TestCase(DetectableHazardType.AmbushSite, 16)]
    [TestCase(DetectableHazardType.GlitchPocket, 20)]
    public void GetDetectionDc_ReturnsCorrectDcForEachHazardType(DetectableHazardType hazardType, int expectedDc)
    {
        // Act
        var dc = _sut.GetDetectionDc(hazardType);

        // Assert
        dc.Should().Be(expectedDc);
    }

    [Test]
    public void GetHazardDescription_ReturnsDescriptionsForAllTypes()
    {
        // Act
        var (obviousName, obviousDesc) = _sut.GetHazardDescription(DetectableHazardType.ObviousDanger);
        var (pitName, pitDesc) = _sut.GetHazardDescription(DetectableHazardType.HiddenPit);
        var (glitchName, glitchDesc) = _sut.GetHazardDescription(DetectableHazardType.GlitchPocket);

        // Assert
        obviousName.Should().Be("Obvious Danger");
        obviousDesc.Should().Match(s => s.Contains("debris") || s.Contains("wiring"));

        pitName.Should().Be("Hidden Pit");
        pitDesc.Should().Match(s => s.Contains("floor") || s.Contains("drop"));

        glitchName.Should().Be("Glitch Pocket");
        glitchDesc.Should().Match(s => s.Contains("reality") || s.Contains("distortion"));
    }

    [Test]
    public void GetHazardConsequence_ReturnsConsequenceForAllTypes()
    {
        // Act & Assert
        _sut.GetHazardConsequence(DetectableHazardType.ObviousDanger).Should().Contain("1d6");
        _sut.GetHazardConsequence(DetectableHazardType.HiddenPit).Should().Contain("2d10");
        _sut.GetHazardConsequence(DetectableHazardType.ToxicZone).Should().Contain("Poisoned");
        _sut.GetHazardConsequence(DetectableHazardType.GlitchPocket).Should().ContainEquivalentOf("glitch");
        _sut.GetHazardConsequence(DetectableHazardType.AmbushSite).Should().Contain("surprise");
    }

    #endregion

    #region Passive Perception Tests

    [Test]
    public void GetPassivePerception_CalculatesWitsDividedByTwo()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 12);

        // Act
        var passive = _sut.GetPassivePerception(player);

        // Assert
        passive.Should().Be(6); // 12 ÷ 2
    }

    [Test]
    [TestCase(8, 4)]
    [TestCase(10, 5)]
    [TestCase(16, 8)]
    [TestCase(20, 10)]
    [TestCase(24, 12)]
    public void GetPassivePerception_HandlesVariousWitsValues(int wits, int expectedPassive)
    {
        // Arrange
        var player = CreateTestPlayer(wits: wits);

        // Act
        var passive = _sut.GetPassivePerception(player);

        // Assert
        passive.Should().Be(expectedPassive);
    }

    [Test]
    public void CheckPassiveDetection_DetectsHazardsWithinPassiveRange()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 16); // Passive = 8
        var hazards = new List<DetectableHazardType>
        {
            DetectableHazardType.ObviousDanger, // DC 8 - should detect
            DetectableHazardType.HiddenPit,     // DC 12 - should NOT detect
            DetectableHazardType.ToxicZone      // DC 16 - should NOT detect
        };

        // Act
        var results = _sut.CheckPassiveDetection(player, hazards);

        // Assert
        results.Should().HaveCount(1);
        results[0].HazardDetected.Should().BeTrue();
        results[0].DetectionMethod.Should().Be(DetectionMethod.PassivePerception);
        results[0].IsHintOnly.Should().BeTrue();
        results[0].HazardType.Should().BeNull(); // Type not revealed for passive
    }

    [Test]
    public void CheckPassiveDetection_HighWitsDetectsMoreHazards()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 24); // Passive = 12
        var hazards = new List<DetectableHazardType>
        {
            DetectableHazardType.ObviousDanger, // DC 8 - should detect
            DetectableHazardType.HiddenPit      // DC 12 - should detect (12 >= 12)
        };

        // Act
        var results = _sut.CheckPassiveDetection(player, hazards);

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.HazardDetected && r.IsHintOnly);
    }

    [Test]
    public void CheckPassiveDetection_LowWitsDetectsNothing()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 6); // Passive = 3
        var hazards = new List<DetectableHazardType>
        {
            DetectableHazardType.ObviousDanger // DC 8 - should NOT detect (3 < 8)
        };

        // Act
        var results = _sut.CheckPassiveDetection(player, hazards);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void CheckPassiveDetection_ReturnsEmptyForNoHazards()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 20);
        var hazards = new List<DetectableHazardType>();

        // Act
        var results = _sut.CheckPassiveDetection(player, hazards);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region Avoidance Options Tests

    [Test]
    public void GetAvoidanceOptions_ReturnsOptionsForAllHazardTypes()
    {
        // Act & Assert
        _sut.GetAvoidanceOptions(DetectableHazardType.ObviousDanger).Should().NotBeEmpty();
        _sut.GetAvoidanceOptions(DetectableHazardType.HiddenPit).Should().NotBeEmpty();
        _sut.GetAvoidanceOptions(DetectableHazardType.ToxicZone).Should().NotBeEmpty();
        _sut.GetAvoidanceOptions(DetectableHazardType.GlitchPocket).Should().NotBeEmpty();
        _sut.GetAvoidanceOptions(DetectableHazardType.AmbushSite).Should().NotBeEmpty();
    }

    [Test]
    public void GetAvoidanceOptions_HiddenPit_IncludesJumpOption()
    {
        // Act
        var options = _sut.GetAvoidanceOptions(DetectableHazardType.HiddenPit);

        // Assert
        options.Should().Contain(o => o.Contains("Jump") || o.Contains("jump"));
    }

    [Test]
    public void GetAvoidanceOptions_ToxicZone_IncludesProtectiveEquipment()
    {
        // Act
        var options = _sut.GetAvoidanceOptions(DetectableHazardType.ToxicZone);

        // Assert
        options.Should().Contain(o => o.Contains("protective") || o.Contains("equipment"));
    }

    [Test]
    public void GetAvoidanceOptions_AmbushSite_IncludesCombatPreparation()
    {
        // Act
        var options = _sut.GetAvoidanceOptions(DetectableHazardType.AmbushSite);

        // Assert
        options.Should().Contain(o => o.Contains("combat") || o.Contains("Prepare"));
    }

    #endregion

    #region Glitch Effect Table Tests

    [Test]
    public void RollGlitchEffect_ReturnsValidRollAndEffect()
    {
        // Act
        var (roll, effectName, effectDescription) = _sut.RollGlitchEffect();

        // Assert
        roll.Should().BeInRange(1, 6);
        effectName.Should().NotBeNullOrEmpty();
        effectDescription.Should().NotBeNullOrEmpty();
    }

    [Test]
    [TestCase(1, "Teleport")]
    [TestCase(2, "Psychic Feedback")]
    [TestCase(3, "Equipment Malfunction")]
    [TestCase(4, "Time Skip")]
    [TestCase(5, "Memory Echo")]
    [TestCase(6, "Reality Anchor")]
    public void GetGlitchEffectDescription_ReturnsCorrectEffectForRoll(int roll, string expectedEffectName)
    {
        // Act
        var (effectName, effectDescription) = _sut.GetGlitchEffectDescription(roll);

        // Assert
        effectName.Should().Be(expectedEffectName);
        effectDescription.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void GetGlitchEffectDescription_Roll2_MentionsPsychicDamage()
    {
        // Act
        var (_, description) = _sut.GetGlitchEffectDescription(2);

        // Assert
        description.Should().Contain("2d10").And.Contain("psychic");
    }

    #endregion

    #region Investigation Prerequisites Tests

    [Test]
    public void CanInvestigate_NormalPlayer_ReturnsTrue()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var canInvestigate = _sut.CanInvestigate(player);

        // Assert
        canInvestigate.Should().BeTrue();
    }

    [Test]
    public void GetInvestigationBlockedReason_NormalPlayer_ReturnsNull()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var reason = _sut.GetInvestigationBlockedReason(player);

        // Assert
        reason.Should().BeNull();
    }

    #endregion

    #region HazardDetectionResult Value Object Tests

    [Test]
    public void HazardDetectionResult_PassiveHint_HasCorrectProperties()
    {
        // Act
        var result = HazardDetectionResult.PassiveHint(DetectableHazardType.HiddenPit, 12);

        // Assert
        result.HazardDetected.Should().BeTrue();
        result.HazardType.Should().BeNull(); // Not revealed for passive
        result.DetectionMethod.Should().Be(DetectionMethod.PassivePerception);
        result.IsHintOnly.Should().BeTrue();
        result.FullyIdentified.Should().BeFalse();
        result.HasAwareness.Should().BeTrue();
        result.TargetDc.Should().Be(12);
    }

    [Test]
    public void HazardDetectionResult_ActiveSuccess_HasCorrectProperties()
    {
        // Act
        var avoidanceOptions = new List<string> { "Option 1", "Option 2" };
        var result = HazardDetectionResult.ActiveSuccess(
            DetectableHazardType.ToxicZone,
            avoidanceOptions,
            "Poisoned for 3 rounds",
            8,
            16,
            "Test roll");

        // Assert
        result.HazardDetected.Should().BeTrue();
        result.HazardType.Should().Be(DetectableHazardType.ToxicZone);
        result.DetectionMethod.Should().Be(DetectionMethod.ActiveInvestigation);
        result.FullyIdentified.Should().BeTrue();
        result.IsHintOnly.Should().BeFalse();
        result.NetSuccesses.Should().Be(8);
        result.TargetDc.Should().Be(16);
        result.IsCritical.Should().BeTrue(); // 8 >= 5
        result.AvoidanceOptions.Should().HaveCount(2);
    }

    [Test]
    public void HazardDetectionResult_AreaSweepSuccess_HasCorrectProperties()
    {
        // Act
        var result = HazardDetectionResult.AreaSweepSuccess(
            DetectableHazardType.AmbushSite,
            new[] { "Option 1" },
            "Surprise round",
            3,
            16);

        // Assert
        result.DetectionMethod.Should().Be(DetectionMethod.AreaSweep);
        result.FullyIdentified.Should().BeTrue();
        result.IsCritical.Should().BeFalse(); // 3 < 5
    }

    [Test]
    public void HazardDetectionResult_NotDetected_HasCorrectProperties()
    {
        // Act
        var result = HazardDetectionResult.NotDetected(5, 12, "Failed check");

        // Assert
        result.HazardDetected.Should().BeFalse();
        result.HazardType.Should().BeNull();
        result.DetectionMethod.Should().Be(DetectionMethod.None);
        result.NetSuccesses.Should().Be(5);
        result.TargetDc.Should().Be(12);
        result.Margin.Should().Be(-7); // 5 - 12
    }

    [Test]
    public void HazardDetectionResult_Empty_HasCorrectProperties()
    {
        // Act
        var result = HazardDetectionResult.Empty();

        // Assert
        result.HazardDetected.Should().BeFalse();
        result.NetSuccesses.Should().Be(0);
        result.TargetDc.Should().Be(0);
    }

    [Test]
    public void HazardDetectionResult_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var passiveResult = HazardDetectionResult.PassiveHint(DetectableHazardType.ObviousDanger, 8);
        var activeResult = HazardDetectionResult.ActiveSuccess(
            DetectableHazardType.HiddenPit,
            new[] { "Go around" },
            "2d10 fall damage",
            6,
            12);
        var notDetectedResult = HazardDetectionResult.NotDetected();

        // Act
        var passiveDisplay = passiveResult.ToDisplayString();
        var activeDisplay = activeResult.ToDisplayString();
        var notDetectedDisplay = notDetectedResult.ToDisplayString();

        // Assert
        passiveDisplay.Should().Match(s => s.Contains("feels wrong") || s.Contains("danger"));
        activeDisplay.Should().Contain("HAZARD DETECTED");
        activeDisplay.Should().Contain("Hidden Pit");
        notDetectedDisplay.Should().Contain("don't detect");
    }

    [Test]
    public void HazardDetectionResult_IsCritical_TrueWhenNetSuccessesAtLeast5()
    {
        // Arrange & Act
        var criticalResult = HazardDetectionResult.ActiveSuccess(
            DetectableHazardType.GlitchPocket, Array.Empty<string>(), "Test", 5, 20);
        var nonCriticalResult = HazardDetectionResult.ActiveSuccess(
            DetectableHazardType.GlitchPocket, Array.Empty<string>(), "Test", 4, 20);

        // Assert
        criticalResult.IsCritical.Should().BeTrue();
        nonCriticalResult.IsCritical.Should().BeFalse();
    }

    #endregion

    #region HazardTriggerResult Value Object Tests

    [Test]
    public void HazardTriggerResult_ObviousDanger_HasCorrectProperties()
    {
        // Act
        var result = HazardTriggerResult.ObviousDanger(4);

        // Assert
        result.HazardType.Should().Be(DetectableHazardType.ObviousDanger);
        result.DamageDealt.Should().Be(4);
        result.DamageType.Should().Be("Physical");
        result.DealtDamage.Should().BeTrue();
        result.AppliedStatus.Should().BeFalse();
        result.RequiresAssistance.Should().BeFalse();
        result.TriggersSurpriseRound.Should().BeFalse();
        result.Severity.Should().Be(1); // Minor
    }

    [Test]
    public void HazardTriggerResult_HiddenPit_HasCorrectProperties()
    {
        // Act
        var result = HazardTriggerResult.HiddenPit(14);

        // Assert
        result.HazardType.Should().Be(DetectableHazardType.HiddenPit);
        result.DamageDealt.Should().Be(14);
        result.DamageType.Should().Be("Physical");
        result.RequiresAssistance.Should().BeTrue();
        result.HasSpecialEffect.Should().BeTrue();
        result.Severity.Should().Be(2); // Moderate (14 >= 10)
    }

    [Test]
    public void HazardTriggerResult_ToxicZone_HasCorrectProperties()
    {
        // Act
        var result = HazardTriggerResult.ToxicZone();

        // Assert
        result.HazardType.Should().Be(DetectableHazardType.ToxicZone);
        result.DamageDealt.Should().Be(0);
        result.StatusApplied.Should().Be("Poisoned");
        result.StatusDuration.Should().Be(3);
        result.AppliedStatus.Should().BeTrue();
        result.DealtDamage.Should().BeFalse();
    }

    [Test]
    public void HazardTriggerResult_GlitchPocket_HasCorrectProperties()
    {
        // Act
        var result = HazardTriggerResult.GlitchPocket(3, "Equipment malfunction", 0);

        // Assert
        result.HazardType.Should().Be(DetectableHazardType.GlitchPocket);
        result.StatusApplied.Should().Be("Disoriented");
        result.StatusDuration.Should().Be(2);
        result.GlitchEffectRoll.Should().Be(3);
        result.IsGlitchEffect.Should().BeTrue();
        result.HasSpecialEffect.Should().BeTrue();
        result.Severity.Should().Be(3); // Severe (glitch effect)
    }

    [Test]
    public void HazardTriggerResult_GlitchPocket_Roll2_IncludesPsychicDamage()
    {
        // Act
        var result = HazardTriggerResult.GlitchPocket(2, "Psychic feedback", 12);

        // Assert
        result.DamageDealt.Should().Be(12);
        result.DamageType.Should().Be("Psychic");
        result.DealtDamage.Should().BeTrue();
    }

    [Test]
    public void HazardTriggerResult_AmbushSite_HasCorrectProperties()
    {
        // Act
        var result = HazardTriggerResult.AmbushSite();

        // Assert
        result.HazardType.Should().Be(DetectableHazardType.AmbushSite);
        result.TriggersSurpriseRound.Should().BeTrue();
        result.TriggersCombat.Should().BeTrue();
        result.DamageDealt.Should().Be(0);
        result.Severity.Should().Be(3); // Severe (combat trigger)
    }

    [Test]
    public void HazardTriggerResult_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var damageResult = HazardTriggerResult.ObviousDanger(5);
        var statusResult = HazardTriggerResult.ToxicZone();
        var ambushResult = HazardTriggerResult.AmbushSite();

        // Act
        var damageDisplay = damageResult.ToDisplayString();
        var statusDisplay = statusResult.ToDisplayString();
        var ambushDisplay = ambushResult.ToDisplayString();

        // Assert
        damageDisplay.Should().Contain("Obvious Danger");
        damageDisplay.Should().Contain("5 physical damage");

        statusDisplay.Should().Contain("Toxic Zone");
        statusDisplay.Should().Contain("Poisoned");
        statusDisplay.Should().Contain("3 round");

        ambushDisplay.Should().Contain("Ambush Site");
        ambushDisplay.Should().Contain("surprise");
    }

    [Test]
    public void HazardTriggerResult_GetGlitchEffectName_ReturnsCorrectNames()
    {
        // Assert
        HazardTriggerResult.GetGlitchEffectName(1).Should().Be("Teleport");
        HazardTriggerResult.GetGlitchEffectName(2).Should().Be("Psychic Feedback");
        HazardTriggerResult.GetGlitchEffectName(3).Should().Be("Equipment Malfunction");
        HazardTriggerResult.GetGlitchEffectName(4).Should().Be("Time Skip");
        HazardTriggerResult.GetGlitchEffectName(5).Should().Be("Memory Echo");
        HazardTriggerResult.GetGlitchEffectName(6).Should().Be("Reality Anchor");
    }

    [Test]
    public void HazardTriggerResult_GlitchEffectCausesDamage_TrueOnlyForRoll2()
    {
        // Assert
        HazardTriggerResult.GlitchEffectCausesDamage(1).Should().BeFalse();
        HazardTriggerResult.GlitchEffectCausesDamage(2).Should().BeTrue();
        HazardTriggerResult.GlitchEffectCausesDamage(3).Should().BeFalse();
        HazardTriggerResult.GlitchEffectCausesDamage(4).Should().BeFalse();
        HazardTriggerResult.GlitchEffectCausesDamage(5).Should().BeFalse();
        HazardTriggerResult.GlitchEffectCausesDamage(6).Should().BeFalse();
    }

    #endregion

    #region DetectableHazardType Enum Extension Tests

    [Test]
    public void DetectableHazardType_GetDetectionDc_ReturnsCorrectValues()
    {
        // Assert
        DetectableHazardType.ObviousDanger.GetDetectionDc().Should().Be(8);
        DetectableHazardType.HiddenPit.GetDetectionDc().Should().Be(12);
        DetectableHazardType.ToxicZone.GetDetectionDc().Should().Be(16);
        DetectableHazardType.AmbushSite.GetDetectionDc().Should().Be(16);
        DetectableHazardType.GlitchPocket.GetDetectionDc().Should().Be(20);
    }

    [Test]
    public void DetectableHazardType_GetDisplayName_ReturnsCorrectNames()
    {
        // Assert
        DetectableHazardType.ObviousDanger.GetDisplayName().Should().Be("Obvious Danger");
        DetectableHazardType.HiddenPit.GetDisplayName().Should().Be("Hidden Pit");
        DetectableHazardType.ToxicZone.GetDisplayName().Should().Be("Toxic Zone");
        DetectableHazardType.GlitchPocket.GetDisplayName().Should().Be("Glitch Pocket");
        DetectableHazardType.AmbushSite.GetDisplayName().Should().Be("Ambush Site");
    }

    [Test]
    public void DetectableHazardType_DealsImmediateDamage_CorrectForEachType()
    {
        // Assert
        DetectableHazardType.ObviousDanger.DealsImmediateDamage().Should().BeTrue();
        DetectableHazardType.HiddenPit.DealsImmediateDamage().Should().BeTrue();
        DetectableHazardType.ToxicZone.DealsImmediateDamage().Should().BeFalse();
        DetectableHazardType.GlitchPocket.DealsImmediateDamage().Should().BeFalse();
        DetectableHazardType.AmbushSite.DealsImmediateDamage().Should().BeFalse();
    }

    [Test]
    public void DetectableHazardType_AppliesStatusEffect_CorrectForEachType()
    {
        // Assert
        DetectableHazardType.ObviousDanger.AppliesStatusEffect().Should().BeFalse();
        DetectableHazardType.HiddenPit.AppliesStatusEffect().Should().BeFalse();
        DetectableHazardType.ToxicZone.AppliesStatusEffect().Should().BeTrue();
        DetectableHazardType.GlitchPocket.AppliesStatusEffect().Should().BeTrue();
        DetectableHazardType.AmbushSite.AppliesStatusEffect().Should().BeFalse();
    }

    [Test]
    public void DetectableHazardType_MayTriggerCombat_TrueOnlyForAmbushSite()
    {
        // Assert
        DetectableHazardType.ObviousDanger.MayTriggerCombat().Should().BeFalse();
        DetectableHazardType.HiddenPit.MayTriggerCombat().Should().BeFalse();
        DetectableHazardType.ToxicZone.MayTriggerCombat().Should().BeFalse();
        DetectableHazardType.GlitchPocket.MayTriggerCombat().Should().BeFalse();
        DetectableHazardType.AmbushSite.MayTriggerCombat().Should().BeTrue();
    }

    [Test]
    public void DetectableHazardType_HasUnpredictableEffects_TrueOnlyForGlitchPocket()
    {
        // Assert
        DetectableHazardType.ObviousDanger.HasUnpredictableEffects().Should().BeFalse();
        DetectableHazardType.HiddenPit.HasUnpredictableEffects().Should().BeFalse();
        DetectableHazardType.ToxicZone.HasUnpredictableEffects().Should().BeFalse();
        DetectableHazardType.GlitchPocket.HasUnpredictableEffects().Should().BeTrue();
        DetectableHazardType.AmbushSite.HasUnpredictableEffects().Should().BeFalse();
    }

    [Test]
    public void DetectableHazardType_GetStatusEffectName_ReturnsCorrectNames()
    {
        // Assert
        DetectableHazardType.ObviousDanger.GetStatusEffectName().Should().BeNull();
        DetectableHazardType.HiddenPit.GetStatusEffectName().Should().BeNull();
        DetectableHazardType.ToxicZone.GetStatusEffectName().Should().Be("Poisoned");
        DetectableHazardType.GlitchPocket.GetStatusEffectName().Should().Be("Disoriented");
        DetectableHazardType.AmbushSite.GetStatusEffectName().Should().BeNull();
    }

    [Test]
    public void DetectableHazardType_GetStatusEffectDuration_ReturnsCorrectDurations()
    {
        // Assert
        DetectableHazardType.ObviousDanger.GetStatusEffectDuration().Should().Be(0);
        DetectableHazardType.ToxicZone.GetStatusEffectDuration().Should().Be(3);
        DetectableHazardType.GlitchPocket.GetStatusEffectDuration().Should().Be(2);
    }

    #endregion

    #region DetectionMethod Enum Extension Tests

    [Test]
    public void DetectionMethod_GetDisplayName_ReturnsCorrectNames()
    {
        // Assert
        DetectionMethod.None.GetDisplayName().Should().Be("Not Detected");
        DetectionMethod.PassivePerception.GetDisplayName().Should().Be("Passive Perception");
        DetectionMethod.ActiveInvestigation.GetDisplayName().Should().Be("Active Investigation");
        DetectionMethod.AreaSweep.GetDisplayName().Should().Be("Area Sweep");
    }

    [Test]
    public void DetectionMethod_RequiresDiceRoll_TrueForActiveAndAreaSweep()
    {
        // Assert
        DetectionMethod.None.RequiresDiceRoll().Should().BeFalse();
        DetectionMethod.PassivePerception.RequiresDiceRoll().Should().BeFalse();
        DetectionMethod.ActiveInvestigation.RequiresDiceRoll().Should().BeTrue();
        DetectionMethod.AreaSweep.RequiresDiceRoll().Should().BeTrue();
    }

    [Test]
    public void DetectionMethod_IsAutomatic_TrueOnlyForPassive()
    {
        // Assert
        DetectionMethod.None.IsAutomatic().Should().BeFalse();
        DetectionMethod.PassivePerception.IsAutomatic().Should().BeTrue();
        DetectionMethod.ActiveInvestigation.IsAutomatic().Should().BeFalse();
        DetectionMethod.AreaSweep.IsAutomatic().Should().BeFalse();
    }

    [Test]
    public void DetectionMethod_RevealsHazardType_TrueForActiveAndAreaSweep()
    {
        // Assert
        DetectionMethod.None.RevealsHazardType().Should().BeFalse();
        DetectionMethod.PassivePerception.RevealsHazardType().Should().BeFalse();
        DetectionMethod.ActiveInvestigation.RevealsHazardType().Should().BeTrue();
        DetectionMethod.AreaSweep.RevealsHazardType().Should().BeTrue();
    }

    [Test]
    public void DetectionMethod_ProvidesHintOnly_TrueOnlyForPassive()
    {
        // Assert
        DetectionMethod.None.ProvidesHintOnly().Should().BeFalse();
        DetectionMethod.PassivePerception.ProvidesHintOnly().Should().BeTrue();
        DetectionMethod.ActiveInvestigation.ProvidesHintOnly().Should().BeFalse();
        DetectionMethod.AreaSweep.ProvidesHintOnly().Should().BeFalse();
    }

    [Test]
    public void DetectionMethod_CanDetectMultipleHazards_TrueOnlyForAreaSweep()
    {
        // Assert
        DetectionMethod.None.CanDetectMultipleHazards().Should().BeFalse();
        DetectionMethod.PassivePerception.CanDetectMultipleHazards().Should().BeFalse();
        DetectionMethod.ActiveInvestigation.CanDetectMultipleHazards().Should().BeFalse();
        DetectionMethod.AreaSweep.CanDetectMultipleHazards().Should().BeTrue();
    }

    [Test]
    public void DetectionMethod_WasSuccessful_FalseOnlyForNone()
    {
        // Assert
        DetectionMethod.None.WasSuccessful().Should().BeFalse();
        DetectionMethod.PassivePerception.WasSuccessful().Should().BeTrue();
        DetectionMethod.ActiveInvestigation.WasSuccessful().Should().BeTrue();
        DetectionMethod.AreaSweep.WasSuccessful().Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock SkillCheckService for testing.
    /// </summary>
    private static SkillCheckService CreateMockSkillCheckService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        var diceService = new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
        var configProvider = Substitute.For<IGameConfigurationProvider>();
        var logger = Substitute.For<ILogger<SkillCheckService>>();

        return new SkillCheckService(diceService, configProvider, logger);
    }

    /// <summary>
    /// Creates a DiceService with deterministic random for testing.
    /// </summary>
    private static DiceService CreateDiceService()
    {
        var seededRandom = new Random(42);
        var diceLogger = Substitute.For<ILogger<DiceService>>();
#pragma warning disable CS0618 // Type or member is obsolete
        return new DiceService(diceLogger, seededRandom);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Creates a test player with specified WITS attribute for detection tests.
    /// </summary>
    /// <param name="wits">The WITS attribute value.</param>
    private static Player CreateTestPlayer(int wits = 10)
    {
        // Create player with specific WITS attribute using the full constructor
        // Other attributes default to 8 (standard starting value)
        return new Player(
            name: "Test Investigator",
            raceId: "human",
            backgroundId: "soldier",
            attributes: new PlayerAttributes(
                might: 8,
                fortitude: 8,
                will: 8,
                wits: wits,
                finesse: 8
            )
        );
    }

    #endregion
}
