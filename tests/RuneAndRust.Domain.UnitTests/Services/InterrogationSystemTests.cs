// ------------------------------------------------------------------------------
// <copyright file="InterrogationSystemTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the Interrogation System including subject resistance,
// method mechanics, information reliability, and Torture consequences.
// Part of v0.15.3f Interrogation System implementation.
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
/// Unit tests for the Interrogation System components.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the required acceptance criteria from the design specification:
/// </para>
/// <list type="number">
///   <item>
///     <description>Subject WILL correctly sets resistance level (Minimal to Extreme)</description>
///   </item>
///   <item>
///     <description>Method success reduces resistance by 1, failure does not</description>
///   </item>
///   <item>
///     <description>Information reliability is based on primary method used</description>
///   </item>
///   <item>
///     <description>Torture caps reliability at 60% regardless of other methods</description>
///   </item>
/// </list>
/// </remarks>
[TestFixture]
public class InterrogationSystemTests
{
    #region Test 1: Subject WILL Sets Resistance Correctly

    /// <summary>
    /// Verifies that each WILL range maps to the correct resistance level.
    /// This is the core mechanic for determining interrogation difficulty.
    /// </summary>
    /// <param name="will">The subject's WILL attribute.</param>
    /// <param name="expectedResistance">The expected resistance level.</param>
    [TestCase(1, SubjectResistance.Minimal)]
    [TestCase(2, SubjectResistance.Minimal)]
    [TestCase(3, SubjectResistance.Low)]
    [TestCase(4, SubjectResistance.Low)]
    [TestCase(5, SubjectResistance.Moderate)]
    [TestCase(6, SubjectResistance.Moderate)]
    [TestCase(7, SubjectResistance.High)]
    [TestCase(8, SubjectResistance.High)]
    [TestCase(9, SubjectResistance.Extreme)]
    [TestCase(10, SubjectResistance.Extreme)]
    public void FromWillAttribute_ReturnsCorrectResistanceLevel(
        int will,
        SubjectResistance expectedResistance)
    {
        // Act
        var resistance = SubjectResistanceExtensions.FromWillAttribute(will);

        // Assert
        resistance.Should().Be(expectedResistance,
            because: $"WILL {will} should map to {expectedResistance} resistance");
    }

    /// <summary>
    /// Verifies that resistance levels have correct check-to-break ranges.
    /// </summary>
    /// <param name="resistance">The resistance level.</param>
    /// <param name="expectedMin">Expected minimum checks to break.</param>
    /// <param name="expectedMax">Expected maximum checks to break.</param>
    [TestCase(SubjectResistance.Minimal, 1, 1)]
    [TestCase(SubjectResistance.Low, 2, 3)]
    [TestCase(SubjectResistance.Moderate, 4, 5)]
    [TestCase(SubjectResistance.High, 6, 8)]
    [TestCase(SubjectResistance.Extreme, 10, 15)]
    public void GetChecksToBreak_ReturnsCorrectRange(
        SubjectResistance resistance,
        int expectedMin,
        int expectedMax)
    {
        // Act
        var minChecks = resistance.GetMinChecksToBreak();
        var maxChecks = resistance.GetMaxChecksToBreak();

        // Assert
        minChecks.Should().Be(expectedMin,
            because: $"{resistance} should require minimum {expectedMin} checks");
        maxChecks.Should().Be(expectedMax,
            because: $"{resistance} should require maximum {expectedMax} checks");
    }

    /// <summary>
    /// Verifies that all resistance levels have display names.
    /// </summary>
    /// <param name="resistance">The resistance level.</param>
    [TestCase(SubjectResistance.Minimal)]
    [TestCase(SubjectResistance.Low)]
    [TestCase(SubjectResistance.Moderate)]
    [TestCase(SubjectResistance.High)]
    [TestCase(SubjectResistance.Extreme)]
    public void GetDisplayName_ReturnsNonEmpty(SubjectResistance resistance)
    {
        // Act
        var displayName = resistance.GetDisplayName();

        // Assert
        displayName.Should().NotBeNullOrWhiteSpace(
            because: $"{resistance} should have a display name defined");
    }

    /// <summary>
    /// Verifies that bribery cost scales with resistance level.
    /// </summary>
    [Test]
    public void GetBaseBriberyCost_ScalesWithResistance()
    {
        // Act
        var minimalCost = SubjectResistance.Minimal.GetBaseBriberyCost();
        var lowCost = SubjectResistance.Low.GetBaseBriberyCost();
        var moderateCost = SubjectResistance.Moderate.GetBaseBriberyCost();
        var highCost = SubjectResistance.High.GetBaseBriberyCost();
        var extremeCost = SubjectResistance.Extreme.GetBaseBriberyCost();

        // Assert - Each tier should cost more
        minimalCost.Should().BeLessThan(lowCost);
        lowCost.Should().BeLessThan(moderateCost);
        moderateCost.Should().BeLessThan(highCost);
        highCost.Should().BeLessThan(extremeCost);
    }

