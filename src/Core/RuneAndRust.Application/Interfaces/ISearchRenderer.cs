namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Renders search results for display to the player.
/// </summary>
/// <remarks>
/// Provides formatting for search output including items found,
/// hidden elements revealed, containers searched, and puzzle hints.
/// </remarks>
public interface ISearchRenderer
{
    /// <summary>
    /// Renders a complete search result with all discoveries.
    /// </summary>
    /// <param name="result">The search result to render.</param>
    /// <returns>Formatted output for display.</returns>
    string Render(ActiveSearchResult result);

    /// <summary>
    /// Renders a summary of a search with no discoveries.
    /// </summary>
    /// <param name="result">The search result with no findings.</param>
    /// <returns>Formatted output indicating thorough but empty search.</returns>
    string RenderEmptySearch(ActiveSearchResult result);

    /// <summary>
    /// Renders individual item discovery text.
    /// </summary>
    /// <param name="item">The found item to render.</param>
    /// <returns>Formatted discovery text for a single item.</returns>
    string RenderItemDiscovery(FoundItem item);

    /// <summary>
    /// Renders the time spent searching.
    /// </summary>
    /// <param name="minutes">Time in minutes.</param>
    /// <returns>Formatted time string.</returns>
    string RenderTimeSpent(int minutes);

    /// <summary>
    /// Renders the Wits check result with bonus breakdown.
    /// </summary>
    /// <param name="result">The search result containing check data.</param>
    /// <returns>Formatted check result string.</returns>
    string RenderWitsCheck(ActiveSearchResult result);
}
