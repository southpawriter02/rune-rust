using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class DiceRollResultTests
{
    #region Critical Detection Tests

    [Test]
    public void IsNaturalMax_WhenFirstDieIsMax_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 10 }, 10);

        Assert.That(result.IsNaturalMax, Is.True);
    }

    [Test]
    public void IsNaturalMax_WhenFirstDieNotMax_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 5 }, 5);

        Assert.That(result.IsNaturalMax, Is.False);
    }

    [Test]
    public void IsNaturalOne_WhenFirstDieIsOne_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 1 }, 1);

        Assert.That(result.IsNaturalOne, Is.True);
    }

    [Test]
    public void IsNaturalOne_WhenFirstDieNotOne_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(pool, new[] { 5 }, 5);

        Assert.That(result.IsNaturalOne, Is.False);
    }

    #endregion

    #region Explosion Tests

    [Test]
    public void HadExplosions_WhenExplosionsExist_ReturnsTrue()
    {
        var pool = new DicePool(1, DiceType.D6, exploding: true);
        var result = new DiceRollResult(pool, new[] { 6 }, 15, explosionRolls: new[] { 6, 3 });

        Assert.That(result.HadExplosions, Is.True);
        Assert.That(result.ExplosionCount, Is.EqualTo(2));
    }

    [Test]
    public void HadExplosions_WhenNoExplosions_ReturnsFalse()
    {
        var pool = new DicePool(1, DiceType.D6);
        var result = new DiceRollResult(pool, new[] { 4 }, 4);

        Assert.That(result.HadExplosions, Is.False);
        Assert.That(result.ExplosionCount, Is.EqualTo(0));
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsCorrectly()
    {
        var pool = new DicePool(3, DiceType.D6, 2);
        var result = new DiceRollResult(pool, new[] { 4, 2, 5 }, 13);

        var output = result.ToString();

        Assert.That(output, Does.Contain("3d6+2"));
        Assert.That(output, Does.Contain("[4, 2, 5]"));
        Assert.That(output, Does.Contain("= 13"));
    }

    [Test]
    public void ToString_WithAdvantage_ShowsAllRolls()
    {
        var pool = new DicePool(1, DiceType.D10);
        var result = new DiceRollResult(
            pool,
            new[] { 8 },
            8,
            AdvantageType.Advantage,
            allRollTotals: new[] { 8, 5 },
            selectedRollIndex: 0);

        var output = result.ToString();

        Assert.That(output, Does.Contain("ADV"));
        Assert.That(output, Does.Contain("[8, 5]"));
    }

    [Test]
    public void DiceTotal_CalculatesCorrectly()
    {
        var pool = new DicePool(2, DiceType.D6, 5);
        var result = new DiceRollResult(pool, new[] { 4, 3 }, 12, explosionRolls: new[] { 6 });

        Assert.That(result.DiceTotal, Is.EqualTo(13)); // 4 + 3 + 6
        Assert.That(result.Total, Is.EqualTo(12)); // As provided
    }

    #endregion
}
