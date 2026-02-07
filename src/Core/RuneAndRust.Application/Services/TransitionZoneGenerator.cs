using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Generates transition zones between adjacent realms with interpolated environmental properties.
/// </summary>
/// <remarks>
/// <para>
/// TransitionZoneGenerator creates <see cref="TransitionZone"/> instances by:
/// <list type="number">
/// <item>Checking realm compatibility via <see cref="IBiomeAdjacencyService"/></item>
/// <item>Loading realm properties via <see cref="IRealmBiomeProvider"/></item>
/// <item>Interpolating all six environmental properties linearly</item>
/// <item>Attaching the transition theme narrative from the adjacency configuration</item>
/// </list>
/// </para>
/// <para>
/// All transition zone generation is logged at structured levels:
/// <list type="bullet">
/// <item>DEBUG — generation attempts, interpolation calculations</item>
/// <item>INFORMATION — successfully generated transitions</item>
/// <item>WARNING — incompatible or same-realm transition attempts</item>
/// </list>
/// </para>
/// </remarks>
public class TransitionZoneGenerator : ITransitionZoneGenerator
{
    private readonly IBiomeAdjacencyService _adjacencyService;
    private readonly IRealmBiomeProvider _biomeProvider;
    private readonly ILogger<TransitionZoneGenerator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionZoneGenerator"/> class.
    /// </summary>
    /// <param name="adjacencyService">Service for checking realm compatibility and transition requirements.</param>
    /// <param name="biomeProvider">Provider for loading realm biome definitions and properties.</param>
    /// <param name="logger">Logger for transition zone generation operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="adjacencyService"/> or <paramref name="biomeProvider"/> is null.
    /// </exception>
    public TransitionZoneGenerator(
        IBiomeAdjacencyService adjacencyService,
        IRealmBiomeProvider biomeProvider,
        ILogger<TransitionZoneGenerator>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(adjacencyService);
        ArgumentNullException.ThrowIfNull(biomeProvider);

        _adjacencyService = adjacencyService;
        _biomeProvider = biomeProvider;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TransitionZoneGenerator>.Instance;

        _logger.LogDebug("TransitionZoneGenerator initialized with {AdjacencyService} and {BiomeProvider}",
            adjacencyService.GetType().Name, biomeProvider.GetType().Name);
    }

    /// <inheritdoc/>
    public TransitionZone? GenerateTransition(RealmId fromRealm, RealmId toRealm)
    {
        _logger.LogDebug("Generating transition from {FromRealm} to {ToRealm}", fromRealm, toRealm);

        // Same realm — no transition needed
        if (fromRealm == toRealm)
        {
            _logger.LogWarning("Cannot generate transition for same realm {Realm}", fromRealm);
            return null;
        }

        // Check compatibility
        var compatibility = _adjacencyService.GetCompatibility(fromRealm, toRealm);
        if (compatibility == BiomeCompatibility.Incompatible)
        {
            _logger.LogWarning("Transition not possible: {FromRealm} → {ToRealm} (incompatible)",
                fromRealm, toRealm);
            return null;
        }

        // Load realm properties
        var fromBiome = _biomeProvider.GetBiome(fromRealm);
        var toBiome = _biomeProvider.GetBiome(toRealm);

        if (fromBiome == null || toBiome == null)
        {
            _logger.LogWarning("Cannot generate transition: biome definition missing for {FromRealm} or {ToRealm}",
                fromRealm, toRealm);
            return null;
        }

        // Interpolate at 50% blend for single-zone transition
        var interpolatedProperties = InterpolateProperties(fromBiome.BaseProperties, toBiome.BaseProperties, 0.5);

        // Get transition theme from adjacency configuration
        var theme = _adjacencyService.GetTransitionTheme(fromRealm, toRealm);

        var zone = new TransitionZone
        {
            FromRealm = fromRealm,
            ToRealm = toRealm,
            SequenceIndex = 0,
            BlendFactor = 0.5,
            InterpolatedProperties = interpolatedProperties,
            TransitionTheme = theme
        };

        _logger.LogInformation(
            "Transition generated: {FromRealm} → {ToRealm} at {BlendFactor:P0} blend " +
            "(temperature: {FromTemp}°C → {InterpolatedTemp}°C → {ToTemp}°C)",
            fromRealm, toRealm, 0.5,
            fromBiome.BaseProperties.TemperatureCelsius,
            interpolatedProperties.TemperatureCelsius,
            toBiome.BaseProperties.TemperatureCelsius);

        return zone;
    }

