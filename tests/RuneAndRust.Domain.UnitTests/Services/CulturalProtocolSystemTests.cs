// ------------------------------------------------------------------------------
// <copyright file="CulturalProtocolSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Cultural Protocol System including Cant fluency mechanics,
// Veil-Speech states, protocol violations, and culture-specific protocols.
// Part of v0.15.3g Cultural Protocols implementation.
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
/// Unit tests for the Cultural Protocol System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the required acceptance criteria from the design specification:
/// </para>
/// <list type="number">
///   <item>
///     <description>Cant fluency correctly applies dice modifiers (None=-1, Basic=0, Fluent=+1)</description>
///   </item>
///   <item>
///     <description>Veil-Speech states apply correct DC and dice modifiers</description>
///   </item>
///   <item>
///     <description>Protocol violations have escalating consequences (DC +2/+4/+6, blocks)</description>
///   </item>
///   <item>
///     <description>Each culture has correct base DC and special protocols</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class CulturalProtocolSystemTests
{
    #region Test 1: CantFluency Dice Modifier Tests

    /// <summary>
    /// Verifies that each fluency level returns the correct dice modifier.
    /// This is the core mechanic for cant-based social interactions.
    /// </summary>
    /// <param name="fluency">The character's cant fluency level.</param>
    /// <param name="expectedModifier">The expected dice pool modifier.</param>
    /// <remarks>
    /// <para>
    /// None: -1d10 (unfamiliar with the cultural dialect)
    /// Basic: +0 (can communicate but lacks nuance)
    /// Fluent: +1d10 (native-level proficiency)
    /// </para>
    /// </remarks>
    [TestCase(CantFluency.None, -1)]
    [TestCase(CantFluency.Basic, 0)]
    [TestCase(CantFluency.Fluent, 1)]
    public void CantFluency_GetDiceModifier_ReturnsCorrectValue(
        CantFluency fluency,
        int expectedModifier)
    {
        // Act
        var modifier = fluency.GetDiceModifier();

        // Assert
        modifier.Should().Be(expectedModifier,
            because: $"{fluency} fluency should provide {expectedModifier:+0;-0;0} dice modifier");
    }

    /// <summary>
    /// Verifies that fluency levels correctly determine understanding capability.
    /// Characters with None fluency cannot understand the cant.
    /// </summary>
    /// <param name="fluency">The character's cant fluency level.</param>
    /// <param name="expectedCanUnderstand">Whether the character can understand.</param>
    [TestCase(CantFluency.None, false)]
    [TestCase(CantFluency.Basic, true)]
    [TestCase(CantFluency.Fluent, true)]
    public void CantFluency_CanUnderstand_ReturnsCorrectValue(
        CantFluency fluency,
        bool expectedCanUnderstand)
    {
        // Act
        var canUnderstand = fluency.CanUnderstand();

        // Assert
        canUnderstand.Should().Be(expectedCanUnderstand,
            because: $"{fluency} fluency should {(expectedCanUnderstand ? "" : "not ")}allow understanding");
    }

    /// <summary>
    /// Verifies that fluency levels correctly determine speaking capability.
    /// Characters with None fluency cannot speak the cant.
    /// </summary>
    /// <param name="fluency">The character's cant fluency level.</param>
    /// <param name="expectedCanSpeak">Whether the character can speak.</param>
    [TestCase(CantFluency.None, false)]
    [TestCase(CantFluency.Basic, true)]
    [TestCase(CantFluency.Fluent, true)]
    public void CantFluency_CanSpeak_ReturnsCorrectValue(
        CantFluency fluency,
        bool expectedCanSpeak)
    {
        // Act
        var canSpeak = fluency.CanSpeak();

        // Assert
        canSpeak.Should().Be(expectedCanSpeak,
            because: $"{fluency} fluency should {(expectedCanSpeak ? "" : "not ")}allow speaking");
    }

    /// <summary>
    /// Verifies that only Fluent grants access to cultural dialogue options.
    /// These are special conversation paths requiring deep cultural knowledge.
    /// </summary>
    /// <param name="fluency">The character's cant fluency level.</param>
    /// <param name="expectedAccess">Whether cultural dialogue is accessible.</param>
    [TestCase(CantFluency.None, false)]
    [TestCase(CantFluency.Basic, false)]
    [TestCase(CantFluency.Fluent, true)]
    public void CantFluency_GrantsCulturalDialogueAccess_OnlyForFluent(
        CantFluency fluency,
        bool expectedAccess)
    {
        // Act
        var hasAccess = fluency.GrantsCulturalDialogueAccess();

        // Assert
        hasAccess.Should().Be(expectedAccess,
            because: $"{fluency} fluency should {(expectedAccess ? "" : "not ")}grant cultural dialogue access");
    }

    /// <summary>
    /// Verifies that all fluency levels have display names defined.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    [TestCase(CantFluency.None)]
    [TestCase(CantFluency.Basic)]
    [TestCase(CantFluency.Fluent)]
    public void CantFluency_GetDisplayName_ReturnsNonEmpty(CantFluency fluency)
    {
        // Act
        var displayName = fluency.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{fluency} should have a display name defined");
    }

    /// <summary>
    /// Verifies that all fluency levels have descriptions with modifier info.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    [TestCase(CantFluency.None)]
    [TestCase(CantFluency.Basic)]
    [TestCase(CantFluency.Fluent)]
    public void CantFluency_GetDescription_ReturnsNonEmpty(CantFluency fluency)
    {
        // Act
        var description = fluency.GetDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{fluency} should have a description defined");
    }

    #endregion

    #region Test 2: VeilSpeechState Tests

    /// <summary>
    /// Verifies that each Veil-Speech state returns the correct dice modifier.
    /// This is specific to Utgard cultural interactions.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <param name="expectedModifier">The expected dice pool modifier.</param>
    /// <remarks>
    /// <para>
    /// Neutral: +0 (no modifier - standard interaction)
    /// Respected: +1d10 (proper Veil-Speech demonstrated)
    /// Offended: -2d10 (direct truth told - offensive)
    /// DeepOffense: -4d10 (repeated offense - may block interaction)
    /// </para>
    /// </remarks>
    [TestCase(VeilSpeechState.Neutral, 0)]
    [TestCase(VeilSpeechState.Respected, 1)]
    [TestCase(VeilSpeechState.Offended, -2)]
    [TestCase(VeilSpeechState.DeepOffense, -4)]
    public void VeilSpeechState_GetDiceModifier_ReturnsCorrectValue(
        VeilSpeechState state,
        int expectedModifier)
    {
        // Act
        var modifier = state.GetDiceModifier();

        // Assert
        modifier.Should().Be(expectedModifier,
            because: $"{state} should provide {expectedModifier:+0;-0;0} dice modifier");
    }

    /// <summary>
    /// Verifies that deception DC reduction is applied for non-deep-offense states.
    /// Deception is culturally respected by Utgard, making it easier (DC -4).
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <param name="expectedDcModifier">The expected DC modifier.</param>
    /// <remarks>
    /// <para>
    /// Neutral/Respected/Offended: -4 DC (deception is culturally respected)
    /// DeepOffense: +0 DC (no deception bonus when deeply offended)
    /// </para>
    /// </remarks>
    [TestCase(VeilSpeechState.Neutral, -4)]
    [TestCase(VeilSpeechState.Respected, -4)]
    [TestCase(VeilSpeechState.Offended, -4)]
    [TestCase(VeilSpeechState.DeepOffense, 0)]
    public void VeilSpeechState_GetDcModifier_ReturnsCorrectValue(
        VeilSpeechState state,
        int expectedDcModifier)
    {
        // Act
        var dcModifier = state.GetDcModifier();

        // Assert
        dcModifier.Should().Be(expectedDcModifier,
            because: $"{state} should provide {expectedDcModifier:+0;-0;0} DC modifier");
    }

    /// <summary>
    /// Verifies that Offended and DeepOffense are identified as offense states.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <param name="expectedIsOffended">Whether the state represents offense.</param>
    [TestCase(VeilSpeechState.Neutral, false)]
    [TestCase(VeilSpeechState.Respected, false)]
    [TestCase(VeilSpeechState.Offended, true)]
    [TestCase(VeilSpeechState.DeepOffense, true)]
    public void VeilSpeechState_IsOffended_ReturnsCorrectValue(
        VeilSpeechState state,
        bool expectedIsOffended)
    {
        // Act
        var isOffended = state.IsOffended();

        // Assert
        isOffended.Should().Be(expectedIsOffended,
            because: $"{state} should {(expectedIsOffended ? "" : "not ")}be an offense state");
    }

    /// <summary>
    /// Verifies that only Respected state is identified as respected.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <param name="expectedIsRespected">Whether the state represents being respected.</param>
    [TestCase(VeilSpeechState.Neutral, false)]
    [TestCase(VeilSpeechState.Respected, true)]
    [TestCase(VeilSpeechState.Offended, false)]
    [TestCase(VeilSpeechState.DeepOffense, false)]
    public void VeilSpeechState_IsRespected_ReturnsCorrectValue(
        VeilSpeechState state,
        bool expectedIsRespected)
    {
        // Act
        var isRespected = state.IsRespected();

        // Assert
        isRespected.Should().Be(expectedIsRespected,
            because: $"{state} should {(expectedIsRespected ? "" : "not ")}be a respected state");
    }

    /// <summary>
    /// Verifies that only DeepOffense blocks interaction.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    /// <param name="expectedAllowsInteraction">Whether interaction is allowed.</param>
    [TestCase(VeilSpeechState.Neutral, true)]
    [TestCase(VeilSpeechState.Respected, true)]
    [TestCase(VeilSpeechState.Offended, true)]
    [TestCase(VeilSpeechState.DeepOffense, false)]
    public void VeilSpeechState_AllowsInteraction_ReturnsCorrectValue(
        VeilSpeechState state,
        bool expectedAllowsInteraction)
    {
        // Act
        var allowsInteraction = state.AllowsInteraction();

        // Assert
        allowsInteraction.Should().Be(expectedAllowsInteraction,
            because: $"{state} should {(expectedAllowsInteraction ? "" : "not ")}allow interaction");
    }

    /// <summary>
    /// Verifies that all Veil-Speech states have display names defined.
    /// </summary>
    /// <param name="state">The Veil-Speech state.</param>
    [TestCase(VeilSpeechState.Neutral)]
    [TestCase(VeilSpeechState.Respected)]
    [TestCase(VeilSpeechState.Offended)]
    [TestCase(VeilSpeechState.DeepOffense)]
    public void VeilSpeechState_GetDisplayName_ReturnsNonEmpty(VeilSpeechState state)
    {
        // Act
        var displayName = state.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{state} should have a display name defined");
    }

    #endregion

    #region Test 3: ProtocolViolationType Tests

    /// <summary>
    /// Verifies that each violation type returns the correct DC increase.
    /// This affects all future social checks with the offended NPC.
    /// </summary>
    /// <param name="violation">The protocol violation type.</param>
    /// <param name="expectedDcIncrease">The expected DC increase.</param>
    /// <remarks>
    /// <para>
    /// None: +0 DC (no violation)
    /// Minor: +2 DC (small faux pas)
    /// Moderate: +4 DC (notable breach)
    /// Severe: +6 DC (serious offense)
    /// Unforgivable: Interaction blocked entirely
    /// </para>
    /// </remarks>
    [TestCase(ProtocolViolationType.None, 0)]
    [TestCase(ProtocolViolationType.Minor, 2)]
    [TestCase(ProtocolViolationType.Moderate, 4)]
    [TestCase(ProtocolViolationType.Severe, 6)]
    public void ProtocolViolationType_GetDcIncrease_ReturnsCorrectValue(
        ProtocolViolationType violation,
        int expectedDcIncrease)
    {
        // Act
        var dcIncrease = violation.GetDcIncrease();

        // Assert
        dcIncrease.Should().Be(expectedDcIncrease,
            because: $"{violation} violation should increase DC by {expectedDcIncrease}");
    }

    /// <summary>
    /// Verifies that Unforgivable violations return maximum DC (interaction blocked).
    /// </summary>
    [Test]
    public void ProtocolViolationType_Unforgivable_HasMaxDcIncrease()
    {
        // Act
        var dcIncrease = ProtocolViolationType.Unforgivable.GetDcIncrease();

        // Assert
        dcIncrease.Should().Be(int.MaxValue,
            because: "Unforgivable violations block interaction entirely");
    }

    /// <summary>
    /// Verifies that each violation type returns the correct dice penalty.
    /// More severe violations remove more dice from the pool.
    /// </summary>
    /// <param name="violation">The protocol violation type.</param>
    /// <param name="expectedPenalty">The expected dice penalty (positive value).</param>
    [TestCase(ProtocolViolationType.None, 0)]
    [TestCase(ProtocolViolationType.Minor, 0)]
    [TestCase(ProtocolViolationType.Moderate, 1)]
    [TestCase(ProtocolViolationType.Severe, 2)]
    public void ProtocolViolationType_GetDicePenalty_ReturnsCorrectValue(
        ProtocolViolationType violation,
        int expectedPenalty)
    {
        // Act
        var penalty = violation.GetDicePenalty();

        // Assert
        penalty.Should().Be(expectedPenalty,
            because: $"{violation} violation should remove {expectedPenalty} dice from pool");
    }

    /// <summary>
    /// Verifies that only Unforgivable violations block interaction.
    /// </summary>
    /// <param name="violation">The protocol violation type.</param>
    /// <param name="expectedBlocks">Whether interaction is blocked.</param>
    [TestCase(ProtocolViolationType.None, false)]
    [TestCase(ProtocolViolationType.Minor, false)]
    [TestCase(ProtocolViolationType.Moderate, false)]
    [TestCase(ProtocolViolationType.Severe, false)]
    [TestCase(ProtocolViolationType.Unforgivable, true)]
    public void ProtocolViolationType_BlocksInteraction_ReturnsCorrectValue(
        ProtocolViolationType violation,
        bool expectedBlocks)
    {
        // Act
        var blocks = violation.BlocksInteraction();

        // Assert
        blocks.Should().Be(expectedBlocks,
            because: $"{violation} violation should {(expectedBlocks ? "" : "not ")}block interaction");
    }

    /// <summary>
    /// Verifies that disposition changes scale with violation severity.
    /// </summary>
    [Test]
    public void ProtocolViolationType_GetDispositionChange_ScalesWithSeverity()
    {
        // Act
        var noneChange = ProtocolViolationType.None.GetDispositionChange();
        var minorChange = ProtocolViolationType.Minor.GetDispositionChange();
        var moderateChange = ProtocolViolationType.Moderate.GetDispositionChange();
        var severeChange = ProtocolViolationType.Severe.GetDispositionChange();
        var unforgivableChange = ProtocolViolationType.Unforgivable.GetDispositionChange();

        // Assert - Each tier should have more negative impact
        noneChange.Should().Be(0);
        minorChange.Should().BeLessThan(noneChange);
        moderateChange.Should().BeLessThan(minorChange);
        severeChange.Should().BeLessThan(moderateChange);
        unforgivableChange.Should().BeLessThan(severeChange);
    }

    /// <summary>
    /// Verifies that most violations are recoverable, but Unforgivable is not.
    /// </summary>
    /// <param name="violation">The protocol violation type.</param>
    /// <param name="expectedRecoverable">Whether recovery is possible.</param>
    [TestCase(ProtocolViolationType.None, true)]
    [TestCase(ProtocolViolationType.Minor, true)]
    [TestCase(ProtocolViolationType.Moderate, true)]
    [TestCase(ProtocolViolationType.Severe, true)]
    [TestCase(ProtocolViolationType.Unforgivable, false)]
    public void ProtocolViolationType_IsRecoverable_ReturnsCorrectValue(
        ProtocolViolationType violation,
        bool expectedRecoverable)
    {
        // Act
        var isRecoverable = violation.IsRecoverable();

        // Assert
        isRecoverable.Should().Be(expectedRecoverable,
            because: $"{violation} violation should {(expectedRecoverable ? "" : "not ")}be recoverable");
    }

    /// <summary>
    /// Verifies that severe and unforgivable violations may trigger hostility.
    /// </summary>
    /// <param name="violation">The protocol violation type.</param>
    /// <param name="expectedMayTrigger">Whether hostility may be triggered.</param>
    [TestCase(ProtocolViolationType.None, false)]
    [TestCase(ProtocolViolationType.Minor, false)]
    [TestCase(ProtocolViolationType.Moderate, false)]
    [TestCase(ProtocolViolationType.Severe, true)]
    [TestCase(ProtocolViolationType.Unforgivable, true)]
    public void ProtocolViolationType_MayTriggerHostility_ReturnsCorrectValue(
        ProtocolViolationType violation,
        bool expectedMayTrigger)
    {
        // Act
        var mayTrigger = violation.MayTriggerHostility();

        // Assert
        mayTrigger.Should().Be(expectedMayTrigger,
            because: $"{violation} violation should {(expectedMayTrigger ? "" : "not ")}potentially trigger hostility");
    }

    /// <summary>
    /// Verifies that all violation types have display names defined.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    [TestCase(ProtocolViolationType.None)]
    [TestCase(ProtocolViolationType.Minor)]
    [TestCase(ProtocolViolationType.Moderate)]
    [TestCase(ProtocolViolationType.Severe)]
    [TestCase(ProtocolViolationType.Unforgivable)]
    public void ProtocolViolationType_GetDisplayName_ReturnsNonEmpty(ProtocolViolationType violation)
    {
        // Act
        var displayName = violation.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{violation} should have a display name defined");
    }

    /// <summary>
    /// Verifies that all violation types have consequence descriptions.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    [TestCase(ProtocolViolationType.None)]
    [TestCase(ProtocolViolationType.Minor)]
    [TestCase(ProtocolViolationType.Moderate)]
    [TestCase(ProtocolViolationType.Severe)]
    [TestCase(ProtocolViolationType.Unforgivable)]
    public void ProtocolViolationType_GetConsequenceDescription_ReturnsNonEmpty(
        ProtocolViolationType violation)
    {
        // Act
        var description = violation.GetConsequenceDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{violation} should have a consequence description defined");
    }

    /// <summary>
    /// Verifies that all violation types have recovery descriptions.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    [TestCase(ProtocolViolationType.None)]
    [TestCase(ProtocolViolationType.Minor)]
    [TestCase(ProtocolViolationType.Moderate)]
    [TestCase(ProtocolViolationType.Severe)]
    [TestCase(ProtocolViolationType.Unforgivable)]
    public void ProtocolViolationType_GetRecoveryDescription_ReturnsNonEmpty(
        ProtocolViolationType violation)
    {
        // Act
        var description = violation.GetRecoveryDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{violation} should have a recovery description defined");
    }

    #endregion

    #region Test 4: CantModifier Value Object Tests

    /// <summary>
    /// Verifies that CantModifier.FromFluency creates correct modifier for each level.
    /// </summary>
    /// <param name="fluency">The fluency level.</param>
    /// <param name="expectedModifier">The expected dice modifier.</param>
    [TestCase(CantFluency.None, -1)]
    [TestCase(CantFluency.Basic, 0)]
    [TestCase(CantFluency.Fluent, 1)]
    public void CantModifier_FromFluency_CreatesCorrectModifier(
        CantFluency fluency,
        int expectedModifier)
    {
        // Act
        var cantModifier = CantModifier.FromFluency(
            fluency,
            "dvergr",
            "Dvergr Trade-Tongue");

        // Assert
        cantModifier.DiceModifier.Should().Be(expectedModifier);
        cantModifier.Fluency.Should().Be(fluency);
        cantModifier.CultureId.Should().Be("dvergr");
        cantModifier.CantName.Should().Be("Dvergr Trade-Tongue");
    }

    /// <summary>
    /// Verifies that CantModifier.Neutral creates a zero-effect modifier.
    /// </summary>
    [Test]
    public void CantModifier_Neutral_CreatesZeroEffectModifier()
    {
        // Act
        var cantModifier = CantModifier.Neutral("common-folk");

        // Assert
        cantModifier.DiceModifier.Should().Be(0);
        cantModifier.Fluency.Should().Be(CantFluency.Basic);
        cantModifier.IsNeutral.Should().BeTrue();
        cantModifier.IsBonus.Should().BeFalse();
        cantModifier.IsPenalty.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CantModifier.FluentSpeaker creates a bonus modifier.
    /// </summary>
    [Test]
    public void CantModifier_FluentSpeaker_CreatesBonusModifier()
    {
        // Act
        var cantModifier = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");

        // Assert
        cantModifier.DiceModifier.Should().Be(1);
        cantModifier.IsBonus.Should().BeTrue();
        cantModifier.IsPenalty.Should().BeFalse();
        cantModifier.HasCulturalDialogueAccess.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CantModifier.Unfamiliar creates a penalty modifier.
    /// </summary>
    [Test]
    public void CantModifier_Unfamiliar_CreatesPenaltyModifier()
    {
        // Act
        var cantModifier = CantModifier.Unfamiliar("utgard", "Utgard Shadow-Tongue");

        // Assert
        cantModifier.DiceModifier.Should().Be(-1);
        cantModifier.IsPenalty.Should().BeTrue();
        cantModifier.IsBonus.Should().BeFalse();
        cantModifier.CanUnderstand.Should().BeFalse();
        cantModifier.CanSpeak.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CantModifier.ToDisplayString formats correctly for each state.
    /// </summary>
    [Test]
    public void CantModifier_ToDisplayString_FormatsCorrectly()
    {
        // Arrange
        var fluent = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");
        var basic = CantModifier.FromFluency(CantFluency.Basic, "dvergr", "Dvergr Trade-Tongue");
        var none = CantModifier.Unfamiliar("dvergr", "Dvergr Trade-Tongue");

        // Act & Assert
        fluent.ToDisplayString().Should().Contain("+1d10");
        fluent.ToDisplayString().Should().Contain("Fluent");

        basic.ToDisplayString().Should().Contain("No modifier");
        basic.ToDisplayString().Should().Contain("Basic");

        none.ToDisplayString().Should().Contain("-1d10");
        none.ToDisplayString().Should().Contain("Unfamiliar");
    }

    /// <summary>
    /// Verifies that CantModifier.ToSocialModifier creates valid SocialModifier.
    /// </summary>
    [Test]
    public void CantModifier_ToSocialModifier_CreatesValidSocialModifier()
    {
        // Arrange
        var cantModifier = CantModifier.FluentSpeaker("dvergr", "Dvergr Trade-Tongue");

        // Act
        var socialModifier = cantModifier.ToSocialModifier();

        // Assert
        socialModifier.Source.Should().Contain("Cant:");
        socialModifier.Source.Should().Contain("Dvergr Trade-Tongue");
        socialModifier.DiceModifier.Should().Be(1);
        socialModifier.DcModifier.Should().Be(0,
            because: "Cant affects dice pool, not DC");
        socialModifier.ModifierType.Should().Be(SocialModifierType.Cultural);
    }

    #endregion

    #region Test 5: VeilSpeechContext Value Object Tests

    /// <summary>
    /// Verifies that VeilSpeechContext.ForDirectTruth sets correct modifiers.
    /// </summary>
    [Test]
    public void VeilSpeechContext_ForDirectTruth_SetsCorrectModifiers()
    {
        // Act
        var context = VeilSpeechContext.ForDirectTruth(VeilSpeechState.Neutral, "utgard-npc-001");

        // Assert
        context.CurrentState.Should().Be(VeilSpeechState.Neutral);
        context.IsTellingDirectTruth.Should().BeTrue();
        context.GetDiceModifier().Should().Be(-2,
            because: "Direct truth offends Utgard and removes 2 dice");
        context.GetDcAdjustment().Should().Be(0);
        context.WillCauseOffense.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that VeilSpeechContext.ForProperVeilSpeech sets correct modifiers.
    /// </summary>
    [Test]
    public void VeilSpeechContext_ForProperVeilSpeech_SetsCorrectModifiers()
    {
        // Act
        var context = VeilSpeechContext.ForProperVeilSpeech(VeilSpeechState.Neutral, "utgard-npc-001");

        // Assert
        context.CurrentState.Should().Be(VeilSpeechState.Neutral);
        context.IsUsingProperVeilSpeech.Should().BeTrue();
        context.GetDiceModifier().Should().Be(1,
            because: "Proper Veil-Speech is respected and adds 1 die");
        context.GetDcAdjustment().Should().Be(0);
        context.WillImproveStanding.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that VeilSpeechContext.ForDeception sets correct modifiers.
    /// </summary>
    [Test]
    public void VeilSpeechContext_ForDeception_SetsCorrectModifiers()
    {
        // Act
        var context = VeilSpeechContext.ForDeception(VeilSpeechState.Neutral, "utgard-npc-001");

        // Assert
        context.CurrentState.Should().Be(VeilSpeechState.Neutral);
        context.IsUsingDeception.Should().BeTrue();
        context.GetDiceModifier().Should().Be(0);
        context.GetDcAdjustment().Should().Be(-4,
            because: "Skilled deception reduces DC by 4 in Utgard culture");
    }

    /// <summary>
    /// Verifies that VeilSpeechContext.ToSocialModifier creates valid SocialModifier.
    /// </summary>
    [Test]
    public void VeilSpeechContext_ToSocialModifier_CreatesValidSocialModifier()
    {
        // Arrange
        var context = VeilSpeechContext.ForProperVeilSpeech(VeilSpeechState.Neutral, "utgard-npc-001");

        // Act
        var socialModifier = context.ToSocialModifier();

        // Assert
        socialModifier.Source.Should().Contain("Veil-Speech");
        socialModifier.DiceModifier.Should().Be(1);
        socialModifier.ModifierType.Should().Be(SocialModifierType.Cultural);
    }

    /// <summary>
    /// Verifies that GetResultingState correctly transitions states.
    /// </summary>
    [Test]
    public void VeilSpeechContext_GetResultingState_TransitionsCorrectly()
    {
        // Arrange - Direct truth from Neutral
        var directTruthFromNeutral = VeilSpeechContext.ForDirectTruth(VeilSpeechState.Neutral, "npc-001");
        // Arrange - Proper Veil-Speech from Neutral
        var properVeilFromNeutral = VeilSpeechContext.ForProperVeilSpeech(VeilSpeechState.Neutral, "npc-001");
        // Arrange - Direct truth from Offended
        var directTruthFromOffended = VeilSpeechContext.ForDirectTruth(VeilSpeechState.Offended, "npc-001");
        // Arrange - Proper Veil-Speech from Offended
        var properVeilFromOffended = VeilSpeechContext.ForProperVeilSpeech(VeilSpeechState.Offended, "npc-001");

        // Assert
        directTruthFromNeutral.GetResultingState().Should().Be(VeilSpeechState.Offended,
            because: "Direct truth from Neutral should transition to Offended");
        properVeilFromNeutral.GetResultingState().Should().Be(VeilSpeechState.Respected,
            because: "Proper Veil-Speech from Neutral should transition to Respected");
        directTruthFromOffended.GetResultingState().Should().Be(VeilSpeechState.DeepOffense,
            because: "Direct truth from Offended should transition to DeepOffense");
        properVeilFromOffended.GetResultingState().Should().Be(VeilSpeechState.Neutral,
            because: "Proper Veil-Speech from Offended should transition to Neutral (apology accepted)");
    }

    #endregion

    #region Test 6: ProtocolCheckResult Value Object Tests

    /// <summary>
    /// Verifies that ProtocolCheckResult.Compliant creates a compliant result.
    /// </summary>
    [Test]
    public void ProtocolCheckResult_Compliant_IsCompliant()
    {
        // Act
        var result = ProtocolCheckResult.Compliant("dvergr", "Logic-Chain");

        // Assert
        result.IsCompliant.Should().BeTrue();
        result.ViolationType.Should().Be(ProtocolViolationType.None);
        result.DcAdjustment.Should().Be(0);
        result.TotalDiceModifier.Should().Be(0);
        result.BlocksInteraction.Should().BeFalse();
        result.CultureId.Should().Be("dvergr");
        result.ProtocolName.Should().Be("Logic-Chain");
    }

    /// <summary>
    /// Verifies that ProtocolCheckResult with violations is not compliant.
    /// </summary>
    [Test]
    public void ProtocolCheckResult_Violation_IsNotCompliant()
    {
        // Act
        var result = ProtocolCheckResult.Violation(
            "dvergr",
            "Logic-Chain",
            ProtocolViolationType.Minor,
            failedRequirements: new[] { "Failed to observe greeting ritual" });

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.ViolationType.Should().Be(ProtocolViolationType.Minor);
        result.DcAdjustment.Should().Be(2,
            because: "Minor violation adds +2 DC");
        result.FailedRequirements.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that ProtocolCheckResult with Unforgivable blocks interaction.
    /// </summary>
    [Test]
    public void ProtocolCheckResult_Unforgivable_BlocksInteraction()
    {
        // Act
        var result = ProtocolCheckResult.Violation(
            "iron-bane",
            "Martial Tribute",
            ProtocolViolationType.Unforgivable,
            failedRequirements: new[] { "Committed blood-oath betrayal" });

        // Assert
        result.BlocksInteraction.Should().BeTrue(
            because: "Unforgivable violations block all further interaction");
    }

    /// <summary>
    /// Verifies that violation results include dice penalties.
    /// </summary>
    [Test]
    public void ProtocolCheckResult_Violation_IncludesDicePenalty()
    {
        // Act
        var moderateResult = ProtocolCheckResult.Violation(
            "dvergr",
            "Logic-Chain",
            ProtocolViolationType.Moderate,
            failedRequirements: new[] { "Illogical argument" });

        var severeResult = ProtocolCheckResult.Violation(
            "gorge-maw",
            "Hospitality Rite",
            ProtocolViolationType.Severe,
            failedRequirements: new[] { "Interrupted greeting" });

        // Assert
        moderateResult.TotalDiceModifier.Should().Be(-1,
            because: "Moderate violation removes 1 die");
        severeResult.TotalDiceModifier.Should().Be(-2,
            because: "Severe violation removes 2 dice");
    }

    /// <summary>
    /// Verifies that NoProtocol creates a neutral result for non-cultural interactions.
    /// </summary>
    [Test]
    public void ProtocolCheckResult_NoProtocol_CreatesNeutralResult()
    {
        // Act
        var result = ProtocolCheckResult.NoProtocol();

        // Assert
        result.IsCompliant.Should().BeTrue();
        result.HasProtocol.Should().BeFalse();
        result.DcAdjustment.Should().Be(0);
        result.TotalDiceModifier.Should().Be(0);
    }

    #endregion

    #region Test 7: ProtocolRequirement Value Object Tests

    /// <summary>
    /// Verifies that ProtocolRequirement factory methods create valid requirements.
    /// </summary>
    [Test]
    public void ProtocolRequirement_FactoryMethods_CreateValidRequirements()
    {
        // Act
        var behavioral = ProtocolRequirement.Behavioral(
            "Listen without interruption",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Moderate);

        var verbal = ProtocolRequirement.Verbal(
            "Use formal address",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Minor);

        var offering = ProtocolRequirement.Offering(
            "Present a gift",
            mandatory: false);

        var mental = ProtocolRequirement.Mental(
            "Open mind to telepathy",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Severe);

        var ritual = ProtocolRequirement.Ritual(
            "Complete greeting ceremony",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Moderate);

        var status = ProtocolRequirement.StatusAcknowledgment(
            "Acknowledge hierarchy",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Moderate);

        // Assert
        behavioral.RequirementType.Should().Be(ProtocolRequirementType.Behavioral);
        behavioral.IsMandatory.Should().BeTrue();
        behavioral.ViolationSeverity.Should().Be(ProtocolViolationType.Moderate);

        verbal.RequirementType.Should().Be(ProtocolRequirementType.Verbal);
        offering.RequirementType.Should().Be(ProtocolRequirementType.Offering);
        offering.IsMandatory.Should().BeFalse();

        mental.RequirementType.Should().Be(ProtocolRequirementType.Mental);
        ritual.RequirementType.Should().Be(ProtocolRequirementType.Ritual);
        status.RequirementType.Should().Be(ProtocolRequirementType.StatusAcknowledgment);
    }

    /// <summary>
    /// Verifies that optional requirements have lower violation severity by default.
    /// </summary>
    [Test]
    public void ProtocolRequirement_Optional_HasDefaultMinorSeverity()
    {
        // Act
        var requirement = ProtocolRequirement.Offering(
            "Offering a small gift shows respect",
            mandatory: false);

        // Assert
        requirement.IsMandatory.Should().BeFalse();
        requirement.ViolationSeverity.Should().Be(ProtocolViolationType.Minor);
    }

    /// <summary>
    /// Verifies that ProtocolRequirement.WithSkillCheck creates requirement with skill check.
    /// </summary>
    [Test]
    public void ProtocolRequirement_WithSkillCheck_HasSkillCheckDefined()
    {
        // Act
        var requirement = ProtocolRequirement.WithSkillCheck(
            text: "Open your mind to the pack's telepathic contact",
            requirementType: ProtocolRequirementType.Mental,
            skillId: "will",
            dc: 14,
            failureConsequence: "Mind remains closed; perceived as hostile or deceptive",
            mandatory: true,
            violationSeverity: ProtocolViolationType.Severe);

        // Assert
        requirement.HasSkillCheck.Should().BeTrue();
        requirement.SkillCheck.Should().NotBeNull();
        requirement.SkillCheck!.Value.SkillId.Should().Be("will");
        requirement.SkillCheck!.Value.Dc.Should().Be(14);
        requirement.ViolationSeverity.Should().Be(ProtocolViolationType.Severe);
    }

    #endregion

    #region Test 8: CultureProtocol Entity Tests

    /// <summary>
    /// Verifies that CultureProtocol.Dvergr has correct base DC and special protocol.
    /// </summary>
    [Test]
    public void CultureProtocol_Dvergr_HasCorrectProperties()
    {
        // Act
        var protocol = CultureProtocol.Dvergr();

        // Assert
        protocol.CultureId.Should().Be("dvergr");
        protocol.BaseDc.Should().Be(18,
            because: "Dvergr have the highest base DC (18)");
        protocol.IsLogicChain.Should().BeTrue(
            because: "Dvergr culture requires logical argument chains");
        protocol.CantName.Should().Contain("Dvergr");
        protocol.HasMandatoryRequirements.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that CultureProtocol.Utgard has correct base DC and special protocol.
    /// </summary>
    [Test]
    public void CultureProtocol_Utgard_HasCorrectProperties()
    {
        // Act
        var protocol = CultureProtocol.Utgard();

        // Assert
        protocol.CultureId.Should().Be("utgard");
        protocol.BaseDc.Should().Be(16,
            because: "Utgard have DC 16");
        protocol.IsVeilSpeech.Should().BeTrue(
            because: "Utgard culture requires Veil-Speech communication");
        protocol.CantName.Should().Contain("Utgard");
    }

    /// <summary>
    /// Verifies that CultureProtocol.GorgeMaw has correct base DC and special protocol.
    /// </summary>
    [Test]
    public void CultureProtocol_GorgeMaw_HasCorrectProperties()
    {
        // Act
        var protocol = CultureProtocol.GorgeMaw();

        // Assert
        protocol.CultureId.Should().Be("gorge-maw");
        protocol.BaseDc.Should().Be(14,
            because: "Gorge-Maw have DC 14");
        protocol.IsHospitalityRite.Should().BeTrue(
            because: "Gorge-Maw culture requires hospitality rituals");
        protocol.CantName.Should().Contain("Gorge-Maw");
    }

    /// <summary>
    /// Verifies that CultureProtocol.RuneLupin has correct base DC and special protocol.
    /// </summary>
    [Test]
    public void CultureProtocol_RuneLupin_HasCorrectProperties()
    {
        // Act
        var protocol = CultureProtocol.RuneLupin();

        // Assert
        protocol.CultureId.Should().Be("rune-lupin");
        protocol.BaseDc.Should().Be(12,
            because: "Rune-Lupin have the lowest base DC (12)");
        protocol.IsTelepathy.Should().BeTrue(
            because: "Rune-Lupin culture uses semi-telepathic communication");
        protocol.CantName.Should().Contain("Lupin");
    }

    /// <summary>
    /// Verifies that CultureProtocol.IronBane has correct base DC and special protocol.
    /// </summary>
    [Test]
    public void CultureProtocol_IronBane_HasCorrectProperties()
    {
        // Act
        var protocol = CultureProtocol.IronBane();

        // Assert
        protocol.CultureId.Should().Be("iron-bane");
        protocol.BaseDc.Should().Be(16,
            because: "Iron-Bane have DC 16");
        protocol.IsMartialTribute.Should().BeTrue(
            because: "Iron-Bane culture requires martial tribute");
        protocol.CantName.Should().Contain("Iron-Bane");
    }

    #endregion

    #region Test 9: Culture Base DC Tests

    /// <summary>
    /// Verifies that each culture has the correct base DC as specified.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="expectedBaseDc">The expected base DC.</param>
    /// <remarks>
    /// <para>
    /// Dvergr: DC 18 (elaborate protocols, precision-focused)
    /// Utgard: DC 16 (complex Veil-Speech requirements)
    /// Gorge-Maw: DC 14 (patient but straightforward hospitality)
    /// Rune-Lupin: DC 12 (pack hierarchy is intuitive)
    /// Iron-Bane: DC 16 (martial honor codes)
    /// </para>
    /// </remarks>
    [TestCase("dvergr", 18)]
    [TestCase("utgard", 16)]
    [TestCase("gorge-maw", 14)]
    [TestCase("rune-lupin", 12)]
    [TestCase("iron-bane", 16)]
    public void CultureProtocol_HasCorrectBaseDc(string cultureId, int expectedBaseDc)
    {
        // Arrange
        var protocol = CreateProtocolForCulture(cultureId);

        // Assert
        protocol.BaseDc.Should().Be(expectedBaseDc,
            because: $"{cultureId} culture should have base DC {expectedBaseDc}");
    }

    #endregion

    #region Test 10: SpecialProtocolType Tests

    /// <summary>
    /// Verifies that each culture has the correct special protocol type.
    /// </summary>
    [Test]
    public void CultureProtocol_SpecialProtocolTypes_AreCorrect()
    {
        // Arrange & Act
        var dvergr = CultureProtocol.Dvergr();
        var utgard = CultureProtocol.Utgard();
        var gorgeMaw = CultureProtocol.GorgeMaw();
        var runeLupin = CultureProtocol.RuneLupin();
        var ironBane = CultureProtocol.IronBane();

        // Assert
        dvergr.SpecialRuleType.Should().Be(SpecialProtocolType.LogicChain);
        utgard.SpecialRuleType.Should().Be(SpecialProtocolType.VeilSpeech);
        gorgeMaw.SpecialRuleType.Should().Be(SpecialProtocolType.HospitalityRite);
        runeLupin.SpecialRuleType.Should().Be(SpecialProtocolType.Telepathy);
        ironBane.SpecialRuleType.Should().Be(SpecialProtocolType.MartialTribute);
    }

    /// <summary>
    /// Verifies that all culture protocols have detailed requirements.
    /// </summary>
    [Test]
    public void CultureProtocol_AllCultures_HaveDetailedRequirements()
    {
        // Arrange & Act
        var dvergr = CultureProtocol.Dvergr();
        var utgard = CultureProtocol.Utgard();
        var gorgeMaw = CultureProtocol.GorgeMaw();
        var runeLupin = CultureProtocol.RuneLupin();
        var ironBane = CultureProtocol.IronBane();

        // Assert
        dvergr.DetailedRequirements.Should().NotBeEmpty(
            because: "Dvergr protocol should have detailed requirements");
        utgard.DetailedRequirements.Should().NotBeEmpty(
            because: "Utgard protocol should have detailed requirements");
        gorgeMaw.DetailedRequirements.Should().NotBeEmpty(
            because: "Gorge-Maw protocol should have detailed requirements");
        runeLupin.DetailedRequirements.Should().NotBeEmpty(
            because: "Rune-Lupin protocol should have detailed requirements");
        ironBane.DetailedRequirements.Should().NotBeEmpty(
            because: "Iron-Bane protocol should have detailed requirements");
    }

    #endregion

    #region Test 11: ProtocolRequirementType Tests

    /// <summary>
    /// Verifies that all protocol requirement types have display names.
    /// </summary>
    /// <param name="requirementType">The requirement type.</param>
    [TestCase(ProtocolRequirementType.Behavioral)]
    [TestCase(ProtocolRequirementType.Verbal)]
    [TestCase(ProtocolRequirementType.Offering)]
    [TestCase(ProtocolRequirementType.Mental)]
    [TestCase(ProtocolRequirementType.Ritual)]
    [TestCase(ProtocolRequirementType.StatusAcknowledgment)]
    public void ProtocolRequirementType_HasDisplayName_ThroughRequirement(
        ProtocolRequirementType requirementType)
    {
        // Arrange
        var requirement = new ProtocolRequirement(
            "Test requirement",
            requirementType,
            IsMandatory: true,
            SkillCheck: null,
            ViolationSeverity: ProtocolViolationType.Minor);

        // Act
        var displayName = requirement.GetTypeDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{requirementType} should have a display name defined");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a protocol for the specified culture using the static factory methods.
    /// </summary>
    /// <param name="cultureId">The culture identifier.</param>
    /// <returns>The culture protocol.</returns>
    private static CultureProtocol CreateProtocolForCulture(string cultureId)
    {
        return cultureId switch
        {
            "dvergr" => CultureProtocol.Dvergr(),
            "utgard" => CultureProtocol.Utgard(),
            "gorge-maw" => CultureProtocol.GorgeMaw(),
            "rune-lupin" => CultureProtocol.RuneLupin(),
            "iron-bane" => CultureProtocol.IronBane(),
            _ => throw new ArgumentException($"Unknown culture: {cultureId}", nameof(cultureId))
        };
    }

    #endregion
}
