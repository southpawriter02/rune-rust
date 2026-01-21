using RuneAndRust.Presentation.Shared.Enums;

namespace RuneAndRust.Presentation.Shared.ValueObjects;

/// <summary>
/// Contains icon definitions for both Unicode and ASCII representations.
/// </summary>
/// <remarks>
/// <para>IconSet provides icons with fallback options for terminals that
/// don't support Unicode characters.</para>
/// <para>Access icons via <see cref="IconKey"/> for type-safe retrieval.</para>
/// </remarks>
public class IconSet
{
    private readonly Dictionary<IconKey, (string Unicode, string Ascii)> _icons;

    /// <summary>
    /// Initializes a new IconSet with the provided icon definitions.
    /// </summary>
    /// <param name="icons">Dictionary of icon key to (Unicode, ASCII) pairs.</param>
    private IconSet(Dictionary<IconKey, (string Unicode, string Ascii)> icons)
    {
        _icons = icons;
    }

    /// <summary>
    /// Creates the default icon set with Unicode and ASCII representations.
    /// </summary>
    /// <returns>A new IconSet with standard game icons.</returns>
    public static IconSet CreateDefault()
    {
        var icons = new Dictionary<IconKey, (string Unicode, string Ascii)>
        {
            // Stats
            [IconKey.Health] = ("♥", "[HP]"),
            [IconKey.Mana] = ("✦", "[MP]"),
            [IconKey.Attack] = ("⚔", "[ATK]"),
            [IconKey.Defense] = ("🛡", "[DEF]"),
            [IconKey.Speed] = ("⚡", "[SPD]"),
            [IconKey.Luck] = ("☘", "[LCK]"),

            // Status Effects
            [IconKey.Buff] = ("↑", "[+]"),
            [IconKey.Debuff] = ("↓", "[-]"),
            [IconKey.Fire] = ("🔥", "[F]"),
            [IconKey.Ice] = ("❄", "[I]"),
            [IconKey.Poison] = ("☠", "[P]"),
            [IconKey.Lightning] = ("⚡", "[L]"),
            [IconKey.Stun] = ("★", "[*]"),
            [IconKey.Shield] = ("🛡", "[S]"),

            // Resources/Materials
            [IconKey.Ore] = ("◆", "[O]"),
            [IconKey.Herb] = ("♣", "[H]"),
            [IconKey.Leather] = ("▬", "[L]"),
            [IconKey.Gem] = ("◇", "[G]"),
            [IconKey.Wood] = ("♠", "[W]"),

            // Navigation
            [IconKey.ArrowUp] = ("↑", "^"),
            [IconKey.ArrowDown] = ("↓", "v"),
            [IconKey.ArrowLeft] = ("←", "<"),
            [IconKey.ArrowRight] = ("→", ">"),

            // UI Indicators
            [IconKey.Check] = ("✓", "[X]"),
            [IconKey.Cross] = ("✗", "[ ]"),
            [IconKey.Warning] = ("⚠", "[!]"),
            [IconKey.Info] = ("ⓘ", "[?]"),
            [IconKey.Lock] = ("🔒", "[L]"),
            [IconKey.Unlock] = ("🔓", "[U]"),
            [IconKey.Star] = ("★", "*"),
            [IconKey.StarEmpty] = ("☆", "."),

            // Entities
            [IconKey.Player] = ("@", "@"),
            [IconKey.Enemy] = ("M", "M"),
            [IconKey.Boss] = ("B", "B"),
            [IconKey.Npc] = ("N", "N"),

            // Dice
            [IconKey.D20] = ("🎲", "[D20]"),
            [IconKey.CriticalSuccess] = ("★", "[20!]"),
            [IconKey.CriticalFailure] = ("✗", "[1!]")
        };

        return new IconSet(icons);
    }

    /// <summary>
    /// Gets the Unicode icon for the specified key.
    /// </summary>
    /// <param name="key">The icon key to retrieve.</param>
    /// <returns>The Unicode icon string, or a fallback if not found.</returns>
    public string GetUnicodeIcon(IconKey key) =>
        _icons.TryGetValue(key, out var icon) ? icon.Unicode : "?";

    /// <summary>
    /// Gets the ASCII fallback icon for the specified key.
    /// </summary>
    /// <param name="key">The icon key to retrieve.</param>
    /// <returns>The ASCII icon string, or a fallback if not found.</returns>
    public string GetAsciiIcon(IconKey key) =>
        _icons.TryGetValue(key, out var icon) ? icon.Ascii : "?";

    /// <summary>
    /// Gets an icon based on the Unicode preference setting.
    /// </summary>
    /// <param name="key">The icon key to retrieve.</param>
    /// <param name="useUnicode">Whether to use Unicode (true) or ASCII (false).</param>
    /// <returns>The appropriate icon string.</returns>
    public string GetIcon(IconKey key, bool useUnicode = true) =>
        useUnicode ? GetUnicodeIcon(key) : GetAsciiIcon(key);

    /// <summary>
    /// Checks if the set contains an icon for the specified key.
    /// </summary>
    /// <param name="key">The icon key to check.</param>
    /// <returns>True if the icon is defined; otherwise false.</returns>
    public bool ContainsIcon(IconKey key) => _icons.ContainsKey(key);
}
