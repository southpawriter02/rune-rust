using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Filter options for the quest journal.
/// </summary>
/// <remarks>
/// Immutable record struct for filtering quest collections.
/// Supports category, status, timed, chain, and text search filters.
/// </remarks>
public readonly record struct QuestJournalFilter
{
    /// <summary>Gets the category filter. Null means all categories.</summary>
    public QuestCategory? Category { get; init; }

    /// <summary>Gets the status filter. Null means all statuses.</summary>
    public QuestStatus? Status { get; init; }

    /// <summary>Gets whether to show only timed quests.</summary>
    public bool TimedOnly { get; init; }

    /// <summary>Gets whether to show only chain quests.</summary>
    public bool ChainsOnly { get; init; }

    /// <summary>Gets the text search filter.</summary>
    public string? SearchText { get; init; }

    /// <summary>Gets the default filter (show all quests).</summary>
    public static QuestJournalFilter Default => new();

    /// <summary>Creates a category filter.</summary>
    public static QuestJournalFilter ForCategory(QuestCategory category) => new()
    {
        Category = category
    };

    /// <summary>Creates a filter for active quests only.</summary>
    public static QuestJournalFilter ActiveOnly => new()
    {
        Status = QuestStatus.Active
    };

    /// <summary>Creates a filter for completed quests only.</summary>
    public static QuestJournalFilter CompletedOnly => new()
    {
        Status = QuestStatus.Completed
    };

    /// <summary>Creates a filter for failed quests only.</summary>
    public static QuestJournalFilter FailedOnly => new()
    {
        Status = QuestStatus.Failed
    };

    /// <summary>Creates a filter for timed quests only.</summary>
    public static QuestJournalFilter TimedQuests => new()
    {
        TimedOnly = true
    };

    /// <summary>Creates a filter for chain quests only.</summary>
    public static QuestJournalFilter ChainQuests => new()
    {
        ChainsOnly = true
    };

    /// <summary>Creates a filter with text search.</summary>
    public static QuestJournalFilter WithSearch(string searchText) => new()
    {
        SearchText = searchText
    };

    /// <summary>Applies the filter to a quest collection.</summary>
    public IEnumerable<Quest> Apply(IEnumerable<Quest> quests)
    {
        ArgumentNullException.ThrowIfNull(quests);

        var result = quests;

        // Copy to locals for lambda capture (struct limitation)
        var categoryFilter = Category;
        var statusFilter = Status;
        var timedOnlyFilter = TimedOnly;
        var chainsOnlyFilter = ChainsOnly;
        var searchTextFilter = SearchText;

        if (categoryFilter.HasValue)
            result = result.Where(q => q.Category == categoryFilter.Value);

        if (statusFilter.HasValue)
            result = result.Where(q => q.Status == statusFilter.Value);

        if (timedOnlyFilter)
            result = result.Where(q => q.IsTimed);

        if (chainsOnlyFilter)
            result = result.Where(q => q.IsInChain);

        if (!string.IsNullOrEmpty(searchTextFilter))
        {
            var searchLower = searchTextFilter.ToLowerInvariant();
            result = result.Where(q =>
                q.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                q.Description.Contains(searchLower, StringComparison.OrdinalIgnoreCase));
        }

        return result;
    }

    /// <summary>Gets whether any filter is active.</summary>
    public bool HasActiveFilter =>
        Category.HasValue ||
        Status.HasValue ||
        TimedOnly ||
        ChainsOnly ||
        !string.IsNullOrEmpty(SearchText);

    /// <summary>Gets a display name for the current filter.</summary>
    public string GetDisplayName()
    {
        var parts = new List<string>();

        if (Category.HasValue)
            parts.Add(Category.Value.ToString());

        if (Status.HasValue)
            parts.Add(Status.Value.ToString());

        if (TimedOnly)
            parts.Add("Timed");

        if (ChainsOnly)
            parts.Add("Chains");

        if (!string.IsNullOrEmpty(SearchText))
            parts.Add($"\"{SearchText}\"");

        return parts.Count > 0 ? string.Join(", ", parts) : "All";
    }
}
