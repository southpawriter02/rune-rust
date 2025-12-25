using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Static loot table definitions for procedural item generation.
/// All descriptions follow AAM-VOICE Layer 2 (Diagnostic) constraints.
/// Domain 4 compliant: No precision measurements or modern terminology.
/// </summary>
/// <remarks>See: SPEC-LOOT-001 for Loot Generation System design.</remarks>
public static class LootTables
{
    #region Quality Weights by Danger Level

    /// <summary>
    /// Quality tier weights (percentages) based on danger level.
    /// Higher danger yields better quality items.
    /// </summary>
    public static readonly Dictionary<DangerLevel, Dictionary<QualityTier, int>> QualityWeightsByDanger = new()
    {
        [DangerLevel.Safe] = new Dictionary<QualityTier, int>
        {
            { QualityTier.JuryRigged, 30 },
            { QualityTier.Scavenged, 60 },
            { QualityTier.ClanForged, 10 },
            { QualityTier.Optimized, 0 },
            { QualityTier.MythForged, 0 }
        },
        [DangerLevel.Unstable] = new Dictionary<QualityTier, int>
        {
            { QualityTier.JuryRigged, 20 },
            { QualityTier.Scavenged, 50 },
            { QualityTier.ClanForged, 25 },
            { QualityTier.Optimized, 5 },
            { QualityTier.MythForged, 0 }
        },
        [DangerLevel.Hostile] = new Dictionary<QualityTier, int>
        {
            { QualityTier.JuryRigged, 10 },
            { QualityTier.Scavenged, 35 },
            { QualityTier.ClanForged, 35 },
            { QualityTier.Optimized, 18 },
            { QualityTier.MythForged, 2 }
        },
        [DangerLevel.Lethal] = new Dictionary<QualityTier, int>
        {
            { QualityTier.JuryRigged, 5 },
            { QualityTier.Scavenged, 25 },
            { QualityTier.ClanForged, 40 },
            { QualityTier.Optimized, 25 },
            { QualityTier.MythForged, 5 }
        }
    };

    #endregion

    #region Item Count Ranges by Danger

    /// <summary>
    /// Minimum and maximum item counts by danger level.
    /// </summary>
    public static readonly Dictionary<DangerLevel, (int Min, int Max)> ItemCountsByDanger = new()
    {
        [DangerLevel.Safe] = (1, 2),
        [DangerLevel.Unstable] = (1, 3),
        [DangerLevel.Hostile] = (2, 4),
        [DangerLevel.Lethal] = (2, 5)
    };

    #endregion

    #region Weapon Templates

