using Microsoft.Extensions.Logging;

namespace RuneAndRust.Domain.Extensions;

/// <summary>
/// Extension methods for <see cref="Enums.SurfaceType"/> providing climbing modifiers and descriptions.
/// </summary>
/// <remarks>
/// <para>
/// Surface types affect climbing checks through two modifier types:
/// <list type="bullet">
///   <item><description>Dice Modifiers: Add or remove dice from the pool (most surfaces)</description></item>
///   <item><description>DC Modifiers: Increase the difficulty class (Glitched surfaces only)</description></item>
/// </list>
/// </para>
/// <para>
/// Modifier Reference:
/// <list type="table">
///   <listheader>
///     <term>Surface</term>
///     <description>Modifier</description>
///   </listheader>
///   <item><term>Stable</term><description>+1d10</description></item>
///   <item><term>Normal</term><description>+0</description></item>
///   <item><term>Wet</term><description>-1d10</description></item>
///   <item><term>Compromised</term><description>-2d10</description></item>
///   <item><term>Collapsing</term><description>-3d10</description></item>
///   <item><term>Glitched</term><description>DC +2</description></item>
/// </list>
/// </para>
/// </remarks>
public static class SurfaceTypeExtensions
{
    /// <summary>
    /// Gets the dice pool modifier for a surface type.
    /// </summary>
    /// <param name="surfaceType">The surface type to get the modifier for.</param>
    /// <returns>
    /// The dice modifier value. Positive values add dice, negative values remove dice.
    /// Returns 0 for surfaces that use DC modifiers instead (e.g., Glitched).
    /// </returns>
    /// <example>
    /// <code>
    /// var modifier = SurfaceType.Wet.GetDiceModifier(); // Returns -1
    /// var totalPool = baseDice + modifier; // e.g., 4 + (-1) = 3
    /// </code>
    /// </example>
    public static int GetDiceModifier(this Enums.SurfaceType surfaceType)
    {
        return surfaceType switch
        {
            Enums.SurfaceType.Stable => 1,       // Well-maintained: +1d10
            Enums.SurfaceType.Normal => 0,       // Standard: no modifier
            Enums.SurfaceType.Wet => -1,         // Slippery: -1d10
            Enums.SurfaceType.Compromised => -2, // Damaged: -2d10
            Enums.SurfaceType.Collapsing => -3,  // Falling apart: -3d10
            Enums.SurfaceType.Glitched => 0,     // Uses DC modifier instead
            _ => 0
        };
    }

    /// <summary>
    /// Gets the difficulty class modifier for a surface type.
    /// </summary>
    /// <param name="surfaceType">The surface type to get the DC modifier for.</param>
    /// <returns>
    /// The DC modifier value. Positive values increase difficulty.
    /// Most surfaces return 0 as they use dice modifiers instead.
    /// </returns>
    /// <remarks>
    /// Currently only <see cref="Enums.SurfaceType.Glitched"/> uses DC modification.
    /// This represents the unpredictable nature of corruption-affected surfaces
    /// where the difficulty itself becomes harder rather than reducing the climber's
    /// ability to attempt the check.
    /// </remarks>
    /// <example>
    /// <code>
    /// var dcMod = SurfaceType.Glitched.GetDcModifier(); // Returns 2
    /// var effectiveDc = baseDc + dcMod; // e.g., 2 + 2 = 4
    /// </code>
    /// </example>
    public static int GetDcModifier(this Enums.SurfaceType surfaceType)
    {
        return surfaceType switch
        {
            Enums.SurfaceType.Glitched => 2, // Corruption increases difficulty
            _ => 0                           // All other surfaces use dice modifiers
        };
    }

    /// <summary>
    /// Gets a human-readable description of the surface type including its modifier.
    /// </summary>
    /// <param name="surfaceType">The surface type to describe.</param>
    /// <returns>A formatted description suitable for display to players.</returns>
    /// <example>
    /// <code>
    /// var desc = SurfaceType.Wet.GetDescription();
    /// // Returns: "Wet surface (-1d10)"
    /// </code>
    /// </example>
    public static string GetDescription(this Enums.SurfaceType surfaceType)
    {
        return surfaceType switch
        {
            Enums.SurfaceType.Stable => "Stable surface (+1d10)",
            Enums.SurfaceType.Normal => "Normal surface",
            Enums.SurfaceType.Wet => "Wet surface (-1d10)",
            Enums.SurfaceType.Compromised => "Compromised surface (-2d10)",
            Enums.SurfaceType.Collapsing => "Collapsing surface (-3d10)",
            Enums.SurfaceType.Glitched => "Glitched surface (DC +2)",
            _ => "Unknown surface"
        };
    }

    /// <summary>
    /// Determines whether this surface type uses DC modification instead of dice modification.
    /// </summary>
    /// <param name="surfaceType">The surface type to check.</param>
    /// <returns>
    /// <c>true</c> if the surface type applies its modifier to the DC rather than the dice pool;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This helper is useful when calculating final modifiers to determine which
    /// value (dice pool or DC) should be adjusted.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (surface.UsesDcModifier())
    /// {
    ///     effectiveDc += surface.GetDcModifier();
    /// }
    /// else
    /// {
    ///     dicePool += surface.GetDiceModifier();
    /// }
    /// </code>
    /// </example>
    public static bool UsesDcModifier(this Enums.SurfaceType surfaceType)
    {
        return surfaceType == Enums.SurfaceType.Glitched;
    }

    /// <summary>
    /// Gets a short display name for the surface type.
    /// </summary>
    /// <param name="surfaceType">The surface type.</param>
    /// <returns>The display name without modifier information.</returns>
    public static string GetDisplayName(this Enums.SurfaceType surfaceType)
    {
        return surfaceType switch
        {
            Enums.SurfaceType.Stable => "Stable",
            Enums.SurfaceType.Normal => "Normal",
            Enums.SurfaceType.Wet => "Wet",
            Enums.SurfaceType.Compromised => "Compromised",
            Enums.SurfaceType.Collapsing => "Collapsing",
            Enums.SurfaceType.Glitched => "Glitched",
            _ => "Unknown"
        };
    }
}
