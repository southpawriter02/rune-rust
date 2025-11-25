using ReactiveUI;
using RuneAndRust.DesktopUI.Services;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RuneAndRust.DesktopUI.ViewModels;

/// <summary>
/// v0.43.19: View model for individual save file display.
/// Wraps SaveFileMetadata with display formatting.
/// </summary>
public class SaveFileViewModel : ViewModelBase
{
    private bool _isSelected;

    /// <summary>
    /// The underlying metadata.
    /// </summary>
    public SaveFileMetadata Metadata { get; }

    /// <summary>
    /// File identifier for load/delete operations.
    /// </summary>
    public string FileName => Metadata.FileName;

    /// <summary>
    /// Display name for the save.
    /// </summary>
    public string SaveName => Metadata.SaveName;

    /// <summary>
    /// Character name in the save.
    /// </summary>
    public string CharacterName => Metadata.CharacterName;

    /// <summary>
    /// Character class display.
    /// </summary>
    public string CharacterClass => Metadata.CharacterClass;

    /// <summary>
    /// Character specialization.
    /// </summary>
    public string Specialization => Metadata.Specialization;

    /// <summary>
    /// Current legend level.
    /// </summary>
    public int Legend => Metadata.Legend;

    /// <summary>
    /// Current floor/milestone.
    /// </summary>
    public int CurrentFloor => Metadata.CurrentFloor;

    /// <summary>
    /// Whether boss was defeated.
    /// </summary>
    public bool BossDefeated => Metadata.BossDefeated;

    /// <summary>
    /// Date/time of save.
    /// </summary>
    public DateTime SaveDate => Metadata.SaveDate;

    /// <summary>
    /// Total play time.
    /// </summary>
    public TimeSpan PlayTime => Metadata.PlayTime;

    /// <summary>
    /// Whether this is an auto-save.
    /// </summary>
    public bool IsAutoSave => Metadata.IsAutoSave;

    /// <summary>
    /// Whether this is a quick-save.
    /// </summary>
    public bool IsQuickSave => Metadata.IsQuickSave;

    /// <summary>
    /// Formatted date string.
    /// </summary>
    public string DisplayDate => SaveDate.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Formatted play time string.
    /// </summary>
    public string DisplayPlayTime => PlayTime.TotalHours > 0
        ? $"{(int)PlayTime.TotalHours}h {PlayTime.Minutes}m"
        : $"{PlayTime.Minutes}m";

    /// <summary>
    /// Progress summary string.
    /// </summary>
    public string DisplayProgress => BossDefeated
        ? $"Floor {CurrentFloor} - Legend {Legend} (Boss Defeated)"
        : $"Floor {CurrentFloor} - Legend {Legend}";

    /// <summary>
    /// Character summary string.
    /// </summary>
    public string CharacterSummary => !string.IsNullOrEmpty(Specialization) && Specialization != "None"
        ? $"{CharacterName} - {CharacterClass} ({Specialization})"
        : $"{CharacterName} - {CharacterClass}";

    /// <summary>
    /// Whether this save is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    /// <summary>
    /// Creates a new SaveFileViewModel from metadata.
    /// </summary>
    public SaveFileViewModel(SaveFileMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }
}

/// <summary>
/// v0.43.19: View model for the save/load screen.
/// Manages save file listing, saving, loading, and deletion.
/// </summary>
public class SaveLoadViewModel : ViewModelBase
{
    private readonly ISaveGameService _saveService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService? _navigationService;

    private SaveFileViewModel? _selectedSave;
    private string _newSaveName = string.Empty;
    private bool _isLoading;
    private bool _isSaving;
    private string _statusMessage = string.Empty;
    private bool _showAutoSaveIndicator;

    /// <summary>
    /// Collection of available save files.
    /// </summary>
    public ObservableCollection<SaveFileViewModel> SaveFiles { get; } = new();

