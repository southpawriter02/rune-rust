// ═══════════════════════════════════════════════════════════════════════════════
// AchievementProvider.cs
// JSON-based provider for achievement definitions with lazy loading.
// Version: 0.12.1a
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;
using RuneAndRust.Infrastructure.Configuration;

namespace RuneAndRust.Infrastructure.Providers;

/// <summary>
/// Provides achievement definitions loaded from JSON configuration.
/// </summary>
/// <remarks>
/// <para>
/// AchievementProvider loads achievement definitions from a JSON configuration file
/// and provides cached access to the loaded definitions with indexed lookups
/// by achievement ID, category, and tier.
/// </para>
/// <para>
/// Definitions are loaded lazily on first access, allowing the application to
/// start quickly while deferring the loading cost until achievements are needed.
/// </para>
/// <para>
/// If the configuration file is missing or invalid, an empty achievement list
/// is used with appropriate warning logs. The system remains functional but
/// without any achievements defined.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var provider = serviceProvider.GetRequiredService&lt;IAchievementProvider&gt;();
///
/// // Get all achievements
/// var allAchievements = provider.GetAllAchievements();
///
/// // Get a specific achievement
/// var achievement = provider.GetAchievementById("first-blood");
///
/// // Filter by category
/// var combatAchievements = provider.GetAchievementsByCategory(AchievementCategory.Combat);
/// </code>
/// </example>
public sealed class AchievementProvider : IAchievementProvider
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Fields
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Primary index: achievement ID -> AchievementDefinition.
    /// </summary>
    private readonly Dictionary<string, AchievementDefinition> _achievementsById = new(StringComparer.Ordinal);

    /// <summary>
    /// List of all achievement definitions.
    /// </summary>
    private List<AchievementDefinition>? _achievements;

    /// <summary>
    /// Configuration options containing the file path.
    /// </summary>
    private readonly AchievementOptions _options;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<AchievementProvider> _logger;

    /// <summary>
    /// Flag indicating whether achievement definitions have been loaded.
    /// </summary>
    private bool _isLoaded;

    /// <summary>
    /// Cached maximum possible points value.
    /// </summary>
    private int? _maxPoints;

    /// <summary>
    /// Lock object for thread-safe lazy loading.
    /// </summary>
    private readonly object _loadLock = new();

    /// <summary>
    /// JSON serializer options for parsing configuration.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    // ═══════════════════════════════════════════════════════════════════════════
    // Constructor
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementProvider"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="options">Configuration options containing the file path.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> or <paramref name="options"/> is null.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Achievement definitions are not loaded until first access via any of the
    /// query methods. This allows the application to start quickly while deferring
    /// the loading cost until achievements are needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typically created via DI
    /// services.AddSingleton&lt;IAchievementProvider, AchievementProvider&gt;();
    /// </code>
    /// </example>
    public AchievementProvider(
        ILogger<AchievementProvider> logger,
        IOptions<AchievementOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _logger.LogDebug(
            "AchievementProvider created with configuration path: {ConfigPath}",
            _options.ConfigurationPath);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // IAchievementProvider Implementation
    // ═══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<AchievementDefinition> GetAllAchievements()
    {
        EnsureLoaded();

        _logger.LogDebug("GetAllAchievements: returning {Count} achievements", _achievements!.Count);

        return _achievements!.AsReadOnly();
    }

    /// <inheritdoc />
    public AchievementDefinition? GetAchievementById(string achievementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(achievementId, nameof(achievementId));

        EnsureLoaded();

        var found = _achievementsById.TryGetValue(achievementId, out var achievement);

        _logger.LogDebug(
            "GetAchievementById: {Id} -> {Result}",
            achievementId,
            found ? achievement!.Name : "not found");

        return achievement;
    }

    /// <inheritdoc />
    public IReadOnlyList<AchievementDefinition> GetAchievementsByCategory(AchievementCategory category)
    {
        EnsureLoaded();

        var filtered = _achievements!
            .Where(a => a.Category == category)
            .ToList();

        _logger.LogDebug(
            "GetAchievementsByCategory: {Category} -> {Count} achievements",
            category,
            filtered.Count);

        return filtered.AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<AchievementDefinition> GetAchievementsByTier(AchievementTier tier)
    {
        EnsureLoaded();

        var filtered = _achievements!
            .Where(a => a.Tier == tier)
            .ToList();

        _logger.LogDebug(
            "GetAchievementsByTier: {Tier} -> {Count} achievements",
            tier,
            filtered.Count);

        return filtered.AsReadOnly();
    }

    /// <inheritdoc />
    public int GetAchievementCount()
    {
        EnsureLoaded();

        _logger.LogDebug("GetAchievementCount: {Count}", _achievements!.Count);

        return _achievements!.Count;
    }

    /// <inheritdoc />
    public int GetMaxPossiblePoints()
    {
        EnsureLoaded();

        // Cache the max points calculation
        _maxPoints ??= _achievements!.Sum(a => a.Points);

        _logger.LogDebug("GetMaxPossiblePoints: {Points}", _maxPoints.Value);

        return _maxPoints.Value;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Loading
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ensures achievement definitions are loaded from configuration.
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

            LoadAchievements();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Loads achievement definitions from the JSON configuration file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the configuration file is missing or invalid, an empty achievement list
    /// is used to ensure the system remains functional.
    /// </para>
    /// </remarks>
    private void LoadAchievements()
    {
        var filePath = _options.ConfigurationPath;

        _logger.LogInformation("Loading achievements from {FilePath}", filePath);

        // Initialize empty list as default
        _achievements = new List<AchievementDefinition>();

        // Check file exists
        if (!File.Exists(filePath))
        {
            _logger.LogWarning(
                "Achievements file not found at {FilePath}, using empty list",
                filePath);
            return;
        }

        try
        {
            // Read and parse JSON
            var json = File.ReadAllText(filePath);

            _logger.LogDebug("Read {ByteCount} bytes from configuration file", json.Length);

            var data = JsonSerializer.Deserialize<AchievementConfigData>(json, JsonOptions);

            // Validate configuration
            if (data?.Achievements is null || data.Achievements.Count == 0)
            {
                _logger.LogWarning("No achievements found in configuration");
                return;
            }

            _logger.LogDebug(
                "Parsing {Count} achievement entries from configuration",
                data.Achievements.Count);

            var loadedCount = 0;
            var skippedCount = 0;

            // Process each achievement entry
            foreach (var entry in data.Achievements)
            {
                var definition = CreateDefinitionFromConfig(entry);

                if (definition is not null)
                {
                    _achievements.Add(definition);
                    _achievementsById[definition.AchievementId] = definition;
                    loadedCount++;

                    _logger.LogDebug(
                        "Loaded achievement: {AchievementId} ({Category}, {Tier})",
                        definition.AchievementId,
                        definition.Category,
                        definition.Tier);
                }
                else
                {
                    skippedCount++;
                }
            }

            _logger.LogInformation(
                "Loaded {Count} achievements ({Points} max points)",
                _achievements.Count,
                _achievements.Sum(a => a.Points));

            if (skippedCount > 0)
            {
                _logger.LogWarning(
                    "Skipped {SkippedCount} invalid achievement entries",
                    skippedCount);
            }

            // Validate loaded achievements
            ValidateAchievements();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse achievements configuration");
            _achievements = new List<AchievementDefinition>();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error loading achievements from: {FilePath}",
                filePath);
            _achievements = new List<AchievementDefinition>();
        }
    }

    /// <summary>
    /// Creates an AchievementDefinition from configuration data.
    /// </summary>
    /// <param name="entry">The configuration entry to convert.</param>
    /// <returns>The created AchievementDefinition, or null if creation failed.</returns>
    private AchievementDefinition? CreateDefinitionFromConfig(AchievementConfigEntry entry)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.Name))
        {
            _logger.LogWarning("Skipping invalid achievement entry: missing id or name");
            return null;
        }

        // Parse category enum
        if (!Enum.TryParse<AchievementCategory>(entry.Category, ignoreCase: true, out var category))
        {
            _logger.LogWarning(
                "Invalid category '{Category}' for achievement {Id}, defaulting to Combat",
                entry.Category,
                entry.Id);
            category = AchievementCategory.Combat;
        }

        // Parse tier enum
        if (!Enum.TryParse<AchievementTier>(entry.Tier, ignoreCase: true, out var tier))
        {
            _logger.LogWarning(
                "Invalid tier '{Tier}' for achievement {Id}, defaulting to Bronze",
                entry.Tier,
                entry.Id);
            tier = AchievementTier.Bronze;
        }

        // Parse conditions
        var conditions = entry.Conditions?
            .Select(CreateConditionFromConfig)
            .Where(c => c is not null)
            .Select(c => c!)
            .ToList() ?? new List<AchievementCondition>();

        // Create the achievement definition
        try
        {
            return AchievementDefinition.Create(
                achievementId: entry.Id,
                name: entry.Name,
                description: entry.Description ?? string.Empty,
                category: category,
                tier: tier,
                conditions: conditions,
                isSecret: entry.IsSecret,
                iconPath: entry.IconPath);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to create achievement {Id}: {Message}",
                entry.Id,
                ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Creates an AchievementCondition from configuration data.
    /// </summary>
    /// <param name="entry">The condition configuration entry.</param>
    /// <returns>The created AchievementCondition, or null if creation failed.</returns>
    private AchievementCondition? CreateConditionFromConfig(ConditionConfigEntry entry)
    {
        // Validate statistic name
        if (string.IsNullOrWhiteSpace(entry.Stat))
        {
            _logger.LogWarning("Skipping condition with empty stat name");
            return null;
        }

        // Parse operator
        var op = entry.Operator switch
        {
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "==" => ComparisonOperator.Equals,
            "=" => ComparisonOperator.Equals,
            _ => ComparisonOperator.GreaterThanOrEqual
        };

        // Warn about unrecognized operators
        if (entry.Operator != ">=" && entry.Operator != "<=" && entry.Operator != "==" && entry.Operator != "=")
        {
            _logger.LogWarning(
                "Unknown operator '{Operator}' for stat {Stat}, defaulting to >=",
                entry.Operator,
                entry.Stat);
        }

        _logger.LogDebug(
            "Parsed condition: {Stat} {Op} {Value}",
            entry.Stat,
            op,
            entry.Value);

        return new AchievementCondition(entry.Stat, op, entry.Value);
    }

    /// <summary>
    /// Validates loaded achievements for consistency.
    /// </summary>
    /// <remarks>
    /// Logs warnings for issues like duplicate IDs or achievements without conditions.
    /// </remarks>
    private void ValidateAchievements()
    {
        // Check for duplicate IDs
        var duplicates = _achievements!
            .GroupBy(a => a.AchievementId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var dup in duplicates)
        {
            _logger.LogWarning(
                "Duplicate achievement ID found: {AchievementId}",
                dup);
        }

        // Check for achievements without conditions
        var noConditions = _achievements!
            .Where(a => a.Conditions.Count == 0)
            .ToList();

        foreach (var achievement in noConditions)
        {
            _logger.LogWarning(
                "Achievement {AchievementId} has no unlock conditions",
                achievement.AchievementId);
        }

        // Log category distribution
        var categoryStats = _achievements!
            .GroupBy(a => a.Category)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        _logger.LogDebug(
            "Achievement category distribution: {Distribution}",
            string.Join(", ", categoryStats));

        // Log tier distribution
        var tierStats = _achievements!
            .GroupBy(a => a.Tier)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        _logger.LogDebug(
            "Achievement tier distribution: {Distribution}",
            string.Join(", ", tierStats));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Internal DTOs
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Root DTO for deserializing achievements.json.
    /// </summary>
    private sealed class AchievementConfigData
    {
        /// <summary>
        /// Gets or sets the list of achievement entries.
        /// </summary>
        public List<AchievementConfigEntry>? Achievements { get; set; }
    }

    /// <summary>
    /// DTO for a single achievement entry in configuration.
    /// </summary>
    private sealed class AchievementConfigEntry
    {
        /// <summary>
        /// Gets or sets the unique identifier for the achievement.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Category { get; set; } = "Combat";

        /// <summary>
        /// Gets or sets the tier name.
        /// </summary>
        public string Tier { get; set; } = "Bronze";

        /// <summary>
        /// Gets or sets the list of unlock conditions.
        /// </summary>
        public List<ConditionConfigEntry>? Conditions { get; set; }

        /// <summary>
        /// Gets or sets whether this is a secret achievement.
        /// </summary>
        public bool IsSecret { get; set; }

        /// <summary>
        /// Gets or sets the optional icon path.
        /// </summary>
        public string? IconPath { get; set; }
    }

    /// <summary>
    /// DTO for a condition entry in configuration.
    /// </summary>
    private sealed class ConditionConfigEntry
    {
        /// <summary>
        /// Gets or sets the statistic name to check.
        /// </summary>
        public string Stat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the comparison operator (>=, &lt;=, ==).
        /// </summary>
        public string Operator { get; set; } = ">=";

        /// <summary>
        /// Gets or sets the threshold value.
        /// </summary>
        public long Value { get; set; }
    }
}
