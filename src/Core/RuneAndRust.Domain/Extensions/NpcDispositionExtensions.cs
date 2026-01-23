namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="NpcDisposition"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Provides utility methods for converting between disposition values and categories,
/// calculating dice modifiers, and understanding disposition thresholds.
/// </para>
/// </remarks>
public static class NpcDispositionExtensions
{
    /// <summary>
    /// Gets the dice modifier for this disposition level.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>Dice pool modifier: positive for bonus, negative for penalty.</returns>
    /// <remarks>
    /// Dice modifiers range from +3d10 (Ally) to -2d10 (Hostile).
    /// These modifiers apply to all social skill checks with the NPC.
    /// </remarks>
    public static int GetDiceModifier(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => 3,
            NpcDisposition.Friendly => 2,
            NpcDisposition.NeutralPositive => 1,
            NpcDisposition.Neutral => 0,
            NpcDisposition.Unfriendly => -1,
            NpcDisposition.Hostile => -2,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the minimum disposition value threshold for this level.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>The minimum numeric value for this disposition category.</returns>
    public static int GetMinThreshold(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => 75,
            NpcDisposition.Friendly => 50,
            NpcDisposition.NeutralPositive => 10,
            NpcDisposition.Neutral => -9,
            NpcDisposition.Unfriendly => -49,
            NpcDisposition.Hostile => -100,
            _ => -100
        };
    }

