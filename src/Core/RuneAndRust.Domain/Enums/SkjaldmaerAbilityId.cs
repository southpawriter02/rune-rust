namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Enumeration of all Skjaldmær (Shield-Maiden) specialization abilities.
/// Used for type-safe ability identification and routing throughout the system.
/// </summary>
/// <remarks>
/// <para>The Skjaldmær specialization has 9 abilities across 4 tiers:</para>
/// <list type="bullet">
/// <item>Tier 1 (Foundation, 0 PP): ShieldWall, Intercept, Bulwark</item>
/// <item>Tier 2 (Discipline, 8+ PP): HoldTheLine, CounterShield, Rally</item>
/// <item>Tier 3 (Mastery, 16+ PP): Unbreakable, GuardiansSacrifice</item>
/// <item>Capstone (The Wall, 24+ PP): TheWallLives</item>
/// </list>
/// </remarks>
public enum SkjaldmaerAbilityId
{
    /// <summary>Tier 1 — Defensive stance granting +3 Defense to self and +1 to adjacent allies.</summary>
    ShieldWall = 1,

    /// <summary>Tier 1 — Reaction ability redirecting nearby ally attacks to self (1 Block Charge).</summary>
    Intercept = 2,

    /// <summary>Tier 1 — Passive ability granting +5 Max HP per Block Charge held.</summary>
    Bulwark = 3,

    /// <summary>Tier 2 — Active ability preventing enemies from moving through character's space (2 turns).</summary>
    HoldTheLine = 4,

    /// <summary>Tier 2 — Reaction ability dealing 1d6 damage on successful block.</summary>
    CounterShield = 5,

    /// <summary>Tier 2 — Active ability granting +2 save bonus to all allies within 6 spaces.</summary>
    Rally = 6,

    /// <summary>Tier 3 — Passive ability reducing all damage taken by 3 (minimum 1 damage).</summary>
    Unbreakable = 7,

    /// <summary>Tier 3 — Reaction ability absorbing 100% of ally damage for 2 Block Charges.</summary>
    GuardiansSacrifice = 8,

    /// <summary>Capstone — Ultimate ability preventing HP from dropping below 1 for 3 turns (once per combat).</summary>
    TheWallLives = 9
}
