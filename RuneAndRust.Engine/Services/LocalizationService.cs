using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Constants;
using RuneAndRust.Core.Interfaces;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// JSON-based localization service implementation (v0.3.15a - The Lexicon).
/// Loads locale strings from data/locales/{locale}.json files.
/// </summary>
/// <remarks>See: SPEC-LOC-001 for Localization System design.</remarks>
public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly string _localesPath;
    private Dictionary<string, string> _strings = new();

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
        var filePath = Path.Combine(_localesPath, $"{locale}.json");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("[Localization] Locale file not found: {Path}. Falling back to {Default}",
                filePath, DefaultLocale);

            if (locale != DefaultLocale)
            {
                return await LoadLocaleAsync(DefaultLocale);
            }

            _logger.LogError("[Localization] Default locale file not found: {Path}", filePath);
            return false;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            using var document = JsonDocument.Parse(json);

            _strings.Clear();
            FlattenJson(document.RootElement, string.Empty);

            CurrentLocale = locale;
            _logger.LogInformation("[Localization] Loaded locale {Locale} with {Count} strings",
                locale, _strings.Count);

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
        if (_strings.TryGetValue(key, out var value))
        {
            return value;
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
    public bool HasKey(string key) => _strings.ContainsKey(key);

    /// <inheritdoc />
    public IReadOnlyList<string> GetMissingKeys()
    {
        return LocKeys.AllKeys
            .Where(k => !_strings.ContainsKey(k))
            .ToList();
    }

    /// <summary>
    /// Recursively flattens a JSON element into dot-separated key-value pairs.
    /// Skips the "meta" section which contains locale metadata.
    /// </summary>
    /// <param name="element">The JSON element to process.</param>
    /// <param name="prefix">The current key prefix (dot-separated path).</param>
    private void FlattenJson(JsonElement element, string prefix)
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

                    FlattenJson(property.Value, newPrefix);
                }
                break;

            case JsonValueKind.String:
                _strings[prefix] = element.GetString() ?? string.Empty;
                break;

            case JsonValueKind.Number:
                _strings[prefix] = element.GetRawText();
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                _strings[prefix] = element.GetBoolean().ToString();
                break;
        }
    }
}
