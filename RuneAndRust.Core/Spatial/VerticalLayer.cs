namespace RuneAndRust.Core.Spatial;

/// <summary>
/// v0.39.1: Vertical layer definitions spanning -300m to +300m
/// Each layer represents approximately 100 meters of vertical distance
/// </summary>
public enum VerticalLayer
{
    /// <summary>
    /// Z=-3: -300 meters (ancient deep infrastructure, heavily decayed)
    /// </summary>
    DeepRoots = -3,

    /// <summary>
    /// Z=-2: -200 meters (lower maintenance tunnels, geothermal systems)
    /// </summary>
    LowerRoots = -2,

    /// <summary>
    /// Z=-1: -100 meters (upper maintenance levels, cooling systems)
    /// </summary>
    UpperRoots = -1,

    /// <summary>
    /// Z=0: Entry level, primary access, most common layer
    /// </summary>
    GroundLevel = 0,

    /// <summary>
    /// Z=+1: +100 meters (mid-facility, administrative and processing)
    /// </summary>
    LowerTrunk = 1,

    /// <summary>
    /// Z=+2: +200 meters (upper facility, observation and control)
    /// </summary>
    UpperTrunk = 2,

    /// <summary>
    /// Z=+3: +300 meters (surface exposure, environmental interface)
    /// </summary>
    Canopy = 3
}

/// <summary>
/// Extension methods for VerticalLayer enum
/// </summary>
public static class VerticalLayerExtensions
{
    /// <summary>
    /// Gets the approximate depth/height in meters for this layer
    /// </summary>
    public static int GetApproximateDepth(this VerticalLayer layer)
    {
        return layer switch
        {
            VerticalLayer.DeepRoots => -300,
            VerticalLayer.LowerRoots => -200,
            VerticalLayer.UpperRoots => -100,
            VerticalLayer.GroundLevel => 0,
            VerticalLayer.LowerTrunk => 100,
            VerticalLayer.UpperTrunk => 200,
            VerticalLayer.Canopy => 300,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a human-readable description of this layer
    /// </summary>
    public static string GetLayerDescription(this VerticalLayer layer)
    {
        return layer switch
        {
            VerticalLayer.DeepRoots => "Deep infrastructure levels, oldest sections, heavily decayed",
            VerticalLayer.LowerRoots => "Lower maintenance tunnels and geothermal service passages",
            VerticalLayer.UpperRoots => "Upper maintenance levels, cooling and cryogenic systems",
            VerticalLayer.GroundLevel => "Primary access level, main chambers and corridors",
            VerticalLayer.LowerTrunk => "Mid-facility, administrative and processing centers",
            VerticalLayer.UpperTrunk => "Upper facility, observation decks and control rooms",
            VerticalLayer.Canopy => "Surface exposure, environmental interface, ash-filled sky",
            _ => "Unknown layer"
        };
    }

    /// <summary>
    /// Gets typical biomes found at this layer
    /// </summary>
    public static List<string> GetTypicalBiomes(this VerticalLayer layer)
    {
        return layer switch
        {
            VerticalLayer.DeepRoots => new List<string> { "The_Roots", "Jotunheim" },
            VerticalLayer.LowerRoots => new List<string> { "The_Roots", "Muspelheim" },
            VerticalLayer.UpperRoots => new List<string> { "The_Roots", "Niflheim" },
            VerticalLayer.GroundLevel => new List<string> { "The_Roots", "Muspelheim", "Niflheim", "Jotunheim", "Alfheim" },
            VerticalLayer.LowerTrunk => new List<string> { "Jotunheim", "Alfheim" },
            VerticalLayer.UpperTrunk => new List<string> { "Alfheim" },
            VerticalLayer.Canopy => new List<string> { "Alfheim" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Gets environmental characteristics for this layer
    /// </summary>
    public static string GetCharacteristics(this VerticalLayer layer)
    {
        return layer switch
        {
            VerticalLayer.DeepRoots => "Ancient, rare, extreme decay, dangerous structural instability",
            VerticalLayer.LowerRoots => "Geothermal activity, steam vents, intense heat, high pressure",
            VerticalLayer.UpperRoots => "Cryogenic systems, frozen zones, ice formations, brittle materials",
            VerticalLayer.GroundLevel => "Most common, varied environments, primary pathways",
            VerticalLayer.LowerTrunk => "Heavy industrial, Aetheric resonance, mechanical systems",
            VerticalLayer.UpperTrunk => "High Aetheric energy, reality distortions, observation platforms",
            VerticalLayer.Canopy => "Exposed to ash-filled sky, environmental hazards, surface access",
            _ => "Unknown characteristics"
        };
    }

    /// <summary>
    /// Converts a Z coordinate to its corresponding VerticalLayer
    /// </summary>
    public static VerticalLayer FromZCoordinate(int z)
    {
        return z switch
        {
            -3 => VerticalLayer.DeepRoots,
            -2 => VerticalLayer.LowerRoots,
            -1 => VerticalLayer.UpperRoots,
            0 => VerticalLayer.GroundLevel,
            1 => VerticalLayer.LowerTrunk,
            2 => VerticalLayer.UpperTrunk,
            3 => VerticalLayer.Canopy,
            _ => VerticalLayer.GroundLevel // Default fallback
        };
    }

    /// <summary>
    /// Checks if this layer is below ground level
    /// </summary>
    public static bool IsBelowGround(this VerticalLayer layer)
    {
        return (int)layer < 0;
    }

    /// <summary>
    /// Checks if this layer is above ground level
    /// </summary>
    public static bool IsAboveGround(this VerticalLayer layer)
    {
        return (int)layer > 0;
    }

    /// <summary>
    /// Gets the distance in layers between two vertical layers
    /// </summary>
    public static int DistanceTo(this VerticalLayer from, VerticalLayer to)
    {
        return Math.Abs((int)from - (int)to);
    }

    /// <summary>
    /// Gets a descriptor for depth/height narrative
    /// Example: "300 meters beneath the surface" or "100 meters above ground level"
    /// </summary>
    public static string GetDepthNarrative(this VerticalLayer layer)
    {
        var depth = layer.GetApproximateDepth();

        if (depth < 0)
        {
            return $"{Math.Abs(depth)} meters beneath the surface";
        }
        else if (depth > 0)
        {
            return $"{depth} meters above ground level";
        }
        else
        {
            return "at ground level";
        }
    }
}
