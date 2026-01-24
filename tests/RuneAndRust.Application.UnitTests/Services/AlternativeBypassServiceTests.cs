// ------------------------------------------------------------------------------
// <copyright file="AlternativeBypassServiceTests.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Unit tests for the AlternativeBypassService, covering brute force attempts,
// alternative method queries, prerequisite evaluation, and consequence handling.
// Part of v0.15.4h Alternative Bypass Methods implementation.
// </summary>
// ------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for the <see cref="AlternativeBypassService"/> service.
/// </summary>
/// <remarks>
/// <para>
/// Tests cover the following areas:
/// <list type="bullet">
///   <item><description>Brute force DC by target type (SimpleDoor 12, ReinforcedDoor 16, Vault 22)</description></item>
///   <item><description>Container DC calculation (8 + strength × 2)</description></item>
///   <item><description>Tool modifiers reduce DC (Crowbar -2, Sledgehammer -4)</description></item>
///   <item><description>Tool bonus dice (Crowbar +1d10, Sledgehammer +2d10)</description></item>
///   <item><description>Retry DC calculation with penalties</description></item>
///   <item><description>Fumble damage and tool breaking</description></item>
///   <item><description>Critical success (net ≥5) reduces consequences</description></item>
///   <item><description>Alternative method listing by obstacle type</description></item>
///   <item><description>Prerequisite evaluation for abilities and items</description></item>
///   <item><description>Difficulty description lookup</description></item>
///   <item><description>Null argument validation (guard clauses)</description></item>
/// </list>
/// </para>
/// </remarks>
[TestFixture]
public class AlternativeBypassServiceTests
{
    // -------------------------------------------------------------------------
    // Test Dependencies
    // -------------------------------------------------------------------------

    private IDiceService _diceService = null!;
    private ILogger<AlternativeBypassService> _logger = null!;
    private AlternativeBypassService _service = null!;

    // -------------------------------------------------------------------------
    // Setup and Teardown
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<AlternativeBypassService>>();
        _diceService = Substitute.For<IDiceService>();

        // Create service under test
        _service = new AlternativeBypassService(_diceService, _logger);
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
    /// Configures the dice service mock for fumble damage roll.
    /// </summary>
    private void SetupFumbleDamageRoll(int damage)
    {
        var pool = DicePool.D6(1);
        var result = new DiceRollResult(pool, new[] { damage });

        // Configure for D6 pool (fumble damage)
        _diceService.Roll(Arg.Is<DicePool>(p => p.DiceType == DiceType.D6), Arg.Any<AdvantageType>())
            .Returns(result);
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

    // =========================================================================
    // BRUTE FORCE DC BY TARGET TYPE TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that SimpleDoor has base DC 12.
    /// </summary>
    [Test]
    public void GetBruteForceOption_SimpleDoor_HasDc12()
    {
        // Act
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);

        // Assert
        option.BaseDc.Should().Be(12);
        option.TargetType.Should().Be(BruteForceTargetType.SimpleDoor);
    }

    /// <summary>
    /// Verifies that ReinforcedDoor has base DC 16.
    /// </summary>
    [Test]
    public void GetBruteForceOption_ReinforcedDoor_HasDc16()
    {
        // Act
        var option = _service.GetBruteForceOption(BruteForceTargetType.ReinforcedDoor);

        // Assert
        option.BaseDc.Should().Be(16);
        option.TargetType.Should().Be(BruteForceTargetType.ReinforcedDoor);
    }

    /// <summary>
    /// Verifies that Vault has base DC 22.
    /// </summary>
    [Test]
    public void GetBruteForceOption_Vault_HasDc22()
    {
        // Act
        var option = _service.GetBruteForceOption(BruteForceTargetType.Vault);

        // Assert
        option.BaseDc.Should().Be(22);
        option.TargetType.Should().Be(BruteForceTargetType.Vault);
    }

