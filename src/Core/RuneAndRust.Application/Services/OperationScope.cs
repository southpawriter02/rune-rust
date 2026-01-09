using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Provides operation scope for correlation across service calls.
/// Creates scoped logging contexts with operation identifiers.
/// </summary>
public class OperationScope : IOperationScope
{
    private readonly ILogger _logger;
    private readonly Stack<string> _operationStack = new();

    /// <inheritdoc />
    public string OperationId { get; }

    /// <inheritdoc />
    public Guid? SessionId { get; private set; }

    /// <summary>
    /// Creates a new operation scope with a unique identifier.
    /// </summary>
    /// <param name="logger">The logger to use for scope creation.</param>
    /// <param name="sessionId">Optional session identifier for correlation.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public OperationScope(ILogger<OperationScope> logger, Guid? sessionId = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        OperationId = Guid.NewGuid().ToString("N")[..8];
        SessionId = sessionId;
    }

    /// <summary>
    /// Sets the session ID for this scope.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    public void SetSessionId(Guid sessionId)
    {
        SessionId = sessionId;
    }

    /// <inheritdoc />
    public IDisposable BeginOperation(string operationName)
    {
        _operationStack.Push(operationName);

        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = OperationId,
            ["Operation"] = operationName,
            ["SessionId"] = SessionId ?? Guid.Empty,
            ["Depth"] = _operationStack.Count
        })!;
    }
}