    /// <summary>
    /// Verifies that resistance levels have typical subject descriptions.
    /// </summary>
    /// <param name="resistance">The resistance level.</param>
    [TestCase(SubjectResistance.Minimal)]
    [TestCase(SubjectResistance.Low)]
    [TestCase(SubjectResistance.Moderate)]
    [TestCase(SubjectResistance.High)]
    [TestCase(SubjectResistance.Extreme)]
    public void GetTypicalSubjectDescription_ReturnsNonEmpty(SubjectResistance resistance)
    {
        // Act
        var description = resistance.GetTypicalSubjectDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace(
            because: $"{resistance} should have a description");
    }

    #endregion

    #region Test 2: Interrogation Method Mechanics

    /// <summary>
    /// Verifies that each method has the correct base DC.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedDc">The expected base DC.</param>
    [TestCase(InterrogationMethod.GoodCop, 14)]
    [TestCase(InterrogationMethod.BadCop, 12)]
    [TestCase(InterrogationMethod.Deception, 16)]
    [TestCase(InterrogationMethod.Bribery, 10)]
    [TestCase(InterrogationMethod.Torture, 0)] // Torture uses WILL×2 instead
    public void GetBaseDc_ReturnsCorrectDc(InterrogationMethod method, int expectedDc)
    {
        // Act
        var baseDc = method.GetBaseDc();

        // Assert
        baseDc.Should().Be(expectedDc,
            because: $"{method} should have DC {expectedDc}");
    }

    /// <summary>
    /// Verifies that each method has the correct reliability percentage.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedReliability">The expected reliability percentage.</param>
    [TestCase(InterrogationMethod.GoodCop, 95)]
    [TestCase(InterrogationMethod.BadCop, 80)]
    [TestCase(InterrogationMethod.Deception, 70)]
    [TestCase(InterrogationMethod.Bribery, 90)]
    [TestCase(InterrogationMethod.Torture, 50)]
    public void GetReliabilityPercent_ReturnsCorrectValue(
        InterrogationMethod method,
        int expectedReliability)
    {
        // Act
        var reliability = method.GetReliabilityPercent();

        // Assert
        reliability.Should().Be(expectedReliability,
            because: $"{method} should have {expectedReliability}% reliability");
    }

    /// <summary>
    /// Verifies that each method has the correct round duration.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedMinutes">The expected duration in minutes.</param>
    [TestCase(InterrogationMethod.GoodCop, 30)]
    [TestCase(InterrogationMethod.BadCop, 15)]
    [TestCase(InterrogationMethod.Deception, 20)]
    [TestCase(InterrogationMethod.Bribery, 10)]
    [TestCase(InterrogationMethod.Torture, 60)]
    public void GetRoundDurationMinutes_ReturnsCorrectValue(
        InterrogationMethod method,
        int expectedMinutes)
    {
        // Act
        var duration = method.GetRoundDurationMinutes();

        // Assert
        duration.Should().Be(expectedMinutes,
            because: $"{method} should take {expectedMinutes} minutes");
    }

    /// <summary>
    /// Verifies that methods have correct disposition changes.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedChange">The expected disposition change.</param>
    [TestCase(InterrogationMethod.GoodCop, 0)]
    [TestCase(InterrogationMethod.BadCop, -5)]
    [TestCase(InterrogationMethod.Deception, -2)]
    [TestCase(InterrogationMethod.Bribery, 0)]
    [TestCase(InterrogationMethod.Torture, -20)]
    public void GetDispositionChangePerRound_ReturnsCorrectValue(
        InterrogationMethod method,
        int expectedChange)
    {
        // Act
        var change = method.GetDispositionChangePerRound();

        // Assert
        change.Should().Be(expectedChange,
            because: $"{method} should change disposition by {expectedChange}");
    }

