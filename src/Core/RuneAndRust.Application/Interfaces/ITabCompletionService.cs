using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides tab completion for user input.
/// </summary>
public interface ITabCompletionService
{
    /// <summary>
    /// Gets all completions for partial input.
    /// </summary>
    /// <param name="partialInput">The full input line so far.</param>
    /// <param name="context">Context for completion (targets, items, etc.).</param>
    /// <returns>List of matching completions.</returns>
    IReadOnlyList<string> GetCompletions(string partialInput, CompletionContext context);
    
    /// <summary>
    /// Gets the best single completion, or null if multiple/none.
    /// </summary>
    /// <param name="partialInput">The full input line so far.</param>
    /// <param name="context">Context for completion.</param>
    /// <returns>Best completion, or null.</returns>
    string? GetBestCompletion(string partialInput, CompletionContext context);
    
    /// <summary>
    /// Completes the input if possible.
    /// </summary>
    /// <param name="partialInput">Current input.</param>
    /// <param name="context">Completion context.</param>
    /// <returns>Completed input, or original if no completion.</returns>
    string Complete(string partialInput, CompletionContext context);
    
    /// <summary>
    /// Registers a completion source.
    /// </summary>
    /// <param name="source">Source to register.</param>
    void RegisterSource(ICompletionSource source);
    
    /// <summary>
    /// Unregisters a completion source.
    /// </summary>
    /// <param name="source">Source to remove.</param>
    void UnregisterSource(ICompletionSource source);
    
    /// <summary>
    /// Gets all registered sources.
    /// </summary>
    IReadOnlyList<ICompletionSource> Sources { get; }
}
