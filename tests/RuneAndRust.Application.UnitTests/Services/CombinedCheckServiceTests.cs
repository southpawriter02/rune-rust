// ------------------------------------------------------------------------------
// <copyright file="CombinedCheckServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the CombinedCheckService, covering synergy definitions,
// primary/secondary check execution, and narrative generation.
// Part of v0.15.5i Exploration Synergies implementation.
// </summary>
// ------------------------------------------------------------------------------

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
/// Unit tests for the <see cref="CombinedCheckService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the following areas:
/// <list type="bullet">
///   <item><description>Synergy definition queries</description></item>
///   <item><description>Primary/secondary check execution flow</description></item>
///   <item><description>Secondary check timing based on primary result</description></item>
///   <item><description>Narrative generation for various outcomes</description></item>
///   <item><description>Available synergies based on context</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class CombinedCheckServiceTests
{
    // =========================================================================
    // TEST DEPENDENCIES
    // =========================================================================

    private ISkillService _skillService = null!;
    private ILogger<CombinedCheckService> _logger = null!;
    private CombinedCheckService _service = null!;
    private Player _player = null!;

    // =========================================================================
    // CONSTANTS
    // =========================================================================

    private const string TestPlayerId = "test-player-001";
    private const string TestLocationId = "wasteland-zone-1";

    // =========================================================================
    // SETUP AND TEARDOWN
    // =========================================================================

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<CombinedCheckService>>();
        _skillService = Substitute.For<ISkillService>();
        _player = CreateTestPlayer();

        _service = new CombinedCheckService(_skillService, _logger);
    }

    private static Player CreateTestPlayer()
    {
        return new Player("TestPlayer");
    }

    // =========================================================================
    // MOCK HELPER METHODS
    // =========================================================================

    /// <summary>
    /// Creates a successful SkillCheckOutcome.
    /// </summary>
    private static SkillCheckOutcome CreateSuccessfulOutcome(string skillId = "test-skill") =>
        new(skillId, true, 15, 3, 18, 12, 6, "Success!");

    /// <summary>
    /// Creates a failed SkillCheckOutcome.
    /// </summary>
    private static SkillCheckOutcome CreateFailedOutcome(string skillId = "test-skill") =>
        new(skillId, false, 5, 3, 8, 12, -4, "Failed!");

    /// <summary>
    /// Configures the skill service for combined check tests.
    /// </summary>
    private void SetupCombinedCheckMocks(bool primarySucceeds, bool secondarySucceeds)
    {
        var primaryOutcome = primarySucceeds
            ? CreateSuccessfulOutcome("wasteland-survival")
            : CreateFailedOutcome("wasteland-survival");

        var secondaryOutcome = secondarySucceeds
            ? CreateSuccessfulOutcome("acrobatics")
            : CreateFailedOutcome("acrobatics");

        _skillService.PerformSkillCheck(Arg.Any<Player>(), "wasteland-survival", Arg.Any<int>())
            .Returns(primaryOutcome);
        _skillService.PerformSkillCheck(Arg.Any<Player>(), "acrobatics", Arg.Any<int>())
            .Returns(secondaryOutcome);
        _skillService.PerformSkillCheck(Arg.Any<Player>(), "system-bypass", Arg.Any<int>())
            .Returns(secondaryOutcome);
    }

    // =========================================================================
    // SYNERGY DEFINITION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GetSynergyDefinition returns correct skills for all synergy types.
    /// </summary>
    [TestCase(SynergyType.FindHiddenPath, WastelandSurvivalCheckType.Navigation, "acrobatics")]
    [TestCase(SynergyType.TrackToLair, WastelandSurvivalCheckType.Tracking, "system-bypass")]
    [TestCase(SynergyType.AvoidPatrol, WastelandSurvivalCheckType.HazardDetection, "acrobatics")]
    [TestCase(SynergyType.FindAndLoot, WastelandSurvivalCheckType.Foraging, "system-bypass")]
    public void GetSynergyDefinition_AllSynergies_ReturnsCorrectSkills(
        SynergyType synergyType,
        WastelandSurvivalCheckType expectedPrimary,
        string expectedSecondary)
    {
        // Act
        var definition = _service.GetSynergyDefinition(synergyType);

        // Assert
        definition.PrimarySkill.Should().Be(expectedPrimary);
        definition.SecondarySkillId.Should().Be(expectedSecondary);
    }

    /// <summary>
    /// Verifies that all synergy definitions use OnPrimarySuccess timing.
    /// </summary>
    [TestCase(SynergyType.FindHiddenPath)]
    [TestCase(SynergyType.TrackToLair)]
    [TestCase(SynergyType.AvoidPatrol)]
    [TestCase(SynergyType.FindAndLoot)]
    public void GetSynergyDefinition_AllSynergies_UseOnPrimarySuccessTiming(SynergyType synergyType)
    {
        // Act
        var definition = _service.GetSynergyDefinition(synergyType);

        // Assert
        definition.SecondaryTiming.Should().Be(SecondaryCheckTiming.OnPrimarySuccess);
    }

    // =========================================================================
    // COMBINED CHECK EXECUTION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that secondary check executes when primary succeeds.
    /// </summary>
    [Test]
    public void ExecuteCombinedCheck_PrimarySucceeds_ExecutesSecondaryCheck()
    {
        // Arrange
        SetupCombinedCheckMocks(primarySucceeds: true, secondarySucceeds: true);
        var context = CombinedCheckContext.WithDcs(
            TestPlayerId, SynergyType.FindHiddenPath, 3, 3);

        // Act
        var result = _service.ExecuteCombinedCheck(_player, SynergyType.FindHiddenPath, context);

        // Assert
        result.PrimarySucceeded.Should().BeTrue();
        result.SecondaryExecuted.Should().BeTrue();
        result.SecondarySucceeded.Should().BeTrue();
        result.OverallSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that secondary check is skipped when primary fails.
    /// </summary>
    [Test]
    public void ExecuteCombinedCheck_PrimaryFails_DoesNotExecuteSecondary()
    {
        // Arrange
        SetupCombinedCheckMocks(primarySucceeds: false, secondarySucceeds: true);
        var context = CombinedCheckContext.WithDcs(
            TestPlayerId, SynergyType.FindHiddenPath, 3, 3);

        // Act
        var result = _service.ExecuteCombinedCheck(_player, SynergyType.FindHiddenPath, context);

        // Assert
        result.PrimarySucceeded.Should().BeFalse();
        result.SecondaryExecuted.Should().BeFalse();
        result.OverallSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that partial success occurs when primary succeeds but secondary fails.
    /// </summary>
    [Test]
    public void ExecuteCombinedCheck_PrimarySucceedsSecondaryFails_ReturnsPartialSuccess()
    {
        // Arrange
        SetupCombinedCheckMocks(primarySucceeds: true, secondarySucceeds: false);
        var context = CombinedCheckContext.WithDcs(
            TestPlayerId, SynergyType.TrackToLair, 3, 3);

        // Act
        var result = _service.ExecuteCombinedCheck(_player, SynergyType.TrackToLair, context);

        // Assert
        result.PrimarySucceeded.Should().BeTrue();
        result.SecondaryExecuted.Should().BeTrue();
        result.SecondarySucceeded.Should().BeFalse();
        result.OverallSuccess.Should().BeFalse();
        result.IsPartialSuccess.Should().BeTrue();
    }

    // =========================================================================
    // SECONDARY CHECK TIMING TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that ShouldExecuteSecondary returns true for successful primary.
    /// </summary>
    [Test]
    public void ShouldExecuteSecondary_PrimarySucceeds_ReturnsTrue()
    {
        // Arrange
        var primaryResult = SimpleCheckOutcome.Succeeded(margin: 3);

        // Act
        var shouldExecute = _service.ShouldExecuteSecondary(
            SynergyType.FindHiddenPath, primaryResult);

        // Assert
        shouldExecute.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ShouldExecuteSecondary returns false for failed primary.
    /// </summary>
    [Test]
    public void ShouldExecuteSecondary_PrimaryFails_ReturnsFalse()
    {
        // Arrange
        var primaryResult = SimpleCheckOutcome.Failed(margin: -3);

        // Act
        var shouldExecute = _service.ShouldExecuteSecondary(
            SynergyType.FindHiddenPath, primaryResult);

        // Assert
        shouldExecute.Should().BeFalse();
    }

    // =========================================================================
    // AVAILABLE SYNERGIES TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GetAvailableSynergies returns correct synergies based on context.
    /// </summary>
    [Test]
    public void GetAvailableSynergies_StandardContext_ReturnsNavigationAndForaging()
    {
        // Arrange
        var context = ExplorationContext.Standard(TestLocationId);

        // Act
        var synergies = _service.GetAvailableSynergies(context);

        // Assert
        synergies.Should().Contain(SynergyType.FindHiddenPath);
        synergies.Should().Contain(SynergyType.FindAndLoot);
        synergies.Should().NotContain(SynergyType.TrackToLair);
        synergies.Should().NotContain(SynergyType.AvoidPatrol);
    }

    /// <summary>
    /// Verifies that GetAvailableSynergies returns all synergies when context allows.
    /// </summary>
    [Test]
    public void GetAvailableSynergies_AllSynergiesContext_ReturnsFourSynergies()
    {
        // Arrange
        var context = ExplorationContext.All(TestLocationId);

        // Act
        var synergies = _service.GetAvailableSynergies(context);

        // Assert
        synergies.Should().HaveCount(4);
        synergies.Should().Contain(SynergyType.FindHiddenPath);
        synergies.Should().Contain(SynergyType.TrackToLair);
        synergies.Should().Contain(SynergyType.AvoidPatrol);
        synergies.Should().Contain(SynergyType.FindAndLoot);
    }

    /// <summary>
    /// Verifies that GetAvailableSynergies returns empty for empty context.
    /// </summary>
    [Test]
    public void GetAvailableSynergies_EmptyContext_ReturnsEmpty()
    {
        // Arrange
        var context = ExplorationContext.Empty(TestLocationId);

        // Act
        var synergies = _service.GetAvailableSynergies(context);

        // Assert
        synergies.Should().BeEmpty();
    }

    // =========================================================================
    // NARRATIVE GENERATION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GenerateNarrative produces appropriate text for full success.
    /// </summary>
    [Test]
    public void GenerateNarrative_FullSuccess_ContainsPositiveText()
    {
        // Arrange
        var primaryResult = SimpleCheckOutcome.Succeeded(margin: 4);
        var secondaryResult = SimpleCheckOutcome.Succeeded(margin: 3);

        // Act
        var narrative = _service.GenerateNarrative(
            SynergyType.FindHiddenPath, primaryResult, secondaryResult);

        // Assert
        narrative.Should().Contain("discover");
        narrative.Should().Contain("traverse");
        narrative.Should().Contain("success");
    }

    /// <summary>
    /// Verifies that GenerateNarrative produces appropriate text for primary failure.
    /// </summary>
    [Test]
    public void GenerateNarrative_PrimaryFailure_ContainsFailureText()
    {
        // Arrange
        var primaryResult = SimpleCheckOutcome.Failed(margin: -3);

        // Act
        var narrative = _service.GenerateNarrative(
            SynergyType.FindHiddenPath, primaryResult, null);

        // Assert
        narrative.Should().Contain("find no hidden");
    }

    // =========================================================================
    // SKILL QUERY TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GetPrimarySkill returns correct skill types.
    /// </summary>
    [TestCase(SynergyType.FindHiddenPath, WastelandSurvivalCheckType.Navigation)]
    [TestCase(SynergyType.TrackToLair, WastelandSurvivalCheckType.Tracking)]
    [TestCase(SynergyType.AvoidPatrol, WastelandSurvivalCheckType.HazardDetection)]
    [TestCase(SynergyType.FindAndLoot, WastelandSurvivalCheckType.Foraging)]
    public void GetPrimarySkill_AllSynergies_ReturnsCorrectSkillType(
        SynergyType synergyType,
        WastelandSurvivalCheckType expectedSkill)
    {
        // Act
        var skill = _service.GetPrimarySkill(synergyType);

        // Assert
        skill.Should().Be(expectedSkill);
    }

    /// <summary>
    /// Verifies that GetSecondarySkillId returns correct skill identifiers.
    /// </summary>
    [TestCase(SynergyType.FindHiddenPath, "acrobatics")]
    [TestCase(SynergyType.TrackToLair, "system-bypass")]
    [TestCase(SynergyType.AvoidPatrol, "acrobatics")]
    [TestCase(SynergyType.FindAndLoot, "system-bypass")]
    public void GetSecondarySkillId_AllSynergies_ReturnsCorrectSkillId(
        SynergyType synergyType,
        string expectedSkillId)
    {
        // Act
        var skillId = _service.GetSecondarySkillId(synergyType);

        // Assert
        skillId.Should().Be(expectedSkillId);
    }

    // =========================================================================
    // VALUE OBJECT TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that SkillSynergyDefinition static factories create correct definitions.
    /// </summary>
    [Test]
    public void SkillSynergyDefinition_StaticFactories_CreateCorrectDefinitions()
    {
        // Act & Assert
        var findPath = SkillSynergyDefinition.FindHiddenPath();
        findPath.SynergyType.Should().Be(SynergyType.FindHiddenPath);
        findPath.PrimarySkill.Should().Be(WastelandSurvivalCheckType.Navigation);
        findPath.SecondarySkillId.Should().Be("acrobatics");
        findPath.SecondaryTiming.Should().Be(SecondaryCheckTiming.OnPrimarySuccess);

        var trackToLair = SkillSynergyDefinition.TrackToLair();
        trackToLair.SynergyType.Should().Be(SynergyType.TrackToLair);
        trackToLair.SecondarySkillId.Should().Be("system-bypass");

        SkillSynergyDefinition.GetAll().Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that CombinedCheckResult factory methods create correct results.
    /// </summary>
    [Test]
    public void CombinedCheckResult_FactoryMethods_CreateCorrectResults()
    {
        // Arrange
        var primaryResult = SimpleCheckOutcome.Succeeded(margin: 4);
        var secondaryResult = SimpleCheckOutcome.Succeeded(margin: 3);

        // Act & Assert - PrimaryFailed
        var failed = CombinedCheckResult.PrimaryFailed(
            SynergyType.FindHiddenPath, primaryResult, "No path found.");
        failed.OverallSuccess.Should().BeFalse();
        failed.SecondaryExecuted.Should().BeFalse();

        // Act & Assert - FullSuccess
        var fullSuccess = CombinedCheckResult.FullSuccess(
            SynergyType.FindHiddenPath, primaryResult, secondaryResult, "Path found and traversed.");
        fullSuccess.OverallSuccess.Should().BeTrue();
        fullSuccess.SecondaryExecuted.Should().BeTrue();
        fullSuccess.SecondarySucceeded.Should().BeTrue();

        // Act & Assert - PartialSuccess
        var failedSecondary = SimpleCheckOutcome.Failed(margin: -2);
        var partial = CombinedCheckResult.PartialSuccess(
            SynergyType.FindHiddenPath, primaryResult, failedSecondary, "Path found but not traversed.");
        partial.OverallSuccess.Should().BeFalse();
        partial.IsPartialSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that ExplorationContext factory methods create correct contexts.
    /// </summary>
    [Test]
    public void ExplorationContext_FactoryMethods_CreateCorrectContexts()
    {
        // Act & Assert - Empty
        var empty = ExplorationContext.Empty(TestLocationId);
        empty.HasAnySynergies.Should().BeFalse();
        empty.AvailableSynergyCount.Should().Be(0);

        // Act & Assert - All
        var all = ExplorationContext.All(TestLocationId);
        all.HasAnySynergies.Should().BeTrue();
        all.AvailableSynergyCount.Should().Be(4);

        // Act & Assert - Standard
        var standard = ExplorationContext.Standard(TestLocationId);
        standard.AllowsNavigation.Should().BeTrue();
        standard.AllowsForaging.Should().BeTrue();
        standard.HasActiveTracking.Should().BeFalse();
        standard.HasPatrols.Should().BeFalse();

        // Act & Assert - Dangerous
        var dangerous = ExplorationContext.Dangerous(TestLocationId);
        dangerous.HasPatrols.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SimpleCheckOutcome factory methods create correct outcomes.
    /// </summary>
    [Test]
    public void SimpleCheckOutcome_FactoryMethods_CreateCorrectOutcomes()
    {
        // Act & Assert - Succeeded
        var success = SimpleCheckOutcome.Succeeded(margin: 3, isCritical: false, message: "Great!");
        success.IsSuccess.Should().BeTrue();
        success.Margin.Should().Be(3);
        success.IsCriticalSuccess.Should().BeFalse();
        success.IsFumble.Should().BeFalse();

        // Act & Assert - Failed
        var failure = SimpleCheckOutcome.Failed(margin: -4, isFumble: false, message: "Oops!");
        failure.IsSuccess.Should().BeFalse();
        failure.Margin.Should().Be(-4);
        failure.IsFumble.Should().BeFalse();

        // Act & Assert - Critical success
        var critical = SimpleCheckOutcome.Succeeded(margin: 6, isCritical: true);
        critical.IsCriticalSuccess.Should().BeTrue();

        // Act & Assert - Fumble
        var fumble = SimpleCheckOutcome.Failed(margin: -6, isFumble: true);
        fumble.IsFumble.Should().BeTrue();
    }
}
