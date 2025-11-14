using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.3: Manages boss loot generation, artifact drops, and set bonuses
/// with database persistence
/// </summary>
public class BossLootService
{
    private static readonly ILogger _log = Log.ForContext<BossLootService>();
    private readonly BossEncounterRepository _repository;
    private readonly DiceService _diceService;

    public BossLootService(BossEncounterRepository repository, DiceService diceService)
    {
        _repository = repository;
        _diceService = diceService;
    }

    // ═══════════════════════════════════════════════════════════
    // BOSS LOOT GENERATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate loot from a boss encounter
    /// Returns formatted combat log message
    /// </summary>
    public LootGenerationResult GenerateBossLoot(int bossEncounterId, string characterId, int bossTdr)
    {
        _log.Information("Generating boss loot: BossEncounterId={BossEncounterId}, TDR={TDR}",
            bossEncounterId, bossTdr);

        var lootTable = _repository.GetBossLootTable(bossEncounterId);
        if (lootTable == null)
        {
            _log.Warning("No loot table found for boss encounter: {BossEncounterId}", bossEncounterId);
            return new LootGenerationResult
            {
                LogMessage = "⚠️ No loot configuration found.\\n",
                Items = new List<GeneratedItem>()
            };
        }

        var result = new LootGenerationResult();
        string logMessage = "\\n╔═══════════════════════════════════════════════════════════════╗\\n";
        logMessage += "║ 💰 BOSS LOOT GENERATION\\n";
        logMessage += "╚═══════════════════════════════════════════════════════════════╝\\n\\n";

        // Generate guaranteed quality drops
        var guaranteedItems = GenerateGuaranteedDrops(lootTable, bossTdr);
        result.Items.AddRange(guaranteedItems);

        foreach (var item in guaranteedItems)
        {
            logMessage += $"✨ [{item.QualityTier}] {item.ItemType} (Attributes: {FormatBonuses(item)})\\n";
        }

        // Roll for artifacts
        var artifact = RollForArtifact(lootTable, bossEncounterId, characterId, bossTdr);
        if (artifact != null)
        {
            result.Items.Add(artifact);
            logMessage += $"\\n🔥 [ARTIFACT] {artifact.ItemName}!\\n";
            logMessage += $"   {artifact.Description}\\n";

            if (!string.IsNullOrEmpty(artifact.UniqueEffectName))
            {
                logMessage += $"   ⚡ {artifact.UniqueEffectName}: {artifact.UniqueEffectDescription}\\n";
            }

            if (!string.IsNullOrEmpty(artifact.SetName))
            {
                logMessage += $"   📦 Part of {artifact.SetName} set ({artifact.SetPieceCount} pieces)\\n";
                logMessage += DisplaySetBonuses(artifact.SetName);
            }
        }

        // Generate unique items
        var uniqueItems = GenerateUniqueItems(bossEncounterId, characterId);
        result.Items.AddRange(uniqueItems);

        foreach (var item in uniqueItems)
        {
            logMessage += $"\\n💎 [UNIQUE] {item.ItemName}!\\n";
            logMessage += $"   {item.Description}\\n";
        }

        // Generate currency rewards
        int silverMarks = _diceService.RollBetween(lootTable.SilverMarksMin, lootTable.SilverMarksMax);
        result.SilverMarks = silverMarks;
        logMessage += $"\\n💰 {silverMarks} Silver Marks\\n";

        // Generate crafting materials
        if (lootTable.DropsCraftingMaterials && !string.IsNullOrEmpty(lootTable.CraftingMaterialPool))
        {
            var craftingMaterials = GenerateCraftingMaterials(lootTable.CraftingMaterialPool, bossTdr);
            result.CraftingMaterials.AddRange(craftingMaterials);

            foreach (var material in craftingMaterials)
            {
                logMessage += $"🔧 {material.Count}x {material.MaterialName}\\n";
            }
        }

        _log.Information("Loot generated: Items={ItemCount}, Artifacts={ArtifactCount}, SilverMarks={Silver}",
            result.Items.Count, result.Items.Count(i => i.IsArtifact), silverMarks);

        result.LogMessage = logMessage;
        return result;
    }

