using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.22.2: Weather Effect Service
/// Handles dynamic weather conditions that affect combat.
/// Weather effects are more temporary and mobile than ambient conditions.
///
/// Responsibilities:
/// - Managing weather effects in rooms/sectors
/// - Applying weather modifiers to combat
/// - Weather damage over time
/// - Hazard amplification from weather
/// - Weather state advancement and dissipation
/// </summary>
public class WeatherEffectService
{
    private static readonly ILogger _log = Log.ForContext<WeatherEffectService>();
    private readonly DiceService _diceService;
    private readonly TraumaEconomyService _traumaService;

    // In-memory storage for active weather effects
    private readonly Dictionary<int, WeatherEffect> _activeWeather = new();
    private int _nextWeatherId = 1;

    public WeatherEffectService(DiceService diceService, TraumaEconomyService traumaService)
    {
        _diceService = diceService;
        _traumaService = traumaService;
    }

    #region Weather Management

    /// <summary>
    /// Creates a new weather effect for a room
    /// </summary>
    public int CreateWeather(int? roomId, WeatherType weatherType, WeatherIntensity intensity,
        int? durationTurns = null)
    {
        var weather = new WeatherEffect
        {
            WeatherId = _nextWeatherId++,
            RoomId = roomId,
            WeatherType = weatherType,
            Intensity = intensity,
            DurationTurns = durationTurns,
            TurnsRemaining = durationTurns ?? 0,
            IsActive = true
        };

        // Apply default effects based on weather type and intensity
        ApplyWeatherDefaults(weather);

        _activeWeather[weather.WeatherId] = weather;

        _log.Information("Weather effect created: {WeatherId} - {WeatherType} ({Intensity}) in Room {RoomId}, Duration: {Duration}",
            weather.WeatherId, weatherType, intensity, roomId, durationTurns?.ToString() ?? "Permanent");

        return weather.WeatherId;
    }

    /// <summary>
    /// Applies default effects based on weather type and intensity
    /// </summary>
    private void ApplyWeatherDefaults(WeatherEffect weather)
    {
        // Base multiplier for intensity
        float intensityMultiplier = weather.Intensity switch
        {
            WeatherIntensity.Low => 0.5f,
            WeatherIntensity.Moderate => 1.0f,
            WeatherIntensity.High => 1.5f,
            WeatherIntensity.Extreme => 2.0f,
            _ => 1.0f
        };

        switch (weather.WeatherType)
        {
            case WeatherType.RealityStorm:
                weather.Name = "Reality Storm";
                weather.Description = "Corrupted Aether manifests as a localized reality distortion";
                weather.Icon = "🌪️";
                weather.AccuracyModifier = (int)(-2 * intensityMultiplier);
                weather.StressPerTurn = (int)(3 * intensityMultiplier);
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.High ? "2d6" : null;
                weather.DamageType = "Psychic";
                break;

            case WeatherType.StaticDischarge:
                weather.Name = "Static Discharge";
                weather.Description = "Electromagnetic chaos from decaying power systems";
                weather.Icon = "⚡";
                weather.AmplifiesHazards = true;
                weather.HazardAmplificationMultiplier = 1.5f * intensityMultiplier;
                weather.AffectedHazardTypes.Add(DynamicHazardType.LivePowerConduit);
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.High ? "3d6" : null;
                weather.DamageType = "Lightning";
                break;

            case WeatherType.CorrosionCloud:
                weather.Name = "Corrosion Cloud";
                weather.Description = "Chemical decay atmosphere eats through metal and flesh";
                weather.Icon = "☁️";
                weather.AcceleratesDegradation = true;
                weather.DegradationMultiplier = (int)(2 * intensityMultiplier);
                weather.StatusEffectApplied = "[Corroded]";
                weather.StatusEffectChance = 0.15f * intensityMultiplier;
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.Moderate ? "2d6" : null;
                weather.DamageType = "Poison";
                break;

            case WeatherType.PsychicResonanceStorm:
                weather.Name = "Psychic Resonance Storm";
                weather.Description = "Trauma echoes manifest as psychic pressure waves";
                weather.Icon = "🧠";
                weather.StressPerTurn = (int)(5 * intensityMultiplier);
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.Moderate ? "3d6" : null;
                weather.DamageType = "Psychic";
                weather.StatusEffectApplied = "[Disoriented]";
                weather.StatusEffectChance = 0.2f * intensityMultiplier;
                break;

            case WeatherType.ToxicFog:
                weather.Name = "Toxic Fog";
                weather.Description = "Industrial chemical dispersal blankets the area";
                weather.Icon = "🌫️";
                weather.AccuracyModifier = (int)(-1 * intensityMultiplier);
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.Moderate ? "2d6" : "1d6";
                weather.DamageType = "Poison";
                weather.StatusEffectApplied = "[Poisoned]";
                weather.StatusEffectChance = 0.25f * intensityMultiplier;
                break;

            case WeatherType.RadiationPulse:
                weather.Name = "Radiation Pulse";
                weather.Description = "Nuclear system leakage floods the area with radiation";
                weather.Icon = "☢️";
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.Low ? "3d6" : "2d6";
                weather.DamageType = "Radiation";
                weather.AcceleratesDegradation = true;
                weather.DegradationMultiplier = (int)(1.5f * intensityMultiplier);
                break;

            case WeatherType.SystemGlitch:
                weather.Name = "System Glitch";
                weather.Description = "Reality instability causes unpredictable effects";
                weather.Icon = "⚠️";
                weather.AccuracyModifier = (int)(-1 * intensityMultiplier);
                weather.MovementCostModifier = (int)(1 * intensityMultiplier);
                break;

            case WeatherType.VoidIncursion:
                weather.Name = "Void Incursion";
                weather.Description = "Blight manifestation corrupts local reality";
                weather.Icon = "🕳️";
                weather.StressPerTurn = (int)(4 * intensityMultiplier);
                weather.DamageFormula = weather.Intensity >= WeatherIntensity.High ? "4d6" : "2d6";
                weather.DamageType = "Corruption";
                break;

            case WeatherType.DataStorm:
                weather.Name = "Data Storm";
                weather.Description = "Information cascade overwhelms Jötun-Readers";
                weather.Icon = "📡";
                weather.StressPerTurn = (int)(3 * intensityMultiplier);
                weather.AccuracyModifier = (int)(-1 * intensityMultiplier);
                break;

            case WeatherType.TemporalDistortion:
                weather.Name = "Temporal Distortion";
                weather.Description = "Chronological malfunction disrupts action timing";
                weather.Icon = "⏰";
                weather.MovementCostModifier = (int)(2 * intensityMultiplier);
                weather.AccuracyModifier = (int)(-2 * intensityMultiplier);
                break;
        }
    }

