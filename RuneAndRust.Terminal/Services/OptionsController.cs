using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Input;
using RuneAndRust.Core.Settings;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Controller for the Options screen modal loop (v0.3.15c - The Polyglot).
/// Handles input navigation, setting modification, key rebinding, and persistence on exit.
/// Uses localized strings via ILocalizationService.
/// </summary>
/// <remarks>
/// See: SPEC-LOC-001 for Localization System design.
/// v0.3.23a: Refactored to use IInputService for standardized input handling.
/// </remarks>
public class OptionsController
{
    private readonly IOptionsScreenRenderer _renderer;
    private readonly ISettingsService _settingsService;
    private readonly IInputConfigurationService _inputConfigService;
    private readonly IInputService _inputService;
    private readonly ILocalizationService _loc;
    private readonly OptionsViewHelperService _viewHelper;
    private readonly ILogger<OptionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsController"/> class.
    /// </summary>
    /// <param name="renderer">The options screen renderer.</param>
    /// <param name="settingsService">The settings service for persistence.</param>
    /// <param name="inputConfigService">The input configuration service for key rebinding.</param>
    /// <param name="inputService">The input service for standardized input handling.</param>
    /// <param name="localizationService">The localization service for string lookup.</param>
    /// <param name="viewHelper">The options view helper service.</param>
    /// <param name="logger">The logger for traceability.</param>
    public OptionsController(
        IOptionsScreenRenderer renderer,
        ISettingsService settingsService,
        IInputConfigurationService inputConfigService,
        IInputService inputService,
        ILocalizationService localizationService,
        OptionsViewHelperService viewHelper,
        ILogger<OptionsController> logger)
    {
        _renderer = renderer;
        _settingsService = settingsService;
        _inputConfigService = inputConfigService;
        _inputService = inputService;
        _loc = localizationService;
        _viewHelper = viewHelper;
        _logger = logger;
    }

    /// <summary>
    /// Runs the Options screen modal loop until the user exits.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogInformation("[Options] Options menu opened");

        // Initialize ViewModel from current GameSettings
        var vm = CreateViewModelFromSettings();
        RefreshItems(vm);