    // ═══════════════════════════════════════════════════════════
    // GUARANTEED DROPS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate guaranteed quality drops (Clan-Forged/Rune-Carved/Artifact)
    /// </summary>
    private List<GeneratedItem> GenerateGuaranteedDrops(BossLootTableData lootTable, int bossTdr)
    {
        var items = new List<GeneratedItem>();

        for (int i = 0; i < lootTable.GuaranteedDropCount; i++)
        {
            string qualityTier = RollQualityTier(lootTable);
            var item = GenerateItemByQuality(qualityTier, bossTdr);
            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// Roll for quality tier based on loot table percentages
    /// </summary>
    private string RollQualityTier(BossLootTableData lootTable)
    {
        int roll = _diceService.RollD100();

        // Cumulative probability
        if (roll <= lootTable.ArtifactChance)
        {
            return "Artifact";
        }
        else if (roll <= lootTable.ArtifactChance + lootTable.RuneCarvedChance)
        {
            return "Rune-Carved";
        }
        else
        {
            return "Clan-Forged";
        }
    }

    /// <summary>
    /// Generate item based on quality tier
    /// </summary>
    private GeneratedItem GenerateItemByQuality(string qualityTier, int bossTdr)
    {
        // Determine item type based on TDR
        string[] itemTypes = { "Weapon", "Armor", "Accessory" };
        string itemType = itemTypes[_diceService.RollBetween(0, itemTypes.Length - 1)];

        var item = new GeneratedItem
        {
            ItemName = $"{qualityTier} {itemType}",
            ItemType = itemType,
            QualityTier = qualityTier,
            IsArtifact = false,
            Description = $"A {qualityTier.ToLower()} quality {itemType.ToLower()}."
        };

        // Assign bonuses based on quality
        int bonusAmount = qualityTier switch
        {
            "Artifact" => _diceService.RollBetween(3, 5),
            "Rune-Carved" => _diceService.RollBetween(2, 3),
            "Clan-Forged" => _diceService.RollBetween(1, 2),
            _ => 1
        };

        // Randomly assign bonuses to attributes
        AssignRandomBonuses(item, bonusAmount);

        return item;
    }

    /// <summary>
    /// Assign random attribute bonuses to item
    /// </summary>
    private void AssignRandomBonuses(GeneratedItem item, int totalBonus)
    {
        // Distribute bonuses across attributes
        for (int i = 0; i < totalBonus; i++)
        {
            int attributeRoll = _diceService.RollD6();

            switch (attributeRoll)
            {
                case 1: item.MightBonus++; break;
                case 2: item.FinesseBonus++; break;
                case 3: item.WitsBonus++; break;
                case 4: item.WillBonus++; break;
                case 5: item.SturdinessBonus++; break;
                case 6: item.DefenseBonus++; break;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ARTIFACT DROPS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Roll for artifact drop based on loot table and TDR
    /// Returns null if no artifact drops
    /// </summary>
    private GeneratedItem? RollForArtifact(BossLootTableData lootTable, int bossEncounterId,
        string characterId, int bossTdr)
    {
        int artifactRoll = _diceService.RollD100();

        // Artifact drop chance (10-20% based on boss tier)
        int artifactChance = bossTdr switch
        {
            >= 80 => 20, // World Boss: 20%
            >= 40 => 15, // Sector Boss: 15%
            >= 20 => 10, // Elite: 10%
            _ => 5       // Default: 5%
        };

        if (artifactRoll > artifactChance)
        {
            _log.Debug("Artifact roll failed: Roll={Roll}, Chance={Chance}", artifactRoll, artifactChance);
            return null;
        }

        // Get available artifacts by TDR
        var availableArtifacts = _repository.GetArtifactsByTDR(bossTdr);

        if (!availableArtifacts.Any())
        {
            _log.Warning("No artifacts available for TDR={TDR}", bossTdr);
            return null;
        }

        // Randomly select artifact
        int artifactIndex = _diceService.RollBetween(0, availableArtifacts.Count - 1);
        var artifactData = availableArtifacts[artifactIndex];

        // Convert ArtifactData to GeneratedItem
        var artifact = new GeneratedItem
        {
            ArtifactId = artifactData.ArtifactId,
            ItemName = artifactData.ArtifactName,
            ItemType = artifactData.ArtifactType,
            QualityTier = "Artifact",
            IsArtifact = true,
            Description = artifactData.Description ?? string.Empty,
            FlavorText = artifactData.FlavorText,
            MightBonus = artifactData.MightBonus,
            FinesseBonus = artifactData.FinesseBonus,
            WitsBonus = artifactData.WitsBonus,
            WillBonus = artifactData.WillBonus,
            SturdinessBonus = artifactData.SturdinessBonus,
            MaxHpBonus = artifactData.MaxHpBonus,
            MaxStaminaBonus = artifactData.MaxStaminaBonus,
            MaxAetherBonus = artifactData.MaxAetherBonus,
            DefenseBonus = artifactData.DefenseBonus,
            SoakBonus = artifactData.SoakBonus,
            AccuracyBonus = artifactData.AccuracyBonus,
            UniqueEffectName = artifactData.UniqueEffectName,
            UniqueEffectDescription = artifactData.UniqueEffectDescription,
            SetName = artifactData.SetName,
            SetPieceCount = artifactData.SetPieceCount
        };

        _log.Information("Artifact dropped: Name={ArtifactName}, TDR={TDR}",
            artifactData.ArtifactName, bossTdr);

        return artifact;
    }

    // ═══════════════════════════════════════════════════════════
    // UNIQUE ITEM DROPS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate unique items from boss (once-per-character tracking)
    /// </summary>
    private List<GeneratedItem> GenerateUniqueItems(int bossEncounterId, string characterId)
    {
        var items = new List<GeneratedItem>();
        var uniqueItemConfigs = _repository.GetBossUniqueItems(bossEncounterId);

        foreach (var config in uniqueItemConfigs)
        {
            // Check once-per-character restriction
            if (config.DropsOncePerCharacter)
            {
                if (_repository.HasReceivedUniqueItem(characterId, config.ArtifactId))
                {
                    _log.Debug("Character already received unique item: CharacterId={CharacterId}, ArtifactId={ArtifactId}",
                        characterId, config.ArtifactId);
                    continue;
                }
            }

            // Roll for drop
            int dropRoll = _diceService.RollD100();
            if (dropRoll > config.DropChance)
            {
                continue;
            }

            // Get artifact data
            var artifactData = _repository.GetArtifact(config.ArtifactId);
            if (artifactData == null)
            {
                _log.Warning("Artifact not found: ArtifactId={ArtifactId}", config.ArtifactId);
                continue;
            }

            // Determine drop count
            int dropCount = _diceService.RollBetween(config.DropCountMin, config.DropCountMax);

            for (int i = 0; i < dropCount; i++)
            {
                var item = new GeneratedItem
                {
                    ArtifactId = artifactData.ArtifactId,
                    ItemName = artifactData.ArtifactName,
                    ItemType = artifactData.ArtifactType,
                    QualityTier = "Unique",
                    IsArtifact = true,
                    IsUnique = true,
                    Description = artifactData.Description ?? string.Empty,
                    FlavorText = artifactData.FlavorText,
                    MightBonus = artifactData.MightBonus,
                    FinesseBonus = artifactData.FinesseBonus,
                    WitsBonus = artifactData.WitsBonus,
                    WillBonus = artifactData.WillBonus,
                    SturdinessBonus = artifactData.SturdinessBonus,
                    MaxHpBonus = artifactData.MaxHpBonus,
                    MaxStaminaBonus = artifactData.MaxStaminaBonus,
                    MaxAetherBonus = artifactData.MaxAetherBonus,
                    DefenseBonus = artifactData.DefenseBonus,
                    SoakBonus = artifactData.SoakBonus,
                    AccuracyBonus = artifactData.AccuracyBonus,
                    UniqueEffectName = artifactData.UniqueEffectName,
                    UniqueEffectDescription = artifactData.UniqueEffectDescription
                };

                items.Add(item);
            }

            // Record unique item drop if once-per-character
            if (config.DropsOncePerCharacter)
            {
                _repository.RecordUniqueItemDrop(characterId, config.ArtifactId);
                _log.Information("Unique item dropped and recorded: CharacterId={CharacterId}, ArtifactId={ArtifactId}",
                    characterId, config.ArtifactId);
            }
        }

        return items;
    }

    // ═══════════════════════════════════════════════════════════
    // CRAFTING MATERIALS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Generate crafting materials from pool
    /// </summary>
    private List<CraftingMaterial> GenerateCraftingMaterials(string craftingMaterialPoolJson, int bossTdr)
    {
        var materials = new List<CraftingMaterial>();

        try
        {
            var pool = JsonSerializer.Deserialize<List<CraftingMaterialDefinition>>(craftingMaterialPoolJson);
            if (pool == null) return materials;

            foreach (var definition in pool)
            {
                int dropRoll = _diceService.RollD100();
                if (dropRoll <= definition.DropChance)
                {
                    int count = _diceService.RollBetween(definition.QuantityMin, definition.QuantityMax);
                    materials.Add(new CraftingMaterial
                    {
                        MaterialName = definition.MaterialName,
                        Count = count
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to parse crafting material pool: {Json}", craftingMaterialPoolJson);
        }

        return materials;
    }

    // ═══════════════════════════════════════════════════════════
    // SET BONUS SYSTEM
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Display set bonuses for a given set
    /// </summary>
    private string DisplaySetBonuses(string setName)
    {
        var setBonuses = _repository.GetSetBonuses(setName);
        if (!setBonuses.Any()) return string.Empty;

        string message = "   Set Bonuses:\\n";
        foreach (var bonus in setBonuses.OrderBy(b => b.PiecesRequired))
        {
            message += $"      ({bonus.PiecesRequired}): {bonus.BonusName} - {bonus.BonusDescription}\\n";
        }

        return message;
    }

    // ═══════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Format item bonuses for display
    /// </summary>
    private string FormatBonuses(GeneratedItem item)
    {
        var bonuses = new List<string>();

        if (item.MightBonus > 0) bonuses.Add($"+{item.MightBonus} Might");
        if (item.FinesseBonus > 0) bonuses.Add($"+{item.FinesseBonus} Finesse");
        if (item.WitsBonus > 0) bonuses.Add($"+{item.WitsBonus} Wits");
        if (item.WillBonus > 0) bonuses.Add($"+{item.WillBonus} Will");
        if (item.SturdinessBonus > 0) bonuses.Add($"+{item.SturdinessBonus} Sturdiness");
        if (item.DefenseBonus > 0) bonuses.Add($"+{item.DefenseBonus} Defense");
        if (item.SoakBonus > 0) bonuses.Add($"+{item.SoakBonus} Soak");
        if (item.MaxHpBonus > 0) bonuses.Add($"+{item.MaxHpBonus} HP");
        if (item.MaxStaminaBonus > 0) bonuses.Add($"+{item.MaxStaminaBonus} Stamina");
        if (item.MaxAetherBonus > 0) bonuses.Add($"+{item.MaxAetherBonus} Aether");

        return bonuses.Any() ? string.Join(", ", bonuses) : "None";
    }
}

// ═══════════════════════════════════════════════════════════
// RESULT MODELS
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Loot generation result
/// </summary>
public class LootGenerationResult
{
    public string LogMessage { get; set; } = string.Empty;
    public List<GeneratedItem> Items { get; set; } = new();
    public int SilverMarks { get; set; }
    public List<CraftingMaterial> CraftingMaterials { get; set; } = new();
}

/// <summary>
/// Generated item from loot
/// </summary>
public class GeneratedItem
{
    public int? ArtifactId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string QualityTier { get; set; } = string.Empty;
    public bool IsArtifact { get; set; }
    public bool IsUnique { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? FlavorText { get; set; }

    // Attribute bonuses
    public int MightBonus { get; set; }
    public int FinesseBonus { get; set; }
    public int WitsBonus { get; set; }
    public int WillBonus { get; set; }
    public int SturdinessBonus { get; set; }

    // Combat bonuses
    public int MaxHpBonus { get; set; }
    public int MaxStaminaBonus { get; set; }
    public int MaxAetherBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int SoakBonus { get; set; }
    public int AccuracyBonus { get; set; }

    // Unique properties
    public string? UniqueEffectName { get; set; }
    public string? UniqueEffectDescription { get; set; }

    // Set membership
    public string? SetName { get; set; }
    public int? SetPieceCount { get; set; }
}

/// <summary>
/// Crafting material drop
/// </summary>
public class CraftingMaterial
{
    public string MaterialName { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Crafting material definition for JSON deserialization
/// </summary>
public class CraftingMaterialDefinition
{
    public string MaterialName { get; set; } = string.Empty;
    public int DropChance { get; set; } = 100;
    public int QuantityMin { get; set; } = 1;
    public int QuantityMax { get; set; } = 1;
}
