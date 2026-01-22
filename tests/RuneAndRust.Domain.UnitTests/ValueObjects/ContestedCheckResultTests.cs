using FluentAssertions;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

/// <summary>
/// Unit tests for ContestedCheckResult value object.
/// </summary>
/// <remarks>
/// v0.15.0d: Tests contested check outcome determination including
/// fumble handling, net success comparison, and tie resolution.
/// </remarks>
[TestFixture]
public class ContestedCheckResultTests
{
    #region DetermineOutcome Tests

    [Test]
    public void DetermineOutcome_InitiatorHigherNet_ReturnsInitiatorWins()
    {
        // Arrange: Initiator has 3 net, defender has 1 net
        var initiatorRoll = CreateRollResult(netSuccesses: 3, isFumble: false);
        var defenderRoll = CreateRollResult(netSuccesses: 1, isFumble: false);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.InitiatorWins);
        margin.Should().Be(2); // 3 - 1 = 2
    }

    [Test]
    public void DetermineOutcome_DefenderHigherNet_ReturnsDefenderWins()
    {
        // Arrange: Initiator has 1 net, defender has 3 net
        var initiatorRoll = CreateRollResult(netSuccesses: 1, isFumble: false);
        var defenderRoll = CreateRollResult(netSuccesses: 3, isFumble: false);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.DefenderWins);
        margin.Should().Be(2); // |1 - 3| = 2
    }

    [Test]
    public void DetermineOutcome_EqualNet_ReturnsTie()
    {
        // Arrange: Both have 2 net successes
        var initiatorRoll = CreateRollResult(netSuccesses: 2, isFumble: false);
        var defenderRoll = CreateRollResult(netSuccesses: 2, isFumble: false);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.Tie);
        margin.Should().Be(0);
    }

    [Test]
    public void DetermineOutcome_InitiatorFumbles_ReturnsInitiatorFumble()
    {
        // Arrange: Initiator fumbles, defender has 2 net
        var initiatorRoll = CreateRollResult(netSuccesses: 0, isFumble: true);
        var defenderRoll = CreateRollResult(netSuccesses: 2, isFumble: false);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.InitiatorFumble);
        margin.Should().Be(2); // Defender's net successes
    }

    [Test]
    public void DetermineOutcome_DefenderFumbles_ReturnsDefenderFumble()
    {
        // Arrange: Defender fumbles, initiator has 3 net
        var initiatorRoll = CreateRollResult(netSuccesses: 3, isFumble: false);
        var defenderRoll = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.DefenderFumble);
        margin.Should().Be(3); // Initiator's net successes
    }

    [Test]
    public void DetermineOutcome_BothFumble_ReturnsBothFumble()
    {
        // Arrange: Both fumble
        var initiatorRoll = CreateRollResult(netSuccesses: 0, isFumble: true);
        var defenderRoll = CreateRollResult(netSuccesses: 0, isFumble: true);

        // Act
        var (outcome, margin) = ContestedCheckResult.DetermineOutcome(initiatorRoll, defenderRoll);

        // Assert
        outcome.Should().Be(ContestedOutcome.BothFumble);
        margin.Should().Be(0);
    }

    #endregion

    #region Computed Properties Tests

    [Test]
    public void WinnerId_ReturnsCorrectWinner_ForEachOutcome()
    {
        // Test InitiatorWins
        var resultInitWins = new ContestedCheckResult
        {
            InitiatorId = "player",
            DefenderId = "npc",
            Outcome = ContestedOutcome.InitiatorWins
        };
        resultInitWins.WinnerId.Should().Be("player");
        resultInitWins.HasWinner.Should().BeTrue();

        // Test DefenderWins
        var resultDefWins = new ContestedCheckResult
        {
            InitiatorId = "player",
            DefenderId = "npc",
            Outcome = ContestedOutcome.DefenderWins
        };
        resultDefWins.WinnerId.Should().Be("npc");
        resultDefWins.HasWinner.Should().BeTrue();

        // Test InitiatorFumble (defender wins)
        var resultInitFumble = new ContestedCheckResult
        {
            InitiatorId = "player",
            DefenderId = "npc",
            Outcome = ContestedOutcome.InitiatorFumble
        };
        resultInitFumble.WinnerId.Should().Be("npc");
        resultInitFumble.HadFumble.Should().BeTrue();

        // Test DefenderFumble (initiator wins)
        var resultDefFumble = new ContestedCheckResult
        {
            InitiatorId = "player",
            DefenderId = "npc",
            Outcome = ContestedOutcome.DefenderFumble
        };
        resultDefFumble.WinnerId.Should().Be("player");
        resultDefFumble.HadFumble.Should().BeTrue();

        // Test Tie
        var resultTie = new ContestedCheckResult
        {
            Outcome = ContestedOutcome.Tie
        };
        resultTie.WinnerId.Should().BeNull();
        resultTie.HasWinner.Should().BeFalse();
        resultTie.IsTie.Should().BeTrue();

        // Test BothFumble
        var resultBothFumble = new ContestedCheckResult
        {
            Outcome = ContestedOutcome.BothFumble
        };
        resultBothFumble.WinnerId.Should().BeNull();
        resultBothFumble.HasWinner.Should().BeFalse();
        resultBothFumble.IsMutualFumble.Should().BeTrue();
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var initiatorRoll = CreateRollResult(netSuccesses: 3, isFumble: false);
        var defenderRoll = CreateRollResult(netSuccesses: 1, isFumble: false);
        var result = new ContestedCheckResult
        {
            InitiatorId = "player",
            DefenderId = "npc",
            InitiatorSkillId = "stealth",
            DefenderSkillId = "perception",
            InitiatorRoll = initiatorRoll,
            DefenderRoll = defenderRoll,
            Outcome = ContestedOutcome.InitiatorWins,
            Margin = 2
        };

        // Act
        var str = result.ToString();

        // Assert
        str.Should().Contain("stealth");
        str.Should().Contain("perception");
        str.Should().Contain("InitiatorWins");
        str.Should().Contain("margin: 2");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock DiceRollResult for testing.
    /// </summary>
    private static DiceRollResult CreateRollResult(int netSuccesses, bool isFumble)
    {
        // Create dice values that produce the desired result
        int[] values;
        if (isFumble)
        {
            // Fumble: 0 successes, at least 1 botch
            values = new[] { 1 }; // Single 1 = fumble
        }
        else if (netSuccesses == 0)
        {
            // 0 net but not fumble
            values = new[] { 5 }; // No success, no botch
        }
        else
        {
            // Create dice showing successes (8+)
            values = Enumerable.Range(0, netSuccesses).Select(_ => 8).ToArray();
        }

        var pool = new DicePool(values.Length, DiceType.D10);
        return new DiceRollResult(pool, values);
    }

    #endregion
}
