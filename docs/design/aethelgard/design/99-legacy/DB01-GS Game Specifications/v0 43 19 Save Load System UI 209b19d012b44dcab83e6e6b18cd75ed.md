# v0.43.19: Save/Load System UI

Type: UI
Description: Save/load UI: save game dialog, load game menu with save list, save file metadata display, delete save confirmation, quick save/load (F5/F9), auto-save indicator. 4-6 hours.
Priority: Must-Have
Status: In Design
Target Version: Alpha
Dependencies: v0.43.1-v0.43.3, v0.3 (Persistence)
Implementation Difficulty: Easy
Balance Validated: No
Parent item: v0.43: Avalonia Desktop UI Implementation (v0%2043%20Avalonia%20Desktop%20UI%20Implementation%20331e1dc7608c4e9f8a029a22c1124c22.md)
Proof-of-Concept Flag: No
Template Validated: No
Voice Validated: No

**Status:** Not Started

**Prerequisites:** v0.43.1-v0.43.3, v0.3 (Persistence)

**Estimated Time:** 4-6 hours

**Group:** Systems & Polish

**Deliverable:** Save/load interface with file management

---

## Executive Summary

v0.43.19 implements the save/load UI, allowing players to save progress, load existing saves, manage multiple save files, and view save metadata.

**What This Delivers:**

- Save game dialog
- Load game menu with save list
- Save file metadata display (timestamp, progress, character)
- Delete save confirmation
- Quick save/load functionality
- Auto-save indicator
- Integration with v0.3 persistence

**Success Metric:** Can save/load games with clear file management interface.

---

## Service Implementation

### SaveLoadViewModel

```csharp
using ReactiveUI;
using RuneAndRust.Persistence;
using System.Collections.ObjectModel;
using [System.Windows](http://System.Windows).Input;

namespace RuneAndRust.DesktopUI.ViewModels;

public class SaveLoadViewModel : ViewModelBase
{
    private readonly ISaveGameService _saveService;
    private readonly IDialogService _dialogService;
    private SaveFileViewModel? _selectedSave;
    private string _newSaveName = "";
    
    public ObservableCollection<SaveFileViewModel> SaveFiles { get; } = new();
    
    public SaveFileViewModel? SelectedSave
    {
        get => _selectedSave;
        set => this.RaiseAndSetIfChanged(ref _selectedSave, value);
    }
    
    public string NewSaveName
    {
        get => _newSaveName;
        set => this.RaiseAndSetIfChanged(ref _newSaveName, value);
    }
    
    public bool CanSave => !string.IsNullOrWhiteSpace(NewSaveName);
    public bool CanLoad => SelectedSave != null;
    public bool CanDelete => SelectedSave != null;
    
    public ICommand SaveGameCommand { get; }
    public ICommand LoadGameCommand { get; }
    public ICommand DeleteSaveCommand { get; }
    public ICommand RefreshSavesCommand { get; }
    
    public SaveLoadViewModel(
        ISaveGameService saveService,
        IDialogService dialogService)
    {
        _saveService = saveService;
        _dialogService = dialogService;
        
        SaveGameCommand = ReactiveCommand.CreateFromTask(SaveGameAsync,
            this.WhenAnyValue(x => x.CanSave));
        LoadGameCommand = ReactiveCommand.CreateFromTask(LoadGameAsync,
            this.WhenAnyValue(x => x.CanLoad));
        DeleteSaveCommand = ReactiveCommand.CreateFromTask(DeleteSaveAsync,
            this.WhenAnyValue(x => x.CanDelete));
        RefreshSavesCommand = ReactiveCommand.Create(LoadSaveFiles);
        
        LoadSaveFiles();
    }
    
    public void LoadSaveFiles()
    {
        SaveFiles.Clear();
        
        var saves = _saveService.GetAllSaveFiles();
        foreach (var save in saves)
        {
            SaveFiles.Add(new SaveFileViewModel(save));
        }
    }
    
    private async Task SaveGameAsync()
    {
        try
        {
            await _saveService.SaveGameAsync(NewSaveName);
            await _dialogService.ShowMessageAsync("Success", "Game saved successfully.");
            
            LoadSaveFiles();
            NewSaveName = "";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync("Error", $"Failed to save game: {ex.Message}");
        }
    }
    
    private async Task LoadGameAsync()
    {
        if (SelectedSave == null) return;
        
        try
        {
            await _saveService.LoadGameAsync(SelectedSave.FileName);
            // Navigate to game
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync("Error", $"Failed to load game: {ex.Message}");
        }
    }
    
    private async Task DeleteSaveAsync()
    {
        if (SelectedSave == null) return;
        
        var confirm = await _dialogService.ShowConfirmationAsync(
            "Delete Save",
            $"Are you sure you want to delete '{SelectedSave.SaveName}'? This cannot be undone.");
        
        if (confirm)
        {
            try
            {
                _saveService.DeleteSave(SelectedSave.FileName);
                LoadSaveFiles();
                SelectedSave = null;
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Error", $"Failed to delete save: {ex.Message}");
            }
        }
    }
    
    public async Task QuickSaveAsync()
    {
        try
        {
            await _saveService.QuickSaveAsync();
            // Show brief notification
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync("Error", $"Quick save failed: {ex.Message}");
        }
    }
    
    public async Task QuickLoadAsync()
    {
        try
        {
            await _saveService.QuickLoadAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync("Error", $"Quick load failed: {ex.Message}");
        }
    }
}

public class SaveFileViewModel
{
    public SaveFileMetadata Metadata { get; }
    
    public string FileName => Metadata.FileName;
    public string SaveName => Metadata.SaveName;
    public DateTime SaveDate => Metadata.SaveDate;
    public string CharacterName => Metadata.CharacterName;
    public int Legend => Metadata.Legend;
    public int CurrentFloor => Metadata.CurrentFloor;
    public TimeSpan PlayTime => Metadata.PlayTime;
    public bool IsAutoSave => Metadata.IsAutoSave;
    
    public string DisplayDate => SaveDate.ToString("yyyy-MM-dd HH:mm:ss");
    public string DisplayPlayTime => $"{(int)PlayTime.TotalHours}h {PlayTime.Minutes}m";
    public string DisplayProgress => $"Floor {CurrentFloor} - Legend {Legend}";
    
    public SaveFileViewModel(SaveFileMetadata metadata)
    {
        Metadata = metadata;
    }
}
```