    /// <summary>
    /// Weapon templates by quality tier with AAM-VOICE descriptions.
    /// </summary>
    public static readonly Dictionary<QualityTier, List<WeaponTemplate>> WeaponsByQuality = new()
    {
        [QualityTier.JuryRigged] = new List<WeaponTemplate>
        {
            new("Rusted Blade", "A corroded length of metal, its edge worn ragged by neglect.",
                "The blade bears the pitting of long exposure. Its grip, wrapped in salvaged cloth, offers uncertain purchase.",
                EquipmentSlot.MainHand, 6, 1200, 15),
            new("Scrap Club", "A length of pipe with metal scraps lashed to one end.",
                "The binding frays at several points. The weight distribution suggests wildly inconsistent swings.",
                EquipmentSlot.MainHand, 6, 1500, 10),
            new("Splintered Hatchet", "A woodcutter's tool long past its prime.",
                "The head wobbles on a cracked haft. Still capable of harm, if unreliably so.",
                EquipmentSlot.MainHand, 4, 900, 12)
        },
        [QualityTier.Scavenged] = new List<WeaponTemplate>
        {
            new("Salvaged Shortsword", "A blade of respectable make, its edge maintained through careful honing.",
                "Dvergr foundry marks suggest pre-Glitch manufacture. The balance, while not refined, serves adequately.",
                EquipmentSlot.MainHand, 6, 1000, 35),
            new("Iron Axe", "A single-headed axe with a serviceable edge.",
                "The head shows careful repair work. Weight favors power over precision.",
                EquipmentSlot.MainHand, 8, 1400, 40),
            new("Hunter's Spear", "A thrusting weapon with a leaf-shaped blade.",
                "The haft bears the wear of many hands. The point, though chipped, retains its threat.",
                EquipmentSlot.MainHand, 6, 1100, 30)
        },
        [QualityTier.ClanForged] = new List<WeaponTemplate>
        {
            new("Dvergr War-Blade", "Folded steel bearing the hammer-marks of skilled smiths.",
                "The distinctive pattern-welding speaks to Dvergr tradition. Balance and edge suggest deliberate craft.",
                EquipmentSlot.MainHand, 8, 1100, 85),
            new("Ironwood Maul", "A heavy striking weapon of reinforced construction.",
                "The head, bound in iron bands, delivers crushing force. The ironwood shaft resists warping.",
                EquipmentSlot.MainHand, 10, 2200, 90),
            new("Raider's Falchion", "A broad-bladed cutting weapon with a weighted tip.",
                "The curve and heft suggest design for mounted use. Ground combat applications remain effective.",
                EquipmentSlot.MainHand, 8, 1300, 80)
        },
        [QualityTier.Optimized] = new List<WeaponTemplate>
        {
            new("Aesir Longsword", "Pre-Glitch craftsmanship of exceptional refinement.",
                "The alloy resists identification by conventional means. Its edge holds beyond reasonable expectation.",
                EquipmentSlot.MainHand, 10, 950, 200),
            new("Rune-Etched Axe", "Ancient symbols trace paths along the blade.",
                "The etchings pulse with faint luminescence under certain conditions. The metal feels warm to the touch.",
                EquipmentSlot.MainHand, 10, 1500, 220),
            new("Calibrated Crossbow", "A mechanical marvel of precision engineering.",
                "The mechanisms function with unnatural smoothness. Draw weight adjusts to the wielder's capability.",
                EquipmentSlot.MainHand, 8, 1800, 180)
        },
        [QualityTier.MythForged] = new List<WeaponTemplate>
        {
            new("Glitch-Touched Blade", "The metal shifts between states, solid and something... else.",
                "Observers report the edge existing in multiple positions simultaneously. Contact produces unexpected wounds.",
                EquipmentSlot.MainHand, 12, 800, 500),
            new("Frost-Giant's Fang", "A massive blade carved from the tooth of a legendary beast.",
                "The bone-metal hybrid defies classification. Cold radiates from the edge in waves.",
                EquipmentSlot.MainHand, 12, 2500, 550)
        }
    };

    #endregion

    #region Armor Templates

