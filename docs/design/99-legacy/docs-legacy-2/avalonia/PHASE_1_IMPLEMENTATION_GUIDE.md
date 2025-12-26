# Phase 1 Implementation Guide
# Avalonia UI Foundation

**Duration:** Week 1 (~16 hours)
**Status:** Ready to Begin
**Dependencies:** None

---

## Overview

Phase 1 establishes the foundational infrastructure for the Avalonia desktop UI. By the end of this phase, you'll have a working application that can launch, display sprites, and demonstrate basic navigation.

**Key Deliverables:**
- ✅ Avalonia project created and building successfully
- ✅ MVVM infrastructure (base classes, DI container)
- ✅ Sprite data structures and rendering system
- ✅ Basic window with navigation framework
- ✅ Example views demonstrating capabilities

---

## Prerequisites

### Development Environment Setup

**Required Tools:**
1. **.NET 8.0 SDK** - Download from https://dotnet.microsoft.com/download
2. **IDE** - Choose one:
   - JetBrains Rider (Recommended - best Avalonia support)
   - Visual Studio 2022 (with .NET desktop workload)
   - Visual Studio Code (with C# extension)

3. **Avalonia Templates:**
   ```bash
   dotnet new install Avalonia.Templates
   ```

**Verify Installation:**
```bash
dotnet --version  # Should show 8.0.x
dotnet new list   # Should show Avalonia templates
```

### Knowledge Requirements

**Must Have:**
- C# fundamentals
- Basic XAML knowledge
- Understanding of MVVM pattern

**Nice to Have:**
- Prior WPF experience
- Reactive programming familiarity
- SkiaSharp knowledge

---

## Step-by-Step Implementation

### Task 1: Create Avalonia Project (1 hour)

**Objective:** Set up the base Avalonia project structure

**Commands:**
```bash
cd /home/user/rune-rust

# Create new Avalonia MVVM project
dotnet new avalonia.mvvm -o RuneAndRust.DesktopUI -n RuneAndRust.DesktopUI

# Add to solution
dotnet sln add RuneAndRust.DesktopUI/RuneAndRust.DesktopUI.csproj

# Add project references
cd RuneAndRust.DesktopUI
dotnet add reference ../RuneAndRust.Core/RuneAndRust.Core.csproj
dotnet add reference ../RuneAndRust.Engine/RuneAndRust.Engine.csproj
dotnet add reference ../RuneAndRust.Persistence/RuneAndRust.Persistence.csproj

# Verify build
dotnet build
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Files Created:**
- `RuneAndRust.DesktopUI/Program.cs`
- `RuneAndRust.DesktopUI/App.axaml`
- `RuneAndRust.DesktopUI/App.axaml.cs`
- `RuneAndRust.DesktopUI/ViewModels/MainWindowViewModel.cs`
- `RuneAndRust.DesktopUI/Views/MainWindow.axaml`

**Test:**
```bash
dotnet run --project RuneAndRust.DesktopUI
```

Should launch a basic window with "Welcome to Avalonia!" text.

---

### Task 2: Configure Project Settings (30 minutes)

**Objective:** Set up project metadata and build configuration

**Edit:** `RuneAndRust.DesktopUI.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <ApplicationIcon>Assets/aethelgard-icon.ico</ApplicationIcon>
    <AssemblyName>RuneAndRust</AssemblyName>
    <RootNamespace>RuneAndRust.DesktopUI</RootNamespace>

    <!-- Application Metadata -->
    <AssemblyTitle>Rune &amp; Rust</AssemblyTitle>
    <Product>Rune &amp; Rust</Product>
    <Company>Aethelgard Games</Company>
    <Copyright>Copyright © 2025</Copyright>
    <Version>0.40.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <AvaloniaResource Include="Assets\**" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.2.0" />
    <PackageReference Include="Avalonia.Desktop" Version="11.2.0" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.2.0" />
    <PackageReference Include="Avalonia.ReactiveUI" Version="11.2.0" />
    <PackageReference Include="Avalonia.Diagnostics" Version="11.2.0" Condition="'$(Configuration)' == 'Debug'" />

    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Serilog" Version="4.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RuneAndRust.Core\RuneAndRust.Core.csproj" />
    <ProjectReference Include="..\RuneAndRust.Engine\RuneAndRust.Engine.csproj" />
    <ProjectReference Include="..\RuneAndRust.Persistence\RuneAndRust.Persistence.csproj" />
  </ItemGroup>
</Project>
```

**Restore Packages:**
```bash
dotnet restore
```

---

### Task 3: Set Up Dependency Injection (2 hours)

**Objective:** Configure DI container for service registration

**Create:** `RuneAndRust.DesktopUI/ServiceConfiguration.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using RuneAndRust.DesktopUI.Services;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.Engine;
using Serilog;

namespace RuneAndRust.DesktopUI;

public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/desktop-ui-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSingleton<ILogger>(Log.Logger);

        // Register UI Services
        services.AddSingleton<SpriteService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<ConfigurationService>();

        // Register Engine Services (from existing project)
        services.AddSingleton<CombatEngine>();
        services.AddSingleton<DungeonGenerator>();
        services.AddSingleton<EnemyAIService>();
        // ... add other engine services as needed

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CombatViewModel>();
        services.AddTransient<CharacterSheetViewModel>();
        services.AddTransient<DungeonViewModel>();

        return services.BuildServiceProvider();
    }
}
```

**Update:** `Program.cs`

```csharp
using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace RuneAndRust.DesktopUI;

