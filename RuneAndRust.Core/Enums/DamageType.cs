using RuneAndRust.Core.Attributes;

namespace RuneAndRust.Core.Enums;

/// <summary>
/// Defines the types of damage in combat (v0.3.3a).
/// Physical damage is reduced by armor soak; elemental damage bypasses soak.
/// </summary>
public enum DamageType
{
    /// <summary>
    /// Physical damage from weapons and impacts. Reduced by armor soak.
    /// </summary>
    [GameDocument(
        "Physical Damage",
        "Harm from blunt force, slashing edges, and piercing points. Armor and thick hide offer meaningful protection against physical trauma. The most common form of injury in the wastes.")]
    Physical = 0,

    /// <summary>
    /// Fire/heat damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Fire Damage",
        "Searing heat that ignores conventional armor. Flames consume flesh regardless of protective layers. Sources include Dvergr forges, volcanic vents, and creatures touched by elemental fire.")]
    Fire = 1,

    /// <summary>
    /// Cold/ice damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Ice Damage",
        "Killing cold that bypasses physical defenses. The chill reaches bone despite armor, slowing and freezing vital organs. Common in the frozen reaches where Hrimthursar dwell.")]
    Ice = 2,

    /// <summary>
    /// Lightning/electrical damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Lightning Damage",
        "Electrical discharge that travels through metal and flesh alike. Armor offers no protection and may conduct the shock more efficiently. Pre-Glitch facilities and storm-touched entities wield this force.")]
    Lightning = 3,

    /// <summary>
    /// Poison/toxin damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Poison Damage",
        "Toxins that work from within, ignoring external defenses. The damage manifests through internal systems regardless of armor. Venomous creatures and contaminated environments deliver this harm.")]
    Poison = 4,

    /// <summary>
    /// Acid/corrosive damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Acid Damage",
        "Corrosive substances that dissolve both armor and flesh. Protective gear may briefly slow the damage but offers no true defense. Certain Blight-creatures secrete such compounds.")]
    Acid = 5,

    /// <summary>
    /// Psychic/mental damage. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Psychic Damage",
        "Assault upon the mind itself, bypassing all physical protection. The harm manifests as overwhelming terror, mental intrusion, or forced memory. Will rather than Sturdiness determines one's resilience.")]
    Psychic = 6,

    /// <summary>
    /// Blight/corruption damage from Runic Blight. Bypasses armor soak.
    /// </summary>
    [GameDocument(
        "Blight Damage",
        "The Runic corruption made manifest as direct harm. Neither armor nor distance protects against the Blight's touch. Victims report reality itself rejecting their existence in the affected area.")]
    Blight = 7
}
