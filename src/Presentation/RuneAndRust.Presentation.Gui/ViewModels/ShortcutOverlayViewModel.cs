namespace RuneAndRust.Presentation.Gui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RuneAndRust.Presentation.Gui.Services;
using System.Collections.ObjectModel;

/// <summary>
/// ViewModel for the shortcut overlay window.
/// </summary>
public partial class ShortcutOverlayViewModel : ViewModelBase
{
    private readonly Action? _closeAction;

    /// <summary>Global shortcuts.</summary>
    public ObservableCollection<ShortcutDisplayInfo> GlobalShortcuts { get; } = [];

    /// <summary>Game context shortcuts.</summary>
    public ObservableCollection<ShortcutDisplayInfo> GameShortcuts { get; } = [];

    /// <summary>Combat context shortcuts.</summary>
    public ObservableCollection<ShortcutDisplayInfo> CombatShortcuts { get; } = [];

    /// <summary>Whether there are global shortcuts.</summary>
    public bool HasGlobalShortcuts => GlobalShortcuts.Count > 0;

    /// <summary>Whether there are game shortcuts.</summary>
    public bool HasGameShortcuts => GameShortcuts.Count > 0;

    /// <summary>Whether there are combat shortcuts.</summary>
    public bool HasCombatShortcuts => CombatShortcuts.Count > 0;

    /// <summary>Creates with shortcut service.</summary>
    public ShortcutOverlayViewModel(IKeyboardShortcutService shortcutService, Action? closeAction = null)
    {
        _closeAction = closeAction;
        var shortcuts = shortcutService.GetAllShortcuts();

        foreach (var shortcut in shortcuts)
        {
            switch (shortcut.Context)
            {
                case ShortcutContext.Global:
                    GlobalShortcuts.Add(shortcut);
                    break;
                case ShortcutContext.Game:
                    GameShortcuts.Add(shortcut);
                    break;
                case ShortcutContext.Combat:
                    CombatShortcuts.Add(shortcut);
                    break;
            }
        }
    }

    /// <summary>Design-time constructor.</summary>
    public ShortcutOverlayViewModel()
    {
        // Sample data for design time
        GlobalShortcuts.Add(new ShortcutDisplayInfo("show-help", "Help / Shortcuts",
            new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F1), ShortcutContext.Global));
        GlobalShortcuts.Add(new ShortcutDisplayInfo("quick-save", "Quick Save",
            new Avalonia.Input.KeyGesture(Avalonia.Input.Key.F5), ShortcutContext.Global));
        GameShortcuts.Add(new ShortcutDisplayInfo("toggle-inventory", "Toggle Inventory",
            new Avalonia.Input.KeyGesture(Avalonia.Input.Key.I), ShortcutContext.Game));
        CombatShortcuts.Add(new ShortcutDisplayInfo("end-turn", "End Turn",
            new Avalonia.Input.KeyGesture(Avalonia.Input.Key.Space), ShortcutContext.Combat));
    }

    /// <summary>Closes the overlay.</summary>
    [RelayCommand]
    private void Close() => _closeAction?.Invoke();
}
