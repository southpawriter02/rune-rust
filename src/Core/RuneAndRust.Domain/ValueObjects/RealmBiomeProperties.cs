namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Encapsulates the baseline environmental properties of a realm biome.
/// </summary>
/// <remarks>
/// <para>
/// RealmBiomeProperties defines the physical characteristics of a realm that
/// affect gameplay mechanics, environmental hazards, and narrative descriptions.
/// </para>
/// <para>
/// Property Ranges:
/// <list type="bullet">
/// <item>Temperature: -273°C to +1000°C (absolute zero to magma)</item>
/// <item>Aetheric Intensity: 0.0 (mundane) to 1.0 (saturated)</item>
/// <item>Humidity: 0% (bone dry) to 100% (underwater)</item>
/// <item>Light Level: 0.0 (total darkness) to 1.0 (bright daylight)</item>
/// <item>Scale Factor: 0.1 (tiny) to 10.0 (giant)</item>
/// <item>Corrosion Rate: 0.0 (pristine) to 1.0 (rapid decay)</item>
/// </list>
/// </para>
/// </remarks>
public sealed record RealmBiomeProperties
{
    /// <summary>
    /// Temperature in Celsius.
    /// </summary>
    /// <remarks>
    /// Affects heat/cold hazards, equipment durability, and survival mechanics.
    /// Comfortable range: 10°C to 25°C.
    /// </remarks>
    public required int TemperatureCelsius { get; init; }

    /// <summary>
    /// Intensity of aetheric (magical) energy from 0.0 to 1.0.
    /// </summary>
    /// <remarks>
    /// High values (above 0.7) indicate CPS risk and reality instability.
    /// Low values (below 0.3) indicate stable, mundane areas.
    /// </remarks>
    public required float AethericIntensity { get; init; }

    /// <summary>
    /// Humidity percentage from 0 to 100.
    /// </summary>
    /// <remarks>
    /// Affects corrosion rates, fire propagation, and environmental descriptions.
    /// </remarks>
    public required int HumidityPercent { get; init; }

    /// <summary>
    /// Ambient light level from 0.0 (total darkness) to 1.0 (bright daylight).
    /// </summary>
    /// <remarks>
    /// Values below 0.2 require light sources for normal vision.
    /// Svartalfheim zones typically have 0.0 (Total Darkness).
    /// </remarks>
    public required float LightLevel { get; init; }

    /// <summary>
    /// Environmental scale factor where 1.0 is normal human scale.
    /// </summary>
    /// <remarks>
    /// Jotunheim has values of 3.0-10.0 (giant scale).
    /// Affects movement distances, fall damage, and creature sizes.
    /// </remarks>
    public required float ScaleFactor { get; init; }

    /// <summary>
    /// Rate of environmental corrosion from 0.0 to 1.0.
    /// </summary>
    /// <remarks>
    /// Affects equipment degradation and structural integrity.
    /// High values in Helheim (toxicity) and Muspelheim (heat damage).
    /// </remarks>
    public required float CorrosionRate { get; init; }

    /// <summary>
    /// Creates default temperate environment properties (Midgard standard).
    /// </summary>
    public static RealmBiomeProperties Temperate() => new()
    {
        TemperatureCelsius = 18,
        AethericIntensity = 0.3f,
        HumidityPercent = 60,
        LightLevel = 0.7f,
        ScaleFactor = 1.0f,
        CorrosionRate = 0.2f
    };

    /// <summary>
    /// Gets whether the temperature is in a thermal extreme (hot or cold hazard).
    /// </summary>
    public bool IsThermalExtreme => TemperatureCelsius < -10 || TemperatureCelsius > 45;

    /// <summary>
    /// Gets whether aetheric intensity is high enough to risk CPS exposure.
    /// </summary>
    public bool IsAethericallyActive => AethericIntensity > 0.6f;

    /// <summary>
    /// Gets whether the environment is dark enough to require light sources.
    /// </summary>
    public bool IsDark => LightLevel < 0.2f;

    /// <summary>
    /// Gets whether the environment is giant-scaled.
    /// </summary>
    public bool IsGiantScale => ScaleFactor > 2.0f;

    /// <summary>
    /// Gets a human-readable temperature description.
    /// </summary>
    public string TemperatureDescription => TemperatureCelsius switch
    {
        < -40 => "Lethal Cold",
        < -10 => "Extreme Cold",
        < 5 => "Cold",
        < 15 => "Cool",
        < 25 => "Temperate",
        < 35 => "Warm",
        < 50 => "Hot",
        < 100 => "Extreme Heat",
        _ => "Lethal Heat"
    };

    /// <summary>
    /// Gets a human-readable light level description.
    /// </summary>
    public string LightDescription => LightLevel switch
    {
        <= 0f => "Total Darkness",
        < 0.1f => "Near Darkness",
        < 0.3f => "Dim",
        < 0.6f => "Low Light",
        < 0.8f => "Normal",
        _ => "Bright"
    };
}
