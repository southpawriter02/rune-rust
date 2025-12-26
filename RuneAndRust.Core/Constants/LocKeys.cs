using System.Reflection;

namespace RuneAndRust.Core.Constants;

/// <summary>
/// Static registry of all localization key constants (v0.3.15a - The Lexicon).
/// Key naming convention: CATEGORY_Subcategory_ElementName -> "Category.Subcategory.ElementName"
/// </summary>
/// <remarks>See: SPEC-LOC-001 for Localization System design.</remarks>
public static class LocKeys
{
    #region UI.MainMenu

    /// <summary>Main menu title prompt.</summary>
    public const string UI_MainMenu_SelectOption = "UI.MainMenu.SelectOption";

    /// <summary>New Game menu option.</summary>
    public const string UI_MainMenu_NewGame = "UI.MainMenu.NewGame";

    /// <summary>Load Game menu option.</summary>
    public const string UI_MainMenu_LoadGame = "UI.MainMenu.LoadGame";

    /// <summary>Options menu option.</summary>
    public const string UI_MainMenu_Options = "UI.MainMenu.Options";

    /// <summary>Quit menu option.</summary>
    public const string UI_MainMenu_Quit = "UI.MainMenu.Quit";

    /// <summary>Version string display.</summary>
    public const string UI_MainMenu_Version = "UI.MainMenu.Version";

    /// <summary>No saved games found message.</summary>
    public const string UI_MainMenu_NoSaves = "UI.MainMenu.NoSaves";

    /// <summary>Options not implemented message.</summary>
    public const string UI_MainMenu_OptionsNotImplemented = "UI.MainMenu.OptionsNotImplemented";

    #endregion

    #region UI.Options

    /// <summary>OPTIONS header title.</summary>
    public const string UI_Options_Title = "UI.Options.Title";

    /// <summary>No settings in tab message.</summary>
    public const string UI_Options_NoSettings = "UI.Options.NoSettings";

    /// <summary>No bindings defined message.</summary>
    public const string UI_Options_NoBindings = "UI.Options.NoBindings";

    /// <summary>Key Bindings panel header.</summary>
    public const string UI_Options_KeyBindingsHeader = "UI.Options.KeyBindingsHeader";

    /// <summary>Settings panel header suffix.</summary>
    public const string UI_Options_SettingsHeaderSuffix = "UI.Options.SettingsHeaderSuffix";

    /// <summary>Press Enter action label.</summary>
    public const string UI_Options_PressEnter = "UI.Options.PressEnter";

    /// <summary>Press key prompt for rebinding. Expects {0}=ActionName.</summary>
    public const string UI_Options_PressKeyFor = "UI.Options.PressKeyFor";

    /// <summary>Press Esc to cancel rebind.</summary>
    public const string UI_Options_PressEscCancel = "UI.Options.PressEscCancel";

    /// <summary>Settings footer legend (non-controls tab).</summary>
    public const string UI_Options_FooterSettings = "UI.Options.FooterSettings";

    /// <summary>Controls footer legend.</summary>
    public const string UI_Options_FooterControls = "UI.Options.FooterControls";

    #endregion

    #region UI.Options.Tabs

    /// <summary>General tab label.</summary>
    public const string UI_Options_Tab_General = "UI.Options.Tabs.General";

    /// <summary>Display tab label.</summary>
    public const string UI_Options_Tab_Display = "UI.Options.Tabs.Display";

    /// <summary>Audio tab label.</summary>
    public const string UI_Options_Tab_Audio = "UI.Options.Tabs.Audio";

    /// <summary>Controls tab label.</summary>
    public const string UI_Options_Tab_Controls = "UI.Options.Tabs.Controls";

    #endregion

    #region UI.Options.Settings

    /// <summary>Autosave Interval setting label.</summary>
    public const string UI_Options_Setting_AutosaveInterval = "UI.Options.Settings.AutosaveInterval";

