// ------------------------------------------------------------------------------
// <copyright file="RestTypeExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for RestType enum providing recovery calculations, formula
// descriptions, and full recovery identification for the Psychic Stress system.
// Part of v0.18.0a Stress Enums & State implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="RestType"/>.
/// Provides recovery calculations, formula descriptions, and recovery type queries.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods support the Psychic Stress recovery system by providing:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       Recovery calculations: stress reduction amounts based on rest type and
///       character WILL attribute.
///     </description>
///   </item>
///   <item>
///     <description>
///       Formula descriptions: human-readable formula strings for UI display
///       and combat/rest logs.
///     </description>
///   </item>
///   <item>
///     <description>
///       Recovery type identification: whether a rest type provides full
///       stress reset (only Sanctuary).
///     </description>
///   </item>
/// </list>
/// </remarks>
/// <seealso cref="RestType"/>
public static class RestTypeExtensions
{
    // -------------------------------------------------------------------------
    // Recovery Calculation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calculates the stress recovery amount for this rest type.
    /// </summary>
    /// <param name="restType">The type of rest.</param>
    /// <param name="willAttribute">
    /// The character's WILL attribute value (used for Short and Long rest).
    /// Defaults to 0 for rest types that do not use WILL (Sanctuary, Milestone).
    /// </param>
    /// <returns>
    /// The amount of stress recovered. Returns <see cref="int.MaxValue"/> for
    /// Sanctuary rest (full reset handled by caller clamping to 0).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="willAttribute"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Character with WILL 4 takes a Short Rest
    /// var recovery = RestType.Short.CalculateRecovery(4); // Returns 8
    ///
    /// // Long Rest with WILL 4
    /// var longRecovery = RestType.Long.CalculateRecovery(4); // Returns 20
    ///
    /// // Sanctuary always fully recovers
    /// var fullRecovery = RestType.Sanctuary.CalculateRecovery(4); // Returns int.MaxValue
    ///
    /// // Milestone is fixed regardless of WILL
    /// var milestone = RestType.Milestone.CalculateRecovery(0); // Returns 25
    /// </code>
    /// </example>
    public static int CalculateRecovery(this RestType restType, int willAttribute = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(willAttribute);

        return restType switch
        {
            RestType.Short => willAttribute * 2,
            RestType.Long => willAttribute * 5,
            RestType.Sanctuary => int.MaxValue,     // Full reset handled by caller
            RestType.Milestone => 25,
            _ => 0
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display name and formula for logging/UI.
    /// </summary>
    /// <param name="restType">The rest type.</param>
    /// <returns>A formatted string with name and formula.</returns>
    /// <example>
    /// <code>
    /// var desc = RestType.Short.GetFormulaDescription(); // Returns "Short Rest (WILL × 2)"
    /// </code>
    /// </example>
    public static string GetFormulaDescription(this RestType restType) => restType switch
    {
        RestType.Short => "Short Rest (WILL × 2)",
        RestType.Long => "Long Rest (WILL × 5)",
        RestType.Sanctuary => "Sanctuary (Full Reset)",
        RestType.Milestone => "Milestone (+25)",
        _ => "Unknown"
    };

    // -------------------------------------------------------------------------
    // Recovery Type Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Indicates whether this rest type provides full stress recovery.
    /// </summary>
    /// <param name="restType">The rest type.</param>
    /// <returns>
    /// <c>true</c> if this rest type fully resets stress to 0;
    /// <c>false</c> for partial recovery rest types.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Only <see cref="RestType.Sanctuary"/> provides full recovery. Sanctuary
    /// locations are rare safe havens in Aethelgard — reaching one is a
    /// significant accomplishment that rewards the player with complete
    /// stress relief.
    /// </para>
    /// </remarks>
    public static bool IsFullRecovery(this RestType restType) =>
        restType == RestType.Sanctuary;
}
