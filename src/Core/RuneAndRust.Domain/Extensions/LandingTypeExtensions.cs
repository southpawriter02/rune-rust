using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.Extensions;

/// <summary>
/// Extension methods for <see cref="LandingType"/> enum.
/// </summary>
public static class LandingTypeExtensions
{
    /// <summary>
    /// Gets the DC modifier for this landing type.
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>DC modifier to apply.</returns>
    /// <example>
    /// <code>
    /// var mod = LandingType.Precision.GetDcModifier(); // Returns 1
    /// var mod = LandingType.Glitched.GetDcModifier(); // Returns 2
    /// var mod = LandingType.Downward.GetDcModifier(); // Returns -1
    /// </code>
    /// </example>
    public static int GetDcModifier(this LandingType landingType)
    {
        return landingType switch
        {
            LandingType.Normal => 0,
            LandingType.Precision => 1,
            LandingType.Glitched => 2,
            LandingType.PrecisionGlitched => 3,
            LandingType.Downward => -1,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a human-readable description of the landing type.
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>A descriptive string for UI display.</returns>
    /// <example>
    /// <code>
    /// var desc = LandingType.Precision.GetDescription(); // "Precision landing (DC +1)"
    /// </code>
    /// </example>
    public static string GetDescription(this LandingType landingType)
    {
        return landingType switch
        {
            LandingType.Normal => "Normal landing",
            LandingType.Precision => "Precision landing (DC +1)",
            LandingType.Glitched => "Glitched surface (DC +2)",
            LandingType.PrecisionGlitched => "Precision on Glitched (DC +3)",
            LandingType.Downward => "Downward leap (DC -1)",
            _ => "Unknown landing type"
        };
    }

    /// <summary>
    /// Gets a short display name for the landing type.
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>The landing type name.</returns>
    public static string GetDisplayName(this LandingType landingType)
    {
        return landingType switch
        {
            LandingType.Normal => "Normal",
            LandingType.Precision => "Precision",
            LandingType.Glitched => "Glitched",
            LandingType.PrecisionGlitched => "Precision+Glitched",
            LandingType.Downward => "Downward",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets whether this landing type involves corruption effects.
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>True if the landing involves Glitched terrain.</returns>
    public static bool IsGlitched(this LandingType landingType)
    {
        return landingType is LandingType.Glitched or LandingType.PrecisionGlitched;
    }

    /// <summary>
    /// Gets whether this landing type is a precision landing.
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>True if the landing requires precision.</returns>
    public static bool IsPrecision(this LandingType landingType)
    {
        return landingType is LandingType.Precision or LandingType.PrecisionGlitched;
    }

    /// <summary>
    /// Gets whether this landing type assists the leap (reduces DC).
    /// </summary>
    /// <param name="landingType">The landing type.</param>
    /// <returns>True if the landing type provides a DC reduction.</returns>
    public static bool IsAssisted(this LandingType landingType)
    {
        return landingType == LandingType.Downward;
    }
}
