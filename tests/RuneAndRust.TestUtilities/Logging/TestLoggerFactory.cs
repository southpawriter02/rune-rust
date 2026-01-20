using Microsoft.Extensions.Logging;

namespace RuneAndRust.TestUtilities.Logging;

/// <summary>
/// Factory for creating test loggers.
/// Caches loggers by type for consistent access within a test.
/// </summary>
public class TestLoggerFactory : ILoggerFactory
{
    private readonly Dictionary<Type, object> _loggers = new();

    /// <summary>
    /// Gets a test logger for the specified type.
    /// Creates a new logger if one doesn't exist for this type.
    /// </summary>
    /// <typeparam name="T">The category type.</typeparam>
    /// <returns>A TestLogger for the specified type.</returns>
    public TestLogger<T> GetTestLogger<T>()
    {
        var type = typeof(T);
        if (!_loggers.TryGetValue(type, out var logger))
        {
            logger = new TestLogger<T>();
            _loggers[type] = logger;
        }
        return (TestLogger<T>)logger;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) =>
        new TestLogger<object>();

    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider) { }

    /// <inheritdoc />
    public void Dispose()
    {
        _loggers.Clear();
        GC.SuppressFinalize(this);
    }
}
