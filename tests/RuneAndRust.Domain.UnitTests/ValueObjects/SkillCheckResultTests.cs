using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for SkillCheckResult value object.
/// </summary>
/// <remarks>
/// v0.15.0c: Updated to test success-counting mechanics including
/// NetSuccesses, Outcome (6-tier), Margin (NetSuccesses - DC), and IsFumble.
/// 
/// Key mechanics:
/// - NetSuccesses = max(0, TotalSuccesses - TotalBotches) (always >= 0)
/// - Margin = NetSuccesses - DifficultyClass (can be negative)
/// - ClassifyOutcome uses margin: &lt;0=Failure, 0=Marginal, 1-2=Full, 3-4=Exceptional, 5+=Critical
/// - IsFumble = TotalSuccesses == 0 AND TotalBotches > 0 → always CriticalFailure
/// </remarks>
[TestFixture]
public class SkillCheckResultTests
{
    #region Constructor and Basic Success Tests

    [Test]
    public void Constructor_WhenNetSuccessesEqualDC_ReturnsMarginalSuccess()
    {
        // Arrange: 1 success (8 is a success), DC of 1 → margin 0 = MarginalSuccess
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: 1 success vs DC 1 = margin 0 = MarginalSuccess
        result.NetSuccesses.Should().Be(1);
        result.Margin.Should().Be(0);
        result.Outcome.Should().Be(SkillOutcome.MarginalSuccess);
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenMarginOneOrTwo_ReturnsFullSuccess()
    {
        // Arrange: 2 successes (8 and 10 are both successes), DC of 1 → margin +1 = FullSuccess
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 10 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: 2 successes vs DC 1 = margin +1 = FullSuccess
        result.NetSuccesses.Should().Be(2);
        result.Margin.Should().Be(1);
        result.Outcome.Should().Be(SkillOutcome.FullSuccess);
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenMarginThreeOrFour_ReturnsExceptionalSuccess()
    {
        // Arrange: 4 successes (8, 9, 10, 8), DC of 1 → margin +3 = ExceptionalSuccess
        var dicePool = new DicePool(4, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 9, 10, 8 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: 4 successes vs DC 1 = margin +3 = ExceptionalSuccess
        result.NetSuccesses.Should().Be(4);
        result.Margin.Should().Be(3);
        result.Outcome.Should().Be(SkillOutcome.ExceptionalSuccess);
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenMarginFiveOrMore_ReturnsCriticalSuccess()
    {
        // Arrange: 6 successes, DC of 1 → margin +5 = CriticalSuccess
        var dicePool = new DicePool(6, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 9, 10, 8, 9, 10 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: 6 successes vs DC 1 = margin +5 = CriticalSuccess
        result.NetSuccesses.Should().Be(6);
        result.Margin.Should().Be(5);
        result.Outcome.Should().Be(SkillOutcome.CriticalSuccess);
        result.IsCriticalSuccess.Should().BeTrue();
        result.IsCritical.Should().BeTrue();
    }

    #endregion

    #region Failure Tests

    [Test]
    public void Constructor_WhenNetSuccessesBelowDC_ReturnsFailure()
    {
        // Arrange: 0 successes (5 and 6 are not successes), DC of 1 → margin -1 = Failure
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 5, 6 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: 0 successes vs DC 1 = margin -1 = Failure
        result.NetSuccesses.Should().Be(0);
        result.Margin.Should().Be(-1);
        result.Outcome.Should().Be(SkillOutcome.Failure);
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void Constructor_WhenFumble_ReturnsCriticalFailure()
    {
        // Arrange: 0 successes + botches → Fumble → CriticalFailure
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 1, 3 }); // 0 successes, 1 botch

        // Act
        var result = new SkillCheckResult(
            "stealth", "Stealth",
            diceResult,
            attributeBonus: 10,
            otherBonus: 5,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: Fumble always results in CriticalFailure (bonuses don't help)
        result.IsFumble.Should().BeTrue();
        result.Outcome.Should().Be(SkillOutcome.CriticalFailure);
        result.IsCriticalFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void Constructor_WhenAllNaturalOnes_ReturnsCriticalFailure()
    {
        // Arrange: All 1s = 0 successes + botches = Fumble
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 1, 1 });

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 10,  // High bonus doesn't prevent fumble
            otherBonus: 5,
            difficultyClass: 5,
            difficultyName: "Trivial");

        // Assert: Fumble always results in CriticalFailure regardless of bonuses/DC
        result.IsFumble.Should().BeTrue();
        result.Outcome.Should().Be(SkillOutcome.CriticalFailure);
        result.IsCriticalFailure.Should().BeTrue();
    }

    #endregion

    #region Margin Tests

    [Test]
    public void Margin_CalculatesAsNetSuccessesMinusDC()
    {
        // Arrange: 1 success (9), DC of 3 → margin = 1 - 3 = -2
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 9, 4 }); // 1 success

        // Act
        var result = new SkillCheckResult(
            "stealth", "Stealth",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 3,
            difficultyName: "Challenging");

        // Assert: 1 success - 3 DC = -2 margin = Failure
        result.NetSuccesses.Should().Be(1);
        result.Margin.Should().Be(-2);
        result.IsSuccess.Should().BeFalse();
        result.Outcome.Should().Be(SkillOutcome.Failure);
    }

    [Test]
    public void NetSuccesses_NeverGoesNegative_FlooredAtZero()
    {
        // Arrange: 1 success (8) - 1 botch (1) = 0 net (floored from 0)
        var dicePool = new DicePool(3, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 1, 5 }); // 1 success, 1 botch

        // Act
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 1,
            difficultyName: "Easy");

        // Assert: NetSuccesses = max(0, 1-1) = 0
        result.NetSuccesses.Should().Be(0);
    }

