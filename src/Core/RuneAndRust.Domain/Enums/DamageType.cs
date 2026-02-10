// ═══════════════════════════════════════════════════════════════════════════════
// DamageType.cs
// Categorizes the types of damage that can be dealt or received in combat.
// Used by the Exploit Weakness vulnerability analysis system.
// Version: 0.20.3b
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the categories of damage available in the combat system.
/// </summary>
/// <remarks>
/// <para>
/// Used by the Jötun-Reader's <see cref="JotunReaderAbilityId.ExploitWeakness"/>
/// ability to classify enemy vulnerabilities, resistances, and immunities.
/// </para>
/// <list type="bullet">
///   <item><description><b>Physical:</b> Slashing, Piercing, Blunt</description></item>
///   <item><description><b>Elemental:</b> Fire, Cold, Lightning</description></item>
///   <item><description><b>Exotic:</b> Poison, Necrotic, Radiant, Psychic</description></item>
/// </list>
/// </remarks>
public enum DamageType
{
    /// <summary>Cutting or slicing damage from bladed weapons.</summary>
    Slashing,

    /// <summary>Stabbing or puncturing damage from pointed weapons.</summary>
    Piercing,

    /// <summary>Crushing or bludgeoning damage from heavy weapons.</summary>
    Blunt,

    /// <summary>Heat and flame damage.</summary>
    Fire,

    /// <summary>Frost and ice damage.</summary>
    Cold,

    /// <summary>Electrical and storm damage.</summary>
    Lightning,

    /// <summary>Toxin and venom damage.</summary>
    Poison,

    /// <summary>Death and decay energy damage.</summary>
    Necrotic,

    /// <summary>Holy and light energy damage.</summary>
    Radiant,

    /// <summary>Mental and psionic damage.</summary>
    Psychic
}
