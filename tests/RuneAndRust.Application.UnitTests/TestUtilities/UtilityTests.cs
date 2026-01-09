using FluentAssertions;
using Microsoft.Extensions.Logging;
using RuneAndRust.TestUtilities;
using RuneAndRust.TestUtilities.Fixtures;
using RuneAndRust.TestUtilities.Logging;

namespace RuneAndRust.Application.UnitTests.TestUtilities;

/// <summary>
/// Tests for SeededRandom and TestFixtureBase utilities.
/// </summary>
[TestFixture]
public class UtilityTests
{
    [Test]
    public void SeededRandom_Create_ProducesDeterministicValues()
    {
        // Arrange
        var random1 = SeededRandom.Create(42);
        var random2 = SeededRandom.Create(42);

        // Act
        var values1 = Enumerable.Range(0, 10).Select(_ => random1.Next()).ToList();
        var values2 = Enumerable.Range(0, 10).Select(_ => random2.Next()).ToList();

        // Assert
        values1.Should().BeEquivalentTo(values2);
    }

    [Test]
    public void SeededRandom_DifferentSeeds_ProduceDifferentValues()
    {
        // Arrange
        var random1 = SeededRandom.Create(42);
        var random2 = SeededRandom.Create(43);

        // Act
        var values1 = Enumerable.Range(0, 10).Select(_ => random1.Next()).ToList();
        var values2 = Enumerable.Range(0, 10).Select(_ => random2.Next()).ToList();

        // Assert
        values1.Should().NotBeEquivalentTo(values2);
    }

    [Test]
    public void SeededRandom_CreateMultiple_ProducesDistinctSequences()
    {
        // Arrange
        var randoms = SeededRandom.CreateMultiple(3).ToList();

        // Act
        var sequences = randoms.Select(r =>
            Enumerable.Range(0, 5).Select(_ => r.Next()).ToList()
        ).ToList();

        // Assert
        sequences.Should().HaveCount(3);
        sequences[0].Should().NotBeEquivalentTo(sequences[1]);
        sequences[1].Should().NotBeEquivalentTo(sequences[2]);
    }

    [Test]
    public void SeededRandom_DefaultSeed_IsConsistent()
    {
        // Arrange & Act
        var random1 = SeededRandom.Create();
        var random2 = SeededRandom.Create(SeededRandom.DefaultSeed);

        var value1 = random1.Next();
        var value2 = random2.Next();

        // Assert
        value1.Should().Be(value2);
        SeededRandom.DefaultSeed.Should().Be(12345);
    }
}

/// <summary>
/// Tests for TestFixtureBase functionality.
/// </summary>
[TestFixture]
public class TestFixtureBaseTests : TestFixtureBase
{
    [Test]
    public void TestFixtureBase_SetUp_InitializesProviders()
    {
        // Assert - SetUp is called automatically by NUnit
        ConfigProvider.Should().NotBeNull();
        Repository.Should().NotBeNull();
        LoggerFactory.Should().NotBeNull();
    }

    [Test]
    public void TestFixtureBase_GetTestLogger_ReturnsLogger()
    {
        // Act
        var logger = GetTestLogger<TestFixtureBaseTests>();

        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeOfType<TestLogger<TestFixtureBaseTests>>();
    }

    [Test]
    public void TestFixtureBase_VerifyLog_FindsMessage()
    {
        // Arrange
        var logger = GetTestLogger<TestFixtureBaseTests>();
        logger.Log(LogLevel.Information, default, "Test message content", null, (s, e) => s);

        // Act & Assert - should not throw
        VerifyLog<TestFixtureBaseTests>(LogLevel.Information, "message content");
    }

    [Test]
    public void TestFixtureBase_CreateMockLogger_ReturnsMoqMock()
    {
        // Act
        var mockLogger = CreateMockLogger<TestFixtureBaseTests>();

        // Assert
        mockLogger.Should().NotBeNull();
        mockLogger.Object.Should().NotBeNull();
    }

    [Test]
    public void TestFixtureBase_CreateSeededRandom_ReturnsSeededInstance()
    {
        // Act
        var random1 = CreateSeededRandom(100);
        var random2 = CreateSeededRandom(100);

        // Assert
        random1.Next().Should().Be(random2.Next());
    }
}
