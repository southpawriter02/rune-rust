using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.UnitTests.ValueObjects;

[TestFixture]
public class StatModifiersTests
{
    [Test]
    public void None_ReturnsAllZeros()
    {
        var modifiers = StatModifiers.None;

        Assert.That(modifiers.MaxHealth, Is.EqualTo(0));
        Assert.That(modifiers.Attack, Is.EqualTo(0));
        Assert.That(modifiers.Defense, Is.EqualTo(0));
        Assert.That(modifiers.Might, Is.EqualTo(0));
    }

    [Test]
    public void ApplyTo_AddsModifiersToBaseStats()
    {
        var baseStats = new Stats(100, 10, 5);
        var modifiers = new StatModifiers { MaxHealth = 20, Attack = -2, Defense = 5 };

        var result = modifiers.ApplyTo(baseStats);

        Assert.That(result.MaxHealth, Is.EqualTo(120));
        Assert.That(result.Attack, Is.EqualTo(8));
        Assert.That(result.Defense, Is.EqualTo(10));
    }

    [Test]
    public void ApplyTo_EnsuresMinimumHealth()
    {
        var baseStats = new Stats(10, 5, 5);
        var modifiers = new StatModifiers { MaxHealth = -100 };

        var result = modifiers.ApplyTo(baseStats);

        Assert.That(result.MaxHealth, Is.EqualTo(1));
    }

    [Test]
    public void ApplyTo_EnsuresNonNegativeAttackAndDefense()
    {
        var baseStats = new Stats(100, 5, 5);
        var modifiers = new StatModifiers { Attack = -10, Defense = -10 };

        var result = modifiers.ApplyTo(baseStats);

        Assert.That(result.Attack, Is.EqualTo(0));
        Assert.That(result.Defense, Is.EqualTo(0));
    }

    [Test]
    public void AdditionOperator_CombinesModifiers()
    {
        var a = new StatModifiers { MaxHealth = 10, Attack = 5, Might = 2 };
        var b = new StatModifiers { MaxHealth = 5, Attack = -2, Will = 3 };

        var result = a + b;

        Assert.That(result.MaxHealth, Is.EqualTo(15));
        Assert.That(result.Attack, Is.EqualTo(3));
        Assert.That(result.Might, Is.EqualTo(2));
        Assert.That(result.Will, Is.EqualTo(3));
    }
}
