using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RuneAndRust.TestUtilities.Logging;
using RuneAndRust.TestUtilities.Mocks;

namespace RuneAndRust.TestUtilities.Fixtures;

/// <summary>
/// Base class for test fixtures with common setup.
/// </summary>
/// <remarks>
/// Provides automatic initialization of mock configuration providers and test loggers.
/// Inherit from this class to get access to common test infrastructure.
/// </remarks>
public abstract class TestFixtureBase
{
    /// <summary>
    /// Mock configuration provider for tests.
    /// </summary>
    protected MockConfigurationProvider ConfigProvider { get; private set; } = null!;

    /// <summary>
    /// Mock repository for tests.
    /// </summary>
    protected Mocks.MockRepository Repository { get; private set; } = null!;

    /// <summary>
    /// Test logger factory for capturing logs.
    /// </summary>
    protected TestLoggerFactory LoggerFactory { get; private set; } = null!;

    /// <summary>
    /// Sets up common test infrastructure.
    /// </summary>
    /// <remarks>
    /// Override this method to add additional setup, but call base.SetUp() first.
    /// </remarks>
    [SetUp]
    public virtual void SetUp()
    {
        ConfigProvider = new MockConfigurationProvider();
        Repository = new Mocks.MockRepository();
        LoggerFactory = new TestLoggerFactory();
    }

    /// <summary>
    /// Creates a mock logger for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>A Moq mock of ILogger{T}.</returns>
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new();

    /// <summary>
    /// Gets a test logger that captures log entries.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>A TestLogger instance that captures log entries.</returns>
    protected TestLogger<T> GetTestLogger<T>() => LoggerFactory.GetTestLogger<T>();

    /// <summary>
    /// Verifies that a log message was written at the specified level.
    /// </summary>
    /// <typeparam name="T">The type the logger was created for.</typeparam>
    /// <param name="level">The expected log level.</param>
    /// <param name="messageFragment">A fragment of the expected message.</param>
    /// <exception cref="AssertionException">Thrown if the expected log message was not found.</exception>
    protected void VerifyLog<T>(LogLevel level, string messageFragment)
    {
        var logger = GetTestLogger<T>();
        var hasMessage = logger.GetEntries(level)
            .Any(e => e.Message.Contains(messageFragment, StringComparison.OrdinalIgnoreCase));

        if (!hasMessage)
        {
            var allEntries = logger.GetEntries(level).Select(e => e.Message).ToList();
            var entriesInfo = allEntries.Count > 0
                ? $"Found entries at {level}: {string.Join(", ", allEntries)}"
                : $"No entries found at {level}";

            Assert.Fail(
                $"Expected log message containing '{messageFragment}' at level {level}, but none was found. {entriesInfo}");
        }
    }

    /// <summary>
    /// Verifies that no log messages were written at the specified level.
    /// </summary>
    /// <typeparam name="T">The type the logger was created for.</typeparam>
    /// <param name="level">The log level to check.</param>
    protected void VerifyNoLogs<T>(LogLevel level)
    {
        var logger = GetTestLogger<T>();
        var count = logger.GetCount(level);

        if (count > 0)
        {
            var entries = logger.GetEntries(level).Select(e => e.Message);
            Assert.Fail(
                $"Expected no log messages at {level}, but found {count}: {string.Join(", ", entries)}");
        }
    }

    /// <summary>
    /// Creates a seeded random instance for deterministic tests.
    /// </summary>
    /// <param name="seed">The seed value (default: 12345).</param>
    /// <returns>A seeded Random instance.</returns>
    protected static Random CreateSeededRandom(int seed = SeededRandom.DefaultSeed)
        => SeededRandom.Create(seed);
}