    /// <summary>
    /// Gets current weather for a room
    /// </summary>
    public WeatherEffect? GetCurrentWeather(int roomId)
    {
        return _activeWeather.Values
            .FirstOrDefault(w => w.RoomId == roomId && w.IsActive && !w.IsSuppressed);
    }

    /// <summary>
    /// Suppresses weather temporarily
    /// </summary>
    public bool SuppressWeather(int weatherId, int duration)
    {
        if (!_activeWeather.TryGetValue(weatherId, out var weather))
        {
            return false;
        }

        weather.IsSuppressed = true;

        _log.Information("Weather effect suppressed: {WeatherId} - {Name} for {Duration} turns",
            weatherId, weather.Name, duration);

        // TODO: Add suppression duration tracking
        return true;
    }

    #endregion

    #region Effect Application

    /// <summary>
    /// Applies weather effects to a character
    /// </summary>
    public HazardResult ApplyWeatherEffects(PlayerCharacter character, WeatherEffect weather)
    {
        if (!weather.IsActive || weather.IsSuppressed)
        {
            return new HazardResult { WasTriggered = false };
        }

        var result = new HazardResult
        {
            WasTriggered = true,
            AffectedCharacters = new List<int> { character.CharacterId }
        };

        var logMessages = new List<string>();
        int totalDamage = 0;

        // Apply stress
        if (weather.StressPerTurn > 0)
        {
            _traumaService.AddStress(character, weather.StressPerTurn);
            logMessages.Add($"{weather.Icon} {weather.Name}: +{weather.StressPerTurn} Psychic Stress");
        }

        // Apply damage
        if (!string.IsNullOrEmpty(weather.DamageFormula))
        {
            var damage = ParseAndRollDamage(weather.DamageFormula);
            totalDamage += damage;
            character.HP = Math.Max(0, character.HP - damage);
            logMessages.Add($"{weather.Icon} {weather.Name}: {damage} {weather.DamageType} damage");
        }

        // Apply status effects (chance-based)
        if (!string.IsNullOrEmpty(weather.StatusEffectApplied) && weather.StatusEffectChance > 0)
        {
            var random = new Random();
            if (random.NextDouble() < weather.StatusEffectChance)
            {
                result.StatusEffectApplied = weather.StatusEffectApplied;
                logMessages.Add($"{weather.Icon} {weather.Name}: Applied {weather.StatusEffectApplied}");
            }
        }

        result.DamageDealt = totalDamage;
        result.LogMessage = string.Join("\n", logMessages);

        _log.Information("Weather effects applied to character {CharacterId}: {Effects}",
            character.CharacterId, string.Join(", ", logMessages));

        return result;
    }

    /// <summary>
    /// Parses damage formula and rolls dice (e.g., "2d6" -> rolls 2d6)
    /// </summary>
    private int ParseAndRollDamage(string formula)
    {
        // Simple parser for "XdY" format
        var parts = formula.Split('d');
        if (parts.Length == 2 && int.TryParse(parts[0], out int numDice))
        {
            return _diceService.RollDamage(numDice);
        }
        return 0;
    }

    /// <summary>
    /// Gets hazard amplification multiplier from active weather
    /// </summary>
    public float GetHazardAmplification(int roomId, DynamicHazardType hazardType)
    {
        var weather = GetCurrentWeather(roomId);
        if (weather == null || !weather.AmplifiesHazards)
        {
            return 1.0f;
        }

        if (weather.AffectedHazardTypes.Count == 0 || weather.AffectedHazardTypes.Contains(hazardType))
        {
            return weather.HazardAmplificationMultiplier;
        }

        return 1.0f;
    }

    #endregion

    #region Turn Management

    /// <summary>
    /// Advances weather state by one turn
    /// </summary>
    public bool AdvanceWeather(int weatherId)
    {
        if (!_activeWeather.TryGetValue(weatherId, out var weather))
        {
            return false;
        }

        bool stillActive = weather.AdvanceTurn();

        if (!stillActive)
        {
            _log.Information("Weather effect dissipated: {WeatherId} - {Name}", weatherId, weather.Name);
        }

        return stillActive;
    }

    /// <summary>
    /// Advances all active weather effects
    /// </summary>
    public void AdvanceAllWeather()
    {
        var dissipatedWeather = new List<int>();

        foreach (var kvp in _activeWeather)
        {
            if (!AdvanceWeather(kvp.Key))
            {
                dissipatedWeather.Add(kvp.Key);
            }
        }

        // Clean up dissipated weather
        foreach (var weatherId in dissipatedWeather)
        {
            _activeWeather.Remove(weatherId);
        }
    }

    #endregion
}