class Program
{
    public static IServiceProvider Services { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Configure services
            Services = ServiceConfiguration.ConfigureServices();

            // Build and run Avalonia app
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex}");
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

**Update:** `App.axaml.cs`

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RuneAndRust.DesktopUI.ViewModels;
using RuneAndRust.DesktopUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace RuneAndRust.DesktopUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Program.Services.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

---

### Task 4: Create Base ViewModel Class (1 hour)

**Objective:** Establish MVVM foundation with ReactiveUI

**Create:** `RuneAndRust.DesktopUI/ViewModels/ViewModelBase.cs`

```csharp
using ReactiveUI;
using System.Runtime.CompilerServices;

namespace RuneAndRust.DesktopUI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// Helper method for raising property changed events
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        this.RaisePropertyChanged(propertyName);
        return true;
    }
}
```

**Update:** `ViewModels/MainWindowViewModel.cs`

```csharp
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _title = "AETHELGARD - Rune & Rust";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    // Commands
    public ICommand NavigateToCombatCommand { get; }
    public ICommand NavigateToCharacterCommand { get; }
    public ICommand NavigateToDungeonCommand { get; }

    public MainWindowViewModel()
    {
        // Initialize with a default view
        _currentView = this;

        // Set up commands
        NavigateToCombatCommand = ReactiveCommand.Create(NavigateToCombat);
        NavigateToCharacterCommand = ReactiveCommand.Create(NavigateToCharacter);
        NavigateToDungeonCommand = ReactiveCommand.Create(NavigateToDungeon);
    }

    private void NavigateToCombat()
    {
        StatusMessage = "Loading combat view...";
        // TODO: Navigate to combat view
    }

    private void NavigateToCharacter()
    {
        StatusMessage = "Loading character sheet...";
        // TODO: Navigate to character view
    }

    private void NavigateToDungeon()
    {
        StatusMessage = "Loading dungeon exploration...";
        // TODO: Navigate to dungeon view
    }
}
```

---

### Task 5: Implement Sprite Data Structures (3 hours)

**Objective:** Create system for loading and rendering 16x16 pixel sprites

**Create:** `RuneAndRust.DesktopUI/Models/PixelSprite.cs`

```csharp
using SkiaSharp;
using System.Collections.Generic;

namespace RuneAndRust.DesktopUI.Models;

public class PixelSprite
{
    public string Name { get; set; } = string.Empty;
    public string[] PixelData { get; set; } = Array.Empty<string>();
    public Dictionary<char, string> Palette { get; set; } = new();

    /// <summary>
    /// Convert sprite to SkiaSharp bitmap at specified scale
    /// </summary>
    public SKBitmap ToBitmap(int scale = 3)
    {
        if (PixelData.Length != 16 || PixelData.Any(row => row.Length != 16))
            throw new InvalidOperationException("Sprite must be 16x16 pixels");

        var bitmap = new SKBitmap(16 * scale, 16 * scale);
        using var canvas = new SKCanvas(bitmap);

        // Parse palette colors
        var colorPalette = Palette.ToDictionary(
            kvp => kvp.Key,
            kvp => ParseColor(kvp.Value)
        );

        // Render each pixel
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                char pixelChar = PixelData[y][x];

                if (colorPalette.TryGetValue(pixelChar, out SKColor color)
                    && color != SKColors.Transparent)
                {
                    using var paint = new SKPaint
                    {
                        Color = color,
                        IsAntialias = false,  // Crisp pixels
                        Style = SKPaintStyle.Fill
                    };

                    canvas.DrawRect(
                        x * scale,
                        y * scale,
                        scale,
                        scale,
                        paint
                    );
                }
            }
        }

        return bitmap;
    }

    private static SKColor ParseColor(string colorString)
    {
        if (colorString.Equals("transparent", StringComparison.OrdinalIgnoreCase))
            return SKColors.Transparent;

        // Parse hex color (#RRGGBB)
        if (colorString.StartsWith("#") && colorString.Length == 7)
        {
            byte r = Convert.ToByte(colorString.Substring(1, 2), 16);
            byte g = Convert.ToByte(colorString.Substring(3, 2), 16);
            byte b = Convert.ToByte(colorString.Substring(5, 2), 16);
            return new SKColor(r, g, b);
        }

        // Try parsing named color
        if (SKColors.TryParse(colorString, out SKColor namedColor))
            return namedColor;

        throw new ArgumentException($"Invalid color format: {colorString}");
    }
}
```

**Create:** `RuneAndRust.DesktopUI/Models/SpriteDefinitions.cs`

```csharp
namespace RuneAndRust.DesktopUI.Models;

public static class SpriteDefinitions
{
    public static PixelSprite Shieldmaiden => new()
    {
        Name = "Shieldmaiden",
        PixelData = new[]
        {
            "0000000110000000",
            "0000001111000000",
            "0000011111100000",
            "0000111111110000",
            "0001122222211000",
            "0001223333221000",
            "0001233443321000",
            "0001234554321000",
            "0001234554321000",
            "0001122662211000",
            "0000112662110000",
            "0000011771100000",
            "0000001771000000",
            "0000011661100000",
            "0000116666110000",
            "0001166666661000",
        },
        Palette = new()
        {
            { '0', "transparent" },
            { '1', "#2a2a2a" },  // outline/shadow
            { '2', "#8b4513" },  // dark brown (hair/leather)
            { '3', "#a0522d" },  // medium brown
            { '4', "#daa520" },  // gold (shield details)
            { '5', "#ffd700" },  // bright gold
            { '6', "#c0c0c0" },  // silver (armor)
            { '7', "#e8e8e8" },  // bright silver
        }
    };

    public static PixelSprite Berserker => new()
    {
        Name = "Berserker",
        PixelData = new[]
        {
            "0000001111000000",
            "0000011221100000",
            "0000112222110000",
            "0000123333210000",
            "0001234443211000",
            "0001234443211000",
            "0001223333221000",
            "0001122222211000",
            "0001155555511000",
            "0001156666511000",
            "0001156776511000",
            "0000115775110000",
            "0000011551100000",
            "0000011551100000",
            "0000116666110000",
            "0001166666661000",
        },
        Palette = new()
        {
            { '0', "transparent" },
            { '1', "#1a1a1a" },  // outline
            { '2', "#8b4513" },  // dark brown (beard)
            { '3', "#a0522d" },  // medium brown
            { '4', "#d2691e" },  // light brown
            { '5', "#8b0000" },  // dark red (armor/blood)
            { '6', "#dc143c" },  // crimson
            { '7', "#ff6347" },  // bright red
        }
    };

    // TODO: Add remaining sprites (Runecaster, Greatsword, Draugr, etc.)

    public static Dictionary<string, PixelSprite> GetAllSprites()
    {
        return new Dictionary<string, PixelSprite>
        {
            { "shieldmaiden", Shieldmaiden },
            { "berserker", Berserker },
            // Add more as implemented
        };
    }
}
```

---

### Task 6: Create Sprite Service (2 hours)

**Objective:** Service for loading, caching, and retrieving sprites

**Create:** `RuneAndRust.DesktopUI/Services/SpriteService.cs`

```csharp
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RuneAndRust.DesktopUI.Models;
using SkiaSharp;
using System.Collections.Concurrent;
using System.IO;

namespace RuneAndRust.DesktopUI.Services;

public class SpriteService
{
    private readonly ConcurrentDictionary<string, PixelSprite> _spriteDefinitions = new();
    private readonly ConcurrentDictionary<string, Bitmap> _bitmapCache = new();

    public SpriteService()
    {
        LoadDefaultSprites();
    }

    private void LoadDefaultSprites()
    {
        var sprites = SpriteDefinitions.GetAllSprites();
        foreach (var kvp in sprites)
        {
            _spriteDefinitions[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Get sprite bitmap at specified scale (cached)
    /// </summary>
    public Bitmap? GetSpriteBitmap(string spriteName, int scale = 3)
    {
        var cacheKey = $"{spriteName}_{scale}";

        if (_bitmapCache.TryGetValue(cacheKey, out var cached))
            return cached;

        if (!_spriteDefinitions.TryGetValue(spriteName.ToLowerInvariant(), out var sprite))
            return null;

        // Generate bitmap
        var skBitmap = sprite.ToBitmap(scale);
        var avaloniaBitmap = ConvertToAvaloniaBitmap(skBitmap);

        _bitmapCache[cacheKey] = avaloniaBitmap;
        return avaloniaBitmap;
    }

    /// <summary>
    /// Convert SkiaSharp bitmap to Avalonia bitmap
    /// </summary>
    private static Bitmap ConvertToAvaloniaBitmap(SKBitmap skBitmap)
    {
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();

        data.SaveTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return new Bitmap(stream);
    }

    /// <summary>
    /// Register custom sprite definition
    /// </summary>
    public void RegisterSprite(string name, PixelSprite sprite)
    {
        _spriteDefinitions[name.ToLowerInvariant()] = sprite;

        // Invalidate cache for this sprite
        var keysToRemove = _bitmapCache.Keys
            .Where(k => k.StartsWith($"{name.ToLowerInvariant()}_"))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _bitmapCache.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Get all registered sprite names
    /// </summary>
    public IEnumerable<string> GetAvailableSprites()
    {
        return _spriteDefinitions.Keys;
    }
}
```

---

### Task 7: Create Navigation Service (1.5 hours)

**Objective:** Handle view navigation throughout the application

**Create:** `RuneAndRust.DesktopUI/Services/NavigationService.cs`

```csharp
using RuneAndRust.DesktopUI.ViewModels;
using System;

namespace RuneAndRust.DesktopUI.Services;

public class NavigationService
{
    private ViewModelBase? _currentView;

    public event EventHandler<ViewModelBase>? CurrentViewChanged;

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            CurrentViewChanged?.Invoke(this, value!);
        }
    }

    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
    {
        CurrentView = viewModel;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = Program.Services.GetService(typeof(TViewModel)) as TViewModel;

        if (viewModel == null)
            throw new InvalidOperationException($"ViewModel {typeof(TViewModel).Name} not registered");

        NavigateTo(viewModel);
    }
}
```

---

### Task 8: Update MainWindow Layout (2 hours)

**Objective:** Create professional window layout with navigation

**Update:** `Views/MainWindow.axaml`

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d" d:DesignWidth="1280" d:DesignHeight="720"
        x:Class="RuneAndRust.DesktopUI.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Icon="/Assets/avalonia-logo.ico"
        Title="{Binding Title}"
        Width="1280" Height="720"
        MinWidth="1024" MinHeight="600"
        Background="#0a0a1a"
        FontFamily="Courier New">

