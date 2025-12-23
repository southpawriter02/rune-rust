using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Core.Models.Combat;

/// <summary>
/// Immutable blueprint defining base stats for an enemy species.
/// Templates are used by EnemyFactory to instantiate Enemy entities with scaling and variance.
/// </summary>
/// <param name="Id">Unique template identifier (e.g., "und_draugr_01").</param>
/// <param name="Name">Display name shown in combat UI.</param>
/// <param name="Description">AAM-VOICE compliant flavor text for Bestiary entries.</param>
/// <param name="Archetype">Combat role archetype for AI behavior selection.</param>
/// <param name="Tier">Threat classification for stat scaling.</param>
/// <param name="BaseHp">Base hit points before scaling.</param>
/// <param name="BaseStamina">Base stamina before scaling.</param>
/// <param name="BaseSoak">Base armor soak value (damage reduction).</param>
/// <param name="Attributes">Base attribute scores (Sturdiness, Might, Wits, Will, Finesse).</param>
/// <param name="WeaponDamageDie">Damage die size for attacks (4 = d4, 6 = d6, etc.).</param>
/// <param name="WeaponName">Display name for the enemy's weapon/attack.</param>
/// <param name="Tags">Tags for status effect interactions (e.g., "Mechanical" = Bleed immune).</param>
/// <param name="AbilityNames">Ability identifiers to hydrate from repository at creation time (v0.2.4a).</param>
/// <param name="LootTableId">Optional reference to loot table for drops.</param>
public record EnemyTemplate(
    string Id,
    string Name,
    string Description,
    EnemyArchetype Archetype,
    ThreatTier Tier,
    int BaseHp,
    int BaseStamina,
    int BaseSoak,
    Dictionary<CharacterAttribute, int> Attributes,
    int WeaponDamageDie,
    string WeaponName,
    List<string> Tags,
    List<string> AbilityNames,
    string? LootTableId = null
);