    /// <inheritdoc/>
    public IReadOnlyList<TransitionZone> GenerateTransitionSequence(RealmId from, RealmId to, int roomCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(roomCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(roomCount, 3);

        _logger.LogDebug("Generating {RoomCount}-zone transition sequence from {From} to {To}",
            roomCount, from, to);

        // Same realm or incompatible — return empty
        if (!CanGenerateTransition(from, to))
        {
            _logger.LogWarning(
                "Cannot generate transition sequence: {From} → {To} " +
                "(same realm or incompatible)",
                from, to);
            return [];
        }

        // Load realm properties
        var fromBiome = _biomeProvider.GetBiome(from);
        var toBiome = _biomeProvider.GetBiome(to);

        if (fromBiome == null || toBiome == null)
        {
            _logger.LogWarning(
                "Cannot generate transition sequence: biome definition missing for {From} or {To}",
                from, to);
            return [];
        }

        // Get transition theme
        var theme = _adjacencyService.GetTransitionTheme(from, to);

        // Calculate evenly distributed blend factors
        // 1 room: 0.50
        // 2 rooms: 0.33, 0.67
        // 3 rooms: 0.25, 0.50, 0.75
        var zones = new List<TransitionZone>(roomCount);

        for (var i = 0; i < roomCount; i++)
        {
            var blendFactor = (double)(i + 1) / (roomCount + 1);
            var interpolated = InterpolateProperties(fromBiome.BaseProperties, toBiome.BaseProperties, blendFactor);

            var zone = new TransitionZone
            {
                FromRealm = from,
                ToRealm = to,
                SequenceIndex = i,
                BlendFactor = blendFactor,
                InterpolatedProperties = interpolated,
                TransitionTheme = theme
            };

            zones.Add(zone);

            _logger.LogDebug(
                "Generated transition zone {Index}/{Total}: {From} → {To} at {BlendFactor:P0} blend " +
                "(temperature: {Temp}°C, humidity: {Humidity}%)",
                i + 1, roomCount, from, to, blendFactor,
                interpolated.TemperatureCelsius, interpolated.HumidityPercent);
        }

        _logger.LogInformation(
            "Transition sequence generated: {From} → {To} with {Count} zones",
            from, to, roomCount);

        return zones;
    }

    /// <inheritdoc/>
    public RealmBiomeProperties InterpolateProperties(RealmBiomeProperties from, RealmBiomeProperties to, double blend)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(blend, 0.0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(blend, 1.0);

        // Linear interpolation: result = from + (to - from) * blend
        var temperature = (int)Math.Round(from.TemperatureCelsius + (to.TemperatureCelsius - from.TemperatureCelsius) * blend);
        var aethericIntensity = (float)(from.AethericIntensity + (to.AethericIntensity - from.AethericIntensity) * blend);
        var humidity = (int)Math.Round(from.HumidityPercent + (to.HumidityPercent - from.HumidityPercent) * blend);
        var lightLevel = (float)(from.LightLevel + (to.LightLevel - from.LightLevel) * blend);
        var scaleFactor = (float)(from.ScaleFactor + (to.ScaleFactor - from.ScaleFactor) * blend);
        var corrosionRate = (float)(from.CorrosionRate + (to.CorrosionRate - from.CorrosionRate) * blend);

        _logger.LogDebug(
            "Interpolated properties at {Blend:P0}: " +
            "temperature {FromTemp}°C → {ResultTemp}°C, " +
            "aetheric {FromAetheric:F2} → {ResultAetheric:F2}, " +
            "humidity {FromHumidity}% → {ResultHumidity}%",
            blend,
            from.TemperatureCelsius, temperature,
            from.AethericIntensity, aethericIntensity,
            from.HumidityPercent, humidity);

        return new RealmBiomeProperties
        {
            TemperatureCelsius = temperature,
            AethericIntensity = Math.Clamp(aethericIntensity, 0f, 1f),
            HumidityPercent = Math.Clamp(humidity, 0, 100),
            LightLevel = Math.Clamp(lightLevel, 0f, 1f),
            ScaleFactor = Math.Max(scaleFactor, 0.1f),
            CorrosionRate = Math.Clamp(corrosionRate, 0f, 1f)
        };
    }

    /// <inheritdoc/>
    public bool CanGenerateTransition(RealmId from, RealmId to)
    {
        // Same realm — no transition needed
        if (from == to)
        {
            _logger.LogDebug("CanGenerateTransition: same realm {Realm}, returning false", from);
            return false;
        }

        var compatibility = _adjacencyService.GetCompatibility(from, to);
        var canGenerate = compatibility != BiomeCompatibility.Incompatible;

        _logger.LogDebug(
            "CanGenerateTransition: {From} → {To} = {CanGenerate} (compatibility: {Compatibility})",
            from, to, canGenerate, compatibility);

        return canGenerate;
    }
}
