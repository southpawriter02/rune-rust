using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Configuration;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Context for sensory descriptor generation.
/// </summary>
public record SensoryContext
{
    /// <summary>
    /// The environment context from v0.0.11a.
    /// </summary>
    public EnvironmentContext Environment { get; init; }

    /// <summary>
    /// The current light source type (if known).
    /// </summary>
    public string? LightSource { get; init; }

    /// <summary>
    /// The current weather condition (if any).
    /// </summary>
    public string? Weather { get; init; }

    /// <summary>
    /// The current time of day (if applicable).
    /// </summary>
    public string? TimeOfDay { get; init; }

    /// <summary>
    /// Whether the location is indoors or outdoors.
    /// </summary>
    public bool IsIndoor { get; init; } = true;

    /// <summary>
    /// Whether the location has active combat.
    /// </summary>
    public bool InCombat { get; init; }

    /// <summary>
    /// Additional tags for filtering.
    /// </summary>
    public IReadOnlyList<string> AdditionalTags { get; init; } = [];

    /// <summary>
    /// Gets all effective tags including environment and additional.
    /// </summary>
    public IEnumerable<string> GetAllTags()
    {
        var tags = new List<string>();

        // Add environment-derived tags
        if (Environment.DerivedTags != null)
            tags.AddRange(Environment.DerivedTags);

        // Add biome as tag
        if (!string.IsNullOrEmpty(Environment.Biome))
            tags.Add(Environment.Biome);

        // Add climate as tag
        if (!string.IsNullOrEmpty(Environment.Climate))
            tags.Add(Environment.Climate);

        // Add indoor/outdoor tag
        tags.Add(IsIndoor ? "indoor" : "outdoor");

        // Add combat tag
        if (InCombat)
            tags.Add("combat");

        // Add additional tags
        tags.AddRange(AdditionalTags);

        return tags.Distinct();
    }
}

/// <summary>
/// Generates multi-sensory environmental descriptions.
/// </summary>
/// <remarks>
/// This service coordinates descriptor selection across all senses,
/// ensuring coherent atmospheric descriptions based on environment context.
/// </remarks>
public class SensoryDescriptorService
{
    private readonly DescriptorService _descriptorService;
    private readonly SensoryConfiguration _config;
    private readonly ILogger<SensoryDescriptorService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random = new();

    public SensoryDescriptorService(
        DescriptorService descriptorService,
        SensoryConfiguration config,
        ILogger<SensoryDescriptorService> logger,
        IGameEventLogger? eventLogger = null)
    {
        _descriptorService = descriptorService ?? throw new ArgumentNullException(nameof(descriptorService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;

        _logger.LogDebug(
            "SensoryDescriptorService initialized with {LightSources} light sources, {WeatherConditions} weather types",
            _config.LightSources.Count, _config.WeatherConditions.Count);
    }

    /// <summary>
    /// Generates a complete multi-sensory description.
    /// </summary>
    /// <param name="context">The sensory context.</param>
    /// <returns>A complete sensory description.</returns>
    public SensoryDescription GenerateSensoryDescription(SensoryContext context)
    {
        _logger.LogDebug(
            "Generating sensory description for biome={Biome}, climate={Climate}",
            context.Environment.Biome, context.Environment.Climate);

        var description = new SensoryDescription
        {
            Lighting = GetLightingDescription(context),
            Sounds = GetSoundDescription(context),
            Smell = GetSmellDescription(context),
            Temperature = GetTemperatureDescription(context),
            Weather = GetWeatherDescription(context),
            TimeOfDay = GetTimeOfDayDescription(context)
        };

        _eventLogger?.LogEnvironment("SensoryDescription", $"Generated sensory description for {context.Environment.Biome}",
            data: new Dictionary<string, object>
            {
                ["biome"] = context.Environment.Biome ?? "unknown",
                ["climate"] = context.Environment.Climate ?? "unknown",
                ["isIndoor"] = context.IsIndoor,
                ["inCombat"] = context.InCombat
            });

        return description;
    }

    /// <summary>
    /// Gets a lighting description based on context.
    /// </summary>
    public LightingDescription GetLightingDescription(SensoryContext context)
    {
        var lightingLevel = context.Environment.Lighting ?? "dim";

        // Determine light source
        var lightSource = context.LightSource ?? InferLightSource(context);

        // Get lighting pool based on source and level
        string poolPath;
        if (!string.IsNullOrEmpty(lightSource) &&
            _config.LightSources.TryGetValue(lightSource, out var sourceDef))
        {
            poolPath = sourceDef.DescriptorPool;
        }
        else
        {
            poolPath = $"lighting.lighting_{lightingLevel}";
        }

        // Use pool path for selection - don't filter by tags since pools are already specific
        var description = _descriptorService.GetDescriptor(poolPath);

        // Fall back to generic lighting pool
        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor($"lighting.lighting_{lightingLevel}");
        }

        // Final fallback to dim lighting
        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("lighting.lighting_dim");
        }

        return new LightingDescription(description ?? string.Empty, lightSource, lightingLevel);
    }

