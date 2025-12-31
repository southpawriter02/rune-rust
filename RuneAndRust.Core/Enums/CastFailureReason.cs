namespace RuneAndRust.Core.Enums;

/// <summary>
/// Enumerates the reasons why a spell cast attempt might fail.
/// Used by MagicService to provide meaningful feedback to the player.
/// </summary>
/// <remarks>
/// See: v0.4.3c (The Incantation) for implementation details.
/// Validation order in MagicService.CanCast:
/// 1. NotMagicUser - Archetype check
/// 2. InsufficientAP - Resource check
/// 3. InvalidTarget - Target type mismatch
/// 4. OutOfRange - Distance check
/// 5. TargetDead - Alive check
/// 6. Silenced - Status effect check
/// 7. ConcentrationConflict - Existing concentration check
/// 8. OnCooldown - Cooldown check
/// 9. SpellNotKnown - Spell knowledge check
/// 10. InvalidCombatState - Combat state validation
/// </remarks>
public enum CastFailureReason
{
    /// <summary>
    /// No failure - cast is valid.
    /// </summary>
    None = 0,

    /// <summary>
    /// The caster's archetype cannot use magic (must be Adept or Mystic).
    /// </summary>
    NotMagicUser = 1,

    /// <summary>
    /// The caster does not have enough Aether Points to cast this spell.
    /// </summary>
    InsufficientAP = 2,

    /// <summary>
    /// The spell cannot target the specified target type (Self, SingleEnemy, etc.).
    /// </summary>
    InvalidTarget = 3,

    /// <summary>
    /// The target is out of range for this spell.
    /// </summary>
    OutOfRange = 4,

    /// <summary>
    /// The target is dead and cannot be affected by this spell.
    /// </summary>
    TargetDead = 5,

    /// <summary>
    /// The caster is silenced and cannot cast spells.
    /// </summary>
    Silenced = 6,

    /// <summary>
    /// The caster is already concentrating on another spell.
    /// </summary>
    ConcentrationConflict = 7,

    /// <summary>
    /// The spell is on cooldown and cannot be cast yet.
    /// </summary>
    OnCooldown = 8,

    /// <summary>
    /// The caster does not know this spell.
    /// </summary>
    SpellNotKnown = 9,

    /// <summary>
    /// Combat state is invalid for spell casting (e.g., not in combat, not player's turn).
    /// </summary>
    InvalidCombatState = 10,

    /// <summary>
    /// The caster's soul has been consumed by corruption (75+ corruption).
    /// Magic no longer answers. Added in v0.4.3d (The Backlash).
    /// </summary>
    SoulLost = 11
}
