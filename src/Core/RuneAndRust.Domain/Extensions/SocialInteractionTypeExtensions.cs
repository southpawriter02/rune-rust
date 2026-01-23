namespace RuneAndRust.Domain.Extensions;

using Microsoft.Extensions.Logging;
using RuneAndRust.Domain.Enums;

/// <summary>
/// Extension methods for <see cref="SocialInteractionType"/> enum.
/// </summary>
/// <remarks>
/// <para>
/// Provides utility methods for determining interaction type characteristics
/// including attribute usage, opposed roll detection, stress costs, reputation
/// costs, multi-round handling, and fumble type mapping.
/// </para>
/// </remarks>
public static class SocialInteractionTypeExtensions
{
    /// <summary>
    /// Gets the base attribute used for this interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>
    /// The attribute ID (\"will\" for most, \"might\" as alternative for intimidation).
    /// </returns>
    public static string GetDefaultAttribute(this SocialInteractionType interactionType)
    {
        // All social interactions default to WILL
        return "will";
    }

    /// <summary>
    /// Gets whether this interaction type allows attribute choice.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if player can choose between attributes.</returns>
    /// <remarks>
    /// Currently only Intimidation allows choosing between MIGHT (physical threat)
    /// and WILL (mental pressure).
    /// </remarks>
    public static bool AllowsAttributeChoice(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Intimidation;
    }

    /// <summary>
    /// Gets the alternate attribute available for this interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The alternate attribute ID, or null if no alternate is available.</returns>
    public static string? GetAlternateAttribute(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Intimidation => "might",
            _ => null
        };
    }

    /// <summary>
    /// Gets whether this interaction type uses opposed rolls.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if the check is opposed by NPC's resistance.</returns>
    /// <remarks>
    /// Opposed interactions roll against the target's attribute rather than a fixed DC.
    /// Deception is opposed by target's WITS, Interrogation by target's WILL.
    /// </remarks>
    public static bool IsOpposed(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Deception ||
               interactionType == SocialInteractionType.Interrogation;
    }

    /// <summary>
    /// Gets the attribute used by the target to oppose this interaction.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The opposing attribute ID, or null if not an opposed check.</returns>
    public static string? GetOpposingAttribute(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Deception => "wits",
            SocialInteractionType.Interrogation => "will",
            _ => null
        };
    }

    /// <summary>
    /// Gets whether this interaction type incurs stress cost.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if the interaction always costs Psychic Stress.</returns>
    /// <remarks>
    /// Deception always incurs Psychic Stress due to the Liar's Burden mechanic.
    /// The stress cost varies based on success or failure of the check.
    /// </remarks>
    public static bool HasStressCost(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Deception;
    }

    /// <summary>
    /// Gets the base stress cost for a successful interaction of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The Psychic Stress cost on success, or 0 if none.</returns>
    public static int GetSuccessStressCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Deception => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the stress cost for a failed interaction of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The Psychic Stress cost on failure, or 0 if none.</returns>
    public static int GetFailureStressCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Deception => 3,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the stress cost for a fumbled interaction of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The Psychic Stress cost on fumble.</returns>
    public static int GetFumbleStressCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Deception => 8,
            SocialInteractionType.Intimidation => 5,
            _ => 0
        };
    }

    /// <summary>
    /// Gets whether this interaction type always costs reputation.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if success still costs reputation.</returns>
    /// <remarks>
    /// Intimidation always carries a reputation cost as witnesses will remember
    /// the player's threatening behavior, even if the target complies.
    /// </remarks>
    public static bool AlwaysCostsReputation(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Intimidation;
    }

    /// <summary>
    /// Gets the reputation cost for a successful interaction of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The reputation change (negative = cost).</returns>
    public static int GetSuccessReputationCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Intimidation => -5,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the reputation cost for a critical success of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The reputation change (negative = cost).</returns>
    public static int GetCriticalSuccessReputationCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Intimidation => -3, // Reduced cost for clean execution
            _ => 0
        };
    }

    /// <summary>
    /// Gets the reputation cost for a failed interaction of this type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The reputation change (negative = cost).</returns>
    public static int GetFailureReputationCost(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Intimidation => -10, // Heavy cost for failed threats
            _ => 0
        };
    }

    /// <summary>
    /// Gets whether this interaction type uses multi-round mechanics.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if the interaction spans multiple rounds.</returns>
    /// <remarks>
    /// Negotiation uses a position track system over multiple rounds.
    /// Interrogation depletes subject resistance over multiple attempts.
    /// </remarks>
    public static bool IsMultiRound(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Negotiation ||
               interactionType == SocialInteractionType.Interrogation;
    }

    /// <summary>
    /// Gets whether this interaction type requires cultural awareness.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>True if cultural context affects the interaction.</returns>
    public static bool RequiresCulturalContext(this SocialInteractionType interactionType)
    {
        return interactionType == SocialInteractionType.Protocol;
    }

    /// <summary>
    /// Gets a human-readable description of the interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>A descriptive string for UI display.</returns>
    public static string GetDescription(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Persuasion => "Honest convincing through reason or appeal",
            SocialInteractionType.Deception => "Lying or misleading the target",
            SocialInteractionType.Intimidation => "Coercion through threat or presence",
            SocialInteractionType.Negotiation => "Bargaining toward mutual agreement",
            SocialInteractionType.Protocol => "Following cultural formalities",
            SocialInteractionType.Interrogation => "Extracting information from a subject",
            _ => "Unknown interaction type"
        };
    }

    /// <summary>
    /// Gets a short display name for the interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The display name for UI.</returns>
    public static string GetDisplayName(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Persuasion => "Persuasion",
            SocialInteractionType.Deception => "Deception",
            SocialInteractionType.Intimidation => "Intimidation",
            SocialInteractionType.Negotiation => "Negotiation",
            SocialInteractionType.Protocol => "Protocol",
            SocialInteractionType.Interrogation => "Interrogation",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the fumble type associated with this interaction.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The <see cref="FumbleType"/> for catastrophic failures.</returns>
    /// <remarks>
    /// Each social interaction type has a specific fumble consequence:
    /// <list type="bullet">
    ///   <item><description>Persuasion: Trust Shattered - locks persuasion with target</description></item>
    ///   <item><description>Deception: Lie Exposed - may trigger combat or exile</description></item>
    ///   <item><description>Intimidation: Challenge Accepted - immediate combat with buffed enemy</description></item>
    /// </list>
    /// </remarks>
    public static FumbleType GetFumbleType(this SocialInteractionType interactionType)
    {
        return interactionType switch
        {
            SocialInteractionType.Persuasion => FumbleType.TrustShattered,
            SocialInteractionType.Deception => FumbleType.LieExposed,
            SocialInteractionType.Intimidation => FumbleType.ChallengeAccepted,
            SocialInteractionType.Negotiation => FumbleType.TrustShattered, // Negotiation collapse
            SocialInteractionType.Protocol => FumbleType.TrustShattered,    // Cultural offense
            SocialInteractionType.Interrogation => FumbleType.TrustShattered, // Subject clams up
            _ => FumbleType.TrustShattered
        };
    }

    /// <summary>
    /// Gets the skill ID used for this interaction type.
    /// </summary>
    /// <param name="interactionType">The interaction type.</param>
    /// <returns>The skill identifier.</returns>
    public static string GetSkillId(this SocialInteractionType interactionType)
    {
        // All social interactions use the Rhetoric skill
        return "rhetoric";
    }
}
