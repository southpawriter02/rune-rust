using RuneAndRust.Core;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23.3: Seeds the database with boss loot tables, artifacts, and set bonuses
/// </summary>
public class BossLootSeeder
{
    private static readonly ILogger _log = Log.ForContext<BossLootSeeder>();
    private readonly BossEncounterRepository _repository;

    public BossLootSeeder(BossEncounterRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Seed all boss loot configurations
    /// Call this after BossEncounterSeeder has populated boss encounters
    /// </summary>
    public void SeedBossLoot()
    {
        _log.Information("Seeding boss loot tables and artifacts");

        try
        {
            // Seed artifact sets
            SeedArtifactSets();

            // Seed loot tables for each boss
            SeedRuinWardenLoot();
            SeedAethericAberrationLoot();
            SeedForlornArchivistLoot();
            SeedOmegaSentinelLoot();

            _log.Information("Boss loot seeding completed successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to seed boss loot");
            throw;
        }
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // ARTIFACT SETS & SET BONUSES
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private void SeedArtifactSets()
    {
        _log.Debug("Seeding artifact sets and set bonuses");

        // Guardian's Aegis (Tank set)
        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Guardian's Aegis",
            PiecesRequired = 2,
            BonusName = "Fortified Stance",
            BonusDescription = "+2 Defense, +10 Max HP",
            BonusEffectScript = "defense_bonus:2,max_hp_bonus:10"
        });

        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Guardian's Aegis",
            PiecesRequired = 4,
            BonusName = "Unwavering Wall",
            BonusDescription = "+3 Soak, immune to knockback",
            BonusEffectScript = "soak_bonus:3,knockback_immune:true"
        });

        // Void-Touched Vestments (Mystic/Aether set)
        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Void-Touched Vestments",
            PiecesRequired = 2,
            BonusName = "Aetheric Resonance",
            BonusDescription = "+20 Max Aether, +1 Will",
            BonusEffectScript = "max_aether_bonus:20,will_bonus:1"
        });

        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Void-Touched Vestments",
            PiecesRequired = 4,
            BonusName = "Void Corruption",
            BonusDescription = "Aether abilities deal +25% damage, cost -1 Aether",
            BonusEffectScript = "aether_damage_mult:1.25,aether_cost_reduction:1"
        });

        // Shadow Reaver's Arsenal (DPS set)
        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Shadow Reaver's Arsenal",
            PiecesRequired = 2,
            BonusName = "Deadly Precision",
            BonusDescription = "+1 Finesse, +5% critical chance",
            BonusEffectScript = "finesse_bonus:1,crit_chance:0.05"
        });

        _repository.CreateSetBonus(new SetBonusData
        {
            SetName = "Shadow Reaver's Arsenal",
            PiecesRequired = 4,
            BonusName = "Reaving Strike",
            BonusDescription = "Critical hits restore 5 Stamina",
            BonusEffectScript = "crit_stamina_restore:5"
        });

        _log.Debug("Artifact sets seeded successfully");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // RUIN-WARDEN LOOT (TDR ~50, Sector Boss)
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private void SeedRuinWardenLoot()
    {
        _log.Debug("Seeding Ruin-Warden loot");

        int bossEncounterId = 1; // Ruin-Warden encounter ID

        // Create loot table
        var lootTableId = _repository.CreateBossLootTable(new BossLootTableData
        {
            BossEncounterId = bossEncounterId,
            GuaranteedDropCount = 2,
            MinimumQualityTier = "Clan-Forged",
            ClanForgedChance = 50,
            RuneCarvedChance = 40,
            ArtifactChance = 10,
            SilverMarksMin = 150,
            SilverMarksMax = 300,
            DropsUniqueItem = true,
            DropsCraftingMaterials = true,
            CraftingMaterialPool = JsonSerializer.Serialize(new List<CraftingMaterialDef>
            {
                new() { MaterialName = "Corrupted Servo", DropChance = 75, QuantityMin = 1, QuantityMax = 2 },
                new() { MaterialName = "Ancient Circuit", DropChance = 50, QuantityMin = 1, QuantityMax = 1 },
                new() { MaterialName = "Ruin Fragment", DropChance = 100, QuantityMin = 2, QuantityMax = 4 }
            })
        });

        // Create Guardian's Aegis set artifacts
        int aegisHelm = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Aegis Warden's Helm",
            ArtifactType = "Armor",
            Description = "A battle-worn helm etched with protective runes",
            FlavorText = "The Warden's gaze never falters.",
            SturdinessBonus = 2,
            DefenseBonus = 2,
            MaxHpBonus = 15,
            UniqueEffectName = "Runic Ward",
            UniqueEffectDescription = "Reduce incoming damage by 2 when HP > 50%",
            SetName = "Guardian's Aegis",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 50
        });

        int aegisGauntlets = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Gauntlets of the Eternal Sentinel",
            ArtifactType = "Armor",
            Description = "Heavy gauntlets that hum with protective energy",
            FlavorText = "Forged to defend the forgotten.",
            MightBonus = 1,
            SturdinessBonus = 2,
            SoakBonus = 2,
            UniqueEffectName = "Sentinel's Grip",
            UniqueEffectDescription = "Blocking grants +1 Defense for 2 turns",
            SetName = "Guardian's Aegis",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 50
        });

        // Unique item: Warden's Core Fragment (once per character)
        int wardenCore = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Warden's Core Fragment",
            ArtifactType = "Accessory",
            Description = "A fragment of the Ruin-Warden's power core",
            FlavorText = "Even broken, it pulses with ancient purpose.",
            SturdinessBonus = 1,
            WillBonus = 1,
            MaxHpBonus = 10,
            UniqueEffectName = "Enduring Protocol",
            UniqueEffectDescription = "Once per combat: When reduced to 0 HP, restore 25% HP instead",
            MinimumTdr = 50
        });

        _repository.CreateBossUniqueItem(new BossUniqueItemData
        {
            BossEncounterId = bossEncounterId,
            ArtifactId = wardenCore,
            DropChance = 100,
            DropCountMin = 1,
            DropCountMax = 1,
            DropsOncePerCharacter = true
        });

        _log.Debug("Ruin-Warden loot seeded successfully");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // AETHERIC ABERRATION LOOT (TDR ~55, Sector Boss)
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private void SeedAethericAberrationLoot()
    {
        _log.Debug("Seeding Aetheric Aberration loot");

        int bossEncounterId = 2; // Aetheric Aberration encounter ID

        // Create loot table
        _repository.CreateBossLootTable(new BossLootTableData
        {
            BossEncounterId = bossEncounterId,
            GuaranteedDropCount = 2,
            MinimumQualityTier = "Clan-Forged",
            ClanForgedChance = 45,
            RuneCarvedChance = 40,
            ArtifactChance = 15,
            SilverMarksMin = 200,
            SilverMarksMax = 350,
            DropsUniqueItem = true,
            DropsCraftingMaterials = true,
            CraftingMaterialPool = JsonSerializer.Serialize(new List<CraftingMaterialDef>
            {
                new() { MaterialName = "Void Crystal", DropChance = 80, QuantityMin = 1, QuantityMax = 2 },
                new() { MaterialName = "Aberrant Essence", DropChance = 60, QuantityMin = 1, QuantityMax = 1 },
                new() { MaterialName = "Reality Shard", DropChance = 100, QuantityMin = 2, QuantityMax = 3 }
            })
        });

        // Create Void-Touched Vestments set artifacts
        int voidRobe = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Void-Touched Robes",
            ArtifactType = "Armor",
            Description = "Robes that seem to absorb light itself",
            FlavorText = "The void whispers secrets best left unknown.",
            WillBonus = 2,
            WitsBonus = 1,
            MaxAetherBonus = 25,
            DefenseBonus = 1,
            UniqueEffectName = "Void Infusion",
            UniqueEffectDescription = "Spending Aether restores 2 HP",
            SetName = "Void-Touched Vestments",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 55
        });

        int voidFocus = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Aberrant Focus",
            ArtifactType = "Weapon",
            Description = "A crystalline focus pulsing with otherworldly energy",
            FlavorText = "Reality bends at your command.",
            WillBonus = 2,
            AccuracyBonus = 1,
            MaxAetherBonus = 15,
            UniqueEffectName = "Reality Tear",
            UniqueEffectDescription = "Aether attacks ignore 2 Defense",
            SetName = "Void-Touched Vestments",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 55
        });

        // Unique item: Aberration's Eye (once per character)
        int aberrationEye = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Aberration's Eye",
            ArtifactType = "Accessory",
            Description = "A crystallized fragment of the Aberration's essence",
            FlavorText = "It sees what should not be seen.",
            WitsBonus = 2,
            WillBonus = 1,
            MaxAetherBonus = 20,
            UniqueEffectName = "Void Sight",
            UniqueEffectDescription = "Reveal all hidden enemies. +10% damage vs Forlorn",
            MinimumTdr = 55
        });

        _repository.CreateBossUniqueItem(new BossUniqueItemData
        {
            BossEncounterId = bossEncounterId,
            ArtifactId = aberrationEye,
            DropChance = 100,
            DropCountMin = 1,
            DropCountMax = 1,
            DropsOncePerCharacter = true
        });

        _log.Debug("Aetheric Aberration loot seeded successfully");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // FORLORN ARCHIVIST LOOT (TDR ~60, Sector Boss)
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private void SeedForlornArchivistLoot()
    {
        _log.Debug("Seeding Forlorn Archivist loot");

        int bossEncounterId = 3; // Forlorn Archivist encounter ID

        // Create loot table
        _repository.CreateBossLootTable(new BossLootTableData
        {
            BossEncounterId = bossEncounterId,
            GuaranteedDropCount = 2,
            MinimumQualityTier = "Clan-Forged",
            ClanForgedChance = 40,
            RuneCarvedChance = 45,
            ArtifactChance = 15,
            SilverMarksMin = 250,
            SilverMarksMax = 400,
            DropsUniqueItem = true,
            DropsCraftingMaterials = true,
            CraftingMaterialPool = JsonSerializer.Serialize(new List<CraftingMaterialDef>
            {
                new() { MaterialName = "Psychic Residue", DropChance = 75, QuantityMin = 1, QuantityMax = 2 },
                new() { MaterialName = "Memory Fragment", DropChance = 65, QuantityMin = 1, QuantityMax = 1 },
                new() { MaterialName = "Lost Tome Page", DropChance = 100, QuantityMin = 2, QuantityMax = 3 }
            })
        });

        // Create Shadow Reaver's Arsenal set artifacts
        int reaverDaggers = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Twin Daggers of the Lost",
            ArtifactType = "Weapon",
            Description = "Blades that hunger for forgotten souls",
            FlavorText = "Every strike whispers a name long forgotten.",
            FinesseBonus = 3,
            WitsBonus = 1,
            AccuracyBonus = 1,
            UniqueEffectName = "Soul Reave",
            UniqueEffectDescription = "Critical hits heal 5 HP and grant +1 Finesse for 2 turns",
            SetName = "Shadow Reaver's Arsenal",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 60
        });

        int reaverCloak = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Cloak of Forgotten Shadows",
            ArtifactType = "Armor",
            Description = "A cloak that seems to absorb all light and sound",
            FlavorText = "You are but a memory waiting to fade.",
            FinesseBonus = 2,
            WitsBonus = 1,
            DefenseBonus = 2,
            UniqueEffectName = "Fade to Nothing",
            UniqueEffectDescription = "First attack each turn has +15% critical chance",
            SetName = "Shadow Reaver's Arsenal",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 60
        });

        // Unique item: Archivist's Forbidden Tome (once per character)
        int forbiddenTome = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Archivist's Forbidden Tome",
            ArtifactType = "Accessory",
            Description = "A leather-bound tome filled with maddening knowledge",
            FlavorText = "Some truths are meant to remain lost.",
            WitsBonus = 2,
            WillBonus = 2,
            MaxAetherBonus = 15,
            UniqueEffectName = "Forbidden Knowledge",
            UniqueEffectDescription = "Gain access to 2 random boss abilities (once per rest)",
            MinimumTdr = 60
        });

        _repository.CreateBossUniqueItem(new BossUniqueItemData
        {
            BossEncounterId = bossEncounterId,
            ArtifactId = forbiddenTome,
            DropChance = 100,
            DropCountMin = 1,
            DropCountMax = 1,
            DropsOncePerCharacter = true
        });

        _log.Debug("Forlorn Archivist loot seeded successfully");
    }

    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
    // OMEGA SENTINEL LOOT (TDR ~100, World Boss)
    // PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP

    private void SeedOmegaSentinelLoot()
    {
        _log.Debug("Seeding Omega Sentinel loot");

        int bossEncounterId = 4; // Omega Sentinel encounter ID

        // Create loot table (World Boss: better drop rates)
        _repository.CreateBossLootTable(new BossLootTableData
        {
            BossEncounterId = bossEncounterId,
            GuaranteedDropCount = 3,
            MinimumQualityTier = "Rune-Carved",
            ClanForgedChance = 0,
            RuneCarvedChance = 60,
            ArtifactChance = 40,
            SilverMarksMin = 500,
            SilverMarksMax = 1000,
            DropsUniqueItem = true,
            DropsCraftingMaterials = true,
            CraftingMaterialPool = JsonSerializer.Serialize(new List<CraftingMaterialDef>
            {
                new() { MaterialName = "Omega Core Fragment", DropChance = 100, QuantityMin = 1, QuantityMax = 1 },
                new() { MaterialName = "Titanium Plating", DropChance = 80, QuantityMin = 2, QuantityMax = 3 },
                new() { MaterialName = "Advanced Servomotor", DropChance = 75, QuantityMin = 1, QuantityMax = 2 },
                new() { MaterialName = "Hyperalloy Ingot", DropChance = 60, QuantityMin = 1, QuantityMax = 1 }
            })
        });

        // Complete Guardian's Aegis set pieces
        int aegisPlate = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Omega Guardian Plate",
            ArtifactType = "Armor",
            Description = "Impenetrable armor forged from the Sentinel's hull",
            FlavorText = "The last bulwark against oblivion.",
            SturdinessBonus = 3,
            MightBonus = 2,
            DefenseBonus = 3,
            SoakBonus = 3,
            MaxHpBonus = 30,
            UniqueEffectName = "Omega Protocol",
            UniqueEffectDescription = "Incoming damage cannot exceed 25% of max HP per hit",
            SetName = "Guardian's Aegis",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 100
        });

        int aegisShield = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Bulwark of the Omega",
            ArtifactType = "Weapon",
            Description = "A massive energy shield that projects a hardlight barrier",
            FlavorText = "None shall pass.",
            SturdinessBonus = 2,
            WillBonus = 1,
            DefenseBonus = 4,
            MaxHpBonus = 20,
            UniqueEffectName = "Hardlight Bastion",
            UniqueEffectDescription = "Blocking an attack reflects 5 damage to the attacker",
            SetName = "Guardian's Aegis",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 100
        });

        // Complete other set pieces (mix into other sets)
        int voidAmulet = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Void-Touched Amulet",
            ArtifactType = "Accessory",
            Description = "An amulet crackling with void energy",
            FlavorText = "Power at a price.",
            WillBonus = 3,
            WitsBonus = 1,
            MaxAetherBonus = 30,
            UniqueEffectName = "Void Empowerment",
            UniqueEffectDescription = "Aether abilities cost -2 Aether but deal -10% HP to self",
            SetName = "Void-Touched Vestments",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 100
        });

        int reaverBoots = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Boots of the Silent Reaver",
            ArtifactType = "Armor",
            Description = "Boots that make no sound, even on broken glass",
            FlavorText = "Death comes on silent feet.",
            FinesseBonus = 2,
            WitsBonus = 2,
            AccuracyBonus = 2,
            UniqueEffectName = "Assassin's Step",
            UniqueEffectDescription = "Moving does not cost stamina. +10% damage when attacking from behind",
            SetName = "Shadow Reaver's Arsenal",
            SetPieceCount = 4,
            DropsFromBossEncounterId = bossEncounterId,
            MinimumTdr = 100
        });

        // Legendary unique: Omega's Will (once per character, ultra-rare)
        int omegaWill = _repository.CreateArtifact(new ArtifactData
        {
            ArtifactName = "Omega's Will",
            ArtifactType = "Accessory",
            Description = "The crystallized will of the Omega Sentinel",
            FlavorText = "I am the shield. I am the sword. I am eternal.",
            MightBonus = 2,
            FinesseBonus = 1,
            WitsBonus = 1,
            WillBonus = 2,
            SturdinessBonus = 2,
            MaxHpBonus = 25,
            MaxStaminaBonus = 15,
            MaxAetherBonus = 15,
            UniqueEffectName = "Eternal Sentinel",
            UniqueEffectDescription = "Once per rest: Prevent fatal damage and become invulnerable for 1 turn",
            MinimumTdr = 100
        });

        _repository.CreateBossUniqueItem(new BossUniqueItemData
        {
            BossEncounterId = bossEncounterId,
            ArtifactId = omegaWill,
            DropChance = 100,
            DropCountMin = 1,
            DropCountMax = 1,
            DropsOncePerCharacter = true
        });

        _log.Debug("Omega Sentinel loot seeded successfully");
    }
}

/// <summary>
/// Helper class for JSON serialization of crafting materials
/// </summary>
internal class CraftingMaterialDef
{
    public string MaterialName { get; set; } = string.Empty;
    public int DropChance { get; set; }
    public int QuantityMin { get; set; }
    public int QuantityMax { get; set; }
}
