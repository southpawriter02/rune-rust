using RuneAndRust.Application.DTOs;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Provides completion candidates for a specific category.
/// </summary>
public interface ICompletionSource
{
    /// <summary>
    /// Gets the category this source provides (commands, items, targets, directions).
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Gets the priority. Higher priority sources are checked first.
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Checks if this source applies to the current context.
    /// </summary>
    /// <param name="context">The completion context.</param>
    /// <returns>True if this source should provide completions.</returns>
    bool AppliesTo(CompletionContext context);
    
    /// <summary>
    /// Gets matching completions for a prefix.
    /// </summary>
    /// <param name="prefix">The text to match against.</param>
    /// <param name="context">The completion context.</param>
    /// <returns>Matching completion strings.</returns>
    IEnumerable<string> GetMatches(string prefix, CompletionContext context);
}