        while (true)
        {
            _renderer.Render(vm);

            var inputEvent = _inputService.ReadNext();

            // Handle input based on event type (v0.3.23a)
            switch (inputEvent)
            {
                // Navigation via GameAction
                case ActionEvent { Action: GameAction.MoveNorth }:
                    NavigateUp(vm);
                    break;

                case ActionEvent { Action: GameAction.MoveSouth }:
                    NavigateDown(vm);
                    break;

                case ActionEvent { Action: GameAction.MoveWest }:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, -1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case ActionEvent { Action: GameAction.MoveEast }:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, +1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case ActionEvent { Action: GameAction.Confirm }:
                    await HandleActionOrToggleAsync(vm);
                    RefreshItems(vm);
                    break;

                case ActionEvent { Action: GameAction.Cancel }:
                case ActionEvent { Action: GameAction.Menu }:
                    await _settingsService.SaveAsync();
                    _inputConfigService.SaveBindings();
                    _logger.LogInformation("[Options] Options menu closed. Settings and bindings saved.");
                    return;

                // Raw key fallback for vim-style navigation and Tab
                case RawKeyEvent { KeyInfo.Key: ConsoleKey.Tab } rawKey:
                    CycleTab(vm, rawKey.KeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift));
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.UpArrow or ConsoleKey.K }:
                    NavigateUp(vm);
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.DownArrow or ConsoleKey.J }:
                    NavigateDown(vm);
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.LeftArrow or ConsoleKey.H }:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, -1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.RightArrow or ConsoleKey.L }:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, +1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.Enter or ConsoleKey.Spacebar }:
                    await HandleActionOrToggleAsync(vm);
                    RefreshItems(vm);
                    break;

                case RawKeyEvent { KeyInfo.Key: ConsoleKey.Escape or ConsoleKey.Q }:
                    await _settingsService.SaveAsync();
                    _inputConfigService.SaveBindings();
                    _logger.LogInformation("[Options] Options menu closed. Settings and bindings saved.");
                    return;
            }
        }
    }

    /// <summary>
    /// Creates a ViewModel populated with current GameSettings values.
    /// </summary>
    private static OptionsViewModel CreateViewModelFromSettings()
    {
        return new OptionsViewModel
        {
            ActiveTab = OptionsTab.General,
            SelectedIndex = 0,
            ReduceMotion = GameSettings.ReduceMotion,
            Theme = (int)GameSettings.Theme,
            TextSpeed = GameSettings.TextSpeed,
            MasterVolume = GameSettings.MasterVolume,
            AmbienceEnabled = GameSettings.AmbienceEnabled,
            AutosaveIntervalMinutes = GameSettings.AutosaveIntervalMinutes,
            Language = GameSettings.Language
        };
    }

    /// <summary>
    /// Navigates up in the current tab's list.
    /// </summary>
    private void NavigateUp(OptionsViewModel vm)
    {
        if (vm.ActiveTab == OptionsTab.Controls)
        {
            vm.SelectedIndex = Math.Max(0, vm.SelectedIndex - 1);
        }
        else
        {
            vm.SelectedIndex = Math.Max(0, vm.SelectedIndex - 1);
        }
    }

    /// <summary>
    /// Navigates down in the current tab's list.
    /// </summary>
    private void NavigateDown(OptionsViewModel vm)
    {
        if (vm.ActiveTab == OptionsTab.Controls)
        {
            vm.SelectedIndex = Math.Min(vm.Bindings.Count - 1, vm.SelectedIndex + 1);
        }
        else
        {
            vm.SelectedIndex = Math.Min(vm.CurrentItems.Count - 1, vm.SelectedIndex + 1);
        }
    }

    /// <summary>
    /// Refreshes the CurrentItems list based on the active tab and current values.
    /// </summary>
    private void RefreshItems(OptionsViewModel vm)
    {
        if (vm.ActiveTab == OptionsTab.Controls)
        {
            RefreshBindings(vm);
            return;
        }

        vm.CurrentItems.Clear();
        var index = 0;

        switch (vm.ActiveTab)
        {
            case OptionsTab.General:
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_AutosaveInterval),
                    ValueDisplay: _viewHelper.FormatSliderValue(vm.AutosaveIntervalMinutes, "AutosaveIntervalMinutes"),
                    Type: SettingType.Slider,
                    IsSelected: index++ == vm.SelectedIndex,
                    MinValue: 1,
                    MaxValue: 60,
                    Step: 5,
                    PropertyName: "AutosaveIntervalMinutes"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_ResetToDefaults),
                    ValueDisplay: "",
                    Type: SettingType.Action,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "ResetToDefaults"
                ));
                break;

            case OptionsTab.Display:
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_Theme),
                    ValueDisplay: _viewHelper.GetThemeName(vm.Theme),
                    Type: SettingType.Enum,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "Theme"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_Language),
                    ValueDisplay: _viewHelper.GetLanguageDisplayName(vm.Language),
                    Type: SettingType.Enum,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "Language"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_ReduceMotion),
                    ValueDisplay: _viewHelper.FormatToggle(vm.ReduceMotion),
                    Type: SettingType.Toggle,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "ReduceMotion"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_TextSpeed),
                    ValueDisplay: _viewHelper.FormatSliderValue(vm.TextSpeed, "TextSpeed"),
                    Type: SettingType.Slider,
                    IsSelected: index++ == vm.SelectedIndex,
                    MinValue: 10,
                    MaxValue: 200,
                    Step: 10,
                    PropertyName: "TextSpeed"
                ));
                break;

            case OptionsTab.Audio:
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_MasterVolume),
                    ValueDisplay: _viewHelper.FormatSliderValue(vm.MasterVolume, "MasterVolume"),
                    Type: SettingType.Slider,
                    IsSelected: index++ == vm.SelectedIndex,
                    MinValue: 0,
                    MaxValue: 100,
                    Step: 5,
                    PropertyName: "MasterVolume"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: _loc.Get(LocKeys.UI_Options_Setting_AmbienceEnabled),
                    ValueDisplay: _viewHelper.FormatToggle(vm.AmbienceEnabled),
                    Type: SettingType.Toggle,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "AmbienceEnabled"
                ));
                break;
        }

        // Clamp selected index to valid range
        if (vm.CurrentItems.Count > 0)
        {
            vm.SelectedIndex = Math.Clamp(vm.SelectedIndex, 0, vm.CurrentItems.Count - 1);
        }
    }

    /// <summary>
    /// Refreshes the Bindings list for the Controls tab.
    /// </summary>
    private void RefreshBindings(OptionsViewModel vm)
    {
        vm.Bindings.Clear();

        // Define the commands to show (in order by category)
        var commands = new[]
        {
            "north", "south", "east", "west", "up", "down",
            "confirm", "cancel", "menu", "help",
            "inventory", "character", "journal", "bench",
            "interact", "look", "search", "wait",
            "attack", "light", "heavy"
        };

        for (int i = 0; i < commands.Length; i++)
        {
            var cmd = commands[i];
            var key = _inputConfigService.GetKeyForCommand(cmd);

            vm.Bindings.Add(new BindingItemView(
                ActionName: _viewHelper.GetCommandDisplayName(cmd),
                KeyDisplay: _viewHelper.FormatKeyName(key),
                Command: cmd,
                Category: _viewHelper.GetCommandCategory(cmd),
                IsSelected: i == vm.SelectedIndex,
                IsUnbound: !key.HasValue
            ));
        }

        // Clamp selected index to valid range
        if (vm.Bindings.Count > 0)
        {
            vm.SelectedIndex = Math.Clamp(vm.SelectedIndex, 0, vm.Bindings.Count - 1);
        }
    }

    /// <summary>
    /// Cycles to the next or previous tab.
    /// </summary>
    private void CycleTab(OptionsViewModel vm, bool reverse)
    {
        var tabs = Enum.GetValues<OptionsTab>();
        var currentIndex = Array.IndexOf(tabs, vm.ActiveTab);
        var direction = reverse ? -1 : 1;
        var newIndex = (currentIndex + direction + tabs.Length) % tabs.Length;

        vm.ActiveTab = tabs[newIndex];
        vm.SelectedIndex = 0;
        RefreshItems(vm);

        _logger.LogTrace("[Options] Switched to tab {Tab}", vm.ActiveTab);
    }

    /// <summary>
    /// Modifies the currently selected setting by the given direction.
    /// </summary>
    private void ModifySetting(OptionsViewModel vm, int direction)
    {
        if (vm.SelectedIndex < 0 || vm.SelectedIndex >= vm.CurrentItems.Count)
            return;

        var item = vm.CurrentItems[vm.SelectedIndex];

        switch (item.Type)
        {
            case SettingType.Slider when item.MinValue.HasValue && item.MaxValue.HasValue && item.Step.HasValue:
                ModifySliderValue(vm, item, direction);
                break;

            case SettingType.Enum:
                ModifyEnumValue(vm, item, direction);
                break;

            case SettingType.Toggle:
                // Left/Right has no effect on toggles
                break;
        }
    }

    /// <summary>
    /// Modifies a slider value by step in the given direction.
    /// </summary>
    private void ModifySliderValue(OptionsViewModel vm, SettingItemView item, int direction)
    {
        var step = item.Step!.Value * direction;

        switch (item.PropertyName)
        {
            case "AutosaveIntervalMinutes":
                vm.AutosaveIntervalMinutes = Math.Clamp(vm.AutosaveIntervalMinutes + step, item.MinValue!.Value, item.MaxValue!.Value);
                _logger.LogDebug("[Options] AutosaveIntervalMinutes changed to {Value}", vm.AutosaveIntervalMinutes);
                break;

            case "TextSpeed":
                vm.TextSpeed = Math.Clamp(vm.TextSpeed + step, item.MinValue!.Value, item.MaxValue!.Value);
                _logger.LogDebug("[Options] TextSpeed changed to {Value}", vm.TextSpeed);
                break;

            case "MasterVolume":
                vm.MasterVolume = Math.Clamp(vm.MasterVolume + step, item.MinValue!.Value, item.MaxValue!.Value);
                _logger.LogDebug("[Options] MasterVolume changed to {Value}", vm.MasterVolume);
                break;
        }
    }

    /// <summary>
    /// Cycles an enum value in the given direction.
    /// </summary>
    private void ModifyEnumValue(OptionsViewModel vm, SettingItemView item, int direction)
    {
        switch (item.PropertyName)
        {
            case "Theme":
                vm.Theme = _viewHelper.CycleTheme(vm.Theme, direction);
                _logger.LogDebug("[Options] Theme changed to {Value}", _viewHelper.GetThemeName(vm.Theme));
                break;

            case "Language":
                var availableLocales = _loc.GetAvailableLocales();
                vm.Language = _viewHelper.CycleLanguage(vm.Language, direction, availableLocales);
                // Reload locale immediately for live preview (v0.3.15c)
                _loc.LoadLocaleAsync(vm.Language).GetAwaiter().GetResult();
                _logger.LogDebug("[Options] Language changed to {Value}", _viewHelper.GetLanguageDisplayName(vm.Language));
                break;
        }
    }

    /// <summary>
    /// Handles action buttons, toggle settings, and key rebinding when Enter/Space is pressed.
    /// </summary>
    private async Task HandleActionOrToggleAsync(OptionsViewModel vm)
    {
        // Handle Controls tab separately (v0.3.10c)
        if (vm.ActiveTab == OptionsTab.Controls && vm.Bindings.Count > 0)
        {
            HandleRebind(vm);
            return;
        }

        if (vm.SelectedIndex < 0 || vm.SelectedIndex >= vm.CurrentItems.Count)
            return;

        var item = vm.CurrentItems[vm.SelectedIndex];

        switch (item.Type)
        {
            case SettingType.Toggle:
                ToggleSetting(vm, item);
                ApplyToGameSettings(vm);
                break;

            case SettingType.Action when item.PropertyName == "ResetToDefaults":
                await ResetToDefaultsAsync(vm);
                break;
        }
    }

    /// <summary>
    /// Handles key rebinding in listening mode.
    /// Captures the next key press and assigns it to the selected action.
    /// </summary>
    /// <remarks>v0.3.23a: Refactored to use IInputService with RawKeyEvent.</remarks>
    private void HandleRebind(OptionsViewModel vm)
    {
        var binding = vm.Bindings[vm.SelectedIndex];
        _logger.LogDebug("[Options] Listening for new key for {Command}", binding.Command);

        // Display listening prompt by updating the view temporarily
        var pressKeyPrompt = _loc.Get(LocKeys.UI_Options_PressKeyFor, binding.ActionName);
        var escCancelPrompt = _loc.Get(LocKeys.UI_Options_PressEscCancel);

        vm.CurrentItems.Clear();
        vm.CurrentItems.Add(new SettingItemView(
            Name: pressKeyPrompt,
            ValueDisplay: $"[yellow]{escCancelPrompt}[/]",
            Type: SettingType.Action,
            IsSelected: true
        ));

        // Re-render with the listening prompt
        // Temporarily show a simplified view
        var tempVm = new OptionsViewModel
        {
            ActiveTab = vm.ActiveTab,
            SelectedIndex = 0,
            CurrentItems = new List<SettingItemView>
            {
                new(
                    Name: pressKeyPrompt,
                    ValueDisplay: $"[yellow]{escCancelPrompt}[/]",
                    Type: SettingType.Action,
                    IsSelected: true
                )
            }
        };
        _renderer.Render(tempVm);

        // Wait for key press - use ReadNext and extract raw key info
        var inputEvent = _inputService.ReadNext();

        // Extract the key info from the event
        ConsoleKeyInfo keyInfo;
        switch (inputEvent)
        {
            case RawKeyEvent rawKey:
                keyInfo = rawKey.KeyInfo;
                break;
            case ActionEvent actionEvent when actionEvent.SourceKey.HasValue:
                // Reconstruct minimal key info from ActionEvent
                keyInfo = new ConsoleKeyInfo('\0', actionEvent.SourceKey.Value, false, false, false);
                break;
            case ActionEvent { Action: GameAction.Cancel }:
                // Cancel action means user wants to abort rebind
                _logger.LogDebug("[Options] Rebind cancelled for {Command}", binding.Command);
                RefreshBindings(vm);
                return;
            default:
                // For other events (SystemEvent, etc.), cancel the rebind
                _logger.LogDebug("[Options] Rebind cancelled for {Command} (non-key event)", binding.Command);
                RefreshBindings(vm);
                return;
        }

        if (keyInfo.Key == ConsoleKey.Escape)
        {
            _logger.LogDebug("[Options] Rebind cancelled for {Command}", binding.Command);
            RefreshBindings(vm);
            return;
        }

        // Check for conflict (is this key already bound to something else?)
        var existingCommand = _inputConfigService.GetCommandForKey(keyInfo.Key);
        if (existingCommand != null && !existingCommand.Equals(binding.Command, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[Options] Conflict: {Key} was bound to '{OldCommand}', reassigning to '{NewCommand}'",
                keyInfo.Key, existingCommand, binding.Command);
        }

        // Remove old key binding for this command (if any)
        var oldKey = _inputConfigService.GetKeyForCommand(binding.Command);
        if (oldKey.HasValue)
        {
            _inputConfigService.RemoveBinding(oldKey.Value);
        }

        // Set the new binding (this automatically overwrites any existing binding for this key)
        _inputConfigService.SetBinding(keyInfo.Key, binding.Command);
        _logger.LogInformation("[Options] Rebound '{Command}' to {Key}", binding.Command, keyInfo.Key);

        RefreshBindings(vm);
    }

    /// <summary>
    /// Toggles a boolean setting.
    /// </summary>
    private void ToggleSetting(OptionsViewModel vm, SettingItemView item)
    {
        switch (item.PropertyName)
        {
            case "ReduceMotion":
                vm.ReduceMotion = !vm.ReduceMotion;
                _logger.LogDebug("[Options] ReduceMotion changed to {Value}", vm.ReduceMotion);
                break;

            case "AmbienceEnabled":
                vm.AmbienceEnabled = !vm.AmbienceEnabled;
                _logger.LogDebug("[Options] AmbienceEnabled changed to {Value}", vm.AmbienceEnabled);
                break;
        }
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    private async Task ResetToDefaultsAsync(OptionsViewModel vm)
    {
        await _settingsService.ResetToDefaultsAsync();

        // Refresh ViewModel from GameSettings (which were just reset)
        vm.ReduceMotion = GameSettings.ReduceMotion;
        vm.Theme = (int)GameSettings.Theme;
        vm.TextSpeed = GameSettings.TextSpeed;
        vm.MasterVolume = GameSettings.MasterVolume;
        vm.AmbienceEnabled = GameSettings.AmbienceEnabled;
        vm.AutosaveIntervalMinutes = GameSettings.AutosaveIntervalMinutes;
        vm.Language = GameSettings.Language;

        // Reload locale to match reset language (v0.3.15c)
        await _loc.LoadLocaleAsync(vm.Language);

        _logger.LogInformation("[Options] Settings reset to defaults");
    }

    /// <summary>
    /// Applies the ViewModel values to the static GameSettings class.
    /// This enables live preview of settings changes.
    /// </summary>
    private static void ApplyToGameSettings(OptionsViewModel vm)
    {
        GameSettings.ReduceMotion = vm.ReduceMotion;
        GameSettings.Theme = (ThemeType)vm.Theme;
        GameSettings.TextSpeed = vm.TextSpeed;
        GameSettings.MasterVolume = vm.MasterVolume;
        GameSettings.AmbienceEnabled = vm.AmbienceEnabled;
        GameSettings.AutosaveIntervalMinutes = vm.AutosaveIntervalMinutes;
        GameSettings.Language = vm.Language;
    }
}