---

## SaveLoadView XAML

```xml
<UserControl xmlns="[https://github.com/avaloniaui](https://github.com/avaloniaui)"
             xmlns:x="[http://schemas.microsoft.com/winfx/2006/xaml](http://schemas.microsoft.com/winfx/2006/xaml)"
             xmlns:vm="using:RuneAndRust.DesktopUI.ViewModels"
             x:Class="RuneAndRust.DesktopUI.Views.SaveLoadView"
             x:DataType="vm:SaveLoadViewModel">
    
    <Grid ColumnDefinitions="*,400" Margin="20">
        <!-- Save Files List -->
        <Border Grid.Column="0" 
                Background="#2C2C2C" 
                Padding="20" 
                Margin="0,0,10,0"
                CornerRadius="5">
            <StackPanel>
                <Grid ColumnDefinitions="*,Auto" Margin="0,0,0,15">
                    <TextBlock Grid.Column="0"
                               Text="Save Files"
                               FontSize="20"
                               FontWeight="Bold"
                               Foreground="White"/>
                    
                    <Button Grid.Column="1"
                            Content="🔄 Refresh"
                            Command="{Binding RefreshSavesCommand}"/>
                </Grid>
                
                <ScrollViewer Height="600">
                    <ItemsControl ItemsSource="{Binding SaveFiles}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Background="{Binding IsSelected, Converter={StaticResource SelectedBackgroundConverter}}"
                                        BorderBrush="#4A4A4A"
                                        BorderThickness="2"
                                        Padding="15"
                                        Margin="0,5"
                                        CornerRadius="5"
                                        Cursor="Hand"
                                        Tapped="OnSaveFileTapped">
                                    <Grid ColumnDefinitions="Auto,*">
                                        <!-- Icon -->
                                        <TextBlock Grid.Column="0"
                                                   Text="{Binding IsAutoSave, Converter={StaticResource SaveIconConverter}}"
                                                   FontSize="32"
                                                   VerticalAlignment="Center"
                                                   Margin="0,0,15,0"/>
                                        
                                        <!-- Info -->
                                        <StackPanel Grid.Column="1">
                                            <TextBlock Text="{Binding SaveName}"
                                                       FontWeight="Bold"
                                                       FontSize="16"
                                                       Foreground="White"/>
                                            
                                            <TextBlock FontSize="12" Foreground="#CCCCCC">
                                                <Run Text="{Binding CharacterName}"/>
                                                <Run Text=" - "/>
                                                <Run Text="{Binding DisplayProgress}"/>
                                            </TextBlock>
                                            
                                            <Grid ColumnDefinitions="*,Auto" Margin="0,5,0,0">
                                                <TextBlock Grid.Column="0"
                                                           Text="{Binding DisplayDate}"
                                                           FontSize="11"
                                                           Foreground="#888888"/>
                                                
                                                <TextBlock Grid.Column="1"
                                                           Text="{Binding DisplayPlayTime}"
                                                           FontSize="11"
                                                           Foreground="#4A90E2"/>
                                            </Grid>
                                        </StackPanel>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </StackPanel>
        </Border>
        
        <!-- Actions Sidebar -->
        <Border Grid.Column="1" 
                Background="#2C2C2C" 
                Padding="20"
                CornerRadius="5">
            <StackPanel Spacing="20">
                <!-- Save Game -->
                <Border Background="#1C1C1C" Padding="15" CornerRadius="5">
                    <StackPanel Spacing="10">
                        <TextBlock Text="Save Game"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <TextBox Text="{Binding NewSaveName}"
                                 Watermark="Enter save name..."
                                 Height="40"/>
                        
                        <Button Content="💾 Save"
                                Command="{Binding SaveGameCommand}"
                                Height="50"
                                HorizontalAlignment="Stretch"/>
                    </StackPanel>
                </Border>
                
                <!-- Load Game -->
                <Border Background="#1C1C1C" Padding="15" CornerRadius="5">
                    <StackPanel Spacing="10">
                        <TextBlock Text="Load Game"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="White"/>
                        
                        <TextBlock Text="Select a save file from the list"
                                   FontSize="12"
                                   Foreground="#CCCCCC"
                                   TextWrapping="Wrap"/>
                        
                        <Button Content="📂 Load"
                                Command="{Binding LoadGameCommand}"
                                Height="50"
                                HorizontalAlignment="Stretch"/>
                    </StackPanel>
                </Border>
                
                <!-- Delete Save -->
                <Border Background="#1C1C1C" Padding="15" CornerRadius="5">
                    <StackPanel Spacing="10">
                        <TextBlock Text="Delete Save"
                                   FontSize="18"
                                   FontWeight="Bold"
                                   Foreground="#DC143C"/>
                        
                        <TextBlock Text="⚠️ This action cannot be undone"
                                   FontSize="11"
                                   Foreground="#888888"/>
                        
                        <Button Content="🗑️ Delete"
                                Command="{Binding DeleteSaveCommand}"
                                Height="50"
                                HorizontalAlignment="Stretch"
                                Background="#8B0000"/>
                    </StackPanel>
                </Border>
                
                <Separator Background="#3C3C3C" Margin="0,10"/>
                
                <!-- Quick Actions -->
                <StackPanel Spacing="10">
                    <TextBlock Text="Quick Actions"
                               FontSize="14"
                               Foreground="#CCCCCC"/>
                    
                    <TextBlock Text="F5 - Quick Save"
                               FontSize="12"
                               Foreground="#888888"/>
                    
                    <TextBlock Text="F9 - Quick Load"
                               FontSize="12"
                               Foreground="#888888"/>
                </StackPanel>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

---

## Auto-Save Indicator Component

```xml
<!-- Overlay shown briefly during auto-save -->
<Border x:Name="AutoSaveIndicator"
        Background="#E0000000"
        Padding="15"
        CornerRadius="10"
        HorizontalAlignment="Right"
        VerticalAlignment="Bottom"
        Margin="20"
        IsVisible="False">
    <StackPanel Orientation="Horizontal" Spacing="10">
        <TextBlock Text="💾"
                   FontSize="20"/>
        <TextBlock Text="Auto-saving..."
                   FontSize="14"
                   Foreground="White"
                   VerticalAlignment="Center"/>
    </StackPanel>
</Border>
```

---

## Integration Points

**With v0.3 (Persistence):**

- Uses SaveGameService
- Displays save metadata
- Manages save files

**With v0.43.3 (Navigation):**

- F5/F9 keyboard shortcuts
- Quick save/load accessibility

---

## Success Criteria

**v0.43.19 is DONE when:**

### ✅ Save Functionality

- [ ]  Can create new saves
- [ ]  Custom save names work
- [ ]  Auto-save functional
- [ ]  Quick save (F5) works

### ✅ Load Functionality

- [ ]  Save list displays all saves
- [ ]  Can select and load saves
- [ ]  Metadata displayed correctly
- [ ]  Quick load (F9) works

### ✅ File Management

- [ ]  Can delete saves
- [ ]  Confirmation for deletion
- [ ]  Refresh updates list
- [ ]  Auto-save indicator shown

---

**Save/load complete. Ready for polish in v0.43.20.**