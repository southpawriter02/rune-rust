using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.3: Handles boss loot generation with guaranteed quality tiers,
/// legendary drops, and artifact system
/// </summary>
public class BossLootService
{
    private static readonly ILogger _log = Log.ForContext<BossLootService>();
    private readonly Random _random;
    private readonly EquipmentDatabase _equipmentDatabase;

    public BossLootService(EquipmentDatabase equipmentDatabase)
    {
        _random = new Random();
        _equipmentDatabase = equipmentDatabase;
    }

    /// <summary>
    /// Generate loot from a defeated boss using boss loot table
    /// Returns list of equipment and currency amount
    /// </summary>
    public (List<Equipment> equipment, int currency, Dictionary<ComponentType, int> materials) GenerateBossLoot(
        Enemy boss,
        BossLootTable lootTable,
        PlayerCharacter? player = null,
        int threatDifficultyRating = 0)
    {
        if (!boss.IsBoss)
        {
            _log.Warning("Attempted to generate boss loot for non-boss enemy: {EnemyId}", boss.Id);
            return (new List<Equipment>(), 0, new Dictionary<ComponentType, int>());
        }

        var equipment = new List<Equipment>();
        var materials = new Dictionary<ComponentType, int>();

        // Scale artifact chance based on TDR
        int artifactChance = (int)(lootTable.ArtifactChance * (1.0 + (threatDifficultyRating * lootTable.TDRScalingFactor / 10.0)));
        int legendaryChance = (int)(lootTable.LegendaryChance * (1.0 + (threatDifficultyRating * lootTable.TDRScalingFactor / 10.0)));

        _log.Information("Generating boss loot: {BossType}, TDR={TDR}, ArtifactChance={Artifact}%, LegendaryChance={Legendary}%",
            boss.Type, threatDifficultyRating, artifactChance, legendaryChance);

        // Roll for artifact first (rarest)
        if (_random.Next(100) < artifactChance)
        {
            var artifact = RollArtifact(lootTable, threatDifficultyRating, player);
            if (artifact != null)
            {
                equipment.Add(artifact);
                _log.Information("🌟 ARTIFACT DROPPED: {BossType}, Item={ItemName}",
                    boss.Type, artifact.Name);
            }
        }
        // Roll for legendary (if no artifact)
        else if (_random.Next(100) < legendaryChance)
        {
            var legendary = GenerateLegendaryItem(player);
            equipment.Add(legendary);
            _log.Information("⚡ LEGENDARY DROPPED: {BossType}, Item={ItemName}, Quality={Quality}",
                boss.Type, legendary.Name, legendary.Quality);
        }
        // Roll for optimized
        else if (_random.Next(100) < lootTable.OptimizedChance)
        {
            var optimized = GenerateOptimizedItem(player);
            equipment.Add(optimized);
            _log.Information("Boss loot: {BossType}, Item={ItemName}, Quality={Quality}",
                boss.Type, optimized.Name, optimized.Quality);
        }
        // Guaranteed quality fallback
        else
        {
            var guaranteed = GenerateGuaranteedItem(lootTable.GuaranteedQuality, player);
            equipment.Add(guaranteed);
            _log.Information("Boss loot (guaranteed): {BossType}, Item={ItemName}, Quality={Quality}",
                boss.Type, guaranteed.Name, guaranteed.Quality);
        }

        // Generate currency
        int currency = _random.Next(lootTable.CurrencyDrop.Min, lootTable.CurrencyDrop.Max + 1);
        _log.Information("Currency dropped: {BossType}, Amount={Amount} Cogs", boss.Type, currency);

        // Generate materials
        materials = GenerateBossMaterials(lootTable);

        return (equipment, currency, materials);
    }

    /// <summary>
    /// Roll for an artifact from the boss's artifact pool
    /// </summary>
    private Equipment? RollArtifact(BossLootTable lootTable, int tdr, PlayerCharacter? player)
    {
        // Filter artifacts by TDR requirement
        var eligibleArtifacts = lootTable.UniqueArtifacts
            .Where(a => a.MinimumTDR <= tdr)
            .ToList();

        if (eligibleArtifacts.Count == 0)
        {
            _log.Debug("No eligible artifacts for TDR={TDR}", tdr);
            return null;
        }

        // Weighted random selection
        int totalWeight = eligibleArtifacts.Sum(a => a.DropWeight);
        int roll = _random.Next(totalWeight);
        int currentWeight = 0;

        foreach (var artifact in eligibleArtifacts)
        {
            currentWeight += artifact.DropWeight;
            if (roll < currentWeight)
            {
                return ConvertArtifactToEquipment(artifact);
            }
        }

        return null;
    }