    /// <summary>Reset to Defaults action label.</summary>
    public const string UI_Options_Setting_ResetToDefaults = "UI.Options.Settings.ResetToDefaults";

    /// <summary>Theme setting label.</summary>
    public const string UI_Options_Setting_Theme = "UI.Options.Settings.Theme";

    /// <summary>Reduce Motion setting label.</summary>
    public const string UI_Options_Setting_ReduceMotion = "UI.Options.Settings.ReduceMotion";

    /// <summary>Text Speed setting label.</summary>
    public const string UI_Options_Setting_TextSpeed = "UI.Options.Settings.TextSpeed";

    /// <summary>Master Volume setting label.</summary>
    public const string UI_Options_Setting_MasterVolume = "UI.Options.Settings.MasterVolume";

    /// <summary>Ambient Soundscape setting label (v0.3.19c).</summary>
    public const string UI_Options_Setting_AmbienceEnabled = "UI.Options.Settings.AmbienceEnabled";

    /// <summary>Language setting label (v0.3.15c).</summary>
    public const string UI_Options_Setting_Language = "UI.Options.Settings.Language";

    #endregion

    #region UI.Options.Themes

    /// <summary>Standard theme name.</summary>
    public const string UI_Options_Theme_Standard = "UI.Options.Themes.Standard";

    /// <summary>High Contrast theme name.</summary>
    public const string UI_Options_Theme_HighContrast = "UI.Options.Themes.HighContrast";

    /// <summary>Protanopia theme name.</summary>
    public const string UI_Options_Theme_Protanopia = "UI.Options.Themes.Protanopia";

    /// <summary>Deuteranopia theme name.</summary>
    public const string UI_Options_Theme_Deuteranopia = "UI.Options.Themes.Deuteranopia";

    /// <summary>Tritanopia theme name.</summary>
    public const string UI_Options_Theme_Tritanopia = "UI.Options.Themes.Tritanopia";

    /// <summary>Unknown theme name (fallback).</summary>
    public const string UI_Options_Theme_Unknown = "UI.Options.Themes.Unknown";

    #endregion

    #region UI.Options.Toggle

    /// <summary>Toggle ON display text.</summary>
    public const string UI_Options_Toggle_On = "UI.Options.Toggle.On";

    /// <summary>Toggle OFF display text.</summary>
    public const string UI_Options_Toggle_Off = "UI.Options.Toggle.Off";

    #endregion

    #region UI.Options.Units

    /// <summary>Minutes unit format. Expects {0}=value.</summary>
    public const string UI_Options_Unit_Minutes = "UI.Options.Units.Minutes";

    /// <summary>Percent unit format. Expects {0}=value.</summary>
    public const string UI_Options_Unit_Percent = "UI.Options.Units.Percent";

    #endregion

    #region UI.Options.Commands (Key Binding Action Names)

    /// <summary>Move North command name.</summary>
    public const string UI_Options_Command_MoveNorth = "UI.Options.Commands.MoveNorth";

    /// <summary>Move South command name.</summary>
    public const string UI_Options_Command_MoveSouth = "UI.Options.Commands.MoveSouth";

    /// <summary>Move East command name.</summary>
    public const string UI_Options_Command_MoveEast = "UI.Options.Commands.MoveEast";

    /// <summary>Move West command name.</summary>
    public const string UI_Options_Command_MoveWest = "UI.Options.Commands.MoveWest";

    /// <summary>Move Up command name.</summary>
    public const string UI_Options_Command_MoveUp = "UI.Options.Commands.MoveUp";

    /// <summary>Move Down command name.</summary>
    public const string UI_Options_Command_MoveDown = "UI.Options.Commands.MoveDown";

    /// <summary>Confirm command name.</summary>
    public const string UI_Options_Command_Confirm = "UI.Options.Commands.Confirm";

    /// <summary>Cancel/Back command name.</summary>
    public const string UI_Options_Command_Cancel = "UI.Options.Commands.Cancel";

