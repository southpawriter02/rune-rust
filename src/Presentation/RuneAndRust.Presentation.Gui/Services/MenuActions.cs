namespace RuneAndRust.Presentation.Gui.Services;

/// <summary>
/// Static class containing menu action definitions with icons.
/// </summary>
public static class MenuActions
{
    /// <summary>
    /// The examine action icon.
    /// </summary>
    public const string ExamineIcon = "🔍";

    /// <summary>
    /// The use action icon.
    /// </summary>
    public const string UseIcon = "✋";

    /// <summary>
    /// The equip action icon.
    /// </summary>
    public const string EquipIcon = "⚔";

    /// <summary>
    /// The unequip action icon.
    /// </summary>
    public const string UnequipIcon = "⬆️";

    /// <summary>
    /// The drop action icon.
    /// </summary>
    public const string DropIcon = "⬇️";

    /// <summary>
    /// The take action icon.
    /// </summary>
    public const string TakeIcon = "✋";

    /// <summary>
    /// The attack action icon.
    /// </summary>
    public const string AttackIcon = "⚔";

    /// <summary>
    /// The talk action icon.
    /// </summary>
    public const string TalkIcon = "💬";

    /// <summary>
    /// The cancel action icon.
    /// </summary>
    public const string CancelIcon = "❌";

    /// <summary>
    /// Formats an Examine menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Examine(string label = "Examine") => $"{ExamineIcon} {label}";

    /// <summary>
    /// Formats a Use menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Use(string label = "Use") => $"{UseIcon} {label}";

    /// <summary>
    /// Formats an Equip menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Equip(string label = "Equip") => $"{EquipIcon} {label}";

    /// <summary>
    /// Formats an Unequip menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Unequip(string label = "Unequip") => $"{UnequipIcon} {label}";

    /// <summary>
    /// Formats a Drop menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Drop(string label = "Drop") => $"{DropIcon} {label}";

    /// <summary>
    /// Formats a Take menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Take(string label = "Take") => $"{TakeIcon} {label}";

    /// <summary>
    /// Formats an Attack menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Attack(string label = "Attack") => $"{AttackIcon} {label}";

    /// <summary>
    /// Formats a Talk menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Talk(string label = "Talk") => $"{TalkIcon} {label}";

    /// <summary>
    /// Formats a Cancel menu item label.
    /// </summary>
    /// <param name="label">Optional custom label.</param>
    /// <returns>Formatted label with icon.</returns>
    public static string Cancel(string label = "Cancel") => $"{CancelIcon} {label}";
}
