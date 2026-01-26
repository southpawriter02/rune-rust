namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Provides active search functionality for rooms and containers,
/// discovering hidden items and elements beyond passive perception.
/// </summary>
/// <remarks>
/// <para>
/// The search service implements active perception mechanics where players
/// deliberately spend time searching an area. Active searching grants a +2
/// bonus to effective perception, allowing discovery of elements that
/// would be missed by passive perception alone.
/// </para>
/// <para>
/// Search operations take time based on area size and include comprehensive
/// coverage of all containers and hidden elements within the search target.
/// </para>
/// </remarks>
public interface ISearchService
{
    /// <summary>
    /// Performs an active search of the specified target.
    /// </summary>
    /// <param name="parameters">Parameters controlling the search operation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Comprehensive results of the search including all discoveries.</returns>
    /// <remarks>
    /// The search process:
    /// 1. Performs Wits check with active search bonus (+2)
    /// 2. Calculates time cost based on area size
    /// 3. Compares enhanced perception against hidden element DCs
    /// 4. Reveals elements/items above passive threshold but at/below active threshold
    /// 5. Searches all containers within the target area
    /// 6. Checks for applicable puzzle hints
    /// 7. Returns comprehensive SearchResult
    /// </remarks>
    Task<ActiveSearchResult> SearchAsync(SearchParameters parameters, CancellationToken ct = default);

    /// <summary>
    /// Calculates the time required to search a specific area.
    /// </summary>
    /// <param name="roomId">The room to calculate search time for.</param>
    /// <returns>Time in minutes required to complete the search.</returns>
    int CalculateSearchTime(string roomId);

    /// <summary>
    /// Gets the effective perception value for a character during active search.
    /// </summary>
    /// <param name="characterId">The character performing the search.</param>
    /// <returns>The effective perception including active bonus.</returns>
    /// <remarks>
    /// Effective perception = Passive Perception + Active Search Bonus (+2)
    /// </remarks>
    int GetActivePerceptionValue(string characterId);

    /// <summary>
    /// Gets the active search bonus constant.
    /// </summary>
    int ActiveSearchBonus { get; }

    /// <summary>
    /// Checks if a specific container has been searched in the current session.
    /// </summary>
    /// <param name="containerId">The container identifier.</param>
    /// <returns>True if the container has already been searched.</returns>
    bool HasBeenSearched(string containerId);

    /// <summary>
    /// Marks a container as searched.
    /// </summary>
    /// <param name="containerId">The container identifier.</param>
    void MarkAsSearched(string containerId);

    /// <summary>
    /// Clears the searched container cache.
    /// </summary>
    void ClearSearchedCache();
}
