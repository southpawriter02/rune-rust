// ------------------------------------------------------------------------------
// <copyright file="InformationGained.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Represents a piece of information extracted during interrogation,
// including its topic, content, and reliability assessment.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.Extensions;

/// <summary>
/// Represents a piece of information extracted during interrogation,
/// including its topic, content, and reliability assessment.
/// </summary>
/// <remarks>
/// <para>
/// When a subject is broken, the interrogator can extract information on
/// various topics. Each piece of information has a reliability rating based
/// on the extraction method used, which indicates how likely it is to be accurate.
/// </para>
/// <para>
/// Information reliability by method:
/// <list type="bullet">
///   <item><description>Good Cop: 95% reliable (genuine cooperation)</description></item>
///   <item><description>Bribery: 90% reliable (paid for truth)</description></item>
///   <item><description>Bad Cop: 80% reliable (fear may cause errors)</description></item>
///   <item><description>Deception: 70% reliable (counter-deception risk)</description></item>
///   <item><description>Torture: 50% reliable (will say anything to stop pain)</description></item>
/// </list>
/// </para>
/// <para>
/// IMPORTANT: If Torture was used at ANY point during the interrogation,
/// maximum reliability is capped at 60% regardless of primary method.
/// </para>
/// </remarks>
public sealed record InformationGained
{
    /// <summary>
    /// Gets the topic or category of the information.
    /// </summary>
    /// <remarks>
    /// Examples: "Location of hideout", "Names of conspirators", "Guard schedules".
    /// </remarks>
    public required string Topic { get; init; }

    /// <summary>
    /// Gets the actual information content provided by the subject.
    /// </summary>
    /// <remarks>
    /// This is what the subject told the interrogator. May be true, partially
    /// true, or completely false depending on reliability and circumstances.
    /// </remarks>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the reliability percentage based on the extraction method.
    /// </summary>
    /// <remarks>
    /// Value from 0-100 indicating probability that the information is accurate.
    /// Higher values indicate more trustworthy intelligence.
    /// </remarks>
    public required int ReliabilityPercent { get; init; }

    /// <summary>
    /// Gets the method used to extract this specific piece of information.
    /// </summary>
    /// <remarks>
    /// The method determines the base reliability rating. This is the method
    /// that was used in the round when the subject broke or was being questioned.
    /// </remarks>
    public required InterrogationMethod SourceMethod { get; init; }

    /// <summary>
    /// Gets whether the information has been verified as true or false.
    /// </summary>
    /// <remarks>
    /// Null if the information has not yet been verified. Verification may
    /// occur through investigation, events, or other means during gameplay.
    /// </remarks>
    public bool? IsVerified { get; init; }

    /// <summary>
    /// Gets the actual truth value (only meaningful after verification).
    /// </summary>
    /// <remarks>
    /// <para>
    /// During extraction, this is set based on the reliability roll but is
    /// hidden from the player. It becomes visible after verification occurs.
    /// </para>
    /// <para>
    /// Game mechanics note: The game rolls percentile dice against reliability
    /// to determine if information is true. A 95% reliable piece of info has
    /// a 95% chance of being accurate.
    /// </para>
    /// </remarks>
    public bool? IsTrue { get; init; }

    /// <summary>
    /// Gets the reliability assessment text for display.
    /// </summary>
    /// <returns>A human-readable assessment of the information's reliability.</returns>
    /// <remarks>
    /// <para>
    /// Assessment levels:
    /// <list type="bullet">
    ///   <item><description>≥90%: "Likely accurate"</description></item>
    ///   <item><description>≥80%: "Probably accurate"</description></item>
    ///   <item><description>≥70%: "Possibly accurate"</description></item>
    ///   <item><description>≥60%: "Questionable"</description></item>
    ///   <item><description>&lt;60%: "UNRELIABLE"</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string GetReliabilityAssessment()
    {
        return ReliabilityPercent switch
        {
            >= 90 => "Likely accurate - subject has incentive to cooperate",
            >= 80 => "Probably accurate - some room for error",
            >= 70 => "Possibly accurate - subject may be withholding or deceiving",
            >= 60 => "Questionable - significant chance of misinformation",
            _ => "UNRELIABLE - subject was desperate and may have said anything"
        };
    }

    /// <summary>
    /// Gets a formatted display string for this information.
    /// </summary>
    /// <returns>A multi-line display string suitable for UI presentation.</returns>
    public string GetDisplayText()
    {
        var verificationStatus = IsVerified switch
        {
            true when IsTrue == true => " [VERIFIED TRUE]",
            true when IsTrue == false => " [VERIFIED FALSE]",
            _ => string.Empty
        };

        return $"Topic: {Topic}\n" +
               $"Content: {Content}\n" +
               $"Reliability: {ReliabilityPercent}% ({GetReliabilityAssessment()}){verificationStatus}";
    }

    /// <summary>
    /// Gets a compact single-line summary of this information.
    /// </summary>
    /// <returns>A compact summary string.</returns>
    public string GetSummary()
    {
        var indicator = ReliabilityPercent >= 80 ? "✓" :
                       ReliabilityPercent >= 60 ? "?" : "⚠";
        var verifiedTag = IsVerified == true
            ? (IsTrue == true ? " [TRUE]" : " [FALSE]")
            : string.Empty;

        return $"{indicator} {Topic}: {ReliabilityPercent}%{verifiedTag}";
    }

    /// <summary>
    /// Creates verified information with the truth value revealed.
    /// </summary>
    /// <param name="isTrue">Whether the information was true.</param>
    /// <returns>A new InformationGained with verification applied.</returns>
    public InformationGained WithVerification(bool isTrue)
    {
        return this with
        {
            IsVerified = true,
            IsTrue = isTrue
        };
    }

    /// <summary>
    /// Creates a new InformationGained for a given topic and method.
    /// </summary>
    /// <param name="topic">The information topic.</param>
    /// <param name="content">The information content.</param>
    /// <param name="method">The extraction method used.</param>
    /// <param name="reliabilityOverride">Optional reliability override (for torture cap).</param>
    /// <returns>A new InformationGained instance.</returns>
    public static InformationGained Create(
        string topic,
        string content,
        InterrogationMethod method,
        int? reliabilityOverride = null)
    {
        var reliability = reliabilityOverride ?? method.GetReliabilityPercent();

        return new InformationGained
        {
            Topic = topic,
            Content = content,
            ReliabilityPercent = reliability,
            SourceMethod = method,
            IsVerified = null,
            IsTrue = null
        };
    }
}