    /// <summary>
    /// Verifies that only Bribery requires resources.
    /// </summary>
    [Test]
    public void RequiresResources_OnlyTrueForBribery()
    {
        // Assert
        InterrogationMethod.GoodCop.RequiresResources().Should().BeFalse();
        InterrogationMethod.BadCop.RequiresResources().Should().BeFalse();
        InterrogationMethod.Deception.RequiresResources().Should().BeFalse();
        InterrogationMethod.Bribery.RequiresResources().Should().BeTrue();
        InterrogationMethod.Torture.RequiresResources().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that only Torture caps reliability at 60%.
    /// </summary>
    [Test]
    public void CapsReliability_OnlyTrueForTorture()
    {
        // Assert
        InterrogationMethod.GoodCop.CapsReliability().Should().BeFalse();
        InterrogationMethod.BadCop.CapsReliability().Should().BeFalse();
        InterrogationMethod.Deception.CapsReliability().Should().BeFalse();
        InterrogationMethod.Bribery.CapsReliability().Should().BeFalse();
        InterrogationMethod.Torture.CapsReliability().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Torture is the only method with negative reputation cost.
    /// </summary>
    [Test]
    public void GetReputationCost_NegativeOnlyForTorture()
    {
        // Act
        var tortureRepCost = InterrogationMethod.Torture.GetReputationCost();
        var goodCopRepCost = InterrogationMethod.GoodCop.GetReputationCost();
        var badCopRepCost = InterrogationMethod.BadCop.GetReputationCost();
        var deceptionRepCost = InterrogationMethod.Deception.GetReputationCost();
        var briberyRepCost = InterrogationMethod.Bribery.GetReputationCost();

        // Assert
        tortureRepCost.Should().BeLessThan(0,
            because: "Torture should have severe reputation cost");
        goodCopRepCost.Should().Be(0);
        badCopRepCost.Should().Be(0);
        deceptionRepCost.Should().Be(0);
        briberyRepCost.Should().Be(0);
    }

    /// <summary>
    /// Verifies that each method maps to the correct underlying system.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedSystem">The expected underlying system (nullable).</param>
    [TestCase(InterrogationMethod.GoodCop, SocialInteractionType.Persuasion)]
    [TestCase(InterrogationMethod.BadCop, SocialInteractionType.Intimidation)]
    [TestCase(InterrogationMethod.Deception, SocialInteractionType.Deception)]
    [TestCase(InterrogationMethod.Bribery, SocialInteractionType.Persuasion)]
    public void GetUnderlyingSystem_ReturnsCorrectSystem(
        InterrogationMethod method,
        SocialInteractionType? expectedSystem)
    {
        // Act
        var underlyingSystem = method.GetUnderlyingSystem();

        // Assert
        underlyingSystem.Should().Be(expectedSystem,
            because: $"{method} should use {expectedSystem}");
    }

    /// <summary>
    /// Verifies that Torture has no underlying system (it's pure attribute check).
    /// </summary>
    [Test]
    public void Torture_GetUnderlyingSystem_ReturnsNull()
    {
        // Act
        var underlyingSystem = InterrogationMethod.Torture.GetUnderlyingSystem();

        // Assert
        underlyingSystem.Should().BeNull(
            because: "Torture uses raw attribute, not a social system");
    }

    /// <summary>
    /// Verifies that each method maps to the correct fumble type.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="expectedFumbleType">The expected fumble type.</param>
    [TestCase(InterrogationMethod.GoodCop, FumbleType.TrustShattered)]
    [TestCase(InterrogationMethod.BadCop, FumbleType.ChallengeAccepted)]
    [TestCase(InterrogationMethod.Deception, FumbleType.LieExposed)]
    [TestCase(InterrogationMethod.Bribery, FumbleType.TrustShattered)]
    [TestCase(InterrogationMethod.Torture, FumbleType.SubjectBroken)]
    public void GetFumbleType_ReturnsCorrectType(
        InterrogationMethod method,
        FumbleType expectedFumbleType)
    {
        // Act
        var fumbleType = method.GetFumbleType();

        // Assert
        fumbleType.Should().Be(expectedFumbleType,
            because: $"{method} fumble should be {expectedFumbleType}");
    }

    #endregion

    #region Test 3: Interrogation Status Tests

    /// <summary>
    /// Verifies that terminal statuses are correctly identified.
    /// </summary>
    /// <param name="status">The interrogation status.</param>
    /// <param name="expectedTerminal">Whether status is terminal.</param>
    [TestCase(InterrogationStatus.NotStarted, false)]
    [TestCase(InterrogationStatus.InProgress, false)]
    [TestCase(InterrogationStatus.SubjectBroken, true)]
    [TestCase(InterrogationStatus.Abandoned, true)]
    [TestCase(InterrogationStatus.SubjectResisting, true)]
    public void IsTerminal_ReturnsCorrectValue(
        InterrogationStatus status,
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
    public void IsSuccess_OnlyTrueForSubjectBroken()
    {
        // Assert
        InterrogationStatus.SubjectBroken.IsSuccess().Should().BeTrue();
        InterrogationStatus.Abandoned.IsSuccess().Should().BeFalse();
        InterrogationStatus.SubjectResisting.IsSuccess().Should().BeFalse();
        InterrogationStatus.InProgress.IsSuccess().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that failure statuses are correctly identified.
    /// </summary>
    [Test]
    public void IsFailure_TrueForAbandonedAndSubjectResisting()
    {
        // Assert
        InterrogationStatus.Abandoned.IsFailure().Should().BeTrue();
        InterrogationStatus.SubjectResisting.IsFailure().Should().BeTrue();
        InterrogationStatus.SubjectBroken.IsFailure().Should().BeFalse();
        InterrogationStatus.InProgress.IsFailure().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that CanExtractInformation is only true for SubjectBroken.
    /// </summary>
    [Test]
    public void CanExtractInformation_OnlyTrueForSubjectBroken()
    {
        // Assert
        InterrogationStatus.SubjectBroken.CanExtractInformation().Should().BeTrue();
        InterrogationStatus.Abandoned.CanExtractInformation().Should().BeFalse();
        InterrogationStatus.SubjectResisting.CanExtractInformation().Should().BeFalse();
        InterrogationStatus.InProgress.CanExtractInformation().Should().BeFalse();
    }

    #endregion

    #region Test 4: InterrogationState Entity Tests

    /// <summary>
    /// Creates a test InterrogationState with required properties.
    /// </summary>
    private static InterrogationState CreateTestState(
        int subjectWill = 5,
        SubjectResistance resistanceLevel = SubjectResistance.Moderate,
        int initialResistance = 4)
    {
        var state = new InterrogationState
        {
            InterrogationId = "test-" + Guid.NewGuid().ToString("N")[..8],
            InterrogatorId = "pc_001",
            SubjectId = "npc_001",
            SubjectWill = subjectWill,
            InitialResistance = initialResistance,
            ResistanceLevel = resistanceLevel
        };
        state.Initialize();
        return state;
    }

    /// <summary>
    /// Verifies that Initialize sets up state correctly.
    /// </summary>
    [Test]
    public void Initialize_SetsCorrectInitialState()
    {
        // Arrange & Act
        var state = CreateTestState(
            subjectWill: 5,
            resistanceLevel: SubjectResistance.Moderate,
            initialResistance: 4);

        // Assert
        state.ResistanceRemaining.Should().Be(4);
        state.RoundNumber.Should().Be(0);
        state.Status.Should().Be(InterrogationStatus.NotStarted);
        state.TotalDispositionChange.Should().Be(0);
        state.TotalReputationCost.Should().Be(0);
        state.TotalResourceCost.Should().Be(0);
        state.TortureUsed.Should().BeFalse();
        state.History.Should().BeEmpty();
        state.InformationGained.Should().BeEmpty();
        state.MethodsUsed.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that recording a round updates state correctly.
    /// </summary>
    [Test]
    public void RecordRound_UpdatesStateCorrectly()
    {
        // Arrange
        var state = CreateTestState();
        var round = new InterrogationRound
        {
            RoundNumber = 1,
            MethodUsed = InterrogationMethod.GoodCop,
            CheckResult = SkillOutcome.FullSuccess,
            DiceRolled = 8,
            SuccessesAchieved = 4,
            SuccessesRequired = 4,
            ResistanceChange = -1,
            ResistanceAfter = 3,
            DispositionChange = 0,
            ResourceCost = 0,
            TimeElapsedMinutes = 30,
            IsFumble = false,
            FumbleType = null,
            NarrativeDescription = "Test narrative"
        };

        // Act
        state.RecordRound(round);

        // Assert
        state.RoundNumber.Should().Be(1);
        state.Status.Should().Be(InterrogationStatus.InProgress);
        state.ResistanceRemaining.Should().Be(3);
        state.History.Should().HaveCount(1);
        state.MethodsUsed.Should().Contain(InterrogationMethod.GoodCop);
    }

    /// <summary>
    /// Verifies that recording a Torture round sets TortureUsed flag.
    /// </summary>
    [Test]
    public void RecordRound_TortureSetsTortureUsedFlag()
    {
        // Arrange
        var state = CreateTestState();
        var round = new InterrogationRound
        {
            RoundNumber = 1,
            MethodUsed = InterrogationMethod.Torture,
            CheckResult = SkillOutcome.FullSuccess,
            DiceRolled = 5,
            SuccessesAchieved = 3,
            SuccessesRequired = 2,
            ResistanceChange = -1,
            ResistanceAfter = 3,
            DispositionChange = -20,
            ResourceCost = 0,
            TimeElapsedMinutes = 60,
            IsFumble = false,
            FumbleType = null,
            NarrativeDescription = "Test narrative"
        };

        // Act
        state.RecordRound(round);

        // Assert
        state.TortureUsed.Should().BeTrue();
        state.TotalReputationCost.Should().BeLessThan(0,
            because: "Torture should incur reputation cost");
    }

    /// <summary>
    /// Verifies that subject breaks when resistance reaches 0.
    /// </summary>
    [Test]
    public void RecordRound_SubjectBreaksWhenResistanceReachesZero()
    {
        // Arrange - Create state with 1 resistance remaining
        var state = CreateTestState(
            subjectWill: 2,
            resistanceLevel: SubjectResistance.Minimal,
            initialResistance: 1);

        var round = new InterrogationRound
        {
            RoundNumber = 1,
            MethodUsed = InterrogationMethod.GoodCop,
            CheckResult = SkillOutcome.FullSuccess,
            DiceRolled = 8,
            SuccessesAchieved = 4,
            SuccessesRequired = 4,
            ResistanceChange = -1,
            ResistanceAfter = 0,
            DispositionChange = 0,
            ResourceCost = 0,
            TimeElapsedMinutes = 30,
            IsFumble = false,
            FumbleType = null,
            NarrativeDescription = "Test narrative"
        };

        // Act
        state.RecordRound(round);

        // Assert
        state.Status.Should().Be(InterrogationStatus.SubjectBroken);
        state.ResistanceRemaining.Should().Be(0);
    }

    /// <summary>
    /// Verifies that GetPrimaryMethod returns the most frequently used method.
    /// </summary>
    [Test]
    public void GetPrimaryMethod_ReturnsMostFrequentMethod()
    {
        // Arrange
        var state = CreateTestState();

        // Record 2 GoodCop rounds, 1 BadCop round
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.BadCop);

        // Act
        var primaryMethod = state.GetPrimaryMethod();

        // Assert
        primaryMethod.Should().Be(InterrogationMethod.GoodCop);
    }

    /// <summary>
    /// Verifies that CalculateReliability uses primary method's reliability.
    /// </summary>
    [Test]
    public void CalculateReliability_UsesPrimaryMethodReliability()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);

        // Act
        var reliability = state.CalculateReliability();

        // Assert
        reliability.Should().Be(95,
            because: "GoodCop has 95% reliability");
    }

    /// <summary>
    /// Verifies that CalculateReliability caps at 60% when Torture was used.
    /// </summary>
    [Test]
    public void CalculateReliability_CappedAt60WhenTortureUsed()
    {
        // Arrange
        var state = CreateTestState();

        // Use mostly GoodCop (95% reliable) but one Torture round
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.Torture);

        // Act
        var reliability = state.CalculateReliability();

        // Assert - Should be capped at 60% even though GoodCop is primary
        reliability.Should().BeLessOrEqualTo(60,
            because: "Torture usage caps reliability at 60%");
    }

    /// <summary>
    /// Verifies that Abandon sets status correctly.
    /// </summary>
    [Test]
    public void Abandon_SetsStatusToAbandoned()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);

        // Act
        state.Abandon();

        // Assert
        state.Status.Should().Be(InterrogationStatus.Abandoned);
    }

