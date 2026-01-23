namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes social interactions for Rhetoric skill checks.
/// </summary>
/// <remarks>
/// <para>
/// Each interaction type has distinct mechanics, modifiers, and potential
/// consequences. The type determines which subsystem processes the check
/// and what outcomes are possible.
/// </para>
/// <para>
/// All social interactions use the Rhetoric skill with WILL as the base
/// attribute, modified by the specific interaction type's rules.
/// </para>
/// </remarks>
public enum SocialInteractionType
{
    /// <summary>
    /// Honest convincing through logic, emotion, or mutual benefit.
    /// </summary>
    /// <remarks>
    /// <para>Uses WILL + Rhetoric. Disposition modifiers apply fully.</para>
    /// <para>Fumble: [Trust Shattered] - All persuasion locked with target.</para>
    /// </remarks>
    /// <example>
    /// "Convince the guard to let us pass."
    /// "Persuade the merchant to lower the price."
    /// </example>
    Persuasion = 0,

    /// <summary>
    /// Lying, misleading, or presenting false information as truth.
    /// </summary>
    /// <remarks>
    /// <para>Opposed roll: WILL + Rhetoric vs. NPC's WITS.</para>
    /// <para>Incurs Psychic Stress regardless of outcome (Liar's Burden).</para>
    /// <para>Fumble: [Lie Exposed] - May trigger combat, exile, quest failure.</para>
    /// </remarks>
    /// <example>
    /// "Claim to be an official inspector."
    /// "Deny involvement in the crime."
    /// </example>
    Deception = 1,

    /// <summary>
    /// Coercion through threat of violence, social ruin, or other harm.
    /// </summary>
    /// <remarks>
    /// <para>Uses MIGHT or WILL + Rhetoric (player choice).</para>
    /// <para>Always costs reputation even on success.</para>
    /// <para>Fumble: [Challenge Accepted] - Immediate combat with buffed enemy.</para>
    /// </remarks>
    /// <example>
    /// "Threaten to expose his secret."
    /// "Physically menace the shopkeeper."
    /// </example>
    Intimidation = 2,

    /// <summary>
    /// Multi-round bargaining to reach a mutually acceptable agreement.
    /// </summary>
    /// <remarks>
    /// <para>Uses position track system with multiple rounds.</para>
    /// <para>Success/failure moves positions until deal or collapse.</para>
    /// <para>Concessions can grant bonuses to subsequent checks.</para>
    /// </remarks>
    /// <example>
    /// "Negotiate trade rights with the guild."
    /// "Broker a ceasefire between factions."
    /// </example>
    Negotiation = 3,

    /// <summary>
    /// Following cultural formalities required by specific groups.
    /// </summary>
    /// <remarks>
    /// <para>Culture-specific DCs and requirements.</para>
    /// <para>Failure may offend and cause permanent disposition loss.</para>
    /// <para>Examples: Dvergr Logic-Chain, Utgard Veil-Speech.</para>
    /// </remarks>
    /// <example>
    /// "Follow Dvergr greeting protocol."
    /// "Speak in Utgard Veil-Speech."
    /// </example>
    Protocol = 4,

    /// <summary>
    /// Extracting information from an unwilling or reluctant subject.
    /// </summary>
    /// <remarks>
    /// <para>Multiple methods available with different costs/reliability.</para>
    /// <para>Subject has resistance that must be depleted through checks.</para>
    /// <para>Method choice affects information reliability.</para>
    /// </remarks>
    /// <example>
    /// "Interrogate the prisoner about the hideout."
    /// "Extract information using bribery."
    /// </example>
    Interrogation = 5
}