    /// <summary>
    /// Gets the maximum disposition value threshold for this level.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>The maximum numeric value for this disposition category.</returns>
    public static int GetMaxThreshold(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => 100,
            NpcDisposition.Friendly => 74,
            NpcDisposition.NeutralPositive => 49,
            NpcDisposition.Neutral => 9,
            NpcDisposition.Unfriendly => -10,
            NpcDisposition.Hostile => -50,
            _ => 100
        };
    }

    /// <summary>
    /// Determines the disposition level from a numeric value.
    /// </summary>
    /// <param name="value">Disposition value (-100 to +100).</param>
    /// <returns>The corresponding <see cref="NpcDisposition"/> level.</returns>
    /// <remarks>
    /// <para>Threshold ranges:</para>
    /// <list type="bullet">
    ///   <item><description>Ally: ≥75</description></item>
    ///   <item><description>Friendly: 50-74</description></item>
    ///   <item><description>NeutralPositive: 10-49</description></item>
    ///   <item><description>Neutral: -9 to +9</description></item>
    ///   <item><description>Unfriendly: -49 to -10</description></item>
    ///   <item><description>Hostile: ≤-50</description></item>
    /// </list>
    /// </remarks>
    public static NpcDisposition FromValue(int value)
    {
        return value switch
        {
            >= 75 => NpcDisposition.Ally,
            >= 50 => NpcDisposition.Friendly,
            >= 10 => NpcDisposition.NeutralPositive,
            >= -9 => NpcDisposition.Neutral,
            >= -49 => NpcDisposition.Unfriendly,
            _ => NpcDisposition.Hostile
        };
    }

    /// <summary>
    /// Gets the midpoint value for a disposition category.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>The approximate midpoint value for this category.</returns>
    /// <remarks>
    /// Useful for initializing NPCs at a specific disposition level.
    /// </remarks>
    public static int GetMidpointValue(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => 87,          // Midpoint of 75-100
            NpcDisposition.Friendly => 62,       // Midpoint of 50-74
            NpcDisposition.NeutralPositive => 30, // Midpoint of 10-49
            NpcDisposition.Neutral => 0,         // Midpoint of -9 to +9
            NpcDisposition.Unfriendly => -30,    // Midpoint of -49 to -10
            NpcDisposition.Hostile => -75,       // Midpoint of -100 to -50
            _ => 0
        };
    }

    /// <summary>
    /// Gets a human-readable description of the disposition level.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => "Ally",
            NpcDisposition.Friendly => "Friendly",
            NpcDisposition.NeutralPositive => "Well-disposed",
            NpcDisposition.Neutral => "Neutral",
            NpcDisposition.Unfriendly => "Unfriendly",
            NpcDisposition.Hostile => "Hostile",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets a detailed description of what this disposition means.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>A longer description for tooltips or help text.</returns>
    public static string GetDetailedDescription(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => "Deep trust and loyalty. Will take personal risks for you.",
            NpcDisposition.Friendly => "Positive regard, willing to help within reason.",
            NpcDisposition.NeutralPositive => "Generally well-disposed, open to interaction.",
            NpcDisposition.Neutral => "No particular opinion, professional distance.",
            NpcDisposition.Unfriendly => "Distrustful, unwilling to help without strong incentive.",
            NpcDisposition.Hostile => "Active animosity, may refuse interaction entirely.",
            _ => "Unknown disposition level."
        };
    }

    /// <summary>
    /// Gets whether this disposition level blocks certain interactions.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>True if some social options may be unavailable.</returns>
    /// <remarks>
    /// Hostile NPCs may refuse to engage in certain interactions like
    /// negotiation or may attack on sight.
    /// </remarks>
    public static bool BlocksSomeInteractions(this NpcDisposition disposition)
    {
        return disposition == NpcDisposition.Hostile;
    }

    /// <summary>
    /// Gets whether this disposition level allows forming alliances.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>True if the NPC could become a companion or ally.</returns>
    /// <remarks>
    /// Only Friendly or Ally disposition allows the NPC to join as a
    /// companion or provide significant assistance.
    /// </remarks>
    public static bool AllowsAlliance(this NpcDisposition disposition)
    {
        return disposition >= NpcDisposition.Friendly;
    }

    /// <summary>
    /// Gets whether this disposition is positive overall.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>True if the disposition provides bonuses to social checks.</returns>
    public static bool IsPositive(this NpcDisposition disposition)
    {
        return disposition >= NpcDisposition.NeutralPositive;
    }

    /// <summary>
    /// Gets whether this disposition is negative overall.
    /// </summary>
    /// <param name="disposition">The disposition level.</param>
    /// <returns>True if the disposition provides penalties to social checks.</returns>
    public static bool IsNegative(this NpcDisposition disposition)
    {
        return disposition <= NpcDisposition.Unfriendly;
    }

    /// <summary>
    /// Gets the next higher disposition level, if any.
    /// </summary>
    /// <param name="disposition">The current disposition level.</param>
    /// <returns>The next higher level, or the current level if already at max.</returns>
    public static NpcDisposition GetNextHigher(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Hostile => NpcDisposition.Unfriendly,
            NpcDisposition.Unfriendly => NpcDisposition.Neutral,
            NpcDisposition.Neutral => NpcDisposition.NeutralPositive,
            NpcDisposition.NeutralPositive => NpcDisposition.Friendly,
            NpcDisposition.Friendly => NpcDisposition.Ally,
            NpcDisposition.Ally => NpcDisposition.Ally,
            _ => disposition
        };
    }

    /// <summary>
    /// Gets the next lower disposition level, if any.
    /// </summary>
    /// <param name="disposition">The current disposition level.</param>
    /// <returns>The next lower level, or the current level if already at min.</returns>
    public static NpcDisposition GetNextLower(this NpcDisposition disposition)
    {
        return disposition switch
        {
            NpcDisposition.Ally => NpcDisposition.Friendly,
            NpcDisposition.Friendly => NpcDisposition.NeutralPositive,
            NpcDisposition.NeutralPositive => NpcDisposition.Neutral,
            NpcDisposition.Neutral => NpcDisposition.Unfriendly,
            NpcDisposition.Unfriendly => NpcDisposition.Hostile,
            NpcDisposition.Hostile => NpcDisposition.Hostile,
            _ => disposition
        };
    }
}
