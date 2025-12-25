namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Service contract for localized string retrieval (v0.3.15c - The Polyglot).
/// Provides string lookup by key with optional format arguments and fallback behavior.
/// </summary>
/// <remarks>
/// See: SPEC-LOC-001 for Localization System design.
/// Key format: "Category.Subcategory.Element" (e.g., "UI.MainMenu.NewGame").
/// Use constants from <see cref="Constants.LocKeys"/> for type-safe key references.
/// </remarks>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the currently loaded locale identifier (e.g., "en-US").
    /// </summary>
    string CurrentLocale { get; }

    /// <summary>
    /// Loads locale data from the specified locale file.
    /// Falls back to en-US if the specified locale is not found.
    /// </summary>
    /// <param name="locale">The locale identifier (e.g., "en-US", "de-DE").</param>
    /// <returns>True if the locale was loaded successfully, false if fallback was used.</returns>
    Task<bool> LoadLocaleAsync(string locale);

    /// <summary>
    /// Retrieves a localized string by key.
    /// Returns the key itself if not found (for debugging/visibility).
    /// </summary>
    /// <param name="key">The localization key (use LocKeys constants).</param>
    /// <returns>The localized string, or the key if not found.</returns>
    string Get(string key);

    /// <summary>
    /// Retrieves a localized string by key with format arguments.
    /// Supports {0}, {1}, etc. placeholders in the localized string.
    /// </summary>
    /// <param name="key">The localization key (use LocKeys constants).</param>
    /// <param name="args">Format arguments to substitute into the string.</param>
    /// <returns>The formatted localized string, or the key if not found.</returns>
    string Get(string key, params object[] args);

    /// <summary>
    /// Checks if a key exists in the current locale.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    bool HasKey(string key);

    /// <summary>
    /// Gets all keys that are defined in LocKeys but missing from the current locale file.
    /// Useful for validation and debugging during development.
    /// </summary>
    /// <returns>List of missing keys.</returns>
    IReadOnlyList<string> GetMissingKeys();

    /// <summary>
    /// Gets a list of available locale codes based on installed JSON files.
    /// Always includes "qps-ploc" for pseudo-localization testing (v0.3.15c).
    /// </summary>
    /// <returns>Sorted list of locale codes (e.g., "en-US", "qps-ploc").</returns>
    IReadOnlyList<string> GetAvailableLocales();
}