    /// <summary>Menu command name.</summary>
    public const string UI_Options_Command_Menu = "UI.Options.Commands.Menu";

    /// <summary>Help command name.</summary>
    public const string UI_Options_Command_Help = "UI.Options.Commands.Help";

    /// <summary>Inventory command name.</summary>
    public const string UI_Options_Command_Inventory = "UI.Options.Commands.Inventory";

    /// <summary>Character command name.</summary>
    public const string UI_Options_Command_Character = "UI.Options.Commands.Character";

    /// <summary>Journal command name.</summary>
    public const string UI_Options_Command_Journal = "UI.Options.Commands.Journal";

    /// <summary>Crafting command name.</summary>
    public const string UI_Options_Command_Crafting = "UI.Options.Commands.Crafting";

    /// <summary>Interact command name.</summary>
    public const string UI_Options_Command_Interact = "UI.Options.Commands.Interact";

    /// <summary>Look command name.</summary>
    public const string UI_Options_Command_Look = "UI.Options.Commands.Look";

    /// <summary>Search command name.</summary>
    public const string UI_Options_Command_Search = "UI.Options.Commands.Search";

    /// <summary>Wait command name.</summary>
    public const string UI_Options_Command_Wait = "UI.Options.Commands.Wait";

    /// <summary>Attack command name.</summary>
    public const string UI_Options_Command_Attack = "UI.Options.Commands.Attack";

    /// <summary>Light Attack command name.</summary>
    public const string UI_Options_Command_LightAttack = "UI.Options.Commands.LightAttack";

    /// <summary>Heavy Attack command name.</summary>
    public const string UI_Options_Command_HeavyAttack = "UI.Options.Commands.HeavyAttack";

    #endregion

    #region UI.Options.Categories (Key Binding Categories)

    /// <summary>Movement category name.</summary>
    public const string UI_Options_Category_Movement = "UI.Options.Categories.Movement";

    /// <summary>Core category name.</summary>
    public const string UI_Options_Category_Core = "UI.Options.Categories.Core";

    /// <summary>Screens category name.</summary>
    public const string UI_Options_Category_Screens = "UI.Options.Categories.Screens";

    /// <summary>Gameplay category name.</summary>
    public const string UI_Options_Category_Gameplay = "UI.Options.Categories.Gameplay";

    /// <summary>Combat category name.</summary>
    public const string UI_Options_Category_Combat = "UI.Options.Categories.Combat";

    /// <summary>Other category name.</summary>
    public const string UI_Options_Category_Other = "UI.Options.Categories.Other";

    #endregion

    #region UI.Options.Keys (Key Names)

    /// <summary>Unbound key display.</summary>
    public const string UI_Options_Key_Unbound = "UI.Options.Keys.Unbound";

    /// <summary>Space key display.</summary>
    public const string UI_Options_Key_Space = "UI.Options.Keys.Space";

    /// <summary>Enter key display.</summary>
    public const string UI_Options_Key_Enter = "UI.Options.Keys.Enter";

    /// <summary>Escape key display.</summary>
    public const string UI_Options_Key_Escape = "UI.Options.Keys.Escape";

    /// <summary>Tab key display.</summary>
    public const string UI_Options_Key_Tab = "UI.Options.Keys.Tab";

    /// <summary>Backspace key display.</summary>
    public const string UI_Options_Key_Backspace = "UI.Options.Keys.Backspace";

    /// <summary>Delete key display.</summary>
    public const string UI_Options_Key_Delete = "UI.Options.Keys.Delete";

    /// <summary>Insert key display.</summary>
    public const string UI_Options_Key_Insert = "UI.Options.Keys.Insert";

    /// <summary>Home key display.</summary>
    public const string UI_Options_Key_Home = "UI.Options.Keys.Home";

    /// <summary>End key display.</summary>
    public const string UI_Options_Key_End = "UI.Options.Keys.End";

    /// <summary>Page Up key display.</summary>
    public const string UI_Options_Key_PageUp = "UI.Options.Keys.PageUp";