  <Grid RowDefinitions="Auto,*,Auto">

    <!-- Title Bar -->
    <Border Grid.Row="0"
            Background="Linear-gradient(180deg, #1a1a2a 0%, #0a0a1a 100%)"
            BorderBrush="#4a4a5a"
            BorderThickness="0,0,0,2"
            Padding="20,16">

      <StackPanel>
        <TextBlock Text="AETHELGARD"
                   FontSize="32"
                   FontWeight="Bold"
                   Foreground="#FFD700"
                   HorizontalAlignment="Center"
                   TextDecorations="None">
          <TextBlock.Effect>
            <DropShadowEffect Color="#8B4513" BlurRadius="8" OffsetX="4" OffsetY="4"/>
          </TextBlock.Effect>
        </TextBlock>

        <TextBlock Text="═══ RUNE &amp; RUST ═══"
                   FontSize="12"
                   Foreground="#87CEEB"
                   HorizontalAlignment="Center"
                   Margin="0,4,0,0"
                   LetterSpacing="3"/>
      </StackPanel>
    </Border>

    <!-- Main Content Area -->
    <Border Grid.Row="1"
            Background="#0a0a0a"
            Padding="20">

      <Grid ColumnDefinitions="200,*">

        <!-- Navigation Menu -->
        <Border Grid.Column="0"
                Background="Linear-gradient(180deg, #1a1a2a 0%, #0a0a1a 100%)"
                BorderBrush="#4a4a5a"
                BorderThickness="2"
                CornerRadius="4"
                Margin="0,0,10,0"
                Padding="12">