    /// <summary>
    /// Verifies that MarkSubjectResisting sets status correctly.
    /// </summary>
    [Test]
    public void MarkSubjectResisting_SetsStatusCorrectly()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);

        // Act
        state.MarkSubjectResisting();

        // Assert
        state.Status.Should().Be(InterrogationStatus.SubjectResisting);
    }

    /// <summary>
    /// Verifies that MarkSubjectBrokenBeyondRecovery applies additional reputation cost.
    /// </summary>
    [Test]
    public void MarkSubjectBrokenBeyondRecovery_AppliesAdditionalReputationCost()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.Torture);
        var initialRepCost = state.TotalReputationCost;

        // Act
        state.MarkSubjectBrokenBeyondRecovery(-20);

        // Assert
        state.Status.Should().Be(InterrogationStatus.Abandoned);
        state.TotalReputationCost.Should().Be(initialRepCost - 20);
    }

    /// <summary>
    /// Helper method to record a round with a specific method.
    /// </summary>
    private static void RecordRoundWithMethod(InterrogationState state, InterrogationMethod method)
    {
        var round = new InterrogationRound
        {
            RoundNumber = state.RoundNumber + 1,
            MethodUsed = method,
            CheckResult = SkillOutcome.FullSuccess,
            DiceRolled = 5,
            SuccessesAchieved = 3,
            SuccessesRequired = 3,
            ResistanceChange = -1,
            ResistanceAfter = Math.Max(0, state.ResistanceRemaining - 1),
            DispositionChange = method.GetDispositionChangePerRound(),
            ResourceCost = method.RequiresResources() ? 50 : 0,
            TimeElapsedMinutes = method.GetRoundDurationMinutes(),
            IsFumble = false,
            FumbleType = null,
            NarrativeDescription = $"Used {method.GetDisplayName()}"
        };

        state.RecordRound(round);
    }

    #endregion

    #region Test 5: InterrogationRound Value Object Tests

    /// <summary>
    /// Verifies that InterrogationRound.ForCheck creates correct instance.
    /// </summary>
    [Test]
    public void InterrogationRound_ForCheck_CreatesCorrectInstance()
    {
        // Act
        var round = InterrogationRound.ForCheck(
            roundNumber: 1,
            method: InterrogationMethod.GoodCop,
            outcome: SkillOutcome.FullSuccess,
            diceRolled: 8,
            successesAchieved: 4,
            successesRequired: 4,
            resistanceChange: -1,
            resistanceAfter: 3,
            narrative: "The subject opens up.");

        // Assert
        round.RoundNumber.Should().Be(1);
        round.MethodUsed.Should().Be(InterrogationMethod.GoodCop);
        round.CheckResult.Should().Be(SkillOutcome.FullSuccess);
        round.ResistanceChange.Should().Be(-1, because: "success reduces resistance by 1");
        round.IsFumble.Should().BeFalse();
        round.TimeElapsedMinutes.Should().Be(30, because: "GoodCop takes 30 minutes");
    }

    /// <summary>
    /// Verifies that InterrogationRound.ForFumble creates correct instance.
    /// </summary>
    [Test]
    public void InterrogationRound_ForFumble_CreatesCorrectInstance()
    {
        // Act
        var round = InterrogationRound.ForFumble(
            roundNumber: 2,
            method: InterrogationMethod.Torture,
            diceRolled: 5,
            resistanceAfter: 3,
            fumbleType: FumbleType.SubjectBroken,
            narrative: "The subject is broken beyond recovery.");

        // Assert
        round.RoundNumber.Should().Be(2);
        round.MethodUsed.Should().Be(InterrogationMethod.Torture);
        round.CheckResult.Should().Be(SkillOutcome.CriticalFailure);
        round.IsFumble.Should().BeTrue();
        round.FumbleType.Should().Be(FumbleType.SubjectBroken);
        round.DispositionChange.Should().Be(-40, because: "Torture fumble doubles the -20 disposition to -40");
    }

    /// <summary>
    /// Verifies that IsSuccess is calculated correctly.
    /// </summary>
    [Test]
    public void InterrogationRound_IsSuccess_CorrectlyCalculated()
    {
        // Arrange
        var successRound = InterrogationRound.ForCheck(
            roundNumber: 1, method: InterrogationMethod.GoodCop,
            outcome: SkillOutcome.FullSuccess,
            diceRolled: 8, successesAchieved: 4, successesRequired: 4,
            resistanceChange: -1, resistanceAfter: 3,
            narrative: "Success");

        var failureRound = InterrogationRound.ForCheck(
            roundNumber: 2, method: InterrogationMethod.GoodCop,
            outcome: SkillOutcome.Failure,
            diceRolled: 8, successesAchieved: 2, successesRequired: 4,
            resistanceChange: 0, resistanceAfter: 3,
            narrative: "Failure");

        // Assert
        successRound.IsSuccess.Should().BeTrue();
        failureRound.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Test 6: InterrogationContext Value Object Tests

    /// <summary>
    /// Verifies that InterrogationContext calculates effective DC correctly.
    /// </summary>
    [Test]
    public void InterrogationContext_CalculateEffectiveDc_Standard()
    {
        // Arrange
        var state = CreateTestState();
        var context = InterrogationContext.CreateMinimal(state, InterrogationMethod.GoodCop);

        // Act
        var effectiveDc = context.CalculateEffectiveDc();

        // Assert
        effectiveDc.Should().Be(14, because: "GoodCop base DC is 14");
    }

    /// <summary>
    /// Verifies that Torture DC is calculated as WILL×2.
    /// </summary>
    [Test]
    public void InterrogationContext_TortureDc_IsWillTimes2()
    {
        // Arrange
        var state = CreateTestState(subjectWill: 6);
        var context = InterrogationContext.CreateMinimal(state, InterrogationMethod.Torture);

        // Act
        var effectiveDc = context.CalculateEffectiveDc();

        // Assert
        effectiveDc.Should().Be(12, because: "Torture DC = WILL(6) × 2 = 12");
    }

    /// <summary>
    /// Verifies that Deception DC includes subject WITS.
    /// </summary>
    [Test]
    public void InterrogationContext_DeceptionDc_IncludesSubjectWits()
    {
        // Arrange
        var state = CreateTestState();
        var context = new InterrogationContext
        {
            BaseContext = SocialContext.CreateMinimal("test-subject", SocialInteractionType.Interrogation),
            InterrogationState = state,
            SelectedMethod = InterrogationMethod.Deception,
            InterrogatorAttribute = 5,
            InterrogatorRhetoric = 3,
            SubjectWits = 4,
            AvailableGold = 0,
            BonusDice = 0,
            DcModifier = 0,
            UseMight = false
        };

        // Act
        var effectiveDc = context.CalculateEffectiveDc();

        // Assert - Deception DC 16 + WITS 4 = 20
        effectiveDc.Should().Be(20,
            because: "Deception DC (16) + Subject WITS (4) = 20");
    }

    /// <summary>
    /// Verifies that dice pool is calculated correctly.
    /// </summary>
    [Test]
    public void InterrogationContext_CalculateDicePool_Standard()
    {
        // Arrange
        var state = CreateTestState();
        var context = new InterrogationContext
        {
            BaseContext = SocialContext.CreateMinimal("test-subject", SocialInteractionType.Interrogation),
            InterrogationState = state,
            SelectedMethod = InterrogationMethod.GoodCop,
            InterrogatorAttribute = 5,
            InterrogatorRhetoric = 3,
            SubjectWits = 4,
            AvailableGold = 0,
            BonusDice = 2,
            DcModifier = 0,
            UseMight = false
        };

        // Act
        var dicePool = context.CalculateDicePool();

        // Assert - Attribute(5) + Rhetoric(3) + Bonus(2) = 10
        dicePool.Should().Be(10);
    }

    /// <summary>
    /// Verifies that Torture uses raw attribute only (no skill).
    /// </summary>
    [Test]
    public void InterrogationContext_TortureDicePool_UsesRawAttributeOnly()
    {
        // Arrange
        var state = CreateTestState();
        var context = new InterrogationContext
        {
            BaseContext = SocialContext.CreateMinimal("test-subject", SocialInteractionType.Interrogation),
            InterrogationState = state,
            SelectedMethod = InterrogationMethod.Torture,
            InterrogatorAttribute = 5,
            InterrogatorRhetoric = 3, // Should be ignored for Torture
            SubjectWits = 4,
            AvailableGold = 0,
            BonusDice = 0,
            DcModifier = 0,
            UseMight = false
        };

        // Act
        var dicePool = context.CalculateDicePool();

        // Assert - Torture uses Attribute only, no Rhetoric
        dicePool.Should().Be(5, because: "Torture uses raw attribute without skill");
    }

    /// <summary>
    /// Verifies that CanAffordBribery returns correct result.
    /// </summary>
    [Test]
    public void InterrogationContext_CanAffordBribery_ChecksResistanceCost()
    {
        // Arrange
        var state = CreateTestState(
            subjectWill: 5,
            resistanceLevel: SubjectResistance.Moderate,
            initialResistance: 4);

        var lowGoldContext = new InterrogationContext
        {
            BaseContext = SocialContext.CreateMinimal("test-subject", SocialInteractionType.Interrogation),
            InterrogationState = state,
            SelectedMethod = InterrogationMethod.Bribery,
            InterrogatorAttribute = 5,
            InterrogatorRhetoric = 3,
            SubjectWits = 4,
            AvailableGold = 10, // Too low for Moderate (75)
            BonusDice = 0,
            DcModifier = 0,
            UseMight = false
        };

        var highGoldContext = lowGoldContext with { AvailableGold = 100 };

        // Assert
        lowGoldContext.CanAffordBribery().Should().BeFalse();
        highGoldContext.CanAffordBribery().Should().BeTrue();
    }

    #endregion

    #region Test 7: InterrogationResult Value Object Tests

    /// <summary>
    /// Verifies that success result has correct properties.
    /// </summary>
    [Test]
    public void InterrogationResult_Success_HasCorrectProperties()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);

        // Force status to SubjectBroken for the test
        // In real scenario, this would happen when resistance reaches 0
        var information = new List<InformationGained>
        {
            new()
            {
                Topic = "hideout",
                Content = "The hideout is in the mill.",
                ReliabilityPercent = 95,
                SourceMethod = InterrogationMethod.GoodCop,
                IsVerified = null,
                IsTrue = true
            }
        };

        // Act
        var result = InterrogationResult.Success(
            state: state,
            information: information,
            narrativeSummary: "The subject broke after 2 rounds.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.SubjectBroken.Should().BeTrue();
        result.HasInformation.Should().BeTrue();
        result.InformationGained.Should().HaveCount(1);
        result.InformationReliability.Should().Be(95);
        result.TortureWasUsed.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that failure result has correct properties.
    /// </summary>
    [Test]
    public void InterrogationResult_Failure_HasCorrectProperties()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        state.Abandon();

        // Act
        var result = InterrogationResult.Failure(
            state: state,
            finalStatus: InterrogationStatus.Abandoned,
            narrativeSummary: "The interrogation was abandoned.");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.SubjectBroken.Should().BeFalse();
        result.HasInformation.Should().BeFalse();
        result.InformationGained.Should().BeEmpty();
        result.InformationReliability.Should().Be(0);
    }

    /// <summary>
    /// Verifies that result with Torture shows reliability cap.
    /// </summary>
    [Test]
    public void InterrogationResult_WithTorture_ShowsReliabilityCap()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);
        RecordRoundWithMethod(state, InterrogationMethod.Torture);
        RecordRoundWithMethod(state, InterrogationMethod.GoodCop);

        var information = new List<InformationGained>
        {
            new()
            {
                Topic = "test",
                Content = "Test info",
                ReliabilityPercent = 60, // Capped due to Torture
                SourceMethod = InterrogationMethod.GoodCop,
                IsVerified = null,
                IsTrue = true
            }
        };

        // Act
        var result = InterrogationResult.Success(
            state: state,
            information: information,
            narrativeSummary: "The subject broke.");

        // Assert
        result.TortureWasUsed.Should().BeTrue();
        result.SubjectTraumatized.Should().BeTrue();
        result.InformationReliability.Should().BeLessOrEqualTo(60);
    }

    /// <summary>
    /// Verifies that HasSignificantCosts is calculated correctly.
    /// </summary>
    [Test]
    public void InterrogationResult_HasSignificantCosts_CalculatedCorrectly()
    {
        // Arrange
        var state = CreateTestState();
        RecordRoundWithMethod(state, InterrogationMethod.Torture); // -30 rep, -20 disposition

        var information = new List<InformationGained>();

        // Act
        var result = InterrogationResult.Success(
            state: state,
            information: information,
            narrativeSummary: "Test");

        // Assert
        result.HasSignificantCosts.Should().BeTrue(
            because: "Torture causes significant reputation and disposition damage");
    }

    #endregion

    #region Test 8: InformationGained Value Object Tests

    /// <summary>
    /// Verifies that InformationGained has correct reliability assessment text.
    /// </summary>
    /// <param name="reliability">The reliability percentage.</param>
    /// <param name="expectedContains">Expected substring in assessment.</param>
    [TestCase(95, "Likely accurate")]
    [TestCase(90, "Likely accurate")]
    [TestCase(80, "Probably accurate")]
    [TestCase(70, "Possibly accurate")]
    [TestCase(60, "Questionable")]
    [TestCase(50, "UNRELIABLE")]
    public void InformationGained_GetReliabilityAssessment_ReturnsCorrectText(
        int reliability,
        string expectedContains)
    {
        // Arrange
        var info = new InformationGained
        {
            Topic = "test",
            Content = "Test content",
            ReliabilityPercent = reliability,
            SourceMethod = InterrogationMethod.GoodCop,
            IsVerified = null,
            IsTrue = true
        };

        // Act
        var assessment = info.GetReliabilityAssessment();

        // Assert
        assessment.Should().ContainEquivalentOf(expectedContains);
    }

    /// <summary>
    /// Verifies that WithVerification creates a new instance with verification status.
    /// </summary>
    [Test]
    public void InformationGained_WithVerification_CreatesNewInstance()
    {
        // Arrange
        var info = new InformationGained
        {
            Topic = "test",
            Content = "Test content",
            ReliabilityPercent = 95,
            SourceMethod = InterrogationMethod.GoodCop,
            IsVerified = null,
            IsTrue = true
        };

        // Act
        var verified = info.WithVerification(true);

        // Assert
        verified.IsVerified.Should().BeTrue();
        info.IsVerified.Should().BeNull(because: "original should be unchanged");
    }

    #endregion

    #region Test 9: FumbleType.SubjectBroken Tests

    /// <summary>
    /// Verifies that SubjectBroken fumble type exists and has correct properties.
    /// </summary>
    [Test]
    public void FumbleType_SubjectBroken_ExistsAndIsCorrect()
    {
        // Arrange
        var fumbleType = FumbleType.SubjectBroken;

        // Assert
        fumbleType.GetDisplayName().Should().Be("Subject Broken");
        fumbleType.GetAssociatedSkillId().Should().Be("interrogation");
        fumbleType.IsInstant().Should().BeTrue();
        fumbleType.IsPermanent().Should().BeTrue();
        fumbleType.BlocksAllTargets().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SubjectBroken has a description.
    /// </summary>
    [Test]
    public void FumbleType_SubjectBroken_HasDescription()
    {
        // Act
        var description = FumbleType.SubjectBroken.GetDescription();

        // Assert
        description.Should().NotBeNullOrWhiteSpace();
        description.Should().ContainAny("broken", "torture", "dead", "insane");
    }

    #endregion
}