    /// <summary>
    /// Currently selected save file.
    /// </summary>
    public SaveFileViewModel? SelectedSave
    {
        get => _selectedSave;
        set
        {
            // Deselect previous
            if (_selectedSave != null)
            {
                _selectedSave.IsSelected = false;
            }

            this.RaiseAndSetIfChanged(ref _selectedSave, value);

            // Select new
            if (_selectedSave != null)
            {
                _selectedSave.IsSelected = true;
            }

            this.RaisePropertyChanged(nameof(CanLoad));
            this.RaisePropertyChanged(nameof(CanDelete));
            this.RaisePropertyChanged(nameof(HasSelectedSave));
        }
    }

    /// <summary>
    /// Name for new save file.
    /// </summary>
    public string NewSaveName
    {
        get => _newSaveName;
        set
        {
            this.RaiseAndSetIfChanged(ref _newSaveName, value);
            this.RaisePropertyChanged(nameof(CanSave));
        }
    }

    /// <summary>
    /// Whether a load operation is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    /// <summary>
    /// Whether a save operation is in progress.
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        private set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    /// <summary>
    /// Status message to display.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    /// <summary>
    /// Whether to show the auto-save indicator.
    /// </summary>
    public bool ShowAutoSaveIndicator
    {
        get => _showAutoSaveIndicator;
        private set => this.RaiseAndSetIfChanged(ref _showAutoSaveIndicator, value);
    }

    /// <summary>
    /// Whether a new save can be created.
    /// </summary>
    public bool CanSave => !string.IsNullOrWhiteSpace(NewSaveName) && !IsSaving;

    /// <summary>
    /// Whether a save can be loaded.
    /// </summary>
    public bool CanLoad => SelectedSave != null && !IsLoading;

    /// <summary>
    /// Whether a save can be deleted.
    /// </summary>
    public bool CanDelete => SelectedSave != null && !IsLoading && !IsSaving;

    /// <summary>
    /// Whether a save is selected.
    /// </summary>
    public bool HasSelectedSave => SelectedSave != null;

    /// <summary>
    /// Whether quick save is available.
    /// </summary>
    public bool HasQuickSave => _saveService?.HasQuickSave ?? false;

    /// <summary>
    /// Number of save files.
    /// </summary>
    public int SaveCount => SaveFiles.Count;

    /// <summary>
    /// Command to save the game.
    /// </summary>
    public ICommand SaveGameCommand { get; }

    /// <summary>
    /// Command to load a save.
    /// </summary>
    public ICommand LoadGameCommand { get; }

    /// <summary>
    /// Command to delete a save.
    /// </summary>
    public ICommand DeleteSaveCommand { get; }

    /// <summary>
    /// Command to refresh the save list.
    /// </summary>
    public ICommand RefreshSavesCommand { get; }

    /// <summary>
    /// Command to perform quick save.
    /// </summary>
    public ICommand QuickSaveCommand { get; }

    /// <summary>
    /// Command to perform quick load.
    /// </summary>
    public ICommand QuickLoadCommand { get; }

    /// <summary>
    /// Command to go back to previous view.
    /// </summary>
    public ICommand BackCommand { get; }

    /// <summary>
    /// Creates a new instance for design-time support.
    /// </summary>
    public SaveLoadViewModel()
    {
        _saveService = null!;
        _dialogService = null!;

        SaveGameCommand = ReactiveCommand.Create(() => { });
        LoadGameCommand = ReactiveCommand.Create(() => { });
        DeleteSaveCommand = ReactiveCommand.Create(() => { });
        RefreshSavesCommand = ReactiveCommand.Create(() => { });
        QuickSaveCommand = ReactiveCommand.Create(() => { });
        QuickLoadCommand = ReactiveCommand.Create(() => { });
        BackCommand = ReactiveCommand.Create(() => { });

        LoadDesignTimeData();
    }

