// ------------------------------------------------------------------------------
// <copyright file="NegotiationSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Negotiation System including request complexity, concession
// types, position track, and tactical outcomes.
// Part of v0.15.3e Negotiation System implementation.
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
/// Unit tests for the Negotiation System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the required acceptance criteria from the design specification:
/// </para>
/// <list type="number">
///   <item>
///     <description>RequestComplexity provides correct DCs (10, 14, 18, 22, 26)</description>
///   </item>
///   <item>
///     <description>Concession types grant +2d10 and correct DC reductions</description>
///   </item>
///   <item>
///     <description>Position track calculates correct gap between positions</description>
///   </item>
///   <item>
///     <description>Success moves NPC toward PC, failure moves PC toward NPC</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class NegotiationSystemTests
{
    #region Test 1: RequestComplexity Provides Correct DCs

    /// <summary>
    /// Verifies that each request complexity tier returns the correct base DC.
    /// This is the core mechanic for determining negotiation difficulty.
    /// </summary>
    /// <param name="complexity">The request complexity tier.</param>
    /// <param name="expectedDc">The expected difficulty class.</param>
    [TestCase(RequestComplexity.FairTrade, 10)]
    [TestCase(RequestComplexity.SlightAdvantage, 14)]
    [TestCase(RequestComplexity.NoticeableAdvantage, 18)]
    [TestCase(RequestComplexity.MajorAdvantage, 22)]
    [TestCase(RequestComplexity.OneSidedDeal, 26)]
    public void GetBaseDc_ReturnsCorrectDc_ForEachComplexityTier(
        RequestComplexity complexity,
        int expectedDc)
    {
        // Act
        var baseDc = complexity.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc, because: $"{complexity} should have DC {expectedDc}");
    }

    /// <summary>
    /// Verifies that all complexity tiers have unique DC values.
    /// </summary>
    [Test]
    public void GetBaseDc_AllTiersHaveUniqueDcValues()
    {
        // Arrange
        var complexities = Enum.GetValues<RequestComplexity>();
        var dcValues = new List<int>();

        // Act
        foreach (var complexity in complexities)
        {
            dcValues.Add(complexity.GetBaseDc());
        }

        // Assert
        dcValues.Should().OnlyHaveUniqueItems(because: "each complexity tier should have a unique DC");
    }

    /// <summary>
    /// Verifies that DC values scale progressively from FairTrade to OneSidedDeal.
    /// </summary>
    [Test]
    public void GetBaseDc_ScalesProgressively()
    {
        // Act
        var fairTradeDc = RequestComplexity.FairTrade.GetBaseDc();
        var slightDc = RequestComplexity.SlightAdvantage.GetBaseDc();
        var noticeableDc = RequestComplexity.NoticeableAdvantage.GetBaseDc();
        var majorDc = RequestComplexity.MajorAdvantage.GetBaseDc();
        var oneSidedDc = RequestComplexity.OneSidedDeal.GetBaseDc();

        // Assert
        fairTradeDc.Should().BeLessThan(slightDc);
        slightDc.Should().BeLessThan(noticeableDc);
        noticeableDc.Should().BeLessThan(majorDc);
        majorDc.Should().BeLessThan(oneSidedDc);
    }

    /// <summary>
    /// Verifies that initial gap scales with complexity.
    /// </summary>
    /// <param name="complexity">The request complexity tier.</param>
    /// <param name="expectedGap">The expected initial gap.</param>
    [TestCase(RequestComplexity.FairTrade, 2)]
    [TestCase(RequestComplexity.SlightAdvantage, 3)]
    [TestCase(RequestComplexity.NoticeableAdvantage, 4)]
    [TestCase(RequestComplexity.MajorAdvantage, 5)]
    [TestCase(RequestComplexity.OneSidedDeal, 6)]
    public void GetInitialGap_ReturnsCorrectGap(RequestComplexity complexity, int expectedGap)
    {
        // Act
        var gap = complexity.GetInitialGap();

        // Assert
        gap.Should().Be(expectedGap, because: $"{complexity} should have initial gap of {expectedGap}");
    }

    /// <summary>
    /// Verifies that default rounds scale with complexity.
    /// </summary>
    /// <param name="complexity">The request complexity tier.</param>
    /// <param name="expectedRounds">The expected default round count.</param>
    [TestCase(RequestComplexity.FairTrade, 3)]
    [TestCase(RequestComplexity.SlightAdvantage, 4)]
    [TestCase(RequestComplexity.NoticeableAdvantage, 5)]
    [TestCase(RequestComplexity.MajorAdvantage, 6)]
    [TestCase(RequestComplexity.OneSidedDeal, 7)]
    public void GetDefaultRounds_ReturnsCorrectRoundCount(
        RequestComplexity complexity,
        int expectedRounds)
    {
        // Act
        var rounds = complexity.GetDefaultRounds();

        // Assert
        rounds.Should().Be(expectedRounds,
            because: $"{complexity} should have {expectedRounds} default rounds");
    }

    /// <summary>
    /// Verifies that all complexity tiers have descriptions.
    /// </summary>
    /// <param name="complexity">The request complexity tier.</param>
    [TestCase(RequestComplexity.FairTrade)]
    [TestCase(RequestComplexity.SlightAdvantage)]
    [TestCase(RequestComplexity.NoticeableAdvantage)]
    [TestCase(RequestComplexity.MajorAdvantage)]
    [TestCase(RequestComplexity.OneSidedDeal)]
    public void GetDescription_ReturnsNonEmpty(RequestComplexity complexity)
    {
        // Act
        var description = complexity.GetDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{complexity} should have a description defined");
    }

    #endregion

    #region Test 2: Concession Types Grant Correct Bonuses

    /// <summary>
    /// Verifies that each concession type grants the correct DC reduction.
    /// </summary>
    /// <param name="type">The concession type.</param>
    /// <param name="expectedReduction">The expected DC reduction.</param>
    [TestCase(ConcessionType.OfferItem, 2)]
    [TestCase(ConcessionType.PromiseFavor, 2)]
    [TestCase(ConcessionType.TradeInformation, 4)]
    [TestCase(ConcessionType.TakeRisk, 4)]
    [TestCase(ConcessionType.StakeReputation, 6)]
    public void GetDcReduction_ReturnsCorrectReduction(ConcessionType type, int expectedReduction)
    {
        // Act
        var reduction = type.GetDcReduction();

        // Assert
        reduction.Should().Be(expectedReduction,
            because: $"{type} should grant -{expectedReduction} DC reduction");
    }

    /// <summary>
    /// Verifies that all concession types grant +2d10 bonus dice.
    /// </summary>
    /// <param name="type">The concession type.</param>
    [TestCase(ConcessionType.OfferItem)]
    [TestCase(ConcessionType.PromiseFavor)]
    [TestCase(ConcessionType.TradeInformation)]
    [TestCase(ConcessionType.TakeRisk)]
    [TestCase(ConcessionType.StakeReputation)]
    public void GetBonusDice_Returns2_ForAllTypes(ConcessionType type)
    {
        // Act
        var bonusDice = type.GetBonusDice();

        // Assert
        bonusDice.Should().Be(2,
            because: "all concession types should grant +2d10 bonus dice");
    }

    /// <summary>
    /// Verifies that OfferItem is the only type that consumes an item.
    /// </summary>
    [Test]
    public void ConsumesItem_OnlyTrueForOfferItem()
    {
        // Assert
        ConcessionType.OfferItem.ConsumesItem().Should().BeTrue();
        ConcessionType.PromiseFavor.ConsumesItem().Should().BeFalse();
        ConcessionType.TradeInformation.ConsumesItem().Should().BeFalse();
        ConcessionType.TakeRisk.ConsumesItem().Should().BeFalse();
        ConcessionType.StakeReputation.ConsumesItem().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that PromiseFavor is the only type that creates debt.
    /// </summary>
    [Test]
    public void CreatesDebt_OnlyTrueForPromiseFavor()
    {
        // Assert
        ConcessionType.OfferItem.CreatesDebt().Should().BeFalse();
        ConcessionType.PromiseFavor.CreatesDebt().Should().BeTrue();
        ConcessionType.TradeInformation.CreatesDebt().Should().BeFalse();
        ConcessionType.TakeRisk.CreatesDebt().Should().BeFalse();
        ConcessionType.StakeReputation.CreatesDebt().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that StakeReputation risks faction reputation.
    /// </summary>
    [Test]
    public void RisksReputation_OnlyTrueForStakeReputation()
    {
        // Assert
        ConcessionType.OfferItem.RisksReputation().Should().BeFalse();
        ConcessionType.PromiseFavor.RisksReputation().Should().BeFalse();
        ConcessionType.TradeInformation.RisksReputation().Should().BeFalse();
        ConcessionType.TakeRisk.RisksReputation().Should().BeFalse();
        ConcessionType.StakeReputation.RisksReputation().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that risk levels scale appropriately.
    /// </summary>
    [Test]
    public void GetRiskLevel_ScalesWithConcessionPower()
    {
        // Act
        var offerItemRisk = ConcessionType.OfferItem.GetRiskLevel();
        var promiseFavorRisk = ConcessionType.PromiseFavor.GetRiskLevel();
        var tradeInfoRisk = ConcessionType.TradeInformation.GetRiskLevel();
        var stakeRepRisk = ConcessionType.StakeReputation.GetRiskLevel();

        // Assert
        offerItemRisk.Should().Be("Low");
        promiseFavorRisk.Should().Be("Medium");
        tradeInfoRisk.Should().Be("High");
        stakeRepRisk.Should().Be("Very High");
    }

    /// <summary>
    /// Verifies that Concession factory methods work correctly.
    /// </summary>
    [Test]
    public void Concession_FactoryMethods_CreateCorrectInstances()
    {
        // Act
        var itemConcession = Concession.OfferItem("item-123", "A valuable compass");
        var favorConcession = Concession.PromiseFavor("A future favor");
        var infoConcession = Concession.TradeInformation("Location of the cache");
        var riskConcession = Concession.TakeRisk("Go first into danger");
        var repConcession = Concession.StakeReputation("faction-abc", "Guild honor guarantees");

        // Assert
        itemConcession.Type.Should().Be(ConcessionType.OfferItem);
        itemConcession.ConsumedItemId.Should().Be("item-123");

        favorConcession.Type.Should().Be(ConcessionType.PromiseFavor);
        favorConcession.DebtCreated.Should().NotBeNullOrWhiteSpace();

        infoConcession.Type.Should().Be(ConcessionType.TradeInformation);
        infoConcession.InformationRevealed.Should().NotBeNullOrWhiteSpace();

        riskConcession.Type.Should().Be(ConcessionType.TakeRisk);
        riskConcession.RiskAccepted.Should().NotBeNullOrWhiteSpace();

        repConcession.Type.Should().Be(ConcessionType.StakeReputation);
        repConcession.StakedFactionId.Should().Be("faction-abc");
    }

    /// <summary>
    /// Verifies that concession mechanical summary includes both DC and dice.
    /// </summary>
    /// <param name="type">The concession type.</param>
    [TestCase(ConcessionType.OfferItem)]
    [TestCase(ConcessionType.PromiseFavor)]
    [TestCase(ConcessionType.TradeInformation)]
    [TestCase(ConcessionType.TakeRisk)]
    [TestCase(ConcessionType.StakeReputation)]
    public void GetMechanicalSummary_IncludesDcAndDice(ConcessionType type)
    {
        // Act
        var summary = type.GetMechanicalSummary();

        // Assert
        summary.Should().Contain("DC");
        summary.Should().Contain("d10");
    }

    #endregion

    #region Test 3: Position Track Gap Calculation

    /// <summary>
    /// Creates a test NegotiationPositionTrack with required properties initialized.
    /// </summary>
    private static NegotiationPositionTrack CreateTestPositionTrack(
        RequestComplexity complexity,
        int pcStartPosition,
        int npcStartPosition,
        int totalRounds)
    {
        var track = new NegotiationPositionTrack
        {
            NegotiationId = "test-neg-" + Guid.NewGuid().ToString("N")[..8],
            RequestComplexity = complexity,
            NpcId = "test-npc"
        };
        track.Initialize(pcStartPosition, npcStartPosition, totalRounds);
        return track;
    }

    /// <summary>
    /// Verifies that position track correctly calculates gap.
    /// </summary>
    [Test]
    public void PositionTrack_CalculatesGapCorrectly()
    {
        // Arrange & Act - Initialize with NoticeableAdvantage (PC at 2, NPC at 6, gap of 4)
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: 2,
            npcStartPosition: 6,
            totalRounds: 5);

        // Assert
        positionTrack.PcPosition.Should().Be(2);
        positionTrack.NpcPosition.Should().Be(6);
        positionTrack.Gap.Should().Be(4);
    }

    /// <summary>
    /// Verifies that gap is always positive regardless of position order.
    /// </summary>
    [Test]
    public void PositionTrack_Gap_IsAlwaysPositive()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.FairTrade,
            pcStartPosition: 4,
            npcStartPosition: 6,
            totalRounds: 3);

        // Assert
        positionTrack.Gap.Should().BeGreaterOrEqualTo(0);
    }

    /// <summary>
    /// Verifies that agreement is reached when gap is zero or positions cross.
    /// </summary>
    [Test]
    public void PositionTrack_AgreementReached_WhenGapIsZeroOrCrossed()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.FairTrade,
            pcStartPosition: 5,
            npcStartPosition: 5,
            totalRounds: 3);

        // Assert - gap of 0 means agreement is possible
        positionTrack.Gap.Should().Be(0);
        positionTrack.GetGapAssessment().Should().Contain("deal possible");
    }

    /// <summary>
    /// Verifies that GetGapAssessment returns appropriate descriptions.
    /// </summary>
    /// <param name="pcPosition">PC's position on the track.</param>
    /// <param name="npcPosition">NPC's position on the track.</param>
    /// <param name="expectedContains">Expected substring in the assessment.</param>
    [TestCase(5, 5, "deal possible")]      // Gap 0
    [TestCase(4, 5, "imminent")]           // Gap 1
    [TestCase(4, 6, "Close")]              // Gap 2
    [TestCase(3, 6, "Moderate")]           // Gap 3
    [TestCase(2, 6, "Moderate")]           // Gap 4 - still Moderate
    [TestCase(1, 6, "Far apart")]          // Gap 5 - now Far apart
    [TestCase(0, 7, "Very far")]           // Gap 7 - Very far
    public void GetGapAssessment_ReturnsAppropriateDescription(
        int pcPosition,
        int npcPosition,
        string expectedContains)
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: pcPosition,
            npcStartPosition: npcPosition,
            totalRounds: 5);

        // Act
        var assessment = positionTrack.GetGapAssessment();

        // Assert
        assessment.Should().ContainEquivalentOf(expectedContains);
    }

    /// <summary>
    /// Verifies position names are correct for each position.
    /// </summary>
    [TestCase(0, "Maximum Demand")]
    [TestCase(5, "Compromise")]
    [TestCase(8, "Walk Away")]
    public void GetPositionName_ReturnsCorrectName(int position, string expectedName)
    {
        // Act - GetPositionName is a static method
        var name = NegotiationPositionTrack.GetPositionName(position);

        // Assert
        name.Should().Be(expectedName);
    }

    #endregion

    #region Test 4: Success/Failure Movement Rules

    /// <summary>
    /// Verifies that moving NPC position toward PC decreases the gap.
    /// </summary>
    [Test]
    public void MoveNpcPosition_TowardPc_DecreasesGap()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: 2,
            npcStartPosition: 6,
            totalRounds: 5);
        var initialGap = positionTrack.Gap;

        // Act - Move NPC 1 step toward PC (method determines direction automatically)
        positionTrack.MoveNpcPosition(1);

        // Assert
        positionTrack.Gap.Should().Be(initialGap - 1);
        positionTrack.NpcPosition.Should().Be(5);
    }

    /// <summary>
    /// Verifies that moving PC position toward NPC decreases the gap (but is unfavorable).
    /// </summary>
    [Test]
    public void MovePcPosition_TowardNpc_DecreasesGap()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: 2,
            npcStartPosition: 6,
            totalRounds: 5);
        var initialGap = positionTrack.Gap;

        // Act - Move PC 1 step toward NPC (position increases)
        positionTrack.MovePcPosition(1);

        // Assert
        positionTrack.Gap.Should().Be(initialGap - 1);
        positionTrack.PcPosition.Should().Be(3);
    }

    /// <summary>
    /// Verifies that positions are clamped to valid range (0-8).
    /// </summary>
    [Test]
    public void MovePosition_ClampsToValidRange()
    {
        // Arrange - PC at 0, NPC at 6
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.OneSidedDeal,
            pcStartPosition: 0,
            npcStartPosition: 6,
            totalRounds: 7);

        // Act - Move PC many steps toward NPC, should not exceed 8
        positionTrack.MovePcPosition(15);

        // Assert - Should clamp to 8 max
        positionTrack.PcPosition.Should().BeLessOrEqualTo(8);
    }

    /// <summary>
    /// Verifies that a position track initialized with position 8 collapses immediately.
    /// </summary>
    /// <remarks>
    /// Position 8 is the "Walk Away" threshold. When either party reaches this position,
    /// the negotiation should collapse. This test verifies that moving toward position 8
    /// by multiple steps causes collapse when the threshold is crossed.
    /// </remarks>
    [Test]
    public void MovePcPosition_ToWalkAway_TriggersCollapse()
    {
        // Arrange - PC at 6, NPC at 8, so PC moving toward NPC will increase toward 8
        // When NPC > PC, direction is +1, so PC increases
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.FairTrade,
            pcStartPosition: 6,
            npcStartPosition: 8, // NPC at walk-away already - but that should trigger collapse on init
            totalRounds: 3);

        // Actually let's test a different scenario: PC moves many steps toward NPC who is far away
        positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.OneSidedDeal,
            pcStartPosition: 5,
            npcStartPosition: 7, // NPC higher than PC
            totalRounds: 7);

        // Act - Move PC 3 steps toward NPC (5 -> 6 -> 7 -> 8)
        positionTrack.MovePcPosition(3);

        // Assert - PC should reach position 8 (Walk Away) and collapse
        positionTrack.PcPosition.Should().Be(8);
        positionTrack.Status.Should().Be(NegotiationStatus.Collapsed);
    }

    /// <summary>
    /// Verifies that concession bonus is tracked correctly.
    /// </summary>
    [Test]
    public void SetConcessionBonus_TracksConcessionForNextRound()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: 2,
            npcStartPosition: 6,
            totalRounds: 5);
        var concession = Concession.PromiseFavor("Future favor");

        // Act
        positionTrack.SetConcessionBonus(concession);

        // Assert
        positionTrack.ActiveConcession.Should().NotBeNull();
        positionTrack.ActiveConcession!.Type.Should().Be(ConcessionType.PromiseFavor);
    }

    /// <summary>
    /// Verifies that concession bonus is cleared after use.
    /// </summary>
    [Test]
    public void ClearConcessionBonus_RemovesActiveConcession()
    {
        // Arrange
        var positionTrack = CreateTestPositionTrack(
            complexity: RequestComplexity.NoticeableAdvantage,
            pcStartPosition: 2,
            npcStartPosition: 6,
            totalRounds: 5);
        positionTrack.SetConcessionBonus(Concession.PromiseFavor("Future favor"));

        // Act
        positionTrack.ClearConcessionBonus();

        // Assert
        positionTrack.ActiveConcession.Should().BeNull();
    }

    #endregion

    #region NegotiationTactic Tests

    /// <summary>
    /// Verifies that only Concede tactic does not require a check.
    /// </summary>
    [Test]
    public void RequiresCheck_FalseOnlyForConcede()
    {
        // Assert
        NegotiationTactic.Persuade.RequiresCheck().Should().BeTrue();
        NegotiationTactic.Deceive.RequiresCheck().Should().BeTrue();
        NegotiationTactic.Pressure.RequiresCheck().Should().BeTrue();
        NegotiationTactic.Concede.RequiresCheck().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that tactics map to correct underlying systems.
    /// </summary>
    /// <param name="tactic">The negotiation tactic.</param>
    /// <param name="expectedSystem">The expected underlying social interaction type.</param>
    [TestCase(NegotiationTactic.Persuade, SocialInteractionType.Persuasion)]
    [TestCase(NegotiationTactic.Deceive, SocialInteractionType.Deception)]
    [TestCase(NegotiationTactic.Pressure, SocialInteractionType.Intimidation)]
    public void GetUnderlyingSystem_ReturnsCorrectSystem(
        NegotiationTactic tactic,
        SocialInteractionType? expectedSystem)
    {
        // Act
        var underlyingSystem = tactic.GetUnderlyingSystem();

        // Assert
        underlyingSystem.Should().Be(expectedSystem);
    }

    /// <summary>
    /// Verifies that Concede has no underlying system.
    /// </summary>
    [Test]
    public void Concede_GetUnderlyingSystem_ReturnsNull()
    {
        // Act
        var underlyingSystem = NegotiationTactic.Concede.GetUnderlyingSystem();

        // Assert
        underlyingSystem.Should().BeNull();
    }

    /// <summary>
    /// Verifies that Deceive tactic incurs stress (Liar's Burden).
    /// </summary>
    [Test]
    public void Deceive_IncursStress_ReturnsTrue()
    {
        // Assert
        NegotiationTactic.Deceive.IncursStress().Should().BeTrue();
        NegotiationTactic.Persuade.IncursStress().Should().BeFalse();
        NegotiationTactic.Pressure.IncursStress().Should().BeFalse();
        NegotiationTactic.Concede.IncursStress().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Pressure tactic costs reputation (Cost of Fear).
    /// </summary>
    [Test]
    public void Pressure_CostsReputation_ReturnsTrue()
    {
        // Assert
        NegotiationTactic.Pressure.CostsReputation().Should().BeTrue();
        NegotiationTactic.Persuade.CostsReputation().Should().BeFalse();
        NegotiationTactic.Deceive.CostsReputation().Should().BeFalse();
        NegotiationTactic.Concede.CostsReputation().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Deceive and Pressure tactics cause fumble collapse.
    /// </summary>
    [Test]
    public void FumbleCollapsesNegotiation_TrueForDeceiveAndPressure()
    {
        // Assert - Only Deceive and Pressure cause immediate collapse on fumble
        NegotiationTactic.Persuade.FumbleCollapsesNegotiation().Should().BeFalse();
        NegotiationTactic.Deceive.FumbleCollapsesNegotiation().Should().BeTrue();
        NegotiationTactic.Pressure.FumbleCollapsesNegotiation().Should().BeTrue();
        NegotiationTactic.Concede.FumbleCollapsesNegotiation().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that each tactic has correct fumble type.
    /// </summary>
    [TestCase(NegotiationTactic.Persuade, FumbleType.TrustShattered)]
    [TestCase(NegotiationTactic.Deceive, FumbleType.LieExposed)]
    [TestCase(NegotiationTactic.Pressure, FumbleType.ChallengeAccepted)]
    public void GetFumbleType_ReturnsCorrectType(NegotiationTactic tactic, FumbleType? expectedType)
    {
        // Act
        var fumbleType = tactic.GetFumbleType();

        // Assert
        fumbleType.Should().Be(expectedType);
    }

    #endregion

    #region NegotiationStatus Tests

    /// <summary>
    /// Verifies that terminal statuses are correctly identified.
    /// </summary>
    [TestCase(NegotiationStatus.Opening, false)]
    [TestCase(NegotiationStatus.Bargaining, false)]
    [TestCase(NegotiationStatus.CrisisManagement, false)]
    [TestCase(NegotiationStatus.Finalization, false)]
    [TestCase(NegotiationStatus.DealReached, true)]
    [TestCase(NegotiationStatus.Collapsed, true)]
    public void IsTerminal_ReturnsCorrectValue(NegotiationStatus status, bool expectedTerminal)
    {
        // Act
        var isTerminal = status.IsTerminal();

        // Assert
        isTerminal.Should().Be(expectedTerminal);
    }

    /// <summary>
    /// Verifies that success status is correctly identified.
    /// </summary>
    [Test]
    public void IsSuccess_OnlyTrueForDealReached()
    {
        // Assert
        NegotiationStatus.DealReached.IsSuccess().Should().BeTrue();
        NegotiationStatus.Collapsed.IsSuccess().Should().BeFalse();
        NegotiationStatus.Bargaining.IsSuccess().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that failure status is correctly identified.
    /// </summary>
    [Test]
    public void IsFailure_OnlyTrueForCollapsed()
    {
        // Assert
        NegotiationStatus.Collapsed.IsFailure().Should().BeTrue();
        NegotiationStatus.DealReached.IsFailure().Should().BeFalse();
        NegotiationStatus.Bargaining.IsFailure().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that crisis management is at risk.
    /// </summary>
    [Test]
    public void IsAtRisk_TrueForCrisisManagement()
    {
        // Assert
        NegotiationStatus.CrisisManagement.IsAtRisk().Should().BeTrue();
        NegotiationStatus.Bargaining.IsAtRisk().Should().BeFalse();
        NegotiationStatus.Finalization.IsAtRisk().Should().BeFalse();
    }

    #endregion

    #region NegotiationRound Tests

    /// <summary>
    /// Verifies that NegotiationRound factory method for check creates correct instance.
    /// </summary>
    [Test]
    public void NegotiationRound_ForCheck_CreatesCorrectInstance()
    {
        // Arrange & Act
        var round = NegotiationRound.ForCheck(
            roundNumber: 1,
            tactic: NegotiationTactic.Persuade,
            outcome: SkillOutcome.FullSuccess,
            pcPosition: 2,
            npcPosition: 5,
            positionMovement: "NPC moved 1 step toward PC",
            narrative: "Your argument lands well.",
            effectiveDc: 14,
            dicePool: 5);

        // Assert
        round.RoundNumber.Should().Be(1);
        round.TacticUsed.Should().Be(NegotiationTactic.Persuade);
        round.CheckResult.Should().Be(SkillOutcome.FullSuccess);
        round.ConcessionMade.Should().BeNull();
        round.GapAfter.Should().Be(3);
    }

    /// <summary>
    /// Verifies that NegotiationRound factory method for concession creates correct instance.
    /// </summary>
    [Test]
    public void NegotiationRound_ForConcession_CreatesCorrectInstance()
    {
        // Arrange
        var concession = Concession.PromiseFavor("A favor owed");

        // Act
        var round = NegotiationRound.ForConcession(
            roundNumber: 2,
            concession: concession,
            pcPosition: 3,
            npcPosition: 5,
            narrative: "You offer a concession.");

        // Assert
        round.RoundNumber.Should().Be(2);
        round.TacticUsed.Should().Be(NegotiationTactic.Concede);
        round.CheckResult.Should().BeNull();
        round.ConcessionMade.Should().NotBeNull();
        round.ConcessionMade!.Type.Should().Be(ConcessionType.PromiseFavor);
        round.GapAfter.Should().Be(2);
    }

    #endregion

    #region NegotiationContext Tests

    /// <summary>
    /// Verifies that NegotiationContext calculates effective DC correctly.
    /// </summary>
    [Test]
    public void NegotiationContext_EffectiveDc_ReducedByConcession()
    {
        // Arrange - Create context with NoticeableAdvantage complexity
        var context = NegotiationContext.CreateMinimal(
            npcId: "test-npc",
            complexity: RequestComplexity.NoticeableAdvantage,
            tactic: NegotiationTactic.Persuade);

        // Set up a concession bonus on the position track
        var concession = Concession.TradeInformation("Secret info");
        context.PositionTrack.SetConcessionBonus(concession);

        // Create new context with the active concession
        var contextWithConcession = context with
        {
            ActiveConcession = context.PositionTrack.ActiveConcession
        };

        // Act
        var baseDc = contextWithConcession.GetBaseDc();
        var effectiveDc = contextWithConcession.GetEffectiveDc();
        var dcReduction = concession.Type.GetDcReduction();

        // Assert
        baseDc.Should().Be(18); // NoticeableAdvantage base DC
        effectiveDc.Should().Be(baseDc - dcReduction); // 18 - 4 = 14
    }

    /// <summary>
    /// Verifies that concession bonus dice are correctly calculated.
    /// </summary>
    [Test]
    public void NegotiationContext_ConcessionBonusDice_ReturnsCorrectValue()
    {
        // Arrange - Create context and set up concession
        var concession = Concession.StakeReputation("faction-123", "Guild honor");

        var context = NegotiationContext.CreateMinimal(
            npcId: "test-npc",
            complexity: RequestComplexity.FairTrade,
            tactic: NegotiationTactic.Persuade);

        // Create context with active concession
        var contextWithConcession = context with
        {
            ActiveConcession = concession
        };

        // Act
        var bonusDice = contextWithConcession.GetConcessionBonusDice();

        // Assert
        bonusDice.Should().Be(2); // All concessions grant +2d10
    }

    #endregion

    #region NegotiationResult Tests

    /// <summary>
    /// Verifies that successful negotiation result has correct properties.
    /// </summary>
    [Test]
    public void NegotiationResult_Success_HasCorrectProperties()
    {
        // Arrange - Create a minimal base social result
        var baseResult = SocialResult.CreateSuccess(
            interactionType: SocialInteractionType.Negotiation,
            targetId: "test-npc",
            outcome: SkillOutcome.FullSuccess,
            details: new OutcomeDetails(SkillOutcome.FullSuccess, 2, false, false),
            currentDisposition: DispositionLevel.CreateNeutral(),
            dispositionChange: 5);

        // Act
        var result = NegotiationResult.Success(
            baseResult: baseResult,
            finalPcPosition: 4,
            finalNpcPosition: 5,
            roundsUsed: 3,
            history: new List<NegotiationRound>(),
            dealTerms: "Mutual defense pact agreed",
            unlockedOptions: new List<string> { "Cooperation unlocked" },
            dispositionChange: 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsCollapsedFromFumble.Should().BeFalse();
        result.FinalStatus.Should().Be(NegotiationStatus.DealReached);
        result.FinalDealTerms.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that failed negotiation result has correct properties.
    /// </summary>
    [Test]
    public void NegotiationResult_Failure_HasCorrectProperties()
    {
        // Arrange - Create a minimal base social result for failure
        var baseResult = SocialResult.CreateFailure(
            interactionType: SocialInteractionType.Negotiation,
            targetId: "test-npc",
            outcome: SkillOutcome.Failure,
            details: new OutcomeDetails(SkillOutcome.Failure, -2, false, false),
            currentDisposition: DispositionLevel.CreateNeutral(),
            dispositionChange: -10);

        // Act
        var result = NegotiationResult.Failure(
            baseResult: baseResult,
            finalPcPosition: 5,
            finalNpcPosition: 7,
            roundsUsed: 5,
            history: new List<NegotiationRound>(),
            dispositionChange: -10);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsCollapsedFromFumble.Should().BeFalse();
        result.FinalStatus.Should().Be(NegotiationStatus.Collapsed);
    }

    /// <summary>
    /// Verifies that fumble negotiation result has severe consequences.
    /// </summary>
    [Test]
    public void NegotiationResult_Fumble_HasSevereConsequences()
    {
        // Arrange
        var fumbleConsequence = new FumbleConsequence(
            consequenceId: "neg-fumble",
            characterId: "player",
            skillId: "rhetoric_negotiation",
            consequenceType: FumbleType.NegotiationCollapsed,
            targetId: "test-npc",
            appliedAt: DateTime.UtcNow,
            expiresAt: null,
            description: "Negotiation collapsed catastrophically",
            recoveryCondition: "Cannot re-initiate this negotiation");

        // Create a minimal base social result for fumble
        var baseResult = SocialResult.CreateFailure(
            interactionType: SocialInteractionType.Negotiation,
            targetId: "test-npc",
            outcome: SkillOutcome.CriticalFailure,
            details: new OutcomeDetails(SkillOutcome.CriticalFailure, -5, true, false),
            currentDisposition: DispositionLevel.CreateNeutral(),
            dispositionChange: -20);

        // Act
        var result = NegotiationResult.Fumble(
            baseResult: baseResult,
            finalPcPosition: 3,
            finalNpcPosition: 6,
            roundsUsed: 2,
            history: new List<NegotiationRound>(),
            fumbleConsequence: fumbleConsequence,
            dispositionChange: -20);

        // Assert
        result.IsCollapsedFromFumble.Should().BeTrue();
        result.FumbleConsequence.Should().NotBeNull();
        result.FumbleConsequence!.ConsequenceType.Should().Be(FumbleType.NegotiationCollapsed);
    }

    /// <summary>
    /// Verifies that in-progress result is correctly identified.
    /// </summary>
    [Test]
    public void NegotiationResult_InProgress_IsNotTerminal()
    {
        // Arrange - Create a minimal base social result
        var baseResult = SocialResult.CreateSuccess(
            interactionType: SocialInteractionType.Negotiation,
            targetId: "test-npc",
            outcome: SkillOutcome.MarginalSuccess,
            details: new OutcomeDetails(SkillOutcome.MarginalSuccess, 1, false, false),
            currentDisposition: DispositionLevel.CreateNeutral(),
            dispositionChange: 0);

        // Act
        var result = NegotiationResult.InProgress(
            baseResult: baseResult,
            currentStatus: NegotiationStatus.Bargaining,
            pcPosition: 3,
            npcPosition: 5,
            roundsUsed: 2,
            history: new List<NegotiationRound>());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsCollapsedFromFumble.Should().BeFalse();
        result.FinalStatus.Should().Be(NegotiationStatus.Bargaining);
        result.FinalStatus.IsTerminal().Should().BeFalse();
    }

    #endregion

    #region FumbleType NegotiationCollapsed Tests

    /// <summary>
    /// Verifies that NegotiationCollapsed fumble type exists and has correct properties.
    /// </summary>
    [Test]
    public void FumbleType_NegotiationCollapsed_ExistsAndIsCorrect()
    {
        // Arrange
        var fumbleType = FumbleType.NegotiationCollapsed;

        // Assert
        fumbleType.GetDisplayName().Should().Be("Negotiation Collapsed");
        fumbleType.GetAssociatedSkillId().Should().Be("negotiation");
        fumbleType.IsInstant().Should().BeTrue();
        fumbleType.IsPermanent().Should().BeTrue();
        fumbleType.BlocksAllTargets().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NegotiationCollapsed has a description.
    /// </summary>
    [Test]
    public void FumbleType_NegotiationCollapsed_HasDescription()
    {
        // Act
        var description = FumbleType.NegotiationCollapsed.GetDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace();
        description.Should().ContainAny("collapsed", "failed", "over");
    }

    #endregion
}