    /// <summary>
    /// Verifies that Container DC is calculated as 8 + (strength × 2).
    /// </summary>
    /// <param name="strength">Container strength rating.</param>
    /// <param name="expectedDc">Expected DC value.</param>
    [TestCase(1, 10)]   // 8 + (1 × 2) = 10
    [TestCase(3, 14)]   // 8 + (3 × 2) = 14
    [TestCase(5, 18)]   // 8 + (5 × 2) = 18
    public void GetBruteForceOption_Container_DcBasedOnStrength(int strength, int expectedDc)
    {
        // Act
        var option = _service.GetBruteForceOption(BruteForceTargetType.Container, strength);

        // Assert
        option.BaseDc.Should().Be(expectedDc);
        option.TargetType.Should().Be(BruteForceTargetType.Container);
    }

    /// <summary>
    /// Verifies max attempts for each target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <param name="expectedMaxAttempts">Expected max attempts.</param>
    [TestCase(BruteForceTargetType.SimpleDoor, 5)]
    [TestCase(BruteForceTargetType.ReinforcedDoor, 3)]
    [TestCase(BruteForceTargetType.Vault, 2)]
    [TestCase(BruteForceTargetType.Container, 3)]
    public void GetBruteForceOption_HasCorrectMaxAttempts(
        BruteForceTargetType targetType,
        int expectedMaxAttempts)
    {
        // Act
        var option = _service.GetBruteForceOption(targetType);

        // Assert
        option.MaxAttempts.Should().Be(expectedMaxAttempts);
    }

    /// <summary>
    /// Verifies retry penalty per attempt for each target type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <param name="expectedPenalty">Expected retry penalty per attempt.</param>
    [TestCase(BruteForceTargetType.SimpleDoor, 1)]
    [TestCase(BruteForceTargetType.ReinforcedDoor, 2)]
    [TestCase(BruteForceTargetType.Vault, 3)]
    [TestCase(BruteForceTargetType.Container, 1)]
    public void GetBruteForceOption_HasCorrectRetryPenalty(
        BruteForceTargetType targetType,
        int expectedPenalty)
    {
        // Act
        var option = _service.GetBruteForceOption(targetType);

        // Assert
        option.RetryPenaltyPerAttempt.Should().Be(expectedPenalty);
    }

    // =========================================================================
    // BRUTE FORCE ATTEMPT TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that successful brute force attempt destroys obstacle.
    /// </summary>
    [Test]
    public void AttemptBruteForce_Success_DestroysObstacle()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
        var context = BruteForceContext.FirstAttempt();
        SetupDiceRoll(netSuccesses: 2);

        // Act
        var result = _service.AttemptBruteForce(mightAttribute: 5, option, context);

        // Assert
        result.Success.Should().BeTrue();
        result.ObstacleDestroyed.Should().BeTrue();
        result.IsFumble.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that critical success (net ≥5) is detected.
    /// </summary>
    [Test]
    public void AttemptBruteForce_CriticalSuccess_WhenNetSuccessesAtLeast5()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
        var context = BruteForceContext.FirstAttempt();
        SetupDiceRoll(netSuccesses: 5);

        // Act
        var result = _service.AttemptBruteForce(mightAttribute: 8, option, context);