          <StackPanel Spacing="8">
            <TextBlock Text="═ NAVIGATION ═"
                       FontSize="14"
                       FontWeight="Bold"
                       Foreground="#FFD700"
                       HorizontalAlignment="Center"
                       Margin="0,0,0,12"/>

            <Button Content="⚔ Combat"
                    Command="{Binding NavigateToCombatCommand}"
                    Classes="nav-button"/>

            <Button Content="👤 Character"
                    Command="{Binding NavigateToCharacterCommand}"
                    Classes="nav-button"/>

            <Button Content="🗺 Dungeon"
                    Command="{Binding NavigateToDungeonCommand}"
                    Classes="nav-button"/>
          </StackPanel>
        </Border>

        <!-- Content Display -->
        <Border Grid.Column="1"
                Background="Linear-gradient(180deg, #1a1a2a 0%, #0a0a1a 100%)"
                BorderBrush="#4a4a5a"
                BorderThickness="2"
                CornerRadius="4"
                Padding="20">

          <ContentControl Content="{Binding CurrentView}"/>
        </Border>
      </Grid>
    </Border>

    <!-- Status Bar -->
    <Border Grid.Row="2"
            Background="Linear-gradient(180deg, #1a1a2a 0%, #0a0a1a 100%)"
            BorderBrush="#4a4a5a"
            BorderThickness="0,2,0,0"
            Padding="20,8">

