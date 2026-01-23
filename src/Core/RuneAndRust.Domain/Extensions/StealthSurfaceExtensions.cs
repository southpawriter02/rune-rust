namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="StealthSurface"/> enum.
/// </summary>
public static class StealthSurfaceExtensions
{
    /// <summary>
    /// Gets the base DC for stealth checks on this surface type.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>
    /// The base DC in successes needed: Silent (2), Normal (3), Noisy (4), VeryNoisy (5).
    /// </returns>
    public static int GetBaseDc(this StealthSurface surface)
    {
        return surface switch
        {
            StealthSurface.Silent => 2,
            StealthSurface.Normal => 3,
            StealthSurface.Noisy => 4,
            StealthSurface.VeryNoisy => 5,
            _ => 3
        };
    }

    /// <summary>
    /// Gets a human-readable description of the surface type.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this StealthSurface surface)
    {
        return surface switch
        {
            StealthSurface.Silent => "Silent surface (DC 2)",
            StealthSurface.Normal => "Normal surface (DC 3)",
            StealthSurface.Noisy => "Noisy surface (DC 4)",
            StealthSurface.VeryNoisy => "Very noisy surface (DC 5)",
            _ => "Unknown surface"
        };
    }

    /// <summary>
    /// Gets example materials for this surface type.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>Array of example material names.</returns>
    public static string[] GetExamples(this StealthSurface surface)
    {
        return surface switch
        {
            StealthSurface.Silent => ["carpet", "moss", "sand", "padding"],
            StealthSurface.Normal => ["concrete", "dirt", "wood", "stone"],
            StealthSurface.Noisy => ["rubble", "gravel", "leaves", "creaky boards"],
            StealthSurface.VeryNoisy => ["scrap metal", "broken glass", "metal grating", "chains"],
            _ => []
        };
    }

    /// <summary>
    /// Gets whether this surface allows natural silent movement.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>True if DC ≤ 2, allowing easier stealth.</returns>
    public static bool IsNaturallySilent(this StealthSurface surface)
    {
        return surface == StealthSurface.Silent;
    }

    /// <summary>
    /// Gets the display name for this surface type.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>A formatted display name.</returns>
    public static string GetDisplayName(this StealthSurface surface)
    {
        return surface switch
        {
            StealthSurface.Silent => "Silent",
            StealthSurface.Normal => "Normal",
            StealthSurface.Noisy => "Noisy",
            StealthSurface.VeryNoisy => "Very Noisy",
            _ => surface.ToString()
        };
    }

    /// <summary>
    /// Gets whether this surface makes stealth particularly difficult.
    /// </summary>
    /// <param name="surface">The surface type.</param>
    /// <returns>True if DC ≥ 4, making stealth challenging.</returns>
    public static bool IsDifficultSurface(this StealthSurface surface)
    {
        return surface is StealthSurface.Noisy or StealthSurface.VeryNoisy;
    }
}
