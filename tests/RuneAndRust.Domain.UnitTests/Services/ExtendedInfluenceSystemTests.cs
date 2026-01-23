// ------------------------------------------------------------------------------
// <copyright file="ExtendedInfluenceSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Extended Influence System including conviction levels,
// influence status, extended influence entity, and attempt results.
// Part of v0.15.3h Extended Influence System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.Services;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for the Extended Influence System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the required acceptance criteria from the design specification:
/// </para>
/// <list type="number">
///   <item>
///     <description>ConvictionLevel correctly maps to DC, threshold, and resistance rate</description>
///   </item>
///   <item>
///     <description>InfluenceStatus terminal and continuable states work correctly</description>
///   </item>
///   <item>
///     <description>ExtendedInfluence entity accumulates pool and resistance correctly</description>
///   </item>
///   <item>
///     <description>Threshold reached triggers conviction change</description>
///   </item>
///   <item>
///     <description>Max resistance with insufficient pool causes failure</description>
///   </item>
///   <item>
///     <description>Stall and Resume mechanics work correctly</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class ExtendedInfluenceSystemTests
{
    #region Test 1: ConvictionLevel Mechanics

    /// <summary>
    /// Verifies that each conviction level maps to the correct base DC.
    /// This is the core mechanic for determining rhetoric check difficulty.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="expectedDc">The expected base DC.</param>
    [TestCase(ConvictionLevel.WeakOpinion, 10)]
    [TestCase(ConvictionLevel.ModerateBelief, 12)]
    [TestCase(ConvictionLevel.StrongConviction, 14)]
    [TestCase(ConvictionLevel.CoreBelief, 16)]
    [TestCase(ConvictionLevel.Fanatical, 18)]
    public void GetBaseDc_ReturnsCorrectDc(ConvictionLevel conviction, int expectedDc)
    {
        // Act
        var baseDc = conviction.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc,
            because: $"{conviction} should have base DC {expectedDc}");
    }

    /// <summary>
    /// Verifies that each conviction level maps to the correct pool threshold.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="expectedThreshold">The expected pool threshold.</param>
    [TestCase(ConvictionLevel.WeakOpinion, 5)]
    [TestCase(ConvictionLevel.ModerateBelief, 10)]
    [TestCase(ConvictionLevel.StrongConviction, 15)]
    [TestCase(ConvictionLevel.CoreBelief, 20)]
    [TestCase(ConvictionLevel.Fanatical, 25)]
    public void GetPoolThreshold_ReturnsCorrectValue(
        ConvictionLevel conviction,
        int expectedThreshold)
    {
        // Act
        var threshold = conviction.GetPoolThreshold();

        // Assert
        threshold.Should().Be(expectedThreshold,
            because: $"{conviction} should require {expectedThreshold} pool points");
    }

    /// <summary>
    /// Verifies that each conviction level maps to the correct resistance per failure.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    /// <param name="expectedResistance">The expected resistance increase per failure.</param>
    [TestCase(ConvictionLevel.WeakOpinion, 0.0)]
    [TestCase(ConvictionLevel.ModerateBelief, 0.0)]
    [TestCase(ConvictionLevel.StrongConviction, 0.5)]
    [TestCase(ConvictionLevel.CoreBelief, 1.0)]
    [TestCase(ConvictionLevel.Fanatical, 2.0)]
    public void GetResistancePerFailure_ReturnsCorrectValue(
        ConvictionLevel conviction,
        decimal expectedResistance)
    {
        // Act
        var resistanceRate = conviction.GetResistancePerFailure();

        // Assert
        resistanceRate.Should().Be(expectedResistance,
            because: $"{conviction} should have resistance rate {expectedResistance}");
    }

    /// <summary>
    /// Verifies that all conviction levels have display names.
    /// </summary>
    /// <param name="conviction">The conviction level.</param>
    [TestCase(ConvictionLevel.WeakOpinion)]
    [TestCase(ConvictionLevel.ModerateBelief)]
    [TestCase(ConvictionLevel.StrongConviction)]
    [TestCase(ConvictionLevel.CoreBelief)]
    [TestCase(ConvictionLevel.Fanatical)]
    public void ConvictionLevel_GetDisplayName_ReturnsNonEmpty(ConvictionLevel conviction)
    {
        // Act
        var displayName = conviction.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{conviction} should have a display name defined");
    }

    /// <summary>
    /// Verifies that DC and threshold increase with conviction level.
    /// </summary>
    [Test]
    public void ConvictionLevel_DcAndThreshold_IncreaseWithLevel()
    {
        // Act
        var weakDc = ConvictionLevel.WeakOpinion.GetBaseDc();
        var moderateDc = ConvictionLevel.ModerateBelief.GetBaseDc();
        var strongDc = ConvictionLevel.StrongConviction.GetBaseDc();
        var coreDc = ConvictionLevel.CoreBelief.GetBaseDc();
        var fanaticalDc = ConvictionLevel.Fanatical.GetBaseDc();

        // Assert - DC should increase
        weakDc.Should().BeLessThan(moderateDc);
        moderateDc.Should().BeLessThan(strongDc);
        strongDc.Should().BeLessThan(coreDc);
        coreDc.Should().BeLessThan(fanaticalDc);
    }

    /// <summary>
    /// Verifies that threshold increases with conviction level.
    /// </summary>
    [Test]
    public void ConvictionLevel_Threshold_IncreaseWithLevel()
    {
        // Act
        var weakThreshold = ConvictionLevel.WeakOpinion.GetPoolThreshold();
        var moderateThreshold = ConvictionLevel.ModerateBelief.GetPoolThreshold();
        var strongThreshold = ConvictionLevel.StrongConviction.GetPoolThreshold();
        var coreThreshold = ConvictionLevel.CoreBelief.GetPoolThreshold();
        var fanaticalThreshold = ConvictionLevel.Fanatical.GetPoolThreshold();

        // Assert - Threshold should increase
        weakThreshold.Should().BeLessThan(moderateThreshold);
        moderateThreshold.Should().BeLessThan(strongThreshold);
        strongThreshold.Should().BeLessThan(coreThreshold);
        coreThreshold.Should().BeLessThan(fanaticalThreshold);
    }

    /// <summary>
    /// Verifies that IsHighDifficulty returns correct values.
    /// </summary>
    [Test]
    public void ConvictionLevel_IsHighDifficulty_CorrectForHigherLevels()
    {
        // Assert
        ConvictionLevel.WeakOpinion.IsHighDifficulty().Should().BeFalse();
        ConvictionLevel.ModerateBelief.IsHighDifficulty().Should().BeFalse();
        ConvictionLevel.StrongConviction.IsHighDifficulty().Should().BeFalse();
        ConvictionLevel.CoreBelief.IsHighDifficulty().Should().BeTrue();
        ConvictionLevel.Fanatical.IsHighDifficulty().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that GetEstimatedAttempts returns reasonable values.
    /// </summary>
    [Test]
    public void ConvictionLevel_GetEstimatedAttempts_ReturnsReasonableValues()
    {
        // Act
        var weakAttempts = ConvictionLevel.WeakOpinion.GetEstimatedAttempts();
        var fanaticalAttempts = ConvictionLevel.Fanatical.GetEstimatedAttempts();

        // Assert
        weakAttempts.Should().BeLessThan(fanaticalAttempts,
            because: "harder convictions should require more attempts");
        weakAttempts.Should().BeGreaterThan(0);
        fanaticalAttempts.Should().BeGreaterThan(5);
    }

    #endregion

    #region Test 2: InfluenceStatus Mechanics

    /// <summary>
    /// Verifies that terminal statuses are correctly identified.
    /// </summary>
    /// <param name="status">The influence status.</param>
    /// <param name="expectedTerminal">Whether status is terminal.</param>
    [TestCase(InfluenceStatus.Active, false)]
    [TestCase(InfluenceStatus.Successful, true)]
    [TestCase(InfluenceStatus.Failed, true)]
    [TestCase(InfluenceStatus.Stalled, false)]
    public void IsTerminal_ReturnsCorrectValue(
        InfluenceStatus status,
        bool expectedTerminal)
    {
        // Act
        var isTerminal = status.IsTerminal();

        // Assert
        isTerminal.Should().Be(expectedTerminal,
            because: $"{status} should {(expectedTerminal ? "" : "not ")}be terminal");
    }

    /// <summary>
    /// Verifies that success status is correctly identified.
    /// </summary>
    [Test]
    public void IsSuccess_OnlyTrueForSuccessful()
    {
        // Assert
        InfluenceStatus.Successful.IsSuccess().Should().BeTrue();
        InfluenceStatus.Failed.IsSuccess().Should().BeFalse();
        InfluenceStatus.Active.IsSuccess().Should().BeFalse();
        InfluenceStatus.Stalled.IsSuccess().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that failure status is correctly identified.
    /// </summary>
    [Test]
    public void IsFailure_OnlyTrueForFailed()
    {
        // Assert
        InfluenceStatus.Failed.IsFailure().Should().BeTrue();
        InfluenceStatus.Successful.IsFailure().Should().BeFalse();
        InfluenceStatus.Active.IsFailure().Should().BeFalse();
        InfluenceStatus.Stalled.IsFailure().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CanContinue is only true for Active status.
    /// </summary>
    [Test]
    public void CanContinue_OnlyTrueForActive()
    {
        // Assert
        InfluenceStatus.Active.CanContinue().Should().BeTrue();
        InfluenceStatus.Successful.CanContinue().Should().BeFalse();
        InfluenceStatus.Failed.CanContinue().Should().BeFalse();
        InfluenceStatus.Stalled.CanContinue().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CanResume is only true for Stalled status.
    /// </summary>
    [Test]
    public void CanResume_OnlyTrueForStalled()
    {
        // Assert
        InfluenceStatus.Stalled.CanResume().Should().BeTrue();
        InfluenceStatus.Active.CanResume().Should().BeFalse();
        InfluenceStatus.Successful.CanResume().Should().BeFalse();
        InfluenceStatus.Failed.CanResume().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsPaused is only true for Stalled status.
    /// </summary>
    [Test]
    public void IsPaused_OnlyTrueForStalled()
    {
        // Assert
        InfluenceStatus.Stalled.IsPaused().Should().BeTrue();
        InfluenceStatus.Active.IsPaused().Should().BeFalse();
        InfluenceStatus.Successful.IsPaused().Should().BeFalse();
        InfluenceStatus.Failed.IsPaused().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsRelevant returns false only for Failed.
    /// </summary>
    [Test]
    public void IsRelevant_FalseOnlyForFailed()
    {
        // Assert
        InfluenceStatus.Failed.IsRelevant().Should().BeFalse();
        InfluenceStatus.Active.IsRelevant().Should().BeTrue();
        InfluenceStatus.Successful.IsRelevant().Should().BeTrue();
        InfluenceStatus.Stalled.IsRelevant().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that all statuses have display names.
    /// </summary>
    /// <param name="status">The influence status.</param>
    [TestCase(InfluenceStatus.Active)]
    [TestCase(InfluenceStatus.Successful)]
    [TestCase(InfluenceStatus.Failed)]
    [TestCase(InfluenceStatus.Stalled)]
    public void InfluenceStatus_GetDisplayName_ReturnsNonEmpty(InfluenceStatus status)
    {
        // Act
        var displayName = status.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{status} should have a display name defined");
    }

    /// <summary>
    /// Verifies that all statuses have status indicators.
    /// </summary>
    /// <param name="status">The influence status.</param>
    [TestCase(InfluenceStatus.Active)]
    [TestCase(InfluenceStatus.Successful)]
    [TestCase(InfluenceStatus.Failed)]
    [TestCase(InfluenceStatus.Stalled)]
    public void InfluenceStatus_GetStatusIndicator_ReturnsNonEmpty(InfluenceStatus status)
    {
        // Act
        var indicator = status.GetStatusIndicator();

        // Assert
        indicator.Should().NotBeNullOrWhiteSpace(
            because: $"{status} should have a status indicator defined");
    }

    /// <summary>
    /// Verifies that GetSortOrder puts Active first.
    /// </summary>
    [Test]
    public void InfluenceStatus_GetSortOrder_ActiveIsFirst()
    {
        // Act
        var activeOrder = InfluenceStatus.Active.GetSortOrder();
        var stalledOrder = InfluenceStatus.Stalled.GetSortOrder();
        var successfulOrder = InfluenceStatus.Successful.GetSortOrder();
        var failedOrder = InfluenceStatus.Failed.GetSortOrder();

        // Assert - Active should be first (lowest number)
        activeOrder.Should().BeLessThan(stalledOrder);
        stalledOrder.Should().BeLessThan(successfulOrder);
        successfulOrder.Should().BeLessThan(failedOrder);
    }

    #endregion

    #region Test 3: ExtendedInfluence Entity Creation

    /// <summary>
    /// Creates a test ExtendedInfluence with specified parameters.
    /// </summary>
    private static ExtendedInfluence CreateTestInfluence(
        ConvictionLevel conviction = ConvictionLevel.ModerateBelief,
        int maxResistance = 6)
    {
        return ExtendedInfluence.Create(
            characterId: "pc_001",
            targetId: "npc_001",
            targetName: "Test NPC",
            beliefId: "belief_001",
            beliefDescription: "The old ways are best.",
            conviction: conviction,
            maxResistance: maxResistance);
    }

    /// <summary>
    /// Verifies that Create sets up influence correctly.
    /// </summary>
    [Test]
    public void Create_SetsCorrectInitialState()
    {
        // Act
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief);

        // Assert
        influence.Id.Should().NotBeEmpty();
        influence.CharacterId.Should().Be("pc_001");
        influence.TargetId.Should().Be("npc_001");
        influence.TargetName.Should().Be("Test NPC");
        influence.BeliefId.Should().Be("belief_001");
        influence.BeliefDescription.Should().Be("The old ways are best.");
        influence.TargetConviction.Should().Be(ConvictionLevel.ModerateBelief);
        influence.Status.Should().Be(InfluenceStatus.Active);
        influence.InfluencePool.Should().Be(0);
        influence.ResistanceModifier.Should().Be(0);
        influence.InteractionCount.Should().Be(0);
        influence.SuccessfulInteractions.Should().Be(0);
        influence.History.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that GetThreshold returns conviction-based value.
    /// </summary>
    [Test]
    public void GetThreshold_ReturnsConvictionBasedValue()
    {
        // Arrange
        var weakInfluence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        var fanaticalInfluence = CreateTestInfluence(ConvictionLevel.Fanatical);

        // Assert
        weakInfluence.GetThreshold().Should().Be(5);
        fanaticalInfluence.GetThreshold().Should().Be(25);
    }

    /// <summary>
    /// Verifies that GetBaseDc returns conviction-based value.
    /// </summary>
    [Test]
    public void GetBaseDc_ReturnsConvictionBasedValue()
    {
        // Arrange
        var weakInfluence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        var fanaticalInfluence = CreateTestInfluence(ConvictionLevel.Fanatical);

        // Assert
        weakInfluence.GetBaseDc().Should().Be(10);
        fanaticalInfluence.GetBaseDc().Should().Be(18);
    }

    /// <summary>
    /// Verifies that GetEffectiveDc includes resistance.
    /// </summary>
    [Test]
    public void GetEffectiveDc_IncludesResistance()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.CoreBelief);

        // Act - Add some failures to build resistance
        influence.IncrementResistance(); // +1 for CoreBelief
        influence.IncrementResistance(); // +1 for CoreBelief

        // Assert
        influence.GetBaseDc().Should().Be(16);
        influence.ResistanceModifier.Should().Be(2);
        influence.GetEffectiveDc().Should().Be(18); // 16 + 2 = 18
    }

    #endregion

    #region Test 4: AddToPool Mechanics

    /// <summary>
    /// Verifies that AddToPool accumulates successes correctly.
    /// </summary>
    [Test]
    public void AddToPool_AccumulatesSuccessesCorrectly()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief);

        // Act
        influence.AddToPool(3);
        influence.AddToPool(2);

        // Assert
        influence.InfluencePool.Should().Be(5);
        influence.InteractionCount.Should().Be(2);
        influence.SuccessfulInteractions.Should().Be(2);
    }

    /// <summary>
    /// Verifies that AddToPool triggers conviction change when threshold is reached.
    /// </summary>
    [Test]
    public void AddToPool_TriggersConvictionChange_WhenThresholdReached()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion); // Threshold: 5

        // Act
        influence.AddToPool(5);

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Successful);
        influence.ConvictionChangedAt.Should().NotBeNull();
        influence.IsThresholdReached().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that AddToPool works when exceeding threshold.
    /// </summary>
    [Test]
    public void AddToPool_WorksWhenExceedingThreshold()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion); // Threshold: 5

        // Act - Add more than threshold
        influence.AddToPool(10);

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Successful);
        influence.InfluencePool.Should().Be(10);
        influence.ProgressPercentage.Should().Be(200);
    }

    /// <summary>
    /// Verifies that AddToPool throws when not in Active state.
    /// </summary>
    [Test]
    public void AddToPool_Throws_WhenNotActive()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5); // Now Successful

        // Act
        var act = () => influence.AddToPool(1);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that zero or negative pool additions are ignored.
    /// </summary>
    [Test]
    public void AddToPool_IgnoresZeroOrNegative()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Act
        influence.AddToPool(0);
        influence.AddToPool(-5);

        // Assert
        influence.InfluencePool.Should().Be(0);
        influence.InteractionCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies progress calculation is accurate.
    /// </summary>
    [Test]
    public void ProgressPercentage_CalculatesCorrectly()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief); // Threshold: 10

        // Act
        influence.AddToPool(5); // 50%

        // Assert
        influence.ProgressPercentage.Should().Be(50);
        influence.RemainingPool.Should().Be(5);
        influence.ProgressRatio.Should().Be(0.5m);
    }

    #endregion

    #region Test 5: IncrementResistance Mechanics

    /// <summary>
    /// Verifies that WeakOpinion and ModerateBelief don't accumulate resistance.
    /// </summary>
    [Test]
    public void IncrementResistance_NoAccumulation_ForLowerConvictions()
    {
        // Arrange
        var weakInfluence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        var moderateInfluence = CreateTestInfluence(ConvictionLevel.ModerateBelief);

        // Act
        var weakIncrease = weakInfluence.IncrementResistance();
        var moderateIncrease = moderateInfluence.IncrementResistance();

        // Assert
        weakIncrease.Should().Be(0);
        moderateIncrease.Should().Be(0);
        weakInfluence.ResistanceModifier.Should().Be(0);
        moderateInfluence.ResistanceModifier.Should().Be(0);
    }

    /// <summary>
    /// Verifies that StrongConviction adds +1 resistance per 2 failures.
    /// </summary>
    [Test]
    public void IncrementResistance_AppliesCorrectly_ForStrongConviction()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.StrongConviction);

        // Act - Two failures should result in +1 resistance
        var increase1 = influence.IncrementResistance();
        var increase2 = influence.IncrementResistance();

        // Assert
        increase1.Should().Be(0); // First failure: 0.5 accumulated, 0 applied
        increase2.Should().Be(1); // Second failure: 1.0 accumulated, 1 applied
        influence.ResistanceModifier.Should().Be(1);
    }

    /// <summary>
    /// Verifies that CoreBelief adds +1 resistance per failure.
    /// </summary>
    [Test]
    public void IncrementResistance_AppliesCorrectly_ForCoreBelief()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.CoreBelief);

        // Act
        var increase1 = influence.IncrementResistance();
        var increase2 = influence.IncrementResistance();

        // Assert
        increase1.Should().Be(1);
        increase2.Should().Be(1);
        influence.ResistanceModifier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Fanatical adds +2 resistance per failure.
    /// </summary>
    [Test]
    public void IncrementResistance_AppliesCorrectly_ForFanatical()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.Fanatical);

        // Act
        var increase = influence.IncrementResistance();

        // Assert
        increase.Should().Be(2);
        influence.ResistanceModifier.Should().Be(2);
    }

    /// <summary>
    /// Verifies that resistance is capped at MaxResistance.
    /// </summary>
    [Test]
    public void IncrementResistance_CappedAtMaxResistance()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.Fanatical, maxResistance: 6);
        // Add pool to 50%+ so hitting max resistance doesn't trigger failure
        // Fanatical threshold is 25, so need 13+ (>50%)
        influence.AddToPool(15);

        // Act - Multiple failures to exceed cap
        influence.IncrementResistance(); // +2 = 2
        influence.IncrementResistance(); // +2 = 4
        influence.IncrementResistance(); // +2 = 6 (capped, but doesn't fail due to high pool)
        influence.IncrementResistance(); // +2 = 6 (still capped)

        // Assert
        influence.ResistanceModifier.Should().Be(6);
        influence.Status.Should().Be(InfluenceStatus.Active); // Should still be active
    }

    /// <summary>
    /// Verifies that max resistance with low pool causes failure.
    /// </summary>
    [Test]
    public void IncrementResistance_CausesFailure_WhenMaxResistanceWithLowPool()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.Fanatical, maxResistance: 6);
        // Pool is 0, threshold is 25, so progress is 0% (< 50%)

        // Act - Build to max resistance
        influence.IncrementResistance(); // +2 = 2
        influence.IncrementResistance(); // +2 = 4
        influence.IncrementResistance(); // +2 = 6 -> should fail

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Failed);
    }

    /// <summary>
    /// Verifies that max resistance with high pool doesn't cause failure.
    /// </summary>
    [Test]
    public void IncrementResistance_DoesNotFail_WhenMaxResistanceWithHighPool()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.CoreBelief, maxResistance: 6);
        // Add pool to get to 50%+ (threshold 20, so need 10+)
        influence.AddToPool(11); // 55%

        // Act - Build to max resistance
        for (var i = 0; i < 6; i++)
        {
            influence.IncrementResistance();
        }

        // Assert - Should still be active because pool >= 50%
        influence.Status.Should().Be(InfluenceStatus.Active);
        influence.ResistanceModifier.Should().Be(6);
    }

    /// <summary>
    /// Verifies that IncrementResistance throws when not in Active state.
    /// </summary>
    [Test]
    public void IncrementResistance_Throws_WhenNotActive()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5); // Now Successful

        // Act
        var act = () => influence.IncrementResistance();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Test 6: Stall and Resume Mechanics

    /// <summary>
    /// Verifies that Stall sets correct state.
    /// </summary>
    [Test]
    public void Stall_SetsCorrectState()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Act
        influence.Stall("Too much resistance", "Wait 24 hours");

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Stalled);
        influence.StallReason.Should().Be("Too much resistance");
        influence.ResumeCondition.Should().Be("Wait 24 hours");
    }

    /// <summary>
    /// Verifies that Stall throws when already terminal.
    /// </summary>
    [Test]
    public void Stall_Throws_WhenAlreadyTerminal()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5); // Now Successful

        // Act
        var act = () => influence.Stall("Test", "Test");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that Resume sets state back to Active.
    /// </summary>
    [Test]
    public void Resume_SetsStateToActive()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.CoreBelief);
        influence.IncrementResistance(); // Build some resistance
        influence.IncrementResistance(); // Resistance = 2
        influence.Stall("Testing", "Complete quest");

        // Act
        influence.Resume();

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Active);
        influence.StallReason.Should().BeNull();
        influence.ResumeCondition.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Resume reduces resistance.
    /// </summary>
    [Test]
    public void Resume_ReducesResistance()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.CoreBelief);
        influence.IncrementResistance(); // +1
        influence.IncrementResistance(); // +1
        influence.IncrementResistance(); // +1, total = 3
        influence.Stall("Testing", "Complete quest");

        // Act
        influence.Resume(resistanceReduction: 2);

        // Assert
        influence.ResistanceModifier.Should().Be(1); // 3 - 2 = 1
    }

    /// <summary>
    /// Verifies that Resume doesn't reduce resistance below zero.
    /// </summary>
    [Test]
    public void Resume_DoesNotReduceResistanceBelowZero()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief);
        // No resistance built (ModerateBelief doesn't accumulate)
        influence.Stall("Testing", "Wait");

        // Act
        influence.Resume(resistanceReduction: 5);

        // Assert
        influence.ResistanceModifier.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Resume throws when not Stalled.
    /// </summary>
    [Test]
    public void Resume_Throws_WhenNotStalled()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Act
        var act = () => influence.Resume();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Test 7: Fail Mechanics

    /// <summary>
    /// Verifies that Fail sets correct state.
    /// </summary>
    [Test]
    public void Fail_SetsCorrectState()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Act
        influence.Fail("Quest failed");

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Failed);
        influence.StallReason.Should().Be("Quest failed"); // Used as failure reason
    }

    /// <summary>
    /// Verifies that Fail can be called without reason.
    /// </summary>
    [Test]
    public void Fail_WorksWithoutReason()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Act
        influence.Fail();

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Failed);
    }

    /// <summary>
    /// Verifies that Fail throws when already terminal.
    /// </summary>
    [Test]
    public void Fail_Throws_WhenAlreadyTerminal()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5); // Now Successful

        // Act
        var act = () => influence.Fail();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that Fail can be called from Stalled state.
    /// </summary>
    [Test]
    public void Fail_WorksFromStalledState()
    {
        // Arrange
        var influence = CreateTestInfluence();
        influence.Stall("Blocked", "Complete quest");

        // Act
        influence.Fail("Quest failed");

        // Assert
        influence.Status.Should().Be(InfluenceStatus.Failed);
    }

    #endregion

    #region Test 8: InfluenceAttemptResult Value Object

    /// <summary>
    /// Verifies that Success factory creates correct result.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_Success_CreatesCorrectResult()
    {
        // Act - previousPool is 5, netSuccesses is 3, so newPoolTotal will be 8
        var result = InfluenceAttemptResult.Success(
            netSuccesses: 3,
            previousPool: 5,
            threshold: 10,
            resistance: 0,
            conviction: ConvictionLevel.ModerateBelief,
            interactionNumber: 2,
            diceRolled: 7,
            successesRolled: 5,
            narrative: "Your words find purchase.");

        // Assert
        result.SkillCheckSucceeded.Should().BeTrue();
        result.NetSuccesses.Should().Be(3);
        result.PoolChange.Should().Be(3);
        result.NewPoolTotal.Should().Be(8);
        result.ThresholdRequired.Should().Be(10);
        result.IsConvictionChanged.Should().BeFalse();
        result.Status.Should().Be(InfluenceStatus.Active);
        result.ProgressPercentage.Should().Be(80);
    }

    /// <summary>
    /// Verifies that Success result marks conviction changed when threshold reached.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_Success_MarksConvictionChanged_WhenThresholdReached()
    {
        // Act - previousPool is 7, netSuccesses is 3, so newPoolTotal will be 10 (threshold)
        var result = InfluenceAttemptResult.Success(
            netSuccesses: 3,
            previousPool: 7,
            threshold: 10,
            resistance: 0,
            conviction: ConvictionLevel.ModerateBelief,
            interactionNumber: 3,
            diceRolled: 7,
            successesRolled: 5,
            narrative: "Your words find purchase.");

        // Assert
        result.IsConvictionChanged.Should().BeTrue();
        result.Status.Should().Be(InfluenceStatus.Successful);
        result.IsBreakthrough.Should().BeTrue();
        result.ProgressPercentage.Should().Be(100);
    }

    /// <summary>
    /// Verifies that Failure factory creates correct result.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_Failure_CreatesCorrectResult()
    {
        // Act - previousResistance is 1, resistanceIncrease is 1, so newResistanceTotal will be 2
        var result = InfluenceAttemptResult.Failure(
            netSuccesses: 0,
            currentPool: 5,
            threshold: 15,
            previousResistance: 1,
            resistanceIncrease: 1,
            conviction: ConvictionLevel.StrongConviction,
            interactionNumber: 4,
            diceRolled: 6,
            successesRolled: 3,
            narrative: "The NPC remains unconvinced.");

        // Assert
        result.SkillCheckSucceeded.Should().BeFalse();
        result.NetSuccesses.Should().Be(0);
        result.PoolChange.Should().Be(0);
        result.ResistanceChange.Should().Be(1);
        result.NewResistanceTotal.Should().Be(2);
        result.Status.Should().Be(InfluenceStatus.Active);
    }

    /// <summary>
    /// Verifies that Failure result marks as Failed when max resistance reached.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_Failure_MarksAsFailed_WhenMaxResistanceWithLowPool()
    {
        // Act - previousResistance is 4, resistanceIncrease is 2, so newResistanceTotal will be 6 (max)
        var result = InfluenceAttemptResult.Failure(
            netSuccesses: -2,
            currentPool: 7,
            threshold: 25,
            previousResistance: 4,
            resistanceIncrease: 2,
            conviction: ConvictionLevel.Fanatical,
            interactionNumber: 5,
            diceRolled: 5,
            successesRolled: 1,
            narrative: "The NPC's resolve hardens.",
            maxResistance: 6);

        // Assert
        result.Status.Should().Be(InfluenceStatus.Failed);
    }

    /// <summary>
    /// Verifies that Stalled factory creates correct result.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_Stalled_CreatesCorrectResult()
    {
        // Act
        var result = InfluenceAttemptResult.Stalled(
            currentPool: 5,
            threshold: 15,
            resistance: 4,
            conviction: ConvictionLevel.StrongConviction,
            interactionNumber: 4,
            stallReason: "NPC becomes defensive",
            resumeCondition: "Wait 24 hours",
            narrative: "The NPC shuts down completely.");

        // Assert
        result.SkillCheckSucceeded.Should().BeFalse();
        result.Status.Should().Be(InfluenceStatus.Stalled);
        result.StallReason.Should().Be("NPC becomes defensive");
        result.ResumeCondition.Should().Be("Wait 24 hours");
    }

    /// <summary>
    /// Verifies that RemainingPool is calculated correctly.
    /// </summary>
    [Test]
    public void InfluenceAttemptResult_RemainingPool_CalculatedCorrectly()
    {
        // Act - previousPool is 4, netSuccesses is 3, so newPoolTotal will be 7
        var result = InfluenceAttemptResult.Success(
            netSuccesses: 3,
            previousPool: 4,
            threshold: 10,
            resistance: 0,
            conviction: ConvictionLevel.ModerateBelief,
            interactionNumber: 2,
            diceRolled: 6,
            successesRolled: 4,
            narrative: "Progress made.");

        // Assert
        result.RemainingPool.Should().Be(3); // 10 - 7 = 3
    }

    #endregion

    #region Test 9: GetStateSummary and Progress Display

    /// <summary>
    /// Verifies that GetStateSummary returns meaningful text.
    /// </summary>
    [Test]
    public void GetStateSummary_ReturnsComprehensiveText()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.StrongConviction);
        influence.AddToPool(5);
        influence.IncrementResistance();
        influence.IncrementResistance();

        // Act
        var summary = influence.GetStateSummary();

        // Assert
        summary.Should().Contain("Test NPC");
        summary.Should().Contain("Active");
        summary.Should().Contain("Strong Conviction");
        summary.Should().Contain("5/15");
        summary.Should().Contain("33%");
        summary.Should().Contain("14");
    }

    /// <summary>
    /// Verifies that ToProgressDisplay returns compact format.
    /// </summary>
    [Test]
    public void ToProgressDisplay_ReturnsCompactFormat()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief);
        influence.AddToPool(5);

        // Act
        var display = influence.ToProgressDisplay();

        // Assert
        display.Should().Contain("Test NPC");
        display.Should().Contain("50%");
    }

    /// <summary>
    /// Verifies that stalled influence shows stall reason in summary.
    /// </summary>
    [Test]
    public void GetStateSummary_ShowsStallReason_WhenStalled()
    {
        // Arrange
        var influence = CreateTestInfluence();
        influence.Stall("NPC is defensive", "Complete side quest");

        // Act
        var summary = influence.GetStateSummary();

        // Assert
        summary.Should().Contain("NPC is defensive");
        summary.Should().Contain("Complete side quest");
    }

    #endregion

    #region Test 10: CanSucceed and EstimatedAttempts

    /// <summary>
    /// Verifies that CanSucceed returns true for active influence.
    /// </summary>
    [Test]
    public void CanSucceed_ReturnsTrue_ForActiveInfluence()
    {
        // Arrange
        var influence = CreateTestInfluence();

        // Assert
        influence.CanSucceed().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanSucceed returns true for successful influence.
    /// </summary>
    [Test]
    public void CanSucceed_ReturnsTrue_ForSuccessfulInfluence()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5);

        // Assert
        influence.CanSucceed().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CanSucceed returns false for failed influence.
    /// </summary>
    [Test]
    public void CanSucceed_ReturnsFalse_ForFailedInfluence()
    {
        // Arrange
        var influence = CreateTestInfluence();
        influence.Fail();

        // Assert
        influence.CanSucceed().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetEstimatedAttemptsRemaining returns reasonable estimate.
    /// </summary>
    [Test]
    public void GetEstimatedAttemptsRemaining_ReturnsReasonableEstimate()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.ModerateBelief); // Threshold: 10

        // Act
        var estimate = influence.GetEstimatedAttemptsRemaining(avgNetSuccesses: 2);

        // Assert - Need 10 pool, avg 2 per success = 5 attempts
        estimate.Should().Be(5);
    }

    /// <summary>
    /// Verifies that GetEstimatedAttemptsRemaining returns zero when complete.
    /// </summary>
    [Test]
    public void GetEstimatedAttemptsRemaining_ReturnsZero_WhenComplete()
    {
        // Arrange
        var influence = CreateTestInfluence(ConvictionLevel.WeakOpinion);
        influence.AddToPool(5);

        // Act
        var estimate = influence.GetEstimatedAttemptsRemaining();

        // Assert
        estimate.Should().Be(0);
    }

    #endregion
}
