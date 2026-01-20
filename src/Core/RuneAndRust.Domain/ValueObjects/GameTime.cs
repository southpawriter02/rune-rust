using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Tracks in-game time for day/night cycle and event scheduling.
/// </summary>
/// <remarks>
/// <para>
/// Time advances with player actions (turns). Default is 6 turns per hour.
/// </para>
/// <para>
/// Day/night affects outdoor room light levels:
/// <list type="bullet">
///   <item><description>Night (0:00-5:00, 20:00-23:59): Dark</description></item>
///   <item><description>Dawn/Dusk (5:00-7:00, 17:00-20:00): Dim</description></item>
///   <item><description>Day (7:00-17:00): Bright</description></item>
/// </list>
/// </para>
/// <para>
/// Weather can further reduce light levels in outdoor areas.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create game time starting at 8 AM on day 1
/// var time = GameTime.Create(hour: 8, day: 1);
/// 
/// // Advance by one turn (1/6th of an hour by default)
/// time.AdvanceTurn();
/// 
/// // Get outdoor light level
/// var light = time.GetOutdoorLightLevel(WeatherType.Clear);
/// </code>
/// </example>
public class GameTime
{
    // ========================================================================
    // Constants
    // ========================================================================

    /// <summary>
    /// Default number of turns that constitute one hour.
    /// </summary>
    public const int DefaultTurnsPerHour = 6;

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// Gets the current hour (0-23).
    /// </summary>
    public int Hour { get; private set; }

    /// <summary>
    /// Gets the current day (1+).
    /// </summary>
    public int Day { get; private set; } = 1;

    /// <summary>
    /// Gets the accumulated turn count within the current hour.
    /// </summary>
    /// <remarks>
    /// When TurnCount reaches TurnsPerHour, hour increments and TurnCount resets.
    /// </remarks>
    public int TurnCount { get; private set; }

    /// <summary>
    /// Gets the number of turns per hour for this game.
    /// </summary>
    public int TurnsPerHour { get; private set; } = DefaultTurnsPerHour;

    /// <summary>
    /// Gets whether it is currently daytime (6:00 - 18:00).
    /// </summary>
    public bool IsDaytime => Hour >= 6 && Hour < 18;

    /// <summary>
    /// Gets whether it is currently nighttime (18:00 - 6:00).
    /// </summary>
    public bool IsNighttime => !IsDaytime;

    /// <summary>
    /// Gets the current time of day descriptor.
    /// </summary>
    /// <remarks>
    /// Maps hour to TimeOfDay:
    /// <list type="bullet">
    ///   <item><description>0-5: Night</description></item>
    ///   <item><description>5-7: Dawn</description></item>
    ///   <item><description>7-12: Morning</description></item>
    ///   <item><description>12-14: Noon</description></item>
    ///   <item><description>14-17: Afternoon</description></item>
    ///   <item><description>17-20: Dusk</description></item>
    ///   <item><description>20-24: Evening</description></item>
    /// </list>
    /// </remarks>
    public TimeOfDay TimeOfDay => Hour switch
    {
        >= 5 and < 7 => TimeOfDay.Dawn,
        >= 7 and < 12 => TimeOfDay.Morning,
        >= 12 and < 14 => TimeOfDay.Noon,
        >= 14 and < 17 => TimeOfDay.Afternoon,
        >= 17 and < 20 => TimeOfDay.Dusk,
        >= 20 and < 24 => TimeOfDay.Evening,
        _ => TimeOfDay.Night
    };

    // ========================================================================
    // Constructor
    // ========================================================================

    /// <summary>
    /// Private constructor for factory pattern.
    /// </summary>
    private GameTime() { }

    // ========================================================================
    // Factory Methods
    // ========================================================================

    /// <summary>
    /// Creates a new GameTime instance.
    /// </summary>
    /// <param name="hour">Starting hour (0-23). Defaults to 8 (morning).</param>
    /// <param name="day">Starting day (1+). Defaults to 1.</param>
    /// <param name="turnsPerHour">Turns per hour. Defaults to 6.</param>
    /// <returns>A new GameTime instance.</returns>
    /// <example>
    /// <code>
    /// // Start at noon on day 3
    /// var time = GameTime.Create(hour: 12, day: 3);
    /// </code>
    /// </example>
    public static GameTime Create(int hour = 8, int day = 1, int turnsPerHour = DefaultTurnsPerHour)
    {
        return new GameTime
        {
            Hour = Math.Clamp(hour, 0, 23),
            Day = Math.Max(1, day),
            TurnCount = 0,
            TurnsPerHour = Math.Max(1, turnsPerHour)
        };
    }

