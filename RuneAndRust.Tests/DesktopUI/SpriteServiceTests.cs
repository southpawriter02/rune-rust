using RuneAndRust.DesktopUI.Models;
using RuneAndRust.DesktopUI.Services;
using Serilog;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for SpriteService.
/// v0.43.21: Updated tests for current SpriteService implementation with caching.
/// </summary>
public class SpriteServiceTests
{
    private readonly ILogger _logger;

    public SpriteServiceTests()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    [Fact]
    public void SpriteService_RegisterSprite_StoresSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();

        // Act
        service.RegisterSprite("test", sprite);

        // Assert
        Assert.True(service.SpriteExists("test"));
        Assert.Contains("test", service.GetAvailableSprites());
    }

    [Fact]
    public void SpriteService_RegisterSprite_ThrowsOnInvalidSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var invalidSprite = new PixelSprite
        {
            Name = "invalid",
            PixelData = new string[15],  // Invalid - should be 16
            Palette = new Dictionary<char, string>()
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.RegisterSprite("invalid", invalidSprite));
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_ReturnsNullForMissingSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);

        // Act
        var bitmap = service.GetSpriteBitmap("nonexistent");

        // Assert
        Assert.Null(bitmap);
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_ReturnsBitmapForExistingSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Act
        var bitmap = service.GetSpriteBitmap("test", scale: 3);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(48, bitmap.Width);  // 16 * 3
        Assert.Equal(48, bitmap.Height);
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_CachesBitmaps()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Act
        var bitmap1 = service.GetSpriteBitmap("test", scale: 3);
        var bitmap2 = service.GetSpriteBitmap("test", scale: 3);

        // Assert
        Assert.Same(bitmap1, bitmap2);  // Should return same cached instance
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_CachesDifferentScalesSeparately()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Act
        var bitmap3x = service.GetSpriteBitmap("test", scale: 3);
        var bitmap5x = service.GetSpriteBitmap("test", scale: 5);

        // Assert
        Assert.NotSame(bitmap3x, bitmap5x);
        Assert.Equal(48, bitmap3x!.Width);
        Assert.Equal(80, bitmap5x!.Width);
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_ThrowsOnInvalidScale()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.GetSpriteBitmap("test", scale: 0));
        Assert.Throws<ArgumentException>(() => service.GetSpriteBitmap("test", scale: -1));
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_ThrowsOnNullSpriteName()
    {
        // Arrange
        var service = new SpriteService(_logger);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.GetSpriteBitmap(null!));
        Assert.Throws<ArgumentException>(() => service.GetSpriteBitmap(string.Empty));
    }

    [Fact]
    public void SpriteService_GetAvailableSprites_ReturnsAllSprites()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("sprite1", PixelSprite.CreateTestSprite());
        service.RegisterSprite("sprite2", PixelSprite.CreateTestSprite());
        service.RegisterSprite("sprite3", PixelSprite.CreateTestSprite());

        // Act
        var sprites = service.GetAvailableSprites().ToList();

        // Assert
        Assert.Equal(3, sprites.Count);
        Assert.Contains("sprite1", sprites);
        Assert.Contains("sprite2", sprites);
        Assert.Contains("sprite3", sprites);
    }

    [Fact]
    public void SpriteService_SpriteExists_ReturnsTrueForExistingSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Act & Assert
        Assert.True(service.SpriteExists("test"));
    }

    [Fact]
    public void SpriteService_SpriteExists_ReturnsFalseForMissingSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);

        // Act & Assert
        Assert.False(service.SpriteExists("nonexistent"));
    }

    [Fact]
    public void SpriteService_ClearCache_DisposesAndRemovesBitmaps()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite = PixelSprite.CreateTestSprite();
        service.RegisterSprite("test", sprite);

        // Get bitmap to populate cache
        var bitmap1 = service.GetSpriteBitmap("test", scale: 3);

        // Act
        service.ClearCache();

        // Get bitmap again - should be different instance
        var bitmap2 = service.GetSpriteBitmap("test", scale: 3);

        // Assert
        Assert.NotSame(bitmap1, bitmap2);  // Cache was cleared, new instance created
    }

    [Fact]
    public void SpriteService_LoadSpritesFromDirectory_CreatesDirectoryIfMissing()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var tempPath = Path.Combine(Path.GetTempPath(), $"sprite_test_{Guid.NewGuid()}");

        try
        {
            // Act
            service.LoadSpritesFromDirectory(tempPath);

            // Assert
            Assert.True(Directory.Exists(tempPath));

            // Should have created sample sprites
            Assert.True(service.SpriteExists("player_warrior"));
            Assert.True(service.SpriteExists("enemy_goblin"));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }

    [Fact]
    public void SpriteService_Performance_HandlesManySpritesFast()
    {
        // Arrange
        var service = new SpriteService(_logger);
        const int spriteCount = 100;

        for (int i = 0; i < spriteCount; i++)
        {
            var sprite = PixelSprite.CreateTestSprite();
            sprite.Name = $"sprite_{i}";
            service.RegisterSprite($"sprite_{i}", sprite);
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < spriteCount; i++)
        {
            service.GetSpriteBitmap($"sprite_{i}", scale: 1);
            service.GetSpriteBitmap($"sprite_{i}", scale: 3);
            service.GetSpriteBitmap($"sprite_{i}", scale: 5);
        }

        stopwatch.Stop();

        // Assert - should complete in < 5 seconds (relaxed for CI environments)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Rendering took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
    }

    [Fact]
    public void SpriteService_GetAvailableSprites_ReturnsSortedList()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("zsprite", PixelSprite.CreateTestSprite());
        service.RegisterSprite("asprite", PixelSprite.CreateTestSprite());
        service.RegisterSprite("msprite", PixelSprite.CreateTestSprite());

        // Act
        var sprites = service.GetAvailableSprites().ToList();

        // Assert - Should be sorted alphabetically
        Assert.Equal("asprite", sprites[0]);
        Assert.Equal("msprite", sprites[1]);
        Assert.Equal("zsprite", sprites[2]);
    }

    [Fact]
    public void SpriteService_CacheStatistics_ReturnsCorrectValues()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("test", PixelSprite.CreateTestSprite());

        // Act - Get bitmap to populate cache
        service.GetSpriteBitmap("test", scale: 3);
        var stats = service.GetCacheStatistics();

        // Assert
        Assert.Equal(1, stats.CachedItemCount);
        Assert.True(stats.CurrentSizeBytes > 0);
        Assert.True(stats.MaxSizeBytes > 0);
    }

    [Fact]
    public void SpriteService_RegisterSprite_OverwritesExisting()
    {
        // Arrange
        var service = new SpriteService(_logger);
        var sprite1 = PixelSprite.CreateTestSprite();
        sprite1.Name = "original";
        var sprite2 = PixelSprite.CreateTestSprite();
        sprite2.Name = "updated";

        // Act
        service.RegisterSprite("test", sprite1);
        service.RegisterSprite("test", sprite2);

        // Assert - Should have only one sprite with key "test"
        var sprites = service.GetAvailableSprites().ToList();
        Assert.Single(sprites.Where(s => s == "test"));
    }

    [Fact]
    public void SpriteService_GetSpriteBitmap_Scale1_Returns16x16()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("test", PixelSprite.CreateTestSprite());

        // Act
        var bitmap = service.GetSpriteBitmap("test", scale: 1);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(16, bitmap.Width);
        Assert.Equal(16, bitmap.Height);
    }

    [Fact]
    public void SpriteService_CachedBitmapCount_TracksCorrectly()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("sprite1", PixelSprite.CreateTestSprite());
        service.RegisterSprite("sprite2", PixelSprite.CreateTestSprite());

        // Initially no cached bitmaps
        Assert.Equal(0, service.CachedBitmapCount);

        // Act - Get bitmaps to populate cache
        service.GetSpriteBitmap("sprite1", scale: 3);
        Assert.Equal(1, service.CachedBitmapCount);

        service.GetSpriteBitmap("sprite2", scale: 3);
        Assert.Equal(2, service.CachedBitmapCount);

        service.GetSpriteBitmap("sprite1", scale: 5);  // Different scale
        Assert.Equal(3, service.CachedBitmapCount);
    }

    [Fact]
    public void SpriteService_ClearCache_ResetsCacheStatistics()
    {
        // Arrange
        var service = new SpriteService(_logger);
        service.RegisterSprite("test", PixelSprite.CreateTestSprite());
        service.GetSpriteBitmap("test", scale: 3);

        Assert.True(service.CachedBitmapCount > 0);

        // Act
        service.ClearCache();

        // Assert
        Assert.Equal(0, service.CachedBitmapCount);
        Assert.Equal(0, service.CurrentCacheSize);
    }

    [Fact]
    public void SpriteService_Constructor_ThrowsOnNullLogger()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SpriteService(null!));
    }

    [Fact]
    public void SpriteService_RegisterSprite_ThrowsOnNullSprite()
    {
        // Arrange
        var service = new SpriteService(_logger);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.RegisterSprite("test", null!));
    }
}
