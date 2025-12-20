namespace RuneAndRust.Core.Enums;

/// <summary>
/// Combat role archetype that drives AI behavior patterns in v0.2.2b.
/// Determines attack priority, target selection, and ability usage.
/// </summary>
public enum EnemyArchetype
{
    /// <summary>
    /// High HP/Soak, Low Damage. Prioritizes protecting allies and absorbing hits.
    /// Examples: Haugbui Laborer, Shield-Drone.
    /// </summary>
    Tank,

    /// <summary>
    /// Balanced offense and defense. Reliable damage dealers.
    /// Examples: Rusted Draugr, Rust-Clan Warrior.
    /// </summary>
    DPS,

    /// <summary>
    /// High Damage, Low HP. Hits hard but fragile. Prioritizes weak targets.
    /// Examples: Ash-Vargr, Scrap-Hound.
    /// </summary>
    GlassCannon,

    /// <summary>
    /// Applies buffs/debuffs. Prioritizes enhancing allies or weakening enemies.
    /// Examples: Rust-Clan Engineer, Blight-Priest.
    /// </summary>
    Support,

    /// <summary>
    /// Low individual stats, group tactics. Strength in numbers.
    /// Examples: Utility Servitor, Scrap-Mite swarm.
    /// </summary>
    Swarm,

    /// <summary>
    /// Ranged/AoE attacks. Stays at distance, targets clusters.
    /// Examples: Rust-Witch, Corrupted Turret.
    /// </summary>
    Caster,

    /// <summary>
    /// Multi-phase encounters with unique mechanics. Reserved for major threats.
    /// Examples: Haugbui Warlord, The Corruptor.
    /// </summary>
    Boss
}
