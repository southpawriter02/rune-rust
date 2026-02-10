namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes the different types of runes and their mechanical effects
/// within the Rúnasmiðr specialization system.
/// </summary>
/// <remarks>
/// <para>Each rune type corresponds to a distinct inscription effect:</para>
/// <list type="bullet">
/// <item><see cref="Enhancement"/> — Weapon damage bonus (Tier 1: +2 damage)</item>
/// <item><see cref="Protection"/> — Armor defense bonus (Tier 1: +1 Defense)</item>
/// <item><see cref="Elemental"/> — Elemental damage bonus (Tier 2: +1d6 elemental, v0.20.2b)</item>
/// <item><see cref="Ward"/> — Defensive damage absorption (Tier 1: absorbs up to 10 damage)</item>
/// <item><see cref="Trap"/> — Triggered ground effect trap (Tier 2: 3d6 damage, v0.20.2b)</item>
/// </list>
/// <para>Rune types are used by <c>InscribedRune</c> and <c>RunestoneWard</c> value objects
/// to determine the effect applied when inscribing equipment or creating wards.</para>
/// </remarks>
public enum RuneType
{
    /// <summary>
    /// Weapon damage bonus inscription.
    /// Tier 1: +2 damage for 10 turns.
    /// Applied when inscribing a weapon via Inscribe Rune ability.
    /// </summary>
    Enhancement = 1,

    /// <summary>
    /// Armor defense bonus inscription.
    /// Tier 1: +1 Defense for 10 turns.
    /// Applied when inscribing armor via Inscribe Rune ability.
    /// </summary>
    Protection = 2,

    /// <summary>
    /// Elemental damage bonus inscription.
    /// Tier 2: +1d6 elemental damage (Fire, Cold, Lightning, or Aetheric).
    /// Introduced in v0.20.2b via Empowered Inscription ability.
    /// </summary>
    Elemental = 3,

    /// <summary>
    /// Defensive ward that absorbs incoming damage.
    /// Tier 1: absorbs up to 10 damage before HP is affected.
    /// Created via Runestone Ward ability.
    /// </summary>
    Ward = 4,

    /// <summary>
    /// Triggered trap inscription placed on ground.
    /// Tier 2: deals 3d6 damage when triggered by enemy movement.
    /// Introduced in v0.20.2b via Runic Trap ability.
    /// </summary>
    Trap = 5
}