    /// <summary>
    /// Armor templates by quality tier with AAM-VOICE descriptions.
    /// </summary>
    public static readonly Dictionary<QualityTier, List<ArmorTemplate>> ArmorByQuality = new()
    {
        [QualityTier.JuryRigged] = new List<ArmorTemplate>
        {
            new("Scrap Vest", "Layers of salvaged material stitched together haphazardly.",
                "Gaps in coverage leave vital areas exposed. Better than bare flesh, if only marginally.",
                EquipmentSlot.Body, 1, 2000, 8),
            new("Dented Helm", "A battered piece of head protection bearing numerous impacts.",
                "The interior padding has long since deteriorated. Visibility remains compromised.",
                EquipmentSlot.Head, 1, 800, 5),
            new("Wrapped Bracers", "Leather strips wound around scavenged plates.",
                "The binding loosens with movement. Protection varies wildly by angle of impact.",
                EquipmentSlot.Hands, 0, 400, 4)
        },
        [QualityTier.Scavenged] = new List<ArmorTemplate>
        {
            new("Leather Cuirass", "Boiled leather shaped to cover the torso.",
                "Standard protective wear among those who travel the wastes. Serviceable against glancing blows.",
                EquipmentSlot.Body, 2, 2500, 25),
            new("Iron Cap", "A simple helm of hammered iron.",
                "The design prioritizes coverage over comfort. A leather liner provides minimal padding.",
                EquipmentSlot.Head, 1, 1000, 15),
            new("Traveler's Boots", "Thick-soled footwear reinforced for rough terrain.",
                "The leather shows careful patching. Metal plates guard the toe and heel.",
                EquipmentSlot.Feet, 1, 900, 18)
        },
        [QualityTier.ClanForged] = new List<ArmorTemplate>
        {
            new("Dvergr Mail Shirt", "Interlocking rings of tempered steel.",
                "The weave pattern indicates Dvergr manufacture. Weight distribution allows extended wear.",
                EquipmentSlot.Body, 3, 5500, 75),
            new("Clan-Marked Helm", "A conical helm bearing lineage markings.",
                "The nasal guard and cheek plates provide comprehensive protection. Ventilation appears deliberate.",
                EquipmentSlot.Head, 2, 1400, 45),
            new("Reinforced Gauntlets", "Articulated metal plates over leather backing.",
                "Finger mobility remains largely unimpaired. Knuckle plates suggest offensive capability.",
                EquipmentSlot.Hands, 1, 600, 35)
        },
        [QualityTier.Optimized] = new List<ArmorTemplate>
        {
            new("Aesir Plate Segment", "A single piece of pre-Glitch battle armor.",
                "The metal appears impossibly light for its apparent density. Impacts seem to diffuse across the surface.",
                EquipmentSlot.Body, 4, 3500, 180),
            new("Void-Glass Visor", "A helmet with a seamless faceplate of unknown material.",
                "Vision remains unimpaired despite the dark surface. The material resists all attempts at scratching.",
                EquipmentSlot.Head, 3, 900, 120),
            new("Phase-Weave Gloves", "Thin material that stiffens on impact.",
                "The fabric flows like silk until struck. Dexterity remains fully preserved.",
                EquipmentSlot.Hands, 2, 200, 95)
        },
        [QualityTier.MythForged] = new List<ArmorTemplate>
        {
            new("Living Mail", "Armor that shifts and adapts to incoming threats.",
                "The metal links rearrange themselves in response to danger. The wearer reports sensations of awareness.",
                EquipmentSlot.Body, 5, 4000, 450),
            new("Seer's Crown", "A circlet that grants perception beyond normal limits.",
                "Wearers describe awareness of threats before they manifest. Prolonged use produces headaches and nosebleeds.",
                EquipmentSlot.Head, 2, 300, 400)
        }
    };

    #endregion

    #region Consumable Templates

