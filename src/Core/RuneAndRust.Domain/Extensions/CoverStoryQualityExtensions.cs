// ------------------------------------------------------------------------------
// <copyright file="CoverStoryQualityExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for CoverStoryQuality enum.
// Part of v0.15.3c Deception System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="CoverStoryQuality"/> enum.
/// </summary>
public static class CoverStoryQualityExtensions
{
    /// <summary>
    /// Gets the DC modifier for this cover story quality.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <returns>The DC modifier (negative values reduce DC).</returns>
    /// <remarks>
    /// DC modifiers:
    /// <list type="bullet">
    ///   <item><description>None: +0</description></item>
    ///   <item><description>Basic: -1</description></item>
    ///   <item><description>Good: -2</description></item>
    ///   <item><description>Excellent: -3</description></item>
    ///   <item><description>Masterwork: -4</description></item>
    /// </list>
    /// </remarks>
    public static int GetDcModifier(this CoverStoryQuality quality)
    {
        return quality switch
        {
            CoverStoryQuality.None => 0,
            CoverStoryQuality.Basic => -1,
            CoverStoryQuality.Good => -2,
            CoverStoryQuality.Excellent => -3,
            CoverStoryQuality.Masterwork => -4,
            _ => 0
        };
    }

    /// <summary>
    /// Gets whether this cover story quality requires preparation.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <returns>True if the quality requires prior preparation.</returns>
    public static bool RequiresPreparation(this CoverStoryQuality quality)
    {
        return quality >= CoverStoryQuality.Good;
    }

    /// <summary>
    /// Gets whether this cover story quality typically needs supporting evidence.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <returns>True if forged documents or other evidence is typically needed.</returns>
    public static bool RequiresEvidence(this CoverStoryQuality quality)
    {
        return quality == CoverStoryQuality.Masterwork;
    }

    /// <summary>
    /// Gets a human-readable description of the cover story quality.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this CoverStoryQuality quality)
    {
        return quality switch
        {
            CoverStoryQuality.None => "None (improvised)",
            CoverStoryQuality.Basic => "Basic (-1 DC) - General story, few details",
            CoverStoryQuality.Good => "Good (-2 DC) - Consistent details, names",
            CoverStoryQuality.Excellent => "Excellent (-3 DC) - Researched, verifiable",
            CoverStoryQuality.Masterwork => "Masterwork (-4 DC) - Airtight with proof",
            _ => "Unknown quality"
        };
    }

    /// <summary>
    /// Gets the short name for this cover story quality.
    /// </summary>
    /// <param name="quality">The cover story quality.</param>
    /// <returns>A short name string.</returns>
    public static string GetShortName(this CoverStoryQuality quality)
    {
        return quality switch
        {
            CoverStoryQuality.None => "None",
            CoverStoryQuality.Basic => "Basic",
            CoverStoryQuality.Good => "Good",
            CoverStoryQuality.Excellent => "Excellent",
            CoverStoryQuality.Masterwork => "Masterwork",
            _ => "Unknown"
        };
    }
}
