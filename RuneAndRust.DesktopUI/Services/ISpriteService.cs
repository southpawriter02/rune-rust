using SkiaSharp;

namespace RuneAndRust.DesktopUI.Services;

/// <summary>
/// Service for loading, caching, and retrieving sprite bitmaps.
/// </summary>
public interface ISpriteService
{
    /// <summary>
    /// Gets a rendered sprite bitmap at the specified scale.
    /// Returns cached version if available.
    /// </summary>
    /// <param name="spriteName">The name of the sprite.</param>
    /// <param name="scale">The scale factor (1, 3, 5, etc.).</param>
    /// <returns>The rendered bitmap, or null if sprite not found.</returns>
    SKBitmap? GetSpriteBitmap(string spriteName, int scale = 3);

    /// <summary>
    /// Registers a sprite programmatically.
    /// </summary>
    /// <param name="name">The unique sprite name.</param>
    /// <param name="sprite">The sprite definition.</param>
    void RegisterSprite(string name, Models.PixelSprite sprite);

    /// <summary>
    /// Gets all available sprite names.
    /// </summary>
    IEnumerable<string> GetAvailableSprites();

    /// <summary>
    /// Checks if a sprite with the given name exists.
    /// </summary>
    bool SpriteExists(string name);

    /// <summary>
    /// Loads all sprites from a directory containing JSON files.
    /// </summary>
    /// <param name="directoryPath">Path to the sprite directory.</param>
    void LoadSpritesFromDirectory(string directoryPath);

    /// <summary>
    /// Clears the bitmap cache and disposes of cached resources.
    /// </summary>
    void ClearCache();
}
