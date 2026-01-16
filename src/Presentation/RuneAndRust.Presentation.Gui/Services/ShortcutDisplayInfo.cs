namespace RuneAndRust.Presentation.Gui.Services;

using Avalonia.Input;

/// <summary>
/// Display information for a keyboard shortcut.
/// </summary>
/// <param name="ActionId">The unique action identifier.</param>
/// <param name="DisplayName">The user-friendly display name.</param>
/// <param name="Gesture">The bound key gesture.</param>
/// <param name="Context">The shortcut context.</param>
public record ShortcutDisplayInfo(
    string ActionId,
    string DisplayName,
    KeyGesture Gesture,
    ShortcutContext Context)
{
    /// <summary>
    /// Gets a formatted string representation of the key gesture.
    /// </summary>
    public string GestureText => FormatGesture(Gesture);

    private static string FormatGesture(KeyGesture gesture)
    {
        var parts = new List<string>();

        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");

        parts.Add(FormatKey(gesture.Key));

        return string.Join("+", parts);
    }

    private static string FormatKey(Key key) => key switch
    {
        Key.D1 => "1",
        Key.D2 => "2",
        Key.D3 => "3",
        Key.D4 => "4",
        Key.D5 => "5",
        Key.D6 => "6",
        Key.D7 => "7",
        Key.D8 => "8",
        Key.D9 => "9",
        Key.D0 => "0",
        _ => key.ToString()
    };
}
