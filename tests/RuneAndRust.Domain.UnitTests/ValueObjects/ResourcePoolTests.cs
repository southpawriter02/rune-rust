using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class ResourcePoolTests
{
    [Test]
    public void Constructor_StartsAtMax_WhenStartAtZeroIsFalse()
    {
        var pool = new ResourcePool("mana", 100, startAtZero: false);

        Assert.That(pool.Current, Is.EqualTo(100));
        Assert.That(pool.Maximum, Is.EqualTo(100));
    }

    [Test]
    public void Constructor_StartsAtZero_WhenStartAtZeroIsTrue()
    {
        var pool = new ResourcePool("rage", 100, startAtZero: true);

        Assert.That(pool.Current, Is.EqualTo(0));
    }

    [Test]
    public void Spend_ReturnsTrueAndDeductAmount_WhenSufficient()
    {
        var pool = new ResourcePool("mana", 100);
        var result = pool.Spend(25);

        Assert.That(result, Is.True);
        Assert.That(pool.Current, Is.EqualTo(75));
    }

    [Test]
    public void Spend_ReturnsFalse_WhenInsufficient()
    {
        var pool = new ResourcePool("mana", 100);
        pool.Spend(90);
        var result = pool.Spend(20);

        Assert.That(result, Is.False);
        Assert.That(pool.Current, Is.EqualTo(10));
    }

    [Test]
    public void Gain_IncreasesCurrentUpToMax()
    {
        var pool = new ResourcePool("mana", 100);
        pool.Spend(30);
        var gained = pool.Gain(50);

        Assert.That(gained, Is.EqualTo(30));
        Assert.That(pool.Current, Is.EqualTo(100));
    }

    [Test]
    public void Lose_DecreasesCurrent_DownToZero()
    {
        var pool = new ResourcePool("rage", 100, startAtZero: false);
        pool.SetCurrent(15);
        var lost = pool.Lose(20);

        Assert.That(lost, Is.EqualTo(15));
        Assert.That(pool.Current, Is.EqualTo(0));
    }

    [Test]
    public void IsFull_ReturnsTrue_WhenCurrentEqualsMax()
    {
        var pool = new ResourcePool("mana", 100);
        Assert.That(pool.IsFull, Is.True);
    }

    [Test]
    public void IsEmpty_ReturnsTrue_WhenCurrentIsZero()
    {
        var pool = new ResourcePool("rage", 100, startAtZero: true);
        Assert.That(pool.IsEmpty, Is.True);
    }

    [Test]
    public void Percentage_ReturnsCorrectRatio()
    {
        var pool = new ResourcePool("mana", 100);
        pool.Spend(50);

        Assert.That(pool.Percentage, Is.EqualTo(0.5f));
    }

    [Test]
    public void SetMaximum_AdjustsProportionally_WhenEnabled()
    {
        var pool = new ResourcePool("mana", 100);
        pool.Spend(50); // 50/100
        pool.SetMaximum(200, adjustCurrentProportionally: true);

        Assert.That(pool.Current, Is.EqualTo(100)); // 50% of 200
        Assert.That(pool.Maximum, Is.EqualTo(200));
    }
}
