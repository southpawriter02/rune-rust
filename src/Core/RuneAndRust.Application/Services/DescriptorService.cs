using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for generating varied text descriptions using weighted descriptor pools.
/// </summary>
public class DescriptorService
{
    private readonly IReadOnlyDictionary<string, DescriptorPool> _pools;
    private readonly ThemeConfiguration _theme;
    private readonly ILogger<DescriptorService> _logger;
    private readonly EnvironmentCoherenceService? _coherenceService;
    private readonly Random _random = new();

    // Track recently used descriptors to avoid repetition
    private readonly Dictionary<string, Queue<string>> _recentlyUsed = new();
    private const int RecentHistorySize = 3;

    public DescriptorService(
        IReadOnlyDictionary<string, DescriptorPool> pools,
        ThemeConfiguration theme,
        ILogger<DescriptorService> logger,
        EnvironmentCoherenceService? coherenceService = null)
    {
        _pools = pools ?? throw new ArgumentNullException(nameof(pools));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _coherenceService = coherenceService;
        _logger.LogDebug("DescriptorService initialized with {PoolCount} pools", pools.Count);
    }

    /// <summary>
    /// Gets a single descriptor from a pool.
    /// </summary>
    /// <param name="poolPath">The pool path (e.g., "environmental.lighting").</param>
    /// <param name="tags">Optional tags to filter descriptors.</param>
    /// <param name="context">Optional context for selection.</param>
    /// <returns>The selected descriptor text.</returns>
    public string GetDescriptor(string poolPath, IEnumerable<string>? tags = null, DescriptorContext? context = null)
    {
        // Check for theme override
        ThemePreset? preset = null;
        var actualPath = poolPath;
        
        if (_theme.Themes.TryGetValue(_theme.ActiveTheme, out preset) &&
            preset.DescriptorOverrides.TryGetValue(poolPath, out var overridePath))
        {
            actualPath = overridePath;
        }

        if (!_pools.TryGetValue(actualPath, out var pool))
        {
            _logger.LogWarning("Descriptor pool not found: {PoolPath}", actualPath);
            return string.Empty;
        }

        var candidates = pool.Descriptors.AsEnumerable();

        // Filter by tags
        if (tags?.Any() == true)
        {
            var tagList = tags.ToList();
            candidates = candidates.Where(d => d.Tags.Intersect(tagList).Any());
        }

        // Filter by theme
        var activeTheme = _theme.ActiveTheme;
        candidates = candidates.Where(d => d.Themes.Count == 0 || d.Themes.Contains(activeTheme));

        // Filter by context constraints
        if (context != null)
        {
            candidates = ApplyContextFilters(candidates, context);
        }

        // Apply theme exclusions
        if (preset != null)
        {
            candidates = candidates.Where(d =>
                !preset.ExcludedTerms.Any(term => d.Text.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var candidateList = candidates.ToList();

        if (candidateList.Count == 0)
        {
            _logger.LogDebug("No matching descriptors in pool: {PoolPath}", actualPath);
            return string.Empty;
        }

        // Avoid recently used
        var descriptor = SelectWeightedAvoidingRecent(poolPath, candidateList, preset);

        TrackUsage(poolPath, descriptor.Id);

        _logger.LogDebug("Selected descriptor: {Id} from pool {Pool}", descriptor.Id, poolPath);

        return descriptor.Text;
    }

    /// <summary>
    /// Generates a combined description from multiple pools using a template.
    /// </summary>
    /// <param name="template">Template string with placeholders (e.g., "The room is {lighting}. {sounds}").</param>
    /// <param name="tags">Tags to filter all pools.</param>
    /// <param name="context">Context for all selections.</param>
    /// <returns>The combined description.</returns>
    public string GenerateCombined(string template, IEnumerable<string>? tags = null, DescriptorContext? context = null)
    {
        var tagList = tags?.ToList();

        return System.Text.RegularExpressions.Regex.Replace(template, @"\{([^}]+)\}", match =>
        {
            var poolPath = match.Groups[1].Value;
            return GetDescriptor(poolPath, tagList, context);
        });
    }

    /// <summary>
    /// Gets a room description combining multiple environmental elements.
    /// </summary>
    /// <param name="roomTags">Tags describing the room (e.g., "dungeon", "cave").</param>
    /// <returns>A combined atmospheric description.</returns>
    public string GenerateRoomAtmosphere(IEnumerable<string> roomTags)
    {
        var tags = roomTags.ToList();

        var lighting = GetDescriptor("environmental.lighting", tags);
        var sound = GetDescriptor("environmental.sounds", tags);
        var smell = GetDescriptor("environmental.smells", tags);
        var temp = GetDescriptor("environmental.temperature", tags);

        // Combine 2-3 elements for variety
        var elements = new[] { lighting, sound, smell, temp }
            .Where(e => !string.IsNullOrEmpty(e))
            .OrderBy(_ => _random.Next())
            .Take(_random.Next(2, 4))
            .ToList();

        if (elements.Count == 0) return string.Empty;

        return string.Join(" ", elements);
    }

    /// <summary>
    /// Gets a combat hit description based on damage dealt.
    /// </summary>
    /// <param name="damage">The damage amount dealt.</param>
    /// <param name="targetMaxHealth">The target's maximum health.</param>
    /// <returns>A descriptive hit message.</returns>
    public string GetCombatHitDescription(int damage, int targetMaxHealth)
    {
        var damagePercent = (double)damage / targetMaxHealth;

        var context = new DescriptorContext { DamagePercent = damagePercent };

        var hit = GetDescriptor("combat.hit_descriptions", context: context);
        var severity = GetDescriptor("combat.damage_severity", context: context);

        if (string.IsNullOrEmpty(hit) && string.IsNullOrEmpty(severity))
        {
            return "hits";
        }

        return $"{hit}, {severity}".Trim(' ', ',');
    }

    /// <summary>
    /// Gets an NPC appearance description combining multiple features.
    /// </summary>
    /// <returns>A complete NPC appearance description.</returns>
    public string GenerateNpcAppearance()
    {
        var physical = GetDescriptor("npcs.physical_features");
        var clothing = GetDescriptor("npcs.clothing");
        var demeanor = GetDescriptor("npcs.demeanor");

        return $"Before you stands a figure with {physical}, {clothing}, {demeanor}.";
    }

    // ===== Environment-Aware Methods (v0.0.11a) =====

    /// <summary>
    /// Gets a descriptor with environment context awareness.
    /// </summary>
    /// <param name="poolPath">The pool path (e.g., "environmental.lighting").</param>
    /// <param name="context">Context including environment for selection.</param>
    /// <returns>The selected descriptor text.</returns>
    public string GetDescriptorWithEnvironment(string poolPath, DescriptorContext context)
    {
        // Apply biome pool overrides if environment is set
        var actualPath = poolPath;
        if (context.Environment.HasValue && _coherenceService != null)
        {
            var biome = _coherenceService.GetBiome(context.Environment.Value.Biome ?? "");
            if (biome?.DescriptorPoolOverrides.TryGetValue(poolPath, out var overridePath) == true)
            {
                // Only use override if the pool exists
                if (_pools.ContainsKey(overridePath))
                {
                    actualPath = overridePath;
                    _logger.LogDebug(
                        "Pool override for biome {Biome}: {Original} -> {Override}",
                        biome.Id, poolPath, overridePath);
                }
            }
        }

        // Use effective tags including environment-derived tags
        var tags = context.GetEffectiveTags().ToList();

        return GetDescriptor(actualPath, tags, context);
    }

    /// <summary>
    /// Generates room atmosphere using environment context.
    /// </summary>
    /// <param name="environment">The room's environment context.</param>
    /// <returns>A coherent atmospheric description.</returns>
    public string GenerateRoomAtmosphereWithEnvironment(EnvironmentContext environment)
    {
        var context = new DescriptorContext
        {
            Environment = environment,
            IncludeEnvironmentTags = true
        };

        var lighting = GetDescriptorWithEnvironment("environmental.lighting", context);
        var sound = GetDescriptorWithEnvironment("environmental.sounds", context);
        var smell = GetDescriptorWithEnvironment("environmental.smells", context);
        var temp = GetDescriptorWithEnvironment("environmental.temperature", context);

        // Combine 2-3 elements for variety, ensuring coherence
        var elements = new[] { lighting, sound, smell, temp }
            .Where(e => !string.IsNullOrEmpty(e))
            .OrderBy(_ => _random.Next())
            .Take(_random.Next(2, 4))
            .ToList();

        if (elements.Count == 0) return string.Empty;

        _logger.LogDebug(
            "Generated atmosphere for {Biome}: {ElementCount} elements",
            environment.Biome, elements.Count);

        return string.Join(" ", elements);
    }

    private static IEnumerable<Descriptor> ApplyContextFilters(IEnumerable<Descriptor> candidates, DescriptorContext context)
    {
        if (context.DamagePercent.HasValue)
        {
            candidates = candidates.Where(d =>
                (!d.MinDamagePercent.HasValue || context.DamagePercent >= d.MinDamagePercent) &&
                (!d.MaxDamagePercent.HasValue || context.DamagePercent < d.MaxDamagePercent));
        }

        return candidates;
    }

    private Descriptor SelectWeightedAvoidingRecent(string poolPath, List<Descriptor> candidates, ThemePreset? preset)
    {
        // Get recently used IDs for this pool
        var recentIds = _recentlyUsed.TryGetValue(poolPath, out var recent)
            ? recent.ToHashSet()
            : new HashSet<string>();

        // Prefer non-recent, but fall back if all are recent
        var preferred = candidates.Where(d => !recentIds.Contains(d.Id)).ToList();
        var pool = preferred.Count > 0 ? preferred : candidates;

        // Apply theme emphasis
        if (preset != null && preset.EmphasizedTerms.Count > 0)
        {
            foreach (var desc in pool)
            {
                if (preset.EmphasizedTerms.Any(term =>
                    desc.Text.Contains(term, StringComparison.OrdinalIgnoreCase)))
                {
                    // Temporarily boost weight
                    desc.EffectiveWeight = (int)(desc.Weight * 1.5);
                }
                else
                {
                    desc.EffectiveWeight = desc.Weight;
                }
            }
        }

        // Weighted random selection
        var totalWeight = pool.Sum(d => d.EffectiveWeight ?? d.Weight);
        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var descriptor in pool)
        {
            cumulative += descriptor.EffectiveWeight ?? descriptor.Weight;
            if (roll < cumulative)
                return descriptor;
        }

        return pool[0];
    }

    private void TrackUsage(string poolPath, string descriptorId)
    {
        if (!_recentlyUsed.TryGetValue(poolPath, out var recent))
        {
            recent = new Queue<string>();
            _recentlyUsed[poolPath] = recent;
        }

        recent.Enqueue(descriptorId);

        while (recent.Count > RecentHistorySize)
        {
            recent.Dequeue();
        }
    }
}

/// <summary>
/// Context information for descriptor selection.
/// </summary>
public class DescriptorContext
{
    /// <summary>
    /// Damage percentage for combat descriptors (0.0 to 1.0).
    /// </summary>
    public double? DamagePercent { get; set; }

    /// <summary>
    /// Health percentage for condition-based descriptors (0.0 to 1.0).
    /// </summary>
    public double? HealthPercent { get; set; }

    /// <summary>
    /// Currently active theme ID.
    /// </summary>
    public string? ActiveTheme { get; set; }

    /// <summary>
    /// Explicit tags to filter descriptors.
    /// </summary>
    public IReadOnlyList<string> Tags { get; set; } = [];

    // ===== v0.0.11a additions =====

    /// <summary>
    /// Environment context for coherent descriptor selection.
    /// </summary>
    public EnvironmentContext? Environment { get; set; }

    /// <summary>
    /// Whether to include derived tags from environment context.
    /// Defaults to true.
    /// </summary>
    public bool IncludeEnvironmentTags { get; set; } = true;

    /// <summary>
    /// Gets all effective tags including environment-derived tags.
    /// </summary>
    public IEnumerable<string> GetEffectiveTags()
    {
        if (IncludeEnvironmentTags && Environment.HasValue)
        {
            return Tags.Concat(Environment.Value.DerivedTags).Distinct();
        }
        return Tags;
    }
}
