// ------------------------------------------------------------------------------
// <copyright file="ProtocolViolationTypeExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the ProtocolViolationType enum providing DC adjustments,
// dice penalties, disposition changes, and recovery information.
// Part of v0.15.3g Cultural Protocols implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="ProtocolViolationType"/> enum providing
/// DC adjustments, dice penalties, disposition changes, and recovery information.
/// </summary>
public static class ProtocolViolationTypeExtensions
{
    /// <summary>
    /// Gets the DC increase caused by this violation type.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// The amount to add to future check DCs with this NPC.
    /// <list type="bullet">
    ///   <item><description><see cref="ProtocolViolationType.None"/>: 0</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Minor"/>: +2</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Moderate"/>: +4</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Severe"/>: +6</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Unforgivable"/>: Interaction blocked</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static int GetDcIncrease(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => 0,
            ProtocolViolationType.Minor => 2,
            ProtocolViolationType.Moderate => 4,
            ProtocolViolationType.Severe => 6,
            ProtocolViolationType.Unforgivable => int.MaxValue, // Interaction blocked
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets the dice pool penalty caused by this violation type.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// The number of dice to remove from the pool (as a positive number).
    /// <list type="bullet">
    ///   <item><description><see cref="ProtocolViolationType.None"/>: 0</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Minor"/>: 0</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Moderate"/>: 1</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Severe"/>: 2</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Unforgivable"/>: N/A (interaction blocked)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static int GetDicePenalty(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => 0,
            ProtocolViolationType.Minor => 0,
            ProtocolViolationType.Moderate => 1,
            ProtocolViolationType.Severe => 2,
            ProtocolViolationType.Unforgivable => 0, // N/A - interaction blocked
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets the disposition change caused by this violation.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// The disposition change (negative values indicate loss).
    /// <list type="bullet">
    ///   <item><description><see cref="ProtocolViolationType.None"/>: 0</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Minor"/>: -5</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Moderate"/>: -15</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Severe"/>: -30</description></item>
    ///   <item><description><see cref="ProtocolViolationType.Unforgivable"/>: -100 (hostile)</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static int GetDispositionChange(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => 0,
            ProtocolViolationType.Minor => -5,
            ProtocolViolationType.Moderate => -15,
            ProtocolViolationType.Severe => -30,
            ProtocolViolationType.Unforgivable => -100,
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>Human-readable violation type name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static string GetDisplayName(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => "None",
            ProtocolViolationType.Minor => "Minor Faux Pas",
            ProtocolViolationType.Moderate => "Protocol Breach",
            ProtocolViolationType.Severe => "Serious Offense",
            ProtocolViolationType.Unforgivable => "Unforgivable Transgression",
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets a human-readable description of this violation type.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>A descriptive string suitable for display.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static string GetDescription(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => "No violation",
            ProtocolViolationType.Minor => "Minor faux pas (+2 DC)",
            ProtocolViolationType.Moderate => "Protocol breach (+4 DC, -1d10)",
            ProtocolViolationType.Severe => "Serious offense (+6 DC, -2d10)",
            ProtocolViolationType.Unforgivable => "Unforgivable transgression (interaction blocked)",
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets a description of the consequences for this violation type.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>Text describing what happens as a result of this violation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static string GetConsequenceDescription(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None =>
                "No violation occurred. The interaction proceeds normally.",
            ProtocolViolationType.Minor =>
                "A small error that is easily overlooked. +2 DC on next check, " +
                "recovers automatically after one successful interaction.",
            ProtocolViolationType.Moderate =>
                "A noticeable breach requiring acknowledgment. +4 DC and -1d10 on future checks. " +
                "Recovery requires explicit apology or small gesture.",
            ProtocolViolationType.Severe =>
                "A serious offense with lasting consequences. +6 DC and -2d10 on future checks. " +
                "Recovery requires significant effort, gifts, or intermediary. NPC may become hostile.",
            ProtocolViolationType.Unforgivable =>
                "A permanent breach of trust. Interaction blocked entirely. " +
                "May cause faction reputation loss and hostility from other NPCs of this culture.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets a description of how to recover from this violation.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>Text describing recovery options.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static string GetRecoveryDescription(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None =>
                "No recovery needed.",
            ProtocolViolationType.Minor =>
                "Recovery is automatic after one successful interaction, or a brief apology.",
            ProtocolViolationType.Moderate =>
                "Requires explicit acknowledgment of error and a small gesture of respect " +
                "(such as a minor gift or formal apology).",
            ProtocolViolationType.Severe =>
                "Requires one or more of: substantial gift appropriate to the culture, " +
                "formal apology before witnesses, completing a task set by the offended party, " +
                "or intervention by a respected third party.",
            ProtocolViolationType.Unforgivable =>
                "Generally impossible with this NPC. May require extraordinary measures such as " +
                "completing a major quest, divine intervention, or may be truly permanent.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Determines if this violation blocks further interaction.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// <c>true</c> if interaction is blocked;
    /// <c>false</c> if interaction can continue with penalties.
    /// </returns>
    public static bool BlocksInteraction(this ProtocolViolationType violation)
    {
        return violation == ProtocolViolationType.Unforgivable;
    }

    /// <summary>
    /// Determines if this violation can be recovered from.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// <c>true</c> if recovery is possible;
    /// <c>false</c> if the damage is permanent.
    /// </returns>
    public static bool IsRecoverable(this ProtocolViolationType violation)
    {
        return violation != ProtocolViolationType.Unforgivable;
    }

    /// <summary>
    /// Determines if this violation may trigger hostility.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// <c>true</c> if the NPC may become hostile;
    /// <c>false</c> if hostility is unlikely.
    /// </returns>
    /// <remarks>
    /// Severe and Unforgivable violations may cause NPCs to become hostile,
    /// potentially initiating combat or calling guards.
    /// </remarks>
    public static bool MayTriggerHostility(this ProtocolViolationType violation)
    {
        return violation is ProtocolViolationType.Severe or ProtocolViolationType.Unforgivable;
    }

    /// <summary>
    /// Determines if this violation affects faction reputation.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>
    /// <c>true</c> if faction reputation is affected;
    /// <c>false</c> if only the individual NPC relationship is affected.
    /// </returns>
    /// <remarks>
    /// Only Unforgivable violations typically affect reputation with the entire
    /// faction or culture. Lesser violations only affect the individual NPC.
    /// </remarks>
    public static bool AffectsFactionReputation(this ProtocolViolationType violation)
    {
        return violation == ProtocolViolationType.Unforgivable;
    }

    /// <summary>
    /// Gets the color hint for UI presentation of this violation type.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>A suggested color name for visual indication.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown violation type is provided.
    /// </exception>
    public static string GetColorHint(this ProtocolViolationType violation)
    {
        return violation switch
        {
            ProtocolViolationType.None => "Green",
            ProtocolViolationType.Minor => "Yellow",
            ProtocolViolationType.Moderate => "Orange",
            ProtocolViolationType.Severe => "Red",
            ProtocolViolationType.Unforgivable => "DarkRed",
            _ => throw new ArgumentOutOfRangeException(
                nameof(violation),
                violation,
                "Unknown protocol violation type")
        };
    }

    /// <summary>
    /// Gets the severity level as an integer for comparison.
    /// </summary>
    /// <param name="violation">The violation type.</param>
    /// <returns>An integer representing relative severity (0-4).</returns>
    public static int GetSeverityLevel(this ProtocolViolationType violation)
    {
        return (int)violation;
    }

    /// <summary>
    /// Determines if this violation is more severe than another.
    /// </summary>
    /// <param name="violation">The violation type to compare.</param>
    /// <param name="other">The other violation type.</param>
    /// <returns>
    /// <c>true</c> if this violation is more severe;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsMoreSevereThan(this ProtocolViolationType violation, ProtocolViolationType other)
    {
        return (int)violation > (int)other;
    }
}