    /// <summary>
    /// Gets a layered sound description.
    /// </summary>
    public SoundDescription GetSoundDescription(SensoryContext context)
    {
        var biome = context.Environment.Biome ?? "dungeon";

        // Get distant sounds
        var distant = GetSoundLayer("distant", biome);

        // Get nearby sounds
        var nearby = GetSoundLayer("nearby", biome);

        // Get immediate sounds (more situational)
        string? immediate = null;
        if (context.InCombat)
        {
            immediate = _descriptorService.GetDescriptor("sounds.sounds_immediate_combat");
        }

        return new SoundDescription
        {
            Distant = distant,
            Nearby = nearby,
            Immediate = immediate
        };
    }

    /// <summary>
    /// Gets a smell description with intensity.
    /// </summary>
    public SmellDescription GetSmellDescription(SensoryContext context)
    {
        var biome = context.Environment.Biome ?? "dungeon";

        // Determine intensity based on biome characteristics
        var intensity = DetermineSmellIntensity(context);

        // Get biome-specific smell
        var poolPath = $"smells.smells_{biome}";
        var smellType = _descriptorService.GetDescriptor(poolPath);

        // Fall back to generic pool
        if (string.IsNullOrEmpty(smellType))
        {
            smellType = _descriptorService.GetDescriptor("smells.smells");
        }

        // Format with intensity template
        var description = FormatSmellWithIntensity(smellType, intensity);

        return new SmellDescription(description, intensity, ExtractSmellType(smellType));
    }

    /// <summary>
    /// Gets a temperature description based on climate.
    /// </summary>
    public string GetTemperatureDescription(SensoryContext context)
    {
        var climate = context.Environment.Climate ?? "temperate";

        // Get climate-specific temperature pool
        var poolPath = $"environmental.temperature_{climate}";
        var description = _descriptorService.GetDescriptor(poolPath);

        // Fall back to generic pool
        if (string.IsNullOrEmpty(description))
        {
            description = _descriptorService.GetDescriptor("environmental.temperature");
        }

        return description ?? string.Empty;
    }

    /// <summary>
    /// Gets a weather description if applicable.
    /// </summary>
    public string? GetWeatherDescription(SensoryContext context)
    {
        if (string.IsNullOrEmpty(context.Weather)) return null;

        if (!_config.WeatherConditions.TryGetValue(context.Weather, out var weatherDef))
        {
            _logger.LogDebug("Unknown weather condition: {Weather}", context.Weather);
            return null;
        }

        // Validate weather is appropriate for climate
        var climate = context.Environment.Climate ?? "temperate";
        if (weatherDef.ValidClimates.Count > 0 && !weatherDef.ValidClimates.Contains(climate))
        {
            _logger.LogDebug(
                "Weather {Weather} not valid for climate {Climate}",
                context.Weather, climate);
            return null;
        }

        var poolPath = context.IsIndoor ? weatherDef.IndoorPool : weatherDef.OutdoorPool;

        return _descriptorService.GetDescriptor(poolPath);
    }

