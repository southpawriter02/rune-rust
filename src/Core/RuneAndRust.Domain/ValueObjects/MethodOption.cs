// ------------------------------------------------------------------------------
// <copyright file="MethodOption.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a bypass method option with its availability and modifiers.
// Part of v0.15.4e Jury-Rigging System implementation.
// </summary>
// ------------------------------------------------------------------------------

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a bypass method option with its availability and modifiers.
/// </summary>
/// <remarks>
/// <para>
/// This value object represents a single bypass method as presented to
/// the player during Method Selection:
/// <list type="bullet">
///   <item><description>Method identifier and display information</description></item>
///   <item><description>Availability status and reason if unavailable</description></item>
///   <item><description>DC modifier and risk descriptions</description></item>
///   <item><description>Special requirements and consequences</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Method">The bypass method.</param>
/// <param name="IsAvailable">Whether this method can be selected in current context.</param>
/// <param name="UnavailableReason">Reason why the method is unavailable, if applicable.</param>
/// <param name="Description">Description of what the method does.</param>
/// <param name="DcModifier">The DC modifier for this method.</param>
/// <param name="RiskDescription">Description of the method's risks.</param>
public readonly record struct MethodOption(
    BypassMethod Method,
    bool IsAvailable,
    string? UnavailableReason,
    string Description,
    int DcModifier,
    string RiskDescription)
{
    // -------------------------------------------------------------------------
    // Computed Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the display name for this method.
    /// </summary>
    public string DisplayName => Method.GetDisplayName();

    /// <summary>
    /// Gets a formatted DC modifier string.
    /// </summary>
    /// <remarks>
    /// Returns strings like "+2 DC", "-4 DC", or "+0 DC".
    /// </remarks>
    public string DcModifierDisplay => Method.GetDcModifierDisplay();

    /// <summary>
    /// Gets a value indicating whether this method has electrocution risk.
    /// </summary>
    /// <remarks>
    /// Wire Manipulation requires a FINESSE save to avoid electrocution.
    /// </remarks>
    public bool HasElectrocutionRisk => Method.HasElectrocutionRisk();

    /// <summary>
    /// Gets a value indicating whether this method requires familiarity.
    /// </summary>
    /// <remarks>
    /// Memorized Sequence is only available if the character has
    /// previously bypassed this mechanism type.
    /// </remarks>
    public bool RequiresFamiliarity => Method.RequiresFamiliarity();

    /// <summary>
    /// Gets a value indicating whether this method requires a glitched mechanism.
    /// </summary>
    /// <remarks>
    /// Glitch Exploitation is only available if the mechanism exhibits
    /// [Glitched] behavior.
    /// </remarks>
    public bool RequiresGlitched => Method.RequiresGlitchedMechanism();

    /// <summary>
    /// Gets a value indicating whether this method will destroy the mechanism.
    /// </summary>
    /// <remarks>
    /// Brute Disassembly destroys the mechanism but guarantees salvage.
    /// </remarks>
    public bool WillDestroy => Method.DestroysMechanism();

    /// <summary>
    /// Gets a value indicating whether this method resets mechanism state.
    /// </summary>
    /// <remarks>
    /// Power Cycling resets lockouts and alerts but also clears partial progress.
    /// </remarks>
    public bool WillReset => Method.ResetsMechanismState();

    /// <summary>
    /// Gets a value indicating whether this method guarantees salvage.
    /// </summary>
    /// <remarks>
    /// Brute Disassembly always yields salvage on success.
    /// </remarks>
    public bool GuaranteesSalvage => Method.GuaranteesSalvage();

    /// <summary>
    /// Gets a color hint for UI display.
    /// </summary>
    public string ColorHint => Method.GetColorHint();

    /// <summary>
    /// Gets an icon hint for UI display.
    /// </summary>
    public string IconHint => Method.GetIconHint();

    // -------------------------------------------------------------------------
    // Factory Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates an available method option.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <returns>A MethodOption that is available for selection.</returns>
    public static MethodOption Available(BypassMethod method)
    {
        return new MethodOption(
            Method: method,
            IsAvailable: true,
            UnavailableReason: null,
            Description: method.GetDescription(),
            DcModifier: method.GetDcModifier(),
            RiskDescription: method.GetRiskDescription());
    }

    /// <summary>
    /// Creates an unavailable method option.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <param name="reason">The reason why the method is unavailable.</param>
    /// <returns>A MethodOption that cannot be selected.</returns>
    public static MethodOption Unavailable(BypassMethod method, string reason)
    {
        return new MethodOption(
            Method: method,
            IsAvailable: false,
            UnavailableReason: reason,
            Description: method.GetDescription(),
            DcModifier: method.GetDcModifier(),
            RiskDescription: method.GetRiskDescription());
    }

    /// <summary>
    /// Creates a method option from context.
    /// </summary>
    /// <param name="method">The bypass method.</param>
    /// <param name="isFamiliar">Whether the character is familiar with this mechanism type.</param>
    /// <param name="isGlitched">Whether the mechanism is glitched.</param>
    /// <returns>A MethodOption with availability based on context.</returns>
    /// <remarks>
    /// Automatically determines availability based on:
    /// <list type="bullet">
    ///   <item><description>MemorizedSequence: Requires familiarity</description></item>
    ///   <item><description>GlitchExploitation: Requires glitched mechanism</description></item>
    /// </list>
    /// </remarks>
    public static MethodOption FromContext(
        BypassMethod method,
        bool isFamiliar,
        bool isGlitched)
    {
        var invalidReason = method.GetInvalidReason(isFamiliar, isGlitched);

        if (invalidReason != null)
        {
            return Unavailable(method, invalidReason);
        }

        return Available(method);
    }

    /// <summary>
    /// Creates method options for all bypass methods in the given context.
    /// </summary>
    /// <param name="isFamiliar">Whether the character is familiar with this mechanism type.</param>
    /// <param name="isGlitched">Whether the mechanism is glitched.</param>
    /// <returns>A list of all method options with availability.</returns>
    public static IReadOnlyList<MethodOption> GetAllForContext(
        bool isFamiliar,
        bool isGlitched)
    {
        return new List<MethodOption>
        {
            FromContext(BypassMethod.PercussiveMaintenance, isFamiliar, isGlitched),
            FromContext(BypassMethod.WireManipulation, isFamiliar, isGlitched),
            FromContext(BypassMethod.GlitchExploitation, isFamiliar, isGlitched),
            FromContext(BypassMethod.MemorizedSequence, isFamiliar, isGlitched),
            FromContext(BypassMethod.BruteDisassembly, isFamiliar, isGlitched),
            FromContext(BypassMethod.PowerCycling, isFamiliar, isGlitched)
        };
    }

    // -------------------------------------------------------------------------
    // Display Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a formatted display string for the method option.
    /// </summary>
    /// <returns>A human-readable description of the method option.</returns>
    public string ToDisplayString()
    {
        var statusStr = IsAvailable ? "✓" : "✗";
        var reasonStr = !IsAvailable && UnavailableReason != null
            ? $" ({UnavailableReason})"
            : "";

        var modifiers = new List<string> { DcModifierDisplay };

        if (HasElectrocutionRisk) modifiers.Add("Electrocution Risk");
        if (WillDestroy) modifiers.Add("Destroys Mechanism");
        if (WillReset) modifiers.Add("Resets State");
        if (GuaranteesSalvage) modifiers.Add("Guaranteed Salvage");

        var modifierStr = string.Join(", ", modifiers);

        return $"[{statusStr}] {DisplayName} ({modifierStr}){reasonStr}";
    }

    /// <summary>
    /// Returns a compact string for logging purposes.
    /// </summary>
    /// <returns>A log-friendly string representation.</returns>
    public string ToLogString()
    {
        return $"MethodOption[{Method} Available={IsAvailable} DC={DcModifier:+0;-0;+0}]";
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return ToDisplayString();
    }
}
