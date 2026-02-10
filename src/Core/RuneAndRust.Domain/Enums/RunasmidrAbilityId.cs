namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Rúnasmiðr (Runesmith) specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Rúnasmiðr specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): InscribeRune, ReadTheMarks, RunestoneWard</item>
/// <item>Tier 2 (Discipline, 8+ PP): EmpoweredInscription, RunicTrap, DvergrTechniques</item>
/// <item>Tier 3 (Mastery, 16+ PP): MasterScrivener, LivingRunes</item>
/// <item>Capstone (Word of Power, 24+ PP): WordOfUnmaking</item>
/// </list>
/// <para>Follows the same pattern as <see cref="SkjaldmaerAbilityId"/> for consistency
/// across specialization implementations.</para>
/// </remarks>
public enum RunasmidrAbilityId
{
    // ===== Tier 1 — Foundation (0 PP, free on specialization selection) =====

    /// <summary>
    /// Tier 1 — Active ability applying temporary enhancement rune to weapon (+2 damage)
    /// or armor (+1 Defense) for 10 turns. Costs 3 AP and 1 Rune Charge.
    /// </summary>
    InscribeRune = 1,

    /// <summary>
    /// Tier 1 — Passive ability automatically identifying Jötun technology
    /// without requiring Perception or Lore checks.
    /// </summary>
    ReadTheMarks = 2,

    /// <summary>
    /// Tier 1 — Active ability creating a defensive ward that absorbs up to 10 damage.
    /// Costs 2 AP and 1 Rune Charge. Only one ward per character at a time.
    /// </summary>
    RunestoneWard = 3,

    // ===== Tier 2 — Discipline (8+ PP invested, 4 PP each to unlock, v0.20.2b) =====

    /// <summary>
    /// Tier 2 — Enhanced inscription adding +1d6 elemental damage to weapons.
    /// Costs 4 AP and 2 Rune Charges. Introduced in v0.20.2b.
    /// </summary>
    EmpoweredInscription = 4,

    /// <summary>
    /// Tier 2 — Triggered ground rune dealing 3d6 damage on enemy contact.
    /// Costs 3 AP and 2 Rune Charges. Introduced in v0.20.2b.
    /// </summary>
    RunicTrap = 5,

    /// <summary>
    /// Tier 2 — Passive ability reducing all crafting costs by 20%.
    /// Introduced in v0.20.2b.
    /// </summary>
    DvergrTechniques = 6,

    // ===== Tier 3 — Mastery (16+ PP invested, 5 PP each to unlock, v0.20.2c) =====

    /// <summary>
    /// Tier 3 — Passive ability doubling the duration of all rune inscriptions.
    /// Introduced in v0.20.2c.
    /// </summary>
    MasterScrivener = 7,

    /// <summary>
    /// Tier 3 — Active ability summoning 2 animated rune entities to attack enemies.
    /// Costs 4 AP and 3 Rune Charges. Introduced in v0.20.2c.
    /// </summary>
    LivingRunes = 8,

    // ===== Capstone — Word of Power (24+ PP invested, 6 PP to unlock, v0.20.2c) =====

    /// <summary>
    /// Capstone — Ultimate ability dispelling all magical effects in a 4-space radius.
    /// Costs 5 AP and 4 Rune Charges. Once per combat. Introduced in v0.20.2c.
    /// </summary>
    WordOfUnmaking = 9
}