        // Assert
        result.Success.Should().BeTrue();
        result.IsCritical.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that failure allows retry if within attempt limit.
    /// </summary>
    [Test]
    public void AttemptBruteForce_Failure_AllowsRetryIfWithinLimit()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor); // max 5 attempts
        var context = BruteForceContext.FirstAttempt();
        SetupDiceRoll(netSuccesses: 0, isFumble: false);

        // Act
        var result = _service.AttemptBruteForce(mightAttribute: 3, option, context);

        // Assert
        result.Success.Should().BeFalse();
        result.RetryPossible.Should().BeTrue();
        result.NewDc.Should().BeGreaterThan(option.BaseDc);
    }

    /// <summary>
    /// Verifies that fumble causes damage and breaks tool.
    /// </summary>
    [Test]
    public void AttemptBruteForce_Fumble_CausesDamageAndBreaksTool()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
        var context = BruteForceContext.FirstAttemptWithTool("crowbar");
        SetupDiceRoll(netSuccesses: 0, isFumble: true);
        SetupFumbleDamageRoll(damage: 4);

        // Act
        var result = _service.AttemptBruteForce(mightAttribute: 3, option, context);

        // Assert
        result.Success.Should().BeFalse();
        result.IsFumble.Should().BeTrue();
        result.DamageToCharacter.Should().BeGreaterThan(0);
        result.ToolBroken.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that fumble without tool does not break tool.
    /// </summary>
    [Test]
    public void AttemptBruteForce_FumbleWithoutTool_DoesNotBreakTool()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor);
        var context = BruteForceContext.FirstAttempt(); // No tool
        SetupDiceRoll(netSuccesses: 0, isFumble: true);
        SetupFumbleDamageRoll(damage: 3);

        // Act
        var result = _service.AttemptBruteForce(mightAttribute: 3, option, context);

        // Assert
        result.IsFumble.Should().BeTrue();
        result.ToolBroken.Should().BeFalse();
    }

    // =========================================================================
    // TOOL MODIFIER TESTS
    // =========================================================================

    /// <summary>
    /// Verifies tool bonus dice values.
    /// </summary>
    /// <param name="tool">Tool name.</param>
    /// <param name="expectedBonus">Expected bonus dice.</param>
    [TestCase("crowbar", 1)]
    [TestCase("sledgehammer", 2)]
    [TestCase("breaching charge", 2)]
    [TestCase("industrial cutter", 2)]
    [TestCase("knife", 0)]
    [TestCase("unknown tool", 0)]
    [TestCase("", 0)]
    public void GetToolBonusDice_ReturnsCorrectValue(string tool, int expectedBonus)
    {
        // Act
        var bonus = _service.GetToolBonusDice(tool);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    /// <summary>
    /// Verifies tool bonus dice lookup is case-insensitive.
    /// </summary>
    [Test]
    public void GetToolBonusDice_IsCaseInsensitive()
    {
        // Act & Assert
        _service.GetToolBonusDice("CROWBAR").Should().Be(1);
        _service.GetToolBonusDice("Sledgehammer").Should().Be(2);
        _service.GetToolBonusDice("BREACHING CHARGE").Should().Be(2);
    }

    // =========================================================================
    // RETRY DC TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GetRetryDc includes retry penalty.
    /// </summary>
    [Test]
    public void GetRetryDc_IncludesRetryPenalty()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor); // DC 12, +1 per retry

        // Act
        var retryDc1 = _service.GetRetryDc(option, attemptsMade: 1, hasFumbled: false);
        var retryDc2 = _service.GetRetryDc(option, attemptsMade: 2, hasFumbled: false);

        // Assert
        retryDc1.Should().Be(13); // 12 + 1
        retryDc2.Should().Be(14); // 12 + 2
    }

    /// <summary>
    /// Verifies that GetRetryDc includes fumble penalty of +2.
    /// </summary>
    [Test]
    public void GetRetryDc_IncludesFumblePenalty()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor); // DC 12

        // Act
        var retryDcWithFumble = _service.GetRetryDc(option, attemptsMade: 1, hasFumbled: true);
        var retryDcWithoutFumble = _service.GetRetryDc(option, attemptsMade: 1, hasFumbled: false);

        // Assert
        retryDcWithFumble.Should().Be(retryDcWithoutFumble + 2);
    }

    // =========================================================================
    // CAN RETRY TESTS
    // =========================================================================

    /// <summary>
    /// Verifies CanRetry returns true when attempts remain.
    /// </summary>
    [Test]
    public void CanRetry_WithAttemptsRemaining_ReturnsTrue()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor); // max 5 attempts

        // Act & Assert
        _service.CanRetry(option, attemptsMade: 1).Should().BeTrue();
        _service.CanRetry(option, attemptsMade: 4).Should().BeTrue();
    }

    /// <summary>
    /// Verifies CanRetry returns false when attempts exhausted.
    /// </summary>
    [Test]
    public void CanRetry_WithNoAttemptsRemaining_ReturnsFalse()
    {
        // Arrange
        var option = _service.GetBruteForceOption(BruteForceTargetType.SimpleDoor); // max 5 attempts

        // Act & Assert
        _service.CanRetry(option, attemptsMade: 5).Should().BeFalse();
        _service.CanRetry(option, attemptsMade: 6).Should().BeFalse();
    }

    // =========================================================================
    // ALTERNATIVE METHOD QUERY TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that GetAlternatives returns correct methods for LockedDoor.
    /// </summary>
    [Test]
    public void GetAlternatives_LockedDoor_ReturnsFourMethods()
    {
        // Act
        var alternatives = _service.GetAlternatives(BypassObstacleType.LockedDoor).ToList();

        // Assert
        alternatives.Should().HaveCount(4);
        alternatives.Should().Contain(a => a.MethodName == "Find Key");
        alternatives.Should().Contain(a => a.MethodName == "Brute Force");
        alternatives.Should().Contain(a => a.MethodName == "Runic Bypass");
        alternatives.Should().Contain(a => a.MethodName == "Alternate Route");
    }

    /// <summary>
    /// Verifies that GetAlternatives returns correct methods for SecuredTerminal.
    /// </summary>
    [Test]
    public void GetAlternatives_SecuredTerminal_ReturnsThreeMethods()
    {
        // Act
        var alternatives = _service.GetAlternatives(BypassObstacleType.SecuredTerminal).ToList();

        // Assert
        alternatives.Should().HaveCount(3);
        alternatives.Should().Contain(a => a.MethodName == "Find Codes");
        alternatives.Should().Contain(a => a.MethodName == "Hotwire to Different Terminal");
        alternatives.Should().Contain(a => a.MethodName == "Accept Partial Access");
    }

    /// <summary>
    /// Verifies that GetAlternatives returns correct methods for ActiveTrap.
    /// </summary>
    [Test]
    public void GetAlternatives_ActiveTrap_ReturnsThreeMethods()
    {
        // Act
        var alternatives = _service.GetAlternatives(BypassObstacleType.ActiveTrap).ToList();

        // Assert
        alternatives.Should().HaveCount(3);
        alternatives.Should().Contain(a => a.MethodName == "Trigger from Distance");
        alternatives.Should().Contain(a => a.MethodName == "Destroy Mechanism");
        alternatives.Should().Contain(a => a.MethodName == "Sacrifice (Shield/Item)");
    }

    /// <summary>
    /// Verifies that GetAlternatives returns empty for unimplemented obstacle types.
    /// </summary>
    [TestCase(BypassObstacleType.PhysicalBarrier)]
    [TestCase(BypassObstacleType.EnergyBarrier)]
    public void GetAlternatives_UnimplementedObstacle_ReturnsEmpty(BypassObstacleType obstacleType)
    {
        // Act
        var alternatives = _service.GetAlternatives(obstacleType).ToList();

        // Assert
        alternatives.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that alternative methods have correct check types.
    /// </summary>
    [Test]
    public void GetAlternatives_LockedDoor_HaveCorrectCheckTypes()
    {
        // Act
        var alternatives = _service.GetAlternatives(BypassObstacleType.LockedDoor).ToList();

        // Assert
        alternatives.Single(a => a.MethodName == "Find Key")
            .RequiredCheck.Should().Be(AlternativeCheckType.Investigation);
        alternatives.Single(a => a.MethodName == "Brute Force")
            .RequiredCheck.Should().Be(AlternativeCheckType.Might);
        alternatives.Single(a => a.MethodName == "Runic Bypass")
            .RequiredCheck.Should().Be(AlternativeCheckType.Runecraft);
        alternatives.Single(a => a.MethodName == "Alternate Route")
            .RequiredCheck.Should().Be(AlternativeCheckType.Perception);
    }

    /// <summary>
    /// Verifies that GetAlternativeById returns correct method.
    /// </summary>
    [Test]
    public void GetAlternativeById_ValidId_ReturnsMethod()
    {
        // Act
        var method = _service.GetAlternativeById("alt_door_find_key");

        // Assert
        method.Should().NotBeNull();
        method!.Value.MethodName.Should().Be("Find Key");
        method.Value.ObstacleType.Should().Be(BypassObstacleType.LockedDoor);
    }

    /// <summary>
    /// Verifies that GetAlternativeById returns null for invalid ID.
    /// </summary>
    [Test]
    public void GetAlternativeById_InvalidId_ReturnsNull()
    {
        // Act
        var method = _service.GetAlternativeById("alt_invalid_method");

        // Assert
        method.Should().BeNull();
    }

    // =========================================================================
    // PREREQUISITE EVALUATION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that method with no prerequisites can be attempted.
    /// </summary>
    [Test]
    public void EvaluateAlternative_NoPrerequisites_CanAttempt()
    {
        // Arrange
        var method = AlternativeMethod.FindKey; // No prerequisites

        // Act
        var result = _service.EvaluateAlternative(
            method,
            characterAbilities: Array.Empty<string>(),
            characterItems: Array.Empty<string>());

        // Assert
        result.CanAttempt.Should().BeTrue();
        result.MissingPrerequisites.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that method with ability prerequisite fails without ability.
    /// </summary>
    [Test]
    public void EvaluateAlternative_MissingAbilityPrerequisite_CannotAttempt()
    {
        // Arrange
        var method = AlternativeMethod.RunicBypass; // Requires [Rune Sight]

        // Act
        var result = _service.EvaluateAlternative(
            method,
            characterAbilities: Array.Empty<string>(),
            characterItems: Array.Empty<string>());

        // Assert
        result.CanAttempt.Should().BeFalse();
        result.MissingPrerequisites.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that method with ability prerequisite succeeds with ability.
    /// </summary>
    [Test]
    public void EvaluateAlternative_HasAbilityPrerequisite_CanAttempt()
    {
        // Arrange
        var method = AlternativeMethod.RunicBypass; // Requires [Rune Sight] and Runecraft

        // Act
        var result = _service.EvaluateAlternative(
            method,
            characterAbilities: new[] { "[Rune Sight]", "Runecraft" },
            characterItems: Array.Empty<string>());

        // Assert
        result.CanAttempt.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that evaluation returns correct difficulty description.
    /// </summary>
    [Test]
    public void EvaluateAlternative_ReturnsDifficultyDescription()
    {
        // Arrange
        var method = AlternativeMethod.FindKey; // DC 14 = Moderate

        // Act
        var result = _service.EvaluateAlternative(
            method,
            characterAbilities: Array.Empty<string>(),
            characterItems: Array.Empty<string>());

        // Assert
        result.EstimatedDifficulty.Should().Be("Moderate");
    }

    // =========================================================================
    // DIFFICULTY DESCRIPTION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies difficulty description for various DC values.
    /// </summary>
    /// <param name="dc">Difficulty class value.</param>
    /// <param name="expectedDescription">Expected description text.</param>
    [TestCase(0, "Automatic")]
    [TestCase(5, "Very Easy")]
    [TestCase(8, "Very Easy")]
    [TestCase(10, "Easy")]
    [TestCase(12, "Easy")]
    [TestCase(14, "Moderate")]
    [TestCase(16, "Moderate")]
    [TestCase(18, "Hard")]
    [TestCase(20, "Hard")]
    [TestCase(22, "Very Hard")]
    [TestCase(24, "Very Hard")]
    [TestCase(25, "Nearly Impossible")]
    [TestCase(30, "Nearly Impossible")]
    public void GetDifficultyDescription_ReturnsCorrectDescription(int dc, string expectedDescription)
    {
        // Act
        var description = _service.GetDifficultyDescription(dc);

        // Assert
        description.Should().Be(expectedDescription);
    }

    // =========================================================================
    // VALUE OBJECT TESTS
    // =========================================================================

    /// <summary>
    /// Verifies BruteForceContext factory methods.
    /// </summary>
    [Test]
    public void BruteForceContext_FactoryMethods_CreateCorrectly()
    {
        // FirstAttempt
        var first = BruteForceContext.FirstAttempt();
        first.AttemptNumber.Should().Be(1);
        first.HasFumbled.Should().BeFalse();
        first.ToolUsed.Should().BeNull();

        // FirstAttemptWithTool
        var firstWithTool = BruteForceContext.FirstAttemptWithTool("crowbar");
        firstWithTool.ToolUsed.Should().Be("crowbar");
        firstWithTool.AttemptNumber.Should().Be(1);

        // Retry
        var retry = BruteForceContext.Retry(previousAttempts: 1, previouslyFumbled: false);
        retry.AttemptNumber.Should().Be(2); // previousAttempts + 1
        retry.HasFumbled.Should().BeFalse();

        // Retry with fumble
        var retryWithFumble = BruteForceContext.Retry(previousAttempts: 2, previouslyFumbled: true, toolName: "sledgehammer");
        retryWithFumble.AttemptNumber.Should().Be(3);
        retryWithFumble.HasFumbled.Should().BeTrue();
        retryWithFumble.ToolUsed.Should().Be("sledgehammer");
    }

    /// <summary>
    /// Verifies BruteForceOption has consequences and tool modifiers.
    /// </summary>
    [Test]
    public void BruteForceOption_HasConsequencesAndToolModifiers()
    {
        // Arrange
        var option = BruteForceOption.SimpleDoor;

        // Assert
        option.Consequences.Should().NotBeEmpty();
        option.ToolModifiers.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies BruteForceResult success factory creates correctly.
    /// </summary>
    [Test]
    public void BruteForceResult_SuccessFactory_CreatesCorrectly()
    {
        // Arrange
        var consequences = new List<AppliedConsequence>
        {
            new AppliedConsequence(ConsequenceType.Noise, "Loud noise alerts nearby")
        };

        // Act
        var result = BruteForceResult.CreateSuccess(
            isCritical: false,
            consequences.AsReadOnly(),
            NoiseLevel.Loud,
            "The door crashes open.",
            contentDamage: 0,
            exhaustion: 0);

        // Assert
        result.Success.Should().BeTrue();
        result.ObstacleDestroyed.Should().BeTrue();
        result.IsCritical.Should().BeFalse();
        result.NoiseLevel.Should().Be(NoiseLevel.Loud);
        result.ConsequencesApplied.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies BruteForceResult failure factory creates correctly.
    /// </summary>
    [Test]
    public void BruteForceResult_FailureFactory_CreatesCorrectly()
    {
        // Act
        var result = BruteForceResult.CreateFailure(
            attemptNumber: 1,
            maxAttempts: 5,
            baseDc: 12,
            retryPenalty: 1,
            "The door holds firm.");

        // Assert
        result.Success.Should().BeFalse();
        result.ObstacleDestroyed.Should().BeFalse();
        result.RetryPossible.Should().BeTrue();
        result.NewDc.Should().Be(13); // 12 + 1
    }

    /// <summary>
    /// Verifies BruteForceResult fumble factory creates correctly.
    /// </summary>
    [Test]
    public void BruteForceResult_FumbleFactory_CreatesCorrectly()
    {
        // Act
        var result = BruteForceResult.CreateFumble(
            attemptNumber: 1,
            maxAttempts: 5,
            baseDc: 12,
            retryPenalty: 1,
            hadTool: true,
            fumbleDamage: 4,
            "You stumble and hurt yourself.");

        // Assert
        result.Success.Should().BeFalse();
        result.IsFumble.Should().BeTrue();
        result.DamageToCharacter.Should().Be(4);
        result.ToolBroken.Should().BeTrue();
        result.NewDc.Should().Be(15); // 12 + 1 + 2 (fumble penalty)
    }

    /// <summary>
    /// Verifies AlternativeMethod has correct data.
    /// </summary>
    [Test]
    public void AlternativeMethod_StaticProperties_HaveCorrectData()
    {
        // Find Key
        var findKey = AlternativeMethod.FindKey;
        findKey.MethodId.Should().Be("alt_door_find_key");
        findKey.MethodName.Should().Be("Find Key");
        findKey.RequiredCheck.Should().Be(AlternativeCheckType.Investigation);
        findKey.CheckDc.Should().Be(14);
        findKey.Prerequisites.Should().BeEmpty();

        // Runic Bypass
        var runic = AlternativeMethod.RunicBypass;
        runic.MethodId.Should().Be("alt_door_runic_bypass");
        runic.RequiredCheck.Should().Be(AlternativeCheckType.Runecraft);
        runic.CheckDc.Should().Be(16);
        runic.Prerequisites.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies AlternativeEvaluationResult factory methods.
    /// </summary>
    [Test]
    public void AlternativeEvaluationResult_FactoryMethods_CreateCorrectly()
    {
        // Ready
        var ready = AlternativeEvaluationResult.Ready("Moderate");
        ready.CanAttempt.Should().BeTrue();
        ready.MissingPrerequisites.Should().BeEmpty();
        ready.EstimatedDifficulty.Should().Be("Moderate");

        // NotReady
        var missing = new List<string> { "[Rune Sight] ability" };
        var notReady = AlternativeEvaluationResult.NotReady(missing.AsReadOnly(), "Moderate");
        notReady.CanAttempt.Should().BeFalse();
        notReady.MissingPrerequisites.Should().HaveCount(1);
    }

    // =========================================================================
    // NULL ARGUMENT VALIDATION TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that constructor throws on null dice service.
    /// </summary>
    [Test]
    public void Constructor_NullDiceService_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AlternativeBypassService(null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("diceService");
    }

    /// <summary>
    /// Verifies that constructor throws on null logger.
    /// </summary>
    [Test]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new AlternativeBypassService(_diceService, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that GetBruteForceOption throws on invalid target type.
    /// </summary>
    [Test]
    public void GetBruteForceOption_InvalidTargetType_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.GetBruteForceOption((BruteForceTargetType)999);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // =========================================================================
    // DISPLAY STRING TESTS
    // =========================================================================

    /// <summary>
    /// Verifies that BruteForceOption has a readable ToDisplayString.
    /// </summary>
    [Test]
    public void BruteForceOption_ToDisplayString_IsReadable()
    {
        // Arrange
        var option = BruteForceOption.SimpleDoor;

        // Act
        var display = option.ToDisplayString();

        // Assert
        display.Should().NotBeNullOrEmpty();
        display.Should().Contain("SimpleDoor");
        display.Should().Contain("DC: 12");
    }

    /// <summary>
    /// Verifies that AlternativeMethod has a readable ToDisplayString.
    /// </summary>
    [Test]
    public void AlternativeMethod_ToDisplayString_IsReadable()
    {
        // Arrange
        var method = AlternativeMethod.RunicBypass;

        // Act
        var display = method.ToDisplayString();

        // Assert
        display.Should().NotBeNullOrEmpty();
        display.Should().Contain("Runic Bypass");
        display.Should().Contain("DC 16");
    }

    /// <summary>
    /// Verifies that BruteForceResult has a readable ToDisplayString.
    /// </summary>
    [Test]
    public void BruteForceResult_ToDisplayString_IsReadable()
    {
        // Arrange
        var result = BruteForceResult.CreateSuccess(
            isCritical: true,
            Array.Empty<AppliedConsequence>(),
            NoiseLevel.Moderate,
            "Door opens cleanly.",
            contentDamage: 0,
            exhaustion: 0);

        // Act
        var display = result.ToDisplayString();

        // Assert
        display.Should().NotBeNullOrEmpty();
        display.Should().Contain("CRITICAL SUCCESS");
        display.Should().Contain("Door opens cleanly.");
    }
}
