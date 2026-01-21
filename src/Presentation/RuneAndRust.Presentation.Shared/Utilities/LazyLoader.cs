using Microsoft.Extensions.Logging;

namespace RuneAndRust.Presentation.Shared.Utilities;

/// <summary>
/// Provides paginated data loading with caching and prefetching support for large data sets.
/// </summary>
/// <typeparam name="T">The type of items being loaded.</typeparam>
/// <remarks>
/// <para>Designed for efficiently browsing large lists such as achievements, leaderboards,
/// and statistics without loading all data into memory at once.</para>
/// <para><b>Key Features:</b></para>
/// <list type="bullet">
/// <item><description>On-demand page loading</description></item>
/// <item><description>Configurable page cache with LRU eviction</description></item>
/// <item><description>Async prefetching for smooth scrolling</description></item>
/// <item><description>Navigation helpers (next/previous/specific page)</description></item>
/// </list>
/// <para><b>Logging:</b> Page loading events are logged at Debug level.</para>
/// </remarks>
/// <example>
/// <code>
/// // Create a lazy loader for achievements with 20 items per page
/// var loader = new LazyLoader&lt;Achievement&gt;(
///     dataSource: (skip, take) => achievementService.GetAchievements(skip, take),
///     pageSize: 20,
///     totalCountSource: () => achievementService.GetTotalCount());
///     
/// // Load the first page
/// var firstPage = loader.LoadPage(0);
/// 
/// // Prefetch the next page in the background
/// await loader.PrefetchAsync(1);
/// 
/// // Navigate to next page (already cached)
/// var secondPage = loader.LoadNextPage();
/// </code>
/// </example>
public class LazyLoader<T>
{
    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE FIELDS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Function that fetches data: (skip, take) => items.
    /// </summary>
    private readonly Func<int, int, IEnumerable<T>> _dataSource;

    /// <summary>
    /// Optional function to get total count without loading all data.
    /// </summary>
    private readonly Func<int>? _totalCountSource;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    private readonly int _pageSize;

    /// <summary>
    /// Maximum number of pages to keep in cache.
    /// </summary>
    private readonly int _cacheSize;

    /// <summary>
    /// Cache of loaded pages: pageIndex -> items.
    /// </summary>
    private readonly Dictionary<int, List<T>> _pageCache;

    /// <summary>
    /// Queue tracking page cache order for LRU eviction.
    /// </summary>
    private readonly Queue<int> _cacheOrder;

    /// <summary>
    /// Lock object for thread-safe cache operations.
    /// </summary>
    private readonly object _cacheLock = new();

    /// <summary>
    /// Optional logger for debug output.
    /// </summary>
    private readonly ILogger? _logger;

    /// <summary>
    /// Cached total count (lazy loaded).
    /// </summary>
    private int? _totalCount;

