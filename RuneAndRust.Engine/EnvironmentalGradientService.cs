using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.39.2: Service for applying environmental gradients to rooms
/// Calculates and applies temperature, Aetheric intensity, and scale gradients
/// </summary>
public class EnvironmentalGradientService
{
    private static readonly ILogger _log = Log.ForContext<EnvironmentalGradientService>();

    public EnvironmentalGradientService()
    {
        _log.Information("EnvironmentalGradientService initialized");
    }

    /// <summary>
    /// Applies all relevant environmental gradients to a room
    /// </summary>
    public void ApplyGradients(Room room, string fromBiome, string toBiome, float blendRatio)
    {
        _log.Debug("Applying gradients to room {RoomId}: {FromBiome} ({FromWeight:P0}) → {ToBiome} ({ToWeight:P0})",
            room.RoomId, fromBiome, 1 - blendRatio, toBiome, blendRatio);

        // Apply temperature gradient for thermal transitions
        if (IsTemperatureTransition(fromBiome, toBiome))
        {
            ApplyTemperatureGradient(room, fromBiome, toBiome, blendRatio);
        }

        // Apply Aetheric gradient for Alfheim transitions
        if (IsAethericTransition(fromBiome, toBiome))
        {
            ApplyAethericGradient(room, fromBiome, toBiome, blendRatio);
        }

        // Apply scale gradient for Jötunheim transitions
        if (IsScaleTransition(fromBiome, toBiome))
        {
            ApplyScaleGradient(room, fromBiome, toBiome, blendRatio);
        }
    }

    #region Temperature Gradient

    /// <summary>
    /// Applies temperature gradient for thermal transitions (Muspelheim ↔ Niflheim)
    /// </summary>
    private void ApplyTemperatureGradient(Room room, string fromBiome, string toBiome, float blendRatio)
    {
        var gradient = CalculateTemperatureGradient(fromBiome, toBiome, blendRatio);

        room.SetEnvironmentalProperty("Temperature", gradient.Temperature);
        room.Description += $" {gradient.Description}";

        _log.Debug("Applied temperature gradient: {Temperature}°C - {Description}",
            gradient.Temperature, gradient.Description);
    }

    /// <summary>
    /// Calculates temperature gradient based on biome blend
    /// </summary>
    public TemperatureGradient CalculateTemperatureGradient(string fromBiome, string toBiome, float blendRatio)
    {
        var fromTemp = GetBiomeBaseTemperature(fromBiome);
        var toTemp = GetBiomeBaseTemperature(toBiome);

        // Linear interpolation between temperatures
        var temperature = fromTemp + (toTemp - fromTemp) * blendRatio;

        var description = temperature switch
        {
            > 200 => "Scorching heat radiates from every surface. Prolonged exposure is lethal.",
            > 100 => "Intense heat makes breathing difficult. Sweat evaporates instantly.",
            > 40 => "Uncomfortably warm. The air shimmers with thermal distortion.",
            > 15 => "Moderate temperature. Neither hot nor cold.",
            > -10 => "Cool but tolerable. Breath mists in the air.",
            > -30 => "Frigid cold penetrates clothing. Extremities numb.",
            _ => "Extreme sub-zero temperatures. Instant frostbite risk."
        };

        return new TemperatureGradient
        {
            Temperature = temperature,
            Description = description
        };
    }

    private float GetBiomeBaseTemperature(string biome)
    {
        return biome switch
        {
            "Muspelheim" => 300f,      // Volcanic heat
            "Niflheim" => -50f,        // Extreme cold
            "TheRoots" => 20f,         // Ambient industrial
            "NeutralZone" => 20f,      // Moderate
            "Jotunheim" => 10f,        // Cool (large thermal mass)
            "Alfheim" => 15f,          // Aetheric ambient
            _ => 20f                   // Default moderate
        };
    }

    private bool IsTemperatureTransition(string fromBiome, string toBiome)
    {
        var thermalBiomes = new[] { "Muspelheim", "Niflheim" };
        return thermalBiomes.Contains(fromBiome) || thermalBiomes.Contains(toBiome);
    }

    #endregion

    #region Aetheric Gradient

    /// <summary>
    /// Applies Aetheric intensity gradient for Alfheim transitions
    /// </summary>
    private void ApplyAethericGradient(Room room, string fromBiome, string toBiome, float blendRatio)
    {
        var gradient = CalculateAethericGradient(fromBiome, toBiome, blendRatio);

        room.SetEnvironmentalProperty("AethericIntensity", gradient.Intensity);
        room.Description += $" {gradient.Description}";

        if (gradient.VisualEffects.Any())
        {
            room.Description += $" {string.Join(" ", gradient.VisualEffects)}";
        }

        _log.Debug("Applied Aetheric gradient: {Intensity:P0} - {Description}",
            gradient.Intensity, gradient.Description);
    }

