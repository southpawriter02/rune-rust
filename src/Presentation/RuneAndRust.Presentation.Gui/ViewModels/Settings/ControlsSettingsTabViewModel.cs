namespace RuneAndRust.Presentation.Gui.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Models;
using Serilog;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the Controls settings tab.
/// </summary>
public partial class ControlsSettingsTabViewModel : ViewModelBase
{
    private readonly Action? _onChanged;

    [ObservableProperty] private ObservableCollection<KeybindingViewModel> _globalBindings = [];
    [ObservableProperty] private ObservableCollection<KeybindingViewModel> _gameBindings = [];
    [ObservableProperty] private bool _isRebinding;
    [ObservableProperty] private KeybindingViewModel? _rebindingTarget;

    /// <summary>Creates from settings with change callback.</summary>
    public ControlsSettingsTabViewModel(ControlsSettings settings, Action onChanged)
    {
        _onChanged = onChanged;
        foreach (var (action, key) in settings.GlobalBindings)
            GlobalBindings.Add(new KeybindingViewModel(action, key, "Global", StartRebind));
        foreach (var (action, key) in settings.GameBindings)
            GameBindings.Add(new KeybindingViewModel(action, key, "Game", StartRebind));
    }

    /// <summary>Design-time constructor.</summary>
    public ControlsSettingsTabViewModel()
    {
        GlobalBindings.Add(new KeybindingViewModel("Help / Shortcuts", "F1", "Global", null));
        GlobalBindings.Add(new KeybindingViewModel("Quick Save", "F5", "Global", null));
        GameBindings.Add(new KeybindingViewModel("Toggle Inventory", "I", "Game", null));
    }

    private void StartRebind(KeybindingViewModel binding)
    {
        IsRebinding = true;
        RebindingTarget = binding;
        Log.Debug("Started rebinding: {Action}", binding.ActionName);
    }

    /// <summary>Completes rebinding with new key.</summary>
    public void CompleteRebind(string newKey)
    {
        if (RebindingTarget != null)
        {
            RebindingTarget.CurrentKey = newKey;
            Log.Information("Rebound {Action} to {Key}", RebindingTarget.ActionName, newKey);
            _onChanged?.Invoke();
        }
        IsRebinding = false;
        RebindingTarget = null;
    }

    /// <summary>Resets all bindings to defaults.</summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        GlobalBindings.Clear();
        GlobalBindings.Add(new KeybindingViewModel("Help / Shortcuts", "F1", "Global", StartRebind));
        GlobalBindings.Add(new KeybindingViewModel("Quick Save", "F5", "Global", StartRebind));
        GlobalBindings.Add(new KeybindingViewModel("Quick Load", "F9", "Global", StartRebind));
        GlobalBindings.Add(new KeybindingViewModel("Close Window", "Escape", "Global", StartRebind));

        GameBindings.Clear();
        GameBindings.Add(new KeybindingViewModel("Toggle Inventory", "I", "Game", StartRebind));
        GameBindings.Add(new KeybindingViewModel("Toggle Map", "M", "Game", StartRebind));
        GameBindings.Add(new KeybindingViewModel("Toggle Quest Log", "J", "Game", StartRebind));
        GameBindings.Add(new KeybindingViewModel("Toggle Character", "C", "Game", StartRebind));

        Log.Information("Controls reset to defaults");
        _onChanged?.Invoke();
    }

    /// <summary>Converts to settings model.</summary>
    public ControlsSettings ToSettings() => new()
    {
        GlobalBindings = GlobalBindings.ToDictionary(b => b.ActionName, b => b.CurrentKey),
        GameBindings = GameBindings.ToDictionary(b => b.ActionName, b => b.CurrentKey)
    };
}

/// <summary>
/// ViewModel for a single keybinding.
/// </summary>
public partial class KeybindingViewModel : ObservableObject
{
    private readonly Action<KeybindingViewModel>? _onRebind;

    /// <summary>Gets the action name.</summary>
    public string ActionName { get; }

    /// <summary>Gets the category.</summary>
    public string Category { get; }

    /// <summary>Gets or sets the current key.</summary>
    [ObservableProperty] private string _currentKey;

    /// <summary>Creates a new keybinding ViewModel.</summary>
    public KeybindingViewModel(string action, string key, string category, Action<KeybindingViewModel>? onRebind)
    {
        ActionName = action;
        _currentKey = key;
        Category = category;
        _onRebind = onRebind;
    }

    /// <summary>Initiates rebinding.</summary>
    [RelayCommand]
    private void Rebind() => _onRebind?.Invoke(this);
}
