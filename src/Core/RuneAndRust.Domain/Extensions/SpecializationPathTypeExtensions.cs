// ------------------------------------------------------------------------------
// <copyright file="SpecializationPathTypeExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for SpecializationPathType enum providing Corruption risk
// checks, creation warnings, and human-readable descriptions.
// Part of v0.17.4a Specialization Enum & Path Types implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="SpecializationPathType"/>.
/// </summary>
/// <remarks>
/// <para>
/// Provides utility methods for the <see cref="SpecializationPathType"/> enum,
/// including Corruption risk evaluation, UI warning generation for character
/// creation, and human-readable descriptions for display purposes.
/// </para>
/// <para>
/// These extension methods are primarily used during character creation Step 5
/// (Specialization Selection) to inform the player about the Corruption
/// implications of their specialization choice.
/// </para>
/// </remarks>
/// <seealso cref="SpecializationPathType"/>
/// <seealso cref="SpecializationId"/>
/// <seealso cref="SpecializationIdExtensions"/>
public static class SpecializationPathTypeExtensions
{
    // -------------------------------------------------------------------------
    // Corruption Risk Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets whether this path type risks Corruption from ability use.
    /// </summary>
    /// <param name="pathType">The path type to evaluate.</param>
    /// <returns>
    /// <c>true</c> if abilities on this path may trigger Corruption gain
    /// (Heretical); <c>false</c> if no Corruption risk exists (Coherent).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Only <see cref="SpecializationPathType.Heretical"/> paths carry
    /// Corruption risk. Coherent paths operate within stable reality
    /// and do not interact with corrupted Aether.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var pathType = SpecializationPathType.Heretical;
    /// bool risksCorruption = pathType.RisksCorruption(); // true
    /// </code>
    /// </example>
    public static bool RisksCorruption(this SpecializationPathType pathType)
    {
        return pathType == SpecializationPathType.Heretical;
    }

    // -------------------------------------------------------------------------
    // UI Warning Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the warning message for this path type during character creation.
    /// </summary>
    /// <param name="pathType">The path type to get the warning for.</param>
    /// <returns>
    /// A warning string for UI display if the path is Heretical;
    /// <c>null</c> for Coherent paths that carry no Corruption risk.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used during character creation Step 5 (Specialization
    /// Selection) to display a warning when the player selects a Heretical
    /// specialization. The warning informs the player that some abilities
    /// may trigger Corruption gain.
    /// </para>
    /// <para>
    /// Coherent paths return <c>null</c> to indicate no warning is needed.
    /// UI code should check for null before displaying.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var warning = SpecializationPathType.Heretical.GetCreationWarning();
    /// // "This specialization interfaces with corrupted Aether. Some abilities may trigger Corruption gain."
    ///
    /// var noWarning = SpecializationPathType.Coherent.GetCreationWarning();
    /// // null
    /// </code>
    /// </example>
    public static string? GetCreationWarning(this SpecializationPathType pathType)
    {
        return pathType switch
        {
            SpecializationPathType.Heretical =>
                "This specialization interfaces with corrupted Aether. " +
                "Some abilities may trigger Corruption gain.",
            SpecializationPathType.Coherent => null,
            _ => null
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a human-readable description of the path type.
    /// </summary>
    /// <param name="pathType">The path type to describe.</param>
    /// <returns>
    /// A descriptive string suitable for UI display explaining what this
    /// path type means for gameplay.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns a concise description of the path type's relationship
    /// with reality and the Aether. Used in tooltips, specialization
    /// detail screens, and character summary displays.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var desc = SpecializationPathType.Coherent.GetDescription();
    /// // "Works within stable reality"
    ///
    /// var desc2 = SpecializationPathType.Heretical.GetDescription();
    /// // "Interfaces with corrupted Aether"
    /// </code>
    /// </example>
    public static string GetDescription(this SpecializationPathType pathType)
    {
        return pathType switch
        {
            SpecializationPathType.Coherent => "Works within stable reality",
            SpecializationPathType.Heretical => "Interfaces with corrupted Aether",
            _ => "Unknown path type"
        };
    }
}
