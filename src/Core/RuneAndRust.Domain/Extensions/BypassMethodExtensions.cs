// ------------------------------------------------------------------------------
// <copyright file="BypassMethodExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for BypassMethod enum providing DC modifiers, requirements,
// and display utilities.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="BypassMethod"/>.
/// </summary>
public static class BypassMethodExtensions
{
    // -------------------------------------------------------------------------
    // DC Modifier Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the DC modifier for the bypass method.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>
    /// The DC modifier value. Negative values make bypass easier;
    /// positive values make it harder.
    /// </returns>
    /// <remarks>
    /// <para>
    /// DC modifiers by method:
    /// <list type="bullet">
    ///   <item><description>PercussiveMaintenance: +0</description></item>
    ///   <item><description>WireManipulation: -2</description></item>
    ///   <item><description>GlitchExploitation: -4</description></item>
    ///   <item><description>MemorizedSequence: -2</description></item>
    ///   <item><description>BruteDisassembly: +2</description></item>
    ///   <item><description>PowerCycling: +0</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetDcModifier(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance => 0,
            BypassMethod.WireManipulation => -2,
            BypassMethod.GlitchExploitation => -4,
            BypassMethod.MemorizedSequence => -2,
            BypassMethod.BruteDisassembly => 2,
            BypassMethod.PowerCycling => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Gets a formatted string describing the DC modifier.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A formatted modifier string (e.g., "+2 DC", "-2 DC", "+0 DC").</returns>
    public static string GetDcModifierDisplay(this BypassMethod method)
    {
        var modifier = method.GetDcModifier();
        return modifier switch
        {
            > 0 => $"+{modifier} DC",
            < 0 => $"{modifier} DC",
            _ => "+0 DC"
        };
    }

    // -------------------------------------------------------------------------
    // Requirement Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if the method has electrocution risk.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if electrocution save is required before attempting.</returns>
    /// <remarks>
    /// <para>
    /// Wire Manipulation requires a FINESSE save DC 12 to avoid electrocution.
    /// Failure deals 2d10 lightning damage before the bypass attempt proceeds.
    /// </para>
    /// </remarks>
    public static bool HasElectrocutionRisk(this BypassMethod method)
    {
        return method == BypassMethod.WireManipulation;
    }

    /// <summary>
    /// Determines if the method requires prior familiarity.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if character must have successfully bypassed this mechanism type before.</returns>
    /// <remarks>
    /// <para>
    /// Memorized Sequence can only be used if the character has previously
    /// bypassed a mechanism of the same type. The familiarity is tracked
    /// per mechanism type category.
    /// </para>
    /// </remarks>
    public static bool RequiresFamiliarity(this BypassMethod method)
    {
        return method == BypassMethod.MemorizedSequence;
    }

    /// <summary>
    /// Determines if the method requires a [Glitched] mechanism.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if mechanism must be in a glitched state.</returns>
    /// <remarks>
    /// <para>
    /// Glitch Exploitation can only target mechanisms exhibiting [Glitched] behavior—
    /// erratic operation, corrupted displays, or unstable states that can be
    /// exploited for easier bypass.
    /// </para>
    /// </remarks>
    public static bool RequiresGlitchedMechanism(this BypassMethod method)
    {
        return method == BypassMethod.GlitchExploitation;
    }

    /// <summary>
    /// Determines if the method destroys the mechanism on success.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if mechanism cannot be reused after successful bypass.</returns>
    /// <remarks>
    /// <para>
    /// Brute Disassembly tears the mechanism apart to gain access.
    /// On success, the mechanism is destroyed but salvage components are guaranteed.
    /// </para>
    /// </remarks>
    public static bool DestroysMechanism(this BypassMethod method)
    {
        return method == BypassMethod.BruteDisassembly;
    }

    /// <summary>
    /// Determines if the method resets mechanism state.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if mechanism state is cleared (lockouts, alerts, partial progress).</returns>
    /// <remarks>
    /// <para>
    /// Power Cycling cuts and restores power, potentially resetting the mechanism
    /// to a default state. This clears lockouts and alerts but also loses any
    /// partial progress from other methods.
    /// </para>
    /// </remarks>
    public static bool ResetsMechanismState(this BypassMethod method)
    {
        return method == BypassMethod.PowerCycling;
    }

