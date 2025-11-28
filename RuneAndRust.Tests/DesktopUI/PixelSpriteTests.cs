using RuneAndRust.DesktopUI.Models;
using SkiaSharp;
using Xunit;

namespace RuneAndRust.Tests.DesktopUI;

/// <summary>
/// Tests for PixelSprite model.
/// v0.43.21: Updated tests for current PixelSprite implementation.
/// </summary>
public class PixelSpriteTests
{
    [Fact]
    public void PixelSprite_Validate_SucceedsForValidSprite()
    {
        // Arrange
        var sprite = CreateValidSprite();

        // Act & Assert - should not throw
        sprite.Validate();
    }

    [Fact]
    public void PixelSprite_Validate_ThrowsWhenRowCountInvalid()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "invalid",
            PixelData = new string[15],  // Should be 16
            Palette = new Dictionary<char, string>()
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sprite.Validate());
        Assert.Contains("must have exactly 16 rows", exception.Message);
    }

    [Fact]
    public void PixelSprite_Validate_ThrowsWhenRowLengthInvalid()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "invalid",
            PixelData = Enumerable.Repeat("###", 16).ToArray(),  // Too short
            Palette = new Dictionary<char, string> { { '#', "#000000" } }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sprite.Validate());
        Assert.Contains("must have exactly 16 pixels", exception.Message);
    }

    [Fact]
    public void PixelSprite_Validate_ThrowsWhenPaletteMissingCharacter()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "invalid",
            PixelData = Enumerable.Repeat("################", 16).ToArray(),
            Palette = new Dictionary<char, string>()  // Missing '#' character
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sprite.Validate());
        Assert.Contains("not defined in the palette", exception.Message);
    }

    [Fact]
    public void PixelSprite_Validate_ThrowsWhenColorInvalid()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "invalid",
            PixelData = Enumerable.Repeat("################", 16).ToArray(),
            Palette = new Dictionary<char, string> { { '#', "not-a-color" } }
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sprite.Validate());
        Assert.Contains("invalid color", exception.Message);
    }

    [Fact]
    public void PixelSprite_ToBitmap_RendersAtScale1()
    {
        // Arrange
        var sprite = CreateValidSprite();

        // Act
        var bitmap = sprite.ToBitmap(scale: 1);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(16, bitmap.Width);
        Assert.Equal(16, bitmap.Height);
    }

    [Fact]
    public void PixelSprite_ToBitmap_RendersAtScale3()
    {
        // Arrange
        var sprite = CreateValidSprite();

        // Act
        var bitmap = sprite.ToBitmap(scale: 3);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(48, bitmap.Width);  // 16 * 3
        Assert.Equal(48, bitmap.Height);
    }

    [Fact]
    public void PixelSprite_ToBitmap_RendersAtScale5()
    {
        // Arrange
        var sprite = CreateValidSprite();

        // Act
        var bitmap = sprite.ToBitmap(scale: 5);

        // Assert
        Assert.NotNull(bitmap);
        Assert.Equal(80, bitmap.Width);  // 16 * 5
        Assert.Equal(80, bitmap.Height);
    }

    [Fact]
    public void PixelSprite_ToBitmap_HandlesTransparentPixels()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "transparent_test",
            PixelData = Enumerable.Repeat("                ", 16).ToArray(),  // All transparent
            Palette = new Dictionary<char, string>()
        };

        // Act
        var bitmap = sprite.ToBitmap(scale: 1);

        // Assert
        Assert.NotNull(bitmap);

        // Sample a pixel - should be transparent
        var centerPixel = bitmap.GetPixel(8, 8);
        Assert.Equal(SKColors.Transparent, centerPixel);
    }

    [Fact]
    public void PixelSprite_ToBitmap_RendersCorrectColors()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "color_test",
            PixelData = new[]
            {
                "R               ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'R', "#FF0000" }  // Pure red
            }
        };

        // Act
        var bitmap = sprite.ToBitmap(scale: 1);

        // Assert
        var topLeftPixel = bitmap.GetPixel(0, 0);
        Assert.Equal(SKColors.Red, topLeftPixel);
    }

    [Fact]
    public void PixelSprite_ToBitmap_ThrowsWhenScaleInvalid()
    {
        // Arrange
        var sprite = CreateValidSprite();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => sprite.ToBitmap(scale: 0));
        Assert.Throws<ArgumentException>(() => sprite.ToBitmap(scale: -1));
    }

    [Fact]
    public void PixelSprite_CreateTestSprite_ReturnsValidSprite()
    {
        // Act
        var sprite = PixelSprite.CreateTestSprite();

        // Assert
        Assert.NotNull(sprite);
        Assert.Equal("test_square", sprite.Name);
        sprite.Validate();  // Should not throw
    }

    [Fact]
    public void PixelSprite_ToBitmap_RendersMultipleColors()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "multi_color",
            PixelData = new[]
            {
                "RG              ",
                "BW              ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'R', "#FF0000" },  // Red
                { 'G', "#00FF00" },  // Green
                { 'B', "#0000FF" },  // Blue
                { 'W', "#FFFFFF" }   // White
            }
        };

        // Act
        var bitmap = sprite.ToBitmap(scale: 1);

        // Assert
        Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));
        Assert.Equal(SKColors.Lime, bitmap.GetPixel(1, 0));  // Green is Lime in SKColors
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(0, 1));
        Assert.Equal(SKColors.White, bitmap.GetPixel(1, 1));
    }

    [Fact]
    public void PixelSprite_ToBitmap_ScaledPixelsAreFilled()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "scale_test",
            PixelData = new[]
            {
                "R               ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                ",
                "                "
            },
            Palette = new Dictionary<char, string>
            {
                { 'R', "#FF0000" }
            }
        };

        // Act
        var bitmap = sprite.ToBitmap(scale: 3);

        // Assert - All pixels in the scaled region should be red
        Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(1, 1));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(2, 2));
        // Adjacent pixel should be transparent
        Assert.Equal(SKColors.Transparent, bitmap.GetPixel(3, 0));
    }

    [Fact]
    public void PixelSprite_Validate_AllowsAllTransparentSprite()
    {
        // Arrange
        var sprite = new PixelSprite
        {
            Name = "all_transparent",
            PixelData = Enumerable.Repeat("                ", 16).ToArray(),
            Palette = new Dictionary<char, string>()
        };

        // Act & Assert - Should not throw
        sprite.Validate();
    }

    [Fact]
    public void PixelSprite_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var sprite = new PixelSprite();

        // Assert
        Assert.Equal(string.Empty, sprite.Name);
        Assert.NotNull(sprite.PixelData);
        Assert.Empty(sprite.PixelData);
        Assert.NotNull(sprite.Palette);
        Assert.Empty(sprite.Palette);
    }

    private PixelSprite CreateValidSprite()
    {
        return new PixelSprite
        {
            Name = "test",
            PixelData = Enumerable.Repeat("################", 16).ToArray(),
            Palette = new Dictionary<char, string>
            {
                { '#', "#4A90E2" }  // Blue
            }
        };
    }
}
