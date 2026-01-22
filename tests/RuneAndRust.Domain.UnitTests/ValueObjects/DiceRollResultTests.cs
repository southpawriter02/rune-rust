using RuneAndRust.Domain.Constants;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class DiceRollResultTests
{
    #region Critical Detection Tests (Legacy - for damage rolls)

    [Test]
    public void IsNaturalMax_WhenFirstDieIsMax_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D10);
#pragma warning disable CS0618
        var result = new DiceRollResult(pool, new[] { 10 });

        Assert.That(result.IsNaturalMax, Is.True);
#pragma warning restore CS0618
    }

    [Test]
    public void IsNaturalMax_WhenFirstDieNotMax_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D10);
#pragma warning disable CS0618
        var result = new DiceRollResult(pool, new[] { 5 });

        Assert.That(result.IsNaturalMax, Is.False);
#pragma warning restore CS0618
    }

    [Test]
    public void IsNaturalOne_WhenFirstDieIsOne_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D10);
#pragma warning disable CS0618
        var result = new DiceRollResult(pool, new[] { 1 });

        Assert.That(result.IsNaturalOne, Is.True);
#pragma warning restore CS0618
    }

    [Test]
    public void IsNaturalOne_WhenFirstDieNotOne_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D10);
#pragma warning disable CS0618
        var result = new DiceRollResult(pool, new[] { 5 });

        Assert.That(result.IsNaturalOne, Is.False);