    // ========================================================================
    // Time Advancement Methods
    // ========================================================================

    /// <summary>
    /// Advances time by one turn.
    /// </summary>
    /// <remarks>
    /// Increments TurnCount. When TurnCount reaches TurnsPerHour,
    /// the hour advances and TurnCount resets to 0.
    /// </remarks>
    /// <returns>True if the hour changed.</returns>
    public bool AdvanceTurn()
    {
        TurnCount++;

        if (TurnCount >= TurnsPerHour)
        {
            TurnCount = 0;
            AdvanceHours(1);
            return true; // Hour changed
        }

        return false; // Hour did not change
    }

    /// <summary>
    /// Advances time by a specified number of hours.
    /// </summary>
    /// <param name="hours">Number of hours to advance.</param>
    /// <remarks>
    /// Automatically handles day rollover when hour exceeds 23.
    /// </remarks>
    public void AdvanceHours(int hours)
    {
        if (hours <= 0)
            return;

        Hour += hours;

        // Handle day rollover
        while (Hour >= 24)
        {
            Hour -= 24;
            Day++;
        }
    }

    /// <summary>
    /// Sets time to a specific hour (for resting, time skips, etc.).
    /// </summary>
    /// <param name="hour">Target hour (0-23).</param>
    public void SetHour(int hour)
    {
        Hour = Math.Clamp(hour, 0, 23);
        TurnCount = 0;
    }

    // ========================================================================
    // Light Level Calculation
    // ========================================================================

    /// <summary>
    /// Gets the light level for outdoor areas based on time and weather.
    /// </summary>
    /// <param name="weather">Current weather type.</param>
    /// <returns>The effective light level for outdoor areas.</returns>
    /// <remarks>
    /// <para>
    /// Base light from time of day:
    /// <list type="bullet">
    ///   <item><description>Night (0-5, 20-24): Dark</description></item>
    ///   <item><description>Dawn/Dusk (5-7, 17-20): Dim</description></item>
    ///   <item><description>Day (7-17): Bright</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Weather modifiers (Fog, HeavyRain, Storm) reduce light by one level:
    /// Bright → Dim, Dim → Dark.
    /// </para>
    /// </remarks>
    public LightLevel GetOutdoorLightLevel(WeatherType weather = WeatherType.Clear)
    {
        // Determine base light from time of day
        var baseLight = TimeOfDay switch
        {
            TimeOfDay.Night => LightLevel.Dark,
            TimeOfDay.Evening => LightLevel.Dark,
            TimeOfDay.Dawn => LightLevel.Dim,
            TimeOfDay.Dusk => LightLevel.Dim,
            _ => LightLevel.Bright // Morning, Noon, Afternoon
        };

        // Weather can reduce light level
        var reducesLight = weather is WeatherType.Fog
                                   or WeatherType.HeavyRain
                                   or WeatherType.Storm;

        if (reducesLight)
        {
            return baseLight switch
            {
                LightLevel.Bright => LightLevel.Dim,
                LightLevel.Dim => LightLevel.Dark,
                _ => baseLight
            };
        }

        return baseLight;
    }

    // ========================================================================
    // Display Methods
    // ========================================================================

    /// <summary>
    /// Gets a formatted display string for the current time.
    /// </summary>
    /// <returns>A string like "Day 3, 2:00 PM (Afternoon)".</returns>
    public string GetDisplayString()
    {
        var period = Hour >= 12 ? "PM" : "AM";
        var displayHour = Hour switch
        {
            0 => 12,
            > 12 => Hour - 12,
            _ => Hour
        };

        return $"Day {Day}, {displayHour}:00 {period} ({TimeOfDay})";
    }

    /// <summary>
    /// Gets a short time string (e.g., "8:00 AM").
    /// </summary>
    /// <returns>Short time format.</returns>
    public string GetTimeString()
    {
        var period = Hour >= 12 ? "PM" : "AM";
        var displayHour = Hour switch
        {
            0 => 12,
            > 12 => Hour - 12,
            _ => Hour
        };

        return $"{displayHour}:00 {period}";
    }

    /// <inheritdoc />
    public override string ToString() => GetDisplayString();
}