      <TextBlock Text="{Binding StatusMessage}"
                 Foreground="#00FF00"
                 FontSize="12"/>
    </Border>
  </Grid>

  <!-- Styles -->
  <Window.Styles>
    <Style Selector="Button.nav-button">
      <Setter Property="HorizontalAlignment" Value="Stretch"/>
      <Setter Property="HorizontalContentAlignment" Value="Left"/>
      <Setter Property="Padding" Value="12,8"/>
      <Setter Property="Background" Value="#2a2a3a"/>
      <Setter Property="BorderBrush" Value="#4a4a5a"/>
      <Setter Property="BorderThickness" Value="2"/>
      <Setter Property="Foreground" Value="White"/>
      <Setter Property="FontWeight" Value="Bold"/>
      <Setter Property="FontSize" Value="13"/>
    </Style>

    <Style Selector="Button.nav-button:pointerover">
      <Setter Property="Background" Value="#3a3a4a"/>
      <Setter Property="BorderBrush" Value="#87CEEB"/>
    </Style>

    <Style Selector="Button.nav-button:pressed">
      <Setter Property="Background" Value="#1a1a2a"/>
    </Style>
  </Window.Styles>
</Window>
```

---

### Task 9: Create Sprite Demo View (2 hours)

**Objective:** Create a view that demonstrates sprite rendering

**Create:** `ViewModels/SpriteDemoViewModel.cs`

```csharp
using Avalonia.Media.Imaging;
using RuneAndRust.DesktopUI.Services;
using System.Collections.ObjectModel;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SpriteDemoViewModel : ViewModelBase
{
    private readonly SpriteService _spriteService;

    public ObservableCollection<SpriteDisplayItem> Sprites { get; }

    public SpriteDemoViewModel(SpriteService spriteService)
    {
        _spriteService = spriteService;
        Sprites = new ObservableCollection<SpriteDisplayItem>();

        LoadSprites();
    }

    private void LoadSprites()
    {
        foreach (var spriteName in _spriteService.GetAvailableSprites())
        {
            var item = new SpriteDisplayItem
            {
                Name = spriteName,
                SmallSprite = _spriteService.GetSpriteBitmap(spriteName, scale: 3),
                LargeSprite = _spriteService.GetSpriteBitmap(spriteName, scale: 5)
            };

            Sprites.Add(item);
        }
    }
}

public class SpriteDisplayItem
{
    public string Name { get; set; } = string.Empty;
    public Bitmap? SmallSprite { get; set; }
    public Bitmap? LargeSprite { get; set; }
}
```

**Create:** `Views/SpriteDemoView.axaml`

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.SpriteDemoView"
             x:DataType="vm:SpriteDemoViewModel">

  <ScrollViewer>
    <StackPanel Spacing="20" Margin="20">

      <TextBlock Text="Sprite Rendering Demo"
                 FontSize="24"
                 FontWeight="Bold"
                 Foreground="#FFD700"/>

      <ItemsControl Items="{Binding Sprites}">
        <ItemsControl.ItemsPanel>
          <ItemsPanelTemplate>
            <WrapPanel Orientation="Horizontal" ItemWidth="220"/>
          </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>

        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <Border Background="#2a2a3a"
                    BorderBrush="#4a4a5a"
                    BorderThickness="2"
                    CornerRadius="4"
                    Padding="12"
                    Margin="8">

              <StackPanel Spacing="8">

                <!-- Sprite Name -->
                <TextBlock Text="{Binding Name}"
                           FontWeight="Bold"
                           Foreground="#87CEEB"
                           HorizontalAlignment="Center"
                           TextTransform="Uppercase"/>

                <!-- Small Sprite (3x) -->
                <Border Background="#1a1a2a"
                        BorderBrush="#3a3a4a"
                        BorderThickness="1"
                        Padding="12"
                        HorizontalAlignment="Center">
                  <Image Source="{Binding SmallSprite}"
                         Width="48" Height="48"
                         RenderOptions.BitmapInterpolationMode="None"/>
                </Border>

                <TextBlock Text="Scale: 3x"
                           FontSize="10"
                           Foreground="#888888"
                           HorizontalAlignment="Center"/>

                <!-- Large Sprite (5x) -->
                <Border Background="#1a1a2a"
                        BorderBrush="#3a3a4a"
                        BorderThickness="1"
                        Padding="12"
                        HorizontalAlignment="Center">
                  <Image Source="{Binding LargeSprite}"
                         Width="80" Height="80"
                         RenderOptions.BitmapInterpolationMode="None"/>
                </Border>

                <TextBlock Text="Scale: 5x"
                           FontSize="10"
                           Foreground="#888888"
                           HorizontalAlignment="Center"/>
              </StackPanel>
            </Border>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>

    </StackPanel>
  </ScrollViewer>
</UserControl>
```

**Create:** `Views/SpriteDemoView.axaml.cs`

```csharp
using Avalonia.Controls;

namespace RuneAndRust.DesktopUI.Views;

public partial class SpriteDemoView : UserControl
{
    public SpriteDemoView()
    {
        InitializeComponent();
    }
}
```

---

### Task 10: Wire Up Demo View (1 hour)

**Update:** `ServiceConfiguration.cs` - Add demo ViewModel

```csharp
// In ConfigureServices method, add:
services.AddTransient<SpriteDemoViewModel>();
```

**Update:** `MainWindowViewModel.cs` - Navigate to demo on startup

```csharp
using RuneAndRust.DesktopUI.Services;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SpriteService _spriteService;

    public MainWindowViewModel(SpriteService spriteService)
    {
        _spriteService = spriteService;

        // Initialize commands
        NavigateToCombatCommand = ReactiveCommand.Create(NavigateToCombat);
        NavigateToCharacterCommand = ReactiveCommand.Create(NavigateToCharacter);
        NavigateToDungeonCommand = ReactiveCommand.Create(NavigateToDungeon);

        // Show sprite demo by default
        ShowSpriteDemo();
    }

    private void ShowSpriteDemo()
    {
        var demoVM = Program.Services.GetRequiredService<SpriteDemoViewModel>();
        CurrentView = demoVM;
        StatusMessage = "Displaying sprite demo";
    }

    // ... rest of class
}
```

---

## Testing Checklist

At the end of Phase 1, verify:

### Build & Run Tests
- [ ] Project builds without errors
- [ ] Application launches successfully
- [ ] No console errors or warnings
- [ ] Window displays with correct title "AETHELGARD - Rune & Rust"

### Visual Tests
- [ ] Title bar displays "AETHELGARD" in gold with shadow effect
- [ ] Navigation menu visible on left side
- [ ] Status bar shows messages at bottom
- [ ] Background gradient renders correctly

### Sprite Tests
- [ ] Sprite demo view displays
- [ ] At least 2 sprites render (Shieldmaiden, Berserker)
- [ ] Sprites display at 3x scale (48×48px)
- [ ] Sprites display at 5x scale (80×80px)
- [ ] Colors match prototype exactly
- [ ] No anti-aliasing (crisp pixel edges)

### Navigation Tests
- [ ] Clicking navigation buttons updates status bar
- [ ] CurrentView updates when navigating
- [ ] No crashes when clicking navigation

### Dependency Injection Tests
- [ ] Services resolve correctly from DI container
- [ ] ViewModels receive dependencies
- [ ] Serilog logs to console and file
- [ ] Log file created in `logs/` directory

---

## Troubleshooting

### Common Issues

**Issue:** "Could not find Avalonia templates"
```bash
# Solution:
dotnet new uninstall Avalonia.Templates
dotnet new install Avalonia.Templates
```

**Issue:** "SkiaSharp not found"
```bash
# Solution: SkiaSharp comes with Avalonia, but if missing:
dotnet add package SkiaSharp --version 2.88.8
```

**Issue:** "Window doesn't display sprites"
- Check that `RenderOptions.BitmapInterpolationMode="None"` is set on Image elements
- Verify SpriteService is registered in DI
- Check logs for sprite loading errors

**Issue:** "Fonts look wrong"
- Ensure `FontFamily="Courier New"` is set
- Install Courier New font if missing on Linux

---

## Success Criteria

Phase 1 is complete when:

1. ✅ Application launches and displays main window
2. ✅ Title and branding visible and styled correctly
3. ✅ Navigation menu functional (buttons clickable)
4. ✅ Sprite demo view shows at least 2 rendered sprites
5. ✅ Sprites render pixel-perfect (no blurring)
6. ✅ Dependency injection working for all services
7. ✅ No errors in logs
8. ✅ Code builds with zero warnings

---

## Next Steps

Upon completing Phase 1:

1. **Commit work:**
   ```bash
   git add .
   git commit -m "feat: Phase 1 complete - Avalonia foundation and sprite rendering"
   git push origin claude/implement-game-gui-015u4BSLv1LyoZaZzVWbTVXq
   ```

2. **Create demo video/screenshots** showing:
   - Application launching
   - Sprite rendering
   - Navigation working

3. **Review with stakeholders** before proceeding to Phase 2

4. **Begin Phase 2:** Combat Grid MVP implementation

---

**End of Phase 1 Guide**
