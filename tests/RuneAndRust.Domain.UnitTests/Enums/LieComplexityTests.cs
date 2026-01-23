// ------------------------------------------------------------------------------
// <copyright file="LieComplexityTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for LieComplexity enum and deception mechanics.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.UnitTests.Enums;

using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Unit tests for <see cref="LieComplexity"/> enum and deception mechanics.
/// </summary>
[TestFixture]
public class LieComplexityTests
{
    #region GetBaseDc Tests

    /// <summary>
    /// Verifies that each lie complexity tier returns the correct base DC.
    /// </summary>
    /// <param name="complexity">The lie complexity tier.</param>
    /// <param name="expectedDc">The expected DC value.</param>
    [TestCase(LieComplexity.WhiteLie, 10)]
    [TestCase(LieComplexity.Plausible, 14)]
    [TestCase(LieComplexity.Unlikely, 18)]
    [TestCase(LieComplexity.Outrageous, 22)]
    public void GetBaseDc_ReturnsCorrectDc(LieComplexity complexity, int expectedDc)
    {
        // Act
        var baseDc = complexity.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc, because: $"{complexity} should have DC {expectedDc}");
    }

    #endregion

    #region Suspicious NPC Modifier Tests

    /// <summary>
    /// Verifies that a suspicious NPC adds +4 to the effective DC.
    /// </summary>
    [Test]
    public void DeceptionContext_SuspiciousNpc_AddsFourToDc()
    {
        // Arrange
        var context = new DeceptionContext(
            LieComplexity: LieComplexity.Plausible,
            TargetId: "npc_guard",
            TargetWits: 3,
            TargetDisposition: DispositionLevel.Create(0),
            NpcSuspicious: true,
            NpcTrusting: false,
            NpcTrainedObserver: false,
            NpcPreviouslyFooled: false,
            NpcHasAlert: false,
            NpcHasFatigued: false,
            NpcKnowsPlayer: false,
            EvidenceContradicts: false,
            ContradictingEvidenceDescription: null,
            CoverStoryQuality: CoverStoryQuality.None,
            HasForgedDocuments: false,
            ForgedDocumentQuality: 0,
            LieContainsTruth: false,
            NpcIsDistracted: false,
            NpcIsIntoxicated: false,
            PlayerHasUntrustworthy: false,
            PlayerFactionStanding: 0);

        // Act
        var effectiveDc = context.EffectiveDc;

        // Assert
        effectiveDc.Should().Be(18, because: "Base DC 14 + 4 (Suspicious) = 18");
    }

    /// <summary>
    /// Verifies that a trusting NPC reduces the effective DC by 4.
    /// </summary>
    [Test]
    public void DeceptionContext_TrustingNpc_SubtractsFourFromDc()
    {
        // Arrange
        var context = new DeceptionContext(
            LieComplexity: LieComplexity.Plausible,
            TargetId: "npc_merchant",
            TargetWits: 3,
            TargetDisposition: DispositionLevel.Create(50),
            NpcSuspicious: false,
            NpcTrusting: true,
            NpcTrainedObserver: false,
            NpcPreviouslyFooled: false,
            NpcHasAlert: false,
            NpcHasFatigued: false,
            NpcKnowsPlayer: false,
            EvidenceContradicts: false,
            ContradictingEvidenceDescription: null,
            CoverStoryQuality: CoverStoryQuality.None,
            HasForgedDocuments: false,
            ForgedDocumentQuality: 0,
            LieContainsTruth: false,
            NpcIsDistracted: false,
            NpcIsIntoxicated: false,
            PlayerHasUntrustworthy: false,
            PlayerFactionStanding: 0);

        // Act
        var effectiveDc = context.EffectiveDc;

        // Assert
        effectiveDc.Should().Be(10, because: "Base DC 14 - 4 (Trusting) = 10");
    }

    #endregion

    #region Liar's Burden Stress Tests

    /// <summary>
    /// Verifies that Liar's Burden calculates correct stress for each outcome.
    /// </summary>
    /// <param name="outcome">The skill outcome.</param>
    /// <param name="isFumble">Whether it was a fumble.</param>
    /// <param name="expectedStress">The expected stress cost.</param>
    [TestCase(SkillOutcome.FullSuccess, false, 1)]
    [TestCase(SkillOutcome.MarginalSuccess, false, 1)]
    [TestCase(SkillOutcome.Failure, false, 3)]
    [TestCase(SkillOutcome.CriticalFailure, true, 13)]
    public void LiarsBurden_CalculatesCorrectStress(
        SkillOutcome outcome,
        bool isFumble,
        int expectedStress)
    {
        // Arrange & Act
        var burden = new LiarsBurden(outcome, isFumble);

        // Assert
        burden.TotalStressCost.Should().Be(expectedStress);
    }

    /// <summary>
    /// Verifies that fumble includes [Lie Exposed] additional stress.
    /// </summary>
    [Test]
    public void LiarsBurden_Fumble_IncludesLieExposedStress()
    {
        // Arrange
        var burden = LiarsBurden.ForFumble();

        // Assert
        burden.BaseStressCost.Should().Be(8, because: "Fumble base cost is 8");
        burden.LieExposedStress.Should().Be(5, because: "[Lie Exposed] adds 5 stress");
        burden.TotalStressCost.Should().Be(13, because: "8 + 5 = 13");
        burden.HasLieExposedConsequence.Should().BeTrue();
    }

    #endregion

    #region Cover Story Modifier Tests

    /// <summary>
    /// Verifies that cover story quality reduces DC appropriately.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <param name="expectedModifier">The expected DC modifier.</param>
    [TestCase(CoverStoryQuality.None, 0)]
    [TestCase(CoverStoryQuality.Basic, -1)]
    [TestCase(CoverStoryQuality.Good, -2)]
    [TestCase(CoverStoryQuality.Excellent, -3)]
    [TestCase(CoverStoryQuality.Masterwork, -4)]
    public void CoverStoryQuality_GetDcModifier_ReturnsCorrectValue(
        CoverStoryQuality quality,
        int expectedModifier)
    {
        // Act
        var modifier = quality.GetDcModifier();

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    #endregion

    #region Detection Severity Tests

    /// <summary>
    /// Verifies that detection severity scales with lie complexity.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <param name="expectedSeverity">The expected severity (1-4).</param>
    [TestCase(LieComplexity.WhiteLie, 1)]
    [TestCase(LieComplexity.Plausible, 2)]
    [TestCase(LieComplexity.Unlikely, 3)]
    [TestCase(LieComplexity.Outrageous, 4)]
    public void LieComplexity_GetDetectionSeverity_ReturnsCorrectValue(
        LieComplexity complexity,
        int expectedSeverity)
    {
        // Act
        var severity = complexity.GetDetectionSeverity();

        // Assert
        severity.Should().Be(expectedSeverity);
    }

    #endregion

    #region Combat Chance Modifier Tests

    /// <summary>
    /// Verifies that combat chance modifier scales with lie complexity.
    /// </summary>
    /// <param name="complexity">The lie complexity.</param>
    /// <param name="expectedModifier">The expected combat chance modifier.</param>
    [TestCase(LieComplexity.WhiteLie, -10)]
    [TestCase(LieComplexity.Plausible, 0)]
    [TestCase(LieComplexity.Unlikely, 10)]
    [TestCase(LieComplexity.Outrageous, 20)]
    public void LieComplexity_GetCombatChanceModifier_ReturnsCorrectValue(
        LieComplexity complexity,
        int expectedModifier)
    {
        // Act
        var modifier = complexity.GetCombatChanceModifier();

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    #endregion
}
