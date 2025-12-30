namespace RuneAndRust.Core.Enums;

/// <summary>
/// Controls how a dialogue option is displayed when conditions fail.
/// </summary>
/// <remarks>See: v0.4.2b (The Lexicon) for Dialogue System implementation.</remarks>
public enum OptionVisibility
{
    /// <summary>
    /// Option is always visible. If conditions fail, show as locked with reason.
    /// Example: "[LOCKED: Friendly] Trade supplies"
    /// </summary>
    ShowLocked = 0,

    /// <summary>
    /// Option is hidden entirely when conditions fail.
    /// Player doesn't know the option exists until they qualify.
    /// </summary>
    Hidden = 1
}
