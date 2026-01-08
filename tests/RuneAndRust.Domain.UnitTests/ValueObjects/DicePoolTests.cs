using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class DicePoolTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_WithValidCount_CreatesDicePool()
    {
        var pool = new DicePool(3, DiceType.D6, 5);

        Assert.That(pool.Count, Is.EqualTo(3));
        Assert.That(pool.DiceType, Is.EqualTo(DiceType.D6));
        Assert.That(pool.Modifier, Is.EqualTo(5));
        Assert.That(pool.Faces, Is.EqualTo(6));
    }

    [Test]
    public void Constructor_WithZeroCount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DicePool(0, DiceType.D6));
    }

    [Test]
    public void Constructor_WithNegativeCount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DicePool(-1, DiceType.D6));
    }

    [Test]
    public void Constructor_WithNegativeMaxExplosions_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DicePool(1, DiceType.D6, 0, true, -1));
    }

    #endregion

    #region Calculation Tests

    [Test]
    public void MinimumResult_CalculatesCorrectly()
    {
        var pool = new DicePool(3, DiceType.D6, 5);

        Assert.That(pool.MinimumResult, Is.EqualTo(8)); // 3*1 + 5
    }

    [Test]
    public void MaximumResult_CalculatesCorrectly()
    {
        var pool = new DicePool(3, DiceType.D6, 5);

        Assert.That(pool.MaximumResult, Is.EqualTo(23)); // 3*6 + 5
    }

    [Test]
    public void AverageResult_CalculatesCorrectly()
    {
        var pool = new DicePool(3, DiceType.D6, 5);

        Assert.That(pool.AverageResult, Is.EqualTo(15.5f)); // 3*3.5 + 5
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsPositiveModifier()
    {
        var pool = new DicePool(3, DiceType.D6, 5);

        Assert.That(pool.ToString(), Is.EqualTo("3d6+5"));
    }

    [Test]
    public void ToString_FormatsNegativeModifier()
    {
        var pool = new DicePool(2, DiceType.D8, -3);

        Assert.That(pool.ToString(), Is.EqualTo("2d8-3"));
    }

    [Test]
    public void ToString_FormatsExploding()
    {
        var pool = new DicePool(1, DiceType.D6, 0, exploding: true);

        Assert.That(pool.ToString(), Is.EqualTo("1d6!"));
    }

    #endregion

    #region Parse Tests

    [Test]
    public void Parse_ValidNotation_CreatesDicePool()
    {
        var pool = DicePool.Parse("3d6+5");

        Assert.That(pool.Count, Is.EqualTo(3));
        Assert.That(pool.DiceType, Is.EqualTo(DiceType.D6));
        Assert.That(pool.Modifier, Is.EqualTo(5));
    }

    [Test]
    public void Parse_NegativeModifier_ParsesCorrectly()
    {
        var pool = DicePool.Parse("2d8-3");

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.DiceType, Is.EqualTo(DiceType.D8));
        Assert.That(pool.Modifier, Is.EqualTo(-3));
    }

    [Test]
    public void Parse_ExplodingNotation_SetsFlag()
    {
        var pool = DicePool.Parse("1d6!");

        Assert.That(pool.Exploding, Is.True);
    }

    [Test]
    public void Parse_OmittedCount_DefaultsToOne()
    {
        var pool = DicePool.Parse("d10");

        Assert.That(pool.Count, Is.EqualTo(1));
        Assert.That(pool.DiceType, Is.EqualTo(DiceType.D10));
    }

    [Test]
    public void Parse_InvalidNotation_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => DicePool.Parse("invalid"));
    }

    [Test]
    public void Parse_UnsupportedDiceType_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() => DicePool.Parse("1d20"));
        Assert.That(ex.Message, Does.Contain("Unsupported dice type"));
    }

    [Test]
    public void TryParse_InvalidNotation_ReturnsFalse()
    {
        var result = DicePool.TryParse("invalid", out var pool);

        Assert.That(result, Is.False);
    }

    #endregion

    #region Static Factory Tests

    [Test]
    public void D10_CreatesD10Pool()
    {
        var pool = DicePool.D10(2, 5);

        Assert.That(pool.Count, Is.EqualTo(2));
        Assert.That(pool.DiceType, Is.EqualTo(DiceType.D10));
        Assert.That(pool.Modifier, Is.EqualTo(5));
    }

    [Test]
    public void WithExploding_CreatesExplodingPool()
    {
        var pool = DicePool.D6().WithExploding();

        Assert.That(pool.Exploding, Is.True);
        Assert.That(pool.MaxExplosions, Is.EqualTo(10));
    }

    [Test]
    public void WithModifier_ChangesModifier()
    {
        var pool = DicePool.D6(3).WithModifier(10);

        Assert.That(pool.Modifier, Is.EqualTo(10));
    }

    #endregion
}
