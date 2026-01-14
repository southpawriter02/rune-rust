using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for managing in-game time and weather.
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Gets the current game time.
    /// </summary>
    GameTime CurrentTime { get; }

    /// <summary>
    /// Gets the current weather.
    /// </summary>
    Weather CurrentWeather { get; }

    /// <summary>
    /// Advances time by one turn.
    /// </summary>
    /// <returns>True if the hour changed.</returns>
    bool AdvanceTurn();

    /// <summary>
    /// Advances time by specified hours.
    /// </summary>
    /// <param name="hours">Number of hours to advance.</param>
    void AdvanceHours(int hours);

    /// <summary>
    /// Gets the formatted time display string.
    /// </summary>
    /// <returns>Display string like "Day 3, 2:00 PM (Afternoon)".</returns>
    string GetTimeDisplay();

    /// <summary>
    /// Gets the formatted time and weather display.
    /// </summary>
    /// <returns>Combined time and weather display.</returns>
    string GetTimeAndWeatherDisplay();

    /// <summary>
    /// Sets the weather to a new type.
    /// </summary>
    /// <param name="weather">The new weather.</param>
    void SetWeather(Weather weather);

    /// <summary>
    /// Generates random weather based on region patterns.
    /// </summary>
    /// <param name="region">The region name for weather patterns.</param>
    void GenerateWeather(string region = "default");
}
