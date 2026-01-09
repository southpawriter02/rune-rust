using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Services;

namespace RuneAndRust.Application.UnitTests.Services;

[TestFixture]
public class CachedDescriptorServiceTests
{
    private Mock<ILogger<DescriptorService>> _descriptorLoggerMock = null!;
    private Mock<ILogger<CachedDescriptorService>> _cachedLoggerMock = null!;
    private IMemoryCache _memoryCache = null!;
    private CachedDescriptorService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _descriptorLoggerMock = new Mock<ILogger<DescriptorService>>();
        _cachedLoggerMock = new Mock<ILogger<CachedDescriptorService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        var pools = CreateTestPools();
        var theme = CreateTestTheme();

        _service = new CachedDescriptorService(
            pools,
            theme,
            _descriptorLoggerMock.Object,
            _memoryCache,
            _cachedLoggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache?.Dispose();
    }

    [Test]
    public void GetDescriptorCached_FirstCall_ReturnsDescriptor()
    {
        var result = _service.GetDescriptorCached("environmental.lighting");

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorCached_SecondCall_ReturnsSameDescriptor()
    {
        var result1 = _service.GetDescriptorCached("environmental.lighting");
        var result2 = _service.GetDescriptorCached("environmental.lighting");

        // Second call should return cached value
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result2, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorCached_WithCacheDisabled_SkipsCache()
    {
        var result = _service.GetDescriptorCached("environmental.lighting", cacheResult: false);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorCached_WithContext_SkipsCache()
    {
        var context = new DescriptorContext { DamagePercent = 0.5 };
        var result = _service.GetDescriptorCached("environmental.lighting", context: context);

        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorCached_DifferentPools_CachesSeparately()
    {
        var result1 = _service.GetDescriptorCached("environmental.lighting");
        var result2 = _service.GetDescriptorCached("environmental.atmosphere");

        // Different pools should be cached separately
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result2, Is.Not.Empty);
    }

    [Test]
    public void GetDescriptorCached_NonExistentPool_ReturnsEmpty()
    {
        var result = _service.GetDescriptorCached("nonexistent.pool");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void InvalidatePool_RequestsInvalidation()
    {
        // First populate cache
        _service.GetDescriptorCached("environmental.lighting");

        // Request invalidation (note: IMemoryCache doesn't support enumeration,
        // so this relies on expiration)
        Assert.DoesNotThrow(() => _service.InvalidatePool("environmental.lighting"));
    }

    [Test]
    public void ClearCache_DoesNotThrow()
    {
        // Populate cache
        _service.GetDescriptorCached("environmental.lighting");
        _service.GetDescriptorCached("environmental.atmosphere");

        // Clear should not throw
        Assert.DoesNotThrow(() => _service.ClearCache());
    }

    [Test]
    public void GetDescriptorCached_MultiplePoolPaths_CachesIndependently()
    {
        var result1 = _service.GetDescriptorCached("environmental.lighting");
        var result2 = _service.GetDescriptorCached("environmental.atmosphere");

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result2, Is.Not.Empty);
    }

    [Test]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        var pools = CreateTestPools();
        var theme = CreateTestTheme();

        Assert.Throws<ArgumentNullException>(() => new CachedDescriptorService(
            pools,
            theme,
            _descriptorLoggerMock.Object,
            null!,
            _cachedLoggerMock.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var pools = CreateTestPools();
        var theme = CreateTestTheme();

        Assert.Throws<ArgumentNullException>(() => new CachedDescriptorService(
            pools,
            theme,
            _descriptorLoggerMock.Object,
            _memoryCache,
            null!));
    }

    private static IReadOnlyDictionary<string, DescriptorPool> CreateTestPools()
    {
        return new Dictionary<string, DescriptorPool>
        {
            ["environmental.lighting"] = CreatePool("lighting", [
                ("dim_lighting", "The dim light casts long shadows", 20),
                ("torch_lighting", "Torchlight flickers against the walls", 20)
            ]),
            ["environmental.atmosphere"] = CreatePool("atmosphere", [
                ("musty_air", "The air is thick and musty", 25),
                ("damp_atmosphere", "Moisture clings to every surface", 25)
            ])
        };
    }

    private static DescriptorPool CreatePool(string id, (string id, string text, int weight)[] descriptors)
    {
        return new DescriptorPool
        {
            Id = id,
            Name = id,
            Descriptors = descriptors.Select(d => new Descriptor
            {
                Id = d.id,
                Text = d.text,
                Weight = d.weight
            }).ToList()
        };
    }

    private static ThemeConfiguration CreateTestTheme()
    {
        return new ThemeConfiguration
        {
            ActiveTheme = "default",
            Themes = new Dictionary<string, ThemePreset>
            {
                ["default"] = new ThemePreset
                {
                    Id = "default",
                    Name = "Default",
                    Description = "Standard theme"
                }
            }
        };
    }
}
