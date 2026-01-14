using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Current weather conditions affecting outdoor areas.
/// </summary>
/// <remarks>
/// <para>
/// Weather affects outdoor gameplay:
/// <list type="bullet">
///   <item><description>Visibility modifiers for perception/skill checks</description></item>
///   <item><description>Light level reduction (Fog, HeavyRain, Storm)</description></item>
///   <item><description>Atmospheric room descriptions</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create foggy weather lasting 3 hours
/// var weather = Weather.Create(WeatherType.Fog, duration: 3);
/// 
/// // Get visibility penalty
/// int penalty = weather.VisibilityModifier; // -4
/// 
/// // Advance time and check if weather ended
/// bool ended = weather.AdvanceHour();
/// </code>
/// </example>
public class Weather
{
    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// Gets the current weather type.
    /// </summary>
    public WeatherType Type { get; private set; } = WeatherType.Clear;

    /// <summary>
    /// Gets the remaining duration in hours.
    /// </summary>
    /// <remarks>
    /// -1 indicates indefinite duration (weather persists until changed).
    /// </remarks>
    public int Duration { get; private set; } = -1;

    /// <summary>
    /// Gets the visibility modifier for skill checks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Visibility modifiers by weather type:
    /// <list type="bullet">
    ///   <item><description>Clear/Cloudy: 0 (no penalty)</description></item>
    ///   <item><description>LightRain: -1</description></item>
    ///   <item><description>HeavyRain: -3</description></item>
    ///   <item><description>Fog: -4</description></item>
    ///   <item><description>Storm: -5</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public int VisibilityModifier => Type switch
    {
        WeatherType.Clear => 0,
        WeatherType.Cloudy => 0,
        WeatherType.LightRain => -1,
        WeatherType.HeavyRain => -3,
        WeatherType.Fog => -4,
        WeatherType.Storm => -5,
        _ => 0
    };

    /// <summary>
    /// Gets whether this weather affects outdoor light levels.
    /// </summary>
    /// <remarks>
    /// Fog, HeavyRain, and Storm reduce light by one level
    /// (Bright → Dim, Dim → Dark).
    /// </remarks>
    public bool AffectsLight => Type is WeatherType.Fog
                                     or WeatherType.HeavyRain
                                     or WeatherType.Storm;

    /// <summary>
    /// Gets whether this weather has a limited duration.
    /// </summary>
    public bool HasDuration => Duration >= 0;

    /// <summary>
    /// Gets whether this weather is permanent (indefinite duration).
    /// </summary>
    public bool IsPermanent => Duration < 0;

    // ========================================================================
    // Constructor
    // ========================================================================

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private Weather() { }

    // ========================================================================
    // Factory Methods
    // ========================================================================

    /// <summary>
    /// Creates a new Weather instance.
    /// </summary>
    /// <param name="type">The weather type.</param>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>A new Weather instance.</returns>
    public static Weather Create(WeatherType type, int duration = -1)
    {
        return new Weather
        {
            Type = type,
            Duration = duration
        };
    }

    /// <summary>
    /// Creates clear weather (default state).
    /// </summary>
    /// <returns>Clear weather with indefinite duration.</returns>
    public static Weather Clear() => Create(WeatherType.Clear);

    /// <summary>
    /// Creates cloudy weather.
    /// </summary>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>Cloudy weather.</returns>
    public static Weather Cloudy(int duration = -1) => Create(WeatherType.Cloudy, duration);

    /// <summary>
    /// Creates light rain.
    /// </summary>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>Light rain weather.</returns>
    public static Weather LightRain(int duration = -1) => Create(WeatherType.LightRain, duration);

    /// <summary>
    /// Creates heavy rain.
    /// </summary>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>Heavy rain weather.</returns>
    public static Weather HeavyRain(int duration = -1) => Create(WeatherType.HeavyRain, duration);

    /// <summary>
    /// Creates fog.
    /// </summary>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>Foggy weather.</returns>
    public static Weather Fog(int duration = -1) => Create(WeatherType.Fog, duration);

    /// <summary>
    /// Creates a storm.
    /// </summary>
    /// <param name="duration">Duration in hours (-1 = indefinite).</param>
    /// <returns>Storm weather.</returns>
    public static Weather Storm(int duration = -1) => Create(WeatherType.Storm, duration);

    // ========================================================================
    // Duration Methods
    // ========================================================================

    /// <summary>
    /// Advances weather duration by one hour.
    /// </summary>
    /// <returns>True if the weather ended (duration reached 0).</returns>
    /// <remarks>
    /// If duration is -1 (indefinite), always returns false.
    /// </remarks>
    public bool AdvanceHour()
    {
        if (Duration < 0)
            return false; // Indefinite, never ends

        Duration--;
        return Duration <= 0;
    }

    /// <summary>
    /// Changes the weather type.
    /// </summary>
    /// <param name="newType">New weather type.</param>
    /// <param name="duration">New duration (-1 = indefinite).</param>
    public void Change(WeatherType newType, int duration = -1)
    {
        Type = newType;
        Duration = duration;
    }

    // ========================================================================
    // Display Methods
    // ========================================================================

    /// <summary>
    /// Gets a descriptive string for the current weather.
    /// </summary>
    /// <returns>A player-facing weather description.</returns>
    public string GetDescription() => Type switch
    {
        WeatherType.Clear => "Clear skies",
        WeatherType.Cloudy => "Overcast",
        WeatherType.LightRain => "Light rain falls",
        WeatherType.HeavyRain => "Heavy rain pours down",
        WeatherType.Fog => "Thick fog blankets the area",
        WeatherType.Storm => "A violent storm rages",
        _ => "Unknown weather"
    };

    /// <summary>
    /// Gets a short description for UI display.
    /// </summary>
    /// <returns>Short weather name.</returns>
    public string GetShortDescription() => Type switch
    {
        WeatherType.Clear => "Clear",
        WeatherType.Cloudy => "Cloudy",
        WeatherType.LightRain => "Light Rain",
        WeatherType.HeavyRain => "Heavy Rain",
        WeatherType.Fog => "Fog",
        WeatherType.Storm => "Storm",
        _ => "Unknown"
    };

    /// <inheritdoc />
    public override string ToString() => GetDescription();
}