    /// <summary>
    /// Calculates Aetheric intensity gradient
    /// </summary>
    public AethericGradient CalculateAethericGradient(string fromBiome, string toBiome, float blendRatio)
    {
        var fromIntensity = GetBiomeAethericIntensity(fromBiome);
        var toIntensity = GetBiomeAethericIntensity(toBiome);

        // Linear interpolation between intensities
        var intensity = fromIntensity + (toIntensity - fromIntensity) * blendRatio;

        var description = intensity switch
        {
            > 0.8f => "Reality bends visibly. Aetheric energy saturates the air.",
            > 0.6f => "Strong Aetheric presence. Colors shift unnaturally.",
            > 0.4f => "Moderate Aetheric resonance. Faint shimmer at edges of vision.",
            > 0.2f => "Traces of Aetheric energy. Subtle distortions.",
            _ => "" // No Aetheric presence
        };

        var effects = new List<string>();
        if (intensity > 0.6f) effects.Add("Floating motes of light drift through the air.");
        if (intensity > 0.4f) effects.Add("Geometric patterns flicker in the periphery.");
        if (intensity > 0.2f) effects.Add("A faint humming resonates from the walls.");

        return new AethericGradient
        {
            Intensity = intensity,
            Description = description,
            VisualEffects = effects
        };
    }

    private float GetBiomeAethericIntensity(string biome)
    {
        return biome switch
        {
            "Alfheim" => 1.0f,         // Full Aetheric saturation
            "TheRoots" => 0.1f,        // Trace Aetheric leakage
            "Muspelheim" => 0.2f,      // Slight Aetheric from heat distortion
            "Niflheim" => 0.15f,       // Crystallized Aetheric traces
            "Jotunheim" => 0.05f,      // Minimal Aetheric presence
            "NeutralZone" => 0.0f,     // No Aetheric presence
            _ => 0.0f
        };
    }

    private bool IsAethericTransition(string fromBiome, string toBiome)
    {
        return fromBiome == "Alfheim" || toBiome == "Alfheim";
    }

    #endregion

    #region Scale Gradient

    /// <summary>
    /// Applies scale gradient for Jötunheim transitions
    /// </summary>
    private void ApplyScaleGradient(Room room, string fromBiome, string toBiome, float blendRatio)
    {
        var gradient = CalculateScaleGradient(fromBiome, toBiome, blendRatio);

        room.SetEnvironmentalProperty("ScaleFactor", gradient.ScaleFactor);
        room.Description += $" {gradient.Description}";

        _log.Debug("Applied scale gradient: {Scale}x - {Description}",
            gradient.ScaleFactor, gradient.Description);
    }

    /// <summary>
    /// Calculates scale gradient for giant-scale transitions
    /// </summary>
    public ScaleGradient CalculateScaleGradient(string fromBiome, string toBiome, float blendRatio)
    {
        var fromScale = GetBiomeScaleFactor(fromBiome);
        var toScale = GetBiomeScaleFactor(toBiome);

        // Linear interpolation between scale factors
        var scale = fromScale + (toScale - fromScale) * blendRatio;

        var description = scale switch
        {
            > 8.0f => "Colossal architecture dwarfs you. Built for giants.",
            > 6.0f => "Massive scale. Doorways 10 meters tall.",
            > 4.0f => "Oversized infrastructure. Clearly not human-scaled.",
            > 2.0f => "Noticeably large. Ceilings uncomfortably high.",
            _ => "" // Human-scaled
        };

        return new ScaleGradient
        {
            ScaleFactor = scale,
            Description = description
        };
    }

    private float GetBiomeScaleFactor(string biome)
    {
        return biome switch
        {
            "Jotunheim" => 10.0f,      // Built for giants
            "TheRoots" => 2.0f,        // Industrial oversized
            "Muspelheim" => 1.5f,      // Slightly larger for heat management
            "Niflheim" => 1.5f,        // Reinforced for structural integrity
            "Alfheim" => 1.0f,         // Human-scale (Aetheric, not physical)
            "NeutralZone" => 1.0f,     // Standard human-scale
            _ => 1.0f
        };
    }

    private bool IsScaleTransition(string fromBiome, string toBiome)
    {
        return fromBiome == "Jotunheim" || toBiome == "Jotunheim";
    }

    #endregion
}

/// <summary>
/// Temperature gradient data
/// </summary>
public class TemperatureGradient
{
    public float Temperature { get; set; }  // Celsius
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Aetheric intensity gradient data
/// </summary>
public class AethericGradient
{
    public float Intensity { get; set; }  // 0.0 to 1.0
    public string Description { get; set; } = string.Empty;
    public List<string> VisualEffects { get; set; } = new();
}

/// <summary>
/// Scale gradient data
/// </summary>
public class ScaleGradient
{
    public float ScaleFactor { get; set; }  // 1.0 = human-scale, 10.0 = giant-scale
    public string Description { get; set; } = string.Empty;
}