    // ═══════════════════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyLoader{T}"/> class.
    /// </summary>
    /// <param name="dataSource">
    /// Function to fetch data. Parameters are (skip, take), returns items.
    /// </param>
    /// <param name="pageSize">Number of items per page. Default is 20.</param>
    /// <param name="cacheSize">Maximum number of pages to cache. Default is 3.</param>
    /// <param name="totalCountSource">
    /// Optional function to get the total item count without loading all data.
    /// </param>
    /// <param name="logger">Optional logger for debug output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dataSource"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new LazyLoader&lt;string&gt;(
    ///     dataSource: (skip, take) => items.Skip(skip).Take(take),
    ///     pageSize: 25,
    ///     cacheSize: 5,
    ///     totalCountSource: () => items.Count);
    /// </code>
    /// </example>
    public LazyLoader(
        Func<int, int, IEnumerable<T>> dataSource,
        int pageSize = 20,
        int cacheSize = 3,
        Func<int>? totalCountSource = null,
        ILogger? logger = null)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _pageSize = pageSize > 0 ? pageSize : 20;
        _cacheSize = cacheSize > 0 ? cacheSize : 3;
        _totalCountSource = totalCountSource;
        _logger = logger;
        _pageCache = new Dictionary<int, List<T>>();
        _cacheOrder = new Queue<int>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the current page index (0-based).
    /// </summary>
    public int CurrentPage { get; private set; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize => _pageSize;

    /// <summary>
    /// Gets the total number of items available.
    /// </summary>
    /// <remarks>
    /// Lazily loaded on first access if <c>totalCountSource</c> was provided.
    /// Returns 0 if no count source was provided.
    /// </remarks>
    public int TotalCount
    {
        get
        {
            _totalCount ??= _totalCountSource?.Invoke() ?? 0;
            return _totalCount.Value;
        }
    }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => TotalCount > 0
        ? (int)Math.Ceiling((double)TotalCount / _pageSize)
        : 0;

    /// <summary>
    /// Gets the number of items currently loaded in cache.
    /// </summary>
    public int LoadedCount
    {
        get
        {
            lock (_cacheLock)
            {
                return _pageCache.Values.Sum(p => p.Count);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether there are more pages after the current page.
    /// </summary>
    public bool HasMorePages => CurrentPage < TotalPages - 1;

    /// <summary>
    /// Gets a value indicating whether there are previous pages before the current page.
    /// </summary>
    public bool HasPreviousPages => CurrentPage > 0;

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - PAGE LOADING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads a specific page by index.
    /// </summary>
    /// <param name="pageIndex">The 0-based page index to load.</param>
    /// <returns>The items on the requested page.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pageIndex"/> is negative.
    /// </exception>
    /// <remarks>
    /// If the page is already cached, returns the cached data.
    /// Otherwise, fetches from the data source and caches the result.
    /// </remarks>
    public IEnumerable<T> LoadPage(int pageIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageIndex);

        lock (_cacheLock)
        {
            // Check if page is already cached
            if (!_pageCache.TryGetValue(pageIndex, out var page))
            {
                // Fetch from data source
                var skip = pageIndex * _pageSize;
                page = _dataSource(skip, _pageSize).ToList();
                CachePage(pageIndex, page);

                _logger?.LogDebug(
                    "LazyLoader: loaded page {Page} ({Count} items)",
                    pageIndex,
                    page.Count);
            }

            CurrentPage = pageIndex;
            return page;
        }
    }

    /// <summary>
    /// Loads the next page after the current page.
    /// </summary>
    /// <returns>
    /// The items on the next page, or an empty enumerable if already at the last page.
    /// </returns>
    public IEnumerable<T> LoadNextPage()
    {
        return HasMorePages
            ? LoadPage(CurrentPage + 1)
            : Enumerable.Empty<T>();
    }

    /// <summary>
    /// Loads the previous page before the current page.
    /// </summary>
    /// <returns>
    /// The items on the previous page, or an empty enumerable if already at the first page.
    /// </returns>
    public IEnumerable<T> LoadPreviousPage()
    {
        return HasPreviousPages
            ? LoadPage(CurrentPage - 1)
            : Enumerable.Empty<T>();
    }

    /// <summary>
    /// Gets a specific item by its overall index.
    /// </summary>
    /// <param name="index">The 0-based item index across all pages.</param>
    /// <returns>
    /// The item at the specified index, or <c>default(T)</c> if the index is out of range.
    /// </returns>
    /// <remarks>
    /// This method loads the appropriate page if not already cached.
    /// </remarks>
    public T? GetItem(int index)
    {
        if (index < 0 || (_totalCount.HasValue && index >= _totalCount.Value))
        {
            return default;
        }

        var pageIndex = index / _pageSize;
        var itemIndex = index % _pageSize;

        var page = LoadPage(pageIndex).ToList();
        return itemIndex < page.Count ? page[itemIndex] : default;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PUBLIC METHODS - PREFETCHING AND CACHE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Prefetches a page in the background for smoother navigation.
    /// </summary>
    /// <param name="pageIndex">The page index to prefetch.</param>
    /// <returns>A task that completes when the page is loaded.</returns>
    /// <remarks>
    /// Use this to preload the next page while the user views the current page.
    /// Does nothing if the page is already cached or the index is invalid.
    /// </remarks>
    public async Task PrefetchAsync(int pageIndex)
    {
        if (pageIndex < 0)
        {
            return;
        }

        // Check if total pages is known and valid
        if (_totalCount.HasValue && pageIndex >= TotalPages)
        {
            return;
        }

        // Check if already cached
        lock (_cacheLock)
        {
            if (_pageCache.ContainsKey(pageIndex))
            {
                return;
            }
        }

        // Prefetch in background
        await Task.Run(() =>
        {
            var skip = pageIndex * _pageSize;
            var page = _dataSource(skip, _pageSize).ToList();

            lock (_cacheLock)
            {
                // Double-check cache after acquiring lock
                if (!_pageCache.ContainsKey(pageIndex))
                {
                    CachePage(pageIndex, page);

                    _logger?.LogDebug(
                        "LazyLoader: prefetched page {Page} ({Count} items)",
                        pageIndex,
                        page.Count);
                }
            }
        });
    }

    /// <summary>
    /// Invalidates the entire cache, forcing reload on next access.
    /// </summary>
    /// <remarks>
    /// Use this when the underlying data has changed and cached data is stale.
    /// Also resets the total count to be re-fetched.
    /// </remarks>
    public void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _pageCache.Clear();
            _cacheOrder.Clear();
            _totalCount = null;

            _logger?.LogDebug("LazyLoader: cache invalidated");
        }
    }

    /// <summary>
    /// Refreshes the total count without invalidating cached pages.
    /// </summary>
    /// <remarks>
    /// Useful when items may have been added/removed but cached pages are still valid.
    /// </remarks>
    public void RefreshTotalCount()
    {
        _totalCount = null;
        _ = TotalCount; // Force re-fetch
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a page to the cache, evicting oldest pages if cache is full.
    /// </summary>
    /// <param name="pageIndex">The page index being cached.</param>
    /// <param name="page">The page items to cache.</param>
    /// <remarks>
    /// Uses a simple LRU (Least Recently Used) eviction policy based on load order.
    /// Must be called within a lock on <see cref="_cacheLock"/>.
    /// </remarks>
    private void CachePage(int pageIndex, List<T> page)
    {
        // Evict oldest pages if cache is at capacity
        while (_cacheOrder.Count >= _cacheSize)
        {
            var oldest = _cacheOrder.Dequeue();
            _pageCache.Remove(oldest);

            _logger?.LogDebug("LazyLoader: evicted page {Page} from cache", oldest);
        }

        // Add new page to cache
        _pageCache[pageIndex] = page;
        _cacheOrder.Enqueue(pageIndex);
    }
}