    /// <summary>
    /// Consumable templates by quality tier with AAM-VOICE descriptions.
    /// </summary>
    public static readonly Dictionary<QualityTier, List<ConsumableTemplate>> ConsumablesByQuality = new()
    {
        [QualityTier.JuryRigged] = new List<ConsumableTemplate>
        {
            new("Questionable Rations", "Preserved food of uncertain provenance.",
                "The packaging has long since degraded. Contents appear stable, if unappetizing.",
                100, 5, new List<string> { "Ration" }),
            new("Cracked Waterskin", "A patched container that leaks at the seams.",
                "Several repairs suggest long use. Contents remain drinkable, if not appetizing.",
                200, 4, new List<string> { "Water" }),
            new("Dirty Bandages", "Cloth strips salvaged from various sources.",
                "Some staining suggests prior use. Better than bleeding out.",
                50, 3)
        },
        [QualityTier.Scavenged] = new List<ConsumableTemplate>
        {
            new("Field Rations", "Standard preserved provisions for wasteland travel.",
                "Compressed nutrient blocks and dried meats. Sustaining, if not satisfying.",
                150, 12, new List<string> { "Ration" }),
            new("Waterskin", "A leather container for carrying water.",
                "The stitching holds well. Sufficient for a day's hydration in the wastes.",
                250, 10, new List<string> { "Water" }),
            new("Healing Salve", "An herbal compound with antiseptic properties.",
                "The mixture smells of pine and something bitter. Application promotes tissue repair.",
                80, 20),
            new("Stamina Tonic", "A stimulant brewed from wasteland herbs.",
                "The acrid taste suggests potency. Energy returns quickly, though jitters may follow.",
                100, 18)
        },
        [QualityTier.ClanForged] = new List<ConsumableTemplate>
        {
            new("Dvergr Iron-Bread", "Dense waybread produced by master bakers.",
                "A single portion sustains for a full day. The complex flavor improves with each bite.",
                120, 35, new List<string> { "Ration" }),
            new("Dvergr Flask", "A metal-bound container of superior construction.",
                "The interior coating prevents contamination. Water stays fresh for extended periods.",
                300, 30, new List<string> { "Water" }),
            new("Wound-Knit Poultice", "A carefully prepared medical compound.",
                "Application produces a warm sensation as flesh mends. Scarring appears reduced.",
                60, 55),
            new("Berserker's Draft", "A battle stimulant of traditional recipe.",
                "Strength floods the limbs while fear recedes. Clarity suffers accordingly.",
                100, 45)
        },
        [QualityTier.Optimized] = new List<ConsumableTemplate>
        {
            new("Restoration Injector", "A pre-Glitch medical device with remaining charge.",
                "The mechanism hisses upon activation. Wounds close with unnatural speed.",
                50, 120),
            new("Cognitive Enhancer", "A neural stimulant in crystalline form.",
                "Dissolution under the tongue produces immediate clarity. Time perception may shift.",
                30, 95)
        },
        [QualityTier.MythForged] = new List<ConsumableTemplate>
        {
            new("Tears of the World-Tree", "A luminescent liquid of impossible origin.",
                "A single drop restores vitality thought lost. The taste evokes memories that aren't yours.",
                20, 350)
        }
    };

    #endregion

    #region Material Templates

    /// <summary>
    /// Crafting material templates by quality tier.
    /// </summary>
    public static readonly Dictionary<QualityTier, List<MaterialTemplate>> MaterialsByQuality = new()
    {
        [QualityTier.JuryRigged] = new List<MaterialTemplate>
        {
            new("Scrap Metal", "Fragments of rusted iron and unknown alloys.", 500, 5),
            new("Frayed Cloth", "Salvaged fabric in various states of decay.", 200, 3),
            new("Cracked Bone", "Skeletal remnants of indeterminate origin.", 300, 2)
        },
        [QualityTier.Scavenged] = new List<MaterialTemplate>
        {
            new("Iron Ingot", "A bar of reasonably pure iron.", 800, 20),
            new("Treated Leather", "Animal hide prepared for working.", 400, 15),
            new("Ironwood Plank", "Dense wood with metallic properties.", 600, 25)
        },
        [QualityTier.ClanForged] = new List<MaterialTemplate>
        {
            new("Dvergr Steel", "Refined metal of superior quality.", 600, 65),
            new("Rune-Touched Stone", "Rock bearing faint inscriptions.", 400, 50),
            new("Void-Silk Thread", "Fabric that absorbs light around it.", 100, 55)
        },
        [QualityTier.Optimized] = new List<MaterialTemplate>
        {
            new("Aesir Alloy Fragment", "Pre-Glitch metal of unknown composition.", 300, 150),
            new("Crystallized Aether", "Solidified energy from the old world.", 50, 180)
        },
        [QualityTier.MythForged] = new List<MaterialTemplate>
        {
            new("World-Tree Splinter", "A fragment of the mythical tree.", 100, 400),
            new("Glitch-Core Shard", "Reality fractures around this material.", 20, 500)
        }
    };

