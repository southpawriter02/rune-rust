// ------------------------------------------------------------------------------
// <copyright file="InterrogationMethodExtensions.cs" company="Rune &amp; Rust">
//     Copyright (c) Rune &amp; Rust. All rights reserved.
// </copyright>
// <summary>
// Extension methods for the InterrogationMethod enum providing DCs, reliability
// percentages, costs, durations, and other method-specific information.
// Part of v0.15.3f Interrogation System implementation.
// </summary>
// ------------------------------------------------------------------------------

namespace RuneAndRust.Domain.Extensions;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for the <see cref="InterrogationMethod"/> enum providing
/// DCs, reliability percentages, costs, durations, and other method-specific information.
/// </summary>
public static class InterrogationMethodExtensions
{
    /// <summary>
    /// Gets the base Difficulty Class (DC) for this interrogation method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The base DC required for success.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Note: Torture's DC is calculated differently - it equals Subject's WILL × 2.
    /// This method returns 0 for Torture as a signal to use the special calculation.
    /// </para>
    /// <para>
    /// DC values by method:
    /// <list type="bullet">
    ///   <item><description>GoodCop: 14 (harder to build genuine rapport)</description></item>
    ///   <item><description>BadCop: 12 (fear is effective but crude)</description></item>
    ///   <item><description>Deception: 16 (opposed by subject's WITS)</description></item>
    ///   <item><description>Bribery: 10 (money talks)</description></item>
    ///   <item><description>Torture: WILL × 2 (special calculation)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static int GetBaseDc(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => 14,
            InterrogationMethod.BadCop => 12,
            InterrogationMethod.Deception => 16,
            InterrogationMethod.Bribery => 10,
            InterrogationMethod.Torture => 0, // Special: calculated as WILL × 2
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the duration in minutes for one round using this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The time in minutes required for one round.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    /// <remarks>
    /// Duration reflects the nature of the method:
    /// <list type="bullet">
    ///   <item><description>GoodCop: 30 min (rapport building takes time)</description></item>
    ///   <item><description>BadCop: 15 min (quick intimidation)</description></item>
    ///   <item><description>Deception: 20 min (setting up lies)</description></item>
    ///   <item><description>Bribery: 10 min (negotiating price)</description></item>
    ///   <item><description>Torture: 60 min (lengthy process)</description></item>
    /// </list>
    /// </remarks>
    public static int GetRoundDurationMinutes(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => 30,
            InterrogationMethod.BadCop => 15,
            InterrogationMethod.Deception => 20,
            InterrogationMethod.Bribery => 10,
            InterrogationMethod.Torture => 60,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the base reliability percentage for information extracted using this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The reliability percentage (0-100).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Reliability indicates how likely the information is to be accurate.
    /// Higher reliability methods produce more trustworthy intelligence.
    /// </para>
    /// <para>
    /// <list type="bullet">
    ///   <item><description>GoodCop: 95% (genuine cooperation)</description></item>
    ///   <item><description>Bribery: 90% (paid for truth)</description></item>
    ///   <item><description>BadCop: 80% (fear may cause errors)</description></item>
    ///   <item><description>Deception: 70% (subject may counter-deceive)</description></item>
    ///   <item><description>Torture: 50% (subject will say anything)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// IMPORTANT: Using Torture at ANY point during an interrogation caps the
    /// final reliability at 60%, regardless of primary method used.
    /// </para>
    /// </remarks>
    public static int GetReliabilityPercent(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => 95,
            InterrogationMethod.BadCop => 80,
            InterrogationMethod.Deception => 70,
            InterrogationMethod.Bribery => 90,
            InterrogationMethod.Torture => 50,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the disposition change applied per round when using this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The disposition change (negative values damage relationship).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    /// <remarks>
    /// Disposition changes accumulate across rounds and affect the subject's
    /// attitude toward the interrogator. Negative values indicate relationship
    /// damage that may affect future interactions.
    /// </remarks>
    public static int GetDispositionChangePerRound(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => 0,
            InterrogationMethod.BadCop => -5,
            InterrogationMethod.Deception => -2,
            InterrogationMethod.Bribery => 0,
            InterrogationMethod.Torture => -20,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the reputation cost for using this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The reputation cost (applied per session for Torture).</returns>
    /// <remarks>
    /// Only Torture incurs reputation cost (-30 per session). Other methods
    /// do not directly damage the interrogator's reputation.
    /// </remarks>
    public static int GetReputationCost(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.Torture => -30,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the underlying social interaction system used by this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>
    /// The SocialInteractionType used by this method, or null for Torture.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Most methods leverage existing social systems:
    /// <list type="bullet">
    ///   <item><description>GoodCop: Persuasion</description></item>
    ///   <item><description>BadCop: Intimidation</description></item>
    ///   <item><description>Deception: Deception</description></item>
    ///   <item><description>Bribery: Persuasion</description></item>
    ///   <item><description>Torture: None (raw attribute check)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static SocialInteractionType? GetUnderlyingSystem(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => SocialInteractionType.Persuasion,
            InterrogationMethod.BadCop => SocialInteractionType.Intimidation,
            InterrogationMethod.Deception => SocialInteractionType.Deception,
            InterrogationMethod.Bribery => SocialInteractionType.Persuasion,
            InterrogationMethod.Torture => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the display name for UI presentation.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>Human-readable method name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static string GetDisplayName(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => "Good Cop",
            InterrogationMethod.BadCop => "Bad Cop",
            InterrogationMethod.Deception => "Deception",
            InterrogationMethod.Bribery => "Bribery",
            InterrogationMethod.Torture => "Torture",
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets a description of the method for player guidance.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>Descriptive text explaining the method.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static string GetDescription(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop =>
                "Build rapport through empathy and understanding. Slowest but most reliable.",
            InterrogationMethod.BadCop =>
                "Threaten and intimidate. Fast but damages relationship and less reliable.",
            InterrogationMethod.Deception =>
                "Trick the subject into revealing information. Risky if they see through you.",
            InterrogationMethod.Bribery =>
                "Offer gold for information. Quick and reliable but costs resources.",
            InterrogationMethod.Torture =>
                "Physical coercion. Severe consequences, unreliable information, moral cost.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the warning text displayed before using this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>Warning text, or empty string if no warning needed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static string GetWarningText(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop =>
                string.Empty,
            InterrogationMethod.BadCop =>
                "⚠️ This method causes -5 disposition per round.",
            InterrogationMethod.Deception =>
                "⚠️ Risk of counter-deception. Incurs Liar's Burden stress.",
            InterrogationMethod.Bribery =>
                "💰 Requires gold. Cost scales with subject resistance.",
            InterrogationMethod.Torture =>
                "⚠️ SEVERE CONSEQUENCES: -30 reputation, subject traumatized, " +
                "info only 50% reliable, factions may become hostile, " +
                "fumble may kill subject.",
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets the fumble type associated with this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>The FumbleType for failures with this method.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static FumbleType GetFumbleType(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => FumbleType.TrustShattered,
            InterrogationMethod.BadCop => FumbleType.ChallengeAccepted,
            InterrogationMethod.Deception => FumbleType.LieExposed,
            InterrogationMethod.Bribery => FumbleType.TrustShattered,
            InterrogationMethod.Torture => FumbleType.SubjectBroken,
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets whether this method requires resources (gold) to use.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if the method requires gold expenditure.</returns>
    public static bool RequiresResources(this InterrogationMethod method)
    {
        return method == InterrogationMethod.Bribery;
    }

    /// <summary>
    /// Gets whether this method can use MIGHT instead of WILL.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if MIGHT can be used as the attribute.</returns>
    /// <remarks>
    /// BadCop (intimidation) and Torture can use either MIGHT or WILL,
    /// allowing physically imposing characters to leverage their strength.
    /// </remarks>
    public static bool CanUseMight(this InterrogationMethod method)
    {
        return method == InterrogationMethod.BadCop ||
               method == InterrogationMethod.Torture;
    }

    /// <summary>
    /// Gets the primary attribute used by this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <param name="useMight">Whether to use MIGHT instead of WILL (where applicable).</param>
    /// <returns>The attribute name to use.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static string GetAttribute(this InterrogationMethod method, bool useMight = false)
    {
        if (useMight && method.CanUseMight())
        {
            return "MIGHT";
        }

        return method switch
        {
            InterrogationMethod.GoodCop => "WILL",
            InterrogationMethod.BadCop => "WILL",
            InterrogationMethod.Deception => "WILL",
            InterrogationMethod.Bribery => "WILL",
            InterrogationMethod.Torture => "WILL",
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }

    /// <summary>
    /// Gets whether this method incurs Liar's Burden stress cost.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if the method incurs stress from deception.</returns>
    /// <remarks>
    /// Only the Deception method incurs Liar's Burden stress because it
    /// involves actively lying to the subject.
    /// </remarks>
    public static bool IncursLiarsBurden(this InterrogationMethod method)
    {
        return method == InterrogationMethod.Deception;
    }

    /// <summary>
    /// Gets whether this method is considered dangerous/extreme.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if the method is considered extreme.</returns>
    /// <remarks>
    /// Dangerous methods require confirmation before use and have
    /// severe consequences. Currently only Torture is dangerous.
    /// </remarks>
    public static bool IsDangerous(this InterrogationMethod method)
    {
        return method == InterrogationMethod.Torture;
    }

    /// <summary>
    /// Gets whether this method traumatizes the subject.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if using this method inflicts trauma.</returns>
    /// <remarks>
    /// Torture permanently inflicts the [Traumatized] condition on the
    /// subject, affecting all future interactions.
    /// </remarks>
    public static bool TraumatizesSubject(this InterrogationMethod method)
    {
        return method == InterrogationMethod.Torture;
    }

    /// <summary>
    /// Gets whether this method's usage caps the final reliability.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>True if using this method caps reliability at 60%.</returns>
    /// <remarks>
    /// Using Torture at ANY point during an interrogation caps the final
    /// information reliability at 60%, regardless of the primary method used.
    /// This reflects that torture corrupts the entire interrogation process.
    /// </remarks>
    public static bool CapsReliability(this InterrogationMethod method)
    {
        return method == InterrogationMethod.Torture;
    }

    /// <summary>
    /// Gets example dialogue or actions for this method.
    /// </summary>
    /// <param name="method">The interrogation method.</param>
    /// <returns>Array of example dialogue lines or action descriptions.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when an unknown method is provided.
    /// </exception>
    public static IReadOnlyList<string> GetExamples(this InterrogationMethod method)
    {
        return method switch
        {
            InterrogationMethod.GoodCop => new[]
            {
                "I understand your situation. Help me help you.",
                "No one has to know we talked. This stays between us.",
                "You seem like a reasonable person caught in bad circumstances."
            },
            InterrogationMethod.BadCop => new[]
            {
                "We can do this the easy way or the hard way.",
                "Your friends already talked. Your silence is pointless.",
                "You don't want to find out what happens if I lose my patience."
            },
            InterrogationMethod.Deception => new[]
            {
                "Your accomplice already confessed everything. Just confirm it.",
                "The guards intercepted your message. We know more than you think.",
                "I'm not your enemy here. The magistrate wants your head."
            },
            InterrogationMethod.Bribery => new[]
            {
                "Everyone has a price. Name yours.",
                "Fifty gold could change your circumstances.",
                "Think of what this gold could mean for your family."
            },
            InterrogationMethod.Torture => new[]
            {
                "This can stop whenever you decide to talk.",
                "[No dialogue - physical coercion]"
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(method),
                method,
                "Unknown interrogation method")
        };
    }
}
