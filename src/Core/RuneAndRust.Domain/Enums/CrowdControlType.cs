namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of crowd control effect types that abilities like Avatar of Destruction
/// can grant immunity to. Used by the Berserkr specialization's Capstone ability system
/// to determine which crowd control effects are negated during the Avatar transformation.
/// </summary>
/// <remarks>
/// Introduced in v0.20.5c as part of the Berserkr Tier 3 &amp; Capstone Abilities implementation.
/// Each value represents a distinct category of crowd control that enemies can impose
/// on a character. Avatar of Destruction grants immunity to all 10 types simultaneously.
/// </remarks>
public enum CrowdControlType
{
    /// <summary>
    /// Complete incapacitation — the character cannot act or move.
    /// Examples: thunderclap, shield bash, concussive blow.
    /// </summary>
    Stun = 1,

    /// <summary>
    /// Immobilization — the character cannot move but can still act.
    /// Examples: root spell, grapple, vine snare.
    /// </summary>
    Root = 2,

    /// <summary>
    /// Fear effect — the character must flee or cower, unable to act normally.
    /// Examples: intimidation, terror aura, horrifying visage.
    /// </summary>
    Fear = 3,

    /// <summary>
    /// Charm effect — the character's attacks are altered or redirected.
    /// Examples: charm person, dominate, beguiling presence.
    /// </summary>
    Charm = 4,

    /// <summary>
    /// Magical paralysis — the character cannot move or act (like stun but magical).
    /// Examples: hold person, petrifying gaze, nerve toxin.
    /// </summary>
    Paralyze = 5,

    /// <summary>
    /// Movement speed reduction — the character can move but at reduced speed.
    /// Examples: slow spell, frost aura, exhaustion.
    /// </summary>
    Slow = 6,

    /// <summary>
    /// Knocked down — the character is on the ground with disadvantage on attacks until standing.
    /// Examples: trip attack, earthquake, force slam.
    /// </summary>
    Prone = 7,

    /// <summary>
    /// Forcibly moved — the character is pushed, pulled, or teleported against their will.
    /// Examples: push attack, gravitational pull, knockback.
    /// </summary>
    ForcedMovement = 8,

    /// <summary>
    /// Unconscious — the character cannot act and is vulnerable to attacks.
    /// Examples: sleep spell, tranquilizer, hypnotic pattern.
    /// </summary>
    Sleep = 9,

    /// <summary>
    /// Confused — the character's actions are randomized or misdirected.
    /// Examples: confusion spell, madness aura, psychic disruption.
    /// </summary>
    Confusion = 10
}