    /// <summary>
    /// Creates a new instance with dependency injection.
    /// </summary>
    public SaveLoadViewModel(
        ISaveGameService saveService,
        IDialogService dialogService,
        INavigationService? navigationService = null)
    {
        _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService;

        // Subscribe to save service events
        _saveService.AutoSaveStarted += OnAutoSaveStarted;
        _saveService.AutoSaveCompleted += OnAutoSaveCompleted;
        _saveService.SaveCompleted += OnSaveCompleted;

        // Create reactive commands
        var canSave = this.WhenAnyValue(x => x.CanSave);
        var canLoad = this.WhenAnyValue(x => x.CanLoad);
        var canDelete = this.WhenAnyValue(x => x.CanDelete);

        SaveGameCommand = ReactiveCommand.CreateFromTask(SaveGameAsync, canSave);
        LoadGameCommand = ReactiveCommand.CreateFromTask(LoadGameAsync, canLoad);
        DeleteSaveCommand = ReactiveCommand.CreateFromTask(DeleteSaveAsync, canDelete);
        RefreshSavesCommand = ReactiveCommand.Create(LoadSaveFiles);
        QuickSaveCommand = ReactiveCommand.CreateFromTask(QuickSaveAsync);
        QuickLoadCommand = ReactiveCommand.CreateFromTask(QuickLoadAsync);
        BackCommand = ReactiveCommand.Create(GoBack);

        // Initial load
        LoadSaveFiles();

        Console.WriteLine("[SAVELOAD] SaveLoadViewModel initialized");
    }

