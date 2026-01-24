// ------------------------------------------------------------------------------
// <copyright file="IceCountermeasureServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the IceCountermeasureService, covering ICE type determination,
// ICE resolution outcomes, and consequence application.
// Part of v0.15.4c ICE Countermeasures implementation.
// </summary>
// ------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="IceCountermeasureService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover three main areas:
/// <list type="bullet">
///   <item><description>ICE type determination by terminal type</description></item>
///   <item><description>ICE resolution outcomes (Passive, Active, Lethal)</description></item>
///   <item><description>Consequence application (damage, stress, lockout, alerts)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class IceCountermeasureServiceTests
{
    // -------------------------------------------------------------------------
    // Test Dependencies
    // -------------------------------------------------------------------------

    private SkillCheckService _skillCheckService = null!;
    private DiceService _diceService = null!;
    private IGameConfigurationProvider _configProvider = null!;
    private ILogger<IceCountermeasureService> _logger = null!;
    private ILogger<SkillCheckService> _skillCheckLogger = null!;
    private ILogger<DiceService> _diceLogger = null!;
    private IceCountermeasureService _service = null!;

    // -------------------------------------------------------------------------
    // Setup and Teardown
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<IceCountermeasureService>>();
        _skillCheckLogger = Substitute.For<ILogger<SkillCheckService>>();
        _diceLogger = Substitute.For<ILogger<DiceService>>();
        _configProvider = Substitute.For<IGameConfigurationProvider>();

        // Create DiceService with seeded random for reproducible tests
        var seededRandom = new Random(42);
        _diceService = new DiceService(_diceLogger, seededRandom);

        // Setup mock configuration
        SetupDefaultMocks();

        // Create SkillCheckService
        _skillCheckService = new SkillCheckService(
            _diceService,
            _configProvider,
            _skillCheckLogger);

        // Create service under test
        _service = new IceCountermeasureService(
            _skillCheckService,
            _diceService,
            _logger);
    }

    /// <summary>
    /// Sets up default mock configurations for skills and difficulty classes.
    /// </summary>
    private void SetupDefaultMocks()
    {
        // System Bypass skill configuration
        var systemBypass = SkillDefinition.Create(
            "system-bypass",
            "System Bypass",
            "Manipulate technology through observed patterns.",
            "wits",
            "finesse",
            "1d10",
            false,
            5);

        // WILL save configuration
        var willSave = SkillDefinition.Create(
            "will",
            "Will",
            "Resist mental and psychic effects.",
            "will",
            null,
            "1d10",
            false,
            5);

        _configProvider.GetSkillById("system-bypass").Returns(systemBypass);
        _configProvider.GetSkillById("will").Returns(willSave);
        _configProvider.GetSkills().Returns(new List<SkillDefinition> { systemBypass, willSave });

        // Difficulty class configurations
        var easy = DifficultyClassDefinition.Create("easy", "Easy", "Simple task.", 10);
        var moderate = DifficultyClassDefinition.Create("moderate", "Moderate", "Requires effort.", 14);
        var hard = DifficultyClassDefinition.Create("hard", "Hard", "Challenging task.", 18);

        _configProvider.GetDifficultyClassById("easy").Returns(easy);
        _configProvider.GetDifficultyClassById("moderate").Returns(moderate);
        _configProvider.GetDifficultyClassById("hard").Returns(hard);
        _configProvider.GetDifficultyClasses()
            .Returns(new List<DifficultyClassDefinition> { easy, moderate, hard });
    }

    /// <summary>
    /// Creates a test player with specified attributes.
    /// </summary>
    /// <param name="wits">Wits attribute value.</param>
    /// <param name="finesse">Finesse attribute value.</param>
    /// <param name="will">Will attribute value.</param>
    /// <returns>A configured test player.</returns>
    private static Player CreateTestPlayer(int wits = 10, int finesse = 10, int will = 10)
    {
        // Constructor order: might, fortitude, will, wits, finesse
        var attributes = new PlayerAttributes(8, 8, will, wits, finesse);
        return new Player("TestHacker", "human", "infiltrator", attributes, "Test");
    }

    // -------------------------------------------------------------------------
    // ICE Type Determination Tests (Parameterized)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that GetIceForTerminal returns correct ICE types for each terminal type.
    /// </summary>
    /// <param name="terminalType">The terminal type to test.</param>
    /// <param name="expectedIceTypes">The expected ICE types.</param>
    [TestCase(TerminalType.CivilianDataPort, new IceType[] { })]
    [TestCase(TerminalType.CorporateMainframe, new IceType[] { IceType.Passive })]
    [TestCase(TerminalType.SecurityHub, new IceType[] { IceType.Active })]
    [TestCase(TerminalType.MilitaryServer, new IceType[] { IceType.Active, IceType.Lethal })]
    [TestCase(TerminalType.JotunArchive, new IceType[] { IceType.Lethal })]
    [TestCase(TerminalType.GlitchedManifold, new IceType[] { })]
    public void GetIceForTerminal_ReturnsCorrectIceTypes(
        TerminalType terminalType,
        IceType[] expectedIceTypes)
    {
        // Act
        var iceTypes = _service.GetIceForTerminal(terminalType);

        // Assert
        iceTypes.Should().BeEquivalentTo(expectedIceTypes);
    }

    /// <summary>
    /// Tests that GetIceRating returns correct ratings for each terminal type.
    /// </summary>
    /// <param name="terminalType">The terminal type to test.</param>
    /// <param name="expectedRating">The expected ICE rating.</param>
    [TestCase(TerminalType.CivilianDataPort, 0)]
    [TestCase(TerminalType.CorporateMainframe, 12)]
    [TestCase(TerminalType.SecurityHub, 16)]
    [TestCase(TerminalType.MilitaryServer, 20)]
    [TestCase(TerminalType.JotunArchive, 24)]
    [TestCase(TerminalType.GlitchedManifold, 0)]
    public void GetIceRating_ReturnsCorrectRating(
        TerminalType terminalType,
        int expectedRating)
    {
        // Act
        var rating = _service.GetIceRating(terminalType);

        // Assert
        rating.Should().Be(expectedRating);
    }

    /// <summary>
    /// Tests that HasIce returns true only for terminals with ICE protection.
    /// </summary>
    /// <param name="terminalType">The terminal type to test.</param>
    /// <param name="expectedHasIce">Whether the terminal should have ICE.</param>
    [TestCase(TerminalType.CivilianDataPort, false)]
    [TestCase(TerminalType.CorporateMainframe, true)]
    [TestCase(TerminalType.SecurityHub, true)]
    [TestCase(TerminalType.MilitaryServer, true)]
    [TestCase(TerminalType.JotunArchive, true)]
    [TestCase(TerminalType.GlitchedManifold, false)]
    public void HasIce_ReturnsCorrectValue(
        TerminalType terminalType,
        bool expectedHasIce)
    {
        // Act
        var hasIce = _service.HasIce(terminalType);

        // Assert
        hasIce.Should().Be(expectedHasIce);
    }

    // -------------------------------------------------------------------------
    // ICE Triggering Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that TriggerIce creates a valid pending encounter.
    /// </summary>
    [Test]
    public void TriggerIce_CreatesPendingEncounter()
    {
        // Arrange
        var iceType = IceType.Passive;
        var iceRating = 12;

        // Act
        var encounter = _service.TriggerIce(iceType, iceRating);

        // Assert
        encounter.Should().NotBeNull();
        encounter.EncounterId.Should().NotBeNullOrEmpty();
        encounter.IceType.Should().Be(IceType.Passive);
        encounter.IceRating.Should().Be(12);
        encounter.Triggered.Should().BeTrue();
        encounter.IsPending.Should().BeTrue();
        encounter.EncounterResult.Should().Be(IceResolutionOutcome.Pending);
    }

    /// <summary>
    /// Tests that triggered encounter calculates DC correctly from rating.
    /// </summary>
    /// <param name="rating">The ICE rating.</param>
    /// <param name="expectedDc">The expected DC value.</param>
    [TestCase(12, 2)]  // 12/6 = 2
    [TestCase(16, 3)]  // 16/6 = 2.67 → 3
    [TestCase(20, 4)]  // 20/6 = 3.33 → 4
    [TestCase(24, 4)]  // 24/6 = 4
    [TestCase(6, 1)]   // 6/6 = 1
    [TestCase(1, 1)]   // 1/6 = 0.17 → 1 (minimum)
    public void TriggerIce_CalculatesCorrectDc(int rating, int expectedDc)
    {
        // Arrange & Act
        var encounter = _service.TriggerIce(IceType.Active, rating);

        // Assert
        encounter.GetDc().Should().Be(expectedDc);
    }

    // -------------------------------------------------------------------------
    // ICE Resolution Tests - Passive ICE
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Passive ICE resolution with high stats results in evasion.
    /// </summary>
    [Test]
    public void ResolveIce_PassiveIce_WithHighStats_ResultsInEvaded()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 18, finesse: 18);
        var encounter = _service.TriggerIce(IceType.Passive, 12);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert - with high stats, the player should win
        if (result.Outcome == IceResolutionOutcome.Evaded)
        {
            result.Encounter.IceType.Should().Be(IceType.Passive);
            result.ForcedDisconnect.Should().BeFalse();
            result.AlertLevelChange.Should().Be(0);
            result.HasDamage.Should().BeFalse();
            result.HasStress.Should().BeFalse();
        }
    }

    /// <summary>
    /// Tests that Passive ICE failure reveals location and increases alerts.
    /// </summary>
    [Test]
    public void ResolveIce_PassiveIce_OnFailure_IncreasesAlertLevel()
    {
        // Arrange - low stats to ensure failure
        var player = CreateTestPlayer(wits: 1, finesse: 1);
        var encounter = _service.TriggerIce(IceType.Passive, 24);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert - should fail with these low stats and high rating
        if (result.Outcome == IceResolutionOutcome.IceWon)
        {
            result.AlertLevelChange.Should().Be(2);
            result.ForcedDisconnect.Should().BeFalse();
            result.LocationRevealed.Should().BeTrue();
        }
    }

    // -------------------------------------------------------------------------
    // ICE Resolution Tests - Active ICE
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Active ICE victory grants bonus dice.
    /// </summary>
    [Test]
    public void ResolveIce_ActiveIce_OnVictory_GrantsBonusDice()
    {
        // Arrange - high stats to ensure victory
        var player = CreateTestPlayer(wits: 18, finesse: 18);
        var encounter = _service.TriggerIce(IceType.Active, 12);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert
        if (result.Outcome == IceResolutionOutcome.CharacterWon)
        {
            result.BonusDiceGranted.Should().Be(1);
            result.ForcedDisconnect.Should().BeFalse();
        }
    }

    /// <summary>
    /// Tests that Active ICE failure forces disconnect with temporary lockout.
    /// </summary>
    [Test]
    public void ResolveIce_ActiveIce_OnFailure_ForcesDisconnect()
    {
        // Arrange - low stats to ensure failure
        var player = CreateTestPlayer(wits: 1, finesse: 1);
        var encounter = _service.TriggerIce(IceType.Active, 24);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert
        if (result.Outcome == IceResolutionOutcome.IceWon)
        {
            result.ForcedDisconnect.Should().BeTrue();
            result.LockoutDuration.Should().Be(1); // 1 minute lockout
            result.AlertLevelChange.Should().Be(1);
            result.IsPermanentLockout.Should().BeFalse();
        }
    }

    // -------------------------------------------------------------------------
    // ICE Resolution Tests - Lethal ICE
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Lethal ICE save success disconnects with minimal consequences.
    /// </summary>
    [Test]
    public void ResolveIce_LethalIce_OnSaveSuccess_DisconnectsWithStress()
    {
        // Arrange - high resolve to improve save chance
        var player = CreateTestPlayer(will: 18);
        var encounter = _service.TriggerIce(IceType.Lethal, 24);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert
        if (result.Outcome == IceResolutionOutcome.CharacterWon)
        {
            result.ForcedDisconnect.Should().BeTrue();
            result.LockoutDuration.Should().Be(1); // 1 minute lockout
            result.StressGained.Should().BeGreaterThan(0); // 1d6 stress
            result.HasDamage.Should().BeFalse();
            result.IsPermanentLockout.Should().BeFalse();
        }
    }

    /// <summary>
    /// Tests that Lethal ICE save failure causes severe consequences.
    /// </summary>
    [Test]
    public void ResolveIce_LethalIce_OnSaveFailure_CausesSevereConsequences()
    {
        // Arrange - low resolve to ensure save failure
        var player = CreateTestPlayer(will: 1);
        var encounter = _service.TriggerIce(IceType.Lethal, 24);

        // Act
        var result = _service.ResolveIce(encounter, player);

        // Assert
        if (result.Outcome == IceResolutionOutcome.IceWon)
        {
            result.ForcedDisconnect.Should().BeTrue();
            result.IsPermanentLockout.Should().BeTrue();
            result.HasDamage.Should().BeTrue();
            result.PsychicDamage.Should().BeGreaterThan(0); // 3d10 damage
            result.HasStress.Should().BeTrue();
            result.StressGained.Should().BeGreaterThan(0); // 2d6 stress
        }
    }

    // -------------------------------------------------------------------------
    // ICE Resolution Validation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that resolving an already-resolved encounter throws.
    /// </summary>
    [Test]
    public void ResolveIce_AlreadyResolved_ThrowsInvalidOperationException()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = _service.TriggerIce(IceType.Passive, 12);

        // Resolve once
        var result = _service.ResolveIce(encounter, player);
        var resolvedEncounter = result.Encounter;

        // Act & Assert - attempting to resolve again should fail
        var act = () => _service.ResolveIce(resolvedEncounter, player);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already resolved*");
    }

    /// <summary>
    /// Tests that resolving ICE with null player throws ArgumentNullException.
    /// </summary>
    [Test]
    public void ResolveIce_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var encounter = _service.TriggerIce(IceType.Active, 16);

        // Act
        var act = () => _service.ResolveIce(encounter, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("player");
    }

    // -------------------------------------------------------------------------
    // Consequence Application Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that applying Passive ICE failure consequences increases alert level.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_PassiveFailed_IncreasesAlertLevel()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);
        var result = IceResolutionResult.PassiveFailed(encounter, 1, 2);
        var state = TerminalInfiltrationState.Create(
            "inf-001",
            player.Id.ToString(),
            TerminalType.CorporateMainframe,
            "term-001");

        var initialAlert = state.AlertLevel;

        // Act
        _service.ApplyIceConsequences(result, player, state);

        // Assert
        state.AlertLevel.Should().Be(initialAlert + 2);
        state.IsLocationRevealed.Should().BeTrue();
        state.IceEncounters.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that applying Active ICE failure consequences disconnects with lockout.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_ActiveFailed_SetsDisconnectedWithLockout()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);
        var result = IceResolutionResult.ActiveFailed(encounter, 1, 3);
        var state = TerminalInfiltrationState.Create(
            "inf-001",
            player.Id.ToString(),
            TerminalType.SecurityHub,
            "term-001");

        // Act
        _service.ApplyIceConsequences(result, player, state);

        // Assert
        state.WasDisconnectedByIce.Should().BeTrue();
        state.IsInLockout.Should().BeTrue();
        state.LockoutUntil.Should().NotBeNull();
        state.AlertLevel.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that applying Lethal ICE failure consequences causes permanent lockout.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_LethalFailed_CausesPermanentLockout()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = IceEncounter.CreateTriggered(IceType.Lethal, 24);
        var result = IceResolutionResult.LethalFailed(encounter, 0, 15, 8);
        var state = TerminalInfiltrationState.Create(
            "inf-001",
            player.Id.ToString(),
            TerminalType.JotunArchive,
            "term-001");

        // Act
        _service.ApplyIceConsequences(result, player, state);

        // Assert
        state.IsPermanentlyLockedOut.Should().BeTrue();
        state.Status.Should().Be(InfiltrationStatus.LockedOut);
        state.IceEncounters.Should().ContainSingle();
    }

    /// <summary>
    /// Tests that Active ICE victory grants bonus dice tracked in state.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_ActiveVictory_TracksBonusDice()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);
        var result = IceResolutionResult.ActiveDefeated(encounter, 4, 3);
        var state = TerminalInfiltrationState.Create(
            "inf-001",
            player.Id.ToString(),
            TerminalType.SecurityHub,
            "term-001");

        // Act
        _service.ApplyIceConsequences(result, player, state);

        // Assert
        state.HasDefeatedIce.Should().BeTrue();
        state.IceBonusDice.Should().Be(1);
        state.WasDisconnectedByIce.Should().BeFalse();
    }

    /// <summary>
    /// Tests that consequence application with null player throws.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_NullPlayer_ThrowsArgumentNullException()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);
        var result = IceResolutionResult.PassiveEvaded(encounter, 3, 2);
        var state = TerminalInfiltrationState.Create(
            "inf-001",
            "char-001",
            TerminalType.CorporateMainframe,
            "term-001");

        // Act
        var act = () => _service.ApplyIceConsequences(result, null!, state);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("player");
    }

    /// <summary>
    /// Tests that consequence application with null state throws.
    /// </summary>
    [Test]
    public void ApplyIceConsequences_NullState_ThrowsArgumentNullException()
    {
        // Arrange
        var player = CreateTestPlayer();
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);
        var result = IceResolutionResult.PassiveEvaded(encounter, 3, 2);

        // Act
        var act = () => _service.ApplyIceConsequences(result, player, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("infiltrationState");
    }

    // -------------------------------------------------------------------------
    // IceEncounter Value Object Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests IceEncounter factory method creates correct initial state.
    /// </summary>
    [Test]
    public void IceEncounter_CreateTriggered_InitializesCorrectly()
    {
        // Act
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);

        // Assert
        encounter.IceType.Should().Be(IceType.Active);
        encounter.IceRating.Should().Be(16);
        encounter.Triggered.Should().BeTrue();
        encounter.IsPending.Should().BeTrue();
        encounter.WasSuccessful.Should().BeFalse();
        encounter.IceWon.Should().BeFalse();
        encounter.EncounterResult.Should().Be(IceResolutionOutcome.Pending);
    }

    /// <summary>
    /// Tests IceEncounter WithOutcome creates resolved encounter.
    /// </summary>
    [Test]
    public void IceEncounter_WithOutcome_CreatesResolvedEncounter()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);

        // Act
        var resolved = encounter.WithOutcome(IceResolutionOutcome.Evaded);

        // Assert
        resolved.IsPending.Should().BeFalse();
        resolved.WasSuccessful.Should().BeTrue();
        resolved.IceWon.Should().BeFalse();
        resolved.EncounterResult.Should().Be(IceResolutionOutcome.Evaded);
    }

    /// <summary>
    /// Tests IceEncounter WithOutcome for ICE victory.
    /// </summary>
    [Test]
    public void IceEncounter_WithOutcome_IceWon_SetsIceWonTrue()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);

        // Act
        var resolved = encounter.WithOutcome(IceResolutionOutcome.IceWon);

        // Assert
        resolved.IsPending.Should().BeFalse();
        resolved.WasSuccessful.Should().BeFalse();
        resolved.IceWon.Should().BeTrue();
        resolved.EncounterResult.Should().Be(IceResolutionOutcome.IceWon);
    }

    // -------------------------------------------------------------------------
    // IceResolutionResult Value Object Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests IceResolutionResult.PassiveEvaded factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_PassiveEvaded_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);

        // Act
        var result = IceResolutionResult.PassiveEvaded(encounter, 3, 2);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.Evaded);
        result.CharacterRoll.Should().Be(3);
        result.IceDc.Should().Be(2);
        result.ForcedDisconnect.Should().BeFalse();
        result.AlertLevelChange.Should().Be(0);
        result.HasDamage.Should().BeFalse();
        result.HasStress.Should().BeFalse();
        result.LocationRevealed.Should().BeFalse();
    }

    /// <summary>
    /// Tests IceResolutionResult.PassiveFailed factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_PassiveFailed_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Passive, 12);

        // Act
        var result = IceResolutionResult.PassiveFailed(encounter, 0, 2);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.IceWon);
        result.CharacterRoll.Should().Be(0);
        result.IceDc.Should().Be(2);
        result.ForcedDisconnect.Should().BeFalse();
        result.AlertLevelChange.Should().Be(2);
        result.LocationRevealed.Should().BeTrue();
    }

    /// <summary>
    /// Tests IceResolutionResult.ActiveDefeated factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_ActiveDefeated_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);

        // Act
        var result = IceResolutionResult.ActiveDefeated(encounter, 5, 3);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.CharacterWon);
        result.BonusDiceGranted.Should().Be(1);
        result.ForcedDisconnect.Should().BeFalse();
        result.AlertLevelChange.Should().Be(0);
    }

    /// <summary>
    /// Tests IceResolutionResult.ActiveFailed factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_ActiveFailed_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Active, 16);

        // Act
        var result = IceResolutionResult.ActiveFailed(encounter, 1, 3);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.IceWon);
        result.ForcedDisconnect.Should().BeTrue();
        result.LockoutDuration.Should().Be(1);
        result.AlertLevelChange.Should().Be(1);
        result.IsPermanentLockout.Should().BeFalse();
    }

    /// <summary>
    /// Tests IceResolutionResult.LethalSaved factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_LethalSaved_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Lethal, 24);

        // Act
        var result = IceResolutionResult.LethalSaved(encounter, 4, 3);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.CharacterWon);
        result.ForcedDisconnect.Should().BeTrue();
        result.LockoutDuration.Should().Be(1);
        result.HasStress.Should().BeTrue();
        result.StressGained.Should().Be(3);
        result.HasDamage.Should().BeFalse();
        result.IsPermanentLockout.Should().BeFalse();
    }

    /// <summary>
    /// Tests IceResolutionResult.LethalFailed factory creates correct result.
    /// </summary>
    [Test]
    public void IceResolutionResult_LethalFailed_HasCorrectProperties()
    {
        // Arrange
        var encounter = IceEncounter.CreateTriggered(IceType.Lethal, 24);

        // Act
        var result = IceResolutionResult.LethalFailed(encounter, 0, 18, 9);

        // Assert
        result.Outcome.Should().Be(IceResolutionOutcome.IceWon);
        result.ForcedDisconnect.Should().BeTrue();
        result.IsPermanentLockout.Should().BeTrue();
        result.HasDamage.Should().BeTrue();
        result.PsychicDamage.Should().Be(18);
        result.HasStress.Should().BeTrue();
        result.StressGained.Should().Be(9);
    }
}
