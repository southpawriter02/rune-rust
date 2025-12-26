# v0.43.18: Settings & Configuration

Type: UI
Description: Settings menu UI: graphics configuration (resolution, fullscreen, vsync), audio sliders (master, music, SFX), control remapping, gameplay options, accessibility settings, settings persistence. 4-6 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3
Implementation Difficulty: Easy
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3

**Estimated Time:** 4-6 hours

**Group:** Systems & Polish

**Deliverable:** Settings menu with all configuration options

---

## Executive Summary

v0.43.18 implements the settings menu, allowing players to configure graphics, audio, controls, gameplay options, and accessibility features.

**What This Delivers:**

- Settings menu UI
- Graphics configuration (resolution, fullscreen, vsync)
- Audio sliders (master, music, SFX)
- Control remapping interface
- Gameplay options (difficulty assists, auto-saves)
- Accessibility settings
- Settings persistence

**Success Metric:** All settings accessible and persistent across sessions.

---

## Service Implementation

### SettingsViewModel

```csharp
using ReactiveUI;
using [RuneAndRust.DesktopUI.Services](http://RuneAndRust.DesktopUI.Services);
using System.Collections.ObjectModel;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly IAudioService _audioService;
    
    // Graphics
    private bool _isFullscreen;
    private bool _vsyncEnabled = true;
    private int _selectedResolutionIndex;
    private int _targetFPS = 60;
    
    // Audio
    private float _masterVolume = 1.0f;
    private float _musicVolume = 0.8f;
    private float _sfxVolume = 1.0f;
    
    // Gameplay
    private bool _autoSaveEnabled = true;
    private int _autoSaveInterval = 5;
    private bool _showDamageNumbers = true;
    private bool _showHitConfirmation = true;
    private bool _pauseOnFocusLost = true;
    
    // Accessibility
    private bool _colorblindMode = false;
    private float _uiScale = 1.0f;
    private bool _reducedMotion = false;
    
    public ObservableCollection<string> AvailableResolutions { get; } = new();
    
    // Graphics Properties
    public bool IsFullscreen
    {
        get => _isFullscreen;
        set => this.RaiseAndSetIfChanged(ref _isFullscreen, value);
    }
    
    public bool VsyncEnabled
    {
        get => _vsyncEnabled;
        set => this.RaiseAndSetIfChanged(ref _vsyncEnabled, value);
    }
    
    public int SelectedResolutionIndex
    {
        get => _selectedResolutionIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedResolutionIndex, value);
    }
    
    public int TargetFPS
    {
        get => _targetFPS;
        set => this.RaiseAndSetIfChanged(ref _targetFPS, Math.Clamp(value, 30, 240));
    }
    
    // Audio Properties
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _masterVolume, Math.Clamp(value, 0f, 1f));
            _audioService.SetMasterVolume(_masterVolume);
        }
    }
    
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _musicVolume, Math.Clamp(value, 0f, 1f));
            _audioService.SetMusicVolume(_musicVolume);
        }
    }
    
    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            this.RaiseAndSetIfChanged(ref _sfxVolume, Math.Clamp(value, 0f, 1f));
            _audioService.SetSFXVolume(_sfxVolume);
        }
    }
    
    // Gameplay Properties
    public bool AutoSaveEnabled
    {
        get => _autoSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _autoSaveEnabled, value);
    }
    
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => this.RaiseAndSetIfChanged(ref _autoSaveInterval, Math.Clamp(value, 1, 30));
    }
    
    public bool ShowDamageNumbers
    {
        get => _showDamageNumbers;
        set => this.RaiseAndSetIfChanged(ref _showDamageNumbers, value);
    }
    
    // Accessibility Properties
    public float UIScale
    {
        get => _uiScale;
        set => this.RaiseAndSetIfChanged(ref _uiScale, Math.Clamp(value, 0.8f, 1.5f));
    }
    
    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand CancelCommand { get; }
    
    public SettingsViewModel(
        IConfigurationService configService,
        IAudioService audioService)
    {
        _configService = configService;
        _audioService = audioService;
        
        SaveSettingsCommand = ReactiveCommand.Create(SaveSettings);
        ResetToDefaultsCommand = ReactiveCommand.Create(ResetToDefaults);
        CancelCommand = ReactiveCommand.Create(Cancel);
        
        LoadSettings();
        LoadAvailableResolutions();
    }
    
    private void LoadSettings()
    {
        var config = _configService.LoadConfiguration();
        
        // Graphics
        IsFullscreen = [config.Graphics](http://config.Graphics).Fullscreen;
        VsyncEnabled = [config.Graphics](http://config.Graphics).VSync;
        TargetFPS = [config.Graphics](http://config.Graphics).TargetFPS;
        
        // Audio
        MasterVolume = [config.Audio](http://config.Audio).MasterVolume;
        MusicVolume = [config.Audio](http://config.Audio).MusicVolume;
        SFXVolume = [config.Audio](http://config.Audio).SFXVolume;
        
        // Gameplay
        AutoSaveEnabled = config.Gameplay.AutoSave;
        AutoSaveInterval = config.Gameplay.AutoSaveInterval;
        ShowDamageNumbers = config.Gameplay.ShowDamageNumbers;
        
        // Accessibility
        UIScale = config.Accessibility.UIScale;
    }
    
    private void SaveSettings()
    {
        var config = new GameConfiguration
        {
            Graphics = new GraphicsConfig
            {
                Fullscreen = IsFullscreen,
                VSync = VsyncEnabled,
                TargetFPS = TargetFPS
            },
            Audio = new AudioConfig
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SFXVolume = SFXVolume
            },
            Gameplay = new GameplayConfig
            {
                AutoSave = AutoSaveEnabled,
                AutoSaveInterval = AutoSaveInterval,
                ShowDamageNumbers = ShowDamageNumbers
            },
            Accessibility = new AccessibilityConfig
            {
                UIScale = UIScale
            }
        };
        
        _configService.SaveConfiguration(config);
    }
    
    private void ResetToDefaults()
    {
        _configService.ResetToDefaults();
        LoadSettings();
    }
    
    private void Cancel()
    {
        LoadSettings(); // Revert changes
    }
    
    private void LoadAvailableResolutions()
    {
        AvailableResolutions.Clear();
        AvailableResolutions.Add("1920x1080");
        AvailableResolutions.Add("2560x1440");
        AvailableResolutions.Add("3840x2160");
    }
}
```

