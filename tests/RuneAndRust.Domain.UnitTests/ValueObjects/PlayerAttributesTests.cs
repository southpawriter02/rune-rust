using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class PlayerAttributesTests
{
    [Test]
    public void Constructor_ValidValues_CreatesInstance()
    {
        var attrs = new PlayerAttributes(10, 12, 8, 14, 10);
        Assert.That(attrs.Might, Is.EqualTo(10));
        Assert.That(attrs.Fortitude, Is.EqualTo(12));
        Assert.That(attrs.Will, Is.EqualTo(8));
        Assert.That(attrs.Wits, Is.EqualTo(14));
        Assert.That(attrs.Finesse, Is.EqualTo(10));
    }

    [Test]
    public void Default_ReturnsAllEight()
    {
        var attrs = PlayerAttributes.Default;
        Assert.That(attrs.Might, Is.EqualTo(8));
        Assert.That(attrs.Fortitude, Is.EqualTo(8));
        Assert.That(attrs.Will, Is.EqualTo(8));
        Assert.That(attrs.Wits, Is.EqualTo(8));
        Assert.That(attrs.Finesse, Is.EqualTo(8));
    }

    [Test]
    public void Constructor_ValueBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerAttributes(0, 8, 8, 8, 8));
    }

    [Test]
    public void Constructor_ValueAboveThirty_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerAttributes(31, 8, 8, 8, 8));
    }

    [Test]
    public void GetByName_ValidName_ReturnsValue()
    {
        var attrs = new PlayerAttributes(10, 12, 8, 14, 16);
        Assert.That(attrs.GetByName("might"), Is.EqualTo(10));
        Assert.That(attrs.GetByName("MIG"), Is.EqualTo(10));
        Assert.That(attrs.GetByName("fortitude"), Is.EqualTo(12));
        Assert.That(attrs.GetByName("finesse"), Is.EqualTo(16));
    }

    [Test]
    public void GetByName_InvalidName_ThrowsArgumentException()
    {
        var attrs = PlayerAttributes.Default;
        Assert.Throws<ArgumentException>(() => attrs.GetByName("strength"));
    }

    [Test]
    public void CalculatePointCost_AllBaseValues_ReturnsZero()
    {
        var attrs = new PlayerAttributes(8, 8, 8, 8, 8);
        Assert.That(attrs.CalculatePointCost(), Is.EqualTo(0));
    }

    [Test]
    public void CalculatePointCost_AllTens_ReturnsCorrectCost()
    {
        // Each 10 costs 2 points (10 - 8 = 2)
        var attrs = new PlayerAttributes(10, 10, 10, 10, 10);
        Assert.That(attrs.CalculatePointCost(), Is.EqualTo(10)); // 2 * 5
    }

    [Test]
    public void CalculatePointCost_FifteenCostsExtraPoint()
    {
        // 14 costs 6 points, 15 costs 8 points (6 + 2)
        var attrs = new PlayerAttributes(15, 8, 8, 8, 8);
        Assert.That(attrs.CalculatePointCost(), Is.EqualTo(8));
    }

    [Test]
    public void IsWithinBudget_UnderBudget_ReturnsTrue()
    {
        var attrs = new PlayerAttributes(10, 10, 10, 10, 10); // 10 points
        Assert.That(attrs.IsWithinBudget(25), Is.True);
    }

    [Test]
    public void IsWithinBudget_OverBudget_ReturnsFalse()
    {
        var attrs = new PlayerAttributes(15, 15, 15, 15, 8); // 32 points
        Assert.That(attrs.IsWithinBudget(25), Is.False);
    }

    [Test]
    public void WithModifiers_AppliesPositiveModifiers()
    {
        var attrs = new PlayerAttributes(10, 10, 10, 10, 10);
        var modifiers = new Dictionary<string, int> { ["might"] = 2, ["wits"] = 1 };
        
        var result = attrs.WithModifiers(modifiers);
        
        Assert.That(result.Might, Is.EqualTo(12));
        Assert.That(result.Wits, Is.EqualTo(11));
        Assert.That(result.Fortitude, Is.EqualTo(10)); // unchanged
    }

    [Test]
    public void WithModifiers_AppliesNegativeModifiers()
    {
        var attrs = new PlayerAttributes(10, 10, 10, 10, 10);
        var modifiers = new Dictionary<string, int> { ["finesse"] = -2 };
        
        var result = attrs.WithModifiers(modifiers);
        
        Assert.That(result.Finesse, Is.EqualTo(8));
    }

    [Test]
    public void WithModifiers_ClampsToMinimum()
    {
        var attrs = new PlayerAttributes(5, 8, 8, 8, 8);
        var modifiers = new Dictionary<string, int> { ["might"] = -10 };
        
        var result = attrs.WithModifiers(modifiers);
        
        Assert.That(result.Might, Is.EqualTo(1)); // clamped to minimum
    }

    [Test]
    public void WithModifiers_ClampsToMaximum()
    {
        var attrs = new PlayerAttributes(28, 8, 8, 8, 8);
        var modifiers = new Dictionary<string, int> { ["might"] = 10 };
        
        var result = attrs.WithModifiers(modifiers);
        
        Assert.That(result.Might, Is.EqualTo(30)); // clamped to maximum
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var attrs = new PlayerAttributes(10, 12, 8, 14, 16);
        var result = attrs.ToString();
        Assert.That(result, Contains.Substring("MIG:10"));
        Assert.That(result, Contains.Substring("FOR:12"));
        Assert.That(result, Contains.Substring("FIN:16"));
    }
}