#pragma warning restore CS0618
    }

    #endregion

    #region Explosion Tests

    [Test]
    public void HadExplosions_WhenExplosionsExist_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D6, exploding: true);
        var result = new DiceRollResult(pool, new[] { 6 }, explosionRolls: new[] { 6, 3 });

        Assert.That(result.HadExplosions, Is.True);
        Assert.That(result.ExplosionCount, Is.EqualTo(2));
    }

    [Test]
    public void HadExplosions_WhenNoExplosions_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D6);
        var result = new DiceRollResult(pool, new[] { 4 });

        Assert.That(result.HadExplosions, Is.False);
        Assert.That(result.ExplosionCount, Is.EqualTo(0));
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsCorrectlyWithSuccessCounting()
    {
        var pool = new DicePool(3, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 8, 2, 9 });  // 2 successes, 0 botches

        var output = result.ToString();

        Assert.That(output, Does.Contain("3d10"));
        Assert.That(output, Does.Contain("[8, 2, 9]"));
        Assert.That(output, Does.Contain("2 successes"));
        Assert.That(output, Does.Contain("0 botches"));
        Assert.That(output, Does.Contain("= 2 net"));
    }

    [Test]
    public void ToString_WithAdvantage_ShowsAllRolls()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(
            pool,
            new[] { 8 },
            AdvantageType.Advantage,
            allRollTotals: new[] { 1, 0 },  // NetSuccesses values
            selectedRollIndex: 0);

        var output = result.ToString();

        Assert.That(output, Does.Contain("ADV"));
        Assert.That(output, Does.Contain("[1, 0]"));
    }

    [Test]
    public void DiceTotal_CalculatesCorrectly()
    {
        var pool = new DicePool(2, DiceType.D6, 5);
        var result = new DiceRollResult(pool, new[] { 4, 3 }, explosionRolls: new[] { 6 });

        Assert.That(result.DiceTotal, Is.EqualTo(13)); // 4 + 3 + 6
        Assert.That(result.RawTotal, Is.EqualTo(13)); // Sum of all dice
        Assert.That(result.Total, Is.EqualTo(18)); // 13 + 5 modifier
    }

    #endregion

    #region Success Counting Tests (v0.15.0a)

    [Test]
    public void TotalSuccesses_CountsDiceShowingEightNineOrTen()
    {
        var pool = new DicePool(5, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 3, 7, 8, 9, 10 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(3)); // 8, 9, 10
    }

    [Test]
    public void TotalSuccesses_ReturnsZero_WhenNoSuccessValues()
    {
        var pool = new DicePool(4, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 2, 5, 7 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(0));
    }

    [Test]
    public void TotalBotches_CountsDiceShowingOne()
    {
        var pool = new DicePool(5, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 4, 1, 8, 1 });

        Assert.That(result.TotalBotches, Is.EqualTo(3));
    }

    [Test]
    public void TotalBotches_ReturnsZero_WhenNoOnes()
    {
        var pool = new DicePool(4, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 2, 5, 8, 10 });

        Assert.That(result.TotalBotches, Is.EqualTo(0));
    }

    [Test]
    public void NetSuccesses_EqualsSuccessesMinusBotches()
    {
        var pool = new DicePool(5, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 8, 9, 1, 10 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(3));
        Assert.That(result.TotalBotches, Is.EqualTo(2));
        Assert.That(result.NetSuccesses, Is.EqualTo(1));
    }

    [Test]
    public void NetSuccesses_MinimumIsZero_WhenBotchesExceedSuccesses()
    {
        var pool = new DicePool(4, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 1, 1, 8 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(1));
        Assert.That(result.TotalBotches, Is.EqualTo(3));
        Assert.That(result.NetSuccesses, Is.EqualTo(0));
    }

    [Test]
    public void IsFumble_TrueWhen_ZeroSuccessesAndAtLeastOneBotch()
    {
        var pool = new DicePool(3, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 4, 2 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(0));
        Assert.That(result.TotalBotches, Is.EqualTo(1));
        Assert.That(result.IsFumble, Is.True);
    }

    [Test]
    public void IsFumble_FalseWhen_HasSuccesses()
    {
        var pool = new DicePool(3, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1, 4, 8 });

        Assert.That(result.TotalSuccesses, Is.EqualTo(1));
        Assert.That(result.TotalBotches, Is.EqualTo(1));
        Assert.That(result.IsFumble, Is.False);
    }

    [Test]
    public void IsCriticalSuccess_TrueWhen_NetSuccessesAtLeastFive()
    {
        var pool = new DicePool(6, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 8, 9, 10, 8, 9, 10 });

        Assert.That(result.NetSuccesses, Is.EqualTo(6));
        Assert.That(result.IsCriticalSuccess, Is.True);
    }

    [Test]
    public void IsCriticalSuccess_FalseWhen_NetSuccessesLessThanFive()
    {
        var pool = new DicePool(4, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 8, 9, 10, 8 });

        Assert.That(result.NetSuccesses, Is.EqualTo(4));
        Assert.That(result.IsCriticalSuccess, Is.False);
    }

    [Test]
    public void TotalDiceEvaluated_IncludesExplosions()
    {
        var pool = new DicePool(2, DiceType.D10, exploding: true);
        var result = new DiceRollResult(pool, new[] { 8, 10 }, explosionRolls: new[] { 5, 10, 3 });

        Assert.That(result.TotalDiceEvaluated, Is.EqualTo(5)); // 2 base + 3 explosions
    }

    [Test]
    public void ExplosionRolls_CountedForSuccesses()
    {
        var pool = new DicePool(1, DiceType.D10, exploding: true);
        var result = new DiceRollResult(pool, new[] { 10 }, explosionRolls: new[] { 9, 8 });

        // 10 + 9 + 8 = 3 successes
        Assert.That(result.TotalSuccesses, Is.EqualTo(3));
    }

    [Test]
    public void ToSumString_FormatsLegacyStyle()
    {
        var pool = new DicePool(3, DiceType.D6, 2);
        var result = new DiceRollResult(pool, new[] { 4, 2, 5 });

        var output = result.ToSumString();

        Assert.That(output, Does.Contain("3d6+2"));
        Assert.That(output, Does.Contain("[4, 2, 5]"));
        Assert.That(output, Does.Contain("= 13")); // 4+2+5+2
    }

    #endregion
}
