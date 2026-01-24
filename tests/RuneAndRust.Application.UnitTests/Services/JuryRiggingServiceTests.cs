// ------------------------------------------------------------------------------
// <copyright file="JuryRiggingServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the JuryRiggingService, covering the five-step procedure
// (Observe, Probe, Pattern, Method Selection, Experiment, Iterate),
// bypass methods, familiarity bonuses, and complication handling.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Domain.Constants;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="JuryRiggingService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the following areas:
/// <list type="bullet">
///   <item><description>Bypass method DC modifiers for all 6 methods</description></item>
///   <item><description>Familiarity bonus (+2d10 dice for known mechanism types)</description></item>
///   <item><description>Complication table (d10 roll to ComplicationEffect mapping)</description></item>
///   <item><description>Method validation (MemorizedSequence requires familiarity, GlitchExploitation requires [Glitched])</description></item>
///   <item><description>DC reduction through iteration (DC -1 per attempt, minimum 4)</description></item>
///   <item><description>Electrocution risk for Wire Manipulation</description></item>
///   <item><description>Critical success salvage (net >= 5 yields components)</description></item>
///   <item><description>Fumble detection (0 successes + botch destroys mechanism)</description></item>
///   <item><description>State transitions through procedure steps</description></item>
///   <item><description>Null argument validation (guard clauses)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class JuryRiggingServiceTests
{
    // -------------------------------------------------------------------------
    // Test Dependencies
    // -------------------------------------------------------------------------

    private IDiceService _diceService = null!;
    private ILogger<JuryRiggingService> _logger = null!;
    private JuryRiggingService _service = null!;

    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const string TestCharacterId = "test-character-001";
    private const string TestMechanismType = "terminal";
    private const string TestMechanismName = "Old World Terminal";
    private const int DefaultBaseDc = 12;

    // -------------------------------------------------------------------------
    // Setup and Teardown
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<JuryRiggingService>>();
        _diceService = Substitute.For<IDiceService>();

        // Create service under test
        _service = new JuryRiggingService(_diceService, _logger);
    }

    // -------------------------------------------------------------------------
    // Mock Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Configures the dice service mock to return a result with specified net successes.
    /// </summary>
    private void SetupDiceRoll(int netSuccesses, bool isFumble = false)
    {
        var successes = Math.Max(0, netSuccesses);
        var botches = isFumble ? 1 : 0;
        var rolls = CreateRollsForNetSuccesses(successes, botches);
        var pool = DicePool.D10(rolls.Count);

        var result = new DiceRollResult(pool, rolls);
        _diceService.Roll(Arg.Any<DicePool>(), Arg.Any<AdvantageType>()).Returns(result);
    }

    /// <summary>
    /// Configures the dice service mock to return a single die result.
    /// </summary>
    private void SetupSingleDieRoll(int value)
    {
        var pool = DicePool.D10(1);
        var result = new DiceRollResult(pool, new[] { value });
        _diceService.Roll(Arg.Any<DiceType>(), Arg.Any<int>(), Arg.Any<int>()).Returns(result);
    }

    /// <summary>
    /// Creates a list of rolls that would produce the specified successes and botches.
    /// </summary>
    private static List<int> CreateRollsForNetSuccesses(int successes, int botches)
    {
        var rolls = new List<int>();

        // Add successes (8, 9, or 10)
        for (var i = 0; i < successes; i++)
        {
            rolls.Add(8 + (i % 3));
        }

        // Add botches (1)
        for (var i = 0; i < botches; i++)
        {
            rolls.Add(1);
        }

        // Pad with neutral values if needed
        if (rolls.Count == 0)
        {
            rolls.Add(5); // Neutral die
        }

        return rolls;
    }

    // -------------------------------------------------------------------------
    // Bypass Method DC Modifier Tests (Parameterized)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Verifies that each bypass method has the correct DC modifier.
    /// </summary>
    /// <param name="method">The bypass method to test.</param>
    /// <param name="expectedModifier">The expected DC modifier.</param>
    [TestCase(BypassMethod.PercussiveMaintenance, 0)]
    [TestCase(BypassMethod.WireManipulation, -2)]
    [TestCase(BypassMethod.GlitchExploitation, -4)]
    [TestCase(BypassMethod.MemorizedSequence, -2)]
    [TestCase(BypassMethod.BruteDisassembly, 2)]
    [TestCase(BypassMethod.PowerCycling, 0)]
    public void BypassMethod_HasCorrectDcModifier(BypassMethod method, int expectedModifier)
    {
        // Act
        var modifier = method.GetDcModifier();

        // Assert
        modifier.Should().Be(expectedModifier);
    }

    /// <summary>
    /// Verifies that bypass method DC modifier display strings are correctly formatted.
    /// </summary>
    /// <param name="method">The bypass method to test.</param>
    /// <param name="expectedDisplay">The expected display string.</param>
    [TestCase(BypassMethod.PercussiveMaintenance, "+0 DC")]
    [TestCase(BypassMethod.WireManipulation, "-2 DC")]
    [TestCase(BypassMethod.GlitchExploitation, "-4 DC")]
    [TestCase(BypassMethod.MemorizedSequence, "-2 DC")]
    [TestCase(BypassMethod.BruteDisassembly, "+2 DC")]
    [TestCase(BypassMethod.PowerCycling, "+0 DC")]
    public void BypassMethod_HasCorrectDcModifierDisplay(BypassMethod method, string expectedDisplay)
    {
        // Act
        var display = method.GetDcModifierDisplay();

        // Assert
        display.Should().Be(expectedDisplay);
    }

    // -------------------------------------------------------------------------
    // Session Initialization Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that InitiateJuryRig creates a valid initial state.
    /// </summary>
    [Test]
    public void InitiateJuryRig_CreatesValidInitialState()
    {
        // Act
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Assert
        state.Should().NotBeNull();
        state.CharacterId.Should().Be(TestCharacterId);
        state.MechanismType.Should().Be(TestMechanismType);
        state.MechanismName.Should().Be(TestMechanismName);
        state.BaseDc.Should().Be(DefaultBaseDc);
        state.IsGlitched.Should().BeFalse();
        state.CurrentStep.Should().Be(JuryRigStep.Observe);
        state.Status.Should().Be(JuryRigStatus.InProgress);
        state.IterationCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that InitiateJuryRig correctly tracks familiarity for known mechanism types.
    /// </summary>
    [Test]
    public void InitiateJuryRig_WithKnownMechanismTypes_TracksCorrectly()
    {
        // Arrange
        var knownTypes = new[] { "terminal", "door-lock", "security-panel" };

        // Act
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType, // "terminal" - should be familiar
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false,
            knownMechanismTypes: knownTypes);

        // Assert
        state.IsFamiliarMechanism.Should().BeTrue();
    }

    /// <summary>
    /// Tests that InitiateJuryRig correctly identifies unfamiliar mechanism types.
    /// </summary>
    [Test]
    public void InitiateJuryRig_WithUnknownMechanismType_IsNotFamiliar()
    {
        // Arrange
        var knownTypes = new[] { "door-lock", "security-panel" };

        // Act
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType, // "terminal" - not in known types
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false,
            knownMechanismTypes: knownTypes);

        // Assert
        state.IsFamiliarMechanism.Should().BeFalse();
    }

    /// <summary>
    /// Tests that InitiateJuryRig throws on null character ID.
    /// </summary>
    [Test]
    public void InitiateJuryRig_NullCharacterId_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.InitiateJuryRig(
            null!,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that InitiateJuryRig throws on empty mechanism type.
    /// </summary>
    [Test]
    public void InitiateJuryRig_EmptyMechanismType_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.InitiateJuryRig(
            TestCharacterId,
            string.Empty,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // -------------------------------------------------------------------------
    // Observation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that successful observation reveals mechanism hints.
    /// </summary>
    [Test]
    public void PerformObservation_Success_RevealsHints()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Mock successful dice roll (3 successes, DC is 10, so net success = 3)
        SetupDiceRoll(netSuccesses: 3);

        // Act
        var result = _service.PerformObservation(state, witsScore: 10);

        // Assert
        result.Success.Should().BeTrue();
        result.WasSkipped.Should().BeFalse();
        result.Hints.Should().NotBeEmpty();
        state.ObservationComplete.Should().BeTrue();
        state.CurrentStep.Should().Be(JuryRigStep.Probe);
    }

    /// <summary>
    /// Tests that failed observation does not reveal hints.
    /// </summary>
    [Test]
    public void PerformObservation_Failure_NoHints()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Mock failed dice roll (0 net successes, no fumble)
        SetupDiceRoll(netSuccesses: 0, isFumble: false);

        // Act
        var result = _service.PerformObservation(state, witsScore: 5);

        // Assert
        result.Success.Should().BeFalse();
        result.Hints.Should().BeEmpty();
        state.ObservationComplete.Should().BeTrue();
        state.CurrentStep.Should().Be(JuryRigStep.Probe);
    }

    /// <summary>
    /// Tests that skipping observation advances state correctly.
    /// </summary>
    [Test]
    public void SkipObservation_AdvancesToProbe()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Act
        var result = _service.SkipObservation(state);

        // Assert
        result.WasSkipped.Should().BeTrue();
        result.Success.Should().BeFalse();
        result.Hints.Should().BeEmpty();
        state.ObservationComplete.Should().BeTrue();
        state.CurrentStep.Should().Be(JuryRigStep.Probe);
    }

    /// <summary>
    /// Tests that observation throws when not at Observe step.
    /// </summary>
    [Test]
    public void PerformObservation_WrongStep_ThrowsInvalidOperationException()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);
        _service.SkipObservation(state); // Move to Probe

        // Act
        var act = () => _service.PerformObservation(state, witsScore: 10);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // Probe Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that probe returns reactions for the mechanism type.
    /// </summary>
    [Test]
    public void PerformProbe_ReturnsReactions()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);
        _service.SkipObservation(state);

        // Act
        var result = _service.PerformProbe(state);

        // Assert
        result.Reactions.Should().NotBeNull();
        result.IsPowered.Should().BeTrue();
        result.IsGlitched.Should().BeFalse();
        state.ProbingComplete.Should().BeTrue();
        state.CurrentStep.Should().Be(JuryRigStep.Pattern);
    }

    /// <summary>
    /// Tests that probe on glitched mechanism indicates glitched state.
    /// </summary>
    [Test]
    public void PerformProbe_GlitchedMechanism_IndicatesGlitched()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: true);
        _service.SkipObservation(state);

        // Act
        var result = _service.PerformProbe(state);

        // Assert
        result.IsGlitched.Should().BeTrue();
        result.CanExploitGlitch.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Pattern Recognition Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that successful pattern recognition on familiar mechanism grants bonus dice.
    /// </summary>
    [Test]
    public void AttemptPatternRecognition_SuccessWithFamiliarity_GrantsBonusDice()
    {
        // Arrange
        var knownTypes = new[] { TestMechanismType };
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false,
            knownMechanismTypes: knownTypes);
        _service.SkipObservation(state);
        _service.PerformProbe(state);

        // Mock successful dice roll
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.AttemptPatternRecognition(state, witsScore: 12);

        // Assert
        result.Success.Should().BeTrue();
        result.IsFamiliar.Should().BeTrue();
        result.BonusDice.Should().Be(2); // Familiarity bonus
        result.HasBonusDice.Should().BeTrue();
        state.PatternRecognized.Should().BeTrue();
    }

    /// <summary>
    /// Tests that successful pattern recognition on unfamiliar mechanism marks it as seen.
    /// </summary>
    [Test]
    public void AttemptPatternRecognition_SuccessWithoutFamiliarity_MarksAsSeen()
    {
        // Arrange - no known types
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);
        _service.SkipObservation(state);
        _service.PerformProbe(state);

        // Mock successful dice roll
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.AttemptPatternRecognition(state, witsScore: 12);

        // Assert
        result.Success.Should().BeTrue();
        result.IsFamiliar.Should().BeFalse();
        result.BonusDice.Should().Be(0); // No bonus without familiarity
        result.IsNewType.Should().BeTrue();
    }

    /// <summary>
    /// Tests that skipping pattern recognition advances state correctly.
    /// </summary>
    [Test]
    public void SkipPatternRecognition_AdvancesToMethodSelection()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);
        _service.SkipObservation(state);
        _service.PerformProbe(state);

        // Act
        var result = _service.SkipPatternRecognition(state);

        // Assert
        result.WasSkipped.Should().BeTrue();
        result.BonusDice.Should().Be(0);
        state.CurrentStep.Should().Be(JuryRigStep.MethodSelection);
    }

    // -------------------------------------------------------------------------
    // Method Selection Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetAvailableMethods returns all 6 bypass methods.
    /// </summary>
    [Test]
    public void GetAvailableMethods_ReturnsAllMethods()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act
        var methods = _service.GetAvailableMethods(state);

        // Assert
        methods.Should().HaveCount(6);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.PercussiveMaintenance);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.WireManipulation);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.GlitchExploitation);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.MemorizedSequence);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.BruteDisassembly);
        methods.Select(m => m.Method).Should().Contain(BypassMethod.PowerCycling);
    }

    /// <summary>
    /// Tests that MemorizedSequence is unavailable without familiarity.
    /// </summary>
    [Test]
    public void GetAvailableMethods_WithoutFamiliarity_MemorizedSequenceUnavailable()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act
        var methods = _service.GetAvailableMethods(state);

        // Assert
        var memorized = methods.First(m => m.Method == BypassMethod.MemorizedSequence);
        memorized.IsAvailable.Should().BeFalse();
        memorized.UnavailableReason.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that MemorizedSequence is available with familiarity.
    /// </summary>
    [Test]
    public void GetAvailableMethods_WithFamiliarity_MemorizedSequenceAvailable()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: true);

        // Act
        var methods = _service.GetAvailableMethods(state);

        // Assert
        var memorized = methods.First(m => m.Method == BypassMethod.MemorizedSequence);
        memorized.IsAvailable.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GlitchExploitation is unavailable on non-glitched mechanism.
    /// </summary>
    [Test]
    public void GetAvailableMethods_NonGlitched_GlitchExploitationUnavailable()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act
        var methods = _service.GetAvailableMethods(state);

        // Assert
        var glitch = methods.First(m => m.Method == BypassMethod.GlitchExploitation);
        glitch.IsAvailable.Should().BeFalse();
        glitch.UnavailableReason.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that GlitchExploitation is available on glitched mechanism.
    /// </summary>
    [Test]
    public void GetAvailableMethods_Glitched_GlitchExploitationAvailable()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: true, isFamiliar: false);

        // Act
        var methods = _service.GetAvailableMethods(state);

        // Assert
        var glitch = methods.First(m => m.Method == BypassMethod.GlitchExploitation);
        glitch.IsAvailable.Should().BeTrue();
    }

    /// <summary>
    /// Tests that SelectMethod throws for invalid method selection.
    /// </summary>
    [Test]
    public void SelectMethod_InvalidMethod_ThrowsArgumentException()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act - try to select MemorizedSequence without familiarity
        var act = () => _service.SelectMethod(state, BypassMethod.MemorizedSequence);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that SelectMethod updates state correctly for valid method.
    /// </summary>
    [Test]
    public void SelectMethod_ValidMethod_UpdatesState()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act
        _service.SelectMethod(state, BypassMethod.PercussiveMaintenance);

        // Assert
        state.SelectedMethod.Should().Be(BypassMethod.PercussiveMaintenance);
        state.CurrentStep.Should().Be(JuryRigStep.Experiment);
    }

    // -------------------------------------------------------------------------
    // Complication Table Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that complication roll 1 produces PermanentLock effect.
    /// </summary>
    [Test]
    public void ProcessComplication_Roll1_ReturnsPermanentLock()
    {
        // Act
        var effect = _service.ProcessComplication(1);

        // Assert
        effect.Should().Be(ComplicationEffect.PermanentLock);
    }

    /// <summary>
    /// Tests that complication roll 2-3 produces AlarmTriggered effect.
    /// </summary>
    /// <param name="roll">The d10 roll.</param>
    [TestCase(2)]
    [TestCase(3)]
    public void ProcessComplication_Roll2Or3_ReturnsAlarmTriggered(int roll)
    {
        // Act
        var effect = _service.ProcessComplication(roll);

        // Assert
        effect.Should().Be(ComplicationEffect.AlarmTriggered);
    }

    /// <summary>
    /// Tests that complication roll 4-5 produces SparksFly effect.
    /// </summary>
    /// <param name="roll">The d10 roll.</param>
    [TestCase(4)]
    [TestCase(5)]
    public void ProcessComplication_Roll4Or5_ReturnsSparksFly(int roll)
    {
        // Act
        var effect = _service.ProcessComplication(roll);

        // Assert
        effect.Should().Be(ComplicationEffect.SparksFly);
    }

    /// <summary>
    /// Tests that complication roll 6-7 produces Nothing effect.
    /// </summary>
    /// <param name="roll">The d10 roll.</param>
    [TestCase(6)]
    [TestCase(7)]
    public void ProcessComplication_Roll6Or7_ReturnsNothing(int roll)
    {
        // Act
        var effect = _service.ProcessComplication(roll);

        // Assert
        effect.Should().Be(ComplicationEffect.Nothing);
    }

    /// <summary>
    /// Tests that complication roll 8-9 produces PartialSuccess effect.
    /// </summary>
    /// <param name="roll">The d10 roll.</param>
    [TestCase(8)]
    [TestCase(9)]
    public void ProcessComplication_Roll8Or9_ReturnsPartialSuccess(int roll)
    {
        // Act
        var effect = _service.ProcessComplication(roll);

        // Assert
        effect.Should().Be(ComplicationEffect.PartialSuccess);
    }

    /// <summary>
    /// Tests that complication roll 10 produces GlitchInFavor effect.
    /// </summary>
    [Test]
    public void ProcessComplication_Roll10_ReturnsGlitchInFavor()
    {
        // Act
        var effect = _service.ProcessComplication(10);

        // Assert
        effect.Should().Be(ComplicationEffect.GlitchInFavor);
    }

    /// <summary>
    /// Tests that complication roll outside 1-10 throws exception.
    /// </summary>
    /// <param name="roll">The invalid roll value.</param>
    [TestCase(0)]
    [TestCase(11)]
    [TestCase(-1)]
    public void ProcessComplication_InvalidRoll_ThrowsArgumentOutOfRangeException(int roll)
    {
        // Act
        var act = () => _service.ProcessComplication(roll);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // Experiment Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that successful experiment bypasses the mechanism.
    /// </summary>
    [Test]
    public void PerformExperiment_Success_BypassesMechanism()
    {
        // Arrange
        var state = CreateStateAtExperiment();
        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);

        // Mock successful dice roll (net successes > 0)
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.PerformExperiment(state, context, systemBypassScore: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(JuryRigOutcome.Success);
        state.Status.Should().Be(JuryRigStatus.Bypassed);
    }

    /// <summary>
    /// Tests that critical success (net >= 5) yields salvage.
    /// </summary>
    [Test]
    public void PerformExperiment_CriticalSuccess_YieldsSalvage()
    {
        // Arrange
        var state = CreateStateAtExperiment();
        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);

        // Mock critical success dice roll (net successes >= 5)
        SetupDiceRoll(netSuccesses: 5);

        // Act
        var result = _service.PerformExperiment(state, context, systemBypassScore: 15);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Outcome.Should().Be(JuryRigOutcome.CriticalSuccess);
        result.HasSalvage.Should().BeTrue();
        result.SalvagedComponents.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that failure triggers complication roll.
    /// </summary>
    [Test]
    public void PerformExperiment_Failure_TriggersComplication()
    {
        // Arrange
        var state = CreateStateAtExperiment();
        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);

        // Mock failed dice roll (net successes = 0, not fumble)
        SetupDiceRoll(netSuccesses: 0, isFumble: false);
        // Mock complication roll (6 = Nothing)
        SetupSingleDieRoll(6);

        // Act
        var result = _service.PerformExperiment(state, context, systemBypassScore: 5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(JuryRigOutcome.Failure);
        result.ComplicationEffect.Should().Be(ComplicationEffect.Nothing);
        result.CanRetry.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Brute Disassembly success destroys mechanism but yields salvage.
    /// </summary>
    [Test]
    public void PerformExperiment_BruteDisassemblySuccess_DestroysMechanismWithSalvage()
    {
        // Arrange
        var state = CreateStateAtExperiment(selectedMethod: BypassMethod.BruteDisassembly);
        var context = JuryRigContext.Create(
            BypassMethod.BruteDisassembly,
            isFamiliar: false,
            ToolQuality.Proper);

        // Mock successful dice roll
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.PerformExperiment(state, context, systemBypassScore: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.MechanismDestroyed.Should().BeTrue();
        result.HasSalvage.Should().BeTrue();
        state.Status.Should().Be(JuryRigStatus.Destroyed);
    }

    // -------------------------------------------------------------------------
    // Electrocution Risk Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that WireManipulation has electrocution risk flag.
    /// </summary>
    [Test]
    public void WireManipulation_HasElectrocutionRisk()
    {
        // Act
        var hasRisk = BypassMethod.WireManipulation.HasElectrocutionRisk();

        // Assert
        hasRisk.Should().BeTrue();
    }

    /// <summary>
    /// Tests that other methods do not have electrocution risk.
    /// </summary>
    /// <param name="method">The bypass method to test.</param>
    [TestCase(BypassMethod.PercussiveMaintenance)]
    [TestCase(BypassMethod.GlitchExploitation)]
    [TestCase(BypassMethod.MemorizedSequence)]
    [TestCase(BypassMethod.BruteDisassembly)]
    [TestCase(BypassMethod.PowerCycling)]
    public void OtherMethods_NoElectrocutionRisk(BypassMethod method)
    {
        // Act
        var hasRisk = method.HasElectrocutionRisk();

        // Assert
        hasRisk.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Iteration Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ApplyIteration reduces DC by 1.
    /// </summary>
    [Test]
    public void ApplyIteration_ReducesDcBy1()
    {
        // Arrange
        var state = CreateStateAtIterate();
        var initialDc = state.GetModifiedDc();

        // Act
        _service.ApplyIteration(state);

        // Assert
        state.IterationCount.Should().Be(1);
        state.GetModifiedDc().Should().Be(initialDc - 1);
        state.CurrentStep.Should().Be(JuryRigStep.MethodSelection);
    }

    /// <summary>
    /// Tests that DC never drops below minimum of 4.
    /// </summary>
    [Test]
    public void ApplyIteration_RespectMinimumDc()
    {
        // Arrange - create state with low base DC
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            baseDc: 5, // Very low DC
            isGlitched: false);

        // Progress through initial steps to MethodSelection
        _service.SkipObservation(state);
        _service.PerformProbe(state);
        _service.SkipPatternRecognition(state);

        // First iteration - progress through experiment and iterate
        ProgressToIterate(state);
        _service.ApplyIteration(state);

        // Second iteration
        ProgressToIterate(state);
        _service.ApplyIteration(state);

        // Third iteration
        ProgressToIterate(state);
        _service.ApplyIteration(state);

        // Assert - DC should not go below 4
        state.IterationCount.Should().Be(3);
        state.GetModifiedDc().Should().BeGreaterThanOrEqualTo(4);
    }

    // -------------------------------------------------------------------------
    // State Transition Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests full successful workflow through all steps.
    /// </summary>
    [Test]
    public void FullWorkflow_Success_TransitionsThroughAllSteps()
    {
        // Arrange
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched: false);

        // Mock dice rolls
        SetupDiceRoll(netSuccesses: 2);

        // Step 1: Observe
        state.CurrentStep.Should().Be(JuryRigStep.Observe);
        _service.SkipObservation(state);

        // Step 2: Probe
        state.CurrentStep.Should().Be(JuryRigStep.Probe);
        _service.PerformProbe(state);

        // Step 3: Pattern
        state.CurrentStep.Should().Be(JuryRigStep.Pattern);
        _service.SkipPatternRecognition(state);

        // Step 4: Method Selection
        state.CurrentStep.Should().Be(JuryRigStep.MethodSelection);
        _service.SelectMethod(state, BypassMethod.PercussiveMaintenance);

        // Step 5: Experiment
        state.CurrentStep.Should().Be(JuryRigStep.Experiment);
        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);
        var result = _service.PerformExperiment(state, context, systemBypassScore: 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        state.Status.Should().Be(JuryRigStatus.Bypassed);
    }

    /// <summary>
    /// Tests that abandoned session has correct status.
    /// </summary>
    [Test]
    public void AbandonSession_SetsAbandonedStatus()
    {
        // Arrange
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);

        // Act
        _service.AbandonSession(state);

        // Assert
        state.Status.Should().Be(JuryRigStatus.Abandoned);
    }

    // -------------------------------------------------------------------------
    // Value Object Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests JuryRigContext computed properties.
    /// </summary>
    [Test]
    public void JuryRigContext_ComputedPropertiesCorrect()
    {
        // Arrange
        var context = JuryRigContext.Create(
            BypassMethod.WireManipulation, // -2 DC
            isFamiliar: true,              // +2 bonus dice
            ToolQuality.Masterwork);       // +2 tool modifier

        // Assert
        context.MethodDcModifier.Should().Be(-2);
        context.EarnedFamiliarityDice.Should().Be(2);
        context.ToolDiceModifier.Should().Be(2);
        context.ElectrocutionRisk.Should().BeTrue();
    }

    /// <summary>
    /// Tests MethodOption factory methods.
    /// </summary>
    [Test]
    public void MethodOption_FactoryMethods_CreateCorrectly()
    {
        // Available
        var available = MethodOption.Available(BypassMethod.PercussiveMaintenance);
        available.IsAvailable.Should().BeTrue();
        available.UnavailableReason.Should().BeNull();

        // Unavailable
        var unavailable = MethodOption.Unavailable(
            BypassMethod.MemorizedSequence,
            "Requires familiarity");
        unavailable.IsAvailable.Should().BeFalse();
        unavailable.UnavailableReason.Should().Be("Requires familiarity");
    }

    /// <summary>
    /// Tests JuryRigResult factory methods.
    /// </summary>
    [Test]
    public void JuryRigResult_FactoryMethods_CreateCorrectly()
    {
        // Success
        var success = JuryRigResult.Success(
            netSuccesses: 3,
            dc: DefaultBaseDc,
            method: BypassMethod.PercussiveMaintenance);
        success.IsSuccess.Should().BeTrue();
        success.Outcome.Should().Be(JuryRigOutcome.Success);
        success.CanRetry.Should().BeFalse();

        // CriticalSuccess
        var critical = JuryRigResult.CriticalSuccess(
            netSuccesses: 6,
            dc: DefaultBaseDc,
            method: BypassMethod.PercussiveMaintenance,
            salvage: new[] { "circuit-board" });
        critical.Outcome.Should().Be(JuryRigOutcome.CriticalSuccess);
        critical.HasSalvage.Should().BeTrue();

        // Failure
        var failure = JuryRigResult.Failure(
            netSuccesses: 0,
            dc: DefaultBaseDc,
            method: BypassMethod.PercussiveMaintenance,
            complicationRoll: 6,
            effect: ComplicationEffect.Nothing,
            damage: 0,
            alert: false,
            narrative: "The mechanism resists your efforts.");
        failure.IsSuccess.Should().BeFalse();
        failure.CanRetry.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Salvage Component Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetSalvageableComponents returns components for known types.
    /// </summary>
    [Test]
    public void GetSalvageableComponents_KnownType_ReturnsComponents()
    {
        // Act
        var components = _service.GetSalvageableComponents("terminal");

        // Assert
        components.Should().NotBeEmpty();
        components.Should().Contain("circuit-board");
    }

    /// <summary>
    /// Tests that GetSalvageableComponents returns default fallback components for unknown types.
    /// </summary>
    /// <remarks>
    /// Even unknown Old World technology can yield some salvage, hence the fallback.
    /// </remarks>
    [Test]
    public void GetSalvageableComponents_UnknownType_ReturnsFallbackComponents()
    {
        // Act
        var components = _service.GetSalvageableComponents("unknown-type-xyz");

        // Assert - default fallback components are returned
        components.Should().NotBeEmpty();
        components.Should().Contain("unknown-component");
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a state that has progressed to the MethodSelection step.
    /// </summary>
    private JuryRigState CreateStateAtMethodSelection(bool isGlitched, bool isFamiliar)
    {
        var knownTypes = isFamiliar ? new[] { TestMechanismType } : null;
        var state = _service.InitiateJuryRig(
            TestCharacterId,
            TestMechanismType,
            TestMechanismName,
            DefaultBaseDc,
            isGlitched,
            knownTypes);

        _service.SkipObservation(state);
        _service.PerformProbe(state);
        _service.SkipPatternRecognition(state);

        return state;
    }

    /// <summary>
    /// Creates a state that has progressed to the Experiment step.
    /// </summary>
    private JuryRigState CreateStateAtExperiment(BypassMethod selectedMethod = BypassMethod.PercussiveMaintenance)
    {
        var state = CreateStateAtMethodSelection(isGlitched: false, isFamiliar: false);
        _service.SelectMethod(state, selectedMethod);
        return state;
    }

    /// <summary>
    /// Creates a state that has progressed to the Iterate step.
    /// </summary>
    private JuryRigState CreateStateAtIterate()
    {
        var state = CreateStateAtExperiment();

        // Mock failed experiment to reach iterate
        SetupDiceRoll(netSuccesses: 0, isFumble: false);
        SetupSingleDieRoll(6); // Nothing complication

        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);
        _service.PerformExperiment(state, context, systemBypassScore: 5);

        return state;
    }

    /// <summary>
    /// Progresses a state from MethodSelection through Experiment to Iterate.
    /// </summary>
    private void ProgressToIterate(JuryRigState state)
    {
        if (state.CurrentStep != JuryRigStep.MethodSelection)
        {
            return;
        }

        _service.SelectMethod(state, BypassMethod.PercussiveMaintenance);

        SetupDiceRoll(netSuccesses: 0, isFumble: false);
        SetupSingleDieRoll(6); // Nothing complication

        var context = JuryRigContext.Create(
            BypassMethod.PercussiveMaintenance,
            isFamiliar: false,
            ToolQuality.Proper);
        _service.PerformExperiment(state, context, systemBypassScore: 5);
    }
}
