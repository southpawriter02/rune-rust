namespace RuneAndRust.Core.Enums;

/// <summary>
/// Identifies distinct specialization paths available to characters.
/// Each specialization is tied to a specific Archetype and provides
/// access to a unique ability tree.
/// </summary>
/// <remarks>
/// See: SPEC-SPECIALIZATION-001 for design documentation.
/// See: v0.4.1a for implementation details.
///
/// Enum value ranges are reserved by Archetype:
/// - 0-9: Warrior specializations
/// - 10-19: Skirmisher specializations
/// - 20-29: Adept specializations
/// - 30-39: Mystic specializations
/// </remarks>
public enum SpecializationType
{
    // ═══════════════════════════════════════════════════════════════════════
    // Warrior Specializations (0-9)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Rage-focused damage dealer who trades defense for overwhelming offense.
    /// Emphasizes self-damage abilities and berserker mechanics.
    /// </summary>
    Berserkr = 0,

    /// <summary>
    /// Defense-focused protector who shields allies and controls the battlefield.
    /// Emphasizes damage mitigation and taunt mechanics.
    /// </summary>
    Guardian = 1,

    // ═══════════════════════════════════════════════════════════════════════
    // Skirmisher Specializations (10-19)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Buff/debuff support specialist who uses shouts and chants.
    /// Emphasizes team-wide effects and morale manipulation.
    /// </summary>
    Skald = 10,

    /// <summary>
    /// Trap and positioning control specialist.
    /// Emphasizes battlefield manipulation and ambush mechanics.
    /// </summary>
    Trapper = 11,

    // ═══════════════════════════════════════════════════════════════════════
    // Adept Specializations (20-29) - Reserved for future implementation
    // ═══════════════════════════════════════════════════════════════════════

    // /// <summary>Scholar specialization placeholder.</summary>
    // Scholar = 20,

    // /// <summary>Artificer specialization placeholder.</summary>
    // Artificer = 21,

    // ═══════════════════════════════════════════════════════════════════════
    // Mystic Specializations (30-39) - Reserved for future implementation
    // ═══════════════════════════════════════════════════════════════════════

    // /// <summary>Runecaster specialization placeholder.</summary>
    // Runecaster = 30,

    // /// <summary>Spiritcaller specialization placeholder.</summary>
    // Spiritcaller = 31,
}