    /// <summary>
    /// Convert UniqueArtifact definition to Equipment item
    /// </summary>
    private Equipment ConvertArtifactToEquipment(UniqueArtifact artifact)
    {
        var equipment = new Equipment
        {
            Name = artifact.Name,
            Description = artifact.Description,
            Quality = QualityTier.MythForged, // Artifacts are always Myth-Forged
            Bonuses = new List<EquipmentBonus>(artifact.Bonuses),
            SpecialEffect = artifact.SpecialEffect
        };

        switch (artifact.Type)
        {
            case ArtifactType.Weapon:
                equipment.Type = EquipmentType.Weapon;
                equipment.WeaponCategory = artifact.WeaponCategory;
                equipment.DamageDice = artifact.DamageDice;
                equipment.DamageBonus = artifact.DamageBonus;
                equipment.WeaponAttribute = GetWeaponAttribute(artifact.WeaponCategory);
                equipment.StaminaCost = 5;
                break;

            case ArtifactType.Armor:
                equipment.Type = EquipmentType.Armor;
                equipment.HPBonus = artifact.HPBonus;
                equipment.DefenseBonus = artifact.DefenseBonus;
                break;

            case ArtifactType.Accessory:
                equipment.Type = EquipmentType.Accessory;
                break;

            case ArtifactType.CraftingMaterial:
                // Crafting materials are handled separately
                _log.Warning("Artifact of type CraftingMaterial cannot be converted to Equipment: {ArtifactName}",
                    artifact.Name);
                break;
        }

        _log.Information("Artifact generated: {Name}, Type={Type}, Special={Special}",
            artifact.Name, artifact.Type, artifact.SpecialEffect);

        return equipment;
    }

    /// <summary>
    /// Generate a random legendary (Myth-Forged) item for player's class
    /// </summary>
    private Equipment GenerateLegendaryItem(PlayerCharacter? player)
    {
        var characterClass = player?.CharacterClass ?? CharacterClass.Warrior;

        // Generate Myth-Forged quality item
        var weapon = GenerateWeaponForClass(characterClass, QualityTier.MythForged);

        // Add special legendary effect
        weapon.SpecialEffect = GetRandomLegendaryEffect();

        return weapon;
    }

    /// <summary>
    /// Generate an Optimized quality item
    /// </summary>
    private Equipment GenerateOptimizedItem(PlayerCharacter? player)
    {
        var characterClass = player?.CharacterClass ?? CharacterClass.Warrior;
        return GenerateWeaponForClass(characterClass, QualityTier.Optimized);
    }

    /// <summary>
    /// Generate guaranteed quality item
    /// </summary>
    private Equipment GenerateGuaranteedItem(QualityTier quality, PlayerCharacter? player)
    {
        var characterClass = player?.CharacterClass ?? CharacterClass.Warrior;
        return GenerateWeaponForClass(characterClass, quality);
    }

    /// <summary>
    /// Generate materials from boss loot table
    /// </summary>
    private Dictionary<ComponentType, int> GenerateBossMaterials(BossLootTable lootTable)
    {
        var materials = new Dictionary<ComponentType, int>();

        // Add guaranteed materials
        foreach (var material in lootTable.GuaranteedMaterials)
        {
            materials[material.Key] = material.Value;
        }

        // Roll for rare materials
        foreach (var material in lootTable.RareMaterialChances)
        {
            if (_random.Next(100) < material.Value)
            {
                materials[material.Key] = materials.ContainsKey(material.Key)
                    ? materials[material.Key] + 1
                    : 1;

                _log.Debug("Rare material dropped: {Material}", material.Key);
            }
        }

        // Roll for epic materials
        foreach (var material in lootTable.EpicMaterialChances)
        {
            if (_random.Next(100) < material.Value)
            {
                materials[material.Key] = materials.ContainsKey(material.Key)
                    ? materials[material.Key] + 1
                    : 1;

                _log.Debug("Epic material dropped: {Material}", material.Key);
            }
        }

        return materials;
    }

