using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for managing in-game time and weather.
/// </summary>
/// <remarks>
/// <para>
/// Provides centralized time and weather management for the game:
/// <list type="bullet">
///   <item><description>Tracks game time (hour, day, time of day)</description></item>
///   <item><description>Manages weather conditions and transitions</description></item>
///   <item><description>Calculates outdoor light levels from time + weather</description></item>
///   <item><description>Supports region-based weather patterns</description></item>
/// </list>
/// </para>
/// </remarks>
public class TimeService : ITimeService
{
    // ========================================================================
    // Dependencies
    // ========================================================================

    private readonly ILogger<TimeService> _logger;
    private readonly Random _random;

    // ========================================================================
    // State
    // ========================================================================

    /// <inheritdoc />
    public GameTime CurrentTime { get; private set; }

    /// <inheritdoc />
    public Weather CurrentWeather { get; private set; }

    // ========================================================================
    // Constructor
    // ========================================================================

    /// <summary>
    /// Creates a new TimeService with default settings.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="startingHour">Starting hour (0-23). Default is 8 (morning).</param>
    /// <param name="startingDay">Starting day. Default is 1.</param>
    public TimeService(
        ILogger<TimeService>? logger = null,
        int startingHour = 8,
        int startingDay = 1)
    {
        _logger = logger ?? NullLogger<TimeService>.Instance;
        _random = new Random();

        // Initialize time and weather
        CurrentTime = GameTime.Create(startingHour, startingDay);
        CurrentWeather = Weather.Clear();

        _logger.LogDebug(
            "TimeService initialized: {Time}, Weather: {Weather}",
            CurrentTime, CurrentWeather);
    }

    // ========================================================================
    // Time Advancement
    // ========================================================================

    /// <inheritdoc />
    public bool AdvanceTurn()
    {
        var hourChanged = CurrentTime.AdvanceTurn();

        if (hourChanged)
        {
            _logger.LogDebug(
                "Time advanced to {Hour}:00 on Day {Day} ({TimeOfDay})",
                CurrentTime.Hour, CurrentTime.Day, CurrentTime.TimeOfDay);

            // Check if weather duration expired
            if (CurrentWeather.AdvanceHour())
            {
                _logger.LogInformation(
                    "Weather duration expired, changing to Clear");
                CurrentWeather = Weather.Clear();
            }
        }

        return hourChanged;
    }

    /// <inheritdoc />
    public void AdvanceHours(int hours)
    {
        if (hours <= 0)
            return;

        var previousDay = CurrentTime.Day;
        CurrentTime.AdvanceHours(hours);

        _logger.LogDebug(
            "Time advanced by {Hours} hours to {Time}",
            hours, CurrentTime);

        // Advance weather duration
        for (var i = 0; i < hours; i++)
        {
            if (CurrentWeather.AdvanceHour())
            {
                _logger.LogInformation("Weather duration expired");
                CurrentWeather = Weather.Clear();
                break;
            }
        }

        // Log day change
        if (CurrentTime.Day > previousDay)
        {
            _logger.LogInformation(
                "Day changed: Day {Day}",
                CurrentTime.Day);
        }
    }

    // ========================================================================
    // Display Methods
    // ========================================================================

    /// <inheritdoc />
    public string GetTimeDisplay() => CurrentTime.GetDisplayString();

    /// <inheritdoc />
    public string GetTimeAndWeatherDisplay()
    {
        var time = CurrentTime.GetDisplayString();
        var weather = CurrentWeather.GetDescription();
        return $"{time}\nWeather: {weather}";
    }

    // ========================================================================
    // Weather Management
    // ========================================================================

    /// <inheritdoc />
    public void SetWeather(Weather weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        var oldWeather = CurrentWeather.Type;
        CurrentWeather = weather;

        _logger.LogInformation(
            "Weather changed from {OldWeather} to {NewWeather}",
            oldWeather, weather.Type);
    }

    /// <inheritdoc />
    public void GenerateWeather(string region = "default")
    {
        // Default weather patterns (can be loaded from config in future)
        var (type, duration) = region.ToLowerInvariant() switch
        {
            "dungeon" => (WeatherType.Clear, -1), // Indoor - always clear
            "forest" => GenerateForestWeather(),
            "mountain" => GenerateMountainWeather(),
            _ => GenerateDefaultWeather()
        };

        var newWeather = Weather.Create(type, duration);
        SetWeather(newWeather);
    }

    /// <summary>
    /// Generates weather for forest regions.
    /// </summary>
    private (WeatherType type, int duration) GenerateForestWeather()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 40 => (WeatherType.Clear, _random.Next(4, 12)),
            < 65 => (WeatherType.Cloudy, _random.Next(2, 8)),
            < 85 => (WeatherType.LightRain, _random.Next(1, 4)),
            < 95 => (WeatherType.HeavyRain, _random.Next(1, 3)),
            _ => (WeatherType.Fog, _random.Next(2, 6))
        };
    }

    /// <summary>
    /// Generates weather for mountain regions.
    /// </summary>
    private (WeatherType type, int duration) GenerateMountainWeather()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 30 => (WeatherType.Clear, _random.Next(2, 8)),
            < 50 => (WeatherType.Cloudy, _random.Next(2, 6)),
            < 75 => (WeatherType.Storm, _random.Next(1, 4)),
            _ => (WeatherType.Fog, _random.Next(2, 8))
        };
    }

    /// <summary>
    /// Generates default weather patterns.
    /// </summary>
    private (WeatherType type, int duration) GenerateDefaultWeather()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 50 => (WeatherType.Clear, _random.Next(6, 12)),
            < 70 => (WeatherType.Cloudy, _random.Next(3, 8)),
            < 85 => (WeatherType.LightRain, _random.Next(2, 5)),
            < 95 => (WeatherType.HeavyRain, _random.Next(1, 3)),
            _ => (WeatherType.Fog, _random.Next(2, 4))
        };
    }
}