    /// <summary>
    /// Loads all save files from the service.
    /// </summary>
    public void LoadSaveFiles()
    {
        try
        {
            SaveFiles.Clear();

            var saves = _saveService.GetAllSaveFiles();
            foreach (var save in saves)
            {
                SaveFiles.Add(new SaveFileViewModel(save));
            }

            this.RaisePropertyChanged(nameof(SaveCount));
            this.RaisePropertyChanged(nameof(HasQuickSave));

            StatusMessage = $"Loaded {SaveFiles.Count} save file(s)";
            Console.WriteLine($"[SAVELOAD] Loaded {SaveFiles.Count} save files");
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load save files";
            Console.WriteLine($"[SAVELOAD] Error loading saves: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the current game with the specified name.
    /// </summary>
    private async Task SaveGameAsync()
    {
        if (!CanSave) return;

        try
        {
            IsSaving = true;
            StatusMessage = "Saving...";

            await _saveService.SaveGameAsync(NewSaveName);

            await _dialogService.ShowMessageAsync("Success", "Game saved successfully.");

            LoadSaveFiles();
            NewSaveName = string.Empty;
            StatusMessage = "Save complete";
        }
        catch (Exception ex)
        {
            StatusMessage = "Save failed";
            await _dialogService.ShowMessageAsync("Error", $"Failed to save game: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Loads the selected save file.
    /// </summary>
    private async Task LoadGameAsync()
    {
        if (SelectedSave == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Loading {SelectedSave.SaveName}...";

            var success = await _saveService.LoadGameAsync(SelectedSave.FileName);

            if (success)
            {
                StatusMessage = "Load complete";
                // In full implementation, navigate to game view
                Console.WriteLine($"[SAVELOAD] Loaded save: {SelectedSave.SaveName}");

                // Navigate back or to game
                _navigationService?.NavigateBack();
            }
            else
            {
                StatusMessage = "Load failed";
                await _dialogService.ShowMessageAsync("Error", "Failed to load the save file. It may be corrupted.");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Load failed";
            await _dialogService.ShowMessageAsync("Error", $"Failed to load game: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes the selected save file after confirmation.
    /// </summary>
    private async Task DeleteSaveAsync()
    {
        if (SelectedSave == null) return;

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Delete Save",
            $"Are you sure you want to delete '{SelectedSave.SaveName}'?\n\nThis action cannot be undone.");

        if (confirm)
        {
            try
            {
                var saveName = SelectedSave.SaveName;
                _saveService.DeleteSave(SelectedSave.FileName);

                SelectedSave = null;
                LoadSaveFiles();

                StatusMessage = $"Deleted '{saveName}'";
                Console.WriteLine($"[SAVELOAD] Deleted save: {saveName}");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync("Error", $"Failed to delete save: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Performs a quick save.
    /// </summary>
    public async Task QuickSaveAsync()
    {
        try
        {
            IsSaving = true;
            StatusMessage = "Quick saving...";

            await _saveService.QuickSaveAsync();

            StatusMessage = "Quick save complete";
            this.RaisePropertyChanged(nameof(HasQuickSave));
            Console.WriteLine("[SAVELOAD] Quick save completed");
        }
        catch (Exception ex)
        {
            StatusMessage = "Quick save failed";
            Console.WriteLine($"[SAVELOAD] Quick save failed: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Performs a quick load.
    /// </summary>
    public async Task QuickLoadAsync()
    {
        if (!HasQuickSave)
        {
            StatusMessage = "No quick save available";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Quick loading...";

            var success = await _saveService.QuickLoadAsync();

            if (success)
            {
                StatusMessage = "Quick load complete";
                Console.WriteLine("[SAVELOAD] Quick load completed");
                _navigationService?.NavigateBack();
            }
            else
            {
                StatusMessage = "Quick load failed";
                await _dialogService.ShowMessageAsync("Error", "Failed to quick load. No quick save found.");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Quick load failed";
            Console.WriteLine($"[SAVELOAD] Quick load failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates back to the previous view.
    /// </summary>
    private void GoBack()
    {
        _navigationService?.NavigateBack();
    }

    /// <summary>
    /// Selects a save file by view model.
    /// </summary>
    public void SelectSave(SaveFileViewModel? save)
    {
        SelectedSave = save;
    }

    private void OnAutoSaveStarted(object? sender, EventArgs e)
    {
        ShowAutoSaveIndicator = true;
        StatusMessage = "Auto-saving...";
    }

    private void OnAutoSaveCompleted(object? sender, EventArgs e)
    {
        ShowAutoSaveIndicator = false;
        StatusMessage = "Auto-save complete";

        // Hide indicator after a delay
        Task.Delay(2000).ContinueWith(_ =>
        {
            if (StatusMessage == "Auto-save complete")
            {
                StatusMessage = string.Empty;
            }
        });
    }

    private void OnSaveCompleted(object? sender, SaveFileMetadata e)
    {
        // Refresh list when a save completes
        LoadSaveFiles();
    }

    /// <summary>
    /// Loads design-time sample data.
    /// </summary>
    private void LoadDesignTimeData()
    {
        SaveFiles.Add(new SaveFileViewModel(new SaveFileMetadata
        {
            FileName = "hero1",
            SaveName = "Thorin's Journey",
            CharacterName = "Thorin",
            CharacterClass = "Shieldmaiden",
            Specialization = "Bulwark",
            Legend = 5,
            CurrentFloor = 3,
            SaveDate = DateTime.Now.AddHours(-2),
            PlayTime = TimeSpan.FromHours(4.5),
            IsAutoSave = false,
            IsQuickSave = false
        }));

        SaveFiles.Add(new SaveFileViewModel(new SaveFileMetadata
        {
            FileName = "quicksave_1",
            SaveName = "Quick Save",
            CharacterName = "Thorin",
            CharacterClass = "Shieldmaiden",
            Legend = 5,
            CurrentFloor = 3,
            SaveDate = DateTime.Now.AddMinutes(-30),
            PlayTime = TimeSpan.FromHours(4.5),
            IsAutoSave = false,
            IsQuickSave = true
        }));

        SaveFiles.Add(new SaveFileViewModel(new SaveFileMetadata
        {
            FileName = "autosave_0",
            SaveName = "Auto Save",
            CharacterName = "Ragnar",
            CharacterClass = "Berserker",
            Legend = 3,
            CurrentFloor = 2,
            SaveDate = DateTime.Now.AddDays(-1),
            PlayTime = TimeSpan.FromHours(2),
            IsAutoSave = true,
            IsQuickSave = false
        }));
    }

    /// <summary>
    /// Cleans up event subscriptions.
    /// </summary>
    public override void Dispose()
    {
        if (_saveService != null)
        {
            _saveService.AutoSaveStarted -= OnAutoSaveStarted;
            _saveService.AutoSaveCompleted -= OnAutoSaveCompleted;
            _saveService.SaveCompleted -= OnSaveCompleted;
        }

        base.Dispose();
    }
}