    /// <summary>
    /// Generate weapon for specific character class
    /// </summary>
    private Equipment GenerateWeaponForClass(CharacterClass characterClass, QualityTier quality)
    {
        var weaponCategory = characterClass switch
        {
            CharacterClass.Warrior => _random.Next(2) == 0 ? WeaponCategory.Axe : WeaponCategory.Greatsword,
            CharacterClass.Scavenger => _random.Next(2) == 0 ? WeaponCategory.Spear : WeaponCategory.Dagger,
            CharacterClass.Mystic => _random.Next(2) == 0 ? WeaponCategory.Staff : WeaponCategory.Focus,
            _ => WeaponCategory.Axe
        };

        var weapon = new Equipment
        {
            Name = GenerateWeaponName(weaponCategory, quality),
            Description = $"A {quality} quality weapon recovered from a fallen foe.",
            Type = EquipmentType.Weapon,
            Quality = quality,
            WeaponCategory = weaponCategory,
            WeaponAttribute = GetWeaponAttribute(weaponCategory),
            DamageDice = GetDamageDiceForQuality(quality),
            DamageBonus = GetDamageBonusForQuality(quality),
            AccuracyBonus = quality >= QualityTier.Optimized ? 1 : 0,
            StaminaCost = 5
        };

        // Add bonuses for higher quality items
        if (quality >= QualityTier.ClanForged)
        {
            weapon.Bonuses.Add(new EquipmentBonus
            {
                AttributeName = GetWeaponAttribute(weaponCategory),
                BonusValue = 1,
                Description = $"+1 {GetWeaponAttribute(weaponCategory)}"
            });
        }

        return weapon;
    }

    /// <summary>
    /// Generate weapon name based on category and quality
    /// </summary>
    private string GenerateWeaponName(WeaponCategory? category, QualityTier quality)
    {
        string[] prefixes = quality switch
        {
            QualityTier.MythForged => new[] { "Legendary", "Ancient", "Mythical", "Corrupted" },
            QualityTier.Optimized => new[] { "Refined", "Enhanced", "Superior" },
            QualityTier.ClanForged => new[] { "Forged", "Crafted", "Tempered" },
            _ => new[] { "Scavenged", "Salvaged" }
        };

        string prefix = prefixes[_random.Next(prefixes.Length)];

        string weaponType = category switch
        {
            WeaponCategory.Axe => "Axe",
            WeaponCategory.Greatsword => "Greatsword",
            WeaponCategory.Spear => "Spear",
            WeaponCategory.Dagger => "Dagger",
            WeaponCategory.Staff => "Staff",
            WeaponCategory.Focus => "Focus",
            _ => "Weapon"
        };

        return $"{prefix} {weaponType}";
    }

    /// <summary>
    /// Get damage dice based on quality tier
    /// </summary>
    private int GetDamageDiceForQuality(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.MythForged => 3,
            QualityTier.Optimized => 2,
            QualityTier.ClanForged => 2,
            QualityTier.Scavenged => 1,
            QualityTier.JuryRigged => 1,
            _ => 1
        };
    }

    /// <summary>
    /// Get damage bonus based on quality tier
    /// </summary>
    private int GetDamageBonusForQuality(QualityTier quality)
    {
        return quality switch
        {
            QualityTier.MythForged => 3,
            QualityTier.Optimized => 2,
            QualityTier.ClanForged => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Get weapon attribute based on weapon category
    /// </summary>
    private string GetWeaponAttribute(WeaponCategory? category)
    {
        return category switch
        {
            WeaponCategory.Axe => "MIGHT",
            WeaponCategory.Greatsword => "MIGHT",
            WeaponCategory.Spear => "FINESSE",
            WeaponCategory.Dagger => "FINESSE",
            WeaponCategory.Staff => "WILL",
            WeaponCategory.Focus => "WILL",
            WeaponCategory.Blade => "FINESSE",
            WeaponCategory.Blunt => "MIGHT",
            WeaponCategory.EnergyMelee => "FINESSE",
            WeaponCategory.Rifle => "FINESSE",
            WeaponCategory.HeavyBlunt => "MIGHT",
            _ => "MIGHT"
        };
    }

    /// <summary>
    /// Get random legendary effect for Myth-Forged items
    /// </summary>
    private string GetRandomLegendaryEffect()
    {
        string[] effects = new[]
        {
            "Inflicts [Bleeding] on critical hits (3 turns)",
            "Ignores 50% of enemy armor",
            "+2 damage per enemy defeated this combat",
            "Regenerate 5 HP per kill",
            "Critical hits restore 10 Stamina",
            "+3 damage when below 50% HP",
            "Attacks have 20% chance to stun (1 turn)",
            "Gain +2 MIGHT when wielding this weapon",
            "Deal +50% damage to Forlorn enemies"
        };

        return effects[_random.Next(effects.Length)];
    }
}
