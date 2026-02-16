namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the recovery condition of a target character for Emergency Surgery bonus calculation.
/// Used by the Bone-Setter's Emergency Surgery ability (Tier 2) to determine additional
/// healing based on the target's current state.
/// </summary>
/// <remarks>
/// <para>Recovery bonus values by condition:</para>
/// <list type="bullet">
/// <item><see cref="Active"/>: No bonus (+0) — target is conscious and fighting</item>
/// <item><see cref="Incapacitated"/>: Minor bonus (+1) — target is stunned/prone</item>
/// <item><see cref="Recovering"/>: Standard bonus (+3) — target is unconscious but stable</item>
/// <item><see cref="Dying"/>: Maximum bonus (+4) — target is at death's door</item>
/// <item><see cref="Dead"/>: Cannot target — beyond medical intervention</item>
/// </list>
/// </remarks>
public enum RecoveryCondition
{
    /// <summary>
    /// Target is conscious and actively fighting or moving.
    /// No Emergency Surgery recovery bonus applied.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Target is conscious but incapacitated (stunned, prone, restrained, etc.).
    /// Emergency Surgery grants +1 recovery bonus.
    /// </summary>
    Incapacitated = 1,

    /// <summary>
    /// Target is unconscious but stable (not at death's door).
    /// Emergency Surgery grants +3 recovery bonus — brings unconscious allies back to fighting condition.
    /// </summary>
    Recovering = 2,

    /// <summary>
    /// Target is at death's door (0 HP, dying condition).
    /// Emergency Surgery grants maximum +4 recovery bonus — last-ditch intervention to prevent death.
    /// </summary>
    Dying = 3,

    /// <summary>
    /// Target is deceased and beyond Emergency Surgery intervention.
    /// Cannot be targeted by Emergency Surgery.
    /// </summary>
    Dead = 4
}
