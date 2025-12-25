using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// JSON-based localization service implementation (v0.3.15b - The Translator).
/// Loads locale strings from data/locales/{locale}.json files with fallback chain.
/// </summary>
/// <remarks>
/// See: SPEC-LOC-001 for Localization System design.
/// v0.3.15a: Initial implementation with JSON loading and flattening.
/// v0.3.15b: Added two-tier fallback chain (primary -> fallback -> return key).
/// </remarks>
public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly string _localesPath;
    private Dictionary<string, string> _strings = new();
    private Dictionary<string, string> _fallbackStrings = new();

    private const string DefaultLocale = "en-US";

    /// <inheritdoc />
    public string CurrentLocale { get; private set; } = DefaultLocale;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
        _localesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "locales");
    }

    /// <inheritdoc />
    public async Task<bool> LoadLocaleAsync(string locale)
    {
        // Load primary locale
        var success = await LoadSingleLocaleAsync(locale, isPrimary: true);

        // Load fallback if different from default (v0.3.15b)
        if (locale != DefaultLocale)
        {
            await LoadSingleLocaleAsync(DefaultLocale, isPrimary: false);
        }
        else
        {
            _fallbackStrings.Clear(); // No fallback needed for en-US
        }

        return success;
    }

    /// <summary>
    /// Loads a single locale file into either primary or fallback dictionary.
    /// </summary>
    /// <param name="locale">The locale code to load.</param>
    /// <param name="isPrimary">True to load into primary strings, false for fallback.</param>
    /// <returns>True if loaded successfully.</returns>
    private async Task<bool> LoadSingleLocaleAsync(string locale, bool isPrimary)
    {
        var filePath = Path.Combine(_localesPath, $"{locale}.json");

        if (!File.Exists(filePath))
        {
            if (isPrimary)
            {
                _logger.LogWarning("[Localization] Locale file not found: {Path}. Falling back to {Default}",
                    filePath, DefaultLocale);

                if (locale != DefaultLocale)
                {
                    return await LoadSingleLocaleAsync(DefaultLocale, isPrimary: true);
                }

                _logger.LogError("[Localization] Default locale file not found: {Path}", filePath);
            }
            return false;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            using var document = JsonDocument.Parse(json);

            var targetDict = isPrimary ? _strings : _fallbackStrings;
            targetDict.Clear();
            FlattenJson(document.RootElement, string.Empty, targetDict);

            if (isPrimary)
            {
                CurrentLocale = locale;
                _logger.LogInformation("[Localization] Loaded locale {Locale} with {Count} strings",
                    locale, _strings.Count);
            }
            else
            {
                _logger.LogDebug("[Localization] Loaded fallback locale {Locale} with {Count} strings",
                    locale, _fallbackStrings.Count);
            }

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[Localization] Failed to parse locale file: {Path}", filePath);
            return false;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "[Localization] Failed to read locale file: {Path}", filePath);
            return false;
        }
    }

    /// <inheritdoc />
    public string Get(string key)
    {
        // Try primary locale first
        if (_strings.TryGetValue(key, out var value))
        {
            return value;
        }

        // Try fallback locale (v0.3.15b)
        if (_fallbackStrings.TryGetValue(key, out var fallbackValue))
        {
            _logger.LogDebug("[Localization] Key {Key} not in primary locale, using fallback", key);
            return fallbackValue;
        }

        _logger.LogDebug("[Localization] Key not found: {Key}", key);
        return key; // Return key for visibility during development
    }

    /// <inheritdoc />
    public string Get(string key, params object[] args)
    {
        var template = Get(key);

        if (template == key)
        {
            return key; // Key not found, don't format
        }

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "[Localization] Format failed for key {Key} with {ArgCount} args",
                key, args.Length);
            return template;
        }
    }

    /// <inheritdoc />
    public bool HasKey(string key) => _strings.ContainsKey(key) || _fallbackStrings.ContainsKey(key);

    /// <inheritdoc />
    public IReadOnlyList<string> GetMissingKeys()
    {
        return LocKeys.AllKeys
            .Where(k => !_strings.ContainsKey(k) && !_fallbackStrings.ContainsKey(k))
            .ToList();
    }

    /// <summary>
    /// Recursively flattens a JSON element into dot-separated key-value pairs.
    /// Skips the "meta" section which contains locale metadata.
    /// </summary>
    /// <param name="element">The JSON element to process.</param>
    /// <param name="prefix">The current key prefix (dot-separated path).</param>
    /// <param name="target">The dictionary to populate with flattened keys.</param>
    private void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> target)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    // Skip metadata section
                    if (property.Name == "meta" || property.Name == "$schema")
                        continue;

                    var newPrefix = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}.{property.Name}";

                    FlattenJson(property.Value, newPrefix, target);
                }
                break;

            case JsonValueKind.String:
                target[prefix] = element.GetString() ?? string.Empty;
                break;

            case JsonValueKind.Number:
                target[prefix] = element.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                target[prefix] = element.GetBoolean().ToString();
                break;
        }
    }
}