    #endregion

    #region ToString and Display Tests

    [Test]
    public void ToString_FormatsWithSuccessCountingDetails()
    {
        // Arrange
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 9 }); // 2 successes
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 2,
            difficultyName: "Moderate");

        // Act
        var str = result.ToString();

        // Assert: v0.15.0c format includes success-counting info
        str.Should().Contain("Perception");
        str.Should().Contain("DC 2");
        str.Should().Contain("Moderate");
        str.Should().Contain("net"); // Success-counting format
    }

    #endregion

    #region Outcome Extension Method Tests

    [Test]
    public void SkillOutcome_IsSuccess_ReturnsTrueForSuccessOutcomes()
    {
        SkillOutcome.MarginalSuccess.IsSuccess().Should().BeTrue();
        SkillOutcome.FullSuccess.IsSuccess().Should().BeTrue();
        SkillOutcome.ExceptionalSuccess.IsSuccess().Should().BeTrue();
        SkillOutcome.CriticalSuccess.IsSuccess().Should().BeTrue();
    }

    [Test]
    public void SkillOutcome_IsSuccess_ReturnsFalseForFailureOutcomes()
    {
        SkillOutcome.Failure.IsSuccess().Should().BeFalse();
        SkillOutcome.CriticalFailure.IsSuccess().Should().BeFalse();
    }

    [Test]
    public void SkillOutcome_IsCritical_ReturnsTrueForCriticalOutcomes()
    {
        SkillOutcome.CriticalSuccess.IsCritical().Should().BeTrue();
        SkillOutcome.CriticalFailure.IsCritical().Should().BeTrue();
    }

    [Test]
    public void SkillOutcome_GetMarginRange_ReturnsCorrectRanges()
    {
        // Note: GetMarginRange returns (int? Min, int? Max) - nullable tuples
        SkillOutcome.CriticalFailure.GetMarginRange().Should().Be((null, null)); // Fumble-based
        SkillOutcome.Failure.GetMarginRange().Should().Be((null, -1)); // Any negative
        SkillOutcome.MarginalSuccess.GetMarginRange().Should().Be((0, 0)); // Exactly 0
        SkillOutcome.FullSuccess.GetMarginRange().Should().Be((1, 2)); // 1-2
        SkillOutcome.ExceptionalSuccess.GetMarginRange().Should().Be((3, 4)); // 3-4
        SkillOutcome.CriticalSuccess.GetMarginRange().Should().Be((5, null)); // 5+
    }

    #endregion

    #region Legacy Backward Compatibility Tests

    [Test]
    public void TotalResult_PreservesLegacyCalculation()
    {
        // Arrange
        var dicePool = new DicePool(1, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 7 });

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 3,
            otherBonus: 2,
            difficultyClass: 12,
            difficultyName: "Moderate");

        // Assert: Legacy TotalResult still available for backward compatibility
        result.TotalResult.Should().Be(12); // 7 + 3 + 2
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Test]
    public void SuccessLevel_MapsFromOutcome()
    {
        // Arrange: 2 successes vs DC 2 = margin 0 = MarginalSuccess → maps to Success
        var dicePool = new DicePool(2, DiceType.D10);
        var diceResult = new DiceRollResult(dicePool, new[] { 8, 9 }); // 2 successes

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = new SkillCheckResult(
            "perception", "Perception",
            diceResult,
            attributeBonus: 0,
            otherBonus: 0,
            difficultyClass: 2,
            difficultyName: "Moderate");

        // Assert: Legacy SuccessLevel maps from new Outcome (MarginalSuccess → Success)
        result.SuccessLevel.Should().Be(SuccessLevel.Success);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    #endregion
}
