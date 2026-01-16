using FluentAssertions;
using NUnit.Framework;
using RuneAndRust.Infrastructure.Services.Audio;

namespace RuneAndRust.Application.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="AudioCache"/>.
/// </summary>
[TestFixture]
public class AudioCacheTests
{
    private AudioCache _cache = null!;
    private string _testDir = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new AudioCache(maxSize: 3);
        _testDir = Path.Combine(Path.GetTempPath(), "audio_cache_test_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Clear();
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    /// <summary>
    /// Verifies GetOrLoad caches loaded files.
    /// </summary>
    [Test]
    public void GetOrLoad_CachesLoadedFiles()
    {
        // Arrange
        var testFile = CreateTestFile("test1.ogg");

        // Act
        var first = _cache.GetOrLoad(testFile);
        var second = _cache.GetOrLoad(testFile);

        // Assert
        first.Should().NotBeNull();
        second.Should().BeSameAs(first);
        _cache.Count.Should().Be(1);
    }

    /// <summary>
    /// Verifies LRU eviction when cache is full.
    /// </summary>
    [Test]
    public void GetOrLoad_EvictsOldestWhenFull()
    {
        // Arrange
        var file1 = CreateTestFile("file1.ogg");
        var file2 = CreateTestFile("file2.ogg");
        var file3 = CreateTestFile("file3.ogg");
        var file4 = CreateTestFile("file4.ogg");

        // Act - Fill cache to max (3)
        _cache.GetOrLoad(file1);
        _cache.GetOrLoad(file2);
        _cache.GetOrLoad(file3);
        _cache.Count.Should().Be(3);

        // Add one more, should evict oldest (file1)
        _cache.GetOrLoad(file4);

        // Assert
        _cache.Count.Should().Be(3);
        _cache.Contains(file1).Should().BeFalse("file1 should be evicted");
        _cache.Contains(file4).Should().BeTrue();
    }

    /// <summary>
    /// Verifies GetOrLoad returns null for non-existent file.
    /// </summary>
    [Test]
    public void GetOrLoad_ReturnsNullForMissingFile()
    {
        // Act
        var result = _cache.GetOrLoad("/nonexistent/file.ogg");

        // Assert
        result.Should().BeNull();
        _cache.Count.Should().Be(0);
    }

    /// <summary>
    /// Verifies Clear empties the cache.
    /// </summary>
    [Test]
    public void Clear_EmptiesCache()
    {
        // Arrange
        var file1 = CreateTestFile("file1.ogg");
        var file2 = CreateTestFile("file2.ogg");
        _cache.GetOrLoad(file1);
        _cache.GetOrLoad(file2);
        _cache.Count.Should().Be(2);

        // Act
        _cache.Clear();

        // Assert
        _cache.Count.Should().Be(0);
    }

    private string CreateTestFile(string name)
    {
        var path = Path.Combine(_testDir, name);
        File.WriteAllBytes(path, new byte[] { 0x00, 0x01, 0x02 });
        return path;
    }
}
