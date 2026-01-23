// ------------------------------------------------------------------------------
// <copyright file="IntimidationSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Intimidation System including target tiers, approach
// attribute selection, and reputation costs.
// Part of v0.15.3d Intimidation System implementation.
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
/// Unit tests for the Intimidation System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the three required acceptance criteria:
/// </para>
/// <list type="number">
///   <item>
///     <description>Target type sets correct DC (Coward 8, Common 12, Veteran 16, Elite 20, FactionLeader 24)</description>
///   </item>
///   <item>
///     <description>MIGHT vs WILL choice works (Physical uses MIGHT, Mental uses WILL)</description>
///   </item>
///   <item>
///     <description>Success always costs reputation (Critical -3, Success -5, Failure -10)</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class IntimidationSystemTests
{
    #region Test 1: Target Type Sets Correct DC

    /// <summary>
    /// Verifies that each intimidation target tier returns the correct base DC.
    /// This is the core mechanic for determining intimidation difficulty.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <param name="expectedDc">The expected difficulty class.</param>
    [TestCase(IntimidationTarget.Coward, 8)]
    [TestCase(IntimidationTarget.Common, 12)]
    [TestCase(IntimidationTarget.Veteran, 16)]
    [TestCase(IntimidationTarget.Elite, 20)]
    [TestCase(IntimidationTarget.FactionLeader, 24)]
    public void GetBaseDc_ReturnsCorrectDc_ForEachTargetTier(
        IntimidationTarget target,
        int expectedDc)
    {
        // Act
        var baseDc = target.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc, because: $"{target} should have DC {expectedDc}");
    }

    /// <summary>
    /// Verifies that all target tiers have unique DC values.
    /// </summary>
    [Test]
    public void GetBaseDc_AllTiersHaveUniqueDcValues()
    {
        // Arrange
        var targets = Enum.GetValues<IntimidationTarget>();
        var dcValues = new List<int>();

        // Act
        foreach (var target in targets)
        {
            dcValues.Add(target.GetBaseDc());
        }

        // Assert
        dcValues.Should().OnlyHaveUniqueItems(because: "each target tier should have a unique DC");
    }

    /// <summary>
    /// Verifies that DC values scale progressively from Coward to FactionLeader.
    /// </summary>
    [Test]
    public void GetBaseDc_ScalesProgressively()
    {
        // Act
        var cowardDc = IntimidationTarget.Coward.GetBaseDc();
        var commonDc = IntimidationTarget.Common.GetBaseDc();
        var veteranDc = IntimidationTarget.Veteran.GetBaseDc();
        var eliteDc = IntimidationTarget.Elite.GetBaseDc();
        var leaderDc = IntimidationTarget.FactionLeader.GetBaseDc();

        // Assert
        cowardDc.Should().BeLessThan(commonDc);
        commonDc.Should().BeLessThan(veteranDc);
        veteranDc.Should().BeLessThan(eliteDc);
        eliteDc.Should().BeLessThan(leaderDc);
    }

    /// <summary>
    /// Verifies that GetShortDescription includes both display name and DC.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    [TestCase(IntimidationTarget.Coward)]
    [TestCase(IntimidationTarget.Common)]
    [TestCase(IntimidationTarget.Veteran)]
    [TestCase(IntimidationTarget.Elite)]
    [TestCase(IntimidationTarget.FactionLeader)]
    public void GetShortDescription_IncludesDisplayNameAndDc(IntimidationTarget target)
    {
        // Act
        var description = target.GetShortDescription();

        // Assert
        description.Should().Contain(target.GetDisplayName());
        description.Should().Contain($"DC {target.GetBaseDc()}");
    }

    #endregion

    #region Test 2: MIGHT vs WILL Choice Works

    /// <summary>
    /// Verifies that each intimidation approach uses the correct attribute.
    /// Physical intimidation uses MIGHT, Mental intimidation uses WILL.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    /// <param name="expectedAttribute">The expected attribute name.</param>
    [TestCase(IntimidationApproach.Physical, "MIGHT")]
    [TestCase(IntimidationApproach.Mental, "WILL")]
    public void GetAttributeName_ReturnsCorrectAttribute(
        IntimidationApproach approach,
        string expectedAttribute)
    {
        // Act
        var attributeName = approach.GetAttributeName();

        // Assert
        attributeName.Should().Be(expectedAttribute,
            because: $"{approach} approach should use {expectedAttribute}");
    }

    /// <summary>
    /// Verifies that Physical approach display name includes MIGHT.
    /// </summary>
    [Test]
    public void Physical_GetDisplayName_IncludesMight()
    {
        // Act
        var displayName = IntimidationApproach.Physical.GetDisplayName();

        // Assert
        displayName.Should().Contain("MIGHT");
        displayName.Should().Contain("Physical");
    }

    /// <summary>
    /// Verifies that Mental approach display name includes WILL.
    /// </summary>
    [Test]
    public void Mental_GetDisplayName_IncludesWill()
    {
        // Act
        var displayName = IntimidationApproach.Mental.GetDisplayName();

        // Assert
        displayName.Should().Contain("WILL");
        displayName.Should().Contain("Mental");
    }

    /// <summary>
    /// Verifies that each approach has example actions defined.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    [TestCase(IntimidationApproach.Physical)]
    [TestCase(IntimidationApproach.Mental)]
    public void GetExampleActions_ReturnsNonEmptyList(IntimidationApproach approach)
    {
        // Act
        var examples = approach.GetExampleActions();

        // Assert
        examples.Should().NotBeEmpty(because: $"{approach} should have example actions");
        examples.Should().HaveCountGreaterThan(2,
            because: "each approach should have multiple examples");
    }

    /// <summary>
    /// Verifies that Physical approach benefits from visible weaponry.
    /// </summary>
    [Test]
    public void Physical_BenefitsFromVisibleWeaponry_ReturnsTrue()
    {
        // Act
        var benefitsFromWeapons = IntimidationApproach.Physical.BenefitsFromVisibleWeaponry();

        // Assert
        benefitsFromWeapons.Should().BeTrue(
            because: "Physical intimidation should benefit from visible weapons");
    }

    /// <summary>
    /// Verifies that Mental approach does not benefit from visible weaponry.
    /// </summary>
    [Test]
    public void Mental_BenefitsFromVisibleWeaponry_ReturnsFalse()
    {
        // Act
        var benefitsFromWeapons = IntimidationApproach.Mental.BenefitsFromVisibleWeaponry();

        // Assert
        benefitsFromWeapons.Should().BeFalse(
            because: "Mental intimidation relies on psychological pressure, not weapons");
    }

    /// <summary>
    /// Verifies that both approaches benefit from reputation.
    /// </summary>
    /// <param name="approach">The intimidation approach.</param>
    [TestCase(IntimidationApproach.Physical)]
    [TestCase(IntimidationApproach.Mental)]
    public void BenefitsFromReputation_ReturnsTrueForAllApproaches(IntimidationApproach approach)
    {
        // Act
        var benefitsFromRep = approach.BenefitsFromReputation();

        // Assert
        benefitsFromRep.Should().BeTrue(
            because: "both approaches should benefit from [Honored] or [Feared] reputation");
    }

    #endregion

    #region Test 3: Success Always Costs Reputation

    /// <summary>
    /// Verifies that CostOfFear always has a negative reputation cost.
    /// This is the defining mechanic of intimidation - it ALWAYS damages reputation.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="expectedCost">The expected reputation cost (always negative).</param>
    [TestCase(SkillOutcome.CriticalSuccess, -3)]
    [TestCase(SkillOutcome.ExceptionalSuccess, -5)]
    [TestCase(SkillOutcome.FullSuccess, -5)]
    [TestCase(SkillOutcome.MarginalSuccess, -5)]
    [TestCase(SkillOutcome.Failure, -10)]
    [TestCase(SkillOutcome.CriticalFailure, -10)]
    public void CostOfFear_AlwaysHasNegativeReputationCost(
        SkillOutcome outcome,
        int expectedCost)
    {
        // Arrange
        var factionId = "test_faction";

        // Act
        var costOfFear = new CostOfFear(outcome, factionId);

        // Assert
        costOfFear.ReputationCost.Should().Be(expectedCost,
            because: $"{outcome} should cost {expectedCost} reputation");
        costOfFear.ReputationCost.Should().BeNegative(
            because: "intimidation ALWAYS damages reputation");
    }

    /// <summary>
    /// Verifies that critical success has the lowest reputation penalty.
    /// </summary>
    [Test]
    public void CostOfFear_CriticalSuccess_HasLowestPenalty()
    {
        // Arrange
        var criticalCost = new CostOfFear(SkillOutcome.CriticalSuccess, "faction").ReputationCost;
        var successCost = new CostOfFear(SkillOutcome.FullSuccess, "faction").ReputationCost;
        var failureCost = new CostOfFear(SkillOutcome.Failure, "faction").ReputationCost;

        // Assert
        Math.Abs(criticalCost).Should().BeLessThan(Math.Abs(successCost),
            because: "critical success should have lower penalty than regular success");
        Math.Abs(successCost).Should().BeLessThan(Math.Abs(failureCost),
            because: "success should have lower penalty than failure");
    }

    /// <summary>
    /// Verifies that failure has the highest reputation penalty.
    /// </summary>
    [Test]
    public void CostOfFear_Failure_HasHighestPenalty()
    {
        // Arrange
        var failureCost = new CostOfFear(SkillOutcome.Failure, "faction");
        var fumbleCost = CostOfFear.ForFumble("faction");

        // Assert
        Math.Abs(failureCost.ReputationCost).Should().BeGreaterThanOrEqualTo(5,
            because: "failure penalty should be significant");
        fumbleCost.ReputationCost.Should().Be(failureCost.ReputationCost,
            because: "fumble has same reputation cost as failure");
    }

    /// <summary>
    /// Verifies that CostOfFear factory methods create correct instances.
    /// </summary>
    [Test]
    public void CostOfFear_FactoryMethods_CreateCorrectInstances()
    {
        // Arrange
        var factionId = "test_faction";

        // Act
        var criticalCost = CostOfFear.ForCriticalSuccess(factionId);
        var successCost = CostOfFear.ForSuccess(factionId);
        var failureCost = CostOfFear.ForFailure(factionId);
        var fumbleCost = CostOfFear.ForFumble(factionId);

        // Assert
        criticalCost.ReputationCost.Should().Be(-3);
        successCost.ReputationCost.Should().Be(-5);
        failureCost.ReputationCost.Should().Be(-10);
        fumbleCost.ReputationCost.Should().Be(-10);
    }

    /// <summary>
    /// Verifies that CostOfFear includes faction ID for tracking.
    /// </summary>
    [Test]
    public void CostOfFear_IncludesFactionId()
    {
        // Arrange
        var factionId = "city_guard";

        // Act
        var costOfFear = new CostOfFear(SkillOutcome.FullSuccess, factionId);

        // Assert
        costOfFear.FactionId.Should().Be(factionId);
    }

    /// <summary>
    /// Verifies that CostOfFear narrative description is not empty.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    [TestCase(SkillOutcome.CriticalSuccess)]
    [TestCase(SkillOutcome.FullSuccess)]
    [TestCase(SkillOutcome.Failure)]
    [TestCase(SkillOutcome.CriticalFailure)]
    public void CostOfFear_GetNarrativeDescription_ReturnsNonEmpty(SkillOutcome outcome)
    {
        // Arrange
        var costOfFear = new CostOfFear(outcome, "test_faction");

        // Act
        var narrative = costOfFear.GetNarrativeDescription();

        // Assert
        narrative.Should().NotBeNullOrWhiteSpace(
            because: "each outcome should have narrative flavor text");
    }

    #endregion

    #region Relative Strength Tests

    /// <summary>
    /// Verifies that FromLevels correctly categorizes relative strength.
    /// </summary>
    /// <param name="playerLevel">The player's level.</param>
    /// <param name="npcLevel">The NPC's level.</param>
    /// <param name="expected">The expected relative strength.</param>
    [TestCase(5, 3, RelativeStrength.PlayerStronger)]
    [TestCase(5, 5, RelativeStrength.Equal)]
    [TestCase(5, 4, RelativeStrength.Equal)]
    [TestCase(5, 6, RelativeStrength.Equal)]
    [TestCase(3, 5, RelativeStrength.PlayerWeaker)]
    [TestCase(3, 6, RelativeStrength.PlayerWeaker)]
    public void FromLevels_ReturnsCorrectRelativeStrength(
        int playerLevel,
        int npcLevel,
        RelativeStrength expected)
    {
        // Act
        var result = RelativeStrengthExtensions.FromLevels(playerLevel, npcLevel);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that PlayerStronger gives player bonus dice.
    /// </summary>
    [Test]
    public void PlayerStronger_GivesPlayerBonusDice()
    {
        // Act
        var playerBonus = RelativeStrength.PlayerStronger.GetPlayerBonusDice();
        var npcBonus = RelativeStrength.PlayerStronger.GetNpcBonusDice();

        // Assert
        playerBonus.Should().Be(1, because: "stronger player should get +1d10");
        npcBonus.Should().Be(0, because: "NPC should not get bonus dice");
    }

    /// <summary>
    /// Verifies that PlayerWeaker gives NPC bonus dice.
    /// </summary>
    [Test]
    public void PlayerWeaker_GivesNpcBonusDice()
    {
        // Act
        var playerBonus = RelativeStrength.PlayerWeaker.GetPlayerBonusDice();
        var npcBonus = RelativeStrength.PlayerWeaker.GetNpcBonusDice();

        // Assert
        playerBonus.Should().Be(0, because: "weaker player should not get bonus dice");
        npcBonus.Should().Be(1, because: "NPC should get +1d10 when stronger");
    }

    /// <summary>
    /// Verifies that Equal gives no bonus dice to either side.
    /// </summary>
    [Test]
    public void Equal_GivesNoBonusDice()
    {
        // Act
        var playerBonus = RelativeStrength.Equal.GetPlayerBonusDice();
        var npcBonus = RelativeStrength.Equal.GetNpcBonusDice();

        // Assert
        playerBonus.Should().Be(0);
        npcBonus.Should().Be(0);
    }

    #endregion

    #region IntimidationResult Tests

    /// <summary>
    /// Verifies that successful intimidation result has expected properties.
    /// </summary>
    [Test]
    public void IntimidationResult_Success_HasCorrectProperties()
    {
        // Arrange & Act
        var result = IntimidationResult.Success(
            SkillOutcome.FullSuccess,
            "test_faction",
            playerSuccesses: 4,
            dc: 12);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFumble.Should().BeFalse();
        result.CombatInitiated.Should().BeFalse();
        result.NpcGainsFurious.Should().BeFalse();
        result.StressCost.Should().Be(0, because: "success has no stress cost");
        result.TargetComplied.Should().BeTrue();
        result.ReputationCost.ReputationCost.Should().BeNegative();
    }

    /// <summary>
    /// Verifies that failed intimidation result has expected properties.
    /// </summary>
    [Test]
    public void IntimidationResult_Failure_HasCorrectProperties()
    {
        // Arrange & Act
        var result = IntimidationResult.Failure(
            "test_faction",
            playerSuccesses: 2,
            dc: 12);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFumble.Should().BeFalse();
        result.CombatInitiated.Should().BeFalse();
        result.StressCost.Should().Be(0, because: "failure has no stress cost (only fumble does)");
        result.TargetComplied.Should().BeFalse();
        result.DispositionChange.Should().Be(IntimidationResult.FailureDispositionPenalty);
    }

    /// <summary>
    /// Verifies that fumble initiates combat with [Challenge Accepted].
    /// </summary>
    [Test]
    public void IntimidationResult_Fumble_InitiatesCombat()
    {
        // Arrange
        var fumbleConsequence = new FumbleConsequence(
            consequenceId: "test_fumble",
            characterId: "player",
            skillId: "rhetoric_intimidation",
            consequenceType: FumbleType.ChallengeAccepted,
            targetId: "test_npc",
            appliedAt: DateTime.UtcNow,
            expiresAt: null,
            description: "[Challenge Accepted] - NPC attacks",
            recoveryCondition: "Combat ends");

        // Act
        var result = IntimidationResult.Fumble(
            "test_faction",
            playerSuccesses: 0,
            dc: 12,
            fumbleConsequence);

        // Assert
        result.IsFumble.Should().BeTrue();
        result.CombatInitiated.Should().BeTrue();
        result.NpcGainsFurious.Should().BeTrue();
        result.NpcWillAcceptSurrender.Should().BeFalse();
        result.StressCost.Should().Be(IntimidationResult.FumbleStressCost,
            because: "fumble incurs +5 Psychic Stress");
        result.NpcInitiativeBonus.Should().Be(IntimidationResult.FumbleInitiativeBonus);
        result.HasSevereConsequences.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that fumble constants match specification.
    /// </summary>
    [Test]
    public void IntimidationResult_FumbleConstants_MatchSpecification()
    {
        // Assert
        IntimidationResult.FumbleStressCost.Should().Be(5,
            because: "fumble should cost +5 Psychic Stress");
        IntimidationResult.FumbleInitiativeBonus.Should().Be(2,
            because: "NPC should gain +2 initiative on fumble");
        IntimidationResult.FumbleDispositionPenalty.Should().Be(-30,
            because: "fumble should cause -30 disposition");
    }

    /// <summary>
    /// Verifies that compliance level scales with outcome tier.
    /// </summary>
    [Test]
    public void IntimidationResult_ComplianceScalesWithOutcome()
    {
        // Arrange & Act
        var criticalResult = IntimidationResult.Success(SkillOutcome.CriticalSuccess, "f", 6, 12);
        var exceptionalResult = IntimidationResult.Success(SkillOutcome.ExceptionalSuccess, "f", 5, 12);
        var fullResult = IntimidationResult.Success(SkillOutcome.FullSuccess, "f", 4, 12);
        var marginalResult = IntimidationResult.Success(SkillOutcome.MarginalSuccess, "f", 3, 12);

        // Assert
        criticalResult.TargetCompliance.Should().Be(TargetCompliance.Complete);
        exceptionalResult.TargetCompliance.Should().Be(TargetCompliance.Full);
        fullResult.TargetCompliance.Should().Be(TargetCompliance.Reluctant);
        marginalResult.TargetCompliance.Should().Be(TargetCompliance.Minimal);
    }

    #endregion

    #region IntimidationContext Tests

    /// <summary>
    /// Verifies that IntimidationContext calculates correct base DC from target type.
    /// </summary>
    [Test]
    public void IntimidationContext_BaseDc_MatchesTargetType()
    {
        // Arrange
        var context = IntimidationContext.CreateMinimal(
            "test_npc",
            IntimidationTarget.Veteran,
            "test_faction",
            IntimidationApproach.Physical,
            RelativeStrength.Equal);

        // Assert
        context.BaseDc.Should().Be(16, because: "Veteran target has DC 16");
    }

    /// <summary>
    /// Verifies that PlayerStronger adds to player dice pool.
    /// </summary>
    [Test]
    public void IntimidationContext_PlayerStronger_AddsToPlayerDice()
    {
        // Arrange
        var context = IntimidationContext.CreateMinimal(
            "test_npc",
            IntimidationTarget.Common,
            "test_faction",
            IntimidationApproach.Physical,
            RelativeStrength.PlayerStronger);

        // Assert
        context.PlayerBonusDice.Should().BeGreaterThanOrEqualTo(1,
            because: "PlayerStronger should grant at least +1d10");
    }

    /// <summary>
    /// Verifies that PlayerWeaker adds to NPC resistance.
    /// </summary>
    [Test]
    public void IntimidationContext_PlayerWeaker_AddsToNpcDice()
    {
        // Arrange
        var context = IntimidationContext.CreateMinimal(
            "test_npc",
            IntimidationTarget.Common,
            "test_faction",
            IntimidationApproach.Physical,
            RelativeStrength.PlayerWeaker);

        // Assert
        context.NpcBonusDice.Should().BeGreaterThanOrEqualTo(1,
            because: "PlayerWeaker should grant NPC at least +1d10");
    }

    /// <summary>
    /// Verifies that IsHighRisk is true for high-tier targets.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <param name="expectedHighRisk">Whether the target is high risk.</param>
    [TestCase(IntimidationTarget.Coward, false)]
    [TestCase(IntimidationTarget.Common, false)]
    [TestCase(IntimidationTarget.Veteran, true)]
    [TestCase(IntimidationTarget.Elite, true)]
    [TestCase(IntimidationTarget.FactionLeader, true)]
    public void IntimidationContext_IsHighRisk_CorrectForTargetType(
        IntimidationTarget target,
        bool expectedHighRisk)
    {
        // Arrange
        var context = IntimidationContext.CreateMinimal(
            "test_npc",
            target,
            "test_faction",
            IntimidationApproach.Physical,
            RelativeStrength.Equal);

        // Assert
        context.IsHighRisk.Should().Be(expectedHighRisk,
            because: $"{target} should{(expectedHighRisk ? "" : " not")} be high risk");
    }

    #endregion

    #region Target Resistance Tests

    /// <summary>
    /// Verifies that higher-tier targets are more likely to resist on fumble.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    /// <param name="expectedLikelyToResist">Whether the target is likely to resist.</param>
    [TestCase(IntimidationTarget.Coward, false)]
    [TestCase(IntimidationTarget.Common, false)]
    [TestCase(IntimidationTarget.Veteran, true)]
    [TestCase(IntimidationTarget.Elite, true)]
    [TestCase(IntimidationTarget.FactionLeader, true)]
    public void IsLikelyToResist_CorrectForTargetTier(
        IntimidationTarget target,
        bool expectedLikelyToResist)
    {
        // Act
        var likelyToResist = target.IsLikelyToResist();

        // Assert
        likelyToResist.Should().Be(expectedLikelyToResist,
            because: $"{target} should{(expectedLikelyToResist ? "" : " not")} likely resist");
    }

    /// <summary>
    /// Verifies that all target tiers have examples defined.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    [TestCase(IntimidationTarget.Coward)]
    [TestCase(IntimidationTarget.Common)]
    [TestCase(IntimidationTarget.Veteran)]
    [TestCase(IntimidationTarget.Elite)]
    [TestCase(IntimidationTarget.FactionLeader)]
    public void GetExamples_ReturnsNonEmptyList(IntimidationTarget target)
    {
        // Act
        var examples = target.GetExamples();

        // Assert
        examples.Should().NotBeEmpty(because: $"{target} should have example NPCs");
    }

    /// <summary>
    /// Verifies that all target tiers have typical responses defined.
    /// </summary>
    /// <param name="target">The intimidation target tier.</param>
    [TestCase(IntimidationTarget.Coward)]
    [TestCase(IntimidationTarget.Common)]
    [TestCase(IntimidationTarget.Veteran)]
    [TestCase(IntimidationTarget.Elite)]
    [TestCase(IntimidationTarget.FactionLeader)]
    public void GetTypicalResponse_ReturnsNonEmpty(IntimidationTarget target)
    {
        // Act
        var response = target.GetTypicalResponse();

        // Assert
        response.Should().NotBeNullOrWhiteSpace(
            because: $"{target} should have a typical response defined");
    }

    #endregion
}
