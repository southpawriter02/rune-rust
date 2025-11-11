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
}