---

## SettingsView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.SettingsView"
             x:DataType="vm:SettingsViewModel">
    
    <Grid RowDefinitions="*,Auto" Margin="20">
        <ScrollViewer Grid.Row="0">
            <StackPanel Spacing="20" MaxWidth="800">
                <!-- Graphics Settings -->
                <Border Background="#2C2C2C" Padding="20" CornerRadius="5">
                    <StackPanel Spacing="15">
                        <TextBlock Text="Graphics"
                                   FontSize="20"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <CheckBox Content="Fullscreen"
                                  IsChecked="{Binding IsFullscreen}"/>
                        
                        <CheckBox Content="V-Sync"
                                  IsChecked="{Binding VsyncEnabled}"/>
                        
                        <StackPanel>
                            <TextBlock Text="Target FPS"/>
                            <Slider Value="{Binding TargetFPS}"
                                    Minimum="30"
                                    Maximum="240"
                                    TickFrequency="30"/>
                            <TextBlock Text="{Binding TargetFPS}"
                                       Foreground="#4A90E2"
                                       HorizontalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Border>
                
                <!-- Audio Settings -->
                <Border Background="#2C2C2C" Padding="20" CornerRadius="5">
                    <StackPanel Spacing="15">
                        <TextBlock Text="Audio"
                                   FontSize="20"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <StackPanel>
                            <Grid ColumnDefinitions="*,Auto">
                                <TextBlock Grid.Column="0" Text="Master Volume"/>
                                <TextBlock Grid.Column="1" Text="{Binding MasterVolume, StringFormat='{}{0:P0}'}"/>
                            </Grid>
                            <Slider Value="{Binding MasterVolume}"
                                    Minimum="0"
                                    Maximum="1"
                                    TickFrequency="0.1"/>
                        </StackPanel>
                        
                        <StackPanel>
                            <Grid ColumnDefinitions="*,Auto">
                                <TextBlock Grid.Column="0" Text="Music Volume"/>
                                <TextBlock Grid.Column="1" Text="{Binding MusicVolume, StringFormat='{}{0:P0}'}"/>
                            </Grid>
                            <Slider Value="{Binding MusicVolume}"
                                    Minimum="0"
                                    Maximum="1"
                                    TickFrequency="0.1"/>
                        </StackPanel>
                        
                        <StackPanel>
                            <Grid ColumnDefinitions="*,Auto">
                                <TextBlock Grid.Column="0" Text="SFX Volume"/>
                                <TextBlock Grid.Column="1" Text="{Binding SFXVolume, StringFormat='{}{0:P0}'}"/>
                            </Grid>
                            <Slider Value="{Binding SFXVolume}"
                                    Minimum="0"
                                    Maximum="1"
                                    TickFrequency="0.1"/>
                        </StackPanel>
                    </StackPanel>
                </Border>
                
                <!-- Gameplay Settings -->
                <Border Background="#2C2C2C" Padding="20" CornerRadius="5">
                    <StackPanel Spacing="15">
                        <TextBlock Text="Gameplay"
                                   FontSize="20"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <CheckBox Content="Auto-Save Enabled"
                                  IsChecked="{Binding AutoSaveEnabled}"/>
                        
                        <StackPanel IsEnabled="{Binding AutoSaveEnabled}">
                            <TextBlock Text="Auto-Save Interval (minutes)"/>
                            <Slider Value="{Binding AutoSaveInterval}"
                                    Minimum="1"
                                    Maximum="30"/>
                            <TextBlock Text="{Binding AutoSaveInterval}"
                                       Foreground="#4A90E2"
                                       HorizontalAlignment="Center"/>
                        </StackPanel>
                        
                        <CheckBox Content="Show Damage Numbers"
                                  IsChecked="{Binding ShowDamageNumbers}"/>
                    </StackPanel>
                </Border>
                
                <!-- Accessibility Settings -->
                <Border Background="#2C2C2C" Padding="20" CornerRadius="5">
                    <StackPanel Spacing="15">
                        <TextBlock Text="Accessibility"
                                   FontSize="20"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <StackPanel>
                            <TextBlock Text="UI Scale"/>
                            <Slider Value="{Binding UIScale}"
                                    Minimum="0.8"
                                    Maximum="1.5"
                                    TickFrequency="0.1"/>
                            <TextBlock Text="{Binding UIScale, StringFormat='{}{0:P0}'}"
                                       Foreground="#4A90E2"
                                       HorizontalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Buttons -->
        <Grid Grid.Row="1" 
              ColumnDefinitions="Auto,*,Auto,Auto" 
              Margin="0,20,0,0">
            <Button Grid.Column="0"
                    Content="Reset to Defaults"
                    Command="{Binding ResetToDefaultsCommand}"
                    Width="150"/>
            
            <Button Grid.Column="2"
                    Content="Save"
                    Command="{Binding SaveSettingsCommand}"
                    Width="100"
                    Margin="0,0,10,0"/>
            
            <Button Grid.Column="3"
                    Content="Cancel"
                    Command="{Binding CancelCommand}"
                    Width="100"/>
        </Grid>
    </Grid>
</UserControl>
```

---

## Success Criteria

**v0.43.18 is DONE when:**

### ✅ Settings Categories

- [ ]  Graphics settings functional
- [ ]  Audio sliders working
- [ ]  Gameplay options available
- [ ]  Accessibility options present

### ✅ Persistence

- [ ]  Settings save on confirm
- [ ]  Settings load on startup
- [ ]  Cancel reverts changes
- [ ]  Reset to defaults works

---

**Settings complete. Ready for save/load in v0.43.19.**