    /// <summary>
    /// Gets a time of day description if applicable.
    /// </summary>
    public string? GetTimeOfDayDescription(SensoryContext context)
    {
        if (string.IsNullOrEmpty(context.TimeOfDay)) return null;
        if (context.IsIndoor) return null; // Time of day only affects outdoor

        if (!_config.TimesOfDay.TryGetValue(context.TimeOfDay, out var timeDef))
        {
            return null;
        }

        return _descriptorService.GetDescriptor(timeDef.OutdoorPool);
    }

    /// <summary>
    /// Generates a brief atmospheric summary using 2-3 senses.
    /// </summary>
    public string GenerateBriefAtmosphere(SensoryContext context)
    {
        var full = GenerateSensoryDescription(context);
        return full.ToNarrative(3);
    }

    /// <summary>
    /// Generates a detailed atmospheric description using all senses.
    /// </summary>
    public string GenerateDetailedAtmosphere(SensoryContext context)
    {
        var full = GenerateSensoryDescription(context);

        var parts = new List<string>();

        // Include all layers of sound
        if (full.Sounds.HasSounds)
        {
            parts.Add(full.Sounds.ToFullDescription());
        }

        // Add other senses
        if (!string.IsNullOrEmpty(full.Lighting.Description))
            parts.Add(full.Lighting.Description);

        if (!string.IsNullOrEmpty(full.Smell.Description))
            parts.Add(full.Smell.Description);

        if (!string.IsNullOrEmpty(full.Temperature))
            parts.Add(full.Temperature);

        if (!string.IsNullOrEmpty(full.Weather))
            parts.Add(full.Weather);

        if (!string.IsNullOrEmpty(full.TimeOfDay))
            parts.Add(full.TimeOfDay);

        return string.Join(" ", parts);
    }

    private string? InferLightSource(SensoryContext context)
    {
        var biome = context.Environment.Biome;

        // Infer common light sources by biome
        return biome switch
        {
            "cave" => _random.NextDouble() > 0.5 ? "bioluminescence" : null,
            "dungeon" => "torch",
            "volcanic" => "lava_glow",
            "frozen" => context.IsIndoor ? "crystal" : null,
            "ruins" => _random.NextDouble() > 0.7 ? "magical" : null,
            "forest" => context.IsIndoor ? null : "sunlight",
            _ => null
        };
    }

    private string? GetSoundLayer(string layer, string biome)
    {
        // Try biome-specific layer pool
        var poolPath = $"sounds.sounds_{layer}_{biome}";
        var sound = _descriptorService.GetDescriptor(poolPath);

        // Fall back to generic layer pool
        if (string.IsNullOrEmpty(sound))
        {
            poolPath = $"sounds.sounds_{layer}";
            sound = _descriptorService.GetDescriptor(poolPath);
        }

        return sound;
    }

    private SmellIntensity DetermineSmellIntensity(SensoryContext context)
    {
        // Strong smells in certain biomes
        return context.Environment.Biome switch
        {
            "swamp" => SmellIntensity.Strong,
            "volcanic" => SmellIntensity.Strong,
            "frozen" => SmellIntensity.Faint, // Cold suppresses smell
            _ => SmellIntensity.Noticeable
        };
    }

    private string FormatSmellWithIntensity(string? smellType, SmellIntensity intensity)
    {
        if (string.IsNullOrEmpty(smellType)) return string.Empty;

        // If the smell already has intensity words, return as-is
        if (smellType.Contains("assaults") || smellType.Contains("fills") ||
            smellType.Contains("hint") || smellType.Contains("reek"))
        {
            return smellType;
        }

        // Otherwise, wrap with intensity template
        return intensity switch
        {
            SmellIntensity.Faint => $"A faint hint of {smellType} lingers in the air.",
            SmellIntensity.Noticeable => smellType,
            SmellIntensity.Strong => $"The pungent {smellType} fills your nostrils.",
            SmellIntensity.Overwhelming => $"The overwhelming {smellType} is nearly unbearable.",
            _ => smellType
        };
    }

    private string? ExtractSmellType(string? description)
    {
        // Simple extraction - could be enhanced
        return description?.Split(' ').LastOrDefault();
    }
}
