namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides operation scope for correlation across service calls.
/// Enables tracking related operations through a shared operation identifier.
/// </summary>
public interface IOperationScope
{
    /// <summary>
    /// Gets the current operation identifier (8-character hex string).
    /// </summary>
    string OperationId { get; }

    /// <summary>
    /// Gets the current session identifier, if set.
    /// </summary>
    Guid? SessionId { get; }

    /// <summary>
    /// Creates a new child scope for a sub-operation.
    /// All logs within this scope will include the operation context.
    /// </summary>
    /// <param name="operationName">The name of the sub-operation.</param>
    /// <returns>A disposable scope that ends when disposed.</returns>
    IDisposable BeginOperation(string operationName);
}
