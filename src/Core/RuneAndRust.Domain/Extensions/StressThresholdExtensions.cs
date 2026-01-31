// ------------------------------------------------------------------------------
// <copyright file="StressThresholdExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for StressThreshold enum providing min/max stress ranges,
// defense penalty lookup, threshold calculation from stress values, and display
// utilities for the Psychic Stress system.
// Part of v0.18.0a Stress Enums & State implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="StressThreshold"/>.
/// Provides stress range queries, penalty calculations, threshold resolution,
/// and display formatting.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods support the Psychic Stress system by providing:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Stress range queries: minimum and maximum stress values for each threshold tier.
///     </description>
///   </item>
///   <item>
///     <description>
///       Penalty calculations: defense penalty derived from threshold ordinal value.
///     </description>
///   </item>
///   <item>
///     <description>
///       Threshold resolution: determines the correct threshold tier for any stress
///       value in the 0-100 range.
///     </description>
///   </item>
///   <item>
///     <description>
///       Display formatting: human-readable threshold descriptions for UI and logging.
///     </description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="StressThreshold"/>
/// <seealso cref="RuneAndRust.Domain.ValueObjects.StressState"/>
public static class StressThresholdExtensions
{
    // -------------------------------------------------------------------------
    // Stress Range Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the minimum stress value for this threshold tier.
    /// </summary>
    /// <param name="threshold">The stress threshold.</param>
    /// <returns>The minimum stress value (inclusive) for the threshold.</returns>
    /// <example>
    /// <code>
    /// var minStress = StressThreshold.Anxious.GetMinStress(); // Returns 40
    /// </code>
    /// </example>
    public static int GetMinStress(this StressThreshold threshold) => threshold switch
    {
        StressThreshold.Calm => 0,
        StressThreshold.Uneasy => 20,
        StressThreshold.Anxious => 40,
        StressThreshold.Panicked => 60,
        StressThreshold.Breaking => 80,
        StressThreshold.Trauma => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(threshold))
    };

    /// <summary>
    /// Gets the maximum stress value for this threshold tier.
    /// </summary>
    /// <param name="threshold">The stress threshold.</param>
    /// <returns>The maximum stress value (inclusive) for the threshold.</returns>
    /// <example>
    /// <code>
    /// var maxStress = StressThreshold.Calm.GetMaxStress(); // Returns 19
    /// </code>
    /// </example>
    public static int GetMaxStress(this StressThreshold threshold) => threshold switch
    {
        StressThreshold.Calm => 19,
        StressThreshold.Uneasy => 39,
        StressThreshold.Anxious => 59,
        StressThreshold.Panicked => 79,
        StressThreshold.Breaking => 99,
        StressThreshold.Trauma => 100,
        _ => throw new ArgumentOutOfRangeException(nameof(threshold))
    };

    // -------------------------------------------------------------------------
    // Penalty Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the defense penalty associated with this threshold.
    /// </summary>
    /// <param name="threshold">The stress threshold.</param>
    /// <returns>The defense penalty value (0 to 5).</returns>
    /// <remarks>
    /// <para>
    /// The defense penalty equals the threshold's ordinal value:
    /// Calm = 0, Uneasy = 1, Anxious = 2, Panicked = 3, Breaking = 4, Trauma = 5.
    /// This penalty is subtracted from the character's effective Defense stat.
    /// </para>
    /// </remarks>
    public static int GetDefensePenalty(this StressThreshold threshold) => (int)threshold;

    // -------------------------------------------------------------------------
    // Threshold Resolution Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines the stress threshold for a given stress value.
    /// </summary>
    /// <param name="stress">The current stress value (0-100).</param>
    /// <returns>The corresponding <see cref="StressThreshold"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if stress is negative or greater than 100.
    /// </exception>
    /// <example>
    /// <code>
    /// var threshold = StressThresholdExtensions.FromStressValue(45); // Returns Anxious
    /// var trauma = StressThresholdExtensions.FromStressValue(100);   // Returns Trauma
    /// </code>
    /// </example>
    public static StressThreshold FromStressValue(int stress)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(stress);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stress, 100);

        return stress switch
        {
            >= 100 => StressThreshold.Trauma,
            >= 80 => StressThreshold.Breaking,
            >= 60 => StressThreshold.Panicked,
            >= 40 => StressThreshold.Anxious,
            >= 20 => StressThreshold.Uneasy,
            _ => StressThreshold.Calm
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a display-friendly string for the threshold.
    /// </summary>
    /// <param name="threshold">The stress threshold.</param>
    /// <returns>A display string with threshold name and stress range.</returns>
    /// <example>
    /// <code>
    /// var display = StressThreshold.Anxious.ToDisplayString(); // Returns "Anxious (40-59)"
    /// </code>
    /// </example>
    public static string ToDisplayString(this StressThreshold threshold) =>
        $"{threshold} ({threshold.GetMinStress()}-{threshold.GetMaxStress()})";
}
