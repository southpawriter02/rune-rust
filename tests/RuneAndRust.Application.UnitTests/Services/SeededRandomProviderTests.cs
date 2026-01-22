using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="SeededRandomProvider"/>.
/// </summary>
/// <remarks>
/// Tests cover deterministic sequence generation, save/restore state,
/// batch generation, and seed management.
/// </remarks>
[TestFixture]
public class SeededRandomProviderTests
{
    private Mock<ILogger<SeededRandomProvider>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SeededRandomProvider>>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DETERMINISTIC SEQUENCE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void Next_WithSameSeed_ProducesIdenticalSequence()
    {
        // Arrange
        var provider1 = new SeededRandomProvider(42, _mockLogger.Object);
        var provider2 = new SeededRandomProvider(42, _mockLogger.Object);

        // Act
        var sequence1 = Enumerable.Range(0, 10).Select(_ => provider1.Next(1, 11)).ToArray();
        var sequence2 = Enumerable.Range(0, 10).Select(_ => provider2.Next(1, 11)).ToArray();

        // Assert
        sequence2.Should().BeEquivalentTo(sequence1, options => options.WithStrictOrdering());
    }

    [Test]
    public void Next_WithDifferentSeeds_ProducesDifferentSequences()
    {
        // Arrange
        var provider1 = new SeededRandomProvider(42, _mockLogger.Object);
        var provider2 = new SeededRandomProvider(12345, _mockLogger.Object);

        // Act
        var sequence1 = Enumerable.Range(0, 10).Select(_ => provider1.Next(1, 11)).ToArray();
        var sequence2 = Enumerable.Range(0, 10).Select(_ => provider2.Next(1, 11)).ToArray();

        // Assert
        sequence2.Should().NotBeEquivalentTo(sequence1);
    }

    [Test]
    public void Next_ReturnsValuesInSpecifiedRange()
    {
        // Arrange
        var provider = new SeededRandomProvider(42, _mockLogger.Object);

        // Act
        var results = Enumerable.Range(0, 100).Select(_ => provider.Next(1, 11)).ToArray();

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeInRange(1, 10));
    }

    [Test]
    public void Next_WithInvalidRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);

        // Act
        var act = () => provider.Next(10, 5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SAVE/RESTORE STATE TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void SaveState_RestoreState_ReplaysSameSequence()
    {
        // Arrange
        var provider = new SeededRandomProvider(42, _mockLogger.Object);
        provider.SaveState();

        // Generate some values
        var firstRun = Enumerable.Range(0, 5).Select(_ => provider.Next(1, 11)).ToArray();

        // Restore and generate again
        provider.RestoreState();
        var secondRun = Enumerable.Range(0, 5).Select(_ => provider.Next(1, 11)).ToArray();

        // Assert
        secondRun.Should().BeEquivalentTo(firstRun, options => options.WithStrictOrdering());
    }

    [Test]
    public void RestoreState_WithoutPriorSave_RestoresToInitialSeed()
    {
        // Arrange
        var provider = new SeededRandomProvider(42, _mockLogger.Object);

        // Generate some values to advance state
        _ = provider.Next(1, 11);
        _ = provider.Next(1, 11);
        _ = provider.Next(1, 11);

        // Restore without prior save
        provider.RestoreState();

        // Generate sequence from initial seed
        var afterRestore = Enumerable.Range(0, 3).Select(_ => provider.Next(1, 11)).ToArray();

        // Create fresh provider with same seed
        var fresh = new SeededRandomProvider(42);
        var expected = Enumerable.Range(0, 3).Select(_ => fresh.Next(1, 11)).ToArray();

        // Assert - should match fresh provider
        afterRestore.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NEXT MANY TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void NextMany_ReturnsCorrectCount()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);

        // Act
        var results = provider.NextMany(5, 1, 11);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Should().BeInRange(1, 10));
    }

    [Test]
    public void NextMany_WithSameSeed_ProducesIdenticalArray()
    {
        // Arrange
        var provider1 = new SeededRandomProvider(42);
        var provider2 = new SeededRandomProvider(42);

        // Act
        var results1 = provider1.NextMany(10, 1, 11);
        var results2 = provider2.NextMany(10, 1, 11);

        // Assert
        results2.Should().BeEquivalentTo(results1, options => options.WithStrictOrdering());
    }

    [Test]
    public void NextMany_WithZeroCount_ReturnsEmptyArray()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);

        // Act
        var results = provider.NextMany(0, 1, 11);

        // Assert
        results.Should().BeEmpty();
    }

    [Test]
    public void NextMany_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);

        // Act
        var act = () => provider.NextMany(-1, 1, 11);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SEED MANAGEMENT TESTS
    // ═══════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetCurrentSeed_ReturnsInitialSeed()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);

        // Act
        var seed = provider.GetCurrentSeed();

        // Assert
        seed.Should().Be(42);
    }

    [Test]
    public void SetSeed_UpdatesSeedAndResetsSequence()
    {
        // Arrange
        var provider = new SeededRandomProvider(42);
        _ = provider.Next(1, 11); // Advance state

        // Act
        provider.SetSeed(12345);
        var afterSetSeed = provider.Next(1, 11);

        // Assert
        provider.GetCurrentSeed().Should().Be(12345);

        // Verify sequence matches fresh provider with same seed
        var fresh = new SeededRandomProvider(12345);
        var expected = fresh.Next(1, 11);
        afterSetSeed.Should().Be(expected);
    }

    [Test]
    public void Constructor_WithNullSeed_GeneratesTimeSeed()
    {
        // Arrange & Act
        var provider1 = new SeededRandomProvider();
        var provider2 = new SeededRandomProvider();

        // Assert - both should have some seed (may or may not differ depending on timing)
        provider1.GetCurrentSeed().Should().NotBe(0);
        provider2.GetCurrentSeed().Should().NotBe(0);
    }
}
