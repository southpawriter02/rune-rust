// ------------------------------------------------------------------------------
// <copyright file="StressSourceExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for StressSource enum providing human-readable descriptions
// and resistance check applicability for the Psychic Stress system.
// Part of v0.18.0a Stress Enums & State implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="StressSource"/>.
/// Provides descriptive text and resistance check applicability.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods support the Psychic Stress system by providing:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Human-readable descriptions for each stress source category,
///       used in combat logs, UI tooltips, and diagnostic logging.
///     </description>
///   </item>
///   <item>
///     <description>
///       Resistance check applicability: whether a WILL-based resistance
///       check typically applies for a given stress source. Narrative and
///       Corruption sources are generally unavoidable.
///     </description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="StressSource"/>
public static class StressSourceExtensions
{
    // -------------------------------------------------------------------------
    // Description Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a display-friendly description of the stress source.
    /// </summary>
    /// <param name="source">The stress source.</param>
    /// <returns>A human-readable description suitable for UI display and logging.</returns>
    /// <example>
    /// <code>
    /// var desc = StressSource.Combat.GetDescription();
    /// // Returns "Combat-related psychological trauma"
    /// </code>
    /// </example>
    public static string GetDescription(this StressSource source) => source switch
    {
        StressSource.Combat => "Combat-related psychological trauma",
        StressSource.Exploration => "Discovery and exploration stress",
        StressSource.Narrative => "Story-driven psychological impact",
        StressSource.Heretical => "Forbidden knowledge or actions",
        StressSource.Environmental => "Environmental hazard exposure",
        StressSource.Corruption => "Runic Blight corruption stress",
        _ => "Unknown stress source"
    };

    // -------------------------------------------------------------------------
    // Resistance Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Indicates if resistance checks typically apply for this source.
    /// </summary>
    /// <param name="source">The stress source.</param>
    /// <returns>
    /// <c>true</c> if WILL-based resistance checks are typically used to
    /// mitigate stress from this source; <c>false</c> if the stress is
    /// generally unavoidable.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Resistable sources (Combat, Exploration, Heretical, Environmental)
    /// allow characters to make WILL-based checks to reduce incoming stress.
    /// Equipment, traits, and abilities may provide contextual bonuses.
    /// </para>
    /// <para>
    /// Non-resistable sources:
    /// <list type="bullet">
    ///   <item><description><see cref="StressSource.Narrative"/>: Unavoidable story beats that affect all characters equally.</description></item>
    ///   <item><description><see cref="StressSource.Corruption"/>: Automatic consequence of failing Corruption resistance checks.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var resistable = StressSource.Combat.TypicallyResistable();       // true
    /// var unavoidable = StressSource.Narrative.TypicallyResistable();   // false
    /// </code>
    /// </example>
    public static bool TypicallyResistable(this StressSource source) => source switch
    {
        StressSource.Combat => true,
        StressSource.Exploration => true,
        StressSource.Narrative => false,        // Unavoidable story beats
        StressSource.Heretical => true,
        StressSource.Environmental => true,
        StressSource.Corruption => false,       // Automatic from Corruption failure
        _ => true
    };
}
