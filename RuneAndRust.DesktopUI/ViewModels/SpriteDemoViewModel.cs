using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// View model for demonstrating the sprite system.
/// Displays all loaded sprites at various scales.
/// </summary>
public class SpriteDemoViewModel : ViewModelBase
{
    private readonly ISpriteService _spriteService;
    private int _scale = 3;
    private string? _selectedSprite;

    /// <summary>
    /// Gets the collection of all available sprite names.
    /// </summary>
    public ObservableCollection<SpriteDisplayInfo> AvailableSprites { get; } = new();

    /// <summary>
    /// Gets or sets the current scale factor.
    /// </summary>
    public int Scale
    {
        get => _scale;
        set
        {
            this.RaiseAndSetIfChanged(ref _scale, value);
            RefreshSprites();
        }
    }

    /// <summary>
    /// Gets or sets the selected sprite name.
    /// </summary>
    public string? SelectedSprite
    {
        get => _selectedSprite;
        set => this.RaiseAndSetIfChanged(ref _selectedSprite, value);
    }

    /// <summary>
    /// Gets the title for the demo view.
    /// </summary>
    public string Title => "Sprite System Demo";

    /// <summary>
    /// Gets the description for the demo view.
    /// </summary>
    public string Description => $"Displaying {AvailableSprites.Count} sprites at {Scale}x scale";

    /// <summary>
    /// Initializes a new instance of SpriteDemoViewModel.
    /// </summary>
    public SpriteDemoViewModel(ISpriteService spriteService)
    {
        _spriteService = spriteService ?? throw new ArgumentNullException(nameof(spriteService));
        LoadSprites();
    }

    /// <summary>
    /// Loads all available sprites from the sprite service.
    /// </summary>
    private void LoadSprites()
    {
        AvailableSprites.Clear();

        foreach (var spriteName in _spriteService.GetAvailableSprites())
        {
            var bitmap = _spriteService.GetSpriteBitmap(spriteName, _scale);
            if (bitmap != null)
            {
                AvailableSprites.Add(new SpriteDisplayInfo
                {
                    Name = spriteName,
                    Bitmap = bitmap,
                    Width = bitmap.Width,
                    Height = bitmap.Height
                });
            }
        }

        this.RaisePropertyChanged(nameof(Description));
    }

    /// <summary>
    /// Refreshes sprites at the new scale.
    /// </summary>
    private void RefreshSprites()
    {
        foreach (var sprite in AvailableSprites)
        {
            var bitmap = _spriteService.GetSpriteBitmap(sprite.Name, _scale);
            if (bitmap != null)
            {
                sprite.Bitmap = bitmap;
                sprite.Width = bitmap.Width;
                sprite.Height = bitmap.Height;
            }
        }

        this.RaisePropertyChanged(nameof(Description));
    }
}

/// <summary>
/// Display information for a single sprite.
/// </summary>
public class SpriteDisplayInfo : ReactiveObject
{
    private SKBitmap? _bitmap;
    private int _width;
    private int _height;

    /// <summary>
    /// Gets or sets the sprite name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered bitmap.
    /// </summary>
    public SKBitmap? Bitmap
    {
        get => _bitmap;
        set => this.RaiseAndSetIfChanged(ref _bitmap, value);
    }

    /// <summary>
    /// Gets or sets the bitmap width.
    /// </summary>
    public int Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    /// <summary>
    /// Gets or sets the bitmap height.
    /// </summary>
    public int Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }
}
