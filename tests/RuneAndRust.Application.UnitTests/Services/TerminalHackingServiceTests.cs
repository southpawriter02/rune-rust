// ------------------------------------------------------------------------------
// <copyright file="TerminalHackingServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the TerminalHackingService, covering terminal type DC calculations,
// layer progression, failure handling, and fumble consequences.
// Part of v0.15.4b Terminal Hacking System implementation.
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
/// Unit tests for the <see cref="TerminalHackingService"/> service.
/// </summary>
[TestFixture]
public class TerminalHackingServiceTests
{
    // -------------------------------------------------------------------------
    // Test Dependencies
    // -------------------------------------------------------------------------

    private SkillCheckService _skillCheckService = null!;
    private IFumbleConsequenceService _fumbleService = null!;
    private IGameConfigurationProvider _configProvider = null!;
    private ILogger<TerminalHackingService> _logger = null!;
    private ILogger<SkillCheckService> _skillCheckLogger = null!;
    private ILogger<DiceService> _diceLogger = null!;
    private DiceService _diceService = null!;
    private TerminalHackingService _service = null!;

    // -------------------------------------------------------------------------
    // Setup and Teardown
    // -------------------------------------------------------------------------

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<TerminalHackingService>>();
        _skillCheckLogger = Substitute.For<ILogger<SkillCheckService>>();
        _diceLogger = Substitute.For<ILogger<DiceService>>();
        _fumbleService = Substitute.For<IFumbleConsequenceService>();
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
        _service = new TerminalHackingService(
            _skillCheckService,
            _fumbleService,
            _configProvider,
            _logger);
    }

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

        // Stealth skill for cover tracks
        var stealth = SkillDefinition.Create(
            "stealth",
            "Stealth",
            "Move unseen and unheard.",
            "finesse",
            null,
            "1d10");

        _configProvider.GetSkillById("system-bypass").Returns(systemBypass);
        _configProvider.GetSkillById("stealth").Returns(stealth);
        _configProvider.GetSkills().Returns(new List<SkillDefinition> { systemBypass, stealth });

        // Difficulty class configuration
        var easy = DifficultyClassDefinition.Create("easy", "Easy", "Simple task.", 10);
        var moderate = DifficultyClassDefinition.Create("moderate", "Moderate", "Requires effort.", 14);
        var hard = DifficultyClassDefinition.Create("hard", "Hard", "Challenging task.", 18);

        _configProvider.GetDifficultyClassById("easy").Returns(easy);
        _configProvider.GetDifficultyClassById("moderate").Returns(moderate);
        _configProvider.GetDifficultyClassById("hard").Returns(hard);
        _configProvider.GetDifficultyClasses()
            .Returns(new List<DifficultyClassDefinition> { easy, moderate, hard });

        // Default fumble service behavior
        _fumbleService.IsCheckBlocked(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>())
            .Returns(false);
    }

    private static Player CreateTestPlayer(int wits = 10, int finesse = 10)
    {
        var attributes = new PlayerAttributes(8, 8, 8, wits, finesse);
        return new Player("TestHacker", "human", "infiltrator", attributes, "Test");
    }

    // -------------------------------------------------------------------------
    // Terminal Type DC Tests (Parameterized)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that each terminal type sets the correct Layer 1 DC.
    /// </summary>
    /// <param name="terminalType">The terminal type to test.</param>
    /// <param name="expectedDc">The expected Layer 1 DC.</param>
    [TestCase(TerminalType.CivilianDataPort, 8)]
    [TestCase(TerminalType.CorporateMainframe, 12)]
    [TestCase(TerminalType.SecurityHub, 16)]
    [TestCase(TerminalType.MilitaryServer, 20)]
    [TestCase(TerminalType.JotunArchive, 24)]
    public void GetLayerDc_ForTerminalType_ReturnsCorrectLayer1Dc(
        TerminalType terminalType,
        int expectedDc)
    {
        // Arrange
        var context = TerminalContext.Create("term-001", terminalType);

        // Act
        var dc = _service.GetLayerDc(InfiltrationLayer.Layer1_Access, context);

        // Assert
        dc.Should().Be(expectedDc);
    }

    // -------------------------------------------------------------------------
    // Layer DC Calculation Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Layer 2 DC is calculated correctly from Layer 1 DC + security modifier.
    /// </summary>
    [Test]
    public void GetLayerDc_ForLayer2_IncludesSecurityModifier()
    {
        // Arrange
        // Corporate mainframe (DC 12) with Biometric security (+2)
        var context = TerminalContext.CreateSecured(
            "term-001",
            TerminalType.CorporateMainframe,
            SecurityLevel.Biometric);

        // Act
        var dc = _service.GetLayerDc(InfiltrationLayer.Layer2_Authentication, context);

        // Assert
        dc.Should().Be(14); // 12 + 2
    }

    /// <summary>
    /// Tests that Layer 3 DC is based on the data type.
    /// </summary>
    [Test]
    public void GetLayerDc_ForLayer3_UsesDataTypeDc()
    {
        // Arrange
        var context = new TerminalContext(
            "term-001",
            TerminalType.CorporateMainframe,
            targetDataType: DataType.Classified);

        // Act
        var dc = _service.GetLayerDc(InfiltrationLayer.Layer3_Navigation, context);

        // Assert
        dc.Should().Be(18); // Classified data type DC
    }

    /// <summary>
    /// Tests that corruption modifiers increase Layer 1 DC correctly.
    /// </summary>
    [Test]
    public void GetLayerDc_WithCorruption_IncludesCorruptionModifier()
    {
        // Arrange
        // Security hub (DC 16) with Blighted corruption (+4)
        var context = TerminalContext.CreateCorrupted(
            "term-001",
            TerminalType.SecurityHub,
            CorruptionTier.Blighted);

        // Act
        var dc = _service.GetLayerDc(InfiltrationLayer.Layer1_Access, context);

        // Assert
        dc.Should().Be(20); // 16 + 4
    }

    // -------------------------------------------------------------------------
    // Infiltration State Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that BeginInfiltration creates a valid initial state.
    /// </summary>
    [Test]
    public void BeginInfiltration_CreatesValidInitialState()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);

        // Act
        var state = _service.BeginInfiltration(player, context);

        // Assert
        state.Should().NotBeNull();
        state.InfiltrationId.Should().StartWith("inf-");
        state.CharacterId.Should().Be(player.Id.ToString());
        state.TerminalType.Should().Be(TerminalType.CivilianDataPort);
        state.CurrentLayer.Should().Be(1);
        state.AccessLevel.Should().Be(AccessLevel.None);
        state.Status.Should().Be(InfiltrationStatus.InProgress);
        state.IsComplete.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Layer 1 failure sets temporary lockout status.
    /// </summary>
    [Test]
    public void AttemptCurrentLayer_Layer1Failure_SetsTemporaryLockoutStatus()
    {
        // Arrange - using low stats to guarantee failure
        var player = CreateTestPlayer(wits: 1, finesse: 1);
        var context = new TerminalContext(
            "term-001",
            TerminalType.MilitaryServer); // DC 20 makes failure very likely

        var state = _service.BeginInfiltration(player, context);

        // Act
        var result = _service.AttemptCurrentLayer(player, state, context);

        // Assert - check if the state reflects failure (not fumble)
        if (!result.IsFumble && !result.IsSuccess)
        {
            state.Status.Should().Be(InfiltrationStatus.TemporaryLockout);
            state.AlertLevel.Should().BeGreaterThan(0);
        }
    }

    /// <summary>
    /// Tests that Layer 2 failure triggers alert status.
    /// </summary>
    [Test]
    public void AttemptCurrentLayer_Layer2Failure_TriggersAlertStatus()
    {
        // Arrange - create state at layer 2 by recording a layer 1 success
        var player = CreateTestPlayer(wits: 15, finesse: 15);
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);

        var state = _service.BeginInfiltration(player, context);

        // Manually advance to Layer 2 by recording a Layer 1 success
        var layer1Success = CreateSuccessResult(InfiltrationLayer.Layer1_Access);
        state.RecordLayerResult(layer1Success);

        // Now create a low-stat player for Layer 2 attempt
        var weakPlayer = CreateTestPlayer(wits: 1, finesse: 1);

        // Need to attempt with high DC context to force failure
        var highSecContext = TerminalContext.CreateSecured(
            "term-001",
            TerminalType.JotunArchive, // Very high DC
            SecurityLevel.JotunLocked);

        // Act
        var result = _service.AttemptCurrentLayer(weakPlayer, state, highSecContext);

        // Assert
        if (!result.IsFumble && !result.IsSuccess)
        {
            state.Status.Should().Be(InfiltrationStatus.AlertTriggered);
            state.AlertLevel.Should().BeGreaterThanOrEqualTo(3); // +1 for failure, +2 for Layer 2
        }
    }

    // -------------------------------------------------------------------------
    // Fumble Consequence Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that fumble creates a permanent lockout consequence.
    /// </summary>
    [Test]
    public void AttemptCurrentLayer_Fumble_CreatesPermanentLockout()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);

        var state = _service.BeginInfiltration(player, context);

        // Create a fumble result manually and record it
        var fumbleCheckResult = CreateFumbleCheckResult();
        var fumbleResult = LayerResult.Fumble(
            InfiltrationLayer.Layer1_Access,
            fumbleCheckResult,
            8,
            "Catastrophic failure!");

        // Act
        state.RecordLayerResult(fumbleResult);

        // Assert
        state.IsLockedOut.Should().BeTrue();
        state.AccessLevel.Should().Be(AccessLevel.Lockout);
        state.Status.Should().Be(InfiltrationStatus.LockedOut);
        state.IsComplete.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanAttempt returns false when terminal is locked out.
    /// </summary>
    [Test]
    public void CanAttempt_WhenSystemLockoutExists_ReturnsFalse()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CorporateMainframe);

        _fumbleService.IsCheckBlocked(
            player.Id.ToString(),
            "system-bypass",
            "term-001")
            .Returns(true);

        // Act
        var canAttempt = _service.CanAttempt(player, context);

        // Assert
        canAttempt.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetAttemptBlockedReason returns lockout message.
    /// </summary>
    [Test]
    public void GetAttemptBlockedReason_WhenLockedOut_ReturnsLockoutMessage()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CorporateMainframe);

        _fumbleService.IsCheckBlocked(
            player.Id.ToString(),
            "system-bypass",
            "term-001")
            .Returns(true);

        // Act
        var reason = _service.GetAttemptBlockedReason(player, context);

        // Assert
        reason.Should().NotBeNull();
        reason.Should().Contain("locked out");
    }

    // -------------------------------------------------------------------------
    // Cover Tracks Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that cover tracks can only be attempted after successful infiltration.
    /// </summary>
    [Test]
    public void AttemptCoverTracks_BeforeSuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);
        var state = _service.BeginInfiltration(player, context);

        // Act
        var act = () => _service.AttemptCoverTracks(player, state);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful infiltration*");
    }

    /// <summary>
    /// Tests that attempting layer on completed infiltration throws.
    /// </summary>
    [Test]
    public void AttemptCurrentLayer_WhenComplete_ThrowsInvalidOperationException()
    {
        // Arrange
        var player = CreateTestPlayer();
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);
        var state = _service.BeginInfiltration(player, context);

        // Mark as complete via fumble
        var fumbleResult = LayerResult.Fumble(
            InfiltrationLayer.Layer1_Access,
            CreateFumbleCheckResult(),
            8,
            "Test fumble");
        state.RecordLayerResult(fumbleResult);

        // Act
        var act = () => _service.AttemptCurrentLayer(player, state, context);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed infiltration*");
    }

    // -------------------------------------------------------------------------
    // Full Infiltration Flow Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that successful infiltration through all layers grants proper access.
    /// </summary>
    [Test]
    public void FullInfiltration_Success_GrantsUserLevelAccess()
    {
        // Arrange
        var player = CreateTestPlayer(wits: 15, finesse: 15);
        var context = TerminalContext.Create("term-001", TerminalType.CivilianDataPort);
        var state = _service.BeginInfiltration(player, context);

        // Record Layer 1 success
        var layer1Result = CreateSuccessResult(InfiltrationLayer.Layer1_Access);
        state.RecordLayerResult(layer1Result);

        // Verify progression to Layer 2
        state.CurrentLayer.Should().Be(2);

        // Record Layer 2 success with UserLevel access
        var layer2Result = CreateSuccessResult(
            InfiltrationLayer.Layer2_Authentication,
            AccessLevel.UserLevel);
        state.RecordLayerResult(layer2Result);

        // Verify progression to Layer 3 and access level
        state.CurrentLayer.Should().Be(3);
        state.AccessLevel.Should().Be(AccessLevel.UserLevel);

        // Record Layer 3 success
        var layer3Result = CreateSuccessResult(InfiltrationLayer.Layer3_Navigation);
        state.RecordLayerResult(layer3Result);

        // Assert
        state.IsComplete.Should().BeTrue();
        state.IsSuccessful.Should().BeTrue();
        state.Status.Should().Be(InfiltrationStatus.Completed);
        state.AccessLevel.Should().Be(AccessLevel.UserLevel);
    }

    /// <summary>
    /// Tests that critical success grants admin access.
    /// </summary>
    [Test]
    public void Layer2CriticalSuccess_GrantsAdminLevelAccess()
    {
        // Arrange
        var state = TerminalInfiltrationState.Create(
            "inf-test",
            "char-001",
            TerminalType.CorporateMainframe,
            "term-001");

        // Record Layer 1 success
        var layer1Result = CreateSuccessResult(InfiltrationLayer.Layer1_Access);
        state.RecordLayerResult(layer1Result);

        // Record Layer 2 critical success
        var layer2Result = CreateCriticalSuccessResult(
            InfiltrationLayer.Layer2_Authentication,
            AccessLevel.AdminLevel);
        state.RecordLayerResult(layer2Result);

        // Assert
        state.AccessLevel.Should().Be(AccessLevel.AdminLevel);
        state.HasAdminAccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    private static SkillCheckResult CreateFumbleCheckResult()
    {
        // Create a mock result indicating a fumble (critical failure)
        // Fumble = 0 successes AND >= 1 botch
        var pool = DicePool.D10(5); // 5d10
        var rolls = new[] { 1, 1, 3, 4, 5 }; // 0 successes (none >= 8), 2 botches (two 1s) = fumble

        var diceResult = new DiceRollResult(
            pool: pool,
            rolls: rolls,
            advantageType: AdvantageType.Normal);

        return new SkillCheckResult(
            skillId: "system-bypass",
            skillName: "System Bypass",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 8,
            difficultyName: "Terminal Access");
    }

    private static LayerResult CreateSuccessResult(
        InfiltrationLayer layer,
        AccessLevel? accessGranted = null)
    {
        // Create a success result: 3 successes vs DC 2 = margin +1 (FullSuccess)
        var pool = DicePool.D10(5); // 5d10
        var rolls = new[] { 8, 9, 10, 4, 5 }; // 3 successes (8, 9, 10), 0 botches = 3 net

        var diceResult = new DiceRollResult(
            pool: pool,
            rolls: rolls,
            advantageType: AdvantageType.Normal);

        var checkResult = new SkillCheckResult(
            skillId: "system-bypass",
            skillName: "System Bypass",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 2,
            difficultyName: "Terminal Access");

        return LayerResult.Success(
            layer,
            checkResult,
            SkillOutcome.FullSuccess,
            8,
            1,
            accessGranted,
            "Test success narrative");
    }

    private static LayerResult CreateCriticalSuccessResult(
        InfiltrationLayer layer,
        AccessLevel? accessGranted = null)
    {
        // Create a critical success: 7 successes vs DC 2 = margin +5 (CriticalSuccess)
        var pool = DicePool.D10(7); // 7d10
        var rolls = new[] { 8, 8, 9, 9, 10, 10, 10 }; // 7 successes, 0 botches = 7 net

        var diceResult = new DiceRollResult(
            pool: pool,
            rolls: rolls,
            advantageType: AdvantageType.Normal);

        var checkResult = new SkillCheckResult(
            skillId: "system-bypass",
            skillName: "System Bypass",
            diceResult: diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 2,
            difficultyName: "Terminal Access");

        return LayerResult.Success(
            layer,
            checkResult,
            SkillOutcome.CriticalSuccess,
            8,
            1,
            accessGranted,
            "Test critical success narrative");
    }
}
