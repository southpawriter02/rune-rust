namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Weather conditions affecting outdoor areas.
/// </summary>
/// <remarks>
/// <para>
/// Weather affects:
/// <list type="bullet">
///   <item><description>Visibility modifiers for skill checks</description></item>
///   <item><description>Light level reduction (Fog, HeavyRain, Storm)</description></item>
///   <item><description>Outdoor room descriptions</description></item>
/// </list>
/// </para>
/// </remarks>
public enum WeatherType
{
    /// <summary>Clear skies, no visibility penalty.</summary>
    Clear,

    /// <summary>Overcast, no visibility penalty.</summary>
    Cloudy,

    /// <summary>Light rain, minor visibility penalty (-1).</summary>
    LightRain,

    /// <summary>Heavy rain, significant visibility penalty (-3), reduces light.</summary>
    HeavyRain,

    /// <summary>Dense fog, major visibility penalty (-4), reduces light.</summary>
    Fog,

    /// <summary>Violent storm, severe visibility penalty (-5), reduces light.</summary>
    Storm
}
