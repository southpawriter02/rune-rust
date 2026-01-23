namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Identifies Myrk-gengr specialization abilities.
/// </summary>
/// <remarks>
/// <para>
/// The Myrk-gengr (Old Norse: "shadow-walker") specialization focuses on
/// stealth, infiltration, and moving unseen through the shadows of the Sprawl.
/// </para>
/// <para>
/// These abilities enhance the base stealth movement (v0.15.2d) system
/// and interact with the [Hidden] status effect.
/// </para>
/// <para>
/// <b>v0.15.2g:</b> Initial implementation of Myrk-gengr abilities.
/// </para>
/// </remarks>
public enum MyrkengrAbility
{
    /// <summary>
    /// Enter [Hidden] without using an action.
    /// Type: Triggered
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: When entering an area with shadows or low lighting,
    /// automatically enter [Hidden] without spending an action.
    /// A stealth check is still required.
    /// </para>
    /// <para>
    /// Trigger: Entering shadows, dim light, or darkness.
    /// Requirement: Cannot be actively observed by enemies.
    /// </para>
    /// </remarks>
    SlipIntoShadow = 0,

    /// <summary>
    /// Stay [Hidden] after attacking once per encounter.
    /// Type: Reactive (1/encounter)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: After making an attack from [Hidden], you may choose
    /// to remain [Hidden] instead of being revealed.
    /// </para>
    /// <para>
    /// Uses: Once per encounter. Resets at end of combat.
    /// Subsequent attacks break [Hidden] normally.
    /// </para>
    /// </remarks>
    GhostlyForm = 1,

    /// <summary>
    /// Grant party members +2d10 on Passive Stealth.
    /// Type: Active (Concentration)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: While maintaining concentration, party members within
    /// 30 feet gain +2d10 to their Passive Stealth checks.
    /// </para>
    /// <para>
    /// Duration: Maintained until concentration is broken.
    /// Party stealth still uses weakest-link rule, but all gain bonus.
    /// </para>
    /// </remarks>
    CloakTheParty = 2,

    /// <summary>
    /// Auto-enter [Hidden] in [Psychic Resonance] zones.
    /// Type: Passive
    /// </summary>
    /// <remarks>
    /// <para>
    /// Effect: When entering a zone with the [Psychic Resonance]
    /// environmental tag, automatically enter [Hidden] without
    /// an action or check.
    /// </para>
    /// <para>
    /// Trigger: Entering [Psychic Resonance] zone.
    /// The psychic static provides natural concealment.
    /// [Hidden] breaks normally when attacking or detected.
    /// </para>
    /// </remarks>
    OneWithTheStatic = 3
}