    /// <summary>Page Down key display.</summary>
    public const string UI_Options_Key_PageDown = "UI.Options.Keys.PageDown";

    #endregion

    #region UI.Creation (Character Creation Wizard)

    /// <summary>Character creation screen title.</summary>
    public const string UI_Creation_Title = "UI.Creation.Title";

    /// <summary>Character creation subtitle/flavor text line 1.</summary>
    public const string UI_Creation_Subtitle = "UI.Creation.Subtitle";

    /// <summary>Character creation instruction/flavor text line 2.</summary>
    public const string UI_Creation_Instruction = "UI.Creation.Instruction";

    /// <summary>Name entry prompt.</summary>
    public const string UI_Creation_NamePrompt = "UI.Creation.NamePrompt";

    /// <summary>Name entry cancel hint.</summary>
    public const string UI_Creation_NameCancelHint = "UI.Creation.NameCancelHint";

    /// <summary>Duplicate name error message.</summary>
    public const string UI_Creation_DuplicateName = "UI.Creation.DuplicateName";

    /// <summary>Empty name validation error.</summary>
    public const string UI_Creation_NameEmpty = "UI.Creation.NameEmpty";

    /// <summary>Name too short validation error.</summary>
    public const string UI_Creation_NameTooShort = "UI.Creation.NameTooShort";

    /// <summary>Name too long validation error.</summary>
    public const string UI_Creation_NameTooLong = "UI.Creation.NameTooLong";

    /// <summary>Lineage selection step title.</summary>
    public const string UI_Creation_Step_Lineage = "UI.Creation.Steps.Lineage";

    /// <summary>Archetype selection step title.</summary>
    public const string UI_Creation_Step_Archetype = "UI.Creation.Steps.Archetype";

    /// <summary>Background selection step title.</summary>
    public const string UI_Creation_Step_Background = "UI.Creation.Steps.Background";

    /// <summary>Stats preview panel header.</summary>
    public const string UI_Creation_Preview = "UI.Creation.Preview";

    /// <summary>Navigation hint for selection menus.</summary>
    public const string UI_Creation_NavigationHint = "UI.Creation.NavigationHint";

    /// <summary>Success screen panel header.</summary>
    public const string UI_Creation_Success_Title = "UI.Creation.Success.Title";

    /// <summary>Success message. Expects {0}=CharacterName.</summary>
    public const string UI_Creation_Success_Message = "UI.Creation.Success.Message";

    /// <summary>Lineage label on success screen.</summary>
    public const string UI_Creation_Success_LineageLabel = "UI.Creation.Success.LineageLabel";

    /// <summary>Archetype label on success screen.</summary>
    public const string UI_Creation_Success_ArchetypeLabel = "UI.Creation.Success.ArchetypeLabel";

    /// <summary>Background label on success screen.</summary>
    public const string UI_Creation_Success_BackgroundLabel = "UI.Creation.Success.BackgroundLabel";

    /// <summary>Closing flavor text on success screen.</summary>
    public const string UI_Creation_Success_Closing = "UI.Creation.Success.Closing";

    /// <summary>Continue prompt (press any key).</summary>
    public const string UI_Creation_ContinuePrompt = "UI.Creation.ContinuePrompt";

    #endregion

    #region Art.TitleScreen

    /// <summary>ASCII art for the title screen logo.</summary>
    public const string Art_TitleScreen_Logo = "Art.TitleScreen.Logo";

    #endregion

    #region Validation Helpers

    /// <summary>
    /// Gets all defined localization keys for validation purposes.
    /// Uses reflection to enumerate all public const string fields.
    /// </summary>
    public static IReadOnlyList<string> AllKeys => _allKeys.Value;

    private static readonly Lazy<List<string>> _allKeys = new(() =>
    {
        var fields = typeof(LocKeys).GetFields(
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.FlattenHierarchy);

        return fields
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string) && f.Name != nameof(AllKeys))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();
    });

    #endregion
}
