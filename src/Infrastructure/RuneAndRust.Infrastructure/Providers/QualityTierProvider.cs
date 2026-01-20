// ═══════════════════════════════════════════════════════════════════════════════
// QualityTierProvider.cs
// JSON-based provider for quality tier definitions with lazy loading.
// Follows the provider pattern established by RecipeProvider.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// JSON-based provider for quality tier definitions with lazy loading.
/// </summary>
/// <remarks>
/// <para>
/// QualityTierProvider loads quality tier definitions from a JSON configuration file
/// and provides thread-safe access to the loaded definitions with indexed lookups
/// by quality level.
/// </para>
/// <para>
/// Tier definitions are loaded lazily on first access, allowing the application to
/// start quickly while deferring the loading cost until quality tiers are needed.
/// </para>
/// <para>Expected JSON format:</para>
/// <code>
/// {
///   "tiers": [
///     {
///       "quality": "Standard",
///       "displayName": "Standard",
///       "colorCode": "#FFFFFF",
///       "statMultiplier": 1.0,
///       "valueMultiplier": 1.0,
///       "minimumMargin": 0,
///       "requiresNatural20": false
///     },
///     {
///       "quality": "Legendary",
///       "displayName": "Legendary",
///       "colorCode": "#FF8000",
///       "statMultiplier": 1.50,
///       "valueMultiplier": 5.0,
///       "minimumMargin": 0,
///       "requiresNatural20": true
///     }
///   ]
/// }
/// </code>
/// <para>
/// If the configuration file is missing or invalid, default tier definitions are
/// used to ensure the system remains functional.
/// </para>
/// </remarks>
public sealed class QualityTierProvider : IQualityTierProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: quality level -> QualityTierDefinition.
    /// </summary>
    private readonly Dictionary<CraftedItemQuality, QualityTierDefinition> _tiers = [];

    /// <summary>
    /// Ordered list of all tier definitions.
    /// </summary>
    private readonly List<QualityTierDefinition> _orderedTiers = [];

    /// <summary>
    /// Path to the configuration file.
    /// </summary>
    private readonly string _configPath;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<QualityTierProvider> _logger;

    /// <summary>
    /// Flag indicating whether tier definitions have been loaded.
    /// </summary>
    private bool _isLoaded;

    /// <summary>
    /// Lock object for thread-safe lazy loading.
    /// </summary>
    private readonly object _loadLock = new();

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new QualityTierProvider instance with lazy loading.
    /// </summary>
    /// <param name="configPath">Path to the quality-tiers.json configuration file.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configPath"/> or <paramref name="logger"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Tier definitions are not loaded until first access. This allows the application
    /// to start quickly while deferring the loading cost.
    /// </para>
    /// <para>
    /// If the configuration file is missing or invalid, default tier definitions
    /// will be used automatically.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var provider = new QualityTierProvider(
    ///     "config/quality-tiers.json",
    ///     loggerFactory.CreateLogger&lt;QualityTierProvider&gt;());
    ///
    /// // Tiers loaded on first access
    /// var masterworkTier = provider.GetTier(CraftedItemQuality.Masterwork);
    /// </code>
    /// </example>
    public QualityTierProvider(string configPath, ILogger<QualityTierProvider> logger)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(
            "Quality tier provider created with configuration path: {ConfigPath}",
            configPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IQualityTierProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public QualityTierDefinition GetTier(CraftedItemQuality quality)
    {
        EnsureLoaded();

        _logger.LogDebug("GetTier called for quality: {Quality}", quality);

        if (_tiers.TryGetValue(quality, out var tier))
        {
            _logger.LogDebug(
                "GetTier: {Quality} -> StatMultiplier: {StatMult:F2}x, ValueMultiplier: {ValueMult:F1}x",
                quality,
                tier.Modifiers.StatMultiplier,
                tier.Modifiers.ValueMultiplier);

            return tier;
        }

        // This should not happen if defaults are loaded, but provide a fallback
        _logger.LogWarning(
            "Quality tier not found: {Quality} - returning Standard tier as fallback",
            quality);

        throw new KeyNotFoundException(
            $"No tier definition found for quality level: {quality}");
    }

    /// <inheritdoc />
    public bool TryGetTier(CraftedItemQuality quality, out QualityTierDefinition tier)
    {
        EnsureLoaded();

        _logger.LogDebug("TryGetTier called for quality: {Quality}", quality);

        var found = _tiers.TryGetValue(quality, out tier);

        _logger.LogDebug(
            "TryGetTier: {Quality} -> {Result}",
            quality,
            found ? "found" : "not found");

        return found;
    }

    /// <inheritdoc />
    public IReadOnlyList<QualityTierDefinition> GetAllTiers()
    {
        EnsureLoaded();

        _logger.LogDebug("GetAllTiers: returning {Count} tier definitions", _orderedTiers.Count);

        return _orderedTiers.AsReadOnly();
    }

    /// <inheritdoc />
    public QualityModifier GetModifiers(CraftedItemQuality quality)
    {
        EnsureLoaded();

        _logger.LogDebug("GetModifiers called for quality: {Quality}", quality);

        if (_tiers.TryGetValue(quality, out var tier))
        {
            _logger.LogDebug(
                "GetModifiers: {Quality} -> StatMultiplier: {StatMult:F2}x, ValueMultiplier: {ValueMult:F1}x",
                quality,
                tier.Modifiers.StatMultiplier,
                tier.Modifiers.ValueMultiplier);

            return tier.Modifiers;
        }

        // Return neutral modifiers as fallback
        _logger.LogWarning(
            "Quality tier not found for modifiers: {Quality} - returning neutral modifiers",
            quality);

        return QualityModifier.None;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Loading
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures tier definitions are loaded from configuration.
    /// </summary>
    /// <remarks>
    /// Thread-safe lazy loading using double-checked locking pattern.
    /// </remarks>
    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        lock (_loadLock)
        {
            if (_isLoaded)
            {
                return;
            }

            LoadTiers();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads tier definitions from the JSON configuration file.
    /// </summary>
    /// <remarks>
    /// If the configuration file is missing or invalid, default tier definitions
    /// are loaded to ensure the system remains functional.
    /// </remarks>
    private void LoadTiers()
    {
        _logger.LogInformation("Loading quality tier definitions from {Path}", _configPath);

        var loadedFromFile = false;

        // Check file exists
        if (File.Exists(_configPath))
        {
            try
            {
                // Read and parse JSON
                var json = File.ReadAllText(_configPath);
                _logger.LogDebug("Read {ByteCount} bytes from configuration file", json.Length);

                var config = JsonSerializer.Deserialize<QualityTierConfiguration>(
                    json,
                    GetJsonOptions());

                // Validate configuration
                if (config?.Tiers is not null && config.Tiers.Count > 0)
                {
                    _logger.LogDebug(
                        "Parsing {Count} tier entries from configuration",
                        config.Tiers.Count);

                    var loadedCount = 0;
                    var skippedCount = 0;

                    foreach (var tierData in config.Tiers)
                    {
                        try
                        {
                            var tier = CreateTierFromData(tierData);

                            if (tier.HasValue && ValidateTier(tier.Value))
                            {
                                AddTierToIndex(tier.Value);
                                loadedCount++;
                            }
                            else
                            {
                                skippedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Failed to load tier: {Quality}",
                                tierData.Quality);
                            skippedCount++;
                        }
                    }

                    _logger.LogInformation(
                        "Loaded {Loaded} quality tiers from configuration, skipped {Skipped}",
                        loadedCount,
                        skippedCount);

                    loadedFromFile = loadedCount > 0;
                }
                else
                {
                    _logger.LogWarning("No quality tiers found in configuration file");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to parse quality tier configuration: {Path}",
                    _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error loading quality tiers from: {Path}",
                    _configPath);
            }
        }
        else
        {
            _logger.LogWarning(
                "Quality tier configuration file not found: {Path}",
                _configPath);
        }

        // If no tiers were loaded from file, use defaults
        if (!loadedFromFile || _tiers.Count == 0)
        {
            _logger.LogInformation("Loading default quality tier definitions");
            LoadDefaultTiers();
        }

        // Ensure ordering is correct
        SortTiers();

        // Log summary
        LogLoadingSummary();
    }

    /// <summary>
    /// Creates a QualityTierDefinition from configuration data.
    /// </summary>
    /// <param name="data">The tier data from JSON.</param>
    /// <returns>The created tier definition, or null if creation failed.</returns>
    private QualityTierDefinition? CreateTierFromData(TierData data)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(data.Quality))
        {
            _logger.LogWarning("Tier entry missing quality value - skipping");
            return null;
        }

        // Parse quality enum
        if (!Enum.TryParse<CraftedItemQuality>(data.Quality, ignoreCase: true, out var quality))
        {
            _logger.LogWarning(
                "Invalid quality value '{Quality}' in tier configuration - skipping",
                data.Quality);
            return null;
        }

        // Use display name from config or default to enum name
        var displayName = !string.IsNullOrWhiteSpace(data.DisplayName)
            ? data.DisplayName
            : quality.GetDisplayName();

        // Use color code from config or default
        var colorCode = !string.IsNullOrWhiteSpace(data.ColorCode)
            ? data.ColorCode
            : quality.GetColorCode();

        // Validate multipliers - must be positive
        var statMultiplier = data.StatMultiplier > 0 ? data.StatMultiplier : 1.0m;
        var valueMultiplier = data.ValueMultiplier > 0 ? data.ValueMultiplier : 1.0m;

        if (data.StatMultiplier <= 0)
        {
            _logger.LogWarning(
                "Tier {Quality} has invalid stat multiplier {Value} - defaulting to 1.0",
                quality,
                data.StatMultiplier);
        }

        if (data.ValueMultiplier <= 0)
        {
            _logger.LogWarning(
                "Tier {Quality} has invalid value multiplier {Value} - defaulting to 1.0",
                quality,
                data.ValueMultiplier);
        }

        // Validate minimum margin - must be non-negative
        var minimumMargin = data.MinimumMargin >= 0 ? data.MinimumMargin : 0;

        if (data.MinimumMargin < 0)
        {
            _logger.LogWarning(
                "Tier {Quality} has negative minimum margin {Value} - defaulting to 0",
                quality,
                data.MinimumMargin);
        }

        // Create the tier definition
        return new QualityTierDefinition
        {
            Quality = quality,
            DisplayName = displayName,
            ColorCode = colorCode,
            Modifiers = QualityModifier.Create(statMultiplier, valueMultiplier),
            MinimumMargin = minimumMargin,
            RequiresNatural20 = data.RequiresNatural20
        };
    }

    /// <summary>
    /// Validates a tier definition.
    /// </summary>
    /// <param name="tier">The tier to validate.</param>
    /// <returns>True if the tier is valid, false otherwise.</returns>
    private bool ValidateTier(QualityTierDefinition tier)
    {
        // Check for duplicates
        if (_tiers.ContainsKey(tier.Quality))
        {
            _logger.LogWarning(
                "Duplicate quality tier: {Quality} - using first definition",
                tier.Quality);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Adds a tier to the lookup index.
    /// </summary>
    /// <param name="tier">The tier to index.</param>
    private void AddTierToIndex(QualityTierDefinition tier)
    {
        _tiers[tier.Quality] = tier;
        _orderedTiers.Add(tier);

        _logger.LogDebug(
            "Loaded tier: {Quality} - StatMultiplier: {StatMult:F2}x, ValueMultiplier: {ValueMult:F1}x, " +
            "MinMargin: {MinMargin}, RequiresNat20: {RequiresNat20}",
            tier.Quality,
            tier.Modifiers.StatMultiplier,
            tier.Modifiers.ValueMultiplier,
            tier.MinimumMargin,
            tier.RequiresNatural20);
    }

    /// <summary>
    /// Loads default tier definitions.
    /// </summary>
    /// <remarks>
    /// Default tiers are used when the configuration file is missing or invalid.
    /// </remarks>
    private void LoadDefaultTiers()
    {
        _logger.LogDebug("Loading default tier definitions");

        // Clear any partial loads
        _tiers.Clear();
        _orderedTiers.Clear();

        // Add default tiers using factory methods
        AddTierToIndex(QualityTierDefinition.CreateStandard());
        AddTierToIndex(QualityTierDefinition.CreateFine());
        AddTierToIndex(QualityTierDefinition.CreateMasterwork());
        AddTierToIndex(QualityTierDefinition.CreateLegendary());

        _logger.LogInformation("Loaded {Count} default quality tier definitions", _tiers.Count);
    }

    /// <summary>
    /// Sorts the ordered tier list by quality level.
    /// </summary>
    private void SortTiers()
    {
        _orderedTiers.Sort((a, b) => a.Quality.CompareTo(b.Quality));

        _logger.LogDebug(
            "Sorted tiers: {TierOrder}",
            string.Join(", ", _orderedTiers.Select(t => t.Quality)));
    }

    /// <summary>
    /// Logs a summary of loaded tier definitions.
    /// </summary>
    private void LogLoadingSummary()
    {
        foreach (var tier in _orderedTiers)
        {
            _logger.LogDebug(
                "Tier summary - {Quality}: Color={Color}, StatMult={StatMult:F2}x, " +
                "ValueMult={ValueMult:F1}x, MinMargin={MinMargin}, Nat20={Nat20}",
                tier.Quality,
                tier.ColorCode,
                tier.Modifiers.StatMultiplier,
                tier.Modifiers.ValueMultiplier,
                tier.MinimumMargin,
                tier.RequiresNatural20);
        }

        _logger.LogInformation(
            "Quality tier provider initialized with {Count} tier definitions",
            _tiers.Count);
    }

    /// <summary>
    /// Gets JSON serializer options.
    /// </summary>
    /// <returns>Configured JsonSerializerOptions.</returns>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Internal DTOs
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for deserializing quality-tiers.json.
    /// </summary>
    private sealed class QualityTierConfiguration
    {
        /// <summary>
        /// Gets or sets the list of tier definitions.
        /// </summary>
        public List<TierData> Tiers { get; set; } = [];
    }

    /// <summary>
    /// DTO for a single tier entry in configuration.
    /// </summary>
    private sealed class TierData
    {
        /// <summary>
        /// Gets or sets the quality level name (e.g., "Standard", "Fine").
        /// </summary>
        public string Quality { get; set; } = null!;

        /// <summary>
        /// Gets or sets the display name for UI presentation.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the hex color code (e.g., "#FFFFFF").
        /// </summary>
        public string? ColorCode { get; set; }

        /// <summary>
        /// Gets or sets the stat multiplier (e.g., 1.25 for +25%).
        /// </summary>
        public decimal StatMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the value multiplier (e.g., 2.5 for +150%).
        /// </summary>
        public decimal ValueMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the minimum margin required to achieve this tier.
        /// </summary>
        public int MinimumMargin { get; set; }

        /// <summary>
        /// Gets or sets whether this tier requires a natural 20 roll.
        /// </summary>
        public bool RequiresNatural20 { get; set; }
    }
}
