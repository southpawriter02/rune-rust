using SkiaSharp;
using System;
using System.Linq;

namespace RuneAndRust.DesktopUI.Models;

/// <summary>
/// Represents a 16×16 pixel art sprite with palette-based coloring.
/// </summary>
public class PixelSprite
{
    /// <summary>
    /// Gets or sets the sprite name (unique identifier).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pixel data as 16 rows of 16 characters.
    /// Space (' ') represents transparent pixels.
    /// </summary>
    public string[] PixelData { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the color palette mapping characters to hex colors.
    /// Format: #RRGGBB or #RRGGBBAA
    /// </summary>
    public Dictionary<char, string> Palette { get; set; } = new();

    /// <summary>
    /// Validates the sprite format for correctness.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when sprite format is invalid.</exception>
    public void Validate()
    {
        // Validate row count
        if (PixelData.Length != 16)
        {
            throw new InvalidOperationException(
                $"Sprite '{Name}' must have exactly 16 rows (found {PixelData.Length})");
        }

        // Validate each row length
        for (int i = 0; i < PixelData.Length; i++)
        {
            if (PixelData[i].Length != 16)
            {
                throw new InvalidOperationException(
                    $"Sprite '{Name}' row {i} must have exactly 16 pixels (found {PixelData[i].Length})");
            }
        }

        // Validate all characters in pixel data are either space or in palette
        var usedChars = PixelData.SelectMany(row => row).Distinct();
        foreach (var ch in usedChars)
        {
            if (ch != ' ' && !Palette.ContainsKey(ch))
            {
                throw new InvalidOperationException(
                    $"Sprite '{Name}' uses character '{ch}' which is not defined in the palette");
            }
        }

        // Validate palette colors can be parsed
        foreach (var kvp in Palette)
        {
            try
            {
                SKColor.Parse(kvp.Value);
            }
            catch
            {
                throw new InvalidOperationException(
                    $"Sprite '{Name}' has invalid color '{kvp.Value}' for character '{kvp.Key}'");
            }
        }
    }

    /// <summary>
    /// Renders the sprite to a SKBitmap at the specified scale.
    /// </summary>
    /// <param name="scale">The scale factor (1 = 16×16, 3 = 48×48, 5 = 80×80).</param>
    /// <returns>A rendered SKBitmap.</returns>
    public SKBitmap ToBitmap(int scale = 3)
    {
        if (scale < 1)
            throw new ArgumentException("Scale must be at least 1", nameof(scale));

        var bitmap = new SKBitmap(16 * scale, 16 * scale);

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                char pixel = PixelData[y][x];

                // Skip transparent pixels
                if (pixel == ' ')
                    continue;

                // Get color from palette
                if (Palette.TryGetValue(pixel, out var colorHex))
                {
                    var color = SKColor.Parse(colorHex);
                    using var paint = new SKPaint
                    {
                        Color = color,
                        IsAntialias = false,  // Pixel art should be sharp
                        Style = SKPaintStyle.Fill
                    };

                    // Draw the pixel as a scaled rectangle
                    canvas.DrawRect(x * scale, y * scale, scale, scale, paint);
                }
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Creates a simple test sprite for demonstration purposes.
    /// </summary>
    public static PixelSprite CreateTestSprite()
    {
        return new PixelSprite
        {
            Name = "test_square",
            PixelData = new string[]
            {
                "                ",
                " ############## ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " #            # ",
                " ############## ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { '#', "#4A90E2" }  // Blue
            }
        };
    }
}
