namespace RuneAndRust.Presentation.Gui.Models;

/// <summary>
/// Aggregate settings model containing all setting categories.
/// </summary>
public class GameSettings
{
    /// <summary>Audio settings.</summary>
    public AudioSettings Audio { get; set; } = new();

    /// <summary>Display settings.</summary>
    public DisplaySettings Display { get; set; } = new();

    /// <summary>Gameplay settings.</summary>
    public GameplaySettings Gameplay { get; set; } = new();

    /// <summary>Controls settings.</summary>
    public ControlsSettings Controls { get; set; } = new();

    /// <summary>Creates a deep clone of the settings.</summary>
    public GameSettings Clone() => new()
    {
        Audio = Audio.Clone(),
        Display = Display.Clone(),
        Gameplay = Gameplay.Clone(),
        Controls = Controls.Clone()
    };
}

/// <summary>Audio-related settings.</summary>
public class AudioSettings
{
    public int MasterVolume { get; set; } = 75;
    public int MusicVolume { get; set; } = 50;
    public int SfxVolume { get; set; } = 85;
    public bool EnableSoundEffects { get; set; } = true;
    public bool MuteAll { get; set; }

    public AudioSettings Clone() => new()
    {
        MasterVolume = MasterVolume,
        MusicVolume = MusicVolume,
        SfxVolume = SfxVolume,
        EnableSoundEffects = EnableSoundEffects,
        MuteAll = MuteAll
    };
}

/// <summary>Display-related settings.</summary>
public class DisplaySettings
{
    public string Theme { get; set; } = "Dark Fantasy";
    public int FontSizePercent { get; set; } = 100;
    public bool EnableCombatAnimations { get; set; } = true;
    public bool EnableRoomTransitions { get; set; } = true;
    public bool HighContrastMode { get; set; }

    public DisplaySettings Clone() => new()
    {
        Theme = Theme,
        FontSizePercent = FontSizePercent,
        EnableCombatAnimations = EnableCombatAnimations,
        EnableRoomTransitions = EnableRoomTransitions,
        HighContrastMode = HighContrastMode
    };
}

/// <summary>Gameplay-related settings.</summary>
public class GameplaySettings
{
    public string Difficulty { get; set; } = "Normal";
    public string AutoSaveFrequency { get; set; } = "Every 5 minutes";
    public bool ShowCombatTooltips { get; set; } = true;
    public bool ShowTutorialHints { get; set; } = true;
    public bool ConfirmDangerousActions { get; set; }

    public GameplaySettings Clone() => new()
    {
        Difficulty = Difficulty,
        AutoSaveFrequency = AutoSaveFrequency,
        ShowCombatTooltips = ShowCombatTooltips,
        ShowTutorialHints = ShowTutorialHints,
        ConfirmDangerousActions = ConfirmDangerousActions
    };
}

/// <summary>Controls/keybinding settings.</summary>
public class ControlsSettings
{
    public Dictionary<string, string> GlobalBindings { get; set; } = new()
    {
        ["Help / Shortcuts"] = "F1",
        ["Quick Save"] = "F5",
        ["Quick Load"] = "F9",
        ["Close Window"] = "Escape"
    };

    public Dictionary<string, string> GameBindings { get; set; } = new()
    {
        ["Toggle Inventory"] = "I",
        ["Toggle Map"] = "M",
        ["Toggle Quest Log"] = "J",
        ["Toggle Character"] = "C"
    };

    public ControlsSettings Clone() => new()
    {
        GlobalBindings = new Dictionary<string, string>(GlobalBindings),
        GameBindings = new Dictionary<string, string>(GameBindings)
    };
}