    #endregion

    #region Junk Templates

    /// <summary>
    /// Junk item templates (universal across quality, low value).
    /// </summary>
    public static readonly List<JunkTemplate> JunkItems = new()
    {
        new("Broken Mechanism", "Gears and springs from some unknown device.", 400, 8),
        new("Corroded Coins", "Currency from before the Glitch, now worthless for trade.", 150, 5),
        new("Shattered Glass", "Fragments of windows or containers.", 200, 3),
        new("Twisted Wire", "Metal filaments tangled beyond easy separation.", 300, 4),
        new("Crumbling Paper", "Documents too damaged to read, but recyclable.", 50, 2),
        new("Empty Containers", "Vessels that once held valuable contents.", 250, 6),
        new("Rusted Nails", "Fasteners reclaimed from collapsed structures.", 400, 3),
        new("Faded Rags", "Cloth scraps with no remaining utility.", 100, 2)
    };

    #endregion

    #region Biome Weightings

    /// <summary>
    /// Item type weights by biome (percentages).
    /// </summary>
    public static readonly Dictionary<BiomeType, Dictionary<ItemType, int>> ItemTypeByBiome = new()
    {
        [BiomeType.Ruin] = new Dictionary<ItemType, int>
        {
            { ItemType.Weapon, 20 },
            { ItemType.Armor, 15 },
            { ItemType.Consumable, 15 },
            { ItemType.Material, 25 },
            { ItemType.Junk, 25 }
        },
        [BiomeType.Industrial] = new Dictionary<ItemType, int>
        {
            { ItemType.Weapon, 15 },
            { ItemType.Armor, 20 },
            { ItemType.Consumable, 10 },
            { ItemType.Material, 35 },
            { ItemType.Junk, 20 }
        },
        [BiomeType.Organic] = new Dictionary<ItemType, int>
        {
            { ItemType.Weapon, 10 },
            { ItemType.Armor, 10 },
            { ItemType.Consumable, 40 },
            { ItemType.Material, 25 },
            { ItemType.Junk, 15 }
        },
        [BiomeType.Void] = new Dictionary<ItemType, int>
        {
            { ItemType.Weapon, 25 },
            { ItemType.Armor, 25 },
            { ItemType.Consumable, 10 },
            { ItemType.Material, 20 },
            { ItemType.Junk, 20 }
        }
    };

    #endregion
}

#region Template Records

/// <summary>
/// Template for weapon generation.
/// </summary>
public record WeaponTemplate(
    string Name,
    string Description,
    string DetailedDescription,
    EquipmentSlot Slot,
    int DamageDie,
    int Weight,
    int Value);

/// <summary>
/// Template for armor generation.
/// </summary>
public record ArmorTemplate(
    string Name,
    string Description,
    string DetailedDescription,
    EquipmentSlot Slot,
    int SoakBonus,
    int Weight,
    int Value);

/// <summary>
/// Template for consumable generation.
/// </summary>
/// <param name="Name">Display name of the consumable.</param>
/// <param name="Description">Short description for inventory lists.</param>
/// <param name="DetailedDescription">Detailed description when examining.</param>
/// <param name="Weight">Weight in grams.</param>
/// <param name="Value">Value in Scrip currency.</param>
/// <param name="Tags">Optional tags for categorization (e.g., "Ration", "Water").</param>
public record ConsumableTemplate(
    string Name,
    string Description,
    string DetailedDescription,
    int Weight,
    int Value,
    List<string>? Tags = null);

/// <summary>
/// Template for crafting material generation.
/// </summary>
public record MaterialTemplate(
    string Name,
    string Description,
    int Weight,
    int Value);

/// <summary>
/// Template for junk item generation.
/// </summary>
public record JunkTemplate(
    string Name,
    string Description,
    int Weight,
    int Value);

#endregion
