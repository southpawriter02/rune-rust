using RuneAndRust.Core;

namespace RuneAndRust.Engine;

/// <summary>
/// Database of all equipment in the game (v0.3)
/// Hard-coded for now, can be moved to JSON/database in v0.4
/// </summary>
public static class EquipmentDatabase
{
    private static readonly List<Equipment> AllEquipment = new();

    static EquipmentDatabase()
    {
        InitializeWarriorWeapons();
        InitializeScavengerWeapons();
        InitializeMysticWeapons();
        InitializeArmor();
        InitializeV016Equipment(); // v0.16 Content Expansion
    }

    /// <summary>
    /// Get all equipment in the database
    /// </summary>
    public static List<Equipment> GetAllEquipment()
    {
        return new List<Equipment>(AllEquipment);
    }

    /// <summary>
    /// Get equipment by exact name
    /// </summary>
    public static Equipment? GetByName(string name)
    {
        return AllEquipment.FirstOrDefault(e =>
            e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all weapons of a specific category
    /// </summary>
    public static List<Equipment> GetWeaponsByCategory(WeaponCategory category)
    {
        return AllEquipment
            .Where(e => e.Type == EquipmentType.Weapon && e.WeaponCategory == category)
            .ToList();
    }

    /// <summary>
    /// Get all armor of a specific category
    /// </summary>
    public static List<Equipment> GetArmorByCategory(ArmorCategory category)
    {
        return AllEquipment
            .Where(e => e.Type == EquipmentType.Armor && e.ArmorCategory == category)
            .ToList();
    }

    /// <summary>
    /// Get random weapon for a character class
    /// </summary>
    public static Equipment? GetRandomWeaponForClass(CharacterClass characterClass, QualityTier quality)
    {
        var weapons = characterClass switch
        {
            CharacterClass.Warrior => AllEquipment
                .Where(e => e.Type == EquipmentType.Weapon &&
                           (e.WeaponCategory == Core.WeaponCategory.Axe ||
                            e.WeaponCategory == Core.WeaponCategory.Greatsword) &&
                           e.Quality == quality)
                .ToList(),
            CharacterClass.Scavenger => AllEquipment
                .Where(e => e.Type == EquipmentType.Weapon &&
                           (e.WeaponCategory == Core.WeaponCategory.Spear ||
                            e.WeaponCategory == Core.WeaponCategory.Dagger) &&
                           e.Quality == quality)
                .ToList(),
            CharacterClass.Mystic => AllEquipment
                .Where(e => e.Type == EquipmentType.Weapon &&
                           (e.WeaponCategory == Core.WeaponCategory.Staff ||
                            e.WeaponCategory == Core.WeaponCategory.Focus) &&
                           e.Quality == quality)
                .ToList(),
            _ => new List<Equipment>()
        };

        if (weapons.Count == 0) return null;
        var random = new Random();
        return weapons[random.Next(weapons.Count)];
    }

    /// <summary>
    /// Get random armor of a specific quality
    /// </summary>
    public static Equipment? GetRandomArmor(QualityTier quality)
    {
        var armors = AllEquipment
            .Where(e => e.Type == EquipmentType.Armor && e.Quality == quality)
            .ToList();

        if (armors.Count == 0) return null;
        var random = new Random();
        return armors[random.Next(armors.Count)];
    }

    #region Warrior Weapons

    private static void InitializeWarriorWeapons()
    {
        // === AXES ===
        AllEquipment.Add(new Equipment
        {
            Name = "Rusty Hatchet",
            Description = "A chipped hatchet held together with scavenged wire. Barely functional.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 5,
            AccuracyBonus = -1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = -1, Description = "-1 Accuracy" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Axe",
            Description = "A serviceable axe recovered from the ruins. Shows signs of wear but still deadly.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 1,
            StaminaCost = 5,
            AccuracyBonus = 0
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Axe",
            Description = "Expertly crafted by clan smiths, this axe is balanced and razor-sharp.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 3, // d8 equivalent = d6+2, but we'll use d6+3 for better progression
            StaminaCost = 5,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 1, Description = "+1 MIGHT checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Optimized War Axe",
            Description = "Pre-Glitch military hardware, maintained to perfection. Cuts through armor like paper.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Axe,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 0,
            StaminaCost = 6,
            AccuracyBonus = 1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = 1, Description = "+1 Accuracy" }
            }
        });

        // === GREATSWORDS ===
        AllEquipment.Add(new Equipment
        {
            Name = "Bent Greatsword",
            Description = "A massive blade warped by heat and time. Heavy and unwieldy.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 2,
            StaminaCost = 8,
            AccuracyBonus = -1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = -1, Description = "-1 Accuracy" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Greatsword",
            Description = "A two-handed blade salvaged from a fallen warrior. Still has some bite.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 4, // d10 equivalent
            StaminaCost = 8,
            AccuracyBonus = 0
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Greatsword",
            Description = "A masterwork blade forged by clan artisans. Heavy, but devastating in skilled hands.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 6, // d10+2 equivalent
            StaminaCost = 8,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 1, Description = "+1 MIGHT checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Warden's Greatsword",
            Description = "A legendary blade once wielded by the facility's guardian. Hums with residual power.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 4, // 2d8+2 equivalent
            StaminaCost = 10,
            AccuracyBonus = 0,
            IgnoresArmor = true,
            SpecialEffect = "Ignores 50% armor, grants [Fortified] on kill",
            Bonuses = new List<EquipmentBonus>()
        });

        // [v0.6] NEW BOSS WEAPONS

        AllEquipment.Add(new Equipment
        {
            Name = "Custodian's Halberd",
            Description = "The ancient guardian's weapon, scarred from centuries of duty. Sweeps through multiple foes.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 3,
            DamageBonus = 2, // 3d8+2 damage
            StaminaCost = 12,
            AccuracyBonus = 0,
            SpecialEffect = "Cleave: Attacks hit adjacent enemies",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 2, Description = "+2 MIGHT checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Omega Maul",
            Description = "A weapon of overwhelming force. The air trembles with each swing. Devastates all nearby foes.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Greatsword,
            WeaponAttribute = "MIGHT",
            DamageDice = 4,
            DamageBonus = 4, // 4d10+4 damage approximated
            StaminaCost = 15,
            AccuracyBonus = 0,
            IgnoresArmor = false,
            SpecialEffect = "AOE: Hits ALL adjacent enemies with full damage",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 3, Description = "+3 MIGHT checks" },
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 2, Description = "+2 STURDINESS checks" }
            }
        });
    }

    #endregion

    #region Scavenger Weapons

    private static void InitializeScavengerWeapons()
    {
        // === SPEARS ===
        AllEquipment.Add(new Equipment
        {
            Name = "Makeshift Spear",
            Description = "A sharpened pipe lashed to a pole. Better than nothing.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Spear,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 4,
            AccuracyBonus = -1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = -1, Description = "-1 Accuracy" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Spear",
            Description = "A functional spear with a metal head. Good reach and balance.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Spear,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 1,
            StaminaCost = 4,
            AccuracyBonus = 0
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Spear",
            Description = "A well-crafted spear with perfect balance. The tip gleams menacingly.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Spear,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 3,
            StaminaCost = 4,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Optimized Combat Lance",
            Description = "Military-grade lance with reach-extending tech. Strikes from unexpected distances.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Spear,
            WeaponAttribute = "FINESSE",
            DamageDice = 2,
            DamageBonus = 0,
            StaminaCost = 5,
            AccuracyBonus = 2,
            SpecialEffect = "Reach 2 zones",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = 2, Description = "+2 Accuracy" }
            }
        });

        // === DAGGERS ===
        AllEquipment.Add(new Equipment
        {
            Name = "Sharpened Scrap",
            Description = "A piece of metal filed to a crude edge. Desperate times...",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Dagger,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = -2, // d4 equivalent
            StaminaCost = 3,
            AccuracyBonus = -1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = -1, Description = "-1 Accuracy" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Dagger",
            Description = "A short blade in decent condition. Quick and deadly in close quarters.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Dagger,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 3,
            AccuracyBonus = 0
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Blade",
            Description = "A finely honed dagger with a keen edge. Strikes with surgical precision.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Dagger,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 2,
            StaminaCost = 3,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Assassin's Fang",
            Description = "A legendary blade of shadow-forged steel. Wounds inflicted never truly heal.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Dagger,
            WeaponAttribute = "FINESSE",
            DamageDice = 2,
            DamageBonus = 0,
            StaminaCost = 4,
            AccuracyBonus = 0,
            SpecialEffect = "Critical hits apply [Bleeding]",
            Bonuses = new List<EquipmentBonus>()
        });
    }

    #endregion

    #region Mystic Weapons

    private static void InitializeMysticWeapons()
    {
        // === STAVES ===
        AllEquipment.Add(new Equipment
        {
            Name = "Crude Staff",
            Description = "A gnarled branch with minimal channeling capacity. Barely functional.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Staff,
            WeaponAttribute = "WILL",
            DamageDice = 1,
            DamageBonus = -2, // d4 equivalent
            StaminaCost = 5,
            AccuracyBonus = -1,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "Accuracy", BonusValue = -1, Description = "-1 Accuracy" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Staff",
            Description = "A recovered focusing staff with intact conduits. Reduces ability costs slightly.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Staff,
            WeaponAttribute = "WILL",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 5,
            AccuracyBonus = 0,
            SpecialEffect = "-1 Stamina cost to abilities",
            Bonuses = new List<EquipmentBonus>()
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Staff",
            Description = "A staff carved with intricate runes. Channels power with impressive efficiency.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Staff,
            WeaponAttribute = "WILL",
            DamageDice = 1,
            DamageBonus = 1,
            StaminaCost = 5,
            AccuracyBonus = 0,
            SpecialEffect = "-2 Stamina cost to abilities",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 1, Description = "+1 WILL checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Runestone Staff",
            Description = "An artifact of immense power, topped with a glowing runestone. Reality bends to your will.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Staff,
            WeaponAttribute = "WILL",
            DamageDice = 1,
            DamageBonus = 4,
            StaminaCost = 6,
            AccuracyBonus = 0,
            SpecialEffect = "-3 Stamina cost to abilities, abilities gain +1 bonus die",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 2, Description = "+2 WILL checks" }
            }
        });

        // [v0.6] NEW BOSS WEAPON

        AllEquipment.Add(new Equipment
        {
            Name = "Mind-Render Scepter",
            Description = "The Forlorn Archivist's psychic focus. Whispers of madness echo within. Heretical abilities cost less trauma.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Staff,
            WeaponAttribute = "WILL",
            DamageDice = 3,
            DamageBonus = 3, // 3d8+3 psychic damage
            StaminaCost = 7,
            AccuracyBonus = 0,
            IgnoresArmor = true, // Psychic damage bypasses armor
            SpecialEffect = "Psychic damage ignores armor. Heretical abilities cost -5 Psychic Stress.",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 4, Description = "+4 WILL checks" }
            }
        });

        // === FOCUSES ===
        AllEquipment.Add(new Equipment
        {
            Name = "Cracked Crystal",
            Description = "A damaged focusing crystal. Amplifies power but cannot be used for melee.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Focus,
            WeaponAttribute = "WILL",
            DamageDice = 0, // No melee damage
            DamageBonus = 0,
            StaminaCost = 0,
            AccuracyBonus = 0,
            SpecialEffect = "+1 bonus die to abilities",
            Bonuses = new List<EquipmentBonus>()
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Focus",
            Description = "An intact focusing crystal that amplifies channeled power significantly.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Focus,
            WeaponAttribute = "WILL",
            DamageDice = 0,
            DamageBonus = 0,
            StaminaCost = 0,
            AccuracyBonus = 0,
            SpecialEffect = "+2 bonus dice to abilities",
            Bonuses = new List<EquipmentBonus>()
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Focus",
            Description = "A perfectly cut crystal that resonates with aetheric energy. Immense amplification.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Focus,
            WeaponAttribute = "WILL",
            DamageDice = 0,
            DamageBonus = 0,
            StaminaCost = 0,
            AccuracyBonus = 0,
            SpecialEffect = "+3 bonus dice to abilities",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 1, Description = "+1 WILL checks" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Aetheric Amplifier",
            Description = "A masterwork device that draws power from the Blight itself. Reality trembles.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Focus,
            WeaponAttribute = "WILL",
            DamageDice = 0,
            DamageBonus = 0,
            StaminaCost = 0,
            AccuracyBonus = 0,
            SpecialEffect = "+4 bonus dice to abilities, restore 5 Stamina on kill",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 2, Description = "+2 WILL checks" }
            }
        });
    }

    #endregion

    #region Armor

    private static void InitializeArmor()
    {
        // === LIGHT ARMOR ===
        AllEquipment.Add(new Equipment
        {
            Name = "Tattered Leathers",
            Description = "Worn leather armor that's seen better days. Offers minimal protection.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Light,
            HPBonus = 2,
            DefenseBonus = 1,
            Bonuses = new List<EquipmentBonus>()
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Leathers",
            Description = "Functional leather armor recovered from the ruins. Light and flexible.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Light,
            HPBonus = 5,
            DefenseBonus = 2,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Leathers",
            Description = "Expertly cured leather reinforced with lightweight plates. Perfect mobility.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Light,
            HPBonus = 10,
            DefenseBonus = 3,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE" },
                new EquipmentBonus { AttributeName = "Evasion", BonusValue = 1, Description = "+1 Evasion" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Shadow Weave",
            Description = "Legendary armor woven from shadow-thread. Nearly weightless, incredibly protective.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Light,
            HPBonus = 15,
            DefenseBonus = 4,
            SpecialEffect = "Gain [Hasted] when below 50% HP",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 2, Description = "+2 FINESSE" },
                new EquipmentBonus { AttributeName = "Evasion", BonusValue = 2, Description = "+2 Evasion" }
            }
        });

        // === MEDIUM ARMOR ===
        AllEquipment.Add(new Equipment
        {
            Name = "Scrap Plating",
            Description = "Metal plates lashed together with wire. Heavy and uncomfortable.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 5,
            DefenseBonus = 2,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -1, Description = "-1 FINESSE" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Chainmail",
            Description = "Well-maintained chainmail that offers solid protection without excessive weight.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 10,
            DefenseBonus = 3,
            Bonuses = new List<EquipmentBonus>()
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Plate",
            Description = "Balanced plate armor crafted by master smiths. Strong and surprisingly mobile.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 15,
            DefenseBonus = 4,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 1, Description = "+1 STURDINESS" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Warden's Aegis",
            Description = "The Warden's own armor, imbued with protective magic. Wounds close as if they never existed.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 25,
            DefenseBonus = 5,
            SpecialEffect = "Immune to [Bleeding]",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 2, Description = "+2 STURDINESS" }
            }
        });

        // === HEAVY ARMOR ===
        AllEquipment.Add(new Equipment
        {
            Name = "Bent Plate",
            Description = "Warped plate armor that limits movement. Better than nothing, but only just.",
            Quality = QualityTier.JuryRigged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 8,
            DefenseBonus = 2,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -2, Description = "-2 FINESSE" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Scavenged Heavy Plate",
            Description = "Thick plate armor from a fallen soldier. Provides excellent protection but restricts movement.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 15,
            DefenseBonus = 4,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -1, Description = "-1 FINESSE" },
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 1, Description = "+1 STURDINESS" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Clan-Forged Full Plate",
            Description = "Masterwork full plate armor. A walking fortress that can withstand tremendous punishment.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 20,
            DefenseBonus = 5,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 2, Description = "+2 STURDINESS" }
            }
        });

        AllEquipment.Add(new Equipment
        {
            Name = "Juggernaut Frame",
            Description = "Powered armor from the old world. An unstoppable force that regenerates damage over time.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 40,
            DefenseBonus = 6,
            SpecialEffect = "Regenerate 5 HP per turn",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "STURDINESS", BonusValue = 3, Description = "+3 STURDINESS" },
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -2, Description = "-2 FINESSE" }
            }
        });
    }

    #endregion

    #region v0.16 Content Expansion Equipment

    private static void InitializeV016Equipment()
    {
        // ===== NEW WEAPONS =====

        // Rusted Machete (Common, Tier 1)
        AllEquipment.Add(new Equipment
        {
            Name = "Rusted Machete",
            Description = "Salvaged from industrial wreckage. Still cuts.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Blade,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 5,
            AccuracyBonus = 0,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 1, Description = "+1 MIGHT" }
            }
        });

        // Scrap-Metal Cudgel (Common, Tier 1)
        AllEquipment.Add(new Equipment
        {
            Name = "Scrap-Metal Cudgel",
            Description = "Heavy. Crude. Effective.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Blunt,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 0,
            StaminaCost = 5,
            AccuracyBonus = 0,
            SpecialEffect = "20% chance to stun (lose 1 action)",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 2, Description = "+2 MIGHT" }
            }
        });

        // Shock-Baton (Uncommon, Tier 2)
        AllEquipment.Add(new Equipment
        {
            Name = "Shock-Baton",
            Description = "Security equipment. Electrical charge still functional. Barely.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.EnergyMelee,
            WeaponAttribute = "FINESSE",
            DamageDice = 1,
            DamageBonus = 2,
            StaminaCost = 6,
            AccuracyBonus = 0,
            SpecialEffect = "Bypasses 2 points of armor",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE" },
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 1, Description = "+1 WITS (Tech)" }
            }
        });

        // Bone-Saw Blade (Uncommon, Tier 2)
        AllEquipment.Add(new Equipment
        {
            Name = "Bone-Saw Blade",
            Description = "Symbiotic Plate-crafted weapon. Disturbingly effective.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Blade,
            WeaponAttribute = "MIGHT",
            DamageDice = 1,
            DamageBonus = 3,
            StaminaCost = 6,
            AccuracyBonus = 0,
            SpecialEffect = "Attacks inflict [Bleeding] (1d4 damage/turn, 3 turns). WARNING: +5 Corruption on equip",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 2, Description = "+2 MIGHT" }
            }
        });

        // Plasma Cutter (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Plasma Cutter",
            Description = "Industrial tool. Cuts through steel. Cuts through bone.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.EnergyMelee,
            WeaponAttribute = "FINESSE",
            DamageDice = 2,
            DamageBonus = 0,
            StaminaCost = 8,
            AccuracyBonus = 1,
            IgnoresArmor = true,
            SpecialEffect = "Ignores armor. Requires power cells (consumable).",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 2, Description = "+2 FINESSE" },
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 2, Description = "+2 WITS (Tech)" }
            }
        });

        // Sentinel's Rifle (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Sentinel's Rifle",
            Description = "Military-grade. Servitor-grade. Kill-grade. Taken from a Sentinel Prime. It remembers killing.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Rifle,
            WeaponAttribute = "FINESSE",
            DamageDice = 2,
            DamageBonus = 2,
            StaminaCost = 7,
            AccuracyBonus = 2,
            SpecialEffect = "Armor piercing (-3 to target Defense). Range: 30ft.",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 3, Description = "+3 FINESSE" },
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 1, Description = "+1 WITS" }
            }
        });

        // Thunder Hammer (Epic, Tier 4)
        AllEquipment.Add(new Equipment
        {
            Name = "Thunder Hammer",
            Description = "Each strike echoes like thunder in The Roots. Construction equipment. Built to break mountains.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.HeavyBlunt,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 4,
            StaminaCost = 10,
            AccuracyBonus = 0,
            SpecialEffect = "AOE 5ft radius on hit (1d6 damage to nearby enemies). Knock prone.",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 4, Description = "+4 MIGHT" }
            }
        });

        // Heretical Blade (Epic, Tier 4)
        AllEquipment.Add(new Equipment
        {
            Name = "Heretical Blade",
            Description = "It wants to cut. It wants to feed. Found in Jötun-Reader ruins. Wrong. Alien. Hungry.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Blade,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 3,
            StaminaCost = 8,
            AccuracyBonus = 1,
            SpecialEffect = "Lifesteal (heal 25% of damage dealt). Whispers to you. WARNING: +10 Corruption on equip",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 3, Description = "+3 MIGHT" }
            }
        });

        // Arc-Cannon (Legendary, Tier 5/MythForged)
        AllEquipment.Add(new Equipment
        {
            Name = "Arc-Cannon",
            Description = "Experimental Pre-Glitch weapon. Devastating. Unstable. Only 3 were made. You have the last one.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Rifle,
            WeaponAttribute = "FINESSE",
            DamageDice = 3,
            DamageBonus = 2,
            StaminaCost = 12,
            AccuracyBonus = 2,
            SpecialEffect = "Chain lightning (bounces to 2 additional targets). Overcharge mode: double damage, weapon breaks.",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 4, Description = "+4 FINESSE" },
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 3, Description = "+3 WITS (Tech)" }
            }
        });

        // The Rust-Eater (Legendary, Tier 5/MythForged)
        AllEquipment.Add(new Equipment
        {
            Name = "The Rust-Eater",
            Description = "Forged from salvaged Sentinel parts. It hunts its own kind. A blade that kills machines. Made from machines.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Weapon,
            WeaponCategory = Core.WeaponCategory.Blade,
            WeaponAttribute = "MIGHT",
            DamageDice = 2,
            DamageBonus = 2,
            StaminaCost = 7,
            AccuracyBonus = 2,
            SpecialEffect = "Deals double damage to Draugr-Pattern enemies. Self-repairs (regains durability).",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 4, Description = "+4 MIGHT" },
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 2, Description = "+2 FINESSE" }
            }
        });

        // ===== NEW ARMOR =====

        // Salvaged Leathers (Common, Tier 1)
        AllEquipment.Add(new Equipment
        {
            Name = "Salvaged Leathers",
            Description = "Patched worker's gear. Better than nothing.",
            Quality = QualityTier.Scavenged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Light,
            HPBonus = 5,
            DefenseBonus = 2,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = 1, Description = "+1 FINESSE" }
            }
        });

        // Reinforced Coveralls (Uncommon, Tier 2)
        AllEquipment.Add(new Equipment
        {
            Name = "Reinforced Coveralls",
            Description = "Heavy-duty work clothing with scavenged metal plates.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 10,
            DefenseBonus = 3,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 1, Description = "+1 MIGHT" },
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 1, Description = "+1 WILL" }
            }
        });

        // Servitor Shell Armor (Uncommon, Tier 2)
        AllEquipment.Add(new Equipment
        {
            Name = "Servitor Shell Armor",
            Description = "Plating torn from deactivated Servitors. Functional.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 8,
            DefenseBonus = 4,
            SpecialEffect = "Resistance to electrical damage (half damage)",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 2, Description = "+2 MIGHT" },
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -1, Description = "-1 FINESSE" }
            }
        });

        // Guardian's Plate (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Guardian's Plate",
            Description = "Security automaton plating. Built to withstand riots.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 15,
            DefenseBonus = 6,
            SpecialEffect = "Damage reduction (reduce all incoming damage by 2)",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 3, Description = "+3 MIGHT" },
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -2, Description = "-2 FINESSE" }
            }
        });

        // Symbiotic Carapace (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Symbiotic Carapace",
            Description = "It's alive. It breathes. It's part of you now.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Medium,
            HPBonus = 12,
            DefenseBonus = 4,
            SpecialEffect = "Regeneration (heal 2 HP/turn in combat). WARNING: +8 Corruption on equip",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 2, Description = "+2 WILL" }
            }
        });

        // Sentinel Prime Armor (Epic, Tier 4)
        AllEquipment.Add(new Equipment
        {
            Name = "Sentinel Prime Armor",
            Description = "Military-grade automaton plating. Near-impenetrable.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Armor,
            ArmorCategory = Core.ArmorCategory.Heavy,
            HPBonus = 20,
            DefenseBonus = 8,
            SpecialEffect = "Energy shields (absorb 20 damage, recharges on rest)",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "MIGHT", BonusValue = 4, Description = "+4 MIGHT" },
                new EquipmentBonus { AttributeName = "FINESSE", BonusValue = -1, Description = "-1 FINESSE" }
            }
        });

        // ===== NEW ACCESSORIES =====

        // Stress Dampener (Uncommon, Tier 2)
        AllEquipment.Add(new Equipment
        {
            Name = "Stress Dampener",
            Description = "Salvaged neural regulator. Dulls the edge of fear.",
            Quality = QualityTier.ClanForged,
            Type = EquipmentType.Accessory,
            SpecialEffect = "-20% Stress gain from all sources",
            Bonuses = new List<EquipmentBonus>()
        });

        // Data-Slate Fragment (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Data-Slate Fragment",
            Description = "Pre-Glitch data device. Shows you what the machines see.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Accessory,
            SpecialEffect = "Access to Layer 2 diagnostic information",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 2, Description = "+2 WITS" },
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 3, Description = "+3 Tech checks" }
            }
        });

        // Iron Heart Pendant (Rare, Tier 3)
        AllEquipment.Add(new Equipment
        {
            Name = "Iron Heart Pendant",
            Description = "A deactivated Iron Heart power core. Warm to the touch.",
            Quality = QualityTier.Optimized,
            Type = EquipmentType.Accessory,
            HPBonus = 10,
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WILL", BonusValue = 1, Description = "+1 WILL" }
            }
        });

        // Corruption Filter (Epic, Tier 4)
        AllEquipment.Add(new Equipment
        {
            Name = "Corruption Filter",
            Description = "Experimental bio-filter. Keeps the wrong thoughts out. Mostly.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Accessory,
            SpecialEffect = "-30% Corruption gain from all sources",
            Bonuses = new List<EquipmentBonus>()
        });

        // Jötun-Reader Shard (Epic, Tier 4)
        AllEquipment.Add(new Equipment
        {
            Name = "Jötun-Reader Shard",
            Description = "Fragment of alien AI. It thinks through you.",
            Quality = QualityTier.MythForged,
            Type = EquipmentType.Accessory,
            SpecialEffect = "Unlock 2 Heretical abilities. WARNING: +15 Corruption on equip",
            Bonuses = new List<EquipmentBonus>
            {
                new EquipmentBonus { AttributeName = "WITS", BonusValue = 4, Description = "+4 WITS (Tech)" }
            }
        });
    }

    #endregion
}
