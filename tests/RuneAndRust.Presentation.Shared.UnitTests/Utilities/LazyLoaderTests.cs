// ═══════════════════════════════════════════════════════════════════════════════
// LazyLoaderTests.cs
// Unit tests for LazyLoader<T> utility.
// Version: 0.13.5f
// ═══════════════════════════════════════════════════════════════════════════════

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Presentation.Shared.Utilities;

namespace RuneAndRust.Presentation.Shared.UnitTests.Utilities;

/// <summary>
/// Unit tests for <see cref="LazyLoader{T}"/>.
/// </summary>
[TestFixture]
public class LazyLoaderTests
{
    private Mock<ILogger> _mockLogger = null!;
    private List<string> _testData = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger>();
        _testData = Enumerable.Range(1, 100).Select(i => $"Item{i}").ToList();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Assert
        lazyLoader.Should().NotBeNull();
        lazyLoader.PageSize.Should().Be(10);
        lazyLoader.TotalCount.Should().Be(100);
        lazyLoader.TotalPages.Should().Be(10);
    }

    [Test]
    public void Constructor_WithNullDataSource_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new LazyLoader<string>(dataSource: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithDefaultParameters_UsesDefaults()
    {
        // Arrange & Act
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => Array.Empty<string>());

        // Assert
        lazyLoader.PageSize.Should().Be(20); // Default page size
    }

    [Test]
    public void Constructor_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var data = Enumerable.Range(1, 95).Select(i => $"Item{i}").ToList();

        // Act
        var loader = new LazyLoader<string>(
            dataSource: (skip, take) => data.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => data.Count);

        // Assert
        loader.TotalPages.Should().Be(10); // ceil(95/10) = 10
    }

    // ═══════════════════════════════════════════════════════════════
    // PAGE LOADING TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void LoadPage_WhenPageNotCached_CallsDataSource()
    {
        // Arrange
        var loaderCallCount = 0;
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) =>
            {
                loaderCallCount++;
                return _testData.Skip(skip).Take(take);
            },
            pageSize: 10);

        // Act
        var page = lazyLoader.LoadPage(0).ToList();

        // Assert
        loaderCallCount.Should().Be(1);
        page.Should().HaveCount(10);
        page[0].Should().Be("Item1");
    }

    [Test]
    public void LoadPage_WhenPageCached_DoesNotCallDataSourceAgain()
    {
        // Arrange
        var loaderCallCount = 0;
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) =>
            {
                loaderCallCount++;
                return _testData.Skip(skip).Take(take);
            },
            pageSize: 10);

        // Act
        lazyLoader.LoadPage(0);
        lazyLoader.LoadPage(0);
        lazyLoader.LoadPage(0);

        // Assert
        loaderCallCount.Should().Be(1);
    }

    [Test]
    public void LoadPage_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act
        var act = () => lazyLoader.LoadPage(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void LoadPage_UpdatesCurrentPage()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10);

        // Act
        lazyLoader.LoadPage(5);

        // Assert
        lazyLoader.CurrentPage.Should().Be(5);
    }

    // ═══════════════════════════════════════════════════════════════
    // CACHE EVICTION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void LoadPage_WhenCacheFull_EvictsLeastRecentlyUsed()
    {
        // Arrange
        var loadedPages = new List<int>();
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) =>
            {
                loadedPages.Add(skip / 10); // Track which page was loaded
                return _testData.Skip(skip).Take(take);
            },
            pageSize: 10,
            cacheSize: 3);

        // Act - Load 4 pages (exceeds cache of 3)
        lazyLoader.LoadPage(0);
        lazyLoader.LoadPage(1);
        lazyLoader.LoadPage(2);
        lazyLoader.LoadPage(3); // Should evict page 0

        // Clear tracking and re-access page 0
        loadedPages.Clear();
        lazyLoader.LoadPage(0);

        // Assert - Page 0 should be reloaded
        loadedPages.Should().Contain(0);
    }

    [Test]
    public void InvalidateCache_ClearsAllCachedPages()
    {
        // Arrange
        var loaderCallCount = 0;
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) =>
            {
                loaderCallCount++;
                return _testData.Skip(skip).Take(take);
            },
            pageSize: 10);

        // Act
        lazyLoader.LoadPage(0);
        loaderCallCount = 0;
        lazyLoader.InvalidateCache();
        lazyLoader.LoadPage(0);

        // Assert - Should reload after invalidation
        loaderCallCount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════════════════════
    // NAVIGATION TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void CurrentPage_StartsAtZero()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act & Assert
        lazyLoader.CurrentPage.Should().Be(0);
    }

    [Test]
    public void LoadNextPage_AdvancesToNextPageAndLoadsIt()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Act
        lazyLoader.LoadPage(0);
        var nextPageItems = lazyLoader.LoadNextPage().ToList();

        // Assert
        lazyLoader.CurrentPage.Should().Be(1);
        nextPageItems[0].Should().Be("Item11");
    }

    [Test]
    public void LoadNextPage_AtLastPage_ReturnsEmpty()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Act
        lazyLoader.LoadPage(9); // Go to last page
        var result = lazyLoader.LoadNextPage();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void LoadPreviousPage_MovesToPreviousPageAndLoadsIt()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10);

        // Act
        lazyLoader.LoadPage(2);
        var prevPageItems = lazyLoader.LoadPreviousPage().ToList();

        // Assert
        lazyLoader.CurrentPage.Should().Be(1);
        prevPageItems[0].Should().Be("Item11");
    }

    [Test]
    public void LoadPreviousPage_AtFirstPage_ReturnsEmpty()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act
        lazyLoader.LoadPage(0);
        var result = lazyLoader.LoadPreviousPage();

        // Assert
        result.Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ITEM TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void GetItem_ReturnsCorrectItemFromPage()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Act
        var item = lazyLoader.GetItem(15); // index 15 is Item16

        // Assert
        item.Should().Be("Item16");
    }

    [Test]
    public void GetItem_WithNegativeIndex_ReturnsDefault()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            totalCountSource: () => _testData.Count);

        // Act
        var item = lazyLoader.GetItem(-1);

        // Assert
        item.Should().BeNull();
    }

    [Test]
    public void GetItem_WithIndexBeyondTotal_ReturnsDefault()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            totalCountSource: () => _testData.Count);

        // Act
        var item = lazyLoader.GetItem(500);

        // Assert
        item.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ASYNC PREFETCH TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task PrefetchAsync_LoadsPageInBackground()
    {
        // Arrange
        var loadedSkips = new List<int>();
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) =>
            {
                loadedSkips.Add(skip);
                return _testData.Skip(skip).Take(take);
            },
            pageSize: 10);

        // Act
        await lazyLoader.PrefetchAsync(2);

        // Assert - Page 2 starts at skip=20
        loadedSkips.Should().Contain(20);
    }

    [Test]
    public async Task PrefetchAsync_WithNegativeIndex_DoesNotThrow()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act
        var act = async () => await lazyLoader.PrefetchAsync(-1);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // PROPERTY TESTS
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public void HasMorePages_WhenNotOnLastPage_ReturnsTrue()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Act & Assert
        lazyLoader.HasMorePages.Should().BeTrue();
    }

    [Test]
    public void HasMorePages_WhenOnLastPage_ReturnsFalse()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            totalCountSource: () => _testData.Count);

        // Act
        lazyLoader.LoadPage(9);

        // Assert
        lazyLoader.HasMorePages.Should().BeFalse();
    }

    [Test]
    public void HasPreviousPages_WhenNotOnFirstPage_ReturnsTrue()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act
        lazyLoader.LoadPage(1);

        // Assert
        lazyLoader.HasPreviousPages.Should().BeTrue();
    }

    [Test]
    public void HasPreviousPages_WhenOnFirstPage_ReturnsFalse()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take));

        // Act & Assert
        lazyLoader.HasPreviousPages.Should().BeFalse();
    }

    [Test]
    public void LoadedCount_ReturnsCorrectCachedItemCount()
    {
        // Arrange
        var lazyLoader = new LazyLoader<string>(
            dataSource: (skip, take) => _testData.Skip(skip).Take(take),
            pageSize: 10,
            cacheSize: 3);

        // Act
        lazyLoader.LoadPage(0);
        lazyLoader.LoadPage(1);

        // Assert
        lazyLoader.LoadedCount.Should().Be(20);
    }
}
