using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class DiceServiceTests
{
    private Mock<ILogger<DiceService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<DiceService>>();
    }

    #region Range Tests

    [Test]
    public void Roll_SingleD10_ReturnsValidRange()
    {
        var service = new DiceService(_mockLogger.Object);
        var results = Enumerable.Range(0, 100)
            .Select(_ => service.Roll(DicePool.D10()))
            .ToList();

        Assert.That(results, Has.All.Matches<DiceRollResult>(r =>
            r.Total >= 1 && r.Total <= 10 && r.Rolls.Count == 1));
    }

    [Test]
    public void Roll_3d6Plus5_ReturnsCorrectRange()
    {
        var service = new DiceService(_mockLogger.Object);
        var pool = new DicePool(3, DiceType.D6, 5);

        var results = Enumerable.Range(0, 100)
            .Select(_ => service.Roll(pool))
            .ToList();

        // Min: 3*1 + 5 = 8, Max: 3*6 + 5 = 23
        Assert.That(results, Has.All.Matches<DiceRollResult>(r =>
            r.Total >= 8 && r.Total <= 23));
    }

    #endregion

    #region Advantage/Disadvantage Tests

    [Test]
    public void Roll_WithAdvantage_RollsTwiceAndTakesHigher()
    {
        // Use seeded random for deterministic result
        var seededRandom = new Random(42);
        var service = new DiceService(_mockLogger.Object, seededRandom);

        var result = service.Roll(DicePool.D10(), AdvantageType.Advantage);

        Assert.That(result.AdvantageType, Is.EqualTo(AdvantageType.Advantage));
        Assert.That(result.AllRollTotals, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(result.AllRollTotals.Max()));
    }

    [Test]
    public void Roll_WithDisadvantage_RollsTwiceAndTakesLower()
    {
        var seededRandom = new Random(42);
        var service = new DiceService(_mockLogger.Object, seededRandom);

        var result = service.Roll(DicePool.D10(), AdvantageType.Disadvantage);

        Assert.That(result.AdvantageType, Is.EqualTo(AdvantageType.Disadvantage));
        Assert.That(result.AllRollTotals, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(result.AllRollTotals.Min()));
    }

    #endregion

    #region Exploding Dice Tests

    [Test]
    public void Roll_ExplodingDice_CanExplode()
    {
        // Roll many times to ensure at least one explosion
        var service = new DiceService(_mockLogger.Object);
        var pool = DicePool.D6().WithExploding();

        var results = Enumerable.Range(0, 1000)
            .Select(_ => service.Roll(pool))
            .ToList();

        // With 1000 rolls of 1d6, we should get some explosions
        Assert.That(results.Any(r => r.HadExplosions), Is.True);
    }

    [Test]
    public void Roll_ExplodingDice_RespectsMaxExplosions()
    {
        // Create a pool with max 2 explosions
        var pool = new DicePool(1, DiceType.D6, 0, true, maxExplosions: 2);
        var service = new DiceService(_mockLogger.Object);

        // Roll many times 
        var results = Enumerable.Range(0, 1000)
            .Select(_ => service.Roll(pool))
            .ToList();

        // No result should have more than 2 explosions
        Assert.That(results, Has.All.Matches<DiceRollResult>(r => r.ExplosionCount <= 2));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void Roll_FromNotation_ParsesAndRolls()
    {
        var service = new DiceService(_mockLogger.Object);

        var result = service.Roll("2d6+3");

        Assert.That(result.Pool.Count, Is.EqualTo(2));
        Assert.That(result.Pool.DiceType, Is.EqualTo(DiceType.D6));
        Assert.That(result.Pool.Modifier, Is.EqualTo(3));
        Assert.That(result.Total, Is.InRange(5, 15)); // 2*1+3 to 2*6+3
    }

    [Test]
    public void Roll_InvalidNotation_ThrowsFormatException()
    {
        var service = new DiceService(_mockLogger.Object);

        Assert.Throws<FormatException>(() => service.Roll("invalid"));
    }

    #endregion

    #region Convenience Method Tests

    [Test]
    public void RollTotal_ReturnsJustTotal()
    {
        var service = new DiceService(_mockLogger.Object);

        var total = service.RollTotal(DicePool.D10());

        Assert.That(total, Is.InRange(1, 10));
    }

    [Test]
    public void Roll_DiceType_Overload_Works()
    {
        var service = new DiceService(_mockLogger.Object);

        var result = service.Roll(DiceType.D8, count: 2, modifier: 1);

        Assert.That(result.Pool.Count, Is.EqualTo(2));
        Assert.That(result.Pool.DiceType, Is.EqualTo(DiceType.D8));
        Assert.That(result.Pool.Modifier, Is.EqualTo(1));
    }

    #endregion

    #region Determinism Tests

    [Test]
    public void Roll_WithSeededRandom_ProducesDeterministicResult()
    {
        var seededRandom1 = new Random(42);
        var service1 = new DiceService(_mockLogger.Object, seededRandom1);
        var result1 = service1.Roll(DicePool.D10());

        var seededRandom2 = new Random(42);
        var service2 = new DiceService(_mockLogger.Object, seededRandom2);
        var result2 = service2.Roll(DicePool.D10());

        Assert.That(result1.Total, Is.EqualTo(result2.Total));
        Assert.That(result1.Rolls[0], Is.EqualTo(result2.Rolls[0]));
    }

    #endregion
}
