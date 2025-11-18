// ==============================================================================
// v0.38.7: Galdr & Ability Flavor Text Integration
// AbilityDatabase_GaldrExtensions.cs
// ==============================================================================
// Purpose: Extends AbilityDatabase with rune school and category mappings
// Usage: Call ConfigureGaldrProperties() after ability creation
// Integration: Enables automatic Galdr flavor text generation
// ==============================================================================

using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Extension methods for configuring Galdr and ability flavor properties.
/// </summary>
public static class AbilityDatabaseGaldrExtensions
{
    /// <summary>
    /// Configures Galdr and ability flavor properties for all abilities.
    /// Call this after ability initialization to enable flavor text generation.
    /// </summary>
    public static void ConfigureGaldrProperties(this AbilityDatabase database)
    {
        // === WARRIOR ABILITIES (Non-Galdr) ===
        ConfigureWarriorAbility(database, "Crushing Blow", "WeaponArt");
        ConfigureWarriorAbility(database, "Rally Cry", "TacticalAbility");
        ConfigureWarriorAbility(database, "Whirlwind Strike", "WeaponArt");
        ConfigureWarriorAbility(database, "Second Wind", "ResourceAbility");
        ConfigureWarriorAbility(database, "Armor Breaker", "WeaponArt");
        ConfigureWarriorAbility(database, "Intimidating Presence", "TacticalAbility");
        ConfigureWarriorAbility(database, "Unstoppable", "ResourceAbility");
        ConfigureWarriorAbility(database, "Execute", "WeaponArt");
        ConfigureWarriorAbility(database, "Bulwark", "DefensiveAbility");
        ConfigureWarriorAbility(database, "Titan's Strength", "ResourceAbility");
        ConfigureWarriorAbility(database, "Last Stand", "DefensiveAbility");

        // === HERETICAL ABILITIES (Non-Galdr) ===
        ConfigureWarriorAbility(database, "Embrace the Machine", "ResourceAbility");
        ConfigureWarriorAbility(database, "Jotun Reader's Gift", "TacticalAbility");
        ConfigureWarriorAbility(database, "Symbiotic Regeneration", "ResourceAbility");

        // === MYSTIC ABILITIES (Galdr - to be added) ===
        // These would be configured when Mystic abilities are added to AbilityDatabase
        // Examples:
        // ConfigureGaldrAbility(database, "Flame Bolt", "Fehu", "Fire");
        // ConfigureGaldrAbility(database, "Frost Lance", "Thurisaz", "Ice");
        // ConfigureGaldrAbility(database, "Lightning Bolt", "Ansuz", "Lightning");
        // ConfigureGaldrAbility(database, "Healing Chant", "Berkanan", "Healing");
        // ConfigureGaldrAbility(database, "Rune Ward", "Tiwaz", null);
        // ConfigureGaldrAbility(database, "Drain Life", "Naudiz", "Shadow");
        // ConfigureGaldrAbility(database, "Frozen Time", "Isa", "Ice");
        // ConfigureGaldrAbility(database, "Cleansing Wave", "Laguz", "Water");
    }

    /// <summary>
    /// Configures a non-Galdr (Warrior/Adept) ability with category.
    /// </summary>
    private static void ConfigureWarriorAbility(
        AbilityDatabase database,
        string abilityName,
        string abilityCategory)
    {
        var ability = database.GetAbility(abilityName);
        if (ability != null)
        {
            ability.RuneSchool = null; // Not a Galdr ability
            ability.AbilityCategory = abilityCategory;
            ability.Element = null; // No elemental association
        }
    }

    /// <summary>
    /// Configures a Galdr (magical) ability with rune school and element.
    /// </summary>
    private static void ConfigureGaldrAbility(
        AbilityDatabase database,
        string abilityName,
        string runeSchool,
        string? element)
    {
        var ability = database.GetAbility(abilityName);
        if (ability != null)
        {
            ability.RuneSchool = runeSchool;
            ability.AbilityCategory = null; // Galdr abilities don't use ability category
            ability.Element = element;
        }
    }

    /// <summary>
    /// Helper method to create Galdr abilities with proper configuration.
    /// Use this when adding new Mystic abilities to AbilityDatabase.
    /// </summary>
    /// <example>
    /// var flameBolt = CreateGaldrAbility(
    ///     name: "Flame Bolt",
    ///     description: "Hurl a bolt of flame...",
    ///     runeSchool: "Fehu",
    ///     element: "Fire",
    ///     apCost: 10,
    ///     attributeUsed: "will",
    ///     bonusDice: 1,
    ///     damageDice: 2
    /// );
    /// </example>
    public static Ability CreateGaldrAbility(
        string name,
        string description,
        string runeSchool,
        string? element,
        int apCost,
        string attributeUsed,
        int bonusDice,
        int damageDice = 0,
        int successThreshold = 2,
        AbilityType type = AbilityType.Attack)
    {
        return new Ability
        {
            Name = name,
            Description = description,
            APCost = apCost,
            StaminaCost = 0, // Galdr uses AP, not Stamina
            Type = type,
            AttributeUsed = attributeUsed,
            BonusDice = bonusDice,
            SuccessThreshold = successThreshold,
            DamageDice = damageDice,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0,
            // v0.38.7 properties
            RuneSchool = runeSchool,
            Element = element,
            AbilityCategory = null // Galdr abilities use RuneSchool, not AbilityCategory
        };
    }

