using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Settings;
using RuneAndRust.Core.ViewModels;
using RuneAndRust.Terminal.Rendering;

namespace RuneAndRust.Terminal.Services;

/// <summary>
/// Controller for the Options screen modal loop (v0.3.10b, extended v0.3.10c).
/// Handles input navigation, setting modification, key rebinding, and persistence on exit.
/// </summary>
public class OptionsController
{
    private readonly IOptionsScreenRenderer _renderer;
    private readonly ISettingsService _settingsService;
    private readonly IInputConfigurationService _inputConfigService;
    private readonly ILogger<OptionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsController"/> class.
    /// </summary>
    /// <param name="renderer">The options screen renderer.</param>
    /// <param name="settingsService">The settings service for persistence.</param>
    /// <param name="inputConfigService">The input configuration service for key rebinding (v0.3.10c).</param>
    /// <param name="logger">The logger for traceability.</param>
    public OptionsController(
        IOptionsScreenRenderer renderer,
        ISettingsService settingsService,
        IInputConfigurationService inputConfigService,
        ILogger<OptionsController> logger)
    {
        _renderer = renderer;
        _settingsService = settingsService;
        _inputConfigService = inputConfigService;
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

            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    CycleTab(vm, key.Modifiers.HasFlag(ConsoleModifiers.Shift));
                    break;

                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    NavigateUp(vm);
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    NavigateDown(vm);
                    break;

                case ConsoleKey.LeftArrow:
                case ConsoleKey.H:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, -1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case ConsoleKey.RightArrow:
                case ConsoleKey.L:
                    if (vm.ActiveTab != OptionsTab.Controls)
                    {
                        ModifySetting(vm, +1);
                        ApplyToGameSettings(vm);
                        RefreshItems(vm);
                    }
                    break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    await HandleActionOrToggleAsync(vm);
                    RefreshItems(vm);
                    break;

                case ConsoleKey.Escape:
                case ConsoleKey.Q:
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
            AutosaveIntervalMinutes = GameSettings.AutosaveIntervalMinutes
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
                    Name: "Autosave Interval",
                    ValueDisplay: OptionsViewHelper.FormatSliderValue(vm.AutosaveIntervalMinutes, "AutosaveIntervalMinutes"),
                    Type: SettingType.Slider,
                    IsSelected: index++ == vm.SelectedIndex,
                    MinValue: 1,
                    MaxValue: 60,
                    Step: 5,
                    PropertyName: "AutosaveIntervalMinutes"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: "Reset to Defaults",
                    ValueDisplay: "",
                    Type: SettingType.Action,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "ResetToDefaults"
                ));
                break;

            case OptionsTab.Display:
                vm.CurrentItems.Add(new SettingItemView(
                    Name: "Theme",
                    ValueDisplay: OptionsViewHelper.GetThemeName(vm.Theme),
                    Type: SettingType.Enum,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "Theme"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: "Reduce Motion",
                    ValueDisplay: OptionsViewHelper.FormatToggle(vm.ReduceMotion),
                    Type: SettingType.Toggle,
                    IsSelected: index++ == vm.SelectedIndex,
                    PropertyName: "ReduceMotion"
                ));
                vm.CurrentItems.Add(new SettingItemView(
                    Name: "Text Speed",
                    ValueDisplay: OptionsViewHelper.FormatSliderValue(vm.TextSpeed, "TextSpeed"),
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
                    Name: "Master Volume",
                    ValueDisplay: OptionsViewHelper.FormatSliderValue(vm.MasterVolume, "MasterVolume"),
                    Type: SettingType.Slider,
                    IsSelected: index++ == vm.SelectedIndex,
                    MinValue: 0,
                    MaxValue: 100,
                    Step: 5,
                    PropertyName: "MasterVolume"
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
    /// Refreshes the Bindings list for the Controls tab (v0.3.10c).
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
                ActionName: OptionsViewHelper.GetCommandDisplayName(cmd),
                KeyDisplay: OptionsViewHelper.FormatKeyName(key),
                Command: cmd,
                Category: OptionsViewHelper.GetCommandCategory(cmd),
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
                vm.Theme = OptionsViewHelper.CycleTheme(vm.Theme, direction);
                _logger.LogDebug("[Options] Theme changed to {Value}", OptionsViewHelper.GetThemeName(vm.Theme));
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
    /// Handles key rebinding in listening mode (v0.3.10c).
    /// Captures the next key press and assigns it to the selected action.
    /// </summary>
    private void HandleRebind(OptionsViewModel vm)
    {
        var binding = vm.Bindings[vm.SelectedIndex];
        _logger.LogDebug("[Options] Listening for new key for {Command}", binding.Command);

        // Display listening prompt by updating the view temporarily
        vm.CurrentItems.Clear();
        vm.CurrentItems.Add(new SettingItemView(
            Name: $"Press key for: {binding.ActionName}",
            ValueDisplay: "[yellow](Press Esc to cancel)[/]",
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
                    Name: $"Press key for: {binding.ActionName}",
                    ValueDisplay: "[yellow](Press Esc to cancel)[/]",
                    Type: SettingType.Action,
                    IsSelected: true
                )
            }
        };
        _renderer.Render(tempVm);

        // Wait for key press
        var keyInfo = Console.ReadKey(intercept: true);

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
        vm.AutosaveIntervalMinutes = GameSettings.AutosaveIntervalMinutes;

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
        GameSettings.AutosaveIntervalMinutes = vm.AutosaveIntervalMinutes;
    }
}