    /// <summary>
    /// Determines if the method guarantees salvage on success.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>True if salvage components are guaranteed on successful bypass.</returns>
    /// <remarks>
    /// <para>
    /// Brute Disassembly always yields salvage components because the mechanism
    /// is systematically torn apart. Other methods only yield salvage on
    /// critical success (net successes >= 5).
    /// </para>
    /// </remarks>
    public static bool GuaranteesSalvage(this BypassMethod method)
    {
        return method == BypassMethod.BruteDisassembly;
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the human-readable display name for the bypass method.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A human-readable name (e.g., "Percussive Maintenance").</returns>
    public static string GetDisplayName(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance => "Percussive Maintenance",
            BypassMethod.WireManipulation => "Wire Manipulation",
            BypassMethod.GlitchExploitation => "Glitch Exploitation",
            BypassMethod.MemorizedSequence => "Memorized Sequence",
            BypassMethod.BruteDisassembly => "Brute Disassembly",
            BypassMethod.PowerCycling => "Power Cycling",
            _ => method.ToString()
        };
    }

    /// <summary>
    /// Gets a description of the bypass method.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A descriptive string explaining the method.</returns>
    public static string GetDescription(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance =>
                "Strike the mechanism with calculated force to jar components into alignment.",
            BypassMethod.WireManipulation =>
                "Access internal wiring to bypass security circuits directly.",
            BypassMethod.GlitchExploitation =>
                "Exploit the mechanism's corrupted behavior to slip past its defenses.",
            BypassMethod.MemorizedSequence =>
                "Apply a button sequence learned from previous successful bypasses.",
            BypassMethod.BruteDisassembly =>
                "Tear the mechanism apart to force access, destroying it in the process.",
            BypassMethod.PowerCycling =>
                "Cut and restore power to reset the mechanism to its default state.",
            _ => "Unknown method."
        };
    }

    /// <summary>
    /// Gets a description of the risks associated with the bypass method.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A string describing the method's risks or requirements.</returns>
    public static string GetRiskDescription(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance =>
                "May permanently break the mechanism on fumble",
            BypassMethod.WireManipulation =>
                "Electrocution risk: FINESSE DC 12 or 2d10 lightning damage",
            BypassMethod.GlitchExploitation =>
                "Unpredictable: Only works on [Glitched] mechanisms",
            BypassMethod.MemorizedSequence =>
                "Only works on mechanism types you've bypassed before",
            BypassMethod.BruteDisassembly =>
                "Destroys the mechanism, but guarantees component salvage",
            BypassMethod.PowerCycling =>
                "Resets all progress, including partial successes",
            _ => "Unknown risk."
        };
    }

    // -------------------------------------------------------------------------
    // Validation Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Determines if the method can be used in the given context.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <param name="isFamiliar">Whether the character is familiar with this mechanism type.</param>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <returns>True if the method is valid for the given context.</returns>
    public static bool IsValidForContext(
        this BypassMethod method,
        bool isFamiliar,
        bool isGlitched)
    {
        // MemorizedSequence requires familiarity
        if (method.RequiresFamiliarity() && !isFamiliar)
        {
            return false;
        }

        // GlitchExploitation requires glitched mechanism
        if (method.RequiresGlitchedMechanism() && !isGlitched)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the reason why a method cannot be used, or null if valid.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <param name="isFamiliar">Whether the character is familiar with this mechanism type.</param>
    /// <param name="isGlitched">Whether the mechanism is in a glitched state.</param>
    /// <returns>A reason string if invalid, or null if the method is valid.</returns>
    public static string? GetInvalidReason(
        this BypassMethod method,
        bool isFamiliar,
        bool isGlitched)
    {
        if (method.RequiresFamiliarity() && !isFamiliar)
        {
            return "Requires familiarity with this mechanism type";
        }

        if (method.RequiresGlitchedMechanism() && !isGlitched)
        {
            return "Mechanism must be [Glitched]";
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // UI Helper Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a color hint for UI display based on risk level.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A color name suggestion (e.g., "yellow", "red", "blue").</returns>
    public static string GetColorHint(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance => "white",
            BypassMethod.WireManipulation => "yellow",      // Electrocution risk
            BypassMethod.GlitchExploitation => "purple",    // Unpredictable
            BypassMethod.MemorizedSequence => "blue",       // Requires setup
            BypassMethod.BruteDisassembly => "red",         // Destructive
            BypassMethod.PowerCycling => "cyan",            // Reset
            _ => "white"
        };
    }

    /// <summary>
    /// Gets an icon hint for UI display.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>An icon identifier or description.</returns>
    public static string GetIconHint(this BypassMethod method)
    {
        return method switch
        {
            BypassMethod.PercussiveMaintenance => "hammer",
            BypassMethod.WireManipulation => "wire",
            BypassMethod.GlitchExploitation => "glitch",
            BypassMethod.MemorizedSequence => "brain",
            BypassMethod.BruteDisassembly => "wrench",
            BypassMethod.PowerCycling => "power",
            _ => "unknown"
        };
    }
}