    /// <summary>
    /// Helper method to create non-Galdr abilities with proper configuration.
    /// Use this when adding new Warrior/Adept abilities to AbilityDatabase.
    /// </summary>
    public static Ability CreateWarriorAbility(
        string name,
        string description,
        string abilityCategory,
        int staminaCost,
        string attributeUsed,
        int bonusDice,
        int damageDice = 0,
        int successThreshold = 2,
        AbilityType type = AbilityType.Attack)
    {
        return new Ability
        {
            Name = name,
            Description = description,
            StaminaCost = staminaCost,
            APCost = 0, // Warriors use Stamina, not AP
            Type = type,
            AttributeUsed = attributeUsed,
            BonusDice = bonusDice,
            SuccessThreshold = successThreshold,
            DamageDice = damageDice,
            MaxRank = 3,
            CostToRank2 = 5,
            CostToRank3 = 0,
            // v0.38.7 properties
            RuneSchool = null, // Not a Galdr ability
            AbilityCategory = abilityCategory,
            Element = null
        };
    }
}

/// <summary>
/// Example Mystic abilities for reference.
/// These would be added to AbilityDatabase when Mystic class is implemented.
/// </summary>
public static class ExampleMysticAbilities
{
    /// <summary>
    /// Example: Fehu (Fire) - Flame Bolt
    /// </summary>
    public static Ability CreateFlameBolt()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Flame Bolt",
            description: "Invoke the Fehu rune to hurl a bolt of primal fire at your enemy. The ancient power of wealth and cattle manifests as consuming flame.",
            runeSchool: "Fehu",
            element: "Fire",
            apCost: 10,
            attributeUsed: "will",
            bonusDice: 1,
            damageDice: 2, // 2d6 + WILL fire damage
            type: AbilityType.Attack
        );
    }

    /// <summary>
    /// Example: Thurisaz (Ice) - Frost Lance
    /// </summary>
    public static Ability CreateFrostLance()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Frost Lance",
            description: "Chant Thurisaz, the giant's rune, to manifest a deadly spear of ice. The thorn of winter pierces your foes.",
            runeSchool: "Thurisaz",
            element: "Ice",
            apCost: 10,
            attributeUsed: "will",
            bonusDice: 1,
            damageDice: 2, // 2d6 + WILL ice damage
            type: AbilityType.Attack
        );
    }

    /// <summary>
    /// Example: Ansuz (Lightning) - Lightning Bolt
    /// </summary>
    public static Ability CreateLightningBolt()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Lightning Bolt",
            description: "Invoke Ansuz, the breath of gods, to unleash divine lightning upon your enemies. Thunder follows in its wake.",
            runeSchool: "Ansuz",
            element: "Lightning",
            apCost: 12,
            attributeUsed: "will",
            bonusDice: 1,
            damageDice: 2, // 2d6 + WILL lightning damage
            type: AbilityType.Attack
        );
    }

    /// <summary>
    /// Example: Berkanan (Healing) - Healing Chant
    /// </summary>
    public static Ability CreateHealingChant()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Healing Chant",
            description: "Sing Berkanan's gentle syllables, the growth-rune. Life itself flows through you, mending wounds and restoring vitality.",
            runeSchool: "Berkanan",
            element: "Healing",
            apCost: 15,
            attributeUsed: "will",
            bonusDice: 1,
            damageDice: 0, // Healing, not damage
            type: AbilityType.Utility
        );
    }

    /// <summary>
    /// Example: Tiwaz (Protection) - Rune Ward
    /// </summary>
    public static Ability CreateRuneWard()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Rune Ward",
            description: "Invoke Tiwaz, the justice-rune, to weave protective barriers of golden light. Divine wards shield you from harm.",
            runeSchool: "Tiwaz",
            element: null, // No specific element
            apCost: 20,
            attributeUsed: "will",
            bonusDice: 0,
            damageDice: 0,
            type: AbilityType.Defense
        );
    }

    /// <summary>
    /// Example: Naudiz (Draining) - Drain Life
    /// </summary>
    public static Ability CreateDrainLife()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Drain Life",
            description: "Whisper Naudiz, the need-rune, to siphon vitality from your foes. Shadow tendrils drain their life force into you.",
            runeSchool: "Naudiz",
            element: "Shadow",
            apCost: 15,
            attributeUsed: "will",
            bonusDice: 1,
            damageDice: 2, // 2d6 + WILL draining damage (also heals caster)
            type: AbilityType.Attack
        );
    }

    /// <summary>
    /// Example: Isa (Stasis) - Frozen Time
    /// </summary>
    public static Ability CreateFrozenTime()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Frozen Time",
            description: "Intone Isa, the ice-rune. Time itself slows as frost spreads across your enemy, trapping them in crystalline stasis.",
            runeSchool: "Isa",
            element: "Ice",
            apCost: 18,
            attributeUsed: "will",
            bonusDice: 0,
            damageDice: 0, // Control ability, no direct damage
            type: AbilityType.Control
        );
    }

    /// <summary>
    /// Example: Laguz (Water/Purification) - Cleansing Wave
    /// </summary>
    public static Ability CreateCleansingWave()
    {
        return AbilityDatabaseGaldrExtensions.CreateGaldrAbility(
            name: "Cleansing Wave",
            description: "Sing Laguz, the water-rune. Purifying waves wash over you, cleansing poison, corruption, and disease.",
            runeSchool: "Laguz",
            element: "Water",
            apCost: 12,
            attributeUsed: "will",
            bonusDice: 0,
            damageDice: 0, // Utility ability
            type: AbilityType.Utility
        );
    }
}
